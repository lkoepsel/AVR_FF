#!/usr/bin/env python3
"""
fu: Forth Upload -
Upload a Forth source file to a Forth-based ATmega328p board via serial port.
Sends each line and waits for the 'ok' prompt before continuing.
Port defaults to /dev/ttyUSB0 at 250000 baud (matches 'tio forth' profile).

Current upload program in use. Tested and developed with Claude.
"""

import time

import click
import serial
import serial.tools.list_ports

__version__ = "1.0.0"

FORTH_PORT = "/dev/ttyUSB0"
FORTH_BAUD = 250000
_OK_TIMEOUT = 5.0
_LINE_DELAY_MS = 50  # ms between lines inside a colon definition

# Context-switching words that change the dictionary and prompt format.
# Each entry maps the exact line content to its expected echo response.
_CONTEXT_WORDS = {
    "empty": b"empty  ok<#,ram> \r\n",
    "flash": b"flash  ok<#,flash> \r\n",
    "eeprom": b"eeprom  ok<#,eeprom> \r\n",
    "ram": b"ram  ok<#,ram> \r\n",
}


def is_blank_or_comment(line: str) -> bool:
    """Return True for empty lines and lines whose only content is a \\ comment."""
    stripped = line.strip()
    return not stripped or stripped.startswith("\\")


def _strip_inline_comment(line: str) -> str:
    """Remove trailing \\ comment from a line, preserving leading content."""
    return line.split("\\")[0].rstrip()


def _opens_definition(line: str) -> bool:
    """True when the first token starts a colon definition."""
    tokens = line.split()
    return bool(tokens) and tokens[0] in (":", ":noname")


def _closes_definition(line: str) -> bool:
    """True when the line contains the ; that ends a colon definition."""
    return ";" in line.split()


def _wait_for_ok(ser, timeout: float) -> tuple[bool, str]:
    """
    Read from ser until a response line contains an 'ok*' token (e.g. 'ok',
    'ok<#,ram>', 'ok<#,flash>') or the response signals a Forth error.
    Returns (success, response_text).
    """
    start = time.monotonic()
    buf = b""
    while time.monotonic() - start < timeout:
        if ser.in_waiting:
            buf += ser.read(ser.in_waiting)
        else:
            chunk = ser.read(1)
            if chunk:
                buf += chunk
        # NAK byte (0x15) is appended by FlashForth after an error response
        if b"\x15" in buf:
            return False, buf.decode("ascii", errors="replace")
        text = buf.decode("ascii", errors="replace")
        for resp_line in text.splitlines():
            stripped = resp_line.strip()
            tokens = stripped.split()
            if any(t.startswith("ok") for t in tokens):
                return True, text
            if (
                stripped.endswith("?")
                or "DEFINED" in stripped
                or "COMPILE ONLY" in stripped
            ):
                return False, text
    return False, buf.decode("ascii", errors="replace")


def _wait_for_exact(ser, expected: bytes, timeout: float = 10.0) -> bool:
    """
    Read from ser until the accumulated bytes contain `expected`.
    Used for slow context-switching words (empty, flash, ram, eeprom)
    that need their specific echo confirmed before upload continues.
    Returns True on match, False on timeout.
    """
    start = time.monotonic()
    buf = b""
    while time.monotonic() - start < timeout:
        if ser.in_waiting:
            buf += ser.read(ser.in_waiting)
        else:
            chunk = ser.read(1)
            if chunk:
                buf += chunk
        if expected in buf:
            return True
    return False


def _detect_port() -> str | None:
    """Return the first USB serial port found, or None."""
    for p in sorted(serial.tools.list_ports.comports()):
        dev = p.device
        if any(
            tag in dev for tag in ("usbserial", "usbmodem", "ttyUSB", "ttyACM", "COM")
        ):
            return dev
    return None


