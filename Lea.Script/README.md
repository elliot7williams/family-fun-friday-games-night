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

The three script projects are included in `../Lea.sln`. A host using this optional
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
Result statuses are `ok`, `off`, `code`, `js`, and `limit`. Messages are bounded
to 512 UTF-16 units. Calling from another thread, using a disposed session,
or passing a foreign page is a host programming error, not a script result.

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

`Ex` is the synchronous test/host option. `ExA` awaits pipe I/O and services
DOM requests on the captured UI thread; it requires an owning synchronization
context. Startup file validation and native setup, and individual parent DOM
callbacks, are still synchronous. `St` may be called from another thread;
it closes the native Job to terminate the worker without mutating the DOM.
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

The parent checks Windows' loopback-exemption list for the session SID.
Startup also proves an unrestricted parent connection to a real listener,
then checks the restricted worker cannot connect to it and cannot open a
host-only canary. Windows may silently drop the connection: `Np=2` means
the 500 ms probe did not connect; `Np=1` means explicit AccessDenied. A probe
timeout alone is not treated as proof: zero capabilities, no exemption, the
positive control, and no pending listener connection are all required.
See Microsoft's [AppContainer documentation](https://learn.microsoft.com/en-us/windows/win32/secauthz/implementing-an-appcontainer)
and [loopback configuration API](https://learn.microsoft.com/en-us/windows/win32/api/networkisolation/nf-networkisolation-networkisolationgetappcontainerconfig).

The compact diagnostics are `Pid/Sid`, `Ac` (verified token/startup), `Ne`
(no loopback exemption), `Nd/Np` (controlled network probe), `Fd` (canary denial),
`Mg` (worker mitigation mask, required 15), and `Mp` (recorded Job peak).
`Mm` is 128 MiB; `Cp` is a 2,500/10,000 Job CPU hard-cap rate, with one allowed
process and kill-on-close. `Tm` is the parent's execution/I/O budget, default
2,000 ms and clamped to 25..5,000 ms. Startup handshake has a separate 5,000 ms
budget. Teardown/ACL cleanup and a running parent callback can extend the total
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
- Stable element wrapper identity, `tagName`, `getAttribute`, and `textContent`.
- Mutable field `value` and `checked`, with Lea's existing field constraints.
- Single-token `classList.add/remove/toggle/contains` (no varargs or force flag).
- Write-only `style.color/backgroundColor/width/height/display/fontSize`.
- A bounded queued `alert`, plus `window` as the global object.

The core `E.Jv(page, node, alias, value)` path applies literal typed values,
not generated code. It supports `t/a`, `tx/vl/ck`, `ac/rc/tc`, and
`fg/bg/wd/ht/dp/fs`. It checks host permission, page and connected-node
ownership, value limits, and the existing style/field validation. Text
replacement detaches removed nodes, removes obsolete retained fields, and
rebuilds style/layout records. It works on elements without authored IDs and
does not expand Lea `${variable}` templates in computed strings.

This is an engine-model experiment. It is not wired into `Lea.Win.V` or
Firespider's native layout invalidation and click routes yet. Browser-host
integration must handle those routes before enabling scripts in the UI.

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
URLs, event listeners, timers, promises integrated with a browser event loop,
fetch/XHR, storage, location/navigation, DOM creation/removal, `innerHTML`, and
the full CSSOM remain future integration work. General browser compatibility
and production untrusted-site safety are not claimed by these tests.

## Verify

```powershell
dotnet restore Lea.Script.Tests/Lea.Script.Tests.csproj --locked-mode
dotnet build Lea.Script.Tests/Lea.Script.Tests.csproj -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```

The test fixture deliberately uses `application/lea-test` so normal loading
does not run it. The executable explicitly supplies its script to the session.
Keep it separate from installed browser runtime files.

The Milestone 142 ten-file in-process package remains separate. Milestone 143
adds an eight-file `ScriptWorker` folder and tests both transports; package it
with `../Lea.Build/Pack-LeaScript.ps1`. Runtime/package byte counts are artifact
measurements, not evidence that compression speeds up execution. The worker
has a cold CLR/process startup cost; persistent successful sessions reuse it.
Third-party redistribution notices are included in `licenses`.
