# First-demand worker observation

## Scope

This checkpoint changes test harnesses, not production runtime behavior.
`Lea.Build/WorkerObservation.cs` is compiled into both the native script tests
and the Framework-compatible Firespider boot harness. The native CLI remains
`--startup-observe`; Firespider gains the optional `-Observe` / `-obs` report.
Ordinary host tests do not instantiate the observer or enable startup tracing.

Firespider observation surrounds only the existing first
`ScriptCode(view, "var retainedState=41;")` call. With `-Case all`, the 42
non-script cases run first and only the final script case receives the report
argument. The remaining script assertions continue unchanged after observation.
Report `flowPassed` therefore describes the watched first demand, not the
entire script case or host matrix; the wrapper log proves their results.

Example commands, using a new report filename in an existing directory:

```powershell
& 'L:\Lea Development\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe' `
    'L:\Lea Development' --startup-observe 'H:\Lea Evidence\new-native.json'
& 'L:\Lea Development\Lea.Build\Test-FirespiderBoot.ps1' `
    -Root 'C:\Users\Elliot\AppData\Local\Lea\HostDiagnostics\Lea-146-demand-observe-v25' `
    -Case all -obs 'H:\Lea Evidence\new-host.json'
```

These are diagnostic commands, not a release installer or a cold-cache benchmark.
Do not rerun a failed case to replace its evidence with a later pass.

## Collection

- Owner-thread timer requests counters every 125 ms for at most four views.
- A single background sampler has one pending slot and retains 512 samples.
- Each process is checked against the exact worker path, parent PID and native
  PID while its limited-query handle remains open for all reads.
- CPU time, working set, commit, page faults and I/O counters are cheap reads.
- Thread snapshots are attempted at most once per 1,000 ms for each
  PID/creation-time identity, with 32 identities and 16 threads as hard bounds.
- Thread ownership is independently checked. Thread-query failures are retained
  separately rather than discarding otherwise valid process counters.
- Startup stderr markers are bounded to 160 characters; observer errors,
  exception text and serialized report size also have explicit bounds.
- Report format 2 includes `format`, `kind`, `flowPassed`, sampling intervals,
  pacing bounds, process creation identity, separate counter/thread costs,
  queue lag inputs, truncation/drop counts, and sampler retirement.
- Output uses create-new semantics. Existing files/directories, invalid names,
  missing parent directories, unsupported cases and non-script runtimes are
  rejected before running the test or compiling the host harness.
- Shutdown restores the previous timing-trace environment value. A watched
  failure still saves the report and retains the original exception as a failure.

The engine keeps its 5,000 ms startup deadline, 128 MiB process-memory limit,
2,500/10,000 Job CPU hard cap, single-process Job, AppContainer, restricted
handles and existing mitigations. No retry, eager worker, deadline extension,
production instrumentation or isolation exception is introduced.

## Verification

Both the SDK test-only build and the older Framework compiler succeed. Initial
Framework compiler output is copied byte-for-byte into the controlled-fault
runtime. The ordinary wrapper subsequently recompiles the unchanged harness
sources for the clean case and full matrix; the final and controlled harness
hashes are tracked separately, not claimed as reproducible compiler outputs.
Fourteen guards pass: six native CLI/path guards, six host wrapper guards and
two direct boot-argument guards. Existing sentinel output and the compiled host
harness remain unchanged; invalid outputs are absent.

Two distinct synthetic failures verify exception/report preservation:

- Native flow: a nonexistent fixture root retains `DirectoryNotFoundException`,
  exits 1, writes a failed report with zero samples, and retires its observer.
- Host flow: an otherwise identical runtime deliberately omits only worker
  `Jint.dll`; first demand retains the missing-runtime setup error, exits 1,
  writes a failed report with zero samples, and retires its observer.

Neither synthetic failure is the intermittent startup timeout. The ordinary
unobserved host clean case also passes, including all 14 clipboard-contract
checks. No clipboard access is needed by those contract checks.

| Observed flow | Existing checks/cases | Samples | Workers | Thread snapshots | Observer elapsed |
| --- | --- | ---: | ---: | ---: | ---: |
| Native view recovery | 60 checks pass | 197 | 8 | 28 | 29,420 ms |
| Host first script demand | Full 43-case matrix passes | 34 | 1 | 4 | 4,887 ms |

All samples have valid process counters; all attempted thread snapshots contain
data. Neither run drops samples/requests, truncates threads, records observer
errors, or leaves its sampler running. Minimum repeated thread-snapshot spacing
is 1,001 ms native and 1,005 ms host. Desktop 940-pixel and narrow 420-pixel
native paint captures are nonblank and inspected, including fields and buttons.

| Sampling wall cost | Native | Host first demand |
| --- | ---: | ---: |
| Counter-read mean / maximum | 0.10 / 9 ms | 0.29 / 9 ms |
| Thread-snapshot mean / maximum | 47.93 / 110 ms | 49.00 / 64 ms |
| All-sample mean / summed sampling time | 7.06 / 1,390 ms | 6.56 / 223 ms |
| Maximum queue lag | 31 ms | 26 ms |

Zero-millisecond readings mean below timer resolution, not free work. These
costs describe the sampler's measured wall time, not total observer CPU cost or
an application speedup. Earlier dense-observer measurements are not a controlled
performance comparison; filesystem cache and machine load are uncontrolled.

The host's parent report reaches ready in 4,132 ms, with launch 425 ms, probe
108 ms and worker CPU 812 ms at that report. The host first-demand stopwatch
prints 4,957 ms and includes additional host/observer work; it is not the
engine's startup-budget stopwatch. Sparse thread samples show the primary
worker thread running, but do not provide stacks or prove a scheduling cause.
Counter trends alone cannot establish the cause of the earlier timeout.

After all 43 cases passed, the external result writer failed on PowerShell
`retry=false` instead of `retry=$false`. That reporting-command error is retained
and the saved log/report are verified in place. The browser matrix is not rerun.

## Preserved state and remaining gate

Diagnostic evidence is preserved under
`H:\Lea Evidence\Lea-146-demand-observe-2026-10-08`, including the source
archive, exact test harnesses, hashes and checkpoint bundle. Its final manifest
records the sealed inventory after checkpoint verification.
Separate normal/fault copies remain under the user's local `Lea\HostDiagnostics`
directory. Their original payload hashes are checked against the prior package;
the synthetic copy's one omitted dependency is explicit.

All 19 production dependencies remain unchanged. The installed browser and the
original Firespider source are not replaced. No release ZIP, new installed EXE,
six-scenario actual-EXE verification, or new 1,261-check full-suite pass is
claimed for this harness change. That preceding full-suite pass remains valid
historical evidence for its original test executable, and the preceding fresh
package timeout remains a failed gate.

Next: use the first-demand observer to retain evidence if the timeout recurs;
investigate startup parsing/initialization and resource/scheduling conditions
without weakening protections. A successful instrumented run is not a causal
fix. Milestone 146 remains open; broader readiness stays about 65%.
