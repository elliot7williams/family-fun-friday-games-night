# Lea.Script

An optional **trusted-script experiment**, separate from Lea's tiny required
runtime. `Lea.Engine` and `Lea.Win` still have no JavaScript interpreter
dependency. The renderer remains Lea's own; this does not use Chromium,
WebView, Safari, or another browser renderer.

This project embeds [Jint 4.16.4](https://www.nuget.org/packages/Jint/4.16.4),
with Acornima 1.7.0 and framework support libraries pinned in
`packages.lock.json`. It evaluates JavaScript expressions, functions, loops,
arrays, objects, JSON, and template strings. It does not implement those
language rules through a new handwritten parser.

## Visual Studio

The two new projects are included in `../Lea.sln`. A host using this optional
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

**Only run scripts you trust.** These are interpreter limits, not hard process
resource limits. Parsing, native built-ins, and host callbacks can outlive a
statement timeout or allocate before a budget check. There is no OS sandbox
or hard cancellation in this project. Jint's own
[threat model](https://github.com/sebastienros/jint/blob/v4.16.4/.github/THREAT_MODEL.md)
requires process isolation for untrusted scripts.

Automatic remote-page scripts, external scripts, modules loaded from files or
URLs, event listeners, timers, promises integrated with a browser event loop,
fetch/XHR, storage, location/navigation, DOM creation/removal, `innerHTML`, and
the full CSSOM remain future integration work. General browser compatibility
and untrusted-site safety are not claimed by these tests.

## Verify

```powershell
dotnet restore Lea.Script.Tests/Lea.Script.Tests.csproj --locked-mode
dotnet build Lea.Script.Tests/Lea.Script.Tests.csproj -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```

The test fixture deliberately uses `application/lea-test` so normal loading
does not run it. The executable explicitly supplies its script to the session.
Keep it separate from installed browser runtime files.

The complete ten-file Release test runtime is approximately 3.57 MiB,
including `Lea.Engine`; its per-file GZip estimate is 1.35 MiB. That is an
artifact-size measurement, not evidence that compression speeds up execution.
It is distributed separately as
`F:\Lea-js-142-experiment_2026-10-03_05-33-17.zip`.
Third-party redistribution notices are included in `licenses`.
