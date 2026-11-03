using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

internal static class FocusClickProbe
{
    private static void Paint(V view)
    {
        using (var image = new Bitmap(view.Width, view.Height))
            view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
    }
    private static void Require(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); Console.WriteLine("PASS " + name); }
    private static void Click(V view, Rectangle rect)
    {
        typeof(V).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(view, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, 0) });
    }
    private static void Probe(V view)
    {
        var rect = view.Kb;
        var point = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        Console.WriteLine("FOCUS bounds=" + rect + "; scroll=" + view.Sy + "; client=" + view.ClientRectangle);
        foreach (var name in new[] { "BoxAt", "HitAt", "FocusAt" })
        {
            var method = typeof(V).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            var target = method.Invoke(view, name == "FocusAt" ? new object[] { point, null } : new object[] { point });
            var node = target == null ? null : (N)target.GetType().GetField("El").GetValue(target);
            Console.WriteLine("FOCUS " + name + "=" + (target == null ? "null" : node == null ? "no-element" : node.Av("id")));
        }
        typeof(V).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(view, new object[] { new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0) });
        Console.WriteLine("FOCUS after=" + (view.Ke == null ? "null" : view.Ke.Av("id")));
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            using (var view = new V())
            {
                view.Size = new Size(520, 280); view.CreateControl();
                foreach (bool repaint in new[] { false, true })
                {
                    view.Go(args[0]); Paint(view);
                    Require(view.Kn == 14, "keyboard fixture target count");
                    for (int index = 0; index < 5; index++) view.Kf();
                    Require(view.Ke != null && view.Ke.Av("id") == "priority-button", "button focus");
                    Require(view.Ka().GetAwaiter().GetResult() == 1, "button activation");
                    view.Kf();
                    Require(view.Ke != null && view.Ke.Av("id") == "priority-generic", "generic focus before click");
                    Require(view.Ka().GetAwaiter().GetResult() == 0, "generic has no activation");
                    if (repaint) Paint(view);
                    Console.WriteLine("CASE repaint=" + repaint);
                    Probe(view);
                    bool retained = view.Ke != null && view.Ke.Av("id") == "priority-generic";
                    Console.WriteLine("OUTCOME retained=" + retained);
                    if (repaint || args.Length > 1) Require(retained, "generic click retains focus; repaint=" + repaint);
                }
                if (args.Length > 1)
                {
                    view.Go(args[0]); Paint(view);
                    for (int index = 0; index < 5; index++) view.Kf();
                    Require(view.Ka().GetAwaiter().GetResult() == 1, "direct case button activation");
                    view.Kf();
                    Require(view.Ke != null && view.Ke.Av("id") == "priority-generic", "direct case generic focus");
                    Click(view, view.Kb);
                    Require(view.Ke != null && view.Ke.Av("id") == "priority-generic", "direct click without diagnostic pre-query retains focus");
                    Require(view.U.Fragment.Length == 0, "direct generic click does not navigate old link");
                    view.Go(args[0]); Paint(view); view.Kf();
                    Require(view.Ke != null && view.Ke.Av("id") == "priority-one", "direct link focus");
                    Click(view, view.Kb);
                    Require(view.U.Fragment == "#one-target", "direct link click after restyle activates current link");
                }
            }
            Console.WriteLine("Focus diagnostic completed; not full-suite qualification.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
