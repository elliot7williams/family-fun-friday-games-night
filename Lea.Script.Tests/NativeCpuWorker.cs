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
                    var start = process.TotalProcessorTime.Ticks;
                    var clock = Stopwatch.StartNew();
                    long current, spins = 0;
                    while ((current = process.TotalProcessorTime.Ticks) - start < 1000000)
                    {
                        if (clock.ElapsedMilliseconds >= 3000)
                        {
                            Console.Error.WriteLine(CpuDelta.Report("timeout", start, current, clock.ElapsedMilliseconds, spins));
                            return 3;
                        }
                        Thread.SpinWait(10000);
                        spins++;
                    }
                    if (output != null)
                        Console.Error.WriteLine(CpuDelta.Report("complete", start, current, clock.ElapsedMilliseconds, spins));
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
