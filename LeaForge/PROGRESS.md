# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **9**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled selectors, dynamic hover/focus, sizing, visibility/opacity, fonts, overflow, boxes, bounded flex-row properties |
| Layout and painting | 20% | 20% | Flex rows, inset stretch, translated contexts, nested content, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive images, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, variables, text/title/style/alert, class, visibility, disclosure, field-state, and focus commands |
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

## Next gains

- Add a native-safe non-empty visible-range bridge outside the .NET Framework
  managed provider array boundary; retain the stable empty fallback until range
  handles survive both in-process and external MTA clients.
- Expand safe page commands only through bounded, fixture-backed aliases.
