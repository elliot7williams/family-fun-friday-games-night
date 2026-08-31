# Saved Style Startup Safety

Update: milestone 64 adds native CSS preview and saved matching styles through
`Ss/Sd/Sn`, while retaining the guards below. CSS can now apply directly or
queue for the active request; arbitrary saved JavaScript still cannot run in
Lea. See `NATIVE_STYLESHEETS.md` for the current contract. The behavior section
below records the narrower milestone 63 fix.

Milestone 63 / runtime 163 fixes a reproduced Firespider startup and timer
failure in the saved style preview path. This is a host integration fix;
Lea does not gain a legacy DOM or a second rendering engine.

## Reproduction

A saved `STYLE` automation calls `qpreviewusercontent`. Previously that method
cast the selected tab's first control directly to `WebBrowser`, even when
normal startup had created a `Lea.Win.V`. The regression reproduced:

```text
System.InvalidCastException: Unable to cast object of type 'Lea.Win.V'
to type 'System.Windows.Forms.WebBrowser'.
  at Firespider_Internet.j.qpreviewusercontent(Int32 index)
  at Firespider_Internet.j.qrunautomation(Int32 index)
  at Firespider_Internet.j.qrunautomations(Boolean startup)
  at System.Windows.Forms.Timer.OnTick(EventArgs e)
```

The `STARTUP` rule also failed inside the existing initialization catch,
skipping subsequent tab-feature initialization. The hourly rule escaped the
UI timer callback and could produce the JIT exception dialog.

## Behavior

The helper now resolves the selected tab and uses `TryCast`. Lea, paused,
empty, missing, and disposed browser controls cannot reach the legacy DOM
preview. Firespider reports that saved style/script preview requires a legacy
tab and leaves the Lea document and saved user-content rule unchanged.
No legacy tab is opened automatically and MSHTML is not loaded by this guard.
Existing explicit legacy preview behavior is unchanged.

Automation retains its existing last-run bookkeeping. An unsupported Lea
preview is skipped, not queued for later. Applying saved arbitrary styles or
JavaScript directly to Lea remains unimplemented; this patch does not claim
that those rules have been applied successfully.

## Integration And Checks

The source change is in the live host's `BrowserFeatures2.vb`. The portable
source change is `Lea.Firespider/Style-startup-host.patch`, used after the earlier
integration patches and with the matching adapter. Check the patch before
applying; do not overwrite unrelated host changes.

`Test-FirespiderBoot.ps1 -Case style-start` seeds saved STARTUP and HOURLY style
rules, verifies later initialization, then exercises the real WinForms timer
callback at a test-only 50 ms interval instead of its normal 60 seconds.
It checks full UI exception stacks, Lea/paused/empty states, style and script
preview, retained source rules, document identity, explicit status, and absence
of MSHTML. Original settings are restored in the fixture's finally block.
The Boot suite now has 36 cases, including the paste case added in this milestone.

This is a confirmed additional cast path. The old `j.tk` timer cast was already
fixed in milestone 49. The stackless intermittent recovery observation from
milestone 58 cannot be conclusively attributed to this path. Initial fragment
placement also failed intermittently during milestone 63; it remains open,
with before/after-paint diagnostics. Do not treat one passing run as proof that
either intermittent issue is resolved.

Runtime 163 is a separate test package. The installed runtime 157 files are
left untouched, and both source trees are backed up separately.
