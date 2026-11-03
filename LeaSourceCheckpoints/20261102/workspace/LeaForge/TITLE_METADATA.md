# Document Title Metadata

2026-10-26 combined candidate74956DA3/core52CE now passes the full native
execution after title-independent stacking test setup, plus18 freshly executed
title/native-edit/empty-page/720+420 checks (receipt96278DC0). Both fresh PNGs
inspected. Full receiptE60A49F4/raw permission audit false;3 exact fixture grants
read-only classifiedE552F8F0, exact footer/binding observation89AD895C. Prior
FF99 results remain historical. No current host/script or release certification.

2026-10-26: Subsequent focus regression is reproduced and fixed in a candidate;
see [Current Hit Geometry](HIT_GEOMETRY.md). Focused tests pass, broader native
regression remains pending. Original title-only results below apply to FF99,
not automatically to the newer combined candidate.

2026-10-25 candidate only; not installed or release qualified.

The native view previously inserted a synthetic #title box and vertical gap
before every document. HTML title metadata belongs to the browser chrome, not
the authored document flow. V.LayCore now starts with authored content instead.
The removal is inverse-checked on the mature view source; root V.cs has the same
scoped removal, without replacing its unrelated work. Engine parsing, P.T/V.T,
DOM nodes and decoder policies are unchanged. Six compiled view sources retained.

Core52CE235E paired with new view
`FF99C552ABD3AEFB2F6DD12ECF83A3B7EAF2A3FDD392571B286F9F804D2939EC`:
`title-metadata-visuals-v1/result.json`, SHA256
`04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4`,
passes18 checks. Old viewB2C1 fails the synthetic-title negative control. Tests
cover metadata retention, authored heading/order/no extra gap at720/420px,
nonblank output, native focus/activation/edit retention, navigation to new title
metadata and a visibly empty metadata-only document. Both PNGs were inspected;
heading, paragraph and field are legible, contained and not overlapping.
Scoped visual-review.json approves only this fixture, not the whole browser.
Input/runtime bytes and ACLs held; foreground owner retired cleanly.
Both old/new DLL sizes remain323584bytes due to PE layout; no size/speed claim.

The unchanged full engine/view suite passed the parser/model section, then FAILED
at the first title-inclusive renderer count: richer widget layout boxes expected12,
got11. Raw receipt `title-metadata-regression-v1/result.json`, SHA256
`118AD7849EC4FF4920EAB10266AC37F45F626A2DE4E6C9F13D57BB69A31B754D`.
Child exit1/no outer timeout/kill, original300s budget and corrected terminal
footer check. Inputs/runtime bytes held, raw fixture ACL deltas retained;
foreground owner retired0/zero leases/unverified=false. This is consistent with
removing one synthetic box, but does not qualify later widget/paint assertions.
Next review title-inclusive explicit and shared box-count contracts, preserve
all authored-widget and geometry assertions, and require absence of synthetic
metadata boxes before updating the representation expectations. Do not merely
subtract counts to make unrelated missing-widget defects pass.
Older B2C1 full/host/script results do not transfer to FF99. Scripting bootstrap
intermittency and native decoder startup failures remain separate open issues.

The parallel startup investigation's18 synthetic deep-frame controls all pass:
`framework-deep-guard-cost-v1/result.json`, SHA256
`379BC440CFB2A631188BC805B24C15AA43081FF5F0B24FC8247B1756CCBF255A`.
Two rounds, depths0/64/128, padded managed frames,10000 empty/stack/allocation
calls each. All rows round below1ms and CPU counter resolution; stack rows have
one total fault, allocation rows two; first empty row115faults. No interpreter
executed or security setting changed. This does not reproduce or explain the
earlier61k-fault bootstrap observation, or measure its actual stack shape.
No guard change or causal performance fix adopted.

Verified local source/evidence archive `title-metadata-source-v1.zip`:
56entries/7297683bytes, SHA256
`9FA3619F328E4B26E5CBFCF929D3136CF7D63C444B2FB51A65E2EBA7623791F1`.
Includes six exact compiled view sources, controls, screenshots/review, full
failure and nested prior core/host sources. Not portable/off-device or release
certification. All required foreground owners have retired.

## Focus Regression Investigation (2026-10-26)

V2/V4 migrated stale synthetic-title counts and numeric geometry offsets to
authored-box/model identity contracts; dimension and widget checks are retained.
V5/V7 then fail with NullReferenceException. V6 was a diagnostic-anchor failure,
not an executed renderer test. V7 receipt SHA256:
`E2481D87900FE5BFA3D90E579E615C55EC1D8E652AF0B3203D49640B707DAA6F`.
`Audit-RendererFailureLocation.ps1` pins the failed test binary to that receipt
and decodes Main token100663299/IL213717. The failure follows
`Click(view, view.Kb)` and dereferences null `view.Ke` in the generic-tabindex
focus-retention assertion. Earlier keyboard focus reached the expected target.
The attribute-state checkpoint was not the fault location. Root cause remains
unproven; V8 adds bounded focus/hit-test observations without changing assertions,
the engine, isolation or deadlines. No full-suite or release pass claimed.
V8 diagnostic outcome (2026-10-26): compilation passed, but full execution failed
before renderer at native decoder termination, 6014ms, If1/Ik0/In0. The focus
probe was not reached. Inputs/runtime bytes held; owner retired exit0 with zero
leases and no unverified child. Receipt SHA256
`CD615712B42B5AFBA14E2271E42878ED241574ECAA800C42154B052EF89F40AD`.
Next isolate keyboard reproduction for diagnosis; full gates remain required.

