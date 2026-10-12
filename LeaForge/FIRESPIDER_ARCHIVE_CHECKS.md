# Firespider Archive Checks

Milestone 146 remains open; broader readiness stays about **65%**. The frozen
lazy-style engine now has fresh ZIP-extraction and independent archived-host
build evidence. The result is mixed, not a verified release or a startup fix.
The preceding [candidate checkpoint](FIRESPIDER_LAZY_RUNTIME.md) remains intact.
Subsequent [fixed process samples](FIRESPIDER_PROCESS_SAMPLES.md) pass three
focused native scripting cases against the archived EXE, but do not reproduce
or resolve the failure retained below.

## Archive And Build Integrity

Both preceding ZIP hashes and every entry's length/hash are verified before
and after extraction: 388 runtime files and 104 host-source files, including
their manifests. All 19 Lea runtime entries remain identical in the extraction,
archived dependencies and independent build. No engine SDK rebuild, mode change,
new dependency, budget increase, retry or isolation relaxation occurs.

Only the extracted VB project is compiled, with .NET Framework 4.8 targeting
support and the installed framework MSBuild. There are no live-source overlays
or copied host EXE in that compilation. All 104 archived inputs remain unchanged.
Intermediate output stays outside the archive snapshot. After compilation,
365 manifest-verified fixture/asset/license files are copied from the extracted
runtime solely for tests; they are not compilation inputs.

The initial local wrapper invocation exits 1 before compilation: quoting the
property value instead of the whole MSBuild argument makes the property list
one scalar (`MSB4115`). The original wrapper, arguments and result are retained.
The corrected invocation builds successfully into a previously absent output.
This correction is not a browser or worker retry.

Independent EXE: 1,134,080 bytes, SHA-256
`D5FB52B3B38B5F700034D151954387037323BB1A922FB7F29D886088089C6C90`.
It differs from the archived EXE; reproducible host compilation is not claimed.
Native browser/worker execution stays on C: NTFS, not H: exFAT archive storage.

## Actual Results

| Output | Host matrix | Actual EXE startup |
| --- | --- | --- |
| Fresh runtime extraction, original harness | 42 cases pass; scripting case fails | All six scenarios pass |
| Independent archived-source build, strengthened harness | All 43 cases pass | All six scenarios pass |

Every native gate is attempted once. The extraction's scripting case reaches
the post-click split title assertion and exits 1 with the old message, "split
script state leaked between panes or source tabs." That message combines three
title expectations; it does not prove an actual cross-pane state leak. The old
harness did not capture the three states at that assertion, so the exact cause
remains unknown. Its source, binary, controller, logs and two pre-failure
captures are retained. The extraction matrix is not retried or labelled passed.

The subsequent rebuilt-host matrix uses the same engine files and a stronger
test harness. Its pass does not reproduce or resolve the earlier failure.
Its scripting case includes inline lifecycle, callback receivers, native
editing, timers, error recovery, split isolation/disposal and remote refusal.
The full host matrix deliberately includes explicit legacy-fallback cases.

Both actual-EXE startup wrappers pass clean, restored, locked, oversized,
lazy-background and lazy-all scenarios, including Resume. No matching exception
or recovery dialog or MSHTML module is detected, and responsiveness holds.
The wrappers close their browsers after observing them; unassisted shutdown is
not established. Startup scenarios do not attest JavaScript output. Readiness
is 3,556..6,250 ms for extraction and 3,083..6,529 ms for rebuild; total
observation is 8,111..8,768 and 8,087..8,816 ms. These are individual observations
from launch, not a speed benchmark or a startup SLA.

## Split Failure Diagnostics

The harness now asserts both panes and the source tab have `Lea ready` titles
and successful script results before clicking the left counter. A non-busy
view alone is no longer treated as proof of successful inline lifecycle.
The original post-click title expectations remain unchanged. Failed ready or
activation checks include each view's bounded title, busy state, worker ID,
script result and existing startup diagnostics. Metadata is read only on failure;
there is no extra page-code probe on the passing path.

Test-only `-sc` aliases `--split-contract`. Three controls exercise the passing
fast path and exact ready/activation failure messages without creating views
or browser workers. Both aliases pass. The full boot runner also executes these
controls and verifies their exit code/footer, alongside clipboard 14 and
observation 12 contracts. A checksum-selected harness must be rebuilt from the
current source to support the new contract. Engine public aliases are unchanged.

The retained main capture and rebuilt split capture are visually inspected:
nonblank Lea content, rendered logo, retained fields, and independent counters
1/0 without overlap. All five captures pass dimension/nonblank sample checks.

## Artifacts And Remaining Work

Evidence: `H:\Lea Evidence\Lea-146-lazy-archive-verification-2026-10-11`.
Fresh extraction:
`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-lazy-archive-extracted-2026-10-11`.
Independent build:
`C:\Users\Elliot\AppData\Local\Lea\FirespiderCandidates\Lea-146-lazy-archive-rebuilt-2026-10-11`.
Archived source: the evidence directory's `archived-source` folder. Complete Lea
source is separately backed up under `H:\Lea Source Backups`; its exact inventory
and archive hash are recorded in `source-backup.json` after this source update.

Original 67 host inputs, three installed files, normal outputs, both prior ZIPs
and earlier sealed evidence remain unchanged. No new runtime ZIP supersedes
them. The earlier full 2,945-check engine pass is historical, not rerun here.
Release verification stays false: diagnose the intermittent split failure,
verify repeated cold starts, address remaining legacy-only commands and expand
real-site/origin/resource compatibility before adoption. Compression remains a
storage/delivery benefit, not proof of faster execution.
