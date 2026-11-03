# Trusted Bootstrap Cost

## 2026-11-01 Combined First-Call External Observation

Current parent36AB/JintB546 trusted prefix first Ex remains UNWRAPPED.65external
original-handle samples/computedPASS, receipt3652E3E6...; source call/assertion/
owner-helper inverse and negatives retained,130NTFS ACLs unchanged,ownerretired0.
Diagnostic images on I:exFAT, not native isolation qualification. Selected current
PE boundary map2E8A2ABC... and independent audit3F8B26A3... verify parsing/prepared
execution/constraint call sites without loading targets.21observed phase8samples
span2131ms/46.875CPUms,+58266totalfaults/0logicalread bytes; this window INCLUDES
bootstrap/page work, NOT exclusive page execution or cause evidence. Typed native
birth conversion audit bug fixed/tested1A6E72B5...; strict identity preserved.
COMBINED_TRUSTED_EXECUTION.md has fullpins/limits/next steps. Both retained
bootstrap/worker and distinct page-execution failures remain open; no deadline fix.

## 2026-11-01 Distinct Page Execution Failure

Combined SDK parent36AB/v7 prefix campaign repeat2 FAILED at phase8 first computed
script: limit; Script budget exceeded. phase=script; reason=TimeoutException.
This is PAGE EXECUTION, not the previously observed bootstrap boundary. Bootstrap
had returned; no worker startup observer/report had been reached. Full original
phases0..7 complete,8199ms outerelapsed/300s, exit1/no outer kill/Job0/drain,
94msretire/5s, trace restored; original and fresh65runtime hashes/ACLs unchanged.
CampaignB769BE60.../read-only audit64DD3D98...; COMBINED_WORKER_STARTUP.md fullpins.
Next external original-handle CPU/memory/thread sampling around exact first Ex
without wrapping/warming the call, preserving1000ms trusted execution budget.
Confirm current Framework interpreter IL execution/constraint boundaries before
any parser/cache/preparation change. No cause/production fix/reliability closure.

## Native Thread Observation

2026-10-29 OwnedThreadSnapshot.cs reads SystemProcessInformation with checked
Windowsx64 layouts256/80bytes and bounded16MiB/6retry/record/thread counts.
Original process HANDLE/PID/creation/liveness verified BEFORE+AFTER query;
no OpenProcess/OpenThread,thread suspension,priority changes or ownership fallback.
Only matching owned process thread rows returned;other system rows discarded.
CPU/creation/context/state layout reference:
https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-tsts/e82d73e4-cedb-4077-9099-d58f3459722f
API stability/layout limits:
https://learn.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntquerysysteminformation

Test-OwnedThreadSnapshot x64 Framework control passes8checks:own native thread,
wrongPID/wrongbirth/invalidhandle rejection,CPU advancement,monotonic context
switches,known sleeping thread inWaiting,state5/reason4,and truncated-buffer
rejection. ReceiptBCC044C4CE8A57C497A7E5D13A339F8A2F3227F4457956A70650FDA5B380E41D.
Compile524/run1581ms,cleanup234/52ms,both0/no timeout/kill,Job0/drain,inputs
unchanged,owner63273 guarded clear/retired0. Control spin300ms reportedCPU15.625ms;
do not infer CPU/wall causality before independent clock/counter cross-calibration.

Test-FontThreadCounters reuses exact earlier diagnostic executable/runtime READONLY,
rehashes65files and checks original ACLs before+after;no new60MBcopy/child code.
Original process owner preserved;one inverse-checked parent sampler hook with
3negative controls,100ms cadence/512cap/30srun/5scleanup. Native sampler added to
parent only;system query may perturb scheduling,and kernel latency isn't guaranteed
by allocation/retry bounds. Native samples don't identify managed owning-thread ID.

