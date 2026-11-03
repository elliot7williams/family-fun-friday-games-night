# Lea Project Context

Updated 2026-10-28; Git base 535add5, dirty workspace. Read AGENTS.md and check
current Git status/diff. Source and exact-candidate results outrank roadmap prose.
Previous full index (including unresolved history) is preserved byte-for-byte in
LeaForge/history/PROJECT_CONTEXT_20261028_before_measured_selection.md,
SHA256 1DE578C376439F72B4B87964AE40655AA2E518D2E7195DE4158678F9989EF8D1.
Earlier history links remain inside it. Historical CURRENT/NEXT notes are not
current instructions.

## Objective And Boundaries

Finish Lea as an independent lightweight engine embedded in Visual Studio and
Firespider, without replacing it with Chromium/WebView. Short public aliases,
source preservation, broad real-site compatibility and reproducible EXE/SDK
delivery remain requirements. No measured completion percentage exists.
All six final gates in LeaForge/FINISH_PLAN.md remain OPEN: reliable startup,
unified candidate, engine/security/lifecycle, Firespider integration,
compatibility, reproducible delivery. Do not sum overlapping test counts.

Mature core source: continuity-evidence/print-styles-v3/core-source.
Mature baseline view: continuity-evidence/print-live-capture-v1/view-source.
Root Lea.Engine/E.cs and Lea.Win/V.cs are NOT automatic mature replacements.
Do not touch unrelated games, FSIN, wallpaper, dirty work or failure evidence.

## Current Task: Document Print Capture

LATEST2026-10-29 direct native GetText evidence: document-broker-v4 exact2E view
native array VT_UNKNOWN/QI checks pass; validated metadata method order(Clone0,
GetText9 among18 methods) then native COM vtable slot12 GetText succeeds and
returns expected19-character Alpha bravo text,privacy guard passes. BSTR/COM/
array ownership cleanup implemented. Then UIA client visible[0].GetText still
crashes0xC0000005. Receipt2BA2EDDBCD603F203CF21EC7DB334F696D4FC68AED17FD37B4785BA74206C012.
Compile1430ms/120s exit0,run16257ms/30s crash;Job0/drain,no timeout,max354ms
cleanup,inputs unchanged;owner63050 clear/retired0. No engine fix/gate pass.
NEXT test cross-apartment COM-marshalled range read separately from UIA legacy
handle conversion (CoMarshalInterThreadInterfaceInStream/CoGetInterfaceAndReleaseStream
or bounded modern native UIA client). Native UI-thread success does NOT prove
cross-thread lifetime correct or establish a Windows bug. Keep legacy failing
read enabled. No new diagnostic source for this next phase yet.
Cleanup Stage196/Verify196/Stage192/Verify192Earlier(v9) fully verified/removed;
1357files recoverable,original ZIPs retained. Unique historical report explicitly
copied/hash-verified in192-v9-verify192earlier AND metadata base64 before removal;
SHA7EEBF1AC70EE4A37CD4C20DFD95AB04166CA27C400C1580CF53DD6F38EED8C85.
Other192 mismatch/unique-report attempts refused,trees kept unless laterfully
verified. Reclaimer only two named reports<=32KiB allowed as archived extras,
not arbitrary notes; all other guards unchanged. STORAGE_CLEANUP.md details.
C:608448512 after test (<600MiB initial reserve). No sessions running.

LATEST2026-10-29 ABI probe executed in document-broker-v3 exact2E view:
SAFEARRAY type13(VT_UNKNOWN),dimension1,bounds0..4,first QueryInterface for
ITextRangeProvider S_OK/non-null. Probe completes/releases refs+array; then UIA
DocumentRange text succeeds and visible[0].GetText still access-violates.
Receipt77065B2125A9F6A4DC707D0A242767AB94E46E190C92214F7C81B4B012983476.
Compile1561ms/120s exit0,run15545ms/30s exit3221225477; no timeout,Job0/drain,
max135ms cleanup,inputs unchanged;owner52045 clear/retired0. No fix/full pass.
Read-only installed Framework64 WPF UIAutomationClient.dll IL inspection confirms
TextPattern_GetVisibleRanges calls RawTextPattern_GetVisibleRanges,CheckError,
UiaHTextRangeFromVariant at IL offsets3,8,41. Do NOT assume pointer truncation.
NEXT extend diagnostic with validated native ITextRangeProvider.GetText COM call
on first queried interface before UIA client; inspect native call/lifetime vs
client wrapper conversion. Preserve original external crash read and deadline.
No raw GetText probe added yet. Need fresh brokerVersion4 after reserve recovery.
Cleanup explicit Stage197/Stage198/Verify198/Verify197 matched original ZIPs,
1398 recoverable files removed from duplicate trees,metadata retained;~28.8MiB
immediate deltas,details STORAGE_CLEANUP.md. C:609992704 aftertest,below600MiB
initial reserve. No running sessions. Prior pending-ABI source-only notes superseded.

