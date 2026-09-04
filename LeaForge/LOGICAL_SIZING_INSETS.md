# Lea Logical Sizing And Insets

Milestone 83 / runtime 183 adds bounded logical CSS sizing and positioned
insets to Lea's current horizontal left-to-right layout. The implementation
maps familiar property names onto the existing compact width, height, and edge
records, so it adds no parallel layout model or persistent per-element state.

## Sizing

The supported logical sizing properties are:

```text
inline-size      -> width
min-inline-size  -> min-width
max-inline-size  -> max-width
block-size       -> height
min-block-size   -> min-height
max-block-size   -> max-height
```

They accept the same bounded values as their physical counterparts: fixed
pixels, percentages, `vw`/`vh`/`vmin`/`vmax`, `em`/`rem`, one
`calc(PU +/- Npx)`, two/three-term `min()` or `max()`, three-term `clamp()`,
custom properties, explicit inheritance, resets, and important declarations.
Physical and logical declarations share normal source order because both write
the same `S.W/Wn/Wx` or `S.H/Hn/Hx` records and their existing lazy tuples.

The bounded page-command bridge adds defining short aliases:

```text
is("id", "50%")   inline size; writes the existing wd/width path
bs("id", "120px") block size; writes the existing ht/height path
```

`inlinesize` and `blocksize` are readable long forms. Literal allow-listed
`document.getElementById(...).style.inlineSize` and `.style.blockSize`
assignments map to the same commands. An empty DOM assignment removes the
corresponding physical inline style and restores the authored cascade.

## Insets

The supported logical positioned properties are:

```text
inset-inline-start  -> left
inset-inline-end    -> right
inset-block-start   -> top
inset-block-end     -> bottom
inset-inline        -> left/right
inset-block         -> top/bottom
```

Each axis shorthand accepts one or two values. One value applies to both ends;
two values apply to start then end. Parsing is atomic, so an invalid token or a
third value preserves both previous edges. Values reuse Lea's signed integer
pixel/`auto` offset parser and its existing `S.Tp/Rt/Bm/Lt` records. Opposing
logical insets therefore use the same absolute/fixed stretch geometry,
clipping, paint, hits, editor placement, and accessibility bounds as physical
insets.

## Deliberate Boundary

Lea currently lays out horizontal left-to-right content. Logical mappings are
therefore fixed to inline start/left, inline end/right, block start/top, and
block end/bottom. `direction`, bidirectional reordering, vertical writing
modes, and writing-mode-dependent remapping are not implemented. Unsupported
or malformed declarations remain inert rather than selecting a guessed axis.

This scope gives common modern stylesheets useful compatibility while keeping
the engine small. A later writing-mode milestone can place direction state in
front of the same compact records without duplicating the dimension resolver.

## Verification

`test-pages/logical-sizing-insets.html` covers all six size properties, both
axis shorthands, four inset longhands, source order, important declarations,
custom properties, explicit inheritance, resets, atomic invalid values,
`is/bs`, readable aliases, familiar DOM assignments, and live geometry. The
repository core suite and compiled Firespider smoke exercise the same model,
including minimum-size clamping in the real host.

The Release engine, WinForms view, compiled Firespider host, all 42 Boot cases,
and all six real EXE startup scenarios pass. The report covers 79/79 executable
probes over 76 fixtures. Its 15-run medians are 554.307 ms clean diagnostic
startup, 3.598 ms local load, and 33.759 ms layout/paint. Retained managed
memory is 19,314.1 KiB for 380 pages, or 50.8 KiB/page. Required runtime files
total 555.0 KiB with a 236.4 KiB GZip estimate. These are local comparison
samples, not performance promises; see `COMPATIBILITY_PERFORMANCE.md`.
