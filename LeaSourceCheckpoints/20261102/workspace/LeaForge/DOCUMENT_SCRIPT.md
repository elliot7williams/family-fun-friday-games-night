# Matched Document Selection Scripting Campaign

Latest2026-10-29: full shutdown-fixed scripting run passes3412 assertions/all89
phases AND unchanged raw permission/file audit. Exact tested view2225/coreA85;
this supersedes only the current scripting qualification,not retained failures
or unresolved startup reliability/legacy UIA/host/release gates. Details below.

Current normal-exit fix2026-10-29: shutdown-image-live-v1 passes336 actual-library
checks AND exact decoder ACL restoration after normal process exit. Production
shared-decoder shutdown hook,not fixture cleanup. Earlier failed stage retained.
Full changed-view scripting/reliability/concurrent shutdown remain unqualified.

Latest integration result2026-10-29: actual matched print/image functionality
passes336 checks but application-exit ACL restoration FAILS. Production shared
decoder lifetime/shutdown ownership is the next blocking fix (section below).

Current2026-10-29: fresh mature staging now uses unique temporary decoder
profiles. Real concurrency/startup-failure/pixel-decode campaign passes63
checks (latest section below). Earlier fixed-profile campaign details are
historical evidence, not the current launcher configuration. Full scripting,
startup reliability, external legacy UIA and host release remain unqualified.

2026-10-29. Fresh candidate in
continuity-evidence/temporal-startup-v1/document-selection-script-suite-v1.
View926C1A28/coreA85B02C6; no Firespider adoption or final release gate closure.

Build receipt02356A28411B30ADE710F4ECC4579A32CF9991DB8A30216D3FFFFEA8B348DD4C:
60 source files/65 runtime files. Binding receipt
83A2D966562AC9E14F55C88D6842FBEBCB3FA42CF95CD27FCF8BBCFFE655D9F2 verifies
six dispatcher digests, seven embedded fixtures, IL comparison46 methods/42
bodies and eight exact8.0.33 configurations with rollForward Disable.

Owned run in document-selection-script-owned-run-v1, receipt
3AE0FB5B8C8AC6AE96980905D7FD0D6C840F2B5CC725DEE12921D3144DE81676:
3412 assertions/all89 phase ends, terminal footer and trace restoration verified.
Elapsed274227ms within original300000ms; exit0, no timeout/kill, Job0, output
drained, cleanup227ms within5000ms. Original owner97343 guarded clear/retired0.
This is one completed behavior campaign, not reliability or all-site proof.

Raw audit remains FALSE: exactly one metadata-only persistent decoder grant on
staged Lea.Image.exe; runtime bytes and other observed ACLs unchanged.
Audit-DocumentSelectionScriptOutcome-v1.generated.ps1 independently holds and
rehashes inputs and classifies the recorded/current ACL using unchanged
Test-DecoderAclDelta.ps1. Three negative controls reject script path, other
profile and temporary-profile substitutions. Observation SHA256
AC6C9A1C7FFBD01795875399F2D5B92D1173E481AD130914637AD58B5471B412.
No permissions modified, profiles deleted or raw failure cleared. Existing
intermittent failures, legacy managed UIA crash and security qualification remain
unresolved. Do not rerun the changed stage as a fresh baseline.

Prepare-DocumentSelectionScript{Build,Verify,Run,Outcome}.ps1 preserve original
helpers and generate unique fresh helpers by inverse-checked projections with
syntax/readback and negative controls. Budgets and isolation checks unchanged.
Next: investigate permission grant/profile lifecycle and legacy broker crash;
then fresh candidate testing before host migration. All six final gates OPEN.

## Decoder Ownership Finding

Audit-DecoderGrantOwnership.ps1 verifies exact pinned launcher source contracts
against the recorded campaign. Mature Rp.cs SHA2E419AAA and root Rp.cs SHA
DF50F83C both call the decoder with fixed profile Lea.Image.Decoder and
temporaryProfile=false. Both add file grants; only temporary profiles attach
Release to the returned worker. Thus this path does not schedule decoder ACL
revocation. This source finding explains the retained grant, not an exploit or
proof that every runtime path cleans up correctly.

