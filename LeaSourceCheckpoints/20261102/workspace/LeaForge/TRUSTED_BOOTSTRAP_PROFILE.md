# Trusted Bootstrap Profile

2026-11-02 diagnostic only; no runtime optimization or release qualification.

Corrected buffered profile sourceD1255F73... under
I:/LeaBuildStorage/trusted-bootstrap-profile-source-20261102-v2 retains the exact
original bootstrap operations and1000ms policy. Seven reversible projections,
six invalid-input controls, malformed-pair rejection and critical-operation
counts. Compact DOM is the same isConnected resource; no page code is logged.
Stamps use process CPU, monotonic wall time and GC count deltas, buffered until
completion/failure. Timing is still perturbed by observation and allocation.

Diagnostic DLLB6B0A59D9DB51560D58A55976A971275B93EE4828E0D05DF55594BA4B51F09F1;
build07DC2217... at trusted-bootstrap-profile-build-20261102-v2. One cold run
receipt47FB59FC847310FB40562C2E4A758C3F80D996E11AA44E71259AD372F1FCAC35 at
continuity-evidence/trusted-bootstrap-profile-v2/result.json. Owned compile3404ms/
120s, child3573ms/30s, retire210/42ms under5s, exit0/Job0/drained/10runtime bytes
and ACL/inputs stable, owner72007retired0. Caller reports2530ms/296.875processCPUms.

Measured intervals in this ONE successful diagnostic observation:

| Interval | Wall ms | Process CPU ms |
| --- | ---: | ---: |
| VM creation | 222.008 | 31.250 |
| Bridge setup | 86.032 | 31.250 |
| Fixed syntax preparation | 1030.181 | 140.625 |
| Bootstrap evaluation | 922.824 | 78.125 |
| Scope-to-events, nested in evaluation | 392.444 | 15.625 |

Preparation precedes evaluation and must not be interpreted as exceeding the
evaluation timeout. Scope-to-events is nested, so do not sum all rows. One gen0
collection increment occurred during preparation; none of the GC counts changed
across evaluation. Process CPU is coarse and includes other threads. The gap
between wall and CPU is NOT attribution to scheduling, paging, JIT or GC.

Audit28BC8309EE69B81A21806DCA7DF40FCF374CCE72DFD4504C438DE1DD9299D477 at
I:/LeaBuildStorage/trusted-bootstrap-profile-audit-20261102-v2/audit.json checks
15ordered markers/counter units/monotonicity, six malformed-trace negatives,
current runtime hashes+ACL/inputs and copied raw evidence readback.

V1 instrumentation was INVALID: PowerShell array-concatenation precedence made
some projection pairs longer than two members, dropping prepared.Value and
vm.Evaluate while the inverse alone still passed. Run030557D7... reports
Incomplete bootstrap checkpoints, not a timeout. Source6DB59C04.../DLL7197F985...
remain retained and MUST NOT supply performance evidence. Corrected source adds
pair-shape and original-operation guards; no product DLL was replaced.

NEXT inspect the measured scope-to-events work with finer buffered diagnostics
and compare fixed setup costs before proposing a bounded optimization. Do not
raise1000ms, bypass constraints, reuse realms or retry to green. Selectors/tree/
attributes/class-list qualification and all existing release gates remain open.
# Detailed Failed Observation, 2026-11-02

Separate v3 diagnostic source `87E7699FF07EDD646DCEA936DFC61B87646C4EA19C6375AE68AD7565EAF9B406`
adds six ordered private checkpoints and adjusts only the diagnostic Bm stage list.
Build receipt `40B540471BEBE09DD44A14642DDF378F0CC7AF1ED59C1C9D2B34918956202FE3`;
DLL `A03E3608E2D98CEB8CEDBD6882052692E8AE98CA5552C3545C35F42422EA807E`.
No product adoption or budget changes.

Cold receipt `278AE2103EA153B378D5DF2D7AD97BC6FE30BB82F6B6C45AB26C02C60CAAB54C`
at `continuity-evidence/trusted-bootstrap-profile-v3/result.json` FAILED at
bootstrap/elements, inner1000ms unchanged. Compile3228ms/run4201ms, retirement165/44ms,
Job0/drained, ten runtime bytes+ACL and inputs stable, owner51670 exited0.
Run exit1/killRequested=true is retained, not converted into a pass.

Preparation1259.104wall/109.375CPUms; evaluation1143.492wall/78.125CPUms.
Nested scope-to-events465.231wall/31.25CPUms comprises consecutive intervals:
130.732,163.803,11.249,34.971,83.297,41.078,0.101wallms.
The eager handler-descriptor factory interval is41.078ms, measuredCPU0 at coarse
process resolution. It does not dominate this observation; no scheduler/JIT/paging
cause or reliable performance improvement is established. Instrumentation perturbs timing.

`Audit-DetailedBootstrapFailure.ps1` verifies exact failure/ownership/bytes/ACL,
copies raw evidence with hash readback, tests seven rejected malformed or incorrectly
classified traces, and rechecks the old15-marker successful parser. Partial traces
require explicit `-Failed`; they cannot qualify a successful bootstrap.
Audit `CD4C52C1DC09D03D1370CC5397141FA736396FAD6DC716A5D4836F1E15FEC302`
at `I:/LeaBuildStorage/trusted-bootstrap-profile-audit-20261102-v3/audit.json`.

