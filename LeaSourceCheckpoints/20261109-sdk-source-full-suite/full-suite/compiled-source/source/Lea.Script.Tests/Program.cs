using System;
using System.IO;
using System.Linq;
using System.Threading;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class Program
    {
        private static int checks;

        [STAThread]
        private static int Main(string[] args)
        {
            var previousStartupTrace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
            try
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
                Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE enabled");
                var root = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);
                var fixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-runtime.html");
                var viewFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-view.html");
                var attributeFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-attributes.html");
                var attributeEditFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-attribute-edits.html");
                var selectorFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-selectors.html");
                var treeFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-tree.html");
                var subtreeFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-subtree.html");
                var removalFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-removal.html");
                var loadFailureFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-load-failure.html");
                if (args.Length > 1 && args[1] == "--lazy-maps")
                {
                    if (args.Length != 3 || (args[2] != "trusted" && args[2] != "isolated"))
                        throw new ArgumentException("Supply only --lazy-maps and trusted or isolated.");
                    LazyMaps.Run(Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-styles.html"), args[2]);
                    Console.WriteLine("PASS Lea.Script lazy maps " + args[2] + ": " + checks + " checks; first-use poisoning, unchanged production budgets.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--vm-options")
                {
                    VmOptions.Run();
                    Console.WriteLine("PASS Lea.Script VM options: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--startup-stages" || args[1] == "-sg"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for startup stage checks.");
                    BootstrapStages.Run(fixture);
                    Console.WriteLine("PASS Lea.Script startup stages: " + checks + " checks; controlled trusted faults only.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--lazy-styles" || args[1] == "-ls"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only a lazy-style mode and optional native capture prefix.");
                    var styleFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-styles.html");
                    if (args[2] == "native") View.Pump(() => LazyStyles.Native(styleFixture, args.Length == 4 ? args[3] : null));
                    else LazyStyles.Run(styleFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script lazy styles " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--lazy-events" || args[1] == "-le"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only a lazy-event mode and optional native capture prefix.");
                    if (args[2] == "native") View.Pump(() => LazyEvents.Native(Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-events.html"), args.Length == 4 ? args[3] : null));
                    else LazyEvents.Run(Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-styles.html"), args[2]);
                    Console.WriteLine("PASS Lea.Script lazy events " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--contains" || args[1] == "-ct"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only a contains mode and optional native capture prefix.");
                    var containsFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-contains.html");
                    if (args[2] == "native") View.Pump(() => Contains.Native(containsFixture, args.Length == 4 ? args[3] : null));
                    else Contains.Run(containsFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script contains " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--agent-waits" || args[1] == "-aw"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only an agent wait mode and optional native capture prefix.");
                    if (args[2] == "native") View.Pump(() => AgentWait.Native(viewFixture, args.Length == 4 ? args[3] : null));
                    else AgentWait.Run(viewFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script agent waits " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1].StartsWith("--execution-report", StringComparison.Ordinal))
                {
                    if (args[1] == "--execution-report-contract") ExecutionReports.Contract();
                    else if (args[1] == "--execution-report") ExecutionReports.Run(fixture);
                    else if (args[1] == "--execution-report-native") ExecutionReports.Native(fixture);
                    else throw new ArgumentException("Unknown execution report diagnostic.");
                    Console.WriteLine("PASS Lea.Script execution report: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--origin-probe")
                {
                    DocumentOrigins.Probe();
                    Console.WriteLine("PASS Lea.Script origin accessor probe: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--origins-trusted" || args[1] == "--origins-isolated"))
                {
                    var isolated = args[1] == "--origins-isolated";
                    DocumentOrigins.Run(isolated);
                    if (isolated) View.Pump(() => DocumentOrigins.Native(args.Length > 2 ? args[2] : null));
                    Console.WriteLine("PASS Lea.Script document origins " + (isolated ? "isolated" : "trusted") + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--load-failures" || args[1] == "-lf"))
                {
                    LoadFailures.Controlled(loadFailureFixture);
                    Console.WriteLine("PASS Lea.Script load failures: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--load-failure-native" || args[1] == "-ln"))
                {
                    View.Pump(() => LoadFailures.Native(loadFailureFixture, args.Length > 2 ? args[2] : null));
                    Console.WriteLine("PASS Lea.Script native load failure: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--subtree-aliases" || args[1] == "--subtree-trusted" || args[1] == "--subtree-isolated" || args[1] == "--subtree-view"))
                {
                    if (args[1] == "--subtree-view") View.Pump(() => SubtreeQueries.Native(subtreeFixture, args.Length > 2 ? args[2] : null));
                    else SubtreeQueries.Run(selectorFixture, args[1].Substring("--subtree-".Length));
                    Console.WriteLine("PASS Lea.Script subtree " + args[1].Substring("--subtree-".Length) + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--bootstrap-preparation" || args[1] == "-bp"))
                {
                    if (args.Length != 4) throw new ArgumentException("Supply a strategy and new JSON report path for --bootstrap-preparation.");
                    BootstrapPreparation.Run(args[2], args[3]);
                    Console.WriteLine("PASS Lea.Script bootstrap preparation diagnostic: " + checks + " checks; not a native startup gate.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--bootstrap-markers-contract" || args[1] == "-bm"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for bootstrap marker checks.");
                    BootstrapMarkers.Contract();
                    Console.WriteLine("PASS Lea.Script bootstrap markers: " + checks + " checks; compiled contract only.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--bootstrap-entry-contract")
                {
                    if (args.Length > 3) throw new ArgumentException("Supply at most one worker executable for --bootstrap-entry-contract.");
                    BootstrapEntry.Contract(args.Length == 3 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script bootstrap entry: " + checks + " checks; compiled contract only.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--bootstrap-prepared-contract")
                {
                    if (args.Length > 3) throw new ArgumentException("Supply at most one worker executable for --bootstrap-prepared-contract.");
                    BootstrapEntry.PreparedContract(args.Length == 3 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script bootstrap prepared: " + checks + " checks; compiled contract only.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--startup-hooks" || args[1] == "-sh"))
                {
                    if (args.Length > 3) throw new ArgumentException("Supply at most one worker executable for startup hook checks.");
                    BootstrapEntry.Checkpoints(args.Length == 3 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script startup hooks: " + checks + " checks; compiled and controlled only.");
                    return 0;
                }
                if (args.Length > 1 && args[1].StartsWith("--bootstrap-cache", StringComparison.Ordinal))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for bootstrap cache checks.");
                    if (args[1] == "--bootstrap-cache-contract") BootstrapCache.Contract();
                    else if (args[1] == "--bootstrap-cache-cold") BootstrapCache.Cold(fixture);
                    else if (args[1] == "--bootstrap-cache") BootstrapCache.Run(attributeFixture);
                    else throw new ArgumentException("Unknown bootstrap cache diagnostic.");
                    Console.WriteLine("PASS Lea.Script bootstrap cache: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-observe")
                {
                    if (args.Length != 3) throw new ArgumentException("Supply a new JSON report path for --startup-observe.");
                    StartupObserve.Run(viewFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script native observation: " + checks + " checks; instrumented diagnostic only.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-volumes")
                {
                    if (args.Length != 4) throw new ArgumentException("Supply an existing alternate-volume staging root and a new JSON report path for --startup-volumes.");
                    Startup.Volumes(fixture, args[2], args[3]);
                    Console.WriteLine("PASS Lea.Script startup volume diagnostic: " + checks + " checks; not a cold-cache benchmark.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--listener-calls" || args[1] == "--listener-calls-trusted"))
                {
                    ListenerCalls.Run(viewFixture, args[1] == "--listener-calls-trusted", args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script listener calls: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--worker-entry")
                {
                    ModernComponents.WorkerEntry();
                    Console.WriteLine("PASS Lea.Script worker entry: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--frame-progress" || args[1] == "-fp"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for frame progress checks.");
                    FrameProgress.Run(fixture);
                    Console.WriteLine("PASS Lea.Script frame progress: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--wire-frames" || args[1] == "-wf"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for wire frame checks.");
                    WireFrames.Run();
                    Worker.Protocol();
                    Console.WriteLine("PASS Lea.Script wire frames: " + checks + " checks; compiled transport only.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--native-cpu")
                {
                    NativeCpu.Run(fixture);
                    Console.WriteLine("PASS Lea.Script native CPU lifetime: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--acl-baseline" || args[1] == "-ab"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for ACL baseline checks.");
                    AclBaseline.Contract();
                    Console.WriteLine("PASS Lea.Script ACL baseline: " + checks + " checks; no worker launch.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--event-recovery" || args[1] == "-er"))
                {
                    if (args.Length != 3) throw new ArgumentException("Supply only a fixture root, event-recovery mode and a new report path.");
                    EventRecovery.Run(viewFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script event recovery: " + checks + " checks; six controlled cycles.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--hard-recovery" || args[1] == "-hr"))
                {
                    if (args.Length != 3) throw new ArgumentException("Supply only a fixture root, hard-recovery mode and a new report path.");
                    HardRecovery.Run(fixture, args[2]);
                    Console.WriteLine("PASS Lea.Script hard recovery: " + checks + " checks; no retries.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "-st")
                {
                    if (args.Length != 3) throw new ArgumentException("Supply only a fixture root, stack-target mode and a new report path.");
                    StackTarget.Run(args[2]);
                    Console.WriteLine("PASS Lea.Script stack target: " + checks + " checks; controlled init wait only.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--cpu-eof" || args[1] == "-ce"))
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for CPU EOF checks.");
                    NativeCpu.Trace(fixture);
                    Console.WriteLine("PASS Lea.Script CPU EOF: " + checks + " checks; controlled test helpers only.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--worker-stderr")
                {
                    WorkerStderr.Run();
                    Console.WriteLine("PASS Lea.Script worker stderr: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--event-receivers" || args[1] == "--event-receivers-trusted" || args[1] == "--event-receivers-isolated"))
                {
                    EventReceivers.Run(viewFixture, args[1] == "--event-receivers-trusted", args.Length > 2 ? args[2] : null, args[1] == "--event-receivers-isolated");
                    Console.WriteLine("PASS Lea.Script event receivers: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--element-receivers" || args[1] == "--element-receivers-isolated"))
                {
                    ElementReceivers.Run(attributeFixture, args[1] == "--element-receivers-isolated", args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script element receivers: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--event-options")
                {
                    EventOptions.Run(viewFixture);
                    Console.WriteLine("PASS Lea.Script event options: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--budget-phases")
                {
                    BudgetPhases.Run(viewFixture);
                    Console.WriteLine("PASS Lea.Script budget phases: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--timer-start" || args[1] == "--timer-phases"))
                {
                    RuntimeStart.Run(viewFixture, args[1] == "--timer-phases");
                    Console.WriteLine("PASS Lea.Script runtime start diagnostic: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--bootstrap-assets")
                {
                    BootstrapAssets.Run(root);
                    Console.WriteLine("PASS Lea.Script bootstrap assets: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--runtime-load")
                {
                    Loader.Run(fixture);
                    Console.WriteLine("Lea.Script runtime loader diagnostic completed.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--io-dispatch")
                {
                    ParentIo.Run(fixture);
                    Startup.BlockedIo(fixture);
                    Console.WriteLine("PASS Lea.Script parent I/O: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--io-bench")
                {
                    ParentIo.Bench(fixture);
                    Console.WriteLine("PASS Lea.Script parent I/O benchmark completed.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--tree-view")
                {
                    View.Tree(treeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script native tree: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--removal-core" || args[1] == "--removal-trusted" || args[1] == "--removal-isolated" || args[1] == "--removal-view" || args[1] == "-de"))
                {
                    var mode = args[1] == "-de" ? "core" : args[1].Substring("--removal-".Length);
                    if (args.Length > (mode == "view" ? 3 : 2)) throw new ArgumentException("Supply only a fixture root and removal mode, plus an optional capture prefix for view mode.");
                    if (mode == "view") View.Pump(() => Removal.Native(removalFixture, args.Length == 3 ? args[2] : null));
                    else Removal.Run(removalFixture, mode);
                    Console.WriteLine("PASS Lea.Script removal " + mode + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--tree" || args[1] == "--tree-trusted"))
                {
                    var trustedOnly = args[1] == "--tree-trusted";
                    Tree.Run(treeFixture, trustedOnly);
                    if (!trustedOnly) View.Tree(treeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script tree" + (trustedOnly ? " trusted-only diagnostics" : "") + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--selector-startup" || args[1] == "-ss" ||
                    args[1] == "--selector-observe" || args[1] == "-so"))
                {
                    if (args.Length < 3 || args.Length > 4) throw new ArgumentException("Supply a new report path and optional worker directory for " + args[1] + ".");
                    Selectors.Startup(selectorFixture, args[2], args.Length == 4 ? args[3] : null,
                        args[1] == "--selector-observe" || args[1] == "-so");
                    Console.WriteLine("PASS Lea.Script selector startup observation: " + checks + " checks; not a full scripting gate.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--selector-observation-contract")
                {
                    if (args.Length != 2) throw new ArgumentException("No extra arguments are accepted for --selector-observation-contract.");
                    SelectorObservation.Contract();
                    Console.WriteLine("PASS Lea.Script selector observation contract: " + checks + " checks; no browser workers.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--select")
                {
                    Selectors.Run(selectorFixture);
                    View.Selectors(selectorFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script selectors: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--scope")
                {
                    BootstrapScopes.Run(attributeFixture);
                    Console.WriteLine("PASS Lea.Script bootstrap scopes: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--class-profile")
                {
                    ClassLists.Profile(args.Length > 2 ? args[2] : null);
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--profile-contract")
                {
                    ClassLists.ProfileContract(fixture);
                    Console.WriteLine("PASS Lea.Script profile contract: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--class-start")
                {
                    ClassLists.Bootstrap(attributeFixture);
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--classes-view")
                {
                    View.ClassLists(attributeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script native classList: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--classes" || args[1] == "--classes-warm"))
                {
                    var warm = args[1] == "--classes-warm";
                    if (warm)
                    {
                        Console.WriteLine("Diagnostic warm functional run; not a cold-start release gate.");
                        try { ClassLists.Bootstrap(attributeFixture); }
                        catch (System.Reflection.TargetInvocationException error) when (error.InnerException is TimeoutException)
                        { Console.WriteLine("Cold bootstrap timeout retained; continuing warm functional diagnostics."); }
                    }
                    ClassLists.Run(attributeFixture);
                    View.ClassLists(attributeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script classList" + (warm ? " warm diagnostics" : "") + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--attribute-edits" || args[1] == "-ae"))
                {
                    AttributeEdits.Run(attributeEditFixture);
                    View.AttributeEdits(attributeEditFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script attribute edits: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--attribute-edits-view")
                {
                    View.AttributeEdits(attributeEditFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script native attribute edits: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--attributes-view")
                {
                    View.Attributes(attributeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script native attributes: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--attributes")
                {
                    Attributes.Run(attributeFixture);
                    View.Attributes(attributeFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script attributes: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--bootstrap")
                {
                    if (args.Length != 3) throw new ArgumentException("Supply one baseline ScriptWorker directory for --bootstrap.");
                    Startup.Compare(fixture, args[2]);
                    Console.WriteLine("PASS Lea.Script bootstrap comparison: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-stop")
                {
                    Startup.StopWithoutPump(fixture);
                    Console.WriteLine("PASS Lea.Script startup stop: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-phase")
                {
                    Startup.ParseMarker(fixture);
                    Console.WriteLine("PASS Lea.Script startup phase: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-report")
                {
                    Startup.Report();
                    Console.WriteLine("PASS Lea.Script startup report: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-errors")
                {
                    Startup.ProfileErrors();
                    Console.WriteLine("PASS Lea.Script profile errors: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-unavailable")
                {
                    Startup.Unavailable(fixture);
                    Console.WriteLine("PASS Lea.Script unavailable worker: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-faults")
                {
                    Startup.LaunchFailures(fixture);
                    Console.WriteLine("PASS Lea.Script launch failures: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-off")
                {
                    Startup.Ui(fixture, "off");
                    Console.WriteLine("PASS Lea.Script startup off: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-stale")
                {
                    Startup.Ui(fixture, "stale");
                    Console.WriteLine("PASS Lea.Script startup stale: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup-io")
                {
                    Startup.BlockedIo(fixture);
                    Console.WriteLine("PASS Lea.Script startup I/O: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--startup")
                {
                    Startup.Run(fixture);
                    Console.WriteLine("PASS Lea.Script startup: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--worker")
                {
                    Worker.Run(fixture);
                    Console.WriteLine("PASS Lea.Script worker: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--micro")
                {
                    Microtasks.Run(viewFixture);
                    Console.WriteLine("PASS Lea.Script microtasks: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--timers")
                {
                    Timers.Run(viewFixture);
                    Console.WriteLine("PASS Lea.Script timers: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--animation-frames" || args[1] == "-rf"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only a frame test mode and optional native capture prefix.");
                    if (args[2] == "native") View.Pump(() => Frames.Native(viewFixture, args.Length == 4 ? args[3] : null));
                    else Frames.Run(viewFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script animation frames " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && (args[1] == "--performance-clock" || args[1] == "-cl"))
                {
                    if (args.Length < 3 || args.Length > 4 || args.Length == 4 && args[2] != "native")
                        throw new ArgumentException("Supply only a clock test mode and optional native capture prefix.");
                    if (args[2] == "native") View.Pump(() => Clock.Native(viewFixture, args.Length == 4 ? args[3] : null));
                    else Clock.Run(viewFixture, args[2]);
                    Console.WriteLine("PASS Lea.Script performance clock " + args[2] + ": " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--events")
                {
                    Events.Run(viewFixture);
                    Console.WriteLine("PASS Lea.Script events: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--view")
                {
                    View.Run(viewFixture, args.Length > 2 ? args[2] : null);
                    Console.WriteLine("PASS Lea.Script native view: " + checks + " checks.");
                    return 0;
                }
                if (args.Length > 1 && args[1] == "--ui")
                {
                    Worker.Ui(fixture);
                    Console.WriteLine("PASS Lea.Script UI: " + checks + " checks.");
                    return 0;
                }
                SuitePhase("begin", 0);
                SelectorObservation.Contract();
                SuitePhase("end", 0);
                SuitePhase("begin", 1);
                AclBaseline.Contract();
                SuitePhase("end", 1);
                SuitePhase("begin", 2);
                BootstrapAssets.Run(root);
                SuitePhase("end", 2);
                SuitePhase("begin", 3);
                BootstrapMarkers.Contract();
                SuitePhase("end", 3);
                SuitePhase("begin", 4);
                var engine = new E();
                SuitePhase("end", 4);
                SuitePhase("begin", 5);
                var page = engine.Go(fixture);
                SuitePhase("end", 5);
                SuitePhase("begin", 6);
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                SuitePhase("end", 6);
                SuitePhase("begin", 7);
                var result = engine.Id(page, "result");
                SuitePhase("end", 7);
                SuitePhase("begin", 8);
                using (var script = new Js(engine, page))
                {
                    Check(page.T == "Lea JavaScript runtime experiment", "page scripts stay opt-in");
                    var code = engine.Q(page, "script").C[0].X;
                    var execution = script.Ex(code);
                    Check(execution.Ok && execution.N == 6, "computed script: " + execution.S + "; " + execution.M);
                    Check(page.T == "Lea 7" && result.C[0].X == "Total: 7", "template strings and computed DOM values");
                    Check(result.Av("class") == "active" && result.Av("style").Contains("140px"), "class and width mutation");
                    Check(Object.ReferenceEquals(field, page.Fs.Single(item => item.El.Av("id") == "field")) && field.V == "{\"total\":7}", "retained field and JSON");
                    Check(page.Fs.Single(item => item.El.Av("id") == "check").C, "computed checked value");
                    Check(script.Ex("if (result !== document.querySelector('#result')) throw new Error('identity'); function sum(n) { let s = 0; for (let i = 1; i <= n; i++) s += i; return s; } document.title = String(sum(10));").Ok && page.T == "55", "functions, loops, persistent globals, wrapper identity");
                    Check(script.Ex("document.querySelector('.item').textContent = 'anonymous node';").Ok && engine.Q(page, ".item").C[0].X == "anonymous node", "mutation without an id");
                    Check(script.Ex("if (document.getElementById('missing') !== null || document.querySelector('p').getAttribute('missing') !== null) throw new Error('null');").Ok, "missing nodes and attributes");
                    Check(script.Ex("if (typeof System !== 'undefined' || typeof importNamespace !== 'undefined' || typeof __lea !== 'undefined' || typeof document.GetType !== 'undefined' || typeof alert.GetType !== 'undefined' || typeof fetch !== 'undefined') throw new Error('host exposure');").Ok, "no CLR, raw bridge, or network API exposure");
                    Check(script.Ex("document.title = ''; document.getElementById('result').textContent = '  ${who}; \\\"quotes\\\"  '; alert('computed ' + (2 + 3));").Ok, "literal typed values");
                    Check(page.T == "" && result.C[0].X == "  ${who}; \"quotes\"  " && page.Ms.Last() == "computed 5", "no alias interpolation or code injection");
                    Check(script.Ex("document.title = ['plain', 'data'].join(' ');").Ok, "execution after scalar values");
                    var child = engine.Id(page, "child");
                    Check(script.Ex("var detached = document.getElementById('child'); document.getElementById('parent').textContent = 'replacement';").Ok, "subtree replacement");
                    Check(child.Up == null && engine.Id(page, "removed") == null && !page.Fs.Any(item => item.El.Av("id") == "removed"), "detached tree and fields removed");
                    Check(!script.Ex("detached.textContent = 'stale';").Ok, "detached mutation rejected");
                    Check(script.Ex("if (typeof total !== 'undefined') throw new Error('realm survived error'); document.title = 'recovered';").Ok && page.T == "recovered", "failed realm discarded and recovery");
                    Check(script.Ex("while (true) {}").S == "limit", "infinite loop budget");
                    var allocation = script.Ex("const allocations = []; for (let i = 0; i < 3000; i++) allocations.push('x'.repeat(8192) + i);");
                    Check(allocation.S == "limit", "allocation budget: " + allocation.S + "; " + allocation.M);
                    Check(!script.Ex("new Array(65537);").Ok, "array size bound");
                    Check(script.Ex("function f() { return 1 + f(); } f();").S == "limit", "recursion budget");
                    Check(script.Ex("for (let i = 0; i < 300; i++) document.getElementById('result');").S == "limit", "host-call budget");
                    Check(script.Ex("let = ;").S == "js", "syntax failure");
                    Check(script.Ex(new string(' ', 16385)).S == "code", "source length bound");
                    Check(script.Ex("document.getElementById('result').style.width = 'nonsense';").S == "js", "invalid CSS rejected");
                    Check(script.Ex("var privateState = 17;").Ok, "permission test setup");
                    engine.Je = false;
                    Check(script.Ex("document.title = 'disabled';").S == "off" && page.T == "recovered", "host scripting permission");
                    Check(script.At.N == 0, "disabled result has no stale mutation count");
                    Check(engine.Jv(page, result, "tx", "disabled") == 0, "typed permission gate");
                    engine.Je = true;
                    Check(script.Ex("if (typeof privateState !== 'undefined') throw new Error('permission realm');").Ok, "permission revocation drops globals");
                    Check(script.Ex("var revokedBetweenCalls = 1;").Ok, "between-call revocation setup");
                    engine.Je = false;
                    engine.Je = true;
                    Check(script.Ex("if (typeof revokedBetweenCalls !== 'undefined') throw new Error('revoked realm');").Ok, "revocation without disabled execution drops globals");
                    var partial = script.Ex("document.title = 'partial'; Object.prototype.leaked = 1; throw new Error('deliberate failure');");
                    Check(partial.S == "js" && partial.N == 1 && page.T == "partial", "failure reports completed mutations without rollback");
                    Check(script.Ex("if ('leaked' in {}) throw new Error('prototype leak'); document.title = 'recovered';").Ok, "failed realm prototype discarded");
                    Exception crossThread = null;
                    var thread = new Thread(() => { try { script.Ex("1;"); } catch (Exception error) { crossThread = error; } });
                    thread.Start();
                    thread.Join();
                    Check(crossThread is InvalidOperationException, "thread affinity");
                }
                SuitePhase("end", 8);
                SuitePhase("begin", 9);
                using (var fresh = new Js(engine, page))
                    Check(fresh.Ex("if (typeof result !== 'undefined') throw new Error('session leak');").Ok, "session realm isolation");
                SuitePhase("end", 9);
                SuitePhase("begin", 10);
                var foreignEngine = new E();
                SuitePhase("end", 10);
                SuitePhase("begin", 11);
                var foreignPage = foreignEngine.Go(fixture);
                SuitePhase("end", 11);
                SuitePhase("begin", 12);
                Check(engine.Jv(page, foreignEngine.Id(foreignPage, "result"), "tx", "foreign") == 0, "cross-page node mutation rejected");
                SuitePhase("end", 12);
                SuitePhase("begin", 13);
                Check(engine.Jv(page, result, "tx", new string('x', 513)) == 0, "typed text bound");
                SuitePhase("end", 13);
                SuitePhase("begin", 14);
                Check(engine.Jv(page, null, "go", "https://example.com/") == 0, "no navigation capability");
                SuitePhase("end", 14);
                SuitePhase("begin", 15);
                var disposed = new Js(engine, page);
                SuitePhase("end", 15);
                SuitePhase("begin", 16);
                disposed.Dispose();
                SuitePhase("end", 16);
                SuitePhase("begin", 17);
                try { disposed.Ex("1;"); throw new Exception("disposed session accepted code"); }
                catch (ObjectDisposedException) { Check(true, "disposed session rejected"); }
                SuitePhase("end", 17);
                SuitePhase("begin", 18);
                Startup.Run(fixture);
                SuitePhase("end", 18);
                SuitePhase("begin", 19);
                ClassLists.ProfileContract(fixture);
                SuitePhase("end", 19);
                SuitePhase("begin", 20);
                BootstrapScopes.Run(attributeFixture);
                SuitePhase("end", 20);
                SuitePhase("begin", 21);
                Selectors.Run(selectorFixture);
                SuitePhase("end", 21);
                SuitePhase("begin", 22);
                View.Selectors(selectorFixture);
                SuitePhase("end", 22);
                SuitePhase("begin", 23);
                Tree.Run(treeFixture);
                SuitePhase("end", 23);
                SuitePhase("begin", 24);
                View.Tree(treeFixture);
                SuitePhase("end", 24);
                SuitePhase("begin", 25);
                Removal.Run(removalFixture, "core");
                SuitePhase("end", 25);
                SuitePhase("begin", 26);
                Removal.Run(removalFixture, "trusted");
                SuitePhase("end", 26);
                SuitePhase("begin", 27);
                Removal.Run(removalFixture, "isolated");
                SuitePhase("end", 27);
                SuitePhase("begin", 28);
                View.Pump(() => Removal.Native(removalFixture, null));
                SuitePhase("end", 28);
                SuitePhase("begin", 29);
                Worker.Run(fixture);
                SuitePhase("end", 29);
                SuitePhase("begin", 30);
                Events.Run(viewFixture);
                SuitePhase("end", 30);
                SuitePhase("begin", 31);
                Attributes.Run(attributeFixture);
                SuitePhase("end", 31);
                SuitePhase("begin", 32);
                View.Attributes(attributeFixture);
                SuitePhase("end", 32);
                SuitePhase("begin", 33);
                AttributeEdits.Run(attributeEditFixture);
                SuitePhase("end", 33);
                SuitePhase("begin", 34);
                View.AttributeEdits(attributeEditFixture);
                SuitePhase("end", 34);
                SuitePhase("begin", 35);
                ClassLists.Run(attributeFixture);
                SuitePhase("end", 35);
                SuitePhase("begin", 36);
                View.ClassLists(attributeFixture);
                SuitePhase("end", 36);
                SuitePhase("begin", 37);
                Microtasks.Run(viewFixture);
                SuitePhase("end", 37);
                SuitePhase("begin", 38);
                Timers.Run(viewFixture);
                SuitePhase("end", 38);
                SuitePhase("begin", 39);
                Frames.Run(viewFixture, "scheduler");
                SuitePhase("end", 39);
                SuitePhase("begin", 40);
                Frames.Run(viewFixture, "trusted");
                SuitePhase("end", 40);
                SuitePhase("begin", 41);
                Frames.Run(viewFixture, "isolated");
                SuitePhase("end", 41);
                SuitePhase("begin", 42);
                View.Pump(() => Frames.Native(viewFixture, null));
                SuitePhase("end", 42);
                SuitePhase("begin", 43);
                Clock.Run(viewFixture, "core");
                SuitePhase("end", 43);
                SuitePhase("begin", 44);
                Clock.Run(viewFixture, "trusted");
                SuitePhase("end", 44);
                SuitePhase("begin", 45);
                Clock.Run(viewFixture, "isolated");
                SuitePhase("end", 45);
                SuitePhase("begin", 46);
                View.Pump(() => Clock.Native(viewFixture, null));
                SuitePhase("end", 46);
                SuitePhase("begin", 47);
                AgentWait.Run(viewFixture, "core");
                SuitePhase("end", 47);
                SuitePhase("begin", 48);
                AgentWait.Run(viewFixture, "trusted");
                SuitePhase("end", 48);
                SuitePhase("begin", 49);
                AgentWait.Run(viewFixture, "isolated");
                SuitePhase("end", 49);
                SuitePhase("begin", 50);
                View.Pump(() => AgentWait.Native(viewFixture, null));
                SuitePhase("end", 50);
                SuitePhase("begin", 51);
                View.Run(viewFixture);
                SuitePhase("end", 51);
                SuitePhase("begin", 52);
                BudgetPhases.Run(viewFixture);
                SuitePhase("end", 52);
                SuitePhase("begin", 53);
                EventOptions.Run(viewFixture);
                SuitePhase("end", 53);
                SuitePhase("begin", 54);
                EventReceivers.Run(viewFixture);
                SuitePhase("end", 54);
                SuitePhase("begin", 55);
                ElementReceivers.Run(attributeFixture);
                SuitePhase("end", 55);
                SuitePhase("begin", 56);
                ListenerCalls.Run(viewFixture);
                SuitePhase("end", 56);
                // Keep this worker-file probe after the existing startup and execution gates.
                SuitePhase("begin", 57);
                ModernComponents.WorkerEntry();
                SuitePhase("end", 57);
                SuitePhase("begin", 58);
                WireFrames.Run();
                SuitePhase("end", 58);
                SuitePhase("begin", 59);
                FrameProgress.Run(fixture);
                SuitePhase("end", 59);
                SuitePhase("begin", 60);
                NativeCpu.Run(fixture);
                SuitePhase("end", 60);
                SuitePhase("begin", 61);
                WorkerStderr.Run();
                SuitePhase("end", 61);
                SuitePhase("begin", 62);
                SubtreeQueries.Run(selectorFixture, "aliases");
                SuitePhase("end", 62);
                SuitePhase("begin", 63);
                SubtreeQueries.Run(selectorFixture, "trusted");
                SuitePhase("end", 63);
                SuitePhase("begin", 64);
                SubtreeQueries.Run(selectorFixture, "isolated");
                SuitePhase("end", 64);
                SuitePhase("begin", 65);
                LoadFailures.Controlled(loadFailureFixture);
                SuitePhase("end", 65);
                SuitePhase("begin", 66);
                View.Pump(() => LoadFailures.Native(loadFailureFixture, null));
                SuitePhase("end", 66);
                SuitePhase("begin", 67);
                DocumentOrigins.Run(false);
                SuitePhase("end", 67);
                SuitePhase("begin", 68);
                DocumentOrigins.Run(true);
                SuitePhase("end", 68);
                SuitePhase("begin", 69);
                View.Pump(() => DocumentOrigins.Native(null));
                SuitePhase("end", 69);
                SuitePhase("begin", 70);
                VmOptions.Run();
                SuitePhase("end", 70);
                SuitePhase("begin", 71);
                BootstrapEntry.Contract();
                SuitePhase("end", 71);
                SuitePhase("begin", 72);
                ExecutionReports.Run(fixture);
                SuitePhase("end", 72);
                SuitePhase("begin", 73);
                BootstrapStages.Run(fixture);
                SuitePhase("end", 73);
                SuitePhase("begin", 74);
                var lazyStyleFixture = Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-styles.html");
                SuitePhase("end", 74);
                SuitePhase("begin", 75);
                LazyStyles.Run(lazyStyleFixture, "core");
                SuitePhase("end", 75);
                SuitePhase("begin", 76);
                LazyStyles.Run(lazyStyleFixture, "trusted");
                SuitePhase("end", 76);
                SuitePhase("begin", 77);
                LazyStyles.Run(lazyStyleFixture, "isolated");
                SuitePhase("end", 77);
                SuitePhase("begin", 78);
                LazyEvents.Run(lazyStyleFixture, "core");
                SuitePhase("end", 78);
                SuitePhase("begin", 79);
                LazyEvents.Run(lazyStyleFixture, "trusted");
                SuitePhase("end", 79);
                SuitePhase("begin", 80);
                LazyEvents.Run(lazyStyleFixture, "isolated");
                SuitePhase("end", 80);
                SuitePhase("begin", 81);
                View.Pump(() => LazyEvents.Native(Path.Combine(root, "LeaForge", "test-pages", "javascript-lazy-events.html"), null));
                SuitePhase("end", 81);
                SuitePhase("begin", 82);
                View.Pump(() => LazyStyles.Native(lazyStyleFixture, null));
                SuitePhase("end", 82);
                SuitePhase("begin", 83);
                var containsFixtureFull = Path.Combine(root, "LeaForge", "test-pages", "javascript-contains.html");
                SuitePhase("end", 83);
                SuitePhase("begin", 84);
                Contains.Run(containsFixtureFull, "aliases");
                SuitePhase("end", 84);
                SuitePhase("begin", 85);
                Contains.Run(containsFixtureFull, "trusted");
                SuitePhase("end", 85);
                SuitePhase("begin", 86);
                Contains.Run(containsFixtureFull, "isolated");
                SuitePhase("end", 86);
                SuitePhase("begin", 87);
                View.Pump(() => Contains.Native(containsFixtureFull, null));
                SuitePhase("end", 87);
                SuitePhase("begin", 88);
                Console.WriteLine("PASS Lea.Script: " + checks + " checks; JavaScript, isolated worker, and typed DOM.");
                SuitePhase("end", 88);
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
                return 1;
            }
            finally
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previousStartupTrace);
                var restored = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previousStartupTrace;
                Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE restored=" + (restored ? "true" : "false"));
                if (!restored) throw new InvalidOperationException("Diagnostic startup trace was not restored.");
            }
        }

        private static readonly System.Diagnostics.Stopwatch suitePhaseClock = System.Diagnostics.Stopwatch.StartNew();
        private static void SuitePhase(string edge, int stage)
        {
            Console.Error.WriteLine("LEA_SUITE_PHASE " + suitePhaseClock.ElapsedMilliseconds + " " + stage + " " + edge);
        }

        internal static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("FAIL " + name);
            checks++;
            Console.WriteLine("PASS " + name);
        }
    }
}
