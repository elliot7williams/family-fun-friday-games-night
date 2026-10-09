# Literal Attribute Editing

Later decoder prefix validation and current verification are recorded in
`IMAGE_HELLO.md`. Its native core and standalone EXE checks pass, but its complete
scripting gate hits an isolated-worker startup timeout. The results below are
the preceding attribute-edit checkpoint, not proof of a passing full scripting
gate for that later runtime.

Lea now has generic bounded attribute writers, rather than only ID/class
reflection. This is an engine capability, not a startup-timeout fix or a claim
of full browser DOM conformance. Milestone 146 remains open; broader readiness
stays about 65%.

## Compact Integration

| Surface | Set | Remove |
| --- | --- | --- |
| Engine | `E.Sa(page, node, name, value)` | `E.Ra(page, node, name)` |
| WinForms control | `V.Sa(node, name, value)` | `V.Da(node, name)` |
| Optional JavaScript | `element.setAttribute(name, value)` | `element.removeAttribute(name)` |

Set returns 1 for an accepted write and 0 for denial. Remove returns 1 for an
existing attribute removed, 0 for an absent attribute, and -1 for denial.
JavaScript methods return undefined on success, including absent removals.
Standard JavaScript names remain standard; native embedding uses short aliases.
The view already has an unrelated private `Ra` range type, so its removal
alias is `Da`. No existing range implementation is renamed.

```csharp
var node = view.Id("status");
view.Sa(node, "data-state", "ready");
view.Da(node, "hidden");
```

Values are literal strings: no Lea command parsing or `${variable}` expansion.
Writes retain node/wrapper/field identity and edited form values, and use the
existing rebuild path for selector lookup, CSS, layout, form metadata, link
targets, semantic accessibility and native notifications. The `open` attribute
also synchronizes `N.Op` on details elements. Present boolean attributes retain
presence semantics even when their value is the string `false`.

## Admission And Semantics

- Engine-owned connected elements only, including html/head elements. Null,
  document/text, detached and foreign nodes are denied. Scripting permission
  and page ownership remain enforced in the parent.
- New attribute names are ASCII-lowercased, limited to 64 UTF-16 units and
  validated independently in the realm and core. Empty names, ASCII whitespace,
  NUL, slash, equals and greater-than are invalid for set. Other local names,
  including non-ASCII, numeric, quote, colon and less-than names, are accepted.
- Values are limited to 1,024 UTF-16 units, with the existing 512-unit ID/class
  bound retained. A node has at most 128 attributes and 64 Ki UTF-16 units of
  combined names/values. Rejection does not change attributes or counters.
- Removal does not validate name syntax, because a missing name is a successful
  no-op. Lookup names are still bounded to 1,024 units. Missing removals do not
  rebuild or increment accepted-mutation counts; same-value sets do count.
- JavaScript receiver and required-argument checks precede conversion. Name
  then value convert to DOM strings, including null/undefined/BigInt; Symbols
  throw TypeError. Invalid names throw InvalidCharacterError DOMException after
  value conversion. Lea bounds and disconnected targets throw TypeError.
- Methods are shared per realm and non-constructible, preserving the existing
  wrapper descriptor conventions. Borrowed methods use their actual receiver.
  Captured string/error bindings keep these tested conversions working when
  those globals are replaced; protection against every modified intrinsic is
  not claimed.
- New typed `sa` and `ra` bridge operations use the existing version-3 shapes,
  request/value quotas, permission checks and mutation accounting. There are no
  startup-budget, VM-limit, AppContainer, Job, handle or origin-policy changes.

