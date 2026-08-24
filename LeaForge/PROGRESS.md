# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **14**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled selectors, dynamic hover/focus, sizing, visibility/opacity, fonts, overflow, boxes, bounded ordered and distributed row/column flex properties |
| Layout and painting | 20% | 20% | Ordered rows, bounded column/reverse flow, around/evenly distribution, item alignment, wrapping, grow/shrink, clipping, reflow, painting |
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

## Next gains

- Add a native-safe non-empty visible-range bridge outside the .NET Framework
  managed provider array boundary; retain the stable empty fallback until range
  handles survive both in-process and external MTA clients.
- Add compact `box-sizing` state and exact content-box/border-box geometry across
  blocks, flex items, fields, and tables with narrow reflow coverage.