Raw font-thread-counters-v1 receipt
BDA2E33D337CB78BCF8A086C3F5A8485142B05B1678F5EDEC966292A718AA112:
66samples,computedN6PASS,17prefix lines/one stop,run7931ms/cleanup111ms,exit0,
no timeout/kill,Job0/drain,all65bytes/ACLs/inputs unchanged,owner27572 retired0.
Audit6FBB5721BCAB10206AF9EBF01B1948C9B9D0AA15B0F3C25F2AC2527098CFE581
verifies native process birth/monotonic thread identity/counters and marker bytes.
Earliest-thread CANDIDATE17932 (not proven managed owning-thread) has25samples
after markerbyte520,5172..7810ms,allstate2 Running. ReportedCPU375ms/contextswitch
delta507,maxquery35.44ms. No sampledWaiting in that window;WaitReason while not
Waiting is not interpreted. This does NOT rule out waits between samples,prove
continuous CPU execution,diagnose scheduling or fix the earlier full failure.

NEXT independently cross-check thread CPU with GetThreadTimes/cycle counters and
arithmetic/sleep controls before interpreting CPU/wall discrepancy. Preserve
budgets/guards/original process ownership,no unchanged full retries. Full60D6C8E2
failure/legacyUIA and all6 final gates remainOPEN. Source checkpointv64.

## Exact Framework Interpreter Mapping

2026-10-29 Audit-FrameworkInterpreterMap/FrameworkInterpreterIlMap reads PE IL,
does NOT load/execute interpreter. Actual Framework rootJint B54618A4 matches
NuGet4.16.4 lib/net462/Jint.dll BYTE-FOR-BYTE (packageE920276A...). Nuspec and DLL
ProductVersion identify commit884bd0c4ebf7806f70041eae0370ee11a9c138bc;retained
interpreter-source-v1 proofD013E5CC pins same commit/archive8D80E70A. Seven relevant
source files rehashed/copied,including Jint/Extensions/Polyfills.cs. This is local
package identity +repository/source pins +selected IL congruence,NOT independent
package-signature verification or a complete reproducible source-to-binary proof.

V1 partial directory retained:stopped on initially wrong Polyfills relativepath.
V2 verifies5selected IL checks;V3 adds3configuration checks,all8pass. New map
includes Engine constraint methods,StackGuard,MemoryLimitConstraint,TimeConstraint,
GCPolyfills,Options+ConstraintOptions,OptionsExtensions.LimitRecursion and actual
parentE685 Js constructors/Start,Vo.Of/At. Source parent8/10 pins verified.

Selected compiled facts:
- TimeConstraint.Check uses GetTimestamp/deadline,not CancellationTokenSource;
  a threadpool timeout callback is not the timeout mechanism here.
- Memory check calls per-thread allocation counter;GCPolyfills caches its getter.
  No GetTotalMemory/forced full GC in these checked methods.
- Stack headroom backstop calls TryEnsureSufficientExecutionStack.
- RunOnEmptyStackCore DOES contain Task/WaitOne,but that lane is not requested by
  the default MaxExecutionStackCount=-1 configuration. Vo.At retains enabled stack
  guard and doesn't set handoff limit;LimitRecursion changes MaxRecursionDepth,
  not MaxExecutionStackCount. Do NOT equate method existence with observed waiting.
- ParentStart still Evaluate(prepared syntax);none of these findings proves the
  timeout cause or permits changing guards/deadlines.

No runtime/settings changes,full failure60D6C8E2 remains. Next bounded owning-thread
CPU/wait/context-switch observation with identity-validated read-only thread handles
while original process/Job ownership remains unchanged. No PID-based process
reopen/retirement,guard disable,prewarming or unchanged full retry for green.
Do not edit NuGet cache/frozen source;any justified fix needs staged source and
matched qualification. Map sources/receipts in font-framework-interpreter-map-v3;
source/evidence checkpointv63. All6 final gates OPEN.
Upstream versioned source reference:
https://github.com/sebastienros/jint/tree/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint

## Current Font Pair External Counters

