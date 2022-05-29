\ HAL: provides constants for LEDs and Buttons
\ Ports, DDRx, PORTx and PINx, page 624 datasheet
$23 constant PINB  \ Port B input register (-1 for input port)
$24 constant DDRB  \ Port B data direction register (port reference)
$25 constant PORTB \ Port B output register (+1 for output port)
$26 constant PINC  \ Port C input register
$27 constant DDRC  \ Port C data direction register
$28 constant PORTC \ Port C output register
$29 constant PIND  \ Port D input register
$2a constant DDRD  \ Port D data direction register
$2b constant PORTD \ Port D output register

%10000000 constant BIT7   \ bit 07  
%01000000 constant BIT6   \ bit 06  
%00100000 constant BIT5   \ bit 05  
%00010000 constant BIT4   \ bit 04  
%00001000 constant BIT3   \ bit 03  
%00000100 constant BIT2   \ bit 02  
%00000010 constant BIT1   \ bit 01  
%00000001 constant BIT0   \ bit 00  

\ Arduino Board Pins, reference using Board Pins, not AVR registers if possible
\ Pins referenced by nn, where nn is the Arduino board pin number
BIT5 DDRC 2constant A5 \ Board Connector A5 PC5
BIT4 DDRC 2constant A4 \ Board Connector A4 PC4
BIT3 DDRC 2constant A3 \ Board Connector A3 PC3
BIT2 DDRC 2constant A2 \ Board Connector A2 PC2
BIT1 DDRC 2constant A1 \ Board Connector A1 PC1
BIT0 DDRC 2constant A0 \ Board Connector A0 PC0
BIT5 DDRB 2constant LED \ Board Connector 13 PB5
BIT5 DDRB 2constant D13 \ Board Connector 13 PB5
BIT4 DDRB 2constant D12 \ Board Connector 12 PB4
BIT3 DDRB 2constant D11 \ Board Connector 11 PB3 PWM OC2A
BIT2 DDRB 2constant D10 \ Board Connector 10 PB2 PWM OC1B
BIT1 DDRB 2constant D9  \ Board Connector  9 PB1 PWM OC1A
BIT0 DDRB 2constant D8  \ Board Connector  8 PB0 
BIT7 DDRD 2constant D7  \ Board Connector  7 PD7 
BIT6 DDRD 2constant D6  \ Board Connector  6 PD6 PWM OC0A
BIT5 DDRD 2constant D5  \ Board Connector  5 PD5 PWM OC0B
BIT4 DDRD 2constant D4  \ Board Connector  4 PD4 
BIT3 DDRD 2constant D3  \ Board Connector  3 PD3 PWM OC2B
BIT2 DDRD 2constant D2  \ Board Connector  2 PD2 
BIT1 DDRD 2constant D1  \ Board Connector  1 PD1 
BIT0 DDRD 2constant D0  \ Board Connector  0 PD0

\ microseconds delay for Atmega
marker -us

\ Opcode only to flash
: op: ( opcode -- ) flash create , ram does> @ i, ;

\ Atmega wdr instruction
$95a8 op: wdr,

\ Clear watchdog (wdr instruction) takes one clock cycle (62.5ns @ 16MHz)
\ Adjust the number of CWD to achieve a one us delay
\ 9 CWD is needed @ 16MHz for ATmega 328 and 2560.
: us ( u -- ) \ blocking wait for u microseconds
  begin
    cwd cwd  cwd cwd cwd cwd cwd cwd cwd
    1- dup 
  0= until
  drop 
;

: .mem_left ( ---)
    cr ."  flash: "   flash  unused u.
    cr ." eeprom: "   eeprom unused u.
    cr ."    ram: "   ram    unused u. 
    cr ;


\ PRIM: primitives for initializing port and set high or low output
: high ( bit port -- ) 1 + mset ;  \ set a pin high
: low ( bit port -- ) 1 + mclr ;  \ set a pin low
: toggle ( bit port -- ) 1 - mset ; \ toggle output value
: output ( bit port -- ) mset ;  \ set pin as output
: input ( bit port -- ) mclr ;  \ set a pin as input
: pullup ( bit port -- ) 2dup input high ; \ set pin as input_pullup
: read ( bit port -- f ) 1 - c@ and ; \ read a pin, returns bit value

: falkey false is turnkey ; \ use to reset turnkey vector

-end_HAL
marker -end_HAL

\ Timer 0 - Used for debouncing buttons
\ Button debounce code based on Elliot Williams article
\ https://hackaday.com/2015/12/10/embed-with-elliot-debounce-your-noisy-buttons-part-ii
\ Initialize Timer/Counter 0 to be a 2 button debouncer @125Hz
\ Counter increments every 8ms
\ TCCR0A [ COM0A1 COM0A0 COM0B1 COM0B0 0 0 WGM01 WGM00 ] = 00000001
\ TCCR0B [ FOC0A FOC0B 0 0 WGM02 CS02 CS01 CS00 ] = 00001100
\ WGM02 WGM00 => Mode 5, PWM, Phase Correct, TOP = OCRA
\ CS02 => scalar of 256
\ OCR0A = 250 ($fa)
\ Frequency = 16 x 10^6 / 256 / 250 = 250Hz
\ Counter performs another divide by 2 => 125 count/sec
\ Every interrupt incr by 1 and toggles D5, 2 toggles = 1 period
\ Oscilloscope shows D5 toggles every 8.01ms
\ See examples/button.fs for how to use

