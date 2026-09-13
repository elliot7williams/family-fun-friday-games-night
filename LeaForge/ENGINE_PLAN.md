# Lea Engine Plan

Lea is a small, fast, alias-first web engine. The goal is not to copy every
large browser feature immediately. The goal is to build a clean core that is
pleasant to code against, starts quickly, and grows through small tested steps.

## 1. Identity and limits

- Keep the public Lea vocabulary within one to three letters where practical.
- Keep the readable implementation underneath the compact public API.
- Keep `Lea.Micro` as the tiny experiment and `LeaForge` as the fuller engine.
- Avoid Chromium, WebView, and other large browser runtimes in the custom path.
- Track startup time, page load time, memory use, executable size, and compressed size.
- Keep every new feature optional when it would make the base engine much larger.

Core public vocabulary:

```text
L       Lea entry point
E       engine
P       page
N       node
Go      open
Bk      back
Fw      forward
Rl      reload
At      current page
t/a     title/alert
bg/fg   background/text color
fs      font size
v/tx    variable/element text
Ck      safe element click
Dg/Dcl  diagnostics/clear request activity
```

## 2. Engine foundation

1. Split the current single-file Forge prototype into small files.
2. Define stable interfaces for loading, parsing, layout, painting, and scripting.
3. Add a `Uri` and encoding utility that handles local files, HTTP, relative URLs,
   redirects, UTF-8, and common content types.
4. Use bounded caches for pages, images, stylesheets, and decoded resources.
   (An eight-entry LRU decoded-payload cache with a 2 MB per-page ceiling is active.)
   Native pictures now have 4 MiB streaming body limits, header preflight,
   a privilege-stripped low-integrity decoder process assigned to its Job while suspended,
   a shared pass deadline, 32 MiB per-view retention, and a
   configurable 128 MiB process-wide Lea image budget through compact
   `V.Ic/Ib/Ip/Im/It/In/If/Ie/Ii/Iw/Ik/Ij/Ih/Ipm/Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk/Iac/Ind/Ial` aliases. Document/POST bodies and linked CSS now
   have 4 MiB/256 KiB wire/decoded bounds and shared I/O budgets; see
   `PICTURE_LIMITS.md` and `INPUT_LIMITS.md` for remaining resource work.
5. Add cancellation and timeouts so a slow page cannot freeze the browser.
   (`GoA`, `RlA`, and `St` are now available in the compatibility engine.)

## 3. HTML and document tree

1. Replace regular-expression-only page parsing with a small tolerant tokenizer. (First stack-based tokenizer added.)
2. Build a lightweight document tree with elements, text, attributes, and parent links. (`N` now provides kind, text, attributes, and children; parent links remain.)
3. Support the useful base tags first: `html`, `head`, `body`, headings, paragraphs,
   lists, links, images, buttons, forms, tables, sections, and code blocks.
   (Single-select controls, URL-encoded GET/POST forms, and structured table rows,
   cells, captions, headers, and column spans are active.)
4. Preserve unknown tags safely so pages can still display their text.
5. Add malformed-markup tests and local fixture pages for every supported tag.

## 4. CSS and layout

1. Add selectors for tag, class, id, attributes, basic pseudo-classes, negation, descendant, child, adjacent/general sibling, and inline styles. (Compiled compact `>`, `+`, and `~` chains are active.)
2. Implement the cascade and inheritance with a small priority model. (First specificity/source-order cascade added.)
   (`link rel=stylesheet` and inline `style` blocks now enter that source order
   together; linked CSS uses cookies, compression, validators, and shared LRU caching.)
3. Add colors, fonts, spacing, borders, width, height, display, visibility, opacity, and overflow. (Colors, fallback font families, bounded underline/strike color/style/thickness, margins, padding, backgrounds, `display:none`, inherited visibility, bounded opacity, borders, non-inherited content/border-box pixel sizing, pixel width/height bounds, text wrapping mode/style with bounded balance, text alignment shorthand/all/last with logical state, transformation, signed letter/word spacing, signed pixel/percentage first-line indentation, one inherited bounded text-shadow layer, line height, word breaking, white-space modes, and clipped overflow are active.)
4. Replace one-WinForms-control-per-node rendering with lightweight layout boxes. (Reusable `Bx` layout pass added.)
5. Paint text, backgrounds, borders, links, controls, and images onto one surface. (Text, nested bold/italic inline runs, backgrounds, solid/dashed/dotted borders, links, images, and basic form controls are active.)
6. Add scrolling and hit testing from layout boxes. (Visible-box painting and bounded vertical scrolling added.)
7. Add compact table row/cell layout. (Equal columns, wrapped text, CSS boxes,
   bounded `colspan`, nested blocks, clickable links, malformed-markup recovery,
   and narrow-view reflow are active.)
8. Add a compact flexbox subset. (`flex`/`inline-flex`, row/row-reverse and
   fixed-height column/column-reverse, stable bounded order, fixed/equal-share
   auto/integer weighted grow and shrink slots, bounded row wrapping, explicit
   pixel min/max floors, pixel basis and row/column gaps, between/around/evenly
   main-axis distribution, and container/item cross-axis alignment are active.
   Intrinsic min-content sizing, fractional
   factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis
   alignment, and auto-height column reordering remain outside this subset.)
9. Add bounded responsive CSS. (`screen`/`all` pixel `min-width`, `max-width`,
   exact `width`, comma alternatives, four nested `@media` levels, and
   `<style media>` are active through compact `Vw` aliases. WinForms resize
   activation preserves live fields; print, negated, and non-pixel queries stay
   outside the supported subset.)
10. Add bounded CSS custom properties. (Stylesheet declarations now compile
    once; inherited case-sensitive values use lazy copy-on-write storage and
    compact `S.Vc/Cv` plus `G.Vc` aliases. Nested `var()` fallback, cycles,
    inline styles, dynamic state, media activation, and strict count/name/value/
    depth/output ceilings are active.)
11. Add a bounded important cascade. (Each compiled declaration carries one
    priority bit; final top-level `!important` markers activate stylesheet and
    inline important tiers while specificity/source order, shorthands, custom
    properties, hover/focus, media activation, diagnostics, and retained fields
    stay on the existing compact paths.)
12. Add bounded selector functions. (`:is`, `:where`, and selector-list `:not`
    compile compound alternatives once for stylesheets and document queries.
    Maximum/zero specificity, forgiving `:is`/`:where`, strict `:not`, dynamic
    state, outer combinators, eight alternatives, and sixteen total authored
    tests are active; nested selector-list functions and inner combinators stay
    outside.)
13. Add bounded structural arithmetic. (`:nth-child`, `:nth-last-child`,
    `:nth-of-type`, and `:nth-last-of-type` compile signed integer, odd/even,
    and `An+B` expressions to two integers. Stylesheets, document queries,
    selector-list alternatives, specificity, and forward/reverse matching share
    32-character and absolute 8,192-value bounds.)
