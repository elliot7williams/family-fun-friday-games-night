# Lea Automatic Hibernation

Milestone 62 connects Lea to Firespider's existing optional hibernation policy.
Previously a pristine background Lea tab always failed eligibility because the
policy understood only `WebBrowser`. The native path now releases an eligible
renderer through the existing Lea suspension lifecycle. No new browser runtime,
background service, disk snapshot, or automatic setting change is introduced.

## Compact Retained-Data Query

`E.Ud(page)` and `V.Ud` return whether field data should be retained before a
URL-only reload. This is a conservative query, not a general unsaved-document
flag, snapshot, or permission to discard the entire tab.

- A value differing from its default is retained, including an empty value
  after clearing a prefilled field, whitespace, and read-only/disabled fields.
- Changed check/radio state, a changed select index, or selected file paths
  are retained. File contents are never opened by the query.
- Unchanged nonempty editable text-like fields are retained conservatively.
  Default choices, range/color values, buttons, and unchanged read-only or
  disabled values do not alone require retention.
- The query includes internal retained fields that disappeared from the visible
  form list after being hidden. It does not blur or modify the native editor.
- A missing page, disposed view, null field, or more than 8,192 combined
  retained/public field entries fails closed. A foreign page raises
  `ArgumentException`, matching the engine's ownership contract.

Use `V.Ud` on the view's owning UI thread. It reports data only; the host must
also consider activity, privacy, ownership, and other state it cannot restore.

## Firespider Policy

The existing manual **Hibernate Eligible Tabs Now** command uses the same
eligibility rules as the optional 30-second timer. Manual hibernation skips
the idle threshold; automatic hibernation requires the configured idle period.
The setting is not enabled by this update.

Lea tabs are excluded when selected, detached/hidden, already paused, grouped,
pinned, locked, private (globally or on the view), loading/downloading, missing
a supported page URL, or covered by a domain exception. A retained failure or
`Ud` also excludes the tab. A nonempty cookie jar is kept because a fresh view
would otherwise discard that session context. The underlying legacy branch
remains unchanged.

Automatic URL-only restoration cannot preserve all live state. This policy
therefore also keeps Lea views with Back/Forward history, a nonzero scroll
position, a keyboard focus target, accepted Lea script commands, or any
details/audio/video/contenteditable element. These are conservative exclusions,
not claims of full media or contenteditable support.

Eligible views are disposed, leaving the existing small Panel/Resume control
and saved URL on the same TabPage. Selection or Resume creates a fresh Lea
context. The released renderer is collectible; production code never forces
garbage collection. This does not add serialization of cookies, DOM state,
arbitrary host mutations, or JavaScript state. Explicit manual tab suspension
retains its previous behavior and is distinct from eligible-tab hibernation.

## Verification

The `hibernate` Boot case reproduces the old exclusion against runtime 161.
Updated coverage checks manual/timer eligibility, actual disposal/collection,
URL and selection retention, resume, idle and disabled policies, private and
protected tabs, changed fields, cookies, history, script/scroll/media state, busy pages
and downloads, failures, and domain exceptions.

`hibernate-start` starts the real configured 30-second timer after restoring
two Lea tabs, ages the background tab's idle record, observes its release,
and verifies close cleanup. Both paths assert no MSHTML startup dependency.
Core tests and the `retained-data.html` compatibility probe exercise `Ud`
separately. The Boot runner now contains 34 cases.

The final clipboard-free core suite (`--core`), compiled host suite, and all
62 compatibility probes across 60 fixtures passed. The editor-only suite passed
12 consecutive processes, and the fragment-only check passed 300 navigation
cycles. Earlier full runs nevertheless failed intermittently at paste and once
at initial fragment placement; another run reported Windows clipboard error
`0x800401D0`. Their causes have not been established. These repetitions do not
prove the intermittent failures fixed, and are not a clean default-suite pass.
Failure diagnostics and explicit `--edit`/`--core`/`--fragment` modes are retained.
No editor or fragment production behavior was changed in this milestone.

`Lea.Firespider/Hibernation-host.patch` records only the two-line dispatch change
to BrowserFeatures6.vb and requires the matching adapter and engine DLLs. It
follows the existing suspension/lazy-restore patches and reproduces the live
file from the saved pre-change baseline, ignoring line endings. Full host
source remains in the dated source backup, not an unrelated host-wide commit.

## Remaining Scope

The intermittent recovery-time Lea-to-WebBrowser cast is still unidentified;
this feature does not claim to fix it. Runtime 162 remains an isolated test
build while the live root runtime 157 stays unchanged. Full modern-web support
and a general JavaScript VM remain unfinished. Compression reduces package
size; hibernation releases eligible renderer state, not all browser memory or
the host process's baseline allocation.
