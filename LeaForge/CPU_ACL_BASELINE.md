# Exact CPU Helper ACL Baselines

Milestone 146 stays open and broader browser readiness stays about 65%.
This changes test setup and diagnostics only. No renderer, parent, worker,
launcher permissions, limits or installed Firespider files change.

## Controlled Failure

A fresh L: NTFS copy of the preceding test executable reproduces the cleanup
assertion after 12 passing checks. This time the controller captures the
helper's original and restored security descriptors independently. The helper
exits successfully with 171 ms total CPU.

```text
before: D:(A;ID;FA;;;WD)
after:  D:AI(A;ID;FA;;;WD)
flags:  32772 -> 33796; XOR 1024
```

The DACL binary bytes and executable hash are identical. Only the
`DiscretionaryAclAutoInherited` bit changes. This proves a metadata-only
difference for this controlled run, not the cause of older failures that did
not capture their original descriptors. The raw failed run is retained.

## Test Setup

The CPU fixture now writes back its existing access descriptor before taking
the cleanup baseline. The shared test-only initializer checks that the entire
DACL byte sequence is unchanged and rejects all control-flag differences
except the auto-inherited bit. Existing volume diagnostics use the same check.
This is not a launcher permission change or an ignore rule for cleanup.

Subsequent cleanup still compares the complete captured descriptor exactly.
On any mismatch it logs both original and observed descriptors before failing.
Held-worker, retired-process, independent native CPU-handle and exact-grant
assertions remain in place, with unchanged 100-ms CPU work, three-second helper
wall guard, five-second hello deadline and 128-MiB/CPU-rate limits.

## Contracts

Test-only `-ab` aliases `--acl-baseline`:

```powershell
Lea.Script.Tests.exe "L:\Lea Development" -ab
Lea.Script.Tests.exe "L:\Lea Development" --acl-baseline
```

Nine checks cover identical/auto-inherited descriptors; reject changed
protection flags, access masks, added/removed/reordered ACEs and missing DACLs;
and verify a fresh private NTFS file initializes idempotently without changing
its data or permissions. Extra arguments are rejected before setup. No worker
is launched by this focused contract. It also runs in the full suite, whose
expected total rises from 3,010 to 3,019. Observer footer defaults are updated;
worker mode stays 87.

Evidence is kept in
`H:\Lea Evidence\Lea-146-cpu-acl-baseline-2026-10-12`.
Native tests run serially in fresh L: NTFS copies, with timing off unless the
fixture itself requests it. No observer, retries, fallback, wider budget,
runtime release ZIP, startup-fix or speedup claim is introduced.

## Recorded Results

- Private test build: actual exit 0, no warnings/errors.
- Baseline contract: nine checks through each spelling, actual exit 0.
  Invalid extra arguments: expected exit 1 before setup or any assertion.
- Native CPU lifetime: 25 checks, actual exit 0, exact grants retired in both
  linked launcher implementations. The fresh helper's initializer reports a
  metadata change before the baseline; executable and DACL bytes stay equal.
- CPU EOF/held-worker suite: 41 checks, actual exit 0, from a separate fresh
  copy. Exact native children are retired after deliberately held exits and
  complete descriptors match the captured baseline after grant cleanup.
- Hook contracts: eight; native startup phase: 16. Both have actual exit 0.
- Worker regression: actual exit 1 after 76 passing assertions, before its
  87-check footer. Active-page setup times out in hello: wall 5,018 ms, CPU
  296 ms, parsed AST observed, last checkpoint `elements`.
- Full scripting: actual exit 1 after 361 passing assertions, before its
  3,019-check footer. Isolated selector setup times out in hello: wall 5,032 ms,
  CPU 609 ms, parsed AST observed, last checkpoint `scope`.
- Observer configuration: four AST-extracted controls pass without launching
  an observer or changing native timing.

The raw metadata-only failure and both startup failures remain evidence.
Older ACL failures without before/after descriptors are not retrospectively
reclassified. Startup root causes and release verification remain open; no
page code runs before verified hello and no failed gate is retried.

All 19 production runtime entries are hash-identical to the preceding
checkpoint, as are 67 original host inputs, three installed files and 21 normal
outputs. The required renderer stays 749,568 bytes, with no runtime growth or
new release package. A complete source ZIP is entry-verified before committing.
