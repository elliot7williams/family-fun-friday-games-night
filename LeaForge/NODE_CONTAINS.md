# Node Containment

Milestone 146 remains open; broader readiness stays about **65%**. Lea now
provides compact native `N.Co(N)` and shared JavaScript `contains(node)` on
its supported element/document wrappers. This follows the DOM Standard's
[inclusive-descendant behavior](https://dom.spec.whatwg.org/#dom-node-contains):
self is included, an unrelated node is excluded, and null returns false.
The wrapper layer still does not expose text/comment nodes, Node constructors,
shadow trees, adoption, reparenting or a complete Node interface.

## Behavior And Bounds

The native alias walks live parent links without allocations, mutation or
layout rebuilding. It accepts at most 128 parent edges; deeper or cyclic
host-supplied ancestry raises a bounded error. Self and null are immediate
answers. Retained detached subtrees keep internal relationships, but no longer
belong to their former document. Different documents remain unrelated.

The private parent-owned `co` bridge shares that implementation in trusted and
isolated execution. Bounded ancestry errors become typed `limit` results;
normal bridge call limits, permissions and realm retirement remain unchanged.
Containment does not grant a disconnected node permission to mutate.

JavaScript methods validate the actual element/document receiver and require
one argument. Explicit null/undefined return false. Unsupported objects,
windows, scalars, unbranded prototypes and proxies are rejected without user
coercion or proxy property traps. All wrappers share one nonconstructible method,
using captured private receiver state and intrinsics. The nullable fast path
does not query the host. Helpers stay inside the bootstrap's private scope.

```csharp
bool inside = group.Co(target);
```

```javascript
if (group.contains(event.target)) {
  document.title = 'Inside';
}
```

## Verification

- All 65 new checks pass: native aliases 11, trusted 22, isolated 22, native view
  10. They cover self/descendants, exclusions, required/null arguments, shared
  method shape, borrowed receivers, hostile intrinsics, exact depth boundary,
  cycles, detached reads/writes, field retention, bridge exhaustion and cleanup.
- The identical new test executable against the old runtime exits 1 before a
  passing assertion because `N.Co` is absent. The malformed-argument control
  exits 1 before any startup. Both intended failures are retained.
- Existing tree 55, privacy 36, assets 4, lazy styles 14, options 16, worker 87
  and startup-phase 16 checks pass. The complete renderer `--core` smoke passes.
- One full scripting run passes all 3,010 checks, actual exit zero. The observer
  default/count/footer controls now match that count; four AST-only controls
  pass without launching an inspector. No failed native gate is retried.
- Native 940/420-pixel captures are nonblank and visually checked, with rendered
  logo, independent inside/outside action results and retained editor fitting
  both widths. The exact pinned worker retires after disposal.

The first test build fails on an inaccessible test helper; its source and log
are retained. The repair uses a pinned process handle. The production builds
already succeeded and are not rebuilt for that test-only correction.

Readable/compact bootstrap sizes are 38,520/27,897 bytes, matching AST JSON.
Compared with the preceding runtime, the Engine PE remains 423,424 bytes; parent
Script and worker each grow 512 bytes to 102,400/50,688. The other 16 runtime
entries stay frozen and the required Engine/Win/Image bundle stays 749,568 bytes.
There is no new dependency, mode switch or general speed claim.

## Retained Browser Failure

A separate standalone browser copy uses the new libraries and unchanged host
EXE. Its single smoke attempt exits 1 before writing a success report: inline
startup returns `timeout`, phase `hello`, wall 5,010 ms, CPU 328 ms. Recorded
startup reaches VM, bridge, source and evaluation markers; the exact later
operation is unknown. This does not establish that containment caused the
failure or resolve earlier startup/split failures. Limits stay unchanged.

No new browser ZIP or installed replacement is published from that failed gate.
The passing full/feature/renderer tests do not make the browser smoke a pass.
Firespider's copied-host and six-scenario matrices are not rerun with these new
engine bytes, so previous host results remain historical only.

Evidence: `H:\Lea Evidence\Lea-146-node-contains-2026-10-12`. Complete source
is separately backed up under `H:\Lea Source Backups`; the exact archive/hash
is in this checkpoint's `source-backup.json`. Original host inputs, installed
files, normal outputs and preceding sealed evidence remain unchanged. Release
reliability and real-site compatibility remain work; milestone 146 stays open.
