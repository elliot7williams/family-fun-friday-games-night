# LeaForge

Latest lifecycle fix: native view navigation no longer dispatches automatic
`load` after failed initial scripts, preventing implicit realm replacement,
inline-block replay and loss of the initial failure. The identical-binary
old-runtime control reproduces the defect; all 200 controlled and eight native
checks pass, as do the renderer smoke and separate copied Firespider script
case. The ordinary suite instead stops after 445 checks at another fresh-worker
startup timeout; that failed gate is retained without retry. Four fresh captures
are inspected. Required bundle size stays 747,520 bytes. No startup-timeout fix,
speedup or release is claimed; readiness remains about 65%. See
[LOAD_FAILURE_GATE.md](LOAD_FAILURE_GATE.md).

Preceding startup diagnostic correction: bounded failure messages retain their
parent summary and newest worker trace without a second prefix truncation.
Parent init/read/verification and worker hello-write entry/completion now have
distinct checkpoints. Both old-runtime controls reproduce the defects; all 67
focused checks, 1,876 ordinary scripting checks and the renderer smoke pass.
The single observed Firespider script case instead fails split-pane loading
with two retained startup timeouts after parsed. Fresh main/attribute captures
are inspected. No intermittent-startup fix, speedup or release is claimed;
readiness remains about 65%. See [HANDSHAKE_REPORTS.md](HANDSHAKE_REPORTS.md).

Latest browser functionality: elements gain descendant `querySelector` and
`querySelectorAll`, with native `N.Q/Qa` aliases. The same final test binary
fails on the preceding runtime, then passes 74 focused checks and four native
view checks on the addition. Renderer and actual standalone-browser smokes
pass. The broad scripting run instead fails after 161 checks at native startup;
the observed Firespider case fails later during split recovery, after completed
DOM bootstrap. Both failed gates and fresh captures are preserved. Required
bundle size is 747,520 bytes. No startup fix, speedup or new release is claimed;
readiness remains about 65%. See [SUBTREE_QUERIES.md](SUBTREE_QUERIES.md).

Preceding diagnostic correction: both linked launchers retain the newest bounded
worker stderr instead of its prefix, decoding UTF-8 across fragmented reads.
All 12 regressions pass; the identical test binary fails on the old runtime.
The candidate passes all 1,784 ordinary scripting checks, the renderer smoke
and the standalone browser EXE smoke. The separately copied Firespider script
case instead fails at startup after parsed, with wall 5,045ms and CPU 734ms.
That failure and an earlier baseline timeout remain preserved without retry.
Binary sizes and protections stay unchanged; no startup fix, speedup or release
is claimed. Readiness remains about 65%. See
[WORKER_STDERR_TAIL.md](WORKER_STDERR_TAIL.md).

Preceding diagnostic correction: startup CPU readings survive exact worker exit
and managed Process disposal through a retained native handle. Missing readings
say unavailable, not zero. The final old-runtime control and 17 focused checks
prove the reporting change; seven report and 18 launch-failure checks pass.
The broad run passes all 1,755 existing checks, then fails the new CPU fixture.
Its failed gate stays retained and unresolved; final test source adds raw
counter logging without weakening assertions. Required bundle size is now
746,496 bytes. No speedup, native-startup fix or release is claimed; readiness
remains about 65%. See [NATIVE_CPU_LIFETIME.md](NATIVE_CPU_LIFETIME.md).

Latest scripting change: private wrapper/cache and lookup operations capture
their intrinsics instead of invoking page overrides. The preceding runtime
fails the new regression; all 36 scope and four resource checks pass on the fix
with the same test binary. The full ordinary gate fails on a 5,018-ms native
startup timeout during timer recovery and is retained without retry. Independent
browser/Firespider script checks pass, but do not replace the failed gate.
Binary sizes, protections and installed files stay unchanged. No stable release
or startup fix is claimed; readiness stays about 65%. See
[DOM_WRAPPER_CAPTURES.md](DOM_WRAPPER_CAPTURES.md).

Preceding startup change: the worker page-frame loop has a separate non-inlined
post-handshake boundary. All 15 worker-entry, 27 native comparison and 1,747
ordinary scripting checks pass, along with four matched observed Firespider
script cases and the actual standalone browser lifecycle smoke. Only the worker
changes; production sizes and protections stay unchanged. Timings overlap and
both versions pass, so the earlier intermittent startup failure is not declared
fixed. No release is claimed; milestone 146 stays open and readiness remains
about 65%. See [DEFERRED_WORKER_FRAMES.md](DEFERRED_WORKER_FRAMES.md).

Latest diagnostic: `-so` / `--selector-observe` extends the existing sampler to
direct synchronous worker sessions. Contract, guard, controlled failure,
selector and native-view checks pass, together with all 1,746 ordinary unobserved
scripting checks. A separate observed Firespider script
case captures two split-worker startup timeouts after parsing; its failed
result and counter/thread data are retained without retry. Production bytes,
limits and the installed browser stay unchanged. No startup fix or release is
claimed; milestone 146 remains open and readiness stays about 65%. See
[SELECTOR_WORKER_OBSERVATION.md](SELECTOR_WORKER_OBSERVATION.md).

Latest reliability change: live readonly attributes now synchronize the retained
native editor through JavaScript and compact `V.Sa/Da` aliases. Final source
verification passes 336 focused and 1,727 full scripting checks, full native
`--core`, editing/clipboard and the actual standalone browser EXE lifecycle.
Separate v33 passes all 43 ordinary host cases with the exact retained final
harness and three intended old-runtime negative controls, plus six actual-EXE
startup scenarios. The failed v32 and
placement-only source revision remain preserved. Only the Win DLL grows 512
bytes; the required bundle is 745,984 bytes. No installed replacement, new
release, startup-timeout fix or speedup is claimed. Milestone 146 stays open;
readiness remains about 65%. See [LIVE_READONLY.md](LIVE_READONLY.md) and
[HOST_ATTRIBUTE_EDITS.md](HOST_ATTRIBUTE_EDITS.md).

Preceding reliability change: immediate rejection of an invalid image-handshake
prefix, with separate bounded `hello/guards` diagnostics. Focused and final full
`--core` checks pass, as does the actual standalone browser EXE lifecycle check.
The complete scripting gate fails during isolated selector-limit recovery at
the unchanged startup deadline (5,017ms); that failure is retained without a
retry. No stable release, installed Firespider replacement, startup-delay fix or
speedup is claimed. Readiness remains about 65%; see [IMAGE_HELLO.md](IMAGE_HELLO.md).

