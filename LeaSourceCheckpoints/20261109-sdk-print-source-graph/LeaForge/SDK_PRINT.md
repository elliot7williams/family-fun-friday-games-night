# Portable Print SDK Candidate

## Current Direct Source Integration: 2026-11-09

New Lea.Sdk/Lea.Print.csproj exports the existing22unchanged mature C# sources
with automatic core/view ProjectReference entries when matching projects exist
and no LeaCorePath/LeaViewPath is supplied. Explicit binary paths or
LeaBuildFromSource=false retain binary references and the missing-core guard.
Defaults preserve the earlier exported SDK's WPF/selection configuration. Root
Lea.Print.csproj remains explicit-binary; no prototype auto-selection was added.
The source net48 guard exists but its wrong-target path remains untested.

Test-SdkPrintSourceGraph.ps1 stages the preceding75verified source members plus
23print members, with six pinned third-party libraries and no Lea binaries.
Old explicit-core failure reproduced, exit1/2898ms/retire172. Clean direct print
build35094ms builds core/view/print; sample-image8122ms and adapter11032ms add
the other roles. Explicit-path print build6093ms succeeds despite unbuildable
sibling project controls. Output core11B154A1/view61DA2736/imageDA3BFA67/
parent406C5634/adapter58E4D8D8 match the preceding source campaign. New print:
414E1CD817F060758978C049595CC7E7BB15EF5270DC4053B40AFCCC2D47923B.

Receipt on I:/LeaBuildStorage/sdk-print-source-graph-20261109-v1/result.json SHA
7D636A1E195DD1C9E207E09F4E86826DCB9C7FDB0231951FA2F8A2BA9571FB48.
All three existing pinned test executables are reused, not recompiled; exact
new source-built DLLs/decoder are copied to fresh NTFS runtime on C:.
Capture/styled-document/XPS/preview foundation PASS20169ms;
range37/geometry16/native-selection27/ticket22 PASS5271ms;
baseline40/friendly46/live-image250 PASS13172ms. Retires190/186/228ms.
No print job submitted; no claim of a physical printer/clean-machine test.

Eight owned runs including expected negative have original handles, Job0,
drained output, no timeout/kill and120sbuild/30stest/5sretirement budgets.
Inputs and14runtime file bytes/ACLs remain stable; owner43567 guarded exit0.
Narrow preview and color image-layout screenshots viewed: framed page/toolbar
fit, visible article label and red/blue image pixels. Audit independently checks
all98source members,22unchanged print CS files, six products, runtime identities,
ownership/budgets, footer/counts and copied evidence. Audit path on I:
sdk-print-source-graph-audit-20261109-v1/audit.json SHA
24A2A32A5FAAB645E3A44E7915725AC4EB9C42CC6CE6DC9C56DDEDEB201A8E03.

Use/source commands: Lea.Sdk/SOURCE_GRAPH.md. Build artifacts remain on I:;
native runtime stays on NTFS. Older package checks below are historical and do
not qualify this new print DLL. JavaScript worker/full scripting, startup repair,
Firespider adoption, broad compatibility/security/notices and complete delivery
remain unfinished. All six release gates remain OPEN.

## ZIP Source-Only Rebuild

2026-10-31 Test-PortableSdkImages -SourceBuild -PrintSdk -Package verifies
ZIP2B035A42.../93entries, stages76sourcefiles excluding10runtimefiles andasserts
noDLL/EXE beforeaggregatebuild. All5products compile38056ms/120s; new sample
14checks2824ms/30s; image42checks6322ms/30s. All4processes0/no timeout/kill/
Job0/drain,maxretire115ms; inputsstable/ACLrestored/ownerretired0.
Receipt sdk-print-package-source-v1/result.json SHA256
649AD2862712AF78954243DBBE56E80D004D31E09ADD440757A205619D69F362.
Audit-PortableSdkSource -Package -PrintSdk independently verifies budgets,
terminalfooters, input/producthashes andrestoredACL. source-audit.json SHA256
DE2213F16532D00D925E4DEFDA453FC43447B6FBE2C0FDDC91B2A1CEFC5B6340.
NewcoreDA37/viewE5B0/worker4333/printAF13/sample6BBD have onlythese14+42results.
They do notinherit shipped5B8A/A622/B5E1/519F/358E font/print/lifecyclepasses.
QualifiedshippedZIP stays unchanged; thisbuild proves sourceportability,not
byte-reproducibility/cleanPC/fullprinting/globalsecurity ornewrelease status.
NextmissingJavaScriptSDK parent/adapter/worker sourceprojects, thennotices/host.

