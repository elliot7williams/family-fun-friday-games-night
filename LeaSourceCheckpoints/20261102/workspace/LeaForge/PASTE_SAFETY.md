# Lea Guarded Paste

Milestone 63 fixes a reproduced data-loss bug in the native editor. Against
runtime 162, a worker held the Windows clipboard open while `Ec("pa")` ran.
Paste deleted the selected field text even though no clipboard data could be
read. Copy and Cut preserved the field under the same lock. The final `--clip`
regression also detects that failure when run beside the runtime 162 DLLs.

## Compact API

`V.Pa(text)` inserts host-supplied text into the active or menu-retained editor
without accessing the clipboard. It shares field validation, text bounds, and
native undo with clipboard paste. It returns 1 for a dispatched insertion, or
0 for an unavailable target or a prefix that contains no insertable text.

```csharp
int inserted = view.Pa("Text supplied by the embedding application");
```

`V.Ce("pa")` remains a target-only availability query. It does not inspect the
clipboard. `V.Ec("pa")` reads a guarded clipboard snapshot before inserting.
An unavailable, empty, or non-text clipboard returns 0 without deleting the
selection. No new alias is exposed to page scripts.

All calls belong on the view's owning UI thread. Missing, disposed, hidden,
disabled, read-only, stale, or no-longer-text editors are rejected. Password
fields may receive text but retain their Copy/Cut restrictions. Command
eligibility checks the actual rendered field instead of relying on the
256-item keyboard navigation list; mouse-edited fields beyond that list work
without increasing the keyboard list's bound.

## Shared Native Path

The single editor remains a native Windows TextBox. Its small subclass routes
`WM_PASTE`, including the native context-menu message, through the same guard.
Ctrl+V and Shift+Insert are consumed before dispatch so they cannot fall through
to an unguarded native paste. Firespider's existing Paste menu reports an
unavailable command through its normal status path without a new host patch.

Clipboard access is read-only. Lea attempts to open it at most three times,
with two 5-ms waits for brief contention; there is no retry after a mutation.
Windows scheduling can extend those waits. An available clipboard takes the
first path without waiting. The clipboard is closed and its memory unlocked
before the editor changes. This follows the ownership requirements of
[OpenClipboard](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-openclipboard).

Only a bounded Unicode text snapshot is read, including Windows' standard
text-format conversion. At most 32,768 UTF-16 code units are copied from the
clipboard allocation, allowing one lookahead unit at the native 32,767-unit
field ceiling. No full arbitrary-sized clipboard object is materialized by
the paste implementation, and no clipboard text is logged or persisted.

Insertion respects the remaining `maxlength` capacity after replacement of
the selected range. Single-line fields stop at CR/LF; multiline fields retain
line breaks. NUL terminates supplied text. Truncation does not split an incoming
surrogate pair or CRLF. An empty normalized prefix or exhausted capacity is
inert. This is not a general grapheme editor or document-body selection engine.

Windows performs the insertion and maintains its one-level undo buffer through
[EM_REPLACESEL](https://learn.microsoft.com/en-us/windows/win32/controls/em-replacesel).
Lea does not revisit the native control after that message, so an edit callback
can dispose the view. New edits clear redo as before. Rejected paste leaves
the selection, field value, and native undo buffer intact.

Clipboard owners can render data synchronously. After retrieval, Lea rechecks
the page, editor, value, selection, and field policy before applying anything.
Owner callbacks that navigate, change the value or selection, or dispose the
view cannot cause a stale snapshot to overwrite the replacement state.
Windows/clipboard-producer latency is not a guaranteed UI-thread deadline.

## Verification And Limits

`Lea.Tests.exe <root> --clip` covers the held-clipboard regression, native paste
messages, shortcuts, selection/undo retention, and clipboard-independent `Pa`.
`--paste` covers supplied text, bounds, protected fields, late form fields,
navigation, and edit-callback disposal without accessing the clipboard.
`--edit` also covers normal/empty/non-text/ANSI/large clipboard contents,
exactly one edit notification, undo/redo, and real delayed-owner callbacks.
The default suite includes these checks; `--core` excludes clipboard checks.

The Firespider `paste` Boot case exercises the real menu, status, native message,
keyboard route, undo/redo, release recovery, and read-only protection. The Boot
suite contains 36 cases, including the saved-style startup guard added in this
milestone. A clipboard-free `Pa` compatibility probe brings the
runner to 63 probes across 60 fixtures.

The separate intermittent recovery-time Lea-to-WebBrowser cast and initial
fragment-placement failure remain open. This fix establishes the cause of the
reproduced locked-clipboard deletion, not every earlier editor/test failure.
An additional saved-style automation cast was reproduced and fixed; see
`STYLE_STARTUP_SAFETY.md`. Its stack does not establish the cause of the earlier
stackless recovery observation.
Clipboard-writing test fixtures can still be affected by other applications.
Runtime 163 remains an isolated test build; the installed runtime 157 is not
replaced. Full modern-web compatibility remains unfinished.
