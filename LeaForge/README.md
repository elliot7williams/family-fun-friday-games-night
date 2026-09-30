# LeaForge

LeaForge is a new, bigger browser-engine experiment for Lea.

The earlier projects stay small:

- `Lea.Micro` is the compressed tiny version.
- `LeaBrowser` is the simple test window.
- `LeaPlus` adds more HTML/CSS/script tests.

LeaForge is where we can grow the more full-featured browser.

## Current Features

- bounded PNG/JPEG/GIF/BMP/ICO preflight, zero-capability AppContainer
  `Lea.Image.exe` decoding with an identity-scoped executable ACL, verified
  restricted low-integrity token, suspended Job prebinding, explicit inherited
  pipes, process mitigations, shared `Ip/Im/It/In/If` accounting, compact
  `Ie/Ii/Iw/Ik/Ij/Ih/Ipm/Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk/Iac/Ind/Ial`
  sandbox diagnostics, verified content-addressed helper staging for FAT32 and
  other non-ACL volumes, `Icl/Ihm/Icm/Iad` combined host/helper admission, and
  cross-view bitmap accounting; see `IMAGE_PREFLIGHT.md` and
  `IMAGE_ISOLATION.md`
- exact URI-normalized cache identity with case-sensitive path/query resources,
  fragment sharing, validator isolation, and persistent recovery; see
  `CACHE_IDENTITY.md`
- bounded document/POST and linked-CSS input, gzip/deflate expansion limits,
  shared I/O budgets, local-sheet origins, and `Hm/Csm`; see `INPUT_LIMITS.md`
- bounded streaming picture reads, one pass deadline, gzip/deflate images,
  retained bitmap budgets, failed-image retry, and `Ic/Ib`; see `PICTURE_LIMITS.md`
- compact `Cn` navigation admission, owned async view completion, coordinated
  picture handoff, and Firespider callback/reload tracking; see `LOAD_LIFECYCLE.md`
- compact `Ss/Sd/Sn` named native CSS, retained editors, saved Firespider styles,
  and request-scoped startup previews; see `NATIVE_STYLESHEETS.md`
- guarded Firespider saved-style startup/timer callbacks without legacy casts
  or automatic fallback; see `STYLE_STARTUP_SAFETY.md`
- guarded clipboard paste and compact `Pa(text)` host insertion with native
  undo, bounded text, and protected targets; see `PASTE_SAFETY.md`
- compact `Ud` retained-field protection and optional native Firespider
  hibernation for eligible idle tabs; see `HIBERNATION.md`
- compact native load failures and explicit GET retry through `Fe/Le/Cr/Ry`,
  retained tabs/status, and no POST replay; see `LOAD_FAILURES.md`
- tabs
- back and forward history per tab
- reload
- bookmarks
- source viewer
- local HTML file loading
- basic HTTP page loading
- bounded in-memory page cache plus an optional compressed persistent cache for faster repeat navigation across launches
- basic HTML rendering for headings, paragraphs, bounded unordered/decimal lists, details disclosures, links, buttons, images, code, pre blocks, and structured tables
- bounded named CSS counters through compact `S.Cr/Ci/Cs`, defining
  `cr/ci/cs`, tree-order scope, eight active instances/operations,
  32-character ASCII names, and named `counter()`/`counters()` marker output;
  see `CSS_COUNTERS.md`
