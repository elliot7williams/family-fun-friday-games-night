using System;
using System.IO;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Text;
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
            var cache = Path.Combine(Path.GetTempPath(), "lea-firespider-cache-" + Guid.NewGuid().ToString("N"));
            try
            {
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", session);
                File.WriteAllText(openPage, "<!doctype html><html><head><title>Lea Open Route</title><style>.hot{color:#b91c1c;font-weight:bold}.host-gauge{color:#176b4d;width:180px;height:16px}.host-value{color:#123f8c;width:180px}.host-wrap{width:160px}.host-flex{display:flex;width:300px;height:60px;gap:8px;justify-content:space-between;align-items:center}.host-flex p{width:70px;height:20px}</style><script type='text/lea'>v('who','Firespider')</script></head><body><div id='host-invalid-auto' autofocus>Invalid autofocus</div><input id='host-auto' value='ready' autofocus accesskey='s' required aria-label='Host search' aria-describedby='host-help'><span id='host-help'>Host field help</span><input id='host-secret' type='password' value='host-secret' aria-label='Host password'><div id='host-key' tabindex='-1' accesskey='k'>Host focus key</div><p>Open route fixture</p><p>Second route match</p><p>Third ROUTE match</p><p id='host-wrap' class='host-wrap'>A compact host paragraph wraps across visual lines for Lea.</p><section id='host-flex' class='host-flex'><p id='host-flex-a'>Flex A</p><p id='host-flex-b'>Flex B</p></section><p id='class-state'>Waiting for class state</p><p id='state'>Waiting for host click</p><button id='host-action' type='button' accesskey='b' data-lea=\"tx('state','Hello ${who}'); v('last','click'); a('Safe host click')\">Run safe action</button><details id='host-details'><summary id='host-details-summary'>Host disclosure</summary><p>Host disclosure content</p></details><progress class='host-gauge' id='host-progress' max='10' value='4'>4 of 10</progress><meter class='host-gauge' id='host-meter' min='0' max='4' value='3'>3 of 4</meter><output class='host-value' id='host-output' name='total' for='host-data'>Host total</output><data class='host-value' id='host-data' value='lea-host'>Host label</data><time class='host-value' id='host-time'>2026-08-21</time><ins class='host-value' id='host-edit' cite='go-target.html#host' datetime='2026-08-21T19:00-04:00'>Host edit</ins><table id='host-table'><thead><tr><th id='host-column' scope='col'>Value</th></tr></thead><tbody><tr><th id='host-row' scope='row'>Lea</th></tr></tbody></table><button id='host-value-update' type='button' accesskey='b x' data-lea=\"tx('host-output','Updated host total')\">Update host value</button><button id='host-time-update' type='button' data-lea=\"tx('host-time','2026-08-22')\">Update host time</button></body></html>");
                File.WriteAllText(mapPage, "<!doctype html><html><head><title>Lea Map Route</title><style>#host-map-image{width:120px;height:60px}</style></head><body><img id='host-map-image' width='120' height='60' usemap='#host-map' alt='Host map' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYxDP9v0PAAM7Ac+IRop4AAAAAElFTkSuQmCC'><map name='host-map'><area id='host-dead' shape='rect' coords='0,0,10,10'><area id='host-rect' shape='rect' coords='0,0,60,60' tabindex='1' accesskey='m' href='#host-target' alt='Host rectangle'><area id='host-default' shape='default' href='#host-target' alt='Host default'></map><p id='host-target'>Host map target</p></body></html>");
                var openMarkup = File.ReadAllText(openPage)
                    .Replace(".host-wrap{width:160px}", ".host-wrap{width:160px}.host-box{box-sizing:border-box;width:180px;height:44px;padding:6px;border:2px solid #176b4d}")
                    .Replace(".host-flex{display:flex;width:300px;height:60px;gap:8px;justify-content:space-between;align-items:center}", ".host-flex{display:flex;width:300px;height:60px;flex-wrap:wrap;gap:6px 8px;justify-content:space-between;align-items:center}")
                    .Replace(".host-flex p{width:70px;height:20px}", ".host-flex p{flex:1 1 180px;height:20px}.host-flex #host-flex-b{flex-shrink:2;min-width:80px}.host-flex-column{display:flex;width:180px;height:100px;flex-direction:column-reverse;row-gap:5px;justify-content:space-evenly;align-items:flex-end}.host-flex-column p{width:70px;margin:0;flex:0 1 60px;min-height:30px}.host-flex-column #host-flex-column-a{order:2;align-self:center}.host-flex-column #host-flex-column-b{order:-1}")
                    .Replace("</style>", ".host-grid{display:grid;width:300px;grid-template-columns:100px 1fr 2fr;gap:7px 10px}.host-grid p{height:20px;margin:0}.host-grid #host-grid-span{grid-column:2/span 2}.host-grid-rows{display:grid;width:300px;height:120px;grid-template-columns:1fr 2fr;grid-template-rows:30px 1fr;gap:6px 8px}.host-grid-rows p{margin:0}.host-grid-rows #host-grid-row-a{grid-column:1/span 2;grid-row:1}.host-grid-rows #host-grid-row-b{grid-column:2;grid-row:2}.host-grid-rows #host-grid-row-c{grid-column:1;grid-row-start:2;grid-row-end:span 1}:root{--host-color:#0f4c81;--host-width:155px;--host-pad:7px}.host-var{--host-color:#0f4c81!important;color:var(--host-color)!important;width:var(--host-width)!important;padding:var(--host-pad)}:is(.host-var,#host-never){border-left:4px solid #176b4d}:where(#host-var){padding-left:2px}.host-var:not(.missing,[data-off]){font-weight:bold}#host-flex>p:nth-child(2n+1){border-right:3px solid #176b4d}#host-flex>p:nth-last-child(-n+1){padding-left:9px}#host-flex-column>p:nth-last-of-type(2n){color:#a13b18}#host-flex>p:nth-child(1 of #host-flex-a,#host-flex-b){border-bottom:3px solid #176b4d}#host-flex>p:nth-last-child(1 of #host-flex-a,#host-flex-b){border-top:2px solid #a13b18}#host-has:has(> .host-direct){width:146px;border-left:5px solid #176b4d}#host-has:has(> .host-body .host-ready[data-host=ready]){padding-right:6px}#host-flex:has(+ #host-has){border-top:4px solid #a13b18}#host-has:has(input:focus){background:#dff6e9;font-weight:bold}.host-media{display:none}@media screen and (max-width:500px){:root{--host-width:110px;--host-pad:4px}#host-media{display:block;width:120px;background:#e8f4ef}#host-box{display:none}}</style>")
                    .Replace("</style>", ".host-grid-areas{display:grid;width:300px;height:100px;grid-template-columns:100px 1fr;grid-template-rows:30px 1fr;grid-template-areas:'top top' 'left .';gap:5px 10px}.host-grid-areas p{margin:0}.host-grid-areas #host-grid-area-top{grid-area:top}.host-grid-areas #host-grid-area-left{grid-area:left}</style>")
                    .Replace("</style>", ".host-grid-lines{display:grid;width:300px;height:100px;grid-template-columns:[left] 100px [middle] 1fr [right];grid-template-rows:[top] 30px [body] 1fr [bottom];gap:5px 10px}.host-grid-lines p{margin:0}.host-grid-lines #host-grid-line-top{grid-column:left/right;grid-row:top/body}.host-grid-lines #host-grid-line-left{grid-column:left/middle;grid-row:body/bottom}</style>")
                    .Replace("</style>", ".host-grid-flow{display:grid;width:300px;grid-template-columns:repeat(3,1fr);grid-auto-flow:row dense;gap:6px}.host-grid-flow p{height:20px;margin:0}.host-grid-flow #host-grid-flow-a,.host-grid-flow #host-grid-flow-b{grid-column:span 2}.host-grid-column{display:grid;width:300px;height:100px;grid-template-rows:repeat(2,1fr);grid-auto-flow:column;gap:5px 10px}.host-grid-column p{margin:0}</style>")
                    .Replace("</style>", ".host-grid-auto-tracks{display:grid;width:300px;height:100px;grid-template-columns:60px;grid-template-rows:20px;grid-auto-columns:40px 1fr;grid-auto-rows:30px minmax(0,2fr);gap:5px 10px}.host-grid-auto-tracks p{margin:0}.host-grid-auto-tracks #host-grid-auto-a{grid-column:1;grid-row:1}.host-grid-auto-tracks #host-grid-auto-b{grid-column:2;grid-row:2}.host-grid-auto-tracks #host-grid-auto-c{grid-column:3;grid-row:3}</style>")
                    .Replace("</style>", ".host-grid-shorthand{display:grid;width:300px;height:100px;grid:auto-flow dense 30px 1fr/60px 1fr 1fr;gap:5px 10px}.host-grid-shorthand p{margin:0}.host-grid-shorthand #host-grid-short-a,.host-grid-shorthand #host-grid-short-b{grid-column:span 2}.host-grid-short-area{display:grid;width:300px;height:100px;grid:'top top' 30px 'left right' 1fr/80px 1fr;gap:5px 10px}.host-grid-short-area p{margin:0}.host-grid-short-area #host-grid-short-top{grid-area:top}.host-grid-short-area #host-grid-short-left{grid-area:left}.host-grid-short-area #host-grid-short-right{grid-area:right}.host-grid-short-column{display:grid;width:300px;height:100px;grid:25px 1fr/dense auto-flow 40px 1fr;gap:5px 10px}.host-grid-short-column p{margin:0}</style>")
                    .Replace("</style>", ".host-grid-align{display:grid;width:300px;height:100px;grid:100px/140px 150px;column-gap:10px;place-items:center}.host-grid-align p{margin:0;padding:0}.host-grid-align #host-grid-align-a{width:40px;height:20px}.host-grid-align #host-grid-align-b{width:50px;height:30px;place-self:end}</style>")
                    .Replace("</style>", ".host-grid-content{display:grid;width:300px;height:220px;grid:40px 60px/80px 100px;place-content:space-around space-evenly}.host-grid-content p{margin:0;padding:0}</style>")
                    .Replace("</section><p id='class-state'>", "</section><section id='host-has'><span class='host-direct'>Direct host child</span><div class='host-body'><span class='host-ready' data-host='ready'>Ready host descendant</span></div><input id='host-has-input' value='Relational host field'></section><section id='host-grid' class='host-grid'><p id='host-grid-a'>Grid A</p><p id='host-grid-b'>Grid B</p><p id='host-grid-c'>Grid C</p><p id='host-grid-span'>Grid span</p></section><section id='host-grid-rows' class='host-grid-rows'><p id='host-grid-row-a'>Grid row A</p><p id='host-grid-row-b'>Grid row B</p><p id='host-grid-row-c'>Grid row C</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-areas' class='host-grid-areas'><p id='host-grid-area-auto'>Area auto</p><p id='host-grid-area-top'>Area top</p><p id='host-grid-area-left'>Area left</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-lines' class='host-grid-lines'><p id='host-grid-line-auto'>Line auto</p><p id='host-grid-line-top'>Line top</p><p id='host-grid-line-left'>Line left</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-flow' class='host-grid-flow'><p id='host-grid-flow-a'>Flow A</p><p id='host-grid-flow-b'>Flow B</p><p id='host-grid-flow-c'>Flow C</p></section><section id='host-grid-column' class='host-grid-column'><p id='host-grid-column-a'>Column A</p><p id='host-grid-column-b'>Column B</p><p id='host-grid-column-c'>Column C</p><p id='host-grid-column-d'>Column D</p><p id='host-grid-column-e'>Column E</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-auto-tracks' class='host-grid-auto-tracks'><p id='host-grid-auto-a'>Auto track A</p><p id='host-grid-auto-b'>Auto track B</p><p id='host-grid-auto-c'>Auto track C</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-shorthand' class='host-grid-shorthand'><p id='host-grid-short-a'>Short A</p><p id='host-grid-short-b'>Short B</p><p id='host-grid-short-c'>Short C</p></section><section id='host-grid-short-area' class='host-grid-short-area'><p id='host-grid-short-top'>Short top</p><p id='host-grid-short-left'>Short left</p><p id='host-grid-short-right'>Short right</p></section><section id='host-grid-short-column' class='host-grid-short-column'><p id='host-grid-short-column-a'>Column A</p><p id='host-grid-short-column-b'>Column B</p><p id='host-grid-short-column-c'>Column C</p><p id='host-grid-short-column-d'>Column D</p><p id='host-grid-short-column-e'>Column E</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-align' class='host-grid-align'><p id='host-grid-align-a'>Align A</p><p id='host-grid-align-b'>Align B</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-content' class='host-grid-content'><p id='host-grid-content-a'>Content A</p><p id='host-grid-content-b'>Content B</p><p id='host-grid-content-c'>Content C</p><p id='host-grid-content-d'>Content D</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-var' class='host-var' style='--host-color:#a13b18;color:#a13b18;width:199px'>Important property host</p><p id='host-media' class='host-media'>Responsive host</p><p id='host-box' class='host-box'>Border-box host</p><section id='host-flex-column' class='host-flex-column'><p id='host-flex-column-a'>Column A</p><p id='host-flex-column-b'>Column B</p></section><svg id='host-svg' width='160' height='80' viewBox='0 0 80 40'><title>Firespider host vector</title><rect x='2' y='2' width='20' height='16' fill='#d7263d'></rect><circle cx='34' cy='10' r='8' fill='#10b981'></circle><path d='M46 2 h28 v16 H46 z' fill='#f59e0b'></path><a href='#state'><rect x='12' y='24' width='56' height='12' fill='#0369a1'></rect></a></svg><p id='class-state'>")
                    .Replace("</svg><p id='class-state'>", "<path d='M2 32 C8 20 14 20 20 32 S32 44 38 32' fill='none' stroke='#7c3aed' stroke-width='2'></path><g transform='translate(58 26)'><g transform='scale(1.5)'><rect x='0' y='0' width='10' height='6' fill='#db2777'></rect></g></g><defs><linearGradient id='host-linear'><stop offset='0' stop-color='#dc2626'></stop><stop offset='1' stop-color='#2563eb'></stop></linearGradient><radialGradient id='host-radial'><stop offset='0' stop-color='#f8fafc'></stop><stop offset='1' stop-color='#7c3aed'></stop></radialGradient></defs><rect x='40' y='24' width='14' height='10' fill='url(#host-linear)'></rect><circle cx='69' cy='10' r='7' fill='url(#host-radial)'></circle></svg><p id='class-state'>");
                File.WriteAllText(openPage, openMarkup);
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
                    var timer = Member<Timer>(form, "t");
                    var tick = typeof(Timer).GetMethod("OnTick", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(tick != null, "Firespider timer tick");
                    tick.Invoke(timer, new object[] { EventArgs.Empty });
                    var pendingTab = new TabPage("Starting");
                    tabs.TabPages.Add(pendingTab);
                    tabs.SelectedTab = pendingTab;
                    tick.Invoke(timer, new object[] { EventArgs.Empty });
                    tabs.TabPages.Remove(pendingTab);
                    pendingTab.Dispose();
                    Check(Member<Label>(form, "su").Text == "Ready", "Firespider startup timer tolerates missing tabs and controls");
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
                    Check(host.Cd == "" && host.Cx == 32 && host.Cb == 16L * 1024 * 1024, "Firespider persistent cache stays opt-in and bounded");
                    host.Cd = cache;
                    host.Cm = 3;
                    host.Cx = 4;
                    host.Cb = 4096;
                    Check(host.Cd == Path.GetFullPath(cache) && host.Cm == 3 && host.Cx == 4 && host.Cb == 4096 && host.Dc == 0 && host.Db == 0, "Firespider persistent cache aliases");
                    host.Cl();
                    host.Cdl();
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
                    var automationRoot = AutomationElement.FromHandle(host.Handle);
                    var automationHost = automationRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host-auto"));
                    object automationValue;
                    Check(automationRoot.Current.ControlType == ControlType.Document && automationRoot.Current.FrameworkId == "Lea" &&
                        automationHost != null && automationHost.Current.ControlType == ControlType.Edit &&
                        automationHost.TryGetCurrentPattern(ValuePattern.Pattern, out automationValue), "Firespider UI Automation document and value provider");
                    var automationTable = automationRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host-table"));
                    object automationGrid = null;
                    object automationTablePattern = null;
                    Check(automationTable != null && automationTable.TryGetCurrentPattern(GridPattern.Pattern, out automationGrid) &&
                        automationTable.TryGetCurrentPattern(TablePattern.Pattern, out automationTablePattern) &&
                        ((GridPattern)automationGrid).Current.RowCount == 2 && ((GridPattern)automationGrid).Current.ColumnCount == 1 &&
                        ((TablePattern)automationTablePattern).Current.GetColumnHeaders().Length == 1 && ((TablePattern)automationTablePattern).Current.GetRowHeaders().Length == 1,
                        "Firespider UI Automation table and grid providers");
                    object automationTextPattern;
                    Check(automationRoot.TryGetCurrentPattern(TextPattern.Pattern, out automationTextPattern), "Firespider UI Automation text provider");
                    var automationDocument = ((TextPattern)automationTextPattern).DocumentRange;
                    var automationText = automationDocument.GetText(-1);
                    var automationFoundText = automationDocument.FindText("host TOTAL", false, true);
                    Check(automationText.Contains("Host total") && !automationText.Contains("host-secret") &&
                        (bool)automationDocument.GetAttributeValue(TextPattern.IsReadOnlyAttribute) && automationFoundText != null && automationFoundText.GetText(-1) == "Host total",
                        "Firespider UI Automation logical text, search, and redaction");
                    var automationWrap = automationRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host-wrap"));
                    var automationWrapRange = ((TextPattern)automationTextPattern).RangeFromChild(automationWrap);
                    automationWrapRange.ScrollIntoView(true);
                    var automationLine = automationWrapRange.Clone();
                    automationLine.ExpandToEnclosingUnit(TextUnit.Line);
                    var automationWrapBounds = automationWrapRange.GetBoundingRectangles().Length;
                    var automationLineBounds = automationLine.GetBoundingRectangles().Length;
                    var automationLineMove = automationLine.Move(TextUnit.Line, 1);
                    Check(automationWrapBounds > 1 && automationLineBounds == 1 && automationLineMove == 1,
                        "Firespider UI Automation visual line geometry: " + automationWrapBounds + ":" + automationLineBounds + ":" + automationLineMove);
                    Check(!CommandKey(host, Keys.Alt | Keys.Shift | Keys.Q) && CommandKey(host, Keys.Alt | Keys.Shift | Keys.K), "Firespider command route preserves unmatched and accepts assigned access keys");
                    WaitFor(() => host.Ke != null && host.Ke.Av("id") == "host-key", "Firespider command-route access-key focus");
                    Check(host.Ki == 0 && host.Aa("q").GetAwaiter().GetResult() == 0, "Firespider host-safe access-key focus");
                    var hostFlex = BlockById(host.At, "host-flex");
                    var hostFlexA = BlockById(host.At, "host-flex-a");
                    var hostFlexB = BlockById(host.At, "host-flex-b");
                    Check(hostFlex.S.Fd == "r" && hostFlex.S.Jc == "b" && hostFlex.S.Ai == "c" && hostFlex.S.Wr == "w" && hostFlex.S.Rg == 6 && hostFlex.S.Gp == 8 &&
                        hostFlexA.S.Fg == 1 && hostFlexA.S.Sh == 1 && hostFlexA.S.Fb == 180 && hostFlexB.S.Sh == 2 && hostFlexB.S.Wn == 80 &&
                        Object.ReferenceEquals(hostFlexA.Up, hostFlex), "Firespider bounded wrapped flex-row model");
                    var hostColumn = BlockById(host.At, "host-flex-column");
                    var hostColumnA = BlockById(host.At, "host-flex-column-a");
                    var hostColumnB = BlockById(host.At, "host-flex-column-b");
                    Check(hostColumn.S.Fd == "y" && hostColumn.S.Rg == 5 && hostColumn.S.Jc == "v" && hostColumn.S.Ai == "e" && hostColumnA.S.Or == 2 && hostColumnA.S.As == "c" && hostColumnB.S.Or == -1 &&
                        hostColumnA.S.Sh == 1 && hostColumnA.S.Fb == 60 && hostColumnA.S.Hn == 30 && Object.ReferenceEquals(hostColumnA.Up, hostColumn),
                        "Firespider bounded distributed column-reverse model");
                    host.Dock = DockStyle.None;
                    host.Size = new System.Drawing.Size(720, 560);
                    Application.DoEvents();
                    PaintView(host);
                    var hostBox = BlockById(host.At, "host-box");
                    var automationHostBox = automationRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host-box"));
                    Check(automationHostBox != null, "Firespider border-box automation element");
                    ((TextPattern)automationTextPattern).RangeFromChild(automationHostBox).ScrollIntoView(true);
                    PaintView(host);
                    var hostBoxBounds = host.Ab(host.Ax(ById(host.At.D, "host-box")));
                    Check(hostBox.S.Bz == "b" && hostBox.S.W == 180 && hostBox.S.H == 44 && hostBoxBounds.Width > 0 && hostBoxBounds.Width <= hostBox.S.W && hostBoxBounds.Height == 44,
                        "Firespider bounded border-box model and visible geometry: " + hostBox.S.Bz + "/" + hostBox.S.W + "x" + hostBox.S.H + " -> " + hostBoxBounds.Width + "x" + hostBoxBounds.Height);
                    var hostResponsiveField = FieldById(host.At, "host-auto");
                    hostResponsiveField.V = "Retained responsive host value";
                    Check(host.Vw > 500 && host.At.Vw == host.Vw && host.Dg().Vw == host.Vw && HasBlock(host.At, "host-box") && !HasBlock(host.At, "host-media"), "Firespider wide viewport aliases and media state");
                    var hostVariable = BlockById(host.At, "host-var");
                    Check(hostVariable.S.F == "#0f4c81" && hostVariable.S.W == 155 && hostVariable.S.Pl == 7 && hostVariable.S.Bl == 4 && hostVariable.S.B && hostVariable.S.Vc == 3 && hostVariable.S.Cv("--host-color") == "#0f4c81" && hostVariable.S.Cv("--host-width") == "155px" && host.Dg().Vc == 3 && host.Dg().Ic == 3, "Firespider functional selector, important, and custom property values");
                    Check(host.Qa(":is(.host-var,.host-media)").Count == 2 && host.Q1(":where(#host-var)") != null && host.Qa(".host-var:not(.missing,[data-off])").Count == 1, "Firespider functional selector queries");
                    Check(hostFlexA.S.Br == 3 && hostFlexB.S.Br == 0 && hostFlexB.S.Pl == 9 && hostColumnA.S.F == "#a13b18" && hostColumnB.S.F != "#a13b18", "Firespider structural arithmetic styles");
                    Check(host.Qa("#host-flex>p:nth-child(2n+1)").Count == 1 && host.Qa("#host-flex>p:nth-last-child(-n+1)").Count == 1 && host.Qa("#host-flex-column>p:nth-last-of-type(2n)").Count == 1, "Firespider structural arithmetic queries");
                    Check(hostFlexA.S.Bb == 3 && hostFlexA.S.Bt == 0 && hostFlexB.S.Bb == 0 && hostFlexB.S.Bt == 2, "Firespider filtered structural styles");
                    Check(host.Qa("#host-flex>p:nth-child(1 of #host-flex-a,#host-flex-b)").Count == 1 && host.Qa("#host-flex>p:nth-last-child(1 of #host-flex-a,#host-flex-b)").Count == 1, "Firespider filtered structural queries");
                    var hostHas = BlockById(host.At, "host-has");
                    var hostHasField = FieldById(host.At, "host-has-input");
                    hostHasField.V = "Retained relational host value";
                    Check(hostHas.S.W == 146 && hostHas.S.Bl == 5 && hostHas.S.Pr == 6 && hostFlex.S.Bt == 4 && hostHas.S.Bg == "", "Firespider relational selector styles and geometry");
                    Check(host.Qa("#host-has:has(> .host-direct)").Count == 1 && host.Qa("#host-has:has(> .host-body .host-ready[data-host=ready])").Count == 1 && host.Qa("#host-flex:has(+ #host-has)").Count == 1, "Firespider relational selector queries");
                    Check(host.Js("fo('host-has-input')") == 1 && BlockById(host.At, "host-has").S.Bg == "#dff6e9" && BlockById(host.At, "host-has").S.B && Object.ReferenceEquals(hostHasField, FieldById(host.At, "host-has-input")) && hostHasField.V == "Retained relational host value", "Firespider relational focus rebuild and retention");
                    Check(host.Js("fo('host-auto')") == 1 && BlockById(host.At, "host-has").S.Bg == "", "Firespider relational focus restore");
                    var hostGrid = BlockById(host.At, "host-grid");
                    var hostGridSpan = BlockById(host.At, "host-grid-span");
                    Check(hostGrid.S.D == "grid" && hostGrid.S.Gc == 3 && hostGrid.S.Gv(0) == 100 && hostGrid.S.Gv(1) == -1 && hostGrid.S.Gv(2) == -2 && hostGrid.S.Rg == 7 && hostGrid.S.Gp == 10 && hostGridSpan.S.Gl == 2 && hostGridSpan.S.Gs == 2,
                        "Firespider bounded grid model and aliases");
                    Check(host.Qa("#host-grid>p").Count == 4 && Object.ReferenceEquals(BlockById(host.At, "host-grid-a").Up, hostGrid), "Firespider grid query and ownership");
                    var hostGridABox = BlockOuter(host, "host-grid-a");
                    var hostGridBBox = BlockOuter(host, "host-grid-b");
                    var hostGridCBox = BlockOuter(host, "host-grid-c");
                    var hostGridSpanBox = BlockOuter(host, "host-grid-span");
                    Check(hostGridABox.Width == 100 && hostGridBBox.Width == 60 && hostGridCBox.Width == 120 && hostGridBBox.Left - hostGridABox.Right == 10 && hostGridSpanBox.Width == 190 && hostGridSpanBox.Left == hostGridBBox.Left && hostGridSpanBox.Top - hostGridABox.Bottom == 7,
                        "Firespider bounded grid geometry");
                    var hostGridRows = BlockById(host.At, "host-grid-rows");
                    var hostGridRowA = BlockById(host.At, "host-grid-row-a");
                    var hostGridRowB = BlockById(host.At, "host-grid-row-b");
                    Check(hostGridRows.S.Rc == 2 && hostGridRows.S.Rv(0) == 30 && hostGridRows.S.Rv(1) == -1 && hostGridRowA.S.Rl == 1 && hostGridRowA.S.Gs == 2 && hostGridRowB.S.Rl == 2 && hostGridRowB.S.Gl == 2,
                        "Firespider bounded grid row aliases");
                    var hostGridRowABox = BlockOuter(host, "host-grid-row-a");
                    var hostGridRowBBox = BlockOuter(host, "host-grid-row-b");
                    var hostGridRowCBox = BlockOuter(host, "host-grid-row-c");
                    Check(hostGridRowABox.Width == 300 && hostGridRowABox.Height == 30 && hostGridRowBBox.Width == 195 && hostGridRowBBox.Height == 84 && hostGridRowCBox.Width == 97 && hostGridRowCBox.Height == 84 && hostGridRowBBox.Left - hostGridRowCBox.Right == 8 && hostGridRowBBox.Top - hostGridRowABox.Bottom == 6,
                        "Firespider bounded grid row geometry");
                    var hostGridAreas = BlockById(host.At, "host-grid-areas");
                    var hostGridAreaTop = BlockById(host.At, "host-grid-area-top");
                    var hostGridAreaLeft = BlockById(host.At, "host-grid-area-left");
                    Check(hostGridAreas.S.Aa == 2 && hostGridAreas.S.Ar == 2 && hostGridAreas.S.Ac == 2 && hostGridAreas.S.Ag("top", 3) == 2 && hostGridAreaTop.S.An == "top" && hostGridAreaLeft.S.An == "left",
                        "Firespider bounded named grid area aliases");
                    var hostGridAreaTopBox = BlockOuter(host, "host-grid-area-top");
                    var hostGridAreaLeftBox = BlockOuter(host, "host-grid-area-left");
                    var hostGridAreaAutoBox = BlockOuter(host, "host-grid-area-auto");
                    Check(hostGridAreaTopBox.Width == 300 && hostGridAreaTopBox.Height == 30 && hostGridAreaLeftBox.Width == 100 && hostGridAreaLeftBox.Height == 65 && hostGridAreaAutoBox.Width == 190 && hostGridAreaAutoBox.Height == 65 && hostGridAreaAutoBox.Left - hostGridAreaLeftBox.Right == 10 && hostGridAreaAutoBox.Top - hostGridAreaTopBox.Bottom == 5,
                        "Firespider bounded named grid area geometry");
                    var hostGridLines = BlockById(host.At, "host-grid-lines");
                    var hostGridLineTop = BlockById(host.At, "host-grid-line-top");
                    var hostGridLineLeft = BlockById(host.At, "host-grid-line-left");
                    Check(hostGridLines.S.Gnc == 3 && hostGridLines.S.Rnc == 3 && hostGridLines.S.Gx("middle") == 2 && hostGridLines.S.Rx("bottom") == 3 && hostGridLineTop.S.Gn == "left" && hostGridLineTop.S.Ge == "right" && hostGridLineLeft.S.Rn == "body",
                        "Firespider bounded named grid line aliases");
                    var hostGridLineTopBox = BlockOuter(host, "host-grid-line-top");
                    var hostGridLineLeftBox = BlockOuter(host, "host-grid-line-left");
                    var hostGridLineAutoBox = BlockOuter(host, "host-grid-line-auto");
                    Check(hostGridLineTopBox.Width == 300 && hostGridLineTopBox.Height == 30 && hostGridLineLeftBox.Width == 100 && hostGridLineLeftBox.Height == 65 && hostGridLineAutoBox.Width == 190 && hostGridLineAutoBox.Height == 65 && hostGridLineAutoBox.Left - hostGridLineLeftBox.Right == 10 && hostGridLineAutoBox.Top - hostGridLineTopBox.Bottom == 5,
                        "Firespider bounded named grid line geometry");
                    var hostGridFlow = BlockById(host.At, "host-grid-flow");
                    var hostGridFlowA = BlockOuter(host, "host-grid-flow-a");
                    var hostGridFlowB = BlockOuter(host, "host-grid-flow-b");
                    var hostGridFlowC = BlockOuter(host, "host-grid-flow-c");
                    Check(hostGridFlow.S.Gf == "d" && hostGridFlowA.Width == 198 && hostGridFlowC.Width == 96 && hostGridFlowC.Top == hostGridFlowA.Top && hostGridFlowB.Top > hostGridFlowA.Bottom,
                        "Firespider bounded dense row grid flow");
                    var hostGridColumn = BlockById(host.At, "host-grid-column");
                    var hostGridColumnA = BlockOuter(host, "host-grid-column-a");
                    var hostGridColumnB = BlockOuter(host, "host-grid-column-b");
                    var hostGridColumnC = BlockOuter(host, "host-grid-column-c");
                    var hostGridColumnE = BlockOuter(host, "host-grid-column-e");
                    Check(hostGridColumn.S.Gf == "c" && hostGridColumnA.Width == 93 && hostGridColumnC.Width == 93 && hostGridColumnE.Width == 94 && hostGridColumnA.Left == hostGridColumnB.Left && hostGridColumnC.Left - hostGridColumnA.Right == 10,
                        "Firespider bounded column grid flow");
                    var hostGridAutoTracks = BlockById(host.At, "host-grid-auto-tracks");
                    var hostGridAutoA = BlockOuter(host, "host-grid-auto-a");
                    var hostGridAutoB = BlockOuter(host, "host-grid-auto-b");
                    var hostGridAutoC = BlockOuter(host, "host-grid-auto-c");
                    Check(hostGridAutoTracks.S.Gac == 2 && hostGridAutoTracks.S.Gav(0) == 40 && hostGridAutoTracks.S.Gav(1) == -1 && hostGridAutoTracks.S.Rac == 2 && hostGridAutoTracks.S.Rav(0) == 30 && hostGridAutoTracks.S.Rav(1) == -2,
                        "Firespider bounded grid auto-track aliases");
                    Check(hostGridAutoA.Width == 60 && hostGridAutoA.Height == 20 && hostGridAutoB.Width == 40 && hostGridAutoB.Height == 30 && hostGridAutoC.Width == 180 && hostGridAutoC.Height == 40 && hostGridAutoB.Left - hostGridAutoA.Right == 10 && hostGridAutoC.Top - hostGridAutoB.Bottom == 5,
                        "Firespider bounded grid auto-track geometry");
                    var hostGridShorthand = BlockById(host.At, "host-grid-shorthand");
                    var hostGridShortA = BlockOuter(host, "host-grid-short-a");
                    var hostGridShortB = BlockOuter(host, "host-grid-short-b");
                    var hostGridShortC = BlockOuter(host, "host-grid-short-c");
                    Check(hostGridShorthand.S.Gf == "d" && hostGridShorthand.S.Gc == 3 && hostGridShorthand.S.Rc == 0 && hostGridShorthand.S.Rac == 2 && hostGridShorthand.S.Rav(0) == 30 && hostGridShorthand.S.Rav(1) == -1 && hostGridShortA.Width == 180 && hostGridShortA.Height == 30 && hostGridShortC.Width == 110 && hostGridShortC.Top == hostGridShortA.Top && hostGridShortB.Height == 65,
                        "Firespider bounded row auto-flow grid shorthand");
                    var hostGridShortArea = BlockById(host.At, "host-grid-short-area");
                    var hostGridShortTop = BlockOuter(host, "host-grid-short-top");
                    var hostGridShortLeft = BlockOuter(host, "host-grid-short-left");
                    var hostGridShortRight = BlockOuter(host, "host-grid-short-right");
                    Check(hostGridShortArea.S.Aa == 3 && hostGridShortArea.S.Ar == 2 && hostGridShortArea.S.Ac == 2 && hostGridShortTop.Width == 300 && hostGridShortTop.Height == 30 && hostGridShortLeft.Width == 80 && hostGridShortLeft.Height == 65 && hostGridShortRight.Width == 210 && hostGridShortRight.Height == 65,
                        "Firespider bounded quoted-area grid shorthand");
                    var hostGridShortColumn = BlockById(host.At, "host-grid-short-column");
                    var hostGridShortColumnA = BlockOuter(host, "host-grid-short-column-a");
                    var hostGridShortColumnB = BlockOuter(host, "host-grid-short-column-b");
                    var hostGridShortColumnC = BlockOuter(host, "host-grid-short-column-c");
                    var hostGridShortColumnE = BlockOuter(host, "host-grid-short-column-e");
                    Check(hostGridShortColumn.S.Gf == "x" && hostGridShortColumn.S.Rc == 2 && hostGridShortColumn.S.Gac == 2 && hostGridShortColumnA.Width == 40 && hostGridShortColumnA.Height == 25 && hostGridShortColumnB.Height == 70 && hostGridShortColumnC.Width == 200 && hostGridShortColumnE.Width == 40,
                        "Firespider bounded column auto-flow grid shorthand");
                    var hostGridAlign = BlockOuter(host, "host-grid-align");
                    var hostGridAlignStyle = BlockById(host.At, "host-grid-align").S;
                    var hostGridAlignA = BlockOuter(host, "host-grid-align-a");
                    var hostGridAlignB = BlockOuter(host, "host-grid-align-b");
                    Check(hostGridAlignStyle.Ji == "c" && hostGridAlignStyle.Ai == "c" && BlockById(host.At, "host-grid-align-b").S.Js == "e" && BlockById(host.At, "host-grid-align-b").S.As == "e",
                        "Firespider bounded grid alignment aliases");
                    Check(hostGridAlignA.Size == new System.Drawing.Size(40, 20) && hostGridAlignA.Left - hostGridAlign.Left == 50 && hostGridAlignA.Top - hostGridAlign.Top == 40 && hostGridAlignB.Size == new System.Drawing.Size(50, 30) && hostGridAlignB.Left - hostGridAlign.Left == 250 && hostGridAlignB.Top - hostGridAlign.Top == 70,
                        "Firespider bounded grid alignment geometry");
                    var hostGridContent = BlockOuter(host, "host-grid-content");
                    var hostGridContentStyle = BlockById(host.At, "host-grid-content").S;
                    var hostGridContentA = BlockOuter(host, "host-grid-content-a");
                    var hostGridContentB = BlockOuter(host, "host-grid-content-b");
                    var hostGridContentC = BlockOuter(host, "host-grid-content-c");
                    Check(hostGridContentStyle.Al == "a" && hostGridContentStyle.Jc == "v", "Firespider bounded grid content alignment aliases");
                    Check(hostGridContentA.Size == new System.Drawing.Size(80, 40) && hostGridContentA.Left - hostGridContent.Left == 40 && hostGridContentA.Top - hostGridContent.Top == 30 && hostGridContentB.Left - hostGridContent.Left == 160 && hostGridContentC.Top - hostGridContent.Top == 130,
                        "Firespider bounded grid content alignment geometry");
                    var hostSvg = BlockById(host.At, "host-svg");
                    var hostSvgAccess = host.Ax(ById(host.At.D, "host-svg"));
                    Check(hostSvg.Vg != null && hostSvg.Vg.Vb && hostSvg.Vg.Ds.Count == 2 && hostSvg.Vg.Ss.Count == 8 && hostSvg.Vg.Ss[2].D.Count == 5 && hostSvg.Vg.Ss[3].U.Fragment == "#state" &&
                        hostSvg.Vg.Ss[4].D.Count == 3 && hostSvg.Vg.Ss[4].D[2].K == 'C' && hostSvg.Vg.Ss[5].M[0] == 1.5D && hostSvg.Vg.Ss[5].M[4] == 58D &&
                        hostSvg.Vg.Ds[0].K == "l" && hostSvg.Vg.Ds[0].Ss.Count == 2 && hostSvg.Vg.Ds[1].K == "r" && hostSvg.Vg.Ss[6].Fd == hostSvg.Vg.Ds[0] && hostSvg.Vg.Ss[7].Fd == hostSvg.Vg.Ds[1],
                        "Firespider bounded inline SVG aliases");
                    Check(BlockOuter(host, "host-svg").Size == new System.Drawing.Size(160, 80) && hostSvgAccess != null && hostSvgAccess.K == "img" && hostSvgAccess.N == "Firespider host vector",
                        "Firespider bounded inline SVG geometry and accessibility");
                    host.Size = new System.Drawing.Size(480, 560);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw == 480 && host.At.Vw == 480 && !HasBlock(host.At, "host-box") && HasBlock(host.At, "host-media") && BlockById(host.At, "host-media").S.W == 120, "Firespider narrow media resize");
                    Check(BlockById(host.At, "host-var").S.W == 110 && BlockById(host.At, "host-var").S.Pl == 4 && BlockById(host.At, "host-var").S.Cv("--host-width") == "110px", "Firespider responsive custom properties");
                    Check(Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")) && hostResponsiveField.V == "Retained responsive host value", "Firespider responsive resize retains field state");
                    host.Size = new System.Drawing.Size(720, 560);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw > 500 && HasBlock(host.At, "host-box") && !HasBlock(host.At, "host-media") && BlockById(host.At, "host-var").S.W == 155 && Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")), "Firespider wide media restore");
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
                    Check(host.Js("fg('class-state','#0f4c81'); bg('state','#e0f2fe'); fs('host-wrap',18)") == 3 &&
                        BlockById(host.At, "class-state").S.F == "#0f4c81" && BlockById(host.At, "state").S.Bg == "#e0f2fe" && BlockById(host.At, "host-wrap").S.Fs == 18,
                        "Firespider scoped style commands");
                    Check(host.Js("vl('host-auto','Host command value'); vl('host-secret','changed-secret'); hd('class-state'); ds('host-auto')") == 4, "Firespider bounded state commands");
                    Check(FieldById(host.At, "host-auto").V == "Host command value" && FieldById(host.At, "host-auto").Ds && host.Ke == null, "Firespider value, disabled, and focus state");
                    Check(FieldById(host.At, "host-secret").V == "changed-secret" && host.Ax(ById(host.At.D, "host-secret")).V.Length == 0, "Firespider command password redaction");
                    Check(host.Ax(ById(host.At.D, "class-state")) == null && host.Js("ck('host-auto')") == 0, "Firespider hidden semantics and checked-target boundary");
                    Check(host.Js("sh('class-state'); en('host-auto'); fo('host-auto')") == 3 && host.Ke != null && host.Ke.Av("id") == "host-auto", "Firespider show, enable, and focus commands");
                    Check(host.Ax(ById(host.At.D, "class-state")) != null && !FieldById(host.At, "host-auto").Ds, "Firespider restored state command model");
                    Check(host.Js("op('host-details'); cl('host-details-summary'); tg('host-details-summary'); op('host-details')") == 4 && ById(host.At.D, "host-details").Op, "Firespider disclosure state commands");
                    Check(host.Ck(ById(host.At.D, "host-action")) == 3, "Firespider safe click commands");
                    Check(ById(host.At.D, "state").C[0].X == "Hello Firespider" && host.At.Vs["last"] == "click", "Firespider safe click state");
                    Check(Member<Label>(form, "su").Text == "Lea: Safe host click", "Firespider safe click alert status");
                    Check(host.Ck(ById(host.At.D, "host-value-update")) == 1 && ValueById(host.At, "host-output").V == "Updated host total", "Firespider output value rebuild");
                    Check(host.Ck(ById(host.At.D, "host-time-update")) == 1 && ValueById(host.At, "host-time").Dt == "2026-08-22", "Firespider time metadata rebuild");
                    var diagnostics = host.Dg();
                    Check(diagnostics != null && diagnostics.N > 0 && diagnostics.J == 23 && diagnostics.Rs.Count > 0, "Firespider Lea diagnostics");
                    var inspectorOpen = type.GetMethod("qleaInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic);
                    var inspectorSnapshot = type.GetMethod("qleaInspectorSnapshot", BindingFlags.Instance | BindingFlags.NonPublic);
                    var inspectorClear = type.GetMethod("qleaInspectorClear", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(inspectorOpen != null && inspectorSnapshot != null && inspectorClear != null, "Firespider Lea Inspector routes");
                    var inspector = inspectorOpen.Invoke(form, null) as Form;
                    Check(inspector != null && inspector.Name == "LeaInspector" && inspector.Text == "Lea Inspector", "Firespider Lea Inspector open");
                    Check((bool)inspectorSnapshot.Invoke(form, null), "Firespider Lea Inspector refresh");
                    Check(Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("Response   200") &&
                        Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("px viewport") &&
                        Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("custom") &&
                        Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("important") &&
                        Member<TextBox>(form, "qleaInspectorSummary").Text.Contains("Disk       0 entries") &&
                        Member<TextBox>(form, "qleaInspectorNodes").Text.Contains("body") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains(".hot") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains(":is") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains("nth-last") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains(" of #host-flex") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains(":has") &&
                        Member<TextBox>(form, "qleaInspectorCss").Text.Contains("grid-template") &&
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

                Console.WriteLine("Firespider Lea smoke passed: bounded inline SVG aliases, geometry, links, and graphic accessibility, bounded has relational selectors with strict lists, maximum specificity, descendant/child/adjacent and complex host relationships, focus rebuilds, retained fields, queries, Inspector, and geometry, bounded nth-child of-S filters with strict lists, maximum specificity, forward/reverse computed styles, queries, Inspector, and host geometry, bounded precompiled An+B and reverse structural selectors with computed styles, queries, Inspector, and host geometry, bounded is, where, and list-not selector functions with specificity, queries, Inspector, and host geometry, bounded CSS important cascade with G.Ic diagnostics, custom properties, inline, responsive values, Inspector counts, and retained fields, bounded inherited CSS custom properties with S.Vc/Cv and G.Vc aliases, responsive values, Inspector counts, and retained fields, bounded responsive CSS media queries with viewport aliases and retained fields, optional bounded persistent cache aliases and Inspector diagnostics, bounded content-box and border-box geometry, scoped style commands, ordered row and column flex layout with around/evenly distribution, item alignment, wrapping, growth, and shrink, semantic accessibility, protected values, rendered bounds, MSAA and UI Automation providers, value, table/grid, and read-only logical text patterns plus Access inspector, bounded autofocus and access keys, keyboard focus and activation, responsive image maps, compact time/edit metadata, output/data values and safe rebuilds, progress and meter gauges, disclosure commands, retained same-page fragments, Lea-first New Tab and explicit legacy fallback, host-controlled go requests, Inspector open/refresh/clear and legacy isolation, class mutations, diagnostics, safe clicks and variables, find and downloads, scripts and alerts, open/save/send, private cookies, close routing, sessions, source, menu, timer, start page, and navigation.");
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
                if (Directory.Exists(cache)) Directory.Delete(cache, true);
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

        private static bool HasBlock(P page, string id)
        {
            foreach (var block in page.Bs)
                if (block.El != null && block.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static System.Drawing.Rectangle BlockOuter(V view, string id)
        {
            var target = BlockById(view.At, id);
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), target) || (bool)type.GetField("Inline").GetValue(value)) continue;
                return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            return System.Drawing.Rectangle.Empty;
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
