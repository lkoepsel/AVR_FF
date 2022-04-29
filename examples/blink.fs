marker -blink
: blink ( ms -- ) 
	LED output 
	begin 
		LED toggle 
		dup ms 
	again
;
