# Firespider event receiver package verification

Separate experimental v28 now passes fresh-package and archived-source runtime
checks using the production dependencies from
`0b94a79c9264b9485aad751480c042ad009399e8` and the host harness at
`d35fbde674a843b57219ee06aa88108fa770ca69`. No production code, public aliases,
startup deadlines, isolation limits or installed Firespider files change here.
The earlier candidate and negative-control results remain in
[HOST_EVENT_RECEIVERS.md](HOST_EVENT_RECEIVERS.md).

## Verified gates

| Build | Host cases | Actual EXE startup scenarios |
| --- | --- | --- |
| Fresh runtime ZIP extraction | All 43 pass | All six pass |
| Independent archived-source build | Complete script case passes | All six pass |

The fresh extraction's one complete host attempt runs from
`2026-10-09T01:19:18.4798757Z` to `2026-10-09T01:26:52.2445955Z`.
The independent script attempt runs from `2026-10-09T01:31:20.4403518Z` to
`2026-10-09T01:32:16.6191077Z`. Neither has startup observation or external timing
trace, and no failing case is retried. Both wrappers also pass the separate
14 clipboard and 12 observation contracts. The latter exercise the observation
helpers, not browser workers.

The script cases cover first demand, inline navigation, both receiver checks,
borrowed native event dispatch, timers, live DOM operations, deliberate script
errors and fresh-realm recovery, split isolation/disposal, tab cleanup and
remote-origin refusal. All four fresh main/split captures are visually inspected:
nonblank rendered pages, a counter of one, and independent split-pane values of
one and zero. The independent build is not claimed to pass the other 42 host
cases. The full extraction matrix includes explicit legacy-fallback cases;
it does not establish complete removal of legacy-only functionality.

All six actual-EXE scenarios pass in each runtime: clean, restored, locked,
oversized, lazy-background and lazy-all. Each remains responsive for at least
eight seconds, without the checked invalid-cast/JIT/crash-recovery dialogs or
MSHTML module. Lazy-all also successfully resumes its selected deferred tab.
Successful cases are streamed into retained JSONL, preserving partial results
if a later case fails. Windows PowerShell STA executes the trusted helper source
without an execution-policy change. Retained stderr is module-import progress
CLIXML; exit codes and structured case records confirm success.

| Scenario | Extracted ready / observed ms | Rebuilt ready / observed ms |
| --- | ---: | ---: |
| Clean | 5,968 / 8,181 | 5,112 / 8,379 |
| Restored | 6,747 / 8,275 | 6,980 / 8,300 |
| Locked | 5,786 / 8,140 | 6,833 / 8,148 |
| Oversized | 5,512 / 8,162 | 5,654 / 8,165 |
| Lazy background | 6,966 / 8,244 | 7,774 / 8,170 |
| Lazy all | 4,988 / 9,054 | 4,999 / 9,415 |

These are surrounding UI-readiness observations, not native worker handshake
measurements. Start-page fixtures can leave the optional interpreter dormant.
The script first-demand/split observations are 5,675/6,354 ms for extraction and
5,469/5,884 ms for rebuild. Their broader stopwatches include parent overhead;
they neither measure nor change the native 5,000 ms startup deadline. Caches,
machine load and storage are uncontrolled. No cold-start, speedup or memory
improvement is proved.

## Archive integrity and independent compilation

Runtime packaging retains 375 payload files plus its manifest. Host-source
packaging retains 103 payload files plus its manifest: the 90 final build inputs,
source manifest and 12 dependency-license files. Each manifest, ZIP stream and
extracted payload matches the audited staged inputs by exact path, length and
SHA256. Inventory checks reject duplicate/case-colliding or unsafe paths and
symlink entries. Frozen archive copies match the originals.

The archived VB project compiles with the installed Framework64 v4 MSBuild and
.NET Framework 4.8 reference assemblies, without restoring packages or reading
the original/live host source. Output and intermediates are outside the extracted
source. All 104 extracted source files remain unchanged, with all 19 production
dependencies matching the tested engine bytes.

