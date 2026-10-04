# Live Element Attributes

Milestone 146 remains experimental. Broader browser readiness stays about 65%,
not a web-standards score. This follow-up adds bounded ID/class reflection to
the optional JavaScript bridge while keeping short typed aliases in the core.

## Implementation

`E.Jv(page, element, "id", value)` replaces the ID attribute; `"cn"` replaces
the raw class attribute. Both accept at most 512 UTF-16 units on connected
engine-owned element nodes, including root/nonpainted elements. Null literal
values, document/text nodes, foreign/detached nodes and disabled scripting are
rejected without an accepted-mutation count or attribute change. Empty values
leave present empty attributes. Values are not Lea code or interpolated strings.
These are typed mutations, not new literal `E.Js` command statements.

The existing rebuild path refreshes selector lookup, CSS, layout, fields and
accessibility label associations. Nodes, JavaScript wrappers and retained field
objects keep their identity and edited values. No new interpreter dependency
is added to the core or WinForms view.

Optional element wrappers expose live writable `id` and `className` plus
`hasAttribute(name)`. Attribute presence is distinguished from an empty value
and the existing missing-attribute `null` result. The shared DOM string-write
helper and attribute-name reads use the captured native String function;
direct Symbols throw before a host call. Objects convert in the realm before
literal parent mutation; `null`, `undefined` and BigInt ID/class values become
strings. Text content keeps its earlier null-to-empty special case. This is
not a claim of protection against all page-modified intrinsics.

