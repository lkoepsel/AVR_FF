\ Initialize timer 1 to a Clear Timer on Compare Match (CTC) pin 10
\ Timer 1 definitions pgs 140-166 DS40002061B
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ CTC Mode 4: WGM3:0 = 0100
\ OC1A/B: COM1B1:0 = 0001 Toggle on Compare Match, Top = OCR1A

-T1_ctc
marker -T1_ctc
\ Timer 1 definitions pgs 140-166 DS40002061B
$80 constant TCCR1A
$81 constant TCCR1B
$88 constant OCR1AL
$89 constant OCR1AH
$8a constant OCR1BL
$8b constant OCR1BH

\ initialize T/C 1 B ONLY as CTC, Top = OCR1A
\ freq(1-5) frequency (based on resolution, see above)
: CTC ( freq -- )
  OCR1AL ! \ freq based on OCR1A
  %00010000 TCCR1A c! \ COM1B1
  %00001010 TCCR1B c! \ WGM12 scalar
  D10 output
;
