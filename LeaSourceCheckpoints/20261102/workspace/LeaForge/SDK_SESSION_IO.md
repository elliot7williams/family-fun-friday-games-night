# SDK Session I/O Prototype

2026-11-01. REJECTED for production: the full suite requires completed native
requests to retain no session-owned parent I/O dispatcher. This prototype
violates that resource-lifetime contract. It was never adopted into Firespider
or the SDK package. The narrow latency measurements below remain experimental
evidence, not an eligible optimization or browser-wide speed/reliability claim.

## Rejection And Backstop

Fullcandidatev5 integrates parent0425/adapter8D1F and rebound entryhost14B51A92,
parent identity inverse/negative controls, fresh fixture path and unchanged
workerF8BE/probes. AssemblyED384D34..., compileFF5A3E89...60sources/193refs,
7689ms/120s clean0/retirement120ms; testEXED01E16AF... . FreshNTFS staging
03BC4310.../embeddedresourceaudit54970F1F... passed. The actual fullrun fails
phase29 at the existing assertion in Worker.Run: completed native execution
retains no owned parent I/O dispatcher. The same contract also checks later
requests retain their realm without an idle dispatcher. Neither assertion was
changed or waived. Receipt:
46B3331D89DE9596FCC86FD167A229858C1BDAB6B547238363ECC6D3A2946148.
73030ms/300s, exit1/no timeout/kill/Job0/drain/retirement213ms/input+ACLstable/
traceactivated+restored/ownerretired0. Phases0..28 completed, no full pass.

The quick SdkScriptWorkerMain now includes those two completion checks in
addition to its original23, so this contract is enforced before an expensive
fullrun. Unchangedparent906E passes all25 in worker-runtime-v3, compile944ms/
test3788ms, all0/no timeout/kill/Job0/drain/maxretirement1001ms/input+ACLstable/
ownerretired0. Receipt B5F55DA81F0F67EBE163933C153BC107E5A7D29E0CCC7551409E0127C9FDD30E.
Prototype0425 fails the same quick assertion in session-io-runtime-v2, receipt
6D6076162A45A32A8F5107C0C2F694CABB962E1BD153360F94367D0287E57D59:
test3379ms/30s,exit1/killRequested/Job0/drain/input+ACLstable/ownerretired0.
Both source snapshots/failures remain retained. This negative control is not a
passing production candidate. Earlier focused tests did not cover this contract.

Next investigate lower request-I/O costs without retaining a per-session idle
dispatcher. Do not merely hide/rename the field, pool session references, relax
the assertion, or turn the experimental benchmark into release qualification.
Any replacement design needs deadline/state-observation, ownership/retirement,
multi-session/resource-bound and full-suite proof before adoption.

## Change

The mature Jp source creates an IoQueue lazily and stops it in every request's
finally block. This creates a new dedicated I/O thread on the next request.
New-SdkSessionIoReuse.ps1 projects four scoped edits into a separate SDK stage:

- Retain the queue only after a successfully completed protocol response.
- Stop it after failures, cancellation, or loss of the active worker.
- Let St signal the current queue through Volatile.Read; owner-side reset still
  clears the field and retains the existing endpoint and ACL cleanup contract.
- Reset/disposal still call StopIo. No new public API or worker-pooling behavior.

No native guard, protocol assertion, source-size limit, CPU/memory cap, request
deadline, or process-retirement budget is changed. Exact inverse and three
negative controls pass; original mature source and the quarantined full-run
candidate remain untouched.

## Evidence

Prepare-SdkScript -Net48 -SessionIoReuse stages 28 source/reference members at
I:\LeaBuildStorage\sdk-script-20261101-session-io-v1.
Source manifest F60D00606441BE0EEA5B664F042994A868FD94814D2775230D2F6404133EB956.
Owned SDK project build: 14342ms/120s, exit0/no timeout/kill/Job0/drain,
retirement134ms, inputs unchanged, foreground owner retired0.
Build receipt FA92DF2B826A0918CAA47818D918C6A20B6A417BAB1255DC0E930EF832512BD3.
Parent0425BFF9D9289C5F50DCA565BB257A394DA3D529F96202FA6D6D7324905707B7;
adapter8D1FC6A909EEFBC5CF71B74F3905F66D9CC74A63158A5DD8FBA7DC3094F3FD74.
Worker remains F8BE1887...; core5B8A/viewA622 remain unchanged.

Test-SdkScriptLocal -HostConfig -Worker -SessionIoReuse passes the existing
23 worker checks in a fresh NTFS folder. Compile1179ms/test4008ms, both exit0,
no timeout/kill/Job0/drain, maxretirement264ms, inputs unchanged, exact six-file
ACL restoration and foreground owner retired0. Runtime receipt:
DEE05B8BD8552ADA0B5D9AF27B551CF4B2853BAAA3D5D221FB126D77A1994184.
This covers real isolated execution, persistent state, source limits, thread
affinity, original-process-handle stop observation, timer/frame cancellation,
disposal, and permission restoration. It does not directly count I/O threads.

