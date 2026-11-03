using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

internal static class NamedVisuals
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("FAIL " + name);
        checks++; Console.WriteLine("PASS " + name);
    }
    private static string Text(N node)
    {
        if (node.K == "#text") return node.X;
        var result = new StringBuilder(); foreach (var child in node.C) result.Append(Text(child)); return result.ToString();
    }
    private static TextBox Field(Control parent, string text)
    {
        foreach (Control child in parent.Controls)
        {
            var box = child as TextBox;
            if (box != null && box.Text == text) return box;
            var nested = Field(child, text); if (nested != null) return nested;
        }
        return null;
    }
    private static void Capture(V view, TextBox field, string directory, string name)
    {
        using (var image = new Bitmap(view.Width, view.Height))
        {
            view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            int dark = 0;
            for (int y = 0; y < image.Height; y += 4)
                for (int x = 0; x < image.Width; x += 4)
                {
                    var color = image.GetPixel(x, y);
                    if (color.R < 130 && color.G < 130 && color.B < 130) dark++;
                }
            Check(dark > 150, name + " nonblank dark pixels");
            image.Save(Path.Combine(directory, name + ".png"), ImageFormat.Png);
            Console.WriteLine("VISUAL " + name + " " + image.Width + " " + image.Height + " " + dark);
        }
        if (field == null) return;
        var location = view.PointToClient(field.PointToScreen(Point.Empty));
        Check(field.Visible && location.X >= 0 && location.X + field.Width <= view.ClientSize.Width,
            name + " native field inside viewport");
        Check(location.Y >= 0 && location.Y + field.Height <= view.ClientSize.Height,
            name + " native field vertically visible");
    }
    private static Rectangle NodeRect(V view, string id)
    {
        var node = view.Id(id);
        var boxes = (IEnumerable)typeof(V).GetField("boxes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
        var result = Rectangle.Empty;
        foreach (object box in boxes)
            if (Object.ReferenceEquals(box.GetType().GetField("El").GetValue(box), node))
            {
                var rect = (Rectangle)box.GetType().GetField("R").GetValue(box);
                result = result.IsEmpty ? rect : Rectangle.Union(result, rect);
            }
        if (node == null || result.IsEmpty) throw new InvalidOperationException("Missing painted node " + id);
        return result;
    }
    private static void FlowOrder(V view)
    {
        var blocks = view.At.Bs.ToList();
        int heading = blocks.FindIndex(block => block.X == "Native fields");
        int first = blocks.FindIndex(block => block.X.Contains("Reference value"));
        int field = blocks.FindIndex(block => block.I != null && block.I.N == "ref");
        int notes = blocks.FindIndex(block => block.X == "Notes");
        int last = blocks.FindIndex(block => block.I != null && block.I.N == "notes");
        Check(heading >= 0 && first > heading, "heading precedes reference label");
        Check(field > first && first >= 0, "reference label precedes its field");
        Check(notes > field && field >= 0, "reference field precedes notes label");
        Check(last > notes && notes >= 0, "notes label precedes its field");
    }
    private static void LiteralPainting()
    {
        var paint = typeof(V).GetMethod("PaintTextCore", BindingFlags.NonPublic | BindingFlags.Static);
        var measure = typeof(V).GetMethod("TextWidth", BindingFlags.NonPublic | BindingFlags.Static, null,
            new[] { typeof(Graphics), typeof(string), typeof(Font), typeof(S) }, null);
        if (paint == null || measure == null) throw new InvalidOperationException("Expected view text methods");
        using (var font = new Font("Segoe UI", 18))
            foreach (string text in new[] { "R&D", "&copy;", "A&&B", "&Z", "end&" })
                using (var actual = new Bitmap(400, 60))
                using (var expected = new Bitmap(400, 60))
                using (var a = Graphics.FromImage(actual))
                using (var e = Graphics.FromImage(expected))
                {
                    a.Clear(Color.White); e.Clear(Color.White);
                    var rect = new Rectangle(0, 0, 400, 60);
                    var flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
                    paint.Invoke(null, new object[] { a, text, font, rect, Color.Black, flags, 100, null });
                    TextRenderer.DrawText(e, text, font, rect, Color.Black, flags | TextFormatFlags.NoPrefix);
                    bool same = true;
                    for (int y = 0; y < 60 && same; y++)
                        for (int x = 0; x < 400; x++)
                            if (actual.GetPixel(x, y) != expected.GetPixel(x, y)) { same = false; break; }
                    Check(same, "literal ampersand painting " + text);
                    int width = (int)measure.Invoke(null, new object[] { a, text, font, null });
                    Check(width == TextRenderer.MeasureText(a, text, font, Size.Empty, flags | TextFormatFlags.NoPrefix).Width,
                        "literal ampersand measurement " + text);
                }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LiteralPainting();
            using (var form = new Form { ClientSize = new Size(980, 720), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); Application.DoEvents();
                view.Go(args[0]); Application.DoEvents();
                using (var warm = new Bitmap(view.Width, view.Height)) view.DrawToBitmap(warm, new Rectangle(Point.Empty, warm.Size));
                FlowOrder(view);
                Capture(view, null, args[1], "desktop-top");
                Check(view.Sy == 0, "desktop top viewport captured");
                Check(NodeRect(view, "native-heading").Bottom <= NodeRect(view, "ref-label").Top, "desktop heading geometry precedes label");
                form.ClientSize = new Size(420, 720); Application.DoEvents();
                Capture(view, null, args[1], "narrow-top");
                Check(view.Sy == 0, "narrow top viewport captured");
                Check(NodeRect(view, "native-heading").Bottom <= NodeRect(view, "ref-label").Top, "narrow heading geometry precedes label");
                form.ClientSize = new Size(980, 720); Application.DoEvents();
                Check(view.At.T == "Lea \u00a9 reference rendering", "decoded title");
                Check(Text(view.Id("relations")) == "Relations: \u2242\u0338 \u2209 \u2260", "combining and relation text");
                Check(Text(view.Id("letters")) == "Letters: \u00c6 \u0153 \ud835\udd04 fj", "supplementary and two-letter text");
                Check(Text(view.Id("literal")) == "&copy; &NotEqualTilde;", "single-pass literal text");
                Check(view.Id("ambiguous").Av("data-x") == "&copyZ &notit; &amp=0", "ambiguous attribute remains literal");
                const string initial = "&copyZ | \u2242\u0338 | R&D";
                Check(view.Kf() == 1 && view.Ke == view.Id("ref"), "keyboard focus reaches reference field");
                Check(view.Ka().GetAwaiter().GetResult() == 1, "keyboard activation opens native field");
                Application.DoEvents();
                var field = Field(view, initial);
                Check(field != null, "decoded native field exists");
                var state = view.At.Fs.Single(item => item.N == "ref");
                Check(state.V == initial, "native state has decoded value");
                Capture(view, field, args[1], "desktop");
                form.ClientSize = new Size(420, 720); Application.DoEvents();
                Capture(view, field, args[1], "narrow");
                Check(Object.ReferenceEquals(field, Field(view, initial)), "field identity retained on narrow resize");
                field.Text = "Edited & retained"; Application.DoEvents();
                Check(state.V == field.Text, "native edit reaches engine state");
                Capture(view, field, args[1], "narrow-edited");
                form.ClientSize = new Size(980, 720); Application.DoEvents();
                Capture(view, field, args[1], "desktop-edited");
                Check(Object.ReferenceEquals(field, Field(view, "Edited & retained")), "edited field identity retained on wide resize");
                Check(state.V == "Edited & retained", "edited state retained on wide resize");
                form.Hide();
            }
            Console.WriteLine("Named visuals: checks=" + checks + "; failures=0");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
