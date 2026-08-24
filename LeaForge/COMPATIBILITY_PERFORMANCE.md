# Lea Compatibility and Performance Report

- Generated: **2026-08-24 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 424.309 ms |
| Clean process + engine + first page p95 | 459.810 ms |
| Local page load median | 2.118 ms |
| Local page load p95 | 5.019 ms |
| Layout + bitmap paint median | 28.177 ms |
| Layout + bitmap paint p95 | 91.183 ms |
| Retained managed memory | 4006.8 KB for 125 loaded pages |
| Retained managed memory per page | 32.1 KB |
| Engine runtime DLLs | 311.5 KB |
| Engine runtime GZip estimate | 132.6 KB (42.6% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 2.198 ms | 6.641 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.226 ms | 2.177 ms |
| `html-tokens.html` | 943 | 15 | 2 | 1.476 ms | 3.093 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.149 ms | 1.768 ms |
| `layout.html` | 616 | 9 | 4 | 0.710 ms | 0.841 ms |
| `tables.html` | 894 | 28 | 6 | 1.734 ms | 7.675 ms |
| `forms.html` | 1566 | 25 | 7 | 1.727 ms | 4.888 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.645 ms | 2.147 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 2.445 ms | 2.895 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 4.789 ms | 5.939 ms |
| `external.html` | 502 | 12 | 6 | 1.453 ms | 1.913 ms |
| `base-url.html` | 819 | 17 | 5 | 1.931 ms | 5.624 ms |
| `class-commands.html` | 1065 | 15 | 4 | 1.721 ms | 2.493 ms |
| `go-command.html` | 971 | 12 | 6 | 1.998 ms | 2.802 ms |
| `lists.html` | 1849 | 39 | 5 | 2.507 ms | 3.242 ms |
| `disclosure.html` | 2270 | 34 | 9 | 3.229 ms | 4.176 ms |
| `fragments.html` | 2234 | 31 | 11 | 3.027 ms | 8.291 ms |
| `gauges.html` | 2115 | 25 | 14 | 2.137 ms | 2.773 ms |
| `values.html` | 1734 | 26 | 7 | 2.085 ms | 2.661 ms |
| `time-edits.html` | 1966 | 27 | 7 | 2.330 ms | 3.074 ms |
| `image-maps.html` | 2569 | 30 | 6 | 2.590 ms | 5.044 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 4.705 ms | 5.662 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 1.843 ms | 2.181 ms |
| `accessibility.html` | 3148 | 60 | 4 | 2.796 ms | 7.214 ms |
| `state-commands.html` | 2711 | 37 | 9 | 4.047 ms | 6.994 ms |

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
| Bounded responsive image maps | PASS | Mp/Ar aliases, rect/circle/poly/default and dead areas, 32 maps, 64 areas, 128 coordinates, responsive scaling, rebuilds, and paint |
| Bounded keyboard focus | PASS | Ti and Ke/Kn/Ki/Kb/Kf/Ka aliases, positive/zero/default/negative order, links, areas, fields, summaries, focus rings, scrolling, and 256-target bounds |
| Bounded focus shortcuts | PASS | Af/Afs and Ak/Aks/An/Kl/Aa aliases, first usable autofocus, Alt+Shift assignment, conflicts, exclusions, focus, activation, and fragment suppression |
| Bounded semantic accessibility | PASS | Ax/Axs/St/Is and V.Ax/Axs/Ab/Ao aliases, 512 records, names, roles, live protected state, bounds, MSAA children, UIA fragments, control types, grid/table coordinates, headers, logical text, visual lines, search, redaction, and patterns |
| CSS selectors and cascade | PASS | 10 compiled rules across 12 blocks |
| Bounded document queries | PASS | Id and first match; 3 ordered matches; invalid selector rejected |
| Document base URLs | PASS | First valid Bu resolves styles, links, images, forms, and downloads while U stays unchanged |
| Layout and painting | PASS | 67 boxes, 702 px document, 990 colored samples |
| Forms and validation | PASS | 31 fields across basic, validation, and richer-widget fixtures |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Host-controlled go requests | PASS | Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected |
| Bounded network timeout | PASS | Tm defaults to 15000 ms and clamps to 100-60000 ms |
| Bounded class mutations | PASS | ac/rc/tc restyle compiled CSS with 32 classes and 512 class-text characters bounded |
| Bounded element-state commands | PASS | hd/sh, ds/en, vl, ck, fo, op/cl/tg, retained fields, one pending focus target, and file-value rejection |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, lists, disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, and responsive image sizing |
| Layout and painting | Partial | Custom WinForms block, inline, table, form, image, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, color, font, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, and safe click commands |
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
| Image maps | Supported subset | Compact Mp/Ar models, rect/circle/poly/default and dead areas, bounded coordinates, scaled custom hit regions, area links/downloads, and safe rebuilds |
| Keyboard focus | Supported subset | Bounded tabindex order, links, image-map areas, fields, summaries, visible focus, scrolling, wrapping, and host activation aliases |
| Focus shortcuts | Supported subset | Bounded autofocus candidates and ASCII access-key alternatives with conflict resolution, Alt+Shift labels, host-safe matching, focus, and activation |
| Accessibility | Supported subset | Bounded Ax hierarchy with MSAA and native UI Automation fragments, roles, names, live protected state, rendered bounds, focus, invoke/value/toggle/selection/disclosure/range/scroll and Tb/Tc-backed grid/table/item patterns, plus read-only logical TextPattern document/child/point ranges with visual-line and partial-range geometry; visible-range arrays use an empty fallback, and this is not the complete platform mapping standards |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 162816 | 159.0 KB |
| `Lea.Win.dll` | 156160 | 152.5 KB |
| `Lea.Win.Test.exe` | 394752 | 385.5 KB |
| `Lea.Bench.exe` | 69120 | 67.5 KB |
| `Firespider Internet 19.exe` | 1109504 | 1083.5 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
