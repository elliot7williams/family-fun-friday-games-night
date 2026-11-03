using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class PrintCacheImageChecks
{
    internal static int Run(string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        Action<Action, string> reject = (action, name) => { bool denied = false; try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; } check(denied, name); };
        var uri = new Uri(Path.Combine(folder, "unavailable.png"));
        using (var uncreated = new V()) reject(() => uncreated.Pix(null, uri), "cache uncreated view");
        Bitmap detached = null;
#if FRIENDLY_BRIDGE
        Lea.Print.Pc captured = null;
#endif
        using (var form = new Forms.Form())
        using (var view = new V { Dock = Forms.DockStyle.Fill })
        {
            form.Controls.Add(view); form.Show(); view.Go(Path.Combine(folder, "print-cache-image.html")); Forms.Application.DoEvents();
            var ready = System.Diagnostics.Stopwatch.StartNew();
            while (view.Busy && ready.ElapsedMilliseconds < 2000) { Forms.Application.DoEvents(); System.Threading.Thread.Sleep(10); }
            check(!view.Busy, "cache fixture ready within owned run");
            var page = view.At; int native = view.In; long memory = view.It;
            reject(() => view.Pix(page, uri), "missing cached image");
            reject(() => view.Pix(null, uri), "null cached page");
            reject(() => view.Pix(page, new Uri("relative", UriKind.Relative)), "relative image address");
            reject(() => Task.Run(() => view.Pix(page, uri)).GetAwaiter().GetResult(), "cache cross thread");
            var pending = typeof(V).GetField("activationPending", BindingFlags.Instance | BindingFlags.NonPublic);
            int prior = (int)pending.GetValue(view); pending.SetValue(view, prior + 1);
            try { reject(() => view.Pix(page, uri), "cache busy view"); } finally { pending.SetValue(view, prior); }
            // Synthetic decoded cache fixture only; production uses Pix, never reflection.
            var cache = (Dictionary<string, Image>)typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            Image previous; bool existed = cache.TryGetValue(uri.AbsoluteUri, out previous);
            try
            {
                using (var bitmap = new Bitmap(3, 2))
                {
                    bitmap.SetPixel(0, 0, Color.Red); cache[uri.AbsoluteUri] = bitmap;
                    detached = view.Pix(page, uri); bitmap.SetPixel(0, 0, Color.Blue);
                    check(detached.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb(), "cache clone detached from mutation");
#if FRIENDLY_BRIDGE
                    var geometry = Lea.Print.Pg.Cp(new Lea.Print.Po(false, .75, 100, false, false, false, false), 850, 1100, new Lea.Print.Pb(0, 0, 850, 1100));
                    captured = Lea.Print.Pc.Cf(view, geometry);
                    var exported = Lea.Print.Ci.Cp(view, captured);
                    check(exported.Count == 1, "bridge captures snapshot image map");
                    string html = Lea.Print.Ht.Cp(captured.Document, new Lea.Print.Po(false, .75, 100, false, false, false, false), null, exported);
                    check(html.Contains("data:image/png;base64,"), "bridge generates self-contained image HTML");
                    check(((System.Collections.Generic.IDictionary<int, Lea.Print.Pi>)exported).IsReadOnly, "bridge map immutable");
                    reject(() => Lea.Print.Ci.Cp(null, captured), "bridge null view");
                    reject(() => Lea.Print.Ci.Cp(view, null), "bridge null capture");
                    reject(() => Task.Run(() => Lea.Print.Ci.Cp(view, captured)).GetAwaiter().GetResult(), "bridge cross thread");
                    pending.SetValue(view, prior + 1);
                    try { reject(() => Lea.Print.Ci.Cp(view, captured), "bridge busy view"); } finally { pending.SetValue(view, prior); }
                    using (var file = new FileStream(Path.Combine(folder, "friendly-cache-copy.html"), FileMode.CreateNew))
                    using (var writer = new StreamWriter(file)) writer.Write(html);
#endif
                    reject(() => view.Pix(page, new Uri(Path.Combine(folder, "not-referenced.png"))), "unreferenced cache address");
                }
                using (var oversized = new Bitmap(2049, 1))
                { cache[uri.AbsoluteUri] = oversized; reject(() => view.Pix(page, uri), "cache pixel bound"); }
            }
            finally { if (existed) cache[uri.AbsoluteUri] = previous; else cache.Remove(uri.AbsoluteUri); }
            check(view.In == native && view.It == memory && Object.ReferenceEquals(page, view.At), "cache capture no decoder/accounting/navigation change");
            view.Go(Path.Combine(folder, "print-friendly.html"));
            reject(() => view.Pix(page, uri), "stale cached page");
#if FRIENDLY_BRIDGE
            reject(() => Lea.Print.Ci.Cp(view, captured), "bridge stale page identity");
#endif
            view.Dispose(); reject(() => view.Pix(page, uri), "cache disposed view");
        }
        using (detached)
        {
            check(detached != null && detached.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb(), "cache clone survives view disposal");
            check(Lea.Print.Pi.Cp(detached).Width == 3, "cache clone enters print PNG boundary");
        }
        return count;
    }
}
