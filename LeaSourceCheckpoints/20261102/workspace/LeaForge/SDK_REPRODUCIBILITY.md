# SDK Reproducibility

Validated 2026-11-01 on dirty base535add5. No runtime engine source change.
The SDK-only Directory.Build.targets enables deterministic builds and maps source
paths to /_/lea/. Existing explicit PathMap remains honored. Short build aliases
are unchanged. This does not claim faster runtime or smaller engine binaries.

## Exact Evidence

All receipts below are under I:/LeaBuildStorage/ and have verified SHA256:

- sdk-build-identity-20261101-v1/result.json: 1459B734A83DB5C498642696EF562F1653D1A97974242EACB7DCDA3009AFAB90
- sdk-identity-reader-controls-20261101-v1/result.json: F037C1A81C4C4A54DBF4075AE87FAE3E8C9042203339E62974B42D9F4CED457B
- full-sdk-source-20261101-v3/source-manifest.json: B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A
- sdk-reproducibility-20261101-v1/result.json: 39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199
- repro-sdk-consumers-20261101-v1/result.json: B8AA0C9ABCA57492875D09471D29AE809C17D89E748C2ACF3FD389161360C927
- sdk-reproducibility-audit-20261101-v1/result.json: DE31C1443C5FF646397B66FD0361F29695D2689FA703F5BDE2BB08E473174F0D

Earlier Framework images differed only in timestamp/MVID; worker debug metadata
contained absolute paths. Metadata masking was diagnostic only, never acceptance.
Reader controls: three positives, two negatives. New acceptance compares raw bytes.

Two fresh 132-member source trees at different directory depths each completed
Ba with the same pinned compiler/references/native toolchain (83389/83204ms,
120s budgets). All 16 outputs match byte-for-byte, including worker PDB and R2R
closure. Worker CodeView path is /_/lea/Lea.Script.Worker/obj/Release/Lea.Script.Worker.pdb.
No cross-machine/different-toolchain claim is made.

Fresh NTFS consumers passed text14, script17 and worker25 unchanged assertions.
All six owned child runs retired cleanly; original deadlines, isolation and ACL
checks remain in force. I: exFAT is staging/backup, not native ACL qualification.

Source package: portable-sdk-source-package-20261101-v2/Lea-full-sdk-source-development-v3.zip,
SHA84118D67704FC3DB8D941E3384C2B624D4A0B56832F437F0B91A2FBD34E5FC0E.
133 ZIP entries: 132 source/dependency members plus manifest, no Lea product
binaries or local toolchain configuration; eight third-party dependencies.
Notices/redistribution review remains open. No GitHub upload verified.

## Remaining Work

Fresh replay WITHOUT added Contains checkpoints PASS3412/all89phases298514ms,
receipt continuity-evidence/deterministic-full-replay-owned-v1/result.json SHA
C9A3C41F5A76A33C7D792B3F7FBC34ED3D7E55BAE64A8844B49C812FE4D3A4CE.
Program/Contains original bytes; engine images unchanged, only3fresh embedded
fixture path/manifest/dispatcher identity bindings.65runtime bytes+ACL stable,
trace restored/no recovery markers, exit0/Job0/drained/253ms retirement, owner95524
retired0. Audit I:/LeaBuildStorage/deterministic-full-replay-audit-20261102-v1/
audit.json SHA3C1053D56C2743A48B546D04E27A52DE84E6882D01A1D9C171B9F5AB2A527F80
rechecks source markers/fullfooter/178phase markers/process limits/current hashes/
ACLs and3negative controls; rawlogs retained onI:. Only1486ms deadline margin.
One pass is not reliability/causeproof; earlier failures remain. Candidate has a
full scripting pass, not Firespider adoption/clean-PC/all-site/release qualification.

