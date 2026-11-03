# Printer-Friendly Capture

2026-10-28 live capture checks pass; host migration still unfinished.

Image-aware print layout2026-10-28: Pi now retains normalized bounded BGRA pixels
alongside its generated PNG. Mi.Cp creates a frozen BitmapSource from raw pixels,
not an encoded image/URI decoder. Grayscale converts RGB luminance and preserves
alpha. Explicit Pm.Ci validates complete snapshot-index image map (max16 images/
8MiB encoded, per-image raw pixel limits still apply); flow uses InlineUIContainer
image controls fitted from intrinsic dimensions to page bounds. Explicit Pw.Di
permits image-only pages and records zero unresolved images only after validation.
Old Pm.Cp/Pw.Dc preserve alt-placeholder behavior. Current sizing is intrinsic
page-fit with line clearance, not full CSS width/height/object-fit/multipage proof.

Actual DLL750E606AD95F3AF0856BB8C73749A0F92F1A8D63ED77438325B87907749EAFCF
on A921 view passes own395 prior checks, project result SHA256
E1710956780923B633CF36BE5548C32440A8D00F56295446BF88DD4CDCB4AC0C.
Build2162/2794ms per120s, behavior20423/4751ms per30s; original exit0/Job0/drain/
no kill/timeout, max147ms retirement, unchanged inputs; TTY93069 guarded clear/
retired0. New Pm/Pw inverses reconstruct preceding data-image sources,3 negative
controls each; all older source projections retained. New Mi/Pi additions held.

Exact-DLL layout-live-v2 receipt SHA256
47844E96DE6713B81439ECFB0E83C639D1BC3388BD206B52718DC6B8D7C78645 passes
222 overlapping focused checks (86 snapshot+136 live,16 WPF image checks).
Real decoder-filled map rendered into actual Pp page; red/blue pixels and two
grayscale luminance regions remain visible. Frozen sources/channel order/alpha,
flow image elements/text, missing/foreign/null map/wrong thread, image-only plan,
XPS export/page count/image resource checks pass. Color v1 and gray v2 PNGs
visually inspected. Compile1568ms/120s/run11278ms/30s, original exit0/Job0/drain/
no kill/timeout, max166ms retirement, inputs unchanged; TTY69288 guarded clear/
retired0. V1 passed220, retained; v2 adds actual grayscale page checks, not retry
for a failing gate. No independently rendered saved-XPS image comparison or valid
printer dispatch proof. General image alpha/CSS/layout/multipage cases incomplete.
Read-only ACL audit6A405D0D23A3D0C3CD264AE92C7389362F90F4210A39ADB6D3F9CAD85C21F791
classifies same fixed decoder RX+Synchronize ACE/3 negatives; raw ACL changed and
decoderSecurityQualifiedfalse remain, no filesystem edits/security waiver.
Next ordinary selection/pagination fidelity and real host routing with options.

Embedded-image recapture2026-10-28: Ps friendly mode retains an internal CacheKey
for data resources, bounded by existing snapshot character budget. Public Image
still empty and CacheKey is not public; normal Cp/Cs unchanged. Ci resolves that
key only through V.Pix's exact active-page/resource/cache/readiness guards; no
URL fetching, encoded-input decoder or broader resource policy is introduced.
Large data keys additionally consume friendly snapshot budget and may fail before
Pi's image-size limit. This is explicit fail-closed accounting, not unlimited data.
Get-PrintImageKeyBaseline reverses only these six source edits and3 negatives
(missing property, unrelated source mutation, duplicate source) are rejected.

Actual print DLL4405C081AC9073FEA2F4940A9F49E54D7B14B11D907EA837B4C290FC7843CA3E
on unchanged A921 view passes own395 checks, project receipt
49ECE0540A0D9544B051728E250BA61C3910392499E3FE226B6D307B21BE58A5.
Build3994/1921ms per120s, behavior20392/5114ms per30s; original exit0/Job0/drain/
no kill/timeout, max202ms retirement, unchanged inputs; TTY56628 guarded clear/
retired0. Existing source inverse checks and image-key3 negatives pass.

