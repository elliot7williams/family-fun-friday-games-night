# Using Lea In Visual Studio

Lea is intended to be a custom engine that a browser project can own and embed.
The target integration does not use Chrome, Chromium, Safari, Edge WebView, or
any other browser engine.

## Firespider host

Firespider Internet is currently a VB.NET WinForms application targeting .NET
Framework 4.6.1. Normal tabs and split view now use `Lea.Win.V`; explicit legacy
tabs and several unmigrated tools still use `System.Windows.Forms.WebBrowser`.
Main host commands remain concentrated in `1.vb`, with Lea-specific routing in
`FirespiderLea.vb` and split-window ownership in `FirespiderSplit.vb`.
Saved style automation also needs `Style-startup-host.patch` in
`BrowserFeatures2.vb` to avoid legacy casts at startup and on timer ticks. It
reports unsupported Lea preview without creating a legacy renderer; see
`STYLE_STARTUP_SAFETY.md`.

`../Lea.Engine` targets .NET Framework 4.6.1 and exposes `E`, `P`, and the short
Lea API. `../Lea.Win` provides the custom `V` WinForms view, and
`../Lea.Win.Test` is its isolated test browser.

The current Firespider target expects this layout relative to the VB project:

```text
FirespiderLea.vb
FirespiderSplit.vb
Lea/FirespiderLea.targets
Lea/Lea.Engine.dll
Lea/Lea.Win.dll
Lea/Lea.Image.exe
Lea/start.html
```

Import `Lea/FirespiderLea.targets` after applying the matching host patches. It
adds both VB source files and the Lea/framework references, and copies the
required image helper beside the browser EXE; do not separately add duplicate
Compile items for those files. Keep all three runtime files from the same build.

## Development reference

During development, add the Lea engine project to the same solution and use a
Project Reference. This gives the browser project source-level debugging and
lets both projects evolve together.

The .NET Framework project files import the standard C# build targets and pass
Release `Rebuild` through MSBuild, so Visual Studio can compile the engine,
WinForms adapter, tests, and test browser directly.

```xml
<ItemGroup>
  <ProjectReference Include="..\Lea.Engine\Lea.Engine.csproj" />
</ItemGroup>
```

## Current public shape

```csharp
var e = new Lea.Engine.E();
var p = await e.GoA("https://example.com");
```

For WinForms rendering, add a `new Lea.Win.V()` to your form with
`Dock = DockStyle.Fill` and call its `GoA` method. WPF embedding is not supplied
by this integration. The host controls the UI and navigation policy; Lea owns
the supported document model, layout, and painting.

Browser UI code should use `GoA` and `RlA` so network waits do not freeze the
window. `St` cancels the active request. The synchronous `Go` and `Rl` aliases
remain available for small tools and deterministic local tests.
`E.Tm` and `V.Tm` configure the request deadline in milliseconds; the compact
property defaults to 15,000 and clamps to 100 through 60,000. It applies to
synchronous and asynchronous HTTP page, POST, stylesheet, and download work.

`E.Ng` and `V.Ng` optionally accept a synchronous `Func<Uri, bool>` host policy
for explicit document navigation. False raises `Er.K="blocked"` before a page
fetch/history change; null allows the usual behavior. GET, POST/multipart,
reload, fragments, and Back/Forward use the guard. Keep callbacks short and do
not re-enter navigation. `V` releases the delegate on disposal and rejects
authorization if the callback disposes it. This is not a redirect, subresource,
or download firewall. Firespider installs it on ordinary and split-view Lea tabs.
Ordinary tabs enforce ownership, offline/lock state, and existing host rules;
denied resume retains its placeholder. Native keyboard/accessibility callbacks
handle expected denials while public APIs keep reporting them. See
`NAVIGATION_POLICY.md` and `SPLIT_VIEW.md` for tested behavior and scope.

Subscribe `V.Fe` on the UI thread to display `V.Le` after a typed document-load
failure. Public calls still throw the same `Er`. Offer explicit retry when
`V.Cr` is true and await `V.Ry()`; it rechecks navigation policy and never
replays POST. Cancellation does not emit failure. See `LOAD_FAILURES.md`.

`E.Ud(page)` and `V.Ud` conservatively detect field data worth retaining before
a URL-only reload, including hidden retained fields and selected file paths.
They do not move focus, write data, or provide a full state snapshot. Firespider
uses the query with privacy, activity, history, and state exclusions under its
existing opt-in hibernation policy. See `HIBERNATION.md`.

Use `V.Ce(code)` and `V.Ec(code)` on the UI thread for native field editing.
`V.Pa(text)` inserts supplied text without clipboard access, with native undo,
maxlength/Unicode bounds, and the same target protections. Clipboard paste
returns 0 when unavailable and preserves the selection; native messages and
shortcuts share that guard. Copy/cut also use bounded clipboard retries and cut
mutates only after the selected text is committed. See `PASTE_SAFETY.md`.
The two-letter commands `cp/cu/pa/sa/de/un/re` cover copy, cut, paste, select-all,
delete, and one-level undo/redo. Firespider's Edit menu now uses these routes,
including retained selection after opening its menu. These APIs do not provide
document-body selection or contenteditable. See `NATIVE_EDITING.md`.

Safe page code can call `go(url)` to request, but not perform, navigation.
Headless integrations call `E.Gt(page)` once after `Js`/`Ck`. WinForms hosts
subscribe to `V.Gr`, read the bounded target from `V.Gu`, and then choose whether
to call `GoA`. Relative targets use `P.Bu`; unsupported schemes, UNC paths, and
remote-page requests for local files are rejected before the host event.

Pages now expose a lightweight tree through `P.D`. Firespider integrations
should use that tree instead of expecting the legacy COM `HtmlDocument` object.
`P.St`, `Ct`, `Ce`, `Ch`, `Sz`, and `Ca` expose compact response diagnostics.
`Et` and `Lm` expose ETag and Last-Modified validators. `Ca` is also true when
a `304 Not Modified` response reuses Lea's cached decoded body.
`E` and `V` expose `Cm`, `Cc`, `Ch`, and `Cl()` for Lea's bounded in-memory
decoded-page cache; `Cs` sets default freshness seconds, while HTTP freshness
headers can override it. Stale validator-bearing entries send `If-None-Match` or
`If-Modified-Since`; `Rl` and `RlA` deliberately fetch a fresh response without
those conditional headers.

Persistent caching remains off until the Visual Studio host sets `Cd` to an
owned directory. `Cx` and `Cb` configure its entry and byte ceilings, `Dc` and
`Db` report the current footprint, and `Cdl()` clears Lea entries while leaving
unrelated host files alone. The defaults are 32 entries and 16 MB, with hard
ceilings of 128 entries and 128 MB. Private mode performs no persistent reads or
writes; versioned gzip entries use atomic replacement, validator reuse, LRU
trimming, and corrupt-file fallback. A host can opt in with:

```csharp
view.Cd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "My Browser", "Lea Cache");
view.Cx = 32;
view.Cb = 16L * 1024 * 1024;
```

Hosts can catch `Lea.Engine.Er` and inspect `K`, `U`, `St`, and `Rt` to present
specific URL, local-file, HTTP, network, timeout, cancellation, or decoding
failures without parsing exception text. A `timeout` is retryable, while a host
`St` cancellation remains non-retryable and is never relabeled as a timeout.

