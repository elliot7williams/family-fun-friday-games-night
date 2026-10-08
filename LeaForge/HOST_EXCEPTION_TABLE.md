# Firespider compact exception table

## Scope

Separate experimental v26 integrates the current compact DOM exception table
into the .NET Framework 4.8 Firespider host. All 19 production dependencies match
the preceding `eae1e1ea6f8fa99265a2ac6fc71f5c96473a6b2a` source checkpoint.
The original 67 host inputs and three installed browser binaries are untouched.
No production engine code, public aliases, deadlines or isolation limits change
in this checkpoint.

The host harness adds checks for all 25 standard DOM exception constants in
both its first on-demand realm and the new inline-navigation realm. Both
constructor and prototype must retain own properties, numeric values, ordering,
enumerability, non-writable/non-configurable descriptors, rejected mutation and
exception branding. The checks require zero bridge calls and the same worker
PID before and after each check. No standard JavaScript names are shortened.

## Results

The candidate compiles successfully from 67 original inputs and 25 overlays,
with 90 unique final source inputs. All 376 staged runtime files match the fresh
NTFS test copy byte-for-byte before testing.

The single complete host-matrix attempt runs from
`2026-10-08T18:50:43.7320897Z` to `2026-10-08T18:57:32.2532779Z`:

- All 42 non-script host cases pass, including startup recovery and its real
  session timer, hibernation startup, native navigation, editing, policies,
  splits, image decoding and resource lifecycle.
- Both new exception-constant checks pass in their respective realms.
- The existing native callback regression passes with captured invocation,
  function/object/DOM0 receivers, cancellation and restored event state.
- The script case later fails at the first script execution after its
  intentional script-error fixture has retired the previous worker. The new
  worker exceeds the unchanged 5,000 ms startup budget. The matrix fails;
  partial passes do not constitute a successful script case or release gate.

The failing launch reports `phase=hello; launch=134; probe=48; wall=5037;
cpu=578 ms`. Its last captured marker is `startup parsed`. A last marker is
not a stack trace or proof of the precise stalled operation. No failed test is
retried, deadline increased, sandbox relaxed or failure converted into a pass.

## Observation boundary

The opt-in sparse observer watches only the successful initial script demand,
not the later failing recovery launch. Its `flowPassed=true` describes that
first demand, not the complete host matrix. Initial parent readiness is
3,731 ms; the surrounding host stopwatch is 4,455 ms including its overhead.
The retained observer report must not be described as a sampled timeout.

OS/CLR caches and system load are not controlled. Neither a smaller bootstrap
nor the observed successful startup establishes a general speedup or fixes the
intermittent timeout.

## Release boundary

The six actual-browser-EXE startup scenarios, release packaging, fresh runtime
extraction and independent archived-source runtime gates are held after the
candidate matrix failure. No new runtime/source release ZIP or installed
replacement is produced. The complete Lea source is backed up separately.

The earlier source suite's 1,267-check pass belongs to its exact tested runtime
and test executable `174C28B7A4EA9D345880BB0533464F35255A6897308AA927B2081A941CE8EE86`.
It is not a new full-suite run in this host checkpoint and does not override
the host failure. Earlier candidate and extraction failures remain retained.

Candidate host EXE: 1,134,080 bytes, SHA256
`C7414C790C0B05314C11E2B07D39EE541FA3A9CE00EB4885752FA82D8E961EB1`.
Host test harness SHA256:
`18C3D6537493E3B443558E759A8D6DBF623AE099C1446CBAB30FB9F3F13D63DB`.

Evidence is retained under
`H:\Lea Evidence\Lea-146-host-exception-table-2026-10-08`, including the exact
candidate runtime, harness source, build/test outputs, observation, source
backup, checkpoint bundle and sealed inventory. The failed native candidate
is retained separately at
`C:\Users\Elliot\AppData\Local\Lea\HostDiagnostics\Lea-146-exception-table-host-v26`.
It is a diagnostic build, not a recommended installed replacement.

## Completion

The original limited Lea 1.0 scope is complete, but the broader lightweight
browser-engine project is not. Practical readiness remains about 65%, not a
measured web-standards conformance score. Hardening milestone 146 stays open.
Next gates are reliable isolated-worker startup, repeatable fresh-host/package
verification, broader standards compatibility and measured performance. Full
Chrome/Safari-class web-platform parity is not the small-engine scope.
