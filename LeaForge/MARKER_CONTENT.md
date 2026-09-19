# Bounded Marker Generated Content

Milestone 105 extends Lea's terminal `::marker` slice with a bounded CSS
generated-content list. The result still occupies the existing `S.Mc` string
and one marker layout atom, so this compatibility gain adds no document node,
public model type, resource loader, or second layout pass.

## Content Grammar

Marker `content` accepts one to eight values in source order:

- a quoted CSS string
- `attr(name)`
- `attr(name raw-string)`
- `attr(name, "fallback")`
- `attr(name raw-string, "fallback")`

Whitespace between values separates CSS tokens and does not become generated
text. Adjacent quoted values are accepted. Each attribute name is a bounded
unnamespaced identifier of at most 64 characters. An existing attribute
contributes its literal value without CSS parsing, including an existing empty
value. A missing attribute contributes the optional decoded quoted fallback,
or an empty string when no fallback is supplied.

```css
li::marker { content: "[" attr(data-code) "] "; }
li.notice::marker { content: attr(aria-label raw-string); }
li.pending::marker { content: attr(data-state, "Pending "); }
```

The concatenated result is capped at 128 UTF-16 code units. If a declaration
has an unsupported item, more than eight items, malformed quoting, unsafe
control input, or oversized output, that declaration is ignored and the prior
cascade value remains. CSS variables are resolved before this grammar is
checked, so quoted fragments stored in custom properties compose normally.

## Cascade And Commands

Generated content is resolved against the originating list item after marker
custom properties and cascade priority are known. This applies independently
to authored rules, hover/media rebuilds, a page marker default, and direct item
commands. The existing defining alias is enough:

```text
ms("content:attr(data-code)")                         page default
ms("item", "content:attr(data-label raw-string)")   one item
markerstyle("item", "content:'[' attr(data-id) '] '")
```

The command validator rejects typed attributes, unquoted or functional
fallbacks, extra arguments, and malformed forms before replacing a prior
direct marker style. No DOM evaluator or JavaScript runtime is involved.
Attribute and fallback values become marker text only; markup-looking text is
never tokenized as HTML and cannot create nodes or requests.

## Layout And Paint

The generated result follows the same priority and geometry as existing marker
text. A non-null generated string takes priority over `list-style-image` and
`list-style-type`. `content:none` still removes the marker, while `normal`
restores the normal image/type path. Outside, inside, LTR, RTL, table, font,
spacing, shadow, opacity, hover, and responsive behavior remain in Lea's one
marker atom.

## Verification

`test-pages/marker-content.html`, focused `Lea.Tests --marker-content`, the
full default suite, the benchmark, and the compiled Firespider host cover raw
and explicit raw-string attributes, missing quoted fallbacks, present and empty
priority, ordered/adjacent/escaped/variable fragments, important priority,
invalid preservation, exact count/name/output bounds, markup-like text, image
precedence, compact/readable commands, page defaults, retained fields, tables,
LTR/RTL inside/outside geometry, hover, responsive rebuilds, and paint.

## Boundary

This slice does not implement typed `attr()` values, unquoted or functional
fallbacks, more than one fallback, or namespaced/escaped attribute names.
Bounded named and implicit
`counter()`/`counters()` output is documented in `MARKER_COUNTERS.md` and
`CSS_COUNTERS.md`; reversed/custom counter styles remain unsupported, as do quote
keywords, images or alternative text in `content`, separate anonymous fragment
boxes, and a general generated-content/CSSOM evaluator. Before/after content
shares this grammar; see `GENERATED_CONTENT.md` and `ATTR_FALLBACKS.md`.

References: [CSS Generated Content Level 3](https://drafts.csswg.org/css-content-3/#content-property)
and [CSS Values Level 5 `attr()`](https://drafts.csswg.org/css-values-5/#attr-notation).
