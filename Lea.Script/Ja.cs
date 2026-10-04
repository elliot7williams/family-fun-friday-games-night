using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lea.Engine;

namespace Lea.Script
{
    // The parent-owned capability surface shared by both script transports.
    internal sealed class Ja
    {
        private readonly E engine;
        private readonly P page;
        private readonly List<N> nodes = new List<N>();
        private readonly Dictionary<N, int> keys = new Dictionary<N, int>();
        internal int Calls { get; private set; }
        internal int Changes { get; private set; }
        private int microtasks;

        internal Ja(E engine, P page) { this.engine = engine; this.page = page; }
        internal void Begin() { Calls = 0; Changes = 0; microtasks = 0; }
        internal void Reset() { nodes.Clear(); keys.Clear(); }

        internal object[] Path(N target)
        {
            if (target == null) return new object[0];
            if (!engine.Jn(page, target)) throw new ArgumentException("The event target is not connected to the bound page.");
            var path = new List<object>();
            for (var current = target; current != null && current != page.D; current = current.Up)
            {
                if (path.Count == 128) throw new LimitException();
                path.Add(Key(current));
            }
            return path.ToArray();
        }

        internal static void EventType(string type)
        {
            if (type != "click" && type != "input" && type != "change" && type != "submit" && type != "DOMContentLoaded" && type != "load")
                throw new ArgumentException("Unsupported Lea event type.", "type");
        }

        internal object Call(object[] args)
        {
            if (++Calls > 256 || !engine.Je) throw new LimitException();
            if (args == null || args.Length < 1 || args.Length > 4) throw new InvalidDataException("Invalid bridge arguments.");
            var operation = Arg(args, 0);
            if (operation == "qm" && args.Length == 1)
            {
                if (++microtasks > 128) throw new LimitException();
                return null;
            }
            if (operation == "id" && args.Length == 2) return Key(engine.Id(page, Arg(args, 1)));
            if (operation == "q" && args.Length == 2) return Key(engine.Q(page, Arg(args, 1)));
            if (operation == "qa" && args.Length == 2)
            {
                var found = engine.Qa(page, Arg(args, 1));
                var result = new object[found.Count];
                for (var index = 0; index < result.Length; index++) result[index] = Key(found[index]);
                return result;
            }
            if (operation == "t" && args.Length == 1) return page.T ?? "";
            if (operation == "put" && args.Length == 4)
            {
                var count = engine.Jv(page, Node(args, 1), Arg(args, 2), Arg(args, 3));
                Changes += count;
                return count == 1;
            }
            var target = Node(args, 1);
            if (operation == "tag" && args.Length == 2) return target == null ? null : target.K.ToUpperInvariant();
            if (operation == "attr" && args.Length == 3)
            {
                var name = Arg(args, 2);
                string value;
                return target != null && target.A.TryGetValue(name, out value) ? value : null;
            }
            if (operation == "text" && args.Length == 2)
            {
                if (target == null) return null;
                var text = new StringBuilder();
                Text(target, text);
                return text.ToString();
            }
            if ((operation == "value" || operation == "checked") && args.Length == 2)
            {
                if (target == null) return null;
                foreach (var field in page.Fs)
                    if (Object.ReferenceEquals(field.El, target))
                        return operation == "value" ? (object)field.V : field.C;
                return operation == "value" ? (object)target.Av("value") : false;
            }
            throw new InvalidDataException("Unsupported bridge operation.");
        }

        private object Key(N node)
        {
            if (node == null) return null;
            int key;
            if (!keys.TryGetValue(node, out key))
            {
                if (nodes.Count == 512) throw new LimitException();
                key = nodes.Count;
                nodes.Add(node);
                keys.Add(node, key);
            }
            return key;
        }

        private N Node(object[] args, int index)
        {
            if (index >= args.Length) throw new InvalidDataException("Missing node handle.");
            if (args[index] == null) return null;
            if (!(args[index] is int)) throw new InvalidDataException("Invalid node handle.");
            var key = (int)args[index];
            if (key < 0 || key >= nodes.Count) throw new InvalidDataException("Unknown node handle.");
            return nodes[key];
        }

        private static string Arg(object[] args, int index)
        {
            if (index >= args.Length || !(args[index] is string)) throw new InvalidDataException("Invalid string argument.");
            var value = (string)args[index];
            if (value.Length > 1024) throw new LimitException();
            return value;
        }

        private static void Text(N node, StringBuilder text)
        {
            if (node.K == "#text")
            {
                if (text.Length + node.X.Length > 16 * 1024) throw new LimitException();
                text.Append(node.X);
            }
            else foreach (var child in node.C) Text(child, text);
        }

        internal sealed class LimitException : Exception { }
    }
}
