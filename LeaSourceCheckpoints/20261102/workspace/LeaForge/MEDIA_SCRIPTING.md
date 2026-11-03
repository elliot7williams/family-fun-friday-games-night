# Scripting Media Queries

Milestone 129 adds bounded CSS `scripting` media queries to Lea's compiled
media records. It reuses the existing compact `Je` scripting-permission alias:

```csharp
var engine = new Lea.Engine.E { Je = false };
var page = engine.Go("page.html");
Console.WriteLine(page.Je);
Console.WriteLine(engine.Dg(page).Je);
```

`true` reports `enabled`; `false` reports `none`. `E.Je` defaults to true,
`P.Je` records the state applied to a page, and `G.Je` exposes it in
diagnostics. `V.Je` gives an embedding host the same live switch. Lea has no
initial-load-only script mode, so it recognizes `initial-only` as valid syntax
but never reports or selects that state.

## Supported Syntax

Lea accepts the discrete boolean and exact forms:

```css
@media (scripting) { }
@media (scripting: none) { }
@media (scripting: initial-only) { }
@media (scripting: enabled) { }
@media (min-width: 700px) and (scripting: enabled) { }
```

The boolean form is inactive for `none` and active for `enabled`. An
`initial-only` rule remains inactive because Lea does not expose that runtime
mode. Scripting conditions compose with width, height, orientation, aspect
ratio, resolution, color scheme, motion, contrast, `and`, comma alternatives,
four nested `@media` levels, and `<style media>`. A boolean parent intersects
with an exact enabled child; impossible and conflicting intersections are
discarded before style application.

Unknown values, comparison ranges, trailing text, malformed conditions,
negated media, and arbitrary scripting features stay inactive.

## State Changes

Changing `E.Je` or `V.Je` reselects existing compiled rules and rebuilds only
when the active rule set changes. Mutable `Fi` objects and their values are
retained. Disabling scripting blocks later `Js` commands. Re-enabling it allows
later commands, but does not rerun inline commands skipped while the page was
loaded with scripting disabled and does not undo commands that already ran.

Active conditions append compact labels to `G.Cs`:

```text
@je
@je=e
@m>=700&@je=e
@je=n
```

This media feature reports Lea's bounded allow-listed command layer. It does
not add general JavaScript evaluation, arbitrary DOM APIs, modules, promises,
timers, or a JavaScript compatibility claim.

Coverage lives in `test-pages/media-scripting.html`, focused
`Lea.Tests --media-scripting`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover both runtime states, boolean semantics,
`initial-only`, width intersections, alternatives, nesting, style media,
malformed rejection, diagnostics, field retention, responsive geometry, UI
Automation visibility, and exact paint.
