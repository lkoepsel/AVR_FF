# FlashForth 5 Quick Reference for AVR Microcontrollers

## Interpreter

The outer interpreter looks for words and numbers delimited by whitespace. Everything is interpreted as a word or a number. Numbers are pushed onto the stack. Words are looked up and acted upon. Names of words are limited to 15 characters. Some words are compile-time use only and cannot be used interpretively.

## Data and the stack

The data stack (S:) is directly accessible and has 16-bit cells for holding numerical values. Functions get their arguments from the stack and leave their results there as well. There is also a return address stack (R:) that can be used for temporary storage.

## Notation

| Symbol | Meaning |
|--------|---------|
| n, n1, n2, n3 | Single-cell integers (16-bit). |
| u, u1, u2 | Unsigned integers (16-bit). |
| x, x1, x2, x3 | Single-cell item (16-bit). |
| c | Character value (8-bit). |
| d ud | Double-cell signed and unsigned (32-bit). |
| t ut | Triple-cell signed and unsigned (48-bit). |
| q uq | Quad-cell signed and unsigned (64-bit). |
| f | Boolean flag: 0 is false. -1 is true. |
| addr, addr1, addr2 | 16-bit addresses. |
| a-addr | cell-aligned address. |
| c-addr | character or byte address. |
| addr.d | 32-bit address. |

## Numbers and values

| Word | Description |
|------|-------------|
| `2` | Leave integer two onto the stack. ( -- 2 ) |
| `#255` | Leave decimal 255 onto the stack. ( -- 255 ) |
| `%11` | Leave integer three onto the stack. ( -- 3 ) |
| `$10` | Leave integer sixteen onto the stack. ( -- 16 ) |
| `23.` | Leave double number on the stack. ( -- 23 0 ) |
| `decimal` | Set number format to base 10. ( -- ) |
| `hex` | Set number format to hexadecimal. ( -- ) |
| `bin` | Set number format to binary. ( -- ) |
| `s>d` | Sign extend single to double number. ( n -- d ) Since double numbers have the most significant bits in the cell above the least significant bits, you can just drop the top cell to recover the single number, provided that the value is not too large to fit in a single cell. |

## Displaying data

| Word | Description |
|------|-------------|
| `.` | Display a number. ( n -- ) |
| `u.` | Display u unsigned. ( u -- ) |
| `u.r` | Display u with field width n, 0 < n < 256. ( u n -- ) |
| `d.` | Display double number. ( d -- ) |
| `ud.` | Display unsigned double number. ( ud -- ) |
| `.s` | Display stack content (nondestructively). |
| `.st` | Emit status string for base, current data section, and display the stack contents. ( -- ) |
| `dump` | Display memory from address, for u bytes. ( addr u -- ) |

## Stack manipulation

| Word | Description |
|------|-------------|
| `dup` | Duplicate top item. ( x -- x x ) |
| `?dup` | Duplicate top item if nonzero. ( x -- 0 \| x x ) |
| `swap` | Swap top two items. ( x1 x2 -- x2 x1 ) |
| `over` | Copy second item to top. ( x1 x2 -- x1 x2 x1 ) |
| `drop` | Discard top item. ( x -- ) |
| `nip` | Remove x1 from the stack. ( x1 x2 -- x2 ) |
| `rot` | Rotate top three items. ( x1 x2 x3 -- x2 x3 x1 ) |
| `tuck` | Insert x2 below x1 in the stack. ( x1 x2 -- x2 x1 x2 ) |
| `pick` | Duplicate the u-th item on top. ( xu ... x0 u -- xu ... x0 xu ) |
| `2dup` | Duplicate top double-cell item. ( d -- d d ) |
| `2swap` | Swap top two double-cell items. ( d1 d2 -- d2 d1 ) |
| `2over` | Copy second double item to top. ( d1 d2 -- d1 d2 d1 ) |
| `2drop` | Discard top double-cell item. ( d -- ) |
| `>r` | Send to return stack. S:( n -- ) R:( -- n ) |
| `r>` | Take from return stack. S:( -- n ) R:( n -- ) |
| `r@` | Copy top item of return stack. S:( -- n ) R:( n -- n ) |
| `rdrop` | Discard top item of return stack. S:( -- ) R:( n -- ) |
| `sp@` | Leave data stack pointer. ( -- addr ) |
| `sp!` | Set the data stack pointer to address. ( addr -- ) |