Generic bounded attribute edits through `E.Sa/Ra`, `V.Sa/Da` and JavaScript
`setAttribute/removeAttribute` remain available. Their preceding checkpoint
passed 322 focused and 1,713 full scripting checks. Its initial disk-full and
later image-handshake failures are retained in [ATTRIBUTE_EDITS.md](ATTRIBUTE_EDITS.md),
not replaced by narrower or later passing checks.

LeaForge is a new, bigger browser-engine experiment for Lea.

The earlier projects stay small:

- `Lea.Micro` is the compressed tiny version.
- `LeaBrowser` is the simple test window.
- `LeaPlus` adds more HTML/CSS/script tests.

LeaForge is where we can grow the more full-featured browser.

Preceding diagnostic checkpoint: `-bp` compares direct, parsed and analyzed
bootstrap initialization with identical embedded bytes. All six final planned
samples/18 grouped checks and three input guards pass; timings overlap, so no
production optimization is adopted. The ordinary scripting suite completes
1,391 checks once with the final tool and exact preceding production. Its outer
summary-text mismatch is preserved and corrected through the same live process
handle, without rerunning. No browser release or startup fix is claimed;
milestone 146 stays open and readiness stays about 65%. See
[BOOTSTRAP_PREPARATION.md](BOOTSTRAP_PREPARATION.md).

Preceding host checkpoint: separate v31 integrates the exact tested event-constructor
dependencies. The same final host/harness rejects the preceding runtime at its
constructor-prototype assertion, then the current runtime passes all 43 ordinary
host cases and six actual-EXE startup scenarios. Both script realms check methods,
bound methods and handler accessors; fresh main/split captures are inspected.
Installed Firespider stays untouched. No startup fix, package, independent rebuild
gate or new full source-suite run is claimed. Milestone 146 stays open and broader
readiness stays about 65%. See [HOST_EVENT_CONSTRUCTORS.md](HOST_EVENT_CONSTRUCTORS.md).

Preceding source checkpoint: shared event operations are now non-constructible
methods, retaining their receiver semantics and descriptors. The identical final
isolated harness rejects the preceding runtime at its constructor-prototype
assertion and passes all 25 checks on the current runtime. The ordinary scripting
suite passes all 1,391 checks; the rebuilt standalone browser EXE smoke passes.
Compact bootstrap grows 66 bytes, with no speedup or startup fix claimed.
A separate unchanged-runtime observation passes but captures no timeout; the
earlier independent release failure remains failed. No new Firespider host
candidate or installed replacement is made. Milestone 146 stays open and
readiness stays about 65%. See [EVENT_CONSTRUCTORS.md](EVENT_CONSTRUCTORS.md).

Previous package checkpoint: the corrected v30 candidate and fresh runtime
extraction each pass all 43 host cases with the exact same checksum-verified
harness. The extraction also passes six actual-EXE startup scenarios. Both ZIPs
pass integrity/extraction checks and the archived host source builds unchanged,
but its independent runtime fails first-demand worker startup at 5,012 ms
against the unchanged 5,000 ms deadline. Its later runtime gates are held; no
retry, verified release, startup fix or installed replacement is claimed.
Milestone 146 stays open and readiness stays about 65%. See
[HOST_PACKAGE_ELEMENT_RECEIVERS.md](HOST_PACKAGE_ELEMENT_RECEIVERS.md).

Previous host checkpoint: separate v30 carries the verified Element receiver
runtime into Firespider. The first harness passes 42 non-script cases, then
fails later script recovery because its field fixture was left renamed. The
corrected final harness passes the complete script case; an identical-harness
old-runtime control fails at the expected Element receiver assertion. All six
actual-EXE startup scenarios pass. No final single 43-case matrix pass, startup
fix, release package or installed replacement is claimed. Milestone 146 stays
open and readiness stays about 65%. See
[HOST_ELEMENT_RECEIVERS.md](HOST_ELEMENT_RECEIVERS.md).

Previous source checkpoint: attribute methods and class-list accessors now use
their receiving element, with shared per-realm functions and private per-element
token caches. All 43 focused and 1,385 full-source checks pass on final revision
B, together with the standalone browser EXE smoke. Earlier revision A startup
failures and the isolated old-runtime receiver failure are retained. Compact
bytes grow 811; no speedup or startup fix is claimed. No new host candidate or
installed replacement is made; milestone 146 remains open and readiness stays
about 65%. See [ELEMENT_RECEIVERS.md](ELEMENT_RECEIVERS.md).

Previous package checkpoint: v28's freshly extracted runtime passes all 43 host
cases and six actual-EXE startup scenarios without startup observation or timing
trace. The archived host source builds independently and passes the complete
script case plus six startup scenarios; all source inputs and 19 production
dependencies match their audits. Both ZIPs pass manifest/stream/extraction
checks. These are experimental test packages, not a startup-timeout fix or an
installed replacement; readiness remains about 65%. See
[HOST_PACKAGE_RECEIVERS.md](HOST_PACKAGE_RECEIVERS.md).

Previous host checkpoint: separate v28 integrates the tested event receiver
bindings and passes all 43 host cases, with only scripting observed, plus all
six actual-EXE startup scenarios. Both new realm checks and borrowed native
dispatch pass. The identical-harness preceding-runtime control fails at the
expected invalid-receiver assertion. Whole-script observation captures eight
workers with no loss/errors, but no timeout occurs or is fixed. No new release
package or installed replacement is made; readiness remains about 65%.
See [HOST_EVENT_RECEIVERS.md](HOST_EVENT_RECEIVERS.md).

Previous source checkpoint: event methods and handler accessors now bind to the
receiving target, sharing their functions across targets while retaining the
existing bounded event API. All 75 focused and 1,342 full-source checks, six
fresh-worker launches/27 comparison checks, and the actual test-browser EXE
smoke pass. The old-runtime control times out in trusted bootstrap before its
receiver assertion and is inconclusive. The compact resource grows 458 bytes;
no size reduction, general speedup, or startup fix is claimed. Installed
Firespider is unchanged, milestone 146 remains open, and readiness stays about
65%. See [EVENT_RECEIVERS.md](EVENT_RECEIVERS.md).

