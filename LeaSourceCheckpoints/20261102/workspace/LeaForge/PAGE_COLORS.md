# Lea Page Colors

2026-10-26 development candidate only; not a replacement for the qualified638
package or final release. Mature host overlay retains About/history/bookmark fixes.

PageColor.vb supplies a bounded iterative parsed-tree lookup. It reads the first
usable unconditional theme-color or msapplication-tilecolor meta value, then the
computed page background or body bgcolor. Unsupported media-qualified metadata
is skipped, not evaluated incorrectly as unconditional. The existing color parser
and legacy WebBrowser branch are retained; this is not full CSS color conformance.
There is no eval, new resource request, engine substitution or remote scripting.
Queue budget65536 and visited-node tracking bound traversal; V7 now explicitly
tests cycle/exact/over-limit behavior. Missing/busy/disposed/failed pages return
no color. Only selected Lea completion refreshes site-mode chrome; custom mode
remains unchanged. Tab selection and current-page-color command use the same route.

## Evidence

Source-pinned original-host audit41C7B4B9 verifies104 non-obj source files against
A1515801. Baseline638 actual handler regression fails the first color assertion;
raw processBB11A4FD remains failed, including forced Job cleanup. Original exit,
Job0 and output drain were independently verified. Recognizing that specific
expected failure is not promoting its raw process result to pass.

Initial candidate8E5568E5 compiles but also fails. V3 diagnostic confirms intended
HTML loaded; V4 confirms metadata nodes and standalone parser output are correct.
The project has OptionInfer Off. Explicitly typed tree/page/color/name locals and
static String.Equals resolve the combined failure in V5. The exact individual
late-bound operation responsible has not been isolated; do not claim that diagnosis.
All earlier source snapshots, binaries, receipts and failures remain retained.
V1 also retains runner finalization failure; null-before handling is corrected.
V3 preparation stopped before launching any process when free space fell below
the unchanged reserve. Separate resume receipt931ADB12 retains that raw failure.
No files were deleted; observed C: space recovered independently, cause unknown.

V5 exact host EXE:
FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818.
Source: continuity-evidence/page-color-host-v5/host-source.
Receipt: continuity-evidence/page-color-host-v5/result.json SHA256
E4AF5A590D9CDB24CFEA9E59587AC7705AE873045E577A7BF7E4FC2A38F15F3C.
Source/harness inverses and three source negative controls pass. Real default
main form passes six local fixtures: theme metadata, invalid-then-valid metadata,
unsupported media skipped, tile-color metadata, body bgcolor and CSS background.
Each checks loaded page, extracted color, automatic toolbar BackColor and same
selected tab/view. Current-page command, custom-mode preservation and null host
also pass. UI properties verified, not screenshots or general visual comparison.
External requests denied by the harness. Existing settings/session isolation and
restoration remain in the original harness. No updater or installed adoption.

Positive process exit0/9276ms under90s, no timeout/kill; original process exit,
original Job active0 and output drain19ms verified. Input/runtime bytes and ACLs
held; owner72243 retired0 safely. Earlier owners97266/84770/94669/22600/88697
also retired0 after verified zero leases/no unverified-child guard. No live owner.
Engine/view/parent/adapter/normal worker unchanged. Later own15 campaign below
passes; fresh package/extraction and matched final gates remain open. Old638
passes are not transferred to FBBD. Printing still needs separate Lea migration.

## Boundary And Host Qualification

V6 rawCAF7BA1F retained: traversal assertions complete but background fixture
casts an explicit legacy q3 tab to Lea V and fails in the HARNESS, not a reported
new startup crash. Correct fixture uses existing qln Lea route with the same
background-completion assertions; no product byte changes. q3 remains an explicit
legacy route (also reached by multiple tools), an unresolved broad migration issue.

V7 receiptFAC021955F0E1F89C96746162DCA9A17DDEA3A0E9ABEDD91271CC2C9D1F51478
passes original six fixtures plus null host, cyclic node graph, exactly65536
queued entries, over-limit rejection, exact tree restoration, actual background
Lea completion via qln and original tab restoration. Positive9442ms/90s, no kill/
timeout, original exit/Job0/drain12ms; inputs/bytes/ACLs held, owner86258 retired0.
V6 owner17982 retired0 safely. Same FBBD host, changed diagnostic harness only.

OWN15 campaign at temporal-startup-v1/host-page-color-owned-v1/result.json:
3829CAFC58C5DFFCE1F84CC1B4E3756757E25954E696FB9F5144663375A53449.
Settings probe, clean/Lea/navigation/second/split/edit/style-start/script host
cases and six ACTUAL EXE startups(clean/restored/locked/oversized/lazy-background/
lazy-all) all pass. Script subset33702ms/90s. Original case lists/budgets30/90/100s
match old campaign; three static parity/negative controls71C8EB94 pass. Initial
negative-control source preserved; interpolation bug prevented its intended
mutation, corrected to a literal numeric mutation. No test assertion was waived.
Owned launcher/adapter unchanged. All15 original exits, Job0 and complete outputs
verified, no timeout/kill, largest shared retirement/drain136ms.806 inputs held,
398 runtime bytes unchanged. One Lea.Image.exe ACL delta accepted under unchanged
decoder policy15F46DBC, not deletion/profile-cleanup proof. Owner33034 retired0
safely; no required live sessions. Original Boot2C44FB4A remains failure-only.

## Next

Separate FBBD package and extraction checks now complete below; old638 ZIP
untouched. Continue printer/tool migration and retained intermittent
startup/security/delivery gates. Preserve evidence and original budgets. No65%
promotion or clean-machine/all-site claim follows from these passes.

## Development Delivery

dist/LeaExes/Firespider-Lea-page-color-development-20261026.zip,
7464791bytes/398 runtime files +2 generated note/manifest. SHA256
BF7C6C6602FC0F65E38952873D89211AFB7138629A64E06C9B4971FF01D70DCF.
Package receiptCA166212 verifies all400 members, exact FBBD/normal60FF/Jint67F3
and retained source checkpointsD4815CEA/DC5D1464. Runtime ZIP only, not full SDK.
Notes distinguish one full-script pass from unresolved earlier intermittency and
warn about remote scripting, legacy tools and remaining release/security gates.

Fresh extraction receipt at page-color-package-extraction-v1/result.json:
DAA4B7E5BFE4E2132FA60E8C65E8DD063D3FB0BCD1BAAC6070978CC6EABEBD27.
All400 extracted member hashes match ZIP;398 runtime hashes match own campaign.
Settings lookup6299ms/30s, navigation10855ms/90s and actual clean EXE startup
13323ms/100s pass3/3. No timeout/kill; all original exits/owned Job0/output drains
verified. Inputs/bytes/ACLs unchanged, no permission delta. Owner96291 retired0
safely. Current-machine smokes, not all15 extracted or clean-PC validation.
Old638 ZIP rehashed unchangedA8742908. No installed/default/L: replacement.
No product binaries changed while packaging or extracting. Complete matching
source remains in verified local checkpoints, not an off-device backup.
