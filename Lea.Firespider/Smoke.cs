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
            object settings = null;
            PropertyInfo newTabSetting = null;
            PropertyInfo dashboardSetting = null;
            PropertyInfo shortcutSetting = null;
            string oldNewTab = null;
            string oldDashboard = null;
            object oldShortcuts = null;
            var session = Path.Combine(Path.GetTempPath(), "lea-firespider-smoke-" + Guid.NewGuid().ToString("N") + ".txt");
            var openPage = Path.Combine(Path.GetTempPath(), "lea-firespider-open-" + Guid.NewGuid().ToString("N") + ".html");
            var mapPage = Path.Combine(Path.GetTempPath(), "lea-firespider-map-" + Guid.NewGuid().ToString("N") + ".html");
            var savedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-page-" + Guid.NewGuid().ToString("N") + ".html");
            var downloadedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-download-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", session);
                File.WriteAllText(openPage, "<!doctype html><html><head><title>Lea Open Route</title><style>.hot{color:#b91c1c;font-weight:bold}.host-gauge{color:#176b4d;width:180px;height:16px}.host-value{color:#123f8c;width:180px}</style><script type='text/lea'>v('who','Firespider')</script></head><body><div id='host-invalid-auto' autofocus>Invalid autofocus</div><input id='host-auto' value='ready' autofocus accesskey='s' required aria-label='Host search' aria-describedby='host-help'><span id='host-help'>Host field help</span><input id='host-secret' type='password' value='host-secret' aria-label='Host password'><div id='host-key' tabindex='-1' accesskey='k'>Host focus key</div><p>Open route fixture</p><p>Second route match</p><p>Third ROUTE match</p><p id='class-state'>Waiting for class state</p><p id='state'>Waiting for host click</p><button id='host-action' type='button' accesskey='b' data-lea=\"tx('state','Hello ${who}'); v('last','click'); a('Safe host click')\">Run safe action</button><progress class='host-gauge' id='host-progress' max='10' value='4'>4 of 10</progress><meter class='host-gauge' id='host-meter' min='0' max='4' value='3'>3 of 4</meter><output class='host-value' id='host-output' name='total' for='host-data'>Host total</output><data class='host-value' id='host-data' value='lea-host'>Host label</data><time class='host-value' id='host-time'>2026-08-21</time><ins class='host-value' id='host-edit' cite='go-target.html#host' datetime='2026-08-21T19:00-04:00'>Host edit</ins><button id='host-value-update' type='button' accesskey='b x' data-lea=\"tx('host-output','Updated host total')\">Update host value</button><button id='host-time-update' type='button' data-lea=\"tx('host-time','2026-08-22')\">Update host time</button></body></html>");
                File.WriteAllText(mapPage, "<!doctype html><html><head><title>Lea Map Route</title><style>#host-map-image{width:120px;height:60px}</style></head><body><img id='host-map-image' width='120' height='60' usemap='#host-map' alt='Host map' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYxDP9v0PAAM7Ac+IRop4AAAAAElFTkSuQmCC'><map name='host-map'><area id='host-dead' shape='rect' coords='0,0,10,10'><area id='host-rect' shape='rect' coords='0,0,60,60' tabindex='1' accesskey='m' href='#host-target' alt='Host rectangle'><area id='host-default' shape='default' href='#host-target' alt='Host default'></map><p id='host-target'>Host map target</p></body></html>");
                var root = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;
                var assembly = Assembly.LoadFrom(Path.Combine(root, "Firespider Internet 19.exe"));
                var type = assembly.GetType("Firespider_Internet.j", true);
                var settingsType = assembly.GetType("Firespider_Internet.My.MySettings", true);
                settings = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
                newTabSetting = settingsType.GetProperty("nf", BindingFlags.Instance | BindingFlags.Public);
                dashboardSetting = settingsType.GetProperty("dashboard", BindingFlags.Instance | BindingFlags.Public);
                shortcutSetting = settingsType.GetProperty("sc", BindingFlags.Instance | BindingFlags.Public);
                oldNewTab = (string)newTabSetting.GetValue(settings, null);
                oldDashboard = (string)dashboardSetting.GetValue(settings, null);
                oldShortcuts = shortcutSetting.GetValue(settings, null);
                newTabSetting.SetValue(settings, new Uri(Path.Combine(root, "Lea", "start.html")).AbsoluteUri, null);
                dashboardSetting.SetValue(settings, "", null);
                shortcutSetting.SetValue(settings, new System.Collections.Specialized.StringCollection(), null);
                string savedUrl;
                using (var form = (Form)Activator.CreateInstance(type, true))
                {
                    form.CreateControl();
                    var init = type.GetMethod("qleaInit", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(init != null, "Lea adapter initialization");
                    init.Invoke(form, new object[] { form, EventArgs.Empty });

                    var file = Member<ToolStripMenuItem>(form, "fl");
                    ToolStripMenuItem legacyMenu = null;
                    foreach (ToolStripItem item in file.DropDownItems)
                        if (item.Text == "New Legacy Tab") legacyMenu = item as ToolStripMenuItem;
                    Check(legacyMenu != null && legacyMenu.ShortcutKeys == (Keys.Control | Keys.Alt | Keys.L), "explicit legacy tab menu");
                    var newTab = Member<ToolStripMenuItem>(form, "N");
                    Check(newTab.Text == "New Lea Tab" && newTab.ToolTipText == "Open a new Lea tab", "primary New Tab identifies Lea");
                    var view = Member<ToolStripMenuItem>(form, "v");
                    ToolStripMenuItem inspectorMenu = null;
                    foreach (ToolStripItem item in view.DropDownItems)
                        if (item.Text == "Lea Inspector") inspectorMenu = item as ToolStripMenuItem;
                    Check(inspectorMenu != null && inspectorMenu.ShortcutKeys == Keys.F12, "Lea Inspector menu and shortcut");

                    var tabs = Member<TabControl>(form, "k");
                    newTab.PerformClick();
                    var primaryLeaTab = tabs.SelectedTab;
                    var host = tabs.SelectedTab == null || tabs.SelectedTab.Controls.Count == 0 ? null : tabs.SelectedTab.Controls[0] as V;

                    Check(host != null, "primary New Tab creates Lea view");
                    Check(host.At != null && host.T == "Lea in Firespider", "primary New Tab uses configured start page");
                    var tabCountBeforeShortcut = tabs.TabCount;
                    var shortcut = new KeyEventArgs(Keys.Control | Keys.T);
                    var keyHandler = type.GetMethod("q8", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    Check(keyHandler != null, "Firespider key handler");
                    keyHandler.Invoke(form, new object[] { form, shortcut });
                    Check(shortcut.SuppressKeyPress && tabs.TabCount == tabCountBeforeShortcut + 1 && tabs.SelectedTab.Controls[0] is V, "Ctrl+T creates Lea view");
                    type.GetMethod("qleaClose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { tabs.SelectedTab });
                    tabs.SelectedTab = primaryLeaTab;
                    legacyMenu.PerformClick();
                    Check(tabs.SelectedTab != null && tabs.SelectedTab.Controls.Count > 0 && tabs.SelectedTab.Controls[0] is WebBrowser, "explicit legacy tab fallback");
                    tabs.SelectedTab = primaryLeaTab;
                    Check(host.Tm == 15000, "Firespider Lea request timeout");
                    host.Tm = 1;
                    Check(host.Tm == 100, "Firespider Lea request timeout forwarding");
                    host.Tm = 15000;
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
                    Check(host.Afs().Count == 2 && host.Af().Av("id") == "host-invalid-auto" && host.Ke.Av("id") == "host-auto", "Firespider bounded autofocus selection");
                    Check(host.An == 4 && host.Ak(ById(host.At.D, "host-value-update")) == "X" && host.Kl(ById(host.At.D, "host-action")) == "Alt+Shift+B", "Firespider assigned access-key aliases");
                    var hostAccess = host.Ax(ById(host.At.D, "host-auto"));
                    Check(host.Axs.Count > 10 && hostAccess != null && hostAccess.K == "textbox" && hostAccess.N == "Host search" && hostAccess.D == "Host field help" && hostAccess.Is(Ax.Rq), "Firespider semantic accessibility aliases");
                    Check(host.Ax(ById(host.At.D, "host-secret")).V.Length == 0 && !host.Ab(hostAccess).IsEmpty && host.Ao != null && host.Ao.GetChildCount() > 0, "Firespider accessibility redaction, bounds, and provider");
                    Check(!CommandKey(host, Keys.Alt | Keys.Shift | Keys.Q) && CommandKey(host, Keys.Alt | Keys.Shift | Keys.K), "Firespider command route preserves unmatched and accepts assigned access keys");
                    WaitFor(() => host.Ke != null && host.Ke.Av("id") == "host-key", "Firespider command-route access-key focus");
                    Check(host.Ki == 0 && host.Aa("q").GetAwaiter().GetResult() == 0, "Firespider host-safe access-key focus");
                    Check(BlockById(host.At, "host-progress").Vm.R == 0.4D && BlockById(host.At, "host-meter").Vm.R == 0.75D, "Firespider compact gauge models");
                    Check(host.At.Dvs.Count == 4 && ValueById(host.At, "host-output").V == "Host total" && ValueById(host.At, "host-data").V == "lea-host", "Firespider compact output and data models");
                    Check(ValueById(host.At, "host-time").K == "t" && ValueById(host.At, "host-time").Dt == "2026-08-21", "Firespider compact time model");
                    Check(ValueById(host.At, "host-edit").K == "i" && ValueById(host.At, "host-edit").U == new Uri(host.At.Bu, "go-target.html#host") && BlockById(host.At, "host-edit").S.Td.IndexOf('u') >= 0, "Firespider compact edit model");
                    var fragmentPage = host.At;
                    var fragmentRequests = host.Dg().Rs.Count;
                    host.GoA("#state").GetAwaiter().GetResult();
                    Check(Object.ReferenceEquals(fragmentPage, host.At) && host.U.Fragment == "#state" && host.Ft("#state") != null, "Firespider same-page fragment route");
                    Check(host.Dg().Rs.Count == fragmentRequests, "Firespider fragment route performs no request");
                    host.Bk();
                    Check(host.Js("ac('class-state','hot')") == 1 && host.Q1("#class-state.hot") != null, "Firespider class command restyle");
                    Check(host.Js("vl('host-auto','Host command value'); vl('host-secret','changed-secret'); hd('class-state'); ds('host-auto')") == 4, "Firespider bounded state commands");
                    Check(FieldById(host.At, "host-auto").V == "Host command value" && FieldById(host.At, "host-auto").Ds && host.Ke == null, "Firespider value, disabled, and focus state");
                    Check(FieldById(host.At, "host-secret").V == "changed-secret" && host.Ax(ById(host.At.D, "host-secret")).V.Length == 0, "Firespider command password redaction");
                    Check(host.Ax(ById(host.At.D, "class-state")) == null && host.Js("ck('host-auto')") == 0, "Firespider hidden semantics and checked-target boundary");
                    Check(host.Js("sh('class-state'); en('host-auto'); fo('host-auto')") == 3 && host.Ke != null && host.Ke.Av("id") == "host-auto", "Firespider show, enable, and focus commands");
                    Check(host.Ax(ById(host.At.D, "class-state")) != null && !FieldById(host.At, "host-auto").Ds, "Firespider restored state command model");
                    Check(host.Ck(ById(host.At.D, "host-action")) == 3, "Firespider safe click commands");
                    Check(ById(host.At.D, "state").C[0].X == "Hello Firespider" && host.At.Vs["last"] == "click", "Firespider safe click state");
                    Check(Member<Label>(form, "su").Text == "Lea: Safe host click", "Firespider safe click alert status");
                    Check(host.Ck(ById(host.At.D, "host-value-update")) == 1 && ValueById(host.At, "host-output").V == "Updated host total", "Firespider output value rebuild");
                    Check(host.Ck(ById(host.At.D, "host-time-update")) == 1 && ValueById(host.At, "host-time").Dt == "2026-08-22", "Firespider time metadata rebuild");
                    var diagnostics = host.Dg();
                    Check(diagnostics != null && diagnostics.N > 0 && diagnostics.J == 14 && diagnostics.Rs.Count > 0, "Firespider Lea diagnostics");
                    var inspectorOpen = type.GetMethod("qleaInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic);
                    var inspectorSnapshot = type.GetMethod("qleaInspectorSnapshot", BindingFlags.Instance | BindingFlags.NonPublic);
                    var inspectorClear = type.GetMethod("qleaInspectorClear", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(inspectorOpen != null && inspectorSnapshot != null && inspectorClear != null, "Firespider Lea Inspector routes");
                    var inspector = inspectorOpen.Invoke(form, null) as Form;
                    Check(inspector != null && inspector.Name == "LeaInspector" && inspector.Text == "Lea Inspector", "Firespider Lea Inspector open");
                    Check((bool)inspectorSnapshot.Invoke(form, null), "Firespider Lea Inspector refresh");
                    Check(Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("Response   200") &&
                        Member<TextBox>(form, "qleaInspectorNodes").Text.Contains("body") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains(".hot") &&
                        Member<TextBox>(form, "qleaInspectorRequests").Text.Contains("GET 200 file") &&
                        Member<TextBox>(form, "qleaInspectorMessages").Text.Contains("commands") &&
                        Member<TextBox>(form, "qleaInspectorAccess").Text.Contains("document") &&
                        Member<TextBox>(form, "qleaInspectorAccess").Text.Contains("Host search"), "Firespider Lea Inspector bounded tabs");
                    Check((bool)inspectorClear.Invoke(form, null) && host.Dg().Rs.Count == 0 &&
                        Member<TextBox>(form, "qleaInspectorRequests").Text.Length == 0, "Firespider Lea Inspector clear requests");
                    inspector.Close();
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
                    var goRequests = 0;
                    host.Gr += delegate { goRequests++; };
                    Check(host.Js("go('" + new Uri(start).AbsoluteUri + "')") == 1, "Firespider go command accepted");
                    WaitFor(() => host.At != null && host.T == "Lea in Firespider", "Firespider go request routed by host");
                    Check(goRequests == 1 && host.Gu == new Uri(start), "Firespider go request event and URL");
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
                    Check(inspectorOpen.Invoke(form, null) == null && Member<Label>(form, "su").Text.Contains("Select a Lea tab"), "Lea Inspector leaves legacy tabs untouched");

                    newTab.PerformClick();
                    host = FindLea(tabs);
                    Check(host != null && host.At != null, "replacement Lea tab");
                    var source = (string)type.GetMethod("qleaSourceText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                    Check(source == host.H && source.Contains("Lea in Firespider"), "Lea source text");
                    Member<ToolStripMenuItem>(form, "pt").PerformClick();
                    Application.DoEvents();
                    var sourceForm = OpenForm("F3");
                    Check(sourceForm != null && Member<Control>(sourceForm, "RN").Text == host.H, "Firespider Page Source routed to Lea");
                    sourceForm.Close();

                    host.Go(mapPage);
                    Check(host.At != null && host.T == "Lea Map Route" && host.At.Mps.Count == 1, "Firespider image map route");
                    var hostMapImage = BlockById(host.At, "host-map-image");
                    Check(hostMapImage.Mp != null && hostMapImage.Mp.As.Count == 3 && hostMapImage.Mw == 120 && hostMapImage.Mh == 60, "Firespider compact image map model");
                    Check(hostMapImage.Mp.As[1].K == "r" && hostMapImage.Mp.As[1].U.Fragment == "#host-target", "Firespider compact area model");
                    form.ClientSize = new System.Drawing.Size(800, 600);
                    form.PerformLayout();
                    host.Dock = DockStyle.None;
                    host.Size = new System.Drawing.Size(360, 240);
                    host.CreateControl();
                    host.PerformLayout();
                    PaintView(host);
                    var hostMapHits = MapHitCount(host, false);
                    var hostMapLinks = MapHitCount(host, true);
                    Check(hostMapHits == 3 && hostMapLinks == 2, "Firespider image map renderer hits: " + hostMapHits + "/" + hostMapLinks + " at " + host.Width + "x" + host.Height + ", boxes " + host.Bc + ", scroll " + host.Sy);
                    Check(host.Ti(hostMapImage.Mp.As[1].El) == 1 && host.Kn == 2 && host.Ki == 0, "Firespider compact keyboard aliases");
                    Check(host.Kf() == 1 && host.Ke.Av("id") == "host-rect" && !host.Kb.IsEmpty, "Firespider image map keyboard focus");
                    PaintView(host);
                    Check(host.Ka().GetAwaiter().GetResult() == 1 && host.U.Fragment == "#host-target", "Firespider image map keyboard activation");
                    host.Go(mapPage);
                    PaintView(host);
                    var hostAccessImage = BlockById(host.At, "host-map-image");
                    Check(host.An == 1 && host.Ak(hostAccessImage.Mp.As[1].El) == "M" && host.Kl(hostAccessImage.Mp.As[1].El) == "Alt+Shift+M", "Firespider image-map access-key aliases");
                    Check(host.Aa("m").GetAwaiter().GetResult() == 1 && host.U.Fragment == "#host-target", "Firespider image-map access-key activation");
                    host.Dock = DockStyle.Fill;
                    host.Go(start);
                    Check(host.At != null && host.T == "Lea in Firespider", "Firespider returns from image map route");

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

                Console.WriteLine("Firespider Lea smoke passed: bounded semantic accessibility, protected values, rendered bounds, MSAA provider and Access inspector, bounded autofocus and access keys, keyboard focus and activation, responsive image maps, compact time/edit metadata, output/data values and safe rebuilds, progress and meter gauges, retained same-page fragments, Lea-first New Tab and explicit legacy fallback, host-controlled go requests, Inspector open/refresh/clear and legacy isolation, class mutations, diagnostics, safe clicks and variables, find and downloads, scripts and alerts, open/save/send, private cookies, close routing, sessions, source, menu, timer, start page, and navigation.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Firespider Lea smoke failed: " + error);
                return 1;
            }
            finally
            {
                if (settings != null && newTabSetting != null) newTabSetting.SetValue(settings, oldNewTab, null);
                if (settings != null && dashboardSetting != null) dashboardSetting.SetValue(settings, oldDashboard, null);
                if (settings != null && shortcutSetting != null) shortcutSetting.SetValue(settings, oldShortcuts, null);
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", oldSession);
                if (File.Exists(session)) File.Delete(session);
                if (File.Exists(openPage)) File.Delete(openPage);
                if (File.Exists(mapPage)) File.Delete(mapPage);
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

        private static B BlockById(P page, string id)
        {
            foreach (var block in page.Bs)
                if (block.El != null && block.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return block;
            throw new InvalidOperationException("Missing Lea block: " + id);
        }

        private static Fi FieldById(P page, string id)
        {
            foreach (var field in page.Fs)
                if (field.El != null && field.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return field;
            throw new InvalidOperationException("Missing Lea field: " + id);
        }

        private static Dv ValueById(P page, string id)
        {
            foreach (var value in page.Dvs)
                if (value.El != null && value.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return value;
            throw new InvalidOperationException("Missing Lea value: " + id);
        }

        private static int MapHitCount(V view, bool links)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea image map hits");
            var count = 0;
            foreach (var value in values)
            {
                var type = value.GetType();
                if (type.GetField("A").GetValue(value) == null) continue;
                if (!links || type.GetField("U").GetValue(value) != null) count++;
            }
            return count;
        }

        private static void PaintView(V view)
        {
            var method = typeof(V).GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Missing Lea paint route");
            using (var bitmap = new System.Drawing.Bitmap(Math.Max(1, view.Width), Math.Max(1, view.Height)))
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            using (var args = new PaintEventArgs(graphics, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height)))
                method.Invoke(view, new object[] { args });
        }

        private static Form OpenForm(string name)
        {
            foreach (Form form in Application.OpenForms)
                if (form.GetType().Name == name) return form;
            return null;
        }

        private static bool CommandKey(V view, Keys key)
        {
            var method = typeof(V).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);
            Check(method != null, "Lea command-key route");
            var message = Message.Create(IntPtr.Zero, 0x0104, (IntPtr)(int)(key & Keys.KeyCode), IntPtr.Zero);
            return (bool)method.Invoke(view, new object[] { message, key });
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
