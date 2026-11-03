using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

internal static class NativeSelectionProbe
{
    [DllImport("user32.dll")] private static extern uint GetClipboardSequenceNumber();
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        checks++;
        Console.WriteLine("PASS " + name);
    }
    private static TextBox Editor(V view)
    { return (TextBox)typeof(V).GetField("ed", Private).GetValue(view); }
    private static void Paint(V view)
    {
        Application.DoEvents();
        using (var bitmap = new Bitmap(view.Width, view.Height))
            view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
    }
    private static Fi Activate(V view, string id)
    {
        for (var index = 0; index <= view.Kn; index++)
        {
            view.Kf();
            if (view.Ke != null && view.Ke.Av("id") == id)
            {
                Check(view.Ka().GetAwaiter().GetResult() == 1, "activate controlled editor " + id);
                return view.At.Fs.Single(field => field.El.Av("id") == id);
            }
        }
        throw new InvalidOperationException("Missing fixture focus target " + id);
    }
    private static void EmptyWithoutMutation(V view, string name)
    {
        var clipboard = GetClipboardSequenceNumber();
        Check(view.Sx() == String.Empty, name);
        Check(GetClipboardSequenceNumber() == clipboard, name + " leaves clipboard sequence unchanged");
    }
    private static void DialogSelection(Form owner, V view, TextBox editor)
    {
        Exception failure = null;
        using (var dialog = new Form { Text = "Native selection focus fixture", ClientSize = new Size(240, 80) })
        using (var focus = new TextBox { Dock = DockStyle.Fill })
        using (var timer = new Timer { Interval = 20 })
        {
            dialog.Controls.Add(focus);
            dialog.Shown += (sender, args) => { focus.Focus(); timer.Start(); };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                try
                {
                    Check(focus.Focused && !editor.Focused, "dialog takes native editor focus");
                    var clipboard = GetClipboardSequenceNumber();
                    Check(view.Sx() == "bravo", "dialog retains readable permitted selection");
                    Check(focus.Focused && GetClipboardSequenceNumber() == clipboard,
                        "dialog selection read preserves dialog focus and clipboard sequence");
                }
                catch (Exception error) { failure = error; }
                finally { dialog.Close(); }
            };
            dialog.ShowDialog(owner);
        }
        if (failure != null) throw new InvalidOperationException("Dialog selection failed", failure);
        Check(view.Sx() == "bravo", "selection remains available after dialog closes");
        Console.WriteLine("PASS Native selection dialog: permitted selection, dialog focus, clipboard sequence and retained read.");
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            using (var form = new Form { ClientSize = new Size(540, 380) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                form.Show();
                view.Go(args[0]);
                Paint(view);
                EmptyWithoutMutation(view, "unselected document has no native selection");
                var field = Activate(view, "plain");
                var editor = Editor(view);
                editor.Select(6, 5);
                var start = editor.SelectionStart;
                var length = editor.SelectionLength;
                var text = editor.Text;
                var focused = form.ActiveControl;
                var clipboard = GetClipboardSequenceNumber();
                Check(view.Ce("cp"), "fixture selection satisfies existing copy permission");
                Check(view.Sx() == "bravo", "reads only selected native text");
                Check(view.Sx() == "bravo", "repeated read retains selection");
                Check(editor.Text == text && editor.SelectionStart == start && editor.SelectionLength == length,
                    "selection read does not edit or select text");
                Check(form.ActiveControl == focused && GetClipboardSequenceNumber() == clipboard,
                    "selection read preserves focus and clipboard sequence");
                if (args.Length > 1 && args[1] == "dialog")
                {
                    DialogSelection(form, view, editor);
                    field = Activate(view, "plain");
                    editor = Editor(view);
                    editor.Select(6, 5);
                }
                typeof(V).GetField("editLock", Private).SetValue(view, true);
                try { EmptyWithoutMutation(view, "edit lock denies read"); }
                finally { typeof(V).GetField("editLock", Private).SetValue(view, false); }
                field.El.A["hidden"] = "";
                try { EmptyWithoutMutation(view, "hidden field denies read"); }
                finally { field.El.A.Remove("hidden"); }
                var disabled = typeof(Fi).GetProperty("Ds");
                disabled.SetValue(field, true, null);
                try { EmptyWithoutMutation(view, "disabled field denies read"); }
                finally { disabled.SetValue(field, false, null); }
                var original = field.V;
                typeof(Fi).GetProperty("V").SetValue(field, "not-the-editor-value", null);
                try { EmptyWithoutMutation(view, "stale editor binding denies read"); }
                finally { typeof(Fi).GetProperty("V").SetValue(field, original, null); }
                var position = view.At.Fs.IndexOf(field);
                view.At.Fs.RemoveAt(position);
                try { EmptyWithoutMutation(view, "detached field denies read"); }
                finally { view.At.Fs.Insert(position, field); }
                editor.Select(0, 0);
                EmptyWithoutMutation(view, "empty selection denies read");
                Activate(view, "password");
                Editor(view).SelectAll();
                EmptyWithoutMutation(view, "password selection is never exposed");
                Activate(view, "readonly");
                Editor(view).Select(0, 4);
                Check(view.Sx() == "read", "readonly non-password selection remains readable");
                view.Go(args[0]);
                Paint(view);
                EmptyWithoutMutation(view, "navigation clears old selection");
                view.Dispose();
                Check(view.Sx() == String.Empty, "disposed view returns empty");
            }
            Console.WriteLine("PASS Native selection: " + checks + " checks; controlled native fields only, no clipboard content read or write.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