## Operators

### Arithmetic with single-cell numbers

Some of these words require `core.txt` and `math.txt`.

| Word | Description |
|------|-------------|
| `+` | Add. ( n1 n2 -- n1+n2 ) sum |
| `-` | Subtract. ( n1 n2 -- n1-n2 ) difference |
| `*` | Multiply. ( n1 n2 -- n1*n2 ) product |
| `/` | Divide. ( n1 n2 -- n1/n2 ) quotient |
| `mod` | Divide. ( n1 n2 -- n.rem ) remainder |
| `/mod` | Divide. ( n1 n2 -- n.rem n.quot ) |
| `u/` | Unsigned 16/16 to 16-bit division. ( u1 u2 -- u2/u1 ) |
| `u/mod` | Unsigned division. ( u1 u2 -- u.rem u.quot ) 16-bit/16-bit to 16-bit |
| `1` | Leave one. ( -- 1 ) |
| `1+` | Add one. ( n -- n1 ) |
| `1-` | Subtract one. ( n -- n1 ) |
| `2+` | Add two. ( n -- n1 ) |
| `2-` | Subtract 2 from n. ( n -- n1 ) |
| `2*` | Multiply by 2; Shift left by one bit. ( u -- u1 ) |
| `2/` | Divide by 2; Shift right by one bit. ( u -- u1 ) |
| `*/` | Scale. ( n1 n2 n3 -- n1*n2/n3 ) Uses 32-bit intermediate result. |
| `*/mod` | Scale with remainder. ( n1 n2 n3 -- n.rem n.quot ) Uses 32-bit intermediate result. |
| `u+/mod` | Unsigned Scale u1*u2/u3. ( u1 u2 u3 -- u.rem u.quot ) Uses 32-bit intermediate result. |
| `abs` | Absolute value. ( n -- u ) |
| `negate` | Negate n. ( n -- -n ) |
| `?negate` | Negate n1 if n2 is negative. ( n1 n2 -- n3 ) |
| `min` | Leave minimum. ( n1 n2 -- n ) |
| `max` | Leave maximum. ( n1 n2 -- n ) |
| `umin` | Unsigned minimum. ( u1 u2 -- u ) |
| `umax` | Unsigned maximum. ( u1 u2 -- u ) |

### Arithmetic with double-cell numbers

Some of these words require `core.txt`, `math.txt` and `qmath.txt`.

| Word | Description |
|------|-------------|
| `d+` | Add double numbers. ( d1 d2 -- d1+d2 ) |
| `d-` | Subtract double numbers. ( d1 d2 -- d1-d2 ) |
| `m+` | Add single cell to double number. ( d n -- d2 ) |
| `m*` | Signed 16*16 to 32-bit multiply. ( n n -- d ) |
| `2*` | Multiply by 2. ( d -- d ) |
| `d2/` | Divide by 2. ( d -- d ) |
| `um*` | Unsigned 16x16 to 32 bit multiply. ( u1 u2 -- ud ) |
| `ud*` | Unsigned 32x16 to 32-bit multiply. ( ud u -- ud ) |
| `um/mod` | Unsigned division. ( ud u1 -- u.rem u.quot ) 32-bit/16-bit to 16-bit |
| `ud/mod` | Unsigned division. ( ud u1 -- u.rem ud.quot ) 32-bit/16-bit to 32-bit |
| `fm/mod` | Floored division. ( d n -- n.rem n.quot ) |
| `sm/rem` | Symmetric division. ( d n -- n.rem n.quot ) 32-bit/16-bit to 16-bit. |
| `m*/` | Scale with triple intermediate result. d2 = d1*n1/n2 ( d1 n1 n2 -- d2 ) |
| `um*/` | Scale with triple intermediate result. ud2 = ud1*u1/u2 ( ud1 u1 u2 -- ud2 ) |
| `dabs` | Absolute value. ( d -- ud ) |
| `dnegate` | Negate double number. ( d -- -d ) |
| `?dnegate` | Negate d if n is negative. ( d n -- -d ) |