The `V` host control now performs cached layout and bounded scrolling. Firespider
can read `Dh`, `Sy`, and `Bc`, and can call `Sc(delta)` without depending on
WinForms mouse-wheel internals. `Rb(index)` and `Rc(index)` expose deterministic
outer-border and content rectangles for layout tests and host diagnostics.

Same-page navigation uses `Ft`, `Fg`, and `Rv`. `Ft` resolves a bounded decoded
ID or named anchor, `Fg` stores the active fragment in a 128-entry retained-page
history without another request, and `Rv` opens closed details ancestors in one
field-preserving rebuild. `V.Go`/`GoA`, rendered links, and Back/Forward scroll
through the same layout-box lookup. A missing target keeps the viewport, `#`
returns to the top, and `P.Bu` remains unchanged while `P.U` tracks the active
fragment. `V.Fa/Fj/Fy` report an initial anchor, its bounded placement count,
and latest target Y. Lea reapplies that one page/fragment pair after first
paint, host/font changes, rebuilds, and current-page intrinsic image decode;
`Sc`, wheel/focus movement, missing targets, navigation, and disposal release
it. Firespider receives the final `Changed` event and address normally and does
not need adapter-side layout code. See `INITIAL_FRAGMENT_STABILITY.md`.

Supported `<progress>` and `<meter>` elements carry a compact `B.Vm` model
instead of creating native controls. `V/Mn/Mx/R` provide normalized value,
range, and paint ratio; meters add `Lo/Hi/Op/Q`, while progress uses `In` for
the indeterminate state. Lea bounds numeric text, finite magnitude, layout,
table-cell placement, inherited color, opacity, clipping, and resize through
the same custom surface used by the rest of the document.

Supported `<output>` and `<data>` elements use the compact `Dv` model instead
of native controls. Hosts can enumerate `P.Dvs`; standalone and inline paint
records expose the same object through `B.Dv` and `Ir.Dv`. `K/V/X/N/F/Fo/El`
provide one-letter kind, bounded current and visible values, output metadata,
and DOM ownership. Safe `tx/text` and class commands rebuild these aliases while
preserving computed styles, form fields, and a data element's machine value.

Supported `<time>`, `<ins>`, and `<del>` elements extend that model without a
new host control. `K=t/i/r` identifies the semantic kind, `Dt` carries bounded
authored datetime text, and `U` carries only a safely resolved file, HTTP, or
HTTPS edit citation. A time element without `datetime` derives `V/Dt` from its
child text. Citations remain metadata rather than entering `P.Ls` or navigation.
The same `B.Dv`/`Ir.Dv` ownership covers inline, standalone, and table content,
including native decoration and text/class rebuilds.

Supported `<map>` and `<area>` elements use `P.Mps`, `B.Mp`, and compact `Ar`
records rather than transparent native controls. `Mp.N/As/El` expose map names,
areas, and ownership; `Ar.K/Cs/U/T/El` expose bounded shape, coordinates, link,
alt text, and ownership. Images retain an authored coordinate canvas through
`B.Mw/Mh`; `V` scales and clips rectangle, circle, polygon, default, and dead
regions after layout settles. First-area priority and dead masking are retained.
Area links join `P.Ls` and reuse normal click, fragment, navigation, command,
and download host routes. Pointer and keyboard activation use the same path.

Hosts can parse one bounded nullable tabindex through `E.Ti` or `V.Ti`.
`V.Ke/Kn/Ki/Kb` expose the focused DOM anchor, sequential count, one-based
index, and visible bounds. `V.Kf(back)` advances or reverses the first 256
targets with wrapping, and `V.Ka()` asynchronously activates the current link,
image-map area, field, or summary. Positive tabindex priorities precede normal
document order; negative values remain click focusable but are omitted from Tab
navigation. Shared maps retain one target per rendered image within a 4,096
map-region scan cap. The view reapplies
`:focus`, paints a default ring, reveals off-screen targets, and leaves
Ctrl+Tab/Alt+Tab available to Firespider and the operating system.

Hosts can enumerate authored autofocus candidates through `E.Afs(page)` or
`V.Afs()` and read the first through `Af`. The list is document ordered and
capped at 32. On loads without a resolved fragment target, the view focuses the first candidate
that resolves to one current rendered target; a focused editable Lea surface
also opens its existing native editor. Non-focusable, hidden, disabled, clipped,
and excess candidates remain inert, and navigation does not force the browser
window itself to the front.

`E.Aks(element)` parses up to 16 unique ASCII letter/digit alternatives from
256 authored characters; `Ak` returns the first. The WinForms assignment layer
resolves collisions in document order and retains at most 64 keys. `V.An`
reports the count, `V.Ak(element)` returns the assigned key, `V.Kl(element)`
returns the Windows `Alt+Shift+K` label, and `await V.Aa(key)` focuses and
activates an assignment. Firespider/WinForms command routing runs first, so
unmatched, host-owned, and differently modified shortcuts are not consumed.
The activation path is the same one used by `Ka`, including areas, fields,
summaries, safe commands, fragments, downloads, and host navigation.
HTML-hidden nodes and their descendants receive default rendered/link/field/hit
suppression and stay outside focus, autofocus, and access-key models.

Accessibility hosts can enumerate `P.Axs` or call `E.Axs(page)` and
`E.Ax(page, element)`. Each bounded `Ax` record keeps `K/N/D/V` role, name,
description, and value; `St` plus `Is(flag)` expose compact live state bits;
`Lv/U/El/Up` retain heading level, link URL, DOM owner, and semantic parent.
The list is capped at 512 records and accessible name/description references at
16 IDs. Password and file values are redacted. Labels, supported HTML and ARIA
roles, landmarks, text, tables, lists, forms, disclosures, and gauges rebuild
with the existing page rather than creating a second document tree.

WinForms hosts use `V.Axs`, `V.Ax(element)`, `V.Ab(record)`, and `V.Ao` for the
same model. `V` exposes MSAA children plus an HWND-rooted Windows UI Automation
fragment tree with stable bounded IDs. Both paths share rendered bounds,
hierarchy, live values/states, protected passwords, keyboard shortcuts, hit
testing, focus, and actions. UIA maps Lea nodes to native control types and
supports invoke, editable value, toggle, radio selection, disclosure,
read-only/editable range, and scroll-item patterns with structure, focus, and
property notifications. Read-only TextPattern partial and point rectangles map
through each retained segment's horizontal LTR/RTL direction while preserving
logical source order. Native `Tb/Tr/Tc` tables also expose concurrent grid
and table patterns; cells expose grid-item/table-item coordinates, bounded
column spans, containing-grid links, and row/column header relationships. The
loaded provider view is capped at 256 complete rows, 256 columns, and 512 cells;
ragged tables remain semantic tables but do not advertise a grid until empty
cell providers are modeled. Add the framework `Accessibility`,
`UIAutomationProvider`, `UIAutomationTypes`, and `WindowsBase` references beside
`Lea.Engine` and `Lea.Win` when compiling the adapter source; the project file
already includes them. The standalone test browser and Firespider Inspector
both include an Access tab for live inspection.

