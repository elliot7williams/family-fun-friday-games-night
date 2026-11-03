# Ordered Mixed Flow

Candidate work 2026-10-25; not adopted or release qualified.

## Current Wrapper Fix

Core `52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC`
omits empty unpainted mixed-flow container blocks. Paint/layout contexts and
children of flex/grid containers still retain their own block. This fixes the
extra section wrapper found by the retained white-space fixture without
changing document order, child styles, grid-item ownership or the DOM.

`ordered-flow-v6/result.json`, SHA256
`5C93311ACDA6D4509580C30E6EB8D85C458C28C3CBA6BC6247D93100EBAB9D7E`:
44 focused checks, 5119 named and 141 numeric checks pass. Previous D0A8 core
fails the two new wrapper-count controls. V5 failed one new test's mistaken
space-collapse expectation; existing pre-line raw text retains spaces and
newlines. Correcting that expectation did not change the implementation or
remove the exact raw-text check. Failed receipt 2B9F314E remains retained.
Foreground owner retired with zero leases/unverified=false; inputs unchanged.

Source archive `ordered-flow-v6/candidate-source.zip`: 53 entries/599124 bytes,
SHA256 `8B34DBB53FA1C4122A8CE7A448D3BA583D57241B1BC200A0E2628825F404F115`.
All member hashes verified; local source preservation, not portable delivery.
Full regression with diagnostic view12A9 passed pictures and the original
white-space block assertions, then FAILED nested-flow count: expected12, got13.
Receipt `ordered-flow-regression-v3/result.json`, SHA256
`44978585AFA7F508FE28880E85CF20E44582FED63555B8C54CB777859FE25F9C`.
Exit1/72560ms/no timeout/kill, inputs/bytes held; raw fixture ACL deltas retained.
Owner retired0/zero leases/unverified=false. The mixed section's prefix text now
has its own anonymous group, so representation assertions may be stale, but
correct ownership/paint geometry must be demonstrated before updating them.
Do not weaken the gate or restore text hoisting to force the former count.
No older visual/host/script passes transfer to this new core. The intermittent
queue failure remains open; DECODER_QUEUE_TRACE.md records its evidence.

Matched core/view source plus this full failure is backed up in
`ordered-flow-regression-v3-source.zip`, 11 entries/1565663 bytes, SHA256
`5126FCF8815155299D81B4BF146753106F377AA839914F3DAB380E974C98617F`.
Every member verified; local-only, not portable delivery or release qualification.

## Nested Representation Verification

The later exact matched full execution `decoder-launch-full-v2` reaches and
passes the updated assertions in140503ms/exit0. Raw summary A06A2CD2 remains
false for a whole-log footer predicate bug and three fixture grants; independent
read-only outcome FA96EC2A verifies the exact terminal source footer, original
deadline, binary/input hashes and exact ACE inverses. DECODER_QUEUE_TRACE.md
has full hashes and limits. This advances behavioral qualification only; no
intermittent startup fix, actual Firespider/script qualification or release claim.

The V6 core with diagnostic view12A9 passes 36 focused model/native geometry
checks at 720x900 and 320x900. Receipt `nested-flow-visuals-v1/result.json`, SHA256
`CABAC7356A80E3464C95B49106B934930E97C9F705DB23FE3CC815A7CD444660`.
Probe exit0/2936ms/no timeout/kill; inputs/runtime bytes and ACLs unchanged;
foreground owner retired0/zero leases/unverified=false. Both PNGs were inspected:
prefix text appears once above the child, both inside the blue container, and
following root text clears its border at both sizes. Scoped visual review is in
`visual-review.json`; no whole-browser visual approval or decoder qualification.

The exact 13-kind sequence and anonymous-prefix ownership are now explicit
regression contracts. Root Smoke.cs and the inverse-checked mature-source overlay
update eight representation/index references and add a no-duplicate/prefix-owner
assertion. Existing paint, auto-height, pixel, link, reflow and parent checks are
retained; budgets/security are unchanged. Full regression V4 failed earlier at
the intermittent decoder deadline, so these revised full-suite assertions have
not yet executed. Receipt `ordered-flow-regression-v4/result.json`, SHA256
`9A1C1B073D72892D652542D9CEAF32C2D5620696B217C5C103309802A597C9C3`.
Child exit1/29462ms/no outer timeout/kill, input/runtime bytes held, raw two fixture
ACL deltas retained. Foreground owner retired0/zero leases/unverified=false.
The older 12-count failure remains retained rather than overwritten.

`nested-flow-qualification-v1.zip`: 32 entries/1817616 bytes, SHA256
`023645FD52440AA8479B0FDB5C85921041371B66157D10EBF78AA103CB3D4695`.
Verified exact source overlays, rendered evidence, review, full failure and nested
matching core/view source archives; local-only, not a portable/release bundle.

