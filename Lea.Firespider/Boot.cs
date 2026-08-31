using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Lea.Win;

namespace Lea.Firespider
{
    internal static class Boot
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly List<Exception> UiErrors = new List<Exception>();

        [STAThread]
        private static int Main(string[] args)
        {
            var root = Path.GetFullPath(args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory);
            var mode = (args.Length > 1 ? args[1] : "clean").ToLowerInvariant();
            var session = Path.Combine(Path.GetTempPath(), "lea-boot-" + Guid.NewGuid().ToString("N") + ".txt");
            var log = session + ".log";
            var oldSession = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
            ApplicationSettingsBase settings = null;
            var saved = new Dictionary<string, object>();
            TcpListener server = null;
            Thread worker = null;
            FileStream sessionLock = null;
            LoopbackSite lazySite = null;
            var release = new ManualResetEvent(false);
            var requested = new ManualResetEvent(false);
            var responded = new ManualResetEvent(false);
            var forms = new List<Form>();
            try
            {
                Check(new[] { "clean", "lea", "legacy", "recovery", "private", "second", "limit", "invalid", "slow", "locked", "menus", "commands", "navigation", "links", "suspend", "groups", "lazy", "lazy-background", "lazy-all", "bulk", "windows", "watch", "domains", "domain-private", "split", "split-life", "split-limits", "edit", "policy", "policy-start", "failures", "failure-start", "hibernate", "hibernate-start" }.Contains(mode), "unknown boot case: " + mode);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (sender, error) => UiErrors.Add(error.Exception);
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", session);
                var map = new ExeConfigurationFileMap { ExeConfigFilename = Path.Combine(root, "Firespider Internet 19.exe.config") };
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                var defaults = (ClientSettingsSection)configuration.GetSection("userSettings/Firespider_Internet.My.MySettings");
                Check(defaults != null && defaults.Settings.Count > 0, "executable settings configuration");
                var assembly = Assembly.LoadFrom(Path.Combine(root, "Firespider Internet 19.exe"));
                var type = assembly.GetType("Firespider_Internet.j", true);
                settings = (ApplicationSettingsBase)assembly.GetType("Firespider_Internet.My.MySettings", true)
                    .GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                foreach (SettingsProperty property in settings.Properties)
                {
                    var value = settings[property.Name];
                    var strings = value as StringCollection;
                    saved[property.Name] = strings == null ? value : Collection(strings.Cast<string>().ToArray());
                }
                var page = new Uri(Path.Combine(root, "Lea", "start.html")).AbsoluteUri;
                foreach (var name in new[] { "dashboard", "rs", "ps", "recoveryEnabled", "t", "rws", "backupEnabled", "frEnabled", "portableMode", "hibernateEnabled", "watchEnabled" })
                    settings[name] = "";
                foreach (var name in new[] { "s", "sc", "ls", "recovery", "bm", "tg" }) settings[name] = new StringCollection();
                settings["nf"] = page;
                settings["h"] = page;
                settings["se"] = "http://127.0.0.1:1/?q=";
                settings["cleanExit"] = "e";
                settings["el"] = "e";
                settings["ep"] = log;
                settings["lazyRestoreMode"] = "off";
                var expected = new[] { page };
                if (mode == "hibernate-start")
                {
                    settings["hibernateEnabled"] = "e";
                    settings["hibernateMinutes"] = "1";
                    settings["hibernateExceptions"] = new StringCollection();
                    expected = new[] { page, page + "#idle" };
                    File.WriteAllLines(session, expected);
                }
                if (mode == "failure-start")
                {
                    lazySite = new LoopbackSite(150) { Status = 503 };
                    settings["bs"] = new StringCollection();
                    settings["cfEnabled"] = "";
                    expected = new[] { lazySite.Url + "failed-start", page };
                    File.WriteAllLines(session, expected);
                }
                if (mode == "policy-start")
                {
                    lazySite = new LoopbackSite();
                    settings["bs"] = Collection("127.0.0.1");
                    expected = new[] { lazySite.Url + "blocked-start", page };
                    File.WriteAllLines(session, expected);
                }
                if (mode == "lazy-background" || mode == "lazy-all")
                {
                    lazySite = new LoopbackSite();
                    settings["lazyRestoreMode"] = mode.Substring(5);
                    expected = new[] { lazySite.Url + "first", lazySite.Url + "second", lazySite.Url + "third" };
                    File.WriteAllLines(session, expected);
                }
                if (mode == "lea" || mode == "second" || mode == "private")
                {
                    File.WriteAllLines(session, new[] { page + "#one", page + "#two" });
                    if (mode != "private") expected = File.ReadAllLines(session);
                }
                if (mode == "legacy" || mode == "recovery" || mode == "private")
                {
                    settings["rs"] = "e";
                    settings["ls"] = Collection(page + "#old-one", page + "#old-two");
                    settings["recovery"] = Collection(page + "#crash-one", page + "#crash-two");
                    if (mode == "legacy") expected = ((StringCollection)settings["ls"]).Cast<string>().ToArray();
                    if (mode == "recovery")
                    {
                        expected = ((StringCollection)settings["recovery"]).Cast<string>().ToArray();
                        File.WriteAllLines(session, new[] { page + "#older-session" });
                        settings["recoveryEnabled"] = "e";
                        settings["recoveryTime"] = "";
                    }
                    if (mode == "private")
                    {
                        settings["ps"] = "e";
                        settings["recoveryEnabled"] = "e";
                        settings["cleanExit"] = "";
                    }
                }
                if (mode == "limit")
                {
                    expected = Enumerable.Range(0, 12).Select(index => page + "#" + index).ToArray();
                    File.WriteAllLines(session, Enumerable.Range(0, 20).Select(index => page + "#" + index).ToArray());
                }
                if (mode == "invalid")
                    File.WriteAllLines(session, new[] { "javascript:alert(1)", "not a URL", "https://example.invalid/" + new string('a', 4096) });
                if (mode == "locked") sessionLock = new FileStream(session, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                if (mode == "slow")
                {
                    server = new TcpListener(IPAddress.Loopback, 0);
                    server.Start();
                    settings["nf"] = "http://127.0.0.1:" + ((IPEndPoint)server.LocalEndpoint).Port + "/";
                    expected = new[] { (string)settings["nf"] };
                    worker = new Thread(() =>
                    {
                        try
                        {
                            using (var client = server.AcceptTcpClient())
                            using (var stream = client.GetStream())
                            {
                                stream.ReadTimeout = 10000;
                                var headerEnd = 0;
                                for (var count = 0; count < 16384 && headerEnd != 0x0d0a0d0a; count++)
                                {
                                    var next = stream.ReadByte();
                                    if (next < 0) throw new IOException("Incomplete test request");
                                    headerEnd = (headerEnd << 8) | next;
                                }
                                if (headerEnd != 0x0d0a0d0a) throw new IOException("Oversized test request");
                                requested.Set();
                                release.WaitOne(25000);
                                responded.Set();
                                var body = Encoding.UTF8.GetBytes("<title>Slow Lea Start</title><p>Ready</p>");
                                var header = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
                                stream.Write(header, 0, header.Length);
                                stream.Write(body, 0, body.Length);
                            }
                        }
                        catch (SocketException) { }
                        catch (IOException) { }
                    });
                    worker.IsBackground = true;
                    worker.Start();
                }
                var construction = Stopwatch.StartNew();
                var form = (Form)Activator.CreateInstance(type);
                construction.Stop();
                forms.Add(form);
                var layouts = 0;
                if (Environment.GetEnvironmentVariable("LEA_FIRESPIDER_PROFILE") == "1")
                    foreach (var name in new[] { "m", "fl", "e", "v", "hi", "bo", "ot", "he" })
                        Member<ToolStripDropDownItem>(form, name).DropDown.Layout += (sender, change) => layouts++;
                if (mode == "recovery" || mode == "private") type.GetField("recoveryRequested", Members).SetValue(form, true);
                var watch = Stopwatch.StartNew();
                Show(form);
                watch.Stop();
                if (Environment.GetEnvironmentVariable("LEA_FIRESPIDER_PROFILE") == "1")
                {
                    var timings = (List<string>)type.GetField("qleaTimings", Members).GetValue(form);
                    Console.WriteLine("PROFILE Construct=" + construction.ElapsedMilliseconds + " ms; " + String.Join("; ", timings.ToArray()));
                    Console.WriteLine("PROFILE RootLayouts=" + layouts + "; " + MenuSignature(Member<ToolStripDropDownItem>(form, "m")));
                }
                var tabs = Member<TabControl>(form, "k");
                Check(tabs.TabCount == expected.Length, "startup tab count: " + tabs.TabCount + " expected " + expected.Length);
                for (var index = 0; index < tabs.TabCount; index++)
                {
                    var tab = tabs.TabPages[index];
                    var deferred = mode == "lazy-all" || (mode == "lazy-background" && index > 0);
                    Check(tab.Controls.Count == 1 && (deferred ? tab.Controls[0] is Panel : tab.Controls[0] is V), "startup created an unexpected tab engine/state");
                }
                if (mode == "slow")
                {
                    Wait(() => requested.WaitOne(0) && ((V)tabs.TabPages[0].Controls[0]).Busy, "asynchronous startup request");
                    Check(!responded.WaitOne(0), "startup waited for the network response");
                    Check(form.IsHandleCreated && form.Visible, "window unavailable during startup request");
                    release.Set();
                }
                if (mode == "failure-start")
                {
                    Wait(() => ((V)tabs.TabPages[0].Controls[0]).Le != null && ((V)tabs.TabPages[1].Controls[0]).At != null, "unavailable/allowed startup completion");
                    Check(((V)tabs.TabPages[0].Controls[0]).At == null && lazySite.Hits == 1, "failed startup committed a page or retried automatically");
                }
                else if (mode == "policy-start")
                {
                    Wait(() => !((V)tabs.TabPages[0].Controls[0]).Busy && ((V)tabs.TabPages[1].Controls[0]).At != null, "blocked/allowed startup completion");
                    Check(((V)tabs.TabPages[0].Controls[0]).At == null && lazySite.Hits == 0, "blocked startup made a request or committed a page");
                }
                else WaitLea(tabs);
                for (var index = 0; index < expected.Length; index++)
                {
                    if (mode == "policy-start" && index == 0) continue;
                    Check((string)Call(form, "qleaUrl", tabs.TabPages[index]) == expected[index], "startup URL ordering");
                }
                Check(tabs.SelectedIndex == 0, "startup did not select first restored page");
                if (mode == "menus")
                {
                    CheckMenuLayouts(Member<ToolStripDropDownItem>(form, "m"));
                    using (var menu = new ToolStripMenuItem("Test"))
                    {
                        var rejected = false;
                        try { type.GetMethod("qorganizeroot", Members).Invoke(form, new object[] { menu, new[] { "Other", "Other" } }); }
                        catch (TargetInvocationException error) { rejected = error.InnerException is ArgumentException; }
                        Check(rejected, "menu failure probe did not reach the duplicate-category path");
                        CheckMenuLayouts(menu);
                    }
                }
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module =>
                    module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "startup loaded the legacy MSHTML engine");
                if (mode == "private") Check(((V)tabs.TabPages[0].Controls[0]).Pv, "private startup view was not private");
                if (mode == "second")
                {
                    var second = (Form)Activator.CreateInstance(type);
                    forms.Add(second);
                    Show(second);
                    var secondTabs = Member<TabControl>(second, "k");
                    Check(secondTabs.TabCount == 1 && secondTabs.TabPages[0].Controls[0] is V, "second window duplicated saved session");
                }
                if (mode == "clean")
                {
                    Member<Button>(form, "zx").PerformClick();
                    Member<ToolStripItem>(form, "N").PerformClick();
                    Check(tabs.TabCount == 3 && tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is V), "primary new-tab routes created legacy controls");
                    Wait(() => tabs.TabPages.Cast<TabPage>().All(tab => !((V)tab.Controls[0]).Busy), "new-tab page loads");
                    type.GetMethod("qrecoverysnapshot", Members).Invoke(form, null);
                    Check(((StringCollection)settings["recovery"]).Count == 3, "recovery snapshot omitted Lea URLs");
                    var legacy = Member<ToolStripMenuItem>(form, "fl").DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "New Legacy Tab");
                    legacy.PerformClick();
                    Check(tabs.SelectedTab.Controls[0] is WebBrowser, "explicit legacy fallback was lost");
                }
                if (mode == "commands") CheckCommands(form, settings, page);
                if (mode == "navigation") CheckNavigation(form, settings, page);
                if (mode == "links") CheckLinks(form, settings, page);
                if (mode == "suspend") CheckSuspension(form, settings, page);
                if (mode == "groups") CheckGroups(form, settings, page);
                if (mode == "lazy") CheckLazyCommands(form, settings, page);
                if (mode == "bulk") CheckBulk(form, settings, page);
                if (mode == "windows") CheckWindows(form, settings, page, forms);
                if (mode == "watch") CheckWatch(form, settings, page);
                if (mode == "domains") CheckDomains(form, settings, page);
                if (mode == "domain-private") CheckDomainPrivate(form, settings, page, forms);
                if (mode == "split") CheckSplit(form, settings, page, forms);
                if (mode == "split-life") CheckSplitLife(form, settings, page, forms);
                if (mode == "split-limits") CheckSplitLimits(form, settings, page, forms);
                if (mode == "edit") CheckEdit(form, settings, page);
                if (mode == "policy") CheckPolicy(form, settings, page, forms);
                if (mode == "failures") CheckFailures(form, settings, page);
                if (mode == "hibernate") CheckHibernation(form, settings, page);
                if (mode == "hibernate-start") CheckHibernationStartup(form, expected);
                if (mode == "recovery")
                {
                    var idle = Stopwatch.StartNew();
                    while (idle.ElapsedMilliseconds < 35000 && UiErrors.Count == 0) { Application.DoEvents(); Thread.Sleep(20); }
                    Check((string)settings["recoveryTime"] != "" && ((StringCollection)settings["recovery"]).Cast<string>().SequenceEqual(expected),
                        "live recovery timer did not retain restored Lea URLs");
                    Console.WriteLine("PASS recovery idle: 35 seconds of event pumping and the real 30-second snapshot timer");
                }
                if (mode == "policy-start")
                {
                    Call(form, "q1", page);
                    WaitLea(tabs);
                    Check(lazySite.Hits == 0 && ((V)tabs.TabPages[0].Controls[0]).U.AbsoluteUri == page, "blocked startup tab could not navigate afterward");
                    Console.WriteLine("PASS policy startup: blocked saved URL makes no request; allowed sibling and later navigation remain usable");
                }
                else if (mode == "failure-start")
                {
                    Call(form, "qleaTick", form, EventArgs.Empty);
                    Check(Member<ToolStripComboBox>(form, "c").Text == expected[0] && Member<Label>(form, "su").Text.Contains("503"), "failed startup lost address or error status");
                    Call(form, "qleaSaveSession");
                    Check(File.ReadAllLines(session).SequenceEqual(expected), "failed startup URL lost from session");
                    lazySite.Status = 200;
                    Member<ToolStripItem>(form, "re").PerformClick();
                    WaitLea(tabs);
                    Check(lazySite.Hits == 2 && ((V)tabs.TabPages[0].Controls[0]).Le == null, "failed first page could not be retried through Reload");
                    Console.WriteLine("PASS failed startup: unavailable saved page, working sibling, address/session retention, and Reload recovery");
                }
                else if (lazySite != null) CheckLazyStartup(form, lazySite, expected, mode == "lazy-all");
                Check(UiErrors.Count == 0, "UI thread exceptions: " + String.Join(" | ", UiErrors.Select(error => error.ToString()).ToArray()));
                if (mode == "locked")
                {
                    form.Close();
                    Check(form.IsDisposed && (string)settings["cleanExit"] == "e", "locked session save skipped normal shutdown cleanup");
                }
                Console.WriteLine("PASS Lea boot: " + mode + "; Show=" + watch.ElapsedMilliseconds + " ms; initial tabs=" + expected.Length);
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("FAIL Lea boot " + mode + ": " + error.GetBaseException());
                foreach (var form in forms)
                    if (!form.IsDisposed && form.GetType().GetField("su", Members) != null)
                        Console.Error.WriteLine("Host status: " + Member<Label>(form, "su").Text);
                if (File.Exists(log)) Console.Error.WriteLine(File.ReadAllText(log));
                return 1;
            }
            finally
            {
                release.Set();
                if (server != null) server.Stop();
                if (worker != null) worker.Join(3000);
                try
                {
                    foreach (var form in forms) { form.Close(); form.Dispose(); }
                }
                finally
                {
                    if (sessionLock != null) sessionLock.Dispose();
                    if (settings != null)
                    {
                        foreach (var pair in saved) settings[pair.Key] = pair.Value;
                        settings.Save();
                    }
                    Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", oldSession);
                    if (File.Exists(session)) File.Delete(session);
                    if (File.Exists(log)) File.Delete(log);
                    release.Dispose();
                    requested.Dispose();
                    responded.Dispose();
                    if (lazySite != null) lazySite.Dispose();
                }
            }
        }

        private static void Show(Form form)
        {
            form.Opacity = 0;
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-30000, -30000);
            form.Show();
        }

        private static StringCollection Collection(params string[] values)
        {
            var result = new StringCollection();
            result.AddRange(values);
            return result;
        }

        private static void CheckEdit(Form form, ApplicationSettingsBase settings, string page)
        {
            var names = new[] { "CO", "CU", "PA", "SA", "D", "U", "RD" };
            foreach (var name in names) Member<ToolStripItem>(form, name).PerformClick();
            var original = Clipboard.GetDataObject();
            var clipboard = new DataObject();
            if (original != null)
                foreach (var format in original.GetFormats(false)) clipboard.SetData(format, false, original.GetData(format, false));
            try
            {
                var tabs = Member<TabControl>(form, "k");
                var first = tabs.SelectedTab;
                var view = (V)first.Controls[0];
                view.Go(Path.Combine(Path.GetDirectoryName(new Uri(page).LocalPath), "..", "test-pages", "editing.html"));
                using (var paint = new Bitmap(view.Width, view.Height)) view.DrawToBitmap(paint, new Rectangle(Point.Empty, paint.Size));
                var field = view.At.Fs.Single(item => item.El.Av("id") == "text");
                var editor = Member<TextBox>(view, "ed");
                var startEdit = typeof(V).GetMethod("StartEdit", Members);
                startEdit.Invoke(view, new object[] { field });
                editor.Select(4, 5);
                var menu = Member<ToolStripDropDownItem>(form, "e");
                menu.ShowDropDown();
                Application.DoEvents();
                Member<ToolStripItem>(form, "CO").PerformClick();
                menu.HideDropDown();
                Check(Clipboard.GetText() == "field" && field.V == "Lea field text", "real Edit menu lost field selection");
                Member<ToolStripItem>(form, "CU").PerformClick();
                Check(field.V == "Lea  text", "host Cut did not edit Lea");
                Member<ToolStripItem>(form, "U").PerformClick();
                Check(field.V == "Lea field text", "host Undo did not restore Lea field");
                Member<ToolStripItem>(form, "RD").PerformClick();
                Check(field.V == "Lea  text", "host Redo did not replay Lea edit");
                Member<ToolStripItem>(form, "SA").PerformClick();
                Clipboard.SetText("host replacement");
                Member<ToolStripItem>(form, "PA").PerformClick();
                Check(field.V == "host replacement", "host Paste did not replace selected field");
                Member<ToolStripItem>(form, "SA").PerformClick();
                Member<ToolStripItem>(form, "D").PerformClick();
                Check(field.V == "", "host Delete did not clear selected field");
                Member<ToolStripItem>(form, "U").PerformClick();
                Check(field.V == "host replacement" && Clipboard.GetText() == "host replacement", "host Delete lost undo or changed the clipboard");
                Member<ToolStripItem>(form, "RD").PerformClick();
                Check(field.V == "", "host Delete redo failed");
                editor.Text = "page field stays";
                editor.SelectAll();
                Clipboard.SetText("toolbar clipboard");
                foreach (var chrome in new[] { "c", "x" })
                {
                    Member<ToolStripComboBox>(form, chrome).ComboBox.Focus();
                    menu.ShowDropDown();
                    foreach (var name in names) Member<ToolStripItem>(form, name).PerformClick();
                    menu.HideDropDown();
                    Check(field.V == "page field stays" && Clipboard.GetText() == "toolbar clipboard", "toolbar focus edited the previous page field");
                }
                startEdit.Invoke(view, new object[] { field });
                editor.Text = "hover selection";
                editor.SelectAll();
                Check(view.Ce("cp"), "hover fixture has no selected native field: " + editor.Visible + "/" + editor.SelectionLength + "/" + (field.V == editor.Text));
                Clipboard.SetText("unchanged");
                // ToolStrip raises MouseHover in HandleMouseHover, not OnMouseHover.
                var hover = typeof(ToolStripItem).GetMethod("HandleMouseHover", Members);
                var copy = Member<ToolStripItem>(form, "CO");
                ((ToolStripDropDownItem)copy.OwnerItem).ShowDropDown();
                copy.Select();
                settings["t"] = "";
                hover.Invoke(Member<ToolStripItem>(form, "CO"), new object[] { EventArgs.Empty });
                Check(Clipboard.GetText() == "unchanged", "hover editing ran while disabled");
                settings["t"] = "e";
                hover.Invoke(Member<ToolStripItem>(form, "CO"), new object[] { EventArgs.Empty });
                Check(Clipboard.GetText() == "hover selection", "enabled hover Copy ignored Lea editor: enabled=" + copy.Enabled + "; available=" + view.Ce("cp") + "; selection=" + editor.SelectionLength + "; matching=" + (field.V == editor.Text));
                ((ToolStripDropDownItem)copy.OwnerItem).HideDropDown();
                settings["t"] = "";
                var secret = view.At.Fs.Single(item => item.El.Av("id") == "secret");
                startEdit.Invoke(view, new object[] { secret });
                Member<ToolStripItem>(form, "CO").PerformClick();
                Member<ToolStripItem>(form, "CU").PerformClick();
                Check(secret.V == "private value" && Clipboard.GetText() == "hover selection", "host exposed password through Copy or Cut");
                var readOnly = view.At.Fs.Single(item => item.El.Av("id") == "readonly");
                startEdit.Invoke(view, new object[] { readOnly });
                foreach (var name in new[] { "CU", "PA", "D", "U", "RD" }) Member<ToolStripItem>(form, name).PerformClick();
                Check(readOnly.V == "Read only", "host changed read-only field");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)),
                    "native Edit commands loaded MSHTML");
                Check((bool)Call(form, "qleaSuspend", first), "editing view did not suspend");
                foreach (var name in names) Member<ToolStripItem>(form, name).PerformClick();
                Check(first.Controls[0] is Panel && view.IsDisposed, "Edit commands resumed a paused tab");
                Call(form, "q3", page);
                var legacy = (WebBrowser)tabs.SelectedTab.Controls[0];
                Wait(() => legacy.Document != null && legacy.ReadyState == WebBrowserReadyState.Complete, "explicit legacy edit page");
                Member<ToolStripItem>(form, "SA").PerformClick();
                Member<ToolStripItem>(form, "CO").PerformClick();
                Check(Clipboard.GetText().Contains("Lea"), "explicit legacy Copy fallback was lost");
                foreach (TabPage tab in tabs.TabPages.Cast<TabPage>().ToArray()) { tabs.TabPages.Remove(tab); tab.Dispose(); }
                foreach (var name in names) Member<ToolStripItem>(form, name).PerformClick();
                Check(tabs.TabCount == 0, "empty-window editing created a tab");
            }
            finally
            {
                settings["t"] = "";
                if (original == null) Clipboard.Clear(); else Clipboard.SetDataObject(clipboard, true);
            }
            Console.WriteLine("PASS edit: real menu/hover dispatch, selection, undo/redo, clipboard, readonly/password, paused/empty tabs, and explicit legacy fallback");
        }

        private static void CheckHibernationStartup(Form form, string[] urls)
        {
            var tabs = Member<TabControl>(form, "k");
            var foreground = tabs.TabPages[0];
            var background = tabs.TabPages[1];
            var view = (V)background.Controls[0];
            var timer = Member<System.Windows.Forms.Timer>(form, "feature6HibernateTimer");
            Check(timer.Enabled && timer.Interval == 30000, "configured hibernation timer did not start");
            var idle = Member<Dictionary<TabPage, DateTime>>(form, "feature6IdleSince");
            idle[background] = DateTime.Now.AddMinutes(-2);
            var watch = Stopwatch.StartNew();
            while (!view.IsDisposed && UiErrors.Count == 0 && watch.ElapsedMilliseconds < 35000) { Application.DoEvents(); Thread.Sleep(20); }
            Check(view.IsDisposed && background.Controls[0] is Panel && UiErrors.Count == 0, "real startup hibernation timer did not release idle Lea");
            Check(tabs.SelectedTab == foreground && foreground.Controls[0] is V && (string)Call(form, "qleaUrl", background) == urls[1], "startup hibernation lost selection or saved target");
            form.Close();
            Check(form.IsDisposed && !timer.Enabled && idle.Count == 0, "closing did not stop hibernation timer and release bookkeeping");
            Console.WriteLine("PASS hibernation startup: real 30-second policy timer, restored idle Lea release, foreground/URL retention, and close cleanup");
        }

        private static void CheckHibernation(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var selected = tabs.SelectedTab;
            settings["hibernateExceptions"] = new StringCollection();
            settings["hibernateMinutes"] = "1";
            var view = (V)Call(form, "qleaNew", page);
            var background = (TabPage)view.Parent;
            WaitLea(tabs);
            tabs.SelectedTab = selected;
            Check((bool)Call(form, "qfeature6tabeligible", background), "pristine background Lea tab is excluded from hibernation");
            Menu(form, "Hibernate Eligible Tabs Now").PerformClick();
            Check(view.IsDisposed && background.Controls.Count == 1 && background.Controls[0] is Panel, "hibernation did not release the native renderer");
            Check(tabs.SelectedTab == selected && (string)Call(form, "qleaUrl", background) == page, "hibernation changed selection or lost the URL");
            tabs.SelectedTab = background;
            WaitLea(tabs);
            Check(background.Controls[0] is V && background.Controls[0] != view, "selecting hibernated Lea did not create a fresh renderer");
            view = (V)background.Controls[0];
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "selected tab eligible for automatic discard");
            tabs.SelectedTab = selected;
            foreach (var protection in new[] { "locked", "pinned" })
            {
                background.Tag = protection;
                Check(!(bool)Call(form, "qfeature6tabeligible", background), "protected Lea tab eligible: " + protection);
            }
            background.Tag = "lea";
            background.ToolTipText = "Tab group: Test";
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "grouped Lea tab eligible");
            background.ToolTipText = "";
            tabs.TabPages.Remove(background);
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "detached or hidden Lea tab eligible");
            tabs.TabPages.Add(background);
            view.Pv = true;
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "sticky private Lea tab eligible");
            view.Pv = false;
            Set(form, "pv", true);
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "global private mode allowed hibernation");
            Set(form, "pv", false);
            var idle = Member<Dictionary<TabPage, DateTime>>(form, "feature6IdleSince");
            idle[background] = DateTime.Now;
            Check((int)Call(form, "qfeature6hibernateeligible", true) == 0 && !view.IsDisposed, "recent background tab discarded before idle threshold");
            idle[background] = DateTime.Now.AddMinutes(-2);
            settings["hibernateEnabled"] = "";
            Call(form, "qfeature6hibernatetick", form, EventArgs.Empty);
            Check(!view.IsDisposed, "disabled timer policy discarded Lea tab");
            settings["hibernateEnabled"] = "e";
            Call(form, "qfeature6hibernatetick", form, EventArgs.Empty);
            Check(view.IsDisposed && background.Controls[0] is Panel && Member<Label>(form, "su").Text.Contains("1 idle"), "timer did not hibernate eligible idle Lea tab");
            Check((int)Call(form, "qfeature6hibernateeligible", true) == 0, "already paused Lea tab hibernated again");
            tabs.SelectedTab = background;
            WaitLea(tabs);
            view = (V)background.Controls[0];
            tabs.SelectedTab = selected;
            view.Go(page + "#history");
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "retained history would be lost by automatic hibernation");
            view.Bk();
            Check(!(bool)Call(form, "qfeature6tabeligible", background), "forward history would be lost by automatic hibernation");
            Call(form, "qleaClose", background);

            var fixture = Path.Combine(Path.GetTempPath(), "lea-hibernate-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                File.WriteAllText(fixture, "<title>Hibernation data</title><input id=edit><p style='height:2400px'>Long reading page</p>");
                view = (V)Call(form, "qleaNew", fixture);
                background = (TabPage)view.Parent;
                WaitLea(tabs);
                tabs.SelectedTab = selected;
                var field = view.At.Fs[0];
                field.V = "Keep this text";
                Check(!(bool)Call(form, "qfeature6tabeligible", background), "edited Lea data eligible");
                field.V = "";
                view.Sc(100);
                Check(view.Sy > 0, "hibernation fixture did not scroll");
                Check(!(bool)Call(form, "qfeature6tabeligible", background), "reading position eligible for discard");
                view.Sc(-100000);
                Check((bool)Call(form, "qfeature6tabeligible", background), "pristine empty-field page did not recover eligibility");
                view.Js("hd('edit')");
                Check(!(bool)Call(form, "qfeature6tabeligible", background), "script state eligible for discard");
                Call(form, "qleaClose", background);
                foreach (var html in new[] { "<details><summary>State</summary>Content</details>", "<audio></audio>", "<video></video>", "<div contenteditable>Editable</div>" })
                {
                    File.WriteAllText(fixture, "<title>Retained document</title>" + html);
                    view = (V)Call(form, "qleaNew", fixture);
                    background = (TabPage)view.Parent;
                    WaitLea(tabs);
                    tabs.SelectedTab = selected;
                    Check(!(bool)Call(form, "qfeature6tabeligible", background), "non-field interactive state eligible for discard: " + html);
                    Call(form, "qleaClose", background);
                }
                using (var site = new LoopbackSite())
                {
                    site.Release.Reset();
                    view = (V)Call(form, "qleaNew", site.Url);
                    background = (TabPage)view.Parent;
                    tabs.SelectedTab = selected;
                    Check(view.Busy && !(bool)Call(form, "qfeature6tabeligible", background), "in-flight first page eligible");
                    site.Release.Set();
                    WaitLea(tabs);
                    settings["hibernateExceptions"] = Collection("127.0.0.1");
                    Check(!(bool)Call(form, "qfeature6tabeligible", background), "domain exception ignored for Lea");
                    settings["hibernateExceptions"] = Collection("example.test");
                    Check((bool)Call(form, "qfeature6tabeligible", background), "unrelated exception blocked Lea");
                    var download = Path.Combine(Path.GetTempPath(), "lea-hibernate-download-" + Guid.NewGuid().ToString("N") + ".bin");
                    try
                    {
                        site.Release.Reset();
                        site.SetCookie = true;
                        var work = view.DlA(site.Url + "download", download);
                        Wait(() => site.Hits == 2, "hibernation download started");
                        Check(view.Busy && !(bool)Call(form, "qfeature6tabeligible", background), "active download eligible for discard");
                        Check((int)Call(form, "qfeature6hibernateeligible", false) == 0 && !view.IsDisposed, "hibernation interrupted active download");
                        site.Release.Set();
                        Wait(() => work.IsCompleted, "retained download completion");
                        Check(work.GetAwaiter().GetResult().Sz > 0 && File.Exists(download), "protected download did not finish");
                        Check(view.Kc == 1 && !(bool)Call(form, "qfeature6tabeligible", background), "cookie context eligible for discard");
                        site.SetCookie = false;
                        view.Kcl();
                        Check((bool)Call(form, "qfeature6tabeligible", background), "cleared cookie context did not recover eligibility");
                    }
                    finally { site.Release.Set(); if (File.Exists(download)) File.Delete(download); }
                    site.Status = 503;
                    try { view.Go(site.Url + "failed"); } catch (Lea.Engine.Er) { }
                    Check(view.Le != null && !(bool)Call(form, "qfeature6tabeligible", background), "failed navigation/error state eligible");
                    Call(form, "qleaClose", background);
                }
                using (var empty = new TabPage())
                {
                    tabs.TabPages.Add(empty);
                    Check(!(bool)Call(form, "qfeature6tabeligible", empty), "empty tab eligible");
                    tabs.TabPages.Remove(empty);
                }
            }
            finally { if (File.Exists(fixture)) File.Delete(fixture); }
            TabPage released;
            var weak = SuspendCollectible(form, page, out released, true);
            Wait(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); return Collected(weak); }, "hibernated renderer retained in host state");
            Check(released.Controls[0] is Panel && tabs.SelectedTab == selected, "collectible hibernation resumed or changed selection");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "Lea hibernation loaded MSHTML");
            settings["hibernateEnabled"] = "";
            Console.WriteLine("PASS hibernation: native release/resume, idle/disabled timer policy, private/protected/grouped/edited/cookie/history/script/scroll/media/busy/error exclusions, and domain exceptions");
        }

        private static void CheckFailures(Form form, ApplicationSettingsBase settings, string page)
        {
            settings["bs"] = new StringCollection();
            settings["cfEnabled"] = "";
            var path = Path.Combine(Path.GetTempPath(), "lea-failures-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                using (var site = new LoopbackSite(150))
                {
                    site.Status = 503;
                    File.WriteAllText(path, "<title>Load failures</title><a id=bad accesskey=b href='" + site.Url + "unavailable'>Unavailable link</a><input id=editor accesskey=e value=retained><p>Retained page</p>");
                    var tabs = Member<TabControl>(form, "k");
                    var view = (V)tabs.SelectedTab.Controls[0];
                    view.Go(path);
                    var retained = view.At;
                    var uiThread = Thread.CurrentThread.ManagedThreadId;
                    var failureThread = 0;
                    var failures = 0;
                    view.Fe += delegate { failureThread = Thread.CurrentThread.ManagedThreadId; failures++; };
                    view.Kf();
                    Check(view.Ke == view.Id("bad"), "failure keyboard target");
                    var args = new KeyEventArgs(Keys.Enter);
                    typeof(V).GetMethod("OnKeyDown", Members).Invoke(view, new object[] { args });
                    var claimed = args.Handled && args.SuppressKeyPress;
                    Wait(() => site.Hits == 1 && failureThread != 0, "failed keyboard notification");
                    Observe(100);
                    Console.WriteLine("Failure input: UI=" + uiThread + "; Fe=" + failureThread + "; claimed before await=" + claimed);
                    Check(args.Handled && args.SuppressKeyPress && UiErrors.Count == 0 && view.At == retained, "ordinary keyboard load failure escaped or lost the page");
                    Check(claimed, "keyboard navigation was not claimed before awaiting the network");
                    Check(failureThread == uiThread, "failure notification left the UI thread");
                    Call(form, "qleaTick", form, EventArgs.Empty);
                    Check(Member<Label>(form, "su").Text.Contains("503"), "failure status was replaced by ready");
                    site.DelayMs = 100;
                    view.Aa("e").GetAwaiter().GetResult();
                    Check(Member<TextBox>(view, "ed").Visible, "failure test did not open the native editor");
                    Member<TextBox>(view, "ed").Text = "edited before failure";
                    var access = new KeyEventArgs(Keys.Alt | Keys.Shift | Keys.B);
                    typeof(V).GetMethod("EditKeyDown", Members).Invoke(view, new object[] { view, access });
                    Check(access.Handled && access.SuppressKeyPress, "editor access key not consumed before network");
                    Wait(() => failures == 2, "editor access-key failure");
                    Check(view.At == retained && UiErrors.Count == 0, "editor shortcut failure escaped or changed page");
                    Check(view.At.Fs.First(field => field.El.Av("id") == "editor").V == "edited before failure", "failed navigation lost the edit");
                    typeof(V).GetMethod("ActivateAccessibility", Members).Invoke(view, new object[] { view.Ax(view.Id("bad")) });
                    Wait(() => failures == 3, "accessible link failure");
                    Check(view.At == retained && UiErrors.Count == 0, "accessible failure escaped or changed page");
                    var failedTab = tabs.SelectedTab;
                    var background = (V)Call(form, "qleaNew", page);
                    tabs.SelectedTab = failedTab;
                    Wait(() => background.At != null && !background.Busy, "background successful page");
                    Call(form, "qleaTick", form, EventArgs.Empty);
                    Check(Member<Label>(form, "su").Text.Contains("503"), "background success hid foreground error");
                    site.Status = 200;
                    Member<ToolStripItem>(form, "re").PerformClick();
                    Wait(() => view.U != null && view.U.AbsoluteUri == site.Url + "unavailable" && !view.Busy, "host Reload retry");
                    Check(view.Le == null && !view.Cr && site.Hits == 4 && failures == 3, "successful GET retry did not clear failure or fetched twice");
                    Check(UiErrors.Count == 0, "native failure/retry raised a popup");
                    Console.WriteLine("PASS failures: keyboard/editor/accessibility errors, UI-thread notification, retained page/status, background completion, and explicit Reload retry");
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static void CheckPolicy(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            settings["bs"] = new StringCollection();
            settings["cfEnabled"] = "";
            settings["cfx"] = new StringCollection();
            settings["externalConfirm"] = "";
            var fixture = Path.Combine(Path.GetTempPath(), "lea-policy-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                using (var site = new LoopbackSite())
                {
                    File.WriteAllText(fixture, "<title>Navigation rules</title><a id=blocked accesskey=b href='" + site.Url + "link'>Blocked link</a>" +
                        "<form method=get action='" + site.Url + "get'><input name=term value=initial><input type=submit></form>" +
                        "<form method=post action='" + site.Url + "post'><input name=term value=initial><input type=submit></form>" +
                        "<form method=post enctype='multipart/form-data' action='" + site.Url + "multipart'><input name=term value=initial><input type=submit></form>");
                    var tabs = Member<TabControl>(form, "k");
                    var tab = tabs.SelectedTab;
                    var view = (V)tab.Controls[0];
                    view.Go(fixture);
                    var original = view.At;
                    settings["bs"] = Collection("127.0.0.1");
                    CheckBlocked(() => view.Go(site.Url), "main-tab direct navigation");
                    CheckBlocked(() => view.GoA(new Uri(site.Url)).GetAwaiter().GetResult(), "main-tab async navigation");
                    foreach (var submit in view.At.Fs.Where(field => field.K == "submit"))
                    {
                        CheckBlocked(() => view.Sm(submit), "main-tab form submission");
                        CheckBlocked(() => view.SmA(submit).GetAwaiter().GetResult(), "main-tab async form submission");
                    }
                    for (var index = 0; index < view.Kn && view.Ke != view.Id("blocked"); index++) view.Kf();
                    Check(view.Ke == view.Id("blocked"), "policy keyboard link fixture");
                    CheckBlocked(() => view.Ka().GetAwaiter().GetResult(), "keyboard API rejection");
                    foreach (var key in new[] { Keys.Enter, Keys.Alt | Keys.Shift | Keys.B })
                    {
                        var args = new KeyEventArgs(key);
                        typeof(V).GetMethod("OnKeyDown", Members).Invoke(view, new object[] { args });
                        Observe(50);
                        Check(args.Handled && args.SuppressKeyPress && UiErrors.Count == 0, "blocked keyboard activation escaped to UI thread");
                    }
                    var policyCalls = 0;
                    var policy = view.Ng;
                    view.Ng = address => { policyCalls++; return policy(address); };
                    var accessible = view.Ax(view.Id("blocked"));
                    Check(accessible != null, "policy accessible link fixture");
                    typeof(V).GetMethod("ActivateAccessibility", Members).Invoke(view, new object[] { accessible });
                    Observe(50);
                    Check(policyCalls == 1 && UiErrors.Count == 0, "blocked accessibility activation escaped or skipped policy");
                    view.Js("go('" + site.Url + "script')");
                    Call(form, "q1", site.Url + "address");
                    settings["h"] = site.Url + "home";
                    Member<ToolStripItem>(form, "HO").PerformClick();
                    Observe(100);
                    Check(site.Hits == 0 && view.At == original && !view.Busy && UiErrors.Count == 0, "denial sent a request, changed page, or raised a popup");

                    settings["bs"] = Collection("example.test");
                    Check(!view.Ng(new Uri("https://child.example.test/")) && view.Ng(new Uri("https://notexample.test/")), "blocked domain boundary");
                    settings["bs"] = new StringCollection();
                    settings["cfEnabled"] = "e";
                    settings["cf"] = Collection("contains|guarded");
                    CheckBlocked(() => view.Go(site.Url + "guarded"), "main-tab content filter");
                    settings["cfx"] = Collection("127.0.0.1");
                    Check(view.Ng(new Uri(site.Url + "guarded")), "content-filter exception");
                    settings["cfEnabled"] = "";
                    settings["cfx"] = new StringCollection();

                    view.Go(site.Url + "first");
                    view.Go(site.Url + "second");
                    var last = view.At;
                    var hits = site.Hits;
                    settings["bs"] = Collection("127.0.0.1");
                    Member<ToolStripItem>(form, "BT").PerformClick();
                    Member<ToolStripItem>(form, "re").PerformClick();
                    Check(view.At == last && !view.CanFw && site.Hits == hits, "blocked Back/Reload changed history or fetched");
                    settings["bs"] = new StringCollection();
                    view.Bk();
                    var first = view.At;
                    settings["bs"] = Collection("127.0.0.1");
                    Member<ToolStripItem>(form, "FT").PerformClick();
                    Check(view.At == first && view.CanFw && site.Hits == hits, "blocked Forward changed history");
                    settings["bs"] = new StringCollection();
                    Set(form, "wo", true);
                    CheckBlocked(() => view.Go(page), "offline file navigation");
                    CheckBlocked(() => view.Go("#offline"), "offline retained fragment");
                    Call(form, "q1", site.Url + "offline");
                    Check(view.At == first && site.Hits == hits, "offline navigation changed state");
                    Set(form, "wo", false);
                    tab.Tag = "locked";
                    CheckBlocked(() => view.Go(page), "locked-tab navigation");
                    Member<ToolStripItem>(form, "BT").PerformClick();
                    Check(view.At == first, "locked Back changed state");
                    tab.Tag = "lea";

                    Set(form, "pv", true);
                    var secret = (V)Call(form, "qleaNew", page);
                    Set(form, "pv", false);
                    WaitLea(tabs);
                    settings["bs"] = Collection("127.0.0.1");
                    Check(secret.Pv && secret.Ng != null, "private view policy missing");
                    CheckBlocked(() => secret.Go(site.Url), "private tab policy");
                    var hidden = Member<Dictionary<string, List<TabPage>>>(form, "hiddenGroupTabs");
                    hidden["Policy"] = new List<TabPage> { tab };
                    tabs.TabPages.Remove(tab);
                    CheckBlocked(() => view.Go(site.Url), "hidden tab policy");
                    settings["bs"] = new StringCollection();
                    view.Go(page);
                    Check(view.U.AbsoluteUri == page, "owned hidden tab denied allowed navigation");
                    hidden.Remove("Policy");
                    CheckBlocked(() => view.Go(site.Url), "detached tab policy");
                    tabs.TabPages.Add(tab);
                    tabs.SelectedTab = tab;
                    view.Go(site.Url + "resume");
                    Call(form, "qleaSuspend", tab);
                    settings["bs"] = Collection("127.0.0.1");
                    var placeholder = tab.Controls[0];
                    var target = (string)Call(form, "qleaUrl", tab);
                    var before = site.Hits;
                    Check(Call(form, "qleaResume", tab, true) == null && tab.Controls[0] == placeholder &&
                        (string)Call(form, "qleaUrl", tab) == target && site.Hits == before, "denied resume lost placeholder or saved target");
                    settings["se"] = site.Url + "search?q=";
                    foreach (var command in new[] { "go", "search", "home" })
                    {
                        Call(form, "qleaDo", command, site.Url + "paused-blocked");
                        Check(tab.Controls[0] == placeholder && (string)Call(form, "qleaUrl", tab) == target && site.Hits == before,
                            "denied paused " + command + " lost the saved page");
                    }
                    settings["bs"] = new StringCollection();
                    tab.Tag = "locked";
                    Call(form, "qleaDo", "go", page);
                    Check(tab.Controls[0] == placeholder, "locked paused address replaced placeholder");
                    tab.Tag = "lea";
                    Set(form, "wo", true);
                    Call(form, "qleaDo", "home", "");
                    Check(tab.Controls[0] == placeholder, "offline paused Home replaced placeholder");
                    Set(form, "wo", false);
                    settings["bs"] = new StringCollection();
                    var resumed = (V)Call(form, "qleaResume", tab, true);
                    WaitLea(tabs);
                    Check(resumed != view && view.IsDisposed && view.Ng == null && resumed.Ng != null && resumed.U.AbsoluteUri == target, "resume did not install/release policy");
                    settings["bs"] = Collection("127.0.0.1");
                    var resumedPage = resumed.At;
                    var resumedHits = site.Hits;
                    CheckBlocked(() => resumed.Rl(), "resume approval must be consumed once");
                    Check(resumed.At == resumedPage && site.Hits == resumedHits, "resume approval bypassed later policy");
                    Call(form, "qleaSuspend", tab);
                    Call(form, "qleaDo", "go", "");
                    Check(tab.Controls[0] is Panel, "empty paused address replaced placeholder");
                    Call(form, "qleaDo", "go", page + "#replacement");
                    WaitLea(tabs);
                    Check(resumed.IsDisposed && ((V)tab.Controls[0]).U.AbsoluteUri == page + "#replacement", "allowed paused address did not replace its saved target");

                    var unopened = (Form)Activator.CreateInstance(form.GetType());
                    forms.Add(unopened);
                    settings["bs"] = Collection("127.0.0.1");
                    var empty = (V)Call(unopened, "qleaNew", site.Url + "unshown");
                    Check(empty.At == null && !empty.Busy && Member<Label>(unopened, "su").Text.Contains("blocked"), "unshown denied initial navigation failed");
                    var retainedGuard = empty.Ng;
                    unopened.Dispose();
                    Check(empty.IsDisposed && empty.Ng == null && !retainedGuard(new Uri(site.Url)), "closed owner still authorizes navigation");
                    Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "main-tab policy loaded MSHTML");
                    Console.WriteLine("PASS policy: domains/filters, GET/POST/multipart, keyboard/accessibility/script, history, offline/lock, private/hidden/paused, ownership and lifecycle");
                }
            }
            finally { if (File.Exists(fixture)) File.Delete(fixture); }
        }

        private static void CheckNavigation(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var first = tabs.SelectedTab;
            var view = (V)first.Controls[0];
            settings["h"] = page + "#home";
            Member<ToolStripItem>(form, "h").PerformClick();
            Wait(() => view.U.AbsoluteUri == page + "#home" && !view.Busy, "Home ignored the saved homepage");
            Menu(form, "Use Current Page As Home").PerformClick();
            Check((string)settings["h"] == page + "#home", "save homepage omitted Lea URL");
            Call(form, "q1", page + "#away");
            Wait(() => view.U.AbsoluteUri == page + "#away" && !view.Busy, "homepage test navigation");
            Member<ToolStripItem>(form, "HO").PerformClick();
            Wait(() => view.U.AbsoluteUri == page + "#home" && !view.Busy, "Home menu ignored saved homepage");
            Menu(form, "Open Homepage In New Tab").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == 2 && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page + "#home", "new homepage did not use Lea");
            settings["h"] = "";
            Member<ToolStripItem>(form, "HO").PerformClick();
            Wait(() => ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page && !((V)tabs.SelectedTab.Controls[0]).Busy, "blank homepage fallback");
            Call(form, "q24", form, EventArgs.Empty);
            Member<ToolStripDropDownItem>(form, "tl").DropDownItems[0].PerformClick();
            Check(tabs.SelectedTab == first && Member<ToolStripComboBox>(form, "c").Text.Contains("#home"), "tab-list selection did not update Lea address");
            Check(Call(form, "qleaDomain") == null, "local file was treated as a web domain");
            Menu(form, "Set Homepage To Current Domain").PerformClick();
            Check((string)settings["h"] == "", "file domain overwrote homepage");
            Console.WriteLine("PASS navigation: saved/default home, new Lea homepage, tab list, and local-file domain guard");

            var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
            settings["ls"] = Collection(page + "#legacy-saved");
            File.WriteAllLines(session, new[] { "javascript:alert(1)", page + "#one", page + "#two" });
            Member<ToolStripItem>(form, "RP").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == 4 && tabs.SelectedIndex == 2 && ((V)tabs.TabPages[2].Controls[0]).U.AbsoluteUri == page + "#one" &&
                ((V)tabs.TabPages[3].Controls[0]).U.AbsoluteUri == page + "#two", "previous session did not prefer and append Lea snapshot");
            Check(first.Controls[0] == view && view.U.AbsoluteUri == page + "#home", "previous session replaced current content");
            File.Delete(session);
            Member<ToolStripItem>(form, "RP").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == 5 && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page + "#legacy-saved", "previous legacy snapshot did not reopen as Lea");
            File.WriteAllText(session, page + "#private-restore");
            Set(form, "pv", true);
            Member<ToolStripItem>(form, "RP").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == 6 && ((V)tabs.SelectedTab.Controls[0]).Pv, "explicit private previous-session restoration");
            Set(form, "pv", false);
            File.WriteAllLines(session, Enumerable.Range(0, 20).Select(index => page + "#limit-" + index).ToArray());
            Member<ToolStripItem>(form, "RP").PerformClick();
            WaitLea(tabs);
            Check(tabs.TabCount == 18 && File.ReadAllLines(session).Length == 20, "previous-session limit or saved records changed");
            var count = tabs.TabCount;
            using (var locked = new FileStream(session, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Member<ToolStripItem>(form, "RP").PerformClick();
                Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("could not be read"), "locked session silently fell back or lost error status");
            }
            File.WriteAllText(session, new string('x', 65537));
            Member<ToolStripItem>(form, "RP").PerformClick();
            Check(tabs.TabCount == count && new FileInfo(session).Length == 65537, "oversized session was opened or changed");
            settings["ls"] = new StringCollection();
            File.WriteAllLines(session, Enumerable.Repeat("invalid", 256).Concat(new[] { page }).ToArray());
            Member<ToolStripItem>(form, "RP").PerformClick();
            Check(tabs.TabCount == count, "session scan exceeded 256 candidates");
            File.Delete(session);
            Member<ToolStripItem>(form, "RP").PerformClick();
            Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("No supported"), "empty session changed tabs");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "navigation commands loaded legacy engine");
            Console.WriteLine("PASS navigation: Lea/legacy snapshots, private opens, append/order, bounds, locked and empty sessions");

            tabs.SelectedTab = first;
            Menu(form, "Close Other Tabs").PerformClick();
            var width = tabs.ItemSize.Width;
            Call(form, "q3", page);
            var old = tabs.SelectedTab;
            Call(form, "qleaNew", page + "#kept");
            var kept = tabs.SelectedTab;
            Call(form, "qleaNew", page + "#right");
            var right = tabs.SelectedTab;
            WaitLea(tabs);
            tabs.SelectedTab = kept;
            Menu(form, "Close Tabs Left Of Current").PerformClick();
            Check(tabs.TabCount == 2 && tabs.SelectedTab == kept && first.IsDisposed && old.IsDisposed && !right.IsDisposed && tabs.ItemSize.Width == width,
                "first mixed left-close failed disposal, selection, or sizing");
            settings["rc"] = new StringCollection();
            Set(form, "pv", true);
            var secret = (V)Call(form, "qleaNew", page + "#private-left");
            Set(form, "pv", false);
            Call(form, "q3", page);
            var legacy = tabs.SelectedTab;
            var browser = (WebBrowser)legacy.Controls[0];
            Wait(() => browser.Url != null && browser.ReadyState == WebBrowserReadyState.Complete, "left-close legacy page load");
            Call(form, "qleaNew", page + "#retained-right");
            var retained = tabs.SelectedTab;
            WaitLea(tabs);
            tabs.SelectedTab = legacy;
            Menu(form, "Close Tabs To The Left").PerformClick();
            Check(tabs.TabCount == 2 && tabs.SelectedTab == legacy && kept.IsDisposed && right.IsDisposed && secret.IsDisposed && !retained.IsDisposed &&
                (int)form.GetType().GetField("I", Members).GetValue(form) == 2, "second mixed left-close failed disposal or bookkeeping");
            Check(!((StringCollection)settings["rc"]).Cast<string>().Any(value => value.Contains("private-left")), "left-close leaked private URL");
            Menu(form, "Close Tabs To The Left").PerformClick();
            Check(tabs.TabCount == 2, "left edge closed unintended tabs");
            using (var stale = new ToolStripMenuItem { Tag = first }) Call(form, "q25", stale, EventArgs.Empty);
            Check(tabs.SelectedTab == legacy, "stale tab-list entry changed selection");
            Console.WriteLine("PASS navigation: both mixed-engine left-close menus, private history, stable counts, and stale tab selection");
        }

        private static void CheckLinks(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var first = tabs.SelectedTab;
            var view = (V)first.Controls[0];
            using (var site = new LoopbackSite())
            {
                Call(form, "q1", site.Url + "path?q=one#part");
                Wait(() => view.U.AbsoluteUri == site.Url + "path?q=one#part" && !view.Busy, "domain source navigation");
                Check(((Uri)Call(form, "qleaDomain")).AbsoluteUri == site.Url, "domain root lost port or retained path/query/fragment");
                Menu(form, "Set Homepage To Current Domain").PerformClick();
                Check((string)settings["h"] == site.Url, "domain homepage lost port");
                Menu(form, "Quick Open Current Domain").PerformClick();
                Wait(() => view.U.AbsoluteUri == site.Url && !view.Busy, "quick domain navigation");
                Menu(form, "Open Current Domain In New Tab").PerformClick();
                WaitLea(tabs);
                Check(tabs.TabCount == 2 && tabs.SelectedTab.Controls[0] is V && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == site.Url, "domain new-tab engine or URL");
                tabs.SelectedTab = first;
                var suspended = Member<Dictionary<TabPage, string>>(form, "suspendedGroupUrls");
                try
                {
                    suspended[first] = "https://user:password@[::1]:8443/a?q=b#c";
                    var domain = (Uri)Call(form, "qleaDomain");
                    Check(domain.Scheme == "https" && domain.Port == 8443 && IPAddress.Parse(domain.Host.Trim('[', ']')).Equals(IPAddress.IPv6Loopback) &&
                        domain.PathAndQuery == "/" && domain.Fragment == "" && domain.UserInfo == "", "domain normalization lost IPv6/port or retained credentials: " + domain.AbsoluteUri);
                }
                finally { suspended.Remove(first); }
                Call(form, "qleaPaste", page.Replace("%20", " ") + "#single", false);
                WaitLea(tabs);
                Check(tabs.TabCount == 3 && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page + "#single", "single raw-space file clipboard URL");
                Call(form, "qleaPaste", site.Url.Substring("http://".Length) + "bare", false);
                WaitLea(tabs);
                Check(tabs.TabCount == 4 && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == site.Url + "bare", "bare clipboard host/port URL");
                var count = tabs.TabCount;
                foreach (var invalid in new[] { "", "not a link", "javascript:alert(1)", "data:text/html,test", "ssh://localhost/", "https://example.invalid/" + new string('x', 4096) })
                    Call(form, "qleaPaste", invalid, false);
                Check(tabs.TabCount == count, "invalid single clipboard input created a tab");
                Call(form, "qleaPaste", new string('x', 65537), true);
                Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("too large"), "character clipboard bound");
                Call(form, "qleaPaste", new string('\u0800', 30000), true);
                Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("too large"), "UTF-8 clipboard bound");
                var batch = "<a href=\"" + site.Url.Replace("http:", "HTTP:") + "batch\">link</a>\n" +
                    String.Join("\n", Enumerable.Range(0, 19).Select(index => page + "#batch-" + index).ToArray());
                Call(form, "qleaPaste", batch, true);
                Check(tabs.TabCount == count + 12 && tabs.SelectedIndex == count && Member<Label>(form, "su").Text.Contains("8 URL(s) skipped"), "batch URL limits/reporting/selection");
                WaitLea(tabs);
                Check(((V)tabs.TabPages[count].Controls[0]).U.AbsoluteUri == site.Url + "batch" && ((V)tabs.TabPages[count + 1].Controls[0]).U.AbsoluteUri == page + "#batch-0", "case-insensitive batch extraction and order");
                count = tabs.TabCount;
                Call(form, "qleaPaste", String.Join("\n", Enumerable.Repeat("http://[]/", 256).Concat(new[] { site.Url + "beyond" }).ToArray()), true);
                Check(tabs.TabCount == count, "clipboard batch exceeded 256 candidates");
                Set(form, "pv", true);
                Call(form, "qleaPaste", page + "#private-paste", false);
                WaitLea(tabs);
                Check(((V)tabs.SelectedTab.Controls[0]).Pv, "private single paste created public view");
                Call(form, "qleaPaste", page + "#private-batch", true);
                WaitLea(tabs);
                Check(((V)tabs.SelectedTab.Controls[0]).Pv, "private batch paste created public view");
                Set(form, "pv", false);
                Check(tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is V), "link opening created legacy controls");
                Call(form, "q3", site.Url + "legacy?q=one#part");
                var legacy = (WebBrowser)tabs.SelectedTab.Controls[0];
                Wait(() => legacy.Url != null && legacy.ReadyState == WebBrowserReadyState.Complete, "legacy domain source");
                Menu(form, "Quick Open Current Domain").PerformClick();
                Wait(() => legacy.Url.AbsoluteUri == site.Url && legacy.ReadyState == WebBrowserReadyState.Complete, "legacy site-root navigation");
                Check(tabs.SelectedTab.Controls[0] == legacy, "current domain navigation replaced legacy engine");
                Menu(form, "Open Current Domain In New Tab").PerformClick();
                WaitLea(tabs);
                Check(tabs.SelectedTab.Controls[0] is V && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == site.Url, "legacy source did not open domain in Lea");
            }
            Console.WriteLine("PASS links: loopback domain navigation, port/IPv6 normalization, bounded single/batch paste, order, and privacy");
        }

        private static void CheckSplit(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            settings["bs"] = new StringCollection();
            settings["cfEnabled"] = "";
            settings["cfx"] = new StringCollection();
            var folder = Path.Combine(Path.GetTempPath(), "lea-split-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                using (var site = new LoopbackSite())
                {
                    var leftPath = Path.Combine(folder, "left.html");
                    var rightPath = Path.Combine(folder, "right.html");
                    var filler = String.Concat(Enumerable.Range(0, 100).Select(index => "<p>Shared row " + index + "</p>").ToArray());
                    File.WriteAllText(leftPath, "<!doctype html><html><head><title>Left sample</title><style>p{margin:12px} .hidden{visibility:hidden}</style></head><body><h1>Shared heading</h1><p>Left marker &lt;script&gt;</p><p>Shared words</p><p class=hidden>Hidden secret</p><div style='visibility:hidden'><p style='visibility:visible'>Visible child</p></div><table><tr><td>Left cell</td></tr></table><a id=blocked href='" + site.Url + "blocked'>Blocked link</a><form method=get action='" + site.Url + "get'><input name=term value=initial><input type=submit></form><form method=post action='" + site.Url + "post'><input name=term value=initial><input type=submit></form><form method=post enctype='multipart/form-data' action='" + site.Url + "multipart'><input name=term value=initial><input type=submit></form>" + filler + "</body></html>");
                    File.WriteAllText(rightPath, "<!doctype html><html><head><title>Right sample</title></head><body><h1>Shared heading</h1><p>Right marker</p><p>shared WORDS</p><p>Shared words</p>" + filler + "</body></html>");
                    var tabs = Member<TabControl>(form, "k");
                    var source = (V)tabs.SelectedTab.Controls[0];
                    source.Go(leftPath);
                    source.At.Fs[0].V = "unsent owner value";
                    Set(form, "pv", true);
                    Call(form, "qleaNew", rightPath);
                    WaitLea(tabs);
                    var privateSource = (V)tabs.SelectedTab.Controls[0];
                    Set(form, "pv", false);
                    Menu(form, "Side-By-Side Tab View").PerformClick();
                    var split = SplitWindow(forms);
                    WaitSplit(split);
                    var left = Member<V>(split, "leftBrowser");
                    var right = Member<V>(split, "rightBrowser");
                    Check(left != source && right != privateSource && !left.Pv && right.Pv && source.At.Fs[0].V == "unsent owner value", "split context/source privacy and field retention");
                    Check(left.U.LocalPath == leftPath && right.U.LocalPath == rightPath && left.At.Fs[0].V == "initial", "split copies should freshly load each selected URL");
                    DrawSplit(split, "split-desktop.png");
                    var sync = Member<CheckBox>(split, "syncScroll");
                    sync.Checked = true;
                    left.Sc(180);
                    Call(split, "qsynctick", split, EventArgs.Empty);
                    Check(left.Sy == 180 && right.Sy == 180, "left-to-right scroll synchronization");
                    right.Sc(90);
                    Call(split, "qsynctick", split, EventArgs.Empty);
                    Check(left.Sy == 270 && right.Sy == 270, "right-to-left scroll synchronization");
                    sync.Checked = false;
                    left.Sc(50);
                    Call(split, "qsynctick", split, EventArgs.Empty);
                    Check(left.Sy == 320 && right.Sy == 270, "disabled scroll synchronization");
                    Member<Button>(split, "differenceButton").PerformClick();
                    var difference = Member<Form>(split, "differenceWindow");
                    var leftText = difference.Controls.Find("LeftOnly", true).Cast<TextBox>().Single().Text;
                    var rightText = difference.Controls.Find("RightOnly", true).Cast<TextBox>().Single().Text;
                    Check(leftText.Contains("Left marker <script>") && leftText.Contains("Left cell") && leftText.Contains("Visible child") &&
                        !leftText.Contains("Shared words") && !leftText.Contains("Hidden secret") && !leftText.Contains("unsent owner value") && rightText == "Right marker", "in-memory comparison text/table/visibility/case semantics; left=" + leftText + "; right=" + rightText);
                    Check(!Directory.GetFiles(folder).Any(path => path.Contains("Differences")) && tabs.TabCount == 2, "comparison created a browser tab or fixture file");
                    Member<Button>(split, "differenceButton").PerformClick();
                    Check(difference.IsDisposed && Member<Form>(split, "differenceWindow") != difference, "comparison reuse should dispose previous window");

                    settings["bs"] = Collection("127.0.0.1");
                    var original = left.At;
                    CheckBlocked(() => left.Go(site.Url), "split direct navigation");
                    CheckBlocked(() => left.GoA(site.Url).GetAwaiter().GetResult(), "split async navigation");
                    foreach (var submit in left.At.Fs.Where(field => field.K == "submit"))
                    {
                        CheckBlocked(() => left.Sm(submit), "split form submission");
                        CheckBlocked(() => left.SmA(submit).GetAwaiter().GetResult(), "split async form submission");
                    }
                    for (var index = 0; index < left.Kn && left.Ke != left.Id("blocked"); index++) left.Kf();
                    Check(left.Ke == left.Id("blocked"), "split keyboard link fixture");
                    CheckBlocked(() => left.Ka().GetAwaiter().GetResult(), "split keyboard link navigation");
                    left.Js("go('" + site.Url + "script')");
                    Observe(100);
                    Check(site.Hits == 0 && left.At == original && !left.Busy && Member<Label>(split, "status").Text.Contains("blocked"), "split navigation guard bypassed or changed document");
                    settings["bs"] = new StringCollection();
                    settings["cfEnabled"] = "e";
                    settings["cf"] = Collection("contains|guarded");
                    CheckBlocked(() => right.Go(site.Url + "guarded"), "split content-filter navigation");
                    settings["cfEnabled"] = "";
                    settings["cf"] = new StringCollection();
                    var work = left.GoA(site.Url + "allowed");
                    Wait(() => work.IsCompleted, "allowed split navigation");
                    work.GetAwaiter().GetResult();
                    Check(site.Hits == 1 && left.U.AbsoluteUri == site.Url + "allowed" && source.U.LocalPath == leftPath, "allowed split link changed source tab or request count");

                    Member<ComboBox>(split, "leftChoice").SelectedIndex = 1;
                    WaitSplit(split);
                    var copied = Member<V>(split, "leftBrowser");
                    Check(left.IsDisposed && left.Ng == null && copied.Pv && copied.U.LocalPath == rightPath && !right.IsDisposed, "split choice replacement leaked context or lost privacy");
                    sync.Checked = true;
                    DrawSplit(split, null);
                    copied.Sc(100000);
                    Call(split, "qsynctick", split, EventArgs.Empty);
                    var beforeLeft = copied.Sy;
                    var beforeRight = right.Sy;
                    Call(split, "qsynctick", split, EventArgs.Empty);
                    Check(copied.Sy == beforeLeft && right.Sy == beforeRight, "clamped scroll positions oscillate");
                    split.Size = split.MinimumSize;
                    DrawSplit(split, "split-small.png");
                    var layout = Member<SplitContainer>(split, "split");
                    Check(layout.Panel1.Width >= 180 && layout.Panel2.Width >= 180 && Member<Button>(split, "differenceButton").Width >= 130, "small split toolbar/pane geometry");
                    difference = Member<Form>(split, "differenceWindow");
                    split.Close();
                    Check(split.IsDisposed && copied.IsDisposed && right.IsDisposed && difference.IsDisposed &&
                        !Member<System.Windows.Forms.Timer>(split, "syncTimer").Enabled && !source.IsDisposed && !privateSource.IsDisposed, "split close lifecycle or source isolation");
                    Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "Lea split view loaded MSHTML");
                    Console.WriteLine("PASS split: native panes, privacy, field retention, guard/link/forms, bidirectional scroll, in-memory comparison, bounded layout, and disposal");
                }
            }
            finally { Directory.Delete(folder, true); }
        }

        private static void CheckSplitLife(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            settings["bs"] = new StringCollection();
            settings["cfEnabled"] = "";
            using (var site = new LoopbackSite(1200))
            using (var closingSite = new LoopbackSite(1200))
            {
                var tabs = Member<TabControl>(form, "k");
                var original = (V)tabs.SelectedTab.Controls[0];
                settings["lazyRestoreMode"] = "all";
                Set(form, "pv", true);
                Call(form, "qlazyopenurls", new List<string> { site.Url + "private" });
                var privateTab = tabs.SelectedTab;
                Set(form, "pv", false);
                Call(form, "qlazyopenurls", new List<string> { page + "#hidden" });
                var hidden = tabs.SelectedTab;
                settings["tg"] = Collection("Split hidden|" + Color.Teal.ToArgb() + "|active|" + Call(form, "qb64", page + "#hidden"));
                Call(form, "qcollapsegroup", "Split hidden");
                var invalid = new TabPage("Invalid");
                Call(form, "qleaPlaceholder", invalid, "javascript:alert(1)", false);
                tabs.TabPages.Add(invalid);
                var empty = new TabPage("Empty");
                tabs.TabPages.Add(empty);
                Call(form, "q862", form, EventArgs.Empty);
                var split = SplitWindow(forms);
                Wait(() => Member<Control>(split, "rightBrowser") is V && site.Hits == 1, "split pending request started");
                var pending = Member<V>(split, "rightBrowser");
                Check(pending.Busy && pending.Pv && privateTab.Controls[0] is Panel && hidden.Controls[0] is Panel &&
                    Member<ComboBox>(split, "leftChoice").Items.Count == 2 && !Member<Button>(split, "differenceButton").Enabled, "split deferred/hidden/invalid source handling");
                Member<ComboBox>(split, "rightChoice").SelectedIndex = 0;
                WaitSplit(split);
                Check(pending.IsDisposed && pending.Ng == null && !Member<V>(split, "rightBrowser").Pv && !original.IsDisposed &&
                    privateTab.Controls[0] is Panel, "split pending replacement cancellation and fresh public context");
                var right = Member<V>(split, "rightBrowser");
                Set(form, "pv", true);
                right.Go(page + "#private-navigation");
                Set(form, "pv", false);
                Check(right.Pv, "split navigation did not retain global private upgrade");
                Member<ComboBox>(split, "rightChoice").SelectedIndex = 1;
                WaitSplit(split);
                Check(right.IsDisposed && Member<V>(split, "rightBrowser").Pv, "private source choice state");
                Member<ComboBox>(split, "rightChoice").SelectedIndex = 0;
                WaitSplit(split);
                Check(!Member<V>(split, "rightBrowser").Pv, "new public context reused private renderer");
                Member<Button>(split, "differenceButton").PerformClick();
                var comparison = Member<Form>(split, "differenceWindow");
                Check(comparison != null && tabs.TabCount == 4, "comparison should stay outside browser sessions");
                Call(form, "qleaSaveSession");
                var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
                Check(!File.ReadAllText(session).Contains(site.Url), "split/private source URL entered automatic session");

                var left = Member<V>(split, "leftBrowser");
                var work = left.GoA(closingSite.Url + "pending-close");
                Wait(() => closingSite.Hits == 1 && left.Busy, "owner-close pending split request");
                form.Close();
                Wait(() => work.IsCompleted, "owner-close pending work cancellation");
                var cancelled = false;
                try { work.GetAwaiter().GetResult(); } catch (Lea.Engine.Er error) { cancelled = error.K == "cancel"; }
                Check(cancelled && form.IsDisposed && split.IsDisposed && left.IsDisposed && comparison.IsDisposed && left.Ng == null &&
                    !Member<System.Windows.Forms.Timer>(split, "syncTimer").Enabled, "owner closure did not dispose/cancel owned split surfaces");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "split lifecycle loaded legacy engine");
                Console.WriteLine("PASS split lifecycle: hidden/deferred sources, async replacement, private context upgrades/reset, session isolation, owner-close cancellation, and timer disposal");
            }
        }

        private static void CheckSplitLimits(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            var folder = Path.Combine(Path.GetTempPath(), "lea-split-limits-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                var leftPath = Path.Combine(folder, "left.html");
                var rightPath = Path.Combine(folder, "right.html");
                File.WriteAllText(leftPath, "<html><body>" + String.Concat(Enumerable.Range(0, 180).Select(index => "<p>Left row " + index + "</p>").ToArray()) + "</body></html>");
                File.WriteAllText(rightPath, "<html><body>" + String.Concat(Enumerable.Range(0, 180).Select(index => "<p>Right row " + index + "</p>").ToArray()) + "</body></html>");
                var tabs = Member<TabControl>(form, "k");
                ((V)tabs.SelectedTab.Controls[0]).Go(leftPath);
                Call(form, "qleaNew", rightPath);
                WaitLea(tabs);
                Call(form, "q862", form, EventArgs.Empty);
                var split = SplitWindow(forms);
                WaitSplit(split);
                Member<Button>(split, "differenceButton").PerformClick();
                var difference = Member<Form>(split, "differenceWindow");
                foreach (var name in new[] { "LeftOnly", "RightOnly" })
                {
                    var text = difference.Controls.Find(name, true).Cast<TextBox>().Single();
                    Check(text.Lines.Length == 150 && text.AccessibleName.Contains("limited") && Encoding.UTF8.GetByteCount(text.Text) < 65536,
                        "comparison difference count/byte limits");
                }
                var extractor = split.GetType().GetMethod("qtext", BindingFlags.Static | BindingFlags.NonPublic);
                var normalize = split.GetType().GetMethod("qline", BindingFlags.Static | BindingFlags.NonPublic);
                Check((string)normalize.Invoke(null, new object[] { "A\0B\nC", 5 }) == "A B C" &&
                    (string)normalize.Invoke(null, new object[] { "A\ud83d\ude00B", 2 }) == "A", "comparison control/surrogate boundary");
                var huge = Path.Combine(folder, "huge.html");
                using (var view = new V())
                {
                    File.WriteAllText(huge, "<html><body>" + String.Concat(Enumerable.Range(0, 20).Select(index => "<p>" + index + " " + new string('\u00e9', 5000) + "</p>").ToArray()) + "</body></html>", Encoding.UTF8);
                    view.Go(huge);
                    var args = new object[] { view, false };
                    var lines = (List<string>)extractor.Invoke(null, args);
                    Check((bool)args[1] && lines.Count > 0 && lines.All(line => line.Length <= 4096) &&
                        Encoding.UTF8.GetByteCount(String.Join(Environment.NewLine, lines.ToArray())) < 65536, "comparison long-line/UTF-8 limits");
                    File.WriteAllText(huge, "<html><body>" + String.Concat(Enumerable.Range(0, 2300).Select(index => "<p>Row " + index + "</p>").ToArray()) + "</body></html>");
                    view.Go(huge);
                    args = new object[] { view, false };
                    lines = (List<string>)extractor.Invoke(null, args);
                    Check((bool)args[1] && lines.Count == 2048 && lines[0] == "Row 0", "comparison unique-block ceiling");
                }
                split.Close();
                var extra = new List<TabPage>();
                for (var index = 0; index < 260; index++)
                {
                    var tab = new TabPage("Lea");
                    Call(form, "qleaPlaceholder", tab, page + "#candidate-" + index, false);
                    tab.Text = new string('t', 400);
                    tabs.TabPages.Add(tab);
                    extra.Add(tab);
                }
                Call(form, "q862", form, EventArgs.Empty);
                split = SplitWindow(forms);
                WaitSplit(split);
                var choices = Member<ComboBox>(split, "leftChoice").Items.Cast<object>().ToArray();
                Check(choices.Length == 256 && choices.All(choice => Member<string>(choice, "Label").Length <= 256), "split candidate/label ceiling");
                split.Close();
                foreach (var tab in extra) Call(form, "qleaDrop", tab);
                extra.Clear();
                for (var index = 0; index < 20; index++)
                {
                    var tab = new TabPage("Lea");
                    Call(form, "qleaPlaceholder", tab, "http://127.0.0.1:1/" + index + "?q=" + new string('x', 3900), false);
                    tabs.TabPages.Add(tab);
                    extra.Add(tab);
                }
                Call(form, "q862", form, EventArgs.Empty);
                split = SplitWindow(forms);
                WaitSplit(split);
                choices = Member<ComboBox>(split, "leftChoice").Items.Cast<object>().ToArray();
                Check(choices.Length < 22 && choices.Length > 2 && choices.Sum(choice => Encoding.UTF8.GetByteCount(Member<string>(choice, "Url"))) <= 65536 &&
                    extra.All(tab => tab.Controls[0] is Panel), "split URL-byte ceiling or unselected deferred loads");
                split.Close();
                foreach (var tab in tabs.TabPages.Cast<TabPage>().ToArray()) Call(form, "qleaDrop", tab);
                Call(form, "qfeature6prunestate");
                Call(form, "q862", form, EventArgs.Empty);
                Check(!Application.OpenForms.Cast<Form>().Any(window => window.GetType().Name == "SplitBrowserForm") &&
                    Member<Label>(form, "su").Text.Contains("No supported tabs"), "empty split source guard");
                Console.WriteLine("PASS split limits: 150 differences, 2048 blocks, UTF-8/line/control bounds, 256 candidates, 64 KiB URLs, deferred isolation, and empty input");
            }
            finally { Directory.Delete(folder, true); }
        }

        private static Form SplitWindow(List<Form> forms)
        {
            var split = Application.OpenForms.Cast<Form>().FirstOrDefault(window => window.GetType().Name == "SplitBrowserForm" && !forms.Contains(window));
            Check(split != null, "split-view command did not open a window");
            split.Opacity = 0;
            split.ShowInTaskbar = false;
            split.Location = new Point(-30000, -30000);
            forms.Add(split);
            return split;
        }

        private static void WaitSplit(Form split)
        {
            Wait(() => Member<Control>(split, "leftBrowser") is V && Member<Control>(split, "rightBrowser") is V,
                "split view still creates legacy browser controls");
            Wait(() => Member<V>(split, "leftBrowser").At != null && !Member<V>(split, "leftBrowser").Busy &&
                Member<V>(split, "rightBrowser").At != null && !Member<V>(split, "rightBrowser").Busy, "split pane loads");
        }

        private static void CheckBlocked(Action action, string name)
        {
            var blocked = false;
            try { action(); } catch (Lea.Engine.Er error) { blocked = error.K == "blocked"; }
            Check(blocked, name + " was not rejected");
        }

        private static void DrawSplit(Form split, string file)
        {
            split.PerformLayout();
            using (var bitmap = new Bitmap(split.Width, split.Height))
            {
                split.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var colors = new HashSet<int>();
                for (var y = 50; y < bitmap.Height - 30; y += 7)
                    for (var x = 10; x < bitmap.Width - 10; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                Check(colors.Count > 12, "split paint is blank");
                var folder = Environment.GetEnvironmentVariable("LEA_SPLIT_SHOTS");
                if (file != null && !String.IsNullOrEmpty(folder)) { Directory.CreateDirectory(folder); bitmap.Save(Path.Combine(folder, file)); }
            }
        }

        private static void CheckDomains(Form form, ApplicationSettingsBase settings, string page)
        {
            using (var firstSite = new LoopbackSite())
            using (var secondSite = new LoopbackSite())
            {
                var tabs = Member<TabControl>(form, "k");
                var first = tabs.SelectedTab;
                var firstView = (V)first.Controls[0];
                firstView.Go(firstSite.Url + "first");
                Call(form, "qleaNew", secondSite.Url + "second");
                WaitLea(tabs);
                var second = tabs.SelectedTab;
                tabs.SelectedTab = first;
                Menu(form, "Reload Tabs From Same Domain").PerformClick();
                WaitLea(tabs);
                Check(firstSite.Hits == 2 && secondSite.Hits == 2, "same-domain reload did not reach both Lea tabs across ports");
                Menu(form, "Reload All Tabs From Same Domain").PerformClick();
                WaitLea(tabs);
                Check(firstSite.Hits == 3 && secondSite.Hits == 3, "second same-domain reload route");
                firstSite.DelayMs = 400;
                Call(form, "qleaReload", firstView);
                Wait(() => firstSite.Hits == 4 && firstView.Busy, "busy domain fixture");
                Menu(form, "Reload Tabs From Same Domain").PerformClick();
                var report = Member<Label>(form, "su").Text;
                WaitLea(tabs);
                firstSite.DelayMs = 0;
                Check(firstSite.Hits == 4 && secondSite.Hits == 4 && report.Contains("1 busy or suspended"), "domain reload did not skip busy context");

                var formsPage = Path.Combine(Path.GetDirectoryName(new Uri(page).LocalPath), "..", "test-pages", "forms.html");
                Call(form, "qleaNew", formsPage);
                WaitLea(tabs);
                var unrelated = tabs.SelectedTab;
                var editor = (V)unrelated.Controls[0];
                editor.At.Fs[0].V = "domain unsent value";
                settings["lazyRestoreMode"] = "all";
                Call(form, "qlazyopenurls", new List<string> { firstSite.Url + "paused", firstSite.Url.Replace("127.0.0.1", "localhost") + "other-host" });
                var paused = tabs.SelectedTab;
                var otherHost = tabs.TabPages[tabs.TabCount - 1];
                Set(form, "pv", true);
                Call(form, "qlazyopenurls", new List<string> { secondSite.Url + "private-domain" });
                var privateTab = tabs.SelectedTab;
                Set(form, "pv", false);
                Call(form, "qleaNew", firstSite.Url + "hidden");
                WaitLea(tabs);
                var hidden = tabs.SelectedTab;
                var hiddenView = (V)hidden.Controls[0];
                settings["tg"] = Collection("Hidden|" + Color.Teal.ToArgb() + "|active|" + Call(form, "qb64", firstSite.Url + "hidden"));
                Call(form, "qcollapsegroup", "Hidden");
                tabs.SelectedTab = first;
                var matches = (List<TabPage>)Call(form, "qleaDomainTabs");
                Check(matches.SequenceEqual(new[] { first, second, paused, privateTab }), "domain matcher included hidden/file/other-host tabs");
                Check((string)Call(form, "qleaSite", "https://EXAMPLE.test:443/a") == "example.test" &&
                    (string)Call(form, "qleaSite", "http://b\u00fccher.test/a") == (string)Call(form, "qleaSite", "https://xn--bcher-kva.test/b"), "case/IDN domain normalization");
                Check((string)Call(form, "qleaSite", page) == "" && (string)Call(form, "qleaSite", "https://example.test/" + new string('x', 4096)) == "", "domain accepted file/overlong URL");
                var hitsA = firstSite.Hits;
                var hitsB = secondSite.Hits;
                Menu(form, "Reload Tabs From Same Domain").PerformClick();
                report = Member<Label>(form, "su").Text;
                WaitLea(tabs);
                Check(firstSite.Hits == hitsA + 1 && secondSite.Hits == hitsB + 1 && report.Contains("2 busy or suspended") &&
                    paused.Controls[0] is Panel && privateTab.Controls[0] is Panel, "domain reload woke deferred/hidden tabs");
                var originalPage = firstView.At;
                var count = tabs.TabCount;
                Menu(form, "Duplicate Tabs From Same Domain").PerformClick();
                WaitLea(tabs);
                var copies = tabs.TabPages.Cast<TabPage>().Skip(count).ToArray();
                Check(copies.Length == 4 && tabs.SelectedTab == copies[0] && copies.All(tab => tab.Controls[0] is V), "domain duplicate engine/count/selection");
                Check(((V)copies[3].Controls[0]).Pv && copies.Take(3).All(tab => !((V)tab.Controls[0]).Pv), "domain duplicate private flags");
                Check(firstView.At == originalPage && paused.Controls[0] is Panel && privateTab.Controls[0] is Panel && editor.At.Fs[0].V == "domain unsent value", "domain duplicate changed original state");
                tabs.SelectedTab = first;
                Menu(form, "Bookmark Tabs From Same Domain").PerformClick();
                Check(((StringCollection)settings["bm"]).Count == 8, "domain bookmarks omitted paused/private/duplicated URLs");
                var shortcut = new KeyEventArgs(Keys.Control | Keys.Shift | Keys.B);
                Call(form, "q8", form, shortcut);
                Check(shortcut.SuppressKeyPress && ((StringCollection)settings["bm"]).Count == 16, "domain bookmark keyboard route");
                Call(form, "q143", form, EventArgs.Empty);
                Check(((StringCollection)settings["bm"]).Contains(firstSite.Url), "domain root bookmark dropped port");
                first.Text = "Title\r\nInjected\tText";
                var text = (string)Call(form, "qleaDomainText", false);
                var snapshot = (string)Call(form, "qleaDomainText", true);
                Check(text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length == 8 && text.Contains("Title  Injected Text") &&
                    !text.Contains("other-host") && !text.Contains("hidden") && !text.Contains("forms.html"), "domain copy payload/sanitization/matching");
                Check(snapshot.Contains("Domain Root: " + firstSite.Url) && snapshot.Contains("Current URL: " + firstView.U.AbsoluteUri), "domain snapshot root/port/current URL");
                Menu(form, "Jump To Next Tab With Same Domain").PerformClick();
                Check(tabs.SelectedTab == second, "domain jump did not skip unrelated tabs");
                tabs.SelectedTab = copies[3];
                Menu(form, "Jump To Next Tab With Same Domain").PerformClick();
                Check(tabs.SelectedTab == first, "domain jump did not wrap");
                settings["rc"] = new StringCollection();
                var removed = ((List<TabPage>)Call(form, "qleaDomainTabs")).Where(tab => tab != first).ToArray();
                var menus = removed.Select(tab => tab.Controls[0].ContextMenuStrip).Where(menu => menu != null).ToArray();
                Menu(form, "Close Tabs From Same Domain").PerformClick();
                Check(tabs.SelectedTab == first && removed.All(tab => tab.IsDisposed) && menus.All(menu => menu.IsDisposed) &&
                    !hidden.IsDisposed && !hiddenView.IsDisposed && otherHost.Controls[0] is Panel && editor.At.Fs[0].V == "domain unsent value", "domain close ownership/selection/hidden preservation");
                Check(!((StringCollection)settings["rc"]).Cast<string>().Any(url => url.Contains("private-domain")), "domain close wrote private history");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "native domain commands loaded MSHTML");
                tabs.SelectedTab = unrelated;
                count = tabs.TabCount;
                var windows = Application.OpenForms.Count;
                foreach (var action in new[] { "rl", "cl", "dp", "wn", "nx", "bm" }) Call(form, "qleaDomainAction", action);
                Check(tabs.TabCount == count && Application.OpenForms.Count == windows && (string)Call(form, "qleaDomainText", false) == "", "file-domain commands changed browser state");

                Call(form, "q3", secondSite.Url + "legacy");
                var legacyTab = tabs.SelectedTab;
                var legacy = (WebBrowser)legacyTab.Controls[0];
                Wait(() => legacy.Url != null && legacy.ReadyState == WebBrowserReadyState.Complete, "domain legacy fixture");
                hitsA = firstSite.Hits;
                hitsB = secondSite.Hits;
                Menu(form, "Reload All Tabs From Same Domain").PerformClick();
                Wait(() => firstSite.Hits == hitsA + 1 && secondSite.Hits == hitsB + 1 && !firstView.Busy && !legacy.IsBusy, "mixed domain reload");
                Menu(form, "Duplicate Tabs From Same Domain").PerformClick();
                WaitLea(tabs);
                Check(!legacy.IsDisposed && tabs.SelectedTab.Controls[0] is V, "mixed domain duplicate replaced source engine");
                tabs.SelectedTab = first;
                Menu(form, "Close Tabs From Same Domain").PerformClick();
                Check(legacy.IsDisposed && legacyTab.IsDisposed && !firstView.IsDisposed, "mixed domain close did not dispose legacy view");
                Console.WriteLine("PASS domains: exact reloads, busy/paused skips, host matching, retained fields, copies/privacy, bookmarks/shortcut, exports, jump, and mixed disposal");
            }
        }

        private static void SelectDeferred(Form form, TabPage tab)
        {
            var batch = (bool)Member<object>(form, "qleaGroupChange");
            Set(form, "qleaGroupChange", true);
            try { Member<TabControl>(form, "k").SelectedTab = tab; }
            finally { Set(form, "qleaGroupChange", batch); }
        }

        private static void CheckDomainPrivate(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            using (var site = new LoopbackSite())
            {
                var tabs = Member<TabControl>(form, "k");
                var original = tabs.SelectedTab.Controls[0];
                settings["lazyRestoreMode"] = "all";
                settings["nf"] = site.Url + "unexpected-home";
                Set(form, "pv", true);
                Call(form, "qlazyopenurls", new List<string> { site.Url + "source-0" });
                Set(form, "pv", false);
                Call(form, "qlazyopenurls", Enumerable.Range(1, 11).Select(index => site.Url + "source-" + index).ToList());
                Call(form, "qlazyopenurls", Enumerable.Range(12, 8).Select(index => site.Url + "source-" + index).ToList());
                var sources = tabs.TabPages.Cast<TabPage>().Skip(1).ToArray();
                SelectDeferred(form, sources[0]);
                Menu(form, "Open All Tabs From Same Domain In New Window").PerformClick();
                var report = Member<Label>(form, "su").Text;
                var window = NewWindow(form, forms);
                var restored = Member<TabControl>(window, "k");
                Observe(300);
                Check(restored.TabCount == 12 && restored.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is Panel) && site.Hits == 0 &&
                    (string)Call(window, "qleaUrl", restored.TabPages[0]) == site.Url + "source-0", "domain window dropped first URL, exceeded bound, or loaded default page");
                Check((bool)Member<object>(window, "pv") && !(bool)Member<object>(form, "pv") && report.Contains("8 skipped"), "domain window privacy upgrade or bounded report");
                Set(window, "pv", false);
                Check(restored.TabPages.Cast<TabPage>().All(tab => (bool)Call(window, "qleaPrivate", tab)), "private domain window lost deferred privacy");
                Call(window, "qleaSaveSession");
                var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
                Check(!File.Exists(session) || !File.ReadAllText(session).Contains(site.Url), "private domain window entered automatic sessions");
                window.Close();
                Check(sources.All(tab => tab.Controls[0] is Panel) && !original.IsDisposed, "domain window changed source documents");
                var count = tabs.TabCount;
                Menu(form, "Duplicate Tabs From Same Domain").PerformClick();
                report = Member<Label>(form, "su").Text;
                WaitLea(tabs);
                var copies = tabs.TabPages.Cast<TabPage>().Skip(count).ToArray();
                Check(copies.Length == 12 && site.Hits == 12 && ((V)copies[0].Controls[0]).Pv && copies.Skip(1).All(tab => !((V)tab.Controls[0]).Pv) &&
                    report.Contains("8 skipped") && sources.All(tab => tab.Controls[0] is Panel), "domain duplicate bound/eager policy/per-source privacy");
                for (var batch = 0; batch < 2; batch++)
                    Call(form, "qlazyopenurls", Enumerable.Range(batch * 12, 12).Select(index => site.Url + "long-" + index + "?q=" + new string('x', 3900)).ToList());
                SelectDeferred(form, sources[0]);
                sources[0].Text = new string('\u00e9', 300);
                foreach (var snapshot in new[] { false, true })
                {
                    var text = (string)Call(form, "qleaDomainText", snapshot);
                    Check(Encoding.UTF8.GetByteCount(text) <= 65536 && text.Contains("omitted by export limits") &&
                        !text.Contains("long-23?") && !text.Contains(new string('\u00e9', 257)), "domain export byte/title limits");
                }
                Check(site.Hits == 12 && sources.All(tab => tab.Controls[0] is Panel), "domain exports woke deferred pages");
                Check((string)Call(form, "qleaDomainLine", "A\0B\nC", 5) == "A B C" &&
                    (string)Call(form, "qleaDomainLine", "A\ud83d\ude00B", 2) == "A", "domain label controls/surrogate boundary");
                foreach (var tab in tabs.TabPages.Cast<TabPage>().Where(tab => ((string)Call(form, "qleaUrl", tab)).Contains("long-")).ToArray())
                    Call(form, "qleaDrop", tab);
                var extra = new List<TabPage>();
                for (var index = 0; index < 260; index++)
                {
                    var tab = new TabPage("Lea");
                    Call(form, "qleaPlaceholder", tab, site.Url + "row-" + index, false);
                    tabs.TabPages.Add(tab);
                    extra.Add(tab);
                }
                var bounded = (string)Call(form, "qleaDomainText", false);
                var omitted = ((List<TabPage>)Call(form, "qleaDomainTabs")).Count - 256;
                Check(bounded.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length == 257 &&
                    bounded.Contains("[" + omitted + " tab(s) omitted") && Encoding.UTF8.GetByteCount(bounded) < 65536, "domain export row ceiling");
                foreach (var tab in extra) Call(form, "qleaDrop", tab);
                Call(form, "qfeature6prunestate");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "domain window loaded legacy engine");
                Console.WriteLine("PASS private domains: first URL, seeded lazy window, privacy upgrade, bounded eager duplicates, sticky flags, original views, and UTF-8 export limits");
            }
        }

        private static void CheckBulk(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var original = tabs.SelectedTab;
            var view = (V)original.Controls[0];
            view.Go(Path.Combine(Path.GetDirectoryName(new Uri(page).LocalPath), "..", "test-pages", "forms.html"));
            view.At.Fs[0].V = "bulk unsent value";
            Call(form, "qopenurls", new List<string> { page + "#bulk-one", page + "#bulk-two" });
            WaitLea(tabs);
            Check(tabs.TabCount == 3 && original.Controls[0] == view && view.At.Fs[0].V == "bulk unsent value" &&
                tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is V), "bulk opening replaced the first document or created legacy tabs");
            Check(tabs.SelectedIndex == 1, "bulk did not select its first new tab");
            Check((int)Call(form, "qopenurls", (object)null) == 0 &&
                (int)Call(form, "qopenurls", new List<string> { "about:blank", "javascript:alert(1)", "invalid" }) == 0 && tabs.TabCount == 3,
                "invalid bulk input changed tabs");
            settings["lazyRestoreMode"] = "all";
            var count = tabs.TabCount;
            Check((int)Call(form, "qopenurls", Enumerable.Range(0, 20).Select(index => page + "#bulk-" + index).ToList()) == 12, "bulk tab bound");
            WaitLea(tabs);
            Check(tabs.TabCount == count + 12 && tabs.TabPages.Cast<TabPage>().Skip(count).All(tab => tab.Controls[0] is V), "direct bulk opening incorrectly became lazy");
            settings["bm"] = Collection("Existing|" + page);
            settings["wp"] = Collection("Test|T|" + Call(form, "qb64", page + "#workspace-one"),
                "Test|T|" + Call(form, "qb64", page + "#workspace-two"), "Test|T|" + Call(form, "qb64", "about:blank"),
                "Test|M|" + Call(form, "qb64", "Workspace|" + page));
            var records = ((StringCollection)settings["wp"]).Cast<string>().ToArray();
            count = tabs.TabCount;
            Call(form, "qopenworkspace", "Test");
            Check(tabs.TabCount == count + 2 && tabs.SelectedIndex == count && tabs.TabPages[count].Controls[0] is Panel &&
                tabs.TabPages[count + 1].Controls[0] is Panel, "workspace did not append Lea lazy tabs");
            Check(Member<Label>(form, "su").Text.Contains("1 URL(s) skipped") && ((StringCollection)settings["bm"]).Count == 2 &&
                ((StringCollection)settings["wp"]).Cast<string>().SequenceEqual(records), "workspace metadata, source preservation, or report");
            Call(form, "qopenworkspace", "Missing");
            Check(tabs.TabCount == count + 2, "missing workspace changed tabs");
            Check(original.Controls[0] == view && view.At.Fs[0].V == "bulk unsent value", "workspace changed existing fields");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "bulk/workspace loaded legacy engine");
            Set(form, "pv", true);
            Call(form, "qopenurls", new List<string> { page + "#private-bulk" });
            WaitLea(tabs);
            Check(((V)tabs.SelectedTab.Controls[0]).Pv, "bulk opening lost private state");
            Set(form, "pv", false);
            Call(form, "q3", page + "#explicit-legacy");
            var legacy = (WebBrowser)tabs.SelectedTab.Controls[0];
            Wait(() => legacy.Url != null && legacy.ReadyState == WebBrowserReadyState.Complete, "bulk legacy fixture");
            var legacyUrl = legacy.Url;
            Call(form, "qopenurls", new List<string> { page + "#after-legacy" });
            WaitLea(tabs);
            Check(tabs.SelectedTab.Controls[0] is V && !legacy.IsDisposed && legacy.Url == legacyUrl && view.At.Fs[0].V == "bulk unsent value",
                "bulk did not preserve mixed-engine documents");
            Console.WriteLine("PASS bulk: preserved documents, eager bounded batches, private views, mixed engines, lazy workspaces, metadata, and counts");
        }

        private static void CheckWindows(Form form, ApplicationSettingsBase settings, string page, List<Form> forms)
        {
            using (var site = new LoopbackSite())
            {
                var original = Member<TabControl>(form, "k").SelectedTab.Controls[0];
                settings["nf"] = site.Url + "unexpected-home";
                settings["lazyRestoreMode"] = "all";
                var urls = new[] { site.Url + "window-one", site.Url + "window-two" };
                settings["lcw"] = Collection(urls);
                Call(form, "q748", form, EventArgs.Empty);
                var window = NewWindow(form, forms);
                var tabs = Member<TabControl>(window, "k");
                Observe(400);
                Check(tabs.TabCount == 2 && tabs.SelectedIndex == 0 && tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is Panel) && site.Hits == 0,
                    "last-window restore created a default/legacy page or loaded deferred URLs");
                Check(Member<List<string>>(window, "qleaInitial") == null, "window retained initial URL seed");
                Member<ToolStripItem>(window, "re").PerformClick();
                WaitLea(tabs);
                Check(site.Hits == 1, "window resume request count");
                settings["tg"] = Collection("Hidden|" + Color.Teal.ToArgb() + "|active|" + Call(window, "qb64", urls[0]));
                Call(window, "qcollapsegroup", "Hidden");
                Set(window, "pv", true);
                Call(window, "qlazyopenurls", new List<string> { site.Url + "private-window" });
                Set(window, "pv", false);
                Call(window, "qrecoverysnapshot");
                Check(((StringCollection)settings["recovery"]).Cast<string>().OrderBy(value => value).SequenceEqual(urls.OrderBy(value => value)),
                    "recovery omitted hidden/paused URLs or saved private URLs");
                window.Close();
                Check(window.IsDisposed && ((StringCollection)settings["lcw"]).Cast<string>().OrderBy(value => value).SequenceEqual(urls.OrderBy(value => value)),
                    "closed-window snapshot omitted Lea/hidden/paused state or leaked private URLs");
                var raw = ((StringCollection)settings["cwh"])[0];
                Check(!raw.Contains("private-window") && raw.Split('\t').Length == 3, "recent window snapshot privacy/bounds");
                settings["tg"] = new StringCollection();
                settings["lazyRestoreMode"] = "background";
                Set(form, "pv", true);
                var history = (ToolStripMenuItem)Menu(form, "Recently Closed Windows");
                Call(form, "q820", history, EventArgs.Empty);
                history.DropDownItems[0].PerformClick();
                var recent = NewWindow(form, forms);
                var recentTabs = Member<TabControl>(recent, "k");
                WaitLea(recentTabs);
                Check(recentTabs.TabCount == 2 && recentTabs.TabPages[0].Controls[0] is V && recentTabs.TabPages[1].Controls[0] is Panel &&
                    ((V)recentTabs.TabPages[0].Controls[0]).Pv && (bool)Call(recent, "qleaPrivate", recentTabs.TabPages[1]), "recent window engine/policy/private inheritance");
                Check(site.Hits == 2, "recent-window restore loaded an unwanted default page");
                recent.Close();
                Set(form, "pv", false);
                Check(((StringCollection)settings["cwh"])[0] == raw, "private reopened window entered recent history");
                var windows = Application.OpenForms.Count;
                settings["lcw"] = Collection(Enumerable.Repeat("invalid", 256).Concat(new[] { page }).ToArray());
                Call(form, "q748", form, EventArgs.Empty);
                Check(Application.OpenForms.Count == windows, "window restore scan bound or invalid-input side effects");
                Check(Member<TabControl>(form, "k").SelectedTab.Controls[0] == original && !original.IsDisposed, "window restore changed origin tab");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "window restore loaded MSHTML");
            }
            Console.WriteLine("PASS windows: actual last/recent commands, no default requests, lazy policies, private inheritance, hidden snapshots, and bounds");
        }

        private static Form NewWindow(Form origin, List<Form> forms)
        {
            var created = Application.OpenForms.Cast<Form>().Where(candidate => candidate.GetType() == origin.GetType() && !forms.Contains(candidate)).ToArray();
            foreach (var candidate in created) { candidate.Opacity = 0; candidate.ShowInTaskbar = false; forms.Add(candidate); }
            Check(created.Length == 1, "closed-window command did not create exactly one window");
            return created[0];
        }

        private static string WatchRecord(int id, string url, bool unread = true)
        {
            Func<string, string> encode = value => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            return String.Join("|", new[] { "v1", encode("source"), encode(id.ToString()), encode(url), encode("Item " + id), "", unread ? "u" : "r", "", encode("page") });
        }

        private static void CheckWatch(Form form, ApplicationSettingsBase settings, string page)
        {
            using (var site = new LoopbackSite())
            {
                var tabs = Member<TabControl>(form, "k");
                var original = tabs.SelectedTab;
                var originalView = (V)original.Controls[0];
                originalView.Go(Path.Combine(Path.GetDirectoryName(new Uri(page).LocalPath), "..", "test-pages", "forms.html"));
                originalView.At.Fs[0].V = "watch unsent value";
                var records = Enumerable.Range(0, 18).Select(index => WatchRecord(index, site.Url + "item-" + index)).ToList();
                records.Add(WatchRecord(18, site.Url + "Case"));
                records.Add(WatchRecord(19, site.Url + "case"));
                records.Add(WatchRecord(20, site.Url + "case"));
                records.Add(WatchRecord(21, site.Url + new string('x', 4096)));
                settings["watchItems"] = Collection(records.ToArray());
                settings["lazyRestoreMode"] = "all";
                Call(form, "q8openunreadwatchitems");
                var report = Member<Label>(form, "su").Text;
                WaitLea(tabs);
                Check(tabs.TabCount == 13 && tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is V) && site.Hits == 12, "watch batch engine, bound, or eager requests");
                var accepted = new HashSet<string>(tabs.TabPages.Cast<TabPage>().Skip(1).Select(tab => ((V)tab.Controls[0]).U.AbsoluteUri), StringComparer.Ordinal);
                Check(accepted.Contains(site.Url + "Case") && accepted.Contains(site.Url + "case"), "watch URL deduplication ignored case-sensitive paths");
                var changed = ((StringCollection)settings["watchItems"]).Cast<string>().ToArray();
                var marked = 0;
                for (var index = 0; index < changed.Length; index++)
                {
                    var fields = changed[index].Split('|');
                    var url = Encoding.UTF8.GetString(Convert.FromBase64String(fields[3]));
                    Check((fields[6] == "r") == accepted.Contains(url), "watch marked a skipped URL read: " + index);
                    if (fields[6] == "r") marked++;
                    fields[6] = "u";
                    Check(String.Join("|", fields) == records[index], "watch altered unrelated record data");
                }
                Check(marked == 13 && report.Contains("13 item(s) marked read"), "watch duplicate count/report: " + marked + "; " + report);
                Check(original.Controls[0] == originalView && originalView.At.Fs[0].V == "watch unsent value", "watch overwrote current form data");
                Call(form, "q8openunreadwatchitems");
                WaitLea(tabs);
                Check(tabs.TabCount == 21 && site.Hits == 20 && ((StringCollection)settings["watchItems"])[21] == records[21], "watch second batch lost skipped items or accepted overlong URL");
                var before = tabs.TabCount;
                Call(form, "q8openunreadwatchitems");
                Check(tabs.TabCount == before, "empty eligible watch batch opened a tab");
                settings["watchItems"] = Collection(new[] { WatchRecord(100, site.Url + "old-unread") }.Concat(
                    Enumerable.Range(0, 499).Select(index => WatchRecord(200 + index, site.Url + "read-" + index, false))).ToArray());
                Call(form, "q8openunreadwatchitems");
                WaitLea(tabs);
                Check(tabs.TabCount == before + 1 && site.Hits == 21, "watch starved an older unread item behind read records");
                before = tabs.TabCount;
                settings["watchItems"] = Collection(new[] { WatchRecord(100, site.Url + "beyond-scan") }.Concat(Enumerable.Repeat("invalid", 500)).ToArray());
                Call(form, "q8openunreadwatchitems");
                Check(tabs.TabCount == before && ((StringCollection)settings["watchItems"])[0].Split('|')[6] == "u", "watch exceeded scan bound");
                Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "watch batch loaded legacy engine");
            }
            Console.WriteLine("PASS watch: exact eager requests, bounded batches, case-sensitive deduplication, skipped/read state, retained fields, and scan limits");
        }

        private static void CheckLazyStartup(Form form, LoopbackSite site, string[] urls, bool all)
        {
            var tabs = Member<TabControl>(form, "k");
            var pages = tabs.TabPages.Cast<TabPage>().ToArray();
            var initialRequests = all ? 0 : 1;
            Observe(1500);
            Check(site.Hits == initialRequests, "deferred startup issued unexpected requests: " + site.Hits);
            Check(Member<HashSet<TabPage>>(form, "feature6LazyTabs").Count == (all ? 3 : 2), "deferred startup bookkeeping");
            Call(form, "qleaTick", form, EventArgs.Empty);
            Call(form, "tk", form, EventArgs.Empty);
            Call(form, "qleaSaveSession");
            Check(File.ReadAllLines(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")).SequenceEqual(urls), "deferred startup session URL ordering");
            Member<ToolStripItem>(form, "re").PerformClick();
            Wait(() => site.Hits == initialRequests + 1 && pages[0].Controls[0] is V && !((V)pages[0].Controls[0]).Busy, "deferred first-page reload");
            Check(pages[1].Controls[0] is Panel && pages[2].Controls[0] is Panel, "Reload woke other deferred tabs");
            tabs.SelectedTab = pages[2];
            WaitLea(tabs);
            Check(site.Hits == initialRequests + 2 && pages[1].Controls[0] is Panel, "selecting one deferred tab loaded others");
            var third = pages[2].Controls[0];
            tabs.SelectedTab = pages[0];
            tabs.SelectedTab = pages[2];
            Observe(250);
            Check(pages[2].Controls[0] == third && site.Hits == initialRequests + 2, "reselecting resumed tab reloaded it");
            Menu(form, "Resume All Suspended Tabs").PerformClick();
            WaitLea(tabs);
            Check(site.Hits == initialRequests + 3 && pages.All(tab => tab.Controls[0] is V), "resume-all did not load exactly the remaining page");
            Check(Member<HashSet<TabPage>>(form, "feature6LazyTabs").Count == 0, "resume-all left stale deferred state");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "lazy startup loaded legacy engine");
            Console.WriteLine("PASS lazy startup: mode=" + (all ? "all" : "background") + "; initial requests=" + initialRequests + "; exact on-demand requests, sessions, and resume-all");
        }

        private static void Observe(int milliseconds)
        {
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(10); }
        }

        private static void CheckLazyCommands(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var original = tabs.SelectedTab;
            var originalView = (V)original.Controls[0];
            originalView.Go(Path.Combine(Path.GetDirectoryName(new Uri(page).LocalPath), "..", "test-pages", "forms.html"));
            originalView.At.Fs[0].V = "unsent value";
            settings["lazyRestoreMode"] = "all";
            var opened = (int)Call(form, "qlazyopenurls", new List<string> { "javascript:alert(1)", page + "#one", page + "#two" });
            Check(opened == 2 && tabs.TabCount == 3 && original.Controls[0] == originalView && !originalView.IsDisposed &&
                originalView.At.Fs[0].V == "unsent value", "lazy restore replaced existing content or unsent data");
            Check(tabs.SelectedIndex == 1 && tabs.TabPages[1].Controls[0] is Panel && tabs.TabPages[2].Controls[0] is Panel,
                "lazy restore did not select first Lea placeholder");
            Observe(250);
            Check(tabs.TabPages[1].Controls[0] is Panel && Member<Label>(form, "su").Text.Contains("1 URL(s) skipped"), "all-mode woke selected placeholder or lost bounded report");
            var count = tabs.TabCount;
            settings["ns"] = Collection("Lazy|" + page + "#named-first", "Lazy|" + page + "#named-second");
            settings["lazyRestoreMode"] = "background";
            Call(form, "qopensession", "Lazy");
            WaitLea(tabs);
            Check(tabs.TabCount == count + 2 && tabs.SelectedIndex == count && tabs.TabPages[count].Controls[0] is V &&
                tabs.TabPages[count + 1].Controls[0] is Panel, "named-session lazy policy or append semantics");
            count = tabs.TabCount;
            settings["lazyRestoreMode"] = "all";
            File.WriteAllLines(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION"), new[] { page + "#previous" });
            Member<ToolStripItem>(form, "RP").PerformClick();
            Check(tabs.TabCount == count + 1 && tabs.SelectedTab.Controls[0] is Panel, "previous-session lazy policy");
            settings["lazyRestoreMode"] = "invalid-policy";
            Call(form, "qlazyopenurls", new List<string> { page + "#eager" });
            WaitLea(tabs);
            Check(tabs.SelectedTab.Controls[0] is V, "invalid lazy policy did not fall back to off");
            settings["lazyRestoreMode"] = "all";
            count = tabs.TabCount;
            opened = (int)Call(form, "qlazyopenurls", Enumerable.Range(0, 20).Select(index => page + "#limit" + index).ToList());
            Check(opened == 12 && tabs.TabCount == count + 12 && tabs.TabPages.Cast<TabPage>().Skip(count).All(tab => tab.Controls[0] is Panel), "lazy restore exceeded tab bound or created renderers");
            count = tabs.TabCount;
            opened = (int)Call(form, "qlazyopenurls", Enumerable.Repeat("invalid", 256).Concat(new[] { page }).ToList());
            Check(opened == 0 && tabs.TabCount == count, "lazy restore exceeded scan bound");
            Check((int)Call(form, "qlazyopenurls", (object)null) == 0, "null lazy input");
            Check((int)Call(form, "qlazyopenurls", new List<string> { " ", "data:text/html,x", "http://localhost/" + new string('x', 4096) }) == 0 && tabs.TabCount == count, "invalid lazy inputs altered tabs");
            Call(form, "qleaPaste", page + "#clipboard-eager", true);
            WaitLea(tabs);
            Check(tabs.SelectedTab.Controls[0] is V, "session policy incorrectly deferred clipboard opens");
            tabs.SelectedTab = original;
            Menu(form, "Close Other Tabs").PerformClick();
            Check(original.Controls[0] == originalView && originalView.At.Fs[0].V == "unsent value", "restore commands changed original document");

            Set(form, "pv", true);
            Call(form, "qlazyopenurls", new List<string> { page + "#private-lazy-one", page + "#private-lazy-two" });
            var privateTab = tabs.SelectedTab;
            Set(form, "pv", false);
            Call(form, "qleaSaveSession");
            Check(!File.ReadAllText(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")).Contains("private-lazy"), "deferred private URL entered automatic session");
            Member<ToolStripItem>(form, "re").PerformClick();
            WaitLea(tabs);
            Check(privateTab.Controls[0] is V && ((V)privateTab.Controls[0]).Pv, "private lazy resume lost context");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "lazy restore loaded legacy engine");
            Console.WriteLine("PASS lazy commands: preserve documents, named/previous policy, bounds, eager clipboard, and private deferred tabs");

            settings["tg"] = Collection("Old|" + Color.Teal.ToArgb() + "|active|" + Call(form, "qb64", originalView.U.AbsoluteUri));
            Call(form, "qcollapsegroup", "Old");
            Call(form, "q3", page);
            var legacy = (WebBrowser)tabs.SelectedTab.Controls[0];
            Wait(() => legacy.Url != null && legacy.ReadyState == WebBrowserReadyState.Complete, "identity legacy fixture");
            var oldTabs = (List<TabPage>)Call(form, "qalltabs");
            var oldMenu = originalView.ContextMenuStrip;
            settings["idp"] = Collection("Lazy|I|" + Call(form, "qb64", "identity"),
                "Lazy|@lazyRestoreMode|" + Call(form, "qb64", "all"), "Lazy|@ps|" + Call(form, "qb64", "e"),
                "Lazy|tabs|" + Call(form, "qb64", page + "#identity-one"), "Lazy|tabs|" + Call(form, "qb64", page + "#identity-two"));
            Call(form, "qloadidentity", "Lazy");
            Check(tabs.TabCount == 2 && tabs.TabPages.Cast<TabPage>().All(tab => tab.Controls[0] is Panel) &&
                oldTabs.All(tab => tab.IsDisposed) && originalView.IsDisposed && oldMenu.IsDisposed && legacy.IsDisposed, "identity restore retained old views or wrong tab state");
            Check((string)settings["activeIdentity"] == "Lazy" && Member<Dictionary<string, List<TabPage>>>(form, "hiddenGroupTabs").Count == 0 &&
                Member<Dictionary<TabPage, bool>>(form, "qleaPaused").Count == 2 && Member<HashSet<TabPage>>(form, "feature6LazyTabs").Count == 2,
                "identity restore left stale ownership state");
            Check(((StringCollection)settings["rc"]).Count == 0, "identity replacement wrote old URLs into new recent history");
            Member<ToolStripItem>(form, "re").PerformClick();
            WaitLea(tabs);
            Check(((V)tabs.SelectedTab.Controls[0]).Pv, "private identity did not initialize private Lea");
            var prior = tabs.TabPages.Cast<TabPage>().ToArray();
            settings["idp"] = Collection("Empty|I|" + Call(form, "qb64", "identity"), "Empty|@lazyRestoreMode|" + Call(form, "qb64", "all"));
            Call(form, "qloadidentity", "Empty");
            Check(tabs.TabCount == 1 && tabs.SelectedTab.Controls[0] is Panel && prior.All(tab => tab.IsDisposed) &&
                (string)Call(form, "qleaUrl", tabs.SelectedTab) == page, "empty identity fallback retained old context");
            Console.WriteLine("PASS lazy identities: mixed/hidden/private disposal, fresh contexts, bookkeeping, and empty fallback");
        }

        private static void CheckSuspension(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var tab = tabs.SelectedTab;
            var view = (V)tab.Controls[0];
            var menu = view.ContextMenuStrip;
            var paused = Member<Dictionary<TabPage, bool>>(form, "qleaPaused");
            var urls = Member<Dictionary<TabPage, string>>(form, "suspendedGroupUrls");
            Menu(form, "Suspend Or Resume Current Tab").PerformClick();
            Check(view.IsDisposed && menu.IsDisposed && tab.Controls.Count == 1 && tab.Controls[0] is Panel,
                "suspension did not dispose renderer/menu and replace with lightweight placeholder");
            Check(urls[tab] == page && paused.ContainsKey(tab), "suspension lost target");
            Call(form, "tk", form, EventArgs.Empty);
            Call(form, "qleaTick", form, EventArgs.Empty);
            Check(!Member<ToolStripItem>(form, "b").Enabled && !Member<ToolStripItem>(form, "f").Enabled, "paused navigation buttons remained enabled");
            Check(!(bool)Call(form, "qsuspendtab", tab), "duplicate suspension accepted");
            var placeholder = tab.Controls[0];
            ((Button)placeholder.Controls[0]).PerformClick();
            WaitLea(tabs);
            Check(tabs.SelectedTab == tab && tab.Controls[0] is V && tab.Controls[0] != view && placeholder.IsDisposed &&
                ((V)tab.Controls[0]).U.AbsoluteUri == page && !paused.ContainsKey(tab), "Resume did not preserve tab and recreate Lea");

            Menu(form, "Suspend Or Resume Current Tab").PerformClick();
            urls[tab] = "javascript:alert(1)";
            Check(!(bool)Call(form, "qfeature6resumetab", tab) && tab.Controls[0] is Panel, "invalid resume target destroyed placeholder");
            urls[tab] = page;
            Check(Member<ToolStripItem>(form, "re").Enabled && Member<ToolStripItem>(form, "re").Available, "Reload menu disabled or hidden");
            Member<ToolStripItem>(form, "re").PerformClick();
            Check(tab.Controls[0] is V, "Reload menu did not resume placeholder");
            WaitLea(tabs);
            Menu(form, "Suspend Or Resume Current Tab").PerformClick();
            Check(tab.Controls[0] is Panel, "resumed tab did not suspend again before replacement navigation");
            Call(form, "q1", page + "#replacement");
            WaitLea(tabs);
            Check(((V)tab.Controls[0]).U.AbsoluteUri == page + "#replacement" && !((V)tab.Controls[0]).CanBk,
                "explicit replacement URL=" + ((V)tab.Controls[0]).U.AbsoluteUri + "; CanBk=" + ((V)tab.Controls[0]).CanBk);
            Menu(form, "Suspend Or Resume Current Tab").PerformClick();
            settings["h"] = page + "#home";
            Member<ToolStripItem>(form, "HO").PerformClick();
            WaitLea(tabs);
            Check(((V)tab.Controls[0]).U.AbsoluteUri == page + "#home", "paused Home target");

            Set(form, "pv", true);
            var secret = (V)Call(form, "qleaNew", page + "#private-paused");
            var secretTab = tabs.SelectedTab;
            WaitLea(tabs);
            Set(form, "pv", false);
            Check((bool)Call(form, "qsuspendtab", secretTab) && paused[secretTab], "private suspension flag");
            Call(form, "qleaSaveSession");
            var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
            Check(!File.ReadAllText(session).Contains("private-paused"), "paused private URL leaked to automatic session");
            Menu(form, "Duplicate Tab").PerformClick();
            WaitLea(tabs);
            Check(((V)tabs.SelectedTab.Controls[0]).Pv && tabs.SelectedTab.Controls[0] is V, "private paused duplicate changed engine/privacy");
            tabs.SelectedTab = secretTab;
            WaitLea(tabs);
            Call(form, "qleaTick", form, EventArgs.Empty);
            Check(secret.IsDisposed && ((V)secretTab.Controls[0]).Pv, "resume/timer downgraded private view");
            settings["rc"] = new StringCollection();
            Call(form, "qsuspendtab", secretTab);
            Call(form, "qleaTabMouseDown", tabs, new MouseEventArgs(MouseButtons.Middle, 1, 1, 1, 0));
            Check(secretTab.IsDisposed && !paused.ContainsKey(secretTab) && !urls.ContainsKey(secretTab) &&
                ((StringCollection)settings["rc"]).Count == 0, "paused middle-close cleanup or privacy");

            using (var site = new LoopbackSite(1000))
            {
                var pending = (V)Call(form, "qleaNew", site.Url + "pending");
                var pendingTab = tabs.SelectedTab;
                Wait(() => site.Hits == 1 && pending.Busy, "pending initial navigation");
                Check((bool)Call(form, "qsuspendtab", pendingTab) && urls[pendingTab] == site.Url + "pending", "pending navigation target lost on suspension");
                Wait(() => !pending.Busy, "pending navigation canceled");
                Check(pending.IsDisposed && pendingTab.Controls[0] is Panel, "canceled request recreated view");
                Call(form, "qfeature6resumetab", pendingTab);
                WaitLea(tabs);
                Check(((V)pendingTab.Controls[0]).U.AbsoluteUri == site.Url + "pending", "pending URL did not resume");
            }
            TabPage released;
            var weak = SuspendCollectible(form, page, out released);
            Check(Member<Dictionary<V, string>>(form, "qleaPending").Count == 0, "host retained completed navigation");
            Wait(() => { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); return Collected(weak); }, "suspended renderer retained in host state");
            tabs.SelectedTab = tab;
            Check(released.Controls[0] is Panel, "background tab resumed without selection");
            tabs.SelectedTab = released;
            WaitLea(tabs);
            Check(released.Controls[0] is V, "selection resumed background tab in old engine");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "suspension loaded legacy engine");
            Console.WriteLine("PASS suspension: disposal/collection, cancellation, resume/menu/selection, navigation, privacy, duplicate, and close");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Collected(WeakReference weak) { return !weak.IsAlive; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference SuspendCollectible(Form form, string page, out TabPage tab, bool hibernate = false)
        {
            var previous = Member<TabControl>(form, "k").SelectedTab;
            var view = (V)Call(form, "qleaNew", page + "#collectible");
            tab = Member<TabControl>(form, "k").SelectedTab;
            Wait(() => !view.Busy, "collectible view load");
            var weak = new WeakReference(view);
            if (hibernate)
            {
                Member<TabControl>(form, "k").SelectedTab = previous;
                Check((int)Call(form, "qfeature6hibernateeligible", false) == 1 && view.IsDisposed, "collectible native hibernation");
            }
            else Check((bool)Call(form, "qsuspendtab", tab), "collectible view suspension");
            view = null;
            return weak;
        }

        private static void CheckGroups(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            var first = tabs.SelectedTab;
            Call(form, "qleaNew", page + "#group-two");
            var second = tabs.SelectedTab;
            WaitLea(tabs);
            settings["tg"] = Collection("Test|" + Color.Teal.ToArgb() + "|suspended|" + Call(form, "qb64", page),
                "Test|" + Color.Teal.ToArgb() + "|suspended|" + Call(form, "qb64", page + "#group-two"));
            // Exercise the saved-group startup timer, including a second tick.
            Call(form, "qgrouploadtick", form, EventArgs.Empty);
            Call(form, "qgrouploadtick", form, EventArgs.Empty);
            Check(first.Controls[0] is Panel && second.Controls[0] is Panel && first.ToolTipText == "Tab group: Test", "startup saved group did not suspend Lea");
            Call(form, "tk", form, EventArgs.Empty);
            Call(form, "qleaTick", form, EventArgs.Empty);
            Call(form, "qcollapsegroup", "Test");
            var hidden = Member<Dictionary<string, List<TabPage>>>(form, "hiddenGroupTabs");
            Check(tabs.TabCount == 1 && tabs.SelectedTab.Controls[0] is V && hidden["Test"].Count == 2 &&
                first.Controls[0] is Panel && second.Controls[0] is Panel, "collapse-all lost placeholders or Lea fallback");
            WaitLea(tabs);
            Call(form, "qleaSaveSession");
            Check(File.ReadAllText(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")).Contains("#group-two"), "hidden suspended URL omitted from session");
            Call(form, "qfeature6prunestate");
            Check(Member<Dictionary<TabPage, bool>>(form, "qleaPaused").ContainsKey(first), "pruning lost hidden tab ownership");
            Call(form, "qresumegroup", "Test");
            Wait(() => first.Controls[0] is V && !((V)first.Controls[0]).Busy && second.Controls[0] is V && !((V)second.Controls[0]).Busy, "hidden group resume");
            Check(tabs.TabCount == 1, "hidden resume expanded group unexpectedly");
            Call(form, "qexpandgroup", "Test");
            Call(form, "qexpandgroup", "Test");
            Check(tabs.TabCount == 3 && tabs.TabPages.Contains(first) && tabs.TabPages.Contains(second) && !hidden.ContainsKey("Test"), "expand duplicated/replaced tabs");
            Check(first.ToolTipText == "Tab group: Test" && first.BackColor.ToArgb() == Color.Teal.ToArgb(), "group styling not retained");

            Overview(form, "qoverviewsuspend", first, second);
            Check(first.Controls[0] is Panel && second.Controls[0] is Panel, "overview suspend omitted Lea");
            Overview(form, "qoverviewreload", first, second);
            WaitLea(tabs);
            Check(first.Controls[0] is V && second.Controls[0] is V, "overview reload replaced Lea engine");
            Set(form, "pv", true);
            Call(form, "qleaNew", page + "#private-overview");
            var secret = tabs.SelectedTab;
            WaitLea(tabs);
            Set(form, "pv", false);
            Call(form, "qsuspendtab", secret);
            settings["rc"] = new StringCollection();
            Overview(form, "qoverviewclose", secret);
            Check(secret.IsDisposed && ((StringCollection)settings["rc"]).Count == 0, "overview close leaked private URL");
            Call(form, "qcollapsegroup", "Test");
            Overview(form, "qoverviewclose", first, second);
            Check(first.IsDisposed && second.IsDisposed && tabs.TabCount == 1 && !form.IsDisposed, "overview hidden close failed ownership cleanup");
            Check(!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(module => module.ModuleName.Equals("mshtml.dll", StringComparison.OrdinalIgnoreCase)), "group routes loaded legacy engine");
            Console.WriteLine("PASS groups: startup restore timer, collapse/expand, hidden suspension/session/resume, overview, privacy, and cleanup");
        }

        private static void Overview(Form form, string command, params TabPage[] selected)
        {
            var records = (IList)Call(form, "qoverviewitems");
            var subset = (IList)Activator.CreateInstance(records.GetType());
            try
            {
                foreach (var record in records)
                    if (selected.Contains(Member<TabPage>(record, "Tab"))) subset.Add(record);
                Check(subset.Count == selected.Length, "overview omitted owned tabs");
                Call(form, command, subset);
            }
            finally
            {
                foreach (var record in records)
                {
                    var bitmap = Member<Bitmap>(record, "Thumbnail");
                    if (bitmap != null) bitmap.Dispose();
                }
            }
        }

        private sealed class LoopbackSite : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly Thread worker;
            private volatile bool stopped;
            private Exception error;
            private readonly int firstDelay;
            private int hits;
            public volatile int DelayMs;
            public volatile int Status = 200;
            public volatile bool SetCookie;
            public readonly ManualResetEvent Release = new ManualResetEvent(true);
            public readonly string Url;
            public int Hits { get { return Interlocked.CompareExchange(ref hits, 0, 0); } }

            public LoopbackSite(int delay = 0)
            {
                firstDelay = delay;
                listener.Start();
                Url = "http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/";
                worker = new Thread(Serve) { IsBackground = true };
                worker.Start();
            }

            private void Serve()
            {
                try
                {
                    for (var request = 0; request < 128 && !stopped; request++)
                        using (var client = listener.AcceptTcpClient())
                        using (var stream = client.GetStream())
                        {
                            stream.ReadTimeout = 3000;
                            var end = 0;
                            for (var bytes = 0; bytes < 16384 && end != 0x0d0a0d0a; bytes++)
                            {
                                var next = stream.ReadByte();
                                if (next < 0) throw new IOException("Incomplete loopback request");
                                end = (end << 8) | next;
                            }
                            Check(end == 0x0d0a0d0a, "loopback request header bound");
                            Interlocked.Increment(ref hits);
                            Check(Release.WaitOne(10000), "loopback response gate timeout");
                            if (request == 0 && firstDelay > 0) Thread.Sleep(firstDelay);
                            if (request > 0 && DelayMs > 0) Thread.Sleep(DelayMs);
                            var body = Encoding.UTF8.GetBytes("<title>Lea navigation</title><p>Local test</p>");
                            var header = Encoding.ASCII.GetBytes("HTTP/1.1 " + Status + " Test\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\n" +
                                (SetCookie ? "Set-Cookie: hibernate=retained; Path=/\r\n" : "") + "Connection: close\r\n\r\n");
                            try
                            {
                                stream.Write(header, 0, header.Length);
                                stream.Write(body, 0, body.Length);
                            }
                            catch (IOException) { if (request != 0 || firstDelay == 0) throw; }
                        }
                }
                catch (Exception failure) { if (!stopped) error = failure; }
            }

            public void Dispose()
            {
                stopped = true;
                Release.Set();
                listener.Stop();
                Check(worker.Join(4000), "loopback server shutdown");
                Release.Dispose();
                Check(error == null, "loopback server: " + error);
            }
        }

        private static void CheckCommands(Form form, ApplicationSettingsBase settings, string page)
        {
            var tabs = Member<TabControl>(form, "k");
            settings["rc"] = new StringCollection();
            settings["ns"] = new StringCollection();
            Menu(form, "Duplicate Tab").PerformClick();
            Check(tabs.TabCount == 2 && tabs.SelectedTab.Controls[0] is V, "Duplicate Tab did not create a Lea tab");
            Wait(() => tabs.TabPages.Cast<TabPage>().All(tab => !((V)tab.Controls[0]).Busy), "duplicate load");
            Check(((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page, "duplicate URL");
            Console.WriteLine("PASS commands: duplicate Lea tab");

            var first = tabs.TabPages[0];
            var closed = tabs.SelectedTab;
            Check(Member<ToolStripItem>(form, "CT").Available, "Close Tab remains hidden");
            var closeKey = new KeyEventArgs(Keys.Control | Keys.W);
            Call(form, "q8", form, closeKey);
            Check(closeKey.SuppressKeyPress, "close shortcut was not consumed");
            var recent = (StringCollection)settings["rc"];
            Check(closed.IsDisposed && tabs.TabCount == 1 && recent.Count == 1 && recent[0] == page,
                "close did not dispose/record Lea tab: disposed=" + closed.IsDisposed + "; tabs=" + tabs.TabCount + "; recent=" + String.Join(",", recent.Cast<string>().ToArray()));
            recent.Insert(0, "javascript:alert(1)");
            var key = new KeyEventArgs(Keys.Control | Keys.Shift | Keys.T);
            Call(form, "q8", form, key);
            Check(key.SuppressKeyPress && tabs.TabCount == 2 && tabs.SelectedTab.Controls[0] is V, "keyboard reopen did not use Lea");
            WaitLea(tabs);
            Check(recent.Count == 0 && ((V)tabs.SelectedTab.Controls[0]).U.AbsoluteUri == page, "reopen did not consume valid history and skip invalid URL");
            Menu(form, "Reopen Closed Tab").PerformClick();
            Check(tabs.TabCount == 2, "empty reopen repeated stale last URL");
            Member<ToolStripItem>(form, "CT").PerformClick();
            Call(form, "q161", form, EventArgs.Empty);
            Member<ToolStripDropDownItem>(form, "RC").DropDownItems[0].PerformClick();
            Check(tabs.TabCount == 2 && tabs.SelectedTab.Controls[0] is V, "recently closed menu used legacy engine");
            WaitLea(tabs);
            Console.WriteLine("PASS commands: close, keyboard reopen, invalid history, and recent-tab menu");

            recent.Clear();
            Set(form, "pv", true);
            var secret = (V)Call(form, "qleaNew", page + "#private");
            var secretTab = (TabPage)secret.Parent;
            WaitLea(tabs);
            Set(form, "pv", false);
            tabs.SelectedTab = first;
            Call(form, "qleaSaveSession");
            Check(!File.ReadAllText(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")).Contains("#private"), "background private tab leaked into automatic session");
            Call(form, "qleaClose", secretTab);
            Check(secret.IsDisposed && recent.Count == 0, "background private close leaked recent history");
            Set(form, "pv", true);
            Call(form, "rca", page + "#private-history");
            Check(recent.Count == 0, "private legacy history leaked");
            Set(form, "pv", false);
            Console.WriteLine("PASS commands: private close and automatic-session isolation");

            Call(form, "q1", page.Replace("%20", " ") + "#bookmark");
            Wait(() => ((V)first.Controls[0]).U.AbsoluteUri == page + "#bookmark" && !((V)first.Controls[0]).Busy, "engine-aware URL navigation");
            settings["bm"] = new StringCollection();
            Member<ToolStripItem>(form, "BP").PerformClick();
            Check(((StringCollection)settings["bm"]).Contains(page + "#bookmark"), "bookmark current omitted Lea URL");
            settings["bm"] = new StringCollection();
            Menu(form, "Bookmark All Tabs").PerformClick();
            Check(((StringCollection)settings["bm"]).Count == 2, "bookmark all omitted Lea tabs");
            var reloads = 0;
            foreach (TabPage tab in tabs.TabPages) ((V)tab.Controls[0]).Done += (sender, change) => reloads++;
            Menu(form, "Reload All Tabs").PerformClick();
            Wait(() => reloads == 2 && tabs.TabPages.Cast<TabPage>().All(tab => !((V)tab.Controls[0]).Busy), "reload all Lea tabs");
            Console.WriteLine("PASS commands: current/all bookmarks, URL navigation, and reload all");

            var tabWidth = tabs.ItemSize.Width;
            Call(form, "q3", page + "#legacy");
            var legacy = (WebBrowser)tabs.SelectedTab.Controls[0];
            Wait(() => legacy.Url != null && legacy.ReadyState == WebBrowserReadyState.Complete, "explicit legacy load");
            Menu(form, "Duplicate Tab").PerformClick();
            Check(tabs.SelectedTab.Controls[0] is WebBrowser, "legacy duplication changed engines");
            var duplicateLegacy = (WebBrowser)tabs.SelectedTab.Controls[0];
            Wait(() => duplicateLegacy.Url != null && duplicateLegacy.Url.AbsoluteUri == page + "#legacy", "legacy duplicate changed the URL into a search");
            var removed = tabs.TabPages.Cast<TabPage>().Skip(1).ToArray();
            tabs.SelectedTab = first;
            Menu(form, "Close Tabs To The Right").PerformClick();
            Check(tabs.TabCount == 1 && tabs.SelectedTab == first && removed.All(tab => tab.IsDisposed), "mixed right-close did not dispose all targets");
            Check((int)form.GetType().GetField("I", Members).GetValue(form) == 1, "right-close left stale tab count");
            Check(tabs.ItemSize.Width == tabWidth, "legacy close did not restore tab width");
            Call(form, "q3", page + "#kept");
            var keptLegacy = tabs.SelectedTab;
            var extra = (V)Call(form, "qleaNew", page + "#extra");
            tabs.SelectedTab = keptLegacy;
            Menu(form, "Close Other Tabs").PerformClick();
            Check(tabs.TabCount == 1 && tabs.SelectedTab == keptLegacy && first.IsDisposed && extra.IsDisposed, "mixed close-other did not preserve selected legacy tab");
            Console.WriteLine("PASS commands: engine-preserving duplicate and mixed bulk-close disposal");

            Call(form, "qleaNew", page + "#session");
            WaitLea(tabs);
            var keptBrowser = (WebBrowser)keptLegacy.Controls[0];
            Wait(() => keptBrowser.Url != null && keptBrowser.ReadyState == WebBrowserReadyState.Complete, "named-session legacy source");
            Call(form, "qsavesession", "  Pair|one  ");
            Call(form, "qsavesession", "  Pair|one  ");
            var named = (StringCollection)settings["ns"];
            Check(named.Count == 2 && named.Cast<string>().All(value => value.StartsWith("Pair-one|")), "named save/replace omitted Lea or failed normalized replacement");
            Call(form, "qopensession", "pair-one");
            Check(tabs.TabCount == 4 && tabs.SelectedIndex == 2 && tabs.TabPages.Cast<TabPage>().Skip(2).All(tab => tab.Controls[0] is V), "named session did not append/select Lea tabs");
            WaitLea(tabs);
            Check(((V)tabs.TabPages[2].Controls[0]).U.AbsoluteUri == page + "#kept" && ((V)tabs.TabPages[3].Controls[0]).U.AbsoluteUri == page + "#session", "named session URL order");
            Check(keptLegacy.Controls[0] == keptBrowser && !keptBrowser.IsDisposed, "named open replaced an existing tab");
            named.Clear();
            named.Add("Limit|javascript:alert(1)");
            named.Add("Limit|https://example.invalid/" + new string('x', 4096));
            foreach (var index in Enumerable.Range(0, 20)) named.Add("Limit|" + page + "#" + index);
            Call(form, "qopensession", "Limit");
            Check(tabs.TabCount == 16 && named.Count == 22 && Member<Label>(form, "su").Text.Contains("10 URL(s) skipped"), "named open limits/reporting changed saved records");
            WaitLea(tabs);
            Set(form, "pv", true);
            named.Clear();
            named.Add("Private|" + page + "#private-open");
            Call(form, "qopensession", "Private");
            Check(tabs.TabCount == 17 && ((V)tabs.SelectedTab.Controls[0]).Pv, "explicit private session open was blocked or non-private");
            WaitLea(tabs);
            Set(form, "pv", false);
            Console.WriteLine("PASS commands: mixed named save/replace, Lea restore, limits, and explicit private open");

            var session = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
            File.WriteAllText(session, page);
            settings["ls"] = Collection(page);
            Call(form, "qleaClearSession");
            Check(!File.Exists(session) && ((StringCollection)settings["ls"]).Count == 0, "clear saved session left Lea data");
            using (var locked = new FileStream(session, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                settings["ls"] = Collection(page);
                var rejected = false;
                try { Call(form, "qleaClearSession"); }
                catch (TargetInvocationException error) { rejected = error.InnerException is IOException; }
                Check(rejected && ((StringCollection)settings["ls"]).Count == 1, "locked clear partially erased saved settings");
            }
            foreach (var tab in tabs.TabPages.Cast<TabPage>().ToArray()) { tabs.TabPages.Remove(tab); tab.Dispose(); }
            Call(form, "q4", form, EventArgs.Empty);
            Call(form, "q16", form, EventArgs.Empty);
            Call(form, "q33", form, EventArgs.Empty);
            Call(form, "q22", form, EventArgs.Empty);
            Call(form, "pc", form, EventArgs.Empty);
            Check(tabs.TabCount == 0, "empty-tab commands created unintended controls");
            Console.WriteLine("PASS commands: saved-session clearing, locked-file preservation, and empty tabs");
            Set(form, "I", 0);
            Call(form, "q3", page);
            Member<ToolStripItem>(form, "CT").PerformClick();
            Check(form.IsDisposed && (string)settings["cleanExit"] == "e", "last legacy tab did not close through normal cleanup");
            Console.WriteLine("PASS commands: last legacy tab closes normally without terminating the harness");
        }

        private static object Call(Form form, string name, params object[] args)
        {
            return form.GetType().GetMethod(name, Members).Invoke(form, args);
        }

        private static void Set(Form form, string name, object value)
        {
            form.GetType().GetField(name, Members).SetValue(form, value);
        }

        private static void WaitLea(TabControl tabs)
        {
            Wait(() => tabs.TabPages.Cast<TabPage>().Select(tab => tab.Controls[0]).OfType<V>().All(view => view.At != null && !view.Busy), "Lea command page loads");
        }

        private static ToolStripItem Menu(Form form, string text)
        {
            var found = FindMenu(Member<ToolStripDropDownItem>(form, "m"), text);
            Check(found != null, "menu not found: " + text);
            return found;
        }

        private static ToolStripItem FindMenu(ToolStripDropDownItem root, string text)
        {
            foreach (ToolStripItem item in root.DropDownItems)
            {
                if (item.Text.Replace("&", "") == text) return item;
                var child = item as ToolStripDropDownItem;
                if (child == null) continue;
                var found = FindMenu(child, text);
                if (found != null) return found;
            }
            return null;
        }

        private static string MenuSignature(ToolStripDropDownItem root)
        {
            var count = 0;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            using (var hash = SHA256.Create())
            {
                WriteMenu(writer, root, ref count);
                writer.Flush();
                return "MenuItems=" + count + "; MenuSHA256=" + BitConverter.ToString(hash.ComputeHash(stream.ToArray())).Replace("-", "");
            }
        }

        private static void CheckMenuLayouts(ToolStripDropDownItem root)
        {
            if (root.DropDownItems.Count == 0) return;
            var layouts = 0;
            LayoutEventHandler handler = (sender, change) => layouts++;
            root.DropDown.Layout += handler;
            try { root.DropDown.PerformLayout(); }
            finally { root.DropDown.Layout -= handler; }
            Check(layouts > 0, "menu layout remained suspended: " + root.Text);
            foreach (ToolStripItem item in root.DropDownItems)
            {
                if (item.Available) Check(item.Bounds.Width > 0 && item.Bounds.Height > 0, "empty menu item bounds: " + item.Text);
                var child = item as ToolStripDropDownItem;
                if (child != null) CheckMenuLayouts(child);
            }
        }

        private static void WriteMenu(BinaryWriter writer, ToolStripItem item, ref int count)
        {
            Check(++count <= 4096, "menu tree exceeds profile bound");
            writer.Write(item.GetType().FullName);
            writer.Write(item.Name ?? "");
            writer.Write(item.Text ?? "");
            writer.Write(item.Enabled);
            writer.Write(item.Image != null);
            var menu = item as ToolStripMenuItem;
            writer.Write(menu == null ? -1 : (int)menu.CheckState);
            writer.Write(menu == null ? -1 : (int)menu.ShortcutKeys);
            var drop = item as ToolStripDropDownItem;
            writer.Write(drop == null ? -1 : drop.DropDownItems.Count);
            if (drop != null)
                foreach (ToolStripItem child in drop.DropDownItems) WriteMenu(writer, child, ref count);
        }

        private static T Member<T>(object target, string name) where T : class
        {
            var property = target.GetType().GetProperty(name, Members);
            return (T)(property == null ? target.GetType().GetField(name, Members).GetValue(target) : property.GetValue(target, null));
        }

        private static void Wait(Func<bool> condition, string name)
        {
            var until = DateTime.UtcNow.AddSeconds(10);
            while (!condition() && DateTime.UtcNow < until) { Application.DoEvents(); Thread.Sleep(10); }
            Check(condition(), name);
        }

        private static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException(name);
        }
    }
}