The mature launcher also lacks the root launcher's retained SetupFailure
endpoint. Do not switch the decoder to temporary profiles without covering
failure before creation, failure after creation/Job assignment, successful
retirement and cleanup retry. Do not copy root Rp.cs wholesale: historical
mature source and staged diagnostic hooks remain authoritative for this build.

Next implementation should project scoped changes into a fresh view build:
unique decoder profile per worker, tracked original process ownership on setup
failure, and retryable grant/profile cleanup only after verified exit. Test two
overlapping decoders so finishing one cannot revoke the other's file access;
verify original DACL preservation and zero pending cleanup after success and
injected failures. Retain failed cleanup endpoints rather than erase evidence.
Existing fixed-profile grants must not be removed globally as part of migration.
Observation: continuity-evidence/decoder-grant-ownership-v1/result.json.
No production/ACL/profile changes or runtime test occurred in this source audit.

## Retained Cleanup Prerequisite

Project-DecoderRetirement.ps1 now inverse-projects three scoped Di.cs changes
when Build-PrintImageView.ps1 stages the mature sources: Drop refuses to forget
an endpoint whose process/cleanup remains pending; reuse/Stop handle a null
process after process retirement. A live reusable worker is not rejected merely
because a future cleanup callback is attached. Historical source is untouched.

DecoderRetirementMain.cs injects only a cleanup callback into an isolated empty
endpoint: two false returns retain the identical endpoint/callback/key, successful
retry clears all three, and another Drop is idempotent. No process/profile/ACL
is created by this fixture. The test runner builds a fresh full selection/UIA
view and uses original120s compilation/30s test/5s retirement budgets.
First15 checks pass in decoder-owner-retirement-v1, but its overly restrictive
reuse guard was corrected afterward; v1 is not evidence for the final changed
view. Final candidate/result are decoder-owner-retirement-v2.
Final view338DA036368984236FD2D75BB2D906A3C50FC9346F18C2A52D7A41C2120CBA6A;
receipt0395EE5AA79E3B30A7EEB7EB9A122F8705E675247DD5B8989EE5CE107E6E13EA.
All15 checks pass, build18104ms/compile240ms/test382ms, clean exit0/Job0/drain,
no timeout/kill,max126ms cleanup; unchanged inputs, owner guarded retired0.
Temporary decoder profiles and setup-failure retention are still unfinished;
no prior3412 scripting result transfers to this changed view.

### Failed-Open Ownership

The staging projection now routes post-launch wrapper-setup failures through
RetireFailedOpen: publish the original endpoint with an empty reuse key before
calling Drop. A false cleanup result or thrown cleanup exception keeps that
endpoint reachable for retry. This covers failures after Rp.Open returns, NOT
failures thrown from inside Rp.Open before returning an endpoint.

decoder-owner-retirement-v3 builds view
2C1B1CD230905DD140AF6F0B987273D96B569B86DC88CC96A2AC112D76311813/coreA85.
Receipt1DF912F0E9085ADEE34B5E9CADB71EF81BFAC14A5BB60C3F8B0E79D606D5DAE7:
25 focused injected-cleanup checks pass. Build20869ms/compile449ms/test452ms,
original budgets, exits0/Job0/drain/no timeout or kill,max142ms cleanup,
unchanged inputs, owner guarded clear/retired0. No real decoder/profile/ACL used
by focused fixture. Unique temporary decoder profiles and internal-launch
failure endpoint retention remain next; security/reliability gate stays open.

Matched staged source regression document-uia-v6 passes86 baseline/capture and
201live overlapping checks. Receipt
ABA308086D246989C517ECF3784479577C413178595559EFABC38985A0D9EBE3;
view9A8036CBFC741C9F407E47118CA17C5212F43B3109ACA0B715EA4B7465965514.
All staged view-source bytes match retirement-v3; distinct build binaries are
not treated as byte-identical. Build23112ms/compile1011ms/live9131ms; clean
exit0/Job0/drain/no timeout/kill,max151ms cleanup,inputs unchanged,owner retired0.
Includes direct provider/selection/copy/capture, HTML source harness, no actual
decoder/full print library, external broker, Firespider routing or print job.

