# In-Memory Documents

2026-11-10 first core candidate for removing disk-backed browser reports.
Implementation fragment: Lea.Engine/MemoryDocument.fragment.cs, injected only
into a fresh copy of the exact current SDK source graph by
Lea.Build/Test-OwnedMemoryDocument.ps1. Root E.cs differs from the qualified SDK
export and was intentionally not overwritten. Fragment is not a standalone
compilation unit or automatically included by the root project.

## Implemented In Candidate

- E.Ht(html) parses/layouts host-provided markup without a file or fetch/cache
  call, pushes normal bounded history, and uses existing host navigation guard.
- Synthetic about:lea-memory/UUID identity gives Og an opaque/non-local origin;
  about:blank would be locally eligible and is deliberately not used.
- P.Mm marks memory entries. Rl/RlA rebuild their retained H rather than fetching
  their synthetic URL. No address/base override parameter is exposed.
- Existing MaxHtml UTF8/character limit, parser/tree limits and input lifecycle
  remain. Null/oversize/guard denial do not change the active history entry.
- Base is reset to the synthetic origin; inline CSS uses existing parser/layout.
  External stylesheet acquisition is skipped, and page.Je is false on creation.

This is host-supplied markup, NOT a sanitizer for hostile HTML. View image/resource
loading, forms, links, CSS resource references, script binding, viewport/reflow,
fragment behavior, rendering and disposal still need candidate-bound review and
tests. Do not infer complete network confinement from this model-only work.

## Verified Boundary

Fresh36file core source set from SDK receipt51A82366; only E.cs/P.cs changed.
Extension/reload inverse verified before compilation. Output core SHA256
A69DBD5F79C785760FC3562BF4FA0F9C1F64D005F6B185AE2459D8234F8271D9.
Owned result I:/LeaBuildStorage/memory-document-model-20261110-v1/result.json,
SHA256F14824486E9EBF0FAC65D014CDD762CD863A120E96DB7A6BE7CBDF379978DC05.
Actual project build14126ms/retire153, test compile710/retire26, model705/retire63;
original120s/60s/30s and5scleanup, all exit0/Job0/drained/no timeout/kill, inputs
unchanged. Owner67353 guarded exit0. Source/project/test/model-only execution on
I: launches no script/image sandbox workers; not exFAT sandbox qualification.

12checks cover content/title, synthetic address, non-script/cache metadata,
opaque nonlocal origin, unique identity, history, sync/async reload, base reset/
external stylesheet omission, navigation denial, null and oversize atomicity.
No full suite, view/host, independent delivery audit or privacy-fix claim.

## Next

V.Ht now implemented as Lea.Win/MemoryDocument.fragment.cs, injected into a fresh
copy of current SDK view sources. Uses CheckIdle/LoadCore/Show and existing cursor
retirement, not reflective document replacement. Root V.cs remains untouched.
Fresh source-built coreF05E474C73BAAE19278EE4C80D9D617BD361432B3AADCD472BEA680280E9CBFD
and viewFEBA2DFD4C987ADDC47B53217947D05B112C60B2E4AE3C942F72FEAE79F3E77E
pass12core checks again plus17actual WinForms view checks at720/360px. Current
view receipt436078461C4F889612B6AD1139FB255C3105E8E7F6BEB6851FA7396F2FF2108A
on I:/LeaBuildStorage/memory-document-view-20261110-v1; independent audit
C59876032BFC698B110F7B639A1CCF7FDB20B69D71325DEF1CB83F32618A50B5 under
memory-document-view-audit-20261110-v1. Owner62291 guarded exit0.

Six owned runs: core build14112ms/retire116, model compile967/30, model938/37,
view build10517/34, view probe compile1147/23, actual view2399/434. Original
120/60/30/120/60/30s and5scleanup, all exit0/Job0/drained/no timeout/kill, all
inputs stable,3NTFSruntime bytes+ACL stable. Model/source/build on I:, actual
view runtime on C: continuity-evidence/memory-document-view-v1/runtime.
Tests cover content/events/busy retirement/private opaque nonlocal flags,
nonblank pixels, history/reload, denied navigation, post-disposal rejection and
no HTML in the probe's isolated TEMP. Both PNGs visually inspected for nonblank
report content/fit. Not a system-wide file trace, broad resource/script security
proof or current Firespider private-report fix. Source/tools/raw/images locally
hash-backed on I:, not yet GitHub readback verified.

Next a separate host candidate
using it for accessibility reports with no report-file write. Review resource/
script/origin policies and reload/fragment/history semantics; prove no generated
report files and no saved private URL/HTML after tab close or process exit.
Retain61986private-retention failure and all existing candidate evidence. Full
matched SDK/host qualification and verified GitHub source checkpoint remain.