2026-10-29 Test-FontBootstrapCounters preserves exact0B09/A8CC components and
60source/65runtime layout;only Program post-computed-assertion stop is added by
pinned New-ExternalBootstrapProgram (original call/assertion/prefix retained).
No child sampler/wrapper/warming call. Parent samples through BORROWED original
OwnedTestProcess private SafeHandle,DangerousAddRef/Release per query,no PID
reopen. Pinned OwnedTestProcess unchanged;Invoke-OwnedTestRun gets renamed and
one parent-only sampling hook via exact inverse/3negative controls. All original
job/output/deadline/retirement guards remain. Prefix30s,compile120s,cleanup5s,
trusted1000ms unchanged.512sample cap/50ms interval;observer can perturb scheduling.

Raw font-bootstrap-counters-v1 receipt
0A5F0A16D34A3F566EE47B02A317C05BF47452C865330A8DE54A600A1841F345:
127samples,computed assertionPASS/N6 and1explicit stop,phases0-7complete/8begin.
Compile15213ms/prefix7984ms,both0/no timeout/kill,cleanup112/24ms,Job0/drain,
all65candidate bytes/ACLs and inputs unchanged,owner30253 guarded clear/retired0.
NOT a fullsuite pass or correction of earlier60D6C8E2 failure.

Audit-FontBootstrapCounters validates sample birth timestamps against original
process birth,monotonic counters/time/observed stderr bytes,fixture prefix and
compile-recipe provenance against pinnedE874 build. Initial audit comparison
lost timestamp precision via DateTime-to-string Parse;fixed by direct DateTime
cast,raw data unchanged. V1audit retained;V2 strengthens recipe binding.
Marker end byte520 brackets arrival at parent5614..5678ms;lastsample7947ms.
Conservative before-arrival-to-last window2333ms/125msCPU/+62437pagefaults,
logical reads0/writes205bytes. NOT exclusive bootstrap/Evaluate timing:includes
delivery/sampling delay,page execution and assertion/teardown boundary effects.
Global max query15.542ms (first query),minimumavailable RAM2622259200bytes.
All sampled working-set flags10(0xA):hard min/max disabled,not an enforced1.4MB
maximum (resident set reaches43MB). API reference:
https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-getprocessworkingsetsizeex
No working-set/priority/GC/guard/budget policy changed. Pagefault counter does not
identify hard vs soft faults;logical read0 does not rule out paging. Cause unknown.

NEXT map actual Framework Jint B54618A4E3FFE33344D478D3A1580658C4D91819B3B97A24326CB3626CB7FD1A
to exact source/IL before editing constraints/wait paths. Do not assume rootJs.cs
or arbitrary tested-interpreter source is the loaded parent/runtime. ParentE685
Start is pinned early-limits-v1/source/8.cs (not root prototype),prepares syntax,
then Evaluate with1000ms. Consider bounded native-thread wait/counter observation,
preserving prefix;no guard disabling/unchanged full retries for green. Model/render
font passes remain scoped,legacy UIA and all6 final release gates remain OPEN.

2026-10-29 currentfont0B09/A8CC fullscript attempt also fails phase8 computed
trusted bootstrap with lastdocumentcheckpoint. Raw60D6C8E2/failureauditFBF16B8A
in temporal-startup-v1/font-script-owned-run-v1. ExactparentE685/resourceEC4B
confirmed,phases0-7complete/8begin,not exclusive document cost or cause proof.
Original1000ms trusted budget unchanged,no outertimeout/kill/ACL delta. No new
fontpair fullpass. FONT_SHORTHAND.md has full pins;older findings below historical.

Updated 2026-10-25. The numeric-core full suite still fails at its first computed
trusted-Js call with a bootstrap/events TimeoutException under the unchanged
1000ms interpreter budget. This is separate from net8 worker hello and outer
suite deadlines. See NUMERIC_REFERENCES.md for the original failure receipt.

## Current Pair And Checkpoint Meaning

The next bounded deep-frame control now passes18rows, receipt
`framework-deep-guard-cost-v1/result.json`, SHA256
`379BC440CFB2A631188BC805B24C15AA43081FF5F0B24FC8247B1756CCBF255A`.
Two rounds at synthetic depths0/64/128, padded non-inlined frames,10000 calls per
empty/stack/allocation row. All measured rows below1ms and CPU counter resolution;
stack rows total1fault, allocation rows2faults; initial empty row115faults. Inputs
held, childexit0/no timeout/kill, ownerretiredclean. No interpreter/security changes.
It does not reproduce the61k-fault pattern or establish actual interpreter stack
shape, so disabling or amortizing guards is not justified by these measurements.
Rendering work proceeds independently; bootstrap reliability remains open.

