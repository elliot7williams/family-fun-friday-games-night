# In-Memory Documents

Current2026-11-10: guarded SDK B117/C83 passes full3412/all89 scripting phases
plus bounded memory-policy checks (SDK_MEMORY_POLICY.md/SDK_SOURCE_FULL_SUITE.md).
Matched Firespider1E390343 build1399CDED binds all12SDK products exactly.
Fresh UI E04992E1/auditBBE1E307 passes memory-report/private regression/navigation;
actualEXE clean startup/settings/navigation6EDCA54B/auditD2B5018C passes with
stable runtime bytes/ACLs and clean owned retirement. Original15workflows,
print/package/extraction and broader security/lifecycle still pending.

Historical F05E/86DB has a reproduced bounded Lea-command policy gap with
scripting enabled. Guarded11E2190C/71F4ED73 passes7new policy checks plus12model/
17view checks; MEMORY_SCRIPT_POLICY.md exact scope, negative and audit. Prior
no-file report results remain valid only for that bounded regression, not full
memory-API security. Later qualification above does not repair historical files.

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

Separate host candidate86DB310DB3CAD186A046308A63FB9CDC352D42B10A9816E51A51FF6C59BAFF4D
now uses qleaMount/Ht, no accessibility report-file write, preserving private
view flag and rollback/disposal on failure. Source712CD05F (105members, four
baseline differences: existing route hook/adapter plus core/view). Build receipt
20263B4F2789086313704E0FCCD59EC42BB9065BA26542B7D80B1CCD98F72C1E,22outputs,
original120s/5s,owner87411 exit0. Source/build on I: memory-report-host-* folders.
Other framework SDK libraries are reused: not a complete source SDK rebuild.

Fresh NTFS report UI receipt4FA98483EB92FCDFFE35B9A9B79776A5A06A2418DC6E3A36B5FB7864FB04B1AF,
auditCEED3213A51E7410E3E0F723EC0C6439A0B76F03ED231D3ED68A6E2119C6AABA
on I:/LeaBuildStorage/memory-report-host-ui-audit-20261110-v1. Probe compile2124ms/
retire260,UI16117/retire9,60s/90s and5scleanup,exit0/Job0/drained/no timeout/kill,
401runtime bytes+ACL/input stable,owner17250 guarded exit0. Existing click/hover/
escaping/empty/loading/paused/no-selection plus original navigation assertions
retained. New private report is private/non-file/opaque/nonlocal; no newly
generated Firespider Accessibility HTML files, private raw URL/title absent from
tested settings collections and session checks. Screenshot visually inspected.
The old61986private-file defect remains evidence; this bounded regression is
addressed on86DB only. Not all serialization, post-crash/global file activity,
worker security or complete browser privacy qualification. No installed adoption.

GitHub checkpointbaba09ad9cdb54e0e0a1832c67dd6d425c92dd10 on existing source-
backup branch verifies all579raw remote SHA256s. Scope
LeaSourceCheckpoints/20261110-memory-reports contains baseline SDK/host and
separate memory-sdk-source (actual tested core/view projects), memory-host-source
(87host sources/assets), tools/docs, raw evidence and retained failures. No
product/dependency/key binaries, not complete matched SDK qualification.
Proof I:/LeaBuildStorage/github-memory-reports-readback-20261110-v1/result.json.
Final postpublication docs/pre-edit copies retained separately in publication
handoff on I:; later wording is not claimed inside this immutable commit.

Next review resource/
script/origin policies and reload/fragment/history semantics; prove no generated
report files and no saved private URL/HTML after tab close or process exit.
Retain61986private-retention failure and all existing candidate evidence. Full
matched SDK/host qualification and verified GitHub source checkpoint remain.
