using System;
using System.Text;

namespace Lea.Engine
{
    // Decode references once at the tokenizer boundary, never on decoded output.
    internal static class Hc
    {
        private const string C1 = "\u20ac\u0081\u201a\u0192\u201e\u2026\u2020\u2021\u02c6\u2030\u0160\u2039\u0152\u008d\u017d\u008f" +
            "\u0090\u2018\u2019\u201c\u201d\u2022\u2013\u2014\u02dc\u2122\u0161\u203a\u0153\u009d\u017e\u0178";

        internal static string D(string value, bool attribute = false)
        {
            if (String.IsNullOrEmpty(value)) return value ?? "";
            StringBuilder result = null;
            var literal = 0;
            var scan = 0;
            while (scan < value.Length)
            {
                var at = value.IndexOf('&', scan);
                if (at < 0) break;
                scan = at + 1;
                if (scan >= value.Length) break;
                if (value[scan] != '#')
                {
                    string replacement; int length;
                    if (!Hn.T(value, scan, attribute, out replacement, out length)) continue;
                    if (result == null) result = new StringBuilder(value.Length);
                    result.Append(value, literal, at - literal).Append(replacement);
                    literal = scan += length;
                    continue;
                }
                var end = at + 2;
                var radix = 10;
                if (end < value.Length && (value[end] == 'x' || value[end] == 'X')) { radix = 16; end++; }
                var first = end;
                var code = 0;
                while (end < value.Length)
                {
                    var ch = value[end];
                    var digit = ch >= '0' && ch <= '9' ? ch - '0' :
                        radix == 16 && ch >= 'a' && ch <= 'f' ? ch - 'a' + 10 :
                        radix == 16 && ch >= 'A' && ch <= 'F' ? ch - 'A' + 10 : -1;
                    if (digit < 0) break;
                    // Saturation consumes arbitrarily long references without integer overflow.
                    code = code > (0x110000 - digit) / radix ? 0x110000 : code * radix + digit;
                    end++;
                }
                if (end == first)
                {
                    scan = end;
                    continue;
                }
                if (end < value.Length && value[end] == ';') end++;
                if (result == null) result = new StringBuilder(value.Length);
                result.Append(value, literal, at - literal);
                if (code == 0 || code > 0x10ffff || code >= 0xd800 && code <= 0xdfff) code = 0xfffd;
                else if (code >= 0x80 && code <= 0x9f) code = C1[code - 0x80];
                result.Append(Char.ConvertFromUtf32(code));
                literal = scan = end;
            }
            if (result == null) return value;
            result.Append(value, literal, value.Length - literal);
            return result.ToString();
        }
    }
}
