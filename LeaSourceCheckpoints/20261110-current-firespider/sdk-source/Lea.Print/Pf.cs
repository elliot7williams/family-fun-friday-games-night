using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Lea.Print
{
    public sealed class Pf
    {
        private readonly Pw plan;
        public string Title { get; private set; }
        public string Address { get; private set; }
        public Pf(Pw plan, string title, string address)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            this.plan = plan;
            Title = Line(title);
            Address = DisplayAddress(address);
        }
        public ContainerVisual Dr(int page)
        {
            DrawingVisual body = plan.Dr(page);
            Pg geometry = plan.Geometry;
            var root = new ContainerVisual();
            var paper = new DrawingVisual();
            using (DrawingContext drawing = paper.RenderOpen())
                drawing.DrawRectangle(Brushes.White, null, new Rect(0, 0, geometry.PaperWidth * .96, geometry.PaperHeight * .96));
            root.Children.Add(paper); root.Children.Add(body);
            var bands = new DrawingVisual();
            using (DrawingContext drawing = bands.RenderOpen())
            {
                if (geometry.Header.Height > 0)
                {
                    Rect header = Rect(geometry.Header);
                    drawing.PushClip(new RectangleGeometry(header));
                    Text(drawing, Title, header.X, header.Y, header.Width, 12);
                    Text(drawing, Address, header.X, header.Y + 15, header.Width, 9);
                    drawing.Pop();
                }
                if (geometry.Footer.Height > 0)
                {
                    Rect footer = Rect(geometry.Footer);
                    drawing.PushClip(new RectangleGeometry(footer));
                    Text(drawing, "Firespider Internet - Page " + (page + 1).ToString(CultureInfo.InvariantCulture) +
                        " of " + plan.PageText.Count.ToString(CultureInfo.InvariantCulture), footer.X, footer.Y + 5, footer.Width, 10);
                    drawing.Pop();
                }
            }
            root.Children.Add(bands); return root;
        }
        private static void Text(DrawingContext drawing, string value, double x, double y, double width, double size)
        {
            if (value.Length == 0) return;
            var text = new FormattedText(value, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface("Arial"), size, Brushes.Black, 1.0)
            { MaxTextWidth = width, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis };
            drawing.DrawText(text, new Point(x, y));
        }
        private static Rect Rect(Pb box) { return new Rect(box.X * .96, box.Y * .96, box.Width * .96, box.Height * .96); }
        private static string Line(string text)
        {
            text = text ?? "";
            if (text.Length > 4096) throw new ArgumentException("Print metadata limit");
            var chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++) if (Char.IsControl(chars[i]) || chars[i] == '\u2028' || chars[i] == '\u2029') chars[i] = ' ';
            return new string(chars);
        }
        private static string DisplayAddress(string address)
        {
            if (String.IsNullOrEmpty(address)) return "";
            if (address.Length > 4096) throw new ArgumentException("Print address limit");
            Uri uri;
            if (!Uri.TryCreate(address, UriKind.Absolute, out uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https" && uri.Scheme != "file")) return "";
            var safe = new UriBuilder(uri) { UserName = "", Password = "", Query = "", Fragment = "" };
            return Line(safe.Uri.AbsoluteUri);
        }
    }
}
