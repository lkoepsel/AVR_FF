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
variable rt_times

: rt_init 
    $ffff rt_history !
    0 rt_pressed !
    0 rt_times !
    right pullup
;

\ Begin the button check code

\ read input register PIND
: pind@ ( -- c )
  \ Make room in the stack top registers. 
  \ Or just use DUP, it inlines the same code automatically.
  [ R25 -Y st,  ]
  [ R24 -Y st,  ]
  [ R24 $9 inn, ]  \ put the low byte on the stack
  [ R25 clr,    ]  \ clear the high byte
; 

\ in_D5, test reading pin D5
: in_D5 ( -- f ) pind@ BIT5 and ;

\ rt_state? determines if right button is down, (in pullup mode, so 0 is true)
: rt_state? ( -- f ) \ return True if button down
    in_D5 if 0 else 1 then 
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
: rt_chk_pressed ( -- )
    rt_down?
    if
        1 rt_pressed !
        $ffff rt_history !
    then
;

\ rt_check is used by ISR to check for if button is pressed
\ measured execution time is ~16us
: rt_check ( -- )
     rt_state? rt_store_state rt_chk_pressed
;


\ Timer/Counter 0 - Debounce Clock (dbnce_clk) definitions
\ disable or enable clock
: dbnce_clk 1 timsk0 ;

\ Disable interrupt before removing the interrupt code
dbnce_clk disable

\ Debounce Clock ISR: Check button interrupt routine, runs every 8ms
: dbnce_clk_ISR
    rt_check
;i


\ initialize Debounce Clock interrupt
: init_dbnce_clk  ( OCR0A -- )
  \ Store the interrupt vector
  ['] dbnce_clk_ISR T0_OVF_VEC int!

  \ Activate T/C 0 for an 8ms interrupt
  %0000.0001 tccr0a c!
  %0000.1100 tccr0b c!
  clock0_per ocr0a c!

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

\ when right button has been pressed, reset its pressed status
\ increment its times variable and print its times value
: rt_btn_pressed ( -- )
    0 rt_pressed !
    rt_incr_times
    .rt_times
;

\ main application loop, continues to check for button presses
: btn_count ( -- )
    begin
        rt_pressed @ 
        if 
            rt_btn_pressed
        then
    again
;

\ application: count_buttons - initialize then count button presses
: count_buttons
    init_dbnce_clk
    rt_init 
    cr btn_count
;

\ test code: determine execution time required to check button
\ toggles pin for measuring execution time to determine button handling speed
\ pin set in routine, to reduce overhead due to measurement
\ 16.9us total time, negligible loop time (250ns) for rt_check (ISR routine)
: time_button ( -- )
    D8 out
    rt_init
    begin
        [ pinb-io #0 sbi, ] \ one toggle per loop
        rt_check
    again
;

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

