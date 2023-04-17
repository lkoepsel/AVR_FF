-fsm
marker -fsm

\ finite STATE machine using two buttons
\ button 1 - advances STATE and lights STATE LEDs
\ button 2 - enters STATE (executes STATE) and lights blue LED
\ BIT_0 - bit 0 of STATE LEDs
\ BIT_1 - bit 1 of STATE LEDs
\ Blue LED - indicates state entered by intensity (off/DIM/MED/MAX)
\ and executing state
\ Speaker - Ultrasonic transducer, 4 states (Off, Audible, 22kHz, 44kHz)
\ Timer 0 - Debounce the buttons (check buttons every 8ms)
\ Timer 1 - CTC PWM for ultrasonic transducer
\ Timer 2 - Fast PWM to vary the intensity of the Blue LED

\ Timer 1 - provides freq for ultrasonic transducer
\ Initialize timer 1 to a Clear Timer on Compare Match (CTC) pin 10
\ Timer 1 definitions pgs 140-166 DS40002061B
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ CTC Mode 4: WGM3:0 = 0100 N=8 (scalar)
\ OC1A/B: COM1B1:0 = 0001 Toggle on Compare Match, Top = OCR1A

\ Timer 1 definitions pgs 140-166 DS40002061B
$80 constant TCCR1A \ T/C 1 Control Register A
$81 constant TCCR1B \ T/C 1 Control Register B
$88 constant OCR1AL \ Output Compare Register Low byte
$89 constant OCR1AH \ Output Compare Register High byte

\ initialize T/C 1 B ONLY as CTC, Top = OCR1A
\ top determines the freq as in freq = 16MHz/(2*N*(1+top))
\ 31 => 16MHz/(2*8*32) = 31.25kHz, 1023 => 976Hz
: speaker ( top -- )
  OCR1AL ! \ freq is based on OCR1A
  %00010000 TCCR1A c!   \ COM1B1, toggle OC1B on timeout
  %00001010 TCCR1B c!   \ WGM12 scalar, mode 4 and scalar=8
  D10 output            \ OC1B (D10) is output
;

\ Timer 2 provides variable intensity (via duty cycle) of Blue LED
\ Mode 3, Fast PWM, Top=OCRA, Clear OC2A on Compare Match
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 10000011
\ WGM22 WGM21 WGM20 => Fast PWM, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00000010
\ OCR2A = variable, determines duty cycle of PWM
\ CS21 => scalar of 8
\ Frequency = fixed @7.8kHz, duty cycle/255 on stack
\ 

\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A

: blue  ( dc/255 -- )
  OCR2A c!
  %10000011 TCCR2A c!
  %00000010 TCCR2B c!
  D11 output
;


D8       2constant BIT_0    \ State LEDs
D9       2constant BIT_1
D10      2constant LED_BLUE \ Transducer Blue LED
0        constant OFF       \ Intensities for Blue LED
31       constant DIM
100      constant MED
255      constant MAX
510      constant AUDIBLE   \ Transducers freq: 2kHz
44       constant 22K       \ 22kHz
24       constant 40K       \ 44kHz

variable FSM_STATE

: init_LEDs
    BIT_0 output
    BIT_1 output
    LED_BLUE output
;

\ words which run when button 1 is pressed
: init_STATE 0 FSM_STATE ! ;
: inc_STATE 1 FSM_STATE +! ; \ increment STATE

\ STATE ranges from 0-3, at 3 it goes back to 0
: check_STATE ( -- ) 
    FSM_STATE @ 3 =
    if init_STATE
    else inc_STATE
    then
;

\ converts STATE to LED, made easy by using sequential pins
: STATE2LED ( -- )
    %00000011 PORTB mclr
    FSM_STATE @ PORTB mset
;

\ There isn't a OCR1A value to turn the speaker off
\ so the pin is made an input
: OFF_speaker
    D10 input
;

\ STATES to execute
: STATE_0
    OFF blue
    OFF_speaker
;

: STATE_1
    DIM blue
    AUDIBLE speaker
;

: STATE_2
    MED blue
    22K speaker
;

: STATE_3
    MAX blue
    40K speaker
;

\ store jumptable in flash for STATES
flash
create STATES
    ' STATE_0 ,
    ' STATE_1 ,
    ' STATE_2 ,
    ' STATE_3 ,

ram
\ jumptable word to execute entry based on STATE
: do-STATE ( STATE -- )
    cells STATES + @
    execute
;

: button_1
    OFF blue
    OFF_speaker
    clr_pressed_1
    check_STATE STATE2LED
;

: button_2
    clr_pressed_2
    FSM_STATE @
    do-STATE
;

: fsm ( -- )
    clr_pressed_1
    clr_pressed_2
    init_STATE
    init_LEDs
    begin
        pressed_1 @ 
        if 
            button_1
        then
        pressed_2 @ 
        if 
            button_2
        then
    again
;

\ to start: init_T0_OV D2 pullup fsm
: init_fsm
    init_T0_OV
    D2 pullup
    D4 pullup
    fsm
;
\ ' init_fsm is turnkey
