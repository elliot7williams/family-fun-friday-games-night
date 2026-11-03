# Native Event Microtasks

Milestone 146, October 3, 2026. Broader lightweight-browser readiness remains
about 65%, a planning estimate, not a standards conformance score. Native
Promise/async continuations and `queueMicrotask` now participate in Lea's
bounded script/event completion. This is progress toward a browser event loop,
not a completed general-purpose web engine.

The later [startup follow-up](STARTUP_DIAGNOSTICS.md) records scheduling,
cancellation and file-permission improvements, final v7 verification, and the
unresolved intermittent deadline. The v4 archives listed below remain retained
historical artifacts, not the latest candidate or a finished installed update.

## Implementation

A regression reproduced native event completion before its queued Promise
DOM writes. Jint's script execution drains jobs, but its function invocation
does not provide that checkpoint automatically. Lea's private event dispatcher
now finishes propagation and event cleanup, captures synchronous cancellation,
then drains Jint's queue while the same invocation's constraints remain active.
See the pinned [Jint invocation implementation](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Engine.cs)
and [task-processing API](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Engine.Advanced.cs).

`queueMicrotask(callback)` uses a private native async function with an initial
`await 0`, joining Jint's own continuation queue only at admission. Bootstrap
performs no public Promise/Reflect helper lookup. Callbacks run
after synchronous work in FIFO order with Promise jobs, and nested jobs join
that queue. No alternate Promise parser/implementation or core/view interpreter
dependency is added. Host commands stay `Ex`/`ExA` and `Ev`/`EvA`; the primitive
bridge admission command is `qm`. Raw bridge/checkpoint functions are not
published to page code.
The pinned [Jint await implementation](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Runtime/Interpreter/Expressions/JintAwaitExpression.cs)
queues the primitive continuation in the same event loop; no second FIFO or
private-library reflection is used. The async function expression is instantiated
inside admission, not while bootstrapping a page that has no microtasks.

Admission is capped at 128 `queueMicrotask` callbacks per execution, including
recursive scheduling, and also consumes the shared 256-call bridge budget.
Promise jobs retain aggregate interpreter execution/memory constraints and
the isolated parent's hard deadline. VM constraints are not re-armed between
the event handler and its checkpoint. Callback errors allow following queued
jobs to drain, report the first error, retain completed writes and discard the
realm. Synchronous script/event failure discards pending work immediately.

The native fixture now uses deferred counter updates, Promise field updates,
queued first-block initialization, DOMContentLoaded updates and Promise load
updates. Those writes must reach mutation counts, painting, native editors and
host completion rather than wait for a later script call.

## Verification

- The initial event/Promise regression fails against the old implementation.
- The first focused microtask suite passes 62 checks across trusted and
  isolated transports. Further cases cover native callback failures and
  discarding jobs after synchronous event failure.
- Release solution build succeeds with 18 existing warnings in older
  experiments and no errors. The final focused test build has no warnings
  or errors.
- The focused worker suite passes all 84 checks, including native-regex work
  inside a queued callback killed by the parent's 25 ms deadline, no late DOM
  writes, fresh-worker recovery, isolation and UI responsiveness.
- An initial full-suite run stops at the independent second-session assertion.
  That assertion originally omitted the status/message. It now records both;
  the detailed focused worker run passes with the unchanged two-second default.
  Do not claim this establishes a cold-start fix or proves that earlier failure
  was a particular status. Cold-start reliability remains a tracked limitation.

- The final full source suite passes all 382 checks, including the expanded
  microtask cases, both transports, retained native editor synchronization,
  lifecycle ordering, isolated cancellation and queue/ownership regressions.
  The added second-session diagnostics remain in place; the initial failed
  run is not treated as proof of a particular underlying cause.
- All 58 focused native-view checks pass. Desktop and narrow screenshots are
  nonblank, include the queued native field value, and were visually inspected.
- The full core regression suite passes. Required engine/view/image source
  is unchanged; interpreter scheduling remains an optional module.

The installed browser is not overwritten by these builds. Host/package gates
are verified separately from the source suite.

### Startup investigation

The initial enabled-host matrix passes 42 cases, then its script case times out
on first supplied code. A focused rerun reaches the split-pane loading wait
before failing; a diagnostic rebuild identifies a subsequent first-demand
failure as `Worker startup deadline exceeded.` The old generic message could
not distinguish this from the execution deadline.

The previous milestone 145 candidate passes the same diagnostic host case;
first demand takes 4,544 ms in total and split loading takes 6,339 ms in that
single control run. These are observations, not a controlled speed benchmark.
The eager resolved-Promise/bound-function scheduler has been replaced by
captured helpers with lazy Promise creation. A regression checks first use
with replaced public Promise/Reflect helpers. Startup and execution budgets
are unchanged. Subsequent verification is required before claiming a fix.

The lazy candidate then passes the focused host script case. Observed first
demand is 5,015 ms in total (startup plus subsequent requests); split load wait
is 4,960 ms. The focused microtask suite passes 76 checks and the final full
source suite passes 386, including both hard-deadline message assertions.
The last incremental Release solution build reports zero warnings/errors.
These passes support the revised implementation, not a general cold-start or
speed guarantee. The full host matrix and actual-EXE startup gates follow.

The complete lazy-candidate host matrix still passes only 42 of 43 cases;
first demanded code again reports a startup timeout. Lazy resolved-Promise
creation therefore does not establish a fix. The scheduler is now a native
async expression instantiated only within `queueMicrotask` admission, avoiding
Promise/Reflect property reads and async-function creation during bootstrap.
Its focused suite passes 76 checks before the final placement adjustment;
final build/host/package verification is recorded separately below.