Latest SOURCE-ONLY diagnosis2026-10-29: added DocumentRangeAbiChecks.cs before
broker client phase. It calls provider CCW ITextProvider.GetVisibleRanges slot4
on UI thread, logs SAFEARRAY vartype/dimension/bounds, queries first element for
ITextRangeProvider, releases COM refs and destroys owned array. No raw GetText
vtable calls; external crash check remains enabled afterward. Intended to separate
array/provider-interface failure from UIA client wrapper issue; NOT run/compiled.
Runner includes new probe underDocBroker. Current C:605863936 below600MiB reserve,
no build launched. Next fresh brokerVersion3 WITHOUT BuildBrokerView uses exact
known failing2E DLL plus new probe. Do not repeat disproven default-interface patch.
Need storage recovery before compile; no sessions/background processes running.

LATEST2026-10-29: ComDefaultInterface hypothesis DISPROVEN by document-broker-v2.
ViewDC2F3EFFDA76C1CEADA601B181A7E54CD0FDD85EBAFF0CFF0019ED64E96349FF
compiled with explicit interface; same document-text success/5 visible ranges/
first range GetText native crash0xC0000005. ReceiptBAF23E058888DFA740FBBDE1AFC79A02C7FC88E62EB5375B6E1123AD3F597308.
Build20347ms/120s,compiler1013ms/120s,run12673ms/30s; exit0/0/crash,Job0/drain,
no timeout,max137ms cleanup,inputs unchanged,owner19640 retired0.
Removed unsuccessful ComDefaultInterface hook from CURRENT builder; failed staged
V.cs/recipe/DLL retained. Main source back to2E semantics; no new production fix.
NEXT inspect managed/native ITextProvider array marshaling (GetVisibleRanges and
GetSelection), default interface/lifetime/vtable at array boundary; do not rerun
same attribute idea or disable checks. Broker mode exact-v5 DLL remains repro;
BuildBrokerView explicitly builds next real source change. No sessions running.
To recover headroom, verified/removed old Verify200(357files),Verify199(354),
Stage199(354),retained original ZIPs+metadata. Earlier Verify200 mismatched
Lea.Bench.exe and was NOT removed. Immediate free deltas22556672 bytes;
STORAGE_CLEANUP.md has receipts. C:607666176 after test (<600MiB initial reserve).

LATEST2026-10-29 broker isolation: document-broker-v1 uses EXACT failing2E4264F2
DLL (not rebuild), coreA85. DocumentBrokerChecks.cs Task MTA client/UI pumping,
five-second bound inside unchanged30s focused test, flushed per-call traces.
DocumentRange.GetText succeeds; GetVisibleRanges returns5; FIRST array range
GetText crashes0xC0000005. Narrowed to array-returned range consumption, not
generic document text. Receipt56D8BF0DAF04FA03B2391A91B8F36B0E912504625272EED560880C9079902671.
Compile1739ms/120s exit0,run13763ms/30s exit3221225477,Job0/drain/no timeout,
max143ms cleanup;inputs unchanged;owner28472 retired0. No fix claimed.
Hypothesis-only source hook adds ComDefaultInterface(typeof(ITextRangeProvider))
to existing ComVisible/ClassInterface.None/MarshalByRefObject Uq. UNBUILT: broker
v2 -BuildBrokerView refused on600MiB reserve BEFORE compile/output;owner87885
retired0,no leases. C:617041920 (~588MiB). Next same freshv2 after reserve recovery.
Command Test-PrintFriendly -KeepForegroundHost -Version2 -Live -Html -Images
-CacheView -DocSelection -DocGestures -DocCopy -DocUia -DocBroker -BuildBrokerView
in retained NoExit TTY. Without BuildBrokerView broker mode pins old2E DLL;
with it rebuilds source, so distinguish candidate hash/evidence. Do not suppress
crash or disable returned-range checks. If hypothesis fails, inspect COM array
marshalling/default-interface/lifetime before new attempts. No sessions running.

