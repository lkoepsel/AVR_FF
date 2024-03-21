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
\ Every interrupt incr by 1 and toggles D6, 2 toggles = 1 period
\ Oscilloscope shows D6 toggles every 8.01ms
\ See examples/button.fs for how to use

\ Timer 0 definitions from m328pdef.inc
#250 constant clock0_per \ clock period for comparison
$11 constant T0_OVF_VEC

\ button definitions
%1111.1100.0001.1111 constant BTN_MASK
%0000.0000.0001.1111 constant BTN_DOWN

2   constant button_count \ number of buttons    
0   constant right
1   constant left
\ D5  2constant right_button
\ D6  2constant left_button

: array: ( n "name" -- )
  create cells allot 
  does> swap cells + 
;

: 2array: ( n "name" -- )
  create 2* cells allot 
  does> swap 2* cells + 
;

ram button_count array: history
: history_init ( button -- ) history $ffff swap ! ;
ram button_count array: pressed
: pressed_init ( button -- ) pressed $0 swap ! ;
: pressed_true ( button -- ) pressed $ffff swap ! ;
ram button_count array: times
: times_init ( button -- ) times $0000 swap ! ;
ram button_count 2array: button_pin
: pin_init ( pin button -- ) button_pin  2! ;

: init ( pin button -- )
    dup history_init
    dup pressed_init
    dup times_init
    rot rot 2dup pullup
    rot pin_init
;

: down? ( button -- f ) \ return True if button down
    button_pin 2@ read if 0 else 1 then 
;

: str_history ( button f -- button ) \ store button state in history
    over history @  2* or  over history !
;

: button_down? ( button -- button f )
    dup
    history @ 
    BTN_MASK and 
    BTN_DOWN = 
;

: pressed? ( button -- )
    button_down?
    if
        dup
        pressed_true
        history_init
        ." DOWN!"
    else
        drop
    then
;

: check_btn ( button -- )
\    dup down? str_history pressed_1?
     dup down? str_history pressed?
;

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
        ." Down 1! "
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
  1 timsk0 mclr
;

\ Disable interrupt before removing the interrupt code
dis_T0_OVF

\ The interrupt routine
\ mset uses a mask to set bits, the mask must include 
\ all bits, in this case: D5, D6 and D6
: T0_OVF_ISR
  right check_btn
  left check_btn

  \ (optional) used to show update rate
  BIT5 BIT6 or BIT7 or ddrd tog
;i


\ initialize T/C 0 Overflow interrupt
: init_T0_OV  ( OCR0A -- )
  \ Store the interrupt vector
  ['] T0_OVF_ISR T0_OVF_VEC int!

  \ Activate T/C 0 for a 1ms interrupt
  %0000.0001 tccr0a c!
  %0000.1100 tccr0b c!
  clock0_per ocr0a c!

  D7 out \ (optional) used to show update rate
  \ Activate timer0 overflow interrupt
  1 timsk0 mset
;

-end_debounce
marker -end_debounce
