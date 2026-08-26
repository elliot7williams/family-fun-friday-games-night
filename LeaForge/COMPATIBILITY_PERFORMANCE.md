# Lea Compatibility and Performance Report

- Generated: **2026-08-26 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **3 clean-process starts; 3 loads per fixture; 3 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 471.090 ms |
| Clean process + engine + first page p95 | 507.335 ms |
| Local page load median | 2.594 ms |
| Local page load p95 | 8.868 ms |
| Layout + bitmap paint median | 34.074 ms |
| Layout + bitmap paint p95 | 90.525 ms |
| Retained managed memory | 8654.9 KB for 188 loaded pages |
| Retained managed memory per page | 46.0 KB |
| Engine runtime DLLs | 408.5 KB |
| Engine runtime GZip estimate | 173.6 KB (42.5% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 1032 | 19 | 1 | 2.777 ms | 2.852 ms |
| `recovery.html` | 1040 | 39 | 0 | 1.904 ms | 1.933 ms |
| `html-tokens.html` | 943 | 15 | 2 | 1.344 ms | 1.353 ms |
| `selectors.html` | 1017 | 20 | 10 | 1.141 ms | 1.201 ms |
| `selector-functions.html` | 2586 | 23 | 18 | 2.583 ms | 2.960 ms |
| `selector-arithmetic.html` | 1955 | 25 | 18 | 3.058 ms | 3.297 ms |
| `selector-filtered-position.html` | 2375 | 25 | 13 | 5.329 ms | 12.895 ms |
| `selector-relational.html` | 2027 | 28 | 15 | 2.552 ms | 5.864 ms |
| `layout.html` | 616 | 9 | 4 | 0.794 ms | 0.931 ms |
| `tables.html` | 894 | 28 | 6 | 2.036 ms | 2.138 ms |
| `forms.html` | 1566 | 25 | 7 | 2.331 ms | 3.411 ms |
| `events-vars.html` | 1132 | 14 | 5 | 1.836 ms | 2.145 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 1.904 ms | 2.251 ms |
| `media-queries.html` | 2229 | 24 | 16 | 1.883 ms | 2.026 ms |
| `custom-properties.html` | 2890 | 23 | 17 | 3.382 ms | 3.972 ms |
| `important-cascade.html` | 2748 | 19 | 16 | 2.811 ms | 3.200 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 3.811 ms | 4.541 ms |
| `external.html` | 502 | 12 | 6 | 1.430 ms | 1.651 ms |
| `base-url.html` | 819 | 17 | 5 | 1.242 ms | 1.350 ms |
| `class-commands.html` | 1169 | 15 | 4 | 1.318 ms | 1.830 ms |
| `go-command.html` | 971 | 12 | 6 | 2.441 ms | 2.472 ms |
| `lists.html` | 1849 | 39 | 5 | 2.341 ms | 2.807 ms |
| `disclosure.html` | 2270 | 34 | 9 | 2.690 ms | 2.707 ms |
| `fragments.html` | 2234 | 31 | 11 | 2.248 ms | 2.474 ms |
| `gauges.html` | 2115 | 25 | 14 | 2.509 ms | 2.895 ms |
| `values.html` | 1734 | 26 | 7 | 1.872 ms | 9.709 ms |
| `time-edits.html` | 1966 | 27 | 7 | 2.504 ms | 5.582 ms |
| `image-maps.html` | 2569 | 30 | 6 | 1.741 ms | 1.760 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 4.084 ms | 5.671 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 2.496 ms | 2.594 ms |
| `accessibility.html` | 3148 | 60 | 4 | 4.335 ms | 4.768 ms |
| `state-commands.html` | 2711 | 37 | 9 | 4.852 ms | 4.910 ms |
| `flex.html` | 6942 | 57 | 57 | 9.379 ms | 13.769 ms |
| `grid.html` | 5699 | 62 | 37 | 8.342 ms | 8.895 ms |
| `grid-areas.html` | 3189 | 31 | 20 | 4.324 ms | 4.989 ms |
| `grid-lines.html` | 5342 | 45 | 31 | 7.362 ms | 14.363 ms |
| `grid-flow.html` | 4271 | 45 | 38 | 5.542 ms | 8.868 ms |
| `grid-auto-tracks.html` | 4332 | 41 | 25 | 5.287 ms | 5.746 ms |
| `grid-shorthand.html` | 5060 | 48 | 33 | 6.864 ms | 8.214 ms |
| `grid-alignment.html` | 3129 | 27 | 23 | 3.610 ms | 3.940 ms |
| `grid-content-alignment.html` | 2723 | 37 | 18 | 3.956 ms | 4.636 ms |
| `svg.html` | 1775 | 25 | 4 | 2.248 ms | 2.319 ms |
| `svg-geometry.html` | 2470 | 31 | 2 | 2.472 ms | 3.125 ms |
| `svg-gradients.html` | 3238 | 43 | 3 | 2.519 ms | 2.977 ms |
| `svg-use-clips.html` | 2947 | 48 | 3 | 2.904 ms | 3.354 ms |
| `svg-strokes.html` | 2832 | 28 | 5 | 2.669 ms | 3.028 ms |
| `box-sizing.html` | 2744 | 20 | 13 | 3.050 ms | 10.177 ms |

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
| Bounded grid layout | PASS | Gd/Gc/Gv columns, Rd/Rc/Rv rows, Gl/Gs and Rl/Rs two-axis placement, bounded px/fr/repeat/minmax tracks, gaps, spans, occupancy, paint, resize, and flow |
| Bounded named grid areas | PASS | Aa/Ar/Ac/Ag area templates plus An item names, rectangular validation, dot cells, definite-first placement, implicit columns, bounds, paint, hits, and flow |
| Bounded named grid lines | PASS | Gnc/Rnc/Gx/Rx templates plus Gn/Ge/Rn/Re item names, area-generated edges, unique validation, repeats, definite-first placement, bounds, paint, hits, and flow |
| Bounded grid auto flow | PASS | Gf r/d/c/x flow, sparse cursors, dense backfill, row/column swapping, equal implicit columns, eight-column overflow, bounds, paint, hits, and flow |
| Bounded grid auto tracks | PASS | S.Av plus Gac/Gav/Rac/Rav repeating pixel, auto, integer-fr, and minmax patterns; implicit endpoints, area tracks, row settlement, validation, bounds, paint, hits, and flow |
| Bounded grid shorthands | PASS | Atomic grid-template/grid explicit and quoted-area forms, row/column auto-flow dense forms, automatic patterns, reset boundaries, explicit auto tracks, geometry, paint, hits, and flow |
| Bounded grid item alignment | PASS | Ji/Js plus Ai/As start, center, end, and stretch defaults/overrides; atomic place-items/place-self, auto fit, exact geometry, paint, hits, and flow |
| Bounded grid content alignment | PASS | Al/Jc start, center, end, between, around, and evenly track-group distribution; atomic place-content, exact geometry, paint, hits, and flow |
| Bounded inline SVG | PASS | Vg/Vs/Vp aliases, viewBox meet sizing, 128 shapes/points, six primitives, text, straight/cubic paths, inherited paint/opacity, links, accessibility, and GDI paint |
| SVG geometry and transforms | PASS | C/S/Q/T/A paths, inherited matrix/translate/scale/rotate/skew transforms, preserveAspectRatio none/meet/slice, clipping, accessibility links, and bounded invalid fallback |
| SVG gradient paint servers | PASS | Vg.Ds, Vs.Fd/Sd, Vd/Vt aliases, bounded local inheritance and cycles, object/user units, transforms, CSS stops/opacity/currentColor, linear/radial fill and stroke paint, links, and invalid fallback |
| SVG reusable geometry and clips | PASS | Vg.Cs, Vs.Cp/Cl/Cm, Vc aliases, bounded group/symbol/nested use expansion, x/y and transforms, cycle rejection, user/object clip units, reusable clip geometry, CSS/group attachment, paint, links, and unresolved fallback |
| SVG stroke and winding styles | PASS | Vs.Fr/Lc/Lj/Ml/Da/Do aliases, bounded stylesheet/presentation/inline cascade, variables, important priority, nonzero/evenodd fill and clips, butt/round/square caps, miter/round/bevel joins, miter limits, dash repetition/offset/inheritance/clearing, paint, and links |
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
| CSS | Partial | Compiled relationship, bounded strict has, is/where/list-not, An+B/reverse structural, and compound of-S filtered selectors, declarations with two-tier important cascade, inherited custom properties and var() fallback, screen/all pixel-width media queries, content/border box and text styles, visibility, positioning, transforms, responsive image sizing, bounded row/column flex, and bounded two-axis grid |
| Layout and painting | Partial | Custom WinForms content/border box, inline, row/column flex, bounded grid, table, form, image, inline vector, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Lea command scripting | Supported | Bounded title, body/ID-scoped color and font, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, and safe click commands |
| General JavaScript and DOM APIs | Not supported | Lea deliberately does not execute arbitrary JavaScript |
| Flexbox | Supported subset | Row/row-reverse and fixed-height column/column-reverse containers, content/border-box pixel bases, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee |
| CSS Grid | Supported subset | Grid/inline-grid with up to eight fixed-pixel, auto, or integer-fr tracks per explicit axis, simple repeat and minmax(0, Nfr), pixel gaps, bounded occupancy, two-axis numeric starts/spans, validated 8x8 named-area templates, unique explicit named lines, area-generated start/end lines, Gf row/column sparse/dense flow, repeating S.Av/Gac/Gav/Rac/Rav pixel/auto/integer-fr implicit patterns, definite endpoint/span growth, area-created track sizing, exact auto-height row settlement, atomic grid-template/grid explicit, quoted-area, and row/column auto-flow shorthands, Ji/Js/Ai/As start/center/end/stretch item alignment, atomic place-items/place-self, Al/Jc start/center/end/between/around/evenly track-group alignment, atomic place-content, and shared nested paint/hit/reflow; no escaped or repeated-name occurrence indexes, reverse flow, pre-explicit implicit tracks, shorthand area-row line names, intrinsic or percentage tracks, auto-fit/fill, baseline or safe/unsafe alignment, writing-mode axis reversal, or subgrid |
| SVG and Canvas | SVG supported subset; Canvas not supported | Bounded inline SVG with intrinsic width/height, preserveAspectRatio none/meet/slice mapping, groups/links, local shape/group/symbol use expansion, user/object clip paths, rect/circle/ellipse/line/polyline/polygon/text, M/L/H/V/Z/C/S/Q/T/A paths, cumulative matrix/translate/scale/rotate/skew transforms, linear/radial object-bounds or user-space gradients with bounded stops/inheritance/transforms, stylesheet/presentation/inline fill/stroke/opacity/font/clip paint, nonzero/evenodd fill and clip winding, caps, joins, miter limits, bounded dash patterns/offsets, currentColor, graphic accessibility, clipped paint, and transformed stroke-bounds hits; 128 shapes/points, 64 sources, 16 transforms/gradients/stops/clips/dash values, eight levels, and 256 text characters. No repeat/reflect spread, patterns, nested clips, masks, filters, markers, vector effects, animation, scripting, external references, symbol viewBox sizing, nested SVG, or Canvas |
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
| `Lea.Engine.dll` | 238080 | 232.5 KB |
| `Lea.Win.dll` | 180224 | 176.0 KB |
| `Lea.Win.Test.exe` | 394752 | 385.5 KB |
| `Lea.Bench.exe` | 156672 | 153.0 KB |
| `Firespider Internet 19.exe` | 1111040 | 1085.0 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 3
```
