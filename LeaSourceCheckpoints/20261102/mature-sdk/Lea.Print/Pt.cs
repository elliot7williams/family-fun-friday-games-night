using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace Lea.Print
{
    // Native selected-text body renderer. Full document styling/media is a separate pipeline.
    public sealed class Pt : IDisposable
    {
        public const int MaxText = 262144;
        public const int MaxPages = 256;
        private readonly object gate = new object();
        private Font font;
        private readonly Pg geometry;
        public ReadOnlyCollection<string> Pages { get; private set; }
        private Pt(Pg geometry, Font font, List<string> pages)
        { this.geometry = geometry; this.font = font; Pages = pages.AsReadOnly(); }

        public static Pt Cp(string text, Pg geometry, string family = "Arial", float points = 12)
        {
            if (String.IsNullOrWhiteSpace(text) || text.Length > MaxText) throw new ArgumentException("Bounded nonempty text required", "text");
            if (geometry == null) throw new ArgumentNullException("geometry");
            if (Single.IsNaN(points) || Single.IsInfinity(points) || points < 6 || points > 72) throw new ArgumentOutOfRangeException("points");
            if (geometry.LayoutWidth > 100000 || geometry.LayoutHeight > 100000) throw new ArgumentException("Text page too large");
            Font font = new Font(family, points * 96 / 72, FontStyle.Regular, GraphicsUnit.Pixel);
            try
            {
                var pages = new List<string>();
                int[] boundaries = StringInfo.ParseCombiningCharacters(text);
                using (var bitmap = new Bitmap(1, 1))
                {
                    bitmap.SetResolution(96, 96);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    using (StringFormat format = Format())
                    {
                        graphics.PageUnit = GraphicsUnit.Pixel;
                        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        int start = 0;
                        while (start < text.Length)
                        {
                            if (pages.Count == MaxPages) throw new InvalidOperationException("Text print page limit");
                            int fitted, lines;
                            graphics.MeasureString(text.Substring(start), font,
                                new SizeF((float)geometry.LayoutWidth, (float)geometry.LayoutHeight), format, out fitted, out lines);
                            int end = start + fitted;
                            if (end < text.Length)
                            {
                                int index = Array.BinarySearch(boundaries, end);
                                if (index < 0) end = boundaries[Math.Max(0, ~index - 1)];
                                if (end > start && text[end - 1] == '\r' && text[end] == '\n') end--;
                            }
                            if (end <= start) throw new InvalidOperationException("Text element does not fit printable body");
                            pages.Add(text.Substring(start, end - start)); start = end;
                        }
                    }
                }
                return new Pt(geometry, font, pages);
            }
            catch { font.Dispose(); throw; }
        }

        public void Dr(Graphics graphics, int page)
        {
            if (graphics == null) throw new ArgumentNullException("graphics");
            if (page < 0 || page >= Pages.Count) throw new ArgumentOutOfRangeException("page");
            lock (gate)
            {
                if (font == null) throw new ObjectDisposedException("Pt");
                GraphicsState state = graphics.Save();
                try
                {
                    graphics.ResetTransform(); graphics.PageUnit = GraphicsUnit.Pixel;
                    graphics.TranslateTransform((float)(geometry.Body.X * graphics.DpiX / 100), (float)(geometry.Body.Y * graphics.DpiY / 100));
                    graphics.ScaleTransform((float)(geometry.UnitsPerPixel * graphics.DpiX / 100), (float)(geometry.UnitsPerPixel * graphics.DpiY / 100));
                    var body = new RectangleF(0, 0, (float)geometry.LayoutWidth, (float)geometry.LayoutHeight);
                    graphics.SetClip(body, CombineMode.Intersect);
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    using (StringFormat format = Format()) graphics.DrawString(Pages[page], font, Brushes.Black, body, format);
                }
                finally { graphics.Restore(state); }
            }
        }

        private static StringFormat Format()
        {
            var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.LineLimit | StringFormatFlags.MeasureTrailingSpaces;
            return format;
        }
        public void Dispose() { lock (gate) { if (font != null) { font.Dispose(); font = null; } } }
    }
}
