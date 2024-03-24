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

\ creates an indexed cell array as in "n name" with n as the index
: array: ( n "name" -- )
  create cells allot 
  does> swap cells + 
;

\ creates an indexed 2cell array as in "n name" with n as the index
: 2array: ( n "name" -- )
  create 2* cells allot 
  does> swap 2* cells + 
;

\ history tracks the state history of the button
ram button_count array: history
: history_init ( button -- ) history $ffff swap ! ;

\ pressed indicates if the button has been pressed, initialized to 0 (false)
ram button_count array: pressed
: pressed_init ( button -- ) pressed $0 swap ! ;
: pressed_true ( button -- ) pressed $ffff swap ! ;

\ times tracks the number of times the button has been pressed
ram button_count array: times
: times_init ( button -- ) times $0000 swap ! ;

\ button_pin attaches a pin to a button, use the Arduino pin references
ram button_count 2array: button_pin
: pin_init ( pin button -- ) button_pin  2! ;

\ ties pin to button and initializes all arrays associated with button
: init ( pin button -- )
    dup history_init
    dup pressed_init
    dup times_init
    rot rot 2dup pullup
    rot pin_init
;

\ Begin the button check code

\ state? determines if button is down, (in pullup mode, so 0 is true)
: state? ( button -- f ) \ return True if button down
    button_pin 2@ read if 0 else 1 then 
;

\ store_state - stores the current button state in history
: store_state ( button f -- button ) \ store button state in history
    over history @  2* or  over history !
;

\ down? determines if the down pattern counts as a press, returns true if so
: down? ( button -- button f )
    dup
    history @ 
    BTN_MASK and 
    BTN_DOWN = 
;

\  set_pressed uses down? to determined if pressed, if so, sets array pressed
\ and clears history
: set_pressed ( button -- )
    down?
    if
        dup
        pressed_true
        history_init
    else
        drop
    then
;

\ check_button is used by ISR to check for if button is pressed
: check_button ( button -- )
     dup state? store_state set_pressed
;


\ Timer/Counter 0 - Debounce Clock (dbnce_clk) definitions
\ disable or enable clock
: dbnce_clk 1 timsk0 ;

\ Disable interrupt before removing the interrupt code
dbnce_clk disable

\ Debounce Clock ISR: Check button interrupt routine, runs every 8ms
: dbnce_clk_ISR
    right check_button
    left check_button

    \ (optional) used to confirm ISR execution rate
    \ if used be sure to adjust mask for same port bits being used for buttons
    \ mset uses a mask to set bits, so mask must include 
    \ bits used in same port, in this case: D5, D6 and D7
    BIT5 BIT6 or BIT7 or ddrd tog
;i


\ initialize Debounce Clock interrupt
: init_dbnce_clk  ( OCR0A -- )
  \ Store the interrupt vector
  ['] dbnce_clk_ISR T0_OVF_VEC int!

  \ Activate T/C 0 for an 8ms interrupt
  %0000.0001 tccr0a c!
  %0000.1100 tccr0b c!
  clock0_per ocr0a c!

  D7 out \ (optional) used to show ISR execution rate
  \ Activate Debounce Clock interrupt
  dbnce_clk enable
;

-end_debounce
marker -end_debounce

\ Application code, in this case tracks number of times a button has been pressed

\ when button has been pressed, increment the variable times
: incr_times ( button -- )
    dup times @ 1+ swap times !
;

\ display the number of times a button has been pressed
: .times ( button -- )
    dup . ." button pressed "
    times @ .
    ." times" cr
;


\ when a button has been pressed, reset its pressed status
\ increment its times variable and print its times value
: button ( button -- )
    dup pressed_init
    dup incr_times
    .times
;

\ main application loop, continues to check for button presses
: btn_count ( -- )
    begin
        right pressed @ 
        if 
            right button
        then
        left pressed @ 
        if 
            left button
        then
    again
;

\ application: count_buttons - initialize then count button presses
: count_buttons
    init_dbnce_clk
    D5 right init 
    D6 left init
    cr btn_count
;

\ test code: determine execution time required to check button
\ toggles pin for measuring execution time to determine button handling speed
\ pin set in routine, to reduce overhead due to measurement
\ 46us total time, 4.6us loop time => 41.5us check_button time
: time_button ( -- )
    D7 out
    D5 right init
    right
    begin
        right check_button
        D7 tog
    again
;

\ test timer: test a value to determine desired interrupt frequency
: test_timer  ( n -- )
  \ Store the interrupt vector
  ['] dbnce_clk_ISR T0_OVF_VEC int!

  \ Activate T/C 0 for an interrupt, OCR0A value on stack
  %0000.0001 tccr0a c!
  %0000.1100 tccr0b c!
  ocr0a c!

  D7 out \ used to show ISR execution rate
  \ Activate timer0 overflow interrupt
  dbnce_clk enable
;

marker -end_buttons
\ ' buttons is turnkey

