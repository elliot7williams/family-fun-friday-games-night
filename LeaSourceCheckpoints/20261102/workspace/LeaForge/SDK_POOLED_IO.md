# Request-Scoped Pooled I/O Experiment

2026-11-01. REJECTED by unchanged full-suite phase18 thread-affinity contract.
Earlier focused passes/pilot observations below are historical, not adoption proof.
The session-retaining experiment remains rejected; see SDK_SESSION_IO.md.

## Rejection And Regression Backstop

Full candidatev6 uses rebuilt outer entry host with exact parent pin, same native
worker and companion, all six manifests/embedded fixture audited and60 sources
compiled without assertion changes. Entry proofDC4A7F6C...,assemblyC53A9E35...,
compileBBBEA754...,staging68EE4771...,resource audit461A6719...; scripts pin full
hashes. Build entry4067ms/full9444ms, clean0/inputs unchanged/owners retired0.

Full run continuity-evidence/sdk-full-script-owned-run-v6/result.json SHA256
7D5AC6977FC19F4BF558BF6435EEF37FAAA7122259BD6458437DB37131B1E0B3:
phase18 ParentIo.Reuse fails "parent I/O reuses one thread across sequential
operations" (5threads/32actions). Existing next assertion also requires a
dedicated background thread outside owner/shared CLR pool; reset/disposal must
retire it. Pooled design violates this contract regardless of narrow latency.
Phases0..17 completed,14828ms/300s,exit1/no timeout/kill,Job0/outputdrained,
retirement150ms,bytes+ACLunchanged,trace restored,originalowner retired0.
Retain all source/binaries/failure evidence. Do not weaken/rename/hide assertions.
Mature parent906E/adapter1C3425 and Firespider remain unchanged.

SdkParentIoContractMain copies the relevant three invariants into a small early
gate via Test-SdkScriptLocal -HostConfig -Worker -ParentIoContract, optionally
-PooledIo. It dispatches32 actions through actual private Jp.Io, checks one
dedicated non-pool thread, then disposal retirement; no native worker is staged
or launched in this mode. Original120s/30s guards and file checks unchanged.
Positive baseline receipt6C60C07DBB5EB9D7924C8F409EADEE7615CAA2C87F2701080E35634F5FCF34D8
PASS3,compile813/test1027ms. Negative pooled receipt
4D50ACD42E78C22C476D717605D6985C0FF4F6ECA3ABB787B769C0FC42FF8164
correctly fails thread reuse (4threads),compile602/test1361ms. Both inputs stable,
Job0/drain/no timeout/kill/ownerretired0. Not a substitute for full qualification.

Next return to mature-parent SDK qualification/startup diagnosis. A future I/O
optimization must preserve dedicated-thread lifetime AND no idle owned dispatcher
after request completion. Balanced repeats of this rejected prototype are not a
promotion priority; no broader performance, startup fix or full SDK claim.

## Design

New-SdkPooledIo projects five scoped edits inside the mature private IoQueue.
It keeps the existing capacity-one BlockingCollection, serialized callbacks,
TaskCompletionSource error propagation, and owner-side deadline/state checks.
Instead of creating a private Thread whose consumer waits between actions, it
schedules a finite queue drain through CLR ThreadPool.QueueUserWorkItem.
The callback returns when no work is queued. Stop completes adding and disposes
the collection when no drain remains scheduled. Pending work follows the
existing drain-before-dispose ordering under the queue gate.

Jp.Run's original unconditional finally StopIo remains unchanged. There is no
session-owned idle dispatcher after completion, new static/session pool, field
rename, worker reuse policy change, public API, or global thread-pool tuning.
Native launch/Job/ACL retirement, protocol validation and resource/deadline
limits are unchanged. These are source properties, not verified runtime claims.