## Shipped ZIP Runtime Verification

2026-10-31 Test-SdkPrintRuntime -Images -Aggregate -Package extracts pinned
ZIP2B035A42... into freshNTFS, verifies93entries/92manifestmembers and copies
exact5B8A/A622/B5E1/519F/358E runtime from that extraction. No rebuild/fallback.
Embedding14checks3642ms,capture/export19112ms,range37/geometry16/selection27/
ticket22 in4844ms,image40baseline/46friendly/250live in13044ms. All4processes0,
no timeout/kill/Job0/drain,maxretire243ms; original30s/5slimits,inputs unchanged,
workerACLrestored/ownerretired0. No printjob/defaultchange.
Receipt sdk-print-package-runtime-v1/result.json SHA256
408F197F20E1A1EFCE406906DD1EEDBF75DFF7DAE07362A9E55B330046BCF6B2.
Audit-SdkPrintRuntime -Images -Aggregate -Package verifies archive identity,
allinput/producthashes,terminalfooters/counts/lifecycle/restoredACL. Audit SHA
7705060A0F4DDFD9A19F6B2BD6F4A6E3BF3A07EDD077EA8404745A7BE2509F65.
Original package receipt's extractedRuntimeExecuted=false is historical, not
rewritten; this follow-up supplies the runtime evidence. Next source-only ZIP
build excluding10runtimefiles, thenmissingJavaScript/notices/host. NotcleanPC,
fullsecurity/completeSDK qualification.

## Print SDK Archive

2026-10-31 Package-AggregateSdk -PrintSdk creates
I:\LeaBuildStorage\sdk-print-aggregate-package-20261031-v1\Lea-SDK-development.zip.
1090374bytes/93entries; SHA256
2B035A42A34A375393D5E4F4E2B27119363C81DF25239C3BF87412B4980C3133.
76source/project/docs files match aggregate build inputs;10runtimefiles include
exact5B8A/A622/B5E1/519F/358E products, root+sample component copies and HTML.
Five evidence receipts preserve original overallfailedsourcecampaign and separate
passingruntime/font/decoder audits. Status andintegritymanifest included.
All ZIP entries and fresh I:verified-extraction files match lengths/hashes.
Receipt result.json records extractedRuntimeExecuted=false: no execution from
this ZIP yet. Older archive untouched. PACKAGE_STATUS.md explicitly supersedes
the historical README statement that printing is missing, without claiming full
printing/security/SDK qualification. JavaScript/notices/fullhost remain absent.
Next fresh NTFS shipped-runtime extraction checks without rebuild, then fresh
source-only build with newcandidate-bound verification. No GitHubupload claim.

## Aggregate Font Regression

2026-10-31 Test-FontShorthand -Candidate -SdkPair -AggregateSdkPair -PrintSdk
qualifies exact5B8Acore/A622view withoutproductionrebuild. Defaultshorthand26,
-Initial12, -Relative18, -Visual -InitialRender142, -Visual -RelativeRender142
allPASS. Testelapsed1686/789/1424/25430/24390ms. Original120scompile/30stest/
5scleanup limits, all10processes0/no timeout/kill/Job0/drain, inputsunchanged.
All5ownersretired0; narrowrelative selectedPNGinspected.

Independent Audit-SdkCoreViewRegressions -AggregateSdkPair -PrintSdk verifies
receiptpins, producthashes, exactterminalfooters, budgets, lifecycle/allinputs.
Audit sdk-print-aggregate-core-view-regression-v1/result.json SHA256
83677E5B754B9100081A82281BD44F479B316F3B7CC11D1D533F9A1086C5C27F.
Next newSDKZIP with76sources andexact5testedproducts; preserve existing ZIP
and sourcecampaignstoragefailure. JS/notices/host/fullrelease remainunfinished.

## Aggregate Decoder Lifecycle

