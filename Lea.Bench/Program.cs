using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
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
                "go-command.html", "lists.html", "disclosure.html", "fragments.html", "gauges.html"
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
                Need(engine.Q(page, "#panel.hot") != null && engine.Q(page, "#remove.old") == null, "page class commands changed");
                var field = page.Fs.First(item => item.El != null && item.El.Av("id") == "field");
                field.V = "Preserved benchmark value";
                Need(engine.Js(page, "rc('panel','hot'); ac('field','hot'); tc('toggle','hot');") == 3, "host class commands changed");
                Need(engine.Q(page, "#panel.hot") == null && engine.Q(page, "#field.hot") != null, "class selectors were not rebuilt");
                Need(field.V == "Preserved benchmark value", "class rebuild replaced mutable field state");
                return "ac/rc/tc restyle compiled CSS with 32 classes and 512 class-text characters bounded";
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
            Capability(builder, "HTML documents", "Partial", "Bounded quote-aware tokens, raw text, RCDATA, entities, recovery, semantic blocks, list markers, details disclosures, progress/meter gauges, tables, and forms; not the full Living Standard");
            Capability(builder, "CSS", "Partial", "Compiled selector subset, cascade, box/text styles, visibility, positioning, transforms, and responsive image sizing");
            Capability(builder, "Layout and painting", "Partial", "Custom WinForms block, inline, table, form, image, clipping, scroll, and stacking renderer");
            Capability(builder, "Forms", "Supported subset", "GET/POST, validation, text/select/choice/file/range/date/color/time-family controls");
            Capability(builder, "Lea command scripting", "Supported", "Bounded title, color, font, alert, variable, text, class mutation, host navigation request, and safe click commands");
            Capability(builder, "General JavaScript and DOM APIs", "Not supported", "Lea deliberately does not execute arbitrary JavaScript");
            Capability(builder, "Flexbox, Grid, Canvas, SVG", "Not supported", "Future compatibility work; no silent fallback claim");
            Capability(builder, "Audio and video", "Not supported", "Future optional module");
            Capability(builder, "Developer diagnostics", "Supported", "Bounded summary, node, CSS, request, message, and variable snapshots");
            Capability(builder, "Document queries", "Supported subset", "Id, first-match Q/Q1, and read-only Qa with compiled selectors and a 256-result ceiling");
            Capability(builder, "Document base URLs", "Supported", "First valid bounded HTTP, HTTPS, or file base; U remains the response URL and Bu drives relative resources");
            Capability(builder, "Same-page fragments", "Supported", "Bounded Ft/Fg/Rv lookup, retained 128-entry history, details reveal, target scrolling, and no-fetch Back/Forward");
            Capability(builder, "Progress and meter gauges", "Supported", "Compact Vm values, finite normalization, meter quality regions, inherited styles, table cells, and custom paint");
            Capability(builder, "Accessibility", "Partial", "Keyboard focus and native fields are present; a complete accessibility tree is not");
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
            var candidates = new List<Artifact>
            {
                MakeArtifact("Lea.Engine.dll", Path.Combine(root, "Lea.Engine", "bin", "Release", "Lea.Engine.dll"), true),
                MakeArtifact("Lea.Win.dll", Path.Combine(root, "Lea.Win", "bin", "Release", "Lea.Win.dll"), true),
                MakeArtifact("Lea.Win.Test.exe", Path.Combine(root, "Lea.Win.Test", "bin", "Release", "Lea.Win.Test.exe"), false),
                MakeArtifact("Lea.Bench.exe", Path.Combine(root, "Lea.Bench", "bin", "Release", "Lea.Bench.exe"), false)
            };
            var browserPath = browser;
            if (String.IsNullOrWhiteSpace(browserPath))
            {
                var known = @"L:\Firespider Internet\Firespider Internet 19.exe";
                if (File.Exists(known)) browserPath = known;
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
                if (File.Exists(Path.Combine(directory.FullName, "Lea.sln"))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Could not find Lea.sln. Pass the repository with -r.");
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
