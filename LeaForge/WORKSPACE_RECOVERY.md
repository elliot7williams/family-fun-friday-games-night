# Workspace Recovery and Profiler Results

Broader browser readiness remains about 65%; milestone 146 is still open.
This checkpoint makes development possible on another NTFS volume and repairs
a misleading diagnostic result. It is not a production startup-speed fix.

## Active Source

The active source checkout is `L:/Lea Development`; open `Lea.sln` there in
Visual Studio. It starts from pushed commit `1c8850c` on
`codex/lea-engine-foundation`, with the 20 Lea source roots and root files.
The original C: checkout and installed `L:/Firespider Internet` are retained
without modification by this checkpoint. C: reported zero free bytes during setup.
Its old Git index/HEAD are not synchronized to this new checkout.

Git currently shares historical objects with the preserved local repositories.
Do not delete the original `.git` repositories when freeing space. The source
ZIP backups and GitHub commits are separate recovery copies. A change-only Git
bundle also preserves this checkpoint independently of its working index.

Temporary files, SDK home and the pinned package cache are under
`L:/Lea Tool Cache`, outside the source tree. Eight package versions are copied
or restored there. All 450 package payload files match the former cache by
inventory, byte length and SHA-256; installation metadata is excluded.
This is a development cache, not a new renderer or deployment dependency.
Restore uses locked mode; no package version or lock file changes.

The first restore attempt used an option unsupported by the installed SDK 8;
its retained log is a command setup failure. A concurrent restore populated
later cache entries before the copy loop reached them, so that loop correctly
refused to overwrite existing entries. The final eight-package audit verifies
the resulting payload, rather than counting the interrupted copy as complete.
The referenced projects and scripting tests build without warnings or errors.
The complete solution/browser build is not implied by that focused build.

## Profiler Contract

Previously `--class-profile` caught and printed each sample's error, then
returned success even if one or both samples failed. The preserved baseline
reproduces exit 0 with two syntax failures. Its actual bootstrap profile also
times out in sample 0 and completes sample 1, still returning 0.

The profiler now reports all attempted sample outcomes and fails the command
if any sample failed. Each interpreter is disposed. A one-shot post-parse
timestamp is removed in `finally`; debugger stepping and the production
parser are not changed. The existing profiler remains explicitly instrumented
in-process code, not a native-worker or unmodified cold-start benchmark.
Its one-second execution budget and other limits remain unchanged.

Eight deterministic `--profile-contract` checks cover invalid syntax, a first
failure followed by success, two successful samples, retained reports and
post-parse marker semantics. The negative input is the packaged HTML runtime
fixture, deliberately parsed as JavaScript, so no source-project file is needed.
Invalid syntax is exposed by Jint's guarded parser as `JavaScriptException`,
not the raw parser exception initially expected by the test setup.

## Evidence

Root logs have the `Lea-local-` prefix; earlier failures are retained.

- `profile-before.log`: the old instrumented bootstrap profile exits 0 after
  a first-sample timeout at 2,903 ms wall / 125 ms CPU, followed by a completed
  36-ms warm sample. These measurements are not a speed comparison.
- `profile-red-verified.log`: the preserved baseline exits 0 despite two
  guarded-parser syntax failures. The earlier `profile-red.log` retains the
  same failures; its wrapper initially expected the wrong exception type.
- `profile-build.log`: a test setup compile error used an unqualified assertion
  helper; it is repaired before the final build. Final builds have no warnings
  or errors.
- `profile-contract.log`: all eight contract checks pass.
- `profile-invalid-final.log`: the actual CLI returns 1, retaining both syntax
  failures and the aggregate failure, without false parsed/completed markers.
- `profile-after.log`: actual instrumented bootstrap sample 0 still times out
  at 2,482 ms wall / 343 ms CPU. Its post-parse hook is captured at 1,177 ms,
  first statement marker at 1,569 ms, and last marker before the timeout at
  2,165 ms. Sample 1 completes in 36 ms; the command correctly returns 1.
  The region before the post-parse hook includes parser and entry/JIT costs.
  The final statement observed is not proof of the slow operation's cause.
- `full-source.log`: the full suite passes all 934 checks from the new NTFS
  checkout, including native hard deadlines/recovery, permission revocation,
  microtasks, timers, DOM and native queues. This one location/run does not
  explain earlier failures or establish stable cold-start readiness.
- `profile-package-layout.log`: eight focused checks pass from a separate
  runtime-style folder containing no source project or worker. This verifies
  diagnostic fixture independence, not the full standalone/native package.

No production engine source, public alias, protocol, deadline, sandbox setting
or dependency version changes. No new release ZIP, host EXE or installed-browser
replacement is made. The existing v17 scripting-host failure, clipboard host
gate, complete current host/package verification, document-origin policy and
wider web compatibility remain work. Historical passes are not transferred.

Before controlled cold-start performance comparisons, free system-drive space
and record the environment. Storage pressure is not asserted as the root cause.
