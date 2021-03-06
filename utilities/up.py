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
from rich.console import Console
from rich.traceback import install
from rich.theme import Theme
from rich.text import Text
from rich.table import Table


# sys.tracebacklimit = 0

custom_theme = Theme({
    "empty": "bold green4",
    "EOF": "bold blue3",
    "error": "bold red1",
    "standard": "dim black"
})
con = Console(theme=custom_theme)

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
            text = Text(style="error")
            text.append("Serial port is busy, probably due to another connection.")
            text.append("\nDisconnect other serial program and re-run.")
            con.print(text, width=120)
            sys.exit()
        else:
            text = Text(style="error")
            text.append("Could not open serial port, ")
            text.append(config.port, style="black")
            text.append(", due to: ")
            text.append(os.strerror(e.errno), style="black")
            con.print(text, width=120)
            sys.exit()


def parse_arg(config):
    parser = argparse.ArgumentParser(
        description="Upload app for FlashForth",
        epilog="""
        Upload to board at higher speeds. To list serial ports:
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

    # check input line regex
    comment = re.compile(r'^\s*\\ .*')      # comment line
    blankline = re.compile(r'^\s*$')        # blank line
    marker_cmd = re.compile(r'^(marker.-)')  # marker command
    del_cmd = re.compile(r'^(-)')           # delete marker command
    empty_cmd = re.compile(r'empty\n')      # empty line

    # check response line regex
    boot = b'ATmega328 14.04.2022\r\n'      # boot response
    null_response = b'\r\n'                 # null response following boot
    badline = b'?\r\n'                      # bad line (compilation error)
    defined = b'DEFINED\r\n'                # word already defined
    compile = b'COMPILE ONLY\r\n'           # compile only error
    del_marker = b'-'                       # starts with -, delete marker

    lineno = 0
    n_bytes_sent = 0
    responses = []
    original = []
    try:
        # FF responds with a reset message and line feed upon connection
        table = Table(width=170)
        table.add_column("Line", width=6, justify="right")
        table.add_column("Original", width=70, justify="left")
        table.add_column("Response", width=90, justify="left")
        compile_error = False

        responses.append(config.ser.readline())
        responses.append(config.ser.readline())

        for line in open(config.file, "rt"):
            original.append(line)
            lineno += 1

            if not (comment.match(line) or blankline.match(line)):
                config.ser.write(str.encode(line))
                n_bytes_sent += len(line)
                response = config.ser.readline()

                if ((response.endswith(badline) or
                     response.endswith(defined) or
                     response.endswith(compile)) and
                        not response.startswith(del_marker)):
                    compile_error = True
                    # text = Text(style="error")
                    # text.append("Compilation error, line: ")
                    # text.append(str(lineno), style="black")
                    # text.append(" ")
                    # text.append(str(response.rstrip(b'\r\n'),'utf-8'), style="standard")
                    # con.print(text, width=120)
                    responses.append(response)
                    table.add_row(str(lineno), line[:-1], str(response), style="error")
                else:
                    table.add_row(str(lineno), line[:-1], str(response))
            # time.sleep(int(config.newlinedelay) * .001)

        # if last line isn't a line feed, fix and advise of error
        # subtract 1, as index goes from 0 and file counts from 1
        orig_index = lineno - 1
        if (not original[orig_index].endswith('\n')):
            config.ser.write(str.encode('\n'))
            text = Text(style="EOF")
            text.append("Last line, ")
            text.append(str(lineno), style="black")
            text.append(", must be only a new line, however, it contains: ")
            text.append(original[orig_index], style="black")
            text.append("\nA new line was sent to ensure closing the last word in the file.")
            con.print(text, width=120)
        if compile_error:
            con.print(table)
        return [n_bytes_sent, lineno]
    except (OSError) as e:
        if parent_fname is not None:
            sys.stderr.write(f"*** {parent_fname}({parent_lineno}): {e} ***")
        else:
            sys.stderr.write(f"*** : {e} ***")
        sys.exit(1)


main()
