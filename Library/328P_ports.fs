empty

%10000000 constant BIT7   \ bit 07  
%01000000 constant BIT6   \ bit 06  
%00100000 constant BIT5   \ bit 05  
%00010000 constant BIT4   \ bit 04  
%00001000 constant BIT3   \ bit 03  
%00000100 constant BIT2   \ bit 02  
%00000010 constant BIT1   \ bit 01  
%00000001 constant BIT0   \ bit 00  

\ Arduino Board Pins, reference using Board Pins
\ not AVR registers if possible
\ Pins referenced by nn, where nn is the Arduino board pin number
BIT5 ddrc 2constant A5 \ Board Connector A5 PC5
BIT4 ddrc 2constant A4 \ Board Connector A4 PC4
BIT3 ddrc 2constant A3 \ Board Connector A3 PC3
BIT2 ddrc 2constant A2 \ Board Connector A2 PC2
BIT1 ddrc 2constant A1 \ Board Connector A1 PC1
BIT0 ddrc 2constant A0 \ Board Connector A0 PC0
BIT5 ddrb 2constant LED \ Board Connector 13 PB5
BIT5 ddrb 2constant D13 \ Board Connector 13 PB5
BIT4 ddrb 2constant D12 \ Board Connector 12 PB4
BIT3 ddrb 2constant D11 \ Board Connector 11 PB3 PWM OC2A
BIT2 ddrb 2constant D10 \ Board Connector 10 PB2 PWM OC1B
BIT1 ddrb 2constant D9  \ Board Connector  9 PB1 PWM OC1A
BIT0 ddrb 2constant D8  \ Board Connector  8 PB0 
BIT7 ddrd 2constant D7  \ Board Connector  7 PD7 
BIT6 ddrd 2constant D6  \ Board Connector  6 PD6 PWM OC0A
BIT5 ddrd 2constant D5  \ Board Connector  5 PD5 PWM OC0B
BIT4 ddrd 2constant D4  \ Board Connector  4 PD4 
BIT3 ddrd 2constant D3  \ Board Connector  3 PD3 PWM OC2B
BIT2 ddrd 2constant D2  \ Board Connector  2 PD2 
BIT1 ddrd 2constant D1  \ Board Connector  1 PD1 
BIT0 ddrd 2constant D0  \ Board Connector  0 PD0

\ PRIM: primitives for initializing port and set high or low output
: high ( bit port -- ) 1 + mset ;  \ set a pin high
: low ( bit port -- ) 1 + mclr ;  \ set a pin low
: toggle ( bit port -- ) 1 - mset ; \ toggle output value
: output ( bit port -- ) mset ;  \ set pin as output
: input ( bit port -- ) mclr ;  \ set a pin as input
: pullup ( bit port -- ) 2dup input high ; \ set pin as input_pullup
: read ( bit port -- f ) 1 - c@ and ; \ read a pin, returns bit value
: on high ;
: off low ;

-end_ports
marker -end_ports

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

\ Timer 1 definitions pgs 140-166 DS40002061B
$80 constant TCCR1A
$81 constant TCCR1B
$88 constant OCR1AL
$89 constant OCR1AH
$8a constant OCR1BL
$8b constant OCR1BH

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
  OCR1BL ! \ n/resolution dcB
  OCR1AL ! \ n/resolution dcA
  %10100000 + TCCR1A c! \ COM1A1 COM1B1 res
  %00001000 + TCCR1B c! \ WGM12 freq
  D9 output
  D10 output
;

\ initialize T/C 1 A ONLY as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1A ( freq dcA -- )
  OCR1AL ! \ n/resolution dcA
  %10000000 + TCCR1A c! \ COM1A1 res
  %00001000 + TCCR1B c! \ WGM12 freq
  D9 output
;

\ initialize T/C 1 B ONLY as Fast PWM w/ variable duty cycle
\ freq(1-5) frequency (based on resolution, see above)
\ res(1-3) resolution of counter (255, 511, 1023)
\ dcx(1-res) n/resolution duty cycle, one for each pin
: FPWM_1B ( freq dcB -- )
  OCR1BL ! \ n/resolution dcB
  %00100000 + TCCR1A c! \ COM1B1 res
  %00001000 + TCCR1B c! \ WGM12 freq
  D10 output
;

-T1_ctc
marker -T1_ctc
