# Document Selection

2026-10-29 pure-native EDIT baseline inconclusive:pre-documentinvalidargument/
nointerface,not prior AV. No legacy gate waiver. LEGACY_UIA_BASELINE.md has all
receipts and cleanup. Next CSS font-shorthand work retains this unresolved issue.

2026-10-29 standalone native SDK client also reproduces visible-readAV,no CLR or
Lea modules in client. No production change/gate closure;next pure-native EDIT
provider control. LEGACY_UIA_BASELINE.md has source/receipt/ownership limits.

2026-10-29 original native VARIANT-buffer conversion still crashes. Parent probe
label and aggregate deadline pass decision corrected with focused validation;
no production change/gate closure. Next standalone native client comparison.
LEGACY_UIA_BASELINE.md records v2 timing overrun without treating it as clean.

2026-10-29 runtime accessibility inventory and exact-framework metadata captured;
all five Windows components have valid signatures. WPF legacy read still fails.
No production fix/gate closure. Next raw original VARIANT-pointer conversion
comparison;LEGACY_UIA_BASELINE.md documents ownership and audit scope.

2026-10-29 cold managed queries and standard WPF TextBox controls(x64/x86 client)
reproduce visible-read crash. Not unique to Lea;legacy gate still open. Next
read-only loaded accessibility-component audit;LEGACY_UIA_BASELINE.md has scope.

2026-10-29 actual native typed SAFEARRAY comparison still reproduces legacy
visible-read crash. Annotation alone emitted IUnknown; manual array emits text
range IID,confirmed at runtime. No production change;next cold legacy ordering.
See LEGACY_UIA_BASELINE.md for results,ownership and scope limits.

Latest2026-10-29 exact2225/coreA85 provider also tested across separate processes:
modern COM document/visible text/geometry/selectedAlpha succeed;stock managed
visible-array text still crashes with no Lea assembly in client/custom C probe.
No production change;next typed SAFEARRAY/interface identity comparison.
[Exact comparison evidence](LEGACY_UIA_BASELINE.md).

Latest2026-10-29 independent provider/client now run in genuinely different
processes under two retained atomic-Job owners. Modern COM/document reads pass,
stock managed visible text still crashes;both jobs retire cleanly in actualv2.
Harness-only positive control passes separately,not UIA qualification. Next
exact2225 Lea server comparison. [Details](LEGACY_UIA_BASELINE.md).

Latest2026-10-29 independent query-only provider,with runtime no-Lea assembly
audit and custom C probe bypassed,reproduces stock managed visible-range crash;
modern COM and document reads pass. Current probes are in-process UI/MTA,NOT
separate-process clients. Next cross-process baseline before attributing fault.
Details:[Independent Legacy UIA Baseline](LEGACY_UIA_BASELINE.md). No gate closure.

## Legacy Native Handle Boundary

### Baseline And COM Lease Comparison

2026-10-29 broker-v9 adds a standard multiline Windows TextBox baseline with
constant Alpha text. It reaches TextPattern but visible-range read throws a
managed WindowsEditBoxRange endpoint-validation error before Lea is tested.
Receipt7257A15C49F4E512D55E5D7FEAF86C7075163AEE0AAC844585AD5352BB368FA4:
build18339ms/compile1268ms/run6664ms,exit1,no timeout/kill,Job0/drain,max132ms.
This is failed/inconclusive baseline evidence,not a passing reference control.

broker-v10 makes the text box read-only and holds a balanced explicit COM MTA
lease around each client task,following Microsoft's [UIA threading contract](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-threading).
CoInitializeEx returns1 (already initialized),balanced CoUninitialize on return.
Only the baseline's managed InvalidOperationException is logged as FAILED/zero
checks so the original Lea assertions still execute;no engine failure is caught
or treated as pass. Lea still access-violates at legacy native range read.
ReceiptF7E036B26382CAAC140E2C5F5D56A854D45E7AE7F84C6B8BA88CC0EADD73D662:
build17444ms/compile1159ms/run15507ms,exit0xC0000005,no timeout/kill,Job0/drain,
max138ms. Explicit COM initialization is not the fix in this campaign.