- page-local `<details>`/`<summary>` state with compact markers, nested retention, pointer activation, and Enter/Space keyboard activation
- bounded quote-aware HTML tokenization with comments, declarations, raw text, RCDATA, entities, and malformed-delimiter recovery
- compact `Id`, `Q`/`Q1`, and read-only bounded `Qa` document queries
- bounded `<base href>` resolution through compact `Bu`
- retained same-page fragments through compact `Ft`/`Fg`/`Rv`, 128-entry history, percent-decoded IDs, named anchors, details reveal, and `Fa/Fj/Fy` first-paint/resize/intrinsic-image placement diagnostics; see `INITIAL_FRAGMENT_STABILITY.md`
- custom `<progress>` and `<meter>` painting through compact bounded `B.Vm` values, meter quality regions, inherited styles, and table-cell reflow
- compact `<output>` and `<data>` values through `P.Dvs`/`B.Dv`/`Ir.Dv`, bounded metadata, safe updates, and inline/table ownership
- compact `<time>`, `<ins>`, and `<del>` metadata through `Dv.K/Dt/U`, safe citation schemes, native decoration, rebuilds, and inline/table ownership
- compact `<map>`/`<area>` models through `P.Mps`/`B.Mp`/`Ar`, bounded shape coordinates, dead-area layering, responsive custom hits, links, and downloads
- bounded keyboard navigation through `Ti` and `Ke/Kn/Ki/Kb/Kf/Ka`, ordered tabindex targets, shared-map regions, visible rings, scrolling, and host activation
- bounded autofocus and host-safe page shortcuts through `Af/Afs`, `Ak/Aks`, and `An/Kl/Aa`, with conflict fallback, resolved-fragment suppression, and `Alt+Shift` activation
- bounded semantic accessibility through `Ax/Axs/St/Is` and `V.Ab/Ao`, with HTML/ARIA roles, labels, hierarchy, protected values, rendered bounds, WinForms/MSAA children, native UI Automation fragments, interaction and `Tb/Tr/Tc` grid/table patterns, plus read-only logical TextPattern ranges with direction-aware visual-line, partial-range, and point geometry
- bounded standards-shaped `CSS.supports()` checks through compact
  `E/V.Su` overloads and literal allow-listed `if/else` command guards, reusing
  the existing declaration/selector/logic parsers and shared command limits;
  see `SUPPORTS_QUERIES.md`
- bounded standards-shaped `@container` custom-property `style()` queries over
  nearest or named ancestors, with computed `S.Cn`, grouped `not`/`and`/`or`,
  nested media/supports/layers, retained rebuilds, and diagnostics; compact
  `S.Ct` plus atomic `E.Cz` measurements add nearest/named width, height,
  reduced-integer aspect-ratio, and portrait/landscape size queries with
  automatic bounded WinForms feedback; scroll-state queries remain
  unsupported; see `CONTAINER_STYLE_QUERIES.md`,
  `CONTAINER_SIZE_QUERIES.md`, and `CONTAINER_RATIO_ORIENTATION.md`
- bounded standards-shaped media matching through compact `E/V.Mq` aliases and
  literal global/window `matchMedia(...).matches` guards, reusing current
  viewport, DPI, preferences, whole-query `not`, strict `only`, and shared
  command limits; see `MATCH_MEDIA.md` and `MEDIA_MODIFIERS.md`
- wrapped inline bold/italic text, styled spans, line breaks, and links inside text blocks
- inherited font-family fallbacks plus native current-color underline and
  line-through decoration
- inherited bounded text transformation through compact `S.Tt=n/u/l/c` and
  shared `E.Tx`, with inline/control presentation and raw DOM/form preservation
- inherited signed pixel letter/word spacing through compact `S.Ls/Wg`, with
  shared wrapping, inline boundaries, control paint, and retained geometry
- inherited signed pixel/percentage first-line indentation through compact
  `S.Ti/Tip`, with bounded `ti` commands, LTR/RTL wrapping, and retained
  paint/hit/accessibility geometry; see `TEXT_INDENT.md`
- one inherited bounded text-shadow layer through compact nullable `S.Ts` and
  `ts`, with signed offsets, optional blur/color, lightweight paint, stable
  geometry, and literal DOM support; see `TEXT_SHADOW.md`
- bounded inherited text-decoration line/color/style/thickness through compact
  `S.Td/Tc/Tds/Tw` and `td/tdl/tdc/tds/tdw`, with atomic shorthand and
  longhands, native solid fast path, lightweight double/dotted/dashed/wavy
  paint, stable geometry, and literal DOM support; see `TEXT_DECORATION.md`
