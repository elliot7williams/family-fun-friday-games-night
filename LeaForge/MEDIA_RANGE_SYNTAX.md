# Media Range Syntax

Milestone 115 extends Lea's bounded compiled media records with Media Queries
Level 4 comparison syntax. It adds no public host state or evaluator. The same
compact `Vw`, `Vh`, and atomic `Vz(width, height)` viewport aliases drive every
comparison.

## Supported Forms

Whole-pixel width and height comparisons accept feature-first, value-first,
and equality forms:

```css
@media (width >= 700px) { }
@media (700px < width) { }
@media (height = 480px) { }
```

Reduced positive integer aspect ratios accept the same forms:

```css
@media (aspect-ratio > 4/3) { }
@media (16/9 > aspect-ratio) { }
```

Finite `dpi`, `dppx`, and `dpcm` resolution values accept the same forms and
chained ranges. Their conversion, bounds, and compact `Dp` host state are
documented in `MEDIA_RESOLUTION.md`.

Ascending and descending chains may mix strict and inclusive operators:

```css
@media (700px <= width < 1100px) { }
@media (768px >= height > 500px) { }
@media (1/1 < aspect-ratio <= 4/3) { }
@media (2/1 > aspect-ratio >= 3/2) { }
```

The operators are `<`, `<=`, `=`, `>=`, and `>`. A chain must move in one
direction; mixed forms such as `(400px < width > 700px)` stay inactive.
Comparisons compose with legacy min/max/exact syntax, orientation,
conjunctions, comma alternatives, four nested `@media` levels, and
`<style media>`.

## Bounds and Exactness

Dimension terms are whole `px` values from 1 through 8,192. Ratio numerators
and denominators are positive whole numbers in the same range and reduce by
GCD. Ratio comparisons use 64-bit cross multiplication, not floating point.

Strictness is retained in the compiled condition rather than rounded to an
adjacent value. Nested intersections therefore reject empty edges such as
`(800px < width)` combined with `(width <= 800px)`, and exact 4/3 viewports do
not satisfy `(aspect-ratio > 4/3)`.

Diagnostics preserve strict boundaries with compact labels:

```text
@m>=700&@m<1100
@h>500&@h<=768
@r>1/1&@r<=4/3
```

Decimal dimension values, non-pixel dimensions, zero or malformed ratios,
mixed chains, over-bound terms, device dimensions/ratios, `calc()`, variables,
and unknown features remain inactive. This is a deterministic bounded subset,
not a general media-feature evaluator.

Coverage lives in `test-pages/media-ranges.html`, focused
`Lea.Tests --media-ranges`, the compatibility benchmark, and the compiled
Firespider host smoke. Tests cover both operand orders, ascending/descending
chains, equality, strict boundaries, nesting, `<style media>`, invalid and
empty ranges, diagnostics, field retention, geometry, and live resize.
