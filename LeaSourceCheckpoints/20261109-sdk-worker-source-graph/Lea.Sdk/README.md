# Lea Core/View SDK Candidate

Current unified source build instructions are in CURRENT_BUILD.md. The latest
128-member development export builds core/view/image/print/Framework scripting,
the separate native JavaScript worker and both samples. Actual outputs passed
14text/17script/25worker fixtures; full scripting/startup/browser qualification
remains open. Earlier component notes below retain their historical scope.

## Combined Source Candidate

`SDK.Full.Build.proj` and `SDK_FULL.md` define the newer development layout:
core/view/image/print, Framework scripting, separate modern worker dependencies,
and text/script consumer projects. Prepare-FullSdk stages the source tree;
Test-FullSdkLibraries and Test-FullSdkNativeWorker build its actual projects,
compile/inspect the x64 ReadyToRun worker, and exercise the new outputs.
Test-FullSdkWorkerGate checks native worker contracts; Audit-FullSdk independently
checks the receipts. See LeaForge/SDK_FULL.md for exact identities and boundaries.
These workspace tools are not yet portable build tooling shipped inside the SDK.
Combined focused passes do not close the full regression, notices or release gates.

## JavaScript Consumer Sample

`ScriptEmbeddingSample.csproj` is a separate net48/C#12 WinForms consumer using
the mature SDK parent/adapter, not either rejected I/O experiment. Its
`LeaSdkBin` property points to the matched core/view/script libraries, framework
dependencies and exact six-file `ScriptWorker` subdirectory. Default relative
layout is `Sample` beside `bin/Release`; `ScriptHost.config` is the EXE AppConfig.
Do not mix parent framework Jint/Acornima with the worker's modern dependencies.

The sample opens local `script-embedding.html`, or one local file argument.
Scripting defaults OFF through both `view.Je=false` and no native binding.
The Local Scripts toggle enables `W.On(view)` and `view.Je=true`; opt-out first
sets permission false, then calls `W.Off(view)`. Adapter detachment alone does
not disable the engine's legacy script subset. Navigation is local-file only;
HTTP/HTTPS/UNC and mapped-network drive targets are rejected.

Owned project build and fresh NTFS execution passed17 focused checks, including
native execution, DOM/API/rendered output and worker/ACL cleanup. Automated mode
is `Lea.ScriptEmbeddingSample.exe --test evidence-directory`; test directories
must be managed by the owned runner, not shared with a running browser. See
`LeaForge/SDK_SCRIPT_SAMPLE.md` for candidate hashes and limits. This project is
not yet included in the older aggregate SDK ZIP and is not a complete SDK.

## Embedding Sample

EmbeddingSample.csproj is a separate WinForms consumer. Set its `LeaSdkBin`
property to the directory containing the rebuilt Lea.Engine.dll and Lea.Win.dll.
It does not silently rebuild or substitute the SDK libraries. Its default
relative path assumes a Sample folder beside the SDK's bin folder.

The ordinary EXE displays the bundled local embedding.html. Automated mode is
`Lea.EmbeddingSample.exe --test embedding.html evidence-directory`; it checks
computed relative size, rendering and partial selection at720/360px and saves
four PNGs. It does not launch JavaScript/image workers or submit print jobs.

2026-10-31 SDKv3's exact rebuilt DLLs passed all14 sample checks with normal owned
cleanup. The original source projects were independently built with SDK8.0.427.
This establishes a local-text embedding only, not the complete SDK/runtime gates.

This source package stages the mature relative-font engine/view sources, not the
older root prototypes. `source-manifest.json` records each file hash and the
source candidate's evidence. These are ordinary .NET Framework 4.8 projects with
relative project references; no Chromium or WebView is involved.

Open `Lea.Win/Lea.Win.csproj` in Visual Studio with .NET desktop development and
the .NET Framework 4.8 targeting pack and a Roslyn compiler supporting C# 12,
matching the qualified view recipe. The legacy Windows Framework MSBuild/csc
cannot compile this view's exception filters. Alternatively, from a Developer Command
Prompt, run:

```bat
MSBuild Lea.Win\Lea.Win.csproj /t:Rebuild /p:Configuration=Release /m:1 /nodeReuse:false
```

Build products go to `bin/Release`. Add a project reference to Lea.Win from a
WinForms host. On its UI thread, a minimal embedding is:

```csharp
var view = new Lea.Win.V { Dock = System.Windows.Forms.DockStyle.Fill };
Controls.Add(view);
view.Go(localHtmlPath);
```

The package is an intermediate source-delivery candidate, not a completed SDK.
Its projects still require their own compile, embedding and runtime verification;
older passing binaries do not qualify rebuilt SDK binaries. The original sources
were qualified as core4DA8/viewCA51, but project builds may produce different
identities and framework metadata.

JavaScript parent/adapter/worker, isolated image worker, print library, dependency
notices, full host sample and deployment validation remain to be incorporated.
Do not substitute an unrestricted image decoder or enable remote scripting to
work around missing components. The view's image-worker path is not certified
by a text-only embedding. Legacy UIA/startup reliability and broad website
compatibility remain unresolved. This folder is not a ready replacement for the
installed Firespider browser, a complete source SDK or a GitHub backup.
