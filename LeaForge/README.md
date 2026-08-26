# LeaForge

LeaForge is a new, bigger browser-engine experiment for Lea.

The earlier projects stay small:

- `Lea.Micro` is the compressed tiny version.
- `LeaBrowser` is the simple test window.
- `LeaPlus` adds more HTML/CSS/script tests.

LeaForge is where we can grow the more full-featured browser.

## Current Features

- tabs
- back and forward history per tab
- reload
- bookmarks
- source viewer
- local HTML file loading
- basic HTTP page loading
- bounded in-memory page cache plus an optional compressed persistent cache for faster repeat navigation across launches
- basic HTML rendering for headings, paragraphs, bounded unordered/decimal lists, details disclosures, links, buttons, images, code, pre blocks, and structured tables
- page-local `<details>`/`<summary>` state with compact markers, nested retention, pointer activation, and Enter/Space keyboard activation
- bounded quote-aware HTML tokenization with comments, declarations, raw text, RCDATA, entities, and malformed-delimiter recovery
- compact `Id`, `Q`/`Q1`, and read-only bounded `Qa` document queries
- bounded `<base href>` resolution through compact `Bu`
- retained same-page fragments through compact `Ft`/`Fg`/`Rv`, 128-entry history, percent-decoded IDs, named anchors, details reveal, and resize-safe scrolling
- custom `<progress>` and `<meter>` painting through compact bounded `B.Vm` values, meter quality regions, inherited styles, and table-cell reflow
- compact `<output>` and `<data>` values through `P.Dvs`/`B.Dv`/`Ir.Dv`, bounded metadata, safe updates, and inline/table ownership
- compact `<time>`, `<ins>`, and `<del>` metadata through `Dv.K/Dt/U`, safe citation schemes, native decoration, rebuilds, and inline/table ownership
- compact `<map>`/`<area>` models through `P.Mps`/`B.Mp`/`Ar`, bounded shape coordinates, dead-area layering, responsive custom hits, links, and downloads
- bounded keyboard navigation through `Ti` and `Ke/Kn/Ki/Kb/Kf/Ka`, ordered tabindex targets, shared-map regions, visible rings, scrolling, and host activation
- bounded autofocus and host-safe page shortcuts through `Af/Afs`, `Ak/Aks`, and `An/Kl/Aa`, with conflict fallback, resolved-fragment suppression, and `Alt+Shift` activation
- bounded semantic accessibility through `Ax/Axs/St/Is` and `V.Ab/Ao`, with HTML/ARIA roles, labels, hierarchy, protected values, rendered bounds, WinForms/MSAA children, native UI Automation fragments, interaction and `Tb/Tr/Tc` grid/table patterns, plus read-only logical TextPattern ranges with visual-line and partial-range geometry
- wrapped inline bold/italic text, styled spans, line breaks, and links inside text blocks
- inherited font-family fallbacks plus underline and line-through decoration
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
- bounded responsive `screen`/`all` pixel-width media queries with compact
  `Vw` aliases, nested conditions, live WinForms resize, and retained fields
- compiled declarations plus bounded inherited CSS custom properties with
  compact `S.Vc/Cv`, nested `var()` fallback, lazy maps, and strict limits
- bounded normal/important cascade for supported declarations, with stylesheet
  and inline tiers, compact `G.Ic` diagnostics, and quoted/function boundaries
- live `:hover` and `:focus` restyling that preserves mutable form values
- non-inherited `content-box`/`border-box` sizing across pixel dimensions,
  min/max clamps, flex basis, fields, images, tables, and cells
- min/max dimensions plus normal, break-word, and break-all text flow
- inherited visibility and effective opacity across text, boxes, controls, and images
- default HTML `hidden` rendering suppression plus focus, autofocus, and access-key exclusion
- responsive `<picture>`/`srcset` selection and bounded base64 embedded images
- compact CSS cascade for colors, fonts, boxes, overflow, and inline styles
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
  `Aa/Ar/Ac/Ag/An` aliases, definite-first placement, stable order, implicit
  tracks, nested content, paint, hits, fields, auto-height measurement, and
  responsive reflow
- auto-height styled semantic containers with nested normal-flow ownership
- nested positioning with explicit z-index, opacity/isolation contexts, settled auto height, and inherited clipping
- compact inset stretching plus translated block stacking contexts
- nested fields, responsive images, and tables with clip-aware native editing
- bounded multipart file uploads plus range, color, and temporal form fields
- tiny script handling for `alert("...")` and `document.title = "..."`
- bounded engine scripting for title, body colors, font size, and host-routed alerts
- safe button/link commands with bounded variables and ID-scoped text updates
- bounded `ac`/`rc`/`tc` class mutations with compiled CSS restyling
- bounded `hd/sh`, `op/cl/tg`, `ds/en`, `vl`, `ck`, and `fo` element-state commands
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
```

The longer `alert(...)` and `document.title = ...` forms still work. The short
forms are intended for small, fast Lea pages and are limited to simple values.
`go` never navigates the engine directly: it queues one bounded HTTP, HTTPS, or
safe local-file request for the browser host to approve and load.
State commands remain ID-scoped and preserve retained field identity through a
single rebuild. Values stop at 1,024 characters, file values cannot be set,
passwords stay redacted, and `E.Kp`/the WinForms view consume only one valid
focus request. Disclosure commands accept a `details` ID or its first direct
`summary` ID and keep `N.Op` plus the authored `open` attribute synchronized.

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
The Reload button bypasses both cache layers and fetches the current page again.

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
