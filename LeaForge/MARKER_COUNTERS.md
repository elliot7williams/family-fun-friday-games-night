# Bounded Marker List Counters

Milestone 106 extends Lea's terminal `::marker` generated-content slice with
the implicit CSS `list-item` counter already represented by `N.Mk`. It adds no
general counter map, computed-style field, document node, alias, or layout pass.
The resolved text still occupies `S.Mc` and one existing marker atom.

## Grammar And Bounds

Marker `content` accepts these two additional item forms:

```css
li::marker { content: "(" counter(list-item, lower-roman) ") "; }
li::marker { content: counters(list-item, ".") ". "; }
```

`counter()` takes the exact implicit name `list-item` and an optional supported
style. `counters()` takes `list-item`, one quoted CSS separator, and an optional
style. An omitted style is `decimal`. Lea accepts the ten existing compact
representations through these built-in CSS names:

- `decimal` and `decimal-leading-zero`
- `lower-alpha`/`lower-latin` and `upper-alpha`/`upper-latin`
- `lower-roman` and `upper-roman`
- `disc`, `circle`, `square`, and `none`

One `counters()` item can join at most eight outermost-to-innermost list values.
Its decoded separator stops at 32 UTF-16 units. The entire declaration still
has M105's limit of eight generated-content items and 128 UTF-16 output units.
Malformed functions, unsupported names/styles, excess nesting, unsafe control
input, or oversized separators/output invalidate only that declaration, so the
preceding cascade value remains.

## Numbering

Lea resolves the current number from the same bounded HTML list pass used by
ordinary markers. This preserves its existing `ol start`, per-`li` `value`,
`reversed`, saturation, negative fallback, alpha rollover, Roman range, and
nested-list behavior. `counter()` emits only the chosen representation; it does
not add the marker style's prefix or suffix, so authored strings supply
punctuation. `counters()` joins every representation by position, including
the separators between empty `none` representations.

This milestone intentionally targets HTML `li` marker blocks and their implicit
`list-item` values. It does not add arbitrary named counters or the
`counter-reset`, `counter-set`, and `counter-increment` properties.

## Cascade And Commands

Counter items compose in source order with quoted strings, raw attributes, and
resolved custom properties. They follow terminal `::marker` specificity,
important tiers, page defaults, direct item styles, hover, and media rebuilds.
The existing compact and readable commands are sufficient:

```text
ms("content:counter(list-item)")
ms("item", "content:counter(list-item,upper-alpha)")
markerstyle("item", "content:counters(list-item,'.',lower-roman)")
```

The command validator rejects unsupported counter grammar before replacing a
prior direct marker style. The originating list value is resolved only during
the retained page rebuild, so the same page default produces the proper text
for every item without host-side numbering.

## Layout And Verification

Counter text has the same content-over-image priority, inside/outside geometry,
font and transform behavior, LTR/RTL placement, table path, and exact paint as
other generated marker text. `test-pages/marker-counters.html`, focused
`Lea.Tests --marker-counters`, the full default suite including WinForms checks, the
102-probe benchmark over 97 fixtures, and the compiled Firespider host cover all
supported styles, mixed fragments, variables, priority, `start`/`value`/
`reversed`, nested paths, empty representations, exact separator/depth/output
bounds, invalid preservation, aliases, image precedence, tables, hover/media
rebuilds, retained fields, and LTR/RTL paint. All 18 host startup regressions,
all 42 Boot cases, and all six actual EXE startup scenarios pass.

## Boundary

Lea does not yet implement arbitrary counter names or counter properties,
custom counter styles or `symbols()`, full CSS counter scope/inheritance,
non-`li` `display:list-item` markers, typed/fallback attributes, quote/image/
alternative generated content, separate fragment boxes, `::before`/`::after`,
or a general generated-content/CSSOM evaluator.

Reference: [CSS Lists and Counters Level 3](https://drafts.csswg.org/css-lists-3/#counter-functions).