- bounded inherited `text-underline-offset` through compact `S.Uo/Uop` and
  `uo`, with signed integer pixels/percentages, shorthand independence,
  offset-aware default-link paint, stable geometry, and literal DOM support;
  see `TEXT_UNDERLINE_OFFSET.md`
- bounded inherited `text-wrap`, `text-wrap-mode`, and `text-wrap-style`
  through compact `S.Wm/Wt` and `tw/twm/tws`, with strict shorthand reset,
  declaration-order `white-space` interaction, same-line-count balancing,
  eight-line/2,048-character work bounds, and literal DOM support; see
  `TEXT_WRAP.md`
- inherited `text-align` shorthand and `text-align-all` through compact
  `S.Ta/Tl` and `ta/taa`, with retained logical start/end state,
  `justify-all`, match-parent, strict shorthand reset, and literal DOM support;
  see `TEXT_ALIGN.md`
- bounded inherited `text-align-last` through compact `S.Tl` and `tl`, with
  auto/physical/logical/justified final and forced-line alignment, horizontal
  LTR/RTL resolution, stable boxes, and literal DOM support; see
  `TEXT_ALIGN_LAST.md`
- non-inherited clip/ellipsis text overflow through compact `S.To=c/e`, with
  bounded LTR/RTL physical inline-end markers, grapheme-safe paint-only
  prefixes, retained link targets, and authored automation text
- bounded non-inherited `line-clamp` and `-webkit-line-clamp` through compact
  `S.Lc=0/1..32` and defining `lc`, with grapheme-safe LTR/RTL markers, reduced
  flow height, hidden hit removal, full logical text, and literal DOM support;
  see `LINE_CLAMP.md`
- bounded inherited `list-style-type` and type/image/position `list-style` through compact
  `S.Lst=n/d/c/s/1/0/a/A/r/R` and defining `lst`, with HTML type/reversed/start/
  value support, bounded bullet/decimal/alpha/Roman formatting, RTL markers,
  no-marker spacing suppression, and literal DOM support; see `LIST_STYLE.md`
- bounded inherited `list-style-position` through compact `S.Lp=o/i`, `lp`, and
  atomic type/image/position `ls`, with true outside logical gutters, inside first-line
  participation, shorthand reset/order, LTR/RTL geometry, and literal DOM
  support; see `LIST_POSITION.md`
- bounded inherited `list-style-image` through compact `S.Li` and `li`, reusing
  Lea's sandboxed image cache/decoder, with atomic three-component shorthand,
  64px intrinsic caps, text fallback, inside/outside LTR/RTL geometry, and
  literal DOM support; see `LIST_IMAGE.md`
- bounded terminal `::marker` styling through compact `B.Ms` plus `S.Mc/Ma/Mn`,
  with isolated text properties, up to eight quoted/`attr()`/implicit
  list-counter/generated-quote fragments, one quoted missing-value attribute fallback, and 128
  generated units, nested paths, image precedence, page/item `ms` and readable
  `markerstyle` commands, and shared LTR/RTL paint; see `MARKER.md`,
  `MARKER_CONTENT.md`, `ATTR_FALLBACKS.md`, `MARKER_COUNTERS.md`, and
  `GENERATED_QUOTES.md`, and `GENERATED_ALT_TEXT.md`
- inherited left/center/right/justify alignment and pixel/relative line spacing
- external stylesheets with document-order cascade and shared bounded caching
- compiled relationship, attribute, structural, link, and form-state selectors,
  plus bounded `:is`/`:where`/list-`:not` alternatives and specificity
- precompiled bounded `An+B` arithmetic for forward/reverse child and type
  positions, with shared stylesheet and `Id/Q/Qa` query behavior
- strict bounded `nth-child(... of S)` and `nth-last-child(... of S)` compound
  filters with matching-sibling counts and maximum filter specificity
