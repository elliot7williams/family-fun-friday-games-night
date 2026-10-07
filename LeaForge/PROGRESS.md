# Lea 1.0 Progress

Scoped Lea 1.0 baseline completion: **100%**

Post-1.0 hardening milestones completed: **145**

Broader lightweight-browser readiness: **about 65%**. This is a practical
project estimate, not a standards conformance score. Full Chrome/Safari-class
web-platform parity is much farther away and is not Lea's size target.

The table below measures the original Lea 1.0 scope: a small custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, memory LRU and optional persistent cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled relationship, bounded strict relational/functional, An+B/reverse, filtered structural, and terminal `::marker`/`::before`/`::after` selectors; declarations with two-tier important cascade, inherited custom properties, bounded style and size container queries, width/height/orientation/aspect-ratio/resolution/theme/motion/contrast/scripting/pointer/hover media with whole-query `not`, strict `only`, fixed screen/print semantics, and `Dp/Th/Rm/Pc/Je/Pt/Hv/Ap/Ah/Mi`, `@supports`, and named `@layer` blocks with normal/unlayered/reversed-important order, bounded named/hex/RGB plus legacy/modern HSL/HSLA, modern HWB, modern CIE Lab/LCH with D50 conversion and Bradford adaptation, predefined sRGB/linear-sRGB/Display-P3/linear-Display-P3/A98-RGB/ProPhoto-RGB/Rec.2020/XYZ `color()` with Bradford adaptation, modern Oklab/OkLCh with D65 conversion and shared local-MINDE sRGB gamut mapping, and two-stop sRGB color mixing with omitted/normalized weights and premultiplied alpha, bounded named reset/increment/set counters and document-order generated quote depth, inherited horizontal LTR/RTL direction and bounded quote pairs, fixed/percentage/calculated plus direction-aware logical dimensions/insets/margins/padding/borders/radii, classic viewport/font-relative and bounded container-relative dimension units plus size functions, bounded vertical sticky positioning, dynamic hover/focus, inherited cursor/pointer interaction, text transformation, signed letter/word spacing, signed pixel/percentage first-line indentation, inherited text-wrap mode/style, non-inherited 1..32 line clamping, inherited bounded list marker type/image/position and shorthand plus isolated pseudo typography, bounded string/attribute-with-quoted-fallback/named-counter/quote-keyword/bitmap visual generated content, and optional bounded string/attribute/counter alternatives, text-align shorthand/all/last with logical state and justify-all, bounded text-decoration line/color/style/thickness and underline offset, and one inherited bounded text-shadow layer, content/border sizing, circular/elliptical radii, bounded outlines, box shadows, gradients, bitmap backgrounds, object fitting and aspect ratios, visibility/opacity, fonts, overflow, boxes, bounded ordered flex and two-axis grid properties |
| Layout and painting | 20% | 20% | Shared physical/logical box edges and per-edge border styles/colors plus fixed/percentage/calculated, viewport-unit, font-relative, container-relative, and min/max/clamp dimensions, content/border, aspect-ratio, sticky, bounded content-box measurement feedback for size containers, direction-aware mixed text/image before/after runs and inside/outside markers with separate pseudo style/content, first-line-indent/bounded balanced wrapping/line-clamping/all-line/final-line/ellipsis and TextPattern range geometry, transformed, spacing-aware, decorated, and bounded shadowed presentation text with raw form retention, native decoration fast path plus lightweight offset-aware custom line paint, normalized rounded backgrounds/borders/outlines/overflow/fields/box-shadows/gradients/bitmaps, pointer-gated retained hits with underlying fallthrough, fitted raster images and maps, ordered flex rows/columns, bounded grid tracks/spans, distribution, alignment, wrapping, grow/shrink, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Native field edit commands and one-level undo/redo, bounded multipart files, richer widgets, validation, responsive/fitted images with a zero-capability AppContainer decoder and combined admission, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, literal bounded `CSS.supports()` and live `matchMedia().matches` guards through compact `Su/Mq`, variables, text/title/body and ID-scoped display/width/height/direction/quotes/text-indent/text-wrap/mode/style/line-clamp/list-style/type/image/position/marker/before/after-style/text-align/all/last/text-decoration/text-underline-offset/text-shadow/logical box/border/radius/style/alert, class, visibility, disclosure, field-state, focus, and literal bounded DOM translation |
| Firespider integration and reliability | 10% | 10% | Lea-first New Tab/Ctrl+T, explicit legacy fallback, Inspector with Access view, MSAA/UIA surfaces including read-only text, host-safe focus shortcuts, navigation, sessions, source, host smokes |
| **Total** | **100%** | **100%** | Scoped Lea 1.0 baseline complete; post-1.0 hardening continues |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Completed post-1.0 hardening

Milestone 146 is in progress: bounded native-event Promise checkpoints and
`queueMicrotask` pass all 386 source and extracted-native-package checks.
The earlier separate candidate passes 43 host and six actual-EXE startup cases;
however, its extracted Firespider scripting case still fails a startup deadline. Do not
count this as a finished cold-start/release milestone or install over the live
browser. Workflow options now have 15 one/two-letter aliases, and drive-root
packaging is repaired. Five audited experimental archives and buildable host
source are retained. See `MICROTASKS.md` for evidence and remaining gates.

Startup follow-up adds bounded `Sd` phase/CPU diagnostics, overlaps the network
denial probe with trusted initialization, and moves asynchronous native launch
off the UI thread without changing budgets or isolation. A no-owner-pump Stop
regression is reproduced and fixed, and shared-file permission changes are
coordinated across concurrent launches and exact-SID cleanup. The final suite
passes 450 source checks, including 64 startup/permission cases; full core, all 43
v7 host cases and six actual-EXE startup scenarios pass. The small variant
passes clean/split/decode/hibernate/lazy-all checks, not its full matrix.
An earlier follow-up full run reproduced a startup timeout. The first extracted
standalone suite also failed recovery after cancellation without a typed result;
that cause remains unknown. The improved diagnostic assertion passes 84 focused
worker checks and all 450 checks in a newly packaged/extracted runtime. These
passes do not explain the earlier failure or prove stable cold-start readiness.
The v7 extracted runtime scripting case passes; its host-source ZIP independently
rebuilds and passes the scripting case after supplying manifest-verified harness
fixtures. A missing-fixture preflight and richer Boot failure diagnostics prevent
that setup error from masquerading as a worker/lifecycle failure.
Milestone 146 remains in progress and broader readiness stays about 65%.
See `STARTUP_DIAGNOSTICS.md` for observations and current verification scope.

The experimental timer follow-up adds bounded timeout/interval tasks through
compact `Td/Tn/Tk/TkA` aliases and automatic `W.On` native dispatch. A coercion
regression is reproduced and fixed. Full source passes all 616 checks, including
165 timer checks after two added viewport assertions; core also passes.
The new timer build's fresh-package gates are not verified, and the compiled v8
candidate fails its first scripting gate
at the existing hello/startup deadline; a traced diagnostic run fails too, before
DOM bootstrap completion. Earlier v7 host passes do not apply to v8. The failure
logs are retained, and the installed host remains untouched. The required bundle
grows by 1,536 bytes to 742,400 with
no interpreter dependency. Full classic-string-script scope/CSP, animation
frames, general event-loop/error semantics and remote-origin policy remain work.
Broader readiness stays about 65%. See `TIMERS.md` for scope and evidence.

Bootstrap follow-up replaces eager fixed-list Set/iterator machinery and fixes
a reproduced wrapper failure caused by a page-modified array iterator. All 136
focused event, 64 traced startup, 76 traced microtask, 165 timer and 58 native-view
checks pass. A six-launch comparison passes 26 checks and samples about 1.3 MiB
lower median native worker peak, not a demonstrated startup speed improvement.
Concurrent initialization is rejected after a diagnostic timeout. Initialization
remains sequential, with improved opt-in phases and bounded failure diagnostics.
The expected full count is 624, but normal full runs still fail fresh-worker
startup in microtask tests (latest: 5,052 ms wall / 453 ms CPU). No timeout or
isolation limit is relaxed. Release/cold-start readiness remains unproven and
broader readiness stays about 65%. Separate v9 compiles and passes its scripting
case and six actual-EXE startup scenarios, not the full 43-case host matrix.
No new release ZIP is published from the failed full source gate, and the
installed host remains unchanged. See `BOOTSTRAP.md` for retained evidence.

Production-path tracing follow-up removes the diagnostic-only prepared parser.
Both modes now evaluate the same bootstrap source, with compact phase markers
retained even without timing enabled. Missing-phase and parser-path regressions
are reproduced before the fixes. Microtask status assertions and worker warmups
now retain typed failure/startup diagnostics. All 70 focused startup checks and
the final 630-check normal full suite pass. Earlier runs in this round fail a
microtask status assertion, a fresh-worker startup deadline, and an in-process
computed-script budget; their logs remain, with no inferred common cause.
Separate v10 compiles and passes its scripting case and six actual-EXE startup
scenarios, not the complete 43-case host matrix or extracted-package/source
rebuild gates. The worker EXE is 512 bytes smaller than v9; that is a binary-size
measurement, not a speed claim. No deadline/isolation setting or required-core
dependency changes. No release ZIP is published, the installed host stays
unchanged, and broader readiness remains about 65%. See `STARTUP_TRACE.md`.

Live attribute follow-up adds writable optional `id`/`className`, presence-aware
`hasAttribute`, and literal 512-unit typed core aliases `id/cn`. Root/nonpainted
elements are supported; node/wrapper/field/editor identity and edited values
survive selector/style/layout/accessibility rebuilds. Shared DOM writes and
attribute-name conversion capture String and reject direct Symbols before a
host call. Empty attributes stay present. No generic attributes, URL writers,
new core dependency or protocol change is added. The complete core suite and all
703 scripting checks pass; all 703 also pass from a fresh standalone ZIP.
Separate v11 passes all 43 host and six actual-EXE startup cases. Three archives
pass inventory/length/hash audits; the extracted host and independently rebuilt
host-source ZIP pass their scripting cases with verified harness fixtures.
Native 940/420-pixel screenshots are inspected. Identified test setup failures
are corrected, while an earlier trusted-script budget failure remains unexplained.
The required bundle stays 742,400 bytes; the optional worker grows 512 bytes.
The installed host is unchanged. Cold-start reliability, broader DOM/site support,
remote-origin policy and Lea-only integration remain work; readiness stays about
65% and milestone 146 remains in progress. See `ATTRIBUTES.md`.

Class-token follow-up fixes reproduced form-feed selector/removal disagreement
and NBSP/vertical-tab splitting in optional script membership. Shared ASCII
word scanning removes temporary token sets for small core compounds; dense
compounds retain hashing without a per-node cache. Six controlled alternating
benchmark runs show about 90% lower common class-query allocation and 8.6% lower
class-write allocation on the 256-row fixture, not a browser-wide speed claim.
Focused core/83 scripting checks, the complete core rerun and all 713 scripting
checks pass; all 713 also pass from a fresh standalone ZIP. Separate v12 passes
all 43 host and six actual-EXE startup cases. Three archives pass inventory,
length/hash/current-snapshot/extraction checks; the extracted and independently
rebuilt host-source scripting cases pass with verified dependencies/fixtures.
The first core run fails delayed-decoder admission despite cleanup; isolated
picture checks and the full rerun pass, without an established cause or relaxed
budgets. The required bundle grows 512 bytes to 742,912 with no new dependency.
The live host remains unchanged; cold-start reliability and broader DOM/site
support are unfinished. Readiness stays about 65% and milestone 146 stays in
progress. See `CLASS_TOKENS.md` for measurements, artifacts and failure records.

Native-image follow-up reproduces and fixes a cancellation callback race that
returned a faulted task with `NullReferenceException`. Local operation references,
matching task tokens and acquired-only semaphore cleanup provide typed
cancellation and fresh-worker recovery. Read-only `Idg` retains a bounded
512-character phase report, with explicit timing boundaries and pre-start
retention. Final focused and complete core suites pass; all 713 source and fresh
standalone scripting checks pass. Separate v13 passes all 43 host and six EXE
startup cases. Three archives pass inventory/length/hash/snapshot/extraction
audits; extracted and independently rebuilt host-source scripting cases pass
with 19 verified dependencies and 348 fixture/license files. Full host/startup
matrices are not repeated on those extractions/rebuild. The required bundle
grows 1,024 bytes to 743,936 without a new dependency or relaxed budgets.
Failed handshake-observer setups are retained separately from the reproduced
production bug. Earlier intermittent startup failures remain unexplained and
the installed host is untouched. Readiness stays about 65%; milestone 146
remains in progress. See `IMAGE_STARTUP.md` for contracts, evidence and artifacts.

The initial live class-list round was unfinished, not a new release. The optional
interpreter adds ordered case-sensitive token operations, live indices and native array
iteration, setter forwarding, branded DOM exceptions, and a narrow native-error
formatter compatibility fix through existing `cn`, without changing protocol,
core aliases, deadlines or isolation. Reproduced empty-membership, replacement
validation-order, premature-forwarding and inherited-error-receiver failures
have focused regression coverage. All 47 in-process and five separate native-view
checks pass, as does the complete core suite, but the normal full scripting suite
fails cold initialization and a fresh isolated
worker exceeds the existing hello deadline. Compilation is not a release gate.
No new runtime ZIP or Firespider candidate is published from these failures;
v13 and the installed browser stay intact. Readiness remains about 65% and
milestone 146 remains in progress. See `CLASS_LISTS.md` for exact verification
scope, diagnostics and remaining work.

The private-block bootstrap follow-up removes the immediately invoked setup
function without exposing its helpers or changing budgets, protocol or native
protections. All 28 new scope checks, 99 normal class-list checks, 70 focused
startup checks, the full 840-check scripting suite and the complete core suite
pass. Six alternating fresh-path baseline/current launches pass all 27 checks,
including isolation and exact ACL retirement; their overlapping timings do not
demonstrate a speed gain. A separate v14 Firespider candidate compiles from 67
host inputs and passes all 43 host cases plus six actual-EXE startup scenarios.
The freshly extracted standalone passes all 840 script checks. The extracted
host runtime passes its scripting case, as does an independently rebuilt
Framework 4.8 host from the source ZIP, with all 19 dependencies and 348 fixtures
verified. Three separate archives have exact inventory and payload-hash audits;
these extraction/rebuild gates do not repeat the full 43-case/six-EXE matrices.
Earlier intermittent failures remain unexplained. Readiness stays
about 65% and milestone 146 stays open. See `BOOTSTRAP_SCOPES.md` for the measured
comparison, privacy boundaries and current artifact verification.

Element selector work adds compact read-only `N.Mt/Cl` aliases and standard
JavaScript `matches/closest` names, reusing the shared bounded compiler without
a second parser or query-collection truncation. All 55 focused script/native
checks, all 895 source scripting checks before the startup-result diagnostic
follow-up, and the complete core suite pass,
including inclusive nearest ancestors, receiver/conversion checks, detached
subtrees, existing mutation/permission gates, live fields and recovery.
Page-replaced WeakMap registration and inherited handle setters are reproduced
and fixed. An earlier full-run worker setup failure remains unexplained; the
improved assertion retains typed diagnostics on future failures. A separate v15
host run completes 27 cases then fails a Windows clipboard write; its scripting
case passes the new selector assertions but fails split-pane worker startup.
Startup results now snapshot bounded phase diagnostics before later lifecycle
attempts can replace `Sd`; five focused cancellation/recovery checks pass.
The full rerun after this diagnostic change fails isolated startup during
bootstrap evaluation (5,040 ms wall / 906 ms child CPU), so the planned 897-check
suite is not a completed pass, although it reaches and passes the new startup
and selector coverage before a later fresh-worker failure. A separate focused
rerun also fails first isolated startup; an explicitly observed diagnostic run
passes all 55 selector checks without identifying a cause. Separate v16 passes
its scripting case with opt-in phase timing and six actual-EXE startup scenarios,
not a new full host matrix or a release ZIP. Readiness observations reach
15,835 ms; this is not a speed improvement or an explanation of the failed
normal runs. The required bundle stays 743,936 bytes, and the optional worker
grows 1,024 bytes, without a speed claim. `:scope`, pseudo-elements, escapes and complete browser selector
semantics remain unsupported. Readiness stays about 65% and milestone 146 stays
open. See `ELEMENT_SELECTORS.md` for contracts, tests and verification scope.

- Added a reproducible separate .NET Framework 4.8 Firespider host/source
  candidate with optional isolated scripting in regular/resumed tabs and both
  split panes. Script-free pages remain dormant; explicit code and authored
  handlers initialize on demand, with template inertness and event/node
  validation retained. Live workers protect pure JavaScript state from automatic
  hibernation. Bounded status errors, native `about:blank`, and split comparison
  idle refresh are verified. The original installed browser/source are unchanged.
  All 305 script checks pass from source and the extracted standalone ZIP;
  full core, all 43 enabled-host Boot cases and all six actual-EXE startup
  scenarios pass. The small variant builds without interpreter dependencies
  and passes clean/split/hibernate/lazy-all cases; its full matrix is not
  repeated. All five runtime/host-source/standalone archives pass inventory,
  hash and length audits, and the extracted host source rebuilds independently.
  The enabled runtime ZIP is 3,678,689 bytes; the small ZIP is 1,101,676 bytes.
  Remote/external scripts, general event-loop and broader DOM/site compatibility,
  and removal of the explicit legacy fallback remain work. Broader readiness
  stays about 65%. See `FIRESPIDER_SCRIPT_HOST.md` and
  `../Lea.Firespider/CANDIDATE.md` for artifacts, build instructions and limits.

- Added optional native WinForms JavaScript integration, six bounded DOM event
  types, capture/bubble ordering, cancellation, inline/DOM0 handlers, and a
  separate compiled `Lea.Script.Browser.exe`. Compact view/provider aliases
  connect isolated scripts to layout, retained native fields, inline page
  initialization and image-aware load events. Bounded queues and page/permission
  generations prevent stale completions after navigation, Stop, detach or
  disposal. The adapter permits local file/about pages, excludes UNC files,
  and refuses HTTP/HTTPS scripts without starting an interpreter.
  All 294 script checks pass from source output and the extracted package;
  full core/native-edit regressions and all six staged Firespider EXE startup
  cases pass. The broader Boot harness passes 24 of 42 cases, then stops because
  the older staged host still creates legacy split-view controls; the remaining
  cases are not verified in this run. The installed browser remains unchanged.
  The 40-file ZIP is 2,946,831 bytes; 23 binary/config files total 7,920,283
  bytes, plus the icon and fixtures. Functional event/view tests use the
  documented five-second budget; the separate browser smoke passes with its
  two-second default. Cold-start deadlines and broader compatibility remain
  work, not performance claims. Broader readiness stays about 65%.
  See `NATIVE_JAVASCRIPT.md` for artifacts, verification and remaining limits.

- Added `Jp`, a process-isolated optional JavaScript session with three-character
  `ExA` asynchronous execution and `St` cancellation. A unique temporary
  zero-capability AppContainer, exact-file read/execute ACLs and runtime leases,
  suspended token/ASLR verification, one-process 128 MiB memory Job, 25% CPU
  rate, mitigation checks, no-loopback-exemption query and positive-control
  network/file probes precede execution. Versioned bounded JSON pipes carry
  only primitive capabilities; the parent retains DOM ownership, permission
  generation, active-page checks and independent request/value limits.
  All 117 checks pass, covering malformed frames/IDs/handles, computed DOM,
  realms, detached nodes, typed limits, cancellation, hard native-regex deadline,
  oversized native allocation, fresh-process recovery, ACL cleanup, and UI
  timer responsiveness. Locked restore, solution build, full core and focused
  image-worker regressions and all six separate Firespider EXE startup cases
  pass. The 33-file test ZIP is 2,686,336 bytes; its 18 runtime files total
  7,122,034 bytes. FAT32 runtime launch fails closed; the extracted NTFS package
  passes the full suite. The shared launcher still has no interpreter
  dependency. This is not yet native browser-host event/layout integration or
  automatic remote scripting; the installed browser is unchanged. Broader
  readiness stays about 65%. See `../Lea.Script/README.md`; the reproducible
  separate test package is produced by `../Lea.Build/Pack-LeaScript.ps1`.
  See `ISOLATED_JAVASCRIPT.md` for verified package paths, hashes and limitations.

