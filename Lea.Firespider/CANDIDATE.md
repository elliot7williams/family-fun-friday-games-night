# Separate Firespider Candidate

Milestone 145 adds a reproducible migrated host candidate. It does not install
over `L:\Firespider Internet` or make Lea a complete modern-web engine. The
clean adapters here remain the source of record; the live host has unrelated
in-progress changes that must be retained.

Milestone 146 finishes native-event Promise checkpoints and adds bounded
`queueMicrotask` support to that optional module. The current builder emits
146 manifests; the packer retains support for archived 145 manifests. See
[microtask verification and limits](../LeaForge/MICROTASKS.md).
The 146 archives remain experimental: an earlier fresh extracted host hits a
script-worker startup deadline. The v7 follow-up adds asynchronous native
launch, Stop-safe handoff, coordinated file ACLs and bounded diagnostics; all
450 source scripting, 43 host and six actual-EXE startup checks pass. A first
standalone extraction still fails cancellation recovery without a typed result;
an updated diagnostic test package passes all 450 checks. The v7 host-source ZIP
independently rebuilds and passes the scripting case with manifest-verified
harness fixtures. These passes do not explain the intermittent failure or
establish cold-start stability. See [startup evidence](../LeaForge/STARTUP_DIAGNOSTICS.md). Do not
install them as a finished replacement for the preserved live browser.

The separate `Lea-146-timers-v8` follow-up compiles with bounded timeout/interval
support and worker protocol version 3; parent and worker must be upgraded together.
Its first scripting gate fails before the timer assertion, with a hello-phase
startup timeout (5,026 ms wall, 1,640 ms CPU). A traced diagnostic run also fails
(5,020 ms wall, 765 ms CPU), reporting VM initialization but not DOM completion.
This narrows the observed phase, not the cause. The v8 full host/startup matrices,
extracted runtime and independent host-source rebuild are not verified. Do not
transfer v7's passing gates to this build or install it over the live browser.
See [timer implementation and evidence](../LeaForge/TIMERS.md).

The `Lea-146-bootstrap-v9` candidate simplifies fixed-list DOM setup and adds
opt-in parsing/initialization diagnostics. Its scripting case and six actual-EXE
startup scenarios pass, but normal complete source runs still fail fresh-worker
startup in microtask tests. Its full 43-case host matrix, extracted packages and
independent host-source rebuild are not verified, and no release ZIP is published
from that failed source gate. See [bootstrap evidence](../LeaForge/BOOTSTRAP.md).

The `Lea-146-trace-v10` diagnostic candidate uses the production parser in both
trace modes and retains compact startup markers without timing enabled. Its
scripting case and six actual-EXE startup scenarios pass, as does the final
630-check normal source suite. Earlier failures in the same round remain
unresolved; the full 43-case host matrix, extracted packages and independent
source rebuild are not verified for v10. No new release ZIP is published.
See [tracing evidence and limits](../LeaForge/STARTUP_TRACE.md).

The `Lea-146-attributes-v11` candidate adds live ID/class metadata through bounded
typed `id/cn` mutations and optional `hasAttribute`. All 703 source scripting and
complete core regressions pass. This candidate passes all 43 host and six actual
EXE startup cases. Three audited experimental archives are retained; a fresh
standalone extraction passes all 703 checks, and the extracted host plus an
independent rebuild of the host-source ZIP pass their scripting cases. The latter
uses 348 manifest-verified fixture/license files from the runtime package for the
harness. Full host/startup matrices are not repeated on those extractions/rebuild.
Earlier cold-start failures remain unexplained, and the live host stays untouched.
See [attribute evidence and package hashes](../LeaForge/ATTRIBUTES.md).

The `Lea-146-class-v12` follow-up shares ASCII class/word token scanning,
including form feed, and reduces small-compound query allocation without a
retained cache or new dependency. Complete core and all 713 source/extracted
standalone scripting checks pass. Separate v12 passes all 43 host and six EXE
startup cases. Three ZIPs pass inventory/length/hash/current-snapshot/extraction
audits; extracted and independently rebuilt host-source scripting cases pass
with 19 verified dependency records and 348 copied fixture/license files.
The full matrices are not repeated on those extractions/rebuild. The required
bundle grows 512 bytes to 742,912. A first source core run fails delayed-decoder
admission; focused pictures and the complete rerun pass without explaining the
failure. Startup reliability remains work and the installed host is unchanged.
See [class-token measurements and artifact hashes](../LeaForge/CLASS_TOKENS.md).

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
return to idle even with synchronized scrolling disabled, and checks live ID/class
reflection with retained node identity and present empty attributes.

See `../LeaForge/FIRESPIDER_SCRIPT_HOST.md` for the final verified artifacts
and results. Do not infer success for that matrix from a build alone.
