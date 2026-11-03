using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lea.Print
{
    internal static class Pz
    {
        internal static bool Pre(Py style, bool fallback)
        { return style == null || style.WhiteSpace.Length == 0 ? fallback : style.WhiteSpace == "p" || style.WhiteSpace == "pw"; }
        internal static void Ap(TextElement element, Py style, bool grayscale)
        {
            if (style == null) return;
            if (style.FontSize > 0) element.FontSize = style.FontSize;
            element.FontWeight = style.Bold ? FontWeights.Bold : FontWeights.Normal;
            element.FontStyle = style.Italic ? FontStyles.Italic : FontStyles.Normal;
            string family = Font(style.FontFamily); if (family.Length > 0) element.FontFamily = new FontFamily(family);
            element.SetValue(FrameworkElement.FlowDirectionProperty, style.Direction == "r" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            element.Foreground = Brush(style.Foreground, grayscale, style.Opacity) ?? Brush("black", grayscale, style.Opacity);
            element.Background = style.Visibility == "h" || style.Visibility == "c" ? null : Brush(style.Background, grayscale, style.Opacity);
            var inline = element as Inline;
            if (inline != null)
            {
                var decorations = new TextDecorationCollection();
                if (style.Decorations.Contains("u")) foreach (var item in TextDecorations.Underline) decorations.Add(item);
                if (style.Decorations.Contains("s")) foreach (var item in TextDecorations.Strikethrough) decorations.Add(item);
                if (style.Decorations.Contains("o")) foreach (var item in TextDecorations.OverLine) decorations.Add(item);
                inline.TextDecorations = decorations;
            }
            var block = element as Block;
            if (block != null)
                block.TextAlignment = style.Alignment == "c" ? TextAlignment.Center : style.Alignment == "j" ? TextAlignment.Justify :
                    style.Alignment == "r" || style.Alignment == "e" && style.Direction != "r" || style.Alignment == "s" && style.Direction == "r" ? TextAlignment.Right : TextAlignment.Left;
        }
        private static string Font(string source)
        {
            if (source.Length > 512) return "";
            var families = new List<string>();
            foreach (string part in source.Split('|'))
            {
                string name = part.Trim().Trim('\'', '"');
                if (name.Length == 0 || name.Length > 80) continue;
                bool safe = true;
                foreach (char c in name) if (!Char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '_') { safe = false; break; }
                if (!safe) continue;
                switch (name.ToLowerInvariant())
                {
                    case "s": name = "Times New Roman"; break;
                    case "ss": name = "Arial"; break;
                    case "m": name = "Consolas"; break;
                    case "c": name = "Comic Sans MS"; break;
                    case "f": name = "Impact"; break;
                    case "ui": name = "Segoe UI"; break;
                }
                families.Add(name); if (families.Count == 4) break;
            }
            return String.Join(",", families);
        }
        private static Brush Brush(string source, bool grayscale, int opacity)
        {
            Color color;
            if (!Color(source, out color)) return null;
            if (grayscale)
            {
                byte gray = (byte)((299 * color.R + 587 * color.G + 114 * color.B + 500) / 1000);
                color.R = color.G = color.B = gray;
            }
            color.A = (byte)Math.Round(color.A * opacity / 100.0, MidpointRounding.AwayFromZero);
            var brush = new SolidColorBrush(color); brush.Freeze(); return brush;
        }
        private static bool Color(string source, out Color color)
        {
            color = Colors.Transparent;
            string text = source.Trim();
            if (text.Length == 0 || text.Length > 64) return false;
            if (text[0] == '#')
            {
                uint value; int count = text.Length - 1;
                if ((count != 3 && count != 4 && count != 6 && count != 8) || !UInt32.TryParse(text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) return false;
                if (count == 3) color = System.Windows.Media.Color.FromRgb((byte)(((value >> 8) & 15) * 17), (byte)(((value >> 4) & 15) * 17), (byte)((value & 15) * 17));
                if (count == 4) color = System.Windows.Media.Color.FromArgb((byte)((value & 15) * 17), (byte)(((value >> 12) & 15) * 17), (byte)(((value >> 8) & 15) * 17), (byte)(((value >> 4) & 15) * 17));
                if (count == 6) color = System.Windows.Media.Color.FromRgb((byte)(value >> 16), (byte)(value >> 8), (byte)value);
                if (count == 8) color = System.Windows.Media.Color.FromArgb((byte)value, (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8));
                return true;
            }
            int open = text.IndexOf('(');
            if (open > 0 && text.EndsWith(")", StringComparison.Ordinal))
            {
                string function = text.Substring(0, open).ToLowerInvariant();
                string[] values = text.Substring(open + 1, text.Length - open - 2).Split(',');
                if (function != "rgb" && function != "rgba" || values.Length != (function == "rgb" ? 3 : 4)) return false;
                byte r, g, b; double alpha = 1;
                if (!Byte.TryParse(values[0].Trim(), out r) || !Byte.TryParse(values[1].Trim(), out g) || !Byte.TryParse(values[2].Trim(), out b) ||
                    values.Length == 4 && (!Double.TryParse(values[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out alpha) || Double.IsNaN(alpha) || Double.IsInfinity(alpha) || alpha < 0 || alpha > 1)) return false;
                color = System.Windows.Media.Color.FromArgb((byte)Math.Round(alpha * 255, MidpointRounding.AwayFromZero), r, g, b); return true;
            }
            foreach (char c in text) if (c < 'A' || c > 'Z' && c < 'a' || c > 'z') return false;
            try { color = (Color)ColorConverter.ConvertFromString(text); return true; }
            catch (FormatException) { return false; }
            catch (ArgumentException) { return false; }
        }
    }
}