The behavior is guided by the [DOM Element reflection definitions](https://dom.spec.whatwg.org/#interface-element)
and [HTML attribute reflection](https://html.spec.whatwg.org/multipage/common-dom-interfaces.html#reflecting-content-attributes-in-idl-attributes).
Lea's existing case-insensitive ID/class matching and attribute dictionary,
32-token/64-character single-token classList mutation bounds, and incomplete
IDL/prototype/namespace surfaces remain limitations. Replacing className keeps
raw whitespace and duplicate tokens; subsequent classList mutations retain
their earlier normalization behavior. SVG animated className, generic
setAttribute/removeAttribute, dataset, DOM creation and URL writers are not
implemented by this change.

Parent capability operations and wire version 3 are unchanged. Existing
startup/execution budgets, AppContainer/Job constraints, ownership, permission
generations, request/value limits, and remote-origin refusal remain active.

## Source Verification

Logs remain in `%TEMP%`, including failed attempts; later passes do not erase
or explain unrelated failures.

- `Lea-attributes-red.log` reproduces the exact missing reflection assertion
  before the production change; its wrapper requires that specific failure.
- `Lea-attributes-green.log` exposes a test syntax error: C# embedded literal
  newlines in a JavaScript quoted class value. The fixture now uses JS escapes.
- `Lea-attributes-core-green.log` passes the focused typed-core checks.
- `Lea-attributes-final-green.log` fails the first trusted in-process call with
  `limit; Script budget exceeded`. It overlapped the end of the focused core
  process; the actual cause is unknown, not proven to be test competition.
- `Lea-attributes-serial-green.log` passes the in-process suite, then fails the
  isolated suite with `off`: the permission test left `Je=false`. Resetting
  permission between modes fixes that identified test setup error.
- `Lea-attributes-permission-green.log` passes the then-current 58 script checks.
- `Lea-attributes-native-green.log`, `Lea-attributes-native-stack.log` and
  `Lea-attributes-native-symbols.log` expose a test's null WinForms accessibility
  root. Stack-preserving rethrow and a full-symbol diagnostic build identify
  the lookup. The test now uses Lea's existing `Ao` accessibility alias.
- `Lea-attributes-native-api-green.log` passes all 13 native checks, including
  retained editor/user text, CSS-width reflow, retained/fresh accessible labels,
  live metadata in input/click handlers, ID restoration, and nonblank desktop/
  narrow rendering. The 940/420-pixel screenshots are visually inspected.
- `Lea-attributes-solution-build.log` builds the solution and standalone browser
  with zero errors and 18 pre-existing warnings in unchanged prototype projects.
- `Lea-attributes-full-script.log` passes all 703 normal source checks, including
  60 current in-process/isolated attribute checks and 13 native attribute checks.
  This is one full pass, not proof of stable cold startup or a speed improvement.
- `Lea-attributes-full-core.log` passes the complete `--core` regression suite,
  including the new typed alias tests and existing layout/native/network checks.

Focused commands:

```powershell
.\Lea.Tests\bin\Release\Lea.Tests.exe . --attributes
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --attributes
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --attributes-view
```

`Pack-LeaScript.ps1` now includes `javascript-attributes.html`, so the full test
entry point has all three JavaScript fixtures in a newly packaged runtime.

## Separate Candidate

`Lea-attributes-v11-candidate-build.log` compiles the separate net48 enabled
host with 67 declared source inputs. Its runtime is
`dist/Firespider/Lea-146-attributes-v11/runtime`; the EXE SHA256 is
`FA4977907FAAB73781F27AF2F7365A075441FEBABDBE84D3EBC2B5396CE11474`.
The required Engine/Win/image bundle stays 742,400 bytes. The optional worker
is 35,328 bytes, 512 more than v10; binary sizes are not latency measurements.

`Lea-attributes-v11-host-all.log` passes all 43 host cases, including live
attribute writes, timers, error recovery, split isolation and real 30-second
snapshot/hibernation timers. First script demand is 5,028 ms and split load wait
4,248 ms; these are aggregate multi-call observations, not per-worker deadlines.
`Lea-attributes-v11-exe-startup.log` passes all six actual-EXE scenarios:
clean/restored/locked/oversized/lazy-background/lazy-all. Each stays responsive
without loading the legacy engine; lazy-all starts deferred and resumes. Readiness
samples are 4,344..6,553 ms, not an SLA or proof of stable startup on other machines.

## Experimental Archives

All three archives pass exact file inventory, length and SHA-256 checks against
their manifests and current build snapshots. Distribution storage is not a
worker launch location; extract to local NTFS/ACL-capable storage.

| Archive | Files | Bytes | SHA256 |
| --- | ---: | ---: | --- |
| `dist/LeaScript/Lea-146-attributes-v11/Lea-js-146-native_2026-10-04_03-48-20.zip` | 41 | 2,981,684 | `AAB4BECDF25288CF1D66B03B0CE23EF41C961935953C13F67BF0946EBFC5B4FF` |
| `dist/Firespider/Lea-146-attributes-v11-packages/Firespider-Lea-146-script-runtime_2026-10-04_03-48-46.zip` | 372 | 3,691,973 | `3A6A165526C2B3A4FF1E97FFFD86579C8CEDCDB2F516D93A3D267C5F6CDBB832` |
| `dist/Firespider/Lea-146-attributes-v11-packages/Firespider-Lea-146-script-host-source_2026-10-04_03-48-46.zip` | 104 | 3,342,801 | `75EC05018BDA4D952981CF089235BDBC395C2F0414DD989F3DBBA568267A1E16` |

The 35,328-byte worker hash is
`F23D492FDBA3071F2C5E36A90064B70EB2C41D31DD4EBE8D0FF42D78DC105F99`.
`Lea-attributes-v11-extracted-full.log` passes all 703 checks from the fresh
standalone extraction. `Lea-attributes-v11-extracted-host-script.log` passes the
fresh extracted Firespider scripting case. `Lea-attributes-v11-independent-build.log`
rebuilds the host-source ZIP using the installed Framework MSBuild and 4.8
reference assemblies, without the live project. Its dependency hashes match the
runtime manifest. After copying 348 manifest-verified fixture/license files from
the runtime package for the harness, `Lea-attributes-v11-rebuilt-host-script.log`
passes the rebuilt host's scripting case. The host source package contains VB
source and compiled Lea dependencies; the full Lea C# source is backed up separately.

The full 43-case and six-EXE matrices are verified on the staged candidate, not
repeated on both extractions or the independent rebuild. These passes do not
diagnose earlier cold-start/budget failures, establish remote-site compatibility
or complete milestone 146. Installed root EXE/Engine/Win hashes still match their
preserved values. The live project and browser remain untouched.

See [progress](PROGRESS.md), [startup tracing](STARTUP_TRACE.md) and
[candidate instructions](../Lea.Firespider/CANDIDATE.md).
