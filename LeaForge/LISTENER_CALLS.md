# Listener Invocation

Event dispatch now invokes functions through the captured intrinsic
`Reflect.apply`, rather than reading a callback's public `.call` property.
The same helper invokes DOM0 handlers and object listeners' current `handleEvent`
operation. Handler ownership uses captured `hasOwnProperty` with intrinsic
invocation as well. No public command or dependency is added.

This follows the callable/operation lookup and receiver rules in the
[Web IDL callback interface algorithm](https://webidl.spec.whatwg.org/#call-a-user-objects-operation).
It is not a claim of complete DOM event conformance: Lea's existing synchronous
error propagation and realm-reset policy remain, rather than general browser
per-listener exception reporting and continued dispatch.

## Regression And Verification

- The new trusted regression fails against the preserved old parent with
  `public call getter reached`. Its callback is valid; dispatch incorrectly
  reads its throwing own `.call` getter.
- All 90 focused checks pass: 28 each through trusted `Js`, isolated `Jp`,
  and the native adapter, plus six native load/activation/isolation/paint checks.
- Coverage includes throwing/null/noncallable/redirecting own `.call` values,
  callable proxies, ignoring `handleEvent` on callable listeners, single live
  operation lookup, receiver/argument identity, DOM0 return-false cancellation,
  ignored object-listener return values, once removal, and public intrinsic
  replacement. Noncallable operations with a fake `.call` and throwing getters
  retain typed `js` failure, no fake mutation, and fresh-realm recovery.
- An initial focused assertion incorrectly expected a `TypeError` prefix in
  the result message. The result contract carries the interpreter message,
  not that prefix. The corrected fixture checks the exact pinned message and
  separately verifies the interpreter throws a `TypeError`. The failed log
  is preserved; production error reporting was not changed.
- Parent, worker, tests, and browser Release builds have zero warnings/errors.
  Bootstrap reproducibility/AST checks and all seven generator self-tests pass.
- Adjacent focused regressions also pass: all 75 event-option checks and all
  14 worker-entry checks. These separate results do not complete the full suite.
- Native keyboard activation updates the counter. Desktop/narrow pixel checks
  pass, and both captures are visually inspected. The actual SDK-output browser
  EXE also passes its inline-startup/counter/isolation smoke, retaining the
  128 MiB worker limit and no-loopback exemption policy. Its capture is inspected.

Run the focused regression from a Release test output with:

```powershell
& '.\Lea.Script.Tests.exe' 'L:\Lea Development' --listener-calls
```

`--listener-calls-trusted` runs only the trusted group. An optional third argument
is the native capture prefix. The group also runs in the full suite immediately
before the existing final worker-entry probe; earlier gates retain their order.

## Retained Startup Failures

Neither complete source run passes. The initial run fails isolated tree recovery
after bridge exhaustion: hello-phase startup reaches 5,018 ms wall/78 ms reported
CPU, with `parsed` the final captured marker. The final test-harness run gets
farther but fails native-view worker recovery at 5,014 ms wall/46 ms reported CPU,
with `evaluate` the final marker. Neither reaches the new listener group.
The expected final full-suite count is 1,261, not a completed pass.

Both failures and the focused fixture error are retained under
`H:/Lea Evidence/Lea-146-listener-calls-2026-10-08` alongside the red/green logs,
old/new runtime snapshots, captures, source backup, and provenance audits.
No deadline is increased and no isolation or test gate is bypassed.

## Size And Adoption

The readable bootstrap is 30,291 UTF-8 bytes; its generated resource is 21,716
bytes, 28.3% smaller. This change adds 40 readable/28 compact bytes compared with
the preceding checkpoint. That is resource compression, not whole-engine size
or a speedup claim. Protocol, aliases, dependency versions, execution/startup
budgets, and origin gates are unchanged.

Release packaging and a new Firespider candidate are held because the full source
gate fails. The previously verified v23 host retains the preceding bootstrap;
its 43-case/six-startup passes do not transfer to this change. Installed browser
files remain untouched. Milestone 146 stays open and broader readiness stays
about 65%; the intermittent worker-startup timeout remains unresolved.
