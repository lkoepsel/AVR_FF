# DEPRECATED - do not use. Enhanced program is fu.py
import datetime
import re
import sys
from time import sleep

import click
import serial
import serial.tools.list_ports
from utilities.CT_connect import conn
from utilities.CT_disconnect import disc

folder = re.compile(r"^/")
comment = re.compile(r"^#")
main_prog = re.compile(r"^\+")
change = re.compile(r"^!")


def xfr(fname, ser_port, dt, c, v):

    # check input line regex
    badline = b"?\x15\r\n"  # bad line (compilation error)
    defined = b"DEFINED\r\n"  # word already defined
    compile_error = b"COMPILE ONLY"  # compile only error
    del_marker = b"-"  # starts with -, delete marker
    pause_line = re.compile(r"^#p[0-9]")

    lineno = 0
    n_bytes_sent = 0
    original = []
    nl = "\n"

    line_functions = {
        "empty\n": empty_ready,
        "flash\n": flash_ready,
        "eeprom\n": eeprom_ready,
        "ram\n": ram_ready,
    }

    try:
        # Wait for a ready response from a warm boot, prior to uploading file
        resp = warm_ready(ser_port, dt)
        clean_orig = clean_file(fname, c)
        error_occurred = False
        for n, line in enumerate(clean_orig[0], 1):
            if line in line_functions:
                resp = line_functions[line](ser_port)
                print(f"{resp} response received, uploading")

            original.append(line)
            lineno += 1

            ser_port.write(str.encode(line))
            n_bytes_sent += len(line)
            resp = ser_port.readline()
            resp_line = str(resp.rstrip(b"\r\n"), "utf8")
            if (
                resp.endswith(badline)
                or resp.endswith(defined)
                or compile_error in resp
                and not resp.startswith(del_marker)
            ):
                error_occurred = True
                print(f"{nl}**** Error Occurred ****")
                print(f"line {clean_orig[1][n]} was '{line.strip()}'")
                print(f"Response was {str(resp)}")
                print(f"**** End of Error ****{nl}")
                break
            else:
                if v:
                    orig_line_no = clean_orig[1][n]
                    resp_line = str(resp.rstrip(b"\r\n"), "utf8")
                    print(f"{orig_line_no}: {resp_line}")
            sleep(int(dt) * 0.001)

        # Determine if last line is a line feed, if not
        # Warn and send a line feed as last line
        # subtract 1, as index goes from 0 and file counts from 1
        orig_index = lineno - 1
        if not original[orig_index].endswith("\n"):
            ser_port.write(str.encode("\n"))
            print("Last line, must only be a new line. ")
            print("New line sent to close last word in the file.")

        if not error_occurred:
            print(f"{nl}Success!")
            print(f"{resp} was response to last line in file")
        return [error_occurred, n_bytes_sent, lineno]

    except OSError as e:
        if fname is not None:
            sys.stderr.write(f"*** {fname}(): {e} ***")
        else:
            sys.stderr.write(f"***: {e} ***")
        sys.exit(1)


def clean_file(ff_file, c):
    f = []
    c_line = 1
    lines = []
    lines.append(0)
    comment = re.compile(r"^\s*\\ .*")  # comment line
    blankline = re.compile(r"^\s*$")  # blank line

    for n, line in enumerate(open(ff_file, "rt"), 1):
        if not (comment.match(line) or blankline.match(line)):
            no_comments = re.sub(r"^(.*?)(\\.*)", "\\1", line)
            f.append(no_comments)
            lines.append(n)
            c_line += 1
    if c:
        for clean_line in f:
            print(f"{clean_line}", end="")
        sys.exit()
    else:
        return (f, lines)


# warm_ready sends a warm reset and waits for an ok
def warm_ready(s, dt):
    ready = b"  ok<#,ram> \r\n"  # ready response, ready for input
    warm = "\x0d"  # hex code for a warm boot
    init_response = ""
    while init_response != ready:
        s.write(str.encode(warm))
        sleep(int(dt) * 0.01)
        init_response = s.readline()
    return "warm"


# empty_ready can follow a warm_ready, and sends the empty command
# then waits for the empty command to be echoed back with an ok
def empty_ready(c):
    empty_cmd = "empty\r\n"  # empty line
    empty_ready = b"empty  ok<#,ram> \r\n"  # ready response, ready for input
    init_response = ""

    c.write(str.encode(empty_cmd))
    print("empty word sent")
    while init_response != empty_ready:
        init_response = c.readline()
    return "empty"