Full-prefix recovery diagnostic now PASS3412/all89phases290304ms, receipt
continuity-evidence/contains-recovery-full-prefix-owned-v1/result.json SHA
1D0BCC947674AED3C0CA66295365CAD009FD474F8F0CB9736AF4D5F21229F7A7.
Original300s/5s limits, retirement281ms, exit0/Job0/drained, owner1242retired0;
65runtime bytes+ACL stable, trace activated/restored.76recovery markers show
isolated cycle limit94ms then recovery1709ms; bridge limit507ms then reset1110ms.
Audit I:/LeaBuildStorage/contains-recovery-prefix-audit-20261102-v1/audit.json
SHAF9708B3EDE53F4771A8646268D85AAED383DF8A5BE50DDB7DB24D9A65E8BD3EE
checks exact hashes/ACL/process limits, both19operation traces,3negative controls,
and preserves rawlogs onI:. Negative-fixture array construction corrected via
named cases before auditPASS; no runtime/assertion/parser weakening or rerun.
DiagnosticOnlytrue/releaseQualifiedfalse: observations do not explain/fix earlier
fullv8 timeout. Next fresh uninstrumented replay, exact engine images/original
Contains, new embedded entry fixture binding; preserve both current evidence sets.

Full-prefix diagnostic prepared/compiled (NOT executed): entry proof
5D9D93F187EBC88C34929855DF0E94C082839F8A1B66EA05E3EC52BE58020C79 under
I:/LeaBuildStorage/contains-recovery-entry-20261102-v1, embedded fixture points to
fresh contains-recovery-full-prefix-v1, never quarantinev8. Assembly proof
AFFDEA3E82AE42779C1ABA5292D8E40847D72CB4A8C4E8B61F0B1E99E2BE196F under
contains-recovery-assembly-20261102-v1.305members/4exact diagnostic changes;
Contains9inserted lines remove to original bytes, dispatcher entry-pin inverse
verified. Engine products/all phase calls/assertions unchanged. Compile proof
E46CFBB42E468672220B4197AFC9CE6F85AE2832CD025D86A52C9FA01D8BCCC8 under
contains-recovery-compile-20261102-v1;8796ms/120s, cleanJob/owner81858retired0.
New diagnostic suiteD0D5B26F... . Next explicit diagnostic staging/audit/owned
runner switches with fresh paths/pins, original3412/89/300s/5s and no release
qualification. C:654372864 currently below reserve+61MB copy requirement.

Prepare-ContainsRecoveryTrace.ps1 now emits test-only checkpoint source:
I:/LeaBuildStorage/contains-recovery-trace-source-20261102-v1/source.json SHA
8D3789D0D3B41D1B6057A4186CA461BD7A113F0C43CB727C59AF5C87D78E6BAB.
Four source replacements, exact inverse/readback and3negative controlsPASS.
Original code, assertions, Tm5000 and engine unchanged; only step ordinal/time/
typed-status stderr markers around Code/cycle/bridge calls, no page data.
NOT compiled/executed. Timing perturbation is possible. Integrate with fresh
full-prefix runtime/new embedded entry fixture directory/manifest+dispatcher,
all60sources/0..88phasecalls and original300s/5s guards; do not reuse quarantinev8
or substitute this diagnostic for unchanged full-suite qualification.

Focused existing --contains isolated now PASS22/7631ms, receipt
continuity-evidence/deterministic-contains-recovery-v1/result.json SHA
C191213E849FF62A2CE19B14D59351692B724074B41B3B851B9EA199BB0D2CC1.
Test-DeterministicContainsRecovery stages23files14.4MB from immutable source
origins, exact fullsuite executable6E46584E..., no recompilation/test projection.
Original typed-cycle limit/reset, detached ancestry, bridge exhaustion/reset and
revocation assertions all pass. Bytes+ACL stable, trace restored, exit0/Job0/
drained, owner35750retired0. Original30s/5s guards unchanged. This separate fresh
process lacks full prefix history, so fullv8 timeout remains unqualified/unfixed.
Next full-prefix recovery checkpoints, no production changes without evidence.

