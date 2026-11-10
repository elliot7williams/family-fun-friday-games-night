# Lea Framework Source Build

This development layout builds four mature libraries: Lea.Engine, Lea.Win,
Lea.Script and Lea.Script.Win, plus the separate Lea.Image decoder executable.
The latest source layout also includes Lea.Print and its22unchanged C# sources.
It preserves the current signed-relative-font
sources and private-binding bootstrap. It is not the complete SDK or a release.

Use Windows, the .NET Framework 4.8 targeting pack and a C#12-capable compiler.
The tested local layout is:

`I:\LeaBuildStorage\sdk-print-source-graph-20261109-v1\source`

Open `Lea.Script.Win\Lea.Script.Win.csproj` in Visual Studio, or from this
layout run:

```powershell
dotnet msbuild Lea.Script.Win\Lea.Script.Win.csproj /t:Build /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false
```

The net48 script projects automatically use relative project references when
matching core/view projects are present. Building the adapter then builds core,
view and parent without prebuilt Lea DLLs. Third-party Framework dependencies
must be supplied in `lib`; the local tested layout already contains them.
Public source checkpoints omit dependency binaries and record their SHA256s.
Authenticate dependency origins before restoring them; hashes alone do not
authenticate a package. Notices/redistribution review remains unfinished.

`/p:LeaBuildFromSource=false` preserves the existing core/view DLL-reference
mode. The adapter's parent-script project reference still builds Lea.Script.
Use matching mature DLLs, not the root workspace's earlier prototypes.
The workspace templates are projected to net48 when the SDK is staged; they
are not standalone mature source projects in their template directory.

Application code still uses the short aliases `E`, `V`, `Js` and `W.On(view)` /
`W.Off(view)`. Attaching the factory is not permission to run scripts. Local
scripting permission and a qualified isolated worker remain separate requirements.
No Chrome/WebView engine or unrestricted worker fallback is introduced.

Open `Sample\EmbeddingSample.csproj` to build the text/image consumer. With
sibling projects present and no explicit `LeaSdkBin`, it selects source mode,
builds core/view/image and copies Lea.Image.exe beside the sample. The decoder
ProjectReference has ReferenceOutputAssembly=false: it remains a separate
confined process, not a DLL loaded into the UI. Build the script adapter above
separately when that role is needed; the sample does not execute JavaScript.

An explicit `LeaSdkBin` preserves binary consumer mode and copies its decoder
when present. The default binary path follows `bin\$(Configuration)`.
Source-mode image deployment is conditional on the image project being present;
text-only layouts still work but do not thereby gain image support.

Building on I: is supported, but native sandbox runtime staging requires NTFS.
Do not run the image sandbox from the exFAT source output on I:. The tested
local sample executable is on NTFS:
`C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\continuity-evidence\sdk-image-source-graph-v1\runtime\Lea.EmbeddingSample.exe`.
It accepts a local HTML path. Keep matching core/view/decoder files together.

Verified in Release: original missing-dependency failure; fresh automatic
four-library source build; explicit binary mode with poisoned core source;
14rendering/selection embedding checks at720/360px;14metadata/reference and
adapter attach/detach checks;28signed-relative-font model checks.

The image-source increment adds a fresh decoder build, an old missing-worker
deployment control, explicit binary-consumer control, 14text/42file-data PNG
view checks at720/360, and 81retirement/14shutdown-race checks on the exact new
combination. Audit6C654D97; detailed receipts in LeaForge/SDK_IMAGE_WORKER.md.

Not verified here: page JavaScript execution, isolated JavaScript worker deployment,
Debug, wrong-framework build rejection,
full scripting/security/lifecycle campaign, Firespider adoption, clean-machine
installation or all-sites compatibility. Startup failures remain unresolved.
Existing target aliases in the full SDK are unchanged; this four-library source
checkpoint is not a substitute for its remaining components or release gates.

## Print Source Build

Open `Lea.Print\Lea.Print.csproj`, or build it directly:

```powershell
dotnet msbuild Lea.Print\Lea.Print.csproj /t:Build /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false
```

Without explicit LeaCorePath/LeaViewPath and with matching sibling projects,
the SDK template selects core/view source references automatically. Full WPF
printing and document selection default on, as in the prior staged SDK. Root
workspace Lea.Print is a separate explicit-binary project; it has not changed.

Explicit LeaCorePath or LeaViewPath prevents automatic source selection;
`LeaBuildFromSource=false` also retains binary mode. Supply both matching DLL
paths for full view capture. The core DLL must exist in binary mode. Source
mode requires net48; the wrong-framework guard was not tested in this increment.
Do not use prototypes or mix products from different candidates.

The tested output adds Lea.Print SHA256
414E1CD817F060758978C049595CC7E7BB15EF5270DC4053B40AFCCC2D47923B.
98source members build six products without seeded Lea binaries. Existing pinned
test executables, not newly compiled harnesses, pass on these new runtime DLLs:
capture/export plus102range/device/ticket and40baseline/46friendly/250live-image
checks. NTFS runtime staging and original120sbuild/30stest/5scleanup limits remain.
No print job was submitted. Audit24A2A32A; SDK_PRINT.md in LeaForge has exact pins.
This is not physical-printer, full security/script-worker or release qualification.