- strict bounded `:has(...)` relational lists with descendant, child, adjacent,
  following-sibling, and four-part compiled chains, maximum specificity,
  dynamic focus restyling, and deterministic traversal limits
- bounded responsive `screen`/`all` plus fixed non-matching `print`, whole-query
  `not`, no-op `only` before media types, pixel width/height,
  portrait/landscape, integer aspect-ratio, `dpi`/`dppx`/`dpcm` resolution,
  light/dark `prefers-color-scheme`, and no-preference/reduce
  `prefers-reduced-motion`, no-preference/reduce `prefers-reduced-data`,
  no-preference/less/more/custom `prefers-contrast`,
  none/initial-only/enabled `scripting`, and primary/union
  `pointer`/`hover` interaction media queries with strict/inclusive modern
  ranges, compact `Vw/Vh/Vz/Dp/Th/Rm/Pdt/Pc/Je/Pt/Hv/Ap/Ah/Mi` aliases,
  nested conditions, live WinForms viewport/DPI/preference/device updates, and
  retained fields; see
  `MEDIA_RANGE_SYNTAX.md`, `MEDIA_RESOLUTION.md`, `MEDIA_COLOR_SCHEME.md`,
  `MEDIA_REDUCED_MOTION.md`, `MEDIA_REDUCED_DATA.md`, `MEDIA_CONTRAST.md`,
  `MEDIA_MODIFIERS.md`, `MEDIA_SCRIPTING.md`, and `MEDIA_INTERACTION.md`
- bounded CSS `@supports` declaration checks and `selector(...)` capability
  checks with grouped `not`/`and`/`or`, nested media, compact `@s` diagnostics,
  strict depth/term/text limits, and retained fields; see `SUPPORTS_QUERIES.md`
- bounded named CSS `@layer` blocks with later normal, unlayered normal,
  reversed important, and strongest inline order; reopened/nested names,
  native-sheet merging, conditional nesting, compact `@l` diagnostics, and
  strict name/layer limits; see `CASCADE_LAYERS.md`
- compiled declarations plus bounded inherited CSS custom properties with
  compact `S.Vc/Cv`, nested `var()` fallback, lazy maps, and strict limits
- bounded normal/important cascade for supported declarations, with stylesheet
  and inline tiers, compact `G.Ic` diagnostics, and quoted/function boundaries
- horizontal logical `inline-size`/`block-size` plus min/max and direction-aware
  logical inset longhand/axis shorthands, reusing physical records and geometry;
  short `is/bs` and familiar DOM aliases; see `LOGICAL_SIZING_INSETS.md`
- horizontal direction-aware logical margin/padding axes and longhands with
  signed bounded margins, atomic parsing, twelve compact aliases, familiar DOM
  forms, and shared box geometry; see `LOGICAL_BOX_MODEL.md`
- horizontal direction-aware logical border axes, edges, and width/style/color
  components with atomic physical lists, lazy per-edge records, 24 compact
  aliases, familiar DOM forms, hidden used widths, shared geometry, and mixed
  rounded paint; see `LOGICAL_BORDERS.md`
- horizontal direction-aware logical corner radii mapped onto the existing
  physical ellipse fields, with explicit inheritance/reset, atomic parsing,
  nine compact aliases, readable/familiar DOM forms, authored restore, and
  shared asymmetric paint; see `LOGICAL_RADII.md`
- inherited horizontal `direction: ltr/rtl` through compact `S.Dr=l/r`, HTML
  `dir`, pre-cascade resolution, translated cross-direction logical inheritance,
  `text-align: start/end`, direction-aware row/reverse/wrapped flex and column
  cross alignment, logical grid auto/dense/column/explicit/area placement and
  alignment, right-origin plain/mixed/wrapped/nowrap/justified inline flow,
  nested list/disclosure markers, inline-end ellipsis and TextPattern geometry,
  and compact live `dr` reflow; see
  `DIRECTION.md`
- live `:hover` and `:focus` restyling that preserves mutable form values
- non-inherited `content-box`/`border-box` sizing across pixel dimensions,
  min/max clamps, flex basis, fields, images, tables, and cells