2026-10-31 Test-DecoderRetirement -RealDecoder -SdkPair -AggregateSdkPair
-PrintSdk, and separate -ShutdownRace, qualify the existing5B8Acore/A622view/
B5E1worker. No production rebuild/substitution. Retirement81checks3668ms,
shutdownrace14checks3602ms; all4processes0/no timeout/kill/Job0/drain,
original120scompile/30stest/5scleanup limits, inputs unchanged/workerACLrestored.
Both retained owners retired0.

Receipts under continuity-evidence:
- sdk-print-aggregate-decoder-retirement-v1: 77234D023B8554545534A3FB5C7C6A30604184BC38829349E32428A87F6D85A9
- sdk-print-aggregate-decoder-shutdown-race-v1: 7696D49DFB87F8495F3183A1348EFB4ECD4884C2719E86B373095209170DB4CC

Audit-AggregateDecoder -PrintSdk verifies current/originalproducts, allinputs,
terminal81/14footers, deadlines/lifecycle andcurrentrestoredACL. Audit receipt
sdk-print-aggregate-decoder-audit-v1/result.json SHA256
9473FA3B160D46F2883E3CD9256AF1A989F2B1BB99890A2A7EC9F98A36431262.
Next samecandidatefont models/rendering thennewSDKZIP. Existing ZIPunchanged;
global confinement/fullsecurity/fullSDK/cleanPC qualification remains incomplete.

## Aggregate Runtime Follow-Up

2026-10-31 C:reserve restored to1315508224bytes atpreflight; no cleanup performed.
Test-SdkPrintRuntime -Images -Aggregate tests existing sourcebuild products
5B8Acore/A622view/B5E1worker/519Fprint/358Esample withoutanotherbuild. Earlier
failed storage campaign is retained. Newruntime receipt:
sdk-print-aggregate-runtime-v1/result.json SHA256
D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676.
Embedding14checks4791ms, capture/export19404ms, range37/geometry16/selection27/
ticket22 in7246ms, image40baseline/46friendly/250live in12891ms. All4processes
exit0/no timeout/kill/Job0/drain,maxretire1036ms; unchanged30s/5s guards,
inputs stable/workerACLrestored/ownerretired0. No printerjob/defaultchange.
Independent Audit-SdkPrintRuntime -Images -Aggregate checks all5producthashes,
every input, restoredACL, counts/footers andlifecycle. runtime-audit.json SHA
93868A8836E302732AAA8484DA69B430BBF65E040868604A600B24DF8B26EE33.
Next candidate-bound font and decoder lifecycle checks andnewprintSDK package;
these tests do not transfer olderbinary results, provefullsecurity orcleanPC.

## Aggregate Source Integration

2026-10-31 Prepare-PrintPortableSdk.ps1 creates76source/project/doc members at
I:\LeaBuildStorage\portable-print-sdk-20261031-v1 without prebuilt DLLs/EXEs.
XML projection builds print after core/view/image and before sample, adds sample
Lea.Print reference with CopyLocal. All C# sources unchanged. ExistingZIP retained.

Test-PortableSdkImages -SourceBuild -PrintSdk compiles all5products in58150ms/
120s. Compiler exit0/no timeout/kill/Job0/drain/retire524ms; inputs unchanged.
Core5B8A501D/viewA622A1C2/workerB5E1A902/print519F26E0/sample358ED2BB are NEW
identities; full hashes in sdk-print-aggregate-source-v1/result.json SHA256
47C4617143CFF63510728C88587E278C423F9E3C7AFA92629BA5D29FFD48DDA6.

C:free space fell to396292096bytes; the original storage guard rejected the
embedding launch AFTER successful compilation. Receipt overallpassed=false,
runs1, no owner attention; retained owner retired0. Runtime tests did not run,
so no previous binary passes transfer. Preserve failed campaign and outputs.
Next test these existing binaries under unchanged owned limits once reserve is
restored, without another rebuild. No source/binary replacement or gate waiver.

## Image And Decoder Regression

2026-10-31 Test-SdkPrintRuntime -Images runs the same prior capture and range
suites, then unchanged pinned PrintFriendlyChecks with SDKPrint0113/coreDC71/
view6F90/workerA4E4. Fresh NTFS; no production DLL rebuild or old worker fallback.
40baseline/46friendly/250live checks pass, including decoder recapture, WPF image
layout, selection, preview and XPS export. Capture19415ms/range4997ms/images13324ms;
all3processes0/no timeout/kill/Job0/drain, original30s/5s limits,maxretire1273ms.
Inputs unchanged/workerACLrestored/ownerretired0. image-layout.png inspected:
article label and red/blue decoded image visible. No printjob/defaultchange.