- Added the optional `Lea.Script` trusted-script experiment and its Visual
  Studio project, using pinned Jint 4.16.4 rather than a handwritten JavaScript
  interpreter. Computed expressions, functions, loops, arrays, JSON, and
  template strings can update engine-owned DOM records through typed `E.Jv`
  mutations. Those values are never parsed as Lea code or interpolated;
  connected-node ownership, live retained fields, subtree detachment, host
  permission generations through `E.Jg`, thread affinity, bounded execution,
  and fresh realms after failure are tested. The 40-check executable, full
  core suite, native editing, locked restore, and solution build pass. The
  complete optional test runtime is about 3.57 MiB with a 1.35 MiB per-file
  GZip estimate. The core has no Jint/Acornima assembly reference. The test
  package is `F:\Lea-js-142-experiment_2026-10-03_05-33-17.zip`; the installed
  browser is unchanged. This is not automatic remote-page execution, a
  complete DOM, or an OS sandbox. Process isolation and native browser-host
  event/layout integration remain the next scripting work. Broader readiness
  stays about 65%; this experiment does not prove modern-site compatibility.
  See `../Lea.Script/README.md`.

- Added bounded two-term `calc()` thresholds to size-container queries. The
  compact `Kx` tree holds two fixed-hundredth length/unit pairs; integer
  evaluation combines pixel, font, viewport, and ancestor-container bases
  without runtime parsing or floating point. Legacy, direct, reversed, exact,
  and chained forms share the path. A dedicated fixture covers decimal
  boundaries, signed results at zero width, invalid unitless arithmetic,
  malformed syntax, live viewport/font/container changes, retained fields,
  diagnostics, and WinForms geometry. The focused, neighboring, full
  core/edit, 137/137 benchmark probes over 132 fixtures, compiled Firespider
  host, all 42 Boot, and all six real EXE startup checks pass. Three-round
  diagnostic medians are 805.772 ms clean start, 9.116 ms local load, and
  39.128 ms layout/paint. Retained memory is 38,521.5 KiB for 528 pages, or
  73.0 KiB/page. Required runtime files are 700.5 KiB with a 286.9 KiB GZip
  estimate. Runtime 241 is packaged separately as
  `F:\Lea-runtime-241-ready_2026-10-03_00-49-07.zip`; installed runtime 157
  remains unchanged. This is a bounded CSS arithmetic subset, not general
  `calc()` or full-web compatibility. See `CALCULATED_CONTAINER_QUERIES.md`.
  The boot edit harness now retries empty clipboard reads briefly and checks
  each toolbar command separately; ten consecutive edit runs and the full
  Boot matrix pass after reproducing transient read contention.

- Added bounded classic viewport and container-relative size-query thresholds
  to the compact `Kx` tree: `vw`/`vh`/`vmin`/`vmax` and
  `cqw`/`cqh`/`cqi`/`cqb`/`cqmin`/`cqmax`. Integer cross-products preserve
  two-decimal boundaries without runtime floating point. Each `cq*` basis
  resolves from the selected query container's nearest eligible ancestor per
  axis, with viewport fallback. `E.Vz` now reevaluates active container rules
  even when the media rule list is unchanged, while retained fields and
  measurements survive. A dedicated fixture covers all units, mixed chains,
  malformed limits, independent engine viewports, dynamic ancestor sizes,
  exact native geometry, and the compiled Firespider host. Focused,
  neighboring, full source/staged core and edit, matching compiled host, all
  42 Boot, and all six EXE startup suites pass. All 136 compatibility probes
  pass over 131 fixtures. Three-round diagnostic medians are 993.128 ms clean
  start, 8.628 ms local load, and 42.283 ms layout/paint. Retained managed
  memory is 38,303.4 KiB for 524 pages, or 73.1 KiB/page. Required runtime
  files are 697.0 KiB with a 286.6 KiB GZip estimate. Runtime 240 is packaged
  separately as `F:\Lea-runtime-240-ready_2026-10-02_23-45-12.zip`; installed
  runtime 157 remains unchanged. Dynamic viewport unit families, vertical
  writing, scroll-state queries, and complete intrinsic containment remain
  future work. See `CONTAINER_QUERY_VIEWPORT.md`.

- Added bounded font-relative size-container query thresholds to the existing
  compact `Kx` tree. Pixel, selected-container `em`, and root `rem` values use
  fixed integer hundredths and one-character `p/e/r` codes, accept up to two
  fractional digits, compose across legacy/direct/value-first/chained forms,
  and allow unitless zero without runtime decimal or floating-point parsing.
  Dynamic container and root fonts reevaluate independently while `E.Cz`
  measurements and mutable fields remain retained. A dedicated fixture covers
  mixed-unit chains, exact boundaries, malformed limits, axis eligibility,
  diagnostics, native sheets, and exact WinForms geometry. Focused,
  neighboring, benchmark, full source/staged core and edit, matching compiled
  Firespider host, all 42 Boot, and all six real EXE startup suites pass.
  All 135 compatibility probes pass over 130 fixtures. Three-round diagnostic
  medians are 860.342 ms clean start, 11.178 ms local load, and 55.986 ms
  layout/paint. Retained managed memory is 38,042.6 KiB for 520 pages, or 73.2
  KiB/page. Required runtime files are 695.0 KiB with a 285.9 KiB GZip
  estimate. Runtime 239 is packaged separately as
  `F:\Lea-runtime-239-ready_2026-10-02_19-22-18.zip`; installed runtime 157
  remains unchanged. Viewport/container-relative query thresholds, vertical writing,
  scroll-state queries, and complete intrinsic containment remain future work.
  See `CONTAINER_QUERY_LENGTHS.md`.

- Added bounded `cqw`, `cqh`, `cqi`, `cqb`, `cqmin`, and `cqmax` dimensions to
  the compact width/height tuple and size-function path. Unit codes 7..12 use
  nearest eligible ancestors per axis, allow different inline/block containers,
  and fall back to the client viewport for a missing axis. Custom properties,
  `calc()` offsets, `min()`/`max()`/`clamp()`, min/max constraints, native
  sheets, compact/readable/DOM commands, and computed-value inheritance share
  the existing parser and bounded three-pass `E.Cz` feedback. Lazy origin
  arrays preserve the parent's basis only for explicit inherited dimensions.
  A dedicated fixture covers all unit codes, mixed ancestor selection,
  fallback, malformed values, dynamic wide/narrow containers, retained fields,
  and exact WinForms geometry. Focused, neighboring, full-core, native-edit,
  and matching compiled Firespider host suites pass, together with all 42 boot
  cases and all six real EXE startup scenarios. All 134 compatibility probes
  pass over 129 fixtures. Three-round diagnostic medians are 668.801 ms clean
  start, 7.022 ms local load, and 30.869 ms layout/paint. Retained managed
  memory is 37,813.5 KiB for 516 pages, or 73.3 KiB/page. Required runtime
  files are 713.0 KiB with a 293.1 KiB GZip estimate. Runtime 238 is packaged
  separately as `F:\Lea-runtime-238-ready_2026-10-02_18-15-09.zip`; installed
  runtime 157 remains unchanged. Container units outside dimension properties,
  relative query thresholds, vertical writing, and complete intrinsic
  containment remain outside this slice. See `CONTAINER_UNITS.md`.

- Added bounded `aspect-ratio` and `orientation` features to the existing CSS
  size-container path. Boolean/exact/min/max, strict/inclusive feature-first,
  value-first, and ascending/descending chained ranges use reduced 1..8,192
  integer ratios; portrait includes square boxes and landscape requires a
  wider box. Both features require `container-type:size`, reuse atomic `E.Cz`
  content-box measurements, and compose with named/nearest selection,
  `style()`, grouped logic, nested media/supports/layers, native sheets,
  diagnostics, pseudo/SVG rules, and retained rebuilds. A dedicated fixture
  covers missing and zero measurements, reduced equivalence, both chain
  directions, square orientation, malformed values, inline-size rejection,
  dynamic landscape/portrait changes, retained fields, and exact automatic
  WinForms geometry. Focused and full source suites, native clipboard/edit
  checks, the matching compiled Firespider host, all 42 Boot cases, and all six
  real EXE startup scenarios pass without MSHTML, an invalid cast, JIT dialog,
  or dead startup window. All 133 compatibility probes pass over 128 fixtures.
  Three-round diagnostic medians are 774.972 ms clean start, 8.095 ms local
  load, and 41.400 ms layout/paint. Retained managed memory is 37,367.8 KiB
  for 512 pages, or 73.0 KiB/page. Required runtime files are 693.0 KiB with a
  285.0 KiB GZip estimate. Runtime 237 is packaged separately and installed
  runtime 157 remains unchanged. Decimal/single-number ratios, container units,
  relative query lengths, scroll-state queries, vertical writing, and complete
  intrinsic containment remain outside this slice. See
  `CONTAINER_RATIO_ORIENTATION.md`.

- Added bounded CSS size container queries to the shared compiled stylesheet
  and retained layout paths. Compact `S.Ct=n/i/s` state supports normal,
  inline-size, and size containers; width, height, inline-size, and block-size
  features accept exact/min/max, feature-first, value-first, chained, boolean,
  and grouped `not`/`and`/`or` conditions. Named and nearest eligible ancestors,
  combined `style()` and size conditions, nesting through media/supports/layers,
  native sheets, pseudo-elements, SVG, and self-query rejection share one
  bounded matcher. Public `E.Cz` installs an atomic 256-entry content-box
  snapshot, while WinForms feeds actual retained box measurements through at
  most three stable layout passes without replacing live fields. A dedicated
  fixture and focused suite cover parsing, eligibility, dynamic resize,
  diagnostics, exact geometry, retained values, and host feedback. Focused and
  full source suites, native clipboard/edit checks, the matching compiled
  Firespider host, all 42 Boot cases, and all six real EXE startup scenarios
  pass without MSHTML, an invalid cast, JIT dialog, or dead startup window. All
  132 compatibility probes pass over 127 fixtures. Three-round diagnostic
  medians are 600.170 ms clean start, 7.623 ms local load, and 39.279 ms
  layout/paint. Retained managed memory is 37,073.1 KiB for 508 pages, or 73.0
  KiB/page. Required runtime files are 690.5 KiB with a 284.6 KiB GZip
  estimate. Runtime 236 is packaged separately and installed runtime 157
  remains unchanged. Scroll-state queries, container units, general-property
  queries, and full intrinsic size containment remain outside this slice. See
  `CONTAINER_SIZE_QUERIES.md`.

- Added bounded CSS container style queries to the shared compiled stylesheet
  path. Exact custom-property `style(--name:value)` features now select the
  nearest ancestor or the nearest ancestor named through computed `S.Cn`;
  grouped `not`/`and`/`or`, different nested ancestors, bounded `var()`
  resolution, and nesting through media, supports, and layers are active.
  Class/name changes and native-sheet installation reevaluate through the
  retained rebuild path without replacing live fields. Size, scroll-state,
  general-property, relative-value, and container-unit queries remain
  unsupported. A dedicated fixture and focused suite cover computed name lists,
  self-query suppression, malformed/unsupported rejection, diagnostics,
  dynamic values and names, retained fields, and exact WinForms geometry.
  Focused and full source suites, native clipboard/edit checks, the matching
  compiled Firespider host, all 42 Boot cases, and all six real EXE startup
  scenarios pass without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 131 compatibility probes pass over 126 fixtures. Three-round
  diagnostic medians are 615.196 ms clean start, 8.222 ms local load, and
  36.282 ms layout/paint. Retained managed memory is 36,358.5 KiB for 504
  pages, or 72.1 KiB/page. Required runtime files are 684.0 KiB with a 282.3
  KiB GZip estimate. Runtime 235 is packaged separately and installed runtime
  157 remains unchanged. See `CONTAINER_STYLE_QUERIES.md`.

- Added bounded Media Queries Level 4 interaction features to the shared
  compiled media path. Primary `pointer`/`hover` and union
  `any-pointer`/`any-hover` queries now work through `@media`, `<style media>`,
  compact `E/V.Mq`, and literal global/window `matchMedia(...).matches`
  guards. Short `Pt/Hv/Ap/Ah/Mi` aliases cover deterministic desktop,
  touch-first hybrid, and no-pointer states; `Ap=b` provides exact
  coarse-plus-fine union matching. WinForms conservatively samples Windows
  mouse, touch, and pen capability until a host override, including device
  changes. A dedicated fixture and focused suite cover host calls, boolean and
  exact CSS, nested hybrid intersections, style media, scripts, malformed
  queries, diagnostics, exact WinForms geometry, and retained field identity,
  value, and command styles. Focused and full source suites, native
  clipboard/edit checks, the matching compiled Firespider host, and the exact
  extracted NTFS package core/edit suites pass. All 42 Boot cases and all six
  real EXE startup scenarios remain responsive without MSHTML, an invalid
  cast, JIT dialog, or dead startup window. All 130 compatibility probes pass
  over 125 fixtures. Three-round diagnostic medians are 567.113 ms clean
  start, 7.621 ms local load, and 38.984 ms layout/paint. Retained managed
  memory is 33,942.3 KiB for 500 pages, or 67.9 KiB/page. Required runtime
  files are 698.5 KiB with a 288.3 KiB GZip estimate. Runtime 234 is packaged
  separately and installed runtime 157 remains unchanged. See
  `MEDIA_INTERACTION.md`.

- Added bounded Media Queries Level 4 modifiers to the one compiled media
  path. Whole-query `not`, no-op `only` before a recognized media type, and
  fixed screen/all/print semantics now work through `@media`, `<style media>`,
  compact `E/V.Mq`, and literal global/window `matchMedia(...).matches`
  guards. Negated children intersect with parent media as exact compact
  predicates, including comma alternatives and live resize; unknown types,
  misplaced/repeated modifiers, and malformed media stay inactive. A dedicated
  fixture and focused suite cover host calls, CSS, style media, scripts,
  diagnostics, exact WinForms geometry, and retained field identity/value.
  Focused and full source suites, native clipboard/edit checks, the matching
  compiled Firespider host, and the extracted NTFS package core/edit suites
  pass. All 42 Boot cases and all six real EXE startup scenarios remain
  responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 129 compatibility probes pass over 124 fixtures. Three-round diagnostic medians
  are 735.376 ms clean start, 7.882 ms local load, and 39.059 ms layout/paint.
  Retained managed memory is 33,755.8 KiB for 496 pages, or 68.1 KiB/page.
  Required runtime files are 692.5 KiB with a 286.3 KiB GZip estimate. Runtime
  233 is packaged separately and installed runtime 157 remains unchanged. The
  removable F: stage exceeded one stalled-decoder timing assertion by 13 ms;
  the exact extracted NTFS package passed that full suite. See
  `MEDIA_MODIFIERS.md`.

- Added standards-shaped bounded media-query scripting without adding a
  general JavaScript VM. Compact `E/V.Mq(query)` evaluates the compiled media
  subset against live viewport, density, preference, and scripting state.
  Literal global or `window.matchMedia("...").matches` guards accept one quoted
  query, optional `!`, and optional `else`. They mix with `CSS.supports()` to
  four total levels and share the existing 32-command budget and retained
  rebuild path. Computed/extra arguments, wrong-case names, comparisons,
  listeners, saved query objects, and arbitrary expressions stay inert. A
  dedicated fixture and focused suite cover startup scripts, host calls,
  clicks, resize, preferences, exact WinForms geometry, field identity, and
  command accounting. Focused, full source, and compiled Firespider host suites
  pass, as do the separate clipboard/edit checks. All 42 Boot cases pass in one
  run and all six real EXE startup scenarios remain responsive without MSHTML,
  an invalid cast, JIT dialog, or dead startup window. All 128 benchmark probes
  pass over 123 fixtures. Three-round diagnostic medians are 549.435 ms clean
  start, 7.063 ms local load, and 39.150 ms layout/paint. Retained managed
  memory is 33,579.8 KiB for 492 pages, or 68.3 KiB/page. Required runtime
  files are 691.0 KiB with a 285.5 KiB GZip estimate. Runtime 232 is packaged
  separately and installed runtime 157 remains unchanged. See `MATCH_MEDIA.md`.

- Added standards-shaped bounded `CSS.supports()` compatibility without adding
  a general JavaScript VM. Compact `E/V.Su` overloads accept condition text or
  one property/value pair and reuse the compiled `@supports` declaration,
  selector, and logical-condition parsers. Literal page scripts may guard the
  existing allow-listed commands with one/two-argument `CSS.supports(...)`, an
  optional leading `!`, and one optional `else` block. Four nested guard levels
  share the existing 32-command budget and retained field rebuild path.
  Computed/extra arguments, malformed calls, wrong-case APIs, arbitrary
  expressions, and over-depth bodies stay inert. A dedicated fixture and
  focused suite cover startup scripts, host-provided commands, click handlers,
  exact WinForms geometry, field identity, and command accounting. Focused,
  full source, and exact NTFS package-mirror suites plus the compiled
  Firespider host pass. All 42 Boot cases are accounted for; the one
  clipboard-sensitive combined `edit` failure passed in isolation before the
  remaining 14 cases passed. All six real EXE startup scenarios remained
  responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. The direct FAT32-stage feature check and zero-capability image decode
  also pass. All 127 benchmark probes pass over 122 fixtures. Three-round
  diagnostic medians are 955.931 ms clean start, 8.349 ms local load, and
  40.097 ms layout/paint. Retained managed memory is 33,491.4 KiB for 488
  pages, or 68.6 KiB/page. Required runtime files are 690.0 KiB with a 285.2
  KiB GZip estimate. Runtime 231 is packaged separately and installed runtime
  157 remains unchanged. See `SUPPORTS_QUERIES.md`.

- Added bounded CSS `prefers-reduced-data` media queries with compact
  `E/P/G/V.Pdt` aliases. `n` selects no-preference and `r` selects reduce;
  headless and WinForms hosts default deterministically to no-preference and
  change the state explicitly rather than inferring a connection policy.
  Boolean queries are active only for reduce. Exact values, width
  intersections, alternatives, nested media, and style media share the
  compiled path; conflicting and unsupported conditions stay inactive. Live
  changes retain mutable fields and expose compact `@pdt=n/r` diagnostics.
  This selector does not add `Save-Data`, cancel resources, or automatically
  choose smaller assets. Runtime 230 also detects non-ACL filesystems such as
  FAT32 and SHA-256 stages only the bounded image helper in a verified
  content-addressed local cache, preserving the zero-capability AppContainer
  instead of falling back to an unrestricted decoder. Focused and full
  source/staged suites plus the final
  compiled Firespider host pass. All 42 Boot cases and all six real EXE startup
  scenarios remain responsive without MSHTML, an invalid cast, JIT dialog, or
  dead startup window. All 126 benchmark probes pass over 121 fixtures.
  Three-round diagnostic medians are 591.221 ms clean start, 6.799 ms local
  load, and 36.196 ms layout/paint. Retained managed memory is 33,405.4 KiB for
  484 pages, or 69.0 KiB/page. Required runtime files are 686.5 KiB with a
  283.3 KiB GZip estimate. Runtime 230 is packaged separately and installed
  runtime 157 remains unchanged. See `MEDIA_REDUCED_DATA.md`.

- Added bounded CSS `scripting` media queries through the existing compact
  `E/P/G/V.Je` permission aliases. Lea defaults to enabled; false selects none,
  while standards-defined initial-only syntax is recognized but never selected
  because Lea has no one-shot script mode. Boolean queries are inactive for
  none and active for enabled. Exact values, width intersections, alternatives,
  nested media, and style media share the compiled path; conflicting and
  unsupported conditions stay inactive. Live changes retain mutable fields,
  expose compact `@je` and `@je=n/e` diagnostics, and do not rerun skipped
  inline commands. Focused and full source/staged suites plus the final compiled
  Firespider host pass. All 42 Boot cases and all six real EXE startup scenarios
  remain responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 125 benchmark probes pass over 120 fixtures. Three-round
  diagnostic medians are 583.675 ms clean start, 7.046 ms local load, and
  37.360 ms layout/paint. Retained managed memory is 33,248.2 KiB for 480
  pages, or 69.3 KiB/page. Required runtime files are 666.5 KiB with a 275.7
  KiB GZip estimate. Runtime 229 is packaged separately and installed runtime
  157 remains unchanged. See `MEDIA_SCRIPTING.md`.

- Added bounded CSS `prefers-contrast` media queries with compact `E/P/G/V.Pc`
  contrast aliases. Headless Lea defaults deterministically to no-preference;
  `n`, `l`, `m`, and `c` select no-preference, less, more, and custom, while
  WinForms views map Windows High Contrast to custom until a host sets an
  explicit value. Boolean queries are inactive for no-preference and active for
  less, more, or custom. Exact values, width intersections, alternatives,
  nested media, and style media share the compiled path; conflicting and
  unsupported conditions stay inactive. Responsive rebuilds retain mutable
  fields and expose compact `@pc` and `@pc=n/l/m/c` diagnostics. Focused and
  full source/staged suites plus the final compiled Firespider host pass. All
  42 Boot cases and all six real EXE startup scenarios remain responsive
  without MSHTML, an invalid cast, JIT dialog, or dead startup window. All 124
  benchmark probes pass over 119 fixtures. Three-round diagnostic medians are
  692.788 ms clean start, 7.989 ms local load, and 39.432 ms layout/paint.
  Retained managed memory is 33,061.4 KiB for 476 pages, or 69.5 KiB/page.
  Required runtime files are 664.5 KiB with a 275.3 KiB GZip estimate. Runtime
  228 is packaged separately and installed runtime 157 remains unchanged. See
  `MEDIA_CONTRAST.md`.

