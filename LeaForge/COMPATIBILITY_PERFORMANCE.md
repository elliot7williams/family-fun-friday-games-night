# Lea Compatibility and Performance Report

- Generated: **2026-08-30 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **5 clean-process starts; 15 loads per fixture; 5 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 609.906 ms |
| Clean process + engine + first page p95 | 751.520 ms |
| Local page load median | 4.204 ms |
| Local page load p95 | 15.615 ms |
| Layout + bitmap paint median | 39.936 ms |
| Layout + bitmap paint p95 | 142.486 ms |
| Retained managed memory | 15022.3 KB for 290 loaded pages |
| Retained managed memory per page | 51.8 KB |
| Engine runtime DLLs | 456.5 KB |
| Engine runtime GZip estimate | 192.5 KB (42.2% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 1032 | 19 | 1 | 2.950 ms | 11.533 ms |
| `recovery.html` | 1040 | 39 | 0 | 2.113 ms | 3.793 ms |
| `html-tokens.html` | 943 | 15 | 2 | 2.022 ms | 2.551 ms |
| `selectors.html` | 1017 | 20 | 10 | 2.087 ms | 2.876 ms |
| `selector-functions.html` | 2586 | 23 | 18 | 4.827 ms | 16.566 ms |
| `selector-arithmetic.html` | 1955 | 25 | 18 | 4.957 ms | 5.557 ms |
| `selector-filtered-position.html` | 2375 | 25 | 13 | 3.875 ms | 5.774 ms |
| `selector-relational.html` | 2027 | 28 | 15 | 3.257 ms | 10.742 ms |
| `layout.html` | 616 | 9 | 4 | 0.814 ms | 1.432 ms |
| `tables.html` | 894 | 28 | 6 | 1.834 ms | 3.324 ms |
| `forms.html` | 1566 | 25 | 7 | 3.000 ms | 4.016 ms |
| `events-vars.html` | 1132 | 14 | 5 | 2.679 ms | 3.179 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 2.811 ms | 7.938 ms |
| `media-queries.html` | 2229 | 24 | 16 | 3.609 ms | 4.511 ms |
| `custom-properties.html` | 2890 | 23 | 17 | 4.647 ms | 5.558 ms |
| `important-cascade.html` | 2748 | 19 | 16 | 3.972 ms | 17.950 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 6.309 ms | 7.580 ms |
| `external.html` | 502 | 12 | 6 | 1.699 ms | 3.143 ms |
| `base-url.html` | 819 | 17 | 5 | 1.369 ms | 20.018 ms |
| `class-commands.html` | 1169 | 15 | 4 | 2.067 ms | 3.641 ms |
| `go-command.html` | 971 | 12 | 6 | 2.632 ms | 3.858 ms |
| `lists.html` | 1849 | 39 | 5 | 3.966 ms | 4.843 ms |
| `disclosure.html` | 2270 | 34 | 9 | 4.898 ms | 18.874 ms |
| `fragments.html` | 2234 | 31 | 11 | 4.225 ms | 5.788 ms |
| `gauges.html` | 2115 | 25 | 14 | 3.347 ms | 4.293 ms |
| `values.html` | 1734 | 26 | 7 | 2.694 ms | 5.995 ms |
| `time-edits.html` | 1966 | 27 | 7 | 3.189 ms | 5.503 ms |
| `image-maps.html` | 2569 | 30 | 6 | 2.705 ms | 4.319 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 6.366 ms | 14.880 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 2.829 ms | 6.677 ms |
| `accessibility.html` | 3148 | 60 | 4 | 4.288 ms | 5.119 ms |
| `state-commands.html` | 2711 | 37 | 9 | 8.817 ms | 15.615 ms |
| `flex.html` | 6942 | 57 | 57 | 17.782 ms | 152.252 ms |
| `grid.html` | 5699 | 62 | 37 | 19.111 ms | 49.098 ms |
| `grid-areas.html` | 3189 | 31 | 20 | 10.268 ms | 27.342 ms |
| `grid-lines.html` | 5342 | 45 | 31 | 15.155 ms | 22.442 ms |
| `grid-flow.html` | 4271 | 45 | 38 | 11.521 ms | 20.637 ms |
| `grid-auto-tracks.html` | 4332 | 41 | 25 | 10.955 ms | 30.860 ms |
| `grid-shorthand.html` | 5060 | 48 | 33 | 14.185 ms | 20.164 ms |
| `grid-alignment.html` | 3129 | 27 | 23 | 5.985 ms | 10.060 ms |
| `grid-content-alignment.html` | 2723 | 37 | 18 | 6.337 ms | 8.259 ms |
| `svg.html` | 1775 | 25 | 4 | 3.385 ms | 8.824 ms |
| `svg-geometry.html` | 2470 | 31 | 2 | 3.364 ms | 7.059 ms |
| `svg-gradients.html` | 3238 | 43 | 3 | 2.903 ms | 5.442 ms |
| `svg-use-clips.html` | 2947 | 48 | 3 | 3.860 ms | 9.455 ms |
| `svg-strokes.html` | 2832 | 28 | 5 | 4.365 ms | 6.279 ms |
| `box-sizing.html` | 2744 | 20 | 13 | 4.344 ms | 5.905 ms |
| `border-radius.html` | 1695 | 16 | 12 | 2.542 ms | 5.848 ms |
| `box-shadows.html` | 2445 | 22 | 19 | 4.748 ms | 5.736 ms |
| `background-gradients.html` | 2943 | 22 | 19 | 4.868 ms | 11.969 ms |
| `background-images.html` | 4326 | 26 | 23 | 5.229 ms | 7.619 ms |
| `object-fit.html` | 5828 | 25 | 16 | 4.440 ms | 7.237 ms |
| `aspect-ratio.html` | 5640 | 49 | 38 | 7.887 ms | 15.647 ms |
| `outlines.html` | 2871 | 23 | 19 | 4.477 ms | 11.357 ms |
| `pointer-interaction.html` | 3471 | 30 | 27 | 7.378 ms | 9.467 ms |
| `text-transform.html` | 3150 | 32 | 25 | 4.813 ms | 11.029 ms |
| `text-spacing.html` | 3942 | 33 | 26 | 6.098 ms | 9.433 ms |
| `text-overflow.html` | 3878 | 34 | 26 | 9.875 ms | 20.342 ms |

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
| Bounded CSS corner radii | PASS | S.R0-R3/Q0-Q3 aliases, atomic pixel/percentage ellipses, longhands, variables, important/reset/bounds, normalized solid/dashed paint, overflow clips, fields, links, and resize |
| Bounded CSS box shadows | PASS | S.Sw with X/Y/B/R/C/I aliases, four-layer cap, atomic parsing, variables, priority/reset/bounds, alpha/current colors, rounded outer/inset/field paint, unchanged hits, and resize |
| Bounded CSS background gradients | PASS | S.Gi and Gi.K/A/X/Y/Ss plus Cs.C/P aliases, one-layer/eight-stop caps, atomic parsing, variables, priority/reset/bounds, normalized alpha/current colors, rounded linear/radial/page/field paint, unchanged hits, and resize |
| Bounded CSS bitmap backgrounds | PASS | S.Bi with U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K aliases, bounded data/file loading, stylesheet/custom-property origins, atomic URL/position/size/shorthand parsing, repeat/axis/cover/contain/page/field paint, rounded clips, opacity, and resize |
| Bounded replaced-image fitting | PASS | S.Of=f/c/v/n/s with Ox/Oy/Oxp/Oyp aliases, atomic variables/priority/reset/noninheritance, clipped fill/contain/cover/none/scale-down paint, fitted map pointer/accessibility/keyboard geometry, and resize |
| Bounded CSS aspect ratios | PASS | S.Rw/Rh/Ra aliases, bounded atomic parsing and reset/noninheritance, intrinsic media fallback, content/border-box, fields, flex/grid/table layout, paint, and resize |
| Bounded CSS outlines | PASS | S.Ow/Os/Oc/Oo aliases, atomic shorthand/longhands, variables/priority/reset/noninheritance, solid/dashed/dotted/auto and signed offsets, rounded box/field focus paint, clipping, stable geometry, and resize |
| Bounded pointer interaction | PASS | S.Cu/Pe inherited aliases, bounded cursor keywords, variables/priority/reset/invalid preservation, pointer-disabled link/field hits and hover, child re-enable, semantic cursors, keyboard/accessibility stability, and resize |
| Bounded text transform | PASS | S.Tt n/u/l/c inherited alias, strict none/upper/lower/capitalize parsing, inline overrides, variables/priority/resets/invalid preservation, multiline/table/control presentation, raw DOM/form retention, hover, paint, and resize |
| Bounded text spacing | PASS | S.Ls/Wg inherited signed pixel aliases, strict normal/reset/bounds/invalid preservation, inline overrides, variables/priority, shared wrap and paint metrics, raw form retention, hover, and resize |
| Bounded text overflow | PASS | S.To c/e non-inherited alias, strict clip/ellipsis, inherit/reset/invalid/variable/important cascade, paint-only inline/table/multiline truncation, preserved authored text, hover, and resize |
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
| CSS | Partial | Compiled relationship, bounded strict has, is/where/list-not, An+B/reverse structural, and compound of-S filtered selectors, declarations with two-tier important cascade, inherited custom properties and var() fallback, screen/all pixel-width media queries, content/border box, circular/elliptical radii, bounded outlines, box shadows and one-layer linear/radial gradients, object fit/position, bounded aspect ratios, inherited cursor/pointer interaction, bounded text transform, signed integer letter/word spacing and text styles, visibility, positioning, transforms, responsive image sizing, bounded row/column flex, and bounded two-axis grid |
| Layout and painting | Partial | Custom WinForms content/border box, normalized rounded backgrounds/borders/outlines/overflow/fields/shadows/gradients, invariant transformed and spacing-aware presentation text with raw form retention, shared line-edge and inline-boundary tracking metrics, fitted raster images and maps, shared aspect-ratio sizing, pointer-gated retained hits with underlying fallthrough, inline, row/column flex, bounded grid, table, form, inline vector, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Text overflow | Supported subset | Non-inherited clip/ellipsis; paint-only bounded text-element prefixes for clipped overflowing horizontal LTR lines, inline runs and tables; retained layout/link targets and full logical automation text; no native-control, bidi, custom-marker, or line-clamp additions |
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
| Image maps | Supported subset | Compact Mp/Ar models, rect/circle/poly/default and dead areas, bounded coordinates, object-fit-aware scaled and cropped hit regions, area links/downloads, and safe rebuilds |
| Keyboard focus | Supported subset | Bounded tabindex order, links, image-map areas, fields, summaries, visible focus, scrolling, wrapping, and host activation aliases |
| Focus shortcuts | Supported subset | Bounded autofocus candidates and ASCII access-key alternatives with conflict resolution, Alt+Shift labels, host-safe matching, focus, and activation |
| Accessibility | Supported subset | Bounded Ax hierarchy with MSAA and native UI Automation fragments, roles, names, live protected state, rendered bounds, focus, invoke/value/toggle/selection/disclosure/range/scroll and Tb/Tc-backed grid/table/item patterns, plus read-only logical TextPattern document/child/point ranges with visual-line and partial-range geometry; visible-range arrays use an empty fallback, and this is not the complete platform mapping standards |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 263680 | 257.5 KB |
| `Lea.Win.dll` | 203776 | 199.0 KB |
| `Lea.Win.Test.exe` | 394752 | 385.5 KB |
| `Lea.Bench.exe` | 195584 | 191.0 KB |
| `Firespider Internet 19.exe` | 1113088 | 1087.0 KB |

The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```