LATEST2026-10-29: full-native contract migration exposed REAL external UIA crash.
Production view remains2E4264F2/coreA85. Test-NativeSelectionFull DocumentSelection
now compiles frozen Smoke.cs against exact runtime with120s compiler/300s run.
Project-DocumentSelectionNativeSmoke.ps1 unique/inverse-checked edits: None->Single,
hidden action absent, and visible ranges must be nonempty<=2048, each text and
geometry nonempty. All unrelated assertions preserved;2 negative controls.
Nativev3 with first2 edits: compile23314ms exit0,run68265ms exit1 at obsolete
visible-ranges==0 assertion. ReceiptDB1A132493DC308EAFD3F053327B9B5D0043019E7FE423799C19270EE79C794C;
max123ms cleanup;owner85243 retired0. Inputs/fixtures/runtime bytes unchanged.
Nativev4 includes third stronger visible-range assertion: compile22246ms exit0,
run76151ms exit3221225477 (AccessViolation) in MS.Internal.Automation.UiaCoreApi.
RawTextRange_GetText -> TextPatternRange.GetText on client thread. No timeout,
Job0/output drained,max124ms cleanup,unchanged inputs/179 fixtures;owner13102
clear/retired0. ReceiptBFC634390B04CB57B551E023CB83FDA989A5D2477D4D2F368BCC40873B79463A.
This is NOT just stale test expectation, NOT a native pass, no unchanged rerun.
NEXT isolate external UIA GetVisibleRanges/returned range read with bounded client
trace checkpoints and same pinned candidate. Preserve crashing assertion and
native evidence; do not suppress AccessViolation or disable external UIA to pass.
Investigate root cause before host adoption. Focused282 remains narrower evidence;
old331 actual-print result belongs to oldCD8 view, not2E. No sessions running.

LATEST2026-10-29 native-suite work supersedes below candidate status:
document-uia-v5 view2E4264F2008C451D0F8FB42DE3F5229AF9D9817142E4ED02F785905820687CE7
passes282 focused (86+196),receipt23C13323A10803C7879A487A9FE640E6BEFFCB38E682026E3CB112C4023E4DE7.
Ut.DocumentModel explicitly distinguishes private legacy full-logical geometry
ranges from public rendered-selection ranges; new snapshots markedtrue. Strict
public stale-text validation retained; legacy geometry follows original model;
legacy ranges cannot Select. Source inverse verified. Build19259ms/120s,
compile925ms/120s,run9561ms/30s; clean exits/Job0/drain,max151ms cleanup;
owner42432 retired0. Prior331 actual-print result applies to OLD viewCD8 only.
Full-native attempt1 accidentally staged oldcore52CE instead ofA85 and failed
legacy private point-range stale-model check; receipt3D6D3FB69DD61A78B98B8C4029F686A32552767C7EF6918449051566D72A4766,
run57176ms/300s exit1/no timeout/Job0/drain,max108ms,owner55122 retired0.
Test-NativeSelectionFull -DocumentSelection now pins focusedv5/newview and A85 core.
Attempt2 correct core/view passed prior RTL failure, then failed obsolete UIA
assertion expectedNone/gotSingle. No full-suite pass. Run70370ms/300s exit1,
no timeout/Job0/drain,max140ms; unchanged inputs/runtime bytes/179 fixtures,
owner58570 retired0;receipt1918BDCE3DFFCC5907265D32A3E15F219809E007ABF34218BDC21540994047C7.
NEXT migrate frozen native Smoke assertions explicitly to new public UIA selection
contract, inverse-check source projection and compile against exact A85/newview.
Keep all unrelated RTL/layout/rendering/privacy/assertions and300s deadline.
Old Smoke line2178 expectsNone; line2079 fullText.Contains("Hidden action") must
be reviewed against rendered-model hidden-text privacy, not blindly retained or
deleted. Tests.rsp+Smoke.cs live in nested-overlap-regression-v1; references pinned
by its receipt. Need fresh Version3, not unchanged rerun. All6 gates OPEN.
No pending sessions. Full scripting remains after native contract migration.

