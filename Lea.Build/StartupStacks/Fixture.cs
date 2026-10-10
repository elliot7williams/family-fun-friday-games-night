using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lea.Diagnostics
{
    internal static class StackFixture
    {
        private static void Main() { WaitForSnapshot(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WaitForSnapshot()
        {
            Console.WriteLine("Lea stack fixture ready");
            Thread.Sleep(60000);
        }
    }
}