Name rules and removal behavior follow the current
[DOM Element attribute algorithms](https://dom.spec.whatwg.org/#dom-element-setattribute).
This differs from historical XML-Name validation; no XML dependency is added.

## Verification

Evidence is retained separately under
`H:\Lea Evidence\Lea-146-attribute-edits-2026-10-09`.

- The preceding 19-file runtime rejects the new test at the exact missing
  compact Sa/Ra API assertion. The failure and preceding runtime are retained.
- Initial source builds pass. Focused attempts A and B pass core/trusted/isolated
  behavior, then expose native test setup mistakes: a non-focusable paragraph,
  followed by a focusable paragraph for which keyboard `Ka` does not synthesize
  a click. The final handler test uses the existing native input activation
  path. Both failures remain; no general paragraph keyboard-click fix is claimed.
- Build C rejects a WinForms alias collision with the existing private Ra type.
  Final revision D uses Da and builds with zero warnings/errors.
- Final D passes 322 focused checks through `-ae`: core limits/ownership,
  trusted and isolated DOM behavior, retained native editor/text, attribute CSS
  reflow, retained/fresh accessible names, dynamic local inline handlers and
  removal, disclosure state, compact view aliases and nonblank 940/420px paint.
  Fresh desktop/narrow captures are visually inspected.
- Bootstrap generation and AST/resource validation pass. Readable bootstrap
  grows from 31,231 to 32,304 UTF-8 bytes; compact grows from 22,576 to 23,386.
  The compact resource is 27.6% smaller than its readable equivalent, but this
  feature is a size increase. No speed or memory improvement is claimed.
- The final ordinary scripting suite passes all 1,713 checks once, with actual
  exit zero and the exact final 21-file test runtime. This includes the preceding
  1,391 checks plus the 322 new checks. The intentional missing-interpreter
  worker-entry probe at the end is a guarded test, not an unhandled startup error.
- The rebuilt standalone browser produces its expected native counter/paint
  report. Its first wrapper returns before the GUI process exits and retains a
  null exit code; that bookkeeping failure and report remain. After confirming
  the original process is no longer live, a separately declared lifecycle gate
  uses an explicit process handle and passes with exit zero, isolated worker,
  128 MiB memory admission, no loopback exemption and no legacy browser modules.
- The broader `Lea.Tests --core` gate fails at CheckPasteBeyondTabLimit with
  insufficient C: disk space, before completing its suite. C: is measured full.
  Diagnostic archival cleanup is rejected before execution; no candidate or
  user file is removed. Separate storage-light `--attributes`, `--tokens` and
  `--select` model checks pass. They do not replace the incomplete full native
  core regression.
- After the user restores over 3 GB of C: space, the identical core EXE and
  Engine/Win/image dependencies are used in a separately recorded full core
  attempt with fresh NTFS scratch. Paste, stylesheet, load and picture-limit
  checks pass; CheckImageCancellation then fails at `stalled image handshake
  header consumed`. Finally cleanup reports `phase=hello; result=cancel`,
  `ready=0`, roughly 3,030ms wall, pipe read count zero and empty stderr. These
  observations do not identify the cause. A separate ordinary CLR launch of
  the exact 4,608-byte helper produces the expected four-byte header and marker,
  then is explicitly terminated because its design sleeps for 60 seconds.
  That unsandboxed diagnostic does not replace the failed native gate. No
  engine/test/worker limit is changed and no successful full core run is claimed.
  Evidence is separate in `H:\Lea Evidence\Lea-146-attribute-edits-core-2026-10-09`;
  the earlier sealed evidence and storage failure remain unchanged.

Focused commands:

```powershell
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . -ae
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --attribute-edits-view
```

The standalone packaging script includes the new attribute-edit fixture.

## Remaining Limits

The tree's existing case-insensitive attribute dictionary is unchanged, including
non-ASCII case-folding limitations. There are no namespace variants, Attr nodes,
dataset proxy, DOM creation, mutation observers, toggleAttribute or complete
Web IDL/prototype descriptors in this change. The initial HTML source string is
not a serialization of the mutated tree.

Supported inline CSS and current renderer projections refresh; changing head
style/link/base metadata does not reload stylesheets or recompute the document
base URL. Resource/script attributes can be stored, but external/dynamic script
loading is not enabled. Remote scripting remains refused by the WinForms adapter.
Trusted Types, CSP and a full document-origin/resource policy remain future work.

Form defaults/constraints refresh and existing edited values survive, but full
HTML dirty-value/dirty-checkedness, type-transition and selection algorithms are
not implemented. Attribute reflection alone is not proof of those behaviors.
Experimental source tests and a standalone rebuild are not a new Firespider
release, installed replacement, independent package gate, controlled cold-cache
benchmark or resolution of the intermittent 5,000ms worker-startup deadline.
