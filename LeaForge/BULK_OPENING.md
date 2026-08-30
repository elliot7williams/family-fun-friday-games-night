# Lea Bulk And Window Restoration

Post-1.0 milestone 56 migrates the remaining shared `qopenurls` route, workspace
restoration, protected-session opening, both closed-window commands, and unread
watchlist batches to Lea. The core DLLs and public compact aliases are unchanged.

## Behavior

- Direct bulk and unread-watchlist opens append new, eagerly loaded Lea tabs.
  Existing views and unsent form values are preserved, including legacy tabs.
- Workspaces and protected saved sessions honor `lazyRestoreMode`, like named
  and previous sessions. Workspace settings still merge into existing settings;
  original workspace records remain intact. Reports include accepted/skipped URLs.
- Last/recent closed-window commands validate URLs before constructing a window.
  Accepted targets enter startup directly, without first requesting a default
  page or creating a legacy control. The originating window is unchanged.
- Reopened windows inherit the initiating window's private flag. Global private
  startup remains respected, and deferred tabs retain their private state.
- Last-window, recent-window, and recovery snapshots include supported ordinary
  Lea, hidden, paused, and legacy URLs. Sticky-private Lea URLs are excluded even
  after the global private flag is disabled. Private windows do not add snapshots.

Shared target selection visits at most 256 URL candidates, accepting up to 12
HTTP/HTTPS/file URLs, each at most 4,096 normalized characters and at most 64 KiB
of aggregate UTF-8 data. Invalid-only window input creates no window. Explicit
bulk limits apply per call, not to the total number of open tabs. Legacy-only
`ls` bookkeeping remains separate from the Lea session file to avoid duplicate
startup restoration.

## Watchlist Accounting

The watchlist scans at most its existing 500-record capacity, newest first, and
collects up to 20 distinct eligible candidate URLs. Lea selects at most 12 of
those. Items are marked read only after their URL is accepted into a new tab;
skipped/overlong entries remain unread. The next invocation can open the remaining
eligible URLs. Already-read recent entries do not starve older unread entries
within the supported 500-record list.

URLs are normalized, then compared ordinally, retaining case-sensitive path
differences such as `/Case` and `/case`. Duplicate records for an accepted URL
are marked together. The status reports tab and item counts separately, since
several records may refer to one tab. Read state still means opened, not that
the page has successfully downloaded; HTTP failures can appear in the opened tab.

## Evidence

The previous runtime failed the new bulk test by replacing the first document
and creating legacy tabs. Its last-window command also reproduced the logged
`Lea.Win.V` to `WebBrowser` cast failure. Both cases pass with this milestone.
This is a confirmed closed-window defect, not proof of the unknown startup
popup's cause.

The Boot suite has 22 cases, including three additions:

- `bulk`: original field retention, direct eager behavior under lazy-all, null/
  invalid input, 12-tab bounds, first-new selection, lazy workspace restoration,
  bookmark merging, record preservation, private views, and mixed-engine tabs.
- `windows`: real last/recent menu routes, exact loopback request counts, no
  unwanted homepage request, lazy policies, private inheritance, hidden/paused
  snapshots, private exclusion, no-op invalid input, and candidate limits.
- `watch`: exact eager request counts, two bounded batches, skipped/read state,
  distinct case-sensitive paths, duplicate accounting, preserved record fields,
  original form retention, old unread entries, and the 500-record scan ceiling.

Full engine and Firespider integration regressions, six real-EXE startup
scenarios, and all 57 compatibility probes are also exercised. The older local
file-disposal test raced against an already-completed read on this run. Its
fixture now holds the file task in a step scheduler until disposal, proving
cancellation before completion without adding a production timing delay or
weakening the cancellation assertion.

`Bulk-opening-host.patch` follows `Lazy-restore-host.patch`. It captures changes
to `1.vb`, `BrowserFeatures.vb`, `BrowserFeatures2.vb`, and `BrowserFeatures8.vb`;
use the matching adapter. Reverse checking against the live host and forward
application to saved baseline copies both pass exact normalized comparisons.

## Remaining Work

The protected-session route shares the tested saved-URL helper; its interactive
vault dialog and Windows encryption UI are not automated by these tests.
Watchlist fetching and broader metadata/privacy settings are not redesigned.
Direct legacy-only same-domain commands, pop-up routing, split view, DOM-based
tools, and automatic hibernation still need migration. Explicit New Legacy Tab
remains available, so the whole host is not yet legacy-engine-free.

These changes avoid unwanted work and broaden Lea ownership. They do not add
full JavaScript or modern-web compatibility, and compressed archive size is not
evidence of faster execution. Runtime engine DLLs remain 471.0 KiB combined.
