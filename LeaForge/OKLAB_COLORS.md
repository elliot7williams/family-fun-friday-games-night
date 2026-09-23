# Bounded Oklab and OkLCh Colors

Milestone 121 adds modern CSS `oklab()` and `oklch()` to Lea's shared bounded
color path. The engine validates and retains authored color text in the existing
compact style records. The WinForms adapter converts it directly to sRGB while
painting, so no new public type, alias, dependency, browser runtime, or
host-side converter is required.

## Accepted syntax

Lea accepts modern space-separated channels with optional slash alpha:

```css
color: oklab(0.627955 0.224863 0.125846);
background: oklab(62.7955% 56.2158% 31.4616% / 50%);
border-color: oklch(0.452014 0.313214 264.052deg);
```

Lightness accepts a finite number or percentage and clamps to 0..1. Oklab `a`
and `b` accept numbers or percentages, where 100% maps to 0.4; conversion work
is bounded by clamping each axis to -4..4. OkLCh chroma accepts a number or
percentage, where 100% maps to 0.4; negative values clamp to zero and conversion
work caps at 4. OkLCh hue accepts unitless degrees or `deg`, `grad`, `rad`, and
`turn`, then wraps to one turn. Alpha accepts a finite number or percentage and
clamps to transparent..opaque.

Shared base-color admission retains the existing 64-character ceiling, and the
WinForms functional-color painter independently caps a function body at 80
characters. Comma syntax, duplicate slashes, nested functions, missing channels,
non-finite numbers, and the `none` channel keyword reject the color.

## Conversion and gamut mapping

Lea uses the D65 Oklab/XYZ/sRGB matrices and extended sRGB transfer functions
from [CSS Color 4](https://drafts.csswg.org/css-color/). In-gamut values take a
direct conversion path. Out-of-gamut values use a bounded local-MINDE search in
OkLCh: lightness and hue stay fixed while chroma is reduced. The search uses the
CSS reference JND of 0.02 and an epsilon of 0.0001. Lightness at or below zero
maps to black; lightness at or above one maps to white.

This gives stable sRGB output without adding a color-management dependency or
unbounded iteration.

## Shared paths

Oklab and OkLCh use the same compact records and cascade behavior as existing
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
other channel expressions, ICC profiles, or arbitrary color-space conversion.
CIE Lab/LCH and bounded predefined sRGB/Display-P3/A98/ProPhoto/Rec. 2020/XYZ
`color()` are documented separately in `LAB_COLORS.md`,
`PREDEFINED_COLORS.md`, and `WIDE_GAMUT_COLORS.md`. Lea's compact page-script `bg` and `fg`
commands keep their command-specific validation; this milestone covers authored
CSS and the shared style paths that already use the bounded CSS color parser.

## Verification

`test-pages/oklab-colors.html` covers numeric and percentage channels, every hue
unit, wrapping, clamping, alpha, out-of-gamut mapping, color-mix stops, malformed
atomic fallback, shadows, gradients, text, generated content, variables, layers,
media, `@supports`, fields, and SVG. `Lea.Tests.exe . --oklab` checks retained
model state and exact bitmap paint. The benchmark and compiled Firespider host
repeat integration-level model, paint, resize, and field-retention checks.