Previous diagnostic checkpoint: optional whole-script observation now follows
navigation, error recovery and split panes. Fair rotation captures eight
workers with no dropped requests/samples; the script case passes. Twelve
reporting/scheduling contracts, 14 argument guards, three controlled failures
and the ordinary clean case pass. No timeout is captured or fixed; production
bytes and installed Firespider are unchanged, and readiness remains about 65%.
See [SCRIPT_FLOW_OBSERVE.md](SCRIPT_FLOW_OBSERVE.md).

Previous host checkpoint: separate v26 carries the compact exception table into
Firespider and passes all 42 non-script cases plus both new realm/descriptor
checks. Its script case later fails isolated-worker startup after an intentional
script error; the first-demand observer does not cover that failed launch.
The matrix is not passed, no retry is made, and further release gates are held.
Installed Firespider stays untouched, milestone 146 stays open, and broader
readiness remains about 65%. See [HOST_EXCEPTION_TABLE.md](HOST_EXCEPTION_TABLE.md).

Previous source checkpoint: the DOM exception constants use one private name
table and bounded descriptor loop. The compact bootstrap shrinks by 475 bytes;
each scripting PE shrinks by 512 bytes. All 105 focused checks, 1,267 full-source
checks, six fresh-worker launches/27 comparison checks, and the standalone
browser EXE smoke pass. Startup ranges overlap, so no speedup or intermittent
timeout fix is claimed. No new Firespider candidate or installed replacement
is produced; readiness stays about 65%. See
[COMPACT_EXCEPTION_TABLE.md](COMPACT_EXCEPTION_TABLE.md).

Previous diagnostic checkpoint: one shared, opt-in worker observer now serves
the native-view and Firespider harnesses, with cheap 125 ms counters and thread
snapshots paced at least one second apart. Native recovery passes 60 checks;
14 input/output guards and two controlled failure-preservation checks pass.
A fresh diagnostic copy passes all 43 host cases with only its first script
demand observed. This does not erase the preceding extraction timeout or prove
a startup fix. Production binaries and installed Firespider stay unchanged;
readiness remains about 65%. See [DEMAND_OBSERVE.md](DEMAND_OBSERVE.md).

Previous host checkpoint: separate v24 passes all 43 candidate host cases and
six actual-EXE startup scenarios, including new real-click callback coverage
with an identical-harness old-worker negative control. Both archives pass
integrity audits, but their fresh runtime extraction fails first-demand worker
startup after 42 non-script cases. The archived source builds independently;
further runtime gates are held. The packages are not verified releases and
installed Firespider stays untouched. See
[HOST_LISTENER_CALLS.md](HOST_LISTENER_CALLS.md).

Preceding source verification: the opt-in native worker observer passes all 60 checks
and captures 266 samples across eight workers. A separate ordinary full source
run, without observation or timing trace, passes all 1,261 checks through the
same final test executable. No timeout is reproduced; measured observer
overhead prevents treating its run as a speed benchmark. Production files and
installed Firespider stay unchanged. Startup reliability remains unresolved;
no new release or host candidate is made. See
[STARTUP_OBSERVE.md](STARTUP_OBSERVE.md).

Previous startup investigation: a diagnostic-only alternating L:/C: comparison passes
all six final launches and 25 checks with identical runtime bytes and unchanged
limits. Startup ranges overlap; relocation is not a demonstrated timeout fix.
Earlier ACL-audit aborts are retained. No production binary changes or new
release are made. See [STARTUP_VOLUMES.md](STARTUP_VOLUMES.md).

Development note: event callbacks now use captured intrinsic invocation, with
all 90 focused trusted/isolated/native checks and the SDK-output browser smoke
passing. At that checkpoint, both complete source runs failed worker startup
before the new group. Release packaging and a new host candidate were held;
the v23 host below
retains the preceding bootstrap. See [LISTENER_CALLS.md](LISTENER_CALLS.md).

Previous host checkpoint: separate v23's second complete attempt and fresh runtime
extraction each pass all 43 host cases and six actual-EXE startup scenarios.
Its archived source builds independently and passes scripting plus six startup
scenarios. Both archives pass input/stream/extraction audits. The initial
disk-full failure remains preserved; these are functional test passes, not a
cold-start fix or stable release. See
[HOST_WORKER_VERIFICATION.md](HOST_WORKER_VERIFICATION.md).
The latest worker separates its guarded entry from interpreter
binding, fixing a reproduced missing-dependency entry failure. All 14 new checks,
full 1,171-check source/package runs, and the standalone EXE smoke pass. A paired
comparison still fails one preserved old-worker startup, so the intermittent
deadline remains unresolved. See [WORKER_ENTRY.md](WORKER_ENTRY.md).
Separate v22 remains the preceding verified Firespider candidate with its old
worker: 43 host cases and six EXE startup scenarios pass on its candidate and
extraction, with an independently verified source rebuild. Those passes do not
transfer to the latest worker. See [HOST_EVENT_OPTIONS.md](HOST_EVENT_OPTIONS.md).
The installed browser stays unchanged; milestone 146 is open and broader
readiness is about 65%.
The checkpoint notes below preserve earlier results rather than transferring
later passes to those historical builds.

Active development remains in the recovery checkout `L:/Lea Development`.
The earlier recovery-checkpoint full source run passes 1,040 checks there, and bootstrap profiling now
correctly fails if any sample failed. The original checkout and installed host
remain intact; this is not a cold-start fix. See
[WORKSPACE_RECOVERY.md](WORKSPACE_RECOVERY.md) for verification boundaries.

The earlier separate v18 candidate passes all six actual-EXE startup scenarios
and the paste case. Its edit assertions pass, but settings restoration fails on
full C:; scripting still times out during bootstrap. Clipboard fixture errors
now retain both test/restoration failures and their phases, with 14 deterministic
checks before host cases. No current full host/package pass or installed-browser
replacement is claimed. See [CLIPBOARD_FIXTURES.md](CLIPBOARD_FIXTURES.md).

