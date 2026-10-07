# Startup Report Tail And Host Follow-Up

Broader lightweight-browser readiness remains **about 65%**, a project estimate,
not a standards score. This checkpoint improves diagnostics and builds separate
Firespider candidates; it does not fix intermittent cold JavaScript startup.
Milestone 146 and the current host-release gates remain open.

## Report Fix

The parent previously appended captured worker stderr to its timing header and
kept only the first 512 characters. A sufficiently long timed trace could lose
its latest worker phase, making an earlier phase look like the endpoint.

`Jp.Sd` now keeps the parent timing header and the newest portion of the captured
worker diagnostics within the same 512-character limit. Omitted earlier detail
is marked with `...` when space permits. Short and exactly fitting reports are
unchanged; tiny remaining budgets avoid a partial separator. Oversized parent
headers are bounded defensively. There is no public API or alias change.

This formats the existing captured stderr, not an unlimited log. The shared
launcher still captures at most its first 1,024 characters. The separate
`Jr.M` error message also has its own 512-character bound and need not contain
the entire `Sd`. A last observed marker is not a stack trace or proof of the
operation that consumed the remaining time.

No worker source, parser path, protocol, deadline, origin rule, CPU/memory limit,
AppContainer permission, or cancellation behavior changes. The parent DLL stays
**97,792 bytes**. Project-reference rebuilding is disabled for the focused SDK
builds: all eight worker files and the other 18 runtime dependencies remain
byte-identical to the preceding dispatcher checkpoint. The rebuilt parent and
test binaries are identified by their recorded hashes, not by transferring old
test passes to new binaries.

## Verification

Root logs use `Lea-io-host-`; evidence is preserved under
`L:/Lea Evidence/Lea-146-io-host-2026-10-07`.

- `report-red.log` reproduces loss of the latest worker phase using the original
  prefix-only formatting, extracted into the production formatter for the test.
- `report-green.log` passes all **seven** deterministic formatter checks.
- `report-native-phase.log` passes all **eight** native phase checks, with normal
  and timed traces, verified isolation, unchanged defaults, and retained reports
  across warm execution.
- `current-io.log` passes all **31** dispatcher/blocked-I/O checks on the current
  parent build. Focused parent, test, and standalone builds have zero warnings
  and zero errors; all 19 browser/test runtime dependencies match.
- `report-full.log` passes the new report checks, startup/dispatcher checks, and
  all 24 trusted tree checks, but fails the first isolated tree startup at
  5,013 ms wall / 921 ms CPU, with `startup parsed` last. The expanded count is
  **1,047**, but there is no full pass at that count. The preceding 1,040-check
  pass remains historical evidence for its earlier frozen parent binary.
- `bootstrap-profile.log` completes both instrumented in-process samples:
  1,608 ms wall / 125 ms CPU, then 14 ms wall / 0 ms recorded CPU. CPU accounting
  is coarse; zero recorded CPU does not mean no work. These probes insert marker
  callbacks and do not use the restricted worker. They locate diagnostic phases,
  not native cold-start performance or the cause of the timeout.

## Separate Candidates

Both net48 candidates compile from 67 original host inputs, 25 overlays, and
four current integration sources. Each audit matches all 19 deployed runtime
dependencies and the eight-file worker inventory. The installed project and
browser are unchanged.

| Candidate | EXE SHA-256 | Parent DLL SHA-256 |
| --- | --- | --- |
| `L:/Lea Builds/Lea-146-io-v20/runtime` | `34F56483F2F04D0D8CC11FB50775D6E826B175CA9E4EF63461087CE397C30134` | `2BF973996E549B38810F9B43E6384DD25CBD78D794762E50051EE29B36DCCF5C` |
| `L:/Lea Builds/Lea-146-startup-report-v21/runtime` | `ABC2F1126DCC79DFBB8A2FF90C319ACC885E45A4A5F30B8C022CD681648E5527` | `5C8810196C1F2DDDA3ED9FE7634FD8385E54EA0298379B7EFE522F506462DC8C` |

Both EXEs are 1,134,080 bytes. v20 contains the dispatcher optimization but not
this report fix. v21 includes both. The unchanged worker executable SHA-256 is
`B8D9D1CE1CF79C16C8CDF1D0522EC98D03B192FD5B450DF1EACEAD8B94A81FA0`.
The parent is built at pre-commit revision `bbb6368`; the retained worker was
built at the earlier `e437f4e` revision. Metadata differences are not a worker
implementation change, and v20 is not silently updated to v21's parent.

v20's normal script case fails first-demand startup at 5,017 ms wall / 1,375 ms
CPU after `startup parsed`. Its separate timed diagnostic fails at 5,005 ms /
765 ms CPU; its prefix-only report stops at `evaluate`, so omitted later detail
cannot be ruled out. Neither is a host-script pass or an automatic retry.

v21 passes all **six actual-EXE startup scenarios**: clean, restored, locked,
oversized, lazy-background, and lazy-all. Windows remain responsive, no checked
startup error dialogs appear, and no MSHTML module is loaded. Lazy-all remains
deferred and resumes successfully. Observed readiness is 4,764..8,717 ms; these
are functional observations, not a latency or smoothness guarantee.

Its separate timed scripting case still fails first-demand startup at 5,022 ms
wall / 921 ms CPU. The captured child reaches `parsed` at 2,202 ms wall / 546 ms
CPU but not `dom`; native launch takes 924 ms. Worker and parent clocks start at
different points. The remaining work includes DOM evaluation and scheduling;
these markers do not identify a single cause.

No complete current 43-case host matrix, extracted-runtime test, independent
archived-source rebuild, new release ZIP, or installed-browser replacement is
claimed. Keep the failures and successful ordinary-startup checks distinct.
The next investigation must explain native cold bootstrap cost rather than
relaxing deadlines or treating a later warm pass as a cold-start fix.
