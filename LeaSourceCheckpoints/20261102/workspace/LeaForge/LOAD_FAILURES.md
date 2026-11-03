# Lea Native Load Failures

Milestone 61 adds four compact WinForms aliases without another browser runtime:

| Alias | Meaning |
| --- | --- |
| `V.Fe` | Event after an ordinary typed document-loading failure |
| `V.Le` | Last `Er`, or null after a new load, successful navigation, cancellation, or disposal |
| `V.Cr` | An explicit failed-GET retry is available and the view is idle |
| `V.Ry()` | Asynchronously retry that GET, or return the current page unchanged |

## Contract

Use the view on its owning WinForms UI thread and await asynchronous calls.
`Fe` uses the owner thread when a control handle is available. It runs after
engine request cleanup, with `Le` already set. Public navigation APIs continue
throwing the same typed `Er`; subscribing does not turn failures into success.
Handlers may navigate or dispose the view. Disposal releases `Fe` subscribers
and the saved failure. Handler exceptions and unexpected non-`Er` failures
are not reclassified as network errors.

Failed navigation retains the existing document, edited fields, and history
position. It does not emit successful `Changed`/`Done` events. A failed first
page remains empty. No automatic retry, error-page renderer, or POST-body
retention is added. Cancellation does not emit `Fe` or offer Retry.

`Ry` is an explicit user retry of an absolute HTTP, HTTPS, file, or about GET.
It checks `Ng` again. It can retry a file or HTTP error after the resource is
fixed, independently of `Er.Rt` (the transport's transient-error hint).
Invalid addresses, POST/multipart failures, and denied Back/Forward navigation
do not create a retry target. An ineligible `Ry` does nothing. Download and
subresource failures are outside this document-load state.

## Native Input And Host

Runtime 160 reproducibly let HTTP 503 escape from keyboard link activation
through `V.OnKeyDown`. Enter and assigned access keys now consume the key before
awaiting the network. Native keyboard, editor access-key, and accessibility
activation handle typed `Er` failures at their event boundary; public `Ka/Aa`
still report them. Existing mouse/command-key boundaries retain their behavior
and page loads publish the same failure state.

Firespider subscribes each mounted view, preserves selected-tab error status
across timer ticks/background completion, and uses Retry from its existing
Reload command. A failed first GET keeps its address for session persistence.
Split panes report bounded errors through their existing status label; the
small Lea test browser also subscribes and uses Retry from Reload.

## Verification And Limits

Core tests cover failure identity/count, retained page/history, retry policy,
file recovery, GET versus POST/multipart, async calls, cancellation, disposal,
and navigation/disposal from a failure callback. The compatibility runner adds
a local failure/retry probe without an internet dependency.

Boot `failures` checks delayed keyboard/editor/accessibility HTTP failures,
early key consumption, UI-thread notification, retained edits/status,
background completion, and the actual Reload menu. `failure-start` restores an
unavailable saved URL beside a local page, retains the failed address/session,
then recovers through Reload. Existing startup checks remain required.

The intermittent recovery-time Lea-to-WebBrowser cast remains unidentified.
This is a separate confirmed input crash fix, not proof that every startup
error is fixed. Runtime 161 is an isolated test package; the live root runtime
157 is intentionally unchanged. Full modern-web compatibility, a security
sandbox, and a general JavaScript VM remain outside the current engine.
Archive compression reduces distribution size, not execution time by itself.
