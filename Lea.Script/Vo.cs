using System;
using Jint;

namespace Lea.Script
{
    internal static class Vo
    {
        internal static Options Of(int milliseconds)
        {
            var options = new Options();
            At(options, milliseconds);
            return options;
        }

        internal static void At(Options options, int milliseconds)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (milliseconds != 1000 && milliseconds != 5000)
                throw new ArgumentOutOfRangeException("milliseconds");
            options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                .TimeoutInterval(TimeSpan.FromMilliseconds(milliseconds)).LimitRecursion(64)
                .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
            options.Constraints.StackOverflowGuard = true;
            options.Interop.Enabled = false;
            options.Interop.AllowGetType = false;
            options.Interop.AllowWrite = false;
        }
    }
}
