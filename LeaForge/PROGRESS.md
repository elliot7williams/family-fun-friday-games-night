# Lea 1.0 Progress

Current completion: **74%**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 8% | Compact API, history, projects, backups |
| Networking | 10% | 8% | Bounded private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 8% | Tolerant nodes, malformed nesting recovery, semantic blocks, structured tables |
| CSS | 15% | 15% | Compiled selectors, dynamic hover/focus, sizing, visibility/opacity, fonts, overflow, boxes |
| Layout and painting | 20% | 17% | Auto-height visual containers, nested positioned contexts, clipping, stacking, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive images, GET/POST submit |
| Lea scripting | 5% | 1% | Early compact commands in LeaForge |
| Firespider integration and reliability | 10% | 7% | Navigation, safe close routing, Lea sessions, source viewing, host smoke tests |
| **Total** | **100%** | **74%** | |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Next gains

- Expanded CSS stacking-context triggers and auto-sized positioned containers: +1%
- Nested form, media, and table contexts in ordinary visual containers: +1%
