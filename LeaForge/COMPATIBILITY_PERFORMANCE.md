# Lea Compatibility and Performance Report

- Generated: **2026-08-25 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 416.975 ms |
| Clean process + engine + first page p95 | 441.283 ms |
| Local page load median | 2.181 ms |
| Local page load p95 | 5.509 ms |
| Layout + bitmap paint median | 31.143 ms |
| Layout + bitmap paint p95 | 84.897 ms |
| Retained managed memory | 6480.2 KB for 170 loaded pages |
| Retained managed memory per page | 38.1 KB |
| Engine runtime DLLs | 351.5 KB |
| Engine runtime GZip estimate | 149.8 KB (42.6% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 970 | 17 | 1 | 2.492 ms | 3.012 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.597 ms | 2.278 ms |
| `html-tokens.html` | 943 | 15 | 2 | 1.587 ms | 3.048 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.613 ms | 2.978 ms |
| `selector-functions.html` | 2586 | 23 | 18 | 3.002 ms | 4.120 ms |
| `selector-arithmetic.html` | 1955 | 25 | 18 | 2.583 ms | 4.389 ms |
| `selector-filtered-position.html` | 2375 | 25 | 13 | 2.976 ms | 4.070 ms |
| `selector-relational.html` | 2027 | 28 | 15 | 2.188 ms | 9.605 ms |
| `layout.html` | 616 | 9 | 4 | 0.714 ms | 1.897 ms |
| `tables.html` | 894 | 28 | 6 | 1.614 ms | 2.236 ms |
| `forms.html` | 1566 | 25 | 7 | 1.565 ms | 2.101 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.515 ms | 2.349 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.836 ms | 3.208 ms |
| `media-queries.html` | 2229 | 24 | 16 | 2.297 ms | 3.187 ms |
| `custom-properties.html` | 2890 | 23 | 17 | 2.536 ms | 5.491 ms |
| `important-cascade.html` | 2748 | 19 | 16 | 2.034 ms | 4.935 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 3.587 ms | 5.772 ms |
| `external.html` | 502 | 12 | 6 | 1.184 ms | 1.582 ms |
| `base-url.html` | 819 | 17 | 5 | 1.241 ms | 1.742 ms |
| `class-commands.html` | 1169 | 15 | 4 | 1.375 ms | 1.807 ms |
| `go-command.html` | 971 | 12 | 6 | 1.491 ms | 1.803 ms |
| `lists.html` | 1849 | 39 | 5 | 2.553 ms | 7.273 ms |
| `disclosure.html` | 2270 | 34 | 9 | 2.297 ms | 3.065 ms |
| `fragments.html` | 2234 | 31 | 11 | 2.535 ms | 3.710 ms |
| `gauges.html` | 2115 | 25 | 14 | 2.124 ms | 3.128 ms |
| `values.html` | 1734 | 26 | 7 | 1.996 ms | 2.390 ms |
| `time-edits.html` | 1966 | 27 | 7 | 2.102 ms | 6.489 ms |
| `image-maps.html` | 2569 | 30 | 6 | 2.294 ms | 2.965 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 3.280 ms | 3.699 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 1.647 ms | 2.119 ms |
| `accessibility.html` | 3148 | 60 | 4 | 2.613 ms | 3.348 ms |
| `state-commands.html` | 2711 | 37 | 9 | 4.589 ms | 8.982 ms |
| `flex.html` | 6942 | 57 | 57 | 7.989 ms | 9.280 ms |
| `box-sizing.html` | 2744 | 20 | 13 | 2.498 ms | 5.509 ms |

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
| Bounded selector functions | PASS | eight alternatives, sixteen tests, one-level compound arguments, forgiving is/where, strict not, maximum/zero specificity, outer combinators, queries, dynamic state, and retained fields |
| Bounded structural arithmetic | PASS | precompiled A/B integers, positive/negative/zero formulas, forward/reverse child and type positions, 32-character and 8192-value bounds, specificity, queries, selector-list composition, and geometry |
| Bounded filtered positions | PASS | strict compound of-S lists, eight alternatives, sixteen total tests, maximum filter specificity, forward/reverse counting, form-state filters, queries, invalid nesting/type rejection, and geometry |
| Bounded relational selectors | PASS | strict precompiled relative lists, descendant/child/adjacent/following relationships, four-part chains, eight alternatives, sixteen total parts, 4096 candidate visits, maximum specificity, dynamic focus, queries, retained fields, and geometry |
| Bounded media queries | PASS | Vw/P.Vw/G.Vw aliases, min/max/exact/comma/nested screen queries, style media, 1-8192 px bounds, live resize, diagnostics, and retained fields |
| Bounded CSS custom properties | PASS | S.Vc/Cv aliases, 32 properties, 64-character names, 512-character values, eight-level resolution, case-sensitive inheritance, nested/cycle fallbacks, inline/hover/media rebuilds, and retained fields |
| Bounded important cascade | PASS | G.Ic diagnostics, stylesheet/inline priority tiers, specificity/source order, shorthands, custom properties, nested marker boundaries, hover/media rebuilds, and retained fields |
| Bounded document queries | PASS | Id and first match; 3 ordered matches; invalid selector rejected |
| Document base URLs | PASS | First valid Bu resolves styles, links, images, forms, and downloads while U stays unchanged |
| Layout and painting | PASS | 67 boxes, 702 px document, 990 colored samples |
| Bounded box sizing | PASS | Bz c/b across width, height, min/max, flex-basis, fields, tables, and 180 px reflow with fixture-backed paint and hits |
| Bounded flex layout | PASS | Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or aliases, stable row order, column/reverse flow, around/evenly distribution, item alignment, fixed, auto, wrapped, growing, shrinking, protected, and locked slots, hits, resize, and following flow |
| Forms and validation | PASS | 31 fields across basic, validation, and richer-widget fixtures |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Host-controlled go requests | PASS | Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected |
| Bounded network timeout | PASS | Tm defaults to 15000 ms and clamps to 100-60000 ms |
| Bounded persistent cache | PASS | Cd opt-in; 32 entries/16 MB defaults; restart hit; ETag disk304; Dc/Db/Dh diagnostics |
| Bounded class mutations | PASS | ac/rc/tc plus one/two-argument bg/fg/fs restyle compiled CSS with bounded IDs, colors, 8-96 px fonts, 32 classes, and retained fields |
| Bounded element-state commands | PASS | hd/sh, ds/en, vl, ck, fo, op/cl/tg, retained fields, one pending focus target, and file-value rejection |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| Optional persistent cache | Supported | Memory-only default, gzip entries, LRU entry/byte ceilings, validators, private isolation, and corrupt-entry recovery |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, lists, disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled relationship, bounded strict has, is/where/list-not, An+B/reverse structural, and compound of-S filtered selectors, declarations with two-tier important cascade, inherited custom properties and var() fallback, screen/all pixel-width media queries, content/border box and text styles, visibility, positioning, transforms, responsive image sizing, and bounded row/column flex properties |
| Layout and painting | Partial | Custom WinForms content/border box, inline, row/column flex, table, form, image, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, body/ID-scoped color and font, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, and safe click commands |
| General JavaScript and DOM APIs | Not supported | Lea deliberately does not execute arbitrary JavaScript |
| Flexbox | Supported subset | Row/row-reverse and fixed-height column/column-reverse containers, content/border-box pixel bases, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee |
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
| `Lea.Engine.dll` | 194560 | 190.0 KB |
| `Lea.Win.dll` | 165376 | 161.5 KB |
| `Lea.Win.Test.exe` | 394752 | 385.5 KB |
| `Lea.Bench.exe` | 111616 | 109.0 KB |
| `Firespider Internet 19.exe` | 1110528 | 1084.5 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
