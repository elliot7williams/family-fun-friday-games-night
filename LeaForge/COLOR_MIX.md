# Bounded CSS Color Mixing

Milestone 120 adds a bounded two-stop CSS `color-mix()` subset to Lea's shared
color path. The engine validates and retains the authored expression, while the
WinForms adapter resolves it directly during paint. Existing compact color
records carry the value, so no new public type, alias, dependency, browser
runtime, or host-side mixer is required.

## Accepted syntax

Lea accepts two colors in the sRGB interpolation space:

```css
color: color-mix(in srgb, red, blue);
background: color-mix(in srgb, hsl(120 100% 25%) 25%, hwb(240 0% 0%));
outline-color: color-mix(in srgb, oklab(.63 .22 .13), oklch(.45 .31 264));
border-color: color-mix(in srgb, currentColor 40%, white);
```

Each stop accepts one existing bounded named, hex, RGB/RGBA, HSL/HSLA, HWB,
CIE Lab/LCH, Oklab/OkLCh, `transparent`, or `currentColor` value and an optional finite 0..100%
percentage. Nested `color-mix()` stops are rejected.

If neither percentage is present, both weights are 50%. If one is omitted, it
receives the complement to 100%. When both are present, positive totals are
normalized. A total below 100% proportionally reduces the resulting alpha; a
total above 100% normalizes to an opaque-scale mix. A zero total is invalid.

Interpolation uses premultiplied alpha in sRGB. Transparent and translucent
stops therefore retain the visible stop's hue instead of introducing dark RGB
channels. Output channels and alpha are clamped to byte range at paint time.

The outer expression is capped at 160 characters, while each base color keeps
the existing 64-character ceiling. Exactly two stops and one interpolation
space are accepted.

## Shared paths

Color mixing uses the existing compact records and cascade behavior across:

- text, backgrounds, physical and logical borders, and outlines
- text decoration, text shadow, and box shadow
- linear and radial gradient stops
- ordinary native field foreground, background, and border paint
- generated content, custom properties, media rules, and cascade layers
- inline SVG fill, stroke, `color`, and gradient `stop-color`
- declaration checks in bounded CSS `@supports`

Malformed atomic shadow and gradient declarations preserve the previous valid
computed value. Responsive rebuilds retain mutable field identity and values.

## Compatibility boundary

This milestone does not add interpolation spaces other than sRGB, custom color
profiles, hue interpolation methods, more than two stops, nested mixes,
relative colors, or channel expressions. Negative and over-100 stop
percentages, a zero total, unknown spaces, and malformed grouping are rejected.
Lea's compact page-script `bg` and `fg` commands keep their existing
command-specific validation; this milestone covers authored CSS and shared
style paths that already use the bounded CSS color parser.

## Verification

`test-pages/color-mix.html` covers equal and omitted weights, low/high totals,
transparent and translucent premultiplied alpha, functional stops,
`currentColor`, malformed atomic fallback, shadows, gradients, text, generated
content, variables, layers, media, `@supports`, fields, and SVG.
`Lea.Tests.exe . --color-mix` checks retained model state and exact bitmap
paint. The benchmark and compiled Firespider host repeat integration-level
model, paint, resize, and field-retention checks.
