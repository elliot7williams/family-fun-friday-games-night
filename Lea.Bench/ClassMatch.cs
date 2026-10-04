using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Lea.Engine;

namespace Lea.Bench
{
    internal static class ClassMatch
    {
        internal static int Run(int rounds)
        {
            var path = Path.Combine(Path.GetTempPath(), "lea-class-bench-" + Guid.NewGuid().ToString("N") + ".html");
            var allocate = AllocationCounter();
            try
            {
                File.WriteAllText(path, Fixture(), new UTF8Encoding(false));
                var engine = new E();
                var page = engine.Go(path);
                var count = page.N;
                Console.WriteLine("Case\tRounds\tOperations\tMatches\tMsMedian\tBytesMedian\tGen0");
                Measure("load", rounds, () => new E().Go(path).N, count, allocate);
                var cases = new Dictionary<string, int>
                {
                    { "p", 256 }, { ".alpha", 256 }, { ".alpha.beta.gamma", 256 },
                    { ".missing", 0 }, { ".alpha.missing", 0 }, { "[class~='gamma']", 256 },
                    { ".alpha:has(> .omega)", 256 },
                    { String.Concat(Enumerable.Range(0, 32).Select(index => ".c" + index)), 1 },
                    { String.Concat(Enumerable.Repeat(".c0", 80)), 1 }
                };
                foreach (var item in cases)
                    Measure(item.Key, rounds, () => engine.Qa(page, item.Key).Count, item.Value, allocate);
                var node = engine.Id(page, "row0");
                var alternate = false;
                Measure("class-write", rounds, () =>
                {
                    alternate = !alternate;
                    return engine.Jv(page, node, "cn", "alpha beta gamma delta epsilon zeta" + (alternate ? " changed" : ""));
                }, 1, allocate);
                if (engine.Id(page, "row0") != node) throw new InvalidOperationException("Benchmark mutation lost its node.");
                return 0;
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static string Fixture()
        {
            var source = new StringBuilder("<!doctype html><html><head><title>Lea class benchmark</title><style>p.alpha{color:#168477}.alpha.beta{padding:2px}.delta.missing{display:none}span.omega{font-weight:bold}[class~=gamma]{margin:1px}.changed{margin-left:4px}</style></head><body>");
            for (var index = 0; index < 256; index++)
                source.Append("<p id=row" + index + " class='alpha beta gamma delta epsilon zeta'><span class='omega child'>Lea</span></p>");
            source.Append("<p class='");
            for (var index = 0; index < 32; index++) source.Append("c" + index + " ");
            source.Append("'>Dense compound</p></body></html>");
            return source.ToString();
        }

        private static void Measure(string name, int rounds, Func<int> operation, int matches, Func<long> allocate)
        {
            const int batch = 10;
            for (var index = 0; index < 3; index++) Need(operation(), matches);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var times = new double[rounds];
            var bytes = new long[rounds];
            var gen0 = GC.CollectionCount(0);
            for (var round = 0; round < rounds; round++)
            {
                var clock = new Stopwatch();
                var before = allocate == null ? 0 : allocate();
                clock.Start();
                for (var index = 0; index < batch; index++) Need(operation(), matches);
                clock.Stop();
                bytes[round] = allocate == null ? -1 : (allocate() - before) / batch;
                times[round] = clock.Elapsed.TotalMilliseconds / batch;
            }
            Array.Sort(times);
            Array.Sort(bytes);
            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", name, rounds, rounds * batch, matches,
                times[rounds / 2].ToString("F4", CultureInfo.InvariantCulture), bytes[rounds / 2], GC.CollectionCount(0) - gen0);
        }

        private static void Need(int actual, int expected)
        { if (actual != expected) throw new InvalidOperationException("Benchmark result mismatch: " + actual + " != " + expected); }

        private static Func<long> AllocationCounter()
        {
            var method = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread", BindingFlags.Public | BindingFlags.Static);
            return method == null ? null : (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), method);
        }
    }
}