Phase-cost comparison BC5D5182A344947EFD4091E717B52B66960C9E79280324EAF994CE2AD357D4D3
under I:/LeaBuildStorage/deterministic-phase-cost-comparison-20261102-v1 compares
prior fullpass7F1448D5... with current timeout. Identical Program source/89marker
regions;86completed comparisons and5negative parser controlsPASS. Current sum
completed phase durations is4308ms lower. Prior remaining86..88 total6635ms,
phase86alone4819ms, versus current86>15s. Contains.cs byte-identical809837EC...;
last assertion typed cycle limit, next operation reset/recovery. This identifies
a diagnostic target, not causal proof: products/paths/run conditions differ and
there is one observation each. Focus fresh exact-image isolatedContains recovery,
then full-prefix corroboration; preserve quarantinedv8 and all original limits.

Fullsuitev8 now compiled60sources8415ms (proofB23E98B3...), staged61MB with6
manifests/37closurefiles (90FD0D60...), embedded fixture auditPASS59A3BEE3... .
Owned run FAILED original300s at phase86 isolatedContains, raw receipt SHA
FC796BCEA5B95CF5B509CCE4661B8BBC356EC4C2C0FF93DE8DC6D0CC0E75ADD9 under
continuity-evidence/sdk-full-script-owned-run-v8/result.json.86completed phases
(0..85), no full3412footer/89qualification. TimedOut/kill/exit57005, Job0/drained,
retirement477ms/5s, owner85930retired0.65runtimebytes stable, sevenACL deltas
(image + six worker closure files) retained untouched; trace restoration not
proven. Candidatev8 quarantined, no profile deletion or earlier-failure fix claim.
Audit I:/LeaBuildStorage/deterministic-full-script-timeout-20261102-v1/audit.json
SHA67ECA68F42508936C7FE87DFBD0811BC120EE8EB629DD0887AD478F838C48B4C
preserves rawlogs/receipt and all original/current SDDL. Next map phase cost and
diagnose performance/cleanup without changing original assertions or budgets.

2026-11-02 assembled full scripting candidatev8:305verifiedmembers including60
sources and182fixtures, eleven exact deterministic role products plus fresh
probes/entry companion/host. Receipt I:/LeaBuildStorage/
sdk-full-script-assembly-20261101-v8/assembly.json SHA
D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB.
Shared bridge common-original inverses and prerequisite runtime gates retained;
Version7 path preserved. No baseline test binary accepted as new compilation.
Original120s/300s/5s and3412checks/89phases unchanged. Compile attempt stopped at
initial600MiB reserve with C:522219520bytes: NO child/output, owner89728closed0.
Next compileVersion8, exact staging/audit and full campaign. Not yet runnable.

Outer entryhostVersion8 now compiled: full receipt5FE22B8F112411009E9C7DF70654A372480FD7EBD3142E71BBDF3E15E7100F56
under I:/LeaBuildStorage/sdk-full-script-entry-20261101-v8; focused receipt
34DDBDD42C53357A260614A5C802491B1442464F810BEFAAB795C5B0BA2BF5DA under
sdk-entry-host-focused-20261101-v3. Both retain parent/worker source inverse and
negative controls, clean compiler owners. Test-SdkEntryCore-Version3 PASS15,
receipt continuity-evidence/sdk-entry-core-runtime-v3/result.json SHA
32257313C2E49F7CBE553CA8B29B32F28759C8495C1B5A23952B5AD5CBCFB0C3.
Runtime4474ms/30s;10NTFSfile hashes+ACL stable, child Job0/output drained, owner
22051retired0. This interpreter-free focused fixture does not evaluate page code
or prove the retained combined startup failures fixed. Fullsuite assembly next.

Deterministic component runtime PASS167 (21/24/15/93/14), receipt
continuity-evidence/sdk-component-probes-runtime-v4/result.json SHA
44FFA4F443AC7680721CE56E86334FD1A1D7708049C78660B24C9D4C2DEC53BC.
All27NTFSfiles retain bytes+ACL, five jobs exit0/Job0/drained, owner97064retired0.
Explicit component-only scope: worker main/page evaluation/native isolation false.
Entry companion source-v3 inverse+3negatives verified62A0530A..., build-v3 receipt
444492AA954130B23FA4E9275FB14F3888BED75FF10BDC859C7243BD7D610D95 under
I:/LeaBuildStorage/sdk-entry-core-20261101-v3/result.json.3outputs, new companion
E08BFB31..., inputstable/clean compiler job/owner13471retired0. Not yet executed.
Next outer hostVersion8 must select bindingsv4, companionv3 and exact deterministic
parent/worker while retaining inverse/negative controls before full-suite assembly.

