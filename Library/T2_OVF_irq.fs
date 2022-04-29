\ Initialize Timer/Counter 2 to a 1ms timer
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 00000001
\ WGM22 WGM20 => PWM, Phase Correct, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00001011
\ OCR2A = 255
\ CS22 CS20 => scalar of 32
\ Frequency = 16 x 10^6 / 32 / 255 = 2000Hz
\ Counter performs another divide by 2 => 1000hz
\ Test using check_ms (100 check_ms) = delta of 100)

\ Disable interrupt before removing the interrupt code
dis_T2_OVF
marker -T2_int
\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A
$70 constant TIMSK2
#10 constant T2_OVF_VEC
#08 constant B_DIVIDER

\ Counters for timer overflows
variable ms_count      \ counter for milliseconds
variable B_count       \ counter for scaled ticks
variable B_scalar      \ scalar for bounce divider

\ bounce scalar counter - divides T/C 2 clock by B_DIVIDER
: bounce 
    B_scalar @ 1 - dup 0=
    if drop 1 B_count +! B_DIVIDER B_scalar !
    else B_scalar !
    then
;

\ The interrupt routine
: T2_OVF_ISR
  1 ms_count +!
  bounce
;i


\ initialize T/C 2 Overflow interrupt
: init_T2_OV
  \ Store the interrupt vector
  ['] T2_OVF_ISR T2_OVF_VEC int!

  \ Activate T/C 2 for a 1ms interrupt
  1 TCCR2A c!
  $b TCCR2B c!
  $ff OCR2A c!

  \ initialize bounce clock scalar
  B_DIVIDER B_scalar !
  \ Activate timer2 overflow interrupt
  1 TIMSK2 mset
;

\ disable T/C 2 Overflow interrupt
: dis_T2_OVF
  1 TIMSK2 mclr
;

\ ms counter used to check accuracy, 100 check_ms will display a delta of 100
: check_ms ( ms -- ) \ ms delay to use for checking ms_count
  begin
    ms_count @ .
    dup ms
  again
;

\ bs counter used to check accuracy, 800 check_b will display a delta of 102
: check_b ( ms -- ) \ ms delay to use for checking bounce scalar
  begin
    B_count @ .
    dup ms
  again
;

\ to initialize T/C 2 for ms interrupt use: init_T2_OV
\ to test for milliseconds, 100 check_ms, delta is 100
\ to test for bounce counter 80 check_b, delta is 10