LATEST2026-10-29: fixed-view actual print projectv4 AND combined livev5 pass.
ViewCD8DE6B65CA7E86E80E7ED0E5A523F595FCF9D17E145FD36C70BE9A1225A7676;
printCFDE51680B60DF4E18B8AA08D283803990BE399602B6E1ACDB6050FE0DE7893E.
Project receipt5E44211C963C6263F485FEE2ECEE6D9C4FC848DBCF564901938C4C8577EE8324:
build2714ms/120s,live21195ms/30s,range compiler2400ms/120s,range run5008ms/30s;
all exit0/Job0/drain/no timeout/kill,max199ms cleanup,inputs unchanged;
owner69689 clear/retired0. Full matched PRINT suites pass, not engine/script gates.
Combined selection-image-live-v5 receipt9931D46201AC69D26B24092F230477B4A595544E66F5C4C1E4710243F9036073:
331 overlapping checks (86+245), actual libraries not source HTML harness.
Ds.Cp/Ht selection HTML, empty/whitespace/full-page guards, native/password refusal,
retained native buffer privacy, detached HTML after navigation all pass, with
decoder/image/WPF/UIA/keyboard suite. Compile1657ms/120s,run13745ms/30s;
exit0/Job0/drain/no timeout/kill,max258ms cleanup,unchanged inputs;owner71527
clear/retired0. Decoder ACL observationA6793F0E55B2918CE7E15B033917E9B0A7119D0DC875FA85AF575DC00F0D43FB
classifies fixed read/execute+synchronize SID grant,3 negative controls; NOT
security/profile-deletion qualification. C:697147392 bytes after tests.
NEXT full changed-view scripting/security/lifecycle and actual Firespider friendly
command integration preserving seven options. External UIA event/broker/actual
clipboard and physical print still unverified. All6 release gates OPEN.
Older pending/failure statements below are historical; failures remain retained.
No pending sessions. This checkpoint supersedes earlier source-only status.

Latest cleanup recovered build headroom: six original old ZIPs218/219/220/221/
222/224 losslessly consolidated in solid-build-archives-v3, all restored/hash-
verified before removal;4 recovery negatives pass, process67144 exited0.
Container7CAE32B9/manifestA5E4E796/receipt34193308 full hashes STORAGE_CLEANUP.md.
Helper supports all three stores. Net logical archive reduction~245MiB; current
C:710266880 bytes (>600MiB initial reserve), source/candidates/failures retained.
Next fresh fixed-view projectv4 now safe to attempt; no gate waiver.

Latest2026-10-29 source-only work: C:456896512 bytes remains below reserve.
Ds.Cp now rejects whitespace-only selections consistently with Ht; Ds.Ht(Po)
is a short selection-only HTML export API and rejects full-page/null options.
Added direct tests for these guards and detached output equality after navigation.
NOT compiled/run; projectv3 older partial result does not qualify new Ds source.
Fresh projectv4 then exact livev5 still next after reserve recovery. Source-only
v22 supplement excludes old binary/evidence trees to avoid repeated large ZIPs;
v20/v21 retain prior attempts. No running processes or background monitoring.

Latest user-requested cleanup2026-10-29: explicitly verified/removed old September
Verify201/202/203 duplicate extracted folders,1089 recoverable files; original
ZIPs+metadata retained and rehashed after removal. Immediate free deltas23445504
bytes (~22MiB); final C:457289728 bytes (~436MiB), still below600MiB build reserve.
STORAGE_CLEANUP.md has exact paths/hashes. No source/current/failure/chat/Windows
deletion; no running sessions. Reclaimer adds only explicit historical mappings.
Engine next remains fresh fixed-view projectv4 then exact livev5 after reserve
recovery. Existing projectv3 partial result must not become a full proof.

