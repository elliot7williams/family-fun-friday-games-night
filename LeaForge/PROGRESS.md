# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **57**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, memory LRU and optional persistent cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled relationship, bounded strict relational/functional, An+B/reverse, and filtered structural selectors, declarations with two-tier important cascade, inherited custom properties and pixel-width media queries, dynamic hover/focus, inherited cursor/pointer interaction, text transformation and signed letter/word spacing, content/border sizing, circular/elliptical radii, bounded outlines, box shadows, gradients, bitmap backgrounds, object fitting and aspect ratios, visibility/opacity, fonts, overflow, boxes, bounded ordered flex and two-axis grid properties |
| Layout and painting | 20% | 20% | Shared content/border and aspect-ratio geometry, transformed and spacing-aware presentation text with raw form retention, normalized rounded backgrounds/borders/outlines/overflow/fields/shadows/gradients/bitmaps, pointer-gated retained hits with underlying fallthrough, fitted raster images and maps, ordered flex rows/columns, bounded grid tracks/spans, distribution, alignment, wrapping, grow/shrink, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive and fitted images, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, variables, text/title/body and ID-scoped style/alert, class, visibility, disclosure, field-state, and focus commands |
| Firespider integration and reliability | 10% | 10% | Lea-first New Tab/Ctrl+T, explicit legacy fallback, Inspector with Access view, MSAA/UIA surfaces including read-only text, host-safe focus shortcuts, navigation, sessions, source, host smokes |
| **Total** | **100%** | **100%** | Scoped Lea 1.0 baseline complete; post-1.0 hardening continues |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Completed post-1.0 hardening

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

- Add a native-safe non-empty visible-range bridge outside the .NET Framework
  managed provider array boundary; retain the stable empty fallback until range
  handles survive both in-process and external MTA clients.
- Keep expanding the bounded HTML/CSS subset and Lea-first host routes without
  adding a general JavaScript VM or external browser runtime.
