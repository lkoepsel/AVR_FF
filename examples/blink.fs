\ blink built-in LED
: blink ( ms -- ) 
	LED output 
	begin 
		LED toggle 
		dup ms 
	again
;

\ blink a pin
: plink ( ms pin -- ) 
	2dup output 
	begin 
		2dup toggle 
		rot dup ms rot rot
	again
;