# flash - the flash response is different and needs to be accounted for
def flash_ready(c):
    print("flash command found", end=" ")
    flash_cmd = "flash\r\n"  # empty line
    flash_ready = b"flash  ok<#,flash> \r\n"  # flash response
    init_response = ""

    c.write(str.encode(flash_cmd))
    print("flash word sent")
    while init_response != flash_ready:
        init_response = c.readline()
        # print(f"{init_response=}")
    return "flash"


# eeprom - the eeprom response is different and needs to be accounted for
def eeprom_ready(c):
    print("eeprom command found", end=" ")
    eeprom_cmd = "eeprom\r\n"  # empty line
    eeprom_ready = b"eeprom  ok<#,eeprom> \r\n"  # eeprom response
    init_response = ""

    c.write(str.encode(eeprom_cmd))
    print("eeprom word sent")
    while init_response != eeprom_ready:
        init_response = c.readline()
        # print(f"{init_response=}")
    return "eeprom"


# ram - the ram response is different and needs to be accounted for
def ram_ready(c):
    print("ram command found", end=" ")
    ram_cmd = "ram\r\n"  # empty line
    ram_ready = b"ram  ok<#,ram> \r\n"  # ram response
    init_response = ""

    c.write(str.encode(ram_cmd))
    print("ram word sent")
    while init_response != ram_ready:
        init_response = c.readline()
        # print(f"{init_response=}")
    return "ram"


def check_port():
    for p in sorted(serial.tools.list_ports.comports()):
        if ("usbmodem" in p.device) or ("usbserial" in p.device) or ("COM" in p.device):
            return p.device
    click.echo("No valid serial ports found.")
    return None


@click.command("up")
@click.version_option("2.5", prog_name="up")
@click.option(
    "-p",
    "--port",
    "port",
    required=False,
    type=str,
    default="TBD",
    help="Port address (e.g., /dev/cu.usbmodem3101, COM3).",
)
@click.argument("forthfile", type=click.Path(exists=True, readable=True), required=True)
@click.option(
    "-c",
    "--clean",
    "clean",
    is_flag=True,
    default=False,
    help="Print clean file to be transferred and exit.",
)
@click.option(
    "-d",
    "--delay_line",
    "delay_line",
    default=0,
    help="delay in milliseconds * 10 per line, default is 0",
)
@click.option(
    "-b",
    "--baud",
    "baud",
    default=1000000,
    help="baud rate of serial port, default is 1000000",
)
@click.option(
    "-v",
    "--verbose",
    "verbose",
    is_flag=True,
    default=False,
    help="print response to every line",
)
def up(port, forthfile, delay_line, clean, baud, verbose):
    """
    Builds an FlashForth application on a board.
    Use with Sublime Text build automation
    https://github.com/lkoepsel/CT_build

    \b
    * Requires a text file containing FlashForth words
    * -p port is not required, use if up is not finding the proper serial port,
    it will guess using 'usbmodem' or 'COM' as an indicator
    * Use '-c' to view the exact lines which are transferred, before transfer,
    file is cleaned of all comments, increasing transfer speed
    * Use '-v' to view the response line for each line transmitted,
    this allows for more detailed debugging
    * Use '-d n' for a n*10ms delay between lines, use if upload has
    errors uploading due to transfer speed
    * Use '-b n' for the serial baud rate, FlashForth has been tested with
    1000000 and it works well, 250000 works well, stock FlashForth is 38400
    """

    disc()
    if port == "TBD":
        port = check_port()
        if port is None:
            click.echo("No valid ports found, re-run with -p option")
            sys.exit(1)

    click.echo(f"Building FF app using {forthfile} file on {port}")
    ser = serial.Serial(port, baud, timeout=1)
    t0 = datetime.datetime.now()
    n = xfr(forthfile, ser, delay_line, clean, verbose)
    et = datetime.datetime.now() - t0
    s = int(n[1] / et.total_seconds())
    print(f"\n{n[2]} lines, {et.total_seconds(): 4.2f} secs, {s} bytes/sec")

    conn()
    sys.exit()
