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

## Matched Pair Qualification

Updated2026-11-02.

Exact parent406C5634 now passes native FirstUse6 and original ClassLists50 checks.
Harness changes only class/report names from the existing first-use harness;
original compiled ClassLists.Suite/assertions and attributes fixture are unchanged.
Fresh process per case, no warmup/retry. Receipt
continuity-evidence/private-binding-trusted-v1/result.json,
SHAD62490714583EC7956AAFCE575579BF81F4EF8F9D1C1288A6C271B3930A43952.
Compile3280/run5205/4753ms; retire163/32/45ms, exit0/Job0/drained/no kill/timeout,
13runtime hashes+ACL and inputs stable; original owner77496 guardedexit0.

Matching worker IL/ReadyToRun built with only the minified DOM resource overlay.
Receipt I:/LeaBuildStorage/private-binding-worker-build-20261102-v1/result.json,
SHA302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398.
Worker DLL F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0,
six-file closure, exact.NET8.0.33/RollForwardDisable, sandbox/protocol unchanged.
Build18152/21831ms, retire125/38ms, owner30190 guardedexit0. Build receipt flags
workerExecuted=false/runtimeQualified=false describe its build-only boundary;
later matching execution is proved separately, not by editing that receipt.

The original isConnected isolated harness8AE71E18... and casesAE7B3FD5... now PASS
against this matching pair: nonmutating reads, descriptors/brands/read-only,
detached reads/write guard, revocation, cycle limit/recovery, tested sandbox flags
and disposal. Receipt continuity-evidence/private-binding-feature-isolated-v1/
result.json SHA8244E1A2816FF045B17445FE152CB49AD3AA0646097F443B7FAD2763010B70FD.
Compile2950/run6871ms, retire177/50ms, exit0/Job0/drained/no kill/timeout,
21runtime bytes+ACL/inputs stable; original owner45332 guardedexit0.

Shared private-binding-trusted-v1/runtime now has21files: original13 unchanged,
plus6worker files and new isolated driver/config. No old executable overwritten.
Prior13file audits retain their historical scope; do not claim current inventory13.
600MiB reserve plus all new worker bytes/256KiB driver allowance retained. The
projection generator initially rejected an overlapping proof-string inverse;
fixed before generating/running the campaign, no invalid native run qualified.

Pair audit I:/LeaBuildStorage/private-binding-pair-audit-20261102-v1/audit.json,
SHA7B6C3E676DC17ADA0C09FA0719594CF2087C1898A0C9DEF329103A82C645E972,
checks exact cases/assertions, builds, all7owned retirements, old13/current21file
inventories, current bytes/ACL and copied raw evidence readback. It also reads
the ACTUAL embedded Lea.Script.Dom.js resource from parent and ReadyToRun worker:
both SHA196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3.
No startup fix/full-suite/host adoption claim follows from these focused passes.

Six older archive ZIPs9160974bytes relocated with verified readback to
I:/LeaBuildStorage/private-worker-archives-20261102-v1. Private archive contents
are retained locally, not uploaded. No current source/build/raw failure removed.

## Next

Run broader original DOM and trusted/isolated scheduling suites, native V
and host workflows on this exact pair before adoption. Do not transfer old
host/worker passes. Full-suite reliability, SDK packaging/notices, compatibility
and clean-machine validation remain open. Preserve reserves/original deadlines.

[Terser options](https://terser.org/docs/options/) document the build-only settings.
