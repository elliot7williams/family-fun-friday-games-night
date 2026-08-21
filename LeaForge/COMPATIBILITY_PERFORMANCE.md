# Lea Compatibility and Performance Report

- Generated: **2026-08-21 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 305.356 ms |
| Clean process + engine + first page p95 | 312.497 ms |
| Local page load median | 1.134 ms |
| Local page load p95 | 2.355 ms |
| Layout + bitmap paint median | 25.957 ms |
| Layout + bitmap paint p95 | 102.738 ms |
| Retained managed memory | 2601.7 KB for 100 loaded pages |
| Retained managed memory per page | 26.0 KB |
| Engine runtime DLLs | 219.0 KB |
| Engine runtime GZip estimate | 95.9 KB (43.8% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 0.641 ms | 1.265 ms |
| `recovery.html` | 1040 | 39 | 0 | 0.752 ms | 1.152 ms |
| `html-tokens.html` | 943 | 15 | 2 | 0.748 ms | 1.003 ms |
| `selectors.html` | 1017 | 20 | 10 | 0.794 ms | 1.417 ms |
| `layout.html` | 616 | 9 | 4 | 0.559 ms | 0.768 ms |
| `tables.html` | 894 | 28 | 6 | 0.816 ms | 1.302 ms |
| `forms.html` | 1566 | 25 | 7 | 1.111 ms | 1.777 ms |
| `events-vars.html` | 1132 | 14 | 5 | 0.743 ms | 1.651 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.063 ms | 1.474 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 2.325 ms | 3.845 ms |
| `external.html` | 502 | 12 | 6 | 1.253 ms | 1.726 ms |
| `base-url.html` | 819 | 17 | 5 | 1.228 ms | 1.699 ms |
| `class-commands.html` | 1065 | 15 | 4 | 0.998 ms | 1.325 ms |
| `go-command.html` | 971 | 12 | 6 | 1.041 ms | 1.491 ms |
| `lists.html` | 1849 | 39 | 5 | 1.351 ms | 2.166 ms |
| `disclosure.html` | 2270 | 34 | 9 | 1.950 ms | 2.683 ms |
| `fragments.html` | 2234 | 31 | 11 | 1.916 ms | 2.838 ms |
| `gauges.html` | 2115 | 25 | 14 | 1.627 ms | 3.203 ms |
| `values.html` | 1734 | 26 | 7 | 1.592 ms | 2.029 ms |
| `time-edits.html` | 1966 | 27 | 7 | 1.785 ms | 2.903 ms |

## Executable Probes

| Feature group | Result | Evidence |
| --- | --- | --- |
| HTML tree and recovery | PASS | Recovered tree, structured table, raw text, RCDATA, and entities |
| Bounded list markers | PASS | Unordered, start/value, nested negative, saturated, inline, and painted markers |
| Bounded disclosures | PASS | Nested open state, retained fields, compact markers, host/view toggles, and painted content |
| Retained fragment navigation | PASS | 128-entry history, zero-fetch page retention, encoded/named targets, hidden reveal, table placement, and resize coverage |
| Bounded progress and meter gauges | PASS | Vm aliases, determinate/indeterminate progress, ordered meter regions, 10^12 numeric ceiling, inherited style, tables, resize, and paint |
| Bounded output and data values | PASS | Dv aliases, 4096-char values, 32 unique references, inline/block/table ownership, 512-char updates, rebuilds, and resize |
| Bounded time and edit metadata | PASS | Dv t/i/r aliases, 128-char datetimes, safe resolved citations, unsafe-scheme rejection, native decoration, ownership, rebuilds, tables, and resize |
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
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, list markers, details disclosures, progress/meter gauges, output/data values, time/edit metadata, tables, and forms; not the full Living Standard |
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
| Same-page fragments | Supported | Bounded Ft/Fg/Rv lookup, retained 128-entry history, details reveal, target scrolling, and no-fetch Back/Forward |
| Progress and meter gauges | Supported | Compact Vm values, finite normalization, meter quality regions, inherited styles, table cells, and custom paint |
| Output and data values | Supported | Compact Dv values, bounded references and text, host aliases, inline/block/table ownership, and safe rebuilds |
| Time and edit metadata | Supported | Compact Dv t/i/r values, bounded datetime and citation aliases, safe schemes, native decorations, inline/block/table ownership, and safe rebuilds |
| Accessibility | Partial | Keyboard focus and native fields are present; a complete accessibility tree is not |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 139776 | 136.5 KB |
| `Lea.Win.dll` | 84480 | 82.5 KB |
| `Lea.Win.Test.exe` | 394240 | 385.0 KB |
| `Lea.Bench.exe` | 50176 | 49.0 KB |
| `Firespider Internet 19.exe` | 1108992 | 1083.0 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
