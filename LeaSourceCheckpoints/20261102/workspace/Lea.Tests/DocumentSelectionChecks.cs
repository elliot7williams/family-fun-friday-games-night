using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class DocumentSelectionChecks
{
    internal static int Run(string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        Action<Action, string> reject = (action, name) => { bool denied = false; try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; } check(denied, name); };
        using (var uncreated = new V()) reject(() => uncreated.Dtx(), "selection uncreated view");
        using (var form = new Forms.Form { ClientSize = new System.Drawing.Size(640, 480) })
        using (var view = new V { Dock = Forms.DockStyle.Fill })
        {
            form.Controls.Add(view); form.Show(); reject(() => view.Dtx(), "selection unloaded view");
            view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); Forms.Application.DoEvents();
            string text = view.Dtx();
            check(text.Contains("Alpha bravo charlie") && text.Contains("SECOND_LINE"), "rendered selection text model");
            check(!text.Contains("SELECTION_SECRET"), "hidden and native fields excluded from document model");
            check(view.Sds() == "", "no initial selection");
#if DOCUMENT_GESTURES
            count += DocumentSelectionGestures.Run(view, folder);
#endif
#if DOCUMENT_COPY
            count += DocumentCopyChecks.Run(view, folder);
#endif
#if DOCUMENT_UIA
            count += DocumentSelectionAutomationChecks.Run(view, folder);
#endif
            int start = text.IndexOf("bravo", StringComparison.Ordinal); view.Sel(start, start + 5);
            check(view.Sds() == "bravo", "forward rendered text range");
            view.Sel(start + 5, start); check(view.Sds() == "bravo", "reverse rendered text range");
            view.Sel(start, start); check(view.Sds() == "", "collapsed document range");
            view.Sel(0, text.Length); check(view.Sds() == text, "bounded select-all API");
            reject(() => view.Sel(-1, 0), "negative document range");
            reject(() => view.Sel(0, text.Length + 1), "oversize document range");
            reject(() => Task.Run(() => view.Dtx()).GetAwaiter().GetResult(), "document text cross thread");
            reject(() => Task.Run(() => view.Sel(0, 1)).GetAwaiter().GetResult(), "document range cross thread");
            int emoji = text.IndexOf("\ud83d\ude00", StringComparison.Ordinal); check(emoji >= 0, "surrogate fixture rendered");
            view.Sel(emoji, emoji + 1); check(view.Sds() == "", "selection does not split surrogate pair");
            view.Sel(emoji, emoji + 2); check(view.Sds() == "\ud83d\ude00", "whole surrogate element selectable");
            int accent = text.IndexOf("e\u0301", StringComparison.Ordinal); check(accent >= 0, "combining fixture rendered");
            view.Sel(accent, accent + 1); check(view.Sds() == "", "selection does not split combining element");
            var pending = typeof(V).GetField("activationPending", BindingFlags.Instance | BindingFlags.NonPublic); int prior = (int)pending.GetValue(view);
            view.Sel(start, start + 5); pending.SetValue(view, prior + 1);
            try { check(view.Sds() == "", "busy state clears document selection"); reject(() => view.Dtx(), "busy text model rejected"); } finally { pending.SetValue(view, prior); }
            check(view.Sds() == "", "cleared selection does not revive");
#if DOCUMENT_PRINT
            var options = new Lea.Print.Po(false, .75, 100, false, false, false, true);
            var geometry = Lea.Print.Pg.Cp(options, 850, 1100, new Lea.Print.Pb(0, 0, 850, 1100));
            reject(() => Lea.Print.Ds.Cp(view, geometry), "document print requires nonempty selection");
            view.Sel(start, start + 5);
            var documentPrint = Lea.Print.Ds.Cp(view, geometry);
            check(documentPrint.Text == "bravo" && String.IsNullOrEmpty(documentPrint.Capture.NativeSelection), "document print selection separate from native fields");
            string selectedHtml = documentPrint.Ht(options);
            reject(() => documentPrint.Ht(null), "document print output requires options");
            reject(() => documentPrint.Ht(new Lea.Print.Po(false, .75, 100, false, false, false, false)), "document print output cannot export full page");
            var selectedOutput = System.Xml.Linq.XDocument.Parse(selectedHtml);
            check(selectedOutput.Root.Element("body").Value.Contains("bravo") && !selectedOutput.Root.Element("body").Value.Contains("charlie"), "document print HTML only selected text");
            view.Sel(0, text.Length);
            check(!Lea.Print.Ds.Cp(view, geometry).Text.Contains("SELECTION_SECRET"), "document print selection excludes hidden and field secrets");
            PrintCaptureProbe.Activate(view, "password"); PrintCaptureProbe.Editor(view).SelectAll();
            reject(() => Lea.Print.Ds.Cp(view, geometry), "document print cannot capture password context");
            PrintCaptureProbe.Activate(view, "plain"); PrintCaptureProbe.Editor(view).Select(0, 5);
            reject(() => Lea.Print.Ds.Cp(view, geometry), "document print cannot substitute native field selection");
            view.Go(Path.Combine(folder, "print-friendly.html"));
            check(documentPrint.Text == "bravo", "captured document selection survives navigation");
            check(documentPrint.Ht(options) == selectedHtml, "captured document selection output remains detached after navigation");
            view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); Forms.Application.DoEvents();
            int whitespace = text.IndexOf(' '); view.Sel(whitespace, whitespace + 1);
            reject(() => Lea.Print.Ds.Cp(view, geometry), "document print rejects whitespace-only selection consistently with HTML writer");
#endif
            view.Sel(start, start + 5); view.Go(Path.Combine(folder, "print-friendly.html")); check(view.Sds() == "", "navigation clears stale document selection");
            view.Dispose(); reject(() => view.Sds(), "disposed document selection");
        }
        return count;
    }
}
