using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Firespider
{
    internal static class Smoke
    {
        [STAThread]
        private static int Main(string[] args)
        {
            var oldSession = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION");
            var session = Path.Combine(Path.GetTempPath(), "lea-firespider-smoke-" + Guid.NewGuid().ToString("N") + ".txt");
            var openPage = Path.Combine(Path.GetTempPath(), "lea-firespider-open-" + Guid.NewGuid().ToString("N") + ".html");
            var savedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-page-" + Guid.NewGuid().ToString("N") + ".html");
            var downloadedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-download-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", session);
                File.WriteAllText(openPage, "<!doctype html><html><head><title>Lea Open Route</title><style>.hot{color:#b91c1c;font-weight:bold}</style><script type='text/lea'>v('who','Firespider')</script></head><body><p>Open route fixture</p><p>Second route match</p><p>Third ROUTE match</p><p id='class-state'>Waiting for class state</p><p id='state'>Waiting for host click</p><button id='host-action' type='button' data-lea=\"tx('state','Hello ${who}'); v('last','click'); a('Safe host click')\">Run safe action</button></body></html>");
                var root = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;
                var assembly = Assembly.LoadFrom(Path.Combine(root, "Firespider Internet 19.exe"));
                var type = assembly.GetType("Firespider_Internet.j", true);
                string savedUrl;
                using (var form = (Form)Activator.CreateInstance(type, true))
                {
                    form.CreateControl();
                    var init = type.GetMethod("qleaInit", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(init != null, "Lea adapter initialization");
                    init.Invoke(form, new object[] { form, EventArgs.Empty });

                    var file = Member<ToolStripMenuItem>(form, "fl");
                    var found = false;
                    foreach (ToolStripItem item in file.DropDownItems)
                        if (item.Text == "New Lea Tab") found = true;
                    Check(found, "New Lea Tab menu");

                    var open = type.GetMethod("qleaNewClick", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(open != null, "Lea tab command");
                    open.Invoke(form, new object[] { form, EventArgs.Empty });

                    var tabs = Member<TabControl>(form, "k");
                    var host = tabs.SelectedTab == null || tabs.SelectedTab.Controls.Count == 0 ? null : tabs.SelectedTab.Controls[0] as V;

                    Check(host != null, "Lea view in selected tab");
                    Check(host.At != null && host.T == "Lea in Firespider", "bundled Lea start page");
                    var scriptCommands = host.Js("t('Firespider Script'); bg('#e8f4ef'); fs(19); a('Host alert');");
                    Check(scriptCommands == 4 && host.At.Jc == 4, "Firespider Lea script commands");
                    Check(host.T == "Firespider Script" && host.At.S.Bg == "#e8f4ef" && host.At.S.Fs == 19, "Firespider Lea script state");
                    Check(Member<Label>(form, "su").Text == "Lea: Host alert", "Firespider Lea alert status");
                    host.Go(Path.Combine(root, "Lea", "start.html"));
                    var privateField = type.GetField("pv", BindingFlags.Instance | BindingFlags.NonPublic);
                    var currentLea = type.GetMethod("qleaAt", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(privateField != null && currentLea != null && !host.Pv, "Lea normal cookie mode");
                    privateField.SetValue(form, true);
                    currentLea.Invoke(form, null);
                    Check(host.Pv && host.Kc == 0, "Firespider private mode routed to Lea cookies");
                    privateField.SetValue(form, false);
                    currentLea.Invoke(form, null);
                    Check(!host.Pv, "Firespider normal cookie mode restored");
                    var timer = Member<Timer>(form, "t");
                    var tick = typeof(Timer).GetMethod("OnTick", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(tick != null, "Firespider timer tick");
                    tick.Invoke(timer, new object[] { EventArgs.Empty });
                    var start = Path.Combine(root, "Lea", "start.html");
                    var opened = (bool)type.GetMethod("qleaOpenPath", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { openPage });
                    WaitFor(() => host.At != null && host.T == "Lea Open Route", "Firespider Open File routed to Lea");
                    Check(opened, "Firespider Open File capability");
                    Check(host.Js("ac('class-state','hot')") == 1 && host.Q1("#class-state.hot") != null, "Firespider class command restyle");
                    Check(host.Ck(ById(host.At.D, "host-action")) == 3, "Firespider safe click commands");
                    Check(ById(host.At.D, "state").C[0].X == "Hello Firespider" && host.At.Vs["last"] == "click", "Firespider safe click state");
                    Check(Member<Label>(form, "su").Text == "Lea: Safe host click", "Firespider safe click alert status");
                    var diagnostics = host.Dg();
                    Check(diagnostics != null && diagnostics.N > 0 && diagnostics.J == 5 && diagnostics.Rs.Count > 0, "Firespider Lea diagnostics");
                    var findPage = type.GetMethod("qleaFind", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(findPage != null && (int)findPage.Invoke(form, new object[] { host, "route", false }) == 3, "Firespider Find routed to Lea");
                    Check(host.Fx == 1 && Member<Label>(form, "su").Text == "Lea: match 1 of 3", "Firespider Find status");
                    findPage.Invoke(form, new object[] { host, "route", true });
                    Check(host.Fx == 2 && Member<Label>(form, "su").Text == "Lea: match 2 of 3", "Firespider next Find match");
                    var downloadPath = type.GetMethod("qleaDownloadPath", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(downloadPath != null && (bool)downloadPath.Invoke(form, new object[] { host, new Uri(openPage), downloadedPage }), "Firespider download routed to Lea");
                    Check(File.ReadAllText(downloadedPage).Contains("Third ROUTE match") && Member<Label>(form, "su").Text.Contains(downloadedPage), "Firespider download destination status");
                    var savedPageOk = (bool)type.GetMethod("qleaSavePath", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { savedPage });
                    Check(savedPageOk && File.ReadAllText(savedPage).Contains("Open route fixture"), "Firespider Save Page routed to Lea");
                    var mailLink = (string)type.GetMethod("qleaMailLink", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                    Check(mailLink.StartsWith("mailto:?") && mailLink.Contains("Lea%20Open%20Route"), "Firespider Send Link uses Lea metadata");
                    var address = Member<ToolStripComboBox>(form, "c");
                    address.Text = new Uri(start).AbsoluteUri;
                    var addressKey = new KeyEventArgs(Keys.Enter);
                    type.GetMethod("qleaAddressKeyDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { address, addressKey });
                    WaitFor(() => host.At != null && host.T == "Lea in Firespider", "address Enter routed to Lea");
                    Check(addressKey.Handled && addressKey.SuppressKeyPress, "address Enter handled by Lea");
                    host.Go(start);
                    Check(host.CanBk, "Lea history before host Back");
                    Member<ToolStripItem>(form, "b").PerformClick();
                    Check(host.CanFw, "Firespider Back routed to Lea");

                    if (tabs.TabCount == 1)
                    {
                        var leaTab = tabs.SelectedTab;
                        var legacyTab = new TabPage("Legacy");
                        legacyTab.Controls.Add(new WebBrowser { Dock = DockStyle.Fill });
                        tabs.TabPages.Add(legacyTab);
                        tabs.SelectedTab = leaTab;
                    }
                    var beforeClose = tabs.TabCount;
                    var closeTab = Member<ToolStripMenuItem>(form, "CT");
                    typeof(ToolStripItem).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(closeTab, new object[] { EventArgs.Empty });
                    Check(tabs.TabCount == beforeClose - 1 && FindLea(tabs) == null, "Firespider Close Tab routed to Lea");

                    open.Invoke(form, new object[] { form, EventArgs.Empty });
                    host = FindLea(tabs);
                    Check(host != null && host.At != null, "replacement Lea tab");
                    var source = (string)type.GetMethod("qleaSourceText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                    Check(source == host.H && source.Contains("Lea in Firespider"), "Lea source text");
                    Member<ToolStripMenuItem>(form, "pt").PerformClick();
                    Application.DoEvents();
                    var sourceForm = OpenForm("F3");
                    Check(sourceForm != null && Member<Control>(sourceForm, "RN").Text == host.H, "Firespider Page Source routed to Lea");
                    sourceForm.Close();

                    type.GetMethod("qleaSaveSession", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                    var saved = File.ReadAllLines(session);
                    Check(saved.Length == 1 && saved[0] == host.U.AbsoluteUri, "Lea session saved");
                    savedUrl = saved[0];
                }

                type.GetField("qleaSessionRestored", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, false);
                using (var restoredForm = (Form)Activator.CreateInstance(type, true))
                {
                    restoredForm.CreateControl();
                    type.GetMethod("qleaInit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(restoredForm, new object[] { restoredForm, EventArgs.Empty });
                    var restoredTabs = Member<TabControl>(restoredForm, "k");
                    var restored = FindLea(restoredTabs);
                    Check(restored != null && restored.U != null && restored.U.AbsoluteUri == savedUrl, "Lea session restored");
                    if (restoredTabs.TabCount == 1)
                    {
                        var leaTab = restoredTabs.SelectedTab;
                        var legacyTab = new TabPage("Legacy");
                        legacyTab.Controls.Add(new WebBrowser { Dock = DockStyle.Fill });
                        restoredTabs.TabPages.Add(legacyTab);
                        restoredTabs.SelectedTab = leaTab;
                    }
                    var beforeMiddle = restoredTabs.TabCount;
                    type.GetMethod("qleaTabMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(restoredForm, new object[] { restoredTabs, new MouseEventArgs(MouseButtons.Middle, 1, 0, 0, 0) });
                    Check(restoredTabs.TabCount == beforeMiddle - 1 && FindLea(restoredTabs) == null, "middle-click Lea tab close");
                }

                Console.WriteLine("Firespider Lea smoke passed: class mutations, diagnostics, safe clicks and variables, find and downloads, scripts and alerts, keyboard navigation, open/save/send, private cookies, close routing, sessions, source, menu, timer, start page, and navigation.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Firespider Lea smoke failed: " + error);
                return 1;
            }
            finally
            {
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", oldSession);
                if (File.Exists(session)) File.Delete(session);
                if (File.Exists(openPage)) File.Delete(openPage);
                if (File.Exists(savedPage)) File.Delete(savedPage);
                if (File.Exists(downloadedPage)) File.Delete(downloadedPage);
            }
        }

        private static V FindLea(TabControl tabs)
        {
            foreach (TabPage tab in tabs.TabPages)
                if (tab.Controls.Count > 0 && tab.Controls[0] is V) return (V)tab.Controls[0];
            return null;
        }

        private static N ById(N node, string id)
        {
            if (node == null) return null;
            if (node.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return node;
            foreach (var child in node.C)
            {
                var found = ById(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static Form OpenForm(string name)
        {
            foreach (Form form in Application.OpenForms)
                if (form.GetType().Name == name) return form;
            return null;
        }

        private static T Member<T>(object target, string name) where T : class
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = target.GetType().GetProperty(name, flags);
            var value = property == null ? null : property.GetValue(target, null);
            if (value == null)
            {
                var field = target.GetType().GetField(name, flags);
                value = field == null ? null : field.GetValue(target);
            }
            var result = value as T;
            if (result == null) throw new InvalidOperationException("Missing host member: " + name);
            return result;
        }

        private static void Check(bool value, string name)
        {
            if (!value) throw new InvalidOperationException(name);
        }

        private static void WaitFor(Func<bool> condition, string name)
        {
            var until = DateTime.UtcNow.AddSeconds(5);
            while (!condition() && DateTime.UtcNow < until)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            Check(condition(), name);
        }

    }
}