The newer source and standalone build add live element parent/child/sibling
navigation, with six shared getters and two-letter bridge commands. All 53
focused checks and the 1,021-check full suite pass; earlier startup/profile
failures remain retained. The full pass follows observed C: storage recovery,
not a proven startup fix. This feature is not in the earlier v18 host candidate.
See [TREE_NAVIGATION.md](TREE_NAVIGATION.md) for scope and remaining release gates.

Separate v19 now includes tree navigation and passes six EXE startup scenarios
and 42 non-script host cases. Its corrected script case passes separately;
the initial full host run fails an incorrect new fixture expectation. Expanded
source tests pass all 55 tree checks within a later incomplete full run, but
worker startup still times out, so the planned 1,023-check suite and a complete
corrected 43-case host run remain unpassed. See
[HOST_TREE_NAVIGATION.md](HOST_TREE_NAVIGATION.md). No new release is installed.

The current source and standalone runtime also reuse one bounded parent I/O
dispatcher per active script execution, retiring it between executions without
discarding a successful worker realm. Final focused, native-worker, and full
runs pass 31, 87, and 1,040 checks. Local read-only scripting benchmarks show
lower warm dispatch overhead, not a general browser or cold-start speed claim.
Separate v19 does not include this change; no new host release is verified.
See [IO_DISPATCH.md](IO_DISPATCH.md) for measurements and release boundaries.

The current startup report also preserves the newest captured worker detail
when its fixed budget requires truncation. Seven formatter, eight native phase,
and 31 dispatcher checks pass. Separate v21 passes all six ordinary EXE startup
scenarios, but its timed scripting case still fails cold bootstrap. The expanded
1,047-check source run fails isolated tree startup too; the earlier 1,040-check
full pass is not transferred to this build. No new browser is installed or
released. See [STARTUP_REPORT_TAIL.md](STARTUP_REPORT_TAIL.md).

## Current Features

- Captured intrinsic event-callback invocation, live object-listener operation
  lookup, and handler ownership unaffected by public `.call` replacement.
  All 90 focused checks pass; full-suite startup remains unresolved. See
  `LISTENER_CALLS.md` for verification and adoption limits.

- A guarded worker entry boundary, argument validation without interpreter
  dependencies, and handled post-mitigation binding failures. Fourteen regressions
  and complete 1,171-check source/package runs pass without changing runtime
  budgets or aliases. See `WORKER_ENTRY.md` for retained startup failures.

- Correct listener argument conversion for the supported JavaScript event subset:
  primitive capture, ordered dictionary reads, duplicate/null conversion, and
  failed-conversion safety. All 75 new checks, complete 1,157-check source/package
  runs, and the separate browser EXE smoke pass at the source checkpoint. Separate
  v22 now passes complete candidate/extracted Firespider host and EXE startup
  matrices, with native option-conversion regression checks and an independently
  verified host-source rebuild. This is not proof of an intermittent startup fix
  or an installed replacement. See `EVENT_OPTIONS.md` and `HOST_EVENT_OPTIONS.md`.

- trusted scripting budget reports with bounded host-phase and exception-type
  detail, unchanged limits, and tested cleanup/recovery. The initial 1,082-check
  run failed fresh-worker startup; see `BUDGET_PHASES.md` for that retained result
- an experimental compact scripting bootstrap generated from retained readable
  source, with AST round trips and build-time stale-file guards; resource size
  is 28.3% lower, not whole-engine size or a proven speedup. Full startup and
  release gates remain open; see `COMPACT_BOOTSTRAP.md`
- bounded per-execution parent script I/O dispatch with no owned idle dispatcher
  between executions, unchanged deadlines, and preserved worker realms; see
  `IO_DISPATCH.md`
- bounded two-term `calc()` thresholds in CSS size-container queries, with
  integer pixel/font/viewport/container resolution and live host feedback; see
  `CALCULATED_CONTAINER_QUERIES.md`
- bounded PNG/JPEG/GIF/BMP/ICO preflight, zero-capability AppContainer
  `Lea.Image.exe` decoding with an identity-scoped executable ACL, verified
  restricted low-integrity token, suspended Job prebinding, explicit inherited
  pipes, process mitigations, shared `Ip/Im/It/In/If` accounting, compact
  `Ie/Ii/Iw/Ik/Ij/Ih/Ipm/Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk/Iac/Ind/Ial`
  sandbox diagnostics, verified content-addressed helper staging for FAT32 and
  other non-ACL volumes, `Icl/Ihm/Icm/Iad` combined host/helper admission, and
  cross-view bitmap accounting; see `IMAGE_PREFLIGHT.md` and
  `IMAGE_ISOLATION.md`
- exact URI-normalized cache identity with case-sensitive path/query resources,
  fragment sharing, validator isolation, and persistent recovery; see
  `CACHE_IDENTITY.md`
- bounded document/POST and linked-CSS input, gzip/deflate expansion limits,
  shared I/O budgets, local-sheet origins, and `Hm/Csm`; see `INPUT_LIMITS.md`
- bounded streaming picture reads, one pass deadline, gzip/deflate images,
  retained bitmap budgets, failed-image retry, and `Ic/Ib`; see `PICTURE_LIMITS.md`
- compact `Cn` navigation admission, owned async view completion, coordinated
  picture handoff, and Firespider callback/reload tracking; see `LOAD_LIFECYCLE.md`
- compact `Ss/Sd/Sn` named native CSS, retained editors, saved Firespider styles,
  and request-scoped startup previews; see `NATIVE_STYLESHEETS.md`
- guarded Firespider saved-style startup/timer callbacks without legacy casts
  or automatic fallback; see `STYLE_STARTUP_SAFETY.md`
- guarded clipboard paste and compact `Pa(text)` host insertion with native
  undo, bounded text, and protected targets; see `PASTE_SAFETY.md`
- compact `Ud` retained-field protection and optional native Firespider
  hibernation for eligible idle tabs; see `HIBERNATION.md`
- compact native load failures and explicit GET retry through `Fe/Le/Cr/Ry`,
  retained tabs/status, and no POST replay; see `LOAD_FAILURES.md`
- tabs
- back and forward history per tab
- reload
- bookmarks
- source viewer
- local HTML file loading
- basic HTTP page loading
- bounded in-memory page cache plus an optional compressed persistent cache for faster repeat navigation across launches
- basic HTML rendering for headings, paragraphs, bounded unordered/decimal lists, details disclosures, links, buttons, images, code, pre blocks, and structured tables
- bounded named CSS counters through compact `S.Cr/Ci/Cs`, defining
  `cr/ci/cs`, tree-order scope, eight active instances/operations,
  32-character ASCII names, and named `counter()`/`counters()` marker output;
  see `CSS_COUNTERS.md`
