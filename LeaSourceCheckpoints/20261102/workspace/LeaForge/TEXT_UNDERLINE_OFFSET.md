# Lea Text Underline Offset

Lea supports a bounded inherited subset of CSS `text-underline-offset` for
block, inline, link, table-cell, and form-control text. The compact computed
style uses two scalar aliases:

- `S.Uo` stores the signed integer offset.
- `S.Uop` distinguishes a percentage from a pixel value.

This adds one integer and one Boolean to the existing `S` record. Pages that
use the default automatic offset retain the native font-decoration fast path
and allocate no separate decoration model.

## Accepted Values

Lea accepts:

```css
text-underline-offset: auto;
text-underline-offset: 6px;
text-underline-offset: -4px;
text-underline-offset: 20%;
```

Pixel offsets are signed integers from -32 through 32. Percentages are signed
integers from -100 through 100 and resolve against the used font size.
Unitless zero is accepted and normalized to pixels. `inherit`, `unset`,
`initial`, `revert`, and `revert-layer` use the ordinary compiled CSS-wide
paths. Empty literal DOM values remove only the inline property and restore the
authored cascade.

The property is inherited and independent of the `text-decoration` shorthand.
Custom properties, normal and important tiers, inline declarations,
hover/focus rebuilding, and media queries therefore behave like other Lea CSS
properties. Fractional values, unsupported units, multiple values, and values
outside the bounds are rejected without replacing the previous valid
declaration.

This follows the supported `auto` and length-percentage behavior from
[CSS Text Decoration Level 4](https://drafts.csswg.org/css-text-decor-4/#text-underline-offset-property)
within Lea's integer and unit bounds.

## Painting

Automatic current-color solid underlines retain the native `Font` path. A
nonzero offset selects Lea's existing lightweight custom decoration painter.
Positive values move the underline outward below its automatic position;
negative values move it inward. The final paint displacement is clamped to 64
pixels even after percentage resolution.

The painter temporarily extends its local vertical clip by at most 72 pixels
so an outward underline can reach padding or nearby visual space. Existing
ancestor overflow clips remain authoritative. Strike-only decoration ignores
underline offset, and underline movement does not alter outer layout, document
height, hit rectangles, find ranges, focus bounds, or accessibility geometry.
Focused native editors keep their Windows editing presentation.

Default links also enter the custom path when offset, color, style, or
thickness requires it. This avoids a duplicate native underline while keeping
ordinary unstyled links on the fast path.

## Commands

The defining short alias is `uo`:

```text
uo("6px")             body underline offset
uo("title", "-3px") element underline offset
uo("title", "25%")  element percentage offset
```

Readable `textunderlineoffset` accepts the same one- and two-argument forms.
Familiar literal assignments share the parser and bounds:

```text
document.body.style.textUnderlineOffset = "10%"
document.getElementById("title").style.textUnderlineOffset = "6px"
```

Unknown targets and invalid or over-limit values return zero without mutation.
The alias updates only `text-underline-offset`; it never rewrites decoration
line, color, style, or thickness.

## Verification

`test-pages/text-underline-offset.html` covers automatic, positive, negative,
and percentage offsets, inheritance, CSS-wide resets, shorthand independence,
invalid preservation, variables, priority, inline overrides, wrapping, tables,
default links, fields, hover, responsive CSS, compact/readable commands, and
literal DOM assignments. Run its focused suite with:

```powershell
Lea.Tests.exe <repository-root> --text-underline-offset
```

The compatibility benchmark and compiled Firespider smoke exercise the same
model, command, paint, default-link, and responsive paths. Milestone 96 passes
92 compatibility probes over 87 fixtures, both complete engine suites, all 42
Firespider Boot cases, and all six actual EXE startup scenarios.

## Boundary

Lea does not add fractional offsets, `em` or other relative length units,
`text-underline-position`, skip-ink controls, per-run native-editor offsets,
vertical writing modes, animation, SVG text offsets, or exact browser glyph
metric matching. This is a deliberately small compatibility slice, not full
CSS Text Decoration Level 4.
