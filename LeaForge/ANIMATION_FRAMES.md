# Animation frame scheduling

Milestone 146 remains open; broader browser readiness remains about 65%.
This is an engine feature, not a startup-fix, speedup or stable-release claim.

## API and execution

Pages can call `requestAnimationFrame(callback)` and
`cancelAnimationFrame(handle)`. Compact native `Js/Jp.Rf` dispatches one
eligible batch; `Rd` gives milliseconds until the next opportunity, or -1
when dormant/unavailable; `Rn` reports pending callbacks. `Jp.RfA` uses the
same owning-UI-context contract as the existing asynchronous commands.

The parent owns `Fq`: 64 pending handles, ordered snapshots, a monotonic
session-relative timestamp and no callback execution before its deadline.
Callbacks requested during a batch wait for another opportunity. Handles
are not reused during session resets and admission fails closed on exhaustion.
Cancellation and retirement free admission before the next callback runs.

The bootstrap keeps callback functions inside the realm. Every callback gets
the same timestamp for that batch, window as its receiver and one argument.
Captured map/apply helpers withstand replacement of the public built-ins.
Microtasks drain after each callback, allowing a Promise to cancel a later
snapshot entry. A callback exception permits remaining snapshot peers before
Lea's existing fail-closed policy reports the error and discards the realm.
Completed DOM mutations are retained. Budgets can terminate a batch sooner.

Ordered snapshots, removal before invocation and same-batch cancellation follow
the [HTML animation-frame algorithm](https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#animation-frames).
Standard page names are retained for compatibility; Lea host aliases remain short.
This does not claim full AnimationFrameProvider conformance: the timestamp is
session-relative, not a complete document-timeline/performance implementation,
and standard error/rejection reporting and worker/offscreen-canvas APIs remain
future work.

## Native view and isolation

Optional `Lea.Win.Jf` exposes `Rd/RfA`. `Lea.Script.Win.W` implements it and
the existing serialized native pump dispatches a due frame before repainting.
Hidden controls suspend frame callbacks and resume them when shown; ordinary
timers continue. Waiting callbacks do not keep the view busy. A direct host
using `Js/Jp` instead of the view owns visibility and dispatch decisions.

The host currently uses a 16-ms minimum opportunity interval and a WinForms
timer, not monitor-vsync or a guaranteed frame rate. There is no compositor or
GPU engine added here. Existing `W.On(view)` enables the frame-capable adapter;
Engine/Win still have no required JavaScript interpreter dependency.

The version-three wire extension adds a bounded `frame` command with one
validated timestamp and up to 64 strictly increasing positive handles.
Both compiled wire copies reject invalid numeric types, negative/nonfinite or
excessive times, malformed arrays, duplicates and out-of-order handles. Frame
callbacks use the same 256-call, statement, recursion, interpreter-memory,
page-time, five-second startup and native Job/AppContainer restrictions.
Permission changes, navigation and stop invalidate pending frames; lifecycle
reset, failure and disposal clear their metadata. They cannot execute while
the permission/page/stop guards reject the session.
The default `Be=0`, 2,000-ms page budget, 128-MiB Job and 25% CPU-rate cap stay
unchanged. Remote automatic scripting remains disabled in `W`.

## Verification and size

The initial compile fails with CS0136 at a new flag shadowing the existing
received-frame variable. That flag is renamed; original source/log/output
evidence is retained, and corrected builds use separate directories. No native
gate is repeated. All five corrected runtime/test projects and the fresh browser
compile with zero warnings/errors.

| Gate | Result |
| --- | --- |
| `-rf scheduler` | 60 scheduler/compiled-protocol assertions |
| `--animation-frames trusted` | 26 assertions |
| `-rf isolated` | 27 assertions; real guarded worker |
| `-rf native` | 14 assertions, including hidden pause/resume, repaint and cleanup |
| Existing wire/timer/worker/startup/assets gates | 194 / 165 / 87 / 16 / 4 assertions |
| Complete renderer regression | Pass |
| Extra arguments / previous worker | Expected rejection; no native page execution |
| Fresh animation browser | Exit 0; Start advances three frames, Pause clears callbacks and prevents later updates |
| Full scripting regression | Exit 1 after 565 assertions at fresh-worker recovery following a hard microtask deadline; expected completion is 2,651 |

The full count is the preceding 2,516 plus eight transport assertions and
60+26+27+14 frame assertions. This expectation is not a passing result. The
one full run fails in the existing worker suite before the new frame suite is
reached. That assertion records only `.Ok`, without the returned result or
startup/execution report; no exact status, phase or cause can be established
from the preserved output. It is not relabeled a startup timeout or frame bug.
The full gate is not repeated and no budget/deadline is relaxed.
The preceding 2,073-assertion CPU-helper failure and earlier startup/EOF/full
failures remain unresolved and are not reclassified by focused passes.

The compact bootstrap is 36,186 -> 26,134 bytes (27.8% source reduction),
with matching AST JSON, fixed-point generation and compiled-resource checks.
It is not identifier-mangled or decompressed at runtime. Four of 19 runtime
records change: Win +512, Script +4,608, Script.Win +1,024 and worker +1,536
bytes. Combined growth is 7,680 bytes, with no new dependency. Required
Engine/Win/Image total is 749,568 bytes. The other 15 runtime records, including
Engine/Image, are unchanged. Neither short aliases nor ZIP compression prove
faster execution.

The separate .NET Framework 4.6.2 test EXE defaults to the new animation page:
`C:\Users\Elliot\AppData\Local\Lea\EngineBuilds\Lea-146-animation-frames-corrected-2026-10-11-browser\Lea.Script.Browser.exe`.
It uses Lea's renderer, not Chromium/Safari/MSHTML. The smoke reports isolation,
no loopback exemption and the 128-MiB limit; its host module list has no MSHTML.
Before/after captures are inspected and sampled pixels confirm bar width
40 -> 89 at frame 3. Native captures at widths 940 and 420 are nonblank and
fit the inspected viewport. These are not arbitrary-site/mobile-platform tests.

Evidence: `H:\Lea Evidence\Lea-146-animation-frames-corrected-2026-10-11`.
The original Firespider sources/installation and historical normal outputs stay
unchanged. No current Firespider startup/matrix rerun or installed replacement
is claimed. Source and runtime ZIPs include limitations/licenses and receive
entry-hash verification before checkpoint sealing.
