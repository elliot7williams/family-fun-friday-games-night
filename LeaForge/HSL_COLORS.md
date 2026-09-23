# Bounded HSL Colors

Milestone 118 adds HSL and HSLA to Lea's shared bounded CSS color path. The
engine retains authored color text in its compact existing style records, and
the WinForms adapter converts that text directly to ARGB when it paints. No
new public type, command, dependency, browser runtime, or adapter-side color
implementation is required.

## Accepted syntax

Lea accepts legacy comma-separated forms:

```css
color: hsl(120, 100%, 25%);
background-color: hsla(0, 100%, 50%, .5);
```

It also accepts modern space-separated forms with an optional slash alpha:

```css
color: hsl(240 100% 50%);
background-color: hsl(240 100% 50% / 50%);
```

Hue is a finite signed decimal or exponent number. A unitless hue is degrees;
`deg`, `grad`, `rad`, and `turn` are also accepted. Hue wraps into one turn, so
negative and over-one-turn values are valid. Saturation and lightness require
percentages and clamp to 0..100%. Alpha accepts a finite number or percentage
and clamps to transparent..opaque.

Shared validated color components use the existing 64-character color ceiling,
and the WinForms functional-color painter separately caps a function body at 80
characters. Comma and slash syntax cannot be mixed, only one slash is accepted,
and nested functions or missing saturation/lightness percent signs reject the
color.

## Shared paths

HSL colors use the same existing compact records and cascade behavior as named,
hex, RGB/RGBA, transparent, and `currentColor` values. Covered paths include:

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

This milestone does not add the CSS `none` channel keyword, relative colors,
`calc()` or other expressions inside channels, wide-gamut `color()`, or
color-profile management. Bounded modern HWB, CIE Lab/LCH, Oklab/OkLCh, and
sRGB mixing are documented separately in `HWB_COLORS.md`, `LAB_COLORS.md`,
`OKLAB_COLORS.md`, and `COLOR_MIX.md`. Lea's compact page-script
`bg` and `fg` commands keep their existing command-specific validation; this
milestone covers authored CSS and all shared style paths that already use the
bounded CSS color parser.

## Verification

`test-pages/hsl-colors.html` covers both syntaxes, every hue unit, wrapping,
channel and alpha clamping, invalid atomic fallback, shadows, gradients, text,
generated content, variables, layers, media, `@supports`, fields, and SVG.
`Lea.Tests.exe . --hsl` checks retained model state and exact bitmap paint. The
benchmark and compiled Firespider host repeat the integration-level model,
paint, resize, and field-retention checks.
