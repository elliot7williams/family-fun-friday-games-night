# Matched Memory-Policy SDK

2026-11-10 complete source build of framework libraries, native worker and samples,
not a finished/release-qualified SDK. Prepare-MemoryPolicySdk.ps1 stages128source
members/eight third-party dependencies; only E.cs/P.cs/V.cs differ from baseline.
No seeded Lea binaries. Source receipt21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183,
preparation7DA2E84D, original test assertion/helper-path inverse verified.

Source/output: I:/LeaBuildStorage/memory-policy-sdk-source-20261110-v1.
SDK.Build.proj actual Bl/Bi/CompileWorkerNative/Bs targets build six framework
products, six-file native worker and samples. Exact8.0.33/Disable runtime and
authenticated8.0.32 native compiler retained, no IL fallback.

## SDK Checks

Receipt467A8668167F7693DFE076436D74B806A10ED1EBEAB7E81BFE580C40CAC1EAF7,
auditFD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC
on I: memory-policy-sdk-audit-20261110-v1. Nine owned runs pass, original30s/
120s and5scleanup,exit0/Job0/drained/no timeout/kill,inputs and28NTFSruntime
bytes+ACL stable.14embedding/17script-sample/25isolated-worker checks pass.
Owner73601 guarded exit0. All source inputs and native PE/cache/runtime policy
independently audited. Not full3412script/reliability or browser qualification.

Core B11754880B8F767C789D1385DE2702FF15EAD92CFDBECB1CE02DC145F28F3DD5,
view C83E47CC6787C8814E36383B83AD0D2DC5E2726ABF3086678AEA6DC0F4F19C22.
Image DA3B/print414E/parent406C/adapter58E4 and native workerF61B rebuild to their
earlier identities. Dependency changes in core/view still require matched
behavior tests; unchanged dependent DLL hashes do not transfer old passes.

## Memory Checks And Storage Resume

Fresh NTFS runtime: continuity-evidence/memory-policy-sdk-memory-v1/runtime.
Original12model+17view checks pass on B117/C83, but final7policy test was never
started because original512MiB child reserve stopped the runner. Partial receipt
859E8E8910AEEEC4559050DA6323239E365E82130E2FCF59880950BCF5108CDA stays FAILED/
incomplete; five owned runs passed,inputs/five product bytes+ACL stable,owner62557
guarded exit0. C: observed4.3MB free afterward, then1.038GB after owner retirement.
Pagefile remained1629224960bytes. Cause unproven; no cleanup/settings changes.

After650MiB reserve revalidation, Resume-OwnedSdkMemoryPolicy.ps1 pins that exact
partial receipt, rechecks all inputs/runtime bytes/ACL/retirement and runs ONLY
the previously unstarted policy executable, same30s/5scleanup. No compiler or
passed test replayed. ResumeFF94C29F1F28DABD0861FE6CA477188EC6CAABDA5FA80AC0091BA9A415E6987B
passes7checks in775ms/retire178,exit0/Job0/drained/input+product stability;
owner21276 guarded exit0. No budget/reserve waiver or rewritten partial result.

Combined auditAB308FD6B83F6C0B41A258175DBC66A0E45BC157EBBED0D90A94F35FF024D62C
on I:/LeaBuildStorage/memory-policy-sdk-memory-audit-20261110-v1 proves all six
memory probe runs across stop/resume, original assertions, exact SDK pair and
product bytes/ACL. Totals:56SDK consumer/worker +12model+17view+7policy checks;
15owned runs across SDK/memory phases. Not an engine-completion percentage.

## Next

Original full scripting now PASS3412/all89,279932ms/retire657,300s/5s unchanged,
65runtime/input bytes+ACL stable and trace restored; run77DFD17A/auditD4F27CA1.
SDK_SOURCE_FULL_SUITE.md exact preparation/pins and initial generator failure.
Next source checkpoint and matched Firespider source build/actual EXE/workflows/
printing/extraction/lifecycle. Preserve all preceding startup failures and old
86DB/F05E policy reproduction. Review modern script/resource/fragment/session/
split policies. Source/evidence locally backed up on I:; GitHub checkpoint for
this new guarded SDK still pending. All six final release gates remain open.
