# Owned Test Working-Set Policy

Updated2026-11-02. Read-only diagnostic, not a fix or release gate.

WorkingSetPolicyMain.cs queries GetProcessWorkingSetSizeEx inside the actual
owned test process before and after its first script call. It neither sets policy
nor changes priority, working sets, garbage collection, guards or deadlines. It
uses the unchanged resource-profiler parent7F2373FE... and existing cold containment
probe. Instrumentation and pre-call policy-query overhead remain diagnostic costs.

The existing trusted-bootstrap-profile-v4/runtime was verified against all10
original hashes/ACLs before adding only Lea.WorkingSetPolicy.Tests.exe and its
config. The new12file inventory was verified after execution. No old product or
driver was overwritten. The original10file receipt remains a historical boundary,
not proof that the runtime still has only10files. No quarantined runtime reused.

## Result

Process15800 reported the same policy before/after: minimum204800bytes,
maximum1413120bytes, flags10 (0xA), hardMinimum=false, hardMaximum=false.
Those are two observations of THIS process, not continuous monitoring or a query
of every enclosing job. The default soft maximum is not an observed hard cap.
The result does not establish the cause of the faults or timeout.

The cold diagnostic FAILED: limit/phase=bootstrap/TimeoutException/stage=elements.
Comparison call2890ms/406.25ms process CPU. Paired resource trace19markers:
prepare1254.2368ms wall/203.125ms threadCPU/+20966faults;
evaluate1082.6207ms wall/78.125ms threadCPU/+17954faults. Both0logical read/write
bytes. Observer effects, coarse CPU counters and sequential sampling still apply.
Do not treat this as successful startup or transfer earlier diagnostic success.

Receipt continuity-evidence/trusted-working-set-policy-v1/result.json,
SHAB753902B772FA3E106C59A7190BC935360030162AD38B9CCD16E1E333F987BF3.
Compile2537ms/retire292ms, run4293ms/retire53ms, exits0/1, bothJob0/drained,
no kill or outer timeout;12runtime hashes+ACL/inputs unchanged. Original owner2460
guardedexit0. Original120s/30s/5s and600MiB plus256KiB new-driver reserve retained.

Read-WorkingSetPolicy.ps1 validates identity/bounds, decoded Boolean flags and
contradictory hard-limit combinations. Nine synthetic parser cases PASS, including
a valid policy transition and invalid/missing fields. Audit v2:
I:/LeaBuildStorage/working-set-policy-audit-20261102-v2/audit.json,
SHAC71EF1631A3D2C2BBFBD8AD985E049A17A230F9808485E5E04FB92323B462760.
It checks the typed failure, actual owned PID, policy values, both retirements,
all original10 runtime files unchanged, exactly2new drivers within256KiB and
the current12file inventory. Copied raw evidence is hash/readback verified.
V1 audit remains retained; v2 adds explicit new-driver names/current inventory/
staging-size checks rather than relying only on before-snapshot counts.

## Decision

No policy change is justified by these measurements. Do not retry the same
bootstrap probe to obtain green. Retain original budgets and guard controls.
Return to implementation-side code-size work: investigate deterministic private
binding minification using a proven JavaScript parser/minifier, without mangling
public properties, function names or bridge aliases. Tooling is not installed
yet. Smaller source is not itself proof of faster startup; new native validation
would be required. All six release gates remain open.

Four historical source ZIPs/2927631bytes were moved with verified readback to
I:/LeaBuildStorage/working-set-source-zips-20261102-v1. Working source, current
builds and raw failures were retained. Old source-bundle extraction auditors may
need archive-path reconstruction from relocation.json; do not waive hashes.

[Microsoft policy query and flag definitions](https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-getprocessworkingsetsizeex).
