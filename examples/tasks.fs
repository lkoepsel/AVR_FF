\ Requires Library/328P_ports
\ Testing execution speed with round-robin tasking
\ See https://wellys.com/posts/board-language_speed/
\ tasks execute @ 26.512kHz, every 37.7us
-tasks
marker -tasks

\ 1. Setup pins as out
: setup
	D2 out
	D3 out
	D4 out
	D5 out
	D6 out
	D7 out
	D8 out
	D9 out
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
