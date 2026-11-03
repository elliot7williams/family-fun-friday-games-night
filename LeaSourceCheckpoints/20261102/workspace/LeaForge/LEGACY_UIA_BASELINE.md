# Independent Legacy UIA Baseline

## Pure-Native EDIT Control Is Inconclusive

2026-10-29 -NativeClient -NativeEdit builds same /MT native EXE as server/client.
Server uses standard EDIT window,nonce/PID/HWND JSON readiness and25ms stop poll,
original owned pair/budgets. No custom ITextProvider/CLR/Lea provider. Module
enumeration now requires ERROR_NO_MORE_FILES,not arbitrary enumeration failure.

Native-edit-v1 top-level EDIT returns80070057 before document text assertion;
receipt706432215DCCCB94EBFD99D9FBC38BC543942E367CF9686DBFA710BA36D36F5D,
pair1846ms/retire660ms. V2 uses normal parent STATIC/child EDIT layout,nonnull
native handle guards;returns80004002 before document read. Receipt
99894D394B97F0E2FCFCEBAB75CC3F02D5286E1062F710F4DE914382E5DF3859,
pair1302ms/retire203ms. V3 adds modern COM text-pattern preflight WITHOUT removing
legacy attempt:preflight returns80004002 before text assertions,so legacy is not
reached. Exact failing COM subcall not logged;do not claim successful document
or pattern acquisition. Receipt
6B926D4A9CC0F4CE8BCDCCE8752418C187826D44E5443B12548D6C6F92F6EFF7,
pair1288ms/retire191ms. This fixture has NOT qualified as a text-pattern baseline.

All3:inputs unchanged,server0/client7 (NOT AV),no timeout/no kill,Jobs0/drain,
owners7716/82060/66596 guarded clear/retired0. V2/v3 server also verifies no CLR/
Lea modules after queries. This does not reproduce or resolve prior crash,does
not prove an OS issue,and does not close any engine gate. Retain all evidence.
Native EDIT diagnostic remains optional;production engine/default tests unchanged.

Next development workstream:documented CSS font-shorthand compatibility defect
in COMPATIBILITY.md. Inspect mature source and add source-pinned failing tests
before a scoped projection fix. Legacy accessibility remains OPEN;do not transfer
modern passes or this unsuitable control into a legacy release pass.

## Standalone Native Client

2026-10-29 NativeLegacyUiaClient.cpp is standalone x64 /MT C++,using SDK C API
declarations/import library directly,not managed reflection/PInvoke. Original
HWND/provider PID correlation retained. Toolhelp module audit checks no CLR/
coreclr/mscoree or Lea.* modules before visible read. Client imports System32
UIAutomationCore;logs actual path. Native document read is exact19chars;native
visible array has oneVARIANT/VT_I8 element;original VARIANT passed to conversion.
Array,VARIANT,document/pattern/node handles retained until Read;RAII for ordinary
completion. Native visible GetText STILL exits0xC0000005. CLR client not necessary.

Test-MinimalTextProvider -CrossProcess -ColdLegacy -NativeClient builds fresh
independent SDK provider and native client. Build-NativeLegacyUiaClient.ps1 uses
retained56-file compiler-kit manifest with pinnedhash A7BF4C74...,existing SDK/
VC paths,120sownedcompile;pair retains30s+5s/originalHandles/atomicJobs.
No runtime fallback/no legacy gate waiver. SDK headers/libs outside compiler-kit
inventory are not a complete portable/reproducible dependency bundle.

minimal-native-legacy-client-v1 receipt
61AE1D963EEA401252B0D5D58ADF57B21BFCF43708395B8E78013B1EB1305F00.
Providercompile602ms/nativecompile7749ms,pair10001ms/retire99ms,server0/clientAV,
no timeout/no kill,both Jobs0/drain,inputs unchanged,owner16157 guarded clear/
retired0. Native module audittrue;managed-assembly auditfalse/not applicable.
No production engine change. Independent custom provider still managed;this
does NOT prove all-OS failure or eliminate provider/harness effects.

Next pure-native standard Win32 EDIT server/client control under same owner
pair/budgets to separate custom managed provider effects too. Also tighten module
enumeration to require ERROR_NO_MORE_FILES at end before relying on full coverage.
Keep existing legacy crash gate and original source/evidence.

## Original Native VARIANT Conversion

