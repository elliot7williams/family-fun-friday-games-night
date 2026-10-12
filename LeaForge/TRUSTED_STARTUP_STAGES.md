# Trusted Startup Stages

Milestone 146 remains open and broader readiness stays about **65%**. This
change makes trusted startup budget failures diagnosable; it is not a fix for
the intermittent startup failures or a stable browser release.

## Runtime Change

Trusted Js budget messages retain the existing prefix, host phase and reason,
then append a bounded `stage` field only when failure occurs during bootstrap:

```text
Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=scope
```

The setup labels are `configure`, `vm`, `factory`, `bridge`, `prepare`,
`evaluate` and `checkpoints`. During evaluation, the existing Bm validator
supplies the six fixed trusted checkpoints: `declarations`, `scope`, `events`,
`interfaces`, `elements` and `document`. These are fixed host labels, not
arbitrary page strings, URLs, raw exception details or source excerpts.

A setup label identifies the host operation entered. A checkpoint identifies
the last verified checkpoint completed; it does not identify a particular
subsequent JavaScript operation as the root cause. No timestamps, sampling,
file I/O, debugger or trace-mode execution path are added to production.

The stage is cleared on successful initialization and every Reset, alongside
the existing realm, timer and frame retirement. Script, microtask, timer,
event and frame failures keep their existing host-boundary messages. Ordinary
JavaScript error text is unchanged. The 512-character cap remains.

Public engine aliases and the private Start signature are unchanged. The
trusted one-second budget, isolated five-second startup, page deadlines,
statement/memory/recursion/regex/array/stack guards, nonblocking window-agent
policy and disabled CLR/GetType/write interop are not relaxed. There is no
automatic retry, fallback or shared realm. The bootstrap assets, shared Bm
validator, worker EXE, parser and IPC protocol are unchanged.

The pinned Jint source resets constraints at the outer host entry in
[ExecuteWithConstraints](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs).
Its [TimeConstraint](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Constraints/TimeConstraint.cs)
compares a monotonic deadline and permits amortized checks. Thus the recorded
checkpoint need not be the first instruction after a test-only delay expires.
This contract does not establish the cause of an earlier natural timeout.

## Controlled Coverage

`Lea.Script.Tests.exe "L:\Lea Development" -sg` is the short alias for
`--startup-stages`. Extra arguments are rejected before script setup.
Fifty assertions cover eight controlled failures: the private test factory,
evaluation entry and all six trusted checkpoints. Each checks production
options, bounded stage reporting, no page work/no automatic retry, full
state retirement, an explicitly requested fresh realm and ordinary errors.
Disabled scripting and rejected oversized code do not initialize a VM.

The first identical test EXE fails on the preceding DLL's missing factory
stage and passes all 50 checks on the candidate. Both Script and Tests builds
complete without warnings/errors. Only Lea.Script.dll changes among 19
production records; its PE length remains **101,376 bytes**. The required
Engine/Win/Image bundle remains **749,568 bytes**, and the worker stays frozen.
Zero PE size growth is not a claim of zero memory or execution overhead.

The initial real-deadline assertion incorrectly demanded `stage=evaluate`.
Its zero-check failed attempt and complete source/binary inputs are retained.
The repaired test keeps the same real one-second constraint, test-only
1,100-ms delay, timeout type, no-write checks and retirement/recovery checks;
it accepts only the fixed evaluation/checkpoint labels and records the actual
returned stage. The runtime is byte-identical between these test tools.

The full suite now expects 2,873 checks, previously 2,823, to include the new
50. Worker-only observation still expects 87. New controlled faults run last,
not before the existing first-use/startup regressions. This is an expected
completion count, not by itself a passing full gate.

## Final Verification

The repaired test-tool build has no warnings/errors. Its identical final test
EXE rejects the preceding DLL's missing factory stage, and the extra-argument
control exits 1 before initialization, as expected. The final runtime is
byte-identical to the initial candidate; only the test expectation changed.

| Final Command | Passing Assertions | Actual Exit |
| --- | ---: | ---: |
| Startup stages | 50 | 0 |
| Real deadline/budget phases | 31 | 0 |
| Fresh-process trusted first call | 1 | 0 |
| Prepared cache/realm isolation/disposal | 14 | 0 |
| Compiled cache contract | 5 | 0 |
| VM options | 16 | 0 |
| Native CPU lifetime | 12 | 1 |
| Native worker | 59 | 1 |
| Native startup phases | 9 | 1 |
| Full scripting regression | 282 | 1 |

The controlled real-deadline case reports `stage=events`. This is the observed
last checkpoint in that deliberate-delay case, not a reproduction of the
preceding natural trusted timeout. The ordinary fresh-process call completes
the unchanged six-write fixture with 2,649 ms wall / 250 ms recorded process
CPU; that includes preparation and is not the interpreter's execution budget
or a cold-machine performance benchmark. Four AST-only observer count/footer
controls pass without launching the inspector.

The full regression fails at recovery after unpublished-launch cancellation:
`phase=hello`, launch 83 ms, probe 30 ms, wall 5,015 ms and total CPU 125 ms.
Its last captured child marker is `startup evaluate`; VM construction returned,
but the exact later blocking operation is unknown. The focused traced startup
case similarly times out at wall 5,012 ms / CPU 78 ms, with final marker
`startup evaluate` at child wall 431 ms / CPU 62 ms. Neither failure is retried.
The full run stops before the new 50-check group at its original late position.

The worker gate fails its combined fresh-realm/old-process retirement assertion
after between-call permission revocation. That assertion did not retain the
returned Jr detail, so its exact failing condition is not established. It is
not an ACL-descriptor equality check and is not attributed to the issue below.

## ACL Investigation

The CPU helper exits zero with independently measured total CPU 140 ms, then
the existing strict SDDL cleanup comparison fails. The failed run did not log
its original descriptor, so the failure is retained without reclassification.
Current read-back of that helper and an untouched paired helper has identical
access entries, differing only by the auto-inherited `AI` flag.

A separate newly created L: fixture uses the unchanged launcher to grant and
remove one synthetic SID, without launching a worker. Its exact before/after
DACL bytes match. Only ControlFlags.DiscretionaryAclAutoInherited changes from
unset to set (1024), while the temporary SID is absent after removal. This
controlled case confirms an SDDL text difference can occur without a retained
grant; it does not retroactively prove the failed native case's missing data.
Microsoft describes this conversion flag in
[automatic inheritance propagation](https://learn.microsoft.com/en-us/windows/win32/secauthz/automatic-propagation-of-inheritable-aces).
No production permission code or existing cleanup assertion is weakened.

The initial PowerShell probe cannot call the Framework File.GetAccessControl
API and never reaches grant/cleanup. The separately compiled Framework control
passes with actual exit zero. Its source, executable and descriptor records
are retained with the evidence.

The standalone browser copy with the candidate parent DLL passes counter,
Start/Pause, three-frame, elapsed-time, isolation and no-MSHTML checks. Captures
show the bar grow from 40 to 90 pixels and 186 ms displayed after pausing.
The host EXE is reused unchanged; this is not a Firespider startup matrix or an
installed release. The failed gates and older startup/CPU failures remain open.

Evidence: `H:\Lea Evidence\Lea-146-trusted-startup-stages-2026-10-11`.
Complete source ZIPs are kept under `H:\Lea Source Backups`. Original Firespider
inputs, installed browsers, normal build outputs and preceding sealed evidence
remain protected. Release gates and modern-web compatibility work remain open.
