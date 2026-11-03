# View Load Lifecycle

Milestone 65 / runtime 165 hardens asynchronous navigation and Firespider's
request tracking without adding a browser dependency.

## Compact Admission

`V.Cn` means "can navigate": the view is open and neither a view-owned page
load/download nor the engine's current operation is pending. Call it and all
view operations on the view's UI thread. It is an admission hint, not a lock
for concurrent callers.

```csharp
if (view.Cn) await view.GoA(address);
```

`Busy` includes background picture work as well. Pictures from a previously
shown synchronous page do not by themselves make `Cn` false: a new navigation
can replace that page. During an asynchronous page load, ownership covers
fetching, the queued presentation continuation, and that page's picture pass.

`Go/GoA`, POST, reload, history, fragments, and downloads reject an overlapping
page operation with `InvalidOperationException`. Rejection does not reset the
active owner's cursor, page, failure state, or request. Native input consumes
this expected busy rejection without swallowing unrelated programming errors.
`Ry` retains its existing no-op behavior when retry is unavailable.

## Completion And Stop

The view releases its owner before `Done` or typed failure notification `Fe`,
so a handler may start another load. The old operation's `finally` only clears
its own token. Synchronous completion also preserves an asynchronous successor's
cursor. A page's pending `go()` command is not dispatched after its `Done`
handler starts a successor or changes the current page/address.

`At` may already hold the engine's newly committed page before the view has
finished presenting it. Await the returned task for the whole view operation;
do not infer completion from `At != null`. `Done`/`Fe` handlers themselves can
start more work, so recheck state before another request.

`St()` requests engine/resource cancellation. Wait for completion before starting
another operation; Stop is not an immediate ownership reset or a rollback.
If the document has already been obtained, it may still be presented, but the
stopped picture pass is not restarted by presentation. Disposed views publish
no further normal load notifications.

Picture passes share one WebClient in sequence. Replacing a page cancels its
pending picture request; page identity and a generation counter reject stale
successes and failures before they enter the cache. The new page can then fetch
its own pictures without a concurrent-WebClient error being cached as failure.
This does not add HTTP image deadlines, streaming limits, or new format support.

## Firespider Ownership

Use the matching adapter and `Lea.Win.dll`; runtime 165 requires `Cn`. No new
patch to the other Firespider source modules is required for this milestone.
Prior startup/style patches still apply to a fresh integration.

Each accepted navigation, retry, or reload has a unique host request token and
pending target. A completion callback can replace that token, including when a
page's Lea command opens the next page. Old cleanup cannot delete the successor's
pending target, queued native CSS, or loading status. An overlapping address
request is ignored before changing those values. New requests discard stale
previews; failure, completion, close, and disposal release owned bookkeeping.

## Checks And Limits

- `Lea.Tests <root> --loads`: step-controlled UI completion, sync/async overlap,
  downloads, native input rejection, success/failure callback navigation, stale
  page commands, cursor retention, disposal admission, and gated picture handoff.
- `Test-FirespiderBoot.ps1 -Root <runtime> -Case load-life`: compiled host with
  two gated pages, page-command navigation, queued CSS, overlapping address
  input/reload, retained fields, reload previews, final bookkeeping, and no
  MSHTML/UI exceptions.
- The benchmark includes the load-lifecycle fixture and a `Cn` admission probe.

The readiness gap, stale image failure, sync callback cursor reset, and host
successor bookkeeping were reproduced before their respective fixes. The
original host test's later color assertion used a fragment without a body
element; the named sheet was present. That test fixture now has an explicit
body, and separate assertions distinguish title, sheet count, color, and fields.
The full core suite also exposed a title-only click wait immediately starting
another navigation at `Smoke.cs`'s inline-link check. Click/host test waits now
require both the expected title and full view readiness; they do not relax
the navigation guard.

This is not a claim to have fixed every startup issue. Earlier intermittent
fragment placement, recovery-time legacy casting, and editor/clipboard/focus
assertions remain unconfirmed unless separately reproduced and attributed.
Short aliases and archive compression reduce API/package size; they do not
establish a runtime speed improvement. Use measured timings, not name length,
to assess performance. Lea's 100% progress remains the scoped 1.0 baseline,
not complete modern-web compatibility.

## Release Validation

Release core, isolated native editing, and full compiled Firespider host tests
pass. All 65 benchmark probes pass across 62 fixtures. The 38 Boot cases pass
in one combined run, including the new successor/reload regression. Six actual
EXE startup scenarios pass: clean, restored, locked, oversized, lazy-background,
and lazy-all with on-demand resume. No startup error dialog or MSHTML was
observed in those native EXE checks; windows remained responsive.

The DLL total is 495,616 bytes (484 KiB), 1 KiB below runtime 164. The diagnostic
GZip estimate is 204.8 KiB. Local medians were 611.822 ms for a clean process,
engine and first page; 3.519 ms for local page loading; and 31.925 ms for layout
and bitmap paint. These are unpaired local measurements, not evidence that the
change is faster. See `COMPATIBILITY_PERFORMANCE.md` for sampling and limits.

Runtime 165 is distributed as a separate ZIP/test directory with source backups.
Installed runtime 157's root EXE and DLLs are not replaced. The live Visual Studio
adapter, `Lea` references, and `bin/Release` output contain the new build.
