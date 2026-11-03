# Combined Trusted Execution Observation

2026-11-01. This investigates campaign phase8 first trusted page execution failure,
not the separate v7phase29 isolated worker hello timeout. Both remain unresolved.
No production DLL/EXE/constraint, dispatcher, priority or system policy changed.

## Exact First Call

Test-CombinedTrustedCounters.ps1 reuses pinned New-ExternalBootstrapProgram, adding
an explicit early stop only AFTER the original computed assertion. Original Ex
call is unwrapped. Four source negatives reject missing call/assertion, duplicate
source and already-projected text. Original phases0..7 remain, then8begin; success
prints an explicitly diagnostic-only stop, never the full3412 terminal footer.
Same64 diagnostic source inventory, changed Program only. Five hundred twelve
external samples maximum/100ms cadence; original owned process safe handle borrowed
under AddRef, no PID reopen. Counter and V2 thread snapshots verify original native
birth. The owning helper has only a renamed function and sample hook; exact inverse
plus3negative controls preserve300s/5s/4MiB output/512MiB reserve/Job isolation.

I:/LeaBuildStorage/combined-trusted-counters-20261101-v1/result.json
SHA2563652E3E658105CB7BEBC0EB7DFD112D3026B93940C275222746B548125BB921C.
Compile7217ms/120s, diagnostic7187ms/300s,65samples/computed success, retirement
89/124ms within5s. Both exit0/no timeout/kill/Job0/outputdrained, original owner
retired0; all inputs and130retained C:NTFS runtime hashes/ACLs unchanged.
New copied diagnostic images/fixtures on I:exFAT avoid another C:61MB copy. The
private ACL contract still uses unchanged C:NTFS temp; this is NOT isolated worker,
new NTFS file grants, cold disk comparison or all-host qualification. Different
image/fixture location and sampling cost limit equivalence to the original failure.

## Compiled Boundaries

CombinedScriptPhaseMap.cs and Audit-CombinedScriptPhaseMap.ps1 inspect exact current
Framework Jint B54618A4... and parent36AB8810... via PEReader, without loading or
executing target assemblies. Generic method-spec names are resolved for selected
Execute/ExecuteForCompletion/ExecuteWithConstraints/ScriptEvaluation/reset methods,
Js.Run/Start/Vo.At and time/deadline constraints. Selected strings overloads parse
before prepared Execute; prepared execution calls the constraint wrapper. Current
time constraint checks/resets use GetTimestamp. Not full-method CFG equivalence or
exclusive phase durations; no new parser/cache/budget policy follows from this.
I:/LeaBuildStorage/combined-script-phase-map-20261101-v1/result.json
SHA2562E8A2ABC34FAB942D03A683A70BA59ADD51DFB75FDE340B37532AA0C37BA78A0.

## Independent Audit

Audit-CombinedTrustedCounters.ps1 pins current inputs, all retained NTFS ACLs,
process guards, owner projection inverse,65monotonic original-handle samples and
5malformed sample negatives. It checks linear IL call-site order for both string
overloads with3negative cases. Call-site verification is not whole-method CFG proof.
I:/LeaBuildStorage/combined-trusted-counter-audit-20261101-v1/result.json
SHA2563F8B26A3778580AC6776924BA49520F3A56258D5270E72A3431C903861543C93.
21samples after externally observed phase8marker span2131ms/46.875CPUms,
fault delta58266/logical read delta0. Includes Js construction/bootstrap/page work;
cannot assign that window to page execution alone. Total faults are not hard-fault
counts, logical I/O is not disk I/O and sampling can affect scheduling. No cause
or speedup established; no successful diagnostic erases a retained failed run.

The first audit failed closed on exact native birth comparison. ConvertFrom-Json
had already decoded birthUtc to DateTime; DateTime.Parse coerced it to a culture
string, losing fractional ticks and timezone information. Typed ToFileTimeUtc or
explicit roundtrip-string parsing now preserves identity, rather than ignoring the
mismatch. Test-ProcessBirthPrecision.ps1 parses the ACTUAL audit assignment via
PowerShell AST and tests UTC/local/JSON/UTCstring/offsetstring against the exact
native FILETIME, malformed string rejection and a legacy precision-loss witness.
I:/LeaBuildStorage/process-birth-precision-controls-20261101-v1/result.json
SHA2561A6E72B5F68E5AF610B526848C230D78B81616BB14F151C523E8E69B890FA839.

## Next Work

Keep both timeouts open. Do not relocate parser work or relax1000ms trusted,
5000ms worker,300s owner or5s cleanup limits. A fresh trace-only parent diagnostic
can mark bootstrap/page boundaries if necessary; do not warm/wrap the first call
or treat it as production qualification. Separately the exported SDK still lacks
portable native-worker source build orchestration; workspace-only native tooling
does not satisfy Visual Studio integration/delivery. That work can proceed with
explicit verified compiler/reference inputs and fresh outputs without adopting
changed engine binaries or closing the startup gate. All six release gates OPEN.