- Added bounded CSS `prefers-reduced-motion` media queries with compact
  `E/P/G/V.Rm` motion aliases. Headless Lea defaults deterministically to
  no-preference; `n` and `r` select no-preference and reduce, while WinForms
  views sample the Windows client-area animation setting until a host sets an
  explicit value. Boolean queries are active only for reduce. Exact values,
  width intersections, alternatives, nested media, and style media share the
  compiled path; conflicting and unsupported conditions stay inactive.
  Responsive rebuilds retain mutable fields and expose compact `@rm=n/r`
  diagnostics. Focused and full source/staged suites plus the final compiled
  Firespider host pass. All 42 Boot cases and all six real EXE startup scenarios
  remain responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 123 benchmark probes pass over 118 fixtures. Three-round
  diagnostic medians are 708.700 ms clean start, 8.274 ms local load, and
  42.686 ms layout/paint. Retained managed memory is 32,866.6 KiB for 472
  pages, or 69.6 KiB/page. Required runtime files are 662.5 KiB with a 274.6
  KiB GZip estimate. Runtime 227 is packaged separately and installed runtime
  157 remains unchanged. See `MEDIA_REDUCED_MOTION.md`.

- Added bounded CSS `prefers-color-scheme` media queries with compact
  `E/P/G/V.Th` theme aliases. Headless Lea defaults deterministically to light;
  `l` and `d` select light and dark, while WinForms views sample the Windows
  application theme until a host sets an explicit value. Boolean, exact
  light/dark, width intersections, alternatives, nested media, and style media
  share the compiled media path. Conflicting nested themes and unsupported
  values stay inactive. Responsive rebuilds retain mutable fields and expose
  compact `@t=l/d` diagnostics. Focused and full source/staged suites plus the
  final compiled Firespider host pass. All 42 Boot cases and all six real EXE
  startup scenarios remain responsive without MSHTML, an invalid cast, JIT
  dialog, or dead startup window. All 122 benchmark probes pass over 117
  fixtures. Three-round diagnostic medians are 908.565 ms clean start, 8.498
  ms local load, and 44.488 ms layout/paint. Retained managed memory is
  32,707.1 KiB for 468 pages, or 69.9 KiB/page. Required runtime files are
  660.5 KiB with a 273.9 KiB GZip estimate. Runtime 226 is packaged separately
  and installed runtime 157 remains unchanged. See `MEDIA_COLOR_SCHEME.md`.

- Added bounded CSS resolution media queries with compact `E/P/G/V.Dp`
  density aliases. Boolean, legacy exact/min/max, feature-first, value-first,
  and same-direction chained range forms accept finite `dpi`, `dppx`, and
  `dpcm` values using `1dppx = 96dpi` and milli-dpi normalization. Resolution
  intersects with width, height, orientation, aspect ratio, alternatives,
  nested media, and style media. WinForms views sample live monitor DPI and
  handle DPI changes until a host sets an explicit value; responsive rebuilds
  retain mutable fields and expose compact `@d` diagnostics. Focused and full
  source/staged suites plus the final compiled Firespider host pass. All 42 Boot
  cases and all six real EXE startup scenarios remain responsive without
  MSHTML, an invalid cast, JIT dialog, or dead startup window. All 121 benchmark
  probes pass over 116 fixtures. Three-round diagnostic medians are 567.127 ms
  clean start, 6.994 ms local load, and 45.296 ms layout/paint. Retained managed
  memory is 32,544.4 KiB for 464 pages, or 70.1 KiB/page. Required runtime files
  are 658.5 KiB with a 273.2 KiB GZip estimate. Runtime 225 is packaged
  separately and installed runtime 157 remains unchanged. See
  `MEDIA_RESOLUTION.md`.

- Completed the standard predefined CSS Color 4 RGB spaces with bounded
  `color(a98-rgb ...)`, `color(prophoto-rgb ...)`, and
  `color(rec2020 ...)` parsing and WinForms paint. Lea applies each standard
  transfer curve and matrix, adapts ProPhoto RGB from D50 to D65 with the
  shared Bradford path, and reuses its local-MINDE Oklab search for sRGB
  display gamut mapping. Numeric/percentage channels, extended finite
  channels, optional alpha, variables, layers, media, `@supports`, generated
  content, shadows, gradients, fields, SVG, color mixing, responsive rebuilds,
  and existing compact records all share the feature without a new public API.
  Focused and full source/staged suites plus the final compiled Firespider host
  pass. All 42 Boot cases and all six real EXE startup scenarios remain
  responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 120 benchmark probes pass over 115 fixtures. Three-round
  diagnostic medians are 633.262 ms clean start, 6.620 ms local load, and
  40.723 ms layout/paint. Retained managed memory is 32,229.6 KiB for 460
  pages, or 70.1 KiB/page. Required runtime files are 653.5 KiB with a 271.3
  KiB GZip estimate. Runtime 224 is packaged separately and installed runtime
  157 remains unchanged. See `WIDE_GAMUT_COLORS.md`.

- Added bounded predefined CSS `color()` to the shared color validator and
  WinForms painter. Modern space/slash syntax accepts finite numeric or
  percentage channels and optional alpha for `srgb`, `srgb-linear`,
  `display-p3`, `display-p3-linear`, `xyz`, `xyz-d50`, and `xyz-d65`.
  Lea uses the CSS Color 4 transfer curves and D65 matrices, shares Bradford
  D50-to-D65 adaptation with Lab, and reuses the bounded local-MINDE Oklab
  search for sRGB gamut mapping. Existing compact records carry the colors
  through text, backgrounds, physical/logical borders, outlines, decorations,
  text/box shadows, gradients, generated content, variables, layers, media,
  fields, two-stop sRGB mixes, SVG fill/stroke/stops, and declaration
  `@supports` without a new public API. Comma syntax, duplicate slashes,
  `none`, nested/channel expressions, custom profiles, A98 RGB, ProPhoto RGB,
  Rec. 2020, and overlong input remain outside this bounded slice. Focused and
  full source and staged suites plus the final compiled Firespider host pass.
  All 42 Boot cases and all six real EXE startup scenarios remain responsive
  without MSHTML, an invalid cast, JIT dialog, or dead startup window. All 119
  benchmark probes pass over 114 fixtures. Three-round diagnostic medians are
  611.914 ms clean start, 6.768 ms local load, and 45.787 ms layout/paint.
  Retained managed memory is 31,798.3 KiB for 456 pages, or 69.7 KiB/page.
  Required runtime files are 652.5 KiB with a 270.8 KiB GZip estimate. Runtime
  223 is packaged separately and installed runtime 157 remains unchanged. See
  `PREDEFINED_COLORS.md`.

- Added bounded modern CSS CIE `lab()` and `lch()` to the shared color validator
  and WinForms painter. Space-separated numeric or percentage channels accept
  optional slash alpha; LCH hue accepts unitless degrees plus `deg`, `grad`,
  `rad`, and `turn`. Lea converts D50 Lab to XYZ, applies the CSS Bradford
  D50-to-D65 adaptation, and reuses its bounded local-MINDE Oklab search for
  out-of-gamut sRGB values. Existing compact records carry the colors through
  text, backgrounds, physical/logical borders, outlines, decorations,
  text/box shadows, gradients, generated content, variables, layers, media,
  fields, two-stop sRGB mixes, SVG fill/stroke/stops, and declaration
  `@supports` without a new public API. Comma syntax, duplicate slashes, `none`,
  nested/channel expressions, relative colors, wide-gamut profiles, and
  overlong input remain outside this bounded slice. Focused and full source and
  staged suites plus the final compiled Firespider host pass. All 42 Boot cases
  and all six real EXE startup scenarios remain responsive without MSHTML, an
  invalid cast, JIT dialog, or dead startup window. All 118 benchmark probes
  pass over 113 fixtures. Three-round diagnostic medians are 799.612 ms clean
  start, 8.569 ms local load, and 47.724 ms layout/paint. Retained managed
  memory is 31,349.9 KiB for 452 pages, or 69.4 KiB/page. Required runtime files
  are 651.0 KiB with a 270.2 KiB GZip estimate. Runtime 222 is packaged
  separately and installed runtime 157 remains unchanged. See `LAB_COLORS.md`.

- Added bounded modern CSS `oklab()` and `oklch()` to the shared color validator
  and WinForms painter. Space-separated numeric or percentage channels accept
  optional slash alpha; OkLCh hue accepts unitless degrees plus `deg`, `grad`,
  `rad`, and `turn`. D65 Oklab/XYZ/sRGB conversion and a bounded CSS-style
  local-MINDE search preserve lightness and hue while reducing out-of-gamut
  chroma with a 0.02 JND and 0.0001 epsilon. Existing compact records carry the
  colors through text, backgrounds, physical/logical borders, outlines,
  decorations, text/box shadows, gradients, generated content, variables,
  layers, media, fields, two-stop sRGB mixes, SVG fill/stroke/stops, and
  declaration `@supports` without a new public API. Comma syntax, duplicate
  slashes, `none`, nested/channel expressions, relative colors, `lab()`/`lch()`,
  wide-gamut profiles, and overlong input remain outside this bounded slice.
  Focused and full source and staged suites plus the final compiled Firespider
  host pass. All 42 Boot cases and all six real EXE startup scenarios remain
  responsive without MSHTML, an invalid cast, JIT dialog, or dead startup
  window. All 117 benchmark probes pass over 112 fixtures. Three-round
  diagnostic medians are 721.573 ms clean start, 6.595 ms local load, and
  48.196 ms layout/paint. Retained managed memory is 30,953.3 KiB for 448 pages,
  or 69.1 KiB/page. Required runtime files are 649.0 KiB with a 269.8 KiB GZip
  estimate. Runtime 221 is packaged separately and installed runtime 157
  remains unchanged. See `OKLAB_COLORS.md`.

- Added bounded two-stop CSS `color-mix(in srgb, ...)` to the shared color
  validator and WinForms painter. Named, hex, RGB/RGBA, HSL/HSLA, HWB,
  `transparent`, and `currentColor` stops accept omitted or 0..100% weights;
  omitted weights receive their CSS complement, explicit weights normalize,
  low totals reduce output alpha, and rendering uses premultiplied-alpha sRGB
  interpolation. Existing compact records carry mixes through text,
  backgrounds, physical/logical borders, outlines, decorations, text/box
  shadows, gradients, generated content, variables, layers, media, fields, SVG
  fill/stroke/stops, and declaration `@supports` without a new public API.
  Other color spaces, hue interpolation clauses, nested mixes, extra stops,
  invalid percentages, zero totals, and overlong input are rejected atomically.
  The focused and full engine suites and final compiled Firespider host pass.
  All 42 Boot cases and all six real EXE startup scenarios remain responsive
  without a legacy engine, invalid cast, JIT dialog, or dead startup window.
  All 116 benchmark probes pass over 111 fixtures. Three-round diagnostic
  medians are 730.060 ms clean start, 6.673 ms local load, and 41.446 ms
  layout/paint. Retained managed memory is 30,540.5 KiB for 444 pages, or 68.8
  KiB/page. Required runtime files are 645.5 KiB with a 268.2 KiB GZip
  estimate. Runtime 220 is packaged separately and installed runtime 157
  remains unchanged. See `COLOR_MIX.md`.

- Added bounded modern CSS `hwb()` colors to the shared color validator and
  WinForms painter. Space-separated hue, whiteness, and blackness accept an
  optional slash alpha; unitless degrees plus `deg`, `grad`, `rad`, and `turn`
  hues wrap to one turn, channels clamp to their paint ranges, and whiteness
  plus blackness at or above 100% uses CSS gray normalization. Existing compact
  records carry HWB through text, backgrounds, physical/logical borders,
  outlines, decorations, text/box shadows, gradients, generated content,
  variables, layers, media, fields, SVG fill/stroke/stops, and `@supports`
  without a new public API. Comma syntax, duplicate slashes, missing
  percentages, nested functions, `none` channels, relative colors, and channel
  expressions remain outside this bounded slice. The focused and full engine
  suites and final compiled Firespider host pass. All 42 Boot cases and all six
  real EXE startup scenarios remain responsive without a legacy engine, invalid
  cast, JIT dialog, or dead startup window. All 115 benchmark probes pass over
  110 fixtures. Three-round diagnostic medians are 696.028 ms clean start,
  5.715 ms local load, and 41.915 ms layout/paint. Retained managed memory is
  30,174.4 KiB for 440 pages, or 68.6 KiB/page. Required runtime files are 643.0
  KiB with a 267.2 KiB GZip estimate. Runtime 219 is packaged separately and
  installed runtime 157 remains unchanged. See `HWB_COLORS.md`.

- Added bounded legacy and modern CSS HSL/HSLA colors to the shared color
  validator and WinForms painter. Comma syntax and space/slash syntax accept
  finite signed hue values in unitless degrees, `deg`, `grad`, `rad`, or `turn`;
  hue wraps to one turn while percentage saturation/lightness and numeric or
  percentage alpha clamp to their legal paint ranges. Existing compact color
  records now carry HSL through text, backgrounds, physical/logical borders,
  outlines, decorations, text/box shadows, gradients, generated content,
  variables, layers, media, fields, SVG fill/stroke/stops, and `@supports`
  without a new public API. Mixed comma/slash syntax, duplicate slashes,
  missing percentages, nested functions, `none` channels, relative/functional
  channels, and newer color spaces remain outside this bounded slice. The
  focused and full engine suites and final compiled Firespider host pass. All
  42 Boot cases and all six real EXE startup scenarios remain responsive
  without a legacy engine, invalid cast, JIT dialog, or dead startup window.
  All 114 benchmark probes pass over 109 fixtures. Three-round diagnostic
  medians are 529.692 ms clean start, 5.526 ms local load, and 41.683 ms
  layout/paint. Retained managed memory is 29,846.0 KiB for 436 pages, or 68.5
  KiB/page. Required runtime files are 642.0 KiB with a 266.8 KiB GZip
  estimate. Runtime 218 is packaged separately and installed runtime 157
  remains unchanged. See `HSL_COLORS.md`.

- Added bounded named CSS cascade layers. Named `@layer name { ... }` blocks
  participate in Lea's compiled cascade across authored and injected native
  sheets. Later layers win normal declarations, unlayered normal rules outrank
  layers, important order reverses so earlier layers win, layered important
  rules outrank unlayered important rules, and inline declarations retain their
  strongest normal/important positions. Reopened and nested names, local
  specificity/source order, custom properties, generated pseudo content,
  direction, SVG paint/stops, media/supports nesting, compact `@l(...)`
  diagnostics, retained fields, and live resize are covered. Limits are 16
  named layers, three 32-character name segments, and the existing four-level
  conditional nesting ceiling. Anonymous layers, comma order statements, and
  full `revert-layer` rollback stay outside this subset. The focused and full
  engine suites and final compiled Firespider host pass. All 42 Boot cases and
  all six real EXE startup scenarios remain responsive without a legacy engine,
  invalid cast, JIT dialog, or dead startup window. All 113 benchmark probes
  pass over 108 fixtures. Three-round diagnostic medians are 527.970 ms
  clean start, 5.441 ms local load, and 39.099 ms layout/paint. Retained managed
  memory is 29,542.4 KiB for 432 pages, or 68.4 KiB/page. Required runtime files
  are 639.5 KiB with a 265.9 KiB GZip estimate. Runtime 217 is staged separately
  and installed runtime 157 remains unchanged. See `CASCADE_LAYERS.md`.

- Added bounded CSS `@supports` feature queries. Declaration conditions reuse
  Lea's existing bounded parsers for documented display, position, box sizing,
  aspect ratio, physical/logical dimensions, flex, gap, alignment, grid,
  object, wrapping, pointer, overflow, clamp, and custom-property families.
  `selector(...)` reuses the compiled selector grammar rather than a parallel
  recognizer. Case-insensitive `not`, `and`, and `or` accept explicit grouping,
  nested supports/media blocks, four condition levels, eight terms, and 2,048
  characters. Mixed ungrouped operators, multiple or important declarations,
  unknown properties/functions, malformed syntax, and over-limit conditions
  stay inactive. Active rules append compact `@s(...)` diagnostics that compose
  with `@m`, while responsive rebuilds retain field identity and values. The
  focused and full engine suites and final compiled Firespider host pass,
  including all 18 startup regressions and live wide, narrow, intermediate,
  geometry, diagnostic, nesting, limit, and retention checks. All 42 Boot cases
  and all six real EXE startup scenarios remain responsive without a legacy
  engine, invalid cast, JIT dialog, or dead startup window. All 112 benchmark
  probes pass over 106 fixtures. Three-round diagnostic medians are 708.758 ms
  clean start, 5.706 ms local load, and 41.211 ms layout/paint. Retained managed
  memory is 28,996.8 KiB for 424 pages, or 68.4 KiB/page. Required runtime files
  are 636.5 KiB with a 264.8 KiB GZip estimate. Runtime 216 is packaged
  separately and installed runtime 157 remains unchanged. See
  `SUPPORTS_QUERIES.md`.

- Added bounded modern media comparison ranges for width, height, and reduced
  integer aspect ratios. Feature-first, value-first, equality, and
  same-direction ascending/descending chains accept `<`, `<=`, `=`, `>=`, and
  `>` while retaining strict boundaries through nested intersections. They
  compose with legacy min/max syntax, orientation, conjunctions, comma
  alternatives, nesting, and `<style media>`. Mixed-direction chains,
  non-pixel dimensions, malformed/zero ratios, over-bound terms, unknown
  features, and empty equal-edge intersections stay inactive. Diagnostics
  preserve exact operators through compact `@m/@h/@r` labels, and responsive
  rebuilds retain mutable fields. The focused and full engine suites and full
  compiled Firespider host pass, including all 18 startup regressions and live
  wide, tall, intermediate, equality, strict-boundary, geometry, diagnostic,
  and retention checks. All 42 Boot cases and all six real EXE startup
  scenarios remain responsive without a legacy engine, invalid cast, JIT
  dialog, or dead startup window. All 111 benchmark probes pass over 105
  fixtures.
  Three-round diagnostic medians are 608.299 ms clean start, 6.690 ms local
  load, and 47.917 ms layout/paint. Retained managed memory is 28,710.9 KiB for
  420 pages, or 68.4 KiB/page. Required runtime files are 631.5 KiB with a
  262.6 KiB GZip estimate. Runtime 215 is packaged separately and installed
  runtime 157 remains unchanged. See `MEDIA_RANGE_SYNTAX.md`.

- Added bounded exact, minimum, and maximum viewport aspect-ratio media
  conditions. Positive numerator and denominator terms stop at 8,192, reduce
  by GCD, and compare through bounded 64-bit cross multiplication. Ratios
  compose with width, height, portrait/landscape, conjunctions, comma
  alternatives, nested intersections, and `<style media>`. Zero, decimal,
  malformed, over-bound, unknown, and empty ranges stay inactive. Active
  diagnostics add normalized `@r` labels without a new page or host API, and
  responsive rebuilds retain mutable fields. The focused and full engine suites
  and full compiled Firespider host pass, including all 18 startup regressions
  and live tall, wide, intermediate, exact, square, geometry, diagnostic, and
  retention checks. All 42 Boot cases pass after the edit case was hardened
  against a queued Windows hover event, and all six real EXE startup scenarios
  remain responsive without a legacy engine, invalid cast, JIT dialog, or dead
  startup window. All 110 benchmark probes pass over 104 fixtures.
  Three-round diagnostic medians are 531.850 ms clean start, 5.337 ms local
  load, and 44.649 ms layout/paint. Retained managed memory is 28,455.2 KiB for
  416 pages, or 68.4 KiB/page. Required runtime files are 626.0 KiB with a
  261.0 KiB GZip estimate. Runtime 214 is packaged separately and installed
  runtime 157 remains unchanged. See `MEDIA_ASPECT_RATIO.md`.

