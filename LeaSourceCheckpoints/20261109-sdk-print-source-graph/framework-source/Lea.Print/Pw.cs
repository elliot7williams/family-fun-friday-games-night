using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lea.Print
{
    // Optional Windows vector pagination; no browser or printer dispatch.
    public sealed class Pw : IDisposable
    {
        public const int MaxText = 262144, MaxPages = 256;
        private FlowDocument document;
        private readonly List<DocumentPage> pages = new List<DocumentPage>();
        private readonly Pg geometry;
        public Pg Geometry { get { return geometry; } }
        public string Source { get; private set; }
        public string DocumentText { get; private set; }
        public bool Structured { get; private set; }
        public bool Styled { get; private set; }
        public int UnresolvedImages { get; private set; }
        public ReadOnlyCollection<string> PageText { get; private set; }
        private Pw(Pg geometry) { this.geometry = geometry; }

        public static Pw Cp(string text, Pg geometry, double points = 12)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (String.IsNullOrWhiteSpace(text) || text.Length > MaxText) throw new ArgumentException("Bounded nonempty text required");
            double size = Size(geometry, points);
            return Build(new FlowDocument(new Paragraph(new Run(text)) { Margin = new Thickness(0) }), text, geometry, size);
        }
        public static Pw Dc(Ps snapshot, Pg geometry, double points = 12)
        {
            double size = Size(geometry, points);
            FlowDocument document = Pm.Cp(snapshot, geometry.Grayscale);
            string source = new TextRange(document.ContentStart, document.ContentEnd).Text;
            if (String.IsNullOrWhiteSpace(source) || source.Length > MaxText) throw new ArgumentException("Bounded nonempty document required");
            Pw result = Build(document, source, geometry, size);
            result.Structured = true;
            result.Styled = snapshot.Styled;
            foreach (Pn node in snapshot.Nodes) if (node.Kind == "img") result.UnresolvedImages++;
            return result;
        }
        public static Pw Di(Ps snapshot, Pg geometry, IReadOnlyDictionary<int, Pi> images, double points = 12)
        {
            double size = Size(geometry, points);
            FlowDocument document = Pm.Ci(snapshot, geometry, images);
            string source = new TextRange(document.ContentStart, document.ContentEnd).Text;
            if (source.Length > MaxText || String.IsNullOrWhiteSpace(source) && images.Count == 0) throw new ArgumentException("Bounded nonempty image document required");
            Pw result = Build(document, source, geometry, size);
            result.Structured = true; result.Styled = snapshot.Styled;
            return result;
        }
        private static double Size(Pg geometry, double points)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (geometry == null) throw new ArgumentNullException("geometry");
            if (Double.IsNaN(points) || Double.IsInfinity(points) || points < 6 || points > 72) throw new ArgumentOutOfRangeException("points");
            double size = points * 96 / 72;
            if (geometry.LayoutWidth < size * 2 || geometry.LayoutHeight < size * 2 ||
                geometry.LayoutWidth > 100000 || geometry.LayoutHeight > 100000) throw new ArgumentException("Unsupported text page size");
            return size;
        }
        private static Pw Build(FlowDocument document, string source, Pg geometry, double size)
        {
            var result = new Pw(geometry);
            try
            {
                result.Source = source; result.document = document;
                document.FontFamily = new FontFamily("Arial"); document.FontSize = size; document.Foreground = Brushes.Black;
                document.PagePadding = new Thickness(0); document.ColumnGap = 0; document.ColumnWidth = geometry.LayoutWidth;
                document.IsColumnWidthFlexible = false; document.PageWidth = geometry.LayoutWidth; document.PageHeight = geometry.LayoutHeight;
                var paginator = (DynamicDocumentPaginator)((IDocumentPaginatorSource)result.document).DocumentPaginator;
                paginator.IsBackgroundPaginationEnabled = false;
                paginator.PageSize = new Size(geometry.LayoutWidth, geometry.LayoutHeight);
                for (int index = 0; ; index++)
                {
                    DocumentPage page = paginator.GetPage(index);
                    if (page == DocumentPage.Missing) break;
                    if (index == MaxPages) { page.Dispose(); throw new InvalidOperationException("Text print page limit"); }
                    result.pages.Add(page);
                }
                if (result.pages.Count == 0) throw new InvalidOperationException("No text pages");
                var texts = new List<string>();
                for (int index = 0; index < result.pages.Count; index++)
                {
                    TextPointer start = index == 0 ? result.document.ContentStart : (TextPointer)paginator.GetPagePosition(result.pages[index]);
                    TextPointer end = index + 1 == result.pages.Count ? result.document.ContentEnd : (TextPointer)paginator.GetPagePosition(result.pages[index + 1]);
                    if (start == null || end == null || start.CompareTo(end) >= 0) throw new InvalidOperationException("Invalid text page range");
                    texts.Add(new TextRange(start, end).Text);
                }
                result.DocumentText = new TextRange(result.document.ContentStart, result.document.ContentEnd).Text;
                if (String.Concat(texts) != result.DocumentText) throw new InvalidOperationException("Text page range loss");
                result.PageText = texts.AsReadOnly();
                return result;
            }
            catch { result.Dispose(); throw; }
        }

        public DrawingVisual Dr(int page)
        {
            Verify();
            if (page < 0 || page >= pages.Count) throw new ArgumentOutOfRangeException("page");
            var output = new DrawingVisual();
            double scale = geometry.UnitsPerPixel * 96 / 100;
            using (DrawingContext drawing = output.RenderOpen())
            {
                drawing.PushClip(new RectangleGeometry(new Rect(geometry.Body.X * .96, geometry.Body.Y * .96,
                    geometry.Body.Width * .96, geometry.Body.Height * .96)));
                drawing.PushTransform(new MatrixTransform(scale, 0, 0, scale, geometry.Body.X * .96, geometry.Body.Y * .96));
                var brush = new VisualBrush(pages[page].Visual)
                {
                    Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
                    ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(0, 0, geometry.LayoutWidth, geometry.LayoutHeight)
                };
                drawing.DrawRectangle(brush, null, new Rect(0, 0, geometry.LayoutWidth, geometry.LayoutHeight));
                drawing.Pop(); drawing.Pop();
            }
            return output;
        }
        internal void Verify()
        {
            if (document == null) throw new ObjectDisposedException("Pw");
            document.Dispatcher.VerifyAccess();
        }
        public void Dispose()
        {
            if (document == null) return;
            document.Dispatcher.VerifyAccess();
            foreach (DocumentPage page in pages) page.Dispose();
            pages.Clear(); document.Blocks.Clear(); document = null;
        }
    }
}
