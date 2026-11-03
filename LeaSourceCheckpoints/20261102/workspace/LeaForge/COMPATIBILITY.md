# Compatibility Objective

2026-10-31 current1474/D10DBdevelopmentpackage has font:initial/model/render,
3412matchedscriptchecks,print+15stagehost+fresh3smokes. FONT_SHORTHAND.md and
CURRENT_HOST.md have exactscope. New staged4DA8 em/percentlonghand+shorthand
passes18new/prior38modelchecks only;FONT_RELATIVE.md records bounds. Noallsite/
fullCSS/zero/fractionalfidelity claim,no new4DA8browseradoption/all6gatesOPEN.

2026-10-29 exactfont0B09/A8CC pair passes86 rendered equivalence/selection checks
at720/360px,with screenshots inspected and unchangedA85 negative failing. This is
longhand equivalence,not CSS pixel-unit/DPI or all-font correctness. Next matched
script/host/print qualification;FONT_SHORTHAND.md. Legacy UIA remainsopen.

2026-10-29 staged font-shorthand candidatev3 passes26computed-style checks while
unchangedA85 baseline fails14positivecases. Atomic resets/cascade/vars/inheritance
implemented for common bounded grammar,NOT full font conformance/render/browser
qualification. FONT_SHORTHAND.md lists unsupported forms and exactcandidatepins.

2026-10-29 font shorthand absence now reproduced against exact matureA85 core:
2longhand/invalid controls pass,all11 positive shorthand cases fail. Source-pinned
fixture and ownership receipts in FONT_SHORTHAND.md. Not yet implemented;next
atomic parsing/reset/cascade fix plus negative,visual and matched qualification.

2026-10-28 unresolved fixture observation: selection test with font:24px Arial
rendered at default-looking18px line height; explicit font-size/font-family
longhands produced computed size24 and37px rendered line height. Investigate
font shorthand and screen font-unit/DPI semantics separately. Measured selection
now follows renderer metrics; this does not certify CSS typography conformance.
See DOCUMENT_SELECTION.md and retained gestures-v4-v8 evidence.

Updated 2026-10-21 from the user's request: "lets try and make it work for all".

The objective is broad real-world website compatibility using Lea's own engine,
not only local/static pages and not a Chromium/WebView fallback. Do not redefine
completion as the currently passing fixture subset. No engine can honestly be
certified against every existing and future website by a finite test run.

## Required Workstreams

| Area | Release evidence required |
| --- | --- |
| HTML and DOM | Parsing/recovery, tree mutation, events, forms and navigation across named standards tests and real pages. |
| CSS and rendering | Cascade, selectors, layout, typography, images, scrolling and responsive behavior with visual comparisons. |
| JavaScript platform | Language/runtime, DOM bindings, tasks/microtasks, lifecycle and recovery on the exact candidate. |
| Network and origins | HTTP/TLS, redirects, cookies/cache, resource loading and origin enforcement, including cross-origin behavior. |
| Modern web APIs | Explicit inventories and tests for storage, fetch/network APIs, graphics, media, workers and other required platform APIs. Untested is not supported. |
| Browser workflows | Search/browsing, tabs/windows, sessions, forms/editing, downloads, accessibility and dependable startup. |
| Security | Remote scripting only after a reviewed implementation and confinement/origin validation, not by disabling existing protections. |
| Delivery | Matching complete source, reproducible builds, Visual Studio embedding, tested EXEs and verified backup. |

These workstreams describe the broad target, not implemented capabilities.
Existing local regression results cover specific bounded behaviors; they are
not arbitrary-site certification. Remote HTTP/HTTPS/UNC scripting remains OFF.
Startup reliability and all release gates in FINISH_PLAN.md remain open.

2026-10-25 numeric-reference candidate515121C4: implemented decimal/hex recovery,
C1 remapping, scalar replacement and single-pass presentation decoding;141
targeted checks plus retained engine/view behavior pass. Full named-reference
coverage was not implemented in that numeric candidate. Later named-reference
implementation and candidate-bound checks are recorded in NAMED_REFERENCES.md;
do not confuse that earlier snapshot with current source. Matched Firespider host/startup15/15 passed, but
full scripting failed early at trusted bootstrap/events1000ms timeout. Adoption
is held; visual/security/broadqualification remain open. See NUMERIC_REFERENCES.md
for exact receipts and audit limits.

Track individual APIs/tests/sites as implemented, partially verified, unsupported
or unknown, with candidate hashes and evidence. Add representative real sites
over time without treating the chosen list as the entire objective. Do not turn
test pass percentages into a browser-completion percentage.

2026-10-26 current EXE638 host-tools source audit41C7B4B9 verifies104 non-obj
source files against retained checkpointA1515801. Printer-friendly q945 returns
not-ready on Lea tabs, and current-page colors use a legacy-only document route.
Both are missing Lea integration, not engine-supported features. The reference
inventory labels other WebBrowser mentions unclassified; mention counts do not
prove reachability, failure or migration. No UI/runtime test or product change.

Later2026-10-26: page-color candidateFBBD913C addresses the color route in a
separate focused actual-host regressionE4AF5A59. Six fixtures and automatic/
command/custom cases pass; metadata media matching and full CSS color conformance
are not claimed. Limits and remaining changed-host qualification: PAGE_COLORS.md.
Printer-friendly pages remain unported; qualified638 package remains unchanged.

Later boundaryFAC02195 and FBBD OWN15 campaign3829CAFC now pass, including six
actual EXE startup scenarios. New FBBD package/extraction not yet done. Multiple
tools still call explicit legacy-tab route q3; Lea fixture creation uses qln.
No all-site, resolved intermittent startup or final security claim. PAGE_COLORS.md.
