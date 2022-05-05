\ HAL: provides constants for LEDs and Buttons
\ Ports, DDRx, PORTx and PINx, page 624 datasheet
$23 constant PINB  \ Port B input register (-1 for input port)
$24 constant DDRB  \ Port B data direction register (port reference)
$25 constant PORTB \ Port B output register (+1 for output port)
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

\ microseconds delay for Atmega
marker -us

\ Opcode only to flash
: op: ( opcode -- ) flash create , ram does> @ i, ;

\ Atmega wdr instruction
$95a8 op: wdr,

\ Clear watchdog (wdr instruction) takes one clock cycle (62.5ns @ 16MHz)
\ Adjust the number of CWD to achieve a one us delay
\ 9 CWD is needed @ 16MHz for ATmega 328 and 2560.
: us ( u -- ) \ blocking wait for u microseconds
  begin
    cwd cwd  cwd cwd cwd cwd cwd cwd cwd
    1- dup 
  0= until
  drop 
;

: .memory ( ---)
    cr ."  flash: "   flash  here u.
    cr ." eeprom: "   eeprom here u.
    cr ."    ram: "   ram    here u. 
    cr ;


\ PRIM: primitives for initializing port and set high or low output
: high ( bit port -- ) 1 + mset ;  \ set a pin high
: low ( bit port -- ) 1 + mclr ;  \ set a pin low
: toggle ( bit port -- ) 1 - mset ; \ toggle output value
: output ( bit port -- ) mset ;  \ set pin as output
: input ( bit port -- ) mclr ;  \ set a pin as input
: pullup ( bit port -- ) 2dup input high ; \ set pin as input_pullup
: read ( bit port -- f ) 1 - c@ and ; \ read a pin, returns bit value

marker -end_HAL
