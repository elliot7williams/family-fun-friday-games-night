# Worker Hook Checkpoints

Milestone 146 remains open. This is a diagnostic change, not a startup fix,
speedup, browser compatibility claim or release. Broader readiness stays about
65%. No installed Firespider files are replaced.

## Why

The preceding standalone browser failed its five-second hello deadline. Its
last worker marker was `evaluate`, which precedes delegate creation, debugger
access, hook subscription and `Engine.Evaluate(source)`. That marker alone
does not show which operation was still running or why the worker timed out.

The ordinary `Be=0` worker now records:

```text
startup evaluate
startup debugger
startup subscribed
startup parsed
```

`debugger` precedes debugger access and `BeforeEvaluate` subscription.
`subscribed` follows successful subscription and precedes evaluation.
The existing `parsed` callback records the parsed AST boundary. A last marker
identifies the last entered interval, not necessarily a blocked method or its
root cause. Missing later markers can also reflect diagnostic delivery timing.
Do not treat an isolated passing run as resolution of the preceding failure.

## Boundaries

The production edit adds only two fixed `Trace` calls. It preserves the same
constrained VM constructor, subscription, evaluation and finally-based removal.
It adds no page-controlled labels or new startup wire messages. The existing
bounded error formatter retains the newest diagnostic tail.

The five-second startup deadline, zero-capability AppContainer, 128-MiB Job,
CPU-rate limit, bridge limits, native guards and page execution after verified
hello are unchanged. The renderer, parent scripting module and compact
bootstrap are frozen. Aliases and permissions do not change.

## Tests

Test-only `-sh` is the short alias for `--startup-hooks`:

```powershell
Lea.Script.Tests.exe "L:\Lea Development" -sh
Lea.Script.Tests.exe "L:\Lea Development" --startup-hooks
```

The eight checks cover the normal compiled entry, unique fixed checkpoints,
compiled call ordering, finally removal, untimed diagnostic output and bounded
failure-tail retention. An optional third argument selects an existing worker
PE for a compiled comparison; it does not execute that worker. This focused
contract intentionally selects the ordinary direct build, not experimental
indirect/prepared entries. Extra arguments are rejected.

The existing 16-check native startup-phase suite also checks ordering and
uniqueness with timing disabled and enabled, while keeping its original
isolation, warm-page and private-bootstrap assertions. Its indirect/prepared
entry assertions remain mode-aware. The full scripting total stays 3,010.

Evidence is kept in
`H:\Lea Evidence\Lea-146-debugger-checkpoints-2026-10-12`.
Runtime tests use a fresh L: NTFS deployment, not the H: archive volume.

## Recorded Results

- Both private Release builds: actual exit 0, no warnings/errors.
- Focused hooks: eight checks through each alias, actual exit 0.
- Identical test PE against the preceding worker: expected exit 1 after two
  assertions, rejecting the missing `debugger` checkpoint. Invalid extra
  arguments: expected exit 1 before any assertion or worker launch.
- Existing entry contract: four checks; VM options: 16; native startup phase:
  16; worker: 87. Each has actual exit 0 and the expected completion footer.
- Native CPU lifetime: actual exit 1 after 12 passing assertions. The helper
  exits successfully with 140 ms total CPU, but exact ACL restoration fails.
  The failing test does not record its original ACL, so this run cannot prove
  an inheritance-flag-only difference or classify the failure as harmless.
- Full scripting: actual exit 1 after 472 passing assertions, before its
  3,010-check footer. Isolated removal fails on the five-second startup hello
  deadline: wall 5,021 ms, CPU 437 ms. The captured tail includes `debugger`,
  `subscribed`, `parsed`, `declarations`, then `scope`. Hook setup and parsing
  completed in this run; the exact later cause remains unknown. No page
  execution, retry, observer, wider deadline or fallback is introduced.
- Separate ordinary browser smoke: actual exit 0, three animation frames,
  retained counter 1 and width 40 to 98, then zero pending frame callbacks.
  The reported 219 ms is the animation interval, not startup time. Before/after
  screenshots are visually inspected. Verified isolated worker, 128-MiB limit,
  no loopback exemption and no `mshtml.dll` in the reported host modules.

Only `ScriptWorker/Lea.Script.Worker.exe` changes among 19 frozen runtime
entries: 50,688 to 51,200 bytes (+512). Its SHA-256 is
`1CA6F91A58AB8E8998D0F24D6342A178CC4A820F613D81F217E44FFD3A8CAFCE`.
The required renderer bundle stays 749,568 bytes. Host EXEs are unchanged.
Original 67 host inputs, three installed files and 21 normal outputs remain
hash-identical. A complete source ZIP is entry-verified before committing.

Both failed gates and the preceding sealed startup failure remain evidence.
No runtime release ZIP is published. Repeated startup reliability, exact
cleanup verification and copied Firespider verification of this new worker
remain open; no general speed or finished-browser claim is made.
