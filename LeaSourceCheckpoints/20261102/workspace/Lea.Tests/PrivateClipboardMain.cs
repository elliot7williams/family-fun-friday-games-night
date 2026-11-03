using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class PrivateClipboardMain
{
    private static int checks, clipboardCalls;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static void Set(string text) { PrivateClipboardStation.AssertPrivate(); clipboardCalls++; Forms.Clipboard.SetText(text); }
    private static string Get() { PrivateClipboardStation.AssertPrivate(); clipboardCalls++; return Forms.Clipboard.GetText(); }
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 1) return 2;
        bool passed = false;
        try { PrivateClipboardStation.Initialize(); Run(args[0]); passed = true; return 0; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally
        {
            string name = PrivateClipboardStation.Name ?? "";
            if (name.IndexOfAny(new[] { '"', '\\', '\r', '\n' }) >= 0) name = "";
            string json = "{\"passed\":" + (passed ? "true" : "false") + ",\"checks\":" + checks + ",\"clipboardCalls\":" + clipboardCalls +
                ",\"privateEstablished\":" + (PrivateClipboardStation.Established ? "true" : "false") + ",\"station\":\"" + name + "\",\"interactiveClipboardRead\":false,\"interactiveClipboardWritten\":false}";
            using (var file = new FileStream(Path.Combine(args[0], "clipboard.json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(json);
            Console.WriteLine(json);
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run(string folder)
    {
        PrivateClipboardStation.AssertPrivate(); Check(PrivateClipboardStation.Established, "private station established before WinForms");
        using (var form = new Forms.Form())
        using (var view = new V { Dock = Forms.DockStyle.Fill })
        {
            form.Controls.Add(view); form.Show(); view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); Forms.Application.DoEvents();
            string text = view.Dtx(); int bravo = text.IndexOf("bravo", StringComparison.Ordinal);
            Check(bravo >= 0 && !text.Contains("SELECTION_SECRET"), "private view selection source");
            view.Sel(bravo, bravo + 5); Set("private-sentinel"); PrivateClipboardStation.AssertPrivate();
            Check(view.Sdc() == 1 && Get() == "bravo", "actual Windows Unicode clipboard selected copy");
            view.Sel(0, 5); Set("private-sentinel"); PrivateClipboardStation.AssertPrivate();
            var key = new Forms.KeyEventArgs(Forms.Keys.Control | Forms.Keys.C);
            typeof(V).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { key });
            Check(key.Handled && key.SuppressKeyPress && Get() == "Alpha", "Ctrl+C handler reaches real private clipboard");
            view.Sel(0, text.Length); PrivateClipboardStation.AssertPrivate();
            Check(view.Sdc() == 1 && Get() == text, "actual selected all Unicode text");
            Check(!Get().Contains("SELECTION_SECRET"), "actual copy has no hidden/field secrets");
            view.Sel(0, 0); Set("private-sentinel"); PrivateClipboardStation.AssertPrivate();
            Check(view.Sdc() == 0 && Get() == "private-sentinel", "empty selection leaves clipboard unchanged");
            view.Sel(bravo, bravo + 5); view.Go(Path.Combine(folder, "print-friendly.html")); PrivateClipboardStation.AssertPrivate();
            Check(view.Sdc() == 0 && Get() == "private-sentinel", "navigation leaves clipboard unchanged");
            bool denied = false;
            try { Task.Run(() => view.Sdc()).GetAwaiter().GetResult(); } catch (InvalidOperationException) { denied = true; }
            Check(denied && Get() == "private-sentinel", "cross-thread copy declined before native adapter");
        }
        PrivateClipboardStation.AssertPrivate(); Forms.Application.ExitThread();
    }
}
