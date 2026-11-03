using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Lea.Win;

internal static class TitleMetadataVisuals
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); checks++; Console.WriteLine("PASS " + name); }
    private static object[] Boxes(V view)
    { return ((IEnumerable)typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view)).Cast<object>().ToArray(); }
    private static object Value(object box, string field) { return box.GetType().GetField(field).GetValue(box); }
    private static TextBox Editor(Control parent)
    {
        foreach (Control child in parent.Controls)
        { if (child is TextBox && child.Visible) return (TextBox)child; var nested = Editor(child); if (nested != null) return nested; }
        return null;
    }
    private static int Dark(Bitmap image)
    {
        int count = 0;
        for (int y = 0; y < image.Height; y += 3)
            for (int x = 0; x < image.Width; x += 3)
            { var pixel = image.GetPixel(x, y); if (pixel.R < 130 && pixel.G < 130 && pixel.B < 130) count++; }
        return count;
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            using (var form = new Form { ClientSize = new Size(720, 500), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); Application.DoEvents(); view.Go(args[0]); Application.DoEvents();
                Check(view.T == "Metadata Sentinel" && view.At.T == view.T, "document title metadata preserved");
                Check(view.At.Bs.Count(block => block.K == "h1" && block.X == "Authored heading") == 1, "authored heading retained once");
                foreach (int width in new[] { 720, 420 })
                {
                    form.ClientSize = new Size(width, 500); Application.DoEvents();
                    using (var image = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                        var boxes = Boxes(view);
                        Check(boxes.All(box => !(bool)Value(box, "Title")), "synthetic page title absent");
                        Check(boxes.All(box => !((string)Value(box, "X")).Contains("Metadata Sentinel")), width + " metadata is not document content");
                        var heading = boxes.First(box => (string)Value(box, "K") == "h1" && !(bool)Value(box, "Inline"));
                        Check(((Rectangle)Value(heading, "R")).Top < 45, width + " authored content starts without title gap");
                        int dark = Dark(image); Check(dark > 80, width + " authored page is nonblank");
                        image.Save(Path.Combine(args[1], "title-" + width + ".png"), ImageFormat.Png);
                        Console.WriteLine("VISUAL title-" + width + " " + width + " 500 " + dark);
                    }
                }
                Check(view.Kf() == 1 && view.Ke == view.Id("field"), "native field remains focusable");
                Check(view.Ka().GetAwaiter().GetResult() == 1, "native field remains activatable");
                Application.DoEvents(); var editor = Editor(view); Check(editor != null, "native editor exists after layout change");
                editor.Text = "Edited value"; Application.DoEvents();
                Check(view.At.Fs.Single(field => field.N == "field").V == "Edited value", "native edit reaches retained state");
                var blank = Path.Combine(args[1], "metadata-only.html");
                File.WriteAllText(blank, "<!doctype html><html><head><title>Other Metadata</title></head><body></body></html>");
                view.Go(blank); Application.DoEvents();
                using (var image = new Bitmap(view.Width, view.Height))
                {
                    view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    Check(view.T == "Other Metadata", "navigation updates title metadata");
                    Check(Boxes(view).All(box => !(bool)Value(box, "Title")), "empty document has no synthetic title");
                    Check(Boxes(view).All(box => !((string)Value(box, "X")).Contains("Other Metadata")), "empty document does not leak metadata into fallback text");
                    Check(Dark(image) == 0, "metadata-only document remains visibly empty");
                }
                form.Hide();
            }
            Console.WriteLine("Title metadata visuals: checks=" + checks + "; failures=0");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
