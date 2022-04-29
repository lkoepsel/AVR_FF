\ Initialize timer 2 to a 1ms counter
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 00000001
\ WGM22 WGM20 => PWM, Phase Correct, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00001011
\ OCR2A = 255
\ CS22 CS20 => scalar of 32
\ Frequency = 16 x 10^6 / 32 / 255 = 2000Hz
\ Counter performs another divide by 2 => 1000hz
\ Test using time_ct (100 time_ct) = delta of 100)
\ Disable interrupt before removing the interrupt code
dis_t2_OV
marker -irqOvf2
\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A
$70 constant TIMSK2
#10 constant T2_OVF_VEC
#08 constant BOUNCE_DIVIDER

\ Counter for timer overflows
variable ms_count      \ counter for milliseconds
variable b_count      \ counter for milliseconds
variable bounce_scalar \ scalar for bounce to check every 8ms

\ The interrupt routine
: t2OverflowIsr
  1 ms_count +!
  bounce_scalar @ 1 - 
  if b_count 1+ BOUNCE_DIVIDER bounce_scalar !
  else bounce_scalar !
  then
;i


\ initialize T/C 2 Overflow interrupt
: init_T2_OV
  \ Store the interrupt vector
  ['] t2OverflowIsr T2_OVF_VEC int!
  \ Activate T/C 2
  1 TCCR2A c!
  $b TCCR2B c!
  $ff OCR2A c!
  \ initialize bounce clock scalar
  BOUNCE_DIVIDER bounce_scalar !
  \ Activate timer2 overflow interrupt
  1 TIMSK2 mset
;

\ disable T/C 2 Overflow interrupt
: dis_t2_OV
  1 TIMSK2 mclr
;

\ ms timer used to check accuracy, 100 time_ct will display a delta of 100
: time_ct ( ms -- ) \ ms delay to use for checking ms_count
  begin
    ms_count @ .
    dup ms
  again
;

\ ms timer used to check accuracy, 100 time_ct will display a delta of 100
: time_bs ( ms -- ) \ ms delay to use for checking ms_count
  begin
    bounce_scalar @ .
    dup ms
  again
;

\ initialize T/C 2 for ms interrupt using
\ init_T2_OV


#08 constant BOUNCE_DIVIDER
variable bounce_scalar \ scalar for bounce to check every 8ms
variable b_count      \ counter for milliseconds
BOUNCE_DIVIDER bounce_scalar !
: test 
bounce_scalar @ 1 - dup 0=
  if drop 1 b_count +! BOUNCE_DIVIDER bounce_scalar !
  else bounce_scalar !
  then
  ;
