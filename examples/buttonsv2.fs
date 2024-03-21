: incr_times ( button -- )
    dup times @ 1+ swap times !
;

: .times ( button -- )
    dup . cr ." button pressed "
    times @ .
    ." times"
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


\ words which run when button is pressed
: incr_count_1
    1 count_1 +!
; \ increment button counter

: .count_1
    cr ." button 1 pressed "
    count_1 @ .
    ." times"
;

: button_1
    clr_pressed_1
    incr_count_1
    .count_1
;

: incr_count_2
    1 count_2 +!
; \ increment button counter

: .count_2
    cr ." button 2 pressed "
    count_2 @ .
    ." times"
;

: button_2
    clr_pressed_2
    incr_count_2
    .count_2
;

\ : btn_count ( -- )
\     clr_pressed_1
\     clr_pressed_2
\     begin
\         pressed_1 @ 
\         if 
\             button_1
\         then
\         pressed_2 @ 
\         if 
\             button_2
\         then
\     again
\ ;

\ to start: init_T0_OV D5 pullup btn_count
: count_buttons
    init_T0_OV
    \ D5 pullup D6 pullup
    D5 right init D6 left init
    btn_count
;
marker -end_buttons
\ ' buttons is turnkey