2026-10-29 -ColdLegacy -OriginalVariant passes original SafeArrayGetElement
VARIANT buffer directly to UiaHTextRangeFromVariant(IntPtr,out IntPtr),not managed
object remarshal. Array/buffer remain alive through Read;default probe unchanged.
Minimal-original-v1 document19chars passes,visibleVARIANT12/elementVT_I8(20),
conversion returnsnonnull,Read still AV. Receipt
003AC528AF26ACB65852C4FAD15D69D5582375AAD4FD88596977BF69D64AE809;
pair9622ms/retire193ms. Its customLegacyProbeBypassed field is WRONG(true),
because parent unconditionally labeled cross-process probes bypassed. Log proves
probe ran. Historical receipt retained,not silently corrected.

Corrected parent field to !$OriginalVariant. Freshv2 reportsfalse and same AV;
receipt5A60D34D30E5D567FDD439881357BDB701AF99F0D59E7CC7EB7E653FB4B92D52.
Pair12278ms/final retirement-reporting5004ms:OVER original5000ms aggregate budget,
NOT clean timing proof. Original process/Job exit and drain were observed and
owners guarded clear/retired0;do not infer budget pass from ownerAttentionfalse.
No original limits relaxed. Both client crashes/local5s watchdog retained.
Managed VARIANT round-trip is not necessary trigger. No production fix/gate pass.

Invoke-UiaProcessPair now records retirementDeadlineExceeded and final passed
also requires final aggregate <=5000ms,including reporting overhead measured
before return. No ownership guard removed. Fresh pair-control-v2 passes lifecycle
mechanics2653ms/retire702ms,both0/no kill/Job0/drain,owner55150 retired0.
Receipt0A6AD260C19B9092AC2662373467FA56EDA0D281D38BB74B193E9C9FEBD85D6D.
Not feature/legacy pass;server metadata query still runs in this control.
Test-UiaPairCompletion extracts actual source decision expressions and passes8
decision-only cases:0/4999/5000 accepted,5001/5004 rejected,wrong member count,
failed member and missing stop rejected. Not native cleanup-failure proof.

Next standalone native C++ legacy client against existing independent server,
keeping original owned pair,budgets and stock gate. Retained compiler-kit manifest
and Test-PrintPackageTransfer.ps1 locate native toolchain. This can isolate CLR
presence altogether;do not infer OS fault from existing PInvoke-only diagnostic.

## Loaded Component And Declaration Audit

2026-10-29 UiaComponentInventory captures actual loaded files immediately after
managed GetVisibleRanges and before failing GetText. WPFcoldv2 still crashes;
compile598/578ms,pair11448ms/retire522ms,server0/clientAV,no timeout/no kill,
Jobs0/drain,inputs unchanged,owner31156 guarded clear/retired0.
Receipt948865CE4ECB2D49B0CA0DF3EA1A982F9D015B394369CFCE5B4C573149A01F9C.

Four managed UIAutomation DLLs load from Windows GAC_MSIL,version4.8.9166.0;
nativeCore loads from Windows System32,7.2.22621.1. Each captured hash matches
readback and each Authenticode status is Valid,signer Microsoft Windows.
Audit-UiaComponentInventory.ps1 pins diagnostic receipt,requires completed
inventory and safe ownership,rehashes around signature queries and saves
component-signatures.json SHA22F40B245DC4AF94B7C80CDDC9A1601718ED52884C19EEFE1E6893020C81DA75.
No app-local replacement observed;valid signatures do not prove functional
compatibility or complete system integrity. No system repair performed.

Audit-FrameworkUiaDeclarations.ps1 reflects exact captured client8035D387 under
Windows PowerShell: GetVisibleRanges uses out VARIANT SAFEARRAY;conversion uses
ref VARIANT/out SafeTextRangeHandle;GetText uses SafeTextRangeHandle/int/out BSTR.
These shapes match current .NET source and Microsoft API documentation,not an
obvious custom declaration mismatch. Initial -File blocked by execution policy;
rerun used process-only ExecutionPolicy Bypass,no persistent policy changes.
framework-declarations.json contains all three method/parameter attributes.

Next minimal-only raw VARIANT-pointer conversion comparison:pass the ORIGINAL
SafeArrayGetElement buffer to native UiaHTextRangeFromVariant instead of managed
object remarshal,keep array/buffer alive through Read,retain stock crashing gate.
This isolates native handle conversion from CLR VARIANT remarshal;not yet run.
Reference https://learn.microsoft.com/en-us/windows/win32/api/uiautomationcoreapi/nf-uiautomationcoreapi-uiahtextrangefromvariant

