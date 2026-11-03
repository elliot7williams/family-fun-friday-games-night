# Portable SDK Layout Candidate

## ZIP Execution And Source Rebuild

2026-10-31 Test-PortableSdkImages -Package -SourceBuild verifies ZIP8E16F555...,
extracts66entries to fresh NTFS, checksmanifest65members, stages53sourcefiles
excluding8prebuilt members, and asserts no DLL/EXE before rebuilding.
Aggregate34183ms/120s; sample14checks2898ms/30s; image42checks6703ms/30s;
all4processes0/no timeout/kill/Job0/drain,maxretire303ms/5s,inputs stable/ACL
restored. Owner retired0. Receipt6ECCE54E... in sdk-package-source-v1.
Audit-PortableSdkSource -Package PASS B699B8989A1D58CEB774F60F960065B149F9B9706A11CC24A8DAF0A8692B2111.
New rebuilt product prefixes4C287352/1AD65CF1/8922D309/44E18619 have only these
embedding/image results; no transferred font/retirement passes or byte identity.

Separate -Package (without -SourceBuild) tests the ZIP's shippedDC71/6F90/A4E4/
87AF products after fresh NTFS extraction. Embedding14checks3592ms/image42checks
8218ms; all3processes0/no timeout/kill/Job0/drain,maxretire609ms/5s,inputsstable/
ACLrestored/ownerretired0. Receipt sdk-package-runtime-v1/result.json SHA256
130A01E4A3CA7D351BBB6DF93E8E0E044A2694AB062AC87D14667205905E414D.
The runner's historical sourceBuiltProducts/source-embedding labels also appear
in runtime-only mode; absent aggregateSourceBuildPassed means no rebuild.
No clean-PC/fullsecurity/completeSDK certification. Next JS/print/notices/host.

## Development Archive

2026-10-31 Package-AggregateSdk.ps1 creates:
I:\LeaBuildStorage\sdk-aggregate-package-20261031-v1\Lea-SDK-development.zip
982372bytes; SHA256
8E16F555B7D94D5ED65A02E5CD57EFBE30764EC8B092F404088A0A4C87F194EA.
66entries:53source/project/docs files,8runtimefiles,3evidence receipts,status
and integritymanifest. Source pins match tested aggregate input inventory;
runtime products match DC71/6F90/A4E4/87AF identities, not earlier SDK outputs.
Every ZIP entry and fresh I:verified-extraction file matched length/hash.
Package receipt in same folder records extractedRuntimeExecuted=false.
Next run extracted sample/image checks and a fresh source-only build on NTFS
under existing owned deadlines. Integrity verification alone is not runtime,
clean-machine, reproducible-build or full SDK qualification. Compression saves
storage/download bytes, not engine execution time. Missing components remain.

## Aggregate Decoder Lifecycle

2026-10-31 source-built DC71E2B9/6F907862/A4E44422 products pass retirement81
in2970ms and shutdown-race14 in4101ms. Separate fresh NTFS suites preserve
120s compile /30s execution /5s cleanup limits. All4 processes exit0, no
timeout/kill, Job0/drained output; inputs unchanged and worker ACL restored.
Both retained owners retired0. Receipts:
- sdk-aggregate-decoder-retirement-v1: 057790C6A830481B0154FC668A50D4A6CBC078929D00196F85CDF87B54E5F92B
- sdk-aggregate-decoder-shutdown-race-v1: 80B92A594BCF92333DC58D2B5F50882A0561EE0C6F145E3D49F372CA4D128A77

Independent Audit-AggregateDecoder verifies product identities, terminal81/14
footers, original budgets, lifecycle, every input hash and current restored ACL.
Audit sdk-aggregate-decoder-audit-v1 SHA256
BF09BC5E7FD4E145D862BA9030FBABD6F8386E0EC28FA1485C954D76777C4245.
No global confinement or complete SDK qualification. Next source SDK package,
fresh extraction verification, then remaining JavaScript/print/notices/host work.

## Aggregate Font Regressions