14. Add bounded filtered structural positions. (`:nth-child(... of S)` and
    `:nth-last-child(... of S)` compile strict compound lists into the existing
    selector array, count matching siblings in forward/reverse order, and add
    maximum filter specificity. Eight alternatives and sixteen total authored
    tests are active; nested lists, inner combinators, and typed filters stay
    outside.)
15. Add bounded relational selectors. (Strict `:has(...)` lists compile to
    compact relative records with implicit descendant plus leading child,
    adjacent, and following relationships. Four-part chains, eight
    alternatives, sixteen total parts, maximum argument specificity, and 4,096
    candidate visits are active for stylesheets and queries. Descendant focus
    rebuilds preserve fields; nested selector-list functions and
    pseudo-elements stay outside.)
16. Add bounded CSS Grid columns and rows. (`grid`/`inline-grid` retain lazy
    compact `Gd/Rd` arrays with `Gc/Gv` and `Rc/Rv` inspection plus direct-item
    `Gl/Gs` and `Rl/Rs` placement. Up to eight fixed pixel or integer-fr tracks
    per explicit axis, simple repeat/minmax forms, pixel gaps, starts, two-axis
    spans, bounded occupancy, stable order, implicit rows, auto-height settling,
    nested flow, paint, hits, fields, resize, benchmark, Inspector, and
    Firespider host paths are active. Bounded named-area templates add equal-row,
    dot-cell, filled-rectangle, 8x8/16-name validation; compact `Aa/Ar/Ac/Ag/An`
    aliases; definite-first placement; and implicit columns. Bounded named line
    groups add lazy `Gnc/Rnc/Gx/Rx` inspection, `Gn/Ge/Rn/Re` item edges,
    unique 16-name-per-axis validation, area-generated start/end lines, and
    mixed name/span placement. Escapes, repeated-name occurrence indexes, named
    spans, intrinsic tracks, auto-fit/fill, and subgrid stay outside. Compact
    `Gf=r/d/c/x` adds sparse/dense row/column auto placement, earlier-hole
    backfill, equal-share implicit columns, non-overlapping eight-column
    overflow, and real-height visual traversal. Repeating implicit column/row
    patterns add `S.Av` plus `Gac/Gav/Rac/Rav` inspection, pixel/auto/integer-fr
    and minmax sizing, definite endpoint/span growth, area-created track sizing,
    and exact auto-height row settlement. Atomic `grid-template`/`grid` forms add
    explicit rows/columns, optionally sized quoted areas, row/column auto-flow
    and dense syntax, explicit auto tracks, and standard owned-property resets
    without resetting gaps. Compact `Ji/Js/Ai/As` item alignment adds
    stretch/start/center/end container defaults and overrides plus atomic
    `place-items`/`place-self`. Compact `Al/Jc` track-group alignment adds
    start/center/end/between/around/evenly distribution plus atomic
    `place-content`. Reverse flow, pre-explicit implicit tracks, quoted area-row
    line names, intrinsic/percentage tracks, baseline alignment, and writing-mode
    axis reversal stay outside.)
17. Add bounded inline SVG. (`B.Vg` carries a compact `Vg` viewport and `Vs`
    shape list; `Vp` holds canonical absolute path steps. Intrinsic dimensions,
    aligned none/meet/slice `viewBox`, groups, SVG links, six basic shapes, text,
    `M/L/H/V/Z/C/S/Q/T/A` paths, and bounded matrix/translate/scale/rotate/skew
    transforms share normal layout, clipping, scrolling, hit, fragment,
    and graphic-accessibility behavior. Presentation and inline fill/stroke,
    widths, opacity, font size, text anchor, and `currentColor` inherit through
    eight levels under 128-shape/point and 256-text limits. Compact `Vd/Vt`
    linear/radial paint servers add bounded stops, local inheritance, units,
    focus, transforms, fills, and strokes. Compact `Vc` clip definitions and
    expanded local `use` instances add group/symbol reuse, user/object clip
    units, transforms, paint, and hit clipping. Patterns, masks, filters,
    animation, external resources, nested SVG, and Canvas stay outside.)

This is the largest speed milestone: fewer controls, less UI overhead, and one
predictable rendering pass.

## 5. Network and resources

- HTTP and HTTPS loading through a small request layer.
- URL-encoded POST navigation with synchronous and asynchronous `Po` aliases.
- Redirects, response status, content type, content encoding, and character encoding. (Final response URL and compact page metadata are active.)
- GZip/Deflate response decompression. (Both are decoded transparently and covered by loopback tests.)
- Image loading for PNG, JPEG, GIF, and ICO where the platform supports them. (Basic async image loading and bounded memory cache added.)
- A small memory and disk cache with explicit size limits. (Bounded memory LRU
  plus an opt-in gzip persistent cache with atomic writes, validator reuse,
  private isolation, and entry/byte ceilings are active.)
- Cache freshness from `max-age`, `Expires`, `no-cache`, `no-store`, and a configurable default. (Active.)
- Conditional cache revalidation with ETag/If-None-Match and
  Last-Modified/If-Modified-Since. (`304` responses reuse the decoded body and
  refresh its lifetime; explicit reload remains unconditional.)
- Compact typed `Er` failures for URL, file, HTTP, network, timeout, cancellation, and decoding problems. (Active.)
- Basic cookies and a clear permission boundary for local files and scripts.
  (A bounded `CookieContainer` now handles HTTP cookie rules; `Kc`, `Km`,
  `Kcl()`, and `Pv` expose count, limit, clearing, and isolated private mode.)

## 6. Lea scripting

The first scripting layer should be a deliberately small command language, not a
full JavaScript replacement.

```text
t("Title")       set title
a("Message")     alert
bg("#fff")       page background
fg("#222")       text color
fs(12)           font size
bg("id", "#fff") element background
fg("id", "#222") element text color
fs("id", 18)      element font size
ti("id", "18px") element first-line indent
Go("url")        navigate
```

The bounded layer is active in `Lea.Engine`. It accepts `t`, `a`, `bg`, `fg`,
`fs`, `dr`, `ti`, `v`, `tx`, `ac`, `rc`, `tc`, `hd`, `sh`, `op`, `cl`, `tg`, `ds`, `en`,
`vl`, `ck`, `fo`, and host-controlled `go`, plus equivalent
title/body-style assignments and `alert(...)`.
Only top-level literal commands run. Execution is limited to eight inline
blocks, 16 KB per block, 32 accepted commands, and 16 queued alerts. External
scripts, JSON data blocks, expressions, control flow, and unknown calls are
ignored. `Je` is the host permission switch and `Js` is the compact manual
bridge. The WinForms host emits `Al` and exposes `Am` instead of allowing page
code to open native UI itself. `Ck` safely dispatches button and link attributes;
`P.Vs` holds 16 bounded variables and `tx` performs ID-scoped text replacement.
Literal `document.getElementById("id")` statements now map familiar text,
value, checked/disabled/hidden/open, focus, classList, and bounded style operations
onto those same compact commands. Quoted IDs/literals and literal booleans are
required; computed expressions, selectors, `innerHTML`, callbacks, and unknown
members remain inert.
`go` resolves one pending HTTP, HTTPS, or safe file-origin URL against `P.Bu`;
`E.Gt` consumes it, while `V.Gu`/`Gr` expose it to browser policy. Every command
has a short alias, a fixed limit, and a local fixture. A full
JavaScript runtime can remain a separate comparison project.