broker-v11 additionally calls native TextPattern_get_DocumentRange and the SAME
TextRange_GetText before the visible-array path. Windows Edit document reads20
characters,visible range still gives validation error. Lea document reads75
characters;its converted visible-array handle still access-violates. This narrows
the distinction to the array-returned legacy range path without proving which
provider/client/core component is at fault.
Receipt5AE31DE294147D3762131450B1AAE09F6366CE315CE9E0CC79BB34857CA397DC:
build18941ms/compile1014ms/run14208ms,exit0xC0000005,no timeout/kill,Job0/drain,
max106ms. All runs' input bytes unchanged,original owners guarded retired0.

### Strong Array Reference Probe

Explicit -RangeLeaseProbe retains the most recently returned visible-range array
on Ur via a UI-thread helper. Original provider methods/privacy/range assertions
and default configuration remain unchanged. Counted reversible V.cs hooks add
only that reference. document-range-lease-probe-v1 STILL access-violates after
document read succeeds;do not adopt it as a production lifetime fix.
ReceiptCB0A8459B126F849C65B420096798638936ED08CA3E5C13ECF54A6A48A6CD571;
view1A3B50CEB960C6C12C28A358BB607EE956DA1568AF1944D4564E9D4B1FE2A79E/coreA85.
Build17951ms/compile1359ms/run14772ms,exit0xC0000005,no parent timeout,
cleanup killRequestedTRUE,Job0/drain,max126ms,inputs unchanged,owner retired0.
Latest-array managed rooting alone is not sufficient;this does not rule out all
COM ownership/lifetime issues. Next minimal independent managed provider and
array-handle ABI comparison;legacy/native final gate stays open. All failed
sources/binaries/logs retained;exact2225 script qualification remains separate.

2026-10-29 follow-up: explicit diagnostic -ProviderThreadProbe removes only the
UseComThreading bit from both providers,using count2/exact-inverse V.cs hooks.
Default configuration is unchanged. document-threading-probe-v1 still fails at
native TextRange_GetText after provider ABI/MTA/modern COM success;removing that
flag alone does NOT fix the legacy crash. Never adopt this probe as a production
workaround or skip legacy range assertions.
Receipt63A4BE479521680945058307DB5A7CE098560C18BC9BA94BE532B0AD704251AB;
viewFB1CD678C11D796775B8FDE1AFAA47E67CBB10B781D778E38A3DEC4499070AC8/coreA85.
Build17976ms/compile1168ms/run14505ms;run exit0xC0000005,no parent timeout/kill,
Job0/drain,max137ms cleanup,inputs unchanged,owner guarded retired0. Original
budgets and privacy/range assertions retained. Next inspect native range-handle
ownership/lifetime and legacy ABI rather than repeat this flag experiment.

2026-10-29: DocumentLegacyHandleChecks now probes the C API used beneath the
managed client before the existing managed range assertions. Its declarations
follow the conversion path in Microsoft's current [UiaCoreApi source](https://source.dot.net/UIAutomationClient/MS/Internal/Automation/UiaCoreApi.cs.html):
native TextPattern_GetVisibleRanges yields a VARIANT array;each element converts
through UiaHTextRangeFromVariant before a TextRange_GetText call. The current
reference is not proof that the installed legacy implementation is identical.

document-broker-v8 observes arrayVT_VARIANT12,elementVT_I8(20)/System.Int64,
successful nonnull handle conversion,then access violation inside native
TextRange_GetText. Direct provider ABI,UI->MTA range read and modern CUIAutomation
visible text/rectangles all pass immediately beforehand. This narrows failure
below the managed TextPatternRange wrapper,not to a proven root cause or OS bug.
Existing managed GetText/privacy/rectangle assertions remain,not bypassed;new
diagnostic fails before reaching them. No production provider/source change.

