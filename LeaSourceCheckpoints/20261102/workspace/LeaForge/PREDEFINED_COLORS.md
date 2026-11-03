# Bounded Predefined CSS Colors

Milestone 123 adds a bounded CSS `color()` core to Lea's shared color path.
The engine validates and retains authored color text in the existing compact
style records. The WinForms adapter converts it directly to sRGB while
painting, so no new public type, alias, dependency, browser runtime, or
host-side converter is required.

## Accepted syntax

Lea accepts modern space-separated channels with optional slash alpha:

```css
color: color(srgb 0.2 0.4 0.8);
background: color(display-p3 44.5% 52.9% 89.1% / 50%);
border-color: color(xyz-d50 0.2005 0.14089 0.4472);
```

The supported spaces are:

- `srgb`
- `srgb-linear`
- `display-p3`
- `display-p3-linear`
- `a98-rgb` (added by Milestone 124)
- `prophoto-rgb` (added by Milestone 124)
- `rec2020` (added by Milestone 124)
- `xyz`, an alias of `xyz-d65`
- `xyz-d50`
- `xyz-d65`

Each channel accepts a finite number or percentage, where 100% maps to 1.0.
Extended values remain valid for gamut mapping; conversion work is bounded to
-10..10 per channel. Alpha accepts a finite number or percentage and clamps to
transparent..opaque.

Shared base-color admission retains the existing 64-character ceiling, and the
WinForms functional-color painter independently caps a function body at 80
characters. Comma syntax, duplicate slashes, nested functions, missing
channels, non-finite numbers, unknown spaces, and the `none` channel keyword
reject the color.

## Conversion and gamut mapping

Lea follows the predefined-space model in
[CSS Color 4](https://drafts.csswg.org/css-color/). Gamma-encoded sRGB and
Display P3 use the extended sRGB transfer curve; their linear variants skip
that step. Official D65 matrices convert linear sRGB or Display P3 to XYZ.
`xyz` and `xyz-d65` feed D65 XYZ directly, while `xyz-d50` uses the Bradford
D50-to-D65 adaptation shared with CIE Lab.

The resulting D65 XYZ is converted to Oklab and then sRGB. In-gamut values take
a direct path. Out-of-gamut values reuse Lea's bounded local-MINDE search in
OkLCh, preserving lightness and hue while reducing chroma with the CSS 0.02 JND
and 0.0001 epsilon.

## Shared paths

Predefined colors use the same compact records and cascade behavior as existing
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

This core milestone does not add custom color profiles, relative colors,
`calc()` or other channel expressions, missing components, or arbitrary
color-space conversion. A98 RGB, ProPhoto RGB, and Rec. 2020 are added by
Milestone 124 and documented in `WIDE_GAMUT_COLORS.md`. Lea's compact page-script
`bg` and `fg` commands keep their command-specific validation; this milestone
covers authored CSS and the shared style paths that already use the bounded CSS
color parser.

## Verification

`test-pages/predefined-colors.html` covers the seven Milestone 123 space names,
numeric and percentage channels, alpha, equivalent gamma/linear P3 pairs,
extended and out-of-gamut channels, color-mix stops, malformed atomic fallback,
shadows, gradients, text, generated content, variables, layers, media,
`@supports`, fields, and SVG. `Lea.Tests.exe . --predefined-colors` checks
retained model state and exact bitmap paint, including the CSS reference P3
equivalence to `#6C88EA` and D50/D65 XYZ equivalence to `#7654CD`. The benchmark
and compiled Firespider host repeat integration-level model, paint, resize,
automation, and field-retention checks.
