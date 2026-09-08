# Lea Text Shadow

Lea supports a bounded inherited subset of CSS `text-shadow` for block, inline,
and form-control text. The computed style stays compact:

- `S.Ts` is null when no text shadow is active.
- A present `Sw` reuses `X`, `Y`, `B`, and `C` for horizontal offset, vertical
  offset, blur radius, and optional color.

Pages without text shadows retain the existing plain-text fast path and do not
allocate a shadow record.

## Accepted Values

Lea accepts one shadow containing two signed integer pixel offsets, an optional
nonnegative integer pixel blur radius, and an optional bounded CSS color. The
color may appear before or after the lengths. Unitless integer lengths are
accepted by the compact command path as pixels.

- offsets: `-128px` through `128px`
- blur: `0px` through `32px`
- source text: at most 256 characters
- color: existing bounded named, hex, `rgb()`, or `rgba()` forms

`none` and `initial` clear the shadow. `inherit`, `unset`, `revert`, and
`revert-layer` use the inherited shadow. Malformed, over-limit, comma-separated,
inset, spread, fractional, and relative-length values are rejected without
replacing the previous valid declaration.

Custom properties, normal and important tiers, inline declarations, inline
overrides, hover/focus rebuilds, and media queries use the ordinary compiled
cascade.

## Painting

The WinForms renderer paints a zero-blur shadow once at its offset. A blurred
shadow uses at most four small eight-sample rings plus its center, with no
per-paint sample-array allocation. The blur is a lightweight approximation,
not a full Gaussian filter.

An omitted color uses the element's current text color. Alpha combines with the
element opacity before painting. Shadows do not alter layout, document height,
hit rectangles, find ranges, focus bounds, or accessibility geometry. Lea only
retains its existing inline-flow records when a shadow needs them.

## Commands

The defining compact command is `ts`, with `textshadow` as its readable form:

```text
ts("3px 2px #334155")                    body shadow
ts("title", "5px 4px 0 #dc2626")       element shadow
textshadow("title", "2px 2px 6px gray") readable form
```

Familiar literal assignments use the same parser and bounds:

```text
document.body.style.textShadow = "3px 2px #334155"
document.getElementById("title").style.textShadow = "5px 4px #dc2626"
document.getElementById("title").style.textShadow = ""
```

An empty assignment removes that inline property and restores the authored
cascade. Unknown targets and invalid or over-limit values are rejected
atomically.

## Verification

`test-pages/text-shadow.html` covers basic, color-first, blurred, inherited,
reset, invalid-preservation, variable, important, inline, field, hover,
responsive, compact-command, and literal-DOM paths. Run its focused suite with:

```powershell
Lea.Tests.exe <repository-root> --text-shadow
```

The compatibility benchmark and compiled Firespider host smoke exercise the
same parser, model, renderer, commands, and responsive rebuilds.

## Boundary

This milestone does not add multiple comma-separated shadows, spread, inset,
percentage or relative lengths, floating-point lengths, native editor shadows,
SVG text shadows, CSS filters, or exact browser Gaussian blur matching.