## Relational

| Word | Description |
|------|-------------|
| `=` | Leave true if x1 x2 are equal. ( x1 x2 -- f ) |
| `<>` | Leave true if x1 x2 are not equal. ( x1 x2 -- f ) |
| `<` | Leave true if n1 less than n2. ( n1 n2 -- f ) |
| `>` | Leave true if n1 greater than n2. ( n1 n2 -- f ) |
| `0=` | Leave true if n is zero. ( n -- f ) Inverts logical value. |
| `0<` | Leave true if n is negative. ( n -- f ) |
| `within` | Leave true if xl <= x < xh. ( x xl xh -- f ) |
| `u<` | Leave true if u1 < u2. ( u1 u2 -- f ) |
| `u>` | Leave true if u1 > u2. ( u1 u2 -- f ) |
| `d=` | Leave true if d1 d2 are equal. ( d1 d2 -- f ) |
| `d0<` | Leave true if d is negative. ( d -- f ) |
| `d<` | Leave true if d1 < d2. ( d1 d2 -- f ) |
| `d>` | Leave true if d1 > d2. ( d1 d2 -- f ) |

## Bitwise

| Word | Description |
|------|-------------|
| `invert` | Ones complement. ( x -- x ) |
| `dinvert` | Invert double number. ( du -- du ) |
| `and` | Bitwise and. ( x1 x2 -- x ) |
| `or` | Bitwise or. ( x1 x2 -- x ) |
| `xor` | Bitwise exclusive-or. ( x -- x ) |
| `lshift` | Left shift by u bits. ( x1 u -- x2 ) |
| `rshift` | Right shift by u bits. ( x1 u -- x2 ) |

## Memory

Typically, the microcontroller has three distinct memory contexts: Flash, EEPROM and SRAM. FlashForth unifies these memory spaces into a single 64kB address space.

### AVR8 Memory map

All operations are restricted to 64kB byte address space that is divided into:

| Range | Description |
|-------|-------------|
| $0000 – (RAMSIZE-1) | SRAM |
| RAMSIZE – (RAMSIZE+EEPROMSIZE-1) | EEPROM |
| ($ffff-FLASHSIZE+1) – $ffff | Flash |

The SRAM space includes the IO-space and special function registers. The high memory mark for the Flash context is set by the combined size of the boot area and FF kernel.

### Memory Context

| Word | Description |
|------|-------------|
| `ram` | Set address context to SRAM. ( -- ) |
| `eeprom` | Set address context to EEPROM. ( -- ) |
| `flash` | Set address context to Flash. ( -- ) |
| `fl-` | Disable writes to Flash, EEPROM. ( -- ) |
| `fl+` | Enable writes to Flash, EEPROM, default. ( -- ) |
| `iflush` | Flush the flash write buffer. ( -- ) |
| `here` | Leave the current data section dictionary pointer. ( -- addr ) |
| `align` | Align the current data section dictionary pointer to a cell boundary. ( -- ) |
| `hi` | Leave the high limit of the current data space. ( -- u ) |

### Accessing Memory

