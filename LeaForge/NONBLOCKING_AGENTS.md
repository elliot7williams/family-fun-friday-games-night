# Nonblocking Page Agents

The optional scripting configuration now sets `AgentCanSuspend=false` through
the existing short `Vo.Of/At` helpers. This applies to both trusted `Js` and
isolated `Jp`: their page code has a Window realm, even when hosted in a
separate OS process. An OS worker is not a JavaScript DedicatedWorker realm.
No public Lea API, interpreter package, bootstrap source or required renderer
changes.

The [HTML agent model](https://html.spec.whatwg.org/multipage/webappapis.html#integration-with-the-javascript-agent-formalism)
does not allow Window agents to block on atomic waits. The pinned
[Jint options](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Options.cs)
default to allowing suspension; its
[wait implementation](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Native/Atomics/AtomicsInstance.cs)
checks that option before entering the blocking wait. Using that native option
keeps validation and errors in the proven interpreter, rather than replacing
or wrapping `Atomics` in page JavaScript.

## Behavior

`Atomics.wait()` now throws a realm `TypeError` for otherwise valid shared
waits, including zero/negative/finite/omitted/NaN/infinite timeouts and a
mismatched expected value. Existing argument validation still precedes that
guard: invalid typed arrays/sharedness throw `TypeError`, and an invalid index
throws `RangeError`.

Atomic load/store/add/compare-exchange/notify still work. Nonblocking
`Atomics.waitAsync()` immediate results and notify-driven Promise wakeups are
retained. A caught forbidden wait leaves subsequent JavaScript, microtasks,
timers and animation usable. An uncaught wait is a cooperative JavaScript
error; normal fresh-realm recovery remains nonblocking. Permission revocation
still prevents execution.

This closes a page-freeze risk, not every possible interpreter/native blocking
path. Shared buffers remain interpreter-local; this does not add Web Workers,
cross-page memory sharing, cross-origin isolation or full shared-memory web
platform conformance. If dedicated JavaScript workers are added later, they
need a separate declared agent policy rather than enabling suspension for
Window pages.

## Verification

The old byte-verified runtime executes a finite 300-ms wait and returns
`timed-out` after 330 ms. No indefinite old-runtime wait is attempted. The
patched finite control instead throws `TypeError`; its 108-ms observed call
includes cold interpreter work and is not a startup/performance benchmark.

| Check | Actual Result |
| --- | --- |
| Old finite control | Exit 0, 3 assertions, default suspension reproduced |
| Patched finite control | Exit 0, 3 assertions, wait rejected |
| Agent core / trusted / isolated / native | Exit 0: 9 / 15 / 16 / 9 assertions |
| VM options | Exit 0: 16 assertions, existing budgets and CLR/stack restrictions retained |
| Clock core / trusted / isolated / native | Exit 0: 57 / 14 / 16 / 10 assertions |
| Timers / worker | Exit 0: 165 / 87 assertions |
| Bootstrap assets / compiled wire | Exit 0: 4 / 210 assertions |
| Startup-phase regression | Exit 1 after 9 assertions, traced hello timeout |
| Invalid `-aw` arguments | Expected exit 1 before execution |
| Preceding worker | Expected exit 1 at the compiled nonblocking-option assertion, before any page execution |
| Observer expected-count/footer assembly | Six AST-selected checks pass; no native observer launch |
| Full scripting regression | Exit 1 after 2,375 assertions: Lea.Win native CPU-lifetime fixture |

The startup failure retains `phase=hello; wall=5006; cpu=46 ms`; its last
reported marker is `startup vm: wall=416; cpu=46 ms`. VM construction returned
in this attempt. This identifies a later boundary than the previous
constructor-stage traces, but asynchronous diagnostic delivery does not prove
the exact blocking operation or cause. The trace case deliberately enables
timing inside its fixture; controller `traceEnabled=false` records mean no
external opt-in trace. Failed evidence remains unchanged.

The new agent suites add 49 full-run checks and option guards add two; the full
suite now expects 2,815, versus 2,764 previously. Worker-only observation still
expects 87. Independent suite outcomes are not substitutes for a full result.

The one full run fails at the unchanged native CPU-lifetime fixture: helper
exit `0x00000003`, independent total process CPU 140 ms, no external trace or
observer. In `NativeCpuWorker.cs`, exit 3 means its loop did not observe 100 ms
of additional CPU within the unchanged three-second wall limit. The 140-ms
independent total includes startup and is not that loop's delta. The underlying
cause remains unestablished; no fixture/deadline/rate gate is relaxed or
retried. The preceding full 2,764-check success remains historical evidence,
not a pass for this new revision.

The separate browser compiles with zero warnings/errors and passes one native
Start/Pause/counter/clock smoke: three frames, zero pending frames after Pause,
and a stable `192 ms` display. Native agent captures at 940 and 420 pixels show
`Lea responsive` and `Ready`, with nonblank paint and no text overlap. Native
disposal retires a pinned process. These checks keep verified isolation,
unchanged resource limits and no `mshtml.dll` in the browser parent module list.

Only two of 19 runtime hashes change. `Lea.Script.dll` remains 101,376 bytes
and `ScriptWorker/Lea.Script.Worker.exe` remains 50,176; all other 17 entries
are frozen. The required renderer remains 749,568 bytes. Distribution ZIP
compression is a storage measure, not proven faster execution.

No startup/page deadlines, statement/memory/recursion/regex/call limits, native
CPU/memory/job protections, AppContainer capabilities, mitigation flags, runtime
leases or fail-closed cleanup are relaxed. Original Firespider inputs,
installed executables and normal build outputs stay untouched. Evidence lives
in `H:\Lea Evidence\Lea-146-nonblocking-agents-2026-10-11`. No startup fix or
stable release is claimed; broader readiness stays about 65% and milestone
146 remains open.