- Extended bounded responsive CSS across both viewport axes. Compact `E.Vh`
  and atomic `E.Vz(width,height)` join `Vw`; `P.Vh`, read-only `V.Vh`, and
  `G.Vh` expose the applied height to documents, hosts, and diagnostics. Pixel
  min/max/exact height and portrait/landscape orientation work in conjunctions,
  comma alternatives, nested intersections, and `<style media>`. Square
  viewports are portrait. Impossible ranges, conflicting orientations,
  non-pixel dimensions, and unknown values stay inactive. Active diagnostics
  add `@h` and `@o`; compiled-rule selection and retained `Fi` models remain
  shared with width queries. The full engine and compiled Firespider host
  suites pass, including all 18 startup regressions and live portrait,
  short-landscape, geometry, diagnostic, and retention checks. All 42 Boot
  cases and all six real EXE startup scenarios also pass without a JIT dialog,
  invalid cast, MSHTML fallback, or unresponsive window. All 109 benchmark
  probes pass over 103 fixtures. Three-round diagnostic medians are
  1027.283 ms clean start, 6.265 ms local load, and 45.515 ms layout/paint.
  Retained managed memory is 28,292.6 KiB for 412 pages, or 68.7 KiB/page.
  Required runtime files are 624.0 KiB with a 260.4 KiB GZip estimate. Runtime
  213 is packaged separately and installed runtime 157 remains unchanged. See
  `MEDIA_FEATURES.md`.

- Added bounded `url(...)` bitmap fragments to terminal marker, before, and
  after `content`. Text-only pseudos retain the compact `S.Mc` fast path;
  image-containing pseudos receive read-only ordered `S.Mf` fragments using
  compact `Cf.K/X/U`, while inline `Ir.P` keeps picture sources separate from
  `Ir.U` link targets. Images count toward the existing eight-item visual
  bound, text remains capped at 128 UTF-16 units, and the alternative half
  rejects images. Relative/custom-property origins, invalid preservation,
  links, tables, LTR/RTL inside/outside markers, hover/media rebuilds, retained
  fields, and one alternative per pseudo are covered. Generated pictures share
  the existing 24-source admission, origin checks, cache, isolated decoder,
  memory ceilings, and 64px intrinsic display cap. The focused and full engine
  suites, compiled Firespider host, all 18 startup regressions, all 42 Boot
  cases, and all six real EXE startup scenarios pass. All 108 benchmark probes
  pass over 102 fixtures. Three-round diagnostic medians are 530.718 ms clean
  start, 5.075 ms local load, and 45.575 ms layout/paint. Retained managed
  memory is 28,133.1 KiB for 408 pages, or 69.0 KiB/page. Required runtime
  files are 622.0 KiB with a 259.5 KiB GZip estimate. Runtime 212 is packaged
  separately and installed runtime 157 remains unchanged. See
  `GENERATED_IMAGES.md`.

- Added optional bounded alternative text after `/` to terminal marker,
  before, and after `content`. Compact non-inherited `S.Ma` distinguishes no
  alternative (`null`) from an explicit decorative empty alternative; `S.Mc`
  remains the visual atom. Visual and alternative lists each stop at eight
  items and 128 UTF-16 units. Alternatives accept strings, attributes with the
  existing quoted fallback, and named/implicit counters; quote keywords,
  missing items, a second slash, controls, and over-bound output preserve the
  prior cascade value. Accessibility substitutes alternatives in generated
  reading order and name-from-content, includes nested table blocks, prefixes
  list-item names with marker alternatives, and retains explicit ARIA priority.
  Alternative-only runs add no visual width. Authored CSS and compact/readable
  `be/af/ms` commands share one parser. The focused and full engine suites,
  compiled Firespider host, all 18 startup regressions, all 42 Boot cases, and
  all six real EXE startup scenarios pass. All 107 benchmark probes pass over
  101 fixtures. Three-round diagnostic medians are 516.525 ms clean start,
  4.906 ms local load, and 39.974 ms layout/paint. Retained managed memory is
  27,643.6 KiB for 404 pages, or 68.4 KiB/page. Required runtime files are
  635.5 KiB with a 265.0 KiB GZip estimate. Runtime 211 is packaged separately
  and installed runtime 157 remains unchanged. See
  `GENERATED_ALT_TEXT.md`.

- Added bounded generated quotes to terminal marker/before/after content.
  `open-quote`, `close-quote`, `no-open-quote`, and `no-close-quote` share one
  document-order depth across marker, before, children, after, and siblings.
  Inherited `quotes` accepts fixed `auto`, `none`, or one to eight explicit
  pairs; each string stops at 32 UTF-16 units, active depth at eight, and output
  keeps the shared eight-item/128-unit ceilings. Invalid declarations preserve
  prior cascade values. Defining `qu`, readable `quotes`, literal
  `style.quotes`, and compact `S.Qc/Qo/Qx` accessors cover body and ID-scoped
  use. Variables, priority, links, tables, accessibility, hover/media, retained
  fields, geometry, and paint remain engine owned. The focused and full engine
  suites, compiled Firespider host, all 18 startup regressions, all 42 Boot
  cases, and all six real EXE startup scenarios pass. All 106 benchmark probes
  pass over 100 fixtures. Three-round diagnostic medians are 621.923 ms clean
  start, 5.769 ms local load, and 45.761 ms layout/paint. Retained managed
  memory is 27,129.9 KiB for 400 pages, or 67.8 KiB/page. Required runtime files
  are 633.0 KiB with a 263.8 KiB GZip estimate. Runtime 210 is packaged
  separately and installed runtime 157 remains unchanged. See
  `GENERATED_QUOTES.md`.

- Added one bounded quoted missing-value fallback to generated-content
  attributes: `attr(name, "fallback")` and
  `attr(name raw-string, "fallback")`. Present attributes win even when empty;
  only missing attributes use the fallback. The shared marker/before/after
  parser retains its eight-item, 64-character attribute-name, and 128-unit
  output limits. Typed attributes, unquoted or functional fallbacks, extra
  arguments, unsafe controls, and oversized output preserve the prior cascade
  value. Variables resolve before validation. Compact/readable commands,
  links, tables, accessibility names, hover, media, and responsive rebuilds
  use the same engine-owned path without Firespider adapter code. The focused
  and full engine suites, compiled Firespider host, all 18 startup regressions,
  all 42 Boot cases, and all six real EXE startup scenarios pass. All 105
  benchmark probes pass over 99 fixtures. Three-round diagnostic medians are
  762.294 ms clean start, 6.180 ms
  local load, and 47.887 ms layout/paint. Retained managed memory is 26,492.5
  KiB for 396 pages, or 66.9 KiB/page. Required runtime files are 630.0 KiB
  with a 262.6 KiB GZip estimate. Runtime 209 is packaged separately and
  installed runtime 157 remains unchanged. See `ATTR_FALLBACKS.md`.

- Added bounded inline `::before`/`::after` and legacy `:before`/`:after`
  generated content through the compiled cascade. Up to eight string,
  attribute, and named-counter fragments produce at most 128 UTF-16 units;
  attribute names stop at 64 characters. Generated runs reuse inherited text
  styling, nested inline/link/value/table ownership, LTR/RTL layout, hover/media
  rebuilds, retained fields, and accessibility reading order and names with
  explicit ARIA priority. Defining `be/af` and readable
  `beforestyle/afterstyle` commands are page/ID scoped through the existing
  bounded style validator. Replaced/native controls suppress runs; pseudo box
  model, flex/grid items, images, quote keywords, alt text, and arbitrary CSSOM
  remain outside the subset. The focused and full suites, compiled Firespider
  host, all 18 startup regressions, all 42 Boot cases, and all six real EXE
  startup scenarios pass. All 104 benchmark probes pass over 99 fixtures.
  Three-round diagnostic medians are 865.282 ms clean start, 6.644 ms local
  load, and 46.790 ms layout/paint. Retained managed memory is 26,204.5 KiB for
  396 pages, or 66.2 KiB/page. Required runtime files are 630.0 KiB with a
  262.5 KiB GZip estimate. Runtime 208 is packaged separately and installed
  runtime 157 remains unchanged. See `GENERATED_CONTENT.md`.

- Added bounded named CSS counters through non-inherited compact `S.Cr/Ci/Cs`
  state and one tree-order counter pass. `counter-reset`, `counter-increment`,
  and `counter-set` apply in specification order with case-sensitive names,
  default integers, duplicate handling, nested scope, preceding-sibling and
  descendant flow, missing-value zero output, and hidden-branch suppression.
  Properties stop at eight operations, node snapshots at eight active
  instances, names at 32 ASCII characters, and values at -999999..999999.
  Defining `cr/ci/cs`, readable commands, and literal DOM properties share one
  strict validator and retained rebuild; ordinary pages skip the counter pass.
  Named `counter()`/`counters()` output reuses the existing ten styles and
  bounded marker atom. The focused mode, full default suite, compiled
  Firespider host, all 18 startup regressions, all 42 Boot cases, and all six
  real EXE startup scenarios pass. All 103 benchmark probes pass over 98
  fixtures. Three-round diagnostic medians are 823.481 ms clean start, 6.518 ms
  local load, and 43.727 ms layout/paint. Retained managed memory is 24,852.9
  KiB for 392 pages, or 63.4 KiB/page. Required runtime files are 607.5 KiB
  with a 253.3 KiB GZip estimate. Runtime 207 is packaged separately and
  installed runtime 157 remains unchanged. See `CSS_COUNTERS.md`.

- Added bounded implicit `list-item` output to terminal `::marker` content through
  `counter()` and outermost-first `counters()` without a general counter map,
  new model field, alias, or layout pass. Ten existing compact decimal/zero/
  alpha/Roman/bullet/none representations accept their built-in CSS names;
  nested paths reuse Lea's HTML `start`, `value`, and `reversed` numbering.
  Counter depth stops at eight, decoded separators at 32 UTF-16 units, and the
  combined content at the existing eight-item/128-unit bounds. Invalid names,
  styles, functions, depth, controls, separators, and output preserve the prior
  declaration. Variables, important tiers, quoted/attribute composition,
  `ms`/`markerstyle`, image priority, inside/outside LTR/RTL paint, tables,
  hover/media rebuilds, and retained fields share existing paths. The focused
  mode, full default suite including WinForms checks, and compiled Firespider
  host pass. All 18 startup regressions, all 42 Boot cases, and all six real EXE
  startup scenarios pass.
  All 102 benchmark probes pass over 97 fixtures. Three-round diagnostic medians
  are 535.998 ms clean start, 4.461 ms local load, and 40.911 ms layout/paint.
  Retained managed memory is 23,707.1 KiB for 388 pages, or 61.1 KiB/page.
  Required runtime files are 601.5 KiB with a 250.9 KiB GZip estimate. Runtime
  206 is packaged separately and installed runtime 157 remains unchanged. See
  `MARKER_COUNTERS.md`.

- Extended terminal `::marker` content with up to eight source-ordered quoted
  strings and `attr(name)`/`attr(name raw-string)` values while retaining the
  single compact `S.Mc` text atom. Attribute names stop at 64 characters and
  concatenated output stops at 128 UTF-16 units; missing/empty values become
  empty strings, markup-looking values remain text, and malformed, typed,
  fallback, over-item, or oversized declarations preserve the preceding
  cascade value. Variables, important tiers, page/item `ms` and readable
  `markerstyle`, dynamic attributes, hover/media rebuilds, image priority,
  inside/outside LTR/RTL paint, tables, and retained fields share existing Lea
  paths. The focused and full default suites, compiled Firespider host, all 18
  startup regressions, all 42 Boot cases, and all six real EXE startup scenarios
  pass. All 101 benchmark probes pass over 96 fixtures. Three-round diagnostic
  medians are 551.817 ms clean start, 4.270 ms local load, and 42.329 ms
  layout/paint. Retained managed memory is 23,048.2 KiB for 384 pages, or 60.0
  KiB/page. Required runtime files are 600.0 KiB with a 250.1 KiB GZip estimate.
  Runtime 205 is packaged separately and installed runtime 157 remains
  unchanged. See `MARKER_CONTENT.md`.

- Added bounded terminal `::marker` styling with a separate compact marker
  style on `B.Ms` and content state on `S.Mc/Mn`. Originating-element selector
  matching, pseudo specificity, source order, variables, important tiers,
  hover, media queries, and inherited marker text state reuse Lea's compiled
  cascade. The supported marker properties are color, font family/size/weight/
  style, text transform, letter/word spacing, line height, text shadow,
  direction, and one quoted `content` string or `none`. Defining `ms` and
  readable `markerstyle` apply a bounded page default or direct item override;
  image/type/content priority and inside/outside LTR/RTL geometry remain
  engine-owned. The focused and full default suites, compiled Firespider host,
  all 18 startup regressions, all 42 Boot cases, and all six real EXE startup
  scenarios pass. All 100 benchmark probes pass over 95 fixtures. Three-round
  diagnostic medians are 688.206 ms clean start, 5.361 ms local load, and
  38.812 ms layout/paint. Retained managed memory is 22,652.0 KiB for 380
  pages, or 59.6 KiB/page. Required runtime files are 599.0 KiB with a 249.7
  KiB GZip estimate. Runtime 204 is packaged separately and installed runtime
  157 remains unchanged. See `MARKER.md`.

- Added bounded inherited list marker images through compact `S.Li` resolved
  `Uri` state. CSS `list-style-image`, defining `li`, readable
  `liststyleimage`, and literal `style.listStyleImage` share origin checks,
  variables, important tiers, CSS-wide values, invalid preservation, hover, and
  responsive replacement. Atomic `list-style`, compact `ls`, readable
  `liststyle`, and literal `style.listStyle` now accept one type, one image, and
  one position in any order with omitted-component resets and `none` ambiguity.
  Marker images reuse the 24-source picture cache and sandboxed decoder, keep
  intrinsic aspect ratio inside 64x64 pixels, use inside/outside LTR/RTL
  geometry, and fall back to marker text after missing, blocked, unsupported, or
  failed resources. Focused and full default suites plus the compiled
  Firespider host pass, including all 18 startup regressions. All 42 Boot cases
  and all six real EXE startup scenarios pass without a legacy engine, invalid
  cast, JIT dialog, or dead startup window. All 99 benchmark probes pass over 94
  fixtures. Three-round diagnostic medians are 574.938 ms
  clean start, 4.663 ms local load, and 43.293 ms layout/paint. Retained managed
  memory is 21,902.0 KiB for 376 pages, or 58.3 KiB/page. Required runtime files
  are 593.0 KiB with a 247.4 KiB GZip estimate. Runtime 203 is packaged
  separately and installed runtime 157 remains unchanged. See `LIST_IMAGE.md`.

- Added bounded inherited list marker position through compact `S.Lp=o/i`.
  `outside` now occupies a measured logical-start gutter while principal text
  keeps its full first-line width; `inside` remains the first inline marker and
  spacer, affecting wrap normally. CSS `list-style-position`, defining `lp`,
  readable `liststyleposition`, and literal `style.listStylePosition` share the
  compiled cascade. Atomic `list-style`, compact `ls`, readable `liststyle`, and
  literal `style.listStyle` accept one type, one position, or both in either
  order, reset omitted components, and reject duplicate/third/image tokens.
  LTR/RTL gutters, type-position independence, CSS-wide values, variables,
  priority, none, tables, commands, hover, responsive replacement, and retained
  fields have focused coverage. Full core and compiled Firespider host suites
  pass, including all 18 startup regressions. All 98 benchmark probes pass over
  93 fixtures. Three-round diagnostic medians are 1,050.391 ms clean start, 4.666
  ms local load, and 37.792 ms layout/paint. Retained managed memory is 21,398.9
  KiB for 372 pages, or 57.5 KiB/page. Required runtime files are 590.5 KiB with
  a 246.6 KiB GZip estimate. Runtime 202 is packaged separately and installed
  runtime 157 remains unchanged. See `LIST_POSITION.md`.

- Added bounded inherited list marker styles through compact
  `S.Lst=n/d/c/s/1/0/a/A/r/R`. CSS `list-style-type` and the bounded one-type
  `list-style` shorthand share variables, important tiers, CSS-wide values,
  invalid preservation, hover, and media replacement. HTML `type`, ordered
  `start`/`value`, and `reversed` feed the same paint-ready `B.Mk`; direct-list
  counting and -999999..999999 saturation keep sequences bounded. Defining
  `lst`, readable `liststyletype`, and literal `style.listStyleType` commands
  retain controls through the existing rebuild. Disc/circle/square, decimal,
  leading zero, alpha rollover, Roman 1..3999 with decimal fallback, nested
  lists, tables, LTR/RTL, commands, hover, and resize have focused coverage.
  The full default/core suites and compiled Firespider interaction/startup smoke
  pass. All 97 benchmark probes pass over 92 fixtures. Three-round diagnostic
  medians are 759.838 ms clean start, 4.989 ms local load, and 42.719 ms
  layout/paint. Retained managed memory is 20,894.2 KiB for 368 pages, or 56.8
  KiB/page. Required runtime files are 588.0 KiB with a 245.9 KiB GZip estimate.
  Runtime 201 is packaged separately and installed runtime 157 remains
  unchanged. See `LIST_STYLE.md`.

- Added bounded non-inherited multi-line clamping through compact
  `S.Lc=0/1..32`. Standard `line-clamp` and compatibility
  `-webkit-line-clamp` share strict CSS-wide, variable, priority, invalid
  preservation, hover, and responsive cascade paths. Defining `lc`, readable
  `lineclamp`, and literal `style.lineClamp/style.webkitLineClamp` commands use
  the same validator and retained rebuild. The existing inline pass wraps and
  optionally balances first, then keeps the requested visual lines, shortens
  the last visible atom at a bounded text-element boundary, adds one physical
  inline-end marker, and removes all later boxes and hits. Normal flow height
  shrinks while authored DOM/form data and logical UI Automation text remain
  complete. LTR/RTL, inline runs, forced breaks, balanced wrapping, tables,
  command reflow, hover, resize, hidden-link targeting, and field retention have
  focused coverage. The compiled Firespider host passes all 18 startup
  regressions and its full interaction suite. All 96 benchmark probes pass over
  91 fixtures. Three-round diagnostic medians are 671.601 ms clean start, 4.114
  ms local load, and 41.279 ms layout/paint. Retained managed memory is 20,333.2
  KiB for 364 pages, or 55.9 KiB/page. Required runtime files are 585.0 KiB with
  a 245.0 KiB GZip estimate. Runtime 200 is packaged separately and installed
  runtime 157 remains unchanged. See `LINE_CLAMP.md`.

- Added bounded inherited CSS Text Level 4 wrapping state. Compact `S.Wm/Wt`
  retain wrap/nowrap mode and auto/balance style independently, while
  `text-wrap` resets an omitted longhand to its initial value. `white-space`
  updates the wrap mode in declaration order without clearing wrap style, and
  explicit wrap can override the native `pre` default. Defining `tw/twm/tws`,
  readable `textwrap/textwrapmode/textwrapstyle`, and literal
  `style.textWrap/textWrapMode/textWrapStyle` routes share variables, important
  tiers, CSS-wide values, hover, and responsive rebuilds. Balance uses the
  existing inline pass, searches only the width needed to preserve the greedy
  line count, accepts only a lower remaining-space variance, and is capped at
  eight lines and 2,048 text characters. LTR/RTL, indent, forced breaks, and
  tables retain their outer geometry. Full default/core and focused suites, the
  compiled-host smoke, all 42 Boot cases, and all six real EXE startup scenarios
  pass without a legacy engine, invalid cast, JIT dialog, or dead startup
  window. All 95 probes pass over 90 fixtures. Runtime medians are 526.125 ms
  clean diagnostic start, 4.257 ms local load, and 38.151 ms layout/paint.
  Retained managed memory is 19,898.6 KiB for 360 pages, or 55.3 KiB/page.
  Required runtime files are 583.0 KiB with a 244.4 KiB GZip estimate. Runtime
  199 stays isolated and installed runtime 157 remains unchanged. See
  `TEXT_WRAP.md`.

- Completed Lea's bounded inherited CSS text-alignment family. Compact
  `S.Ta/Tl` now retain physical `l/c/r/j` or logical `s/e` state so inherited
  start/end values resolve against each descendant's horizontal direction.
  `text-align` acts as the shorthand for all/last alignment: ordinary values
  reset the last-line state to auto, `justify-all` sets both states to justify,
  and `match-parent` resolves a parent's logical edge through the parent's
  direction. The separate `text-align-all` longhand changes all-line alignment
  without clearing `text-align-last`. Defining `ta/taa/tl` aliases, readable
  `textalign/textalignall/textalignlast`, and literal
  `style.textAlign/textAlignAll/textAlignLast` assignments share the strict
  cascade. No new record or layout pass was added; line placement still uses the
  existing inline pass and retained geometry. Full default/core and focused
  suites, the compiled-host smoke, all 42 Boot cases, and all six real EXE
  startup scenarios pass without a legacy engine, invalid cast, JIT dialog, or
  dead startup window. All 94 probes pass over 89 fixtures. Runtime medians are
  497.401 ms clean diagnostic start, 4.353 ms local load, and 38.156 ms
  layout/paint. Retained managed memory is 19,432.6 KiB for 356 pages, or 54.6
  KiB/page. Required runtime files are 579.5 KiB with a 243.3 KiB GZip
  estimate. Runtime 198 stays isolated and installed runtime 157 remains
  unchanged. See `TEXT_ALIGN.md`.

