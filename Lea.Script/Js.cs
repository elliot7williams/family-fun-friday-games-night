using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lea.Engine;

namespace Lea.Script
{
    // Trusted-script experiment. Do not enable automatic website execution here.
    public sealed class Js : IDisposable
    {
        private readonly E engine;
        private readonly P page;
        private readonly int thread;
        private readonly List<N> nodes = new List<N>();
        private readonly Dictionary<N, int> keys = new Dictionary<N, int>();
        private Jint.Engine vm;
        private bool disposed;
        private bool running;
        private int calls;
        private int changes;
        private long generation;
        private static readonly string bootstrap = LoadBootstrap();

        public Js(E engine, P page)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (page == null) throw new ArgumentNullException("page");
            engine.Q(page, "body");
            this.engine = engine;
            this.page = page;
            thread = Thread.CurrentThread.ManagedThreadId;
            generation = engine.Jg;
        }

        public Jr At { get; private set; }

        public Jr Ex(string code)
        {
            CheckThread();
            if (disposed) throw new ObjectDisposedException("Js");
            if (running) throw new InvalidOperationException("A Lea script is already running.");
            changes = 0;
            calls = 0;
            if (generation != engine.Jg) { Reset(); generation = engine.Jg; }
            if (!engine.Je) { Reset(); return Result("off", "Scripting is disabled."); }
            engine.Q(page, "body");
            if (code == null || code.Length > 16 * 1024) return Result("code", "Code must be at most 16 KiB.");
            running = true;
            try
            {
                if (vm == null) Start();
                vm.Execute(code);
                return Result("ok", "");
            }
            catch (Exception error) when (error is JavaScriptException || error is JintException ||
                error is Acornima.ParseErrorException || error is TimeoutException || error is BridgeLimitException)
            {
                var limited = error is TimeoutException || error is BridgeLimitException ||
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
            vm.SetValue("__lea", new ClrFunction(vm, "__lea", (_, args) => Bridge(args)));
            vm.Execute(bootstrap);
            calls = 0;
        }

        private JsValue Bridge(JsValue[] args)
        {
            if (++calls > 256) throw new BridgeLimitException();
            if (!engine.Je) throw new BridgeLimitException();
            var operation = Arg(args, 0);
            if (operation == "id") return Key(engine.Id(page, Arg(args, 1)));
            if (operation == "q") return Key(engine.Q(page, Arg(args, 1)));
            if (operation == "qa")
            {
                var found = engine.Qa(page, Arg(args, 1));
                var values = new JsValue[found.Count];
                for (var index = 0; index < values.Length; index++) values[index] = Key(found[index]);
                return new JsArray(vm, values);
            }
            if (operation == "t") return page.T ?? "";
            if (operation == "put")
            {
                var node = Node(args, 1);
                var count = engine.Jv(page, node, Arg(args, 2), Arg(args, 3));
                changes += count;
                return count == 1;
            }
            var target = Node(args, 1);
            if (target == null) return JsValue.Null;
            if (operation == "tag") return target.K.ToUpperInvariant();
            if (operation == "attr")
            {
                string value;
                return target.A.TryGetValue(Arg(args, 2), out value) ? (JsValue)value : JsValue.Null;
            }
            if (operation == "text")
            {
                var text = new StringBuilder();
                Text(target, text);
                return text.ToString();
            }
            if (operation == "value" || operation == "checked")
            {
                foreach (var field in page.Fs)
                    if (Object.ReferenceEquals(field.El, target))
                        return operation == "value" ? (JsValue)field.V : (JsValue)field.C;
                return operation == "value" ? (JsValue)target.Av("value") : (JsValue)false;
            }
            return JsValue.Undefined;
        }

        private JsValue Key(N node)
        {
            if (node == null) return JsValue.Null;
            int key;
            if (!keys.TryGetValue(node, out key))
            {
                if (nodes.Count == 512) throw new BridgeLimitException();
                key = nodes.Count;
                nodes.Add(node);
                keys.Add(node, key);
            }
            return key;
        }

        private N Node(JsValue[] args, int index)
        {
            if (index >= args.Length || args[index].IsNull()) return null;
            if (!args[index].IsNumber()) throw new BridgeLimitException();
            var number = args[index].AsNumber();
            if (number < 0 || number >= nodes.Count || number != Math.Truncate(number)) throw new BridgeLimitException();
            return nodes[(int)number];
        }

        private static string Arg(JsValue[] args, int index)
        {
            if (index >= args.Length || !args[index].IsString()) throw new BridgeLimitException();
            var value = args[index].AsString();
            if (value.Length > 1024) throw new BridgeLimitException();
            return value;
        }

        private static void Text(N node, StringBuilder text)
        {
            if (node.K == "#text")
            {
                if (text.Length + node.X.Length > 16 * 1024) throw new BridgeLimitException();
                text.Append(node.X);
            }
            else foreach (var child in node.C) Text(child, text);
        }

        private Jr Result(string status, string message)
        { return At = new Jr(status, changes, message); }

        private void Reset() { vm = null; nodes.Clear(); keys.Clear(); }
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

        private sealed class BridgeLimitException : Exception { }
    }
}
