\ Series of words, which enable timing activites using an oscilloscope

\ asm-based toggle word to flip bits, minimum overhead
: tog_D8 ( -- )
  [ pinb-io #0 sbi, ] \ one toggle per loop
;

\ simple test using tog word, uses 11 clock cycles or 687ns
: time_it ( -- )
    D8 out
    begin
        tog_D8
        \ put code here to test
    again
;

\ optimized for minmimum overhead, 4 clock cycles or 250ns
: time_ito ( -- )
    D8 out
    begin
        [ pinb-io #0 sbi, ] \ one toggle per loop
        \ put code here to test
    again
;
