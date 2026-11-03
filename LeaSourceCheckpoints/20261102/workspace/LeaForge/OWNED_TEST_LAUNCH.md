# Owned Diagnostic Launch

2026-10-26. Standalone test infrastructure, not a production engine change or
completed full-suite integration. Existing development ZIP A8742908 is unchanged.

## Problem

Full script V8 hit its original300s deadline. The older utility killed its parent
and drained output but had no original Job handle proving all descendants exited.
The unverified-child guard was retained until user reboot. Raw failureBD9A11F7
and correctionAC1EEA16 remain unchanged. No PID lookup/name scan is cleanup proof.

## Implementation

Lea.Build/OwnedTestProcess.cs creates an unnamed, noninherited kill-on-close Job,
then supplies it through PROC_THREAD_ATTRIBUTE_JOB_LIST in CreateProcessW.
The child also starts suspended: original process handle, Job membership,
creation time and initial accounting are checked before ResumeThread.
Only NUL stdin and stdout/stderr write handles are explicitly inherited.
The trace/profiling/binding environment exclusions match the older launch policy.
No breakaway, CPU/memory override, worker policy replacement or remote scripting
permission is added. Existing worker nested jobs retain their own restrictions.

Start errors return the owned object for explicit retirement after Job creation.
A controlled post-create/pre-resume fault exercises this path. Retirement uses
TerminateJobObject, the original process handle and accounting on the original
Job; disposal refuses live processes. The foreground owner retains unresolved
objects and source leases rather than dropping them. This is diagnostic ownership,
not a new browser sandbox or proof of production Np failure handling.

Microsoft documents assignment at creation before the initial thread runs:
[JOB_LIST](https://devblogs.microsoft.com/oldnewthing/20230209-00/?p=107812/).
[Nested jobs](https://learn.microsoft.com/en-us/windows/win32/procthread/nested-jobs)
describe hierarchical accounting. [Job accounting](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_basic_accounting_information)
defines ActiveProcesses. Prototype targets the current Windows10+/x64 PS7 host,
not a newly validated legacy-Windows or portable SDK target.

## Verification

Original six controls:430180118195F310A998531F91E817EEEBF7655CA5DA7A146FA44E224C3684DA
at owned-test-process-controls-v1/result.json. Exact v1 sources snapshotted there
before tightening path validation. The initial console summary displayed null
fields due to an OrderedDictionary projection; authoritative JSON passed. V2
casts the console summary explicitly; raw v1 evidence is retained.

Current eight controls:
7FB338C94345DF9C572F5C0ACDD70283BC9D244F93C6245C6C865A6E9AAA4025
at owned-test-process-controls-v2/result.json:

- Drive-relative application and directory inputs rejected before process creation.
- Normal exit7 preserves controlled stdout and stderr exactly.
- Forced parent and parent/child stop.
- Parent exits while a child remains; early disposal rejected.
- Nested owned Job and descendant stop.
- Post-create/pre-resume fault: user code does not execute, disposal rejected until retirement.

Every process case verifies assignment before resume, original process exit,
zero remaining Job processes and complete stream drain. Shared retirement/drain
budget5000ms; largest observed83ms. Test execution budget30000ms. Owner64637
retired0 with no owned leases. These timings are observations, not a speedup claim.

Real exact Firespider638 scripting subset:
DB8AF85726C90E8EE327C5000A0034BEA7195362E345E5CEEC0B9BBC28F12D29
at owned-lea-host-v1/result.json. Original90000ms host budget, exit0/32490ms.
Shared retirement/drain615ms; original exit and Job active0, complete output,
inputs/398 runtime files/ACLs unchanged. Owner30669 retired0 with no owned leases.
This checks compatibility with existing Lea nested jobs and the recorded host
subset, not all confinement paths, full script suite, crash journal or profile cleanup.
Both controls and real subset used normal nonadmin retained NoExit owners.

## Full-Suite Integration

Separate Invoke-OwnedTestRun.ps1 adapter and Test-OwnedFullScriptSuite.ps1 now
integrated. Five adapter controlsA97DA64E and12 final-gate/registration contracts
633723B8 pass. Earlier C6B00397 failure and exact sources retained:100ms polling
caused output backpressure;10ms bounded polling fixes it without changing limits.
Full run334A4567:3412 checks/all89 phases,289304ms under original300s, no kill or
timeout. Original process exit, original Job0 and complete output/drain247ms;
owner35298 retired0. BuildFC4AD1E5/binding05408970/testA728BB52, unchanged normal
worker60FF and product libraries. Trace activation/restoration and bytes held.
Raw audit false solely for one metadata-only Lea.Image.exe grant. Read-only
outcome6331FD2A accepts it under unchanged decoder policy; three negatives reject.
No raw rewrite, ACL mutation or profile deletion. Prior intermittent failures,
production lifecycle/profile cleanup and final release gates remain unresolved.

## Future Runs

Keep the separate owned Run adapter and older utility/evidence intact.
Preserve original300s execution and
shared5000ms retirement/drain; do not repeat unchanged solely for green.
Keep behavioral failure distinct from cleanup proof: a timed-out/killed test
must remain failed even when its whole owned Job has retired. Incomplete Job,
original-process or output evidence must retain the foreground owner.
Use bounded output handling, source/fixture/binary pins and fail-closed trace
verification in finally. Then validate unchanged production worker restrictions
on the integrated path before interpreting full-suite results.
