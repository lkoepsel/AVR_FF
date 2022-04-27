#! python3
import argparse
import serial
import sys
import os
import re
import datetime
import traceback
import time
from dotenv import dotenv_values

class Config(object):
    def __init__(self):
        self.port = os.environ.get('port', '/dev/cu.usbmodem3199')
        self.baudrate = '38400'
        self.xonxoff = True
        self.newlinedelay = 50
        self.empty = .25
        self.bytesize = 8
        self.parity = 'N'
        self.stopbits = 1


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
    try:
        config.ser = serial.Serial(config.port, config.baudrate, timeout=1,
                                   xonxoff=config.xonxoff)
    except serial.SerialException as e:
        if e.errno == 16:
            print(f"Serial port is busy, probably due to another connection.")
            print(f"Disconnect other serial program and re-run.")
            sys.exit()
        else:
            print('Could not open serial port', config.port, 'due to:',
                  os.strerror(e.errno))
       # raise e


def parse_arg(config):
    parser = argparse.ArgumentParser(
        description="Upload app for FlashForth",
        epilog="""Upload to board at higher speeds.""")
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
                        type=int, default=60,
                        help="Newline delay(milliseconds), default is 60")
    parser.add_argument("-e", "--empty", action="store",
                        type=int, default=100,
                        help="words empty delay, default is 100")
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

# specific function to show hex ASCII values for hidden characters on error


def rtoASCII(r):
    print(str(bytearray(r)))
    # return " ".join(str(hex(char)) for char in r)
    return


def main():

    config = Config()
    parse_arg(config)

    values = dotenv_values('.env')
    config.port = values['port']
    serial_open(config)
    t0 = datetime.datetime.now()
    n = xfr(None, 0, config)
    elapsed_time = datetime.datetime.now() - t0
    speed = int(n[0] / elapsed_time.total_seconds())
    if config.stats:
        print(f'\n*** lines read: {n[1]}')
        print(f"*** elapsed time: {elapsed_time} ({speed} bytes/s) ***")


def xfr(parent_fname, parent_lineno, config):

    comment = re.compile(r'^\s*\\ .*')
    blankline = re.compile(r'^\s*$')
    badline = b'?\r\n'
    defined = b'DEFINED\r\n'
    marker_cmd = re.compile(r'^(marker.-)')
    del_cmd = re.compile(r'^(-)')
    empty_cmd = re.compile(r'empty\n')
    lineno = 1
    n_bytes_sent = 0
    response = []
    orig_file = ['Start']
    try:
        for line in open(config.file, "rt"):
            orig_file.append(line)
            if config.verbose:
                print(lineno, line, end='')
            if (empty_cmd.match(line) or marker_cmd.match(line) or
                    del_cmd.match(line)):
                if config.verbose:
                    print(f"empty command or marker found")
                time.sleep(config.empty * .001)
            if (not comment.match(line)) and (not blankline.match(line)):
                config.ser.write(str.encode(line))
                n_bytes_sent += len(line)
                response_line = config.ser.readline()
                time.sleep(config.newlinedelay * .001)
                response.append(response_line)
                if (response_line.endswith(badline) or
                        response_line.endswith(defined)):
                    m = (
                        f"*** Compilation error, line:"
                        f"{lineno - 1} {str(response_line)}"
                    )
                    print(m)

            lineno += 1
        if (not orig_file[lineno - 1].endswith('\n')):
            lineno -= 1
            m = (
                f"Last line ({lineno}) must end w/ a new line:"
                f" {orig_file[lineno]}"
                )
            print(m)
        return [n_bytes_sent, lineno]
    except (OSError) as e:
        if parent_fname is not None:
            sys.stderr.write(f"*** {parent_fname}({parent_lineno}): {e} ***")
        else:
            sys.stderr.write(f"*** : {e} ***")
        sys.exit(1)


main()
