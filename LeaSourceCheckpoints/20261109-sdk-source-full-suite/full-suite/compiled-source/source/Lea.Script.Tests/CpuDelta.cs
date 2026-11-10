using System;
using System.Globalization;

namespace Lea.Script.Tests
{
    internal static class CpuDelta
    {
        internal static string Report(string result, long start, long finish, long wall, long spins)
        {
            var delta = start >= 0 && finish >= start ?
                (finish - start).ToString(CultureInfo.InvariantCulture) : "unavailable";
            return String.Format(CultureInfo.InvariantCulture,
                "CPU work: result={0}; startTicks={1}; finishTicks={2}; deltaTicks={3}; wallMs={4}; spins={5}; targetTicks=1000000; wallLimitMs=3000",
                result, start, finish, delta, wall, spins);
        }
    }
}
