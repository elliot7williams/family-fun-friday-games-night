# Bounded Before And After Content

Milestone 108 adds a deliberately small generated-content subset to Lea's
custom renderer. Milestone 109 extends its shared attribute item with one
quoted missing-value fallback, Milestone 110 adds bounded generated quote
keywords and inherited quote pairs, and Milestone 111 adds an optional bounded
alternative list after `/`. It does not load Chromium, WebView, MSHTML, or
another browser engine.
Milestone 112 adds bounded `url(...)` bitmap fragments to the visual list.

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
- `attr(name, "fallback")` or `attr(name raw-string, "fallback")`
- `counter(name[, style])`
- `counters(name, separator[, style])`
- `open-quote`, `close-quote`, `no-open-quote`, or `no-close-quote`
- `url(...)` bitmap images

Attribute names stop at 64 characters, counter names at 32 ASCII characters,
counter nesting and active state at eight entries, separators at 32 UTF-16
units, counter values at -999999 through 999999, and final text output at 128
UTF-16 units. Images count toward the eight-value limit but not the text-unit
limit. A present attribute always wins, including a present empty value. A
missing attribute contributes the optional decoded quoted fallback, or empty
text when no fallback is supplied. Markup-looking attribute and fallback
values stay text. Invalid, typed, unquoted/functional fallback, extra-argument,
over-item, or oversized declarations do not replace the preceding valid
cascade value. `none` and `normal` suppress the run. Custom properties resolve
before this grammar is validated.

The visual list may be followed by `/` and one to eight alternative strings,
attributes, or counters producing at most 128 UTF-16 units. `S.Ma=null` means
no alternative, while an explicit empty string suppresses decorative visual
text in accessibility output. Quote keywords and images are not accepted after
the slash. See `GENERATED_ALT_TEXT.md` and `GENERATED_IMAGES.md`.

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

Generated text and image runs participate in nested inline order, links and hit testing,
semantic value ownership, simple and nested table cells, LTR/RTL layout,
painting, responsive rebuilds, and retained form identity. Accessible names
use the rendered generated text for name-from-content roles; explicit
`aria-labelledby` and `aria-label` continue to take priority. Generated-only
content appears in accessibility reading order.

Replaced content and native controls suppress these pseudo runs. Quote depth
and the inherited `quotes` property use the bounded contract in
`GENERATED_QUOTES.md`. Lea does not provide pseudo margins, padding, borders,
backgrounds, dimensions, positioning, flex/grid items, image functions or
author-controlled generated-image sizing, arbitrary CSSOM, or JavaScript
execution in this subset. Generated bitmap URLs reuse the bounded 24-source
sandboxed image path and a 64px intrinsic display cap described in
`GENERATED_IMAGES.md`.

`LeaForge/test-pages/generated-content.html`,
`LeaForge/test-pages/generated-images.html`, and the focused
`Lea.Tests.exe <root> --generated-content` mode cover syntax, cascade, counters,
quoted fallbacks, present/empty priority, limits, commands, links, tables,
accessibility, RTL, hover/media rebuilds, retention, layout, and paint. The
compiled Firespider smoke repeats the host paths without adapter-side
generated-content code. See `ATTR_FALLBACKS.md` for the shared strict attribute
grammar, `GENERATED_QUOTES.md` for quote state and limits, and
`GENERATED_ALT_TEXT.md` for accessibility alternatives.

The bounded design follows the relevant model in the
[CSS Pseudo-Elements specification](https://drafts.csswg.org/css-pseudo-4/) and
[CSS Generated Content specification](https://drafts.csswg.org/css-content/)
without claiming full compatibility.
