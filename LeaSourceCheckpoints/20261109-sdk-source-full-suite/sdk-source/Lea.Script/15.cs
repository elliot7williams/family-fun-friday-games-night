using System;
using System.IO;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Lea.Script
{
    internal sealed class Dc
    {
        private static readonly string[] names = {
            "INDEX_SIZE_ERR", "DOMSTRING_SIZE_ERR", "HIERARCHY_REQUEST_ERR", "WRONG_DOCUMENT_ERR",
            "INVALID_CHARACTER_ERR", "NO_DATA_ALLOWED_ERR", "NO_MODIFICATION_ALLOWED_ERR", "NOT_FOUND_ERR",
            "NOT_SUPPORTED_ERR", "INUSE_ATTRIBUTE_ERR", "INVALID_STATE_ERR", "SYNTAX_ERR", "INVALID_MODIFICATION_ERR",
            "NAMESPACE_ERR", "INVALID_ACCESS_ERR", "VALIDATION_ERR", "TYPE_MISMATCH_ERR", "SECURITY_ERR", "NETWORK_ERR",
            "ABORT_ERR", "URL_MISMATCH_ERR", "QUOTA_EXCEEDED_ERR", "TIMEOUT_ERR", "INVALID_NODE_TYPE_ERR", "DATA_CLONE_ERR"
        };
        private readonly int thread = Thread.CurrentThread.ManagedThreadId;
        private Jint.Engine bound;
        private bool used;

        internal void Bind(Jint.Engine engine)
        {
            CheckThread();
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (bound != null) throw new InvalidOperationException("DOM constants bind to one realm.");
            bound = engine;
            engine.SetValue("__dc", (JsValue)new ClrFunction(engine, "__dc", Install));
        }

        private void CheckThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread)
                throw new InvalidOperationException("DOM constants require the owning thread.");
        }

        private JsValue Install(JsValue receiver, JsValue[] args)
        {
            CheckThread();
            if (used || args.Length != 2 || !(args[0] is Jint.Native.Function.Function) || !args[1].IsObject())
                throw new InvalidDataException("Invalid DOM constant initialization.");
            var constructor = args[0].AsObject();
            var prototype = args[1].AsObject();
            if (!ReferenceEquals(constructor.Engine, bound) || !ReferenceEquals(prototype.Engine, bound) ||
                !ReferenceEquals(constructor.GetOwnProperty("prototype").Value, prototype))
                throw new InvalidDataException("DOM constant realm or prototype mismatch.");
            used = true;
            for (var index = 0; index < names.Length; index++)
            {
                // Independent immutable descriptors; no shared mutable VM descriptor state.
                var value = (JsValue)(index + 1);
                if (!constructor.DefineOwnProperty(names[index], new PropertyDescriptor(value, false, true, false)) ||
                    !prototype.DefineOwnProperty(names[index], new PropertyDescriptor(value, false, true, false)))
                    throw new InvalidDataException("DOM constant initialization rejected.");
            }
            return JsValue.Undefined;
        }
    }
}
