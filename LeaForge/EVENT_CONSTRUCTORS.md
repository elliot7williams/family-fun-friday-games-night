# Non-constructible shared event operations

Lea's shared `addEventListener` and `removeEventListener` now use method syntax
instead of constructible function expressions. They remain shared once per realm
through a private method table, retaining names, length two, default options,
receiver lookup, argument conversion, listener ownership and existing descriptors.
There is no new public alias, host capability, protocol or security exception.

This corrects constructor behavior, not complete EventTarget interface bindings.
The existing own/nonwritable/nonenumerable/nonconfigurable operation descriptors
are unchanged rather than redesigned into the full standard prototype surface.
The [Web IDL operation-function algorithm](https://webidl.spec.whatwg.org/#es-operations)
creates builtin operation functions; these are not ordinary JavaScript constructors.

## Regression evidence

The event suite now checks both methods, bound methods and all ten handler
accessors. They expose no own `prototype`, reject use as `Reflect.construct`'s
new target, and reject direct construction before argument conversion or callback
execution. Rejection leaves the document unchanged. Borrowed calls, shared
metadata, nullish/global receivers, listener removal, cancellation, separate
targets, typed handlers and poisoned public intrinsics continue to pass.

`--event-receivers-isolated` adds a test-only isolated mode. Existing trusted and
complete modes retain their behavior. The exact final test executable is used
for both old-runtime and updated-runtime isolated controls:
`4B9E86E1784D9A6ACA4DA0B05404798C81493D45BE06903BF4B4E1832C9B3470`.

- The frozen preceding runtime starts successfully, then fails at
  `event operation constructor prototype`, not a startup timeout. Its 19
  production files remain unchanged; no failed sample is retried.
- The updated runtime passes all 25 isolated event checks with that same binary.
- The ordinary complete Lea.Script suite passes all 1,391 checks, including
  the new constructor assertions in trusted, isolated and native-adapter modes.
  Its run is `2026-10-09T07:27:48.1952763Z` to `2026-10-09T07:32:20.4969277Z`,
  without startup observation or external timing trace. This is the scripting
  suite, not a newly executed full HTML/CSS/core compatibility suite.
- The rebuilt actual standalone browser EXE passes inline loading, native
  keyboard counter activation and repaint. Its inspected capture is nonblank
  with counter one. The report verifies AppContainer isolation, no loopback
  exemption, the 128 MiB limit and no checked MSHTML/WebView/Chromium/WebKit module.
  Browser EXE: 405,504 bytes, SHA256
  `8C023320D8678D1E6B15204A4DBF2457A1DFEDF9D0ED0D7461F7FC821317BAE9`.

All source builds use the existing offline dependencies and pinned build revision
`9fda9e3d886c6c1cd63a8d90a93a054f2772d3d2`. Generator write/check, seven round
trips and changed-literal control pass; all four parent/worker embedded resources
in test/browser outputs match the generated compact file.

## Size and unchanged boundaries

Readable bootstrap: 31,231 bytes, SHA256
`D904441DA256FC750B86524C35C89270518D3D89D5C7DF99DF94F608657FAFCA`.
Compact bootstrap: 22,576 bytes, SHA256
`60B97515B42374D4C160010327E78985EC3A855B3FA909CD392BD6E4958FB3B7`.
Compared with the preceding source, readable bytes grow 100 and compact bytes
grow 66. No compressed package-size reduction or runtime speed/memory gain is claimed.

Only three of 19 production files change: parent script DLL 91,136 bytes, worker
EXE 45,056 bytes (+512), and dependency-rebuilt adapter 12,800 bytes. Adapter
source is unchanged. The other 16 files exactly match the preceding runtime.
Test EXE size is 440,320 bytes; it is not part of the engine's required bundle.
Native 5,000 ms startup, trusted 1,000 ms bootstrap, execution defaults, memory,
CPU hard cap, process count, restricted token, AppContainer, exact pipe list,
ASLR and mandatory retirement are unchanged.

## Separate startup investigation

Before this source change, one explicitly diagnostic whole-script observation
runs the unchanged independent v30 runtime that failed its ordinary release
gate. It passes, capturing eight workers, 208 samples and 44 thread snapshots,
without dropped requests/samples or observer errors; the sampler retires.
The exact host harness remains unchanged, and no production runtime is modified.

The first worker reports parent startup 3,078 ms wall/593 ms CPU. Its worker
trace spans 701 ms wall/94 ms CPU from evaluate to parsed, then 1,040 ms wall/
218 ms CPU from parsed to DOM completion. Observation has overhead: a sample
takes up to 165 ms, with up to 108 ms queue lag. Page-fault counters do not
distinguish hard from soft faults; sampled thread states do not identify a
root-cause stack or prove scheduler, disk or JIT attribution.

No timeout is captured in that diagnostic attempt, so it neither explains nor
overrides the retained ordinary independent-build failure at 5,012 ms. That
release gate remains failed and its later startup gates stay held. Passing
the new ordinary source suite likewise does not establish a cold-start fix.
See [HOST_PACKAGE_ELEMENT_RECEIVERS.md](HOST_PACKAGE_ELEMENT_RECEIVERS.md).

Diagnostic evidence is under
`H:/Lea Evidence/Lea-146-independent-startup-observed-2026-10-09`. Source-fix
evidence is under `H:/Lea Evidence/Lea-146-event-constructors-2026-10-09`, with
old/current runtimes, final harness, logs/results, generated sources/resource
audits, inspected browser capture, full source archive and checkpoint bundle.
Three bootstrap source snapshots were initially copied into the frozen runtime;
they are moved, byte-identical, to a separate source snapshot folder before final
inventory/sealing. The original placement record is retained; no failed runtime
test is repeated for that evidence-layout correction.

The original 67 Firespider source inputs and three installed binaries remain
unchanged. No new Firespider candidate, host matrix, host release ZIP, independent
host rebuild pass or installed replacement is claimed for this source change.
Next integration must use the exact tested production files in a separate host.
Startup reliability, safe internet-page scripting/resource policy and broader
site compatibility remain open. Milestone 146 stays open and broader readiness
stays about 65%, a planning estimate rather than a web-standards score.
