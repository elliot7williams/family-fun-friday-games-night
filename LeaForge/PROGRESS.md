# Lea 1.0 Progress

Current completion: **53%**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 8% | Compact API, history, projects, backups |
| Networking | 10% | 7% | Typed errors, freshness, ETag/Last-Modified revalidation, compression, LRU cache |
| HTML and document tree | 20% | 7% | Tolerant nodes, text, attributes, blocks, structured tables |
| CSS | 15% | 5% | Cascade, descendants, inheritance, colors, fonts, display, borders, pixel sizing |
| Layout and painting | 20% | 11% | Border/content boxes, table rows/cells, fixed sizing, reflow, painting, scrolling |
| Resources, forms, and media | 10% | 7% | Images, painted fields, native select menus, reset, GET/POST submit |
| Lea scripting | 5% | 1% | Early compact commands in LeaForge |
| Firespider integration and reliability | 10% | 7% | Navigation, safe close routing, Lea sessions, source viewing, host smoke tests |
| **Total** | **100%** | **53%** | |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Next gains

- Nested block content and links inside table cells: +1%
- Cookie storage with clear and private-session behavior: +1%