Parsed table blocks expose `B.Tb`, with `Tb.Rs`, `Tr.Cs`, and compact `Tc` cell
records. `V` paints these on the same lightweight surface with shared row
heights, CSS cell boxes, wrapped text, bounded column spans, and resize reflow.
`Tc.Bs` carries structured cell content; nested links join `P.Ls` and use the
same lightweight hit-testing and navigation path as links outside tables.

Normal text blocks keep their compatible flattened value in `B.X` and expose
styled nested content through `B.Is`. Each compact `Ir` run carries `K`, `X`,
an optional link `U`, and computed style `S`. `V` wraps mixed bold/italic text,
styled spans, and links together, with a hit region for each linked fragment.
Blocks without nested inline markup stay on the original one-box layout path.
`B.El`, `Ir.El`, `Dv.El`, `Fi.El`, and `Tc.El` retain the originating document element.
`E.Dy(page, hover, focus)` uses those references to reapply the already-compiled
`:hover` and `:focus` rules while retaining mutable form values. `V` drives the
state automatically for nested inline content, fields, and table cells.

The tolerant tree builder applies scoped implied closing for paragraphs,
headings, list and definition entries, table rows/cells, options, links, and
buttons. Nested lists keep their own scope, while mismatched end tags unwind to
the matching ancestor without discarding following text. Blocks include all six
heading levels, semantic containers, definition lists, quotes, captions,
details/summary, addresses, and horizontal rules.

`N.Op` retains each details element's page-local open state. The first direct
summary receives a compact `B.Mk` marker (`>` closed, `v` open), while `B.Hd`
keeps closed content in the document and form models but outside layout and
paint. `E.Ds` and `V.Ds` share one rebuild path that preserves mutable fields,
nested disclosure state, compiled `[open]` styling, and block ownership. The
WinForms view activates valid summaries by pointer, Enter, or Space, including
wrapped summaries and summaries inside structured table cells.

Computed styles use `S.Ta` for compact `l/c/r/j` alignment, `S.Lh` for pixel
line height, and `S.Lm` for a relative multiplier. `V` applies these to wrapped
blocks and table cells, centers glyphs within taller lines, and justifies only
wrapped non-final lines. Blocks without these properties retain the native
single-box text path.

Inherited `S.Tt` uses `n/u/l/c` for `text-transform:none`, uppercase,
lowercase, or capitalize. Hosts can call `E.Tx(text, style)` when they need the
same bounded presentation string. Lea transforms retained block/inline/table
text, semantic value labels, buttons, placeholders, option labels, and
unfocused field paint while preserving authored `P.X`, node text, `Fi.V`,
option values, and submitted form data. Focused native editors show the raw
value. Dynamic hover and viewport rebuilds retain field identity, and UI
Automation maps the transformed visual case back to authored logical text.

Inherited `S.Ls/Wg` exposes letter and word spacing as signed integer pixels
bounded to -32..128. Normal/initial resets and parent-restoring inherit/unset/
revert values share the compiled cascade. Retained text measurement, wrapping,
inline boundary gaps, button sizing, and glyph painting use the same metrics,
so hosts receive updated hit, find, focus, and automation rectangles without
adapter-side spacing code. Blocks, inline runs, table cells, and unfocused
controls support the feature; authored field values remain unchanged. Focused
native editors and select popups keep native Windows spacing. Zero-spacing
pages retain their existing fast path; complex-script/bidi tracking and
fractional or relative spacing units remain outside this implementation.

Non-inherited `S.To=c/e` exposes clip/ellipsis text overflow. A non-visible
overflow mode and a line wider than its content box are both required. Lea
keeps original layout and pointer targets, including collapsed link spaces,
and paints a shortened text-element prefix plus a synthetic marker. No host
string shortening is needed. Document and form values remain authored; UI
Automation retains full logical text and uses the painted prefix for visible
text rectangles. Horizontal RTL overflow places the marker at physical left,
right-aligns the authored prefix inside its retained atom, and maps partial and
point ranges from the right. Native controls and SVG text keep their existing
behavior. The current path does not implement custom markers, two-ended
overflow, line clamping, Unicode bidi shaping, or vertical layout.

`S.Ws` carries compact `n/nw/p/pw/pl` white-space modes. Lea preserves explicit
spaces and line breaks for `pre`/`pre-wrap`, keeps line breaks while collapsing
spaces for `pre-line`, and disables soft wrapping for `nowrap`/`pre`. `S.Ov`
uses `v/h/c/a/s`; non-visible modes clip glyph painting and link hit regions to
the content box. Auto and scroll currently clip safely without inner scrollbars.

Positive pixel bounds use `S.Wn/Wx/Hn/Hx` for minimum/maximum width and height.
Optional `S.Wp/Wo`, `Wnp/Wno`, and `Wxp/Wxo` expose precompiled percentage plus
pixel-offset width, minimum, and maximum tuples. Matching `S.Hp/Ho`, `Hnp/Hno`,
and `Hxp/Hxo` expose height tuples. The WinForms layout applies the same clamp
order to blocks, controls, images, flex/grid items, tables, and nested cell
blocks. `S.Wb` uses `n/b/a/k` for normal, break-word,
anywhere/break-all, and keep-all. These modes come from `word-break`,
`overflow-wrap`, or `word-wrap`; `white-space:nowrap` still disables wrapping.

`S.Bz` is the non-inherited box edge: `c` is the default `content-box`, while
`b` is `border-box`. Shared bounded helpers interpret fixed, percentage,
classic viewport-unit, one `calc(PU +/- Npx)`, two/three-term `min()`/`max()`,
or three-term `clamp()` width/height/min/max forms plus pixel flex-basis values
before layout. Border-box values contain visible
padding and border but not margins; content dimensions floor at one render
pixel. Blocks, fields, flex items, images, tables, and table cells use the same
geometry, clipping, paint, hit, accessibility-bound, and narrow-reflow paths.

Circular and elliptical corners use clockwise `S.R0/R1/R2/R3` horizontal and
`Q0/Q1/Q2/Q3` vertical aliases. Pixels remain nonnegative and percentages use
negative compact values. Atomic one-to-four-value `border-radius`, optional
slash-separated vertical radii, corner longhands, custom properties, important
tiers, and resets share one CSS normalization step when opposing corners exceed
the box. The WinForms view uses that path for backgrounds, solid/dashed/dotted
borders, descendant overflow clipping, and ordinary field interiors while
retaining the existing rectangular layout, link bounds, and responsive reflow.
Pixels cap at 2,000 and percentages at 1,000; malformed declarations preserve
the previous computed state. Percentage and viewport-unit terms are limited to
1..100 and pixel terms to 0..2000; arbitrary arithmetic and nested functions
remain unsupported. Height percentages require a definite viewport, retained
parent, positioned containing block, or grid area. `vw`, `vh`, `vmin`, and
`vmax` always use the live client viewport. See `CALCULATED_WIDTHS.md`,
`CALCULATED_HEIGHTS.md`, `VIEWPORT_UNITS.md`, `FONT_RELATIVE_UNITS.md`, and
`SIZE_FUNCTIONS.md`.

