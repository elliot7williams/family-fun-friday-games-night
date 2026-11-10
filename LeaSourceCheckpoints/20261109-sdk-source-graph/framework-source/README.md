# Lea Framework Source Build

This development layout builds four mature libraries: Lea.Engine, Lea.Win,
Lea.Script and Lea.Script.Win. It preserves the current signed-relative-font
sources and private-binding bootstrap. It is not the complete SDK or a release.

Use Windows, the .NET Framework 4.8 targeting pack and a C#12-capable compiler.
The tested local layout is:

`I:\LeaBuildStorage\sdk-source-graph-20261109-v2\source`

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

Verified in Release: original missing-dependency failure; fresh automatic
four-library source build; explicit binary mode with poisoned core source;
14rendering/selection embedding checks at720/360px;14metadata/reference and
adapter attach/detach checks;28signed-relative-font model checks.

Not verified here: page JavaScript execution, isolated/native worker deployment,
image-worker or printing source builds, Debug, wrong-framework build rejection,
full scripting/security/lifecycle campaign, Firespider adoption, clean-machine
installation or all-sites compatibility. Startup failures remain unresolved.
Existing target aliases in the full SDK are unchanged; this four-library source
checkpoint is not a substitute for its remaining components or release gates.
