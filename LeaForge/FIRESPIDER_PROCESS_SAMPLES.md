# Firespider Process Samples

Milestone 146 remains open; broader readiness stays about **65%**. Three
declared fresh-process scripting samples pass against the exact archived
Firespider EXE, using the current split diagnostics. The preceding
[extraction failure](FIRESPIDER_ARCHIVE_CHECKS.md) is not reproduced, explained
or relabelled passed. No production engine files or installed host are changed.

## Fixed Sample

A new C: NTFS folder contains a fresh extraction of the pinned runtime ZIP.
All 388 entries and 19 engine dependencies are verified. Its archived EXE
still has SHA-256
`4D3C3D47892C7FCBF345E029661423D86F2E3CBEC1830EE4C6A049DA174E75BD`.
The separately copied, source-verified diagnostic harness has SHA-256
`23C6EFC7D360DA65033E7019E530D16B85C5FFCBCCE5A884DC645AFD14DEF4B1`.

The plan declares three separate processes before any sample starts. Each
result, controller/source hash, stdout/stderr and capture is retained in its
own files. This is fixed sampling, not retrying a failed assertion to obtain a
pass. There is no observer, controller-enabled startup trace, mode switch,
budget increase or sandbox relaxation; the worker stays `Be=0` with its
5,000 ms startup bound. Fresh processes are not proof of a cold OS/file cache.

| Sample | Actual exit | Complete scripting case | First demand, ms | Split load, ms |
| --- | ---: | --- | ---: | ---: |
| 1 | 0 | Pass | 4,497 | 4,847 |
| 2 | 0 | Pass | 5,675 | 4,701 |
| 3 | 0 | Pass | 3,983 | 4,632 |

Each first passes clipboard 14, observation 12 and split-report 3 contracts.
The native case then covers successful inline readiness, keyboard activation,
callback receivers, field edits, timers, error recovery, independent split
workers/state/disposal, tab cleanup and remote-script refusal. All owned
processes are gone after each sample and all frozen deployment hashes match.
The timings above measure end-to-end host work, not just worker handshake;
they are individual observations, not a speed benchmark or startup SLA.

These three focused cases are not another 43-case matrix or six-scenario
actual-EXE startup suite. The earlier complete 2,945-check engine run is not
rerun. Passing samples alone do not diagnose the earlier combined title
assertion or establish reliable release behavior.

## Small Compile-Only API

`Test-FirespiderBoot.ps1 -CompileOnly` now has the short alias `-co`. It compiles
the current harness and returns before running contracts, clipboard code or
browser cases. It cannot be combined with explicit case, profile, observation
or harness-checksum selectors. The normal compile-and-run and verified `-hh`
paths retain their existing behavior.

```powershell
.\Lea.Build\Test-FirespiderBoot.ps1 -r 'C:\path\candidate\runtime' -co
.\Lea.Build\Test-FirespiderBoot.ps1 -r 'C:\path\candidate\runtime' -CompileOnly
```

The tool still uses the installed framework C# compiler. An initial hypothesis
that it could not compile this source was disproved: the underlying compiler
exits 0 and creates a valid PE. The hypothesis wrapper then exits 1 because it
expected failure; its inputs, result, binary and initial wrapper are retained.
No SDK/compiler replacement or new dependency is introduced.

All eight compile controls pass: four conflicting-option rejections, missing
root and missing-library failures, and both successful aliases. Negative
cases preserve an existing generated harness. Compile-only succeeds with only
the two reference DLLs present, producing the correctly named PE without a
host EXE, assets, fixtures or script runtime to launch.

A separate fresh runtime extraction then passes the ordinary automatic
compile-and-run `-c script` command, without a copied harness or `-hh` override.
Its 14/12/3 contracts and complete native scripting case pass with actual exit
zero. All 388 packaged entries remain unchanged. This validates the public
entry path; it does not add another declared sample to the table above.

## Evidence And Limits

Evidence started on October 11 and completed on October 12:
`H:\Lea Evidence\Lea-146-split-process-samples-2026-10-11`.
The three-sample deployment is
`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-split-process-samples-2026-10-11`.
The automatic-compiler test copy is
`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-boot-compiler-2026-10-12`.

Complete Lea source is backed up separately under `H:\Lea Source Backups`,
with the exact inventory/hash in this checkpoint's `source-backup.json`.
Original host inputs, installed files, normal outputs, prior ZIPs and earlier
sealed evidence remain unchanged. No new engine/browser release ZIP is made.
Repeated startup reliability, the unresolved split failure, remaining
legacy-only commands and real-site/origin/resource compatibility still gate
adoption. Compression remains a storage/delivery property, not a speed claim.
