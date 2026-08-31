# Named Native Stylesheets

Milestone 64 / runtime 164 adds bounded, host-owned CSS to Lea's existing
parser, cascade, layout, and native painter. Firespider uses it for saved CSS
and preview automation without a legacy DOM or another browser engine.

## Compact API

```csharp
view.Ss("theme", "body{color:#24313a}#panel{width:240px}");
int sheets = view.Sn;
view.Sd("theme");
```

`E.Ss(page, key, source)` installs or replaces a named sheet; `E.Sd(page, key)`
removes it. `V.Ss/Sd` also update the native view. Both return 1 on acceptance
or removal, 0 for rejected input or a missing removal. The engine requires a
page it owns and throws for null/foreign pages. View calls return 0 with no
page or after disposal. Call view APIs on its UI thread.

`P.Sn`, `V.Sn`, and diagnostic `G.Sn` count host sheets, not authored sheets or
rules. Empty/unsupported-only CSS may be accepted and occupy one slot. This
is not a CSS syntax validator. An identical key/source is idempotent and avoids
recompilation and another layout notification.

## Cascade And Retention

Sheets follow authored styles at the ordinary author origin. Existing
specificity, inline style, and `!important` behavior still apply. Replacing a
key preserves its insertion position; removing then adding it appends it.
This follows the relevant ordering model in the
[CSS cascade specification](https://www.w3.org/TR/css-cascade-5/#cascade-sorting),
but does not implement that specification in full.

Compiled rules reuse Lea's supported selectors, custom properties, dynamic
focus/hover state, and pixel-width media queries. Inactive media rules count
against the rule limit. Relative resources use the document base URL. There
is no new import loader, CSSOM, cascade layer, or JavaScript evaluator.

Mutation keeps the document/source, page command state, history entry, and
retained form data. It does not reload the document, execute page commands,
re-run autofocus, or emit `Done`. Changed view state emits `Changed` and an
accessibility reorder notification. Scroll is retained subject to layout's
normal range clamp. Hiding the edited field closes its native editor without
discarding its value. Visible editors retain selection and one-level undo;
font changes, including media-query changes after resize, refresh the editor.
New documents do not inherit host sheets; retained history pages keep theirs.

## Bounds

- Keys: 1-32 ASCII letters, digits, `_`, or `-`; case-sensitive.
- Up to eight named sheets per retained page.
- Up to 64 KiB UTF-8 source bytes per sheet; 128 KiB across named sheets.
- Invalid Unicode, oversized replacements, and excess sheets are rejected.
- Authored and named sheets share the existing 1,024 compiled-rule ceiling.
- Parsing checks one extra rule to reject overflow without truncating a
  replacement or evicting authored/previous sheets.

Rejection leaves the previous sheet intact. These are source/compiled-rule
bounds, not a claim that all .NET strings, layout objects, or history fit in
128 KiB. Original source and compiled data are retained within these bounds.

## Firespider Behavior

Apply `Lea.Firespider/Native-styles-host.patch` after
`Style-startup-host.patch`, together with the matching adapter and DLLs.
The `fs-preview` and `fs-saved-0` through `fs-saved-6` keys are reserved for
Firespider. Explicit legacy tabs keep their original preview behavior.

After document completion, up to seven enabled matching saved styles are
installed. HTTP/HTTPS DNS patterns match exact hosts and dot-delimited
subdomains with IDN normalization; IP addresses require equality. Disabled,
nonmatching, and saved arbitrary JavaScript entries are not executed by Lea.
Source settings are not rewritten. Private views remain private.

The scan considers at most 256 records and 1 MiB of examined record characters,
skips records over 100,000 characters, and caps Base64 input at 98,304 characters.
Decoded CSS must be valid UTF-8 and within 64 KiB. Limits are reported in the
selected view's status. Updates are atomic per sheet, not across the entire
saved-style batch; a rejected replacement retains that slot's previous CSS.
Settings changes take effect on the next completion or explicit preview,
not through a live settings observer.

Preview replaces one named sheet. During a host-owned load it queues one
preview for that request target. It is consumed during that request's completion
or cleared when the attempt ends, fails, or its view closes. It cannot remain
queued for a later retry. A busy view without a tracked request asks the user
to wait. Paused, missing, empty, and disposed tabs never enter the legacy DOM
path. Missing saved-style records are harmless. Arbitrary saved JavaScript
still reports that a legacy tab is required; it never opens one automatically.

Automatic hibernation conservatively retains views with named styles because
URL-only reconstruction would lose host-owned state. Manual suspension keeps
its existing destructive-to-live-state semantics.

## Regression Checks

`Lea.Tests <root> --sheets` covers cascade, named order, media queries, dynamic
state, source/data/history retention, Unicode byte boundaries, aggregate and
rule limits, native painting, editor font/selection/undo, scroll, and disposal.
It is also included in the full core suite. The compatibility runner includes
the `stylesheets.html` fixture and a named-sheet probe.

`Test-FirespiderBoot.ps1 -Case styles` uses gated loopback requests to cover
saved CSS, loading previews, domain/IP/IDN matching, encoding limits, fields,
private context, close cleanup, and failed-request isolation. The regression
reproduced a queued preview surviving a fully failed request before cleanup
was added. It waits for the host's complete load pipeline, not only the
engine's `Busy` flag.

`-Case style-start` covers the real STARTUP and timer automation routes,
later initialization, named replacement, missing records, unsupported script
status, paused/empty tabs, and absence of MSHTML. The full Boot suite has
37 cases. Runtime 164 remains an isolated test build; installed runtime 157
is not replaced. The earlier intermittent fragment placement failure and
unattributed recovery-time cast remain open, as described in
`STYLE_STARTUP_SAFETY.md`.

## Verification Record

Release core, isolated native editing, and compiled Firespider host suites pass.
All 64 compatibility probes pass across 61 fixtures. The engine DLLs total
485 KiB, up 3 KiB from runtime 163; their diagnostic GZip estimate is 204.9 KiB.
Compression reduces stored/downloaded bytes, not automatically execution time.
The sampled clean engine start, load, and bitmap-paint medians are 563.321,
4.211, and 35.495 ms respectively, not a measured speed improvement or a
performance promise. See `COMPATIBILITY_PERFORMANCE.md` for the method.

All 37 Boot cases passed across combined and focused runs, but the initial
combined run failed its password Copy/Cut assertion. A focused run then failed
the toolbar-focus assertion. Both original assertions combined field retention
and clipboard equality, so they do not establish which state changed or prove
password disclosure. Diagnostic assertions now distinguish these conditions
without recording sensitive contents. Four later runtime 164 isolated edit
passes and three runtime 163 comparison passes did not reproduce the failures.
The cause is still open; passing reruns are not treated as a fix. This remains
an experimental test build, not a browser validated for sensitive browsing.

All six actual EXE startup scenarios passed: clean, restored, locked session,
oversized session, lazy background, and lazy all (including Resume). Windows
remained responsive; no MSHTML or startup error dialog was observed.
