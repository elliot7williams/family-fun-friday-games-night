using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class DocumentSelectionGestures
{
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr parameter, IntPtr point);
    private static void Mouse(V view, int message, Point point, bool down)
    { SendMessage(view.Handle, message, new IntPtr(down ? 1 : 0), new IntPtr((point.X & 65535) | ((point.Y & 65535) << 16))); }
    internal static int Run(V view, string folder)
    {
        int checks = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); checks++; };
        view.Sel(0, 0);
        var snapshot = typeof(V).GetField("documentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
        var parts = (IEnumerable)snapshot.GetType().GetField("Ps").GetValue(snapshot); Rectangle first = Rectangle.Empty; string expected = "";
        foreach (var part in parts)
        {
            string text = (string)part.GetType().GetField("X").GetValue(part);
            if (!text.Contains("Alpha bravo")) continue;
            first = (Rectangle)part.GetType().GetField("R").GetValue(part); expected = text; break;
        }
        check(!first.IsEmpty, "gesture fixture text geometry");
        var left = new Point(first.Left, first.Top + first.Height / 2); var right = new Point(first.Right, left.Y); var page = view.At;
        Mouse(view, 0x201, left, true); Mouse(view, 0x200, right, true); Mouse(view, 0x202, right, false);
        check(view.Sds() == expected, "native mouse-message forward selection");
        check(!view.Capture && Object.ReferenceEquals(page, view.At), "drag retires mouse capture without navigation");
        using (var bitmap = new Bitmap(view.Width, view.Height))
        {
            view.DrawToBitmap(bitmap, view.ClientRectangle); int blue = 0;
            for (int y = Math.Max(0, first.Top); y < Math.Min(bitmap.Height, first.Bottom); y++)
                for (int x = Math.Max(0, first.Left); x < Math.Min(bitmap.Width, first.Right); x++)
                { var color = bitmap.GetPixel(x, y); if (color.B - color.R > 30) blue++; }
            check(blue > 80, "selection highlight visible pixels");
            bitmap.Save(Path.Combine(folder, "document-selection-highlight.png"), System.Drawing.Imaging.ImageFormat.Png);
        }
        Mouse(view, 0x201, right, true); Mouse(view, 0x200, left, true); Mouse(view, 0x202, left, false);
        check(view.Sds() == expected, "native mouse-message reverse selection");
        var key = typeof(V).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
        var all = new Forms.KeyEventArgs(Forms.Keys.Control | Forms.Keys.A); key.Invoke(view, new object[] { all });
        check(all.Handled && view.Sds() == view.Dtx(), "Ctrl+A page selection hook");
        var escape = new Forms.KeyEventArgs(Forms.Keys.Escape); key.Invoke(view, new object[] { escape });
        check(escape.Handled && view.Sds() == "", "Escape clears page selection");
        view.Sel(0, 0); snapshot = typeof(V).GetField("documentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
        parts = (IEnumerable)snapshot.GetType().GetField("Ps").GetValue(snapshot); Rectangle variable = Rectangle.Empty;
        foreach (var part in parts)
            if ((string)part.GetType().GetField("X").GetValue(part) == "iiiiWWWW") { variable = (Rectangle)part.GetType().GetField("R").GetValue(part); break; }
        check(!variable.IsEmpty, "variable glyph fixture geometry");
        int narrow, fontSize = 0;
        foreach (var pair in view.Pst(view.Width, view.Height)) if (pair.Key.Av("id") == "variable") fontSize = pair.Value.Fs;
        check(fontSize == 24, "variable fixture computed font longhand active");
        using (var graphics = view.CreateGraphics())
        using (var font = new Font("Arial", fontSize))
            narrow = Forms.TextRenderer.MeasureText(graphics, "iiii", font, Size.Empty, Forms.TextFormatFlags.SingleLine | Forms.TextFormatFlags.NoPrefix | Forms.TextFormatFlags.NoPadding).Width;
        check(narrow * 2 < variable.Width, "fixture distinguishes measured from uniform character widths");
        left = new Point(variable.Left, variable.Top + variable.Height / 2); right = new Point(variable.Left + narrow, left.Y);
        Mouse(view, 0x201, left, true); Mouse(view, 0x200, right, true); Mouse(view, 0x202, right, false);
        Console.WriteLine("VARIABLE selected=" + view.Sds() + " measuredPrefix=" + narrow + " bounds=" + variable);
        check(view.Sds() == "iiii", "measured narrow glyph drag endpoint");
        using (var bitmap = new Bitmap(view.Width, view.Height))
        {
            view.DrawToBitmap(bitmap, view.ClientRectangle); int farthest = -1, highlighted = 0;
            for (int y = variable.Top; y < Math.Min(bitmap.Height, variable.Top + 2); y++)
                for (int x = variable.Left; x < Math.Min(bitmap.Width, variable.Right); x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (Math.Abs(color.R - 183) <= 3 && Math.Abs(color.G - 217) <= 3 && Math.Abs(color.B - 244) <= 3)
                    { farthest = Math.Max(farthest, x); highlighted++; }
                }
            bitmap.Save(Path.Combine(folder, "document-selection-variable.png"), System.Drawing.Imaging.ImageFormat.Png);
            Console.WriteLine("VARIABLE highlightRight=" + (farthest + 1) + " expectedRight=" + (variable.Left + narrow));
            check(Math.Abs(farthest + 1 - (variable.Left + narrow)) <= 2, "measured partial highlight width");
            check(highlighted >= narrow, "measured highlight background coverage");
        }
        var failure = typeof(V).GetProperty("Le"); var previousFailure = failure.GetValue(view, null);
        var error = Activator.CreateInstance(typeof(Lea.Engine.Er), BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { "io", new Uri("https://example.test/"), 0, false, "selection error fixture", null }, null);
        failure.SetValue(view, error, null);
        try
        {
            var unavailable = new Forms.KeyEventArgs(Forms.Keys.Control | Forms.Keys.A);
            bool consumed = (bool)typeof(V).GetMethod("DocumentSelectionKey", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { unavailable });
            check(!consumed && !unavailable.Handled, "load-error selection key safely declined");
        }
        finally { failure.SetValue(view, previousFailure, null); }
        view.Sel(0, 0); snapshot = typeof(V).GetField("documentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
        parts = (IEnumerable)snapshot.GetType().GetField("Ps").GetValue(snapshot); Rectangle link = Rectangle.Empty;
        foreach (var part in parts)
            if ((string)part.GetType().GetField("X").GetValue(part) == "LINK_SELECTION") { link = (Rectangle)part.GetType().GetField("R").GetValue(part); break; }
        check(!link.IsEmpty, "link selection geometry");
        left = new Point(link.Left, link.Top + link.Height / 2); right = new Point(link.Right, left.Y);
        Mouse(view, 0x201, left, true); Mouse(view, 0x200, right, true); Mouse(view, 0x202, right, false); Forms.Application.DoEvents();
        check(Object.ReferenceEquals(page, view.At) && view.Sds() == "LINK_SELECTION", "drag link selects without navigation");
        PrintCaptureProbe.Activate(view, "plain"); var editor = PrintCaptureProbe.Editor(view); editor.Select(0, 5);
        check(view.Sds() == "" && view.Sx() == "FIELD", "native field selection remains separate");
        view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); Forms.Application.DoEvents();
        return checks;
    }
}
