using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Print;
using Lea.Win;

internal static class PrintCaptureProbe
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; }
        catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    internal static TextBox Editor(V view) { return (TextBox)typeof(V).GetField("ed", Private).GetValue(view); }
    internal static void Activate(V view, string id)
    {
        for (int i = 0; i <= view.Kn; i++)
        {
            view.Kf();
            if (view.Ke != null && view.Ke.Av("id") == id)
            { if (view.Ka().GetAwaiter().GetResult() != 1) throw new Exception("Activation failed"); return; }
        }
        throw new Exception("Missing editor " + id);
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            Reject(() => Pc.Cp(null), "null view rejected");
            using (var uncreated = new V()) Reject(() => Pc.Cp(uncreated), "uncreated handle rejected");
            using (var form = new Form { ClientSize = new Size(540, 380) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); view.Go(args[0]); Application.DoEvents();
                using (var bitmap = new Bitmap(view.Width, view.Height))
                    view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                Pc capture = Pc.Cp(view);
                Check(capture.Document.Nodes.Count > 0, "ready document captured");
                Check(capture.Document.Title == "Lea native selection", "live title");
                Check(capture.Document.Nodes.Any(n => n.Text.Contains("Plain")), "parsed content captured");
                Check(capture.NativeSelection == "", "no invented document selection");
                Reject(() => Task.Run(() => Pc.Cp(view)).GetAwaiter().GetResult(), "cross thread rejected without UI invoke");
                var pending = typeof(V).GetField("activationPending", Private);
                int prior = (int)pending.GetValue(view); pending.SetValue(view, prior + 1);
                try { Reject(() => Pc.Cp(view), "busy view rejected"); }
                finally { pending.SetValue(view, prior); }
                Activate(view, "plain"); TextBox editor = Editor(view); editor.Select(6, 5);
                var focus = form.ActiveControl; int start = editor.SelectionStart, length = editor.SelectionLength;
                capture = Pc.Cp(view);
                Check(capture.NativeSelection == "bravo", "permitted native selection captured");
                Check(form.ActiveControl == focus && editor.SelectionStart == start && editor.SelectionLength == length,
                    "focus and selection preserved");
                editor.Select(0, 5);
                Check(capture.NativeSelection == "bravo", "capture detached from live editor");
                Activate(view, "password"); Editor(view).SelectAll();
                Check(Pc.Cp(view).NativeSelection == "", "password selection denied");
                view.Dispose(); Reject(() => Pc.Cp(view), "disposed view rejected");
            }
            if (checks != 13) throw new Exception("Unexpected check count");
            Console.WriteLine("PASS Lea print capture: 13 checks"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
