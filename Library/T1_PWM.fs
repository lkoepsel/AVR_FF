\ Initialize timer 1 as a Fast PWM (Single Slope)
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ freq(1-5) frequency (Hz) @ (1 res=255) (62.4K, 7.80K, 975, 244, 60)
\ freq(1-5) frequency (Hz) @ (2 res=511) (31.20K, 3.90K, 488, 122, 30)
\ freq(1-5) frequency (Hz) @ (3 res=1023)(15.60K, 1.95K, 244, 61, 15)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dc(1-res) n/resolution duty cycle

-T1_ctc
marker -T1_ctc
\ Timer 1 definitions pgs 140-166 DS40002061B
$80 constant TCCR1A
$81 constant TCCR1B
$88 constant OCR1AL
$89 constant OCR1AH
$8a constant OCR1BL
$8b constant OCR1BH

\ initialize T/C 1 as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1 ( freq res dcA dcB -- )
  OCR1BL ! \ n/resolution dcB
  OCR1AL ! \ n/resolution dcA
  %10110000 + TCCR1A c! \ COM1A1 COM1B1 res
  %00001000 + TCCR1B c! \ WGM12 freq
  D9 output
  D10 output
;
