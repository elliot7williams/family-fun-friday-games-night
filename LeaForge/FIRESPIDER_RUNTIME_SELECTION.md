# Firespider runtime selection

Milestone 146 remains open; broader readiness stays about **65%**. This checkpoint
builds a fresh separate Firespider Internet 19 EXE with the frozen, previously
tested dictionary-frame runtime. It does not rebuild, migrate or overwrite the
installed .NET Framework 4.6.1 host.

## Small build API

`Build-FirespiderCandidate.ps1 -Runtime` (alias `-r`) selects the engine input
folder. Omitting it preserves the previous Release-output default. The existing
`-s`, `-o` and `-ns` options are unchanged. Candidate output cannot already exist,
be inside the live source, or be inside the selected runtime folder.

The script checks all required inputs before creating the candidate: 19 runtime
files and the browser binding-redirect configuration in scripting mode, or just
the three required engine files with `-ns`. It records relative paths, sizes and
SHA-256 values in `runtimeInputs`, plus `runtimeSource`, in `source-manifest.json`.
Selected bytes and copied Lea dependencies are checked before compilation; the
compiled output and inputs are checked again afterward. Runtime selection is
not a signature or a claim that arbitrary third-party inputs are safe.

The package builder checks that engine files in the compiled runtime still match
the selected inputs before creating ZIPs. Existing manifests without the new
optional provenance fields remain supported. The host-source package includes
copied VB inputs and dependencies, not all Lea C# source; the separate full source
backup remains necessary.

```powershell
$runtime = 'H:\Lea Evidence\Lea-146-dictionary-frames-2026-10-11\browser-build'
.\Lea.Build\Build-FirespiderCandidate.ps1 -r $runtime -o 'H:\Lea Candidates\Firespider-next'
```

## Verification

Proof: `H:\Lea Evidence\Lea-146-runtime-selection-2026-10-11`.

- All 10 input contracts pass: short alias, missing folder, file instead of
  folder, nested runtime/source output, missing core/parent/worker/configuration,
  and occupied output preservation. No compiler or browser is launched by them.
- All three package contracts pass: changed runtime bytes, escaping runtime
  paths, and changed source snapshots are rejected before package creation.
- A fresh .NET Framework 4.8 candidate compiles successfully from 67 copied host
  inputs and the current clean Lea adapters. Its 19 engine files match the prior
  frozen runtime, including the eight-file native worker deployment.
- The actual EXE passes all six startup scenarios once: clean, restored, locked,
  oversized, lazy-background and lazy-all. Each remains responsive; no startup
  error dialog or MSHTML module is detected. Lazy-all resumes into Lea.
- Native startup runs only from a separate C: NTFS deployment. It does not run
  from the H: exFAT source/proof folder.

The startup wrapper exits 0; it closes the browser after observation, so this is
not a claim that each browser exits normally on its own. Observed readiness is
4,608-6,754 ms, not a speed benchmark or an established startup SLA. No standalone
browser, scripting full suite, copied-host 43-case suite or renderer suite is
rerun here: their preceding results remain historical evidence for the identical
engine files. The current scripting runtime uses experimental `Be=2`; production
defaults remain `Be=0`.

The first contract and builder tool invocations failed before compilation because
a Windows PowerShell child inherited PowerShell 7 module search paths and could
not resolve `Get-FileHash`. Their records remain preserved. The local proof helper
then supplied Windows PowerShell paths only for its child and used fresh named
fixtures and logs. No global environment changes or native startup retries were
made. Earlier intermittent native-worker failures are not reclassified as fixed.

Original 67 source inputs, three installed files and normal Lea output hashes
remain unchanged. Packages are checked entry by entry against their manifests,
and a separate complete source backup is verified. No independent archived-source
rebuild, universal startup fix, speedup or stable-release claim is made.

Test EXE:
`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-runtime-selection-2026-10-11\Firespider Internet 19.exe`.
Keep its accompanying engine files and `ScriptWorker` folder together.