2026-10-29 latest attempt: C: recovered818909184 bytes, so fixed-view projectv3
was launched normally. Actual fixed-view print build and live capture passed;
reserve then fell below512MiB BEFORE range compiler, overall resultfalse. Do NOT
call this a full project pass. PrintF8CA60F583FF919F5455B672E7C35EA27D7B46C9BE4D6EA530EDCA12AE628643;
receiptC5A963B95AF31FC0BDCFC4716B601FAB47EEA47FDAE4A69DF38CF2958A4A018E.
Build11156ms/120s,live21899ms/30s; exits0/Job0/drain/no timeout,max221ms
cleanup. Inputs unchanged; owner10013 clear/retired0. C:364285952 afterward.
Next fresh project Version4 once reserves stable, then exact pair liveVersion5.
Do not substitute partially passingv3 for full project proof or loosen runner gate.
Read-only storage investigation: no hiberfil; pagefile700407808 unchanged stamp;
Codex thread_history DB658018304, largest compressed old session1009172002,
current growing session163326878 bytes. Nothing touched; these observations do
not attribute the~455MB volume decline. Asked for drive>=2GB or further project
cleanup selection. No background work/running sessions; no new binary adoption.

2026-10-29 current continuation: project Version3 refused before evidence/compiler
creation on600MiB reserve; C:271343616 bytes free. Owner78671 confirmed no leases/
unverified child and exited0. No new runtime result; do not retry unchanged without
reserve recovery. Read-only temp scan found one134649192-byte old temp, untouched.
Old project binary scan mostly latest suite evidence; no deletions/compression.
Added UNRUN test assertions: native FIELD buffer retained after navigation must
not appear in Sx; Ds must reject password and native plain-field contexts.
These edits postdate281-checkv4; its result does not qualify the added tests.
Next remains fixed-view project Version3, rebind live exact pair, Version5 with
new privacy regressions when storage reserve is restored. No sessions running.

2026-10-29 IMPORTANT latest fix: document-copy Ce(cp) made legacy Sx read retained
ed.SelectedText with no native edit. Actual document-print tests exposed this;
selection-image-live-v3/v4 fail native separation (v4 null/empty assertion change
did not fix it). Both failures retained; owners95680/20604 retired0. Not null bug.
Build-PrintImageView copy projection now guards Sx with edit/heldEdit presence,
inverse verified. document-uia-v4 passes281 focused (86+195), view
CD8DE6B65CA7E86E80E7ED0E5A523F595FCF9D17E145FD36C70BE9A1225A7676;
receipt07EDA947DC3E181AD783AE1BF4DD6F2966EDFD647D63E68CAD0A89984D49DA06.
Build22263ms/120s, compiler2519ms/120s, run13202ms/30s; exits0/Job0/drain,
no timeout/kill, max143ms cleanup, unchanged inputs; owner98043 clear/retired0.
Ds.Cp(view,geometry) added as explicit optional document-selection print capture,
Text+Capture frozen with generation/selection recheck. EnableDocumentSelection
project flag includes Ds.cs only for supporting view; old APIs preserved.
selection-print-project-v2 broad legacy print suites passed with OLD view620395E1,
printC0F6C2FEF2F1D9882DBA91490A676C7B6CACBF61633873B3E625255D8224D2A6;
receipt782DE1EB02E7EEA51CBD2CB2C8DC3C9990A57DDA9B701538544D3190977A05C7.
Owner26800 retired0. Ds behavior NOT qualified due to above failure.
NEXT run Test-PrintProject -SelectionView Version3 (now pins fixedv4), all prior
layout flags, then rebind Test-PrintFriendly SelectionLibrary project/proof/print/
view hashes to that exact candidate and run Version5 full combined document-print
checks. Current SelectionLibrary pins OLD buggyv3; do not claim it tests fixedv4.
C: last~970MiB, recheck reserves; no pending sessions. No host adoption/gate closure.

