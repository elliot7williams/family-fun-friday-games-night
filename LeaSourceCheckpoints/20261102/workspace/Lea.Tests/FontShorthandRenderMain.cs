using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class FontShorthandRenderMain
{
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private sealed class Capture : IDisposable
    {
        internal Bitmap Plain, Selected;
        internal string Geometry, Text;
        public void Dispose() { if (Plain != null) Plain.Dispose(); if (Selected != null) Selected.Dispose(); }
    }
    private static Bitmap Paint(V view, string path)
    {
        view.Refresh(); Forms.Application.DoEvents();
        var bitmap = new Bitmap(view.Width, view.Height);
        view.DrawToBitmap(bitmap, view.ClientRectangle); bitmap.Save(path, ImageFormat.Png); return bitmap;
    }
    private static Capture Read(V view, string template, string declarations, string stem)
    {
        string path = stem + ".html";
        File.WriteAllText(path, template.Replace("FONT_DECLARATION", declarations));
        view.Go(path); view.Refresh(); Forms.Application.DoEvents();
        var result = new Capture();
        try
        {
            result.Text = view.Dtx();
            Console.WriteLine("FONT_RENDER text=" + result.Text.Replace("\n", "\\n"));
            Check(Regex.Replace(result.Text, "\\s+", " ").Trim() == "iiiiWWWW Alpha bravo charlie delta echo foxtrot golf hotel india juliet kilo lima.", "fixture complete visible text across wrapping");
            result.Plain = Paint(view, stem + ".png");
            int ink = 0;
            for (int y = 0; y < result.Plain.Height; y++)
                for (int x = 0; x < result.Plain.Width; x++)
                { var color = result.Plain.GetPixel(x, y); if (color.R < 80 && color.G < 80 && color.B < 80) ink++; }
            Check(ink > 100, "rendered font ink is nonblank");
            int start = result.Text.IndexOf("iiii", StringComparison.Ordinal);
            view.Sel(start, start + 4); Check(view.Sds() == "iiii", "partial font selection payload");
            object snapshot = typeof(V).GetField("documentSelection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            var parts = (IEnumerable)snapshot.GetType().GetField("Ps").GetValue(snapshot);
            var geometry = new List<string>();
            foreach (var part in parts)
                geometry.Add((string)part.GetType().GetField("X").GetValue(part) + "|" + part.GetType().GetField("R").GetValue(part));
            Check(geometry.Count > 0, "font selection has geometry");
            result.Geometry = String.Join("\n", geometry.ToArray());
            result.Selected = Paint(view, stem + "-selected.png");
            int highlighted = 0;
            for (int y = 0; y < result.Selected.Height; y++)
                for (int x = 0; x < result.Selected.Width; x++)
                { var color = result.Selected.GetPixel(x, y); if (Math.Abs(color.R - 183) <= 3 && Math.Abs(color.G - 217) <= 3 && Math.Abs(color.B - 244) <= 3) highlighted++; }
            Check(highlighted > 20, "font selection highlight visible");
            Console.WriteLine("FONT_RENDER sample=" + Path.GetFileName(stem) + " ink=" + ink + " highlight=" + highlighted + " parts=" + geometry.Count);
            return result;
        }
        catch { result.Dispose(); throw; }
    }
    private static bool Equal(Bitmap left, Bitmap right)
    {
        if (left.Size != right.Size) return false;
        for (int y = 0; y < left.Height; y++)
            for (int x = 0; x < left.Width; x++)
                if (left.GetPixel(x, y) != right.GetPixel(x, y)) return false;
        return true;
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 && !(args.Length == 3 && (args[2] == "--initial" || args[2] == "--relative"))) return 2;
            string template = File.ReadAllText(args[0]);
            string[][] cases = {
                new[] { "plain", "font:24px Arial;", "font-size:24px;font-family:Arial;font-weight:normal;font-style:normal;line-height:normal;" },
                new[] { "styled", "font:italic bold 22px/36px 'Times New Roman', serif;", "font-size:22px;font-family:'Times New Roman',serif;font-style:italic;font-weight:bold;line-height:36px;" },
                new[] { "reset", "font-weight:bold;font-style:italic;line-height:2;font:24px/1.5 Arial;", "font-size:24px;font-family:Arial;font-weight:normal;font-style:normal;line-height:1.5;" }
            };
            if (args.Length == 3 && args[2] == "--initial")
            {
                var extended = new List<string[]>(cases);
                extended.Add(new[] { "initial", "font-size:29px;font-family:Arial;font-weight:bold;font-style:italic;line-height:41px;font:initial;", "font-size:16px;font-weight:normal;font-style:normal;line-height:normal;" });
                extended.Add(new[] { "initial-variable", "font-size:29px;font-family:Arial;font-weight:bold;font-style:italic;line-height:41px;font:var(--missing, INITIAL)!important;font-size:25px;", "font-size:16px;font-weight:normal;font-style:normal;line-height:normal;" });
                cases = extended.ToArray();
            }
            if (args.Length == 3 && args[2] == "--relative")
            {
                const string anchor = "body { margin:12px;";
                if (template.IndexOf(anchor, StringComparison.Ordinal) < 0 || template.IndexOf(anchor, template.IndexOf(anchor, StringComparison.Ordinal) + anchor.Length, StringComparison.Ordinal) >= 0) throw new Exception("Relative font parent fixture anchor");
                template = template.Replace(anchor, "body { font-size:20px; margin:12px;");
                cases = new[] {
                    new[] { "relative-em", "font:1.5em Arial;", "font:30px Arial;" },
                    new[] { "relative-percent", "font:150% Arial;", "font:30px Arial;" },
                    new[] { "relative-long-em", "font-family:Arial;font-size:1.5em;", "font-family:Arial;font-size:30px;" },
                    new[] { "relative-long-percent", "font-family:Arial;font-size:150%;", "font-family:Arial;font-size:30px;" },
                    new[] { "relative-order", "font-size:10px;font:italic bold 150%/1.5 Arial;", "font:italic bold 30px/1.5 Arial;" }
                };
            }
            using (var form = new Forms.Form { AutoScaleMode = Forms.AutoScaleMode.None, ClientSize = new Size(720, 500), Text = "Lea font equivalence" })
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show();
                foreach (int width in new[] { 720, 360 })
                {
                    form.ClientSize = new Size(width, 500); Forms.Application.DoEvents();
                    Check(view.Width == width, "exact font test viewport");
                    foreach (var sample in cases)
                    using (var shorthand = Read(view, template, sample[1], Path.Combine(args[1], sample[0] + "-" + width + "-short")))
                    using (var longhand = Read(view, template, sample[2], Path.Combine(args[1], sample[0] + "-" + width + "-long")))
                    {
                        Check(shorthand.Text == longhand.Text, "font text equivalence");
                        Check(shorthand.Geometry == longhand.Geometry, "font line/selection geometry equivalence");
                        Check(Equal(shorthand.Plain, longhand.Plain), "font plain pixels equivalent");
                        Check(Equal(shorthand.Selected, longhand.Selected), "font selected pixels equivalent");
                        Console.WriteLine("FONT_RENDER equivalent=" + sample[0] + " width=" + width);
                    }
                }
            }
            Console.WriteLine("FONT_RENDER PASS " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
