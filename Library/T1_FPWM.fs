\ **16.9.3 Fast PWM Mode**
\ Initialize timer 1 to a Fast PWM (Single Slope) pins 9 and 10
\ Timer 1 definitions pgs 140-166 DS40002061B
\ TCCR1A [ COM1A1 COM1A0 COM1B1 COM1B0 0 0 WGM11 WGM10 ]
\ TCCR1B [ ICNC1 ICES1 0 WGM13 WGM12 CS12 CS11 CS10 ]
\ freq(1-5) frequency (Hz) @ (1 res=255) (62.4K, 7.80K, 975, 244, 60)
\ freq(1-5) frequency (Hz) @ (2 res=511) (31.20K, 3.90K, 488, 122, 30)
\ freq(1-5) frequency (Hz) @ (3 res=1023)(15.60K, 1.95K, 244, 61, 15)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dc(1-res) n/resolution duty cycle
\ use freq/resolution combinations for easier implementation
\ for example: 62K_8 is 62.4K freq with 8-bit resolution
\              16K_0 is 15.6K freq with 10-bit resolution
\ duty cycle is defined by n/resolution
\ for example: 8-bit resolution => 255, 127/255 = 50% dc
\              9-bit resolution => 511, 255/511 = 50% dc
\             10-bit resolution => 1023, 511/1023 = 50% dc
\ duty cycle per pin is independent, both MUST be set

\ Three options:
\ FPWM_1: (freq dcA dcB) Both D9 and D10 have PWM frequencies
\ FPWM_1A: (freq dcA) Only D9 (T/C 1A) has a PWM frequency
\ FPWM_1B: (freq dcB) Only D10 (T/C 1B) has a PWM frequency

\ freq: 8-bit resolution => duty cycle n/255
1 1 2constant 62K_8
2 1 2constant 8K_8
3 1 2constant 975_8
4 1 2constant 244_8
5 1 2constant 60_8

\ freq: 9-bit resolution => duty cycle n/512
1 2 2constant 31K_9
2 2 2constant 4K_9
3 2 2constant 488_9
4 2 2constant 122_9
5 2 2constant 30_9

\ freq: 10-bit resolution => duty cycle n/1023
1 3 2constant 16K_0
2 3 2constant 2K_0
3 3 2constant 244_0
4 3 2constant 61_0
5 3 2constant 15_0

\ initialize T/C 1 as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1 ( freq dcA dcB -- )
  ocr1bl ! \ n/resolution dcB
  ocr1al ! \ n/resolution dcA
  %10100000 + tccr1a c! \ COM1A1 COM1B1 res
  %00001000 + tccr1b c! \ WGM12 freq
  D9 output
  D10 output
;

\ initialize T/C 1 A ONLY as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1A ( freq dcA -- )
  ocr1al ! \ n/resolution dcA
  %10000000 + tccr1a c! \ COM1A1 res
  %00001000 + tccr1b c! \ WGM12 freq
  D9 output
;

\ initialize T/C 1 B ONLY as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1B ( freq dcB -- )
  ocr1bl ! \ n/resolution dcB
  %00100000 + tccr1a c! \ COM1B1 res
  %00001000 + tccr1b c! \ WGM12 freq
  D10 output
;

-T1_FPWM
marker -T1_FPWM
