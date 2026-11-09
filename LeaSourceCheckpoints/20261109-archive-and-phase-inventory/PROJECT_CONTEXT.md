# Lea Project Context

Updated 2026-11-09. Git base535add5, branch codex/lea-engine-foundation,
dirty worktree. Read AGENTS.md and current Git status/diff before editing.
Source and exact-candidate evidence outrank older roadmap prose.

## Objective And Release Boundary

Independent lightweight Lea engine, short public aliases, Visual Studio embedding,
Firespider adoption without Chromium/WebView, broad site compatibility, retained
source and reproducible EXE/SDK delivery. All six FINISH_PLAN gates remain OPEN:
startup, unified candidate, engine/security/lifecycle, host migration,
compatibility, delivery. No defensible completion percentage or all-sites claim.
Keep mature Lea work distinct from prototypes, games, FSIN and Bubble Wallpaper.

## Current Task And Next Action

2026-11-09 goal continuation: previous turn made verified archive progress.
Current task is startup diagnosis, not further automatic cleanup. New static AST
inventory BootstrapPhaseInventory.cjs/test pins both original bootstrap resources;
five synthetic/five negative controls pass, report4A392DC5 on I: under
bootstrap-phase-inventory-20261109-v1. Both resources have37direct lexical-block
function declarations; textual phase distribution14/13/10. Counts do not prove
timing/cause. Next bounded native profile should separate block-entry setup from
textual spans once original storage reserve is satisfied. C: about285MiB free;
no new native builds, changed engine binaries or installed browser replacement.
CURRENT_BOOTSTRAP_PROFILE.md records boundaries. All six gates remain open.

2026-11-09 current user request: prepare a verified older-project archive on I:
for MANUAL cleanup, leaving C: originals untouched. Archive tooling:
Lea.Build/Prepare-ManualEvidenceArchive.ps1; selected temporal-startup-v1.
Do not delete or bypass the earlier cleanup policy refusal. Archive completed:
15083files/1299400429logical bytes, ZIP534551120bytes; all entries hashed and
source bytes/metadata rechecked, exit0. ManifestA9E4843D, ZIPB9A3A0DA;
STORAGE_CLEANUP.md has full hashes. Original C: folder untouched, manual cleanup
only, no reclaimed-space claim. Latest C: about288MiB free; builds remain paused.
Recovered-parent source ZIP fresh extraction rebuild now PASS7ED30B07,
audit80E9CC01 (PARENT_SOURCE_RECOVERY.md); byte-identical406C, no runtime test.
Prior preflight stop remains historical. Six release gates remain OPEN.

2026-11-09: I: is readable external storage; F: has no usable mounted filesystem.
Storage preflight stopped the initial v5 compile before launch; that owner was
recovered/retired. Fresh compile passed9586ms, retirement85ms, exit0/Job0/no kill/
stable inputs; owner retired. Receipt on I: under
private-binding-full-compile-20261103-v5/result.json SHA
A3E1DDC2500EB4DE49023DE7699168ACC33F789E0D4BEB3A1E0855F2FECC358F.

Continue the signed-relative-font increment and SDK qualification.
No completion percentage is defensible; all six FINISH_PLAN gates remain OPEN.
Packaged Firespider B1A remains unchanged. Do not edit prototype Jp/E/V as if
they were the mature production sources.

### Current Metadata-Corrected Candidate
- Core C5CC0B99CFA54C090C0E553B2C6080186E4E412BD254A533307860D5522C67E0.
- View 4EE7E01AF6F24A87BF3B4BE873B895539108C8792A281962F54AF4A3D03DF344.
- Both now declare net48 target metadata, matching the earlier SDK baseline.
  The previous13076D91/65B87DAE libraries lacked this attribute. This correction
  is NOT proven to fix startup timing.
- Current core28model checks PASS: font-net48-relative-candidate-v1/result.json,
  SHA E1A27B9A2ACCCAC6820393FEEBBD6145911D30E9ECF50459DE28E9AB5B4FC46A.
- Matching pair114signed render/selection checks PASS at720/360:
  font-net48-signed-render-v1/result.json SHA
  952D427EBE4CC04653F22C23833A7D1845BC323AF931ED5181A9D72236181337.
  These six owned compile/test runs clean; inputs stable at execution, owners retired.
