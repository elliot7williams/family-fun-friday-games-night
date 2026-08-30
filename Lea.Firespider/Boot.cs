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
            var mode = args.Length > 1 ? args[1] : "clean";
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
                foreach (var form in forms) { form.Close(); form.Dispose(); }
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
