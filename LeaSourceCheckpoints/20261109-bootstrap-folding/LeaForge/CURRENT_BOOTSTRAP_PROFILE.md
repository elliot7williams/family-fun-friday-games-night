# Current Trusted Bootstrap Profile

2026-11-09. Diagnostic only; no engine fix or host adoption.

The current C5CC/4EE7 full-suite v5 failed during trusted computed-script
bootstrap after the events checkpoint (F099EC00; independently audited68075D65).
The original1000ms policy and full-suite300s/5s limits remain unchanged.

## Source And Validation

Prepare-TrustedBootstrapProfile.ps1 -CurrentPrivateBindings reuses the corrected
seven operation-preserving projections, six malformed-input controls and malformed
pair rejection. Current recovered Js8.cs matches21792E79; original Vm creation,
prepare, Evaluate, marker, bridge and budget operations are retained exactly.
Current private bootstrap resource196BCE96 is unchanged. Source proof D74BB7C6:
I:/LeaBuildStorage/current-bootstrap-profile-source-20261109-v1/source.json.

Test-CurrentTrustedBootstrap.ps1 pins14recovered parent source files against the
original406C build receipt. Missing historical14.cs is NOT recovered. New
DiagnosticScriptAssemblyInfo.cs explicitly supplies diagnostic-only version1.0
and net48 metadata. This prevents claiming a byte-identical parent restoration.
Production DLLs and failed full-suite stages are never overwritten.

Separate diagnostic parent C56FB88B72F9D2492101B28B8E8B93FD6B82A94F95A7C1444F6D5E3E5112F21B.
Two owned compiles4768/2788ms; cold test4134ms. Retire138/33/104ms, all exit0,
Job0/drained/no timeout/kill. Ten runtime files and input hashes/SDDL unchanged.
Original owner44595 guarded exit0. One containment probe, not the3412-check suite.

Run receipt I:/LeaBuildStorage/current-bootstrap-profile-run-20261109-v1/result.json
SHA E926C855BBAA2528E7FD113A2D1A337C616A1F0173C44B06B74C94EEA22D3B0F.
Independent audit CAA0FF016F66044C93C2A9855AA0D76532D50E449A643AD763545164C4F79643
in current-bootstrap-profile-audit-20261109-v1 validates15ordered monotonic
markers/frequencies, six malformed-trace negatives, ownership, exact hashes/SDDL
and copies/readback-checks raw evidence. Runtime staging is NTFS on C:; source,
build recipes, reports and copied evidence live on I:.

## One Observation

| Interval | Wall ms | Process CPU ms |
| --- | ---: | ---: |
| VM creation | 297.16 | 31.25 |
| Bridge setup | 91.21 | 0.00 |
| Fixed syntax preparation | 1171.59 | 109.38 |
| Bootstrap evaluation | 856.12 | 78.12 |
| Scope to events, nested in evaluation | 335.96 | 15.62 |
| Events to interfaces, nested in evaluation | 156.94 | 0.00 |

Caller total2756ms/250processCPUms. Preparation is BEFORE evaluation, not a
violation of its1000ms timeout. One gen0 collection occurred during preparation,
none during evaluation. Zero coarse CPU delta is not proof of zero computation.
Nested spans must not be added to the evaluation total. Buffered observers and
replacement metadata can affect timings. No comparison or speed improvement is
proved; a successful diagnostic does not erase the full-suite failure.

## Fixed Folding Comparison, 2026-11-09

Test-CurrentTrustedBootstrap.ps1 -FoldConstants changes exactly one preparation
option in a separate diagnostic host: FoldConstants=false to true. Exact inverse
and three malformed/already-projected controls pass. The current resource and
14recovered behavioral sources remain unchanged; explicit diagnostic metadata is
identical in both arms. Original1000ms/20000statements/8MiB/64recursion, regex,
array and interop constraints are unchanged. No page code is precomputed.

Folded parent51CB6B6C19326CEF3020073A0EF0E2B9E80887E689AEE9491D90344501F43650.
Fixed baseline/folded/folded/baseline, fresh process each time, no retries:

| Case | Caller total ms | Preparation ms | Evaluation ms |
| --- | ---: | ---: | ---: |
| Baseline1 | 3261 | 1225.89 | 1164.15 |
| Folded2 | 2135 | 868.78 | 680.13 |
| Folded3 | 3539 | 1670.75 | 953.32 |
| Baseline4 | 1663 | 657.28 | 588.17 |

All four containment probes completed. Six owned compiles/tests exit0/Job0/drain,
no kill/outer timeout; all20runtime bytes/SDDL and inputs stable. Owner65505
guarded exit0. Mean caller total baseline2462ms/folded2837ms. Wide variation,
only two observations per arm, buffered instrumentation and coarse CPU counters
preclude a speed claim. The sampled evaluation interval includes observer costs
and is not an independent interpreter-timeout test. NOT adopted as a startup fix.

Receipt01984D30DCC0F2FE1824CA175372C040BA4BE12EC159659902D087A895DB0007
at I:/LeaBuildStorage/current-bootstrap-fold-abba-20261109-v1/result.json.
Independent audit7B8CEB47E9CEC7410874F274A69ABDA1C40362A8E07D1110CB2E09C0B6BC8319
at current-bootstrap-fold-audit-20261109-v1 validates raw reports, all60ordered
markers, fixed sequence, exact one-option inverse, runtime/input identities,
ownership and copied raw evidence. Fullv5 remains failed; no worker/host adoption.

CurrentCpuPolicy.cs only queries the calling process's immediate Windows job.
CpuPolicy.test.ps1 passes13synthetic decoder controls, including union modes and
invalid flag/range rejection. Actual tool-host snapshot PID576: InJob=true,
query succeeds, flags/value0, rate control disabled. Receipt91E2B7E1DA97B4F7BF46F13F9E13645269766E8F8174351283A73898E07A5CB8
at I:/LeaBuildStorage/current-tool-cpu-policy-20261109-v1/result.json.
This neither queries every ancestor/native test job nor proves the timing cause.
No quotas, priorities, working sets or process ownership were changed.

References: [pinned Jint preparation options](https://raw.githubusercontent.com/sebastienros/jint/v4.16.4/Jint/PreparationOptions.cs),
[job CPU control structure](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_cpu_rate_control_information),
[job query scope](https://learn.microsoft.com/en-us/windows/win32/api/jobapi2/nf-jobapi2-queryinformationjobobject).

## Next Action

Evaluation already used856ms in this diagnostic, but events-to-interfaces was
only157ms. Do not assume the last completed marker identifies the cause.
Attribute the larger scope-to-events/declaration spans and the wall/CPU gap using
bounded observations before choosing an optimization. Preserve original policy,
fresh realms, all checkpoints, isolation and failure evidence. Do not prewarm or
retry an unchanged full suite to claim green. Matched full-suite, print, host,
compatibility and delivery qualification remain unfinished.
