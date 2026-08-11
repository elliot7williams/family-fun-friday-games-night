using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Tests
{
    internal static class Smoke
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
                var forms = Path.Combine(root, "LeaForge", "test-pages", "forms.html");
                var engine = new E();
                var page = engine.Go(forms);
                Eq(8, page.Fs.Count, "field count");
                Eq("text,password,textarea,checkbox,radio,radio,reset,submit", Kinds(page), "field kinds");
                Ok(page.Fs[3].C && page.Fs[4].C && !page.Fs[5].C, "choice defaults");

                using (var view = new V())
                {
                    view.Size = new Size(720, 760);
                    view.CreateControl();
                    view.Go(forms);
                    Paint(view);
                    Ok(view.Bc > page.Fs.Count, "form layout boxes");
                    Eq("Open result", view.At.Bs[view.At.Bs.Count - 1].X, "button label");

                    view.At.Fs[0].V = "changed";
                    view.At.Fs[3].C = false;
                    view.Rs(view.At.Fs[6]);
                    Eq("Lea", view.At.Fs[0].V, "reset text");
                    Ok(view.At.Fs[3].C, "reset checkbox");

                    view.At.Fs[0].V = "small engine";
                    view.Sm(view.At.Fs[7]);
                    Eq("Lea Form Result", view.T, "GET result");
                    Ok(view.Q.Contains("q=small%20engine") && view.Q.Contains("updates=yes") && view.Q.Contains("size=tiny") && view.Q.Contains("mode=lea"), "GET values: " + view.Q + " uri=" + view.U.AbsoluteUri + " local=" + view.U.LocalPath);

                    view.Go(forms);
                    view.At.Fs[0].V = "async engine";
                    var submit = view.SmA(view.At.Fs[7]);
                    while (!submit.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    submit.GetAwaiter().GetResult();
                    Eq("Lea Form Result", view.T, "async GET result");
                    Ok(view.Q.Contains("q=async%20engine"), "async GET values");

                    Check(view, root, "css.html", 3, 4);
                    Check(view, root, "images.html", 5, 6);
                    Check(view, root, "layout.html", 3, 4);
                    view.Go(Path.Combine(root, "LeaForge", "test-pages", "scroll.html"));
                    Paint(view);
                    Eq(16, view.Bc, "scroll boxes");
                    view.Sc(100000);
                    Eq(Math.Max(0, view.Dh - view.Height), view.Sy, "bounded scroll");
                }

                Console.WriteLine("Lea smoke passed: forms, sync/async GET, CSS, images, layout, and scrolling.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Lea smoke failed: " + error);
                return 1;
            }
        }

        private static void Paint(V view)
        {
            using (var image = new Bitmap(view.Width, view.Height))
                view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
        }

        private static void Check(V view, string root, string file, int blocks, int boxes)
        {
            view.Go(Path.Combine(root, "LeaForge", "test-pages", file));
            Paint(view);
            Eq(blocks, view.At.Bs.Count, file + " blocks");
            Eq(boxes, view.Bc, file + " boxes");
        }

        private static string Kinds(P page)
        {
            var values = new string[page.Fs.Count];
            for (var i = 0; i < values.Length; i++) values[i] = page.Fs[i].K;
            return String.Join(",", values);
        }

        private static void Eq<T>(T expected, T actual, string name)
        {
            if (!Object.Equals(expected, actual)) throw new InvalidOperationException(name + ": expected " + expected + ", got " + actual);
        }

        private static void Ok(bool value, string name)
        {
            if (!value) throw new InvalidOperationException(name);
        }
    }
}