- min/max dimensions plus normal, break-word, and break-all text flow
- inherited visibility and effective opacity across text, boxes, controls, and images
- default HTML `hidden` rendering suppression plus focus, autofocus, and access-key exclusion
- responsive `<picture>`/`srcset` selection and bounded base64 embedded images
- compact CSS cascade for named, hex, RGB/RGBA, bounded legacy/modern
  HSL/HSLA, modern HWB, modern CIE Lab/LCH, predefined sRGB/linear-sRGB/
  Display-P3/linear-Display-P3/A98-RGB/ProPhoto-RGB/Rec.-2020/XYZ `color()`, modern Oklab/OkLCh, and bounded
  two-stop sRGB `color-mix()` colors, fonts,
  boxes, overflow, and inline styles;
  hue formats accept degree/gradian/radian/turn units and normalized wrapping,
  while HWB adds clamped whiteness/blackness and gray normalization, CIE
  Lab/LCH add D50 conversion with Bradford D50-to-D65 adaptation, predefined
  `color()` adds sRGB, Display P3, A98 RGB, ProPhoto RGB, Rec. 2020, and D50/D65 XYZ spaces, and Oklab/OkLCh add
  D65 conversion with bounded local-MINDE sRGB gamut mapping
  across shared HTML/field/SVG paint; see `HSL_COLORS.md`, `HWB_COLORS.md`,
  `LAB_COLORS.md`, `PREDEFINED_COLORS.md`, `WIDE_GAMUT_COLORS.md`,
  `OKLAB_COLORS.md`, and `COLOR_MIX.md`
- bounded rounded outer/inset CSS box shadows through compact `S.Sw` and
  `Sw.X/Y/B/R/C/I`, atomic four-entry lists, RGB/HSL/HWB/Lab/LCH/predefined/Oklab/OkLCh/mixed alpha/current colors, variables,
  priority/reset/bounds, ordinary fields, and a 12-band blur ceiling
- bounded linear/radial CSS background gradients through compact `S.Gi`,
  `Gi.K/A/X/Y/Ss`, and `Cs.C/P`, with one atomic layer, eight normalized stops,
  RGB/HSL/HWB/Lab/LCH/predefined/Oklab/OkLCh/mixed alpha/current colors, variables, priority/reset/bounds, rounded page/box/field
  paint, opacity, clipping, and resize-safe hit geometry
- bounded `flex`/`inline-flex` row and column flow with compact
  `Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or` state, stable order and reversal,
  fixed/equal-share/weighted grow and shrink slots, explicit pixel min/max
  protection, row wrapping, pixel basis and row/column gaps, around/evenly
  distribution, container/item alignment, natural auto-height columns, nested
  content, and shared hit paths
- bounded `grid`/`inline-grid` columns and rows with compact `Gd/Gc/Gv` and
  `Rd/Rc/Rv` tracks plus `Gl/Gs` and `Rl/Rs` item placement, fixed pixel and
  integer `fr` sizes, simple `repeat()`/`minmax(0,Nfr)`, pixel gaps, bounded
  occupancy, two-axis spans, validated named-area templates with compact
  `Aa/Ar/Ac/Ag/An` aliases, unique named lines through `Gnc/Rnc/Gx/Rx` and
  `Gn/Ge/Rn/Re`, generated area edges, `Gf` row/column sparse/dense auto flow,
  repeating implicit track patterns through `S.Av` and `Gac/Gav/Rac/Rav`,
  atomic `grid-template`/`grid` explicit, quoted-area, and row/column auto-flow
  shorthands, compact `Ji/Js/Ai/As` item alignment with atomic
  `place-items`/`place-self`, `Al/Jc` track-group alignment with atomic
  `place-content`, definite-first placement, bounded implicit endpoints, nested
  content, paint, hits, fields, auto-height row measurement, and responsive
  reflow
