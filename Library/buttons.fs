-button
marker -button

: btn_count ( -- )
    init_pressed_1
    init_pressed_2
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
: start init_T0_OV D2 pullup D4 pullup btn_count ;
' start is turnkey