## 7. Browser shell

- Tabs and per-tab history.
- Address bar with local file and web URL support.
- Back, forward, reload, stop, and home.
- Bookmarks saved to a small JSON file.
- Downloads with a visible destination and status.
- Find-in-page.
- Page source and a small developer panel showing nodes, styles, requests, and logs.
- Keyboard shortcuts that do not complicate the engine API.

Downloads and find-in-page are active in the .NET Framework engine and WinForms
view. `Dl`/`DlA` stream local, HTTP, and HTTPS responses through an explicit
host-selected destination, decode HTTP content encoding, use a same-folder
temporary file, and reject payloads above 64 MB. `Fd` is capped at 256 matches;
the WinForms view highlights painted matches and cycles them with `Nx`/`Pr`.
Download links raise a host request rather than gaining direct file access.

Bounded developer diagnostics are active through `Dg`. The compact `G` snapshot
contains page/response counts, up to 512 redacted tree lines, 256 compiled CSS
summaries, and the latest 32 local/network/cache/download events. `Dcl` clears
request activity. `Lea.Win.Test` presents Summary, Nodes, CSS, Requests, and
Messages tabs in a resizable inspector without adding controls to rendered pages.
Firespider now exposes the same bounded data in a separate Lea Inspector window
from its View menu or a Lea tab's context menu. `F12` opens it, `R` refreshes,
and `C` clears only request activity. Legacy tabs remain on their original path.

## 8. Safety and reliability

- Never execute arbitrary native code from a page.
- Keep script commands allow-listed.
- Isolate local file access behind explicit browser behavior.
- Handle broken markup, failed images, bad URLs, and network errors gracefully.
- Add regression fixtures before expanding the feature set.

## 9. Testing and speed work

- Parser tests for tags, nesting, attributes, malformed HTML, and entities.
- CSS tests for selector priority, inheritance, and computed values.
- Layout tests for wrapping, spacing, scrolling, and hit testing.
- Engine tests for cache hits, reloads, history, redirects, and failures.
- Browser smoke tests using the `test-pages` folder.
- Benchmarks for startup, parse time, render time, memory, and EXE size.
- A compressed release build alongside the readable development build.

## 10. Release milestones

### Lea 0.1: Tiny core

HTML text, links, local files, compact aliases, history, and a small test window.

### Lea 0.2: Fast renderer

Document tree, CSS cascade, layout boxes, one-surface painting, scrolling, and images.

### Lea 0.3: Useful browser

Tabs, bookmarks, cache, forms, downloads, find-in-page, and robust networking.

### Lea 0.4: Lea language

Safe short commands, events, page updates, examples, and a standalone Lea API.

### Lea 1.0: Small dependable browser

Stable API, repeatable EXE packaging, benchmarks, developer tools, documentation,
and a clear compatibility list.

## 11. Visual Studio integration

Lea must be easy to add to a normal Visual Studio browser project without
bringing in another browser engine.

### Project shape

- `Lea.Engine`: platform-neutral core library containing networking, parsing,
  document nodes, CSS, layout, cache, and the compact API.
- `Lea.Render`: small rendering library with a platform adapter boundary.
- `Lea.Win`: optional Windows adapter for WinForms/WPF drawing and input.
- `Lea.Browser`: optional ready-made test browser shell.
- `Lea.Micro`: tiny single-file experiment and size benchmark.

The custom engine libraries must have no dependency on Chrome, Chromium,
WebView, Safari, Edge WebView, or another embedded browser runtime.

### How a project will consume Lea

Support three simple Visual Studio paths:

1. Add the Lea projects to the solution with a Project Reference during development.
2. Reference a built `Lea.Engine.dll` and the optional platform adapter directly.
3. Use a small local package or release folder containing DLLs, symbols, docs,
   and the Lea icon for larger projects.

### API boundary

- Keep the compact aliases stable after Lea 0.4.
- Keep UI code separate from the engine so a user's own browser window can host it.
- Expose page loading, navigation, document access, layout, painting, input,
  downloads, and permissions through small interfaces.
- Avoid exposing internal parser classes or WinForms controls from the core.
- Provide one short example that opens a page inside a user's own `Form`.
- Provide a second example that uses Lea without any browser window for testing.

### Compatibility targets

- First target: Windows and Visual Studio with .NET 8.
- Keep core code independent enough that a later Windows or cross-platform
  adapter does not require changing the document and layout engine.
- Record any platform-specific image, font, networking, or input behavior.

### Integration tests

- A clean sample Visual Studio app references Lea only through the public API.
- The sample opens a local page, a web page, follows a link, paints a page,
  and handles a failed request.
- The sample build does not install or load any external browser runtime.
- A versioned API smoke test catches accidental alias or interface changes.

## Current next milestone

Milestone 103 adds bounded inherited CSS list marker images through compact
`S.Li`, defining `li`, and the existing atomic `ls` shorthand. CSS
`list-style-image` supports bounded URLs, CSS-wide values, variables, priority,
hover, media, readable commands, and literal DOM assignments. The shorthand
accepts one type, one image, and one position in any order with atomic reset and
standard `none` ambiguity. Marker images reuse the existing sandboxed loader and
cache, preserve aspect ratio inside 64x64 pixels, use inside/outside LTR/RTL
geometry, and fall back to marker text after failure. See `LIST_IMAGE.md`. All
99 probes pass over 94 fixtures; the full default suite and compiled Firespider
host pass, including all 18 startup regressions, all 42 Boot cases, and all six
real EXE startup scenarios. Next investigate another bounded HTML/CSS
compatibility slice while keeping `::marker`, marker-side,
cross-item gutter alignment, custom counters, generated content, marker image
sizing, SVG marker decoding, vertical writing modes, full Unicode bidi shaping,
`dir=auto`, and general CSS/JavaScript evaluators outside the current boundary.

Keep the stable empty UI Automation visible-range fallback until a native-safe
non-empty bridge survives both direct and external MTA clients. Continue host
coverage and bounded HTML/CSS compatibility work without adding another browser
runtime.

The latest visible-range experiment also reused the already-marshaled
`TextPattern` object as the returned range identity and explicitly retained its
COM interface pointer. Both current .NET and .NET Framework external clients
still terminated with an access violation on the first range `GetText` call.
The unsafe non-empty array was removed and the proven empty fallback remains.

The current picture hardening adds 4 MiB streaming body limits, one asynchronous
pass deadline, 32 MiB retained bitmap accounting through `Ic/Ib`, failed-image
retry, and local-picture origin checks. See `PICTURE_LIMITS.md` for exact bounds
and `IMAGE_PREFLIGHT.md` for header/worker/process admission. `PROGRESS.md` records verified milestone status
and the separate Firespider startup checks.