Receipt sdk-print-images-v1/result.json SHA256
CD013D8EB1C4AFC55112F8337DE60ABC8D1EB1294C8F74C1C3EF1A104EFFF6A5.
Audit-SdkPrintRuntime -Images verifies products, every input, budgets/lifecycle,
40/46/250counts, priorrangefooters and restoredworkerACL; runtime-audit.json SHA
5C5D19442BF9698A336A129B0CF97B97FC8581C3712B7A4273DF930DF69F0537.
Next integrate print sources/project into a new aggregate SDK stage and verify
that fresh build. Existing source/binary ZIP remains unchanged. No physical
printing, globalsecurity or completeSDK qualification is implied.

## Runtime Regression

2026-10-31 Test-SdkPrintRuntime.ps1 copies pinned unchanged prior test executables
and fixtures into fresh NTFS alongside SDK DC71core/6F90view/0113print products.
Live styled capture/foundation/export passes in20507ms; range37, geometry16,
native-selection27 and prepared-ticket22 pass in6173ms. Both original30s test
budgets and5s cleanup limits retained. Both processes exit0/no timeout/kill,
Job0/drained output, maxretire746ms; inputs unchanged and owner retired0.
No printer dispatch or default-printer change. No production DLL rebuild in test.

Receipt continuity-evidence/sdk-print-runtime-v1/result.json SHA256
B04530D0F69FC45CE28241660B43F8D0A57425AF22254D46CDD8B968B1B89B3D.
Independent Audit-SdkPrintRuntime verifies allinput/product hashes, deadlines,
terminalfooters andcleanup: runtime-audit.json SHA256
1FD02700D36F664B12B934EDC52E5F091242C2E2FDB9B2D23FA85CF0DFF39734.
Next run additional friendly/image-layout/live-decoder suites on these binaries
before aggregate packaging. These passes are not fullprinting/physicaloutput or
global security qualification. Existing SDK ZIP remains unchanged.

2026-10-31: Prepare-SdkPrint.ps1 stages 22 unchanged C# files from the tested
relative-font-print-project-v1 source tree, plus one projected project, at
I:\LeaBuildStorage\sdk-print-20261031-v2.
Source receipt SHA256:
A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002.
Each source is matched to that receipt and copied with hash verification.

Only project configuration is projected through XML: modern C#12, shared
compiler disabled, Release default, sibling bin output, relative core/view
reference defaults, WPF and document-selection components enabled by default.
Existing explicit core reference guard and conditional feature groups remain.
No root prototype or installed browser files are replaced.

The stage includes pinned SDK DC71E2B9 core and 6F907862 view references.
Build with Test-CoreViewSdkBuild -Version 3 -DotnetSdk -PrintLibrary using the
required retained nonadmin NoExit TTY. Original 120s deadline and 5s retirement
guard remain. Build passed in5926ms; exit0/no timeout/kill, Job0/output drained,
retirement149ms, inputs unchanged, owner retired0.

Build receipt:
I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-print-library\result.json
SHA256 BC0C7001376EC1FF19FFCBEF36987F0A99282A6A79E30421EE4937F6F1B46E55.
Lea.Print.dll SHA256:
0113B106A84DDD4E19DFD6574AD13FD2511D34C17316ADC24062A73EB8021F3B.

The build alone proves compilation, not printing behavior. Runtime capture,
range and export now passed above; broader layout/image/live-view tests remain.
No printer was dispatched and no default printer was changed. Prior browser
print passes do not qualify this new binary. Aggregate SDK.Build.proj and the
existing SDK ZIP remain unchanged; print is not yet included in that package.

Retained v1 staging failed before project projection because PowerShell XML
adaptation returned no first PropertyGroup from indexing a single group.
v2 selects it with namespace-independent XPath. No compilation occurred in v1;
the partial folder is retained, not overwritten or described as a test pass.

Next: run the matched print regression suites on the new library/core/view,
then add print to a new portable aggregate source package. JavaScript, notices,
host delivery, startup reliability and legacy accessibility remain unfinished.