The current core52CE/viewB2C1 full scripting failure0856EF52 is at the first
computed trusted call, with last marker=document. This marker must not be read
as an exclusive measurement of document construction. Exact PE-resource audit
`current-bootstrap-checkpoint-v2/result.json`, SHA256
`EA55E0A130E9F17295956519494992B0CF1FFC8111D88996955654AFED0DA9B9`,
reads the embedded bootstrap from parentE685 without executing it, verifies
resourceEC4B6513 and the pinned parent build/source2/8, plus a wrong-resource
negative control. Document/window setup precedes checkpoint(document); bridge
global deletion, dispatch closure creation and interpreter completion follow it.
Source Js.Start sets stage=checkpoints only after Evaluate returns. Thus the
failure is a late-bootstrap observation, not proof of a slow document operation.
An initial audit had a reserved PowerShell variable naming error; partial V1
artifacts/failure record remain retained. V2 does not modify runtime or limits.

Current matched same-prefix probe `temporal-startup-v1/current-flow-bootstrap-cost-v1/result.json`,
SHA256 `CA628F5355BD3BED7C79E11C88679F13BD600C3513E6CD9CF640304A4852C304`,
captures a six-mutation success:2228ms wall/93ms process CPU,2393608 owning-thread
allocated bytes (includes wrapper),61225 all-page-fault delta, one Gen0 increment,
zero logical read/write-byte delta. Sampled evaluate starts996ms; declarations
1183,scope1254,events1505,elements1642, stage reset already observed1657ms. Document
was too short to sample. Remaining call time includes the separate page operation.
All samples/counter brackets valid, observer joined,17 expected prefix phase lines
and one explicit stop,62 compiled sources,65 runtime files/ACLs unchanged.
Foreground owner retired0/zero leases/unverified=false. Original1000ms Evaluate
budget remains unchanged; total call wall is not an extended interpreter budget.

These observations resemble older successful probes but neither clear the real
failed full execution nor identify hard faults, paging, GC or scheduler causality.
No optimization adopted. Next use bounded controls to distinguish deep interpreter
stack/guard behavior from allocation/working-set effects; earlier shallow guard
microbenchmarks do not establish deep-stack behavior. Do not disable guards,
extend budgets or rerun unchanged full suites merely for passing evidence.

Verified local `current-bootstrap-evidence-v1.zip`:89entries/7124092bytes,
SHA256 `979DAC770EBDDE8160566841FD72F6D002FCF43178D3A74F0621A383A3D3E293`.
Contains exact diagnostic sources/resources, logs, recipes and nested matching
source/failure archive. Not portable delivery/off-device backup/release approval.

## Same-Context Probe

The test-only `BootstrapExceptionObserver.cs` observes the first computed call's
managed first-chance events, retaining the original assertion, prefix and budget.
It limits detailed records to24, types to16 and counted events to10000 before
invalidating the diagnostic, filters the owning thread, counts other-thread events
separately, suppresses recursive observation and removes its handler in finally.
No sampling thread or interpreter changes. It still warms execution and adds cost.

`bootstrap-exception-cost-v1/result.json`, SHA256
`0698BFCFFC721F86742C87CFF23F311C3558FB6F717CD0118FF9B4A244ED4B37`:
computedOK/N6, firstcall2194ms/109.375msCPU/+61084faults; zero managed events on
either thread category, no callback errors/overflow.17phase lines/1stop verified,
compile15167ms/probe7334ms bothexit0/no timeout/kill,398inputs/65runtimebytes+ACLs
held. Owner47854 exited0/leases0/unverifiedfalse.4supportsource copies byteverified.
This is no evidence that failed runs cannot throw or that native exceptions are
absent; it narrows the managed-exception-storm hypothesis for this successful call.

