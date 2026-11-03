# Portable Native SDK Build

2026-11-01. A local development source-build milestone, not finished Lea/Firespider.
The original independent engine/runtime remains; no Chromium/WebView is added.

## Workflow

Lea.Sdk/SDK.Full.Build.proj adds CompileWorkerNative for already-built IL,
BuildWorkerNative for IL then native compilation, and BuildAll. Short aliases:
Bl libraries, Bi worker IL, Bn native worker, Bs samples, Ba all. Default remains
BuildLibraries. PowerShell7.4+/Windowsx64, authenticated Microsoft crossgen2 8.0.32
and installed8.0.33 references are explicit prerequisites; no automatic native-tool
download/version substitution/IL fallback. The toolchain generator is a byte
snapshot, NOT origin authentication. SDK_FULL.md inside the source describes setup.

Build-NativeWorker.ps1 pins supplied manifest SHA256, every compiler file/runtime
DLL and six IL/runtime members. Requires exact runtime8.0.33/RollForwardDisable,
ReadyToRun Jint/Acornima, and verifies all three AMD64 R2R headers after compilation.
Publishes exact six-file closure from fresh private staging. Its sibling receipt
allows unchanged builds only after all input/output hashes revalidate; changed
inputs or modified output fail closed rather than overwriting old output. Native
compilation/header checks alone do not qualify security or runtime behavior.

## Native Target Controls

Test-PortableNativeBuild invokes the actual MSBuild target with the existing
qualified source IL and compiler. Eight checks: native closure, unchanged cache,
missing manifest/wrong manifest hash/unrecorded reference/IL-only interpreter/
relaxed runtime policy/tampered cached image rejection. Nine children, expected
exit codes0/0/0/1/1/1/1/1/1; no timeout/kill/Job residue/output loss, ownerretired0.
Produced six files byte-identical to the old qualified5BF8 closure, not new worker
runtime qualification. Receipt:
I:/LeaBuildStorage/portable-native-build-20261101-v1/result.json
SHA256DB9683B3B9F271930993BC72036E093A948A48B0184DF998FE05D070F2FD17FA.

## Complete Exported Source Build

Prepare-FullSdk -Version2 creates a NEW source-only candidate from the mature
core/view/print/image/parent/adapter/worker/consumer sources. Adds three native
build assets,131members total; eight third-party dependency DLLs, NO Lea product
EXE/DLL inputs. Worker project XML dependency/output inverse retained.
I:/LeaBuildStorage/full-sdk-source-20261101-v2/source-manifest.json
SHA256E796F6ED2A1AA4C5A18962A1A03D75359EA951A4A35DC117DF1411EEDD8E3A2C.

Test-PortableFullSdk runs Ba from that exported directory. Framework libraries,
worker IL/native and both sample EXEs build in77589ms/120s with no old Lea product
staging. Fresh C:NTFS text consumer passes14checks3807ms; script consumer17checks
3359ms, original30s budgets/5s cleanup, worker six ACLs restored/input bytes stable.
All three children0/no timeout/kill/Job0/drain, ownerretired0. Receipt:
I:/LeaBuildStorage/portable-full-sdk-build-20261101-v1/result.json
SHA25617AAF573A110588C3D9A0C43944465B0504B44865CBBAFDFBE6C9D319C29C2C7.

New products (source paths/build metadata may affect identity; cause not audited):
- Engine:59AA3DEE913E18EE528B4CFB4665D871C5E4D013D9C763433C04925FD4F4240D
- View:38666DD456DF00ADD2067E668FC9936543AF6239BA148F35D447CFA5241E5508
- Image:306436C2F914416878AEDC404A23D5EDA89E9C87FBFCD040F832E8B02C2165E8
- Print:35E0E63AB4B93FC0D120FC9B3FD6F35695DF9967EA7D4FBF0574D0F55D85D73F
- Parent:6203AAFD5EC9F43D9CD0D7A2B961FC22ECAA956AAD22AD15EDF9A9F742C58019
- Adapter:A1EC3FA63CD0FE0A32141F5FE071C436B2C9444FB36453C93EB26F9039D7DF28
- Worker:25BD53348CB21FA70DCD22D78591C6B172E94526F2E446A68267A4F433840A8A

Test-FullSdkWorkerGate -Portable uses these exact staged outputs:25original
isolation/no-idle-dispatcher/CPU/memory/noCLRnetwork/realm/affinity/retirement/ACL
checks pass3053ms, compile595ms, both0/Job0/drain/ownerretired0. Receipt:
I:/LeaBuildStorage/portable-sdk-worker-gate-20261101-v1/result.json
SHA2569F293CD17DED933C51D4D9D55EE6896AB6DE77B99B8DE4B6688CEB6548AB7ADC.
Older default runner mode unchanged; replaying its historical root-source hashes
requires checkpoint73 because the runner gained a new explicit Portable mode.

Audit-PortableSdkBuild verifies current sources/products/ACLs,14owned children,
expected negative messages, footer14/17/25, five aliases and131source members:
I:/LeaBuildStorage/portable-sdk-build-audit-20261101-v1/result.json
SHA256835E6B92B906566352171B85172CA67ED000B1CEE284AF3CDDCB10567D02CDAF.
Selected360px selected-text and script-window screenshots visually inspected.

## Preserved Source And Remaining Work

Package-PortableSdkSource includes exactly131source members plus original manifest,
132entries, full SHA readback; excludes bin/obj/worker-il/local toolchain config.
I:/LeaBuildStorage/portable-sdk-source-package-20261101-v1/Lea-full-sdk-source-development-v2.zip
SHA256483A73DD1979955D1C457F377BD0BDA4D896792B35CF6B3E4EB30E3040868AB0,
same verified C:copy continuity-evidence/portable-full-sdk-source-v2.zip. Includes
eight dependency images; redistribution/notices review is NOT complete. No GitHub
upload confirmed. Working source and every retained failure remain preserved.

Source build and focused consumers do not close broader qualification. New seven
product identities cannot inherit old font/image/print/fullscript results. Inspect
reproducibility/compiler-path metadata before further source rebuilds; then run
broader exact-candidate regressions, retain both timing failures and complete
startup/fullscript/Firespider/notices/clean-machine delivery. All six gates OPEN.