An earlier completed milestone adds bounded `text-overflow` through non-inherited
`S.To=c/e` clip/ellipsis state. Strict values, explicit inheritance, CSS-wide
resets, invalid preservation, variables, priority, hover, and responsive rules
feed paint-only truncation for overflowing clipped lines. Original line sizes,
authored text, and pointer targets survive; collapsed spaces inside links now
retain their target. Synthetic markers and bounded text-element prefixes cover
blocks, mixed inline runs, explicit lines, unbreakable words, and tables.
Logical automation text stays complete while visible text geometry follows
the painted prefix. Horizontal RTL lines place the marker at physical left and
map partial/point ranges through their physical direction. Native controls, SVG
  text, two-ended/custom markers, Unicode bidi shaping, and vertical layout
  remain outside text-overflow; bounded line clamping is implemented separately
  by Milestone 100.

The preceding completed milestone adds bounded inherited `letter-spacing` and
`word-spacing` through compact `S.Ls/Wg`. Signed -32..128 integer pixels,
normal/reset values, parent-restoring inherit/unset/revert, invalid preservation,
variables, priority, inline overrides, and responsive rules share the cascade.
Retained line layout, button sizing, glyph paint, hit/focus/find rectangles,
and automation geometry use shared metrics with no line-edge tracking and
rounded average inline boundary gaps. Word-only spacing preserves shaped words;
zero-spacing pages retain the fast path. Blocks, tables, unfocused controls,
hover reflow, authored field retention, and exact text-element metrics are
covered by `text-spacing.html`, the benchmark, and compiled Firespider smoke.
Native editor/popup spacing, full bidi/complex-script tracking, and relative or
fractional spacing units remain outside this milestone.

The preceding completed milestone adds bounded inherited CSS `text-transform`
through compact `S.Tt=n/u/l/c` state and shared `E.Tx` presentation handling.
Strict none/uppercase/lowercase/capitalize parsing, variables, important tiers,
CSS-wide inheritance/resets, malformed preservation, inline overrides,
multiline text, tables, semantic values, buttons, unfocused controls, dynamic
hover, and responsive rebuilds share the retained text path. DOM text, mutable
field and option values, form submission, and focused native editor text keep
their authored casing. UI Automation keeps logical source text while mapping
case-transformed visual segments back to geometry. Exact engine, renderer,
automation, editor, benchmark, and compiled Firespider coverage use
`text-transform.html`.

The previous completed milestone adds bounded inherited CSS pointer interaction
through compact `S.Cu/Pe` state. Ten cursor codes cover automatic/default,
pointer, text, wait/progress, crosshair, move, help, not-allowed, and horizontal
or vertical resize families; `pointer-events:auto/none` supplies one mouse gate.
Variables, important tiers, inherit/reset behavior, invalid preservation,
dynamic hover, and responsive rebuilds share the compiled cascade. Retained
boxes, links, summaries, fields, fitted image maps, and linked SVG shapes use
one z-ordered gate with underlying fallthrough, while paint, layout, keyboard
focus, access keys, and accessibility bounds remain stable. Exact cursor, hit,
hover, click, field, benchmark, and compiled Firespider coverage use
`pointer-interaction.html`.

The previous completed milestone adds bounded CSS outlines through compact
non-inherited `S.Ow/Os/Oc/Oo` state. Atomic shorthand and longhands accept
solid, dashed, dotted, and auto style, 0..32 pixel width, signed -64..64 pixel
offset, named/hex/functional alpha colors, `currentColor`, variables, important
tiers, CSS-wide resets, malformed preservation, responsive rules, and dynamic
`:focus`. Rounded boxes, images, fields, opacity, stacking, and ancestor
overflow share one painter; layout, hits, accessibility bounds, native editor
placement, and following flow remain stable. Exact pixels, retained focus,
benchmark, and compiled Firespider coverage use `outlines.html`.

The earlier completed milestone adds bounded CSS `aspect-ratio` through compact
non-inherited `S.Rw/Rh/Ra` state. One positive number or two-part ratio plus
optional intrinsic `auto` fallback compiles atomically under strict digit,
component, and resolved-ratio bounds. Variables, important tiers, CSS-wide
resets, malformed preservation, and responsive rebuilds share the existing
cascade. One 8,192-pixel-capped resolver serves content/border boxes, normal
and nested flow, text minimums, raster/SVG intrinsic sizing, fields, flex,
grid, and table minimums while explicit dimensions and min/max clamps win.
Exact geometry, pixels, benchmark, and compiled Firespider coverage use
`aspect-ratio.html`.

The earlier completed milestone adds bounded replaced-image fitting through
compact `S.Of=f/c/v/n/s` and `Ox/Oy/Oxp/Oyp` state. Fill, contain, cover, none,
and scale-down use one 8,192-pixel-capped positioned object rectangle with
keyword, signed pixel, unitless compact pixel, or percentage coordinates.
Variables, important tiers, resets, invalid preservation, noninheritance, and
responsive rebuilds share the existing cascade. Clipped raster paint and
image-map pointer, keyboard, and accessibility geometry share that rectangle,
so letterboxes are inert and cropped source coordinates remain aligned without
changing layout. Exact pixels, benchmark, and compiled Firespider coverage use
`object-fit.html`.

The earlier completed milestone adds bounded CSS bitmap backgrounds through
compact `S.Bi` and `Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` state. One atomic
layer accepts bounded data, local file, HTTP, or HTTPS URLs; repeat, repeat-x,
repeat-y, or no-repeat; pixel or percentage positions and sizes; auto,
cover, and contain sizing; linked-stylesheet and custom-property origins;
variables, important tiers, resets, malformed preservation, and
noninheritance. The existing 24-entry, 4 MB-per-image cache now feeds rounded
page, box, and ordinary-field paint with effective opacity, ancestor clipping,
responsive loading, stable layout/hits, benchmark, and Firespider host
coverage. Multiple layers and attachment/origin/clip/blend modes remain outside
this bounded subset.

The previous completed milestone adds bounded CSS background gradients through
compact `S.Gi`, `Gi.K/A/X/Y/Ss`, and `Cs.C/P` state. One atomic linear or radial
layer accepts up to eight normalized stops, integer degree or direction heads,
and centered or percentage radial origins. Supported colors include named,
hex, `rgb()`/`rgba()`, alpha, `currentColor`, and transparent forms; variables,
important tiers, resets, malformed preservation, and noninheritance use the
existing cascade. Rounded page, box, and field paint shares effective opacity,
ancestor clipping, stable layout/hits, resize, benchmark, and Firespider host
coverage. Repeating gradients, multiple layers, length stops, and radial size
keywords remain outside this bounded subset.

