using System;

internal sealed class CombinedPrefixComplete : Exception
{
    internal static void Stop() { throw new CombinedPrefixComplete(); }
}
