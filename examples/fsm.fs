-fsm
marker -fsm

\ finite STATE machine using two buttons
\ button 1 - advances STATE and lights STATE LEDs
\ button 2 - enters STATE (executes STATE) and lights blue LED
\ LED_0 - bit 0 of STATE LEDs
\ LED_1 - bit 1 of STATE LEDs
\ Blue LED - indicates state entered by intensity (off/DIM/MED/HI)
\ and executing state
\ Speaker - Ultrasonic transducer, 4 states (Off, Audible, 22kHz, 44kHz)
\ Timer 0 - Debounce the buttons (check buttons every 8ms)
\ Timer 1 - CTC PWM for ultrasonic transducer
\ Timer 2 - Fast PWM to vary the intensity of the Blue LED


variable STATE
D8       2constant LED_0
D9       2constant LED_1
D10      2constant LED_BLUE
0        constant OFF
31       constant DIM
100      constant MED
255      constant HI
510      constant AUDIBLE   \ 2kHz
44       constant 22K       \ 22kHz
24       constant 40K       \ 44kHz


\ words which run when button is pressed
: init_STATE 0 STATE ! ;
: inc_STATE 1 STATE +! ; \ increment STATE

\ STATE ranges from 0-3, at 3 it goes back to 0
: check_STATE ( -- ) 
    STATE @ 3 =
    if init_STATE
    else inc_STATE
    then
;

: BLUE_OFF OFF init_T2 ;
: BLUE_DIM DIM init_T2 ;
: BLUE_MED MED init_T2 ;
: BLUE_HI HI init_T2 ;

: SPKR_OFF D10 input ;
: SPKR_AUD AUDIBLE CTC ;
: SPKR_ULTRA_1 22K CTC ;
: SPKR_ULTRA_2 40K CTC ;

\ states to execute
: STATE_0 BLUE_OFF SPKR_OFF ;
: STATE_1 BLUE_DIM SPKR_AUD ;
: STATE_2 BLUE_MED SPKR_ULTRA_1 ;
: STATE_3 BLUE_HI SPKR_ULTRA_2 ;

\ store jumptable in flash for STATES
flash
create STATES ' STATE_0 , ' STATE_1 , ' STATE_2 , ' STATE_3 , 

\ jumptable word to execute entry based on STATE
: do-STATE ( STATE -- )
    cells STATES + @ execute ;

\ converts STATE to LED, made easy by using sequential pins
: STATE2LED ( -- )
    %00000011 PORTB mclr STATE @ PORTB mset ;
: .STATE cr ." STATE " STATE @ . ;
: incr_count_2 1 count_2 +! ; \ increment button counter
: .count_2 cr ." button 2 pressed " count_2 @ . ." times" ;

: button_1 SPKR_OFF BLUE_OFF clr_pressed_1 check_STATE STATE2LED ;
: button_2 clr_pressed_2 STATE @ do-STATE ;

: init_LEDs LED_0 output LED_1 output LED_BLUE output ;

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
: init_fsm init_T0_OV D2 pullup D4 pullup fsm ;
' init_fsm is turnkey
