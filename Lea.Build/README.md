# Lea.Build

These files are fallback build sources for machines that have the old Windows C# compiler but do not have the modern .NET SDK.

The compiled test executables go in:

```text
dist/LeaExes
```

They are .NET Framework executables:

- `LeaMicro.exe`
- `LeaBrowser.exe`
- `LeaPlus.exe`
- `LeaForge.exe`

Each exe is compiled with `../assets/lea.ico` embedded as its Windows icon.

The cleaner source projects are still:

- `Lea.Core`
- `Lea`
- `LeaBrowser`
- `Lea.Micro`
- `LeaPlus`
- `LeaForge`

`LeaForge.exe` is compiled directly from `../LeaForge/Program.cs` because that file is intentionally compatible with the older Windows C# compiler.

## Source Backups

Run `Backup-Lea.ps1` to create a dated, source-only ZIP in the Documents
`Lea Source Backups` folder. Git and GitHub remain the main version history;
the ZIP is an additional local copy without `bin`, `obj`, or `dist` files.

Run `Backup-Firespider.ps1` to create the matching source-only host ZIP from
`L:\Firespider Internet` in the Documents `Firespider Source Backups` folder.
It excludes Git/editor/build/cache output and compiled binaries while retaining
the VB project, Lea adapter, resources, configuration, installer source, and
design assets. Pass `-Source` only when the host checkout moves.

## Compatibility And Performance