ReceiptC74FED88EB2ED5326F77F99E565CB962226EF3F7316188EE2CFAABF2E40F9B60;
view0C0587233AC7365E25A59006CD1D2F23BCFD7A4FA99D0F667BB9456B046776B5/coreA85.
All staged source bytes match shutdown-fixed2225 build;different binaries remain
distinct. Build17054ms/compile1246ms/run13080ms,original budgets. Run exit1,
native access violation plus existing five-second internal broker timeout;
no parent deadline timeout. Cleanup killRequestedTRUE,Job0/output drained,
max136ms retirement,inputs unchanged,original owner guarded clear/retired0.
Keep stdout/stderr/failed stage;no full native or legacy qualification.

Next controlled investigation: native-handle conversion/ownership and provider
COM threading. Microsoft's [ProviderOptions contract](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationcore/ne-uiautomationcore-provideroptions)
explains UseComThreading;an alternate flag experiment would be diagnostic only,
not a production fix or justification to remove threading/privacy checks.
Exact2225 full scripting and decoder qualification remain separate evidence.

2026-10-28: basic drag/highlight/Ctrl+A/Escape implemented; full selection unfinished.

Current menu candidateA9A0089631F10BC2B9491DA414AF428FD64D6234B1DF31D65F12C97DC0698E60
adds two inverse-verified Ce/Ec document-copy hooks in explicit -DocCopy mode.
DocumentCopyAvailable only enables cp for ready current document selection on
the UI thread with no active/menu-retained editor. Native editor/password guards
fall through unchanged; Sds now also invalidates menu-retained editor contexts.
Ce enablement, collapsed Ec no-write and native/password separation tested;
valid Ec->Sdc native clipboard dispatch is implemented but NOT executed here.

document-copy-v3 receiptFEE9DC38150E86F97217AC7393CBFD9DC1AD6B3FD0CB4BC482DCA388A344A4BA
passes238 focused overlapping checks (86+152), including14 copy/writer/menu checks.
Build19462ms/120s, compiler1177ms/120s/run8811ms/30s; original exit0/Job0/drain/
no kill/timeout, max129ms retirement, unchanged inputs; TTY61035 guarded retired0.
V2 refused before compilation because new multiline hook anchors assumed CRLF
while frozen V uses LF; fixed anchors without weakening uniqueness/inverse check,
refusal retained and original owner89578 guarded retired0. No user clipboard use.

Real clipboard test attempts refused at private-station setup, not adapter code;
PRIVATE_CLIPBOARD.md records exact failures,0 clipboard calls/no elevation/reuse.
Actual Sdc/Ctrl+C/Ec native adapter remains UNVERIFIED. UIA selection unsupported,
full changed-view395/native/script/security/lifecycle/browser gates unfinished.
Next UIA and matched whole-view regressions while clipboard isolation is unavailable.

Latest copy dispatch candidateE62D0E1B137DF7AF782CC655234999F13CEDE614B7B1B24DF152BB85CDE9571B
adds Sdc() and Ctrl+C document branch. Copies Unicode selected payload only;
blank/stale/native-field selection does not write. External/apartment clipboard
errors return0. After writer returns, exact selection/page/range/readiness/text
are rechecked; reentry changes return0. As with native CopyEdit, a native write
may already have happened before reentry is detected; no rollback promise.

document-copy-v1 receipt5D8CD2221F9273F3DDFC482F0A4C891744C155421B9C2A1306B0265B0D87F8FA
passes233 overlapping checks (86 snapshot+147 live;9 writer-boundary checks).
Build17427ms/120s, compiler1072ms/120s/run10299ms/30s; original exit0/Job0/drain/
no kill/timeout, max139ms retirement, unchanged inputs; TTY32953 guarded retired0.
Tests inject Action<string> to inspect payload/failures/selection and navigation
reentry. Windows clipboard adapter and Ctrl+C end-to-end NOT exercised; user's
clipboard untouched. No new whole-view395/scripting/security/lifecycle proof.
Existing Ce/Ec native-editor menus remain unchanged: document-copy menu routing
and UIA still unfinished. Next isolated Windows clipboard test, menu integration,
UIA, broader text/whole candidate gates. Post-run runner-only fix restores original
non-copy -Live output prefix; AST parsed, no unchanged gate rerun.

