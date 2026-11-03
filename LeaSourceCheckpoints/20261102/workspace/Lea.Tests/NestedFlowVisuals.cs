using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

internal static class NestedFlowVisuals
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); checks++; Console.WriteLine("PASS " + name); }
    private static B ById(V view, string id)
    { return view.At.Bs.Single(block => block.El != null && block.El.Av("id") == id && block.K != "body"); }
    private static Rectangle Rect(V view, B block, string field = "C")
    {
        var boxes = (IEnumerable)typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
        foreach (object box in boxes)
        {
            var type = box.GetType();
            if (Object.ReferenceEquals(type.GetField("B").GetValue(box), block) && !(bool)type.GetField("Inline").GetValue(box))
                return (Rectangle)type.GetField(field).GetValue(box);
        }
        throw new InvalidOperationException("Missing layout box for " + block.K + ":" + block.X);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            using (var form = new Form { ClientSize = new Size(720, 900), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show(); Application.DoEvents();
                view.Go(args[0]); Application.DoEvents();
                var blocks = view.At.Bs;
                Check(blocks.Count == 13, "13 blocks include the separate anonymous prefix group");
                Check(String.Join(",", blocks.Select(block => block.K)) == "h1,section,p,div,article,h2,p,p,p,section,body,p,p", "exact ordered model kinds");
                Check(view.At.Ls.Count == 1, "nested link recorded once");
                var outer = ById(view, "outer"); var first = ById(view, "first");
                var middle = ById(view, "middle"); var inner = ById(view, "inner");
                var middleTail = ById(view, "middle-tail"); var outerTail = ById(view, "outer-tail");
                var mixed = ById(view, "mixed"); var child = ById(view, "mixed-child"); var after = ById(view, "after");
                var heading = blocks.Single(block => block.K == "h2");
                var innerText = blocks.Single(block => block.X.Contains("Nested flow link"));
                var prefix = blocks.Single(block => block.K == "body" && block.El == mixed.El);
                Check(mixed.X == "", "painted mixed context does not duplicate prefix text");
                Check(blocks.Count(block => block.X.Contains("Loose text before a nested block.")) == 1, "prefix appears exactly once");
                Check(prefix.Up == mixed, "anonymous prefix owned by mixed context");
                Check(child.Up == mixed, "nested child owned by mixed context");
                Check(after.Up == null, "following root block exits context");
                Check(first.Up == outer && middle.Up == outer && inner.Up == middle && outerTail.Up == outer, "nested visual context ownership retained");
                Check(heading.Up == inner && innerText.Up == inner && middleTail.Up == middle, "nested text ownership retained");
                int wideWidth = 0;
                foreach (int width in new[] { 720, 320 })
                {
                    form.ClientSize = new Size(width, 900); Application.DoEvents();
                    using (var image = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                        var o = Rect(view, outer); var ob = Rect(view, outer, "R");
                        var m = Rect(view, middle); var i = Rect(view, inner);
                        var mx = Rect(view, mixed); var mb = Rect(view, mixed, "R");
                        var p = Rect(view, prefix); var c = Rect(view, child); var a = Rect(view, after);
                        if (width == 720) wideWidth = o.Width;
                        Check(width == 720 ? o.Width == 360 : o.Width < wideWidth, width + " declared width or narrow reflow");
                        Check(Rect(view, first).Left == o.Left + 4, width + " child padding coordinate");
                        Check(m.Left == o.Left + 34, width + " middle margin border padding coordinate");
                        Check(i.Left == m.Left + 19, width + " inner margin border padding coordinate");
                        Check(i.Bottom > Rect(view, innerText).Bottom && Rect(view, innerText).Top > Rect(view, heading).Bottom, width + " inner ordered auto height");
                        Check(m.Bottom > Rect(view, middleTail).Bottom && Rect(view, middleTail).Top > Rect(view, inner, "R").Bottom, width + " middle subtree resumes");
                        Check(o.Bottom > Rect(view, outerTail).Bottom && Rect(view, outerTail).Top > Rect(view, middle, "R").Bottom, width + " outer subtree resumes");
                        Check(c.Top > p.Bottom, width + " separate prefix paints before nested child");
                        Check(mx.Bottom > c.Bottom && p.Left >= mx.Left && p.Right <= mx.Right, width + " mixed auto height contains prefix and child");
                        Check(a.Top > ob.Bottom && a.Top > mb.Bottom, width + " following root clears both containers");
                        Check(Rect(view, inner, "R").Right <= m.Right, width + " deep box remains inside parent");
                        var color = image.GetPixel(ob.Left + 4, ob.Top + 4);
                        Check(Math.Abs(color.R - 232) + Math.Abs(color.G - 245) + Math.Abs(color.B - 239) < 4, width + " background paints through padding");
                        int dark = 0;
                        for (int y = 0; y < image.Height; y += 4)
                            for (int x = 0; x < image.Width; x += 4)
                            { var pixel = image.GetPixel(x, y); if (pixel.R < 130 && pixel.G < 130 && pixel.B < 130) dark++; }
                        Check(dark > 150, width + " nonblank rendered pixels");
                        image.Save(Path.Combine(args[1], "nested-" + width + ".png"), ImageFormat.Png);
                        Console.WriteLine("VISUAL nested-" + width + " " + width + " 900 " + dark);
                        Console.WriteLine("GEOMETRY " + width + " prefix=" + p + " child=" + c + " context=" + mx + " after=" + a);
                    }
                }
                form.Hide();
            }
            Console.WriteLine("Nested flow visuals: checks=" + checks + "; failures=0");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
