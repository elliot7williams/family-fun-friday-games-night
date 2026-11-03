# Node Connectivity

2026-11-02 separate candidate; isolated and native-view passes and earlier trusted pass, but
fresh trusted rerun failed the original bootstrap budget. Not browser adopted.

## Matching Worker

Worker build receiptDB8F6F426B4773E821A7930B46BA74B59E46802B540BFFB89DD775B5550AFCB6
at I:/LeaBuildStorage/is-connected-worker-build-20261102-v1/result.json pins
unchanged SDK worker sources except the matching compact DOM resource. Owned
IL15475ms and ReadyToRun21726ms under120s each, cleanup109/36ms under5s,
exit0/inputs stable/owner89907retired0. Six native closure files; exact8.0.33
and rollForwardDisable. Worker DLL8F81649B75A028EB3E98EB79417B5A24F0C36F8903A503C5099C4A9EC8D095F9.

Isolated receipt429BD1EF2F574E657F056C9F8A34D245E34122EBCD98CD95F0FC035294F465AF
at continuity-evidence/is-connected-isolated-v1/result.json: compile3495ms,
run6051ms, cleanup656/48ms, all exit0/Job0/drained/16runtime hashes+ACL stable,
owner37799retired0. Shared Session test verifies existing AppContainer/network/
file guards, 128MiB/2500CPU policy, completed retirement, original semantic
cases, revocation and cycle recovery. No remote scripting enabled.

Shared-harness trusted rerun v3 FAILED before semantic cases: first read returns
limit, bootstrap TimeoutException, stage=events. ReceiptD41035C11C3786A5E1D1A8E77923D6F7A7F1CE09C661DEA224B4CD529F39C025
under continuity-evidence/is-connected-trusted-v3. Exit1/4192ms/clean14ms,
inputs/runtime bytes+ACL stable, owner73493retired0. No retry-to-green or increased
budget. Earlier v2 pass remains tied to its original harness in source113 backup;
do not transfer it to the changed Session harness as a new passing result.
NEXT controlled baseline-vs-candidate bootstrap comparison under original limits,
then native lifecycle and prior DOM regressions. This single failure does not
establish a feature regression or resolve known bootstrap intermittency.

## Bootstrap Comparison

2026-11-02 fixed ABBA comparison uses one identical harness executable, fixture,
configuration, dependency closure and common containment script. Only script DLL
differs: baselineD2685E2F... versus candidateFBD50D21.... All four preplanned
processes ran. Observations: baselinePASS2743ms/281.25CPUms, candidatePASS2599ms/
359.375CPUms, candidatePASS2515ms/187.5CPUms, baselineFAIL2780ms/234.375CPUms.
The final baseline returns limit/phase=bootstrap/TimeoutException/stage=document.
These timings cover construction+Ex, not just the internal bootstrap budget.

Campaign receipt944DD563AF9ECC490139A06DC8E672E38CB884E6A8B47FD276CF69673EDAA9FB
at continuity-evidence/connectivity-bootstrap-comparison-v1/result.json remains
FAILED: runner stops at final killRequested cleanup guard before appending the
fourth observation. Do not rewrite it into a passing/complete campaign. All four
raw reports were independently inspected; final exit1/Job0/drained/83ms cleanup,
owner42178retired0, 20runtime bytes+ACL stable. Evidence audit
3BC563ED3433069DCB5E5B36CE56861CFC935AEBA73CD963378CA850AEE122BC at
I:/LeaBuildStorage/connectivity-bootstrap-comparison-audit-20261102-v1/audit.json
records experimentPassed=false and copied raw evidence with hash readback.

This demonstrates a similar bootstrap-limit failure in the unchanged baseline;
it neither excludes a candidate regression nor identifies scheduling/JIT/GC as
the cause. No limits changed and no retries added. NEXT native isolated-view
integration for the feature while preserving the unresolved trusted startup gate;
investigate the shared bootstrap path separately, not by blaming the getter or
continually retrying until green.

