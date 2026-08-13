# Lea 1.0 Progress

Current completion: **58%**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 8% | Compact API, history, projects, backups |
| Networking | 10% | 8% | Bounded private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 7% | Tolerant nodes, text, attributes, blocks, structured tables |
| CSS | 15% | 7% | Linked/inline cascade, inheritance, text alignment/spacing, colors, fonts, borders, sizing |
| Layout and painting | 20% | 13% | Inline emphasis/link flow, structured tables, box sizing, reflow, painting, scrolling |
| Resources, forms, and media | 10% | 7% | Images, painted fields, native select menus, reset, GET/POST submit |
| Lea scripting | 5% | 1% | Early compact commands in LeaForge |
| Firespider integration and reliability | 10% | 7% | Navigation, safe close routing, Lea sessions, source viewing, host smoke tests |
| **Total** | **100%** | **58%** | |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Next gains

- More block tags and malformed nesting recovery: +1%
- Basic `white-space` and overflow behavior: +1%
