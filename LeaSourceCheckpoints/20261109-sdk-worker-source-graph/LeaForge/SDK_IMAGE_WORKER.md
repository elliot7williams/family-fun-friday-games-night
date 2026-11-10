# SDK Image Worker Increment

## Current Source Integration: 2026-11-09

EmbeddingSample.csproj now auto-selects sibling source projects without an
explicit LeaSdkBin. It builds core/view/image and copies the decoder beside
the executable, keeping ReferenceOutputAssembly=false. Explicit binary paths
do not rebuild sibling projects and copy their decoder when available. Default
binary lookup now follows Configuration, rather than always Release.

Test-SdkImageSourceGraph.ps1 stages75source members and six pinned third-party
dependencies, with no seeded Lea product binaries. Clean sample build produces
core/view/image; separate adapter build produces parent/adapter. Exact library
hashes11B154A1/61DA2736/406C5634/58E4D8D8 match the preceding source graph.
New decoder SHA256:
DA3BFA671C54EEFD78092A9B91CB080189D8681BF6433C2F9C75AC40118D4E91.
Both decoder CS inputs retain their original hashes; no decoder behavior edit.

Producer receipt177FDA95290C6AEC605325072AEB598CC7A39A5F552910BA3FD1083DD093648F
is at I:/LeaBuildStorage/sdk-image-source-graph-20261109-v1/result.json.
Old consumer control builds but omits the decoder. New explicit-binary consumer
builds despite deliberately unbuildable sibling source projects and deploys the
correct worker. Fresh NTFS sample staging passes14text and42file/data PNG view
checks at720/360. Desktop file/narrow data screenshots viewed: visible red/blue
pixels, nonblank framing. Native sandbox runtime is not qualified on exFAT I:.

Current Test-DecoderRetirement.ps1 -ImageSourceGraph -RealDecoder adds81checks;
adding -ShutdownRace adds14. Receipts under continuity-evidence:
current-image-source-decoder-retirement-v1/result.json SHA
2B4E25C2043E047F53ED953EC0FF5F66DAD3B7F14EB627C36735B25E42C24CD1;
current-image-source-decoder-shutdown-race-v1/result.json SHA
A1F6D0459DA0C59AF142824AED4243076373BE2D7C8EEF85D9D912A342A05E28.
Original120scompiler/30stest/5sretirement budgets and assertions unchanged.
All11owned runs exit0/Job0/drained, no timeout/kill, inputs/byte/ACL stable,
decoder descriptor restored; owners93028/63589/25963 guarded exit0.

Audit-SdkImageSourceGraph.ps1 verifies all75source members, deployed identities,
receipts, ACLs, ownership/deadlines, controls and151check footers. Audit at
I:/LeaBuildStorage/sdk-image-source-graph-audit-20261109-v1/audit.json SHA
6C654D97E50047ABEE9BB2F3FA6C3030B0732B0596F74127E4572B644DE12A4E.
First audit recipeB9BC0BB0 and explanation remain in
sdk-image-source-graph-audit-failure-20261109-v1 on I:. It incorrectly rejected
the historical decoderProfileChanged=true field, which the runner sets for
every real-worker campaign. Corrected audit preserves/requires this flag and
realWorkerCampaign, without changing original receipts or native tests.
The flag alone proves neither a policy change nor universal security.

This qualifies only these build/deployment, PNG view and lifecycle fixtures.
Printing, JavaScript worker/full scripting, crash recovery, broad image formats,
security/sites, Firespider integration and clean-machine release remain open.
No installed browser or startup policy changed; all six release gates stay OPEN.

## Historical Standalone Worker

2026-10-31 standalone net48 project Lea.Sdk/Lea.Image.csproj stages the two
original tested decoder sources without changes. Prepare-SdkImageWorker.ps1
binds them to original receipt09690529... and its explicit source-input pins.
The original FE744BE1 worker targeted net461; this new framework/project build
has a different identity and needs its own runtime evidence.

Staging: I:\LeaBuildStorage\sdk-image-worker-20261031-v1.
New worker:EF89F8AA04E51921B54D1BAFE889244BF39229E57A08FBD7BFCB0D6B5E098BD3.
Build receipt95F4A5D6DC02D49A9A27DFF1245010385FD4BF349ADA253962D895C814E277A0.
ModernSDK8.0.427/sharedcompilationdisabled,ownedbuildPASS;owner22637retired0.
Only Lea.Image.exe is emitted in its Release folder,no omitted config/dependency.

## SDK-Pair Verification

Test-DecoderRetirement.ps1 -SdkPair requires the real new worker and exact SDK
core772A/view8008. It copies them into fresh NTFS test folders;no ACL isolation
claims are made from the exFAT staging drive. Existing assertions,120scompile/
30stest/5scleanup and original process/Job ownership remain unchanged.

- sdk-decoder-retirement-v1:81checks PASS,3701ms. Includes real red/blue PNG
  pixel decoding,isolation flags,invalid-image startup cleanup,post-create stages,
  cleanup retry and restored original worker descriptor. Receipt
  DC95142ED4BA9E7972A24E1A9F07BF0570ACB1AB6053C940C9815C149BBE69C2.
- sdk-decoder-shutdown-race-v1:14checks PASS,3186ms. Includes busy endpoint
  preservation,retry retirement,admitted-task completion and launch rejection
  after shutdown. Receipt
  7868A8864A905EA44F12A6C21BD1A0723612CEFBA4F8BB31B72A5F5B203CD15B.

Both run pairs exit0,no timeout/kill,Job0/outputdrained,inputs unchanged and
original decoder ACL restored. Owners48528/10157 guardedretired0.
Audit-SdkImageWorker.ps1 independently verifies original compiled-source pins,
exact copied/tested product hashes,terminal footers and lifecycle/ACL receipts.
Consolidated auditB815E839FCFC104F617302A05D48AAF7028F163A801F66B2A34FFCF5A3C0B2AA
at continuity-evidence/sdk-image-worker-audit-v1/result.json.

## Remaining Work

Portable61-filelayout andactualVimage-loading now pass42checks,file/dataPNGs at
720/360/defaultbundledworkerpath/hardening/ACLrestore. SDK_PORTABLE_LAYOUT.md
hasreceipt D5313F3D... andscope. Current source sample integration is above;
complete aggregate SDK/worker/printing qualification remains unfinished.

Not universal security/profile-crash-recovery proof,full UI image-layout fidelity
or complete SDK qualification. Compose portable SDK source/sample/worker layout
and test actual view image loading there. JavaScript parent/adapter/worker,
printing,notices,full host delivery and unresolved legacy UIA/startup reliability
still require work. Browser development ZIPs remain unchanged.
