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

Add V.Ht through existing LoadCore/Show lifecycle, then a separate host candidate
using it for accessibility reports with no report-file write. Review resource/
script/origin policies and reload/fragment/history semantics; prove no generated
report files and no saved private URL/HTML after tab close or process exit.
Retain61986private-retention failure and all existing candidate evidence. Full
matched SDK/host qualification and verified GitHub source checkpoint remain.