Latest matched actual-library live run: selection-image-live-v2 passes318
overlapping checks (86+232) against view620395E1/print9765FE7A exact pair.
Real decoder/embedded recapture/WPF color/grayscale/XPS image resources plus all
document selection/gesture/copy guards/UIA/keyboard checks run together; NOT
Ht/Pi source harness. Receipt2FA8B1FC0F54F44594E47BA4F1C2E0B31ACC10261581F990FF9C48F65CE52D56.
Compile1621ms/120s, run13262ms/30s; exits0/Job0/drain/no timeout/kill,
max212ms cleanup, unchanged inputs, owner44056 clear/retired0. V1 prior222
matched image-only checks passed, owner97588 retired0; not added to318.
ACL observationF0D54A79F05D63EAC75D399387566CD23193C42EA87EF1B955C496DEFB7E367E:
only fixed decoder SID read/execute+synchronize grant,3 negative controls pass;
NOT security qualification/profile deletion proof. WPF color PNG inspected.
Next full scripting/security/lifecycle qualification and document-selection print
capture/host workflow; external UIA events remain unverified. No pending sessions.

Latest broader regression: selection-print-project-v1 passes against exact
view620395E1 (full hash below). Rebuilt print library
9765FE7AED959EE9189DC63608EA9B26357847AF2068932D30C3FA0757D84594;
receipt1EC85E64B255C61E0501626DC7432BA0B5B97880F98F6C422D2A059623B8D5F3.
Live styled capture/foundation footer and range37/device16/dialog27/ticket22
suite footers pass. This is matched print regression, NOT full engine/script/
security/lifecycle/host qualification. Build2608ms/120s, live17876ms/30s,
range compiler2182ms/120s, range run5101ms/30s. All exits0/Job0/drained,
no timeout/kill, max245ms cleanup, inputs unchanged; owner13523 retired0.
Test-PrintProject.ps1 -SelectionView now explicitly pinsv3 proof and DLL;
requires all LayoutImages/prior modes. No browser adoption or gate closure.
Next bind latest actual print library to image/selection live checks and continue
full changed-view script/security/lifecycle qualification before host migration.

Latest engine candidate: document-uia-v3,280 overlapping checks (86+194).
DocumentSelection.cs adds Left/Right collapse/move, Shift extension, Home/End
rendered-line and Ctrl+Home/End document navigation. Text-element boundaries
preserved; held native editor guard strengthened. Word/vertical movement and
automatic caret scrolling NOT implemented. Tests use reflected key dispatch,
not physical keyboard input. View620395E1C3EC73DF2903531A52943A0900C12A9AED03E0244F0422FC9080CF81;
receipt3B2C83EA0B8E372C8F0C0A2EB9592FD7D80804F2942D08A85CBA952756460FDA.
Build20899ms/120s, compiler950ms/120s, run11577ms/30s; exits0/Job0/drain,
no timeout/kill, max128ms cleanup, unchanged inputs; owner2345 clear/retired0.
Next external accessibility events/full changed-view suites and print/host adoption.
No final gate closed. No running sessions.

Latest verification: document-uia-v2 passes263 overlapping checks (86+177),
including25 direct-provider checks. Added measured RangeFromPoint hit, offscreen
collapsed range/nonfinite rejection, Unicode character movement/endpoint roundtrip.
View CF347171DE5AEB705511B761993DADEACB876F87544527415A2DB8ECDC2D88DE;
receipt F38D0504F9C838D7516536CB4E056A6984E8DA68C4B6450F79D4EA968CE1C6D8.
Build20307ms/120s, compiler1257ms/120s, test10110ms/30s; all exits0/Job0/drain,
no timeout, max164ms cleanup, inputs unchanged, owner82848 clear/retired0.
No engine source change sincev1; new build hash differs because stage paths differ.
External UIA event/broker and full changed-view regressions remain next; no host
adoption/gate closure. Older candidates below retain their own evidence boundary.

