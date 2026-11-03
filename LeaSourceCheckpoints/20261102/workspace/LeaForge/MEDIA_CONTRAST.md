# Preferred Contrast Media Queries

Milestone 128 adds bounded CSS `prefers-contrast` media queries to Lea's
compiled media records. The compact `Pc` alias carries the selected preference:

```csharp
var engine = new Lea.Engine.E { Pc = 'm' };
var page = engine.Go("page.html");
Console.WriteLine(page.Pc);
Console.WriteLine(engine.Dg(page).Pc);
```

`n` means no preference, `l` means less, `m` means more, and `c` means custom.
`E.Pc` defaults to `n`; `P.Pc` records the preference applied to a page, and
`G.Pc` exposes it in diagnostics. Invalid alias values are ignored. Setting
`V.Pc` gives an embedding host an explicit override. Otherwise, a WinForms view
samples Windows High Contrast when its handle is created and refreshes when it
receives `WM_SETTINGCHANGE`. An active Windows High Contrast setting maps to
`custom`, because the available platform flag does not establish whether the
chosen palette has more or less contrast. An inactive or unavailable setting
maps to the deterministic no-preference default.

## Supported Syntax

Lea accepts the discrete boolean and exact forms:

```css
@media (prefers-contrast) { }
@media (prefers-contrast: no-preference) { }
@media (prefers-contrast: less) { }
@media (prefers-contrast: more) { }
@media (prefers-contrast: custom) { }
@media (min-width: 700px) and (prefers-contrast: more) { }
```

The boolean form is inactive for no-preference and active for less, more, or
custom. Contrast conditions compose with width, height, orientation, aspect
ratio, resolution, color scheme, reduced motion, `and`, comma alternatives,
four nested `@media` levels, and `<style media>`. A boolean parent intersects
with an exact non-default child; impossible and conflicting intersections are
discarded before style application.

Unknown values, comparison ranges, trailing text, malformed conditions,
negated media, and arbitrary preference features stay inactive. This milestone
selects authored rules only. It does not implement `forced-colors`, enforce a
system color palette, expose CSS system colors, or infer whether a Windows High
Contrast palette is specifically higher or lower contrast.

## Diagnostics and Rebuilds

Active conditions append compact contrast labels to `G.Cs`:

```text
@pc
@pc=c
@m>=700&@pc=m
```

Changing `Pc` reselects existing compiled rules and rebuilds only when the
active rule set changes. Mutable `Fi` objects and their values are retained.
Headless hosts choose the preference explicitly through `E.Pc`; there is no
hidden operating-system dependency in the engine assembly.

Coverage lives in `test-pages/media-contrast.html`, focused
`Lea.Tests --media-contrast`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover aliases, defaults, all four exact values,
boolean semantics, width intersections, alternatives, nesting, style media,
malformed rejection, diagnostics, field retention, Windows preference
plumbing, responsive geometry, and exact paint.
