# Startup I/O and Post-Parse Diagnostics

Standalone cleanup building block now tested: OwnedTestProcess.cs uses original
Job/process handles and atomic JOB_LIST. Eight controls7FB338C9 and real exact638
host-script DB8AF857 pass with Job active0, original exit and complete drain.
No runtime/ACL changes; normal owners retired safely. Not full-suite integrated
or an intermittent-startup fix; see OWNED_TEST_LAUNCH.md for exact scope/next step.

Reporting repair integrated after verified user reboot2026-10-26 (CIM boot
18:56:21 local). Test-CurrentNativeScriptSuiteV2 now starts restoration=false
for in-process tracing and verifies in finally even when unsafe Run throws.
Nine actual-finally AST controlsD020176E pass, including pinned V8 replay and
original-error preservation, missing/duplicate/wrong-path records, incomplete
stream/exit and unavailable helper. Raw V8 result unchanged. No full-suite rerun
or stronger descendant-cleanup claim; that ownership work remains open.

Earlier source-only reporting repair: Read-AllStartupTraceProof.ps1
passes15 controls5AA231AB including pinned V8 process/stderr evidence. Forced
stops cannot certify restoration even with valid markers; absent/duplicate/
reversed/unknown markers, incomplete streams and unobserved exit fail closed.
Not integrated into the locked runner before user restart; no native process,
guard clearing or input-lease release. Integrate in runner finally after reboot,
including the path where Run throws. Raw V8 result remains unchanged.

2026-10-26 fullV7 receipt83047075 fails removal startup5015ms/cpu281ms before
parsed. Parent PowerShell trace environment was correctly restored but removed
from child by the pinned shared Run sanitization. Do not label this all-worker
timed tracing or weaken sanitization. Explicit NullString restores absent values
instead of empty strings;12controls2C2F35DD pass; V1 false-restoration artifact
retained. Outcome9controlsB1EBFE66 pass. New inverse-checked diagnostic Program
Main overlay enables tracing inside test process, restores in finally and emits
activation/restoration markers. Five source controls9FE6B8A4 pass; buildCF5B99D8/
bindingC804E827 passes60-source/65-file audit. Worker
60FF unchanged. Deliberately clean child probes retain explicit sanitization.

FullV8 BD9A11F7 completed phases0..85, original300s forced parent stop in86.
Timed Main activation marker exists; restoration marker does not. Raw result
traceEnvironmentRestored=true is a runner default left when unsafe Run throws
before post-Run verification. Read-only correctionAC1EEA16 records this without
editing raw result. Both authored paint contracts pass in prefix at940/420px,
but no footer/full pass and no claim prior intermittent startup is fixed.
Inputs/bytes held;7 ACL deltas (Image + six worker files) retained, not waived.
Original parent exit and stream drain proven; descendant retirement NOT proven.
Owner44601/pwsh14000 stays at prompt with unverified guard/252 input leases.
User agreed to restart after source backup. No new native/browser tests, no guard
clearing, no relaxed deadline or unchanged green retry. After restart fix
fail-closed reporting and strengthen owned forced-stop cleanup first.

Latest2026-10-26 fullV6 receipt4E396AF0 fails phase55 isolated receivers startup
5020ms/cpu171ms after parsed/declarations/scope. No outer timeout/kill; normal60FF,
original5000ms/300s, inputs/bytes held, one Image ACL delta, owner80179 retired0.
Inverse-checked Tree observation captures35valid samples, ready624ms/cpu31ms,
trace restored. Summary6CA93AE0 uses reader with15controls319E0CE8. Reader V1
missed blank-line binding; V2 fixes it and adds the negative/positive coverage.
This successful changed diagnostic prefix does not fix prior failures. Next
capture timed stages across all worker launches, not one test at a time. Observer
scope text says removal historically; build observedStartup=Tree and exact source
inverse identify actual target. No worker/engine implementation changed here.