Latest engine milestone: document-uia-v1 passes254 overlapping focused checks
(86 snapshot+168 live,16 new direct-provider checks). View SHA256
C16C984D471BB347975298E1372FD0F00A937617B9E41D187487FEBB01DDF6CC;
receipt B97AC9FB633C295038169E8C1C2967003AE1C36CDBDFBEBFA721603ED69F0ACE.
UIA Single/GetSelection/Select now use rendered document text; measured rectangles,
Unicode character boundaries, stale-range rejection verified. Source projection
inverse verified. Build18644ms/120s, compiler979ms/120s, test9931ms/30s;
all exit0/Job0/drained/no timeout/kill, max148ms cleanup; inputs unchanged.
Owner60794 guarded clear/retired0. No running sessions. Event implementation is
compiled but external event delivery/screen-reader broker remains UNTESTED.
Next: direct point/character movement/layout invalidation checks and external UIA
events, then matched full changed-view regression before host adoption. No EXE
or browser adoption. DOCUMENT_SELECTION.md supersedes earlier UIA-unsupported notes.

2026-10-28 latest user request was project storage cleanup. Completed verified
NTFS compression of147 large older dist/.runtime157-184 staging files using
Compress-HistoricalEvidence.ps1 -Scope LegacyLarge -OlderRuntimeStages
-Attempt cleanup-20261028. Zero deletions; stored reduction29516636 bytes;
C: free1865998336 bytes afterward. Receipt/details: STORAGE_CLEANUP.md.
HistoricalText attempt found no remaining eligible files and changed nothing.
No running cleanup sessions. UIA work subsequently built/tested as above. Prior
238 checks below apply ONLY to the earlier copy candidate.

Latest viewA9A0089631F10BC2B9491DA414AF428FD64D6234B1DF31D65F12C97DC0698E60
adds Ce/Ec copy hooks with exact source inverse. Valid document cp availability,
collapsed command no-write, native/password/menu-retained editor separation pass.
document-copy-v3 receipt
FEE9DC38150E86F97217AC7393CBFD9DC1AD6B3FD0CB4BC482DCA388A344A4BA:
238 overlapping checks (86+152,14 copy/writer/menu). Build19462ms/120s,
compiler1177ms/120s/run8811ms/30s; exits0/Job0/drain/no kill/timeout,
max129ms retirement, inputs unchanged; TTY61035 guarded clear/retired0.
V2 source-anchor refusal (CRLF vs LF) before compile preserved; owner retired0.
Sdc/Ctrl+C/Ec native writer implemented but not validated with Windows clipboard.
Private station attempts: create-only autogenerated name exists, then fresh name
access denied. Both0 clipboard calls, tests exit1/Job0/drained/no leases; guarded
owners retired0. No UAC/elevation/reuse/user clipboard access. PRIVATE_CLIPBOARD.md
has proofs/limits. This component test is unavailable, not a whole-goal impasse.
Continue UIA and full changed-view regression; actual clipboard requires available
approved isolation strategy and then-current candidate binding. Reentry rejection
does not undo a native write already performed. No pending exec sessions/browser
adoption. DOCUMENT_SELECTION.md records current source/writer boundaries.

## Measured Selection Evidence

Current working addition Lea.Win/DocumentSelection.cs plus PrintImages.cs is
compiled alongside six frozen mature sources. Build-PrintImageView.ps1 generates
four gesture plus two optional copy-menu inverse-verified V.cs hooks; originals untouched.
Explicit -DocSelection -DocGestures required together.

Basic APIs Dtx/Sel/Sds, mouse drag/highlight, Ctrl+A/Escape, link-drag suppression
and native-field separation are implemented. Position/highlight now use renderer
font prefix measurements and text-element boundaries instead of equal widths.
Latest candidate/test metadata is checkpointed; use
continuity-evidence/document-selection-gestures-v8/result.json as authoritative.
View SHA256 213D347E0544E8ADF6C27BFA1A6EB7F4FB95051505E3583C81CD7E6E63175258.
Receipt SHA256 4ECA1DEB9AAFEFB43AF271F02D0F98312BCAFD3BAA6742A4BD1BE25FD517BE4F.
224 overlapping checks (86 snapshot+138 live); build16724ms/120s,
compiler889ms/120s, run9540ms/30s; original exits0/Job0/drained/no kill/timeout,
max114ms retirement, inputs unchanged. Original TTY46134 guarded clear/retired0.
Variable-font partial drag selects iiii; prefix/highlight right edge48 matches.
PNG inspected. V4-v7 failures retained: shorthand fixture/default size, test
point-vs-pixel mismatch, and broad blue mask counting ClearType glyph fringes.
V8 checks computed longhand size and actual highlight-background coverage/edge.
Font shorthand behavior remains an unresolved compatibility observation.