- auto-height styled semantic containers with nested normal-flow ownership
- nested positioning with explicit z-index, opacity/isolation contexts, settled auto height, and inherited clipping
- bounded vertical sticky positioning through compact `S.Pn=k`, shared retained
  geometry, and nearest-parent clamps; see `STICKY_POSITIONING.md`
- compact inset stretching plus translated block stacking contexts
- nested fields, responsive images, and tables with clip-aware native editing
- bounded multipart file uploads plus range, color, and temporal form fields
- tiny script handling for `alert("...")` and `document.title = "..."`
- bounded engine scripting for title, body colors, font size, and host-routed alerts
- safe button/link commands with bounded variables and ID-scoped text updates
- bounded `ac`/`rc`/`tc` class mutations with compiled CSS restyling
- bounded `hd/sh`, `op/cl/tg`, `ds/en`, `vl`, `ck`, and `fo` element-state commands
- literal `document.getElementById(...)` text/state/style/class/focus translation
- bounded `dp` display mutation and literal host-controlled location translation
- bounded percentage/calculated widths with `wd` and literal `style.width`
- bounded definite percentage/calculated heights with `ht` and literal `style.height`
- bounded `vw`/`vh`/`vmin`/`vmax` dimensions and calculations with compact
  `Wu/Hu/Vh` aliases; see `VIEWPORT_UNITS.md`
- bounded `em`/`rem` dimensions and calculations with compact `Wu/Hu/Rf`
  aliases; see `FONT_RELATIVE_UNITS.md`
- bounded two/three-term `min()`/`max()` and three-term `clamp()` dimensions
  with lazy `Wk/Hk/Wq/Hq/Wv/Hv` aliases; see `SIZE_FUNCTIONS.md`
- bounded developer snapshots plus a tabbed inspector with an Access view in the test browser
- repeatable compatibility, timing, memory, binary-size, and compression report
- streamed local/HTTP downloads with explicit destinations and a 64 MB ceiling
- highlighted, case-insensitive find-in-page with next/previous cycling

## Lea Short Commands

LeaForge accepts compact aliases for common page commands:

```text
t("Title")       title
a("Message")     alert
bg("#fff")       background
fg("#222")       text color
fs(12)           font size
bg("id", "#fff") element background
fg("id", "#222") element text color
fs("id", 18)      element font size
dp("id", "none") element display
wd("id", "50%")  element width
ht("id", "50%")  element height
lst("id", "lower-alpha") element list marker style
lp("id", "inside") element marker position
li("id", "url('marker.png')") element marker image
ls("id", "upper-roman url('marker.png') outside") list style shorthand
dr("rtl")         body direction
dr("id", "rtl")  element direction
mi("id", "8px 12px") inline margin
mb("id", "6px")      block margin
pi("id", "8px 12px") inline padding
pb("id", "6px")      block padding
v("name", "Lea") variable
tx("id", "Hi ${name}") element text
ac("id", "hot")       add class
rc("id", "hot")       remove class
tc("id", "hot")       toggle class
hd("id")              hide element
sh("id")              show element
op("id")              open details
cl("id")              close details
tg("id")              toggle details
ds("id")              disable field
en("id")              enable field
vl("id", "value")     set bounded field value
ck("id", "1")         set or toggle checked state
fo("id")              request rendered focus
go("next.html")        request navigation from the host
be("id", "content:'New '") element before style
af("id", "content:' done'") element after style
be("id", "content:'[!] ' / 'Important '") accessible before text
qu("id", "'<' '>'") element generated quote pairs
```

