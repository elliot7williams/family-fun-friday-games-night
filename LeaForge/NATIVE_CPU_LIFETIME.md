# Native CPU Counter Lifetime

This is a diagnostic/lifetime correction, not a native-startup fix, speed
benchmark or release. Milestone 146 remains open; broader readiness remains
about 65%.

## Change

The shared restricted launcher now retains its original process handle in a
SafeProcessHandle until disposal, instead of closing it immediately after
ResumeThread. Its internal two-letter Ct property reads kernel plus user CPU
time through GetProcessTimes. It does not reopen a process by PID. The handle
is parent-only; the child's exact three-pipe inheritance list is unchanged.
Failure paths and ordinary disposal close the retained handle.

Jp startup reporting uses that exact native identity independently of the
managed Process wrapper. A real zero reading remains zero. Missing, closed,
invalid or failed counter readings become cpu=unavailable, not fabricated
cpu=0 ms. Numeric readings use invariant formatting. The 512-character bound
and all startup/execution deadlines, AppContainer/Job/mitigation protections
and remote-script restrictions remain unchanged.

Microsoft documents the required query rights and 100-nanosecond CPU units in
[GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes).

## Evidence

The final identical test EXE on the preceding runtime measures 140ms through
an independent native handle, deliberately disposes the managed Process
wrapper, and reproduces the old cpu=0 ms report. The candidate passes all
17 focused lifetime checks in both linked launcher implementations, including
exit/disposal, exact CPU equality, file-grant cleanup and unavailable reports.
The bounded, guarded CPU-work helper is a test executable, not an engine
dependency or a browser fallback.

Seven report-format checks and 18 simulated launch-failure checks pass.
The latter's shell verifier incorrectly expected 17 checks; its actual exit
was zero with one 18-check completion line and empty stderr. The original
verifier result and independent read-back are retained without a rerun.

The single unobserved broad run passes all 1,755 existing scripting checks
and eight new assertions, then fails the new CPU fixture's second-worker
exit/CPU assertion. Its failing raw counters were not logged, so its cause
is unresolved. It is not a passing 1,772-check gate. The earlier fixture's
positive CPU/exit assertion failure is also retained without inferring its
unlogged counter values.

A separate observation, one child per linked launcher, records normal exits
and exact independent/Lea readings of 156ms and 125ms. It does not replace
the failed broad gate. An initial observation helper failed before launch
because PowerShell boxed reflection arguments; both helper records remain.
Final test source adds raw exit/timestamp/kernel/user/stderr logging before
the unchanged assertion. The final source passes its new identical-binary
negative control and 17 focused checks; no second broad run is substituted.

The full required renderer smoke and actual standalone browser EXE lifecycle
smoke pass. One separately copied, observed Firespider script case passes with
242 samples, eight worker identities, zero observer/sample errors or drops,
and a retired sampler. Three fresh host captures and the standalone capture
are visually inspected. This is not the full 43-case host matrix, six actual
EXE startup checks or a new installed browser release.

The initial legacy compiler could not parse existing project source.
Its failed log is retained; installed SDK builds complete without warnings
or errors. Only Lea.Win.dll and Lea.Script.dll change among 19 production
records: each grows by 512 bytes. The required Engine/Win/Image bundle is
746,496 bytes. The interpreter, worker, bootstrap and public alias API do not
change. Neither short aliases nor smaller archives establish runtime speed.

Evidence, exact runtime inventories and source-backup metadata are under
H:\Lea Evidence\Lea-146-native-cpu-lifetime-2026-10-09. Full source ZIPs remain
under H:\Lea Source Backups; ZIP entry streams are checked against source
hashes. Original Firespider inputs and installed binaries are not overwritten.

## Still Open

- Capture and explain the CPU fixture failure under the broad workload.
- Re-establish the complete scripting and Firespider release gates.
- Explain the earlier intermittent five-second native startup failures.
- Continue modern-web compatibility and secure remote-script policy work.
