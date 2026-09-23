# LeaForge Codex Workbench

This is the new Codex work area for continuing LeaForge.

The full staged roadmap is in [ENGINE_PLAN.md](ENGINE_PLAN.md).
Visual Studio embedding details are in [INTEGRATION.md](INTEGRATION.md).
Weighted completion is tracked in [PROGRESS.md](PROGRESS.md).

## Goal

Build Lea into a more complete, self-owned browser engine while keeping the tiny Lea spirit:

- understandable source
- fast startup
- separate experiments for big features
- no Chromium/WebView dependency for the custom renderer path

## Next Build Targets

Milestone 123 adds bounded predefined CSS `color()` to Lea's shared color
validator and WinForms painter. Numeric/percentage channels and optional alpha
cover sRGB, linear sRGB, Display P3, linear Display P3, XYZ D50/D65, and the
`xyz` D65 alias. CSS Color 4 transfer curves and matrices, shared Bradford
D50-to-D65 adaptation, and bounded local-MINDE sRGB gamut mapping reuse the
existing compact authored-color records. Text, backgrounds, borders, outlines,
decorations, shadows, gradients, generated content, variables, layers, media,
fields, sRGB mix stops, SVG paint, and declaration `@supports` need no new
alias or dependency. Comma forms, `none`, relative or calculated channels,
custom profiles, A98 RGB, ProPhoto RGB, Rec. 2020, and overlong input remain
outside the slice. All 119 benchmark probes pass over 114 fixtures. Focused and
full source/staged suites and the compiled Firespider host are green. All 42
Boot cases and all six real EXE startup scenarios remain responsive without
MSHTML, an invalid cast, JIT dialog, or dead startup window. Three-round
diagnostic medians are 611.914 ms clean start, 6.768 ms local load, and 45.787
ms layout/paint; retained memory is 69.7 KiB/page and the required runtime is
652.5 KiB. Runtime 223 is packaged separately and installed runtime 157 remains
unchanged. See `PREDEFINED_COLORS.md`.
Container/style queries,
`font-tech()`/`font-format()`,
general-enclosed conditions, JavaScript `CSS.supports()`, locale-sensitive
automatic quote selection, typed/unquoted/functional attribute fallbacks,
pseudo box-model/flex/grid items, generated image functions and sizing,
arbitrary CSSOM, reversed/custom counters, vertical writing, full Unicode bidi,
and general JavaScript evaluation remain outside this slice.

Milestone 60 installs the existing `V.Ng` hook on main Lea tabs, enforcing host
rules, offline/locked state, and live ownership. Denied resume retains its
placeholder, including blocked address/search/Home commands. Later requests
do not reuse its one-time initial approval.
Expected native keyboard/accessibility denials no longer become unhandled
callbacks, while public API errors stay visible. Two new Boot cases bring the
suite to 30; the runner has 60 compatibility probes across 59 fixtures.
Runtime 160 remains isolated and root runtime 157 is unchanged. See
`NAVIGATION_POLICY.md`. Next investigate the intermittent recovery cast and
continue redirect/resource/download policy, native error pages, printing,
properties, toolbar editing, popup handling, DOM tools, and hibernation.

Milestone 59 adds `V.Ce/Ec` with two-letter native field commands and migrates
Firespider's seven Edit menu commands. It fixes the reproduced `j.c9` Lea cast,
an equal-font disposal exception, and non-undoable deletion. Tests cover real
menu/hover dispatch, toolbar-focus isolation, protected fields, stale values,
and disposal. There are 28 Boot cases and 59 compatibility probes; recovery
now pumps events for 35 seconds with its real snapshot timer enabled. Twenty
additional runtime-158 recovery repeats passed without identifying the earlier
intermittent callback. Runtime 159 remains an isolated test build; live root
runtime 157 is unchanged. See `NATIVE_EDITING.md`. Next continue investigation
of that callback and migrate remaining printing/properties/toolbar-edit routes,
general navigation policy, pop-ups, DOM tools, and automatic hibernation.

Milestone 58 makes split view Lea-native: independent private contexts,
asynchronous loads, synchronized scrolling, bounded in-memory comparisons, and
owner-close cancellation/disposal. `E.Ng/V.Ng` adds a compact optional document
navigation guard; split view uses it for existing host rules. Three Boot cases
bring the suite to 27, with 58 compatibility probes. See `SPLIT_VIEW.md` and
`Split-view-host.patch`. One recovery run reported an unidentified legacy cast;
nine isolated repeats passed, and Boot now retains full exception stacks. Keep
that issue open. Next diagnose the callback and address general Lea navigation
policy, pop-ups, redirect/resource policy, DOM tools, and automatic hibernation.

