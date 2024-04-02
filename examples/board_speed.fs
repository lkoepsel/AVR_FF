\ Testing execution speed with round-robin tasking
$0003 constant pinb-io  \ IO-space address
$0009 constant pind-io  \ IO-space address

\ 1. Setup pins as output
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
: task0 [ pind-io #2 sbi, ] ;
: task1 [ pind-io #3 sbi, ] ;
: task2 [ pind-io #4 sbi, ] ;
: task3 [ pind-io #5 sbi, ] ;
: task4 [ pind-io #6 sbi, ] ;
: task5 [ pind-io #7 sbi, ] ;
: task6 [ pinb-io #0 sbi, ] ;
: task7 [ pinb-io #1 sbi, ] ;

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

