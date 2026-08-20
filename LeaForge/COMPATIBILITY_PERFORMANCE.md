# Lea Compatibility and Performance Report

- Generated: **2026-08-20 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 452.784 ms |
| Clean process + engine + first page p95 | 456.078 ms |
| Local page load median | 1.468 ms |
| Local page load p95 | 3.805 ms |
| Layout + bitmap paint median | 34.611 ms |
| Layout + bitmap paint p95 | 89.311 ms |
| Retained managed memory | 1235.7 KB for 55 loaded pages |
| Retained managed memory per page | 22.5 KB |
| Engine runtime DLLs | 189.5 KB |
| Engine runtime GZip estimate | 83.6 KB (44.1% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 1.303 ms | 2.727 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.497 ms | 1.941 ms |
| `html-tokens.html` | 943 | 15 | 2 | 1.352 ms | 1.932 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.565 ms | 3.003 ms |
| `layout.html` | 616 | 9 | 4 | 0.934 ms | 1.159 ms |
| `tables.html` | 894 | 28 | 6 | 1.242 ms | 1.641 ms |
| `forms.html` | 1566 | 25 | 7 | 1.847 ms | 2.563 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.278 ms | 1.938 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.938 ms | 3.174 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 3.993 ms | 4.487 ms |
| `external.html` | 502 | 12 | 6 | 1.937 ms | 3.217 ms |

## Executable Probes

| Feature group | Result | Evidence |
| --- | --- | --- |
| HTML tree and recovery | PASS | Recovered tree, structured table, raw text, RCDATA, and entities |
| CSS selectors and cascade | PASS | 10 compiled rules across 12 blocks |
| Layout and painting | PASS | 67 boxes, 702 px document, 990 colored samples |
| Forms and validation | PASS | 31 fields across basic, validation, and richer-widget fixtures |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, and responsive image sizing |
| Layout and painting | Partial | Custom WinForms block, inline, table, form, image, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, color, font, alert, variable, text, and safe click commands |
| General JavaScript and DOM APIs | Not supported | Lea deliberately does not execute arbitrary JavaScript |
| Flexbox, Grid, Canvas, SVG | Not supported | Future compatibility work; no silent fallback claim |
| Audio and video | Not supported | Future optional module |
| Developer diagnostics | Supported | Bounded summary, node, CSS, request, message, and variable snapshots |
| Accessibility | Partial | Keyboard focus and native fields are present; a complete accessibility tree is not |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 117760 | 115.0 KB |
| `Lea.Win.dll` | 76288 | 74.5 KB |
| `Lea.Win.Test.exe` | 392192 | 383.0 KB |
| `Lea.Bench.exe` | 29184 | 28.5 KB |
| `Firespider Internet 19.exe` | 1102336 | 1076.5 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