Latest measured candidate213D347E0544E8ADF6C27BFA1A6EB7F4FB95051505E3583C81CD7E6E63175258
passes224 focused checks, receipt
4ECA1DEB9AAFEFB43AF271F02D0F98312BCAFD3BAA6742A4BD1BE25FD517BE4F.
Font prefix widths now determine text-element hit boundaries and highlight edges;
logical endpoints also snap to the complete document text-element boundary.
The iiiiWWWW fixture has computed longhand size24; independent renderer-contract
font measurement gives28px narrow-prefix advance. Mouse selection returns iiii
and background highlight right48 matches20+28. Coverage and right-edge checks
exclude ClearType-colored glyph fringes; an over-wide proportional fill fails.
Partial-highlight PNG visually inspected. These checks cover this Arial/LTR case,
not general shaping/bidi/ligature/CSS-unit correctness.
Build16724ms/120s, compiler889ms/120s/run9540ms/30s; original exits0/Job0/drain/
no kill/timeout, max114ms retirement, inputs unchanged; TTY46134 guarded retired0.

V4-V7 failed evidence retained: fixture shorthand rendered default size; a test
font used mismatched units; then a broad blue-pixel predicate counted ClearType
fringes outside the correct highlight (visible in V7 PNG). V8 uses explicit
longhands/computed-size assertion and known background color/coverage, not a
weakened geometric tolerance. The shorthand observation is still an unresolved
compatibility issue. No unchanged rerun for green and no deadline changes.
Next complex-script/clipping/wrapping/spacing and selection-performance cases,
clipboard and UIA, then full changed-view/print/host qualification.
Older proportional-candidate notes below are historical verification boundaries.

Current gesture candidateCB78D33AD6DB69FE4775D194C7AEBDC9BF5C6EAF530A6D5BB16A388705708ABF
passes focused218 checks (86 snapshot+132 live,34 selection/gesture-specific).
Receipt77A557448B186968F32732401BC62597DE8F4679DBB6B09A46ACE72AA9D73706.
Build17400ms/120s, compiler1037ms/120s, behavior8010ms/30s; original exit0/
Job0/drain/no kill/timeout, max163ms retirement, inputs unchanged; TTY4885 guarded
clear/retired0. Prior gestures-v1 passes214, v2 passes217; retained. V2 adds link
drag/native-field checks, v3 fixes load-error Ctrl+A readiness with targeted test.
No failed gates retried unchanged. Four scoped generated hooks in mature V.cs
paint/move/click/key paths have exact source inverse; frozen originals untouched.
Current source requires explicit -DocSelection -DocGestures build together.

Mouse tests send native window messages for forward/reverse/full-segment/link
drags; assert selected text/capture retirement/no navigation. Paint pixels and
document-selection-highlight.png inspected. Ctrl+A/Escape handler tests call the
protected key entry via reflection, not hardware keyboard input. Reflection also
injects a temporary load error/busy state; all state restored. Native-field Sx
remains separate. Clipboard not accessed or changed. No production reflection.
That older candidate used proportional positioning; latest measured result above
supersedes it for the tested variable-width fixture. Bidi/general glyph accuracy
and broader selection/copy/UIA advertisement remain unqualified.
Prior395/full new-view scripting/security/lifecycle/host gates NOT run on this
view; print source harness remains separate. No browser EXE/package adoption.

Additive Lea.Win/DocumentSelection.cs builds with six unchanged mature view
sources and PrintImages.cs through explicit -CacheView -DocSelection mode.
Do not add prototype/root V.cs to this build or replace historical sources.

- Dtx(): bounded rendered-text selection model, at most32768 characters/2048
  existing automation text segments. Uses mapped rendered segments, not raw
  page.X substrings; hidden/password/native editable field text excluded.
- Sel(anchor,caret): forward/reverse/collapsed UTF-16 offsets in Dtx text. Invalid
  offsets rejected; endpoints snap backward to .NET4.8 StringInfo text-element
  boundaries. Surrogate pairs/combining marks tested, not full modern emoji/ZWJ
  grapheme certification. Does not change native field selection or clipboard.
- Sds(): selected document text; exact page identity and rendered text freshness
  checked. Busy/error/native editor/navigation invalidates state without revival.
  UI thread/existing handle required; wrong-thread/disposed use rejected.

