# Framework SDK Source Graph

2026-11-09. Visual Studio integration fix, not a startup repair/full SDK release.

Current unified export now has128source members/eight dependencies, original
17worker C# sources with current shared resource and six-file native compilation.
All six framework/image/print outputs match the preceding source build; worker
matches F61B. Actual SDK targets and14text/17script/25worker fixtures pass,
auditBBE9F917/nine owned runs. SDK_CURRENT_SOURCE.md and Lea.Sdk/CURRENT_BUILD.md
supersede older component-only next steps, not their frozen evidence. Matched
full3412/89 now PASS283861ms/audit322CFA7E with65runtime bytes+ACL/trace stable;
SDK_SOURCE_FULL_SUITE.md exact pins. Startup reliability/Firespider qualification
remains unfinished; no causal fix or release gate closure claimed.

Latest print extension stages98source members and builds six products. The
new print template directly builds sibling core/view unless explicit binary
paths are given. Old missing-core failure and poisoned-source binary control
retained. New414E1CD8print passes existing capture/export and102range/device/
ticket plus336friendly/image/live checks, audit24A2A32A across8owned runs.
Print C# behavior sources unchanged; native runtime on NTFS C:. SDK_PRINT.md
records exact pins and unchanged release boundary. Full worker still unfinished.

Follow-up source layout sdk-image-source-graph-20261109-v1 now adds the decoder
project and automatic embedding deployment:75source members, no Lea binary
inputs, exact same four libraries plus DA3BFA67decoder.14text/42PNG view and
81retirement/14race checks PASS; audit6C654D97 covers11owned runs. The old missing
decoder control and explicit-binary compatibility are retained. See
SDK_IMAGE_WORKER.md for exact receipts and SOURCE_GRAPH.md in Lea.Sdk for use.
Runtime tests stage on NTFS C:, not the exFAT build drive. Historical results
below remain scoped to their original project templates/receipts; the current
sample template has changed and its new evidence is separate.

The script/adapter templates previously used core/view HintPath DLL references.
From clean outputs, the old adapter project failed CS0234/CS0246 for Lea.Engine.
New net48 layouts select relative core/view ProjectReference entries; explicit
LeaBuildFromSource=false keeps binary mode. The existing script-parent reference
is preserved. A guard rejects explicit source mode on non-net48 targets, but its
wrong-target failure path was not runtime-tested here. No engine aliases or
execution/security budgets changed, and root prototypes were not used.

## Source And Candidate

Test-SdkSourceGraph.ps1 stages current35core/10view sources from the exact
C5CC/4EE7 font receipts, all15recovered parent files, original adapter sources,
current A228/196bootstrap and relative net48 project metadata. Four products
are compiled from source; only six third-party DLLs are dependency inputs.
Explicit target metadata already in the sources is retained without duplication.

Tested project root: I:/LeaBuildStorage/sdk-source-graph-20261109-v2/source.
72source/project/resource/sample files are inventoried by the audit.

| Product | SHA256 |
| --- | --- |
| Lea.Engine |11B154A135F6A1B455080A384AF281759EF09DB18347C15C805149153A8C362A|
| Lea.Win |61DA2736DB46AE97B2D2D42DD141AEF2A691693CEE7013547BEFCEDD7562813F|
| Lea.Script |406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F|
| Lea.Script.Win |58E4D8D8D2CB32A0A5791198FD7635554A579A5625A10AF3D542A51513268FDA|

New core metadata/compiler output differs from C5CC; new checks belong to these
actual outputs. Identical view/parent hashes do not transfer prior full-suite
results to this new core combination. No installed/browser binary was replaced.

## Verification

V1harness stopped after the expected old-project failure because its matcher
also demanded an MSB warning that was not emitted. Raw receiptF201D5EF and recipe
are retained. V2fixes that matcher and reuses the exact negative run after source
hash equivalence checks, without rerunning the unchanged control for green.
Old compile4906ms/retire97, exit1/Job0/drained/no kill/timeout.

V2graph receipt E9F481961EDB427F255715F51F28F4ED0E9D675EEA79C10D5E97B772C339A8BF
at sdk-source-graph-20261109-v2/result.json on I:.
Automatic source build37948ms; binary mode12539ms succeeds despite an isolated
`#error` in its core-source project; consumer compile4538ms; embedding4779ms.
Retires106/33/35/95ms, all0/no timeout/kill/Job0/drained, inputs stable.
14embedding checks cover computed30px font, visible text, nonblank rendering,
selection payload and pixels at720/360. Desktop plain/narrow selected PNGs viewed:
nonblank, correctly wrapped, selected text visible.4runtime bytes+ACL stable.
Owner86720 guarded exit0; stopped V1owner67495 also guarded exit0.

Follow-up Test-SdkSourceGraphContracts.ps1 receipt
71742BFD3760DF59952F4C7139CA562657F0600D32540F840CC80703E37D34D1 at
sdk-source-graph-contracts-20261109-v1/result.json on I:.
14checks cover four net48 attributes, actual assembly-reference versions and
adapter null/attach/detach contracts.28existing signed-relative-font model cases
were freshly compiled and run against the new core, including10rejection controls.
Compiles2954/3197ms, runs624/1130ms, retires102/33/73/96ms; all0/no kill/timeout/
Job0/drained.12runtime bytes+ACL and inputs stable; owner54799 guarded exit0.
An initial preflight stopped before child/output creation; initial failing
criterion was not captured. Same retained shell rechecked all exact predicates
false and reserves above threshold, then started the fresh unchanged-limit test.
contract-preflight-observation.json SHA
F70C43763908D7AB5D9F30663D297EBC425B0D31E38A35D0587FFD463ABB6374 retained.

Audit-SdkSourceGraph.ps1 independently rechecks9owned runs (one negative), exact
receipts, input/output bytes,16runtime byte/ACL records, project edges, poison
isolation, all three test footers and copied raw evidence/screenshots.
I:/LeaBuildStorage/sdk-source-graph-audit-20261109-v1/audit.json SHA
DFBE3C8A161FA6108926AEB5856E5F75F5F4962596111E981A065EB559E873CB.

## Remaining Work

56focused checks do not certify the complete SDK. No page JS or worker executed
in the follow-up contracts; image/printing/native-worker deployment, source-only
archive/dependency restore, full scripting/security/lifecycle and actual
Firespider migration still need qualification. Startup failure and full-v4 ACL
quarantine remain unresolved. All six release gates stay OPEN; no percentage
or general speed/compatibility claim. SOURCE_GRAPH.md in Lea.Sdk describes use.
