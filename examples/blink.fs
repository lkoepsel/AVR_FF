marker -blink
: blink5 ( -- ) 
	begin 
		LED output LED toggle 
		500 ms 
	again
;
: blink10 ( -- ) 
	begin 
		LED output LED toggle 
		1000 ms 
	again
;
