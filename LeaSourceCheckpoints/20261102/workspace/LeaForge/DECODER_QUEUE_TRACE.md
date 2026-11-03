# Decoder Queue Investigation

Updated 2026-10-25. Diagnostic candidate only; no release gate closed.

## Reproduction

Ordered-flow core D0A80611 with literal-text view 9D9C7A39 failed the
retained full suite at the decoder deadline (receipt 921E6A0C). A separate
inverse-checked diagnostic overlay preserves private Read/Open/native policy,
deadlines, semaphore ownership and dedicated-thread scheduling. It records
wait, admission, dispatch and thread entry, bounded by the existing 512-character
diagnostic limit. Original Run signature remains available.

Diagnostic view SHA256:
`4D060C015B78486A551E800C06DC21C75BF94F1916EB381855FAD1176D617CA2`.
Controlled result `continuity-evidence/decoder-queue-trace-v1/result.json`
SHA256 `6AF56EF9D858E49A60FA050CED05A2430443C4AA8588B371CAB57BDA3E50BC0D`
passes the original 21 controls plus 32 diagnostic checks, with no native launch.

The native-only subset passed in 38302 ms, receipt F760B078; its raw aggregate
remains false for two fixture ACE changes. Read-only classification AA51DEE4
matches exactly the fixed decoder SID's explicit RX/Synchronize grants and
reconstructs baseline descriptors in memory. No ACL or security policy changed.
This subset is not a replacement for the full gate.

The full unchanged test executable reproduced the failure in 33026 ms:
`continuity-evidence/decoder-queue-full-v1/result.json`, SHA256
`460A610FCE5E3518D7B9EDB824B6D55611BBA7F505EE8236DA9CEE28E6511202`.
It exited 1 without outer timeout/kill. Failing request wall=2993 ms,
queue=wait, admitMs/dispatchMs/threadMs=-1, worker=0, In/Iw/Ik=0.
This localizes the observation before post-admission marking, not that request's
helper launch. It does not yet identify a prior owner or prove root cause.
Inputs and candidate bytes held. All three foreground owners retired with
zero leases and no unverified-child flag; this is not independent descendant
or decoder-profile cleanup certification.

## Remaining Work

Fine launch diagnostics now pass all 82 controls (prior71+11new).
Receipt `decoder-launch-proof-v1/result.json`, SHA256
`74FE78B4FC8BC9AEA16AF41596421CBD80B2B8C44AE61D99B549205D5A1EB3CB`;
view `2C184E108839CCAAEEAF4B7A1A2E92E73162EC7DCD67A18F687FB73F0980163B`,
core52CE. Inverse-checked overlays add elapsed checkpoints for preparation, state
lock, previous-worker drop, profile, ACL, pipes, attributes, native create, guards,
job assignment, resume and return. The observer is thread-local and exceptions
are isolated; per-owner event text is capped180 characters while latest phase
continues updating. Combined report remains512 bounded. No launch/security/
retirement policy changes. Controls test thread isolation and bounded storage.
Full diagnostic attempt V1 passed pictures, then failed an existing report
contract: parent token verification was labeled `guards=`, which the malformed
hello test correctly reserves for helper handshake guards. The helper was
rejected before admission; this was diagnostic ambiguity, not a security bypass.
Receipt `decoder-launch-full-v1/result.json`, SHA256
`8A1C3139D65FC5947FC9699E9967E6B591EB585EBE6AE2D0A07AF0E1CB6352B8`:
exit1/44978ms/no timeout/kill, inputs/runtime bytes held, raw ACL deltas retained;
foreground owner retired0/zero leases/unverified=false. Sample parent launch:
prepare0, profile15..39, ACL39..41, create41..228, job228, resume249, return259ms.
This successful launch sample does not explain or resolve slower earlier runs.