## Cold Client And Framework Provider Controls

2026-10-29 -ColdLegacy skips modern COM reads at compile time and bypasses
server array-metadata probing before readiness. Original managed document/count/
visible text assertions remain,expected footer4;default modern-first footer9
unchanged. Minimal coldv1 still crashes at visible GetText (document/count pass).
ReceiptD26BA321252D462A0590FD9E6011CB1A03AE4418445413AF07AABE766A708D70,
compile573ms,pair10880ms/retire171ms. Query ordering is not necessary trigger.

New WpfTextProviderServerMain.cs uses standard WPF read-only TextBox,not Lea or
custom ITextProvider. Separate framework-only client locates its Edit descendant
from validated server HWND;exact19char document/one-visible-range checks precede
the same GetText assertion. Fresh WPFx64 baseline STILL0xC0000005 there.
Receipt31BB16BD2E115C00AB888766EC0123F1E4986E3816DDAE9E5B1CC75EECC5E51E,
pair10784ms/retire417ms. This is a failing control,not conformance pass.

ClientX86 compiles client /platform:x86 while WPF server staysx64;runtime logs
pointerbytes4. WPFx86client also crashes at same visible GetText,not document.
Receipt34CB3D311CE64D7D13F08CB2356E1A091A1535BB840F3A53041BCE40C4FC6CF5,
clientcompile622ms/server499ms,pair14062ms/retire1621ms.
All3:inputsunmodified,no parent timeout/no kill,server0/clientAV,both Jobs0,
output drained,owners guarded clear/retired0. Original local5s watchdog also
logs during crash handling. No deadlines/isolation relaxed. No production change.

The failure is not unique to Lea/custom minimal providers,nor fixed by changing
client bitness. It does NOT prove OS corruption or eliminate every harness issue.
Next inspect actual loaded UIAutomationClient/UIAutomationTypes/nativeCore paths,
versions,hashes and signatures;compare framework client declarations with exports.
Keep failed legacy gate;do not waive it based on the WPF result.

## Native Array Identifier Comparison

2026-10-29 document-native-broker-v3 passes300 overlapping checks (86+214).
Native SDK array: VT_UNKNOWN13/features0240/IID_IUnknown; first element pointer
equals its QI ITextRangeProvider pointer. Direct native GetText reads75chars.
Receipt C2227354CBE242D4EE82000DA883CF3D2951DC400DB8989C3DF3D9C54503BDFB.
No evidence of a wrong first-element vtable from these observations.

Explicit SafeArrayUserDefinedSubType annotation in independent typedv3 STILL
emits IID_IUnknown. Modern COM passes/stock managed visible text crashes.
Receipt25757E8C606127583B6A9B91BE4951C4B7A242620D2088FC6BC0D7CD03B2EA1F.
Typedv1 was invalid (internal contract); publicv2/v3 retained as failure evidence.

New minimal-only -TypedArrays -NativeArrays experiment uses PreserveSig HRESULT/
out SAFEARRAY methods with SDK IID/order,SafeArrayCreateEx and balanced interface
references via SafeArrayPutElement. Server metadata independently verifies actual
array IID5347ad7b-c355-46f8-aff5-909033582f63 (ITextRangeProvider),not IUnknown.
Native-array-v1 modern COM document/visible/rectangles pass,stock managed document
passes,visible GetText STILL0xC0000005. Array IID alone is not the fix.
Receipt239B28ACDDF0E71976E60FBE288FC923028CC076950FBAABC49C0643B9CD7B56.
Pair11072ms/retirement163ms,server0/clientAV,no parent timeout/no kill,Jobs0/
drain,inputs unchanged,no Lea client assembly,owner guarded clear/retired0.
Client also reports its original five-second local watchdog during crash handling;
this is NOT a clean client pass. Original deadlines/isolation unchanged.
Metadata probe now rejects non-interface element types before pointer-size reads.

No production engine change. Legacy gate remains open; no OS fault established.
Next isolate call ordering: a cold stock-managed client before any modern COM
queries,compared with current modern-first ordering,under same owned pair.
Do not remove existing legacy checks or change layout to conceal the crash.
Native API semantics reference:
https://learn.microsoft.com/en-us/windows/win32/api/oleauto/nf-oleauto-safearraycreateex
https://learn.microsoft.com/en-us/windows/win32/api/oleauto/nf-oleauto-safearrayputelement

## Exact Lea Separate-Process Comparison

