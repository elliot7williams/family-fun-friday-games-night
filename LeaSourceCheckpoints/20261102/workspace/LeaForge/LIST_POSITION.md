# Bounded List Marker Position

Milestone 102 gives Lea's existing marker atom a real inside/outside layout
choice without adding another layout pass or Firespider control. Inherited
`S.Lp` stores one character:

| Code | CSS value | Used layout |
| --- | --- | --- |
| `o` | `outside` | marker occupies the logical-start gutter |
| `i` | `inside` | marker is the first inline box on the principal line |

`outside` is the initial value. Its marker is measured once with the list
item's existing font/style, separated by one bounded measured space, and placed
beyond the content edge. The principal content keeps its full line width. In
LTR the marker sits left of the content; in horizontal RTL it sits right of the
content. `inside` uses Lea's ordinary inline atom path, so its marker and spacer
consume first-line width and participate in wrapping. A loaded marker image uses
the same choice: outside occupies the logical-start gutter, while inside
contributes its bounded intrinsic geometry to wrapping and line height.
Markerless fallback emits no marker, spacer, or extra flow record.

## Cascade And Shorthand

CSS `list-style-position` accepts `inside`, `outside`, and the existing
CSS-wide values. It is inherited independently from `list-style-type`.
Variables, important tiers, inline declarations, hover, media replacement,
empty authored restoration, and invalid preservation use the ordinary compiled
cascade.

The bounded `list-style` shorthand accepts one type, one image, and one position
in any order. It is atomic, parenthesis-aware, and limited to 4,096 characters.
Omitted components reset to their initial values: `disc` for type, `none` for
image, and `outside` for position. Duplicate, unknown, or overfilled combinations
preserve the preceding declaration. See `LIST_IMAGE.md` for URL parsing and the
standard `none` ambiguity.

Examples:

```css
li { list-style-position: inside; }
li { list-style: upper-roman inside; }
li { list-style: outside square url("marker.png"); }
li { list-style: inside; } /* disc inside */
```

## Commands

The short commands keep each responsibility clear:

```text
lst("items", "upper-alpha")      type only
lp("items", "inside")           position only
li("items", "url('marker.png')") image only
ls("items", "upper-roman url('marker.png') inside") shorthand
liststyleposition("items", "outside") readable position
liststyle("items", "square url('marker.png') inside") readable shorthand
```

Literal `style.listStyleType`, `style.listStyleImage`,
`style.listStylePosition`, and `style.listStyle` assignments on `document.body`
or one literal ID map to those same validators. An empty literal assignment
removes that inline declaration and restores the authored cascade. Every
accepted command uses the retained rebuild, preserving fields, document
identity, history, and request state.

## Verification

`test-pages/list-position.html` and focused `Lea.Tests --list-position` cover
initial/inherited/reset state, longhand independence, shorthand order and reset,
variables, priority, invalid atomicity, none, LTR/RTL inside/outside geometry,
wrapping, tables, hover, responsive replacement, compact/readable/literal
commands, and retained fields. The benchmark and compiled Firespider host check
the same model and physical marker gutters without adapter-side geometry.

## Boundary

Lea uses one deterministic measured-space gutter rather than attempting
cross-item marker-column alignment. This milestone does not add
`::marker`, `marker-side`, custom counter styles, generated content, vertical
writing, full Unicode bidi shaping, or arbitrary CSSOM. Bounded marker images
were added in Milestone 103; see `LIST_IMAGE.md`.
Outside markers follow each list item's own horizontal direction and are clipped
when that item has non-visible overflow.

References: [CSS Lists and Counters](https://drafts.csswg.org/css-lists/) and
[CSS 2 list marker positioning](https://drafts.csswg.org/css2/#list-style).
