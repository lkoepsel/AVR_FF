\ **16.9.2 Clear Timer on Compare Match (CTC)**
\ Initialize timer 1 to a Clear Timer on Compare Match (CTC) pin 9
\ Timer 1 definitions pgs 140-166 DS40002061B
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ CTC Mode 4: WGM3:0 = 0100
\ OC1A/B: COM1A1:0 = 01 Toggle on Compare Match, Top = OCR1A

\ initialize T/C 1 B ONLY as CTC, Top = OCR1A
\ freq(1-5) frequency (based on resolution, see above)
: CTC_1 ( scalar freq -- )
  ocr1al ! \ freq based on OCR1A
  %01000000 tccr1a c! \ COM1A1
  %00001000 + tccr1b c! \ WGM12 scalar
  D9 output
;

-T1_CTC
marker -T1_CTC

\ 1 0 CTC_1 \ Fastest Freq: 8MHz or 1 period = 125 ns
\ 3 1024 CTC_1 \ Example Freq - 16x10^6 / 64 / 1024 / 2 => 122Hz
\ 5 $ffff CTC_1 \ Slowest Freq: 119mHz or 1 period = 8.4 seconds
\ Calculation freq = 16MHz /scalar /OCR1A / 2
