# Short Private Bootstrap Prototype

Milestone 146 remains open, with broader readiness about 65%. This is a
separate copied-project experiment, not the normal engine default or a release.
The readable source, ordinary exact-AST generator and installed Firespider
remain unchanged. No browser runtime dependency is added.

## Purpose

The preceding timed full-run failure reached its parsed-AST callback at child
wall 4,390 ms, after evaluation entry at 2,124 ms. The gap includes parser and
related evaluation setup; it is not a pure parser benchmark or proven root
cause. This prototype reduces input size without switching interpreters,
widening budgets, moving startup outside its Job or executing page code early.

Developer-only Terser 5.46.1 shortens private lexical bindings with optimization
and property mangling disabled, top-level/module modes off, eval mangling off,
and function/class names preserved. Acorn 8.15.0 and ESLint Scope 8.4.0 verify
normalized structure and resolved declaration/reference identities. Public
property names, unresolved globals, operators, literals, strict scopes and
explicit/inferred declaration function names must match. Direct eval/with are
rejected. Twelve positive/negative controls and deterministic generation pass.
This is not a universal semantic-equivalence proof; runtime tests remain gates.
See [the tool](../Lea.Build/Bootstrap/Short/README.md),
[Terser options](https://terser.org/docs/options/) and
[ESLint Scope](https://github.com/eslint/js/blob/main/packages/eslint-scope/README.md).

All Node dependencies are developer-only, pinned with a committed lockfile.
Install scripts are disabled. The normal browser/Visual Studio build still
uses committed JavaScript and does not need Node or these packages.

## Size

The prototype is generated only inside a separate exact 590-file source copy:

- Compact JavaScript: 28,165 to 21,766 bytes, saving 6,399 bytes (about 22.7%).
- Copied `Lea.Script.dll`: 102,400 to 96,256 bytes, saving 6 KiB.
- Copied worker: 51,200 to 45,056 bytes, saving 6 KiB.
- Required renderer: unchanged 749,568 bytes; other 17 runtime entries frozen.

Only the copied `Dom.compact.js` and its generated hash property differ from
the normal source. Readable `Dom.js` is hash-identical. The short payload's
SHA-256 is
`959DE554A0C223F7DEF82E0174A604CC5ECB47E9DEE0E1833BD340E1EE3F4463`.
No normal generated asset or default project file is replaced.

Prototype binary SHA-256:

| Entry | SHA-256 |
| --- | --- |
| Lea.Script.dll | 912F6E75EDED7139F3C2D7FB78A816406E9183251C22797F50946A29A91708D4 |
| ScriptWorker/Lea.Script.Worker.exe | 7D5E0B386BAC577D10CCDF7C7B906BC80E657D439F06884389A9F03B3B6F1CC6 |

## Verification

All prototype gates have actual exit 0 and expected completion counts:

- Asset identity four; direct entry four; hook checkpoints eight; VM options 16.
- Private scopes 36; events 136; options 75; receivers 81; callback calls 90.
- Allocation diagnostic 23; ordinary trusted 26; isolated 28; native nine.
- Native CPU 25; startup phase 16; worker 87.
- One full scripting regression: all 3,105 checks.

The allocation diagnostic retains its explicitly separate five-second CLR
measurement scope. Ordinary trusted startup is still one second; native hello
still five seconds. Default `Be=0`, zero-capability AppContainer, 128-MiB Job,
CPU-rate, native guards, bridge/handle bounds and verified hello before page
execution remain unchanged. No inspector, fallback or failed-gate retry is used.

The ordinary standalone browser smoke also passes with the copied prototype:
three animation frames, no pending callbacks, retained counter, verified
isolation and no `mshtml.dll` in the reported host modules. Native event and
browser captures are visually checked, including narrow field retention.
The displayed animation elapsed value is not startup time.

Two initial copied-build failures are retained: missing generated NuGet
framework imports (`MSB3644`), then a missing image-helper post-build input
(`MSB3030`). Existing locked imports and the frozen helper are supplied only
to the copy; no targeting pack is installed or target framework changed.
Successful repaired builds have no warnings/errors. The engine projects are
not rebuilt merely to repair the test post-build copy.

## Limits And Next Work

One successful full run and a smaller payload do not prove reliable cold
startup, explain the preceding failures or establish an execution speedup.
Repeated controlled comparisons and copied Firespider validation are still
needed before deciding whether to promote this pipeline. Normal-source
freshness and Visual Studio packaging need their own verified promotion plan;
the exact-AST normal generator is not silently weakened.

The preceding failures remain evidence. All 590 existing source files,
67 original Firespider host inputs, three installed files and 21 normal outputs
stay hash-identical. New developer tooling, lockfile and this report are added
to the complete source backup, verified entry by entry. No runtime release ZIP,
installed-browser overwrite, readiness increase or completion claim is made.

Evidence: `H:\Lea Evidence\Lea-146-short-bootstrap-2026-10-12`.
Private test browser:
`L:\Lea Native Checks\Lea-146-short-bootstrap-2026-10-12-browser\Lea.Script.Browser.exe`.