- Exact sources/binaries/receipts copied and hashed on I:/LeaBuildStorage/
  font-net48-candidate-20261108-v1. Fresh prior regressions on this exact pair
  now add26shorthand,12initial,142initial-render and142relative-render checks:
  continuity-evidence/font-net48-prior-{shorthand,initial,initial-render,
  relative-render}-v1/result.json. All four passed with stable inputs,
  ownerAttention=false and eight clean owned runs. Total464 focused checks
  belong to C5CC/4EE7, not transferred from13076D91/65B87DAE.
  Net48 metadata audit9E7EF0EA is under the startup
  investigation folder. FONT_RELATIVE.md holds detailed scope.

### Full-Suite Failure And Timing
- Previous pair full-v4 timed out300074ms during70(VmOptions), phases0..69
  complete; receipt6A1DAC82, audit0B9F49C4. All65runtime bytes stable, but
  Lea.Image.exe ACL changed and trace-restoration proof missing.
  QUARANTINED continuity-evidence/private-binding-full-suite-v4 remains untouched.
  No repair, reuse or unchanged rerun to clear the gate. Job0/drained/retire1967ms.
- Diagnostic old/new/new/old pilot on the PREVIOUS pair passed12isolated
  startup/DOM sessions. Receipt69440043, independent sample auditE46D8B52 at
  I:/LeaBuildStorage/signed-font-startup-abba-20261108-v3. All five owned runs
  clean;43runtime bytes/ACL unchanged. Baseline mean total1798.83ms vs candidate
  2058ms; medians1840/1420.5ms conflict. One small pilot proves neither a speed
  improvement nor the timeout cause. No security/budget limits relaxed.
- v1 diagnostic compile error88AB8C32 and v2 overly strict network-code
  assertion74E1E3F6 retained. Harness now follows the actual Np1-or2 denial/Mg15
  contract. These were harness corrections, not engine-policy changes.
- Historical-prefix logged ready wall medians651/812ms and CPU46/203ms;
  E13D397E observations are NOT independent paired trials. No OS/storage cause
  established. Production parent406C/F61B worker pair remains unchanged.

### Sources, Storage And Next Step
- Exact full-suite60sources recovered on I:/LeaBuildStorage/
  signed-suite-recovery-complete-20261108-v1; all-source audit218860AF.
- Parent source recovery now15/15exact files under signed-font-startup-
  investigation-20261108-v1/parent-source. Missing14.cs recovered from restored
  GitHub mature-sdk checkout; exact CE6F3CCA hash and raw remote bytes match.
  Completion proof74C7A0C6, original buildD32732C0.6.cs is mature Jp,8.cs is Js.
  New I: recovered-script-parent-build-20261109-v1 source project rebuilds EXACT
  406C5634 bytes; receipt0039F8C1/audit6C86D172. Compile8041/retire116ms,
  Job0/drained/inputs stable, owner67174 exit0. No instrumentation or guessed
  metadata in this rebuild. PARENT_SOURCE_RECOVERY.md scope and full pins.
  Successful original recipe archived as build.ps1 (5B35128A); current wrapper
  additionally offers an unexecuted archive mode. Source-only19entry ZIPD759DFFA
  verified, package proof0E6AB3F0. Fresh-extraction rebuild stopped BEFORE launch
  at C: reserve (627695616bytes free/600MiB required), owner10792 exit0; no child
  or attempt output. Preflight record20808823 on I:. User asked for1GB free C:.
- I: is working exFAT storage. Use it for source/build inputs/archive outputs;
  native ACL staging stays NTFS. Former I: source paths are mostly absent.
  Documents/Lea Source Backups is a junction to L:, not C: consumption.
- Completed diagnostic stages v1/v3 were hash-verified to I: with SDDL metadata,
  then removed from C:. Locators continuity-evidence/signed-font-startup-abba-
  v1-relocation.json and v3-relocation.json require restoration before replay.
  Full-suite-v4 quarantine was NOT moved. C: free space fluctuates; cause unproven.
- Source143 ZIP EF7A225C (1646files) plus scoped later checkpoints retained on I:.
  Newest verified GitHub source checkpoint is5fbf9d3 below; prior commits retained.