Full-script integration preparation (NOT main-suite/runtime qualification):
- sdk-component-probe-sources-20261101-v4/sources.json: AF66E1B7EC800A6C155C676E92E65B99A2F732151A55F82822505F7A328E9912.
- sdk-full-script-bindings-20261101-v4/bindings.json: 206B9E49C275F6305DDEFB5DEC7448502C8873001E0E24095B68C7A311A92A3D.
- sdk-component-probes-20261101-v4/result.json: 97CD0F8118FF656CB7A14B5F2451EC87CE3F7A3B0627B22B5D757D3D76C352D7.
All under I:/LeaBuildStorage. Six projected sources retain inverse+18negative
controls; eleven selected SDK products match both builds; bridge/entry identity
projection preserves assertions and original constraints. Five probe hosts compile
with original120s/5s guards, all jobs exit0/Job0/drained, owner11514retired0;
dispatcher five-manifest inverse verified. No engine runtime source change.
Next: explicitVersion4 runtime probe gate, entry host build, planned freshv8
full-suite assembly/compile/staging and owned300s campaign. Older runtime passes
do not qualify these probes. Both startup timing failures remain unresolved.

Decoder follow-up now runtime PASS on exact current core/view/image outputs:
- Retirement81: continuity-evidence/repro-sdk-decoder-retirement-v1/result.json,
  SHA B5F32DE4B111774F42598D2C7C02564AE02F46893A8227499E910104643F3446.
- Shutdown-race14: continuity-evidence/repro-sdk-decoder-shutdown-race-v1/result.json,
  SHA 07292AFCE9F295A726D31B67E826D0A99C707FB5ED8F4816D904219B1850CC0B.
Real decoding/red-blue pixels and missing-sidecar retry exercised. Worker ACL
restored, inputs stable; four owned children exit0/Job0/output drained under
original120scompile/30stest/5sretirement limits. Runtime2992/2798ms. Owners63459/
44247retired0. This does not qualify full scripting, host migration or all security.

Relative rendering now PASS142 in freshv3, receipt SHA
557F2F1CD4BE6ADEAA950BF7D4CA35BE41D77E9B65F57BCFBE0B6EEACB42B8EB at
continuity-evidence/repro-sdk-font-relative-render-v3/result.json. Inputs stable,
owner16036retired0, original assertions/deadlines. Narrow em360long image inspected.
v2 rendering never started: compiler exit0, retirement7157ms exceeded5s; raw failed
receipt CBB90732... kept. Original-handle recovery35ms67E0AA11... confirmed Job0/
drained and disposed retained resources; owner6786retired0. v2 remains FAILED.
The four-campaign audit below predatesv3 and does not independently qualifyv3.

Continuation update: tool input works again; old90861 handle absent, new95203
owner retired0. Relative-render-v1 stopped on storage reserve with ZERO child runs,
before compilation, inputs unchanged, ownerAttention false. Receipt SHA
5BA1C150954F5D666FF7D50548F726C744A2FC8C157FEE29D868F12860A50E41 at
continuity-evidence/repro-sdk-font-relative-render-v1/result.json. Preserve this
attempt; use -Version2 next. Initial C:986MB fell to315MB during preparation.
No engine failure/pass inferred. Stable space is now the blocker, not tool stdin.

Audit-ReproSdkFonts.ps1 verifies all four completed campaigns, exact current
core/view/input hashes and eight clean owned runs under original120s/30s/5s
limits. Seven malformed guard/footer negatives PASS. Audit receipt:
I:/LeaBuildStorage/repro-sdk-font-audit-20261101-v1/result.json SHA
7657573C3BDCDA8417A41BD80070EF114A188173F1A667B37396694539E0266A.
It explicitly leaves relative rendering and idle owner90861 retirement unproven.

