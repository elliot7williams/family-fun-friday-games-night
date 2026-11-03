# Reduced Motion Media Queries

Milestone 127 adds bounded CSS `prefers-reduced-motion` media queries to Lea's
compiled media records. The compact `Rm` alias carries the selected preference:

```csharp
var engine = new Lea.Engine.E { Rm = 'r' };
var page = engine.Go("page.html");
Console.WriteLine(page.Rm);
Console.WriteLine(engine.Dg(page).Rm);
```

`n` means no preference and `r` means reduce. `E.Rm` defaults to `n`; `P.Rm`
records the preference applied to a page, and `G.Rm` exposes it in diagnostics.
Invalid alias values are ignored. Setting `V.Rm` gives an embedding host an
explicit override. Otherwise, a WinForms view reads Windows client-area
animation policy when its handle is created and refreshes when it receives a
`WM_SETTINGCHANGE` notification. Enabled or unavailable animation policy uses
the deterministic no-preference default; disabled client-area animation maps
to reduce.

## Supported Syntax

Lea accepts the discrete boolean, no-preference, and reduce forms:

```css
@media (prefers-reduced-motion) { }
@media (prefers-reduced-motion: no-preference) { }
@media (prefers-reduced-motion: reduce) { }
@media (min-width: 700px) and (prefers-reduced-motion: reduce) { }
```

The boolean form is inactive for no-preference and active for reduce. Motion
conditions compose with width, height, orientation, aspect ratio, resolution,
color scheme, `and`, comma alternatives, four nested `@media` levels, and
`<style media>`. Conflicting nested motion preferences are discarded before
style application.

Unknown values, comparison ranges, trailing text, malformed conditions,
negated media, and arbitrary preference features stay inactive. Lea does not
implement CSS transitions, animations, smooth scrolling, a site preference
manager, or automatic content rewriting in this milestone. The feature selects
authored rules only.

## Diagnostics and Rebuilds

Active conditions append compact motion labels to `G.Cs`:

```text
@rm=n
@m>=700&@rm=r
```

Changing `Rm` reselects existing compiled rules and rebuilds only when the
active rule set changes. Mutable `Fi` objects and their values are retained.
Headless hosts choose the preference explicitly through `E.Rm`; there is no
hidden operating-system dependency in the engine assembly.

Coverage lives in `test-pages/media-reduced-motion.html`, focused
`Lea.Tests --media-motion`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover aliases, defaults, boolean semantics, exact
values, width intersections, alternatives, nesting, style media, malformed
rejection, diagnostics, field retention, Windows policy plumbing, responsive
geometry, and exact paint.