Independent Frameworkdeliverycontrol `framework-exception-delivery-v1/result.json`,
SHA256 `A1D23ED7CF156BD5C5557F51386CD34EB32E9D5B582FA7D21FE7A1B4B5EACED6`,
passes1own/1other caughtApplicationException, handlerremoval check and2s threadjoin.
Compile9879ms/probe574ms bothexit0/inputsheld; owner53307 exited0/leases0/unverified.
An initial WindowsPowerShell -File attempt was blocked by its script policy before
executing the control; no policy change was made. The separately compiled Framework
EXE control was used instead. No browserrelease or startupfix inferred.

Isolated .NET Framework guard control `framework-guard-cost-v1/result.json`, SHA256
`B5935776796D3780846E80A9D1D996449DFBED656B36A4A918E6421A7672FE33`,
compares two rounds of10000 empty-loop, EnsureSufficientExecutionStack and owning-
thread allocation-counter calls. First round0/0/3ms, faultdeltas11/1/112; second
round all0ms/0faultdelta. CPUdeltas0 at available resolution, no logicalI/O orGen0
increment. This does not reproduce the observed faultstorm or justify disabling
either guard. It does not measure deepstack/interpreter behavior or establish a
causal startup fix. Compile8866ms/probe666ms bothexit0/no timeout/kill; inputsheld,
owner session13922 exited0/leases0/unverifiedfalse. Probe/helper sources retained.

External diagnostic tooling is implemented. `Test-ExternalBootstrapTools.ps1`
passes eight checks against the pinned original Program.cs: reversible stop-only
overlay, five malformed/repeated-input rejection cases, native counter sanity and
invalid-handle rejection. Receipt `external-bootstrap-tools-v1/result.json`, SHA256
`4E74FEF6F9BBC68FF0419DFA5EF3F472F79DC5D0508887748245B2CB352DA810`.
The native helper reads the supplied handle without reopening a PID and reports
no managed GC data (which would belong to the observing process). This validation
uses the observer's own held process handle. It does not replace or clear the
failed full suite.

`Prepare-ExternalBootstrapCost.ps1` derives a separate runner from V4, retaining
the pinned process utility's deadlines, output bounds, original-handle ownership
and retirement. Only the test Program stop is compiled into the matched60-source
target; counters execute in the parent, at the existing roughly100ms polling loop,
with1024sample cap and errors recorded without discarding process ownership.
External V1 receipt `external-bootstrap-cost-v1/result.json`, SHA256
`6AD669297BAA9BFB0AF7BF8FD206A97ADF3D86E91F3C5194BE660838D41D59E2`:
computedPASS,17phase lines and exactly1stop, compile18904ms/probe8311ms bothexit0,
no timeout/kill,70samples/no errors/overflow.395input hashes and65runtime files'
bytes/ACLs unchanged; owner session59094 exited0 with0leases/unverifiedfalse.
Four support sources are byte-verified backups alongside the held runner snapshot.
Original1000ms interpreter budget unchanged; original failed fullsuite still open.

Across the entire observed process, CPU0->6562500ticks, faults593->86337,
logicalread0->37026850bytes. These are not exclusive bootstrap deltas or hard-fault
classification. Availablephysical minimum2225545216bytes, memoryload maximum73%,
working-set flags10 throughout (hardmaximum bit absent). Sampling does not rule
out transient memory pressure or outer policy/scheduling. First query14.7923ms,
mean0.32365ms, so external observation also has overhead. No causal fix inferred.

`Test-SameContextBootstrapCost.ps1` compiles the exact matched 60-source suite
plus `BootstrapCostObserver.cs`. Only Program.cs changes: the first computed
call is wrapped in a read-only observer, and an explicit diagnostic argument
stops after the original computed-result assertion. The preceding phases 0-7,
fixture, interpreter/parent/core binaries and assertion remain unchanged. This
is not the full suite. It rejects later phases and a successful call without
the expected stop marker.

