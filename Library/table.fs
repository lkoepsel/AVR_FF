\ lookup table example, if values need to be retained, use flash
empty
decimal
0 constant one
1 constant ten
2 constant twenty
3 constant thirty
4 constant forty
5 constant fifty

flash
create TWENS
    1 ,
    10 ,
    20 ,
    30 ,
    40 ,
    50 ,

ram
: V2D
    cells TWENS
    + @
;

\ thirty V2D . 30  ok<#,ram>
\ forty V2D . 40  ok<#,ram>
