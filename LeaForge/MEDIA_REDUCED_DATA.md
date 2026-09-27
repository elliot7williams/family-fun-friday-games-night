# Reduced Data Media Queries

Milestone 130 adds bounded CSS `prefers-reduced-data` media queries to Lea's
compiled media records. The compact `Pdt` alias carries the selected preference:

```csharp
var engine = new Lea.Engine.E { Pdt = 'r' };
var page = engine.Go("page.html");
Console.WriteLine(page.Pdt);
Console.WriteLine(engine.Dg(page).Pdt);
```

`n` means no preference and `r` means reduce. `E.Pdt` defaults to `n`, `P.Pdt`
records the preference applied to a page, and `G.Pdt` exposes it in diagnostics.
Invalid alias values are ignored. `V.Pdt` gives an embedding host the same
explicit control. Lea does not infer this preference from a network connection
or operating-system setting.

## Supported Syntax

Lea accepts the discrete boolean, no-preference, and reduce forms:

```css
@media (prefers-reduced-data) { }
@media (prefers-reduced-data: no-preference) { }
@media (prefers-reduced-data: reduce) { }
@media (min-width: 700px) and (prefers-reduced-data: reduce) { }
```

The boolean form is inactive for no-preference and active for reduce. Data
conditions compose with width, height, orientation, aspect ratio, resolution,
color scheme, motion, contrast, scripting, `and`, comma alternatives, four
nested `@media` levels, and `<style media>`. Conflicting nested data
preferences are discarded before style application.

Unknown values, comparison ranges, trailing text, malformed conditions,
negated media, and arbitrary preference features stay inactive. This milestone
selects authored CSS only. It does not add the `Save-Data` request header,
cancel or replace resources, rewrite markup, throttle networking, or choose
smaller images automatically. A host can combine `Pdt` with its own resource
or request policy when it deliberately implements those behaviors.

## Diagnostics and Rebuilds

Active conditions append compact data-preference labels to `G.Cs`:

```text
@pdt=n
@m>=700&@pdt=r
```

Changing `Pdt` reselects existing compiled rules and rebuilds only when the
active rule set changes. Mutable `Fi` objects and their values are retained.
Headless and WinForms hosts both choose the preference explicitly; the engine
assembly has no hidden platform or connection dependency.

Coverage lives in `test-pages/media-reduced-data.html`, focused
`Lea.Tests --media-data`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover aliases, defaults, boolean semantics, exact
values, width intersections, alternatives, nesting, style media, malformed
rejection, diagnostics, field retention, host control, responsive geometry,
and exact paint.
