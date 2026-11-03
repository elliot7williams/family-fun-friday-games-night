# Interaction Media Queries

Milestone 134 adds the Media Queries Level 4 interaction features to Lea's
existing bounded media compiler. CSS, `<style media>`, compact `E/V.Mq`, and
literal global or `window.matchMedia(...).matches` guards all use the same
compiled conditions.

## Compact Host State

```csharp
engine.Pt = 'f';                 // primary pointer: n, c, or f
engine.Hv = true;                // primary pointer can hover
engine.Ap = 'b';                 // any pointer: n, c, f, or b (both)
engine.Ah = true;                // at least one pointer can hover
engine.Mi('c', false, 'b', true); // atomic media-input update
```

`Pt`, `Hv`, `Ap`, and `Ah` are available on `E`, `P`, `G`, and `V`. `Mi` is the
preferred two-letter update when more than one capability changes because it
validates and applies the four values in one responsive rebuild. Invalid
pointer characters reject the complete update.

Headless `E` defaults to a conventional desktop state: fine primary pointer,
primary hover, any fine pointer, and any hover. A WinForms `V` conservatively
samples Windows mouse, touch, and pen metrics until the host writes one of the
interaction aliases or calls `Mi`. Device-change messages refresh automatic
state. Mouse input is fine and hover-capable, touch is coarse, and detected pen
input contributes a fine pointer; hosts can override the complete state when
their product has better knowledge of the user's primary device.

## Supported Queries

- `pointer: none | coarse | fine` and boolean `(pointer)`;
- `hover: none | hover` and boolean `(hover)`;
- `any-pointer: none | coarse | fine` and boolean `(any-pointer)`;
- `any-hover: none | hover` and boolean `(any-hover)`;
- conjunctions, comma alternatives, whole-query `not`, nested `@media`, and
  `<style media>` through the existing media limits.

`pointer` and `hover` describe the host-selected primary pointing device.
`any-pointer` and `any-hover` describe the union of available devices. `Ap='b'`
therefore matches both `(any-pointer: coarse)` and `(any-pointer: fine)`.
`any-pointer: none` matches only `Ap='n'`; its boolean form is true for `c`,
`f`, or `b`. Hover boolean forms are true only for the `hover` state.

Conflicting primary-pointer or hover conjunctions stay inactive. A nested or
flat conjunction of `any-pointer: coarse` and `any-pointer: fine` compiles to
the exact hybrid requirement `@ap=b`. Unknown values, range syntax, min/max
prefixes, and malformed conditions remain inactive.

## Rebuilds And Diagnostics

Interaction changes select from existing compiled rules without reparsing CSS.
Mutable `Fi` objects, edited values, and inline command styles survive the
responsive rebuild. Diagnostics use:

```text
@pt=f   primary fine pointer
@hv=h   primary hover
@ap=b   at least one coarse and one fine pointer
@ah=h   at least one hover-capable pointer
```

The feature does not identify individual devices, expose device counts, infer
keyboard availability, or add event listeners or a general JavaScript runtime.
Automatic platform detection is currently Windows-only; other hosts set the
compact aliases directly.

Coverage lives in `test-pages/media-interaction.html` and the focused
`Lea.Tests.exe . --media-interaction` suite. The compatibility runner and
compiled Firespider host smoke cover desktop, touch-first hybrid, no-pointer,
boolean, exact, nested, malformed, diagnostic, retained-field, script-guard,
automatic-host, and exact WinForms geometry paths.

The semantics follow Media Queries Level 4 interaction features:
<https://www.w3.org/TR/mediaqueries-4/#interaction>.