Actual-DLL data-image-live-v1 receipt SHA256
42A506FAB12844444563A0090BE1956DC786AC1FF91FB54A4B2FDB03FBA69CFC passes
206 overlapping focused checks (86 snapshot+120 live). Real decoder-filled data
image is captured again through Ci with no new decode, then second-copy HTML
contains an embedded PNG. Public metadata redaction/visibility verified. First
copy reopened/pixel checked and PNG inspected; second generated copy not yet
independently reopened in a third view. Compile1528ms/120s/run11574ms/30s, exits0/
Job0/drained/no kill/timeout, max206ms retirement, inputs unchanged; TTY17250
guarded clear/retired0. Worker raw ACL delta/securityQualifiedfalse retained;
read-only E0A790308590F2F4B328B7BA1E2E6D4E9C4DCC2C3A81781D02C462AE1C32E729
classifies one fixed decoder RX+Synchronize ACE,3 negatives; no filesystem edit,
profile deletion/global security certification. No browser EXE/command adoption.

Real decoder roundtrip2026-10-28: print-decoder-roundtrip-v2 receipt SHA256
74F8D56516299EABB482CF267602FC9120CC7EC4DBD6F8CBB6591552B9407789 passes
202 focused checks (86 snapshot+116 live, overlapping prior suites), including10
real image-path checks. Pinned workerFE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535
decodes a generated32x16 red/blue PNG into the real cache. Ci/Ht embeds generated
PNG without the original source path or another cache decode. A fresh view loads
that HTML, decodes its data image and renders both colors (>=100 pixels each).
Source pixel equality/hardening flags/CPU budget and article text pass; screenshot
decoder-copy.png inspected. Uses actual A921 view/4E88 print DLL, not embedded
production sources. Separate synthetic guard fixtures remain explicitly synthetic.
Compile2615ms/120s, run18606ms/30s; original exit0/Job0/drained/no timeout/kill,
max308ms retirement, inputs unchanged; TTY39873 guarded clear/retired0.

V1 failed initial cache-population assertion, receipt
22876DB5915A7D2867E6C3A32EE1C2C576EFE761C68E85FE8F51FAF323413412.
Test Ready could skip the first paint cycle if Busy was initially false; added
Refresh/DoEvents before its existing8s readiness sublimit. No assertion/deadline
removed; original30s outer budget retained. V1 owner66955 guarded clear/retired0.

Raw worker ACL changed. Audit-PrintDecoderRoundTrip.ps1 observes read-only:
B15F6CA7BF31E5DF9104C1F840F131726F9064ECD2F4585DDAAF0C546CCCEF6D.
Exactly one explicit fixed decoder SID RX+Synchronize allow ACE; removing it in
memory exactly reconstructs baseline. Write/owner/extra-ACE negatives rejected.
No actual ACL edits, rawAuditPassedfalse, decoderSecurityQualifiedfalse, no profile
deletion or global filesystem/security certification. Do not waive broader gates.
Original data-URI source snapshot keys, WPF image placement, general image/CSS
fidelity, page counters/selection/host adoption and new-view full scripting remain
unfinished. Roundtrip success does not qualify those other paths.

Cache bridge2026-10-28: additive Lea.Win/PrintImages.cs builds alongside six
unchanged mature sources (not root prototype replacement), DLL SHA256
A921DDF96C9648014305DE36D40A165E55C387125B6B73C41AB5CF4A3D0B0AEF.
V.Pix requires UI handle/thread, exact active page, ready state, active PicSources
membership, present decoded cache and2048axis/1048576pixel bounds. Returns a fresh
caller-owned32bpp bitmap; never starts fetch/decode or changes cache/accounting.
Pc keeps original page identity, Ci.Cp(V,Pc) builds immutable bounded image map,
rejecting stale/busy capture and unavailable image keys. No production reflection.

New matching print DLL4E88BDA503BACEAB0DB3BD748E33133810D05471548066B06451CF4BB99E0F9A
passes prior395 checks with new view, cache-project-v2/result.json SHA256
3E1B7958720F1097FFC7063FFC6CDD6664EABBF995E0CBDDECBF4CA0FCD20109.
Build4634/2438ms per120s; standalone21180/range5649ms per30s; all original exits0/
Job0/drain/no kill/timeout, max321ms retirement; inputs unchanged; owner2673
guarded clear/retired0. V1 precedence bug in runner rejected Ht BEFORE compiler;
guarded owner34393 retirement0, parentheses fixed and refusal retained. Older F14
timeout remains unresolved, not erased by success on a changed candidate.

