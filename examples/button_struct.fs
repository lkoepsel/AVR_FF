\ button definitions
2   constant buttons \ number of buttons    

0   constant history
1   constant pressed
2   constant count

0	constant right
1	constant left

\ create button buttons count 1 + * cells allot
: .button buttons count 1 + * . ;
