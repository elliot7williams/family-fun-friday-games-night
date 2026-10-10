# Element Subtree Queries

Milestone 146 remains experimental. Broader readiness stays about 65%, not a
standards score. This is a browser-functionality addition, not a startup fix.

## Native Aliases

`N.Q(selector)` returns the first matching descendant or null. `N.Qa(selector)`
returns a read-only static snapshot of at most 256 matching descendants. Results
follow tree order, not selector-list order; duplicate selector groups do not
duplicate matches. The receiver itself is excluded. Matching can still refer
to ancestors outside the search subtree. Lea's existing selector compiler and
matcher are reused, without another parsing dependency or an interpreter.

```csharp
var panel = e.Id(p, "panel");
var first = panel.Q("input");
var fields = panel.Qa("input");
```

These trusted host reads do not require script permission, run JavaScript or
grant mutation ownership. Retained detached nodes can query their remaining
descendants; former document ancestors no longer match. Later queries see live
attributes while earlier snapshots retain their membership and node identity.

Raw selectors are bounded to 1,024 UTF-16 units, 16 groups and 16 total parts.
Invalid or unsupported selectors throw `FormatException`, including on empty
subtrees. Traversal stops at the first match for Q or at the result cap for Qa.
A traversal beyond 8,192 node visits or depth 128 also throws `FormatException`,
so malformed cycles or oversized child lists supplied by native host code
cannot recurse or search without a bound.

## JavaScript

Elements gain shared, non-constructible `querySelector(selector)` and
`querySelectorAll(selector)` methods. Receiver branding precedes the required
DOM-string conversion. Existing wrapper identity and captured conversion/map
intrinsics are retained. Private bridge operations reuse `q/qa`, with an
element handle instead of adding another capability surface.

Invalid or unsupported selectors become branded `SyntaxError` DOMExceptions;
valid misses return null or an empty array. Caught syntax errors do not retire
the realm. Permission revocation, the 512-handle and 256-call ceilings, realm
retirement on budget failures, and detached mutation denial remain intact.
Interpreter statement/allocation/time limits can also stop result expansion.

This follows the descendant, first-result and static-result model described by
the [DOM query interfaces](https://dom.spec.whatwg.org/#dom-parentnode-queryselector).
It is not full DOM conformance: querySelectorAll returns an Array rather than a
NodeList, and Lea retains its case-insensitive bounded selector subset. `:scope`,
pseudo-elements, escaping and complete selector semantics remain unsupported.
The existing document query methods still use their empty-result error behavior.

## Verification

The identical final test binary fails against the preceding runtime in two
separate controls: absent native aliases and absent JavaScript methods. With
the new runtime, 32 native alias, 21 trusted and 21 isolated checks pass. They
cover order, subtree boundaries, ancestor matching, snapshots, wrapper/field
identity, conversion/branding, syntax limits, detached reads, mutation denial,
budget retirement/recovery and traversal/result ceilings. Four compiled
bootstrap-resource checks pass. The initial test build's incorrect adapter
property names are repaired; its failed build log remains preserved. The final
test, engine, script and worker builds complete without warnings/errors.

The ordinary unobserved full suite is planned for 1,858 checks but fails after
161 existing assertions during the untraced native startup-marker gate. It
reports hello-phase startup timeout, launch 271ms, parent probe 43ms, total wall
5,010ms and CPU 437ms; the last recorded worker marker is `evaluate`. This
interval includes hook setup, parser/binding/JIT and other startup costs, not
a proven parser stack trace. The new subtree tests are at the suite's end and
are not reached in this failed broad run. The failure remains recorded without
retry, independently of the passing focused tests. Earlier startup and CPU
fixture failures remain unmodified; no intermittent-startup fix is claimed.
The original helper miscounts this prefix as 153 by excluding eight passing
profile assertions. A read-only audit confirms their delegation to the same
check counter and corrects the total to 161. The original result and failed
backup-audit attempt remain preserved; neither native gate is rerun. The first
source ZIP is fully verified but predates this documentation correction; a
second verified ZIP preserves the final source.

The required renderer smoke, four new native-view checks and actual standalone
browser lifecycle/click/isolation/retirement smoke pass. The observed Firespider
script case fails later during split recovery: launch 120ms, parent probe 20ms,
wall 5,008ms and CPU 640ms. Its failure text records completed DOM bootstrap at
worker wall 4,540ms / CPU 640ms, followed by a timing-truncated `probes` marker.
The retired sampler keeps 304 samples across eight PIDs with no errors or
dropped requests/samples. A unique parsed-phase fingerprint associates the
failed startup with PID 32928 (an inference), ten samples and final CPU 640ms;
the last global sample is from another worker and is not used as its phase.
These boundaries do not establish the cause. The next startup investigation
should distinguish first hello-frame write from parent read rather than assume
the parser is still running. No page code moves before verified startup.

All five fresh view/browser/host captures are visually inspected, including
host main, attributes and split views captured before the later failure.
These narrow passes and captures do not substitute for either failed gate.
No full 43-case host matrix, six actual-EXE startup
scenarios, new release package or installed replacement is claimed.

## Size And Preservation

Only Engine DLL, Script DLL and worker EXE change among 19 production records.
They grow by 1,024, 1,024 and 512 bytes to 421,888, 94,208 and 46,592 bytes.
Win, Image, interpreter dependencies and browser/main EXEs stay byte-identical.
The required Engine/Win/Image bundle is 747,520 bytes, not the whole optional
scripting distribution. Readable bootstrap is 33,629 bytes; generated bootstrap
is 24,408 bytes, 27.4% smaller, with matching AST JSON and fixed-point generation.
This is source-size reduction, not measured startup, memory or browsing speed.

No startup deadline, native security permission, Job memory/CPU setting,
interpreter limit or remote-script policy changes. Original 67 Firespider
inputs and three installed binaries stay unchanged. Native execution remains
on NTFS; H: stores evidence/backups. C: still has little free space, but it is
not established as the cause of the startup failure.

Plans, frozen preceding/candidate runtime bytes, identical-binary controls,
actual test exits/logs, captures, source backup and Git checkpoint are kept in
`H:/Lea Evidence/Lea-146-subtree-queries-2026-10-09`. Complete source ZIPs remain
under `H:/Lea Source Backups`, with every entry's size and SHA-256 verified.