- Added bounded inherited CSS `text-align-last` through compact `S.Tl` state
  and the defining `tl` body/ID command. `auto`, `start`, `end`, `left`,
  `right`, `center`, `justify`, `match-parent`, CSS-wide values, variables,
  important tiers, inline overrides, hover, responsive rebuilds, readable
  `textalignlast`, and literal `style.textAlignLast` share the compiled
  cascade. The existing inline pass aligns final visual lines and lines before
  forced breaks without adding another layout pass or changing outer boxes,
  hits, find, focus, or accessibility geometry. Logical start/end resolve for
  horizontal LTR/RTL; ordinary `text-align:justify` still starts its last line,
  while explicit `text-align-last:justify` fills it. Full default/core and
  focused suites, the compiled-host smoke, all 42 Boot cases, and all six
  actual EXE startup scenarios pass without a legacy engine, invalid cast, JIT
  dialog, or dead startup window. All 93 probes pass over 88 fixtures. Runtime
  medians are 506.068 ms clean diagnostic start, 3.981 ms local load, and
  38.396 ms layout/paint. Retained managed memory is 19,089.4 KiB for 352
  pages, or 54.2 KiB/page. Required runtime files are 578.5 KiB with a 242.9
  KiB GZip estimate. Runtime 197 stays isolated and installed runtime 157
  remains unchanged. See `TEXT_ALIGN_LAST.md`.

- Added bounded inherited CSS `text-underline-offset` through compact scalar
  `S.Uo/Uop` state and the defining `uo` body/ID command. Signed integer pixels
  are bounded to -32..32, percentages to -100..100, and final resolved paint to
  64 pixels. CSS-wide values, variables, important tiers, inline overrides,
  hover, responsive rebuilds, readable `textunderlineoffset`, and literal
  `style.textUnderlineOffset` share the compiled cascade. The property remains
  independent of the text-decoration shorthand. Automatic unstyled lines stay
  on the native font fast path; nonzero offsets reuse the lightweight custom
  painter without changing layout, hits, find, focus, or accessibility
  geometry. Default links avoid duplicate native lines when offset or custom
  decoration components are active. Full default/core and focused suites, the
  compiled-host smoke, all 42 Boot cases, and all six actual EXE startup
  scenarios pass without a legacy engine, invalid cast, JIT dialog, or dead
  startup window. All 92 probes pass over 87 fixtures. Runtime medians are
  507.538 ms clean diagnostic start, 4.066 ms local load, and 36.635 ms
  layout/paint. Retained managed memory is 18,774.2 KiB for 348 pages, or 53.9
  KiB/page. Required runtime files are 576.5 KiB with a 242.5 KiB GZip
  estimate. Runtime 196 stays isolated and installed runtime 157 remains
  unchanged. See `TEXT_UNDERLINE_OFFSET.md`.

- Added bounded inherited CSS text-decoration line/color/style/thickness through
  compact scalar `S.Td/Tc/Tds/Tw` state and defining
  `td/tdl/tdc/tds/tdw` body/ID commands. Atomic `text-decoration` and separate
  line/color/style/thickness longhands support underline and line-through,
  current or bounded colors, solid/double/dotted/dashed/wavy styles, and auto or
  1..8 pixel thickness. CSS-wide inheritance/reset, variables, important tiers,
  inline overrides, hover, responsive rebuilds, readable names, and literal
  `style.textDecoration*` share the compiled cascade with strict invalid-value
  preservation. Current-color solid lines retain native Font painting; custom
  paint uses existing text rectangles and no point-list allocation. Outer
  layout, hits, find, focus, and accessibility geometry remain stable. Full
  default/core and focused suites, compiled-host smoke, all 42 Boot cases, and
  all six actual EXE startup scenarios pass without a legacy engine, invalid
  cast, JIT dialog, or dead startup window. All 91 probes pass over 86 fixtures.
  Runtime medians are 579.495 ms clean diagnostic start, 3.815 ms local load,
  and 37.244 ms layout/paint. Retained managed memory is 18,389.6 KiB for 344
  pages, or 53.5 KiB/page. Required runtime files are 575.0 KiB with a 241.9
  KiB GZip estimate. Runtime 195 stays isolated and installed runtime 157
  remains unchanged. See `TEXT_DECORATION.md`.

- Added one bounded inherited CSS `text-shadow` layer through nullable compact
  `S.Ts`, reusing `Sw.X/Y/B/C` for signed offsets, blur, and color, plus defining
  short `ts` body/ID commands. Offsets are limited to -128..128 pixels, blur to
  0..32 pixels, and source/color input uses existing strict bounds. CSS-wide
  inheritance/reset, variables, important tiers, inline overrides, hover,
  responsive rebuilds, readable `textshadow`, and literal `style.textShadow`
  all share the compiled cascade. The WinForms renderer paints zero blur once
  and approximates blur with at most four allocation-free sample rings; layout,
  hits, find, focus, and accessibility geometry remain unchanged. Full
  default/core suites, the focused test, and the compiled-host smoke pass. All
  42 Boot cases pass, with the clipboard-sensitive edit case recorded on its
  focused rerun, and all six actual EXE startup scenarios remain responsive
  without a legacy engine, invalid cast, JIT dialog, or dead startup window. All
  90 probes pass over 85 fixtures. Runtime medians are 519.936 ms clean
  diagnostic start, 4.478 ms local load, and 40.560 ms layout/paint. Retained
  managed memory is 18,024.5 KiB for 340 pages, or 53.0 KiB/page. Required
  runtime files are 568.5 KiB with a 239.9 KiB GZip estimate. Runtime 194 stays
  isolated and installed runtime 157 remains unchanged. See `TEXT_SHADOW.md`.

- Added bounded inherited CSS `text-indent` through compact `S.Ti/Tip` signed
  integer pixel/percentage state and short `ti` body/ID commands. The parser
  accepts -2000..2000 pixels, -100..100 percent, unitless zero, CSS-wide values,
  variables, important tiers, hover, and responsive rebuilds while preserving
  prior state on malformed or unsupported input. The WinForms renderer changes
  only the first visual line's available width and resolves inline-start for
  LTR/RTL, including negative values; later wrapped lines retain full width.
  Paint, links, hits, find, focus, and accessibility share the retained geometry.
  Full default/core suites, the focused test, compiled-host smoke, all 89 probes
  over 84 fixtures, all 42 Boot cases, and all six actual EXE startup scenarios
  pass without a legacy engine, invalid cast, JIT dialog, or dead startup window.
  Runtime medians are 498.029 ms clean diagnostic start, 3.635 ms local load,
  and 35.871 ms layout/paint. Retained managed memory is 17,794.9 KiB for 336
  pages, or 53.0 KiB/page. Required runtime files are 567.0 KiB with a 239.2 KiB
  GZip estimate. Runtime 193 stays isolated and installed runtime 157 remains
  unchanged. See `TEXT_INDENT.md`.

  A parallel UI Automation experiment reused the already-marshaled TextPattern
  object as range identity and explicitly retained its COM pointer. Current .NET
  and .NET Framework external clients still failed at the first non-empty range
  `GetText` call, so the unsafe path was removed and the stable empty visible-
  range fallback remains. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added combined host/helper image-memory admission after the hardened
  AppContainer handshake and before encoded payload transfer. A clamped
  process-wide `Icl` ceiling defaults to 384 MiB; `Ihm/Icm/Iad` expose the host
  private sample, conservative host plus worker-private/Job-peak plus preflight
  reservation signal, and admission result. Missing samples and saturating
  arithmetic overflow fail closed. A deterministic 128 MiB test projection is
  rejected with zero native payload admissions, released `It`, and termination
  of the exact reused helper, while normal PNG/JPEG/GIF/BMP/ICO, cancellation,
  restart, AppContainer, network/file denial, and Job behavior continue to
  pass. The full default/core suite, compiled-host smoke, all 88 probes over 83
  fixtures, all 42 Boot cases, and all six actual EXE startup scenarios pass
  without a legacy engine, invalid cast, JIT dialog, or dead startup window.
  Runtime medians are 515.731 ms clean diagnostic start, 3.652 ms local load,
  and 36.048 ms layout/paint. Retained managed memory is 17,552.3 KiB for 332
  pages, or 52.9 KiB/page. Required runtime files are 565.0 KiB with a 238.7 KiB
  GZip estimate. Runtime 192 stays isolated and installed runtime 157 remains
  unchanged. See `IMAGE_ISOLATION.md`. Combined admission uses conservative
  point samples rather than a continuous OS-wide host cap; the scoped baseline
  stays at 100%, not full-web compatibility.

- Moved the shared native image decoder into a deterministic zero-capability
  AppContainer without adding a runtime or widening the image protocol. The
  launcher creates or reuses one per-user profile, adds an exact SID-scoped
  read/execute ACE to `Lea.Image.exe` only, and verifies the real suspended
  child token's identity, empty capability set, low integrity, and stripped
  privileges before retaining the existing handle allowlist, Job, ASLR, CPU,
  memory, and mitigation gates. New `Iac/Ind/Ial` aliases are each three letters
  and report the live boundary. Focused tests place a listener on loopback and
  a canary in the host's temp directory; both accesses are denied while PNG,
  JPEG, GIF, BMP, and ICO decoding, worker reuse, cancellation, and restart
  continue to pass. The full default/core suite, compiled-host smoke, all 87
  probes over 83 fixtures, all 42 Boot cases, and all six actual EXE startup
  scenarios pass without a legacy engine, invalid cast, JIT dialog, or dead
  startup window. The Boot loopback server now releases speculative zero-byte
  legacy connections within one second without hiding partial/malformed HTTP,
  eliminating a reproduced mixed-domain reload timeout; five focused repeats
  and the final combined matrix pass. Runtime medians are 534.292 ms clean diagnostic start, 3.908
  ms local load, and 35.246 ms layout/paint. Retained managed memory is 17,552.3
  KiB for 332 pages, or 52.9 KiB/page. Required runtime files are 563.0 KiB with
  a 238.0 KiB GZip estimate. Runtime 191 stays isolated and installed runtime
  157 remains unchanged. See `IMAGE_ISOLATION.md`. The AppContainer profile
  persists per user, system paths available to AppContainers remain available,
  and host/helper peak admission remains future work; the scoped baseline stays
  at 100%, not full-web compatibility.

- Added bounded direction-aware paint-only ellipsis and read-only TextPattern
  range geometry without growing public model records. LTR and RTL unwrapped
  overflow place the marker at physical inline-end, retain grapheme-safe
  authored prefixes, preserve original layout and hit rectangles, and suppress
  elided link paint without inventing accessible ellipsis text. RTL partial,
  wrapped, and point-derived automation ranges now map from the physical right
  while full logical text remains in source order. Plain, linked, combining,
  table, wrapped, and live `dr` cases share one fixture. The full repository
  suite, focused smoke, compiled-host smoke, all 86 probes over 83 fixtures, all
  42 Boot cases, and all six actual EXE startup scenarios pass without a legacy
  engine, invalid cast, JIT dialog, or dead startup window. The smoke harness
  now restores its UI synchronization context around blocking download checks,
  and the compiled host waits for fitted image decode before exact pixel
  assertions. Runtime medians are 518.830 ms clean diagnostic start, 3.709 ms
  local load, and 35.461 ms layout/paint. Retained managed memory is 17,552.3
  KiB for 332 pages, or 52.9 KiB/page. Required runtime files remain 559.5 KiB
  with a 236.2 KiB GZip estimate. Runtime 190 stays isolated and installed
  runtime 157 remains unchanged. See `DIRECTION.md`. Unicode bidi glyph
  reordering/embedding, `dir=auto`, vertical writing modes, and custom markers
  remain outside this milestone; the scoped baseline stays at 100%, not
  full-web compatibility.

- Added bounded direction-aware horizontal inline flow and marker placement
  without growing `S`, `B`, `Ir`, or renderer records. RTL plain text, mixed
  styled runs/links, wrapped, nowrap, justified, physical/logical alignment,
  structured table parts, and live `dr` reflow retain source atoms while final
  X coordinates advance from the right. Ordered, nested, unordered, and
  disclosure markers reuse `B.Mk` at inline-start. Unwrapped clipped lines keep
  the right edge and overflow left; source/accessibility text and link ownership
  remain stable. Final cache-navigation stress also reproduced and fixed an
  asynchronous `V.OnPaint` layout-enumeration race: layout and owner records are
  now published through synchronized snapshots, with full UI-thread stacks kept
  by the Boot harness. The formerly failing case passes 20/20 repeated runs.
  All 85 probes over 82 fixtures, the focused repository
  checkpoint, compiled-host smokes, all 42 Boot cases, and all six actual EXE
  startup scenarios pass without legacy-engine, cast, or JIT dialogs. Runtime
  medians are 504.408 ms clean diagnostic start, 3.410 ms local load, and 35.733
  ms layout/paint. Retained managed memory is 17,361.6 KiB for 328 pages, or
  52.9 KiB/page. Required runtime files remain 559.0 KiB with a 236.0 KiB GZip
  estimate. Runtime 189 stays isolated and installed runtime 157 remains
  unchanged. See `DIRECTION.md`. Unicode bidi glyph reordering/embedding,
  `dir=auto`, vertical writing modes, and RTL paint-only ellipsis remain outside
  this milestone; the scoped baseline stays at 100%, not full-web compatibility.

- Added bounded direction-aware horizontal flex and grid layout without growing
  the compact style model. Row and row-reverse combine `S.Fd` with inherited
  `S.Dr` as one effective reverse bit across justify, order, and wrapped lines;
  column flex maps horizontal start/end alignment. Grid occupancy remains
  logical while final track coordinates map auto, dense, column flow, explicit
  lines/spans, named areas, unequal tracks, content alignment, and self-alignment
  into RTL. Source/accessibility order remains stable and compact `dr` rebuilds
  geometry in place. All 84 probes over 81 fixtures and compiled-host smokes
  pass, all 42 Boot cases pass, and both six-scenario actual EXE startup runs
  remain responsive without legacy-engine, cast, or JIT dialogs. Measured
  medians are 538.084 ms clean diagnostic start, 3.601 ms local load, and 33.564
  ms layout/paint. Retained managed memory is 21,234.4 KiB for 405 pages, or
  52.4 KiB/page. Required runtime files remain 558.5 KiB with a 235.7 KiB GZip
  estimate. Runtime 188 stays isolated and installed runtime 157 remains
  unchanged. See `DIRECTION.md`. Unicode bidi glyph
  reordering, `dir=auto`, vertical writing modes, and `wrap-reverse` remain
  outside this milestone; the scoped baseline stays at 100%, not full-web
  compatibility.

- Added bounded inherited horizontal `direction: ltr/rtl` through compact
  `S.Dr=l/r`, HTML `dir`, and an independent pre-cascade direction pass so
  logical properties map correctly regardless of declaration order. Logical
  insets, margin, padding, border shorthands/components, all four corner radii,
  and `text-align: start/end` now map for RTL while physical properties remain
  physical. Explicit logical inheritance translates between the parent's and
  child's directions. Short/readable `dr`, body and ID targets, familiar DOM
  assignments, empty authored restore, variables, important tiers, CSS-wide
  values, and invalid rejection share one bounded style path. Full repository
  and compiled-host smokes pass, along with 83 probes over 80 fixtures, all 42
  Boot cases, and all six actual EXE startup scenarios without legacy-engine,
  cast, or JIT dialogs. Measured medians are 526.671 ms clean diagnostic start,
  3.672 ms local load, and 34.037 ms layout/paint. Retained managed memory is
  20,871.2 KiB for 400 pages, or 52.2 KiB/page. Required runtime files total
  558.5 KiB with a 235.6 KiB GZip estimate. Runtime 187 stays isolated and
  installed runtime 157 remains unchanged. See `DIRECTION.md`. Unicode bidi
  glyph reordering and vertical writing modes remain outside this milestone;
  the scoped baseline stays at 100%, not full-web compatibility.

- Added bounded horizontal left-to-right logical corner radii. Four logical
  longhands map directly onto the existing clockwise `R0..R3/Q0..Q3`
  elliptical fields with no new style record or renderer path. Physical and
  logical source order, important tiers, custom properties, explicit shorthand
  and corner inheritance, CSS-wide resets, invalid atomic preservation, and
  normalized box paint stay shared. Nine `rd/rtl/rtr/rbr/rbl/rss/rse/res/ree`
  aliases are at most three letters, with readable and familiar literal DOM
  forms plus authored restore. Full repository and compiled-host smokes pass,
  along with 82 probes over 79 fixtures, all 42 Boot cases, and all six actual
  EXE startup scenarios without legacy-engine or JIT dialogs. Measured medians
  are 530.557 ms clean diagnostic start, 3.557 ms local load, and 33.472 ms
  layout/paint. Retained managed memory is 20,366.8 KiB for 395 pages, or 51.6
  KiB/page. Required runtime files total 555.0 KiB with a 234.6 KiB GZip
  estimate. Runtime 186 stays isolated and installed runtime 157 remains
  unchanged. See `LOGICAL_RADII.md`. The scoped baseline stays at 100%, not
  full-web compatibility.

- Added bounded horizontal left-to-right logical borders. Six axis/edge
  shorthands and eighteen width/style/color components compile into the four
  physical widths plus one lazy eight-string edge-detail record; uniform
  borders allocate no new record. Physical side shorthands and one-to-four-value
  width/style/color lists now preserve per-edge cascade state atomically.
  Source order, important tiers, custom properties, functional colors,
  inheritance/reset, hidden used widths, normal/positioned/flex/grid/table/form
  geometry, and mixed rounded paint remain on shared paths. All 24 aliases are
  at most three letters, with readable and familiar literal DOM forms. Full
  repository and compiled-host smokes pass, along with 81 probes over 78
  fixtures, all 42 Boot cases, and all six real EXE startup scenarios. Measured
  medians are 534.686 ms clean diagnostic start, 3.658 ms local load, and 33.597
  ms layout/paint. Retained managed memory is 20,091.5 KiB for 390 pages, or
  51.5 KiB/page. Required runtime files total 553.0 KiB with a 234.0 KiB GZip
  estimate. Runtime 185 stays isolated and installed runtime 157 remains
  unchanged. See `LOGICAL_BORDERS.md`. The scoped baseline stays at 100%, not
  full-web compatibility.

- Added bounded horizontal left-to-right logical margins and padding. Four axis
  shorthands and eight start/end longhands compile into existing
  `S.Mt/Mr/Mb/Ml` and `S.Pt/Pr/Pb/Pl` records. Physical/logical source order,
  important tiers, custom properties, inheritance/reset, atomic invalid
  fallback, signed -2,000..2,000 pixel margins, non-negative padding, normal,
  positioned, flex, grid, table, paint, hit, editor, and accessibility geometry
  remain on shared paths. Short `mi/mb/pi/pb`, eight three-letter edge aliases,
  readable forms, and all familiar literal DOM properties need no new style or
  host state. Full repository and compiled-host smokes pass, along with 80
  probes over 77 fixtures, all 42 Boot cases, and all six real EXE startup
  scenarios. Measured medians are 555.956 ms clean diagnostic start, 3.405 ms
  local load, and 34.165 ms layout/paint. Retained managed memory is 19,652.8
  KiB for 385 pages, or 51.0 KiB/page. Required runtime files total 558.5 KiB
  with a 237.6 KiB GZip estimate. Runtime 184 stays isolated and installed
  runtime 157 remains unchanged. See `LOGICAL_BOX_MODEL.md`. The scoped baseline
  stays at 100%, not full-web compatibility.

