\ blink built-in LED, requires reset to stop
: blink ( ms -- ) 
    LED out 
    begin 
        LED tog
        dup ms 
    again
;

\ blink? built-in LED, quit on key press
: blink? ( ms -- ) 
    LED out
    begin 
        LED tog
        dup ms 
        key?
    until
    drop
;

\ blink a pin (pin as in Dnn), requires reset to stop
: plink ( ms pin -- ) 
    2dup out 
    begin 
        2dup tog
        rot dup ms rot rot
    again
;

\ blink a pin (pin as in Dnn), quit on key press
: plink? ( ms pin -- ) 
    2dup out 
    begin 
        2dup tog
        rot dup ms rot rot
        key?
    until
    drop drop drop
;
