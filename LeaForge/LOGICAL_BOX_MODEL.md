# Lea Logical Box Model

Milestone 84 / runtime 184 adds bounded logical CSS margins and padding to
Lea's horizontal left-to-right layout. All properties compile into the existing
eight integer box-edge fields, so the feature adds no parallel layout model or
persistent per-element state.

## Properties

The supported axis shorthands are:

```text
margin-inline   -> margin-left / margin-right
margin-block    -> margin-top / margin-bottom
padding-inline  -> padding-left / padding-right
padding-block   -> padding-top / padding-bottom
```

One value applies to both ends. Two values apply to start then end. Lea also
supports all eight logical longhands:

```text
margin-inline-start   -> margin-left
margin-inline-end     -> margin-right
margin-block-start    -> margin-top
margin-block-end      -> margin-bottom
padding-inline-start  -> padding-left
padding-inline-end    -> padding-right
padding-block-start   -> padding-top
padding-block-end     -> padding-bottom
```

Physical and logical declarations share normal source order and the existing
normal/important cascade because they write the same `S.Mt/Mr/Mb/Ml` and
`S.Pt/Pr/Pb/Pl` records. Custom properties, `inherit`, `initial`, and `unset`
resolve before those writes. Axis shorthands are atomic: an invalid token or a
third value preserves both prior edges. Physical margin/padding shorthands and
longhands now use the same atomic parser.

## Values

Margins accept fixed signed integer pixels from -2,000 through 2,000. Padding
accepts fixed non-negative integer pixels through 2,000. Unitless integers and
the `px` suffix are accepted. Four-digit authored values outside the retained
range clamp to the appropriate boundary.

Percentage, viewport, font-relative, calculated, and `auto` margins/padding are
not implemented in this milestone. Negative padding is invalid. Malformed
declarations remain inert instead of silently becoming zero.

## Compact Commands

Every logical property has a defining alias of at most three letters:

```text
mi / mb / pi / pb       margin/padding inline/block axes
mis / mie / mbs / mbe   margin inline/block start/end
pis / pie / pbs / pbe   padding inline/block start/end
```

Readable forms remove punctuation, for example `margininline`,
`marginblockend`, and `paddinginlinestart`. Literal allow-listed DOM assignments
such as `document.getElementById("item").style.marginInline = "8px 12px"`
translate to the same compact commands. Assigning an empty string removes that
logical inline declaration and restores the authored cascade.

## Shared Geometry

The WinForms renderer already reads the eight physical records for normal,
relative, absolute, fixed, flex, grid, table, form, paint, hit, editor, and
accessibility geometry. Logical declarations therefore reach every supported
layout family without a renderer branch. Negative margins participate in the
same bounded integer calculations; padding remains part of the content/border
box and clipping paths.

Lea still has one horizontal left-to-right axis model. `direction`, bidi
reordering, vertical writing modes, and writing-mode-dependent remapping remain
explicitly unsupported. A future direction milestone can remap the same eight
records without changing host integration.

## Verification

`test-pages/logical-box-model.html` covers all twelve properties, one/two-value
axes, physical/logical source order, important declarations, custom properties,
inheritance, resets, malformed atomic fallback, signed bounds, all twelve short
and readable commands, all twelve familiar DOM properties, authored restore,
and exact content/outer geometry.

The Release engine, WinForms view, compiled Firespider host, all 42 Boot cases,
and all six real EXE startup scenarios pass. The report covers 80/80 executable
probes over 77 fixtures. Its 15-run medians are 555.956 ms clean diagnostic
startup, 3.405 ms local load, and 34.165 ms layout/paint. Retained managed
memory is 19,652.8 KiB for 385 pages, or 51.0 KiB/page. Required runtime files
total 558.5 KiB with a 237.6 KiB GZip estimate. These are local comparison
samples, not performance promises; see `COMPATIBILITY_PERFORMANCE.md`.
