using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace Lea.Win
{
    public sealed partial class V
    {
        private bool documentSelectionEventQueued;
        private Lea.Engine.P documentAnnouncedPage;
        private string documentAnnouncedText;
        private int documentAnnouncedStart, documentAnnouncedEnd;
        private void CheckDocumentRange(Ut text)
        {
            if (closed || !IsHandleCreated || Busy || Le != null || !Object.ReferenceEquals(text.P, At)) throw new ElementNotAvailableException("Document range is no longer ready/current");
            if (text.DocumentModel && DocumentSelectionSnapshot().X != text.X) throw new ElementNotAvailableException("Document range text/layout changed");
        }
        private void SelectDocumentRange(Ut text, int start, int end)
        {
            CheckDocumentRange(text);
            if (!text.DocumentModel) throw new InvalidOperationException("Selection requires a rendered document range");
            if (edit != null || heldEdit != null) throw new InvalidOperationException("Native editor selection is separate");
            Sel(start, end);
        }
        private ITextRangeProvider[] DocumentSelectionRanges()
        {
            Sds();
            if (documentSelection == null) return new ITextRangeProvider[0];
            var text = DocumentSelectionSnapshot();
            return new ITextRangeProvider[] { new Uq(this, text, Math.Min(documentAnchor, documentCaret), Math.Max(documentAnchor, documentCaret)) };
        }
        private ITextRangeProvider[] DocumentVisibleRanges(Ut text)
        {
            CheckDocumentRange(text); var ranges = new List<ITextRangeProvider>(); int start = -1, end = 0;
            foreach (var part in text.Ps)
            {
                if (AutomationTextRectangle(part).IsEmpty)
                {
                    if (start >= 0) { ranges.Add(new Uq(this, text, start, end)); start = -1; }
                    continue;
                }
                if (start < 0) start = part.S;
                end = part.E;
            }
            if (start >= 0) ranges.Add(new Uq(this, text, start, end));
            if (ranges.Count == 0) ranges.Add(new Uq(this, text, 0, 0));
            return ranges.ToArray();
        }
        private ITextRangeProvider DocumentRangeFromPoint(Ut text, System.Windows.Point screen)
        {
            CheckDocumentRange(text);
            if (Double.IsNaN(screen.X) || Double.IsInfinity(screen.X) || Double.IsNaN(screen.Y) || Double.IsInfinity(screen.Y)) throw new ArgumentException("Finite screen point required");
            var bounds = RectangleToScreen(ClientRectangle);
            var point = PointToClient(new Point((int)Math.Round(Math.Max(bounds.Left, Math.Min(bounds.Right, screen.X))), (int)Math.Round(Math.Max(bounds.Top, Math.Min(bounds.Bottom, screen.Y)))));
            int caret = Math.Max(0, DocumentSelectionPoint(text, point));
            return new Uq(this, text, caret, caret);
        }
        private double[] DocumentRangeRectangles(Ut text, int start, int end)
        {
            if (!text.DocumentModel) { CheckDocumentRange(text); return AutomationTextRectangles(text, start, end); }
            CheckDocumentRange(text); var current = DocumentSelectionSnapshot(); var result = new List<double>();
            using (var graphics = CreateGraphics())
                foreach (var part in current.Ps)
                {
                    var rect = MeasuredDocumentRectangle(graphics, part, Math.Max(start, part.S), Math.Min(end, part.E));
                    if (rect.IsEmpty) continue;
                    var point = PointToScreen(rect.Location); result.Add(point.X); result.Add(point.Y); result.Add(rect.Width); result.Add(rect.Height);
                }
            return result.ToArray();
        }
        private static int DocumentNextBoundary(string text, int start)
        {
            foreach (int next in System.Globalization.StringInfo.ParseCombiningCharacters(text)) if (next > start) return next;
            return text.Length;
        }
        private void QueueDocumentSelectionEvent()
        {
            if (closed || !IsHandleCreated || documentSelectionEventQueued || !AutomationInteropProvider.ClientsAreListening) return;
            documentSelectionEventQueued = true;
            try
            {
                BeginInvoke(new Action(delegate
                {
                    bool changed = false;
                    try
                    {
                        if (closed || !IsHandleCreated) return;
                        Sds(); var text = documentSelection;
                        int start = text == null ? 0 : Math.Min(documentAnchor, documentCaret), end = text == null ? 0 : Math.Max(documentAnchor, documentCaret);
                        changed = !Object.ReferenceEquals(documentAnnouncedPage, text == null ? null : text.P) || documentAnnouncedText != (text == null ? null : text.X) || documentAnnouncedStart != start || documentAnnouncedEnd != end;
                        documentAnnouncedPage = text == null ? null : text.P; documentAnnouncedText = text == null ? null : text.X;
                        documentAnnouncedStart = start; documentAnnouncedEnd = end;
                    }
                    finally { documentSelectionEventQueued = false; }
                    if (changed && !closed && IsHandleCreated)
                        try { AutomationInteropProvider.RaiseAutomationEvent(TextPatternIdentifiers.TextSelectionChangedEvent, UiRoot, new AutomationEventArgs(TextPatternIdentifiers.TextSelectionChangedEvent)); }
                        catch (InvalidOperationException) { }
                        catch (ElementNotAvailableException) { }
                }));
            }
            catch (InvalidOperationException) { documentSelectionEventQueued = false; }
        }
    }
}
