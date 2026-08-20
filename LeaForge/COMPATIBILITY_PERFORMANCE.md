# Lea Compatibility and Performance Report

- Generated: **2026-08-20 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 674.895 ms |
| Clean process + engine + first page p95 | 767.800 ms |
| Local page load median | 1.777 ms |
| Local page load p95 | 6.225 ms |
| Layout + bitmap paint median | 50.180 ms |
| Layout + bitmap paint p95 | 193.751 ms |
| Retained managed memory | 1891.2 KB for 80 loaded pages |
| Retained managed memory per page | 23.6 KB |
| Engine runtime DLLs | 200.0 KB |
| Engine runtime GZip estimate | 88.3 KB (44.2% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 1.321 ms | 1.968 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.488 ms | 2.689 ms |
| `html-tokens.html` | 943 | 15 | 2 | 1.612 ms | 2.858 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.579 ms | 6.754 ms |
| `layout.html` | 616 | 9 | 4 | 0.949 ms | 1.466 ms |
| `tables.html` | 894 | 28 | 6 | 1.629 ms | 6.808 ms |
| `forms.html` | 1566 | 25 | 7 | 2.205 ms | 2.946 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.423 ms | 7.080 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 2.028 ms | 8.233 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 4.270 ms | 22.544 ms |
| `external.html` | 502 | 12 | 6 | 2.469 ms | 7.826 ms |
| `base-url.html` | 819 | 17 | 5 | 1.709 ms | 3.863 ms |
| `class-commands.html` | 1065 | 15 | 4 | 1.292 ms | 7.630 ms |
| `go-command.html` | 971 | 12 | 6 | 1.232 ms | 2.046 ms |
| `lists.html` | 1849 | 39 | 5 | 2.617 ms | 3.661 ms |
| `disclosure.html` | 2270 | 34 | 9 | 2.849 ms | 8.800 ms |

## Executable Probes

| Feature group | Result | Evidence |
| --- | --- | --- |
| HTML tree and recovery | PASS | Recovered tree, structured table, raw text, RCDATA, and entities |
| Bounded list markers | PASS | Unordered, start/value, nested negative, saturated, inline, and painted markers |
| Bounded disclosures | PASS | Nested open state, retained fields, compact markers, host/view toggles, and painted content |
| CSS selectors and cascade | PASS | 10 compiled rules across 12 blocks |
| Bounded document queries | PASS | Id and first match; 3 ordered matches; invalid selector rejected |
| Document base URLs | PASS | First valid Bu resolves styles, links, images, forms, and downloads while U stays unchanged |
| Layout and painting | PASS | 67 boxes, 702 px document, 990 colored samples |
| Forms and validation | PASS | 31 fields across basic, validation, and richer-widget fixtures |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Host-controlled go requests | PASS | Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected |
| Bounded network timeout | PASS | Tm defaults to 15000 ms and clamps to 100-60000 ms |
| Bounded class mutations | PASS | ac/rc/tc restyle compiled CSS with 32 classes and 512 class-text characters bounded |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, list markers, details disclosures, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, and responsive image sizing |
| Layout and painting | Partial | Custom WinForms block, inline, table, form, image, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, color, font, alert, variable, text, class mutation, host navigation request, and safe click commands |
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
| `Lea.Engine.dll` | 126464 | 123.5 KB |
| `Lea.Win.dll` | 78336 | 76.5 KB |
| `Lea.Win.Test.exe` | 393216 | 384.0 KB |
| `Lea.Bench.exe` | 37888 | 37.0 KB |
| `Firespider Internet 19.exe` | 1108992 | 1083.0 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
