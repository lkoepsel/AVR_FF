\ used to setup tasks, sets pins to output
: setup ( -- )
    D13 out
;

\ individual tasks to run, toggles the pin
: task_13 ( -- )
	D13 tog
;

\ run all tasks including setup to measure execution speed
: alltasks ( -- )
	setup
	begin
		task_13
	again
;

\ used to setup tasks, sets pins to output
: setup_8 ( -- )
    D2 out
    D3 out
    D4 out
    D5 out
    D6 out
    D7 out
    D8 out
    D13 out
;

\ used to initialize pins to known (low) state
: init_8 ( -- )
    D2 low
    D3 low
    D4 low
    D5 low
    D6 low
    D7 low
    D8 low
    D13 low
;

\ individual tasks to run, toggles the pin
: task_2 ( -- )
	D2 tog
;

\ individual tasks to run, toggles the pin
: task_3 ( -- )
	D3 tog
;

\ individual tasks to run, toggles the pin
: task_4 ( -- )
	D4 tog
;

\ individual tasks to run, toggles the pin
: task_5 ( -- )
	D5 tog
;

\ individual tasks to run, toggles the pin
: task_6 ( -- )
	D6 tog
;

\ individual tasks to run, toggles the pin
: task_7 ( -- )
	D7 tog
;

\ individual tasks to run, toggles the pin
: task_8 ( -- )
	D8 tog
;

: task_2a [ pind-io #2 sbi, ] ; 
: task_3a [ pind-io #3 sbi, ] ; 
: task_4a [ pind-io #4 sbi, ] ; 
: task_5a [ pind-io #5 sbi, ] ; 
: task_6a [ pind-io #6 sbi, ] ; 
: task_7a [ pind-io #7 sbi, ] ; 
: task_8a [ pinb-io #0 sbi, ] ; 
: task_13a [ pinb-io #5 sbi, ] ; 

: task_2ai [ pind-io #2 sbi, ] ; inlined
: task_3ai [ pind-io #3 sbi, ] ; inlined 
: task_4ai [ pind-io #4 sbi, ] ; inlined
: task_5ai [ pind-io #5 sbi, ] ; inlined
: task_6ai [ pind-io #6 sbi, ] ; inlined
: task_7ai [ pind-io #7 sbi, ] ; inlined
: task_8ai [ pinb-io #0 sbi, ] ; inlined
: task_13ai [ pinb-io #5 sbi, ] ; inlined


\ run all tasks including setup to measure execution speed
: alltasks_8 ( -- )
	setup_8
	init_8
	begin
		task_2
		task_3
		task_4
		task_5
		task_6
		task_7
		task_8
		task_13
	again
;

\ run all tasks including setup to measure execution speed
: alltasks_8a ( -- )
	setup_8
	init_8
	begin
		task_2a
		task_3a
		task_4a
		task_5a
		task_6a
		task_7a
		task_8a
		task_13a
	again
;

\ run all tasks including setup to measure execution speed
: alltasks_8ai ( -- )
	setup_8
	init_8
	begin
		task_2ai
		task_3ai
		task_4ai
		task_5ai
		task_6ai
		task_7ai
		task_8ai
		task_13ai
	again
;
