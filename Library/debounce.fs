\ Timer 0 - Used for debouncing buttons
\ Button debounce code based on Elliot Williams article
\ https://hackaday.com/2015/12/10/embed-with-elliot
\ -debounce-your-noisy-buttons-part-ii
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
