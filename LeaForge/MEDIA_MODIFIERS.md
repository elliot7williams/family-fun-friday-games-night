# Media Query Modifiers

Milestone 133 adds bounded media-query `not` and `only` modifiers to Lea's
shared compiled media path. CSS `@media`, `<style media>`, compact `E/V.Mq`,
and literal `matchMedia(...).matches` guards therefore use one implementation.

## Supported Forms

`not` negates the result of the complete bounded query:

```css
@media not print { /* active in Lea's screen host */ }
@media not screen and (min-width: 800px) { /* active below 800px */ }
@media not (orientation: portrait) { /* active in landscape */ }
```

`only` is accepted only before a recognized media type and does not change the
result:

```css
@media only screen and (min-width: 800px) { }
```

Lea is a fixed screen renderer. `screen` and `all` match; `print` is recognized
but does not match. Consequently `not print` matches and `not screen` does not.
Comma alternatives retain OR behavior, so each modifier applies only to its
own query group.

Negated queries can be intersected with parent `@media` blocks. Lea stores
those intersections as compact bounded predicate pairs rather than attempting
to approximate the complement with width ranges. This keeps cases such as a
minimum-width parent containing a negated wider child exact during resize.

## Strict Boundary

- one optional leading `not` or `only` modifier per query group;
- `only` requires `screen`, `all`, or `print` immediately after it;
- media types must appear at the beginning of the remaining query;
- misplaced, repeated, unknown, or malformed modifiers are inactive;
- unknown media types and `speech` remain outside Lea's bounded subset;
- nested media-condition `or`, arbitrary parenthesized Boolean trees, and
  general-enclosed syntax remain unsupported;
- the existing 2,048-character, eight-alternative, and four-level media limits
  are unchanged.

Diagnostics prefix negated predicates with `!`, for example `!(@m>=800)` and
`!(@type=print)`. Nested intersections retain both sides, such as
`(@m>=300&!(@m>=800))`.

## Coverage

`test-pages/media-modifiers.html` and the focused
`Lea.Tests.exe . --media-modifiers` suite cover host matching, CSS, style media,
comma alternatives, nested intersections, script guards, malformed placement,
diagnostics, live resize, exact WinForms geometry, and retained field identity
and values. The compatibility benchmark and compiled Firespider host smoke use
the same paths.

The syntax follows Media Queries Level 4, where `not` negates an individual
media query and `only` has no effect on the result but is restricted to a media
type: <https://www.w3.org/TR/mediaqueries-4/#mq-modifiers>.