The previous completed milestone adds bounded CSS box shadows through compact
`S.Sw` lists and `Sw.X/Y/B/R/C/I` offsets, blur, spread, color, and inset state.
Atomic lists accept up to four entries; offsets and spread cap at 512 pixels,
blur caps at 128 pixels, and painting uses at most 12 concentric bands. Hex,
named, `rgb()`/`rgba()`, modern slash alpha, `currentColor`, variables,
important tiers, resets, malformed preservation, and noninheritance feed one
rounded outer/inset painter. Exact alpha/blur/field pixels, unchanged layout and
link hits, resize, benchmark, and Firespider host coverage remain on the
existing retained box surface.

The preceding completed milestone adds bounded CSS corner radii through compact
clockwise `S.R0/R1/R2/R3` horizontal and `Q0/Q1/Q2/Q3` vertical state. Atomic
pixel/percentage circular and elliptical shorthands, corner longhands, custom
properties, important/reset behavior, malformed preservation, and strict value
bounds feed one CSS-normalized GDI+ path for backgrounds, solid/dashed/dotted
borders, inherited overflow clipping, and ordinary fields. Exact paint, link,
resize, benchmark, and Firespider host coverage remain on the existing box
layout surface.

The previous completed vector milestone adds bounded SVG winding and stroke styles through
compact `Vs.Fr/Lc/Lj/Ml/Da/Do` state. Specificity-ordered stylesheet,
presentation, inline, custom-property, and important declarations share the
inherited paint path for nonzero/evenodd fills and clips, butt/round/square caps,
miter/round/bevel joins, bounded miter limits, and normalized dash lists/offsets.
Native region unions, GDI+ pen settings, painted holes, expanded linked hits,
strict invalid preservation, exact tests, benchmark, and Firespider host coverage
all remain inside the existing retained vector surface.

The previous completed milestone added bounded reusable SVG geometry and clipping
through compact `Vg.Cs`, `Vs.Cp/Cl/Cm`, `Vc`, and `S.Cp` aliases. Local
shape/group/symbol/nested `use` expansion, user/object clip units, transforms,
multi-shape unions, reusable clip children, presentation/CSS attachment,
group-owned clip matrices, paint, and link-hit intersection reuse Lea's retained
path under 64-source, 16-clip, 32-clip-shape, four-link, and 128-painted-shape
ceilings with cycles, empty clips, and missing references failing closed.

The latest completed grid milestone added bounded Grid content alignment.
Compact `Al/Jc=s/c/e/b/a/v` state, one/two-value atomic `place-content`, and the
shared integer distribution helper position fixed track groups across definite
block/inline axes. Exact center/end/between/around/evenly geometry, resets,
invalid preservation, paint, hits, benchmark, and Firespider host coverage use
the existing track arrays and renderer.

The previous grid milestone added bounded Grid item alignment. Compact
`Ji/Js/Ai/As` state, start/center/end/stretch defaults and overrides, atomic
`place-items`/`place-self`, measured simple-text auto width, resolved-row block
alignment, exact geometry, paint, hits, benchmark, and Firespider host coverage
all reuse the existing slot and renderer path.

The previous grid milestone added bounded atomic `grid-template` and `grid`
shorthands. Explicit rows/columns, optionally sized quoted areas,
row/column auto-flow and dense forms, explicit auto tracks, standard reset
boundaries, and preserved gaps all compile into the existing aliases. Scratch
state makes malformed sides, areas, tracks, flow markers, and extra slashes
atomic. Exact geometry, paint, hits, benchmark, and Firespider host coverage use
the existing placement map and renderer.

The previous grid milestone added bounded repeating
`grid-auto-columns/rows` sizing. `S.Av` plus `Gac/Gav/Rac/Rav` inspect up to
eight pixel, auto, integer-fr, or minmax entries; patterns cycle after explicit
tracks, size area-created tracks, honor definite endpoints and largest spans,
and contribute exact pixel minima to auto-height row settlement. Atomic invalid
handling, reset values, geometry, paint, hits, benchmark, and Firespider host
coverage all share the existing placement map and custom renderer.

The previous grid milestone added bounded `grid-auto-flow` through one
`Gf=r/d/c/x` alias. Sparse/dense row and column cursors, earlier-hole backfill,
equal implicit columns, an eight-column overflow fallback, auto-height visual
traversal, exact geometry, invalid values, benchmark, and Firespider coverage
all share the existing placement map and custom renderer.

The previous grid milestone added bounded unique named lines to column
and row track lists. Lazy exact maps expose `Gnc/Rnc/Gx/Rx`, items retain
`Gn/Ge/Rn/Re`, named areas generate start/end edges without duplicate storage,
and shorthand/longhand resolution shares definite-first occupancy, exact
geometry, invalid-limit, benchmark, and Firespider host coverage.

The latest completed selector milestone added bounded strict `:has(...)` relative lists
with compact precompiled relationship/part records, descendant, child,
adjacent, and following matching, four-part chains, maximum specificity,
deterministic traversal limits, dynamic focus retention, shared queries,
WinForms geometry, benchmark probes, Inspector diagnostics, and Firespider.

The previous milestone added bounded `of S` filters to `:nth-child` and
`:nth-last-child`. Strict compound alternatives reuse compiled selector arrays,
matching-sibling forward/reverse counts, maximum filter specificity, shared
functional limits, stylesheet/query behavior, dynamic state, retained fields,
WinForms geometry, benchmark probes, Inspector diagnostics, and Firespider.

The previous milestone added precompiled `An+B` arithmetic and reverse
structural positions with compact `A/B` records and strict bounds.

The earlier selector-list milestone added bounded `:is`, `:where`, and `:not`
functions with compound alternatives, maximum/zero specificity, forgiving/
strict parsing, dynamic state, and host coverage.

The earlier important milestone added a bounded cascade with one compiled bit,
normal/important stylesheet and inline tiers, compact `G.Ic` diagnostics, and
dynamic/host coverage.

The earlier custom-property milestone added compiled declarations and inherited
CSS custom properties behind `S.Vc/Cv` and `G.Vc`, with lazy copy-on-write maps,
nested fallback, strict limits, dynamic state, and responsive rebuild coverage.

The earlier responsive milestone added bounded CSS behind compact `E/V/P/G.Vw`
aliases. Screen/all pixel conditions compile once and activate only at width
boundaries while preserving fields.

The previous milestone added an optional gzip persistent cache behind the
existing request layer. Compact `Cd/Cx/Cb/Dc/Db/Cdl` aliases retain a memory-only
default, while atomic versioned records, LRU ceilings, validator reuse,
private-mode isolation, and recovery are covered across the release suites.

The current milestone makes representation identity ordinal after `Uri`
canonicalization. Path and query casing stay distinct in memory, persistent
filenames and metadata, validators, linked CSS, and navigation; fragments alone
still share one representation. An older entry is reusable only when its stored
exact key matches; case-mismatched collisions are rejected and no entries are
silently migrated. See `CACHE_IDENTITY.md`.

The previous milestone added non-inherited compact `S.Bz=c/b` state for
`content-box` and `border-box`. One shared conversion now interprets pixel
width, height, min/max, and flex-basis values before blocks, fields, images,
flex items, tables, and cells enter layout. Exact wide and 180-pixel geometry,
non-inheritance, invalid-value rejection, paint, links, accessibility bounds,
benchmark, and Firespider host probes preserve the original content-box path.

