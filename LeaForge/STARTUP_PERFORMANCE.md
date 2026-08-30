# Firespider Startup Performance

Milestone 51 profiles and reduces the host startup delay observed in milestone
50. This is a Firespider UI improvement, not a faster HTML/CSS engine or a change
to the supported web platform. Lea.Engine.dll and Lea.Win.dll are unchanged.

## Measured Cause And Change

The original Form.Load path added hundreds of menu items individually, applied
appearance changes, then reorganized those items into categories. WinForms
repeatedly recalculated dropdown layout during that work. The first profile
measured 13.8 seconds in menu construction/reorganization within a 15.2-second
Form.Show call.

The host now suspends layout on its eight root dropdowns while building the
menus and resumes them in reverse order in Finally. Newly created category
dropdowns likewise suspend layout until all their items are assigned, with
Finally cleanup even when category construction fails. Commands are neither
removed nor deferred until the user opens a menu.

## Paired Samples

Five alternating before/after runs used separate fresh processes, the same
local start page, seeded settings, and the same Boot.cs harness. Both binaries
included identical opt-in timing checkpoints. These are warm-machine samples
on this Windows workstation, not cold-boot or cross-hardware guarantees.

| Run | Before Form.Show (ms) | After Form.Show (ms) | Before menus (ms) | After menus (ms) |
| --- | ---: | ---: | ---: | ---: |
| 1 | 15477 | 1652 | 14196 | 467 |
| 2 | 13681 | 1791 | 12489 | 480 |
| 3 | 14062 | 1675 | 12824 | 469 |
| 4 | 14587 | 1683 | 13188 | 462 |
| 5 | 13589 | 1688 | 12406 | 467 |
| Median | 14062 | 1683 | 12824 | 467 |

Median Form.Show time decreased by 88.0%. It includes initial native window
creation and Load/Shown work, but excludes the preceding form constructor and
does not mean an arbitrary network page has finished loading. The menu column
measures menus-start through menus-ready; the final root resume is included in
Form.Show and took another 13-18 ms in the after samples. Constructor samples
ranged from 725-1029 ms and are reported separately by the harness.

Root layout events fell from 1,075 to 8 in every paired run. Each run retained
552 menu-tree items, with identical order, types, names, labels, enabled state,
image presence, check state, shortcut keys, and child counts. The binary-encoded
tree signature was identical in all ten samples:

```text
DCFFBB5B9EDCC43B6E0A0241A75DD2C37C8C84FA199AF13C615149CE626C99C1
```

Signatures describe the seeded clean case, not every possible user profile or
event-handler implementation. The separate clean-case command checks verify
that primary Lea new-tab routes and the explicit legacy option still work.

## Reproduction And Regression Checks

```powershell
.\Lea.Build\Test-FirespiderBoot.ps1 -Root '<extracted-runtime>' -Case clean -Profile
.\Lea.Build\Test-FirespiderBoot.ps1 -Root '<extracted-runtime>' -Case menus
```

Repeat the first command in fresh processes for multiple samples. Compare
matched before/after host builds without running performance tests concurrently.
The eleven-case default includes all ten previous boot regressions and the
new menus case. The latter traverses every non-empty dropdown, checks that
PerformLayout still fires and available items have non-empty bounds, then
forces a duplicate-category error and verifies layout has resumed afterward.

The adapter's private timing list is allocated only when
LEA_FIRESPIDER_PROFILE=1 and retains at most 32 phase/time strings. It performs
no file I/O and contains no page URLs. Test-FirespiderBoot.ps1 -Profile scopes
and restores that environment override. Checkpoint times are cumulative from
the adapter field's initialization during construction, not process launch.

Test-FirespiderStartup.ps1 now polls from process launch and reports ReadyMs
for its first ready Lea UI Automation surface. It continues observing for at
least eight seconds by default to catch delayed dialogs, while retaining the
configuration, responsiveness, session, and no-MSHTML checks. ReadyMs includes
polling/UI Automation overhead; it is not an exact first-paint measurement.

The four standalone EXE checks passed after the change: clean (4,409 ms),
restored (6,134 ms), locked session (5,153 ms), and oversized session (4,814 ms)
to the first observed ready surface. All remained responsive through the
eight-second observation period and did not load MSHTML. These are separate
one-run readiness observations, not the paired Form.Show benchmark above.
The full engine and compiled host suites, including eighteen startup-event
regressions, and all 57 compatibility probes also passed.

## Source And Remaining Work

Startup-layout-host.patch contains the incremental host edits after milestone
50, and requires the matching FirespiderLea.vb. Check patch applicability before
using it on another checkout. The full Firespider source backup retains the
rest of the existing project and its unrelated changes.

This supersedes the unprofiled 11-19-second observation in LEA_STARTUP.md.
Startup still includes constructor, settings, native-control, and first-page
costs. Continue measuring those before further changes. Advanced legacy-only
commands and modern web compatibility remain unfinished; compressing a ZIP
or shortening identifiers does not eliminate those execution costs.
