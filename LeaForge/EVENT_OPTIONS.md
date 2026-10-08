# Event Listener Conversion

The optional JavaScript bootstrap now converts event-listener arguments before
changing the listener list. This fixes a reproduced bug: the preceding runtime
treated truthy primitive capture options other than `true` as bubbling options.
The regression reported phases `1,3,3,3,3,3` instead of six capturing phases.

## Behavior

- Non-object options use boolean conversion. Numbers, strings, BigInts, and
  Symbols do not consult boxed prototype properties. Null and undefined retain
  the empty-dictionary defaults; boxed values and functions use dictionary reads.
- Addition reads `capture`, `once`, `passive`, and `signal` once, in inherited
  dictionary order, even for duplicate or null callbacks. Removal reads only
  `capture`. Type conversion precedes callback validation and option conversion.
- Both methods require two arguments, convert the type to a DOM string once,
  accept null/undefined callbacks, and reject non-object non-null callbacks.
  Captured conversion/error helpers survive replaced public globals. Method
  names and required arity are `addEventListener`/`removeEventListener` and two.
- Getter exceptions retain their original identity. Failed argument conversion
  does not install the requested listener. Side effects performed by a supplied
  getter are not rolled back.
- Any supplied, non-undefined `signal` is rejected. AbortSignal remains unsupported;
  invalid falsy values are no longer silently ignored. Existing duplicate identity,
  once/passive behavior, six supported event types, and the 128-listener ceiling
  are retained.

The conversion follows the [DOM EventTarget definitions](https://dom.spec.whatwg.org/#interface-eventtarget)
and [Web IDL union](https://webidl.spec.whatwg.org/#es-union),
[dictionary](https://webidl.spec.whatwg.org/#es-dictionary), and DOMString rules
for this supported subset. It is not a complete Web IDL/EventTarget implementation:
receiver/prototype conformance, custom dispatch, AbortSignal, and error notification
are outside this change. Standard JavaScript names remain standard; the compact
embedding aliases and private bridge commands are unchanged.

## Verification

- The old compiled runtime fails the new primitive-capture regression before the fix.
- `--event-options` passes 75 checks: the same suite in trusted, isolated, and
  native-adapter execution, plus real keyboard activation, isolation checks, and
  nonblank 940/420-pixel rendering checks.
- The complete source run passes 1,157 checks. The new group runs last, after
  existing startup, DOM, worker, event, microtask, timer, view, and budget tests.
- A fresh extraction passes the same complete 1,157-check suite. Its actual
  browser EXE also passes smoke mode: local inline initialization, native counter
  activation, isolated worker, and a visually inspected nonblank capture.
  Stage, ZIP-stream, and extraction inventories/hashes cover all 45 payload files
  plus the manifest. Only the two scripting binaries differ among 19 audited
  runtime dependencies; the other 17 are byte-identical to the preceding checkpoint.
- Current source, worker, and test builds have zero warnings and errors. No
  interpreter package, public API, deadline, mitigation, or remote-script gate changed.

This full pass does not explain the intermittent startup failure preserved at the
preceding checkpoint. No cold-cache experiment or startup speedup is claimed.
The original test package is separate from Firespider. Subsequent v22 verification
passes complete 43-case host and six-scenario EXE startup runs on both the copied
candidate and fresh runtime extraction, plus the independently rebuilt archived
host's script and six EXE startup cases. Native host checks cover the conversion
behavior; see [HOST_EVENT_OPTIONS.md](HOST_EVENT_OPTIONS.md). No installed-browser
replacement or intermittent startup fix is claimed.

## Size And Provenance

Readable bootstrap: 30,251 bytes; compact bootstrap: 21,688 bytes, 28.3% smaller,
with matching parsed structure and reproducible generation. The compact asset
grew 583 bytes to add the conversion behavior. It is embedded directly, without
runtime decompression or identifier mangling.

Parent `Lea.Script.dll`: 90,112 bytes; worker EXE: 44,032 bytes. Each is 512 bytes
larger than the preceding frozen checkpoint. Both were built from `cc19554` plus
these uncommitted source edits; the preceding worker retained older SDK Git
metadata, so binary differences are not solely a JavaScript-source comparison.
Other runtime dependencies are audited separately. A smaller archive is not
evidence of faster execution.

Milestone 146 and broader readiness remain open/about 65%. The installed
Firespider browser has not been changed by this checkpoint.