- Added bounded horizontal left-to-right logical sizing and positioned insets.
  `inline-size`/`block-size` and their min/max forms compile into existing
  width/height records; four logical inset longhands and atomic one/two-value
  axis shorthands compile into `Tp/Rt/Bm/Lt`. Source order, important tiers,
  custom properties, explicit size inheritance, resets, all existing dimension
  terms/functions, positioned stretching, layout, paint, hits, editors, and
  accessibility remain on shared paths. Short `is/bs`, readable command forms,
  and literal `style.inlineSize/style.blockSize` assignments reuse `wd/ht`.
  Writing modes and bidi remapping remain explicit limits. Full repository and
  compiled-host smokes pass, along with 79 probes over 76 fixtures, all 42 Boot
  cases, and all six real EXE startup scenarios. Measured medians are 554.307 ms
  clean diagnostic start, 3.598 ms local load, and 33.759 ms layout/paint.
  Retained managed memory is 19,314.1 KiB for 380 pages, or 50.8 KiB/page.
  Required runtime files total 555.0 KiB with a 236.4 KiB GZip estimate.
  Runtime 183 stays isolated and installed runtime 157 remains unchanged. See
  `LOGICAL_SIZING_INSETS.md`. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added bounded retained initial-fragment placement to the WinForms renderer.
  One current page/fragment pair is reapplied after first paint, host or font
  changes, stylesheet/DOM rebuilds, and asynchronous intrinsic image decode.
  Ordinary scrolling, focus movement, missing targets, navigation, and disposal
  release the anchor. Compact `V.Fa/Fj/Fy` expose active state, a saturated
  placement count, and the latest document Y without adding a timer, request,
  host layout path, or external browser runtime. Synchronous image completion
  now dirties current-page layout so decoded intrinsic dimensions participate
  in reflow. Full repository and compiled-host smokes pass, along with 78 probes
  over 75 fixtures, all 42 Boot cases, and all six real EXE startup scenarios.
  Measured medians are 528.623 ms clean diagnostic start, 3.501 ms local load,
  and 34.481 ms layout/paint. Retained managed memory is 19,046.1 KiB for 375
  pages, or 50.8 KiB/page. Required runtime files total 554.5 KiB with a 236.1
  KiB GZip estimate. Runtime 182 stays isolated and installed runtime 157
  remains unchanged. See `INITIAL_FRAGMENT_STABILITY.md`. The scoped baseline
  stays at 100%, not full-web compatibility.

- Added bounded two/three-term CSS `min()` and `max()` plus three-term
  `clamp()` to width, height, minimum, maximum, compact `wd/ht`, and literal DOM
  style paths. Every argument reuses one fixed, percentage, viewport,
  font-relative, or `calc(PU +/- Npx)` term. Lazy `S.Wk/Wnk/Wxk` and matching
  height aliases expose function kind; `Wq/Hq/Wv/Hv` inspect argument counts
  and values without allocating state on ordinary pages. One resolver carries
  the functions through blocks, positioning, flex, grid, tables, fields,
  images, aspect ratios, content/border boxes, paint, hits, editing,
  accessibility, media replacement, and live resize. Percentage height
  functions below auto-height parents remain authored but use natural layout.
  Invalid arity, nested functions, decimals, and arbitrary arithmetic remain
  atomic. Full repository and compiled-host smokes pass, along with 77 probes
  over 74 fixtures, all 42 Boot cases, and all six real EXE startup scenarios.
  Measured medians are 476.482 ms clean diagnostic start, 3.451 ms local load,
  and 32.159 ms layout/paint. Retained managed memory is 18,925.5 KiB for 370
  pages, or 51.1 KiB/page. Required runtime files total 536.5 KiB with a 229.0
  KiB GZip estimate. Runtime 181 stays isolated and installed runtime 157
  remains unchanged. See `SIZE_FUNCTIONS.md`. The scoped baseline stays at
  100%, not full-web compatibility.

- Added bounded `em` and `rem` values to width, height, minimum, maximum,
  compact `wd/ht`, and literal DOM style paths. Existing lazy tuples add unit
  codes `5/6`; compact `P.Rf` retains an explicit root font and `V.Rf` exposes
  the applied host fallback. `em` follows each element's computed font while
  `rem` follows the root across media changes. One resolver carries both units
  through blocks, positioning, flex, grid, tables, fields, images, aspect
  ratios, content/border boxes, paint, hits, editing, and accessibility.
  Strict integer 1..100 terms and one `calc(PU +/- Npx)` form preserve the small
  parser; `font-size` remains bounded integer pixels and unsupported suffixes
  are now rejected atomically. Full repository and compiled-host smokes pass,
  along with 76 probes over 73 fixtures, all 42 Boot cases, and all six real EXE
  scenarios. Measured medians are 517.283 ms clean diagnostic start, 3.494 ms
  local load, and 32.135 ms layout/paint. Retained managed memory is 18,243.2
  KiB for 365 pages, or 50.0 KiB/page. Required runtime files total 533.5 KiB
  with a 227.7 KiB GZip estimate. Runtime 180 stays isolated and installed
  runtime 157 remains unchanged. See `FONT_RELATIVE_UNITS.md`. The scoped
  baseline stays at 100%, not full-web compatibility.

- Added bounded classic `vw`, `vh`, `vmin`, and `vmax` values to width, height,
  minimum, maximum, compact `wd/ht`, and literal DOM style paths. Responsive
  tuples stay lazy and add compact `Wu/Wnu/Wxu` and `Hu/Hnu/Hxu` unit codes;
  `V.Vh` complements the live `V.Vw` alias. One resolver now carries viewport
  references through blocks, positioned content, flex, grid, tables, controls,
  images, aspect ratios, content/border boxes, paint, hits, editing, and
  accessibility. Viewport units remain live below auto-height parents and on
  height-only resizes, while ordinary percentage semantics are unchanged.
  Strict 1..100 terms and one `calc(PU +/- Npx)` form avoid a general evaluator;
  dynamic viewport variants, decimals, nested functions, and arbitrary
  arithmetic remain inert. Full repository and compiled-host smokes pass,
  along with 75 probes over 72 fixtures, all 42 Boot cases, and all six real EXE
  scenarios. Measured medians are 507.920 ms clean diagnostic start, 3.299 ms
  local load, and 32.998 ms layout/paint. Retained managed memory is 17,741.3
  KiB for 360 pages, or 49.3 KiB/page. Required runtime files total 550.0 KiB
  with a 233.9 KiB GZip estimate. Runtime 179 is isolated; installed runtime
  157 remains unchanged. See `VIEWPORT_UNITS.md`. The scoped baseline stays at
  100%, not full-web compatibility.

- Added bounded percentage and single-expression calculated heights through
  compact `S.Hp/Ho`, `Hnp/Hno`, and `Hxp/Hxo` tuples plus `ht(id,value)`,
  readable `height(id,value)`, and literal `element.style.height`. Definite
  viewport, parent, positioned, flex, and grid references now feed one shared
  height resolver across blocks, tables, controls, images, aspect ratios,
  content/border boxes, paint, hits, editing, accessibility, and live resize.
  Percentages below auto-height parents remain natural instead of guessing a
  circular reference. Invalid declarations are atomic; plain authored
  four-digit pixel heights preserve Lea's legacy 2000px safety cap while DOM
  commands remain strict. Firespider startup now automatically restores an
  unclean session without a blocking recovery dialog, and the retired
  `fsin.glitch.me` default falls back to the bundled Lea start page. The EXE
  harness uses bounded native tab/Resume checks and inspects only separate
  top-level dialogs, avoiding menu-label false positives and startup stalls.
  Full repository and compiled-host smokes pass, along with 74 probes over 71
  fixtures, all 42 Boot cases, and all six real EXE scenarios. Measured medians
  are 578.142 ms clean diagnostic start, 3.413 ms local load, and 35.418 ms
  layout/paint. Retained managed memory is 17,177.5 KiB for 355 pages, or 48.4
  KiB/page. Required runtime files total 548.5 KiB with a 233.3 KiB GZip
  estimate. Runtime 178 is isolated; installed runtime 157 remains unchanged.
  See `CALCULATED_HEIGHTS.md` and `Startup-smooth-host.patch`. The scoped
  baseline stays at 100%, not full-web compatibility.

- Added bounded vertical `position:sticky` through compact `S.Pn=k`. Sticky
  boxes keep their normal-flow slot, establish retained positioning/stacking
  context ownership, follow signed pixel `top`/`bottom` root-scroll insets, move
  descendants as one integer-offset subtree, and stop at the nearest retained
  parent content edge. Overflow clips are rebuilt from moved owners, so paint,
  links, pointer hits, native editors, keyboard focus, and accessibility use the
  same geometry. Pages without an active vertical sticky inset retain the old
  scroll fast path. Extreme `Sc` deltas now use a bounded 64-bit intermediate
  instead of wrapping. Full engine/WinForms, compiled Firespider, and extracted
  runtime tests pass. The benchmark passes 73 probes over 70 fixtures. All 42
  Boot cases pass, including the real 35-second recovery timer without the old
  cast dialog, and all six actual EXE startup scenarios are responsive,
  dialog-free, and MSHTML-free. Measured medians are 494.192 ms clean-process,
  3.432 ms local-load, and 33.163 ms layout/paint. Retained managed memory is
  16,694.9 KiB for 350 pages, or 47.7 KiB/page. Required runtime files total
  546.5 KiB with a 232.5 KiB GZip estimate. Runtime 177 is isolated;
  installed runtime 157 remains unchanged. See `STICKY_POSITIONING.md`. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added bounded percentage and single-expression calculated widths without a
  general CSS evaluator. Authored `width`, `min-width`, and `max-width` compile
  into optional percentage/pixel tuples and resolve at layout time across root
  and nested blocks, controls, images, flex, grid, tables, paint, hits, and
  accessibility. Supported calculations contain one 1..100% term, one `+` or
  `-`, and one 0..2000px term; invalid declarations remain atomic. Custom
  properties, important priority, explicit inheritance, content/border boxes,
  min/max clamping, and resize reflow share the existing bounded paths. Compact
  `wd(id,value)`, readable `width(id,value)`, and literal
  `element.style.width` use the same parser; empty assignment restores authored
  width without erasing unrelated inline style or field state. Local async file
  navigation now yields and rechecks cancellation before disk input, removing a
  disk-speed race in disposal. Full engine, WinForms, compiled Firespider, and
  staged runtime tests pass. The benchmark passes 72 probes over 69 fixtures.
  All 42 Boot cases pass, including the real 35-second recovery timer without
  the old cast dialog, and all six actual EXE startup scenarios are responsive
  and MSHTML-free. Measured medians are 504.566 ms clean-process, 3.470 ms
  local-load, and 31.742 ms layout/paint. Retained managed memory is 16,587.0
  KiB for 345 pages, or 48.1 KiB/page. Required runtime files total 544.5 KiB
  with a 231.8 KiB GZip estimate. Runtime 176 is isolated and source-backed-up;
  installed runtime 157 remains unchanged. See `CALCULATED_WIDTHS.md`. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added compact `dp(id,value)` display mutation plus literal
  `document.getElementById(...).style.display`, `location`, `location.href`, and
  `location.assign(...)` compatibility. Display accepts only a bounded standard
  keyword set; empty assignment restores the authored cascade while retaining
  unrelated inline declarations. Familiar navigation maps to the existing
  guarded `go` request, and relative local-file targets now preserve real query
  and fragment components. Computed values, `location.replace`, history APIs,
  arbitrary methods, unsupported display modes, and unsafe schemes remain inert.
  Core and WinForms tests verify retained layout/accessibility state and real
  flex reflow; the compiled Firespider smoke passes through the rebuilt host.
  The benchmark passes 71 probes over 68 fixtures. All 42 Boot cases pass in one
  combined run, including the real 35-second recovery timer without the old cast
  dialog, and all six actual EXE startup scenarios remain responsive and
  MSHTML-free. Measured medians are 512.230 ms clean-process, 3.203 ms local-load,
  and 32.563 ms layout/paint. Retained managed memory is 16,216.9 KiB for 340
  pages, or 47.7 KiB/page. Required runtime files total 541.0 KiB with a 230.7
  KiB GZip estimate. Runtime 175 is isolated and installed runtime 157 remains
  unchanged. See `DISPLAY_NAVIGATION.md`. The scoped baseline stays at 100%, not
  full-web compatibility.

- Added a literal `document.getElementById` compatibility bridge without adding
  a general JavaScript VM. Quoted ID lookups can use `textContent/innerText`,
  `value`, checked/disabled/hidden/open booleans, `focus()`,
  `classList.add/remove/toggle`, and background/color/font-size styles. Each
  statement maps to the existing short text, state, class, focus, and style
  commands, retaining all target, value, field, focus, style, and
  32-command bounds. `innerHTML`, selectors, computed IDs/values, callbacks,
  expressions, and unknown APIs remain inert. The dedicated fixture, full core
  and WinForms suites, strengthened compiled Firespider smoke, and benchmark all
  pass, with 70 probes over 67 fixtures. All 42 Boot cases pass in one combined
  run; recovery pumped the real 30-second timer without the old cast dialog.
  All six actual EXE startup scenarios remained responsive and MSHTML-free.
  Firespider reports ASLR true, CPU 2500, all 15 helper guard bits, and 8,486,912
  private/Job-peak bytes. Required runtime files total 540.0 KiB with a 230.1
  KiB GZip estimate; measured medians are 634.045 ms clean-process, 4.069 ms
  local-load, and 33.864 ms layout/paint, not a speedup claim. Runtime 174 is
  isolated and installed runtime 157 remains unchanged. See `DOM_BRIDGE.md`.
  The scoped baseline stays at 100%, not full-web compatibility.

- Added verified process-creation mitigations and a Job CPU hard cap to the
  image-helper boundary. The extended startup policy requests DEP, SEHOP,
  heap-corruption termination, forced relocation, bottom-up ASLR, and
  architecture-supported high-entropy ASLR; Windows must report forced
  relocation plus bottom-up randomization while the process is still suspended.
  The Job must then accept and read back enabled hard-cap flags and the exact
  CPU rate before assignment/resume. Compact `Ias/Icp/Icr` aliases report startup
  ASLR, active CPU policy, and a configurable 100-10,000 basis-point rate; 2500
  (25.00%) is the default, and a change restarts the shared worker. Live tests
  query both ASLR and the Job's private CPU policy, clamp both rate bounds, and
  prove a changed rate gets a new PID. Full picture/core/host and benchmark
  suites pass, with 69 probes over 66 fixtures, all 42 Boot cases in one combined
  run, and all six real EXE startup scenarios. The recovery case pumped the real
  30-second timer without the old cast dialog. Firespider reports ASLR true,
  CPU 2500, all 15 helper guard bits, and 8,556,544 private/Job-peak bytes.
  Required runtime files total 538.5 KiB with a 229.7 KiB GZip estimate; medians
  are 499.587 ms clean-process, 3.231 ms local-load, and 32.724 ms layout/paint,
  not a speedup claim. The helper remains outside an AppContainer and retains
  user read/network identity; capability-denied networking, custom ACLs, and a
  combined host/helper memory ceiling remain open. Runtime 173 is isolated and
  source-backed-up; installed runtime 157 remains unchanged. The scoped baseline
  stays at 100%, not full-web compatibility.

- Replaced the normal image-helper launch with a compact native restricted
  launcher. `CreateRestrictedToken(DISABLE_MAX_PRIVILEGE)` leaves at most the
  traversal privilege enabled; the token is set to low integrity RID 4096.
  Extended startup inherits only stdin/stdout/stderr, creates the helper hidden
  and suspended, assigns the existing one-process/memory Job, and resumes only
  after that boundary is active. The helper then permanently enables strict-
  handle checks, disables legacy extension points, enables supported image-load
  and child-process policies, and sends a required handshake before receiving
  encoded bytes. Compact `Irt/Ili/Isp/Ihl/Img/Ipk` aliases expose each launch
  guard and Job-reported peak process bytes. Live tests inspect the actual child
  token, integrity SID, Job membership, and mitigation policies; an unguarded
  fake worker fails before native admission. Five codecs still reuse one PID,
  timeout/restart/cap and abrupt-host cleanup pass, and the real Firespider host
  reports all 15 current guard bits with 8,540,160 private/Job-peak bytes under
  its 192 MiB cap. Full picture/core/editor/WinForms, compiled adapter, and
  benchmark suites pass, with 69 probes over 66 fixtures, all 42 Boot cases in
  one combined run, and six real EXE startup scenarios. Those windows stayed
  responsive; the 35-second recovery timer produced no startup cast dialog,
  orphan helper, or MSHTML. Required runtime files total 536.0 KiB with a 228.8
  KiB GZip estimate; medians are 521.959 ms clean-process, 3.323 ms local-load,
  and 33.453 ms layout/paint, not a speedup claim. The helper is not yet an
  AppContainer and retains user read/network identity; custom ACLs, network
  denial, startup ASLR, CPU-rate limits, combined host/helper peak memory,
  image cookie/cache sharing, and full modern-web behavior remain open. See
  `IMAGE_ISOLATION.md`. Runtime 172 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added mandatory Windows Job Object ownership around `Lea.Image.exe` before
  request bytes are sent. The Job permits one process, enforces a configurable
  192 MiB default hard memory ceiling, exits on unhandled exceptions, and kills
  the helper when its browser handle closes. Compact `Ij/Ih/Ipm` aliases expose
  last Job assignment, a clamped 64-512 MiB cap, and last sampled private bytes;
  changing the cap restarts the shared worker. Five codecs reuse one PID, cap
  bounds/restart pass, and an abruptly terminated child test host proves that a
  helper blocked for 60 seconds is removed by Job close. The real host sampled
  8,466,432 helper private bytes under its cap. This milestone also replaces
  silent native copy/cut success with bounded Lea-owned clipboard commits: busy
  clipboard operations return 0 while preserving text, selection, and undo, and
  cut mutates only after a successful write. Deterministic reader callbacks now
  cover navigation/value/selection/disposal races without clipboard-manager
  prefetch. Full picture, core, repeated isolated editor, compiled host, and
  benchmark suites pass, with 69 probes across 66 fixtures, all 42 Boot cases in
  one combined run, and six actual EXE launch scenarios. Those windows stayed
  responsive; no startup error dialog, orphan helper, or MSHTML was observed.
  Required runtime files total 525.5 KiB with a 224.0 KiB GZip estimate; current
  medians are 477.096 ms clean-process, 3.156 ms local-load, and 31.199 ms
  layout/paint, not a speedup claim. Restricted-token/AppContainer isolation,
  process mitigations, peak whole-process memory, image cookie/cache sharing,
  and full modern-web behavior remain open. See `IMAGE_ISOLATION.md` and
  `NATIVE_EDITING.md`. Runtime 171 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not
  full-web compatibility.

- Moved accepted PNG/JPEG/GIF/BMP/ICO native parsing out of Firespider into the
  required 7 KiB `Lea.Image.exe` helper. All views share one reusable serialized
  worker; the compact `Ie/Ii/Iw/Ik` aliases expose its path, availability, last
  PID, and cancellation kills. The bounded pipe returns validated raw 32-bit
  pixels, missing/malformed helpers remain nonfatal, and a deadline, Stop,
  replacement, or disposal can kill exact native work and restart cleanly.
  Full picture, core, isolated editor, compiled host, and benchmark suites pass,
  with 69 probes across 66 fixtures, all 42 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those windows stayed responsive; no startup
  error dialog, orphan image helper, or MSHTML was observed, including deferred
  runtime resume. Required runtime files total 521.5 KiB with a 222.2 KiB GZip
  estimate; current medians are 486.317 ms for a clean diagnostic process,
  3.299 ms for local fixture loads, and 33.184 ms for layout/paint, not a speedup
  claim. Restricted-token/Job/AppContainer isolation, measured whole-process
  memory, image cookie/cache sharing, and full modern-web behavior remain open.
  Earlier intermittent fragment and recovery-cast observations remain
  unconfirmed. See `IMAGE_ISOLATION.md`. Runtime 170 is isolated and
  source-backed-up; installed runtime 157 remains unchanged. The scoped baseline
  stays at 100%, not full-web compatibility.

- Added bounded PNG/JPEG/GIF/BMP/ICO structural preflight before GDI+, moved
  validation/copying to a worker task, and added atomic process-wide Lea image
  reservations. Compact `Ip/Im/It/In/If` aliases expose the per-image pixel cap,
  configurable 128 MiB shared limit, current reserved/retained estimate, native
  attempts, and active failure sentinels. The existing 32 MiB per-view `Ic/Ib`
  budget remains. Hostile dimensions and bounded JPEG headers stop with zero
  native admission; multi-view exhaustion recovers after disposal or Reload.
  Full picture, core, isolated editor, compiled host, and benchmark suites pass,
  with 69 probes across 66 fixtures, all 42 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those EXE windows stayed responsive; no
  startup error dialog or MSHTML was observed, including deferred-tab resume.
  Engine DLLs total 509.5 KiB with a 216.9 KiB GZip estimate; current medians are
  489.765 ms for a clean diagnostic process, 3.285 ms for local fixture loads,
  and 31.072 ms for layout/paint, not a speedup claim. Killable helper-process
  codecs, measured whole-process memory, image cookie/cache sharing, and full
  modern-web behavior remain open. Earlier intermittent fragment, recovery-cast,
  and editor/focus observations remain unconfirmed. See `IMAGE_PREFLIGHT.md`.
  Runtime 169 is isolated and source-backed-up; installed runtime 157 remains
  unchanged. The scoped baseline stays at 100%, not full-web compatibility.

