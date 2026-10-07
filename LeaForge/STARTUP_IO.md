# Startup I/O and Post-Parse Diagnostics

Milestone 146 remains open. Broader lightweight-browser readiness stays about
65%, a project estimate rather than a standards conformance score. This change
fixes cancellation/state observation during blocked asynchronous worker I/O;
it does not resolve intermittent interpreter startup deadlines.

## Parent Lifecycle Fix

Previously, disabling scripting or navigating while a worker pipe read was
blocked could wait until the startup deadline. If the child never finished,
the parent could report `timeout` instead of the actual `off` or `stale` state.
A real native off case took 3,746 ms after permission revocation.

`Jp` now checks owner-thread state before starting I/O and after its outcome.
An asynchronous wait observes state every 50 ms while preserving the original
single deadline-versus-I/O task. The periodic observation does not restart the
deadline. A late producer result cannot replace an already-selected timeout
while the UI thread is paused. Synchronous waits also recheck state before
returning their result or timeout; they do not gain a UI message pump.

Revocation reaches the existing cleanup path, which retires the child Job and
its exact file leases. Native launch still has to return before its ownership
can safely be released; this is not a promise of immediate cancellation while
the OS launch itself is blocked. Observation latency also depends on the owner
thread being able to run. No timeout, CPU/memory bound, isolation protection,
network denial check, or file capability is relaxed.

## Post-Parse Boundary

The worker attaches a one-shot Jint `BeforeEvaluate` callback around the same
production `Evaluate(string)` call in both tracing modes. It records
`startup parsed` without enabling debugger stepping, and removes the callback
in `finally` before page scripts execute. Warm execution retains the startup
report and emits no second post-parse marker. There is no new page-visible
function or interpreter dependency.

This marker observes Jint's post-parse evaluation hook, not a sampled native
stack. The interval before it includes parser and entry/JIT costs. An absent
marker does not prove that parsing alone is still running. A captured marker
followed by no `startup dom` narrows the unfinished region but does not explain
why it was slow. See the exact-version primary sources:
[Engine](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Engine.cs) and
[DebugHandler](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Runtime/Debugger/DebugHandler.cs).

## Verification

Retained root logs use the `Lea-phase-` prefix. Failed runs remain evidence,
not discarded retries or proof of a common cause.

- `io-red.log`: the preserved v16 parent library returns `TimeoutException`
  instead of `off` in the controlled blocked-producer regression. Its baseline
  inventory and library hash are retained under
  `dist/Bench/Lea-146-phase-io-baseline`.
- `io-green.log`: all 16 controlled checks pass for permission-off, navigation,
  ordinary deadline, and late completion while the owner is paused. Producer
  cleanup is verified separately; this test does not launch a native child.
- `off-red.log`: a real native worker reports `off` only after 3,746 ms, failing
  the new observation bound. `off-green.log` passes all four checks at 127 ms;
  `stale-green.log` passes four checks at 79 ms. The latter invalidates during
  launch, so it is not substituted for the controlled blocked-I/O coverage.
- `red.log` reproduces the missing post-parse marker; `focused.log` passes all
  eight marker/isolation/default/one-shot checks in normal and traced modes.
  The earlier `red-build.log` is a test setup compilation error using an
  inaccessible Options member, repaired before the executable regression.
- `full-final.log` completes startup state and phase checks but fails isolated
  selector recovery while starting a replacement worker: 5,010 ms wall and
  1,046 ms CPU, with `startup parsed` captured and no DOM completion marker.
  The planned full count is 926; the failed run is not a 926-check pass.
- `worker-final.log` stops after 59 passing checks at a warmup assertion that
  did not record its result. The test now retains status/message/startup details
  at that assertion; `worker-diagnostic-build.log` verifies its compilation,
  not the missing native hard-deadline gates. `ui-final.log` fails its initial
  native warmup at 5,023 ms wall / 421 ms CPU with `startup evaluate` last.
- Final production, test-only and browser builds compile without warnings or
  errors. The read-only `parent-job.log` query observes no CPU rate control on
  the calling PowerShell process's immediate Job. It is not evidence about the
  complete parent hierarchy or the worker's scheduling cause.

## Experimental Candidate

Separate v17 is compiled from current production sources. The first preserved
copy is `H:/Lea Builds/Lea-146-startup-io-v17`; H: is exFAT, so its scripting
test correctly fails closed at the ACL-volume preflight. This is a setup
failure, not an interpreter timeout. A 478-file hash-verified identical copy is
retained at `L:/Lea Builds/Lea-146-startup-io-v17`, separate from the live
`L:/Firespider Internet` installation. See its `ntfs-copy-manifest.json`.

The NTFS candidate passes the six actual-EXE ordinary startup scenarios. The
first launch overlaps the end of the copy audit, so these are functional
observations, not a controlled performance comparison. The separate NTFS
scripting gate (`host-script-ntfs.log`) fails at its first worker startup:
5,019 ms wall / 421 ms CPU, with `startup evaluate` last. No release ZIP or
installed-browser replacement is authorized by these results. The complete
43-case host matrix and extracted-package/source-rebuild gates are not passed
for this candidate. Earlier v14/v16 passes do not transfer to v17.

Executable SHA-256:
`CC26741F79055C632676D36D949D6A7588AAC883B20ADD1D3126A18449BAF0FC`.

The system drive has less than 20 MiB free during these observations. An
attempt to relocate old generated folders was blocked before execution; no
source or preserved artifact was removed. Storage pressure is recorded as an
environment limitation, not asserted as the cause of the startup timeout.
Clipboard host-gate failure and wider compatibility/security work remain open.
