\ blink built-in LED, requires reset to stop
: blink ( ms -- ) 
	LED output 
	begin 
		LED toggle 
		dup ms 
	again
;

\ blink a pin (pin as in Dnn), requires reset to stop
: plink ( ms pin -- ) 
	2dup output 
	begin 
		2dup toggle 
		rot dup ms rot rot
	again
;

\ blink? built-in LED, quit on key press
: blink? ( ms -- ) 
	LED output 
	begin 
		LED toggle 
		dup ms 
		key?

	until
	drop
;

\ blink a pin (pin as in Dnn), quit on key press
: plink? ( ms pin -- ) 
	2dup output 
	begin 
		2dup toggle 
		rot dup ms rot rot
		key?
	until
	drop drop drop
;