- page-local `<details>`/`<summary>` state with compact markers, nested retention, pointer activation, and Enter/Space keyboard activation
- bounded quote-aware HTML tokenization with comments, declarations, raw text, RCDATA, entities, and malformed-delimiter recovery
- compact `Id`, `Q`/`Q1`, and read-only bounded `Qa` document queries
- bounded `<base href>` resolution through compact `Bu`
- retained same-page fragments through compact `Ft`/`Fg`/`Rv`, 128-entry history, percent-decoded IDs, named anchors, details reveal, and `Fa/Fj/Fy` first-paint/resize/intrinsic-image placement diagnostics; see `INITIAL_FRAGMENT_STABILITY.md`
- custom `<progress>` and `<meter>` painting through compact bounded `B.Vm` values, meter quality regions, inherited styles, and table-cell reflow
- compact `<output>` and `<data>` values through `P.Dvs`/`B.Dv`/`Ir.Dv`, bounded metadata, safe updates, and inline/table ownership
- compact `<time>`, `<ins>`, and `<del>` metadata through `Dv.K/Dt/U`, safe citation schemes, native decoration, rebuilds, and inline/table ownership
- compact `<map>`/`<area>` models through `P.Mps`/`B.Mp`/`Ar`, bounded shape coordinates, dead-area layering, responsive custom hits, links, and downloads
- bounded keyboard navigation through `Ti` and `Ke/Kn/Ki/Kb/Kf/Ka`, ordered tabindex targets, shared-map regions, visible rings, scrolling, and host activation
- bounded autofocus and host-safe page shortcuts through `Af/Afs`, `Ak/Aks`, and `An/Kl/Aa`, with conflict fallback, resolved-fragment suppression, and `Alt+Shift` activation
- bounded semantic accessibility through `Ax/Axs/St/Is` and `V.Ab/Ao`, with HTML/ARIA roles, labels, hierarchy, protected values, rendered bounds, WinForms/MSAA children, native UI Automation fragments, interaction and `Tb/Tr/Tc` grid/table patterns, plus read-only logical TextPattern ranges with direction-aware visual-line, partial-range, and point geometry
- bounded standards-shaped `CSS.supports()` checks through compact
  `E/V.Su` overloads and literal allow-listed `if/else` command guards, reusing
  the existing declaration/selector/logic parsers and shared command limits;
  see `SUPPORTS_QUERIES.md`
- bounded standards-shaped `@container` custom-property `style()` queries over
  nearest or named ancestors, with computed `S.Cn`, grouped `not`/`and`/`or`,
  nested media/supports/layers, retained rebuilds, and diagnostics; compact
  `S.Ct` plus atomic `E.Cz` measurements add nearest/named width, height,
  reduced-integer aspect-ratio, and portrait/landscape size queries with
   automatic bounded WinForms feedback; fixed-hundredth `px`/`em`/`rem`
   thresholds use selected-container and root font bases, while bounded
   `vw`/`vh`/`vmin`/`vmax` and `cq*` thresholds use live viewport and ancestor
   measurements; scroll-state queries remain unsupported; see `CONTAINER_STYLE_QUERIES.md`,
  `CONTAINER_SIZE_QUERIES.md`, `CONTAINER_RATIO_ORIENTATION.md`, and
   `CONTAINER_QUERY_LENGTHS.md` plus `CONTAINER_QUERY_VIEWPORT.md`
- bounded container-relative width/height dimensions through compact unit codes
  7..12 for `cqw`/`cqh`/`cqi`/`cqb`/`cqmin`/`cqmax`, independent nearest-axis
  selection, viewport fallback, calculated/size-function values, explicit
  computed inheritance, commands, native sheets, and existing three-pass
  feedback; see `CONTAINER_UNITS.md`
- bounded standards-shaped media matching through compact `E/V.Mq` aliases and
  literal global/window `matchMedia(...).matches` guards, reusing current
  viewport, DPI, preferences, whole-query `not`, strict `only`, and shared
  command limits; see `MATCH_MEDIA.md` and `MEDIA_MODIFIERS.md`
- wrapped inline bold/italic text, styled spans, line breaks, and links inside text blocks
- inherited font-family fallbacks plus native current-color underline and
  line-through decoration
- inherited bounded text transformation through compact `S.Tt=n/u/l/c` and
  shared `E.Tx`, with inline/control presentation and raw DOM/form preservation
- inherited signed pixel letter/word spacing through compact `S.Ls/Wg`, with
  shared wrapping, inline boundaries, control paint, and retained geometry
- inherited signed pixel/percentage first-line indentation through compact
  `S.Ti/Tip`, with bounded `ti` commands, LTR/RTL wrapping, and retained
  paint/hit/accessibility geometry; see `TEXT_INDENT.md`
- one inherited bounded text-shadow layer through compact nullable `S.Ts` and
  `ts`, with signed offsets, optional blur/color, lightweight paint, stable
  geometry, and literal DOM support; see `TEXT_SHADOW.md`
- bounded inherited text-decoration line/color/style/thickness through compact
  `S.Td/Tc/Tds/Tw` and `td/tdl/tdc/tds/tdw`, with atomic shorthand and
  longhands, native solid fast path, lightweight double/dotted/dashed/wavy
  paint, stable geometry, and literal DOM support; see `TEXT_DECORATION.md`
- bounded inherited `text-underline-offset` through compact `S.Uo/Uop` and
  `uo`, with signed integer pixels/percentages, shorthand independence,
  offset-aware default-link paint, stable geometry, and literal DOM support;
  see `TEXT_UNDERLINE_OFFSET.md`
- bounded inherited `text-wrap`, `text-wrap-mode`, and `text-wrap-style`
  through compact `S.Wm/Wt` and `tw/twm/tws`, with strict shorthand reset,
  declaration-order `white-space` interaction, same-line-count balancing,
  eight-line/2,048-character work bounds, and literal DOM support; see
  `TEXT_WRAP.md`
