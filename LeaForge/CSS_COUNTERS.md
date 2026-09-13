# Bounded Named CSS Counters

Milestone 107 adds a compact named-counter slice to Lea without adding a host
control or external browser runtime. Non-inherited `counter-reset`,
`counter-increment`, and `counter-set` values compile into `S.Cr`, `S.Ci`, and
`S.Cs`. A bounded `N.Ct` snapshot carries active counter instances through one
tree-order pass before ordinary block construction.

## Grammar And Bounds

Each property accepts `none`, a CSS-wide value, or whitespace-separated
name/integer pairs:

```css
.book { counter-reset: chapter 2; }
.chapter { counter-increment: chapter; }
.appendix { counter-set: chapter 9; }
.chapter::marker { content: counter(chapter, upper-roman) ". "; }
```

Omitted integers use the CSS defaults: reset `0`, increment `1`, and set `0`.
Names are case-sensitive ASCII identifiers of at most 32 characters. One
property holds at most eight operations, one node snapshot holds at most eight
active counter instances, and values are restricted to `-999999..999999` with
arithmetic saturated at those bounds. The implicit name `list-item` stays on
Lea's existing HTML list path and is rejected by the named-counter properties.
`reversed()` and custom counter styles are not supported.

Malformed names, integers, functions, or over-bound operation lists invalidate
only that declaration. A preceding valid cascade value remains in force.
Duplicate resets and sets keep the last operation for that name; duplicate
increments add in source order.

## Scope And Output

Lea applies reset, then increment, then set for each element. Counter state
follows document tree order through parents, preceding siblings, and their last
descendants. A reset creates a nested instance; a later sibling reset replaces
the sibling's instance at that scope. `display:none` and non-rendered document
branches neither mutate counters nor pass hidden descendant mutations onward.

`counter(name)` emits the innermost value. `counters(name, separator)` emits
the active instances from outermost to innermost. Referencing an absent valid
name produces `0`. Named output supports the same ten bounded decimal,
leading-zero, alpha, Roman, bullet, and none representations as the implicit
list counter. It stays inside Lea's existing eight-item, eight-level,
32-character-separator, and 128 UTF-16-unit marker-content limits.

## Short Commands

The defining commands are one or two letters:

```text
cr("chapter 2")
cr("item", "chapter 2")
ci("item", "chapter")
cs("item", "chapter 9")
```

Readable `counterreset`, `counterincrement`, and `counterset` forms are also
accepted. The literal bridge recognizes `style.counterReset`,
`style.counterIncrement`, and `style.counterSet` on `document.body` or a
bounded `getElementById` target. Every route uses the same validator and
retained-page rebuild, so host adapters carry no counter state.

## Verification And Boundary

`test-pages/css-counters.html`, focused `Lea.Tests --css-counters`, the full
default suite, and the compiled Firespider host cover operation order, defaults,
case-sensitive names, duplicate operations, tree-order scope, self-nesting,
missing counters, exact bounds, hidden branches, formats, variables, priority,
tables, commands, hover/media rebuilds, retained fields, geometry, and paint.
All 103 benchmark probes pass over 98 fixtures. All 18 host startup regressions,
all 42 Boot cases, and all six actual EXE startup scenarios pass.

This is not the complete CSS counter model. Lea does not implement
`reversed()`, custom `@counter-style`, implicit counters for arbitrary
`display:list-item` elements, counter properties on pseudo-elements, general
`::before`/`::after` generated boxes, custom separator styling, or a general
CSSOM/JavaScript evaluator.

Reference: [CSS Lists and Counters Level 3](https://drafts.csswg.org/css-lists-3/).