Earlier API-only candidate view SHA256
F45CB9C0E5EA0B64ACEB03B1E2522B35AF4DA13585CE7EA46B04B0F56163C69E.
Focused document-selection-api-v1 receipt SHA256
F433FA748CAE37B767E8D7C5660904D9A7563906EF6C88BEF90B28A44CD1D9C2:
86 snapshot+121 live checks=207 overlapping checks,23 selection-specific.
Build18562ms/120s, test compiler1234ms/120s, behavior10180ms/30s; original
exit0/Job0/drained/no timeout/kill, max706ms retirement, inputs unchanged;
TTY84468 guarded clear/retired0. Tests use real rendered fixture and actual view
API, test reflection only sets busy guard. Ht/Pi remain source harness on old
453 snapshot DLL; this is NOT prior395 qualification of the changed view.

Earlier API-only checkpoint limits (superseded where newer evidence above applies):
richer keyboard commands, document-edit/layout invalidation under live
scripts, bidi/mixed-script/long document edge cases, print selection capture and
Firespider route. Existing UIA still explicitly reports selection unsupported.
Basic gesture/highlight support is verified above; full selection/browser adoption
is not claimed. Next matched full candidate regression and printer-friendly workflow.

## 2026-10-28 UIA Selection Provider

Current926 view now requalified for matched print: projectv5 passes7C696BB4,
printBFCE52A2/coreA85; actual livev6 passes336 overlapping1A901B8A,selection/
privacy/HTML/decoder/WPF together. All owned exits0/Job0/drain/no timeout/kill,
inputs unchanged;owners14712/92034 retired0. ACL1457F1F4 narrowly classified,
not security waiver. Full hashes/budgets PROJECT_CONTEXT.md. Legacy UIA failure
still open. Fresh .NET8033 script initializer view-only projection prepared with
inverse/parser/3 negatives,NO script runtime tested yet; next full helper projection.

Latest contiguous visible-range contract fixed: one range for contiguous visible
lines; clipped model intervals split spans; empty view returns degenerate range.
native-broker-v2 view926C1A28/coreA85 passes300 overlapping244799D5. Public
merging/native text/rectangles/Single selection pass; synthetic clipped middle/
fully clipped snapshot checks pass,live model unchanged (not OS occlusion proof).
Build18177ms/compile1754ms/run10745ms,all0/Job0/drain/no timeout/kill,max129ms,
owner69794 retired0. Legacy brokerv7 same engine source still crashes1A8AB52D;
cleanup killRequestedtrue,owner85077 retired0. This is contract improvement,NOT
legacy crash fix. Matched print/whole suites need new926 requalification.

Latest2026-10-29 separate native-client campaign passes303 overlapping checks
(86+217),document-native-broker-v1,receipt2F68437B. Exact2E/coreA85. Native
CUIAutomation reads5 ranges+finite positive rectangles and Single/Alpha selection;
UI->MTA marshalled GetText also passes. Compile1682ms/run11486ms,exit0/Job0/drain,
no timeout/kill,max225ms cleanup,inputs unchanged;owner70203 retired0.
Explicit -NativeBrokerOnly has legacyBrokerQualified=false; default broker crash
test NOT removed/redefined. v5 cross-thread success still crashes legacy(1C39E421),
v6 native5 reads succeed then legacy crashes(23EC17CB);v6 cleanup killRequestedtrue,
all jobs retired. Full hashes/budgets in PROJECT_CONTEXT.md. This narrows failure
to the legacy path but does not establish root cause or fix it. Full native gate
remains open. No engine DLL change/adoption; separate evidence is not gate closure.

Pending SOURCE-ONLY apartment diagnostic: DocumentRangeApartmentChecks marshals
the first visible range UI->MTA, calls native GetText, and owns COM initialization,
stream/interface/BSTR cleanup. It runs between existing ABI and legacy client
phases,with5s bound/UI pumping; failing external read retained. Uncompiled/unrun
because C:607420416 below600MiB reserve. Next brokerVersion5 WITHOUT rebuild uses
exact failing2E DLL. PROJECT_CONTEXT.md compact checkpoint preserves full old index
in history/PROJECT_CONTEXT_20261029_before_broker_apartment.md (hash4FBDD3C3).

