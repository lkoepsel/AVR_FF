\ Testing execution speed with round-robin tasking
\ See https://wellys.com/posts/board-language_speed/
\ tasks execute @ 26.512kHz, every 37.72us
-tasks
marker -tasks

\ 1. Setup pins as output
: setup
	D2 output
	D3 output
	D4 output
	D5 output
	D6 output
	D7 output
	D8 output
	D9 output
;

\ 2. Define 8 tasks, each to toggle a specific pin
: task0 D2 tog ;
: task1 D3 tog ;
: task2 D4 tog ;
: task3 D5 tog ;
: task4 D6 tog ;
: task5 D7 tog ;
: task6 D8 tog ;
: task7 D9 tog ;

\ 3. Cycle through each task

: alltasks 
	setup 
		begin
			task0
			task1
			task2
			task3
			task4
			task5
			task6
			task7
		again
;