## Internal Launcher Recovery Staging

Project-DecoderLauncherFailure.ps1 inverse-projects scoped Rp.cs changes into
fresh builds, preserving historical source and diagnostic stage hooks. Temporary
profile setup failures now retain the exact native process handle/Job, pipes and
cleanup callback in an Rp.SetupFailure endpoint when cleanup cannot complete.
An unassigned suspended child uses its original SafeProcessHandle for termination;
cleanup waits for verified exit before revoking access. No PID reopening added.
Di routes the carried endpoint into its retained worker slot before retrying
cleanup. If that retry fails, the resulting SetupFailure preserves both original
and cleanup errors in an AggregateException with the same endpoint.

Final focused candidate decoder-owner-retirement-v5 view
BD1FFB58F779A2EACF90F2FD7E9E3C0E9BE7D2B2301E725D3AB98828C77CDBC8/coreA85,
receiptCE724408F40C8D78C29A188A31B7C04723E6D5AAE60B70B7F5F927C4CD5C364B:
34 injected checks pass, including manager recovery of a SetupFailure endpoint,
retention of the original error chain and successful cleanup retry. Build19083ms/
compile331ms/test1257ms; original budgets,exit0/Job0/drain/no timeout/kill,
max136ms cleanup,unchanged inputs,owner guarded clear/retired0.

These checks use injected empty endpoints: native CreateProcess failure and
real grant/profile deletion have NOT been exercised. The normal decoder still
uses the fixed persistent profile. Next introduce unique decoder profiles and
test overlapping real workers plus injected native startup failures and ACL/
profile restoration. No security gate closure or transfer of older script proof.
Earlier v4/document-uia-v7 passed28 and86+201 respectively, but preceded the
error-chain preservation change; do not transfer those results to v5.

Final matched staged-source document-uia-v8 passes86 baseline/capture+201live
checks. Receipt4F6042AE3E651A6DA4D586DA8B8BBC164510BA9AD3CEE1E2669CB84501463355;
viewF8F548200906DC3F7D4755620031FC23A74577327FE67C6D753AAD5FE166A330.
All staged source bytes matchv5; binaries differ. Build18084ms/compile1020ms/
live9259ms; cleanexit0/Job0/drain/no timeout/kill,max133ms cleanup,unchanged
inputs,owner guarded retired0. Same source-HTML/direct-provider scope asv6,
not real decoder/native-launch failure/security/external broker qualification.

## Unique Decoder Profiles And Real Native Checks

Project-DecoderLauncherFailure.ps1 now also replaces the default decoder call
with Lea.Image.Decoder plus a fresh GUID and temporaryProfile=true. Existing
fixed-profile grants are untouched. Each worker owns only its unique SID's
grant; verified process exit precedes revocation and profile-delete API call.
Normal reuse remains supported while that worker is live.

decoder-owner-retirement-v8 view
A51EB4931C01F2A6C9B377E8E79F45CE850ABFD0D343C432F1329099FF8FDC1C/coreA85,
receipt8985FC8DAB686202294574F7C4BBBDD7140D44131B3B4ADD42454630D0959B9D:
63 checks pass, including34 prior injected recovery checks. Two actual native
workers use distinct SIDs and both hold access to the same staged EXE. First
retirement removes its grant, preserves the second grant/live worker; second
retirement restores the original descriptor. Isolation flags include restricted,
low, zero-capability container, applied file access and temporary cleanup.
Invalid native image fails before child creation with Win32Exception and restores
its original descriptor. Actual red/blue PNG decoding verifies dimensions and
both pixels, then restores decoder permissions. Parent independently compares
the staged decoder ACL before/after the complete owned child campaign.
Profile cleanup callbacks completed (DeleteAppContainerProfile returned0 under
existing Release); independent profile-store inspection is NOT claimed.

