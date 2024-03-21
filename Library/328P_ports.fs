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
: tog ( bit port -- ) 1 - mset ; \ toggle output value
: out ( bit port -- ) mset ;  \ set pin as output
: in ( bit port -- ) mclr ;  \ set a pin as input
: pullup ( bit port -- ) 2dup in high ; \ set pin as input_pullup
: read ( bit port -- f ) 1 - c@ and ; \ read a pin, returns bit value
: on high ;
: off low ;

-end_ports
marker -end_ports