Previous2026-10-26: normal fullV5 receipt7CB1C9A2 fails phase24 native tree
startup at5003ms/cpu62ms, captured subscribed marker. No outer timeout/kill,
inputs/bytes held, one Image ACL delta retained; owner49712 retired0 safely.
Diagnostic fullV4 receipt7F31548C passed native removal with38valid samples,
ready684ms/cpu109ms and immediate trace restoration. Success in that changed
diagnostic run does not resolve earlier startup failures. Next observe the
tree startup in the full prefix, not a warmed focused retry.

Milestone146 remains open. The earlier65% was a historical project estimate,
not a measured current completion score. The original change below fixes
cancellation/state observation during blocked asynchronous worker I/O; it does
not resolve intermittent interpreter startup deadlines.

## Current Observation

2026-10-26 exact worker60FF read-only IL audit551E62C1 shows evaluate trace before
callback creation, debugger subscription and Evaluate itself. A last captured
marker is not a native stack; stderr is drained asynchronously. Failed fullV3
23E8711E still records phase28 startup5012ms at unchanged5000ms deadline.

Focused native-removal observation46DE39A3 preserves assertions and original
worker/parent bytes, uses supported timed trace, samples the original retained
SafeHandle (AddRef/Release, no PID reopen), restores host trace flag and joins its
observer. Eleven functional checks pass;159 monotonic samples valid. Ready at
worker3570ms/cpu140ms. Canary388->network2132ms, configured2604->vm3073ms,
evaluate3084->parsed3257ms. Captured samples have a different clock origin and
asynchronous latency. Sampling max9ms;9593 all-page-fault delta and69046 logical
read bytes do not identify hard faults, disk I/O or cause. This is a successful
different prefix, NOT reproduction or resolution of fullV3. Raw audit retains
one decoder ACL delta. SetupV1/V2 failures are preserved separately.

The exact8.0.32 compile pack removed by system update is restored project-locally
from official NuGet; all163 DLL hashes match frozen inputs. Managed17-source
rebuild686A5457 and native9DCA4B73 reproduce83FE/60FF byte-for-byte with current
pinned compiler. No SDK installation, production replacement or security change.
The split diagnostic is now complete below; fullV3 remains unresolved.

## Split Probe Diagnostic

Two copied source overlays add before-constructor and entered/client/connect
timing markers using the worker clock. New-WorkerNetworkProbeSource.ps1 inverse
checks normalized originals;9 source controls5017D955 pass. Original connection,
denial500ms wait, cleanup and guard statements remain. Managed buildEF8067EA
produces057ACEA0; native3E1D1894 produces03C0B40A with diagnostic IL unchanged.
Resources/references match baseline; constructor signature is diagnostic-only.
These binaries are NOT normal worker60FF or production performance changes.

Fresh private removal run7DBA5056 passes11 functional checks and63 valid monotonic
samples; observer joins. SummaryE66E1615 records worker-clock entry4ms after outer
network-before, TcpClient construction29ms, BeginConnect initiation19ms, ready
664ms/cpu125ms. This does not reproduce the prior1744ms constructor interval,
prove a speedup or explain the failed fullV3. Instrumentation changes layout and
adds timing/observer cost. Async capture timestamps are not exact event times.
Inputs/runtime bytes held; raw audit retains one decoder ACL delta. Owners all
retired0/no leases/unverified child. Parent/view/interpreter and original startup
5000ms/outer90s unchanged; no security bypass or warm retry.

Next observe first native-removal startup within the failed full-suite prefix,
with preceding phases and NORMAL60FF intact, in a fresh diagnostic-only candidate.
No speculative network rewrite is justified by the successful split observation.

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
# Owned Full-Suite Checkpoint 2026-10-26

Run334A4567 passed3412 checks/all89 phases in289304ms under300s with no kill or
timeout. Original exit/Job0/complete output drain247ms; owner35298 retired0.
Trace activation/restoration verified, bytes/inputs held. Raw ACL audit remains
false for one decoder grant; read-only6331FD2A accepts unchanged policy. This
does not resolve prior intermittent startup failures or prove profile cleanup.

