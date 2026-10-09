# Firespider Attribute Writer Integration

Separate v32 carries the bounded attribute writers and captured name validator
into Firespider, but its full host run is not passing: 42 cases pass before a
new live-field assertion exposes native readonly synchronization. Actual-EXE
startup checks are held. The failure, exact runtime and zero-capture bookkeeping
error are retained; see `LIVE_READONLY.md` for the independently reproduced fix.
Original source and installed browser stay untouched.
This remains an experimental integration checkpoint; milestone 146 is open and
broader lightweight-browser readiness stays about 65%.

## Host Coverage

Only the host harness changes in the v32 checkpoint. Production engine dependencies
come unchanged from `ATTRIBUTE_NAME_CAPTURES.md`, whose ordinary source suite
passed all 1,721 checks. There is no new alias, dependency, protocol, execution
budget, startup deadline, AppContainer capability, Job allowance or origin rule.

First-demand and inline-navigation checks now exercise shared non-constructible
`setAttribute/removeAttribute`, receiver-before-conversion and required-argument
admission, literal values, removal no-ops, forbidden-name DOMExceptions, parent
bounds and atomic rejection. Page replacement of regex `test`/`exec` and string
`indexOf`/`slice` cannot intercept the captured validator. Each check completes
four accepted mutations without replacing the page or its worker realm.

A separate live-field check sets generic attributes through JavaScript and the
compact native `V.Sa/Da` aliases. It checks retained editor/model/text identity,
changed default value without losing edited text, readonly and required presence,
live inline width, literal command-looking text, retained/fresh accessible names,
missing/invalid removals and restoration of the fixture's original metadata.
The authored field ID stays intact for later script recovery.

## Negative Controls

One host EXE and one checksum-verified harness are used for both controls and the
current runtime. Each control swaps only its nineteen engine dependency records,
then restores and verifies the current files before the next attempt.

- The v31 production runtime starts successfully, passes the existing
  exception/event/Element checks, then fails `host generic attribute methods`.
- The immediate preceding `ad905a6` runtime starts successfully, passes those
  same checks, then fails `poisoned host attribute validator`.

Both failures are retained without a retry. Neither is reclassified as a startup
timeout. The final harness SHA256 is
`B3A8AA87309BA739CC7D92D00154281A49BA1914D0A88DEA3ABF0CF06A1964F9`.

## Build And Artifacts

The candidate builds for .NET Framework 4.8 from 67 unchanged original host
inputs and 25 overlays, with 90 unique final build inputs. Its copied runtime
contains 379 files including the single compiled boot harness. The production
dependencies are checked against the preceding standalone source checkpoint.

Native experimental browser:
`C:/Users/Elliot/AppData/Local/Lea/HostDiagnostics/Lea-146-attribute-edits-host-v32/Firespider Internet 19.exe`.
The EXE SHA256 is
`FE62EE42618EC57E88E4C5028FA742AE1F7A439092F17BD55E1359A6E3403651`.
Keep its runtime directory together; the executable is not self-contained.

Buildable host source/output:
`H:/Lea Builds/Lea-146-attribute-edits-host-v32`.
Evidence:
`H:/Lea Evidence/Lea-146-host-attribute-edits-2026-10-09`.

No new release package, extracted runtime, independent source rebuild gate,
source-suite rerun, speedup, cold-cache benchmark, resolved intermittent startup
cause or installed replacement is claimed. Earlier failures remain preserved.
Remote-page scripting, broader site compatibility and legacy-only host paths
remain unfinished.

## Corrected v33 Integration

Separate v33 uses the final readonly fix in `LIVE_READONLY.md`. The exact same
final harness is reused without compilation; its source and SHA256 match v32.
Only the WinForms production DLL changes from v32, by 512 bytes. No deadline,
capability, execution allowance or origin-policy change is made.

Three planned negative controls reach their intended failures: old attribute
API, old page-mutable validator and old native readonly synchronization. Each
uses this same host/harness, retains its failure and restores all nineteen
current dependency records. The ordinary current-runtime matrix then passes
all 43 cases once, including both attribute-method realms, the active field,
fixture cleanup, later worker recovery, split isolation and remote-origin refusal.
Three newly generated main/split/attribute captures are visually inspected.

All six actual-EXE startup scenarios also pass once: clean, restored, locked,
oversized, lazy-background and lazy-all. Each is observed for at least eight
seconds and stays responsive without a legacy engine; lazy-all resumes its
deferred Lea tab. Whole-browser readiness measurements are not script-worker
handshake deadlines or cold-cache benchmarks.

The first startup-helper launch was blocked by PowerShell execution policy
before any browser case ran. Its empty stdout, policy stderr and zero-case
record are retained. A child-process-only policy override runs the reviewed
helper; no system policy changes, browser-case retries or production changes
are used. This does not erase earlier genuine worker-startup failures.

Final v33 native experimental browser:
`C:/Users/Elliot/AppData/Local/Lea/HostDiagnostics/Lea-146-live-readonly-host-v33/Firespider Internet 19.exe`.
Its EXE SHA256 is
`45C53E7F6998E8B4FFEC9DD0C9E9A667B9322693855020DDF35E921572777E19`.
Buildable host source/output:
`H:/Lea Builds/Lea-146-live-readonly-host-v33`.
Evidence:
`H:/Lea Evidence/Lea-146-live-readonly-host-2026-10-09`.
Keep the 379-file runtime together; this is not a self-contained executable or
an installed replacement. The v32 failure above remains failed and preserved.