The final admission-time implementation builds successfully with zero
warnings/errors and passes the complete 386-check source suite, including
the 76 microtask cases and both independent hard-deadline assertions. Neither
the startup deadline, execution ceilings nor native Job restrictions is raised.

The final v4 candidate passes all 43 host Boot cases, including the previously
failing scripted host path. First demand takes 5,285 ms in total across startup
and subsequent lifecycle/code requests; split-load wait takes 4,661 ms in that
run. This does not imply a 5,285 ms startup handshake or prove general latency
or cold-start reliability. Main/split screenshots are nonblank and inspected.
All 15 short-option mappings across seven workflow scripts are verified from
parameter metadata; actual EXE startup uses the `-r`/`-s` aliases.

## Final Gates

- All six actual v4 EXE startup scenarios pass: clean, restored, locked,
  oversized, lazy-background and lazy-all. Windows remain responsive and no
  JIT/error dialog or legacy engine load is detected. Observed readiness is
  4,375..6,822 ms, not a controlled speed benchmark.
- The script-free candidate compiles without warnings/errors and passes clean,
  split, hibernation and lazy-all cases through the short options. Its full
  42-case matrix is not repeated. PE metadata and file inventory show no
  interpreter/adapter dependency. Required engine/view/image binaries remain
  739,840 bytes.
- A fresh complete core regression run passes.
- All five archives pass exact inventory, per-file SHA-256 and length audits:
  370 enabled runtime, 104 enabled host-source, 341 small runtime, 76 small
  host-source and 40 standalone native files. Native ZIP separators are
  normalized for manifest comparison. The drive-root output bug found during
  packaging is fixed and both host modes package successfully to `F:\`.
- The extracted native package passes all 386 checks. Its separate browser
  smoke passes inline initialization/counter activation with the unchanged
  2,000 ms default, verified isolation and no legacy browser modules. Its
  screenshot is nonblank and inspected. Binary/configuration files total
  7,941,787 bytes across 23 files, excluding icon/fixtures/notices.
- The extracted enabled host-source project rebuilds independently without
  warnings/errors; its saved framework/module settings need no override.
- The extracted enabled runtime's script case fails at first demanded code
  with `Worker startup deadline exceeded.` This contradicts any claim of a
  complete cold-start fix. A passing build-folder matrix does not establish
  fresh-path reliability. The archives are experimental, not a recommended
  installed replacement; existing candidates and the installed build remain
  preserved. Diagnose launch/handshake costs before claiming reliability or
  changing the execution/sandbox limits.
- The extracted enabled EXE's clean-startup scenario passes: responsive native
  Lea window, no detected error dialog and no legacy engine module. Readiness
  is 4,324 ms in that run. This script-free pass does not cancel the scripted
  startup failure above.

## Artifacts

Separate v4 candidate:

```text
C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\dist\Firespider\Lea-146-2026-10-03-v4\runtime\Firespider Internet 19.exe
SHA256 36E9871A956880A594C3CD2E4546E492867FEAB68E3D2A661F6897CC645FDB44
```

The small candidate is in `dist\Firespider\Lea-146-small-2026-10-03\runtime`:

```text
SHA256 66BDFB59015DAB7B599B773E5B04FE0DA48464E1E9EAF1F6AE73E088AABACA8E
```

Experimental distribution archives:

```text
F:\Firespider-Lea-146-script-runtime_2026-10-03_19-51-53.zip
Bytes 3620402; files 370
SHA256 71D4075815850C73AEA22E28AAFFC2190C8618433D70B3089A512C485901D519

F:\Firespider-Lea-146-script-host-source_2026-10-03_19-51-53.zip
Bytes 3282187; files 104
SHA256 BD425F5E5C12B86C7FA8683204C9BF526AB34DF7B2347F436B1ED163328C45F9

F:\Firespider-Lea-146-small-runtime_2026-10-03_19-53-04.zip
Bytes 1098122; files 341
SHA256 32473B3B37A8C6B52AF68D981A078C4111C67F68EACC29AB0CAC8C3319590017

F:\Firespider-Lea-146-small-host-source_2026-10-03_19-53-04.zip
Bytes 768047; files 76
SHA256 C185DDCDFD493F597ABCA1A73C71C4948C4059091DACC29CB156C624588BE25D

F:\Lea-js-146-native_2026-10-03_19-53-53.zip
Bytes 2895204; files 40
SHA256 8262CA8B55105BADDE8259CF05F21F54EA0360A53E42A15F63893AB01D44FEE5
```

Complete source snapshots are retained separately with `Backup-Lea.ps1` and
`Backup-Firespider.ps1`, in `F:\Lea Source Backups` and
`F:\Firespider Source Backups`. The buildable host-source ZIP includes Lea
binaries, not all Lea C# source; keep the complete Lea source snapshot too.

## Limits

This is one checkpoint after the bounded native dispatch turn, not HTML's full
per-callback cleanup/event-loop behavior. The synchronous cancellation result
is fixed before the deferred callbacks. `queueMicrotask` callback errors follow
Lea's reset-on-error policy, not complete browser error-event semantics.
Native Promise rejection handlers work, but unhandled-rejection notification,
timers, animation frames, external scripts, broader DOM APIs and general
modern-site compatibility remain unfinished. HTTP/HTTPS/UNC script execution
still refuses without starting a worker.

The adapter/demo keep their 2,000 ms default; the separate Firespider candidate
and functional native tests use the documented 5,000 ms ceiling. These are
limits, not speed guarantees. Compression only reduces distribution size.
Worker launch requires local NTFS/ACL-capable storage; F: stores archives only.

See the [HTML microtask API](https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#microtask-queuing),
[scripting API and boundaries](../Lea.Script/README.md), and
[separate host build instructions](../Lea.Firespider/CANDIDATE.md).
