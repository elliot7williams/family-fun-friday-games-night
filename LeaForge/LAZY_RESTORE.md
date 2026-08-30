# Lea-Native Lazy Restore

Post-1.0 milestone 55 routes saved-session restoration through Lea's bounded
tab creation path. This changes the Firespider adapter and host, not the engine
DLLs, public compact aliases, or HTML/CSS compatibility.

## Restore Policy

The existing `lazyRestoreMode` setting now applies to Lea startup/recovery,
legacy saved URLs converted to Lea, named and previous sessions, and the host's
`qlazyopenurls` callers, including research collections and identity loading.

| Mode | Initial work | Activation |
| --- | --- | --- |
| `off` | Load each accepted URL in a new Lea view | Existing eager behavior |
| `background` | Load the first accepted URL; defer the rest | Select a deferred tab, Reload, or Resume |
| `all` | Create only tab/Panel placeholders | Resume/Reload the initially selected tab; selecting another deferred tab loads it |

The default remains `off`; this milestone does not overwrite the user's setting.
Invalid setting values fall back to `off`. A deferred tab has no `Lea.Win.V`,
legacy `WebBrowser`, document, or page request until resumed. Automatic selection
events during batch creation do not accidentally start deferred pages.

Explicit saved-session opens append new tabs and select the first new tab,
preserving existing documents and unsent field values. Clipboard URL opening
remains eager. Research/lazy commands report accepted and skipped counts.
The status timer no longer replaces deferred-tab status with a generic Ready.

## Ownership And Bounds

Targets are validated before tab mutation. Each call visits at most 256 input
entries and accepts at most 12 URLs, with 4,096 characters per normalized URL
and 64 KiB total UTF-8 session data. HTTP, HTTPS, and absolute file URLs are
supported. Invalid/empty input leaves existing tabs unchanged. These are batch
limits, not a global cap on all open browser tabs.

Deferred tabs share milestone 54's pause/resume ownership: URL and private flag
survive; resuming creates a fresh Lea context. Private deferred URLs do not enter
automatic Lea sessions, and resuming after the global private toggle is disabled
does not downgrade a previously private tab. Automatic private startup still
does not restore saved URLs.

Identity switching is an explicit replacement operation: create the new identity's
tabs, dispose old visible/hidden/paused/mixed-engine tabs and Lea menus, and prune
retained tab bookkeeping. Empty identities receive a fresh start-page tab under
the same lazy policy. Old private or normal contexts are not reused, and old URLs
are not added to recent history by replacement. A private identity enables the
private flag; switching identities does not downgrade an already-private window.

`Lazy-restore-host.patch` follows `Suspension-host.patch` and changes only
`BrowserFeatures.vb`, `BrowserFeatures4.vb`, `BrowserFeatures6.vb`, and
`BrowserFeatures7.vb`. Use the matching `FirespiderLea.vb`. The patch is already
applied in `L:\Firespider Internet`; reverse checking and forward application to
the saved baseline both passed, with exact normalized source comparisons.

## Verification

- All 19 form-start Boot cases passed. The three new cases are `lazy`,
  `lazy-background`, and `lazy-all`.
- Loopback tests measure exactly one initial request in `background` and zero
  in `all` for three saved pages. Reload, selecting another tab, returning to a
  loaded tab, session saving, and Resume All issue only the expected requests.
- Command tests cover preserved fields, named/previous sessions, invalid modes,
  input/batch bounds, eager clipboard behavior, private deferred tabs, mixed and
  hidden identity disposal, fresh contexts, bookkeeping, and empty identities.
- All six real-EXE startup scenarios passed: clean, restored, locked, oversized,
  lazy-background, and lazy-all. The last case invokes the actual Resume button.
  Each checks responsiveness, visible error dialogs, and absence of MSHTML.
- Full engine and Firespider host regressions passed; 57/57 compatibility probes
  passed. Core DLLs remain 471.0 KiB combined, with no new runtime dependency.

The standalone tests use UI Automation for Lea and window discovery, with bounded
native messages for standard WinForms tab counts and the Resume button. This
older host exposes those controls as panes without UIA invocation patterns.
Lazy EXE cases use fresh temporary executable paths/configuration and isolated
session files; original user settings are not rewritten to select a lazy mode.

## Limits And Next Work

This is deferred creation/reloading, not a suspended DOM/form/history snapshot.
The previous startup cast remains guarded and covered by regression tests; the
latest reported popup was not reproduced, so these results do not identify its
unknown cause. They verify the tested startup paths do not show that dialog.

Legacy `qopenurls` callers, including workspace/closed-window bulk routes, and
automatic hibernation eligibility still need migration. The host's broader
identity settings/export/privacy system is not comprehensively redesigned here.
No general JavaScript VM or full modern-web compatibility is claimed. Fewer
startup page requests avoid work, but ZIP compression and shorter aliases alone
do not demonstrate faster execution; benchmark timings remain machine-local.