Milestone 57 migrates the same-domain command family to Lea-aware matching,
including mixed-engine disposal, paused/busy reload skips, private copies,
seeded private new windows, and bounded exports. Snapshot cancellation no
longer writes a default file. Two new Boot cases bring the suite to 24; see
`DOMAIN_COMMANDS.md` and `Domain-commands-host.patch`. Milestone 58 follows with
split view and its navigation policy hook.

Milestone 56 migrates shared bulk opening, workspaces/protected sessions,
closed-window commands, and unread watch batches to Lea. It fixes reproduced
first-document replacement and a closed-window legacy cast, seeds restored
windows without default requests, and retains private/hidden/paused snapshots.
Watch records remain unread when skipped and preserve case-sensitive URLs.
Three new Boot cases bring the suite to 22; see `BULK_OPENING.md` and
`Bulk-opening-host.patch`. Milestone 57 follows with same-domain operations.

Milestone 55 makes the existing off/background/all saved-session policies native
to Lea. Deferred tabs create only placeholders; loopback tests confirm one/zero
initial page requests for background/all. Explicit restore preserves existing
documents; identity switches replace old mixed/private/hidden contexts. Three
new Boot cases bring the suite to nineteen, and all six EXE startup scenarios
pass, including Resume-button activation. See `LAZY_RESTORE.md` and
`Lazy-restore-host.patch`. Milestone 56 extends it to bulk/workspace/closed-window
routes; automatic hibernation and retained document snapshots remain future work.

Milestone 54 adds Lea-native tab/group/overview suspension and fresh-view resume,
preserving private state and hidden/paused session URLs. Core cancellation and
post-disposal callback guards prevent late presentation. Native UI Automation
teardown fixes a reproduced retained-renderer regression. Reload is visible
again; two Boot cases bring the suite to sixteen. See `SUSPENSION.md` and
`Suspension-host.patch`. Milestone 55 builds saved-session deferral on this
lifecycle; automatic hibernation and retained snapshots remain future work.

Milestone 53 fixes saved-homepage handling and the hidden/disabled Home menu,
migrates previous-session and clipboard/site-root opening, and unifies both
left-close variants. Existing pages survive explicit restore; ports/IPv6,
private opens, text/session bounds, and mixed-engine disposal are covered by
the new navigation/links Boot cases. `NAVIGATION_COMMANDS.md` and
`Navigation-host.patch` document the fourteen-case suite and remaining limits.

Milestone 52 routes common duplicate, close/reopen, bookmark, reload-all, and
named-session commands through Lea-aware helpers. Close Tab is visible again;
last-legacy-tab closure uses normal cleanup. Private Lea views no longer enter
recent history or automatic session snapshots through these paths. The new
twelfth Boot case and `Tab-session-host.patch` are documented in
`TAB_SESSION_COMMANDS.md`, including explicit private opens and restore limits.

Milestone 51 profiles the slow host startup and batches root/category menu
layout with Finally cleanup. Five matched before/after samples reduce median
Form.Show from 14.062 to 1.683 seconds, retaining the same 552-item menu tree.
The new eleventh Boot case checks nested layout and failure cleanup. Opt-in
phase diagnostics and real EXE readiness timing are documented in
`STARTUP_PERFORMANCE.md`; engine DLLs and HTML/CSS behavior are unchanged.

Milestone 50 moves normal first-page and primary new-tab startup onto Lea,
unifies bounded restore precedence, preserves Lea recovery URLs, and fixes the
EXE's compressed-settings parsing and failed-session-save shutdown paths.
`LEA_STARTUP.md` documents the source patches and ten real form-start cases.

Milestone 49 fixes reproduced Firespider startup and mixed-engine callback
failures. The tracked adapter, `Lea.Firespider/Startup-host.patch`, eighteen
focused host checks, and `Lea.Build/Test-FirespiderStartup.ps1` retain both the
fixes and their reproduction paths. See `STARTUP_RELIABILITY.md`.

1. Continue migrating advanced group/session/window routes, pop-up/split-view
   operations, and DOM-dependent commands; keep distinguishing legacy-only
   commands from Lea-supported ones and host timing from engine-only timing.
