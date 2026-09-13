# Bounded Marker Styling

Milestone 104 adds a bounded `::marker` pseudo-element slice without adding a
second document node, layout pass, script runtime, or image loader. Each list
item block keeps its originating style in `B.S` and its compact marker style in
`B.Ms`. `S.Mc` stores optional generated marker content and `S.Mn` records
`content:none` without overloading an empty string.

## Selectors And Cascade

Lea accepts `::marker` only as the terminal pseudo-element of an otherwise
supported selector. The selector matches the originating list item, and the
pseudo-element contributes one type-selector point of specificity. Universal,
type, class, ID, descendant, child, adjacent, state, variable, important, media,
and source-order behavior reuse the existing compiled CSS path. Marker rules
never change the originating item style and document `Q/Qa` queries do not
return pseudo-elements.

```css
li::marker { color: #2563eb; font-weight: bold; }
#tasks > li::marker { content: "Lea "; }
#active:hover::marker { color: #dc2626; }
```

The marker starts from the list item's inherited text state. Lea then applies
the marker defaults `text-transform:none` and preformatted, non-wrapping marker
whitespace before the pseudo cascade. Explicit `text-transform:inherit` can
restore the originating item's transform.

## Supported Properties

The bounded marker declaration set is:

- `color`
- `font-family`, `font-size`, `font-weight`, and `font-style`
- `text-transform`, `letter-spacing`, `word-spacing`, and `line-height`
- one bounded `text-shadow` layer
- `direction`
- `content`

`content` accepts `normal`, `none`, CSS-wide values, or up to eight quoted
strings, `attr(name)`/`attr(name raw-string)`, and named or implicit
`counter()`/`counters()` values. CSS escapes are decoded to Unicode scalar
values; invalid scalar values become the replacement character. Attribute
names are capped at 64 characters, nested counters at eight levels, separators
at 32 units, and the concatenated string at 128 UTF-16 units. See
`MARKER_CONTENT.md`, `MARKER_COUNTERS.md`, and `CSS_COUNTERS.md`.
`normal` uses the ordinary marker source, so a decoded `list-style-image` wins
over `list-style-type`. A literal string wins over the image. `none` suppresses
the marker and its spacer while retaining the list item.

## Commands

The defining marker command is `ms`; `markerstyle` is its readable form:

```text
ms("color:#2563eb;font-weight:bold")             page marker default
ms("item", "content:'Lea ';font-size:22px")     one item marker
markerstyle("item", "color:#dc2626")            readable form
ms("item", "")                                  restore authored CSS
```

The command block is limited to 1,024 characters and 16 declarations. Unknown
properties, malformed blocks, excess declarations, and control input are
rejected atomically. Custom properties are accepted within Lea's existing
limits and may feed marker values through `var()`. A page default is applied
before authored pseudo rules; a direct item command is the marker's inline
override. Every accepted command uses the retained rebuild and preserves fields,
history, document identity, and request state.

## Layout And Paint

Text markers reuse the existing inline atom, font cache, spacing, line-height,
shadow, color, and opacity paths. Outside markers occupy the measured logical
start gutter without reducing principal line width. Inside markers are the first
inline atom and affect line height and wrapping. Horizontal LTR and RTL share
the existing direction-aware geometry. Marker images keep their existing
sandboxed decode, 64-pixel intrinsic cap, cache, and text fallback behavior.

## Verification

`test-pages/marker.html`, focused `Lea.Tests --marker`, the full default suite,
the 103-probe benchmark over 98 fixtures, and the compiled Firespider host cover
style isolation, specificity, variables, priority, CSS-wide behavior, escaped
content, image precedence, compact/readable commands, validation bounds,
tables, LTR/RTL inside/outside geometry, exact paint, hover, responsive rebuilds,
and retained controls. All 18 host startup regressions, all 42 Boot cases, and
all six actual EXE startup scenarios pass without MSHTML, an invalid cast, a JIT
dialog, or a dead startup window.

## Boundary

Lea does not yet implement typed/fallback `attr()` values, reversed/custom
counter styles, pseudo-element counter properties, images or quote keywords
in `content`, separate anonymous fragment boxes, `marker-side`, cross-item marker
column alignment, author-controlled marker image sizing, SVG image decoding,
vertical writing, full Unicode bidi shaping, pseudo-element DOM objects, or a
general CSSOM/JavaScript evaluator.

References: [CSS Pseudo-Elements Level 4](https://drafts.csswg.org/css-pseudo/#marker-pseudo)
and [CSS Lists and Counters](https://drafts.csswg.org/css-lists/#marker-properties).
