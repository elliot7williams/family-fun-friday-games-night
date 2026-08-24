# Lea Compatibility and Performance Report

- Generated: **2026-08-24 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 439.466 ms |
| Clean process + engine + first page p95 | 676.922 ms |
| Local page load median | 2.378 ms |
| Local page load p95 | 6.956 ms |
| Layout + bitmap paint median | 41.118 ms |
| Layout + bitmap paint p95 | 128.716 ms |
| Retained managed memory | 4627.2 KB for 130 loaded pages |
| Retained managed memory per page | 35.6 KB |
| Engine runtime DLLs | 324.5 KB |
| Engine runtime GZip estimate | 137.9 KB (42.5% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 2.371 ms | 3.672 ms |
| `recovery.html` | 1040 | 39 | 0 | 2.179 ms | 3.142 ms |
| `html-tokens.html` | 943 | 15 | 2 | 2.413 ms | 6.545 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.887 ms | 2.685 ms |
| `layout.html` | 616 | 9 | 4 | 1.244 ms | 1.468 ms |
| `tables.html` | 894 | 28 | 6 | 1.470 ms | 2.911 ms |
| `forms.html` | 1566 | 25 | 7 | 1.725 ms | 2.405 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.772 ms | 4.945 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.743 ms | 6.886 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 5.131 ms | 7.016 ms |
| `external.html` | 502 | 12 | 6 | 1.582 ms | 2.448 ms |
| `base-url.html` | 819 | 17 | 5 | 1.627 ms | 2.538 ms |
| `class-commands.html` | 1169 | 15 | 4 | 2.135 ms | 8.214 ms |
| `go-command.html` | 971 | 12 | 6 | 2.230 ms | 4.061 ms |
| `lists.html` | 1849 | 39 | 5 | 3.471 ms | 40.612 ms |
| `disclosure.html` | 2270 | 34 | 9 | 3.722 ms | 4.972 ms |
| `fragments.html` | 2234 | 31 | 11 | 2.506 ms | 3.274 ms |
| `gauges.html` | 2115 | 25 | 14 | 2.816 ms | 4.287 ms |
| `values.html` | 1734 | 26 | 7 | 1.404 ms | 1.858 ms |
| `time-edits.html` | 1966 | 27 | 7 | 2.619 ms | 5.446 ms |
| `image-maps.html` | 2569 | 30 | 6 | 2.322 ms | 7.539 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 4.759 ms | 5.797 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 1.943 ms | 3.609 ms |
| `accessibility.html` | 3148 | 60 | 4 | 4.102 ms | 6.116 ms |
| `state-commands.html` | 2711 | 37 | 9 | 6.064 ms | 6.712 ms |
| `flex.html` | 6942 | 57 | 57 | 9.718 ms | 15.704 ms |

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
| Bounded flex layout | PASS | Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or aliases, stable row order, column/reverse flow, around/evenly distribution, item alignment, fixed, auto, wrapped, growing, shrinking, protected, and locked slots, hits, resize, and following flow |
| Forms and validation | PASS | 31 fields across basic, validation, and richer-widget fixtures |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Host-controlled go requests | PASS | Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected |
| Bounded network timeout | PASS | Tm defaults to 15000 ms and clamps to 100-60000 ms |
| Bounded class mutations | PASS | ac/rc/tc plus one/two-argument bg/fg/fs restyle compiled CSS with bounded IDs, colors, 8-96 px fonts, 32 classes, and retained fields |
| Bounded element-state commands | PASS | hd/sh, ds/en, vl, ck, fo, op/cl/tg, retained fields, one pending focus target, and file-value rejection |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, lists, disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, responsive image sizing, and bounded row/column flex properties |
| Layout and painting | Partial | Custom WinForms block, inline, row/column flex, table, form, image, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, body/ID-scoped color and font, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, and safe click commands |
| General JavaScript and DOM APIs | Not supported | Lea deliberately does not execute arbitrary JavaScript |
| Flexbox | Supported subset | Row/row-reverse and fixed-height column/column-reverse containers, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, basis, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee |
| Grid, Canvas, SVG | Not supported | Future compatibility work; no silent fallback claim |
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
| `Lea.Engine.dll` | 168448 | 164.5 KB |
| `Lea.Win.dll` | 163840 | 160.0 KB |
| `Lea.Win.Test.exe` | 394752 | 385.5 KB |
| `Lea.Bench.exe` | 77824 | 76.0 KB |
| `Firespider Internet 19.exe` | 1109504 | 1083.5 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
