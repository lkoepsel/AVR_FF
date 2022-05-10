\ Initialize Timer/Counter 2 to a 2.0024ms interrupt timer
\ Counter increments every 1.0012ms
\ Add a debounce counter which divides by B_DIVIDER
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 00000001
\ WGM22 WGM20 => PWM, Phase Correct, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00001011
\ OCR2A = 125
\ CS22 CS20 => scalar of 32
\ Frequency = 16 x 10^6 / 32 / 250 = 2000Hz
\ Counter performs another divide by 2 => 998.8Hz count freq
\ Every interrupt incr by 1 and toggles D5, 2 toggles = 1 period
\ Oscilloscope shows D5 toggles every 1.0012ms
\ Oscilloscope shows D3 toggles every  8.0096ms 

-T2_int
marker -T2_int

\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A
$70 constant TIMSK2
#10 constant T2_OVF_VEC
#250 constant ms_value

#8 constant B_DIVIDER

\ disable T/C 2 Overflow interrupt
: dis_T2_OVF
  1 TIMSK2 mclr
;

\ Disable interrupt before removing the interrupt code
dis_T2_OVF

\ Counters for timer overflows
variable ms_count      \ counter for milliseconds
variable B_count       \ counter for scaled ticks
variable B_scalar      \ scalar for bounce divider

: init_bounce B_DIVIDER B_scalar ! ;

\ bounce scalar counter - divides T/C 2 clock by B_DIVIDER
: bounce 
    B_scalar @ 1 - dup 0=
    if drop 1 B_count +! D3 toggle init_bounce
    else B_scalar !
    then
;

\ The interrupt routine
: T2_OVF_ISR
  1 ms_count +!
  D5 toggle
  bounce
;i


\ initialize T/C 2 Overflow interrupt
: init_T2_OV  ( OCR2A -- )
  \ Store the interrupt vector
  ['] T2_OVF_ISR T2_OVF_VEC int!

  \ Activate T/C 2 for a 1ms interrupt
  1 TCCR2A c!
  $0b TCCR2B c!
  ms_value OCR2A c!

  \ initialize bounce clock scalar
  init_bounce
  D3 output
  D5 output
  \ Activate timer2 overflow interrupt
  1 TIMSK2 mset
;

\ ms counter used to check accuracy, 
\ 100 check_ms will display a delta of 102 (2 ms overhead)
: check_ms ( n -- ) \ n ms delay
  begin
    ms_count @ u.
    dup ms
  again
;

\ bs counter used to check accuracy
\ 80 check_b will display a delta of 10
: check_b ( n -- ) \ n ms delay
  begin
    B_count @ .
    dup ms
  again
;

\ to initialize T/C 2 for ms interrupt use: init_T2_OV
\ to test for milliseconds, 100 check_ms, delta is 102
\ to test for bounce counter 80 check_b, delta is 10
