# Firespider Event Constructor Integration

Milestone 146 remains open. Broader lightweight-browser readiness stays about
65%; this is an experimental integration checkpoint, not a release or a startup
reliability fix.

## Host Change

The first-demand and inline-navigation event receiver checks now also require
the shared event methods, their bound forms, and all ten handler accessors to
be non-constructible. Checks cover the absence of a constructor prototype,
`Reflect.construct` new-target rejection, direct construction rejection, and
no argument conversion. Existing receiver ordering, metadata, document mutation
and realm identity checks remain in place.

Only the host test source changes here. The production files are exactly the
preceding tested event-constructor checkpoint documented in
`EVENT_CONSTRUCTORS.md`. Public aliases, security policy and execution/startup
budgets are unchanged.

## Verification

- A fresh net48 candidate builds from 67 unchanged original Firespider inputs
  and 25 declared overlays: 90 unique final build inputs. The installed browser
  and original source are not modified.
- The final host harness is compiled once. Its SHA256 is
  `5A5611E4AE6FB089B00BA0B322E884BC4FF80A3E4220C51864C4E57B571D4E11`.
- With the identical host and harness, the preceding production dependencies
  fail the first-demand `host event constructor prototype` assertion, after
  successful startup. This is the intended negative control, not a timeout.
  All 19 preceding files are checked unchanged, then the current 19 files are
  restored and verified before positive testing. No failed runtime gate is
  retried.
- One ordinary, unobserved full candidate run passes all 43 ordered host cases
  in 415,868 ms, from `2026-10-09T08:08:26.8509683Z`. Both event-constructor realm
  checks, existing Element receiver checks, borrowed native dispatch, field
  retention/fixture cleanup, script recovery, split isolation and disposal pass.
  First-demand and split-load waits are 3,888 and 4,420 ms respectively. These
  are outer host measurements, not native worker startup or cold-cache timings.
- The actual browser EXE passes clean, restored, locked, oversized,
  lazy-background and lazy-all startup scenarios once, with eight-second
  observation periods. Windows remain responsive; no checked startup error
  dialogs or MSHTML loading are observed. Lazy-all Resume passes. These startup
  pages can leave scripting dormant, so this does not prove worker reliability.
- Fresh main/split script captures are timestamp-checked and visually inspected:
  the counter, fields and independent script realms paint correctly. Captures
  are not inherited from an earlier run.
- All 377 native runtime files are checked unchanged after testing and frozen
  in evidence. The 19 production files match in six roots: SDK browser/tests,
  candidate dependencies/output, native candidate, and frozen tested runtime.
  The original 67 host inputs and three installed binaries remain unchanged.

## Artifacts And Limits

Native experimental browser:
`C:/Users/Elliot/AppData/Local/Lea/HostDiagnostics/Lea-146-event-constructors-host-v31/Firespider Internet 19.exe`.
Its SHA256 is
`D271339A6068B83BF0550EEAB7F6745F44F67AB89D9BA19DC4E5DF6B3E3D2EC1`.
Keep the runtime folder together; the EXE is not a self-contained distribution.

Build source/output:
`H:/Lea Builds/Lea-146-event-constructors-host-v31`.
Evidence:
`H:/Lea Evidence/Lea-146-host-event-constructors-2026-10-09`.
Evidence retains source, the exact tested runtime, logs, failed negative control,
captures, byte inventories, source-backup verification and the Git checkpoint.

No new release ZIP, package extraction, independently rebuilt runtime gate,
full engine/source-suite rerun, size reduction, speedup, startup-timeout fix or
installed replacement is claimed. The preceding source suite's 1,391 checks
remain preceding evidence, not checks rerun here. The earlier independent
ordinary worker-startup failure remains failed. Safe internet-page scripting,
broader site compatibility and legacy-only host commands remain unfinished.