- inherited `text-align` shorthand and `text-align-all` through compact
  `S.Ta/Tl` and `ta/taa`, with retained logical start/end state,
  `justify-all`, match-parent, strict shorthand reset, and literal DOM support;
  see `TEXT_ALIGN.md`
- bounded inherited `text-align-last` through compact `S.Tl` and `tl`, with
  auto/physical/logical/justified final and forced-line alignment, horizontal
  LTR/RTL resolution, stable boxes, and literal DOM support; see
  `TEXT_ALIGN_LAST.md`
- non-inherited clip/ellipsis text overflow through compact `S.To=c/e`, with
  bounded LTR/RTL physical inline-end markers, grapheme-safe paint-only
  prefixes, retained link targets, and authored automation text
- bounded non-inherited `line-clamp` and `-webkit-line-clamp` through compact
  `S.Lc=0/1..32` and defining `lc`, with grapheme-safe LTR/RTL markers, reduced
  flow height, hidden hit removal, full logical text, and literal DOM support;
  see `LINE_CLAMP.md`
- bounded inherited `list-style-type` and type/image/position `list-style` through compact
  `S.Lst=n/d/c/s/1/0/a/A/r/R` and defining `lst`, with HTML type/reversed/start/
  value support, bounded bullet/decimal/alpha/Roman formatting, RTL markers,
  no-marker spacing suppression, and literal DOM support; see `LIST_STYLE.md`
- bounded inherited `list-style-position` through compact `S.Lp=o/i`, `lp`, and
  atomic type/image/position `ls`, with true outside logical gutters, inside first-line
  participation, shorthand reset/order, LTR/RTL geometry, and literal DOM
  support; see `LIST_POSITION.md`
- bounded inherited `list-style-image` through compact `S.Li` and `li`, reusing
  Lea's sandboxed image cache/decoder, with atomic three-component shorthand,
  64px intrinsic caps, text fallback, inside/outside LTR/RTL geometry, and
  literal DOM support; see `LIST_IMAGE.md`
- bounded terminal `::marker` styling through compact `B.Ms` plus `S.Mc/Ma/Mn`,
  with isolated text properties, up to eight quoted/`attr()`/implicit
  list-counter/generated-quote fragments, one quoted missing-value attribute fallback, and 128
  generated units, nested paths, image precedence, page/item `ms` and readable
  `markerstyle` commands, and shared LTR/RTL paint; see `MARKER.md`,
  `MARKER_CONTENT.md`, `ATTR_FALLBACKS.md`, `MARKER_COUNTERS.md`, and
  `GENERATED_QUOTES.md`, and `GENERATED_ALT_TEXT.md`
- inherited left/center/right/justify alignment and pixel/relative line spacing
- external stylesheets with document-order cascade and shared bounded caching
- compiled relationship, attribute, structural, link, and form-state selectors,
  plus bounded `:is`/`:where`/list-`:not` alternatives and specificity
- precompiled bounded `An+B` arithmetic for forward/reverse child and type
  positions, with shared stylesheet and `Id/Q/Qa` query behavior
- strict bounded `nth-child(... of S)` and `nth-last-child(... of S)` compound
  filters with matching-sibling counts and maximum filter specificity
- strict bounded `:has(...)` relational lists with descendant, child, adjacent,
  following-sibling, and four-part compiled chains, maximum specificity,
  dynamic focus restyling, and deterministic traversal limits
- bounded responsive `screen`/`all` plus fixed non-matching `print`, whole-query
  `not`, no-op `only` before media types, pixel width/height,
  portrait/landscape, integer aspect-ratio, `dpi`/`dppx`/`dpcm` resolution,
  light/dark `prefers-color-scheme`, and no-preference/reduce
  `prefers-reduced-motion`, no-preference/reduce `prefers-reduced-data`,
  no-preference/less/more/custom `prefers-contrast`,
  none/initial-only/enabled `scripting`, and primary/union
  `pointer`/`hover` interaction media queries with strict/inclusive modern
  ranges, compact `Vw/Vh/Vz/Dp/Th/Rm/Pdt/Pc/Je/Pt/Hv/Ap/Ah/Mi` aliases,
  nested conditions, live WinForms viewport/DPI/preference/device updates, and
  retained fields; see
  `MEDIA_RANGE_SYNTAX.md`, `MEDIA_RESOLUTION.md`, `MEDIA_COLOR_SCHEME.md`,
  `MEDIA_REDUCED_MOTION.md`, `MEDIA_REDUCED_DATA.md`, `MEDIA_CONTRAST.md`,
  `MEDIA_MODIFIERS.md`, `MEDIA_SCRIPTING.md`, and `MEDIA_INTERACTION.md`
- bounded CSS `@supports` declaration checks and `selector(...)` capability
  checks with grouped `not`/`and`/`or`, nested media, compact `@s` diagnostics,
  strict depth/term/text limits, and retained fields; see `SUPPORTS_QUERIES.md`
- bounded named CSS `@layer` blocks with later normal, unlayered normal,
  reversed important, and strongest inline order; reopened/nested names,
  native-sheet merging, conditional nesting, compact `@l` diagnostics, and
  strict name/layer limits; see `CASCADE_LAYERS.md`
- compiled declarations plus bounded inherited CSS custom properties with
  compact `S.Vc/Cv`, nested `var()` fallback, lazy maps, and strict limits
- bounded normal/important cascade for supported declarations, with stylesheet
  and inline tiers, compact `G.Ic` diagnostics, and quoted/function boundaries
- horizontal logical `inline-size`/`block-size` plus min/max and direction-aware
  logical inset longhand/axis shorthands, reusing physical records and geometry;
  short `is/bs` and familiar DOM aliases; see `LOGICAL_SIZING_INSETS.md`
- horizontal direction-aware logical margin/padding axes and longhands with
  signed bounded margins, atomic parsing, twelve compact aliases, familiar DOM
  forms, and shared box geometry; see `LOGICAL_BOX_MODEL.md`
- horizontal direction-aware logical border axes, edges, and width/style/color
  components with atomic physical lists, lazy per-edge records, 24 compact
  aliases, familiar DOM forms, hidden used widths, shared geometry, and mixed
  rounded paint; see `LOGICAL_BORDERS.md`
- horizontal direction-aware logical corner radii mapped onto the existing
  physical ellipse fields, with explicit inheritance/reset, atomic parsing,
  nine compact aliases, readable/familiar DOM forms, authored restore, and
  shared asymmetric paint; see `LOGICAL_RADII.md`