Focused exact-DLL cache-bridge-v1 receipt SHA256
BBE697313E898A3E5ABEAF0C6CBB8D651E7DDD5FE3C034FD6993438D7291FF13:
86 snapshot+106 live checks (192, overlaps prior395). Ht/Pi/Ci come only from
actual DLL. Compile1114ms/120s, run9635ms/30s, exits0/Job0/drain/no kill/timeout,
max275ms retirement, unchanged inputs; TTY23224 guarded clear/retired0. Synthetic
cache bitmap tests exact-page guard/immutability/embedded HTML, field mutation/
view disposal/cross-thread/busy/size/missing guards. Test reflection injects a
decoded fixture and restores prior cache/accounting before disposal; not a real
decoder-filled cache or permission grant. Old source-cache run184 checks receipt
8FEAE4FB93C89AF62D774FDC59D247B1647A952F6033C3F094704F194AE7BC69 has
terminal child Jobs/no leases; its terminal handle44013 vanished across environment
transition and owner pwsh absent. Parent final status unobserved, not claimed0.

Remaining: real isolated-decoder cache population and embedded PNG reopen/pixel
ownership tests; snapshot addresses for original data-URI images currently omitted
and thus fail closed in Ci. WPF image placement, HTML page counters, ordinary
selection, full new-view scripting/security/lifecycle and q945/q946 routing remain
unfinished. No Firespider EXE/package updated or release gate closed.

Image preparation2026-10-28: Pi.Cp(Bitmap) accepts decoded pixels only, redraws
into a fresh32bpp bitmap and encodes a bounded PNG. Limits2048 per axis,1048576
pixels,4MiB encoded. Ht.Cp overload accepts IReadOnlyDictionary<int,Pi> keyed by
snapshot image index, max16 images/8MiB encoded aggregate. Rejects missing/null/
nonimage entries and selected-text image maps. No source URL is copied or fetched;
only generated PNG data is emitted, with intrinsic dimensions and escaped alt.
HTML policy now permits data images but no other resource origin. Text-only
overload still emits alt fallback; callers must explicitly choose image mode.

Source-harness images-v2 receipt SHA256
EB28FB5B06E84C77D507BC3ACEB863FE8B89C6E046986D0088B5DBCBFB2AAEB0:
86 snapshot+83 live checks (22 image-specific). Compile1069ms/120s, run8056ms/30s;
original exit0/Job0/drain/no kill/timeout, max291ms retirement, unchanged inputs;
TTY96167 guarded clear/retired0. Verifies source mutation/disposal detachment,
known PNG pixel readback/signature, bounds/missing-resource/selection guards and
exact encoded-stream limit. Earlier images-v1 passes165 and remains retained.

Not yet a live image-capture implementation: mature V.pics is private and Pc
has no image API. Pi/Ht compiled into source test executable against old453968A8
snapshot DLL, not a rebuilt print component; no production reflection permitted.
No decoder spawned/reopen image screenshot/network grant/native job/browser route.
Next bounded UI-thread cache-only pixel capture in a separately staged mature
view, isolated decoder roundtrip and pixel tests, matched library/full regressions.
WPF print image layout and HTML page counters remain separate unfinished work.

Timeout investigation: Test-PrintSuiteTiming.ps1 pins original suite EXE/fixtures,
F14AD958 candidate DLL and mature core/view, then invokes the ORIGINAL assembly
entry through a separate .NET4.8 STA host adding elapsed Console line traces.
No assertions/fixtures removed, same30s owned child deadline/5s cleanup; logging
host nevertheless changes entry/hosting and is diagnostic only, not gate parity.
Timing-v1 result SHA256
EC418415C8906597C7E78D32DAA04C9479F1323F1A0BBA885304FC19C85D18AB passes
original final footer in18358ms process time (last trace17227ms); compiler505ms.
Original exit0/Job0/drained, no kill/timeout, max213ms retirement, inputs unchanged;
TTY79268 guarded clear/retired0. Largest inter-check gaps: XPS export2473ms,
initial load1954ms, ready capture1326ms. These intervals include all work since
previous log, not isolated function benchmarks. Prior standalone30s timeout not
reproduced in changed host; cause unresolved. No range suite/full395 qualification.
Next inspect standalone-vs-host startup/runtime/IO differences before broad retry;
never substitute timing host or longer budget for original qualification gate.

