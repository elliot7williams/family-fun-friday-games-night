# Bounded Before And After Content

Milestone 108 adds a deliberately small generated-content subset to Lea's
custom renderer. It does not load Chromium, WebView, MSHTML, or another browser
engine.

## Selectors And Cascade

Lea accepts terminal `::before` and `::after` selectors plus the legacy
`:before` and `:after` spellings. The originating element is matched by the
existing compiled selector engine, including specificity, source order,
custom properties, `!important`, `:hover`, and supported media queries.
Unsupported double-colon pseudos are rejected.

The pseudo style inherits Lea's compact text state. Supported declarations are
`content`, color, font family/size/weight/style, text transform, letter/word
spacing, line height, text shadow, direction, and custom properties. Generated
text is an inline run; it is not an independent CSS box.

## Content Grammar

One valid `content` declaration may concatenate up to eight values:

- quoted strings
- `attr(name)` or `attr(name raw-string)`
- `counter(name[, style])`
- `counters(name, separator[, style])`

Attribute names stop at 64 characters, counter names at 32 ASCII characters,
counter nesting and active state at eight entries, separators at 32 UTF-16
units, counter values at -999999 through 999999, and final output at 128 UTF-16
units. Missing attributes become empty text. Markup-looking attribute values
stay text. Invalid, typed, fallback, over-item, or oversized declarations do
not replace the preceding valid cascade value. `none` and `normal` suppress the
run.

## Host Aliases

The defining commands are:

```text
be(style)       before style on body
be(id, style)   before style on one element
af(style)       after style on body
af(id, style)   after style on one element
```

Readable `beforestyle` and `afterstyle` commands use the same forms. An empty
style clears the command override and reveals authored CSS. Command input uses
the existing 1,024-character and 16-declaration bounds.

## Integration Behavior

Generated runs participate in nested inline order, links and hit testing,
semantic value ownership, simple and nested table cells, LTR/RTL layout,
painting, responsive rebuilds, and retained form identity. Accessible names
use the rendered generated text for name-from-content roles; explicit
`aria-labelledby` and `aria-label` continue to take priority. Generated-only
content appears in accessibility reading order.

Replaced content and native controls suppress these pseudo runs. Lea does not
provide pseudo margins, padding, borders, backgrounds, dimensions, positioning,
flex/grid items, generated images, quote keywords, alternative text, arbitrary
CSSOM, or JavaScript execution in this subset.

`LeaForge/test-pages/generated-content.html` and the focused
`Lea.Tests.exe <root> --generated-content` mode cover syntax, cascade, counters,
limits, commands, links, tables, accessibility, RTL, hover/media rebuilds,
retention, layout, and paint. The compiled Firespider smoke repeats the host
paths without adapter-side generated-content code.

The bounded design follows the relevant model in the
[CSS Pseudo-Elements specification](https://drafts.csswg.org/css-pseudo-4/) and
[CSS Generated Content specification](https://drafts.csswg.org/css-content/)
without claiming full compatibility.
