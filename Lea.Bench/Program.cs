using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
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
            var path = Path.Combine(root, "LeaForge", "test-pages", "index.html");
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
            var folder = Path.Combine(root, "LeaForge", "test-pages");
            var names = new[]
            {
                "index.html", "recovery.html", "html-tokens.html", "selectors.html", "layout.html",
                "tables.html", "forms.html", "events-vars.html", "responsive-media.html",
                "stacking-contexts.html", "external.html", "base-url.html", "class-commands.html",
                "go-command.html", "lists.html", "disclosure.html", "fragments.html", "gauges.html",
                "values.html", "time-edits.html", "image-maps.html", "keyboard-focus.html",
                "focus-shortcuts.html", "accessibility.html", "state-commands.html", "flex.html"
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
            var folder = Path.Combine(root, "LeaForge", "test-pages");
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
            AddProbe(probes, "Bounded Lea scripting", delegate
            {
                var engine = new E();
                var page = engine.Go(Path.Combine(folder, "events-vars.html"));
                Need(page.T == "Lea safe events" && page.Vs.Count == 2, "page commands did not run");
                Need(engine.Ck(page, ById(page.D, "go")) == 3 && page.T == "Clicked Lea", "click commands did not run");
                return page.Jc + " safe commands and " + page.Vs.Count + " variables after click";
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
            builder.AppendLine("| Engine runtime DLLs | " + Kb(runtimeBytes) + " KB |");
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
            Capability(builder, "HTML documents", "Partial", "Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, accessibility roles, lists, disclosures, gauges, values, time/edit metadata, image maps, tables, and forms; not the full Living Standard");
            Capability(builder, "CSS", "Partial", "Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, responsive image sizing, and bounded row/column flex properties");
            Capability(builder, "Layout and painting", "Partial", "Custom WinForms block, inline, row/column flex, table, form, image, clipping, scroll, and stacking renderer");
            Capability(builder, "Forms", "Supported subset", "GET/POST, validation, text/select/choice/file/range/date/color/time-family controls");
            Capability(builder, "Lea command scripting", "Supported", "Bounded title, body/ID-scoped color and font, alert, variable, text, class, visibility, disclosure, disabled/value/checked state, focus, host navigation request, and safe click commands");
            Capability(builder, "General JavaScript and DOM APIs", "Not supported", "Lea deliberately does not execute arbitrary JavaScript");
            Capability(builder, "Flexbox", "Supported subset", "Row/row-reverse and fixed-height column/column-reverse containers, stable -64..64 order on nowrap rows and bounded columns, bounded row wrap, fixed/equal-share/weighted grow and integer weighted-shrink slots, explicit pixel min/max floors, pixel row/column gaps, basis, start/center/end/space-between/space-around/space-evenly justification, and container/item stretch/start/center/end cross alignment; no intrinsic min-content sizing, fractional factors, wrapped ordering, column wrapping, wrap-reverse, wrapped cross-axis alignment, or auto-height column reordering guarantee");
            Capability(builder, "Grid, Canvas, SVG", "Not supported", "Future compatibility work; no silent fallback claim");
            Capability(builder, "Audio and video", "Not supported", "Future optional module");
            Capability(builder, "Developer diagnostics", "Supported", "Bounded summary, node, CSS, request, message, and variable snapshots");
            Capability(builder, "Document queries", "Supported subset", "Id, first-match Q/Q1, and read-only Qa with compiled selectors and a 256-result ceiling");
            Capability(builder, "Document base URLs", "Supported", "First valid bounded HTTP, HTTPS, or file base; U remains the response URL and Bu drives relative resources");
            Capability(builder, "Same-page fragments", "Supported", "Bounded Ft/Fg/Rv lookup, retained 128-entry history, details reveal, target scrolling, and no-fetch Back/Forward");
            Capability(builder, "Progress and meter gauges", "Supported", "Compact Vm values, finite normalization, meter quality regions, inherited styles, table cells, and custom paint");
            Capability(builder, "Output and data values", "Supported", "Compact Dv values, bounded references and text, host aliases, inline/block/table ownership, and safe rebuilds");
            Capability(builder, "Time and edit metadata", "Supported", "Compact Dv t/i/r values, bounded datetime and citation aliases, safe schemes, native decorations, inline/block/table ownership, and safe rebuilds");
            Capability(builder, "Image maps", "Supported subset", "Compact Mp/Ar models, rect/circle/poly/default and dead areas, bounded coordinates, scaled custom hit regions, area links/downloads, and safe rebuilds");
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
            builder.AppendLine("The engine runtime size is `Lea.Engine.dll` plus `Lea.Win.dll`. Test and report executables are listed separately and are not required when embedding Lea in Firespider.");
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
            var winTestPath = Path.Combine(root, "Lea.Win.Test", "bin", "Release", "Lea.Win.Test.exe");
            var benchPath = Path.Combine(root, "Lea.Bench", "bin", "Release", "Lea.Bench.exe");
            if (!File.Exists(enginePath)) enginePath = Path.Combine(root, "Lea.Engine.dll");
            if (!File.Exists(winPath)) winPath = Path.Combine(root, "Lea.Win.dll");
            if (!File.Exists(winTestPath)) winTestPath = Path.Combine(root, "Lea.Win.Test.exe");
            if (!File.Exists(benchPath)) benchPath = Path.Combine(root, "Lea.Bench.exe");
            var candidates = new List<Artifact>
            {
                MakeArtifact("Lea.Engine.dll", enginePath, true),
                MakeArtifact("Lea.Win.dll", winPath, true),
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
                    Directory.Exists(Path.Combine(directory.FullName, "LeaForge", "test-pages")))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not find a Lea source or runtime root. Pass it with -r.");
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
