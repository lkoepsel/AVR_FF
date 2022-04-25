-button
marker -button
: button ( pin -- )
	read 
	if ." Not Pressed!" 
	else ." Pressed" 
	then 
;
