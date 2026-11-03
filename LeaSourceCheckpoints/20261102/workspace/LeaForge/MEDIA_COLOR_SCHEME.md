# Preferred Color Scheme Media Queries

Milestone 126 adds bounded CSS `prefers-color-scheme` media queries to Lea's
compiled media records. The compact `Th` alias carries the selected theme:

```csharp
var engine = new Lea.Engine.E { Th = 'd' };
var page = engine.Go("page.html");
Console.WriteLine(page.Th);
Console.WriteLine(engine.Dg(page).Th);
```

`l` means light and `d` means dark. `E.Th` defaults to `l`; `P.Th` records the
theme applied to a page, and `G.Th` exposes it in diagnostics. Invalid alias
values are ignored. Setting `V.Th` gives an embedding host an explicit
override. Otherwise, a WinForms view reads Windows
`AppsUseLightTheme` when its handle is created and refreshes when it receives a
`WM_SETTINGCHANGE` notification. Missing or inaccessible Windows settings use
the deterministic light default.

## Supported Syntax

Lea accepts the discrete boolean, light, and dark forms:

```css
@media (prefers-color-scheme) { }
@media (prefers-color-scheme: light) { }
@media (prefers-color-scheme: dark) { }
@media (min-width: 700px) and (prefers-color-scheme: dark) { }
```

The boolean form is active for both supported values. Theme conditions compose
with width, height, orientation, aspect ratio, resolution, `and`, comma
alternatives, four nested `@media` levels, and `<style media>`. Conflicting
nested light and dark conditions are discarded before style application.

`no-preference`, unknown values, comparison ranges, trailing text, malformed
conditions, negated media, and arbitrary preference features stay inactive.
Lea does not synthesize a `color-scheme` property, recolor authored content,
or provide dark defaults for native fields. This milestone selects authored
rules only.

## Diagnostics and Rebuilds

Active conditions append compact theme labels to `G.Cs`:

```text
@t=l
@m>=700&@t=d
```

Changing `Th` reselects existing compiled rules and rebuilds only when the
active rule set changes. Mutable `Fi` objects and their values are retained.
Headless hosts choose the theme explicitly through `E.Th`; there is no hidden
operating-system dependency in the engine assembly.

Coverage lives in `test-pages/media-color-scheme.html`, focused
`Lea.Tests --media-theme`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover aliases, defaults, boolean/light/dark
selection, width intersections, alternatives, nesting, style media, malformed
rejection, diagnostics, field retention, Windows theme plumbing, responsive
geometry, and exact paint.