- Fresh entry-v2 compile receipt C7B74036 passed, path-bound to full-suite-v5.
  Version5 full-suite assembly on I: is prepared, manifest F647679D939FD323
  55585B61561761BAFAB7B6239D83601A0FFE23B0BCF21DEA. Exactly five scoped
  replacements; unchanged assertions,182fixtures and60compiled sources.
  Build/stage/audit/owned-run wrappers accept v5 with exact C5CC/4EE7 pins.
- Fresh v5 stage591FBFFC/auditE01AC763 PASS,247files/resource7 pinned.
  Full run F099EC00 FAILED at phase8 computed script: bootstrap TimeoutException,
  last checkpoint events. Phases0..7 complete; child exit1 after12785ms,
  retire190ms/Job0/drained/no kill or outer timeout. All65runtime bytes/ACL and
  inputs unchanged; trace restored, owner62949 guarded exit0. Independent
  failure audit68075D65 and raw evidence on I:/LeaBuildStorage/
  font-net48-full-failure-20261109-v1. Full v5 retained; no unchanged green rerun.
- Current-parent buffered diagnostic E926C855 completed on a separate rebuilt
  parent C56FB88B, explicit diagnostic metadata (14.cs was unrecovered then).
  SourceD74BB7C6 preserves14recovered inputs/current bootstrap196BCE96, seven
  reversible projections/sixnegative controls. Three owned runs clean, tenruntime
  byte/ACL and inputs stable; owner44595 guarded exit0. AuditCAA0FF01 validates
  15markers/sixtrace negatives. One containment probe, NOT full-suite validation.
  Evaluation856.12ms, scope-to-events335.96ms, events-to-interfaces156.94ms;
  preparation1171.59ms BEFORE evaluation. No cause or performance gain proved.
  CURRENT_BOOTSTRAP_PROFILE.md contains exact pins and scope.
- Fixed constant-folding ABBA completed four fresh containment probes. Folded
  diagnostic parent51CB6B6C, exact one-option inverse/three source controls;
  receipt01984D30/audit7B8CEB47 on I: current-bootstrap-fold-{abba,audit}-20261109-v1.
  Six owned runs clean,20runtime bytes/SDDL and inputs stable, owner65505 exit0.
  Mean caller baseline2462ms/folded2837ms, wide variation. NOT adopted, no speed
  or timeout-fix claim. Fullv5 remains failed. CURRENT_BOOTSTRAP_PROFILE.md details.
- Calling tool host CPU-policy query91E2B7E1: immediate job flags/value0 (rate
  control disabled),13decoder controls PASS. Does not query every ancestor/test
  job or establish cause; no quotas/priorities/working-set settings changed.
- Separate native timer/frame global installer Gd implemented/tested, NOT adopted.
  AST source6E5B5FD8/eightnegative controls; exact JavaScript functions retained.
  Node312/two metadata comparisons pass33905C1E. Actual compiled parent39595C95
  passes19installer contracts and four fresh ABBA containment probes. Eightowned
  runs clean,22runtime byte/ACL and inputs stable; owner83730 guarded exit0.
  Receipt350F8F2F/audit38C9699E on I: native-window-globals-{abba,audit}-20261109-v1.
  Mean total baseline1897.5ms/native2162ms, no speed gain; compact JS438bytes
  shorter but DLL512bytes larger. NATIVE_WINDOW_GLOBALS.md exact limits/sources.
- NEXT: retain folding as an unadopted experiment; attribute remaining startup
  declaration/scope-to-events spans and wall/CPU gap
  before choosing an optimization, preserving1000ms and full3412/89 campaign300s/5s.
  No proven font causality or metadata timeout fix. Do not transfer older passes.
  Use detailed fresh startup traces; do not remove probes,
  relax budgets or change isolation to pass. Print/host/package qualification
  follows only after appropriate engine verification.
  Also retain native Gd unadopted; do not port it to workers or repeat unchanged
  timing trials without a new justified hypothesis. Original fullv5 still failed.
  The recovered original project is now the source basis for further parent
  changes; do not keep substituting diagnostic metadata into new product builds.
  First finish the source-archive extraction/rebuild when the original reserve
  is available; do not claim its unexecuted mode or lower guards to clear it.
