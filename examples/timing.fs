\ checking timing loops using ms and us
\ 0 ms: 1.00ms per high, 2.00ms per period
\ 1 ms: 2.00ms per high, 4.00ms per period
\ 2 ms: 3.00ms per high, 6.00ms per period
\ 3 ms: 4.00ms per high, 8.00ms per period

\ 9 cwd commands in us
\ 0 us: unstable
\ 1 us: 7.15us per high, 14.29us per period => 6.15us oh
\ 2 us: 7.64us per high, 15.3 us per period => 5.64us oh
\ 3 us: 8.65us per high, 17.3 us per period => 5.65us oh
\ 10 us: 15.66us per high, 31.33 us per period 5.66us oh

marker -timing \ test fastest loop through code
: time_0 ( -- ) \ 4.64us per loop or a period of 9.27us or 108kHz
	D3 output
	begin
		D3 toggle
	again
;

: time_us ( -- ) \ 4.6us per loop or a period of 9.2us or 108kHz
	D3 output
	begin
		D3 toggle
		10 us
	again
;