| Word | Description |
|------|-------------|
| `!` | Store x to address. ( x a-addr -- ) |
| `@` | Fetch from address. ( a-addr -- x ) |
| `@+` | Fetch cell and increment address by cell size. ( a-addr1 -- a-addr2 x ) |
| `2!` | Store 2 cells to address. ( x1 x2 a-addr -- ) |
| `2@` | Fetch 2 cells from address. ( a-addr -- x1 x2 ) |
| `c!` | Store character to address. ( c addr -- ) |
| `c@` | Fetch character from address. ( addr -- c ) |
| `c@+` | Fetch char, increment address. ( addr1 -- addr2 c ) |
| `+!` | Add n to cell at address. ( n addr -- ) |
| `-@` | Fetch from addr and decrement addr by 2. ( addr1 -- addr2 x ) |
| `>a` | Write to the A register. ( x -- ) |
| `a>` | Read from the A register. ( -- x ) |

### Accessing Extended (Flash) Memory

| Word | Description |
|------|-------------|
| `x!` | Store u to real flash address. ( u addr.d -- ) |
| `x@` | Fetch from real flash address. ( addr.d -- u ) |

### Accessing bits in RAM

| Word | Description |
|------|-------------|
| `mset` | Set bits in file register with mask c. ( c addr -- ) |
| `mclr` | Clear bits in file register with mask c. ( c addr -- ) |
| `mtst` | AND file register byte with mask c. ( c addr -- x ) |

The following come from `bit.txt`:

| Word | Description |
|------|-------------|
| `bit1:` *name* | Define a word to set a bit. ( addr bit -- ) |
| `bit0:` *name* | Define a word to clear a bit. ( addr bit -- ) |
| `bit?:` *name* | Define a word to test a bit. ( addr bit -- ) When executed, *name* leaves a flag. ( -- f ) |

## The Dictionary

### Dictionary management

| Word | Description |
|------|-------------|
| `marker` *-my-mark* | Mark the dictionary and memory allocation state with -my-mark. |
| `-my-mark` | Return to the dictionary and allotted-memory state that existed before -my-mark was created. |
| `find` *name* | Find name in dictionary. ( -- n ) Leave 1 immediate, -1 normal, 0 not found. |
| `forget` *name* | Forget dictionary entries back to *name*. |
| `empty` | Reset all dictionary and allotted-memory pointers. ( -- ) |
| `words` | List all words in dictionary. ( -- ) |
| `words` *xxx* | List words containing *xxx*. ( -- ) |

### Defining constants and variables

| Word | Description |
|------|-------------|
| `constant` *name* | Define new constant. ( n -- ) |
| `2constant` *name* | Define double constant. ( x x -- ) |
| *name* | Leave value on stack. ( -- n ) |
| `variable` *varname* | Define a variable in the current data section. ( -- ) Use `ram`, `eeprom` or `flash` to set data section. |
| `2variable` *name* | Define double variable. ( -- ) |
| *varname* | Leave address on stack. ( -- addr ) |
| `value` *valname* | Define value. ( n -- ) |
| `to` *valname* | Assign new value to *valname*. ( n -- ) |
| *valname* | Leave value on stack. ( -- n ) |
| `user` *name* | Define a user variable at offset +n. ( +n -- ) |

### Examples

```forth
ram
3 value xx                  \ Define value in SRAM.
variable yy                 \ Define variable in SRAM.
6 yy !                      \ Store 6 in variable yy.
eeprom 5 value zz ram       \ Define value in EEPROM.

warm                        \ Warm restart clears SRAM data.
4 to xx                     \ Sets new value.
hi here - u.                \ Number of bytes free.
```

## Defining compound data objects

