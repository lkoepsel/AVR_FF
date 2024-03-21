: incr_times ( button -- )
    dup times @ 1+ swap times !
;

: .times ( button -- )
    dup . ." button pressed "
    times @ .
    ." times" cr
;


: button ( button -- )
    dup pressed_init
    dup incr_times
    .times
;

: btn_count ( -- )
    right pressed_init
    left pressed_init
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

\ to start: init_T0_OV D5 pullup btn_count
: count_buttons
    init_T0_OV
    D5 right init D6 left init
    btn_count
;
marker -end_buttons
\ ' buttons is turnkey
