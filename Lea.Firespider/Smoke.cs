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
            PropertyInfo lazySetting = null;
            string oldNewTab = null;
            string oldDashboard = null;
            object oldShortcuts = null;
            object oldLazy = null;
            var session = Path.Combine(Path.GetTempPath(), "lea-firespider-smoke-" + Guid.NewGuid().ToString("N") + ".txt");
            var openPage = Path.Combine(Path.GetTempPath(), "lea-firespider-open-" + Guid.NewGuid().ToString("N") + ".html");
            var mapPage = Path.Combine(Path.GetTempPath(), "lea-firespider-map-" + Guid.NewGuid().ToString("N") + ".html");
            var stickyPage = Path.Combine(Path.GetTempPath(), "lea-firespider-sticky-" + Guid.NewGuid().ToString("N") + ".html");
            var savedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-page-" + Guid.NewGuid().ToString("N") + ".html");
            var downloadedPage = Path.Combine(Path.GetTempPath(), "lea-firespider-download-" + Guid.NewGuid().ToString("N") + ".html");
            var cache = Path.Combine(Path.GetTempPath(), "lea-firespider-cache-" + Guid.NewGuid().ToString("N"));
            try
            {
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", session);
                File.WriteAllText(openPage, "<!doctype html><html><head><title>Lea Open Route</title><style>.hot{color:#b91c1c;font-weight:bold}.host-gauge{color:#176b4d;width:180px;height:16px}.host-value{color:#123f8c;width:180px}.host-wrap{width:160px}.host-flex{display:flex;width:300px;height:60px;gap:8px;justify-content:space-between;align-items:center}.host-flex p{width:70px;height:20px}</style><script type='text/lea'>v('who','Firespider')</script></head><body><div id='host-invalid-auto' autofocus>Invalid autofocus</div><input id='host-auto' value='ready' autofocus accesskey='s' required aria-label='Host search' aria-describedby='host-help'><span id='host-help'>Host field help</span><input id='host-secret' type='password' value='host-secret' aria-label='Host password'><div id='host-key' tabindex='-1' accesskey='k'>Host focus key</div><p>Open route fixture</p><p>Second route match</p><p>Third ROUTE match</p><p id='host-wrap' class='host-wrap'>A compact host paragraph wraps across visual lines for Lea.</p><section id='host-flex' class='host-flex'><p id='host-flex-a'>Flex A</p><p id='host-flex-b'>Flex B</p></section><p id='class-state'>Waiting for class state</p><p id='state'>Waiting for host click</p><button id='host-action' type='button' accesskey='b' data-lea=\"tx('state','Hello ${who}'); v('last','click'); a('Safe host click')\">Run safe action</button><details id='host-details'><summary id='host-details-summary'>Host disclosure</summary><p>Host disclosure content</p></details><progress class='host-gauge' id='host-progress' max='10' value='4'>4 of 10</progress><meter class='host-gauge' id='host-meter' min='0' max='4' value='3'>3 of 4</meter><output class='host-value' id='host-output' name='total' for='host-data'>Host total</output><data class='host-value' id='host-data' value='lea-host'>Host label</data><time class='host-value' id='host-time'>2026-08-21</time><ins class='host-value' id='host-edit' cite='go-target.html#host' datetime='2026-08-21T19:00-04:00'>Host edit</ins><table id='host-table'><thead><tr><th id='host-column' scope='col'>Value</th></tr></thead><tbody><tr><th id='host-row' scope='row'>Lea</th></tr></tbody></table><button id='host-value-update' type='button' accesskey='b x' data-lea=\"tx('host-output','Updated host total')\">Update host value</button><button id='host-time-update' type='button' data-lea=\"tx('host-time','2026-08-22')\">Update host time</button></body></html>");
                File.WriteAllText(mapPage, "<!doctype html><html><head><title>Lea Map Route</title><style>#host-map-image{width:120px;height:90px;object-fit:contain;object-position:center}</style></head><body><img id='host-map-image' width='120' height='60' usemap='#host-map' alt='Host map' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAKCAYAAAC0VX7mAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAApSURBVDhPY7ijpvafGKya/JooPGogboxNMzY8aiBujE0zNkxlA1//BwCR7MvUhfG8CgAAAABJRU5ErkJggg=='><map name='host-map'><area id='host-dead' shape='rect' coords='0,0,10,10'><area id='host-rect' shape='rect' coords='0,0,60,60' tabindex='1' accesskey='m' href='#host-target' alt='Host rectangle'><area id='host-default' shape='default' href='#host-target' alt='Host default'></map><p id='host-target'>Host map target</p></body></html>");
                File.WriteAllText(stickyPage, "<!doctype html><html><head><title>Lea Sticky Host</title><style>#lead,#tail{height:230px}.rail{position:relative;height:420px;padding:8px;overflow:hidden}.stick{position:sticky;top:14px;height:72px;padding:6px;background:#1d4ed8}.stick p{height:30px;margin:0;background:#176b4d}.fill{height:300px}</style></head><body><p id='lead'>Lead</p><section id='host-sticky-rail' class='rail'><div id='host-sticky' class='stick'><p id='host-sticky-child'>Sticky host</p></div><p id='host-sticky-fill' class='fill'>Fill</p></section><p id='tail'>Tail</p></body></html>");
                var openMarkup = File.ReadAllText(openPage)
                    .Replace(".host-wrap{width:160px}", ".host-wrap{width:160px}.host-box{box-sizing:border-box;width:180px;height:44px;padding:6px;border:2px solid #176b4d;border-radius:18px 7px/12px 5px;box-shadow:6px 4px 3px 1px rgba(15,76,129,.5),inset 4px 0 0 #a13b18;background-image:linear-gradient(to right,#dff6e9 0%,#bfdbfe 100%)}")
                    .Replace("</style>", ".host-bitmap{width:140px;height:50px;background:#fff url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYxDP9v0PAAM7Ac+IRop4AAAAAElFTkSuQmCC') no-repeat right bottom/24px 18px;border-radius:12px}</style>")
                    .Replace("<section id='host-flex'", "<p id='host-bitmap' class='host-bitmap'>Bitmap host</p><section id='host-flex'")
                    .Replace("</style>", ".host-ratio{width:160px;aspect-ratio:2/1;background:#dff6e9}</style>")
                    .Replace("<p id='host-bitmap'", "<p id='host-ratio' class='host-ratio'>Ratio host</p><p id='host-bitmap'")
                    .Replace("</style>", ".host-outline{width:150px;height:44px;outline:4px solid #a13b18;outline-offset:3px;background:#dff6e9}</style>")
                    .Replace("<p id='host-ratio'", "<p id='host-outline' class='host-outline'>Outline host</p><p id='host-ratio'")
                    .Replace("</style>", ".host-pointer-live,.host-pointer-dead{display:block;width:150px;height:32px}.host-pointer-live{cursor:pointer}.host-pointer-dead{cursor:not-allowed;pointer-events:none}</style>")
                    .Replace("<p id='host-outline'", "<a id='host-pointer-live' class='host-pointer-live' href='#state'>Live pointer host</a><a id='host-pointer-dead' class='host-pointer-dead' href='#state'>Disabled pointer host</a><p id='host-outline'")
                    .Replace("</style>", ".host-transform{width:190px;text-transform:uppercase}.host-transform span{text-transform:lowercase}.host-transform-field{width:190px;height:32px;text-transform:lowercase}.host-transform-responsive{text-transform:uppercase}@media screen and (max-width:500px){.host-transform-responsive{text-transform:lowercase}}</style>")
                    .Replace("</body>", "<p id='host-transform' class='host-transform'>Host <span>MiXeD TEXT</span></p><input id='host-transform-field' class='host-transform-field' name='host-transform-field' value='Host Field MiXeD'><p id='host-transform-responsive' class='host-transform-responsive'>Responsive Host MiXeD</p></body>")
                    .Replace("</style>", ".host-spacing-normal,.host-spacing-wide{width:240px;text-align:left}.host-spacing-normal{letter-spacing:0;word-spacing:0}.host-spacing-wide{letter-spacing:4px;word-spacing:9px}.host-spacing-field{width:220px;height:32px;letter-spacing:2px;word-spacing:6px}.host-spacing-responsive{letter-spacing:5px;word-spacing:10px}@media screen and (max-width:500px){.host-spacing-responsive{letter-spacing:-1px;word-spacing:0}}</style>")
                    .Replace("</body>", "<p id='host-spacing-normal' class='host-spacing-normal'>Host spacing words</p><p id='host-spacing-wide' class='host-spacing-wide'>Host spacing words</p><input id='host-spacing-field' class='host-spacing-field' name='host-spacing-field' value='Host raw words'><p id='host-spacing-responsive' class='host-spacing-responsive'>Responsive host spacing</p></body>")
                    .Replace("</style>", ".host-overflow{width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.host-overflow-responsive{text-overflow:clip}@media screen and (max-width:500px){.host-overflow-responsive{text-overflow:ellipsis}}</style>")
                    .Replace("</body>", "<p id='host-overflow' class='host-overflow'>Host overflow keeps this entire authored sentence</p><p id='host-overflow-responsive' class='host-overflow host-overflow-responsive'>Responsive host overflow keeps its full text</p></body>")
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
                    .Replace("</style>", ".host-calculated{width:calc(100% - 120px);max-width:calc(75% + 5px);background:#e8f3ff}</style>")
                    .Replace("</style>", ".host-height-shell{height:320px}.host-height{height:calc(100% - 80px);max-height:calc(75% + 5px);background:#dff6e9}</style>")
                    .Replace("</style>", ".host-stroke{fill:none;stroke:#0f766e;stroke-width:3;stroke-linecap:square;stroke-linejoin:bevel;stroke-miterlimit:7;stroke-dasharray:4 2 1;stroke-dashoffset:2}</style>")
                    .Replace("</section><p id='class-state'>", "</section><section id='host-has'><span class='host-direct'>Direct host child</span><div class='host-body'><span class='host-ready' data-host='ready'>Ready host descendant</span></div><input id='host-has-input' value='Relational host field'></section><section id='host-grid' class='host-grid'><p id='host-grid-a'>Grid A</p><p id='host-grid-b'>Grid B</p><p id='host-grid-c'>Grid C</p><p id='host-grid-span'>Grid span</p></section><section id='host-grid-rows' class='host-grid-rows'><p id='host-grid-row-a'>Grid row A</p><p id='host-grid-row-b'>Grid row B</p><p id='host-grid-row-c'>Grid row C</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-areas' class='host-grid-areas'><p id='host-grid-area-auto'>Area auto</p><p id='host-grid-area-top'>Area top</p><p id='host-grid-area-left'>Area left</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-lines' class='host-grid-lines'><p id='host-grid-line-auto'>Line auto</p><p id='host-grid-line-top'>Line top</p><p id='host-grid-line-left'>Line left</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-flow' class='host-grid-flow'><p id='host-grid-flow-a'>Flow A</p><p id='host-grid-flow-b'>Flow B</p><p id='host-grid-flow-c'>Flow C</p></section><section id='host-grid-column' class='host-grid-column'><p id='host-grid-column-a'>Column A</p><p id='host-grid-column-b'>Column B</p><p id='host-grid-column-c'>Column C</p><p id='host-grid-column-d'>Column D</p><p id='host-grid-column-e'>Column E</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-auto-tracks' class='host-grid-auto-tracks'><p id='host-grid-auto-a'>Auto track A</p><p id='host-grid-auto-b'>Auto track B</p><p id='host-grid-auto-c'>Auto track C</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-shorthand' class='host-grid-shorthand'><p id='host-grid-short-a'>Short A</p><p id='host-grid-short-b'>Short B</p><p id='host-grid-short-c'>Short C</p></section><section id='host-grid-short-area' class='host-grid-short-area'><p id='host-grid-short-top'>Short top</p><p id='host-grid-short-left'>Short left</p><p id='host-grid-short-right'>Short right</p></section><section id='host-grid-short-column' class='host-grid-short-column'><p id='host-grid-short-column-a'>Column A</p><p id='host-grid-short-column-b'>Column B</p><p id='host-grid-short-column-c'>Column C</p><p id='host-grid-short-column-d'>Column D</p><p id='host-grid-short-column-e'>Column E</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-align' class='host-grid-align'><p id='host-grid-align-a'>Align A</p><p id='host-grid-align-b'>Align B</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-grid-content' class='host-grid-content'><p id='host-grid-content-a'>Content A</p><p id='host-grid-content-b'>Content B</p><p id='host-grid-content-c'>Content C</p><p id='host-grid-content-d'>Content D</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-calculated' class='host-calculated' style='color:#123f8c'>Calculated host width</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-height-shell' class='host-height-shell'><p id='host-height' class='host-height' style='color:#123f8c'>Calculated host height</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-var' class='host-var' style='--host-color:#a13b18;color:#a13b18;width:199px'>Important property host</p><p id='host-media' class='host-media'>Responsive host</p><p id='host-box' class='host-box'>Border-box host</p><section id='host-flex-column' class='host-flex-column'><p id='host-flex-column-a'>Column A</p><p id='host-flex-column-b'>Column B</p></section><svg id='host-svg' width='160' height='80' viewBox='0 0 80 40'><title>Firespider host vector</title><rect x='2' y='2' width='20' height='16' fill='#d7263d'></rect><circle cx='34' cy='10' r='8' fill='#10b981'></circle><path d='M46 2 h28 v16 H46 z' fill='#f59e0b'></path><a href='#state'><rect x='12' y='24' width='56' height='12' fill='#0369a1'></rect></a></svg><p id='class-state'>")
                    .Replace("</svg><p id='class-state'>", "<path d='M2 32 C8 20 14 20 20 32 S32 44 38 32' fill='none' stroke='#7c3aed' stroke-width='2'></path><g transform='translate(58 26)'><g transform='scale(1.5)'><rect x='0' y='0' width='10' height='6' fill='#db2777'></rect></g></g><defs><linearGradient id='host-linear'><stop offset='0' stop-color='#dc2626'></stop><stop offset='1' stop-color='#2563eb'></stop></linearGradient><radialGradient id='host-radial'><stop offset='0' stop-color='#f8fafc'></stop><stop offset='1' stop-color='#7c3aed'></stop></radialGradient><g id='host-reuse'><rect x='0' y='0' width='10' height='8'></rect><circle cx='7' cy='4' r='2' fill='#f8fafc'></circle></g><clipPath id='host-half' clipPathUnits='objectBoundingBox' clip-rule='evenodd'><rect x='0' y='0' width='.5' height='1'></rect></clipPath></defs><rect x='40' y='24' width='14' height='10' fill='url(#host-linear)'></rect><circle cx='69' cy='10' r='7' fill='url(#host-radial)'></circle><use href='#host-reuse' x='26' y='27' fill='#0d9488'></use><rect x='46' y='27' width='10' height='8' fill='#f59e0b' clip-path='url(#host-half)'></rect><path class='host-stroke' d='M2 20 H22' fill-rule='evenodd'></path></svg><p id='class-state'>");
                File.WriteAllText(openPage, openMarkup);
                var root = Path.GetFullPath(args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory);
                var assembly = Assembly.LoadFrom(Path.Combine(root, "Firespider Internet 19.exe"));
                var type = assembly.GetType("Firespider_Internet.j", true);
                var settingsType = assembly.GetType("Firespider_Internet.My.MySettings", true);
                settings = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
                newTabSetting = settingsType.GetProperty("nf", BindingFlags.Instance | BindingFlags.Public);
                dashboardSetting = settingsType.GetProperty("dashboard", BindingFlags.Instance | BindingFlags.Public);
                shortcutSetting = settingsType.GetProperty("sc", BindingFlags.Instance | BindingFlags.Public);
                lazySetting = settingsType.GetProperty("lazyRestoreMode", BindingFlags.Instance | BindingFlags.Public);
                oldNewTab = (string)newTabSetting.GetValue(settings, null);
                oldDashboard = (string)dashboardSetting.GetValue(settings, null);
                oldShortcuts = shortcutSetting.GetValue(settings, null);
                oldLazy = lazySetting.GetValue(settings, null);
                newTabSetting.SetValue(settings, new Uri(Path.Combine(root, "Lea", "start.html")).AbsoluteUri, null);
                dashboardSetting.SetValue(settings, "", null);
                shortcutSetting.SetValue(settings, new System.Collections.Specialized.StringCollection(), null);
                lazySetting.SetValue(settings, "off", null);
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
                    StartupEvents(form, session);
                    if (args.Length > 1 && args[1] == "--startup")
                    {
                        Console.WriteLine("Firespider startup regression smoke passed.");
                        return 0;
                    }
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
                    Check(host.Pv, "Firespider existing private view is not downgraded");
                    host.Pv = false;
                    Check(!host.Pv, "explicit Lea cookie-mode reset remains available to embedders");
                    tick.Invoke(timer, new object[] { EventArgs.Empty });
                    var start = Path.Combine(root, "Lea", "start.html");
                    var opened = (bool)type.GetMethod("qleaOpenPath", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { openPage });
                    WaitFor(() => !host.Busy && host.At != null && host.T == "Lea Open Route", "Firespider Open File routed to Lea");
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
                    host.Size = new System.Drawing.Size(720, 720);
                    Application.DoEvents();
                    host.Sc(-Int32.MaxValue);
                    PaintView(host);
                    var hostPointerLive = BlockById(host.At, "host-pointer-live");
                    var hostPointerDead = BlockById(host.At, "host-pointer-dead");
                    var hostPointerLiveRect = BlockOuter(host, "host-pointer-live");
                    var hostPointerDeadRect = BlockOuter(host, "host-pointer-dead");
                    Check(hostPointerLive.S.Cu == "p" && hostPointerLive.S.Pe && hostPointerDead.S.Cu == "n" && !hostPointerDead.S.Pe,
                        "Firespider compact cursor and pointer-event aliases");
                    Check(!ElementHit(host, "host-pointer-live").IsEmpty && ElementHit(host, "host-pointer-dead").IsEmpty,
                        "Firespider pointer-event retained hit gate");
                    MoveView(host, hostPointerLiveRect);
                    Check(Object.ReferenceEquals(host.Cursor, Cursors.Hand), "Firespider pointer cursor mapping");
                    MoveView(host, hostPointerDeadRect);
                    Check(Object.ReferenceEquals(host.Cursor, Cursors.Default), "Firespider pointer-disabled cursor fallthrough");
                    PaintView(host);
                    var hostTransform = BlockById(host.At, "host-transform");
                    var hostTransformField = FieldById(host.At, "host-transform-field");
                    Check(hostTransform.S.Tt == "u" && hostTransform.X == "HOST mixed text" && hostTransform.Is.Count == 2 && hostTransform.Is[1].S.Tt == "l" &&
                        hostTransformField.S.Tt == "l" && hostTransformField.V == "Host Field MiXeD" && E.Tx(hostTransformField.V, hostTransformField.S) == "host field mixed" &&
                        BlockById(host.At, "host-transform-responsive").X == "RESPONSIVE HOST MIXED",
                        "Firespider bounded text transform aliases, inline runs, controls, and responsive model");
                    hostTransformField.V = "Retained Host MiXeD";
                    var hostSpacingNormal = BlockById(host.At, "host-spacing-normal");
                    var hostSpacingWide = BlockById(host.At, "host-spacing-wide");
                    var hostSpacingField = FieldById(host.At, "host-spacing-field");
                    Check(hostSpacingNormal.S.Ls == 0 && hostSpacingNormal.S.Wg == 0 && hostSpacingWide.S.Ls == 4 && hostSpacingWide.S.Wg == 9 &&
                        hostSpacingField.S.Ls == 2 && hostSpacingField.S.Wg == 6 && hostSpacingField.V == "Host raw words" &&
                        InlineBounds(host, "host-spacing-wide").Width > InlineBounds(host, "host-spacing-normal").Width,
                        "Firespider bounded text spacing aliases, retained metrics, and raw control state");
                    hostSpacingField.V = "Retained host raw words";
                    Check(BlockById(host.At, "host-overflow").S.To == "e" && EllipsisCount(host, "host-overflow") == 1 &&
                        EllipsisCount(host, "host-overflow-responsive") == 0 && host.At.X.Contains("Host overflow keeps this entire authored sentence"),
                        "Firespider text overflow alias, painted marker, and authored text");
                    var hostOutline = BlockById(host.At, "host-outline");
                    Check(hostOutline.S.Ow == 4 && hostOutline.S.Os == "s" && hostOutline.S.Oc == "#a13b18" && hostOutline.S.Oo == 3 &&
                        BlockOuter(host, "host-outline").Size == new System.Drawing.Size(150, 44),
                        "Firespider bounded outline aliases and geometry");
                    var hostRatio = BlockById(host.At, "host-ratio");
                    Check(hostRatio.S.Rw == 2D && hostRatio.S.Rh == 1D && !hostRatio.S.Ra &&
                        BlockOuter(host, "host-ratio").Size == new System.Drawing.Size(160, 80),
                        "Firespider bounded aspect ratio aliases and geometry");
                    var hostBitmap = BlockById(host.At, "host-bitmap");
                    Check(hostBitmap.S.Bi != null && hostBitmap.S.Bi.U != null && hostBitmap.S.Bi.U.Scheme == "data" &&
                        hostBitmap.S.Bi.R == "n" && hostBitmap.S.Bi.X == 100 && hostBitmap.S.Bi.Y == 100 &&
                        hostBitmap.S.Bi.W == 24 && hostBitmap.S.Bi.H == 18,
                        "Firespider bounded bitmap background aliases");
                    using (var hostPaint = RenderView(host))
                    {
                        var outlineRect = BlockOuter(host, "host-outline");
                        outlineRect.Offset(0, -host.Sy);
                        var ratioRect = BlockOuter(host, "host-ratio");
                        ratioRect.Offset(0, -host.Sy);
                        var bitmapRect = BlockOuter(host, "host-bitmap");
                        bitmapRect.Offset(0, -host.Sy);
                        Check(ColorDistance(hostPaint.GetPixel(outlineRect.Left - 5, outlineRect.Top + outlineRect.Height / 2), System.Drawing.Color.FromArgb(161, 59, 24)) < 16,
                            "Firespider bounded outline paint");
                        Check(ColorDistance(hostPaint.GetPixel(ratioRect.Right - 5, ratioRect.Bottom - 5), System.Drawing.Color.FromArgb(223, 246, 233)) < 8,
                            "Firespider bounded aspect ratio paint");
                        Check(bitmapRect.Width == 140 && bitmapRect.Height == 50 &&
                            ColorDistance(hostPaint.GetPixel(bitmapRect.Right - 5, bitmapRect.Bottom - 5), System.Drawing.Color.FromArgb(23, 107, 77)) < 8 &&
                            ColorDistance(hostPaint.GetPixel(bitmapRect.Left + 5, bitmapRect.Bottom - 5), System.Drawing.Color.White) < 8,
                            "Firespider bounded bitmap background paint");
                    }
                    var hostBox = BlockById(host.At, "host-box");
                    var automationHostBox = automationRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "host-box"));
                    Check(automationHostBox != null, "Firespider border-box automation element");
                    ((TextPattern)automationTextPattern).RangeFromChild(automationHostBox).ScrollIntoView(true);
                    PaintView(host);
                    var hostBoxBounds = host.Ab(host.Ax(ById(host.At.D, "host-box")));
                    Check(hostBox.S.Bz == "b" && hostBox.S.W == 180 && hostBox.S.H == 44 && hostBox.S.R0 == 18 && hostBox.S.R1 == 7 && hostBox.S.Q0 == 12 && hostBox.S.Q1 == 5 &&
                        hostBox.S.Sw.Count == 2 && hostBox.S.Sw[0].X == 6 && hostBox.S.Sw[0].Y == 4 && hostBox.S.Sw[0].B == 3 && hostBox.S.Sw[0].R == 1 && !hostBox.S.Sw[0].I && hostBox.S.Sw[1].I && hostBox.S.Sw[1].X == 4 &&
                        hostBox.S.Gi != null && hostBox.S.Gi.K == "l" && hostBox.S.Gi.A == 90 && hostBox.S.Gi.Ss.Count == 2 && hostBox.S.Gi.Ss[0].C == "#dff6e9" && hostBox.S.Gi.Ss[1].P == 100 &&
                        hostBoxBounds.Width > 0 && hostBoxBounds.Width <= hostBox.S.W && hostBoxBounds.Height == 44,
                        "Firespider bounded border-box/radius model and visible geometry: " + hostBox.S.Bz + "/" + hostBox.S.W + "x" + hostBox.S.H + " -> " + hostBoxBounds.Width + "x" + hostBoxBounds.Height);
                    var hostCalculated = BlockById(host.At, "host-calculated");
                    var hostCalculatedWide = BlockOuter(host, "host-calculated").Width;
                    Check(hostCalculated.S.Wp == 100 && hostCalculated.S.Wo == -120 && hostCalculated.S.Wxp == 75 && hostCalculated.S.Wxo == 5 && hostCalculatedWide > 0,
                        "Firespider calculated width model and geometry");
                    var hostHeight = BlockById(host.At, "host-height");
                    var hostHeightTall = BlockOuter(host, "host-height").Height;
                    Check(hostHeight.S.Hp == 100 && hostHeight.S.Ho == -80 && hostHeight.S.Hxp == 75 && hostHeight.S.Hxo == 5 && hostHeightTall == 240,
                        "Firespider calculated height model and geometry");
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
                    Check(hostSvg.Vg != null && hostSvg.Vg.Vb && hostSvg.Vg.Ds.Count == 2 && hostSvg.Vg.Cs.Count == 1 && hostSvg.Vg.Ss.Count == 12 && hostSvg.Vg.Ss[2].D.Count == 5 && hostSvg.Vg.Ss[3].U.Fragment == "#state" &&
                        hostSvg.Vg.Ss[4].D.Count == 3 && hostSvg.Vg.Ss[4].D[2].K == 'C' && hostSvg.Vg.Ss[5].M[0] == 1.5D && hostSvg.Vg.Ss[5].M[4] == 58D &&
                        hostSvg.Vg.Ds[0].K == "l" && hostSvg.Vg.Ds[0].Ss.Count == 2 && hostSvg.Vg.Ds[1].K == "r" && hostSvg.Vg.Ss[6].Fd == hostSvg.Vg.Ds[0] && hostSvg.Vg.Ss[7].Fd == hostSvg.Vg.Ds[1] &&
                        hostSvg.Vg.Ss[8].K == "rect" && hostSvg.Vg.Ss[8].M[4] == 26D && hostSvg.Vg.Ss[9].K == "circle" && hostSvg.Vg.Cs[0].U == "b" && hostSvg.Vg.Cs[0].Ss[0].Fr == "e" && hostSvg.Vg.Ss[10].Cl && hostSvg.Vg.Ss[10].Cp == hostSvg.Vg.Cs[0] &&
                        hostSvg.Vg.Ss[11].Fr == "e" && hostSvg.Vg.Ss[11].S == "#0f766e" && hostSvg.Vg.Ss[11].W == 3D && hostSvg.Vg.Ss[11].Lc == "s" && hostSvg.Vg.Ss[11].Lj == "b" && hostSvg.Vg.Ss[11].Ml == 7D && hostSvg.Vg.Ss[11].Do == 2D && hostSvg.Vg.Ss[11].Da.Count == 6,
                        "Firespider bounded inline SVG aliases");
                    Check(BlockOuter(host, "host-svg").Size == new System.Drawing.Size(160, 80) && hostSvgAccess != null && hostSvgAccess.K == "img" && hostSvgAccess.N == "Firespider host vector",
                        "Firespider bounded inline SVG geometry and accessibility");
                    host.Size = new System.Drawing.Size(480, 560);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw == 480 && host.At.Vw == 480 && !HasBlock(host.At, "host-box") && HasBlock(host.At, "host-media") && BlockById(host.At, "host-media").S.W == 120, "Firespider narrow media resize");
                    Check(BlockById(host.At, "host-var").S.W == 110 && BlockById(host.At, "host-var").S.Pl == 4 && BlockById(host.At, "host-var").S.Cv("--host-width") == "110px", "Firespider responsive custom properties");
                    Check(Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")) && hostResponsiveField.V == "Retained responsive host value", "Firespider responsive resize retains field state");
                    Check(BlockById(host.At, "host-transform-responsive").S.Tt == "l" && BlockById(host.At, "host-transform-responsive").X == "responsive host mixed" &&
                        Object.ReferenceEquals(hostTransformField, FieldById(host.At, "host-transform-field")) && hostTransformField.V == "Retained Host MiXeD",
                        "Firespider responsive text transform and raw field retention");
                    Check(BlockById(host.At, "host-spacing-responsive").S.Ls == -1 && BlockById(host.At, "host-spacing-responsive").S.Wg == 0 &&
                        Object.ReferenceEquals(hostSpacingField, FieldById(host.At, "host-spacing-field")) && hostSpacingField.V == "Retained host raw words",
                        "Firespider responsive text spacing and raw field retention");
                    Check(BlockById(host.At, "host-overflow-responsive").S.To == "e" && EllipsisCount(host, "host-overflow-responsive") == 1,
                        "Firespider narrow text overflow marker");
                    host.Size = new System.Drawing.Size(720, 560);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw > 500 && HasBlock(host.At, "host-box") && !HasBlock(host.At, "host-media") && BlockById(host.At, "host-var").S.W == 155 && Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")), "Firespider wide media restore");
                    Check(BlockById(host.At, "host-transform-responsive").S.Tt == "u" && BlockById(host.At, "host-transform-responsive").X == "RESPONSIVE HOST MIXED" &&
                        Object.ReferenceEquals(hostTransformField, FieldById(host.At, "host-transform-field")), "Firespider wide text transform restore");
                    Check(BlockById(host.At, "host-spacing-responsive").S.Ls == 5 && BlockById(host.At, "host-spacing-responsive").S.Wg == 10 &&
                        Object.ReferenceEquals(hostSpacingField, FieldById(host.At, "host-spacing-field")), "Firespider wide text spacing restore");
                    Check(BlockById(host.At, "host-overflow-responsive").S.To == "c" && EllipsisCount(host, "host-overflow-responsive") == 0,
                        "Firespider wide text overflow restore");
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
                    Check(host.Js("document.getElementById('class-state').textContent='DOM host'; document.getElementById('class-state').style.color='#166534'; document.getElementById('class-state').classList.remove('hot')") == 3 &&
                        ById(host.At.D, "class-state").C[0].X == "DOM host" && BlockById(host.At, "class-state").S.F == "#166534" && host.Q1("#class-state.hot") == null,
                        "Firespider literal DOM text, style, and class bridge");
                    Check(host.Js("dp('class-state','none')") == 1 && !HasBlock(host.At, "class-state"), "Firespider compact display command");
                    Check(host.Js("document.getElementById('class-state').style.display='block'") == 1 && HasBlock(host.At, "class-state") && BlockById(host.At, "class-state").S.D == "block",
                        "Firespider literal DOM display bridge");
                    Check(host.Js("dp('class-state','contents'); document.getElementById('class-state').style.display='ruby'") == 0 && BlockById(host.At, "class-state").S.D == "block",
                        "Firespider display allow-list boundary");
                    Check(host.Js("wd('host-calculated','50%')") == 1 && BlockById(host.At, "host-calculated").S.Wp == 50, "Firespider compact width command");
                    PaintView(host);
                    Check(BlockOuter(host, "host-calculated").Width < hostCalculatedWide, "Firespider compact percentage width reflow");
                    Check(host.Js("document.getElementById('host-calculated').style.width=''") == 1 && BlockById(host.At, "host-calculated").S.Wp == 100 && BlockById(host.At, "host-calculated").S.F == "#123f8c",
                        "Firespider literal DOM width reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-calculated").Width == hostCalculatedWide, "Firespider authored calculated width restore");
                    Check(host.Js("wd('host-calculated','min(100%,400px)'); document.getElementById('host-calculated').style.width='101%'") == 0 && BlockById(host.At, "host-calculated").S.Wp == 100,
                        "Firespider calculated width boundary");
                    Check(host.Js("ht('host-height','50%')") == 1 && BlockById(host.At, "host-height").S.Hp == 50, "Firespider compact height command");
                    PaintView(host);
                    Check(BlockOuter(host, "host-height").Height == 160, "Firespider compact percentage height reflow");
                    Check(host.Js("document.getElementById('host-height').style.height=''") == 1 && BlockById(host.At, "host-height").S.Hp == 100 && BlockById(host.At, "host-height").S.F == "#123f8c",
                        "Firespider literal DOM height reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-height").Height == hostHeightTall, "Firespider authored calculated height restore");
                    Check(host.Js("ht('host-height','min(100%,400px)'); document.getElementById('host-height').style.height='101%'") == 0 && BlockById(host.At, "host-height").S.Hp == 100,
                        "Firespider calculated height boundary");
                    Check(host.Js("vl('host-auto','Host command value'); vl('host-secret','changed-secret'); hd('class-state'); ds('host-auto')") == 4, "Firespider bounded state commands");
                    Check(FieldById(host.At, "host-auto").V == "Host command value" && FieldById(host.At, "host-auto").Ds && host.Ke == null, "Firespider value, disabled, and focus state");
                    Check(FieldById(host.At, "host-secret").V == "changed-secret" && host.Ax(ById(host.At.D, "host-secret")).V.Length == 0, "Firespider command password redaction");
                    Check(host.Ax(ById(host.At.D, "class-state")) == null && host.Js("ck('host-auto')") == 0, "Firespider hidden semantics and checked-target boundary");
                    Check(host.Js("sh('class-state'); en('host-auto'); fo('host-auto')") == 3 && host.Ke != null && host.Ke.Av("id") == "host-auto", "Firespider show, enable, and focus commands");
                    Check(host.Ax(ById(host.At.D, "class-state")) != null && !FieldById(host.At, "host-auto").Ds, "Firespider restored state command model");
                    Check(host.Js("document.getElementById('host-auto').value='DOM host value'; document.getElementById('host-auto').disabled=true; document.getElementById('host-auto').disabled=false; document.getElementById('host-auto').focus(); document.getElementById('host-details').open=true; document.getElementById('host-details').open=false") == 6 &&
                        FieldById(host.At, "host-auto").V == "DOM host value" && !FieldById(host.At, "host-auto").Ds && host.Ke != null && host.Ke.Av("id") == "host-auto" && !ById(host.At.D, "host-details").Op,
                        "Firespider literal DOM field, focus, and disclosure bridge");
                    Check(host.Js("document.getElementById('class-state').innerHTML='<b>bad</b>'") == 0 && ById(host.At.D, "class-state").C[0].X == "DOM host", "Firespider rejects arbitrary DOM APIs");
                    Check(host.Js("op('host-details'); cl('host-details-summary'); tg('host-details-summary'); op('host-details')") == 4 && ById(host.At.D, "host-details").Op, "Firespider disclosure state commands");
                    Check(host.Ck(ById(host.At.D, "host-action")) == 3, "Firespider safe click commands");
                    Check(ById(host.At.D, "state").C[0].X == "Hello Firespider" && host.At.Vs["last"] == "click", "Firespider safe click state");
                    Check(Member<Label>(form, "su").Text == "Lea: Safe host click", "Firespider safe click alert status");
                    Check(host.Ck(ById(host.At.D, "host-value-update")) == 1 && ValueById(host.At, "host-output").V == "Updated host total", "Firespider output value rebuild");
                    Check(host.Ck(ById(host.At.D, "host-time-update")) == 1 && ValueById(host.At, "host-time").Dt == "2026-08-22", "Firespider time metadata rebuild");
                    var diagnostics = host.Dg();
                    Check(diagnostics != null && diagnostics.N > 0 && diagnostics.J == 38 && diagnostics.Rs.Count > 0, "Firespider Lea diagnostics");
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
                    Check(host.Js("window.location.assign('" + new Uri(start).AbsoluteUri + "')") == 1, "Firespider familiar location request accepted");
                    WaitFor(() => !host.Busy && host.At != null && host.T == "Lea in Firespider", "Firespider go request routed by host");
                    Check(goRequests == 1 && host.Gu == new Uri(start), "Firespider go request event and URL");
                    var address = Member<ToolStripComboBox>(form, "c");
                    address.Text = new Uri(start).AbsoluteUri;
                    var addressKey = new KeyEventArgs(Keys.Enter);
                    type.GetMethod("qleaAddressKeyDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { address, addressKey });
                    WaitFor(() => !host.Busy && host.At != null && host.T == "Lea in Firespider", "address Enter routed to Lea");
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

                    host.Dock = DockStyle.None;
                    host.Size = new System.Drawing.Size(720, 300);
                    host.Go(stickyPage);
                    PaintView(host);
                    var hostStickyBefore = BlockOuter(host, "host-sticky");
                    var hostStickyChildBefore = BlockOuter(host, "host-sticky-child");
                    host.Sc(360);
                    PaintView(host);
                    var hostSticky = BlockOuter(host, "host-sticky");
                    var hostStickyChild = BlockOuter(host, "host-sticky-child");
                    var hostStickyAccess = host.Ab(host.Ax(ById(host.At.D, "host-sticky-child")));
                    Check(BlockById(host.At, "host-sticky").S.Pn == "k" && hostSticky.Top == host.Sy + 14 &&
                        hostStickyChild.Top - hostSticky.Top == hostStickyChildBefore.Top - hostStickyBefore.Top &&
                        !hostStickyAccess.IsEmpty && hostStickyAccess.Top == hostStickyChild.Top - host.Sy,
                        "Firespider bounded sticky alias and shared geometry");

                    host.Go(mapPage);
                    Check(host.At != null && host.T == "Lea Map Route" && host.At.Mps.Count == 1, "Firespider image map route");
                    var hostMapImage = BlockById(host.At, "host-map-image");
                    Check(hostMapImage.Mp != null && hostMapImage.Mp.As.Count == 3 && hostMapImage.Mw == 120 && hostMapImage.Mh == 60 &&
                        hostMapImage.S.Of == "c" && hostMapImage.S.Ox == 50 && hostMapImage.S.Oy == 50 && hostMapImage.S.Oxp && hostMapImage.S.Oyp,
                        "Firespider compact fitted image-map model");
                    Check(hostMapImage.Mp.As[1].K == "r" && hostMapImage.Mp.As[1].U.Fragment == "#host-target", "Firespider compact area model");
                    form.ClientSize = new System.Drawing.Size(800, 600);
                    form.PerformLayout();
                    host.Dock = DockStyle.None;
                    host.Size = new System.Drawing.Size(360, 240);
                    host.CreateControl();
                    host.PerformLayout();
                    PaintView(host);
                    var hostMapRect = BlockOuter(host, "host-map-image");
                    Check(hostMapRect.Size == new System.Drawing.Size(120, 90), "Firespider fitted image-map layout");
                    using (var mapPaint = RenderView(host))
                    {
                        var paintedMap = hostMapRect;
                        paintedMap.Offset(0, -host.Sy);
                        Check(ColorDistance(mapPaint.GetPixel(paintedMap.Left + 10, paintedMap.Top + 5), System.Drawing.Color.White) < 8 &&
                            ColorDistance(mapPaint.GetPixel(paintedMap.Left + 20, paintedMap.Top + 45), System.Drawing.Color.FromArgb(220, 38, 38)) < 20 &&
                            ColorDistance(mapPaint.GetPixel(paintedMap.Right - 20, paintedMap.Top + 45), System.Drawing.Color.FromArgb(37, 99, 235)) < 20,
                            "Firespider fitted image paint");
                    }
                    var hostMapHits = MapHitCount(host, false);
                    var hostMapLinks = MapHitCount(host, true);
                    Check(hostMapHits == 3 && hostMapLinks == 2, "Firespider image map renderer hits: " + hostMapHits + "/" + hostMapLinks + " at " + host.Width + "x" + host.Height + ", boxes " + host.Bc + ", scroll " + host.Sy);
                    Check(host.Ti(hostMapImage.Mp.As[1].El) == 1 && host.Kn == 2 && host.Ki == 0, "Firespider compact keyboard aliases");
                    Check(host.Kf() == 1 && host.Ke.Av("id") == "host-rect" && host.Kb == new System.Drawing.Rectangle(hostMapRect.Left, hostMapRect.Top + 15, 60, 60) &&
                        host.Ab(host.Ax(hostMapImage.Mp.As[1].El)) == host.Kb,
                        "Firespider fitted image-map keyboard and accessibility geometry");
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
                    type.GetMethod("qleaRestoreSession", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(restoredForm, null);
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

                Console.WriteLine("Firespider CSS interaction and paint smoke passed: compact text-overflow, text-spacing, text-transform, pointer, outline, radius, shadow, gradient, bitmap-background, object-fit, and aspect-ratio aliases reached the compiled host.");
                Console.WriteLine("Firespider calculated height smoke passed: compact ht, literal style.height, definite parent resolution, shared host geometry, reset, and strict bounds.");

                Console.WriteLine("Firespider Lea smoke passed: bounded sticky positioning with compact k and shared host geometry, bounded percentage/calc widths with compact wd and literal DOM reflow, bounded display alias and literal location requests, literal bounded DOM translation, inherited cursor and pointer-event aliases with host hit gating and fallthrough, bounded CSS outlines with compact aliases and exact non-layout host paint, bounded CSS aspect ratios with compact aliases and exact host geometry/paint, bounded CSS object-fit and object-position with compact aliases, exact fitted paint, and image-map keyboard/accessibility geometry, bounded CSS bitmap backgrounds with compact aliases and exact host paint, bounded inline SVG aliases, winding, stroke styles, geometry, links, and graphic accessibility, bounded has relational selectors with strict lists, maximum specificity, descendant/child/adjacent and complex host relationships, focus rebuilds, retained fields, queries, Inspector, and geometry, bounded nth-child of-S filters with strict lists, maximum specificity, forward/reverse computed styles, queries, Inspector, and host geometry, bounded precompiled An+B and reverse structural selectors with computed styles, queries, Inspector, and host geometry, bounded is, where, and list-not selector functions with specificity, queries, Inspector, and host geometry, bounded CSS important cascade with G.Ic diagnostics, custom properties, inline, responsive values, Inspector counts, and retained fields, bounded inherited CSS custom properties with S.Vc/Cv and G.Vc aliases, responsive values, Inspector counts, and retained fields, bounded responsive CSS media queries with viewport aliases and retained fields, optional bounded persistent cache aliases and Inspector diagnostics, bounded content-box and border-box geometry, scoped style commands, ordered row and column flex layout with around/evenly distribution, item alignment, wrapping, growth, and shrink, semantic accessibility, protected values, rendered bounds, MSAA and UI Automation providers, value, table/grid, and read-only logical text patterns plus Access inspector, bounded autofocus and access keys, keyboard focus and activation, responsive image maps, compact time/edit metadata, output/data values and safe rebuilds, progress and meter gauges, disclosure commands, retained same-page fragments, Lea-first New Tab and explicit legacy fallback, host-controlled go requests, Inspector open/refresh/clear and legacy isolation, class mutations, diagnostics, safe clicks and variables, find and downloads, scripts and alerts, open/save/send, private cookies, close routing, sessions, source, menu, timer, start page, and navigation.");
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
                if (settings != null && lazySetting != null) lazySetting.SetValue(settings, oldLazy, null);
                Environment.SetEnvironmentVariable("LEA_FIRESPIDER_SESSION", oldSession);
                if (File.Exists(session)) File.Delete(session);
                if (File.Exists(openPage)) File.Delete(openPage);
                if (File.Exists(mapPage)) File.Delete(mapPage);
                if (File.Exists(stickyPage)) File.Delete(stickyPage);
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

        private static void StartupEvents(Form form, string session)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var tabs = Member<TabControl>(form, "k");
            var selected = tabs.SelectedTab;
            var address = Member<ToolStripComboBox>(form, "c");
            var search = Member<ToolStripComboBox>(form, "x");
            var failures = new System.Collections.Generic.List<string>();
            Action<string, Action> test = (name, action) =>
            {
                try { action(); Console.WriteLine("PASS startup: " + name); }
                catch (Exception error)
                {
                    var cause = error.GetBaseException();
                    failures.Add(name + ": " + cause.GetType().Name + ": " + cause.Message);
                    Console.WriteLine("FAIL startup: " + failures[failures.Count - 1]);
                }
            };
            Action<string, object, EventArgs> call = (name, sender, args) =>
                form.GetType().GetMethod(name, flags).Invoke(form, new object[] { sender, args });
            test("address selection event", () => typeof(ToolStripComboBox).GetMethod("OnSelectedIndexChanged", flags)
                .Invoke(address, new object[] { EventArgs.Empty }));
            test("search selection event", () => typeof(ToolStripComboBox).GetMethod("OnSelectedIndexChanged", flags)
                .Invoke(search, new object[] { EventArgs.Empty }));
            using (var legacyTab = new TabPage("Background legacy"))
            using (var browser = new WebBrowser())
            {
                legacyTab.Controls.Add(browser);
                tabs.TabPages.Add(legacyTab);
                tabs.SelectedTab = selected;
                var title = form.Text;
                var url = address.Text;
                var progress = Member<Control>(form, "LO").Size;
                var destination = new Uri("about:blank");
                test("background legacy navigating", () => call("nn", browser, new WebBrowserNavigatingEventArgs(destination, "")));
                test("background legacy navigated", () => call("nv", browser, new WebBrowserNavigatedEventArgs(destination)));
                test("background legacy completed", () => call("n0", browser, new WebBrowserDocumentCompletedEventArgs(destination)));
                test("background legacy progress", () =>
                {
                    call("q14", browser, new WebBrowserProgressChangedEventArgs(50, 100));
                    Check(Member<Control>(form, "LO").Size == progress, "background progress changed selected tab chrome");
                });
                test("Lea chrome survives background callbacks", () => Check(form.Text == title && address.Text == url &&
                    tabs.SelectedTab == selected, "background callback replaced Lea title, URL, or selection"));
                test("active legacy navigation and progress", () =>
                {
                    tabs.SelectedTab = legacyTab;
                    try
                    {
                        var args = new WebBrowserNavigatingEventArgs(destination, "");
                        call("nn", browser, args);
                        Check(!args.Cancel && legacyTab.Text == "Please Wait...", "active legacy navigation was lost");
                        call("q14", browser, new WebBrowserProgressChangedEventArgs(50, 100));
                        Check(Member<Control>(form, "LO").Width == Convert.ToInt32(form.Width * .5), "active legacy progress was lost");
                    }
                    finally { tabs.SelectedTab = selected; }
                });
                legacyTab.Tag = "locked";
                test("background lock belongs to sender", () =>
                {
                    var args = new WebBrowserNavigatingEventArgs(destination, "");
                    call("nn", browser, args);
                    Check(args.Cancel, "background locked tab was allowed to navigate");
                });
                legacyTab.Tag = null;
                selected.Tag = "locked";
                test("selected Lea lock does not lock other tabs", () =>
                {
                    var args = new WebBrowserNavigatingEventArgs(destination, "");
                    call("nn", browser, args);
                    Check(!args.Cancel, "unrelated selected tab cancelled background navigation");
                });
                selected.Tag = "lea";
                tabs.TabPages.Remove(legacyTab);
                test("detached legacy callbacks", () =>
                {
                    var args = new WebBrowserNavigatingEventArgs(destination, "");
                    call("nn", browser, args);
                    Check(args.Cancel, "detached browser navigation was not cancelled");
                    call("nv", browser, new WebBrowserNavigatedEventArgs(destination));
                    call("n0", browser, new WebBrowserDocumentCompletedEventArgs(destination));
                });
                test("wrong-engine callback sender", () =>
                {
                    var args = new WebBrowserNavigatingEventArgs(destination, "");
                    call("nn", selected.Controls[0], args);
                    Check(args.Cancel, "wrong-engine navigation sender was accepted");
                    call("nv", selected.Controls[0], new WebBrowserNavigatedEventArgs(destination));
                    call("n0", selected.Controls[0], new WebBrowserDocumentCompletedEventArgs(destination));
                });
            }
            test("empty-tab notification timer", () =>
            {
                tabs.SelectedIndex = -1;
                try
                {
                    var timer = Member<Timer>(form, "ti");
                    typeof(Timer).GetMethod("OnTick", flags).Invoke(timer, new object[] { EventArgs.Empty });
                }
                finally { tabs.SelectedTab = selected; }
            });
            test("empty-tab navigation commands", () =>
            {
                tabs.SelectedIndex = -1;
                try
                {
                    foreach (var name in new[] { "qleaBackClick", "qleaForwardClick", "qleaHomeClick", "qleaReloadClick", "qleaStopClick" })
                        call(name, form, EventArgs.Empty);
                }
                finally { tabs.SelectedTab = selected; }
            });
            test("locked session file", () =>
            {
                using (var stream = new FileStream(session, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    form.GetType().GetMethod("qleaRestoreSession", flags).Invoke(form, null);
            });
            test("oversized session file", () =>
            {
                using (var stream = new FileStream(session, FileMode.Create)) stream.SetLength(65537);
                var count = tabs.TabCount;
                form.GetType().GetMethod("qleaRestoreSession", flags).Invoke(form, null);
                Check(tabs.TabCount == count && Member<Label>(form, "su").Text.Contains("session"), "oversized session was not reported");
            });
            test("invalid and overlong session URLs", () =>
            {
                var count = tabs.TabCount;
                File.WriteAllLines(session, new[] { "", "not a url", "javascript:alert(1)", "about:blank", "https://example.invalid/" + new string('a', 4096) });
                form.GetType().GetMethod("qleaRestoreSession", flags).Invoke(form, null);
                Check(tabs.TabCount == count, "invalid session entry opened a tab");
            });
            test("session restore/save limits and private mode", () =>
            {
                var count = tabs.TabCount;
                var urls = new string[14];
                for (var index = 0; index < urls.Length; index++) urls[index] = ((V)selected.Controls[0]).U.AbsoluteUri;
                File.WriteAllLines(session, urls);
                var privacy = form.GetType().GetField("pv", flags);
                var oldPrivacy = privacy.GetValue(form);
                try
                {
                    privacy.SetValue(form, true);
                    form.GetType().GetMethod("qleaRestoreSession", flags).Invoke(form, null);
                    Check(tabs.TabCount == count, "private mode restored a saved session");
                    privacy.SetValue(form, false);
                    form.GetType().GetMethod("qleaRestoreSession", flags).Invoke(form, null);
                    Check(tabs.TabCount == count + 12, "session did not restore exactly twelve tabs");
                    form.GetType().GetMethod("qleaSaveSession", flags).Invoke(form, null);
                    Check(File.ReadAllLines(session).Length == 12 && new FileInfo(session).Length <= 65536, "saved session exceeded limits");
                    privacy.SetValue(form, true);
                    form.GetType().GetMethod("qleaSaveSession", flags).Invoke(form, null);
                    Check(!File.Exists(session), "private mode retained the session file");
                }
                finally
                {
                    privacy.SetValue(form, oldPrivacy);
                    while (tabs.TabCount > count)
                        form.GetType().GetMethod("qleaClose", flags).Invoke(form, new object[] { tabs.TabPages[tabs.TabCount - 1] });
                    tabs.SelectedTab = selected;
                }
            });
            File.Delete(session);
            Check(failures.Count == 0, String.Join("\n", failures.ToArray()));
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

        private static System.Drawing.Rectangle InlineBounds(V view, string id)
        {
            var target = BlockById(view.At, id);
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            var result = System.Drawing.Rectangle.Empty;
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), target) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var rect = (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
                result = result.IsEmpty ? rect : System.Drawing.Rectangle.Union(result, rect);
            }
            return result;
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

        private static System.Drawing.Rectangle ElementHit(V view, string id)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea pointer hits");
            foreach (var value in values)
            {
                var type = value.GetType();
                var element = type.GetField("El").GetValue(value) as N;
                if (element != null && element.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase))
                    return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            return System.Drawing.Rectangle.Empty;
        }

        private static void MoveView(V view, System.Drawing.Rectangle rect)
        {
            var method = typeof(V).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic);
            Check(method != null, "Lea mouse move route");
            method.Invoke(view, new object[] { new MouseEventArgs(MouseButtons.None, 0, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2 - view.Sy, 0) });
        }

        private static void PaintView(V view)
        {
            using (var bitmap = RenderView(view)) { }
        }

        private static System.Drawing.Bitmap RenderView(V view)
        {
            var method = typeof(V).GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Missing Lea paint route");
            var bitmap = new System.Drawing.Bitmap(Math.Max(1, view.Width), Math.Max(1, view.Height));
            try
            {
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                using (var args = new PaintEventArgs(graphics, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height)))
                    method.Invoke(view, new object[] { args });
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private static int ColorDistance(System.Drawing.Color left, System.Drawing.Color right)
        {
            return Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
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
