# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a FlashForth development environment for the Arduino Uno/Nano (ATmega328P). FlashForth is a Forth interpreter that runs directly on the microcontroller — code is developed interactively over a serial connection rather than compiled on a host machine.

**Target hardware**: Arduino Uno, Arduino Nano, Microchip 328PB Xplained Mini  
**Firmware**: FlashForth 5.0 (custom-compiled at 250K baud)  
**Serial tool**: CoolTerm (`FF5_0.CoolTermSettings`) or `tio`  
**File upload**: `fu.py` (primary upload tool in this repo)

## Uploading Code to the Board

There is no build/compile step on the host. Workflow:

1. Connect to the board via CoolTerm (`FF5_0.CoolTermSettings`, 250K baud) or `tio`
2. Upload `.fs` files with `fu.py` (see below)
3. **Always load `Library/328P_ports.fs` first** — it is the mandatory HAL required by virtually all other files
4. Test interactively at the Forth prompt; `empty` clears all user definitions

## fu.py — Forth Upload Tool

`fu.py` is self-contained in this repo. It requires `click` and `pyserial`; `pyproject.toml` declares both. Install once with `uv`, then use the `fu` command from anywhere:

```bash
# Install (run once from the repo root)
uv tool install .

# Reinstall after editing fu.py
uv tool install --reinstall .
```

`fu` uploads `.fs` files to the board over serial. It skips blank lines and whole-line `\` comments, strips trailing inline comments before sending, and uses a depth-tracking strategy: lines inside a colon definition are paced with a configurable delay; lines at interpreter level wait for the `ok` prompt.

```bash
# Basic upload (auto-detects USB serial port)
fu Library/328P_ports.fs

# Specify port and baud explicitly
fu --port /dev/ttyACM0 --baud 250000 blink.fs

# Preview exactly what will be sent (no serial connection opened)
fu --clean blink.fs

# Verbose: print each line as it is sent with original line numbers
fu --verbose blink.fs

# Increase per-line delay inside definitions if upload errors occur
fu --delay 100 blink.fs

# Pipe mode for tio (press ctrl-t R inside tio, then run this)
fu --pipe blink.fs
```

**Key behaviours:**
- Port auto-detection scans for `ttyUSB*`, `ttyACM*`, `usbserial`, `usbmodem`, `COM*`; falls back to `/dev/ttyUSB0`
- Context-switching words (`empty`, `flash`, `ram`, `eeprom`) are sent individually and confirmed by their exact echo response before upload continues — `empty` gets a 10-second timeout since it erases all user flash
- Lines starting with `-` (marker deletions like `-end_ports`) are non-fatal on first upload — a `?` response means the marker didn't exist yet, which is expected
- Errors report the **original file line number** (not the stripped-line count) so the location matches the editor
- FlashForth error responses (`?` + NAK byte `\x15`, `DEFINED`, `COMPILE ONLY`) abort the upload; stats are always printed even on error

## Repository Structure

```
Library/     Reusable word definitions (HAL + peripheral drivers)
  328P_ports.fs    Mandatory HAL: Arduino pin constants + port I/O words
  T1_CTC.fs        Timer 1 CTC mode
  T1_FPWM.fs       Timer 1 Fast PWM
  T1_PWMPC.fs      Timer 1 Phase Correct PWM
  T2_FPWM.fs       Timer 2 Fast PWM
  T2_ms.fs         Timer 2 1ms interrupt (millisecond counter)
  buttons.fs       Button debouncing via Timer 0 ISR
  buttonsv2.fs     Alternative debounce implementation
  table.fs         Jump table / table lookup words
  time_it.fs       Execution timing utility

forth/       FlashForth demo files from upstream author Mikael N.
  asm.fs           AVR assembler written in Forth
  asm2.fs          Extended assembler
  task.fs          Cooperative multitasking primitives
  i2c-base.fs      TWI/I2C low-level driver
  i2c-ds1307.fs    DS1307 RTC over I2C
  see.fs / see2.fs Forth decompiler/disassembler
  irqAtmega328.fs  Interrupt vector definitions for ATmega328P
  bit.fs           Bit manipulation words
  xdump.fs         Memory dump utility

examples/    Standalone example programs
  blink.fs         LED blink (Hello World)
  fsm.fs           Finite state machine with timers, buttons, PWM
  tasks.fs         Multitasking benchmark
  timing.fs        Blocking delay timing loops

fu.py              Forth upload tool (primary way to send .fs files to the board)
pyproject.toml     Package definition for fu.py (declares click + pyserial deps)
m328Pdef.inc       ATmega328P register/constant definitions (reference)
wordsAll.txt       Complete FlashForth word list (reference)
```

## FlashForth Conventions Used in This Repo

**File structure**: Every file begins with `-name` (removes prior marker/definitions) then `marker -name` to set a removal point. Files often end with `-end_name` / `marker -end_name`.

**Stack comments**: `( before -- after )` on the same line as the definition.

**Flash vs RAM**: Use `flash` before definitions stored in flash (constants, code), `ram` for variables. The HAL file starts with `empty` to wipe all previous user flash definitions.

**Pin constants**: Defined as `2constant` pairs — `(bitmask ddr-addr)`. Always reference pins by Arduino board number (e.g., `LED`, `D13`, `D2`) rather than AVR register names directly.

**Port I/O words** (defined in `328P_ports.fs`):
- `out` / `in` / `pullup` — set direction
- `high` / `low` / `tog` — set output state
- `on` / `off` — aliases for high/low
- `read` — read a pin, returns bit value (not boolean)

**Interrupts**: ISR words use `;i` instead of `;`. Install with `['] isr-word vec-num int!`. Always disable the interrupt before redefining the ISR word.

**Multitasking**: Cooperative via `task.fs`. Tasks are defined with `task:`, initialized with `tinit`, and started with `run`. Background tasks typically have no TIB (tib-size = 0).

**Assembler**: `forth/asm.fs` provides an inline AVR assembler. Instruction words end with a comma (e.g., `mov,`, `ldi,`, `ret,`). Branch condition codes are constants (`eq,`, `cs,`, `mi,`, etc.). Use `if,` / `then,` / `begin,` / `until,` for control flow in assembly.

## Compiling a Custom FlashForth Binary

Requires MPLAB X IDE + XC8 compiler **v2.45** (v2.46 generates errors).

Key customizations made to this build:
- Baud rate: 250K (changed in `config-xc8.inc`)
- Added port/timer registers directly to `registers.inc` (DDRB/DDRC/DDRD, TCCR*, OCR*, TIMSK*)
- Custom reset banner string

Programming command (Microchip SNAP, ATmega328P):
```bash
avrdude -p m328p -P usb -c snap_isp -e -U flash:w:FF.hex:i -U efuse:w:0xfc:m -U hfuse:w:0xdf:m -U lfuse:w:0xff:m
```
