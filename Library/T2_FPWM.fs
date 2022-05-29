\ Initialize Timer/Counter 2 to Fast PWM 
\ Use to vary intensity of an LED
\ Mode 3, Fast PWM, Top=OCRA, Clear OC2A on Compare Match
\ TCCR2A [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ] = 10000011
\ WGM22 WGM21 WGM20 => Fast PWM, TOP = OCRA
\ TCCR2B [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ] = 00000010
\ OCR2A = 127
\ CS21 => scalar of 8
\ Frequency = 7.8kHz, duty cycle/255 on stack
\ 

-T2
marker -T2

\ Timer 2 definitions from m328pdef.inc
$b0 constant TCCR2A
$b1 constant TCCR2B
$b3 constant OCR2A
#127 constant clock2_per \ clock period for 50% duty

\ initialize Timer/Counter 2 to Fast PWM, top = OCRA
: init_T2  ( dc/255 -- )

  \ Activate T/C 2 for Fast PWM
  OCR2A c!
  %10000011 TCCR2A c!
  %00000010 TCCR2B c!
  D11 output
;