Logical `border-start-start-radius`, `border-start-end-radius`,
`border-end-end-radius`, and `border-end-start-radius` map directly onto those
same four physical ellipse pairs for the current horizontal direction.
Explicit shorthand/corner inheritance, reset keywords, source order,
priority, variables, and invalid preservation stay engine-owned. Compact
`rd/rtl/rtr/rbr/rbl/rss/rse/res/ree`, readable forms, and familiar literal DOM
properties require no adapter state or renderer branch. See `LOGICAL_RADII.md`.

Size functions add lazy `S.Wk/Wnk/Wxk` and `Hk/Hnk/Hxk` kind state plus
`Wq/Hq/Wv/Hv` argument inspection. The adapter does not evaluate the function;
`Lea.Win.V` resolves every term through the same containing-block, viewport,
element-font, and root-font path used by plain dimensions. A height function
containing a percentage remains natural below an auto-height parent.

For the current horizontal layout, `inline-size`,
`min-inline-size`, and `max-inline-size` map directly to the existing width
records; `block-size` and its min/max forms map to height. Logical inset
longhands and one/two-value `inset-inline`/`inset-block` write the existing
`S.Tp/Rt/Bm/Lt` edges atomically. Hosts need no new layout state. Page code can
use short `is/bs`, readable `inlinesize/blocksize`, or literal allow-listed
`style.inlineSize/style.blockSize` assignments. Horizontal `direction:ltr/rtl`
remaps inline start/end; vertical writing modes are not supplied. See
`LOGICAL_SIZING_INSETS.md` and `DIRECTION.md`.

Logical margins and padding follow the same host-free mapping. `margin-inline`,
`margin-block`, `padding-inline`, `padding-block`, and all eight start/end
longhands compile into existing `S.Mt/Mr/Mb/Ml` and `S.Pt/Pr/Pb/Pl` fields.
One/two-value axes, physical/logical source order, important tiers, custom
properties, inheritance, resets, and atomic validation are resolved before the
renderer sees them. Signed fixed margins cap at +/-2,000 pixels; padding remains
non-negative. Page code can use `mi/mb/pi/pb`, the eight three-letter edge
aliases, readable forms, or familiar literal DOM assignments. No adapter or
host change is required. See `LOGICAL_BOX_MODEL.md`.

Logical borders also require no host-side axis model. `border-inline`,
`border-block`, all four logical edge shorthands, and their width/style/color
components resolve into `S.Bt/Br/Bb/Bl`; `S.Bsv(edge)` and `S.Bcv(edge)` expose
effective per-edge style/color. A lazy detail array exists only for mixed edges,
so uniform borders retain the original compact record. Page code can use all 24
two/three-letter aliases, readable forms, or familiar literal DOM assignments.
The WinForms adapter handles per-edge used widths and mixed rounded paint; the
Firespider adapter needs no changes. Keep `Lea.Engine.dll` and `Lea.Win.dll` from
the same runtime because older views only inspect global `S.Bs/Bc`. See
`LOGICAL_BORDERS.md`.

`S.Dr` exposes inherited horizontal direction as `l` or `r`. Lea resolves HTML
`dir` and CSS `direction` before ordinary declarations, then writes logical
insets, boxes, borders, radii, and start/end text alignment into the existing
physical records. Cross-direction explicit inheritance is translated by the
engine. The WinForms renderer also maps row/reverse/wrapped flex slots, column
cross alignment, and logical grid auto/dense/column/explicit/area tracks plus
content/self alignment through that same bit. Plain, mixed, wrapped, nowrap,
justified, and structured-cell inline atoms keep source order while RTL
coordinates advance from the right; existing `B.Mk` list/disclosure markers use
the same path. Horizontal inline-end ellipsis and read-only TextPattern range/
point geometry also follow that direction without changing public host records.
Hosts may use compact `dr`, readable `direction`, or literal bounded DOM forms
and need no extra axis state. Unicode bidi glyph reordering, embedding controls,
`dir=auto`, vertical writing modes, and `wrap-reverse` remain outside this
milestone. See `DIRECTION.md`.

CSS outlines are available through non-inherited `S.Ow/Os/Oc/Oo`. Hosts can
inspect bounded width, compact none/solid/dashed/dotted/auto style, authored
color, and signed offset without performing paint calculations. Atomic
shorthand/longhands, variables, important tiers, resets, responsive rules, and
dynamic `:focus` all rebuild through the existing style path. `V` paints
rounded box, image, and field outlines with opacity and ancestor clipping while
leaving `V.Rb`/`V.Rc`, hit routes, accessibility bounds, native editor
placement, document height, and following flow unchanged.

Inherited pointer interaction is available through `S.Cu/Pe`. Hosts can inspect
the compact automatic/default, pointer, text, wait, crosshair, move, help,
not-allowed, and axis-resize cursor state without translating CSS keywords.
`Pe=false` removes that element from Lea's retained mouse hover and click path;
eligible content underneath can receive the pointer instead. Links, summaries,
ordinary fields, fitted image maps, and linked SVG shapes share the gate.
Variables, important declarations, resets, invalid preservation, dynamic hover,
and responsive rebuilds use the existing computed-style path. Layout, paint,
keyboard focus, access keys, and semantic accessibility remain unchanged, so a
host does not need a second input or cursor implementation.

CSS box shadows are available through the non-inherited `S.Sw` list. Each
compact `Sw` exposes `X/Y` offsets, `B` blur, `R` spread, `C` color, and `I`
inset state. Lea accepts up to four entries atomically, caps signed offsets and
spread at 512 pixels, blur at 128 pixels, and soft rendering at 12 bands.
Rounded outer/inset shadows, alpha and `currentColor`, ordinary fields,
variables, important tiers, resets, and ancestor overflow use the retained box
painter. Shadows are visual only, so host layout, `V.Rb`/`V.Rc`, accessibility
bounds, document height, and link/control hits remain stable.

CSS bitmap backgrounds are available through non-inherited `S.Bi`. Hosts can
inspect `Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` for one resolved image, repeat
mode, position, size, and cover/contain state. The parser applies `background`,
`background-image`, `background-repeat`, `background-position`, and
`background-size` atomically; linked styles and direct custom-property values
retain their own URL origin. The custom view loads bounded data/file/HTTP(S)
resources through its existing 24-entry cache and paints them on pages, rounded
boxes, and ordinary fields with effective opacity and inherited clipping.
Layout, accessibility bounds, document height, and host hit routes remain
unchanged.

Raster `<img>` fitting is available through non-inherited `S.Of` and
`S.Ox/Oy/Oxp/Oyp`. `Of=f/c/v/n/s` represents fill, contain, cover, none, and
scale-down; `Ox/Oy` plus the percentage flags represent the centered initial
`object-position`. The engine accepts bounded keyword, pixel, unitless compact
pixel, and percentage positions, custom properties, important declarations,
resets, and responsive rebuilds. The WinForms view computes one capped fitted
rectangle inside the existing image content box and clips overflowing cover or
none paint. Image maps use that same rectangle for pointer hits, sequential
focus bounds, and semantic accessibility, excluding letterboxes and preserving
cropped source-coordinate transforms. Hosts need no additional control or
adapter code, and image layout dimensions remain unchanged.