Direct native GetText probe brokerv4 SUCCEEDS on queried array range: slot12,
validated interface metadata order,expected19 chars/privacy. Native array/QI
checks pass; later UIA client visible[0] read still access-violates.
Receipt2BA2EDDB,compile1430ms/run16257ms,Job0/drain/no timeout,owner63050 retired0.
This does not prove cross-thread COM lifetime correct or a Windows defect.
Next isolate COM cross-apartment marshalled call vs legacy client conversion;
retain original external crash. No production fix/full native pass claimed.

Executed ABI diagnostic brokerv3: exact2E provider array VT_UNKNOWN(13),rank1,
bounds0..4,first ITextRangeProvider QueryInterface S_OK/non-null. Native array/
interface checks pass,external visible[0].GetText still access-violates.
Receipt77065B21,compile1561ms/run15545ms,Job0/drain/no timeout,owner52045 retired0.
Installed .NET client IL includes UiaHTextRangeFromVariant conversion; pointer
truncation NOT established. Next direct native GetText diagnostic before client,
not yet added. Root cause unresolved/no fix/full pass. Source-only ABI notes below
are historical; no test/process remains running.

Pending native-array diagnostic: DocumentRangeAbiChecks probes exact provider
SAFEARRAY type/bounds and first element QueryInterface before UIA client reads.
All COM refs/owned array released; no raw GetText call; external failing check is
still present. SOURCE ONLY/uncompiled/unrun due reserve. Next brokerVersion3 pins
old2E view without BuildBrokerView. This is investigation, not a fix or gate pass.

ComDefaultInterface experiment FAILED: document-broker-v2 rebuiltDC2F3EFF still
crashes at visible[0].GetText after successful document text and5 visible ranges.
ReceiptBAF23E05; build20347ms/compile1013ms/run12673ms,Job0/drain/no timeout,
owner19640 retired0. Attribute hook removed from current source; exact failed
staged source retained. NOT a fix. Next array-return marshalling/lifetime boundary
investigation; no unchanged hypothesis rerun or checks disabled.

Broker-only reproduction document-broker-v1 exact2E view: document text succeeds,
visible array has5 ranges, first range GetText access-violates. Receipt56D8BF0D;
compile1739ms/run13763ms,exit0/0xC0000005,no timeout,Job0/drain,owner28472 retired0.
Small MTA client with flushed per-call traces/five-second bound added; does NOT
replace full gate. Explicit ComDefaultInterface Uq projection prepared as hypothesis,
unbuilt due600MiB reserve (C:~588MiB); brokerv2 output not created,owner87885 retired0.
Next rebuild with -DocBroker -BuildBrokerView Version2 after reserve. No fix/pass
claimed; complete code/evidence/command in PROJECT_CONTEXT.md.

IMPORTANT latest external broker failure: nativev4 source contract migration
(None->Single, hidden absent, visible ranges nonempty/bounded with text+geometry)
compiled but crashed0xC0000005 in Windows RawTextRange_GetText/TextPatternRange.GetText
on client thread. ReceiptBFC63439 full hash PROJECT_CONTEXT.md. Compile22246ms,
run76151ms/300s,Job0/drain/no timeout,unchanged inputs/fixtures,owner13102 retired0.
Priorv3 failed obsolete visible==0 assertion (DB1A1324),owner85243 retired0.
Frozen Smoke source inverse preserved,2 negative controls, all unrelated assertions
retained. This is real unresolved integration failure, not completed migration.
Next bounded external UIA range trace/isolation on exact2E4264F2/coreA85 candidate;
do not disable broker check or catch/suppress access violation to clear gate.

Latest native regression migration2026-10-29: view2E4264F2 separates Ut legacy
logical geometry from rendered public selection model via DocumentModel marker.
New public ranges keep strict stale-text validation; legacy private ranges retain
geometry semantics but cannot Select. Focused282 checks pass23C13323,owner42432
retired0; not requalified actual print331 on this changed view. Fullnativev1 wrong
core/private legacy snapshot failure retained. Correct-corev2 passes RTL section,
fails obsolete expectedNone vsSingle UIA assertion; receipt1918BDCE. Original
70370ms/300s exit1/no timeout/Job0/drain,unchanged inputs/fixtures,owner58570 retired0.
Next inverse-checked frozen Smoke contract migration (None->Single and hidden text
expectation review), preserving unrelated assertions/deadlines. Full native gate
NOT passed; no host adoption. PROJECT_CONTEXT.md locates exact source/evidence.