The longer `alert(...)` and `document.title = ...` forms still work. The short
forms are intended for small, fast Lea pages and are limited to simple values.
Literal `document.getElementById("id")` statements may use `textContent` or
`innerText`, field/state booleans, `focus()`, `classList.add/remove/toggle`, and
the already bounded background/color/font-size/display/width/height/direction
and logical margin/padding/border/radius styles. Lea translates
these familiar forms into the short commands above; it does not expose arbitrary DOM
objects, expressions, `innerHTML`, callbacks, or a general JavaScript runtime.
`go` never navigates the engine directly: it queues one bounded HTTP, HTTPS, or
safe local-file request for the browser host to approve and load.
Literal `location`, `location.href`, and `location.assign(...)` forms use that
same request path. See `DISPLAY_NAVIGATION.md`.
`wd` accepts fixed pixels, percentages, and one bounded `calc(P% +/- Npx)` form;
authored width/min/max values use the same model. See `CALCULATED_WIDTHS.md`.
`ht` applies the same syntax to height/min/max, resolving percentages only when
the containing height is definite. See `CALCULATED_HEIGHTS.md`.
State commands remain ID-scoped and preserve retained field identity through a
single rebuild. Values stop at 1,024 characters, file values cannot be set,
passwords stay redacted, and `E.Kp`/the WinForms view consume only one valid
focus request. Disclosure commands accept a `details` ID or its first direct
`summary` ID and keep `N.Op` plus the authored `open` attribute synchronized.
`be/af` and readable `beforestyle/afterstyle` apply the bounded inline generated
content subset described in `GENERATED_CONTENT.md`. `qu`, readable `quotes`, and
literal `style.quotes` configure the bounded generated-quote pairs described in
`GENERATED_QUOTES.md`. Visual `url(...)` fragments for before, after, and marker
content reuse the sandboxed image path described in `GENERATED_IMAGES.md`.

The .NET Framework test browser opens Lea Inspector with `F12`, `Ctrl+Shift+I`,
or the `I` toolbar button. Its Summary, Nodes, CSS, Requests, and Messages tabs
use the compact `Dg` API; Access uses `V.Axs`. Neither path inspects private
renderer fields.
Firespider's experimental Lea tabs expose the same bounded inspector from the
View menu (`F12`) and the tab context menu, with refresh and request-log clearing.
Firespider's normal New Tab command and `Ctrl+T` now open Lea; **New Legacy Tab**
(`Ctrl+Alt+L`) keeps the original engine available for unmigrated features.

## Compact Engine API

Small C# programs can use the short engine facade:

```csharp
var e = new LeaForge.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
Console.WriteLine(p.N + " nodes, " + p.Ls + " links");
```

The main actions are `Go`, `Bk`, `Fw`, `Rl`, and `At`. Page values are shortened
to `U` (response URL), `Bu` (document base URL), `T` (title), `H` (HTML), `N`
(node count), and `Ls` (link count).

Alias naming is a core Lea rule. See [ALIASES.md](ALIASES.md) for the vocabulary
and limits used by future engine features.

Normal navigation reuses bounded parsed or decoded representations. `E` and `V`
stay memory-only until a host sets `Cd`; `Cx`/`Cb` bound persistent entries and
bytes, `Dc`/`Db` report its footprint, and `Cdl()` clears only Lea cache files.
Path and query casing remain exact while fragments share one representation; see
`CACHE_IDENTITY.md`. The Reload button bypasses both cache layers and fetches the
current page again.

## Run

Open `../Lea.sln` in Visual Studio after `LeaForge` is added to it, or compile the fallback executable from `Lea.Build`.

The compiled test exe is expected at:

```text
../dist/LeaExes/LeaForge.exe
```

After a Release solution build, regenerate the engine report with:

```powershell
..\Lea.Bench\bin\Release\Lea.Bench.exe -r .. -o COMPATIBILITY_PERFORMANCE.md -n 15
```

The report marks unsupported browser-platform features explicitly; a passing
fixture probe is not presented as full Chrome, Safari, Firefox, or HTML Living
Standard compatibility.

## Local Test Page

Open this path in LeaForge:

```text
LeaForge/test-pages/index.html
```

## Important

LeaForge is still our own experimental renderer. It does not use Chromium, Edge WebView, or a full JavaScript VM.
Lea.Engine now includes only a small allow-listed JavaScript-shaped command
bridge; it does not evaluate arbitrary JavaScript.
