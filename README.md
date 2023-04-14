# Programming the Arduino Uno in FlashForth

I originally thought of this framework as similar to the Arduino framework, where one has libraries which contain specialized code for specific operations. While that makes some sense, it isn't "Forth-like", in that it isn't the way Forth was designed to work. Or more importantly, as this continues to be a site to help someone program in Forth, the Arduino approach isn't appropriate to Forth.

A key value of Forth, is that it enables the programmer to deeply understand the processor on which they are working. At times, this can be a detriment, as it takes much more time to translate a datasheet into working code as compared to downloading a library, this approach helps you learn how the processor works. This knowledge can be valuable as you continue to work with the processor.

The file *m328Pdef.inc* is included in this repository to document AATmega328P constants, the file *Library/328P_HAL.fs* will be the Forth version of this, except it will base many of the constants per the UNO as mentioned above.

The ATmega328P datasheet is the ultimate arbiter of register names and usage.

## Boards and Microcontrollers
This code is being developed for the Arduino Uno. If tested on other boards, I will note that.

## Using up.py to transfer files
The utilities/up.py script will upload a forth file to the FlashForth Uno. It will strip all comments from file prior to sending. I've found this reduces possible errors and accelerates the speed of transmission.:
```
usage: upload_FF.py [-h] [--config] [--port PORT] [--xonxoff] [--baudrate BAUDRATE] [-n NEWLINEDELAY] [-e] [-t] [-v] FILE

Upload app for FlashForth

positional arguments:
  FILE                  file to send

optional arguments:
  -h, --help            show this help message and exit
  --config, -f          Print port configuration
  --port PORT, -p PORT  Serial port name
  --xonxoff             Serial port XON/XOFF enable, default is True
  --baudrate BAUDRATE, -s BAUDRATE
                        Serial port baudrate, default is 38400
  -n NEWLINEDELAY, --newlinedelay NEWLINEDELAY
                        Newline delay(milliseconds), default is 0
  -e , --empty 
                        issue empty command prior to sending, will remove all user words! Default is false
  -t, --print-statistics
                        print transfer statistics, default is true
  -v, --verbose         print all lines in input file, default is False

Upload to board at higher speeds.
```
The best way to use it is:
```
$ up.py Library/328P_HAL.fs

*** lines read: 227
*** elapsed time: 0:00:03.876169 (1189 bytes/s) ***
```
If a compilation is failing, the program will automatically print all lines with the errors in red. My recommendation is to fix the first error found, as Forth compilation will fail many times, once an error has occurred.

### .env file required
A .env file is required at the parent level *./AVR_FF* for the port, with *port* defined. An example is:
```
port=/dev/cu.usbmodem1401
```
### Python
At a minimum, *pyserial*, *rich* and *dotenv* are required using your favorite Python installer...pip, Anaconda, etc.
## examples folder
* *blink* - classic *Hello, World* example for microcontrollers
* *buttons* - demonstrates how to use button de-bouncing found in the HAL
* *fsm* - finite state machine with extensive documentation
* *tasks* - example used for benchmarking in this [entry](/posts/board-language_speed/)
* *timing* - using the examples in the forth folder, how to create blocking timing loops
## forth folder
The **forth** folder is a collection of forth files which are part of FlashFort demonstrations written by Mikael N. Please use them as examples as well.
## Library folder
* *328P_HAL* - mandatory file required for many of the examples, baseline words and definitions for the Arduino Uno
* *T1_CTC* - Timer 1, CTC definitions
* *T1_PWM* - Timer 1, PWM definitions
* *T2_FPWM* - Timer 2, Fast PWM definitions
* *T2_ms* - Timer 2, 1ms interrupt definitions
## utilities folder
See up.py discussion above.
## Sources
I also write about C, MicroPython and Forth programming on microcontrollers at [Wellys](https://wellys.com).

Other sources of information which were helpful:
* [FlashForth Atmega](https://flashforth.com/atmega.html)
* [Forth & Arduino](https://arduino-forth.com) Outstanding site with tutorials on Flashforth and the AVR microcontrollers
* [FlashForth Intro on Wellys](https://wellys.com/posts/flashforth/)