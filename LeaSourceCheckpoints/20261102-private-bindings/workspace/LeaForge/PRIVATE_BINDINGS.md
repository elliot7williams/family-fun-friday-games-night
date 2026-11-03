# Private Bootstrap Binding Minification

Updated2026-11-02. New source/build candidate, NOT adopted in Firespider.
The global-install parent/worker remains the better-qualified pair. All six
release gates remain open. Source size reduction is not completion percentage.

## Implementation

Minify-PrivateBindings.cjs derives from the better-qualified global-install
source8752B10B..., not the rejected IIFE or unqualified deferred maps. Terser5.44.1
is pinned with package-lock/integrity, installed build-only on I: with lifecycle
scripts disabled. npm cache/temp also live on I:. It is not a shipped engine
dependency. Lockfile is retained in Lea.Build/JavaScriptToolchain.

Semantic compression and property mangling are disabled. Function/class names
are retained; lexical bindings can shorten. Host Evaluate, limits, checkpoints,
bridge strings and readable source are unchanged. Output is generated twice with
identical bytes. Acorn structural comparison retains literals/operators/public
keys/function names and ignores local identifier spellings and insignificant
formatting. Four deliberate bridge/property/function/limit mutations are rejected.
This is NOT a complete binding-equivalence proof; behavioral tests remain needed.

Candidate I:/LeaBuildStorage/private-binding-source-20261102-v1/source.json,
SHA9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2.
Bootstrap text27635->21342bytes:6293bytes saved, about22.77%. Readable Dom.js is
byte-identical to its original. This is not a23% reduction of the whole engine.

PrivateBindings.test.cjs derives the frozen312-check mock suite with exact inverse
of its harness substitutions. Both versions retain3eager maps. V2 additionally
compares public descriptors/function names/arity on global/document/element/style/
classList and interface prototypes.312checks PASS across readable controls and
compact variants, plus2metadata comparisons. V1's narrower metadata observation
is retained separately. No Jint/worker/native semantics claim follows from Node.

## Native Build And Fixed Comparison

Build I:/LeaBuildStorage/private-binding-build-20261102-v1/result.json,
SHAD32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C.
DLL406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F,
107520->101376bytes:6144bytes saved, about5.71% for this script DLL only.
Compile8233ms/retire141ms, original120s/5s, owner99890 guardedexit0.
Host8.cs/policy10.cs/checkpoint2.cs match the global-install build exactly.

Fixed baseline/candidate/candidate/baseline cold probes all PASS under original
1000ms and30s/5s ownership. Reports: baseline2673/2229ms wall,250/265.625CPUms;
candidate2847/2305ms wall,312.5/234.375CPUms. Candidate mean is not faster in this
small sample; variability and only2observations each prevent causal claims.
No demonstrated speed or startup reliability fix. No retries or warmup.

Receipt continuity-evidence/private-binding-comparison-v1/result.json,
SHA2D4758CB6BCC81CB2DAE266A7BB55F07CA49D92E66182A47FA9FC19651899CF4.
Compile3369ms/retire138ms; four run retirements10/67/22/30ms. All Job0/drained,
no kill/outertimeout;20runtime hashes+ACL/inputs stable; owner38584 guardedexit0.
Verified baseline reused without overwriting it; candidate has a fresh closure.

Audit I:/LeaBuildStorage/private-binding-audit-20261102-v1/audit.json,
SHAF4F0B0A22D82343CB9F9CEB8CABAF5084662DBED6CE3B7BFC50D764CDB6549B7,
checks source/tooling/mock/build/fixed sequence/host code and all6owned retirements,
current bytes/ACLs plus raw evidence readback. An npm lockfile's empty root key
required structured -AsHashtable parsing; fixed before the successful audit.

## Storage And Backup Boundary

C: transiently reached0free during test creation; the attempted new test file
was empty, with no prior source overwritten. It was repaired before the passing
tests. Three older source ZIPs673058bytes were moved with hash/readback checks:
I:/LeaBuildStorage/minifier-reserve-source-zips-20261102-v1/relocation.json.
The recovery mover itself is I:/LeaBuildStorage/Relocate-MinifierReserve.ps1.
Two historical private checkpoint ZIPs9670410bytes and one older runtime ZIP
7456300bytes were likewise moved, not published, to minifier-checkpoint-zips-
20261102-v1 and minifier-runtime-archive-20261102-v1 on I:. All contents retained;
raw failures/current builds/working source unchanged. Old path auditors require
reconstruction from relocation manifests. Cause of disk swings is unproven.

Backup-CurrentSourceToExternal.ps1 now includes .cjs/.mjs; older checkpoints'
extension filter omitted them. No existing JS test is assumed remotely backed up
merely because an older source ZIP passed its narrower scope. Include the frozen
LazyMapProjection.test.cjs dependency with the minifier test backup.

## Next

Run original trusted semantic suites and first-use/revocation tests against this
exact DLL, then build matching isolated worker and run isolated/native/browser
regressions before considering adoption. Do not transfer old host/worker passes
to this DLL. Full-suite reliability, SDK packaging/notices, compatibility and clean
machine validation remain open. Preserve disk reserves and original deadlines.

[Terser options](https://terser.org/docs/options/) document the build-only settings.
