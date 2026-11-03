# Bounded Generated Quotes

Milestone 110 adds CSS generated-quote state to Lea's existing marker,
`::before`, and `::after` content path. It remains part of the custom renderer;
no Chromium, WebView, MSHTML, JavaScript runtime, extra document node, or second
layout pass is introduced.

## Quote Pairs

The inherited `quotes` property accepts:

- `auto`, using Lea's fixed curly double/single pairs
- `none`, producing no visible quote strings while depth still changes
- one to eight explicit opening/closing string pairs
- supported CSS-wide values and `var()` substitution

Each decoded quote string stops at 32 UTF-16 units. Odd string counts, more
than eight pairs, controls, malformed strings, and oversized values invalidate
only that declaration, preserving the earlier cascade value. `S.Qc` exposes the
computed pair count; `S.Qo(depth)` and `S.Qx(depth)` return the bounded opening
and closing strings. Deeper valid nesting reuses the final supplied pair.

## Content Keywords

The shared eight-item, 128-unit generated-content grammar now also accepts:

```css
content: open-quote;
content: close-quote;
content: no-open-quote;
content: no-close-quote;
```

`open-quote` emits the current opening string and increments depth;
`close-quote` decrements first and emits the matching closing string. The `no-`
forms make the same depth change without output. A close at depth zero is a
safe no-op. Opening beyond eight active levels invalidates that content
declaration atomically.

Depth follows rendered document order across `::marker`, `::before`, child
content, and `::after`, including sibling elements, links, tables, variables,
hover, and responsive rebuilds. Ordinary pages without quote or counter source
skip the state prepass.

## Aliases

The defining command is `qu`; `quotes` is its readable form:

```text
qu("auto")                 body quote pairs
qu("id", "'<' '>'")       element quote pairs
quotes("id", "none")     readable element form
```

An empty command value removes the inline override. Literal
`element.style.quotes` and `document.body.style.quotes` assignments translate
to the same validator and retained rebuild.

## Verification And Boundary

`test-pages/generated-quotes.html`, focused `Lea.Tests --generated-quotes`, the
full engine suite, the benchmark, and the compiled Firespider host cover nested
and cross-sibling depth, silent changes, defaults, explicit and absent pairs,
exact bounds, invalid preservation, variables, commands, markers, links,
tables, accessibility, hover/media changes, retained fields, geometry, and
paint.

Lea does not implement locale-sensitive `auto` pairs, language-specific pair
selection, pseudo box-model or flex/grid items, pseudo-element DOM objects,
arbitrary CSSOM, or general JavaScript. Optional bounded alternative text is
documented in `GENERATED_ALT_TEXT.md`; bounded visual bitmap fragments are
documented in `GENERATED_IMAGES.md`.

Reference: [CSS Generated Content Level 3](https://drafts.csswg.org/css-content-3/#quotes).