The [DOM Standard](https://dom.spec.whatwg.org/#dom-node-isconnected) defines
connectivity by a shadow-including root that is a document. This candidate covers
Lea's existing document and element wrappers, not shadow DOM, text-node wrappers,
cross-document adoption or complete Node prototype conformance.

## Existing DOM Regressions

2026-11-02 four unchanged focused entries in the original test executable pass
against the exact new parent/worker pair: containment isolated22, subtree queries
isolated21, containment native10, subtree native4, total57 assertions. No test
sources or assertions changed; only candidate dependency and fixture bindings.

Receipt04080131811DD89DC7A3DAAFFC6931DEB4BCD2A8D6A73B0E9EC232D309708DFB at
continuity-evidence/connectivity-dom-regressions-v1/result.json. Runs6847/5726/
4660/4844ms under original focused30s each; cleanup209/74/15/14ms under5s,
exit0/Job0/drained/25staged hashes+ACL stable, inputs stable/owner71036retired0.
Startup trace activated/restored in every process. Desktop/narrow containment
PNGs manually inspected: retained editing controls/content fit both tested sizes.

Audit13A562AAD319ED613065BA6FC6F2C2347D8934B788B5A1911C8BBAF3068130C4 at
I:/LeaBuildStorage/connectivity-dom-regression-audit-20261102-v1/audit.json
independently checks exact57counts, original ownership/deadlines/current files and
copies logs/images with hash readback. This is not selectors/tree/attributes/
class-list coverage, full suite, trusted reliability or host adoption.
NEXT those broader prior DOM suites, then matched SDK/host qualification. Keep
the baseline bootstrap failure and all existing release gates unresolved.

2026-11-02 broader suite attempt FAILED immediately in original Selectors.Run:
first trusted code reports limit/bootstrap/TimeoutException/stage=document.
Original compiled test EXE6E46584E... and assertions unchanged, reflection driver
only. Tree, Attributes and ClassLists were not reached; do not label them tested.
Receipt9CE53E15E1DACF8DF67402CF50F992CB55F02777732AE423ECC7B75C0B89AB0A at
continuity-evidence/connectivity-broader-dom-v1/result.json. Compile2726ms/120s,
suite3901ms/30s exit1; cleanup58ms/5s/Job0/drained,19runtime bytes+ACL/inputs
stable, owner37986retired0. Cleanup killRequested remains true. A restored marker
was printed but the startup-trace restoration gate is false; retain that boundary.
Failure preservation audit610E8BB20A0EBBC62F5B024E9CE4E30684CBA2F58F048EFEB5E1E93030D2A371
at I:/LeaBuildStorage/connectivity-broader-dom-failure-20261102-v1/audit.json
rechecks terminal ownership/current inputs/files and copied failure evidence.
NEXT shared trusted bootstrap cost diagnosis under unchanged limits, then return
to the pending suites. No feature regression cause proven and no browser adoption.

## Native View

Native-view receiptE7692D6487E00789EF94D591B8EA83D9E4331C7B34FB2D6A9BBEEC3A3AEA08BF
at continuity-evidence/is-connected-native-v1/result.json verifies the exact
candidate parent/worker with original SDK core/view/adapter. Owned compile3941ms/
120s, run4955ms/30s, cleanup164/78ms under5s; exit0/Job0/drained/18runtime hashes
and ACL stable, owner83557retired0. Local inline scripts read connectivity;
native focus/activation edits a field then triggers subtree removal. Tests verify
retained field value, detached editor cleanup, nonmutating connectivity read,
AppContainer/network/file guards, desktop/narrow nonblank rendering and exact
worker-handle exit on view disposal. Original view5000ms worker limit retained.

Connected and detached940x700 and detached420x700 PNGs manually inspected:
expected text/field/status/button visible, no apparent overlap/clipping. Evidence
auditF5C0634F63FB8336DF5C3D6956BB1BAC57C56991E49DCB198109FCF234F844DC at
I:/LeaBuildStorage/is-connected-native-audit-20261102-v1/audit.json rechecks
current files/ACL/inputs, owned limits/footer and copies logs/screenshots with
hash readback. This is not the Firespider EXE or a full rendering fidelity test.

NEXT run unchanged prior DOM regressions on the exact new parent/worker pair,
then matched SDK/host qualification. Trusted-bootstrap intermittency remains an
open gate; native success is not permission to silently adopt the candidate.

`Prepare-IsConnected.ps1` projects mature Dom.js11848A22... and compactEC4B6513...
into I:/LeaBuildStorage/is-connected-source-20261102-v1. Source receipt
B4BEA8392042E88DE4D833AA158B35869811A95AC9D5D8CA5AF8D02B53FA359E.
The getter uses branded `nodeKey(this)` and existing bounded `co` document
containment. No bridge command, write capability or remote scripting added.
Readable and compact projections have exact inverses; six negative controls
reject missing, duplicate and already-projected inputs. All three JS sources
(both bootstraps and IsConnectedCases.js) passed syntax checks.

Owned build receipt2596ED5B... at
I:/LeaBuildStorage/is-connected-script-build-20261102-v1/result.json binds the
new DLL FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C.
Original engine/native launcher C# and dependency identities retained; only
compact DOM resource changes. Diagnostic retirement source was NOT included.

Trusted test receipt
210F1DBC79DB6D600EFDEEB9037E715452C8FA34B332E7F4D478ADF6B78BCCE1 at
continuity-evidence/is-connected-trusted-v2/result.json: owned compile2974ms/120s,
run4881ms/30s, retire139/38ms under5s, exit0/Job0/drained/inputs+bytes+ACL stable.
Owner24194 retired0. Tests cover live nonmutating attached reads, document
receiver, enumerable/configurable getter without setter, illegal receivers and
proxy trap avoidance, strict readonly write, captured intrinsics, detached
subtree reads preserving containment without granting writes, revoked scripting,
bounded native parent cycle and recovery.

V1 failed before getter execution because the new test EXE lacked dependency
binding redirects: retained receipt295AA1EA.../owner46160retired0. V2 adds the
original pinned BBBBB19D... test configuration, no engine/assertion changes.

Next: investigate the failed trusted rerun, qualify
native lifecycle behavior, run prior DOM regressions, and perform
matched SDK/host/package qualification before adoption. The tested development
ZIP remains unchanged. Intermittent scripting timeouts and all six release gates
remain open; this result is not all-sites or full DOM conformance proof.
# 2026-11-02 Matched Global Installation Candidate

Feature semantics now PASS both modes on new parentF5BDB1FF.../workerA72EF9C0...,
not transferred from older feature binaries. TrustedFC427EDD... and isolated
75E76ACE... receipts at continuity-evidence/global-install-feature-{trusted,isolated}-v1.
Original harness8AE71E18.../casesAE7B3FD5... unchanged, original budgets/sandbox flags.
All4owned jobs0/drained/exit0/no kill/timeout,10/16runtime bytes+ACL and inputs stable,
owners3357/43716guardedexit0. AuditC8094177... verified exact candidate/raw logs/readback.
See GLOBAL_INSTALL.md for full pins/timings. Browser adoption, broader DOM/native-view/
host/full-suite and reliability qualification remain pending; all6release gates OPEN.

# 2026-11-02 Matched Native View

Matched global-install parentF5BDB1FF/workerA72EF9C0 native V case PASS EB81FF97...
at I:/LeaBuildStorage/global-install-shared-native-20261102-v1/result.json.
Inline lifecycle, real focus/activation, edited field retention, detached editor
cleanup, nonmutating detached reads, sandbox guards and exact worker exit verified.
Compile3872/run5211ms, retirement192/135ms, both0/Job0/drained/no kill/timeout,
23runtime bytes+ACL/inputs stable, owner50389exit0. Three desktop/narrow PNGs
manually inspected; nonblank/expected states. AuditFA35B432... verified raw evidence,
PNG identities/dimensions and unchanged original21 plus2native driver files.
GLOBAL_INSTALL.md full pins. ClassLists bootstrap timeout remains a failed campaign;
this native pass does not close startup, Firespider host or other release gates.

