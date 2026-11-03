# Aspect-Ratio Media Queries

Milestone 114 adds bounded viewport aspect-ratio conditions to Lea's existing
compiled media records:

```css
@media (aspect-ratio: 4/3) { }
@media (min-aspect-ratio: 16/9) { }
@media (max-aspect-ratio: 3/4) { }
```

Ratios use two positive whole numbers from 1 through 8192. Lea reduces each
fraction with its greatest common divisor, so `8/6` and `4/3` compile to the
same condition and diagnostic. Comparisons use bounded 64-bit cross
multiplication rather than floating-point division.

Exact, minimum, and maximum ratios work with `screen`/`all`, width and height
conditions, portrait/landscape orientation, `and` conjunctions, comma
alternatives, four nested `@media` levels, and `<style media>`. Nested ratio
ranges intersect exactly. Empty intersections are discarded before style
rules are created.

Active diagnostics use compact `@r` labels:

```text
@r=4/3
@r>=16/9
@m>=700&@r>=4/3&@r<=16/9
```

Zero, negative, decimal, missing-denominator, over-bound, malformed, and
unknown ratio features stay inactive. Device aspect-ratio and general
media-feature evaluation remain outside this bounded subset. Bounded
feature-first, value-first, and chained comparison syntax is documented in
`MEDIA_RANGE_SYNTAX.md`; display resolution is documented separately in
`MEDIA_RESOLUTION.md`.

No new page or host API is needed. Headless hosts continue to use
`E.Vz(width,height)`, and WinForms `V` derives the ratio from its live client
viewport. Rule activation retains mutable `Fi` field identity and values.

Coverage lives in `test-pages/media-aspect-ratio.html`, focused
`Lea.Tests --media-aspect`, the compatibility benchmark, and the compiled
Firespider host smoke. The checks cover normalization, exact and inclusive
ranges, orientation conjunctions, alternatives, nested intersections,
`<style media>`, invalid inputs, diagnostics, field retention, geometry, and
live wide/tall/square transitions.
