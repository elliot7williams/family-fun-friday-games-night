# Lea Tab And Session Commands

Milestone 52 moves common Firespider commands across the engine boundary.
The custom engine DLLs and public alias API are unchanged; the host now uses
the existing Lea URL, navigation, reload, private-state, and disposal APIs.

## Command Behavior

- Duplicate Tab opens a fresh navigation to the current URL in the same
  engine. It does not clone form edits or navigation history. An explicitly
  selected legacy tab still duplicates as legacy.
- Close Tab is now available in the menu. The original resources hid it,
  which also prevented the Ctrl+W path from activating it. Both engines use
  normal form shutdown when the last tab closes, not the legacy VB End path.
- Lea's close button/context command and Close Other Tabs/Close Tabs To The
  Right share URL capture, disposal, tab-count bookkeeping, and retained-state
  cleanup. Bulk close preserves the selected tab. Legacy tab-width restoration
  is retained without enlarging ordinary Lea tabs.
- Reopen Closed Tab, Ctrl+Shift+T, and the recently closed menu open supported
  saved URLs as Lea tabs. Stack reopening consumes entries and clears the
  stale fallback, so an empty stack does not reopen the same URL indefinitely.
- Bookmark Current Page and Bookmark All Tabs retain Lea URLs. Existing
  selected-tab URL navigation now delegates to Lea when appropriate, including
  the bookmark-link path. Recognized absolute URLs are parsed before search
  heuristics, so file URLs containing spaces stay URLs in both engines.
  Reload All Tabs reloads both engines and leaves
  suspended placeholders alone.
- Save/Open Named Session and the manager share their existing save/open
  helpers. Saving normalizes the name before replacing matching records.
  Opening appends Lea tabs, preserves existing tabs, and selects the first
  newly opened page. Saving supports the existing mixed-engine/hidden-tab list.
- Clear Saved Session deletes the Lea session file as well as legacy saved
  URLs. A failed file deletion leaves those settings intact and the menu reports
  failure. This clears the current saved snapshot; normal shutdown can still
  save the then-open tabs, as before.

## Bounds And Privacy

The existing recent-tab stack retains at most 25 distinct URLs; reopening checks
at most 25 entries in one action. Recorded/reopened URLs use the shared 4,096-
character HTTP(S)/file validator. Unsupported or overlong entries are skipped.

Named-session opening uses Lea's existing restore ceiling: twelve accepted
URLs, 64 KiB of UTF-8 URL data, and at most 256 candidate URLs examined per
action. The status reports the actual opened and skipped counts. This does not
truncate saved named-session records; the host's existing settings collection
and named-session lookup are not a newly bounded storage format.

Private mode does not add closed URLs to the persistent recent stack. A
background Lea view still marked private is excluded from recent history and
automatic Lea-session saving even after the window flag changes. Automatic
private startup still skips session restoration. Explicitly opening a named
session in private mode is allowed and creates private Lea views. Explicit
bookmark and named-session saves remain deliberate persistent user actions,
including in private mode; this is not a claim of complete legacy-host privacy.

## Verification

```powershell
.\Lea.Build\Test-FirespiderBoot.ps1 -Root '<test-host>' -Case commands
```

The new twelfth Boot case exercises actual menus and keyboard dispatch on the
compiled host, with temporary session/log files and restored test settings.
Its search target is loopback-only. Exact-URL checks cover raw-space file
navigation and legacy duplication as well as verifying the selected engine.
It first reproduced silent Duplicate Tab failure, then exposed the hidden Close
Tab command. Its checks cover duplicate/close/reopen, invalid history, private
retention, bookmarks, navigation, reload completion, mixed-engine bulk disposal,
tab counts/widths, named save/replace and ordered opening, limits without record
loss, explicit private opening, clearing and locked-file preservation, empty
tabs, and normal last-legacy-tab shutdown. Dialog-based named/clear commands
are tested through their shared helpers, not automated dialog interaction.
The launcher requires both exit code zero and the final case-success marker,
so abrupt successful process termination cannot masquerade as a passing test.

The final build passed all twelve Boot cases, eighteen existing startup-event
regressions, the full engine and compiled-host suites, all 57 compatibility
probes, and clean/restored/locked/oversized standalone EXE startup checks.
Standalone startup remained responsive and did not load MSHTML. This is
integration validation, not a new engine-speed or web-compatibility claim.

## Source And Remaining Work

Tab-session-host.patch records only the edits to 1.vb and BrowserFeatures.vb
after milestone 51 and requires the matching FirespiderLea.vb. Applying the patch
to a baseline source copy reproduces the live files. Full source backups retain
the rest of the existing host checkout; unrelated changes are not reverted.

Milestone 53 subsequently migrates both left-close variants, the previous-session
menu, and selected clipboard/home/domain operations; see `NAVIGATION_COMMANDS.md`.
This is not every Firespider command. Advanced legacy session/placeholder and
same-domain bulk routes still need migration. Lea still does not implement
the full modern web platform or a general JavaScript VM.
