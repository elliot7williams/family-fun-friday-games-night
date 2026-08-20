# Lea 1.0 Progress

Current completion: **83%**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 9% | Streamed bounded downloads, private cookies, typed errors, validators, compression, LRU cache |
| HTML and document tree | 20% | 8% | Tolerant nodes, malformed nesting recovery, semantic blocks, structured tables |
| CSS | 15% | 15% | Compiled selectors, dynamic hover/focus, sizing, visibility/opacity, fonts, overflow, boxes |
| Layout and painting | 20% | 20% | Inset stretch, translated contexts, nested content, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Bounded multipart files, richer widgets, validation, responsive images, GET/POST submit |
| Lea scripting | 5% | 3% | Safe click events, 16 bounded variables, text/title/style/alert commands, host enable switch |
| Firespider integration and reliability | 10% | 8% | Keyboard navigation, open/save/send routes, sessions, source, host smoke tests |
| **Total** | **100%** | **83%** | |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Next gains

- Bounded HTML tokenizer with raw-text, comment, and entity fixtures: +2%
