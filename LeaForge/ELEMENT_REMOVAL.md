# Connected element removal

Milestone 146 remains open; broader readiness stays about **65%**. Lea can now
delete a connected element and its subtree through two-letter native aliases
and the optional JavaScript facade. The renderer is still Lea's own.

## API

`E.De(page, element)` and `V.De(element)` return 1 for a deletion, 0 for a
denied/no-op call. Null, text/document records, foreign nodes, disconnected
nodes and disabled mutations are no-ops; a foreign page is a host argument
error. Connected html/body and nonpainted elements can be detached. Surrounding
text records are retained. `P.N` continues to count elements, not text/document
records.

```csharp
view.De(view.Id("temporary"));
```

The optional facade exposes a shared, zero-argument, nonconstructible
`element.remove()`. It checks its actual branded receiver using captured
intrinsics, ignores extra arguments and returns undefined. The private bridge
operation is `de`; only an issued integer handle crosses the transport. Every
call, including a no-op, consumes the existing 256-call budget.

```javascript
document.getElementById('temporary').remove();
```

Deletion clears only the detached subtree root's parent link. Internal links,
existing wrappers, query snapshots and class-list identity remain readable.
Connected selectors and sibling navigation update; paint/accessibility records
and obsolete form/focus models are rebuilt. Surviving field model, value and
native editor selection are retained. Changes completed before a later script
error remain applied and are reported in `Jr.N`, as with existing typed writes.
Detached hover/focus flags and obsolete ancestor hover are cleared; unrelated
connected focus and its form model are retained.

The parentless no-op follows the [DOM remove algorithm](https://dom.spec.whatwg.org/#dom-childnode-remove).
This is **connected-element editing**, not a complete ChildNode/Node implementation:
retained detached descendants still have no write capability, so remove calls
within those subtrees are ignored. Creation/insertion/reparenting, text/comment
wrappers, mutation observers, ranges, shadow/custom-element callbacks and live
head stylesheet/base/title integration remain unfinished. Removing a style or
base element does not recompute the previously captured head projections.
Remote/external automatic scripts remain disabled by the Win adapter.

## Verification

Proof: `H:\Lea Evidence\Lea-146-element-removal-2026-10-11`.

- All six compilation jobs finish without warnings/errors. Four runtime files
  change: Engine, Win, Script and Worker. The other 15 deployment records remain
  unchanged. The required three-file bundle remains **749,056 bytes**.
- Final core `--removal-core` and alias `-de` each pass 18 checks. Trusted and
  isolated suites each pass 16. Three preceding component controls are rejected
  independently, plus the extra-argument control.
- The native view passes seven behavior and four paint checks, including async
  deletion, focused-input cleanup, retained model/editor/value/selection, narrow
  geometry and click-handler deletion. Two fresh captures are inspected.
- The complete renderer `--core` regression passes. The final scripting run
  exits 1 after **1,980 passing assertions**, with `Controlled EOF worker did not
  exit` in `NativeCpu.Retire` for the Lea.Win test launcher. It does not reach
  the expected 2,418-assertion completion (61 additions over the preceding
  baseline). The cause is not established; later checks are unexecuted, not
  passing. No test processes remain after cleanup.
- Native tests use production `Be=0`, unchanged isolation and deadlines, no
  debugger/startup observer, no native retry or warm fallback.

Two initial core tool failures are retained. The first assertion counted all
tree records instead of elements; the second assumed removing html also removes
surrounding whitespace. Only test assertions change between the three compiled
tools; all 19 engine runtime files remain byte-identical. The native wrapper
also initially expected seven checks instead of counting the existing four
paint assertions. Its actual child exit is 0 with all 11 checks; a separate
completion audit corrects the recognizer without rerunning the browser. Original
logs and results are not overwritten or relabeled. A subsequent probe found a
real bug: removed nodes retained hover/focus and ancestors retained hover.
The final Engine rebuild clears this state and adds three core assertions.
Fresh focused/native/renderer gates pass for these final bytes; the preceding
2,415-check scripting success is retained as historical evidence only, not
substituted for the failed final run.

The standalone browser shell is reused unchanged with these freshly compiled
engine files in a separate experimental deployment. One native smoke invocation
passes startup and a scripted click with title `Lea 1`, the 128-MiB isolated
worker, no loopback exemption and no MSHTML module. Its fresh screenshot shows
the Lea icon, counter value 1 and native fields. This is not a broad-suite pass.
Its package includes this limitations report, the removal fixture and dependency license
notices, excludes test tools/symbols, and is checked entry by entry. The complete
source is backed up separately. Existing normal outputs and the original 67 host
inputs/three installed files remain unchanged. Firespider's six actual-EXE
startup scenarios and 43-case copied-host matrix are **not** newly run here;
the preceding Firespider test candidate still contains the preceding engine.
No installed replacement, independent archive rebuild, stable release, resolved
intermittence or speedup is claimed. Compression affects distribution size, not
proof of execution speed.
