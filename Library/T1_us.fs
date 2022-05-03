\ Initialize timer 1 as a free running clock at 16MHz
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ] = 00000000
\ WGM13 WGM12 WGM11 WGM10 => Normal, TOP = 0xFFFF
\ TCCR1B [ ICNC1 ICES1 0 WGM13 CS12 CS12 CS11 CS10 ] = 00000001
\ CS10 => scalar of 1
\ tick = 1/(16MHz) = 62.5ns or 62.5 x 10^-9s
\ Test using example/millis (delay(1)) = 16020 ticks
\ (16.020x10^3 ticks)x(62.5x10^-9 secs/tick) = 1.00125 x 10^-3 seconds
\

\ Disable interrupt before removing the interrupt code
-T1_ctc
marker -T1_ctc
\ Timer 2 definitions from m328pdef.inc
$80 constant TCCR1A
$81 constant TCCR1B
$84 constant TCNT1L
$85 constant TCNT1H

\ initialize T/C 1 to free running 16MHz clock
: init_T1
  0 TCCR1A c!
  1 TCCR1B c!
;

-check
marker -check

\ ms counter used to check accuracy, 100 check_ms will display a delta of 100
: check_T1 ( ms -- ) \ ms delay to use for checking ms_count
  begin
    TCNT1L @ u.
    dup ms
  again
;

\ to initialize T/C 2 for ms interrupt use: init_T2_OV
\ to test for milliseconds, 100 check_ms, delta is 100
\ to test for bounce counter 80 check_b, delta is 10