## Remaining

## Request Latency

2026-11-01 SdkSessionIoBenchmarkMain executes one first-use script plus32identical
counter/title DOM requests per fresh process. No uncounted warmups or console
output during the measured request sequence. SameF8BEworker/core5B8A/viewA622,
fixture, default2000ms/128MiB/CPU2500/guards and30s test/5s cleanup budgets.
The baseline906E and candidate0425 were run in the planned ABBA order, each in
a fresh NTFS folder. All four cases pass37correctness/policy/retirement/ACL
checks; all eight compile/test processes exit0/no timeout/kill/Job0/drain,
inputs stable, six worker ACLs restored, foreground owners retired0.

| Case | First Execution ms | 32-Request Mean ms | Median ms |
| --- | ---: | ---: | ---: |
| Baseline A1 | 2819.98 | 17.90 | 5.15 |
| Candidate B1 | 2487.74 | 5.51 | 2.46 |
| Candidate B2 | 2058.20 | 2.54 | 1.05 |
| Baseline A2 | 3675.09 | 13.17 | 5.44 |

Pooled64requests per variant: baseline mean15.5355ms/median5.3000ms versus
candidate mean4.0258ms/median1.31425ms. Nearest-rankp95:103.8022ms vs9.1131ms.
These observations are specific to this machine/workload; first-use variation,
OS caches and the small four-process sample prevent broad speed/reliability or
statistical claims. No direct total thread-creation count was collected; queue
policy is asserted for every request and direct thread reuse was tested above.
All first-use observations and every measured request are retained.

Audit-SdkIoBenchmark validates pinned four receipts, identical source hash,
component identities, sequential ABBA birth/finish times, budgets/lifecycle,
current byte/ACL integrity, exact32sample order/37check footer and three parser
negative controls. Comparison receipt:
9B14F0A05B04F451619EA42CC49623B7BBC1D3088AA73A7C0A7278416BECDC7F.
It records browserSpeedupProven=false/startupFailureResolved=false/
releaseQualified=false. Case receipts A1 A0F71B58..., B1 08FF7921...,
B2 68AFAF2D..., A2 C0E5A167...; directories sdk-script-io-benchmark-*-v1/v2.

2026-11-01 direct lifecycle follow-up: SdkSessionIoMain.cs passes38checks in
fresh NTFS sdk-script-session-io-lifecycle-v2. Same parent0425/adapter8D1F/
workerF8BE. Eight successful requests retain the same queue and actual Thread
object. Original process handles and Thread.Join prove retirement after foreign-
thread idle stop, JS failure, recovery/revocation, malformed protocol transport,
controlled in-flight I/O cancellation, and idle disposal. Runtime file ACLs
restore exactly. Compile806ms/test13216ms, both0/no timeout/kill/Job0/drain,
maxretirement235ms, inputs unchanged/ownerretired0. Receipt:
8D5F034EF2EBEC7D14257270689C571B28309F6266B2A13A1610DEB4EF9DF617.

V1 failed its in-flight assertion after earlier lifecycle cases passed; its
delayed stop against an infinite JS loop could race the statement guard.
Exact result status was not captured, so that cause is not proven. Failure
FB21689626238DE31E92113E7EBE3CEBCA32A88EF8197ECD591973B8A0ACFC82 and source
snapshot remain retained. V2 uses an explicit trusted test I/O handshake,
observes the parent request running before St, releases the blocked queue, and
verifies cancellation and thread/process retirement. It tests in-flight I/O,
not an exhaustive page-execution cancellation race. No production change or
engine/test deadline increase was made; both attempts restore ACLs.

The actual WinForms adapter workflow also passes the existing26checks on this
candidate: sdk-script-session-io-adapter-v1, compile1015ms/test5564ms,
all0/no timeout/kill/Job0/drain/maxretirement928ms, inputs unchanged, six ACLs
restored/ownerretired0. Receipt:
C7B86C94CE91B5C15285F3099CC0AD58C07C21724BD60366F10D48024B0ECD91.
Same real message-loop, inline/event/microtask/timer/frame/navigation/tail-await
and permission workflows as the prior adapter test. Still no measured speedup
or full-suite qualification.

This design cannot be promoted without violating the existing completion
contract. The focused38-check lifecycle,26-check adapter and narrow benchmark
results above do not override its full-suite rejection. Preserve these results
as evidence while investigating a contract-preserving alternative.
Do not transfer parent906E/adapter1C34 results to these new DLLs.

The earlier phase50 intermittent load failure, trusted cold bootstrap timeout,
full300s campaign shortfall, legacy UIA limitation, and all six release gates
remain open. Thread churn is an observed source behavior, not an established
cause of the phase66 timing difference or the timeout.
