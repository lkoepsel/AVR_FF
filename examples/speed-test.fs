\ Waggle PB5 as quickly as we can, in both high- and low-level code.
\ Before sending this file, we should send asm.txt and bit.txt.

$0025 constant portb    \ RAM address
$0023 constant pinb     \ RAM address
$0005 constant portb-io \ IO-space address
$0003 constant pinb-io  \ IO-space address
1 #5 lshift constant bit5

: initPB5
  bit5 ddrb mset \ set pin as output
  bit5 portb mclr \ initially known state
;

\ low-level bit fiddling, via assembler
: blink-asm ( -- )
  initPB5
  [
  begin,
    portb-io #5 sbi,  portb-io #5 cbi, \ 3.3Mhz, one cycle, on and off
    portb-io #5 sbi,  portb-io #5 cbi, \ not symmetrical
    portb-io #5 sbi,  portb-io #5 cbi,
    portb-io #5 sbi,  portb-io #5 cbi,
    wdr,
  again,
  ]
;

\ use toggle command to flip bits, allows for symmetry
: blink-tog ( -- )
  initPB5
  [
  begin,
    pinb-io #5 sbi,  \ one toggle per loop, 2MHz or 4 clock cycles, symmetrical
  again,
  ]
;
