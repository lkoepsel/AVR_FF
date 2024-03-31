\ Forthy debounce version
\ "fast" in that it hardcodes the pins for buttons
\ More Forth-like in that it makes assumptions and is implementation-specific

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

\ Timer 0 definitions from m328pdef.inc
#250 constant clock0_per \ clock period for comparison
$11 constant T0_OVF_VEC

\ button definitions
%1111.1100.0001.1111 constant BTN_MASK
%0000.0000.0001.1111 constant BTN_DOWN

2   constant button_count \ number of buttons    
D5  2constant right
D6  2constant left
ram
variable rt_history
variable rt_pressed
variable lt_history
variable lt_pressed
variable rt_times
variable lt_times

: rt_init 
    $ffff rt_history !
    0 rt_pressed !
    0 rt_times !
    right pullup
;

\ Begin the button check code

\ rt_state? determines if right button is down, (in pullup mode, so 0 is true)
: rt_state? ( -- f ) \ return True if button down
    right read if 0 else 1 then 
;

\ rt_store_state - stores the right button state in history
: rt_store_state ( f --  ) \ store button state in history
     rt_history @  2* or  rt_history !
;

\ down? determines if the down pattern counts as a press, returns true if so
: rt_down? ( -- f )
    rt_history @ 
    BTN_MASK and 
    BTN_DOWN = 
;

\  set_pressed uses down? to determined if pressed, if so, sets array pressed
\ and clears history
: rt_set_pressed ( -- )
    rt_down?
    if
        1 rt_pressed !
        $ffff rt_history !
    then
;

\ check_button is used by ISR to check for if button is pressed
: rt_check ( -- )
     rt_state? rt_store_state rt_set_pressed
;


\ Timer/Counter 0 - Debounce Clock (dbnce_clk) definitions
\ disable or enable clock
: dbnce_clk 1 timsk0 ;

\ Disable interrupt before removing the interrupt code
dbnce_clk disable

\ Debounce Clock ISR: Check button interrupt routine, runs every 8ms
: dbnce_clk_ISR
    rt_check
    \ left check_button

    \ (optional) used to confirm ISR execution rate
    \ if used be sure to adjust mask for same port bits being used for buttons
    \ mset uses a mask to set bits, so mask must include 
    \ bits used in same port, in this case: D5, D6 and D7
    \ BIT5 or BIT7 or ddrd tog
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
: rt_incr_times ( -- )
    rt_times @ 1+ rt_times !
;

\ display the number of times a button has been pressed
: .rt_times ( -- )
    ." right button pressed "
    rt_times @ .
    ." times" cr
;


\ when a button has been pressed, reset its pressed status
\ increment its times variable and print its times value
\ : button ( button -- )
\     dup pressed_init
\     dup incr_times
\     .times
\ ;

\ when a button has been pressed, reset its pressed status
\ increment its times variable and print its times value
: right_button ( -- )
    0 rt_pressed !
    rt_incr_times
    .rt_times
;

\ main application loop, continues to check for button presses
: btn_count ( -- )
    begin
        rt_pressed @ 
        if 
            right_button
        then
        \ left pressed @ 
        \ if 
        \     left button
        \ then
    again
;

\ application: count_buttons - initialize then count button presses
: count_buttons
    init_dbnce_clk
    rt_init 
    \ D6 left init
    \ blue out
    \ green out
    \ red out
    cr btn_count
;

\ test code: determine execution time required to check button
\ toggles pin for measuring execution time to determine button handling speed
\ pin set in routine, to reduce overhead due to measurement
\ 46us total time, 4.6us loop time => 41.5us check_button time
\ : time_button ( -- )
\     D7 out
\     D5 right init
\     right
\     begin
\         right check_button
\         D7 tog
\     again
\ ;

\ \ test timer: test a value to determine desired interrupt frequency
\ : test_timer  ( n -- )
\   \ Store the interrupt vector
\   ['] dbnce_clk_ISR T0_OVF_VEC int!

\   \ Activate T/C 0 for an interrupt, OCR0A value on stack
\   %0000.0001 tccr0a c!
\   %0000.1100 tccr0b c!
\   ocr0a c!

\   D7 out \ used to show ISR execution rate
\   \ Activate timer0 overflow interrupt
\   dbnce_clk enable
\ ;

marker -end_buttons
\ ' buttons is turnkey

