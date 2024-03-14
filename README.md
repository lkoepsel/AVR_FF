# Programming the *Arduino* *Uno* in *Flashforth*

I originally thought of this framework as similar to the *Arduino* framework, where one had libraries which contain specialized code for specific operations. While a Hardware Abstraction Layer (HAL) makes sense, creating Libraries in the vein of the *Arduino* framework, does not. It isn't "Forth-like", in that Libraries aren't the way *Forth* was designed to work. Or most importantly, as this continues to be a site to help someone program in *Forth*, the *Arduino* approach isn't appropriate to developing code in *Forth*.

A key value of *Forth*, is that it enables the programmer to deeply understand the processor on which they are working. At times, this can be a detriment, as it takes much more time to translate a datasheet into working code as compared to downloading a library, however, this approach helps you learn how the processor works. This knowledge can be valuable as you continue to work with the processor.

The file *m328Pdef.inc* is included in this repository to document AATmega328P constants, the file *Library/328P_HAL.fs* is the *Forth* version of this, except it will base many of the constants per the UNO as mentioned above.

The ATmega328P datasheet is the ultimate arbiter of register names and usage.

## Boards and Microcontrollers
This code is being developed for the *Arduino* *Uno*. If tested on other boards, I will note that.

## Automating serial transfers
In developing code, it helps to automate the mundane tasks, in particular, uploading files to the microcontroller board. As I have several development environments which require such automation, I created a repository specific to the task, [CT_build](https://github.com/lkoepsel/CT_build). It automates microcontroller upload tasks for *C*, *MicroPython* and *FlashForth*. Please consider using it.

The previously existing *utilities* folder has been replaced by the *up* tool in the repository above.

## Compiling FlashForth
I have a complete [entry](https://wellys.com/posts/flashforth_compile/), however, I will touch on it here as well. Its beneficial to compile your own version as it allows you to continue to make things more efficient for you. For example, I do the following:
1. Increase the baud rate to 250K
2. Add the 3 DDRn registers, DDRB, DDRC and DDRD. It's easy to reference the other two registers for each port, off of the DDRn register.
3. Change the prompt, so I know that this is a different version
4. 
## examples folder
* *blink* - classic *Hello, World* example for microcontrollers
* *buttons* - demonstrates how to use button de-bouncing found in the HAL
* *fsm* - finite state machine with extensive documentation
* *tasks* - example used for benchmarking in this [entry](/posts/board-language_speed/)
* *timing* - using the examples in the forth folder, how to create blocking timing loops
## forth folder
The **forth** folder is a collection of forth files which are part of FlashFort demonstrations written by Mikael N. Please use them as examples as well.
## Library folder
* *328P_HAL* - mandatory file required for many of the examples, baseline words and definitions for the *Arduino* *Uno*
* *T1_CTC* - Timer 1, CTC definitions
* *T1_PWM* - Timer 1, PWM definitions
* *T2_FPWM* - Timer 2, Fast PWM definitions
* *T2_ms* - Timer 2, 1ms interrupt definitions
## utilities folder
See up.py discussion above.
## Sources
I also write about C, MicroPython and *Forth* programming on microcontrollers at [Wellys](https://wellys.com).

Other sources of information which were helpful:
* [FlashForth Atmega](https://flashforth.com/atmega.html)
* [Forth & *Arduino*](https://arduino-forth.com) Outstanding site with tutorials on Flashforth and the AVR microcontrollers
* [FlashForth Intro on Wellys](https://wellys.com/posts/flashforth/)