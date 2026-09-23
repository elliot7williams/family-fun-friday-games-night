# Bounded CIE Lab and LCH Colors

Milestone 122 adds modern CSS `lab()` and `lch()` to Lea's shared bounded color
path. The engine validates and retains authored color text in the existing
compact style records. The WinForms adapter converts it directly to sRGB while
painting, so no new public type, alias, dependency, browser runtime, or
host-side converter is required.

## Accepted syntax

Lea accepts modern space-separated channels with optional slash alpha:

```css
color: lab(44.36% 36.05 -58.99);
background: lab(44.36 28.84% -47.192% / 50%);
border-color: lch(44.36 69.1334 301.43deg);
```

Lightness accepts a finite number or percentage, where 100% maps to 100, and
clamps to 0..100. Lab `a` and `b` accept numbers or percentages, where 100%
maps to 125; conversion work is bounded by clamping each axis to -1250..1250.
LCH chroma accepts a number or percentage, where 100% maps to 150; negative
values clamp to zero and conversion work caps at 1500. LCH hue accepts unitless
degrees or `deg`, `grad`, `rad`, and `turn`, then wraps to one turn. Alpha
accepts a finite number or percentage and clamps to transparent..opaque.

Shared base-color admission retains the existing 64-character ceiling, and the
WinForms functional-color painter independently caps a function body at 80
characters. Comma syntax, duplicate slashes, nested functions, missing
channels, non-finite numbers, and the `none` channel keyword reject the color.

## Conversion and gamut mapping

CSS CIE Lab and LCH use a D50 reference white. Lea follows the
[CSS Color 4 conversion model](https://drafts.csswg.org/css-color/) to convert
Lab to D50 XYZ, then applies the specified Bradford D50-to-D65 adaptation
matrix. Adapted XYZ is converted to Oklab and then sRGB. Lightness at or below
zero maps to black; lightness at or above 100 maps to white.

In-gamut values take a direct conversion path. Out-of-gamut values reuse Lea's
bounded local-MINDE search in OkLCh: lightness and hue stay fixed while chroma
is reduced. The search uses the CSS reference JND of 0.02 and an epsilon of
0.0001. This provides stable sRGB output without adding a color-management
dependency or unbounded iteration.

## Shared paths

Lab and LCH use the same compact records and cascade behavior as existing
bounded colors. Covered paths include:

- text, backgrounds, physical and logical borders, and outlines
- text decoration, text shadow, and box shadow
- linear and radial gradient stops and two-stop sRGB color mixing
- ordinary native field foreground, background, and border paint
- generated content, custom properties, media rules, and cascade layers
- inline SVG fill, stroke, `color`, and gradient `stop-color`
- declaration checks in bounded CSS `@supports`

Malformed atomic shadow and gradient declarations preserve the previous valid
computed value. Responsive rebuilds retain mutable field identity and values.

## Compatibility boundary

This milestone does not add legacy comma forms, relative colors, `calc()` or
other channel expressions, wide-gamut `color()`, ICC profiles, or arbitrary
color-space conversion. Lea's compact page-script `bg` and `fg` commands keep
their command-specific validation; this milestone covers authored CSS and the
shared style paths that already use the bounded CSS color parser.

## Verification

`test-pages/lab-colors.html` covers numeric and percentage channels, every hue
unit, wrapping, clamping, alpha, D50 conversion, Bradford adaptation,
out-of-gamut mapping, color-mix stops, malformed atomic fallback, shadows,
gradients, text, generated content, variables, layers, media, `@supports`,
fields, and SVG. `Lea.Tests.exe . --lab` checks retained model state and exact
bitmap paint, including the CSS reference equivalence
`lab(44.36% 36.05 -58.99)` to `#7654CD`. The benchmark and compiled Firespider
host repeat integration-level model, paint, resize, and field-retention checks.
