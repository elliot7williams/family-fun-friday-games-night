# Lazy Event Storage

Milestone 146 remains open. Broader readiness stays about 65%. This reduces
unnecessary event-container allocations without enlarging the required engine
or optional scripting binaries. It does not prove a startup fix, total heap
reduction, cold-cache improvement or general speedup.

## Runtime Change

Each private target state still carries its node/key, style and class-list
bindings. Its listener array and three null-prototype maps now start as null:

- `listeners`: created when an actual listener or DOM0 slot is registered.
- `properties`: created on the first DOM0 assignment, including explicit null.
- `inline`: created after authored inline code is successfully compiled.
- `handlers`: created when a non-null DOM0/inline slot is activated.

Valid empty getters, removals, null listener callbacks and no-listener dispatch
leave these containers dormant. Populated containers are reused after removal
or deactivation; they are not repeatedly destroyed and rebuilt. An explicit
null handler still masks an inline attribute. First-use maps retain the captured
constructor, not a later page override.

Shared public methods/descriptors, branded receivers, argument conversion,
capture/bubble/once/passive behavior, inline/DOM0 slot order, cancellation,
private state and the combined 128-listener ceiling are preserved. The wire,
256-call bridge, 512-node handles, sandbox, memory/CPU limits and deadlines do
not change. No public engine alias or dependency is added.

## Allocation Evidence

A test-only pre-bootstrap WeakMap observer retains private state references
outside the page realm. The controlled CLR diagnostic observes two realm and
16 element targets with zero arrays/maps. The preceding source creates four
containers per target, giving a source-derived 72 for that same 18-target
scenario. The identical diagnostic against the old runtime rejects its eager
document/window containers before wrapping the 16 elements.

An empty dispatch also leaves storage dormant. The first ordinary listener
needs one array and no maps. Explicit null adds only a property map. DOM0 and
authored inline use initialize only their required maps. After exercising three
targets and wrapping ancestors, the diagnostic reports 21 target states,
three arrays, two property maps, one inline map and three slot maps. It checks
reuse, captured intrinsics, cancellation and hidden containers.

These are container/reference observations, not a measured total heap or
wall-clock benchmark. The first one-second `Js` allocation test timed out at
bootstrap `events` before any assertion. Its source, executable and failure
are retained. Allocation measurement was separated into a clearly labeled
five-second CLR diagnostic using the actual embedded asset and shared `Ja`
bridge. Ordinary trusted bootstrap remains one second, and native hello
remains five seconds; neither production gate is replaced or widened.

The first diagnostic then passed 22 assertions but its own global `inline`
variable collided with its privacy check. That failed source/PE/result is also
retained. Renaming only the diagnostic variable fixes that test; the ordinary
behavior suites are source-identical before/after this repair.

## Tests

Test-only two-letter `-le` aliases `--lazy-events`, with `core`, `trusted`,
`isolated` and `native` modes. `core` is allocation observation, not startup
verification. Only native mode accepts an optional new capture prefix.

```powershell
Lea.Script.Tests.exe "L:\Lea Development" -le core
Lea.Script.Tests.exe "L:\Lea Development" -le trusted
Lea.Script.Tests.exe "L:\Lea Development" -le isolated
Lea.Script.Tests.exe "L:\Lea Development" -le native
```

All 86 focused checks pass: allocation 23, trusted 26, isolated 28, native nine.
The identical old-runtime comparisons fail as expected after two assertions;
extra arguments fail before setup. Native clicks cover ordinary, DOM0 and
authored handlers, with retained field editing and visually checked 940/420
captures. Exact workers retire after disposal.

Existing gates pass: events 136, receiver checks 81, callback calls 90, privacy
36, lazy styles 14, asset identity four, hook contracts eight, CPU lifetime 25,
startup phase 16 and worker 87. Four observer footer controls pass without
launching an observer. The full test total is now 3,105, but this run does not
reach its footer.

Retained failures, with no retry:

- Event-options: trusted bootstrap times out at `events`, before any assertion.
- Full scripting: fails after 233 assertions in the timed startup fixture.
  Hello wall 5,011 ms, CPU 1,046 ms; final parsed marker at child wall 4,390 ms,
  CPU 1,015 ms. It does not establish which later operation caused the timeout.

A separate ordinary browser smoke passes: three animation frames, zero pending
callbacks, retained counter 1, width 40 to 96, isolated worker, 128-MiB Job,
no loopback exemption and no `mshtml.dll` in the reported host modules.
Its 210-ms value is the animation interval, not startup time. Captures are
visually inspected. A passing browser run does not erase the failed gates.

## Size And Preservation

The generator's seven controls, write and freshness check pass with matching
AST JSON: readable 38,887 bytes, compact 28,165 bytes. The readable source stays
in the source archive; both runtimes embed only the exact compact resource.
Compact SHA-256:
`DE2FB534DE7A0C2D14480D02EFDEA6B8DF850392D87D8A1369AA982F05FDDCF7`.

Only two of 19 runtime entries change, with zero byte growth:

| Entry | Bytes | SHA-256 |
| --- | ---: | --- |
| Lea.Script.dll | 102400 | 8621C0FD7E56E9397414AFCF5E72A44C57DF2CFB55CE6F5760C219F44DC43411 |
| ScriptWorker/Lea.Script.Worker.exe | 51200 | 47475AC5E73D00EAECEB350663F55E201E7F65364B1448C8CFAF7C50A592028E |

The required renderer stays 749,568 bytes. Original 67 Firespider host inputs,
three installed files and 21 normal outputs remain hash-identical. Tests use
private L: NTFS deployments. A complete source ZIP is entry-verified before
committing. No runtime release ZIP or installed-browser replacement is made.
Copied Firespider verification of these new bytes and reliable repeated
startup remain open.

Evidence: `H:\Lea Evidence\Lea-146-lazy-events-2026-10-12`.