CSS `aspect-ratio` is available through non-inherited `S.Rw/Rh/Ra`. `Rw/Rh`
hold the positive preferred ratio components and `Ra` records optional
intrinsic `auto` preference. The bounded parser accepts one number or `A/B`,
variables, important declarations, resets, and responsive rebuilds atomically.
Hosts do not perform ratio math: `V` applies one capped resolver to content- and
border-box blocks, raster/SVG intrinsic fallback, fields, flex items, grid
items, and table minimums. Explicit dimensions and min/max constraints remain
authoritative, and normal text can grow past its preferred ratio rather than
overlap following content.

CSS background gradients are available through non-inherited `S.Gi`. Hosts can
inspect `Gi.K/A/X/Y/Ss` for one linear/radial image and `Cs.C/P` for up to eight
normalized color stops. The parser applies `background` and `background-image`
atomically, so invalid or over-limit input does not partially replace an
existing image. The custom view paints gradients on page backgrounds, rounded
boxes, and ordinary fields with effective opacity and inherited clipping;
layout rectangles, accessibility bounds, document height, and hit routes stay
unchanged. Repeating or layered gradients, pixel stops, and radial size keywords
remain explicit compatibility limits.

Lea's bounded flex subset uses `S.D=flex/inline-flex` and compact
`S.Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or` values. The model preserves direct element items
under their visual container, while `V` allocates fixed, equal-share auto,
integer weighted-growth, or integer weighted-shrink slots and reuses the
existing child layout, clipping, stacking, hit, paint, and accessibility paths.
Explicit pixel `Wn/Wx/Hn/Hx` floors and clamps participate in allocation; locked
or exhausted minimums retain visible overflow. Row/row-reverse, fixed-height
column/column-reverse, stable bounded order, row wrapping, pixel basis,
one/two-value or separate row/column gaps, start/center/end/between/around/evenly
main-axis distribution, and container or per-item cross-axis alignment are active.
Auto-height columns retain source-order natural
stacking and row gaps. Intrinsic min-content sizing, fractional factors, wrapped
ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment,
inline-level outer formatting, and auto-height column reordering remain explicit
compatibility limits.

Lea's bounded CSS Grid subset uses `S.D=grid/inline-grid`, lazy compact `Gd/Rd`
column and row track arrays, public `Gc/Gv(index)` and `Rc/Rv(index)` inspection,
and direct-item `Gl/Gs` plus `Rl/Rs` start/span aliases. Hosts can supply one to
eight fixed `0..2000px` or integer `1..64fr` tracks per explicit axis, simple
bounded `repeat()`, `minmax(0,Nfr)`, pixel row/column gaps, numeric starts,
`span N`, and stable `Or` ordering. `V` uses bounded row-major occupancy,
allocates fractional remainder deterministically, shrinks oversized fixed
tracks to definite space, and settles auto-height row containers through its
existing measurement pass. Block ownership, nested flow, positioning, clipping,
painting, hit testing, form retention, and accessibility bounds remain shared.
Bounded `grid-template-areas` adds `S.Aa/Ar/Ac/Ag` container inspection and
`S.An` item names. Up to eight equal-width quoted rows, eight cells per row,
16 exact ASCII names, dot cells, and filled rectangular regions compile once.
Named and fully numeric definite items reserve occupancy before automatic items;
area dimensions can create equal-share implicit tracks when no explicit column
size exists. Bracketed track line groups compile into lazy, unique,
case-sensitive maps with `S.Gnc/Rnc` counts and one-based `Gx/Rx(name)` lookup.
Direct items retain `Gn/Ge` column and `Rn/Re` row names; named shorthands,
longhands, numeric/name combinations, spans, and area-generated
`name-start/name-end` edges resolve before automatic placement. Each axis keeps
at most 16 explicit names, eight names per line, 32 characters per name, and
the existing eight-track/512-character bounds. Invalid declarations are atomic.
`grid-auto-flow` adds compact `S.Gf=r/d/c/x` state for sparse/dense row/column
placement. Dense modes backfill earlier holes; column modes wrap through the
bounded explicit/template row count and create equal-share implicit columns.
Resolved non-default flow order feeds the existing real-height measurement pass,
while DOM ownership and semantics stay unchanged. Eight columns are the hard
ceiling; excess content continues in additional final-column rows without
overlap. Repeating `grid-auto-columns/rows` patterns expose compact
`S.Gac/Gav(index)` and `Rac/Rav(index)` inspection. Each pattern accepts one to
eight pixel, integer-fr, bounded `auto` (`S.Av`), or `minmax(0,Nfr)` entries and
cycles after the explicit sized tracks. Definite numeric endpoints and largest
spans can grow implicit columns; area-created unsized tracks use the same
pattern. Pixel implicit rows contribute their exact minimum to the auto-height
measurement pass. Reset values clear the lazy pattern, and an invalid
declaration preserves its prior value. Escaped or repeated-name occurrence
indexes and named spans remain outside this subset. `grid-template` compiles
explicit `rows / columns` and optionally sized quoted area rows while preserving
automatic state. `grid` accepts those forms plus row/column `auto-flow` and
`dense` forms, resets all owned explicit and implicit properties to their
defaults, and leaves `Gp/Rg` gaps unchanged. Explicit `auto` tracks use `S.Av`.
Both shorthands are capped at 1,024 characters and one top-level slash; scratch
state makes every invalid declaration atomic. Grid item alignment uses compact
container `Ji/Ai` defaults and item `Js/As` overrides with `t/s/c/e` for
stretch/start/center/end. Longhands and atomic `place-items`/`place-self` align
margin boxes within resolved areas; simple auto-width text is measured and
definite dimensions remain exact. Block-axis center/end/stretch requires a
resolved row rectangle. Reverse flow, pre-explicit implicit tracks, line names
around quoted shorthand area rows, intrinsic or percentage tracks, auto-fit/fill,
and subgrid remain explicit compatibility limits. Track groups use compact
`Al/Jc=s/c/e/b/a/v` block/inline-axis state for start, center, end, between,
around, and evenly distribution. Atomic one/two-value `place-content` writes both
axes after complete validation. Fixed tracks in a definite grid expose free
space; flexible/auto tracks consume it during sizing first. Baseline, safe/unsafe
alignment, writing-mode axis reversal, and true inline-level outer formatting
remain outside the bounded model.

Inherited visibility uses `S.Vi` with `v/h/c`. Hidden and collapsed content
keeps its layout rectangle but produces no pixels, links, field hits, or editor.
`S.Op` is a bounded effective percentage: ancestor and local opacity combine,
decimal and percentage syntax clamp safely, and cascade overrides remain
deterministic. Opaque content stays on the original fast paint path; faded text,
backgrounds, borders, controls, and images use lightweight alpha rendering.

Stylesheets accept descendant, direct-child (`>`), adjacent-element (`+`), and
general-sibling (`~`) selectors. Compact chains such as `main>p+p~p` work
without required spaces, and sibling matching ignores formatting whitespace
between HTML elements. Lea tokenizes these chains once per CSS rule and reuses
them during tree matching.