- inherited horizontal `direction: ltr/rtl` through compact `S.Dr=l/r`, HTML
  `dir`, pre-cascade resolution, translated cross-direction logical inheritance,
  `text-align: start/end`, direction-aware row/reverse/wrapped flex and column
  cross alignment, logical grid auto/dense/column/explicit/area placement and
  alignment, right-origin plain/mixed/wrapped/nowrap/justified inline flow,
  nested list/disclosure markers, inline-end ellipsis and TextPattern geometry,
  and compact live `dr` reflow; see
  `DIRECTION.md`
- live `:hover` and `:focus` restyling that preserves mutable form values
- non-inherited `content-box`/`border-box` sizing across pixel dimensions,
  min/max clamps, flex basis, fields, images, tables, and cells
- min/max dimensions plus normal, break-word, and break-all text flow
- inherited visibility and effective opacity across text, boxes, controls, and images
- default HTML `hidden` rendering suppression plus focus, autofocus, and access-key exclusion
- responsive `<picture>`/`srcset` selection and bounded base64 embedded images
- compact CSS cascade for named, hex, RGB/RGBA, bounded legacy/modern
  HSL/HSLA, modern HWB, modern CIE Lab/LCH, predefined sRGB/linear-sRGB/
  Display-P3/linear-Display-P3/A98-RGB/ProPhoto-RGB/Rec.-2020/XYZ `color()`, modern Oklab/OkLCh, and bounded
  two-stop sRGB `color-mix()` colors, fonts,
  boxes, overflow, and inline styles;
  hue formats accept degree/gradian/radian/turn units and normalized wrapping,
  while HWB adds clamped whiteness/blackness and gray normalization, CIE
  Lab/LCH add D50 conversion with Bradford D50-to-D65 adaptation, predefined
  `color()` adds sRGB, Display P3, A98 RGB, ProPhoto RGB, Rec. 2020, and D50/D65 XYZ spaces, and Oklab/OkLCh add
  D65 conversion with bounded local-MINDE sRGB gamut mapping
  across shared HTML/field/SVG paint; see `HSL_COLORS.md`, `HWB_COLORS.md`,
  `LAB_COLORS.md`, `PREDEFINED_COLORS.md`, `WIDE_GAMUT_COLORS.md`,
  `OKLAB_COLORS.md`, and `COLOR_MIX.md`
- bounded rounded outer/inset CSS box shadows through compact `S.Sw` and
  `Sw.X/Y/B/R/C/I`, atomic four-entry lists, RGB/HSL/HWB/Lab/LCH/predefined/Oklab/OkLCh/mixed alpha/current colors, variables,
  priority/reset/bounds, ordinary fields, and a 12-band blur ceiling
- bounded linear/radial CSS background gradients through compact `S.Gi`,
  `Gi.K/A/X/Y/Ss`, and `Cs.C/P`, with one atomic layer, eight normalized stops,
  RGB/HSL/HWB/Lab/LCH/predefined/Oklab/OkLCh/mixed alpha/current colors, variables, priority/reset/bounds, rounded page/box/field
  paint, opacity, clipping, and resize-safe hit geometry
- bounded `flex`/`inline-flex` row and column flow with compact
  `Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or` state, stable order and reversal,
  fixed/equal-share/weighted grow and shrink slots, explicit pixel min/max
  protection, row wrapping, pixel basis and row/column gaps, around/evenly
  distribution, container/item alignment, natural auto-height columns, nested
  content, and shared hit paths
- bounded `grid`/`inline-grid` columns and rows with compact `Gd/Gc/Gv` and
  `Rd/Rc/Rv` tracks plus `Gl/Gs` and `Rl/Rs` item placement, fixed pixel and
  integer `fr` sizes, simple `repeat()`/`minmax(0,Nfr)`, pixel gaps, bounded
  occupancy, two-axis spans, validated named-area templates with compact
  `Aa/Ar/Ac/Ag/An` aliases, unique named lines through `Gnc/Rnc/Gx/Rx` and
  `Gn/Ge/Rn/Re`, generated area edges, `Gf` row/column sparse/dense auto flow,
  repeating implicit track patterns through `S.Av` and `Gac/Gav/Rac/Rav`,
  atomic `grid-template`/`grid` explicit, quoted-area, and row/column auto-flow
  shorthands, compact `Ji/Js/Ai/As` item alignment with atomic
  `place-items`/`place-self`, `Al/Jc` track-group alignment with atomic
  `place-content`, definite-first placement, bounded implicit endpoints, nested
  content, paint, hits, fields, auto-height row measurement, and responsive
  reflow
- auto-height styled semantic containers with nested normal-flow ownership
- nested positioning with explicit z-index, opacity/isolation contexts, settled auto height, and inherited clipping
- bounded vertical sticky positioning through compact `S.Pn=k`, shared retained
  geometry, and nearest-parent clamps; see `STICKY_POSITIONING.md`
- compact inset stretching plus translated block stacking contexts
- nested fields, responsive images, and tables with clip-aware native editing
- bounded multipart file uploads plus range, color, and temporal form fields
- tiny script handling for `alert("...")` and `document.title = "..."`
- bounded engine scripting for title, body colors, font size, and host-routed alerts
- safe button/link commands with bounded variables and ID-scoped text updates
- bounded `ac`/`rc`/`tc` class mutations with compiled CSS restyling
- bounded `hd/sh`, `op/cl/tg`, `ds/en`, `vl`, `ck`, and `fo` element-state commands
- literal `document.getElementById(...)` text/state/style/class/focus translation
- bounded `dp` display mutation and literal host-controlled location translation
- bounded percentage/calculated widths with `wd` and literal `style.width`
- bounded definite percentage/calculated heights with `ht` and literal `style.height`
- bounded `vw`/`vh`/`vmin`/`vmax` dimensions and calculations with compact
  `Wu/Hu/Vh` aliases; see `VIEWPORT_UNITS.md`
- bounded `em`/`rem` dimensions and calculations with compact `Wu/Hu/Rf`
  aliases; see `FONT_RELATIVE_UNITS.md`
- bounded two/three-term `min()`/`max()` and three-term `clamp()` dimensions
  with lazy `Wk/Hk/Wq/Hq/Wv/Hv` aliases; see `SIZE_FUNCTIONS.md`