2026-10-31 new DC71E2B9 core / 6F907862 view pass shorthand26, initial12,
relative18, initial-render142 and relative-render142 checks. Tests copy exact
source-built products, never rebuild or substitute prior binaries. Original
120s compile / 30s test / 5s retirement limits retained. All10 processes exit0,
no timeout/kill, Job0/drained output, inputs unchanged. All5 owners retired0.
Narrow relative selected screenshot inspected. Independent audit:
`continuity-evidence/sdk-aggregate-core-view-regression-20261031-v1/result.json`,
SHA256 F6C8ADB6D27726EF18D7988C8C456799289CDF22B14C21444ACF8956CE9F9B40.
Reproduce via Test-FontShorthand -Candidate -SdkPair -AggregateSdkPair with
separate default, -Initial, -Relative, -Visual -InitialRender and
-Visual -RelativeRender modes; retained nonadmin NoExit TTY required.
Aggregate worker retirement/shutdown now qualified narrowly as recorded above.
No complete SDK, full browser compatibility or release qualification claimed.

## Fresh Aggregate Source Build

2026-10-31 Test-PortableSdkImages.ps1 -SourceBuild stages53source/project/docs
files,explicitly excludes8binmembers and checks no DLL/EXE exists before build.
SDK.Build.proj then builds core,view,imageworker and sample from this fresh tree.
Aggregate34196ms/120s;new sample14checks3554ms/30s;new view/image42checks6577ms/
30s;all4 compiler/test processes0/no timeout/kill/Job0/drain,maxretire381ms/5s,
inputs unchanged andworkerACLrestored. Data360 screenshot inspected;
owner87068 guardedretired0. This supersedes missingaggregate-build notes below.

Rawreceipt continuity-evidence/sdk-portable-source-v1/result.json SHA256
57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36.
Audit-PortableSdkSource.ps1 independentlychecks run budgets/retirement/terminal
footers/currentinput+producthashes/workerACL. Auditreceipt06A03EB50DC7F9E5C63169470980389F4B5E5D9120C961E51BA9FF8F337A334E.
NewcoreDC71E2B9/view6F907862/workerA4E44422/sample87AF9880 fullhashes are inraw
receipt. These differ from earlierSDK772A/8008/EF89;no priorbroaderfont/lifecycle
qualification is transferred. Nextnewcandidate-boundregressions/sourcepackage,
thenremainingSDKcomponents. Byte-reproducibility/cleanmachine/fullSDK stillopen.

2026-10-31 Prepare-PortableSdk.ps1 stages61 verified files at
I:\LeaBuildStorage\portable-sdk-20261031-v1. It copies45 core/view source/project
inputs,3 image-worker source/project inputs and the qualified sample sources.
Known tested binaries are provided in bin/Release and beside the sample EXE.
SDK.Build.proj builds view/core,image worker and sample sequentially. Relative
project paths do not depend on the evidence directories or a particular drive.

The sample project is a structured XML projection of the tested project,adding
CopyToOutputDirectory for Lea.Image.exe;the C# consumer source is unchanged.
The manifest records both original and projected project hashes. The combined
source build has NOT yet been executed or qualified. Existingbinary qualification
must not be transferred to newly rebuilt aggregate outputs.

## Actual View Image Loading

Test-PortableSdkImages.ps1 copies all61 manifest members into fresh NTFS staging
and compiles SdkImageViewMain beside the sample binaries. The probe uses V's
default worker path,not an override. File and data-URI PNGs are loaded at720/360.
42checks cover cache success,worker starts/isolation flags,unchanged2500ms CPU
budget,document text,known file-cache pixels and visible red/blue image pixels.
Four screenshots are retained;data360 was inspected.

Receipt continuity-evidence/sdk-portable-images-v1/result.json SHA256
D5313F3D71C5CE44B5506B95E014551690B7BF0948D67AA35BD08E7BAD6D89E7.
Compile664ms/120s,test8338ms/30s,exit0/no timeout/kill,Job0/outputdrained,
maxretire224ms/5s,allinputbytes unchanged andworkerACLrestored. Owner16430
guardedretired0. Tested exact products remain772Acore/8008view/EF89imageworker,
not the browser package's4DA8/CA51/FE74 pairing.

No arbitrary-site,image-format,globalconfinement,profile-crash-recovery orcomplete
SDK proof. Next fresh aggregate source build and candidate-bound regressions.
JavaScript parent/adapter/worker,printing,notices andfullhostsample delivery remain
unfinished. Legacy UIA/startup reliability andall6finalreleasegates remainOPEN.
