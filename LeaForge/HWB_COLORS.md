# Bounded HWB Colors

Milestone 119 adds CSS `hwb()` to Lea's shared bounded color path. The engine
keeps authored color text in the existing compact style records, and the
WinForms adapter converts it directly to ARGB while painting. No new public
type, alias, dependency, browser runtime, or host-side converter is required.

## Accepted syntax

Lea accepts the modern space-separated form with optional slash alpha:

```css
color: hwb(120 20% 30%);
background-color: hwb(240 0% 0% / 50%);
border-color: hwb(0 0% 0% / .5);
```

Hue is a finite signed decimal or exponent number. A unitless hue is degrees;
`deg`, `grad`, `rad`, and `turn` are also accepted, and hue wraps into one turn.
Whiteness and blackness require percentages and each clamps to 0..100%. Alpha
accepts a finite number or percentage and clamps to transparent..opaque.

If clamped whiteness plus blackness is at least 100%, Lea applies the CSS gray
normalization rule: the result is `whiteness / (whiteness + blackness)` on all
three RGB channels. Otherwise it scales the pure hue by the remaining channel
range and adds whiteness.

Shared validated color components retain the existing 64-character ceiling,
and the WinForms functional-color painter separately caps a function body at 80
characters. Comma syntax, multiple slashes, nested functions, and missing
whiteness/blackness percent signs reject the color.

## Shared paths

HWB uses the same compact records and cascade behavior as named, hex, RGB/RGBA,
HSL/HSLA, transparent, and `currentColor` values. Covered paths include:

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

This milestone does not add comma-separated HWB, the CSS `none` channel keyword,
relative colors, `calc()` or other channel expressions, `color-mix()`, `lab()`,
`lch()`, `oklab()`, `oklch()`, wide-gamut `color()`, or color-profile
management. Lea's compact page-script `bg` and `fg` commands keep their existing
command-specific validation; this milestone covers authored CSS and the shared
style paths that already use the bounded CSS color parser.

## Verification

`test-pages/hwb-colors.html` covers every hue unit, wrapping, mixed channels,
gray normalization, clamping, numeric and percentage alpha, malformed atomic
fallback, shadows, gradients, text, generated content, variables, layers,
media, `@supports`, fields, and SVG. `Lea.Tests.exe . --hwb` checks retained
model state and exact bitmap paint. The benchmark and compiled Firespider host
repeat integration-level model, paint, resize, and field-retention checks.
