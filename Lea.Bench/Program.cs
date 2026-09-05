using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Bench
{
    internal static class Program
    {
        private const int ViewWidth = 1024;
        private const int ViewHeight = 720;

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var options = Options.Read(args);
                var root = FindRoot(options.Root);
                if (options.StartupOnly) return StartupOnly(root);
                var output = Path.GetFullPath(options.Output ?? Path.Combine(root, "LeaForge", "COMPATIBILITY_PERFORMANCE.md"));
                var fixtures = FixturePaths(root);

                Console.WriteLine("Lea report: warming " + fixtures.Count + " fixtures...");
                Warm(fixtures);

                var startup = MeasureStartup(root, Math.Max(3, Math.Min(7, options.Iterations / 3)));
                var loads = fixtures.Select(path => MeasureLoad(path, options.Iterations)).ToList();
                var render = MeasureRender(fixtures, Math.Max(3, options.Iterations / 3));
                var memory = MeasureMemory(fixtures, Math.Max(4, options.Iterations / 3));
                var probes = RunProbes(root);
                var artifacts = ArtifactPaths(root, options.Browser);
                var report = Report(options.Iterations, startup, loads, render, memory, probes, artifacts);

                var folder = Path.GetDirectoryName(output);
                if (!String.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                File.WriteAllText(output, report, new UTF8Encoding(false));

                Console.WriteLine("Lea report written: " + output);
                Console.WriteLine("Clean startup median: " + F(startup) + " ms");
                Console.WriteLine("Load median: " + F(loads.SelectMany(item => item.Times).ToArray()) + " ms");
                Console.WriteLine("Render median: " + F(render.Times) + " ms");
                Console.WriteLine("Compatibility probes: " + probes.Count(item => item.Pass) + "/" + probes.Count + " passed");
                return probes.All(item => item.Pass) ? 0 : 1;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Lea report failed: " + error.Message);
                return 1;
            }
        }

        private static int StartupOnly(string root)
        {
            var path = Path.Combine(Pages(root), "index.html");
            return new E().Go(path).St == 200 ? 0 : 1;
        }

        private static double[] MeasureStartup(string root, int rounds)
        {
            var times = new double[rounds];
            var executable = Assembly.GetExecutingAssembly().Location;
            for (var i = 0; i < rounds; i++)
            {
                var start = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = "-s -r \"" + root.Replace("\"", "") + "\"",
                    WorkingDirectory = root,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var watch = Stopwatch.StartNew();
                using (var process = Process.Start(start))
                {
                    Need(process != null, "Could not start the Lea startup probe");
                    if (!process.WaitForExit(30000))
                    {
                        process.Kill();
                        throw new TimeoutException("Lea startup probe exceeded 30 seconds");
                    }
                    watch.Stop();
                    Need(process.ExitCode == 0, "Lea startup probe exited with code " + process.ExitCode);
                }
                times[i] = watch.Elapsed.TotalMilliseconds;
            }
            return times;
        }

        private static IList<string> FixturePaths(string root)
        {
            var folder = Pages(root);
            var names = new[]
            {
                "index.html", "recovery.html", "html-tokens.html", "selectors.html", "selector-functions.html", "selector-arithmetic.html", "selector-filtered-position.html", "selector-relational.html", "layout.html",
                "tables.html", "forms.html", "editing.html", "retained-data.html", "stylesheets.html", "load-lifecycle.html", "image-loading.html", "image-preflight.html", "input-loading.html", "cache-identity.html", "events-vars.html", "dom-bridge.html", "display-navigation.html", "calculated-widths.html", "calculated-heights.html", "viewport-units.html", "font-relative-units.html", "size-functions.html", "logical-sizing-insets.html", "logical-box-model.html", "logical-borders.html", "sticky-positioning.html", "responsive-media.html", "media-queries.html", "custom-properties.html", "important-cascade.html",
                "stacking-contexts.html", "external.html", "base-url.html", "class-commands.html",
                "go-command.html", "lists.html", "disclosure.html", "fragments.html", "fragment-stability.html", "gauges.html",
                "values.html", "time-edits.html", "image-maps.html", "keyboard-focus.html",
                "focus-shortcuts.html", "accessibility.html", "state-commands.html", "flex.html", "grid.html", "grid-areas.html", "grid-lines.html", "grid-flow.html", "grid-auto-tracks.html", "grid-shorthand.html", "grid-alignment.html", "grid-content-alignment.html", "svg.html", "svg-geometry.html", "svg-gradients.html", "svg-use-clips.html", "svg-strokes.html",
                "box-sizing.html", "border-radius.html", "box-shadows.html", "background-gradients.html", "background-images.html", "object-fit.html", "aspect-ratio.html", "outlines.html", "pointer-interaction.html", "text-transform.html", "text-spacing.html", "text-overflow.html"
            };
            var paths = names.Select(name => Path.Combine(folder, name)).ToList();
            foreach (var path in paths)
                if (!File.Exists(path)) throw new FileNotFoundException("Missing benchmark fixture", path);
            return paths;
        }

        private static void Warm(IEnumerable<string> fixtures)
        {
            var engine = new E();
            foreach (var path in fixtures) Need(engine.Go(path).St == 200, "Warm-up failed for " + Path.GetFileName(path));
            using (var view = NewView())
            {
                view.Go(fixtures.First());
                using (var image = Draw(view)) { }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static LoadResult MeasureLoad(string path, int iterations)
        {
            var times = new double[iterations];
            G snapshot = null;
            for (var i = 0; i < iterations; i++)
            {
                var engine = new E();
                var watch = Stopwatch.StartNew();
                var page = engine.Go(path);
                watch.Stop();
                Need(page.St == 200, "Load failed for " + Path.GetFileName(path));
                times[i] = watch.Elapsed.TotalMilliseconds;
                snapshot = engine.Dg(page);
            }

            return new LoadResult
            {
                Name = Path.GetFileName(path),
                Bytes = new FileInfo(path).Length,
                Nodes = snapshot == null ? 0 : snapshot.N,
                CssRules = snapshot == null ? 0 : snapshot.C,
                Times = times
            };
        }

        private static RenderResult MeasureRender(IList<string> fixtures, int rounds)
        {
            var times = new List<double>();
            var boxes = 0;
            var height = 0;
            var colored = 0;
            for (var round = 0; round < rounds; round++)
            {
                foreach (var path in fixtures)
                {
                    using (var view = NewView())
                    {
                        view.Go(path);
                        var watch = Stopwatch.StartNew();
                        using (var image = Draw(view))
                        {
                            watch.Stop();
                            if (round == 0) colored += ColoredSamples(image);
                        }
                        times.Add(watch.Elapsed.TotalMilliseconds);
                        boxes += view.Bc;
                        height = Math.Max(height, view.Dh);
                    }
                }
            }
            Need(boxes > 0 && colored > 0, "Rendered output was blank");
            return new RenderResult { Times = times.ToArray(), Boxes = boxes, Height = height, ColoredSamples = colored };
        }

        private static MemoryResult MeasureMemory(IList<string> fixtures, int rounds)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var before = GC.GetTotalMemory(true);
            var pages = new List<P>();
            var engine = new E();
            for (var round = 0; round < rounds; round++)
                foreach (var path in fixtures) pages.Add(engine.Go(path));
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(engine);
            GC.KeepAlive(pages);
            return new MemoryResult
            {
                Pages = pages.Count,
                Bytes = Math.Max(0, after - before)
            };
        }

        private static IList<Probe> RunProbes(string root)
        {
            var folder = Pages(root);
            var probes = new List<Probe>();
            AddProbe(probes, "HTML tree and recovery", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "recovery.html"));
                var table = engine.Go(Path.Combine(folder, "tables.html"));
                var tokens = engine.Go(Path.Combine(folder, "html-tokens.html"));
                Need(engine.Dg(page).N > 8, "document tree is incomplete");
                Need(table.Bs.Any(block => block.Tb != null), "table model is missing");
                Need(tokens.T == "Lea & Tokenizer <b>literal</b>" && tokens.Vs["markup"] == "<strong>raw</strong>", "raw-text tokenization changed");
                return "Recovered tree, structured table, raw text, RCDATA, and entities";
            });
            AddProbe(probes, "Bounded list markers", delegate
            {
                var page = new E().Go(Path.Combine(folder, "lists.html"));
                var outer = page.Bs.First(block => block.El != null && block.El.Av("id") == "ten");
                var nested = page.Bs.First(block => block.El != null && block.El.Av("id") == "minus-ten");
                var bounded = page.Bs.First(block => block.El != null && block.El.Av("id") == "max-next");
                Need(outer.Mk == "10." && nested.Mk == "-10." && bounded.Mk == "999999.", "list marker values changed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "lists.html"));
                    using (var image = Draw(view)) Need(view.Bc > page.Bs.Count && ColoredSamples(image) > 0, "list marker layout is blank");
                }
                return "Unordered, start/value, nested negative, saturated, inline, and painted markers";
            });
            AddProbe(probes, "Bounded disclosures", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "disclosure.html"));
                var details = ById(page.D, "closed");
                var summary = ById(page.D, "closed-summary");
                var content = page.Bs.First(block => block.El != null && block.El.Av("id") == "closed-content");
                var marker = page.Bs.First(block => block.El != null && block.El.Av("id") == "closed-summary");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "closed-field");
                field.V = "Retained benchmark value";
                Need(!details.Op && content.Hd && marker.Mk == ">", "closed disclosure model changed");
                Need(engine.Ds(page, summary) == 1 && details.Op, "disclosure toggle failed");
                content = page.Bs.First(block => block.El != null && block.El.Av("id") == "closed-content");
                Need(!content.Hd, "opened disclosure content stayed hidden");
                Need(field.V == "Retained benchmark value", "disclosure rebuild replaced mutable field state");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "disclosure.html"));
                    Need(view.Ds(ById(view.At.D, "closed-summary")) == 1, "view disclosure toggle failed");
                    using (var image = Draw(view)) Need(view.Bc > 0 && ColoredSamples(image) > 0, "disclosure layout is blank");
                }
                return "Nested open state, retained fields, compact markers, host/view toggles, and painted content";
            });
            AddProbe(probes, "Retained fragment navigation", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "fragments.html"));
                var requests = engine.Dg(page).Rs.Count;
                Need(engine.Ft(page, "#encoded%20target") != null, "encoded fragment lookup failed");
                Need(Object.ReferenceEquals(page, engine.Go("#target")) && engine.At.U.Fragment == "#target", "fragment navigation replaced the page");
                Need(engine.Dg(page).Rs.Count == requests, "fragment navigation performed a request");
                var hidden = engine.Ft(page, "#closed-target");
                Need(engine.Rv(page, hidden) == 1 && ById(page.D, "closed-details").Op, "hidden fragment was not revealed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "fragments.html"));
                    Need(view.Fg("#cell-target") == 1 && view.Sy > 0, "view fragment placement failed");
                    using (var image = Draw(view)) Need(ColoredSamples(image) > 0, "fragment layout is blank");
                }
                return "128-entry history, zero-fetch page retention, encoded/named targets, hidden reveal, table placement, and resize coverage";
            });
            AddProbe(probes, "Stable initial fragments", delegate
            {
                var path = Path.Combine(folder, "fragment-stability.html");
                using (var view = NewView())
                {
                    view.Size = new Size(300, 140);
                    view.Go(new Uri(new Uri(path).AbsoluteUri + "#resize-target"));
                    using (var image = Draw(view)) Need(ColoredSamples(image) > 0, "initial fragment layout is blank");
                    Need(view.Fa && view.Fj >= 2 && view.Fy >= view.Sy && view.Fy < view.Sy + view.ClientSize.Height,
                        "initial fragment anchor or diagnostics changed");
                    var narrowTarget = view.Fy;
                    var narrowJumps = view.Fj;
                    view.Size = new Size(720, 140);
                    using (var image = Draw(view)) { }
                    Need(view.Fj > narrowJumps && view.Fy < narrowTarget && view.Fy >= view.Sy && view.Fy < view.Sy + view.ClientSize.Height,
                        "initial fragment did not follow settled host geometry");
                    var settledJumps = view.Fj;
                    view.Sc(1);
                    view.Size = new Size(640, 140);
                    using (var image = Draw(view)) { }
                    Need(!view.Fa && view.Fj == settledJumps, "manual scroll did not release the initial fragment anchor");
                }
                return "Fa/Fj/Fy diagnostics, first-paint retention, responsive host settlement, and manual-scroll release";
            });
            AddProbe(probes, "Host navigation guard", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "forms.html"));
                engine.Ng = url => false;
                foreach (var action in new Action[] { () => engine.Go(page.U), () => engine.Rl(), () => engine.Fg("#blocked") })
                {
                    var rejected = false;
                    try { action(); } catch (Er error) { rejected = error.K == "blocked" && !error.Rt; }
                    Need(rejected && engine.At == page && !engine.CanBk && !engine.Busy, "navigation guard changed retained state");
                }
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "forms.html"));
                    var retained = view.At;
                    view.Ng = url => false;
                    var rejected = false;
                    try { view.GoA(Path.Combine(folder, "index.html")).GetAwaiter().GetResult(); }
                    catch (Er error) { rejected = error.K == "blocked"; }
                    Need(rejected && view.At == retained && !view.Busy, "view guard alias or async rejection failed");
                    view.Dispose();
                    Need(view.Ng == null, "disposed view retained host policy");
                }
                return "Ng policy, typed rejection, retained page/history, async view alias, and disposal; not a redirect/resource firewall";
            });
            AddProbe(probes, "Blocked native input", delegate
            {
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "focus-shortcuts.html"));
                    using (var bitmap = Draw(view)) Need(ColoredSamples(bitmap) > 0, "input fixture is blank");
                    var retained = view.At;
                    var calls = 0;
                    view.Ng = url => { calls++; return false; };
                    var keyDown = typeof(V).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var key in new[] { Keys.Alt | Keys.Shift | Keys.G, Keys.Enter })
                    {
                        var args = new KeyEventArgs(key);
                        keyDown.Invoke(view, new object[] { args });
                        Application.DoEvents();
                        Need(args.Handled && args.SuppressKeyPress, "blocked keyboard input was not consumed");
                    }
                    Need(calls == 2 && view.At == retained && view.U.Fragment == "", "blocked input changed page/history");
                    var rejected = false;
                    try { view.Ka().GetAwaiter().GetResult(); } catch (Er error) { rejected = error.K == "blocked"; }
                    Need(rejected, "public activation hid its rejection");
                }
                return "Expected Ng denials stay out of native keyboard callbacks; public Ka still reports blocked";
            });
            AddProbe(probes, "Bounded progress and meter gauges", delegate
            {
                var page = new E().Go(Path.Combine(folder, "gauges.html"));
                var progress = page.Bs.First(block => block.El != null && block.El.Av("id") == "styled-progress");
                var extreme = page.Bs.First(block => block.El != null && block.El.Av("id") == "extreme-progress");
                var worst = page.Bs.First(block => block.El != null && block.El.Av("id") == "worst-meter");
                Need(progress.Vm != null && progress.Vm.R == 0.375D && progress.S.F == "#176b4d", "progress model or inherited style changed");
                Need(extreme.Vm.Mx == 1000000000000D && extreme.Vm.V == extreme.Vm.Mx, "extreme progress value is unbounded");
                Need(worst.Vm.Q == 2 && worst.Vm.R == 0.9D, "meter quality model changed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "gauges.html"));
                    using (var image = Draw(view)) Need(view.Bc > page.Bs.Count && ColoredSamples(image) > 0, "gauge layout is blank");
                }
                return "Vm aliases, determinate/indeterminate progress, ordered meter regions, 10^12 numeric ceiling, inherited style, tables, resize, and paint";
            });
            AddProbe(probes, "Bounded output and data values", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "values.html"));
                var output = page.Dvs.First(value => value.El.Av("id") == "total-output");
                var data = page.Dvs.First(value => value.El.Av("id") == "inline-data");
                var cell = page.Dvs.First(value => value.El.Av("id") == "cell-data");
                Need(page.Dvs.Count == 7 && output.K == "o" && output.V == "42 points", "output value collection changed");
                Need(output.N == "total" && output.Fo == "calc" && output.F.SequenceEqual(new[] { "price", "qty" }), "output ownership aliases changed");
                Need(data.K == "d" && data.V == "lea-97" && data.X == "Lea compact", "data value aliases changed");
                Need(cell.El.Up != null && cell.El.Up.K == "td", "table data ownership changed");
                Need(page.Bs.SelectMany(block => block.Is).Any(run => Object.ReferenceEquals(run.Dv, data)), "inline data run is missing");
                Need(page.Dvs.First(value => value.El.Av("id") == "many-refs").F.Count == 32, "output reference bound changed");
                Need(engine.Ck(page, ById(page.D, "update-output")) == 1 && page.Dvs.First(value => value.El.Av("id") == "live-output").V == "Updated total", "safe output rebuild failed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "values.html"));
                    using (var image = Draw(view)) Need(view.Bc > 0 && ColoredSamples(image) > 0, "value layout is blank");
                    view.Size = new Size(150, 720);
                    using (var image = Draw(view)) Need(view.At.Dvs.Count == 7 && view.Bc > 0, "value resize path changed");
                }
                return "Dv aliases, 4096-char values, 32 unique references, inline/block/table ownership, 512-char updates, rebuilds, and resize";
            });
            AddProbe(probes, "Bounded time and edit metadata", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "time-edits.html"));
                var release = page.Dvs.First(value => value.El.Av("id") == "release-time");
                var inserted = page.Dvs.First(value => value.El.Av("id") == "inline-ins");
                var removed = page.Dvs.First(value => value.El.Av("id") == "inline-del");
                Need(page.Dvs.Count == 10 && release.K == "t" && release.V == "2026-08-21T19:00-04:00" && release.Dt == release.V, "time aliases changed");
                Need(inserted.K == "i" && inserted.U == new Uri(page.Bu, "go-target.html#change") && inserted.Dt == "2026-08-21T19:05-04:00", "inserted metadata changed");
                Need(removed.K == "r" && removed.U == null, "unsafe removed citation was accepted");
                Need(page.Bs.SelectMany(block => block.Is).Any(run => Object.ReferenceEquals(run.Dv, inserted) && run.S.Td.IndexOf('u') >= 0), "inserted inline ownership or decoration changed");
                Need(page.Bs.SelectMany(block => block.Is).Any(run => Object.ReferenceEquals(run.Dv, removed) && run.S.Td.IndexOf('s') >= 0), "removed inline ownership or decoration changed");
                Need(engine.Ck(page, ById(page.D, "update-live")) == 1 && page.Dvs.First(value => value.El.Av("id") == "live-time").Dt == "2026-08-22", "text time rebuild failed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "time-edits.html"));
                    using (var image = Draw(view)) Need(view.Bc > 0 && ColoredSamples(image) > 0, "time and edit layout is blank");
                    view.Size = new Size(160, 800);
                    using (var image = Draw(view)) Need(view.At.Dvs.Count == 10 && view.Bc > 0, "time and edit resize path changed");
                }
                return "Dv t/i/r aliases, 128-char datetimes, safe resolved citations, unsafe-scheme rejection, native decoration, ownership, rebuilds, tables, and resize";
            });
            AddProbe(probes, "Bounded responsive image maps", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "image-maps.html"));
                var map = page.Mps.First();
                var image = page.Bs.First(block => block.El != null && block.El.Av("id") == "mapped");
                Need(page.Mps.Count == 2 && map.N == "LeaMap" && map.As.Count == 7, "image map collection changed");
                Need(map.As[0].U == null && map.As[1].K == "r" && map.As[1].Cs.SequenceEqual(new[] { 0D, 0D, 100D, 80D }), "rectangle or dead area model changed");
                Need(map.As[2].K == "c" && map.As[2].Cs.Count == 3 && map.As[3].K == "p" && map.As[3].Cs.Count == 8, "circle or polygon model changed");
                Need(map.As[4].Cs.Count == 0 && map.As[5].Cs.Count == 0 && map.As[6].K == "d", "empty/default area model changed");
                Need(Object.ReferenceEquals(image.Mp, map) && image.Mw == 320 && image.Mh == 160 && page.Ls.Count == 6, "image map ownership or links changed");
                Need(engine.Ck(page, ById(page.D, "style-map")) == 1 && page.Mps.Count == 2 && page.Bs.First(block => block.El != null && block.El.Av("id") == "mapped").S.W == 240, "image map rebuild failed");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "image-maps.html"));
                    using (var bitmap = Draw(view)) Need(view.Bc > 0 && ColoredSamples(bitmap) > 0, "image map layout is blank");
                    view.Size = new Size(180, 800);
                    using (var bitmap = Draw(view)) Need(view.At.Mps.Count == 2 && view.Bc > 0, "image map resize path changed");
                }
                return "Mp/Ar aliases, rect/circle/poly/default and dead areas, 32 maps, 64 areas, 128 coordinates, responsive scaling, rebuilds, and paint";
            });
            AddProbe(probes, "Bounded keyboard focus", delegate
            {
                var path = Path.Combine(folder, "keyboard-focus.html");
                var engine = new E();
                var page = engine.Go(path);
                Need(engine.Ti(ById(page.D, "priority-one")) == 1 && engine.Ti(ById(page.D, "negative-link")) == -1 && engine.Ti(ById(page.D, "invalid-link")) == null, "tabindex parsing changed");
                using (var view = NewView())
                {
                    view.Size = new Size(520, 280);
                    view.Go(path);
                    using (var bitmap = Draw(view)) Need(view.Kn == 14 && view.Ki == 0 && view.Ke == null, "keyboard target collection changed");
                    Need(view.Kf() == 1 && view.Ke.Av("id") == "priority-one" && !view.Kb.IsEmpty, "positive tabindex ordering changed");
                    Need(view.Kf() == 2 && view.Ke.Av("id") == "priority-area" && view.Kb.Size == new Size(100, 100), "image-map keyboard focus changed");
                    Need(view.Kf() == 3 && view.Ke.Av("id") == "priority-area" && view.Kb.Size == new Size(50, 50), "shared image-map keyboard slot changed");
                    Need(view.Ka().GetAwaiter().GetResult() == 1 && view.U.Fragment == "#area-target", "image-map keyboard activation changed");
                    view.Go(path);
                    for (var index = 0; index < 14; index++) view.Kf();
                    Need(view.Ke.Av("id") == "last-link" && view.Sy > 0 && !view.Kb.IsEmpty, "off-screen keyboard scrolling changed");
                    Need(view.Kf() == 1 && view.Ke.Av("id") == "priority-one", "keyboard wrapping changed");
                }
                return "Ti and Ke/Kn/Ki/Kb/Kf/Ka aliases, positive/zero/default/negative order, links, areas, fields, summaries, focus rings, scrolling, and 256-target bounds";
            });
            AddProbe(probes, "Bounded focus shortcuts", delegate
            {
                var path = Path.Combine(folder, "focus-shortcuts.html");
                var engine = new E();
                var page = engine.Go(path);
                var auto = ById(page.D, "auto-field");
                Need(engine.Afs(page).Count == 3 && engine.Af(page).Av("id") == "invalid-auto", "autofocus candidate model changed");
                Need(engine.Ak(auto) == "S" && engine.Aks(auto).SequenceEqual(new[] { "S", "1" }), "access-key alternatives changed");
                using (var view = NewView())
                {
                    view.Size = new Size(520, 300);
                    view.Go(path);
                    using (var bitmap = Draw(view)) Need(view.Ke != null && view.Ke.Av("id") == "auto-field" && !view.Kb.IsEmpty, "rendered autofocus selection changed");
                    Need(view.An == 6 && view.Ak(ById(view.At.D, "fallback-button")) == "X" && view.Kl(ById(view.At.D, "run-button")) == "Alt+Shift+B", "assigned access-key model changed");
                    Need(view.Aa("b").GetAwaiter().GetResult() == 1 && ById(view.At.D, "shortcut-state").C[0].X == "button shortcut activated", "button access-key activation changed");
                    Need(view.Aa("r").GetAwaiter().GetResult() == 1 && view.Ke.Av("id") == "focus-region" && view.Ki == 0, "focus-only access target changed");
                    Need(view.Aa("g").GetAwaiter().GetResult() == 1 && view.U.Fragment == "#target", "link access-key activation changed");
                    view.Go(new UriBuilder(new Uri(path)) { Fragment = "target" }.Uri);
                    using (var bitmap = Draw(view)) Need(view.Ke == null && view.Sy > 0, "fragment autofocus suppression changed");
                }
                return "Af/Afs and Ak/Aks/An/Kl/Aa aliases, first usable autofocus, Alt+Shift assignment, conflicts, exclusions, focus, activation, and fragment suppression";
            });
            AddProbe(probes, "Bounded semantic accessibility", delegate
            {
                var path = Path.Combine(folder, "accessibility.html");
                var engine = new E();
                var page = engine.Go(path);
                var user = ById(page.D, "user-name");
                var semantic = engine.Ax(page, user);
                Need(page.Axs.Count > 20 && page.Axs.Count <= 512 && Object.ReferenceEquals(page.Axs, engine.Axs(page)), "bounded accessibility list changed");
                Need(semantic != null && semantic.K == "textbox" && semantic.N == "Display name" && semantic.D == "Shown beside your work." && semantic.Is(Ax.Rq), "accessible name, description, role, or state changed");
                Need(engine.Ax(page, ById(page.D, "secret")).V.Length == 0 && engine.Ax(page, ById(page.D, "custom")).Is(Ax.Mx), "semantic redaction or ARIA state changed");
                using (var view = NewView())
                {
                    view.Size = new Size(760, 560);
                    view.Go(path);
                    using (var bitmap = Draw(view)) Need(!view.Ab(view.Ax(ById(view.At.D, "user-name"))).IsEmpty, "semantic view bounds changed");
                    Need(view.Ao != null && view.Ao.Role == AccessibleRole.Document && view.Ao.GetChildCount() > 0, "WinForms accessibility root changed");
                    var automation = AutomationElement.FromHandle(view.Handle);
                    Need(automation.Current.ControlType == ControlType.Document && automation.Current.FrameworkId == "Lea", "UI Automation document provider changed");
                    var automationUser = automation.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "user-name"));
                    object valuePattern;
                    Need(automationUser != null && automationUser.Current.ControlType == ControlType.Edit && automationUser.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern), "UI Automation field or value pattern changed");
                    var automationProgress = automation.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "load-progress"));
                    object rangePattern;
                    Need(automationProgress != null && automationProgress.TryGetCurrentPattern(RangeValuePattern.Pattern, out rangePattern) && ((RangeValuePattern)rangePattern).Current.IsReadOnly, "UI Automation range pattern changed");
                    var automationTable = automation.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "people"));
                    object gridPattern = null;
                    object tablePattern = null;
                    Need(automationTable != null && automationTable.TryGetCurrentPattern(GridPattern.Pattern, out gridPattern) && automationTable.TryGetCurrentPattern(TablePattern.Pattern, out tablePattern), "UI Automation table patterns changed");
                    var grid = (GridPattern)gridPattern;
                    var table = (TablePattern)tablePattern;
                    Need(grid.Current.RowCount == 3 && grid.Current.ColumnCount == 2 && grid.GetItem(2, 1).Current.AutomationId == "people-note", "UI Automation table grid changed");
                    Need(table.Current.RowOrColumnMajor == RowOrColumnMajor.RowMajor && table.Current.GetColumnHeaders().Length == 2 && table.Current.GetRowHeaders().Length == 1, "UI Automation table headers changed");
                    object textPattern;
                    Need(automation.TryGetCurrentPattern(TextPattern.Pattern, out textPattern), "UI Automation text pattern changed");
                    var document = ((TextPattern)textPattern).DocumentRange;
                    var documentText = document.GetText(-1);
                    Need(documentText.Contains("Lea semantic fixture") && documentText.Contains("Shared note") && documentText.Contains("Hidden action") && !documentText.Contains("do-not-expose"), "UI Automation logical text or redaction changed");
                    Need((bool)document.GetAttributeValue(TextPattern.IsReadOnlyAttribute) && document.FindText("shared NOTE", false, true).GetText(-1) == "Shared note", "UI Automation text attributes or search changed");
                    var intro = automation.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "intro"));
                    var introRange = ((TextPattern)textPattern).RangeFromChild(intro);
                    var visualLine = introRange.Clone();
                    visualLine.ExpandToEnclosingUnit(TextUnit.Line);
                    Need(introRange.GetBoundingRectangles().Length == 2 && visualLine.GetBoundingRectangles().Length == 1 &&
                        visualLine.Move(TextUnit.Line, 1) == 1 && visualLine.GetBoundingRectangles().Length == 1, "UI Automation visual line geometry changed");
                }
                return "Ax/Axs/St/Is and V.Ax/Axs/Ab/Ao aliases, 512 records, names, roles, live protected state, bounds, MSAA children, UIA fragments, control types, grid/table coordinates, headers, logical text, visual lines, search, redaction, and patterns";
            });
            AddProbe(probes, "CSS selectors and cascade", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "selectors.html"));
                var snapshot = engine.Dg(page);
                Need(snapshot.C > 0 && page.Bs.Count > 0 && page.S != null, "compiled styles are missing");
                return snapshot.C + " compiled rules across " + snapshot.B + " blocks";
            });
            AddProbe(probes, "Bounded selector functions", delegate
            {
                var path = Path.Combine(folder, "selector-functions.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "card");
                var where = page.Bs.First(item => item.El != null && item.El.Av("id") == "where-target");
                var adjacent = page.Bs.First(item => item.El != null && item.El.Av("id") == "adjacent");
                var field = page.Fs.Single(item => item.El != null && item.El.Av("id") == "field");
                field.V = "Retained benchmark selector";
                Need(block.S.F == "#0f4c81" && where.S.F == "#176b4d" && where.S.Pl == 9, "is maximum or where zero specificity changed");
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "keep").S.B &&
                    !page.Bs.First(item => item.El != null && item.El.Av("id") == "skip").S.B && adjacent.S.W == 120 && adjacent.S.Ml == 17,
                    "not alternatives or outer combinators changed");
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "forgiving").S.W == 144 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "forgiving").S.H == 44, "is or where forgiving alternatives changed");
                Need(engine.Qa(page, ":is(.card,.note)").Count == 3 && engine.Qa(page, ".item:not(.skip,[data-off])").Count == 1 &&
                    engine.Qa(page, ":is(.a > .b,.c)").Count == 1 && engine.Qa(page, ":not(.a,,.c)").Count == 0 &&
                    engine.Qa(page, ":is(.a,.b,.c,.d,.e,.f,.g,.h,.i)").Count == 0, "functional document queries, forgiving behavior, or bounds changed");
                engine.Dy(page, ById(page.D, "hover"), null);
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "hover").S.Pl == 19, "functional hover selector changed");
                engine.Dy(page, null, ById(page.D, "field"));
                Need(field.S.Bl == 5 && field.S.Pl == 12 && Object.ReferenceEquals(field, page.Fs.Single(item => item.El != null && item.El.Av("id") == "field")) && field.V == "Retained benchmark selector", "functional focus rebuild replaced field state");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var paint = Draw(view)) Need(BlockContent(view, "adjacent").Width == 120 && BlockContent(view, "where-target").Width > 0, "functional selector view geometry changed");
                }
                return "eight alternatives, sixteen tests, one-level compound arguments, forgiving is/where, strict not, maximum/zero specificity, outer combinators, queries, dynamic state, and retained fields";
            });
            AddProbe(probes, "Bounded structural arithmetic", delegate
            {
                var path = Path.Combine(folder, "selector-arithmetic.html");
                var engine = new E();
                var page = engine.Go(path);
                var one = page.Bs.First(item => item.El != null && item.El.Av("id") == "a1");
                var five = page.Bs.First(item => item.El != null && item.El.Av("id") == "a5");
                var seven = page.Bs.First(item => item.El != null && item.El.Av("id") == "a7");
                var invalid = page.Bs.First(item => item.El != null && item.El.Av("id") == "invalid");
                Need(one.S.F == "#0f4c81" && one.S.Bg == "#edf7f2" && one.S.B && one.S.H == 31 && one.S.Ml == 11,
                    "positive, negative, reverse, specificity, or selector-list structural matching changed");
                Need(five.S.I && five.S.B && seven.S.Pl == 13 && page.Bs.First(item => item.El != null && item.El.Av("id") == "a8").S.Pl == 13,
                    "unit or reverse structural ranges changed");
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "m2").S.Bl == 4 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "m2").S.W == 142 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "m4").S.W == 142, "type structural formulas changed");
                Need(invalid.S.W == 73 && invalid.S.H == 19 && invalid.S.Pl == 3 && invalid.S.Bl == 2 && invalid.S.Bg == "", "invalid structural expressions escaped bounds");
                Need(engine.Qa(page, "#forward > p:nth-child(3n+1)").Count == 3 &&
                    engine.Qa(page, "#forward > p:nth-child(-n+3)").Count == 3 &&
                    engine.Qa(page, "#forward > p:nth-last-child(2n)").Count == 4 &&
                    engine.Qa(page, "#mixed > div:nth-last-of-type(2n+1)").Count == 2 &&
                    engine.Qa(page, "#forward > :is(p:nth-child(4n+1),.missing)").Count == 2, "structural document queries changed");
                Need(engine.Qa(page, "#forward > p:nth-child(8192n+1)").Count == 1 &&
                    engine.Qa(page, "#forward > p:nth-child(8193n+1)").Count == 0 &&
                    engine.Qa(page, "#forward > p:nth-child(2n+-1)").Count == 0 &&
                    engine.Qa(page, "#forward > p:nth-child(2n of .rank)").Count == 4, "structural query limits or filtered compatibility changed");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var paint = Draw(view)) Need(BlockContent(view, "a1").Width == 96 && BlockContent(view, "a1").Height == 31 && BlockContent(view, "m2").Width == 142, "structural formula view geometry changed");
                }
                return "precompiled A/B integers, positive/negative/zero formulas, forward/reverse child and type positions, 32-character and 8192-value bounds, specificity, queries, selector-list composition, and geometry";
            });
            AddProbe(probes, "Bounded filtered positions", delegate
            {
                var path = Path.Combine(folder, "selector-filtered-position.html");
                var engine = new E();
                var page = engine.Go(path);
                var one = page.Bs.First(item => item.El != null && item.El.Av("id") == "f1");
                var five = page.Bs.First(item => item.El != null && item.El.Av("id") == "f5");
                var seven = page.Bs.First(item => item.El != null && item.El.Av("id") == "f7");
                var eight = page.Bs.First(item => item.El != null && item.El.Av("id") == "f8");
                var invalid = page.Bs.First(item => item.El != null && item.El.Av("id") == "invalid");
                Need(one.S.Bg == "#edf7f2" && five.S.Bg == "#edf7f2" && eight.S.Bg == "#edf7f2" &&
                    seven.S.B && eight.S.B && page.Bs.First(item => item.El != null && item.El.Av("id") == "f3").S.Bl == 4 && five.S.Pl == 13,
                    "filtered forward, reverse, list, or compound matching changed");
                Need(one.S.F == "#0f4c81" && page.Bs.First(item => item.El != null && item.El.Av("id") == "f4").S.F == "#0f4c81" &&
                    seven.S.F == "#0f4c81" && page.Bs.First(item => item.El != null && item.El.Av("id") == "specific").S.H == 33, "filtered reverse parity or specificity changed");
                Need(page.Fs.First(item => item.El != null && item.El.Av("id") == "e2").S.Bg == "#dff6e9" &&
                    page.Fs.First(item => item.El != null && item.El.Av("id") == "e2").S.B && page.Fs.First(item => item.El != null && item.El.Av("id") == "e3").S.Bl == 5,
                    "filtered form-state matching changed");
                Need(invalid.S.W == 73 && invalid.S.H == 19 && invalid.S.Pl == 3 && invalid.S.Bl == 2 && invalid.S.Br == 0 && invalid.S.Mr == 0 && invalid.S.Bg == "",
                    "invalid filtered selectors escaped bounds");
                Need(engine.Qa(page, "#items>p:nth-child(2n+1 of .pick)").Count == 3 &&
                    engine.Qa(page, "#items>.pick:nth-child(-n+3)").Count == 2 &&
                    engine.Qa(page, "#items>p:nth-child(-n+3 of .pick)").Count == 3 &&
                    engine.Qa(page, "#items>p:nth-last-child(2n of .pick,[data-alt])").Count == 3 &&
                    engine.Qa(page, "#forms>input:nth-child(2 of :enabled)").Count == 1, "filtered document queries changed");
                Need(engine.Qa(page, "#invalid:nth-child(n of #invalid,.a,.b,.c,.d,.e,.f,.g):nth-last-child(n of #invalid,.h,.i,.j,.k,.l,.m,.o)").Count == 1 &&
                    engine.Qa(page, "#invalid:nth-child(n of #invalid,.a,.b,.c,.d,.e,.f,.g):nth-last-child(n of #invalid,.h,.i,.j,.k,.l,.m,.o):nth-child(n of #invalid)").Count == 0 &&
                    engine.Qa(page, "#invalid:nth-child(1 of .pick,)").Count == 0 && engine.Qa(page, "#invalid:nth-of-type(1 of #invalid)").Count == 0,
                    "filtered strictness, total tests, or typed rejection changed");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var paint = Draw(view)) Need(BlockContent(view, "f1").Width == 108 && BlockContent(view, "specific").Height == 33, "filtered selector view geometry changed");
                }
                return "strict compound of-S lists, eight alternatives, sixteen total tests, maximum filter specificity, forward/reverse counting, form-state filters, queries, invalid nesting/type rejection, and geometry";
            });
            AddProbe(probes, "Bounded relational selectors", delegate
            {
                var path = Path.Combine(folder, "selector-relational.html");
                var engine = new E();
                var page = engine.Go(path);
                var direct = page.Bs.First(item => item.El != null && item.El.Av("id") == "direct");
                var descendant = page.Bs.First(item => item.El != null && item.El.Av("id") == "descendant");
                var invalid = page.Bs.First(item => item.El != null && item.El.Av("id") == "invalid");
                Need(direct.S.W == 118 && direct.S.Bg == "#edf7f2" && direct.S.B && descendant.S.Bl == 4,
                    "relational child, descendant, list, or compound matching changed");
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "adjacent").S.H == 31 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "following").S.Pl == 11 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "complex").S.Br == 6 &&
                    page.Bs.First(item => item.El != null && item.El.Av("id") == "specific").S.H == 37,
                    "relational sibling, complex-chain, or specificity matching changed");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "focus-input");
                field.V = "Retained relational benchmark";
                engine.Dy(page, null, field.El);
                Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "focus-card").S.Bg == "#dff6e9" &&
                    Object.ReferenceEquals(field, page.Fs.First(item => item.El != null && item.El.Av("id") == "focus-input")) && field.V == "Retained relational benchmark",
                    "relational focus rebuild or field retention changed");
                Need(engine.Qa(page, ".card:has(> img.hero)").Count == 1 &&
                    engine.Qa(page, ".card:has(.body .ready[data-state=go])").Count == 1 &&
                    engine.Qa(page, "#adjacent:has(+ dt)").Count == 1 &&
                    engine.Qa(page, "#following:has(~ li.end)").Count == 1 &&
                    engine.Qa(page, "section:has(> .body > .row + .ready)").Count == 1,
                    "relational document queries changed");
                Need(engine.Qa(page, "#direct:has(> img,.a,.b,.c,.d,.e,.f,.g)").Count == 1 &&
                    engine.Qa(page, "#complex:has(> .body .ready,.a .b,.c .d,.e .f,.g .h,.i .j,.k .l,.m .n)").Count == 1 &&
                    engine.Qa(page, "#invalid:has(.ready,)").Count == 0 && engine.Qa(page, "#invalid:has(:has(.ready))").Count == 0 &&
                    engine.Qa(page, "#invalid:has(> .a .b .c .d .e)").Count == 0,
                    "relational strictness, nesting, or compile limits changed");
                Need(invalid.S.W == 92 && invalid.S.H == 21 && invalid.S.Pl == 3 && invalid.S.Bl == 0 && invalid.S.Br == 0 && invalid.S.Ml == 0 && invalid.S.Mr == 0,
                    "invalid relational selectors escaped bounds");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var paint = Draw(view)) Need(BlockContent(view, "direct").Width == 118 && BlockContent(view, "specific").Height == 37, "relational selector view geometry changed");
                }
                return "strict precompiled relative lists, descendant/child/adjacent/following relationships, four-part chains, eight alternatives, sixteen total parts, 4096 candidate visits, maximum specificity, dynamic focus, queries, retained fields, and geometry";
            });
            AddProbe(probes, "Bounded media queries", delegate
            {
                var path = Path.Combine(folder, "media-queries.html");
                var engine = new E();
                var page = engine.Go(path);
                var field = page.Fs.Single(item => item.El != null && item.El.Av("id") == "field");
                field.V = "Retained benchmark value";
                Need(engine.Vw == 1024 && page.Vw == 1024 && engine.Dg(page).C == 16, "default responsive state changed");
                Need(page.Bs.Any(block => block.El != null && block.El.Av("id") == "wide") &&
                    !page.Bs.Any(block => block.El != null && block.El.Av("id") == "narrow"), "wide media selection changed");
                engine.Vw = 480;
                Need(page.Vw == 480 && engine.Dg(page).C == 16 &&
                    page.Bs.Any(block => block.El != null && block.El.Av("id") == "narrow") &&
                    page.Bs.Any(block => block.El != null && block.El.Av("id") == "nested") &&
                    page.Bs.Any(block => block.El != null && block.El.Av("id") == "exact") &&
                    !page.Bs.Any(block => block.El != null && block.El.Av("id") == "wide"), "narrow media selection changed");
                Need(Object.ReferenceEquals(field, page.Fs.Single(item => item.El != null && item.El.Av("id") == "field")) &&
                    field.V == "Retained benchmark value" && field.S.W == 140, "responsive rebuild replaced field state");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var widePaint = Draw(view)) Need(view.Vw == 840 && BlockContent(view, "wide").Width == 320, "wide responsive view changed");
                    var liveField = view.At.Fs.Single(item => item.El != null && item.El.Av("id") == "field");
                    liveField.V = "Retained view value";
                    view.Size = new Size(480, 760);
                    using (var narrowPaint = Draw(view)) Need(view.Vw == 480 && BlockContent(view, "narrow").Width == 180, "narrow responsive view changed");
                    Need(Object.ReferenceEquals(liveField, view.At.Fs.Single(item => item.El != null && item.El.Av("id") == "field")) && liveField.V == "Retained view value", "responsive view replaced field state");
                }
                return "Vw/P.Vw/G.Vw aliases, min/max/exact/comma/nested screen queries, style media, 1-8192 px bounds, live resize, diagnostics, and retained fields";
            });
            AddProbe(probes, "Bounded CSS custom properties", delegate
            {
                var path = Path.Combine(folder, "custom-properties.html");
                var engine = new E();
                var page = engine.Go(path);
                var field = page.Fs.Single(item => item.El != null && item.El.Av("id") == "var-field");
                field.V = "Retained benchmark variable";
                Need(page.S.Vc == 19 && page.S.Cv("--brand") == "#176b4d" && page.S.Cv("--Brand") == "#123f8c", "custom property aliases or case sensitivity changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "base").S.W == 260 &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "child").S.F == "#7e22ce" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "depth-ok").S.W == 177 &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "depth-limit").S.W == 43, "custom inheritance, values, or depth bounds changed");
                engine.Dy(page, ById(page.D, "hover"), null);
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "hover").S.Pl == 18, "custom hover cascade changed");
                engine.Vw = 480;
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "base").S.W == 150 &&
                    Object.ReferenceEquals(field, page.Fs.Single(item => item.El != null && item.El.Av("id") == "var-field")) && field.V == "Retained benchmark variable", "responsive custom value or retained field changed");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var widePaint = Draw(view)) Need(BlockContent(view, "base").Width == 260, "custom property wide geometry changed");
                    view.Size = new Size(480, 760);
                    using (var narrowPaint = Draw(view)) Need(BlockContent(view, "base").Width == 150, "custom property responsive geometry changed");
                }
                return "S.Vc/Cv aliases, 32 properties, 64-character names, 512-character values, eight-level resolution, case-sensitive inheritance, nested/cycle fallbacks, inline/hover/media rebuilds, and retained fields";
            });
            AddProbe(probes, "Bounded important cascade", delegate
            {
                var path = Path.Combine(folder, "important-cascade.html");
                var engine = new E();
                var page = engine.Go(path);
                var sheet = page.Bs.First(block => block.El != null && block.El.Av("id") == "sheet");
                var inline = page.Bs.First(block => block.El != null && block.El.Av("id") == "inline");
                var field = page.Fs.Single(item => item.El != null && item.El.Av("id") == "field");
                field.V = "Retained benchmark priority";
                Need(engine.Dg(page).Ic == 19 && sheet.S.F == "#a13b18" && sheet.S.W == 140 && sheet.S.Bg == "#7e22ce" &&
                    inline.S.F == "#0f4c81" && inline.S.W == 190 && inline.S.Bg == "#176b4d", "important stylesheet or inline tier changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "specific").S.Pl == 9 &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "order").S.W == 91 &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "custom-child").S.F == "#123f8c" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "fallback").S.W == 133, "important specificity, order, inheritance, or fallback changed");
                engine.Dy(page, ById(page.D, "hover"), null);
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "hover").S.W == 180, "important hover cascade changed");
                engine.Dy(page, null, null);
                engine.Vw = 480;
                Need(engine.Dg(page).Ic == 22 && page.Bs.First(block => block.El != null && block.El.Av("id") == "media").S.W == 115 &&
                    Object.ReferenceEquals(field, page.Fs.Single(item => item.El != null && item.El.Av("id") == "field")) && field.V == "Retained benchmark priority" && field.S.W == 150,
                    "important media cascade or retained field changed");
                using (var view = NewView())
                {
                    view.Size = new Size(840, 760);
                    view.Go(path);
                    using (var widePaint = Draw(view)) Need(BlockContent(view, "sheet").Width == 140 && BlockContent(view, "inline").Width == 190, "important wide view geometry changed");
                    var liveField = view.At.Fs.Single(item => item.El != null && item.El.Av("id") == "field");
                    liveField.V = "Retained important view";
                    view.Size = new Size(480, 760);
                    using (var narrowPaint = Draw(view)) Need(BlockContent(view, "media").Width == 115, "important narrow view geometry changed");
                    Need(Object.ReferenceEquals(liveField, view.At.Fs.Single(item => item.El != null && item.El.Av("id") == "field")) && liveField.V == "Retained important view" && liveField.S.W == 150, "important view replaced field state");
                }
                return "G.Ic diagnostics, stylesheet/inline priority tiers, specificity/source order, shorthands, custom properties, nested marker boundaries, hover/media rebuilds, and retained fields";
            });
            AddProbe(probes, "Bounded document queries", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "selectors.html"));
                Need(engine.Id(page, "stage") != null, "Id lookup failed");
                Need(engine.Q(page, "#stage > p.direct") != null, "first query failed");
                Need(engine.Qa(page, ".card").Count == 3, "all query count changed");
                Need(engine.Qa(page, "p,,div").Count == 0, "invalid query was accepted");
                return "Id and first match; 3 ordered matches; invalid selector rejected";
            });
            AddProbe(probes, "Document base URLs", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "base-url.html"));
                var expected = new Uri(page.U, "../");
                Need(page.Bu == expected && page.U != page.Bu, "document base URL changed");
                Need(page.S.Bg == "#edf7f2", "base-relative stylesheet is missing");
                Need(page.Ls.Count == 2 && page.Ls.All(link => link.U.IsFile), "base-relative links changed");
                Need(page.Fs.Any(field => field.U == new Uri(expected, "test-pages/form-result.html")), "base-relative form action changed");
                return "First valid Bu resolves styles, links, images, forms, and downloads while U stays unchanged";
            });
            AddProbe(probes, "Layout and painting", delegate
            {
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "stacking-contexts.html"));
                    using (var image = Draw(view))
                    {
                        var colored = ColoredSamples(image);
                        Need(view.Bc > 0 && view.Dh > 0 && colored > 0, "painted view is blank");
                        return view.Bc + " boxes, " + view.Dh + " px document, " + colored + " colored samples";
                    }
                }
            });
            AddProbe(probes, "Bounded box sizing", delegate
            {
                var path = Path.Combine(folder, "box-sizing.html");
                var page = new E().Go(path);
                Need(ById(page.D, "content-box") != null && page.Bs.First(block => block.El != null && block.El.Av("id") == "content-box").S.Bz == "c", "content-box model changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "border-box").S.Bz == "b" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "flex-content").S.Bz == "c" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "flex-border").S.Fb == 140, "border-box cascade, inheritance, or flex basis changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1200);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockOuter(view, "content-box").Size == new Size(232, 78) && BlockOuter(view, "border-box").Size == new Size(200, 50), "content/border box geometry changed");
                        Need(BlockOuter(view, "border-min").Size == new Size(180, 42) && BlockOuter(view, "border-max").Size == new Size(170, 44), "border-box min/max geometry changed");
                        Need(BlockOuter(view, "border-field").Size == new Size(180, 44) && BlockOuter(view, "border-image").Size == new Size(100, 60) && BlockOuter(view, "flex-content").Width == 162 && BlockOuter(view, "flex-border").Width == 140 && BlockOuter(view, "border-table").Size == new Size(240, 100), "field, image, flex, or table box sizing changed");
                    }
                    view.Size = new Size(180, 1200);
                    using (var image = Draw(view))
                    {
                        var border = BlockOuter(view, "border-box");
                        var flex = BlockContent(view, "border-flex");
                        var item = BlockOuter(view, "flex-border");
                        Need(border.Width > 32 && border.Width < 200 && BlockOuter(view, "border-field").Width == border.Width && BlockOuter(view, "border-table").Width == border.Width, "narrow border-box reflow changed");
                        Need(item.Width > 22 && item.Right <= flex.Right, "narrow border-box flex containment changed");
                    }
                }
                return "Bz c/b across width, height, min/max, flex-basis, fields, tables, and 180 px reflow with fixture-backed paint and hits";
            });
            AddProbe(probes, "Bounded CSS corner radii", delegate
            {
                var path = Path.Combine(folder, "border-radius.html");
                var page = new E().Go(path);
                var solid = page.Bs.First(block => block.El != null && block.El.Av("id") == "solid");
                var dashed = page.Bs.First(block => block.El != null && block.El.Av("id") == "dashed");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                Need(solid.S.R0 == 30 && solid.S.R1 == 12 && solid.S.Q0 == 20 && solid.S.Q2 == 30, "elliptical radius aliases changed");
                Need(dashed.S.R0 == -50 && dashed.S.R1 == -25 && dashed.S.Q0 == 18 && dashed.S.Q1 == 10, "percentage radius aliases changed");
                Need(atomic.S.R0 == 1 && atomic.S.R3 == 4 && atomic.S.Q0 == 5 && atomic.S.Q3 == 8, "atomic invalid radius fallback changed");
                Need(field.S.R0 == 18 && field.S.R1 == 36 && field.S.Q2 == 18 && field.S.Q3 == 36, "custom-property field radius changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1200);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var solidOuter = BlockOuter(view, "solid");
                        var clipContent = BlockContent(view, "clipper");
                        Need(ColorDistance(image.GetPixel(solidOuter.Left, solidOuter.Top), Color.White) < 24 &&
                            ColorDistance(image.GetPixel(solidOuter.Left + solidOuter.Width / 2, solidOuter.Top + 2), Color.FromArgb(22, 101, 52)) < 24, "rounded background or solid border paint changed");
                        Need(ColorDistance(image.GetPixel(clipContent.Left + 1, clipContent.Top + 1), Color.White) < 24 &&
                            ColorDistance(image.GetPixel(clipContent.Left + clipContent.Width / 2, clipContent.Top + clipContent.Height / 2), Color.FromArgb(239, 68, 68)) < 12, "rounded overflow clip changed");
                    }
                    view.Size = new Size(300, 1200);
                    using (var image = Draw(view)) Need(BlockOuter(view, "dashed").Right <= view.ClientSize.Width - 1, "rounded narrow reflow changed");
                }
                return "S.R0-R3/Q0-Q3 aliases, atomic pixel/percentage ellipses, longhands, variables, important/reset/bounds, normalized solid/dashed paint, overflow clips, fields, links, and resize";
            });
            AddProbe(probes, "Bounded CSS box shadows", delegate
            {
                var path = Path.Combine(folder, "box-shadows.html");
                var page = new E().Go(path);
                var outer = page.Bs.First(block => block.El != null && block.El.Av("id") == "outer");
                var soft = page.Bs.First(block => block.El != null && block.El.Av("id") == "soft");
                var inset = page.Bs.First(block => block.El != null && block.El.Av("id") == "inset");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var clipChild = page.Bs.First(block => block.El != null && block.El.Av("id") == "shadow-clip-child");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                Need(outer.S.Sw.Count == 2 && outer.S.Sw[0].X == 12 && outer.S.Sw[1].X == -8, "outer shadow aliases changed");
                Need(soft.S.Sw.Count == 1 && soft.S.Sw[0].B == 12 && soft.S.Sw[0].R == 4, "blur/spread shadow aliases changed");
                Need(inset.S.Sw.Count == 1 && inset.S.Sw[0].I && atomic.S.Sw.Count == 1 && atomic.S.Sw[0].X == 3, "inset or atomic fallback changed");
                Need(field.S.Sw.Count == 2 && field.S.Sw[1].I, "field shadow model changed");
                Need(clipChild.S.Op == 50 && clipChild.S.Sw.Count == 1, "shadow opacity or overflow fixture model changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1200);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var outerRect = BlockOuter(view, "outer");
                        var insetRect = BlockOuter(view, "inset");
                        var clipRect = BlockContent(view, "shadow-clipper");
                        Need(ColorDistance(image.GetPixel(outerRect.Right + 6, outerRect.Top + outerRect.Height / 2 + 4), Color.FromArgb(220, 38, 38)) < 24, "outer shadow paint changed");
                        Need(ColorDistance(image.GetPixel(insetRect.Left + 4, insetRect.Top + insetRect.Height / 2), Color.FromArgb(22, 163, 74)) < 24, "inset shadow paint changed");
                        Need(ColorDistance(image.GetPixel(clipRect.Left + 160, clipRect.Top + clipRect.Height / 2), Color.FromArgb(223, 135, 139)) < 30 &&
                            ColorDistance(image.GetPixel(clipRect.Right + 3, clipRect.Top + clipRect.Height / 2), Color.White) < 18, "shadow opacity or overflow clip paint changed");
                    }
                    view.Size = new Size(260, 1200);
                    using (var image = Draw(view)) Need(BlockOuter(view, "outer").Right <= view.ClientSize.Width - 1, "shadow narrow reflow changed");
                }
                return "S.Sw with X/Y/B/R/C/I aliases, four-layer cap, atomic parsing, variables, priority/reset/bounds, alpha/current colors, rounded outer/inset/field paint, unchanged hits, and resize";
            });
            AddProbe(probes, "Bounded CSS background gradients", delegate
            {
                var path = Path.Combine(folder, "background-gradients.html");
                var page = new E().Go(path);
                var linear = page.Bs.First(block => block.El != null && block.El.Av("id") == "linear");
                var radial = page.Bs.First(block => block.El != null && block.El.Av("id") == "radial");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var normalized = page.Bs.First(block => block.El != null && block.El.Av("id") == "normalized");
                var limit = page.Bs.First(block => block.El != null && block.El.Av("id") == "limit");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                Need(page.S.Gi != null && page.S.Gi.K == "l" && page.S.Gi.Ss.Count == 2, "page gradient model changed");
                Need(linear.S.Gi != null && linear.S.Gi.K == "l" && linear.S.Gi.A == 90 && linear.S.Gi.Ss.Count == 3 && linear.S.Gi.Ss[1].P == 50, "linear gradient aliases changed");
                Need(radial.S.Gi != null && radial.S.Gi.K == "r" && radial.S.Gi.X == 25 && radial.S.Gi.Y == 50, "radial gradient aliases changed");
                Need(atomic.S.Gi != null && atomic.S.Gi.Ss[0].P == 10 && normalized.S.Gi.Ss[1].P == 40 && limit.S.Gi.Ss.Count == 2, "gradient atomic fallback, normalization, or cap changed");
                Need(field.S.Gi != null && field.S.Gi.A == 90 && field.S.Gi.Ss.Count == 2, "field gradient model changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1200);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var linearRect = BlockOuter(view, "linear");
                        var radialRect = BlockOuter(view, "radial");
                        var fieldRect = BlockContent(view, "field");
                        Need(ColorDistance(image.GetPixel(2, 2), Color.White) < 20 && ColorDistance(image.GetPixel(717, 2), Color.FromArgb(226, 232, 240)) < 30, "page gradient paint changed");
                        Need(ColorDistance(image.GetPixel(linearRect.Left + 8, linearRect.Top + linearRect.Height / 2), Color.FromArgb(220, 38, 38)) < 45 &&
                            ColorDistance(image.GetPixel(linearRect.Right - 8, linearRect.Top + linearRect.Height / 2), Color.FromArgb(22, 163, 74)) < 45, "linear gradient paint changed");
                        Need(ColorDistance(image.GetPixel(radialRect.Left + radialRect.Width / 4, radialRect.Top + radialRect.Height / 2), Color.FromArgb(254, 240, 138)) < 35 &&
                            ColorDistance(image.GetPixel(radialRect.Right - 10, radialRect.Top + radialRect.Height / 2), Color.FromArgb(124, 58, 237)) < 50, "radial gradient paint changed");
                        Need(ColorDistance(image.GetPixel(fieldRect.Left + 10, fieldRect.Bottom - 8), Color.FromArgb(153, 246, 228)) < 35, "field gradient paint changed");
                    }
                    view.Size = new Size(280, 1200);
                    using (var image = Draw(view)) Need(BlockOuter(view, "linear").Right <= view.ClientSize.Width - 1, "gradient narrow reflow changed");
                }
                return "S.Gi and Gi.K/A/X/Y/Ss plus Cs.C/P aliases, one-layer/eight-stop caps, atomic parsing, variables, priority/reset/bounds, normalized alpha/current colors, rounded linear/radial/page/field paint, unchanged hits, and resize";
            });
            AddProbe(probes, "Bounded CSS bitmap backgrounds", delegate
            {
                var path = Path.Combine(folder, "background-images.html");
                var engine = new E();
                var page = engine.Go(path);
                var repeat = page.Bs.First(block => block.El != null && block.El.Av("id") == "repeat");
                var positioned = page.Bs.First(block => block.El != null && block.El.Av("id") == "no-repeat");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var shorthand = page.Bs.First(block => block.El != null && block.El.Av("id") == "shorthand");
                var external = page.Bs.First(block => block.El != null && block.El.Av("id") == "external-image");
                Need(page.S.Bi != null && page.S.Bi.R == "n" && page.S.Bi.X == 100 && page.S.Bi.Y == 100 && page.S.Bi.W == 24 && page.S.Bi.H == 24,
                    "page bitmap aliases changed");
                Need(repeat.S.Bi != null && repeat.S.Bi.U.Scheme == "data" && repeat.S.Bi.R == "b" && repeat.S.Bi.W == 12 && repeat.S.Bi.H == 12,
                    "repeated bitmap aliases changed");
                Need(positioned.S.Bi != null && positioned.S.Bi.R == "n" && positioned.S.Bi.X == 100 && positioned.S.Bi.Y == 100 && positioned.S.Bi.W == 30 && positioned.S.Bi.H == 20,
                    "positioned bitmap aliases changed");
                Need(atomic.S.Bi != null && atomic.S.Bi.U == repeat.S.Bi.U && shorthand.S.Bi != null && shorthand.S.Bi.R == "n" && shorthand.S.Bi.W == 26 && shorthand.S.Bi.H == 22,
                    "bitmap atomic fallback or shorthand changed");
                Need(external.S.Bi != null && external.S.Bi.U == new Uri(Path.GetFullPath(Path.Combine(root, "assets", "lea.ico"))),
                    "linked stylesheet bitmap origin changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1400);
                    LoadView(view, path, "bitmap background fixture");
                    using (var image = Draw(view))
                    {
                        var repeatRect = BlockOuter(view, "repeat");
                        var positionedRect = BlockOuter(view, "no-repeat");
                        var roundedRect = BlockOuter(view, "rounded");
                        var fieldRect = BlockContent(view, "field");
                        Need(ColorDistance(image.GetPixel(repeatRect.Right - 6, repeatRect.Bottom - 6), Color.FromArgb(23, 107, 77)) < 4,
                            "repeated bitmap paint changed");
                        Need(ColorDistance(image.GetPixel(positionedRect.Right - 6, positionedRect.Bottom - 6), Color.FromArgb(23, 107, 77)) < 4 &&
                            ColorDistance(image.GetPixel(positionedRect.Left + 5, positionedRect.Bottom - 6), Color.White) < 8,
                            "positioned bitmap paint changed");
                        Need(ColorDistance(image.GetPixel(roundedRect.Left, roundedRect.Top), Color.White) < 8 &&
                            ColorDistance(image.GetPixel(roundedRect.Left + roundedRect.Width / 2, roundedRect.Top + roundedRect.Height / 2), Color.FromArgb(23, 107, 77)) < 4,
                            "rounded bitmap clip changed");
                        Need(ColorDistance(image.GetPixel(fieldRect.Right - 8, fieldRect.Top + fieldRect.Height / 2), Color.FromArgb(23, 107, 77)) < 8,
                            "field bitmap paint changed");
                        Need(ColorDistance(image.GetPixel(image.Width - 8, image.Height - 8), Color.FromArgb(126, 34, 206)) < 4,
                            "page bitmap paint changed");
                    }
                    view.Size = new Size(350, 1800);
                    using (var image = Draw(view))
                    {
                        var dynamicRect = BlockOuter(view, "dynamic");
                        Need(dynamicRect.Bottom <= image.Height &&
                            ColorDistance(image.GetPixel(dynamicRect.Left + dynamicRect.Width / 2, dynamicRect.Top + dynamicRect.Height / 2), Color.FromArgb(126, 34, 206)) < 4,
                            "responsive bitmap paint changed");
                        Need(BlockOuter(view, "repeat").Right <= view.ClientSize.Width - 1, "bitmap narrow reflow changed");
                    }
                }
                return "S.Bi with U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K aliases, bounded data/file loading, stylesheet/custom-property origins, atomic URL/position/size/shorthand parsing, repeat/axis/cover/contain/page/field paint, rounded clips, opacity, and resize";
            });
            AddProbe(probes, "Bounded replaced-image fitting", delegate
            {
                var path = Path.Combine(folder, "object-fit.html");
                var engine = new E();
                var page = engine.Go(path);
                var fill = page.Bs.First(block => block.El != null && block.El.Av("id") == "fill");
                var contain = page.Bs.First(block => block.El != null && block.El.Av("id") == "contain");
                var cover = page.Bs.First(block => block.El != null && block.El.Av("id") == "cover-right");
                var none = page.Bs.First(block => block.El != null && block.El.Av("id") == "none");
                var down = page.Bs.First(block => block.El != null && block.El.Av("id") == "scale-down");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset");
                var child = page.Bs.First(block => block.El != null && block.El.Av("id") == "child");
                Need(fill.S.Of == "f" && contain.S.Of == "c" && cover.S.Of == "v" && none.S.Of == "n" && down.S.Of == "s",
                    "object-fit aliases changed");
                Need(cover.S.Ox == 100 && cover.S.Oy == 100 && cover.S.Oxp && cover.S.Oyp &&
                    none.S.Ox == 25 && none.S.Oy == 75 && none.S.Oxp && none.S.Oyp,
                    "object-position aliases changed");
                Need(atomic.S.Of == "c" && reset.S.Of == "f" && reset.S.Ox == 50 && reset.S.Oy == 50 && child.S.Of == "f",
                    "object geometry atomic reset or noninheritance changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1800);
                    LoadView(view, path, "object-fit fixture");
                    using (var image = Draw(view))
                    {
                        var containRect = BlockContent(view, "contain");
                        var coverRect = BlockContent(view, "cover-right");
                        var mappedRect = BlockContent(view, "mapped-fit");
                        var mappedBlock = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "mapped-fit");
                        var mapBounds = view.Ab(view.Ax(mappedBlock.Mp.As[0].El));
                        Need(ColorDistance(image.GetPixel(containRect.Left + 10, containRect.Top + 10), Color.White) < 8 &&
                            ColorDistance(image.GetPixel(containRect.Left + 20, containRect.Top + containRect.Height / 2), Color.FromArgb(220, 38, 38)) < 20,
                            "object contain paint changed");
                        Need(ColorDistance(image.GetPixel(coverRect.Left + 60, coverRect.Top + 20), Color.FromArgb(37, 99, 235)) < 20,
                            "positioned object cover paint changed");
                        Need(mapBounds == new Rectangle(mappedRect.Left, mappedRect.Top + 20, 80, 80) && view.Kn == 2 && view.Kf() == 1 && view.Kb == mapBounds,
                            "fitted image-map geometry changed");
                    }
                    view.Size = new Size(350, 1800);
                    using (var image = Draw(view))
                    {
                        var dynamic = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "dynamic");
                        Need(dynamic.S.Of == "v" && dynamic.S.Ox == 100 && dynamic.S.Oy == 100,
                            "responsive object geometry changed");
                    }
                }
                return "S.Of=f/c/v/n/s with Ox/Oy/Oxp/Oyp aliases, atomic variables/priority/reset/noninheritance, clipped fill/contain/cover/none/scale-down paint, fitted map pointer/accessibility/keyboard geometry, and resize";
            });
            AddProbe(probes, "Bounded CSS aspect ratios", delegate
            {
                var path = Path.Combine(folder, "aspect-ratio.html");
                var engine = new E();
                var page = engine.Go(path);
                var wide = page.Bs.First(block => block.El != null && block.El.Av("id") == "wide");
                var autoImage = page.Bs.First(block => block.El != null && block.El.Av("id") == "auto-image");
                var fallback = page.Bs.First(block => block.El != null && block.El.Av("id") == "fallback-image");
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset");
                var child = page.Bs.First(block => block.El != null && block.El.Av("id") == "ratio-child");
                Need(wide.S.Rw == 16D && wide.S.Rh == 9D && !wide.S.Ra,
                    "aspect-ratio aliases changed");
                Need(autoImage.S.Rw == 1D && autoImage.S.Rh == 1D && autoImage.S.Ra &&
                    fallback.S.Rw == 4D && fallback.S.Rh == 3D && fallback.S.Ra,
                    "intrinsic ratio fallback changed");
                Need(reset.S.Rw == 0D && reset.S.Rh == 0D && reset.S.Ra &&
                    child.S.Rw == 0D && child.S.Rh == 0D,
                    "aspect ratio reset or noninheritance changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 3600);
                    LoadView(view, path, "aspect-ratio fixture");
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "wide").Size == new Size(240, 135) &&
                            BlockContent(view, "auto-image").Size == new Size(100, 50) &&
                            BlockContent(view, "fallback-image").Size == new Size(100, 75) &&
                            BlockOuter(view, "ratio-table").Size == new Size(240, 120),
                            "aspect ratio shared layout changed");
                    }
                    view.Size = new Size(350, 4200);
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "dynamic").Size == new Size(200, 200),
                            "responsive aspect ratio changed");
                    }
                }
                return "S.Rw/Rh/Ra aliases, bounded atomic parsing and reset/noninheritance, intrinsic media fallback, content/border-box, fields, flex/grid/table layout, paint, and resize";
            });
            AddProbe(probes, "Bounded CSS outlines", delegate
            {
                var path = Path.Combine(folder, "outlines.html");
                var engine = new E();
                var page = engine.Go(path);
                var solid = page.Bs.First(block => block.El != null && block.El.Av("id") == "solid");
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic");
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset");
                var child = page.Bs.First(block => block.El != null && block.El.Av("id") == "outline-child");
                Need(solid.S.Ow == 4 && solid.S.Os == "s" && solid.S.Oc == "#a13b18" && solid.S.Oo == 3,
                    "outline aliases changed");
                Need(atomic.S.Ow == 2 && atomic.S.Os == "s" && reset.S.Ow == 3 && reset.S.Os == "n" && child.S.Ow == 0,
                    "outline atomic reset or noninheritance changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1900);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var rect = BlockOuter(view, "solid");
                        Need(rect.Size == new Size(160, 50) &&
                            ColorDistance(image.GetPixel(rect.Left - 5, rect.Top + rect.Height / 2), Color.FromArgb(161, 59, 24)) < 16,
                            "outline geometry or paint changed");
                    }
                    Need(view.Js("fo('focus-field')") == 1, "outline focus command changed");
                    using (var image = Draw(view))
                    {
                        var field = BlockOuter(view, "focus-field");
                        Need(ColorDistance(image.GetPixel(field.Left - 7, field.Top + field.Height / 2), Color.FromArgb(220, 38, 38)) < 24,
                            "focused field outline changed");
                    }
                    view.Size = new Size(350, 2300);
                    using (var image = Draw(view))
                    {
                        var responsive = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "responsive");
                        Need(responsive.S.Ow == 6 && responsive.S.Os == "o" && responsive.S.Oo == 5,
                            "responsive outline changed");
                    }
                }
                return "S.Ow/Os/Oc/Oo aliases, atomic shorthand/longhands, variables/priority/reset/noninheritance, solid/dashed/dotted/auto and signed offsets, rounded box/field focus paint, clipping, stable geometry, and resize";
            });
            AddProbe(probes, "Bounded pointer interaction", delegate
            {
                var path = Path.Combine(folder, "pointer-interaction.html");
                var engine = new E();
                var page = engine.Go(path);
                var pointer = page.Bs.First(block => block.El != null && block.El.Av("id") == "pointer");
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset");
                var variable = page.Bs.First(block => block.El != null && block.El.Av("id") == "variable");
                var child = page.Bs.First(block => block.El != null && block.El.Av("id") == "none-child");
                var enabled = page.Bs.First(block => block.El != null && block.El.Av("id") == "auto-child");
                var hoverCursor = page.Bs.First(block => block.El != null && block.El.Av("id") == "hover-cursor");
                Need(page.S.Cu == "c" && page.S.Pe && pointer.S.Cu == "p" && pointer.S.Pe && hoverCursor.S.Cu == "p",
                    "cursor aliases or inheritance changed");
                Need(reset.S.Cu.Length == 0 && reset.S.Pe && variable.S.Cu == "p" && !variable.S.Pe,
                    "pointer reset, variables, or event gate changed");
                Need(child.S.Cu == "n" && !child.S.Pe && enabled.S.Cu == "h" && enabled.S.Pe,
                    "pointer subtree inheritance or re-enable changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2200);
                    view.Go(path);
                    Rectangle pointerRect;
                    Rectangle hoverCursorRect;
                    Rectangle deadRect;
                    Rectangle enabledRect;
                    using (var image = Draw(view))
                    {
                        pointerRect = BlockOuter(view, "pointer");
                        hoverCursorRect = BlockOuter(view, "hover-cursor");
                        deadRect = BlockOuter(view, "dead-link");
                        enabledRect = BlockOuter(view, "auto-child");
                        Need(deadRect.Size == new Size(202, 46) && ElementHit(view, "dead-link").IsEmpty &&
                            !ElementHit(view, "live-link").IsEmpty && ElementHit(view, "dead-field").IsEmpty && !ElementHit(view, "live-field").IsEmpty,
                            "pointer hit gating or stable geometry changed");
                    }
                    Move(view, pointerRect);
                    Need(Object.ReferenceEquals(view.Cursor, Cursors.Hand), "pointer cursor mapping changed");
                    Move(view, hoverCursorRect);
                    Need(Object.ReferenceEquals(view.Cursor, Cursors.Help) &&
                        view.At.Bs.First(block => block.El != null && block.El.Av("id") == "hover-cursor").S.Cu == "h",
                        "dynamic hover cursor changed");
                    Move(view, deadRect);
                    Need(Object.ReferenceEquals(view.Cursor, Cursors.Cross), "pointer fallthrough cursor changed");
                    Move(view, enabledRect);
                    Need(Object.ReferenceEquals(view.Cursor, Cursors.Help), "pointer child cursor changed");
                    Need(view.Js("fo('dead-field')") == 1 && view.Ke != null && view.Ke.Av("id") == "dead-field" && !view.Kb.IsEmpty,
                        "pointer suppression changed keyboard focus");
                    view.Size = new Size(350, 2600);
                    using (var image = Draw(view))
                    {
                        var responsive = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "responsive");
                        Need(responsive.S.Cu == "w" && !responsive.S.Pe,
                            "responsive pointer state changed");
                    }
                }
                return "S.Cu/Pe inherited aliases, bounded cursor keywords, variables/priority/reset/invalid preservation, pointer-disabled link/field hits and hover, child re-enable, semantic cursors, keyboard/accessibility stability, and resize";
            });
            AddProbe(probes, "Bounded text transform", delegate
            {
                var path = Path.Combine(folder, "text-transform.html");
                var engine = new E { Vw = 720 };
                var page = engine.Go(path);
                var inherited = page.Bs.First(block => block.El != null && block.El.Av("id") == "inherited");
                var upper = page.Bs.First(block => block.El != null && block.El.Av("id") == "upper");
                var lower = page.Bs.First(block => block.El != null && block.El.Av("id") == "lower");
                var inline = page.Bs.First(block => block.El != null && block.El.Av("id") == "inline");
                var invalid = page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid");
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset");
                var variable = page.Bs.First(block => block.El != null && block.El.Av("id") == "variable");
                var priority = page.Bs.First(block => block.El != null && block.El.Av("id") == "priority");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                Need(page.S.Tt == "c" && inherited.X == "Inherited Heading Text" && upper.S.Tt == "u" && upper.X == "UPPER MIXED TEXT" &&
                    lower.S.Tt == "l" && lower.X == "lower mixed text", "text transform aliases or inheritance changed");
                Need(inline.Is.Count == 5 && inline.X == "START mixed low Lea-Engine Words Raw MiXeD END" &&
                    inline.Is[1].S.Tt == "l" && inline.Is[2].S.Tt == "c" && inline.Is[3].S.Tt == "n",
                    "inline text transform runs changed");
                Need(invalid.S.Tt == "c" && reset.S.Tt == "n" && variable.S.Tt == "l" && priority.S.Tt == "u",
                    "text transform cascade behavior changed");
                Need(field.V == "Field MiXeD Text" && E.Tx(field.V, field.S) == "field mixed text" && page.X.Contains("Upper MiXeD Text"),
                    "raw form or document text changed");
                engine.Dy(page, ById(page.D, "dynamic"), null);
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "dynamic").X == "DYNAMIC MIXED TEXT",
                    "dynamic text transform changed");
                engine.Vw = 350;
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "responsive").X == "responsive mixed text" &&
                    Object.ReferenceEquals(field, page.Fs.First(item => item.El != null && item.El.Av("id") == "field")),
                    "responsive transform or retained field changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2300);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var upperRect = BlockOuter(view, "upper");
                        Need(!upperRect.IsEmpty && ColorDistance(image.GetPixel(upperRect.Left + 3, upperRect.Top + 3), Color.FromArgb(248, 250, 252)) < 12,
                            "text transform paint changed");
                    }
                    var dynamicRect = BlockOuter(view, "dynamic");
                    Move(view, dynamicRect);
                    Need(view.At.Bs.First(block => block.El != null && block.El.Av("id") == "dynamic").X == "DYNAMIC MIXED TEXT",
                        "text transform hover repaint changed");
                    view.Size = new Size(350, 2600);
                    using (var image = Draw(view))
                    {
                        var responsive = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "responsive");
                        Need(responsive.S.Tt == "l" && responsive.X == "responsive mixed text", "text transform resize changed");
                    }
                }
                return "S.Tt n/u/l/c inherited alias, strict none/upper/lower/capitalize parsing, inline overrides, variables/priority/resets/invalid preservation, multiline/table/control presentation, raw DOM/form retention, hover, paint, and resize";
            });
            AddProbe(probes, "Bounded text spacing", delegate
            {
                var path = Path.Combine(folder, "text-spacing.html");
                var engine = new E { Vw = 720 };
                var page = engine.Go(path);
                var positive = page.Bs.First(block => block.El != null && block.El.Av("id") == "positive");
                var negative = page.Bs.First(block => block.El != null && block.El.Av("id") == "negative");
                var inline = page.Bs.First(block => block.El != null && block.El.Av("id") == "inline");
                var invalid = page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid");
                var bounded = page.Bs.First(block => block.El != null && block.El.Av("id") == "bounded");
                var variable = page.Bs.First(block => block.El != null && block.El.Av("id") == "variable");
                var priority = page.Bs.First(block => block.El != null && block.El.Av("id") == "priority");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                Need(page.S.Ls == 1 && page.S.Wg == 2 && positive.S.Ls == 3 && positive.S.Wg == 8 &&
                    negative.S.Ls == -2 && negative.S.Wg == -4, "text spacing aliases or inheritance changed");
                Need(inline.Is.Count == 4 && inline.Is[1].S.Ls == 6 && inline.Is[1].S.Wg == 12 &&
                    inline.Is[2].S.Ls == 0 && inline.Is[2].S.Wg == 0, "inline text spacing runs changed");
                Need(invalid.S.Ls == 5 && invalid.S.Wg == 7 && bounded.S.Ls == 1 && bounded.S.Wg == 2 &&
                    variable.S.Ls == 4 && variable.S.Wg == 9 && priority.S.Ls == 7 && priority.S.Wg == 11,
                    "text spacing cascade or bounds changed");
                Need(field.V == "Field raw words" && page.Fs.First(item => item.El != null && item.El.Av("id") == "button").V == "Send raw words",
                    "spaced form values changed");
                engine.Dy(page, ById(page.D, "dynamic"), null);
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "dynamic").S.Ls == 8 &&
                    Object.ReferenceEquals(field, page.Fs.First(item => item.El != null && item.El.Av("id") == "field")),
                    "dynamic text spacing changed");
                engine.Vw = 350;
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "responsive").S.Ls == -1 &&
                    Object.ReferenceEquals(field, page.Fs.First(item => item.El != null && item.El.Av("id") == "field")),
                    "responsive spacing or retained field changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2600);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var normalWidth = InlineBounds(view, "normal").Width;
                        var positiveWidth = InlineBounds(view, "positive").Width;
                        var negativeWidth = InlineBounds(view, "negative").Width;
                        Need(positiveWidth > normalWidth && normalWidth > negativeWidth && ColoredSamples(image) > 0,
                            "text spacing metric or paint changed");
                    }
                    var dynamicRect = BlockOuter(view, "dynamic");
                    Move(view, dynamicRect);
                    using (var image = Draw(view))
                        Need(view.At.Bs.First(block => block.El != null && block.El.Av("id") == "dynamic").S.Wg == 14,
                            "text spacing hover repaint changed");
                    view.Size = new Size(350, 2900);
                    using (var image = Draw(view))
                    {
                        var responsive = view.At.Bs.First(block => block.El != null && block.El.Av("id") == "responsive");
                        Need(responsive.S.Ls == -1 && responsive.S.Wg == 0 && !BlockOuter(view, "after-spacing").IsEmpty,
                            "text spacing resize changed");
                    }
                }
                return "S.Ls/Wg inherited signed pixel aliases, strict normal/reset/bounds/invalid preservation, inline overrides, variables/priority, shared wrap and paint metrics, raw form retention, hover, and resize";
            });
            AddProbe(probes, "Bounded text overflow", delegate
            {
                var path = Path.Combine(folder, "text-overflow.html");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 3200);
                    view.Go(path);
                    var field = view.At.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                    var block = view.At.Bs.First(item => item.El != null && item.El.Av("id") == "ellipsis");
                    Need(view.At.S.To == "c" && block.S.To == "e" && view.At.X.Contains("Alpha beta gamma delta epsilon ending"),
                        "text overflow aliases or authored text changed");
                    using (var image = Draw(view))
                    {
                        Need(EllipsisCount(view, "clip") == 0 && EllipsisCount(view, "ellipsis") == 1 &&
                            EllipsisCount(view, "short") == 0 && EllipsisCount(view, "visible") == 0 &&
                            EllipsisCount(view, "pre") == 2 && EllipsisCount(view, "inline") == 1 &&
                            EllipsisCount(view, "cell") == 1 && EllipsisCount(view, "inherited") == 1 &&
                            EllipsisCount(view, "noninherited") == 0 && EllipsisCount(view, "invalid") == 1 &&
                            EllipsisCount(view, "important") == 1 && ColoredSamples(image) > 0,
                            "ellipsis paint, cascade, or retained line behavior changed");
                    }
                    Move(view, BlockOuter(view, "dynamic"));
                    using (var image = Draw(view)) Need(EllipsisCount(view, "dynamic") == 1, "ellipsis hover changed");
                    view.Size = new Size(350, 3400);
                    using (var image = Draw(view))
                        Need(EllipsisCount(view, "responsive") == 1 &&
                            Object.ReferenceEquals(field, view.At.Fs.First(item => item.El != null && item.El.Av("id") == "field")),
                            "ellipsis resize or field retention changed");
                }
                return "S.To c/e non-inherited alias, strict clip/ellipsis, inherit/reset/invalid/variable/important cascade, paint-only inline/table/multiline truncation, preserved authored text, hover, and resize";
            });
            AddProbe(probes, "Bounded flex layout", delegate
            {
                var path = Path.Combine(folder, "flex.html");
                var page = new E().Go(path);
                var spread = page.Bs.First(block => block.El != null && block.El.Av("id") == "spread");
                var first = page.Bs.First(block => block.El != null && block.El.Av("id") == "first");
                var reverse = page.Bs.First(block => block.El != null && block.El.Av("id") == "reverse");
                var wrap = page.Bs.First(block => block.El != null && block.El.Av("id") == "wrap");
                var wrapOne = page.Bs.First(block => block.El != null && block.El.Av("id") == "wrap-one");
                var grow = page.Bs.First(block => block.El != null && block.El.Av("id") == "grow");
                var growOne = page.Bs.First(block => block.El != null && block.El.Av("id") == "grow-one");
                var growTwo = page.Bs.First(block => block.El != null && block.El.Av("id") == "grow-two");
                var shrinkOne = page.Bs.First(block => block.El != null && block.El.Av("id") == "shrink-one");
                var shrinkTwo = page.Bs.First(block => block.El != null && block.El.Av("id") == "shrink-two");
                var shrinkLock = page.Bs.First(block => block.El != null && block.El.Av("id") == "shrink-lock");
                var orderOne = page.Bs.First(block => block.El != null && block.El.Av("id") == "order-one");
                var orderTwo = page.Bs.First(block => block.El != null && block.El.Av("id") == "order-two");
                var column = page.Bs.First(block => block.El != null && block.El.Av("id") == "column");
                var columnReverse = page.Bs.First(block => block.El != null && block.El.Av("id") == "column-reverse");
                var around = page.Bs.First(block => block.El != null && block.El.Av("id") == "around");
                var evenly = page.Bs.First(block => block.El != null && block.El.Av("id") == "evenly");
                var aroundOne = page.Bs.First(block => block.El != null && block.El.Av("id") == "around-one");
                Need(spread.S.D == "flex" && spread.S.Fd == "r" && spread.S.Jc == "b" && spread.S.Ai == "c" && spread.S.Gp == 10, "flex style aliases changed");
                Need(Object.ReferenceEquals(first.Up, spread) && reverse.S.Fd == "x", "flex ownership or reverse direction changed");
                Need(wrap.S.Wr == "w" && wrap.S.Rg == 12 && wrap.S.Gp == 10 && wrapOne.S.Fg == 1 && wrapOne.S.Fb == 150, "wrapped flex aliases changed");
                Need(grow.S.Rg == 4 && grow.S.Gp == 8 && growOne.S.Fg == 1 && growOne.S.Fb == 80 && growTwo.S.Fg == 2 && growTwo.S.Fb == 80, "weighted flex aliases changed");
                Need(shrinkOne.S.Sh == 1 && shrinkTwo.S.Sh == 2 && shrinkLock.S.Sh == 0 && shrinkLock.S.Wn == 100 && shrinkLock.S.Wx == 100, "weighted flex shrink aliases changed");
                Need(orderOne.S.Or == 2 && orderTwo.S.Or == -1, "flex order aliases changed");
                Need(column.S.Fd == "c" && columnReverse.S.Fd == "y", "flex column aliases changed");
                Need(around.S.Jc == "a" && evenly.S.Jc == "v" && aroundOne.S.As == "t", "distributed flex or align-self aliases changed");
                using (var view = NewView())
                {
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var one = BlockOuter(view, "first");
                        var two = BlockOuter(view, "second");
                        var three = BlockOuter(view, "third");
                        var reverseOne = BlockOuter(view, "reverse-one");
                        var reverseTwo = BlockOuter(view, "reverse-two");
                        var wrapOneBox = BlockOuter(view, "wrap-one");
                        var wrapTwoBox = BlockOuter(view, "wrap-two");
                        var wrapThreeBox = BlockOuter(view, "wrap-three");
                        var growOneBox = BlockOuter(view, "grow-one");
                        var growTwoBox = BlockOuter(view, "grow-two");
                        var growFixedBox = BlockOuter(view, "grow-fixed");
                        var shrinkOneBox = BlockOuter(view, "shrink-one");
                        var shrinkTwoBox = BlockOuter(view, "shrink-two");
                        var shrinkLockBox = BlockOuter(view, "shrink-lock");
                        var orderOneBox = BlockOuter(view, "order-one");
                        var orderTwoBox = BlockOuter(view, "order-two");
                        var orderThreeBox = BlockOuter(view, "order-three");
                        var columnOneBox = BlockOuter(view, "column-one");
                        var columnTwoBox = BlockOuter(view, "column-two");
                        var columnThreeBox = BlockOuter(view, "column-three");
                        var columnReverseOneBox = BlockOuter(view, "column-reverse-one");
                        var columnReverseTwoBox = BlockOuter(view, "column-reverse-two");
                        var columnReverseThreeBox = BlockOuter(view, "column-reverse-three");
                        var columnAutoOneBox = BlockOuter(view, "column-auto-one");
                        var columnAutoTwoBox = BlockOuter(view, "column-auto-two");
                        var aroundBox = BlockOuter(view, "around");
                        var aroundOneBox = BlockOuter(view, "around-one");
                        var aroundTwoBox = BlockOuter(view, "around-two");
                        var aroundThreeBox = BlockOuter(view, "around-three");
                        var evenlyBox = BlockOuter(view, "evenly");
                        var evenlyOneBox = BlockOuter(view, "evenly-one");
                        var evenlyTwoBox = BlockOuter(view, "evenly-two");
                        var evenlyThreeBox = BlockOuter(view, "evenly-three");
                        var after = BlockOuter(view, "after-flex");
                        Need(one.Left < two.Left && two.Left < three.Left && two.Left - one.Right == 122, "flex distribution changed");
                        Need(reverseOne.Left > reverseTwo.Left, "row-reverse changed");
                        Need(wrapOneBox.Width == 175 && wrapOneBox.Left - wrapTwoBox.Right == 10 && wrapThreeBox.Top - wrapOneBox.Top == 56, "wrapped flex geometry changed");
                        Need(growOneBox.Width == 140 && growTwoBox.Width == 200 && growFixedBox.Width == 80 && growTwoBox.Left - growOneBox.Right == 8, "weighted flex geometry changed");
                        Need(shrinkOneBox.Width == 140 && shrinkTwoBox.Width == 100 && shrinkLockBox.Width == 100 && shrinkLockBox.Left - shrinkTwoBox.Right == 10, "weighted flex shrink geometry changed");
                        Need(orderTwoBox.Left < orderOneBox.Left && orderOneBox.Left < orderThreeBox.Left && orderOneBox.Left - orderTwoBox.Right == 8, "ordered row geometry changed");
                        Need(columnTwoBox.Height == 106 && columnOneBox.Height == 74 && columnThreeBox.Height == 40 && columnOneBox.Top - columnTwoBox.Bottom == 10, "ordered column growth changed");
                        Need(columnReverseOneBox.Height == 50 && columnReverseTwoBox.Height == 30 && columnReverseThreeBox.Height == 30 && columnReverseOneBox.Top - columnReverseTwoBox.Bottom == 10, "column-reverse shrink changed");
                        Need(columnAutoTwoBox.Top - columnAutoOneBox.Bottom == 7, "auto-height column gap changed");
                        Need(aroundOneBox.Left - aroundBox.Left == 38 && aroundTwoBox.Left - aroundOneBox.Right == 63 && aroundOneBox.Height == 90 && aroundThreeBox.Bottom == aroundBox.Bottom - 12, "space-around or row align-self changed");
                        Need(evenlyOneBox.Top - evenlyBox.Top == 39 && evenlyTwoBox.Top - evenlyOneBox.Bottom == 37 && evenlyThreeBox.Right == evenlyBox.Right - 12, "space-evenly or column align-self changed");
                        Need(after.Top > evenlyBox.Bottom, "flex following flow changed");
                        Need(ColoredSamples(image) > 0, "flex layout is blank");
                    }
                }
                return "Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or aliases, stable row order, column/reverse flow, around/evenly distribution, item alignment, fixed, auto, wrapped, growing, shrinking, protected, and locked slots, hits, resize, and following flow";
            });
            AddProbe(probes, "Bounded grid layout", delegate
            {
                var path = Path.Combine(folder, "grid.html");
                var page = new E().Go(path);
                var equal = page.Bs.First(block => block.El != null && block.El.Av("id") == "equal");
                var mixed = page.Bs.First(block => block.El != null && block.El.Av("id") == "mixed");
                var span = page.Bs.First(block => block.El != null && block.El.Av("id") == "mixed-four");
                var rows = page.Bs.First(block => block.El != null && block.El.Av("id") == "rows");
                var rowSpan = page.Bs.First(block => block.El != null && block.El.Av("id") == "rows-b");
                var invalid = page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid");
                Need(equal.S.D == "grid" && equal.S.Gc == 3 && equal.S.Gv(0) == -1 && equal.S.Gv(2) == -1 && equal.S.Rg == 9 && equal.S.Gp == 6, "equal grid aliases changed");
                Need(mixed.S.Gc == 3 && mixed.S.Gv(0) == 100 && mixed.S.Gv(1) == -1 && mixed.S.Gv(2) == -2, "mixed grid tracks changed");
                Need(span.S.Gl == 2 && span.S.Gs == 2 && rows.S.Rc == 3 && rows.S.Rv(0) == 30 && rows.S.Rv(1) == -1 && rows.S.Rv(2) == -2 && rowSpan.S.Rl == 2 && rowSpan.S.Rs == 2, "grid row or placement aliases changed");
                Need(invalid.S.Gc == 0 && invalid.S.Rc == 0 && page.Bs.First(block => block.El != null && block.El.Av("id") == "limit").S.Gc == 8 && page.Bs.First(block => block.El != null && block.El.Av("id") == "limit").S.Rc == 8, "grid bounds changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2200);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var equalOne = BlockOuter(view, "equal-one");
                        var equalTwo = BlockOuter(view, "equal-two");
                        var equalFour = BlockOuter(view, "equal-four");
                        var mixedOne = BlockOuter(view, "mixed-one");
                        var mixedTwo = BlockOuter(view, "mixed-two");
                        var mixedThree = BlockOuter(view, "mixed-three");
                        var mixedFour = BlockOuter(view, "mixed-four");
                        var orderOne = BlockOuter(view, "order-one");
                        var orderTwo = BlockOuter(view, "order-two");
                        var orderThree = BlockOuter(view, "order-three");
                        var rowsA = BlockOuter(view, "rows-a");
                        var rowsB = BlockOuter(view, "rows-b");
                        var rowsC = BlockOuter(view, "rows-c");
                        var autoRows = BlockOuter(view, "auto-rows");
                        var autoRowA = BlockOuter(view, "auto-row-a");
                        var autoRowB = BlockOuter(view, "auto-row-b");
                        var autoRowC = BlockOuter(view, "auto-row-c");
                        Need(equalOne.Width == 106 && equalTwo.Left - equalOne.Right == 6 && equalFour.Width == 218 && equalFour.Top - equalOne.Bottom == 9, "equal grid geometry changed");
                        Need(mixedOne.Width == 100 && mixedTwo.Width == 80 && mixedThree.Width == 160 && mixedFour.Width == 250 && mixedFour.Left == mixedTwo.Left, "mixed grid geometry changed");
                        Need(orderTwo.Left < orderThree.Left && orderOne.Top > orderTwo.Bottom, "ordered grid placement changed");
                        Need(rowsA.Width == 198 && rowsA.Height == 30 && rowsB.Width == 198 && rowsB.Height == 112 && rowsC.Width == 96 && rowsC.Height == 70 && rowsB.Left - rowsC.Right == 6 && rowsB.Bottom == rowsC.Bottom, "two-axis grid row geometry changed");
                        Need(autoRowA.Width == 240 && autoRowA.Height == 20 && autoRowB.Top - autoRowA.Bottom == 5 && autoRowB.Top == autoRowC.Top && autoRowB.Bottom == autoRowC.Bottom && autoRowC.Bottom == autoRows.Bottom, "auto-height grid row settling changed");
                        Need(BlockOuter(view, "after-grid").Top > BlockOuter(view, "grid-target").Bottom && ColoredSamples(image) > 0, "grid following flow or paint changed");
                    }
                    var wide = BlockOuter(view, "responsive-one").Width;
                    var field = view.At.Fs.First(item => item.El != null && item.El.Av("id") == "grid-field");
                    field.V = "Retained benchmark grid field";
                    view.Size = new Size(360, 2200);
                    using (var image = Draw(view)) Need(BlockOuter(view, "responsive-one").Width < wide && Object.ReferenceEquals(field, view.At.Fs.First(item => item.El != null && item.El.Av("id") == "grid-field")) && field.V == "Retained benchmark grid field" && ColoredSamples(image) > 0, "narrow grid reflow or field retention changed");
                }
                return "Gd/Gc/Gv columns, Rd/Rc/Rv rows, Gl/Gs and Rl/Rs two-axis placement, bounded px/fr/repeat/minmax tracks, gaps, spans, occupancy, paint, resize, and flow";
            });
            AddProbe(probes, "Bounded named grid areas", delegate
            {
                var path = Path.Combine(folder, "grid-areas.html");
                var page = new E().Go(path);
                var dashboard = page.Bs.First(block => block.El != null && block.El.Av("id") == "dashboard");
                var head = page.Bs.First(block => block.El != null && block.El.Av("id") == "area-head");
                var main = page.Bs.First(block => block.El != null && block.El.Av("id") == "area-main");
                Need(dashboard.S.Aa == 4 && dashboard.S.Ar == 3 && dashboard.S.Ac == 3 && dashboard.S.Ag("head", 0) == 1 && dashboard.S.Ag("head", 3) == 3 && dashboard.S.Ag("main", 1) == 2 && dashboard.S.Ag("main", 3) == 2, "named grid area aliases changed");
                Need(head.S.An == "head" && main.S.An == "main" && page.Bs.First(block => block.El != null && block.El.Av("id") == "area-auto").S.An == "", "named grid item aliases changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "bad-uneven").S.Aa == 0 && page.Bs.First(block => block.El != null && block.El.Av("id") == "bad-shape").S.Aa == 0 && page.Bs.First(block => block.El != null && block.El.Av("id") == "too-many").S.Aa == 0, "invalid named grid templates changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1500);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var dashboardRect = BlockOuter(view, "dashboard");
                        var headRect = BlockOuter(view, "area-head");
                        var navRect = BlockOuter(view, "area-nav");
                        var mainRect = BlockOuter(view, "area-main");
                        var footRect = BlockOuter(view, "area-foot");
                        var autoRect = BlockOuter(view, "area-auto");
                        Need(headRect.Width == 360 && headRect.Height == 40 && navRect.Width == 80 && navRect.Height == 78 && mainRect.Width == 272 && mainRect.Height == 78 && footRect.Width == 176 && footRect.Height == 50 && autoRect.Width == 176 && autoRect.Height == 50, "named grid area geometry changed");
                        Need(mainRect.Left - navRect.Right == 8 && mainRect.Top - headRect.Bottom == 6 && footRect.Top - navRect.Bottom == 6 && autoRect.Left - footRect.Right == 8 && autoRect.Bottom == dashboardRect.Bottom, "named grid placement or gaps changed");
                        var left = BlockOuter(view, "implicit-left");
                        var right = BlockOuter(view, "implicit-right");
                        Need(left.Width == 96 && right.Width == 96 && right.Left - left.Right == 8 && ColoredSamples(image) > 0, "named implicit columns or paint changed");
                    }
                }
                return "Aa/Ar/Ac/Ag area templates plus An item names, rectangular validation, dot cells, definite-first placement, implicit columns, bounds, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded named grid lines", delegate
            {
                var path = Path.Combine(folder, "grid-lines.html");
                var page = new E().Go(path);
                var layout = page.Bs.First(block => block.El != null && block.El.Av("id") == "line-layout");
                var head = page.Bs.First(block => block.El != null && block.El.Av("id") == "line-head");
                var area = page.Bs.First(block => block.El != null && block.El.Av("id") == "area-lines");
                Need(layout.S.Gnc == 7 && layout.S.Rnc == 5 && layout.S.Gx("edge") == 1 && layout.S.Gx("main-end") == 4 && layout.S.Rx("footer-start") == 3 && head.S.Gn == "edge" && head.S.Ge == "edge-end", "named grid line aliases changed");
                Need(area.S.Gx("lead-start") == 1 && area.S.Gx("lead-end") == 2 && area.S.Rx("lead-end") == 2, "area-generated named lines changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "bad-duplicate").S.Gx("keep") == 1 && page.Bs.First(block => block.El != null && block.El.Av("id") == "too-many-lines").S.Gc == 0 && page.Bs.First(block => block.El != null && block.El.Av("id") == "line-limit").S.Gnc == 16, "named grid line validation changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1700);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var layoutRect = BlockOuter(view, "line-layout");
                        var headRect = BlockOuter(view, "line-head");
                        var navRect = BlockOuter(view, "line-nav");
                        var mainRect = BlockOuter(view, "line-main");
                        var footRect = BlockOuter(view, "line-foot");
                        var autoRect = BlockOuter(view, "line-auto");
                        Need(headRect.Width == 360 && headRect.Height == 40 && navRect.Width == 80 && navRect.Height == 78 && mainRect.Width == 272 && mainRect.Height == 78 && footRect.Width == 176 && footRect.Height == 50 && autoRect.Width == 176 && autoRect.Height == 50, "named grid line geometry changed");
                        Need(mainRect.Left - navRect.Right == 8 && mainRect.Top - headRect.Bottom == 6 && footRect.Top - navRect.Bottom == 6 && autoRect.Left - footRect.Right == 8 && autoRect.Bottom == layoutRect.Bottom, "named grid line placement or gaps changed");
                        var lead = BlockOuter(view, "area-line-lead");
                        var story = BlockOuter(view, "area-line-story");
                        var implicitOne = BlockOuter(view, "implicit-row-one");
                        var implicitThree = BlockOuter(view, "implicit-row-three");
                        Need(lead.Width == 96 && story.Width == 96 && story.Left - lead.Right == 8 && implicitThree.Top > implicitOne.Bottom && ColoredSamples(image) > 0, "area-generated lines, implicit numeric rows, or paint changed");
                    }
                }
                return "Gnc/Rnc/Gx/Rx templates plus Gn/Ge/Rn/Re item names, area-generated edges, unique validation, repeats, definite-first placement, bounds, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded grid auto flow", delegate
            {
                var path = Path.Combine(folder, "grid-flow.html");
                var page = new E().Go(path);
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "row-sparse").S.Gf == "r" && page.Bs.First(block => block.El != null && block.El.Av("id") == "row-dense").S.Gf == "d" && page.Bs.First(block => block.El != null && block.El.Av("id") == "column-flow").S.Gf == "c" && page.Bs.First(block => block.El != null && block.El.Av("id") == "column-dense").S.Gf == "x", "grid auto-flow aliases changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "flow-reset").S.Gf == "r" && page.Bs.First(block => block.El != null && block.El.Av("id") == "flow-invalid").S.Gf == "c", "grid auto-flow validation changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1900);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var sparseA = BlockOuter(view, "row-sparse-a");
                        var sparseB = BlockOuter(view, "row-sparse-b");
                        var sparseC = BlockOuter(view, "row-sparse-c");
                        var denseA = BlockOuter(view, "row-dense-a");
                        var denseB = BlockOuter(view, "row-dense-b");
                        var denseC = BlockOuter(view, "row-dense-c");
                        Need(sparseA.Width == 198 && sparseC.Width == 96 && sparseC.Top == sparseB.Top && denseA.Width == 198 && denseC.Top == denseA.Top && denseB.Top > denseA.Bottom, "row sparse or dense placement changed");
                        var one = BlockOuter(view, "column-one");
                        var two = BlockOuter(view, "column-two");
                        var three = BlockOuter(view, "column-three");
                        var five = BlockOuter(view, "column-five");
                        Need(one.Width == 93 && three.Width == 93 && five.Width == 94 && one.Left == two.Left && three.Left - one.Right == 10, "column flow implicit tracks changed");
                        Need(BlockOuter(view, "column-sparse-c").Left == BlockOuter(view, "column-sparse-b").Left && BlockOuter(view, "column-dense-c").Left == BlockOuter(view, "column-dense-a").Left, "column dense placement changed");
                        var eight = BlockOuter(view, "ceiling-eight");
                        var nine = BlockOuter(view, "ceiling-nine");
                        Need(eight.Left == nine.Left && nine.Top > eight.Top && ColoredSamples(image) > 0, "column flow ceiling or paint changed");
                    }
                }
                return "Gf r/d/c/x flow, sparse cursors, dense backfill, row/column swapping, equal implicit columns, eight-column overflow, bounds, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded grid auto tracks", delegate
            {
                var path = Path.Combine(folder, "grid-auto-tracks.html");
                var page = new E().Go(path);
                var pattern = page.Bs.First(block => block.El != null && block.El.Av("id") == "track-pattern").S;
                Need(pattern.Gac == 2 && pattern.Gav(0) == 40 && pattern.Gav(1) == -1 && pattern.Rac == 2 && pattern.Rav(0) == 30 && pattern.Rav(1) == -2, "grid auto-track aliases changed");
                var mixed = page.Bs.First(block => block.El != null && block.El.Av("id") == "auto-mix").S;
                var invalid = page.Bs.First(block => block.El != null && block.El.Av("id") == "track-invalid").S;
                Need(mixed.Gav(0) == S.Av && mixed.Gav(1) == -2 && invalid.Gav(0) == 35 && invalid.Gav(1) == -2 && invalid.Rav(0) == 30 && invalid.Rav(1) == S.Av, "grid auto-track auto or invalid preservation changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "track-reset").S.Gac == 0 && page.Bs.First(block => block.El != null && block.El.Av("id") == "track-limit").S.Gac == 8, "grid auto-track reset or bounds changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1700);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var patternA = BlockOuter(view, "pattern-a");
                        var patternB = BlockOuter(view, "pattern-b");
                        var patternC = BlockOuter(view, "pattern-c");
                        var patternD = BlockOuter(view, "pattern-d");
                        var patternE = BlockOuter(view, "pattern-e");
                        Need(patternA.Size == new Size(60, 20) && patternB.Size == new Size(40, 30) && patternC.Size == new Size(60, 105) && patternD.Size == new Size(40, 30) && patternE.Size == new Size(60, 20), "repeating grid auto-track geometry changed");
                        var mixA = BlockOuter(view, "auto-mix-a");
                        var mixB = BlockOuter(view, "auto-mix-b");
                        var mixC = BlockOuter(view, "auto-mix-c");
                        var mixD = BlockOuter(view, "auto-mix-d");
                        var mixE = BlockOuter(view, "auto-mix-e");
                        Need(mixA.Width == 50 && mixB.Width == 25 && mixC.Width == 50 && mixD.Width == 25 && mixE.Width == 50, "mixed auto and fraction implicit columns changed");
                        var rowCycle = BlockOuter(view, "row-cycle");
                        var rowA = BlockOuter(view, "row-cycle-a");
                        var rowC = BlockOuter(view, "row-cycle-c");
                        var rowE = BlockOuter(view, "row-cycle-e");
                        var largeGap = BlockOuter(view, "row-large-gap");
                        var largeGapA = BlockOuter(view, "row-large-gap-a");
                        var largeGapB = BlockOuter(view, "row-large-gap-b");
                        var largeGapC = BlockOuter(view, "row-large-gap-c");
                        var areaLead = BlockOuter(view, "area-lead");
                        var areaStory = BlockOuter(view, "area-story");
                        var areaAction = BlockOuter(view, "area-action");
                        Need(rowCycle.Height == 135 && rowA.Height == 35 && rowC.Height == 55 && rowE.Height == 35 && largeGap.Height == 120 && largeGapA.Height == 10 && largeGapB.Height == 20 && largeGapC.Height == 10 && largeGapB.Top - largeGapA.Bottom == 40 && areaLead.Width == 50 && areaStory.Width == 30 && areaAction.Width == 100 && ColoredSamples(image) > 0, "implicit row settling, large gaps, area tracks, or paint changed");
                    }
                }
                return "S.Av plus Gac/Gav/Rac/Rav repeating pixel, auto, integer-fr, and minmax patterns; implicit endpoints, area tracks, row settlement, validation, bounds, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded grid shorthands", delegate
            {
                var path = Path.Combine(folder, "grid-shorthand.html");
                var page = new E().Go(path);
                var explicitGrid = page.Bs.First(block => block.El != null && block.El.Av("id") == "explicit-grid").S;
                var preserved = page.Bs.First(block => block.El != null && block.El.Av("id") == "template-preserve").S;
                Need(explicitGrid.Rc == 2 && explicitGrid.Rv(0) == 30 && explicitGrid.Rv(1) == -1 && explicitGrid.Gc == 3 && explicitGrid.Gv(0) == 80 && explicitGrid.Gv(1) == -1 && explicitGrid.Gv(2) == -2 && explicitGrid.Gf == "r" && explicitGrid.Gac == 0, "explicit grid shorthand aliases changed");
                Need(preserved.Rc == 2 && preserved.Gc == 2 && preserved.Gf == "x" && preserved.Gav(0) == 25 && preserved.Rav(0) == 35, "grid-template shorthand preservation changed");
                var area = page.Bs.First(block => block.El != null && block.El.Av("id") == "area-grid").S;
                var rowFlow = page.Bs.First(block => block.El != null && block.El.Av("id") == "row-flow-grid").S;
                var columnFlow = page.Bs.First(block => block.El != null && block.El.Av("id") == "column-flow-grid").S;
                Need(area.Aa == 3 && area.Ar == 2 && area.Ac == 2 && area.Ag("head", 3) == 2 && rowFlow.Gf == "d" && rowFlow.Gc == 3 && rowFlow.Rac == 2 && columnFlow.Gf == "x" && columnFlow.Rc == 2 && columnFlow.Gac == 2, "area or auto-flow shorthand aliases changed");
                var explicitAuto = page.Bs.First(block => block.El != null && block.El.Av("id") == "explicit-auto").S;
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "grid-reset").S;
                var invalid = page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid-grid").S;
                Need(explicitAuto.Rv(0) == S.Av && explicitAuto.Gv(0) == S.Av && reset.Gc == 0 && reset.Rc == 0 && reset.Gac == 0 && reset.Gf == "r" && reset.Gp == 7 && invalid.Gc == 2 && invalid.Rc == 1 && invalid.Gac == 1 && invalid.Rac == 1 && invalid.Gf == "c", "grid shorthand auto, reset, or atomic validation changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2000);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var explicitA = BlockOuter(view, "explicit-a");
                        var explicitB = BlockOuter(view, "explicit-b");
                        var explicitC = BlockOuter(view, "explicit-c");
                        var explicitF = BlockOuter(view, "explicit-f");
                        Need(explicitA.Size == new Size(80, 30) && explicitB.Size == new Size(66, 30) && explicitC.Size == new Size(134, 30) && explicitF.Size == new Size(134, 65), "explicit grid shorthand geometry changed");
                        var areaHead = BlockOuter(view, "area-head");
                        var areaNav = BlockOuter(view, "area-nav");
                        var areaMain = BlockOuter(view, "area-main");
                        Need(areaHead.Size == new Size(300, 30) && areaNav.Size == new Size(90, 65) && areaMain.Size == new Size(200, 65), "area grid shorthand geometry changed");
                        var rowA = BlockOuter(view, "row-flow-a");
                        var rowB = BlockOuter(view, "row-flow-b");
                        var rowC = BlockOuter(view, "row-flow-c");
                        var columnA = BlockOuter(view, "column-flow-a");
                        var columnB = BlockOuter(view, "column-flow-b");
                        var columnC = BlockOuter(view, "column-flow-c");
                        var columnE = BlockOuter(view, "column-flow-e");
                        Need(rowA.Size == new Size(180, 30) && rowB.Size == new Size(180, 65) && rowC.Size == new Size(110, 30) && rowC.Top == rowA.Top && columnA.Size == new Size(40, 25) && columnB.Size == new Size(40, 70) && columnC.Size == new Size(200, 25) && columnE.Size == new Size(40, 25) && ColoredSamples(image) > 0, "auto-flow shorthand geometry or paint changed");
                    }
                }
                return "Atomic grid-template/grid explicit and quoted-area forms, row/column auto-flow dense forms, automatic patterns, reset boundaries, explicit auto tracks, geometry, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded grid item alignment", delegate
            {
                var path = Path.Combine(folder, "grid-alignment.html");
                var page = new E().Go(path);
                var place = page.Bs.First(block => block.El != null && block.El.Av("id") == "place-grid").S;
                var placeB = page.Bs.First(block => block.El != null && block.El.Av("id") == "place-b").S;
                var placeC = page.Bs.First(block => block.El != null && block.El.Av("id") == "place-c").S;
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic-items").S;
                var atomicSelf = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic-self").S;
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset-items").S;
                var resetSelf = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset-self").S;
                var two = page.Bs.First(block => block.El != null && block.El.Av("id") == "two-items").S;
                var twoSelf = page.Bs.First(block => block.El != null && block.El.Av("id") == "two-self").S;
                Need(place.Ji == "c" && place.Ai == "c" && placeB.Js == "e" && placeB.As == "e" && placeC.Js == "t" && placeC.As == "t", "grid alignment aliases changed");
                Need(atomic.Ji == "e" && atomic.Ai == "s" && atomicSelf.Js == "e" && atomicSelf.As == "s" && reset.Ji == "t" && reset.Ai == "t" && resetSelf.Js == "" && resetSelf.As == "", "grid alignment atomic/reset state changed");
                Need(two.Ai == "s" && two.Ji == "e" && twoSelf.As == "e" && twoSelf.Js == "c", "grid two-value place axis order changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1400);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var grid = BlockOuter(view, "place-grid");
                        var a = BlockOuter(view, "place-a");
                        var b = BlockOuter(view, "place-b");
                        var c = BlockOuter(view, "place-c");
                        var d = BlockOuter(view, "place-d");
                        Need(a.Size == new Size(40, 20) && a.Left - grid.Left == 50 && a.Top - grid.Top == 40 && b.Size == new Size(50, 30) && b.Left - grid.Left == 250 && b.Top - grid.Top == 70, "grid center/end alignment geometry changed");
                        Need(c.Size == new Size(140, 100) && c.Left == grid.Left && c.Top - grid.Top == 110 && d.Size == new Size(60, 40) && d.Left - grid.Left == 150 && d.Top - grid.Top == 110, "grid stretch/start alignment geometry changed");
                        var fit = BlockOuter(view, "fit-grid");
                        var fitText = BlockOuter(view, "fit-text");
                        Need(fitText.Width > 1 && fitText.Width < fit.Width && fitText.Left - fit.Left == (fit.Width - fitText.Width) / 2 && fitText.Height < fit.Height && fitText.Top - fit.Top == (fit.Height - fitText.Height) / 2, "grid auto-size alignment changed");
                        var fitClamp = BlockOuter(view, "fit-clamp-grid");
                        var fitClampText = BlockOuter(view, "fit-clamp-text");
                        Need(fitClampText.Width == 100 && fitClampText.Left == fitClamp.Left, "grid auto-fit minimum clamp changed");
                        Need(ColoredSamples(image) > 0, "grid alignment paint changed");
                    }
                }
                return "Ji/Js plus Ai/As start, center, end, and stretch defaults/overrides; atomic place-items/place-self, auto fit, exact geometry, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded grid content alignment", delegate
            {
                var path = Path.Combine(folder, "grid-content-alignment.html");
                var page = new E().Go(path);
                var center = page.Bs.First(block => block.El != null && block.El.Av("id") == "center-grid").S;
                var axis = page.Bs.First(block => block.El != null && block.El.Av("id") == "axis-grid").S;
                var between = page.Bs.First(block => block.El != null && block.El.Av("id") == "between-grid").S;
                var mixed = page.Bs.First(block => block.El != null && block.El.Av("id") == "mixed-grid").S;
                var atomic = page.Bs.First(block => block.El != null && block.El.Av("id") == "atomic-content").S;
                var reset = page.Bs.First(block => block.El != null && block.El.Av("id") == "reset-content").S;
                Need(center.Al == "c" && center.Jc == "c" && axis.Al == "e" && axis.Jc == "s", "grid content center or axis aliases changed");
                Need(between.Al == "b" && between.Jc == "b" && mixed.Al == "a" && mixed.Jc == "v", "grid content distribution aliases changed");
                Need(atomic.Al == "e" && atomic.Jc == "c" && reset.Al == "s" && reset.Jc == "s", "grid content atomic/reset state changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 2600);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        var centerGrid = BlockOuter(view, "center-grid");
                        var centerA = BlockOuter(view, "center-a");
                        var centerD = BlockOuter(view, "center-d");
                        Need(centerA.Size == new Size(80, 40) && centerA.Left - centerGrid.Left == 55 && centerA.Top - centerGrid.Top == 55 && centerD.Left - centerGrid.Left == 145 && centerD.Top - centerGrid.Top == 105, "grid centered content geometry changed");
                        var betweenGrid = BlockOuter(view, "between-grid");
                        var betweenB = BlockOuter(view, "between-b");
                        var betweenC = BlockOuter(view, "between-c");
                        Need(betweenB.Left - betweenGrid.Left == 200 && betweenC.Top - betweenGrid.Top == 160, "grid space-between content geometry changed");
                        var mixedGrid = BlockOuter(view, "mixed-grid");
                        var mixedA = BlockOuter(view, "mixed-a");
                        var mixedB = BlockOuter(view, "mixed-b");
                        var mixedC = BlockOuter(view, "mixed-c");
                        Need(mixedA.Left - mixedGrid.Left == 40 && mixedA.Top - mixedGrid.Top == 30 && mixedB.Left - mixedGrid.Left == 160 && mixedC.Top - mixedGrid.Top == 130 && ColoredSamples(image) > 0, "grid around/evenly content geometry or paint changed");
                    }
                }
                return "Al/Jc start, center, end, between, around, and evenly track-group distribution; atomic place-content, exact geometry, paint, hits, and flow";
            });
            AddProbe(probes, "Bounded inline SVG", delegate
            {
                var path = Path.Combine(folder, "svg.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "vector");
                var graphic = engine.Ax(page, ById(page.D, "vector"));
                Need(block.Vg != null && block.Vg.Vb && block.Vg.W == 320D && block.Vg.H == 180D && block.Vg.Ss.Count == 10, "SVG model or bounds changed");
                Need(block.Vg.Ss[6].K == "path" && block.Vg.Ss[6].D.Count == 5 && block.Vg.Ss[7].U.Fragment == "#svg-target", "SVG path or linked shape changed");
                Need(graphic != null && graphic.K == "img" && graphic.N == "Inline SVG status chart", "SVG accessibility mapping changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 760);
                    view.Go(path);
                    using (var image = Draw(view)) Need(BlockContent(view, "vector").Size == new Size(320, 180) && ColoredSamples(image) > 0, "SVG layout or paint changed");
                }
                return "Vg/Vs/Vp aliases, viewBox meet sizing, 128 shapes/points, six primitives, text, straight/cubic paths, inherited paint/opacity, links, accessibility, and GDI paint";
            });
            AddProbe(probes, "SVG geometry and transforms", delegate
            {
                var path = Path.Combine(folder, "svg-geometry.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "geometry");
                var shapes = block.Vg.Ss;
                Need(block.Vg.A == "mm" && block.Vg.M == "m" && shapes.Count == 10, "SVG geometry model or defaults changed");
                Need(shapes[0].D.Count == 3 && shapes[0].D[1].K == 'C' && shapes[0].D[2].K == 'C' && shapes[0].D[2].A == 50D && shapes[0].D[2].B == 60D, "SVG cubic or smooth control changed");
                Need(shapes[1].D.Count == 3 && shapes[1].D[2].K == 'Q' && shapes[1].D[2].A == 55D && shapes[1].D[2].B == 100D, "SVG quadratic or smooth control changed");
                Need(shapes[2].D.Count >= 2 && shapes[2].D.Skip(1).All(step => step.K == 'C'), "SVG arc conversion changed");
                Need(shapes[3].M[0] == 1.5D && shapes[3].M[3] == 1.5D && shapes[3].M[4] == 120D && shapes[3].M[5] == 10D, "SVG cumulative transform changed");
                Need(shapes[7].U != null && shapes[7].U.Fragment == "#geometry-target" && shapes[8].M[4] == 132D && shapes[9].M[0] == 1D, "SVG link, inline transform, or invalid fallback changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1100);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "geometry").Size == new Size(360, 200), "SVG transformed viewport size changed");
                        Need(!FragmentHit(view, "#geometry-target").IsEmpty, "SVG transformed link hit changed");
                        Need(ColorDistance(image.GetPixel(BlockContent(view, "aspect-none").Right - 4, BlockContent(view, "aspect-none").Top + 50), Color.FromArgb(220, 38, 38)) < 12, "SVG preserveAspectRatio none paint changed");
                        Need(ColorDistance(image.GetPixel(BlockContent(view, "aspect-min").Left + 50, BlockContent(view, "aspect-min").Top + 50), Color.FromArgb(22, 163, 74)) < 12, "SVG aligned meet paint changed");
                        Need(ColorDistance(image.GetPixel(BlockContent(view, "aspect-slice").Left + 100, BlockContent(view, "aspect-slice").Top + 50), Color.FromArgb(37, 99, 235)) < 12, "SVG slice clipping paint changed");
                    }
                }
                return "C/S/Q/T/A paths, inherited matrix/translate/scale/rotate/skew transforms, preserveAspectRatio none/meet/slice, clipping, accessibility links, and bounded invalid fallback";
            });
            AddProbe(probes, "SVG gradient paint servers", delegate
            {
                var path = Path.Combine(folder, "svg-gradients.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "gradient-vector");
                Need(block.Vg != null && block.Vg.Ds.Count == 5 && block.Vg.Ss.Count == 8, "SVG gradient model or invalid rejection changed");
                Need(block.Vg.Ds[0].K == "l" && block.Vg.Ds[0].Ss.Count == 3 && block.Vg.Ds[0].Ss[1].C == "#16a34a" && Math.Abs(block.Vg.Ds[0].Ss[1].A - .6D) < .001D, "SVG linear stops or CSS paint changed");
                Need(block.Vg.Ds[1].Ss.Count == 3 && block.Vg.Ds[2].U == "u" && block.Vg.Ds[3].K == "r" && block.Vg.Ds[3].P[3] == .35D, "SVG inherited, user-space, or radial gradient changed");
                Need(block.Vg.Ss[0].Fd == block.Vg.Ds[0] && block.Vg.Ss[4].Sd == block.Vg.Ds[4] && block.Vg.Ss[5].U.Fragment == "#gradient-target" && block.Vg.Ss[6].F == "none" && block.Vg.Ss[7].F == "none", "SVG gradient references or fail-closed state changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 700);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "gradient-vector").Size == new Size(360, 220) && ColoredSamples(image) > 0, "SVG gradient layout or paint changed");
                        Need(!FragmentHit(view, "#gradient-target").IsEmpty, "SVG gradient link hit changed");
                    }
                }
                return "Vg.Ds, Vs.Fd/Sd, Vd/Vt aliases, bounded local inheritance and cycles, object/user units, transforms, CSS stops/opacity/currentColor, linear/radial fill and stroke paint, links, and invalid fallback";
            });
            AddProbe(probes, "SVG reusable geometry and clips", delegate
            {
                var path = Path.Combine(folder, "svg-use-clips.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "reuse-vector");
                var shapes = block.Vg.Ss;
                Need(block.Vg != null && block.Vg.Cs.Count == 4 && shapes.Count == 14, "SVG use expansion or clip model changed");
                Need(shapes[0].M[4] == 5D && shapes[0].F == "#dc2626" && shapes[2].M[0] == 1.2D && shapes[4].M[4] == 85D && shapes[6].M[4] == 113D, "SVG group, symbol, or nested use matrix changed");
                Need(block.Vg.Cs[0].U == "u" && block.Vg.Cs[0].M[4] == 2D && block.Vg.Cs[1].U == "b" && block.Vg.Cs[2].Ss[0].M[4] == 125D && block.Vg.Cs[3].Ss.Count == 2, "SVG clip units, transform, use, or union changed");
                Need(shapes[8].Cp == block.Vg.Cs[0] && shapes[9].Cp == block.Vg.Cs[1] && shapes[10].Cp == block.Vg.Cs[2] && shapes[11].Cp == block.Vg.Cs[3] && shapes[13].Cl && shapes[13].Cp == null, "SVG clip attachment or fail-closed state changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 720);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "reuse-vector").Size == new Size(360, 240) && ColoredSamples(image) > 0, "SVG reusable clip layout or paint changed");
                        Need(!FragmentHit(view, "#clip-target").IsEmpty, "SVG clipped link hit changed");
                    }
                }
                return "Vg.Cs, Vs.Cp/Cl/Cm, Vc aliases, bounded group/symbol/nested use expansion, x/y and transforms, cycle rejection, user/object clip units, reusable clip geometry, CSS/group attachment, paint, links, and unresolved fallback";
            });
            AddProbe(probes, "SVG stroke and winding styles", delegate
            {
                var path = Path.Combine(folder, "svg-strokes.html");
                var engine = new E();
                var page = engine.Go(path);
                var block = page.Bs.First(item => item.El != null && item.El.Av("id") == "stroke-vector");
                var shapes = block.Vg.Ss;
                Need(block.Vg != null && block.Vg.Cs.Count == 1 && shapes.Count == 13, "SVG stroke fixture or model changed");
                Need(shapes[0].Fr == "e" && shapes[1].Fr == "n" && block.Vg.Cs[0].Ss[0].Fr == "e", "SVG fill or clip winding changed");
                Need(shapes[3].Lc == "b" && shapes[4].Lc == "r" && shapes[5].Lc == "s" && shapes[6].Lj == "m" && shapes[6].Ml == 64D && shapes[7].Lj == "r" && shapes[8].Lj == "b", "SVG cap, join, or bounded miter state changed");
                Need(shapes[9].S == "#0f766e" && shapes[9].W == 5D && shapes[9].Do == 3D && shapes[9].Da.Count == 6 && shapes[9].Da[0] == 8D && shapes[9].Da[3] == 8D, "SVG stylesheet dash cascade changed");
                Need(shapes[10].F == "#f97316" && shapes[10].S == "#7c3aed" && shapes[10].W == 2D && shapes[11].Da.Count == 2 && shapes[12].Da.Count == 0, "SVG important paint or inherited dash state changed");
                using (var view = NewView())
                {
                    view.Size = new Size(720, 760);
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockContent(view, "stroke-vector").Size == new Size(360, 260) && ColoredSamples(image) > 0, "SVG stroke layout or paint changed");
                        Need(!FragmentHit(view, "#stroke-target").IsEmpty, "SVG capped stroke link hit changed");
                    }
                }
                return "Vs.Fr/Lc/Lj/Ml/Da/Do aliases, bounded stylesheet/presentation/inline cascade, variables, important priority, nonzero/evenodd fill and clips, butt/round/square caps, miter/round/bevel joins, miter limits, dash repetition/offset/inheritance/clearing, paint, and links";
            });
            AddProbe(probes, "Forms and validation", delegate
            {
                var engine = new E();
                var forms = engine.Go(Path.Combine(folder, "forms.html"));
                var validation = engine.Go(Path.Combine(folder, "form-validation.html"));
                var widgets = engine.Go(Path.Combine(folder, "form-widgets.html"));
                Need(forms.Fs.Count == 9 && validation.Fs.Count == 12 && widgets.Fs.Count == 10, "field models changed");
                Need(validation.Fs[0].Rq && validation.Fs[0].Ok, "validation state is missing");
                return "31 fields across basic, validation, and richer-widget fixtures";
            });
            AddProbe(probes, "Native field edit commands", delegate
            {
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1000);
                    view.Go(Path.Combine(folder, "editing.html"));
                    using (var image = Draw(view)) { }
                    var start = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                    var field = view.At.Fs.First(item => item.El.Av("id") == "text");
                    start.Invoke(view, new object[] { field });
                    Need(view.Ce("de") && view.Ec("de") == 1 && field.V == "", "selected field delete failed");
                    Need(view.Ec("un") == 1 && field.V == "Lea field text" && !view.Ce("un") && view.Ce("re"), "native undo state failed");
                    Need(view.Ec("re") == 1 && field.V == "", "native redo failed");
                    start.Invoke(view, new object[] { view.At.Fs.First(item => item.El.Av("id") == "notes") });
                    start.Invoke(view, new object[] { view.At.Fs.First(item => item.El.Av("id") == "readonly") });
                    Need(view.Ce("cp") && !view.Ce("cu") && !view.Ce("pa"), "readonly command state failed");
                    start.Invoke(view, new object[] { view.At.Fs.First(item => item.El.Av("id") == "secret") });
                    Need(!view.Ce("cp") && !view.Ce("cu"), "password command state failed");
                }
                return "Ce/Ec two-letter commands, native one-level undo/redo, readonly/password guards, shared-font field switching; probe does not touch clipboard";
            });
            AddProbe(probes, "Bounded supplied paste", delegate
            {
                using (var view = NewView())
                {
                    view.Size = new Size(720, 1000);
                    view.Go(Path.Combine(folder, "editing.html"));
                    using (var image = Draw(view)) { }
                    var start = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                    var field = view.At.Fs.First(item => item.El.Av("id") == "limited");
                    start.Invoke(view, new object[] { field });
                    Need(view.Pa("1234567\ud83d\ude00") == 1 && field.V == "1234567", "Pa split a surrogate at maxlength");
                    Need(view.Ec("un") == 1 && field.V == "1234", "Pa lost native undo");
                    start.Invoke(view, new object[] { view.At.Fs.First(item => item.El.Av("id") == "notes") });
                    Need(view.Pa("One\r\nTwo") == 1, "Pa multiline dispatch");
                    Need(view.At.Fs.First(item => item.El.Av("id") == "notes").V == "One\r\nTwo", "Pa lost line breaks");
                    start.Invoke(view, new object[] { view.At.Fs.First(item => item.El.Av("id") == "readonly") });
                    Need(view.Pa("blocked") == 0, "Pa overwrote readonly field");
                    view.Dispose();
                    Need(view.Pa("blocked") == 0, "Pa ran after disposal");
                }
                return "Pa host-supplied text, maxlength/Unicode, multiline, native undo, protected/disposed targets; no clipboard access";
            });
            AddProbe(probes, "Named native stylesheets", delegate
            {
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "stylesheets.html"));
                    var page = view.At;
                    var field = page.Fs.First(item => item.El.Av("id") == "field");
                    field.V = "Retained stylesheet data";
                    Need(view.Ss("theme", "#panel{width:222px;background:#d7263d}") == 1 && view.Sn == 1 && view.Dg().Sn == 1, "Ss/Sn diagnostics");
                    using (var image = Draw(view)) { }
                    Need(page.Bs.First(item => item.El != null && item.El.Av("id") == "panel").S.W == 222, "injected rule did not reach renderer");
                    Need(view.Ss("theme", new string('x', 65537)) == 0 && view.Sn == 1, "oversized sheet changed retained state");
                    Need(view.Sd("theme") == 1 && view.Sn == 0 && field.V == "Retained stylesheet data" && page.Fs.Contains(field), "Sd lost retained fields");
                }
                return "Ss/Sd/Sn named sheets, native computed/paint route, atomic source bound, retained field identity, and diagnostics";
            });
            AddProbe(probes, "View load admission", delegate
            {
                using (var view = NewView())
                {
                    var ready = 0;
                    view.Done += delegate { Need(view.Cn && !view.Busy, "Done before ready"); ready++; };
                    Need(view.Cn, "new view cannot navigate");
                    var work = view.GoA(Path.Combine(folder, "load-lifecycle.html"));
                    Need(work.IsCompleted || (!view.Cn && view.Busy), "pending view load has no owner");
                    var end = DateTime.UtcNow.AddSeconds(10);
                    while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
                    Need(work.IsCompleted, "view load timeout");
                    work.GetAwaiter().GetResult();
                    Need(ready == 1 && view.Cn && !view.Busy, "completion did not restore admission");
                    view.Dispose();
                    Need(!view.Cn, "disposed view accepts navigation");
                }
                return "Cn admission, Busy through presentation, ready callback, and disposed guard; deterministic overlap/handoff regressions run in Lea.Tests --loads";
            });
            AddProbe(probes, "Bounded picture retention", delegate
            {
                using (var view = NewView())
                {
                    var work = view.GoA(Path.Combine(folder, "image-loading.html"));
                    var end = DateTime.UtcNow.AddSeconds(10);
                    while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
                    Need(work.IsCompleted, "picture fixture timeout");
                    work.GetAwaiter().GetResult();
                    using (var image = Draw(view)) { }
                    Need(view.Ic == 1 && view.Ib == 4 && view.At.Fs[0].V == "Retained image data", "deduplicated picture accounting or field data changed");
                    view.Go(Path.Combine(folder, "second.html"));
                    Need(view.Ic == 0 && view.Ib == 0, "unreferenced image memory retained");
                }
                return "Ic/Ib bitmap count/bytes, shared embedded source, retained fields, native paint, and page replacement; streaming/deadline/cache-limit regressions run in Lea.Tests --pics";
            });
            AddProbe(probes, "Native image preflight", delegate
            {
                using (var view = NewView())
                {
                    var work = view.GoA(Path.Combine(folder, "image-preflight.html"));
                    var end = DateTime.UtcNow.AddSeconds(10);
                    while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
                    Need(work.IsCompleted, "image preflight fixture timeout");
                    work.GetAwaiter().GetResult();
                    using (var image = Draw(view)) Need(ColoredSamples(image) > 0, "image preflight fixture is blank");
                    Need(view.Ip == 8388608 && view.Im == 128L * 1024 * 1024 && view.Ic == 1 && view.If == 1 &&
                        view.In == 1 && view.Ib == 4 && view.It == 4 && view.Ii && File.Exists(view.Ie) &&
                        view.Iw > 0 && view.Iw != System.Diagnostics.Process.GetCurrentProcess().Id && view.Ik == 0 && view.Ij &&
                        view.Ih == 192L * 1024 * 1024 && view.Ipm > 0 && view.Ipm <= view.Ih && view.Irt && view.Ili &&
                        view.Isp && view.Ihl && view.Ias && view.Icp && view.Icr == 2500 &&
                        (view.Img & 3) == 3 && view.Ipk > 0 && view.Ipk <= view.Ih &&
                        view.At.Fs[0].V == "Retained preflight data",
                        "image restricted-launch aliases, worker admission, or retained accounting changed");
                }
                using (var released = NewView()) Need(released.It == 0, "image preflight fixture leaked process reservation");
                return "Pi preflight, Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk hardened-worker aliases, one native decode, one pre-native rejection, retained fields, paint, and disposal";
            });
            AddProbe(probes, "Bounded document input", delegate
            {
                using (var view = NewView())
                {
                    var work = view.GoA(Path.Combine(folder, "input-loading.html"));
                    var end = DateTime.UtcNow.AddSeconds(10);
                    while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
                    Need(work.IsCompleted, "input fixture timeout");
                    work.GetAwaiter().GetResult();
                    using (var image = Draw(view)) { }
                    Need(view.Hm == 4194304 && view.Csm == 262144 && view.Cn && !view.Busy && view.Le == null,
                        "input limit aliases or completion changed");
                    Need(view.At.S.Bg == "#f4f6f8" && view.At.Fs[0].V == "Retained bounded input", "linked CSS or retained input changed");
                }
                return "Hm/Csm document/sheet ceilings, local asynchronous input, linked CSS, native paint, and retained fields; wire/decoded/deadline/origin regressions run in Lea.Tests --input";
            });
            AddProbe(probes, "Exact cache identity", delegate
            {
                var path = Path.Combine(folder, "cache-identity.html");
                var page = new E().Go(path);
                Need(page.Ls.Count == 4, "cache identity fixture links changed");
                var keys = typeof(E).GetMethod("CacheKey", BindingFlags.Static | BindingFlags.NonPublic);
                Need(keys != null, "cache key helper is unavailable");
                var first = (string)keys.Invoke(null, new object[] { page.Ls[0].U });
                var second = (string)keys.Invoke(null, new object[] { page.Ls[1].U });
                var third = (string)keys.Invoke(null, new object[] { page.Ls[2].U });
                var fragment = (string)keys.Invoke(null, new object[] { page.Ls[3].U });
                Need(first != second && second != third && third == fragment,
                    "path/query case or fragment representation identity changed");
                Need(page.Ls[0].U.AbsolutePath.EndsWith("/Case/Resource.html", StringComparison.Ordinal) &&
                    page.Ls[1].U.AbsolutePath.EndsWith("/case/Resource.html", StringComparison.Ordinal) &&
                    page.Ls[1].U.Query == "?Mode=Upper" && page.Ls[2].U.Query == "?Mode=upper",
                    "authored URL case was not retained");
                using (var view = NewView())
                {
                    view.Go(path);
                    using (var image = Draw(view)) Need(view.Bc > 0 && ColoredSamples(image) > 0, "cache identity fixture is blank");
                }
                return "ordinal path/query keys after URI canonicalization, fragment stripping, authored link case, native layout, and network/disk regressions in Lea.Tests --cache";
            });
            AddProbe(probes, "Bounded Lea scripting", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "events-vars.html"));
                Need(page.T == "Lea safe events" && page.Vs.Count == 2, "page commands did not run");
                Need(engine.Ck(page, ById(page.D, "go")) == 3 && page.T == "Clicked Lea", "click commands did not run");
                return page.Jc + " safe commands and " + page.Vs.Count + " variables after click";
            });
            AddProbe(probes, "Literal DOM compatibility", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "dom-bridge.html"));
                var status = ById(page.D, "status");
                var name = page.Fs.First(item => item.El != null && item.El.Av("id") == "name");
                Need(page.Jc == 11 && status.C[0].X == "DOM ready" && name.V == "DOM value", "initial DOM assignments changed");
                Need(page.Bs.First(block => Object.ReferenceEquals(block.El, status)).S.Bg == "#e0f2fe" && page.Bs.First(block => Object.ReferenceEquals(block.El, status)).S.Fs == 19, "DOM style assignments changed");
                Need(page.Fs.First(item => item.El.Av("id") == "agree").C && page.Fs.First(item => item.El.Av("id") == "locked").Ds && ById(page.D, "more").Op, "DOM state assignments changed");
                Need(Object.ReferenceEquals(engine.Kp(page), name.El) && engine.Ck(page, ById(page.D, "apply")) == 9, "DOM focus or onclick translation changed");
                Need(status.C[0].X == "Clicked DOM" && name.V == "Clicked value" && status.Av("class").Length == 0, "DOM onclick mutation changed");
                Need(engine.Js(page, "document.getElementById('status').innerHTML='bad'; document.getElementById(id).textContent='bad'; document.getElementById('status').style.visibility='hidden';") == 0, "unsupported DOM syntax was accepted");
                return "getElementById literal text, value, booleans, classList, focus, three styles, 20 accepted commands, and rejected arbitrary DOM APIs";
            });
            AddProbe(probes, "Display and location compatibility", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "display-navigation.html"));
                Need(!page.Bs.Any(block => block.El != null && block.El.Av("id") == "panel") &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid").S.D == "grid", "authored display filtering changed");
                Need(engine.Ck(page, ById(page.D, "show")) == 1 && page.Bs.First(block => block.El != null && block.El.Av("id") == "panel").S.D == "block", "compact display command failed");
                Need(engine.Ck(page, ById(page.D, "hide")) == 1 && !page.Bs.Any(block => block.El != null && block.El.Av("id") == "panel"), "DOM display none failed");
                Need(engine.Ck(page, ById(page.D, "flex")) == 1 && page.Bs.First(block => block.El != null && block.El.Av("id") == "row").S.D == "flex", "DOM flex display failed");
                Need(engine.Ck(page, ById(page.D, "clear")) == 1 && page.Bs.First(block => block.El != null && block.El.Av("id") == "row").S.D == "block" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "row").S.F == "#123f8c", "DOM display reset lost the cascade");
                Need(engine.Ck(page, ById(page.D, "href")) == 1 && engine.Gt(page) == new Uri(new Uri(Path.Combine(folder, "go-target.html")).AbsoluteUri + "#href"), "location href request changed");
                Need(engine.Ck(page, ById(page.D, "assign")) == 1 && engine.Gt(page) == new Uri(new Uri(Path.Combine(folder, "go-target.html")).AbsoluteUri + "#assign"), "location assign request changed");
                Need(engine.Js(page, "location='go-target.html?mode=one#both'") == 1 && engine.Gt(page) == new UriBuilder(new Uri(Path.Combine(folder, "go-target.html"))) { Query = "mode=one", Fragment = "both" }.Uri, "direct location query or fragment changed");
                Need(engine.Js(page, "dp('row','contents'); location.href=target; location.replace('go-target.html');") == 0 && engine.Gt(page) == null, "display or location boundary changed");
                return "dp plus literal style.display, empty reset, retained inline style, validated modes, href/assign mapping, and existing guarded go requests";
            });
            AddProbe(probes, "Percentage and calculated widths", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "calculated-widths.html"));
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var outer = block("outer");
                var minimum = block("minimum");
                var maximum = block("maximum");
                Need(outer != null && outer.S.Wp == 100 && outer.S.Wo == -60, "calculated parent width model changed");
                Need(minimum != null && minimum.S.Wp == 20 && minimum.S.Wnp == 50 && minimum.S.Wno == -10, "calculated minimum width model changed");
                Need(maximum != null && maximum.S.Wp == 100 && maximum.S.Wxp == 75 && maximum.S.Wxo == 5, "calculated maximum width model changed");
                Need(block("invalid").S.W == 240 && block("variable").S.Wp == 75 && block("inherit-child").S.Wp == 60, "width cascade, variable, or inheritance changed");
                Need(engine.Ck(page, ById(page.D, "set-width")) == 1 && block("command").S.Wp == 100 && block("command").S.Wo == -120, "compact wd command failed");
                Need(engine.Ck(page, ById(page.D, "dom-width")) == 1 && block("command").S.Wp == 50, "literal DOM width command failed");
                Need(engine.Ck(page, ById(page.D, "clear-width")) == 1 && block("command").S.W == 280 && block("command").S.Wp == 0 && block("command").S.F == "#123f8c", "width reset lost authored or inline state");
                Need(engine.Js(page, "wd('command','min(max(100px,200px),400px)'); document.getElementById('command').style.width='101%';") == 0 && block("command").S.W == 280, "nested or out-of-range width expression was accepted");
                return "wd plus literal style.width, 1..100 percent, calc(P% +/- 0..2000px), min/max, variables, inheritance, reset, and strict expression bounds";
            });
            AddProbe(probes, "Percentage and calculated heights", delegate
            {
                var path = Path.Combine(folder, "calculated-heights.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var minimum = block("minimum");
                var maximum = block("maximum");
                Need(block("viewport").S.Hp == 50 && block("minus").S.Ho == -80 && block("plus").S.Ho == 20, "calculated height model changed");
                Need(minimum.S.Hp == 20 && minimum.S.Hnp == 50 && minimum.S.Hno == -10, "calculated minimum height model changed");
                Need(maximum.S.Hp == 100 && maximum.S.Hxp == 75 && maximum.S.Hxo == 5, "calculated maximum height model changed");
                Need(block("invalid").S.H == 140 && block("variable").S.Hp == 75 && block("inherit-child").S.Hp == 60, "height cascade, variable, or inheritance changed");
                using (var view = new V { Size = new Size(760, 1000) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    var outer = BlockContent(view, "outer");
                    Need(outer.Height == 400 && BlockContent(view, "half").Height == 200 && BlockContent(view, "minus").Height == 320, "calculated block height geometry changed");
                    Need(BlockContent(view, "flex-row-a").Height == 120 && BlockContent(view, "flex-column-b").Height == 140 && BlockContent(view, "sized-table").Height == 180,
                        "calculated flex or table height geometry changed");
                    Need(BlockContent(view, "absolute-height").Height == 150 && BlockContent(view, "fixed-height").Height == 250 &&
                        BlockContent(view, "height-field").Height == 100 && BlockContent(view, "height-image").Height == 100,
                        "calculated positioned, field, or image height geometry changed");
                    Need(engine.Ck(page, ById(page.D, "set-height")) == 1 && block("command").S.Hp == 100 && block("command").S.Ho == -120, "compact ht command failed");
                    Need(view.Ck(ById(view.At.D, "set-height")) == 1, "view compact ht command failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "command").Height == 280, "compact ht reflow changed");
                }
                Need(engine.Ck(page, ById(page.D, "dom-height")) == 1 && block("command").S.Hp == 50, "literal DOM height command failed");
                Need(engine.Ck(page, ById(page.D, "clear-height")) == 1 && block("command").S.H == 180 && block("command").S.Hp == 0 && block("command").S.F == "#123f8c", "height reset lost authored or inline state");
                Need(engine.Js(page, "ht('command','min(max(100px,200px),400px)'); document.getElementById('command').style.height='101%';") == 0 && block("command").S.H == 180, "nested or out-of-range height expression was accepted");
                return "ht plus literal style.height, definite viewport/parent resolution, auto fallback, flex/grid/table/aspect-ratio geometry, min/max, variables, inheritance, reset, and strict bounds";
            });
            AddProbe(probes, "Bounded viewport units", delegate
            {
                var path = Path.Combine(folder, "viewport-units.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                Need(block("vw").S.Wp == 50 && block("vw").S.Wu == 1 && block("vh-width").S.Wu == 2 &&
                    block("vmin-width").S.Wu == 3 && block("vmax-width").S.Wu == 4, "viewport width unit model changed");
                Need(block("vh").S.Hp == 50 && block("vh").S.Hu == 2 && block("vw-height").S.Hu == 1 &&
                    block("vmin-height").S.Hu == 3 && block("vmax-height").S.Hu == 4, "viewport height unit model changed");
                Need(block("calc-width").S.Wo == -20 && block("calc-height").S.Ho == -20 &&
                    block("minimum-width").S.Wnu == 1 && block("maximum-height").S.Hxu == 2, "viewport calc or min/max model changed");
                Need(block("inherit-child").S.Wu == 3 && block("inherit-child").S.Hu == 4 &&
                    block("variable").S.Wu == 4 && block("variable").S.Hu == 3 && block("invalid").S.W == 140 && block("invalid").S.H == 90,
                    "viewport inheritance, variable, or atomic fallback changed");
                using (var view = new V { Size = new Size(800, 600) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "vw").Width == 400 && BlockContent(view, "vh-width").Width == 150 &&
                        BlockContent(view, "vh").Height == 300 && BlockContent(view, "vw-height").Height == 200,
                        "viewport primary or cross-axis geometry changed");
                    Need(BlockContent(view, "vmin-width").Width == 60 && BlockContent(view, "vmax-height").Height == 80 &&
                        BlockContent(view, "auto-viewport-child").Height == 150, "vmin/vmax or auto-parent geometry changed");
                    Need(BlockContent(view, "flex-a").Size == new Size(200, 150) && BlockContent(view, "grid-a").Size == new Size(200, 150) &&
                        BlockContent(view, "viewport-table").Size == new Size(400, 150), "viewport flex, grid, or table geometry changed");
                    Need(view.Ck(ById(view.At.D, "set-viewport")) == 2, "viewport compact commands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "command").Size == new Size(380, 280), "viewport compact command reflow changed");
                    view.Size = new Size(640, 400);
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "vw").Width == 320 && BlockContent(view, "vh").Height == 200 &&
                        BlockContent(view, "fixed").Size == new Size(40, 64), "viewport resize geometry changed");
                }
                Need(engine.Js(page, "wd('command','101vw'); ht('command','20dvh')") == 0, "unsupported viewport values were accepted");
                return "vw/vh/vmin/vmax plus calc offsets, cross-axis sizing, lazy unit aliases, min/max, variables, inheritance, layout families, commands, and live resize";
            });
            AddProbe(probes, "Bounded font-relative dimensions", delegate
            {
                var path = Path.Combine(folder, "font-relative-units.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                Need(page.Rf == 20 && block("em-size").S.Wu == 5 && block("em-size").S.Hu == 5 &&
                    block("rem-size").S.Wu == 6 && block("rem-size").S.Hu == 6, "font-relative unit or root-font model changed");
                Need(block("calc-font").S.Wo == -20 && block("calc-font").S.Ho == 10 &&
                    block("minimum-font-width").S.Wnu == 5 && block("maximum-root-height").S.Hxu == 6,
                    "font-relative calc or min/max model changed");
                Need(block("font-inherit-child").S.Wu == 5 && block("font-inherit-child").S.Hu == 6 &&
                    block("font-variable").S.Wu == 5 && block("font-variable").S.Hu == 6 &&
                    block("font-invalid").S.W == 130 && block("invalid-font-size").S.Fs == 18,
                    "font-relative inheritance, variable, or atomic fallback changed");
                using (var view = new V { Size = new Size(800, 600) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    Need(view.Rf == 20 && BlockContent(view, "em-size").Size == new Size(180, 72) &&
                        BlockContent(view, "rem-size").Size == new Size(200, 80) && BlockContent(view, "calc-font").Size == new Size(160, 110),
                        "font-relative primary or calculated geometry changed");
                    Need(BlockContent(view, "font-flex-a").Size == new Size(120, 60) &&
                        BlockContent(view, "font-grid-a").Size == new Size(200, 60) && BlockContent(view, "font-table").Size == new Size(400, 108),
                        "font-relative flex, grid, or table geometry changed");
                    Need(view.Ck(ById(view.At.D, "set-font-size")) == 2, "font-relative compact commands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "font-command").Size == new Size(140, 100), "font-relative command reflow changed");
                    Need(view.Ck(ById(view.At.D, "clear-font-size")) == 2 && view.Ck(ById(view.At.D, "grow-font")) == 1,
                        "font-relative reset or font mutation command failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "font-command").Size == new Size(200, 120) && BlockContent(view, "font-mutable").Size == new Size(200, 80),
                        "font-relative reset or font mutation geometry changed");
                    var field = view.At.Fs.First(item => item.El != null && item.El.Av("id") == "font-field");
                    field.V = "Retained benchmark font value";
                    view.Size = new Size(640, 500);
                    using (var image = Draw(view)) { }
                    Need(view.Rf == 16 && BlockContent(view, "rem-size").Size == new Size(160, 64) &&
                        BlockContent(view, "em-size").Size == new Size(180, 72), "responsive rem/em geometry changed");
                    Need(Object.ReferenceEquals(field, view.At.Fs.First(item => item.El != null && item.El.Av("id") == "font-field")) &&
                        field.V == "Retained benchmark font value", "root-font rebuild lost field state");
                }
                Need(engine.Js(page, "wd('font-command','101em'); ht('font-command','1.5rem')") == 0,
                    "unsupported font-relative values were accepted");
                return "em/rem dimensions plus root-font state, calc offsets, lazy aliases, min/max, variables, inheritance, layout families, commands, font mutation, and responsive rebuild";
            });
            AddProbe(probes, "Bounded CSS size functions", delegate
            {
                var path = Path.Combine(folder, "size-functions.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var minimum = block("min-two").S;
                var clamp = block("clamp-main").S;
                Need(minimum.Wk == 1 && minimum.Hk == 1 && minimum.Wq(0) == 2 && minimum.Wv(0, 0, 1) == 75 && minimum.Wv(0, 1, 0) == 500,
                    "minimum function model changed");
                Need(clamp.Wk == 3 && clamp.Hk == 3 && clamp.Wq(0) == 3 && clamp.Wv(0, 1, 1) == 50 &&
                    block("max-three").S.Wk == 2 && block("max-three").S.Wq(0) == 3, "maximum or clamp model changed");
                Need(block("function-limits").S.Wnk == 2 && block("function-limits").S.Wxk == 1 &&
                    block("function-inherit-child").S.Wk == 3 && block("function-variable").S.Hk == 3 &&
                    block("function-invalid").S.W == 130 && block("function-invalid").S.Wk == 0,
                    "function constraint, inheritance, variable, or fallback model changed");
                using (var view = new V { Size = new Size(800, 600) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "min-two").Size == new Size(450, 260) &&
                        BlockContent(view, "max-two").Size == new Size(280, 140) &&
                        BlockContent(view, "clamp-main").Size == new Size(300, 180) &&
                        BlockContent(view, "clamp-font").Size == new Size(360, 80), "primary size function geometry changed");
                    Need(BlockContent(view, "function-limits").Size == new Size(480, 300) &&
                        BlockContent(view, "function-flex-a").Size == new Size(120, 60) &&
                        BlockContent(view, "function-grid-a").Size == new Size(200, 90) &&
                        BlockContent(view, "function-table").Size == new Size(500, 100), "constraint or layout-family function geometry changed");
                    Need(view.Ck(ById(view.At.D, "set-function-size")) == 2, "compact size function commands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "function-command").Size == new Size(350, 70), "compact size function command reflow changed");
                    Need(view.Ck(ById(view.At.D, "clear-function-size")) == 2, "size function reset failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "function-command").Size == new Size(220, 120) &&
                        BlockContent(view, "live-function").Size == new Size(400, 60) &&
                        BlockContent(view, "media-function").Size == new Size(320, 80), "size function reset, live, or media geometry changed");
                    var field = view.At.Fs.First(item => item.El != null && item.El.Av("id") == "function-field");
                    field.V = "Retained benchmark function value";
                    view.Size = new Size(640, 500);
                    using (var image = Draw(view)) { }
                    Need(view.Rf == 16 && BlockContent(view, "live-function").Size == new Size(320, 50) &&
                        BlockContent(view, "media-function").Size == new Size(384, 80),
                        "responsive size function geometry changed");
                    Need(view.At.Bs.First(item => item.El != null && item.El.Av("id") == "media-function").S.Wk == 2,
                        "responsive size function record was not replaced");
                    Need(Object.ReferenceEquals(field, view.At.Fs.First(item => item.El != null && item.El.Av("id") == "function-field")) &&
                        field.V == "Retained benchmark function value", "size function rebuild lost field state");
                }
                Need(engine.Js(page, "wd('function-command','min(100px)'); ht('function-command','clamp(20px,30px)')") == 0,
                    "unsupported size functions were accepted");
                return "min/max with two or three bounded terms, three-term clamp, lazy Wk/Hk and Wq/Hq/Wv/Hv aliases, mixed units/calc, min/max properties, layout families, commands, media replacement, and resize";
            });
            AddProbe(probes, "Bounded logical sizing and insets", delegate
            {
                var path = Path.Combine(folder, "logical-sizing-insets.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var size = block("logical-size").S;
                Need(size.Wp == 50 && size.Wo == 20 && size.Wn == 180 && size.Wx == 240 &&
                    size.Hp == 50 && size.Hn == 120 && size.Hx == 160, "logical size record mapping changed");
                Need(block("order-physical").S.W == 140 && block("order-logical").S.W == 120 &&
                    block("important").S.W == 110 && block("important").S.H == 80, "logical source order or important cascade changed");
                var axis = block("axis-position").S;
                var invalid = block("invalid-position").S;
                Need(axis.Lt == 24 && axis.Rt == 30 && axis.Tp == 18 && axis.Bm == 26 &&
                    invalid.Lt == 11 && invalid.Rt == 13 && invalid.Tp == 19 && invalid.Bm == 21,
                    "logical axis inset mapping or atomic fallback changed");
                using (var view = new V { Size = new Size(760, 900) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "logical-size").Size == new Size(220, 150) &&
                        BlockContent(view, "variable").Size == new Size(285, 75) &&
                        BlockContent(view, "inherit-child").Size == new Size(144, 48),
                        "logical size geometry changed");
                    var shell = BlockContent(view, "position-shell");
                    var placed = BlockContent(view, "axis-position");
                    Need(placed.Left - shell.Left == 24 && placed.Top - shell.Top == 18 && placed.Size == new Size(346, 196),
                        "logical inset geometry changed");
                    Need(view.Ck(ById(view.At.D, "set-logical")) == 2, "short logical size commands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "command").Height == 120, "logical command view reflow changed");
                    Need(view.Ck(ById(view.At.D, "dom-logical")) == 2 && view.Ck(ById(view.At.D, "clear-logical")) == 2,
                        "familiar logical DOM style or reset failed");
                    using (var image = Draw(view)) { }
                    Need(BlockContent(view, "command").Size == new Size(210, 90), "logical DOM reset geometry changed");
                }
                Need(engine.Ck(page, ById(page.D, "set-logical")) == 2 && block("command").S.Wp == 50 && block("command").S.H == 120,
                    "short logical aliases did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "dom-logical")) == 2 && block("command").S.Wp == 25 && block("command").S.H == 70,
                    "familiar logical DOM styles did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "clear-logical")) == 2 && block("command").S.W == 210 && block("command").S.H == 90,
                    "logical DOM reset lost authored state");
                Need(engine.Js(page, "is('command','101%');bs('command','1.5em')") == 0 && block("command").S.W == 210,
                    "unsupported logical command values were accepted");
                return "horizontal-tb/LTR inline/block size mapping, min/max, source order, important, variables, inheritance, logical inset axes/longhands, atomic fallback, is/bs, literal DOM styles, shared geometry, and reset";
            });
            AddProbe(probes, "Bounded logical margins and padding", delegate
            {
                var path = Path.Combine(folder, "logical-box-model.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var axis = block("axis-box").S;
                Need(axis.Mt == 14 && axis.Mr == 18 && axis.Mb == 16 && axis.Ml == 12 &&
                    axis.Pt == 6 && axis.Pr == 10 && axis.Pb == 7 && axis.Pl == 8,
                    "logical box axis mapping changed");
                var longhand = block("longhand-box").S;
                Need(longhand.Mt == 23 && longhand.Mr == 22 && longhand.Mb == 24 && longhand.Ml == 21 &&
                    longhand.Pt == 6 && longhand.Pr == 5 && longhand.Pb == 7 && longhand.Pl == 4,
                    "logical box longhand mapping changed");
                var order = block("source-logical").S;
                var invalid = block("invalid").S;
                var bounded = block("bounded").S;
                Need(order.Mt == 16 && order.Mr == 15 && order.Mb == 17 && order.Ml == 14 && order.Pt == 4 && order.Pr == 3 && order.Pb == 5 && order.Pl == 2,
                    "logical box source order changed");
                Need(invalid.Mt == 14 && invalid.Mr == 13 && invalid.Mb == 15 && invalid.Ml == -22 && invalid.Pt == 6 && invalid.Pr == 9 && invalid.Pb == 11 && invalid.Pl == 8,
                    "logical box atomic fallback changed");
                Need(bounded.Ml == -2000 && bounded.Mr == 2000 && bounded.Pt == 2000 && bounded.Pb == 2000,
                    "logical box bounds changed");
                using (var view = new V { Size = new Size(760, 900) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    var shell = BlockContent(view, "geometry-shell");
                    var content = BlockContent(view, "axis-box");
                    var outer = BlockOuter(view, "axis-box");
                    Need(content.Size == new Size(100, 40) && outer.Size == new Size(118, 53) &&
                        outer.Left - shell.Left == 32 && outer.Top - shell.Top == 44 && content.Left - shell.Left == 40 && content.Top - shell.Top == 50,
                        "logical box shared geometry changed");
                    Need(view.Ck(ById(view.At.D, "set-axis")) == 4, "short logical box axis commands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "command").Size == new Size(138, 53), "logical box command reflow changed");
                    Need(view.Ck(ById(view.At.D, "dom-edges")) == 8 && view.Ck(ById(view.At.D, "clear-box")) == 12,
                        "familiar logical box styles or reset failed");
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "command").Size == new Size(125, 43), "logical box DOM reset geometry changed");
                }
                Need(engine.Ck(page, ById(page.D, "set-axis")) == 4 && block("command").S.Ml == 12 && block("command").S.Pt == 6,
                    "short logical box axes did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "set-edges")) == 8 && block("command").S.Mt == 23 && block("command").S.Pl == 4,
                    "short logical box edges did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "dom-axis")) == 4 && engine.Ck(page, ById(page.D, "dom-edges")) == 8 &&
                    block("command").S.Ml == 17 && block("command").S.Pb == 12,
                    "familiar logical box styles did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "clear-box")) == 12 && block("command").S.Ml == 4 && block("command").S.Pb == 2,
                    "logical box reset lost authored state");
                Need(engine.Js(page, "mi('command','1px 2px 3px');pi('command','-1px');mis('command','2px 3px');mie('command','auto')") == 0,
                    "unsupported logical box command values were accepted");
                return "horizontal-tb/LTR logical margin/padding axes and longhands, signed bounded margins, atomic parsing, source order, important, variables, inheritance, reset, twelve compact/readable commands, familiar DOM styles, and shared geometry";
            });
            AddProbe(probes, "Bounded logical borders", delegate
            {
                var path = Path.Combine(folder, "logical-borders.html");
                var engine = new E();
                var page = engine.Go(path);
                Func<string, B> block = id => page.Bs.First(item => item.El != null && item.El.Av("id") == id);
                var axis = block("axis").S;
                Need(axis.Bt == 6 && axis.Br == 4 && axis.Bb == 6 && axis.Bl == 4 && axis.Bsv(0) == "dashed" && axis.Bsv(1) == "solid" &&
                    axis.Bcv(0) == "#176b4d" && axis.Bcv(3) == "#123f8c", "logical border axis mapping changed");
                var longhand = block("longhand").S;
                Need(longhand.Bt == 4 && longhand.Br == 3 && longhand.Bb == 5 && longhand.Bl == 2 &&
                    longhand.Bsv(0) == "dashed" && longhand.Bsv(2) == "double" && longhand.Bcv(1) == "#176b4d" && longhand.Bcv(3) == "#a13b18",
                    "logical border longhand mapping changed");
                var physical = block("physical").S;
                Need(physical.Bt == 1 && physical.Br == 2 && physical.Bb == 3 && physical.Bl == 4 &&
                    physical.Bsv(1) == "dashed" && physical.Bcv(2) == "#333333", "physical border component lists changed");
                var invalid = block("invalid").S;
                Need(invalid.Br == 7 && invalid.Bl == 8 && invalid.Bsv(0) == "dashed" && invalid.Bsv(3) == "dotted" &&
                    invalid.Bcv(1) == "#176b4d" && invalid.Bcv(3) == "#a13b18", "logical border atomic fallback changed");
                var inherited = block("inherit-child").S;
                Need(inherited.Bt == 2 && inherited.Br == 3 && inherited.Bb == 4 && inherited.Bl == 5 && inherited.Bsv(0) == "dotted" && inherited.Bcv(3) == "#b7791f",
                    "logical border inheritance changed");
                using (var view = new V { Size = new Size(760, 900) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view))
                    {
                        Need(BlockOuter(view, "axis").Size == new Size(108, 52) && BlockOuter(view, "hidden").Size == new Size(109, 24),
                            "logical border layout changed");
                        var rounded = BlockOuter(view, "rounded");
                        Need(ColorDistance(image.GetPixel(rounded.Left + 2, rounded.Top + rounded.Height / 2), Color.FromArgb(161, 59, 24)) < 20 &&
                            ColorDistance(image.GetPixel(rounded.Left + rounded.Width / 2, rounded.Bottom - 2), Color.FromArgb(183, 121, 31)) < 20,
                            "mixed rounded logical border paint changed");
                    }
                    Need(view.Ck(ById(view.At.D, "set-short")) == 6, "short logical border shorthands failed");
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "command").Size == new Size(137, 61), "logical border shorthand reflow changed");
                    Need(view.Ck(ById(view.At.D, "set-components")) == 18, "short logical border components failed");
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "command").Size == new Size(153, 77), "logical border component reflow changed");
                    Need(view.Ck(ById(view.At.D, "dom-all")) == 24 && view.Ck(ById(view.At.D, "clear-all")) == 24,
                        "familiar logical border styles or reset failed");
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "command").Size == new Size(128, 48), "logical border reset geometry changed");
                }
                Need(engine.Ck(page, ById(page.D, "set-short")) == 6 && block("command").S.Bl == 6 && block("command").S.Bt == 8,
                    "short logical border shorthands did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "set-components")) == 18 && block("command").S.Bl == 14 && block("command").S.Bb == 17,
                    "short logical border components did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "dom-all")) == 24 && block("command").S.Bl == 21 && block("command").S.Bt == 23,
                    "familiar logical border styles did not reach compact records");
                Need(engine.Ck(page, ById(page.D, "clear-all")) == 24 && block("command").S.Bl == 2 && block("command").S.Bsv(3) == "solid" && block("command").S.Bec == 0,
                    "logical border reset lost authored state");
                Need(engine.Js(page, "biw('command','2px bad');biy('command','solid groove');bic('command','red #xyz');siw('command','wide')") == 0,
                    "unsupported logical border command values were accepted");
                Need(engine.Js(page, "bi('command','THICK SOLID RED')") == 1 && block("command").S.Bl == 5 && block("command").S.Bsv(3) == "solid",
                    "case-insensitive logical border keywords changed");
                return "horizontal-tb/LTR logical border axes and edges, physical per-edge components, bounded atomic parsing, source order, important, variables, inheritance/reset, twenty-four compact/readable commands, familiar DOM styles, mixed rounded paint, and shared geometry";
            });
            AddProbe(probes, "Bounded sticky positioning", delegate
            {
                var path = Path.Combine(folder, "sticky-positioning.html");
                var page = new E().Go(path);
                var stickyBlock = page.Bs.First(block => block.El != null && block.El.Av("id") == "sticky");
                var railBlock = page.Bs.First(block => block.El != null && block.El.Av("id") == "rail");
                Need(stickyBlock.S.Pn == "k" && stickyBlock.S.Tp == 14 && Object.ReferenceEquals(stickyBlock.Up, railBlock), "sticky style or containing block changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "bottom").S.Bm == 12 &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "invalid").S.Pn == "r", "bottom inset or atomic position fallback changed");
                using (var view = new V { Size = new Size(720, 300) })
                {
                    view.CreateControl();
                    view.Go(path);
                    using (var image = Draw(view)) { }
                    var before = BlockOuter(view, "sticky");
                    var childBefore = BlockOuter(view, "sticky-child");
                    var follower = BlockOuter(view, "follower");
                    view.Sc(360);
                    using (var image = Draw(view)) { }
                    var moved = BlockOuter(view, "sticky");
                    var child = BlockOuter(view, "sticky-child");
                    var hit = ElementHit(view, "sticky-link");
                    var bounds = view.Ab(view.Ax(ById(view.At.D, "sticky-link")));
                    Need(moved.Top == view.Sy + 14 && child.Top - moved.Top == childBefore.Top - before.Top &&
                        follower == BlockOuter(view, "follower"), "sticky shared geometry or normal flow changed");
                    Need(!hit.IsEmpty && !bounds.IsEmpty && bounds.IntersectsWith(hit), "sticky hit or accessibility geometry changed");
                    view.Sc(Int32.MaxValue);
                    using (var image = Draw(view)) { }
                    Need(BlockOuter(view, "sticky").Bottom == BlockContent(view, "rail").Bottom, "sticky containing-block clamp changed");
                }
                return "Pn=k, signed top/bottom state, normal flow, shared descendants/hits/accessibility, root scroll pinning, and parent clamp";
            });
            AddProbe(probes, "Host-controlled go requests", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "go-command.html"));
                var target = new Uri(Path.Combine(folder, "go-target.html"));
                Need(engine.Ck(page, ById(page.D, "relative")) == 1, "relative go command failed");
                Need(engine.Gt(page) == target && engine.Gt(page) == null, "go request was not resolved and consumed once");
                Need(engine.Ck(page, ById(page.D, "unsafe")) == 0 && engine.Gt(page) == null, "unsafe go scheme was accepted");
                return "Relative file and HTTPS requests accepted; one pending request; unsafe schemes rejected";
            });
            AddProbe(probes, "Bounded network timeout", delegate
            {
                var engine = new E();
                Need(engine.Tm == 15000, "default timeout changed");
                engine.Tm = 1;
                Need(engine.Tm == 100, "minimum timeout changed");
                engine.Tm = Int32.MaxValue;
                Need(engine.Tm == 60000, "maximum timeout changed");
                return "Tm defaults to 15000 ms and clamps to 100-60000 ms";
            });
            AddProbe(probes, "Bounded persistent cache", delegate
            {
                var path = Path.Combine(Path.GetTempPath(), "lea-bench-cache-" + Guid.NewGuid().ToString("N"));
                try
                {
                    using (var server = new CacheServer())
                    {
                        var defaults = new E();
                        Need(defaults.Cd == "" && defaults.Cx == 32 && defaults.Cb == 16L * 1024 * 1024, "persistent cache defaults changed");
                        var freshUrl = new Uri(server.U, "?fresh=1");
                        var seed = new E { Cd = path, Cm = 0 };
                        var first = seed.Go(freshUrl);
                        Need(!first.Ca && seed.Dc == 1 && seed.Db > 0 && seed.Cc == 0, "persistent seed was not stored");
                        var requests = server.Hits;
                        var restarted = new E { Cd = path, Cm = 0 };
                        var hit = restarted.Go(freshUrl);
                        Need(hit.Ca && hit.H == first.H && server.Hits == requests && restarted.Dg(hit).Dh == 1, "persistent restart hit failed");

                        var etagUrl = new Uri(server.U, "?etag=1");
                        new E { Cd = path }.Go(etagUrl);
                        requests = server.Hits;
                        var validator = new E { Cd = path };
                        var validated = validator.Go(etagUrl);
                        Need(validated.Ca && server.Hits == requests + 1 && server.LastIfNoneMatch == "\"lea-bench\"", "persistent ETag validation failed");
                        Need(validator.Dg(validated).Rs.Any(line => line.Contains(" disk304 ")), "persistent validator diagnostics are missing");
                        return "Cd opt-in; 32 entries/16 MB defaults; restart hit; ETag disk304; Dc/Db/Dh diagnostics";
                    }
                }
                finally
                {
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                }
            });
            AddProbe(probes, "Bounded class mutations", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "class-commands.html"));
                Need(page.Jc == 7 && engine.Q(page, "#panel.hot") != null && engine.Q(page, "#remove.old") == null, "page class or scoped style commands changed");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "panel").S.F == "#0f4c81", "page scoped foreground command changed");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                field.V = "Preserved benchmark value";
                Need(engine.Js(page, "rc('panel','hot'); ac('field','hot'); tc('toggle','hot');") == 3, "host class commands changed");
                Need(engine.Js(page, "fg('panel','#334155'); bg('toggle','#e0f2fe'); fs('remove',24);") == 3, "host scoped style commands changed");
                Need(engine.Q(page, "#panel.hot") == null && engine.Q(page, "#field.hot") != null, "class selectors were not rebuilt");
                Need(page.Bs.First(block => block.El != null && block.El.Av("id") == "panel").S.F == "#334155" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "toggle").S.Bg == "#e0f2fe" &&
                    page.Bs.First(block => block.El != null && block.El.Av("id") == "remove").S.Fs == 24, "scoped style rebuild changed");
                Need(engine.Js(page, "fg('missing','#fff'); bg('unsafe-style','#fff'); fs('panel',97);") == 0, "unsafe scoped style command was accepted");
                Need(field.V == "Preserved benchmark value", "class rebuild replaced mutable field state");
                return "ac/rc/tc plus one/two-argument bg/fg/fs restyle compiled CSS with bounded IDs, colors, 8-96 px fonts, 32 classes, and retained fields";
            });
            AddProbe(probes, "Bounded element-state commands", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "state-commands.html"));
                var name = page.Fs.First(item => item.El != null && item.El.Av("id") == "name");
                Need(page.Jc == 9 && name.V == "Page ready", "initial state commands changed");
                Need(page.Fs.First(item => item.El.Av("id") == "pick").V == "two" && page.Fs.First(item => item.El.Av("id") == "agree").C, "value or checked command changed");
                Need(page.Fs.First(item => item.El.Av("id") == "locked").Ds && Object.ReferenceEquals(engine.Kp(page), name.El), "disable or focus request changed");
                Need(engine.Ck(page, ById(page.D, "apply")) == 7 && name.V == "Clicked Lea", "state click commands changed");
                Need(!page.Fs.First(item => item.El.Av("id") == "locked").Ds && page.Fs.First(item => item.El.Av("id") == "choice-a").C, "enable or radio state changed");
                engine.Kp(page);
                Need(engine.Ck(page, ById(page.D, "hide-field")) == 3 && name.V == "Changed while hidden", "hidden retained field state changed");
                Need(engine.Ck(page, ById(page.D, "show-field")) == 1 && Object.ReferenceEquals(name, page.Fs.First(item => item.El.Av("id") == "name")), "shown field identity changed");
                var details = ById(page.D, "advanced");
                var detailValue = page.Fs.First(item => item.El.Av("id") == "advanced-value");
                detailValue.V = "Retained benchmark detail";
                Need(details.Op && engine.Js(page, "cl('advanced-summary'); op('advanced'); tg('advanced-summary'); op('advanced')") == 4 && details.Op, "disclosure state commands changed");
                Need(Object.ReferenceEquals(detailValue, page.Fs.First(item => item.El.Av("id") == "advanced-value")) && detailValue.V == "Retained benchmark detail", "disclosure command rebuild replaced field state");
                Need(engine.Js(page, "vl('upload','private.txt'); vl('color','red'); fo('panel')") == 0, "unsafe state value or target was accepted");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "state-commands.html"));
                    Need(view.Ke != null && view.Ke.Av("id") == "name", "view did not consume initial focus request");
                    Need(view.Js("cl('advanced-summary')") == 1 && !ById(view.At.D, "advanced").Op, "view disclosure close command failed");
                    Need(view.Js("op('advanced')") == 1 && ById(view.At.D, "advanced").Op, "view disclosure open command failed");
                    Need(view.Js("ds('name')") == 1 && view.Ke == null, "view retained a disabled focus target");
                    Need(view.Js("en('name'); fo('name')") == 2 && view.Ke != null && view.Ke.Av("id") == "name", "view did not restore command focus");
                }
                return "hd/sh, ds/en, vl, ck, fo, op/cl/tg, retained fields, one pending focus target, and file-value rejection";
            });
            AddProbe(probes, "Find and local download", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "find-download.html"));
                Need(engine.Fd(page, "cobalt") == 3, "find count changed");
                var source = Path.Combine(folder, "download-sample.txt");
                var target = Path.Combine(Path.GetTempPath(), "lea-report-" + Guid.NewGuid().ToString("N") + ".txt");
                try
                {
                    var download = engine.Dl(source, target);
                    Need(download.St == 200 && File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(target)), "download bytes changed");
                }
                finally { File.Delete(target); }
                return "3 text matches and atomic local download";
            });
            AddProbe(probes, "Bounded diagnostics", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "events-vars.html"));
                var snapshot = engine.Dg(page);
                Need(snapshot.Ns.Count <= 512 && snapshot.Cs.Count <= 256 && snapshot.Rs.Count <= 32, "diagnostic bounds changed");
                Need(snapshot.Rs.Count == 1 && snapshot.Vs.Count == 2, "diagnostic evidence is incomplete");
                return snapshot.Ns.Count + " node lines, " + snapshot.Cs.Count + " CSS lines, " + snapshot.Rs.Count + " request";
            });
            AddProbe(probes, "Retained field data", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "retained-data.html"));
                Need(!engine.Ud(page), "pristine defaults retained unnecessarily");
                var field = page.Fs.First(item => item.El.Av("id") == "text");
                field.V = "Keep this value";
                Need(engine.Ud(page), "changed field not retained");
                engine.Js(page, "hd('text')");
                Need(!page.Fs.Contains(field) && engine.Ud(page), "hidden field data lost from query");
                field.V = field.D;
                Need(!engine.Ud(page), "restored value did not clear query");
                using (var view = NewView())
                {
                    view.Go(Path.Combine(folder, "retained-data.html"));
                    Need(!view.Ud, "native Ud default");
                    view.At.Fs.First(item => item.K == "file").Fp.Add("not-read.txt");
                    Need(view.Ud, "selected file not retained");
                }
                return "E.Ud/V.Ud; pristine choices, edited and hidden retained fields, selected files, read-only bounded query";
            });
            AddProbe(probes, "Native load failure and retry", delegate
            {
                var missing = Path.Combine(Path.GetTempPath(), "lea-probe-retry-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    using (var view = NewView())
                    {
                        view.Go(Path.Combine(folder, "forms.html"));
                        var retained = view.At;
                        var notifications = 0;
                        view.Fe += delegate { notifications++; };
                        Er failure = null;
                        try { view.Go(missing); } catch (Er error) { failure = error; }
                        Need(failure != null && failure == view.Le && view.Cr && notifications == 1 && view.At == retained, "failed GET state changed");
                        File.WriteAllText(missing, "<title>Retry probe</title><p>Available</p>");
                        var work = view.Ry();
                        var end = DateTime.UtcNow.AddSeconds(5);
                        while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
                        Need(work.IsCompleted, "retry timed out");
                        work.GetAwaiter().GetResult();
                        Need(view.Le == null && !view.Cr && view.T == "Retry probe" && notifications == 1, "successful retry state changed");
                    }
                    return "Fe/Le typed failure, retained page, Cr/Ry explicit GET retry, and success reset";
                }
                finally { if (File.Exists(missing)) File.Delete(missing); }
            });
            return probes;
        }

        private static string Report(int iterations, double[] startup, IList<LoadResult> loads, RenderResult render,
            MemoryResult memory, IList<Probe> probes, IList<Artifact> artifacts)
        {
            var allLoadTimes = loads.SelectMany(item => item.Times).ToArray();
            var runtime = artifacts.Where(item => item.Runtime).ToList();
            var runtimeBytes = runtime.Sum(item => item.Bytes);
            var gzipBytes = Gzip(runtime.Select(item => item.Path));
            var builder = new StringBuilder();
            builder.AppendLine("# Lea Compatibility and Performance Report");
            builder.AppendLine();
            builder.AppendLine("- Generated: **" + DateTime.UtcNow.ToString("yyyy-MM-dd 'UTC'") + "**");
            builder.AppendLine("- Runtime: **.NET " + Environment.Version + ", " + (Environment.Is64BitProcess ? "64-bit" : "32-bit") + "**");
            builder.AppendLine("- Method: **" + startup.Length + " clean-process starts; " + iterations + " loads per fixture; " + Math.Max(3, iterations / 3) + " render rounds at " + ViewWidth + "x" + ViewHeight + "**");
            builder.AppendLine();
            builder.AppendLine("These are repeatable local diagnostic samples, not performance promises. Re-run on the target computer after meaningful engine changes.");
            builder.AppendLine();
            builder.AppendLine("## Current Snapshot");
            builder.AppendLine();
            builder.AppendLine("| Measurement | Result |");
            builder.AppendLine("| --- | ---: |");
            builder.AppendLine("| Clean process + engine + first page median | " + F(startup) + " ms |");
            builder.AppendLine("| Clean process + engine + first page p95 | " + P95(startup).ToString("0.000") + " ms |");
            builder.AppendLine("| Local page load median | " + F(allLoadTimes) + " ms |");
            builder.AppendLine("| Local page load p95 | " + P95(allLoadTimes).ToString("0.000") + " ms |");
            builder.AppendLine("| Layout + bitmap paint median | " + F(render.Times) + " ms |");
            builder.AppendLine("| Layout + bitmap paint p95 | " + P95(render.Times).ToString("0.000") + " ms |");
            builder.AppendLine("| Retained managed memory | " + Kb(memory.Bytes) + " KB for " + memory.Pages + " loaded pages |");
            builder.AppendLine("| Retained managed memory per page | " + Kb(memory.Pages == 0 ? 0 : memory.Bytes / memory.Pages) + " KB |");
            builder.AppendLine("| Engine runtime files | " + Kb(runtimeBytes) + " KB |");
            builder.AppendLine("| Engine runtime GZip estimate | " + Kb(gzipBytes) + " KB (" + Ratio(gzipBytes, runtimeBytes) + ") |");
            builder.AppendLine();
            builder.AppendLine("## Fixture Loads");
            builder.AppendLine();
            builder.AppendLine("Each sample includes local file input, HTML parsing, CSS compilation, document modelling, and engine bookkeeping.");
            builder.AppendLine();
            builder.AppendLine("| Fixture | Bytes | Nodes | CSS rules | Median | p95 |");
            builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: |");
            foreach (var load in loads)
                builder.AppendLine("| `" + load.Name + "` | " + load.Bytes + " | " + load.Nodes + " | " + load.CssRules + " | " + F(load.Times) + " ms | " + P95(load.Times).ToString("0.000") + " ms |");
            builder.AppendLine();
            builder.AppendLine("## Executable Probes");
            builder.AppendLine();
            builder.AppendLine("| Feature group | Result | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (var probe in probes)
                builder.AppendLine("| " + probe.Name + " | " + (probe.Pass ? "PASS" : "FAIL") + " | " + Md(probe.Detail) + " |");
            builder.AppendLine();
            builder.AppendLine("## Compatibility Scope");
            builder.AppendLine();
            builder.AppendLine("| Area | Status | Lea 1.0 scope |");
            builder.AppendLine("| --- | --- | --- |");
            Capability(builder, "Local files and HTTP/HTTPS", "Supported", "Bounded navigation, redirects, cache, cookies, compression, private mode, and typed errors");
            Capability(builder, "Optional persistent cache", "Supported", "Memory-only default, gzip entries, LRU entry/byte ceilings, validators, private isolation, and corrupt-entry recovery");
            Capability(builder, "HTML documents", "Partial", "Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, lists, disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard");
            Capability(builder, "CSS", "Partial", "Compiled relationship, bounded strict has, is/where/list-not, An+B/reverse structural, and compound of-S filtered selectors, declarations with two-tier important cascade, inherited custom properties and var() fallback, screen/all pixel-width media queries, fixed/percentage/calc dimensions and classic vw/vh/vmin/vmax plus bounded em/rem units, two/three-term min/max and three-term clamp size functions, content/border box, circular/elliptical radii, bounded outlines, box shadows and one-layer linear/radial gradients, object fit/position, bounded aspect ratios, inherited cursor/pointer interaction, bounded text transform, signed integer letter/word spacing and text styles, visibility, absolute/fixed and bounded vertical sticky positioning, transforms, responsive image sizing, bounded row/column flex, and bounded two-axis grid");
            Capability(builder, "Layout and painting", "Partial", "Custom WinForms content/border box, normalized rounded backgrounds/borders/outlines/overflow/fields/shadows/gradients, invariant transformed and spacing-aware presentation text with raw form retention, shared line-edge and inline-boundary tracking metrics, fitted raster images and maps, shared aspect-ratio sizing, pointer-gated retained hits with underlying fallthrough, inline, row/column flex, bounded grid, table, form, inline vector, clipping, scroll, and stacking renderer");
            Capability(builder, "Forms", "Supported subset", "GET/POST, validation, text/select/choice/file/range/date/color/time-family controls");
            Capability(builder, "Text overflow", "Supported subset", "Non-inherited clip/ellipsis; paint-only bounded text-element prefixes for clipped overflowing horizontal LTR lines, inline runs and tables; retained layout/link targets and full logical automation text; no native-control, bidi, custom-marker, or line-clamp additions");
            Capability(builder, "Lea command scripting", "Supported", "Bounded title, body/ID-scoped color, font, display, width, and height, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, safe click commands, and literal compatibility translation");
            Capability(builder, "Literal DOM bridge", "Supported subset", "Literal document.getElementById IDs with textContent/innerText, value, checked/disabled/hidden/open, focus(), classList add/remove/toggle, and background/color/fontSize/display/width/height; literal location href/assign requests; mapped to bounded Lea aliases");
            Capability(builder, "General JavaScript and DOM APIs", "Not supported", "Lea deliberately does not execute arbitrary JavaScript");
            Capability(builder, "Flexbox", "Supported subset", "Row/row-reverse and fixed-height column/column-reverse containers, content/border-box pixel bases, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee");
            Capability(builder, "CSS Grid", "Supported subset", "Grid/inline-grid with up to eight fixed-pixel, auto, or integer-fr tracks per explicit axis, simple repeat and minmax(0, Nfr), pixel gaps, bounded occupancy, two-axis numeric starts/spans, validated 8x8 named-area templates, unique explicit named lines, area-generated start/end lines, Gf row/column sparse/dense flow, repeating S.Av/Gac/Gav/Rac/Rav pixel/auto/integer-fr implicit patterns, definite endpoint/span growth, area-created track sizing, exact auto-height row settlement, atomic grid-template/grid explicit, quoted-area, and row/column auto-flow shorthands, Ji/Js/Ai/As start/center/end/stretch item alignment, atomic place-items/place-self, Al/Jc start/center/end/between/around/evenly track-group alignment, atomic place-content, and shared nested paint/hit/reflow; no escaped or repeated-name occurrence indexes, reverse flow, pre-explicit implicit tracks, shorthand area-row line names, intrinsic or percentage tracks, auto-fit/fill, baseline or safe/unsafe alignment, writing-mode axis reversal, or subgrid");
            Capability(builder, "SVG and Canvas", "SVG supported subset; Canvas not supported", "Bounded inline SVG with intrinsic width/height, preserveAspectRatio none/meet/slice mapping, groups/links, local shape/group/symbol use expansion, user/object clip paths, rect/circle/ellipse/line/polyline/polygon/text, M/L/H/V/Z/C/S/Q/T/A paths, cumulative matrix/translate/scale/rotate/skew transforms, linear/radial object-bounds or user-space gradients with bounded stops/inheritance/transforms, stylesheet/presentation/inline fill/stroke/opacity/font/clip paint, nonzero/evenodd fill and clip winding, caps, joins, miter limits, bounded dash patterns/offsets, currentColor, graphic accessibility, clipped paint, and transformed stroke-bounds hits; 128 shapes/points, 64 sources, 16 transforms/gradients/stops/clips/dash values, eight levels, and 256 text characters. No repeat/reflect spread, patterns, nested clips, masks, filters, markers, vector effects, animation, scripting, external references, symbol viewBox sizing, nested SVG, or Canvas");
            Capability(builder, "Audio and video", "Not supported", "Future optional module");
            Capability(builder, "Developer diagnostics", "Supported", "Bounded summary, node, CSS, request, message, and variable snapshots");
            Capability(builder, "Document queries", "Supported subset", "Id, first-match Q/Q1, and read-only Qa with compiled selectors and a 256-result ceiling");
            Capability(builder, "Document base URLs", "Supported", "First valid bounded HTTP, HTTPS, or file base; U remains the response URL and Bu drives relative resources");
            Capability(builder, "Same-page fragments", "Supported", "Bounded Ft/Fg/Rv lookup, retained 128-entry history, details reveal, no-fetch Back/Forward, and Fa/Fj/Fy initial placement across first paint, resize, and intrinsic images with manual-scroll release");
            Capability(builder, "Progress and meter gauges", "Supported", "Compact Vm values, finite normalization, meter quality regions, inherited styles, table cells, and custom paint");
            Capability(builder, "Output and data values", "Supported", "Compact Dv values, bounded references and text, host aliases, inline/block/table ownership, and safe rebuilds");
            Capability(builder, "Time and edit metadata", "Supported", "Compact Dv t/i/r values, bounded datetime and citation aliases, safe schemes, native decorations, inline/block/table ownership, and safe rebuilds");
            Capability(builder, "Image maps", "Supported subset", "Compact Mp/Ar models, rect/circle/poly/default and dead areas, bounded coordinates, object-fit-aware scaled and cropped hit regions, area links/downloads, and safe rebuilds");
            Capability(builder, "Keyboard focus", "Supported subset", "Bounded tabindex order, links, image-map areas, fields, summaries, visible focus, scrolling, wrapping, and host activation aliases");
            Capability(builder, "Focus shortcuts", "Supported subset", "Bounded autofocus candidates and ASCII access-key alternatives with conflict resolution, Alt+Shift labels, host-safe matching, focus, and activation");
            Capability(builder, "Accessibility", "Supported subset", "Bounded Ax hierarchy with MSAA and native UI Automation fragments, roles, names, live protected state, rendered bounds, focus, invoke/value/toggle/selection/disclosure/range/scroll and Tb/Tc-backed grid/table/item patterns, plus read-only logical TextPattern document/child/point ranges with visual-line and partial-range geometry; visible-range arrays use an empty fallback, and this is not the complete platform mapping standards");
            builder.AppendLine();
            builder.AppendLine("## Binary Footprint");
            builder.AppendLine();
            builder.AppendLine("| Artifact | Bytes | Size |");
            builder.AppendLine("| --- | ---: | ---: |");
            foreach (var artifact in artifacts)
                builder.AppendLine("| `" + artifact.Name + "` | " + artifact.Bytes + " | " + Kb(artifact.Bytes) + " KB |");
            builder.AppendLine();
            builder.AppendLine("The engine runtime size is `Lea.Engine.dll`, `Lea.Win.dll`, and the required `Lea.Image.exe` decoder helper. Test and report executables are listed separately and are not required when embedding Lea in Firespider.");
            builder.AppendLine();
            builder.AppendLine("## Re-run");
            builder.AppendLine();
            builder.AppendLine("From the repository root after a Release build:");
            builder.AppendLine();
            builder.AppendLine("```powershell");
            builder.AppendLine(".\\Lea.Bench\\bin\\Release\\Lea.Bench.exe -r . -o .\\LeaForge\\COMPATIBILITY_PERFORMANCE.md -n " + iterations);
            builder.AppendLine("```");
            return builder.ToString();
        }

        private static IList<Artifact> ArtifactPaths(string root, string browser)
        {
            var enginePath = Path.Combine(root, "Lea.Engine", "bin", "Release", "Lea.Engine.dll");
            var winPath = Path.Combine(root, "Lea.Win", "bin", "Release", "Lea.Win.dll");
            var imagePath = Path.Combine(root, "Lea.Image", "bin", "Release", "Lea.Image.exe");
            var winTestPath = Path.Combine(root, "Lea.Win.Test", "bin", "Release", "Lea.Win.Test.exe");
            var benchPath = Path.Combine(root, "Lea.Bench", "bin", "Release", "Lea.Bench.exe");
            if (!File.Exists(enginePath)) enginePath = Path.Combine(root, "Lea.Engine.dll");
            if (!File.Exists(winPath)) winPath = Path.Combine(root, "Lea.Win.dll");
            if (!File.Exists(imagePath)) imagePath = Path.Combine(root, "Lea.Image.exe");
            if (!File.Exists(winTestPath)) winTestPath = Path.Combine(root, "Lea.Win.Test.exe");
            if (!File.Exists(benchPath)) benchPath = Path.Combine(root, "Lea.Bench.exe");
            var candidates = new List<Artifact>
            {
                MakeArtifact("Lea.Engine.dll", enginePath, true),
                MakeArtifact("Lea.Win.dll", winPath, true),
                MakeArtifact("Lea.Image.exe", imagePath, true),
                MakeArtifact("Lea.Win.Test.exe", winTestPath, false),
                MakeArtifact("Lea.Bench.exe", benchPath, false)
            };
            var browserPath = browser;
            if (String.IsNullOrWhiteSpace(browserPath))
            {
                var local = Path.Combine(root, "Firespider Internet 19.exe");
                var known = @"L:\Firespider Internet\Firespider Internet 19.exe";
                if (File.Exists(local)) browserPath = local;
                else if (File.Exists(known)) browserPath = known;
            }
            if (!String.IsNullOrWhiteSpace(browserPath) && File.Exists(browserPath))
                candidates.Add(MakeArtifact("Firespider Internet 19.exe", browserPath, false));
            return candidates.Where(item => item != null).ToList();
        }

        private static Artifact MakeArtifact(string name, string path, bool runtime)
        {
            if (!File.Exists(path)) return null;
            return new Artifact { Name = name, Path = Path.GetFullPath(path), Bytes = new FileInfo(path).Length, Runtime = runtime };
        }

        private static long Gzip(IEnumerable<string> paths)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true))
                    foreach (var path in paths)
                    {
                        var bytes = File.ReadAllBytes(path);
                        gzip.Write(bytes, 0, bytes.Length);
                    }
                return output.Length;
            }
        }

        private static V NewView()
        {
            var view = new V { Size = new Size(ViewWidth, ViewHeight) };
            view.CreateControl();
            return view;
        }

        private static void LoadView(V view, string path, string name)
        {
            var work = view.GoA(path);
            var end = DateTime.UtcNow.AddSeconds(10);
            while (!work.IsCompleted && DateTime.UtcNow < end) { Application.DoEvents(); Thread.Sleep(2); }
            Need(work.IsCompleted, name + " timeout");
            work.GetAwaiter().GetResult();
        }

        private static Bitmap Draw(V view)
        {
            var image = new Bitmap(view.Width, view.Height);
            view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            return image;
        }

        private static int ColoredSamples(Bitmap image)
        {
            var count = 0;
            for (var y = 0; y < image.Height; y += 12)
                for (var x = 0; x < image.Width; x += 12)
                {
                    var color = image.GetPixel(x, y);
                    if (color.A != 0 && (color.R < 248 || color.G < 248 || color.B < 248)) count++;
                }
            return count;
        }

        private static int ColorDistance(Color left, Color right)
        {
            return Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
        }

        private static void Move(V view, Rectangle rect)
        {
            var method = typeof(V).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Mouse move route unavailable");
            method.Invoke(view, new object[] { new MouseEventArgs(MouseButtons.None, 0, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, 0) });
        }

        private static Rectangle ElementHit(V view, string id)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values != null)
                foreach (var value in values)
                {
                    var type = value.GetType();
                    var element = type.GetField("El").GetValue(value) as N;
                    if (element != null && element.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase))
                        return (Rectangle)type.GetField("R").GetValue(value);
                }
            return Rectangle.Empty;
        }

        private static Rectangle FragmentHit(V view, string fragment)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values != null)
                foreach (var value in values)
                {
                    var type = value.GetType();
                    var url = type.GetField("U").GetValue(value) as Uri;
                    if (url != null && url.Fragment.Equals(fragment, StringComparison.OrdinalIgnoreCase))
                        return (Rectangle)type.GetField("R").GetValue(value);
                }
            return Rectangle.Empty;
        }

        private static Rectangle BlockOuter(V view, string id)
        {
            var field = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IEnumerable;
            if (values != null)
                foreach (var value in values)
                {
                    var type = value.GetType();
                    var block = type.GetField("B").GetValue(value) as B;
                    if (block != null && block.El != null && block.El.Av("id") == id && !(bool)type.GetField("Inline").GetValue(value))
                        return (Rectangle)type.GetField("R").GetValue(value);
                }
            throw new InvalidOperationException("Missing layout block " + id);
        }

        private static Rectangle BlockContent(V view, string id)
        {
            var field = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IEnumerable;
            if (values != null)
                foreach (var value in values)
                {
                    var type = value.GetType();
                    var block = type.GetField("B").GetValue(value) as B;
                    if (block != null && block.El != null && block.El.Av("id") == id && !(bool)type.GetField("Inline").GetValue(value))
                        return (Rectangle)type.GetField("C").GetValue(value);
                }
            throw new InvalidOperationException("Missing content block " + id);
        }

        private static int EllipsisCount(V view, string id)
        {
            var values = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) as System.Collections.IEnumerable;
            var count = 0;
            foreach (var value in values)
            {
                var type = value.GetType();
                var element = type.GetField("El").GetValue(value) as N;
                if (element != null && element.Av("id") == id && (string)type.GetField("X").GetValue(value) == "" &&
                    (string)type.GetField("Px").GetValue(value) == "\u2026") count++;
            }
            return count;
        }

        private static Rectangle InlineBounds(V view, string id)
        {
            var field = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IEnumerable;
            var result = Rectangle.Empty;
            if (values != null)
                foreach (var value in values)
                {
                    var type = value.GetType();
                    var block = type.GetField("B").GetValue(value) as B;
                    if (block == null || block.El == null || block.El.Av("id") != id || !(bool)type.GetField("Inline").GetValue(value)) continue;
                    var rect = (Rectangle)type.GetField("R").GetValue(value);
                    result = result.IsEmpty ? rect : Rectangle.Union(result, rect);
                }
            if (result.IsEmpty) throw new InvalidOperationException("Missing inline layout block " + id);
            return result;
        }

        private static N ById(N node, string id)
        {
            if (node == null) return null;
            if (String.Equals(node.Av("id"), id, StringComparison.OrdinalIgnoreCase)) return node;
            foreach (var child in node.C)
            {
                var found = ById(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static void AddProbe(ICollection<Probe> probes, string name, Func<string> check)
        {
            try { probes.Add(new Probe { Name = name, Pass = true, Detail = check() }); }
            catch (Exception error) { probes.Add(new Probe { Name = name, Pass = false, Detail = error.Message }); }
        }

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static string FindRoot(string start)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(String.IsNullOrWhiteSpace(start) ? Directory.GetCurrentDirectory() : start));
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Lea.sln")) || (
                    File.Exists(Path.Combine(directory.FullName, "Lea.Engine.dll")) &&
                    File.Exists(Path.Combine(directory.FullName, "Lea.Win.dll")) &&
                    (Directory.Exists(Path.Combine(directory.FullName, "test-pages")) ||
                    Directory.Exists(Path.Combine(directory.FullName, "LeaForge", "test-pages"))))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not find a Lea source or runtime root. Pass it with -r.");
        }

        private static string Pages(string root)
        {
            var source = Path.Combine(root, "LeaForge", "test-pages");
            return Directory.Exists(source) ? source : Path.Combine(root, "test-pages");
        }

        private static string F(double[] values) { return Median(values).ToString("0.000"); }
        private static double Median(double[] values)
        {
            if (values == null || values.Length == 0) return 0;
            var sorted = values.OrderBy(value => value).ToArray();
            var middle = sorted.Length / 2;
            return sorted.Length % 2 == 0 ? (sorted[middle - 1] + sorted[middle]) / 2 : sorted[middle];
        }
        private static double P95(double[] values)
        {
            if (values == null || values.Length == 0) return 0;
            var sorted = values.OrderBy(value => value).ToArray();
            return sorted[Math.Min(sorted.Length - 1, (int)Math.Ceiling(sorted.Length * .95) - 1)];
        }
        private static string Kb(long bytes) { return (bytes / 1024d).ToString("0.0"); }
        private static string Ratio(long compressed, long raw) { return raw == 0 ? "n/a" : (compressed * 100d / raw).ToString("0.0") + "% of raw"; }
        private static string Md(string value) { return (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " "); }
        private static void Capability(StringBuilder builder, string area, string status, string scope)
        {
            builder.AppendLine("| " + area + " | " + status + " | " + scope + " |");
        }

        private sealed class CacheServer : IDisposable
        {
            private readonly TcpListener listener;
            private readonly Thread thread;
            private volatile bool stopping;
            private int hits;
            private string lastIfNoneMatch = "";

            public Uri U { get; private set; }
            public int Hits { get { return Interlocked.CompareExchange(ref hits, 0, 0); } }
            public string LastIfNoneMatch { get { return lastIfNoneMatch; } }

            public CacheServer()
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                U = new Uri("http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/page");
                thread = new Thread(Serve) { IsBackground = true };
                thread.Start();
            }

            private void Serve()
            {
                while (!stopping)
                {
                    TcpClient client;
                    try { client = listener.AcceptTcpClient(); }
                    catch (SocketException) { if (stopping) return; throw; }
                    catch (ObjectDisposedException) { return; }
                    using (client)
                    using (var stream = client.GetStream())
                    {
                        string request;
                        var validator = "";
                        using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
                        {
                            request = reader.ReadLine() ?? "";
                            string line;
                            while ((line = reader.ReadLine()) != null && line.Length > 0)
                                if (line.StartsWith("If-None-Match:", StringComparison.OrdinalIgnoreCase)) validator = line.Substring(14).Trim();
                        }
                        lastIfNoneMatch = validator;
                        Interlocked.Increment(ref hits);
                        var etag = request.IndexOf("etag=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (etag && validator == "\"lea-bench\"")
                        {
                            Write(stream, "HTTP/1.1 304 Not Modified\r\nCache-Control: max-age=60\r\nETag: \"lea-bench\"\r\nContent-Length: 0\r\nConnection: close\r\n\r\n", null);
                            continue;
                        }
                        var body = Encoding.UTF8.GetBytes("<html><head><title>Lea cache benchmark</title></head><body>Persistent cache.</body></html>");
                        var cache = etag ? "Cache-Control: max-age=0\r\nETag: \"lea-bench\"\r\n" : "Cache-Control: max-age=60\r\n";
                        Write(stream, "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\n" + cache + "Content-Length: " + body.Length + "\r\nConnection: close\r\n\r\n", body);
                    }
                }
            }

            private static void Write(Stream stream, string headers, byte[] body)
            {
                var head = Encoding.ASCII.GetBytes(headers);
                stream.Write(head, 0, head.Length);
                if (body != null) stream.Write(body, 0, body.Length);
            }

            public void Dispose()
            {
                stopping = true;
                listener.Stop();
                thread.Join(2000);
            }
        }

        private sealed class LoadResult
        {
            public string Name;
            public long Bytes;
            public int Nodes;
            public int CssRules;
            public double[] Times;
        }

        private sealed class RenderResult
        {
            public double[] Times;
            public int Boxes;
            public int Height;
            public int ColoredSamples;
        }

        private sealed class MemoryResult
        {
            public int Pages;
            public long Bytes;
        }

        private sealed class Probe
        {
            public string Name;
            public bool Pass;
            public string Detail;
        }

        private sealed class Artifact
        {
            public string Name;
            public string Path;
            public long Bytes;
            public bool Runtime;
        }

        private sealed class Options
        {
            public string Root;
            public string Output;
            public string Browser;
            public int Iterations = 15;
            public bool StartupOnly;

            public static Options Read(string[] args)
            {
                var options = new Options();
                for (var i = 0; i < args.Length; i++)
                {
                    var key = args[i].ToLowerInvariant();
                    if (key == "-s") options.StartupOnly = true;
                    else if (key == "-r" && i + 1 < args.Length) options.Root = args[++i];
                    else if (key == "-o" && i + 1 < args.Length) options.Output = args[++i];
                    else if (key == "-b" && i + 1 < args.Length) options.Browser = args[++i];
                    else if (key == "-n" && i + 1 < args.Length)
                    {
                        int value;
                        if (!Int32.TryParse(args[++i], out value) || value < 3 || value > 100)
                            throw new ArgumentException("-n must be from 3 to 100.");
                        options.Iterations = value;
                    }
                    else throw new ArgumentException("Unknown option: " + args[i] + ". Use -r root -o report -n iterations -b browser.");
                }
                return options;
            }
        }
    }
}
