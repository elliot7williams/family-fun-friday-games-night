# Current Hit Geometry

2026-10-26 current validation: one full native execution now passes with the
title-independent overlap setup,121914ms/exit0. Exact footer/input/binary audit
89AD895C and read-only3-grant attributionE552F8F0; raw audit still false.
Fresh18 title/native-edit/720+420 visual checks also pass on this DLL96278DC0,
with both PNGs inspected. Prior failed runs below remain retained evidence.
Actual matched Firespider/script qualification and startup reliability remain open.

## Reproduction

The isolated keyboard fixture reproduced the V7 focus regression with core52CE
and viewFF99. After button activation and keyboard traversal to `priority-generic`,
the visible focus bounds are X20/Y154/W268/H24 at scroll97. `BoxAt` and `FocusAt`
identify that generic region, but the paint-built `HitAt` still identifies
`priority-one`. Clicking clears focus and can activate a stale link. Explicitly
painting first removes the stale hit and retains the expected focus.

Retained baseline: `continuity-evidence/focus-click-probe-v2/result.json`, SHA256
`17B690D66F4D5490819FC2DAF1E3F29367495A221C636CAE47BF0A73C54C0E04`.
This diagnostic reports the failure; its process success is not a baseline fix.

## Candidate

`Dirty` and changed scroll positions invalidate hit geometry. `HitAt` refreshes
stale geometry through shared `PaintDocument`, without invoking external Paint
event subscribers. Normal paint uses that same path. Link, field, map, vector,
clipping and stacking hit production retain the existing rendering rules.
The change is applied to root V.cs and by an inverse-checked overlay to the
mature source; root and mature source remain distinct.

The refresh uses synchronous rendering when input precedes queued painting.
It avoids a second hit-testing implementation, but can duplicate pending paint
work. Broad latency/allocation cost has not been measured; no speed claim.

Candidate view SHA256:
`74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC`.
Focused receipt `current-hit-geometry-v3/result.json`, SHA256
`7EF54C6150CB19A89AF32A04E756C95F8C0D2EC411711B010B3AC8D794C523E0`:
repaint/no-repaint focus, direct click without diagnostic pre-query, no stale-link
navigation, and direct current-link activation pass. Inputs and runtime bytes
held. Full native/host/script/security/delivery gates remain separate.

Full `current-hit-full-regression-v2` subsequently passes the repaired focus
assertion, then fails later nested-positioning exact overlap (escapeY139 vs
coverY190). Exit1/121465ms, no outer timeout/kill, inputs/runtime bytes unchanged,
owner retired cleanly. Receipt SHA256
`EED30044BB4C5735CEE6999A31961BBC7BB7C1123A237DE4989DE3F71C8FFA3D`.
The overlap fixture mixes flow placement with an absolute top135px; investigate
the title-gap-dependent setup without weakening overlap or stacking assertions.
No full pass is claimed.

PowerShell updated externally to7.6.9 during this work. The old compiler pin
correctly rejected changed Roslyn bytes. A separate20261026 wrapper pins the
new valid Microsoft-signed compiler files; old recipes and failure are preserved.
