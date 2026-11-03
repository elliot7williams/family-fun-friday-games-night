using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lea.Engine
{
    public sealed partial class E
    {
        // Parse into a temporary style so an invalid component cannot reset earlier values.
        private void FontShorthand(S style, S inherited, string value)
        {
            string source = (value ?? "").Trim();
            if (source.Length == 0 || source.Length > 512) return;
            if (source.Equals("initial", StringComparison.OrdinalIgnoreCase))
            {
                CopyShorthandFont(style, new S { Fs = 16 }); return;
            }
            if (source.Equals("inherit", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase))
            {
                CopyShorthandFont(style, inherited ?? new S()); return;
            }
            var parsed = new S();
            int index = 0, normals = 0;
            bool weight = false, slant = false, size = false;
            while (index < source.Length)
            {
                SkipFontSpace(source, ref index);
                int start = index;
                while (index < source.Length && !Char.IsWhiteSpace(source[index]) && source[index] != '/') index++;
                string token = source.Substring(start, index - start).ToLowerInvariant();
                if (token.Length == 0) return;
                if (Regex.IsMatch(token, "^[0-9]{1,3}px$"))
                {
                    int pixels;
                    if (!Int32.TryParse(token.Substring(0, token.Length - 2), out pixels) || pixels < 1 || pixels >= 200) return;
                    parsed.Fs = pixels; size = true; break;
                }
                int relativePixels;
                if (TryRelativeFontSize(inherited, token, out relativePixels))
                { parsed.Fs = relativePixels; size = true; break; }
                if (token == "normal") { if (++normals > 4) return; }
                else if (token == "italic" || token == "oblique")
                { if (slant) return; slant = true; parsed.I = true; }
                else if (token == "bold" || Regex.IsMatch(token, "^[1-9]00$"))
                { if (weight) return; weight = true; parsed.B = token == "bold" || String.CompareOrdinal(token, "700") >= 0; }
                else return;
                if (normals + (weight ? 1 : 0) + (slant ? 1 : 0) > 4) return;
            }
            if (!size) return;
            // A family must be separated from the size/line-height by whitespace.
            bool separated = index < source.Length && Char.IsWhiteSpace(source[index]);
            SkipFontSpace(source, ref index);
            if (index < source.Length && source[index] == '/')
            {
                index++; SkipFontSpace(source, ref index);
                int start = index;
                while (index < source.Length && !Char.IsWhiteSpace(source[index])) index++;
                string line = source.Substring(start, index - start);
                parsed.Lh = -1; parsed.Lm = -1;
                LineHeight(parsed, line);
                if (parsed.Lh < 0 || parsed.Lm < 0) return;
                separated = index < source.Length && Char.IsWhiteSpace(source[index]);
                SkipFontSpace(source, ref index);
            }
            if (!separated || index >= source.Length) return;
            string family = source.Substring(index);
            string[] names = family.Split(',');
            if (names.Length > 8) return;
            foreach (string item in names)
            {
                string name = item.Trim();
                if (name.Length >= 2 && ((name[0] == '"' && name[name.Length - 1] == '"') || (name[0] == '\'' && name[name.Length - 1] == '\'')))
                {
                    name = name.Substring(1, name.Length - 2);
                    if (!Regex.IsMatch(name, "^[a-zA-Z0-9 _-]{1,64}$") || name.Trim().Length == 0) return;
                }
                else
                {
                    if (!Regex.IsMatch(name, "^-?[_a-zA-Z][a-zA-Z0-9_-]*(?:\\s+-?[_a-zA-Z][a-zA-Z0-9_-]*)*$")) return;
                    string keyword = name.ToLowerInvariant();
                    if (keyword == "inherit" || keyword == "initial" || keyword == "unset" || keyword == "revert" || keyword == "revert-layer" || keyword == "default") return;
                }
            }
            FontFamily(parsed, family);
            if (parsed.Ff.Length == 0) return;
            CopyShorthandFont(style, parsed);
        }

        private static void SkipFontSpace(string source, ref int index)
        { while (index < source.Length && Char.IsWhiteSpace(source[index])) index++; }

        private static bool TryRelativeFontSize(S inherited, string value, out int pixels)
        {
            pixels = 0;
            var match = Regex.Match((value ?? "").Trim(), "^(?<n>(?:[0-9]{1,4}(?:\\.[0-9]{1,4})?|\\.[0-9]{1,4}))(?<u>em|%)$", RegexOptions.IgnoreCase);
            decimal factor;
            if (!match.Success || !Decimal.TryParse(match.Groups["n"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out factor)) return false;
            int basis = inherited != null && inherited.Fs > 0 ? inherited.Fs : 16;
            decimal computed = basis * factor / (match.Groups["u"].Value == "%" ? 100M : 1M);
            if (computed <= 0 || computed >= 200) return false;
            int rounded = (int)Decimal.Round(computed, 0, MidpointRounding.AwayFromZero);
            if (rounded < 1 || rounded >= 200) return false;
            pixels = rounded; return true;
        }

        private static void CopyShorthandFont(S target, S value)
        { target.Fs = value.Fs; target.Ff = value.Ff; target.B = value.B; target.I = value.I; target.Lh = value.Lh; target.Lm = value.Lm; }
    }
}