V2 renames only this stage to `token`, preserving all handshake assertions.
A new control checks the report distinction. All83 controls pass, receipt
`decoder-launch-proof-v2/result.json`, SHA256
`EEB2A55F52143D5F4A8170339CCA18308A6D5ABB15E33680012F2EC623D6EC8E`;
view `B2C1A05ABC9772857F69C9B6AE34C9E1CB69E51DFC35E8F03F60F70F4A523E6D`.
Control owner retired clean, inputs/runtime bytes and ACLs unchanged. Full V2
child passed in140503ms with exit0, no stderr/timeout/kill, same test EXE/fixtures
and original300s budget. Final source-exact success footer is present once at
the end. All revised nested model and geometry assertions were reached/passed.
Raw receipt `decoder-launch-full-v2/result.json`, SHA256
`A06A2CD21986FFBDF882753FB5D639CF4112B24F5686A27385F3A9268CEAC3DE`,
remains false: the runner incorrectly applied StartsWith to the entire log instead
of its terminal line, plus three fixture ACL deltas. Neither result is overwritten.

Read-only `outcome-observation.json`, SHA256
`FA96EC2A08E8CEC9E2F10244F7EE078223A10DFF43924EB671B44BBEE223DF92`,
checks exact five candidate hashes, matched source/test overlay evidence, exact
terminal literal footer, exit/drain/deadline conditions and retained input hashes.
Each ACL delta is exactly one explicit fixed decoder SID RX/Synchronize ACE on
Image, Handshake or Tests; removal only in memory reconstructs each baseline.
No filesystem permission/policy change or decoder-profile deletion proof.
Foreground owner retired0/zero leases/unverified=false. This is one behavioral
pass, not a fix for the earlier intermittent queue/native startup failures.

`Test-LeaSuiteFooter.ps1` now checks a single exact terminal footer. Nine controls
pass (receipt `suite-footer-proof-v1/result.json`, SHA256
`ADD7618BCBFDB7F00D191B705AF42BDA287B0EEBD5D4581197BB1F3353BA0E4D`),
including successful and failed retained logs, duplicate/early/extended/empty
footers. A corrected future V3 runner is prepared but NOT executed; no unchanged
warm rerun was used to replace this bad summary. Host/script results on older
pairs do not qualify core52CE/viewB2C1; that qualification is next.

Verified local V2 archive `decoder-launch-source-v2.zip`:69entries/2232894bytes,
SHA256 `5A1F5B2679C4DFD855DE736002DF895D0379DB1AC88A9341348465C1B9BD2409`.
Includes exact compiled view/control sources, receipts/classification, footer
helper/tests, prepared unexecuted runner and nested prior source/failure archives.
Not portable delivery/off-device backup or release approval.
No warm retry or policy change; V1 failure and source are retained in verified
54-entry/2010923-byte local archive `decoder-launch-source-v1.zip`, SHA256
`976634E098F880748383D4DEDE6B9E714E65127828E2ACC601E6DB65DCD6BB54`.
Archive includes matching source/control copies, traces and prior source evidence;
not a portable build kit or off-device backup. No latency fix claimed.

The revised nested-model full suite failed earlier at the decoder deadline:
`ordered-flow-regression-v4/result.json`, SHA256
`9A1C1B073D72892D652542D9CEAF32C2D5620696B217C5C103309802A597C9C3`.
Same core52CE/view12A9 as the focused native rendering test. Request elapsed3093,
admitMs1120, dispatchMs1120, threadMs1127; native Read wall1904, hello reached at
1768, ready0, worker6508, Ik1/In0. Owner snapshot `12:delayed:stopped`, cancel1.
Unlike earlier queue-only failures, this request acquired admission, launched
and killed a helper before its hardening handshake completed. The worker's
6000ms response delay occurs after its hello in source, not before handshake.
No admission/guard success is inferred from process creation. Launch cost and
prior retirement still need finer attribution; no proven latency fix yet.
Full child exit1/29462ms/no outer timeout/kill, inputs/runtime bytes held; two raw
fixture ACL deltas retained, owner retired0/zero leases/unverified=false.
No warm retry or deadline/security change used. Updated layout full assertions
were not reached. Next inspect fine-grained launch and prior retirement timing.

