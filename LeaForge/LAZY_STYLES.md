# Lazy Element Styles

Milestone 146 remains open; broader readiness stays about **65%**. This is a
bounded element-wrapping allocation improvement, not a startup-failure fix,
general speed benchmark or stable release.

## Change

Previously every newly wrapped element immediately created its style object
and six bound setter functions, even if page code only used its ID, text,
attributes, events or tree relationships. The six-name table was already
shared; it is not newly deduplicated here.

The existing private element state now caches `style` in the same way it
already caches `classList`. The first style read creates the facade, and later
reads return the same object. Wrapping or querying an element alone does not
create its style object or setters. The factory is private to each realm,
uses the captured definition/state intrinsics and is not exposed on globals.
Separate elements and replacement realms have independent style objects.

The element's own `style` property intentionally becomes a read-only accessor
instead of a read-only data property. It stays non-enumerable/non-configurable;
strict overwrites fail, and the getter validates its Element receiver. A
borrowed getter returns the receiving element's style, not another node's.
Normal `element.style.color = ...` and retained-style use are unchanged.
Existing individual setter closures remain bound to their original element,
including their legacy behavior when called with a foreign `this`.

This is still Lea's six-property, write-only facade: `color`,
`backgroundColor`, `width`, `height`, `display` and `fontSize`. Setter metadata,
ordinary expandos and undefined property reads remain unchanged. No
CSSStyleDeclaration constructor, cssText, computed style, getPropertyValue,
setProperty, removeProperty or other CSSOM API is claimed. Removed-element
writes still fail without mutation and retire the failed realm.

All public native aliases, host-call/statement/memory/recursion/array/regex
limits, nonblocking window agents, isolated five-second startup, page
deadlines, Job/AppContainer/file/network guards and remote-script policy are
unchanged. There is no retry, fallback, new dependency or worker-mode switch;
the ordinary worker remains `Be=0`.

## Allocation Evidence

`-ls` aliases `--lazy-styles`, with `core`, `trusted`, `isolated` and `native`
modes. Extra arguments are rejected before startup except for a native capture
prefix. The core fixture wraps 16 elements and then requests four styles.

One identical test EXE against the preceding frozen runtime records **16 style
objects / 96 setter definitions** before any style read and exits 1 on the
new lazy-allocation assertion, as expected. The candidate records **0 / 0**
after the same wrapping operation. Its first style read records **1 / 6**;
repeat reads add nothing. Four requested styles record **4 / 24**, leaving
twelve unused facades dormant. A test-only captured defineProperty interceptor
counts actual style definitions and distinct target objects under unchanged
production options. It is not a wall-time or heap-byte benchmark.

All new focused checks pass: **14 core, 17 trusted, 19 isolated and 22 native**,
72 total. They cover identity, exact descriptor shape, receiver checks,
bound-setter compatibility, all six mutations, public intrinsic/global
collisions, stale-element rejection, explicit clean recovery, native policy,
pinned exact-worker disposal and nonblank 940/420-pixel captures. Existing
privacy coverage adds the private factory name and passes all 36 checks.

## Build And Compatibility

The Script, worker and test builds complete without warnings/errors. The
generator's seven AST round trips and changed-literal control pass; generated
assets retain matching ASTs. Readable source is 37,833 bytes and compact source
27,407, an increase of 259/187 bytes. The initial generator wrapper's argument
failure is retained; explicit argument calls succeed before any engine build.

Only two of 19 runtime records change: the parent Script DLL grows by 512 bytes
to 101,888; the worker remains 50,176 bytes. The required Engine/Win/Image
bundle stays 749,568 bytes. This trades small fixed code/state overhead for
avoiding unrequested per-element objects; no general speedup is inferred from
binary size or compression.

Existing assets, compiled entry/options, 50 startup-stage, 31 real-deadline,
first-call, prepared-cache and native CPU-lifetime checks pass. The native
worker and startup-phase suites pass 87/16 on this candidate. Those isolated
passes do not erase earlier worker/startup/CPU or ACL-comparison failures.
The full suite now expects 2,945 checks, previously 2,873, including the new 72
at the original late-test position. Worker-only observation still expects 87.

One final full regression passes **all 2,945 checks**, actual exit zero, with no
external observer or opt-in startup trace from the controller and no retries.
Existing fixtures may enable their own declared startup traces. The full run
does not reproduce or explain preceding intermittent native failures, and is
not the copied Firespider host/startup release matrix. Four AST-only observer
count/footer controls also pass; no inspector is launched.

The native style captures are visually inspected at both widths, showing
independent colors/backgrounds, retained text, responsive columns and no
overlap. The standalone browser copy passes counter, Start/Pause, three-frame,
elapsed-time, isolation and no-MSHTML smoke. Its captures show 40..88 pixels
of animation and 185 ms displayed after pausing. The host EXE is reused
unchanged with the updated parent/worker libraries; no installed browser is
replaced. The compressed package verifies every entry against its source file.
Archive compression is a storage/delivery property, not a runtime speed claim.

Evidence: `H:\Lea Evidence\Lea-146-lazy-styles-2026-10-11`. Complete source
backups remain under `H:\Lea Source Backups`. Original Firespider source,
installed browsers, normal build outputs and preceding sealed evidence remain
unchanged. Milestone 146 and modern-web/release verification remain open.
