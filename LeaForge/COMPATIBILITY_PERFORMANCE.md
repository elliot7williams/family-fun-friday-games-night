# Lea Compatibility and Performance Report

- Generated: **2026-09-13 UTC**
- Runtime: **.NET 4.0.30319.42000, 32-bit**
- Method: **3 clean-process starts; 3 loads per fixture; 3 render rounds at 1024x720**

These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.

## Current Snapshot

| Measurement | Result |
| --- | ---: |
| Clean process + engine + first page median | 688.206 ms |
| Clean process + engine + first page p95 | 716.270 ms |
| Local page load median | 5.361 ms |
| Local page load p95 | 15.642 ms |
| Layout + bitmap paint median | 38.812 ms |
| Layout + bitmap paint p95 | 206.195 ms |
| Retained managed memory | 22652.0 KB for 380 loaded pages |
| Retained managed memory per page | 59.6 KB |
| Engine runtime files | 599.0 KB |
| Engine runtime GZip estimate | 249.7 KB (41.7% of raw) |

## Fixture Loads

Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.

| Fixture | Bytes | Nodes | CSS rules | Median | p95 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `index.html` | 1032 | 19 | 1 | 3.500 ms | 3.746 ms |
| `recovery.html` | 1040 | 39 | 0 | 2.675 ms | 3.201 ms |
| `html-tokens.html` | 943 | 15 | 2 | 2.263 ms | 2.394 ms |
| `selectors.html` | 1017 | 20 | 10 | 2.782 ms | 2.823 ms |
| `selector-functions.html` | 2586 | 23 | 18 | 5.204 ms | 5.265 ms |
| `selector-arithmetic.html` | 1955 | 25 | 18 | 5.650 ms | 5.719 ms |
| `selector-filtered-position.html` | 2375 | 25 | 13 | 7.145 ms | 7.504 ms |
| `selector-relational.html` | 2027 | 28 | 15 | 4.666 ms | 5.505 ms |
| `layout.html` | 616 | 9 | 4 | 1.404 ms | 1.761 ms |
| `tables.html` | 894 | 28 | 6 | 7.880 ms | 11.708 ms |
| `forms.html` | 1566 | 25 | 7 | 3.764 ms | 3.931 ms |
| `editing.html` | 880 | 18 | 6 | 1.950 ms | 1.950 ms |
| `retained-data.html` | 1254 | 32 | 4 | 3.613 ms | 6.391 ms |
| `stylesheets.html` | 829 | 16 | 4 | 2.105 ms | 2.165 ms |
| `load-lifecycle.html` | 445 | 10 | 4 | 1.593 ms | 2.076 ms |
| `image-loading.html` | 802 | 12 | 4 | 1.343 ms | 1.429 ms |
| `image-preflight.html` | 727 | 10 | 2 | 1.477 ms | 2.756 ms |
| `input-loading.html` | 434 | 13 | 4 | 1.847 ms | 1.882 ms |
| `cache-identity.html` | 609 | 12 | 2 | 1.481 ms | 1.543 ms |
| `events-vars.html` | 1132 | 14 | 5 | 3.467 ms | 4.543 ms |
| `dom-bridge.html` | 2134 | 20 | 5 | 4.476 ms | 4.883 ms |
| `display-navigation.html` | 1554 | 19 | 6 | 2.747 ms | 3.806 ms |
| `calculated-widths.html` | 3031 | 41 | 24 | 7.424 ms | 8.326 ms |
| `calculated-heights.html` | 4598 | 47 | 34 | 10.503 ms | 10.810 ms |
| `viewport-units.html` | 5907 | 55 | 41 | 12.715 ms | 13.925 ms |
| `font-relative-units.html` | 5438 | 49 | 35 | 12.042 ms | 12.136 ms |
| `size-functions.html` | 6894 | 56 | 36 | 15.917 ms | 17.556 ms |
| `logical-sizing-insets.html` | 3221 | 24 | 16 | 7.880 ms | 8.943 ms |
| `logical-box-model.html` | 5522 | 25 | 15 | 6.360 ms | 7.335 ms |
| `logical-borders.html` | 7903 | 25 | 17 | 8.711 ms | 9.689 ms |
| `direction-logical.html` | 3833 | 30 | 23 | 8.204 ms | 8.532 ms |
| `direction-layout.html` | 4452 | 62 | 31 | 19.605 ms | 20.170 ms |
| `direction-inline.html` | 3556 | 60 | 37 | 10.986 ms | 11.963 ms |
| `sticky-positioning.html` | 1785 | 18 | 13 | 3.610 ms | 4.252 ms |
| `responsive-media.html` | 2575 | 21 | 5 | 3.344 ms | 4.329 ms |
| `media-queries.html` | 2229 | 24 | 16 | 4.854 ms | 4.977 ms |
| `custom-properties.html` | 2890 | 23 | 17 | 5.149 ms | 6.157 ms |
| `important-cascade.html` | 2748 | 19 | 16 | 5.074 ms | 5.455 ms |
| `direction-overflow.html` | 2246 | 27 | 20 | 5.220 ms | 6.444 ms |
| `stacking-contexts.html` | 3149 | 32 | 22 | 8.189 ms | 11.229 ms |
| `external.html` | 502 | 12 | 6 | 2.490 ms | 3.188 ms |
| `base-url.html` | 819 | 17 | 5 | 2.178 ms | 2.277 ms |
| `class-commands.html` | 1169 | 15 | 4 | 2.771 ms | 3.969 ms |
| `go-command.html` | 971 | 12 | 6 | 3.235 ms | 3.236 ms |
| `lists.html` | 1849 | 39 | 5 | 4.865 ms | 5.403 ms |
| `list-style.html` | 5755 | 86 | 36 | 15.642 ms | 16.538 ms |
| `list-position.html` | 5606 | 67 | 40 | 12.498 ms | 13.401 ms |
| `list-image.html` | 5824 | 56 | 37 | 14.373 ms | 15.141 ms |
| `marker.html` | 4091 | 37 | 31 | 11.740 ms | 12.845 ms |
| `disclosure.html` | 2270 | 34 | 9 | 5.445 ms | 6.216 ms |
| `fragments.html` | 2234 | 31 | 11 | 4.805 ms | 5.681 ms |
| `fragment-stability.html` | 1515 | 13 | 3 | 2.405 ms | 3.021 ms |
| `gauges.html` | 2115 | 25 | 14 | 4.266 ms | 4.668 ms |
| `values.html` | 1734 | 26 | 7 | 2.921 ms | 3.535 ms |
| `time-edits.html` | 1966 | 27 | 7 | 3.400 ms | 3.857 ms |
| `image-maps.html` | 2569 | 30 | 6 | 3.523 ms | 3.651 ms |
| `keyboard-focus.html` | 3721 | 41 | 18 | 7.777 ms | 9.044 ms |
| `focus-shortcuts.html` | 1838 | 20 | 13 | 3.913 ms | 4.827 ms |
| `accessibility.html` | 3148 | 60 | 4 | 4.826 ms | 5.347 ms |
| `state-commands.html` | 2711 | 37 | 9 | 9.247 ms | 10.438 ms |
| `flex.html` | 6942 | 57 | 57 | 21.568 ms | 23.597 ms |
| `grid.html` | 5699 | 62 | 37 | 20.604 ms | 20.766 ms |
| `grid-areas.html` | 3189 | 31 | 20 | 8.885 ms | 10.074 ms |
| `grid-lines.html` | 5342 | 45 | 31 | 10.820 ms | 11.146 ms |
| `grid-flow.html` | 4271 | 45 | 38 | 13.361 ms | 13.416 ms |
| `grid-auto-tracks.html` | 4332 | 41 | 25 | 9.183 ms | 12.687 ms |
| `grid-shorthand.html` | 5060 | 48 | 33 | 15.877 ms | 22.245 ms |
| `grid-alignment.html` | 3129 | 27 | 23 | 8.405 ms | 9.873 ms |
| `grid-content-alignment.html` | 2723 | 37 | 18 | 7.828 ms | 11.014 ms |
| `svg.html` | 1775 | 25 | 4 | 3.627 ms | 4.204 ms |
| `svg-geometry.html` | 2470 | 31 | 2 | 5.070 ms | 5.127 ms |
| `svg-gradients.html` | 3238 | 43 | 3 | 2.876 ms | 5.442 ms |
| `svg-use-clips.html` | 2947 | 48 | 3 | 3.759 ms | 3.942 ms |
| `svg-strokes.html` | 2832 | 28 | 5 | 2.620 ms | 2.625 ms |
| `box-sizing.html` | 2744 | 20 | 13 | 3.280 ms | 5.253 ms |
| `border-radius.html` | 1695 | 16 | 12 | 2.778 ms | 3.600 ms |
| `logical-radii.html` | 4754 | 22 | 14 | 4.279 ms | 6.433 ms |
| `box-shadows.html` | 2445 | 22 | 19 | 6.728 ms | 11.210 ms |
| `background-gradients.html` | 2943 | 22 | 19 | 6.390 ms | 7.206 ms |
| `background-images.html` | 4326 | 26 | 23 | 5.751 ms | 7.224 ms |
| `object-fit.html` | 5828 | 25 | 16 | 3.421 ms | 4.003 ms |
| `aspect-ratio.html` | 5640 | 49 | 38 | 10.004 ms | 11.138 ms |
| `outlines.html` | 2871 | 23 | 19 | 3.842 ms | 4.482 ms |
| `pointer-interaction.html` | 3471 | 30 | 27 | 8.986 ms | 10.252 ms |
| `text-transform.html` | 3150 | 32 | 25 | 7.039 ms | 7.974 ms |
| `text-spacing.html` | 3942 | 33 | 26 | 9.688 ms | 11.617 ms |
| `text-indent.html` | 2947 | 26 | 20 | 4.964 ms | 9.054 ms |
| `text-decoration.html` | 3829 | 34 | 24 | 5.421 ms | 6.031 ms |
| `text-underline-offset.html` | 4690 | 36 | 29 | 6.765 ms | 6.811 ms |
| `text-wrap.html` | 5382 | 47 | 35 | 7.635 ms | 9.102 ms |
| `text-align.html` | 4395 | 44 | 32 | 6.652 ms | 6.958 ms |
| `text-align-last.html` | 3465 | 38 | 27 | 5.885 ms | 6.246 ms |
| `text-shadow.html` | 2640 | 25 | 19 | 6.131 ms | 8.200 ms |
| `text-overflow.html` | 3878 | 34 | 26 | 8.882 ms | 10.645 ms |
| `line-clamp.html` | 5339 | 46 | 29 | 7.639 ms | 8.385 ms |

