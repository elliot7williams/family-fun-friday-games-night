# Native Worker Startup Observation

## Result And Scope

The opt-in `--startup-observe` diagnostic runs the existing native `View.Run`
recovery flow without retries, changed deadlines, or changed production code.
Its first complete run passes all 58 existing checks and two observer checks.
Eight worker processes produce 266 samples over 37,883 ms: 265 successful
samples and one retained process-exited sampling error. The sampler retires,
with no dropped requests, dropped samples, or UI-side observer errors.
Both 940px and 420px paint captures are nonblank and visually inspected.

A separate ordinary full source run, with both observation and timing trace
disabled, passes **all 1,261 checks** through the same test executable. This
includes the 90 listener-invocation checks. The earlier isolated-tree and
native-recovery startup failures remain retained failures, not reclassified
passes. One later pass does not establish reliable cold startup or a timeout fix.

All 19 production dependencies remain byte-identical to the listener-invocation
checkpoint. No new release, host candidate, or installed replacement is made.
Milestone 146 remains open; broader readiness remains about 65%.

## Bounded Observer

- A 125 ms owner-thread timer locates only this test's open Lea views and their
  currently attached worker PIDs. It queues the newest captured startup line,
  bounded to 160 characters, without executing page code.
- A background consumer performs native sampling through a one-request queue.
  At most 512 samples and 32 bounded UI-side errors are retained; overflow
  counts remain visible. At most 16 threads are retained per sample.
- Before reading counters, a limited-query process handle verifies the exact
  expected worker image path, PID, and this test process as parent. That handle
  stays open for the whole sample. Thread timing handles independently verify
  the owning PID before querying CPU and creation times.
- Samples include process CPU, page faults, working set, commit charge, I/O
  counters, thread states/wait reasons, sampling duration, and queue timestamps.
  Query failures are retained rather than converted to successful observations.
- The original startup tracing environment value is restored on exit. Reports
  use create-new semantics, and the original test exception is rethrown after
  its report is saved. The diagnostic is not added to the normal full suite.

The native limited-query timing/ownership contracts are documented by Microsoft:
[GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes),
[GetThreadTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getthreadtimes),
and [GetProcessIdOfThread](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocessidofthread).
Process identity uses `NtQueryInformationProcess`; this test-only native API can
change across Windows versions and query failures are reported.

## Observed Markers

These are the first sampled final startup-probe lines for each PID, not complete
parent launch-to-handshake durations. Earlier or brief phases can be missed.

| Worker PID | Samples | Probe Marker Wall / CPU (ms) |
| --- | ---: | ---: |
| 62592 | 75 | 4177 / 875 |
| 63148 | 34 | 2967 / 531 |
| 55140 | 32 | 2850 / 531 |
| 62864 | 24 | 2073 / 203 |
| 50064 | 23 | 2168 / 250 |
| 60140 | 24 | 2430 / 421 |
| 59896 | 22 | 1994 / 265 |
| 62356 | 32 | 2010 / 296 |

The retained error is `Process with an Id of 62864 is not running`, observed
during worker retirement. No sample exceeds the thread cap. Sampling itself
takes 27-111 ms, median 42 ms, mean 43.37 ms, totaling 11,537 ms of background
sampling wall time. Maximum queue lag is 35 ms. This is substantial observation
overhead, not measured observer CPU consumption or time added to the UI thread.

No timeout occurs in this observation. Thread-state snapshots are not stack
traces, cannot prove a deadlock, and do not identify why an earlier run stalled.
Counters are read sequentially, while the queued phase predates those reads.
Page faults do not distinguish soft from hard faults; I/O counters do not prove
physical-disk delay. Caches, machine load, and scheduling are uncontrolled.
Consequently, neither this pass nor these timings prove a speedup or root cause.

## Reproduction And Guards

Build the test project against the existing production outputs, then run:

```powershell
$env:TEMP = 'L:\Lea Tool Cache\temp'
$env:TMP = $env:TEMP
& 'L:\Lea Development\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe' `
  'L:\Lea Development' --startup-observe `
  'H:\new-native-observation.json'
```

The report's parent directory must exist and the report must not already exist.
Four final-executable guards pass: missing report argument, extra argument,
existing report, and missing report parent. Existing contents remain unchanged
and invalid paths create no output.

A controlled nonexistent fixture root exits 1, saves the engine file-read
failure and inner `DirectoryNotFoundException`, retires the sampler, and rethrows
the original failure without awarding an observation pass. The first external
verification incorrectly expected a later load assertion; it failed because
the missing fixture throws earlier. That verifier result is retained. Inspection
of the same log/report verifies the actual expected file-read failure without
rerunning or changing the harness.

The final test executable SHA-256 is
`9F49FED3BBAB8D9823C851328EA7CBB5DE9D47C5AB08641A11A521767CD5BB94`.
Both this diagnostic and the ordinary full suite use those exact bytes. The
final test-only build has zero warnings and errors.

Evidence lives under `H:/Lea Evidence/Lea-146-startup-observe-2026-10-08`:
`observed-view.json`, `observation-analysis.json`, desktop/narrow captures,
`failed-view.json`, both failure-verification results, guard logs, and
`full-source.log`/`full-source-result.json` preserve the measured scope.
The ordinary full run finishes at 2026-10-08 14:15:08 UTC. Environment snapshots
are not simultaneous telemetry of any earlier timeout.

The next startup investigation still needs evidence from an actual stall. A new
Firespider candidate also needs verification against the current bootstrap;
older v23 host passes do not transfer to these production bytes.
