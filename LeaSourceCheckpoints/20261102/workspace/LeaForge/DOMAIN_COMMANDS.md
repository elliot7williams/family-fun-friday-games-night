# Lea Same-Domain Commands

Post-1.0 milestone 57 migrates Firespider's same-domain reload, close-other,
bookmark, duplicate, next-tab, new-window, copy-list, and snapshot routes to
engine-aware helpers. Compact internal action aliases are `rl`, `cl`, `bm`,
`dp`, `nx`, and `wn`. The core engine DLLs and public aliases are unchanged.

## Matching And Ownership

Matching uses exact, case-insensitive IDN-normalized HTTP/HTTPS hostnames,
including matches across ports and schemes. This is not same-origin or
registrable-domain matching; subdomains remain distinct. File URLs do not
match. Only visible tabs participate, in tab order, preserving the commands'
existing scope. Hidden-group tabs remain untouched.

- Both reload menus refresh active matching views in their existing engine.
  Busy and suspended views are skipped and counted, not resumed or interrupted.
- Close-other preserves the selected tab and disposes matching views, menus,
  and retained state through the shared mixed-engine close path. Private Lea
  URLs do not enter recent history.
- Duplicate appends at most twelve new eager Lea tabs, preserving every source
  document and its private flag. It selects the first new tab and reports skips.
- New-window includes the first matching URL, avoids an unwanted default-page
  request, and honors saved-session lazy policy. If any source tab is private,
  the whole new window is private; the original window is unchanged.
- Next-tab wraps among matches. Selecting a paused tab uses normal Lea resume.
- Bookmark commands and Ctrl+Shift+B retain existing duplicate-bookmark
  semantics. Current-domain bookmarks retain the port and omit credentials.
  Explicit bookmark/export commands may include private URLs at the user's
  request; automatic snapshots still exclude private Lea URLs.

New-tab/window batches retain the shared 4,096-character normalized URL and
64 KiB aggregate UTF-8 limits. New windows also retain the 256-candidate scan
ceiling. These limits apply per action, not to the total number of open tabs.

## Bounded Exports

Copy-list and snapshot text include at most 256 rows and 64 KiB of UTF-8 text,
with an omission count when truncated. Labels process only a 256-character
prefix, replace control characters with spaces, and avoid splitting a trailing
surrogate pair. Snapshot URL/homepage headers are bounded too. Save Snapshot
writes only after the dialog returns OK; cancel no longer falls through to a
default filename. Clipboard failure leaves a readable status.

## Verification

The milestone 56 runtime failed the new same-domain reload regression: neither
Lea tab received a request through the legacy-cast command. The migrated paths
pass using exact loopback request counts. The two new Boot cases bring the
suite to 24:

- `domains`: both reload menus, cross-port matching, busy/paused skips,
  IDN/case normalization, hidden/unrelated tab isolation, retained form values,
  actual duplicate/bookmark/shortcut/next/close commands, private copies and
  history exclusion, bounded pure exports, and explicit mixed-engine disposal.
- `domain-private`: first-URL retention, private new-window inheritance under
  lazy-all, zero default-page requests, sticky deferred privacy, twelve-tab
  creation limits, per-source private duplicates, large UTF-8 exports, label
  sanitization, surrogate boundaries, and the 256-row export ceiling.

Full engine and compiled-host regressions, six real-EXE startup scenarios, and
all 57 compatibility probes accompany the build. The unknown reported startup
popup was not reproduced. Previously fixed startup callbacks and the original
timer cast remain covered; this milestone is not a claim to have diagnosed a
new popup without its exception details.

`Domain-commands-host.patch` follows `Bulk-opening-host.patch` and records ten
command changes in `1.vb`; use the matching adapter. Reverse checking against
the live host and forward application to its saved baseline both pass exact
normalized comparisons. OS clipboard I/O and the interactive Save dialog are
not automated; tests exercise the pure export builder without overwriting the
user's clipboard or choosing a file for them.
An extracted-package check also reproduced a test-launcher quoting failure for
paths ending in a backslash. The runner now escapes trailing backslashes before
its closing native argument quote; both path forms are exercised.

## Remaining Work

Open Current Domain In Default Browser, pop-up routing, split view, DOM-based
tools, and automatic hibernation still need migration. Explicit legacy tabs
remain available. Lea supports a bounded HTML/CSS and scripting subset, not a
general JavaScript VM or the full modern web. Core DLLs remain 471.0 KiB combined;
short aliases and compressed delivery archives are not runtime-speed evidence.