Simple selectors are also compiled once per rule. They support tag, universal,
ID, class, attribute presence and the six common attribute-value operators,
plus basic structural, link, and initial form-state pseudo-classes. Structural
positions count element siblings and compile `odd`, `even`, signed integers,
and `An+B` arithmetic for forward/reverse child and type matching into two
bounded integers. Expressions stop at 32 characters and absolute `A/B` values
at 8,192. `:nth-child` and `:nth-last-child` accept strict `of S` filters with
up to eight compound alternatives, count only matching siblings, and add the
maximum filter specificity. Filter alternatives share the sixteen-test ceiling;
inner combinators, nested lists, and filtered type positions remain outside.
Bounded `:is(...)`,
`:where(...)`, and `:not(...)` compile up to eight
compound alternatives each and sixteen authored functional tests per simple
selector. `:is` and list `:not` use maximum argument specificity; `:where` has
zero specificity. Lea discards unsupported alternatives from forgiving
`:is`/`:where` lists, rejects an invalid `:not` list, and deliberately leaves
nested selector-list functions and inner combinators outside this subset. Dynamic
`:hover` and `:focus` use the same compiled path and compact `El` references in
the WinForms hit-test model. The `Id/Q/Qa` host queries reuse identical rules.

Strict bounded `:has(...)` relative lists also compile once. Lea supports an
implicit descendant relationship and explicit leading `>`, `+`, or `~`, with
up to four compound parts in each relative chain, eight alternatives, sixteen
total parts per outer simple selector, and 4,096 candidate visits per
evaluation. The pseudo-class contributes its most specific argument, matching
Selectors Level 4 cascade behavior. Parent styles and host queries can react to
descendant focus without replacing fields. Empty or malformed lists, nested
selector-list functions, pseudo-elements, oversized chains, and excess work
are rejected rather than entering an unbounded tree search.

Responsive styles use `E.Vw`, `P.Vw`, `V.Vw`, `V.Vh`, and `G.Vw`. A headless
host sets `E.Vw` directly; the WinForms view forwards its client width and rebuilds only
when a media boundary changes. Existing `Fi` objects and mutable values survive
that rebuild. The bounded compiler accepts `screen`/`all` with pixel
`min-width`, `max-width`, and exact `width`, comma alternatives, nested
`@media`, and `<style media>`. Widths clamp to 1-8,192 pixels; query alternatives,
nesting, and total rules are capped at 8, 4, and 1,024. Unsupported media stays
inactive instead of silently behaving as screen CSS.

Responsive dimension tuples expose `Wu/Wnu/Wxu` and `Hu/Hnu/Hxu` unit codes:
`0` is containing percentage, `1` is `vw`, `2` is `vh`, `3` is `vmin`, and `4`
is `vmax`; `5` is `em` and `6` is `rem`. The WinForms renderer resolves
viewport units from live client dimensions, `em` from the target's computed
font, and `rem` from `P.Rf` or the host fallback exposed by `V.Rf`. The same
values flow through layout, paint, hits, fields, and accessibility while tuple
storage remains lazy. See `VIEWPORT_UNITS.md` and `FONT_RELATIVE_UNITS.md`.

CSS declarations are compiled once per stylesheet rule. Bounded custom
properties inherit through the compact style tree and use lazy copy-on-write
storage, so pages without them do not allocate a variable map per style. Hosts
inspect `S.Vc`, `S.Cv(name)`, and aggregate `G.Vc`; names remain case-sensitive.
Lea resolves nested `var(--name, fallback)` after the winning custom-property
cascade, including inline, hover/focus, and media changes. Limits are 128
declarations per block, 32 properties per style, 64-character names,
512-character raw values, eight resolution levels, and 1,024 expanded
characters. Missing, cyclic, oversized, and excess values fail safely into an
authored fallback or leave the ordinary declaration unchanged.

Compiled declaration records also retain a single important-priority bit. Lea
recognizes only a final top-level literal `!important` marker, ignoring
case while preserving identical text inside quotes and functions. Supported
ordinary and custom properties run through normal stylesheet, normal inline,
important stylesheet, and important inline tiers; specificity and source order
continue to settle declarations within each stylesheet tier. This applies to
hover/focus and active media rebuilds without replacing retained fields. Hosts
can inspect the bounded active stylesheet count through `G.Ic` and the
Inspector; inline declarations remain element-local and are not added to that
aggregate.

`S.Ff` carries up to eight CSS font-family fallbacks; `V` resolves and caches
the first installed Windows family with a 32-entry bound. `S.Td` uses compact
`n/u/s/us` decoration modes. Underline, line-through, native decoration tags,
and explicit removal of a link underline all use the same `BoxFont` path.

Inherited block indentation is available through `S.Ti/Tip`. `Ti` holds a
signed -2000..2000 pixel or -100..100 percentage integer and `Tip` identifies
the percentage form. The WinForms view resolves percentages against settled
content width and adjusts only the first visual line from LTR or RTL
inline-start; later lines retain full width. Paint, links, hits, find, focus,
and accessibility reuse the settled rectangles. Hosts can call bounded
`ti(value)` or `ti(id,value)`, use readable `textindent`, or send literal
`style.textIndent` assignments without implementing layout themselves. See
`TEXT_INDENT.md`.

Inherited text shadows are available through nullable `S.Ts`, reusing the
compact `Sw.X/Y/B/C` offset, blur, and color fields for one layer. Offsets are
bounded to -128..128 pixels and blur to 0..32 pixels. The WinForms view paints
the shadow without changing layout, hit, find, focus, or accessibility bounds;
pages without shadows allocate no record. Hosts can call `ts(value)` or
`ts(id,value)`, use readable `textshadow`, or send literal `style.textShadow`
assignments. See `TEXT_SHADOW.md`.

`E` and `V` expose the same cookie aliases: `Kc`, `Km`, `Kcl()`, and `Pv`.
Cookies remain in memory and are bounded to 64 entries. Firespider maps its
existing `pv` flag to each active Lea view, so private mode receives a fresh
isolated jar and crossing the privacy boundary clears Lea's page cache.

The first valid bounded `<base href>` sets `P.Bu` and `V.Bu`; `P.U` and `V.U`
remain the actual response URL. Relative stylesheets, links, images, form
actions, and download links resolve through `Bu`. Lea accepts HTTP, HTTPS, and
file bases up to 4,096 characters, skips unsupported or malformed values, and
falls back to `U` when no valid base is present.

External `<link rel="stylesheet">` resources use the document base URL for
relative resolution and the same request layer as navigation. They therefore share
cookies, compression, conditional validation, timeout/cancellation, `Cm` LRU
capacity, and `Cl()` clearing. A page may load eight links of up to 256 KB each;
missing, oversized, unsupported-media, and non-CSS links are skipped safely.

