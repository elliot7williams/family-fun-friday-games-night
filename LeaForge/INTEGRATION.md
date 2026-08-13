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
failures without parsing exception text.

The `V` host control now performs cached layout and bounded scrolling. Firespider
can read `Dh`, `Sy`, and `Bc`, and can call `Sc(delta)` without depending on
WinForms mouse-wheel internals. `Rb(index)` and `Rc(index)` expose deterministic
outer-border and content rectangles for layout tests and host diagnostics.

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

Computed styles use `S.Ta` for compact `l/c/r/j` alignment, `S.Lh` for pixel
line height, and `S.Lm` for a relative multiplier. `V` applies these to wrapped
blocks and table cells, centers glyphs within taller lines, and justifies only
wrapped non-final lines. Blocks without these properties retain the native
single-box text path.

`E` and `V` expose the same cookie aliases: `Kc`, `Km`, `Kcl()`, and `Pv`.
Cookies remain in memory and are bounded to 64 entries. Firespider maps its
existing `pv` flag to each active Lea view, so private mode receives a fresh
isolated jar and crossing the privacy boundary clears Lea's page cache.

External `<link rel="stylesheet">` resources use the page URL for relative
resolution and the same request layer as navigation. They therefore share
cookies, compression, conditional validation, timeout/cancellation, `Cm` LRU
capacity, and `Cl()` clearing. A page may load eight links of up to 256 KB each;
missing, oversized, unsupported-media, and non-CSS links are skipped safely.

Basic fields are exposed through `P.Fs`. The view paints inactive controls on
the Lea surface, reuses one editor for focused text, opens native choice menus
for single-select fields, and provides `Rs`, `Sm`, and `SmA` for reset and
URL-encoded GET/POST submission. Direct engine and view POST navigation use
`Po`/`PoA`. `P.Q` preserves query text on both web and local pages.

## Experimental Firespider tab

The source in `../Lea.Firespider` now powers a real experimental tab in the
Firespider project. **File > New Lea Tab** or `Ctrl+Alt+L` creates a `Lea.Win.V`
inside Firespider's existing tab control. Back, forward, home, go, search,
reload, and stop dispatch to Lea while that tab is selected and delegate to the
original handlers for legacy tabs.

Close Tab (`Ctrl+W`), the tab-strip close target, and middle-click now dispatch
through Lea-aware close handling. Page Source (`Ctrl+U`) fills Firespider's
existing source window from `V.H`. Lea sessions use a separate bounded file at
`%LocalAppData%\Firespider Internet\Lea.session`; private mode removes it, and
legacy tabs continue using Firespider's existing settings collections.

Firespider imports one `Lea/FirespiderLea.targets` file. The import supplies the
two DLL references, partial VB adapter, and bundled local start page. Debug and
Release builds pass `Smoke.cs`, which verifies tab creation, start-page loading,
host navigation, timer status, close routing, page source, and save/restore
across two host forms without loading another browser engine or touching the
user's real session. The adapter removes unconditional `WebBrowser` casts from
the paths it owns and delegates by active tab type.

## Integration rules

- Reference the core engine, not the test browser shell, when embedding Lea.
- Keep browser window code in the user's project or in the optional Lea.Win layer.
- Do not require a hidden WebView or a downloaded browser runtime.
- Keep the core API stable and versioned before Lea 1.0.
- Include examples for both a browser window and a headless parser test.

The project split will happen after the current Forge renderer is separated into
modules. Until then, `LeaForge` is the active experimental host and `ShortApi.cs`
is the compact API prototype.
