# Phase-Level Bootstrap Resource Diagnostics

Updated2026-11-02. Diagnostic observation, not a product fix or release gate.
Earlier aggregate fault/CPU observations and synthetic guard controls remain in
TRUSTED_BOOTSTRAP_COST.md. This adds phase-level accounting, not first discovery
of the broad wall/CPU gap. No unchanged production test was retried to claim green.

## Implementation And Pins

BootstrapResourceCounters.cs uses read-only Windows counters for the calling
thread/process. Prepare-ResourceBootstrapProfile.ps1 extends the existing detailed
buffered profiler with sequential thread CPU, process CPU, process faults/I/O,
thread cycles, working set and commit snapshots at its existing marks. It adds
no bootstrap checkpoints, page code logging or per-stage console writes. Every
original host operation and1000ms bound remains. Three missing-boundary controls
and exact inverse cover the incremental projection. Observer costs remain real.

Source I:/LeaBuildStorage/trusted-bootstrap-profile-source-20261102-v4/source.json,
SHA8A80C461B25495B17862096469E0CC17AC061947380CB59019C47CB8136F60EC.
Build result in trusted-bootstrap-profile-build-20261102-v4 on I:,
SHA8849F3956D4BC6D6E97E444BFB6451C58102515F5EA8FE3B07B9DA19011FDEEE.
Diagnostic parent7F2373FE748206C2CE66366F6BC35B43B9AEBC82B3B5DA3565FCCE47D0CCC338.
Compile11969ms/retire1318ms; owner56520 guardedexit0.

Fresh cold run continuity-evidence/trusted-bootstrap-profile-v4/result.json,
SHA2B084ABE6DD48B6C3B7D5AE7CA5EA4EEAFAD5C09CAAADD888E837D04854A59FA.
Compilation3061ms/retire194ms; run3914ms/retire44ms. All exit0/Job0/drained,
no kill or outer timeout.10runtime bytes+ACL and inputs unchanged; owner94261
guardedexit0. Bootstrap comparison returned ok/2702ms total/296.875ms process CPU.

## Observations

21 resource samples align exactly with the21 original detailed profile markers.
Preparation:1109.7671ms wall,125ms thread and process CPU,+20947process faults,
zero logical read/write byte delta, working set+2945024bytes, commit+1552384bytes.
Evaluation:941.2121ms wall,125ms thread and process CPU,+17035process faults,
zero logical read/write byte delta, working set+1613824bytes, commit+1036288bytes.
The reported thread ID stays4572. Cycle deltas are retained, not converted to time.
Samples are sequential, not atomic; CPU times are coarse. Aggregates overlap the
adjacent intervals and must not be added to them.

Read-BootstrapResources.ps1 enforces syntax, marker order/count, stable thread,
CPU identity and monotonic counters while allowing working set/commit to fall.
Its synthetic test passes9cases, including malformed/truncated traces. An initial
test-array construction error was corrected before the passing test/audit.

Audit I:/LeaBuildStorage/resource-bootstrap-audit-20261102-v1/audit.json,
SHA530C587CF9189DB537485F750D622096E52C2B39E1B16016D986E55DFB189D38,
verifies source/build/run identities, paired traces, parser controls, all three
owned retirements, current bytes/ACLs and copied raw evidence readback.
resources.json contains the full parsed interval table. startupFixed=false.

## Limits And Next Action

Faults include hard and soft faults; zero logical I/O does not exclude paging.
Do not attribute the gap to paging, storage, GC, guards, scheduling or JIT from
these counters alone. This instrumentation may affect first-call behaviour; its
single success does not resolve uninstrumented failures or qualify any candidate.
The existing deep/shallow guard controls did not reproduce the fault pattern;
do not disable or amortize guards based on the new trace.

Next query the actual owned test process's working-set policy/flags at bounded
points, rather than infer its policy from the calling tool host or system-wide
free RAM. Do not change working sets, priority, paging configuration, constraints
or deadlines. The global-install candidate remains the better-qualified product
candidate; no Firespider/worker adoption or release gate closure here.

Storage: four historical source ZIPs/4824026bytes relocated with verified hashes
and readback to I:/LeaBuildStorage/resource-profile-source-zips-20261102-v1.
Working source/current builds/raw failures unchanged. Reconstruct old archive
paths from relocation.json before running auditors that require those paths.

API references: [process memory counters](https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters),
[memory query](https://learn.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-getprocessmemoryinfo),
[thread cycles](https://learn.microsoft.com/en-us/windows/win32/api/realtimeapiset/nf-realtimeapiset-querythreadcycletime).
