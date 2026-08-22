# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **2**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled selectors, dynamic hover/focus, sizing, visibility/opacity, fonts, overflow, boxes |
| Layout and painting | 20% | 20% | Inset stretch, translated contexts, nested content, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive images, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, variables, text/title/style/alert and class commands |
| Firespider integration and reliability | 10% | 10% | Lea-first New Tab/Ctrl+T, explicit legacy fallback, Inspector with Access view, MSAA surface, host-safe focus shortcuts, navigation, sessions, source, host smokes |
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

## Next gains

- Expand safe page commands only through bounded, fixture-backed aliases.
- Continue platform accessibility mapping, compatibility, performance, and
  Firespider migration work.
