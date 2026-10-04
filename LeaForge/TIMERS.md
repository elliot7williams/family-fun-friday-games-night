# Bounded Browser Timers

Experimental follow-up to milestone 146. Broader readiness stays about 65%,
not a standards score; the earlier intermittent startup/recovery failures
remain unresolved. Timers extend functionality, not proof of cold-start stability.

## Design

- `setTimeout`/`setInterval` return shared positive integer IDs; either clear API
  cancels either kind. Callback functions and arguments remain inside Jint's
  realm, including object identity, bound callbacks and asynchronous continuations.
- Parent-owned `Tq` holds at most 64 records and uses a monotonic Stopwatch.
  Selection scans the bounded table, ordering by deadline then scheduling order;
  interval rearming does not jump ahead of an already ready timeout. It neither
  creates a thread/native timer per callback nor uses the system wall clock.
- Each call to `Tk`/`TkA` dispatches one due timer. No due record means no new VM.
  The private dispatcher ends the timer task/rearms an interval before processing
  Promise/queueMicrotask jobs, so microtask-created timers start with fresh nesting.
  Synchronous nesting greater than five clamps short delays to four milliseconds.
- `Ja` validates the compact `ts`/`tc`/`te` capabilities in the parent. Timer IDs,
  source bounds, active count and the existing 256-call ceiling cannot be bypassed
  by a child-reported counter. Worker frame version 3 adds a strict integer timer
  task; previous version-2 frames are rejected before page execution.
- `Js/Jp.Td`, `Tn`, `Tk` and `Jp.TkA` support manual embedding. Native `Jt` is an
  optional interface, leaving the existing `Jx` contract and non-script providers
  unchanged. `W.On(view)` opts into automatic dispatch through the existing
  32-entry native queue. One lazy WinForms timer belongs to that binding and is
  stopped while queued work runs, preventing a repeating timer from flooding it.
- Completion uses existing retained-editor synchronization, repaint and `Xd`
  notifications. Waiting timers do not set Busy; a live interpreter still protects
  retained JavaScript state from Firespider's automatic hibernation. Binding
  retirement removes the UI pump and cancels/disposes the isolated realm.