`Lea.Bench` is the repeatable .NET Framework report runner. After a Release
build, run it from the repository root:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```

Its compact switches are `-r` for repository root, `-o` for report output,
`-n` for iterations, and optional `-b` for a Firespider executable path.
The same runner recognizes an extracted Lea runtime package and measures its
local DLLs and Firespider executable without requiring a source solution.
Its fixture set includes `font-relative-units.html`, covering bounded `em` and
`rem` model, layout, commands, reset, root media changes, and resize behavior.
It also includes `size-functions.html`, covering bounded `min()`, `max()`, and
`clamp()` records, mixed terms, constraints, media replacement, commands,
shared layout geometry, reset, and resize behavior.
`logical-sizing-insets.html` covers horizontal logical size/min/max mappings,
logical inset longhands and axis shorthands, source order, atomic fallback,
`is/bs`, familiar DOM aliases, shared geometry, and reset. See
`LeaForge/LOGICAL_SIZING_INSETS.md`.
`logical-box-model.html` covers all horizontal logical margin/padding axes and
longhands, signed bounded margins, physical/logical source order, important,
variables, inheritance/reset, atomic fallback, twelve short/readable commands,
all familiar DOM forms, shared geometry, and authored restore. See
`LeaForge/LOGICAL_BOX_MODEL.md`.
`logical-borders.html` covers all horizontal logical border axes, edges, and
width/style/color components, corrected physical per-edge lists, lazy records,
source order, important, variables, inheritance/reset, atomic fallback, all 24
short/readable commands and familiar DOM forms, shared geometry, hidden edges,
mixed rounded paint, and authored restore. See `LeaForge/LOGICAL_BORDERS.md`.
`logical-radii.html` covers all four horizontal logical corners, physical and
logical source order, important, variables, shorthand/corner inheritance, reset
keywords, atomic fallback, all nine short/readable radius commands, familiar DOM
forms, authored restore, and asymmetric paint. See
`LeaForge/LOGICAL_RADII.md`.
`direction-logical.html` covers inherited horizontal LTR/RTL state, HTML `dir`,
variables, important and pre-cascade resolution, direction-aware logical
insets/boxes/borders/radii, translated cross-direction inheritance,
`text-align: start/end`, compact/readable `dr`, familiar DOM forms, reset, and
invalid preservation. See `LeaForge/DIRECTION.md`.
`direction-layout.html` covers exact LTR/RTL flex row, reverse, order, wrap and
column cross-axis geometry plus logical grid auto, dense, column, explicit line,
span, named-area, content/self alignment, unequal tracks, paint, source order,
and live compact `dr` reflow. See `LeaForge/DIRECTION.md`.
`direction-inline.html` covers right-origin plain, mixed, wrapped, nowrap, and
justified inline flow; physical/logical text alignment; nested ordered,
unordered, and disclosure markers; structured table parts; links; source order;
paint; and live compact `dr` reflow. See `LeaForge/DIRECTION.md`.
`direction-overflow.html` covers bounded LTR/RTL physical inline-end ellipsis,
right-aligned grapheme-safe RTL paint prefixes, retained link hits, table-cell
clips, full logical text, direction-aware partial/wrapped/point TextPattern
geometry, and live compact `dr` reflow. See `LeaForge/DIRECTION.md`.
`fragment-stability.html` covers `Fa/Fj/Fy`, first-paint retention, responsive
host settlement, intrinsic image reflow, missing targets, and manual-scroll
release. See `LeaForge/INITIAL_FRAGMENT_STABILITY.md`.
`text-wrap.html` covers inherited wrap mode/style, strict shorthand reset,
`white-space` source order, CSS-wide values, variables, important tiers,
bounded same-line-count balance, LTR/RTL, indent, forced breaks, tables,
compact/readable/literal commands, hover, and responsive resize. See
`LeaForge/TEXT_WRAP.md`.
`line-clamp.html` covers non-inherited standard/WebKit none and 1..32 state,
explicit inheritance and resets, invalid preservation, variables, priority,
grapheme-safe LTR/RTL markers, reduced flow height, removed hidden boxes/hits,
full logical text, forced breaks, balancing, tables, compact/readable/literal
commands, hover, and responsive resize. See `LeaForge/LINE_CLAMP.md`.
`list-style.html` covers inherited standard list types, strict shorthand
shorthand, HTML list/item hints, CSS precedence, reversed/start/value sequences,
alpha rollover, bounded Roman fallback, leading zero, nested lists, no-marker
spacing, LTR/RTL, tables, compact/readable/literal commands, hover, retained
fields, and responsive resize. See `LeaForge/LIST_STYLE.md`.
`list-position.html` covers inherited outside/inside state, independent
longhands, atomic one/two-component shorthand order and reset, CSS-wide values,
variables, priority, invalid preservation, none, LTR/RTL gutters, wrapping,
tables, compact/readable/literal commands, hover, retained fields, and
responsive resize. See `LeaForge/LIST_POSITION.md`.
`list-image.html` covers inherited/reset marker images, relative and data URLs,
three-component shorthand order, `none` ambiguity, variables, priority, invalid
atomicity, cache reuse, bounded image pixels and geometry, text fallback,
LTR/RTL inside/outside placement, tables, compact/readable/literal commands,
hover, retained fields, and responsive resize. See `LeaForge/LIST_IMAGE.md`.
`marker.html` covers bounded terminal `::marker` selectors, isolated inherited
and direct marker text styles, quoted/escaped/none content, variables, priority,
image/type/content precedence, inside/outside LTR/RTL geometry, tables,
compact/readable commands, hover, retained fields, and responsive resize. See
`LeaForge/MARKER.md`.
`marker-content.html` covers up to eight ordered quoted/`attr()` fragments,
explicit raw-string mode, quoted missing-value fallbacks, present/empty
priority, variables, count/name/output bounds, invalid preservation,
markup-like text, commands, image precedence, tables, LTR/RTL geometry, hover,
retained fields, and resize.
See `LeaForge/MARKER_CONTENT.md`.
`marker-counters.html` covers bounded implicit `list-item` `counter()` and
`counters()` output, ten compact built-in styles, HTML start/value/reversed
paths, eight nested levels, 32-unit separators, the shared output ceiling,
variables, priority, invalid preservation, commands, image precedence, tables,
LTR/RTL geometry, hover, retained fields, and resize. See
`LeaForge/MARKER_COUNTERS.md`.
`css-counters.html` covers bounded named `counter-reset`, `counter-increment`,
and `counter-set`, specification operation order, defaults, duplicate handling,
case-sensitive names, tree-order scope, self nesting, missing values, exact
entry/name/integer bounds, hidden branches, formats, variables, priority,
tables, compact/readable/literal commands, hover/media, retained fields,
geometry, and paint. See `LeaForge/CSS_COUNTERS.md`.
`generated-quotes.html` covers inherited `auto`/`none`/explicit quote pairs,
open/close and silent content keywords, document-order depth across marker,
before, children, after, and siblings, exact pair/depth/output limits,
variables, invalid preservation, `qu`/readable/literal commands, links, tables,
accessibility, hover/media, retained fields, geometry, and paint. See
`LeaForge/GENERATED_QUOTES.md`.
`generated-images.html` covers mixed string/bitmap generated fragments,
relative/data URLs, variables, alternatives, image-only links, tables,
inside/outside markers, exact item bounds, invalid preservation, commands,
hover/media rebuilds, retained fields, sandboxed loading, geometry, and paint.
See `LeaForge/GENERATED_IMAGES.md`.

## Firespider Startup Checks

`Lea.Tests.exe <repository-root> --edit` runs native clipboard/editor checks
alone. `--core` runs the remaining engine suite without accessing the system
clipboard; the default still runs both. `--fragment` repeats initial fragment
navigation 25 times. `--clip` isolates the held-clipboard regression; `--paste`
checks supplied text and field bounds without accessing the clipboard.
`--cache` isolates exact path/query cache identity, fragment sharing, validators,
linked CSS, and persistent restart behavior.
`--direction-overflow` isolates horizontal LTR/RTL ellipsis and read-only range
geometry without running the rest of the suite.
`--text-indent` isolates bounded inherited pixel/percentage indentation,
compact/literal command routes, LTR/RTL first-line geometry, wrapping, hover,
paint, and responsive resize.
`--text-shadow` isolates one inherited bounded text-shadow layer, compact and
literal command routes, resets/invalid preservation, block/field paint, stable
geometry, hover, and responsive resize.
`--text-decoration` isolates bounded inherited line/color/style/thickness,
atomic shorthand and component commands, invalid preservation, native/custom
paint, wrapping, tables, links, fields, hover, and responsive resize.
`--text-underline-offset` isolates bounded inherited signed pixel/percentage
offsets, shorthand independence, invalid preservation, compact/readable/literal
commands, paint position, wrapping, tables, default links, fields, hover, and
responsive resize.
`--text-wrap` isolates inherited wrap mode/style, strict shorthand and
longhand interaction, `white-space` source order, compact/readable/literal
commands, bounded same-line-count balance, LTR/RTL, indent, forced breaks,
tables, hover, and responsive resize.
`--line-clamp` isolates non-inherited standard/WebKit line clamping, strict
1..32 bounds, cascade and command routes, retained geometry and flow height,
LTR/RTL markers, hidden hits, full logical automation text, forced breaks,
tables, hover, and responsive resize.
`--list-style` isolates inherited marker types, HTML type and reversed sequence
behavior, strict cascade and command routes, marker text/geometry, no-marker
spacing, nested lists, tables, retained fields, hover, and responsive resize.
`--list-position` isolates inherited marker position, shorthand composition,
LTR/RTL outside gutters and inside first-line geometry, tables, retained fields,
commands, hover, and responsive resize.
`--list-image` isolates inherited marker images, URL-aware shorthand and `none`
ambiguity, image admission/cache/paint, 64px geometry, text fallback, LTR/RTL
inside/outside placement, tables, retained fields, commands, hover, and
responsive resize.
`--marker` isolates bounded terminal pseudo rules, marker-only inherited text
state, specificity, quoted/escaped/none content, custom properties, important
tiers, command limits, image priority, LTR/RTL geometry, tables, hover, paint,
and responsive resize.
`--marker-content` isolates bounded ordered string/attribute content, raw-string
mode, quoted missing-value fallbacks, present/empty priority, variables, exact
fragment/name/output limits, invalid command preservation, image precedence,
tables, LTR/RTL paint, hover, and responsive resize.
`--marker-counters` isolates implicit list-item counter output, built-in
numeric/symbolic styles, nested paths, HTML numbering, exact depth/separator/
output limits, invalid command preservation, image precedence, tables, LTR/RTL
paint, hover, and responsive resize.
`--css-counters` isolates bounded named reset/increment/set state, tree-order
scope, named marker output, exact operation/name/value limits,
compact/readable/literal commands, tables, hover/media, retained fields, and
paint.
`--generated-content` isolates terminal double/single-colon before/after rules,
bounded string/attribute/quoted-fallback/named-counter content, inherited text
styles, specificity/variables/important, nested/link/value/table ownership,
accessibility names and reading order, compact/readable commands, exact limits,
replaced-control suppression, LTR/RTL paint, hover/media, and retained fields.
`--generated-quotes` isolates bounded inherited quote pairs and document-order
generated quote depth, including markers, before/after, exact limits, commands,
accessibility, tables, hover/media, retained fields, geometry, and paint.
`--generated-alt` isolates visual/alternative generated-content separation,
explicit empty alternatives, attributes, fallbacks, counters, variables,
eight-item and 128-unit limits, invalid preservation, links, explicit ARIA
priority, tables, markers, commands, hover/media, retained fields, geometry,
paint, and accessibility.
`--generated-images` isolates mixed string/bitmap before, after, and marker
content, relative/data URL origins, visual/alternative separation, links,
tables, exact item limits, invalid preservation, compact commands, hover/media
rebuilds, shared image admission, retained fields, geometry, and paint.
`--text-align` isolates the inherited text-align shorthand/all/last family,
logical inheritance and match-parent, justify-all, compact/readable/literal
commands, LTR/RTL geometry, shorthand resets, tables, hover, and responsive resize.
`--text-align-last` isolates bounded inherited final-line alignment, auto and
physical/logical/justify values, compact/readable/literal commands, final and
forced-line LTR/RTL geometry, stable boxes, tables, hover, and responsive resize.
`--sheets` isolates native named CSS, byte/rule limits, retained data,
painting, and editor font/selection/undo. It is included in `--core`.
`--css-supports-script` isolates compact `E/V.Su` capability overloads and
literal one/two-argument `CSS.supports()` guards, including else/negation,
selector and compound checks, four-level nesting, the shared command ceiling,
click/host bridges, retained fields, exact geometry, and strict rejection.
`--match-media-script` isolates compact `E/V.Mq` live media checks and literal
global/window `matchMedia(...).matches` guards, including viewport and
preference state, else/negation, mixed four-level nesting, the shared command
ceiling, click/host bridges, retained fields, resize, and exact geometry.
`--media-modifiers` isolates whole-query `not`, no-op `only` before recognized
media types, fixed screen/print behavior, comma alternatives, nested predicate
intersections, style media, literal matchMedia guards, malformed placement,
diagnostics, retained fields, resize, and exact WinForms geometry.
`--media-interaction` isolates compact `Pt/Hv/Ap/Ah/Mi` state, primary
pointer/hover and any-pointer/any-hover queries, coarse-plus-fine hybrid unions,
boolean/exact/not/nested/style-media selection, literal matchMedia guards,
malformed rejection, diagnostics, retained fields, Windows host plumbing, and
exact WinForms geometry.
`--container-style` isolates bounded nearest and named ancestor
`@container style()` queries, computed `S.Cn` name lists, custom-property and
`var()` comparison, grouped `not`/`and`/`or`, nested container/media/supports/
layer conditions, strict unsupported-query rejection, diagnostics, class and
native-sheet rebuilds, retained fields, and exact WinForms geometry.
`--container-size` isolates bounded `container-name`/`container-type` and the
`container` shorthand, compact `S.Ct`, atomic `E.Cz` content-box snapshots,
nearest/named width and height features, min/max and modern ranges, logical
size aliases, grouped size/style conditions, nesting, diagnostics, malformed
and ineligible queries, native sheets, retained fields, and automatic bounded
WinForms feedback with exact geometry.
`--container-features` isolates reduced-integer `aspect-ratio` boolean/exact/
min/max and strict/inclusive feature-first/value-first/chained ranges plus
portrait/landscape orientation, square portrait semantics, both-axis
eligibility, nested logic, diagnostics, malformed rejection, native sheets,
retained fields, and automatic bounded WinForms relayout.
`--container-units` isolates compact `cqw`/`cqh`/`cqi`/`cqb`/`cqmin`/`cqmax`
dimension codes, independent nearest inline/block ancestor selection, viewport
fallback, calculations and size functions, custom properties, computed
inheritance, commands, native sheets, dynamic measurements, retained fields,
and exact bounded WinForms geometry.
These test modes do not retry or suppress failed assertions. Windows
clipboard access can fail while another process holds it. Record separately
which modes passed; a clipboard-free run is not a complete default-suite pass.

Run the executable checks with Windows PowerShell and the built host root:

```powershell
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File .\Lea.Build\Test-FirespiderStartup.ps1 -Root 'L:\Firespider Internet'
```

The default checks clean, restored, locked, oversized, lazy-background, and
lazy-all temporary sessions, validates the EXE config, polls the selected-tab
title and native tab state from process launch, and observes for at least eight
seconds. The overall readiness limit is that observation period plus sixty
seconds. Separate top-level windows are inspected for exception/JIT and real
crash-recovery dialogs without walking Firespider's large menu tree. It checks
responsiveness and absence of MSHTML. Use `-Scenario clean` for one launch.
Lazy cases copy the runtime into a temporary directory with isolated default
settings; lazy-all also invokes the Resume button and waits for a Lea document.
Native tab-count/button messages have bounded timeouts for older WinForms
controls that expose only window-level UI Automation panes; an early busy tab
probe is retried inside the readiness loop.
It refuses to run alongside an existing Firespider instance and never closes
an existing browser process, and restores the session environment override.
The browser can still update its ordinary profile settings when it closes;
use an extracted runtime test directory for isolated package checks.

The compiled `Lea.Firespider.Smoke.exe <host-root> --startup` runs eighteen
event/session regressions off-screen. Omitting `--startup` runs the full host
suite. Source for the corresponding host edits is retained in
`Lea.Firespider/Startup-host.patch`, with reproduction notes in
`LeaForge/STARTUP_RELIABILITY.md`.

`Test-FirespiderBoot.ps1 -Root <host-root>` adds forty-two real form-start cases,
including private startup, restore precedence, second windows, a gated slow
HTTP response, and locked-session shutdown. It compiles `Lea.Firespider/Boot.cs`
beside the test host and runs separate off-screen processes with a sixty-second
watchdog. Use `-Case clean` for a single case. See `LeaForge/LEA_STARTUP.md`.
`-Case pictures` checks startup completion with a stalled image response,
retained fields, `Ic/Ib`, and request cleanup. `Lea.Tests <root> --pics` covers
streaming limits, deadlines, compression, bitmap retention, retry, cancellation,
and late disposal. See `LeaForge/PICTURE_LIMITS.md` for remaining decoder/input limits.
`-Case input` adds oversized-page failure/recovery and a shared linked-CSS
deadline. `Lea.Tests <root> --input` checks wire/decoded/file/cache/origin
boundaries, POST responses, timeouts, and cancellation; see `INPUT_LIMITS.md`.
`-Case cache` checks exact path/query resources, fragment-only reuse,
case-different navigation, synchronized layout snapshots during asynchronous
paint/navigation overlap, host request cleanup, full UI-thread exception
diagnostics, and no MSHTML. See
`LeaForge/CACHE_IDENTITY.md`.
`-Case decode` checks one valid helper decode, over-axis PNG preflight,
`Ip/Im/It/In/If/Ie/Ii/Iw/Ik/Ij/Ih/Ipm/Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk/Iac/Ind/Ial/Icl/Ihm/Icm/Iad`,
retained form data, zero-capability AppContainer and exact helper ACL,
restricted low-integrity token, startup ASLR, Job CPU cap, mitigation handshake,
private/Job-peak memory, combined host/helper admission, host cleanup,
child-process exit, and no MSHTML. The focused `--pics` suite additionally
forces an over-limit projection before payload admission and attempts live
loopback and host-file access, requiring all three boundaries while normal
decode succeeds. See
`LeaForge/IMAGE_ISOLATION.md`.
`-Case load-life` checks page-command successors, request ownership, queued CSS,
busy address input, and cleanup. `Lea.Tests <root> --loads` adds deterministic
view completion, callback, download, and image-handoff regressions. See
`LeaForge/LOAD_LIFECYCLE.md`.

The full core and compiled Firespider smokes also exercise the literal bounded
DOM bridge in `dom-bridge.html`: `document.getElementById` text, field/state,
focus, classList, and bounded style forms translate to existing short commands,
while `innerHTML`, computed targets, selectors, and unknown APIs remain inert.
`display-navigation.html` adds compact `dp`, literal `style.display`, authored
cascade restoration, WinForms flex reflow, and host-controlled literal location
requests with local query/fragment preservation. See
`LeaForge/DISPLAY_NAVIGATION.md`.
`calculated-widths.html` adds compact `wd`, literal `style.width`, percentage
and `calc(P% +/- Npx)` parsing, calculated min/max constraints, nested
flex/grid/table layout, cascade restoration, and resize reflow. See
`LeaForge/CALCULATED_WIDTHS.md`.
`calculated-heights.html` adds compact `ht`, literal `style.height`, definite
viewport/parent percentage resolution, calculated min/max constraints,
positioned/flex/grid/table/control/image layout, cascade restoration, and
vertical resize reflow. See `LeaForge/CALCULATED_HEIGHTS.md`.
`viewport-units.html` adds bounded `vw`, `vh`, `vmin`, and `vmax` dimensions,
cross-axis calculations and min/max constraints, lazy compact unit aliases,
auto-parent resolution, shared layout families, commands, and two-axis resize
reflow. See `LeaForge/VIEWPORT_UNITS.md`.
`size-functions.html` adds two/three-term `min()` and `max()`, three-term
`clamp()`, lazy compact model aliases, mixed bounded terms, width/height
constraints, layout families, commands, media replacement, and resize reflow.
See `LeaForge/SIZE_FUNCTIONS.md`.
`-Case edit` tests native field menu/hover commands, protected fields,
paused/empty tabs, and the explicit legacy fallback; see `NATIVE_EDITING.md`.
`-Case paste` checks guarded real-menu/native/keyboard paste under a held
clipboard lock, retained undo, release recovery, and compact `Pa` insertion;
see `PASTE_SAFETY.md`.
`-Case style-start` checks saved style automation at startup and through an
accelerated real timer callback. It covers the legacy preview cast guard;
CSS now applies natively while arbitrary JavaScript remains unsupported.
`-Case styles` adds matched saved CSS, queued previews, request-end cleanup,
Unicode limits, retained fields, privacy, and domain matching. See
`NATIVE_STYLESHEETS.md` for behavior and limits, and `STYLE_STARTUP_SAFETY.md`
for the original startup cast reproduction and remaining intermittent issues.
`-Case recovery` also observes 35 seconds of UI events and validates the real
30-second recovery snapshot timer against the restored Lea URLs.
`-Case policy` checks main-tab rules, denied input/forms/history, private/hidden
ownership, resume, and disposal. `-Case policy-start` checks blocked saved-URL
startup alongside an allowed page. See `LeaForge/NAVIGATION_POLICY.md`.
`-Case failures` checks native load failures, retained edits/status, and Reload
retry. `-Case failure-start` checks unavailable restored pages and recovery.
See `LeaForge/LOAD_FAILURES.md` for the compact failure API and remaining limits.
`-Case hibernate` checks retained-data exclusions, native renderer collection,
and resume. `-Case hibernate-start` observes the actual 30-second policy timer
after restoration and verifies closing cleanup. See `LeaForge/HIBERNATION.md`.
Native argument quoting accepts host paths with or without a trailing separator.
`-Case menus` verifies nested layout and error cleanup. `-Profile` reports
constructor/phase timings, root layout counts, and a deterministic menu-tree
signature without enabling profiling in normal browser use. The EXE test
also reports `ReadyMs` and `ObservedMs`. See `LeaForge/STARTUP_PERFORMANCE.md`
for the paired startup measurements and their limits.

`-Case commands` exercises Lea/mixed-engine duplicate, close/reopen, bookmarks,
reload, named sessions, private-state handling, and normal last-tab shutdown.
The runner requires a final success marker as well as a zero process exit code.
See `LeaForge/TAB_SESSION_COMMANDS.md` for behavior, bounds, and remaining routes.

`-Case navigation` covers saved/default Home, previous-session restoration,
tab-list selection, and both mixed-engine left-close menus. `-Case links` checks
loopback site-root navigation, IPv6/port handling, and bounded single/batch URL
opening without reading or overwriting the user's clipboard. The runner retains
the process handle and drains redirected output before checking completion.
See `LeaForge/NAVIGATION_COMMANDS.md` for exact behavior and test boundaries.

`-Case suspend` checks native Lea disposal/collection, cancellation, private
resume, navigation, duplicate, and close. `-Case groups` checks the saved-group
startup timer, collapse/expand, hidden ownership, sessions, and overview routes.
See `LeaForge/SUSPENSION.md` for reload semantics and remaining legacy paths.

`-Case lazy` covers saved-session policy, preserved documents, URL bounds,
private placeholders, eager clipboard batches, and mixed/hidden identity
replacement. `-Case lazy-background` and `-Case lazy-all` count loopback requests
at startup, selection, Reload, and Resume All. See `LeaForge/LAZY_RESTORE.md`.

`-Case bulk` covers direct eager and workspace lazy restoration, preserved
documents, metadata, privacy, and bounds. `-Case windows` invokes last/recent
closed-window routes with exact request counts and snapshot checks. `-Case watch`
verifies two bounded unread batches, duplicate/case-sensitive URLs, preserved
read state, and older unread records. See `LeaForge/BULK_OPENING.md`.

`-Case domains` covers visible-domain matching, both reload menus, paused/busy
skips, retained fields, mixed-engine commands, bookmarks/shortcut, private copies,
and close disposal. Its loopback server discards speculative legacy connections
that send zero header bytes within one second, while partial/malformed requests
remain failures. `-Case domain-private` checks seeded private new windows,
lazy behavior, exact requests, twelve-tab creation limits, and bounded exports
with control-character/surrogate and row-limit checks. OS clipboard and Save
dialog interaction are not automated. See `LeaForge/DOMAIN_COMMANDS.md`.

`-Case split` exercises independent Lea panes, private sources, fields, scroll
sync, comparison, host navigation rules, and disposal. `-Case split-life` adds
pending-load replacement, deferred sources, sticky privacy, and owner-close
cancellation. `-Case split-limits` covers comparison/source-list byte and count
ceilings. `LEA_SPLIT_SHOTS` optionally names an output folder for desktop and
small-window screenshots. See `LeaForge/SPLIT_VIEW.md` for unsupported routes.