Actual library follow-up: F14AD95878DC0824BA02DF146F8047D5BC750062B30A250CA42236E7C650ED6C
builds with Ht in the real project. Full attempt print-html-project-v1 receipt
7586782F4CFEBBA5893B75647CEF29B1C5966474714430A10233E02AC0FB79A6 FAILED:
build3628ms/120s exit0; prior capture30011ms/30s timeout, owned kill/exit57005,
Job0/drained,117ms retirement, inputs unchanged; TTY50230 guarded clear/retired0.
Last stdout styled gray body-pixel checks, no final capture footer; range suite
not launched. Not a completed395-check regression. No unchanged retry. Reported
space drop from mixed table is not reliable; explicit readback685371392 free.
Current memory/pagefile observation does not prove timeout cause.

Focused actual-library receipt print-html-library-live-v1/result.json SHA256
F66CAFEF5BE6D65A4968867E1C3913888A7832E2D88110DB7AA3781A5AC39828 passes
86 snapshot+61 live/HTML checks. Ht comes ONLY from DLL, no embedded Ht source.
Explicit htmlSourceHarnessfalse/fullProjectRegressionPassedfalse prevents this
result from standing in for failed full gate. Compile1540ms/120s, run8493ms/30s,
original exit0/Job0/drained/no kill/timeout, max198ms retirement; inputs unchanged,
TTY72991 guarded clear/retired0. No native job/browser EXE/package changed.
Next diagnose long prior-suite phase with original deadline intact, then complete
HTML delivery limitations/selection and actual host route. Earlier source-harness
descriptions below retain their historical narrower boundary.

HTML delivery preparation: Ht.Cp(Ps,Po,string) generates a fresh inert document
using XmlWriter with allowlisted semantic elements and numeric table spans.
No page scripts/events/style/resource URLs/base element are copied. Links allow
credential-free HTTP(S); header addresses omit queries/fragments and local paths.
Selection-only mode escapes literal text and preserves line breaks; blank selected
text fails closed. Input4MiB and expanded HTML16MiB caps prevent escape expansion
from bypassing output bounds. Header/footer and orientation/margin/scale/grayscale
CSS options are emitted. Source project now includes Ht.cs/System.Xml.

HTML-v3 result SHA256
BC8071FFC688F6F5C86AA0ADE721516AB972B931E90EF1F3C5EA35F459364E02:
86 snapshot+61 live checks, including21 HTML checks and reopening in a fresh Lea
view. Compile1602ms/120s; behavior7457ms/30s; original exit0/Job0/drained, no kill/
timeout, max186ms retirement; inputs unchanged; TTY10608 guarded clear/retired0.
friendly-copy.png visually inspected. V1 checks passed without reopen; V2 exposed
screen Page0 during visual review; V3 moves counter to print-only styling, and
checks screen footer text/hidden counter. Earlier outputs preserved.

IMPORTANT: Ht compiled into test executable against actual453968A8 snapshot DLL,
not a rebuilt delivered print library. No Firespider routing/package change.
This is not complete HTML fidelity: images emit alt text, not fetched image bytes;
computed source styles are replaced by fixed print typography; generated CSS
markup does not prove scale/grayscale/@page/counter behavior in Lea's HTML print
renderer. Print counters and real multipage HTML require further qualification.
Ordinary document selection remains missing; caller-supplied selection tests do
not qualify selection capture. Complete those paths before claiming q945 parity.

Expanded live-v2 receipt SHA256
4C9C1B0EAFEE853A7FBD7D7E81B8E6A9C4FAD6A18835B10C870DEA59870A71C8:
86 snapshot+40 live checks against unchanged453968A8 DLL. Three additional option
combinations exercise portrait/landscape, .35/.5/1.25-inch margins,75/110/125%
scale, header/footer toggles and grayscale glyph Fill values in actual XPS.
Compile1589ms/120s, run8194ms/30s, original exits0/Job0/drained, no kill/timeout,
max252ms retirement, inputs unchanged, original TTY72277 guarded clear/retired0.
This is text fixture output, not full image/CSS fidelity or native printer proof.
Legacy q945 opens an HTML copy in a browser tab and has seven options. Migration
must preserve that delivery workflow and selected-text behavior; detached preview
alone is not parity. Ordinary document selection and HTML-copy delivery remain
unfinished. C recovered above reserve; no host or package changed.

