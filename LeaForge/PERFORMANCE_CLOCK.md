# Performance Clock

Lea's optional scripting module exposes a small parent-owned animation clock.
The required renderer remains unchanged; this does not add a browser runtime.

| Surface | Meaning |
| --- | --- |
| `performance.now()` | Monotonic elapsed milliseconds in the bound host session |
| `performance.timeOrigin` | Fixed epoch origin for that session, read-only |
| `performance.toJSON()` | A fresh object containing `timeOrigin` |
| `Js.Nw` / `Jp.Nw` | Read-only host elapsed-time alias |
| `Js.To` / `Jp.To` | Read-only host origin alias |
| Private `nw` / `to` | Exact one-argument bridge operations |
| Test `-cl core/trusted/isolated/native` | Focused clock regressions |

## Contract

The parent captures the UTC origin once and advances the clock with
`Stopwatch`, not `Date.now()`. Page reads and animation-frame timestamps are
rounded down to whole milliseconds. All callbacks in a frame batch still
receive the same sampled timestamp; a subsequent `performance.now()` may be
later. This is host-paced animation, not monitor-vsync or a frame-rate claim.

Host clock reads do not create a Jint interpreter or native worker. They require
the owning thread and reject disposal. Clock metadata remains available while
scripting is disabled, stopped or a page binding is stale; this is not
permission to execute page code. Replacing a failed realm does not reset the
bound session's clock. Creating a new host session captures a new origin.

`Performance` is not constructible. Its methods and origin getter reject
foreign receivers. The global `performance` attribute is replaceable, and its
methods are writable/configurable. Replacing them affects only that page's
realm, not the native clock. Bridge reads use the existing 256-call ceiling
and permission guard; they do not mutate the DOM or enqueue work.

Numeric bridge replies now admit finite signed `Double`, `Int64` and `Decimal`
values whose converted magnitude is at most `Number.MAX_SAFE_INTEGER`.
JSON epoch integers therefore work in the isolated worker as well as trusted
code. Existing integer/string/bool/array replies and inbound Int32-only argument
validation remain unchanged. Unsupported `Single`, nonfinite and oversized
replies are rejected; this is not an unrestricted CLR numeric bridge.

## Limits

This is a bounded subset inspired by the
[High Resolution Time working draft](https://www.w3.org/TR/2026/WD-hr-time-3-20260901/),
not a claim of complete Performance conformance. In particular, Lea's origin
belongs to the bound host session, not a standards-complete navigation start.
Performance's EventTarget inheritance, timeline entries, marks/measures,
observers, worker-global clocks and cross-origin isolation are not implemented.
Whole-millisecond coarsening does not eliminate all timing attacks.

The zero-capability AppContainer, 15 mitigations, 128 MiB job, 2,500-basis-point
CPU-rate cap, five-second startup deadline, page budgets, leases, protocol
limits and fail-closed recovery remain unchanged. Remote page script execution
is still disabled in the WinForms adapter.

## Verification

| Check | Actual Result |
| --- | --- |
| Clock core / trusted / isolated / native | Exit 0: 57 / 14 / 16 / 10 assertions |
| Compiled wire replies | Exit 0: 210 assertions |
| Frame scheduler / isolated / native | Exit 0: 60 / 27 / 14 assertions |
| Trusted frame regression | Exit 1 before assertions: trusted bootstrap timeout |
| Timer regression | Exit 1 after 133 assertions: worker startup hello timeout |
| Worker regression | Exit 1 after 51 assertions: cooperative-function warmup startup timeout |
| Startup / bootstrap assets | Exit 0: 16 / 4 assertions |
| Required renderer | Exit 0: native smoke footer |
| Invalid `-cl` arguments | Expected exit 1 before execution |
| Preceding worker Codec | Expected exit 1 rejecting the first double reply; no worker/page execution |
| Observer count/footer assembly | Six AST-selected checks pass; no native observer launch |
| Full scripting regression | Exit 0: exact 2,764-check footer, one attempt, no observer/opt-in trace |

The new clock suites total 97 assertions. The complete scripting suite now
expects 2,764: the previous 2,651 plus 16 wire checks and 97 clock checks. The
observer's full default follows that compiled test revision; worker mode still
expects 87. A passing browser smoke is not a full-regression pass.

The separate browser compiles without warnings and passes one native smoke:
counter increment, three animated frames, Pause with zero pending frames and a
stable `178 ms` display. Its bar changes from 40 to 87 pixels. Desktop and
narrow clock captures are nonblank, show the same duration and have no text
overlap. The browser records verified isolation, unchanged resource limits and
no `mshtml.dll` in its parent module list. None of these observations is a speed
benchmark or universal compatibility claim.

The initial generator path error, stale executable's short-alias rejection,
dependent build precondition failure and missing test import remain recorded.
The corrected generator uses the existing executable's descriptive forms,
passes seven round trips and the changed-literal control, and checks matching
AST JSON/fixed-point output. Compact bootstrap size is 27,220 versus 37,574
readable UTF-8 bytes. The corrected tests compile without warnings.

Only two of 19 runtime entries change: `Lea.Script.dll` adds 2,560 bytes and
`ScriptWorker/Lea.Script.Worker.exe` adds 1,536. The other 17 entries are frozen;
the required renderer remains 749,568 bytes. Distribution ZIP compression
reduces storage, not proven execution time. Original Firespider inputs,
installed executables and normal build outputs remain unchanged.

Evidence is retained under
`H:\Lea Evidence\Lea-146-performance-clock-2026-10-11`. This is an experimental
build, not a verified Firespider replacement or stable release. The focused
failures are retained, not replaced by the successful full run. Their exact
cause remains unestablished; there are no automatic native retries.
Broader readiness remains about 65%; milestone 146 stays open.
