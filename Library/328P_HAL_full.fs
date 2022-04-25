empty
\ HAL: provides constants for LEDs and Buttons
\ Ports, DDRx, PORTx and PINx, page 624 datasheet
$23 constant PINB  \ Port B input register
$24 constant DDRB  \ Port B data direction register
$25 constant PORTB \ Port B output register
$26 constant PINC  \ Port C input register
$27 constant DDRC  \ Port C data direction register
$28 constant PORTC \ Port C output register
$29 constant PIND  \ Port D input register
$2a constant DDRD  \ Port D data direction register
$2b constant PORTD \ Port D output register

%10000000 constant PIN7   \ bit 07  
%01000000 constant PIN6   \ bit 06  
%00100000 constant PIN5   \ bit 05  
%00010000 constant PIN4   \ bit 04  
%00001000 constant PIN3   \ bit 03  
%00000100 constant PIN2   \ bit 02  
%00000010 constant PIN1   \ bit 01  
%00000001 constant PIN0   \ bit 00  

\ Arduino Board Pins, reference using Board Pins, not AVR registers if possible
\ Pins referenced by nn, where nn is the Arduino board pin number
PIN5 DDRC 2constant A5 \ Board Connector A5 PC5
PIN4 DDRC 2constant A4 \ Board Connector A4 PC4
PIN3 DDRC 2constant A3 \ Board Connector A3 PC3
PIN2 DDRC 2constant A2 \ Board Connector A2 PC2
PIN1 DDRC 2constant A1 \ Board Connector A1 PC1
PIN0 DDRC 2constant A0 \ Board Connector A0 PC0
PIN5 DDRB 2constant LED \ Board Connector 13 PB5
PIN5 DDRB 2constant D13 \ Board Connector 13 PB5
PIN4 DDRB 2constant D12 \ Board Connector 12 PB4
PIN3 DDRB 2constant D11 \ Board Connector 11 PB3 PWM OC2A
PIN2 DDRB 2constant D10 \ Board Connector 10 PB2 PWM OC1B
PIN1 DDRB 2constant D9  \ Board Connector  9 PB1 PWM OC1A
PIN0 DDRB 2constant D8  \ Board Connector  8 PB0 
PIN7 DDRD 2constant D7  \ Board Connector  7 PD7 
PIN6 DDRD 2constant D6  \ Board Connector  6 PD6 PWM OC0A
PIN5 DDRD 2constant D5  \ Board Connector  5 PD5 PWM OC0B
PIN4 DDRD 2constant D4  \ Board Connector  4 PD4 
PIN3 DDRD 2constant D3  \ Board Connector  3 PD3 PWM OC2B
PIN2 DDRD 2constant D2  \ Board Connector  2 PD2 
PIN1 DDRD 2constant D1  \ Board Connector  1 PD1 
PIN0 DDRD 2constant D0  \ Board Connector  0 PD0

\ !!PWM Registers, a work in progress, remove line when PWM correct
\ Timer/Counter Control Registers
$44  constant TCCR0A \ Timer/Counter Register A
\ [ COM0A1 COM0A0 COM0B1 COM0B0 - - WGM01 WGM00]
$45  constant TCCR0B \ Timer/Counter Register B
\ [ FOCOA FOCOB - - WGM02 CS02 CS01 CS00]
$47  constant OCR0A  \ Output Compare Register A
$48  constant OCR0B  \ Output Compare Register B
$88  constant OCR1AL \ Output Compare Register 1 A low
$89  constant OCR1AH \ Output Compare Register 1 A high
$8a  constant OCR1BL \ Output Compare Register 1 B low
$8b  constant OCR1BH \ Output Compare Register 1 B high

\ TCCR2A - Timer/Counter2 Control Register A
\ [ COM2A1 COM2A0 COM2B1 COM2B0 0 0 WGM21 WGM20 ]
$b0  constant TCCR2A \ Control Register A
$0  constant WGM20  \ Waveform Genration Mode
$1  constant WGM21  \ Waveform Genration Mode
$4  constant COM2B0 \ Compare Output Mode bit 0
$5  constant COM2B1 \ Compare Output Mode bit 1
$6  constant COM2A0 \ Compare Output Mode bit 1
$7  constant COM2A1 \ Compare Output Mode bit 1

\ TCCR2B - Timer/Counter2 Control Register B
\ [ FOC2A FOC2B 0 0 WGM22 CS22 CS21 CS20 ]
$b1  constant TCCR2B \ Control Register B
$0  constant CS20   \ Clock Select bit 0
$1  constant CS21   \ Clock Select bit 1
$2  constant CS22   \ Clock Select bit 2
$3  constant WGM22  \ Waveform Generation Mode
$6  constant FOC2B  \ Force Output Compare B
$7  constant FOC2A  \ Force Output Compare A

$b2  constant TCNT2  \ Timer/Counter 2
$b3  constant OCR2A  \ Output Compare Register A
$b4  constant OCR2B  \ Output Compare Register B

0    constant RESET  \ for resetting registers
0    constant NORML  \ Normal mode, $FF TOP
%001 constant PWMPCF \ PWM, Phase Correct $FF TOP
%011 constant FASTF   \ Fast PWM, $FF TOP
%101 constant PWMPCA \ PWM, Phase Correct OCRA TOP
%111 constant FASTA \ Fast PWM, Phase Correct OCRA TOP

\ PRIM: primitives for initializing port and set high or low output
: high ( bit port -- ) 1 + mset ;  \ set a pin high
: low ( bit port -- ) 1 + mclr ;  \ set a pin low
: toggle ( bit port -- ) 1 - mset ; \ toggle output value
: output ( bit port -- ) mset ;  \ set pin as output
: input ( bit port -- ) mclr ;  \ set a pin as input
: pullup ( bit port -- ) 2dup input high ; \ set pin as input_pullup
: read ( bit port -- f ) 1 - c@ and ; \ read a pin, returns bit value
