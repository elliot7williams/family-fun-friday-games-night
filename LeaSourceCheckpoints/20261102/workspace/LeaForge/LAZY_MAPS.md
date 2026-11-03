# Deferred Private Map Candidate

Updated2026-11-02. Separate experimental parent A17246FB..., NOT adopted.
Current better-qualified candidate remains global-install F5BDB1FF/A72EF9C0.
This source change has NOT fixed trusted startup. No matching worker built.

## Implementation

`Prepare-LazyMapCandidate.ps1` derives from global-install source8752B10B...,
not the rejected IIFE wrapper. Only private list-state and exception WeakMaps
become null until first legitimate creation. The WeakMap constructor, get and set
are captured before page code. Invalid receivers return the original branded
TypeError without creating a map or invoking receiver proxy traps. Observable API
descriptors, original function bodies except these private accesses, bridge commands,
host Evaluate/checkpoints/policies and all public aliases remain unchanged.

Source `I:/LeaBuildStorage/lazy-map-source-20261102-v1/source.json`:
`4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803`.
Both readable and compact projections have exact inverse and16malformed/duplicate/
already-projected controls. Node full-bootstrap mock test PASS312 (78x4), receipt
`node-allocation-test.json` beside source. Counted mock constructions:
original3 versus candidate1 at startup, candidate2 at first classList, both3 after
DOMException use. Early invalid receivers create no map. Replaced public WeakMap,
its get/set and Reflect.apply before first use do not break candidate operations.
This is a NODE MOCK proof, not Jint allocation counts, timing or native qualification.

Parent build `I:/LeaBuildStorage/lazy-map-build-20261102-v1/result.json`:
`F4194E982B87835E9EC08F553136E558C0C0652C48961D97E1FA3A794547007C`.
DLL `A17246FBF5F0EA68B931E7125ECD41189CC21ABA8D798396CB339C74F7728098`.
Compile7022ms/retire123ms, original120s/5s, owner57682guardedexit0. Host8.cs,
policy10.cs and checkpoint2.cs match earlier parent source exactly.

## Native Interpreter Attempts

The harness `LazyMapTrustedMain.cs` calls the original compiled private trusted
ClassLists.Suite with unchanged assertions, followed by a SEPARATE fresh-process
FirstUse probe intended to test early branding, poisoned public constructors/
intrinsics, nonmutation and revocation. No warmup before either process.

V1 launcher incorrectly retained scheduling's javascript-view.html instead of the
original javascript-attributes.html. It passed bootstrap/partial assertions but
failed field/CSS retention. INVALID fixture for this suite; do not qualify results.
Receipt `continuity-evidence/lazy-map-trusted-v1/result.json`:
`BD7F51B5473BBD51D647B6444B8916D204C92716053B80A927045EC95B33CF5B`.
Compile3333/run5005ms, retire125/55ms,13bytes+ACL/inputs stable, owner21105exit0.
Raw failure/logs/runtime retained. No complete suite footer or FirstUse process.

Corrected fresh V2 uses the exact original attributes fixture DFC37D26... from
staging proof90FD0D60.... It FAILED BEFORE the first class-list assertion:
Script budget exceeded; phase=bootstrap; TimeoutException; stage=document.
Receipt `continuity-evidence/lazy-map-trusted-v2/result.json`:
`068DD6A27069736060383DB42EEAD94EBA11C0A8141870C7824CEF69B5D41C45`.
Compile3492/run3616ms, retire298/49ms. Exit0/1, both Job0/drained,13runtime hashes+
ACL/inputs stable, owner22081guardedexit0. Run killRequested=true remains recorded;
outer timedOut=false, original120s/30s/5s limits. This is NOT a passing startup gate.
The sequential campaign stopped; FirstUse probe has NOT executed in Lea/Jint.
C: subsequently observed507850752bytes, below reserves, so no independent probe
or further native build was launched. Cause of storage fluctuations remains unproven.

Audit `6300B86A3840F211EFEDCE6796E30D9835B5D3A9CA081D0788757AC60B8E85C3`
at `I:/LeaBuildStorage/lazy-map-audit-20261102-v1/audit.json` verifies exact mock
counts/source/test identity, unchanged host policy, both failed outcomes/fixtures,
owned retirement/current bytes+ACL, unexecuted probe and raw evidence copy/readback.
It explicitly preserves V2 killRequested and candidateQualified=false.

## Next

Do not retry unchanged ClassLists or transfer global-install passes to A17246FB.
FirstUse remains a separate pending case and may run only after original reserves
and current ownership/bytes/ACL checks permit it. No speed or reliability gain proven.
A stronger NEW, unimplemented hypothesis is to defer the remaining event-target
map too: window/document could keep private direct state records while element
wrappers use a captured-constructor map on demand. This needs full receiver/proxy/
event/listener/captured-intrinsic validation; it is not an implemented feature.
All six release gates remain OPEN; no browser adoption or overall completion claim.