-T0_int
marker -T0_int

\ Timer 0 definitions from m328pdef.inc
$44 constant TCCR0A
$45 constant TCCR0B
$47 constant OCR0A
$6e constant TIMSK0
$11 constant T0_OVF_VEC
#250 constant clock0_per \ clock period for comparison

\ button definitions
%1111.1100.0001.1111 constant BTN_MASK
%0000.0000.0001.1111 constant BTN_DOWN

\ button 1 definitions
variable history_1
variable pressed_1
variable count_1

: init_history_1 $ffff history_1 ! ; ( -- ) \ init presses register
: pressed_true_1 $ffff pressed_1 ! ; ( -- ) \ store true in pressed

: down_1? ( pin -- f ) \ return True if button down
    read if 0 else 1 then 
;

: str_history_1 ( f -- ) \ store button state in history
    history_1 @ 2* or history_1 !
;

: btn_pressed_1? 
    history_1 @ BTN_MASK 
    and BTN_DOWN = 
;

: pressed_1? ( -- )
    btn_pressed_1?
    if
        pressed_true_1
        init_history_1
    then
;

: check_btn_1 ( pin -- )
    down_1? str_history_1 pressed_1?
;

: clr_pressed_1 0 pressed_1 ! ; \ initialize pressed to False
\ end button 1 definitions

\ button 2 definitions
variable history_2
variable pressed_2
variable count_2

: init_history_2 $ffff history_2 ! ; ( -- ) \ init presses register
: pressed_true_2 $ffff pressed_2 ! ; ( -- ) \ store true in pressed

: down_2? ( pin -- f ) \ return True if button down
    read 
    if 0
    else 1
    then 
;

: str_history_2 ( f -- ) \ store button state in history
    history_2 @ 2* or history_2 !
;

: btn_pressed_2? 
    history_2 @ BTN_MASK 
    and BTN_DOWN = 
;

: pressed_2? ( -- )
    btn_pressed_2?
    if
        pressed_true_2
        init_history_2
    then
;

: check_btn_2 ( pin -- )
    down_2? str_history_2 pressed_2?
;

: clr_pressed_2 0 pressed_2 ! ; \ initialize pressed to False
\ end button 2 definitions

\ T0 definitions
\ disable T/C 0 Overflow interrupt
: dis_T0_OVF
  1 TIMSK0 mclr
;

\ Disable interrupt before removing the interrupt code
dis_T0_OVF

\ The interrupt routine
\ mset uses a mask to set bits, the mask must include 
\ all bits, in this case: D2, D4 and D5
: T0_OVF_ISR
  D2 check_btn_1
  D4 check_btn_2

  \ (optional) used to show update rate
  BIT2 BIT4 or BIT5 or DDRD toggle
;i


\ initialize T/C 0 Overflow interrupt
: init_T0_OV  ( OCR0A -- )
  \ Store the interrupt vector
  ['] T0_OVF_ISR T0_OVF_VEC int!

  \ Activate T/C 0 for a 1ms interrupt
  %0000.0001 TCCR0A c!
  %0000.1100 TCCR0B c!
  clock0_per OCR0A c!

  D5 output \ (optional) used to show update rate
  \ Activate timer0 overflow interrupt
  1 TIMSK0 mset
;
\ Initialize timer 1 to a Clear Timer on Compare Match (CTC) pin 10
\ Timer 1 definitions pgs 140-166 DS40002061B
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ CTC Mode 4: WGM3:0 = 0100 N=8 (scalar)
\ OC1A/B: COM1B1:0 = 0001 Toggle on Compare Match, Top = OCR1A

-T1_ctc
marker -T1_ctc
\ Timer 1 definitions pgs 140-166 DS40002061B
$80 constant TCCR1A \ T/C 1 Control Register A
$81 constant TCCR1B \ T/C 1 Control Register B
$88 constant OCR1AL \ Output Compare Register Low byte
$89 constant OCR1AH \ Output Compare Register High byte

\ initialize T/C 1 B ONLY as CTC, Top = OCR1A
\ top determines the freq as in freq = 16MHz/(2*N*(1+top))
\ 31 => 16MHz/(2*8*32) = 31.25kHz, 1023 => 976Hz
: CTC ( top -- )
  OCR1AL ! \ freq based on OCR1A
  %00010000 TCCR1A c! \ COM1B1
  %00001010 TCCR1B c! \ WGM12 scalar
  D10 output
;

\ Initialize Timer/Counter 2 to Fast PWM 
\ Use to vary intensity of an LED
\ Mode 3, Fast PWM, Top=OCRA, Clear OC2A on Compare Match
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 10000011
\ WGM22 WGM21 WGM20 => Fast PWM, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00000010
\ OCR2A = 127
\ CS21 => scalar of 8
\ Frequency = 7.8kHz, duty cycle/255 on stack
\ 

-T2
marker -T2

\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A
#127 constant clock2_per \ clock period for 50% duty

\ initialize Timer/Counter 2 to Fast PWM, top = OCRA
: init_T2  ( dc/255 -- )

  \ Activate T/C 2 for Fast PWM
  OCR2A c!
  %10000011 TCCR2A c!
  %00000010 TCCR2B c!
  D11 output
;