| Word | Description |
|------|-------------|
| `create` *name* | Create a word definition and store the current data section pointer. |
| `does>` | Define the runtime action of a created word. |
| `allot` | Advance the current data section dictionary pointer by u bytes. ( u -- ) |
| `,` | Append x to the current data section. ( x -- ) |
| `c,` | Append c to the current data section. ( c -- ) |
| `," xxx"` | Append a string at HERE. ( -- ) |
| `i,` | Append x to the flash data section. ( x -- ) |
| `ic,` | Append c to the flash data section. ( c -- ) |
| `cf,` | Compile xt into the flash dictionary. ( addr -- ) |
| `c>n` | Convert code field addr to name field addr. ( addr1 -- addr2 ) |
| `n>c` | Convert name field addr to code field addr. ( addr1 -- addr2 ) |
| `n>l` | Convert nfa to lfa. ( nfa -- lfa ) Not implemented; use `2-` instead. |
| `>body` | Leave the parameter field address of the created word. ( xt -- a-addr ) |
| `:noname` | Define headerless forth code. ( -- addr ) |
| `>xa` | Convert a Flash virtual address to a real executable address. ( a-addr1 -- a-addr2 ) |
| `xa>` | Convert a real executable address to a Flash virtual address. ( a-addr1 -- a-addr2 ) |

### Array examples

```forth
ram
create my-array 20 allot        \ ...of creating an array,
my-array 20 $ff fill            \ ...filling it with 1s, and
my-array 20 dump                \ ...displaying its content.

create my-cell-array
    100 , 340 , 5 ,             \ Initialised cell array.
my-cell-array 2 cells + @       \ Should leave 5. ( -- x )

create my-byte-array
    18 c, 21 c, 255 c,          \ Initialised byte array.
my-byte-array 2 chars + c@      \ Should leave 255. ( -- c )

: mk-byte-array                 \ Defining word ( n -- )
    create allot                \ ...to make byte array objects
    does> + ;                   \ ...as shown in FF user's guide.

10 mk-byte-array my-bytes       \ Creates an array object my-bytes. ( n -- addr )
10 18 my-bytes c!
21 1 my-bytes c!
255 2 my-bytes c!

: mk-cell-array                 \ Defining word ( n -- )
    create cells allot          \ ...to make cell array objects.
    does> swap cells + ;

5 mk-cell-array my-cells        \ Creates an array object my-cells. ( n -- addr )
3000 0 my-cells !               \ Sets an element
45000 1 my-cells !              \ ...and another.
63000 2 my-cells !
1 my-cells @ .                  \ Should print 45000
```

## Memory operations

Some of these words come from `core.txt`.

| Word | Description |
|------|-------------|
| `cmove` | Move u bytes from address-1 to address-2. ( addr1 addr2 u -- ) Copy proceeds from low addr to high address. |
| `fill` | Fill u bytes with c starting at address. ( addr u c -- ) |
| `erase` | Fill u bytes with 0 starting at address. ( addr u -- ) |
| `blanks` | Fill u bytes with spaces starting at address. ( addr u -- ) |
| `cells` | Convert cells to address units. ( u -- u ) |
| `chars` | Convert chars to address units. ( u -- u ) |
| `char+` | Add one to address. ( addr1 -- addr2 ) |
| `cell+` | Add size of cell to address. ( addr1 -- addr2 ) |
| `aligned` | Align address to a cell boundary. ( addr -- a-addr ) |

## Predefined constants

| Word | Description |
|------|-------------|
| `cell` | Size of one cell in characters. ( -- n ) |
| `true` | Boolean true value. ( -- -1 ) |
| `false` | Boolean false value. ( -- 0 ) |
| `bl` | ASCII space. ( -- c ) |
| `Fcy` | Number of cpu instruction-cycle frequency in kHz. ( -- u ) |
| `ti#` | Size of the terminal input buffer. ( -- u ) |

## Predefined variables