2. Keep the stable empty UI Automation visible-range fallback until a native-safe
   non-empty bridge survives direct plus external MTA clients.
3. Keep strengthening host tests as more legacy tab routes move to Lea.
4. Continue the bounded HTML/CSS subset without adding a general JavaScript VM
   or external browser runtime.

The repeatable compatibility and performance report is now generated by
`Lea.Bench`; its current results live in `COMPATIBILITY_PERFORMANCE.md`.
The latest `text-overflow.html` fixture brings the benchmark to 58 fixtures
and 57 executable compatibility probes. Compact non-inherited `S.To=c/e`
adds strict clip/ellipsis parsing, CSS-wide resets and explicit inheritance,
variables, priority, invalid preservation, hover, and responsive rules.
Paint-only truncation keeps retained line sizes and pointer targets, including
collapsed inline link spaces. Text-element prefixes, first-character clipping,
block-styled markers, full logical automation text, visible prefix geometry,
tables, explicit lines, and raw field retention have executable coverage.
  Native controls, SVG text, custom/two-ended overflow, and bidi layout remain
  outside text-overflow; bounded line clamping is implemented separately.

The preceding `text-spacing.html` fixture brought the benchmark to 57 fixtures
and 56 executable compatibility probes. Compact inherited `S.Ls/Wg` carries
signed -32..128 pixel letter and word spacing. Strict parsing, invalid
preservation, parent-restoring CSS-wide values, variables, important priority,
inline overrides, shared measurement/paint, line-edge suppression, rounded
inline boundary gaps, word-only shaping, controls, automation geometry, hover,
and responsive field retention are covered by the engine, view, and compiled
Firespider smokes. Zero-spacing text keeps the existing fast path. Native
editors/popups, complex-script tracking, and non-integer units remain limited.