Build18508ms/compile367ms/native4187ms; original120s/30s/5s budgets,exit0/Job0/
drain/no timeout/kill,max150ms cleanup,unchanged input bytes,owner retired0.
Failedv7 retained: real concurrency and invalid-image cleanup passed, then the
pixel fixture supplied zero reservation/host usage and was correctly rejected.
v8 uses positive reservation and actual host private bytes under the same512MiB
combined limit; no engine admission policy change. v7 is not a passing campaign.
The test executable and receipts preserve failure evidence; future fixture
source snapshots should be staged alongside binaries before compilation.

Still required: failure after native process creation/Job assignment, cleanup
retry under actual revocation failure, repeated lifecycle/reliability, full
changed-view scripting/print integration and host adoption. No final gate closed.

Matched-source document-uia-v9 view937D0DBAEEBBDC856A68D3D06B3FE4C5085C4B0C408F402263884E46C23AA872
passes86 baseline/capture+201live checks. Receipt
3F32A044A308A28167CD098A99715FFBCC08307616C417928B0667A9A6C500DD.
Build16575ms/compile957ms/live9906ms,exit0/Job0/drain/no timeout/kill,
max130ms cleanup,unchanged inputs,owner retired0; all staged source bytes
match nativev8, distinct binaries. Same direct-provider/source-HTML scope asv6;
no full actual print library/external broker/Firespider adoption qualification.

## Real Post-Create Failure And Filesystem Retry

decoder-owner-retirement-v9 passes75 overlapping checks on view
8CC105D4B983FC67E66EE97487BE0771AA42626838A8F668DDEC0E6FF99F183D/coreA85.
Receipt0D796C789B451732C87161162BC32EF0890CB99414A22EA71AB8A96C6A5715B2.
No production code changed this campaign; staged source bytes matchv8 and the
document-uia-v9 regression sources. Distinct binaries remain separately pinned.

CPU99 invokes existing Jb validation AFTER CreateProcessAsUser and verified
container-token inspection, BEFORE Job assignment/resume. Exact observed stages:
sid,sid-ok,acl,pipe,attrs,create,token,job. ArgumentOutOfRangeException(cpu)
survives cleanup; original decoder descriptor restored. This executes original
SafeProcessHandle termination/exit wait for a real suspended child without a
new injection hook. Outer owned Job independently reports0 active at retirement.

A real multi-file temporary launcher owns EXE+test sidecar grants. Removing only
the generated sidecar makes Release encounter a genuine missing-file IO error:
process already retired,EXE grant removed,cleanup callback retained. Recreating
the sidecar permits retry/profile-delete completion; no EXE/sidecar grant remains
and original descriptors match. This tests common multi-file cleanup, NOT a
permission-denied revocation failure on the normal decoder EXE. The sidecar and
invalid-image artifacts remain as evidence, no working source removed.

Prior two-worker concurrency/invalid-image failure/real PNG pixels also pass.
Test source now copied into evidence and held before compilation. Build17218ms/
compile434ms/native3382ms; unchanged120s/30s/5s budgets,exit0/Job0/drain/no timeout/
kill,max128ms cleanup,unchanged inputs,original owner guarded retired0.
Post-Job-assignment failure,permission-denial retry and independent profile-store
observation remain gaps. Next fresh full matched print/script qualification;
no final engine/security/host/release gate closure.

## Full Print Pass And Application-Exit Failure

Test-PrintProject.ps1 -DecoderView requires complete selection/image-layout mode
and exact75-check candidate proof. It preserves the old pinned selection mode
and explicitly records currentFullNativeQualified=false/legacyUiAUnresolved=true.
decoder-print-project-v1 receipt
9577146D8D08906833F3717D7047401E3B94FBA810382BA2F4319FCA85AC831D passes the
full print capture/range/device/native-selection/ticket project campaigns with
view8CC105D4/coreA85 and printE525BEC5530E4954F8BD9668E108A073B6208C81DC25E935AE24964FDECD3E46.
Build2313ms/live21074ms/rangecompile2300ms/range4311ms,original budgets,
exit0/Job0/drain/no timeout/kill,max163ms cleanup,inputs unchanged,owner retired0.
No physical print dispatch or browser migration proved.

