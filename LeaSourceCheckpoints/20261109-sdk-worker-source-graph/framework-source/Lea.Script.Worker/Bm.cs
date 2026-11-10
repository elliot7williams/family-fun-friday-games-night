using System;
using System.IO;
using Jint;
using Jint.Native;

namespace Lea.Script
{
    internal sealed class Bm
    {
        // Only the trusted bootstrap sends arguments; page microtasks use an empty call.
        private static readonly string[] stages = { "declarations", "scope", "events", "interfaces", "elements", "document" };
        private int next;

        internal string Of(JsValue[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length == 0) return null;
            if (next == stages.Length || values.Length != 1 || !values[0].IsString() || values[0].AsString() != stages[next])
                throw new InvalidDataException("Invalid bootstrap checkpoint.");
            return stages[next++];
        }

        internal void End()
        {
            if (next != stages.Length) throw new InvalidDataException("Incomplete bootstrap checkpoints.");
        }
    }
}