Latest live qualification: print-friendly-live-v1/result.json SHA256
FCF8A45412F53D9D5BD772525337B5FC734948C57D606A821DC83ED93ED8E232.
Actual453968A8 candidate DLL passes40 baseline+46 friendly snapshot checks and23
live checks. Compile1850ms/120s, behavior7268ms/30s; original exit0/Job0/drained,
no timeout/kill, max318ms retirement, unchanged inputs, owner1904 guarded clear
and retired0 through original TTY43457. Readiness/UI-thread/busy guards,
focus/viewport/page preservation, native selection/password privacy, print-media
filtering, ordinary capture unchanged, detached WPF preview and XPS content pass.
friendly-live.png visually inspected: readable body/header/footer, excluded chrome
absent. No native job or host command executed. New live harness/fixture retained
in source and copied evidence; supplemental ZIP pending because C reached0 free.
No browser EXE/package changed. Next restore reserve, checkpoint live harness,
then preserve options while routing actual q945/q946 with matched host tests.
Earlier statements below about Cf being untested describe the prior boundary.

CURRENT: direct new project library453968A8D218CBCEA15E417F9218713C1BC2496D6CE598AA92425E7D3E104B79
compiles and passes395 prior foundation/range/device/native-dialog/ticket checks.
Receipt120081FE1A8B946D16D36BCD4EA889AD24F43FAF80FEAFAB12EAD6986F094541.
Build3699/2557ms within120s each; behavior21124/5620ms within30s each. Original
handles/exits0/Job0/output/no kill/timeout, max253ms retirement, unchanged inputs,
owner8289 retired0. Ps/Pc source inverses independently pass3 negatives each.
This qualifies old functional paths on the new library, NOT direct Pc.Cf live
behavior or host routing. The86 focused snapshot checks remain separate evidence.
No Firespider EXE/package updated. Next direct live friendly capture/options and
actual q945/q946 route; all release gates open. Storage recovery in STORAGE_CLEANUP.md
restored headroom without reducing600MiB initial/512MiB runner reserves.
Full matching component checkpoint print-friendly-project-source-20261028.zip
verified227 files/6335646bytes, SHA256
6D496652B6AF27A48C078DA2F07E82AD4FF12B8615E37E45B60043957CA324EB.
Includes current print sources, candidate binaries/test sources/receipts and
projection/build recipes; supplements separate mature engine/view source archives,
not complete SDK or off-device backup. This index postdates archived documents.

Pc.Cf(V,Pg) source bridge added after the focused v2 snapshot run. It requires
geometry and delegates to existing UI capture checks with an explicit friendly
flag; only that mode calls Ps.Cf over print styles. Ordinary Cp/Cs remain on their
old path. Compilation now verified above; new Cf live UI/detached friendly output
and actual host route remain unverified. The86 focused tests apply to Ps/Py.
Get-PrintFriendlyViewBaseline reverses only its method/signature/expression changes
to old Pc.cs957D5527198B6D296C7396F59F060268F6E968A189A21850B6AB5E01DFC91754.
Source-only view-source-projection.json passes3 negatives (unrelated selection
edit, missing method, duplicated source); helper/source hashes retained there.
Friendly full-project runner now checks Ps and Pc independently; syntax passes.
The initial source-only stage preceded the new395-check library result above;
still no browser binary update.

All mounted drives checked below1GB free (C~289MB/D424MB/L86MB, others smaller or
unavailable); user asked for larger drive or a movable personal file. Read-only
ShadowStorage query failed initialization: no conclusion about snapshot storage.
Do not weaken build reserve or repeat compression without an identified benefit.
Source-only bridge audit SHA256
92614130940544D8FC816E35E56D22CCB4CCBD611B144A043DAF080E3CCCD503.
Local checkpoint print-friendly-source-20261028-v4.zip verified59 entries/
641114bytes, SHA25668C9ED10062F0F89801BBDFC8A4006E4A73E0139D209FBBCCC22733EB5002B2F.
Current source/recipes/verification boundaries retained; local supplement only.
This index postdates archived documentation.
Subsequent user-authorized archive reclamation removes two old verification
copies only after complete recoverability checks. C~358MiB remains below build
reserve; current feature source/binary status unchanged. STORAGE_CLEANUP.md has
receipts and retained recovery archives. No new library/host test claim.
Three more historical stages now archived/verified and raw copies removed;
cumulative readback22 packages/26814 recoverable files. C~394MiB still below
reserve, so component/host verification boundary unchanged. STORAGE_CLEANUP.md.

Corrected v2 receipt SHA256
5F80BA111884A05F64791EBA3837EA75CF8421C115B58219CACBEB25B3DC5FBF
passes40 unchanged snapshot checks +46 friendly checks on the current Ps/Py
compiled into a .NET4.8 test EXE against pinned mature coreA85B02C6. Compiler
1594ms/120s, test1764ms/30s, exit0/Job0/output/no kill/timeout, max154ms retirement,
inputs unchanged, owner19521 retired0. C recovered above reserve before this
launch; cause not attributed. No production DLL or browser replacement.

