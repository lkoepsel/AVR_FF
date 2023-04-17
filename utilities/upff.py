#!/usr/bin/env python3
# Latest version April 17, 2023
# Features:
# Will disconnect/reconnect Serial automatically
# If an error occurs, won't reconnect Serial and will activate ST
# Checks for eeprom, flash, ram words and waits for response

# Possible commands and responses, not used. Retained for consideration.
# marker_cmd = re.compile(r'^(marker.-)')  # marker command
# del_cmd = re.compile(r'^(-)')           # delete marker command
# empty_cmd = re.compile(r'empty\n')      # empty line

# check response line regex
# boot = b'ATmega328 14.04.2022\r\n'      # boot response
# null_response = b'\r\n'                 # null response following boot

import argparse
import serial
import sys
import os
import re
import datetime
import time
import subprocess
from time import sleep
from dotenv import dotenv_values


class Config(object):
    def __init__(self):
        self.port = os.environ.get('port', '/dev/cu.usbmodem3199')
        self.baudrate = '38400'
        self.xonxoff = False
        self.newlinedelay = 5
        self.empty = .25
        self.bytesize = 8
        self.parity = 'N'
        self.stopbits = 1


# Serial Disconnect Connect
# Sends a Cmd-D to Serial to disconnect then reconnect serial port
def Serial_CmdD():
    cmd = ['open', '/Applications/Serial_DC.app/']
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"Serial returned: {result.stdout=} {result.stderr=}")
    return result.returncode


# Serial Disconnect Connect
# Sends a Cmd-D to Serial to disconnect then reconnect serial port
def activate_ST():
    cmd = ['open', '/Applications/act_ST.app/']
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"Serial returned: {result.stdout=} {result.stderr=}")
    return result.returncode


def serial_open(config):
    if config.cfg:
        print("Port:"
              + str(config.port)
              + " Baudrate:"
              + str(config.baudrate)
              + " xonxoff:"
              + str(config.xonxoff)
              + " newlinedelay:"
              + str(config.newlinedelay)
              + " empty:"
              + str(config.empty)
              + " bytesize:"
              + str(config.bytesize)
              + " parity:"
              + str(config.parity)
              + " stopbits:"
              + str(config.stopbits))

    atts = 1
    while(atts < 5):
        try:
            config.ser = serial.Serial(config.port, config.baudrate, timeout=1,
                                       xonxoff=config.xonxoff)
            atts = 5

        except serial.SerialException as e:
            if e.errno == 16:

                print(f"Serial port busy, disconnecting app Serial")
                rs = Serial_CmdD()
                if rs == 0:
                    print(
                        f"Disconnected Serial app after {atts} attempt(s)")
                    sleep(.6)
                    # sleep req'd to prevent a race condition with Serial
                else:
                    sleep(.5)
                    print(f"{atts=}")
                atts += 1

            else:
                print(f"Could not open serial port, {config.port}")
                print(f"Due to: {os.strerror(e.errno)}")
                sys.exit()


def parse_arg(config):
    parser = argparse.ArgumentParser(
        description="Upload app for FlashForth",
        epilog="""
        Upload to board at higher ss. To list serial ports:
        python3 -m serial.tools.list_ports
        """)
    parser.add_argument("file", metavar="FILE", help="file to send")
    parser.add_argument("--config", "-f", action="store_true",
                        default=False,
                        help="Print port configuration, default is False")
    parser.add_argument("--port", "-p", action="store",
                        type=str, default='/dev/cu.usbmodem3188',
                        help="Serial port name")
    parser.add_argument("--xonxoff", action="store_true",
                        default=True,
                        help="Serial port XON/XOFF enable, default is True")
    parser.add_argument("--baudrate", "-s", action="store",
                        type=str, default=38400,
                        help="Serial port baudrate, default is 38400")
    parser.add_argument("-n", "--newlinedelay", action="store",
                        type=int, default=1,
                        help="Newline delay(milliseconds), default is 2")
    parser.add_argument("-e", "--empty", action="store_true", default=True,
                        help="empty command prior to upload")
    parser.add_argument("-t", "--print-statistics", action="store_true",
                        default=True, help="print transfer statistics")
    parser.add_argument("-v", "--verbose", action="store_true", default=False,
                        help="print all lines in input file, default is False")

    arg = parser.parse_args()
    config.cfg = arg.config
    config.file = arg.file
    config.port = arg.port
    config.xonxoff = arg.xonxoff
    config.baudrate = arg.baudrate
    config.newlinedelay = arg.newlinedelay
    config.empty = arg.empty
    config.stats = arg.print_statistics
    config.verbose = arg.verbose
    config.bytesize = 8
    config.parity = 'N'
    config.stopbits = 1


def clean_file(c):
    f = []
    c_line = 1
    lines = []
    lines.append(0)
    comment = re.compile(r'^\s*\\ .*')      # comment line
    blankline = re.compile(r'^\s*$')        # blank line

    for n, line in enumerate(open(c.file, "rt"), 1):
        if not (comment.match(line) or blankline.match(line)):
            no_comments = re.sub(r'^(.*?)(\\.*)', "\\1", line)
            f.append(no_comments)
            # print(f"{n=} {c_line=} {no_comments}")
            lines.append(n)
            c_line += 1
    return(f, lines)


