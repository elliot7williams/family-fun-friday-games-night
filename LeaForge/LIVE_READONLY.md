# Live Native Readonly Synchronization

A Firespider integration check exposed a real editor bug: generic attribute
writes update the field model's `Ro` state, but an already active native textbox
only received `ReadOnly` when editing started. A readonly field could therefore
remain keyboard-editable until the editor was reopened.

## Fix

`V.Commands` now synchronizes the active textbox before raising `Changed`, and
`PlaceEditor` synchronizes it during layout/placement. Each setter is conditional
on an actual state difference. The existing textbox is retained; its text,
selection and native undo availability are not deliberately reset. There is no
new public alias, allocation, forced layout or editor recreation in this fix.

Both JavaScript `setAttribute/removeAttribute` and native `V.Sa/Da` transitions
are covered. Presence semantics remain unchanged: `readonly="false"` is still
readonly. The parent model, native edit-command policy and accessibility model
already consult `Ro`; this change brings the textbox's own native input policy
into agreement. It is not an origin/sandbox/startup change or a speed claim.

## Preserved Failures

The first separate v32 host matrix passes 42 cases, including saved-style and
30-second startup/recovery timers, then fails its new live-field assertion after
successful JavaScript writes. First-demand and inline-navigation attribute
method checks pass. That host run is not a complete passing integration gate.
Its actual-EXE startup checks are held; no automatic retry is made.

The focused new regression against the exact preceding production reports
`model=True; native=False`. This independently confirms the readonly discrepancy
rather than inferring it solely from the host's combined assertion.

Revision A synchronizes placement only. JavaScript add/remove and retained
selection checks pass, then the compact alias assertion fails because command
completion has not updated the textbox yet. Revision B additionally synchronizes
`Commands` and passes all 336 focused checks. Both failed results remain intact.

The v32 runner also exposes a bookkeeping error while recording zero captures:
pipeline serialization yields null instead of an empty array. Its failed matrix
result was already recorded. The secondary error and an explicit empty capture
inventory are retained; they do not reclassify the substantive field failure.

## Scope

Only the production WinForms DLL changes from `542dd74`, growing by 512 bytes to
315,904. Engine, image worker, script module, script worker and pinned dependencies
are unchanged. The required Engine/Win/image bundle is now 745,984 bytes. There
is no compression or runtime-speed improvement claimed.

This synchronizes readonly state, not all dynamic input-type transitions or
every editor property. Full-web compatibility and safe remote-page scripting
remain unfinished. Milestone 146 remains open and broader readiness remains
about 65%.

## Final Verification

Final revision B passes all 336 focused attribute checks and the complete
ordinary 1,727-check scripting suite once. The rebuilt full native `--core`
suite passes, as do native editing/clipboard checks and the actual standalone
browser EXE lifecycle check. Its fresh capture is visually inspected. Native
test coverage includes unchanged text, selection and undo availability across
both script and compact alias readonly transitions; it does not require that
an undo entry already exists.

Two outer helper errors are preserved without rerunning the successful cases.
The editing wrapper looked for the wrong summary text despite process exit zero;
the browser wrapper incorrectly consulted `LASTEXITCODE` after `Start-Process`.
Separate corrections verify the original summaries, exit/result records and
checksums, rather than replacing those failed wrapper records.

Separate v33 then passes all 43 ordinary Firespider host cases using the exact
retained v32 harness. Three old-runtime controls fail at their intended API,
validator and readonly assertions. The new active-field assertion passes,
including its cleanup and later scripting/split recovery. Three fresh host
captures are visually inspected; see `HOST_ATTRIBUTE_EDITS.md`.
All six actual-EXE startup scenarios pass once, including deferred-tab resume,
with responsive windows and no legacy engine. A prior policy-blocked helper
never launched the browser; its diagnostic files remain separate from these
actual-case results. Production startup budgets and protections are unchanged.

These are bounded fixture checks, not proof of full website compatibility,
cold-cache speed or a fix for the previously observed worker-startup timeout.

Source regression evidence:
`H:/Lea Evidence/Lea-146-live-readonly-2026-10-09`.
Failed v32 integration evidence:
`H:/Lea Evidence/Lea-146-host-attribute-edits-2026-10-09`.
Corrected v33 integration evidence:
`H:/Lea Evidence/Lea-146-live-readonly-host-2026-10-09`.
