# Separate Firespider Candidate

Milestone 145 adds a reproducible migrated host candidate. It does not install
over `L:\Firespider Internet` or make Lea a complete modern-web engine. The
clean adapters here remain the source of record; the live host has unrelated
in-progress changes that must be retained.

## Build

Build the current Lea solution in Release, then:

```powershell
.\Lea.Build\Build-FirespiderCandidate.ps1
.\Lea.Build\Build-FirespiderCandidate.ps1 -NoScript
```

Use `-Source` for another live project location and `-Out` for a new, separate
candidate directory. The output must not exist or be inside the live source
directory. This machine uses the installed .NET Framework MSBuild toolset and
4.8 reference assemblies to handle the legacy binary resources correctly.
The script preserves failed candidates for diagnosis; it does not delete or
overwrite existing candidates.

Each candidate contains a Visual Studio-compatible `Firespider Internet.vbproj`,
the exact declared source/resource inputs, clean `FirespiderLea.vb` and
`FirespiderSplit.vb` overlays, Lea dependencies and a `runtime` directory.
Only the copied project/configuration is migrated to .NET Framework 4.8.
Its `LeaScriptEnabled` property records the chosen optional-module mode.
The script also sets that property explicitly for its build invocation.

Historic unused Flash/Director references/imports are removed from the copied
project only after every compiled VB input is checked for those namespace
identifiers. Any use stops the automatic removal. Framework-provided references
are not copied into the runtime as redundant DLLs. Paths escaping the live
project are rejected, and every copied source input is immediately checked
against its source SHA256.

`source-manifest.json` records original input hashes, final overlay/project/
configuration hashes, removed COM entries, framework-provided references and
the selected target/module mode. The original source project hash differs from
the transformed overlay hash by design. Third-party scripting licenses are
included in enabled runtimes. Worker launch requires a local NTFS/ACL-capable
volume; ZIP distribution storage alone is not a launch location.

## Packages

After verification, package a candidate with:

```powershell
.\Lea.Build\Pack-FirespiderCandidate.ps1 -Candidate 'C:\path\candidate' -Out 'C:\path\packages'
```

The separate runtime and buildable host-source ZIPs include a per-file
`package-manifest.json`. The source ZIP preserves the final transformed
project/configuration, original declared inputs, clean adapter overlays and
compiled Lea dependencies. It is a host source snapshot, not the complete
Lea C# source archive; retain `Backup-Lea.ps1` output as well. The packer
rejects changed snapshot inputs and existing output ZIPs. Open the extracted
VB project in Visual Studio to build it with .NET Framework 4.8 targeting
support installed.

## Behavior

The enabled candidate attaches the optional module to regular Lea tabs,
resumed/new tabs and both split panes. Each pane/tab owns an independent
isolated session. Active workers prevent automatic hibernation, including
pure JavaScript variables that have not changed the DOM. Closing, navigation,
Stop and permission revocation use the native session-retirement path.
Bounded script errors appear in the appropriate status bar, not JIT dialogs.

Script-free local pages start no JavaScript worker. Inert template blocks do
not change that decision. Authored handlers, classic inline blocks and explicit
`JsA` calls initialize one when needed; delayed supplied code receives the
already-requested ready/load lifecycle. Unsupported event types and foreign
nodes are rejected even while the interpreter is dormant.

The host candidate uses a 5,000 ms execution ceiling. The optional module and
standalone demo retain their 2,000 ms default. A ceiling is not an observed
latency or performance guarantee. Real message-loop integration is necessary:
the scripted host harness runs under `Application.Run` and awaits calls rather
than synchronously pumping nested script waits.

HTTP/HTTPS/UNC script execution remains disabled. Ordinary supported remote
HTML navigation/forms still use Lea's native actions and host policy; origin
refusal is not a script-created `preventDefault`. External scripts, general
event-loop semantics and broader DOM/CSSOM/site compatibility remain work.
Explicit legacy fallback and advanced legacy-only host commands still exist;
this does not make the entire Firespider application Lea-only.

The core now loads `about:blank` internally through both navigation paths,
preserving its optional query/fragment and avoiding the HTTP cache. Other
`about` documents and host-bearing forms remain unsupported. This follows
the [HTML about:blank URL matching definition](https://html.spec.whatwg.org/multipage/urls-and-fetching.html#about:blank)
for this internal document path, not the full inherited-origin/iframe model.

## Verify

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -File .\Lea.Build\Test-FirespiderBoot.ps1 -Root 'C:\path\candidate\runtime' -Case all
powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -File .\Lea.Build\Test-FirespiderStartup.ps1 -Root 'C:\path\candidate\runtime' -Scenario all
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
.\Lea.Tests\bin\Release\Lea.Tests.exe . --core
```

The Boot runner retains its 42 existing cases and adds `script` automatically
when the optional module is present. The scripted case checks the actual
compiled target, dormant startup, pure-state hibernation protection, lifecycle,
native click/default cancellation, error status/recovery, split session
independence/disposal, tab cleanup and remote-origin refusal. It uses a real
WinForms message loop, as does the rest of the Boot harness. The EXE startup
runner copies optional runtime files into its isolated lazy-restore scenarios
as well. The scripted case also checks that split-view comparison controls
return to idle even with synchronized scrolling disabled.

See `../LeaForge/FIRESPIDER_SCRIPT_HOST.md` for the final verified artifacts
and results. Do not infer success for that matrix from a build alone.