The preceding `text-transform.html` fixture covers compact inherited `S.Tt=n/u/l/c`
state and shared `E.Tx` presentation handling for strict none, uppercase,
lowercase, and capitalize behavior across normal and multiline blocks, styled
inline overrides, tables, semantic values, buttons, placeholders, select
labels, and unfocused fields. Variables, priority, CSS-wide inheritance/resets,
invalid preservation, hover, responsive rebuilds, raw DOM/form retention,
focused native editing, and case-insensitive UI Automation geometry mapping
share the retained text path.
The preceding `pointer-interaction.html` fixture covers compact inherited `S.Cu/Pe`
state for automatic/default, pointer, text, wait, crosshair, move, help,
not-allowed, and axis-resize cursors plus `pointer-events:auto/none`. Variables,
priority, resets, invalid preservation, inherited suppression, child re-enable,
hover, links, fields, image/SVG hit gates, underlying fallthrough, keyboard and
accessibility stability, and responsive changes share the retained host path.
The preceding `outlines.html` fixture covers compact `S.Ow/Os/Oc/Oo` state and bounded
atomic shorthand/longhands, solid/dashed/dotted/auto styles, alpha and
`currentColor`, signed offsets, variables, priority/reset/noninheritance,
rounded boxes, fields, dynamic focus, clipping, stable geometry, and responsive
repaint. The preceding `aspect-ratio.html` fixture covers compact `S.Rw/Rh/Ra`
state and bounded
number and two-part ratios, optional intrinsic `auto` fallback, variables,
priority/reset/noninheritance, content/border boxes, min/max constraints,
normal and nested flow, raster/SVG media, fields, flex, grid, tables, and
responsive repaint. The preceding `object-fit.html` fixture covers compact
`S.Of=f/c/v/n/s` and
`Ox/Oy/Oxp/Oyp` state covers bounded fill, contain, cover, none, scale-down,
pixel/percentage position, variables, priority/reset/noninheritance,
responsive clipped paint, and fitted image-map pointer, keyboard, and
accessibility geometry. The preceding `background-images.html` fixture covers
compact `S.Bi` and
`Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` state covers one atomic bitmap layer,
bounded data/file/HTTP(S) origins, repeat axes, no-repeat, pixel/percentage
position and size, auto/cover/contain sizing, variables, priority/reset/bounds,
noninheritance, rounded page/box/field paint, opacity, clipping, stable hit
geometry, responsive repaint, and the shared image cache. The preceding
`background-gradients.html` fixture covers compact `S.Gi`, `Gi.K/A/X/Y/Ss`,
and `Cs.C/P` gradient state. The earlier `box-shadows.html` fixture covers compact `S.Sw` and
`Sw.X/Y/B/R/C/I`, atomic four-entry lists, rounded outer/inset paint, and the
12-band blur ceiling.
The bounded HTML scanner now handles quoted delimiters, comments, declarations,
raw text, RCDATA, entities, malformed delimiters, and explicit tree limits.
Document hosts now use `E.Id/Q/Qa` or `V.Id/Q1/Qa` for bounded compiled queries.
Pages now expose `Bu`; the first valid bounded `<base href>` drives relative
styles, links, images, forms, and downloads while `U` remains the response URL.
Safe scripts and hosts now use `ac/rc/tc` for bounded ID-scoped class changes;
compiled CSS is reapplied without replacing edited form state.
Safe scripts and hosts now also use `hd/sh`, `op/cl/tg`, `ds/en`, `vl`, `ck`, and
`fo` for bounded visibility, disclosure, disabled, value, checked, and focus state. A retained field
map preserves one existing model while hidden; `E.Kp` and `V` consume one focus
request without exposing files, passwords, arbitrary attributes, or native UI.
`grid-content-alignment.html` brings the benchmark to 42 fixtures and 41
executable compatibility probes. Compact `Al/Jc` state and atomic
`place-content` cover start, center, end, between, around, and evenly track-group
distribution across both axes, with exact geometry, paint, hits, following flow,
and Firespider host checks. The preceding `grid-alignment.html` fixture covers
compact `Ji/Js/Ai/As` container defaults and item overrides across start,
center, end, and stretch, while atomic
`place-items`/`place-self`, measured auto width, exact geometry, paint, hits,
following flow, and Firespider host checks share the existing renderer. The
preceding `grid-shorthand.html` fixture covers atomic `grid-template` and `grid`
compilation across
explicit tracks, optionally sized quoted areas, row/column auto-flow and dense
forms, explicit auto tracks, owned reset boundaries, malformed declaration
preservation, exact geometry, paint, hits, following flow, and Firespider host
checks. The preceding `grid-auto-tracks.html` fixture covers `S.Av` plus
`Gac/Gav/Rac/Rav` repeating pixel,
auto, integer-fr, and minmax implicit patterns, definite endpoint/span growth,
area-created columns, exact auto-height row settlement, atomic validation,
paint, hits, following flow, and Firespider host geometry. The preceding
`grid-flow.html` fixture covers compact `Gf=r/d/c/x`, sparse and dense row/column cursors,
earlier-hole backfill, equal-share implicit columns, the non-overlapping
eight-column ceiling, auto-height visual traversal, exact paint/flow geometry,
and Firespider host checks cover the new path. The preceding `grid-lines.html`
fixture covers lazy `Gnc/Rnc/Gx/Rx` maps, item `Gn/Ge/Rn/Re` edges,
case-sensitive unique validation, area-generated start/end lines, repeat and
declaration-order limits, exact paint/flow geometry, and Firespider host checks
cover that path. The preceding `grid-areas.html` fixture covers compact
`Aa/Ar/Ac/Ag/An` aliases, equal-row and rectangular validation, dot cells,
definite-first occupancy,
implicit columns, nested inline hits, exact paint/flow geometry, strict 8x8 and
16-name limits. The original `grid.html` fixture covers lazy compact `Gd/Gc/Gv`
columns and `Rd/Rc/Rv` rows with direct-item `Gl/Gs` and `Rl/Rs` placement,
fixed pixel
and integer-fr tracks, simple repeat/minmax forms, gaps, bounded occupancy,
two-axis spans, implicit rows, stable order-modified traversal, auto-height
settling, nesting, fields, paint, hits, resize, strict invalid limits, Inspector
visibility, and Firespider geometry. The preceding
`selector-relational.html` fixture covers strict precompiled `:has` lists with implicit descendants, explicit child,
adjacent and following relationships, four-part chains, maximum argument
specificity, dynamic descendant focus, retained fields, shared query behavior,
4,096 candidate visits, invalid nesting/list/part rejection, and WinForms plus
Firespider geometry. The preceding `selector-filtered-position.html` fixture
covers strict compound `of S` lists. Matching-sibling forward/reverse counts, maximum filter
specificity, form-state filters, shared test bounds, invalid nesting/type/list
rejection, document queries, dynamic retention, and WinForms geometry cover the
shared compiler path. The preceding `selector-arithmetic.html` fixture covers precompiled
positive/negative/zero `An+B`, forward/reverse child and type positions,
specificity, selector-list composition, document queries, strict syntax/value
bounds, and WinForms geometry. The preceding
`selector-functions.html` fixture covers bounded compound lists,
maximum `:is`/`:not` specificity, zero `:where` specificity, forgiving versus
strict alternatives, outer combinators, document queries, dynamic state, and
retained fields. In the preceding important
fixture, one bit per compiled declaration drives normal/important stylesheet
and inline tiers; compact `G.Ic`
diagnostics, specificity/source order, shorthands, custom properties, marker
boundaries, hover/media activation, WinForms geometry, and retained fields cover
the path. The preceding custom-property fixture covers lazy inherited maps,
compact `S.Vc/Cv` and `G.Vc`, nested/cycle fallbacks, and strict count/name/value/
depth/output limits. Compact `E/V/P/G.Vw`
state drives bounded screen/all pixel min/max/exact, comma, nested, and style
media conditions. Rules compile once, WinForms resize activates only the matching
set, diagnostics carry short `@m` labels, and mutable fields retain identity.
Non-inherited compact `Bz=c/b` state applies pixel
width/height, min/max, and flex-basis values to the content or border edge across
blocks, fields, images, flex items, tables, and cells. Exact wide geometry,
narrow reflow, paint, hit, benchmark, and Firespider host bounds use the shared
renderer path. The opt-in persistent cache uses compact `Cd/Cx/Cb/Dc/Db/Cdl`
aliases, compressed atomic entries, validators, LRU ceilings, private isolation,
and corrupt-entry recovery while retaining the zero-disk-I/O default. Bounded
`flex`/`inline-flex` row and column flow uses compact
`Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or` state for direction, stable order,
around/evenly distribution, container/item alignment, wrapping, row/column gaps, integer growth/shrink, and
pixel basis. Fixed, equal-share auto, wrapped, growing, min-protected, and locked
slots retain direct item ownership and reuse nested layout, painting, hits,
narrow overflow, and Firespider coverage.
Firespider now opens Lea Inspector with `F12` or a Lea tab's context menu. Its
Summary, Nodes, CSS, Requests, and Messages tabs consume only `V.Dg()`; its
Access tab reads bounded `V.Axs`. Refresh and request-log clearing are covered
by the off-screen host smoke suite.
The safe page language now uses `go(url)` to queue one host-controlled request.
`E.Gt` consumes it headlessly; `V.Gu`/`Gr` route it through the test browser and
Firespider while unsupported schemes and remote-to-file requests are rejected.
One-argument `bg/fg/fs` still styles the body; two arguments now scope the same
bounded color or 8..96px font values to one safe ID, with a 4KB inline-style cap
and field-preserving compiled-CSS rebuild verified through every host layer.
`E.Tm`/`V.Tm` now apply a bounded 100-60,000 ms network deadline to synchronous
and asynchronous requests, preserving typed retryable timeout and distinct
host-cancellation results.
Firespider's normal New Tab command and `Ctrl+T` now create Lea views. A visible
New Legacy Tab command keeps the original `q3`/`WebBrowser` route available for
features that have not crossed the adapter boundary yet.
List items now expose compact `B.Mk` markers. Unordered lists retain `-`, while
ordered lists honor bounded decimal `start` and `value` sequences independently
across nested scopes; simple and inline text share the same resize-safe paint path.
Details elements now retain compact page-local `N.Op` state. Their first direct
summary uses `B.Mk` (`>`/`v`), closed content remains modelled behind `B.Hd`, and
`E.Ds`/`V.Ds` preserve nested and field state across pointer or keyboard toggles.
Same-page links now use bounded `Ft` target lookup, `Fg` retained history, and
`Rv` details reveal. IDs and named anchors scroll through existing layout boxes
without fetching, while missing, top, table, encoded, Back/Forward, and resize
cases share one 128-entry path in the engine, test browser, and Firespider.
Progress and meter elements now expose compact `B.Vm` state. Determinate and
indeterminate progress, ordered meter regions, finite clamping, inherited
colors, table cells, custom painting, and resize reflow share `gauges.html`.
Output and data elements now expose compact `P.Dvs`, `B.Dv`, and `Ir.Dv` state.
Bounded current/visible values, output references, form/name metadata, DOM
ownership, safe updates, table cells, style rebuilds, and resize share
`values.html` without adding native controls.
Time and edit elements now reuse that compact value path. `K=t/i/r`, `Dt`, and
`U` expose bounded machine-readable time, authored edit dates, and safe resolved
citations. Native insert/remove decoration, inline and block ownership, table
cells, text/class rebuilds, Firespider hosting, and resize share
`time-edits.html` without adding native controls or citation navigation.
Image maps now expose compact `P.Mps`, `B.Mp`, and `Ar` state. Rectangle,
circle, polygon, default, and dead regions use bounded coordinates and one
shape-aware custom hit path. First-area layering, malformed empty shapes,
percent-decoded names, area links/downloads, class rebuilds, viewport scaling,
Firespider hosting, and paint share `image-maps.html` without native controls.
Keyboard navigation now parses bounded `tabindex` values through `Ti` and
exposes `Ke/Kn/Ki/Kb/Kf/Ka` on the custom view. Positive priorities, default
tree order, negative click focus, links, repeated image-map regions, fields,
summaries, generic regions, focus CSS, visible rings, editor escape, scrolling,
wrapping, and host activation share `keyboard-focus.html`. `Ctrl+Tab` and
`Alt+Tab` remain available to browser and operating-system chrome.
Autofocus and page shortcuts now stay on that same bounded focus model.
`Af/Afs` expose the first 32 authored candidates, while `Ak/Aks` parse up to 16
unique ASCII letter/digit alternatives from 256 characters. The view selects
the first rendered focusable autofocus candidate unless a resolved fragment owns the
load, assigns at most 64 conflict-free keys in document order, and exposes
`An/Ak/Kl/Aa`. `Alt+Shift+key` gives Firespider first refusal, consumes only an
assigned page key, and reuses existing focus, field, summary, image-map, link,
download, command, and navigation paths. `focus-shortcuts.html` covers hidden,
disabled, conflicting, fallback, editor, fragment, and WinForms command cases.
HTML `hidden` now supplies default rendering suppression as well as focus and
shortcut exclusion, so ordinary hidden links and controls do not leak into
layout or hit routes.
Semantic accessibility now uses one bounded flat `P.Axs` list of compact `Ax`
records tied to the existing `N` tree. Native and supported explicit roles,
accessible names/descriptions, hierarchy, heading levels, URLs, live form and
disclosure values/states, focus, and strict redaction share one 512-record path.
`E.Ax/Axs` and `V.Ax/Axs/Ab/Ao` expose that model to hosts; `V` maps it to real
WinForms/MSAA children with rendered bounds, keyboard shortcuts, focus, and
default actions. `accessibility.html` covers HTML labels, ARIA references and
states, landmarks, tables, fields, gauges, exclusions, rebuilds, and limits.
The native UI Automation provider now also maps real `Tb/Tr/Tc` ownership to
bounded grid/table and grid-item/table-item patterns. Zero-based coordinates,
column spans, containing-grid links, both header directions, range rejection,
the MTA client, benchmark probe, and Firespider host smoke share that one model.
The root provider now also exposes bounded read-only logical TextPattern ranges
over `P.X` and settled boxes. Document, child, and point ranges cover search,
movement, read-only attributes, rendered rectangles, embedded children,
scrolling, stale-page rejection, and protected-value exclusion. The public MTA
client and Firespider smoke verify direct ranges. Plain wrapped blocks and inline
flow now add bounded visual line starts, line-unit movement, merged per-line
rectangles, and proportional partial-range geometry, verified by the MTA client,
benchmark, and Firespider host. Visible-range arrays stay empty: managed arrays,
explicit COM identities, manually built `SAFEARRAY(VT_UNKNOWN)` values, and both
in-process and external MTA clients still produced invalid range handles, so a
future implementation must live beyond the managed provider array boundary.

## Rules For Future Codex Work

- Keep `LeaForge` separate from `Lea.Micro`, `LeaBrowser`, and `LeaPlus`.
- Do not replace the custom renderer with WebView unless we create a separate comparison project.
- Prefer small working steps and compile an exe after each meaningful milestone.
- Keep local test pages in `test-pages`.
- Keep public docs in `README.md`; keep working notes here.
- Keep new public Lea features within the alias rules in `ALIASES.md`.