Verification limit: real view APIs/native mouse messages, reflected key/error
fixtures; Ht/Pi are a SOURCE harness on old453 snapshot DLL. NOT whole-new-view
395/full scripting/security/lifecycle qualification or physical-input campaign.
UIA still reports selection unsupported; clipboard untouched. No browser adoption.
Next: complex-script/clipping/wrapping accuracy and performance, clipboard copying,
UIA selection/events, matched full changed-view regressions, then print/host route.
Details: LeaForge/DOCUMENT_SELECTION.md. No pending exec sessions from V8.

## Print And Browser Boundaries

Latest separately qualified print DLL750E606AD95F3AF0856BB8C73749A0F92F1A8D63ED77438325B87907749EAFCF
uses A921DDF9 cache-only view, NOT the latest selection view.
Own395 regression: print-image-layout-project-v1, receipt E1710956.
Own222 focused real-decoder/WPF checks: print-image-layout-live-v2, receipt47844E96.
Full hashes, source projections, raw ACL observations and all limits are in
LeaForge/PRINT_FRIENDLY.md. Pm.Ci/Pw.Di image-aware paths, Mi raw BGRA source,
Pi bounded image values, Ci cache bridge and Ht inert HTML copy are implemented.
Image-only/color/grayscale/XPS image-resource checks pass; full CSS/page-break
fidelity, independent saved-XPS image raster parity and physical output do not.
Old F14 standalone30s timeout remains retained/unexplained, not erased by newer
candidate success. No deadline changes or unchanged reruns to obtain green.

Firespider q945/q946 still legacy WebBrowser printer-friendly workflow; must retain
HTML-copy tab, seven options, selection, orientation/margins/scale/grayscale/
header/footer. No new browser EXE/package from current selection/image work.
LeaForge/CURRENT_HOST.md, PRINTING.md and PRINT_FRIENDLY.md locate host evidence.
Native package-transfer experiment is NOT adopted: PRINT_PACKAGE_TARGET.md.
Earlier virtual job changed default printer under automatic default management;
HP restored via direct API, preference unchanged. Do not submit more virtual jobs
on that profile or alter preference to clear a test. Raw decoder ACL grants are
retained and narrowly read-only classified, never a general security waiver.

## Ownership, Storage And Preservation

Use retained nonadmin NoExit TTY owners and pinned OwnedTestProcess/
Invoke-OwnedTestRun helpers. Original handles/atomic Jobs/explicit inheritance,
deadline120s compiler/30s focused test/shared5s cleanup remain unchanged.
No PID reopen/kill substitute. Retire owner only after zero global process leases
and no unverified child. Missing observation handle alone is not terminal.
Do not end with needed exec sessions still running. See OWNED_TEST_LAUNCH.md.
Initial build reserve600MiB, owned-run reserve512MiB: recheck explicit numeric free
bytes, not mixed PowerShell formatted tables. No background monitoring.

User authorized verified archival/removal of old project duplicates. Preserve
source/current candidates/failures. Older ZIPs210-217 are losslessly consolidated
in two7z stores with full restore/hash checks; 22-package/26814-file audit retained.
LeaForge/STORAGE_CLEANUP.md and Get-ConsolidatedBuildArchive.ps1 locate recovery.
D: is RECOVERY, not build/backup space. Disk growth attribution remains unresolved.
Local verified ZIP supplements are NOT complete SDKs or off-device backups.
Latest source/evidence supplement: print-data-source-20261028-v30.zip,109 files/2329883
source bytes,811453 ZIP bytes, verified SHA256
3659C98A302BC58E162938FA74CBD7B41609115F9D9DDC981C99F3BE559A1582.
Includes UIA source/candidate/tests and prior helper sources; retained v14 contains
menu/private-clipboard attempt evidence. Supplements are not complete SDK backups.
V11 retains V4-V8 failures/measurements; earlier checkpoints remain retained.
This index postdates archived documents. Earlier checkpoints remain retained.