@click.command()
@click.version_option(__version__, prog_name="fu")
@click.argument("file", type=click.Path(exists=True, dir_okay=False))
@click.option(
    "--port",
    "-p",
    default=None,
    show_default=True,
    help=f"Serial port device (auto-detected if omitted, fallback {FORTH_PORT})",
)
@click.option(
    "--baud",
    "-b",
    default=FORTH_BAUD,
    show_default=True,
    type=int,
    help="Baud rate",
)
@click.option(
    "--timeout",
    "-t",
    default=_OK_TIMEOUT,
    show_default=True,
    type=float,
    help="Per-line timeout in seconds waiting for 'ok'",
)
@click.option(
    "--delay",
    "-d",
    default=_LINE_DELAY_MS,
    show_default=True,
    type=int,
    help="Delay in ms between lines inside a colon definition (prevents ring-buffer overflow)",
)
@click.option(
    "--pipe",
    is_flag=True,
    help="Write lines to stdout instead of a serial port (for tio ctrl-t R)",
)
@click.option(
    "--clean",
    "-c",
    is_flag=True,
    help="Print the comment-stripped file and exit without uploading",
)
@click.option("--verbose", "-v", is_flag=True, help="Print each line as it is sent")
def fu(file, port, baud, timeout, delay, pipe, clean, verbose):
    """
    fu: Forth Upload
    Upload a Forth source file to a serial Forth board.

    Skips blank lines and comment-only lines (lines starting with \\).
    Strips trailing inline comments before sending.
    Inside a colon definition, each line is sent with a configurable delay
    so the board's serial ring buffer does not overflow.  After the closing
    ';' (and after any top-level word), waits for the Forth 'ok' prompt
    before continuing.  Handles FlashForth's extended prompt (ok<#,ram>).
    Context-switching words (empty, flash, ram, eeprom) are confirmed by
    their exact echo response before upload continues.

    Use --pipe to write lines to stdout instead of opening a serial port.
    This lets tio send the file via its built-in ctrl-t R shell command:
    press ctrl-t R inside tio, then enter: fu --pipe blink.fs

    Use --clean to preview exactly what will be sent (comments stripped)
    without opening the serial port.

    Examples:
      fu blink.fs
      fu --port /dev/ttyACM0 blink.fs
      fu --delay 100 --verbose myapp.fth
      fu --pipe blink.fs        (run via tio ctrl-t R)
      fu --clean blink.fs       (preview stripped output)
    """
    raw_lines = open(file).read().splitlines()

    # Build the cleaned line list: skip blank/comment lines, strip inline comments,
    # and track the mapping back to original file line numbers for error reporting.
    cleaned = []  # list of (original_lineno, stripped_content)
    for lineno, line in enumerate(raw_lines, 1):
        if is_blank_or_comment(line):
            continue
        stripped = _strip_inline_comment(line)
        if stripped:
            cleaned.append((lineno, stripped))

    if clean:
        for _, line in cleaned:
            click.echo(line)
        return

    line_delay = delay / 1000.0

    if not cleaned:
        if not pipe:
            click.echo(f"Uploaded 0 lines from {file}")
        return

    if pipe:
        for _, line in cleaned:
            click.echo(line.rstrip() + "\r", nl=False)
            time.sleep(line_delay)
        return

    # Resolve port: explicit > auto-detected > hardcoded default
    resolved_port = port or _detect_port() or FORTH_PORT

    t0 = time.monotonic()
    chars_sent = 0
    exit_code = 0

    try:
        with serial.Serial(resolved_port, baud, timeout=timeout) as ser:
            time.sleep(0.1)
            ser.reset_input_buffer()

            depth = 0
            for orig_lineno, line in cleaned:
                bare = line.rstrip()

                # Context-switching words need their exact echo confirmed.
                # 'empty' is especially slow as it erases all user flash.
                if bare in _CONTEXT_WORDS:
                    ser.write((bare + "\r\n").encode())
                    chars_sent += len(bare) + 2
                    if verbose:
                        click.echo(
                            f"[{orig_lineno:4d}] {bare}  (waiting for context switch)"
                        )
                    ok = _wait_for_exact(ser, _CONTEXT_WORDS[bare], timeout=10.0)
                    if not ok:
                        click.echo(
                            f"Error on line {orig_lineno}: '{bare}' got no confirmation",
                            err=True,
                        )
                        exit_code = 1
                        break
                    continue

                ser.write((bare + "\r\n").encode())
                chars_sent += len(bare) + 2

                if verbose:
                    click.echo(f"[{orig_lineno:4d}] {bare}")

                if _opens_definition(bare):
                    depth += 1
                if _closes_definition(bare):
                    depth = max(0, depth - 1)

                if depth > 0:
                    time.sleep(line_delay)
                    if ser.in_waiting:
                        ser.read(ser.in_waiting)
                else:
                    ok, response = _wait_for_ok(ser, timeout)
                    if not ok:
                        # Lines starting with '-' are marker deletions; a '?'
                        # response on first upload is expected (marker not yet
                        # defined) and is not a fatal error.
                        if bare.startswith("-"):
                            if verbose:
                                click.echo(
                                    f"[{orig_lineno:4d}] warning: '{bare}' not yet defined"
                                )
                            continue
                        click.echo(f"Error on line {orig_lineno}: {bare}", err=True)
                        if response.strip():
                            click.echo(f"  Response: {response.strip()}", err=True)
                        exit_code = 1
                        break

    except serial.SerialException as exc:
        click.echo(f"Serial error: {exc}", err=True)
        exit_code = 1

    elapsed = time.monotonic() - t0
    rate = int(chars_sent / elapsed) if elapsed > 0 else 0
    click.echo(
        f"Uploaded {len(cleaned)} lines, {chars_sent} chars"
        f" in {elapsed:.2f}s ({rate} chars/sec)"
    )
    if exit_code:
        raise SystemExit(exit_code)


if __name__ == "__main__":
    fu()