Latest2026-10-29: document-print capture now passes actual fixed-view integration.
selection-print-project-v4 full prior print suite passes, receipt5E44211C;
viewCD8DE6B6/printCFDE5168 (full hashes PROJECT_CONTEXT.md),owner69689 retired0.
selection-image-live-v5 passes331 overlapping (86+245),receipt9931D462:
Ds.Cp/Ht selected-only HTML, whitespace/empty/full-page guards, native/password
refusal, retained editor buffer privacy, detached output after navigation pass.
Real decoder/image/WPF/accessibility/keyboard checks run on same exact pair.
Compile1657ms/120s,run13745ms/30s,all exit0/Job0/drain/no timeout/kill,
max258ms cleanup,unchanged inputs;owner71527 retired0. ACLA6793F0E classified
only decoder read/execute+synchronize grant with3 negative controls; not security
qualification. Older failures remain but pending Ds source-only notes superseded.
Full engine/script/security/lifecycle/host/physical-print/external-UIA gates OPEN.

Pending source-only refinement2026-10-29: Ds.Cp rejects whitespace-only selection,
matching existing HTML writer; Ds.Ht(Po) permits selection-only output and rejects
null/full-page options. Tests add guarded output/detached navigation equality and
whitespace checks. UNBUILT/UNRUN due C:~436MiB below reserve. Rebuild projectv4,
then repin actual livev5; no older binary result qualifies this changed source.

Fixed-view projectv3 built printF8CA60F5 and live capture passed, then storage
reserve blocked range compiler before launch; overallfalse, receiptC5A963B9.
Original build/live exited0/Job0/drained/unchanged inputs; owner10013 retired0.
No full fixed-view project qualification and no new document-print behavior result.
Next fresh projectv4 when reserve stable, then repin actual-library livev5.

2026-10-29 pending test extension: explicit retained FIELD selection after
navigation Sx privacy check, Ds password/plain-field rejection added but UNRUN.
Fixed-view projectv3 refused before compile on600MiB reserve (C:~259MiB),
owner78671 retired0 without children. Prior281-checkv4 remains unchanged evidence,
not validation of these additions. Source supplementv21 readback verified.

2026-10-29 superseding defect/fix: Ce(cp) document hook enabled legacy Sx even
without a native editor, exposing retained editor selection. Combined Ds tests
selection-image-live-v3/v4 failed and are retained. Null/empty assertion adjustment
was insufficient; actual bug fixed by explicit edit/heldEdit Sx guard in mature
copy source projection. document-uia-v4 passes281 focused checks,viewCD8DE6B6,
receipt07EDA947 (full hashes PROJECT_CONTEXT.md). All exits0/Job0/drain/no timeout,
unchanged inputs; owner98043 retired0. Prior broader results do NOT qualifyv4.
Ds.Cp optional print bridge added with immutable text/snapshot and generation/
selection recheck, conditional EnableDocumentSelection project compile. Broad
legacy suite selection-print-project-v2 passed OLD view620395E1/printC0F6C2FE;
Ds end-to-end behavior remains unqualified. Next rebuild project against fixedv4,
rebind actual live runner and rerun document-print checks. No host adoption.

Actual-library integration selection-image-live-v2 passes318 overlapping checks
(86+232), not Ht/Pi source harness. Exact view620395E1/print9765FE7A pair from
matched project regression tested with real decoder, HTML embedded-image recapture,
WPF color/grayscale/XPS resources AND all gesture/copy/UIA/keyboard checks.
Receipt2FA8B1FC0F54F44594E47BA4F1C2E0B31ACC10261581F990FF9C48F65CE52D56.
Compile1621ms/120s, run13262ms/30s; exits0/Job0/drain/no timeout/kill,
max212ms cleanup; unchanged inputs, owner44056 retired0. V1 image-only222 checks
passed separately, owner97588 retired0; do not sum overlapping counts.
ACL observationF0D54A79 classifies one fixed decoder SID ReadAndExecute+Synchronize
ACE,3 negatives; not security qualification or profile deletion. Color WPF PNG
inspected. Full script/security/lifecycle and actual host/document-print-selection
workflow remain; external UIA events not verified.