Images accept `src`, compact `srcset` width/density candidates, and direct
screen-compatible `<picture><source>` fallbacks. Base64 PNG, JPEG, GIF, BMP,
and icon data URLs decode under a 4 MB limit into the existing 24-entry image
cache. Accepted codecs run in the shared killable `Lea.Image.exe` process and
return bounded raw pixels; `Ie/Ii/Iw/Ik` expose helper path, availability, last
PID, and cancellation kills. `Ij/Ih/Ipm` expose mandatory Job assignment, its
64-512 MiB hard process-memory cap, and last sampled private bytes.
`Irt/Ili/Isp/Ihl` confirm the actual child token has stripped privileges and
low integrity, assignment
while the primary thread is suspended, and a three-pipe inherited-handle
allowlist. `Ias` confirms forced-relocation and bottom-up ASLR at creation;
`Icp` confirms the Job CPU hard cap, while `Icr` configures its 100-10,000
basis-point rate and restarts the shared worker when changed. `Img` is the
helper mitigation handshake mask and `Ipk` is the Job's peak process-memory
value. `Iac/Ind/Ial` confirm a deterministic AppContainer token, zero
capabilities including no network capability, and an identity-scoped
read/execute ACE on only the helper executable. Lea creates or reuses the
per-user profile on first native decode. If profile creation, ACL installation,
token inspection, or any existing Job/mitigation gate fails, the image fails
closed without an unrestricted fallback. `Icl` configures a 128 MiB-1 GiB
combined host/helper ceiling (384 MiB default). `Ihm/Icm/Iad` report the host
private sample, conservative host plus worker private/Job-peak plus preflight
reservation signal, and admission result. Missing/overflowing samples or a
signal above the ceiling terminate the helper before `In` increments or encoded
bytes are written. The helper must acknowledge required guards before the host
sends encoded bytes. CSS
width/height wins over HTML dimensions; otherwise `width` and
`height` attributes feed the same constrained layout path.

Blocks and tables support lightweight `position:relative`, `position:absolute`,
and `position:fixed` through `S.Pn/Tp/Rt/Bm/Lt/Zi`. `B.Up` links each retained
visual, positioned, or clipping block to its nearest compact parent context.
Semantic containers with meaningful backgrounds, borders, spacing, or dimensions
retain their own layout box; unstyled wrappers remain flattened. Auto-height
visual containers finalize from the bottom up around nested normal-flow children,
including mixed direct text and several levels of nesting. Relative
space remains in normal flow; absolute and fixed boxes are out of flow. Absolute
boxes use the nearest positioned containing block, while fixed boxes use viewport
coordinates even when nested. Overflow clips inherit through nested descendants.
Paint, hover, and clicks share a stable hierarchical z-index path, keeping child
layers inside their parent layer. `S.Za` distinguishes numeric z-index from
`auto`, while `S.Zc` records local opacity and `isolation:isolate` contexts.
Fixed boxes also establish a context. Auto-height owners use one bounded measure
pass followed by final placement, allowing bottom-anchored descendants and
overflow hit clips to resolve against settled parent geometry. Transforms,
filters, containment, and the full CSS stacking model remain future work.

Fields are exposed through `P.Fs`. The view paints controls on the Lea surface,
reuses one editor for focused text, opens native choice/file/color dialogs, and
paints range controls as click-to-set sliders. `Rs`, `Sm`, and `SmA` cover reset,
GET, URL-encoded POST, and multipart POST submission. Direct text POST uses
`Po`/`PoA`; bounded binary POST uses `Pb`/`PbA`. `P.Q` preserves query text on
both web and local pages.

`Fi.Rq/Mn/Mx/Pa/Lo/Hi/Sp` expose parsed constraints; `Ok/Vm` report validity and
`Nv` records validation bypass. `Fi.Fp/Ac/Mu/En` hold selected paths, accept,
multiple, and compact encoding state. Hosts may call `V.Sf` and `V.Vf`, while
`Sm/SmA` validate automatically and keep invalid pages in place without raising
`Sb`. Files, steps, colors, multiple email addresses, time, month, week, and
local datetime join the existing checks and CSS validity state. Uploads are
bounded to eight files, 4 MB each and 8 MB combined.

The first script boundary is an allow-listed command layer, not a general
JavaScript runtime. `E.Je`/`V.Je` enable it and `E.Js(page, code)`/`V.Js(code)`
provide the compact host bridge. Pages expose accepted-command count through
`P.Jc` and bounded alert messages through `P.Ms`. The WinForms view raises `Al`
for each new message and exposes it through `Am`; it never lets page code open a
native dialog directly. Inline scripts may set the title, body background,
foreground, font size, direction, and text indentation using
`t/a/bg/fg/fs/dr/ti` or equivalent literal
`document.*` assignments and `alert(...)`. Two-argument `bg/fg/fs` forms safely
scope those three styles to one bounded element ID, accept only existing
hex/named-color or 8..96px values, reject metadata targets, and cap each inline
style at 4,096 characters. `v/var` stores up to 16 small page
variables and `${name}` substitutes them into later literals. `tx/text` replaces
text under one safe element ID. `ac/rc/tc` add, remove, or toggle one bounded
class token on a safe ID and reuse the compiled CSS rebuild while preserving
mutable fields. Each target accepts at most 32 classes and 512 class-attribute
characters. `dp` writes or clears one allow-listed inline display value, keeping
other inline declarations and retained state intact. `wd` writes or clears one
bounded fixed, percentage, or calculated inline width through the same retained
rebuild. `hd/sh` provide
command-authoritative hide/show state, while
`op/cl/tg` open, close, or toggle a bounded `details` element through its own ID
or first direct `summary`, preserving nested field identity. `ds/en` update
supported field availability. `vl` sets one current field value
under a 1,024-character limit, rejects file/button injection, validates color
and select values, and keeps passwords redacted. `ck` toggles or explicitly
sets checkbox/radio state and preserves radio-group exclusivity. Existing field
objects remain retained by DOM owner while hidden.

`fo` queues one enabled, non-HTML-hidden focus target; the first accepted
request wins. Headless hosts take it once with `E.Kp(page)`. `V` resolves it
against final rendered focus targets, scrolls it into view, and clears stale
focus after hide/disable commands. `E.Ck(page, element)` and `V.Ck(element)` route
button and link `data-lea`/`onclick` values through this same parser. Lea ignores external scripts, JSON
data blocks, expressions, control flow, and unknown calls. Per-page limits are
eight script blocks, 16 KB per block, 32 accepted commands per run, 16 variables,
and 16 alerts.

For easier migration, literal `document.getElementById("id")` statements map
`textContent/innerText`, `value`, checked/disabled/hidden/open booleans,
`focus()`, `classList.add/remove/toggle`, and background/color/font-size/display/width/direction/text-indent styles
onto the existing compact commands. The adapter receives no DOM object and adds
no evaluator. IDs and class names remain bounded literals, every mutation uses
the normal validators and retained rebuild, and unsupported `innerHTML`,
selectors, computed values, event APIs, or control flow remain inert. See
`DOM_BRIDGE.md`, `DISPLAY_NAVIGATION.md`, and `CALCULATED_WIDTHS.md`.

Core downloads use `E.Dl`/`DlA`; `V` forwards those aliases and raises `Dr` for
links carrying the HTML `download` attribute. The host receives `Du` and `Dn`,
shows the destination UI, and passes the selected full path back to Lea. Streams
are capped at 64 MB, decoded for gzip/deflate transport encoding, and committed
through a temporary file. `E.Fd` provides bounded text counts, while
`V.Fd`/`Nx`/`Pr`/`Fc` manage highlighted painted matches and scrolling.

Hosts can inspect the compact `N` tree without maintaining a second selector
implementation. `E.Id(page, id)`, `E.Q(page, selector)`, and `E.Qa(page,
selector)` provide ID, first-match, and bounded all-match lookup. `Qa` returns a
read-only list capped at 256 nodes. `V` forwards these as `Id`, `Q1`, and `Qa`;
`V.Q` remains the current URL query string for compatibility.

