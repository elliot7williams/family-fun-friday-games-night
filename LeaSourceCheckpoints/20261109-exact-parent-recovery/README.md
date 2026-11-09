# Exact Lea Script Parent Source Recovery

All15original parent source files are recovered, including14.cs matching the
historical SHA256 and raw GitHub readback. The reconstructed original project
produced literal byte-identical406C, verified by an independent byte comparison.
This clears the missing parent-source problem, not the engine startup timeout.

Includes current source148ZIP, all19recovered source/project/resource/targets
files, a source-only component ZIP, changed tooling/docs, the successful original
recipe snapshot and rebuild/recovery/control receipts. No dependency/runtime
binaries or toolchain are bundled; not a complete engine SDK or browser release.
Dependency hashes required by the component are in parent-source-package.json.

The fresh source-ZIP extraction/rebuild is pending: its attempt stopped BEFORE
launch at the unchanged600MiB C: reserve. The current wrapper adds an unexecuted
archive mode; do not transfer the direct build pass to it. Historical successful
recipe bytes are included separately. Startup/full-suite/host/compatibility gates
remain open. FINISH_PLAN.md now uses current candidate pins and next actions.