- Preserved exact cache resource identity after URI canonicalization. Memory and
  persistent entries, SHA-256 filenames, stored-key validation, validators,
  linked CSS, same-document fragments, and navigation now compare path/query
  text ordinally while fragment variants share one representation. Older
  case-mismatched disk collisions are rejected rather than reused or migrated.
  Full cache, core, isolated editor, compiled host, and benchmark suites pass,
  with 68 probes
  across 65 fixtures, all 41 Boot cases in one combined run, and six actual EXE
  launch scenarios. Those EXE windows stayed responsive; no startup error dialog
  or MSHTML was observed, including deferred-tab resume. Engine DLLs remain
  exactly 500.0 KiB with a 212.9 KiB GZip estimate; current medians are 491.286
  ms for a clean diagnostic process, 3.251 ms for local fixture loads, and 30.497
  ms for layout/paint, not a speedup claim. Native decoder peaks, process-wide
  memory, image cookie/cache sharing, `Vary`, and full modern-web behavior remain
  open. Earlier intermittent fragment, recovery-cast, and editor/focus
  observations remain unconfirmed. See `CACHE_IDENTITY.md`. Runtime 168 is
  isolated and source-backed-up; installed runtime 157 remains unchanged. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added bounded streaming reads for local/HTTP documents, GET and POST responses,
  and linked stylesheets. Compact `Hm/Csm` aliases expose the 4 MiB document and
  256 KiB CSS ceilings. Known-length, unknown-length, gzip, and deflate input is
  checked on both wire and decoded bytes; one captured `Tm` budget covers the
  document and all optional sheets. Stop, overlap, cache reuse, local-file/UNC
  origins, typed large/timeout/decode/cancel failures, retained-page recovery,
  and exact-limit acceptance are covered. A split lifecycle startup test now
  waits for the form's async ready state before invoking its comparison command.
  Final input, core, isolated editor, compiled host, and benchmark suites pass,
  with 67 probes across 64 fixtures, all 40 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those EXE windows stayed responsive; no
  startup error dialog or MSHTML was observed, including deferred-tab resume.
  Engine DLLs total 500.0 KiB with a 212.9 KiB GZip estimate; current medians are
  792.480 ms for a clean diagnostic process, 4.159 ms for local fixture loads,
  and 31.697 ms for layout/paint, not a speedup claim. Native decoder peaks,
  process-wide memory, HTTP cache URL case handling, image cookie/cache sharing,
  and full modern-web behavior remain open. Earlier intermittent fragment,
  recovery-cast, and editor/focus observations remain unconfirmed. See
  `INPUT_LIMITS.md`. Runtime 167 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added bounded streaming picture reads, a shared asynchronous pass deadline,
  cancellation with late-resource disposal, and a 32 MiB retained bitmap budget.
  Compact `Ic/Ib` counters expose retained images and estimated pixel bytes;
  case-sensitive cache keys, failed-image Reload retry, and local-file origin
  checks prevent stale reuse and repaint/refetch loops. Stalled images no longer
  hold successful document completion and Firespider request cleanup indefinitely.
  Final picture, core, isolated editor, and compiled host suites pass, along with
  66 probes across 63 fixtures, all 39 Boot cases in one combined run, and six
  actual EXE launch scenarios. No startup dialogs or MSHTML were observed in
  those EXE scenarios, including deferred-tab resume. DLLs total 491.5 KiB
  (208.3 KiB GZip estimate), not a speedup claim. Native decoder peaks and
  document/CSS input buffering still need limits; earlier intermittent fragment,
  recovery-cast, and editor/focus observations remain unconfirmed. See
  `PICTURE_LIMITS.md`. Runtime 166 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Fixed reproduced async view-readiness and old-image cache races with compact
  `Cn` admission, request ownership through presentation, and coordinated picture
  passes. Completion/failure callbacks can start successors without old cleanup
  resetting their cursor or dispatching stale page commands. Firespider now owns
  navigation/retry/reload bookkeeping by request identity; page-command successors
  retain pending targets and preview CSS, while overlapping input is harmless.
  Core, isolated native editing, and full compiled host suites pass, plus all
  65 probes across 62 fixtures. All 38 Boot cases passed in one combined run,
  and six real EXE scenarios passed with responsive windows, deferred resume,
  no observed startup dialogs, and no MSHTML. DLLs total 484 KiB (204.8 KiB GZip
  estimate), not a claim of increased execution speed. Test waits now distinguish
  title/data commitment from view completion. Earlier intermittent fragment,
  recovery-cast, and editor/clipboard/focus issues remain unconfirmed. See
  `LOAD_LIFECYCLE.md`. Runtime 165 is isolated and source-backed-up; installed
  runtime 157 is unchanged. The scoped baseline stays at 100%, not full-web support.

- Added compact `Ss/Sd/Sn` named native CSS with byte/rule limits, atomic
  replacement, retained documents/fields/history, and live media/font updates.
  Firespider now applies matching saved CSS and startup/timer previews without
  a legacy DOM. Fixed a reproduced preview queue surviving failed requests;
  missing records and closed/paused views are guarded. Core, isolated native
  editing, and compiled host suites pass; all 64 probes pass across 61 fixtures.
  Engine DLLs total 485 KiB. All 37 Boot cases passed across combined/focused
  runs, but not in one clean combined run: an editor password Copy/Cut assertion
  failed, followed by a toolbar-focus assertion on a focused run. Four later
  isolated runtime 164 editor runs and three runtime 163 comparison runs passed.
  Those early assertions did not distinguish clipboard changes from field
  changes; added diagnostics do, without logging field or clipboard contents.
  Their cause remains unconfirmed, as do the older fragment/recovery issues.
  Six real EXE launch scenarios pass, including deferred-tab resume, with
  responsive windows and no MSHTML or startup error dialogs observed.
  See `NATIVE_STYLESHEETS.md`. Runtime 164 is isolated and source-backed-up;
  installed runtime 157 remains unchanged. The scoped baseline stays at 100%,
  not a percentage of complete modern-web support.

- Fixed reproduced selected-text loss when Windows clipboard access fails.
  Compact `V.Pa(text)` shares bounded native insertion, one-level undo, Unicode
  limits, target validation, and callback protection with guarded clipboard
  paste. Native menu/message/keyboard routes share the fix, and mouse-edited
  fields beyond the keyboard-list limit now accept edit commands. Also fixed
  a reproduced Firespider saved-style automation startup/timer legacy cast;
  unsupported preview leaves Lea unchanged and later initialization continues.
  The release core and compiled host suites, 36 Boot cases, six real EXE launch
  scenarios, and all 63 probes across 60 fixtures pass. Engine DLLs total
  482 KiB. An intermittent initial-fragment failure recurred earlier in this
  milestone and remains open with before/after-paint diagnostics. The earlier
  stackless recovery observation cannot be conclusively attributed to the new
  confirmed style cast. See `PASTE_SAFETY.md` and `STYLE_STARTUP_SAFETY.md`.
  Runtime 163 is isolated; the installed runtime 157 remains unchanged.

- Added compact `E.Ud/V.Ud` retained-field queries and Lea-aware optional
  Firespider hibernation. Eligible idle renderers are disposed and collectible;
  private/protected/edited/busy/error tabs, cookies, and non-restorable live state are
  kept. The real 30-second timer, close cleanup, URL retention, and fresh-view
  resume are covered. All 62 compatibility probes pass across 60 fixtures;
  core-only and compiled host suites pass, with 12 separate editor passes and
  300 focused fragment navigation cycles. Earlier intermittent paste/clipboard
  failures and one initial-fragment failure remain unexplained; diagnostics and
  independent test modes are retained. The 34-case Boot suite includes two
  hibernation cases. The separate recovery cast is still open. See
  `HIBERNATION.md`; the scoped baseline percentage is unchanged.

- Added compact native document-failure state/notification `V.Le/Fe` and
  explicit GET retry `V.Cr/Ry`, with retained pages/edits/history, cancellation
  cleanup, UI-thread notification, and no POST replay. Fixed reproduced
  unhandled HTTP failures from keyboard navigation and editor access keys;
  keys are consumed before awaiting the network. Firespider keeps selected-tab
  errors visible, preserves failed first-page addresses/sessions, and retries
  through Reload. Split/test hosts also report failures. The Boot suite covers
  32 cases and all 61 compatibility probes pass across 59 local fixtures.
  This is separate from the still-unidentified recovery cast. See
  `LOAD_FAILURES.md`; the scoped baseline percentage is unchanged.

- Installed compact `V.Ng` policy on ordinary Firespider Lea tabs for blocked
  sites, navigation filters, offline/locked state, and live ownership. Hidden
  and private tabs retain policy; denied Resume/address/search/Home preserve
  suspended targets and placeholders.
  Fixed reproduced unhandled keyboard navigation denials and handled expected
  errors in native accessibility and host history/initial-load callbacks.
  Thirty Boot cases and sixty compatibility probes pass, including blocked
  saved-URL startup with an allowed sibling. The intermittent recovery cast
  remains unidentified; see `NAVIGATION_POLICY.md` for explicit policy limits.

- Added compact `V.Ce/Ec` native field commands for copy, cut, paste,
  select-all, delete, undo, and redo. Firespider's real Edit menu no longer
  casts Lea views to WebBrowser; private/read-only fields, retained menu
  selection, toolbar focus, paused/empty tabs, and legacy fallback are covered.
  Fixed shared-font disposal while switching fields and retained native undo
  for Delete. The Boot suite has 28 cases, including a 35-second recovery-timer
  observation; the compatibility runner has 59 probes. The separate
  intermittent recovery cast remains unresolved. See `NATIVE_EDITING.md`.

- Replaced split view's legacy controls with independent Lea panes, retained
  private source flags, asynchronous loading, synchronized scrolling, bounded
  source lists, and explicit disposal/cancellation. Text comparison stays in
  memory with bounded read-only results. Compact `E.Ng/V.Ng` guards explicit
  page navigation without changing retained state on rejection. Three Boot
  cases bring the suite to 27; the guard adds a 58th compatibility probe. See
  `SPLIT_VIEW.md` for tests, policy boundaries, an unresolved intermittent
  recovery-time legacy cast, and remaining DOM migration work.

- Migrated same-domain reload, close, bookmark, duplicate, next-tab, new-window,
  copy, and snapshot commands to Lea-aware paths. Matching includes active,
  paused, and mixed-engine visible tabs; hidden tabs stay untouched. Copies
  preserve privacy and fields, new windows retain the first URL without a
  default request, and exports are bounded by rows and UTF-8 bytes. Fixed
  snapshot cancellation falling through to a write. Two new Boot cases bring
  the suite to 24. See `DOMAIN_COMMANDS.md` for evidence and remaining scope.

- Migrated shared bulk/workspace/protected-session opening, last/recent closed
  windows, and unread watch batches to Lea. Fixed reproduced first-tab loss and
  a closed-window legacy cast. Explicit opens retain existing forms; seeded new
  windows avoid default requests and honor private/lazy state. Bounded snapshots
  include hidden/paused Lea URLs while excluding private ones. Watch batches
  retain skipped unread records and case-sensitive paths. Three new Boot cases
  bring the suite to 22. A step-scheduled disposal fixture replaces a reproduced
  test race. See `BULK_OPENING.md` for verification and remaining migration scope.

- Added bounded Lea-native lazy session restoration under the existing off,
  background, and all policies. Deferred tabs create no renderer or page request
  until resumed. Explicit sessions preserve existing fields; identity switches
  replace old mixed/hidden/private contexts and menus. Private deferred sessions,
  batch limits, eager clipboard behavior, accurate restore counts, and status
  retention are covered. All 19 Boot cases, six real EXE startup scenarios,
  full engine/host suites, and 57 compatibility probes passed. See
  `LAZY_RESTORE.md` for remaining legacy routes and startup-report limitations.

- Added Lea-native tab/group/overview suspension and same-tab fresh-view resume,
  pending-load cancellation, hidden/paused automatic sessions, private-state
  retention, and paused close/duplicate routes. Restored the hidden Reload menu.
  Local/CSS/image continuations and callback disposal are guarded. Native UI
  Automation teardown fixes a reproduced retained-view regression; a weak-
  reference test now confirms collection. Sixteen Boot cases, full engine and
  host suites, 57 compatibility probes, and four real EXE startup cases passed.
  The newly reported startup popup was not reproduced. The prior timer cast
  remains guarded and tested. See `SUSPENSION.md` for unsaved-state and migration
  limits; archive size is not a runtime-speed claim.

- Migrated saved/default Home, previous-session reopening, both left-close
  variants, tab-list selection, and clipboard/site-root navigation to Lea-aware
  paths. Enabled the hidden Home menu, preserved existing tabs during explicit
  restore, retained ports/IPv6 while dropping root-URL credentials, and bounded
  clipboard text and session scanning. Private explicit opens and mixed-engine
  disposal have compiled-host coverage. Fourteen boot cases, the full engine
  and host suites, and four real EXE startup scenarios passed. The launcher now
  retains process exit codes under Windows PowerShell. See
  `NAVIGATION_COMMANDS.md` for limits and unautomated clipboard OS I/O.

- Migrated common Firespider duplicate, close/reopen, bookmark, reload-all,
  and named-session commands to engine-aware paths. Exposed the hidden Close
  Tab command, retained legacy duplication and tab sizing, and replaced its
  last-tab abrupt termination with normal cleanup. Mixed bulk closes dispose
  controls and prune retained state. Navigation recognizes absolute file URLs
  before search heuristics. Recent history and automatic Lea session saves
  exclude private Lea URLs; explicit private named-session opening works.
  Named opens preserve existing tabs/records, enforce Lea restore bounds, and
  report skipped URLs. Clear Saved Session now includes the Lea file with
  locked-file preservation. Twelve boot cases, including real command/shortcut
  checks and a mandatory completion marker, plus full engine/host and four EXE
  startup suites passed. See `TAB_SESSION_COMMANDS.md` for coverage and limits.

- Profiled and reduced Firespider's startup menu-layout cost. Root and category
  dropdowns now batch layout with Finally cleanup, retaining the same 552-item
  command tree. Five alternating fresh-process samples reduced median
  Form.Show from 14.062 to 1.683 seconds (88.0% less time), and root layout
  events from 1,075 to 8. Eleven form-start cases include nested-menu bounds,
  resumed layout, and forced category-build failure cleanup; the full engine
  and compiled host suites and four standalone EXE startup scenarios passed.
  Opt-in, bounded phase timing performs no file I/O; EXE tests now report
  readiness while still observing for delayed dialogs. This changes host
  startup, not engine DLLs or web-platform compatibility. See
  `STARTUP_PERFORMANCE.md` for measurements and limits.

- Moved normal Firespider first-page startup, toolbar New Tab, and Help onto
  Lea. One bounded restore path prioritizes accepted crash recovery, Lea
  sessions, and legacy URLs converted to Lea tabs; it selects the first page,
  avoids duplicate restoration in later windows, and honors private startup.
  Requests can remain pending while the window opens, and recovery snapshots
  now retain Lea URLs. Real EXE tests also exposed and fixed the legacy .NET
  reader's rejection of compact settings XML, preserving all 89 values, and
  session-write failures skipping normal shutdown. Ten form-start cases,
  four standalone startup cases, eighteen event/session regressions, full
  engine/host suites, and all 57 compatibility probes passed. Default startup
  exposes a Lea document without loading MSHTML. Existing host initialization
  still takes roughly 11-19 seconds in the form harness; profiling that cost
  remains next work, not a claimed speedup. See `LEA_STARTUP.md`.

- Hardened Firespider startup and mixed-engine events. Regression tests first
  reproduced Lea-to-WebBrowser casts in three legacy callbacks, invalid
  selection-event casts, a missing-tab timer fault, background progress/lock
  misrouting, and session-file failures. Callbacks now validate their owning
  tab, selected-tab chrome stays isolated, selection handlers accept EventArgs,
  and empty-tab navigation is harmless. Session I/O is bounded to 64 KiB,
  twelve URLs, and 4,096 characters per URL, with readable status on I/O errors.
  Eighteen focused host checks, the full engine/host suites, all 57 compatibility
  probes, and clean/restored/locked/oversized executable startup checks passed.
  This milestone changes host reliability, not the supported HTML/CSS subset
  or engine speed. See `STARTUP_RELIABILITY.md` for reproduction and limits.

- Added bounded non-inherited CSS text overflow through compact `S.To=c/e`
  clip/ellipsis state. Strict values, explicit inheritance, resets, invalid
  preservation, variables, important tiers, hover, and responsive rules feed
  paint-only text-element truncation. Original line sizes, authored text, and
  pointer targets remain intact, including corrected collapsed link spaces.
  Synthetic markers, first-character clipping, mixed inline runs, explicit
  and unbreakable lines, tables, automation prefixes, and retained form state
  have engine, renderer, benchmark, and compiled Firespider coverage. The suite
  now has 58 fixtures and 57 executable compatibility probes.

- Added bounded inherited CSS letter and word spacing through compact
  `S.Ls/Wg` signed -32..128 pixel state. Strict normal/reset parsing,
  parent-restoring inheritance, invalid preservation, variables, important
  priority, inline overrides, hover, and responsive rules share the cascade.
  Measurement and paint share text-element metrics, omit line-edge tracking,
  average inline boundary gaps, preserve word-only shaping, and retain the
  zero-spacing fast path. Blocks, multiline content, table cells, button sizing,
  unfocused controls, authored values, native editing, and automation rectangles
  have fixture-backed coverage. Full engine, renderer, benchmark, and compiled
  Firespider smokes bring the suite to 57 fixtures and 56 executable probes.

- Added bounded inherited CSS `text-transform` through compact `S.Tt=n/u/l/c`
  state and shared `E.Tx` presentation handling. Strict none, uppercase,
  lowercase, and capitalize parsing supports variables, important tiers,
  CSS-wide inherit/reset behavior, malformed preservation, inline overrides,
  multiline text, table cells, semantic values, buttons, placeholders, select
  labels, unfocused fields, dynamic hover, and responsive rules. Authored DOM
  text, mutable field and option values, submitted form data, and focused native
  editor text retain their original casing. UI Automation keeps logical source
  text and maps transformed visual segments case-insensitively. Exact engine,
  renderer, automation, editor, resize, benchmark, and compiled Firespider
  coverage bring the suite to 56 fixtures and 55 executable probes.

- Added bounded inherited CSS pointer interaction through compact `S.Cu/Pe`.
  Automatic/default, pointer, text, wait/progress, crosshair, move, help,
  not-allowed, and horizontal/vertical resize cursor families map to ten small
  states; `pointer-events:auto/none` controls one boolean mouse gate. Variables,
  important tiers, CSS-wide inheritance/resets, invalid preservation, dynamic
  hover, and responsive rules share the compiled cascade. Retained boxes,
  links, summaries, fields, fitted image maps, and linked SVG shapes use one
  z-ordered gate with eligible underlying fallthrough. Paint, geometry,
  keyboard focus, access keys, and accessibility bounds remain available.
  Exact cursor, hover, hit, click, field, focus, responsive, benchmark, and
  compiled Firespider coverage bring the suite to 55 fixtures and 54 executable
  probes.

- Added bounded CSS outlines through compact non-inherited `S.Ow/Os/Oc/Oo`
  width, style, color, and signed-offset aliases. Atomic shorthand and
  longhands accept solid, dashed, dotted, and automatic focus styles; widths
  cap at 32 pixels and offsets at -64..64. Named, hex, `rgb()`/`rgba()`, alpha,
  `currentColor`, variables, important tiers, CSS-wide resets, malformed
  preservation, responsive rules, and dynamic `:focus` share the existing
  cascade. Rounded boxes, images, fields, retained nested owners, opacity, and
  ancestor overflow share one paint path while layout, hits, accessibility
  bounds, and following flow remain unchanged. Exact pixels, field retention,
  resize, benchmark, and compiled Firespider coverage bring the suite to 54
  fixtures and 53 executable probes.

- Added bounded CSS `aspect-ratio` sizing through compact non-inherited
  `S.Rw/Rh/Ra` preferred-width, preferred-height, and intrinsic-auto aliases.
  Atomic parsing accepts one positive number or a two-part ratio, optional
  `auto`, variables, important tiers, CSS-wide resets, malformed preservation,
  and responsive media rebuilds under strict numeric limits. One shared sizing
  path covers content/border boxes, normal and nested blocks, text minimums,
  raster and vector intrinsic fallback, fields, flex bases, grid tracks, and
  table minimums while explicit dimensions and min/max clamps remain decisive.
  Exact geometry, pixels, resize, benchmark, and compiled Firespider host
  coverage bring the suite to 53 fixtures and 52 executable probes.

