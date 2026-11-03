using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Lea.Win;

internal static class DocumentCopyChecks
{
    internal static int Run(V view, string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        var copy = typeof(V).GetMethod("CopyDocumentSelection", BindingFlags.Instance | BindingFlags.NonPublic);
        Func<Action<string>, int> invoke = writer => (int)copy.Invoke(view, new object[] { writer });
        string text = view.Dtx(), written = null; int start = text.IndexOf("bravo", StringComparison.Ordinal);
        view.Sel(start, start + 5);
        check(view.Ce("cp") && view.Ce("CP"), "document copy command enabled");
        check(view.Sx() == "", "document copy command never exposes retained native editor text");
        check(!view.Ce("cu") && !view.Ce("de"), "document selection not editable command");
        check(invoke(value => written = value) == 1 && written == "bravo", "document copy selected payload");
        check(view.Sds() == "bravo", "copy preserves selection");
        view.Sel(0, text.Length); written = null;
        check(invoke(value => written = value) == 1 && !written.Contains("SELECTION_SECRET"), "copy excludes hidden and native-field secrets");
        view.Sel(0, 0); int calls = 0;
        check(!view.Ce("cp") && view.Ec("cp") == 0, "collapsed copy menu declined without writing");
        check(invoke(value => calls++) == 0 && calls == 0, "collapsed selection does not invoke writer");
        view.Sel(start, start + 5);
        check(invoke(value => { throw new ExternalException("fixture clipboard busy"); }) == 0, "clipboard failure not reported successful");
        check(invoke(value => { throw new System.Threading.ThreadStateException("fixture clipboard apartment"); }) == 0, "clipboard apartment failure handled");
        check(invoke(value => view.Sel(0, 0)) == 0, "copy reentry selection change rejected");
        view.Sel(start, start + 5);
        check(invoke(value => view.Go(Path.Combine(folder, "print-friendly.html"))) == 0, "copy reentry navigation rejected");
        calls = 0; check(invoke(value => calls++) == 0 && calls == 0, "stale document copy does not invoke writer");
        view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); System.Windows.Forms.Application.DoEvents();
        view.Sel(0, view.Dtx().Length); PrintCaptureProbe.Activate(view, "password"); PrintCaptureProbe.Editor(view).SelectAll();
        check(!view.Ce("cp") && view.Sds() == "", "password context cannot fall back to document copy");
        PrintCaptureProbe.Activate(view, "plain"); PrintCaptureProbe.Editor(view).Select(0, 5);
        check(view.Ce("cp") && view.Sx() == "FIELD", "native text field copy availability retained");
        view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); System.Windows.Forms.Application.DoEvents();
        view.Sel(0, 5);
        check(view.Sx() == "", "document selection cannot expose native buffer retained after navigation");
        return count;
    }
}
