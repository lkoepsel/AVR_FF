empty
/ words to print an ASCII string
/ useful in debugging other programs printing hex strings

/ pa - print ascii, however, prints in reverse order, (mirror image)
: pa for emit next ;

/ pick - put nth member at TOS
: pick 2* sp@ + @ ;

/ reverse print ascii - start at beginning and print ascii
: rpa for r@ cpick emit next ;

/ example: hex 31 32 33 34 4 rpa abort 1234

/all
: pa for emit next ; : pick 2* sp@ + @ ; : rpa for r@ pick emit next ;
