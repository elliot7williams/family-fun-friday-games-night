# Firespider Element receiver package checks

Experimental v30 now passes a complete corrected-harness candidate matrix and
the fresh runtime package gates. The independently compiled archived-source
runtime fails its first script-worker startup. The overall release is therefore
not verified. Native startup reliability remains open; installed Firespider,
production engine bytes, aliases, security boundaries and deadlines are unchanged.

## Gate results

| Build | Host cases | Actual EXE startup scenarios |
| --- | --- | --- |
| Existing v30 candidate, final harness B | All 43 pass | Six passed at the preceding host checkpoint |
| Fresh runtime ZIP extraction | All 43 pass | All six pass in this package checkpoint |
| Independent archived-source build | Script case fails before receiver assertions | Held after the script failure |

The corrected candidate attempt runs from `2026-10-09T03:07:21.8287071Z` to
`2026-10-09T03:15:17.9506255Z`. The extracted runtime attempt runs from
`2026-10-09T03:21:44.8237418Z` to `2026-10-09T03:29:47.6838950Z`. Each passes
all 43 ordered cases in one run, including both Element receiver realm checks,
borrowed field access/setter behavior, fixture restoration, script-error recovery,
split isolation/disposal and remote-origin refusal. Clipboard (14 checks) and
observation (12 checks) contracts also pass. The full matrix includes explicit
legacy-fallback cases, not complete removal of legacy-only functionality.

No startup observer or external timing trace is enabled, and no failed runtime
attempt is retried. These are integration/package checks, not a new full engine
source-suite run. The 1,385-check result belongs to the preceding Element source
checkpoint. The earlier harness A fixture failure and corrected B script result
remain preserved separately in [HOST_ELEMENT_RECEIVERS.md](HOST_ELEMENT_RECEIVERS.md).

The fresh EXE stays responsive in all six eight-second startup checks, without
the checked invalid-cast/JIT/crash-recovery dialogs or an MSHTML module. Lazy-all
also resumes its deferred selected tab. Windows PowerShell STA runs the trusted
helper source without changing execution policy; partial results are streamed.

| Scenario | Fresh ready / observed ms |
| --- | ---: |
| Clean | 4,970 / 8,258 |
| Restored | 6,527 / 8,132 |
| Locked | 6,623 / 8,133 |
| Oversized | 5,645 / 8,239 |
| Lazy background | 5,013 / 8,354 |
| Lazy all | 3,583 / 8,915 |

Start-page checks can leave the optional interpreter dormant. Surrounding
script first-demand/split stopwatches are 5,228/5,789 ms on the candidate and
5,177/6,972 ms on extraction. These include host overhead, not just the native
handshake. They neither measure nor change its 5,000 ms budget. No controlled
cold-cache, speed or memory improvement is demonstrated.

Candidate and fresh main/split captures are visually inspected: nonblank pages,
retained `Elliot` editor value and independent split counters of one and zero.

## Identical harness reuse

`Lea.Build/Test-FirespiderBoot.ps1` accepts optional `-HarnessSHA256` (`-hh`).
With it, the helper requires an existing boot EXE with the exact 64-digit SHA256
before contracts or browser execution, and does not recompile it. Without it,
the existing compilation path remains; that default branch is not separately
rerun in this checkpoint. Matrix cases, contracts, cleanup and timeouts are unchanged.

Short and empty hashes, a missing harness and a mismatched existing harness all
reject before executing the harness. No binary is created or modified by those
guards. Lowercase input and the short alias work in the passing complete matrices.
The initial external guard wrapper's exception-type assertion error is retained;
its correction repeats guard checks only, not native browser startup.

The exact final B harness is unchanged before/after each runtime attempt:
`274D860F08BA17B8983EDF49736139506DE51470940C6C7F6BC6DDC3EBC0B7BD`.

## Archives and independent build

Packages are under `H:/Lea Builds/Lea-146-element-receivers-packages-v30`.
Runtime `Firespider-Lea-146-script-runtime_2026-10-08_23-15-54.zip`:
3,708,613 bytes, 375 payload files plus manifest, SHA256
`F6D0C79BCF0A697E6B24CF6B290B6FF739B7E2AE1714066B952CA44A0E3815EC`.

