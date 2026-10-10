# Storage-Resumed Revalidation

Milestone 146 remains open and broader lightweight-browser readiness stays
about 65%. This checkpoint rechecks existing binaries after the user confirmed
freeing space on C:. It does not change the engine or claim a startup fix.

## Scope

The baseline is commit `57f827e45351eb15aaaf66d759af58448649450c`.
All 19 production records, the ordinary scripting test executable and renderer
runtime are verified before and after each gate. The scripting test executable
remains SHA256
`E07AA713F93BF3428DBD8513E60D8DEDB34E8C2191060A3AA298C6EA2236CB96`.
The required renderer bundle remains 747,520 bytes.

Three gates were declared before execution: the ordinary scripting suite,
renderer smoke and native-view load-failure regression. Each runs once, in
that order, after the preceding process exits. There is no external debugger,
startup observer, automatic gate retry, production rebuild or installed-browser
replacement. Existing internal checkpoints and tests' own diagnostics remain.
The native regression's explicit realm recovery is part of its contract, not a
rerun of a failed gate. Startup/page deadlines and all isolation limits remain
unchanged.

## Results

| Gate | Actual exit | Result |
| --- | ---: | --- |
| Ordinary 2,084-check scripting suite | 1 | Stops after 1,380 passing assertions at native inline-script/lifecycle startup |
| Renderer smoke | 0 | Complete smoke summary present |
| Native-view load-failure regression | 0 | All eight checks pass, including explicit recovery and process retirement |

The ordinary suite starts at `2026-10-10T08:39:46.8901008Z` and its result is
recorded at `08:43:11.4025506Z`. Its failure is:

```text
Worker startup deadline exceeded. phase=hello; launch=210; probe=6;
wall=5011; cpu=703 ms
```

The newest worker checkpoint is `startup parsed`; DOM bootstrap and the
verified hello do not complete. C: has 636,420,096 free bytes before this gate
and 635,654,144 free bytes at its terminal result, roughly 607/606 MiB.
Therefore this attempt is not blocked by the earlier full-C: condition.
These endpoint samples do not prove the cause of the timeout or rule out
memory pressure, other storage latency or scheduling effects.

The native regression produces an inspected, nonblank 940x700 capture with
`Lea load 1`, one initial attempt, the Lea icon and the retained `Elliot` field.
It verifies the existing 2,000-ms default page budget, 2,500-basis-point CPU-rate cap,
128-MiB memory limit, AppContainer/network/file protections and mitigation mask
15. Explicit recovery obtains a fresh realm and disposal retires it. This
narrow pass does not replace the failed broad gate or the earlier failed
copied-Firespider gates.

## Preservation And Next Work

Evidence is preserved under
`H:/Lea Evidence/Lea-146-storage-revalidation-2026-10-10`, including the
predeclared plan, frozen runtime, actual exit codes, logs, capture, source ZIP
and hash inventory. The original 67 Firespider inputs and three installed
binaries remain unchanged; all owned test processes retire. Previous failed
gates remain intact. No controlled performance comparison or new release is
claimed.

Source review of the captured Jint declaration-initialization stack has not
yet justified changing bootstrap execution. Avoiding that path during bootstrap
could merely defer its cold initialization to the first page script, under
the shorter page deadline. Keep investigating before adopting such a change.
L: also remains extremely tight at about 3 MiB free, so new builds require
deliberate output placement or more space; no files are deleted by this run.
