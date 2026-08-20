# Lea Compatibility and Performance Report

- Generated: **2026-08-20 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 427.546 ms |
| Clean process + engine + first page p95 | 470.338 ms |
| Local page load median | 1.188 ms |
| Local page load p95 | 2.844 ms |
| Layout + bitmap paint median | 31.170 ms |
| Layout + bitmap paint p95 | 88.775 ms |
| Retained managed memory | 1322.0 KB for 60 loaded pages |
| Retained managed memory per page | 22.0 KB |
| Engine runtime DLLs | 192.0 KB |
| Engine runtime GZip estimate | 84.7 KB (44.1% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 1.226 ms | 2.073 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.298 ms | 2.492 ms |
| `html-tokens.html` | 943 | 15 | 2 | 0.758 ms | 1.876 ms |
| `selectors.html` | 1017 | 20 | 10 | 0.751 ms | 2.357 ms |
| `layout.html` | 616 | 9 | 4 | 1.108 ms | 4.413 ms |
| `tables.html` | 894 | 28 | 6 | 1.390 ms | 2.300 ms |
| `forms.html` | 1566 | 25 | 7 | 1.223 ms | 3.082 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.128 ms | 2.144 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.331 ms | 2.406 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 2.521 ms | 5.949 ms |
| `external.html` | 502 | 12 | 6 | 0.956 ms | 2.744 ms |
| `base-url.html` | 819 | 17 | 5 | 1.049 ms | 2.711 ms |

## Executable Probes

| Feature group | Result | Evidence |
| --- | --- | --- |
| HTML tree and recovery | PASS | Recovered tree, structured table, raw text, RCDATA, and entities |
| CSS selectors and cascade | PASS | 10 compiled rules across 12 blocks |
| Bounded document queries | PASS | Id and first match; 3 ordered matches; invalid selector rejected |
| Document base URLs | PASS | First valid Bu resolves styles, links, images, forms, and downloads while U stays unchanged |
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
| Document queries | Supported subset | Id, first-match Q/Q1, and read-only Qa with compiled selectors and a 256-result ceiling |
| Document base URLs | Supported | First valid bounded HTTP, HTTPS, or file base; U remains the response URL and Bu drives relative resources |
| Accessibility | Partial | Keyboard focus and native fields are present; a complete accessibility tree is not |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 119808 | 117.0 KB |
| `Lea.Win.dll` | 76800 | 75.0 KB |
| `Lea.Win.Test.exe` | 392192 | 383.0 KB |
| `Lea.Bench.exe` | 31744 | 31.0 KB |
| `Firespider Internet 19.exe` | 1102336 | 1076.5 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
