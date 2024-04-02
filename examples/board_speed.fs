\ Testing execution speed with round-robin tasking
\ Requires 328P_ports and asm

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

\ inlined for maximum performance
: task0_i [ pind-io #2 sbi, ] ; inlined
: task1_i [ pind-io #3 sbi, ] ; inlined
: task2_i [ pind-io #4 sbi, ] ; inlined
: task3_i [ pind-io #5 sbi, ] ; inlined
: task4_i [ pind-io #6 sbi, ] ; inlined
: task5_i [ pind-io #7 sbi, ] ; inlined
: task6_i [ pinb-io #0 sbi, ] ; inlined
: task7_i [ pinb-io #1 sbi, ] ; inlined

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

\ inlined for maximum performance
: alltasks_i
    setup 
        begin
            task0_i
            task1_i
            task2_i
            task3_i
            task4_i
            task5_i
            task6_i
            task7_i
        again
;

: onetask 
    setup 
        begin
            task6
        again
;