# warm_ready sends a warm reset and waits for an ok
def warm_ready(c):
    ready = b'  ok<#,ram> \r\n'             # ready response, ready for input
    warm = '\x0D'                           # hex code for a warm boot
    init_response = ""
    while (init_response != ready):
        c.ser.write(str.encode(warm))
        time.sleep(int(c.newlinedelay) * .01)
        init_response = c.ser.readline()
    return("warm")


# empty_ready can follow a warm_ready, and sends the empty command
# then waits for the empty command to be echoed back with an ok
def empty_ready(c):
    empty_cmd = 'empty\r\n'                  # empty line
    empty_ready = b'empty  ok<#,ram> \r\n'   # ready response, ready for input
    init_response = ""

    c.ser.write(str.encode(empty_cmd))
    print("empty word sent")
    while (init_response != empty_ready):
        init_response = c.ser.readline()
    return("empty")


# flash - the flash response is different and needs to be accounted for
def flash_ready(c):
    print(f"flash command found", end=' ')
    flash_cmd = 'flash\r\n'                  # empty line
    flash_ready = b'flash  ok<#,flash> \r\n'   # flash response
    init_response = ""

    c.ser.write(str.encode(flash_cmd))
    print("flash word sent")
    while (init_response != flash_ready):
        init_response = c.ser.readline()
        # print(f"{init_response=}")
    return("flash")


# eeprom - the eeprom response is different and needs to be accounted for
def eeprom_ready(c):
    print(f"eeprom command found", end=' ')
    eeprom_cmd = 'eeprom\r\n'                  # empty line
    eeprom_ready = b'eeprom  ok<#,eeprom> \r\n'   # eeprom response
    init_response = ""

    c.ser.write(str.encode(eeprom_cmd))
    print("eeprom word sent")
    while (init_response != eeprom_ready):
        init_response = c.ser.readline()
        # print(f"{init_response=}")
    return("eeprom")


# ram - the ram response is different and needs to be accounted for
def ram_ready(c):
    print(f"ram command found", end=' ')
    ram_cmd = 'ram\r\n'                  # empty line
    ram_ready = b'ram  ok<#,ram> \r\n'   # ram response
    init_response = ""

    c.ser.write(str.encode(ram_cmd))
    print("ram word sent")
    while (init_response != ram_ready):
        init_response = c.ser.readline()
        # print(f"{init_response=}")
    return("ram")


def main():

    config = Config()
    parse_arg(config)

    values = dotenv_values('../.env')
    config.port = values['port']
    config.baudrate = values['baudrate']
    serial_open(config)
    t0 = datetime.datetime.now()
    n = xfr(None, 0, config)
    config.ser.close()

    if not n[0]:
        Serial_CmdD()

        et = datetime.datetime.now() - t0
        s = int(n[1] / et.total_seconds())
        print(f'\n{n[2]} lines, {et.total_seconds():4.2f} secs, {s} bytes/sec')

    else:
        activate_ST()


def xfr(parent_fname, parent_lineno, config):

    # check input line regex
    badline = b'?\r\n'                      # bad line (compilation error)
    defined = b'DEFINED\r\n'                # word already defined
    compile = b'COMPILE ONLY\r\n'           # compile only error
    del_marker = b'-'                       # starts with -, delete marker

    lineno = 0
    n_bytes_sent = 0
    original = []
    nl = '\n'

    try:
        # Wait for a ready response from a warm boot, prior to uploading file
        resp = warm_ready(config)

        clean_orig = clean_file(config)
        error_occurred = False
        for n, line in enumerate(clean_orig[0], 1):
            if line == 'empty\n':
                resp = empty_ready(config)
                print(f"{resp} response received, uploading", config.file)
                pass

            if line == 'flash\n':
                resp = flash_ready(config)
                print(f"{resp} response received, uploading", config.file)
                pass

            if line == 'eeprom\n':
                resp = eeprom_ready(config)
                print(f"{resp} response received, uploading", config.file)
                pass

            if line == 'ram\n':
                resp = ram_ready(config)
                print(f"{resp} response received, uploading", config.file)
                pass

            original.append(line)
            lineno += 1

            config.ser.write(str.encode(line))
            n_bytes_sent += len(line)
            resp = config.ser.readline()

            if ((resp.endswith(badline) or
                 resp.endswith(defined) or
                 resp.endswith(compile)) and
                    not resp.startswith(del_marker)):
                error_occurred = True
                print(f"{nl}**** Error Occurred ****")
                print(f"line {clean_orig[1][n]} was '{line.strip()}'")
                print(f"Response was {str(resp)=}")
                print(f"**** End of Error ****{nl}")
                break
            time.sleep(int(config.newlinedelay) * .001)

        # Determine if last line is a line feed, if not
        # Warn and send a line feed as last line
        # subtract 1, as index goes from 0 and file counts from 1
        orig_index = lineno - 1
        if (not original[orig_index].endswith('\n')):
            config.ser.write(str.encode('\n'))
            print(f"Last line, must only be a new line. ")
            print(f"New line sent to close last word in the file.")

        if not error_occurred:
            print(f"{nl}0 errors")
            print(f"{resp} was response to last line in file")
        return [error_occurred, n_bytes_sent, lineno]

    except (OSError) as e:
        if parent_fname is not None:
            sys.stderr.write(f"*** {parent_fname}({parent_lineno}): {e} ***")
        else:
            sys.stderr.write(f"*** : {e} ***")
        sys.exit(1)


main()
