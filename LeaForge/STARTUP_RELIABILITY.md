# Firespider Startup Reliability

Milestone 49, 2026-08-30. This is a host-integration hardening milestone, not
an increase in full-web compatibility or a claimed rendering speedup.

Milestone 63 additionally reproduces and fixes the saved-style automation
startup/timer cast. See `STYLE_STARTUP_SAFETY.md` for that separate path,
regression coverage, and the remaining stackless recovery observation.

Milestone 78 removes a separate blocking startup condition: unclean sessions
are recovered automatically instead of waiting on a modal question, and the
retired built-in home falls back to the bundled Lea start page. The executable
harness now distinguishes top-level dialogs from the identically named menu
command and uses bounded native tab/Resume checks. All six real EXE scenarios
pass without an exception, JIT, script-error, recovery dialog, or MSHTML.

## Reproduced Before The Fix

The new smoke exercised the previously built Firespider executable and observed:

- Address/search SelectedIndexChanged events throwing InvalidCastException
  because their handlers expected KeyEventArgs instead of EventArgs.
- Legacy Navigating, Navigated, and DocumentCompleted callbacks throwing the
  reported Lea.Win.V-to-WebBrowser InvalidCastException while Lea was selected.
- Background progress changing selected-tab chrome and navigation checking
  another tab's lock state.
- Detached legacy callbacks failing and the notification timer dereferencing
  a missing selected tab.
- A locked session file throwing IOException through the startup path, and
  oversized session input lacking an explicit bound or diagnostic.

## Changes

The adapter validates the callback sender and its live owner tab. Detached or
wrong-engine navigation is cancelled; selected-tab chrome updates require an
owner match. Background legacy document policies still run on their sender.
Empty-tab navigation commands do not fall through to legacy casts. The host's
selection-event signatures and notification timer are corrected directly.

Session restoration reads at most 64 KiB under a read-only sharing lock, accepts
at most twelve HTTP(S)/file URLs, and caps each input and normalized URL at
4,096 characters. Expected I/O/access failures produce a status message rather
than an unhandled startup exception. Saving uses the same limits. Private mode
does not restore a session and removes the session file when saving.

`Lea.Firespider/Startup-host.patch` preserves this milestone's exact host edits
without including unrelated changes in the live Firespider checkout. It must
be paired with the matching tracked `FirespiderLea.vb` adapter. Full host source
is also retained in the dated Firespider source-only backup ZIP.

## Verification

- Release Lea solution and .NET Framework Firespider builds succeeded.
- Eighteen focused event/session regressions passed against the rebuilt host.
- Full Lea.Tests and compiled Firespider integration suites passed.
- 58 fixtures and all 57 executable compatibility probes passed.
- Actual executable startup passed clean, restored, locked-session, and
  oversized-session checks with responsive windows and no detected exception
  dialog; the live root executable also passed its clean launch check.

The focused suite includes positive active-legacy navigation/progress checks,
sender ownership, detached/wrong-engine callbacks, private mode, malformed URL
skipping, twelve-tab restoration, and bounded saving. The executable test uses
temporary sessions; it does not prove every historical profile or legacy menu
path is error-free. Existing optional Shockwave/ClickOnce build warnings remain.

## Remaining Work

Firespider's legacy initial-page and explicit fallback paths still use the
Windows WebBrowser control. Lea tabs remain custom-rendered. Continue migrating
legacy-only routes and add regression cases for any new user-provided popup
trace. Text measurement caching remains a separate, unimplemented performance
experiment that requires controlled cache-on/cache-off measurements.
