# Element receiver bindings

## Change

`getAttribute`, `hasAttribute`, and the `classList` getter/setter now resolve
their actual receiving element through captured private WeakMap lookup. Borrowing
them from one Lea wrapper no longer silently reads or changes that original
element. Invalid receivers, including document/window, primitives, inherited
lookalikes and proxies, throw the captured `TypeError` before argument conversion.
Attribute methods require a name, convert it once, and reject direct Symbols.
Null names still convert to the string `null`; nullish receivers are invalid.

The receiving-element behavior follows the
[DOM Element interface](https://dom.spec.whatwg.org/#interface-element) and
[Web IDL operation receiver checks](https://webidl.spec.whatwg.org/#es-operations).
The existing own-property attributes are retained, rather than implementing
the complete Element prototype or Web IDL interface surface. The two shared
attribute methods keep their names, arity one, and non-constructible method
semantics. The shared class accessors preserve each element's stable, live
token-list identity and the existing value-forwarding setter.

Four per-element function copies are replaced by shared per-realm functions.
Each private target state owns its lazy token cache, so an inherited
`Object.prototype.classTokens` accessor cannot intercept it. The new helpers
remain private from globals, eval and Function. This is a correctness change,
not a measured memory or startup improvement, and not comprehensive resistance
to all public intrinsic replacement. No public alias, worker protocol,
dependency, remote-script policy or resource limit changes.

## Size and identity

| Item | Before bytes | After bytes | Change |
| --- | ---: | ---: | ---: |
| Readable bootstrap | 30,074 | 31,131 | +1,057 |
| Compact bootstrap | 21,699 | 22,510 | +811 |
| Parent scripting DLL | 90,624 | 91,136 | +512 |
| Worker EXE | 44,032 | 44,544 | +512 |
| Native scripting adapter | 12,800 | 12,800 | 0 |

Source SHA256:
`39960850C2BF456DD9566775E958C8C82F584D2290E0F1191BE05CBB90C79812`.
Compact SHA256:
`B4CB844D30A6E898679C1CFD3AE386F2AB9270B976F062D199940ACB07C0C832`.
Parent SHA256:
`CB0065FC1D6ECDBB85A9F606D905C06CF22BB7E4C0045C34FA1BC54590EB3782`.
Worker SHA256:
`FB297E0285D64C846C80A1839DD8411007A06153E19F9639ECC7A424A7ABF1D7`.
Adapter SHA256:
`3A2C176E5B1AAE99A83BD2F8EDCA5A33C580CBDEDEFF91D7DBB1D55956B7D16F`.

Three of 19 production files change; the other 16 remain byte-identical.
The adapter is dependency-rebuilt with unchanged source; its changed bytes are
not an assembly-equivalence claim. Test and browser outputs match all 19 frozen
final production files. The bootstrap remains a compact, unmangled embedded
resource with no runtime decompression. Its growth is not a size reduction or
evidence of faster execution.

## Verification

- Generator self-tests pass seven syntax round trips and the changed-literal
  control. Final generation/check agree on AST JSON, fixed-point output and
  hashes. Both builds have zero warnings/errors; all four final compiled-resource
  checks pass.
- Final revision B passes 43 focused trusted/isolated/native-view checks and
  all 1,385 ordinary full-source checks. Coverage includes borrowed reads/writes,
  required conversion order, non-constructibility, captured lookup/conversion,
  independent token caches, realm retirement and recovery, and existing event,
  attribute, class-list, tree, timer, microtask and worker gates. Global timing
  trace and startup observation are off; existing internal tracing fixtures
  are unchanged. Final test EXE SHA256:
  `8FF773EFFA682F0FF21D3A9F4615FFD38E3681C119DC00CA5177F7073CD5601F`.
- Native checks retain the edited `Elliot` field, focus and updated accessible
  label while the borrowed setter triggers a 190-pixel reflow and ID rename.
  Nonblank 940/420-pixel captures and the narrow editor bounds pass; both
  captures are visually inspected.
- The actual standalone browser EXE passes inline startup and native keyboard
  counter activation/repaint. Its report confirms AppContainer isolation, no
  loopback exemption, the 128 MiB process limit and no checked MSHTML/WebView/
  Chromium/WebKit renderer module. Its window capture is visually inspected.
  Browser EXE: 405,504 bytes, SHA256
  `3B4333C4A15648517C080C56D3CB754EC1BEE1D8F65BBF0F99B3A5DC3CF7AE7A`.

The isolated-only preceding-runtime control fails specifically at `borrowed
attribute target`, after successful worker startup, with all 19 original
production files unchanged. It uses the initial test harness, not the final B
binary: later test-only preflight/reporting and method-semantics/privacy checks
are not identical. The borrowed-read assertion itself is unchanged. This
demonstrates the original receiver bug without claiming an identical-harness
full-suite comparison.

## Retained failures

Revision A passes its focused checks and browser smoke, but its full source
attempt fails at the first computed script with `limit`. That original harness
omits the diagnostic message, so its precise failure phase is not established.
A later reporting-only edit preserves the message for future failures. Three
separate adjacent A invocations fail during trusted bootstrap with
`TimeoutException`, before their class/event/privacy assertions. None is retried.

Review then finds that A's shared attribute functions are constructible, unlike
the original methods. Final revision B uses method syntax and adds explicit
non-constructor checks. Its new full attempt passes; that substantive revision
does not erase A's failures or establish a startup fix. The trusted 1,000 ms
budget and native 5,000 ms startup deadline remain unchanged.

External result wrappers initially mark both successful B runs incomplete
because their expected summary text is wrong. Original logs/result files are
retained. Separate verification records check the actual zero test exit codes,
exact saved success summaries and hashes, without rerunning either test.

The preceding bootstrap-region investigation also retains a cold trusted
timeout and a warm completion; two native startup-phase samples pass. A last
phase marker is not a root-cause stack. Those observations do not explain or
resolve intermittent cold-start failures, and are not speed benchmarks.

## Preservation and next gate

Evidence is under `H:\Lea Evidence\Lea-146-element-receivers-2026-10-08`, with
baseline/A/B runtimes, source snapshots, failed and passing harnesses, build
and test logs, captures, source archive and checkpoint bundle. The preceding
diagnostic evidence is separately sealed under
`H:\Lea Evidence\Lea-146-bootstrap-regions-2026-10-08`.

No new Firespider host candidate, host matrix, actual Firespider launch matrix,
release package or installed replacement is claimed for B. Earlier v28 host
packages contain the preceding event-receiver revision, not these Element
changes. The original host inputs and installed browser remain untouched.
Next integration must use these exact production bytes in a separate host
candidate and retain every failure. Startup reliability and broader modern-web
compatibility remain open. Milestone 146 stays open; the original limited
baseline stays at 100% and broader readiness stays about 65%, a practical
estimate rather than a standards score.