## Original V4 Implementation

The previous container path collected inline text first, then emitted all nested
blocks/fields. That painted later labels before an earlier heading. Normal-flow
containers now emit anonymous inline groups and nested blocks in DOM order. A
container block retains ownership, and internal B.An lets LinkBlocks attach each
anonymous group to its owning container rather than skipping to an ancestor.
The DOM is not rewritten. Flex/grid roots keep their existing layout paths; a
normal mixed container inside a grid remains one grid item with its own children.
This is not full anonymous-box/HTML layout conformance.

Inline styles, links, generated before/after text, hidden nodes and native fields
are covered. Parent styles are retained until child declaration resolution, so
explicit percentage inheritance works; literal text runs receive inherited-only
paint style. Existing named/numeric decoding is retained.

Current core SHA256
`D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6`,
paired with literal-text view9D9C. Root E.cs/B.cs carry scoped changes. The build
overlays inverse-checked edits onto mature preserved sources, not older root E.cs.

Targeted `ordered-flow-v4/result.json`, SHA256
`FD5A70395712EC00EDE31D3D36077C80E99647BAC3B2345445E9486392F3D054`:
40ordering/style/ownership checks,5119named and141numeric checks pass. Previous
core9208 fails11of40 ordering checks. Inputs held, owner80422 exited0/zero leases/
unverifiedfalse. No interpreter deadline/security changes.

Earlier attempts remain retained: V1 failed compilation because a PowerShell edit
row expanded during concatenation (now parenthesized and row-length guarded).
V2's fixture used a title outside head; valid HTML and visible-flow assertions
correct that fixture error. V3 passes38focused checks but broad regression finds
lost explicit width inheritance. Its broad failed receipt is
`C3751CE1943C79B38B0180BDF569E4EEAF2B919FBDD23C7E38AE50D92BA595F4`;
V4 corrects style handoff and adds both block/field inheritance checks.

## Native Visuals

`ordered-flow-visuals-v2/result.json`, SHA256
`420AC4000877524E5EA285BFFA79AF2F0D0896CEA46A2D9B80E35E431BA4D4D4`:
45checks pass, including logical order, heading-before-label painted geometry,
top viewport, native field activation/editing/identity and literal ampersand pixel
contracts. Six nonblank PNGs at980/420px include top and focused/edited states.
Probe3575ms/exit0/no timeout/kill, inputs/runtime bytes+ACLs held. Owner8474 exited0/
zero leases/unverifiedfalse. The E261 negative paint control still fails as intended.

Assistant review `visual-review.json`, SHA256
`BF1BFDE403844F66954E4C85F306FAFC4385EB8C627FE845E00C2C21917C9252`,
confirms authored heading/label/field order and top coverage in all six images.
Full visual approval remains false: combining-glyph fallback still needs checking,
and the view paints a document-title line above the authored H1. Neither concern
is silently covered by passing geometry assertions.

## Open Regression Gate

The V4 broad retained engine/view run remains FAILED, receipt
`ordered-flow-regression-v2/result.json`, SHA256
`921E6A0C44CA04EF0CD96C228B02C0231A1B9B66571EA877F664E47F4FC5D7ED`.
It passes the previously failing inheritance point but stops at the decoder
deadline assertion:4688ms, phase=queue/result=cancel, worker=0, Ik=0. Parent
exit1/33577ms, no outer timeout/kill; inputs/runtime bytes held, owner99488 exited0/
zero leases/unverifiedfalse. No helper-start/kill claim or cleanup certification.
This failure has not been attributed to either ordering code or environment. Do
not rerun merely to get a green result, relax its deadline, or transfer older passes.

Next investigate decoder queue/deadline behavior, then complete broad, host and
scripting qualification for this core/view pair. Earlier15host/3320script passes
belong to core9208/viewE261 and do not qualify this pair.

## Source Preservation

`ordered-flow-v4/candidate-source.zip`,49entries/595992bytes, SHA256
`676F599FA5DFFB0453E48618FCCA3E7CA5FFFEA38D3C10B7F9B77DE1EE2D925C`,
preserves33compiled core sources, probes, data, local recipes and nested view source.
The visual probe, six PNGs, review, exact test binaries/logs and nested source ZIP
are preserved in `ordered-flow-visuals-v2-backup.zip`,30entries/1364567bytes, SHA256
`573008450BA8C062969DD25A5DC3CE80AB2F46914EE29ABDB5BC00A0C853ADD7`.
All archive bytes verified. Local private backups, not complete browser/toolchain
portability, an off-device backup, or a release bundle.