- bounded developer snapshots plus a tabbed inspector with an Access view in the test browser
- repeatable compatibility, timing, memory, binary-size, and compression report
- streamed local/HTTP downloads with explicit destinations and a 64 MB ceiling
- highlighted, case-insensitive find-in-page with next/previous cycling

## Lea Short Commands

LeaForge accepts compact aliases for common page commands:

```text
t("Title")       title
a("Message")     alert
bg("#fff")       background
fg("#222")       text color
fs(12)           font size
bg("id", "#fff") element background
fg("id", "#222") element text color
fs("id", 18)      element font size
dp("id", "none") element display
wd("id", "50%")  element width
ht("id", "50%")  element height
lst("id", "lower-alpha") element list marker style
lp("id", "inside") element marker position
li("id", "url('marker.png')") element marker image
ls("id", "upper-roman url('marker.png') outside") list style shorthand
dr("rtl")         body direction
dr("id", "rtl")  element direction
mi("id", "8px 12px") inline margin
mb("id", "6px")      block margin
pi("id", "8px 12px") inline padding
pb("id", "6px")      block padding
v("name", "Lea") variable
tx("id", "Hi ${name}") element text
ac("id", "hot")       add class
rc("id", "hot")       remove class
tc("id", "hot")       toggle class
hd("id")              hide element
sh("id")              show element
op("id")              open details
cl("id")              close details
tg("id")              toggle details
ds("id")              disable field
en("id")              enable field
vl("id", "value")     set bounded field value
ck("id", "1")         set or toggle checked state
fo("id")              request rendered focus
go("next.html")        request navigation from the host
be("id", "content:'New '") element before style
af("id", "content:' done'") element after style
be("id", "content:'[!] ' / 'Important '") accessible before text
qu("id", "'<' '>'") element generated quote pairs
```

The longer `alert(...)` and `document.title = ...` forms still work. The short
forms are intended for small, fast Lea pages and are limited to simple values.
Literal `document.getElementById("id")` statements may use `textContent` or
`innerText`, field/state booleans, `focus()`, `classList.add/remove/toggle`, and
the already bounded background/color/font-size/display/width/height/direction
and logical margin/padding/border/radius styles. Lea translates
these familiar forms into the short commands above; it does not expose arbitrary DOM
objects, expressions, `innerHTML`, callbacks, or a general JavaScript runtime.
`go` never navigates the engine directly: it queues one bounded HTTP, HTTPS, or
safe local-file request for the browser host to approve and load.
Literal `location`, `location.href`, and `location.assign(...)` forms use that
same request path. See `DISPLAY_NAVIGATION.md`.
`wd` accepts fixed pixels, percentages, and one bounded `calc(P% +/- Npx)` form;
authored width/min/max values use the same model. See `CALCULATED_WIDTHS.md`.
`ht` applies the same syntax to height/min/max, resolving percentages only when
the containing height is definite. See `CALCULATED_HEIGHTS.md`.
State commands remain ID-scoped and preserve retained field identity through a
single rebuild. Values stop at 1,024 characters, file values cannot be set,
passwords stay redacted, and `E.Kp`/the WinForms view consume only one valid
focus request. Disclosure commands accept a `details` ID or its first direct
`summary` ID and keep `N.Op` plus the authored `open` attribute synchronized.
`be/af` and readable `beforestyle/afterstyle` apply the bounded inline generated
content subset described in `GENERATED_CONTENT.md`. `qu`, readable `quotes`, and
literal `style.quotes` configure the bounded generated-quote pairs described in
`GENERATED_QUOTES.md`. Visual `url(...)` fragments for before, after, and marker
content reuse the sandboxed image path described in `GENERATED_IMAGES.md`.

The .NET Framework test browser opens Lea Inspector with `F12`, `Ctrl+Shift+I`,
or the `I` toolbar button. Its Summary, Nodes, CSS, Requests, and Messages tabs
use the compact `Dg` API; Access uses `V.Axs`. Neither path inspects private
renderer fields.
Firespider's experimental Lea tabs expose the same bounded inspector from the
View menu (`F12`) and the tab context menu, with refresh and request-log clearing.
Firespider's normal New Tab command and `Ctrl+T` now open Lea; **New Legacy Tab**
(`Ctrl+Alt+L`) keeps the original engine available for unmigrated features.

## Compact Engine API

Small C# programs can use the short engine facade:

```csharp
var e = new LeaForge.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
Console.WriteLine(p.N + " nodes, " + p.Ls + " links");
```

The main actions are `Go`, `Bk`, `Fw`, `Rl`, and `At`. Page values are shortened
to `U` (response URL), `Bu` (document base URL), `T` (title), `H` (HTML), `N`
(node count), and `Ls` (link count).

Alias naming is a core Lea rule. See [ALIASES.md](ALIASES.md) for the vocabulary
and limits used by future engine features.

Normal navigation reuses bounded parsed or decoded representations. `E` and `V`
stay memory-only until a host sets `Cd`; `Cx`/`Cb` bound persistent entries and
bytes, `Dc`/`Db` report its footprint, and `Cdl()` clears only Lea cache files.
Path and query casing remain exact while fragments share one representation; see
`CACHE_IDENTITY.md`. The Reload button bypasses both cache layers and fetches the
current page again.

## Run

Open `../Lea.sln` in Visual Studio after `LeaForge` is added to it, or compile the fallback executable from `Lea.Build`.

The compiled test exe is expected at:

```text
../dist/LeaExes/LeaForge.exe
```

After a Release solution build, regenerate the engine report with:

```powershell
..\Lea.Bench\bin\Release\Lea.Bench.exe -r .. -o COMPATIBILITY_PERFORMANCE.md -n 15
```

The report marks unsupported browser-platform features explicitly; a passing
fixture probe is not presented as full Chrome, Safari, Firefox, or HTML Living
Standard compatibility.

## Local Test Page

Open this path in LeaForge:

```text
LeaForge/test-pages/index.html
```

## Important

LeaForge is still our own experimental renderer. It does not use Chromium, Edge WebView, or a full JavaScript VM.
Lea.Engine now includes only a small allow-listed JavaScript-shaped command
bridge; it does not evaluate arbitrary JavaScript.
