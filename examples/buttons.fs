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

: btn_count ( -- )
    clr_pressed_1
    clr_pressed_2
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

\ to start: init_T0_OV D2 pullup btn_count
: buttons
    init_T0_OV
    D2 pullup D4 pullup
    btn_count
;
marker -end_buttons
\ ' buttons is turnkey