Blocking pipe work can occupy pool threads. Microsoft documents starvation risks
for blocking work on shared pools; this experiment must be tested under parallel
sessions and cancellation rather than assuming pooling is automatically faster:
[Thread-pool starvation](https://microsoft.github.io/vs-threading/docs/threadpool_starvation.html),
[QueueUserWorkItem](https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool.queueuserworkitem?view=netframework-4.8).
No global MinThreads/MaxThreads adjustment or security fallback is proposed.

## Source Evidence

Prepare-SdkScript -Net48 -PooledIo stages28members at
I:\LeaBuildStorage\sdk-script-20261101-pooled-io-v2.
Manifest9744A314A7CC630B995BF4FA76A1234083BEB65DEB9A9EE2C4F183A8CC25CF15.
Candidate6.cs AACAA8A24FD81811182FAED14CC7E02AC189F3C1159E968AA381526576294DE4;
original5CC754F6DDF643811F57637D0BBBC1526955AFC47D210EC00AC2FFD19F1E3FAC.
The replacement journal reverses all five edits exactly, preserving mixed source
newline conventions outside edited spans. Three negative controls pass and the
original completion-retirement statement is verified unchanged.
V1 preparation refused mixed newlines before projection; partial source stage
is retained. V2 maps normalized match offsets back to original byte-equivalent
text spans, not an unrelated whole-file formatting rewrite.

Build wiring: Test-CoreViewSdkBuild -Version3 -DotnetSdk -ScriptLibraries
-ScriptNet48 -ScriptPooledIo, with the usual retained foreground-owner flag.
Build passed after verified project-only cleanup restored C: above600MiB.
Proof I:/LeaBuildStorage/core-view-sdk-build-20261031-v3-script-libraries-net48-pooled-io/result.json
SHA2568730DBD801165B3E815A54777E35F4628D143AC608C0A66F160CBE7CFCC64DB0.
Compile14570ms/120000ms,exit0/no timeout/kill,Job0/outputdrained,retirement191ms;
inputs unchanged,owner retired0. ParentAFA6D95F21EC093F20F88177FD0B3710B956B1775958317C4BE6ED09864341B6;
adapter3337594CC43BAD8A0BBE5993B26D518F9E2C4DB2BFDD967D45B0FD1B9B265DC4.
Compilation is not runtime correctness, speed evidence or host adoption.

## Focused Runtime Evidence

Fresh NTFS folders under continuity-evidence; original30s test/120s compile
guards, resource caps, six-file native worker and host configuration preserved.
Test-SdkScriptLocal -HostConfig -Worker -PooledIo selects this pinned build;
-Adapter and -PooledIoLifecycle use separate fresh evidence paths.

- sdk-script-pooled-io-runtime-v1: FAIL cold native startup, hello phase,
  wall5021ms/cpu140ms,launch743ms/probe154ms. Worker trace reached events;
  exact blocking cause unproven. Receipt
  C41941C2D016B616B9069599A33EF1673D0BF96E30025FB654E86201B52C6A27.
- Unchanged parent906E sdk-script-worker-runtime-v4: PASS25, test4963ms,
  3C8FBCDBE7E87C035F77B41B1DB2C22FD694B5E3D57598B106B70C87CE42D519.
- Same pooled parent sdk-script-pooled-io-runtime-v2: PASS25,test5454ms,
  FF0A0DF9BDC982C16D0AC226E57FA47800BE578D5380F77838283361F63AA6D8.
  Includes both original no-idle-owned-dispatcher assertions. This later pass
  does not resolve/erase the first failure or prove cold-start reliability.
- sdk-script-pooled-io-lifecycle-v1: PASS20,test777ms,
  0C06A0C1C8B29A2B959AE1542138A455F916B7696270B3A6ABA430A4455D2F55.
  SdkPooledIoMain directly reflects the private queue: admission capacity one,
  serial completion, exact action error, subsequent use, pool-thread execution,
  idle drain return, empty/active stop, rejection after stop, admitted work drain,
  collection disposal and two independent concurrent queues. Explicit handshakes
  and bounded waits; no ThreadPool global tuning. Not a native cancellation or
  pool saturation test. Failure paths release handshakes and stop/drain queues.
- sdk-script-pooled-io-adapter-v1: PASS26,test6873ms,
  6E0BF5E7C6AEDC708928376FBC65661386C9A02149AF942DC1DD2C07506F98D3.
  Actual WinForms view: opt-in binding, inline/ready/load/click, supplied code,
  microtasks/timers/frames, source bounds, original worker handle retirement on
  navigation, dormant script-free page, revocation, detachment and disposal.

All above compile/test jobs retired with active count0/output drained, no
outer timeout, inputs unchanged and exact worker ACL restoration. Failed first
test exits1; successful tests exit0. Each original foreground owner exited0.
No speed claim, full-suite pass, remote scripting enablement or Firespider
binary change. Next native cancellation/late completion under pooled contention
and responsiveness before benchmark/full-suite promotion decisions.

## Native Cancellation And Pilot

SdkPooledNativeMain adds real native stop and permission-revocation scenarios.
It first starts an isolated session, holds two unrelated shared-pool callbacks,
then starts code that mutates the DOM title before an infinite loop. The owner
pumps a real WinForms message loop. Only after the DOM handshake does it stop
from a foreign thread or revoke permission. Both must finish before1000ms,
without changing original2000ms execution/5000ms startup/outer30s guards.
Original worker handles must exit; dispatcher/endpoint/pending cleanup clear;
captured queue must finish late I/O and dispose; title and exact ACLs checked.
Callbacks are released/joined and handle references released on failure too.
No pool Min/MaxThreads changes; two blockers are bounded pressure, NOT saturation.

First nativev1 completed stop69ms, then failed because its manually installed
UI context was removed between scenarios. Receipt89FE24CF30E31601A948014AD95E081B57CE59EF564705CAEED718CC34782576 retained.
V2 adopts the existing adapter test's Application.Run/timer-host pattern:
continuity-evidence/sdk-script-pooled-io-native-v2/result.json SHA256
89732237035943C3B0B43EDD632083158632E832497B595EEA09BD4504A7DBAF.
PASS25,stop131ms/cancel and revoke126ms/off; compile1035ms/test5788ms,
exit0/Job0/drained/no outer timeout, inputs+ACLstable, ownerretired0.
This does not prove every cancellation race or resolve earlier cold-start failure.

Exploratory sequential baseline->pooled comparison uses the unchanged32-request
DOM-mutating test, both with retirement after each request. Each PASS37, no
uncounted warmup calls; first execution timing retained separately. Both keep
the native worker/isolation/resource limits, original deadlines and ACL cleanup.
Audit-SdkPooledBenchmark reads back receipts, all pinned inputs, six worker file
identities, source hash, original guard durations, exact footer and sample order.

| Case | Mean ms | Median ms | p95 ms | Max ms | First execution ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| Baseline906E | 10.3224 | 4.3374 | 48.6702 | 57.3247 | 3546.4297 |
| PooledAFA6 | 3.3388 | 1.3799 | 4.7530 | 54.6855 | 1692.9362 |

Exact unrounded values/samples in I:/LeaBuildStorage/
sdk-pooled-io-pilot-20261101-v1/result.json SHA256
D8F10E3FB009F3C23652B3E8B4721D00A080E1727C74E630A76C4E3313BFCF7F.
Case receipts baseline sdk-script-io-benchmark-baseline-v3
20E68BC89146BAA62991BA0288690C893D90EA6B4ED9930E241620371917329D;
pooled sdk-script-pooled-io-benchmark-v1
E6D2067F5120195690E132643A5B75841FA0B5D68031AA24719577C3F6552D0C.
One ordered pair is not a repeatability/statistical/browser-wide speedup proof;
do not attribute first-use differences to pooling or erase failed startup.
Next balanced repeat and full matched89-phase suite before any promotion.

## Historical Pre-Rejection Checklist

- Build and pass the25-check worker gate, including both no-idle-dispatcher checks.
- Verify bounded queue admission, serial ordering and propagated action errors.
- Verify Stop before/after scheduling, active native I/O cancellation and late
  completion, permission revocation, disposal and exact ACL restoration.
- Exercise independent concurrent sessions and parent thread-pool contention,
  including timer/state-observation responsiveness and fail-closed cleanup.
- Run actual adapter workflows, controlled baseline comparison without warmups,
  and the unchanged full89-phase/3412-check campaign at original deadlines.

C: observed about652MiB free after cleanup. Original600MiB build preflight and
512MiB execution reserve still apply; do not weaken them. STORAGE_CLEANUP.md
records verified archives; source/current candidates/quarantines were preserved.
