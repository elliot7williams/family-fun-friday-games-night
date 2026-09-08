# Lea Text Indent

Lea supports a bounded inherited subset of CSS `text-indent` for block text.
The computed style stays compact:

- `S.Ti` stores the signed integer value.
- `S.Tip` is `true` for a percentage and `false` for pixels.

The initial value is `0px`. Styles without indentation stay on Lea's existing
plain-text fast path.

## Accepted Values

Lea accepts signed integer pixels from `-2000px` through `2000px`, signed
percentages from `-100%` through `100%`, and unitless zero. A leading `+` is
allowed and normalized away. Fractional values, relative lengths, multiple
values, `hanging`, and `each-line` are rejected without replacing the previous
valid declaration.

`inherit`, `unset`, `revert`, and `revert-layer` restore the inherited compact
value. `initial` resets it to zero. Custom properties, normal and important
tiers, inline declarations, hover/focus rebuilds, and media queries use the
ordinary compiled cascade.

## Layout

Pixels apply directly. Percentages resolve against the block's settled content
width at layout time, so responsive resizing does not require another stored
dimension. The used indentation is clamped to leave at least one pixel of
first-line layout space.

Only the first visual line changes. A positive LTR value moves inline-start to
the right and reduces that line's wrapping width. A positive RTL value moves
inline-start to the left from the physical right edge. Negative values extend
the first line beyond inline-start. Later wrapped or explicit lines use the
ordinary content width.

Retained inline rectangles drive paint, links, hit testing, find highlights,
focus, MSAA, and UI Automation, so those surfaces follow the same settled
geometry without a second host-side calculation. Native field editors and SVG
text do not receive block text indentation.

## Commands

The defining compact command is `ti`, with `textindent` as its readable form:

```text
ti("24px")                         body indentation
ti("article", "18%")              element indentation
textindent("article", "-12px")    readable form
```

Familiar literal assignments use the same parser and bounds:

```text
document.body.style.textIndent = "20px"
document.getElementById("article").style.textIndent = "15%"
document.getElementById("article").style.textIndent = ""
```

An empty assignment removes that inline property and restores the authored
cascade. Unknown targets and invalid or over-limit values are rejected
atomically.

## Verification

`test-pages/text-indent.html` covers pixels, percentages, negative values,
inheritance, resets, invalid preservation, variables, important priority,
LTR/RTL geometry, wrapping, hover, responsive resize, compact commands, and
literal DOM assignments. Run its focused suite with:

```powershell
Lea.Tests.exe <repository-root> --text-indent
```

The compatibility benchmark and compiled Firespider host smoke exercise the
same parser, model, renderer, command, and resize paths.

## Boundary

This is horizontal first-line indentation, not the entire CSS Text model. Lea
does not add `hanging`, `each-line`, fractional or font-relative indentation,
vertical writing modes, Unicode bidi shaping, or native-control indentation in
this milestone.