Test-PrintFriendly.ps1 -DecoderLibrary binds that exact library/view pair without
source Ht/Pi substitution,retains real decoder/data-image/WPF/selection checks,
and additionally REQUIRES the staged decoder ACL unchanged after child exit.
decoder-image-live-v1 receipt
ACD2E48AAD5BAFEF4590753DE0791E6305C00C2BDFADE5E4872427B717FB286E:
86 baseline/capture+250 actual-library live checks pass; OVERALL FALSE.
One temporary-profile read/execute+synchronize grant remains. Compile2187ms/
live14067ms; exit0/Job0/drain/no timeout/kill,max356ms cleanup,inputs unchanged,
owner guarded retired0. Full before/after descriptors retained in receipt.

Explicit worker Dispose restores grants in native tests, but static Di reuses
its last worker and has no ProcessExit/ApplicationExit/shutdown hook; Drop is
called only on replacement/failure/Stop. Application exit is therefore a separate
unclosed lifetime boundary. No temporary grant exception or manual normalization
was applied. Retain this failure; do NOT add test-only Drop to manufacture pass.
Next production shared-decoder lifetime/shutdown design must account for multiple
views/in-flight decoding,retained cleanup failures and bounded normal shutdown;
fresh actual-library exit regression then full scripting. Forced termination and
profile-store recovery remain separate gaps. No final release gate closed.

## Production Shared-Decoder Shutdown

Project-DecoderRetirement.ps1 now stages Di's ProcessExit/DomainUnload handlers.
Shutdown sets a process-wide stopping flag; Open checks it under state lock so
new workers cannot start afterward. Monitor.TryEnter waits at most1000ms;
Drop retires the existing worker using its original owner, retaining an endpoint
on cleanup failure. Shutdown reports false when busy or cleanup is incomplete;
the automatic handler emits LEA_DECODER_SHUTDOWN pending. Repeat calls can retry.
Normal view disposal does not shut down the global decoder used by other views.
This is bounded waiting,not proof of an overall OS cleanup deadline or forced
termination recovery. Native profile APIs can have independent failure/latency.

decoder-owner-retirement-v10 view
2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA/coreA85
passes81 checks including pending/retry/idempotent shutdown/stopping flag.
ReceiptEE71940D73E7A63839DF9806155090858225BC254A6370360ACC7F96EC1B3EC6;
build18631ms/compile479ms/native3885ms,max111ms retirement.
shutdown-print-project-v1 print
D94B951071528562EB1F546E636196B1F02DF03C5965323F96E7A338E179AEC4 passes full
project regression,receipt8B9A1DA0D1B25F6E9EC8BE95F9FC265BFB6E56644A99C71615595BFF1F91D09E;
build2462ms/live22178ms/rangecompile1957ms/range4712ms,max120ms retirement.
shutdown-image-live-v1 binds that exact pair,passes336(86+250) actual-library
checks and parent-observed exact descriptor restoration AFTER process exit.
Receipt3890C80383BFD39523EA695757DF4B4014EBB8C99C69527480EF8996D7483636;
compile1639ms/live13995ms,max205ms retirement. No fixture Drop/shutdown call added.
All original budgets,exit0/Job0/drain/no timeout/kill,unchanged inputs,owners
guarded retired0. Earlier failure/temporary grant stage was not normalized.

Next full scripting on this exact pair; concurrent in-flight shutdown and busy
state-lock branches still need runtime tests,repeated normal exits and forced
kill/profile recovery are distinct. No independent profile-store deletion proof,
legacy UIA fix,host adoption or final release gate closure.

## Full Shutdown-Fixed Scripting Qualification

Prepare-ShutdownScript.ps1 stages a fresh initializer,builder,verifier and owned
runner from pinned earlier helpers. Each projection reverses byte-exactly to its
original,passes parser/readback and three rejecting negative controls. The new
matched pair proof additionally requires decoder ACL restoration. Native/legacy
qualification remains explicitly false/unresolved. No runtime limits or assertion
contracts changed; original helpers and all failed evidence retained.
Preparation receiptE794F06C29A118FA6BB2C27D5A4CF07BAC0C38BB6ED1E2F1505CFC2159F0E463.