The earlier milestone overloaded `bg/fg/fs` with bounded two-argument
forms that style one safe element ID while preserving their one-argument body
forms. Existing named/hex colors, 8..96px font limits, safe target filtering,
4KB inline-style cap, and field-preserving rebuild apply unchanged. Page scripts,
manual host calls, click commands, WinForms, benchmark, and Firespider all verify
the scoped cascade and rejection paths.

The earlier milestone added compact `S.As` item alignment plus
`S.Jc=a/v` space-around and space-evenly distribution. A shared bounded integer
helper now distributes main-axis room for rows and columns, while direct items
can override container alignment with stretch/start/center/end. `flex.html`,
exact remainder geometry, stretch, narrow reflow, paint/hit/flow tests,
benchmark, and Firespider host probes cover the public state and rendered path.

The earlier milestone added compact `S.Or` state with stable signed -64..64
ordering on nowrap rows and fixed-height columns, plus `S.Fd=c/y` for column and
column-reverse. `Lea.Win` allocates bounded vertical grow/shrink slots with
`Hn/Hx` protection, reverse main flow, justification, and cross-axis alignment;
auto-height columns retain natural source-order stacking.

The earlier milestone added compact `S.Sh` state and bounded integer
`flex-shrink`. One-, two-, and three-part shorthand retains its shrink weight;
`flex:none` locks an item. `Lea.Win` clamps each outer pixel basis through
`Wx/Wn`, removes line deficit by basis-scaled weight, iteratively freezes items
at explicit minimums, and preserves measurable overflow when no legal room
remains.

The earlier flex milestone extended bounded CSS flex rows with compact
`S.Wr/Rg/Fg/Fb` aliases for wrapping, row gaps, integer growth, and pixel basis.
One/two-value and separate gaps plus common `flex` shorthand feed a per-line
allocator that retains row reversal, distributes free width by growth weight,
and advances wrapped lines by settled item height.

The previous milestone added bounded `op/cl/tg` page commands for
opening, closing, and toggling `details` by its own ID or first direct `summary`.
They synchronize `N.Op` and `open`, preserve nested edited fields through the
shared rebuild, reject duplicate summaries and unrelated targets, and are
covered through engine, WinForms, benchmark, and Firespider hosts.

The previous milestone added bounded visual-line snapshots for plain wrapped
blocks and inline flow, visual `TextUnit.Line` expansion and movement, merged
per-line rectangles, and proportional partial-range cropping. The public MTA
client, repeatable benchmark, and Firespider host smoke verify two-line ranges
and one-line navigation without adding a retained text tree.

An earlier milestone added a bounded read-only `ITextProvider` over `P.X` and
settled layout ownership. Document, child, and point ranges support bounded text,
search, read-only attributes, character/word/line/document navigation, rendered
bounds, enclosing and embedded elements, scroll-to-view, and stale-page
rejection. Password and file values are excluded. The public MTA client,
benchmark, and Firespider host smoke verify the pattern; `GetVisibleRanges`
returns a stable empty collection. Managed arrays, explicit COM identities,
manual `SAFEARRAY(VT_UNKNOWN)` construction, native-callable adapters, and
external MTA clients all produced invalid UIA range handles when non-empty. A
full x86/x64 C++ provider/range wrapper, private scalar-offset bridge, official
native SAFEARRAY construction, retained lifetimes, IID metadata, and standard
versus agile marshaling produced the same invalid handle and were removed, so a
future bridge needs a different platform boundary rather than another wrapper.

The previous milestone added `IGridProvider`, `ITableProvider`,
`IGridItemProvider`, and `ITableItemProvider` over the existing `Tb/Tr/Tc`
model. It exposes zero-based row/column coordinates, bounded loaded dimensions,
column spans, containing-grid links, and row/column header relationships. The
MTA client and Firespider host smokes verify the native patterns without a new
browser runtime or retained table tree.

The repeatable compatibility and performance report is now implemented by
`Lea.Bench`. It probes supported HTML, CSS, forms, scripting, layout, downloads,
and diagnostics, then records local load timing, bitmap paint timing, retained
managed memory, binary footprint, and compressed runtime size in
`COMPATIBILITY_PERFORMANCE.md`.

List-item blocks now carry compact paint-ready `B.Mk` markers. Unordered lists
use `-`; ordered lists use bounded invariant decimal markers and honor safe
`start` and per-item `value` integers. Each nested list computes its own scope,
increments saturate at 999999, and malformed or oversized values fall back
without unbounded text. Simple blocks, inline runs, table-cell parts, measurement,
paint, and resize all share the same marker text path through `lists.html`.

Details elements now carry compact page-local `N.Op` state. The first direct
summary uses `B.Mk` as `>` or `v`; all content masked by a closed ancestor keeps
its block and form models behind `B.Hd`. `E.Ds`/`V.Ds` toggle through one bounded
rebuild that reapplies `[open]` CSS and preserves fields and nested state.
`disclosure.html` covers pointer and Enter/Space activation, duplicate and stray
summaries, summary-free host control, table cells, nesting, paint, and resize.

Same-page fragments now use `E.Ft/Fg/Rv` and matching `V` aliases. Targets are
decoded under a 4,096-character ceiling, resolve IDs before named anchors, and
reuse the current page through a 128-entry history without another request.
Closed details ancestors reveal in one rebuild; links, sync/async Go,
Back/Forward, table targets, top and missing fragments, direct loads, and resize
placement share the retained layout path covered by `fragments.html`.

Progress and meter blocks now carry compact `B.Vm` state. `V/Mn/Mx/R` expose
normalized values and paint ratios; meters add ordered `Lo/Hi/Op/Q` regions and
progress uses `In` for indeterminate state. Numeric text and magnitude are
bounded before clamping. Normal flow, inherited styles, table cells, opacity,
clipping, painting, and resize all share the custom path in `gauges.html`.

Output and data elements now carry compact `Dv` state through `P.Dvs`, `B.Dv`,
and `Ir.Dv`. One-letter `K` kinds distinguish output and data; `V/X/N/F/Fo/El`
expose bounded current, visible, naming, reference, form, and ownership data.
Values stop at 4,096 characters, `for` stops at 32 unique bounded ID tokens,
and safe `tx/text` updates retain their 512-character ceiling. Inline styles,
standalone blocks, table cells, class/text rebuilds, and resize remain on Lea's
custom renderer and share the fixture in `values.html`.

Time and edit elements now reuse the same `Dv` ownership path. Compact `K`
values `t/i/r` distinguish time, inserted, and removed content; `Dt` retains at
most 128 authored datetime characters and `U` resolves bounded edit citations
only through file, HTTP, or HTTPS. A time without `datetime` uses its child text
as the machine value. Citations never become navigation links. Native
underline/strike style, nested inline ownership, standalone and table blocks,
safe rebuilds, resize, and Firespider hosting share `time-edits.html`.