| Word | Description |
|------|-------------|
| `base` | Variable containing number base. ( -- a-addr ) |
| `irq` | Interrupt vector (SRAM variable). ( -- a-addr ) Always disable user interrupts and clear related interrupt enable bits before zeroing interrupt vector: `di false to irq ei` |
| `turnkey` | Vector for user start-up word. ( -- a-addr ) EEPROM value mirrored in SRAM. |
| `prompt` | Deferred execution vector for the info displayed by quit. Default value is `.st`. ( -- a-addr ) |
| `'emit` | EMIT vector. Default is rx1. ( -- a-addr ) |
| `'key` | KEY vector. Default is rx1. ( -- a-addr ) |
| `'key?` | KEY? vector. Default is rx1?. ( -- a-addr ) |
| `'source` | Current input source. ( -- a-addr ) |
| `s0` | Variable for start of data stack. ( -- a-addr ) |
| `r0` | Bottom of return stack. ( -- a-addr ) |
| `rcnt` | Number of saved return stack cells. ( -- a-addr ) |
| `tib` | Address of the terminal input buffer. ( -- a-addr ) |
| `tiu` | Terminal input buffer pointer. ( -- a-addr ) |
| `>in` | Variable containing the offset, in characters, from the start of `tib` to the current parse area. ( -- a-addr ) |
| `pad` | Address of the temporary area for strings. ( -- addr ) `: pad tib ti# + ;` Each task has its own pad but has zero default size. If needed the user must allocate it separately with allot for each task. |
| `dp` | Leave the address of the current data section dictionary pointer. ( -- a-addr ) EEPROM variable mirrored in RAM. |
| `hp` | Hold pointer for formatted numeric output. ( -- a-addr ) |
| `up` | Variable holding a user pointer. ( -- addr ) |
| `latest` | Variable holding the address of the latest defined word. ( -- a-addr ) |

## The Compiler

### Defining functions

| Word | Description |
|------|-------------|
| `:` | Begin colon definition. ( -- ) |
| `;` | End colon definition. ( -- ) |
| `[` | Enter interpreter state. ( -- ) |
| `]` | Enter compilation state. ( -- ) |
| `state` | Compilation state. ( -- f ) State can only be changed with `[` and `]`. |
| `[i` | Enter Forth interrupt context. ( -- ) |
| `]i` | Exit Forth interrupt context. ( -- ) |
| `;i` | End an interrupt word. ( -- ) |
| `literal` | Compile value on stack at compile time. ( x -- ) At run time, leave value on stack. ( -- x ) |
| `2literal` | Compile double value on stack at compile time. ( x x -- ) At run time, leave value on stack. ( -- x x ) |
| `inline` *name* | Inline the following word. |
| `inlined` | Mark the last compiled word as inlined. ( -- ) |
| `immediate` | Mark latest definition as immediate. ( -- ) |
| `immed?` | Leave a nonzero value if addr contains an immediate flag. ( addr -- f ) |
| `in?` | Leave a nonzero flag if nfa has inline bit set. ( nfa -- f ) |
| `postpone` *name* | Postpone action of immediate word. ( -- ) |
| `see` *name* | Show definition. Load `see.txt`. |

### Comments

| Word | Description |
|------|-------------|
| `(` *comment text* `)` | Inline comment. |
| `\` *comment text* | Skip rest of line. |

### Examples of colon definitions

```forth
: square ( n -- n**2 )      \ Example with stack comment.
    dup * ;                  \ ...body of definition.