The observer samples the existing startupStage field, wall time and process CPU
on a separate bounded thread. Polling is 10ms, unchanged-stage records at most
every 100ms, 128 records, 10-second observation ceiling and 2-second join.
Sampling affects execution and CPU includes the observer; these are observations,
not exact exclusive stage measurements.

V2 captured 26 records, a successful six-mutation computed result, the correct
prefix/early stop, and clean observer/parent retirement. Inputs and 65 candidate
files/ACLs stayed unchanged. Compile exited 0 in 16039ms; probe exited 0 in 9513ms,
no outer timeout or kill. Original owner39546 retired with zero leases/unverified
child state. Receipt: `same-context-bootstrap-cost-v2/result.json`, SHA256
`45B88EFE80EB5778220AB41B1836CB7B20100FF4149D8D125C1BDEBC20912399`.

First observed transitions (milliseconds after starting the observation):

| Stage | Wall | Process CPU |
| --- | ---: | ---: |
| vm | 89 | 0 |
| bridge | 204 | 0 |
| prepare | 256 | 0 |
| evaluate | 942 | 15 |
| declarations | 1095 | 15 |
| scope | 1162 | 15 |
| events | 1462 | 31 |
| elements | 1600 | 31 |
| document | 1618 | 31 |

Total call wall2193ms/CPU78ms includes VM/preparation, bootstrap and a separate
page operation. It does not mean the 1000ms interpreter limit was extended.
Unobserved/short stages are not missing functionality. One successful diagnostic
does not clear the failed full run or establish cold reliability.

The low reported CPU relative to wall time motivates original-handle I/O/page
fault/scheduling observations and counter validation in this same prefix. It
does not establish which cause dominates or justify changing source semantics.

## Native Counter Follow-Through

