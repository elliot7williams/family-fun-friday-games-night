# Lea.Script

An optional **JavaScript experiment**, separate from Lea's tiny required
runtime. `Js` is the trusted in-process option; `Jp` is the isolated-process
option. `Lea.Engine` and `Lea.Win` still have no JavaScript interpreter
dependency. The renderer remains Lea's own; this does not use Chromium,
WebView, Safari, or another browser renderer.

This project embeds [Jint 4.16.4](https://www.nuget.org/packages/Jint/4.16.4),
with Acornima 1.7.0 and framework support libraries pinned in
`packages.lock.json`. It evaluates JavaScript expressions, functions, loops,
arrays, objects, JSON, and template strings. It does not implement those
language rules through a new handwritten parser.

## Visual Studio

The script projects are included in `../Lea.sln`. A host using this optional
module must target **.NET Framework 4.6.2 or later**; Firespider currently
targets 4.6.1 and is not migrated or automatically opted in by this experiment.
Add a project reference to `Lea.Script` and let NuGet restore its locked
dependencies. Keep all output dependencies and binding redirects with the
host executable.

```csharp
var e = new Lea.Engine.E();
var p = e.Go("local-test.html");
using (var js = new Lea.Script.Js(e, p))
{
    var result = js.Ex(@"
        const n = [3, 4].reduce((a, b) => a + b, 0);
        document.getElementById('result').textContent = `Total: ${n}`;
        document.title = `Lea ${n}`;
    ");
    // result.Ok, result.S, result.N, and result.M describe execution.
}
```

`Js` binds one engine-owned page and one calling thread. `Ex` explicitly
executes supplied code; neither constructing the session nor loading a page
automatically executes standard script blocks. Successful calls retain their
JavaScript global state in that session. Separate sessions have separate
realms. Any execution failure drops the realm; a subsequent call creates a fresh
one. Mutations completed before a failure remain on the page and are included
in `Jr.N`; execution is not transactional.

`Je=false` prevents execution. `E.Jg` changes whenever the host changes `Je`,
so even disabling and re-enabling between calls discards the previous realm.
Result statuses include `ok`, `off`, `code`, `node`, `js`, and `limit`. Messages are bounded
to 512 UTF-16 units. Calling from another thread, using a disposed session,
or passing a foreign page is a host programming error, not a script result.

## Timers

The optional realm exposes `setTimeout`, `setInterval`, `clearTimeout`, and
`clearInterval`. Function callbacks retain argument identity and receive the
realm's window as `this` (ordinary JavaScript bound-function rules still apply).
Callbacks execute as separate tasks, each followed by a bounded Promise/
`queueMicrotask` checkpoint. Timeout and interval IDs share one map; either
clear method can cancel either kind, including from a callback or microtask.
Delays use signed 32-bit conversion and negative clamping. Synchronous timer
nesting above five applies a four-ms minimum; microtasks do not inherit that
timer-task nesting. A repeating timer is rearmed after its callback, before
the checkpoint, without an overdue catch-up burst.

Compact embedding aliases on `Js` and `Jp`:

| Alias | Meaning |
| --- | --- |
| `Td` | Milliseconds until the next eligible timer; `-1` for none/ineligible |
| `Tn` | Number of retained timer records, including a currently firing timer |
| `Tk` | Execute at most one due timer synchronously; idle calls do not start a VM |
| `TkA` | `Jp`'s owning-UI-context asynchronous timer task |

Plain sessions need their host to call `Tk`/`TkA` on the owning thread after
waiting for `Td`; do not spin-poll or synchronously block a UI waiting for it.
`Lea.Script.Win.W.On(view)` provides automatic dispatch through the native
view's existing bounded script queue. The optional `Lea.Win.Jt` provider
interface supplies `Td`/`TkA` without an interpreter dependency in Lea.Win.
One lazy WinForms timer drives all records in a binding; waiting timers do not
make the view busy. Navigation, Stop, permission revocation, detach and disposal
retire the binding and its timer pump. A failed realm drops all timer records,
preserving mutations completed before the failure, as with other script calls.

Each realm permits 64 active timers and 64 callback arguments per timer; string
handler code is limited to 16 KiB. Function references, argument objects and
code strings stay inside the interpreter; only IDs/delays cross the parent
bridge. String handlers use captured indirect-eval semantics (global `var`,
eval-scoped `let`/`const`), not full HTML classic-script declaration/fetch/CSP/
Trusted Types behavior. Native hard deadlines and cooperative/memory/host-call
budgets remain active for every timer task. Timer accuracy, background throttling,
animation frames, a full browser event loop and error-event reporting remain work.
Automatic/supplied HTTP/HTTPS/UNC scripting stays disabled in the Win adapter.
Worker protocol version is now **3**; replace parent and worker binaries together.
Version-2 worker frames fail closed. See the source notes in `LeaForge/TIMERS.md`.

## Isolated worker

`Jp` binds the **currently active** page and its owning thread. The same DOM
bridge remains in the parent; only primitive JSON values and opaque integer
node handles cross the pipe. No page, field, engine, or CLR object is handed
to the interpreter process. Both sides validate versioned length frames;
the parent separately enforces execution IDs, request ordering, call counts,
allowed operations, argument types, connected-node ownership, and value sizes.

```csharp
// On a WinForms UI thread with its synchronization context installed:
var jp = new Lea.Script.Jp(e, p);
var result = await jp.ExA("document.title = 'Computed ' + (3 + 4);");
// On navigation/shutdown: jp.St(); await any active ExA task; jp.Dispose();
```

`Ex` is the synchronous test/host option. `ExA` runs native process setup on a
dedicated background thread, awaits pipe I/O, and services DOM requests on the
captured UI thread; it requires an owning synchronization context. Startup
file validation, parent DOM callbacks, and teardown are still synchronous.
Native OS launch itself cannot be interrupted: the session retains its file
leases and running state until launch returns, then checks cancellation,
permission generation, and page ownership before continuing. `St` may be called from another thread;
it closes the native Job to terminate the worker without mutating the DOM.
The launch thread publishes that Job under the same lock used by Stop and
closes it for a pending Stop without waiting for an owner-context continuation.
Do not dispose a session until its active execution task has completed.
Concurrent execution is rejected. Success preserves the worker realm; errors,
permission generations, navigation to another page, and disposal discard it.
Completed mutations are retained, not rolled back.

Each session uses a unique temporary AppContainer profile. The shared native
launcher creates the worker suspended, verifies its expected SID, zero
capabilities, low integrity, stripped privileges, startup ASLR and Job limits,
then resumes it. It grants read/execute only to the eight exact worker runtime
files, never their folder; read-only file leases prevent replacement while
the session is live. Local ACL-capable storage is required; network volumes,
missing files and reparse-point paths fail closed. There is no unconfined or
in-process fallback. Exact session ACLs and the profile are removed on cleanup
on a best-effort basis, including startup failures.
File ACL read/modify/write operations share a path-keyed named mutex across
launcher assemblies and host processes. Grant and exact-SID cleanup use the
same critical section; another live session's rule is preserved. Lock waits
are bounded to one second and unavailable synchronization fails closed.

The parent checks Windows' loopback-exemption list for the session SID.
Startup also proves an unrestricted parent connection to a real listener,
then checks the restricted worker cannot connect to it and cannot open a
host-only canary. Windows may silently drop the connection: `Np=2` means
the 500 ms probe did not connect; `Np=1` means explicit AccessDenied. A probe
timeout alone is not treated as proof: zero capabilities, no exemption, the
positive control, and no pending listener connection are all required.
The denial connection now runs while the worker initializes its trusted VM
and DOM bootstrap. Its result is still required before `hello`; the parent
does not send any page code until it verifies the complete handshake. A
connection that succeeds during this overlap returns failure, even after the
500 ms observation window has elapsed.
See Microsoft's [AppContainer documentation](https://learn.microsoft.com/en-us/windows/win32/secauthz/implementing-an-appcontainer)
and [loopback configuration API](https://learn.microsoft.com/en-us/windows/win32/api/networkisolation/nf-networkisolation-networkisolationgetappcontainerconfig).

The compact diagnostics are `Pid/Sid`, `Ac` (verified token/startup), `Ne`
(no loopback exemption), `Nd/Np` (controlled network probe), `Fd` (canary denial),
`Mg` (worker mitigation mask, required 15), and `Mp` (recorded Job peak).
`Sd` retains a 512-character startup report: phase, native launch, parent
positive-control probe, total startup wall time, and child CPU time in ms.
Warm calls leave that report unchanged. A startup failure also snapshots the
report into its bounded `Jr.M` message, so a later lifecycle/recovery start cannot
erase the failed attempt's evidence. Execution-failure messages are unchanged.
The worker sends compact
entry/guards/init/canary/network/VM/bridge/source/evaluate/parsed/DOM/probe markers to
bounded stderr even without timing enabled. Optional `LEA_SCRIPT_STARTUP_TRACE=1`
adds wall/CPU measurements to those markers. Traced and untraced startup both use the VM's regular string
parser and the same regex limits; tracing no longer substitutes a prepared
script. A one-shot post-parse evaluation hook records `parsed` without debugger
stepping and is removed before page execution. The pre-hook interval includes
parser and entry/JIT costs; an absent marker is not a parser stack trace.
Bootstrap initialization remains sequential.
No diagnostic function is exposed to page code. File-validation failures
before native launch leave `Sd` empty. See `../LeaForge/STARTUP_DIAGNOSTICS.md`.
The fixed-list bootstrap follow-up and rejected concurrency experiment are
recorded in `../LeaForge/BOOTSTRAP.md`; full-suite cold-start failures remain open.
See `../LeaForge/STARTUP_TRACE.md` for production-path tracing, compact markers,
and the latest passing and failing gates.
Asynchronous worker I/O also observes permission/navigation/Stop state on its
owner context every 50 ms without restarting the original I/O deadline. Native
launch still retains ownership until the OS call returns. See
`../LeaForge/STARTUP_IO.md` for the reproduced lifecycle defect, focused fixes,
and still-failing full startup gates.
Otherwise-unmapped native AppContainer profile COM failures now become typed
I/O setup failures, retaining their HRESULT and description. Existing mapped
exception types, including critical memory failures, are preserved. See
`../LeaForge/PROFILE_FAILURES.md` for deterministic error/cleanup regressions and
the separately verified real disk-full/native-view failure gate. The fix handles
that setup error; it does not free storage or make the worker available there.
`Mm` is 128 MiB; `Cp` is a 2,500/10,000 Job CPU hard-cap rate, with one allowed
process and kill-on-close. `Tm` is the parent's execution/I/O budget, default
2,000 ms and clamped to 25..5,000 ms. Startup handshake has a separate 5,000 ms
budget. Timeout messages identify startup versus execution without changing
the status or budget. Native OS launch, teardown/ACL cleanup and a running parent callback can extend the total
call duration; the host callback itself cannot be forcibly preempted.

Worker Jint limits match `Js` below except its cooperative timeout is five
seconds, allowing for blocked RPC and cold CLR execution. The independently
enforced parent deadline remains authoritative. Additional `Jr.S` statuses
are `setup`, `protocol`, `worker`, `timeout`, `cancel`, and `stale`. Recovery
creates a fresh process and realm; missing/isolation-invalid workers do not
execute any page script.

This requires Windows 10 or later and .NET Framework 4.6.2 or later. Copy the
worker project output's eight runtime files into `ScriptWorker` beside the
host, or supply that folder to the third constructor argument. Worker files
must come from a trusted build. A hostile host, administrator, or modified
runtime is outside this boundary. These checks are not a security audit or
a claim that arbitrary remote websites are ready to run.

## DOM bridge

The bridge exposes JavaScript objects and primitive values, never Lea's CLR
objects, reflection types, file handles, or network clients:

- `document.title`, `body`, and `documentElement`.
- `getElementById`, `querySelector`, and `querySelectorAll`, using Lea's existing
  bounded selector implementation. `querySelectorAll` returns an Array snapshot,
  not a standards-complete NodeList. Invalid/unsupported selectors currently
  follow Lea's empty-result behavior, not browser `SyntaxError` behavior.
- Stable element wrapper identity, `tagName`, `getAttribute`, `hasAttribute`,
  `textContent`, and live writable `id`/`className` strings.
- Mutable field `value` and `checked`, with Lea's existing field constraints.
- Read-only element `parentElement`, `firstElementChild`, `lastElementChild`,
  `previousElementSibling`, `nextElementSibling`, and `childElementCount`;
  document child endpoints/count; live text-skipping navigation and stable
  detached-subtree identity without detached writes. Six shared getters use
  private two-letter bridge commands and an 8,192-visit ceiling. See
  [tree navigation contracts and tests](../LeaForge/TREE_NAVIGATION.md).
- Element `matches(selector)` and inclusive nearest-ancestor `closest(selector)`
  reuse the bounded selector compiler. Methods brand-check their actual receiver
  before converting the required argument, retain wrapper identity, and can read
  detached subtrees without enabling detached writes. Invalid, unsupported or
  over-limit selectors throw a branded `SyntaxError` DOMException; valid misses
  return false/null. Core `N.Mt/Cl` aliases provide trusted host reads. The
  existing case-insensitive selector rules remain; `:scope`, pseudo-elements,
  escaping and complete browser selector semantics are not added. See
  [element selector contracts and tests](../LeaForge/ELEMENT_SELECTORS.md).
- Stable live `classList` with variadic `add/remove`, forced `toggle`, ordered
  `replace`, `contains/item`, raw `value`, readonly indices and native iteration.
- Write-only `style.color/backgroundColor/width/height/display/fontSize`.
- A bounded queued `alert`, plus `window` as the global object.

The core `E.Jv(page, node, alias, value)` path applies literal typed values,
not generated code. It supports `t/a`, `tx/vl/ck`, `id/cn`, `ac/rc/tc`, and
`fg/bg/wd/ht/dp/fs`. It checks host permission, page and connected-node
ownership, value limits, and the existing style/field validation. Text
replacement detaches removed nodes, removes obsolete retained fields, and
rebuilds style/layout records. It works on elements without authored IDs and
does not expand Lea `${variable}` templates in computed strings.

`id` and `cn` replace the ID and raw class attributes of connected Lea elements,
including root/nonpainted elements, up to 512 UTF-16 units per value. Empty
strings leave present empty attributes; missing `getAttribute` is `null` while
`hasAttribute` distinguishes presence. Writes retain node/wrapper/field identity
and rebuild selector, style, layout and accessible-label state. The optional
bridge uses a captured string converter for DOM writes and attribute names;
direct Symbols throw before a host call, while `null`/`undefined` ID/class writes
become strings. Text content retains its existing null-to-empty special case.
No generic `setAttribute`, URL mutation or DOM creation capability is added.
The existing case-insensitive core ID/class selector matching remains a
limitation; script `classList` operations are case-sensitive. SVG animated class
names and complete IDL/prototype/namespace behavior are not implemented. These
two aliases are
typed `E.Jv` mutations, not new statements in the literal Lea command language.
See [attribute implementation and tests](../LeaForge/ATTRIBUTES.md).

Class selectors, word-attribute selectors, and bounded class mutations split
on ASCII whitespace, including form feed but not NBSP or vertical tab. Script
membership uses the same separators with ordered unique tokens and branded DOM
exceptions. Bulk writes retain the existing 512-unit `cn` ceiling, captured
intrinsics and field identity. Small core compounds avoid transient
token sets, while dense compounds retain hashing. See
[class-token scope and measurements](../LeaForge/CLASS_TOKENS.md).
See [live class-list scope and tests](../LeaForge/CLASS_LISTS.md) for the optional
interface and its remaining compatibility boundaries.

## Native view integration

`Lea.Script.Win` attaches the isolated runtime to `Lea.Win.V`; the view and core
still do not reference Jint. Reference the adapter project from a .NET Framework
4.6.2-or-later WinForms host, deploy the worker folder and output dependencies,
and attach it before loading a page:

```csharp
Lea.Script.Win.W.On(view);
await view.GoA("local-test.html");
var result = await view.JsA("document.title = 'Lea ' + (2 + 3);");
// W.Off(view) detaches it. View navigation/Stop/disposal retire sessions.
```

The adapter currently executes **local file/about documents only**, excluding
UNC network files. HTTP/HTTPS
documents return `origin` for inline, supplied, and event code without starting
an interpreter; this is a temporary integration restriction, not modern-site
support. External/module/async/defer loading is not implemented. The installed
Firespider EXE remains on 4.6.1 without this adapter. Its eventual adoption needs
framework migration and host-specific tests, not just DLL replacement.
The optional third `W.On` argument sets the per-execution millisecond budget
for each page session, clamped through `Jp.Tm`; its default remains 2,000 ms.
Milestone 145 keeps script-free pages dormant: no worker is started unless a
classic inline block, supported authored handler, or supplied `JsA` code needs
one. Template contents are inert. Deferred supplied code establishes the ready
and already-requested load lifecycle before execution. Dormant event requests
still validate their type and connected-node ownership.

`../Lea.Build/Build-FirespiderCandidate.ps1` builds a separate .NET Framework
4.8 Firespider source/runtime candidate from the live project plus the clean
adapter sources. `LeaScriptEnabled=true` attaches this optional module to
normal tabs and split panes. The candidate uses a documented 5,000 ms ceiling,
protects live workers from automatic hibernation, and reports bounded errors
in its status bars. `-NoScript` builds the separate small-engine variant.
Neither path installs over the live browser. The installed .NET Framework
4.6.1 host remains unchanged; copying these DLLs alone is not a migration.

At most eight classic inline blocks (empty, `text/javascript`, or
`application/javascript` type, no `src`) are snapshotted in document order
after parsing. Each is limited to 16 KiB; scanning stops at 8,192 nodes and
skips template contents. This is not parser-blocking browser script execution.
`DOMContentLoaded` runs after those blocks; the view dispatches window `load`
after its image work finishes. `document.readyState` moves from `loading` to
`interactive` to `complete`. Repeated page initialization/load requests do not
repeat successful lifecycle handlers. A failed realm is reinitialized from the
inline snapshot on its next call; prior DOM writes are not rolled back.

The compact `Jx` provider interface exposes `ExA/EvA/PgA/St`; `Xr` carries
`S/N/M/D/Ok`. A view's `Xf` factory binds the current page and permission
generation. `JsA`, `Xe`, and `Xl` queue work; `Xs/Xt/Xn/Xo` expose the current
provider, tail task, pending count, and result, and `Xd` reports completion.
There are at most 32 queued executions and 32 native activations. Defaults and
checkbox/radio preactivation are serialized along with event delivery. Factory
reentry, revocation, navigation, Stop, detach, and disposal retire stale work;
workers are stopped immediately and providers disposed exactly once after
pending work settles. Old-page completions cannot repaint a replacement page.
Host completion errors become `host` results without poisoning the queue.

Computed writes use the view's existing invalidation, find, accessibility,
alert, and navigation notification path. Script field values synchronize the
native editor without generating another input event. Native text edits emit
`input`, editing commits emit `change`, checkbox/radio/range/color/select actions
emit `input` then `change`, and asynchronous form submission emits cancelable
`submit` after validation. Synchronous `Sm` is still the legacy host route, and
file selection/accessibility-only field edits are not yet event-complete.

`E.Jb` defaults to true for the original bounded classic/literal fallback.
Attaching an optional view factory sets it false, preventing duplicate classic
autorun/onclick execution. Explicit `text/lea`, `application/lea`, and
`data-lea` commands remain available. `E.Jn` checks connected-node ownership.

## Microtasks

Milestone 146 adds `queueMicrotask(callback)` and finishes Jint's pending
Promise jobs at native event completion. Deferred DOM writes are included in
the result's mutation count and synchronized with native painting/editors
before the view reports completion. Inline blocks each finish their own
checkpoint before the next block; ready/load events use the same path.

The private scheduler uses Jint's native async/await continuation queue, not a
second Promise implementation. It performs no public Promise/Reflect helper
lookup during bootstrap and remains usable when page code replaces those helpers.
FIFO/nested ordering, `async`/`await` event continuations, handled rejections,
callback `this`/arguments and hidden host helpers are tested. Native events
finish propagation and clear event phase/current target before one checkpoint.
Their synchronous cancellation result is captured before deferred callbacks.
This bounded dispatch-turn behavior is not the HTML algorithm's complete
per-callback cleanup/event-loop model.

At most 128 `queueMicrotask` admissions are allowed per `Ex`/`Ev` execution,
including recursive admissions; each also consumes a shared bridge call.
Native Promise jobs retain the interpreter's execution/memory constraints,
and the isolated parent's deadline remains authoritative. The checkpoint
runs inside the dispatch invocation so those jobs cannot run without its
constraints. No new required core/view dependency or long host alias is added.

An uncaught `queueMicrotask` callback records its first error while later
queued jobs drain. The call then reports `js`, preserves completed DOM writes
and discards the realm, following Lea's existing failure policy. Synchronous
script/event failure discards queued work without running it. Permission,
navigation, Stop and disposal still retire the isolated session. Timers,
animation-frame scheduling, `error`/unhandled-rejection notifications,
external scripts and a general browser event loop remain unfinished.

See the [HTML microtask API](https://html.spec.whatwg.org/multipage/timers-and-user-prompts.html#microtask-queuing)
and the pinned [Jint task-processing API](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Engine.Advanced.cs).

## DOM events

Both `Js.Ev` and `Jp.Ev/EvA` accept a parent-validated node plus one supported
type: `click`, `input`, `change`, `submit`, `DOMContentLoaded`, or `load`.
The 128-element ancestor path contains opaque handles, not script-supplied
nodes. Current protocol version 3 carries a strict boolean `Jr.D` cancellation
result and timer frames; older workers fail closed rather than mixing schemas.

Element wrappers, document, and window support `addEventListener` and
`removeEventListener`, capture/bubble ordering, callback objects, duplicate
suppression, `once`, `passive`, and ordered DOM0/inline handler slots. A realm
has at most 128 listener records, including active DOM0 handlers. `onclick`,
`oninput`, `onchange`, `onsubmit`, and `onload` accept functions/null; authored
handler bodies are lazily compiled. The limited inline scope does not implement
HTML's legacy element/form name lookup or body-to-window handler forwarding.

Events expose type, target, currentTarget, eventPhase, bubbles, cancelable,
defaultPrevented, timeStamp, composedPath, preventDefault, stopPropagation,
and stopImmediatePropagation. Click/submit cancellation suppresses the native
default action; passive listeners cannot cancel. These synthesized events have
`isTrusted=false`. Listener exceptions currently fail the dispatch and reset
the realm, rather than reporting and continuing as standard browsers do.
There is no Event constructor, public dispatchEvent, AbortSignal, shadow DOM,
keyboard/focus/pointer event family, or general browser event loop yet.
Behavior is guided by the [DOM event algorithms](https://dom.spec.whatwg.org/#interface-eventtarget)
and [HTML event-handler model](https://html.spec.whatwg.org/multipage/webappapis.html#event-handlers),
but this is a bounded subset, not conformance with those specifications.

## Budgets and security boundary

Per call: 16 KiB source, 20,000 statements, one-second interpreter timeout,
8 MiB interpreter allocation budget, recursion limit 64, 100 ms regular
expression timeout, 65,536 array entries, and 256 host calls. A session retains
at most 512 node wrappers. Text reads stop at 16 KiB; typed text writes stop
at 512 units, titles at 256, alerts at 512, and command values at 1,024.

**Only use in-process `Js` for scripts you trust.** These are interpreter limits, not hard process
resource limits. Parsing, native built-ins, and host callbacks can outlive a
statement timeout or allocate before a budget check. There is no OS sandbox
or hard cancellation in `Js`. Jint's own
[threat model](https://github.com/sebastienros/jint/blob/v4.16.4/.github/THREAT_MODEL.md)
requires process isolation for untrusted scripts.

Automatic remote-page scripts, external scripts, modules loaded from files or
URLs, additional DOM events, animation frames, the full browser event loop,
fetch/XHR, storage, location/navigation, DOM creation/removal, `innerHTML`, and
the full CSSOM remain future integration work. General browser compatibility
and production untrusted-site safety are not claimed by these tests.

Native `V.Idg` separately reports the last completed image-decoder delegate,
bounded to 512 characters without URLs, paths or image content. It is not the
script worker's `Sd` report or whole-navigation timing. Pre-start cancellation
retains the previous value; see [image cancellation and timing boundaries](../LeaForge/IMAGE_STARTUP.md).

## Verify

```powershell
dotnet restore Lea.Script.Tests/Lea.Script.Tests.csproj --locked-mode
dotnet build Lea.Script.Tests/Lea.Script.Tests.csproj -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```

The test fixture deliberately uses `application/lea-test` so normal loading
does not run it. The executable explicitly supplies its script to the session.
`javascript-view.html` instead contains ordinary classic blocks and native
event controls. `--events`, `--timers` and `--view` select focused checks; the full
suite includes all three. Keep these packages separate from installed browser files.
Functional event/view fixtures use the supported 5,000 ms budget to avoid
conflating cold CLR/IPC scheduling with event behavior. Strict cooperative-limit
assertions are retained after function warmup; the independent 25 ms native
regex deadline still verifies forcible termination. The separate browser smoke
uses the unchanged 2,000 ms default. Cold runs under competing compilation have
hit deadlines; these tests do not establish performance parity or a latency SLA.

The earlier 142/143 packages remain separate. Milestone 144 adds the native
adapter and `Lea.Script.Browser.exe`; build its project and the test project,
then package them with `../Lea.Build/Pack-LeaScript.ps1`. Extract the complete
ZIP onto a local NTFS/ACL-capable volume before starting the browser EXE.
Runtime/package byte counts are artifact
measurements, not evidence that compression speeds up execution. The worker
has a cold CLR/process startup cost; persistent successful sessions reuse it.
Third-party redistribution notices are included in `licenses`.
