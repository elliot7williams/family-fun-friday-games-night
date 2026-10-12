# Firespider With Lazy Styles

Milestone 146 remains open; broader readiness stays about **65%**. This is a
fresh, separate Firespider Internet 19 candidate using the frozen runtime from
[lazy element styles](LAZY_STYLES.md), including ordinary worker mode `Be=0`.
It supersedes the older dictionary-frame candidate for this test checkpoint,
not the installed browser. No engine implementation changes are made here.

## Build And Runtime

The builder copies 67 original host inputs and the clean Lea adapters, then
targets .NET Framework 4.8 in the copy only. All 19 selected engine files and
the browser binding-redirect configuration are recorded and hash-checked
before and after compilation. All 19 engine files match the preceding sealed
lazy-style runtime; its required Engine/Win/Image bundle remains 749,568 bytes.

```powershell
$runtime = 'C:\Users\Elliot\AppData\Local\Lea\EngineBuilds\Lea-146-lazy-styles-2026-10-11-browser'
.\Lea.Build\Build-FirespiderCandidate.ps1 -r $runtime -o 'H:\Lea Candidates\Firespider-lazy-next'
```

Choose a new output directory. Native workers must run from a local NTFS
deployment, not the H: exFAT build/archive volume. The available test EXE is:

`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-lazy-runtime-2026-10-11\Firespider Internet 19.exe`

Keep the accompanying files and `ScriptWorker` directory together. EXE SHA-256:
`4D3C3D47892C7FCBF345E029661423D86F2E3CBEC1830EE4C6A049DA174E75BD`.
The historic window title still says Firespider Internet 17; this checkpoint
does not rename the original host's UI.

## Verification

Evidence: `H:\Lea Evidence\Lea-146-firespider-lazy-runtime-2026-10-11`.

- All 10 candidate-input contracts pass without launching a compiler/browser.
- The copied host and SDK-compiled current 4.8 boot harness build successfully.
- All 43 distinct host cases pass once, including optional native scripting,
  recovery, split-pane isolation/disposal, and remote-origin refusal. The matrix
  also deliberately tests the explicit legacy fallback; it is not removed.
- Clipboard and observation contracts pass 14/12 checks. No external stack
  observer or controller-enabled startup trace is used.
- All six actual-EXE startup scenarios pass once: clean, restored, locked,
  oversized, lazy-background and lazy-all. The wrapper detects no matching
  startup error dialog or loaded MSHTML module; the browser remains responsive.
  Lazy-all starts deferred and its Resume command loads Lea. Its report's
  `Lea=False` describes the initial deferred state, not a failed resume.
- All three package rejection contracts pass: changed runtime, escaping runtime
  paths and changed source snapshots are rejected before packaging.
- Both ZIPs are checked entry by entry for inventory, length and SHA-256.
  A separate complete Lea source backup is checked against the live source.

Every listed helper has actual exit zero, one attempt, and no timeout. The
startup wrapper closes its browsers after observation; this does not establish
unassisted shutdown. Reported readiness spans 3,065..5,363 ms and total
observation 8,082..8,816 ms. These are individual test observations, not a speed
benchmark or startup SLA. Startup scenarios do not attest JavaScript output;
the separate matrix's scripting case does.

The preceding sealed engine checkpoint passed 72 focused lazy-style checks
and one full 2,945-check regression. Those engine suites are not rerun here.
Original host inputs, three installed files, normal Lea outputs and selected
runtime hashes remain unchanged. Earlier intermittent native startup failures
are not reproduced, explained or reclassified as fixed.

## Packages And Remaining Work

Packages are under the evidence directory's `packages` folder:

| Package | ZIP bytes | Entries, including manifest | SHA-256 |
| --- | ---: | ---: | --- |
| `Firespider-Lea-146-script-runtime_2026-10-11_22-25-42.zip` | 3,667,436 | 388 | `84D5E23BCB8F5D0E6F84D5B59BAEF239882FD143189E3819689D5030C92E3AB5` |
| `Firespider-Lea-146-script-host-source_2026-10-11_22-25-42.zip` | 3,304,813 | 104 | `98C1D52E76859E53FEA05C53ACCD28405F87B0245A1716456849967CDA83A30E` |

Host-source contains copied VB inputs and dependencies, not all Lea C# source.
Complete Lea backups remain under `H:\Lea Source Backups`, with the exact
archive inventory/hash in this checkpoint's `source-backup.json`. Compression
reduces storage/delivery size; it does not by itself make execution faster.

Fresh archive-extraction tests, an independent host-source rebuild, repeated
cold-start reliability, and modern-web compatibility remain release work.
The actual candidate is experimental, not an installed replacement or a stable
release. This verification does not add new public aliases or widen limits.
