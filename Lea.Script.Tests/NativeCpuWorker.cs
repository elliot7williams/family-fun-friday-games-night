using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Lea.Script.Tests
{
    internal static class NativeCpuWorker
    {
        private static int Main(string[] args)
        {
            if (args.Length > 2 || (args.Length > 0 && args[0] != "--trace") ||
                (args.Length > 1 && args[1] != "--hold")) return 64;
            using (var output = args.Length == 0 ? null : Console.OpenStandardOutput())
            {
                Mark(output, 'i');
                if (Lea.Image.Mg.Apply() != 15) return 2;
                Mark(output, 'm');
                using (var input = Console.OpenStandardInput()) Mark(output, input.ReadByte() == -1 ? 'e' : 'd');
                using (var process = Process.GetCurrentProcess())
                {
                    var start = process.TotalProcessorTime;
                    var clock = Stopwatch.StartNew();
                    while ((process.TotalProcessorTime - start).TotalMilliseconds < 100)
                    {
                        if (clock.ElapsedMilliseconds >= 3000) return 3;
                        Thread.SpinWait(10000);
                    }
                }
                Mark(output, 'c');
                if (args.Length == 2) { Mark(output, 'h'); Thread.Sleep(Timeout.Infinite); }
                Mark(output, 'x');
            }
            return 0;
        }

        private static void Mark(Stream output, char marker)
        {
            if (output == null) return;
            output.WriteByte((byte)marker);
            output.Flush();
        }
    }
}
