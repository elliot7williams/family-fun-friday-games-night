# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **30**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, memory LRU and optional persistent cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled relationship, bounded strict relational/functional, An+B/reverse, and filtered structural selectors, declarations with two-tier important cascade, inherited custom properties and pixel-width media queries, dynamic hover/focus, content/border sizing, visibility/opacity, fonts, overflow, boxes, bounded ordered flex and two-axis grid properties |
| Layout and painting | 20% | 20% | Shared content/border geometry, ordered flex rows/columns, bounded grid tracks/spans, distribution, alignment, wrapping, grow/shrink, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive images, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, variables, text/title/body and ID-scoped style/alert, class, visibility, disclosure, field-state, and focus commands |
| Firespider integration and reliability | 10% | 10% | Lea-first New Tab/Ctrl+T, explicit legacy fallback, Inspector with Access view, MSAA/UIA surfaces including read-only text, host-safe focus shortcuts, navigation, sessions, source, host smokes |
| **Total** | **100%** | **100%** | Scoped Lea 1.0 baseline complete; post-1.0 hardening continues |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Completed post-1.0 hardening

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
