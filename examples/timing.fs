\ checking timing loops using ms and us
\ loop with no delay, for baseline
marker -timing \ test fastest loop through code
: time_0 ( -- ) \ 4.63us/loop or a period of 9.27us or 107.9kHz
	D3 output
	begin
		D3 toggle
	again
;

\ looping using a millisecond (ms) delay, 
\ adds 1ms, subtract 1 for a specific delay so 0ms is a 1ms loop, 1ms for 2ms
\ 0 ms: 1.00ms/high, 2.00ms/period
\ 1 ms: 2.00ms/high, 4.00ms/period
\ 2 ms: 3.00ms/high, 6.00ms/period
\ 3 ms: 4.00ms/high, 8.00ms/period
\ 5 ms: 6.01ms/high, 12.01ms/period

: time_ms ( -- ) \ very accurate timing, can range down to 1ms/loop
	D3 output
	begin
		D3 toggle
		dup ms
	again
;

\  0 us: unstable, can't use
\  1 us:  6.52us/high, 13.03us/period => 5.52us overhead
\  2 us:  7.52us/high, 15.04 us/period => 5.52us
\  3 us:  8.52us/high, 17.04 us/period => 5.52us
\ 10 us: 15.54us/high, 31.07 us/period 5.52us
\ 20 us: 25.57us/high, 51.38us/period => 5.57us
: time_us ( -- ) \ 
	D3 output
	begin
		D3 toggle
		dup us
	again
;

: time_ct ( -- ) \ very accurate timing, can range down to 1ms/loop
	D3 output
	begin
		counter @
		dup ms
		counter @ - .u
	again
;
