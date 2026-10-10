using System;
using System.Diagnostics;
using System.Threading;

namespace Lea.Script.Tests
{
    internal static class NativeCpuWorker
    {
        private static int Main()
        {
            if (Lea.Image.Mg.Apply() != 15) return 2;
            using (var input = Console.OpenStandardInput()) input.ReadByte();
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
            return 0;
        }
    }
}
