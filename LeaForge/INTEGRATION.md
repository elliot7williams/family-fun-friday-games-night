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

The `V` host control now performs cached layout and bounded scrolling. Firespider
can read `Dh`, `Sy`, and `Bc`, and can call `Sc(delta)` without depending on
WinForms mouse-wheel internals.

Basic fields are exposed through `P.Fs`. The view paints inactive controls on
the Lea surface, reuses one editor for focused text, and provides `Rs`, `Sm`,
and `SmA` for reset and supported GET submission. `P.Q` preserves query text on
both web and local pages.

## Experimental Firespider tab

The source in `../Lea.Firespider` now powers a real experimental tab in the
Firespider project. **File > New Lea Tab** or `Ctrl+Alt+L` creates a `Lea.Win.V`
inside Firespider's existing tab control. Back, forward, home, go, search,
reload, and stop dispatch to Lea while that tab is selected and delegate to the
original handlers for legacy tabs.

Firespider imports one `Lea/FirespiderLea.targets` file. The import supplies the
two DLL references, partial VB adapter, and bundled local start page. Debug and
Release builds pass `Smoke.cs`, which verifies tab creation, start-page loading,
and host Back-button routing without loading another browser engine.

## Integration rules

- Reference the core engine, not the test browser shell, when embedding Lea.
- Keep browser window code in the user's project or in the optional Lea.Win layer.
- Do not require a hidden WebView or a downloaded browser runtime.
- Keep the core API stable and versioned before Lea 1.0.
- Include examples for both a browser window and a headless parser test.

The project split will happen after the current Forge renderer is separated into
modules. Until then, `LeaForge` is the active experimental host and `ShortApi.cs`
is the compact API prototype.
