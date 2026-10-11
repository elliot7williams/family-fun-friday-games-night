using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Lea.Script.Win;
using Lea.Win.Test;

namespace Lea.Script.Browser
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            var smoke = args.Length > 0 && args[0] == "--smoke";
            var start = smoke ? args.Length > 1 ? args[1] : DefaultPage() : args.Length > 0 ? args[0] : DefaultPage();
            var report = smoke && args.Length > 2 ? Path.GetFullPath(args[2]) : null;
            var exit = 0;
            try
            {
                using (var window = new TestWindow(start, view => W.On(view), "Lea Script Browser"))
                using (var icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "lea.ico")))
                {
                    window.Icon = icon;
                    if (smoke)
                    {
                        window.ShowInTaskbar = false;
                        window.StartPosition = FormStartPosition.Manual;
                        window.Location = new Point(-20000, -20000);
                        var loaded = new TaskCompletionSource<bool>();
                        window.View.Done += (sender, eventArgs) => loaded.TrySetResult(true);
                        window.Shown += async (sender, eventArgs) =>
                        {
                            try
                            {
                                if (await Task.WhenAny(loaded.Task, Task.Delay(15000)) != loaded.Task) throw new TimeoutException("Browser startup deadline exceeded.");
                                var view = window.View;
                                if (view.Xo == null || !view.Xo.Ok || view.T != "Lea ready") throw new InvalidOperationException("Inline startup failed: " + (view.Xo == null ? "no result" : view.Xo.S + "; " + view.Xo.M));
                                for (var i = 0; i <= view.Kn && (view.Ke == null || view.Ke.Av("id") != "counter"); i++) view.Kf();
                                await view.Ka();
                                var runtime = (W)view.Xs;
                                if (view.T != "Lea 1" || view.Id("result").C[0].X != "1" || !runtime.At.Ac) throw new InvalidOperationException("Native browser click failed.");
                                var frames = 0;
                                if (view.Id("frame-start") != null)
                                {
                                    if (report != null)
                                        using (var before = new Bitmap(window.ClientSize.Width, window.ClientSize.Height))
                                        {
                                            window.DrawToBitmap(before, new Rectangle(Point.Empty, before.Size));
                                            before.Save(report + ".before.png", System.Drawing.Imaging.ImageFormat.Png);
                                        }
                                    for (var i = 0; i <= view.Kn && (view.Ke == null || view.Ke.Av("id") != "frame-start"); i++) view.Kf();
                                    await view.Ka();
                                    var clock = System.Diagnostics.Stopwatch.StartNew();
                                    while (clock.ElapsedMilliseconds < 5000)
                                    {
                                        Int32.TryParse(view.Id("frames").C[0].X, out frames);
                                        if (frames >= 3 && runtime.At.Rn > 0 && view.Xn == 0) break;
                                        await Task.Delay(10);
                                    }
                                    if (frames < 3 || runtime.At.Rn == 0) throw new InvalidOperationException("Browser animation did not advance.");
                                    for (var i = 0; i <= view.Kn && (view.Ke == null || view.Ke.Av("id") != "frame-stop"); i++) view.Kf();
                                    await view.Ka();
                                    if (runtime.At.Rn != 0) throw new InvalidOperationException("Browser animation did not pause.");
                                    var paused = view.Id("frames").C[0].X;
                                    var elapsed = view.Id("elapsed") == null ? "" : view.Id("elapsed").C[0].X;
                                    if (elapsed.Length != 0 && (!elapsed.EndsWith(" ms", StringComparison.Ordinal) ||
                                        !Int32.TryParse(elapsed.Substring(0, elapsed.Length - 3), out var milliseconds) || milliseconds < 0))
                                        throw new InvalidOperationException("Browser animation clock is invalid.");
                                    Int32.TryParse(paused, out frames);
                                    await Task.Delay(80);
                                    if (view.Id("frames").C[0].X != paused) throw new InvalidOperationException("Paused animation changed.");
                                    if (elapsed.Length != 0 && view.Id("elapsed").C[0].X != elapsed) throw new InvalidOperationException("Paused animation clock changed.");
                                }
                                view.Sc(-view.Sy);
                                if (report != null)
                                {
                                    using (var bitmap = new Bitmap(window.ClientSize.Width, window.ClientSize.Height))
                                    {
                                        window.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                                        bitmap.Save(report + ".png", System.Drawing.Imaging.ImageFormat.Png);
                                    }
                                    File.WriteAllText(report, new JavaScriptSerializer().Serialize(new {
                                        ok = true, title = view.T, nodes = view.At.N, pid = runtime.At.Pid,
                                        frames, frameCallbacks = runtime.At.Rn,
                                        frameStyle = view.Id("bar") == null ? "" : view.Id("bar").Av("style"),
                                        elapsed = view.Id("elapsed") == null ? "" : view.Id("elapsed").C[0].X,
                                        clockNow = runtime.At.Nw, timeOrigin = runtime.At.To,
                                        isolated = runtime.At.Ac, noLoopbackExemption = runtime.At.Ne,
                                        memoryLimit = runtime.At.Mm, modules = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>().Select(module => module.ModuleName).ToArray()
                                    }));
                                }
                            }
                            catch (Exception error) { exit = 1; if (report != null) File.WriteAllText(report + ".error.txt", error.ToString()); }
                            finally { window.Close(); }
                        };
                    }
                    Application.Run(window);
                }
            }
            catch (Exception error)
            {
                if (report != null) File.WriteAllText(report + ".error.txt", error.ToString());
                else MessageBox.Show(error.Message, "Lea Script Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            return exit;
        }

        private static string DefaultPage()
        { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LeaForge", "test-pages", "javascript-animation.html"); }
    }
}
