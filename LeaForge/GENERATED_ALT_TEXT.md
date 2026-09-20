# Bounded Generated Alternative Text

Milestone 111 adds the optional alternative-text half of CSS generated
content. Lea keeps the visual output small and unchanged while exposing a
separate compact string for accessibility:

```css
.notice::before { content: "[!] " / "Important: "; }
.decorative::before { content: "* " / ""; }
li::marker { content: "[NEW] " / "New item: "; }
```

## Compact Model

`S.Mc` remains the visual generated-content atom. `S.Ma` is the optional
alternative atom:

- `null` means no alternative was supplied, so accessibility uses `S.Mc`
- `""` means an explicit empty alternative, so the visual text is decorative
- a non-empty value replaces the visual generated text in accessibility output

`S.Ma` is non-inherited and exists only on the computed marker, before, or
after pseudo style. It adds no layout box, paint branch, resource, or host-side
adapter state. A declaration such as `content: "" / "Status: "` retains a
zero-width generated run so its alternative remains available to accessibility.

## Bounded Grammar

The visual list keeps Lea's existing strings, attributes, counters, and quote
keywords. A single top-level `/` may introduce an alternative list containing:

- quoted strings
- `attr(name)` or `attr(name raw-string)`
- either attribute form with one quoted missing-value fallback
- `counter(name[, style])`
- `counters(name, separator[, style])`

The visual and alternative halves each stop at eight items and 128 UTF-16
output units. Existing 64-character attribute names, 32-character ASCII
counter names, eight counter levels, 32-unit separators, and bounded counter
values still apply. Generated quote keywords are visual-only. A missing
alternative item, a second slash, an unsupported token, unsafe controls, or an
over-bound result invalidates the whole declaration and preserves the earlier
cascade value.

## Accessibility

Before and after alternatives replace only their own visual run in reading
order and name-from-content roles. Marker alternatives prefix the owning list
item's accessible name. Table-cell blocks use the same recursive accessibility
walk as ordinary blocks. Explicit `aria-labelledby` and `aria-label` remain
higher priority.

Visual text, geometry, hit testing, links, and paint continue to use `S.Mc`.
Empty alternatives therefore suppress decorative speech without hiding what is
painted.

## Commands And Tests

`be`, `af`, and `ms`, plus their readable aliases, accept the same syntax:

```text
be("status", "content:'[!] ' / 'Important: '")
af("status", "content:' done' / ' complete'")
ms("item", "content:'N ' / 'New item: '")
```

`LeaForge/test-pages/generated-alt.html` and
`Lea.Tests.exe <root> --generated-alt` cover strings, empty alternatives,
attributes and fallbacks, counters, variables, exact item/output bounds,
invalid preservation, links, explicit ARIA priority, tables, markers, commands,
hover/media rebuilds, retained fields, geometry, paint, and accessibility.
The benchmark and compiled Firespider host repeat the integration paths.

This bounded design follows the alternative-text model in
[CSS Generated Content Level 3](https://www.w3.org/TR/css-content-3/) without
claiming support for generated images, speech styling, or the full CSS content
model.
