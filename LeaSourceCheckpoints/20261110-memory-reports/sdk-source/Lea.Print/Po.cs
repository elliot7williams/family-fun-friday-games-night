using System;

namespace Lea.Print
{
    public sealed class Po
    {
        public bool Landscape { get; private set; }
        public double MarginInches { get; private set; }
        public int ScalePercent { get; private set; }
        public bool Grayscale { get; private set; }
        public bool Header { get; private set; }
        public bool Footer { get; private set; }
        public bool SelectionOnly { get; private set; }

        public Po(bool landscape, double marginInches, int scalePercent,
            bool grayscale, bool header, bool footer, bool selectionOnly)
        {
            if (Double.IsNaN(marginInches) || Double.IsInfinity(marginInches) || marginInches < 0 || marginInches > 5)
                throw new ArgumentOutOfRangeException("marginInches");
            if (scalePercent < 25 || scalePercent > 400) throw new ArgumentOutOfRangeException("scalePercent");
            Landscape = landscape; MarginInches = marginInches; ScalePercent = scalePercent;
            Grayscale = grayscale; Header = header; Footer = footer; SelectionOnly = selectionOnly;
        }
    }

    public struct Pb
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public Pb(double x, double y, double width, double height) : this()
        { X = x; Y = y; Width = width; Height = height; }
    }

    public sealed class Pg
    {
        public double PaperWidth { get; private set; }
        public double PaperHeight { get; private set; }
        public bool Grayscale { get; private set; }
        public Pb Body { get; private set; }
        public Pb Header { get; private set; }
        public Pb Footer { get; private set; }
        public double UnitsPerPixel { get; private set; }
        public double LayoutWidth { get { return Body.Width / UnitsPerPixel; } }
        public double LayoutHeight { get { return Body.Height / UnitsPerPixel; } }
        private Pg() { }

        // All device rectangles are in hundredths of an inch, already oriented by the printer.
        public static Pg Cp(Po options, double paperWidth, double paperHeight, Pb printable)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (!Finite(paperWidth) || !Finite(paperHeight) || paperWidth <= 0 || paperHeight <= 0 ||
                !Finite(printable.X) || !Finite(printable.Y) || !Finite(printable.Width) || !Finite(printable.Height) ||
                printable.X < 0 || printable.Y < 0 || printable.Width <= 0 || printable.Height <= 0 ||
                !Finite(printable.X + printable.Width) || !Finite(printable.Y + printable.Height) ||
                printable.X + printable.Width > paperWidth + 0.01 || printable.Y + printable.Height > paperHeight + 0.01)
                throw new ArgumentException("Invalid oriented printable area");
            double margin = options.MarginInches * 100;
            double left = Math.Max(margin, printable.X), top = Math.Max(margin, printable.Y);
            double right = Math.Min(paperWidth - margin, printable.X + printable.Width);
            double bottom = Math.Min(paperHeight - margin, printable.Y + printable.Height);
            double header = options.Header ? 30 : 0, footer = options.Footer ? 30 : 0;
            if (right <= left || bottom - top <= header + footer)
                throw new InvalidOperationException("Margins and header/footer leave no printable content");
            double factor = options.ScalePercent / 96.0;
            var result = new Pg
            {
                PaperWidth = paperWidth, PaperHeight = paperHeight,
                Grayscale = options.Grayscale,
                Header = new Pb(left, top, right - left, header),
                Footer = new Pb(left, bottom - footer, right - left, footer),
                Body = new Pb(left, top + header, right - left, bottom - top - header - footer),
                UnitsPerPixel = factor
            };
            if (!Finite(result.LayoutWidth) || !Finite(result.LayoutHeight)) throw new ArgumentException("Layout overflow");
            return result;
        }
        private static bool Finite(double value) { return !Double.IsNaN(value) && !Double.IsInfinity(value); }
    }
}
