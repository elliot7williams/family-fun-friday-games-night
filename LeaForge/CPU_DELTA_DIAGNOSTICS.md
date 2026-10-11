# Controlled CPU Work Delta

Milestone 146 remains open and broader readiness stays about **65%**. This is
test-only diagnostic work, not a startup fix, speedup or stable release.
All 19 production runtime files remain byte-identical to the nonblocking-agent
checkpoint. Original Firespider source, installed binaries and normal build
outputs are not changed.

## Measurement

The controlled CPU helper now retains its initial and latest CPU samples, wall
time and completed spin count. On its existing exit-3 timeout it emits one
bounded stderr report. Trace mode also reports completed work; successful
quiet mode emits no new report. The five stdout phase bytes are unchanged.
The loop still requests 100 ms of additional CPU, uses SpinWait(10000), and
retains its three-second wall guard. Reporting happens after the result is
decided; no observer, background sampling, retry or longer budget is added.

The samples are process CPU ticks, not wall time. The .NET Framework
[Process reference source](https://github.com/microsoft/referencesource/blob/main/System/services/monitoring/system/diagnosticts/Process.cs)
reads GetProcessTimes for TotalProcessorTime rather than caching its CPU value.
Microsoft documents kernel/user CPU durations in 100-nanosecond units in
[GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes).
That source does not explain why an earlier helper exhausted its wall guard.

CpuDelta.cs is linked into the test executable and its controlled helper only.
It formats invariant numeric fields, avoids invalid/overflowed subtraction,
and distinguishes a genuine zero delta from an unavailable delta. Its actual
outcomes are fixed `complete` and `timeout` strings; the numeric report fits
within 512 characters. It is not a new engine API or dependency.

The parent still uses its existing asynchronous stderr drain. An absent report
is not proof that the helper never entered Main. Independent native exit and
total CPU assertions remain unchanged and are not replaced by self-reporting.

## Checks

The test-tool build succeeds with no warnings or errors. Eight new report
controls cover completion, a synthetic below-target timeout with positive
total CPU, zero, invalid/decreasing samples, extreme numeric bounds, the exact
100 ms threshold and culture-independent formatting. Synthetic values are
formatting controls, not measurements of a real timeout.

- Quiet native CPU lifetime: **25 checks**, actual exit zero.
- Traced EOF/held-exit cleanup: **41 checks**, actual exit zero.
- Focused native startup phases: **16 checks**, actual exit zero.
- Four AST-only observer count/footer controls pass; no inspector is launched.
- Three captured helper deltas and quiet-success report suppression are checked
  independently against the retained logs.

The full observer default is now 2,823 checks, previously 2,815, to include the
eight report controls. Worker-only mode still expects 87; explicit count
overrides still work. This updates expected metadata, not a passing full gate.

The traced helpers all record a loop delta of 1,093,750 ticks, or 109.375 ms:

| Launcher | Parent Total CPU (Whole ms) | Loop Wall (ms) | Completed Spins |
| --- | ---: | ---: | ---: |
| Startup | 171 | 558 | 8,100 |
| Lea.Script | 125 | 891 | 12,179 |
| Lea.Win | 140 | 1,560 | 25,720 |

Startup CPU and loop CPU are different intervals. These successful measurements
do not reproduce or reclassify the preceding exit-3 failure, whose logged
140 ms was total process CPU and whose loop delta was not recorded.

## Unresolved Gates

One quiet full regression exits 1 after **66 passing assertions** at the first
trusted computed-script check: `limit; Script budget exceeded. phase=bootstrap;
reason=TimeoutException`. It fails before this run reaches the CPU lifetime
fixture. The 2,823-check completion is not reached; no retry substitutes for
the failure. The exact bootstrap operation responsible remains unknown.
The separate passing startup suite does not establish intermittent reliability.

A fresh copy of the unchanged browser EXE passes its counter, Start/Pause,
three-frame, elapsed-time and isolation smoke. Captures show the bar grow from
40 to 90 pixels with 189 ms displayed after pausing. The original browser
deployment is not modified. This is not a new browser build or Firespider
startup matrix, and no installed engine is replaced.

Evidence is under `H:\Lea Evidence\Lea-146-cpu-delta-2026-10-11`. An initial
evidence-script boolean syntax error is retained with its source snapshot;
the corrected contract verifier does not rerun the native gates. Complete
source ZIPs remain under `H:\Lea Source Backups`, with every entry verified
against source hashes. Release gates and the older native-startup/CPU failures
remain open; production deadlines, isolation, memory and CPU-rate limits are
unchanged.
