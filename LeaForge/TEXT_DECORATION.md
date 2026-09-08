# Lea Text Decoration

Lea supports a bounded inherited subset of CSS text decoration for block,
inline, link, table-cell, and form-control text. The compact computed style uses
four scalar aliases:

- `S.Td` stores line flags: `n`, `u`, `s`, or `us`.
- `S.Tc` stores an optional decoration color; empty means current text color.
- `S.Tds` stores style: empty solid, `d` double, `o` dotted, `a` dashed, or
  `w` wavy.
- `S.Tw` stores integer pixel thickness; zero means the font-derived default.

This adds two short strings and one integer to the existing `S` record, with no
list or per-decoration model allocation.

## Accepted Values

Lea accepts `text-decoration` as an atomic shorthand and these longhands:

```css
text-decoration: underline dashed #176b4d 2px;
text-decoration-line: underline line-through;
text-decoration-color: currentcolor;
text-decoration-style: wavy;
text-decoration-thickness: 3px;
```

Supported lines are `underline` and `line-through`. Supported styles are
`solid`, `double`, `dotted`, `dashed`, and `wavy`. Thickness is `auto`,
`from-font`, or an integer from 1 through 8 pixels; compact unitless integers
are also treated as pixels. The shorthand is limited to 256 characters and six
tokens. Line and color longhands are limited to 64 characters.

`none`, `initial`, `unset`, `revert`, and `revert-layer` reset supported state;
`inherit` retains inherited state. Custom properties, normal and important
tiers, inline declarations, hover/focus rebuilding, and media queries use the
ordinary compiled cascade. Unsupported lines such as `overline`, malformed or
duplicate components, reserved color keywords, fractional/relative lengths,
and out-of-range thicknesses are rejected atomically without replacing the
previous valid declaration.

## Painting

Current-color solid underline and strike use the existing native `Font` path.
Changing color, style, or thickness activates a small GDI painter for solid,
double, dotted, dashed, or wavy lines. It computes from the existing text
rectangle, clips to that rectangle, and allocates no point list.

Custom decoration retains inline flow when wrapping needs per-line paint. It
does not change outer box geometry, document height, hit rectangles, find
ranges, focus bounds, or accessibility geometry. Focused native editors keep
their native Windows editing presentation.

## Commands

The defining aliases are `td`, `tdl`, `tdc`, `tds`, and `tdw`:

```text
td("underline dashed #176b4d 2px")        body shorthand
td("title", "underline wavy red 3px")   element shorthand
tdl("title", "line-through")            line only
tdc("title", "#2563eb")                 color only
tds("title", "double")                  style only
tdw("title", "3px")                     thickness only
```

Readable `textdecoration`, `textdecorationline`, `textdecorationcolor`,
`textdecorationstyle`, and `textdecorationthickness` names share those routes.
Familiar literal assignments use the same parser and bounds:

```text
document.body.style.textDecoration = "underline dashed #176b4d 2px"
document.getElementById("title").style.textDecorationLine = "line-through"
document.getElementById("title").style.textDecorationColor = "#2563eb"
document.getElementById("title").style.textDecorationStyle = "double"
document.getElementById("title").style.textDecorationThickness = "3px"
```

An empty assignment removes that inline property and restores the authored
cascade. Longhand updates preserve the other three components. Unknown targets
and invalid or over-limit values return zero without mutation.

## Verification

`test-pages/text-decoration.html` covers all five line styles, combined lines,
longhands, inheritance, resets, invalid preservation, variables, priority,
inline resets, wrapping, tables, links, fields, hover, responsive CSS, compact
commands, and literal DOM assignments. Run its focused suite with:

```powershell
Lea.Tests.exe <repository-root> --text-decoration
```

The compatibility benchmark and compiled Firespider smoke exercise the same
model, parser, command, paint, and responsive paths.

## Boundary

This decoration milestone does not itself add overline, skip-ink, underline
position, percentage/relative thickness, fractional lengths, native-editor
custom lines, SVG text decoration, animation, spelling/grammar lines, or exact
browser glyph metric matching. Bounded underline offset is implemented as the
separate inherited `S.Uo/Uop` slice described in `TEXT_UNDERLINE_OFFSET.md`.