```

## Flow control

### Structured flow control

| Word | Description |
|------|-------------|
| `if` *xxx* `else` *yyy* `then` | Conditional execution. ( f -- ) |
| `begin` *xxx* `again` | Infinite loop. ( -- ) |
| `begin` *xxx cond* `until` | Loop until *cond* is true. ( -- ) |
| `begin` *xxx cond* `while` *yyy* `repeat` | Loop while *cond* is true. ( -- ) *yyy* is not executed on the last iteration. |
| `for` *xxx* `next` | Loop u times. ( u -- ) `r@` gets the loop counter u-1 ... 0 |
| `endit` | Sets loop counter to zero so that we leave a `for` loop when `next` is encountered. ( -- ) |

From `doloop.txt`, we get the ANSI loop constructs which iterate from *initial* up to, but not including, *limit*:

```
limit initial do words-to-repeat loop
limit initial do words-to-repeat value +loop
```

| Word | Description |
|------|-------------|
| `i` | Leave the current loop index. ( -- n ) Innermost loop, for nested loops. |
| `j` | Leave the next-outer loop index. ( -- n ) |
| `leave` | Leave the do loop immediately. ( -- ) |
| `?do` | Starts a do loop which is not run if the arguments are equal. ( limit initial -- ) |

### Loop examples

```forth
decimal
: sumdo  0 100 0 do i + loop ;      \ sumdo leaves 4950
: sumfor 0 100 for r@ + next ;      \ sumfor leaves 4950
: print-twos 10 0 do i u. 2 +loop ;
```

### Case example

From `case.txt`, we get words `case`, `of`, `endof`, `default` and `endcase` to define case constructs.

```forth
: testcase
    4 for r@
        case
            0 of ." zero"    endof
            1 of ." one"     endof
            2 of ." two"     endof
            default ." default" endof
        endcase
    next
;
```

### Unstructured flow control

| Word | Description |
|------|-------------|
| `exit` | Exit from a word. ( -- ) If exiting from within a for loop, we must drop the loop count with `rdrop`. |
| `abort` | Reset stack pointer and execute quit. ( -- ) |
| `?abort` | If flag is false, print message and abort. ( f addr u -- ) |
| `?abort?` | If flag is false, output `?` and last word executed, followed by text *xxx*. ( f -- ) |
| `abort"` *xxx"* | If flag is false, type out last word executed, followed by text *xxx*. ( f -- ) |
| `quit` | Interpret from keyboard. ( -- ) |
| `warm` | Make a warm start. Reset reason will be displayed on restart: S: Reset instruction, E: External reset pin, W: Watchdog reset, U: Return stack underflow, O: Return stack overflow, B: Brown out reset, P: Power on reset, M: Math error, A: Address error. Note that irq vector is cleared. |

## Vectored execution (Function pointers)

| Word | Description |
|------|-------------|
| `'` *name* | Search for *name* and leave its execution token (address). ( -- addr ) |
| `[']` *name* | Search for *name* and leave it's execution token. ( -- ) |
| `execute` | Execute word at address. ( addr -- ) The actual stack effect will depend on the word executed. |
| `@ex` | Fetch vector from addr and execute. ( addr -- ) |
| `defer` *vec-name* | Define a deferred execution vector. ( -- ) |
| `is` *vec-name* | Store execution token in *vec-name*. ( addr -- ) |
| *vec-name* | Execute the word whose execution token is stored in *vec-name*'s data space. |
| `int!` | Store interrupt vector to table. ( xt vector-no -- ) Interrupt vector table in RAM. |

### Autostart example

```forth
' my-app is turnkey     \ Autostart my-app.
false is turnkey        \ Disable turnkey application.
```

## Interrupt example

```forth
ram variable icnt1      \ ...from FF source.
: irq_forth             \ It's a Forth colon definition
    [i                  \ ...in the Forth interrupt context.
        icnt1 @ 1+
        icnt1 !
    ]i
;i
' irq_forth 0 int!      \ Set the user interrupt vector.

: init                  \ Alternatively, compile a word
    ['] irq_forth 0 int!    \ ...so that we can install the
;                           \ ...interrupt service function
' init is turnkey           \ ...at every warm start.
```

## The P register

The P register can be used as a variable or as a pointer. It can be used in conjunction with `for..next` or at any other time.

| Word | Description |
|------|-------------|
| `!p` | Store address to P(ointer) register. ( addr -- ) |
| `@p` | Fetch the P register to the stack. ( -- addr ) |
| `!p>r` | Push contents of P to return stack and store new address to P. ( addr -- ) ( R: -- addr ) |
| `r>p` | Pop from return stack to P register. ( -- ) ( R: addr -- ) |
| `p+` | Increment P register by one. ( -- ) |
| `p2+` | Add 2 to P register. ( -- ) |
| `p++` | Add n to the p register. ( n -- ) |
| `p!` | Store x to the location pointed to by the p register. ( x -- ) |
| `pc!` | Store c to the location pointed to by the p register. ( c -- ) |
| `p@` | Fetch the cell pointed to by the p register. ( -- x ) |
| `pc@` | Fetch the char pointed to by the p register. ( -- c ) |

