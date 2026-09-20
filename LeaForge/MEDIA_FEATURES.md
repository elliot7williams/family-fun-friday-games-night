# Height and Orientation Media Features

Milestone 113 extends Lea's existing bounded media compiler from one viewport
axis to two. It keeps the compact width alias and adds:

```csharp
engine.Vh = 720;
engine.Vz(1024, 720);
```

`Vh` is the applied viewport height. `Vz(width, height)` is the preferred
two-letter host call when both dimensions change because it clamps and applies
the pair in one responsive rebuild. Width and height clamp independently to
1..8192 pixels. The headless defaults are 1024 by 768.

`P.Vw/Vh` expose the pair applied to a page. `G.Vw/Vh` expose the same pair in
diagnostics. A WinForms `V` updates both values from its client size and keeps
`V.Vw/Vh` read-only for hosts.

## Supported Queries

Lea accepts `screen` and `all` queries containing:

- `width`, `min-width`, and `max-width` in whole pixels
- `height`, `min-height`, and `max-height` in whole pixels
- `orientation: portrait` and `orientation: landscape`
- `aspect-ratio`, `min-aspect-ratio`, and `max-aspect-ratio` as bounded
  positive integer fractions
- feature-first, value-first, and ascending/descending comparison ranges for
  width, height, and aspect ratio
- `and` conjunctions and comma alternatives
- up to four nested `@media` levels and eight resulting alternatives
- the same conditions in a `<style media>` attribute

Portrait is active when height is greater than or equal to width, so a square
viewport is portrait. Landscape requires width to be greater than height.
Nested conditions intersect both dimension ranges and orientation. Impossible
ranges and conflicting orientations stay inactive.

Print, negated, unknown, malformed, non-pixel dimension, mixed-direction range,
and unknown orientation queries stay inactive. This remains a deliberately
bounded screen media subset, not the complete CSS media-query standard.

Aspect-ratio normalization, exact comparison, diagnostics, and limits are
documented separately in `MEDIA_ASPECT_RATIO.md`. Modern comparison grammar and
strict-boundary behavior are in `MEDIA_RANGE_SYNTAX.md`.

## Diagnostics and State

Active CSS diagnostics retain `@m` for width and add `@h` for height and `@o`
for orientation. For example:

```text
@m>=800&@h<=500&@o=l
```

Changing either viewport dimension recompiles no CSS. Lea selects from the
existing compiled rule records and rebuilds only when the active set changes.
Mutable `Fi` field objects and their values are retained across those rebuilds.

Coverage lives in `test-pages/media-features.html`, the focused
`Lea.Tests --media-features` mode, the compatibility benchmark, and the
compiled Firespider host smoke. Those checks cover atomic and individual
updates, exact/min/max height, both orientations, square boundaries,
alternatives, nested intersections, style media, invalid queries, diagnostics,
clamps, field retention, geometry, and live control resizing.
