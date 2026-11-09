using System;
using System.IO;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Lea.Script
{
    internal sealed class Gd
    {
        private static readonly string[] names = {
            "setTimeout", "setInterval", "clearTimeout", "clearInterval",
            "requestAnimationFrame", "cancelAnimationFrame"
        };
        private readonly int thread = Thread.CurrentThread.ManagedThreadId;
        private Jint.Engine bound;
        private bool used;

        internal void Bind(Jint.Engine engine)
        {
            CheckThread();
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (bound != null) throw new InvalidOperationException("Window globals bind to one realm.");
            bound = engine;
            engine.SetValue("__wg", (JsValue)new ClrFunction(engine, "__wg", Install));
        }

        private void CheckThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread)
                throw new InvalidOperationException("Window globals require the owning thread.");
        }

        private JsValue Install(JsValue receiver, JsValue[] args)
        {
            CheckThread();
            if (used || args.Length != names.Length)
                throw new InvalidDataException("Invalid window global initialization.");
            foreach (var value in args)
                if (!(value is Jint.Native.Function.Function) || !ReferenceEquals(value.AsObject().Engine, bound))
                    throw new InvalidDataException("Window global function realm mismatch.");
            used = true;
            for (var index = 0; index < names.Length; index++)
                if (!bound.Global.DefineOwnProperty(names[index], new PropertyDescriptor(args[index], true, true, true)))
                    throw new InvalidDataException("Window global initialization rejected.");
            return JsValue.Undefined;
        }
    }
}
