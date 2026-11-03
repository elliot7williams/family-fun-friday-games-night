# Lea Navigation Commands

Milestone 53 continues the Firespider migration without changing the engine DLLs
or public short-alias API. The matching adapter and `Navigation-host.patch`
extend milestone 52's host source.

## Home, Sites, And Tabs

- Lea's Home toolbar button, menu, and hover route honor the saved homepage.
  An empty setting falls back to the bundled Lea start page. The old Home menu
  resource was both hidden and disabled; the adapter now enables it.
- Use Current Page As Home saves a supported Lea or legacy URL. Open Homepage
  In New Tab creates a Lea view. Explicit legacy Home actions retain their
  original handlers.
- Quick Open Current Domain navigates the selected engine to its HTTP(S) root.
  Open Current Domain In New Tab uses Lea; Set Homepage To Current Domain and
  Copy Current Domain use the same URI helper. Ports and IPv6 are preserved;
  path, query, fragment, and credentials are not copied into the root URL.
  File and non-web URLs do not produce a domain or overwrite the homepage.
- Copy Page Link reads either engine's URL. Tab-list selection updates Lea's
  address/title state and ignores stale, disposed, or detached tab entries.
- Both left-close menu variants now share the same `a/l/r` group-close helper
  as close-other/right. They preserve selection and right-hand tabs, dispose
  removed controls, maintain counts and legacy widths, and retain the existing
  private-history exclusion.

## Previous Sessions

The explicit previous-session command appends saved Lea URLs and selects the
first new tab without replacing the current page. A missing, empty, or entirely
invalid Lea snapshot falls back to the legacy saved URL list, opened as Lea.
Locked or oversized Lea files report failure without falling back to stale
legacy data. Neither successful nor failed opening truncates the saved file.

The shared session reader accepts at most 64 KiB and 256 lines. Both automatic
and explicit restoration examine at most 256 candidate URLs, accept at most
twelve tabs and 64 KiB of encoded URL data, and reject unsupported schemes or
URLs longer than 4,096 characters. Automatic private startup remains disabled;
an explicit previous-session open in private mode creates private Lea views.

## Clipboard Opening

Single-link opening accepts supported absolute HTTP(S)/file URLs, including
file URLs containing spaces, and the host's existing bare-link notation. It
does not turn rejected input into a search. Multiple-link opening extracts
case-insensitive HTTP(S) and `file:///` URLs from text using the standard regex
engine and the same bounded restore route. It preserves order and reports the
number opened and skipped; repeated authored URLs are not deduplicated.

Input exceeding 65,536 characters or 64 KiB of UTF-8 is rejected as a whole,
before regex matching. The Windows clipboard retrieval itself is still the
framework operation, not a streaming or size-limited OS read. Batch file URLs
must encode spaces, since whitespace separates extracted links. This is URL
extraction, not an HTML parser. These explicit commands work in private mode.

## Verification

`Test-FirespiderBoot.ps1 -Case navigation` exercises real Home, tab-list,
previous-session, and both mixed-engine left-close menus. It checks snapshot
precedence, append/order, private opening, tab and input limits, retained saved
records, locked/oversized/empty inputs, disposal, history, and stale selection.

`-Case links` uses a loopback HTTP server for real site-root navigation and
new-tab opening. A stored-tab URL tests IPv6/port/credential normalization.
Clipboard parsing/opening is exercised through its shared helper with fixture
strings, not by reading or overwriting the user's clipboard. Copy-to-clipboard
OS calls themselves are not automated here.

The launcher keeps the child process handle and waits for redirected output
before checking the exit code and mandatory success marker. This fixes the
observed Windows PowerShell missing-exit-code failure. Test settings restoration
now also runs if form disposal throws during a failed test.

The final build passed all fourteen Boot cases, the full engine and compiled-
host suites (including eighteen startup-event regressions), all 57 compatibility
probes, and clean/restored/locked/oversized real EXE startup checks. Those EXE
checks observed responsive Lea windows for at least eight seconds without
MSHTML or detected startup error dialogs. Applying the host patch to the
milestone 52 source baseline reproduces the live `1.vb` exactly apart from
line-ending normalization.

## Remaining Work

Advanced group/session/window routes, same-domain bulk commands, and many
legacy DOM-dependent commands still need migration. This milestone does not
claim a Lea-only host, complete legacy privacy, full modern-web compatibility,
or a general JavaScript VM. Engine performance and compression characteristics
are unchanged; smaller names or archives do not establish a runtime speedup.
