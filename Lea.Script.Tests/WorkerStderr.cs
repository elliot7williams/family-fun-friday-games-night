using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Lea.Script;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class WorkerStderr
    {
        internal static void Run()
        {
            foreach (var assembly in new[] { typeof(Jp).Assembly, typeof(V).Assembly })
            {
                var type = assembly.GetType("Lea.Win.Rp", true);
                var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
                foreach (var sample in new[]
                {
                    new { text = "", chunks = 1 },
                    new { text = "startup parsed\nstartup dom\n", chunks = 1 },
                    new { text = "\ufeffstartup dom\n", chunks = 1 },
                    new { text = "x" + new string('\u20ac', 349) + "\nstartup probes\n", chunks = 1 },
                    new { text = new string('x', 1015) + "\u20ac\u00e9\nstartup dom\n", chunks = 17 },
                    new { text = new string('a', 5000) + "\nstartup parsed\nstartup dom\nstartup probes\n", chunks = 37 }
                })
                {
                    var expected = sample.text.Substring(Math.Max(0, sample.text.Length - 1024));
                    using (var input = new Chunks(Encoding.UTF8.GetBytes(sample.text), sample.chunks))
                    using (var worker = (IDisposable)constructor.Invoke(new object[] { null, null, null, input, null, true, true, true, true, true }))
                    {
                        var property = type.GetProperty("E");
                        var done = SpinWait.SpinUntil(() => input.Ended && (string)property.GetValue(worker, null) == expected, 3000);
                        var actual = (string)property.GetValue(worker, null);
                        Console.WriteLine("Worker stderr " + assembly.GetName().Name + ": source=" + sample.text.Length +
                            "; chunks=" + sample.chunks + "; retained=" + actual.Length);
                        Program.Check(done && actual == expected && actual.Length <= 1024,
                            assembly.GetName().Name + ": stderr retains the exact bounded UTF-8 text tail; source=" + sample.text.Length + "; chunks=" + sample.chunks);
                    }
                }
            }
        }

        private sealed class Chunks : MemoryStream
        {
            private readonly int maximum;
            private volatile bool ended;
            internal bool Ended { get { return ended; } }
            internal Chunks(byte[] bytes, int maximum) : base(bytes, false) { this.maximum = maximum; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                var read = base.Read(buffer, offset, Math.Min(count, maximum));
                if (read == 0) ended = true;
                return read;
            }
        }
    }
}
