marker -blink
: blink ( ms -- ) 
	LED output
	begin 
		LED toggle 
		ms 
	again
;