Get-PrintFriendlyBaseline.ps1 reverses only scoped added methods/anchors and
matches old Ps.cs547AA5CC exactly. Source-only audit in v2/source-projection.json
passes3 negatives (unrelated edit, missing helper, duplicated source). Direct
Test-PrintProject -Friendly -Range -Device -Dialog -Ticket now recognizes this
change explicitly; its other source pins, tests and budgets remain. Initial
full project launch stopped before staging/compiler at600MiB reserve, owner27340
retired0; C418750464 free afterward. No full395-check/new library result yet.

Existing Firespider q945/q946 in the retained host still casts to WebBrowser,
opens PrinterOptionsForm and builds a separate simplified HTML page. Lea cannot
take that route. Do not silently substitute ordinary print preview or claim the
command migrated before options/selection/cleaned content and actual routing pass.

New opt-in Ps.Cf overloads in Lea.Print/Ps.cs build a detached filtered snapshot.
They remove nav/aside/footer/form/menu/dialog/video/audio/canvas subtrees, exact
navigation/banner/complementary roles and whole class tokens ad/ads/advert/
advertisement/cookie/cookies/popup/modal/share/social. Existing script/control/
hidden/style guards and tree/character limits remain. Ordinary Cp/Cs do not
enable this filter. Input DOM is never rewritten; resources remain unfetched.
This is filtering only, not article extraction, reader typography, HTML output,
selection support, UI options or a finished printer-friendly experience.

Test-PrintFriendly.ps1 compiles the current Ps/Py into a .NET4.8 test executable
against mature A85B02C6 core, includes unchanged PrintSnapshot.cs regression
checks and new semantic/class/role/content/resource/style-map/limit checks.
Original owned compiler120s/test30s/retirement5s limits and600MiB reserve remain.
No current browser or production Lea.Print DLL was changed.

V1 compiled but FAILED at relative-link assertion in the new fixture. Engine P's
constructor records origin but does not initialize U; test helper now explicitly
sets U to its URL (matching existing PrintSnapshot helper). No resolver behavior
was changed to clear this failure. Corrected source passes v2 above.
V1 compiler/test original handles exited, Job0/output/no kill/timeout; owner42452
retired0. Full failure evidence retained in print-friendly-v1. A subsequent V2
launch stopped at600MiB reserve BEFORE output creation/compiler/test; owner70907
retired0 with no child or ledger. C observed391598080 bytes free, cause unknown.

Next: restore safe build headroom without deleting source/current candidates or
failure evidence, run the explicit Friendly full project mode, then add styled
UI capture/options and an actual q945/q946 Lea route with host regression.
Without -Friendly, old source pins deliberately reject changed Ps.cs. Old395 checks do not apply
to changed source. New source checkpoints are local supplements, not portable SDKs.

WIP checkpoint print-friendly-wip-source-20261028.zip verified26 entries/
360994bytes, SHA2568CCD3A93594A7C5291322724AC158A76EE71DB4262D586107B02434F3EB61F82.
Contains current source/docs/recipes and V1 failed evidence. Small bounded backup
only (979857 input bytes,32MiB+2x-input free-space guard), no build/test reserve
was lowered or bypassed. This archive index postdates its documentation contents.

Follow-up print-friendly-source-20261028-v2.zip verified45 entries/578937bytes,
SHA2563EA427121137EA881C0EE863DD61F2E0B52A5837D3AB43E087A62447D08B33F5.
Includes current source, project/projection recipes and passing v2 plus failed v1
evidence. Same bounded backup-only guard; all build reserves unchanged. This
archive index postdates the archived documentation.

Headroom follow-up: two historical compression batches preserve all bytes/ACLs/
times/streams and delete nothing, but C falls to282984448 free despite reported
allocation savings. Both complete; full project not launched. Source-only project
runner PowerShell syntax verified. STORAGE_CLEANUP.md records scopes/receipts;
no engine performance or cause of space consumption inferred.
Local follow-up print-friendly-source-20261028-v3.zip verified55 entries/
637057bytes, SHA2566C69F39D4B857B54C721002AD9595264FE3CCE7ABC8A650A21216BBA3B803309.
Includes cleanup recipe and preservation receipts with current feature source;
backup-only, no build reserve change. This index postdates archived documentation.
