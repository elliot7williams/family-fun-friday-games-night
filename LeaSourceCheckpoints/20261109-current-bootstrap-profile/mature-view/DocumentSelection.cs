using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Lea.Win
{
    public sealed partial class V
    {
        private Ut documentSelection;
        private int documentAnchor, documentCaret;
        private bool documentDragging, documentSuppressClick;
        private Point documentDragOrigin;

        public string Dtx() { return DocumentSelectionSnapshot().X; }
        public int Sdc()
        {
            return CopyDocumentSelection(value =>
            {
                var data = new DataObject(); data.SetText(value, TextDataFormat.UnicodeText);
                Clipboard.SetDataObject(data, true, 10, 20);
            });
        }
        private int CopyDocumentSelection(Action<string> write)
        {
            if (write == null) throw new ArgumentNullException("write");
            DocumentSelectionThread();
            string value = Sds();
            if (value.Length == 0) return 0;
            var selection = documentSelection; int anchor = documentAnchor, caret = documentCaret;
            try { write(value); }
            catch (System.Runtime.InteropServices.ExternalException) { return 0; }
            catch (System.Threading.ThreadStateException) { return 0; }
            if (closed || IsDisposed || Busy || Le != null || !Object.ReferenceEquals(selection, documentSelection) ||
                !Object.ReferenceEquals(selection.P, At) || documentAnchor != anchor || documentCaret != caret || Sds() != value) return 0;
            return 1;
        }
        private bool DocumentCopyAvailable(string code)
        {
            if (!String.Equals(code, "cp", StringComparison.OrdinalIgnoreCase) || closed || IsDisposed || !IsHandleCreated || InvokeRequired ||
                Busy || Le != null || At == null || edit != null || heldEdit != null || documentSelection == null) return false;
            return Sds().Length != 0;
        }
        public void Sel(int anchor, int caret)
        {
            var text = DocumentSelectionSnapshot();
            if (anchor < 0 || caret < 0 || anchor > text.X.Length || caret > text.X.Length) throw new ArgumentOutOfRangeException("selection");
            documentAnchor = DocumentBoundary(text.X, anchor); documentCaret = DocumentBoundary(text.X, caret);
            documentSelection = text;
            DocumentSelectionChanged();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            documentSuppressClick = false;
            if (e.Button != MouseButtons.Left || Busy || Le != null || At == null || edit != null) return;
            var box = BoxAt(e.Location);
            if (box != null && box.B != null && box.B.I != null) { documentSelection = null; DocumentSelectionChanged(); return; }
            var text = DocumentSelectionSnapshot(); int point = DocumentSelectionPoint(text, e.Location);
            if (point < 0) return;
            Focus();
            if (!Object.ReferenceEquals(text.P, At) || Busy) return;
            documentSelection = text; documentAnchor = documentCaret = point;
            documentDragging = true; documentDragOrigin = e.Location; Capture = true; DocumentSelectionChanged();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left || !documentDragging) return;
            MoveDocumentSelection(e);
            documentSuppressClick = documentSelection != null && documentCaret != documentAnchor &&
                (Math.Abs(e.X - documentDragOrigin.X) > 2 || Math.Abs(e.Y - documentDragOrigin.Y) > 2);
            documentDragging = false; Capture = false;
        }
        private bool MoveDocumentSelection(MouseEventArgs e)
        {
            if (!documentDragging) return false;
            if (Busy || Le != null || documentSelection == null || !Object.ReferenceEquals(documentSelection.P, At))
            { documentSelection = null; documentDragging = false; Capture = false; DocumentSelectionChanged(); return false; }
            var text = DocumentSelectionSnapshot();
            if (text.X != documentSelection.X) { documentSelection = null; documentDragging = false; Capture = false; DocumentSelectionChanged(); return false; }
            int point = DocumentSelectionPoint(text, e.Location);
            if (point >= 0) documentCaret = point;
            documentSuppressClick = documentCaret != documentAnchor && (Math.Abs(e.X - documentDragOrigin.X) > 2 || Math.Abs(e.Y - documentDragOrigin.Y) > 2);
            documentSelection = text; DocumentSelectionChanged(); return true;
        }
        protected override void OnMouseCaptureChanged(EventArgs e)
        { base.OnMouseCaptureChanged(e); if (!Capture) documentDragging = false; }
        private bool ConsumeDocumentSelectionClick(MouseEventArgs e)
        {
            bool consume = e.Button == MouseButtons.Left && documentSuppressClick;
            documentSuppressClick = false; return consume;
        }
        private bool DocumentSelectionKey(KeyEventArgs e)
        {
            if (closed || !IsHandleCreated || Busy || Le != null || At == null || edit != null || heldEdit != null || e.Alt) return false;
            if (e.Control && !e.Shift && e.KeyCode == Keys.A) { var text = Dtx(); Sel(0, text.Length); }
            else if (e.Control && !e.Shift && e.KeyCode == Keys.C) { if (Sdc() == 0) return false; }
            else if (!e.Control && e.KeyCode == Keys.Escape && documentSelection != null) { documentSelection = null; DocumentSelectionChanged(); }
            else if (DocumentSelectionArrow(e)) { }
            else return false;
            e.Handled = e.SuppressKeyPress = true; return true;
        }
        private bool DocumentSelectionArrow(KeyEventArgs e)
        {
            bool left = e.KeyCode == Keys.Left, right = e.KeyCode == Keys.Right;
            bool home = e.KeyCode == Keys.Home, end = e.KeyCode == Keys.End;
            if ((!left && !right && !home && !end) || (e.Control && (left || right))) return false;
            Sds();
            if (documentSelection == null) return false;
            string text = documentSelection.X;
            int next = documentCaret;
            if (home) next = e.Control ? 0 : (next == 0 ? 0 : text.LastIndexOf('\n', next - 1) + 1);
            else if (end) { next = e.Control ? text.Length : text.IndexOf('\n', next); if (next < 0) next = text.Length; }
            else if (!e.Shift && documentAnchor != documentCaret) next = left ? Math.Min(documentAnchor, documentCaret) : Math.Max(documentAnchor, documentCaret);
            else if (left) next = DocumentBoundary(text, Math.Max(0, next - 1));
            else
            {
                next = text.Length;
                foreach (int boundary in StringInfo.ParseCombiningCharacters(text)) if (boundary > documentCaret) { next = boundary; break; }
            }
            Sel(e.Shift ? documentAnchor : next, next);
            return true;
        }
        private void PaintDocumentSelection(Graphics graphics)
        {
            if (documentSelection == null || Busy || At == null) return;
            if (Sds().Length == 0) return;
            var text = DocumentSelectionSnapshot(); int start = Math.Min(documentAnchor, documentCaret), end = Math.Max(documentAnchor, documentCaret);
            using (var brush = new SolidBrush(Color.FromArgb(72, 0, 120, 215)))
                foreach (var part in text.Ps)
                {
                    var rect = MeasuredDocumentRectangle(graphics, part, Math.Max(start, part.S), Math.Min(end, part.E));
                    if (!rect.IsEmpty) graphics.FillRectangle(brush, rect);
                }
        }
        private int DocumentSelectionPoint(Ut text, Point point)
        {
            Us best = null; long distance = Int64.MaxValue;
            foreach (var part in text.Ps)
            {
                var rect = AutomationTextRectangle(part); if (rect.IsEmpty) continue;
                long dx = point.X < rect.Left ? (long)rect.Left - point.X : point.X > rect.Right ? (long)point.X - rect.Right : 0;
                long dy = point.Y < rect.Top ? (long)rect.Top - point.Y : point.Y > rect.Bottom ? (long)point.Y - rect.Bottom : 0;
                long score = dx * dx + dy * dy; if (score >= distance) continue; best = part; distance = score;
            }
            if (best == null) return -1;
            var bounds = AutomationTextLayoutRectangle(best);
            double target = best.Rev ? bounds.Right - (double)point.X : point.X - (double)bounds.Left;
            int[] elements = StringInfo.ParseCombiningCharacters(best.X);
            var boundaries = new int[elements.Length + 1]; Array.Copy(elements, boundaries, elements.Length); boundaries[elements.Length] = best.X.Length;
            Lea.Engine.S style;
            using (var graphics = CreateGraphics())
            using (var font = DocumentSelectionFont(best, out style))
            {
                int low = 0, high = boundaries.Length - 1;
                while (low < high)
                {
                    int mid = (low + high) / 2;
                    if (DocumentAdvance(graphics, best.X, boundaries[mid], font, style) < target) low = mid + 1; else high = mid;
                }
                int chosen = low;
                if (low > 0 && Math.Abs(DocumentAdvance(graphics, best.X, boundaries[low - 1], font, style) - target) <=
                    Math.Abs(DocumentAdvance(graphics, best.X, boundaries[low], font, style) - target)) chosen--;
                return DocumentBoundary(text.X, best.S + boundaries[chosen]);
            }
        }
        private Font DocumentSelectionFont(Us part, out Lea.Engine.S style)
        {
            foreach (var box in BoxSnapshot())
                if (Object.ReferenceEquals(box.El, part.El) && !box.Title)
                {
                    style = box.S;
                    return BoxFont(box.K, box.S, false, box.U != null, box.Inline && box.B != null ? box.B.K : null);
                }
            style = null; return (Font)Font.Clone();
        }
        private static int DocumentAdvance(Graphics graphics, string text, int end, Font font, Lea.Engine.S style)
        { return end == 0 ? 0 : TextWidth(graphics, text.Substring(0, end), font, style); }
        private Rectangle MeasuredDocumentRectangle(Graphics graphics, Us part, int start, int end)
        {
            if (end <= start) return Rectangle.Empty;
            var bounds = AutomationTextLayoutRectangle(part); Lea.Engine.S style;
            using (var font = DocumentSelectionFont(part, out style))
            {
                int from = DocumentAdvance(graphics, part.X, Math.Max(0, start - part.S), font, style);
                int to = DocumentAdvance(graphics, part.X, Math.Min(part.X.Length, end - part.S), font, style);
                int left = part.Rev ? bounds.Right - to : bounds.Left + from;
                return Rectangle.Intersect(new Rectangle(left, bounds.Top, Math.Max(1, to - from), bounds.Height), AutomationTextRectangle(part));
            }
        }
        public string Sds()
        {
            DocumentSelectionThread();
            if (documentSelection == null) return "";
            if (Busy || Le != null || !Object.ReferenceEquals(documentSelection.P, At) || edit != null || heldEdit != null)
            { documentSelection = null; DocumentSelectionChanged(); return ""; }
            var current = DocumentSelectionSnapshot();
            if (current.X != documentSelection.X) { documentSelection = null; DocumentSelectionChanged(); return ""; }
            int start = Math.Min(documentAnchor, documentCaret), end = Math.Max(documentAnchor, documentCaret);
            return current.X.Substring(start, end - start);
        }
        private void DocumentSelectionThread()
        {
            if (closed || IsDisposed || Disposing) throw new ObjectDisposedException("V");
            if (!IsHandleCreated || InvokeRequired) throw new InvalidOperationException("Document selection requires the view UI thread and an existing handle");
        }
        private void DocumentSelectionChanged()
        {
            Invalidate();
#if DOCUMENT_UIA
            QueueDocumentSelectionEvent();
#endif
        }
        private Ut DocumentSelectionSnapshot()
        {
            DocumentSelectionThread();
            if (Busy || Le != null || At == null || At.D == null) throw new InvalidOperationException("Page is not ready for document selection");
            var page = At; var original = AutomationText(); var value = new StringBuilder(); var parts = new List<Us>(); Us previous = null;
            foreach (var part in original.Ps)
            {
                string separator = "";
                if (previous != null)
                {
                    if (!AutomationSameTextLine(previous, part)) separator = "\n";
                    else if (part.Rev ? previous.R.Left > part.R.Right : part.R.Left > previous.R.Right) separator = " ";
                }
                if (separator.Length + part.X.Length > MaxAutomationTextChars - value.Length) break;
                value.Append(separator);
                var mapped = new Us(part.X, part.R, part.Cp, part.Fx, part.El, part.O, part.Q, part.Rev) { S = value.Length };
                value.Append(part.X); mapped.E = value.Length; parts.Add(mapped); previous = part;
            }
            if (!Object.ReferenceEquals(page, At) || Busy || Le != null) throw new InvalidOperationException("Page changed during selection snapshot");
            string text = value.ToString();
            var snapshot = new Ut(page, text, parts, AutomationTextLineBoundaries(text, parts));
#if DOCUMENT_UIA
            snapshot.DocumentModel = true;
#endif
            return snapshot;
        }
        private static int DocumentBoundary(string text, int position)
        {
            if (position == text.Length) return position;
            int boundary = 0;
            foreach (int start in StringInfo.ParseCombiningCharacters(text)) { if (start > position) break; boundary = start; }
            return boundary;
        }
    }
}