2026-10-29 LeaTextProviderServerMain.cs renders a fresh constant HTML fixture in
the EXACT2225/coreA85 pair from the full scripting-qualified runtime. It checks
Dtx equals Alpha bravo minimal,selects0..5 and verifies Sds equals Alpha before
publishing readiness. Different GUI server/client executables run in different
owned processes. Client remains compiled against framework/UIA only;runtime
no-Lea audit is repeated before/after modern COM reads and before managed range
read. Driver checks the pinned5F4C37A3 full-script receipt and copies exact DLLs,
not a rebuilt lookalike. No production engine source changed.

Invoke-UiaProcessPair now accepts a separate server EXE,keeping original atomic
Job ownership/nonce/HWND/PID checks/shared30s+5s budgets unchanged. Explicit
--lea client fixture requires Single selection (minimal fixture still None)
and runs selected-Alpha text/rectangle assertions,not only document reads.

lea-cross-process-provider-v1 receipt
5D5D2B5475C2BF73BC8A0F5A44EF9CA5D4BD08AAB463DB278C98FFA4A9E15A96:
modern COM document,visible text/rectangles AND selectedAlpha/rectangles pass.
Stock managed document read passes,visible-array returns,visible[0].GetText
access-violates in stock RawTextRange_GetText. Custom C API probe bypassed;
client runtime audit confirms no Lea assembly loaded. Overall FALSE.
Compileclient639ms/compileserver593ms,pair11294ms/shared retirement163ms;
server exit0/client0xC0000005,no parent timeout/no kill,both Jobs0/output drained,
unchanged inputs,original owner guarded clear/retired0. Server stops normally
after client exit;no browser release or all-client qualification inferred.

Both minimal and actual Lea now reproduce the issue across genuine process
boundaries. Next compare native SAFEARRAY IID/element interface identity and
typed provider-array marshalling against the SDK contract. This is a hypothesis,
not a proven cause. Do not remove the legacy gate or change document/layout
semantics to avoid it;exact2225 full scripting proof remains separate.

## Separate-Process Result

2026-10-29: MinimalTextProviderPairModes.cs adds a GUI server and windowless
client mode to the independent query fixture. Atomic ready-file publication
contains fresh nonce/HWND/provider PID. Parent matches the PID to the retained
server handle;client verifies GetWindowThreadProcessId matches that PID and is
not its own. No PID-based process reopening. No fixture Lea assembly references.

Invoke-UiaProcessPair.ps1 uses pinned OwnedTestProcess.Start for BOTH processes,
retains both original owners in the global ledger before launch,checks atomic
Job-before-resume and creation times,pumps bounded4MiB output,and enforces one
shared30s execution/5s retirement-and-drain budget. The existing normal-teardown
allowance remains inside that shared5s. Unverified owners stay retained;no
generic Process.Start/background orphan or per-process deadline extensions.

minimal-pair-control-v1 verifies readiness/process separation/normal stop and
both successful exits WITHOUT running UIA text queries. It is a harness control,
NOT an accessibility pass. Receipt
F01FF1665128BFC3318D87CB7F14A052994ACE599F1E26E127772390C1920312:
compile591ms,pair2558ms,shared retirement513ms;both exit0/no kill/Job0/drain,
inputs unchanged,owner guarded clear/retired0.

minimal-cross-process-provider-v2 executes actual modern COM and stock managed
queries in a different client process. Runtime no-Lea audit passes. Modern COM
document/visible text/rectangles pass;stock managed document read passes,then
stock visible[0].GetText access-violates. Custom legacy C probe is not called.
Server receives nonce-matched stop and exits0;client exits0xC0000005. Overall
FAIL,not a release pass. Receipt
27718ED01F4A00F99B0D119488A0F2C0BA2FF69DAF06386CB198346669607F64:
compile619ms,pair10571ms,shared retirement131ms;no deadline timeout/no kill,
both Jobs0/output drained,inputs unchanged,original owner guarded retired0.
Client/provider PID correspondence and logs retained in each pair receipt.

First pairv1 already reproduced the client crash,receipt
5926359D21B8250C3B421C666D8558509E1B9CF71C2C030C1071598D8F700606,
but transient normal server teardown triggered cleanup killRequestedTRUE despite
server exit0. It is retained and NOT clean-teardown qualification. The helper
then adopted the pinned single-process helper's same shared-budget normal
teardown allowance and added pre-client/loop/final deadline and post-flush
budget checks. Controlv1 and actualv2 above test the hardened version.