Host source `Firespider-Lea-146-script-host-source_2026-10-08_23-15-54.zip`:
3,351,264 bytes, 103 payload files plus manifest, SHA256
`04352B2A4472D7C9B9027437CE956330054483EAEAF1127F2B63B3BD8E905309`.

Both archives pass exact staged-input/manifest/stream/extraction length and
SHA256 checks, rejecting unsafe paths, duplicate/case-colliding names and symlinks.
Frozen archive copies match. The host archive contains 90 final build inputs,
the source manifest and 12 dependency licenses, plus the package manifest.
Complete Lea engine source is backed up separately; frozen dependency DLLs in
the host-source ZIP are not claimed to contain that source.

All 104 extracted host-source files stay unchanged after the offline Framework
build, with output and intermediates outside the source. No restore or original
live-host source is needed. The explicit tested command is:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" `
  'H:\Lea Builds\Lea-146-element-receivers-source-extracted-v30\Firespider Internet.vbproj' `
  /nologo /tv:4.0 /t:Rebuild /verbosity:minimal /nodeReuse:false /m:1 `
  '/p:Configuration=Release;TargetFrameworkVersion=v4.8;LeaScriptEnabled=true;OutputPath=H:\Lea Builds\Lea-146-element-receivers-independent-v30\runtime\;BaseIntermediateOutputPath=H:\Lea Builds\Lea-146-element-receivers-independent-v30\obj\;IntermediateOutputPath=H:\Lea Builds\Lea-146-element-receivers-independent-v30\obj\;GenerateManifests=false;AutoGenerateBindingRedirects=true'
```

Compilation exits zero in 8,147 ms. It is not a default Visual Studio publishing
or reproducible-PE claim. Both host EXEs are 1,134,080 bytes; their hashes differ:
package `CD9A5C17302BFDD9DE9C828E72EE7E87B15C0C8A26D526CE076949A97CB52826`,
rebuilt `CAD2CF15A5B1710E523A5A0745C813547B567BAE8EC1633F4B68CFD748622A40`.

The independent native runtime uses verified package fixtures/assets overlaid
with the independently compiled EXE/config/start page and 19 dependencies.
Its stale package manifest is deliberately not copied. Native tests run on NTFS
L: (extraction) and C: (independent), not H:'s exFAT evidence/build storage.
Low C:/L:/F: space remains a constraint; no user data or failed evidence is removed.

## Retained startup failure

The independent script attempt runs from `2026-10-09T03:33:58.8879657Z` to
`2026-10-09T03:34:16.5967203Z`. Its contracts pass, then the very first supplied
script (`var retainedState=41;`) fails native startup before either Element
receiver assertion. The result is not a receiver regression assertion failure:

```text
Worker startup deadline exceeded. phase=hello; launch=497; probe=97;
wall=5012; cpu=781 ms
last captured marker: startup parsed
```

The 5,000 ms native and 1,000 ms trusted budgets are unchanged. The last marker
is not a stack or root-cause diagnosis. Passing candidate/extraction runs do not
erase this failure or preceding v24/v26 failures. The independent six-EXE startup
gate is held; neither an independent full script pass nor a 43-case pass is claimed.

The external wrapper copied two old TEMP captures after this failure. Their
hashes match the earlier fresh captures; provenance explicitly rejects them as
independent rendering evidence. They remain retained, not deleted or presented
as new screenshots.

## Preservation and next work

Evidence is under
`H:/Lea Evidence/Lea-146-element-receivers-packages-2026-10-08`: passing and
failed logs/results, ZIPs, extracted-source audit, guard evidence, harness/source,
capture provenance, exact post-test frozen runtimes, full Lea source backup and
checkpoint bundle. All 19 production files match in seven build/runtime roots;
the original 67 host inputs and three installed binaries are unchanged.

The experimental extracted EXE is
`L:/Lea Builds/Lea-146-element-receivers-extracted-v30/Firespider Internet 19.exe`.
Keep its accompanying files together. It is not a verified stable release or
installed replacement. Milestone 146 stays open and broader readiness stays
about 65%, a practical estimate rather than a standards score.

Next: use the retained first-demand failure to diagnose worker startup without
relaxing its deadline or isolation, then finish the held distribution gates.
Safe remote-script/resource policy, remaining legacy-only commands and broader
web compatibility still gate everyday browsing. Compressed size is not runtime speed.