The behavioral references are [HTML timers](https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#timers)
and the [global-scope Web IDL definitions](https://html.spec.whatwg.org/multipage/webappapis.html#windoworworkerglobalscope).
Lea's ceilings and failure policy are additional constraints, not a conformance claim.

## Limits

Default isolated execution stays 2,000 ms, maximum 5,000 ms, startup 5,000 ms,
128 MiB native memory, 25% CPU hard cap and one child per Job. Trusted `Js`
retains its one-second cooperative timeout; it is not a security boundary.
Timer callbacks, string code and microtasks retain the existing execution limits.
An uncaught callback failure drains its following microtasks, reports a bounded
failure, and discards the realm and its peers, retaining completed page writes.
This differs from the full browser error-event/continue model.

At most 64 callback arguments and 16 KiB of string-handler code are accepted.
String handlers use captured indirect eval: global `var` works, but `let`/`const`
remain eval-scoped. Full classic-script declaration/CSP/Trusted Types behavior,
module/external scripts, real origin policy, background throttling/suspension,
animation frames and general event-loop/error/rejection semantics remain work.
The Win adapter still refuses HTTP/HTTPS/UNC execution. Scheduling delay is a
minimum, not an accuracy, animation smoothness or performance guarantee.

Engine + Win + image binary bytes are 419,328 + 313,856 + 9,216 = 742,400,
1,536 bytes more than the previous required bundle. Engine and image binaries
are unchanged. No interpreter dependency is added to that required bundle.

## Verification

Logs are retained in `%TEMP%`:

1. Initial solution build succeeds with 18 warnings in unchanged prototype
   projects. `Lea-timers-initial-tests-build.log` builds the focused projects with
   zero warnings/errors. `Lea-timers-initial-tests.log` passes 151 checks.
2. `Lea-timers-coercion-red-build.log` builds with zero warnings/errors.
   `Lea-timers-coercion-red.log` reproduces the wrong handler conversion:
   concatenation preferred `valueOf` rather than the required string hint.
   Captured String conversion plus explicit Symbol rejection fixes that case.
3. `Lea-timers-final-build.log`: solution builds with zero warnings/errors on the
   incremental run; this does not mean the prototype warnings were repaired.
4. `Lea-timers-full-source-tests.log`: all 614 checks pass, including 163 timer
   checks and previous source/worker/event/microtask/view/permission regressions.
   Three first-path startup walls are 4,207 / 2,513 / 2,387 ms; one UI startup gap
   is 235 ms. These observations are not a latency benchmark or a fixed-startup claim.
5. `Lea-timers-core-tests.log`: full core suite passes, including native rendering,
   editing, network/input/image bounds and isolated image-worker regressions.
6. `Lea-timers-native-capture-build.log`: focused projects build without warnings/
   errors after making the screenshot helper perform layout and capture actual
   client bounds. `Lea-timers-final-focused-tests.log`: all 165 timer checks pass,
   including the two added viewport-size assertions. Captures at 940x820 and
   420x820 are nonblank and visually inspected; timer-written text, the Lea image,
   fields and controls render without the former padded capture strip.
7. `Lea-timers-current-full-source-tests.log`: the final source suite passes all
   616 checks, including all 165 timer assertions. Fresh-path startup walls are
   3,778 / 3,078 / 3,100 ms; the traced worker reaches DOM completion at 2,395 ms
   worker wall / 203 ms CPU. These source observations do not erase the separate
   Firespider failures below or establish reliable cold-start latency.

The timer suite covers deterministic deadline/FIFO/nesting/admission/reset rules,
both transports, coercion, captured helpers, argument identity, string/Promise
callbacks, peer/self/microtask cancellation, failure recovery, permission and
navigation retirement, automatic native delivery, editor synchronization,
Stop/disposal and a 25-ms native hard deadline with live UI ticks/no late writes.
The complete source suite at its current 616-check count passes. Fresh timer
package gates are not verified; the separate Firespider scripting gate fails.

## Firespider Gate

`dist/Firespider/Lea-146-timers-v8` compiles as a separate .NET Framework 4.8
candidate, leaving `L:\Firespider Internet` unchanged. Its EXE SHA256 is
`EA321C0950112B5AA64E57F4BDC87F4F7F47A71C1B348DEA4074A6F15E729F43`.
`Lea-timers-v8-candidate-build.log` retains the successful build.

`Lea-timers-v8-host-script.log` fails the first supplied-script call, before the
new host timer check: startup hello phase, launch 493 ms, probe 43 ms, total
5,026 ms wall / 1,640 ms CPU. `Lea-timers-v8-host-script-trace.log` retains a
second diagnostic failure: launch 460 ms, probe 56 ms, total 5,020 ms wall /
765 ms CPU. Worker trace reports entry, guards, init and VM initialization
(976 ms worker wall / 250 ms CPU), but no DOM-complete stage before retirement.
The evidence narrows the observed stage, not the cause, and does not justify
raising budgets or relaxing native protections.

The v8 host timer assertion, full 43-case host matrix, six actual-EXE startup
scenarios, extracted candidate and independent host-source rebuild are not
verified. Earlier v7 passes are historical evidence only. No v8 release ZIP
is published from these failed gates; the separate candidate is diagnostic,
not a finished browser replacement.

## Commands

```powershell
dotnet build Lea.sln -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --timers
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
.\Lea.Tests\bin\Release\Lea.Tests.exe . --core
```

See [startup diagnostics](STARTUP_DIAGNOSTICS.md),
[native integration](NATIVE_JAVASCRIPT.md) and [worker boundaries](../Lea.Script/README.md).
