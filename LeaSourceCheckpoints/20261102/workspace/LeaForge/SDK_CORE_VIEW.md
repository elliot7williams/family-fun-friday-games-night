# SDK Core/View Qualification

2026-10-31 SDKv3 is an isolated source-project candidate, not the older root
prototype projects and not a replacement for the currently packaged browser.
Source staging/build and the local embedding sample are documented in
INTEGRATION.md. The DLLs are independently rebuilt, so browser4DA8/CA51 results
are not transferred to them.

Exact SDK core:772A51ADD8C8A9CE80964BEDB043D912B0DFB95422E3DC84FAC6F49B93366270.
Exact SDK view:80085FA1CD380CDF2E7AB0F8BBE5AC8982A11E95F82C43F776EE16ADC1A0D0C0.

## Candidate-Bound Regression

Test-FontShorthand.ps1 -SdkPair copies these exact DLLs after holding the pinned
SDK build receipt55595335... and verifies destination hashes. It does not rebuild
or substitute browser binaries. Existing fixtures/assertions/deadlines remain.

| Suite | Result | Runtime |
| --- | --- | --- |
| Shorthand model | 12controls/14cases PASS | 2051ms |
| Initial model | 5controls/7cases PASS | 809ms |
| Relative model | 6controls/12cases PASS | 1287ms |
| Prior/initial rendering | 142checks PASS at720/360 | 26361ms |
| Relative rendering | 142checks PASS at720/360 | 24898ms |

All10 compiler/test processes exited0 without timeout or forced termination,
Job0/output drained,inputs unchanged. Foreground owners64962/99587/66745/13195/
48601 retired0 after zero-lease/unverified-child guards. Each test retained30s,
compilers120s,cleanup5s. No SDK full release claim or global coverage percentage.

Audit-SdkCoreViewRegressions.ps1 independently checks pinned receipts, exact
terminal footers, run counts/budgets/retirement and all current input hashes.
Consolidated receipt continuity-evidence/sdk-core-view-regression-20261031-v1/result.json,
SHA256 DBA241938557999854BF1CDA48E8C20CF3498EC5E9AD4E32B6D6A1A42D562353.
It links each raw receipt. Desktop/narrow fixture screenshots are retained;
narrow relative-order selection was inspected. The original14-check embedding
sample remains separate evidence, not another broad rendering qualification.

## Remaining Work

2026-10-31 standaloneSDKimageworker sourcebuild and SDK-pair realdecoder81check/
shutdownrace14check campaigns now pass. SDK_IMAGE_WORKER.md contains exactpins
andsecurity boundaries. Supersedes nextworker-build notes below,not fullviewimage
layout/globalsecurity orcompleteSDK proof. Portablecombinedlayout remainsnext.

Next integrate the isolated image worker and test actual decoding, permission
restore and shutdown on the SDK pair. JavaScript parent/adapter/worker,printing,
full host sample,dependency notices,portable combined source packaging and
clean-machine validation remain unfinished. Legacy UIA and intermittent startup
reliability remain open. The SDK manifests still state completeSdk=false.