This is no longer solely an in-process/self-automation reproduction. It still
does not prove an OS defect or full provider conformance;common managed provider
publication/marshalling assumptions remain candidates. Next run exact2225 Lea
in the same separate-process setup,retaining all current crashing tests. No
production engine changes or final release gate closure.

2026-10-29. MinimalTextProviderMain.cs supplies one fixed Alpha text range on a
standalone WinForms control. It references only framework/UIA assemblies,not
Lea.Engine/Lea.Win. This is a query-only diagnostic provider,not a full text
editing/movement conformance implementation. It retains its range permanently.
Both provider and client run in ONE process on separate UI/MTA threads;these
results do not qualify cross-process screen-reader behavior.

## Confirmed Comparison

minimal-text-provider-v4 executes a runtime no-Lea assembly audit,then modern
CUIAutomation document/visible-text/rectangle reads successfully. Native legacy
document read succeeds (19 chars),but converted visible-array GetText access-
violates. ReceiptA43D6EE1C902988C183D3E54783178D634246B20B4382E6A36A26C12E5CA0E70;
compile595ms/run9302ms,exit0xC0000005,no parent timeout,cleanup killRequestedTRUE,
Job0/drain,max235ms retirement,inputs unchanged,owner guarded retired0.

To rule out the custom C API probe as a necessary trigger,-ManagedOnly skips
DocumentLegacyHandleChecks entirely. minimal-managed-provider-v3 confirms:
- Runtime assembly audit passes,no Lea assembly loaded.
- Modern COM document/visible-text/rectangle reads complete.
- Stock managed DocumentRange.GetText succeeds and visible array is returned.
- Stock managed visible[0].GetText access-violates inside
  MS.Internal.Automation.UiaCoreApi.RawTextRange_GetText.

Receipt360B1E054428220FECA649EDD2C12F55A6B363DCDB8947DDFBC0E50E5901457D;
compile656ms/run9434ms,exit0xC0000005,no parent timeout,
cleanup killRequestedTRUE,Job0/drain,max192ms retirement,inputs unchanged,
original owner guarded clear/retired0. Overall result FALSE,no pass footer.
Loaded UIAutomationClient file4.8.9166.0/assembly4.0.0.0;actual loaded native core
C:/Windows/System32/uiautomationcore.dll file7.2.22621.1. Versions are recorded
observations,not a claim that a newer runtime fixes this.

This proves Lea's layout/selection code and the custom legacy PInvoke probe are
not necessary to reproduce this in-process failure. It does NOT prove an OS bug:
shared provider publication/marshalling assumptions or environment behavior can
still be involved. Microsoft [deprecates the C client GetText API](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationcoreapi/nf-uiautomationcoreapi-textrange_gettext)
in favor of COM,but that alone does not close or remove Lea's legacy gate.

## Fixture Corrections And Retained Evidence

v1 independent C probe crashes without modern comparison;receipt9449EB8E2EEEFC2AF0AE81BBDD003684C9F52EA904B0AA396DED69129013E9AD.
v2 modern comparison initially rejects None selection because reused helper's
Lea contract requiresSingle;receipt6ACB732CD32DE50D044C30EA9FE544AB94B166E99BF9FD0A1DCA2A5511400F86.
DocumentNativeClientChecks now accepts an exact expectedSelection parameter,
default1 unchanged for ALL existing Lea calls;minimal read-only fixture passes0.
No accepting-any-mode fallback or Lea selection/privacy check removal.
v3 runner compiles then stops before native run on null empty argument list;
receipt25EFA812046BB8F5257C5CB209988B70E3586C7EFCD1187CBA550004815C330A.
Fixed by explicit empty-array initialization;v4 is actual combined repro above.
Managed-onlyv1/v2 reproduce stock wrapper failure;v2 adds precise boundary/module
traces;v3 adds machine-readable runtime audit derived from captured output.
Failed receipts/sources/executables retained. Every owner guarded retired0.
Original compile120s/run30s/shared-retirement5s budgets unchanged.

## Next Action

Separate provider and legacy client into genuinely different processes under
original-handle/atomic-Job ownership and unchanged deadlines. Compare minimal
provider first,then exact2225 Lea. Do not launch an unmanaged orphan/helper with
generic Process.Start or call these in-process probes external-process proof.
Keep the existing crashing tests;no blanket exception suppression or capability
removal. Exact2225 engine scripting3412-check pass remains separate evidence;
all final release gates remain open. No production engine source changed here.
