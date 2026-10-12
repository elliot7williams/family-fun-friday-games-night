# Trusted budget phases

Later [trusted startup stages](TRUSTED_STARTUP_STAGES.md) append a bounded last
setup/checkpoint field to bootstrap budget errors only. The original host
phase/reason prefix and all deadlines remain; the results below are historical.

This checkpoint improves trusted `Js` failure diagnostics. It is not a fix
for intermittent native-worker startup, a new verified Firespider release,
or a completed milestone 146. Broader readiness remains about 65%.

## Runtime change

`Js` still returns `limit`, retires its failed realm and timer metadata, and
preserves the page changes completed before failure. Its budget message now
adds the current host boundary and caught exception type, for example:

```text
Script budget exceeded. phase=bootstrap; reason=TimeoutException
Script budget exceeded. phase=script; reason=StatementsCountOverflowException
```

The five phases are `bootstrap`, `script`, `checkpoint`, `timer`, and `event`.
The `Script budget exceeded.` prefix is retained, raw exception detail is
omitted, and the existing 512-unit cap remains. Ordinary JavaScript error
messages are unchanged. A phase is not the original callback or source line:
the observed unbounded queueMicrotask callback fails while Jint is still in
`Execute`, so it correctly reports `script`. Timer/event entries also include
their follow-up microtasks. Native `Jp` startup reports and worker messages
are not changed by this trusted-only adjustment.

Default options remain 20,000 statements, 8 MiB, one second, recursion 64,
100-ms regex timeout, array size 65,536, stack-overflow guarding, and disabled
CLR/GetType/write interop. No timeout was increased, retry/fallback added,
or bootstrap source, parser, or IPC protocol changed. Public aliases and constructor signature
are unchanged. An internal factory, available to the friend test assembly,
allows tests to create the same configured VM and add controlled observations
or faults; the public constructor always uses the normal factory-free path.

Parent `Lea.Script.dll`: 89,600 bytes, unchanged PE length from the compact
checkpoint, SHA-256
`1CF7AF2ABC97C418F226F67EB2D752A933BBC0F9185D49CE012F6462BBBAE2B2`.
This is not a claim of zero memory or execution overhead. The worker EXE
remains 43,520 bytes, SHA-256
`7C946C43ACF1396C4C345787D5D4EBD5682A284F9C8F11F44565A29DAB01A3AA`.

## Verification

`--budget-phases` passes 31 checks on the final test binary:

- The real existing one-second constraint expires after a test-only 1,100-ms
  BeforeEvaluate delay in bootstrap or page evaluation. Both report the right
  phase/type, prevent the attempted write, clear running/realm/timer state,
  and recover into a fresh realm without another injected delay.
- Real unbounded script, queued-microtask, timer, and window-load handlers
  report their host boundary and budget type with bounded messages.
- A test-only custom constraint throws at checkpoint entry. Its reset count
  follows pinned Jint 4.16.4's entry/exit resets: bootstrap, page, checkpoint.
  This is a synthetic fault, not an observed natural checkpoint timeout.
  Raw exception detail is excluded, state retires, and recovery succeeds.
- Ordinary JavaScript error text and subsequent successful execution remain
  intact. No test disables or enlarges the production options.

Separate final privacy coverage passes all 28 trusted/isolated checks. The
ordinary timer suite passes all 165 checks, including native UI dispatch,
hard-deadline cancellation, prevention of late writes, and process retirement.
Earlier timer failures from the compact/readable checkpoint remain historical
failures; this pass does not establish their cause or prove a cold-start fix.

The expanded full suite is planned for 1,082 checks. It fails in Startup.Run
at fresh-path worker sample 1, before the new 31-check group is reached:
launch 874 ms, probe 25 ms, wall 5,071 ms, CPU 640 ms. The last captured child
marker is `startup parsed` at wall 2,763 ms / CPU 515 ms, with no DOM-ready
marker. Sample 0 separately passes at wall 2,545 ms / CPU 156 ms. This is not
a full pass. New intentional-failure tests run last, preserving the earlier
suite's first-use ordering instead of warming up its startup tests first.

Development fixture failures are also retained: the initial microtask test
incorrectly expected `checkpoint` instead of `script`; its detailed failure
confirmed the actual boundary. Another probe incorrectly registered top-level
`load` on document rather than window and correctly returned `ok`. Both fixture
expectations were corrected without changing runtime dispatch behavior.

## One-sample boundary probes

The first timer script is now a shared test constant, retaining the original
suite payload. SDK Roslyn parses the previous archived and current C# source;
their decoded literal values match exactly: 645 UTF-8 bytes, SHA-256
`BD5966000D95D7652997C12632CC18BCFB159727D00EB44981F2420DCFD5BCEA`.
Two optional probes ran before the reporting change, using the
preserved compact parent DLL `839628B8...`:

- `--timer-start`: normal production Ex, one sample, passes two assertions;
  wall 1,832 ms / recorded CPU delta 15 ms after constructing the page/session.
- `--timer-phases`: reflective private Start, then public Ex, one sample;
  bootstrap ready at 2,230 ms / CPU 250 ms, page script parsed at 2,244 ms,
  returned at 2,518 ms, two assertions pass.

The reflective boundary differs from normal Run and its page parse observer
adds work. These are separate diagnostics, not retries, speed comparisons,
or cold OS-cache benchmarks. Their successful samples do not locate an earlier
failed invocation. No page code or production limit is modified by the probes.

## Release boundaries

The standalone test browser is rebuilt separately. No new Firespider candidate
or corrected 43-case host matrix is claimed, and installed files stay intact.
The full source and extracted full-package gates remain open. Preserve the
diagnostic improvement and failure evidence while continuing the native
startup investigation; do not transfer earlier release passes to this build.
