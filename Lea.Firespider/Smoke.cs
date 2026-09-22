using System;
using System.Collections.Generic;
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
                var hostMarker = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAKCAYAAAC0VX7mAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAApSURBVDhPY7ijpvafGKya/JooPGogboxNMzY8aiBujE0zNkxlA1//BwCR7MvUhfG8CgAAAABJRU5ErkJggg==";
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
                    .Replace("</style>", ".host-viewport-portrait,.host-viewport-landscape,.host-viewport-short{display:none}@media (orientation:portrait){.host-viewport-portrait{display:block;width:181px}}@media (orientation:landscape){.host-viewport-landscape{display:block;width:241px}}@media (max-height:500px){.host-viewport-short{display:block;width:171px}}</style>")
                    .Replace("</body>", "<p id='host-viewport-portrait' class='host-viewport-portrait'>Portrait host viewport</p><p id='host-viewport-landscape' class='host-viewport-landscape'>Landscape host viewport</p><p id='host-viewport-short' class='host-viewport-short'>Short host viewport</p></body>")
                    .Replace("</style>", ".host-ratio-wide,.host-ratio-tall{display:none}@media (min-aspect-ratio:4/3) and (orientation:landscape){.host-ratio-wide{display:block;width:204px}}@media (max-aspect-ratio:1/1){.host-ratio-tall{display:block;width:164px}}</style>")
                    .Replace("</body>", "<p id='host-ratio-wide' class='host-ratio-wide'>Wide ratio host</p><p id='host-ratio-tall' class='host-ratio-tall'>Tall ratio host</p></body>")
                    .Replace("</style>", ".host-range-wide,.host-range-tall{display:none}@media (700px <= width < 1000px) and (3/2 <= aspect-ratio < 2/1){.host-range-wide{display:block;width:206px}}@media (width <= 500px) and (aspect-ratio < 1/1){.host-range-tall{display:block;width:166px}}</style>")
                    .Replace("</body>", "<p id='host-range-wide' class='host-range-wide'>Wide range host</p><p id='host-range-tall' class='host-range-tall'>Tall range host</p></body>")
                    .Replace("</style>", ".host-support-wide,.host-support-narrow{display:none}@supports (display:grid){@supports selector(:has(*)){@media (width>=800px){.host-support-wide{display:block;width:208px}}}}@media (width<=500px){@supports not (display:ruby){.host-support-narrow{display:block;width:168px}}}</style>")
                    .Replace("</body>", "<p id='host-support-wide' class='host-support-wide'>Wide supports host</p><p id='host-support-narrow' class='host-support-narrow'>Narrow supports host</p></body>")
                    .Replace("</style>", ".host-spacing-normal,.host-spacing-wide{width:240px;text-align:left}.host-spacing-normal{letter-spacing:0;word-spacing:0}.host-spacing-wide{letter-spacing:4px;word-spacing:9px}.host-spacing-field{width:220px;height:32px;letter-spacing:2px;word-spacing:6px}.host-spacing-responsive{letter-spacing:5px;word-spacing:10px}@media screen and (max-width:500px){.host-spacing-responsive{letter-spacing:-1px;word-spacing:0}}</style>")
                    .Replace("</body>", "<p id='host-spacing-normal' class='host-spacing-normal'>Host spacing words</p><p id='host-spacing-wide' class='host-spacing-wide'>Host spacing words</p><input id='host-spacing-field' class='host-spacing-field' name='host-spacing-field' value='Host raw words'><p id='host-spacing-responsive' class='host-spacing-responsive'>Responsive host spacing</p></body>")
                    .Replace("</style>", ".host-indent,.host-indent-rtl,.host-indent-responsive{width:240px;padding:0 20px;text-align:left}.host-indent{text-indent:28px}.host-indent-rtl{direction:rtl;text-align:start;text-indent:24px}.host-indent-responsive{text-indent:12px}@media screen and (max-width:500px){.host-indent-responsive{text-indent:30%}}</style>")
                    .Replace("</body>", "<p id='host-indent' class='host-indent'>HostIndent uses compact first line geometry</p><p id='host-indent-rtl' class='host-indent-rtl'>HostRTL uses inline start indentation</p><p id='host-indent-responsive' class='host-indent-responsive'>HostResponsive changes indentation on resize</p></body>")
                    .Replace("</style>", ".host-text-shadow{width:260px;color:#111827;text-shadow:5px 3px 0 #dc2626}.host-text-shadow-responsive{width:260px;text-shadow:2px 2px 0 #64748b}@media screen and (max-width:500px){.host-text-shadow-responsive{text-shadow:-4px 3px 4px #7c3aed}}</style>")
                    .Replace("</style>", ".host-decoration{width:260px;color:#111827;text-decoration:underline double #2563eb 2px;text-underline-offset:6px}.host-decoration-responsive{text-decoration:underline dotted #64748b 1px;text-underline-offset:2px}@media screen and (max-width:500px){.host-decoration-responsive{text-decoration:line-through wavy #7c3aed 3px;text-underline-offset:25%}}</style>")
                    .Replace("</style>", ".host-wrap-auto,.host-wrap-balance,.host-wrap-responsive,.host-wrap-command{width:220px}.host-wrap-auto{text-wrap:auto}.host-wrap-balance{text-wrap:balance}.host-wrap-responsive{text-wrap:auto}.host-wrap-command{text-wrap:nowrap}@media screen and (max-width:500px){.host-wrap-responsive{text-wrap:balance}}</style>")
                    .Replace("</style>", ".host-align,.host-align-responsive{width:260px}.host-align{direction:rtl;text-align:justify-all}.host-align-responsive{direction:rtl;text-align:center}@media screen and (max-width:500px){.host-align-responsive{text-align:end}}</style>")
                    .Replace("</style>", ".host-align-last,.host-align-last-responsive{width:260px}.host-align-last{text-align-last:center}.host-align-last-responsive{text-align-last:left}@media screen and (max-width:500px){.host-align-last-responsive{text-align-last:end}}</style>")
                    .Replace("</style>", ".host-overflow{width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.host-overflow-responsive{text-overflow:clip}@media screen and (max-width:500px){.host-overflow-responsive{text-overflow:ellipsis}}</style>")
                    .Replace("</body>", "<p id='host-overflow' class='host-overflow'>Host overflow keeps this entire authored sentence</p><p id='host-overflow-responsive' class='host-overflow host-overflow-responsive'>Responsive host overflow keeps its full text</p></body>")
                    .Replace("</style>", ".host-clamp,.host-clamp-responsive,.host-clamp-command{width:180px;text-align:left}.host-clamp{line-clamp:2}.host-clamp-responsive{line-clamp:none}.host-clamp-command{-webkit-line-clamp:none}@media screen and (max-width:500px){.host-clamp-responsive{line-clamp:1}}</style>")
                    .Replace("</body>", "<p id='host-clamp' class='host-clamp'>Firespider uses Lea to keep two rendered lines and remove every later retained line.</p><p id='host-clamp-responsive' class='host-clamp-responsive'>Responsive Firespider line clamping becomes one compact line in a narrow browser window.</p><p id='host-clamp-command' class='host-clamp-command'>Compact host commands apply line clamping without long engine method names.</p></body>")
                    .Replace("</style>", ".host-list{width:190px;margin-left:48px}.host-list-upper{list-style-type:upper-alpha;text-align:left}.host-list-inside{list-style:upper-roman inside;text-align:left}.host-list-responsive{list-style-type:circle;list-style-position:outside}.host-list-command{list-style-type:lower-alpha}@media screen and (max-width:500px){.host-list-responsive{list-style-type:square;list-style-position:inside}}</style>")
                    .Replace("</body>", "<ol start='27'><li id='host-list-upper' class='host-list host-list-upper'>Host upper marker</li></ol><ol start='4'><li id='host-list-inside' class='host-list host-list-inside'><strong>HostInside</strong> marker</li></ol><ul><li id='host-list-responsive' class='host-list host-list-responsive'><strong>HostResponsive</strong> marker</li></ul><ol start='3'><li id='host-list-command' class='host-list host-list-command'>Host command marker<input id='host-list-field' value='Host marker field'></li></ol><ol reversed><li id='host-list-reverse-three' class='host-list'>Host reverse three</li><li class='host-list'>Host reverse two</li><li class='host-list'>Host reverse one</li></ol></body>")
                    .Replace("</style>", ".host-list-image{list-style:upper-alpha url('" + hostMarker + "') outside}.host-list-image-inside{list-style:none url('" + hostMarker + "') inside}.host-list-image-responsive{list-style-type:circle;list-style-image:none}@media screen and (max-width:500px){.host-list-image-responsive{list-style-image:url('" + hostMarker + "')}}</style>")
                    .Replace("</body>", "<ol><li id='host-list-image' class='host-list host-list-image'><strong>HostImage</strong> outside marker</li><li id='host-list-image-inside' class='host-list host-list-image-inside'><strong>HostImageInside</strong> marker</li><li id='host-list-image-responsive' class='host-list host-list-image-responsive'>Host responsive image marker</li></ol></body>")
                    .Replace("</style>", ".host-marker::marker{content:'Lea ';color:#dc2626;font-size:22px;font-weight:bold}.host-marker-inside{list-style-position:inside}.host-marker-inside::marker{content:'In ';color:#2563eb;font-size:20px}.host-marker-generated::marker{content:'[' attr(data-code) '] ' / 'Host marker: ';color:#0f766e}.host-marker-fallback::marker{content:attr(data-missing,'Marker fallback ');color:#a13b18}.host-marker-counter::marker{content:'(' counters(list-item,'.',upper-roman) ') ';color:#0f766e}.host-marker-responsive::marker{content:attr(data-wide,'Wide ');color:#64748b}.host-marker-command::marker{content:'CSS ';color:#64748b}@media screen and (max-width:500px){.host-marker-responsive::marker{content:attr(data-narrow,'Narrow ');color:#7c3aed;font-size:20px}}</style>")
                    .Replace("</body>", "<ol><li id='host-marker' class='host-list host-marker'>Host styled marker</li><li id='host-marker-inside' class='host-list host-marker-inside'>Host inside styled marker</li><li id='host-marker-generated' class='host-list host-marker-generated' data-code='HOST'>Host generated marker</li><li id='host-marker-fallback' class='host-list host-marker-fallback'>Host marker fallback</li><li id='host-marker-responsive' class='host-list host-marker-responsive'>Host responsive styled marker</li><li id='host-marker-command' class='host-list host-marker-command' data-code='CMD'>Host marker command</li></ol><ol start='9'><li>Host outer counter<ol start='3'><li id='host-marker-counter' class='host-list host-marker-counter'>Host nested counter marker</li></ol></li></ol></body>")
                    .Replace("</style>", ".host-css-counter-scope{counter-reset:hostCount 2}.host-css-counter{counter-increment:hostCount 3}.host-css-counter::marker{content:'N' counter(hostCount,upper-roman) ' ';color:#dc2626}.host-css-counter-nested-scope{counter-reset:hostCount 1}.host-css-counter-nested{counter-increment:hostCount 2}.host-css-counter-nested::marker{content:counters(hostCount,'.',upper-roman) ' ';color:#2563eb}</style>")
                    .Replace("</body>", "<div><ol class='host-css-counter-scope'><li id='host-css-counter' class='host-list host-css-counter'>Host named counter<ol class='host-css-counter-nested-scope'><li id='host-css-counter-nested' class='host-list host-css-counter-nested'>Host nested named counter</li></ol></li></ol></div></body>")
                    .Replace("</style>", ".host-generated::before{content:'[HOST] ' / '';color:#dc2626;font-size:22px;font-weight:bold}.host-generated::after{content:' [A]' / ' ending';color:#2563eb;font-style:italic}.host-generated-fallback::before{content:attr(data-missing,'Fallback ');color:#0f766e}.host-generated-link::before{content:attr(data-open,'Open ') / 'Accessible ';color:#dc2626}.host-generated-link::after{content:' now' / ' complete';color:#2563eb}.host-generated-scope{counter-reset:hostGenerated 2}.host-generated-counter{counter-increment:hostGenerated}.host-generated-counter::before{content:'G' counter(hostGenerated) ' ';color:#0f766e}.host-generated-cell::before{content:attr(data-prefix,'Cell ') / 'Accessible ';color:#dc2626}.host-generated-command::before{content:'CSS ' / 'Authored ';color:#64748b}.host-generated-command::after{content:' end' / ' complete';color:#64748b}.host-generated-responsive::after{content:attr(data-state,' Wide') / ' Wide accessible';color:#64748b}@media screen and (max-width:500px){.host-generated-responsive::after{content:attr(data-state,' Narrow') / ' Narrow accessible';color:#2563eb;font-size:20px}}</style>")
                    .Replace("</body>", "<p id='host-generated' class='host-generated'>Host generated content</p><p id='host-generated-fallback' class='host-generated-fallback'>Host attr fallback</p><a id='host-generated-link' class='host-generated-link' href='#state'>generated link</a><div class='host-generated-scope'><p id='host-generated-counter' class='host-generated-counter'>Generated counter</p></div><table><tr><td id='host-generated-cell' class='host-generated-cell'>Generated table</td></tr></table><p id='host-generated-command' class='host-generated-command' data-before='CMD' data-after='READ'>Host generated command</p><p id='host-generated-responsive' class='host-generated-responsive'>Generated responsive</p></body>")
                    .Replace("</style>", ":root{--host-generated-picture:url('" + hostMarker + "')}.host-generated-picture::before{content:'<' var(--host-generated-picture) '>' / 'Host picture '}.host-generated-picture-link::before{content:var(--host-generated-picture) / 'Host picture link'}.host-generated-picture-marker::marker{content:'[' var(--host-generated-picture) ']' / 'Host picture marker: '}.host-generated-picture-responsive::before{content:'WIDE ' / 'Wide picture '}@media screen and (max-width:500px){.host-generated-picture-responsive::before{content:var(--host-generated-picture) / 'Narrow picture '}}</style>")
                    .Replace("</body>", "<p id='host-generated-picture' class='host-generated-picture'>Host picture content</p><a id='host-generated-picture-link' class='host-generated-picture-link' href='#state'></a><ol><li id='host-generated-picture-marker' class='host-list host-generated-picture-marker'>Host picture marker</li></ol><p id='host-generated-picture-responsive' class='host-generated-picture-responsive'>Host picture responsive</p></body>")
                    .Replace("</style>", ".host-quote::before{content:open-quote}.host-quote::after{content:close-quote}.host-quote span::before{content:open-quote}.host-quote span::after{content:close-quote}.host-quote-pair::before{content:open-quote}.host-quote-pair::after{content:close-quote}.host-quote-responsive{quotes:'(' ')'}@media screen and (max-width:500px){.host-quote-responsive{quotes:'[' ']'}}</style>")
                    .Replace("</body>", "<a id='host-quote' class='host-quote' style=\"quotes:'<' '>' '[' ']'\" href='#state'>Host <span>nested</span> quote</a><p id='host-quote-command' class='host-quote-pair' style=\"quotes:'(' ')'\">Host quote command</p><p id='host-quote-responsive' class='host-quote-pair host-quote-responsive'>Host responsive quote</p></body>")
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
                    .Replace("</style>", ".host-viewport{width:calc(50vw - 20px);height:25vh;min-width:20vmin;max-height:40vmax;background:#fee9d8}</style>")
                    .Replace("</style>", "html{font-size:20px}body{font-size:10px}.host-font{font-size:18px;width:10em;height:4em;min-width:8rem;max-height:5rem;background:#e8f3ff}.host-rem{font-size:12px;width:10rem;height:5em;background:#dff6e9}</style>")
                    .Replace("</style>", ".host-size-function{font-size:16px;width:clamp(180px,25em,420px);height:min(6rem,140px);min-width:max(10rem,200px);max-height:clamp(80px,20vh,120px);background:#e8f3ff}.host-size-min{font-size:16px;width:min(50vw,360px,20rem);height:max(4em,70px);background:#dff6e9}</style>")
                    .Replace("</style>", ".host-logical-shell{position:relative;inline-size:400px;block-size:240px;background:#fff4cc}.host-logical{position:absolute;inline-size:calc(50% + 20px);min-inline-size:180px;max-inline-size:240px;block-size:50%;min-block-size:100px;max-block-size:160px;inset-inline:24px auto;inset-block:18px auto;background:#fee9d8}</style>")
                    .Replace("</style>", ".host-logical-box-shell{position:relative;width:400px;height:220px;margin:0;padding:0;background:#e8f3ff}.host-logical-box{position:absolute;left:20px;top:30px;width:100px;height:40px;margin-inline:12px 18px;margin-block:14px 16px;padding-inline:8px 10px;padding-block:6px 7px;background:#dff6e9}</style>")
                    .Replace("</style>", ".host-logical-border-shell{position:relative;width:400px;height:180px;margin:0;padding:0;background:#fff4cc}.host-logical-border{position:absolute;left:20px;top:30px;width:100px;height:40px;margin:0;padding:0;border-inline:4px solid #123f8c;border-block:6px dashed #176b4d;border-radius:14px}.host-logical-border-command{width:120px;height:40px;margin:0;padding:0;border:2px solid #123f8c}</style>")
                    .Replace("</style>", ".host-logical-radius{width:160px;height:70px;margin:0;padding:0;border:6px solid #176b4d;background:#fff4cc;border-radius:0;border-start-start-radius:36px 28px;border-start-end-radius:12px 9px;border-end-end-radius:18px 14px;border-end-start-radius:24px 20px}.host-logical-radius-command{width:140px;height:54px;margin:0;padding:0;border:5px solid #123f8c;border-radius:2px 3px 4px 5px/6px 7px 8px 9px}</style>")
                    .Replace("</style>", ".host-direction{direction:rtl;width:120px;margin-inline:9px 11px;padding-inline:5px 7px;border-inline-start:3px solid #123f8c;border-inline-end:4px dashed #a13b18;border-start-start-radius:20px 21px;border-start-end-radius:10px 11px}</style>")
                    .Replace("</style>", ".host-direction-flex{direction:rtl;display:flex;width:220px;height:40px;gap:10px;align-items:start}.host-direction-flex p{width:40px;height:20px;margin:0;padding:0}.host-direction-grid{direction:rtl;display:grid;width:220px;height:60px;grid-template-columns:40px 60px 80px;grid-template-rows:25px 25px;gap:10px;justify-content:start}.host-direction-grid p{margin:0;padding:0}.host-direction-grid #host-direction-grid-span{grid-column:1/span 2}.host-direction-grid #host-direction-grid-side{grid-column:3}</style>")
                    .Replace("</style>", ".host-direction-inline{direction:rtl;width:240px;margin:0;padding:0}.host-direction-list{direction:rtl;width:240px;margin:0;padding:0}.host-direction-list li{margin-inline-start:18px;padding:0}.host-direction-overflow{direction:rtl;width:126px;margin:0;padding:0;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.host-direction-range{direction:rtl;width:280px;margin:0;padding:0;white-space:nowrap}.host-direction-command{direction:ltr;width:142px;margin:0;padding:0;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}</style>")
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
                    .Replace("<p id='class-state'>", "<p id='host-viewport' class='host-viewport' style='color:#123f8c'>Viewport host size</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-font' class='host-font' style='color:#123f8c'>Element font host size</p><p id='host-rem' class='host-rem'>Root font host size</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-size-function' class='host-size-function' style='color:#123f8c'>Bounded host function size</p><p id='host-size-min' class='host-size-min'>Host minimum and maximum size</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-logical-shell' class='host-logical-shell'><p id='host-logical' class='host-logical' style='color:#123f8c'>Logical host size</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-logical-box-shell' class='host-logical-box-shell'><p id='host-logical-box' class='host-logical-box' style='color:#123f8c'>Logical host box</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-logical-border-shell' class='host-logical-border-shell'><p id='host-logical-border' class='host-logical-border'>Logical host border</p></section><p id='host-logical-border-command' class='host-logical-border-command'>Logical border command</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-logical-radius' class='host-logical-radius'>Logical host radius</p><p id='host-logical-radius-command' class='host-logical-radius-command'>Logical radius command</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-direction' class='host-direction'>RTL host direction</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<section id='host-direction-flex' class='host-direction-flex'><p id='host-direction-flex-a'>A</p><p id='host-direction-flex-b'>B</p><p id='host-direction-flex-c'>C</p></section><section id='host-direction-grid' class='host-direction-grid'><p id='host-direction-grid-span'>Span</p><p id='host-direction-grid-side'>Side</p></section><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-direction-plain' class='host-direction-inline'>Amber Bronze Cobalt</p><p id='host-direction-inline' class='host-direction-inline'><span id='host-inline-first'>First</span> <a id='host-inline-second' href='#state'>Second</a> <strong id='host-inline-third'>Third</strong></p><ol class='host-direction-list' start='4'><li id='host-direction-list'><span id='host-direction-list-text'>Host item</span></li></ol><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-direction-overflow' class='host-direction-overflow'><span>OriginAlpha</span> <a id='host-direction-overflow-link' href='#state'>LinkedMiddle</a> DestinationGamma</p><p id='host-direction-range' class='host-direction-range'><span>RangeFirst</span> <span>RangeSecond</span> <span>RangeThird</span></p><p id='host-direction-command' class='host-direction-command'>CommandOrigin CommandMiddle CommandDestination</p><p id='class-state'>")
                    .Replace("<p id='class-state'>", "<p id='host-var' class='host-var' style='--host-color:#a13b18;color:#a13b18;width:199px'>Important property host</p><p id='host-media' class='host-media'>Responsive host</p><p id='host-box' class='host-box'>Border-box host</p><section id='host-flex-column' class='host-flex-column'><p id='host-flex-column-a'>Column A</p><p id='host-flex-column-b'>Column B</p></section><svg id='host-svg' width='160' height='80' viewBox='0 0 80 40'><title>Firespider host vector</title><rect x='2' y='2' width='20' height='16' fill='#d7263d'></rect><circle cx='34' cy='10' r='8' fill='#10b981'></circle><path d='M46 2 h28 v16 H46 z' fill='#f59e0b'></path><a href='#state'><rect x='12' y='24' width='56' height='12' fill='#0369a1'></rect></a></svg><p id='class-state'>")
                    .Replace("</svg><p id='class-state'>", "<path d='M2 32 C8 20 14 20 20 32 S32 44 38 32' fill='none' stroke='#7c3aed' stroke-width='2'></path><g transform='translate(58 26)'><g transform='scale(1.5)'><rect x='0' y='0' width='10' height='6' fill='#db2777'></rect></g></g><defs><linearGradient id='host-linear'><stop offset='0' stop-color='#dc2626'></stop><stop offset='1' stop-color='#2563eb'></stop></linearGradient><radialGradient id='host-radial'><stop offset='0' stop-color='#f8fafc'></stop><stop offset='1' stop-color='#7c3aed'></stop></radialGradient><g id='host-reuse'><rect x='0' y='0' width='10' height='8'></rect><circle cx='7' cy='4' r='2' fill='#f8fafc'></circle></g><clipPath id='host-half' clipPathUnits='objectBoundingBox' clip-rule='evenodd'><rect x='0' y='0' width='.5' height='1'></rect></clipPath></defs><rect x='40' y='24' width='14' height='10' fill='url(#host-linear)'></rect><circle cx='69' cy='10' r='7' fill='url(#host-radial)'></circle><use href='#host-reuse' x='26' y='27' fill='#0d9488'></use><rect x='46' y='27' width='10' height='8' fill='#f59e0b' clip-path='url(#host-half)'></rect><path class='host-stroke' d='M2 20 H22' fill-rule='evenodd'></path></svg><p id='class-state'>")
                    .Replace("<p>Open route fixture</p>", "<p id='host-text-shadow' class='host-text-shadow'>Open route fixture</p>")
                    .Replace("<p>Second route match</p>", "<p id='host-text-shadow-responsive' class='host-text-shadow-responsive host-decoration-responsive'>Second route match</p>")
                    .Replace("<p>Third ROUTE match</p>", "<p id='host-decoration' class='host-decoration'>Third ROUTE match</p>");
                openMarkup = openMarkup.Replace("<p id='class-state'>", "<p id='host-wrap-auto' class='host-wrap-auto'>A lightweight browser engine built for everyone</p><p id='host-wrap-balance' class='host-wrap-balance'>A lightweight browser engine built for everyone</p><p id='host-wrap-responsive' class='host-wrap-responsive'>A lightweight browser engine built for everyone</p><p id='host-wrap-command' class='host-wrap-command'>A lightweight browser engine built for everyone</p><p id='host-align' class='host-align'>Host Align Words</p><p id='host-align-responsive' class='host-align-responsive'>ResponsiveAlign</p><p id='host-align-last' class='host-align-last'>HostLast</p><p id='host-align-last-responsive' class='host-align-last-responsive'>ResponsiveLast</p><p id='class-state'>");
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
                    var hostIndent = BlockById(host.At, "host-indent");
                    var hostIndentRtl = BlockById(host.At, "host-indent-rtl");
                    Check(hostIndent.S.Ti == 28 && !hostIndent.S.Tip &&
                        InlineTextRect(host, hostIndent, "HostIndent").Left == BlockContent(host, "host-indent").Left + 28 &&
                        hostIndentRtl.S.Dr == "r" && hostIndentRtl.S.Ti == 24 &&
                        InlineTextRect(host, hostIndentRtl, "HostRTL").Right == BlockContent(host, "host-indent-rtl").Right - 24,
                        "Firespider bounded LTR and RTL text indent geometry");
                    Check(host.Js("ti('host-indent','36px')") == 1 &&
                        InlineTextRect(host, BlockById(host.At, "host-indent"), "HostIndent").Left == BlockContent(host, "host-indent").Left + 36,
                        "Firespider compact text indent command");
                    Check(host.Js("document.getElementById('host-indent').style.textIndent='20%'") == 1 &&
                        BlockById(host.At, "host-indent").S.Tip && InlineTextRect(host, BlockById(host.At, "host-indent"), "HostIndent").Left ==
                        BlockContent(host, "host-indent").Left + BlockContent(host, "host-indent").Width / 5,
                        "Firespider literal DOM text indent command");
                    var hostTextShadow = BlockById(host.At, "host-text-shadow");
                    var hostTextShadowResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostTextShadow.S.Ts != null && hostTextShadow.S.Ts.X == 5 && hostTextShadow.S.Ts.Y == 3 && hostTextShadow.S.Ts.B == 0 && hostTextShadow.S.Ts.C == "#dc2626" &&
                        hostTextShadowResponsive.S.Ts != null && hostTextShadowResponsive.S.Ts.X == 2 && hostTextShadowResponsive.S.Ts.Y == 2,
                        "Firespider bounded text shadow aliases and responsive model");
                    using (var hostTextShadowPaint = RenderView(host))
                        Check(HasColor(hostTextShadowPaint, BlockOuter(host, "host-text-shadow"), System.Drawing.Color.FromArgb(220, 38, 38)),
                            "Firespider text shadow paint");
                    Check(host.Js("ts('host-text-shadow','6px 2px 0 #2563eb')") == 1 && BlockById(host.At, "host-text-shadow").S.Ts.X == 6 && BlockById(host.At, "host-text-shadow").S.Ts.C == "#2563eb",
                        "Firespider compact text shadow command");
                    Check(host.Js("document.getElementById('host-text-shadow').style.textShadow='-3px 2px 0 #176b4d'") == 1 && BlockById(host.At, "host-text-shadow").S.Ts.X == -3 && BlockById(host.At, "host-text-shadow").S.Ts.C == "#176b4d",
                        "Firespider literal DOM text shadow command");
                    var hostDecoration = BlockById(host.At, "host-decoration");
                    var hostDecorationResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostDecoration.S.Td == "u" && hostDecoration.S.Tc == "#2563eb" && hostDecoration.S.Tds == "d" && hostDecoration.S.Tw == 2 && hostDecoration.S.Uo == 6 && !hostDecoration.S.Uop &&
                        hostDecorationResponsive.S.Td == "u" && hostDecorationResponsive.S.Tc == "#64748b" && hostDecorationResponsive.S.Tds == "o" && hostDecorationResponsive.S.Tw == 1 && hostDecorationResponsive.S.Uo == 2 && !hostDecorationResponsive.S.Uop,
                        "Firespider bounded text decoration and underline offset aliases");
                    using (var hostDecorationPaint = RenderView(host))
                    {
                        var hostDecorationArea = BlockOuter(host, "host-decoration");
                        hostDecorationArea.Inflate(0, 12);
                        Check(HasColor(hostDecorationPaint, hostDecorationArea, System.Drawing.Color.FromArgb(37, 99, 235)),
                            "Firespider text decoration and offset paint");
                    }
                    Check(host.Js("td('host-decoration','line-through dashed #176b4d 2px')") == 1 && BlockById(host.At, "host-decoration").S.Td == "s" && BlockById(host.At, "host-decoration").S.Tds == "a",
                        "Firespider compact text decoration command");
                    Check(host.Js("document.getElementById('host-decoration').style.textDecorationLine='underline'") == 1 && BlockById(host.At, "host-decoration").S.Td == "u" &&
                        BlockById(host.At, "host-decoration").S.Tc == "#176b4d" && BlockById(host.At, "host-decoration").S.Tds == "a" && BlockById(host.At, "host-decoration").S.Tw == 2,
                        "Firespider text decoration line longhand command");
                    Check(host.Js("document.getElementById('host-decoration').style.textDecoration='underline wavy #dc2626 3px'") == 1 && BlockById(host.At, "host-decoration").S.Td == "u" && BlockById(host.At, "host-decoration").S.Tds == "w" && BlockById(host.At, "host-decoration").S.Tw == 3,
                        "Firespider literal DOM text decoration command");
                    Check(host.Js("uo('host-decoration','-3px')") == 1 && BlockById(host.At, "host-decoration").S.Uo == -3 && !BlockById(host.At, "host-decoration").S.Uop,
                        "Firespider compact text underline offset command");
                    Check(host.Js("document.getElementById('host-decoration').style.textUnderlineOffset='25%'") == 1 && BlockById(host.At, "host-decoration").S.Uo == 25 && BlockById(host.At, "host-decoration").S.Uop && BlockById(host.At, "host-decoration").S.Tds == "w",
                        "Firespider literal DOM text underline offset command and shorthand independence");
                    var hostAlignLast = BlockById(host.At, "host-align-last");
                    var hostAlignLastResponsive = BlockById(host.At, "host-align-last-responsive");
                    var hostAlignLastLine = InlineTextRect(host, hostAlignLast, "HostLast");
                    Check(hostAlignLast.S.Tl == "c" && hostAlignLastResponsive.S.Tl == "l" &&
                        Math.Abs(hostAlignLastLine.Left + hostAlignLastLine.Right - BlockContent(host, "host-align-last").Left - BlockContent(host, "host-align-last").Right) <= 2,
                        "Firespider bounded text align last model and centered geometry");
                    Check(host.Js("tl('host-align-last','right')") == 1, "Firespider compact text align last command");
                    PaintView(host);
                    hostAlignLast = BlockById(host.At, "host-align-last");
                    Check(hostAlignLast.S.Tl == "r" && InlineTextRect(host, hostAlignLast, "HostLast").Right == BlockContent(host, "host-align-last").Right,
                        "Firespider compact text align last geometry");
                    Check(host.Js("document.getElementById('host-align-last').style.textAlignLast='center'") == 1,
                        "Firespider literal DOM text align last command");
                    PaintView(host);
                    hostAlignLast = BlockById(host.At, "host-align-last");
                    hostAlignLastLine = InlineTextRect(host, hostAlignLast, "HostLast");
                    Check(hostAlignLast.S.Tl == "c" && Math.Abs(hostAlignLastLine.Left + hostAlignLastLine.Right - BlockContent(host, "host-align-last").Left - BlockContent(host, "host-align-last").Right) <= 2,
                        "Firespider literal DOM text align last geometry");
                    var hostAlign = BlockById(host.At, "host-align");
                    var hostAlignResponsive = BlockById(host.At, "host-align-responsive");
                    Check(hostAlign.S.Ta == "j" && hostAlign.S.Tl == "j" && hostAlignResponsive.S.Ta == "c" &&
                        InlineTextRect(host, hostAlign, "Host").Right == BlockContent(host, "host-align").Right &&
                        InlineTextRect(host, hostAlign, "Words").Left == BlockContent(host, "host-align").Left,
                        "Firespider bounded text align shorthand and justify all geometry");
                    Check(host.Js("ta('host-align','center')") == 1, "Firespider compact text align shorthand command");
                    PaintView(host);
                    hostAlign = BlockById(host.At, "host-align");
                    var hostAlignLine = System.Drawing.Rectangle.Union(InlineTextRect(host, hostAlign, "Host"), InlineTextRect(host, hostAlign, "Words"));
                    Check(hostAlign.S.Ta == "c" && hostAlign.S.Tl == "" &&
                        Math.Abs(hostAlignLine.Left + hostAlignLine.Right - BlockContent(host, "host-align").Left - BlockContent(host, "host-align").Right) <= 2,
                        "Firespider compact text align shorthand geometry");
                    Check(host.Js("tl('host-align','right');taa('host-align','left')") == 2,
                        "Firespider compact text align all command");
                    PaintView(host);
                    hostAlign = BlockById(host.At, "host-align");
                    hostAlignLine = System.Drawing.Rectangle.Union(InlineTextRect(host, hostAlign, "Host"), InlineTextRect(host, hostAlign, "Words"));
                    Check(hostAlign.S.Ta == "l" && hostAlign.S.Tl == "r" &&
                        hostAlignLine.Right == BlockContent(host, "host-align").Right,
                        "Firespider text align all preserves final-line override");
                    Check(host.Js("document.getElementById('host-align').style.textAlign='end'") == 1,
                        "Firespider literal DOM text align shorthand");
                    PaintView(host);
                    hostAlign = BlockById(host.At, "host-align");
                    hostAlignLine = System.Drawing.Rectangle.Union(InlineTextRect(host, hostAlign, "Host"), InlineTextRect(host, hostAlign, "Words"));
                    Check(hostAlign.S.Ta == "e" && hostAlign.S.Tl == "" &&
                        hostAlignLine.Left == BlockContent(host, "host-align").Left,
                        "Firespider literal DOM logical text align geometry");
                    var hostWrapAuto = BlockById(host.At, "host-wrap-auto");
                    var hostWrapBalance = BlockById(host.At, "host-wrap-balance");
                    var hostWrapResponsive = BlockById(host.At, "host-wrap-responsive");
                    var hostWrapCommand = BlockById(host.At, "host-wrap-command");
                    var hostWrapAutoLines = InlineLines(host, hostWrapAuto);
                    var hostWrapBalanceLines = InlineLines(host, hostWrapBalance);
                    Check(hostWrapAuto.S.Wm == "w" && hostWrapAuto.S.Wt == "" &&
                        hostWrapBalance.S.Wm == "w" && hostWrapBalance.S.Wt == "b" &&
                        hostWrapResponsive.S.Wt == "" && hostWrapCommand.S.Wm == "n" &&
                        hostWrapAutoLines.Length >= 2 && hostWrapAutoLines.Length == hostWrapBalanceLines.Length &&
                        LineWidthSpread(hostWrapBalanceLines) < LineWidthSpread(hostWrapAutoLines) &&
                        BlockOuter(host, "host-wrap-auto").Size == BlockOuter(host, "host-wrap-balance").Size,
                        "Firespider bounded text wrap state and balanced geometry");
                    Check(host.Js("tw('host-wrap-auto','balance')") == 1,
                        "Firespider compact text wrap command");
                    PaintView(host);
                    hostWrapAuto = BlockById(host.At, "host-wrap-auto");
                    Check(hostWrapAuto.S.Wt == "b" && LineWidthSpread(InlineLines(host, hostWrapAuto)) == LineWidthSpread(hostWrapBalanceLines),
                        "Firespider compact text wrap command geometry");
                    Check(host.Js("twm('host-wrap-command','wrap');tws('host-wrap-command','balance')") == 2,
                        "Firespider compact text wrap longhand commands");
                    PaintView(host);
                    hostWrapCommand = BlockById(host.At, "host-wrap-command");
                    Check(hostWrapCommand.S.Wm == "w" && hostWrapCommand.S.Wt == "b" && InlineLines(host, hostWrapCommand).Length > 1,
                        "Firespider compact text wrap longhand geometry");
                    Check(host.Js("document.getElementById('host-wrap-command').style.textWrap='nowrap'") == 1,
                        "Firespider literal DOM text wrap nowrap");
                    PaintView(host);
                    hostWrapCommand = BlockById(host.At, "host-wrap-command");
                    Check(hostWrapCommand.S.Wm == "n" && hostWrapCommand.S.Wt == "" && InlineLines(host, hostWrapCommand).Length == 1,
                        "Firespider literal DOM text wrap nowrap geometry");
                    Check(host.Js("document.getElementById('host-wrap-command').style.textWrap='balance'") == 1,
                        "Firespider literal DOM text wrap balance");
                    PaintView(host);
                    hostWrapCommand = BlockById(host.At, "host-wrap-command");
                    Check(hostWrapCommand.S.Wm == "w" && hostWrapCommand.S.Wt == "b" && InlineLines(host, hostWrapCommand).Length > 1,
                        "Firespider literal DOM text wrap balance geometry");
                    Check(BlockById(host.At, "host-overflow").S.To == "e" && EllipsisCount(host, "host-overflow") == 1 &&
                        EllipsisCount(host, "host-overflow-responsive") == 0 && host.At.X.Contains("Host overflow keeps this entire authored sentence"),
                        "Firespider text overflow alias, painted marker, and authored text");
                    var hostClamp = BlockById(host.At, "host-clamp");
                    var hostClampResponsive = BlockById(host.At, "host-clamp-responsive");
                    Check(hostClamp.S.Lc == 2 && InlineLines(host, hostClamp).Length == 2 && EllipsisCount(host, "host-clamp") == 1 &&
                        hostClampResponsive.S.Lc == 0 && EllipsisCount(host, "host-clamp-responsive") == 0 &&
                        host.At.X.Contains("Firespider uses Lea to keep two rendered lines"),
                        "Firespider bounded line clamp model, geometry, and authored text");
                    Check(host.Js("lc('host-clamp-command','2')") == 1, "Firespider compact line clamp command");
                    PaintView(host);
                    var hostClampCommand = BlockById(host.At, "host-clamp-command");
                    Check(hostClampCommand.S.Lc == 2 && InlineLines(host, hostClampCommand).Length == 2 && EllipsisCount(host, "host-clamp-command") == 1,
                        "Firespider compact line clamp command geometry");
                    Check(host.Js("document.getElementById('host-clamp-command').style.webkitLineClamp='1'") == 1,
                        "Firespider literal WebKit line clamp command");
                    PaintView(host);
                    hostClampCommand = BlockById(host.At, "host-clamp-command");
                    Check(hostClampCommand.S.Lc == 1 && InlineLines(host, hostClampCommand).Length == 1 && EllipsisCount(host, "host-clamp-command") == 1,
                        "Firespider literal WebKit line clamp geometry");
                    var hostListUpper = BlockById(host.At, "host-list-upper");
                    var hostListInside = BlockById(host.At, "host-list-inside");
                    var hostListResponsive = BlockById(host.At, "host-list-responsive");
                    var hostListCommand = BlockById(host.At, "host-list-command");
                    var hostListImage = BlockById(host.At, "host-list-image");
                    var hostListImageInside = BlockById(host.At, "host-list-image-inside");
                    var hostListImageResponsive = BlockById(host.At, "host-list-image-responsive");
                    var hostMarkerStyled = BlockById(host.At, "host-marker");
                    var hostMarkerInside = BlockById(host.At, "host-marker-inside");
                    var hostMarkerGenerated = BlockById(host.At, "host-marker-generated");
                    var hostMarkerFallback = BlockById(host.At, "host-marker-fallback");
                    var hostMarkerCounter = BlockById(host.At, "host-marker-counter");
                    var hostMarkerResponsive = BlockById(host.At, "host-marker-responsive");
                    var hostMarkerCommand = BlockById(host.At, "host-marker-command");
                    var hostCssCounter = BlockById(host.At, "host-css-counter");
                    var hostCssCounterNested = BlockById(host.At, "host-css-counter-nested");
                    var hostGenerated = BlockById(host.At, "host-generated");
                    var hostGeneratedFallback = BlockById(host.At, "host-generated-fallback");
                    var hostGeneratedLink = BlockById(host.At, "host-generated-link");
                    var hostGeneratedCounter = BlockById(host.At, "host-generated-counter");
                    var hostGeneratedCell = BlockById(host.At, "host-generated-cell");
                    var hostGeneratedCommand = BlockById(host.At, "host-generated-command");
                    var hostGeneratedResponsive = BlockById(host.At, "host-generated-responsive");
                    var hostGeneratedPicture = BlockById(host.At, "host-generated-picture");
                    var hostGeneratedPictureLink = BlockById(host.At, "host-generated-picture-link");
                    var hostGeneratedPictureMarker = BlockById(host.At, "host-generated-picture-marker");
                    var hostGeneratedPictureResponsive = BlockById(host.At, "host-generated-picture-responsive");
                    var hostQuote = BlockById(host.At, "host-quote");
                    var hostQuoteCommand = BlockById(host.At, "host-quote-command");
                    var hostQuoteResponsive = BlockById(host.At, "host-quote-responsive");
                    var hostListField = FieldById(host.At, "host-list-field");
                    Check(hostListUpper.S.Lst == "A" && hostListUpper.S.Lp == "o" && hostListUpper.Mk == "AA." &&
                        hostListInside.S.Lst == "R" && hostListInside.S.Lp == "i" && hostListInside.Mk == "IV." &&
                        hostListResponsive.S.Lst == "c" && hostListResponsive.S.Lp == "o" && hostListResponsive.Mk == "\u25e6" &&
                        hostListCommand.S.Lst == "a" && hostListCommand.Mk == "c." && BlockById(host.At, "host-list-reverse-three").Mk == "3.",
                        "Firespider bounded list marker type, position, shorthand, and reversed numbering");
                    Check(hostListImage.S.Li != null && hostListImage.S.Li.Scheme == "data" && hostListImage.S.Lst == "A" && hostListImage.S.Lp == "o" &&
                        hostListImageInside.S.Li != null && hostListImageInside.S.Lst == "n" && hostListImageInside.S.Lp == "i" && hostListImageResponsive.S.Li == null,
                        "Firespider bounded list marker image model and shorthand");
                    Check(hostMarkerStyled.Ms != null && hostMarkerStyled.Ms.F == "#dc2626" && hostMarkerStyled.Ms.Fs == 22 && hostMarkerStyled.Ms.B && hostMarkerStyled.Mk == "Lea " &&
                        hostMarkerInside.Ms.F == "#2563eb" && hostMarkerInside.S.Lp == "i" && hostMarkerInside.Mk == "In " &&
                        hostMarkerGenerated.Mk == "[HOST] " && hostMarkerGenerated.Ms.Ma == "Host marker: " &&
                        host.Ax(host.Id("host-marker-generated")).N == "Host marker: Host generated marker" &&
                        hostMarkerFallback.Mk == "Marker fallback " && hostMarkerCounter.Mk == "(IX.III) " &&
                        hostMarkerResponsive.Mk == "Wide " && hostMarkerCommand.Mk == "CSS ",
                        "Firespider bounded marker pseudo-element model");
                    Check(hostCssCounter.Mk == "NV " && hostCssCounter.S.Ci == "hostCount 3" &&
                        hostCssCounterNested.Mk == "V.III ", "Firespider bounded named CSS counter model");
                    Check(hostGenerated.X == "[HOST] Host generated content [A]" && Run(hostGenerated, "be").S.F == "#dc2626" &&
                        Run(hostGenerated, "be").S.Fs == 22 && Run(hostGenerated, "af").S.F == "#2563eb" &&
                        hostGeneratedFallback.X == "Fallback Host attr fallback" &&
                        hostGeneratedLink.X == "Open generated link now" && Run(hostGeneratedLink, "be").U != null &&
                        hostGeneratedCounter.X == "G3 Generated counter" && hostGeneratedCell.X == "Cell Generated table" &&
                        hostGeneratedCommand.X == "CSS Host generated command end" && hostGeneratedResponsive.X == "Generated responsive Wide",
                        "Firespider bounded before and after generated-content model");
                    Check(Run(hostGenerated, "be").S.Ma == "" && host.Ax(host.Id("host-generated")).N == "Host generated content ending" &&
                        host.Ax(host.Id("host-generated-link")).N == "Accessible generated link complete" &&
                        host.Ax(host.Id("host-generated-cell")).N == "Accessible Generated table" &&
                        host.Ax(host.Id("host-generated-command")).N == "Authored Host generated command complete" &&
                        host.Ax(host.Id("host-generated-responsive")).N == "Generated responsive Wide accessible",
                        "Firespider generated alternatives and accessibility");
                    Check(Run(hostGeneratedPicture, "be").S.Mf != null && Run(hostGeneratedPicture, "be").S.Mf.Count == 3 &&
                        Run(hostGeneratedPicture, "be").S.Mf[1].U != null && Run(hostGeneratedPicture, "be").S.Mf[1].U.Scheme == "data" &&
                        host.Ax(host.Id("host-generated-picture")).N == "Host picture Host picture content" &&
                        hostGeneratedPictureLink.X == "" && Run(hostGeneratedPictureLink, "be").U != null &&
                        host.Ax(host.Id("host-generated-picture-link")).N == "Host picture link" &&
                        hostGeneratedPictureMarker.Ms.Mf.Count == 3 &&
                        host.Ax(host.Id("host-generated-picture-marker")).N == "Host picture marker: Host picture marker" &&
                        Run(hostGeneratedPictureResponsive, "be").S.Mf == null,
                        "Firespider bounded generated image model and accessibility");
                    Check(hostQuote.X == "<Host [nested] quote>" && hostQuote.S.Qc == 2 && hostQuote.S.Qo(0) == "<" && hostQuote.S.Qx(1) == "]" &&
                        hostQuoteCommand.X == "(Host quote command)" && hostQuoteResponsive.X == "(Host responsive quote)" &&
                        host.Ax(host.Id("host-quote")).N == "<Host [nested] quote>",
                        "Firespider bounded generated quote model and accessibility");
                    var hostListUpperContent = BlockContent(host, "host-list-upper");
                    var hostListScroll = host.Sy;
                    host.Sc(hostListUpperContent.Top - host.Sy - 40);
                    PaintView(host);
                    hostListUpperContent = BlockContent(host, "host-list-upper");
                    var hostListUpperMarker = InlineTextRect(host, hostListUpper, "AA.");
                    Check(hostListUpperMarker.Right < hostListUpperContent.Left,
                        "Firespider list marker paint geometry: " + hostListUpperMarker + " in " + hostListUpperContent);
                    var hostListInsideContent = BlockContent(host, "host-list-inside");
                    var hostListInsideMarker = InlineTextRect(host, hostListInside, "IV.");
                    Check(hostListInsideMarker.Left == hostListInsideContent.Left &&
                        InlineTextRect(host, hostListInside, "HostInside").Left > hostListInsideMarker.Right,
                        "Firespider inside list marker geometry: " + hostListInsideMarker + " in " + hostListInsideContent);
                    var hostImageMarker = InlineKindRect(host, hostListImage, "lm");
                    var hostInsideImageMarker = InlineKindRect(host, hostListImageInside, "lm");
                    Check(hostImageMarker.Size == new System.Drawing.Size(20, 10) && hostImageMarker.Right < BlockContent(host, "host-list-image").Left &&
                        hostInsideImageMarker.Left == BlockContent(host, "host-list-image-inside").Left &&
                        InlineTextRect(host, hostListImageInside, "HostImageInside").Left > hostInsideImageMarker.Right,
                        "Firespider list marker image inside/outside geometry");
                    Check(InlineTextRect(host, hostMarkerStyled, "Lea ").Right < BlockContent(host, "host-marker").Left &&
                        InlineTextRect(host, hostMarkerInside, "In ").Left == BlockContent(host, "host-marker-inside").Left &&
                        InlineTextRect(host, hostMarkerGenerated, "[HOST] ").Right < BlockContent(host, "host-marker-generated").Left &&
                        InlineTextRect(host, hostMarkerFallback, "Marker fallback ").Right < BlockContent(host, "host-marker-fallback").Left &&
                        InlineTextRect(host, hostMarkerCounter, "(IX.III) ").Right < BlockContent(host, "host-marker-counter").Left &&
                        InlineTextRect(host, hostCssCounter, "NV ").Right < BlockContent(host, "host-css-counter").Left &&
                        InlineTextRect(host, hostCssCounterNested, "V.III ").Right < BlockContent(host, "host-css-counter-nested").Left,
                        "Firespider marker pseudo-element outside/inside geometry");
                    Check(InlineKindRect(host, hostGenerated, "be").Width > 0 && InlineKindRect(host, hostGenerated, "af").Width > 0 &&
                        InlineKindRect(host, hostGenerated, "be").Left < InlineKindRect(host, hostGenerated, "af").Left &&
                        InlineKindRect(host, hostGeneratedFallback, "be").Width > 0 &&
                        InlineKindRect(host, hostGeneratedLink, "be").Width > 0 && InlineKindRect(host, hostGeneratedCell, "be").Width > 0,
                        "Firespider generated before/after link and table geometry");
                    Check(InlinePictureRect(host, hostGeneratedPicture, "be").Size == new System.Drawing.Size(20, 10) &&
                        InlinePictureRect(host, hostGeneratedPictureLink, "be").Size == new System.Drawing.Size(20, 10) &&
                        InlinePictureRect(host, hostGeneratedPictureMarker, "lm").Right < BlockContent(host, "host-generated-picture-marker").Left,
                        "Firespider generated image before, link, and marker geometry");
                    Check(InlineKindRect(host, hostQuote, "be").Width > 0 && InlineKindRect(host, hostQuote, "af").Width > 0,
                        "Firespider generated quote geometry");
                    host.Sc(hostListScroll - host.Sy);
                    PaintView(host);
                    Check(host.Js("lst('host-list-command','upper-roman')") == 1, "Firespider compact list marker command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Lst == "R" && hostListCommand.S.Lp == "o" && hostListCommand.Mk == "III." &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider compact list marker command model and field retention");
                    Check(host.Js("lp('host-list-command','inside')") == 1, "Firespider compact list position command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Lp == "i" && InlineTextRect(host, hostListCommand, "III.").Left == BlockContent(host, "host-list-command").Left,
                        "Firespider compact list position command geometry");
                    Check(host.Js("document.getElementById('host-list-command').style.listStyle='square outside'") == 1,
                        "Firespider literal DOM list shorthand command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Lst == "s" && hostListCommand.S.Lp == "o" &&
                        InlineTextRect(host, hostListCommand, "\u25aa").Right < BlockContent(host, "host-list-command").Left,
                        "Firespider literal DOM list shorthand geometry");
                    Check(host.Js("document.getElementById('host-list-command').style.listStyleType='square'") == 1,
                        "Firespider literal DOM list marker command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Lst == "s" && hostListCommand.S.Lp == "o" && hostListCommand.Mk == "\u25aa",
                        "Firespider literal DOM list marker model");
                    Check(host.Js("li('host-list-command','url(" + hostMarker + ")')") == 1, "Firespider compact list image command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Li != null && InlineKindRect(host, hostListCommand, "lm").Right < BlockContent(host, "host-list-command").Left,
                        "Firespider compact list image command geometry");
                    Check(host.Js("document.getElementById('host-list-command').style.listStyleImage='none'") == 1,
                        "Firespider literal DOM list image command");
                    PaintView(host);
                    hostListCommand = BlockById(host.At, "host-list-command");
                    Check(hostListCommand.S.Li == null && InlineTextRect(host, hostListCommand, "\u25aa").Right < BlockContent(host, "host-list-command").Left,
                        "Firespider literal DOM list image fallback geometry");
                    Check(host.Js("ms('host-marker-command',\"content:attr(data-code) / 'Command marker: ';color:#dc2626;font-size:25px;font-weight:bold\")") == 1,
                        "Firespider compact marker style command");
                    PaintView(host);
                    hostMarkerCommand = BlockById(host.At, "host-marker-command");
                    Check(hostMarkerCommand.Ms.F == "#dc2626" && hostMarkerCommand.Ms.Fs == 25 && hostMarkerCommand.Ms.B && hostMarkerCommand.Mk == "CMD" &&
                        hostMarkerCommand.Ms.Ma == "Command marker: " &&
                        host.Ax(host.Id("host-marker-command")).N == "Command marker: Host marker command" &&
                        InlineTextRect(host, hostMarkerCommand, "CMD").Right < BlockContent(host, "host-marker-command").Left,
                        "Firespider compact marker style command geometry");
                    Check(host.Js("markerstyle('host-marker-command','content:attr(data-code raw-string);color:#2563eb;font-style:italic')") == 1 &&
                        host.Js("ms('host-marker-command','content:attr(data-code number)')") == 0 &&
                        host.Js("ms('host-marker-command','background:red')") == 0,
                        "Firespider readable marker style alias and validation");
                    PaintView(host);
                    hostMarkerCommand = BlockById(host.At, "host-marker-command");
                    Check(hostMarkerCommand.Ms.F == "#2563eb" && hostMarkerCommand.Ms.I && hostMarkerCommand.Mk == "CMD",
                        "Firespider readable marker style command model");
                    Check(host.Js("ms('host-marker-counter','content:counter(list-item,upper-alpha);color:#dc2626;font-size:24px')") == 1,
                        "Firespider compact marker counter command");
                    PaintView(host);
                    hostMarkerCounter = BlockById(host.At, "host-marker-counter");
                    Check(hostMarkerCounter.Mk == "C" && hostMarkerCounter.Ms.F == "#dc2626" && hostMarkerCounter.Ms.Fs == 24 &&
                        InlineTextRect(host, hostMarkerCounter, "C").Right < BlockContent(host, "host-marker-counter").Left,
                        "Firespider compact marker counter command geometry");
                    Check(host.Js("markerstyle('host-marker-counter','content:counters(list-item,\"/\",lower-roman);color:#2563eb')") == 1 &&
                        host.Js("ms('host-marker-counter','content:counter(none)')") == 0,
                        "Firespider readable nested marker counter and validation");
                    PaintView(host);
                    hostMarkerCounter = BlockById(host.At, "host-marker-counter");
                    Check(hostMarkerCounter.Mk == "ix/iii" && hostMarkerCounter.Ms.F == "#2563eb",
                        "Firespider readable nested marker counter model");
                    Check(host.Js("cr('host-css-counter','hostCount 7');ci('host-css-counter','hostCount 2');cs('host-css-counter','hostCount 4')") == 3,
                        "Firespider compact named counter commands");
                    PaintView(host);
                    hostCssCounter = BlockById(host.At, "host-css-counter");
                    Check(hostCssCounter.Mk == "NIV " && hostCssCounter.S.Cr == "hostCount 7" &&
                        hostCssCounter.S.Ci == "hostCount 2" && hostCssCounter.S.Cs == "hostCount 4" &&
                        InlineTextRect(host, hostCssCounter, "NIV ").Right < BlockContent(host, "host-css-counter").Left,
                        "Firespider compact named counter command geometry");
                    Check(host.Js("document.getElementById('host-css-counter').style.counterSet='hostCount 6'") == 1,
                        "Firespider literal named counter command");
                    PaintView(host);
                    hostCssCounter = BlockById(host.At, "host-css-counter");
                    Check(hostCssCounter.Mk == "NVI ", "Firespider literal named counter model");
                    Check(host.Js("be('host-generated-command',\"content:attr(data-before) / 'Command ';color:#dc2626;font-size:24px\");afterstyle('host-generated-command',\"content:attr(data-after) / ' complete';color:#2563eb;font-style:italic\")") == 2,
                        "Firespider compact and readable generated-content commands");
                    PaintView(host);
                    hostGeneratedCommand = BlockById(host.At, "host-generated-command");
                    Check(hostGeneratedCommand.X == "CMDHost generated commandREAD" && Run(hostGeneratedCommand, "be").S.Fs == 24 &&
                        Run(hostGeneratedCommand, "be").S.Ma == "Command " && Run(hostGeneratedCommand, "af").S.Ma == " complete" &&
                        host.Ax(host.Id("host-generated-command")).N == "Command Host generated command complete" &&
                        Run(hostGeneratedCommand, "af").S.I && InlineKindRect(host, hostGeneratedCommand, "be").Width > 0 &&
                        host.Js("be('host-generated-command','content:url(javascript:blocked)')") == 0,
                        "Firespider generated-content command model, geometry, and validation");
                    Check(host.Js("be('host-generated-command',\"content:url(" + hostMarker + ") / 'Host command picture '\")") == 1,
                        "Firespider generated image command");
                    PaintView(host);
                    hostGeneratedCommand = BlockById(host.At, "host-generated-command");
                    Check(Run(hostGeneratedCommand, "be").S.Mf != null && Run(hostGeneratedCommand, "be").S.Mf.Count == 1 &&
                        host.Ax(host.Id("host-generated-command")).N == "Host command picture Host generated command complete" &&
                        InlinePictureRect(host, hostGeneratedCommand, "be").Size == new System.Drawing.Size(20, 10),
                        "Firespider generated image command model, accessibility, and geometry");
                    Check(host.Js("be('host-generated-command',\"content:attr(data-missing, 'Host fallback ') / 'Fallback command '\")") == 1,
                        "Firespider generated attr fallback command");
                    PaintView(host);
                    hostGeneratedCommand = BlockById(host.At, "host-generated-command");
                    Check(hostGeneratedCommand.X == "Host fallback Host generated commandREAD" &&
                        host.Js("be('host-generated-command',\"content:attr(data-missing, bare)\")") == 0 &&
                        host.Js("be('host-generated-command',\"content:'Bad ' / open-quote\")") == 0 &&
                        host.Ax(host.Id("host-generated-command")).N == "Fallback command Host generated command complete",
                        "Firespider generated attr fallback command, validation, and accessibility");
                    Check(host.Js("qu('host-quote-command','\"<\" \">\"')") == 1,
                        "Firespider compact quotes command");
                    PaintView(host);
                    hostQuoteCommand = BlockById(host.At, "host-quote-command");
                    Check(hostQuoteCommand.X == "<Host quote command>" && hostQuoteCommand.S.Qc == 1 &&
                        host.Js("document.getElementById('host-quote-command').style.quotes='\"{\" \"}\"'") == 1 &&
                        host.Js("qu('host-quote-command','\"odd\"')") == 0,
                        "Firespider generated quote command and validation");
                    PaintView(host);
                    Check(BlockById(host.At, "host-quote-command").X == "{Host quote command}",
                        "Firespider literal DOM quotes command model");
                    hostListField.V = "Retained host marker field";
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
                    var hostViewport = BlockById(host.At, "host-viewport");
                    Check(host.Vh == 720 && hostViewport.S.Wp == 50 && hostViewport.S.Wu == 1 && hostViewport.S.Wo == -20 && hostViewport.S.Wnp == 20 && hostViewport.S.Wnu == 3 &&
                        hostViewport.S.Hp == 25 && hostViewport.S.Hu == 2 && hostViewport.S.Hxp == 40 && hostViewport.S.Hxu == 4 &&
                        BlockOuter(host, "host-viewport").Size == new System.Drawing.Size(340, 180),
                        "Firespider viewport unit model and geometry");
                    var hostFont = BlockById(host.At, "host-font");
                    var hostRem = BlockById(host.At, "host-rem");
                    Check(host.At.Rf == 20 && host.Rf == 20 && hostFont.S.Wu == 5 && hostFont.S.Hu == 5 && hostFont.S.Wnu == 6 && hostFont.S.Hxu == 6 &&
                        hostRem.S.Wu == 6 && hostRem.S.Hu == 5 && BlockOuter(host, "host-font").Size == new System.Drawing.Size(180, 72) &&
                        BlockOuter(host, "host-rem").Size == new System.Drawing.Size(200, 60),
                        "Firespider font-relative model and geometry");
                    var hostSizeFunction = BlockById(host.At, "host-size-function");
                    var hostSizeMin = BlockById(host.At, "host-size-min");
                    Check(hostSizeFunction.S.Wk == 3 && hostSizeFunction.S.Hk == 1 && hostSizeFunction.S.Wnk == 2 && hostSizeFunction.S.Hxk == 3 &&
                        hostSizeFunction.S.Wq(0) == 3 && hostSizeFunction.S.Hq(0) == 2 && hostSizeFunction.S.Wv(0, 1, 1) == 25 && hostSizeFunction.S.Wv(0, 1, 3) == 5 &&
                        hostSizeMin.S.Wk == 1 && hostSizeMin.S.Hk == 2 && hostSizeMin.S.Wq(0) == 3 &&
                        BlockOuter(host, "host-size-function").Size == new System.Drawing.Size(400, 120) && BlockOuter(host, "host-size-min").Size == new System.Drawing.Size(360, 70),
                        "Firespider bounded size function model and geometry");
                    var hostLogical = BlockById(host.At, "host-logical");
                    var hostLogicalShellRect = BlockOuter(host, "host-logical-shell");
                    var hostLogicalRect = BlockOuter(host, "host-logical");
                    Check(hostLogical.S.Wp == 50 && hostLogical.S.Wo == 20 && hostLogical.S.Wn == 180 && hostLogical.S.Wx == 240 &&
                        hostLogical.S.Hp == 50 && hostLogical.S.Hn == 100 && hostLogical.S.Hx == 160 &&
                        hostLogical.S.Lt == 24 && !hostLogical.S.Rt.HasValue && hostLogical.S.Tp == 18 && !hostLogical.S.Bm.HasValue &&
                        hostLogicalRect.Size == new System.Drawing.Size(220, 120) && hostLogicalRect.Left - hostLogicalShellRect.Left == 24 && hostLogicalRect.Top - hostLogicalShellRect.Top == 18,
                        "Firespider logical size/inset model and geometry");
                    var hostLogicalBox = BlockById(host.At, "host-logical-box");
                    var hostLogicalBoxShellRect = BlockOuter(host, "host-logical-box-shell");
                    var hostLogicalBoxRect = BlockOuter(host, "host-logical-box");
                    Check(hostLogicalBox.S.Mt == 14 && hostLogicalBox.S.Mr == 18 && hostLogicalBox.S.Mb == 16 && hostLogicalBox.S.Ml == 12 &&
                        hostLogicalBox.S.Pt == 6 && hostLogicalBox.S.Pr == 10 && hostLogicalBox.S.Pb == 7 && hostLogicalBox.S.Pl == 8 &&
                        hostLogicalBoxRect.Size == new System.Drawing.Size(118, 53) && hostLogicalBoxRect.Left - hostLogicalBoxShellRect.Left == 32 && hostLogicalBoxRect.Top - hostLogicalBoxShellRect.Top == 44,
                        "Firespider logical margin/padding model and geometry");
                    var hostLogicalBorder = BlockById(host.At, "host-logical-border");
                    var hostLogicalBorderShellRect = BlockOuter(host, "host-logical-border-shell");
                    var hostLogicalBorderRect = BlockOuter(host, "host-logical-border");
                    Check(hostLogicalBorder.S.Bt == 6 && hostLogicalBorder.S.Br == 4 && hostLogicalBorder.S.Bb == 6 && hostLogicalBorder.S.Bl == 4 &&
                        hostLogicalBorder.S.Bsv(0) == "dashed" && hostLogicalBorder.S.Bsv(1) == "solid" &&
                        hostLogicalBorder.S.Bcv(0) == "#176b4d" && hostLogicalBorder.S.Bcv(3) == "#123f8c" &&
                        hostLogicalBorderRect.Size == new System.Drawing.Size(108, 52) && hostLogicalBorderRect.Left - hostLogicalBorderShellRect.Left == 20 && hostLogicalBorderRect.Top - hostLogicalBorderShellRect.Top == 30,
                        "Firespider logical border model and geometry");
                    Check(host.Js("bi('host-logical-border-command','5px solid #176b4d');bb('host-logical-border-command','6px dashed #a13b18');bis('host-logical-border-command','7px dotted #123f8c')") == 3,
                        "Firespider compact logical border commands");
                    var hostLogicalBorderCommand = BlockById(host.At, "host-logical-border-command");
                    Check(hostLogicalBorderCommand.S.Bt == 6 && hostLogicalBorderCommand.S.Br == 5 && hostLogicalBorderCommand.S.Bb == 6 && hostLogicalBorderCommand.S.Bl == 7 &&
                        hostLogicalBorderCommand.S.Bsv(3) == "dotted" && hostLogicalBorderCommand.S.Bcv(3) == "#123f8c" &&
                        BlockOuter(host, "host-logical-border-command").Size == new System.Drawing.Size(132, 52),
                        "Firespider logical border command reflow");
                    var hostLogicalRadius = BlockById(host.At, "host-logical-radius");
                    Check(hostLogicalRadius.S.R0 == 36 && hostLogicalRadius.S.Q0 == 28 && hostLogicalRadius.S.R1 == 12 && hostLogicalRadius.S.Q1 == 9 &&
                        hostLogicalRadius.S.R2 == 18 && hostLogicalRadius.S.Q2 == 14 && hostLogicalRadius.S.R3 == 24 && hostLogicalRadius.S.Q3 == 20 &&
                        BlockOuter(host, "host-logical-radius").Size == new System.Drawing.Size(172, 82),
                        "Firespider logical corner radius model and geometry");
                    Check(host.Js("rd('host-logical-radius-command','10px 11px 12px 13px/14px 15px 16px 17px');rss('host-logical-radius-command','20px 21px');rse('host-logical-radius-command','22px 23px');res('host-logical-radius-command','24px 25px');ree('host-logical-radius-command','26px 27px')") == 5,
                        "Firespider compact logical radius commands");
                    var hostLogicalRadiusCommand = BlockById(host.At, "host-logical-radius-command");
                    Check(hostLogicalRadiusCommand.S.R0 == 20 && hostLogicalRadiusCommand.S.Q0 == 21 && hostLogicalRadiusCommand.S.R1 == 22 && hostLogicalRadiusCommand.S.Q1 == 23 &&
                        hostLogicalRadiusCommand.S.R2 == 26 && hostLogicalRadiusCommand.S.Q2 == 27 && hostLogicalRadiusCommand.S.R3 == 24 && hostLogicalRadiusCommand.S.Q3 == 25 &&
                        BlockOuter(host, "host-logical-radius-command").Size == new System.Drawing.Size(150, 64),
                        "Firespider logical radius commands reach compact host records");
                    var hostDirection = BlockById(host.At, "host-direction");
                    Check(hostDirection.S.Dr == "r" && hostDirection.S.Mr == 9 && hostDirection.S.Ml == 11 && hostDirection.S.Pr == 5 && hostDirection.S.Pl == 7 &&
                        hostDirection.S.Br == 3 && hostDirection.S.Bl == 4 && hostDirection.S.R1 == 20 && hostDirection.S.Q1 == 21 && hostDirection.S.R0 == 10 && hostDirection.S.Q0 == 11,
                        "Firespider RTL direction and logical mapping");
                    Check(host.Js("dr('host-direction','ltr')") == 1, "Firespider compact direction command");
                    hostDirection = BlockById(host.At, "host-direction");
                    Check(hostDirection.S.Dr == "l" && hostDirection.S.Ml == 9 && hostDirection.S.Mr == 11 && hostDirection.S.Pl == 5 && hostDirection.S.Pr == 7 &&
                        hostDirection.S.Bl == 3 && hostDirection.S.Br == 4 && hostDirection.S.R0 == 20 && hostDirection.S.Q0 == 21 && hostDirection.S.R1 == 10 && hostDirection.S.Q1 == 11,
                        "Firespider direction command remaps authored logical styles");
                    var hostDirectionFlex = BlockOuter(host, "host-direction-flex");
                    var hostDirectionFlexA = BlockOuter(host, "host-direction-flex-a");
                    var hostDirectionFlexB = BlockOuter(host, "host-direction-flex-b");
                    var hostDirectionFlexC = BlockOuter(host, "host-direction-flex-c");
                    Check(hostDirectionFlexA.Right == hostDirectionFlex.Right && hostDirectionFlexA.Left > hostDirectionFlexB.Left && hostDirectionFlexB.Left > hostDirectionFlexC.Left && hostDirectionFlexA.Left - hostDirectionFlexB.Right == 10,
                        "Firespider RTL flex row geometry");
                    var hostDirectionGrid = BlockOuter(host, "host-direction-grid");
                    var hostDirectionGridSpan = BlockOuter(host, "host-direction-grid-span");
                    var hostDirectionGridSide = BlockOuter(host, "host-direction-grid-side");
                    Check(hostDirectionGridSpan.Width == 110 && hostDirectionGridSpan.Right == hostDirectionGrid.Right && hostDirectionGridSide.Width == 80 && hostDirectionGridSide.Left - hostDirectionGrid.Left == 20 && hostDirectionGridSpan.Left - hostDirectionGridSide.Right == 10,
                        "Firespider RTL grid line and span geometry");
                    var hostDirectionPlain = BlockById(host.At, "host-direction-plain");
                    var hostDirectionAmber = InlineTextRect(host, hostDirectionPlain, "Amber");
                    var hostDirectionBronze = InlineTextRect(host, hostDirectionPlain, "Bronze");
                    var hostDirectionCobalt = InlineTextRect(host, hostDirectionPlain, "Cobalt");
                    Check(hostDirectionAmber.Right == BlockContent(host, "host-direction-plain").Right &&
                        hostDirectionAmber.Left > hostDirectionBronze.Left && hostDirectionBronze.Left > hostDirectionCobalt.Left,
                        "Firespider plain RTL inline geometry");
                    var hostInlineFirst = InlineElementBounds(host, "host-inline-first");
                    var hostInlineSecond = InlineElementBounds(host, "host-inline-second");
                    var hostInlineThird = InlineElementBounds(host, "host-inline-third");
                    Check(hostInlineFirst.Right == BlockContent(host, "host-direction-inline").Right &&
                        hostInlineFirst.Left > hostInlineSecond.Left && hostInlineSecond.Left > hostInlineThird.Left,
                        "Firespider mixed RTL inline geometry");
                    var hostDirectionList = BlockById(host.At, "host-direction-list");
                    var hostDirectionMarker = InlineTextRect(host, hostDirectionList, "4.");
                    Check(hostDirectionList.Mk == "4." && hostDirectionMarker.Left > BlockContent(host, "host-direction-list").Right &&
                        InlineElementBounds(host, "host-direction-list-text").Right == BlockContent(host, "host-direction-list").Right,
                        "Firespider RTL ordered outside-marker geometry");
                    var hostDirectionOverflow = BlockById(host.At, "host-direction-overflow");
                    var hostOverflowContent = BlockContent(host, "host-direction-overflow");
                    var hostOverflowMarker = EllipsisRect(host, "host-direction-overflow");
                    var hostOverflowPrefix = EllipsisPrefixBounds(host, "host-direction-overflow");
                    var hostOverflowScroll = host.Sy;
                    host.Sc(hostOverflowMarker.Top - host.Sy - 40);
                    PaintView(host);
                    var hostOverflowPoint = new System.Drawing.Point(hostOverflowMarker.Left + hostOverflowMarker.Width / 2,
                        hostOverflowMarker.Top - host.Sy + hostOverflowMarker.Height / 2);
                    var hostOverflowLinked = HasFragmentHitAt(host, "#state", hostOverflowPoint);
                    Check(hostDirectionOverflow.S.Dr == "r" && hostDirectionOverflow.S.To == "e" &&
                        hostOverflowContent.Contains(hostOverflowMarker) && hostOverflowMarker.Right < hostOverflowContent.Left + hostOverflowContent.Width / 2 &&
                        hostOverflowPrefix[1].Right == hostOverflowPrefix[0].Right && hostOverflowPrefix[1].Width <= hostOverflowPrefix[0].Width &&
                        hostOverflowLinked,
                        "Firespider RTL ellipsis paint and retained link geometry: content=" + hostOverflowContent +
                        ", marker=" + hostOverflowMarker + ", layout=" + hostOverflowPrefix[0] + ", paint=" + hostOverflowPrefix[1] +
                        ", point=" + hostOverflowPoint);
                    var hostAutomation = typeof(V).GetMethod("AutomationText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(host, null);
                    var hostAutomationText = (string)hostAutomation.GetType().GetField("X").GetValue(hostAutomation);
                    var hostRangeFirst = AutomationRangeRect(host, hostAutomation, hostAutomationText, "RangeFirst");
                    var hostRangeSecond = AutomationRangeRect(host, hostAutomation, hostAutomationText, "RangeSecond");
                    var hostRangeThird = AutomationRangeRect(host, hostAutomation, hostAutomationText, "RangeThird");
                    var hostRangePartial = AutomationRangeRect(host, hostAutomation, hostAutomationText, "Range");
                    Check(hostAutomationText.IndexOf('\u2026') < 0 && hostRangeFirst.Left > hostRangeSecond.Left && hostRangeSecond.Left > hostRangeThird.Left &&
                        hostRangePartial.Width > 0 && hostRangePartial.Width < hostRangeFirst.Width && hostRangePartial.Right >= hostRangeFirst.Right - 1 &&
                        AutomationWordAtPoint(host, hostAutomation, hostRangeFirst) == "RangeFirst" && AutomationWordAtPoint(host, hostAutomation, hostRangeThird) == "RangeThird",
                        "Firespider RTL TextPattern range and point geometry");
                    var hostDirectionCommandContent = BlockContent(host, "host-direction-command");
                    var hostDirectionCommandMarker = EllipsisRect(host, "host-direction-command");
                    Check(hostDirectionCommandMarker.Left > hostDirectionCommandContent.Left + hostDirectionCommandContent.Width / 2 &&
                        host.Js("dr('host-direction-command','rtl')") == 1,
                        "Firespider LTR ellipsis command baseline");
                    hostDirectionCommandContent = BlockContent(host, "host-direction-command");
                    hostDirectionCommandMarker = EllipsisRect(host, "host-direction-command");
                    Check(hostDirectionCommandMarker.Right < hostDirectionCommandContent.Left + hostDirectionCommandContent.Width / 2,
                        "Firespider direction command remaps ellipsis geometry");
                    host.Sc(hostOverflowScroll - host.Sy);
                    PaintView(host);
                    Check(host.Js("dr('host-direction-flex','ltr')") == 1, "Firespider direction layout command");
                    hostDirectionFlex = BlockOuter(host, "host-direction-flex");
                    hostDirectionFlexA = BlockOuter(host, "host-direction-flex-a");
                    Check(hostDirectionFlexA.Left == hostDirectionFlex.Left, "Firespider direction command remaps flex geometry");
                    var hostResponsiveField = FieldById(host.At, "host-auto");
                    hostResponsiveField.V = "Retained responsive host value";
                    Check(host.Vw > 500 && host.At.Vw == host.Vw && host.Dg().Vw == host.Vw && HasBlock(host.At, "host-box") && !HasBlock(host.At, "host-media"), "Firespider wide viewport aliases and media state");
                    Check(host.At.Vh == host.Vh && host.Dg().Vh == host.Vh &&
                        (host.Vh >= host.Vw ? HasBlock(host.At, "host-viewport-portrait") && !HasBlock(host.At, "host-viewport-landscape") :
                        HasBlock(host.At, "host-viewport-landscape") && !HasBlock(host.At, "host-viewport-portrait")),
                        "Firespider viewport-height aliases and orientation state");
                    Check(host.Vw * 3L >= host.Vh * 4L ? HasBlock(host.At, "host-ratio-wide") && !HasBlock(host.At, "host-ratio-tall") :
                        host.Vw <= host.Vh ? HasBlock(host.At, "host-ratio-tall") && !HasBlock(host.At, "host-ratio-wide") :
                        !HasBlock(host.At, "host-ratio-wide") && !HasBlock(host.At, "host-ratio-tall"),
                        "Firespider initial aspect-ratio media state");
                    var initialRangeWide = host.Vw >= 700 && host.Vw < 1000 && host.Vw * 2L >= host.Vh * 3L && host.Vw < host.Vh * 2L;
                    var initialRangeTall = host.Vw <= 500 && host.Vw < host.Vh;
                    Check(initialRangeWide ? HasBlock(host.At, "host-range-wide") && !HasBlock(host.At, "host-range-tall") :
                        initialRangeTall ? HasBlock(host.At, "host-range-tall") && !HasBlock(host.At, "host-range-wide") :
                        !HasBlock(host.At, "host-range-wide") && !HasBlock(host.At, "host-range-tall"),
                        "Firespider initial media range state");
                    Check(host.Vw >= 800 ? HasBlock(host.At, "host-support-wide") && !HasBlock(host.At, "host-support-narrow") :
                        host.Vw <= 500 ? HasBlock(host.At, "host-support-narrow") && !HasBlock(host.At, "host-support-wide") :
                        !HasBlock(host.At, "host-support-wide") && !HasBlock(host.At, "host-support-narrow"),
                        "Firespider initial supports query state");
                    var hostVariable = BlockById(host.At, "host-var");
                    Check(hostVariable.S.F == "#0f4c81" && hostVariable.S.W == 155 && hostVariable.S.Pl == 7 && hostVariable.S.Bl == 4 && hostVariable.S.B && hostVariable.S.Vc == 4 && hostVariable.S.Cv("--host-color") == "#0f4c81" && hostVariable.S.Cv("--host-width") == "155px" && host.Dg().Vc == 4 && host.Dg().Ic == 3, "Firespider functional selector, important, and custom property values");
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
                    Check(host.Vh == 560 && host.At.Vh == 560 && host.Dg().Vh == 560 && HasBlock(host.At, "host-viewport-portrait") && !HasBlock(host.At, "host-viewport-landscape") && !HasBlock(host.At, "host-viewport-short"),
                        "Firespider portrait viewport-height resize");
                    Check(HasBlock(host.At, "host-ratio-tall") && !HasBlock(host.At, "host-ratio-wide") && BlockById(host.At, "host-ratio-tall").S.W == 164,
                        "Firespider tall aspect-ratio media state");
                    Check(HasBlock(host.At, "host-range-tall") && !HasBlock(host.At, "host-range-wide") &&
                        BlockById(host.At, "host-range-tall").S.W == 166 && Has(host.Dg().Cs, "@m<=500&@r<1/1"),
                        "Firespider tall media range state and diagnostic");
                    Check(HasBlock(host.At, "host-support-narrow") && !HasBlock(host.At, "host-support-wide") &&
                        BlockById(host.At, "host-support-narrow").S.W == 168 && Has(host.Dg().Cs, "@s(not(display:ruby))&@m<=500"),
                        "Firespider narrow supports query state and diagnostic");
                    Check(BlockById(host.At, "host-var").S.W == 110 && BlockById(host.At, "host-var").S.Pl == 4 && BlockById(host.At, "host-var").S.Cv("--host-width") == "110px", "Firespider responsive custom properties");
                    Check(Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")) && hostResponsiveField.V == "Retained responsive host value", "Firespider responsive resize retains field state");
                    Check(BlockById(host.At, "host-transform-responsive").S.Tt == "l" && BlockById(host.At, "host-transform-responsive").X == "responsive host mixed" &&
                        Object.ReferenceEquals(hostTransformField, FieldById(host.At, "host-transform-field")) && hostTransformField.V == "Retained Host MiXeD",
                        "Firespider responsive text transform and raw field retention");
                    Check(BlockById(host.At, "host-spacing-responsive").S.Ls == -1 && BlockById(host.At, "host-spacing-responsive").S.Wg == 0 &&
                        Object.ReferenceEquals(hostSpacingField, FieldById(host.At, "host-spacing-field")) && hostSpacingField.V == "Retained host raw words",
                        "Firespider responsive text spacing and raw field retention");
                    var hostIndentResponsive = BlockById(host.At, "host-indent-responsive");
                    Check(hostIndentResponsive.S.Ti == 30 && hostIndentResponsive.S.Tip &&
                        InlineTextRect(host, hostIndentResponsive, "HostResponsive").Left == BlockContent(host, "host-indent-responsive").Left +
                        (int)Math.Round(BlockContent(host, "host-indent-responsive").Width * 30 / 100D),
                        "Firespider responsive percentage text indent");
                    hostTextShadowResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostTextShadowResponsive.S.Ts != null && hostTextShadowResponsive.S.Ts.X == -4 && hostTextShadowResponsive.S.Ts.Y == 3 && hostTextShadowResponsive.S.Ts.B == 4 && hostTextShadowResponsive.S.Ts.C == "#7c3aed",
                        "Firespider responsive text shadow");
                    hostDecorationResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostDecorationResponsive.S.Td == "s" && hostDecorationResponsive.S.Tc == "#7c3aed" && hostDecorationResponsive.S.Tds == "w" && hostDecorationResponsive.S.Tw == 3 &&
                        hostDecorationResponsive.S.Uo == 25 && hostDecorationResponsive.S.Uop,
                        "Firespider responsive text decoration and underline offset");
                    hostAlignLastResponsive = BlockById(host.At, "host-align-last-responsive");
                    Check(hostAlignLastResponsive.S.Tl == "e" && InlineTextRect(host, hostAlignLastResponsive, "ResponsiveLast").Right == BlockContent(host, "host-align-last-responsive").Right,
                        "Firespider responsive text align last");
                    hostAlignResponsive = BlockById(host.At, "host-align-responsive");
                    Check(hostAlignResponsive.S.Ta == "e" && InlineTextRect(host, hostAlignResponsive, "ResponsiveAlign").Left == BlockContent(host, "host-align-responsive").Left,
                        "Firespider responsive logical text align");
                    hostWrapResponsive = BlockById(host.At, "host-wrap-responsive");
                    Check(hostWrapResponsive.S.Wt == "b" && InlineLines(host, hostWrapResponsive).Length > 1,
                        "Firespider responsive balanced text wrap");
                    Check(BlockById(host.At, "host-overflow-responsive").S.To == "e" && EllipsisCount(host, "host-overflow-responsive") == 1,
                        "Firespider narrow text overflow marker");
                    hostClampResponsive = BlockById(host.At, "host-clamp-responsive");
                    Check(hostClampResponsive.S.Lc == 1 && InlineLines(host, hostClampResponsive).Length == 1 && EllipsisCount(host, "host-clamp-responsive") == 1,
                        "Firespider narrow line clamp geometry");
                    hostListResponsive = BlockById(host.At, "host-list-responsive");
                    Check(hostListResponsive.S.Lst == "s" && hostListResponsive.S.Lp == "i" && hostListResponsive.Mk == "\u25aa" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")) && hostListField.V == "Retained host marker field",
                        "Firespider responsive list marker and field retention");
                    hostListImageResponsive = BlockById(host.At, "host-list-image-responsive");
                    Check(hostListImageResponsive.S.Li != null && InlineKindRect(host, hostListImageResponsive, "lm").Right < BlockContent(host, "host-list-image-responsive").Left,
                        "Firespider responsive list image marker");
                    hostMarkerResponsive = BlockById(host.At, "host-marker-responsive");
                    Check(hostMarkerResponsive.Mk == "Narrow " && hostMarkerResponsive.Ms.F == "#7c3aed" && hostMarkerResponsive.Ms.Fs == 20 &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider responsive marker pseudo-element and field retention");
                    hostGeneratedResponsive = BlockById(host.At, "host-generated-responsive");
                    Check(hostGeneratedResponsive.X == "Generated responsive Narrow" && Run(hostGeneratedResponsive, "af").S.Fs == 20 &&
                        host.Ax(host.Id("host-generated-responsive")).N == "Generated responsive Narrow accessible" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider responsive generated content and field retention");
                    hostGeneratedPictureResponsive = BlockById(host.At, "host-generated-picture-responsive");
                    Check(Run(hostGeneratedPictureResponsive, "be").S.Mf != null &&
                        host.Ax(host.Id("host-generated-picture-responsive")).N == "Narrow picture Host picture responsive" &&
                        InlinePictureRect(host, hostGeneratedPictureResponsive, "be").Size == new System.Drawing.Size(20, 10),
                        "Firespider responsive generated image model and geometry");
                    hostQuoteResponsive = BlockById(host.At, "host-quote-responsive");
                    Check(hostQuoteResponsive.X == "[Host responsive quote]" && hostQuoteResponsive.S.Qo(0) == "[" &&
                        InlineKindRect(host, hostQuoteResponsive, "be").Width > 0 &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider responsive generated quotes and field retention");
                    host.Size = new System.Drawing.Size(900, 480);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw == 900 && host.Vh == 480 && host.At.Vh == 480 && host.Dg().Vh == 480 &&
                        HasBlock(host.At, "host-viewport-landscape") && HasBlock(host.At, "host-viewport-short") && !HasBlock(host.At, "host-viewport-portrait") &&
                        Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")) && hostResponsiveField.V == "Retained responsive host value",
                        "Firespider short landscape media state and retained field");
                    Check(HasBlock(host.At, "host-ratio-wide") && !HasBlock(host.At, "host-ratio-tall") && BlockById(host.At, "host-ratio-wide").S.W == 204 &&
                        Has(host.Dg().Cs, "@r>=4/3&@o=l"), "Firespider wide aspect-ratio media state and diagnostic");
                    Check(HasBlock(host.At, "host-range-wide") && !HasBlock(host.At, "host-range-tall") &&
                        BlockById(host.At, "host-range-wide").S.W == 206 &&
                        Has(host.Dg().Cs, "@m>=700&@m<1000&@r>=3/2&@r<2/1"),
                        "Firespider wide media range state and diagnostic");
                    Check(HasBlock(host.At, "host-support-wide") && !HasBlock(host.At, "host-support-narrow") &&
                        BlockById(host.At, "host-support-wide").S.W == 208 &&
                        Has(host.Dg().Cs, "@s(display:grid)&@s(selector(:has(*)))&@m>=800"),
                        "Firespider wide supports query state and diagnostic");
                    host.Size = new System.Drawing.Size(720, 560);
                    Application.DoEvents();
                    PaintView(host);
                    Check(host.Vw > 500 && HasBlock(host.At, "host-box") && !HasBlock(host.At, "host-media") && BlockById(host.At, "host-var").S.W == 155 && Object.ReferenceEquals(hostResponsiveField, FieldById(host.At, "host-auto")), "Firespider wide media restore");
                    Check(!HasBlock(host.At, "host-ratio-wide") && !HasBlock(host.At, "host-ratio-tall"), "Firespider intermediate aspect-ratio restore");
                    Check(!HasBlock(host.At, "host-range-wide") && !HasBlock(host.At, "host-range-tall"), "Firespider intermediate media range restore");
                    Check(!HasBlock(host.At, "host-support-wide") && !HasBlock(host.At, "host-support-narrow"), "Firespider intermediate supports query restore");
                    Check(BlockById(host.At, "host-transform-responsive").S.Tt == "u" && BlockById(host.At, "host-transform-responsive").X == "RESPONSIVE HOST MIXED" &&
                        Object.ReferenceEquals(hostTransformField, FieldById(host.At, "host-transform-field")), "Firespider wide text transform restore");
                    Check(BlockById(host.At, "host-spacing-responsive").S.Ls == 5 && BlockById(host.At, "host-spacing-responsive").S.Wg == 10 &&
                        Object.ReferenceEquals(hostSpacingField, FieldById(host.At, "host-spacing-field")), "Firespider wide text spacing restore");
                    hostGeneratedResponsive = BlockById(host.At, "host-generated-responsive");
                    Check(hostGeneratedResponsive.X == "Generated responsive Wide" &&
                        host.Ax(host.Id("host-generated-responsive")).N == "Generated responsive Wide accessible" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider wide generated content restore");
                    hostGeneratedPictureResponsive = BlockById(host.At, "host-generated-picture-responsive");
                    Check(Run(hostGeneratedPictureResponsive, "be").S.Mf == null &&
                        hostGeneratedPictureResponsive.X == "WIDE Host picture responsive" &&
                        host.Ax(host.Id("host-generated-picture-responsive")).N == "Wide picture Host picture responsive",
                        "Firespider wide generated image restore");
                    hostQuoteResponsive = BlockById(host.At, "host-quote-responsive");
                    Check(hostQuoteResponsive.X == "(Host responsive quote)" && hostQuoteResponsive.S.Qo(0) == "(" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")),
                        "Firespider wide generated quote restore");
                    hostIndentResponsive = BlockById(host.At, "host-indent-responsive");
                    Check(hostIndentResponsive.S.Ti == 12 && !hostIndentResponsive.S.Tip &&
                        InlineTextRect(host, hostIndentResponsive, "HostResponsive").Left == BlockContent(host, "host-indent-responsive").Left + 12,
                        "Firespider wide text indent restore");
                    hostTextShadowResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostTextShadowResponsive.S.Ts != null && hostTextShadowResponsive.S.Ts.X == 2 && hostTextShadowResponsive.S.Ts.Y == 2 && hostTextShadowResponsive.S.Ts.B == 0 && hostTextShadowResponsive.S.Ts.C == "#64748b",
                        "Firespider wide text shadow restore");
                    hostDecorationResponsive = BlockById(host.At, "host-text-shadow-responsive");
                    Check(hostDecorationResponsive.S.Td == "u" && hostDecorationResponsive.S.Tc == "#64748b" && hostDecorationResponsive.S.Tds == "o" && hostDecorationResponsive.S.Tw == 1 &&
                        hostDecorationResponsive.S.Uo == 2 && !hostDecorationResponsive.S.Uop,
                        "Firespider wide text decoration and underline offset restore");
                    hostAlignLastResponsive = BlockById(host.At, "host-align-last-responsive");
                    Check(hostAlignLastResponsive.S.Tl == "l" && InlineTextRect(host, hostAlignLastResponsive, "ResponsiveLast").Left == BlockContent(host, "host-align-last-responsive").Left,
                        "Firespider wide text align last restore");
                    hostAlignResponsive = BlockById(host.At, "host-align-responsive");
                    var hostAlignResponsiveLine = InlineTextRect(host, hostAlignResponsive, "ResponsiveAlign");
                    Check(hostAlignResponsive.S.Ta == "c" &&
                        Math.Abs(hostAlignResponsiveLine.Left + hostAlignResponsiveLine.Right - BlockContent(host, "host-align-responsive").Left - BlockContent(host, "host-align-responsive").Right) <= 2,
                        "Firespider wide text align restore");
                    hostWrapResponsive = BlockById(host.At, "host-wrap-responsive");
                    Check(hostWrapResponsive.S.Wt == "" && InlineLines(host, hostWrapResponsive).Length > 1,
                        "Firespider wide text wrap restore");
                    Check(BlockById(host.At, "host-overflow-responsive").S.To == "c" && EllipsisCount(host, "host-overflow-responsive") == 0,
                        "Firespider wide text overflow restore");
                    hostClampResponsive = BlockById(host.At, "host-clamp-responsive");
                    Check(hostClampResponsive.S.Lc == 0 && InlineLines(host, hostClampResponsive).Length > 1 && EllipsisCount(host, "host-clamp-responsive") == 0,
                        "Firespider wide line clamp restore");
                    hostListResponsive = BlockById(host.At, "host-list-responsive");
                    Check(hostListResponsive.S.Lst == "c" && hostListResponsive.S.Lp == "o" && hostListResponsive.Mk == "\u25e6" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")) && hostListField.V == "Retained host marker field",
                        "Firespider wide list marker restore");
                    hostListImageResponsive = BlockById(host.At, "host-list-image-responsive");
                    Check(hostListImageResponsive.S.Li == null && InlineTextRect(host, hostListImageResponsive, "\u25e6").Right < BlockContent(host, "host-list-image-responsive").Left,
                        "Firespider wide list image fallback restore");
                    hostMarkerResponsive = BlockById(host.At, "host-marker-responsive");
                    Check(hostMarkerResponsive.Mk == "Wide " && hostMarkerResponsive.Ms.F == "#64748b" &&
                        Object.ReferenceEquals(hostListField, FieldById(host.At, "host-list-field")) && hostListField.V == "Retained host marker field",
                        "Firespider wide marker pseudo-element restore");
                    Check(BlockById(host.At, "host-progress").Vm.R == 0.4D && BlockById(host.At, "host-meter").Vm.R == 0.75D, "Firespider compact gauge models");
                    Check(host.At.Dvs.Count == 4 && ValueById(host.At, "host-output").V == "Host total" && ValueById(host.At, "host-data").V == "lea-host", "Firespider compact output and data models");
                    Check(ValueById(host.At, "host-time").K == "t" && ValueById(host.At, "host-time").Dt == "2026-08-21", "Firespider compact time model");
                    Check(ValueById(host.At, "host-edit").K == "i" && ValueById(host.At, "host-edit").U == new Uri(host.At.Bu, "go-target.html#host") && BlockById(host.At, "host-edit").S.Td.IndexOf('u') >= 0, "Firespider compact edit model");
                    var fragmentPage = host.At;
                    var fragmentRequests = host.Dg().Rs.Count;
                    host.GoA("#state").GetAwaiter().GetResult();
                    PaintView(host);
                    Check(Object.ReferenceEquals(fragmentPage, host.At) && host.U.Fragment == "#state" && host.Ft("#state") != null, "Firespider same-page fragment route");
                    Check(host.Dg().Rs.Count == fragmentRequests, "Firespider fragment route performs no request");
                    Check(host.Fa && host.Fj >= 2 && host.Fy >= 0, "Firespider retained fragment diagnostics");
                    var fragmentJumps = host.Fj;
                    var fragmentY = host.Fy;
                    host.Sc(1);
                    Check(!host.Fa && host.Fj == fragmentJumps && host.Fy == fragmentY, "Firespider manual scroll releases fragment anchor");
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
                    Check(host.Js("wd('host-calculated','min(max(100px,200px),400px)'); document.getElementById('host-calculated').style.width='101%'") == 0 && BlockById(host.At, "host-calculated").S.Wp == 100,
                        "Firespider calculated width boundary");
                    Check(host.Js("ht('host-height','50%')") == 1 && BlockById(host.At, "host-height").S.Hp == 50, "Firespider compact height command");
                    PaintView(host);
                    Check(BlockOuter(host, "host-height").Height == 160, "Firespider compact percentage height reflow");
                    Check(host.Js("document.getElementById('host-height').style.height=''") == 1 && BlockById(host.At, "host-height").S.Hp == 100 && BlockById(host.At, "host-height").S.F == "#123f8c",
                        "Firespider literal DOM height reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-height").Height == hostHeightTall, "Firespider authored calculated height restore");
                    Check(host.Js("ht('host-height','min(max(100px,200px),400px)'); document.getElementById('host-height').style.height='101%'") == 0 && BlockById(host.At, "host-height").S.Hp == 100,
                        "Firespider calculated height boundary");
                    Check(host.Js("wd('host-viewport','25vh'); ht('host-viewport','25vw')") == 2 &&
                        BlockById(host.At, "host-viewport").S.Wu == 2 && BlockById(host.At, "host-viewport").S.Hu == 1,
                        "Firespider compact viewport unit commands");
                    PaintView(host);
                    Check(BlockOuter(host, "host-viewport").Size == new System.Drawing.Size(140, 180), "Firespider viewport command reflow");
                    Check(host.Js("document.getElementById('host-viewport').style.width=''; document.getElementById('host-viewport').style.height=''") == 2,
                        "Firespider literal viewport unit reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-viewport").Size == new System.Drawing.Size(340, 140), "Firespider authored viewport unit restore");
                    Check(host.Js("wd('host-viewport','101vw'); ht('host-viewport','20dvh')") == 0 && BlockById(host.At, "host-viewport").S.Wu == 1,
                        "Firespider viewport unit boundary");
                    Check(host.Js("wd('host-font','8rem'); ht('host-font','5em')") == 2 && BlockById(host.At, "host-font").S.Wu == 6 && BlockById(host.At, "host-font").S.Hu == 5,
                        "Firespider compact font-relative commands");
                    PaintView(host);
                    Check(BlockOuter(host, "host-font").Size == new System.Drawing.Size(160, 90), "Firespider font-relative command reflow");
                    Check(host.Js("document.getElementById('host-font').style.width=''; document.getElementById('host-font').style.height=''") == 2,
                        "Firespider literal font-relative reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-font").Size == new System.Drawing.Size(180, 72), "Firespider authored font-relative restore");
                    Check(host.Js("wd('host-font','101em'); ht('host-font','1.5rem')") == 0 && BlockById(host.At, "host-font").S.Wu == 5,
                        "Firespider font-relative boundary");
                    Check(host.Js("wd('host-size-function','min(30rem,350px)'); ht('host-size-function','max(5em,90px)')") == 2 &&
                        BlockById(host.At, "host-size-function").S.Wk == 1 && BlockById(host.At, "host-size-function").S.Hk == 2,
                        "Firespider compact size function commands");
                    PaintView(host);
                    Check(BlockOuter(host, "host-size-function").Size == new System.Drawing.Size(350, 90), "Firespider compact size function reflow");
                    Check(host.Js("document.getElementById('host-size-function').style.width='max(10rem,300px)'; document.getElementById('host-size-function').style.height='clamp(60px,4em,100px)'") == 2,
                        "Firespider literal size function commands");
                    PaintView(host);
                    Check(BlockOuter(host, "host-size-function").Size == new System.Drawing.Size(300, 64), "Firespider literal size function reflow");
                    Check(host.Js("document.getElementById('host-size-function').style.width=''; document.getElementById('host-size-function').style.height=''") == 2,
                        "Firespider literal size function reset");
                    PaintView(host);
                    var restoredHostSize = BlockOuter(host, "host-size-function").Size;
                    Check(restoredHostSize == new System.Drawing.Size(400, 112), "Firespider authored size function restore in live host viewport: " + restoredHostSize);
                    Check(host.Js("wd('host-size-function','min(100px)'); ht('host-size-function','clamp(20px,30px)')") == 0 &&
                        BlockById(host.At, "host-size-function").S.Wk == 3 && BlockById(host.At, "host-size-function").S.Hk == 1,
                        "Firespider size function boundary");
                    Check(host.Js("is('host-logical','200px'); bs('host-logical','90px')") == 2 &&
                        BlockById(host.At, "host-logical").S.W == 200 && BlockById(host.At, "host-logical").S.H == 90,
                        "Firespider short logical size commands");
                    PaintView(host);
                    Check(BlockOuter(host, "host-logical").Size == new System.Drawing.Size(200, 100), "Firespider short logical size reflow and minimum constraint");
                    Check(host.Js("document.getElementById('host-logical').style.inlineSize='180px'; document.getElementById('host-logical').style.blockSize='80px'") == 2,
                        "Firespider familiar logical DOM styles");
                    PaintView(host);
                    Check(BlockOuter(host, "host-logical").Size == new System.Drawing.Size(180, 100), "Firespider familiar logical DOM reflow and minimum constraint");
                    Check(host.Js("document.getElementById('host-logical').style.inlineSize=''; document.getElementById('host-logical').style.blockSize=''") == 2,
                        "Firespider logical DOM reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-logical").Size == new System.Drawing.Size(220, 120) && BlockById(host.At, "host-logical").S.F == "#123f8c",
                        "Firespider authored logical size restore");
                    Check(host.Js("is('host-logical','101%'); bs('host-logical','1.5em')") == 0 && BlockById(host.At, "host-logical").S.Wp == 50,
                        "Firespider logical size boundary");
                    Check(host.Js("mi('host-logical-box','21px 22px'); mb('host-logical-box','23px 24px'); pi('host-logical-box','4px 5px'); pb('host-logical-box','6px 7px')") == 4,
                        "Firespider short logical box commands");
                    PaintView(host);
                    hostLogicalBox = BlockById(host.At, "host-logical-box");
                    Check(hostLogicalBox.S.Mt == 23 && hostLogicalBox.S.Mr == 22 && hostLogicalBox.S.Mb == 24 && hostLogicalBox.S.Ml == 21 &&
                        hostLogicalBox.S.Pt == 6 && hostLogicalBox.S.Pr == 5 && hostLogicalBox.S.Pb == 7 && hostLogicalBox.S.Pl == 4 &&
                        BlockOuter(host, "host-logical-box").Size == new System.Drawing.Size(109, 53),
                        "Firespider short logical box reflow");
                    Check(host.Js("document.getElementById('host-logical-box').style.marginInline='9px 11px';document.getElementById('host-logical-box').style.marginBlock='13px 15px';document.getElementById('host-logical-box').style.paddingInline='5px 7px';document.getElementById('host-logical-box').style.paddingBlock='3px 4px'") == 4,
                        "Firespider familiar logical box DOM styles");
                    PaintView(host);
                    Check(BlockOuter(host, "host-logical-box").Size == new System.Drawing.Size(112, 47), "Firespider familiar logical box DOM reflow");
                    Check(host.Js("document.getElementById('host-logical-box').style.marginInline='';document.getElementById('host-logical-box').style.marginBlock='';document.getElementById('host-logical-box').style.paddingInline='';document.getElementById('host-logical-box').style.paddingBlock=''") == 4,
                        "Firespider logical box DOM reset");
                    PaintView(host);
                    Check(BlockOuter(host, "host-logical-box").Size == new System.Drawing.Size(118, 53) && BlockById(host.At, "host-logical-box").S.F == "#123f8c",
                        "Firespider authored logical box restore");
                    Check(host.Js("mi('host-logical-box','1px 2px 3px'); pi('host-logical-box','-1px'); mis('host-logical-box','2px 3px'); mie('host-logical-box','auto')") == 0 &&
                        BlockById(host.At, "host-logical-box").S.Ml == 12 && BlockById(host.At, "host-logical-box").S.Pr == 10,
                        "Firespider logical box boundary");
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
                    Check(diagnostics != null && diagnostics.N > 0 && diagnostics.J == 123 && diagnostics.Rs.Count > 0,
                        "Firespider Lea diagnostics: nodes=" + (diagnostics == null ? -1 : diagnostics.N) +
                        ", rules=" + (diagnostics == null ? -1 : diagnostics.J) +
                        ", requests=" + (diagnostics == null ? -1 : diagnostics.Rs.Count));
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
                    WaitFor(delegate
                    {
                        using (var image = RenderView(host))
                        {
                            var rect = hostMapRect;
                            rect.Offset(0, -host.Sy);
                            return ColorDistance(image.GetPixel(rect.Left + 20, rect.Top + 45), System.Drawing.Color.FromArgb(220, 38, 38)) < 20 &&
                                ColorDistance(image.GetPixel(rect.Right - 20, rect.Top + 45), System.Drawing.Color.FromArgb(37, 99, 235)) < 20;
                        }
                    }, "Firespider fitted image readiness");
                    Check(host.Iac && host.Ind && host.Ial && host.Irt && host.Ili && host.Ij && host.Iad &&
                        host.Icl == 384L * 1024 * 1024 && host.Ihm > 0 && host.Icm >= host.Ihm + host.Ipk && host.Icm <= host.Icl,
                        "Firespider image decoder AppContainer and combined-memory boundary");
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

                Console.WriteLine("Firespider CSS interaction and paint smoke passed: compact before/after generated content with attr fallbacks, accessible alternatives, inherited generated quotes, and sandboxed bitmap fragments, be/af/ms/qu aliases, text-indent, text-wrap/mode/style, line-clamp, list-style/type/image/position, marker pseudo-element styling with generated attr/fallback, image, and named/list-counter content, counter reset/increment/set aliases, text-align/all/last, text-decoration, text-underline-offset, text-shadow, text-overflow, text-spacing, text-transform, pointer, outline, radius, box-shadow, gradient, bitmap-background, object-fit, and aspect-ratio aliases reached the compiled host.");
                Console.WriteLine("Firespider calculated height smoke passed: compact ht, literal style.height, definite parent resolution, shared host geometry, reset, and strict bounds.");
                Console.WriteLine("Firespider viewport unit smoke passed: vw/vh/vmin/vmax, calc, compact commands, literal reset, live host geometry, and strict bounds.");
                Console.WriteLine("Firespider font-relative unit smoke passed: em/rem, root font alias, compact commands, literal reset, shared host geometry, and strict bounds.");
                Console.WriteLine("Firespider size function smoke passed: bounded min/max/clamp, compact aliases, literal commands, reset, shared host geometry, and strict bounds.");
                Console.WriteLine("Firespider logical sizing smoke passed: bounded horizontal inline/block sizes and insets, is/bs, literal DOM styles, shared host geometry, reset, and strict bounds.");
                Console.WriteLine("Firespider logical box smoke passed: bounded horizontal margins/padding, mi/mb/pi/pb, literal DOM styles, shared host geometry, reset, and strict bounds.");
                Console.WriteLine("Firespider logical border smoke passed: bounded horizontal axes/edges, per-edge style/color, bi/bb/bis, shared host geometry, and command reflow.");
                Console.WriteLine("Firespider logical radius smoke passed: four horizontal corner mappings, rd/rss/rse/res/ree commands, shared geometry, and compact host records.");
                Console.WriteLine("Firespider direction smoke passed: inherited RTL mapping, direction-aware flex/grid/inline, list-marker, inline-end ellipsis, and TextPattern range geometry reached the compiled host.");
                Console.WriteLine("Firespider image sandbox smoke passed: zero-capability AppContainer, combined host/helper admission, exact helper ACL, restricted token, low integrity, and Job limits reached the compiled host.");
                Console.WriteLine("Firespider initial fragment stability smoke passed: Fa/Fj/Fy diagnostics and manual-scroll release reached the compiled host.");

                    Console.WriteLine("Firespider Lea smoke passed: bounded inline before/after/marker generated content with quoted attr fallbacks, visual/accessibility alternatives, generated quote depth, and sandboxed bitmap fragments, be/af/ms/qu aliases, accessibility, links, counters, tables, responsive paint, and retained fields; bounded horizontal logical margin/padding properties with twelve compact aliases, bounded horizontal logical sizing/insets with short is/bs and familiar DOM aliases, bounded sticky positioning with compact k and shared host geometry, bounded percentage/calc widths with compact wd and literal DOM reflow, bounded display alias and literal location requests, literal bounded DOM translation, inherited cursor and pointer-event aliases with host hit gating and fallthrough, bounded CSS text decoration and inherited underline offsets with compact aliases and host paint, bounded CSS outlines with compact aliases and exact non-layout host paint, bounded CSS aspect ratios with compact aliases and exact host geometry/paint, bounded CSS object-fit and object-position with compact aliases, exact fitted paint, and image-map keyboard/accessibility geometry, bounded CSS bitmap backgrounds with compact aliases and exact host paint, bounded inline SVG aliases, winding, stroke styles, geometry, links, and graphic accessibility, bounded has relational selectors with strict lists, maximum specificity, descendant/child/adjacent and complex host relationships, focus rebuilds, retained fields, queries, Inspector, and geometry, bounded nth-child of-S filters with strict lists, maximum specificity, forward/reverse computed styles, queries, Inspector, and host geometry, bounded precompiled An+B and reverse structural selectors with computed styles, queries, Inspector, and host geometry, bounded is, where, and list-not selector functions with specificity, queries, Inspector, and geometry, bounded CSS important cascade with G.Ic diagnostics, custom properties, inline, responsive values, Inspector counts, and retained fields, bounded inherited CSS custom properties with S.Vc/Cv and G.Vc aliases, responsive values, Inspector counts, and retained fields, bounded responsive CSS media queries with compact width/height aliases, portrait/landscape, exact/min/max aspect-ratio states, modern strict/inclusive range syntax, bounded @supports declaration and selector capability checks, diagnostics, and retained fields, optional bounded persistent cache aliases and Inspector diagnostics, bounded content-box and border-box geometry, scoped style commands, ordered row and column flex layout with around/evenly distribution, item alignment, wrapping, growth, and shrink, semantic accessibility, protected values, rendered bounds, MSAA and UI Automation providers, value, table/grid, and read-only logical text patterns plus Access inspector, bounded autofocus and access keys, keyboard focus and activation, responsive image maps, compact time/edit metadata, output/data values and safe rebuilds, progress and meter gauges, disclosure commands, retained same-page fragments, Lea-first New Tab and explicit legacy fallback, host-controlled go requests, Inspector open/refresh/clear and legacy isolation, class mutations, diagnostics, safe clicks and variables, find and downloads, scripts and alerts, open/save/send, private cookies, close routing, sessions, source, menu, timer, start page, and navigation.");
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
            {
                var found = BlockById(block, id);
                if (found != null) return found;
            }
            throw new InvalidOperationException("Missing Lea block: " + id);
        }

        private static B BlockById(B block, string id)
        {
            if (block == null) return null;
            if (block.El != null && block.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return block;
            if (block.Tb == null) return null;
            foreach (var row in block.Tb.Rs)
                foreach (var cell in row.Cs)
                    foreach (var part in cell.Bs)
                    {
                        var found = BlockById(part, id);
                        if (found != null) return found;
                    }
            return null;
        }

        private static bool HasBlock(P page, string id)
        {
            foreach (var block in page.Bs)
                if (BlockById(block, id) != null) return true;
            return false;
        }

        private static bool Has(IList<string> values, string text)
        {
            foreach (var value in values)
                if (value.IndexOf(text, StringComparison.Ordinal) >= 0) return true;
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

        private static System.Drawing.Rectangle BlockContent(V view, string id)
        {
            var target = BlockById(view.At, id);
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), target) || (bool)type.GetField("Inline").GetValue(value)) continue;
                return (System.Drawing.Rectangle)type.GetField("C").GetValue(value);
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

        private static System.Drawing.Rectangle EllipsisRect(V view, string id)
        {
            var values = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) as System.Collections.IEnumerable;
            foreach (var value in values)
            {
                var type = value.GetType();
                var element = type.GetField("El").GetValue(value) as N;
                if (element != null && element.Av("id") == id && (string)type.GetField("X").GetValue(value) == "" &&
                    (string)type.GetField("Px").GetValue(value) == "\u2026") return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            throw new InvalidOperationException("Missing Lea ellipsis marker: " + id);
        }

        private static System.Drawing.Rectangle[] EllipsisPrefixBounds(V view, string id)
        {
            var values = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) as System.Collections.IEnumerable;
            foreach (var value in values)
            {
                var type = value.GetType();
                var block = type.GetField("B").GetValue(value) as B;
                var prefix = type.GetField("Px").GetValue(value) as string;
                if (block != null && block.El != null && block.El.Av("id") == id && !String.IsNullOrEmpty(prefix) &&
                    (string)type.GetField("X").GetValue(value) != "")
                    return new[] { (System.Drawing.Rectangle)type.GetField("R").GetValue(value), (System.Drawing.Rectangle)type.GetField("C").GetValue(value) };
            }
            throw new InvalidOperationException("Missing Lea ellipsis prefix: " + id);
        }

        private static System.Drawing.RectangleF AutomationRangeRect(V view, object automation, string source, string value)
        {
            var start = source.IndexOf(value, StringComparison.Ordinal);
            Check(start >= 0, "Missing Lea automation range text: " + value);
            var values = (double[])typeof(V).GetMethod("AutomationTextRectangles", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(view, new object[] { automation, start, start + value.Length });
            Check(values.Length >= 4, "Missing Lea automation range rectangle: " + value);
            var result = new System.Drawing.RectangleF((float)values[0], (float)values[1], (float)values[2], (float)values[3]);
            for (var index = 4; index + 3 < values.Length; index += 4)
                result = System.Drawing.RectangleF.Union(result, new System.Drawing.RectangleF((float)values[index], (float)values[index + 1], (float)values[index + 2], (float)values[index + 3]));
            return result;
        }

        private static string AutomationWordAtPoint(V view, object automation, System.Drawing.RectangleF rectangle)
        {
            var range = typeof(V).GetMethod("AutomationTextRangeFromPoint", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(view, new object[] { automation, new System.Windows.Point(rectangle.Left + rectangle.Width / 2F, rectangle.Top + rectangle.Height / 2F) });
            range.GetType().GetMethod("ExpandToEnclosingUnit").Invoke(range, new object[] { TextUnit.Word });
            return ((string)range.GetType().GetMethod("GetText").Invoke(range, new object[] { -1 })).Trim();
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

        private static System.Drawing.Rectangle InlineElementBounds(V view, string id)
        {
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            var result = System.Drawing.Rectangle.Empty;
            foreach (var value in values)
            {
                var type = value.GetType();
                var element = type.GetField("El").GetValue(value) as N;
                if (element == null || element.Av("id") != id || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var rect = (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
                result = result.IsEmpty ? rect : System.Drawing.Rectangle.Union(result, rect);
            }
            return result;
        }

        private static System.Drawing.Rectangle[] InlineLines(V view, B block)
        {
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            var lines = new List<System.Drawing.Rectangle>();
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var rect = (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
                var found = -1;
                for (var index = 0; index < lines.Count; index++) if (lines[index].Top == rect.Top) { found = index; break; }
                if (found < 0) lines.Add(rect);
                else lines[found] = System.Drawing.Rectangle.Union(lines[found], rect);
            }
            lines.Sort(delegate(System.Drawing.Rectangle left, System.Drawing.Rectangle right) { return left.Top.CompareTo(right.Top); });
            return lines.ToArray();
        }

        private static int LineWidthSpread(System.Drawing.Rectangle[] lines)
        {
            if (lines == null || lines.Length < 2) return 0;
            var minimum = Int32.MaxValue;
            var maximum = 0;
            foreach (var line in lines)
            {
                minimum = Math.Min(minimum, line.Width);
                maximum = Math.Max(maximum, line.Width);
            }
            return maximum - minimum;
        }

        private static System.Drawing.Rectangle InlineTextRect(V view, B block, string text)
        {
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                if ((string)type.GetField("X").GetValue(value) == text) return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            return System.Drawing.Rectangle.Empty;
        }

        private static System.Drawing.Rectangle InlineKindRect(V view, B block, string kind)
        {
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                if ((string)type.GetField("K").GetValue(value) == kind) return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            return System.Drawing.Rectangle.Empty;
        }

        private static System.Drawing.Rectangle InlinePictureRect(V view, B block, string kind)
        {
            var member = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea layout boxes");
            foreach (var value in values)
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) ||
                    !(bool)type.GetField("Inline").GetValue(value) || !(type.GetField("P").GetValue(value) is Uri)) continue;
                if ((string)type.GetField("K").GetValue(value) == kind) return (System.Drawing.Rectangle)type.GetField("R").GetValue(value);
            }
            return System.Drawing.Rectangle.Empty;
        }

        private static Ir Run(B block, string kind)
        {
            foreach (var run in block.Is) if (run.K == kind) return run;
            throw new InvalidOperationException("Missing Lea inline run: " + kind);
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

        private static bool HasFragmentHitAt(V view, string fragment, System.Drawing.Point point)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("Missing Lea pointer hits");
            foreach (var value in values)
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url != null && url.Fragment.Equals(fragment, StringComparison.OrdinalIgnoreCase) &&
                    ((System.Drawing.Rectangle)type.GetField("R").GetValue(value)).Contains(point)) return true;
            }
            return false;
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

        private static bool HasColor(System.Drawing.Bitmap image, System.Drawing.Rectangle area, System.Drawing.Color color)
        {
            area.Intersect(new System.Drawing.Rectangle(System.Drawing.Point.Empty, image.Size));
            for (var y = area.Top; y < area.Bottom; y++)
                for (var x = area.Left; x < area.Right; x++)
                    if (ColorDistance(image.GetPixel(x, y), color) < 8) return true;
            return false;
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
