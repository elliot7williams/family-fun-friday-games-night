# Direct Selector Worker Observation

Milestone 146 remains open; broader browser readiness remains about 65%.
This is test-only diagnostic coverage, not a production startup fix, engine
speedup, new browser release or installed replacement.

## Diagnostic

The existing selector startup diagnostic can now sample its direct synchronous
`Jp` session through the same native sampler used by WinForms and Firespider:

```powershell
Lea.Script.Tests.exe . -so 'H:\new-selector-report.json'
Lea.Script.Tests.exe . --selector-observe 'H:\new-selector-report.json' 'C:\existing-worker-directory'
Lea.Script.Tests.exe . --selector-observation-contract
```

`-so` writes the existing per-call report and a companion `.samples.json` file.
Both paths are validated before work and opened with create-new semantics.
The original `-ss` / `--selector-startup` command remains available without
native sampling. No page-facing API or production dependency changes.

One background consumer polls a published session without UI pumping. Each
snapshot holds the session lifecycle lock while pairing the PID and latest
bounded endpoint phase; native queries still verify the exact executable path,
PID, parent and process creation identity. Counters and paced thread timing use
the existing limited-query handles. UI observation retains its four-view,
one-slot queue behavior. Direct mode reports one session and zero views.

The nominal poll interval stays 125 ms, with counter/thread query cost added to
direct-mode cadence. There is no guarantee of capturing every short-lived
worker or phase. Existing 512-sample, 32-error, 16-thread and one-second thread
pacing bounds remain. Stop retires the consumer and restores timing trace.
Original selector exceptions are preserved even if either report output fails.

## Verification

Final revision B builds without warnings/errors and passes 19 deterministic
contracts: no-UI polling, retirement/idempotence, trace restoration, empty
sessions, supplied-path retention, create-new output, unrelated-process
rejection before counters/threads, locked PID/phase pairing, text/count bounds
and expected callback failure reporting. Revision A's JSON-reader type error
remains preserved; B uses the structured reader's `IList` array shape.

Six CLI/path guards and two controlled original failures pass. Existing main
and companion report sentinels stay unchanged, invalid outputs stay absent,
and missing fixture/worker cases retain failed per-call and sampler reports
with zero samples and a retired consumer. Both command spellings are exercised.

One final direct selector run passes 26 checks, including bridge exhaustion,
worker retirement and genuine fresh-process recovery. Its initial/recovery
workers have distinct PIDs and verify AppContainer, guard mask 15, denied
network/files and no loopback exemption. The sampler retains 45 samples and six
thread snapshots across two workers, with no errors, drops or thread truncation.
Ready walls are 3,062 and 2,211 ms; these are instrumented observations, not
cold-cache measurements or a speed comparison. No timeout occurs in this run.

The ordinary unobserved full scripting suite passes all 1,746 checks once with
this same final test executable and a genuine zero exit code. It includes the
19 new deterministic contracts, but does not include native observation as a
normal gate. The suite's existing intentional missing-interpreter test reports
FileNotFoundException before its successful completion summary; that is not an
unexpected startup failure. This full pass does not replace the failed observed
Firespider result below.

The existing native-view observation passes 60 checks, with 184 samples and
27 thread snapshots across eight workers. It has no errors, drops or thread
truncation, retires, and produces inspected nonblank desktop/narrow captures.

## Captured Firespider Failure

A fresh diagnostic copy retains v33's host EXE and nineteen production files.
Only its test harness is recompiled against the changed shared sampler. The
unobserved clean case and existing 12 observation/14 clipboard contracts pass.
The planned whole-script observed case instead fails `scripted split pane loads`.
It is retained as failed, without a manual retry or substituted passing gate.

Both split workers hit the unchanged startup deadline. Parent result snapshots
retain `startup parsed`, but no completed `startup dom` or verified hello for
those attempts:

| Split | Native wall ms | Native CPU ms | Worker parsed wall ms | Parsed CPU ms |
| --- | ---: | ---: | ---: | ---: |
| Left | 5055 | 531 | 3955 | 500 |
| Right | 5033 | 953 | 4073 | 796 |

Worker-relative clocks begin inside Main and exclude earlier CLR/native launch
work. The sampler retains 257 samples and 63 thread snapshots across nine
workers, including ten/nine samples for the failed left/right processes. There
are no counter/thread/observer errors, drops or thread truncations. Its total
sampling wall time is 1,632 ms, maximum 69 ms; this is observer cost, not measured
observer CPU or time added to the UI thread.

Main-thread snapshots are often Ready, with increasing CPU; one left snapshot
is Running after parsed. These are not stack traces, cannot identify a JIT/AV/
scheduling/CPU-control cause, and do not prove a deadlock or a fix. The right
sampler's last captured phase is evaluate; its parent's failure snapshot later
retains parsed. Subsequent lifecycle calls create other workers that become
ready, but do not erase the original typed failures or pass the split case.
Fresh main/attribute captures are inspected; no fresh split capture is produced.

The failed observation now covers a later boundary than the earlier retained
evaluate-before-parsed timeout. Further investigation needs stack/scheduling
evidence during that region, or a justified measured bootstrap improvement.
No production allowance is enlarged to force a pass.

## Preservation

All nineteen production records remain byte-identical to `fdbb03f`. Native
5,000-ms startup, 25% CPU, 128-MiB memory and interpreter/Job/AppContainer/origin
guards are unchanged. The required Engine/Win/image bundle remains 745,984
bytes. Core rendering, original host inputs and installed binaries are untouched.
No new full native-core, 43-case host, six actual-EXE startup, extracted-package
or independent source rebuild gate is claimed for this diagnostic checkpoint.

Initial artifact-freeze ordering and host TEMP/runtime path collisions stop
their helpers before real cases; both are retained separately from actual
runtime results. Final source, both test revisions, exact production/runtime
copies, reports, logs and captures are retained under:
`H:/Lea Evidence/Lea-146-selector-worker-observation-2026-10-09`.