- Added bounded replaced-image fitting through compact non-inherited
  `S.Of=f/c/v/n/s` fill, contain, cover, none, and scale-down state plus
  `Ox/Oy/Oxp/Oyp` pixel-or-percentage position aliases. Atomic variables,
  important tiers, resets, malformed preservation, and responsive media rules
  feed one 8,192-pixel-capped object rectangle. Raster paint clips cover/none
  overflow to the element content box without changing layout. Image-map
  pointer, keyboard, and accessibility geometry shares that positioned object,
  excludes contain/none letterboxes, and clips cover crops. Exact pixels,
  cache reuse, narrow repaint, benchmark, and compiled Firespider host coverage
  bring the suite to 52 fixtures and 51 executable probes.

- Added bounded CSS bitmap backgrounds through compact `S.Bi` and
  `Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` aliases. One atomic image layer accepts
  bounded `data:`, local file, HTTP, or HTTPS URLs; both-axis, horizontal,
  vertical, or no-repeat modes; pixel/percentage positions and sizes;
  `auto`, `cover`, and `contain`; custom-property and linked-stylesheet origins;
  important tiers, resets, malformed preservation, and noninheritance. A shared
  24-entry, 4 MB-per-image cache feeds page, rounded box, and ordinary-field
  paint with effective opacity, clipping, stable layout/hits, responsive
  restyling, exact pixels, benchmark, and Firespider host coverage. The suite
  now has 51 fixtures and 50 executable probes.

- Added bounded CSS background gradients through compact `S.Gi`,
  `Gi.K/A/X/Y/Ss`, and `Cs.C/P` aliases. One atomic linear or radial layer
  accepts up to eight normalized stops, integer angles or direction keywords,
  centered or percentage radial origins, named/hex/`rgb()`/`rgba()` colors,
  `currentColor`, variables, important tiers, resets, and noninheritance.
  Malformed or over-limit declarations preserve the previous computed image.
  Rounded page, box, and ordinary-field painting shares effective opacity,
  overflow clips, stable layout and hit geometry, resize, exact pixel tests,
  benchmark, and Firespider host coverage. The suite now has 50 fixtures and 49
  executable probes.

- Added bounded CSS box shadows through compact `S.Sw` lists and `Sw.X/Y/B/R/C/I`
  aliases for offsets, blur, spread, color, and inset state. Atomic comma lists
  accept up to four shadows with signed pixel offsets/spread, nonnegative blur,
  named/hex/`rgb()`/`rgba()`/`currentColor`, custom properties, important tiers,
  resets, and noninheritance. Offsets/spread cap at 512 pixels, blur at 128, and
  soft paint at 12 concentric bands. Rounded outer and inset shadows, alpha,
  ordinary fields, unchanged layout/link hits, narrow repaint, exact pixels,
  benchmark, and Firespider host coverage bring the suite to 49 fixtures and 48
  executable probes.

- Added bounded circular and elliptical CSS corner radii through compact
  `S.R0/R1/R2/R3` horizontal and `Q0/Q1/Q2/Q3` vertical aliases. Atomic
  one-to-four-value `border-radius` lists, optional slash-separated vertical
  lists, four corner longhands, pixels, percentages, unitless compact pixels,
  custom properties, important tiers, and reset values feed one normalized
  GDI+ path. Opposing radii scale to fit; pixels cap at 2,000 and percentages
  at 1,000. Rounded backgrounds, solid/dashed/dotted borders, inherited overflow
  clips, ordinary form fields, links, narrow reflow, exact pixels, benchmark,
  and Firespider host coverage bring the suite to 48 fixtures and 47 executable
  probes.

- Added bounded SVG winding and stroke styles through compact
  `Vs.Fr/Lc/Lj/Ml/Da/Do` aliases. Presentation, specificity-ordered stylesheet,
  inline, custom-property, and important declarations now share one inherited
  vector paint state for nonzero/evenodd fill and clip rules, butt/round/square
  caps, miter/round/bevel joins, miter limits, and dash arrays/offsets. Odd dash
  lists repeat to an even list; `none` clears them; malformed values retain the
  inherited state; and 16-value, 256-character, 2,000-unit, and 64-miter ceilings
  keep parsing and GDI+ paint bounded. Region clip unions, painted holes, stroke
  bounds, linked hits, exact pixels, benchmark, and Firespider host coverage
  bring the suite to 47 fixtures and 46 executable probes.

- Added bounded SVG reusable geometry and clip paths through compact `Vg.Cs`,
  `Vs.Cp/Cl/Cm`, `Vc`, and computed `S.Cp` aliases. Local `<use>` expands
  shapes, groups, symbols, nested uses, `x/y`, transforms, and inherited paint
  under four-link, 64-source, and 128-result ceilings with cycle rejection.
  User-space and object-bounds clip paths accept primitive/path/text geometry,
  transforms, multi-shape unions, reusable children, presentation/stylesheet/
  inline attachment, and inherited group contexts. Paint and linked hit bounds
  share the transformed clip, while missing or empty clips fail closed. Exact
  model, matrix, paint, hit, benchmark, and Firespider host coverage bring the
  suite to 46 fixtures and 45 executable probes.

- Added bounded SVG gradient paint servers through compact `Vg.Ds`, `Vs.Fd/Sd`,
  `Vd`, and `Vt` aliases. Linear and radial gradients support object-bounds and
  user-space coordinates, percentages, radial focus, local `href` inheritance,
  `gradientTransform`, presentation/stylesheet/inline stop color and opacity,
  `currentColor`, transformed fills and strokes, and linked-shape hits. Parsing
  is capped at 16 definitions, 16 normalized stops each, and four inheritance
  hops; duplicate IDs are deterministic while unsupported spread modes,
  unresolved references, malformed coordinates, and cycles fail closed. Exact
  model, opacity, color, paint, link, benchmark, and Firespider host coverage
  bring the suite to 45 fixtures and 44 executable probes.

- Extended bounded inline SVG with compact `Vg.A/M` aspect-ratio aliases,
  cumulative `Vs.M` transform matrices, and canonical curve controls in
  `Vp.A/B/C/D`. The engine now handles matrix, translate, scale, centered
  rotate, skewX/skewY lists; `none`, aligned `meet`, and aligned `slice`
  viewBox mapping; and absolute/relative `C/S/Q/T/A` geometry. Smooth controls
  resolve during parsing and each elliptical arc becomes at most four cubic
  steps, retaining the existing coordinate, path, transform, depth, and shape
  ceilings. Exact matrix/control models, transformed paint and links, clipping,
  invalid fallback, benchmark, and Firespider host coverage bring the suite to
  44 fixtures and 43 executable probes.

- Added bounded inline SVG through compact `B.Vg`, `Vg`, `Vs`, and `Vp`
  aliases. Intrinsic sizing, default meet `viewBox` scaling, groups, links,
  inherited presentation/inline fill, stroke, opacity, `currentColor`, six
  basic shapes, text, and `M/L/H/V/Z` straight paths share Lea's existing
  layout, clipping, paint, hit, fragment, and graphic-accessibility paths.
  Eight-level, 128-shape/point, 256-text, finite-coordinate limits, invalid
  rejection, exact paint, benchmark, and Firespider host coverage bring the
  suite to 43 fixtures and 42 executable probes.

- Added bounded CSS Grid content alignment. Compact `Al/Jc=s/c/e/b/a/v` state
  distributes fixed track groups on the block/inline axes using start, center,
  end, between, around, or evenly modes. Atomic one/two-value `place-content`,
  resets, invalid preservation, definite-height row offsets, authored gaps,
  exact geometry, paint, hits, following flow, benchmark, and Firespider host
  coverage bring the suite to 42 fixtures and 41 executable probes.

- Added bounded CSS Grid item alignment. Compact `Ji/Js` inline-axis state joins
  `Ai/As` block-axis state for stretch/start/center/end container defaults and
  item overrides. Atomic `place-items`/`place-self`, margin-box placement,
  measured simple-text auto widths, resolved-row vertical alignment, reset and
  invalid preservation, exact geometry, paint, hits, following flow, benchmark,
  and Firespider host coverage bring the suite to 41 fixtures and 40 executable
  probes.

- Added bounded atomic CSS Grid shorthands. `grid-template` compiles explicit
  rows/columns and optionally sized quoted areas while preserving automatic
  state. `grid` adds row/column `auto-flow` and `dense` forms, resets all owned
  explicit/implicit longhands but not gaps, and accepts `auto` in explicit track
  lists through `S.Av`. Scratch parsing prevents malformed sides, areas, tracks,
  flow markers, or extra slashes from partially changing a prior layout. Exact
  aliases, reset boundaries, dense geometry, paint, hits, following flow,
  benchmark, and Firespider host coverage bring the suite to 40 fixtures and 39
  executable probes.

- Added bounded repeating CSS Grid implicit-track sizing through `S.Av` and
  `S.Gac/Gav/Rac/Rav`. One-to-eight-entry pixel, auto, integer-fr, and minmax
  patterns cycle after explicit tracks, size area-created columns, honor
  definite endpoints and largest spans, and settle authored pixel rows in
  auto-height grids. Atomic reset/invalid parsing, exact mixed geometry, paint,
  hits, following flow, benchmark, and Firespider host coverage bring the suite
  to 39 fixtures and 38 executable probes.

- Added bounded CSS Grid auto placement through `S.Gf=r/d/c/x`. Sparse and
  dense row/column flow, earlier-hole backfill, equal-share implicit columns,
  definite-first occupancy, real-height visual traversal, and a safe
  non-overlapping eight-column overflow path all reuse the existing renderer.
  Reset/invalid parsing, exact geometry, paint, hits, following flow, benchmark,
  and Firespider host coverage bring the suite to 38 fixtures and 37 executable
  probes.

- Added bounded named CSS Grid lines. Lazy `S.Gnc/Rnc` maps and one-based
  `Gx/Rx(name)` lookup inspect unique explicit names, while item
  `Gn/Ge/Rn/Re` edges support shorthand and longhand placement. Named areas
  generate start/end lines without duplicate storage. Case-sensitive names,
  multiple names per line, outer named repeat groups, mixed numeric/name spans,
  atomic invalid declarations, 8-track/16-name ceilings, exact paint/hit/flow
  geometry, and Firespider host coverage bring the suite to 37 fixtures and 36
  executable probes.

- Added bounded named CSS Grid areas. `S.Aa/Ar/Ac/Ag` inspect a validated
  8x8 template and `S.An` exposes each item's area name. Equal-row validation,
  dot cells, filled rectangles, 16-name and 32-character ceilings,
  definite-first placement, implicit columns, nested inline hits, exact paint
  geometry, a dedicated fixture/probe, and Firespider host coverage bring the
  suite to 36 fixtures and 35 executable probes.

- Added bounded CSS Grid rows with lazy compact `Rd`, public `Rc/Rv`
  inspection, and direct-item `Rl/Rs` placement aliases alongside the existing
  column model. Fixed pixel and integer-fr row templates, repeat/minmax forms,
  row starts/spans, bounded occupancy, two-axis spans, auto-height measurement,
  exact paint/reflow geometry, benchmark, and Firespider host coverage all pass.

- Added bounded `grid`/`inline-grid` columns with a lazy compact `Gd` track
  array, public `Gc/Gv` inspection, and direct-item `Gl/Gs` start/span aliases.
  One to eight fixed pixel or integer-fr tracks, simple repeat/minmax forms,
  pixel gaps, sparse automatic rows, stable order-modified traversal, spans,
  nesting, fields, paint, hits, resize, strict invalid limits, benchmark,
  Inspector, and Firespider host coverage bring the suite to 35 fixtures and 34
  executable probes.

- Added bounded `autofocus` candidates and assigned `accesskey` alternatives
  without consuming unmatched or host-owned shortcuts.
- Added bounded `Ax` semantic accessibility records, compact live state flags,
  rendered bounds, host aliases, and a WinForms/MSAA hierarchy with protected
  values, focus, keyboard shortcuts, and default actions.
- Added bounded `hd/sh`, `ds/en`, `vl`, `ck`, and `fo` state commands, retained
  field ownership while hidden, one-shot `Kp` focus handoff, WinForms focus
  validation, and engine/host/benchmark fixtures.
- Added a native HWND-rooted UI Automation fragment provider over `Ax`, stable
  bounded runtime IDs, native control properties, protected values,
  invoke/value/toggle/selection/disclosure/range/scroll patterns, and
  structure/focus/property notifications; the provider and patterns are
  verified by an MTA UIA client.
- Added `Tb/Tr/Tc`-backed UI Automation grid, table, grid-item, and table-item
  patterns with zero-based coordinates, bounded loaded rows/cells, column spans,
  containing-grid links, row/column header relationships, and public MTA plus
  Firespider host verification.
- Added a bounded read-only UI Automation TextPattern over `P.X` and settled
  layout ownership with document/child/point ranges, character/word/line/document
  movement, search, attributes, rendered bounds, embedded children, scrolling,
  stale-page rejection, protected-value exclusion, and public MTA plus Firespider
  host verification.
- Added bounded visual-line TextPattern snapshots for plain and inline wrapped
  text, visual `TextUnit.Line` expansion/movement, one rectangle per visible
  line, proportional partial-range cropping, and public MTA, benchmark, and
  Firespider host verification.
- Added bounded `op/cl/tg` disclosure commands for a `details` ID or its first
  direct `summary`, synchronized `N.Op`/`open` state, retained nested fields,
  strict invalid-target rejection, and engine, WinForms, benchmark, plus
  Firespider verification.
- Added bounded single-line flex rows with compact `Fd/Jc/Ai/Gp` computed-style
  aliases, `flex`/`inline-flex`, row reversal, fixed and equal-share auto slots,
  pixel gaps, distribution and cross-axis alignment, direct inline-item model
  ownership, nested content, hit testing, painting, and fixture-backed reflow.
- Extended that flex subset with compact `Wr/Rg/Fg/Fb` state, bounded wrapping,
  two-value and separate row/column gaps, pixel basis, common `flex` shorthand,
  integer weighted growth, row reversal per line, exact geometry, narrow reflow,
  benchmark coverage, and Firespider host verification.
- Added compact `Sh` state and bounded integer `flex-shrink`, corrected shorthand
  shrink semantics, clamped flex bases through `Wx/Wn`, distributed deficits by
  outer-basis weight, froze items at explicit pixel minimums, retained overflow
  when locked floors cannot fit, and verified exact desktop/narrow geometry plus
  benchmark and Firespider host models.
- Added compact `Or` state with stable -64..64 ordering for nowrap rows and
  fixed-height columns, extended `Fd` with `c/y` column aliases, added bounded
  vertical grow/shrink, column-reverse flow, main/cross-axis alignment,
  `Hn/Hx` protection, and natural auto-height row gaps, then verified parser,
  exact geometry, paint, hits, following flow, resize, benchmark, and Firespider
  host paths.
- Added compact `As` item alignment plus `Jc=a/v` around/evenly distribution,
  shared the integer spacing helper across row and column axes, allowed direct
  stretch/start/center/end overrides without changing container defaults, and
  verified exact remainder geometry, stretching, narrow reflow, painting, link
  hits, benchmark probes, and the Firespider host model.
- Added two-argument `bg/fg/fs` overloads for ID-scoped background, foreground,
  and font-size changes while retaining the one-argument body forms, existing
  color and 8..96px bounds, safe target filtering, 4KB inline-style ceiling,
  field-preserving CSS rebuild, click execution, WinForms, benchmark, and
  Firespider host verification.
- Added non-inherited compact `S.Bz=c/b` state for `content-box` and
  `border-box`, applying one bounded conversion to pixel width, height, min/max,
  and flex-basis values across blocks, fields, images, flex items, tables, and
  cells. Exact wide/narrow geometry, cascade rejection, paint, hit,
  accessibility bounds, benchmark, and Firespider host coverage verify it.
- Added an opt-in gzip persistent representation cache with compact
  `Cd/Cx/Cb/Dc/Db/Cdl` aliases, SHA-256 URL filenames, same-directory atomic
  replacement, 32-entry/16 MB defaults, 128-entry/128 MB hard ceilings, LRU
  timestamps, cross-engine fresh hits and validator-backed `304` reuse. Private
  sessions perform no persistent reads or writes; `no-store`, reload, malformed
  files, entry/byte trimming, unrelated host files, bounded diagnostics,
  WinForms forwarding, benchmark loopback probes, and Firespider Inspector
  coverage are verified.
- Added a bounded responsive CSS compiler for `screen`/`all`, `min-width`,
  `max-width`, exact `width`, comma alternatives, nested `@media`, and
  `<style media>`. Compact `E/V/P/G.Vw` viewport aliases, 1-8,192 pixel bounds,
  four-level nesting, eight alternatives, 1,024 compiled rules, active-rule
  diagnostics, field-preserving boundary rebuilds, WinForms resize forwarding,
  fixtures, benchmark probes, and Firespider host coverage are verified.
- Added compiled declaration records plus bounded inherited CSS custom properties
  and `var()` fallback. Compact `S.Vc/Cv` and `G.Vc` aliases expose counts and
  case-sensitive lookup; lazy copy-on-write storage keeps ordinary styles on the
  low-allocation path. The engine caps each style at 32 properties, 64-character
  names, 512-character raw values, eight nested resolutions, 1,024 expanded
  characters, and 128 declarations per block. Missing, cyclic, oversized, and
  excess values fail into authored fallbacks while inline, hover, media,
  WinForms, benchmark, Inspector, and Firespider paths preserve live fields.
- Added one compiled important-priority bit and a bounded two-tier cascade for
  every supported ordinary and custom property. A final top-level literal
  `!important` marker is stripped while quoted/function text stays intact;
  stylesheet and inline priority, specificity, source order, shorthand and
  longhand interaction, inherited variables, hover, media, and field retention
  are verified. Compact `G.Ic` diagnostics and both Inspectors report the active
  stylesheet count. The compatibility suite now covers 30 fixtures and 29
  executable probes.
- Added bounded `:is(...)`, `:where(...)`, and selector-list `:not(...)` to the
  shared stylesheet/query compiler. Each function accepts eight compound
  alternatives, each simple selector permits sixteen authored functional tests,
  and nesting/inner combinators remain excluded. `:is`/`:not` use maximum
  argument specificity, `:where` uses zero, forgiving lists discard unsupported
  alternatives, and `:not` stays strict. Engine, query, dynamic state, WinForms,
  benchmark, Inspector, and Firespider host coverage bring the suite to 31
  fixtures and 30 executable probes.
- Added precompiled `An+B` arithmetic plus `:nth-last-child` and
  `:nth-last-of-type` to the existing child/type position matcher. Positive,
  negative, zero, unit, odd/even, fixed, forward, and reverse forms share
  compact `A/B` integers, 32-character expressions, and absolute 8,192-value
  bounds. Specificity, selector-list composition, invalid syntax, document
  queries, WinForms geometry, benchmark, Inspector, and Firespider coverage
  bring the suite to 32 fixtures and 31 executable probes.
- Added bounded `of S` filtering to `:nth-child` and `:nth-last-child`. Strict
  compound lists compile into the existing selector array, count only matching
  inclusive siblings in forward/reverse order, add maximum filter specificity,
  and share the eight-alternative/sixteen-test limits. Compound attributes and
  pseudo states, dynamic rebuilds, retained fields, invalid lists, nesting,
  typed misuse, stylesheet/query parity, WinForms geometry, benchmark,
  Inspector, and Firespider coverage bring the suite to 33 fixtures and 32
  executable probes.
- Added strict bounded `:has(...)` relative lists to the shared stylesheet/query
  compiler. Compact records cover implicit descendants plus leading child,
  adjacent, and following relationships with four-part chains, eight
  alternatives, sixteen total parts, maximum argument specificity, and a 4,096
  candidate-visit ceiling. Dynamic descendant focus rebuilds preserve fields;
  malformed lists, nested selector functions, pseudo-elements, oversized
  chains, and excess work fail closed. Engine, query, WinForms, benchmark,
  Inspector, and Firespider coverage bring the suite to 34 fixtures and 33
  executable probes.

## Next gains

- Diagnose the fresh-path isolated script-worker startup deadline with
  launch/handshake timing evidence. Passing warm/source suites do not prove it
  is resolved; retain the current deadlines and native protections while measuring.
- Add a native-safe non-empty visible-range bridge outside the .NET Framework
  managed provider array boundary; retain the stable empty fallback until range
  handles survive both in-process and external MTA clients.
- Validate the separate .NET Framework 4.8 candidate before adopting it in the
  live project. Keep original source, a rollback build and the installed browser
  intact; remaining legacy-only commands still need Lea implementations.
- Extend the bounded timer and microtask implementation with full classic-string
  semantics, animation frames and error/rejection notification, then expand DOM operations
  and real-site compatibility coverage.
- Implement document-origin and resource/network policy before enabling remote
  or external scripts. Local scripting tests do not prove safe internet-page
  execution; keep that gate closed until the boundaries are tested.
- Measure cold/warm script and page-loading costs in controlled runs. Keep the
  JavaScript module optional and do not equate compressed size with runtime speed.
