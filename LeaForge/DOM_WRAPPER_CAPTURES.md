# DOM Wrapper Intrinsic Captures

Milestone 146 remains open; broader browser readiness stays about 65%.
The reproduced wrapper-cache bug is corrected. The complete scripting gate
fails on a native startup timeout, so this checkpoint is not a stable release
or a verified startup fix.

## Change And Regression

Document lookup and new-wrapper construction previously used page-mutable
`String`, `Map.prototype` operations, array mapping and `Object` construction
helpers. Overriding Map methods could intercept Lea's private wrapper cache.
The exact preceding runtime fails the new regression with
`public wrapper cache`, using the identical final test executable. Its original
failure, production bytes and source remain preserved.

Wrapper caching now reuses captured Map get/set operations, including existing
and previously unseen nodes. Document lookups use the captured String function;
query-result mapping uses a captured array method. New style/node descriptors
and private event-target dictionaries use Lea's captured property-definition
and object-creation functions. Wrapper identity, tree navigation, literal
conversion behavior, method metadata, public aliases and parent-side admission
remain unchanged. This does not claim immunity to every prototype modification
or complete Web IDL/document-method conformance.

All 36 scope checks pass in trusted and isolated modes. New cases cover poisoned
cache methods, existing/new/missing wrapper identity, one-time name conversion,
poisoned global String and array mapping, new child/parent wrappers under Object
overrides, connected style writes, and existing reset/recovery protections.

The generator passes seven round trips and its changed-literal control, then
generation/check verifies the full AST and committed hashes. Both compiled
resources pass all four asset checks. Two helper invocations rejected invalid
self-test argument shapes before generation; their logs/scripts stay preserved.
The corrected invocation does not repeat a completed generator gate.

## Size And Protections

Readable bootstrap: 32,655 bytes. Compact bootstrap: 23,639 bytes, 43 more than
the preceding compact source. Only the Script DLL and worker change among
nineteen production records. Their sizes remain 92,672 and 46,080 bytes; the
required renderer bundle remains 745,984 bytes. No general speed or memory
improvement is measured or claimed.

Native startup remains five seconds; the default page deadline remains two
seconds. The 25% CPU rate, 128-MiB Job memory cap, interpreter constraints,
AppContainer, mitigations, exact pipes and file/network denial remain intact.
Remote-origin scripting remains refused. No fallback or automatic retry is added.

## Full Gate Failure

The ordinary unobserved suite completes 1,328 passing assertion lines, then
fails during isolated timer-error recovery. A fresh worker misses startup at
5,018 ms; its retained report is `phase=hello; launch=72; probe=6`, with last
recorded stage `startup vm`. No `startup bridge`, source, evaluation, parsed,
DOM or verified hello is recorded for this attempt.

The parent prints `cpu=0 ms`, but its existing counter exception fallback also
prints zero. This is not proof of zero CPU use. This unobserved run has no native
counter/thread samples or timed worker trace, and establishes no root cause.
The earlier after-parsed/before-DOM failures remain unchanged and preserved.

The planned 1,755-check full gate is failed and not retried. Focused passes and
the independent browser/host passes below do not replace it. Remaining ordinary
checks are unverified on this runtime; no full native-core, full host-matrix,
package or independent source-rebuild release gate is claimed.

## Independent Browser Checks

The unchanged actual standalone browser EXE, with the two candidate dependencies,
passes its local inline lifecycle, native click, isolation and worker-retirement
smoke once. Its fresh capture is visually inspected.

A fresh 379-file native Firespider copy retains the prior host EXE and identical
checksum-verified harness; only the two candidate dependencies change. The
planned observed whole-script case passes, including split isolation/disposal,
events, timers, expected script errors/recovery, aliases and remote-origin
refusal. The sampler retires with 282 samples, eight workers and 69 thread
snapshots, with no errors, drops or truncation. Three inspected host captures
come from a new unique temporary directory, with verified attempt timestamps.
This is independent integration evidence, not a release gate or timeout fix.

## Preservation And Next Work

Original 67 Firespider source inputs and three installed binaries remain
unchanged. Source, both production revisions, the unchanged test executable,
negative control, full failure, observations and captures remain under
`H:/Lea Evidence/Lea-146-dom-wrapper-captures-2026-10-09`. All current source is
archived and verified entry by entry before sealing the Git checkpoint.

Next startup diagnostics must distinguish unavailable CPU counters from zero
and investigate the newly recorded VM-to-bridge boundary without losing the
previous bootstrap failures. Continue broader DOM/event robustness and secure
origin work while retaining the native deadline and all protections.
