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
        private Jint.Engine vm;
        private Jint.Native.JsValue dispatch;
        private bool disposed;
        private bool running;
        private long generation;
        private static readonly string bootstrap = LoadBootstrap();

        public Js(E engine, P page)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (page == null) throw new ArgumentNullException("page");
            engine.Q(page, "body");
            this.engine = engine;
            this.page = page;
            bridge = new Ja(engine, page);
            thread = Thread.CurrentThread.ManagedThreadId;
            generation = engine.Jg;
        }

        public Jr At { get; private set; }

        public Jr Ex(string code) { return Run(code, null, null); }

        public Jr Ev(N target, string type)
        { Ja.EventType(type); return Run(null, target, type); }

        private Jr Run(string code, N target, string type)
        {
            CheckThread();
            if (disposed) throw new ObjectDisposedException("Js");
            if (running) throw new InvalidOperationException("A Lea script is already running.");
            bridge.Begin();
            if (generation != engine.Jg) { Reset(); generation = engine.Jg; }
            if (!engine.Je) { Reset(); return Result("off", "Scripting is disabled."); }
            engine.Q(page, "body");
            if (type == null && (code == null || code.Length > 16 * 1024)) return Result("code", "Code must be at most 16 KiB.");
            if (type != null && target != null && !engine.Jn(page, target)) return Result("node", "Event target is not connected to the bound page.");
            running = true;
            try
            {
                if (vm == null) Start();
                if (type == null) { vm.Execute(code); return Result("ok", ""); }
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
                var message = limited ? "Script budget exceeded." : error.Message;
                return Result(limited ? "limit" : "js", message.Length <= 512 ? message : message.Substring(0, 512));
            }
            finally { running = false; }
        }

        private void Start()
        {
            vm = new Jint.Engine(options =>
            {
                options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                    .TimeoutInterval(TimeSpan.FromSeconds(1)).LimitRecursion(64)
                    .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                options.Constraints.StackOverflowGuard = true;
                options.Interop.Enabled = false;
                options.Interop.AllowGetType = false;
                options.Interop.AllowWrite = false;
            });
            vm.SetValue("__lea", new ClrFunction(vm, "__lea", (_, args) =>
                Codec.Value(vm, bridge.Call(Codec.Arguments(args)))));
            dispatch = vm.Evaluate(bootstrap);
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