- Owned read-only scheduling control6FAA0177: native child4852/TID12860 identity
  verified against original HANDLE/birth; both samples normal priority32/memory5,
  power masks0. Two runs clean, owner81334 exit0. Not an interpreter/ancestor-job
  measurement, no settings changed, no cause or timeout fix established.
- Passed original full-suite-v3 stage archived to I:/LeaBuildStorage/
  private-binding-full-suite-v3-archive-20261109-v1 after verifying249files,
  staging/pass receipt pins and all65runtime bytes/SDDL. Scoped C: copy removed,
  61234554logical bytes reclaimed. C: locator continuity-evidence/
  private-binding-full-suite-v3-relocation.json. Restore original absolute paths
  and recorded SDDL on NTFS before replay or historical fixture rebuild.
  Full-v4 quarantine NOT changed. Archive script dry-run/apply/syntax PASS.
- All focused, compiler and full-suite tool owners retired. Full v5 is failed,
  not a qualified engine/browser candidate. Packaged Firespider remains unchanged.
- Source144 archive on I: verified1654files/14101441source bytes, ZIP SHA
  A2FBE04B4F58D93E4ACB1C4869A6C97A1564857B9A9DA005B35B42A64DC10AE2.
  Root/Lea text-source scope, not complete historical offline SDK. Regression/
  entry180file checkpoint manifest723FEA36 at
  I:/LeaBuildStorage/font-net48-regression-checkpoints-20261109-v1.
- New source145 ZIP DDDC44C079FA93063E76228D3F65E67D30B12BBC5FA9A385072E6B3B6A048C8F
  verified1658files. GitHub source-only checkpoint84files (current tools/docs,
  mature core/view,14exact parent sources, current bootstrap and diagnostic
  source/results) committed/pushed nonforce to existing backup branch:
  codex/lea-source-backup-20261102-v1 commit74e538706eca5584b7853f7cd61769eec6845447.
  Every staged blob matched local bytes; credential-pattern scan passed. All84
  raw GitHub downloads at that commit matched SHA256. Readback proofF8DD8819
  on I:/LeaBuildStorage/github-current-profile-readback-20261109-v1/result.json.
  Backup working checkout on I:; shared workspace Git index/branch untouched.
  No runtime dependency binaries uploaded; not a complete offline SDK or release.
- Latest source146 ZIP8FC65AE3163DF975A6B63AF5D7E56749527ACD133E0A376765737438736116C4
  verified1661files on I:. Folding/CPU source-only follow-up16files pushed
  nonforce as7037d35209192cc2c16d76a9dfce1b313e6ecf67 on the same backup branch,
  folderLeaSourceCheckpoints/20261109-bootstrap-folding. Secret scan/staged raw
  byte checks PASS; all16raw GitHub SHA256 readbacks match. ProofA9D37946 at
  I:/LeaBuildStorage/github-folding-readback-20261109-v1/result.json.
  All native owners retired; no production binary or failed stage changed.
- New source147 ZIP3767A9BC76EE3315B3F936A951EB824AACCC933CA35C9A8BDC3B1FC8DB40125E
  verified1666files on I:. Native-window source-only checkpoint23files pushed
  nonforce as5fbf9d3b4b2955e934e7abc4b5ab8cf4a23df345 on the same branch,
  folderLeaSourceCheckpoints/20261109-native-window-globals. Secret/staged-byte
  scans and all23raw remote SHA256 readbacks PASS. Proof8E1D13B0 at
  I:/LeaBuildStorage/github-native-globals-readback-20261109-v1/result.json.
- Matching baseline/native22compiled primary files copied/hash verified on I:
  native-window-globals-runtime-checkpoint-20261109-v1, manifest2AB9E412 with
  original file SDDL. Archival only; restore NTFS paths/permissions before replay.
  Attempt to reclaim only the unadopted native C: runtime was rejected by tool
  policy BEFORE execution. No locator/removal was performed; C: runtime remains.
  Do not retry a destructive operation through another mechanism to bypass policy.

Detailed prior notes and all retained failure/source/build pins:
LeaForge/history/signed-font-20261108-history.md; previous private-binding
history remains separately linked below.

## Authoritative Sources And Candidates

