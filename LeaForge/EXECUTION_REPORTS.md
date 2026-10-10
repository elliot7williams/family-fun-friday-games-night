# Bounded native execution reports

`Jp.Ed` is the isolated session's latest execution diagnostic, separate from
its startup report `Sd`. Both use short embedding aliases. Reports are
host-owned observations, not permission authority or page-visible APIs.

| Alias | Meaning |
| --- | --- |
| `Sd` | Most recent native startup/handshake report |
| `Ed` | Most recent command's parent execution phase, wall time and CPU delta |
| `Tm` | Parent command/I/O deadline in milliseconds; default 2,000 |
| `Cp` | Native Job CPU-rate hard cap in basis points; 2,500 means 25% |
| `Mm` | Native Job process-memory limit in bytes; 128 MiB |

```csharp
using (var script = new Lea.Script.Jp(engine, page))
{
    var result = script.Ex("document.title='Lea';");
    var startup = script.Sd;
    var execution = script.Ed;
    var message = result.M;
}
```

Execution phases are `send`, `read`, `verify`, `bridge`, `reply` and `done`.
They identify the parent boundary, not the exact JavaScript statement or
worker-side cause. Wall time covers the command path after successful startup.
CPU is the difference between two readings of the exact retained child handle,
not total startup/lifetime CPU. Unavailable or decreasing counters remain
`unavailable`; an observed rounded zero is not proof of no CPU work.

The report captures the newest available bounded worker stderr. It does not
wait for a new trace or enable an external observer. Success leaves the result
message unchanged. A valid worker `js`/`limit` end frame also retains its
original message and status, while `Ed` reports the completed parent boundary.
Parent-detected timeout, transport, protocol and state failures reserve their
existing message prefix, execution summary and newest diagnostic tail within
the existing 512-character result bound. Capture occurs before retirement.

`Ed` starts empty, is cleared when a new call is accepted for validation, and
stays empty for disabled/stale/invalid-code/no-due-timer or failed-startup paths
that never enter command execution. Thread/disposal/concurrency programming
errors remain exceptions. Later calls can replace `Ed`, but already-returned
`Jr.M` snapshots remain unchanged. `Sd` is not overwritten by command failure.

No bridge operations, page access, interpreter settings, startup deadline,
task deadline, CPU policy, memory limit or native protections are loosened.

## Verification

The cancellation-report precursor rebuilds only the test executable. All 19
preceding candidate runtime hashes remain identical. Its single focused
`--startup-stop` run passes five checks: unpublished child termination,
no writes, bounded cancellation report, explicit recovery and retained
failure snapshot. Recovery startup is 4,143 ms with 546 ms total child CPU.
That narrower pass does not clear the preceding broad recovery failure or
prove its cause; the failing assertion now prints its actual recovery result.

The new execution-report candidate passes eight summary/API contracts and
71 controlled transport checks. The same compiled API contract rejects the
preceding Script assembly's missing `Ed` property without starting a VM.
Controlled transports bypass startup only inside the test assembly and make
no native-isolation claim. They cover all parent phases, unavailable CPU,
oversized diagnostic tails, deadline failure, cancellation, permission
revocation, malformed frames, transport EOF, valid limits, success,
non-execution clearing and blocked-producer retirement.

Seven separate real-worker checks pass. A 25-ms command deadline returns a
bounded `phase=read` report at 39 ms, retires the exact child, preserves `Sd`
and supports explicit fresh-worker recovery. This is observed timing from one
run, not a strict 25-ms termination guarantee or performance benchmark.
Fourteen unchanged interpreter-option checks and four exact bootstrap-resource
checks also pass. The preceding renderer proof is not rerun: Engine/Win/Image
and their test executable are unchanged, and it is not a new renderer gate.

The single broad scripting attempt fails after 175 assertions at fresh
worker startup: parent phase `hello`, wall 5,021 ms, child CPU 343 ms,
launch 307 ms and positive-control probe 52 ms. Its last worker checkpoint
is `startup vm`, before bridge setup. Command execution never begins in
that failing call, so no execution-phase diagnosis or root cause is inferred.
The failed gate is retained without retry or deadline increase.

The standalone browser passes with actual exit code 0 and expected isolation
settings. One separate copied .NET Framework 4.8 Firespider script case passes,
including fields, native events, recovery, split isolation/disposal and remote
refusal. Its helper verifies the child exit; the outer report leaves its
numeric exit code null. The full 43-case host matrix and installed EXE startup
are not rerun or claimed fixed. Four fresh captures are inspected. The
attributes capture retains a narrowed viewport with unused black area, not
evidence of polished full-window layout.

The initial staging helper stops at `Candidate exists` after successful builds
because it names an older evidence directory. No old copy is overwritten and
no native gate has started. A separate staging repair selects a fresh folder
and copies the same compiled files without rebuilding. Both that failure and
the preceding failed broad gates remain retained.

Evidence: `H:\Lea Evidence\Lea-146-execution-reports-2026-10-10`, with the
unchanged-runtime cancellation precursor at
`H:\Lea Evidence\Lea-146-cancel-recovery-2026-10-10`.
Only the candidate Script DLL changes among 19 runtime records: it is 96,256
bytes, 1,024 bytes larger than the prior candidate. Required Engine/Win/Image
remain 749,056 bytes, excluding optional interpreter/framework dependencies.
Normal production outputs and original Firespider source/installed files stay
unchanged. No default adoption, speedup, release or milestone completion is
claimed; broader readiness remains about 65%.

## CPU Policy Clarification

Some recent evidence schemas and descriptions incorrectly label `2500` as
`cpuMs` or a 2,500-ms CPU budget. The archived files are preserved, not silently
rewritten. Lea's `Jb.Add` actually sets `CpuRate=2500` with ENABLE/HARD_CAP and
verifies that policy through the native query. It does not set a Job lifetime
CPU-time limit. The current alias and numeric policy do not change.

The CPU-rate units and nested-Job behavior follow
[Microsoft's CPU-rate-control documentation](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_cpu_rate_control_information).
The rate is 25% of the applicable Job CPU budget, with ancestor Job policy
potentially constraining that budget further. It is distinct from measured
CPU milliseconds, the 5-second startup deadline, 2-second parent task default
and Jint's 1/5-second interpreter timeouts. This clarification is not evidence
that CPU throttling caused the intermittent startup delay.
