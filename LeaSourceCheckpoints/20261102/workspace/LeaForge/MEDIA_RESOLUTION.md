# Resolution Media Queries

Milestone 125 adds bounded CSS resolution media queries to Lea's compiled media
records. The compact `Dp` alias carries display density in dots per inch:

```csharp
var engine = new Lea.Engine.E { Dp = 192 };
var page = engine.Go("page.html");
Console.WriteLine(page.Dp);
Console.WriteLine(engine.Dg(page).Dp);
```

`E.Dp` clamps to 1..960 dpi and defaults to 96. `P.Dp` records the density
applied to a page, and `G.Dp` exposes it in diagnostics. Setting `V.Dp` gives an
embedding host an explicit override. Otherwise, the WinForms view samples
`Graphics.DpiX` when its handle is created and follows `WM_DPICHANGED` when the
window moves between monitors.

## Supported Syntax

Lea accepts boolean, legacy, feature-first, value-first, and chained ranges:

```css
@media (resolution) { }
@media (resolution: 96dpi) { }
@media (min-resolution: 2dppx) { }
@media (max-resolution: 75.5906dpcm) { }
@media (resolution >= 1.5dppx) { }
@media (144dpi <= resolution) { }
@media (1dppx < resolution <= 2dppx) { }
```

The supported units are `dpi`, `dppx`, and `dpcm`. Lea uses the CSS conversion
`1dppx = 96dpi` and normalizes all query values to thousandths of a dpi before
comparison. This preserves equivalent practical values such as `96dpi`,
`1dppx`, and `37.7953dpcm` without adding floating-point state to each rule.

Resolution conditions compose with width, height, orientation, aspect ratio,
`and`, comma alternatives, four nested `@media` levels, and `<style media>`.
Nested conditions intersect in the same compiled condition, and empty strict
ranges are discarded before style application.

## Bounds and Diagnostics

Query numbers are finite signed decimals with at most four integer and four
fractional digits. Normalized values must stay within the engine's bounded
integer range. Zero and negative comparisons retain their normal range meaning;
because the host density is always positive, exact zero and non-positive upper
bounds do not activate. `infinite`, `calc()`, variables, unknown units,
mixed-direction chains, trailing text, and malformed forms stay inactive.

Active CSS diagnostics use compact `@d` labels:

```text
@d=96dpi
@d>=144dpi
@d>96dpi&@d<=192dpi
```

Changing density reselects existing compiled rules and rebuilds only when the
active rule set changes. Mutable `Fi` objects and their values are retained.
The density affects media selection; it does not automatically scale authored
CSS pixels or add device-width/device-height/device-aspect-ratio features.

Coverage lives in `test-pages/media-resolution.html`, focused
`Lea.Tests --media-resolution`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover unit conversion, every supported syntax,
strict and negative bounds, width intersections, alternatives, nesting,
diagnostics, malformed rejection, field retention, WinForms DPI plumbing,
responsive geometry, and exact paint.
