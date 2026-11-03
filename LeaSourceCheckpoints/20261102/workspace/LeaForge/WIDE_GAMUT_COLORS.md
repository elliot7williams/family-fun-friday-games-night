# Bounded Wide-Gamut CSS Colors

Milestone 124 completes Lea's bounded CSS Color 4 predefined RGB family by
adding `a98-rgb`, `prophoto-rgb`, and `rec2020` to the shared `color()` path.
The engine retains authored text in its existing compact color records, while
the WinForms adapter converts it to sRGB during paint. No public type, alias,
dependency, browser runtime, ICC loader, or host-side converter is added.

## Accepted syntax

Lea accepts modern space-separated channels with optional slash alpha:

```css
color: color(a98-rgb 0.44091 0.49971 0.37408);
background: color(prophoto-rgb 36.589% 41.717% 31.333% / 50%);
border-color: color(rec2020 0.522 0.557 0.892);
```

Each channel accepts a finite number or percentage, where 100% maps to 1.0.
Extended values remain valid for gamut mapping; conversion work keeps the
existing -10..10 per-channel bound. Alpha accepts a finite number or percentage
and clamps to transparent..opaque.

Shared base-color admission keeps the 64-character ceiling, and the WinForms
functional-color painter independently caps a function body at 80 characters.
Comma syntax, duplicate slashes, nested functions, missing channels,
non-finite numbers, unknown spaces, and the `none` channel keyword reject the
color atomically.

## Conversion and gamut mapping

Lea follows the predefined-space algorithms and matrices in
[CSS Color 4](https://drafts.csswg.org/css-color/):

- A98 RGB uses the signed 563/256 decoding exponent and its D65 XYZ matrix.
- ProPhoto RGB uses a 1/16 linear toe through 16/512, signed gamma 1.8 above
  it, its D50 XYZ matrix, and Lea's shared Bradford D50-to-D65 adaptation.
- Rec. 2020 uses the signed 2.4 decoding exponent and its D65 XYZ matrix.

The resulting D65 XYZ value is converted to Oklab and then sRGB. In-gamut
values use the direct path. Out-of-gamut values reuse Lea's bounded local-MINDE
search in OkLCh, preserving lightness and hue while reducing chroma with the
CSS 0.02 JND and 0.0001 epsilon.

## Shared paths

Wide-gamut colors use the existing compact records and cascade behavior across:

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

This milestone does not add custom `@color-profile`/ICC spaces, Rec. 2100 HDR
spaces, invented linear aliases for A98/ProPhoto/Rec. 2020, relative colors,
`calc()` or other channel expressions, or missing `none` components. Lea's
compact page-script `bg` and `fg` commands keep their command-specific
validation; this milestone covers authored CSS and shared style paths already
using the bounded color parser.

## Verification

`test-pages/wide-gamut-colors.html` covers all three spaces, numeric and
percentage channels, each transfer curve, ProPhoto D50 adaptation, alpha,
extended and out-of-gamut channels, color-mix stops, malformed atomic fallback,
shadows, gradients, text, generated content, variables, layers, media,
`@supports`, fields, and SVG. `Lea.Tests.exe . --wide-gamut-colors` checks
retained model state and exact bitmap paint. It includes the CSS reference A98
and ProPhoto leaf equivalence to `color(srgb 0.41587 0.50367 0.36664)`, plus
the Rec. 2020 equivalence `color(rec2020 0.522 0.557 0.892)` to `#6C88EA`.
The benchmark and compiled Firespider host repeat integration-level model,
paint, automation, resize, and field-retention checks.
