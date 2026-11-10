# Lea Aggregate SDK Source Candidate

This development layout combines mature core, view, isolated image decoder,
printing, JavaScript parent/WinForms adapter, native-worker sources and two
consumer samples. It is not a complete or release-qualified SDK. No Chromium or
WebView is used. Existing binary/package results do not qualify new outputs.

Current embedding template also selects sibling source core/view/image projects
when no explicit LeaSdkBin is supplied. Its decoder is a non-assembly project
reference copied beside the executable; explicit binary mode copies the decoder
from LeaSdkBin instead. SOURCE_GRAPH.md describes the separately tested current
layout and NTFS runtime requirement. This is not qualification of the complete
aggregate SDK, scripting worker or Firespider host.

Use Windows, the .NET Framework4.8 targeting pack and a C#12-capable compiler.
The worker targets net8.0/win-x64, runtime8.0.33 with roll-forward disabled.
Projects use relative references; no original project/evidence drive is needed.
New net48 script-project templates select core/view project references when
those source projects are present. The adapter project can then build all four
Framework libraries directly, without prebuilt Lea DLLs. Set
`LeaBuildFromSource=false` to retain explicit core/view binary references;
the existing parent-script project reference remains. This changes the source
dependency graph, not script permission, worker deployment or startup budgets.
Package-scoped Directory.Build.targets enables deterministic compilation and maps
source/debug paths to /_/lea/. This affects build metadata and debug locations,
not execution limits or browser compatibility. Reproducibility still requires the
same compiler, references and native toolchain; a new build needs runtime checks.

`SDK.Build.proj /t:BuildLibraries` builds the Framework libraries and image EXE.
`/t:BuildWorkerIl` separately builds the worker IL into worker-il/Release.
That IL output is NOT an executable native runtime for the confined worker.
The portable native step requires Windows x64 PowerShell7.4+, authenticated
Microsoft crossgen2 8.0.32 tools and the installed .NET8.0.33 runtime references.
There is no download, version substitution or IL fallback. Record your local
tool/reference byte identities only AFTER authenticating their origin:

```powershell
pwsh -NoProfile -File .\New-NativeToolchain.ps1 -cc C:\Tools\crossgen2\tools -rt "$env:ProgramFiles\dotnet\shared\Microsoft.NETCore.App\8.0.33" -o .\native-toolchain.json
$tc = (Resolve-Path .\native-toolchain.json).Path
$th = (Get-FileHash $tc).Hash
MSBuild SDK.Build.proj /t:Bn /m:1 /nodeReuse:false /p:Configuration=Release "/p:LeaNativeToolchain=$tc" "/p:LeaNativeToolchainSha256=$th"
```

The C:\Tools path is an example local installation, not a bundled toolchain.
The manifest pins every compiler file and runtime DLL reference; recording hashes
does not authenticate their origin. Keep the manifest local and pass its trusted
SHA256. Bn builds IL, then compiles and checks the three x64 ReadyToRun images in
the exact six-file worker closure. Cached outputs are reused only if all input
and output hashes still match. Changed inputs require a fresh ScriptWorker output
or deliberately cleaning development build outputs; old output is not overwritten.
LeaNativeOutput and LeaWorkerIlDirectory support explicit fresh build directories.
CompileWorkerNative uses already-built IL, for controlled build orchestration.
Do not disable native execution mitigations or copy unqualified files to bypass
this step. Compilation/header checks alone are not runtime/security qualification.

After native staging, `/t:BuildSamples` builds the text/selection consumer and
the opt-in local JavaScript consumer. The latter copies its six-file worker
closure and host EXE configuration. It defaults scripting permission off as well
as adapter off; detachment alone is not a scripting-disable switch. HTTP/HTTPS/
UNC/mapped-network navigation is not supported by this local sample.

Short target aliases: Bl libraries, Bi worker IL, Bn native worker, Bs samples,
Ba all. Ba invokes BuildAll (libraries, worker IL/native and both samples) with
the same toolchain properties. Default target remains BuildLibraries; no native
tool is silently selected. Source build/consumer/clean-machine qualification is
candidate-specific and does not follow from these target definitions.

Framework dependencies are in lib. The worker uses DIFFERENT Jint/Acornima
binaries exclusively in lib/worker. Do not merge these folders or substitute
versions. Source/dependency provenance and projection changes are recorded in
source-manifest.json. Dependency notices/redistribution review are pending;
presence here is not a redistribution license assertion.

Pending: qualified native source-build/deployment workflow, combined sample and
component regressions, full JavaScript/security/lifecycle suite, startup diagnosis,
complete notices/package verification, clean-machine and Firespider migration.
All six final release gates remain open. This is not a GitHub backup or a claim
that all websites work. Compression affects archive size, not execution speed.