## Executable Probes

| Feature group | Result | Evidence |
| --- | --- | --- |
| HTML tree and recovery | PASS | Recovered tree, structured table, raw text, RCDATA, and entities |
| Bounded list markers | PASS | Unordered, start/value, nested negative, saturated, inline, and painted markers |
| Bounded list marker styles | PASS | S.Lst one-character inherited state, disc/circle/square/none, bounded decimal/zero/alpha/Latin/Roman formatting, HTML type and reversed lists, cascade, lst/DOM commands, RTL paint, hits, and retained resize |
| Bounded list marker position | PASS | S.Lp one-character inherited inside/outside state, independent longhands, atomic list-style composition and reset, lp/ls/readable/DOM commands, LTR/RTL gutters, table/hover/media paths, and retained fields |
| Bounded list marker images | PASS | S.Li inherited URI state, li/ls/readable/DOM commands, URL-aware atomic shorthand and none ambiguity, sandboxed deduplicated image loading, 64px intrinsic cap, text fallback, inside/outside LTR/RTL paint, tables, hover/media, and retained fields |
| Bounded marker pseudo-element styles | PASS | B.Ms compact marker style, terminal ::marker cascade, inherited text state with UA reset, bounded literal content, image precedence, ms/markerstyle commands, LTR/RTL paint, hover/media rebuilds, and retained fields |
| Bounded disclosures | PASS | Nested open state, retained fields, compact markers, host/view toggles, and painted content |
| Retained fragment navigation | PASS | 128-entry history, zero-fetch page retention, encoded/named targets, hidden reveal, table placement, and resize coverage |
| Stable initial fragments | PASS | Fa/Fj/Fy diagnostics, first-paint retention, responsive host settlement, and manual-scroll release |
| Host navigation guard | PASS | Ng policy, typed rejection, retained page/history, async view alias, and disposal; not a redirect/resource firewall |
| Blocked native input | PASS | Expected Ng denials stay out of native keyboard callbacks; public Ka still reports blocked |
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
| Bounded logical corner radii | PASS | horizontal-tb/LTR logical corners mapped onto R0-R3/Q0-Q3, source order, important, variables, inheritance/reset, atomic rejection, nine compact/readable commands, familiar DOM styles, authored restore, and asymmetric paint |
| Bounded CSS box shadows | PASS | S.Sw with X/Y/B/R/C/I aliases, four-layer cap, atomic parsing, variables, priority/reset/bounds, alpha/current colors, rounded outer/inset/field paint, unchanged hits, and resize |
| Bounded CSS background gradients | PASS | S.Gi and Gi.K/A/X/Y/Ss plus Cs.C/P aliases, one-layer/eight-stop caps, atomic parsing, variables, priority/reset/bounds, normalized alpha/current colors, rounded linear/radial/page/field paint, unchanged hits, and resize |
| Bounded CSS bitmap backgrounds | PASS | S.Bi with U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K aliases, bounded data/file loading, stylesheet/custom-property origins, atomic URL/position/size/shorthand parsing, repeat/axis/cover/contain/page/field paint, rounded clips, opacity, and resize |
| Bounded replaced-image fitting | PASS | S.Of=f/c/v/n/s with Ox/Oy/Oxp/Oyp aliases, atomic variables/priority/reset/noninheritance, clipped fill/contain/cover/none/scale-down paint, fitted map pointer/accessibility/keyboard geometry, and resize |
| Bounded CSS aspect ratios | PASS | S.Rw/Rh/Ra aliases, bounded atomic parsing and reset/noninheritance, intrinsic media fallback, content/border-box, fields, flex/grid/table layout, paint, and resize |
| Bounded CSS outlines | PASS | S.Ow/Os/Oc/Oo aliases, atomic shorthand/longhands, variables/priority/reset/noninheritance, solid/dashed/dotted/auto and signed offsets, rounded box/field focus paint, clipping, stable geometry, and resize |
| Bounded pointer interaction | PASS | S.Cu/Pe inherited aliases, bounded cursor keywords, variables/priority/reset/invalid preservation, pointer-disabled link/field hits and hover, child re-enable, semantic cursors, keyboard/accessibility stability, and resize |
| Bounded text transform | PASS | S.Tt n/u/l/c inherited alias, strict none/upper/lower/capitalize parsing, inline overrides, variables/priority/resets/invalid preservation, multiline/table/control presentation, raw DOM/form retention, hover, paint, and resize |
| Bounded text spacing | PASS | S.Ls/Wg inherited signed pixel aliases, strict normal/reset/bounds/invalid preservation, inline overrides, variables/priority, shared wrap and paint metrics, raw form retention, hover, and resize |
| Bounded text indent | PASS | S.Ti/Tip inherited signed pixel/percentage aliases, strict reset/bounds, variables/priority, LTR/RTL first-line wrap geometry, ti/DOM commands, hover, paint, and resize |
| Bounded text decoration | PASS | S.Td/Tc/Tds/Tw compact aliases, bounded atomic shorthand and longhands, solid/double/dotted/dashed/wavy underline/strike paint, inheritance/reset, variables/priority, td/tdl/tdc/tds/tdw and DOM commands, stable geometry, hover, and resize |
| Bounded text underline offset | PASS | S.Uo/Uop compact inherited signed pixel/percentage state, strict resets and bounds, shorthand independence, uo/readable/DOM commands, offset-aware custom paint, stable geometry, hover, and resize |
| Bounded balanced text wrapping | PASS | S.Wm/Wt compact inherited mode/style state, text-wrap shorthand, white-space source order, bounded same-count balancing, tw/twm/tws readable/DOM routes, LTR/RTL/indent/forced/table geometry, hover, and resize |
| Bounded text alignment shorthand | PASS | S.Ta/Tl compact physical/logical state, text-align shorthand, text-align-all, justify-all, match-parent, ta/taa/readable/DOM routes, inherited LTR/RTL geometry, hover, and resize |
| Bounded text align last | PASS | S.Tl inherited compact alignment, auto/start/end/left/right/center/justify/match-parent cascade, tl/readable/DOM commands, final and forced-line LTR/RTL geometry, stable boxes, hover, and resize |
| Bounded text shadow | PASS | S.Ts inherited nullable alias, one bounded offset/blur/color layer, strict reset/invalid preservation, inline override, variables/priority, ts/DOM commands, block/field paint, stable geometry, hover, and resize |
| Bounded text overflow | PASS | S.To c/e non-inherited alias, strict clip/ellipsis, inherit/reset/invalid/variable/important cascade, paint-only inline/table/multiline truncation, preserved authored text, hover, and resize |
| Bounded line clamping | PASS | S.Lc compact non-inherited 0/1..32 state, standard/WebKit syntax, strict cascade, lc/readable/DOM routes, retained LTR/RTL inline and table geometry, grapheme-safe markers, hidden hit removal, full logical text, hover, flow re-layout, and resize |
| Bounded flex layout | PASS | Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or aliases, stable row order, column/reverse flow, around/evenly distribution, item alignment, fixed, auto, wrapped, growing, shrinking, protected, and locked slots, hits, resize, and following flow |
| Direction-aware flex and grid layout | PASS | S.Dr plus row/reverse/order/wrap flex, column cross-axis alignment, logical grid auto/explicit/dense/column/area placement, content/self alignment, source order, paint, and compact dr reflow |
| Direction-aware inline flow and markers | PASS | S.Dr right-origin plain/mixed/wrapped/nowrap/justified lines, physical/logical alignment, nested ordered/unordered/disclosure markers, table parts, source order, paint, hits, and compact dr reflow |
| Direction-aware ellipsis and range geometry | PASS | bounded LTR/RTL physical inline-end ellipsis, grapheme-safe right-aligned paint prefixes, retained link hits, table clips, full logical text, direction-aware partial/wrapped/point TextPattern geometry, and compact dr reflow |
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
| Native field edit commands | PASS | Ce/Ec two-letter commands, native one-level undo/redo, readonly/password guards, shared-font field switching; probe does not touch clipboard |
| Bounded supplied paste | PASS | Pa host-supplied text, maxlength/Unicode, multiline, native undo, protected/disposed targets; no clipboard access |
| Named native stylesheets | PASS | Ss/Sd/Sn named sheets, native computed/paint route, atomic source bound, retained field identity, and diagnostics |
| View load admission | PASS | Cn admission, Busy through presentation, ready callback, and disposed guard; deterministic overlap/handoff regressions run in Lea.Tests --loads |
| Bounded picture retention | PASS | Ic/Ib bitmap count/bytes, shared embedded source, retained fields, native paint, and page replacement; streaming/deadline/cache-limit regressions run in Lea.Tests --pics |
| Native image preflight | PASS | Pi preflight, hardened-worker and Icl/Ihm/Icm/Iad combined-memory aliases, one native decode, one pre-native rejection, retained fields, paint, and disposal |
| Combined image memory admission | PASS | Icl bounded combined ceiling, Ihm host sample, Icm conservative host/helper/reserve projection, Iad denial, zero native payload admissions, and reservation cleanup |
| Zero-capability image sandbox | PASS | Iac/Ind zero-capability AppContainer, exact helper-file RX ACL, active denied loopback/host-file probes, bounded native decode, and no added runtime |
| Bounded document input | PASS | Hm/Csm document/sheet ceilings, local asynchronous input, linked CSS, native paint, and retained fields; wire/decoded/deadline/origin regressions run in Lea.Tests --input |
| Exact cache identity | PASS | ordinal path/query keys after URI canonicalization, fragment stripping, authored link case, native layout, and network/disk regressions in Lea.Tests --cache |
| Bounded Lea scripting | PASS | 7 safe commands and 2 variables after click |
| Literal DOM compatibility | PASS | getElementById literal text, value, booleans, classList, focus, three styles, 20 accepted commands, and rejected arbitrary DOM APIs |
| Display and location compatibility | PASS | dp plus literal style.display, empty reset, retained inline style, validated modes, href/assign mapping, and existing guarded go requests |
| Percentage and calculated widths | PASS | wd plus literal style.width, 1..100 percent, calc(P% +/- 0..2000px), min/max, variables, inheritance, reset, and strict expression bounds |
| Percentage and calculated heights | PASS | ht plus literal style.height, definite viewport/parent resolution, auto fallback, flex/grid/table/aspect-ratio geometry, min/max, variables, inheritance, reset, and strict bounds |
| Bounded viewport units | PASS | vw/vh/vmin/vmax plus calc offsets, cross-axis sizing, lazy unit aliases, min/max, variables, inheritance, layout families, commands, and live resize |
| Bounded font-relative dimensions | PASS | em/rem dimensions plus root-font state, calc offsets, lazy aliases, min/max, variables, inheritance, layout families, commands, font mutation, and responsive rebuild |
| Bounded CSS size functions | PASS | min/max with two or three bounded terms, three-term clamp, lazy Wk/Hk and Wq/Hq/Wv/Hv aliases, mixed units/calc, min/max properties, layout families, commands, media replacement, and resize |
| Bounded logical sizing and insets | PASS | horizontal-tb/LTR inline/block size mapping, min/max, source order, important, variables, inheritance, logical inset axes/longhands, atomic fallback, is/bs, literal DOM styles, shared geometry, and reset |
| Bounded logical margins and padding | PASS | horizontal-tb/LTR logical margin/padding axes and longhands, signed bounded margins, atomic parsing, source order, important, variables, inheritance, reset, twelve compact/readable commands, familiar DOM styles, and shared geometry |
| Bounded logical borders | PASS | horizontal-tb/LTR logical border axes and edges, physical per-edge components, bounded atomic parsing, source order, important, variables, inheritance/reset, twenty-four compact/readable commands, familiar DOM styles, mixed rounded paint, and shared geometry |
| Bounded horizontal direction mapping | PASS | inherited ltr/rtl direction with HTML dir, variables, important and pre-cascade resolution, remapped logical insets/boxes/borders/radii, cross-direction inherit, start/end alignment, dr/readable/DOM commands, and reset |
| Bounded sticky positioning | PASS | Pn=k, signed top/bottom state, normal flow, shared descendants/hits/accessibility, root scroll pinning, and parent clamp |
| Host-controlled go requests | PASS | Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected |
| Bounded network timeout | PASS | Tm defaults to 15000 ms and clamps to 100-60000 ms |
| Bounded persistent cache | PASS | Cd opt-in; 32 entries/16 MB defaults; restart hit; ETag disk304; Dc/Db/Dh diagnostics |
| Bounded class mutations | PASS | ac/rc/tc plus one/two-argument bg/fg/fs restyle compiled CSS with bounded IDs, colors, 8-96 px fonts, 32 classes, and retained fields |
| Bounded element-state commands | PASS | hd/sh, ds/en, vl, ck, fo, op/cl/tg, retained fields, one pending focus target, and file-value rejection |
| Find and local download | PASS | 3 text matches and atomic local download |
| Bounded diagnostics | PASS | 42 node lines, 5 CSS lines, 1 request |
| Retained field data | PASS | E.Ud/V.Ud; pristine choices, edited and hidden retained fields, selected files, read-only bounded query |
| Native load failure and retry | PASS | Fe/Le typed failure, retained page, Cr/Ry explicit GET retry, and success reset |