Developer hosts call `E.Dg(page)` or `V.Dg()` for a compact `G` snapshot rather
than reflecting into the renderer. Counts cover the response, document, CSS,
commands, cache, and cookies. Bounded lists expose redacted node summaries,
compiled CSS selectors/declarations, request activity, alert messages, and page
variables. Mutable field values, passwords, and selected file paths are omitted.
`Dcl()` clears the latest request activity only. The standalone test browser
uses this contract for its right-side inspector and keeps page rendering on the
existing single `V` surface.

## Bounded inline SVG

An inline `<svg>` becomes one normal `B` with `K=svg` and `B.Vg`. This keeps
vector content inside the same layout, Flex/Grid ownership, scrolling,
overflow clip, stacking, hit, and accessibility paths as text and images. The
WinForms adapter uses `System.Drawing` directly; embedding Lea does not add an
external browser or vector runtime.

The supported slice covers authored/intrinsic pixel width and height, aligned
`none`, `meet`, and `slice` `viewBox` mapping, `g` inheritance, SVG `a` links, rounded `rect`,
`circle`, `ellipse`, `line`, `polyline`, `polygon`, text with start/middle/end
anchors, straight `path` commands `M/m`, `L/l`, `H/h`, `V/v`, and `Z/z`, plus
cubic `C/c` and `S/s`, quadratic `Q/q` and `T/t`, and elliptical `A/a` curves.
Presentation and inline transform lists support matrix, translate, scale,
centered rotate, skewX, and skewY with cumulative group inheritance.
Presentation attributes, specificity-ordered stylesheets, inline styles,
custom-property values, and important priority cover fill, stroke, stroke width,
opacity, fill/stroke opacity, font size, text anchor, and `currentColor`.
Local linear and radial gradients resolve once through `Vg.Ds` and `Vs.Fd/Sd`.
They support object-bounds/user-space coordinates, percentage values, radial
focus, up to four `href` inheritance hops, gradient transforms, and up to 16
presentation, stylesheet, or inline color/opacity stops. Gradient fills and
strokes follow the shape and viewport matrices through the same clipped path.

Local `<use>` references to a shape, group, symbol, link, or another use expand
into the ordinary `Vs` list with inherited paint, `x/y`, and transforms. Lea
indexes up to 64 unique local IDs, follows four use links, rejects cycles, and
still enforces the 128 painted-shape ceiling, so embedding hosts do not need a
parallel instance-tree API.

Clip paths resolve through `Vg.Cs` and each shape's `Cp/Cl/Cm` state. Up to 16
definitions can hold 32 primitive/path/text or reused geometry shapes each.
User-space clips keep the matrix where a direct or group clip became active;
object-bounds clips normalize through the target's logical bounds. Presentation,
stylesheet, and inline local references intersect fills, strokes, and linked hit
bounds. Missing, empty, cyclic, or over-limit definitions paint nothing.

Painted and clipping paths expose nonzero/evenodd winding through `Vs.Fr`.
Stroke state uses `Lc=b/r/s` for butt/round/square caps, `Lj=m/r/b` for
miter/round/bevel joins, `Ml` for a 1-64 miter limit, `Da` for a normalized
even dash list, and `Do` for its offset. Up to 16 nonnegative pixel/unit dash
values fit in 256 authored characters; odd lists repeat, `none` clears an
inherited list, and invalid or all-zero lists preserve inherited state. The
WinForms adapter maps the state to one native pen and uses bounded miter/cap
extents for linked stroke hits.

Parsing is capped at eight group levels, 128 shapes, 128 primitive/path points,
256 text characters, 4,096 path characters, finite coordinates, and a 2,000 px
intrinsic size. Transform lists are capped at 16 functions and 1,024 characters;
arcs expand to no more than four cubic steps. Gradient definitions, stops, and
dash values are capped at 16 each. Repeat/reflect spread, patterns, nested clip paths, masks,
filters, markers, animation, scripting, external references, symbol viewBox
sizing, nested SVG, and Canvas remain explicit compatibility limits.

## Experimental Firespider tab

The source in `../Lea.Firespider` now powers real engine-backed tabs in the
Firespider project. Its normal **New Lea Tab** command and `Ctrl+T` create a
`Lea.Win.V` inside Firespider's existing tab control. **New Legacy Tab**
(`Ctrl+Alt+L`) remains an explicit `q3` fallback. Back, forward, home, go,
search, reload, stop, and address/search Enter keys dispatch by active tab type
and delegate to the original handlers for legacy tabs. Feature-specific
`q3(...)` callers remain legacy until their immediate `WebBrowser` assumptions
are migrated.

Close Tab (`Ctrl+W`), the tab-strip close target, and middle-click now dispatch
through Lea-aware close handling. Page Source (`Ctrl+U`) fills Firespider's
existing source window from `V.H`. Open File loads through Lea, Save Page writes
`V.H` as UTF-8, and Send Link uses `V.T` and `V.U`. Lea sessions use a separate
bounded file at `%LocalAppData%\Firespider Internet\Lea.session`; private mode
removes it, and legacy tabs continue using Firespider's settings collections.

Firespider subscribes to `V.Gr` for each Lea tab and sends `V.Gu` through its
existing asynchronous Lea load/status route. The page command cannot invoke a
legacy control, native process, dialog, or direct file API.
Literal `location`, `location.href`, and `location.assign(...)` spellings map to
that same bounded `go` request. Relative file targets retain query and fragment
components; computed values, replacement/history APIs, and unsafe schemes do not
produce a request.

**View > Lea Inspector** (`F12`) and each Lea tab's context menu open a compact
owned diagnostics window. Summary, Nodes, CSS, Requests, and Messages are built
only from the bounded public `V.Dg()` snapshot; Access reads `V.Axs`. `R`/`F5`
refresh, `C` clears the request activity through `V.Dcl()`, and `X`/Escape
closes the window. Selecting a legacy tab produces a neutral selection message
and never casts or reflects into the legacy browser control.

Firespider imports one `Lea/FirespiderLea.targets` file. The import supplies the
two DLL references, partial VB adapter, and bundled local start page. Debug and
Release builds pass `Smoke.cs`, which verifies Lea-first New Tab creation,
the explicit legacy fallback, keyboard
navigation, host-controlled `go` requests, Inspector open/refresh/clear and legacy isolation, bounded diagnostics,
script/style and safe click execution, alert status routing, local open,
page save, find cycling, streamed downloads, send-link metadata, timer status,
close and source routing, and
session save/restore across two host forms without loading another browser
engine or touching the user's data. The adapter removes
unconditional `WebBrowser` casts from the paths it owns and delegates by active
tab type.

## Integration rules

- Reference the core engine, not the test browser shell, when embedding Lea.
- Keep browser window code in the user's project or in the optional Lea.Win layer.
- Do not require a hidden WebView or a downloaded browser runtime.
- Keep the core API stable and versioned before Lea 1.0.
- Include examples for both a browser window and a headless parser test.

The project split will happen after the current Forge renderer is separated into
modules. Until then, `LeaForge` is the active experimental host and `ShortApi.cs`
is the compact API prototype.