Candidate: continuity-evidence/temporal-startup-v1/shutdown-script-suite-v1.
View2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA/coreA85.
Build receipt06780E3D2B16D476AF5A18CDBFB52E476F7093893267F21D6AE1A8FB522196A8,
test executable01920503F569E0A83B54B008374205ACC986467C58AA226C53FB9C7E675C5DE1.
Compile stages9575ms and15966ms,original120s budgets,clean exits/Job0/drain,
inputs unchanged,owner guarded retired0.
BindingDA0ECC3CA70C673A39F716F16C7354A999BA449C3AF520D2D87C836CA9F53593 verifies
60 sources,65 runtime files,six dispatcher digests,seven embedded fixtures,
46 methods/42 bodies IL comparison and exact eight8.0.33/rollForward Disable
configurations. Compilation/binding alone is not runtime evidence.

Runtime: shutdown-script-owned-run-v1 receipt
5F4C37A3C7858E1C8EB95E57147A82FCED3D19D8AAE9E5C97EA1C0C29F39C5E0,
process receiptE84F12C94ABDA847C45D714ED07EE10ED7BEA97032ACB38B98181587A7CAC50B.
3412 terminal assertions/all phase ends0..88,footer verified,trace activation and
environment restoration verified. RAW OVERALL PASS: zero changed runtime files/
permission descriptors,input bytes unchanged. Elapsed283797ms within300000ms;
exit0/no timeout/no kill/Job0/output drained,retirement154ms within5000ms.
Original owner guarded clear/retired0. No pending sessions.

This is one full diagnostic campaign. It does not resolve concurrent in-flight
shutdown/busy-lock coverage,forced termination/profile-store recovery,repeated
startup reliability,legacy managed UIA crash,all-site compatibility,SDK delivery
or Firespider adoption. All six final release gates remain open.

## Qualified Candidate Concurrent Shutdown

DecoderRetirementMain --shutdown-race uses a real decoder with its admission/
handshake completed,paused by a started callback BEFORE request write. A second
thread deliberately owns Di's state lock. Main-thread Shutdown reaches its real
busy-lock timeout and returnsfalse without discarding the endpoint/callback.
Release of the lock permits successful retry/retirement; releasing the decode
callback yields a closed-endpoint failure,completion without worker resurrection,
rejected subsequent Open and exact permission-descriptor restoration. Event
rendezvous provide deterministic ordering; no sleeps,production fault hooks,
deadline changes or stopping-flag reset. Fourteen checks cover this interleaving.
This is not a blocked native response read or an exhaustive race proof.

Initial rebuilt decoder-shutdown-race-v1 passes14 checks (build17272ms/
compile499ms/test3085ms,max140ms cleanup). To avoid transferring a result between
different binaries,Test-DecoderRetirement.ps1 -QualifiedView pins the full
scripting pass receipt5F4C37A3 and copies its exact2225 view into a fresh isolated
directory. CoreA85 and native decoder bytes remain pinned,source staged before
compilation,after-exit ACL checked independently by parent.
qualified-decoder-shutdown-race-v2 receipt
8829C75EE852BBB68BF5164274A6E493AF423A1EAF7030804280A8996567EC08:
14 checks pass,compile511ms/test3251ms,max232ms cleanup,original120s/30s/5s budgets,
exit0/Job0/drain/no timeout/kill,inputs unchanged,owner guarded clear/retired0.
The earlier qualifiedv1 runner stopped before compiling/testing because a local
path variable collided case-insensitively with a SwitchParameter;local name
fixed in v2. Failed receipt/runner retained,not treated as an engine regression.

No production source changed this campaign; previous exact2225 full scripting
proof remains distinct and valid. Next legacy external UIA crash work/repeated
lifecycle coverage. Forced termination,independent profile-store recovery and
blocked native-read shutdown remain unqualified. No final gate closure.
