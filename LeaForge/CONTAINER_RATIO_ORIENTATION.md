# Container Ratio And Orientation Queries

Milestone 137 extends Lea's bounded size-container path with the remaining
dimension-derived features in the current slice: `aspect-ratio` and
`orientation`. The implementation reuses compiled `Kx` conditions, compact
`S.Ct` eligibility, atomic `E.Cz` content-box snapshots, and WinForms feedback;
it does not add another parser, public object, or host-side layout model.

## Aspect Ratio

Lea compares the measured content-box width and height as a reduced integer
ratio. The bounded subset accepts these forms:

```css
@container card (aspect-ratio) { }
@container card (aspect-ratio: 4/3) { }
@container card (min-aspect-ratio: 1/1) { }
@container card (max-aspect-ratio: 3/2) { }
@container card (aspect-ratio >= 4/3) { }
@container card (4/3 <= aspect-ratio) { }
@container card (1/1 < aspect-ratio <= 4/3) { }
@container card (3/2 >= aspect-ratio > 1/1) { }
```

Numerators and denominators are integers from 1 through 8,192 and are reduced
with the existing greatest-common-divisor helper, so `8/6` and `4/3` compare
identically. Exact, `min-`/`max-`, strict/inclusive feature-first and
value-first, ascending and descending chained, and boolean forms share the
same query tree. Boolean `aspect-ratio` is active when the measured width is
nonzero. An explicit comparison against an indeterminate `0x0` box is inactive.

## Orientation

The discrete feature accepts:

```css
@container card (orientation: portrait) { }
@container card (orientation: landscape) { }
```

Portrait is active when measured height is greater than or equal to width, so
a square box is portrait. Landscape is active only when width is greater than
height. This follows the same physical-axis boundary as Lea's horizontal
writing-mode size-query subset.

## Eligibility And Composition

Both features require width and height. Only `container-type:size` is eligible;
an `inline-size` container remains eligible for width/inline-size features but
not ratio or orientation. Selection begins at the styled element's parent and
continues to use the nearest eligible unnamed or named ancestor.

Ratio and orientation terms compose with the existing bounded `not`, `and`,
and `or` logic, custom-property `style()` tests, nested container rules,
media/supports/layer conditions, linked and native sheets, pseudo-elements,
and SVG styles. Compact `@c(...)` diagnostics preserve the authored condition.
Class, stylesheet, and measured-size changes use the retained rebuild, so live
field identity, values, selection, and host ownership survive.

## Limits

- integer ratio components from 1 through 8,192
- two ratio terms for one chained range
- eight total size/style terms per container condition
- four nested conditional/container levels
- 2,048 UTF-16 units of condition text
- 256 measured containers per atomic snapshot
- three automatic WinForms feedback passes

Malformed, zero, decimal, missing-denominator, mixed-direction chain, unknown
orientation, unmeasured, and axis-ineligible queries stay inactive.

## Deliberately Unsupported

Decimal or single-number ratio syntax, relative query lengths, container units,
vertical writing modes, scroll-state queries, query CSSOM, and complete
intrinsic size-containment effects remain outside this milestone.

The feature definitions follow the W3C CSS Containment Module Level 3 draft:
https://www.w3.org/TR/css-contain-3/

## Verification

`container-features.html` and `Lea.Tests --container-features` cover missing
feedback, boolean and reduced exact ratios, min/max, feature-first,
value-first, ascending and descending chained ranges, portrait, landscape,
square portrait behavior, zero width, both-axis eligibility, grouped style and
size logic, nested containers, media/supports/layers, malformed rules,
diagnostics, native sheets, dynamic class sizes, retained fields, and exact
automatic WinForms geometry. The compatibility runner and compiled Firespider
smoke repeat the host-level feedback and dynamic relayout paths.