In a definition, `!p>r` and `r>p` should always be used to allow proper nesting of words.

## Characters

| Word | Description |
|------|-------------|
| `digit?` | Convert char to a digit according to base. ( c -- n f ) |
| `>digit` | Convert n to ascii character value. ( n -- c ) |
| `>pr` | Convert a character to an ASCII value. ( c -- c ) Nongraphic characters converted to a dot. |
| `char` *char* | Parse a character and leave ASCII value. ( -- n ) For example: `char A` ( -- 65 ) |
| `[char]` *char* | Compile inline ASCII character. ( -- ) |

## Strings

Some of these words come from `core.txt`.

| Word | Description |
|------|-------------|
| `s"` *text"* | Compile string into flash. ( -- addr u ) |
| `."` *text"* | Compile string to print into flash. ( -- ) |
| `place` | Place string from a1 to a2 as a counted string. ( addr1 u addr2 -- ) |
| `n=` | Compare strings in RAM(addr) and Flash(nfa). Leave true if strings match, n < 16. ( addr nfa u -- f ) |
| `scan` | Scan string until c is found. c-addr must point to RAM and u < 255. ( c-addr u c -- caddr1 u1 ) |
| `skip` | Skip chars until c not found. c-addr must point to RAM and u < 255. ( c-addr u c -- caddr1 u1 ) |
| `/string` | Trim string. ( addr u n -- addr+n u-n ) |
| `>number` | Convert string to a number. ( 0 0 addr1 u1 -- ud.l ud.h addr2 u2 ) |
| `number?` | Convert string to a number and flag. ( addr1 -- addr2 0 \| n 1 \| d.l d.h 2 ) Prefix: # decimal, $ hexadecimal, % binary. |
| `sign?` | Get optional minus sign. ( addr1 n1 -- addr2 n2 flag ) |
| `type` | Type line to terminal, u < 256. ( addr u -- ) |
| `accept` | Get line from the terminal. ( c-addr +n1 -- +n2 ) At most n1 characters are accepted, until the line is terminated with a carriage return. |
| `source` | Leave address of input buffer and number of characters. ( -- c-addr u ) |
| `evaluate` | Interpret a string in SRAM. ( addr n -- ) |
| `interpret` | Interpret the buffer. ( c-addr u -- ) |
| `parse` | Parse a word in TIB. ( c -- addr length ) |
| `word` | Parse a word in TIB and write it into TIB. Leave the address of length byte on the stack. ( c -- c-addr ) |

## Pictured numeric output

Formatted string representing an unsigned double-precision integer is constructed in the end of `tib`.

| Word | Description |
|------|-------------|
| `<#` | Begin conversion to formatted string. ( -- ) |
| `#` | Convert 1 digit to formatted string. ( ud1 -- ud2 ) |
| `#s` | Convert remaining digits. ( ud1 -- ud2 ) Note that ud2 will be zero. |
| `hold` | Append char to formatted string. ( c -- ) |
| `sign` | Add minus sign to formatted string, if n<0. ( n -- ) |
| `#>` | End conversion, leave address and count of formatted string. ( ud1 -- c-addr u ) |

For example, the following:

```forth
-1 34. <# # # # #s rot sign #> type
```

results in `-034 ok`

## Interaction with the operator

Interaction with the user is via a serial communications port, typically UART1. Settings are 38400 baud, 8N1, using Xon/Xoff handshaking. Which particular serial port is selected is determined by the vectors `'emit`, `'key` and `'key?`.