Owner snapshot overlay added in a separate diagnostic candidate:
view `12A9CAF4880BA5A377018BECEF25FBE27740B8455F02CA3E16F5CA7FB642DF9C`,
controls receipt `C397B4C7BF909BCA2FB5B9031AD2B7D2BB3873C63EDE33FAB877804BEAE56423`
at `continuity-evidence/decoder-owner-proof-v1/result.json`. All 71 controls pass.
The snapshot contains an operation ID, fixed helper category, stage, age and
cancellation flag, with no arbitrary arguments. Native mark and stop callbacks
only gain diagnostic updates; lifecycle/security behavior remains unchanged.
Snapshots are observations, not atomic ownership or independent cleanup proofs.

The one full attempt with this candidate passed the picture section, then failed
`white-space block count: expected 10, got 11`. Receipt
`continuity-evidence/decoder-owner-full-v1/result.json`, SHA256
`6579A3D2C8B021B1D2980816D3387118EA3A3661591754FFD6B8F4A5DC8C434D`.
Exit 1 in 59740 ms, no timeout/kill, bytes/inputs unchanged; raw fixture ACL
changes retained, no classification used to promote this behavioral failure.
No queue-owner failure snapshot was captured. This does NOT resolve the earlier
intermittent decoder failure. All new foreground owners retired cleanly.

Owner source/evidence backup: 42 entries, 949780 bytes, SHA256
`A253F1718C907EB63ED376D38190CCB612628F70D57A475DF75DB8B8B1D716EA`,
`continuity-evidence/decoder-owner-source-v1.zip`, verified member hashes/local
only. Includes prior source archive; not a portable kit or release certification.
Ordered-flow V5 is now testing a scoped fix for the extra unpainted wrapper.

Controlled overlap regression `Lea.Tests/DecoderRetirementProof.cs` passes 11
new checks plus the prior 21+32 controls. Receipt:
`continuity-evidence/decoder-retirement-v1/result.json`, SHA256
`D422D0E8EB89783C99B2583F90CEE31AEA459337E51FD0077A3AF5EB4703E8D3`.
Child exit 0 in 753 ms, no native helper, inputs/runtime bytes and ACLs unchanged;
foreground owner exited 0 with zero leases/unverified=false. It demonstrates
that cancellation does not release an unretired owner, canceled waiters cannot
steal its permit, and the next live request executes once after retirement.
This uses controlled barriers, not timing sleeps or a native-failure reproduction.
The rebuilt view D27411E7 has all six sources byte-identical to diagnostic view
4D060C01, but different binary bytes; native/full results do not transfer to it.
Compiled proof sources and generated runner are retained in that evidence folder.

The immediately preceding fake-helper test has a 1000 ms budget. Its assertion
allows a timeout with zero admission counters; it does not require proof that
an invalid handshake was received. Navigation can finish before late decode
retirement. That is a hypothesis to correlate, not a proven cause of this failure.
Do not release the gate while work still runs, relax deadlines/security, change
the test to an easier helper, or use repeated warm passes as qualification.

Next: correlate queue owner and retirement phase in the full sequence, then fix
the evidenced lifecycle issue and rerun the unchanged matched suite. Host/script
passes on older binaries do not qualify this candidate.

## Source Preservation

Verified local archive `continuity-evidence/decoder-queue-trace-source-v1.zip`:
41 entries, 784011 bytes, SHA256
`3CEB637EBE8212886263C2C47CDBF25FD9A1280BAC0AB1109FC4842BB52F4349`.
Includes compiled view sources, controls, recipes, logs, failures and nested
ordered-flow source. Receipt: `decoder-queue-trace-v1/source-backup.json`.
Not an off-device backup, portable delivery kit or release certification.