Image maps now use `P.Mps`, image-owned `B.Mp`, and compact `Ar` areas. Named
maps and hash references are bounded; rectangle, circle, polygon, default, and
dead shapes retain finite capped coordinates and DOM ownership. `B.Mw/Mh`
separate the authored coordinate canvas from current CSS layout, allowing one
shape-aware hit path to scale, clip, rebuild, and preserve first-area priority.
Area links, fragments, commands, and downloads stay on existing host routes.
Malformed shapes stay readable but inert. Engine, WinForms, resize, and
Firespider coverage share `image-maps.html`.

Sequential keyboard focus now stays on that same custom surface. `Ti` parses a
bounded nullable tabindex, while `Ke/Kn/Ki/Kb/Kf/Ka` expose focus state,
geometry, traversal, and activation to hosts. Positive values precede normal
document order, negative values remain click-only, and the first 256 rendered
targets include links, repeated image-map regions, enabled fields, summaries,
and explicit generic regions after a 4,096 map-region scan cap. Focus reapplies compiled CSS, paints a default
ring, scrolls off-screen targets, wraps predictably, and keeps Ctrl/Alt+Tab for
host chrome. Engine, WinForms, editor, repeated-map, Firespider, and resize
coverage share `keyboard-focus.html`.

Post-1.0 focus shortcuts now extend that model without a parallel control path.
`Af/Afs` expose the first 32 authored autofocus candidates, while `Ak/Aks`
parse 16 unique ASCII letter/digit alternatives from at most 256 characters.
The WinForms view selects the first visible focusable candidate on loads without
a resolved fragment target, assigns at most 64 conflict-free keys in document order, and
exposes `An/Ak/Kl/Aa` for hosts. Firespider keeps first refusal over command
keys; only an assigned, otherwise-unclaimed `Alt+Shift+key` is consumed. Focus,
editor opening, commands, links, mapped areas, fragments, downloads, and
navigation reuse existing bounded routes. Engine, WinForms, command-message,
editor, fragment, limit, benchmark, and Firespider coverage share
`focus-shortcuts.html`. HTML-hidden nodes receive default layout/paint/link/hit
suppression and remain excluded from focus, autofocus, and access-key assignment.

HTML tokenization now uses a small quote-aware scanner rather than a single
tag-splitting regex. It recognizes comments and declarations, preserves raw
`script/style` text, decodes title/textarea RCDATA and normal text entities,
keeps malformed `<` delimiters as text, and caps scan steps, tree nodes, stack
depth, attributes, names, and attribute values. `html-tokens.html` plus a
generated stress document verify each limit.

Document queries now reuse the compiled CSS selector subset. `E.Id/Q/Qa` and
`V.Id/Q1/Qa` provide ID, first document-order match, and a read-only list of at
most 256 matches. Query text, groups, and total selector parts are capped;
invalid or unsupported selectors return no matches rather than entering an
unbounded fallback parser. `V.Q1` preserves the older `V.Q` URL-query alias.

Document base resolution now honors the first valid `<base href>` under a
4,096-character ceiling. `P.Bu` and `V.Bu` expose the result while `U` remains
the response URL. Stylesheets, links, images, forms, and download targets share
the resolved HTTP, HTTPS, or file base; invalid and unsupported candidates are
skipped, and pages without a valid base fall back to `U`.

Safe class mutation now uses `ac/rc/tc` for add, remove, and toggle against one
validated element ID. Names are 64 characters or fewer, each element retains
at most 32 tokens and 512 class-attribute characters, and metadata/script
targets are rejected. Valid no-op adds/removes remain accepted commands;
remove and toggle clear duplicate matches. The existing single CSS rebuild per
run preserves mutable fields across page, host, click, and WinForms execution.

Bounded element-state mutation now uses `hd/sh` for authoritative visibility,
`op/cl/tg` for disclosure state, `ds/en` for supported field availability, `vl`
for current values, `ck` for checkbox/radio state, and `fo` for one pending
focus request. IDs reuse the
existing 64-character boundary; field values stop at 1,024 characters; files
and button values cannot be injected; select/color values are validated; and
passwords remain redacted. A retained field map keyed by the existing DOM node
preserves identity and live state while command-hidden. `E.Kp` takes focus
headlessly, while `V` resolves it against final rendered targets and clears
focus invalidated by hide or disable. `state-commands.html` covers inline,
host, click, WinForms, boundary, retained-state, benchmark, and Firespider paths.

The `inset` shorthand now expands atomically into `Tp/Rt/Bm/Lt`, and opposing
edges stretch auto-sized absolute and fixed boxes against their containing
rectangle. Supported pixel `translate`, `translateX`, and `translateY` values
use compact `Tx/Ty` aliases, apply after normal positioning without consuming
flow, and create the existing `Zc` local stacking context. The
`position-values.html` fixture verifies cascade reset, used edges, horizontal
and vertical stretch, parent-relative translation, trapped high-z descendants,
paint order, click routing, and following normal flow.

Nested fields, responsive images, and structured tables now retain their
nearest ordinary visual owner through `B.Up`, including fields reached through
an unstyled form wrapper. They participate in parent-relative width, ordered
flow, automatic height, inherited overflow clipping, and following root flow.
The WinForms editor intersects its content box with the inherited clip and
viewport before it becomes visible, so focused fields cannot paint outside a
clipped ancestor. `nested-content.html` verifies ownership, geometry, image
paint, table height, clipped editing, and mutable field state together.

`S.Za` now distinguishes numeric z-index from `auto`, and `S.Zc` records local
opacity or `isolation:isolate` context creation. Fixed boxes remain contexts.
The hierarchical paint and hit path therefore traps descendants only when the
parent establishes a compact context; positioned `z-index:auto` descendants can
participate in the ancestor layer. Auto-height context owners use one bounded
measurement pass followed by final placement, so bottom/right offsets and
overflow clips resolve against settled geometry without making flat pages pay
for another layout pass.

Ordinary visual containers now retain compact layout ownership through `B.Up`
when backgrounds, borders, spacing, or dimensions make their own box meaningful.
The layout closes auto-height contexts from the deepest subtree outward, grows
each parent around normal-flow descendants, then places its next sibling or root
block. Mixed direct text, nested links, three-level containers, and narrow-view
reflow share this path. Unstyled semantic wrappers remain flattened so simple
pages keep the original low-overhead layout.

Nested positioning now retains compact parent relationships through `B.Up`.
The WinForms layout records each context's content rectangle, child-flow cursor,
inherited clip, and fixed ancestry. Absolute descendants resolve against their
nearest positioned container; nested fixed descendants stay viewport anchored.
Paint and hit testing use the same hierarchical z-index path, so a high-z child
cannot escape beneath or above its parent's stacking layer. A focused fixture
covers nested top/right/bottom/left offsets, inherited overflow clipping,
overlapping links, normal-flow continuation, and fixed behavior while scrolling.