Additional font follow-up (exact deterministic pair, unchanged assertions):
- Initial model5controls/7cases: 01A7B092792593807EDD89BEBF221C80BA8F6F8F56D894FFC83FF0F7C05689D6.
- Relative model6controls/12cases: CD0A75CA9B30C07B54EF47181128716C1EF998D5734305797AC63CEAA74E7610.
- Initial rendering142checks: 964771AB5BFCA608E3E114C7A8FD9625DDB72D86F687E487CB90A1E6FC4A49F7.
Receipts continuity-evidence/repro-sdk-font-MODE-v1/result.json. All inputs stable,
six child jobs exit0/Job0/output drained. Runtime972/1704/24342ms within30s limits.
Initial/relative terminal owners retired0; initial-render owner90861 is still
idle because tool input is rejected by policy. User asked to close visible idle
terminal. Relative rendering not launched; no complete font campaign claim.

Font shorthand runtime follow-up PASS12controls/14cases, unchanged assertions:
continuity-evidence/repro-sdk-font-shorthand-v1/result.json SHA
7D7A4F356177EF62527A95101B8595EB95F6444178FFE236527D942B69124B2D.
Inputs stable, compile/test exit0, clean retirement, owner retired0. Remaining
initial/relative model and render modes are still NOT qualified on this candidate.

Decoder candidate mode prepared (NOT runtime qualified): Test-DecoderRetirement.ps1
-RealDecoder -SdkPair -AggregateSdkPair -ReproSdk, with separate -ShutdownRace run.
Both require retained NoExit owner, fresh per-mode evidence, >=600MiB C: reserve.
Core/view/image hashes from both deterministic builds are held and verified;
compiler/runtime/cleanup tail matches checkpoint78. Syntax check passes.

Font candidate mode prepared (NOT runtime qualified): Test-FontShorthand.ps1
-Candidate -SdkPair -AggregateSdkPair -ReproSdk. Separate runs cover default
shorthand, -Initial, -Relative, -Visual -InitialRender, -Visual -RelativeRender.
Each requires retained NoExit foreground owner and >=600MiB C: reserve. Both
build copies of core/view are pinned before fresh NTFS staging; compilation,
assertions and cleanup tail compare byte-identically with source checkpoint77.
PowerShell syntax passes. No successful font runtime claim for this candidate.

Print follow-up PASS: continuity-evidence/repro-sdk-print-runtime-v1/result.json,
SHA409F1A2DBC9E27E398631976D26D1B4F02DB9E8E0279BA414FF560833A499CA4.
Test-SdkPrintRuntime -ReproSdk -Images pins both source-built copies of core/view/
print/image products; copies exact candidate to fresh NTFS and reuses unchanged
test executables/assertions. Capture/range/image runs exit0 in18716/6593/19404ms,
each30s; clean Job retirement/output drain, unchanged inputs and restored worker
ACL. Range37/geometry16/native-selection27/ticket22, baseline40/friendly46/live250.
No print job or host routing. Narrow preview inspected; physical printing remains
unqualified. C: dropped to268MB, below600MiB preflight, so further native runs wait.

Image-view follow-up PASS42: continuity-evidence/repro-sdk-view-images-v1/result.json,
SHA C4969F8C1F0908D0B9B0DB5D1630C06A15C90ACE410503E554C8168B7288C3A8.
Test-PortableSdkImages -ReproSdk checks pinned products from both builds before
fresh NTFS execution. File/data images at 360/720px, decoded/rendered pixel and
worker hardening assertions unchanged. Compile/test exit0, inputs unchanged,
worker ACL restored, retained owner retired0; data360 screenshot inspected.

Run broader regressions against these exact deterministic products before host
adoption. Older font/image/print/full-script successes do not qualify new images.
Both retained startup/trusted execution timing failures remain unresolved.
All six FINISH_PLAN release gates remain open; no defensible completion percentage.
