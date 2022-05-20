\ Initialize Timer/Counter 2 to a multi-tasking timer 1000Hz
\ Don't use T and T0 simultaneously, they conflict interrupts
\ Counter increments every 1.0012ms
\ Mode 5, PWM, Phase Correct, Top=OCRA
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 00000001
\ WGM22 WGM20 => PWM, Phase Correct, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00001011
\ OCR2A = 250
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
$0a constant T2_OVF_VEC \ vector number, not address
$fa constant clock2_per \ clock period for comparison

\ Counters for timer overflows
variable ms_count      \ counter for milliseconds

\ disable T/C 2 Overflow interrupt
: dis_T2_OVF
  1 TIMSK2 mclr
;

\ Disable interrupt before removing the interrupt code
dis_T2_OVF

\ The interrupt routine
: T2_OVF_ISR
  1 ms_count +!
  D3 toggle
;i


\ initialize T/C 2 Overflow interrupt
: init_T2_OV  ( OCR2A -- )
  \ Store the interrupt vector
  ['] T2_OVF_ISR T2_OVF_VEC int!

  \ Activate T/C 2 for a 1ms interrupt
  %00000001 TCCR2A c!
  %00001011 TCCR2B c!
  clock2_per OCR2A c!

  D3 output
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

\ to initialize T/C 2 for ms interrupt use: init_T2_OV
\ to test for milliseconds, 100 check_ms, delta is 102
