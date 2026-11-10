using System;

namespace Lea.Engine
{
    internal static partial class Hn
    {
        internal static bool A(char ch)
        { return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9'; }

        internal static bool T(string value, int start, bool attribute, out string replacement, out int length)
        {
            replacement = null;
            length = 0;
            int limit = Math.Min(M, value.Length - start), count = 0;
            while (count < limit && A(value[start + count])) count++;
            if (count < limit && value[start + count] == ';') count++;
            // Longest match first; lookups compare slices without allocating substrings.
            for (int n = count; n > 0; n--)
            {
                int low = 0, high = Keys.Length - 1;
                while (low <= high)
                {
                    int mid = low + (high - low) / 2;
                    string key = Keys[mid];
                    int order = String.CompareOrdinal(value, start, key, 0, Math.Min(n, key.Length));
                    if (order == 0) order = n.CompareTo(key.Length);
                    if (order < 0) high = mid - 1;
                    else if (order > 0) low = mid + 1;
                    else
                    {
                        int next = start + n;
                        if (attribute && key[key.Length - 1] != ';' && next < value.Length &&
                            (value[next] == '=' || A(value[next]))) return false;
                        replacement = Values[mid]; length = n; return true;
                    }
                }
            }
            return false;
        }
    }
}
