# SDK Image Worker Increment

2026-10-31 standalone net48 project Lea.Sdk/Lea.Image.csproj stages the two
original tested decoder sources without changes. Prepare-SdkImageWorker.ps1
binds them to original receipt09690529... and its explicit source-input pins.
The original FE744BE1 worker targeted net461; this new framework/project build
has a different identity and needs its own runtime evidence.

Staging: I:\LeaBuildStorage\sdk-image-worker-20261031-v1.
New worker:EF89F8AA04E51921B54D1BAFE889244BF39229E57A08FBD7BFCB0D6B5E098BD3.
Build receipt95F4A5D6DC02D49A9A27DFF1245010385FD4BF349ADA253962D895C814E277A0.
ModernSDK8.0.427/sharedcompilationdisabled,ownedbuildPASS;owner22637retired0.
Only Lea.Image.exe is emitted in its Release folder,no omitted config/dependency.

## SDK-Pair Verification

Test-DecoderRetirement.ps1 -SdkPair requires the real new worker and exact SDK
core772A/view8008. It copies them into fresh NTFS test folders;no ACL isolation
claims are made from the exFAT staging drive. Existing assertions,120scompile/
30stest/5scleanup and original process/Job ownership remain unchanged.

- sdk-decoder-retirement-v1:81checks PASS,3701ms. Includes real red/blue PNG
  pixel decoding,isolation flags,invalid-image startup cleanup,post-create stages,
  cleanup retry and restored original worker descriptor. Receipt
  DC95142ED4BA9E7972A24E1A9F07BF0570ACB1AB6053C940C9815C149BBE69C2.
- sdk-decoder-shutdown-race-v1:14checks PASS,3186ms. Includes busy endpoint
  preservation,retry retirement,admitted-task completion and launch rejection
  after shutdown. Receipt
  7868A8864A905EA44F12A6C21BD1A0723612CEFBA4F8BB31B72A5F5B203CD15B.

Both run pairs exit0,no timeout/kill,Job0/outputdrained,inputs unchanged and
original decoder ACL restored. Owners48528/10157 guardedretired0.
Audit-SdkImageWorker.ps1 independently verifies original compiled-source pins,
exact copied/tested product hashes,terminal footers and lifecycle/ACL receipts.
Consolidated auditB815E839FCFC104F617302A05D48AAF7028F163A801F66B2A34FFCF5A3C0B2AA
at continuity-evidence/sdk-image-worker-audit-v1/result.json.

## Remaining Work

Portable61-filelayout andactualVimage-loading now pass42checks,file/dataPNGs at
720/360/defaultbundledworkerpath/hardening/ACLrestore. SDK_PORTABLE_LAYOUT.md
hasreceipt D5313F3D... andscope. Aggregatecombinedsourcebuild remainsunverified.

Not universal security/profile-crash-recovery proof,full UI image-layout fidelity
or complete SDK qualification. Compose portable SDK source/sample/worker layout
and test actual view image loading there. JavaScript parent/adapter/worker,
printing,notices,full host delivery and unresolved legacy UIA/startup reliability
still require work. Browser development ZIPs remain unchanged.