`NativeProcessSample.cs` reads the original current-process handle using
[GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes),
[GetProcessIoCounters](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprocessiocounters)
and [GetProcessMemoryInfo](https://learn.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-getprocessmemoryinfo).
No PID reopening or process/job setting changes are used.

Standalone .NET Framework calibration passed all five conditions: CPU advancement,
managed CPU within native before/after brackets, at least 1 MiB logical I/O in
each direction, monotonic counters, and invalid-handle rejection. Approximately
109.375ms native CPU work took702ms wall; total control723ms. This is a controlled
counter check, not a browser benchmark. Receipt `native-counter-calibration-v1/result.json`:
`F0AE623CEC0F54807BE7DC349B4F82BFC15B77F9EFA3A44D6C805DC33DE89165`.
Original owner49852 retired cleanly, no guard or interpreter execution.

The revised same-prefix V3 probe keeps the same stop,60s guard and1000ms
interpreter options. It captured a successful six-mutation call in2240ms wall /
125ms CPU, with all managed/native CPU brackets agreeing. First observed evaluate
988ms, scope1226ms, events1502ms, elements1642ms. Logical process read/write byte
deltas stayed zero; total page faults rose61176, working set35872768->43245568
bytes. Counter sampling at most0.139ms in26records. This does not distinguish hard
faults, soft faults, mapped-file paging, GC activity, or scheduling delays.
The copied top-level runtime DLL/EXE attributes were Archive, not Compressed;
this says nothing about system DLLs or the full memory environment.

V3 receipt `same-context-bootstrap-cost-v3/result.json`:
`193212EAF3845FEEFB43FB421BA202E700DE153879B6512C1618B988FAA30032`.
Inputs and65candidate bytes/ACLs stayed unchanged; original owner26926 retired
with zero leases/unverified state. This remains a diagnostic success, not a fix
or a passed full suite. Next distinguish GC/allocation/working-set behavior and
validate scheduling evidence before changing the bootstrap implementation.

A separate read-only current tool-host observation found immediate job flags0.
It is not a measurement of the retired test process or outer jobs. The
[job query API](https://learn.microsoft.com/en-us/windows/win32/api/jobapi2/nf-jobapi2-queryinformationjobobject)
returns the calling process's immediate job when passed NULL; no claim of
unrestricted scheduling follows from that observation.

## GC And Allocation Controls

2026-10-31 continuity readback: owned-thread-cpu-control-v1/result.json reports
35 passing calibration checks, inputs unchanged,ownerAttention=false; SHA256
5AEB13701E38BF595CADCB1E18991F4C6C2837A5A5C2DB217085F128C3C3CE0B.
GetThreadTimes and native snapshot CPU readings agree in arithmetic/spin/sleep
controls. Arithmetic rounds:309/308ms wall,62.5ms CPU each. Spin:309/308ms wall,
31.25/15.625ms CPU. Sleep:320/322ms wall,zero reported CPU. Cycle counts are
not converted to time. This corroborates counter agreement,NOT the cause of
bootstrap timeout or unrestricted scheduling. No retained pwsh test owner was
present in the2026-10-31 process inventory. Full candidate qualification remains
failed/open; do not transfer diagnostic success to the full suite.

The counter helper now records process-wide collection counts and sampled managed
heap estimates, using `GC.GetTotalMemory(false)` without requesting a collection.
The optional allocation API is resolved once and read only on the script-owning
thread before/after the call, not on the observer. It measures total managed
allocation on that thread, including wrapper overhead, not retained heap or
native allocations. See [.NET allocation counter semantics](https://learn.microsoft.com/en-us/dotnet/api/system.gc.getallocatedbytesforcurrentthread).

CalibrationV2 passes the existing CPU/I/O/invalid-handle conditions plus monotonic
GC counters and owning-thread allocation advancement. The allocation counter is
available on this machine. Receipt468022F6...; owner47886 retired cleanly.

V4 same-context diagnostic: computed resultOK/N6,2158ms wall/62ms processCPU,
owning-thread allocation2223128bytes. Observed collection counts stayed2/1/0
through sampledbootstrap stages; Gen0 became3 after startupStage cleared. That
is not proof of absence of all GC pauses. Heap1306456->3218216, working set
35819520->43274240, total faults+61240, logical read/write bytes0. Maximum counter
sampling0.1222ms. Correctprefix/stop and observer retirement, all65candidate
bytes/ACLs/inputhashes unchanged. Original owner68970 retired cleanly.
Receipt `same-context-bootstrap-cost-v4/result.json`:
`2A70EC6F295E01EA87AE247083FEB8390FB68A583FE8AAEBAB921A0AF605368D`.

Standalone calibrationV3 adds a main-thread idle control, not JavaScript. Over
2001ms for a requested2000ms sleep: faultcount+1, allocation+232bytes, nativeCPU
ticks0, logical read/write bytes0, GC0/0/0 unchanged. Existing calibration checks
still pass; owner68505 retired cleanly. Receipt `native-counter-calibration-v3/result.json`:
`EEF0D73284BFD7ED25E5670DC7A52AD7C5A9816070A40B9571391693D7602545`.
This differs from the bootstrap's high faultcount, but is not a same-sampler idle
experiment or proof that the sampling cannot affect behavior.

Next use an external original-handle observer around the same prefix, keeping
the target's first call unwrapped. In-process instrumentation may warm runtime
paths, so its repeated successes do not resolve the original failure. Query
working-set policy/memory pressure where possible; do not force collections,
trim working sets, change priorities/limits or relax the1000ms budget.

## Retained Harness Failure

V1 was invalid: PowerShell parsed a concatenated replacement as a third array
item, so its early stop was absent. It collected a sample, continued to phase27
and hit the diagnostic's 60-second guard. The parent was killed, its original
handle observed exit, and streams drained. Raw receiptD4A5D9A3... remains failed.
The original owner was retired only after zero process leases and zero currently
running instances of the candidate executable names; review572CF1BF... explicitly
does NOT certify original descendant-handle audit, crash/profile cleanup or any
release gate. Candidate permission changes and the failed run remain preserved.

`New-BootstrapCostProgram.ps1` fixes this with explicit two-item edits and stop,
assertion and inverse checks. `Test-BootstrapCostProgram.ps1` passes six real/
negative overlay checks (receipt4B6BAE33...). V2 validates exactly phases0-7 plus
phase8 begin, and rejects accidental continuation. No production source, budget,
permission policy or failed full-suite result was changed.
