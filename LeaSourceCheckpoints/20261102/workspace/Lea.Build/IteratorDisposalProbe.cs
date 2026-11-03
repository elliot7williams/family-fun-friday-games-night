using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

public static class IteratorDisposalProbe
{
    private static string Value(object item)
    {
        if (item == null) return "null";
        if (item is string text) return text;
        var type = item.GetType();
        if (type.Name == "B") return (string)type.GetProperty("X").GetValue(item);
        return string.Join("|", type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(f => f.Name, StringComparer.Ordinal).Select(f => f.Name + "=" + f.GetValue(item)));
    }

    public static object[] Run(string path)
    {
        var context = new AssemblyLoadContext("Lea iterator probe " + Guid.NewGuid(), true);
        try
        {
            var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(path));
            var engine = assembly.GetType("Lea.Engine.E", true);
            var block = assembly.GetType("Lea.Engine.B", true);
            var blocks = Array.CreateInstance(block, 3);
            for (int i = 0; i < 2; i++) {
                var value = Activator.CreateInstance(block, true);
                block.GetProperty("X").SetValue(value, "block" + i);
                blocks.SetValue(value, i);
            }
            var cases = new[] {
                ("Tokens", (object)"<div a='1'>first<span>second</span></div>"),
                ("CssGroups", (object)"div, span[data-x='a,b'], a:is(.x,.y)"),
                ("AccessibilityBlocks", (object)blocks)
            };
            var results = new List<object>();
            foreach (var test in cases)
            {
                var method = engine.GetMethod(test.Item1, BindingFlags.Static | BindingFlags.NonPublic);
                IEnumerable Sequence() => (IEnumerable)method.Invoke(null, new object[] { test.Item2 });
                var items = new List<string>();
                var complete = Sequence().GetEnumerator();
                try { while (complete.MoveNext()) items.Add(Value(complete.Current)); }
                finally { (complete as IDisposable)?.Dispose(); }
                var partial = Sequence().GetEnumerator();
                bool first, resumed;
                string resumedValue = null;
                try {
                    first = partial.MoveNext();
                    if (!first) throw new InvalidDataException("Fixture must yield before disposal.");
                    ((IDisposable)partial).Dispose();
                    resumed = partial.MoveNext();
                    if (resumed) resumedValue = Value(partial.Current);
                } finally { (partial as IDisposable)?.Dispose(); }
                results.Add(new { method=test.Item1, items=items.ToArray(), movedAfterDispose=resumed, resumedValue });
            }
            return results.ToArray();
        }
        finally { context.Unload(); }
    }
}
