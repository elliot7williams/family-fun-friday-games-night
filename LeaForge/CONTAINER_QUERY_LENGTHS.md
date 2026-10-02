# Relative Container Query Lengths

Milestone 139 extends Lea's bounded size-container query compiler with fixed
pixel, container-font, and root-font thresholds. It reuses the existing `Kx`
query tree and `E.Cz` content-box feedback rather than introducing a general
CSS math evaluator.

## Supported Lengths

Width, height, inline-size, and block-size query features accept:

```css
@container card (width >= 320px) { }
@container card (width > 39em) { }
@container page (49rem < width <= 50rem) { }
@container card (319.5px < width <= 320px) { }
@container card (0 < inline-size) { }
```

The units work in legacy `min-`/`max-` forms, exact colon forms, direct and
value-first comparisons, and ascending or descending chains. Each side of a
chain may use a different supported unit. Unitless values are accepted only
when the value is zero.

`px` values range from 0 through 8,192. `em` and `rem` values range from 0
through 100. All three units allow up to two fractional digits.

## Compact Model

One size-query term stores its magnitude as integer hundredths in `Kx.L` and
its basis as a one-character `Kx.U` code:

```text
p  CSS pixels
e  query-container font size
r  root-element font size
```

The evaluator multiplies measured integer dimensions and compiled thresholds
into the same hundredth-pixel scale. It performs no regular-expression,
decimal, or floating-point work while matching a query. Existing integer-pixel
queries use the same path.

## Resolution

An `em` threshold uses the computed inherited `S.Fs` on the selected query
container. A `rem` threshold walks the existing bounded `Scq` chain to the
root computed style. When no explicit computed font size exists, Lea uses the
16px CSS initial basis.

This basis belongs to the selected container. Two differently styled
containers can therefore evaluate the same `@container (width > 40em)` rule
differently. Changing a container font reevaluates `em` without changing
`rem`; changing the root font reevaluates `rem` without changing the
container's `em` basis. Mutable fields retain identity, value, selection, and
host ownership through those rebuilds.

## Limits

- 0..8,192px or 0..100em/rem
- at most two fractional digits
- up to eight combined size/style terms
- four conditional/container nesting levels
- 2,048 UTF-16 units of condition text
- existing 256-container snapshot and three-feedback-pass ceilings

Negative values, exponent notation, more than two fractional digits, nonzero
unitless values, calculations, viewport units, container-relative units,
font-metric units such as `ch`, and over-limit values remain unsupported.
Malformed rules stay inactive and do not partly enter the cascade.

The selected-container font basis follows CSS Containment Module Level 3:
https://www.w3.org/TR/css-contain-3/#size-features

## Verification

`container-query-lengths.html` and
`Lea.Tests --container-query-lengths` cover `px`/`em`/`rem`, decimal and zero
values, all comparison directions, mixed-unit chains, selected-container and
root font changes, axis eligibility, malformed limits, diagnostics, native
sheets, retained fields, and exact WinForms geometry. The compatibility runner
and compiled Firespider smoke repeat the dynamic font, root sheet, fractional
pixel, retention, and host feedback paths.