Matched broader print regression selection-print-project-v1 passes withv3
view620395E1. Actual rebuilt print9765FE7AED959EE9189DC63608EA9B26357847AF2068932D30C3FA0757D84594;
receipt1EC85E64B255C61E0501626DC7432BA0B5B97880F98F6C422D2A059623B8D5F3.
Live capture/foundation plus range37/device16/dialog27/ticket22 footers verified.
All four owned runs exits0/Job0/drained/no timeout/kill; max245ms cleanup;
inputs unchanged, owner13523 clear/retired0. Test-PrintProject -SelectionView
requires full layout-image/prior modes and pins focusedv3 proof and actual DLL.
This is not scripting/security/lifecycle/host qualification, physical printing,
or new focused image/selection tests against the rebuilt actual print library.

Keyboard extension candidate document-uia-v3 passes280 overlapping checks
(86+194). Left/Right move or collapse, Shift extends/retracts whole emoji/accent
elements; Home/End uses rendered line and Ctrl+Home/End document endpoints.
Held native editors bypass document key handling. Reflected key dispatch only,
not physical input; word/vertical movement/automatic caret scrolling unfinished.
View620395E1C3EC73DF2903531A52943A0900C12A9AED03E0244F0422FC9080CF81;
receipt3B2C83EA0B8E372C8F0C0A2EB9592FD7D80804F2942D08A85CBA952756460FDA.
Build20899ms/120s, compile950ms/120s, run11577ms/30s; exit0/Job0/drain,
no timeout/kill, max128ms cleanup, unchanged inputs; owner2345 clear/retired0.
External events/broker and full changed-view/browser gates remain unqualified.

Extended document-uia-v2:263 overlapping checks (86+177),25 direct-provider.
Measured point resolves expected first character; far-offscreen point collapsed,
NaN rejected; emoji/combining character forward/backward/endpoint moves preserve
whole text elements. View CF347171DE5AEB705511B761993DADEACB876F87544527415A2DB8ECDC2D88DE;
receipt F38D0504F9C838D7516536CB4E056A6984E8DA68C4B6450F79D4EA968CE1C6D8.
Build20307ms/120s, compiler1257ms/120s, run10110ms/30s; all exits0/Job0/drain,
no timeout, max164ms cleanup, unchanged inputs; owner82848 clear/retired0.
Production sources unchanged fromv1; stage-specific build hash is separately bound.
External events/broker, script/layout invalidation and full candidate gates remain.

document-uia-v1 view C16C984D471BB347975298E1372FD0F00A937617B9E41D187487FEBB01DDF6CC
passes254 overlapping checks (86 snapshot+168 live;16 new direct-provider checks).
Receipt B97AC9FB633C295038169E8C1C2967003AE1C36CDBDFBEBFA721603ED69F0ACE.
SupportedTextSelection.Single, document FindText/Select/GetSelection, collapsed
caret, rendered model without hidden/native-field secrets, Unicode character
expansion, readonly attribute, visible ranges, measured rectangles and stale
navigation ranges pass. All source hooks inverse-verified against frozen mature
V.cs. DocumentSelectionAutomation.cs is explicitly included only in DocUia mode.
Build18644ms/120s, compiler979ms/120s, run9931ms/30s; exits0/Job0/drained,
no timeout/kill, max148ms cleanup, inputs unchanged; owner60794 retired0.
Selection-change event coalescing implemented/compiled but external delivery and
out-of-process UIA broker NOT tested. Point hit, Unicode movement, complex layout,
script changes and full changed-view regression need further checks. Ht/Pi source
harness still uses old453 print DLL; no full395/security/lifecycle/browser adoption.
Earlier statements that selection is unsupported describe older candidates only.