The tested build uses these explicit flags, not the project's default publishing
configuration:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" `
  'H:\Lea Builds\Lea-146-event-receivers-source-extracted-v28\Firespider Internet.vbproj' `
  /nologo /tv:4.0 /t:Rebuild /verbosity:minimal /nodeReuse:false `
  '/p:Configuration=Release;TargetFrameworkVersion=v4.8;LeaScriptEnabled=true;OutputPath=H:\Lea Builds\Lea-146-event-receivers-independent-v28\runtime\;BaseIntermediateOutputPath=H:\Lea Builds\Lea-146-event-receivers-independent-v28\obj\;IntermediateOutputPath=H:\Lea Builds\Lea-146-event-receivers-independent-v28\obj\;GenerateManifests=false;AutoGenerateBindingRedirects=true'
```

Compilation exits zero in 9,493 ms. This is not a default Visual Studio build
or reproducible-PE claim. Both host EXEs are 1,134,080 bytes, but their hashes
differ. For independent runtime testing, byte-verified fixtures/assets come from
the runtime ZIP; the independently compiled EXE/config/start page and 19
dependencies are overlaid. The stale package manifest is not copied into that
different runtime. Those fixtures are not inputs required to compile the source
project. Complete Lea engine source is preserved separately, not claimed to be
inside the host-source ZIP's frozen dependency DLLs.

Native execution stays on ACL-capable NTFS: L: for the fresh extraction and C:
for the independent runtime. H: holds compilation output, archives and evidence,
not native AppContainer execution. Low C:/L:/F: space remains a constraint; no
user data or failed evidence is removed. Relocation is not a demonstrated fix.

## Test artifacts

Packages are under `H:/Lea Builds/Lea-146-event-receivers-packages-v28`.
Runtime ZIP `Firespider-Lea-146-script-runtime_2026-10-08_21-12-43.zip`:
3,708,237 bytes, 376 entries, SHA256
`D9EE05C59A6E608C9E2313E077C990A28E9918F7107B73F07A2DFBF8D0279CB8`.

Host-source ZIP `Firespider-Lea-146-script-host-source_2026-10-08_21-12-43.zip`:
3,351,031 bytes, 104 entries, SHA256
`DC5CA7A465D2CBF67FE108FF99D19469E415153805D3E798383EEC8CBC9A6906`.

Extracted host EXE SHA256:
`8A93AEA093B65F68C397553EA75EBAF3ADA044FEA3F38F80E02B8ABA00E71A08`.
Independent host EXE SHA256:
`7B04BD7BF330795CF2357C7DC8A109E722ECD26550E2E5DC234449E39AB887BF`.

The tested package EXE is
`L:/Lea Builds/Lea-146-event-receivers-extracted-v28/Firespider Internet 19.exe`.
Keep its accompanying files together. This is an experimental test package,
not a stable browser release or installed replacement.

Evidence is under
`H:/Lea Evidence/Lea-146-event-receivers-packages-2026-10-08`: both ZIPs, exact
post-test runtime inventories and frozen copies, unchanged source-input audit,
build/host/startup logs, streamed cases, inspected captures, harness source,
complete Lea source backup and checkpoint bundle. The final checkpoint audit
checks all 19 production files in six roots, the original 67 host inputs and
three installed binaries. Test harness EXEs are preserved separately from the
distribution payload.

## Remaining work

The intermittent five-second native worker startup timeout remains unresolved;
these passes do not erase v24/v26 or other retained failures. No new full engine
source suite is run here; the 1,342-check result belongs to the exact preceding
source checkpoint and unchanged production bytes. No compressed-size benefit
is equated with faster execution.

Startup reliability, safe remote-script/resource policy, remaining legacy-only
commands and broader website compatibility still gate everyday use. Hardening
milestone 146 stays open; broader readiness stays about 65%, a practical estimate
rather than a standards score. Installed Firespider remains untouched.
