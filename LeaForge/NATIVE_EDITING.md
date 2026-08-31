# Lea Native Field Editing

Milestone 59 adds `V.Ce(code)` to query a field command and `V.Ec(code)` to run it.
Commands are case-insensitive, exactly two ASCII letters, and run on the view's
UI thread. `Ec` returns 1 when dispatched, or 0 when unavailable. Unknown codes,
missing/disabled/hidden fields, stale retained values, and disposed views return 0.
Milestone 63 adds guarded clipboard paste and clipboard-free `V.Pa(text)`;
see `PASTE_SAFETY.md` for the reproduced data-loss fix and exact bounds.

| Code | Command |
| --- | --- |
| `cp` | Copy selected text |
| `cu` | Cut selected text |
| `pa` | Read a bounded clipboard snapshot, then insert through the native control |
| `sa` | Select all field text |
| `de` | Delete selection or the next character |
| `un` | Undo the last native edit |
| `re` | Redo that edit |

The existing single native TextBox remains the editor. No new text engine,
per-field history collections, or browser dependency is added. Windows supplies
the native text operations and one-level undo buffer; Lea guards clipboard
reads and paste bounds. Ctrl+V/Shift+Insert share that guarded path. Ctrl+Z
and Ctrl+Y share the undo/redo state. New edits invalidate redo; changing the
field or page clears the buffer. This is not multi-level document undo.

Menu focus loss retains the last editor's selection and native buffer, without
keeping it visible. A command revalidates the live field and its value before
reactivating it. Navigation, keyboard target changes, and disposal release the
retained target. Read-only fields allow selection/copy only; password fields
refuse copy/cut without changing the clipboard or field. Delete expands partial
UTF-16 surrogate boundaries. Ordinary native typing behavior is unchanged.

The host-only commands are not exposed to page scripts. `Ce("pa")` checks the
editable target without inspecting the clipboard. An unavailable/empty/non-text
clipboard now returns 0 without replacing the selection. Unexpected host or native-control
exceptions may still propagate to the caller; Firespider reports a bounded status and
uses its existing error log. Hosts should call these APIs from the UI thread.

## Firespider Fixes

The previous runtime reproducibly failed the real Copy command immediately
after startup with `InvalidCastException` in `j.c9`, casting `Lea.Win.V` to
`WebBrowser`. All seven Edit menu commands and their hover routes now dispatch
by the selected engine. Paused and empty tabs are inert; an explicitly selected
legacy tab retains its document editing path. No host patch beyond the updated
tracked `FirespiderLea.vb` adapter is needed for this milestone.
Address/search focus makes these menu commands inert instead of accidentally
editing the retained page field; full toolbar-menu editing remains future work.

The regression also found `ArgumentException` from `Font.GetHeight` when
switching equal-font fields, especially text inputs and textareas. WinForms can
retain the previous equal Font instance in its property setter. Lea now reuses
that owned font instead of disposing an instance still used by its editor.

## Evidence And Remaining Work

`editing.html` and the core suite cover aliases, selection, clipboard, menu
focus, live values, one-level undo/redo, keyboard routing, read-only/password
protection, maximum-length/multiline paste, Unicode deletion, stale fields,
hidden targets, navigation, and disposal. The `edit` Boot case exercises actual
menu wiring, drop-down focus, hover settings, paused/empty tabs, and explicit
legacy fallback. The clipboard is materialized before testing and restored.
The compatibility runner adds a clipboard-free native editing probe.
The recovery Boot case now also enables its real snapshot timer and pumps UI
events for 35 seconds, checking that the 30-second save retains both Lea URLs.

The separately observed intermittent recovery-time cast remains unidentified.
Twenty additional runtime-158 recovery runs passed; this does not prove the
issue fixed or connect it to `j.c9`. Full exception stacks remain enabled in
the Boot harness. Runtime 159 is an isolated test package, not a claim that all
startup errors or legacy command routes are fixed.

Document-body text selection, contenteditable, multi-level history, native Lea
printing, and general Edit-menu dispatch into browser address/search controls
remain separate work. Their ordinary native keyboard editing still works.
No claim of arbitrary modern-web compatibility or runtime speedup is made.
