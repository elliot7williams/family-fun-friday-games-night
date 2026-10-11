# Hard-Deadline Recovery Diagnostics

Milestone 146 remains open. This checkpoint changes tests and documentation
only. All 19 production runtime records remain identical to the animation-frame
checkpoint, including default `Be=0`. Required engine/view/image bytes remain
749,568. No engine-size growth, measured speedup, startup fix, installed
Firespider replacement or stable release is claimed. Broader readiness stays
about 65%.

## Compact Check

`Lea.Script.Tests` accepts two-letter `-hr`, or `--hard-recovery`, followed by a
new report path. No extra arguments are accepted. An existing report or a
missing parent directory is rejected before page execution. The report is
created once, including when the native sequence fails; no native retry is
performed.

The native sequence deliberately follows the existing worker test's codes:

1. Warm up with `1;` using the default 2,000-ms execution budget.
2. Execute a native regex under a 25-ms parent execution deadline.
3. Restore 2,000 ms and replace the retired worker with a title mutation.
4. Warm up an empty microtask.
5. Execute the same native regex inside a microtask under 25 ms.
6. Restore 2,000 ms and replace that worker with the same title mutation.

The fixed startup deadline is still five seconds; Job memory is 128 MiB and
CPU rate is 2,500 basis points (25%), not a CPU-millisecond allowance. A call's
total elapsed time includes startup and retirement, so it is not the execution
deadline measurement. `Sd` and `Ed` separately record those parent boundaries.
The deadline assertions retain the existing less-than-two-second termination
guard. Interpreter, wire, filesystem and network protections are unchanged.

Each row snapshots result status/message/changes, `Sd/Ed`, execution budget,
before/after PID and SID, old process birth time, worker ownership, retained
read leases, idle I/O ownership, timer/frame counts and document title. Metadata
such as `Ac` can outlive a retired worker: `activeWorker` explicitly distinguishes
that retained metadata from a currently running process. Diagnostics are copied
before the next lifecycle attempt can overwrite them.

The test pins the old process handle before execution; PID reuse cannot satisfy
its retirement assertion. It verifies old-process exit and removal of that
session's exact file ACL grants, eight read leases while active and zero after
retirement, no idle parent I/O dispatcher, separate bounded replacement reports,
and final disposal cleanup. ACL/process checks run after the measured call;
these focused results are not cold-start benchmarks or a full-suite pass.

The main `--worker` test also now includes status, message, startup/execution
reports and PID in its regex, microtask and memory-recovery assertion messages.
Its script codes, budgets, assertions and expected count are unchanged.

## Recorded Results

Evidence: `H:\Lea Evidence\Lea-146-hard-recovery-2026-10-11`.

| Gate | Result |
| --- | --- |
| Test-only build | Zero warnings/errors |
| `-hr` native recovery | 18 assertions pass; six calls, two hard kills, two replacements |
| Native execution-report regression | 7 assertions pass |
| Extra argument | Expected exit 1 before execution |
| Existing report | Expected exit 1; protected report unchanged |
| Missing-worker negative control | Expected exit 1; one setup-failure row saved, PID zero, cleanup verified |
| Existing worker regression | Actual exit 1 after 45 assertions; no retry |
| Broad scripting regression | Not run after the focused worker failure |

The new worker failure is at `independent session SID and realm`, while its
first worker is still alive. The second worker reports `timeout`, parent phase
`hello`, launch 79 ms, positive-control probe 7 ms, wall 5,012 ms and CPU 31 ms.
Its last recorded stderr marker is `startup configured`. This distinguishes
a startup failure from a page execution failure, but does not establish the
cause or prove that the last recorded marker is the precise blocking operation.
No stack observer or opt-in startup trace was enabled for these runs.

The preceding full run's failure after 565 assertions remains unresolved:
its microtask replacement assertion did not record a status/phase. This new
startup failure must not be used to retroactively classify that older failure.
The broader suite still expects 2,651 assertions; a focused success does not
replace that gate. Earlier startup/EOF/CPU-helper failures are retained.

Both successful and failed gates are single attempts with actual process exit
codes. Owned candidate/control processes are verified absent afterward. Original
Firespider source, installed files and historical normal outputs are byte-checked
and remain untouched. No new browser is compiled because its runtime is unchanged.

## Reproduction

Use a new report filename in an existing directory. The separate diagnostic
executable for this checkpoint is not the installed Firespider browser:

```powershell
& 'C:\Users\Elliot\AppData\Local\Lea\EngineBuilds\Lea-146-hard-recovery-2026-10-11\Lea.Script.Tests.exe' 'L:\Lea Development' -hr 'L:\Lea Development\hard-recovery-new.json'
```

Do not write new results into sealed evidence directories. The saved source ZIP,
per-file hashes, checkpoint Git bundle and manifest preserve this investigation.