- Current core/view are C5CC/4EE7, with exact staged sources in the net48
  candidate checkpoint above. Current parent406C/workerF61B sources/results are
  distinct from the older SDK references below. Older I: paths are historical
  and are NOT presumed present on the reconnected disk.
- Mature core: continuity-evidence/print-styles-v3/core-source.
- Mature view: continuity-evidence/print-live-capture-v1/view-source.
- Root Lea.Engine/E.cs and Lea.Win/V.cs are NOT automatic mature replacements.
- Historical SDK stages/builds were under I:/LeaBuildStorage. Source net48 parent:
  sdk-script-20261101-net48-v1; build proof
  core-view-sdk-build-20261031-v3-script-libraries-net48/result.json
  E3B542B8CAD81E81321E30EBCDF6682F2AC0E371A3DCF6BE0A5B299498793AD7.
- Historical parent906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C;
  adapter1C3425A5CC1311AB61DE01FD49202A83CB7EA047B952BCAAF280C6ABC5025F2A.
- SDK core5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A;
  viewA622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D.
- Native workerF8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C;
  sdk-script-worker-native-20261101-v1/result.json
  81690529F6933FA641969386747B6C9549BBD14D91F29B5CF1D70BF908CE6F97.
  Target installed .NET8.0.33/RollForwardDisable; 8.0.32 crossgen tool is distinct.
  Keep modern worker Jint/Acornima distinct from framework parent dependencies.
- Rejected pooled parentAFA6D95F21EC093F20F88177FD0B3710B956B1775958317C4BE6ED09864341B6;
  adapter3337594CC43BAD8A0BBE5993B26D518F9E2C4DB2BFDD967D45B0FD1B9B265DC4.
  Rejected session-retaining parent0425BFF9...; SDK_SESSION_IO.md.
- Framework host EXE requires Lea.Sdk/ScriptHost.config binding redirects;
  DLL.config alone is insufficient. Remote HTTP/HTTPS/UNC scripting remains OFF.
- Last verified development browser ZIP is2D684D53, Firespider B1A, on
  L:/Lea Builds/Firespider-Lea-private-binding-development-20261103.zip.
  CURRENT_HOST.md has exact hashes and bounded extraction/startup evidence.
- Aggregate SDK print package has core/view/image/print, NOT complete JavaScript,
  full host integration, notices or release delivery; SDK_PORTABLE_LAYOUT.md.

## Full-Suite Evidence And Open Problems

Baseline full suite is60 sources/65 runtime files/182 fixtures,3412 checks,
89 phases. Latest original relative-font PASS receipt:
continuity-evidence/temporal-startup-v1/relative-font-script-owned-run-v1/result.json
7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205,
296694ms/300s; not transferable to changed SDK libraries.

SDK fullv3 mature906E failed phase50 generic native nonblocking fixture load.
Failure-only AgentWait diagnostic preserves condition/success message/calls and
budgets. Fullv4 reached phases0..85, timed out at86, and left seven ACL deltas
(image+six worker files) and trace-restoration failure. QUARANTINED, untouched.
Fullv5 rejected retained-session I/O at29. Fullv6 rejected pooled I/O at18.
All evidence remains; previous index has full pins/source locations.

Cold startup remains intermittent: pooled quickv1 failed hello wall5021ms/cpu140;
later quickv2 passed25, but does not erase it. Earlier trusted-parent1000ms
bootstrap failures and full-suite margin also remain unresolved.

Legacy managed UIA visible-range text access still crashes0xC0000005; native COM,
cross-apartment and modern controls have separate passing evidence. No OS defect
or full legacy gate closure proven. Actual clipboard adapter, broad shaping/bidi/
selection behavior, physical printing and broad-site/API compatibility unfinished.
Do not submit more virtual print jobs/change printer preferences to clear gates.

## V6 Reproduction Index

Build-SdkFullScriptEntry -Version6: proofDC4A7F6C..., hostEC963A3F...,
entry manifest3FB9AEEC..., dispatcherFC938FC2..., compile4067ms clean.
Assemble-SdkFullScriptV2 -Version6: assemblyC53A9E35...;
Build-SdkFullScript -Version6: proofBBBEA754..., compile9444ms clean,
testEXE479CF1D4...; no assertions changed.
Stage-SdkFullScriptRuntime -Version6: staging68EE4771...;
Audit-SdkFullScriptStaging -Version6: audit461A6719...;
Test-OwnedSdkFullScript -Version6: retained failure as above. Do not rerun
unchanged for green. Scripts pin full hashes; do not replace pins with unverified
dynamic selection. Embedded fixture resource was read without loading its host.

