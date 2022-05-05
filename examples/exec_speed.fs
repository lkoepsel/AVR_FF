\ Testing execution speed with round-robin tasking
\ See https://wellys.com/posts/board-language_speed/

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
: task0 D2 toggle ;
: task1 D3 toggle ;
: task2 D4 toggle ;
: task3 D5 toggle ;
: task4 D6 toggle ;
: task5 D7 toggle ;
: task6 D8 toggle ;
: task7 D9 toggle ;

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
