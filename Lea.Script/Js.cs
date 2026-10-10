using System;
using System.IO;
using System.Threading;
using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lea.Engine;

namespace Lea.Script
{
    // Trusted in-process option. Use Jp for the isolated worker experiment.
    public sealed class Js : IDisposable
    {
        private readonly E engine;
        private readonly P page;
        private readonly int thread;
        private readonly Ja bridge;
        private readonly Func<Action<Options>, Jint.Engine> factory;
        private Jint.Engine vm;
        private Jint.Native.JsValue dispatch;
        private bool disposed;
        private bool running;
        private long generation;
        private static readonly string bootstrap = LoadBootstrap();
        private static readonly ScriptPreparationOptions preparation = new ScriptPreparationOptions
        {
            StaticAnalysis = true,
            FoldConstants = false,
            ParsingOptions = new ScriptParsingOptions
            {
                CompileRegex = false,
                RegexTimeout = TimeSpan.FromMilliseconds(100)
            }
        };
        // Share only the fixed trusted syntax tree, never a realm or page script.
        private static readonly Lazy<Prepared<Acornima.Ast.Script>> prepared =
            new Lazy<Prepared<Acornima.Ast.Script>>(() => Jint.Engine.PrepareScript(bootstrap, options: preparation));

        public Js(E engine, P page) : this(engine, page, null) { }

        internal Js(E engine, P page, Func<Action<Options>, Jint.Engine> factory)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (page == null) throw new ArgumentNullException("page");
            engine.Q(page, "body");
            this.engine = engine;
            this.page = page;
            this.factory = factory;
            bridge = new Ja(engine, page);
            thread = Thread.CurrentThread.ManagedThreadId;
            generation = engine.Jg;
        }

        public Jr At { get; private set; }
        public int Td { get { CheckThread(); return disposed || !engine.Je || generation != engine.Jg || engine.At != page ? -1 : bridge.Timers.Td; } }
        public int Tn { get { CheckThread(); return bridge.Timers.N; } }

        public Jr Ex(string code) { return Run(code, null, null); }
        public Jr Tk() { return Run(null, null, null, true); }

        public Jr Ev(N target, string type)
        { Ja.EventType(type); return Run(null, target, type); }

        private Jr Run(string code, N target, string type, bool timer = false)
        {
            CheckThread();
            if (disposed) throw new ObjectDisposedException("Js");
            if (running) throw new InvalidOperationException("A Lea script is already running.");
            bridge.Begin();
            if (generation != engine.Jg) { Reset(); generation = engine.Jg; }
            if (!engine.Je) { Reset(); return Result("off", "Scripting is disabled."); }
            if (timer && engine.At != page) { Reset(); return Result("stale", "The bound page is no longer active."); }
            engine.Q(page, "body");
            if (!timer && type == null && (code == null || code.Length > 16 * 1024)) return Result("code", "Code must be at most 16 KiB.");
            if (type != null && target != null && !engine.Jn(page, target)) return Result("node", "Event target is not connected to the bound page.");
            var token = timer ? bridge.Timers.Take() : 0;
            if (timer && token == 0) return Result("ok", "");
            running = true;
            var phase = "bootstrap";
            try
            {
                if (vm == null) Start();
                if (timer)
                {
                    phase = "timer";
                    vm.Invoke(dispatch, Jint.Native.JsValue.Undefined,
                        new[] { (Jint.Native.JsValue)"tm", Codec.Value(vm, new object[] { token }) });
                    return Result("ok", "");
                }
                if (type == null)
                {
                    phase = "script";
                    vm.Execute(code);
                    phase = "checkpoint";
                    vm.Invoke(dispatch, Jint.Native.JsValue.Undefined, new[] { (Jint.Native.JsValue)"mt", Codec.Value(vm, new object[0]) });
                    return Result("ok", "");
                }
                phase = "event";
                var result = vm.Invoke(dispatch, Jint.Native.JsValue.Undefined,
                    new[] { (Jint.Native.JsValue)type, Codec.Value(vm, bridge.Path(target)) });
                return Result("ok", "", result.AsBoolean());
            }
            catch (Exception error) when (error is JavaScriptException || error is JintException ||
                error is Acornima.ParseErrorException || error is TimeoutException ||
                error is Ja.LimitException || error is InvalidDataException)
            {
                var limited = error is TimeoutException || error is Ja.LimitException ||
                    error is StatementsCountOverflowException || error is MemoryLimitExceededException ||
                    error is RecursionDepthOverflowException;
                Reset();
                var message = limited ? "Script budget exceeded. phase=" + phase + "; reason=" + error.GetType().Name : error.Message;
                return Result(limited ? "limit" : "js", message.Length <= 512 ? message : message.Substring(0, 512));
            }
            finally { if (timer) bridge.Timers.End(); running = false; }
        }

        private void Start()
        {
            vm = factory == null ? new Jint.Engine(Vo.Of(1000)) :
                factory(options => Vo.At(options, 1000));
            vm.SetValue("__lea", (Jint.Native.JsValue)new ClrFunction(vm, "__lea", (_, args) =>
                Codec.Value(vm, bridge.Call(Codec.Arguments(args)))));
            vm.SetValue("__jobs", (Jint.Native.JsValue)new ClrFunction(vm, "__jobs", (_, args) =>
            { vm.Advanced.ProcessTasks(); return Jint.Native.JsValue.Undefined; }));
            dispatch = vm.Evaluate(prepared.Value);
            bridge.Begin();
        }

        private Jr Result(string status, string message, bool prevented = false)
        { return At = new Jr(status, bridge.Changes, message, prevented); }
        private void Reset() { vm = null; dispatch = null; bridge.Reset(); }
        private void CheckThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread)
                throw new InvalidOperationException("Use this Lea script session on its owning thread.");
        }

        public void Dispose()
        {
            CheckThread();
            if (running) throw new InvalidOperationException("A Lea script is running.");
            disposed = true;
            Reset();
        }

        private static string LoadBootstrap()
        {
            using (var source = typeof(Js).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
            using (var reader = new StreamReader(source)) return reader.ReadToEnd();
        }
    }
}
