# Lea 1.0 Progress

Current completion: **35%**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 8% | Compact API, history, projects, backups |
| Networking | 10% | 3% | HTTP/local loading, async navigation, stop |
| HTML and document tree | 20% | 6% | Tolerant nodes, text, attributes, basic blocks |
| CSS | 15% | 4% | Cascade, descendants, inheritance, colors, fonts, display |
| Layout and painting | 20% | 9% | Cached vertical boxes, reflow, visible painting, bounded scrolling |
| Resources, forms, and media | 10% | 2% | Async images, scaling, cache, placeholders |
| Lea scripting | 5% | 1% | Early compact commands in LeaForge |
| Firespider integration and reliability | 10% | 2% | Compatible DLLs, test host, integration map |
| **Total** | **100%** | **35%** | |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Next gains

- Forms and text input: +3%
- Firespider experimental Lea tab: +3%
