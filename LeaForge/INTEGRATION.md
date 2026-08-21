# Using Lea In Visual Studio

Lea is intended to be a custom engine that a browser project can own and embed.
The target integration does not use Chrome, Chromium, Safari, Edge WebView, or
any other browser engine.

## Firespider host

Firespider Internet is currently a VB.NET WinForms application targeting .NET
Framework 4.6.1. Its browser surface is `System.Windows.Forms.WebBrowser`, and
the main navigation, tab creation, and document events are concentrated in
`1.vb`. Lea should replace that control behind a small adapter boundary instead
of rewriting the browser menus and settings all at once.

The first compatibility library is now in `../Lea.Engine`. It targets .NET
Framework 4.6.1 and exposes `E`, `P`, and `Ln` with the short Lea API. It is a
foundation for integration. `../Lea.Win` now provides the first custom `V`
WinForms view, and `../Lea.Win.Test` is its isolated test browser.

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

## Planned public shape

```csharp
var e = LeaForge.L.E();
var p = e.Go("https://example.com");
```

The final integration API will keep this compact surface while the internal
engine is split into clear modules. A UI adapter will let a user's own WinForms
or WPF window host Lea's layout and paint output.

Browser UI code should use `GoA` and `RlA` so network waits do not freeze the
window. `St` cancels the active request. The synchronous `Go` and `Rl` aliases
remain available for small tools and deterministic local tests.
`E.Tm` and `V.Tm` configure the request deadline in milliseconds; the compact
property defaults to 15,000 and clamps to 100 through 60,000. It applies to
synchronous and asynchronous HTTP page, POST, stylesheet, and download work.

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
`E.Cm`, `Cc`, `Ch`, and `Cl()` configure or inspect Lea's bounded decoded-page
payload cache; `Cs` sets default freshness seconds, while HTTP freshness headers
can override it. Stale validator-bearing entries send `If-None-Match` or
`If-Modified-Since`; `Rl` and `RlA` deliberately fetch a fresh response without
those conditional headers.

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
fragment. Firespider receives the final `Changed` event and address normally.

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

`S.Ws` carries compact `n/nw/p/pw/pl` white-space modes. Lea preserves explicit
spaces and line breaks for `pre`/`pre-wrap`, keeps line breaks while collapsing
spaces for `pre-line`, and disables soft wrapping for `nowrap`/`pre`. `S.Ov`
uses `v/h/c/a/s`; non-visible modes clip glyph painting and link hit regions to
the content box. Auto and scroll currently clip safely without inner scrollbars.

Positive pixel bounds use `S.Wn/Wx/Hn/Hx` for minimum/maximum width and height.
The WinForms layout applies the same clamp order to blocks, controls, tables,
and nested cell blocks. `S.Wb` uses `n/b/a/k` for normal, break-word,
anywhere/break-all, and keep-all. These modes come from `word-break`,
`overflow-wrap`, or `word-wrap`; `white-space:nowrap` still disables wrapping.

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
positions count element siblings and include type-aware fixed, odd, and even
matching. `:not(...)` compiles one simple-selector argument and uses that
argument's specificity; complex arguments remain a later extension. Dynamic
`:hover` and `:focus` use the same compiled selector path and are driven by
compact `El` references in the WinForms hit-test model.

`S.Ff` carries up to eight CSS font-family fallbacks; `V` resolves and caches
the first installed Windows family with a 32-entry bound. `S.Td` uses compact
`n/u/s/us` decoration modes. Underline, line-through, native decoration tags,
and explicit removal of a link underline all use the same `BoxFont` path.

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
cache. CSS width/height wins over HTML dimensions; otherwise `width` and
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
foreground, and font size using `t/a/bg/fg/fs` or equivalent literal
`document.*` assignments and `alert(...)`. `v/var` stores up to 16 small page
variables and `${name}` substitutes them into later literals. `tx/text` replaces
text under one safe element ID. `ac/rc/tc` add, remove, or toggle one bounded
class token on a safe ID and reuse the compiled CSS rebuild while preserving
mutable fields. Each target accepts at most 32 classes and 512 class-attribute
characters. `E.Ck(page, element)` and `V.Ck(element)` route
button and link `data-lea`/`onclick` values through this same parser. Lea ignores external scripts, JSON
data blocks, expressions, control flow, and unknown calls. Per-page limits are
eight script blocks, 16 KB per block, 32 accepted commands per run, 16 variables,
and 16 alerts.

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

**View > Lea Inspector** (`F12`) and each Lea tab's context menu open a compact
owned diagnostics window. Summary, Nodes, CSS, Requests, and Messages are built
only from the bounded public `V.Dg()` snapshot. `R`/`F5` refresh, `C` clears the
request activity through `V.Dcl()`, and `X`/Escape closes the window. Selecting
a legacy tab produces a neutral selection message and never casts or reflects
into the legacy browser control.

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