## Compatibility Scope

| Area | Status | Lea 1.0 scope |
| --- | --- | --- |
| Local files and HTTP/HTTPS | Supported | Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors |
| Optional persistent cache | Supported | Memory-only default, gzip entries, LRU entry/byte ceilings, validators, private isolation, and corrupt-entry recovery |
| HTML documents | Partial | Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, direction-aware ordered/unordered lists and disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard |
| CSS | Partial | Compiled relationship, bounded strict has, is/where/list-not, An+B/reverse structural, compound of-S filtered, and terminal ::marker selectors, declarations with two-tier important cascade, inherited custom properties and var() fallback, screen/all pixel-width media queries, inherited horizontal LTR/RTL direction, fixed/percentage/calc dimensions and classic vw/vh/vmin/vmax plus bounded em/rem units, two/three-term min/max and three-term clamp size functions, content/border box, circular/elliptical radii, bounded outlines, box shadows, one inherited bounded text-shadow layer, bounded text-decoration line/color/style/thickness and signed pixel/percentage underline offset, inherited text-align shorthand/all/last with logical state and justify-all, inherited text-wrap mode/style with bounded balance, non-inherited bounded line-clamp/WebKit compatibility, isolated bounded marker typography/content, and one-layer linear/radial gradients, object fit/position, bounded aspect ratios, inherited cursor/pointer interaction, bounded text transform, signed integer letter/word spacing, signed pixel/percentage first-line indentation, and text styles, visibility, absolute/fixed and bounded vertical sticky positioning, transforms, responsive image sizing, direction-aware row/column flex, and direction-aware bounded two-axis grid |
| Layout and painting | Partial | Custom WinForms content/border box, normalized rounded backgrounds/borders/outlines/overflow/fields/box-shadows/gradients, bounded offset and lightweight blurred text shadows, native fast-path plus lightweight offset-aware solid/double/dotted/dashed/wavy text decorations, direction-aware source-stable inline atoms, bounded same-count balanced wrapping and 1..32 line clamping, inside/outside text and image markers with isolated pseudo style/content, first-line indentation, inherited logical all/final/forced-line alignment, and paint-only single-line ellipsis geometry, invariant transformed and spacing-aware presentation text with raw form retention, shared line-edge and inline-boundary tracking metrics, fitted raster images and maps, shared aspect-ratio sizing, pointer-gated retained hits with underlying fallthrough, row/column flex, bounded grid, table, form, inline vector, clipping, scroll, and stacking renderer |
| Forms | Supported subset | GET/POST, validation, text/select/choice/file/range/date/color/time-family controls |
| Text overflow | Supported subset | Non-inherited clip/ellipsis with bounded LTR/RTL physical inline-end markers and grapheme-safe paint-only authored prefixes for inline runs and tables; retained layout/link targets, full logical automation text, and direction-aware partial range geometry; no native-control, Unicode bidi shaping, or custom overflow-marker additions |
| Line clamping | Supported subset | Non-inherited line-clamp and -webkit-line-clamp none/1..32 with LTR/RTL grapheme-safe ellipsis, reduced flow height, hidden retained boxes/hits, full logical source and UI Automation text, inline runs, forced breaks, balanced text, and table cells; no block-ellipsis strings, longhands, descendant block fragmentation, or legacy WebKit display requirements |
| Lea command scripting | Supported | Bounded title, body/ID-scoped color, font, display, width, height, direction, text indent, text alignment/all/last, text wrap/mode/style, line clamp, list and marker style, text decoration, underline offset, and text shadow, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, safe click commands, and literal compatibility translation |
| Literal DOM bridge | Supported subset | Literal document.getElementById IDs with textContent/innerText, value, checked/disabled/hidden/open, focus(), classList add/remove/toggle, and background/color/fontSize/display/width/height/direction/textIndent/textAlign/textAlignAll/textAlignLast/textWrap/textWrapMode/textWrapStyle/lineClamp/webkitLineClamp/textDecoration/textDecorationLine/textDecorationColor/textDecorationStyle/textDecorationThickness/textUnderlineOffset/textShadow; literal location href/assign requests; mapped to bounded Lea aliases |
| General JavaScript and DOM APIs | Not supported | Lea deliberately does not execute arbitrary JavaScript |
| Flexbox | Supported subset | Row/row-reverse and fixed-height column/column-reverse containers, content/border-box pixel bases, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee |
| CSS Grid | Supported subset | Grid/inline-grid with up to eight fixed-pixel, auto, or integer-fr tracks per explicit axis, simple repeat and minmax(0, Nfr), pixel gaps, bounded occupancy, two-axis numeric starts/spans, validated 8x8 named-area templates, unique explicit named lines, area-generated start/end lines, Gf row/column sparse/dense flow, repeating S.Av/Gac/Gav/Rac/Rav pixel/auto/integer-fr implicit patterns, definite endpoint/span growth, area-created track sizing, exact auto-height row settlement, atomic grid-template/grid explicit, quoted-area, and row/column auto-flow shorthands, Ji/Js/Ai/As start/center/end/stretch item alignment, atomic place-items/place-self, Al/Jc start/center/end/between/around/evenly track-group alignment, atomic place-content, and shared nested paint/hit/reflow; no escaped or repeated-name occurrence indexes, reverse flow, pre-explicit implicit tracks, shorthand area-row line names, intrinsic or percentage tracks, auto-fit/fill, baseline or safe/unsafe alignment, writing-mode axis reversal, or subgrid |
| SVG and Canvas | SVG supported subset; Canvas not supported | Bounded inline SVG with intrinsic width/height, preserveAspectRatio none/meet/slice mapping, groups/links, local shape/group/symbol use expansion, user/object clip paths, rect/circle/ellipse/line/polyline/polygon/text, M/L/H/V/Z/C/S/Q/T/A paths, cumulative matrix/translate/scale/rotate/skew transforms, linear/radial object-bounds or user-space gradients with bounded stops/inheritance/transforms, stylesheet/presentation/inline fill/stroke/opacity/font/clip paint, nonzero/evenodd fill and clip winding, caps, joins, miter limits, bounded dash patterns/offsets, currentColor, graphic accessibility, clipped paint, and transformed stroke-bounds hits; 128 shapes/points, 64 sources, 16 transforms/gradients/stops/clips/dash values, eight levels, and 256 text characters. No repeat/reflect spread, patterns, nested clips, masks, filters, markers, vector effects, animation, scripting, external references, symbol viewBox sizing, nested SVG, or Canvas |
| Audio and video | Not supported | Future optional module |
| Developer diagnostics | Supported | Bounded summary, node, CSS, request, message, and variable snapshots |
| Document queries | Supported subset | Id, first-match Q/Q1, and read-only Qa with compiled selectors and a 256-result ceiling |
| Document base URLs | Supported | First valid bounded HTTP, HTTPS, or file base; U remains the response URL and Bu drives relative resources |
| Same-page fragments | Supported | Bounded Ft/Fg/Rv lookup, retained 128-entry history, details reveal, no-fetch Back/Forward, and Fa/Fj/Fy initial placement across first paint, resize, and intrinsic images with manual-scroll release |
| Progress and meter gauges | Supported | Compact Vm values, finite normalization, meter quality regions, inherited styles, table cells, and custom paint |
| Output and data values | Supported | Compact Dv values, bounded references and text, host aliases, inline/block/table ownership, and safe rebuilds |
| Time and edit metadata | Supported | Compact Dv t/i/r values, bounded datetime and citation aliases, safe schemes, native decorations, inline/block/table ownership, and safe rebuilds |
| Image maps | Supported subset | Compact Mp/Ar models, rect/circle/poly/default and dead areas, bounded coordinates, object-fit-aware scaled and cropped hit regions, area links/downloads, and safe rebuilds |
| Keyboard focus | Supported subset | Bounded tabindex order, links, image-map areas, fields, summaries, visible focus, scrolling, wrapping, and host activation aliases |
| Focus shortcuts | Supported subset | Bounded autofocus candidates and ASCII access-key alternatives with conflict resolution, Alt+Shift labels, host-safe matching, focus, and activation |
| Accessibility | Supported subset | Bounded Ax hierarchy with MSAA and native UI Automation fragments, roles, names, live protected state, rendered bounds, focus, invoke/value/toggle/selection/disclosure/range/scroll and Tb/Tc-backed grid/table/item patterns, plus read-only logical TextPattern document/child/point ranges with direction-aware visual-line, partial-range, and point geometry; visible-range arrays use an empty fallback, and this is not the complete platform mapping standards |

## Binary Footprint

| Artifact | Bytes | Size |
| --- | ---: | ---: |
| `Lea.Engine.dll` | 333824 | 326.0 KB |
| `Lea.Win.dll` | 270336 | 264.0 KB |
| `Lea.Image.exe` | 9216 | 9.0 KB |
| `Lea.Win.Test.exe` | 395264 | 386.0 KB |
| `Lea.Bench.exe` | 345088 | 337.0 KB |
| `Firespider Internet 19.exe` | 1131520 | 1105.0 KB |

The engine runtime size is `Lea.Engine.dll`, `Lea.Win.dll`, and the required `Lea.Image.exe` decoder helper. Test and report executables are listed separately and are not required when embedding Lea in Firespider.

## Re-run

From the repository root after a Release build:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 3
```
