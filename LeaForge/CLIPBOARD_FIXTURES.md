# Firespider Clipboard Fixture Diagnostics

This follow-up fixes error preservation in the Firespider test harness. It does
not change Lea's production clipboard retries, editing API, engine runtime, or
worker startup deadline. Broader browser readiness remains about 65%.

## Reproduced Harness Problem

The earlier v15 host matrix stopped with clipboard error `0x800401D0` after 27
completed cases. Its traceback identifies `CheckEdit` but not the fixture write
or restoration operation. That evidence does not establish which operation
failed or whether restoration masked another failure.

A deterministic contract now demonstrates the masking behavior separately:
when both a test body and its `finally` cleanup throw, the old helper reports
only the cleanup exception. The preserved before-build fails the assertion
`cleanup cannot replace the primary test failure`. This proves a harness defect,
not the historical Windows clipboard error's cause.

## Changes

- Snapshot the clipboard before any editing command can modify it.
- Preserve the original exception and stack on a body-only failure; keep a
  cleanup-only failure as a failure. If both fail, report both original
  exceptions in an ordered `AggregateException`.
- Preserve nested settings-reset and clipboard-restoration errors as well.
- Label native clipboard failures with the fixture phase and original HRESULT:
  `edit.snapshot`, `edit.paste`, `edit.toolbar`, `edit.hover-disabled`,
  `edit.password`, `edit.read`, `edit.restore`, `paste.snapshot`,
  `paste.recovery`, and `paste.restore`.
- Print full host errors, retaining phase wrappers and aggregate exceptions.
  The two modified clipboard assertions report lengths instead of contents.
- Run all 14 deterministic checks before every `Test-FirespiderBoot.ps1` case.
  These checks never access the real clipboard, add no retries, and do not
  reclassify ordinary assertion errors.

The existing Framework compiler accepts the harness without a language-version
upgrade. The first baseline compilation fails with `CS1583`; the preserved
second attempt with TEMP/TMP explicitly on L: succeeds. No compiler-resource
validation is bypassed, and that observation alone does not prove the first
compiler failure's root cause.

## Current Candidate And Results

The separate NTFS candidate is
`L:/Lea Builds/Lea-146-clipboard-v18/runtime`. Its EXE SHA256 is
`F2969601DB9599AA1049A1A7CCB99FC94F49DD950391154D2EB01FCEB799DDC7`.

The locked SDK restore and Browser build succeed with zero build warnings and
errors. The candidate audit matches 67 original host inputs, 25 staged overlays,
four current integration sources, and 19 current engine dependencies in both
staged and runtime locations. Production engine code is unchanged from the
native-profile error checkpoint; its earlier 968-check full source pass is not
claimed as a new run of this harness change.

Logs use the `Lea-clipboard-followup-` prefix in `L:/Lea Development`:

| Log | Observed Result |
| --- | --- |
| `contract-red-final.log` | Old cleanup semantics fail the masking assertion after five checks. |
| `contract-green.log` | All 14 checks pass with the same Engine/Win dependencies. |
| `host-edit.log` | Editing assertions pass; process exits 1 when settings restoration cannot write `user.config` on full C:. Not a host-case pass. |
| `host-edit-repeat.log` | Same editing-body pass and disk-full restoration failure. Not a host-case pass. |
| `host-paste.log` | Entire case passes, including locked clipboard, menu/native/keyboard routes, retained edits/undo, recovery, readonly protection, and settings restoration. |
| `exe-startup.log` | All six actual-EXE startup scenarios pass without a legacy engine or exception dialog. |
| `host-script.log` | First worker startup times out in `phase=hello`: 5,057 ms wall, 562 ms CPU, last marker `startup parsed`. Settings restoration also fails for disk space. |

Startup readiness observations range from 4,381 to 6,833 ms. These are not a
benchmark, speed improvement, or startup SLA. The deferred-all case resumes
successfully; its initial deferred selection is not a rendered Lea page.

C: continues to report zero free bytes. Redirecting compiler/test temporary
files to L: does not redirect Framework user settings. The paste pass does not
negate either settings-save failure, and disk fullness is not established as the
cause of the separate interpreter timeout. No settings provider is substituted,
no persistence failure is suppressed, and no runtime retry/deadline is widened.

The installed EXE remains byte-identical. This is not a completed 43-case host
matrix, extracted-package validation, archived host-source rebuild, or new
release ZIP. Cold scripting startup, reliable settings persistence, broader
compatibility, and document-origin/security work remain open.

## Reproduction

Use Windows PowerShell with TEMP and TMP on an ACL-capable NTFS volume:

```powershell
$env:TEMP = 'L:\Lea Tool Cache\temp'
$env:TMP = $env:TEMP
.\Lea.Build\Test-FirespiderBoot.ps1 -Root 'L:\Lea Builds\Lea-146-clipboard-v18\runtime' -Case paste
.\Lea.Build\Test-FirespiderStartup.ps1 -Root 'L:\Lea Builds\Lea-146-clipboard-v18\runtime' -Scenario all
```

The compiled harness also accepts `--clipboard-contract` without a host root,
settings access, or Windows clipboard access.
