N == 300_000_000

Results on i5-3550@3.3GHz 8Gb RAM:

SERIAL
Base stage, milliseconds: 71
Main stage, milliseconds: 5122
PARALLEL_SPLIT_BY_PRIMES
Base stage, milliseconds: 62
Main stage, milliseconds: 4335
PARALLEL_SPLIT_BY_NUMBERS
Base stage, milliseconds: 62
Main stage, milliseconds: 2860
THREAD_POOL
Base stage, milliseconds: 61
Main stage, milliseconds: 2782
USING_PARALLEL_FOR
Base stage, milliseconds: 64
Main stage, milliseconds: 2758
WORK_POOLING
Base stage, milliseconds: 62
Main stage, milliseconds: 2650
WORK_POOLING_WITH_LOCK
Base stage, milliseconds: 63
Main stage, milliseconds: 2675

Results on AMD A10-7300 Radeon R6, 10 Compute cores 4C+6G 1.9GHz 8Gb RAM:

SERIAL
Base stage, milliseconds: 164
Main stage, milliseconds: 14301
PARALLEL_SPLIT_BY_PRIMES
Base stage, milliseconds: 162
Main stage, milliseconds: 11992
PARALLEL_SPLIT_BY_NUMBERS
Base stage, milliseconds: 159
Main stage, milliseconds: 8311
THREAD_POOL
Base stage, milliseconds: 164
Main stage, milliseconds: 7958
USING_PARALLEL_FOR
Base stage, milliseconds: 162
Main stage, milliseconds: 8134
WORK_POOLING
Base stage, milliseconds: 160
Main stage, milliseconds: 8049
WORK_POOLING_WITH_LOCK
Base stage, milliseconds: 159
Main stage, milliseconds: 8273
