# Current Lea Source SDK

2026-11-10 current B117/C83 source SDK now passes original3412/all89 scripting
checks in279932ms/retire657,300s/5s unchanged,65runtime/input bytes+ACL stable,
trace restored. Receipt77DFD17A/auditD4F27CA1. LeaForge/SDK_SOURCE_FULL_SUITE.md
exact pins; matched Firespider qualification remains next, not a final release.

2026-11-10 current candidate source export:
`I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source`.
128source members/eight dependencies, no seeded Lea products, only E.cs/P.cs/V.cs
changed from the preceding export. CoreB1175488/viewC83E47CC, six framework
products and native worker/source samples built.56consumer/worker +12model+
17view+7policy checks completed, auditsFD6BBBC7/AB308FD6. The storage-stopped
partial result remains unchanged; only its unstarted policy step resumed after
reserve recovery. LeaForge/SDK_MEMORY_POLICY.md exact pins/limits. Not full3412,
browser/print/package/security or release qualification; next exact full suite.

Preceding2026-11-09 development checkpoint, not a release-qualified browser/SDK.

The preceding source export is:
`I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source`.
It contains128source/project/resource/build/documentation members and eight
pinned third-party dependency DLLs. No prebuilt Lea product was seeded.
The frozen SOURCE_GRAPH.md/SDK_FULL.md within that export describe the preceding
print milestone and generic SDK tooling; this document records the newer build.

Open the individual net48 projects in Visual Studio, or build these SDK targets
with Windows, a C#12-capable compiler and the .NET Framework4.8 targeting pack:

```powershell
dotnet msbuild SDK.Build.proj /t:Bl /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false
dotnet msbuild SDK.Build.proj /t:Bi /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false
```

Bl builds core/view/image/print/parent/adapter. Bi builds net8.0/win-x64 worker
IL into worker-il/Release, not the confined native runtime directory. The worker
uses separate lib/worker dependencies; Framework dependencies stay in lib.
Offline restore succeeded with installed8.0.33 reference/app-host packs and
cleared NuGet sources. There is no worker IL fallback, runtime roll-forward or
automatic native compiler download.

Authenticate Microsoft's crossgen2 8.0.32 tools before recording their hashes.
The tested compiler's retained signed-package receipt was revalidated, including
all recorded tools. Installed .NET8.0.33 runtime references were inventoried.
The manifest generator records byte identities; it does not authenticate origin:

```powershell
pwsh -NoProfile -File New-NativeToolchain.ps1 -cc C:\Tools\crossgen2\tools -rt "$env:ProgramFiles\dotnet\shared\Microsoft.NETCore.App\8.0.33" -o .\native-toolchain.json
$tc = (Resolve-Path .\native-toolchain.json).Path
$th = (Get-FileHash $tc).Hash
dotnet msbuild SDK.Build.proj /t:CompileWorkerNative /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false "/p:LeaNativeToolchain=$tc" "/p:LeaNativeToolchainSha256=$th"
dotnet msbuild SDK.Build.proj /t:Bs /m:1 /nodeReuse:false /p:Configuration=Release /p:UseSharedCompilation=false
```

CompileWorkerNative publishes exactly six files under bin/Release/ScriptWorker,
with native-header checks for worker/Jint/Acornima and a sibling build receipt.
Bs builds both consumer samples. Ba/Bn remain existing aggregate aliases but
this increment tested the four phases separately, not a new one-command Ba run.
Release only was tested. Compiler120s, focused runtime30s and cleanup5s limits
were retained. Build TEMP, CLI home and NuGet cache were redirected to I: only
for the owned build process; original process environment was restored.

Building on I: exFAT is supported; sandbox execution requires NTFS runtime
staging. The current tested script sample is:
`C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\continuity-evidence\sdk-worker-source-graph-v1\script\Lea.ScriptEmbeddingSample.exe`.
Its scripting toggle starts off and this sample permits local files only.
Do not treat it as an unrestricted website browser or run its sandbox from I:.
Application aliases E/V/Js/W.On/W.Off remain unchanged.

Actual new outputs pass14text,17script-sample and25isolated-worker checks.
They cover local DOM updates, separate process/isolation flags, original
2000ms execution/2500ms CPU/128MiB worker policy, bounded code/thread affinity,
no CLR/network API, persistent realm without idle parent dispatcher, original
handle retirement and exact six-file ACL restoration. These are bounded fixtures,
not universal security. A subsequent matched full campaign now also passes all
3412checks/89phases in283861ms, original300souter/5scleanup,65runtime byte/ACL
and trace restoration verified. Independent audit322CFA7E; exact records and
test-source binding are in LeaForge/SDK_SOURCE_FULL_SUITE.md.

Worker DLL SHA256F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0
matches the retained original; all six framework/image/print products also match
the preceding source build. New combined runtime validation still matters:
older full-suite passes were not transferred to the11B1core combination; it now
has its own passing campaign. LeaForge/SDK_CURRENT_SOURCE.md records exact
receipts. Historical startup failures, matched Firespider adoption, broad
site/security compatibility, notices and
clean-machine delivery remain unfinished. All six release gates stay OPEN.

## Firespider Candidate

The current SDK also builds preserved Firespider19 source without source changes.
Project: `I:\LeaBuildStorage\current-firespider-build-20261109-v1\source\Firespider Internet.vbproj`.
EXE9513A0BD passes3startup smokes and the original15workflow cases, including
six actual EXE profiles, with stable400-member runtime bytes/ACLs and no legacy
engine module or forced cleanup. CURRENT_SOURCE_HOST.md in LeaForge records
exact pins, NTFS test executable, audit correction and remaining print/package/
reliability scope. The live L: project/installed browser has not been overwritten.
