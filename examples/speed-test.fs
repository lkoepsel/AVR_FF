\ Waggle PB5 as quickly as we can, in both high- and low-level code.
\ Before sending this file, we should send asm.txt and bit.txt.

: initD8
  D8 out \ set pin as output
  D8 low \ initially known state
;

\ use toggle command to flip bits, allows for symmetry
: blink-tog ( -- )
  initD8
  [
  begin,
    pinb-io #0 sbi,  \ one toggle per loop, 2MHz or 4 clock cycles, symmetrical
  again,
  ]
;

\ read input register PIND
: pind@ ( -- c )
  \ Make room in the stack top registers. 
  \ Or just use DUP, it inlines the same code automatically.
  [ R25 -Y st,  ]
  [ R24 -Y st,  ]
  [ R24 $9 inn, ]  \ put the low byte on the stack
  [ R25 clr,    ]  \ clear the high byte
; 

\ in_D5, test reading pin D5
: in_D5 D5 pullup pind@ BIT5 and . ;
