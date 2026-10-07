# Live Class Lists (In Progress)

This is experimental source work, not a verified browser release. Broader
readiness remains about 65%; milestone 146 remains open. The last verified
candidate remains v13. Its passes do not apply to these changed script binaries.
The installed Firespider browser is not replaced.

## Behavior

The optional interpreter now supplies a stable live `DOMTokenList` for each
wrapped element's `classList`. It implements ordered unique ASCII-whitespace
tokens, case-sensitive membership and writes, variadic atomic `add/remove`,
forced `toggle`, ordered `replace`, unsigned `item`, readonly virtual indices,
enumeration, raw `value`, and `classList` setter forwarding. Empty membership is
false; invalid tokens throw branded `SyntaxError`/`InvalidCharacterError` DOM
exceptions. Class has no supported-token vocabulary, so `supports` throws a
TypeError. Standard DOM names remain familiar; the host API stays compact.

Array `keys/values/entries/forEach` implementations are captured native functions,
not a new iterator engine. Iterators are live and stay completed after exhaustion;
`forEach` snapshots length and observes current indexed values. Native lookup,
string conversion, proxy/reflect operations and private WeakMap access are
captured before page code can replace globals. Parsed class arrays are not
cached per node. Reads observe current attributes, including external `cn` writes.

Writes reuse `cn` and its 512-UTF-16-unit ceiling. Bulk changes preserve field,
editor and edited-value identity through existing CSS/layout/accessibility
rebuilds. Missing-attribute and forced no-op operations do not rebuild. Legacy
`ac/rc/tc`, the literal DOM translator and case-insensitive core CSS matching are
unchanged; this is not complete browser DOM or selector conformance.

`DOMException` includes native Error branding, private name/message state,
legacy code mapping, readonly legacy constants, accessors, subclassing and
required-new behavior. Prototypes are their final object literals, avoiding
descriptor-copy loops and eager class evaluation. An Error formatter shim fixes
Jint's wrong accessor receiver while retaining generic receiver checks and the
name-conversion-before-message-read order. It replaces the public writable
`Error.prototype.toString`; it does not patch Jint's CLR implementation.

## Boundaries

There is no new renderer dependency, core alias or worker protocol version.
The existing 16 KiB user-code, 256 bridge-call, 512 handle, 20,000 statement,
8 MiB interpreter-memory and one-second trusted-interpreter execution limits
remain. Isolated startup keeps its existing five-second deadline, AppContainer,
native Job and network-denial gates. Tests use the existing maximum `Jp.Tm=5000`;
production defaults and budgets are not increased.

Each indexed/length read consumes bridge work. Large iteration is therefore
bounded by the existing call ceiling; it is not unlimited. Remote/external
scripts remain disabled pending document-origin/resource policy.

## Evidence

Logs are retained under `dist/Bench/Lea-146-classlists-wip-evidence`.

- `Lea-classlist-red.log` reproduces the original empty-membership bug.
- `Lea-classlist-replace-warm-red.log` reproduces whitespace-before-empty
  replacement validation. Empty arguments now take precedence for `replace`,
  while `add/remove` retain per-argument validation order.
- `Lea-classlist-forward-warm-red.log` reproduces early string conversion before
  an overridden `value` setter. Forwarding now invokes that setter unchanged.
- `Lea-classlist-subclass-warm.log` isolates the native formatter's wrong
  receiver. Direct construction, branding and conversion pass before the
  subclass stringification failure; the formatter regression passes afterward.
- An earlier batched variant passes 61 in-process/isolated/native checks. That
  result does not verify the subsequently expanded source or its release gates.
- `Lea-classlist-final-warm.log` passes all 47 in-process checks, including exact
  descriptors/arity, exception constants, conversion order, 100-token bulk
  writes, global replacement, atomic size rejection and retained fields. The
  worker then times out: hello wall 5,012 ms / CPU 1,140 ms, last phase evaluate.
  The earlier formatter run also times out at 5,016 ms / CPU 1,000 ms.
- `Lea-classlist-final-native.log` passes all five native-view checks. Desktop
  940px and narrow 420px screenshots are inspected: the edited field remains
  visible, correctly bounded and retained. This successful worker launch does
  not explain the isolated-suite timeouts or prove consistent startup.
- `Lea-classlist-core-full.log` passes the complete core suite. The required
  engine and WinForms sources are unchanged in this round.
- `Lea-classlist-full-source.log` fails the normal full suite at its first
  computed script with status `limit`. No full-suite pass is claimed.
- `Lea-classlist-final-full-source.log` gets farther, then fails the first fresh
  worker execution in the startup suite: hello wall 5,013 ms / CPU 468 ms,
  last phase evaluate. This does not explain the earlier cold failure.
- `Lea-classlist-solution-build.log` compiles with zero errors and 18 existing
  prototype warnings. The script test project compiles without warnings.

The public Error formatter change still needs the complete scripting regression
suite; passing focused checks does not establish that broader compatibility.
No host/startup matrix, extracted runtime suite or independent host-source rebuild
is run for this unfinished variant. No new candidate or release ZIP is produced.

Bootstrap tracing distinguishes cold and warm behavior but is not a speed
benchmark or release gate. A previous-bootstrap comparison completes at
1,800 ms cold / 11 ms warm; an instrumented expanded sample times out at
2,491 ms / 234 ms CPU and completes warm at 353 ms / 46 ms CPU. Timings include
construction/parsing/setup, not solely the one-second execution window. Trace
calls change execution. Neither a common cause nor stable cold readiness is
established. A system snapshot finds about 245 MB free on C:, but does not prove
that storage caused any timeout. Builds and timed tests run separately.

Earlier String-prototype poisoning and disabled-permission failures were test
setup mistakes and were corrected. They are retained as such, not engine fixes.

## Commands

From the repository root after building Release:

```powershell
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --class-start
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --classes
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --classes-view
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --classes-warm
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --class-profile
```

`--class-start` invokes the unmodified private production Start path.
`--classes-warm` explicitly retains an initial cold timeout and continues with
fresh-realm functional diagnostics in the warmed process; it is not a release
pass. `--class-profile` runs instrumented setup twice, reports exceptions, and
returns zero even when a diagnostic sample fails. Neither bypass belongs in the
browser, normal `--classes` check, or normal full-suite release gate.

Next: reduce cold bootstrap work while preserving observable interface
descriptors, captured intrinsics, execution budgets and isolation. Then repeat
normal source, native-view, host/startup, extraction and source-rebuild gates
before creating a new candidate. Source backups and Git retain the unfinished
implementation independently of release archives.

## References

- [DOMTokenList algorithms](https://dom.spec.whatwg.org/#interface-domtokenlist)
- [Web IDL interfaces and value iteration](https://webidl.spec.whatwg.org/)
- [Pinned Jint Error formatter](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Native/Error/ErrorPrototype.cs)
- [Pinned Jint time constraint](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Constraints/TimeConstraint.cs)