File fields now use compact `Fi.Fp/Ac/Mu/En` state and `V.Sf` selection. The
native picker and multipart serializer enforce eight-file, 4 MB per-file, and
8 MB combined bounds; HTML file values never become local paths. Synchronous
and asynchronous submit paths send successful controls and binary file content
through bounded `Pb/PbA` requests. Range and color have dedicated painting and
interaction, while time, month, week, local datetime, multiple email, and step
validation share the existing compact validity path.

Form fields now parse compact `Rq/Mn/Mx/Pa/Lo/Hi/Nv` constraints and expose
`Ok/Vm` results. `V.Vf` and both submit paths validate required controls,
radio groups, selected values, lengths, bounded patterns, email/URL syntax,
numbers, and ISO dates. Invalid forms remain on-page, receive visible state and
`:invalid` CSS, and suppress submit events; `novalidate` remains an explicit
bypass. Select, textarea, and button child text no longer paints twice.

Positioning uses compact `S.Pn/Tp/Rt/Bm/Lt/Zi` fields and `B.Up` context links.
Relative blocks and tables move while preserving their normal-flow slot.
Absolute boxes leave flow and resolve against the nearest positioned ancestor;
fixed boxes remain in viewport coordinates through nested context chains.
Inherited overflow clips constrain descendants. Hierarchical z-index paths use
one stable order for painting, hover selection, and overlap clicks. Auto-sized
positioned containers and the full CSS stacking-context trigger set remain
later work.

Responsive images now select compact `srcset` width or density candidates and
screen-compatible `<picture>` sources before falling back to `<img src>`.
Base64 `data:image/...` resources decode under a 4 MB bound and share the same
24-entry image cache. HTML dimensions apply when CSS does not override them,
and tiny embedded pixels use stable nearest-neighbor expansion.

Dynamic `:hover` and `:focus` selectors are compiled with the static selector
set. Paint blocks and runs retain a compact `El` element reference, allowing
`E.Dy` and the WinForms view to reapply styles without loading or reparsing the
page. Form objects are reused so edits, checks, and selections survive restyles.

`S.Vi` stores inherited `v/h/c` visibility. Hidden and collapsed boxes retain
layout but skip every paint, hit-registration, and native-editor path; a child
may explicitly return to visible. `S.Op` stores the effective `0..100` opacity
after combining ancestor and local decimal/percentage values. The opaque fast
path is unchanged, while faded text, boxes, borders, controls, and images use
alpha painting. Lea intentionally uses lightweight primitive alpha rather than
a full stacking-context compositor.

General sibling (`~`) chains scan the preceding element siblings once and keep
the nearest compiled match. `:not(...)` accepts one simple selector containing
tag, universal, ID, class, attribute, or an existing basic pseudo-class. Its
argument contributes specificity directly, and the argument is compiled once
with the rest of the rule. Complex selector lists and nested negation remain a
later extension.

`S.Wn/Wx/Hn/Hx` store positive pixel min/max dimensions. The shared layout
clamps apply the declared size, maximum, and then minimum consistently to normal
blocks, fields, tables, and nested table content while keeping minimum widths
inside the available viewport. `S.Wb` stores `n/b/a/k` for normal, emergency
break-word, anywhere/break-all, and keep-all behavior. `word-break`,
`overflow-wrap`, and legacy `word-wrap` all use the existing inline layout pass.

Lea compiles every simple selector once when its stylesheet loads. Attribute
presence plus `=`, `~=`, `|=`, `^=`, `$=`, and `*=` matching are active, as
are root, empty, link, fixed/odd/even child and type positions, only/first/last
positions, initial checked/disabled/enabled/required/read-only/read-write form
states, and live hover/focus state. Hover follows the pointed element and its
ancestors; focus follows the active form element.

Lea now supports inherited
left, center, right, and basic justified text plus pixel, percentage, and
unitless line heights in normal blocks and table cells. Default text still uses
the original one-box path; only requested text flow enters the word-line pass.

`S.Ff` stores a bounded, compact font fallback list with generic aliases, and
`V` caches up to 32 resolved Windows families. `S.Td` uses `n/u/s/us` for none,
underline, strike, or both; `S.Tc/Tds/Tw` add optional color,
solid/double/dotted/dashed/wavy style, and bounded integer thickness. CSS
`text-decoration` plus its line/color/style/thickness longhands, compact
`td/tdl/tdc/tds/tdw`, plain link overrides, and native `u/s/strike/del` tags
share native solid or lightweight custom paint paths.

Inherited `S.Uo/Uop` stores a signed integer `text-underline-offset` and its
pixel/percentage kind. Values are bounded to -32..32 pixels or -100..100
percent, and resolved paint is clamped to 64 pixels. Compact `uo`, readable
`textunderlineoffset`, and literal `style.textUnderlineOffset` share the
compiled cascade without resetting `S.Td/Tc/Tds/Tw`.

Inherited `S.Ta/Tl` stores all-line/final-line alignment as compact physical
`l/c/r/j` or logical `s/e` state; empty means logical start/auto respectively.
CSS `text-align`, `text-align-all`, and `text-align-last`, compact `ta/taa/tl`,
readable command names, and literal `style.textAlign*` assignments share the
compiled cascade. Ordinary shorthand values reset final-line state, while
`justify-all` sets both and the all-line longhand preserves `Tl`. The existing
word-line pass applies alignment to all visual lines and the line immediately
before a forced break. Horizontal logical values resolve at layout, except
`match-parent`, which resolves using the parent direction; vertical writing and
full Unicode bidi shaping remain outside this bounded slice.

Inherited `S.Wm/Wt` stores wrapping mode and style as compact empty/`w`/`n`
and empty/`b` state. CSS `text-wrap`, `text-wrap-mode`, and `text-wrap-style`,
compact `tw/twm/tws`, readable command names, and literal `style.textWrap*`
assignments share the compiled cascade. The shorthand accepts mode/style in
either order and resets omitted parts; `white-space` updates mode without
clearing style. The existing word-line pass performs bounded same-line-count
balance for auto-width text groups. Stable, pretty, avoid-short-last-line,
vertical writing, and arbitrary CSSOM evaluation remain outside this slice.

Inherited `S.Tt=n/u/l/c` stores bounded none, uppercase, lowercase, or
capitalize presentation state. One invariant `E.Tx` helper transforms block,
inline, table, value, button, placeholder, option-label, and unfocused field
display strings while raw DOM and form values remain unchanged. Dynamic and
responsive rebuilds recompute display text and retain mutable field identity.

Inherited `S.Ls/Wg` adds bounded integer-pixel letter and word spacing to the
same text path. Layout and paint share text-element advances, line-edge gap
suppression, rounded inline boundary averages, and shaped word-only runs.
Tables, buttons, custom fields, responsive rebuilds, and automation bounds
reuse retained ownership while raw authored text stays unchanged.

CSS relationship matching supports descendant, direct-child (`>`), and
adjacent-element (`+`) combinators with or without spaces. Matching runs from
right to left, skips text nodes for adjacency, includes every simple selector
in specificity scoring, and compiles each selector once when CSS is loaded.
