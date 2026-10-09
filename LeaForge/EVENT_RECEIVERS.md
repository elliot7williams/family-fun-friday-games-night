# Event receiver bindings

## Change

`addEventListener`, `removeEventListener`, and the five supported `on*` accessors
now resolve their receiving object through the private event-state WeakMap.
Borrowing a method or accessor from another Lea node no longer silently operates
on the original node. Invalid receivers, including proxies and objects inheriting
from a node, throw the captured `TypeError` before event-argument conversion.
Null or undefined receivers use the associated realm global, including detached
method calls. Element, document, and window state remain independent.

This follows the receiving-target model in the
[DOM event methods](https://dom.spec.whatwg.org/#interface-eventtarget), the
[Web IDL receiver conversion](https://webidl.spec.whatwg.org/#es-operations),
and the [HTML handler accessors](https://html.spec.whatwg.org/multipage/webappapis.html#event-handler-idl-attributes).
It does not implement the entire EventTarget interface or its prototype layout.
The existing own-property attributes, supported event whitelist, 128-listener
limit, option conversion, cancellation, ordering, and realm-reset behavior stay
unchanged. No public alias or global is added or renamed.

The realm creates one shared set of two methods and ten accessor functions,
instead of recreating those functions for every target. One captured
`Object.defineProperties` call installs the shared null-prototype descriptors
on each target. Each private state gains its node reference. Event dispatch also
uses the captured state lookup, rather than a page-replaceable WeakMap method.
This removes repeated function construction, but is not a measured memory or
startup improvement, nor universal protection against intrinsic replacement.

## Size and identity

| Item | Before bytes | After bytes | Change |
| --- | ---: | ---: | ---: |
| Readable bootstrap | 29,609 | 30,074 | +465 |
| Compact bootstrap | 21,241 | 21,699 | +458 |
| Parent scripting DLL | 90,112 | 90,624 | +512 |
| Worker EXE | 43,520 | 44,032 | +512 |
| Native scripting adapter | 12,800 | 12,800 | 0 |

This correctness change is not a compressed-size reduction. There is no new
runtime dependency or decompressor. The generated resource remains identical
between parent and worker, under its existing resource identity.

Source SHA256:
`7B8149510F9737EA4911CA0CF7F0531D32197A1AC66BFD6AA473CA6E1D23B47A`.
Compact SHA256:
`B63F860998DF384257DBE7F06E68BE5A31D6172BC411FEE165B5125E2FB478E3`.
Parent SHA256:
`692E0AC7721FC79BE898313C6DCCCF6A474D6C8308D0219359C1B21D6CF56AE8`.
Worker SHA256:
`D2ED43A68C90CA7BB1C5DC2DEE08C4946B3E2A29CDD81B44CFBB18178F4B6206`.
Adapter SHA256:
`F497920932D96D7F4E514B3CEC8F25377E198ACB82A54A75FDC1A93DFDC421A2`.

Three of the 19 production files change; the other 16 remain byte-identical.
The adapter rebuild changes its hash without a size or source change. Its three
source files match the preceding source audit, and all 26 method signatures,
IL bodies, local signatures, max-stack values and method attributes match the
preceding DLL. This is not byte identity or a full-assembly equivalence proof.
The initial audit's incorrect two-file expectation is retained, corrected on
the same build bytes before any runtime checks.

## Verification

- Generator self-tests pass seven syntax round trips and the changed-literal
  control; generation and check agree on AST JSON, fixed-point output and hashes.
- Test and standalone-browser builds pass with zero warnings or errors. All
  four compiled-resource checks pass.
- The focused suite passes 75 checks across trusted, isolated, and native-view
  runtimes. It covers borrowed add/remove and handler accessors, fake/proxy
  receivers before conversion, nullish/default-global binding, shared function
  metadata, independent state, bubbling, cancellation, captured intrinsics,
  actual keyboard activation, native isolation, and desktop/narrow paint.
- The ordinary full source suite passes all 1,342 checks, including the new
  group and existing event-option/listener/timer/DOM tests. No external timing
  trace or startup observer is enabled; existing fixtures retain their own
  tracing modes. Final test EXE SHA256:
  `2FC5ACFAD06F4661BF715094572FED4F23ABF5ABBF68B06083B9FECD4E7A907A`.
- The actual SDK-output browser EXE smoke passes inline startup and native
  keyboard/counter repaint. Its report confirms AppContainer use, no loopback
  exemption, the 128 MiB process limit, and no checked MSHTML/WebView/CEF/WebKit
  browser module. The desktop, narrow, and actual-window captures are inspected
  and nonblank. Browser EXE: 405,504 bytes, SHA256
  `6BC653FEDEC234B517FA3070C33C4BDE266C41AA79EB5B9154DE7E8FAA67BB58`.

The old-runtime negative control uses the same final test EXE and the preceding
19 production files. It runs trusted-only from the H: evidence directory, with
no native worker. It fails during bootstrap at the unchanged trusted budget:
`Script budget exceeded. phase=bootstrap; reason=TimeoutException`, before the
receiver assertion. This control is inconclusive, not a demonstrated old/new
bug differential. The failure and result are retained, with no retry or limit
increase. Passing new-runtime checks do not erase that failure.

## Fresh-worker comparison

Three opposite-order pairs use the same current parent/harness, the frozen
preceding worker, and seven byte-identical worker dependencies. All six planned
fresh-NTFS-path launches pass all 27 checks, including unchanged isolation and
defaults, process retirement, and exact ACL cleanup. There are no retries.

| Pair | Baseline wall ms | Current wall ms | Baseline CPU ms | Current CPU ms | Baseline peak bytes | Current peak bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 2,960 | 2,344 | 296 | 156 | 32,018,432 | 32,133,120 |
| 1 | 2,514 | 2,417 | 171 | 171 | 32,182,272 | 32,133,120 |
| 2 | 2,484 | 2,333 | 109 | 156 | 32,202,752 | 32,202,752 |

These six observations do not control OS/CLR caches or machine load and do not
establish a general speed or memory improvement. Fresh paths are not controlled
cold caches. Earlier five-second native startup failures remain unresolved.

## Preservation and next gate

Evidence is under `H:\Lea Evidence\Lea-146-event-receivers-2026-10-08`, including
the preceding and tested runtimes, original audit abort, inconclusive control,
build/test logs, captures, full source archive, checkpoint bundle and sealed
inventory. Source backups use H: because C:, L: and F: have little free space.

No new Firespider host candidate, host matrix, actual Firespider EXE launch
matrix, release package, or installed replacement is claimed for these bytes.
The 5,000 ms native startup deadline and all native/JavaScript isolation and
resource limits remain unchanged. The next integration gate must use these
exact production bytes in a separate host candidate and retain every failure.
Remote-script policy and remaining compatibility work are still open.

Milestone 146 remains open. The original scoped baseline stays at 100%, and
broader lightweight-browser readiness stays about 65%, a practical estimate
rather than a web-platform conformance score.
