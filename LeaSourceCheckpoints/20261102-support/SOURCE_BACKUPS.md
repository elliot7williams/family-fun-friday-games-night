# Lea Source Backups

## Verified Checkpoint

2026-11-02: commit `1cfdb5026bf5fe7e53777b2609f0e850742931c8` on the
backup branch contains 1696 source files plus manifest and byte-preservation
attributes. Verify-SourceCheckpoint.ps1 checked all1698 raw Git blob IDs against
the copied files, checked SHA256 source hashes, verified the remote head and
independently downloaded the manifest from the commit-addressed GitHub URL.
Remote manifest SHA256:
`4BA84236829EBA1677FBA80BB46670BBF5F59A4916C06F9AA1259E83C28831E0`.
Receipt: I:/LeaBuildStorage/github-source-20261102-v1/verified-backup-receipt.json.
This does not certify USB endurance or engine functionality.

Initial publication normalized some line endings. The second commit restores
exact bytes under snapshot-local -text attributes; only the verified second
commit is the exact-byte checkpoint. An initial verifier path-array defect was
corrected before its successful verification. Git automatic maintenance reported
a USB permission error, but both commits succeeded; subsequent verifier commands
disable automatic maintenance for that invocation only. No global Git trust or
maintenance settings were changed.

## Destination Decision, 2026-11-02

The user delegated repository organization and asked to follow past practice.
The existing public repository is elliot7williams/family-fun-friday-games-night;
its codex/lea-engine-foundation branch already contains Lea development history.
Use a separate codex/lea-source-backup-20261102-v1 branch based on the current
remote development branch, not the stale local tracking reference. Do not change
master, the development branch, releases, or the active worktree/index.

Lea.Build/Publish-SourceCheckpoint.ps1 creates a separate checkout on I: and adds
LeaSourceCheckpoints/20261102. The snapshot contains checkpoint128's workspace
Lea trees and continuity instructions, the distinct mature SDK source export,
and global-install/lazy-map candidate DOM sources. SHA256 manifests distinguish
these generations. Root games are not added to the checkpoint. Existing ancestor
repository files remain unchanged.

This is not a complete offline SDK, binary release, backup of every historical
experiment, or backup of raw test evidence. Third-party libraries and their
unfinished notice packaging are excluded. The global-install candidate is not
adopted in Firespider; lazy-map remains unqualified. All six release gates remain
open. Original files, archives, and test evidence are retained locally.

The publisher checks archive members and copied source hashes, screens common
credential patterns (not a comprehensive secret audit), pushes without force,
and verifies the remote ref. Its local backup-receipt.json records the actual
commit and verified scope. A push is not considered successful without that
receipt and independent remote readback.
