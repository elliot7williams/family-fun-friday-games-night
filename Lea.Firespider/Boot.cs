using System;
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
            var release = new ManualResetEvent(false);
            var requested = new ManualResetEvent(false);
            var responded = new ManualResetEvent(false);
            var forms = new List<Form>();
            try
            {
                Check(new[] { "clean", "lea", "legacy", "recovery", "private", "second", "limit", "invalid", "slow", "locked", "menus", "commands", "navigation", "links" }.Contains(mode), "unknown boot case: " + mode);
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
                foreach (var name in new[] { "dashboard", "rs", "ps", "recoveryEnabled", "t", "rws", "backupEnabled", "frEnabled", "portableMode", "hibernateEnabled" })
                    settings[name] = "";
                foreach (var name in new[] { "s", "sc", "ls", "recovery", "bm" }) settings[name] = new StringCollection();
                settings["nf"] = page;
                settings["h"] = page;
                settings["se"] = "http://127.0.0.1:1/?q=";
                settings["cleanExit"] = "e";
                settings["el"] = "e";
                settings["ep"] = log;
                settings["lazyRestoreMode"] = "off";
                var expected = new[] { page };
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
                foreach (TabPage tab in tabs.TabPages)
                    Check(tab.Controls.Count == 1 && tab.Controls[0] is V, "startup created a non-Lea tab");
                if (mode == "slow")
                {
                    Wait(() => requested.WaitOne(0) && ((V)tabs.TabPages[0].Controls[0]).Busy, "asynchronous startup request");
                    Check(!responded.WaitOne(0), "startup waited for the network response");
                    Check(form.IsHandleCreated && form.Visible, "window unavailable during startup request");
                    release.Set();
                }
                Wait(() => tabs.TabPages.Cast<TabPage>().All(tab => ((V)tab.Controls[0]).At != null && !((V)tab.Controls[0]).Busy), "startup page loads");
                for (var index = 0; index < expected.Length; index++)
                    Check(((V)tabs.TabPages[index].Controls[0]).U.AbsoluteUri == expected[index], "startup URL ordering");
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
                Check(UiErrors.Count == 0, "UI thread exceptions: " + String.Join(" | ", UiErrors.Select(error => error.Message).ToArray()));
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
                    if (!form.IsDisposed) Console.Error.WriteLine("Host status: " + Member<Label>(form, "su").Text);
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

        private sealed class LoopbackSite : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly Thread worker;
            private volatile bool stopped;
            private Exception error;
            public readonly string Url;

            public LoopbackSite()
            {
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
                            var body = Encoding.UTF8.GetBytes("<title>Lea navigation</title><p>Local test</p>");
                            var header = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
                            stream.Write(header, 0, header.Length);
                            stream.Write(body, 0, body.Length);
                        }
                }
                catch (Exception failure) { if (!stopped) error = failure; }
            }

            public void Dispose()
            {
                stopped = true;
                listener.Stop();
                Check(worker.Join(4000), "loopback server shutdown");
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