## Ownership, Storage And Backup

OwnedTestProcess.cs pinned
D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA;
Invoke-OwnedTestRun.ps1 pinned
E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00.
Nonadmin retained NoExit TTY/original handles/atomic Jobs/explicit inheritance.
Compile120s/focused30s/full300s/shared cleanup5s;600MiB preflight and512MiB owned
reserve unchanged. No PID-reopen/kill substitution. Retire owner only with zero
leases and no unverified child. Never leave needed sessions unobserved at handoff.

I: is user-authorized64GB exFAT, staging/verified archives only;
capacity/endurance not certified. Actual ACL qualification on C: NTFS; reuse only
with verified current bytes/ACL and clean ownership, never quarantined candidates.
C: fluctuates independently, including637MB->0->644MB on2026-11-02; cause
unproven. No system/chat/history/database deletion. Verify reserve afresh.
Older source ZIPs relocated after flush/hash/length/readback checks; original
paths/hashes/SDDL/time and restore locations in STORAGE_CLEANUP.md.
Latest native reserve relocation: two old print-data v61/v62 sourceZIPs,
2740293bytes, I:/LeaBuildStorage/native-view-source-zips-20261102-v1;
manifest2705F31F739EE95580F93799980CAD0BF40B83A522A730266C2C8A66CCFCEB49.
Reserve rejection61464 had no output/child runs; owner exited0. Native test
started only after original600MiB plus driver space preflight was satisfied.
User permits project-only verified archival/removal, not loss of source/current
candidates/quarantined failures. STORAGE_CLEANUP.md has restore locators/receipts;
old live-path auditors need reconstruction, not a waived hash check.

Latest pre-lazy-map source checkpoint127:
I:/LeaBuildStorage/source-20261031-v127/current-source.zip,
965958BC6925ACA21092174F92D336834B4F8CA9CE1A9091CC9C41DAC448ACDC,
1559files. Lazy-map checkpoint locator: source-20261031-v128/result.json on I:;
consult its actual verified/files/hash fields, not the backup script's blank stdout.
Historical frozen sources separately preserved in print-data checkpointv64;
source supplement scope is not the whole repository. No current GitHub upload
verified. New source checkpoint follows this task. Do not schedule memory jobs.
User delegated backup repository organization on2026-11-02. Verified existing
public origin elliot7williams/family-fun-friday-games-night already has Lea
development history. Use separate codex/lea-source-backup-20261102-v1 branch
based on live remote fc229c03..., not stale local535add5. Main/development branches
and active worktree/index remain untouched. SOURCE_BACKUPS.md defines source-only
scope; third-party libraries, raw test evidence and historical experiments are not
all included. Publication and independent readback PASS at commit1cfdb502...;
I:/LeaBuildStorage/github-source-20261102-v1/verified-backup-receipt.json records
exact scope. No local source/evidence deleted; no release gate closed.

## History And Detailed Documentation

Complete previous index preserved byte-for-byte at
LeaForge/history/PROJECT_CONTEXT_20261101_before_sdk_full_v6.md,
D98C5C99D6B65319E9D6FB7E5718E80D58CE5EE7FD5CDD5A16175DBDE1F76112,
124009bytes. It links earlier history and full evidence; its NEXT statements are
historical, not current instructions. The older20261029 index remains retained.
Feature docs: SDK_SCRIPT, SDK_CORE_VIEW, SDK_IMAGE_WORKER, SDK_PRINT,
SDK_SCRIPT_SAMPLE, SDK_FULL,
SDK_PORTABLE_LAYOUT, SDK_POOLED_IO, SDK_SESSION_IO, CURRENT_HOST, FINISH_PLAN,
TRUSTED_BOOTSTRAP_COST, LEGACY_UIA_BASELINE, DOCUMENT_SELECTION,
PRIVATE_CLIPBOARD, PRINTING, STORAGE_CLEANUP (all under LeaForge).
