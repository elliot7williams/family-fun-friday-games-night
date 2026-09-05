using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Tests
{
    internal static class Smoke
    {
        private static string pages;

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 1 && args[0] == "--image-fake") return UnhardenedImageWorker();
                var root = Path.GetFullPath(args.Length > 0 ? args[0] : Directory.GetCurrentDirectory());
                pages = Path.Combine(root, "LeaForge", "test-pages");
                if (!Directory.Exists(pages)) pages = Path.Combine(root, "test-pages");
                if (args.Length > 1 && args[1] == "--job-host") return ImageJobHost(root);
                if (args.Length > 1 && args[1] == "--clip") { CheckClipboardContention(root); return 0; }
                if (args.Length > 1 && args[1] == "--paste") { CheckPasteText(root); return 0; }
                if (args.Length > 1 && args[1] == "--fragment") { CheckFragmentStartup(root); return 0; }
                if (args.Length > 1 && args[1] == "--sheets") { CheckSheets(root); return 0; }
                if (args.Length > 1 && args[1] == "--loads") { CheckLoadLifecycle(root); return 0; }
                if (args.Length > 1 && args[1] == "--pics") { CheckPictureLimits(root); return 0; }
                if (args.Length > 1 && args[1] == "--input") { CheckInputLimits(root); return 0; }
                if (args.Length > 1 && args[1] == "--cache") { CheckCacheIdentity(); return 0; }
                var coreOnly = args.Length > 1 && args[1] == "--core";
                if (coreOnly)
                {
                    var paths = new List<string>(args);
                    paths.RemoveAt(1);
                    args = paths.ToArray();
                }
                if (!coreOnly) { CheckClipboardContention(root); CheckEditCommands(root); }
                if (args.Length > 1 && args[1] == "--edit") return 0;
                CheckPasteText(root);
                CheckSheets(root);
                CheckLoadLifecycle(root);
                CheckPictureLimits(root);
                CheckInputLimits(root);
                CheckCacheIdentity();
                CheckNavigationGuard(root);
                CheckLoadFailures(root);
                CheckRetainedData(root);
                CheckDisposal(root);
                using (var slow = new CompressedServer())
                {
                    var disposingView = new V();
                    var events = 0;
                    disposingView.Changed += delegate { events++; };
                    disposingView.Done += delegate { events++; };
                    var work = disposingView.GoA(new Uri(slow.U, "?delay=1000&dispose=1"));
                    WaitFor(delegate { return slow.Hits == 1; }, "view disposal request started");
                    disposingView.Dispose();
                    var before = events;
                    WaitFor(delegate { return work.IsCompleted; }, "disposed view request completion");
                    Eq("cancel", Fails(delegate { work.GetAwaiter().GetResult(); }, "disposed view cancels document request").K, "disposed view cancellation kind");
                    Eq(before, events, "disposed view emits no late callbacks");
                    disposingView.Dispose();
                }
                Console.WriteLine("Lea asynchronous view-disposal smoke passed.");
                var forms = Path.Combine(pages, "forms.html");
                var engine = new E();
                var page = engine.Go(forms);
                Eq(15000, engine.Tm, "default request timeout milliseconds");
                engine.Tm = 1;
                Eq(100, engine.Tm, "minimum request timeout");
                engine.Tm = Int32.MaxValue;
                Eq(60000, engine.Tm, "maximum request timeout");
                engine.Tm = 15000;
                Eq(200, page.St, "local status");
                Eq("text/html", page.Ct, "local content type");
                Eq("utf-8", page.Ch, "local charset");
                Ok(page.Sz > 0 && !page.Ca, "local response metadata");
                Eq(page.U, page.Bu, "document URL is fallback base URL");
                Eq(9, page.Fs.Count, "field count");
                Eq("text,password,textarea,select,checkbox,radio,radio,reset,submit", Kinds(page), "field kinds");
                Eq(3, page.Fs[3].Os.Count, "select options");
                Eq("lea", page.Fs[3].V, "selected option value");
                Eq("Lea green", page.Fs[3].Os[1].T, "selected option text");
                Ok(page.Fs[3].Os[2].Ds, "disabled option");
                Ok(page.Fs[4].C && page.Fs[5].C && !page.Fs[6].C, "choice defaults");

                var validationPath = Path.Combine(pages, "form-validation.html");
                var validationPage = engine.Go(validationPath);
                Eq(12, validationPage.Fs.Count, "validation field count");
                Eq("search,email,url,number,date,checkbox,radio,radio,select,submit,text,submit", Kinds(validationPage), "validation field kinds");
                Ok(validationPage.Fs[0].Rq, "required alias");
                Eq(3, validationPage.Fs[0].Mn, "minimum length alias");
                Eq(12, validationPage.Fs[0].Mx, "maximum length alias");
                Eq("[A-Za-z ]+", validationPage.Fs[0].Pa, "pattern alias");
                Eq("10", validationPage.Fs[3].Lo, "number minimum alias");
                Eq("20", validationPage.Fs[3].Hi, "number maximum alias");
                Eq("2026-01-01", validationPage.Fs[4].Lo, "date minimum alias");
                Ok(validationPage.Fs[10].Nv && validationPage.Fs[11].Nv, "form novalidate alias");
                Ok(validationPage.Fs[0].Ok && validationPage.Fs[0].Vm.Length == 0, "initial validation state");
                Eq("#176b4d", validationPage.Fs[0].S.Bc, "initial valid pseudo selector");

                var widgetsPath = Path.Combine(pages, "form-widgets.html");
                var widgetsPage = engine.Go(widgetsPath);
                Eq(10, widgetsPage.Fs.Count, "richer widget field count");
                Eq("file,range,color,time,month,week,datetime-local,email,reset,submit", Kinds(widgetsPage), "richer widget kinds");
                Ok(widgetsPage.Fs[0].Rq && widgetsPage.Fs[0].Mu, "required multiple file aliases");
                Eq(".txt,text/plain", widgetsPage.Fs[0].Ac, "file accept alias");
                Eq(0, widgetsPage.Fs[0].Fp.Count, "file paths start private and empty");
                Eq("m", widgetsPage.Fs[0].En, "multipart encoding alias");
                Eq("5", widgetsPage.Fs[1].Sp, "range step alias");
                Eq("25", widgetsPage.Fs[1].V, "range initial value");
                Eq("#176b4d", widgetsPage.Fs[2].V, "color initial value");
                Ok(widgetsPage.Fs[7].Mu, "multiple email alias");

                var tablePage = engine.Go(Path.Combine(pages, "tables.html"));
                Eq(3, tablePage.Bs.Count, "table page blocks");
                Eq("table", tablePage.Bs[1].K, "table block kind");
                var table = tablePage.Bs[1].Tb;
                Ok(table != null, "parsed table model");
                Eq("Lea engine commands", table.Cp, "table caption");
                Eq(4, table.Rs.Count, "table row count");
                Eq(3, table.Cs, "table column count");
                Eq(3, table.Rs[0].Cs.Count, "header cell count");
                Ok(table.Rs[0].Cs[0].H, "header cell kind");
                Eq("Alias", table.Rs[0].Cs[0].X, "header cell text");
                Eq(1, table.Rs[1].Cs[0].Bs.Count, "plain cell fast block");
                Eq(2, table.Rs[2].Cs[0].Sp, "cell column span");
                var nestedCell = table.Rs[3].Cs[0];
                Eq(3, nestedCell.Sp, "nested cell column span");
                Eq(3, nestedCell.Bs.Count, "nested cell blocks");
                Eq("strong,p,a", CellKinds(nestedCell), "nested cell block kinds");
                Eq(1, tablePage.Ls.Count, "table link count");
                Eq("second.html", Path.GetFileName(tablePage.Ls[0].U.LocalPath), "table link target");
                Eq(480, tablePage.Bs[1].S.W, "table CSS width");
                Eq(1, table.Rs[1].Cs[0].S.Bt, "cell CSS border");
                Eq(6, table.Rs[1].Cs[0].S.Pl, "cell CSS padding");

                var externalPath = Path.Combine(pages, "external.html");
                var externalPage = engine.Go(externalPath);
                Eq("#edf7f2", externalPage.S.Bg, "external body background");
                Eq(3, externalPage.Bs.Count, "external CSS hidden block");
                Eq("#176b4d", externalPage.Bs[0].S.F, "external heading color");
                Eq(19, externalPage.Bs[0].S.Fs, "external heading size");
                Eq("#7e22ce", externalPage.Bs[1].S.F, "later inline stylesheet order");
                Ok(externalPage.Bs[1].S.B, "external inherited bold");
                Eq("#a13b18", externalPage.Bs[2].S.F, "external class color");
                Eq("Lea External CSS", Complete(new E().GoA(externalPath), "async local external CSS").T, "async local external CSS");

                var basePath = Path.Combine(pages, "base-url.html");
                var basePage = engine.Go(basePath);
                var responseUrl = new Uri(basePath);
                var expectedBase = new Uri(responseUrl, "../");
                Eq(responseUrl, basePage.U, "base element keeps response URL");
                Eq(expectedBase, basePage.Bu, "first valid document base URL");
                Eq("#edf7f2", basePage.S.Bg, "base URL external stylesheet");
                Eq("#176b4d", basePage.Bs[0].S.F, "base URL stylesheet rule");
                Eq(2, basePage.Ls.Count, "base URL link count");
                Eq(new Uri(expectedBase, "test-pages/second.html"), basePage.Ls[0].U, "base URL link target");
                Eq(new Uri(expectedBase, "../assets/lea.ico"), BlockById(basePage, "base-image").U, "base URL image target");
                Eq(new Uri(expectedBase, "test-pages/form-result.html"), FieldById(basePage, "base-submit").U, "base URL form target");
                Eq(new Uri(expectedBase, "test-pages/download-sample.txt"), basePage.Ls[1].U, "base URL download target");
                var asyncBasePage = Complete(new E().GoA(basePath), "async base page");
                Eq(expectedBase, asyncBasePage.Bu, "async document base URL");
                Eq("#edf7f2", asyncBasePage.S.Bg, "async base URL external stylesheet");

                var scriptPath = Path.Combine(pages, "script-bridge.html");
                var scriptEngine = new E();
                var scriptPage = scriptEngine.Go(scriptPath);
                Eq("Lea Script Bridge", scriptPage.T, "script title assignment");
                Eq(9, scriptPage.Jc, "bounded page command count");
                Eq(2, scriptPage.Ms.Count, "script alert queue");
                Eq("Page ready", scriptPage.Ms[0], "JavaScript alert bridge");
                Eq("Second alert", scriptPage.Ms[1], "short alert alias");
                Eq("#f0fff8", scriptPage.S.Bg, "short background command");
                Eq("#164e3f", scriptPage.S.F, "short foreground command");
                Eq(20, scriptPage.S.Fs, "short font-size command");
                Eq(3, scriptPage.Bs.Count, "script nodes stay out of page blocks");
                Eq("Lea Script Bridge", Complete(new E().GoA(scriptPath), "async page scripts").T, "async page scripts");

                var manualCommands = scriptEngine.Js(scriptPage, "t(\"Manual \\u004c\"); bg(\"#112233\"); fg(\"white\"); fs(22); a(\"Manual\\nalert\");");
                Eq(5, manualCommands, "manual command count");
                Eq(14, scriptPage.Jc, "cumulative command count");
                Eq("Manual L", scriptPage.T, "escaped manual title");
                Eq("#112233", scriptPage.S.Bg, "manual background rebuild");
                Eq("white", scriptPage.S.F, "manual named foreground");
                Eq(22, scriptPage.S.Fs, "manual font-size rebuild");
                Eq("Manual alert", scriptPage.Ms[2], "manual alert normalization");
                Eq(0, scriptEngine.Js(scriptPage, "eval('t(\\\"bad\\\")'); if(true){t('bad');}; fs(7); bg('red;display:none');"), "unknown and unsafe commands ignored");
                Eq("Manual L", scriptPage.T, "ignored commands do not mutate title");

                var domPath = Path.Combine(pages, "dom-bridge.html");
                var domEngine = new E();
                var domPage = domEngine.Go(domPath);
                Eq(11, domPage.Jc, "bounded DOM command count");
                Eq("DOM ready", ById(domPage.D, "status").C[0].X, "DOM textContent assignment");
                var domStatus = BlockById(domPage, "status");
                Eq("#e0f2fe", domStatus.S.Bg, "DOM backgroundColor assignment");
                Eq("#164e63", domStatus.S.F, "DOM color assignment");
                Eq(19, domStatus.S.Fs, "DOM fontSize assignment");
                Eq("panel ready", ById(domPage.D, "status").Av("class"), "DOM classList add");
                var domName = FieldById(domPage, "name");
                Eq("DOM value", domName.V, "DOM field value assignment");
                Ok(FieldById(domPage, "agree").C, "DOM checked assignment");
                Ok(FieldById(domPage, "locked").Ds, "DOM disabled assignment");
                Ok(ById(domPage.D, "notice").A.ContainsKey("hidden") && domEngine.Ax(domPage, ById(domPage.D, "notice")) == null, "DOM hidden assignment");
                Ok(ById(domPage.D, "more").Op, "DOM open assignment");
                Ok(Object.ReferenceEquals(domName.El, domEngine.Kp(domPage)), "DOM focus call");
                Eq(9, domEngine.Ck(domPage, ById(domPage.D, "apply")), "bounded DOM onclick commands");
                Eq("Clicked DOM", ById(domPage.D, "status").C[0].X, "DOM innerText assignment");
                Eq("Clicked value", domName.V, "DOM onclick value assignment");
                Ok(!FieldById(domPage, "agree").C && !FieldById(domPage, "locked").Ds, "DOM onclick field state");
                Ok(!ById(domPage.D, "notice").A.ContainsKey("hidden") && domEngine.Ax(domPage, ById(domPage.D, "notice")) != null, "DOM show assignment");
                Ok(!ById(domPage.D, "more").Op && ById(domPage.D, "status").Av("class").Length == 0, "DOM disclosure and class removal");
                Ok(Object.ReferenceEquals(domName.El, domEngine.Kp(domPage)), "DOM onclick focus call");
                Eq(0, domEngine.Js(domPage, "document.getElementById('status').innerHTML='<b>bad</b>'; document.querySelector('#status').textContent='bad'; document.getElementById(target).textContent='bad'; document.getElementById('status').style.visibility='hidden'; document.getElementById('apply').click(); document.getElementById('agree').checked=!true;"), "unknown or computed DOM APIs ignored");
                Eq("Clicked DOM", ById(domPage.D, "status").C[0].X, "rejected DOM APIs do not mutate text");

                var displayPath = Path.Combine(pages, "display-navigation.html");
                var displayEngine = new E();
                var displayPage = displayEngine.Go(displayPath);
                Eq(0, displayPage.Jc, "display and location fixture has no startup commands");
                Ok(!HasBlock(displayPage, "panel") && displayEngine.Ax(displayPage, ById(displayPage.D, "panel")) == null, "authored display none suppresses layout and accessibility");
                Eq("grid", BlockById(displayPage, "invalid").S.D, "invalid authored display preserves earlier declaration");
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "show")), "compact display command accepted");
                Eq("block", BlockById(displayPage, "panel").S.D, "compact display command overrides authored none");
                Ok(displayEngine.Ax(displayPage, ById(displayPage.D, "panel")) != null, "display command restores accessibility");
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "hide")), "literal DOM display none accepted");
                Ok(!HasBlock(displayPage, "panel"), "literal DOM display none removes layout");
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "flex")), "literal DOM flex display accepted");
                Eq("flex", BlockById(displayPage, "row").S.D, "literal DOM flex display reaches computed style");
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "clear")), "empty DOM display reset accepted");
                Eq("block", BlockById(displayPage, "row").S.D, "empty DOM display restores authored cascade");
                Eq("#123f8c", BlockById(displayPage, "row").S.F, "display reset preserves unrelated inline style");
                Eq(0, displayEngine.Js(displayPage, "dp('row','contents'); display('row','ruby'); document.getElementById('row').style.display='flow-root';"), "unsupported display modes rejected");
                Eq("block", BlockById(displayPage, "row").S.D, "rejected display modes preserve style");
                var displayTargetBase = new Uri(Path.Combine(pages, "go-target.html"));
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "href")), "literal location href accepted");
                Eq(new Uri(displayTargetBase.AbsoluteUri + "#href"), displayEngine.Gt(displayPage), "literal location href resolves against document base");
                Eq(1, displayEngine.Ck(displayPage, ById(displayPage.D, "assign")), "literal location assign accepted");
                Eq(new Uri(displayTargetBase.AbsoluteUri + "#assign"), displayEngine.Gt(displayPage), "literal location assign resolves against document base");
                Eq(1, displayEngine.Js(displayPage, "location='go-target.html?mode=one#both';"), "direct literal location assignment accepted");
                var displayQueryTarget = new UriBuilder(displayTargetBase) { Query = "mode=one", Fragment = "both" }.Uri;
                Eq(displayQueryTarget, displayEngine.Gt(displayPage), "file location keeps query and fragment components");
                Eq(1, displayEngine.Js(displayPage, "document.location.href='go-target.html#document'; window.location.assign('go-target.html#ignored');"), "one pending familiar location request per run");
                Eq(new Uri(displayTargetBase.AbsoluteUri + "#document"), displayEngine.Gt(displayPage), "first familiar location request wins");
                Eq(0, displayEngine.Js(displayPage, "location.href=target; window.location.replace('go-target.html'); location.href='javascript:alert(1)';"), "computed, replacement, and unsafe location forms rejected");
                Eq<Uri>(null, displayEngine.Gt(displayPage), "rejected familiar navigation queues nothing");

                var calculatedPath = Path.Combine(pages, "calculated-widths.html");
                var calculatedEngine = new E();
                var calculatedPage = calculatedEngine.Go(calculatedPath);
                Eq(0, calculatedPage.Jc, "calculated width fixture has no startup commands");
                var calculatedOuter = BlockById(calculatedPage, "outer").S;
                Eq(100, calculatedOuter.Wp, "calculated width percentage model");
                Eq(-60, calculatedOuter.Wo, "calculated width pixel offset model");
                Eq(50, BlockById(calculatedPage, "half").S.Wp, "percentage width model");
                Eq(-80, BlockById(calculatedPage, "minus").S.Wo, "negative calculated width offset");
                Eq(20, BlockById(calculatedPage, "plus").S.Wo, "positive calculated width offset");
                var calculatedMinimum = BlockById(calculatedPage, "minimum").S;
                Eq(20, calculatedMinimum.Wp, "percentage preferred width");
                Eq(50, calculatedMinimum.Wnp, "calculated minimum width percentage");
                Eq(-10, calculatedMinimum.Wno, "calculated minimum width offset");
                var calculatedMaximum = BlockById(calculatedPage, "maximum").S;
                Eq(75, calculatedMaximum.Wxp, "calculated maximum width percentage");
                Eq(5, calculatedMaximum.Wxo, "calculated maximum width offset");
                Eq(240, BlockById(calculatedPage, "invalid").S.W, "invalid calculated expression preserves earlier width");
                Eq(75, BlockById(calculatedPage, "variable").S.Wp, "custom property calculated width percentage");
                Eq(-15, BlockById(calculatedPage, "variable").S.Wo, "custom property calculated width offset");
                Eq(60, BlockById(calculatedPage, "inherit-child").S.Wp, "explicit inherited percentage width");
                Eq(1, calculatedEngine.Ck(calculatedPage, ById(calculatedPage.D, "set-width")), "compact calculated width command");
                Eq(100, BlockById(calculatedPage, "command").S.Wp, "compact width command reaches style model");
                Eq(-120, BlockById(calculatedPage, "command").S.Wo, "compact width command keeps offset");
                Eq(1, calculatedEngine.Js(calculatedPage, "width('command','75%')"), "long width command alias");
                Eq(75, BlockById(calculatedPage, "command").S.Wp, "long width alias updates percentage");
                Eq(1, calculatedEngine.Ck(calculatedPage, ById(calculatedPage.D, "dom-width")), "literal DOM percentage width");
                Eq(50, BlockById(calculatedPage, "command").S.Wp, "literal DOM width reaches style model");
                Eq(1, calculatedEngine.Ck(calculatedPage, ById(calculatedPage.D, "clear-width")), "empty DOM width reset");
                Eq(280, BlockById(calculatedPage, "command").S.W, "empty DOM width restores authored cascade");
                Eq(0, BlockById(calculatedPage, "command").S.Wp, "empty DOM width clears inline percentage");
                Eq("#123f8c", BlockById(calculatedPage, "command").S.F, "width reset preserves unrelated inline style");
                Eq(0, calculatedEngine.Js(calculatedPage, "wd('command','min(max(100px,200px),400px)'); width('command','calc(100% / 2)'); document.getElementById('command').style.width='101%';"), "nested or out-of-range width expressions rejected");
                Eq(280, BlockById(calculatedPage, "command").S.W, "rejected width expressions preserve cascade");

                var calculatedHeightPath = Path.Combine(pages, "calculated-heights.html");
                var calculatedHeightEngine = new E();
                var calculatedHeightPage = calculatedHeightEngine.Go(calculatedHeightPath);
                Eq(0, calculatedHeightPage.Jc, "calculated height fixture has no startup commands");
                Eq(50, BlockById(calculatedHeightPage, "viewport").S.Hp, "viewport percentage height model");
                Eq(50, BlockById(calculatedHeightPage, "half").S.Hp, "nested percentage height model");
                Eq(-80, BlockById(calculatedHeightPage, "minus").S.Ho, "negative calculated height offset");
                Eq(20, BlockById(calculatedHeightPage, "plus").S.Ho, "positive calculated height offset");
                var calculatedHeightMinimum = BlockById(calculatedHeightPage, "minimum").S;
                Eq(20, calculatedHeightMinimum.Hp, "percentage preferred height");
                Eq(50, calculatedHeightMinimum.Hnp, "calculated minimum height percentage");
                Eq(-10, calculatedHeightMinimum.Hno, "calculated minimum height offset");
                var calculatedHeightMaximum = BlockById(calculatedHeightPage, "maximum").S;
                Eq(75, calculatedHeightMaximum.Hxp, "calculated maximum height percentage");
                Eq(5, calculatedHeightMaximum.Hxo, "calculated maximum height offset");
                Eq(140, BlockById(calculatedHeightPage, "invalid").S.H, "invalid calculated expression preserves earlier height");
                Eq(75, BlockById(calculatedHeightPage, "variable").S.Hp, "custom property calculated height percentage");
                Eq(-15, BlockById(calculatedHeightPage, "variable").S.Ho, "custom property calculated height offset");
                Eq(60, BlockById(calculatedHeightPage, "inherit-child").S.Hp, "explicit inherited percentage height");
                Eq(2000, BlockById(calculatedHeightPage, "legacy-height-cap").S.H, "oversized authored height keeps legacy bounded cap");
                Eq(1, calculatedHeightEngine.Ck(calculatedHeightPage, ById(calculatedHeightPage.D, "set-height")), "compact calculated height command");
                Eq(100, BlockById(calculatedHeightPage, "command").S.Hp, "compact height command reaches style model");
                Eq(-120, BlockById(calculatedHeightPage, "command").S.Ho, "compact height command keeps offset");
                Eq(1, calculatedHeightEngine.Js(calculatedHeightPage, "height('command','75%')"), "long height command alias");
                Eq(75, BlockById(calculatedHeightPage, "command").S.Hp, "long height alias updates percentage");
                Eq(1, calculatedHeightEngine.Ck(calculatedHeightPage, ById(calculatedHeightPage.D, "dom-height")), "literal DOM percentage height");
                Eq(50, BlockById(calculatedHeightPage, "command").S.Hp, "literal DOM height reaches style model");
                Eq(1, calculatedHeightEngine.Ck(calculatedHeightPage, ById(calculatedHeightPage.D, "clear-height")), "empty DOM height reset");
                Eq(180, BlockById(calculatedHeightPage, "command").S.H, "empty DOM height restores authored cascade");
                Eq(0, BlockById(calculatedHeightPage, "command").S.Hp, "empty DOM height clears inline percentage");
                Eq("#123f8c", BlockById(calculatedHeightPage, "command").S.F, "height reset preserves unrelated inline style");
                Eq(0, calculatedHeightEngine.Js(calculatedHeightPage, "ht('command','min(max(100px,200px),400px)'); height('command','calc(100% / 2)'); document.getElementById('command').style.height='101%';"), "nested or out-of-range height expressions rejected");
                Eq(180, BlockById(calculatedHeightPage, "command").S.H, "rejected height expressions preserve cascade");

                var logicalPath = Path.Combine(pages, "logical-sizing-insets.html");
                var logicalEngine = new E();
                var logicalPage = logicalEngine.Go(logicalPath);
                Eq(0, logicalPage.Jc, "logical sizing fixture has no startup commands");
                var logicalSize = BlockById(logicalPage, "logical-size").S;
                Ok(logicalSize.Wp == 50 && logicalSize.Wo == 20 && logicalSize.Wn == 180 && logicalSize.Wx == 240 &&
                    logicalSize.Hp == 50 && logicalSize.Hn == 120 && logicalSize.Hx == 160,
                    "logical size and constraint aliases");
                Eq(new Size(140, 70), new Size(BlockById(logicalPage, "order-physical").S.W, BlockById(logicalPage, "order-physical").S.H), "later physical sizes win source order");
                Eq(new Size(120, 60), new Size(BlockById(logicalPage, "order-logical").S.W, BlockById(logicalPage, "order-logical").S.H), "later logical sizes win source order");
                var logicalVariable = BlockById(logicalPage, "variable").S;
                Ok(logicalVariable.Wp == 75 && logicalVariable.Wo == -15 && logicalVariable.Hp == 25, "logical custom-property dimensions");
                Eq(new Size(110, 80), new Size(BlockById(logicalPage, "important").S.W, BlockById(logicalPage, "important").S.H), "physical and logical important cascade");
                Ok(BlockById(logicalPage, "inherit-child").S.Wp == 60 && BlockById(logicalPage, "inherit-child").S.Hp == 40, "explicit logical size inheritance");
                Eq(new Size(0, 0), new Size(BlockById(logicalPage, "reset").S.W, BlockById(logicalPage, "reset").S.H), "logical initial and unset reset");
                var logicalAxis = BlockById(logicalPage, "axis-position").S;
                Ok(logicalAxis.Lt == 24 && logicalAxis.Rt == 30 && logicalAxis.Tp == 18 && logicalAxis.Bm == 26, "logical axis inset shorthands");
                var logicalLonghand = BlockById(logicalPage, "longhand-position").S;
                Ok(logicalLonghand.Lt == 32 && !logicalLonghand.Rt.HasValue && !logicalLonghand.Tp.HasValue && logicalLonghand.Bm == 20, "logical inset longhands");
                var logicalInvalid = BlockById(logicalPage, "invalid-position").S;
                Ok(logicalInvalid.Lt == 11 && logicalInvalid.Rt == 13 && logicalInvalid.Tp == 19 && logicalInvalid.Bm == 21, "invalid logical axis shorthands are atomic");
                Eq(2, logicalEngine.Ck(logicalPage, ById(logicalPage.D, "set-logical")), "short logical size command aliases");
                Ok(BlockById(logicalPage, "command").S.Wp == 50 && BlockById(logicalPage, "command").S.H == 120, "short logical aliases reach physical records");
                Eq(2, logicalEngine.Ck(logicalPage, ById(logicalPage.D, "dom-logical")), "familiar logical DOM styles");
                Ok(BlockById(logicalPage, "command").S.Wp == 25 && BlockById(logicalPage, "command").S.H == 70, "familiar logical DOM styles reach records");
                Eq(2, logicalEngine.Ck(logicalPage, ById(logicalPage.D, "clear-logical")), "logical DOM size reset");
                Eq(new Size(210, 90), new Size(BlockById(logicalPage, "command").S.W, BlockById(logicalPage, "command").S.H), "logical DOM reset restores authored sizes");
                Eq("#123f8c", BlockById(logicalPage, "command").S.F, "logical DOM reset preserves unrelated inline style");
                Eq(2, logicalEngine.Js(logicalPage, "inlinesize('command','180px');blocksize('command','75px')"), "long logical command aliases");
                Eq(new Size(180, 75), new Size(BlockById(logicalPage, "command").S.W, BlockById(logicalPage, "command").S.H), "long logical aliases update sizes");
                Eq(0, logicalEngine.Js(logicalPage, "is('command','101%');bs('command','1.5em')"), "unsupported logical command values rejected");
                Eq(new Size(180, 75), new Size(BlockById(logicalPage, "command").S.W, BlockById(logicalPage, "command").S.H), "rejected logical commands preserve sizes");

                var logicalBoxPath = Path.Combine(pages, "logical-box-model.html");
                var logicalBoxEngine = new E();
                var logicalBoxPage = logicalBoxEngine.Go(logicalBoxPath);
                Eq(0, logicalBoxPage.Jc, "logical box fixture has no startup commands");
                var logicalAxisBox = BlockById(logicalBoxPage, "axis-box").S;
                Ok(logicalAxisBox.Mt == 14 && logicalAxisBox.Mr == 18 && logicalAxisBox.Mb == 16 && logicalAxisBox.Ml == 12 &&
                    logicalAxisBox.Pt == 6 && logicalAxisBox.Pr == 10 && logicalAxisBox.Pb == 7 && logicalAxisBox.Pl == 8,
                    "logical margin and padding axis shorthands");
                var logicalLonghandBox = BlockById(logicalBoxPage, "longhand-box").S;
                Ok(logicalLonghandBox.Mt == 23 && logicalLonghandBox.Mr == 22 && logicalLonghandBox.Mb == 24 && logicalLonghandBox.Ml == 21 &&
                    logicalLonghandBox.Pt == 6 && logicalLonghandBox.Pr == 5 && logicalLonghandBox.Pb == 7 && logicalLonghandBox.Pl == 4,
                    "logical margin and padding longhands");
                var logicalPhysicalOrder = BlockById(logicalBoxPage, "source-physical").S;
                Ok(logicalPhysicalOrder.Mt == 32 && logicalPhysicalOrder.Mr == 11 && logicalPhysicalOrder.Mb == 13 && logicalPhysicalOrder.Ml == 31 &&
                    logicalPhysicalOrder.Pt == 5 && logicalPhysicalOrder.Pr == 9 && logicalPhysicalOrder.Pb == 10 && logicalPhysicalOrder.Pl == 3,
                    "later physical box edges win source order");
                var logicalOrder = BlockById(logicalBoxPage, "source-logical").S;
                Ok(logicalOrder.Mt == 16 && logicalOrder.Mr == 15 && logicalOrder.Mb == 17 && logicalOrder.Ml == 14 &&
                    logicalOrder.Pt == 4 && logicalOrder.Pr == 3 && logicalOrder.Pb == 5 && logicalOrder.Pl == 2,
                    "later logical box axes win source order");
                var logicalVariables = BlockById(logicalBoxPage, "variables").S;
                Ok(logicalVariables.Ml == -9 && logicalVariables.Mr == 19 && logicalVariables.Pt == 5 && logicalVariables.Pb == 7,
                    "logical box custom properties and negative margin");
                var logicalImportant = BlockById(logicalBoxPage, "important").S;
                Ok(logicalImportant.Ml == 20 && logicalImportant.Mr == 55 && logicalImportant.Pt == 6 && logicalImportant.Pb == 13,
                    "physical and logical box important cascade");
                var logicalInherited = BlockById(logicalBoxPage, "inherit-child").S;
                Ok(logicalInherited.Mt == 32 && logicalInherited.Mr == 31 && logicalInherited.Mb == 33 && logicalInherited.Ml == 30 &&
                    logicalInherited.Pt == 10 && logicalInherited.Pr == 9 && logicalInherited.Pb == 11 && logicalInherited.Pl == 8,
                    "explicit logical box inheritance");
                var logicalReset = BlockById(logicalBoxPage, "reset").S;
                Ok(logicalReset.Mt == 0 && logicalReset.Mr == 0 && logicalReset.Mb == 0 && logicalReset.Ml == 0 &&
                    logicalReset.Pt == 0 && logicalReset.Pr == 0 && logicalReset.Pb == 0 && logicalReset.Pl == 0,
                    "logical box initial and unset reset");
                var logicalBoxInvalid = BlockById(logicalBoxPage, "invalid").S;
                Ok(logicalBoxInvalid.Mt == 14 && logicalBoxInvalid.Mr == 13 && logicalBoxInvalid.Mb == 15 && logicalBoxInvalid.Ml == -22 &&
                    logicalBoxInvalid.Pt == 6 && logicalBoxInvalid.Pr == 9 && logicalBoxInvalid.Pb == 11 && logicalBoxInvalid.Pl == 8,
                    "malformed logical box declarations are atomic");
                var logicalBounded = BlockById(logicalBoxPage, "bounded").S;
                Ok(logicalBounded.Ml == -2000 && logicalBounded.Mr == 2000 && logicalBounded.Pt == 2000 && logicalBounded.Pb == 2000,
                    "logical box signed and unsigned bounds");
                Eq(4, logicalBoxEngine.Ck(logicalBoxPage, ById(logicalBoxPage.D, "set-axis")), "short logical box axis aliases");
                var logicalCommand = BlockById(logicalBoxPage, "command").S;
                Ok(logicalCommand.Mt == 14 && logicalCommand.Mr == 18 && logicalCommand.Mb == 16 && logicalCommand.Ml == 12 &&
                    logicalCommand.Pt == 6 && logicalCommand.Pr == 10 && logicalCommand.Pb == 7 && logicalCommand.Pl == 8,
                    "short logical box axes reach compact records");
                Eq(8, logicalBoxEngine.Ck(logicalBoxPage, ById(logicalBoxPage.D, "set-edges")), "short logical box edge aliases");
                logicalCommand = BlockById(logicalBoxPage, "command").S;
                Ok(logicalCommand.Mt == 23 && logicalCommand.Mr == 22 && logicalCommand.Mb == 24 && logicalCommand.Ml == 21 &&
                    logicalCommand.Pt == 6 && logicalCommand.Pr == 5 && logicalCommand.Pb == 7 && logicalCommand.Pl == 4,
                    "short logical box edges reach compact records");
                Eq(12, logicalBoxEngine.Js(logicalBoxPage, "margininline('command','1px 2px');marginblock('command','3px 4px');paddinginline('command','5px 6px');paddingblock('command','7px 8px');margininlinestart('command','9px');margininlineend('command','10px');marginblockstart('command','11px');marginblockend('command','12px');paddinginlinestart('command','13px');paddinginlineend('command','14px');paddingblockstart('command','15px');paddingblockend('command','16px')"), "readable logical box command aliases");
                logicalCommand = BlockById(logicalBoxPage, "command").S;
                Ok(logicalCommand.Mt == 11 && logicalCommand.Mr == 10 && logicalCommand.Mb == 12 && logicalCommand.Ml == 9 &&
                    logicalCommand.Pt == 15 && logicalCommand.Pr == 14 && logicalCommand.Pb == 16 && logicalCommand.Pl == 13,
                    "readable logical box commands reach compact records");
                Eq(4, logicalBoxEngine.Ck(logicalBoxPage, ById(logicalBoxPage.D, "dom-axis")), "familiar logical box axis styles");
                Eq(8, logicalBoxEngine.Ck(logicalBoxPage, ById(logicalBoxPage.D, "dom-edges")), "familiar logical box edge styles");
                logicalCommand = BlockById(logicalBoxPage, "command").S;
                Ok(logicalCommand.Mt == 21 && logicalCommand.Mr == 19 && logicalCommand.Mb == 23 && logicalCommand.Ml == 17 &&
                    logicalCommand.Pt == 10 && logicalCommand.Pr == 8 && logicalCommand.Pb == 12 && logicalCommand.Pl == 6,
                    "familiar logical box styles reach compact records");
                Eq(12, logicalBoxEngine.Ck(logicalBoxPage, ById(logicalBoxPage.D, "clear-box")), "logical box DOM reset");
                logicalCommand = BlockById(logicalBoxPage, "command").S;
                Ok(logicalCommand.Mt == 6 && logicalCommand.Mr == 5 && logicalCommand.Mb == 7 && logicalCommand.Ml == 4 &&
                    logicalCommand.Pt == 1 && logicalCommand.Pr == 3 && logicalCommand.Pb == 2 && logicalCommand.Pl == 2 && logicalCommand.F == "#123f8c",
                    "logical box DOM reset restores authored cascade");
                Eq(0, logicalBoxEngine.Js(logicalBoxPage, "mi('command','1px 2px 3px');pi('command','-1px');mis('command','2px 3px');mie('command','auto')"),
                    "malformed logical box commands rejected");
                Ok(BlockById(logicalBoxPage, "command").S.Ml == 4 && BlockById(logicalBoxPage, "command").S.Pr == 3,
                    "rejected logical box commands preserve records");

                var logicalBorderPath = Path.Combine(pages, "logical-borders.html");
                var logicalBorderEngine = new E();
                var logicalBorderPage = logicalBorderEngine.Go(logicalBorderPath);
                Eq(0, logicalBorderPage.Jc, "logical border fixture has no startup commands");
                var logicalBorderAxis = BlockById(logicalBorderPage, "axis").S;
                Ok(logicalBorderAxis.Bt == 6 && logicalBorderAxis.Br == 4 && logicalBorderAxis.Bb == 6 && logicalBorderAxis.Bl == 4 &&
                    logicalBorderAxis.Bsv(0) == "dashed" && logicalBorderAxis.Bsv(1) == "solid" && logicalBorderAxis.Bsv(2) == "dashed" && logicalBorderAxis.Bsv(3) == "solid" &&
                    logicalBorderAxis.Bcv(0) == "#176b4d" && logicalBorderAxis.Bcv(1) == "#123f8c" && logicalBorderAxis.Bcv(2) == "#176b4d" && logicalBorderAxis.Bcv(3) == "#123f8c",
                    "logical border axis shorthands");
                var logicalBorderLonghand = BlockById(logicalBorderPage, "longhand").S;
                Ok(logicalBorderLonghand.Bt == 4 && logicalBorderLonghand.Br == 3 && logicalBorderLonghand.Bb == 5 && logicalBorderLonghand.Bl == 2 &&
                    logicalBorderLonghand.Bsv(0) == "dashed" && logicalBorderLonghand.Bsv(1) == "solid" && logicalBorderLonghand.Bsv(2) == "double" && logicalBorderLonghand.Bsv(3) == "dotted" &&
                    logicalBorderLonghand.Bcv(0) == "#123f8c" && logicalBorderLonghand.Bcv(1) == "#176b4d" && logicalBorderLonghand.Bcv(2) == "#b7791f" && logicalBorderLonghand.Bcv(3) == "#a13b18",
                    "logical border edge components");
                var logicalBorderComponents = BlockById(logicalBorderPage, "axis-components").S;
                Ok(logicalBorderComponents.Bt == 11 && logicalBorderComponents.Br == 9 && logicalBorderComponents.Bb == 13 && logicalBorderComponents.Bl == 7 &&
                    logicalBorderComponents.Bsv(0) == "dashed" && logicalBorderComponents.Bsv(1) == "dotted" && logicalBorderComponents.Bsv(2) == "double" && logicalBorderComponents.Bsv(3) == "solid",
                    "logical border axis components");
                var logicalBorderSource = BlockById(logicalBorderPage, "source").S;
                Ok(logicalBorderSource.Bt == 1 && logicalBorderSource.Br == 1 && logicalBorderSource.Bb == 1 && logicalBorderSource.Bl == 5 &&
                    logicalBorderSource.Bsv(0) == "double" && logicalBorderSource.Bsv(1) == "solid" && logicalBorderSource.Bsv(3) == "dashed" &&
                    logicalBorderSource.Bcv(1) == "#176b4d" && logicalBorderSource.Bcv(3) == "#123f8c",
                    "physical and logical border source order");
                var physicalBorder = BlockById(logicalBorderPage, "physical").S;
                Ok(physicalBorder.Bt == 1 && physicalBorder.Br == 2 && physicalBorder.Bb == 3 && physicalBorder.Bl == 4 &&
                    physicalBorder.Bsv(0) == "solid" && physicalBorder.Bsv(1) == "dashed" && physicalBorder.Bsv(2) == "dotted" && physicalBorder.Bsv(3) == "double" &&
                    physicalBorder.Bcv(0) == "#111111" && physicalBorder.Bcv(1) == "#222222" && physicalBorder.Bcv(2) == "#333333" && physicalBorder.Bcv(3) == "#444444" && physicalBorder.Bec == 8,
                    "physical border component lists");
                var logicalBorderVariables = BlockById(logicalBorderPage, "variables").S;
                Ok(logicalBorderVariables.Bl == 8 && logicalBorderVariables.Br == 10 && logicalBorderVariables.Bcv(3) == "rgb(18 63 140)" && logicalBorderVariables.Bcv(1) == "rgba(23, 107, 77, .8)",
                    "logical border variables and functional colors");
                var logicalBorderImportant = BlockById(logicalBorderPage, "important").S;
                Ok(logicalBorderImportant.Bl == 4 && logicalBorderImportant.Br == 11 && logicalBorderImportant.Bsv(3) == "solid", "logical border important cascade");
                var logicalBorderInherited = BlockById(logicalBorderPage, "inherit-child").S;
                Ok(logicalBorderInherited.Bt == 2 && logicalBorderInherited.Br == 3 && logicalBorderInherited.Bb == 4 && logicalBorderInherited.Bl == 5 &&
                    logicalBorderInherited.Bsv(0) == "dotted" && logicalBorderInherited.Bcv(3) == "#b7791f", "explicit logical border inheritance");
                var logicalBorderReset = BlockById(logicalBorderPage, "reset").S;
                Ok(logicalBorderReset.Bt == 0 && logicalBorderReset.Br == 0 && logicalBorderReset.Bb == 0 && logicalBorderReset.Bl == 0 &&
                    logicalBorderReset.Bsv(0) == "" && logicalBorderReset.Bcv(3) == "", "logical border initial and unset reset");
                var logicalBorderInvalid = BlockById(logicalBorderPage, "invalid").S;
                Ok(logicalBorderInvalid.Bt == 0 && logicalBorderInvalid.Br == 7 && logicalBorderInvalid.Bb == 0 && logicalBorderInvalid.Bl == 8 &&
                    logicalBorderInvalid.Bsv(0) == "dashed" && logicalBorderInvalid.Bsv(2) == "dotted" && logicalBorderInvalid.Bsv(3) == "dotted" &&
                    logicalBorderInvalid.Bcv(1) == "#176b4d" && logicalBorderInvalid.Bcv(3) == "#a13b18", "malformed logical border declarations are atomic");
                Eq(6, logicalBorderEngine.Ck(logicalBorderPage, ById(logicalBorderPage.D, "set-short")), "short logical border shorthand aliases");
                var logicalBorderCommand = BlockById(logicalBorderPage, "command").S;
                Ok(logicalBorderCommand.Bt == 8 && logicalBorderCommand.Br == 7 && logicalBorderCommand.Bb == 9 && logicalBorderCommand.Bl == 6 &&
                    logicalBorderCommand.Bsv(0) == "double" && logicalBorderCommand.Bcv(3) == "#123f8c", "short logical border shorthands reach compact records");
                Eq(18, logicalBorderEngine.Ck(logicalBorderPage, ById(logicalBorderPage.D, "set-components")), "short logical border component aliases");
                logicalBorderCommand = BlockById(logicalBorderPage, "command").S;
                Ok(logicalBorderCommand.Bt == 16 && logicalBorderCommand.Br == 15 && logicalBorderCommand.Bb == 17 && logicalBorderCommand.Bl == 14 &&
                    logicalBorderCommand.Bsv(0) == "dashed" && logicalBorderCommand.Bcv(2) == "#b7791f", "short logical border components reach compact records");
                Eq(24, logicalBorderEngine.Ck(logicalBorderPage, ById(logicalBorderPage.D, "dom-all")), "familiar logical border DOM styles");
                logicalBorderCommand = BlockById(logicalBorderPage, "command").S;
                Ok(logicalBorderCommand.Bt == 23 && logicalBorderCommand.Br == 22 && logicalBorderCommand.Bb == 24 && logicalBorderCommand.Bl == 21 &&
                    logicalBorderCommand.Bsv(3) == "dotted" && logicalBorderCommand.Bcv(0) == "#123f8c", "familiar logical border DOM styles reach compact records");
                Eq(24, logicalBorderEngine.Ck(logicalBorderPage, ById(logicalBorderPage.D, "clear-all")), "logical border DOM reset");
                logicalBorderCommand = BlockById(logicalBorderPage, "command").S;
                Ok(logicalBorderCommand.Bt == 2 && logicalBorderCommand.Br == 2 && logicalBorderCommand.Bb == 2 && logicalBorderCommand.Bl == 2 &&
                    logicalBorderCommand.Bsv(0) == "solid" && logicalBorderCommand.Bcv(3) == "#123f8c" && logicalBorderCommand.Bec == 0 && logicalBorderCommand.F == "#123f8c",
                    "logical border DOM reset restores uniform authored border");
                Eq(0, logicalBorderEngine.Js(logicalBorderPage, "biw('command','2px bad');biy('command','solid groove');bic('command','red #xyz');siw('command','wide')"),
                    "malformed logical border commands rejected");
                Ok(BlockById(logicalBorderPage, "command").S.Bl == 2 && BlockById(logicalBorderPage, "command").S.Bsv(3) == "solid",
                    "rejected logical border commands preserve records");
                Eq(1, logicalBorderEngine.Js(logicalBorderPage, "bi('command','THICK SOLID RED')"), "case-insensitive logical border keywords");
                Ok(BlockById(logicalBorderPage, "command").S.Bl == 5 && BlockById(logicalBorderPage, "command").S.Br == 5 &&
                    BlockById(logicalBorderPage, "command").S.Bsv(3) == "solid" && BlockById(logicalBorderPage, "command").S.Bcv(1) == "RED",
                    "uppercase logical border keywords reach compact records");

                var directionPath = Path.Combine(pages, "direction-logical.html");
                var directionEngine = new E();
                var directionPage = directionEngine.Go(directionPath);
                Eq(0, directionPage.Jc, "direction fixture has no startup commands");
                var directionOrder = BlockById(directionPage, "order").S;
                Ok(directionOrder.Dr == "r" && directionOrder.Mr == 11 && directionOrder.Pl == 13 && directionOrder.Br == 3 &&
                    directionOrder.R0 == 17 && directionOrder.Q0 == 19,
                    "direction resolves before logical declarations regardless of source order");
                var directionAttribute = BlockById(directionPage, "attribute").S;
                Ok(directionAttribute.Dr == "r" && directionAttribute.Mr == 21 && directionAttribute.Pl == 23,
                    "HTML dir maps logical edges");
                Ok(BlockById(directionPage, "attribute-override").S.Dr == "l" && BlockById(directionPage, "attribute-override").S.Ml == 25,
                    "CSS direction overrides HTML dir");
                var directionVariable = BlockById(directionPage, "variable").S;
                var directionImportant = BlockById(directionPage, "important").S;
                Ok(directionVariable.Dr == "r" && directionVariable.Mr == 27 && directionVariable.Ta == "r" &&
                    directionImportant.Dr == "l" && directionImportant.Ml == 29 && directionImportant.Ta == "r",
                    "direction variables, important cascade, and logical text alignment");
                var directionInset = BlockById(directionPage, "inset").S;
                var directionInsetLong = BlockById(directionPage, "inset-long").S;
                Ok(directionInset.Rt == 12 && directionInset.Lt == 18 && directionInset.Tp == 14 && directionInset.Bm == 16 &&
                    directionInsetLong.Rt == 23 && directionInsetLong.Lt == 25 && directionInsetLong.Tp == 27 && directionInsetLong.Bm == 29,
                    "RTL logical inset axes and edges");
                var directionBoxes = BlockById(directionPage, "boxes").S;
                Ok(directionBoxes.Mt == 32 && directionBoxes.Mr == 31 && directionBoxes.Mb == 34 && directionBoxes.Ml == 33 &&
                    directionBoxes.Pt == 42 && directionBoxes.Pr == 41 && directionBoxes.Pb == 44 && directionBoxes.Pl == 43,
                    "RTL logical margin and padding mapping");
                var directionBorders = BlockById(directionPage, "borders").S;
                Ok(directionBorders.Bt == 52 && directionBorders.Br == 51 && directionBorders.Bb == 54 && directionBorders.Bl == 53 &&
                    directionBorders.Bsv(0) == "solid" && directionBorders.Bsv(1) == "dashed" && directionBorders.Bsv(2) == "double" && directionBorders.Bsv(3) == "dotted" &&
                    directionBorders.Bcv(0) == "#176b4d" && directionBorders.Bcv(1) == "#123f8c" && directionBorders.Bcv(2) == "#b7791f" && directionBorders.Bcv(3) == "#a13b18",
                    "RTL logical border axes and components");
                var directionRadii = BlockById(directionPage, "radii").S;
                Ok(directionRadii.R0 == 62 && directionRadii.R1 == 61 && directionRadii.R2 == 63 && directionRadii.R3 == 64 &&
                    directionRadii.Q0 == 72 && directionRadii.Q1 == 71 && directionRadii.Q2 == 73 && directionRadii.Q3 == 74,
                    "RTL logical corner mapping");
                var crossDirection = BlockById(directionPage, "child").S;
                Ok(crossDirection.Dr == "l" && crossDirection.Ml == 31 && crossDirection.Mr == 33 && crossDirection.Pl == 41 && crossDirection.Pr == 43 &&
                    crossDirection.Bl == 51 && crossDirection.Br == 53 && crossDirection.Bsv(3) == "dashed" && crossDirection.Bsv(1) == "dotted" &&
                    crossDirection.Bcv(3) == "#123f8c" && crossDirection.Bcv(1) == "#a13b18" &&
                    crossDirection.R0 == 62 && crossDirection.R1 == 61 && crossDirection.R2 == 64 && crossDirection.R3 == 63 &&
                    crossDirection.Q0 == 72 && crossDirection.Q1 == 71 && crossDirection.Q2 == 74 && crossDirection.Q3 == 73,
                    "explicit logical inheritance translates parent and child directions");
                Ok(BlockById(directionPage, "align-start").S.Ta == "r" && BlockById(directionPage, "align-end").S.Ta == "l",
                    "RTL start and end text alignment");
                Ok(BlockById(directionPage, "unset-direction").S.Dr == "r" && BlockById(directionPage, "unset-direction").S.Mr == 35 &&
                    BlockById(directionPage, "initial-direction").S.Dr == "l" && BlockById(directionPage, "initial-direction").S.Ml == 37 &&
                    BlockById(directionPage, "revert-direction").S.Dr == "r" && BlockById(directionPage, "revert-direction").S.Mr == 39,
                    "inherited, initial, unset, and HTML-backed revert direction");
                Eq(1, directionEngine.Ck(directionPage, ById(directionPage.D, "set-short")), "short direction alias");
                Ok(BlockById(directionPage, "command").S.Dr == "r" && BlockById(directionPage, "command").S.Mr == 5 &&
                    BlockById(directionPage, "command").S.R1 == 6 && BlockById(directionPage, "command").S.Q1 == 7,
                    "short direction alias remaps authored logical styles");
                Eq(1, directionEngine.Ck(directionPage, ById(directionPage.D, "set-readable")), "readable direction alias");
                Ok(BlockById(directionPage, "command").S.Dr == "l" && BlockById(directionPage, "command").S.Ml == 5 && BlockById(directionPage, "command").S.R0 == 6,
                    "readable direction alias restores LTR mapping");
                Eq(1, directionEngine.Ck(directionPage, ById(directionPage.D, "set-dom")), "literal DOM direction style");
                Ok(BlockById(directionPage, "command").S.Dr == "r" && BlockById(directionPage, "command").S.Mr == 5, "DOM direction remaps logical styles");
                Eq(1, directionEngine.Ck(directionPage, ById(directionPage.D, "clear-dom")), "empty DOM direction reset");
                Ok(BlockById(directionPage, "command").S.Dr == "l" && BlockById(directionPage, "command").S.Ml == 5 && BlockById(directionPage, "command").S.F == "#123f8c",
                    "empty DOM direction restores authored cascade and preserves unrelated style");
                Eq(1, directionEngine.Js(directionPage, "dr('rtl')"), "body direction short alias");
                Ok(BlockById(directionPage, "body-probe").S.Dr == "r" && BlockById(directionPage, "body-probe").S.Mr == 9,
                    "body direction alias reaches descendants");
                Eq(1, directionEngine.Js(directionPage, "document.body.style.direction='ltr'"), "body DOM direction assignment");
                Ok(BlockById(directionPage, "body-probe").S.Dr == "l" && BlockById(directionPage, "body-probe").S.Ml == 9,
                    "body DOM direction assignment remaps descendants");
                Eq(1, directionEngine.Js(directionPage, "document.body.style.direction=''"), "body DOM direction reset");
                Eq(0, directionEngine.Js(directionPage, "dr('command','sideways');direction('command','vertical-rl')"), "unsupported directions rejected");
                Ok(BlockById(directionPage, "command").S.Dr == "l" && BlockById(directionPage, "command").S.Ml == 5,
                    "rejected directions preserve authored state");

                var directionLayoutPath = Path.Combine(pages, "direction-layout.html");
                var directionLayoutPage = new E().Go(directionLayoutPath);
                var directionLayoutFlex = BlockById(directionLayoutPage, "flex-rtl");
                var directionLayoutA = BlockById(directionLayoutPage, "flex-rtl-a");
                var directionLayoutB = BlockById(directionLayoutPage, "flex-rtl-b");
                Ok(directionLayoutFlex.S.Dr == "r" && directionLayoutFlex.S.Fd == "r" &&
                    Object.ReferenceEquals(directionLayoutA.Up, directionLayoutFlex) && directionLayoutPage.Bs.IndexOf(directionLayoutA) < directionLayoutPage.Bs.IndexOf(directionLayoutB),
                    "direction layout keeps compact state, ownership, and source order");
                Ok(BlockById(directionLayoutPage, "grid-dense").S.Gf == "d" && BlockById(directionLayoutPage, "grid-column-flow").S.Gf == "c" &&
                    BlockById(directionLayoutPage, "grid-area").S.Aa == 2,
                    "direction layout reuses bounded grid flow and area records");
                using (var directionView = new V())
                {
                    directionView.Size = new Size(720, 1600);
                    directionView.Go(directionLayoutPath);
                    using (var directionPaint = Render(directionView))
                    {
                        var ltr = BlockRect(directionView, BlockById(directionView.At, "flex-ltr"));
                        var ltrA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-ltr-a"));
                        var ltrB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-ltr-b"));
                        Eq(ltr.Left, ltrA.Left, "LTR row starts at the physical left");
                        Eq(10, ltrB.Left - ltrA.Right, "LTR row gap");

                        var rtl = BlockRect(directionView, BlockById(directionView.At, "flex-rtl"));
                        var rtlA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-a"));
                        var rtlB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-b"));
                        var rtlC = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-c"));
                        Eq(rtl.Right, rtlA.Right, "RTL row starts at inline end");
                        Eq(10, rtlA.Left - rtlB.Right, "RTL row first gap");
                        Ok(rtlA.Left > rtlB.Left && rtlB.Left > rtlC.Left, "RTL row visual direction");

                        var rtlEnd = BlockRect(directionView, BlockById(directionView.At, "flex-rtl-end"));
                        var rtlEndA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-end-a"));
                        var rtlEndB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-end-b"));
                        Eq(rtlEnd.Left, rtlEndB.Left, "RTL flex-end uses physical left edge");
                        Eq(90, rtlEndA.Right - rtlEnd.Left, "RTL flex-end group width");

                        var rtlReverse = BlockRect(directionView, BlockById(directionView.At, "flex-rtl-reverse"));
                        var rtlReverseA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-reverse-a"));
                        var rtlReverseB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-rtl-reverse-b"));
                        Eq(rtlReverse.Left, rtlReverseA.Left, "RTL row-reverse cancels inline reversal");
                        Eq(10, rtlReverseB.Left - rtlReverseA.Right, "RTL row-reverse gap");

                        var ordered = BlockRect(directionView, BlockById(directionView.At, "flex-order"));
                        var orderA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-order-a"));
                        var orderB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-order-b"));
                        var orderC = BlockOuterRect(directionView, BlockById(directionView.At, "flex-order-c"));
                        Eq(ordered.Right, orderB.Right, "RTL order-modified first item reaches inline start");
                        Ok(orderB.Left > orderA.Left && orderA.Left > orderC.Left, "RTL order remains stable before direction mapping");

                        var wrap = BlockRect(directionView, BlockById(directionView.At, "flex-wrap"));
                        var wrapA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-wrap-a"));
                        var wrapB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-wrap-b"));
                        var wrapC = BlockOuterRect(directionView, BlockById(directionView.At, "flex-wrap-c"));
                        var wrapD = BlockOuterRect(directionView, BlockById(directionView.At, "flex-wrap-d"));
                        Eq(wrap.Right, wrapA.Right, "RTL wrapped first line starts at inline start");
                        Eq(wrap.Right, wrapC.Right, "RTL wrapped second line starts at inline start");
                        Ok(wrapA.Top == wrapB.Top && wrapC.Top == wrapD.Top && wrapC.Top > wrapA.Bottom, "RTL wrapping keeps source line groups");

                        var column = BlockRect(directionView, BlockById(directionView.At, "flex-column"));
                        var columnA = BlockOuterRect(directionView, BlockById(directionView.At, "flex-column-a"));
                        var columnB = BlockOuterRect(directionView, BlockById(directionView.At, "flex-column-b"));
                        Eq(column.Right, columnA.Right, "RTL column flex start cross alignment");
                        Eq(column.Left, columnB.Left, "RTL column flex end cross alignment");

                        var grid = BlockRect(directionView, BlockById(directionView.At, "grid-auto"));
                        var gridA = BlockOuterRect(directionView, BlockById(directionView.At, "grid-auto-a"));
                        var gridB = BlockOuterRect(directionView, BlockById(directionView.At, "grid-auto-b"));
                        var gridC = BlockOuterRect(directionView, BlockById(directionView.At, "grid-auto-c"));
                        Eq(grid.Right, gridA.Right, "RTL grid auto placement starts at inline start");
                        Ok(gridA.Size == new Size(40, 25) && gridB.Size == new Size(60, 25) && gridC.Size == new Size(80, 25), "RTL grid preserves logical track sizes");
                        Eq(10, gridA.Left - gridB.Right, "RTL unequal grid first gap");
                        Eq(10, gridB.Left - gridC.Right, "RTL unequal grid second gap");

                        var explicitGrid = BlockRect(directionView, BlockById(directionView.At, "grid-explicit"));
                        var explicitSpan = BlockOuterRect(directionView, BlockById(directionView.At, "grid-explicit-span"));
                        var explicitSide = BlockOuterRect(directionView, BlockById(directionView.At, "grid-explicit-side"));
                        Ok(explicitSpan.Width == 110 && explicitSpan.Right == explicitGrid.Right && explicitSide.Width == 80 && explicitSide.Left - explicitGrid.Left == 20,
                            "RTL explicit grid lines and spans map physically");

                        var denseA = BlockOuterRect(directionView, BlockById(directionView.At, "grid-dense-a"));
                        var denseB = BlockOuterRect(directionView, BlockById(directionView.At, "grid-dense-b"));
                        var denseC = BlockOuterRect(directionView, BlockById(directionView.At, "grid-dense-c"));
                        Ok(denseC.Top == denseA.Top && denseB.Top > denseA.Bottom && denseA.Left - denseC.Right == 10,
                            "RTL dense grid backfills the logical final column");

                        var columnGridA = BlockOuterRect(directionView, BlockById(directionView.At, "grid-column-a"));
                        var columnGridB = BlockOuterRect(directionView, BlockById(directionView.At, "grid-column-b"));
                        var columnGridC = BlockOuterRect(directionView, BlockById(directionView.At, "grid-column-c"));
                        var columnGridD = BlockOuterRect(directionView, BlockById(directionView.At, "grid-column-d"));
                        Ok(columnGridA.Left == columnGridB.Left && columnGridC.Left == columnGridD.Left && columnGridA.Left - columnGridC.Right == 10 && columnGridA.Top == columnGridC.Top,
                            "RTL column grid flow advances from the physical right");

                        var areaGrid = BlockRect(directionView, BlockById(directionView.At, "grid-area"));
                        var areaLead = BlockOuterRect(directionView, BlockById(directionView.At, "grid-area-lead"));
                        var areaSide = BlockOuterRect(directionView, BlockById(directionView.At, "grid-area-side"));
                        Ok(areaLead.Width == 110 && areaLead.Right == areaGrid.Right && areaSide.Width == 80 && areaLead.Left - areaSide.Right == 10,
                            "RTL named grid areas retain logical spans");

                        var contentEnd = BlockRect(directionView, BlockById(directionView.At, "grid-content-end"));
                        var contentEndA = BlockOuterRect(directionView, BlockById(directionView.At, "grid-content-end-a"));
                        var contentEndB = BlockOuterRect(directionView, BlockById(directionView.At, "grid-content-end-b"));
                        Eq(contentEnd.Left, contentEndB.Left, "RTL grid content end maps to physical left");
                        Eq(110, contentEndA.Right - contentEnd.Left, "RTL grid content group width");

                        var selfGrid = BlockRect(directionView, BlockById(directionView.At, "grid-self"));
                        var selfStart = BlockOuterRect(directionView, BlockById(directionView.At, "grid-self-start"));
                        var selfEnd = BlockOuterRect(directionView, BlockById(directionView.At, "grid-self-end"));
                        Eq(selfGrid.Right, selfStart.Right, "RTL grid justify-items start");
                        Eq(selfGrid.Left, selfEnd.Left, "RTL grid justify-self end");
                        Ok(ColorDistance(CenterPixel(directionPaint, gridA), Color.FromArgb(220, 236, 255)) < 8, "direction-aware grid paint");
                    }

                    var command = BlockRect(directionView, BlockById(directionView.At, "command-flex"));
                    var commandA = BlockOuterRect(directionView, BlockById(directionView.At, "command-flex-a"));
                    Eq(command.Left, commandA.Left, "direction command flex starts LTR");
                    Eq(1, directionView.Js("dr('command-flex','rtl')"), "direction command triggers layout rebuild");
                    Paint(directionView);
                    command = BlockRect(directionView, BlockById(directionView.At, "command-flex"));
                    commandA = BlockOuterRect(directionView, BlockById(directionView.At, "command-flex-a"));
                    Eq(command.Right, commandA.Right, "direction command remaps flex geometry");
                }
                Console.WriteLine("Lea direction-aware layout smoke passed.");

                var viewportPath = Path.Combine(pages, "viewport-units.html");
                var viewportEngine = new E();
                var viewportPage = viewportEngine.Go(viewportPath);
                Eq(0, viewportPage.Jc, "viewport unit fixture has no startup commands");
                var viewportWidthStyle = BlockById(viewportPage, "vw").S;
                Ok(viewportWidthStyle.Wp == 50 && viewportWidthStyle.Wu == 1, "viewport width unit model");
                Ok(BlockById(viewportPage, "vh-width").S.Wp == 25 && BlockById(viewportPage, "vh-width").S.Wu == 2 &&
                    BlockById(viewportPage, "vmin-width").S.Wu == 3 && BlockById(viewportPage, "vmax-width").S.Wu == 4,
                    "cross-axis and minimum/maximum viewport width unit model");
                Ok(BlockById(viewportPage, "calc-width").S.Wp == 50 && BlockById(viewportPage, "calc-width").S.Wu == 1 && BlockById(viewportPage, "calc-width").S.Wo == -20,
                    "calculated viewport width model");
                Ok(BlockById(viewportPage, "vh").S.Hp == 50 && BlockById(viewportPage, "vh").S.Hu == 2 &&
                    BlockById(viewportPage, "vw-height").S.Hu == 1 && BlockById(viewportPage, "vmin-height").S.Hu == 3 && BlockById(viewportPage, "vmax-height").S.Hu == 4,
                    "viewport height unit model");
                Ok(BlockById(viewportPage, "calc-height").S.Hp == 50 && BlockById(viewportPage, "calc-height").S.Hu == 2 && BlockById(viewportPage, "calc-height").S.Ho == -20,
                    "calculated viewport height model");
                var viewportMinimumWidth = BlockById(viewportPage, "minimum-width").S;
                var viewportMaximumWidth = BlockById(viewportPage, "maximum-width").S;
                var viewportMinimumHeight = BlockById(viewportPage, "minimum-height").S;
                var viewportMaximumHeight = BlockById(viewportPage, "maximum-height").S;
                Ok(viewportMinimumWidth.Wnp == 25 && viewportMinimumWidth.Wnu == 1 && viewportMaximumWidth.Wxp == 50 && viewportMaximumWidth.Wxu == 1 && viewportMaximumWidth.Wxo == -5,
                    "viewport minimum and maximum width model");
                Ok(viewportMinimumHeight.Hnp == 25 && viewportMinimumHeight.Hnu == 2 && viewportMaximumHeight.Hxp == 50 && viewportMaximumHeight.Hxu == 2 && viewportMaximumHeight.Hxo == -5,
                    "viewport minimum and maximum height model");
                var viewportInherited = BlockById(viewportPage, "inherit-child").S;
                Ok(viewportInherited.Wp == 40 && viewportInherited.Wu == 3 && viewportInherited.Hp == 30 && viewportInherited.Hu == 4,
                    "inherited viewport unit model");
                var viewportVariable = BlockById(viewportPage, "variable").S;
                Ok(viewportVariable.Wp == 30 && viewportVariable.Wu == 4 && viewportVariable.Wo == 10 &&
                    viewportVariable.Hp == 30 && viewportVariable.Hu == 3 && viewportVariable.Ho == -10,
                    "custom-property viewport unit model");
                Ok(BlockById(viewportPage, "invalid").S.W == 140 && BlockById(viewportPage, "invalid").S.H == 90,
                    "invalid viewport values preserve prior declarations");
                Eq(2, viewportEngine.Ck(viewportPage, ById(viewportPage.D, "set-viewport")), "compact viewport size commands");
                Ok(BlockById(viewportPage, "command").S.Wu == 1 && BlockById(viewportPage, "command").S.Hu == 2,
                    "compact viewport commands reach style model");
                Eq(2, viewportEngine.Ck(viewportPage, ById(viewportPage.D, "dom-viewport")), "literal DOM viewport size commands");
                Ok(BlockById(viewportPage, "command").S.Wu == 2 && BlockById(viewportPage, "command").S.Hu == 1,
                    "literal DOM cross-axis viewport model");
                Eq(2, viewportEngine.Ck(viewportPage, ById(viewportPage.D, "clear-viewport")), "empty viewport size reset");
                Ok(BlockById(viewportPage, "command").S.W == 200 && BlockById(viewportPage, "command").S.H == 120 &&
                    BlockById(viewportPage, "command").S.Wp == 0 && BlockById(viewportPage, "command").S.Wu == 0 &&
                    BlockById(viewportPage, "command").S.Hp == 0 && BlockById(viewportPage, "command").S.Hu == 0,
                    "viewport reset restores authored pixels and releases lazy state");
                Eq(0, viewportEngine.Js(viewportPage, "wd('command','0vw'); ht('command','101vh'); width('command','calc(10vmin * 2)'); height('command','calc(50vmax + 2001px)'); document.getElementById('command').style.height='20dvh';"),
                    "invalid viewport unit commands rejected");
                Ok(BlockById(viewportPage, "command").S.W == 200 && BlockById(viewportPage, "command").S.H == 120,
                    "rejected viewport commands preserve cascade");

                var fontUnitPath = Path.Combine(pages, "font-relative-units.html");
                var fontUnitEngine = new E();
                var fontUnitPage = fontUnitEngine.Go(fontUnitPath);
                Eq(20, fontUnitPage.Rf, "root font-size alias");
                var emStyle = BlockById(fontUnitPage, "em-size").S;
                var remStyle = BlockById(fontUnitPage, "rem-size").S;
                Ok(emStyle.Wp == 10 && emStyle.Wu == 5 && emStyle.Hp == 4 && emStyle.Hu == 5,
                    "em dimension model");
                Ok(remStyle.Wp == 10 && remStyle.Wu == 6 && remStyle.Hp == 4 && remStyle.Hu == 6,
                    "rem dimension model");
                var fontCalc = BlockById(fontUnitPage, "calc-font").S;
                Ok(fontCalc.Wp == 10 && fontCalc.Wu == 5 && fontCalc.Wo == -20 &&
                    fontCalc.Hp == 5 && fontCalc.Hu == 6 && fontCalc.Ho == 10,
                    "calculated font-relative model");
                Ok(BlockById(fontUnitPage, "minimum-font-width").S.Wnu == 5 &&
                    BlockById(fontUnitPage, "maximum-root-width").S.Wxu == 6 &&
                    BlockById(fontUnitPage, "minimum-font-height").S.Hnu == 5 &&
                    BlockById(fontUnitPage, "maximum-root-height").S.Hxu == 6,
                    "font-relative minimum and maximum model");
                var inheritedFontUnit = BlockById(fontUnitPage, "font-inherit-child").S;
                Ok(inheritedFontUnit.Wp == 8 && inheritedFontUnit.Wu == 5 && inheritedFontUnit.Hp == 4 && inheritedFontUnit.Hu == 6,
                    "inherited font-relative model");
                var variableFontUnit = BlockById(fontUnitPage, "font-variable").S;
                Ok(variableFontUnit.Wp == 9 && variableFontUnit.Wu == 5 && variableFontUnit.Hp == 4 && variableFontUnit.Hu == 6 && variableFontUnit.Ho == -5,
                    "custom-property font-relative model");
                Ok(BlockById(fontUnitPage, "font-invalid").S.W == 130 && BlockById(fontUnitPage, "font-invalid").S.H == 85 &&
                    BlockById(fontUnitPage, "invalid-font-size").S.Fs == 18,
                    "invalid font-relative values and unsupported font-size units preserve prior state");
                Eq(2, fontUnitEngine.Ck(fontUnitPage, ById(fontUnitPage.D, "set-font-size")), "compact font-relative size commands");
                Ok(BlockById(fontUnitPage, "font-command").S.Wu == 5 && BlockById(fontUnitPage, "font-command").S.Hu == 6,
                    "compact font-relative commands reach style model");
                Eq(2, fontUnitEngine.Ck(fontUnitPage, ById(fontUnitPage.D, "dom-font-size")), "literal DOM font-relative commands");
                Ok(BlockById(fontUnitPage, "font-command").S.Wu == 6 && BlockById(fontUnitPage, "font-command").S.Hu == 5,
                    "literal DOM font-relative model");
                Eq(2, fontUnitEngine.Ck(fontUnitPage, ById(fontUnitPage.D, "clear-font-size")), "font-relative inline reset");
                Ok(BlockById(fontUnitPage, "font-command").S.W == 200 && BlockById(fontUnitPage, "font-command").S.H == 120 &&
                    BlockById(fontUnitPage, "font-command").S.Wu == 0 && BlockById(fontUnitPage, "font-command").S.Hu == 0,
                    "font-relative reset restores authored pixels and releases lazy state");
                Eq(0, fontUnitEngine.Js(fontUnitPage, "wd('font-command','0em'); ht('font-command','101rem'); width('font-command','1.5em'); height('font-command','calc(2rem * 2)');"),
                    "invalid font-relative commands rejected");
                Ok(BlockById(fontUnitPage, "font-command").S.W == 200 && BlockById(fontUnitPage, "font-command").S.H == 120,
                    "rejected font-relative commands preserve cascade");

                var sizeFunctionPath = Path.Combine(pages, "size-functions.html");
                var sizeFunctionEngine = new E();
                var sizeFunctionPage = sizeFunctionEngine.Go(sizeFunctionPath);
                var minFunction = BlockById(sizeFunctionPage, "min-two").S;
                Ok(minFunction.Wk == 1 && minFunction.Hk == 1 && minFunction.Wq(0) == 2 && minFunction.Hq(0) == 2 &&
                    minFunction.Wv(0, 0, 1) == 75 && minFunction.Wv(0, 0, 3) == 0 && minFunction.Wv(0, 1, 0) == 500,
                    "bounded minimum function model and compact values");
                var maxFunction = BlockById(sizeFunctionPage, "max-three").S;
                Ok(maxFunction.Wk == 2 && maxFunction.Wq(0) == 3 && maxFunction.Wv(0, 2, 1) == 8 && maxFunction.Wv(0, 2, 3) == 6,
                    "bounded maximum function model and compact values");
                var clampFunction = BlockById(sizeFunctionPage, "clamp-main").S;
                Ok(clampFunction.Wk == 3 && clampFunction.Hk == 3 && clampFunction.Wq(0) == 3 && clampFunction.Hq(0) == 3 &&
                    clampFunction.Wv(0, 0, 0) == 200 && clampFunction.Wv(0, 1, 1) == 50 && clampFunction.Wv(0, 2, 0) == 400,
                    "bounded clamp function model and compact values");
                var functionLimits = BlockById(sizeFunctionPage, "function-limits").S;
                Ok(functionLimits.Wnk == 2 && functionLimits.Wxk == 1 && functionLimits.Hnk == 2 && functionLimits.Hxk == 1 &&
                    functionLimits.Wq(1) == 2 && functionLimits.Wq(2) == 2 && functionLimits.Hq(1) == 2 && functionLimits.Hq(2) == 2,
                    "size functions in minimum and maximum properties");
                var inheritedFunction = BlockById(sizeFunctionPage, "function-inherit-child").S;
                Ok(inheritedFunction.Wk == 3 && inheritedFunction.Hk == 1 && inheritedFunction.Wq(0) == 3 && inheritedFunction.Hq(0) == 2 &&
                    inheritedFunction.Wv(0, 1, 1) == 15 && inheritedFunction.Wv(0, 1, 3) == 6,
                    "inherited size function records");
                var variableFunction = BlockById(sizeFunctionPage, "function-variable").S;
                Ok(variableFunction.Wk == 2 && variableFunction.Hk == 3 && variableFunction.Wq(0) == 2 && variableFunction.Hq(0) == 3,
                    "custom-property size function records");
                var mediaFunction = BlockById(sizeFunctionPage, "media-function").S;
                Ok(mediaFunction.Wk == 3 && mediaFunction.Wq(0) == 3,
                    "wide media size function record");
                var invalidSizeFunction = BlockById(sizeFunctionPage, "function-invalid").S;
                Ok(invalidSizeFunction.W == 130 && invalidSizeFunction.H == 85 && invalidSizeFunction.Wk == 0 && invalidSizeFunction.Hk == 0 &&
                    invalidSizeFunction.Wq(0) == 0 && invalidSizeFunction.Hq(0) == 0,
                    "invalid size functions preserve earlier declarations atomically");
                Ok(BlockById(sizeFunctionPage, "zero-first").S.Wk == 2 && BlockById(sizeFunctionPage, "function-auto-child").S.Hk == 2,
                    "zero-first and unresolved-height function model");
                Ok(emStyle.Wk == 0 && emStyle.Hk == 0 && emStyle.Wq(0) == 0 && emStyle.Hq(0) == 0,
                    "ordinary responsive dimensions do not allocate function state");
                Eq(2, sizeFunctionEngine.Ck(sizeFunctionPage, ById(sizeFunctionPage.D, "set-function-size")), "compact size function commands");
                Ok(BlockById(sizeFunctionPage, "function-command").S.Wk == 3 && BlockById(sizeFunctionPage, "function-command").S.Hk == 3,
                    "compact size function commands reach model");
                Eq(2, sizeFunctionEngine.Ck(sizeFunctionPage, ById(sizeFunctionPage.D, "dom-function-size")), "literal DOM size function commands");
                Ok(BlockById(sizeFunctionPage, "function-command").S.Wk == 2 && BlockById(sizeFunctionPage, "function-command").S.Hk == 1,
                    "literal DOM size function model");
                Eq(2, sizeFunctionEngine.Ck(sizeFunctionPage, ById(sizeFunctionPage.D, "clear-function-size")), "size function inline reset");
                Ok(BlockById(sizeFunctionPage, "function-command").S.W == 220 && BlockById(sizeFunctionPage, "function-command").S.H == 120 &&
                    BlockById(sizeFunctionPage, "function-command").S.Wk == 0 && BlockById(sizeFunctionPage, "function-command").S.Hk == 0,
                    "size function reset restores authored pixels and releases lazy state");
                Eq(0, sizeFunctionEngine.Js(sizeFunctionPage,
                    "wd('function-command','min(100px)');ht('function-command','clamp(20px,30px)');width('function-command','max(1px,2px,3px,4px)');height('function-command','min(max(1px,2px),3px)');"),
                    "invalid size function commands rejected");
                Ok(BlockById(sizeFunctionPage, "function-command").S.W == 220 && BlockById(sizeFunctionPage, "function-command").S.H == 120,
                    "rejected size function commands preserve cascade");

                var scriptsOff = new E();
                scriptsOff.Je = false;
                var disabledScriptPage = scriptsOff.Go(scriptPath);
                Eq("Script Before", disabledScriptPage.T, "disabled page scripts keep HTML title");
                Eq(0, disabledScriptPage.Jc, "disabled page command count");
                Eq(0, disabledScriptPage.Ms.Count, "disabled alerts");
                Eq(0, scriptsOff.Js(disabledScriptPage, "t('Disabled manual command');"), "manual commands respect script setting");

                var boundedEngine = new E();
                var boundedPage = boundedEngine.Go(forms);
                var boundedCode = new StringBuilder();
                for (var commandIndex = 0; commandIndex < 40; commandIndex++) boundedCode.Append("t('T").Append(commandIndex).Append("');");
                Eq(32, boundedEngine.Js(boundedPage, boundedCode.ToString()), "manual command limit");
                Eq("T31", boundedPage.T, "commands after limit ignored");
                Eq(0, boundedEngine.Js(boundedPage, new String(' ', 16 * 1024) + "t('oversized');"), "oversized code ignored");
                Eq("T31", boundedPage.T, "oversized code does not mutate page");
                var alertEngine = new E();
                var alertPage = alertEngine.Go(forms);
                var alertCode = new StringBuilder();
                for (var alertIndex = 0; alertIndex < 20; alertIndex++) alertCode.Append("a('A").Append(alertIndex).Append("');");
                Eq(16, alertEngine.Js(alertPage, alertCode.ToString()), "alert queue limit");
                Eq(16, alertPage.Ms.Count, "bounded alert queue");

                var eventPath = Path.Combine(pages, "events-vars.html");
                var eventEngine = new E();
                var eventPage = eventEngine.Go(eventPath);
                Eq("Lea safe events", eventPage.T, "variable-expanded page title");
                Eq(4, eventPage.Jc, "variable page command count");
                Eq(2, eventPage.Vs.Count, "bounded page variables");
                Eq("Lea", eventPage.Vs["who"], "short variable value");
                Eq("#e0f2fe", eventPage.S.Bg, "variable-expanded style");
                Eq(3, eventEngine.Ck(eventPage, ById(eventPage.D, "go")), "safe data-lea click commands");
                Eq("Clicked Lea", eventPage.T, "click-updated title");
                Eq("Hello Lea", ById(eventPage.D, "status").C[0].X, "click-updated element text");
                Eq("Safe click ran", eventPage.Ms[0], "click alert");
                Ok(eventPage.X.Contains("Hello Lea"), "text update rebuilds page text");
                Eq(1, eventEngine.Ck(eventPage, ById(eventPage.D, "familiar")), "familiar onclick alias");
                Eq("Familiar click command", ById(eventPage.D, "status").C[0].X, "onclick text update");
                Eq(2, eventEngine.Ck(eventPage, ById(eventPage.D, "continue")), "link click commands");
                Eq("link", eventPage.Vs["last"], "link click variable");
                Eq(0, eventEngine.Ck(eventPage, ById(eventPage.D, "unsafe")), "unsafe click code ignored");
                var eventDiagnostics = eventEngine.Dg(eventPage);
                Eq(eventPage.U, eventDiagnostics.U, "diagnostic page URL");
                Eq("Clicked Lea", eventDiagnostics.T, "diagnostic title");
                Eq(14, eventDiagnostics.N, "diagnostic node count");
                Eq(6, eventDiagnostics.B, "diagnostic block count");
                Eq(1, eventDiagnostics.L, "diagnostic link count");
                Eq(3, eventDiagnostics.F, "diagnostic field count");
                Eq(5, eventDiagnostics.C, "diagnostic CSS rule count");
                Eq(10, eventDiagnostics.J, "diagnostic command count");
                Eq(3, eventDiagnostics.V, "diagnostic variable count");
                Eq(2, eventDiagnostics.M, "diagnostic alert count");
                Ok(eventDiagnostics.Ns.Count <= 512 && Has(eventDiagnostics.Ns, "    body"), "bounded diagnostic node tree");
                Ok(eventDiagnostics.Cs.Count == 5 && Has(eventDiagnostics.Cs, "button"), "compiled CSS diagnostics");
                Ok(eventDiagnostics.Rs.Count == 1 && Has(eventDiagnostics.Rs, "GET 200 file"), "local request diagnostics");
                Ok(Has(eventDiagnostics.Ms, "commands 10") && Has(eventDiagnostics.Ms, "alert Safe click ran"), "command message diagnostics");
                Ok(Has(eventDiagnostics.Vs, "last = link"), "variable diagnostics");
                eventEngine.Dcl();
                Eq(0, eventEngine.Dg(eventPage).Rs.Count, "clear request diagnostics");
                eventEngine.Je = false;
                Eq(0, eventEngine.Ck(eventPage, ById(eventPage.D, "go")), "click commands respect script setting");
                eventEngine.Je = true;

                var goPath = Path.Combine(pages, "go-command.html");
                var goTargetPath = Path.Combine(pages, "go-target.html");
                var goTarget = new Uri(goTargetPath);
                var goEngine = new E();
                var goPage = goEngine.Go(goPath);
                Eq(1, goPage.Jc, "go fixture variable command");
                Eq(1, goEngine.Ck(goPage, ById(goPage.D, "relative")), "relative go command accepted");
                Eq(goTarget, goEngine.Gt(goPage), "relative go request resolves against document base");
                Eq<Uri>(null, goEngine.Gt(goPage), "go request is consumed once");
                Eq(1, goEngine.Ck(goPage, ById(goPage.D, "double")), "one pending go request per command run");
                Eq(goTarget, goEngine.Gt(goPage), "first go request wins");
                Eq(0, goEngine.Ck(goPage, ById(goPage.D, "unsafe")), "unsafe go schemes rejected");
                Eq<Uri>(null, goEngine.Gt(goPage), "rejected go commands queue nothing");
                Eq(1, goEngine.Js(goPage, "go('" + goTarget.AbsoluteUri + "')"), "absolute file go accepted from file page");
                Eq(goTarget, goEngine.Gt(goPage), "absolute file go request");
                Eq(1, goEngine.Js(goPage, "go('https://example.com/lea')"), "absolute HTTPS go accepted");
                Eq(new Uri("https://example.com/lea"), goEngine.Gt(goPage), "absolute HTTPS go request");
                Eq(0, goEngine.Js(goPage, "go(''); go('about:blank'); go('//remote/share')"), "empty, unsupported, and UNC go targets rejected");
                goEngine.Je = false;
                Eq(0, goEngine.Ck(goPage, ById(goPage.D, "relative")), "go command respects script setting");
                goEngine.Je = true;

                var classPath = Path.Combine(pages, "class-commands.html");
                var classEngine = new E();
                var classPage = classEngine.Go(classPath);
                Eq(7, classPage.Jc, "page class and style command count");
                Eq("panel hot", ById(classPage.D, "panel").Av("class"), "add class alias");
                Eq("keep", ById(classPage.D, "remove").Av("class"), "remove class alias");
                Eq("panel hot", ById(classPage.D, "toggle").Av("class"), "toggle class alias adds");
                Eq("#0f4c81", BlockById(classPage, "panel").S.F, "scoped foreground overrides class CSS");
                Eq("#fff4c2", BlockById(classPage, "toggle").S.Bg, "scoped background alias");
                Eq(21, BlockById(classPage, "remove").S.Fs, "scoped font-size alias");
                var classField = FieldById(classPage, "field");
                classField.V = "Edited class value";
                Eq(3, classEngine.Js(classPage, "rc('panel','hot'); ac('field','hot'); tc('toggle','hot');"), "host class commands");
                Eq("panel", ById(classPage.D, "panel").Av("class"), "host remove class");
                Eq("field hot", ById(classPage.D, "field").Av("class"), "host add field class");
                Eq("panel", ById(classPage.D, "toggle").Av("class"), "host toggle class removes");
                Eq("#0f4c81", BlockById(classPage, "panel").S.F, "scoped foreground survives class rebuild");
                Eq("#166534", FieldById(classPage, "field").S.F, "field class reapplies CSS");
                Eq(4, classEngine.Js(classPage, "fg('panel','#334155'); bg('toggle','#e0f2fe'); fs('remove',24); fg('field','#14532d');"), "host scoped style commands");
                Eq("#334155", BlockById(classPage, "panel").S.F, "host scoped foreground command");
                Eq("#e0f2fe", BlockById(classPage, "toggle").S.Bg, "host scoped background command");
                Eq(24, BlockById(classPage, "remove").S.Fs, "host scoped font-size command");
                Eq("#14532d", FieldById(classPage, "field").S.F, "field scoped style command");
                Eq("Edited class value", FieldById(classPage, "field").V, "class rebuild preserves field value");
                Eq(1, classEngine.Js(classPage, "ac('field','HOT');"), "duplicate add is accepted");
                Eq(2, ClassCount(ById(classPage.D, "field")), "duplicate add stays unique");
                Eq(1, classEngine.Js(classPage, "rc('remove','missing');"), "absent remove is accepted");
                Eq(0, classEngine.Js(classPage, "ac('missing','hot'); ac('unsafe-style','hot'); ac('panel','two words'); ac('panel','" + new String('x', 65) + "');"), "invalid class commands ignored");
                Eq(0, classEngine.Js(classPage, "bg('missing','#fff'); fg('unsafe-style','#fff'); bg('panel','red;display:none'); fs('panel',7); fs('panel',97); fs('panel','12em'); fg('panel','#zzzzzz');"), "unsafe scoped style commands ignored");

                var classLimitCode = new StringBuilder();
                for (var classIndex = 0; classIndex < 32; classIndex++) classLimitCode.Append("ac('limit','c").Append(classIndex).Append("');");
                Eq(32, classEngine.Js(classPage, classLimitCode.ToString()), "class count command limit");
                Eq(32, ClassCount(ById(classPage.D, "limit")), "class count storage limit");
                Eq(0, classEngine.Js(classPage, "ac('limit','c32');"), "class beyond storage limit rejected");
                for (var classIndex = 0; classIndex < 7; classIndex++)
                    Eq(1, classEngine.Js(classPage, "ac('length','" + new String((char)('a' + classIndex), 64) + "');"), "bounded long class " + classIndex);
                Eq(0, classEngine.Js(classPage, "ac('length','" + new String('z', 64) + "');"), "class attribute length limit");

                var clickClassEngine = new E();
                var clickClassPage = clickClassEngine.Go(classPath);
                FieldById(clickClassPage, "field").V = "Click-preserved value";
                Eq(4, clickClassEngine.Ck(clickClassPage, ById(clickClassPage.D, "flip")), "click class and style commands");
                Eq("panel", ById(clickClassPage.D, "panel").Av("class"), "click toggle removes class");
                Eq("field hot", ById(clickClassPage.D, "field").Av("class"), "click toggle adds class");
                Eq("#dbeafe", BlockById(clickClassPage, "panel").S.Bg, "click scoped background command");
                Eq("Click-preserved value", FieldById(clickClassPage, "field").V, "click class rebuild preserves field value");
                Eq("Class state changed", clickClassPage.Ms[0], "click class alert");

                var statePath = Path.Combine(pages, "state-commands.html");
                var stateEngine = new E();
                var statePage = stateEngine.Go(statePath);
                Eq(9, statePage.Jc, "initial state command count");
                var stateName = FieldById(statePage, "name");
                Eq("Page ready", stateName.V, "initial bounded field value command");
                Eq("two", FieldById(statePage, "pick").V, "initial select value command");
                Eq(1, FieldById(statePage, "pick").Si, "initial select index command");
                Eq("#118866", FieldById(statePage, "color").V, "initial color value command");
                Ok(FieldById(statePage, "agree").C, "initial checked command");
                Ok(!FieldById(statePage, "choice-a").C && FieldById(statePage, "choice-b").C, "radio command keeps one group choice");
                Ok(FieldById(statePage, "locked").Ds, "initial disable command");
                Ok(stateEngine.Ax(statePage, ById(statePage.D, "panel")) == null, "hide command removes rendered semantics");
                Ok(Object.ReferenceEquals(ById(statePage.D, "name"), stateEngine.Kp(statePage)), "pending focus command target");
                Eq<N>(null, stateEngine.Kp(statePage), "pending focus command is consumed once");
                var advancedDetails = ById(statePage.D, "advanced");
                var advancedValue = FieldById(statePage, "advanced-value");
                Ok(advancedDetails.Op && advancedDetails.A.ContainsKey("open"), "initial disclosure open command");
                advancedValue.V = "Retained detail command value";
                Eq(1, stateEngine.Js(statePage, "cl('advanced-summary')"), "close disclosure by first summary command");
                Ok(!advancedDetails.Op && stateEngine.Ax(statePage, ById(statePage.D, "advanced-value")) == null, "closed command hides disclosure semantics");
                Eq(1, stateEngine.Js(statePage, "op('advanced')"), "open disclosure by details command");
                Ok(advancedDetails.Op && Object.ReferenceEquals(advancedValue, FieldById(statePage, "advanced-value")) && advancedValue.V == "Retained detail command value", "open command retains disclosure field state");
                Eq(1, stateEngine.Js(statePage, "tg('advanced-summary')"), "toggle disclosure command");
                Ok(!advancedDetails.Op, "toggle command closes open disclosure");
                Eq(0, stateEngine.Js(statePage, "op('panel'); cl('missing'); tg('advanced-extra')"), "invalid disclosure command targets rejected");
                Eq(1, stateEngine.Js(statePage, "op('advanced')"), "open disclosure no-op state restore");

                Eq(7, stateEngine.Ck(statePage, ById(statePage.D, "apply")), "state click command count");
                Eq("Clicked Lea", stateName.V, "click field value command");
                Ok(!FieldById(statePage, "locked").Ds, "enable command");
                Ok(!FieldById(statePage, "agree").C, "checked toggle command");
                Ok(FieldById(statePage, "choice-a").C && !FieldById(statePage, "choice-b").C, "radio set command");
                Ok(stateEngine.Ax(statePage, ById(statePage.D, "panel")) != null, "show command restores rendered semantics");
                Ok(Object.ReferenceEquals(ById(statePage.D, "name"), stateEngine.Kp(statePage)), "click focus request");
                Eq("State commands ran", statePage.Ms[0], "state command alert");

                Eq(3, stateEngine.Ck(statePage, ById(statePage.D, "hide-field")), "hide retains field command count");
                Eq("Changed while hidden", stateName.V, "hidden field accepts bounded value command");
                Ok(stateEngine.Ax(statePage, ById(statePage.D, "name")) == null, "hidden field leaves semantic model");
                Eq(1, stateEngine.Ck(statePage, ById(statePage.D, "show-field")), "show retained field command");
                Ok(Object.ReferenceEquals(stateName, FieldById(statePage, "name")), "hidden field model identity retained");
                Eq("Changed while hidden", FieldById(statePage, "name").V, "shown field retains command value");
                Eq(2, stateEngine.Js(statePage, "ck('choice-b','1'); hd('choice-b')"), "hide selected radio command");
                Eq(1, stateEngine.Js(statePage, "ck('choice-a','1')"), "select against hidden radio peer");
                Eq(1, stateEngine.Js(statePage, "sh('choice-b')"), "show cleared radio peer");
                Ok(FieldById(statePage, "choice-a").C && !FieldById(statePage, "choice-b").C, "hidden retained radio group stays exclusive");

                Eq(1, stateEngine.Js(statePage, "vl('secret','command-secret')"), "password value command accepted");
                Eq("command-secret", FieldById(statePage, "secret").V, "password live value command");
                Eq("", stateEngine.Ax(statePage, ById(statePage.D, "secret")).V, "password command remains semantically redacted");
                Eq(0, stateEngine.Js(statePage, "vl('upload','C:\\\\private.txt'); vl('color','red'); vl('pick','blocked'); ck('name','1'); ds('panel'); fo('panel'); op('name')"), "unsafe state targets and values rejected");
                Eq(0, FieldById(statePage, "upload").Fp.Count, "file command cannot select local paths");
                var longStateValue = new String('q', 1024);
                Eq(1, stateEngine.Js(statePage, "vl('name','" + longStateValue + "')"), "bounded 1024-character field command");
                Eq(1024, FieldById(statePage, "name").V.Length, "bounded field command storage");
                Eq(0, stateEngine.Js(statePage, "vl('name','" + longStateValue + "q')"), "oversized field command rejected");
                Eq(1024, FieldById(statePage, "name").V.Length, "oversized field command leaves state");
                Eq(1, stateEngine.Js(statePage, "fo('name'); fo('locked')"), "first pending focus command wins");
                Eq("name", stateEngine.Kp(statePage).Av("id"), "first pending focus target retained");

                var variableEngine = new E();
                var variablePage = variableEngine.Go(forms);
                var variableCode = new StringBuilder();
                for (var variableIndex = 0; variableIndex < 17; variableIndex++)
                    variableCode.Append("v('v").Append(variableIndex).Append("','").Append(variableIndex).Append("');");
                Eq(16, variableEngine.Js(variablePage, variableCode.ToString()), "page variable limit");
                Eq(16, variablePage.Vs.Count, "bounded variable storage");
                Eq(0, variableEngine.Js(variablePage, "v('1bad','value'); tx('missing','value'); tx('body id','value');"), "invalid variables and text targets ignored");
                variablePage.Fs[1].V = "private-password-value";
                Ok(!Has(variableEngine.Dg(variablePage).Ns, "private-password-value"), "diagnostics omit field values");

                var findPath = Path.Combine(pages, "find-download.html");
                var findPage = engine.Go(findPath);
                Eq(3, engine.Fd(findPage, "cobalt"), "engine page-text find count");
                Eq(3, engine.Fd(findPage, "COBALT"), "engine find is case insensitive");
                Eq(0, engine.Fd(findPage, "missing phrase"), "engine missing find count");

                var downloadSource = Path.Combine(pages, "download-sample.txt");
                var downloadSync = Path.Combine(Path.GetTempPath(), "lea-download-" + Guid.NewGuid().ToString("N") + ".txt");
                var downloadAsync = Path.Combine(Path.GetTempPath(), "lea-download-" + Guid.NewGuid().ToString("N") + ".txt");
                try
                {
                    Eq(64L * 1024 * 1024, engine.Dm, "bounded download maximum");
                    File.WriteAllText(downloadSync, "replace this download");
                    var saved = engine.Dl(downloadSource, downloadSync);
                    Eq(200, saved.St, "local download status");
                    Eq("text/plain", saved.Ct, "local download content type");
                    Eq(new FileInfo(downloadSource).Length, saved.Sz, "local download size");
                    Eq(Path.GetFileName(downloadSync), saved.N, "local download name");
                    Eq(File.ReadAllText(downloadSource), File.ReadAllText(downloadSync), "streamed local download atomically replaces destination");
                    var savedAsync = engine.DlA(new Uri(downloadSource), downloadAsync).GetAwaiter().GetResult();
                    Eq(new FileInfo(downloadSource).Length, savedAsync.Sz, "async local download size");
                    Eq(File.ReadAllText(downloadSource), File.ReadAllText(downloadAsync), "async local download bytes");
                }
                finally
                {
                    File.Delete(downloadSync);
                    File.Delete(downloadAsync);
                }

                var inlinePath = Path.Combine(pages, "inline.html");
                var inlinePage = engine.Go(inlinePath);
                Eq(3, inlinePage.Bs.Count, "inline page avoids duplicate link block");
                Eq(1, inlinePage.Ls.Count, "inline link count");
                Eq(2, inlinePage.Bs[0].Is.Count, "heading inline runs");
                Ok(inlinePage.Bs[0].Is[1].S.B, "heading emphasis crosses neutral span");
                var inline = inlinePage.Bs[1];
                Eq("A bold and italic line with one linked phrase and color. Next line.", inline.X, "inline compatibility text");
                Eq(11, inline.Is.Count, "compact inline run count");
                Eq("#f1f5f9", inline.S.Bg, "inline block background");
                Eq("", inline.Is[0].S.Bg, "block box style does not leak into text runs");
                Ok(inline.Is[1].S.B, "strong run is bold");
                Ok(inline.Is[2].S.B && inline.Is[2].S.I, "nested emphasis inherits bold and italic");
                Ok(inline.Is[4].U != null && inline.Is[5].U != null && inline.Is[5].S.B, "nested link runs retain target and emphasis");
                Eq("#a13b18", inline.Is[7].S.F, "inline class color");
                Eq("br", inline.Is[9].K, "inline line break");
                Eq(0, inlinePage.Bs[2].Is.Count, "plain text fast path");

                var textPath = Path.Combine(pages, "text-layout.html");
                var textPage = engine.Go(textPath);
                Eq(5, textPage.Bs.Count, "text layout blocks");
                Eq("c", textPage.Bs[1].S.Ta, "center alignment alias");
                Eq(28, textPage.Bs[1].S.Lh, "pixel line-height alias");
                Eq("r", textPage.Bs[2].S.Ta, "right alignment alias");
                Eq(175, textPage.Bs[2].S.Lm, "percentage line-height alias");
                Eq("j", textPage.Bs[3].S.Ta, "justify alignment alias");
                Eq(0, textPage.Bs[3].Is.Count, "styled plain text keeps compact page model");
                var textCell = textPage.Bs[4].Tb.Rs[0].Cs[0];
                Eq("r", textCell.S.Ta, "table cell alignment inheritance");
                Eq(160, textCell.S.Lm, "unitless table-cell line height");

                var whitePath = Path.Combine(pages, "white-space.html");
                var whitePage = engine.Go(whitePath);
                Eq(10, whitePage.Bs.Count, "white-space block count");
                Eq("nw", whitePage.Bs[1].S.Ws, "nowrap alias");
                Eq("h", whitePage.Bs[1].S.Ov, "hidden overflow alias");
                Eq("v", whitePage.Bs[2].S.Ov, "visible overflow alias");
                Eq("p", whitePage.Bs[3].S.Ws, "pre alias");
                Eq("c", whitePage.Bs[3].S.Ov, "clip overflow alias");
                Eq("Alpha    Beta\nGamma", whitePage.Bs[3].X, "pre text preservation");
                Eq("pw", whitePage.Bs[4].S.Ws, "pre-wrap alias");
                Eq("pl", whitePage.Bs[5].S.Ws, "pre-line alias");
                Eq("a", whitePage.Bs[6].S.Ov, "auto overflow alias");
                Eq("pl", whitePage.Bs[7].S.Ws, "inherited white-space alias");
                Eq("s", whitePage.Bs[9].S.Ov, "scroll overflow alias");
                var whiteCell = whitePage.Bs[8].Tb.Rs[0].Cs[0];
                Eq("pw", whiteCell.S.Ws, "table pre-wrap alias");
                Eq("h", whiteCell.S.Ov, "table overflow alias");
                Eq("Cell    keeps spaces\nand wraps compact table text across more lines.", whiteCell.X, "table white-space preservation");

                var sizingPath = Path.Combine(pages, "sizing-breaks.html");
                var sizingPage = engine.Go(sizingPath);
                Eq(10, sizingPage.Bs.Count, "sizing and word-break block count");
                Eq(1, sizingPage.Fs.Count, "sizing field count");
                Eq(120, sizingPage.Bs[1].S.W, "declared small width");
                Eq(260, sizingPage.Bs[1].S.Wn, "minimum width alias");
                Eq(520, sizingPage.Bs[2].S.W, "declared large width");
                Eq(280, sizingPage.Bs[2].S.Wx, "maximum width alias");
                Eq(20, sizingPage.Bs[3].S.H, "declared small height");
                Eq(58, sizingPage.Bs[3].S.Hn, "minimum height alias");
                Eq(120, sizingPage.Bs[4].S.H, "declared large height");
                Eq(42, sizingPage.Bs[4].S.Hx, "maximum height alias");
                Eq("n", sizingPage.Bs[5].S.Wb, "normal overflow-wrap alias");
                Eq("b", sizingPage.Bs[6].S.Wb, "legacy word-wrap break alias");
                Eq("a", sizingPage.Bs[7].S.Wb, "word-break all alias");
                Eq(260, sizingPage.Fs[0].S.Wx, "field maximum width");
                Eq(40, sizingPage.Fs[0].S.Hn, "field minimum height");
                Eq(260, sizingPage.Bs[9].S.Wx, "table maximum width");
                Eq("a", sizingPage.Bs[9].Tb.Rs[0].Cs[0].S.Wb, "table-cell anywhere wrap alias");

                var visibilityPath = Path.Combine(pages, "visibility-opacity.html");
                var visibilityPage = engine.Go(visibilityPath);
                Eq(16, visibilityPage.Bs.Count, "visibility and opacity block count");
                Eq(2, visibilityPage.Fs.Count, "visibility field count");
                Eq(2, visibilityPage.Ls.Count, "visibility link count");
                Eq(100, visibilityPage.Bs[1].S.Op, "clamped opacity percentage");
                Eq(50, visibilityPage.Bs[2].S.Op, "percentage opacity alias");
                Ok(visibilityPage.Bs[3].S.Zc && Object.ReferenceEquals(visibilityPage.Bs[4].Up, visibilityPage.Bs[3]), "opacity stacking parent retained");
                Eq(25, visibilityPage.Bs[4].S.Op, "nested effective opacity");
                Eq(80, visibilityPage.Bs[5].S.Op, "opacity cascade override");
                Eq("h", visibilityPage.Bs[6].S.Vi, "hidden visibility alias");
                Eq("c", visibilityPage.Bs[7].S.Vi, "collapse visibility alias");
                Eq("v", visibilityPage.Bs[8].S.Vi, "visible child override");
                Eq(0, visibilityPage.Bs[9].S.Op, "negative opacity clamp");
                Eq("h", visibilityPage.Bs[10].S.Vi, "hidden link visibility");
                Eq(50, visibilityPage.Fs[0].S.Op, "field opacity");
                Eq("h", visibilityPage.Fs[1].S.Vi, "hidden field visibility");
                Eq(100, visibilityPage.Bs[13].S.Op, "opaque image state");
                Eq(50, visibilityPage.Bs[14].S.Op, "faded image state");
                Eq(50, visibilityPage.Bs[15].S.Op, "table effective opacity");
                Eq(25, visibilityPage.Bs[15].Tb.Rs[0].Cs[0].S.Op, "table-cell effective opacity");

                var dynamicPath = Path.Combine(pages, "dynamic-state.html");
                var dynamicPage = engine.Go(dynamicPath);
                Eq(5, dynamicPage.Bs.Count, "dynamic state block count");
                Eq(2, dynamicPage.Fs.Count, "dynamic state field count");
                Eq(6, dynamicPage.Bs[1].S.Pl, "non-hover card padding");
                Eq("Consolas|m", dynamicPage.Bs[1].S.Ff, "negated hover baseline");
                Eq("Consolas|m", dynamicPage.Fs[0].S.Ff, "negated focus baseline");
                var cardNode = ById(dynamicPage.D, "card");
                var nestedNode = ById(dynamicPage.D, "nested");
                var fieldNode = ById(dynamicPage.D, "field");
                var linkNode = ById(dynamicPage.D, "state-link");
                var cellNode = ById(dynamicPage.D, "cell");
                Ok(cardNode != null && nestedNode != null && fieldNode != null && linkNode != null && cellNode != null, "dynamic fixture nodes");
                var savedField = dynamicPage.Fs[0];
                savedField.V = "Edited value";
                engine.Dy(dynamicPage, nestedNode, fieldNode);
                Eq(18, dynamicPage.Bs[1].S.Pl, "ancestor hover card padding");
                Eq("#fff4d6", dynamicPage.Bs[1].S.Bg, "ancestor hover background");
                Ok(Run(dynamicPage.Bs[1], "strong").S.I, "hover descendant selector");
                Eq("Georgia|s", dynamicPage.Fs[0].S.Ff, "focus font");
                Eq("#176b4d", dynamicPage.Fs[0].S.Bg, "focus background");
                Eq(16, dynamicPage.Fs[0].S.Pl, "focus layout style");
                Ok(Object.ReferenceEquals(savedField, dynamicPage.Fs[0]) && dynamicPage.Fs[0].V == "Edited value", "dynamic restyle preserves field state");
                engine.Dy(dynamicPage, linkNode, null);
                var dynamicLink = Run(dynamicPage.Bs[1], "a");
                Eq("#7e22ce", dynamicLink.S.F, "inline link hover color");
                Ok(dynamicLink.S.B && dynamicLink.S.Td == "n", "inline link hover text style");
                engine.Dy(dynamicPage, cellNode, null);
                Eq(20, dynamicPage.Bs[4].Tb.Rs[0].Cs[0].S.Pl, "table cell hover padding");
                engine.Dy(dynamicPage, null, null);
                Eq(6, dynamicPage.Bs[1].S.Pl, "hover reset padding");
                Eq("Consolas|m", dynamicPage.Fs[0].S.Ff, "focus reset font");
                Ok(Object.ReferenceEquals(savedField, dynamicPage.Fs[0]) && dynamicPage.Fs[0].V == "Edited value", "dynamic reset preserves field state");

                var mediaPath = Path.Combine(pages, "responsive-media.html");
                var mediaPage = engine.Go(mediaPath);
                Eq(11, mediaPage.Bs.Count, "responsive media block count");
                Eq("h1,p,img,p,img,p,img,p,img,p,img", BlockKinds(mediaPage), "responsive media block kinds");
                Eq(96, mediaPage.Bs[2].S.W, "HTML image width");
                Eq(54, mediaPage.Bs[2].S.H, "HTML image height");
                Eq("data", mediaPage.Bs[2].U.Scheme, "embedded image scheme");
                Ok(mediaPage.Bs[4].U.OriginalString.Contains("Y6hTOvc"), "one-times density candidate");
                Ok(mediaPage.Bs[6].U.OriginalString.Contains("Y5D1u/E"), "picture screen source candidate");
                Eq("lea.ico", Path.GetFileName(mediaPage.Bs[8].U.LocalPath), "smallest adequate width candidate");
                Eq(130, mediaPage.Bs[10].S.W, "CSS image width overrides HTML");
                Eq(44, mediaPage.Bs[10].S.H, "CSS image height overrides HTML");

                var mediaQueryPath = Path.Combine(pages, "media-queries.html");
                var mediaQueryEngine = new E();
                Eq(1024, mediaQueryEngine.Vw, "default viewport width alias");
                var mediaQueryPage = mediaQueryEngine.Go(mediaQueryPath);
                Eq(1024, mediaQueryPage.Vw, "page viewport width alias");
                Ok(HasBlock(mediaQueryPage, "wide") && HasBlock(mediaQueryPage, "either") && HasBlock(mediaQueryPage, "style-media"), "wide media rules active");
                Ok(!HasBlock(mediaQueryPage, "narrow") && !HasBlock(mediaQueryPage, "nested") && !HasBlock(mediaQueryPage, "exact"), "narrow media rules inactive");
                Ok(!HasBlock(mediaQueryPage, "print-only") && !HasBlock(mediaQueryPage, "unsupported"), "unsupported media rules inactive");
                Eq(320, BlockById(mediaQueryPage, "wide").S.W, "minimum-width style");
                var responsiveField = FieldById(mediaQueryPage, "field");
                Eq(260, responsiveField.S.W, "wide field width");
                responsiveField.V = "Retained responsive value";
                var wideDiagnostics = mediaQueryEngine.Dg(mediaQueryPage);
                Eq(1024, wideDiagnostics.Vw, "diagnostic viewport width alias");
                Eq(16, wideDiagnostics.C, "wide active CSS rule count");
                Ok(Has(wideDiagnostics.Cs, "@m>=701") && Has(wideDiagnostics.Cs, "@m>=900") && Has(wideDiagnostics.Cs, "@m>=700"), "wide media diagnostics");

                mediaQueryEngine.Vw = 480;
                Eq(480, mediaQueryPage.Vw, "responsive page viewport update");
                Ok(HasBlock(mediaQueryPage, "narrow") && HasBlock(mediaQueryPage, "nested") && HasBlock(mediaQueryPage, "exact"), "narrow and exact media rules active");
                Ok(!HasBlock(mediaQueryPage, "wide") && !HasBlock(mediaQueryPage, "either") && !HasBlock(mediaQueryPage, "style-media"), "wide media rules deactivate");
                Eq(180, BlockById(mediaQueryPage, "narrow").S.W, "maximum-width style");
                Eq(172, BlockById(mediaQueryPage, "nested").S.W, "nested media intersection");
                Eq(148, BlockById(mediaQueryPage, "exact").S.W, "exact-width style");
                Ok(Object.ReferenceEquals(responsiveField, FieldById(mediaQueryPage, "field")) && responsiveField.V == "Retained responsive value", "media rebuild preserves field state");
                Eq(140, responsiveField.S.W, "narrow field width");
                var narrowDiagnostics = mediaQueryEngine.Dg(mediaQueryPage);
                Eq(16, narrowDiagnostics.C, "narrow active CSS rule count");
                Ok(Has(narrowDiagnostics.Cs, "@m<=600") && Has(narrowDiagnostics.Cs, "@m=480") && Has(narrowDiagnostics.Cs, "@m400-700"), "narrow media diagnostics");

                mediaQueryEngine.Vw = 350;
                Ok(HasBlock(mediaQueryPage, "narrow") && HasBlock(mediaQueryPage, "either") && !HasBlock(mediaQueryPage, "nested") && !HasBlock(mediaQueryPage, "exact"), "comma media alternative and nested lower boundary");
                Eq(15, mediaQueryEngine.Dg(mediaQueryPage).C, "small active CSS rule count");
                mediaQueryEngine.Vw = 700;
                Ok(!HasBlock(mediaQueryPage, "wide") && HasBlock(mediaQueryPage, "nested") && HasBlock(mediaQueryPage, "style-media"), "inclusive 700 pixel boundaries");
                Eq(14, mediaQueryEngine.Dg(mediaQueryPage).C, "boundary active CSS rule count");
                mediaQueryEngine.Vw = 800;
                Ok(HasBlock(mediaQueryPage, "wide") && !HasBlock(mediaQueryPage, "nested") && !HasBlock(mediaQueryPage, "either") && HasBlock(mediaQueryPage, "style-media"), "between-alternative media state");
                Eq(15, mediaQueryEngine.Dg(mediaQueryPage).C, "middle active CSS rule count");
                mediaQueryEngine.Vw = 0;
                Eq(1, mediaQueryEngine.Vw, "minimum viewport clamp");
                mediaQueryEngine.Vw = Int32.MaxValue;
                Eq(8192, mediaQueryEngine.Vw, "maximum viewport clamp");

                var customPath = Path.Combine(pages, "custom-properties.html");
                var customEngine = new E();
                var customPage = customEngine.Go(customPath);
                Eq(19, customPage.S.Vc, "inherited custom property count alias");
                Eq("#176b4d", customPage.S.Cv("--brand"), "custom property lookup alias");
                Eq("#123f8c", customPage.S.Cv("--Brand"), "case-sensitive custom property lookup");
                Eq("", customPage.S.Cv("--BRAND"), "custom property case mismatch");
                Eq(19, customEngine.Dg(customPage).Vc, "diagnostic custom property count alias");
                var customBase = BlockById(customPage, "base");
                Eq("#176b4d", customBase.S.F, "nested custom property color");
                Ok(customBase.S.W == 260 && customBase.S.Pt == 8 && customBase.S.Mt == 8 && customBase.S.Mr == 8, "custom property dimensions and shorthand");
                var customChild = BlockById(customPage, "child");
                Ok(customChild.S.F == "#7e22ce" && customChild.S.Pl == 12 && customChild.S.W == 260 && customChild.S.Cv("--brand") == "#7e22ce", "scoped inherited custom properties");
                Eq("#7e22ce", BlockById(customPage, "inherit").S.F, "explicit custom property inheritance");
                Ok(BlockById(customPage, "initial").S.F == "#334155" && BlockById(customPage, "initial").S.Cv("--brand") == "", "custom property initial removal");
                Eq(177, BlockById(customPage, "depth-ok").S.W, "custom property depth ceiling success");
                Eq(43, BlockById(customPage, "depth-limit").S.W, "custom property depth fallback");
                Ok(BlockById(customPage, "late").S.F == "#a13b18" && BlockById(customPage, "late").S.Bg == "#edf7f2", "late custom declaration and nested fallback");
                var customFallback = BlockById(customPage, "fallback");
                Ok(customFallback.S.F == "#a13b18" && customFallback.S.W == 180 && customFallback.S.Bt == 2 && customFallback.S.Bc == "#0f4c81" && customFallback.S.Ff == "Consolas|m", "custom fallback shorthands and comma value");
                Eq("#0f4c81", BlockById(customPage, "cycle").S.F, "custom property cycle fallback");
                Ok(BlockById(customPage, "case").S.F == "#123f8c" && BlockById(customPage, "case").S.Bg == "#176b4d", "case-sensitive custom property values");
                Ok(BlockById(customPage, "invalid").S.F == "#334155" && BlockById(customPage, "invalid").S.W == 130, "unresolved variable declaration rejection");
                Ok(BlockById(customPage, "inline").S.F == "#0f4c81" && BlockById(customPage, "inline").S.Pl == 10, "inline custom property and fallback");
                var customField = FieldById(customPage, "var-field");
                customField.V = "Retained custom value";
                Eq(260, customField.S.W, "custom property field width");
                customEngine.Dy(customPage, ById(customPage.D, "hover"), null);
                Ok(BlockById(customPage, "hover").S.F == "#a13b18" && BlockById(customPage, "hover").S.Pl == 18, "dynamic custom property restyle");
                Ok(Object.ReferenceEquals(customField, FieldById(customPage, "var-field")) && customField.V == "Retained custom value", "custom hover rebuild preserves field state");
                customEngine.Vw = 480;
                Ok(BlockById(customPage, "base").S.W == 150 && BlockById(customPage, "base").S.Pl == 6 && FieldById(customPage, "var-field").S.W == 150, "responsive custom property values");
                Ok(Object.ReferenceEquals(customField, FieldById(customPage, "var-field")) && customField.V == "Retained custom value", "custom media rebuild preserves field state");

                var customLimitPath = Path.Combine(Path.GetTempPath(), "lea-custom-limit-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var customHtml = new StringBuilder("<!doctype html><html><head><style>:root{--caf\u00e9:99px;");
                    for (var customIndex = 0; customIndex < 40; customIndex++) customHtml.Append("--v").Append(customIndex).Append(':').Append(100 + customIndex).Append("px;");
                    customHtml.Append("--oversized:").Append(new String('x', 520)).Append(";}#cap{width:var(--v31,1px);height:var(--v32,41px);padding:var(--oversized,7px)}</style></head><body><p id='cap'>Cap</p></body></html>");
                    File.WriteAllText(customLimitPath, customHtml.ToString(), new UTF8Encoding(false));
                    var customLimitPage = new E().Go(customLimitPath);
                    var customCap = BlockById(customLimitPage, "cap");
                    Eq(32, customLimitPage.S.Vc, "custom property count ceiling");
                    Ok(customCap.S.W == 131 && customCap.S.H == 41 && customCap.S.Pl == 7, "custom property count and value limit fallbacks");
                    Eq("", customLimitPage.S.Cv("--caf\u00e9"), "non-ASCII custom property omitted");
                    Eq("", customLimitPage.S.Cv("--v32"), "excess custom property omitted");
                }
                finally
                {
                    if (File.Exists(customLimitPath)) File.Delete(customLimitPath);
                }

                var importantPath = Path.Combine(pages, "important-cascade.html");
                var importantEngine = new E();
                var importantPage = importantEngine.Go(importantPath);
                Eq(19, importantEngine.Dg(importantPage).Ic, "important declaration diagnostic alias");
                var sheetPriority = BlockById(importantPage, "sheet");
                Ok(sheetPriority.S.F == "#a13b18" && sheetPriority.S.W == 140 && sheetPriority.S.Bg == "#7e22ce" && sheetPriority.S.Cv("--brand") == "#7e22ce", "stylesheet important overrides inline normal declarations");
                var inlinePriority = BlockById(importantPage, "inline");
                Ok(inlinePriority.S.F == "#0f4c81" && inlinePriority.S.W == 190 && inlinePriority.S.Bg == "#176b4d" && inlinePriority.S.Cv("--brand") == "#176b4d", "inline important overrides stylesheet important declarations");
                var specificPriority = BlockById(importantPage, "specific");
                Ok(specificPriority.S.F == "#0f4c81" && specificPriority.S.Pt == 9 && specificPriority.S.Pr == 9 && specificPriority.S.Pb == 9 && specificPriority.S.Pl == 9, "important specificity and source order");
                var orderedPriority = BlockById(importantPage, "order");
                Ok(orderedPriority.S.F == "#0f4c81" && orderedPriority.S.W == 91, "later normal declarations cannot replace important values");
                var shorthandPriority = BlockById(importantPage, "shorthand");
                Ok(shorthandPriority.S.Mt == 8 && shorthandPriority.S.Mr == 8 && shorthandPriority.S.Mb == 8 && shorthandPriority.S.Ml == 8 && shorthandPriority.S.Pt == 5 && shorthandPriority.S.Pr == 5 && shorthandPriority.S.Pb == 5 && shorthandPriority.S.Pl == 19, "important shorthand and longhand priority");
                var customPriority = BlockById(importantPage, "custom");
                Ok(customPriority.S.F == "#176b4d" && customPriority.S.Pl == 7 && customPriority.S.Cv("--tone") == "#176b4d", "important custom property cascade");
                Ok(BlockById(importantPage, "custom-child").S.F == "#123f8c", "local normal custom property overrides inherited important value");
                Eq(133, BlockById(importantPage, "fallback").S.W, "important custom property fallback");
                var literalPriority = BlockById(importantPage, "literal");
                Ok(literalPriority.S.Cv("--label") == "\"text !important\"" && literalPriority.S.Cv("--wrapped") == "var(--missing, 17px !important)", "quoted and nested important text remains a value");
                var importantField = FieldById(importantPage, "field");
                importantField.V = "Retained important field";
                Ok(BlockById(importantPage, "hover").S.F == "#176b4d" && BlockById(importantPage, "hover").S.W == 130 && BlockById(importantPage, "media").S.W == 205 && importantField.S.W == 240, "initial important cascade state");
                importantEngine.Dy(importantPage, ById(importantPage.D, "hover"), null);
                Ok(BlockById(importantPage, "hover").S.F == "#a13b18" && BlockById(importantPage, "hover").S.W == 180, "hover important custom and ordinary declarations");
                importantEngine.Dy(importantPage, ById(importantPage.D, "field"), null);
                Ok(FieldById(importantPage, "field").S.W == 170 && Object.ReferenceEquals(importantField, FieldById(importantPage, "field")) && importantField.V == "Retained important field", "important hover rebuild preserves field state");
                importantEngine.Dy(importantPage, null, null);
                importantEngine.Vw = 480;
                Ok(importantEngine.Dg(importantPage).Ic == 22 && BlockById(importantPage, "media").S.F == "#7e22ce" && BlockById(importantPage, "media").S.W == 115 && FieldById(importantPage, "field").S.W == 150, "responsive important declarations and diagnostics");
                Ok(Object.ReferenceEquals(importantField, FieldById(importantPage, "field")) && importantField.V == "Retained important field", "important media rebuild preserves field state");
                importantEngine.Vw = 840;
                Ok(importantEngine.Dg(importantPage).Ic == 19 && BlockById(importantPage, "media").S.W == 205 && FieldById(importantPage, "field").S.W == 240, "wide important cascade restore");

                var mapPath = Path.Combine(pages, "image-maps.html");
                var mapEngine = new E();
                var mapPage = mapEngine.Go(mapPath);
                Eq(2, mapPage.Mps.Count, "bounded unique image map collection");
                var imageMap = mapPage.Mps[0];
                Eq("LeaMap", imageMap.N, "image map name alias");
                Eq(7, imageMap.As.Count, "image map area collection");
                Ok(imageMap.El != null && imageMap.El.K == "map", "image map element ownership");
                var deadArea = imageMap.As[0];
                Ok(deadArea.K == "r" && deadArea.U == null && deadArea.Cs.Count == 4, "dead rectangle area model");
                var rectArea = imageMap.As[1];
                Ok(rectArea.K == "r" && rectArea.Cs.Count == 4 && rectArea.Cs[0] == 0D && rectArea.Cs[1] == 0D && rectArea.Cs[2] == 100D && rectArea.Cs[3] == 80D, "rectangle alias, excess drop, and coordinate ordering");
                Eq("#rect-target", rectArea.U.Fragment, "rectangle area link");
                Eq("Rectangle target", rectArea.T, "area alt text alias");
                var circleArea = imageMap.As[2];
                Ok(circleArea.K == "c" && circleArea.Cs.Count == 3 && circleArea.Cs[2] == 35D, "circle alias and excess drop");
                var polyArea = imageMap.As[3];
                Ok(polyArea.K == "p" && polyArea.Cs.Count == 8, "polygon alias and odd coordinate drop");
                Ok(imageMap.As[4].Cs.Count == 0 && imageMap.As[4].U != null, "zero-radius circle keeps empty link model");
                Ok(imageMap.As[5].K == "r" && imageMap.As[5].Cs.Count == 0, "malformed rectangle creates no shape");
                Ok(imageMap.As[6].K == "d" && imageMap.As[6].Cs.Count == 0, "default area ignores coordinates");
                Eq("unused", mapPage.Mps[1].N, "unreferenced valid map model");
                var mappedImage = BlockById(mapPage, "mapped");
                Ok(Object.ReferenceEquals(mappedImage.Mp, imageMap), "percent-decoded image map ownership");
                Ok(mappedImage.Mw == 320 && mappedImage.Mh == 160, "authored image map coordinate canvas aliases");
                Ok(BlockById(mapPage, "unmapped").Mp == null, "non-hash usemap rejected");
                Eq(6, mapPage.Ls.Count, "mapped area links join page links once");
                Eq(1, mapEngine.Ck(mapPage, ById(mapPage.D, "style-map")), "safe image map class rebuild");
                Eq(240, BlockById(mapPage, "mapped").S.W, "image map style rebuild");
                Ok(Object.ReferenceEquals(BlockById(mapPage, "mapped").Mp, mapPage.Mps[0]), "rebuilt image map ownership");
                Eq(2, mapPage.Mps.Count, "image map rebuild does not duplicate maps");
                Eq(6, mapPage.Ls.Count, "image map rebuild does not duplicate links");

                var boundedMapPath = Path.Combine(Path.GetTempPath(), "lea-image-maps-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var html = new StringBuilder("<!doctype html><html><body><img width='320' height='160' src='missing.png' usemap='#m0'><map name='m0'>");
                    var coordinates = new StringBuilder("2000000,-2000000");
                    for (var coordinate = 2; coordinate < 140; coordinate++) coordinates.Append(',').Append(coordinate);
                    html.Append("<area shape='poly' coords='").Append(coordinates).Append("' href='").Append(new string('h', 4200)).Append("' alt='").Append(new string('a', 600)).Append("'>");
                    html.Append("<area shape='poly' coords='").Append(new string('1', 4200)).Append("'>");
                    for (var areaIndex = 2; areaIndex < 70; areaIndex++) html.Append("<area shape='default'>");
                    html.Append("</map>");
                    for (var mapIndex = 1; mapIndex < 35; mapIndex++) html.Append("<map name='m").Append(mapIndex).Append("'></map>");
                    html.Append("</body></html>");
                    File.WriteAllText(boundedMapPath, html.ToString(), new UTF8Encoding(false));
                    var boundedMapPage = new E().Go(boundedMapPath);
                    Eq(32, boundedMapPage.Mps.Count, "image map count cap");
                    Eq(64, boundedMapPage.Mps[0].As.Count, "area count cap");
                    Eq(128, boundedMapPage.Mps[0].As[0].Cs.Count, "polygon coordinate cap");
                    Ok(boundedMapPage.Mps[0].As[0].Cs[0] == 1000000D && boundedMapPage.Mps[0].As[0].Cs[1] == -1000000D, "finite map coordinate magnitude cap");
                    Ok(boundedMapPage.Mps[0].As[0].U == null && boundedMapPage.Mps[0].As[0].T.Length == 512, "bounded area href and alt metadata");
                    Eq(0, boundedMapPage.Mps[0].As[1].Cs.Count, "coordinate text length cap");
                }
                finally
                {
                    if (File.Exists(boundedMapPath)) File.Delete(boundedMapPath);
                }

                var keyboardPath = Path.Combine(pages, "keyboard-focus.html");
                var keyboardEngine = new E();
                var keyboardPage = keyboardEngine.Go(keyboardPath);
                Eq((int?)1, keyboardEngine.Ti(ById(keyboardPage.D, "priority-one")), "positive tabindex alias");
                Eq((int?)0, keyboardEngine.Ti(ById(keyboardPage.D, "zero-link")), "zero tabindex alias");
                Eq((int?)-1, keyboardEngine.Ti(ById(keyboardPage.D, "negative-link")), "negative tabindex alias");
                Eq((int?)null, keyboardEngine.Ti(ById(keyboardPage.D, "invalid-link")), "invalid tabindex stays omitted");
                Eq((int?)null, keyboardEngine.Ti(ById(keyboardPage.D, "default-first")), "omitted tabindex alias");
                var boundedTab = ById(keyboardPage.D, "priority-one");
                boundedTab.A["tabindex"] = "+999999999999999";
                Eq((int?)32767, keyboardEngine.Ti(boundedTab), "positive tabindex cap");
                boundedTab.A["tabindex"] = "-99999999999999";
                Eq((int?)-32767, keyboardEngine.Ti(boundedTab), "negative tabindex cap");
                boundedTab.A["tabindex"] = "12345678901234567";
                Eq((int?)null, keyboardEngine.Ti(boundedTab), "tabindex text length cap");

                var shortcutPath = Path.Combine(pages, "focus-shortcuts.html");
                var shortcutEngine = new E();
                var shortcutPage = shortcutEngine.Go(shortcutPath);
                Eq(1, shortcutPage.Ls.Count, "HTML-hidden shortcut links stay out of rendered link model");
                Eq(3, shortcutEngine.Afs(shortcutPage).Count, "bounded autofocus candidate aliases");
                Eq("invalid-auto", shortcutEngine.Af(shortcutPage).Av("id"), "first authored autofocus alias");
                var hiddenAuto = ById(shortcutPage.D, "hidden-auto");
                hiddenAuto.A["style"] = "display:block";
                shortcutEngine.Dy(shortcutPage, hiddenAuto, null);
                Eq(2, shortcutPage.Ls.Count, "author display CSS can override hidden presentation hint");
                var autoField = ById(shortcutPage.D, "auto-field");
                Eq("S", shortcutEngine.Ak(autoField), "first access-key alias");
                Eq(2, shortcutEngine.Aks(autoField).Count, "ordered access-key alternatives");
                Eq("1", shortcutEngine.Aks(autoField)[1], "numeric access-key alternative");
                var tokenButton = ById(shortcutPage.D, "token-button");
                Eq(1, shortcutEngine.Aks(tokenButton).Count, "invalid and duplicate access-key tokens are removed");
                Eq("Z", shortcutEngine.Ak(tokenButton), "valid access key follows invalid token");
                tokenButton.A["accesskey"] = "a b c d e f g h i j k l m n o p q";
                Eq(16, shortcutEngine.Aks(tokenButton).Count, "access-key token count cap");
                tokenButton.A["accesskey"] = new string('a', 257);
                Eq(0, shortcutEngine.Aks(tokenButton).Count, "access-key text length cap");
                var boundedAutofocusPath = Path.Combine(Path.GetTempPath(), "lea-autofocus-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var boundedAutofocus = new StringBuilder("<!doctype html><html><body>");
                    for (var autofocusIndex = 0; autofocusIndex < 40; autofocusIndex++)
                        boundedAutofocus.Append("<button id='a").Append(autofocusIndex).Append("' autofocus>focus</button>");
                    boundedAutofocus.Append("</body></html>");
                    File.WriteAllText(boundedAutofocusPath, boundedAutofocus.ToString(), new UTF8Encoding(false));
                    var boundedAutofocusEngine = new E();
                    var boundedAutofocusPage = boundedAutofocusEngine.Go(boundedAutofocusPath);
                    Eq(32, boundedAutofocusEngine.Afs(boundedAutofocusPage).Count, "autofocus candidate count cap");
                    Eq("a0", boundedAutofocusEngine.Af(boundedAutofocusPage).Av("id"), "bounded autofocus preserves document order");
                }
                finally
                {
                    if (File.Exists(boundedAutofocusPath)) File.Delete(boundedAutofocusPath);
                }

                var accessibilityPath = Path.Combine(pages, "accessibility.html");
                var accessibilityEngine = new E();
                var accessibilityPage = accessibilityEngine.Go(accessibilityPath);
                Ok(accessibilityPage.Axs.Count > 20 && accessibilityPage.Axs.Count <= 512, "bounded accessibility list");
                Ok(Object.ReferenceEquals(accessibilityPage.Axs, accessibilityEngine.Axs(accessibilityPage)), "accessibility list alias");
                Eq("document", accessibilityPage.Axs[0].K, "accessibility document role");
                Eq("Lea Accessibility", accessibilityPage.Axs[0].N, "accessibility document name");
                var headingAx = accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "main-title"));
                Eq("heading", headingAx.K, "native heading role");
                Eq(1, headingAx.Lv, "native heading level");
                Eq("Semantic access", headingAx.N, "heading content name");
                var userNode = ById(accessibilityPage.D, "user-name");
                var userAx = accessibilityEngine.Ax(accessibilityPage, userNode);
                Eq("textbox", userAx.K, "text field role");
                Eq("Display name", userAx.N, "explicit HTML label name");
                Eq("Shown beside your work.", userAx.D, "ARIA description references");
                Eq("Elliot", userAx.V, "live text field value");
                Ok(userAx.Is(Ax.Fs) && userAx.Is(Ax.Rq), "field focusable and required states");
                Eq("form", userAx.Up.K, "semantic parent ownership");
                accessibilityPage.Fs[0].V = "Updated name";
                Eq("Updated name", userAx.V, "semantic field value stays live without rebuild");
                var secretAx = accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "secret"));
                Eq("Password", secretAx.N, "wrapping label name");
                Eq("", secretAx.V, "password semantic value redaction");
                Ok(secretAx.Is(Ax.Ro), "readonly semantic state");
                var newsAx = accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "news"));
                Eq("checkbox", newsAx.K, "checkbox role");
                Ok(newsAx.Is(Ax.Ck), "live checked state");
                accessibilityPage.Fs[2].C = false;
                Ok(!newsAx.Is(Ax.Ck), "checked state updates without rebuild");
                var customAx = accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "custom"));
                Eq("checkbox", customAx.K, "first supported explicit role token");
                Eq("Compact mode", customAx.N, "labelledby precedes aria-label");
                Ok(customAx.Is(Ax.Mx) && customAx.Is(Ax.Fs), "mixed and focusable ARIA states");
                Eq("progressbar", accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "load-progress")).K, "progress semantic role");
                Eq("4", accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "load-progress")).V, "progress semantic value");
                Eq("rowheader", accessibilityEngine.Ax(accessibilityPage, accessibilityEngine.Q(accessibilityPage, "th[scope=row]")).K, "table row-header role");
                Ok(accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "presented-link")).Up.K != "presentation", "presentation role stays out of semantic list");
                Ok(accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "hidden-button")) == null, "display-none subtree excluded from semantics");
                Ok(accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "aria-hidden-button")) == null, "aria-hidden subtree excluded from semantics");
                var summaryNode = ById(accessibilityPage.D, "more-summary");
                var summaryAx = accessibilityEngine.Ax(accessibilityPage, summaryNode);
                Ok(summaryAx.Is(Ax.Co) && accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "more-copy")) == null, "closed disclosure semantic state and exclusion");
                Eq(1, accessibilityEngine.Ds(accessibilityPage, summaryNode), "semantic disclosure rebuild");
                Ok(accessibilityEngine.Ax(accessibilityPage, summaryNode).Is(Ax.Ex) && accessibilityEngine.Ax(accessibilityPage, ById(accessibilityPage.D, "more-copy")) != null, "expanded disclosure semantic state and content");
                accessibilityEngine.Dy(accessibilityPage, null, userNode);
                Ok(accessibilityEngine.Ax(accessibilityPage, userNode).Is(Ax.Fc), "focused semantic state follows dynamic rebuild");

                using (var accessibilityView = new V { Size = new Size(760, 560) })
                {
                    accessibilityView.CreateControl();
                    var accessibilityHandle = accessibilityView.Handle;
                    accessibilityView.Go(accessibilityPath);
                    Paint(accessibilityView);
                    Eq(accessibilityView.At.Axs.Count, accessibilityView.Axs.Count, "view accessibility list alias");
                    var viewUser = accessibilityView.Ax(accessibilityView.Id("user-name"));
                    Ok(viewUser != null && !accessibilityView.Ab(viewUser).IsEmpty, "view semantic bounds alias");
                    var accessibleRoot = accessibilityView.Ao;
                    Ok(accessibleRoot != null, "WinForms accessibility root provider");
                    Eq(AccessibleRole.Document, accessibleRoot.Role, "WinForms document accessibility role");
                    Ok(accessibleRoot.GetChildCount() > 0, "WinForms accessibility root children");
                    var accessibleUser = AccessibleByNameAndRole(accessibleRoot, "Display name", AccessibleRole.Text, 0);
                    Ok(accessibleUser != null, "WinForms labelled field accessibility child");
                    Eq(AccessibleRole.Text, accessibleUser.Role, "WinForms text accessibility role");
                    Eq("Elliot", accessibleUser.Value, "WinForms accessible field value");
                    Eq("", accessibleUser.DefaultAction, "WinForms text field has no false default action");
                    Ok((accessibleUser.State & AccessibleStates.Focusable) != 0 && !accessibleUser.Bounds.IsEmpty, "WinForms accessible focus and bounds");
                    accessibleUser.Select(AccessibleSelection.TakeFocus);
                    Eq("user-name", accessibilityView.Ke.Av("id"), "WinForms accessibility selection focuses Lea target");
                    Ok((accessibleUser.State & AccessibleStates.Focused) != 0 && !accessibleUser.Bounds.IsEmpty, "WinForms accessible object survives semantic rebuild");
                    var accessibleSecret = AccessibleByNameAndRole(accessibleRoot, "Password", AccessibleRole.Text, 0);
                    Eq("", accessibleSecret.Value, "WinForms password value redaction");
                    Ok((accessibleSecret.State & AccessibleStates.Protected) != 0, "WinForms password protected state");
                    var accessibleLink = AccessibleByNameAndRole(accessibleRoot, "Home", AccessibleRole.Link, 0);
                    Eq(AccessibleRole.Link, accessibleLink.Role, "WinForms link accessibility role");
                    Eq("Alt+Shift+H", accessibleLink.KeyboardShortcut, "WinForms assigned keyboard shortcut");
                    Eq("Open", accessibleLink.DefaultAction, "WinForms link default action");
                    var accessibleSummary = AccessibleByNameAndRole(accessibleRoot, "More details", AccessibleRole.PushButton, 0);
                    Ok((accessibleSummary.State & AccessibleStates.Collapsed) != 0, "WinForms collapsed disclosure state");
                }

                using (var automationHost = new Form { Size = new Size(800, 600), ShowInTaskbar = false })
                using (var automationView = new V { Dock = DockStyle.Fill })
                {
                    automationHost.Controls.Add(automationView);
                    automationHost.Show();
                    automationView.Go(accessibilityPath);
                    Paint(automationView);
                    Application.DoEvents();
                    var handle = automationView.Handle;
                    var data = new Dictionary<string, object>();
                    Exception automationError = null;
                    using (var completed = new ManualResetEvent(false))
                    {
                        var client = new Thread(new ThreadStart(delegate
                        {
                            try
                            {
                                var rootElement = AutomationElement.FromHandle(handle);
                                data["root-name"] = rootElement.Current.Name;
                                data["root-type"] = rootElement.Current.ControlType;
                                data["root-framework"] = rootElement.Current.FrameworkId;
                                data["count"] = rootElement.FindAll(TreeScope.Descendants, Condition.TrueCondition).Count;
                                var userElement = AutomationById(rootElement, "user-name");
                                data["user-type"] = userElement.Current.ControlType;
                                data["user-required"] = (bool)userElement.GetCurrentPropertyValue(AutomationElement.IsRequiredForFormProperty);
                                var userValue = (ValuePattern)userElement.GetCurrentPattern(ValuePattern.Pattern);
                                data["user-before"] = userValue.Current.Value;
                                userValue.SetValue("UIA name");
                                data["user-after"] = userValue.Current.Value;
                                var secretElement = AutomationById(rootElement, "secret");
                                data["secret-password"] = secretElement.Current.IsPassword;
                                object secretPattern;
                                data["secret-value"] = secretElement.TryGetCurrentPattern(ValuePattern.Pattern, out secretPattern);
                                var newsElement = AutomationById(rootElement, "news");
                                var newsToggle = (TogglePattern)newsElement.GetCurrentPattern(TogglePattern.Pattern);
                                data["news-before"] = newsToggle.Current.ToggleState;
                                newsToggle.Toggle();
                                data["news-after"] = newsToggle.Current.ToggleState;
                                var radioElement = AutomationById(rootElement, "pro-plan");
                                var radioSelection = (SelectionItemPattern)radioElement.GetCurrentPattern(SelectionItemPattern.Pattern);
                                radioSelection.Select();
                                data["radio-selected"] = radioSelection.Current.IsSelected;
                                var rangeElement = AutomationById(rootElement, "zoom-level");
                                var rangeValue = (RangeValuePattern)rangeElement.GetCurrentPattern(RangeValuePattern.Pattern);
                                data["range-bounds"] = rangeValue.Current.Minimum + ":" + rangeValue.Current.Maximum + ":" + rangeValue.Current.SmallChange;
                                rangeValue.SetValue(8D);
                                data["range-value"] = rangeValue.Current.Value;
                                var themeElement = AutomationById(rootElement, "theme");
                                var themeValue = (ValuePattern)themeElement.GetCurrentPattern(ValuePattern.Pattern);
                                themeValue.SetValue("dark");
                                data["theme-value"] = themeValue.Current.Value;
                                var summaryElement = AutomationById(rootElement, "more-summary");
                                var disclosure = (ExpandCollapsePattern)summaryElement.GetCurrentPattern(ExpandCollapsePattern.Pattern);
                                data["summary-before"] = disclosure.Current.ExpandCollapseState;
                                disclosure.Expand();
                                data["summary-after"] = disclosure.Current.ExpandCollapseState;
                                var progressElement = AutomationById(rootElement, "load-progress");
                                var progressValue = (RangeValuePattern)progressElement.GetCurrentPattern(RangeValuePattern.Pattern);
                                data["progress"] = progressValue.Current.Value + ":" + progressValue.Current.Maximum + ":" + progressValue.Current.IsReadOnly;
                                var peopleElement = AutomationById(rootElement, "people");
                                var peopleGrid = (GridPattern)peopleElement.GetCurrentPattern(GridPattern.Pattern);
                                var peopleTable = (TablePattern)peopleElement.GetCurrentPattern(TablePattern.Pattern);
                                data["table-size"] = peopleGrid.Current.RowCount + ":" + peopleGrid.Current.ColumnCount;
                                data["table-major"] = peopleTable.Current.RowOrColumnMajor;
                                var columnHeaders = peopleTable.Current.GetColumnHeaders();
                                var rowHeaders = peopleTable.Current.GetRowHeaders();
                                data["table-column-headers"] = columnHeaders.Length + ":" + columnHeaders[0].Current.Name + ":" + columnHeaders[1].Current.Name;
                                data["table-row-headers"] = rowHeaders.Length + ":" + rowHeaders[0].Current.Name;
                                var engineElement = peopleGrid.GetItem(1, 1);
                                var engineGridItem = (GridItemPattern)engineElement.GetCurrentPattern(GridItemPattern.Pattern);
                                var engineTableItem = (TableItemPattern)engineElement.GetCurrentPattern(TableItemPattern.Pattern);
                                data["cell-position"] = engineGridItem.Current.Row + ":" + engineGridItem.Current.Column + ":" + engineGridItem.Current.RowSpan + ":" + engineGridItem.Current.ColumnSpan;
                                data["cell-grid"] = engineGridItem.Current.ContainingGrid.Current.AutomationId;
                                var engineColumnHeaders = engineTableItem.Current.GetColumnHeaderItems();
                                var engineRowHeaders = engineTableItem.Current.GetRowHeaderItems();
                                data["cell-column-headers"] = engineColumnHeaders.Length + ":" + engineColumnHeaders[0].Current.Name;
                                data["cell-row-headers"] = engineRowHeaders.Length + ":" + engineRowHeaders[0].Current.Name;
                                var noteLeft = peopleGrid.GetItem(2, 0);
                                var noteRight = peopleGrid.GetItem(2, 1);
                                var noteGridItem = (GridItemPattern)noteLeft.GetCurrentPattern(GridItemPattern.Pattern);
                                data["span-cell"] = noteLeft.Current.AutomationId + ":" + noteRight.Current.AutomationId + ":" + noteGridItem.Current.Column + ":" + noteGridItem.Current.ColumnSpan;
                                try { peopleGrid.GetItem(3, 0); data["table-range"] = false; }
                                catch (ArgumentOutOfRangeException) { data["table-range"] = true; }
                                var homeElement = AutomationById(rootElement, "home-link");
                                data["home-key"] = homeElement.Current.AccessKey;
                                object homePattern;
                                data["home-invoke"] = homeElement.TryGetCurrentPattern(InvokePattern.Pattern, out homePattern);
                                object scrollPattern;
                                data["home-scroll"] = homeElement.TryGetCurrentPattern(ScrollItemPattern.Pattern, out scrollPattern);
                                var textPattern = (TextPattern)rootElement.GetCurrentPattern(TextPattern.Pattern);
                                var documentText = textPattern.DocumentRange;
                                var fullText = documentText.GetText(-1);
                                data["text-selection"] = textPattern.SupportedTextSelection;
                                data["text-prefix"] = documentText.GetText(4);
                                data["text-content"] = fullText.Contains("Lea semantic fixture") && fullText.Contains("Shared note") && fullText.Contains("Hidden action") && !fullText.Contains("do-not-expose");
                                data["text-readonly"] = documentText.GetAttributeValue(TextPattern.IsReadOnlyAttribute);
                                var firstCharacter = documentText.Clone();
                                firstCharacter.ExpandToEnclosingUnit(TextUnit.Character);
                                var movedCharacter = firstCharacter.Move(TextUnit.Character, 1);
                                data["text-character"] = movedCharacter + ":" + firstCharacter.GetText(-1);
                                var foundText = documentText.FindText("shared NOTE", false, true);
                                data["text-find"] = foundText == null ? "" : foundText.GetText(-1);
                                var readOnlyText = documentText.FindAttribute(TextPattern.IsReadOnlyAttribute, true, false);
                                data["text-find-attribute"] = readOnlyText == null ? "" : readOnlyText.GetText(4);
                                var endpointText = documentText.Clone();
                                endpointText.MoveEndpointByRange(TextPatternRangeEndpoint.Start, foundText, TextPatternRangeEndpoint.Start);
                                endpointText.MoveEndpointByRange(TextPatternRangeEndpoint.End, foundText, TextPatternRangeEndpoint.End);
                                data["text-endpoints"] = endpointText.GetText(-1) + ":" + endpointText.Compare(foundText) + ":" +
                                    (endpointText.CompareEndpoints(TextPatternRangeEndpoint.Start, documentText, TextPatternRangeEndpoint.Start) > 0);
                                var visibleText = textPattern.GetVisibleRanges();
                                data["text-visible"] = visibleText != null && visibleText.Length == 0;
                                var homeText = textPattern.RangeFromChild(homeElement);
                                data["text-child"] = homeText.GetText(-1) + ":" + homeText.GetEnclosingElement().Current.AutomationId;
                                var introElement = AutomationById(rootElement, "intro");
                                ((ScrollItemPattern)introElement.GetCurrentPattern(ScrollItemPattern.Pattern)).ScrollIntoView();
                                var introText = textPattern.RangeFromChild(introElement);
                                var introBounds = introElement.Current.BoundingRectangle;
                                var pointText = textPattern.RangeFromPoint(new System.Windows.Point(introBounds.Left + introBounds.Width / 2D, introBounds.Top + introBounds.Height / 2D));
                                pointText.ExpandToEnclosingUnit(TextUnit.Word);
                                data["text-point"] = pointText.GetText(-1).Trim().Length > 0;
                                data["text-bounds"] = introText.GetText(-1) + ":" + introText.GetBoundingRectangles().Length;
                                var visualLine = introText.Clone();
                                visualLine.ExpandToEnclosingUnit(TextUnit.Line);
                                var firstLine = visualLine.GetText(-1);
                                var firstLineBounds = visualLine.GetBoundingRectangles();
                                var movedLine = visualLine.Move(TextUnit.Line, 1);
                                var secondLine = visualLine.GetText(-1);
                                var secondLineBounds = visualLine.GetBoundingRectangles();
                                data["text-lines"] = firstLine.Trim().Length > 0 && firstLine.Length < introText.GetText(-1).Length &&
                                    movedLine == 1 && secondLine.Trim().Length > 0 && firstLineBounds.Length == 1 && secondLineBounds.Length == 1;
                                var compactText = introText.FindText("compact", false, false);
                                var compactBounds = compactText.GetBoundingRectangles();
                                data["text-partial-bounds"] = compactBounds.Length == 1 && firstLineBounds.Length == 1 &&
                                    compactBounds[0].Width > 0 && compactBounds[0].Width < firstLineBounds[0].Width;
                                var embedded = documentText.GetChildren();
                                var embeddedHome = false;
                                var embeddedTable = false;
                                foreach (var child in embedded)
                                {
                                    if (child.Current.AutomationId == "home-link") embeddedHome = true;
                                    if (child.Current.AutomationId == "people") embeddedTable = true;
                                }
                                data["text-children"] = embeddedHome && embeddedTable && embedded.Length <= 64;
                                foundText.ScrollIntoView(true);
                                data["text-scroll"] = true;
                                var areaElement = AutomationById(rootElement, "logo-area");
                                ((ScrollItemPattern)areaElement.GetCurrentPattern(ScrollItemPattern.Pattern)).ScrollIntoView();
                                data["area-scroll"] = true;
                            }
                            catch (Exception error) { automationError = error; }
                            finally { completed.Set(); }
                        }));
                        client.SetApartmentState(ApartmentState.MTA);
                        client.Start();
                        var automationUntil = DateTime.UtcNow.AddSeconds(30);
                        while (!completed.WaitOne(10))
                        {
                            Application.DoEvents();
                            if (DateTime.UtcNow >= automationUntil) throw new TimeoutException("UI Automation client probe exceeded 30 seconds.");
                        }
                        if (!client.Join(1000)) throw new TimeoutException("UI Automation client probe did not exit.");
                    }
                    if (automationError != null) throw new InvalidOperationException("UI Automation client probe failed.", automationError);
                    Eq("Lea Accessibility", (string)data["root-name"], "UIA document name");
                    Eq(ControlType.Document, (ControlType)data["root-type"], "UIA document control type");
                    Eq("Lea", (string)data["root-framework"], "UIA framework identity");
                    Ok((int)data["count"] > 30 && (int)data["count"] <= 511, "bounded UIA fragment descendants");
                    Eq(ControlType.Edit, (ControlType)data["user-type"], "UIA text field control type");
                    Ok((bool)data["user-required"], "UIA required field property");
                    Eq("Elliot", (string)data["user-before"], "UIA initial value");
                    Eq("UIA name", (string)data["user-after"], "UIA editable value pattern");
                    Ok((bool)data["secret-password"] && !(bool)data["secret-value"], "UIA password protection and value suppression");
                    Eq(ToggleState.On, (ToggleState)data["news-before"], "UIA initial checkbox state");
                    Eq(ToggleState.Off, (ToggleState)data["news-after"], "UIA checkbox toggle pattern");
                    Ok((bool)data["radio-selected"], "UIA radio selection pattern");
                    Eq("0:10:2", (string)data["range-bounds"], "UIA slider range bounds");
                    Eq(8D, (double)data["range-value"], "UIA slider range mutation");
                    Eq("Dark", (string)data["theme-value"], "UIA select value mutation");
                    Eq(ExpandCollapseState.Collapsed, (ExpandCollapseState)data["summary-before"], "UIA collapsed disclosure state");
                    Eq(ExpandCollapseState.Expanded, (ExpandCollapseState)data["summary-after"], "UIA disclosure expand pattern");
                    Eq("4:10:True", (string)data["progress"], "UIA read-only progress range");
                    Eq("3:2", (string)data["table-size"], "UIA grid row and column count");
                    Eq(RowOrColumnMajor.RowMajor, (RowOrColumnMajor)data["table-major"], "UIA table row-major order");
                    Eq("2:Name:Role", (string)data["table-column-headers"], "UIA table column headers");
                    Eq("1:Lea", (string)data["table-row-headers"], "UIA table row headers");
                    Eq("1:1:1:1", (string)data["cell-position"], "UIA grid item coordinates");
                    Eq("people", (string)data["cell-grid"], "UIA grid item containing table");
                    Eq("1:Role", (string)data["cell-column-headers"], "UIA table item column header");
                    Eq("1:Lea", (string)data["cell-row-headers"], "UIA table item row header");
                    Eq("people-note:people-note:0:2", (string)data["span-cell"], "UIA colspan grid mapping");
                    Ok((bool)data["table-range"], "UIA out-of-range grid rejection");
                    Eq("Alt+Shift+H", (string)data["home-key"], "UIA access key property");
                    Ok((bool)data["home-invoke"] && (bool)data["home-scroll"], "UIA invoke and scroll-item patterns");
                    Eq(SupportedTextSelection.None, (SupportedTextSelection)data["text-selection"], "UIA read-only text selection mode");
                    Eq("Lea ", (string)data["text-prefix"], "UIA bounded document text prefix");
                    Ok((bool)data["text-content"] && (bool)data["text-readonly"], "UIA logical text and read-only attribute");
                    Eq("1:e", (string)data["text-character"], "UIA character range movement");
                    Eq("Shared note", (string)data["text-find"], "UIA case-insensitive text search");
                    Eq("Lea ", (string)data["text-find-attribute"], "UIA read-only text attribute search");
                    Eq("Shared note:True:True", (string)data["text-endpoints"], "UIA text endpoint movement and comparison");
                    Ok((bool)data["text-visible"], "UIA bounded visible text ranges");
                    Ok((bool)data["text-point"], "UIA point text range");
                    Eq("A compact accessibility model for Lea hosts.:2", (string)data["text-bounds"], "UIA visual-line text range bounds");
                    Ok((bool)data["text-lines"], "UIA visual line unit navigation");
                    Ok((bool)data["text-partial-bounds"], "UIA partial text range geometry");
                    Eq("Home:home-link", (string)data["text-child"], "UIA child text range and enclosing element");
                    Ok((bool)data["text-children"], "UIA embedded text children");
                    Ok((bool)data["text-scroll"], "UIA text scroll-to-view");
                    Ok((bool)data["area-scroll"] && automationView.Sy > 0, "UIA image-map area scroll-to-view");
                    Eq("UIA name", automationView.Id("user-name") == null ? "" : automationView.Ax(automationView.Id("user-name")).V, "UIA value reaches Lea model");
                    automationHost.Hide();
                }

                var boundedAccessibilityPath = Path.Combine(Path.GetTempPath(), "lea-accessibility-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var boundedAccessibility = new StringBuilder("<!doctype html><html><head><title>Bounded accessibility</title></head><body>");
                    var references = new StringBuilder();
                    for (var referenceIndex = 0; referenceIndex < 20; referenceIndex++)
                    {
                        boundedAccessibility.Append("<span id='r").Append(referenceIndex).Append("'>R").Append(referenceIndex).Append("</span>");
                        if (referenceIndex > 0) references.Append(' ');
                        references.Append('r').Append(referenceIndex);
                    }
                    boundedAccessibility.Append("<button id='refs' aria-labelledby='").Append(references).Append("'>fallback</button>");
                    for (var accessibilityIndex = 0; accessibilityIndex < 600; accessibilityIndex++)
                        boundedAccessibility.Append("<button id='x").Append(accessibilityIndex).Append("'>Action ").Append(accessibilityIndex).Append("</button>");
                    boundedAccessibility.Append("</body></html>");
                    File.WriteAllText(boundedAccessibilityPath, boundedAccessibility.ToString(), new UTF8Encoding(false));
                    var boundedAccessibilityEngine = new E();
                    var boundedAccessibilityPage = boundedAccessibilityEngine.Go(boundedAccessibilityPath);
                    Eq(512, boundedAccessibilityPage.Axs.Count, "accessibility record count cap");
                    Eq("R0 R1 R2 R3 R4 R5 R6 R7 R8 R9 R10 R11 R12 R13 R14 R15", boundedAccessibilityEngine.Ax(boundedAccessibilityPage, ById(boundedAccessibilityPage.D, "refs")).N, "accessibility ID reference cap");
                }
                finally
                {
                    if (File.Exists(boundedAccessibilityPath)) File.Delete(boundedAccessibilityPath);
                }

                var positioningPath = Path.Combine(pages, "positioning.html");
                var positioningPage = engine.Go(positioningPath);
                Eq(6, positioningPage.Bs.Count, "positioning block count");
                Eq(4, positioningPage.Ls.Count, "positioning link count");
                var stickyPath = Path.Combine(pages, "sticky-positioning.html");
                var stickyPage = engine.Go(stickyPath);
                Eq("k", BlockById(stickyPage, "sticky").S.Pn, "sticky position alias");
                Eq(14, BlockById(stickyPage, "sticky").S.Tp.Value, "sticky top inset");
                Eq("k", BlockById(stickyPage, "bottom").S.Pn, "bottom sticky position alias");
                Eq(12, BlockById(stickyPage, "bottom").S.Bm.Value, "sticky bottom inset");
                Eq("r", BlockById(stickyPage, "invalid").S.Pn, "invalid position preserves earlier declaration");
                Ok(Object.ReferenceEquals(BlockById(stickyPage, "sticky").Up, BlockById(stickyPage, "rail")), "sticky retains containing block alias");
                Ok(Object.ReferenceEquals(BlockById(stickyPage, "sticky-child").Up, BlockById(stickyPage, "sticky")), "sticky retains descendant ownership");
                var contextsPath = Path.Combine(pages, "position-contexts.html");
                var contextsPage = engine.Go(contextsPath);
                Eq(13, contextsPage.Bs.Count, "position context block count");
                Eq(3, contextsPage.Ls.Count, "position context link count");
                Eq("a", contextsPage.Bs[2].S.Pn, "absolute position alias");
                Eq(118, contextsPage.Bs[2].S.Tp, "absolute top offset");
                Eq(330, contextsPage.Bs[2].S.Lt, "absolute left offset");
                Eq("a", contextsPage.Bs[4].S.Pn, "absolute table position alias");
                Eq("a", contextsPage.Bs[11].S.Pn, "right anchored absolute alias");
                Eq(35, contextsPage.Bs[11].S.Rt, "absolute right offset");
                Eq(80, contextsPage.Bs[11].S.Bm, "absolute bottom offset");
                Eq("f", contextsPage.Bs[12].S.Pn, "fixed position alias");
                Eq(-70, contextsPage.Bs[12].S.Rt, "negative fixed right offset");
                Eq(18, contextsPage.Bs[12].S.Bm, "fixed bottom offset");
                var positionValuesPath = Path.Combine(pages, "position-values.html");
                var positionValuesPage = engine.Go(positionValuesPath);
                Eq(10, positionValuesPage.Bs.Count, "position values block count");
                Eq(4, positionValuesPage.Ls.Count, "position values link count");
                Eq("h1,section,p,p,p,div,div,p,a,p", BlockKinds(positionValuesPage), "position values block kinds");
                Eq(14, positionValuesPage.Bs[2].S.Tp.Value, "inset top alias");
                Eq(20, positionValuesPage.Bs[2].S.Rt.Value, "inset right alias");
                Ok(!positionValuesPage.Bs[2].S.Bm.HasValue, "inset auto bottom alias");
                Eq(24, positionValuesPage.Bs[2].S.Lt.Value, "inset left alias");
                Eq(76, positionValuesPage.Bs[3].S.Tp.Value, "four-value inset top");
                Eq(190, positionValuesPage.Bs[3].S.Rt.Value, "four-value inset right");
                Eq(12, positionValuesPage.Bs[3].S.Bm.Value, "four-value inset bottom");
                Eq(24, positionValuesPage.Bs[3].S.Lt.Value, "four-value inset left");
                Ok(!positionValuesPage.Bs[4].S.Tp.HasValue && !positionValuesPage.Bs[4].S.Lt.HasValue, "auto inset corner aliases");
                Eq(18, positionValuesPage.Bs[4].S.Rt.Value, "corner right inset");
                Eq(14, positionValuesPage.Bs[4].S.Bm.Value, "corner bottom inset");
                Ok(positionValuesPage.Bs[5].S.Tx == 0 && positionValuesPage.Bs[5].S.Ty == 0 && !positionValuesPage.Bs[5].S.Zc, "transform none resets translate context");
                Ok(positionValuesPage.Bs[6].S.Tx == 18 && positionValuesPage.Bs[6].S.Ty == 10 && positionValuesPage.Bs[6].S.Zc, "translate aliases create local context");
                Ok(Object.ReferenceEquals(positionValuesPage.Bs[2].Up, positionValuesPage.Bs[1]) && Object.ReferenceEquals(positionValuesPage.Bs[3].Up, positionValuesPage.Bs[1]) && Object.ReferenceEquals(positionValuesPage.Bs[4].Up, positionValuesPage.Bs[1]), "inset children retain positioned owner");
                Ok(Object.ReferenceEquals(positionValuesPage.Bs[7].Up, positionValuesPage.Bs[6]), "translated context owns high child");
                var nestedPositionPath = Path.Combine(pages, "nested-positioning.html");
                var nestedPositionPage = engine.Go(nestedPositionPath);
                Eq(15, nestedPositionPage.Bs.Count, "nested positioning block count");
                Eq(4, nestedPositionPage.Ls.Count, "nested positioning link count");
                Eq("h1,div,div,p,p,div,p,div,p,p,p,p,p,p,p", BlockKinds(nestedPositionPage), "nested positioning block kinds");
                Ok(Object.ReferenceEquals(nestedPositionPage.Bs[2].Up, nestedPositionPage.Bs[1]), "nested clipper parent alias");
                Ok(Object.ReferenceEquals(nestedPositionPage.Bs[3].Up, nestedPositionPage.Bs[2]), "absolute grandchild parent alias");
                Ok(Object.ReferenceEquals(nestedPositionPage.Bs[4].Up, nestedPositionPage.Bs[1]), "absolute child parent alias");
                Ok(Object.ReferenceEquals(nestedPositionPage.Bs[6].Up, nestedPositionPage.Bs[5]), "stacked child parent alias");
                Ok(Object.ReferenceEquals(nestedPositionPage.Bs[8].Up, nestedPositionPage.Bs[7]), "fixed child parent alias");
                Ok(nestedPositionPage.Bs[9].Up == null, "root flow block has no context parent");
                var nestedFlowPath = Path.Combine(pages, "nested-flow.html");
                var nestedFlowPage = engine.Go(nestedFlowPath);
                Eq(12, nestedFlowPage.Bs.Count, "nested flow block count");
                Eq(1, nestedFlowPage.Ls.Count, "nested flow link count");
                Eq("h1,section,p,div,article,h2,p,p,p,section,p,p", BlockKinds(nestedFlowPage), "nested flow block kinds");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[2].Up, nestedFlowPage.Bs[1]), "outer visual context owns first child");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[3].Up, nestedFlowPage.Bs[1]), "outer visual context owns middle child");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[4].Up, nestedFlowPage.Bs[3]), "middle visual context owns inner child");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[5].Up, nestedFlowPage.Bs[4]) && Object.ReferenceEquals(nestedFlowPage.Bs[6].Up, nestedFlowPage.Bs[4]), "inner visual context owns text children");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[8].Up, nestedFlowPage.Bs[1]), "outer visual context resumes after nested subtree");
                Ok(Object.ReferenceEquals(nestedFlowPage.Bs[10].Up, nestedFlowPage.Bs[9]), "mixed-content visual context owns child");
                Ok(nestedFlowPage.Bs[11].Up == null, "following root block leaves visual contexts");
                var nestedContentPath = Path.Combine(pages, "nested-content.html");
                var nestedContentPage = engine.Go(nestedContentPath);
                Eq(9, nestedContentPage.Bs.Count, "nested content block count");
                Eq(2, nestedContentPage.Fs.Count, "nested content field count");
                Eq("h1,section,input,div,input,img,table,p,p", BlockKinds(nestedContentPage), "nested content block kinds");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[2].Up, nestedContentPage.Bs[1]), "form field crosses wrapper into visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[3].Up, nestedContentPage.Bs[1]) && Object.ReferenceEquals(nestedContentPage.Bs[4].Up, nestedContentPage.Bs[3]), "clipped field keeps nearest visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[5].Up, nestedContentPage.Bs[1]), "picture image belongs to visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[6].Up, nestedContentPage.Bs[1]), "table belongs to visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[7].Up, nestedContentPage.Bs[1]) && nestedContentPage.Bs[8].Up == null, "nested content exits visual parent cleanly");
                var stackingPath = Path.Combine(pages, "stacking-contexts.html");
                var stackingPage = engine.Go(stackingPath);
                Eq(17, stackingPage.Bs.Count, "stacking context block count");
                Eq(9, stackingPage.Ls.Count, "stacking context link count");
                Eq("h1,div,p,p,div,p,p,div,p,p,div,p,p,section,p,p,p", BlockKinds(stackingPage), "stacking context block kinds");
                Ok(stackingPage.Bs[1].S.Zc && !stackingPage.Bs[1].S.Za && stackingPage.Bs[1].S.Op == 75, "local opacity context aliases");
                Ok(stackingPage.Bs[4].S.Zc && stackingPage.Bs[4].S.Op == 100, "isolation context alias");
                Ok(!stackingPage.Bs[7].S.Zc && !stackingPage.Bs[7].S.Za && stackingPage.Bs[7].S.Zi == 0, "auto isolation and z-index reset aliases");
                Ok(!stackingPage.Bs[10].S.Zc && stackingPage.Bs[10].S.Za && stackingPage.Bs[10].S.Zi == 0, "numeric zero z-index alias");
                Ok(stackingPage.Bs[13].S.Za && stackingPage.Bs[13].S.Zi == 3, "auto-height positioned context z-index");
                Ok(Object.ReferenceEquals(stackingPage.Bs[2].Up, stackingPage.Bs[1]) && Object.ReferenceEquals(stackingPage.Bs[5].Up, stackingPage.Bs[4]), "opacity and isolation context ownership");
                Ok(Object.ReferenceEquals(stackingPage.Bs[8].Up, stackingPage.Bs[7]) && Object.ReferenceEquals(stackingPage.Bs[11].Up, stackingPage.Bs[10]), "auto and zero z-index context ownership");
                Ok(Object.ReferenceEquals(stackingPage.Bs[14].Up, stackingPage.Bs[13]) && Object.ReferenceEquals(stackingPage.Bs[15].Up, stackingPage.Bs[13]), "auto-height positioned ownership");
                var backLayer = positioningPage.Bs[1].S;
                var frontLayer = positioningPage.Bs[2].S;
                var rightLayer = positioningPage.Bs[3].S;
                var staticLayer = positioningPage.Bs[4].S;
                Eq("r", backLayer.Pn, "relative position alias");
                Eq(20, backLayer.Lt.Value, "left offset alias");
                Eq(1, backLayer.Zi, "back z-index alias");
                Eq(-82, frontLayer.Tp.Value, "signed top offset alias");
                Eq(40, frontLayer.Lt.Value, "front left offset alias");
                Eq(5, frontLayer.Zi, "front z-index alias");
                Eq(15, rightLayer.Rt.Value, "right offset alias");
                Eq(10, rightLayer.Bm.Value, "bottom offset alias");
                Eq("s", staticLayer.Pn, "static position alias");
                Eq(-300, staticLayer.Tp.Value, "static top remains parsed");
                Eq(999, staticLayer.Zi, "static z-index remains parsed");
                Eq("r", positioningPage.Bs[5].S.Pn, "relative table position alias");
                Eq(12, positioningPage.Bs[5].S.Lt.Value, "relative table left offset");
                Eq(3, positioningPage.Bs[5].S.Zi, "relative table z-index");

                var selectorPath = Path.Combine(pages, "selectors.html");
                var selectorPage = engine.Go(selectorPath);
                Eq(12, selectorPage.Bs.Count, "relationship selector block count");
                Eq("h1,p,p,p,p,div,div,div,h2,p,div,p", BlockKinds(selectorPage), "relationship selector block kinds");
                Eq("#176b4d", selectorPage.Bs[1].S.F, "compact child selector");
                Eq(17, selectorPage.Bs[1].S.Fs, "universal child selector");
                Eq("#a13b18", selectorPage.Bs[2].S.F, "nested child selector");
                Eq("#e8f4ed", selectorPage.Bs[3].S.Bg, "adjacent selector skips text nodes");
                Eq(0, selectorPage.Bs[3].S.Bl, "nonmatching paragraph adjacency");
                Eq(4, selectorPage.Bs[4].S.Bl, "compact adjacent selector border");
                Eq(7, selectorPage.Bs[4].S.Pl, "compact adjacent selector padding");
                Eq(24, selectorPage.Bs[4].S.Lh, "chained compact selector");
                Eq(0, selectorPage.Bs[5].S.Ml, "first sibling margin");
                Eq(18, selectorPage.Bs[6].S.Ml, "second sibling adjacent margin");
                Eq(18, selectorPage.Bs[7].S.Ml, "third sibling adjacent margin");
                Ok(selectorPage.Bs[9].S.I, "heading adjacent selector");
                Ok(!selectorPage.Bs[11].S.I, "adjacent selector respects separator");
                var queryStage = engine.Id(selectorPage, "STAGE");
                Ok(queryStage != null && queryStage.K == "main", "case-insensitive Id query");
                Eq("Direct child", NodeText(engine.Q(selectorPage, "main#stage > p.direct")), "first child query");
                Eq("Nested child", NodeText(engine.Q(selectorPage, "main > article > p.nested")), "nested child query");
                Eq("Card two", NodeText(engine.Q(selectorPage, ".card + .card")), "adjacent first query");
                Eq("Following heading", NodeText(engine.Q(selectorPage, ".missing, h2 + p.lead")), "selector group first query");
                var queryCards = engine.Qa(selectorPage, ".card");
                Eq(3, queryCards.Count, "bounded all query count");
                Eq("Card one", NodeText(queryCards[0]), "all query document order start");
                Eq("Card three", NodeText(queryCards[2]), "all query document order end");
                var queryGroup = engine.Qa(selectorPage, ".card, p.direct");
                Eq(4, queryGroup.Count, "selector group all query count");
                Eq("Direct child", NodeText(queryGroup[0]), "selector group document order");
                var queryReadOnly = false;
                try { queryCards.Add(queryStage); }
                catch (NotSupportedException) { queryReadOnly = true; }
                Ok(queryReadOnly, "all query result is read-only");
                Ok(engine.Id(selectorPage, "missing") == null && engine.Q(selectorPage, ".missing") == null, "missing query result");
                Eq(0, engine.Qa(selectorPage, "p,,div").Count, "invalid selector group rejected");
                Ok(engine.Q(selectorPage, new String('x', 1025)) == null, "oversized selector rejected");

                var negationPath = Path.Combine(pages, "selector-negation.html");
                var negationPage = engine.Go(negationPath);
                Eq(9, negationPage.Bs.Count, "general sibling and negation block count");
                Eq(2, negationPage.Fs.Count, "negation field count");
                Eq("#176b4d", negationPage.Bs[0].S.F, "negation argument specificity");
                Eq("#2b3340", negationPage.Bs[1].S.F, "general sibling excludes source");
                Eq(17, negationPage.Bs[1].S.Fs, "quoted tilde attribute selector");
                Ok(negationPage.Bs[1].S.Bg == "#edf7f2" && negationPage.Bs[1].S.Td == "u" && !negationPage.Bs[1].S.I, "class attribute and first-child negation");
                Ok(negationPage.Bs[2].S.F == "#176b4d" && negationPage.Bs[2].S.B && negationPage.Bs[2].S.I, "adjacent and general sibling combination");
                Eq("#2b3340", negationPage.Bs[3].S.F, "general sibling target kind");
                Ok(negationPage.Bs[4].S.F == "#176b4d" && negationPage.Bs[4].S.Bg == "#edf7f2" && negationPage.Bs[4].S.Td == "", "general sibling across intervening element");
                Ok(negationPage.Bs[5].S.F == "#176b4d" && negationPage.Bs[5].S.Bg == "" && negationPage.Bs[5].S.Td == "u", "negated class selector");
                Ok(negationPage.Bs[6].S.Bl == 3 && negationPage.Bs[6].S.Pl == 7, "later matching general sibling");
                Ok(!negationPage.Bs[1].S.F.Equals("#c02050", StringComparison.OrdinalIgnoreCase), "negated universal matches nothing");
                Ok(!negationPage.Fs[0].Ds && negationPage.Fs[0].S.B && negationPage.Fs[0].S.Bg == "#dff6e9", "negated disabled pseudo");
                Ok(negationPage.Fs[1].Ds && negationPage.Fs[1].S.I && negationPage.Fs[1].S.F == "#8a4b22", "negated enabled pseudo");

                var selectorFunctionPath = Path.Combine(pages, "selector-functions.html");
                var selectorFunctionEngine = new E();
                var selectorFunctionPage = selectorFunctionEngine.Go(selectorFunctionPath);
                Ok(BlockById(selectorFunctionPage, "card").S.F == "#0f4c81" && BlockById(selectorFunctionPage, "where-target").S.F == "#176b4d" && BlockById(selectorFunctionPage, "where-target").S.Pl == 9, "is maximum and where zero specificity");
                Ok(BlockById(selectorFunctionPage, "keep").S.B && BlockById(selectorFunctionPage, "keep").S.Bg == "#edf7f2" && !BlockById(selectorFunctionPage, "skip").S.B && BlockById(selectorFunctionPage, "skip").S.Bg == "#ffffff" && !BlockById(selectorFunctionPage, "off").S.B, "not selector list alternatives");
                Ok(BlockById(selectorFunctionPage, "adjacent").S.W == 120 && BlockById(selectorFunctionPage, "adjacent").S.Ml == 17 && BlockById(selectorFunctionPage, "note-div").S.W == 120 && BlockById(selectorFunctionPage, "note-div").S.Ml == 0, "functional selectors with outer combinators");
                var selectorField = FieldById(selectorFunctionPage, "field");
                selectorField.V = "Retained functional selector";
                Ok(selectorField.S.Bg == "#dff6e9" && selectorField.S.B && selectorField.S.Bl == 2 && selectorField.S.Pl == 4, "is and not enabled field selector");
                Ok(FieldById(selectorFunctionPage, "disabled").S.F == "#8a4b22" && FieldById(selectorFunctionPage, "disabled").S.I && FieldById(selectorFunctionPage, "blocked").S.Bg == "" && !FieldById(selectorFunctionPage, "blocked").S.B, "functional form-state alternatives");
                var invalidFunction = BlockById(selectorFunctionPage, "invalid").S;
                Ok(invalidFunction.F == "#334155" && invalidFunction.Bg == "" && invalidFunction.W == 0 && invalidFunction.H == 0 && invalidFunction.Pl == 0, "bounded invalid functional selectors rejected");
                Ok(BlockById(selectorFunctionPage, "forgiving").S.W == 144 && BlockById(selectorFunctionPage, "forgiving").S.H == 44, "is and where forgive unsupported alternatives");
                Eq(3, selectorFunctionEngine.Qa(selectorFunctionPage, ":is(.card, .note)").Count, "is query alternatives");
                Eq(1, selectorFunctionEngine.Qa(selectorFunctionPage, ":where(#where-target, #missing)").Count, "where query alternatives");
                Eq(1, selectorFunctionEngine.Qa(selectorFunctionPage, ".item:not(.skip, [data-off])").Count, "not query alternatives");
                Eq(0, selectorFunctionEngine.Qa(selectorFunctionPage, ":is(.a,.b,.c,.d,.e,.f,.g,.h,.i)").Count, "functional selector alternative ceiling");
                Eq(1, selectorFunctionEngine.Qa(selectorFunctionPage, ":is(.a > .b,.c)").Count, "is query forgives unsupported combinator alternative");
                Eq(1, selectorFunctionEngine.Qa(selectorFunctionPage, ":is(:not(.a),.c)").Count, "is query forgives nested functional alternative");
                Eq(0, selectorFunctionEngine.Qa(selectorFunctionPage, ":not(.a,,.c)").Count, "not query remains unforgiving");
                selectorFunctionEngine.Dy(selectorFunctionPage, ById(selectorFunctionPage.D, "hover"), null);
                Ok(BlockById(selectorFunctionPage, "hover").S.F == "#a13b18" && BlockById(selectorFunctionPage, "hover").S.Pl == 19, "dynamic is hover selector");
                selectorFunctionEngine.Dy(selectorFunctionPage, null, ById(selectorFunctionPage.D, "field"));
                Ok(selectorField.S.Bl == 5 && selectorField.S.Pl == 12 && Object.ReferenceEquals(selectorField, FieldById(selectorFunctionPage, "field")) && selectorField.V == "Retained functional selector", "dynamic is focus preserves field state");
                selectorFunctionEngine.Dy(selectorFunctionPage, null, null);
                Ok(FieldById(selectorFunctionPage, "field").S.Bl == 2 && FieldById(selectorFunctionPage, "field").S.Pl == 4, "functional focus selector clears");

                var selectorArithmeticPath = Path.Combine(pages, "selector-arithmetic.html");
                var selectorArithmeticEngine = new E();
                var selectorArithmeticPage = selectorArithmeticEngine.Go(selectorArithmeticPath);
                var arithmeticOne = BlockById(selectorArithmeticPage, "a1");
                var arithmeticFour = BlockById(selectorArithmeticPage, "a4");
                var arithmeticFive = BlockById(selectorArithmeticPage, "a5");
                var arithmeticSeven = BlockById(selectorArithmeticPage, "a7");
                var arithmeticEight = BlockById(selectorArithmeticPage, "a8");
                Ok(arithmeticOne.S.F == "#0f4c81" && arithmeticOne.S.Bg == "#edf7f2" && arithmeticOne.S.B && arithmeticOne.S.H == 31 && arithmeticOne.S.Ml == 11, "positive structural formula and specificity");
                Ok(BlockById(selectorArithmeticPage, "a2").S.Bg == "#edf7f2" && !BlockById(selectorArithmeticPage, "a2").S.B && BlockById(selectorArithmeticPage, "a2").S.H == 17 && BlockById(selectorArithmeticPage, "a3").S.Bg == "#edf7f2" && BlockById(selectorArithmeticPage, "a3").S.B, "negative structural formula range");
                Ok(arithmeticFour.S.F == "#0f4c81" && arithmeticFour.S.Td == "u" && arithmeticFour.S.H == 17, "zero coefficient structural formula");
                Ok(arithmeticFive.S.I && arithmeticFive.S.B && arithmeticFive.S.H == 31 && arithmeticFive.S.Ml == 11, "unbounded n formula and selector-list composition");
                Ok(arithmeticSeven.S.F == "#0f4c81" && arithmeticSeven.S.I && arithmeticSeven.S.B && arithmeticSeven.S.Pl == 13 && arithmeticEight.S.I && !arithmeticEight.S.B && arithmeticEight.S.Pl == 13, "reverse child formulas");
                Ok(BlockById(selectorArithmeticPage, "m2").S.Bl == 4 && BlockById(selectorArithmeticPage, "m2").S.W == 142 && BlockById(selectorArithmeticPage, "m4").S.Bl == 4 && BlockById(selectorArithmeticPage, "m4").S.W == 142 && BlockById(selectorArithmeticPage, "m1").S.Bl == 0 && BlockById(selectorArithmeticPage, "m3").S.W == 0, "forward and reverse type formulas");
                Ok(BlockById(selectorArithmeticPage, "mp2").S.F == "#a13b18" && BlockById(selectorArithmeticPage, "mp1").S.F == "#334155", "negative last-of-type formula");
                var arithmeticInvalid = BlockById(selectorArithmeticPage, "invalid").S;
                Ok(arithmeticInvalid.W == 73 && arithmeticInvalid.H == 19 && arithmeticInvalid.Pl == 3 && arithmeticInvalid.Bl == 2 && arithmeticInvalid.Bg == "", "bounded and malformed structural formulas rejected");
                Eq(3, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(3n + 1)").Count, "positive formula query");
                Eq(3, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(-n+3)").Count, "negative formula query");
                Eq(4, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(n + 5)").Count, "unit coefficient query");
                Eq(4, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(2n-1)").Count, "negative offset formula query");
                Eq(4, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(-2n+7)").Count, "negative coefficient stepped query");
                Eq(8, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(n-2)").Count, "unit formula crossing zero query");
                Eq(0, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(-n-1)").Count, "negative formula below one query");
                Eq(1, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(+3)").Count, "signed fixed position query");
                Eq(0, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(-1)").Count, "negative fixed position query");
                Eq(4, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-last-child(2n)").Count, "reverse child query");
                Eq(2, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#mixed > div:nth-of-type(2n)").Count, "type formula query");
                Eq(2, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#mixed > div:nth-last-of-type(2n+1)").Count, "reverse type formula query");
                Eq(2, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > :is(p:nth-child(4n+1),.missing)").Count, "formula inside selector-list query");
                Eq(1, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(8192n+1)").Count, "structural coefficient ceiling");
                Eq(0, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(8193n+1)").Count, "structural coefficient overflow rejected");
                Eq(0, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(2n+-1)").Count, "malformed structural signs rejected");
                Eq(0, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(2 n+1)").Count, "split structural coefficient rejected");
                Eq(4, selectorArithmeticEngine.Qa(selectorArithmeticPage, "#forward > p:nth-child(2n of .rank)").Count, "filtered structural query");

                var selectorFilteredPath = Path.Combine(pages, "selector-filtered-position.html");
                var selectorFilteredEngine = new E();
                var selectorFilteredPage = selectorFilteredEngine.Go(selectorFilteredPath);
                Ok(BlockById(selectorFilteredPage, "f1").S.Bg == "#edf7f2" && BlockById(selectorFilteredPage, "f5").S.Bg == "#edf7f2" && BlockById(selectorFilteredPage, "f8").S.Bg == "#edf7f2" && BlockById(selectorFilteredPage, "f3").S.Bg == "", "filtered odd child positions");
                Ok(BlockById(selectorFilteredPage, "f7").S.B && BlockById(selectorFilteredPage, "f8").S.B && !BlockById(selectorFilteredPage, "f5").S.B, "filtered reverse child range");
                Ok(BlockById(selectorFilteredPage, "f3").S.Bl == 4 && BlockById(selectorFilteredPage, "f5").S.Pl == 13 && BlockById(selectorFilteredPage, "f1").S.F == "#0f4c81" && BlockById(selectorFilteredPage, "f4").S.F == "#0f4c81" && BlockById(selectorFilteredPage, "f7").S.F == "#0f4c81", "filtered compound alternatives and reverse parity");
                Ok(BlockById(selectorFilteredPage, "specific").S.H == 33, "filtered selector maximum specificity");
                var filteredField = FieldById(selectorFilteredPage, "e2");
                filteredField.V = "Retained filtered field";
                Ok(filteredField.S.Bg == "#dff6e9" && filteredField.S.B && FieldById(selectorFilteredPage, "e3").S.Bl == 5 && FieldById(selectorFilteredPage, "d1").S.Bg == "", "filtered form-state positions");
                ById(selectorFilteredPage.D, "e1").A["disabled"] = "";
                selectorFilteredEngine.Dy(selectorFilteredPage, null, ById(selectorFilteredPage.D, "e2"));
                Ok(filteredField.S.Bg == "" && !filteredField.S.B && FieldById(selectorFilteredPage, "e3").S.Bg == "#dff6e9" && FieldById(selectorFilteredPage, "e3").S.B && Object.ReferenceEquals(filteredField, FieldById(selectorFilteredPage, "e2")) && filteredField.V == "Retained filtered field", "filtered form-state rebuild and retention");
                ById(selectorFilteredPage.D, "e1").A.Remove("disabled");
                selectorFilteredEngine.Dy(selectorFilteredPage, null, null);
                Ok(filteredField.S.Bg == "#dff6e9" && filteredField.S.B && FieldById(selectorFilteredPage, "e3").S.Bg == "", "filtered form-state restore");
                var filteredInvalid = BlockById(selectorFilteredPage, "invalid").S;
                Ok(filteredInvalid.W == 73 && filteredInvalid.H == 19 && filteredInvalid.Pl == 3 && filteredInvalid.Bl == 2 && filteredInvalid.Br == 0 && filteredInvalid.Ml == 0 && filteredInvalid.Mr == 0 && filteredInvalid.Bg == "", "invalid filtered positions rejected");
                Eq(3, selectorFilteredEngine.Qa(selectorFilteredPage, "#items > p:nth-child(2n+1 of .pick)").Count, "filtered odd query");
                Eq(3, selectorFilteredEngine.Qa(selectorFilteredPage, "#items > p:nth-child(-n+3 of .pick)").Count, "filtered range query");
                Eq(2, selectorFilteredEngine.Qa(selectorFilteredPage, "#items > .pick:nth-child(-n+3)").Count, "physical position comparison query");
                Eq(2, selectorFilteredEngine.Qa(selectorFilteredPage, "#items > p:nth-last-child(-n+2 of .pick)").Count, "filtered reverse query");
                Eq(3, selectorFilteredEngine.Qa(selectorFilteredPage, "#items > p:nth-last-child(2n of .pick,[data-alt])").Count, "filtered reverse alternatives query");
                Eq("f5", selectorFilteredEngine.Q(selectorFilteredPage, "#items > p:nth-child(2 of .pick[data-ready])").Av("id"), "filtered compound query");
                Eq("e2", selectorFilteredEngine.Q(selectorFilteredPage, "#forms > input:nth-child(2 of :enabled)").Av("id"), "filtered pseudo query");
                Eq(1, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(n of #invalid,.a,.b,.c,.d,.e,.f,.g):nth-last-child(n of #invalid,.h,.i,.j,.k,.l,.m,.o)").Count, "filtered sixteen-test ceiling");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(n of #invalid,.a,.b,.c,.d,.e,.f,.g):nth-last-child(n of #invalid,.h,.i,.j,.k,.l,.m,.o):nth-child(n of #invalid)").Count, "filtered total test overflow rejected");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(1 of .pick,)").Count, "filtered list remains strict");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(1 of .a > .b)").Count, "filtered inner combinator rejected");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-of-type(1 of #invalid)").Count, "typed structural filter rejected");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(1 of :is(#invalid))").Count, "nested filtered selector-list rejected");
                Eq(0, selectorFilteredEngine.Qa(selectorFilteredPage, "#invalid:nth-child(1 of .a,.b,.c,.d,.e,.f,.g,.h,.i)").Count, "filtered alternative overflow rejected");

                var selectorRelationalPath = Path.Combine(pages, "selector-relational.html");
                var selectorRelationalEngine = new E();
                var selectorRelationalPage = selectorRelationalEngine.Go(selectorRelationalPath);
                Ok(BlockById(selectorRelationalPage, "direct").S.W == 118 && BlockById(selectorRelationalPage, "direct").S.Bg == "#edf7f2" && BlockById(selectorRelationalPage, "direct").S.B, "relational direct-child styles");
                Ok(BlockById(selectorRelationalPage, "descendant").S.Bl == 4 && BlockById(selectorRelationalPage, "adjacent").S.H == 31 && BlockById(selectorRelationalPage, "following").S.Pl == 11, "relational descendant and sibling styles");
                Ok(BlockById(selectorRelationalPage, "complex").S.Br == 6 && BlockById(selectorRelationalPage, "specific").S.H == 37, "relational complex chain and maximum specificity");
                var relationalField = FieldById(selectorRelationalPage, "focus-input");
                relationalField.V = "Retained relational field";
                Ok(BlockById(selectorRelationalPage, "focus-card").S.Bg == "" && !BlockById(selectorRelationalPage, "focus-card").S.B, "relational focus baseline");
                selectorRelationalEngine.Dy(selectorRelationalPage, null, ById(selectorRelationalPage.D, "focus-input"));
                Ok(BlockById(selectorRelationalPage, "focus-card").S.Bg == "#dff6e9" && BlockById(selectorRelationalPage, "focus-card").S.B && Object.ReferenceEquals(relationalField, FieldById(selectorRelationalPage, "focus-input")) && relationalField.V == "Retained relational field", "relational descendant focus rebuild and retention");
                selectorRelationalEngine.Dy(selectorRelationalPage, null, null);
                Ok(BlockById(selectorRelationalPage, "focus-card").S.Bg == "" && !BlockById(selectorRelationalPage, "focus-card").S.B, "relational descendant focus restore");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, ".card:has(> img.hero)").Count, "relational direct-child query");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, ".card:has(.body .ready[data-state=go])").Count, "relational descendant query");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, "#adjacent:has(+ dt)").Count, "relational adjacent query");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, "#following:has(~ li.end)").Count, "relational following query");
                Eq("complex", selectorRelationalEngine.Q(selectorRelationalPage, "section:has(> .body > .row + .ready)").Av("id"), "relational complex query");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, "#direct:has(> img,.a,.b,.c,.d,.e,.f,.g)").Count, "relational eight-alternative ceiling");
                Eq(1, selectorRelationalEngine.Qa(selectorRelationalPage, "#complex:has(> .body .ready,.a .b,.c .d,.e .f,.g .h,.i .j,.k .l,.m .n)").Count, "relational sixteen-part ceiling");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(.ready,)").Count, "relational list remains strict");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(:has(.ready))").Count, "nested relational selector rejected");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(::before)").Count, "relational pseudo-element rejected");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(> > .ready)").Count, "malformed relational combinator rejected");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(.a,.b,.c,.d,.e,.f,.g,.h,.i)").Count, "relational alternative overflow rejected");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(> .a .b .c .d .e)").Count, "relational part overflow rejected");
                Eq(0, selectorRelationalEngine.Qa(selectorRelationalPage, "#invalid:has(> .a .b .c,.d .e,.f .g,.h .i,.j .k,.l .m,.n .o,.p .q)").Count, "relational total part overflow rejected");
                var relationalInvalid = BlockById(selectorRelationalPage, "invalid").S;
                Ok(relationalInvalid.W == 92 && relationalInvalid.H == 21 && relationalInvalid.Pl == 3 && relationalInvalid.Bl == 0 && relationalInvalid.Br == 0 && relationalInvalid.Ml == 0 && relationalInvalid.Mr == 0, "invalid relational styles rejected");

                var hasBoundPath = Path.Combine(Path.GetTempPath(), "lea-has-bound-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var hasBound = new StringBuilder("<!doctype html><html><head><style>#root:has(> .target){width:77px}</style></head><body><section id='root'>Root");
                    for (var item = 0; item < 4096; item++) hasBound.Append("<i" + (item == 4095 ? " class='target'" : "") + "></i>");
                    hasBound.Append("</section></body></html>");
                    File.WriteAllText(hasBoundPath, hasBound.ToString(), new UTF8Encoding(false));
                    Eq(77, BlockById(selectorRelationalEngine.Go(hasBoundPath), "root").S.W, "relational traversal ceiling includes final bounded candidate");
                    hasBound.Replace("class='target'", "");
                    hasBound.Replace("</section>", "<i class='target'></i></section>");
                    File.WriteAllText(hasBoundPath, hasBound.ToString(), new UTF8Encoding(false));
                    Eq(0, BlockById(selectorRelationalEngine.Go(hasBoundPath), "root").S.W, "relational traversal overflow fails bounded");
                }
                finally { if (File.Exists(hasBoundPath)) File.Delete(hasBoundPath); }

                var selectorStatePath = Path.Combine(pages, "selector-states.html");
                var selectorStatePage = engine.Go(selectorStatePath);
                Eq("#2b3340", selectorStatePage.S.F, "root pseudo inheritance");
                Eq(13, selectorStatePage.Bs.Count, "attribute and pseudo selector block count");
                Eq(5, selectorStatePage.Fs.Count, "state selector field count");
                var selectorHeading = selectorStatePage.Bs[0].S;
                Eq("#176b4d", selectorHeading.F, "attribute value selector specificity");
                Eq("#edf7f2", selectorHeading.Bg, "attribute presence selector");
                Ok(selectorHeading.B, "attribute word selector");
                Ok(selectorHeading.I, "attribute language selector");
                Eq(3, selectorHeading.Bl, "attribute prefix selector");
                Eq(7, selectorHeading.Pl, "attribute suffix selector");
                Eq("u", selectorHeading.Td, "attribute substring selector");
                Eq(17, selectorHeading.Fs, "quoted attribute value with spaces and comma");
                Eq("#176b4d", selectorStatePage.Bs[1].S.F, "first-child selector");
                Ok(selectorStatePage.Bs[2].S.I, "even nth-child selector");
                Eq("Georgia|s", selectorStatePage.Bs[2].S.Ff, "fixed nth-of-type selector");
                Eq("u", selectorStatePage.Bs[3].S.Td, "fixed nth-child selector");
                Ok(selectorStatePage.Bs[4].S.I && selectorStatePage.Bs[4].S.Bg == "#e8f4ed", "last-child selector");
                Eq(3, selectorStatePage.Bs[5].Is.Count, "type pseudo inline run count");
                Eq("#176b4d", selectorStatePage.Bs[5].Is[0].S.F, "first-of-type selector");
                Ok(selectorStatePage.Bs[5].Is[1].S.I && selectorStatePage.Bs[5].Is[1].S.Fs == 17, "only-of-type selector");
                Ok(selectorStatePage.Bs[5].Is[2].S.Bg == "#e8f4ed" && selectorStatePage.Bs[5].Is[2].S.Td == "u", "last and nth-of-type selectors");
                Eq(3, selectorStatePage.Bs[6].S.Bl, "only-child selector");
                Eq("#0b63ce", selectorStatePage.Bs[7].Is[0].S.F, "link pseudo selector");
                Eq("n", selectorStatePage.Bs[7].Is[0].S.Td, "link pseudo decoration");
                Ok(selectorStatePage.Fs[0].C && selectorStatePage.Fs[0].S.Bg == "#dff6e9", "checked pseudo selector");
                Ok(selectorStatePage.Fs[0].S.Bl == 2 && selectorStatePage.Fs[0].S.Ff == "m", "enabled and empty pseudo selectors");
                Ok(selectorStatePage.Fs[1].Ds && selectorStatePage.Fs[1].S.F == "#8a4b22" && selectorStatePage.Fs[1].S.Bl == 0, "disabled pseudo selector");
                Ok(selectorStatePage.Fs[2].S.B && selectorStatePage.Fs[2].S.Td == "u", "required and read-write pseudo selectors");
                Ok(selectorStatePage.Fs[3].Ro && selectorStatePage.Fs[3].S.I && selectorStatePage.Fs[3].S.Td == "", "read-only pseudo selector");
                Eq("u", selectorStatePage.Fs[4].S.Td, "editable read-write pseudo selector");

                var textStylePath = Path.Combine(pages, "text-style.html");
                var textStylePage = engine.Go(textStylePath);
                Eq(8, textStylePage.Bs.Count, "text style block count");
                Eq("Georgia|s", textStylePage.Bs[0].S.Ff, "heading font family list");
                Eq("No Such Lea Font|Segoe UI|ss", textStylePage.Bs[1].S.Ff, "inherited font family list");
                Eq("Missing Serif|Georgia|s", textStylePage.Bs[2].S.Ff, "font fallback list");
                Eq("m", textStylePage.Bs[3].S.Ff, "monospace family alias");
                Eq("u", textStylePage.Bs[4].Is[0].S.Td, "underline decoration alias");
                Eq("s", textStylePage.Bs[4].Is[1].S.Td, "strike decoration alias");
                Eq("us", textStylePage.Bs[4].Is[2].S.Td, "combined decoration alias");
                Eq("u", textStylePage.Bs[5].Is[0].S.Td, "native underline tag");
                Eq("s", textStylePage.Bs[5].Is[2].S.Td, "native deletion tag");
                Eq("n", textStylePage.Bs[5].Is[3].S.Td, "native decoration cascade override");
                Eq("n", textStylePage.Bs[6].Is[0].S.Td, "plain link decoration override");
                Eq("", textStylePage.Bs[6].Is[1].S.Td, "default link decoration state");
                var styledCell = textStylePage.Bs[7].Tb.Rs[0].Cs[0];
                Eq("Times New Roman|s", styledCell.S.Ff, "table cell font family");
                Eq("u", styledCell.S.Td, "table cell decoration");

                var transformPath = Path.Combine(pages, "text-transform.html");
                var transformEngine = new E { Vw = 720 };
                var transformPage = transformEngine.Go(transformPath);
                Eq(19, transformPage.Bs.Count, "text transform block count");
                Ok(transformPage.S.Tt == "c" && BlockById(transformPage, "inherited").S.Tt == "c" &&
                    BlockById(transformPage, "inherited").X == "Inherited Heading Text", "inherited capitalize alias and text");
                Ok(BlockById(transformPage, "upper").S.Tt == "u" && BlockById(transformPage, "upper").X == "UPPER MIXED TEXT" &&
                    BlockById(transformPage, "lower").S.Tt == "l" && BlockById(transformPage, "lower").X == "lower mixed text" &&
                    BlockById(transformPage, "none").S.Tt == "n" && BlockById(transformPage, "none").X == "None MiXeD Text",
                    "uppercase lowercase and none presentation text");
                var transformInline = BlockById(transformPage, "inline");
                Eq(5, transformInline.Is.Count, "text transform inline run count");
                Ok(transformInline.X == "START mixed low Lea-Engine Words Raw MiXeD END" &&
                    transformInline.Is[0].S.Tt == "u" && transformInline.Is[1].S.Tt == "l" &&
                    transformInline.Is[2].S.Tt == "c" && transformInline.Is[3].S.Tt == "n",
                    "inline text transform overrides and compact aliases");
                Ok(BlockById(transformPage, "invalid").S.Tt == "c" && BlockById(transformPage, "invalid").X == "Invalid Preserves Capitalize" &&
                    BlockById(transformPage, "reset").S.Tt == "n" && BlockById(transformPage, "reset").X == "Initial Reset MiXeD" &&
                    BlockById(transformPage, "unset-parent").Is[1].S.Tt == "l",
                    "invalid preservation and CSS-wide text transform resets");
                Ok(BlockById(transformPage, "variable").S.Tt == "l" && BlockById(transformPage, "variable").X == "variable mixed text" &&
                    BlockById(transformPage, "priority").S.Tt == "u" && BlockById(transformPage, "priority").X == "IMPORTANT MIXED TEXT",
                    "text transform variables and important cascade");
                Ok(BlockById(transformPage, "preserved").X == "FIRST LINE\nSECOND LINE" &&
                    BlockById(transformPage, "cell").X == "cell mixed text", "multiline and table text transforms");
                var transformField = FieldById(transformPage, "field");
                Ok(BlockById(transformPage, "button").X == "SEND MIXED" && FieldById(transformPage, "button").V == "Send MiXeD" &&
                    transformField.V == "Field MiXeD Text" && E.Tx(transformField.V, transformField.S) == "field mixed text" &&
                    E.Tx(FieldById(transformPage, "placeholder").P, FieldById(transformPage, "placeholder").S) == "Placeholder Mixed Text" &&
                    E.Tx(FieldById(transformPage, "choice").Os[0].T, FieldById(transformPage, "choice").S) == "CHOICE MIXED TEXT",
                    "transformed controls preserve raw form values");
                Ok(transformPage.X.Contains("Upper MiXeD Text") && transformPage.X.Contains("Send MiXeD"),
                    "document text remains authored rather than presentation transformed");
                transformEngine.Dy(transformPage, ById(transformPage.D, "dynamic"), null);
                Ok(BlockById(transformPage, "dynamic").S.Tt == "u" && BlockById(transformPage, "dynamic").X == "DYNAMIC MIXED TEXT" &&
                    Object.ReferenceEquals(transformField, FieldById(transformPage, "field")), "dynamic transform rebuild and field retention");
                transformEngine.Dy(transformPage, null, null);
                Eq("dynamic mixed text", BlockById(transformPage, "dynamic").X, "dynamic transform restore");
                transformEngine.Vw = 350;
                Ok(BlockById(transformPage, "responsive").S.Tt == "l" && BlockById(transformPage, "responsive").X == "responsive mixed text" &&
                    Object.ReferenceEquals(transformField, FieldById(transformPage, "field")), "responsive transform rebuild and field retention");

                TextTransformSmoke(transformPath);

                var spacingPath = Path.Combine(pages, "text-spacing.html");
                var spacingEngine = new E { Vw = 720 };
                var spacingPage = spacingEngine.Go(spacingPath);
                Eq(21, spacingPage.Bs.Count, "text spacing block count");
                Ok(spacingPage.S.Ls == 1 && spacingPage.S.Wg == 2 &&
                    BlockById(spacingPage, "inherited").S.Ls == 1 && BlockById(spacingPage, "inherited").S.Wg == 2,
                    "inherited letter and word spacing aliases");
                Ok(BlockById(spacingPage, "normal").S.Ls == 0 && BlockById(spacingPage, "normal").S.Wg == 0 &&
                    BlockById(spacingPage, "positive").S.Ls == 3 && BlockById(spacingPage, "positive").S.Wg == 8 &&
                    BlockById(spacingPage, "negative").S.Ls == -2 && BlockById(spacingPage, "negative").S.Wg == -4,
                    "normal positive and negative spacing values");
                var spacingInline = BlockById(spacingPage, "inline");
                Ok(spacingInline.Is.Count == 4 && spacingInline.Is[0].S.Ls == 2 && spacingInline.Is[0].S.Wg == 5 &&
                    spacingInline.Is[1].S.Ls == 6 && spacingInline.Is[1].S.Wg == 12 &&
                    spacingInline.Is[2].S.Ls == 0 && spacingInline.Is[2].S.Wg == 0,
                    "inline spacing overrides and resets");
                Ok(BlockById(spacingPage, "invalid").S.Ls == 5 && BlockById(spacingPage, "invalid").S.Wg == 7 &&
                    BlockById(spacingPage, "bounded").S.Ls == 1 && BlockById(spacingPage, "bounded").S.Wg == 2 &&
                    BlockById(spacingPage, "reset").S.Ls == 0 && BlockById(spacingPage, "reset").S.Wg == 0,
                    "invalid bounded and CSS-wide spacing behavior");
                var spacingUnset = BlockById(spacingPage, "unset-parent");
                Ok(spacingUnset.Is.Count == 2 && spacingUnset.Is[1].S.Ls == 3 && spacingUnset.Is[1].S.Wg == 6,
                    "inherited unset spacing behavior");
                Ok(BlockById(spacingPage, "variable").S.Ls == 4 && BlockById(spacingPage, "variable").S.Wg == 9 &&
                    BlockById(spacingPage, "priority").S.Ls == 7 && BlockById(spacingPage, "priority").S.Wg == 11,
                    "spacing variables and important cascade");
                var spacingCell = BlockById(spacingPage, "spacing-table").Tb.Rs[0].Cs[0];
                Ok(spacingCell.S.Ls == 3 && spacingCell.S.Wg == 7 && spacingCell.X == "Cell spacing words wrap safely",
                    "table cell spacing model");
                var spacingField = FieldById(spacingPage, "field");
                Ok(FieldById(spacingPage, "button").V == "Send raw words" && spacingField.V == "Field raw words" &&
                    FieldById(spacingPage, "placeholder").P == "Placeholder spaced words" &&
                    FieldById(spacingPage, "choice").Os[0].T == "Choice spaced words",
                    "spaced controls preserve authored form values");
                spacingEngine.Dy(spacingPage, ById(spacingPage.D, "dynamic"), null);
                Ok(BlockById(spacingPage, "dynamic").S.Ls == 8 && BlockById(spacingPage, "dynamic").S.Wg == 14 &&
                    Object.ReferenceEquals(spacingField, FieldById(spacingPage, "field")), "dynamic spacing rebuild and field retention");
                spacingEngine.Dy(spacingPage, null, null);
                Ok(BlockById(spacingPage, "dynamic").S.Ls == 1 && BlockById(spacingPage, "dynamic").S.Wg == 2,
                    "dynamic spacing restore");
                spacingEngine.Vw = 350;
                Ok(BlockById(spacingPage, "responsive").S.Ls == -1 && BlockById(spacingPage, "responsive").S.Wg == 0 &&
                    Object.ReferenceEquals(spacingField, FieldById(spacingPage, "field")), "responsive spacing rebuild and field retention");

                TextSpacingSmoke(spacingPath);
                TextOverflowSmoke(Path.Combine(pages, "text-overflow.html"));

                var recoveryPath = Path.Combine(pages, "recovery.html");
                var recoveryPage = engine.Go(recoveryPath);
                Eq(23, recoveryPage.Bs.Count, "recovered block count");
                Eq("header,h4,p,p,p,li,li,li,li,dt,dd,dt,dd,blockquote,figcaption,summary,p,article,address,h5,h6,hr,footer", BlockKinds(recoveryPage), "recovered block kinds");
                Eq("First paragraph keeps emphasis", recoveryPage.Bs[2].X, "unclosed paragraph text");
                Ok(recoveryPage.Bs[2].Is.Count == 2 && recoveryPage.Bs[2].Is[1].S.B, "unclosed emphasis stays in its paragraph");
                Eq("Mismatched bold and italic survives.", recoveryPage.Bs[4].X, "mismatched inline text");
                Ok(recoveryPage.Bs[4].Is[1].S.B && recoveryPage.Bs[4].Is[2].S.B && recoveryPage.Bs[4].Is[2].S.I, "mismatched inline styles recover");
                Eq("Second item", recoveryPage.Bs[6].X, "outer list text excludes nested list");
                Eq("Nested item", recoveryPage.Bs[7].X, "nested list gets its own block");
                var listNodes = Nodes(recoveryPage.D, "li");
                Eq(4, listNodes.Count, "recovered list node count");
                Ok(Object.ReferenceEquals(listNodes[0].Up, listNodes[1].Up) && Object.ReferenceEquals(listNodes[0].Up, listNodes[3].Up), "outer list items remain siblings");
                Ok(listNodes[2].Up != null && listNodes[2].Up.K == "ul" && Object.ReferenceEquals(listNodes[2].Up.Up, listNodes[1]), "nested list scope is preserved");
                var termNodes = Nodes(recoveryPage.D, "dt");
                var detailNodes = Nodes(recoveryPage.D, "dd");
                Eq(2, termNodes.Count, "definition term recovery");
                Eq(2, detailNodes.Count, "definition detail recovery");
                Ok(Object.ReferenceEquals(termNodes[0].Up, termNodes[1].Up) && Object.ReferenceEquals(termNodes[0].Up, detailNodes[0].Up), "definition entries remain siblings");
                var headingFive = Nodes(recoveryPage.D, "h5")[0];
                var headingSix = Nodes(recoveryPage.D, "h6")[0];
                Ok(Object.ReferenceEquals(headingFive.Up, headingSix.Up), "unclosed headings recover as siblings");

                var listPath = Path.Combine(pages, "lists.html");
                var listPage = engine.Go(listPath);
                Eq("", listPage.Bs[0].Mk, "non-list marker stays empty");
                Eq("-", BlockById(listPage, "bullet-one").Mk, "unordered list marker");
                Eq("-", BlockById(listPage, "bullet-two").Mk, "unordered inline marker");
                Eq("3.", BlockById(listPage, "three").Mk, "ordered start marker");
                Eq("4.", BlockById(listPage, "four").Mk, "ordered continuation marker");
                Eq("7.", BlockById(listPage, "seven").Mk, "ordered value marker");
                Eq("8.", BlockById(listPage, "eight").Mk, "ordered value continuation");
                Eq("9.", BlockById(listPage, "parent").Mk, "outer marker before nested list");
                Eq("-2.", BlockById(listPage, "minus-two").Mk, "nested negative start marker");
                Eq("-10.", BlockById(listPage, "minus-ten").Mk, "nested value override marker");
                Eq("-9.", BlockById(listPage, "minus-nine").Mk, "nested value continuation");
                Eq("10.", BlockById(listPage, "ten").Mk, "outer scope survives nested list");
                Eq("1.", BlockById(listPage, "bad-start").Mk, "oversized list start rejected");
                Eq("2.", BlockById(listPage, "bad-value").Mk, "malformed list value ignored");
                Eq("999999.", BlockById(listPage, "max-value").Mk, "maximum list marker");
                Eq("999999.", BlockById(listPage, "max-next").Mk, "list marker increment bounded");
                Eq("-999999.", BlockById(listPage, "min-value").Mk, "minimum list marker");
                Eq("-999998.", BlockById(listPage, "min-next").Mk, "negative list marker increment");
                var listTable = listPage.Bs[listPage.Bs.Count - 1].Tb;
                Ok(listTable != null && listTable.Rs.Count == 1, "list table model");
                Eq("4.", listTable.Rs[0].Cs[0].Bs[0].Mk, "ordered marker inside table cell");

                var disclosurePath = Path.Combine(pages, "disclosure.html");
                var disclosureEngine = new E();
                var disclosurePage = disclosureEngine.Go(disclosurePath);
                var closedDetails = ById(disclosurePage.D, "closed");
                var closedSummary = ById(disclosurePage.D, "closed-summary");
                var outerDetails = ById(disclosurePage.D, "outer");
                var nestedDetails = ById(disclosurePage.D, "nested");
                Ok(!closedDetails.Op && outerDetails.Op && !nestedDetails.Op, "initial disclosure open state");
                Eq(">", BlockById(disclosurePage, "closed-summary").Mk, "closed disclosure marker");
                Eq("v", BlockById(disclosurePage, "outer-summary").Mk, "open disclosure marker");
                Ok(BlockById(disclosurePage, "closed-content").Hd && FieldBlock(disclosurePage, FieldById(disclosurePage, "closed-field")).Hd, "closed disclosure hides retained content");
                Ok(!BlockById(disclosurePage, "after-closed").Hd && !BlockById(disclosurePage, "outer-content").Hd, "content outside and inside open details stays visible");
                Ok(!BlockById(disclosurePage, "nested-summary").Hd && BlockById(disclosurePage, "nested-content").Hd, "nested disclosure visibility");
                Ok(BlockById(disclosurePage, "second-summary").Hd && BlockById(disclosurePage, "second-summary").Mk.Length == 0, "only first direct summary is active");
                Ok(!BlockById(disclosurePage, "stray-summary").Hd && BlockById(disclosurePage, "stray-summary").Mk.Length == 0, "stray summary remains ordinary content");
                Ok(BlockById(disclosurePage, "no-summary-content").Hd, "summary-free details starts closed");
                Ok(!BlockById(disclosurePage, "cell-summary").Hd && BlockById(disclosurePage, "cell-content").Hd, "table cell disclosure visibility");
                Ok(Object.ReferenceEquals(BlockById(disclosurePage, "closed-content").Up, BlockById(disclosurePage, "closed")), "hidden block keeps visual ownership");

                var retainedField = FieldById(disclosurePage, "closed-field");
                retainedField.V = "Edited while closed";
                Eq(1, disclosureEngine.Ds(disclosurePage, closedSummary), "host opens disclosure by summary");
                Ok(closedDetails.Op && closedDetails.A.ContainsKey("open"), "open state and attribute stay synchronized");
                Eq("v", BlockById(disclosurePage, "closed-summary").Mk, "opened disclosure marker");
                Eq("#176b4d", BlockById(disclosurePage, "closed-summary").S.F, "open attribute reapplies CSS");
                Ok(!BlockById(disclosurePage, "closed-content").Hd && !FieldBlock(disclosurePage, retainedField).Hd, "opened disclosure reveals content");
                Ok(Object.ReferenceEquals(retainedField, FieldById(disclosurePage, "closed-field")) && retainedField.V == "Edited while closed", "disclosure rebuild preserves mutable field state");
                Eq(1, disclosureEngine.Ds(disclosurePage, closedSummary), "host closes disclosure by summary");
                Ok(!closedDetails.Op && !closedDetails.A.ContainsKey("open") && retainedField.V == "Edited while closed", "closed disclosure preserves edited field state");
                Eq(0, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "second-summary")), "duplicate summary cannot toggle details");
                Eq(0, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "stray-summary")), "stray summary cannot toggle details");

                var noSummary = ById(disclosurePage.D, "no-summary");
                Eq(1, disclosureEngine.Ds(disclosurePage, noSummary), "host opens summary-free details");
                Ok(noSummary.Op && !BlockById(disclosurePage, "no-summary-content").Hd, "summary-free host state reveals content");
                Eq(1, disclosureEngine.Ds(disclosurePage, noSummary), "host closes summary-free details");
                Eq(1, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "cell-summary")), "table summary toggles through engine");
                Ok(!BlockById(disclosurePage, "cell-content").Hd, "table disclosure reveals cell content");

                Eq(1, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "nested-summary")), "nested disclosure opens independently");
                Ok(nestedDetails.Op && !BlockById(disclosurePage, "nested-content").Hd, "nested content opens");
                Eq(1, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "outer-summary")), "outer disclosure closes");
                Ok(outerDetails.Op == false && nestedDetails.Op && BlockById(disclosurePage, "nested-summary").Hd && BlockById(disclosurePage, "nested-content").Hd, "closed outer disclosure masks nested open state");
                Eq(1, disclosureEngine.Ds(disclosurePage, ById(disclosurePage.D, "outer-summary")), "outer disclosure reopens");
                Ok(outerDetails.Op && nestedDetails.Op && !BlockById(disclosurePage, "nested-content").Hd, "nested open state survives outer toggle");

                var fragmentPath = Path.Combine(pages, "fragments.html");
                var fragmentEngine = new E();
                var fragmentPage = fragmentEngine.Go(fragmentPath);
                var fragmentRequests = fragmentEngine.Dg(fragmentPage).Rs.Count;
                Eq("target", fragmentEngine.Ft(fragmentPage, "#target").Av("id"), "fragment ID target lookup");
                Eq("encoded target", fragmentEngine.Ft(fragmentPage, "#encoded%20target").Av("id"), "percent-decoded fragment target lookup");
                Eq("legacy-anchor", fragmentEngine.Ft(fragmentPage, "legacy-anchor").Av("name"), "named anchor fragment lookup");
                Ok(fragmentEngine.Ft(fragmentPage, "#missing-target") == null && fragmentEngine.Ft(fragmentPage, "#" + new String('x', 4097)) == null, "missing and oversized fragments stay bounded");
                var retainedFragmentField = FieldById(fragmentPage, "fragment-field");
                retainedFragmentField.V = "Edited before fragment reveal";

                var fragmentGoPage = fragmentEngine.Go("#target");
                Ok(Object.ReferenceEquals(fragmentPage, fragmentGoPage) && fragmentEngine.CanBk, "same-document Go retains page and adds history");
                Eq("#target", fragmentEngine.At.U.Fragment, "fragment Go updates current address");
                Eq(fragmentRequests, fragmentEngine.Dg(fragmentPage).Rs.Count, "fragment Go performs no request");
                Eq(1, fragmentEngine.Fg("#missing-target"), "missing fragment still updates same-document history");
                Eq("#missing-target", fragmentEngine.At.U.Fragment, "missing fragment address");
                Ok(Object.ReferenceEquals(fragmentPage, fragmentEngine.Bk()) && fragmentEngine.At.U.Fragment == "#target", "fragment Back retains document and URL");
                Ok(Object.ReferenceEquals(fragmentPage, fragmentEngine.Fw()) && fragmentEngine.At.U.Fragment == "#missing-target", "fragment Forward retains document and URL");
                Eq(0, fragmentEngine.Fg(new Uri(Path.Combine(pages, "second.html"))), "cross-document URL is not a fragment navigation");

                Eq(1, fragmentEngine.Fg("#closed-target"), "closed fragment history navigation");
                var closedFragmentTarget = fragmentEngine.Ft(fragmentPage, fragmentEngine.At.U.Fragment);
                Eq(1, fragmentEngine.Rv(fragmentPage, closedFragmentTarget), "fragment reveals one closed details ancestor");
                Ok(ById(fragmentPage.D, "closed-details").Op && !BlockById(fragmentPage, "closed-target").Hd, "revealed fragment content joins layout model");
                Ok(Object.ReferenceEquals(retainedFragmentField, FieldById(fragmentPage, "fragment-field")) && retainedFragmentField.V == "Edited before fragment reveal", "fragment reveal preserves field state");
                Eq(0, fragmentEngine.Rv(fragmentPage, closedFragmentTarget), "already visible fragment needs no rebuild");
                var fragmentAsyncPage = Complete(fragmentEngine.GoA("#encoded%20target"), "async fragment target");
                Ok(Object.ReferenceEquals(fragmentPage, fragmentAsyncPage) && fragmentEngine.At.U.Fragment == "#encoded%20target", "asynchronous fragment Go retains page");
                Eq(fragmentRequests, fragmentEngine.Dg(fragmentPage).Rs.Count, "asynchronous fragment Go performs no request");

                Eq(1, fragmentEngine.Fg("#"), "empty fragment navigates to document top");
                Eq(1, fragmentEngine.Fg("#target"), "fragment navigation after top");
                Ok(Object.ReferenceEquals(fragmentPage, fragmentEngine.Go("#target")), "same fragment Go is a bounded no-op");
                fragmentEngine.Bk();
                Eq("#", fragmentEngine.At.U.Fragment, "same fragment does not duplicate history");
                fragmentEngine.Fw();
                for (var fragmentIndex = 0; fragmentIndex < 140; fragmentIndex++) Eq(1, fragmentEngine.Fg("#bounded-" + fragmentIndex), "bounded fragment history push");
                var fragmentBackCount = 0;
                while (fragmentEngine.CanBk) { fragmentEngine.Bk(); fragmentBackCount++; }
                Eq(127, fragmentBackCount, "fragment history keeps 128 entries");

                var gaugePath = Path.Combine(pages, "gauges.html");
                var gaugePage = new E().Go(gaugePath);
                var styledProgress = BlockById(gaugePage, "styled-progress");
                Eq("progress", styledProgress.K, "progress block kind");
                Ok(styledProgress.Vm != null && !styledProgress.Vm.In, "determinate progress value model");
                Eq(0D, styledProgress.Vm.Mn, "progress minimum alias");
                Eq(8D, styledProgress.Vm.Mx, "progress maximum alias");
                Eq(3D, styledProgress.Vm.V, "progress value alias");
                Eq(0.375D, styledProgress.Vm.R, "progress ratio alias");
                Eq("#176b4d", styledProgress.S.F, "progress inherits foreground style");
                Eq("", styledProgress.X, "supported progress hides fallback text");
                Ok(BlockById(gaugePage, "indeterminate-progress").Vm.In, "missing progress value is indeterminate");
                var badProgress = BlockById(gaugePage, "bad-progress").Vm;
                Ok(!badProgress.In && badProgress.Mx == 1D && badProgress.V == 0D && badProgress.R == 0D, "malformed progress uses bounded defaults");
                var clampedProgress = BlockById(gaugePage, "clamped-progress").Vm;
                Ok(clampedProgress.V == 100D && clampedProgress.R == 1D, "progress value clamps to maximum");
                var extremeProgress = BlockById(gaugePage, "extreme-progress").Vm;
                Ok(extremeProgress.Mx == 1000000000000D && extremeProgress.V == 1000000000000D && extremeProgress.R == 1D, "extreme progress numbers clamp to finite ceiling");

                var bestMeter = BlockById(gaugePage, "best-meter").Vm;
                var suboptimalMeter = BlockById(gaugePage, "suboptimal-meter").Vm;
                var worstMeter = BlockById(gaugePage, "worst-meter").Vm;
                Ok(bestMeter.Mn == 0D && bestMeter.Mx == 100D && bestMeter.Lo == 25D && bestMeter.Hi == 75D && bestMeter.Op == 10D, "meter boundary aliases");
                Eq(0, bestMeter.Q, "optimum meter quality");
                Eq(1, suboptimalMeter.Q, "suboptimal meter quality");
                Eq(2, worstMeter.Q, "least-good meter quality");
                var reversedMeter = BlockById(gaugePage, "reversed-meter").Vm;
                Ok(reversedMeter.Mn == 10D && reversedMeter.Mx == 10D && reversedMeter.V == 10D && reversedMeter.Lo == 10D && reversedMeter.Hi == 10D && reversedMeter.Op == 10D && reversedMeter.R == 0D, "reversed meter range collapses safely");
                var badMeter = BlockById(gaugePage, "bad-meter").Vm;
                Ok(badMeter.Mn == 0D && badMeter.Mx == 1D && badMeter.V == 0D && badMeter.Lo == 0D && badMeter.Hi == 1D && badMeter.Op == 0.5D, "malformed meter uses bounded defaults");
                var cellMeter = BlockById(gaugePage, "cell-meter");
                Ok(cellMeter.K == "meter" && cellMeter.Vm != null && cellMeter.Vm.R == 0.75D, "table cell meter model");

                var valuePath = Path.Combine(pages, "values.html");
                var valueEngine = new E();
                var valuePage = valueEngine.Go(valuePath);
                Eq(7, valuePage.Dvs.Count, "bounded page value collection");
                var totalValue = ValueById(valuePage, "total-output");
                Eq("o", totalValue.K, "output value kind alias");
                Eq("42 points", totalValue.V, "output current value alias");
                Eq("42 points", totalValue.X, "output visible text alias");
                Eq("total", totalValue.N, "output name alias");
                Eq("calc", totalValue.Fo, "output form owner alias");
                Eq(2, totalValue.F.Count, "output for references deduplicate");
                Eq("price", totalValue.F[0], "first output reference");
                Eq("qty", totalValue.F[1], "second output reference");
                var totalBlock = BlockById(valuePage, "total-output");
                Ok(Object.ReferenceEquals(totalValue, totalBlock.Dv) && Object.ReferenceEquals(totalValue.El, totalBlock.El), "standalone output block ownership");
                var boldOutput = false;
                foreach (var run in totalBlock.Is)
                    if (Object.ReferenceEquals(run.Dv, totalValue) && run.S.B && run.El != null && run.El.K == "strong") boldOutput = true;
                Ok(boldOutput, "output keeps nested computed inline style and ownership");
                Eq("#176b4d", totalBlock.S.F, "output computed foreground style");

                var inlineValue = ValueById(valuePage, "inline-data");
                Eq("d", inlineValue.K, "data value kind alias");
                Eq("lea-97", inlineValue.V, "data machine value alias");
                Eq("Lea compact", inlineValue.X, "data visible text alias");
                var inlineRun = RunByValue(BlockById(valuePage, "inline-line"), inlineValue);
                Ok(inlineRun != null && Object.ReferenceEquals(inlineRun.Dv, inlineValue), "inline data run ownership");
                var cellValue = ValueById(valuePage, "cell-data");
                var cellValueBlock = BlockById(valuePage, "cell-data");
                Ok(Object.ReferenceEquals(cellValueBlock.Dv, cellValue) && cellValueBlock.K == "data", "table cell data value model");
                var cellValueRun = RunByValue(cellValueBlock, cellValue);
                Ok(cellValueRun != null && cellValueRun.S.I && cellValueRun.El != null, "table cell data keeps inline style and ownership");
                Eq(32, ValueById(valuePage, "many-refs").F.Count, "output references cap at 32");
                Eq("r32", ValueById(valuePage, "many-refs").F[31], "output reference cap keeps order");
                Eq("", ValueById(valuePage, "empty-output").V, "empty output remains host-readable");

                Eq(1, valueEngine.Ck(valuePage, ById(valuePage.D, "update-output")), "safe output update command");
                var updatedOutput = ValueById(valuePage, "live-output");
                Eq("Updated total", updatedOutput.V, "output model rebuilds after text update");
                Ok(Object.ReferenceEquals(updatedOutput, BlockById(valuePage, "live-output").Dv), "rebuilt output block keeps value ownership");
                Eq(1, valueEngine.Ck(valuePage, ById(valuePage.D, "update-data")), "safe data text update command");
                var updatedData = ValueById(valuePage, "live-data");
                Eq("sku-19", updatedData.V, "data machine value survives text update");
                Eq("Updated label", updatedData.X, "data visible text rebuilds after update");
                Eq(1, valueEngine.Ck(valuePage, ById(valuePage.D, "style-data")), "safe data class update command");
                Eq("#b91c1c", BlockById(valuePage, "live-data").S.F, "data computed style rebuilds after class update");
                Eq(7, valuePage.Dvs.Count, "value rebuild does not duplicate page models");

                var boundedValuePath = Path.Combine(Path.GetTempPath(), "lea-values-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var longMachine = new string('v', 4200);
                    var longUpdate = new string('u', 600);
                    File.WriteAllText(boundedValuePath, "<!doctype html><html><body><data id='bound-data' value='" + longMachine + "'>label</data><output id='bound-output'>initial</output><button id='bound-update' data-lea=\"tx('bound-output','" + longUpdate + "')\">Update</button></body></html>", new UTF8Encoding(false));
                    var valueBoundEngine = new E();
                    var valueBoundPage = valueBoundEngine.Go(boundedValuePath);
                    Eq(4096, ValueById(valueBoundPage, "bound-data").V.Length, "data machine value cap");
                    Eq(1, valueBoundEngine.Ck(valueBoundPage, ById(valueBoundPage.D, "bound-update")), "bounded output update command");
                    Eq(512, ValueById(valueBoundPage, "bound-output").V.Length, "safe output update cap");
                }
                finally
                {
                    if (File.Exists(boundedValuePath)) File.Delete(boundedValuePath);
                }

                var timePath = Path.Combine(pages, "time-edits.html");
                var timeEngine = new E();
                var timePage = timeEngine.Go(timePath);
                Eq(10, timePage.Dvs.Count, "bounded time and edit collection");
                var releaseTime = ValueById(timePage, "release-time");
                Eq("t", releaseTime.K, "time value kind alias");
                Eq("2026-08-21T19:00-04:00", releaseTime.V, "time machine value alias");
                Eq("August 21, 2026", releaseTime.X, "time visible text alias");
                Eq("2026-08-21T19:00-04:00", releaseTime.Dt, "time datetime alias");
                Ok(releaseTime.U == null && Object.ReferenceEquals(releaseTime, BlockById(timePage, "release-time").Dv), "standalone time ownership");

                var inlineTime = ValueById(timePage, "inline-time");
                Eq("2026-08-21", inlineTime.V, "text time machine value");
                Eq("2026-08-21", inlineTime.Dt, "text time datetime value");
                Ok(RunByValue(BlockById(timePage, "inline-semantic"), inlineTime) != null, "inline time run ownership");
                var inlineIns = ValueById(timePage, "inline-ins");
                Eq("i", inlineIns.K, "inserted value kind alias");
                Eq("2026-08-21T19:05-04:00", inlineIns.Dt, "inserted datetime alias");
                Eq(new Uri(timePage.Bu, "go-target.html#change"), inlineIns.U, "resolved inserted citation alias");
                var insertedRun = RunByValue(BlockById(timePage, "inline-semantic"), inlineIns);
                Ok(insertedRun != null && insertedRun.S.Td.IndexOf('u') >= 0, "inserted text default underline and ownership");
                var insertedBold = false;
                foreach (var run in BlockById(timePage, "inline-semantic").Is)
                    if (Object.ReferenceEquals(run.Dv, inlineIns) && run.S.B) insertedBold = true;
                Ok(insertedBold, "inserted text keeps nested computed style");
                var inlineDel = ValueById(timePage, "inline-del");
                Eq("r", inlineDel.K, "removed value kind alias");
                Ok(inlineDel.U == null, "unsafe removed citation rejected");
                var removedRun = RunByValue(BlockById(timePage, "inline-semantic"), inlineDel);
                Ok(removedRun != null && removedRun.S.Td.IndexOf('s') >= 0, "removed text default strike and ownership");

                var addedValue = ValueById(timePage, "added-block");
                var addedBlock = BlockById(timePage, "added-block");
                Ok(Object.ReferenceEquals(addedValue, addedBlock.Dv) && addedValue.U == new Uri(timePage.Bu, "go-target.html#added"), "standalone inserted metadata ownership");
                var addedItalic = false;
                foreach (var run in addedBlock.Is)
                    if (Object.ReferenceEquals(run.Dv, addedValue) && run.S.I) addedItalic = true;
                Ok(addedItalic, "standalone inserted text keeps nested style");
                var cellTime = ValueById(timePage, "cell-time");
                var cellRemoved = ValueById(timePage, "cell-del");
                Ok(Object.ReferenceEquals(BlockById(timePage, "cell-time").Dv, cellTime), "table time metadata ownership");
                var cellRemovedRun = RunByValue(BlockById(timePage, "cell-del"), cellRemoved);
                Ok(cellRemovedRun != null && cellRemovedRun.S.I && cellRemovedRun.S.Td.IndexOf('s') >= 0, "table removed text keeps nested style and ownership");

                Eq(1, timeEngine.Ck(timePage, ById(timePage.D, "update-live")), "safe text time update command");
                var liveTime = ValueById(timePage, "live-time");
                Ok(liveTime.V == "2026-08-22" && liveTime.Dt == "2026-08-22" && liveTime.X == "2026-08-22", "text time metadata rebuild");
                Eq(1, timeEngine.Ck(timePage, ById(timePage.D, "update-fixed")), "safe authored time label update command");
                var fixedTime = ValueById(timePage, "fixed-time");
                Ok(fixedTime.V == "2026-08-21" && fixedTime.Dt == "2026-08-21" && fixedTime.X == "Updated date label", "authored datetime survives label rebuild");
                Eq(1, timeEngine.Ck(timePage, ById(timePage.D, "style-added")), "safe inserted class update command");
                Eq("#b91c1c", BlockById(timePage, "added-block").S.F, "inserted computed style rebuilds after class update");
                Eq(10, timePage.Dvs.Count, "time and edit rebuild does not duplicate models");

                var boundedTimePath = Path.Combine(Path.GetTempPath(), "lea-time-edits-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var longDate = new string('d', 200);
                    var longCitation = new string('c', 4200);
                    File.WriteAllText(boundedTimePath, "<!doctype html><html><body><time id='bound-time' datetime='" + longDate + "'>label</time><ins id='bound-edit' cite='" + longCitation + "' datetime='" + longDate + "'>edit</ins></body></html>", new UTF8Encoding(false));
                    var boundedTimePage = new E().Go(boundedTimePath);
                    Eq(128, ValueById(boundedTimePage, "bound-time").Dt.Length, "time datetime cap");
                    Eq(128, ValueById(boundedTimePage, "bound-edit").Dt.Length, "edit datetime cap");
                    Ok(ValueById(boundedTimePage, "bound-edit").U == null, "oversized edit citation rejected");
                }
                finally
                {
                    if (File.Exists(boundedTimePath)) File.Delete(boundedTimePath);
                }

                var tokenPath = Path.Combine(pages, "html-tokens.html");
                var tokenEngine = new E();
                var tokenPage = tokenEngine.Go(tokenPath);
                Eq("Lea & Tokenizer <b>literal</b>", tokenPage.T, "RCDATA title keeps literal markup");
                Eq("1 > 0", ById(tokenPage.D, "entity").Av("data-note"), "quoted greater-than attribute");
                Eq("A < B", ById(tokenPage.D, "entity").Av("title"), "quoted less-than attribute");
                Eq("A & B # L <safe> &leaUnknown;", ById(tokenPage.D, "entity").C[0].X, "named and numeric text entities");
                Eq("2 < 3 and 5 > 4", NodeText(ById(tokenPage.D, "math")), "malformed delimiter stays text");
                Eq(1, tokenPage.Fs.Count, "RCDATA textarea field count");
                Eq("Line & <b>literal</b>", tokenPage.Fs[0].V, "RCDATA textarea keeps literal markup");
                Eq("<strong>raw</strong>", tokenPage.Vs["markup"], "script raw text keeps tag-shaped strings");
                Eq("&amp;", tokenPage.Vs["entity"], "script raw text does not decode entities");
                Eq("<strong>raw</strong> &amp;", ById(tokenPage.D, "script-state").C[0].X, "raw script commands execute intact");
                Eq("entity", tokenEngine.Q(tokenPage, "[data-note=\"1 > 0\"]").Av("id"), "query quoted delimiter attribute");
                Eq(0, Nodes(tokenPage.D, "b").Count, "RCDATA markup does not create elements");
                Eq(0, Nodes(tokenPage.D, "strong").Count, "script markup does not create elements");
                Eq(0, Nodes(tokenPage.D, "not-a-tag").Count, "style markup does not create elements");
                Ok(ById(tokenPage.D, "ghost") == null && tokenPage.X.IndexOf("hidden marker", StringComparison.Ordinal) < 0, "comments stay outside the document tree");
                Eq(2, tokenEngine.Dg(tokenPage).C, "raw style rules compile");

                var boundedTreePath = Path.Combine(Path.GetTempPath(), "lea-tree-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    var boundedHtml = new StringBuilder("<!doctype html><html><head><base href='");
                    boundedHtml.Append(new String('x', 4097)).Append("'><base href='../'></head><body><p id='attrs'");
                    for (var attribute = 0; attribute < 200; attribute++) boundedHtml.Append(" a").Append(attribute).Append("='v'");
                    boundedHtml.Append(">bounded</p>");
                    for (var depth = 0; depth < 200; depth++) boundedHtml.Append("<div>");
                    boundedHtml.Append("deep");
                    for (var depth = 0; depth < 200; depth++) boundedHtml.Append("</div>");
                    for (var node = 0; node < 9000; node++) boundedHtml.Append("<i>x</i>");
                    boundedHtml.Append("</body></html>");
                    File.WriteAllText(boundedTreePath, boundedHtml.ToString());
                    var treeEngine = new E();
                    var boundedTree = treeEngine.Go(boundedTreePath);
                    Eq(8192, TreeNodes(boundedTree.D), "bounded HTML node count");
                    Ok(boundedTree.N < 8192, "bounded HTML element count");
                    Eq(128, ById(boundedTree.D, "attrs").A.Count, "bounded HTML attribute count");
                    Ok(NodeDepth(boundedTree.D) <= 129, "bounded HTML tree depth");
                    Eq(256, treeEngine.Qa(boundedTree, "i").Count, "bounded query result count");
                    Eq(new Uri(new Uri(boundedTreePath), "../"), boundedTree.Bu, "oversized base URL skipped");
                }
                finally { File.Delete(boundedTreePath); }

                Console.WriteLine("Lea parser and model smoke passed; starting WinForms renderer checks.");
                using (var view = new V())
                {
                    view.Size = new Size(720, 760);
                    view.CreateControl();
                    var alertEvents = 0;
                    view.Al += delegate { alertEvents++; };
                    view.Go(scriptPath);
                    Paint(view);
                    Eq(2, alertEvents, "WinForms alert events");
                    Eq("Second alert", view.Am, "WinForms latest alert");
                    Eq("#f0fff8", view.At.S.Bg, "WinForms scripted background");
                    using (var scriptPaint = Render(view))
                        Ok(ColorDistance(scriptPaint.GetPixel(2, 2), Color.FromArgb(240, 255, 248)) < 4, "scripted page background painting");
                    Eq(3, view.Js("t('View Script'); bg('#dff7ed'); a('View alert');"), "WinForms manual commands");
                    Eq("View Script", view.T, "WinForms manual title");
                    Eq(3, alertEvents, "WinForms manual alert event");
                    Eq("View alert", view.Am, "WinForms manual alert message");

                    view.Go(domPath);
                    Paint(view);
                    Eq(11, view.At.Jc, "WinForms DOM page commands");
                    Ok(view.Ke != null && view.Ke.Av("id") == "name", "WinForms DOM focus call");
                    Eq(2, view.Js("document.getElementById('status').textContent='WinForms DOM'; document.getElementById('status').style.color='#0f766e';"), "WinForms DOM host commands");
                    Eq("WinForms DOM", ById(view.At.D, "status").C[0].X, "WinForms DOM text update");
                    Eq("#0f766e", BlockById(view.At, "status").S.F, "WinForms DOM style update");

                    view.Je = false;
                    Eq(0, view.Js("t('Blocked view script');"), "WinForms script setting");
                    Eq("Lea DOM bridge", view.T, "disabled WinForms script leaves title");
                    view.Je = true;

                    view.Go(displayPath);
                    Paint(view);
                    Ok(!HasBlock(view.At, "panel"), "WinForms authored display none suppresses layout");
                    Eq(1, view.Ck(ById(view.At.D, "show")), "WinForms compact display command");
                    Paint(view);
                    Ok(HasBlock(view.At, "panel") && !BlockRect(view, BlockById(view.At, "panel")).IsEmpty, "WinForms display command restores layout");
                    Eq(1, view.Ck(ById(view.At.D, "hide")), "WinForms DOM display none command");
                    Paint(view);
                    Ok(!HasBlock(view.At, "panel"), "WinForms DOM display none removes layout");
                    Eq(1, view.Ck(ById(view.At.D, "flex")), "WinForms DOM flex display command");
                    Paint(view);
                    var flexOne = BlockRect(view, BlockById(view.At, "one"));
                    var flexTwo = BlockRect(view, BlockById(view.At, "two"));
                    Eq(flexOne.Top, flexTwo.Top, "WinForms DOM flex display reflows children horizontally");
                    Ok(flexTwo.Left > flexOne.Right, "WinForms DOM flex display preserves gap");
                    Eq(1, view.Ck(ById(view.At.D, "clear")), "WinForms empty display reset command");
                    Paint(view);
                    var blockOne = BlockRect(view, BlockById(view.At, "one"));
                    var blockTwo = BlockRect(view, BlockById(view.At, "two"));
                    Ok(blockTwo.Top > blockOne.Bottom, "WinForms display reset restores vertical flow");
                    var displayGoEvents = 0;
                    Uri displayGoTarget = null;
                    EventHandler displayNavigation = delegate { displayGoEvents++; displayGoTarget = view.Gu; };
                    view.Gr += displayNavigation;
                    Eq(1, view.Ck(ById(view.At.D, "href")), "WinForms familiar location command");
                    Eq(1, displayGoEvents, "WinForms familiar location event");
                    Eq(new Uri(new Uri(Path.Combine(pages, "go-target.html")).AbsoluteUri + "#href"), displayGoTarget, "WinForms familiar location URL");
                    view.Gr -= displayNavigation;

                    view.Size = new Size(760, 1200);
                    view.Go(calculatedPath);
                    Paint(view);
                    var calculatedOuterRect = BlockRect(view, BlockById(view.At, "outer"));
                    Eq(view.ClientSize.Width - 100, calculatedOuterRect.Width, "WinForms calculated parent width");
                    Eq(calculatedOuterRect.Width / 2, BlockRect(view, BlockById(view.At, "half")).Width, "WinForms percentage child width");
                    Eq(calculatedOuterRect.Width - 80, BlockRect(view, BlockById(view.At, "minus")).Width, "WinForms negative calculated child width");
                    Eq(calculatedOuterRect.Width / 2 + 20, BlockRect(view, BlockById(view.At, "plus")).Width, "WinForms positive calculated child width");
                    Eq(calculatedOuterRect.Width / 2 - 10, BlockRect(view, BlockById(view.At, "minimum")).Width, "WinForms calculated minimum width");
                    Eq(calculatedOuterRect.Width * 75 / 100 + 5, BlockRect(view, BlockById(view.At, "maximum")).Width, "WinForms calculated maximum width");
                    Eq(240, BlockRect(view, BlockById(view.At, "invalid")).Width, "WinForms invalid calculated width fallback");
                    Eq((int)Math.Round(calculatedOuterRect.Width * .75D) - 15, BlockRect(view, BlockById(view.At, "variable")).Width, "WinForms custom property calculated width");
                    var inheritedParentRect = BlockRect(view, BlockById(view.At, "inherit-parent"));
                    Eq((int)Math.Round(inheritedParentRect.Width * .6D), BlockRect(view, BlockById(view.At, "inherit-child")).Width, "WinForms explicitly inherited percentage width");
                    var flexRect = BlockRect(view, BlockById(view.At, "flex"));
                    Eq(flexRect.Width / 2 - 10, BlockRect(view, BlockById(view.At, "flex-a")).Width, "WinForms calculated flex item width");
                    Eq(flexRect.Width / 2, BlockRect(view, BlockById(view.At, "flex-b")).Width, "WinForms percentage flex item width");
                    var gridRect = BlockRect(view, BlockById(view.At, "grid"));
                    var gridSlotWidth = (gridRect.Width - 10) / 2;
                    Ok(Math.Abs(BlockRect(view, BlockById(view.At, "grid-a")).Width - gridSlotWidth / 2) <= 1, "WinForms percentage grid item width");
                    Eq(calculatedOuterRect.Width - 40, BlockRect(view, BlockById(view.At, "sized-table")).Width, "WinForms calculated table width");
                    var flexTableWrapRect = BlockRect(view, BlockById(view.At, "flex-table-wrap"));
                    Eq(flexTableWrapRect.Width / 2, BlockRect(view, BlockById(view.At, "flex-table")).Width, "WinForms percentage table flex item width");
                    var cellTable = BlockById(view.At, "cell-table");
                    var cellHalfRect = BlockRect(view, cellTable.Tb.Rs[0].Cs[0].Bs[0]);
                    var cellContentRect = ContainingCell(view, cellTable, cellHalfRect);
                    Eq((int)Math.Round(cellContentRect.Width * .5D), cellHalfRect.Width, "WinForms percentage table-cell content width");
                    Eq(400, BlockRect(view, BlockById(view.At, "ratio-max")).Width, "WinForms aspect-ratio width uses original maximum reference");
                    Eq(1, view.Ck(ById(view.At.D, "set-width")), "WinForms compact calculated width command");
                    Paint(view);
                    Eq(calculatedOuterRect.Width - 120, BlockRect(view, BlockById(view.At, "command")).Width, "WinForms compact width command reflow");
                    Eq(1, view.Ck(ById(view.At.D, "dom-width")), "WinForms literal DOM width command");
                    Paint(view);
                    Eq(calculatedOuterRect.Width / 2, BlockRect(view, BlockById(view.At, "command")).Width, "WinForms literal DOM width reflow");
                    Eq(1, view.Ck(ById(view.At.D, "clear-width")), "WinForms empty width reset command");
                    Paint(view);
                    Eq(280, BlockRect(view, BlockById(view.At, "command")).Width, "WinForms width reset restores authored pixels");
                    view.Size = new Size(560, 1200);
                    Paint(view);
                    var narrowCalculatedOuter = BlockRect(view, BlockById(view.At, "outer"));
                    Eq(view.ClientSize.Width - 100, narrowCalculatedOuter.Width, "WinForms calculated parent responds to resize");
                    Eq(narrowCalculatedOuter.Width / 2, BlockRect(view, BlockById(view.At, "half")).Width, "WinForms percentage child responds to resize");
                    Eq(narrowCalculatedOuter.Width - 80, BlockRect(view, BlockById(view.At, "minus")).Width, "WinForms calculated child responds to resize");

                    view.Size = new Size(760, 1000);
                    view.Go(calculatedHeightPath);
                    Paint(view);
                    var rootHeightReference = view.ClientSize.Height - 18;
                    var viewportHeight = (int)Math.Round(rootHeightReference * .5D);
                    Eq(viewportHeight, BlockRect(view, BlockById(view.At, "viewport")).Height, "WinForms viewport percentage height");
                    Eq((int)Math.Round(viewportHeight * .5D), BlockRect(view, BlockById(view.At, "viewport-child")).Height, "WinForms nested percentage height");
                    var calculatedHeightOuter = BlockRect(view, BlockById(view.At, "outer"));
                    Eq(400, calculatedHeightOuter.Height, "WinForms fixed containing height");
                    Eq(200, BlockRect(view, BlockById(view.At, "half")).Height, "WinForms percentage child height");
                    Eq(320, BlockRect(view, BlockById(view.At, "minus")).Height, "WinForms negative calculated child height");
                    Eq(220, BlockRect(view, BlockById(view.At, "plus")).Height, "WinForms positive calculated child height");
                    Eq(190, BlockRect(view, BlockById(view.At, "minimum")).Height, "WinForms calculated minimum height");
                    Eq(305, BlockRect(view, BlockById(view.At, "maximum")).Height, "WinForms calculated maximum height");
                    Eq(140, BlockRect(view, BlockById(view.At, "invalid")).Height, "WinForms invalid calculated height fallback");
                    Eq(285, BlockRect(view, BlockById(view.At, "variable")).Height, "WinForms custom property calculated height");
                    Eq(240, BlockRect(view, BlockById(view.At, "inherit-parent")).Height, "WinForms inherited-height parent percentage");
                    Eq(144, BlockRect(view, BlockById(view.At, "inherit-child")).Height, "WinForms explicitly inherited percentage height");
                    Ok(BlockRect(view, BlockById(view.At, "auto-child")).Height < 100, "unresolved percentage height retains natural auto size");
                    Eq(120, BlockRect(view, BlockById(view.At, "flex-row-a")).Height, "WinForms percentage row-flex item height");
                    Eq(210, BlockRect(view, BlockById(view.At, "flex-row-b")).Height, "WinForms calculated row-flex item height");
                    Eq(75, BlockRect(view, BlockById(view.At, "flex-column-a")).Height, "WinForms percentage column-flex item height");
                    Eq(140, BlockRect(view, BlockById(view.At, "flex-column-b")).Height, "WinForms calculated column-flex item height");
                    var calculatedGrid = BlockRect(view, BlockById(view.At, "grid"));
                    var calculatedGridSlotHeight = (calculatedGrid.Height - 10) / 2;
                    Eq((int)Math.Round(calculatedGridSlotHeight * .5D), BlockRect(view, BlockById(view.At, "grid-a")).Height, "WinForms percentage grid-area height");
                    Eq(180, BlockRect(view, BlockById(view.At, "sized-table")).Height, "WinForms calculated table height");
                    Eq(400, BlockRect(view, BlockById(view.At, "ratio")).Width, "WinForms aspect-ratio width uses calculated height");
                    Eq(150, BlockRect(view, BlockById(view.At, "absolute-height")).Height, "WinForms absolute percentage containing height");
                    Eq(250, BlockRect(view, BlockById(view.At, "fixed-height")).Height, "WinForms fixed percentage viewport height");
                    Eq(100, BlockRect(view, BlockById(view.At, "height-field")).Height, "WinForms native field percentage height");
                    Eq(100, BlockRect(view, BlockById(view.At, "height-image")).Height, "WinForms image percentage height");
                    view.Sc(BlockRect(view, BlockById(view.At, "height-field")).Top - 24);
                    Paint(view);
                    Eq(100, view.Ab(view.Ax(ById(view.At.D, "height-field"))).Height, "WinForms percentage field accessibility height");
                    Eq(100, view.Ab(view.Ax(ById(view.At.D, "height-image"))).Height, "WinForms percentage image accessibility height");
                    Eq(220, BlockOuterRect(view, BlockById(view.At, "border-box")).Height, "WinForms calculated border-box height");
                    Eq(196, BlockRect(view, BlockById(view.At, "border-box")).Height, "WinForms calculated border-box content height");
                    Eq(1, view.Ck(ById(view.At.D, "set-height")), "WinForms compact calculated height command");
                    Paint(view);
                    Eq(280, BlockRect(view, BlockById(view.At, "command")).Height, "WinForms compact height command reflow");
                    Eq(1, view.Ck(ById(view.At.D, "dom-height")), "WinForms literal DOM height command");
                    Paint(view);
                    Eq(200, BlockRect(view, BlockById(view.At, "command")).Height, "WinForms literal DOM height reflow");
                    Eq(1, view.Ck(ById(view.At.D, "clear-height")), "WinForms empty height reset command");
                    Paint(view);
                    Eq(180, BlockRect(view, BlockById(view.At, "command")).Height, "WinForms height reset restores authored pixels");
                    view.Size = new Size(760, 800);
                    Paint(view);
                    var narrowRootHeightReference = view.ClientSize.Height - 18;
                    var narrowViewportHeight = (int)Math.Round(narrowRootHeightReference * .5D);
                    Eq(narrowViewportHeight, BlockRect(view, BlockById(view.At, "viewport")).Height, "WinForms viewport percentage height responds to resize");
                    Eq((int)Math.Round(narrowViewportHeight * .5D), BlockRect(view, BlockById(view.At, "viewport-child")).Height, "WinForms nested percentage height responds to resize");

                    view.Size = new Size(760, 900);
                    view.Go(logicalPath);
                    Paint(view);
                    var logicalShellRect = BlockRect(view, BlockById(view.At, "size-shell"));
                    Eq(new Size(400, 300), logicalShellRect.Size, "WinForms logical shell size");
                    Eq(new Size(220, 150), BlockRect(view, BlockById(view.At, "logical-size")).Size, "WinForms constrained logical size");
                    Eq(new Size(140, 70), BlockRect(view, BlockById(view.At, "order-physical")).Size, "WinForms physical source-order size");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "order-logical")).Size, "WinForms logical source-order size");
                    Eq(new Size(285, 75), BlockRect(view, BlockById(view.At, "variable")).Size, "WinForms logical custom-property size");
                    Eq(new Size(110, 80), BlockRect(view, BlockById(view.At, "important")).Size, "WinForms logical important size");
                    Eq(new Size(144, 48), BlockRect(view, BlockById(view.At, "inherit-child")).Size, "WinForms inherited logical percentage size");
                    var logicalPositionShell = BlockRect(view, BlockById(view.At, "position-shell"));
                    var logicalAxisRect = BlockRect(view, BlockById(view.At, "axis-position"));
                    Ok(logicalAxisRect.Left - logicalPositionShell.Left == 24 && logicalAxisRect.Top - logicalPositionShell.Top == 18 &&
                        logicalAxisRect.Size == new Size(346, 196), "WinForms logical axis inset stretch");
                    var logicalLonghandRect = BlockRect(view, BlockById(view.At, "longhand-position"));
                    Ok(logicalLonghandRect.Left - logicalPositionShell.Left == 32 && logicalPositionShell.Bottom - logicalLonghandRect.Bottom == 20 &&
                        logicalLonghandRect.Size == new Size(100, 50), "WinForms logical longhand placement");
                    var logicalInvalidRect = BlockRect(view, BlockById(view.At, "invalid-position"));
                    Ok(logicalInvalidRect.Left - logicalPositionShell.Left == 11 && logicalInvalidRect.Top - logicalPositionShell.Top == 19,
                        "WinForms atomic logical inset fallback");
                    Eq(2, view.Ck(ById(view.At.D, "set-logical")), "WinForms short logical size commands");
                    Paint(view);
                    Ok(BlockById(view.At, "command").S.Wp == 50 && BlockRect(view, BlockById(view.At, "command")).Height == 120,
                        "WinForms short logical command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "dom-logical")), "WinForms familiar logical DOM styles");
                    Paint(view);
                    Ok(BlockById(view.At, "command").S.Wp == 25 && BlockRect(view, BlockById(view.At, "command")).Height == 70,
                        "WinForms familiar logical DOM reflow");
                    Eq(2, view.Ck(ById(view.At.D, "clear-logical")), "WinForms logical DOM reset");
                    Paint(view);
                    Eq(new Size(210, 90), BlockRect(view, BlockById(view.At, "command")).Size, "WinForms logical DOM reset geometry");

                    view.Size = new Size(760, 900);
                    view.Go(logicalBoxPath);
                    Paint(view);
                    var logicalBoxShellRect = BlockRect(view, BlockById(view.At, "geometry-shell"));
                    var logicalBoxContentRect = BlockRect(view, BlockById(view.At, "axis-box"));
                    var logicalBoxOuterRect = BlockOuterRect(view, BlockById(view.At, "axis-box"));
                    Ok(logicalBoxContentRect.Size == new Size(100, 40) && logicalBoxOuterRect.Size == new Size(118, 53) &&
                        logicalBoxOuterRect.Left - logicalBoxShellRect.Left == 32 && logicalBoxOuterRect.Top - logicalBoxShellRect.Top == 44 &&
                        logicalBoxContentRect.Left - logicalBoxShellRect.Left == 40 && logicalBoxContentRect.Top - logicalBoxShellRect.Top == 50,
                        "WinForms logical margin and padding geometry");
                    Eq(4, view.Ck(ById(view.At.D, "set-axis")), "WinForms short logical box axes");
                    Paint(view);
                    var logicalCommandContent = BlockRect(view, BlockById(view.At, "command"));
                    var logicalCommandOuter = BlockOuterRect(view, BlockById(view.At, "command"));
                    Ok(logicalCommandContent.Size == new Size(120, 40) && logicalCommandOuter.Size == new Size(138, 53),
                        "WinForms short logical box axis reflow");
                    Eq(8, view.Ck(ById(view.At.D, "dom-edges")), "WinForms familiar logical box edges");
                    Paint(view);
                    logicalCommandOuter = BlockOuterRect(view, BlockById(view.At, "command"));
                    Eq(new Size(134, 62), logicalCommandOuter.Size, "WinForms familiar logical box edge reflow");
                    Eq(12, view.Ck(ById(view.At.D, "clear-box")), "WinForms logical box reset");
                    Paint(view);
                    logicalCommandOuter = BlockOuterRect(view, BlockById(view.At, "command"));
                    Eq(new Size(125, 43), logicalCommandOuter.Size, "WinForms authored logical box restore geometry");

                    view.Go(logicalBorderPath);
                    using (var logicalBorderPaint = Render(view))
                    {
                        var logicalBorderShell = BlockRect(view, BlockById(view.At, "geometry-shell"));
                        var logicalBorderAxisOuter = BlockOuterRect(view, BlockById(view.At, "axis"));
                        Ok(logicalBorderAxisOuter.Size == new Size(108, 52) && logicalBorderAxisOuter.Left - logicalBorderShell.Left == 20 &&
                            logicalBorderAxisOuter.Top - logicalBorderShell.Top == 30, "WinForms logical border axis geometry");
                        var hiddenBorderOuter = BlockOuterRect(view, BlockById(view.At, "hidden"));
                        Eq(new Size(109, 24), hiddenBorderOuter.Size, "WinForms hidden logical edge geometry");
                        var roundedBorderOuter = BlockOuterRect(view, BlockById(view.At, "rounded"));
                        Ok(ColorDistance(logicalBorderPaint.GetPixel(roundedBorderOuter.Left + 2, roundedBorderOuter.Top + roundedBorderOuter.Height / 2), Color.FromArgb(161, 59, 24)) < 20 &&
                            ColorDistance(logicalBorderPaint.GetPixel(roundedBorderOuter.Left + roundedBorderOuter.Width / 2, roundedBorderOuter.Bottom - 2), Color.FromArgb(183, 121, 31)) < 20,
                            "WinForms mixed rounded logical border paint");
                    }
                    Eq(6, view.Ck(ById(view.At.D, "set-short")), "WinForms short logical border shorthands");
                    Paint(view);
                    Eq(new Size(137, 61), BlockOuterRect(view, BlockById(view.At, "command")).Size, "WinForms short logical border geometry");
                    Eq(18, view.Ck(ById(view.At.D, "set-components")), "WinForms short logical border components");
                    Paint(view);
                    Eq(new Size(153, 77), BlockOuterRect(view, BlockById(view.At, "command")).Size, "WinForms logical border component geometry");
                    Eq(24, view.Ck(ById(view.At.D, "dom-all")), "WinForms familiar logical border DOM styles");
                    Paint(view);
                    Eq(new Size(167, 91), BlockOuterRect(view, BlockById(view.At, "command")).Size, "WinForms logical border DOM geometry");
                    Eq(24, view.Ck(ById(view.At.D, "clear-all")), "WinForms logical border DOM reset");
                    Paint(view);
                    Eq(new Size(128, 48), BlockOuterRect(view, BlockById(view.At, "command")).Size, "WinForms authored border restore geometry");

                    view.Size = new Size(800, 600);
                    view.Go(viewportPath);
                    Paint(view);
                    var viewportWidth = view.ClientSize.Width;
                    var viewportHeightUnits = view.ClientSize.Height;
                    var viewportMinimum = Math.Min(viewportWidth, viewportHeightUnits);
                    var viewportMaximum = Math.Max(viewportWidth, viewportHeightUnits);
                    Eq(viewportHeightUnits, view.Vh, "WinForms compact viewport height alias");
                    Eq((int)Math.Round(viewportWidth * .5D), BlockRect(view, BlockById(view.At, "vw")).Width, "WinForms vw width");
                    Eq((int)Math.Round(viewportHeightUnits * .25D), BlockRect(view, BlockById(view.At, "vh-width")).Width, "WinForms vh cross-axis width");
                    Eq((int)Math.Round(viewportMinimum * .1D), BlockRect(view, BlockById(view.At, "vmin-width")).Width, "WinForms vmin width");
                    Eq((int)Math.Round(viewportMaximum * .1D), BlockRect(view, BlockById(view.At, "vmax-width")).Width, "WinForms vmax width");
                    Eq((int)Math.Round(viewportWidth * .5D) - 20, BlockRect(view, BlockById(view.At, "calc-width")).Width, "WinForms calculated viewport width");
                    Eq((int)Math.Round(viewportHeightUnits * .5D), BlockRect(view, BlockById(view.At, "vh")).Height, "WinForms vh height");
                    Eq((int)Math.Round(viewportWidth * .25D), BlockRect(view, BlockById(view.At, "vw-height")).Height, "WinForms vw cross-axis height");
                    Eq((int)Math.Round(viewportMinimum * .1D), BlockRect(view, BlockById(view.At, "vmin-height")).Height, "WinForms vmin height");
                    Eq((int)Math.Round(viewportMaximum * .1D), BlockRect(view, BlockById(view.At, "vmax-height")).Height, "WinForms vmax height");
                    Eq((int)Math.Round(viewportHeightUnits * .5D) - 20, BlockRect(view, BlockById(view.At, "calc-height")).Height, "WinForms calculated viewport height");
                    Eq((int)Math.Round(viewportWidth * .25D), BlockRect(view, BlockById(view.At, "minimum-width")).Width, "WinForms viewport minimum width");
                    Eq((int)Math.Round(viewportWidth * .5D) - 5, BlockRect(view, BlockById(view.At, "maximum-width")).Width, "WinForms viewport maximum width");
                    Eq((int)Math.Round(viewportHeightUnits * .25D), BlockRect(view, BlockById(view.At, "minimum-height")).Height, "WinForms viewport minimum height");
                    Eq((int)Math.Round(viewportHeightUnits * .5D) - 5, BlockRect(view, BlockById(view.At, "maximum-height")).Height, "WinForms viewport maximum height");
                    Eq((int)Math.Round(viewportWidth * .5D), BlockRect(view, BlockById(view.At, "viewport-child")).Width, "WinForms viewport width ignores parent width");
                    Eq((int)Math.Round(viewportHeightUnits * .25D), BlockRect(view, BlockById(view.At, "viewport-child")).Height, "WinForms viewport height ignores parent height");
                    Eq(300, BlockRect(view, BlockById(view.At, "percent-child")).Width, "WinForms percentage width keeps parent reference");
                    Eq(100, BlockRect(view, BlockById(view.At, "percent-child")).Height, "WinForms percentage height keeps parent reference");
                    Eq((int)Math.Round(viewportHeightUnits * .25D), BlockRect(view, BlockById(view.At, "auto-viewport-child")).Height,
                        "WinForms viewport height resolves below auto parent");
                    Eq((int)Math.Round(viewportMinimum * .4D), BlockRect(view, BlockById(view.At, "inherit-child")).Width, "WinForms inherited vmin width");
                    Eq((int)Math.Round(viewportMaximum * .3D), BlockRect(view, BlockById(view.At, "inherit-child")).Height, "WinForms inherited vmax height");
                    Eq((int)Math.Round(viewportMaximum * .3D) + 10, BlockRect(view, BlockById(view.At, "variable")).Width, "WinForms custom-property vmax width");
                    Eq((int)Math.Round(viewportMinimum * .3D) - 10, BlockRect(view, BlockById(view.At, "variable")).Height, "WinForms custom-property vmin height");
                    Eq(new Size(140, 90), BlockRect(view, BlockById(view.At, "invalid")).Size, "WinForms invalid viewport fallback");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .25D)),
                        BlockRect(view, BlockById(view.At, "flex-a")).Size, "WinForms viewport flex item geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .25D)),
                        BlockRect(view, BlockById(view.At, "grid-a")).Size, "WinForms viewport grid item geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .5D), (int)Math.Round(viewportHeightUnits * .25D)),
                        BlockRect(view, BlockById(view.At, "viewport-table")).Size, "WinForms viewport table geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .25D)),
                        BlockRect(view, BlockById(view.At, "absolute")).Size, "WinForms positioned viewport geometry");
                    Eq(new Size((int)Math.Round(viewportMinimum * .1D), (int)Math.Round(viewportMaximum * .1D)),
                        BlockRect(view, BlockById(view.At, "fixed")).Size, "WinForms fixed vmin/vmax geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .1D)),
                        BlockRect(view, BlockById(view.At, "viewport-field")).Size, "WinForms viewport field geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .1D)),
                        BlockRect(view, BlockById(view.At, "viewport-image")).Size, "WinForms viewport image geometry");
                    Eq((int)Math.Round(viewportHeightUnits * .5D), BlockRect(view, BlockById(view.At, "ratio")).Width,
                        "WinForms aspect-ratio width derives from viewport height");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .25D)),
                        BlockOuterRect(view, BlockById(view.At, "border-box")).Size, "WinForms viewport border-box outer geometry");
                    Eq(new Size((int)Math.Round(viewportWidth * .25D) - 24, (int)Math.Round(viewportHeightUnits * .25D) - 24),
                        BlockRect(view, BlockById(view.At, "border-box")).Size, "WinForms viewport border-box content geometry");
                    view.Sc(BlockRect(view, BlockById(view.At, "viewport-link")).Top - 24);
                    Paint(view);
                    Eq(new Size((int)Math.Round(viewportWidth * .25D), (int)Math.Round(viewportHeightUnits * .1D)),
                        view.Ab(view.Ax(ById(view.At.D, "viewport-link"))).Size, "WinForms viewport accessibility geometry");
                    var viewportLinkHit = HitElementRect(view, "viewport-link");
                    Ok(!viewportLinkHit.IsEmpty, "WinForms viewport link retains a hit region");
                    Eq(BlockRect(view, BlockById(view.At, "viewport-link")).Size, viewportLinkHit.Size, "WinForms viewport link hit geometry");
                    Eq(2, view.Ck(ById(view.At.D, "set-viewport")), "WinForms compact viewport commands");
                    Paint(view);
                    Eq(new Size((int)Math.Round(viewportWidth * .5D) - 20, (int)Math.Round(viewportHeightUnits * .5D) - 20),
                        BlockRect(view, BlockById(view.At, "command")).Size, "WinForms compact viewport command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "dom-viewport")), "WinForms literal viewport DOM commands");
                    Paint(view);
                    Eq(new Size((int)Math.Round(viewportHeightUnits * .25D), (int)Math.Round(viewportWidth * .25D)),
                        BlockRect(view, BlockById(view.At, "command")).Size, "WinForms literal cross-axis viewport reflow");
                    Eq(2, view.Ck(ById(view.At.D, "clear-viewport")), "WinForms viewport command reset");
                    Paint(view);
                    Eq(new Size(200, 120), BlockRect(view, BlockById(view.At, "command")).Size, "WinForms viewport reset restores authored size");
                    view.Size = new Size(640, 400);
                    Paint(view);
                    Eq(320, BlockRect(view, BlockById(view.At, "vw")).Width, "WinForms vw responds to width resize");
                    Eq(100, BlockRect(view, BlockById(view.At, "vh-width")).Width, "WinForms vh width responds to height resize");
                    Eq(200, BlockRect(view, BlockById(view.At, "vh")).Height, "WinForms vh responds to height resize");
                    Eq(160, BlockRect(view, BlockById(view.At, "vw-height")).Height, "WinForms vw height responds to width resize");
                    Eq(new Size(40, 64), BlockRect(view, BlockById(view.At, "fixed")).Size, "WinForms vmin/vmax respond to resize");
                    view.Size = new Size(640, 500);
                    Paint(view);
                    Eq(500, view.Vh, "WinForms compact viewport height alias responds to resize");
                    Eq(250, BlockRect(view, BlockById(view.At, "vh")).Height, "WinForms vh responds to height-only resize");
                    Eq(320, BlockRect(view, BlockById(view.At, "vw")).Width, "WinForms vw stays stable on height-only resize");

                    view.Size = new Size(800, 600);
                    view.Go(fontUnitPath);
                    Paint(view);
                    Eq(20, view.At.Rf, "WinForms authored root font-size alias");
                    Eq(20, view.Rf, "WinForms compact root font-size alias");
                    Eq(new Size(180, 72), BlockRect(view, BlockById(view.At, "em-size")).Size, "WinForms em dimensions");
                    Eq(new Size(200, 80), BlockRect(view, BlockById(view.At, "rem-size")).Size, "WinForms rem dimensions");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "font-child")).Size, "WinForms nested em dimensions");
                    Eq(new Size(160, 110), BlockRect(view, BlockById(view.At, "calc-font")).Size, "WinForms calculated font-relative dimensions");
                    Eq(144, BlockRect(view, BlockById(view.At, "minimum-font-width")).Width, "WinForms em minimum width");
                    Eq(200, BlockRect(view, BlockById(view.At, "maximum-root-width")).Width, "WinForms rem maximum width");
                    Eq(72, BlockRect(view, BlockById(view.At, "minimum-font-height")).Height, "WinForms em minimum height");
                    Eq(100, BlockRect(view, BlockById(view.At, "maximum-root-height")).Height, "WinForms rem maximum height");
                    Eq(72, BlockRect(view, BlockById(view.At, "font-auto-child")).Height, "WinForms em height resolves below auto parent");
                    Eq(new Size(96, 80), BlockRect(view, BlockById(view.At, "font-inherit-child")).Size, "WinForms inherited font-relative dimensions");
                    Eq(new Size(162, 75), BlockRect(view, BlockById(view.At, "font-variable")).Size, "WinForms custom-property font-relative dimensions");
                    Eq(new Size(130, 85), BlockRect(view, BlockById(view.At, "font-invalid")).Size, "WinForms invalid font-relative fallback");
                    Eq(new Size(180, 54), BlockRect(view, BlockById(view.At, "invalid-font-size")).Size, "WinForms unsupported font-size unit stays atomic");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "font-flex-a")).Size, "WinForms font-relative flex geometry");
                    Eq(new Size(200, 60), BlockRect(view, BlockById(view.At, "font-grid-a")).Size, "WinForms font-relative grid geometry");
                    Eq(new Size(400, 108), BlockRect(view, BlockById(view.At, "font-table")).Size, "WinForms font-relative table geometry");
                    Eq(new Size(180, 80), BlockRect(view, BlockById(view.At, "font-absolute")).Size, "WinForms positioned font-relative geometry");
                    Eq(new Size(100, 90), BlockRect(view, BlockById(view.At, "font-fixed")).Size, "WinForms fixed font-relative geometry");
                    Eq(new Size(140, 56), BlockRect(view, BlockById(view.At, "font-field")).Size, "WinForms font-relative field geometry");
                    Eq(new Size(120, 100), BlockRect(view, BlockById(view.At, "font-image")).Size, "WinForms font-relative image geometry");
                    Eq(new Size(180, 90), BlockRect(view, BlockById(view.At, "font-ratio")).Size, "WinForms aspect ratio uses em height");
                    Eq(new Size(200, 90), BlockOuterRect(view, BlockById(view.At, "font-border-box")).Size, "WinForms font-relative border-box outer geometry");
                    Eq(new Size(176, 66), BlockRect(view, BlockById(view.At, "font-border-box")).Size, "WinForms font-relative border-box content geometry");
                    view.Sc(BlockRect(view, BlockById(view.At, "font-link")).Top - 24);
                    Paint(view);
                    Eq(new Size(180, 72), view.Ab(view.Ax(ById(view.At.D, "font-link"))).Size, "WinForms font-relative accessibility geometry");
                    Eq(new Size(180, 72), HitElementRect(view, "font-link").Size, "WinForms font-relative link hit geometry");
                    Eq(2, view.Ck(ById(view.At.D, "set-font-size")), "WinForms compact font-relative commands");
                    Paint(view);
                    Eq(new Size(140, 100), BlockRect(view, BlockById(view.At, "font-command")).Size, "WinForms compact font-relative command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "dom-font-size")), "WinForms literal font-relative DOM commands");
                    Paint(view);
                    Eq(new Size(160, 56), BlockRect(view, BlockById(view.At, "font-command")).Size, "WinForms literal font-relative command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "clear-font-size")), "WinForms font-relative command reset");
                    Paint(view);
                    Eq(new Size(200, 120), BlockRect(view, BlockById(view.At, "font-command")).Size, "WinForms font-relative reset restores authored size");
                    Eq(new Size(120, 48), BlockRect(view, BlockById(view.At, "font-mutable")).Size, "WinForms initial em geometry before font mutation");
                    Eq(1, view.Ck(ById(view.At.D, "grow-font")), "WinForms compact font mutation");
                    Paint(view);
                    Eq(new Size(200, 80), BlockRect(view, BlockById(view.At, "font-mutable")).Size, "WinForms em geometry follows font mutation");
                    var fontField = FieldById(view.At, "font-field");
                    fontField.V = "Retained font-relative value";
                    view.Size = new Size(640, 500);
                    Paint(view);
                    Eq(16, view.At.Rf, "WinForms responsive root font-size model");
                    Eq(16, view.Rf, "WinForms responsive root font-size alias");
                    Eq(new Size(160, 64), BlockRect(view, BlockById(view.At, "rem-size")).Size, "WinForms rem dimensions follow root media change");
                    Eq(new Size(180, 72), BlockRect(view, BlockById(view.At, "em-size")).Size, "WinForms em dimensions retain element font reference");
                    Eq(90, BlockRect(view, BlockById(view.At, "calc-font")).Height, "WinForms calculated rem height follows root media change");
                    Ok(Object.ReferenceEquals(fontField, FieldById(view.At, "font-field")) && fontField.V == "Retained font-relative value",
                        "WinForms root font rebuild retains mutable fields");

                    view.Size = new Size(800, 600);
                    view.Go(sizeFunctionPath);
                    Paint(view);
                    Eq(new Size(450, 260), BlockRect(view, BlockById(view.At, "min-two")).Size, "WinForms two-term minimum dimensions");
                    Eq(new Size(280, 140), BlockRect(view, BlockById(view.At, "max-two")).Size, "WinForms two-term maximum dimensions");
                    Eq(new Size(300, 180), BlockRect(view, BlockById(view.At, "clamp-main")).Size, "WinForms clamp dimensions");
                    Eq(new Size(360, 80), BlockRect(view, BlockById(view.At, "clamp-font")).Size, "WinForms font-relative clamp dimensions");
                    Eq(new Size(500, 80), BlockRect(view, BlockById(view.At, "min-three")).Size, "WinForms three-term minimum dimensions");
                    Eq(new Size(160, 80), BlockRect(view, BlockById(view.At, "max-three")).Size, "WinForms three-term maximum dimensions");
                    Eq(new Size(500, 150), BlockRect(view, BlockById(view.At, "calc-function")).Size, "WinForms calculated size function terms");
                    Eq(new Size(240, 60), BlockRect(view, BlockById(view.At, "zero-first")).Size, "WinForms zero-first explicit size function");
                    Eq(new Size(480, 300), BlockRect(view, BlockById(view.At, "function-limits")).Size, "WinForms function min/max constraints");
                    Eq(new Size(300, 120), BlockRect(view, BlockById(view.At, "function-inherit-child")).Size, "WinForms inherited size functions");
                    Eq(new Size(240, 72), BlockRect(view, BlockById(view.At, "function-variable")).Size, "WinForms custom-property size functions");
                    Eq(new Size(130, 85), BlockRect(view, BlockById(view.At, "function-invalid")).Size, "WinForms invalid size function fallback");
                    Ok(BlockRect(view, BlockById(view.At, "function-auto-child")).Height < 120, "WinForms percentage height function stays natural below auto parent");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "function-flex-a")).Size, "WinForms size function flex geometry");
                    Eq(new Size(200, 90), BlockRect(view, BlockById(view.At, "function-grid-a")).Size, "WinForms size function grid geometry");
                    Eq(new Size(500, 100), BlockRect(view, BlockById(view.At, "function-table")).Size, "WinForms size function table geometry");
                    Eq(new Size(180, 100), BlockRect(view, BlockById(view.At, "function-absolute")).Size, "WinForms positioned size function geometry");
                    Eq(new Size(200, 60), BlockRect(view, BlockById(view.At, "function-fixed")).Size, "WinForms fixed size function geometry");
                    Eq(new Size(140, 50), BlockRect(view, BlockById(view.At, "function-field")).Size, "WinForms size function field geometry");
                    Eq(new Size(144, 100), BlockRect(view, BlockById(view.At, "function-image")).Size, "WinForms size function image geometry");
                    Eq(new Size(144, 72), BlockRect(view, BlockById(view.At, "function-ratio")).Size, "WinForms aspect ratio uses function height");
                    Eq(new Size(200, 90), BlockOuterRect(view, BlockById(view.At, "function-border-box")).Size, "WinForms size function border-box outer geometry");
                    Eq(new Size(176, 66), BlockRect(view, BlockById(view.At, "function-border-box")).Size, "WinForms size function border-box content geometry");
                    view.Sc(BlockRect(view, BlockById(view.At, "function-link")).Top - 24);
                    Paint(view);
                    Eq(new Size(216, 60), view.Ab(view.Ax(ById(view.At.D, "function-link"))).Size, "WinForms size function accessibility geometry");
                    Eq(new Size(216, 60), HitElementRect(view, "function-link").Size, "WinForms size function link hit geometry");
                    Eq(2, view.Ck(ById(view.At.D, "set-function-size")), "WinForms compact size function commands");
                    Paint(view);
                    Eq(new Size(350, 70), BlockRect(view, BlockById(view.At, "function-command")).Size, "WinForms compact size function command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "dom-function-size")), "WinForms literal size function DOM commands");
                    Paint(view);
                    Eq(new Size(300, 84), BlockRect(view, BlockById(view.At, "function-command")).Size, "WinForms literal size function command reflow");
                    Eq(2, view.Ck(ById(view.At.D, "clear-function-size")), "WinForms size function command reset");
                    Paint(view);
                    Eq(new Size(220, 120), BlockRect(view, BlockById(view.At, "function-command")).Size, "WinForms size function reset restores authored size");
                    Eq(new Size(400, 60), BlockRect(view, BlockById(view.At, "live-function")).Size, "WinForms live size function geometry");
                    Eq(new Size(320, 80), BlockRect(view, BlockById(view.At, "media-function")).Size, "WinForms wide media size function geometry");
                    var functionField = FieldById(view.At, "function-field");
                    functionField.V = "Retained size function value";
                    view.Size = new Size(640, 500);
                    Paint(view);
                    Eq(16, view.Rf, "WinForms responsive root font for size functions");
                    Eq(new Size(320, 50), BlockRect(view, BlockById(view.At, "live-function")).Size, "WinForms size functions follow viewport and root-font resize");
                    Ok(BlockById(view.At, "media-function").S.Wk == 2 && BlockById(view.At, "media-function").S.Wq(0) == 2,
                        "WinForms media rebuild replaces size function state");
                    Eq(new Size(384, 80), BlockRect(view, BlockById(view.At, "media-function")).Size, "WinForms narrow media size function geometry");
                    Ok(Object.ReferenceEquals(functionField, FieldById(view.At, "function-field")) && functionField.V == "Retained size function value",
                        "WinForms size function resize retains mutable fields");

                    view.Size = new Size(720, 420);
                    view.Go(eventPath);
                    Paint(view);
                    var eventField = FieldById(view.At, "go");
                    var eventButton = BlockRect(view, FieldBlock(view.At, eventField));
                    Ok(!eventButton.IsEmpty, "safe event button layout");
                    Click(view, eventButton);
                    Eq("Clicked Lea", view.T, "WinForms click command title");
                    Eq("Hello Lea", ById(view.At.D, "status").C[0].X, "WinForms click text update");
                    Eq(4, alertEvents, "WinForms click alert event");
                    Eq("Safe click ran", view.Am, "WinForms click alert message");
                    var viewDiagnostics = view.Dg();
                    Eq("Clicked Lea", viewDiagnostics.T, "WinForms diagnostics title");
                    Ok(Has(viewDiagnostics.Rs, "events-vars.html"), "WinForms request diagnostics");
                    Paint(view);
                    var clickedEventPage = view.At;
                    var eventLink = HitRect(view, "second.html");
                    Ok(!eventLink.IsEmpty, "nested safe event link hit target");
                    Click(view, eventLink);
                    WaitFor(delegate { return !view.Busy && view.T == "LeaForge Second Page"; }, "safe event link navigation");
                    Eq("link", clickedEventPage.Vs["last"], "nested link click variable");
                    Eq(5, alertEvents, "nested link click alert event");

                    var goEvents = 0;
                    Uri viewGoTarget = null;
                    view.Gr += delegate { goEvents++; viewGoTarget = view.Gu; };
                    view.Go(goPath);
                    Paint(view);
                    var overrideLink = HitRect(view, "second.html");
                    Ok(!overrideLink.IsEmpty, "go override link hit target");
                    Click(view, overrideLink);
                    WaitFor(delegate { return goEvents == 1; }, "WinForms go request event");
                    Eq(goTarget, viewGoTarget, "WinForms go request URL");
                    Eq("Lea Go Command", view.T, "go request suppresses default link navigation");
                    view.Go(viewGoTarget);
                    Eq("Lea Go Target", view.T, "host-approved WinForms go navigation");

                    view.Go(classPath);
                    Paint(view);
                    var viewClassField = FieldById(view.At, "field");
                    viewClassField.V = "View-preserved value";
                    Eq(4, view.Ck(ById(view.At.D, "flip")), "WinForms class and scoped style click commands");
                    Paint(view);
                    Eq("panel", ById(view.At.D, "panel").Av("class"), "WinForms class remove");
                    Eq("field hot", ById(view.At.D, "field").Av("class"), "WinForms class add");
                    Eq("#dbeafe", BlockById(view.At, "panel").S.Bg, "WinForms scoped style restyle");
                    Eq("#166534", FieldById(view.At, "field").S.F, "WinForms class restyle");
                    Eq("View-preserved value", FieldById(view.At, "field").V, "WinForms class field preservation");
                    Eq(6, alertEvents, "WinForms class alert event");

                    view.Size = new Size(720, 700);
                    view.Go(statePath);
                    Paint(view);
                    Eq("name", view.Ke.Av("id"), "WinForms consumes initial focus command");
                    Eq("Page ready", FieldById(view.At, "name").V, "WinForms initial field command");
                    Ok(view.Ax(view.Id("panel")) == null, "WinForms hidden command semantics");
                    Eq(7, view.Ck(view.Id("apply")), "WinForms state click commands");
                    Paint(view);
                    Eq("name", view.Ke.Av("id"), "WinForms applies focus command");
                    Eq("Clicked Lea", FieldById(view.At, "name").V, "WinForms value command");
                    Ok(!BlockRect(view, BlockById(view.At, "panel")).IsEmpty, "WinForms show command layout");
                    Ok(!FieldById(view.At, "locked").Ds && FieldById(view.At, "choice-a").C, "WinForms enable and radio commands");
                    Eq(7, alertEvents, "WinForms state command alert event");
                    Eq("State commands ran", view.Am, "WinForms state command message");
                    Eq(1, view.Js("ds('name')"), "WinForms disable focused command");
                    Eq<N>(null, view.Ke, "WinForms clears disabled focus target");
                    Eq(2, view.Js("en('name'); fo('name')"), "WinForms restore focused command");
                    Eq("name", view.Ke.Av("id"), "WinForms refocuses enabled command target");
                    Eq(3, view.Ck(view.Id("hide-field")), "WinForms hidden field commands");
                    Paint(view);
                    Ok(view.Ax(view.Id("name")) == null, "WinForms hidden field leaves accessibility tree");
                    Eq(1, view.Ck(view.Id("show-field")), "WinForms show retained field command");
                    Paint(view);
                    Eq("Changed while hidden", FieldById(view.At, "name").V, "WinForms hidden field state retained");

                    view.Size = new Size(520, 260);
                    view.Go(findPath);
                    Paint(view);
                    Eq(64L * 1024 * 1024, view.Dm, "WinForms download maximum");
                    Eq(15000, view.Tm, "WinForms request timeout");
                    view.Tm = 1;
                    Eq(100, view.Tm, "WinForms request timeout forwarding");
                    view.Tm = 15000;
                    Eq(3, view.Fd("cobalt"), "WinForms painted find count");
                    Eq(1, view.Fx, "WinForms first find index");
                    Ok(!view.Fr.IsEmpty, "WinForms current find rectangle");
                    using (var findPaint = Render(view))
                    {
                        var current = view.Fr;
                        Ok(ColorDistance(findPaint.GetPixel(current.Right - 1, current.Bottom - 1), Color.FromArgb(194, 65, 12)) < 20, "active find highlight painting");
                    }
                    Eq(2, view.Nx(), "WinForms next find index");
                    Eq(3, view.Nx(), "WinForms wrapped find progression");
                    Ok(view.Sy > 0 && !view.Fr.IsEmpty, "WinForms find scrolls match into view");
                    Eq(2, view.Pr(), "WinForms previous find index");
                    Eq(0, view.Fd("not on this page"), "WinForms missing find count");
                    view.Fc();
                    Eq(0, view.Fn, "WinForms clear find");

                    var downloadRequests = 0;
                    view.Dr += delegate { downloadRequests++; };
                    view.Sc(100000);
                    Paint(view);
                    var downloadHit = HitRect(view, "download-sample.txt");
                    Ok(!downloadHit.IsEmpty, "download link hit target");
                    Click(view, downloadHit);
                    Eq(1, downloadRequests, "WinForms download request event");
                    Eq("lea-download-sample.txt", view.Dn, "download attribute suggested name");
                    Eq("download-sample.txt", Path.GetFileName(view.Du.LocalPath), "download request URL");
                    Eq("Lea Find and Download", view.T, "download request does not navigate");

                    view.Size = new Size(720, 760);
                    view.Go(forms);
                    view.Bk();
                    Eq(7, alertEvents, "history does not replay script alerts");
                    view.Fw();
                    Paint(view);
                    Ok(view.Bc > page.Fs.Count, "form layout boxes");
                    Eq("Open result", view.At.Bs[view.At.Bs.Count - 1].X, "button label");

                    view.At.Fs[0].V = "changed";
                    view.At.Fs[3].V = "light";
                    view.At.Fs[3].Si = 0;
                    view.At.Fs[4].C = false;
                    view.Rs(view.At.Fs[7]);
                    Eq("Lea", view.At.Fs[0].V, "reset text");
                    Eq("lea", view.At.Fs[3].V, "reset select");
                    Eq(1, view.At.Fs[3].Si, "reset selected index");
                    Ok(view.At.Fs[4].C, "reset checkbox");

                    view.At.Fs[0].V = "small engine";
                    view.Sm(view.At.Fs[8]);
                    Eq("Lea Form Result", view.T, "GET result");
                    Ok(view.Q.Contains("q=small+engine") && view.Q.Contains("theme=lea") && view.Q.Contains("updates=yes") && view.Q.Contains("size=tiny") && view.Q.Contains("mode=lea"), "GET values: " + view.Q + " uri=" + view.U.AbsoluteUri + " local=" + view.U.LocalPath);

                    view.Go(forms);
                    view.At.Fs[0].V = "async engine";
                    var submit = view.SmA(view.At.Fs[8]);
                    while (!submit.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    submit.GetAwaiter().GetResult();
                    Eq("Lea Form Result", view.T, "async GET result");
                    Ok(view.Q.Contains("q=async+engine"), "async GET values");

                    var submitEvents = 0;
                    view.Sb += delegate { submitEvents++; };
                    view.Go(validationPath);
                    Paint(view);
                    var guardedSubmit = view.At.Fs[9];
                    var guardedUrl = view.U.AbsoluteUri;
                    Ok(!view.Vf(guardedSubmit), "invalid guarded form");
                    Eq("required", view.At.Fs[0].Vm, "required validation message");
                    Eq("email", view.At.Fs[1].Vm, "email validation message");
                    Eq("url", view.At.Fs[2].Vm, "URL validation message");
                    Eq("min", view.At.Fs[3].Vm, "number minimum validation message");
                    Eq("min", view.At.Fs[4].Vm, "date minimum validation message");
                    Eq("required", view.At.Fs[5].Vm, "checkbox validation message");
                    Eq("required", view.At.Fs[6].Vm, "radio-group validation message");
                    Eq("required", view.At.Fs[8].Vm, "select validation message");
                    Eq("#fff1f2", view.At.Fs[0].S.Bg, "invalid pseudo background");
                    using (var invalidPaint = Render(view))
                    {
                        var invalidBox = BlockRect(view, FieldBlock(view.At, view.At.Fs[0]));
                        Ok(ColorDistance(BoxPixel(invalidPaint, invalidBox), Color.FromArgb(255, 241, 242)) < 4, "invalid field painting");
                    }
                    view.Sm(guardedSubmit);
                    Eq(guardedUrl, view.U.AbsoluteUri, "invalid submit blocks navigation");
                    Eq(0, submitEvents, "invalid submit event suppression");

                    view.At.Fs[0].V = "ab";
                    Ok(!view.Vf(guardedSubmit), "minimum length rejects short value");
                    Eq("short", view.At.Fs[0].Vm, "minimum length validation message");
                    view.At.Fs[0].V = "abcdefghijklmnop";
                    Ok(!view.Vf(guardedSubmit), "maximum length rejects long value");
                    Eq("long", view.At.Fs[0].Vm, "maximum length validation message");

                    view.At.Fs[0].V = "Lea search";
                    view.At.Fs[1].V = "lea@example.com";
                    view.At.Fs[2].V = "https://example.com";
                    view.At.Fs[3].V = "many";
                    view.At.Fs[4].V = "June";
                    view.At.Fs[5].C = true;
                    view.At.Fs[7].C = true;
                    view.At.Fs[8].V = "lea";
                    view.At.Fs[8].Si = 1;
                    Ok(!view.Vf(guardedSubmit), "typed values reject malformed input");
                    Eq("number", view.At.Fs[3].Vm, "number syntax validation message");
                    Eq("date", view.At.Fs[4].Vm, "date syntax validation message");
                    view.At.Fs[3].V = "21";
                    view.At.Fs[4].V = "2027-01-01";
                    Ok(!view.Vf(guardedSubmit), "maximum bounds reject high values");
                    Eq("max", view.At.Fs[3].Vm, "number maximum validation message");
                    Eq("max", view.At.Fs[4].Vm, "date maximum validation message");
                    view.At.Fs[0].V = "123";
                    view.At.Fs[3].V = "15";
                    view.At.Fs[4].V = "2026-06-15";
                    Ok(!view.Vf(guardedSubmit), "pattern rejects otherwise valid form");
                    Eq("pattern", view.At.Fs[0].Vm, "pattern validation message");
                    view.At.Fs[0].V = "Lea search";
                    Ok(view.Vf(guardedSubmit), "corrected guarded form");
                    Ok(view.At.Fs[0].Ok && view.At.Fs[0].Vm.Length == 0 && !view.At.Fs[0].El.A.ContainsKey("data-lea-invalid"), "validity marker clears");
                    view.Sm(guardedSubmit);
                    Eq("Lea Form Result", view.T, "validated GET result");
                    Ok(view.Q.Contains("q=Lea+search") && view.Q.Contains("email=lea%40example.com") && view.Q.Contains("count=15") && view.Q.Contains("terms=yes") && view.Q.Contains("mode=full") && view.Q.Contains("theme=lea"), "validated GET values: " + view.Q);
                    Eq(1, submitEvents, "valid submit event");

                    view.Go(validationPath);
                    var bypassSubmit = view.At.Fs[11];
                    Ok(bypassSubmit.Nv && view.At.Fs[10].V.Length == 0, "novalidate setup");
                    view.Sm(bypassSubmit);
                    Eq("Lea Form Result", view.T, "novalidate GET result");
                    Ok(view.Q.Contains("skip=") && view.Q.Contains("bypass=yes"), "novalidate GET values");
                    Eq(2, submitEvents, "novalidate submit event");

                    var uploadOne = Path.Combine(Path.GetTempPath(), "lea-widget-" + Guid.NewGuid().ToString("N") + "-one.txt");
                    var uploadTwo = Path.Combine(Path.GetTempPath(), "lea-widget-" + Guid.NewGuid().ToString("N") + "-two.txt");
                    File.WriteAllText(uploadOne, "Lea upload one", Encoding.UTF8);
                    File.WriteAllText(uploadTwo, "Lea upload two", Encoding.UTF8);
                    try
                    {
                        view.Go(widgetsPath);
                        Paint(view);
                        Eq(12, view.Bc, "richer widget layout boxes");
                        var widgetSubmit = view.At.Fs[9];
                        Ok(!view.Vf(widgetSubmit), "required file rejects empty selection");
                        Eq("required", view.At.Fs[0].Vm, "required file validation message");
                        Ok(view.Sf(view.At.Fs[0], uploadOne, uploadTwo), "bounded multiple file selection");
                        Eq(2, view.At.Fs[0].Fp.Count, "selected file path count");
                        Ok(view.At.Fs[0].V.Contains(Path.GetFileName(uploadOne)) && view.At.Fs[0].V.Contains(Path.GetFileName(uploadTwo)), "selected file display names");
                        Ok(view.Vf(widgetSubmit), "valid richer widget form");

                        view.At.Fs[1].V = "27";
                        view.At.Fs[2].V = "green";
                        view.At.Fs[3].V = "25:00";
                        view.At.Fs[4].V = "2026-13";
                        view.At.Fs[5].V = "2026-W99";
                        view.At.Fs[6].V = "not-local-time";
                        view.At.Fs[7].V = "lea@example.com,broken";
                        Ok(!view.Vf(widgetSubmit), "richer typed values reject malformed input");
                        Eq("step", view.At.Fs[1].Vm, "range step validation message");
                        Eq("color", view.At.Fs[2].Vm, "color validation message");
                        Eq("time", view.At.Fs[3].Vm, "time validation message");
                        Eq("month", view.At.Fs[4].Vm, "month validation message");
                        Eq("week", view.At.Fs[5].Vm, "week validation message");
                        Eq("datetime-local", view.At.Fs[6].Vm, "local datetime validation message");
                        Eq("email", view.At.Fs[7].Vm, "multiple email validation message");

                        view.Rs(view.At.Fs[8]);
                        Eq(0, view.At.Fs[0].Fp.Count, "reset clears selected files");
                        Eq("25", view.At.Fs[1].V, "reset restores range");
                        Eq("#176b4d", view.At.Fs[2].V, "reset restores color");
                        Paint(view);
                        var rangeBox = BlockRect(view, view.At.Bs[2]);
                        var rangeX = rangeBox.Left + 12 + (rangeBox.Width - 24) * 3 / 4;
                        Click(view, new Rectangle(rangeX - 1, rangeBox.Top + rangeBox.Height / 2 - 1, 2, 2));
                        Eq("70", view.At.Fs[1].V, "range click snaps to compact step");
                        using (var widgetPaint = Render(view))
                        {
                            var colorBox = BlockRect(view, view.At.Bs[3]);
                            Ok(ColorDistance(widgetPaint.GetPixel(colorBox.Left + 12, colorBox.Top + colorBox.Height / 2), Color.FromArgb(23, 107, 77)) < 4, "color swatch paint");
                        }
                    }
                    finally
                    {
                        File.Delete(uploadOne);
                        File.Delete(uploadTwo);
                    }

                    Check(view, root, "css.html", 3, 4);
                    Check(view, root, "images.html", 5, 6);
                    Check(view, root, "layout.html", 3, 4);
                    var flexPath = Path.Combine(pages, "flex.html");
                    var flexPage = engine.Go(flexPath);
                    var spreadModel = BlockById(flexPage, "spread");
                    var autoModel = BlockById(flexPage, "auto");
                    var reverseModel = BlockById(flexPage, "reverse");
                    var wrapModel = BlockById(flexPage, "wrap");
                    var wrapOneModel = BlockById(flexPage, "wrap-one");
                    var growModel = BlockById(flexPage, "grow");
                    var growOneModel = BlockById(flexPage, "grow-one");
                    var growTwoModel = BlockById(flexPage, "grow-two");
                    var shrinkOneModel = BlockById(flexPage, "shrink-one");
                    var shrinkTwoModel = BlockById(flexPage, "shrink-two");
                    var shrinkLockModel = BlockById(flexPage, "shrink-lock");
                    var orderOneModel = BlockById(flexPage, "order-one");
                    var orderTwoModel = BlockById(flexPage, "order-two");
                    var orderThreeModel = BlockById(flexPage, "order-three");
                    var columnModel = BlockById(flexPage, "column");
                    var columnReverseModel = BlockById(flexPage, "column-reverse");
                    var aroundModel = BlockById(flexPage, "around");
                    var evenlyModel = BlockById(flexPage, "evenly");
                    var aroundOneModel = BlockById(flexPage, "around-one");
                    var aroundTwoModel = BlockById(flexPage, "around-two");
                    var aroundThreeModel = BlockById(flexPage, "around-three");
                    var twoPartModel = BlockById(flexPage, "after-flex");
                    Eq("flex", spreadModel.S.D, "flex display model");
                    Eq("r", spreadModel.S.Fd, "flex row alias");
                    Eq("b", spreadModel.S.Jc, "space-between alias");
                    Eq("c", spreadModel.S.Ai, "center alignment alias");
                    Eq(10, spreadModel.S.Gp, "flex gap alias");
                    Eq("inline-flex", autoModel.S.D, "inline-flex display model");
                    Eq(12, autoModel.S.Gp, "column-gap alias");
                    Eq("x", reverseModel.S.Fd, "row-reverse alias");
                    Eq("w", wrapModel.S.Wr, "flex wrap alias");
                    Eq(12, wrapModel.S.Rg, "flex row-gap alias");
                    Eq(10, wrapModel.S.Gp, "two-value flex column gap");
                    Eq(1, wrapOneModel.S.Fg, "flex shorthand grow alias");
                    Eq(150, wrapOneModel.S.Fb, "flex shorthand basis alias");
                    Eq(4, growModel.S.Rg, "row-gap alias");
                    Eq(8, growModel.S.Gp, "separate column-gap alias");
                    Eq(1, growOneModel.S.Fg, "flex-grow property alias");
                    Eq(80, growOneModel.S.Fb, "flex-basis property alias");
                    Eq(2, growTwoModel.S.Fg, "weighted flex grow alias");
                    Eq(80, growTwoModel.S.Fb, "weighted flex basis alias");
                    Eq(1, twoPartModel.S.Fg, "two-part flex grow alias");
                    Eq(1, twoPartModel.S.Sh, "two-part flex shrink alias");
                    Eq(0, twoPartModel.S.Fb, "unitless flex shrink is not a pixel basis");
                    Eq(1, shrinkOneModel.S.Sh, "default weighted shrink alias");
                    Eq(2, shrinkTwoModel.S.Sh, "double weighted shrink alias");
                    Eq(0, shrinkLockModel.S.Sh, "locked shrink alias");
                    Eq(140, shrinkLockModel.S.Fb, "locked flex basis alias");
                    Eq(100, shrinkLockModel.S.Wn, "flex minimum width model");
                    Eq(100, shrinkLockModel.S.Wx, "flex maximum width model");
                    Eq(2, orderOneModel.S.Or, "positive flex order alias");
                    Eq(-1, orderTwoModel.S.Or, "negative flex order alias");
                    Eq(2, orderThreeModel.S.Or, "explicit-plus flex order alias");
                    Eq("c", columnModel.S.Fd, "flex column alias");
                    Eq("y", columnReverseModel.S.Fd, "flex column-reverse alias");
                    Eq("a", aroundModel.S.Jc, "space-around alias");
                    Eq("v", evenlyModel.S.Jc, "space-evenly alias");
                    Eq("t", aroundOneModel.S.As, "align-self stretch alias");
                    Eq("c", aroundTwoModel.S.As, "align-self center alias");
                    Eq("e", aroundThreeModel.S.As, "align-self end alias");
                    Eq("s", BlockById(flexPage, "evenly-one").S.As, "align-self start alias");
                    Ok(Object.ReferenceEquals(BlockById(flexPage, "first").Up, spreadModel) && Object.ReferenceEquals(BlockById(flexPage, "nested-copy").Up, BlockById(flexPage, "auto-nested")), "flex visual ownership");
                    view.Size = new Size(720, 1900);
                    view.Go(flexPath);
                    var wideColumnWidth = 0;
                    var wideAroundWidth = 0;
                    var wideAroundInset = 0;
                    using (var flexPaint = Render(view))
                    {
                        var spread = BlockRect(view, BlockById(view.At, "spread"));
                        var first = BlockOuterRect(view, BlockById(view.At, "first"));
                        var second = BlockOuterRect(view, BlockById(view.At, "second"));
                        var third = BlockOuterRect(view, BlockById(view.At, "third"));
                        Eq(spread.Left, first.Left, "flex row starts at content edge");
                        Eq(spread.Right, third.Right, "space-between reaches content edge");
                        Eq(122, second.Left - first.Right, "first distributed flex gap");
                        Eq(122, third.Left - second.Right, "second distributed flex gap");
                        Eq(spread.Top + (spread.Height - first.Height) / 2, first.Top, "first flex item centers");
                        Eq(spread.Top + (spread.Height - second.Height) / 2, second.Top, "second flex item centers");

                        var auto = BlockRect(view, BlockById(view.At, "auto"));
                        var autoLink = BlockOuterRect(view, BlockById(view.At, "auto-link"));
                        var autoButton = BlockOuterRect(view, BlockById(view.At, "auto-button"));
                        var autoNested = BlockOuterRect(view, BlockById(view.At, "auto-nested"));
                        Eq(auto.Left, autoLink.Left, "auto flex starts at content edge");
                        Eq(144, autoButton.Left - autoLink.Left, "auto flex second slot");
                        Eq(144, autoNested.Left - autoButton.Left, "auto flex third slot");
                        Eq(autoLink.Width, autoNested.Width, "auto flex slots share width");
                        Eq(auto.Height, autoLink.Height, "auto flex link stretches");
                        Eq(auto.Height, autoButton.Height, "auto flex field stretches");
                        Ok(!BlockRect(view, BlockById(view.At, "nested-copy")).IsEmpty, "nested flex content layout");
                        Ok(!HitRect(view, "second.html").IsEmpty, "flex link hit target");

                        var reverse = BlockRect(view, BlockById(view.At, "reverse"));
                        var reverseOne = BlockOuterRect(view, BlockById(view.At, "reverse-one"));
                        var reverseTwo = BlockOuterRect(view, BlockById(view.At, "reverse-two"));
                        var reverseThree = BlockOuterRect(view, BlockById(view.At, "reverse-three"));
                        Ok(reverseOne.Left > reverseTwo.Left && reverseTwo.Left > reverseThree.Left, "row-reverse visual order");
                        Eq(reverse.Right, reverseOne.Right, "row-reverse flex-start edge");
                        Eq(reverse.Bottom, reverseOne.Bottom, "row-reverse end alignment");
                        Eq(reverse.Bottom, reverseThree.Bottom, "row-reverse shared end alignment");

                        var wrap = BlockRect(view, BlockById(view.At, "wrap"));
                        var wrapOne = BlockOuterRect(view, BlockById(view.At, "wrap-one"));
                        var wrapTwo = BlockOuterRect(view, BlockById(view.At, "wrap-two"));
                        var wrapThree = BlockOuterRect(view, BlockById(view.At, "wrap-three"));
                        var wrapFour = BlockOuterRect(view, BlockById(view.At, "wrap-four"));
                        Eq(175, wrapOne.Width, "wrapped grow slot width");
                        Eq(10, wrapOne.Left - wrapTwo.Right, "wrapped first-line column gap");
                        Eq(10, wrapThree.Left - wrapFour.Right, "wrapped second-line column gap");
                        Eq(wrap.Right, wrapOne.Right, "wrapped row-reverse first edge");
                        Eq(wrap.Right, wrapThree.Right, "wrapped row-reverse second edge");
                        Eq(wrapOne.Top, wrapTwo.Top, "wrapped first line top");
                        Eq(wrapThree.Top, wrapFour.Top, "wrapped second line top");
                        Eq(56, wrapThree.Top - wrapOne.Top, "wrapped row gap");

                        var grow = BlockRect(view, BlockById(view.At, "grow"));
                        var growOne = BlockOuterRect(view, BlockById(view.At, "grow-one"));
                        var growTwo = BlockOuterRect(view, BlockById(view.At, "grow-two"));
                        var growFixed = BlockOuterRect(view, BlockById(view.At, "grow-fixed"));
                        Eq(grow.Left, growOne.Left, "weighted grow starts at content edge");
                        Eq(140, growOne.Width, "single-weight flex width");
                        Eq(200, growTwo.Width, "double-weight flex width");
                        Eq(80, growFixed.Width, "fixed flex width");
                        Eq(8, growTwo.Left - growOne.Right, "weighted first gap");
                        Eq(8, growFixed.Left - growTwo.Right, "weighted second gap");
                        Eq(grow.Right, growFixed.Right, "weighted grow fills row");

                        var shrink = BlockRect(view, BlockById(view.At, "shrink"));
                        var shrinkOne = BlockOuterRect(view, BlockById(view.At, "shrink-one"));
                        var shrinkTwo = BlockOuterRect(view, BlockById(view.At, "shrink-two"));
                        var shrinkLock = BlockOuterRect(view, BlockById(view.At, "shrink-lock"));
                        Eq(shrink.Left, shrinkOne.Left, "weighted shrink starts at content edge");
                        Eq(140, shrinkOne.Width, "single-weight shrink width");
                        Eq(100, shrinkTwo.Width, "double-weight shrink width");
                        Eq(100, shrinkLock.Width, "locked max-clamped width");
                        Eq(10, shrinkTwo.Left - shrinkOne.Right, "shrink first gap");
                        Eq(10, shrinkLock.Left - shrinkTwo.Right, "shrink second gap");
                        Eq(shrink.Right, shrinkLock.Right, "weighted shrink fills row");

                        var ordered = BlockRect(view, BlockById(view.At, "ordered"));
                        var orderOne = BlockOuterRect(view, BlockById(view.At, "order-one"));
                        var orderTwo = BlockOuterRect(view, BlockById(view.At, "order-two"));
                        var orderThree = BlockOuterRect(view, BlockById(view.At, "order-three"));
                        Eq(ordered.Left, orderTwo.Left, "negative order starts row");
                        Eq(8, orderOne.Left - orderTwo.Right, "ordered row first gap");
                        Eq(8, orderThree.Left - orderOne.Right, "equal order remains stable");

                        var column = BlockRect(view, BlockById(view.At, "column"));
                        var columnOne = BlockOuterRect(view, BlockById(view.At, "column-one"));
                        var columnTwo = BlockOuterRect(view, BlockById(view.At, "column-two"));
                        var columnThree = BlockOuterRect(view, BlockById(view.At, "column-three"));
                        wideColumnWidth = column.Width;
                        Eq(column.Top, columnTwo.Top, "ordered column starts with negative order");
                        Eq(106, columnTwo.Height, "double-weight column growth");
                        Eq(74, columnOne.Height, "single-weight column growth");
                        Eq(40, columnThree.Height, "locked column basis");
                        Eq(10, columnOne.Top - columnTwo.Bottom, "column first row gap");
                        Eq(10, columnThree.Top - columnOne.Bottom, "column second row gap");
                        Eq(column.Left + column.Width / 2, columnOne.Left + columnOne.Width / 2, "column cross-axis center");
                        Eq(column.Left + column.Width / 2, columnTwo.Left + columnTwo.Width / 2, "wide column cross-axis center");
                        Eq(column.Bottom, columnThree.Bottom, "column growth fills main axis");
                        Ok(!HitRect(view, "column.html").IsEmpty, "column link hit target");

                        var columnReverse = BlockRect(view, BlockById(view.At, "column-reverse"));
                        var columnReverseOne = BlockOuterRect(view, BlockById(view.At, "column-reverse-one"));
                        var columnReverseTwo = BlockOuterRect(view, BlockById(view.At, "column-reverse-two"));
                        var columnReverseThree = BlockOuterRect(view, BlockById(view.At, "column-reverse-three"));
                        Eq(50, columnReverseOne.Height, "column shrink first minimum-aware height");
                        Eq(30, columnReverseTwo.Height, "column shrink weighted minimum");
                        Eq(30, columnReverseThree.Height, "column locked height");
                        Eq(columnReverse.Bottom, columnReverseOne.Bottom, "column-reverse starts at bottom");
                        Eq(10, columnReverseOne.Top - columnReverseTwo.Bottom, "column-reverse first gap");
                        Eq(10, columnReverseTwo.Top - columnReverseThree.Bottom, "column-reverse second gap");
                        Eq(columnReverse.Right, columnReverseOne.Right, "column cross-axis end alignment");
                        Eq(columnReverse.Right, columnReverseThree.Right, "column-reverse shared end alignment");
                        var columnAuto = BlockRect(view, BlockById(view.At, "column-auto"));
                        var columnAutoOne = BlockOuterRect(view, BlockById(view.At, "column-auto-one"));
                        var columnAutoTwo = BlockOuterRect(view, BlockById(view.At, "column-auto-two"));
                        Eq(7, columnAutoTwo.Top - columnAutoOne.Bottom, "auto-height column row gap");
                        Eq(columnAuto.Left + columnAuto.Width / 2, columnAutoOne.Left + columnAutoOne.Width / 2, "auto-height column cross alignment");

                        var around = BlockRect(view, BlockById(view.At, "around"));
                        var aroundOne = BlockOuterRect(view, BlockById(view.At, "around-one"));
                        var aroundTwo = BlockOuterRect(view, BlockById(view.At, "around-two"));
                        var aroundThree = BlockOuterRect(view, BlockById(view.At, "around-three"));
                        wideAroundWidth = around.Width;
                        wideAroundInset = aroundOne.Left - around.Left;
                        Eq(26, wideAroundInset, "space-around leading space");
                        Eq(63, aroundTwo.Left - aroundOne.Right, "space-around first distributed gap");
                        Eq(63, aroundThree.Left - aroundTwo.Right, "space-around second distributed gap");
                        Eq(90, aroundOne.Height, "align-self stretch overrides parent start");
                        Eq(around.Top + 35, aroundTwo.Top, "align-self row center");
                        Eq(around.Bottom, aroundThree.Bottom, "align-self row end");
                        Ok(!HitRect(view, "around.html").IsEmpty, "space-around link hit target");

                        var evenly = BlockRect(view, BlockById(view.At, "evenly"));
                        var evenlyOne = BlockOuterRect(view, BlockById(view.At, "evenly-one"));
                        var evenlyTwo = BlockOuterRect(view, BlockById(view.At, "evenly-two"));
                        var evenlyThree = BlockOuterRect(view, BlockById(view.At, "evenly-three"));
                        Eq(27, evenlyOne.Top - evenly.Top, "space-evenly leading space");
                        Eq(37, evenlyTwo.Top - evenlyOne.Bottom, "space-evenly first distributed gap");
                        Eq(37, evenlyThree.Top - evenlyTwo.Bottom, "space-evenly second distributed gap");
                        Eq(evenly.Left, evenlyOne.Left, "align-self column start");
                        Eq(evenly.Left + evenly.Width / 2, evenlyTwo.Left + evenlyTwo.Width / 2, "align-self column center");
                        Eq(evenly.Right, evenlyThree.Right, "align-self column end");
                        Ok(BlockOuterRect(view, BlockById(view.At, "after-flex")).Top > evenly.Bottom, "normal flow follows distributed flex layouts");
                        Ok(ColorDistance(CenterPixel(flexPaint, first), Color.FromArgb(230, 239, 255)) < 8, "flex item paint");
                        Ok(ColorDistance(CenterPixel(flexPaint, wrapOne), Color.FromArgb(243, 232, 255)) < 8, "wrapped flex item paint");
                        Ok(ColorDistance(flexPaint.GetPixel(columnThree.Left + 4, columnThree.Top + 4), Color.FromArgb(220, 239, 232)) < 8, "column flex item paint");
                        Ok(ColorDistance(flexPaint.GetPixel(aroundOne.Left + 4, aroundOne.Top + 4), Color.FromArgb(223, 232, 251)) < 8, "space-around item paint");
                        Ok(ColorDistance(flexPaint.GetPixel(evenlyThree.Left + 4, evenlyThree.Top + 4), Color.FromArgb(247, 231, 202)) < 8, "space-evenly item paint");
                    }
                    view.Size = new Size(360, 1900);
                    Paint(view);
                    var narrowAuto = BlockRect(view, BlockById(view.At, "auto"));
                    var narrowLink = BlockOuterRect(view, BlockById(view.At, "auto-link"));
                    var narrowNested = BlockOuterRect(view, BlockById(view.At, "auto-nested"));
                    Ok(narrowLink.Width < 132 && narrowNested.Right <= narrowAuto.Right, "auto flex slots reflow in narrow view");
                    var narrowWrapOne = BlockOuterRect(view, BlockById(view.At, "wrap-one"));
                    var narrowWrapTwo = BlockOuterRect(view, BlockById(view.At, "wrap-two"));
                    Ok(narrowWrapTwo.Top > narrowWrapOne.Bottom, "wrapped flex moves to one item per narrow line");
                    var narrowShrink = BlockRect(view, BlockById(view.At, "shrink"));
                    var narrowShrinkOne = BlockOuterRect(view, BlockById(view.At, "shrink-one"));
                    var narrowShrinkTwo = BlockOuterRect(view, BlockById(view.At, "shrink-two"));
                    var narrowShrinkLock = BlockOuterRect(view, BlockById(view.At, "shrink-lock"));
                    Eq(120, narrowShrinkOne.Width, "narrow flex respects first minimum");
                    Eq(80, narrowShrinkTwo.Width, "narrow flex respects second minimum");
                    Eq(100, narrowShrinkLock.Width, "narrow flex preserves locked minimum");
                    Eq(24, narrowShrinkLock.Right - narrowShrink.Right, "unshrinkable flex overflow remains explicit");
                    var narrowColumn = BlockRect(view, BlockById(view.At, "column"));
                    var narrowColumnOne = BlockOuterRect(view, BlockById(view.At, "column-one"));
                    Ok(narrowColumn.Width < wideColumnWidth, "column cross axis reflows in narrow view");
                    Eq(narrowColumn.Left + narrowColumn.Width / 2, narrowColumnOne.Left + narrowColumnOne.Width / 2, "narrow column remains centered");
                    var narrowAround = BlockRect(view, BlockById(view.At, "around"));
                    var narrowAroundOne = BlockOuterRect(view, BlockById(view.At, "around-one"));
                    Ok(narrowAround.Width < wideAroundWidth && narrowAroundOne.Left - narrowAround.Left < wideAroundInset, "space-around reflows in narrow view");
                    var gridPath = Path.Combine(pages, "grid.html");
                    var gridPage = engine.Go(gridPath);
                    var equalGrid = BlockById(gridPage, "equal");
                    var mixedGrid = BlockById(gridPage, "mixed");
                    var mixedSpan = BlockById(gridPage, "mixed-four");
                    var separateSpan = BlockById(gridPage, "separate-two");
                    var rowGrid = BlockById(gridPage, "rows");
                    var rowSpan = BlockById(gridPage, "rows-b");
                    var separateRow = BlockById(gridPage, "rows-c");
                    Eq("grid", equalGrid.S.D, "grid display model");
                    Eq(3, equalGrid.S.Gc, "repeated grid track count");
                    Eq(-1, equalGrid.S.Gv(0), "minmax fraction track alias");
                    Eq(-1, equalGrid.S.Gv(2), "repeated final fraction track alias");
                    Eq(0, equalGrid.S.Gv(3), "bounded track lookup");
                    Eq(9, equalGrid.S.Rg, "legacy grid row gap alias");
                    Eq(6, equalGrid.S.Gp, "legacy grid column gap alias");
                    Eq(3, mixedGrid.S.Gc, "mixed grid track count");
                    Eq(100, mixedGrid.S.Gv(0), "fixed grid track alias");
                    Eq(-1, mixedGrid.S.Gv(1), "single fraction grid track alias");
                    Eq(-2, mixedGrid.S.Gv(2), "weighted fraction grid track alias");
                    Eq(2, BlockById(gridPage, "equal-four").S.Gs, "implicit grid span alias");
                    Ok(mixedSpan.S.Gl == 2 && mixedSpan.S.Gs == 2, "grid shorthand start and span aliases");
                    Ok(separateSpan.S.Gl == 2 && separateSpan.S.Gs == 2, "separate grid start and end aliases");
                    Ok(rowGrid.S.Rc == 3 && rowGrid.S.Rv(0) == 30 && rowGrid.S.Rv(1) == -1 && rowGrid.S.Rv(2) == -2 && rowGrid.S.Rv(3) == 0, "grid row track aliases");
                    Ok(rowSpan.S.Rl == 2 && rowSpan.S.Rs == 2 && separateRow.S.Rl == 3 && separateRow.S.Rs == 1, "grid row placement aliases");
                    Eq("inline-grid", BlockById(gridPage, "fields").S.D, "inline-grid display model");
                    Eq(0, BlockById(gridPage, "invalid").S.Gc, "grid track overflow rejected");
                    Ok(BlockById(gridPage, "invalid-one").S.Gl == 0 && BlockById(gridPage, "invalid-one").S.Gs == 1, "invalid grid placement rejected");
                    Ok(BlockById(gridPage, "invalid-one").S.Rl == 0 && BlockById(gridPage, "invalid-one").S.Rs == 1 && BlockById(gridPage, "invalid").S.Rc == 0, "invalid grid row values rejected");
                    Ok(BlockById(gridPage, "limit").S.Gc == 8 && BlockById(gridPage, "limit").S.Rc == 8, "eight-track grid ceiling accepted");
                    Ok(BlockById(gridPage, "reset").S.Gc == 0 && BlockById(gridPage, "reset").S.Rc == 0, "grid templates reset");
                    Ok(Object.ReferenceEquals(BlockById(gridPage, "equal-one").Up, equalGrid) && Object.ReferenceEquals(BlockById(gridPage, "nested-copy").Up, BlockById(gridPage, "nested-card")), "grid visual ownership");

                    view.Size = new Size(720, 2200);
                    view.Go(gridPath);
                    var gridField = FieldById(view.At, "grid-field");
                    gridField.V = "Retained grid field";
                    var wideResponsiveWidth = 0;
                    using (var gridPaint = Render(view))
                    {
                        var equal = BlockRect(view, BlockById(view.At, "equal"));
                        var equalOne = BlockOuterRect(view, BlockById(view.At, "equal-one"));
                        var equalTwo = BlockOuterRect(view, BlockById(view.At, "equal-two"));
                        var equalThree = BlockOuterRect(view, BlockById(view.At, "equal-three"));
                        var equalFour = BlockOuterRect(view, BlockById(view.At, "equal-four"));
                        var equalFive = BlockOuterRect(view, BlockById(view.At, "equal-five"));
                        Eq(106, equalOne.Width, "equal fraction grid width");
                        Eq(6, equalTwo.Left - equalOne.Right, "equal grid column gap");
                        Eq(6, equalThree.Left - equalTwo.Right, "equal grid second column gap");
                        Eq(equal.Left, equalOne.Left, "grid starts at content edge");
                        Eq(equal.Right, equalThree.Right, "grid fills content width");
                        Eq(equalOne.Top, equalThree.Top, "grid first row alignment");
                        Eq(9, equalFour.Top - equalOne.Bottom, "grid row gap");
                        Eq(218, equalFour.Width, "two-column grid span width");
                        Eq(6, equalFive.Left - equalFour.Right, "grid span following gap");

                        var mixed = BlockRect(view, BlockById(view.At, "mixed"));
                        var mixedOne = BlockOuterRect(view, BlockById(view.At, "mixed-one"));
                        var mixedTwo = BlockOuterRect(view, BlockById(view.At, "mixed-two"));
                        var mixedThree = BlockOuterRect(view, BlockById(view.At, "mixed-three"));
                        var mixedFour = BlockOuterRect(view, BlockById(view.At, "mixed-four"));
                        var mixedFive = BlockOuterRect(view, BlockById(view.At, "mixed-five"));
                        Eq(100, mixedOne.Width, "fixed grid track width");
                        Eq(80, mixedTwo.Width, "single fraction grid width");
                        Eq(160, mixedThree.Width, "weighted fraction grid width");
                        Eq(10, mixedTwo.Left - mixedOne.Right, "mixed grid column gap");
                        Eq(250, mixedFour.Width, "explicit mixed grid span width");
                        Eq(mixedTwo.Left, mixedFour.Left, "explicit grid start position");
                        Eq(7, mixedFour.Top - mixedOne.Bottom, "mixed grid row gap");
                        Eq(mixed.Left, mixedFive.Left, "sparse placement resumes on next row");

                        var ordered = BlockRect(view, BlockById(view.At, "ordered"));
                        var orderOne = BlockOuterRect(view, BlockById(view.At, "order-one"));
                        var orderTwo = BlockOuterRect(view, BlockById(view.At, "order-two"));
                        var orderThree = BlockOuterRect(view, BlockById(view.At, "order-three"));
                        Eq(ordered.Left, orderTwo.Left, "negative order starts grid");
                        Eq(4, orderThree.Left - orderTwo.Right, "stable grid order fills first row");
                        Ok(orderOne.Top > orderTwo.Bottom, "positive order moves to next grid row");

                        var center = BlockRect(view, BlockById(view.At, "center"));
                        var centerOne = BlockOuterRect(view, BlockById(view.At, "center-one"));
                        var centerTwo = BlockOuterRect(view, BlockById(view.At, "center-two"));
                        Eq(65, centerOne.Left - center.Left, "centered grid leading space");
                        Eq(10, centerTwo.Left - centerOne.Right, "centered fixed track gap");

                        var separateOne = BlockOuterRect(view, BlockById(view.At, "separate-one"));
                        var separateTwo = BlockOuterRect(view, BlockById(view.At, "separate-two"));
                        var separateThree = BlockOuterRect(view, BlockById(view.At, "separate-three"));
                        Eq(6, separateTwo.Left - separateOne.Right, "separate placement starts second column");
                        Eq(218, separateTwo.Width, "separate placement spans two columns");
                        Ok(separateThree.Top > separateTwo.Bottom, "separate placement advances row");

                        var rows = BlockRect(view, BlockById(view.At, "rows"));
                        var rowsA = BlockOuterRect(view, BlockById(view.At, "rows-a"));
                        var rowsB = BlockOuterRect(view, BlockById(view.At, "rows-b"));
                        var rowsC = BlockOuterRect(view, BlockById(view.At, "rows-c"));
                        Eq(new Size(198, 30), rowsA.Size, "fixed grid row and column span geometry");
                        Eq(new Size(198, 112), rowsB.Size, "two-axis grid span geometry");
                        Eq(new Size(96, 70), rowsC.Size, "weighted grid row geometry");
                        Ok(rowsA.Location == rows.Location && rowsB.Left == rows.Left + 102 && rowsB.Top == rows.Top + 38 && rowsC.Left == rows.Left && rowsC.Top == rows.Top + 80, "explicit two-axis grid placement");
                        Ok(rowsB.Left - rowsC.Right == 6 && rowsB.Bottom == rows.Bottom && rowsC.Bottom == rows.Bottom, "grid row spans remain bounded and nonoverlapping");
                        var autoRows = BlockRect(view, BlockById(view.At, "auto-rows"));
                        var autoRowA = BlockOuterRect(view, BlockById(view.At, "auto-row-a"));
                        var autoRowB = BlockOuterRect(view, BlockById(view.At, "auto-row-b"));
                        var autoRowC = BlockOuterRect(view, BlockById(view.At, "auto-row-c"));
                        Ok(autoRowA.Height == 20 && autoRowA.Width == 240 && autoRowB.Top - autoRowA.Bottom == 5 && autoRowB.Top == autoRowC.Top && autoRowB.Bottom == autoRowC.Bottom && autoRowC.Bottom == autoRows.Bottom, "auto-height grid rows settle before final placement");

                        var fieldRect = BlockOuterRect(view, FieldBlock(view.At, gridField));
                        var buttonRect = BlockOuterRect(view, BlockById(view.At, "grid-button"));
                        Eq(fieldRect.Top, buttonRect.Top, "grid fields share a row");
                        Eq(8, buttonRect.Left - fieldRect.Right, "grid field column gap");
                        Ok(!BlockRect(view, BlockById(view.At, "nested-copy")).IsEmpty, "nested grid content layout");
                        Ok(!FragmentHitRect(view, "#grid-target").IsEmpty, "grid link hit target");
                        var zeroOne = BlockOuterRect(view, BlockById(view.At, "zero-one"));
                        var zeroTwo = BlockOuterRect(view, BlockById(view.At, "zero-two"));
                        Eq(1, zeroOne.Width, "zero grid track renderer floor");
                        Eq(4, zeroTwo.Left - zeroOne.Right, "zero grid track remains nonoverlapping");
                        var overflowOne = BlockOuterRect(view, BlockById(view.At, "overflow-one"));
                        var overflowTwo = BlockOuterRect(view, BlockById(view.At, "overflow-two"));
                        Ok(overflowOne.Width == 6 && overflowTwo.Width == 14 && overflowTwo.Left - overflowOne.Right == 10, "oversized fixed grid tracks shrink into content");
                        wideResponsiveWidth = BlockOuterRect(view, BlockById(view.At, "responsive-one")).Width;
                        Ok(BlockOuterRect(view, BlockById(view.At, "after-grid")).Top > BlockOuterRect(view, BlockById(view.At, "grid-target")).Bottom, "normal flow follows grid layouts");
                        Ok(ColorDistance(gridPaint.GetPixel(equalOne.Left + 4, equalOne.Top + 4), Color.FromArgb(230, 239, 255)) < 8, "grid item paint");
                        Ok(ColorDistance(gridPaint.GetPixel(equalFour.Left + 4, equalFour.Top + 4), Color.FromArgb(223, 245, 232)) < 8, "grid span paint");
                        Ok(ColorDistance(gridPaint.GetPixel(rowsB.Left + 4, rowsB.Top + 4), Color.FromArgb(253, 227, 197)) < 8, "grid row span paint");
                    }
                    view.Size = new Size(360, 2200);
                    Paint(view);
                    var narrowResponsiveWidth = BlockOuterRect(view, BlockById(view.At, "responsive-one")).Width;
                    Ok(narrowResponsiveWidth < wideResponsiveWidth, "fraction grid tracks reflow in narrow view");
                    Ok(Object.ReferenceEquals(gridField, FieldById(view.At, "grid-field")) && gridField.V == "Retained grid field", "grid resize retains field state");
                    var areaPath = Path.Combine(pages, "grid-areas.html");
                    var areaPage = engine.Go(areaPath);
                    var dashboard = BlockById(areaPage, "dashboard");
                    var areaHead = BlockById(areaPage, "area-head");
                    var areaMain = BlockById(areaPage, "area-main");
                    Ok(dashboard.S.Aa == 4 && dashboard.S.Ar == 3 && dashboard.S.Ac == 3, "named grid area dimensions and count");
                    Ok(dashboard.S.Ag("head", 0) == 1 && dashboard.S.Ag("head", 1) == 1 && dashboard.S.Ag("head", 2) == 1 && dashboard.S.Ag("head", 3) == 3, "named grid header area lookup");
                    Ok(dashboard.S.Ag("main", 0) == 2 && dashboard.S.Ag("main", 1) == 2 && dashboard.S.Ag("main", 2) == 1 && dashboard.S.Ag("main", 3) == 2 && dashboard.S.Ag("missing", 0) == 0 && dashboard.S.Ag("main", 4) == 0, "named grid area bounds and lookup limits");
                    Ok(areaHead.S.An == "head" && areaMain.S.An == "main" && BlockById(areaPage, "area-auto").S.An == "", "named grid item aliases");
                    Ok(BlockById(areaPage, "implicit-columns").S.Gc == 0 && BlockById(areaPage, "implicit-columns").S.Ac == 2, "named areas define bounded implicit columns");
                    Ok(BlockById(areaPage, "bad-uneven").S.Aa == 0 && BlockById(areaPage, "bad-shape").S.Aa == 0 && BlockById(areaPage, "bad-token").S.Aa == 0 && BlockById(areaPage, "too-many").S.Aa == 0, "invalid named grid templates rejected");
                    Ok(BlockById(areaPage, "area-reset").S.Aa == 0 && BlockById(areaPage, "area-reset").S.Ar == 0 && BlockById(areaPage, "area-limit").S.Aa == 1 && BlockById(areaPage, "area-limit").S.Ar == 8 && BlockById(areaPage, "area-limit").S.Ac == 8, "named grid reset and 8x8 ceiling");
                    view.Size = new Size(720, 1500);
                    view.Go(areaPath);
                    using (var areaPaint = Render(view))
                    {
                        var dashboardRect = BlockRect(view, BlockById(view.At, "dashboard"));
                        var headRect = BlockOuterRect(view, BlockById(view.At, "area-head"));
                        var navRect = BlockOuterRect(view, BlockById(view.At, "area-nav"));
                        var mainRect = BlockOuterRect(view, BlockById(view.At, "area-main"));
                        var footRect = BlockOuterRect(view, BlockById(view.At, "area-foot"));
                        var autoRect = BlockOuterRect(view, BlockById(view.At, "area-auto"));
                        Eq(new Size(360, 40), headRect.Size, "named header area span geometry");
                        Eq(new Size(80, 78), navRect.Size, "named navigation area geometry");
                        Eq(new Size(272, 78), mainRect.Size, "named main area span geometry");
                        Eq(new Size(176, 50), footRect.Size, "named footer area span geometry");
                        Eq(new Size(176, 50), autoRect.Size, "automatic item fills named-template dot cell");
                        Ok(headRect.Location == dashboardRect.Location && mainRect.Left - navRect.Right == 8 && mainRect.Top - headRect.Bottom == 6 && footRect.Top - navRect.Bottom == 6 && autoRect.Left - footRect.Right == 8 && autoRect.Bottom == dashboardRect.Bottom, "named grid exact placement and gaps");
                        var implicitLeft = BlockOuterRect(view, BlockById(view.At, "implicit-left"));
                        var implicitRight = BlockOuterRect(view, BlockById(view.At, "implicit-right"));
                        Ok(implicitLeft.Width == 96 && implicitRight.Width == 96 && implicitRight.Left - implicitLeft.Right == 8 && implicitLeft.Height == 60 && implicitRight.Height == 60, "named areas create equal implicit columns");
                        Ok(!FragmentHitRect(view, "#area-target").IsEmpty, "named area inline link layout and hit target");
                        Ok(BlockOuterRect(view, BlockById(view.At, "after-areas")).Top > BlockOuterRect(view, BlockById(view.At, "area-target")).Bottom, "normal flow follows named grids");
                        Ok(ColorDistance(areaPaint.GetPixel(mainRect.Left + 4, mainRect.Top + 4), Color.FromArgb(253, 227, 197)) < 8, "named grid area paint");
                    }
                    var linePath = Path.Combine(pages, "grid-lines.html");
                    var linePage = engine.Go(linePath);
                    var lineLayout = BlockById(linePage, "line-layout");
                    var lineHead = BlockById(linePage, "line-head");
                    var lineMain = BlockById(linePage, "line-main");
                    Ok(lineLayout.S.Gnc == 7 && lineLayout.S.Rnc == 5 && lineLayout.S.Gx("edge") == 1 && lineLayout.S.Gx("side-end") == 2 && lineLayout.S.Gx("main-end") == 4 && lineLayout.S.Rx("footer-start") == 3 && lineLayout.S.Rx("bottom") == 4, "named grid line counts and lookups");
                    Ok(lineLayout.S.Gx("missing") == 0 && lineLayout.S.Rx("") == 0 && lineLayout.S.Gx("Edge") == 0, "named grid line lookup bounds and case");
                    Ok(lineHead.S.Gn == "edge" && lineHead.S.Ge == "edge-end" && lineHead.S.Rn == "top" && lineHead.S.Re == "body-start" && lineMain.S.Gn == "main-start" && lineMain.S.Ge == "main-end", "named grid item aliases");
                    var areaLines = BlockById(linePage, "area-lines");
                    Ok(areaLines.S.Gnc == 0 && areaLines.S.Rnc == 0 && areaLines.S.Gx("lead-start") == 1 && areaLines.S.Gx("lead-end") == 2 && areaLines.S.Gx("story-start") == 2 && areaLines.S.Gx("story-end") == 3 && areaLines.S.Rx("lead-start") == 1 && areaLines.S.Rx("lead-end") == 2, "area-generated grid line lookups");
                    Ok(BlockById(linePage, "bad-duplicate").S.Gc == 1 && BlockById(linePage, "bad-duplicate").S.Gx("keep") == 1 && BlockById(linePage, "bad-duplicate").S.Gx("dup") == 0 && BlockById(linePage, "bad-name").S.Gx("valid") == 1, "invalid named line templates preserve prior values");
                    Ok(BlockById(linePage, "bad-reserved").S.Rc == 0 && BlockById(linePage, "too-many-lines").S.Gc == 0 && BlockById(linePage, "too-many-on-line").S.Gc == 0 && BlockById(linePage, "too-long-line").S.Gc == 0 && BlockById(linePage, "line-reset").S.Gnc == 0, "named line invalid and reset limits");
                    Ok(BlockById(linePage, "repeat-lines").S.Gc == 2 && BlockById(linePage, "repeat-lines").S.Gx("rep-end") == 3 && BlockById(linePage, "line-limit").S.Gc == 8 && BlockById(linePage, "line-limit").S.Gnc == 16 && BlockById(linePage, "line-limit").S.Gx("p") == 9, "named line repeat and ceilings");
                    Ok(BlockById(linePage, "case-lines").S.Gx("Case") == 1 && BlockById(linePage, "case-lines").S.Gx("case") == 0 && BlockById(linePage, "case-item").S.Gn == "case", "named line case-sensitive placement input");
                    view.Size = new Size(720, 1700);
                    view.Go(linePath);
                    using (var linePaint = Render(view))
                    {
                        var layoutRect = BlockRect(view, BlockById(view.At, "line-layout"));
                        var headRect = BlockOuterRect(view, BlockById(view.At, "line-head"));
                        var navRect = BlockOuterRect(view, BlockById(view.At, "line-nav"));
                        var mainRect = BlockOuterRect(view, BlockById(view.At, "line-main"));
                        var footRect = BlockOuterRect(view, BlockById(view.At, "line-foot"));
                        var autoRect = BlockOuterRect(view, BlockById(view.At, "line-auto"));
                        Eq(new Size(360, 40), headRect.Size, "named line header geometry");
                        Eq(new Size(80, 78), navRect.Size, "named line navigation geometry");
                        Eq(new Size(272, 78), mainRect.Size, "named line main geometry");
                        Eq(new Size(176, 50), footRect.Size, "named line explicit span geometry");
                        Eq(new Size(176, 50), autoRect.Size, "named line automatic final cell");
                        Ok(headRect.Location == layoutRect.Location && mainRect.Left - navRect.Right == 8 && mainRect.Top - headRect.Bottom == 6 && footRect.Top - navRect.Bottom == 6 && autoRect.Left - footRect.Right == 8 && autoRect.Bottom == layoutRect.Bottom, "named line placement and gaps");
                        var separateLeft = BlockOuterRect(view, BlockById(view.At, "separate-line-left"));
                        var separateRight = BlockOuterRect(view, BlockById(view.At, "separate-line-right"));
                        Ok(separateLeft.Width == 96 && separateRight.Width == 96 && separateRight.Left - separateLeft.Right == 8 && separateLeft.Height == 60 && separateRight.Height == 60, "separate named line longhands");
                        var generatedLead = BlockOuterRect(view, BlockById(view.At, "area-line-lead"));
                        var generatedStory = BlockOuterRect(view, BlockById(view.At, "area-line-story"));
                        Ok(generatedLead.Width == 96 && generatedStory.Width == 96 && generatedStory.Left - generatedLead.Right == 8 && generatedLead.Height == 60 && generatedStory.Height == 60, "area-generated named line geometry");
                        var implicitRowOne = BlockOuterRect(view, BlockById(view.At, "implicit-row-one"));
                        var implicitRowThree = BlockOuterRect(view, BlockById(view.At, "implicit-row-three"));
                        Ok(implicitRowThree.Top > implicitRowOne.Bottom && implicitRowThree.Top - implicitRowOne.Bottom >= 10, "numeric placement retains implicit row bounds");
                        Ok(!FragmentHitRect(view, "#line-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-lines")).Top > BlockOuterRect(view, BlockById(view.At, "line-target")).Bottom, "named line hits and following flow");
                        Ok(ColorDistance(linePaint.GetPixel(mainRect.Left + 4, mainRect.Top + 4), Color.FromArgb(199, 233, 255)) < 8, "named grid line paint");
                    }
                    var flowPath = Path.Combine(pages, "grid-flow.html");
                    var flowPage = engine.Go(flowPath);
                    Ok(BlockById(flowPage, "row-sparse").S.Gf == "r" && BlockById(flowPage, "row-dense").S.Gf == "d" && BlockById(flowPage, "column-flow").S.Gf == "c" && BlockById(flowPage, "column-dense").S.Gf == "x", "grid auto-flow compact aliases");
                    Ok(BlockById(flowPage, "flow-reset").S.Gf == "r" && BlockById(flowPage, "flow-invalid").S.Gf == "c", "grid auto-flow reset and invalid preservation");
                    view.Size = new Size(720, 1900);
                    view.Go(flowPath);
                    using (var flowPaint = Render(view))
                    {
                        var rowSparseA = BlockOuterRect(view, BlockById(view.At, "row-sparse-a"));
                        var rowSparseB = BlockOuterRect(view, BlockById(view.At, "row-sparse-b"));
                        var rowSparseC = BlockOuterRect(view, BlockById(view.At, "row-sparse-c"));
                        var rowDenseA = BlockOuterRect(view, BlockById(view.At, "row-dense-a"));
                        var rowDenseB = BlockOuterRect(view, BlockById(view.At, "row-dense-b"));
                        var rowDenseC = BlockOuterRect(view, BlockById(view.At, "row-dense-c"));
                        Ok(rowSparseA.Width == 198 && rowSparseC.Width == 96 && rowSparseC.Top == rowSparseB.Top && rowSparseC.Left > rowSparseB.Left, "sparse row auto flow keeps its forward cursor");
                        Ok(rowDenseA.Width == 198 && rowDenseC.Width == 96 && rowDenseC.Top == rowDenseA.Top && rowDenseC.Left > rowDenseA.Left && rowDenseB.Top > rowDenseA.Bottom, "dense row auto flow backfills the first hole");
                        var columnOne = BlockOuterRect(view, BlockById(view.At, "column-one"));
                        var columnTwo = BlockOuterRect(view, BlockById(view.At, "column-two"));
                        var columnThree = BlockOuterRect(view, BlockById(view.At, "column-three"));
                        var columnFour = BlockOuterRect(view, BlockById(view.At, "column-four"));
                        var columnFive = BlockOuterRect(view, BlockById(view.At, "column-five"));
                        Ok(columnOne.Width == 93 && columnThree.Width == 93 && columnFive.Width == 94 && columnOne.Height == 47 && columnTwo.Height == 48, "column auto flow creates equal implicit tracks");
                        Ok(columnOne.Left == columnTwo.Left && columnThree.Left == columnFour.Left && columnThree.Left - columnOne.Right == 10 && columnFive.Left - columnThree.Right == 10 && columnTwo.Top - columnOne.Bottom == 5, "column auto flow fills rows then columns");
                        var columnSparseA = BlockOuterRect(view, BlockById(view.At, "column-sparse-a"));
                        var columnSparseB = BlockOuterRect(view, BlockById(view.At, "column-sparse-b"));
                        var columnSparseC = BlockOuterRect(view, BlockById(view.At, "column-sparse-c"));
                        var columnDenseA = BlockOuterRect(view, BlockById(view.At, "column-dense-a"));
                        var columnDenseB = BlockOuterRect(view, BlockById(view.At, "column-dense-b"));
                        var columnDenseC = BlockOuterRect(view, BlockById(view.At, "column-dense-c"));
                        Ok(columnSparseC.Left == columnSparseB.Left && columnSparseC.Top > columnSparseB.Top, "sparse column flow leaves the earlier hole");
                        Ok(columnDenseC.Left == columnDenseA.Left && columnDenseC.Top > columnDenseA.Top && columnDenseB.Left > columnDenseA.Right, "dense column flow backfills the earlier hole");
                        var ceilingEight = BlockOuterRect(view, BlockById(view.At, "ceiling-eight"));
                        var ceilingNine = BlockOuterRect(view, BlockById(view.At, "ceiling-nine"));
                        Ok(ceilingEight.Left == ceilingNine.Left && ceilingNine.Top > ceilingEight.Top, "column flow uses bounded overflow rows after eight columns");
                        Ok(!FragmentHitRect(view, "#flow-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-flow")).Top > BlockOuterRect(view, BlockById(view.At, "flow-target")).Bottom, "grid auto-flow hits and following flow");
                        Ok(ColorDistance(flowPaint.GetPixel(columnFive.Left + 4, columnFive.Top + 4), Color.FromArgb(253, 227, 197)) < 8, "grid auto-flow paint");
                    }
                    var autoTrackPath = Path.Combine(pages, "grid-auto-tracks.html");
                    var autoTrackPage = engine.Go(autoTrackPath);
                    var trackPattern = BlockById(autoTrackPage, "track-pattern").S;
                    Ok(trackPattern.Gac == 2 && trackPattern.Gav(0) == 40 && trackPattern.Gav(1) == -1 && trackPattern.Gav(2) == 0, "grid auto-column pattern aliases");
                    Ok(trackPattern.Rac == 2 && trackPattern.Rav(0) == 30 && trackPattern.Rav(1) == -2 && trackPattern.Rav(2) == 0, "grid auto-row pattern aliases");
                    var autoMixStyle = BlockById(autoTrackPage, "auto-mix").S;
                    Ok(autoMixStyle.Gac == 2 && autoMixStyle.Gav(0) == S.Av && autoMixStyle.Gav(1) == -2 && S.Av == -65, "grid automatic track value alias");
                    Ok(BlockById(autoTrackPage, "track-reset").S.Gac == 0 && BlockById(autoTrackPage, "track-reset").S.Rac == 0, "grid auto-track reset values");
                    var invalidTracks = BlockById(autoTrackPage, "track-invalid").S;
                    Ok(invalidTracks.Gac == 2 && invalidTracks.Gav(0) == 35 && invalidTracks.Gav(1) == -2 && invalidTracks.Rac == 2 && invalidTracks.Rav(0) == 30 && invalidTracks.Rav(1) == S.Av, "invalid grid auto-track values preserve prior patterns");
                    Ok(BlockById(autoTrackPage, "track-limit").S.Gac == 8 && BlockById(autoTrackPage, "track-limit").S.Rac == 8, "grid auto-track pattern ceiling");
                    view.Size = new Size(720, 1700);
                    view.Go(autoTrackPath);
                    using (var autoTrackPaint = Render(view))
                    {
                        var pattern = BlockOuterRect(view, BlockById(view.At, "track-pattern"));
                        var patternA = BlockOuterRect(view, BlockById(view.At, "pattern-a"));
                        var patternB = BlockOuterRect(view, BlockById(view.At, "pattern-b"));
                        var patternC = BlockOuterRect(view, BlockById(view.At, "pattern-c"));
                        var patternD = BlockOuterRect(view, BlockById(view.At, "pattern-d"));
                        var patternE = BlockOuterRect(view, BlockById(view.At, "pattern-e"));
                        Ok(pattern.Size == new Size(300, 200) && patternA.Size == new Size(60, 20) && patternB.Size == new Size(40, 30) && patternC.Size == new Size(60, 105) && patternD.Size == new Size(40, 30) && patternE.Size == new Size(60, 20), "repeating grid auto-track geometry");
                        Ok(patternB.Left - patternA.Right == 10 && patternC.Left - patternB.Right == 10 && patternD.Left - patternC.Right == 10 && patternE.Left - patternD.Right == 10 && patternB.Top - patternA.Bottom == 5 && patternC.Top - patternB.Bottom == 5 && patternD.Top - patternC.Bottom == 5, "grid auto-track pattern order and gaps");
                        var mixA = BlockOuterRect(view, BlockById(view.At, "auto-mix-a"));
                        var mixB = BlockOuterRect(view, BlockById(view.At, "auto-mix-b"));
                        var mixC = BlockOuterRect(view, BlockById(view.At, "auto-mix-c"));
                        var mixD = BlockOuterRect(view, BlockById(view.At, "auto-mix-d"));
                        var mixE = BlockOuterRect(view, BlockById(view.At, "auto-mix-e"));
                        Ok(mixA.Width == 50 && mixB.Width == 25 && mixC.Width == 50 && mixD.Width == 25 && mixE.Width == 50 && mixE.Right - mixA.Left == 220, "mixed auto and fraction implicit columns");
                        var rowCycle = BlockOuterRect(view, BlockById(view.At, "row-cycle"));
                        var rowA = BlockOuterRect(view, BlockById(view.At, "row-cycle-a"));
                        var rowC = BlockOuterRect(view, BlockById(view.At, "row-cycle-c"));
                        var rowE = BlockOuterRect(view, BlockById(view.At, "row-cycle-e"));
                        Ok(rowCycle.Height == 135 && rowA.Height == 35 && rowC.Height == 55 && rowE.Height == 35 && rowC.Top - rowA.Bottom == 5 && rowE.Top - rowC.Bottom == 5, "auto-height repeating implicit rows settle exactly");
                        var largeGap = BlockOuterRect(view, BlockById(view.At, "row-large-gap"));
                        var largeGapA = BlockOuterRect(view, BlockById(view.At, "row-large-gap-a"));
                        var largeGapB = BlockOuterRect(view, BlockById(view.At, "row-large-gap-b"));
                        var largeGapC = BlockOuterRect(view, BlockById(view.At, "row-large-gap-c"));
                        Ok(largeGap.Height == 120 && largeGapA.Height == 10 && largeGapB.Height == 20 && largeGapC.Height == 10 && largeGapB.Top - largeGapA.Bottom == 40 && largeGapC.Top - largeGapB.Bottom == 40, "auto-height implicit rows reserve authored large gaps");
                        var areaLead = BlockOuterRect(view, BlockById(view.At, "area-lead"));
                        var areaStory = BlockOuterRect(view, BlockById(view.At, "area-story"));
                        var areaAction = BlockOuterRect(view, BlockById(view.At, "area-action"));
                        Ok(areaLead.Width == 50 && areaStory.Width == 30 && areaAction.Width == 100 && areaStory.Left - areaLead.Right == 10 && areaAction.Left - areaStory.Right == 10, "area-created tracks use the automatic column pattern");
                        Ok(!FragmentHitRect(view, "#auto-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-auto-tracks")).Top > BlockOuterRect(view, BlockById(view.At, "auto-target")).Bottom, "grid auto-track hits and following flow");
                        Ok(ColorDistance(autoTrackPaint.GetPixel(patternC.Left + 4, patternC.Top + 4), Color.FromArgb(199, 233, 255)) < 8, "grid auto-track paint");
                    }
                    var shorthandPath = Path.Combine(pages, "grid-shorthand.html");
                    var shorthandPage = engine.Go(shorthandPath);
                    var explicitGrid = BlockById(shorthandPage, "explicit-grid").S;
                    Ok(explicitGrid.Rc == 2 && explicitGrid.Rv(0) == 30 && explicitGrid.Rv(1) == -1 && explicitGrid.Gc == 3 && explicitGrid.Gv(0) == 80 && explicitGrid.Gv(1) == -1 && explicitGrid.Gv(2) == -2, "explicit grid shorthand track aliases");
                    Ok(explicitGrid.Gf == "r" && explicitGrid.Gac == 0 && explicitGrid.Rac == 0 && explicitGrid.Aa == 0, "explicit grid shorthand resets automatic state");
                    var templatePreserve = BlockById(shorthandPage, "template-preserve").S;
                    Ok(templatePreserve.Rc == 2 && templatePreserve.Gc == 2 && templatePreserve.Gf == "x" && templatePreserve.Gac == 1 && templatePreserve.Gav(0) == 25 && templatePreserve.Rac == 1 && templatePreserve.Rav(0) == 35, "grid-template shorthand preserves automatic state");
                    var areaGrid = BlockById(shorthandPage, "area-grid").S;
                    Ok(areaGrid.Aa == 3 && areaGrid.Ar == 2 && areaGrid.Ac == 2 && areaGrid.Rc == 2 && areaGrid.Rv(0) == 30 && areaGrid.Rv(1) == -1 && areaGrid.Gc == 2 && areaGrid.Gv(0) == 90 && areaGrid.Gv(1) == -1 && areaGrid.Ag("head", 3) == 2, "quoted area grid shorthand aliases");
                    var areaAuto = BlockById(shorthandPage, "area-auto").S;
                    Ok(areaAuto.Rc == 1 && areaAuto.Rv(0) == S.Av && areaAuto.Gc == 2 && areaAuto.Gv(0) == 50 && areaAuto.Gv(1) == -1, "omitted shorthand area row uses explicit auto");
                    var rowFlowGrid = BlockById(shorthandPage, "row-flow-grid").S;
                    Ok(rowFlowGrid.Gf == "d" && rowFlowGrid.Gc == 3 && rowFlowGrid.Rc == 0 && rowFlowGrid.Gac == 0 && rowFlowGrid.Rac == 2 && rowFlowGrid.Rav(0) == 30 && rowFlowGrid.Rav(1) == -1, "row auto-flow grid shorthand aliases");
                    var columnFlowGrid = BlockById(shorthandPage, "column-flow-grid").S;
                    Ok(columnFlowGrid.Gf == "x" && columnFlowGrid.Rc == 2 && columnFlowGrid.Rv(0) == 25 && columnFlowGrid.Rv(1) == -1 && columnFlowGrid.Gc == 0 && columnFlowGrid.Gac == 2 && columnFlowGrid.Gav(0) == 40 && columnFlowGrid.Gav(1) == -1 && columnFlowGrid.Rac == 0, "column auto-flow grid shorthand aliases");
                    var explicitAuto = BlockById(shorthandPage, "explicit-auto").S;
                    Ok(explicitAuto.Rc == 2 && explicitAuto.Rv(0) == S.Av && explicitAuto.Rv(1) == 20 && explicitAuto.Gc == 2 && explicitAuto.Gv(0) == S.Av && explicitAuto.Gv(1) == S.Av, "explicit grid auto track aliases");
                    var gridReset = BlockById(shorthandPage, "grid-reset").S;
                    Ok(gridReset.Gc == 0 && gridReset.Rc == 0 && gridReset.Aa == 0 && gridReset.Gac == 0 && gridReset.Rac == 0 && gridReset.Gf == "r" && gridReset.Gp == 7 && gridReset.Rg == 7, "grid shorthand resets all grid state except gaps");
                    var templateReset = BlockById(shorthandPage, "template-reset").S;
                    Ok(templateReset.Gc == 0 && templateReset.Rc == 0 && templateReset.Aa == 0 && templateReset.Gac == 1 && templateReset.Gav(0) == 40 && templateReset.Rac == 1 && templateReset.Rav(0) == 50 && templateReset.Gf == "c", "grid-template reset preserves automatic state");
                    var invalidGrid = BlockById(shorthandPage, "invalid-grid").S;
                    Ok(invalidGrid.Gc == 2 && invalidGrid.Gv(0) == 50 && invalidGrid.Rc == 1 && invalidGrid.Rv(0) == 20 && invalidGrid.Gac == 1 && invalidGrid.Gav(0) == 30 && invalidGrid.Rac == 1 && invalidGrid.Rav(0) == 40 && invalidGrid.Gf == "c", "invalid grid shorthand is atomic");
                    var invalidTemplate = BlockById(shorthandPage, "invalid-template").S;
                    Ok(invalidTemplate.Gc == 2 && invalidTemplate.Gv(0) == 45 && invalidTemplate.Gv(1) == -1 && invalidTemplate.Rc == 2 && invalidTemplate.Rv(0) == 25 && invalidTemplate.Rv(1) == -1 && invalidTemplate.Aa == 0, "invalid grid-template shorthand is atomic");
                    view.Size = new Size(720, 2000);
                    view.Go(shorthandPath);
                    using (var shorthandPaint = Render(view))
                    {
                        var explicitA = BlockOuterRect(view, BlockById(view.At, "explicit-a"));
                        var explicitB = BlockOuterRect(view, BlockById(view.At, "explicit-b"));
                        var explicitC = BlockOuterRect(view, BlockById(view.At, "explicit-c"));
                        var explicitD = BlockOuterRect(view, BlockById(view.At, "explicit-d"));
                        var explicitF = BlockOuterRect(view, BlockById(view.At, "explicit-f"));
                        Ok(explicitA.Size == new Size(80, 30) && explicitB.Size == new Size(66, 30) && explicitC.Size == new Size(134, 30) && explicitD.Height == 65 && explicitF.Size == new Size(134, 65), "explicit grid shorthand geometry");
                        Ok(explicitB.Left - explicitA.Right == 10 && explicitC.Left - explicitB.Right == 10 && explicitD.Top - explicitA.Bottom == 5, "explicit grid shorthand gaps and placement");
                        var preserveA = BlockOuterRect(view, BlockById(view.At, "template-preserve-a"));
                        var preserveB = BlockOuterRect(view, BlockById(view.At, "template-preserve-b"));
                        var preserveC = BlockOuterRect(view, BlockById(view.At, "template-preserve-c"));
                        Ok(preserveA.Size == new Size(70, 40) && preserveB.Size == new Size(70, 60) && preserveC.Width == 120 && preserveB.Top - preserveA.Bottom == 5 && preserveC.Left - preserveA.Right == 10, "grid-template shorthand host geometry");
                        var shorthandAreaHead = BlockOuterRect(view, BlockById(view.At, "area-head"));
                        var shorthandAreaNav = BlockOuterRect(view, BlockById(view.At, "area-nav"));
                        var shorthandAreaMain = BlockOuterRect(view, BlockById(view.At, "area-main"));
                        Ok(shorthandAreaHead.Size == new Size(300, 30) && shorthandAreaNav.Size == new Size(90, 65) && shorthandAreaMain.Size == new Size(200, 65) && shorthandAreaNav.Top - shorthandAreaHead.Bottom == 5 && shorthandAreaMain.Left - shorthandAreaNav.Right == 10, "quoted area grid shorthand geometry");
                        var rowA = BlockOuterRect(view, BlockById(view.At, "row-flow-a"));
                        var rowB = BlockOuterRect(view, BlockById(view.At, "row-flow-b"));
                        var rowC = BlockOuterRect(view, BlockById(view.At, "row-flow-c"));
                        Ok(rowA.Size == new Size(180, 30) && rowC.Size == new Size(110, 30) && rowC.Top == rowA.Top && rowC.Left - rowA.Right == 10 && rowB.Size == new Size(180, 65) && rowB.Top - rowA.Bottom == 5, "row auto-flow shorthand dense geometry");
                        var columnA = BlockOuterRect(view, BlockById(view.At, "column-flow-a"));
                        var columnB = BlockOuterRect(view, BlockById(view.At, "column-flow-b"));
                        var columnC = BlockOuterRect(view, BlockById(view.At, "column-flow-c"));
                        var columnE = BlockOuterRect(view, BlockById(view.At, "column-flow-e"));
                        Ok(columnA.Size == new Size(40, 25) && columnB.Size == new Size(40, 70) && columnC.Size == new Size(200, 25) && columnE.Size == new Size(40, 25), "column auto-flow shorthand geometry");
                        Ok(columnB.Top - columnA.Bottom == 5 && columnC.Left - columnA.Right == 10 && columnE.Left - columnC.Right == 10, "column auto-flow shorthand gaps and placement");
                        Ok(!FragmentHitRect(view, "#shorthand-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-shorthands")).Top > BlockOuterRect(view, BlockById(view.At, "shorthand-target")).Bottom, "grid shorthand hits and following flow");
                        Ok(ColorDistance(shorthandPaint.GetPixel(shorthandAreaMain.Left + 4, shorthandAreaMain.Top + 4), Color.FromArgb(199, 233, 255)) < 8, "grid shorthand paint");
                    }
                    var alignmentPath = Path.Combine(pages, "grid-alignment.html");
                    var alignmentPage = engine.Go(alignmentPath);
                    var placeGrid = BlockById(alignmentPage, "place-grid").S;
                    var placeAStyle = BlockById(alignmentPage, "place-a").S;
                    var placeBStyle = BlockById(alignmentPage, "place-b").S;
                    var placeCStyle = BlockById(alignmentPage, "place-c").S;
                    var placeDStyle = BlockById(alignmentPage, "place-d").S;
                    Ok(placeGrid.Ji == "c" && placeGrid.Ai == "c" && placeAStyle.Js == "" && placeAStyle.As == "", "place-items compact aliases");
                    Ok(placeBStyle.Js == "e" && placeBStyle.As == "e" && placeCStyle.Js == "t" && placeCStyle.As == "t" && placeDStyle.Js == "s" && placeDStyle.As == "s", "place-self and grid alignment longhand aliases");
                    var longhandGrid = BlockById(alignmentPage, "longhand-grid").S;
                    var longhandBStyle = BlockById(alignmentPage, "longhand-b").S;
                    Ok(longhandGrid.Ji == "e" && longhandGrid.Ai == "e" && longhandBStyle.Js == "c" && longhandBStyle.As == "c", "justify and align item/self aliases");
                    var atomicItems = BlockById(alignmentPage, "atomic-items").S;
                    var atomicSelf = BlockById(alignmentPage, "atomic-self").S;
                    Ok(atomicItems.Ji == "e" && atomicItems.Ai == "s" && atomicSelf.Js == "e" && atomicSelf.As == "s", "invalid place shorthands preserve both prior axes atomically");
                    var resetItems = BlockById(alignmentPage, "reset-items").S;
                    var resetSelf = BlockById(alignmentPage, "reset-self").S;
                    Ok(resetItems.Ji == "t" && resetItems.Ai == "t" && resetSelf.Js == "" && resetSelf.As == "", "place shorthand resets restore compact defaults");
                    var twoItems = BlockById(alignmentPage, "two-items").S;
                    var twoSelf = BlockById(alignmentPage, "two-self").S;
                    Ok(twoItems.Ai == "s" && twoItems.Ji == "e" && twoSelf.As == "e" && twoSelf.Js == "c", "two-value place shorthand axis order");
                    view.Size = new Size(720, 1400);
                    view.Go(alignmentPath);
                    using (var alignmentPaint = Render(view))
                    {
                        var place = BlockOuterRect(view, BlockById(view.At, "place-grid"));
                        var placeA = BlockOuterRect(view, BlockById(view.At, "place-a"));
                        var placeB = BlockOuterRect(view, BlockById(view.At, "place-b"));
                        var placeC = BlockOuterRect(view, BlockById(view.At, "place-c"));
                        var placeD = BlockOuterRect(view, BlockById(view.At, "place-d"));
                        Ok(placeA.Size == new Size(40, 20) && placeA.Left - place.Left == 50 && placeA.Top - place.Top == 40, "place-items center geometry");
                        Ok(placeB.Size == new Size(50, 30) && placeB.Left - place.Left == 250 && placeB.Top - place.Top == 70, "place-self end geometry");
                        Ok(placeC.Size == new Size(140, 100) && placeC.Left == place.Left && placeC.Top - place.Top == 110, "place-self stretch geometry");
                        Ok(placeD.Size == new Size(60, 40) && placeD.Left - place.Left == 150 && placeD.Top - place.Top == 110, "grid alignment start geometry");
                        var longhand = BlockOuterRect(view, BlockById(view.At, "longhand-grid"));
                        var longhandA = BlockOuterRect(view, BlockById(view.At, "longhand-a"));
                        var longhandB = BlockOuterRect(view, BlockById(view.At, "longhand-b"));
                        Ok(longhandA.Size == new Size(40, 20) && longhandA.Left - longhand.Left == 100 && longhandA.Top - longhand.Top == 80, "grid longhand end geometry");
                        Ok(longhandB.Size == new Size(50, 30) && longhandB.Left - longhand.Left == 200 && longhandB.Top - longhand.Top == 35, "grid self center override geometry");
                        var fit = BlockOuterRect(view, BlockById(view.At, "fit-grid"));
                        var fitText = BlockOuterRect(view, BlockById(view.At, "fit-text"));
                        Ok(fitText.Width > 1 && fitText.Width < fit.Width && fitText.Left - fit.Left == (fit.Width - fitText.Width) / 2, "grid non-stretch auto width fits and centers");
                        Ok(fitText.Height < fit.Height && fitText.Top - fit.Top == (fit.Height - fitText.Height) / 2, "grid non-stretch natural height centers");
                        var fitClamp = BlockOuterRect(view, BlockById(view.At, "fit-clamp-grid"));
                        var fitClampText = BlockOuterRect(view, BlockById(view.At, "fit-clamp-text"));
                        Ok(fitClampText.Width == 100 && fitClampText.Left == fitClamp.Left, "grid auto-fit minimum stays clamped to its area");
                        Ok(!FragmentHitRect(view, "#alignment-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-alignment")).Top > BlockOuterRect(view, BlockById(view.At, "alignment-target")).Bottom, "grid alignment hits and following flow");
                        Ok(ColorDistance(alignmentPaint.GetPixel(placeC.Left + 4, placeC.Top + 4), Color.FromArgb(253, 227, 197)) < 8, "grid alignment paint");
                    }
                    var contentAlignmentPath = Path.Combine(pages, "grid-content-alignment.html");
                    var contentAlignmentPage = engine.Go(contentAlignmentPath);
                    var centerContent = BlockById(contentAlignmentPage, "center-grid").S;
                    var axisContent = BlockById(contentAlignmentPage, "axis-grid").S;
                    var betweenContent = BlockById(contentAlignmentPage, "between-grid").S;
                    var mixedContent = BlockById(contentAlignmentPage, "mixed-grid").S;
                    Ok(centerContent.Al == "c" && centerContent.Jc == "c" && axisContent.Al == "e" && axisContent.Jc == "s", "place-content center and two-value aliases");
                    Ok(betweenContent.Al == "b" && betweenContent.Jc == "b" && mixedContent.Al == "a" && mixedContent.Jc == "v", "place-content distribution aliases");
                    var longhandContent = BlockById(contentAlignmentPage, "longhand-content").S;
                    var atomicContent = BlockById(contentAlignmentPage, "atomic-content").S;
                    var resetContent = BlockById(contentAlignmentPage, "reset-content").S;
                    var stretchContent = BlockById(contentAlignmentPage, "stretch-content").S;
                    Ok(longhandContent.Al == "c" && longhandContent.Jc == "e", "align-content and justify-content longhand aliases");
                    Ok(atomicContent.Al == "e" && atomicContent.Jc == "c", "invalid place-content preserves both prior axes atomically");
                    Ok(resetContent.Al == "s" && resetContent.Jc == "s" && stretchContent.Al == "s" && stretchContent.Jc == "s", "place-content reset and bounded stretch aliases");
                    view.Size = new Size(720, 2600);
                    view.Go(contentAlignmentPath);
                    using (var contentAlignmentPaint = Render(view))
                    {
                        var center = BlockOuterRect(view, BlockById(view.At, "center-grid"));
                        var centerA = BlockOuterRect(view, BlockById(view.At, "center-a"));
                        var centerD = BlockOuterRect(view, BlockById(view.At, "center-d"));
                        Ok(centerA.Size == new Size(80, 40) && centerA.Left - center.Left == 55 && centerA.Top - center.Top == 55, "grid centered track-group geometry");
                        Ok(centerD.Size == new Size(100, 60) && centerD.Left - center.Left == 145 && centerD.Top - center.Top == 105, "grid centered second track geometry");
                        var axis = BlockOuterRect(view, BlockById(view.At, "axis-grid"));
                        var axisA = BlockOuterRect(view, BlockById(view.At, "axis-a"));
                        var axisD = BlockOuterRect(view, BlockById(view.At, "axis-d"));
                        Ok(axisA.Left == axis.Left && axisA.Top - axis.Top == 110 && axisD.Left - axis.Left == 90 && axisD.Top - axis.Top == 160, "grid two-axis end/start content geometry");
                        var between = BlockOuterRect(view, BlockById(view.At, "between-grid"));
                        var betweenA = BlockOuterRect(view, BlockById(view.At, "between-a"));
                        var betweenB = BlockOuterRect(view, BlockById(view.At, "between-b"));
                        var betweenC = BlockOuterRect(view, BlockById(view.At, "between-c"));
                        Ok(betweenA.Location == between.Location && betweenB.Left - between.Left == 200 && betweenC.Top - between.Top == 160, "grid space-between track distribution");
                        var mixed = BlockOuterRect(view, BlockById(view.At, "mixed-grid"));
                        var mixedA = BlockOuterRect(view, BlockById(view.At, "mixed-a"));
                        var mixedB = BlockOuterRect(view, BlockById(view.At, "mixed-b"));
                        var mixedC = BlockOuterRect(view, BlockById(view.At, "mixed-c"));
                        var mixedD = BlockOuterRect(view, BlockById(view.At, "mixed-d"));
                        Ok(mixedA.Left - mixed.Left == 40 && mixedA.Top - mixed.Top == 30 && mixedB.Left - mixed.Left == 160 && mixedC.Top - mixed.Top == 130 && mixedD.Location == new Point(mixed.Left + 160, mixed.Top + 130), "grid mixed around/evenly content geometry");
                        var longhand = BlockOuterRect(view, BlockById(view.At, "longhand-content"));
                        var longhandItem = BlockOuterRect(view, BlockById(view.At, "longhand-item"));
                        Ok(longhandItem.Left - longhand.Left == 120 && longhandItem.Top - longhand.Top == 60, "grid longhand content alignment geometry");
                        Ok(!FragmentHitRect(view, "#content-target").IsEmpty && BlockOuterRect(view, BlockById(view.At, "after-content")).Top > BlockOuterRect(view, BlockById(view.At, "content-target")).Bottom, "grid content alignment hits and following flow");
                        Ok(ColorDistance(contentAlignmentPaint.GetPixel(mixedA.Left + 4, mixedA.Top + 4), Color.FromArgb(253, 227, 197)) < 8, "grid content alignment paint");
                    }
                    var svgPath = Path.Combine(pages, "svg.html");
                    var svgPage = engine.Go(svgPath);
                    var vector = BlockById(svgPage, "vector");
                    Eq("svg", vector.K, "inline SVG block kind");
                    Eq("Inline SVG status chart", vector.X, "inline SVG title label");
                    Ok(vector.Vg != null && vector.Vg.Vb && vector.Vg.W == 320D && vector.Vg.H == 180D && vector.Vg.Vw == 160D && vector.Vg.Vh == 90D, "bounded SVG viewport aliases");
                    Eq(10, vector.Vg.Ss.Count, "supported SVG shape count and invalid rejection");
                    Eq("rect", vector.Vg.Ss[0].K, "SVG rectangle alias");
                    Ok(vector.Vg.Ss[0].P.Count == 6 && vector.Vg.Ss[0].P[4] == 4D, "SVG rounded rectangle coordinates");
                    Ok(vector.Vg.Ss[2].F == "currentColor" && vector.Vg.Ss[2].C == "#2563eb" && Math.Abs(vector.Vg.Ss[2].O - .8D) < .001D, "SVG inherited currentColor and opacity");
                    Ok(vector.Vg.Ss[6].K == "path" && vector.Vg.Ss[6].D.Count == 5 && vector.Vg.Ss[6].D[0].K == 'M' && vector.Vg.Ss[6].D[4].K == 'Z', "SVG straight relative path steps");
                    Ok(vector.Vg.Ss[7].U != null && vector.Vg.Ss[7].U.Fragment == "#svg-target" && vector.Vg.Ss[7].Fo == .85D, "SVG linked shape and fill opacity");
                    Ok(vector.Vg.Ss[8].K == "text" && vector.Vg.Ss[8].T == "Lea vector" && vector.Vg.Ss[8].A == "m" && vector.Vg.Ss[8].Z == 9D, "SVG text aliases");
                    Ok(vector.Vg.Ss[9].K == "path" && vector.Vg.Ss[9].D.Count == 2 && vector.Vg.Ss[9].D[1].K == 'C', "SVG cubic path acceptance");
                    Ok(svgPage.Ls.Count == 1 && svgPage.Ls[0].U.Fragment == "#svg-target", "SVG page link collection");
                    var svgGraphic = engine.Ax(svgPage, ById(svgPage.D, "vector"));
                    Ok(svgGraphic.K == "img" && svgGraphic.N == "Inline SVG status chart", "SVG graphic accessibility name");
                    view.Size = new Size(720, 760);
                    view.Go(svgPath);
                    using (var svgPaint = Render(view))
                    {
                        var svgRect = BlockRect(view, BlockById(view.At, "vector"));
                        Eq(new Size(320, 180), svgRect.Size, "SVG intrinsic layout size");
                        Ok(ColorDistance(svgPaint.GetPixel(svgRect.Left + 20, svgRect.Top + 20), Color.FromArgb(215, 38, 61)) < 12, "SVG rectangle paint");
                        Ok(ColorDistance(svgPaint.GetPixel(svgRect.Left + 110, svgRect.Top + 30), Color.FromArgb(16, 185, 129)) < 12, "SVG rgb circle paint");
                        Ok(ColorDistance(svgPaint.GetPixel(svgRect.Left + 170, svgRect.Top + 30), Color.FromArgb(81, 130, 239)) < 18, "SVG inherited currentColor paint");
                        Ok(ColorDistance(svgPaint.GetPixel(svgRect.Left + 160, svgRect.Top + 90), Color.FromArgb(245, 158, 11)) < 12, "SVG straight path paint");
                        Ok(!FragmentHitRect(view, "#svg-target").IsEmpty, "SVG linked shape hit target");
                        Ok(BlockOuterRect(view, BlockById(view.At, "after-svg")).Top > svgRect.Bottom, "normal flow follows inline SVG");
                    }
                    var svgGeometryPath = Path.Combine(pages, "svg-geometry.html");
                    var svgGeometryPage = engine.Go(svgGeometryPath);
                    var geometry = BlockById(svgGeometryPage, "geometry");
                    Ok(geometry.Vg != null && geometry.Vg.A == "mm" && geometry.Vg.M == "m", "SVG default preserveAspectRatio aliases");
                    Eq(10, geometry.Vg.Ss.Count, "SVG curve, transform, and invalid rejection shape count");
                    var cubic = geometry.Vg.Ss[0];
                    Ok(cubic.D.Count == 3 && cubic.D[1].K == 'C' && cubic.D[2].K == 'C', "SVG cubic and smooth canonical steps");
                    Ok(cubic.D[2].A == 50D && cubic.D[2].B == 60D && cubic.D[2].C == 60D && cubic.D[2].D == 60D, "SVG smooth cubic reflected controls");
                    var quadratic = geometry.Vg.Ss[1];
                    Ok(quadratic.D.Count == 3 && quadratic.D[1].K == 'Q' && quadratic.D[2].K == 'Q', "SVG quadratic and smooth canonical steps");
                    Ok(quadratic.D[2].A == 55D && quadratic.D[2].B == 100D, "SVG smooth quadratic reflected control");
                    var arc = geometry.Vg.Ss[2];
                    var arcCurves = true;
                    for (var arcIndex = 1; arcIndex < arc.D.Count; arcIndex++) arcCurves = arcCurves && arc.D[arcIndex].K == 'C';
                    Ok(arc.D.Count >= 2 && arc.D.Count <= 5 && arcCurves && arc.D[arc.D.Count - 1].X == 120D && arc.D[arc.D.Count - 1].Y == 50D, "SVG elliptical arc cubic conversion");
                    var nestedTransform = geometry.Vg.Ss[3];
                    Ok(nestedTransform.M[0] == 1.5D && nestedTransform.M[3] == 1.5D && nestedTransform.M[4] == 120D && nestedTransform.M[5] == 10D, "SVG cumulative translate and scale matrix");
                    var rotateTransform = geometry.Vg.Ss[4];
                    Ok(Math.Abs(rotateTransform.M[0]) < .0001D && Math.Abs(rotateTransform.M[1] - 1D) < .0001D && Math.Abs(rotateTransform.M[2] + 1D) < .0001D, "SVG centered rotate matrix");
                    Ok(Math.Abs(geometry.Vg.Ss[5].M[2] - 1D) < .0001D && geometry.Vg.Ss[6].M[1] < 0D, "SVG skew transform matrices");
                    Ok(geometry.Vg.Ss[7].U != null && geometry.Vg.Ss[7].U.Fragment == "#geometry-target" && geometry.Vg.Ss[7].M[4] == 145D, "SVG transformed linked shape");
                    Ok(geometry.Vg.Ss[8].M[4] == 132D && geometry.Vg.Ss[8].M[5] == 82D, "SVG inline style transform matrix");
                    Ok(geometry.Vg.Ss[9].M[0] == 1D && geometry.Vg.Ss[9].M[3] == 1D && geometry.Vg.Ss[9].M[4] == 0D, "SVG unsafe transform falls back to inherited identity");
                    var aspectNone = BlockById(svgGeometryPage, "aspect-none");
                    var aspectMin = BlockById(svgGeometryPage, "aspect-min");
                    var aspectSlice = BlockById(svgGeometryPage, "aspect-slice");
                    Ok(aspectNone.Vg.A == "n" && aspectMin.Vg.A == "nx" && aspectMin.Vg.M == "m" && aspectSlice.Vg.A == "mx" && aspectSlice.Vg.M == "s", "SVG preserveAspectRatio compact modes");
                    Ok(svgGeometryPage.Ls.Count == 1 && svgGeometryPage.Ls[0].U.Fragment == "#geometry-target", "SVG transformed link collection");
                    var geometryGraphic = engine.Ax(svgGeometryPage, ById(svgGeometryPage.D, "geometry"));
                    Ok(geometryGraphic.K == "img" && geometryGraphic.N == "Curves and transformed shapes", "SVG geometry accessibility name");
                    view.Size = new Size(720, 1100);
                    view.Go(svgGeometryPath);
                    using (var geometryPaint = Render(view))
                    {
                        var geometryRect = BlockRect(view, BlockById(view.At, "geometry"));
                        Eq(new Size(360, 200), geometryRect.Size, "SVG geometry viewport size");
                        Ok(HasColor(geometryPaint, new Rectangle(geometryRect.Left, geometryRect.Top, 160, 200), Color.FromArgb(220, 38, 38)), "SVG cubic curve paint");
                        Ok(HasColor(geometryPaint, new Rectangle(geometryRect.Left, geometryRect.Top + 70, 160, 125), Color.FromArgb(124, 58, 237)), "SVG quadratic curve paint");
                        Ok(HasColor(geometryPaint, new Rectangle(geometryRect.Left + 155, geometryRect.Top + 50, 95, 100), Color.FromArgb(234, 88, 12)), "SVG arc paint");
                        Ok(ColorDistance(geometryPaint.GetPixel(geometryRect.Left + 250, geometryRect.Top + 30), Color.FromArgb(13, 148, 136)) < 12, "SVG nested transform paint geometry");
                        var geometryHit = FragmentHitRect(view, "#geometry-target");
                        Ok(!geometryHit.IsEmpty && geometryHit.Left >= geometryRect.Left + 288 && geometryHit.Top >= geometryRect.Top + 154, "SVG transformed linked hit geometry");
                        var noneRect = BlockRect(view, BlockById(view.At, "aspect-none"));
                        var minRect = BlockRect(view, BlockById(view.At, "aspect-min"));
                        var sliceRect = BlockRect(view, BlockById(view.At, "aspect-slice"));
                        Ok(ColorDistance(geometryPaint.GetPixel(noneRect.Right - 4, noneRect.Top + 50), Color.FromArgb(220, 38, 38)) < 12, "SVG preserveAspectRatio none stretches paint");
                        Ok(ColorDistance(geometryPaint.GetPixel(minRect.Left + 50, minRect.Top + 50), Color.FromArgb(22, 163, 74)) < 12 && ColorDistance(geometryPaint.GetPixel(minRect.Right - 10, minRect.Top + 50), Color.White) < 12, "SVG xMinYMax meet alignment paint");
                        Ok(ColorDistance(geometryPaint.GetPixel(sliceRect.Left + 100, sliceRect.Top + 50), Color.FromArgb(37, 99, 235)) < 12, "SVG xMidYMax slice clipping paint");
                    }
                    var svgGradientPath = Path.Combine(pages, "svg-gradients.html");
                    var svgGradientPage = engine.Go(svgGradientPath);
                    var gradientVector = BlockById(svgGradientPage, "gradient-vector");
                    Ok(gradientVector.Vg != null && gradientVector.Vg.Ds.Count == 5, "SVG valid, invalid, and cyclic gradient definition count");
                    Eq(8, gradientVector.Vg.Ss.Count, "SVG gradient shape count");
                    var baseGradient = gradientVector.Vg.Ds[0];
                    Ok(baseGradient.I == "base-gradient" && baseGradient.K == "l" && baseGradient.U == "b" && baseGradient.P.Count == 4 && baseGradient.Ss.Count == 3, "SVG compact linear gradient aliases");
                    Ok(baseGradient.Ss[0].O == 0D && baseGradient.Ss[0].C == "#dc2626" && baseGradient.Ss[1].O == .5D && baseGradient.Ss[1].C == "#16a34a" && Math.Abs(baseGradient.Ss[1].A - .6D) < .001D && baseGradient.Ss[2].C == "#2563eb", "SVG gradient stops, stylesheet paint, opacity, and currentColor");
                    Ok(Math.Abs(baseGradient.M[0] - Math.Cos(10D * Math.PI / 180D)) < .0001D && Math.Abs(baseGradient.M[1] - Math.Sin(10D * Math.PI / 180D)) < .0001D, "SVG gradient transform matrix");
                    var derivedGradient = gradientVector.Vg.Ds[1];
                    Ok(derivedGradient.I == "derived-gradient" && derivedGradient.P[0] == 1D && derivedGradient.P[2] == 0D && derivedGradient.M[0] == 1D && derivedGradient.Ss.Count == 3 && derivedGradient.Ss[1].C == "#16a34a", "SVG inherited gradient coordinates, transform override, and stops");
                    var userGradient = gradientVector.Vg.Ds[2];
                    Ok(userGradient.U == "u" && userGradient.P[0] == 0D && userGradient.P[2] == 180D && userGradient.Ss[1].C == "#10b981" && Math.Abs(userGradient.Ss[1].A - .8D) < .001D, "SVG user-space gradient and inline stop style");
                    var radialGradient = gradientVector.Vg.Ds[3];
                    Ok(radialGradient.K == "r" && radialGradient.P.Count == 5 && radialGradient.P[0] == .5D && radialGradient.P[2] == .5D && radialGradient.P[3] == .35D && radialGradient.P[4] == .4D && radialGradient.Ss.Count == 3, "SVG radial gradient center, radius, focus, and stops");
                    Ok(gradientVector.Vg.Ss[0].Fd == baseGradient && gradientVector.Vg.Ss[1].Fd == derivedGradient && gradientVector.Vg.Ss[2].Fd == radialGradient && gradientVector.Vg.Ss[3].Fd == userGradient, "SVG resolved gradient fill references");
                    Ok(gradientVector.Vg.Ss[4].Sd == gradientVector.Vg.Ds[4] && gradientVector.Vg.Ss[4].F == "none", "SVG resolved gradient stroke reference");
                    Ok(gradientVector.Vg.Ss[5].Fd == derivedGradient && gradientVector.Vg.Ss[5].U.Fragment == "#gradient-target", "SVG linked gradient shape reference");
                    Ok(gradientVector.Vg.Ss[6].Fd == null && gradientVector.Vg.Ss[6].F == "none" && gradientVector.Vg.Ss[7].Fd == null && gradientVector.Vg.Ss[7].F == "none", "SVG unsupported spread and cyclic references fail closed");
                    Ok(svgGradientPage.Ls.Count == 1 && svgGradientPage.Ls[0].U.Fragment == "#gradient-target", "SVG gradient page link collection");
                    var gradientGraphic = engine.Ax(svgGradientPage, ById(svgGradientPage.D, "gradient-vector"));
                    Ok(gradientGraphic.K == "img" && gradientGraphic.N == "Linear and radial gradient samples", "SVG gradient accessibility name");
                    view.Size = new Size(720, 700);
                    view.Go(svgGradientPath);
                    using (var gradientPaint = Render(view))
                    {
                        var gradientRect = BlockRect(view, BlockById(view.At, "gradient-vector"));
                        Eq(new Size(360, 220), gradientRect.Size, "SVG gradient viewport size");
                        Ok(HasColor(gradientPaint, new Rectangle(gradientRect.Left + 8, gradientRect.Top + 8, 145, 55), Color.FromArgb(220, 38, 38)) && HasColor(gradientPaint, new Rectangle(gradientRect.Left + 8, gradientRect.Top + 8, 145, 55), Color.FromArgb(37, 99, 235)), "SVG transformed object-bounds linear gradient paint");
                        Ok(HasColor(gradientPaint, new Rectangle(gradientRect.Left + 28, gradientRect.Top + 72, 105, 105), Color.FromArgb(219, 39, 119)) && HasColor(gradientPaint, new Rectangle(gradientRect.Left + 28, gradientRect.Top + 72, 105, 105), Color.FromArgb(112, 26, 117)), "SVG focused radial gradient paint");
                        Ok(HasColor(gradientPaint, new Rectangle(gradientRect.Left + 15, gradientRect.Top + 135, 330, 65), Color.FromArgb(15, 118, 110)) && HasColor(gradientPaint, new Rectangle(gradientRect.Left + 15, gradientRect.Top + 135, 330, 65), Color.FromArgb(245, 158, 11)), "SVG user-space gradient stroke paint");
                        var gradientHit = FragmentHitRect(view, "#gradient-target");
                        Ok(!gradientHit.IsEmpty && gradientHit.Top >= gradientRect.Top + 180, "SVG transformed gradient link hit geometry");
                        Ok(ColorDistance(gradientPaint.GetPixel(gradientRect.Left + 20, gradientRect.Top + 198), Color.White) < 12 && ColorDistance(gradientPaint.GetPixel(gradientRect.Left + 70, gradientRect.Top + 198), Color.White) < 12, "SVG rejected gradient references remain unpainted");
                    }
                    var svgReusePath = Path.Combine(pages, "svg-use-clips.html");
                    var svgReusePage = engine.Go(svgReusePath);
                    var reuseVector = BlockById(svgReusePage, "reuse-vector");
                    Ok(reuseVector.Vg != null && reuseVector.Vg.Cs.Count == 4, "SVG valid and cyclic clip definition count");
                    Eq(14, reuseVector.Vg.Ss.Count, "SVG expanded use and clipped shape count");
                    var reuseShapes = reuseVector.Vg.Ss;
                    Ok(reuseShapes[0].K == "rect" && reuseShapes[0].F == "#dc2626" && reuseShapes[0].M[4] == 5D && reuseShapes[0].M[5] == 5D && reuseShapes[1].K == "circle" && reuseShapes[1].F == "#f8fafc", "SVG group use expansion, paint inheritance, and x/y matrix");
                    Ok(reuseShapes[2].K == "path" && reuseShapes[2].F == "#7c3aed" && reuseShapes[2].M[0] == 1.2D && reuseShapes[2].M[3] == 1.2D && reuseShapes[2].M[4] == 54D && reuseShapes[2].M[5] == 6D, "SVG symbol use expansion and transform matrix");
                    Ok(reuseShapes[4].M[4] == 85D && reuseShapes[6].M[4] == 113D && reuseShapes[4].F == "#0d9488" && reuseShapes[7].K == "circle", "SVG nested group use expansion and cumulative translation");
                    var circleClip = reuseVector.Vg.Cs[0];
                    var halfClip = reuseVector.Vg.Cs[1];
                    var useClip = reuseVector.Vg.Cs[2];
                    var groupClip = reuseVector.Vg.Cs[3];
                    Ok(circleClip.I == "circle-clip" && circleClip.U == "u" && circleClip.Ss.Count == 1 && circleClip.M[4] == 2D && circleClip.Ss[0].M[4] == 2D, "SVG user-space clip aliases and transform");
                    Ok(halfClip.I == "half-clip" && halfClip.U == "b" && halfClip.Ss.Count == 1 && halfClip.Ss[0].P[2] == .5D, "SVG object-bounds clip aliases");
                    Ok(useClip.I == "use-clip" && useClip.Ss.Count == 1 && useClip.Ss[0].K == "path" && useClip.Ss[0].M[4] == 125D && useClip.Ss[0].M[5] == 38D, "SVG reusable geometry inside clip path");
                    Ok(groupClip.Ss.Count == 2 && groupClip.Ss[0].K == "rect" && groupClip.Ss[1].K == "circle", "SVG multi-shape clip union");
                    Ok(reuseShapes[8].Cl && reuseShapes[8].Cp == circleClip && reuseShapes[9].Cl && reuseShapes[9].Cp == halfClip && reuseShapes[10].Cl && reuseShapes[10].Cp == useClip, "SVG presentation, inline, and stylesheet clip references");
                    Ok(reuseShapes[11].Cp == groupClip && reuseShapes[12].Cp == groupClip && reuseShapes[11].Cm[4] == 10D && reuseShapes[11].Cm[5] == 82D && reuseShapes[12].Cm[4] == 10D && reuseShapes[12].M[0] == .9D, "SVG inherited group clip attachment matrix");
                    Ok(reuseShapes[13].Cl && reuseShapes[13].Cp == null, "SVG unresolved clip reference fails closed");
                    Ok(svgReusePage.Ls.Count == 1 && svgReusePage.Ls[0].U.Fragment == "#clip-target" && reuseShapes[10].U.Fragment == "#clip-target", "SVG clipped use page link collection");
                    var reuseGraphic = engine.Ax(svgReusePage, ById(svgReusePage.D, "reuse-vector"));
                    Ok(reuseGraphic.K == "img" && reuseGraphic.N == "Reusable and clipped vector samples", "SVG reuse and clip accessibility name");
                    view.Size = new Size(720, 720);
                    view.Go(svgReusePath);
                    using (var reusePaint = Render(view))
                    {
                        var reuseRect = BlockRect(view, BlockById(view.At, "reuse-vector"));
                        Eq(new Size(360, 240), reuseRect.Size, "SVG reuse and clip viewport size");
                        Ok(ColorDistance(reusePaint.GetPixel(reuseRect.Left + 14, reuseRect.Top + 14), Color.FromArgb(220, 38, 38)) < 12 && ColorDistance(reusePaint.GetPixel(reuseRect.Left + 46, reuseRect.Top + 26), Color.FromArgb(248, 250, 252)) < 12, "SVG reused group paint");
                        Ok(ColorDistance(reusePaint.GetPixel(reuseRect.Left + 64, reuseRect.Top + 106), Color.FromArgb(234, 88, 12)) < 12 && ColorDistance(reusePaint.GetPixel(reuseRect.Left + 14, reuseRect.Top + 78), Color.White) < 12, "SVG transformed user-space circle clip paint");
                        Ok(ColorDistance(reusePaint.GetPixel(reuseRect.Left + 150, reuseRect.Top + 106), Color.FromArgb(22, 163, 74)) < 12 && ColorDistance(reusePaint.GetPixel(reuseRect.Left + 210, reuseRect.Top + 106), Color.White) < 12, "SVG object-bounds half clip paint");
                        Ok(ColorDistance(reusePaint.GetPixel(reuseRect.Left + 30, reuseRect.Top + 185), Color.FromArgb(219, 39, 119)) < 12 && ColorDistance(reusePaint.GetPixel(reuseRect.Left + 20, reuseRect.Top + 218), Color.White) < 12, "SVG inherited group clip paint");
                        var clippedHit = FragmentHitRect(view, "#clip-target");
                        Ok(!clippedHit.IsEmpty && clippedHit.Left >= reuseRect.Left + 248 && clippedHit.Right <= reuseRect.Left + 332 && clippedHit.Top >= reuseRect.Top + 74, "SVG clipped link hit bounds");
                        Ok(ColorDistance(reusePaint.GetPixel(reuseRect.Left + 200, reuseRect.Top + 194), Color.White) < 12, "SVG unresolved clip remains unpainted");
                    }
                    var svgStrokePath = Path.Combine(pages, "svg-strokes.html");
                    var svgStrokePage = engine.Go(svgStrokePath);
                    var strokeVector = BlockById(svgStrokePage, "stroke-vector");
                    Ok(strokeVector.Vg != null && strokeVector.Vg.Cs.Count == 1, "SVG stroke fixture and clip model");
                    Eq(13, strokeVector.Vg.Ss.Count, "SVG stroke and winding shape count");
                    var strokeShapes = strokeVector.Vg.Ss;
                    Ok(strokeShapes[0].Fr == "e" && strokeShapes[1].Fr == "n" && strokeShapes[2].Cp == strokeVector.Vg.Cs[0] && strokeVector.Vg.Cs[0].Ss[0].Fr == "e", "SVG fill-rule and clip-rule aliases");
                    Ok(strokeShapes[3].Lc == "b" && strokeShapes[4].Lc == "r" && strokeShapes[5].Lc == "s" && strokeShapes[5].U.Fragment == "#stroke-target", "SVG line-cap aliases and linked square cap");
                    Ok(strokeShapes[6].Lj == "m" && strokeShapes[6].Ml == 64D && strokeShapes[7].Lj == "r" && strokeShapes[8].Lj == "b", "SVG line-join and bounded miter aliases");
                    Ok(strokeShapes[9].F == "none" && strokeShapes[9].S == "#0f766e" && strokeShapes[9].W == 5D && strokeShapes[9].Lc == "r" && strokeShapes[9].Do == 3D && strokeShapes[9].Da.Count == 6 && strokeShapes[9].Da[0] == 8D && strokeShapes[9].Da[1] == 4D && strokeShapes[9].Da[2] == 2D && strokeShapes[9].Da[3] == 8D, "SVG stylesheet variables and odd dash repetition");
                    Ok(strokeShapes[10].F == "#f97316" && strokeShapes[10].S == "#7c3aed" && strokeShapes[10].W == 2D && strokeShapes[10].Fr == "e" && strokeShapes[10].Lj == "b", "SVG stylesheet, inline, and important paint cascade");
                    Ok(strokeShapes[11].Da.Count == 2 && strokeShapes[11].Da[0] == 5D && strokeShapes[11].Da[1] == 3D && strokeShapes[12].Da.Count == 0, "SVG over-limit inherited and explicit none dash state");
                    var strokeGraphic = engine.Ax(svgStrokePage, ById(svgStrokePage.D, "stroke-vector"));
                    Ok(strokeGraphic.K == "img" && strokeGraphic.N == "Stroke and winding samples", "SVG stroke accessibility name");
                    view.Size = new Size(720, 760);
                    view.Go(svgStrokePath);
                    using (var strokePaint = Render(view))
                    {
                        var strokeRect = BlockRect(view, BlockById(view.At, "stroke-vector"));
                        Eq(new Size(360, 260), strokeRect.Size, "SVG stroke viewport size");
                        Ok(ColorDistance(strokePaint.GetPixel(strokeRect.Left + 16, strokeRect.Top + 20), Color.FromArgb(220, 38, 38)) < 12 && ColorDistance(strokePaint.GetPixel(strokeRect.Left + 54, strokeRect.Top + 40), Color.White) < 12, "SVG even-odd painted hole");
                        Ok(ColorDistance(strokePaint.GetPixel(strokeRect.Left + 174, strokeRect.Top + 40), Color.FromArgb(22, 163, 74)) < 12, "SVG nonzero painted center");
                        Ok(ColorDistance(strokePaint.GetPixel(strokeRect.Left + 260, strokeRect.Top + 20), Color.FromArgb(245, 158, 11)) < 12 && ColorDistance(strokePaint.GetPixel(strokeRect.Left + 300, strokeRect.Top + 40), Color.White) < 12, "SVG even-odd clip region");
                        Ok(ColorDistance(strokePaint.GetPixel(strokeRect.Left + 16, strokeRect.Top + 104), Color.White) < 12 && ColorDistance(strokePaint.GetPixel(strokeRect.Left + 124, strokeRect.Top + 104), Color.FromArgb(219, 39, 119)) < 20 && ColorDistance(strokePaint.GetPixel(strokeRect.Left + 244, strokeRect.Top + 104), Color.FromArgb(124, 58, 237)) < 20, "SVG butt, round, and square cap paint");
                        Ok(ColorDistance(strokePaint.GetPixel(strokeRect.Left + 50, strokeRect.Top + 232), Color.White) < 12 && ColorDistance(strokePaint.GetPixel(strokeRect.Left + 16, strokeRect.Top + 220), Color.FromArgb(249, 115, 22)) < 20, "SVG stylesheet even-odd fill and important stroke paint");
                        var squareHit = FragmentHitRect(view, "#stroke-target");
                        Ok(!squareHit.IsEmpty && squareHit.Left <= strokeRect.Left + 244 && squareHit.Right >= strokeRect.Left + 328 && squareHit.Top <= strokeRect.Top + 96, "SVG square-cap link hit bounds");
                    }
                    var radiusPath = Path.Combine(pages, "border-radius.html");
                    var radiusPage = engine.Go(radiusPath);
                    var solidRadius = BlockById(radiusPage, "solid").S;
                    Ok(solidRadius.R0 == 30 && solidRadius.R1 == 12 && solidRadius.R2 == 24 && solidRadius.R3 == 4 &&
                        solidRadius.Q0 == 20 && solidRadius.Q1 == 8 && solidRadius.Q2 == 30 && solidRadius.Q3 == 4, "elliptical border-radius compact aliases");
                    var dashedRadius = BlockById(radiusPage, "dashed").S;
                    Ok(dashedRadius.R0 == -50 && dashedRadius.R1 == -25 && dashedRadius.R2 == -50 && dashedRadius.R3 == -25 &&
                        dashedRadius.Q0 == 18 && dashedRadius.Q1 == 10 && dashedRadius.Q2 == 18 && dashedRadius.Q3 == 10, "percentage border-radius compact aliases");
                    var fieldRadius = FieldById(radiusPage, "field").S;
                    Ok(fieldRadius.R0 == 18 && fieldRadius.R1 == 36 && fieldRadius.R2 == 18 && fieldRadius.R3 == 36 &&
                        fieldRadius.Q0 == 18 && fieldRadius.Q1 == 36 && fieldRadius.Q2 == 18 && fieldRadius.Q3 == 36, "custom-property field radius aliases");
                    var atomicRadius = BlockById(radiusPage, "atomic").S;
                    Ok(atomicRadius.R0 == 1 && atomicRadius.R1 == 2 && atomicRadius.R2 == 3 && atomicRadius.R3 == 4 &&
                        atomicRadius.Q0 == 5 && atomicRadius.Q1 == 6 && atomicRadius.Q2 == 7 && atomicRadius.Q3 == 8, "invalid radius shorthand preserves atomic state");
                    var priorityRadius = BlockById(radiusPage, "priority").S;
                    Ok(priorityRadius.R0 == 22 && priorityRadius.Q0 == 11 && priorityRadius.R1 == 3 && priorityRadius.Q1 == 3, "important corner radius cascade");
                    var resetRadius = BlockById(radiusPage, "reset").S;
                    Ok(resetRadius.R0 == 0 && resetRadius.R1 == 0 && resetRadius.Q2 == 0 && resetRadius.Q3 == 0, "radius reset state");
                    var boundedRadius = BlockById(radiusPage, "bounded").S;
                    Ok(boundedRadius.R0 == 2000 && boundedRadius.R1 == -1000 && boundedRadius.Q2 == 2000 && boundedRadius.Q3 == -1000, "bounded oversized radius state");
                    view.Size = new Size(720, 1200);
                    view.Go(radiusPath);
                    using (var radiusPaint = Render(view))
                    {
                        var solidOuter = BlockOuterRect(view, BlockById(view.At, "solid"));
                        Ok(ColorDistance(radiusPaint.GetPixel(solidOuter.Left, solidOuter.Top), Color.White) < 12, "rounded solid corner excludes background");
                        Ok(ColorDistance(radiusPaint.GetPixel(solidOuter.Left + solidOuter.Width / 2, solidOuter.Top + 2), Color.FromArgb(22, 101, 52)) < 24, "rounded solid border paint");
                        Ok(ColorDistance(CenterPixel(radiusPaint, solidOuter), Color.FromArgb(254, 243, 199)) < 12, "rounded solid background paint");
                        Eq(solidOuter, HitRect(view, "second.html"), "rounded link hit bounds");
                        var clipper = BlockById(view.At, "clipper");
                        var clipContent = BlockRect(view, clipper);
                        Eq(clipContent, OwnedClip(view, BlockById(view.At, "clip-child"), "p"), "rounded overflow keeps bounded clip geometry");
                        var clipCorner = radiusPaint.GetPixel(clipContent.Left + 1, clipContent.Top + 1);
                        var clipCenter = CenterPixel(radiusPaint, clipContent);
                        Ok(ColorDistance(clipCorner, Color.White) < 24 &&
                            ColorDistance(clipCenter, Color.FromArgb(239, 68, 68)) < 12, "rounded overflow clips child corners " + clipCorner + " / " + clipCenter);
                        var dashedOuter = BlockOuterRect(view, BlockById(view.At, "dashed"));
                        Ok(ColorDistance(radiusPaint.GetPixel(dashedOuter.Left, dashedOuter.Top), Color.White) < 24 &&
                            HasColor(radiusPaint, dashedOuter, Color.FromArgb(3, 105, 161)), "rounded dashed border paint");
                        var fieldOuter = BlockRect(view, FieldBlock(view.At, FieldById(view.At, "field")));
                        Ok(ColorDistance(radiusPaint.GetPixel(fieldOuter.Left, fieldOuter.Top), Color.White) < 24 &&
                            ColorDistance(CenterPixel(radiusPaint, fieldOuter), Color.FromArgb(220, 252, 231)) < 12, "rounded field paint");
                    }
                    view.Size = new Size(300, 1200);
                    Paint(view);
                    Ok(BlockOuterRect(view, BlockById(view.At, "dashed")).Right <= view.ClientSize.Width - 1, "rounded boxes reflow narrowly");

                    var logicalRadiusPath = Path.Combine(pages, "logical-radii.html");
                    var logicalRadiusEngine = new E();
                    var logicalRadiusPage = logicalRadiusEngine.Go(logicalRadiusPath);
                    var logicalRadiusMapping = BlockById(logicalRadiusPage, "mapping").S;
                    Ok(logicalRadiusMapping.R0 == 11 && logicalRadiusMapping.Q0 == 12 && logicalRadiusMapping.R1 == 13 && logicalRadiusMapping.Q1 == 14 &&
                        logicalRadiusMapping.R2 == 15 && logicalRadiusMapping.Q2 == 16 && logicalRadiusMapping.R3 == 17 && logicalRadiusMapping.Q3 == 18,
                        "logical corner radius physical mapping");
                    var logicalRadiusSource = BlockById(logicalRadiusPage, "source").S;
                    Ok(logicalRadiusSource.R0 == 20 && logicalRadiusSource.Q0 == 21 && logicalRadiusSource.R2 == 23 && logicalRadiusSource.Q2 == 24,
                        "physical and logical corner source order");
                    var logicalRadiusPriority = BlockById(logicalRadiusPage, "priority").S;
                    Ok(logicalRadiusPriority.R1 == 25 && logicalRadiusPriority.Q1 == 26 && logicalRadiusPriority.R0 == 3 && logicalRadiusPriority.Q3 == 3,
                        "important logical corner radius cascade");
                    var logicalRadiusVariable = BlockById(logicalRadiusPage, "variables").S;
                    Ok(logicalRadiusVariable.R3 == -30 && logicalRadiusVariable.Q3 == -40, "logical corner radius variable percentages");
                    var logicalRadiusInherited = BlockById(logicalRadiusPage, "inherit-all").S;
                    var logicalCornerInherited = BlockById(logicalRadiusPage, "inherit-corners").S;
                    Ok(logicalRadiusInherited.R0 == 31 && logicalRadiusInherited.Q1 == 42 && logicalRadiusInherited.R2 == 33 && logicalRadiusInherited.Q3 == 44 &&
                        logicalCornerInherited.R0 == 31 && logicalCornerInherited.Q1 == 42 && logicalCornerInherited.R2 == 33 && logicalCornerInherited.Q3 == 44,
                        "physical shorthand and logical corner radius inheritance");
                    var logicalRadiusReset = BlockById(logicalRadiusPage, "reset").S;
                    Ok(logicalRadiusReset.R0 == 0 && logicalRadiusReset.R1 == 0 && logicalRadiusReset.R2 == 0 && logicalRadiusReset.R3 == 0 &&
                        logicalRadiusReset.Q0 == 0 && logicalRadiusReset.Q1 == 0 && logicalRadiusReset.Q2 == 0 && logicalRadiusReset.Q3 == 0,
                        "logical corner radius reset keywords");
                    var logicalRadiusInvalid = BlockById(logicalRadiusPage, "invalid").S;
                    Ok(logicalRadiusInvalid.R0 == 5 && logicalRadiusInvalid.R1 == 6 && logicalRadiusInvalid.R2 == 7 && logicalRadiusInvalid.R3 == 8 &&
                        logicalRadiusInvalid.Q0 == 9 && logicalRadiusInvalid.Q1 == 10 && logicalRadiusInvalid.Q2 == 11 && logicalRadiusInvalid.Q3 == 12,
                        "malformed logical corner radii preserve atomic state");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "set-short")), "short physical and logical radius aliases");
                    var logicalRadiusCommand = BlockById(logicalRadiusPage, "command").S;
                    Ok(logicalRadiusCommand.R0 == 30 && logicalRadiusCommand.Q0 == 31 && logicalRadiusCommand.R1 == 32 && logicalRadiusCommand.Q1 == 33 &&
                        logicalRadiusCommand.R2 == 36 && logicalRadiusCommand.Q2 == 37 && logicalRadiusCommand.R3 == 34 && logicalRadiusCommand.Q3 == 35,
                        "short radius aliases reach compact records");
                    Eq(0, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "set-invalid")), "malformed radius commands rejected");
                    Ok(BlockById(logicalRadiusPage, "command").S.R0 == 30 && BlockById(logicalRadiusPage, "command").S.Q2 == 37,
                        "rejected radius commands preserve compact records");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "clear-all")), "radius DOM reset after short aliases");
                    logicalRadiusCommand = BlockById(logicalRadiusPage, "command").S;
                    Ok(logicalRadiusCommand.R0 == 2 && logicalRadiusCommand.R1 == 3 && logicalRadiusCommand.R2 == 4 && logicalRadiusCommand.R3 == 5 &&
                        logicalRadiusCommand.Q0 == 6 && logicalRadiusCommand.Q1 == 7 && logicalRadiusCommand.Q2 == 8 && logicalRadiusCommand.Q3 == 9,
                        "radius DOM reset restores authored ellipse");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "set-readable")), "readable physical and logical radius aliases");
                    logicalRadiusCommand = BlockById(logicalRadiusPage, "command").S;
                    Ok(logicalRadiusCommand.R0 == 50 && logicalRadiusCommand.Q0 == 51 && logicalRadiusCommand.R1 == 52 && logicalRadiusCommand.Q1 == 53 &&
                        logicalRadiusCommand.R2 == 56 && logicalRadiusCommand.Q2 == 57 && logicalRadiusCommand.R3 == 54 && logicalRadiusCommand.Q3 == 55,
                        "readable radius aliases reach compact records");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "clear-all")), "radius DOM reset after readable aliases");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "set-dom")), "familiar physical and logical radius DOM styles");
                    logicalRadiusCommand = BlockById(logicalRadiusPage, "command").S;
                    Ok(logicalRadiusCommand.R0 == 70 && logicalRadiusCommand.Q0 == 71 && logicalRadiusCommand.R1 == 72 && logicalRadiusCommand.Q1 == 73 &&
                        logicalRadiusCommand.R2 == 76 && logicalRadiusCommand.Q2 == 77 && logicalRadiusCommand.R3 == 74 && logicalRadiusCommand.Q3 == 75,
                        "familiar radius DOM styles reach compact records");
                    Eq(9, logicalRadiusEngine.Ck(logicalRadiusPage, ById(logicalRadiusPage.D, "clear-all")), "radius DOM authored restore");
                    view.Size = new Size(720, 1500);
                    view.Go(logicalRadiusPath);
                    using (var logicalRadiusPaint = Render(view))
                    {
                        var logicalPaintOuter = BlockOuterRect(view, BlockById(view.At, "paint"));
                        Ok(ColorDistance(logicalRadiusPaint.GetPixel(logicalPaintOuter.Left + 1, logicalPaintOuter.Top + 1), Color.White) < 24 &&
                            ColorDistance(logicalRadiusPaint.GetPixel(logicalPaintOuter.Right - 2, logicalPaintOuter.Top + 2), Color.FromArgb(23, 107, 77)) < 24 &&
                            ColorDistance(CenterPixel(logicalRadiusPaint, logicalPaintOuter), Color.FromArgb(255, 244, 204)) < 16,
                            "logical asymmetric corner radius paint");
                    }
                    var shadowPath = Path.Combine(pages, "box-shadows.html");
                    var shadowPage = engine.Go(shadowPath);
                    var outerShadow = BlockById(shadowPage, "outer").S.Sw;
                    Ok(outerShadow.Count == 2 && outerShadow[0].X == 12 && outerShadow[0].Y == 8 && outerShadow[0].B == 0 && outerShadow[0].R == 0 && outerShadow[0].C == "#dc2626" && !outerShadow[0].I &&
                        outerShadow[1].X == -8 && outerShadow[1].C == "#2563eb", "outer box-shadow compact aliases and authored order");
                    var softShadow = BlockById(shadowPage, "soft").S.Sw[0];
                    Ok(softShadow.X == 0 && softShadow.Y == 0 && softShadow.B == 12 && softShadow.R == 4 && softShadow.C == "rgba(15, 23, 42, 0.5)", "blur, spread, and alpha color shadow aliases");
                    var insetShadow = BlockById(shadowPage, "inset").S.Sw[0];
                    Ok(insetShadow.I && insetShadow.X == 10 && insetShadow.B == 0 && insetShadow.R == 0, "inset box-shadow compact alias");
                    var fieldShadow = FieldById(shadowPage, "field").S.Sw;
                    Ok(fieldShadow.Count == 2 && !fieldShadow[0].I && fieldShadow[1].I && fieldShadow[1].X == 7, "field outer and inset shadow model");
                    Ok(BlockById(shadowPage, "shadow-clip-child").S.Op == 50 && BlockById(shadowPage, "shadow-clip-child").S.Sw.Count == 1, "shadow effective opacity and overflow fixture model");
                    var atomicShadow = BlockById(shadowPage, "atomic").S.Sw;
                    Ok(atomicShadow.Count == 1 && atomicShadow[0].X == 3 && atomicShadow[0].Y == 4 && atomicShadow[0].B == 5 && atomicShadow[0].R == 6 && atomicShadow[0].C == "#be123c", "invalid functional color preserves shadow list atomically");
                    var priorityShadow = BlockById(shadowPage, "priority").S.Sw;
                    Ok(priorityShadow.Count == 1 && priorityShadow[0].X == 7 && priorityShadow[0].Y == 5 && priorityShadow[0].C == "#0f4c81", "important custom-property shadow cascade");
                    Ok(BlockById(shadowPage, "current").S.Sw[0].C == "currentColor" && BlockById(shadowPage, "reset").S.Sw.Count == 0, "currentColor and none shadow state");
                    var boundedShadow = BlockById(shadowPage, "bounded").S.Sw[0];
                    Ok(boundedShadow.X == 512 && boundedShadow.Y == -512 && boundedShadow.B == 128 && boundedShadow.R == -512, "bounded oversized shadow aliases");
                    var limitShadow = BlockById(shadowPage, "limit").S.Sw;
                    Ok(limitShadow.Count == 1 && limitShadow[0].X == 2 && limitShadow[0].Y == 3 && BlockById(shadowPage, "child").S.Sw.Count == 0, "shadow list cap and noninheritance");
                    view.Size = new Size(720, 1200);
                    view.Go(shadowPath);
                    using (var shadowPaint = Render(view))
                    {
                        var outer = BlockOuterRect(view, BlockById(view.At, "outer"));
                        Ok(ColorDistance(shadowPaint.GetPixel(outer.Right + 6, outer.Top + outer.Height / 2 + 4), Color.FromArgb(220, 38, 38)) < 24 &&
                            ColorDistance(shadowPaint.GetPixel(outer.Left - 5, outer.Top + outer.Height / 2), Color.FromArgb(37, 99, 235)) < 24, "ordered outer shadow paint");
                        Eq(outer, HitRect(view, "second.html"), "shadows do not expand link hit geometry");
                        var soft = BlockOuterRect(view, BlockById(view.At, "soft"));
                        Ok(ColorDistance(shadowPaint.GetPixel(soft.Right + 8, soft.Top + soft.Height / 2), Color.White) > 12, "bounded blur shadow paint");
                        var inset = BlockOuterRect(view, BlockById(view.At, "inset"));
                        Ok(ColorDistance(shadowPaint.GetPixel(inset.Left + 4, inset.Top + inset.Height / 2), Color.FromArgb(22, 163, 74)) < 24 &&
                            ColorDistance(CenterPixel(shadowPaint, inset), Color.FromArgb(253, 230, 138)) < 12, "inset shadow and background paint");
                        var alpha = BlockOuterRect(view, BlockById(view.At, "alpha"));
                        Ok(ColorDistance(shadowPaint.GetPixel(alpha.Right + 4, alpha.Top + alpha.Height / 2), Color.FromArgb(135, 139, 149)) < 20, "space/slash alpha shadow color paint");
                        var field = BlockRect(view, FieldBlock(view.At, FieldById(view.At, "field")));
                        Ok(ColorDistance(shadowPaint.GetPixel(field.Left + 5, field.Top + field.Height / 2), Color.FromArgb(13, 148, 136)) < 30 &&
                            ColorDistance(CenterPixel(shadowPaint, field), Color.FromArgb(207, 250, 254)) < 18 &&
                            ColorDistance(shadowPaint.GetPixel(field.Right + 3, field.Top + field.Height / 2 + 2), Color.FromArgb(124, 58, 237)) < 30, "field outer and inset shadow paint");
                        var clipper = BlockRect(view, BlockById(view.At, "shadow-clipper"));
                        Ok(ColorDistance(shadowPaint.GetPixel(clipper.Left + 160, clipper.Top + clipper.Height / 2), Color.FromArgb(223, 135, 139)) < 30 &&
                            ColorDistance(shadowPaint.GetPixel(clipper.Right + 3, clipper.Top + clipper.Height / 2), Color.White) < 18, "effective shadow opacity and ancestor overflow clip paint");
                    }
                    view.Size = new Size(260, 1200);
                    Paint(view);
                    Ok(BlockOuterRect(view, BlockById(view.At, "outer")).Right <= view.ClientSize.Width - 1, "shadow boxes reflow without layout expansion");
                    var gradientPath = Path.Combine(pages, "background-gradients.html");
                    var gradientPage = engine.Go(gradientPath);
                    Ok(gradientPage.S.Gi != null && gradientPage.S.Gi.K == "l" && gradientPage.S.Gi.A == 90 && gradientPage.S.Gi.Ss.Count == 2, "page gradient compact model");
                    var linearGradient = BlockById(gradientPage, "linear").S.Gi;
                    Ok(linearGradient != null && linearGradient.K == "l" && linearGradient.A == 90 && linearGradient.X == 50 && linearGradient.Y == 50 && linearGradient.Ss.Count == 3 &&
                        linearGradient.Ss[0].C == "#dc2626" && linearGradient.Ss[0].P == 0 && linearGradient.Ss[1].P == 50 && linearGradient.Ss[2].P == 100, "linear gradient aliases and stops");
                    var cssRadialGradient = BlockById(gradientPage, "radial").S.Gi;
                    Ok(cssRadialGradient != null && cssRadialGradient.K == "r" && cssRadialGradient.X == 25 && cssRadialGradient.Y == 50 && cssRadialGradient.Ss.Count == 2, "radial gradient center aliases");
                    var gradientField = FieldById(gradientPage, "field").S.Gi;
                    Ok(gradientField != null && gradientField.A == 90 && gradientField.Ss.Count == 2, "field gradient model");
                    var atomicGradient = BlockById(gradientPage, "atomic").S.Gi;
                    Ok(atomicGradient != null && atomicGradient.A == 90 && atomicGradient.Ss[0].P == 10 && atomicGradient.Ss[1].P == 90, "invalid gradient preserves prior state atomically");
                    var priorityGradient = BlockById(gradientPage, "priority").S.Gi;
                    Ok(priorityGradient != null && priorityGradient.A == 90 && priorityGradient.Ss[0].C == "#0f4c81" && priorityGradient.Ss[1].C == "#16a34a", "important custom-property gradient cascade");
                    Ok(BlockById(gradientPage, "current").S.Gi.Ss[0].C == "currentColor" && BlockById(gradientPage, "reset").S.Gi == null, "currentColor and none gradient state");
                    var boundedGradient = BlockById(gradientPage, "bounded").S.Gi;
                    Ok(boundedGradient.Ss[0].P == 0 && boundedGradient.Ss[1].P == 100, "bounded gradient stop positions");
                    var normalizedGradient = BlockById(gradientPage, "normalized").S.Gi;
                    Ok(normalizedGradient.Ss.Count == 4 && normalizedGradient.Ss[0].P == 0 && normalizedGradient.Ss[1].P == 40 && normalizedGradient.Ss[2].P == 80 && normalizedGradient.Ss[3].P == 100, "implicit gradient stop normalization");
                    var limitedGradient = BlockById(gradientPage, "limit").S.Gi;
                    var shorthandGradient = BlockById(gradientPage, "shorthand").S;
                    Ok(limitedGradient.Ss.Count == 2 && shorthandGradient.Gi != null && shorthandGradient.Bg.Length == 0 && BlockById(gradientPage, "child").S.Gi == null,
                        "gradient stop cap, shorthand reset, and noninheritance");
                    Ok(BlockById(gradientPage, "alpha").S.Op == 50, "gradient effective opacity model");
                    view.Size = new Size(720, 1200);
                    view.Go(gradientPath);
                    using (var gradientPaint = Render(view))
                    {
                        Ok(ColorDistance(gradientPaint.GetPixel(2, 2), Color.White) < 20 &&
                            ColorDistance(gradientPaint.GetPixel(717, 2), Color.FromArgb(226, 232, 240)) < 30, "page background gradient paint");
                        var linear = BlockOuterRect(view, BlockById(view.At, "linear"));
                        Ok(ColorDistance(gradientPaint.GetPixel(linear.Left + 8, linear.Top + linear.Height / 2), Color.FromArgb(220, 38, 38)) < 45 &&
                            ColorDistance(gradientPaint.GetPixel(linear.Left + linear.Width / 2, linear.Top + linear.Height / 2), Color.FromArgb(245, 158, 11)) < 45 &&
                            ColorDistance(gradientPaint.GetPixel(linear.Right - 8, linear.Top + linear.Height / 2), Color.FromArgb(22, 163, 74)) < 45, "three-stop linear gradient paint");
                        Ok(ColorDistance(gradientPaint.GetPixel(linear.Left, linear.Top), Color.FromArgb(220, 38, 38)) > 80, "rounded gradient corner clip");
                        Eq(linear, HitRect(view, "second.html"), "gradient does not expand link hit geometry");
                        var vertical = BlockOuterRect(view, BlockById(view.At, "vertical"));
                        Ok(ColorDistance(gradientPaint.GetPixel(vertical.Left + vertical.Width / 2, vertical.Top + 6), Color.FromArgb(37, 99, 235)) < 55 &&
                            ColorDistance(gradientPaint.GetPixel(vertical.Left + vertical.Width / 2, vertical.Bottom - 7), Color.FromArgb(220, 38, 38)) < 55, "directional vertical gradient paint");
                        var radial = BlockOuterRect(view, BlockById(view.At, "radial"));
                        Ok(ColorDistance(gradientPaint.GetPixel(radial.Left + radial.Width / 4, radial.Top + radial.Height / 2), Color.FromArgb(254, 240, 138)) < 35 &&
                            ColorDistance(gradientPaint.GetPixel(radial.Right - 10, radial.Top + radial.Height / 2), Color.FromArgb(124, 58, 237)) < 50, "off-center radial gradient paint");
                        var field = BlockRect(view, FieldBlock(view.At, FieldById(view.At, "field")));
                        Ok(ColorDistance(gradientPaint.GetPixel(field.Left + 10, field.Bottom - 8), Color.FromArgb(153, 246, 228)) < 35 &&
                            ColorDistance(gradientPaint.GetPixel(field.Right - 10, field.Bottom - 8), Color.FromArgb(103, 232, 249)) < 35, "rounded field gradient paint");
                    }
                    view.Size = new Size(280, 1200);
                    Paint(view);
                    Ok(BlockOuterRect(view, BlockById(view.At, "linear")).Right <= view.ClientSize.Width - 1, "gradient boxes reflow without layout expansion");

                    var bitmapPath = Path.Combine(pages, "background-images.html");
                    var bitmapEngine = new E();
                    var bitmapPage = bitmapEngine.Go(bitmapPath);
                    Ok(bitmapPage.S.Bi != null && bitmapPage.S.Bi.R == "n" && bitmapPage.S.Bi.X == 100 && bitmapPage.S.Bi.Y == 100 &&
                        bitmapPage.S.Bi.Xp && bitmapPage.S.Bi.Yp && bitmapPage.S.Bi.W == 24 && bitmapPage.S.Bi.H == 24,
                        "page bitmap compact aliases");
                    var repeatBitmap = BlockById(bitmapPage, "repeat").S.Bi;
                    Ok(repeatBitmap != null && repeatBitmap.U != null && repeatBitmap.U.Scheme == "data" && repeatBitmap.R == "b" &&
                        repeatBitmap.W == 12 && repeatBitmap.H == 12, "custom-property bitmap and repeat aliases");
                    var positionedBitmap = BlockById(bitmapPage, "no-repeat").S.Bi;
                    Ok(positionedBitmap != null && positionedBitmap.R == "n" && positionedBitmap.X == 100 && positionedBitmap.Y == 100 &&
                        positionedBitmap.W == 30 && positionedBitmap.H == 20, "positioned no-repeat bitmap aliases");
                    Ok(BlockById(bitmapPage, "repeat-x").S.Bi.R == "x" && BlockById(bitmapPage, "repeat-y").S.Bi.R == "y",
                        "axis repeat aliases");
                    Ok(BlockById(bitmapPage, "cover").S.Bi.K == "c" && BlockById(bitmapPage, "contain").S.Bi.K == "n",
                        "cover and contain aliases");
                    Ok(BlockById(bitmapPage, "atomic").S.Bi.U == repeatBitmap.U, "invalid bitmap URL preserves prior state atomically");
                    var atomicPosition = BlockById(bitmapPage, "position-atomic").S.Bi;
                    Ok(atomicPosition.X == 25 && atomicPosition.Y == 75 && atomicPosition.Xp && atomicPosition.Yp,
                        "invalid bitmap position preserves prior state atomically");
                    var atomicSize = BlockById(bitmapPage, "size-atomic").S.Bi;
                    Ok(atomicSize.W == 40 && atomicSize.Wp && atomicSize.Ha, "invalid bitmap size preserves prior state atomically");
                    var shorthandBitmap = BlockById(bitmapPage, "shorthand").S;
                    Ok(shorthandBitmap.Bg == "#ffffff" && shorthandBitmap.Bi != null && shorthandBitmap.Bi.R == "n" &&
                        shorthandBitmap.Bi.X == 100 && shorthandBitmap.Bi.Y == 100 && shorthandBitmap.Bi.W == 26 && shorthandBitmap.Bi.H == 22,
                        "atomic bitmap background shorthand");
                    var resetBitmap = BlockById(bitmapPage, "reset").S.Bi;
                    Ok((resetBitmap == null || resetBitmap.U == null) && BlockById(bitmapPage, "child").S.Bi == null,
                        "bitmap reset and noninheritance");
                    Eq(new Uri(Path.GetFullPath(Path.Combine(root, "assets", "lea.ico"))), BlockById(bitmapPage, "external-image").S.Bi.U,
                        "linked stylesheet bitmap origin");
                    Ok(BlockById(bitmapPage, "dynamic").S.Bi == null, "wide bitmap media state");
                    bitmapEngine.Vw = 350;
                    Ok(BlockById(bitmapPage, "dynamic").S.Bi != null && BlockById(bitmapPage, "dynamic").S.Bi.K == "c",
                        "responsive bitmap media state");

                    BitmapBackgroundPaintSmoke(bitmapPath);

                    var objectPath = Path.Combine(pages, "object-fit.html");
                    var objectEngine = new E();
                    var objectPage = objectEngine.Go(objectPath);
                    Eq("f", BlockById(objectPage, "fill").S.Of, "fill object-fit alias");
                    Eq("c", BlockById(objectPage, "contain").S.Of, "contain object-fit alias");
                    Eq("v", BlockById(objectPage, "cover-left").S.Of, "cover object-fit alias");
                    Eq("n", BlockById(objectPage, "none").S.Of, "none object-fit alias");
                    Eq("s", BlockById(objectPage, "scale-down").S.Of, "scale-down object-fit alias");
                    var coverLeftStyle = BlockById(objectPage, "cover-left").S;
                    var coverRightStyle = BlockById(objectPage, "cover-right").S;
                    Ok(coverLeftStyle.Ox == 0 && coverLeftStyle.Oy == 0 && coverLeftStyle.Oxp && coverLeftStyle.Oyp &&
                        coverRightStyle.Ox == 100 && coverRightStyle.Oy == 100 && coverRightStyle.Oxp && coverRightStyle.Oyp,
                        "keyword object-position aliases");
                    var noneStyle = BlockById(objectPage, "none").S;
                    Ok(noneStyle.Ox == 25 && noneStyle.Oy == 75 && noneStyle.Oxp && noneStyle.Oyp,
                        "percentage object-position aliases");
                    var atomicPositionStyle = BlockById(objectPage, "position-atomic").S;
                    Ok(atomicPositionStyle.Ox == 13 && atomicPositionStyle.Oy == 7 && !atomicPositionStyle.Oxp && !atomicPositionStyle.Oyp,
                        "pixel position and invalid preservation");
                    Ok(BlockById(objectPage, "atomic").S.Of == "c" && BlockById(objectPage, "reset").S.Of == "f" &&
                        BlockById(objectPage, "reset").S.Ox == 50 && BlockById(objectPage, "reset").S.Oy == 50,
                        "atomic object fit and reset state");
                    Ok(objectPage.S.Of == "v" && objectPage.S.Ox == 100 && objectPage.S.Oy == 100 && BlockById(objectPage, "child").S.Of == "f" &&
                        BlockById(objectPage, "child").S.Ox == 50 && BlockById(objectPage, "child").S.Oy == 50,
                        "object fit and position noninheritance");
                    var variableObjectStyle = BlockById(objectPage, "variable").S;
                    var priorityObjectStyle = BlockById(objectPage, "priority").S;
                    Ok(variableObjectStyle.Of == "v" && variableObjectStyle.Ox == 100 && variableObjectStyle.Oy == 50 &&
                        priorityObjectStyle.Of == "c" && priorityObjectStyle.Ox == 0 && priorityObjectStyle.Oy == 0,
                        "variable and important object geometry");
                    Ok(BlockById(objectPage, "dynamic").S.Of == "c", "wide responsive object fit state");
                    objectEngine.Vw = 350;
                    Ok(BlockById(objectPage, "dynamic").S.Of == "v" && BlockById(objectPage, "dynamic").S.Ox == 100 && BlockById(objectPage, "dynamic").S.Oy == 100,
                        "narrow responsive object fit state");

                    ObjectFitPaintSmoke(objectPath);

                    var ratioPath = Path.Combine(pages, "aspect-ratio.html");
                    var ratioEngine = new E();
                    var ratioPage = ratioEngine.Go(ratioPath);
                    var wideRatio = BlockById(ratioPage, "wide").S;
                    var tallRatio = BlockById(ratioPage, "tall").S;
                    Ok(wideRatio.Rw == 16D && wideRatio.Rh == 9D && !wideRatio.Ra &&
                        tallRatio.Rw == 3D && tallRatio.Rh == 4D && !tallRatio.Ra,
                        "aspect-ratio fraction aliases");
                    var autoRatio = BlockById(ratioPage, "auto-image").S;
                    var fallbackRatio = BlockById(ratioPage, "fallback-image").S;
                    Ok(autoRatio.Rw == 1D && autoRatio.Rh == 1D && autoRatio.Ra &&
                        fallbackRatio.Rw == 4D && fallbackRatio.Rh == 3D && fallbackRatio.Ra,
                        "auto intrinsic ratio aliases");
                    var atomicRatio = BlockById(ratioPage, "atomic").S;
                    var resetRatio = BlockById(ratioPage, "reset").S;
                    Ok(atomicRatio.Rw == 1.5D && atomicRatio.Rh == 1D && !atomicRatio.Ra &&
                        resetRatio.Rw == 0D && resetRatio.Rh == 0D && resetRatio.Ra &&
                        BlockById(ratioPage, "dot-ratio").S.Rw == .5D,
                        "decimal ratio forms, invalid preservation, and reset");
                    Ok(BlockById(ratioPage, "ratio-parent").S.Rw == 3D && BlockById(ratioPage, "ratio-child").S.Rw == 0D,
                        "aspect-ratio noninheritance");
                    var variableRatio = BlockById(ratioPage, "variable").S;
                    var priorityRatio = BlockById(ratioPage, "priority").S;
                    Ok(variableRatio.Rw == 5D && variableRatio.Rh == 2D && !variableRatio.Ra &&
                        priorityRatio.Rw == 2D && priorityRatio.Rh == 1D,
                        "variable and important aspect ratio");
                    Ok(BlockById(ratioPage, "dynamic").S.Rw == 4D && BlockById(ratioPage, "dynamic").S.Rh == 1D,
                        "wide responsive aspect ratio state");
                    ratioEngine.Vw = 350;
                    Ok(BlockById(ratioPage, "dynamic").S.Rw == 1D && BlockById(ratioPage, "dynamic").S.Rh == 1D,
                        "narrow responsive aspect ratio state");

                    AspectRatioPaintSmoke(ratioPath);

                    var outlinePath = Path.Combine(pages, "outlines.html");
                    var outlineEngine = new E();
                    var outlinePage = outlineEngine.Go(outlinePath);
                    var solidOutline = BlockById(outlinePage, "solid").S;
                    var dashedOutline = BlockById(outlinePage, "dashed").S;
                    var dottedOutline = BlockById(outlinePage, "dotted").S;
                    Ok(solidOutline.Ow == 4 && solidOutline.Os == "s" && solidOutline.Oc == "#a13b18" && solidOutline.Oo == 3 &&
                        dashedOutline.Ow == 3 && dashedOutline.Os == "d" && dottedOutline.Ow == 4 && dottedOutline.Os == "o",
                        "outline compact aliases");
                    var atomicOutline = BlockById(outlinePage, "atomic").S;
                    var longhandOutline = BlockById(outlinePage, "longhand").S;
                    var resetOutline = BlockById(outlinePage, "reset").S;
                    Ok(atomicOutline.Ow == 2 && atomicOutline.Os == "s" && atomicOutline.Oc == "#176b4d" &&
                        longhandOutline.Ow == 5 && longhandOutline.Os == "s" && longhandOutline.Oo == 6 &&
                        resetOutline.Ow == 3 && resetOutline.Os == "n" && resetOutline.Oc.Length == 0,
                        "outline atomic invalid preservation, longhands, and reset");
                    Ok(BlockById(outlinePage, "outline-parent").S.Ow == 3 && BlockById(outlinePage, "outline-child").S.Ow == 0 &&
                        BlockById(outlinePage, "outline-child").S.Os == "n",
                        "outline noninheritance");
                    var variableOutline = BlockById(outlinePage, "variable").S;
                    var priorityOutline = BlockById(outlinePage, "priority").S;
                    Ok(variableOutline.Ow == 3 && variableOutline.Os == "d" && variableOutline.Oo == 4 &&
                        priorityOutline.Ow == 5 && priorityOutline.Os == "s" && priorityOutline.Oc == "#176b4d",
                        "outline variables and important cascade");
                    var retainedOutlineField = FieldById(outlinePage, "focus-field");
                    retainedOutlineField.V = "Retained focused outline";
                    outlineEngine.Dy(outlinePage, null, ById(outlinePage.D, "focus-field"));
                    Ok(retainedOutlineField.S.Ow == 4 && retainedOutlineField.S.Os == "s" && retainedOutlineField.S.Oo == 5 &&
                        Object.ReferenceEquals(retainedOutlineField, FieldById(outlinePage, "focus-field")) && retainedOutlineField.V == "Retained focused outline",
                        "dynamic focus outline and retained field");
                    outlineEngine.Dy(outlinePage, null, null);
                    Ok(retainedOutlineField.S.Os == "n" && retainedOutlineField.S.Ow == 3,
                        "dynamic focus outline reset");
                    Ok(BlockById(outlinePage, "responsive").S.Ow == 2 && BlockById(outlinePage, "responsive").S.Os == "s",
                        "wide responsive outline state");
                    outlineEngine.Vw = 350;
                    Ok(BlockById(outlinePage, "responsive").S.Ow == 6 && BlockById(outlinePage, "responsive").S.Os == "o" && BlockById(outlinePage, "responsive").S.Oo == 5,
                        "narrow responsive outline state");

                    OutlinePaintSmoke(outlinePath);

                    var pointerPath = Path.Combine(pages, "pointer-interaction.html");
                    var pointerEngine = new E();
                    var pointerPage = pointerEngine.Go(pointerPath);
                    Ok(pointerPage.S.Cu == "c" && pointerPage.S.Pe && BlockById(pointerPage, "inherited").S.Cu == "c" && BlockById(pointerPage, "inherited").S.Pe,
                        "inherited cursor and pointer-event defaults");
                    Ok(BlockById(pointerPage, "pointer").S.Cu == "p" && BlockById(pointerPage, "text").S.Cu == "t" &&
                        BlockById(pointerPage, "wait").S.Cu == "w" && BlockById(pointerPage, "move").S.Cu == "m" &&
                        BlockById(pointerPage, "resize-x").S.Cu == "e" && BlockById(pointerPage, "resize-y").S.Cu == "s" &&
                        BlockById(pointerPage, "forbidden").S.Cu == "n" && BlockById(pointerPage, "hover-cursor").S.Cu == "p",
                        "compact cursor keyword aliases");
                    Ok(BlockById(pointerPage, "invalid-cursor").S.Cu == "h" && !BlockById(pointerPage, "invalid-events").S.Pe &&
                        BlockById(pointerPage, "reset").S.Cu.Length == 0 && BlockById(pointerPage, "reset").S.Pe,
                        "invalid pointer preservation and CSS-wide reset");
                    Ok(BlockById(pointerPage, "variable").S.Cu == "p" && !BlockById(pointerPage, "variable").S.Pe &&
                        BlockById(pointerPage, "priority").S.Cu == "w" && !BlockById(pointerPage, "priority").S.Pe,
                        "pointer variables and important cascade");
                    Ok(!BlockById(pointerPage, "none-parent").S.Pe && !BlockById(pointerPage, "none-child").S.Pe &&
                        BlockById(pointerPage, "none-child").S.Cu == "n" && BlockById(pointerPage, "auto-child").S.Pe && BlockById(pointerPage, "auto-child").S.Cu == "h",
                        "pointer-event inheritance and child re-enable");
                    Ok(!FieldById(pointerPage, "dead-field").S.Pe && FieldById(pointerPage, "dead-field").S.Cu == "t" &&
                        FieldById(pointerPage, "live-field").S.Pe && FieldById(pointerPage, "live-field").S.Cu == "t",
                        "field pointer aliases");
                    Ok(BlockById(pointerPage, "responsive").S.Cu == "d" && BlockById(pointerPage, "responsive").S.Pe,
                        "wide responsive pointer state");
                    pointerEngine.Vw = 350;
                    Ok(BlockById(pointerPage, "responsive").S.Cu == "w" && !BlockById(pointerPage, "responsive").S.Pe,
                        "narrow responsive pointer state");

                    PointerInteractionSmoke(pointerPath);

                    view.Size = new Size(720, 760);
                    var boxes = Path.Combine(pages, "boxes.html");
                    var boxPage = engine.Go(boxes);
                    Eq(4, boxPage.Bs.Count, "box-model blocks");
                    Eq(280, boxPage.Bs[1].S.W, "CSS width");
                    Eq(4, boxPage.Bs[1].S.Bt, "border shorthand width");
                    Eq("solid", boxPage.Bs[1].S.Bs, "border shorthand style");
                    Eq("#18734b", boxPage.Bs[1].S.Bc, "border shorthand color");
                    Eq(360, boxPage.Bs[2].S.W, "fixed width");
                    Eq(54, boxPage.Bs[2].S.H, "fixed height");
                    Eq(5, boxPage.Bs[3].S.Bt, "top border width");
                    Eq(2, boxPage.Bs[3].S.Br, "right border width");
                    Eq(3, boxPage.Bs[3].S.Bb, "bottom border width");
                    Eq(1, boxPage.Bs[3].S.Bl, "left border width");
                    var boxSizingPath = Path.Combine(pages, "box-sizing.html");
                    var boxSizingPage = engine.Go(boxSizingPath);
                    Eq("c", BlockById(boxSizingPage, "content-box").S.Bz, "content-box compact style");
                    Eq("b", BlockById(boxSizingPage, "border-box").S.Bz, "border-box compact style");
                    Eq("b", BlockById(boxSizingPage, "border-min").S.Bz, "border-box minimum compact style");
                    Eq("b", BlockById(boxSizingPage, "border-max").S.Bz, "invalid box-sizing value is ignored");
                    Eq("b", FieldById(boxSizingPage, "border-field").S.Bz, "field border-box compact style");
                    Eq("b", BlockById(boxSizingPage, "border-image").S.Bz, "image border-box compact style");
                    Eq("b", BlockById(boxSizingPage, "border-flex").S.Bz, "flex container border-box compact style");
                    Eq("c", BlockById(boxSizingPage, "flex-content").S.Bz, "box sizing does not inherit");
                    Eq("b", BlockById(boxSizingPage, "flex-border").S.Bz, "flex item border-box compact style");
                    Eq(140, BlockById(boxSizingPage, "flex-border").S.Fb, "border-box flex basis model");
                    Eq("b", BlockById(boxSizingPage, "border-table").S.Bz, "table border-box compact style");
                    Eq("b", BlockById(boxSizingPage, "border-table").Tb.Rs[0].Cs[0].S.Bz, "table cell border-box compact style");
                    view.Go(boxes);
                    Paint(view);
                    Eq(5, view.Bc, "box-model boxes");
                    Eq(280, view.Rc(2).Width, "solid content width");
                    Eq(308, view.Rb(2).Width, "solid border-box width");
                    Eq(54, view.Rc(3).Height, "fixed content height");
                    Eq(76, view.Rb(3).Height, "fixed border-box height");
                    Eq(320, view.Rc(4).Width, "sided content width");
                    Eq(347, view.Rb(4).Width, "sided border-box width");
                    view.Size = new Size(720, 1200);
                    view.Go(boxSizingPath);
                    using (var boxSizingPaint = Render(view))
                    {
                        var contentBox = BlockRect(view, BlockById(view.At, "content-box"));
                        var contentOuter = BlockOuterRect(view, BlockById(view.At, "content-box"));
                        var borderBox = BlockRect(view, BlockById(view.At, "border-box"));
                        var borderOuter = BlockOuterRect(view, BlockById(view.At, "border-box"));
                        Eq(new Size(200, 50), contentBox.Size, "content-box declared content dimensions");
                        Eq(new Size(232, 78), contentOuter.Size, "content-box outer dimensions");
                        Eq(new Size(168, 22), borderBox.Size, "border-box derived content dimensions");
                        Eq(new Size(200, 50), borderOuter.Size, "border-box declared outer dimensions");
                        Eq(new Size(180, 42), BlockOuterRect(view, BlockById(view.At, "border-min")).Size, "border-box minimum dimensions");
                        Eq(new Size(170, 44), BlockOuterRect(view, BlockById(view.At, "border-max")).Size, "border-box maximum dimensions");
                        Eq(new Size(180, 44), BlockOuterRect(view, BlockById(view.At, "border-field")).Size, "border-box field dimensions");
                        Eq(new Size(164, 28), BlockRect(view, BlockById(view.At, "border-field")).Size, "border-box field content dimensions");
                        Eq(new Size(100, 60), BlockOuterRect(view, BlockById(view.At, "border-image")).Size, "border-box image dimensions");
                        Eq(new Size(86, 46), BlockRect(view, BlockById(view.At, "border-image")).Size, "border-box image content dimensions");
                        Eq(420, BlockOuterRect(view, BlockById(view.At, "border-flex")).Width, "border-box flex container width");
                        Eq(396, BlockRect(view, BlockById(view.At, "border-flex")).Width, "border-box flex content width");
                        Eq(new Size(162, 62), BlockOuterRect(view, BlockById(view.At, "flex-content")).Size, "content-box flex item dimensions");
                        Eq(new Size(140, 40), BlockOuterRect(view, BlockById(view.At, "flex-border")).Size, "border-box flex item dimensions");
                        Eq(new Size(118, 18), BlockRect(view, BlockById(view.At, "flex-border")).Size, "border-box flex item content dimensions");
                        Eq(new Size(240, 100), BlockOuterRect(view, BlockById(view.At, "border-table")).Size, "border-box table dimensions");
                        Eq(new Size(216, 76), BlockRect(view, BlockById(view.At, "border-table")).Size, "border-box table content dimensions");
                        Eq(new Size(216, 52), ElementRect(view, ById(view.At.D, "border-cell"), true).Size, "border-box table cell dimensions");
                        Eq(new Size(200, 36), ElementRect(view, ById(view.At.D, "border-cell"), false).Size, "border-box table cell content dimensions");
                        Eq(borderOuter, HitRect(view, "second.html"), "border-box link hit bounds");
                        Ok(ColorDistance(boxSizingPaint.GetPixel(borderOuter.Right - 10, borderOuter.Bottom - 10), Color.FromArgb(232, 245, 239)) < 4, "border-box background paint");
                    }
                    view.Size = new Size(180, 1200);
                    Paint(view);
                    var narrowBorder = BlockOuterRect(view, BlockById(view.At, "border-box"));
                    var narrowField = BlockOuterRect(view, BlockById(view.At, "border-field"));
                    var narrowFlex = BlockRect(view, BlockById(view.At, "border-flex"));
                    var narrowFlexBorder = BlockOuterRect(view, BlockById(view.At, "flex-border"));
                    var narrowTable = BlockOuterRect(view, BlockById(view.At, "border-table"));
                    Ok(narrowBorder.Width > 32 && narrowBorder.Width < 200 && narrowField.Width == narrowBorder.Width, "border-box blocks and fields reflow narrowly");
                    Ok(narrowFlexBorder.Width > 22 && narrowFlexBorder.Right <= narrowFlex.Right, "border-box flex item remains inside narrow content");
                    Ok(narrowTable.Width == narrowBorder.Width, "border-box table reflows narrowly");
                    view.Size = new Size(720, 760);
                    view.Go(Path.Combine(pages, "tables.html"));
                    Paint(view);
                    Eq(17, view.Bc, "table layout boxes");
                    Eq(480, view.Rc(2).Width, "table content width");
                    Eq(484, view.Rb(2).Width, "table border-box width");
                    Eq(view.Rb(4).Y, view.Rb(5).Y, "header cells share a row");
                    Eq(160, view.Rb(4).Width, "equal table column width");
                    Eq(320, view.Rb(10).Width, "spanning cell width");
                    Eq(160, view.Rb(11).Width, "cell after span width");
                    Ok(view.Rb(13).Y >= view.Rb(12).Y && view.Rb(15).Bottom <= view.Rb(12).Bottom, "nested blocks stay in cell");
                    view.Size = new Size(420, 760);
                    Paint(view);
                    Ok(view.Rb(2).Width < 484, "table reflows to narrow view");
                    Eq(view.Rb(4).Y, view.Rb(5).Y, "narrow header row alignment");
                    view.Size = new Size(720, 760);
                    Paint(view);
                    Click(view, view.Rb(15));
                    WaitFor(delegate { return !view.Busy && view.T == "LeaForge Second Page"; }, "table cell link navigation");
                    view.Size = new Size(300, 520);
                    view.Go(inlinePath);
                    Paint(view);
                    Ok(view.Bc > inlinePage.Bs.Count + 1, "inline runs create paint boxes");
                    var inlineBlock = BlockRect(view, view.At.Bs[1]);
                    Ok(inlineBlock.Height > 40, "inline text wraps and honors line break");
                    Ok(SameInlineHeights(view, view.At.Bs[0]), "heading inline runs retain heading size");
                    var inlineLink = LinkRect(view, "second.html");
                    Ok(!inlineLink.IsEmpty && inlineBlock.Contains(new Point(inlineLink.Left, inlineLink.Top)), "inline link stays inside paragraph");
                    Eq(3, LinkBoxes(view, "second.html", true), "three inline link words retain their targets");
                    var inlineLinkSpace = Rectangle.Empty;
                    foreach (var value in LayoutBoxes(view))
                    {
                        var type = value.GetType();
                        var url = type.GetField("U").GetValue(value) as Uri;
                        if ((string)type.GetField("X").GetValue(value) == " " && url != null && Path.GetFileName(url.LocalPath) == "second.html")
                            inlineLinkSpace = (Rectangle)type.GetField("R").GetValue(value);
                    }
                    Ok(!inlineLinkSpace.IsEmpty, "inline link keeps whitespace geometry");
                    Click(view, inlineLinkSpace);
                    WaitFor(delegate { return !view.Busy && view.T == "LeaForge Second Page"; }, "inline link whitespace navigation");
                    view.Size = new Size(420, 760);
                    view.Go(textPath);
                    Paint(view);
                    var centerBlock = view.At.Bs[1];
                    var centerBox = BlockRect(view, centerBlock);
                    var centerLines = TextLines(view, centerBlock);
                    Eq(2, centerLines.Length, "center line count");
                    Ok(Centered(centerLines[0], centerBox) && Centered(centerLines[1], centerBox), "centered line positions");
                    Ok(centerLines[1].Top - centerLines[0].Top >= 28, "pixel line spacing");
                    var rightBlock = view.At.Bs[2];
                    var rightBox = BlockRect(view, rightBlock);
                    var rightLines = TextLines(view, rightBlock);
                    Eq(2, rightLines.Length, "right line count");
                    Ok(rightLines[0].Right == rightBox.Right && rightLines[1].Right == rightBox.Right, "right line positions");
                    Ok(rightLines[1].Top - rightLines[0].Top > 20, "percentage line spacing");
                    var justifyBlock = view.At.Bs[3];
                    var justifyBox = BlockRect(view, justifyBlock);
                    var justifyLines = TextLines(view, justifyBlock);
                    Ok(justifyLines.Length > 1, "justified text wraps");
                    Ok(justifyLines[0].Left == justifyBox.Left && justifyLines[0].Right == justifyBox.Right, "wrapped line justification");
                    Ok(justifyLines[justifyLines.Length - 1].Width < justifyBox.Width, "final justify line stays natural");
                    var cellPart = view.At.Bs[4].Tb.Rs[0].Cs[0].Bs[0];
                    var cellLines = TextLines(view, cellPart);
                    var cellBox = ContainingCell(view, view.At.Bs[4], cellLines[0]);
                    Ok(cellLines.Length > 1 && !cellBox.IsEmpty, "table text line layout");
                    foreach (var cellLine in cellLines) Ok(cellLine.Right == cellBox.Right, "right-aligned table line");
                    view.Size = new Size(520, 900);
                    view.Go(whitePath);
                    Paint(view);
                    var nowrapBox = BlockRect(view, view.At.Bs[1]);
                    var nowrapLines = TextLines(view, view.At.Bs[1]);
                    Eq(1, nowrapLines.Length, "nowrap line count");
                    Ok(nowrapLines[0].Right > nowrapBox.Right, "nowrap keeps natural line width");
                    Eq(nowrapBox, ClipRect(view, view.At.Bs[1]), "hidden overflow clip");
                    var visibleBox = BlockRect(view, view.At.Bs[2]);
                    var visibleLines = TextLines(view, view.At.Bs[2]);
                    Eq(1, visibleLines.Length, "visible nowrap line count");
                    Ok(visibleLines[0].Right > visibleBox.Right && ClipRect(view, view.At.Bs[2]).IsEmpty, "visible overflow remains unclipped");
                    Eq(2, TextLines(view, view.At.Bs[3]).Length, "pre explicit lines");
                    Ok(PaintedText(view, view.At.Bs[3]).Contains("    "), "pre spaces remain painted");
                    Ok(TextLines(view, view.At.Bs[4]).Length > 2 && PaintedText(view, view.At.Bs[4]).Contains("    "), "pre-wrap preserves and wraps");
                    Eq(2, TextLines(view, view.At.Bs[5]).Length, "pre-line explicit lines");
                    Ok(!PaintedText(view, view.At.Bs[5]).Contains("  "), "pre-line collapses spaces");
                    var cropBox = BlockRect(view, view.At.Bs[6]);
                    var cropLines = TextLines(view, view.At.Bs[6]);
                    Eq(30, cropBox.Height, "fixed overflow height");
                    Ok(cropLines.Length > 2 && cropLines[cropLines.Length - 1].Bottom > cropBox.Bottom, "overflow retains hidden layout content");
                    Eq(cropBox, ClipRect(view, view.At.Bs[6]), "auto overflow clip");
                    Eq(2, TextLines(view, view.At.Bs[7]).Length, "inherited pre-line layout");
                    var whiteCellPart = view.At.Bs[8].Tb.Rs[0].Cs[0].Bs[0];
                    var whiteCellLines = TextLines(view, whiteCellPart);
                    var whiteCellBox = ContainingCell(view, view.At.Bs[8], whiteCellLines[0]);
                    Ok(whiteCellLines.Length > 2 && whiteCellBox.Height == 38, "table white-space layout");
                    Eq(whiteCellBox, ClipRect(view, whiteCellPart), "table cell overflow clip");
                    Eq(BlockRect(view, view.At.Bs[9]), ClipRect(view, view.At.Bs[9]), "scroll overflow clip");
                    view.Go(sizingPath);
                    Paint(view);
                    Eq(260, BlockRect(view, view.At.Bs[1]).Width, "minimum width layout");
                    Eq(280, BlockRect(view, view.At.Bs[2]).Width, "maximum width layout");
                    Eq(58, BlockRect(view, view.At.Bs[3]).Height, "minimum height layout");
                    var maxHeightBox = BlockRect(view, view.At.Bs[4]);
                    var maxHeightLines = TextLines(view, view.At.Bs[4]);
                    Eq(42, maxHeightBox.Height, "maximum height layout");
                    Ok(maxHeightLines.Length > 2 && maxHeightLines[maxHeightLines.Length - 1].Bottom > maxHeightBox.Bottom, "maximum height keeps overflow layout");
                    Eq(maxHeightBox, ClipRect(view, view.At.Bs[4]), "maximum height overflow clip");
                    var normalBox = BlockRect(view, view.At.Bs[5]);
                    var normalLines = TextLines(view, view.At.Bs[5]);
                    Eq(1, normalLines.Length, "normal long word line count");
                    Ok(normalLines[0].Right > normalBox.Right, "normal long word remains intact");
                    var breakBox = BlockRect(view, view.At.Bs[6]);
                    var breakLines = TextLines(view, view.At.Bs[6]);
                    Ok(breakLines.Length > 2 && LinesFitWidth(breakLines, breakBox), "break-word bounded layout");
                    var anywhereBox = BlockRect(view, view.At.Bs[7]);
                    var anywhereLines = TextLines(view, view.At.Bs[7]);
                    Ok(anywhereLines.Length > 1 && LinesFitWidth(anywhereLines, anywhereBox), "break-all bounded layout");
                    Ok(anywhereLines[0].Right > breakLines[0].Right + 40, "break-all uses remaining line width");
                    var fieldBox = BlockRect(view, view.At.Bs[8]);
                    Eq(260, fieldBox.Width, "field maximum width layout");
                    Eq(40, fieldBox.Height, "field minimum height layout");
                    Eq(260, BlockRect(view, view.At.Bs[9]).Width, "table maximum width layout");
                    var sizingCellPart = view.At.Bs[9].Tb.Rs[0].Cs[0].Bs[0];
                    var sizingCellLines = TextLines(view, sizingCellPart);
                    var sizingCellBox = ContainingCell(view, view.At.Bs[9], sizingCellLines[0]);
                    Ok(sizingCellLines.Length > 1 && LinesFitWidth(sizingCellLines, sizingCellBox), "table-cell anywhere wrapping");
                    var visibilityLoad = view.GoA(visibilityPath);
                    while (!visibilityLoad.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    visibilityLoad.GetAwaiter().GetResult();
                    using (var visibilityPaint = Render(view))
                    {
                        var opaqueBox = BlockRect(view, view.At.Bs[1]);
                        var halfBox = BlockRect(view, view.At.Bs[2]);
                        var quarterBox = BlockRect(view, view.At.Bs[4]);
                        var hiddenBox = BlockRect(view, view.At.Bs[6]);
                        var collapsedBox = BlockRect(view, view.At.Bs[7]);
                        var shownBox = BlockRect(view, view.At.Bs[8]);
                        var white = Color.White;
                        var opaqueDistance = ColorDistance(BoxPixel(visibilityPaint, opaqueBox), white);
                        var halfDistance = ColorDistance(BoxPixel(visibilityPaint, halfBox), white);
                        var quarterDistance = ColorDistance(BoxPixel(visibilityPaint, quarterBox), white);
                        Ok(opaqueDistance > halfDistance && halfDistance > quarterDistance, "rendered opacity progression");
                        Ok(!hiddenBox.IsEmpty && !collapsedBox.IsEmpty && shownBox.Top > collapsedBox.Bottom && collapsedBox.Top > hiddenBox.Bottom, "hidden visibility keeps layout space");
                        Eq(white.ToArgb(), BoxPixel(visibilityPaint, hiddenBox).ToArgb(), "hidden box leaves page background");
                        Eq(white.ToArgb(), BoxPixel(visibilityPaint, collapsedBox).ToArgb(), "collapsed visibility leaves page background");
                        Ok(ColorDistance(BoxPixel(visibilityPaint, shownBox), white) > 10, "visible child of hidden parent paints");
                        Eq(1, HitCount(view, false), "transparent link hit and hidden link suppression");
                        Eq(1, HitCount(view, true), "faded field hit and hidden field suppression");
                        var opaqueImage = BlockRect(view, view.At.Bs[13]);
                        var fadedImage = BlockRect(view, view.At.Bs[14]);
                        Ok(ColorDistance(CenterPixel(visibilityPaint, opaqueImage), white) > ColorDistance(CenterPixel(visibilityPaint, fadedImage), white), "image opacity rendering");
                    }
                    view.Go(selectorPath);
                    Paint(view);
                    Eq(12, view.At.Bs.Count, "relationship selector render blocks");
                    Eq("stage", view.Id("stage").Av("id"), "WinForms Id query");
                    Eq("Direct child", NodeText(view.Q1("#stage > p.direct")), "WinForms first query");
                    Eq(3, view.Qa(".card").Count, "WinForms all query");
                    Eq("", view.Q, "WinForms URL query alias remains available");
                    var firstCard = BlockRect(view, view.At.Bs[5]);
                    var secondCard = BlockRect(view, view.At.Bs[6]);
                    var thirdCard = BlockRect(view, view.At.Bs[7]);
                    Eq(firstCard.Left + 18, secondCard.Left, "adjacent selector second-card layout");
                    Eq(firstCard.Left + 18, thirdCard.Left, "adjacent selector third-card layout");
                    view.Go(basePath);
                    Paint(view);
                    Eq(responseUrl, view.U, "WinForms response URL with base");
                    Eq(expectedBase, view.Bu, "WinForms document base URL");
                    Ok(!HitRect(view, "second.html").IsEmpty, "WinForms base URL link hit");
                    view.Go(negationPath);
                    Paint(view);
                    Eq(9, view.At.Bs.Count, "general sibling and negation render blocks");
                    Ok(view.Bc > view.At.Bs.Count, "negation fields paint separately");
                    Ok(BlockRect(view, view.At.Bs[6]).Left > BlockRect(view, view.At.Bs[1]).Left, "general sibling border and padding layout");
                    using (var combinedSelectorFont = RenderFont(view, "p", view.At.Bs[2].S)) Ok(HasFontStyle(combinedSelectorFont, FontStyle.Bold) && HasFontStyle(combinedSelectorFont, FontStyle.Italic), "combined sibling selector font");
                    using (var enabledSelectorFont = RenderFont(view, "button", view.At.Bs[7].S)) Ok(HasFontStyle(enabledSelectorFont, FontStyle.Bold), "negated disabled rendered font");
                    using (var disabledSelectorFont = RenderFont(view, "button", view.At.Bs[8].S)) Ok(HasFontStyle(disabledSelectorFont, FontStyle.Italic), "negated enabled rendered font");
                    view.Go(selectorFunctionPath);
                    Paint(view);
                    Eq(120, BlockRect(view, BlockById(view.At, "adjacent")).Width, "functional selector view width");
                    Ok(BlockRect(view, BlockById(view.At, "adjacent")).Left > BlockRect(view, BlockById(view.At, "seed")).Left, "functional adjacent selector view margin");
                    var selectorViewField = FieldById(view.At, "field");
                    selectorViewField.V = "Retained selector view";
                    Move(view, BlockRect(view, BlockById(view.At, "hover")));
                    Paint(view);
                    Ok(BlockById(view.At, "hover").S.F == "#a13b18" && BlockById(view.At, "hover").S.Pl == 19, "functional hover selector view state");
                    Ok(Object.ReferenceEquals(selectorViewField, FieldById(view.At, "field")) && selectorViewField.V == "Retained selector view", "functional hover view preserves field state");
                    view.Go(selectorArithmeticPath);
                    Paint(view);
                    Eq(96, BlockRect(view, BlockById(view.At, "a1")).Width, "structural formula view width");
                    Eq(31, BlockRect(view, BlockById(view.At, "a1")).Height, "structural formula view height");
                    Ok(BlockRect(view, BlockById(view.At, "a5")).Left > BlockRect(view, BlockById(view.At, "a6")).Left, "structural selector-list view margin");
                    Eq(142, BlockRect(view, BlockById(view.At, "m2")).Width, "reverse type formula view width");
                    using (var arithmeticBold = RenderFont(view, "p", BlockById(view.At, "a1").S)) Ok(HasFontStyle(arithmeticBold, FontStyle.Bold), "reverse formula rendered font");
                    using (var arithmeticItalic = RenderFont(view, "p", BlockById(view.At, "a5").S)) Ok(HasFontStyle(arithmeticItalic, FontStyle.Italic), "forward formula rendered font");
                    view.Go(selectorFilteredPath);
                    Paint(view);
                    Eq(108, BlockRect(view, BlockById(view.At, "f1")).Width, "filtered selector view width");
                    Eq(33, BlockRect(view, BlockById(view.At, "specific")).Height, "filtered specificity view height");
                    Ok(FieldById(view.At, "e2").S.Bg == "#dff6e9" && FieldById(view.At, "e2").S.B && FieldById(view.At, "e3").S.Bl == 5, "filtered form-state view styles");
                    using (var filteredBold = RenderFont(view, "p", BlockById(view.At, "f8").S)) Ok(HasFontStyle(filteredBold, FontStyle.Bold), "filtered reverse rendered font");
                    view.Go(selectorRelationalPath);
                    Paint(view);
                    Eq(118, BlockRect(view, BlockById(view.At, "direct")).Width, "relational direct-child view width");
                    Eq(37, BlockRect(view, BlockById(view.At, "specific")).Height, "relational specificity view height");
                    Eq(6, BlockById(view.At, "complex").S.Br, "relational complex view border");
                    using (var relationalBold = RenderFont(view, "section", BlockById(view.At, "direct").S)) Ok(HasFontStyle(relationalBold, FontStyle.Bold), "relational rendered font");
                    view.Go(selectorStatePath);
                    Paint(view);
                    Eq(13, view.At.Bs.Count, "attribute and pseudo selector render blocks");
                    Ok(view.Bc > view.At.Bs.Count, "selector fields and inline content paint separately");
                    Ok(BlockRect(view, view.At.Bs[6]).Left > BlockRect(view, view.At.Bs[1]).Left, "only-child border and padding layout");
                    using (var stateSerifFont = RenderFont(view, "p", view.At.Bs[2].S)) Eq("Georgia", stateSerifFont.FontFamily.Name, "nth-of-type rendered font");
                    using (var stateMonoFont = RenderFont(view, "input", view.At.Fs[0].S)) Eq("Consolas", stateMonoFont.FontFamily.Name, "empty field rendered font");
                    using (var stateLinkFont = RenderFont(view, "a", view.At.Bs[7].Is[0].S, true, "p")) Ok(!HasFontStyle(stateLinkFont, FontStyle.Underline), "link pseudo rendered decoration");
                    view.Go(dynamicPath);
                    Paint(view);
                    var liveField = view.At.Fs[0];
                    liveField.V = "Live edited value";
                    Move(view, BlockRect(view, view.At.Bs[1]));
                    Paint(view);
                    Eq(18, view.At.Bs[1].S.Pl, "live ancestor hover layout");
                    Eq("#fff4d6", view.At.Bs[1].S.Bg, "live ancestor hover paint");
                    Ok(Run(view.At.Bs[1], "strong").S.I, "live hover descendant style");
                    var liveLinkRect = LinkRect(view, "second.html");
                    Move(view, liveLinkRect);
                    Paint(view);
                    var liveLink = Run(view.At.Bs[1], "a");
                    Eq("#7e22ce", liveLink.S.F, "live inline hover color");
                    Ok(liveLink.S.B && liveLink.S.Td == "n", "live inline hover decoration");
                    var liveFieldRect = BlockRect(view, view.At.Bs[2]);
                    Move(view, liveFieldRect);
                    Paint(view);
                    liveFieldRect = BlockRect(view, view.At.Bs[2]);
                    Click(view, liveFieldRect);
                    Paint(view);
                    Eq("Georgia|s", view.At.Fs[0].S.Ff, "live field focus font");
                    Eq("#176b4d", view.At.Fs[0].S.Bg, "live field focus paint");
                    Eq(16, view.At.Fs[0].S.Pl, "live field focus layout");
                    Ok(Object.ReferenceEquals(liveField, view.At.Fs[0]) && view.At.Fs[0].V == "Live edited value", "live restyle preserves editor field");
                    Move(view, new Point(-10, -10));
                    Paint(view);
                    Eq(6, view.At.Bs[1].S.Pl, "live hover clears outside view");
                    Click(view, new Rectangle(0, 0, 2, 2));
                    Paint(view);
                    Eq("Consolas|m", view.At.Fs[0].S.Ff, "live focus clears on background click");
                    Ok(Object.ReferenceEquals(liveField, view.At.Fs[0]) && view.At.Fs[0].V == "Live edited value", "live blur preserves field value");
                    view.Size = new Size(840, 760);
                    view.Go(mediaQueryPath);
                    Paint(view);
                    Eq(840, view.Vw, "view viewport width forwarding");
                    Ok(HasBlock(view.At, "wide") && !HasBlock(view.At, "narrow"), "wide view media state");
                    Eq(320, BlockRect(view, BlockById(view.At, "wide")).Width, "wide responsive geometry");
                    Ok(!FragmentHitRect(view, "#wide-target").IsEmpty && FragmentHitRect(view, "#narrow-target").IsEmpty, "wide responsive hit state");
                    var responsiveViewField = FieldById(view.At, "field");
                    responsiveViewField.V = "Retained view value";
                    view.Size = new Size(480, 760);
                    Application.DoEvents();
                    Paint(view);
                    Eq(480, view.Vw, "resized view viewport width");
                    Ok(!HasBlock(view.At, "wide") && HasBlock(view.At, "narrow") && HasBlock(view.At, "nested") && HasBlock(view.At, "exact"), "narrow view media state");
                    Eq(180, BlockRect(view, BlockById(view.At, "narrow")).Width, "narrow responsive geometry");
                    Ok(FragmentHitRect(view, "#wide-target").IsEmpty && !FragmentHitRect(view, "#narrow-target").IsEmpty, "narrow responsive hit state");
                    Ok(Object.ReferenceEquals(responsiveViewField, FieldById(view.At, "field")) && responsiveViewField.V == "Retained view value", "view resize preserves field state");
                    view.Size = new Size(840, 760);
                    view.Go(customPath);
                    Paint(view);
                    var customViewField = FieldById(view.At, "var-field");
                    customViewField.V = "Retained custom view value";
                    Eq(260, BlockRect(view, BlockById(view.At, "base")).Width, "custom variable view geometry");
                    Move(view, BlockRect(view, BlockById(view.At, "hover")));
                    Paint(view);
                    Ok(BlockById(view.At, "hover").S.F == "#a13b18" && BlockById(view.At, "hover").S.Pl == 18, "live custom property hover");
                    view.Size = new Size(480, 760);
                    Application.DoEvents();
                    Paint(view);
                    Eq(150, BlockRect(view, BlockById(view.At, "base")).Width, "responsive custom variable geometry");
                    Ok(Object.ReferenceEquals(customViewField, FieldById(view.At, "var-field")) && customViewField.V == "Retained custom view value", "custom view rebuild preserves field state");
                    view.Size = new Size(840, 760);
                    view.Go(importantPath);
                    Paint(view);
                    Eq(140, BlockRect(view, BlockById(view.At, "sheet")).Width, "important stylesheet view geometry");
                    Eq(190, BlockRect(view, BlockById(view.At, "inline")).Width, "important inline view geometry");
                    var importantViewField = FieldById(view.At, "field");
                    importantViewField.V = "Retained important view value";
                    Move(view, BlockRect(view, BlockById(view.At, "hover")));
                    Paint(view);
                    Ok(BlockById(view.At, "hover").S.F == "#a13b18" && BlockRect(view, BlockById(view.At, "hover")).Width == 180, "important hover view cascade");
                    Move(view, new Point(-10, -10));
                    view.Size = new Size(480, 760);
                    Application.DoEvents();
                    Paint(view);
                    Eq(115, BlockRect(view, BlockById(view.At, "media")).Width, "important responsive view geometry");
                    Ok(Object.ReferenceEquals(importantViewField, FieldById(view.At, "field")) && importantViewField.V == "Retained important view value" && importantViewField.S.W == 150, "important view rebuild preserves field state");
                    var mediaLoad = view.GoA(mediaPath);
                    while (!mediaLoad.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    mediaLoad.GetAwaiter().GetResult();
                    using (var mediaPaint = Render(view))
                    {
                        Eq(11, view.At.Bs.Count, "responsive media render blocks");
                        Ok(PicCount(view) <= 24 && PicCached(view, view.At.Bs[2].U) && PicCached(view, view.At.Bs[4].U) &&
                            PicCached(view, view.At.Bs[6].U) && PicCached(view, view.At.Bs[8].U),
                            "bounded deduplicated responsive image cache keys");
                        var embeddedBox = BlockRect(view, view.At.Bs[2]);
                        var densityBox = BlockRect(view, view.At.Bs[4]);
                        var pictureBox = BlockRect(view, view.At.Bs[6]);
                        var widthBox = BlockRect(view, view.At.Bs[8]);
                        var cssBox = BlockRect(view, view.At.Bs[10]);
                        Eq(new Size(96, 54), embeddedBox.Size, "HTML-sized embedded image layout");
                        Eq(new Size(80, 48), densityBox.Size, "density image layout");
                        Eq(new Size(110, 56), pictureBox.Size, "picture image layout");
                        Eq(new Size(300, 60), widthBox.Size, "width candidate layout");
                        Eq(new Size(130, 44), cssBox.Size, "CSS-sized embedded image layout");
                        Ok(ColorDistance(CenterPixel(mediaPaint, embeddedBox), Color.FromArgb(23, 107, 77)) < 4, "embedded data image paint");
                        Ok(ColorDistance(CenterPixel(mediaPaint, densityBox), Color.FromArgb(126, 34, 206)) < 4, "density candidate paint");
                        Ok(ColorDistance(CenterPixel(mediaPaint, pictureBox), Color.FromArgb(29, 78, 216)) < 4, "picture source paint");
                        Ok(ColorDistance(CenterPixel(mediaPaint, cssBox), Color.FromArgb(23, 107, 77)) < 4, "CSS-sized data image paint");
                    }
                    view.Size = new Size(720, 900);
                    view.Go(mapPath);
                    using (var mapPaint = Render(view))
                    {
                        var mappedRect = BlockRect(view, BlockById(view.At, "mapped"));
                        Eq(new Size(320, 160), mappedRect.Size, "mapped image dimensions");
                        Eq(4, HitCount(view, false), "non-empty image map link hits");
                        Eq(new Rectangle(mappedRect.Left, mappedRect.Top, 100, 80), AreaHitRect(view, "rect-area"), "rectangle area hit bounds");
                        Eq(new Rectangle(mappedRect.Left + 125, mappedRect.Top + 5, 70, 70), AreaHitRect(view, "circle-area"), "circle area hit bounds");
                        Eq(new Rectangle(mappedRect.Left + 200, mappedRect.Top, 120, 80), AreaHitRect(view, "poly-area"), "polygon area hit bounds");
                        Eq(mappedRect, AreaHitRect(view, "default-area"), "default area covers image");
                        Eq("dead-area", AreaAt(view, new Point(mappedRect.Left + 10, mappedRect.Top + 10)), "dead area masks lower links");
                        Eq("rect-area", AreaAt(view, new Point(mappedRect.Left + 50, mappedRect.Top + 40)), "rectangle point hit");
                        Eq("circle-area", AreaAt(view, new Point(mappedRect.Left + 160, mappedRect.Top + 40)), "circle point hit");
                        Eq("poly-area", AreaAt(view, new Point(mappedRect.Left + 250, mappedRect.Top + 40)), "polygon point hit");
                        Eq("default-area", AreaAt(view, new Point(mappedRect.Left + 150, mappedRect.Top + 130)), "default fallback point hit");
                        Eq("default-area", AreaAt(view, new Point(mappedRect.Left + 80, mappedRect.Top + 120)), "empty circle creates no hit");
                        Ok(ColorDistance(CenterPixel(mapPaint, mappedRect), Color.FromArgb(23, 107, 77)) < 4, "mapped image remains normally painted");
                        Move(view, new Point(mappedRect.Left + 160, mappedRect.Top + 40));
                        Eq(Cursors.Hand, view.Cursor, "image map link cursor");
                        Move(view, new Point(mappedRect.Left + 10, mappedRect.Top + 10));
                        Eq(Cursors.Default, view.Cursor, "dead image map cursor");
                        Click(view, new Rectangle(mappedRect.Left + 9, mappedRect.Top + 9, 2, 2));
                        Eq("", view.U.Fragment, "dead image map click stays on page");
                        Click(view, new Rectangle(mappedRect.Left + 49, mappedRect.Top + 39, 2, 2));
                    }
                    WaitFor(delegate { return view.U.Fragment == "#rect-target"; }, "rectangle image map navigation");
                    view.Go(mapPath);
                    Paint(view);
                    Eq(1, view.Ck(ById(view.At.D, "style-map")), "WinForms image map class rebuild");
                    Paint(view);
                    var styledMapRect = BlockRect(view, BlockById(view.At, "mapped"));
                    var styledAreaRect = AreaHitRect(view, "rect-area");
                    Eq(240, styledMapRect.Width, "class-rebuilt mapped image width");
                    Eq(75, styledAreaRect.Width, "class-rebuilt area scaling");
                    view.Size = new Size(180, 900);
                    Paint(view);
                    var narrowMapRect = BlockRect(view, BlockById(view.At, "mapped"));
                    var narrowAreaRect = AreaHitRect(view, "rect-area");
                    Ok(narrowMapRect.Width > 0 && narrowMapRect.Width < styledMapRect.Width, "mapped image narrows with viewport");
                    Ok(narrowAreaRect.Width > 0 && narrowAreaRect.Width < styledAreaRect.Width && Math.Abs(narrowAreaRect.Width * styledMapRect.Width - styledAreaRect.Width * narrowMapRect.Width) <= styledMapRect.Width, "image map hit scales with rendered width");

                    view.Size = new Size(520, 280);
                    view.Go(keyboardPath);
                    Paint(view);
                    Eq(14, view.Kn, "bounded sequential keyboard target count");
                    Eq(0, view.Ki, "keyboard focus starts unset");
                    Ok(view.Ke == null && view.Kb.IsEmpty, "keyboard aliases start clear");
                    var focusOrder = new[] { "priority-one", "priority-area", "priority-area", "priority-two", "priority-button", "priority-generic", "default-first", "zero-link", "invalid-link", "text-field", "focus-summary", "default-area", "default-area", "last-link" };
                    for (var focusIndex = 0; focusIndex < focusOrder.Length; focusIndex++)
                    {
                        Eq(focusIndex + 1, view.Kf(), "forward keyboard focus index " + focusIndex);
                        Eq(focusOrder[focusIndex], view.Ke.Av("id"), "forward keyboard focus order " + focusIndex);
                        Ok(!view.Kb.IsEmpty, "focused keyboard target has visible bounds " + focusIndex);
                        if (focusIndex == 0)
                        {
                            Eq("#176b4d", BlockById(view.At, "priority-one").S.Bg, "keyboard focus reapplies link focus CSS");
                            var ring = view.Kb;
                            ring.Inflate(3, 3);
                            using (var focusPaint = Render(view)) Ok(HasColor(focusPaint, ring, SystemColors.Highlight), "default keyboard focus ring paints");
                        }
                        if (focusIndex == 1) Eq(new Size(100, 100), view.Kb.Size, "first image-map keyboard region uses scaled bounds");
                        if (focusIndex == 2) Eq(new Size(50, 50), view.Kb.Size, "shared map creates a distinct second keyboard region");
                    }
                    Ok(view.Sy > 0 && !view.Kb.IsEmpty, "off-screen keyboard target scrolls into view");
                    Eq(1, view.Kf(), "forward keyboard traversal wraps");
                    Eq("priority-one", view.Ke.Av("id"), "forward wrap target");
                    Eq(14, view.Kf(true), "backward keyboard traversal wraps");
                    Eq("last-link", view.Ke.Av("id"), "backward wrap target");
                    var reverseTab = Key(view, Keys.Shift | Keys.Tab);
                    Ok(reverseTab.Handled && reverseTab.SuppressKeyPress && view.Ke.Av("id") == "default-area" && view.Kb.Size == new Size(50, 50), "Shift+Tab uses backward focus traversal");

                    view.Go(keyboardPath);
                    Paint(view);
                    view.Kf();
                    view.Kf();
                    Eq("priority-area", view.Ke.Av("id"), "priority image-map area receives focus");
                    var areaEnter = Key(view, Keys.Enter);
                    WaitFor(delegate { return view.U.Fragment == "#area-target"; }, "Enter activates focused image-map area");
                    Ok(areaEnter.Handled && areaEnter.SuppressKeyPress, "image-map Enter is consumed");

                    view.Go(keyboardPath);
                    Paint(view);
                    Ok(InputKey(view, Keys.Tab) && InputKey(view, Keys.Shift | Keys.Tab) && !InputKey(view, Keys.Control | Keys.Tab), "page captures only plain forward and reverse Tab");
                    var forwardTab = Key(view, Keys.Tab);
                    Ok(forwardTab.Handled && forwardTab.SuppressKeyPress && view.Ke.Av("id") == "priority-one", "Tab enters bounded page focus order");
                    var controlTab = Key(view, Keys.Control | Keys.Tab);
                    Ok(!controlTab.Handled && view.Ke.Av("id") == "priority-one", "Ctrl+Tab remains available to browser chrome");
                    var linkSpace = Key(view, Keys.Space);
                    Ok(!linkSpace.Handled && view.U.Fragment.Length == 0, "Space does not activate focused link");
                    var linkEnter = Key(view, Keys.Enter);
                    WaitFor(delegate { return view.U.Fragment == "#one-target"; }, "Enter activates focused normal link");
                    Ok(linkEnter.Handled && linkEnter.SuppressKeyPress, "normal link Enter is consumed");
                    Eq("priority link activated", NodeText(ById(view.At.D, "focus-state")), "keyboard link activation runs safe command before navigation");

                    view.Go(keyboardPath);
                    Paint(view);
                    for (var buttonFocus = 0; buttonFocus < 5; buttonFocus++) view.Kf();
                    Eq("priority-button", view.Ke.Av("id"), "positive tabindex button order");
                    Eq(1, view.Ka().GetAwaiter().GetResult(), "compact keyboard activation alias");
                    Eq("button activated", NodeText(ById(view.At.D, "focus-state")), "keyboard button activation runs safe command");
                    view.Kf();
                    Eq("priority-generic", view.Ke.Av("id"), "generic tabindex region receives focus");
                    Eq(0, view.Ka().GetAwaiter().GetResult(), "generic tabindex region has no activation behavior");
                    Click(view, view.Kb);
                    Eq("priority-generic", view.Ke.Av("id"), "click focus retains generic tabindex region");
                    var negativeGenericRect = BlockRect(view, BlockById(view.At, "negative-generic"));
                    view.Sc(negativeGenericRect.Top - view.Sy - 40);
                    Paint(view);
                    negativeGenericRect.Offset(0, -view.Sy);
                    Click(view, negativeGenericRect);
                    Eq("negative-generic", view.Ke.Av("id"), "negative tabindex remains click focusable");
                    Eq(0, view.Ki, "negative tabindex stays outside sequential order");
                    Ok(!view.Kb.IsEmpty, "negative tabindex click focus keeps visible bounds");
                    var negativeRing = view.Kb;
                    negativeRing.Inflate(3, 3);
                    using (var negativeFocusPaint = Render(view)) Ok(HasColor(negativeFocusPaint, negativeRing, SystemColors.Highlight), "negative tabindex click focus ring paints");

                    view.Go(keyboardPath);
                    Paint(view);
                    for (var fieldFocus = 0; fieldFocus < 10; fieldFocus++) view.Kf();
                    Eq("text-field", view.Ke.Av("id"), "text field joins keyboard sequence");
                    Eq(1, view.Ka().GetAwaiter().GetResult(), "keyboard activation opens text editor");
                    var editorTab = EditKey(view, Keys.Tab);
                    Ok(editorTab.Handled && editorTab.SuppressKeyPress && view.Ke.Av("id") == "focus-summary", "Tab leaves active editor through page sequence");

                    view.Go(keyboardPath);
                    Paint(view);
                    for (var summaryFocus = 0; summaryFocus < 11; summaryFocus++) view.Kf();
                    Eq("focus-summary", view.Ke.Av("id"), "summary joins keyboard sequence");
                    var summarySpace = Key(view, Keys.Space);
                    Paint(view);
                    Ok(summarySpace.Handled && summarySpace.SuppressKeyPress && ById(view.At.D, "focus-details").Op, "Space activates focused summary");

                    view.Size = new Size(520, 300);
                    view.Go(shortcutPath);
                    Paint(view);
                    Eq("invalid-auto", view.Af().Av("id"), "view forwards first authored autofocus candidate");
                    Eq(3, view.Afs().Count, "view forwards bounded autofocus candidates");
                    Eq("auto-field", view.Ke.Av("id"), "autofocus skips non-focusable and hidden candidates");
                    Eq(1, view.Ki, "autofocus field retains sequential index");
                    Ok(!view.Kb.IsEmpty, "autofocus target has visible bounds");
                    Eq(6, view.An, "assigned access-key count excludes hidden and disabled targets");
                    Eq("S", view.Ak(ById(view.At.D, "auto-field")), "view assigned access-key alias");
                    Eq("Alt+Shift+B", view.Kl(ById(view.At.D, "run-button")), "platform access-key label alias");
                    Eq("X", view.Ak(ById(view.At.D, "fallback-button")), "conflicting access key uses authored fallback");
                    Eq("", view.Ak(ById(view.At.D, "disabled-key")), "disabled access key remains unassigned");
                    Ok(InputKey(view, Keys.Alt | Keys.Shift | Keys.B) && !InputKey(view, Keys.Alt | Keys.Shift | Keys.Q), "only assigned Alt+Shift page keys become input keys");
                    var unmatchedAccess = Key(view, Keys.Alt | Keys.Shift | Keys.Q);
                    Ok(!unmatchedAccess.Handled && !unmatchedAccess.SuppressKeyPress && view.Ke.Av("id") == "auto-field", "unmatched page shortcut remains available to host");
                    var hostShortcut = Key(view, Keys.Control | Keys.Shift | Keys.B);
                    Ok(!hostShortcut.Handled && view.Ke.Av("id") == "auto-field", "non-access modifier shortcut remains available to host");
                    var accessButton = Key(view, Keys.Alt | Keys.Shift | Keys.B);
                    WaitFor(delegate { return NodeText(ById(view.At.D, "shortcut-state")) == "button shortcut activated"; }, "Alt+Shift button activation");
                    Ok(accessButton.Handled && accessButton.SuppressKeyPress && view.Ke.Av("id") == "run-button", "assigned button shortcut is consumed and focused");
                    view.Go(shortcutPath);
                    Paint(view);
                    Ok(!CommandKey(view, Keys.Alt | Keys.Shift | Keys.Q), "unassigned WinForms command key remains available to host");
                    Ok(CommandKey(view, Keys.Alt | Keys.Shift | Keys.B), "assigned WinForms command key is accepted after host routing");
                    WaitFor(delegate { return NodeText(ById(view.At.D, "shortcut-state")) == "button shortcut activated"; }, "WinForms command-key button activation");
                    Eq(1, view.Aa("r").GetAwaiter().GetResult(), "compact focus-only access activation alias");
                    Eq("focus-region", view.Ke.Av("id"), "focus-only access target receives focus");
                    Eq(0, view.Ki, "negative tabindex access target stays outside sequential order");
                    Eq(1, view.Aa("x").GetAwaiter().GetResult(), "fallback access key activation");
                    Eq("fallback shortcut activated", NodeText(ById(view.At.D, "shortcut-state")), "fallback access key runs safe command");
                    Eq(1, view.Aa("z").GetAwaiter().GetResult(), "valid token after invalid access token activates");
                    Eq("token shortcut activated", NodeText(ById(view.At.D, "shortcut-state")), "later valid token runs safe command");
                    Eq(0, view.Aa("d").GetAwaiter().GetResult(), "disabled shortcut does not activate");

                    view.Go(shortcutPath);
                    Paint(view);
                    Eq(1, view.Aa("g").GetAwaiter().GetResult(), "link access key activation");
                    Eq("#target", view.U.Fragment, "link access key retains fragment route");
                    Eq("link shortcut activated", NodeText(ById(view.At.D, "shortcut-state")), "link access key runs safe command before navigation");

                    view.Go(shortcutPath);
                    Paint(view);
                    Eq(1, view.Aa("s").GetAwaiter().GetResult(), "editable field access key activation");
                    Ok(Editor(view).Visible && view.Ke.Av("id") == "auto-field", "editable access key opens the native editor");
                    var editorAccess = EditKey(view, Keys.Alt | Keys.Shift | Keys.X);
                    WaitFor(delegate { return NodeText(ById(view.At.D, "shortcut-state")) == "fallback shortcut activated"; }, "editor access-key activation");
                    Ok(editorAccess.Handled && editorAccess.SuppressKeyPress && !Editor(view).Visible, "assigned shortcut leaves active editor and is consumed");

                    var shortcutFragment = new UriBuilder(new Uri(shortcutPath)) { Fragment = "target" }.Uri;
                    view.Go(shortcutFragment);
                    Paint(view);
                    Ok(view.Ke == null && view.Kb.IsEmpty && view.Sy > 0, "fragment navigation suppresses autofocus and scrolls target");
                    var missingShortcutFragment = new UriBuilder(new Uri(shortcutPath)) { Fragment = "missing-target" }.Uri;
                    view.Go(missingShortcutFragment);
                    Paint(view);
                    Eq("auto-field", view.Ke.Av("id"), "unresolved fragment does not suppress autofocus");
                    view.Go(shortcutPath);
                    Paint(view);
                    Eq("auto-field", view.Ke.Av("id"), "new unfragmented load reapplies autofocus");

                    var boundedFocusPath = Path.Combine(Path.GetTempPath(), "lea-keyboard-focus-" + Guid.NewGuid().ToString("N") + ".html");
                    try
                    {
                        var boundedFocus = new StringBuilder("<!doctype html><html><body>");
                        for (var keyIndex = 0; keyIndex < 300; keyIndex++) boundedFocus.Append("<a id='k").Append(keyIndex).Append("' href='#k").Append(keyIndex).Append("'>key</a>");
                        boundedFocus.Append("</body></html>");
                        File.WriteAllText(boundedFocusPath, boundedFocus.ToString(), new UTF8Encoding(false));
                        view.Go(boundedFocusPath);
                        Paint(view);
                        Eq(256, view.Kn, "keyboard target count cap");
                        Eq(1, view.Kf(), "bounded keyboard first target");
                        Eq("k0", view.Ke.Av("id"), "bounded keyboard preserves document order");
                        Eq(256, view.Kf(true), "bounded keyboard reverse wrap");
                        Eq("k255", view.Ke.Av("id"), "bounded keyboard drops targets after cap");
                    }
                    finally
                    {
                        if (File.Exists(boundedFocusPath)) File.Delete(boundedFocusPath);
                    }
                    view.Size = new Size(720, 500);
                    view.Go(positioningPath);
                    using (var positioningPaint = Render(view))
                    {
                        Eq(8, view.Bc, "positioning layout boxes");
                        Eq(4, HitCount(view, false), "positioning link hits");
                        var backBox = BlockRect(view, view.At.Bs[1]);
                        var frontBox = BlockRect(view, view.At.Bs[2]);
                        var positionedRightBox = BlockRect(view, view.At.Bs[3]);
                        var staticBox = BlockRect(view, view.At.Bs[4]);
                        Eq(backBox.Top, frontBox.Top, "relative top preserves normal flow slot");
                        Eq(backBox.Left + 20, frontBox.Left, "relative left offsets");
                        Eq(positionedRightBox.Left + 35, backBox.Left, "relative right offset");
                        Eq(positionedRightBox.Top + 92, staticBox.Top, "relative bottom preserves following flow");
                        Eq(staticBox.Left + 20, backBox.Left, "static left offset ignored");
                        var tableBox = BlockRect(view, view.At.Bs[5]);
                        Eq(staticBox.Left + 12, tableBox.Left, "relative table horizontal offset");
                        Ok(tableBox.Top > staticBox.Bottom, "relative table retains normal flow");
                        var overlap = Rectangle.Intersect(backBox, frontBox);
                        Ok(overlap.Width > 200 && overlap.Height == 70, "positioned layer overlap");
                        var topPixel = positioningPaint.GetPixel(overlap.Right - 10, overlap.Bottom - 10);
                        Ok(ColorDistance(topPixel, Color.FromArgb(29, 78, 216)) < 4, "z-index top layer paint");
                        Click(view, new Rectangle(overlap.Right - 12, overlap.Bottom - 12, 4, 4));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Dynamic State"; }, "topmost overlap link navigation");
                    view.Size = new Size(720, 300);
                    view.Go(stickyPath);
                    using (var stickyPaint = Render(view))
                    {
                        var rail = BlockRect(view, BlockById(view.At, "rail"));
                        var sticky = BlockOuterRect(view, BlockById(view.At, "sticky"));
                        var child = BlockOuterRect(view, BlockById(view.At, "sticky-child"));
                        var follower = BlockOuterRect(view, BlockById(view.At, "follower"));
                        Ok(sticky.Top >= rail.Top && sticky.Bottom < follower.Top, "sticky begins in normal flow");
                        view.Sc(360);
                        using (var scrolledStickyPaint = Render(view))
                        {
                            var moved = BlockOuterRect(view, BlockById(view.At, "sticky"));
                            var movedChild = BlockOuterRect(view, BlockById(view.At, "sticky-child"));
                            Eq(view.Sy + 14, moved.Top, "sticky top follows viewport inset");
                            Eq(child.Top - sticky.Top, movedChild.Top - moved.Top, "sticky descendant follows owner offset");
                            Eq(follower, BlockOuterRect(view, BlockById(view.At, "follower")), "sticky preserves following flow geometry");
                            var stickyHit = HitRect(view, "second.html");
                            Ok(!stickyHit.IsEmpty && stickyHit.Top >= 14 && stickyHit.Bottom <= view.ClientSize.Height, "sticky hit follows painted geometry");
                            var stickyBounds = view.Ab(view.Ax(ById(view.At.D, "sticky-link")));
                            Ok(!stickyBounds.IsEmpty && stickyBounds.Contains(new Point(stickyHit.Left + 1, stickyHit.Top + 1)), "sticky accessibility follows visible geometry");
                            Ok(ColorDistance(scrolledStickyPaint.GetPixel(moved.Left + 3, moved.Top - view.Sy + 3), Color.FromArgb(29, 78, 216)) < 8, "sticky owner remains painted at inset");
                            var fieldHit = HitElementRect(view, "sticky-field");
                            Click(view, fieldHit);
                            Ok(Editor(view).Visible && Editor(view).Bounds.Top >= moved.Top - view.Sy, "sticky native editor uses moved geometry");
                        }
                    }
                    view.Go(stickyPath);
                    using (var stickyResetPaint = Render(view)) { }
                    var stickyRail = BlockRect(view, BlockById(view.At, "rail"));
                    var stickyFollower = BlockOuterRect(view, BlockById(view.At, "follower"));
                    view.Sc(stickyRail.Bottom + 120);
                    using (var clampedStickyPaint = Render(view))
                    {
                        var clamped = BlockOuterRect(view, BlockById(view.At, "sticky"));
                        Eq(stickyRail.Bottom, clamped.Bottom, "sticky stops at containing block bottom");
                        Eq(stickyFollower, BlockOuterRect(view, BlockById(view.At, "follower")), "sticky clamp keeps normal flow slot");
                        Ok(HitRect(view, "second.html").IsEmpty, "clamped offscreen sticky link has no visible hit");
                    }
                    view.Size = new Size(720, 500);
                    view.Go(contextsPath);
                    using (var contextPaint = Render(view))
                    {
                        Eq(16, view.Bc, "absolute and fixed layout boxes");
                        Eq(3, HitCount(view, false), "clipped position link hits");
                        var flowA = BlockRect(view, view.At.Bs[1]);
                        var absolute = BlockRect(view, view.At.Bs[2]);
                        var flowB = BlockRect(view, view.At.Bs[3]);
                        var croppedTable = BlockRect(view, view.At.Bs[4]);
                        var absoluteRight = BlockRect(view, view.At.Bs[11]);
                        var fixedBox = BlockRect(view, view.At.Bs[12]);
                        Eq(flowA.Bottom + 28, flowB.Top, "absolute block leaves no flow slot");
                        Eq(358, absolute.Left, "absolute left anchors to page content");
                        Eq(144, absolute.Top, "absolute top anchors to initial content context");
                        Eq(658, absoluteRight.Right, "absolute right anchors to page content");
                        Eq(413, absoluteRight.Bottom, "absolute bottom anchors to viewport");
                        Eq(562, fixedBox.Left, "fixed right anchor with negative offset");
                        Eq(474, fixedBox.Bottom, "fixed bottom viewport anchor");
                        Eq(croppedTable, OwnedClip(view, view.At.Bs[4], "td"), "table children inherit positioned overflow clip");
                        Ok(fixedBox.Right > view.ClientSize.Width, "fixed layout can extend beyond viewport");
                        var fixedHit = HitRect(view, "dynamic-state.html");
                        Eq(view.ClientSize.Width, fixedHit.Right, "fixed link hit clips to viewport");
                        Ok(ColorDistance(CenterPixel(contextPaint, absolute), Color.FromArgb(185, 28, 28)) < 4, "absolute layer paint");
                        var fixedVisible = Rectangle.Intersect(fixedBox, view.ClientRectangle);
                        Ok(ColorDistance(CenterPixel(contextPaint, fixedVisible), Color.FromArgb(29, 78, 216)) < 4, "fixed layer paint");
                    }
                    var fixedBeforeScroll = BlockRect(view, view.At.Bs[12]);
                    view.Sc(210);
                    using (var scrolledContextPaint = Render(view))
                    {
                        Eq(210, view.Sy, "position context scroll amount");
                        Eq(fixedBeforeScroll, BlockRect(view, view.At.Bs[12]), "fixed layout stays in viewport coordinates");
                        var fixedVisible = Rectangle.Intersect(fixedBeforeScroll, view.ClientRectangle);
                        Ok(ColorDistance(CenterPixel(scrolledContextPaint, fixedVisible), Color.FromArgb(29, 78, 216)) < 4, "fixed layer remains painted after scroll");
                        var absolute = BlockRect(view, view.At.Bs[2]);
                        Ok(ColorDistance(scrolledContextPaint.GetPixel(absolute.Left + absolute.Width / 2, absolute.Top + absolute.Height / 2), Color.FromArgb(185, 28, 28)) > 40, "absolute layer scrolls with document");
                        Click(view, HitRect(view, "dynamic-state.html"));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Dynamic State"; }, "fixed clipped link navigation after scroll");
                    view.Size = new Size(720, 500);
                    view.Go(positionValuesPath);
                    using (var positionValuesPaint = Render(view))
                    {
                        Ok(view.Bc > view.At.Bs.Count, "inset and translate inline paint boxes");
                        Ok(!HitRect(view, "second.html").IsEmpty && !HitRect(view, "forms.html").IsEmpty && !HitRect(view, "form-result.html").IsEmpty && !HitRect(view, "dynamic-state.html").IsEmpty, "inset and translate link hits");
                        var insetOwner = BlockRect(view, view.At.Bs[1]);
                        var stretchX = BlockOuterRect(view, view.At.Bs[2]);
                        var stretchY = BlockOuterRect(view, view.At.Bs[3]);
                        var corner = BlockOuterRect(view, view.At.Bs[4]);
                        var anchor = BlockRect(view, view.At.Bs[5]);
                        var translated = BlockRect(view, view.At.Bs[6]);
                        var translatedChild = BlockOuterRect(view, view.At.Bs[7]);
                        var translateCover = BlockOuterRect(view, view.At.Bs[8]);
                        var afterValues = BlockOuterRect(view, view.At.Bs[9]);
                        Eq(insetOwner.Left + 24, stretchX.Left, "opposing inset stretch left");
                        Eq(insetOwner.Right - 20, stretchX.Right, "opposing inset stretch right");
                        Eq(insetOwner.Top + 14, stretchX.Top, "opposing inset stretch top");
                        Eq(52, stretchX.Height, "explicit inset block height");
                        Eq(insetOwner.Left + 24, stretchY.Left, "four-value inset left");
                        Eq(insetOwner.Right - 190, stretchY.Right, "four-value inset right");
                        Eq(insetOwner.Top + 76, stretchY.Top, "opposing vertical inset top");
                        Eq(insetOwner.Bottom - 12, stretchY.Bottom, "opposing vertical inset stretch bottom");
                        Eq(insetOwner.Right - 18, corner.Right, "auto inset right anchor");
                        Eq(insetOwner.Bottom - 14, corner.Bottom, "auto inset bottom anchor");
                        Ok(afterValues.Top > BlockOuterRect(view, view.At.Bs[1]).Bottom, "inset children leave owner flow intact");
                        Eq(anchor.Left + 18, translated.Left, "translate x applies after absolute position");
                        Eq(anchor.Top + 10, translated.Top, "translate y applies after absolute position");
                        Eq(translated.Left + 30, translatedChild.Left, "translated child follows parent x");
                        Eq(translated.Top + 18, translatedChild.Top, "translated child follows parent y");
                        Eq(translatedChild, translateCover, "translated context overlap geometry");
                        Ok(ColorDistance(CenterPixel(positionValuesPaint, translateCover), Color.FromArgb(23, 107, 77)) < 4, "translated context traps high child below root cover");
                        Click(view, HitRect(view, "dynamic-state.html"));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Dynamic State"; }, "translated context overlap click");
                    view.Size = new Size(720, 500);
                    view.Go(nestedPositionPath);
                    using (var nestedPaint = Render(view))
                    {
                        Eq(42, view.Bc, "nested context layout boxes");
                        Eq(26, HitCount(view, false), "nested context link hits include ten visible internal spaces");
                        var low = BlockRect(view, view.At.Bs[1]);
                        var clipper = BlockRect(view, view.At.Bs[2]);
                        var cropped = BlockRect(view, view.At.Bs[3]);
                        var escape = BlockRect(view, view.At.Bs[4]);
                        var high = BlockRect(view, view.At.Bs[5]);
                        var cover = BlockRect(view, view.At.Bs[6]);
                        var fixedParent = BlockRect(view, view.At.Bs[7]);
                        var fixedChild = BlockRect(view, view.At.Bs[8]);
                        Eq(low.Left + 2, clipper.Left, "nested normal flow uses parent content left");
                        Eq(low.Top + 2, clipper.Top, "nested normal flow uses parent content top");
                        Eq(clipper.Left + 72, cropped.Left, "absolute grandchild uses nearest containing block left");
                        Eq(clipper.Top + 6, cropped.Top, "absolute grandchild uses nearest containing block top");
                        Eq(low.Right - 190, escape.Left, "absolute child uses low parent right");
                        Eq(low.Bottom - 125, escape.Top, "absolute child uses low parent bottom");
                        Eq(high.Left + 30, cover.Left, "stacked child uses high parent left");
                        Eq(high.Top + 25, cover.Top, "stacked child uses high parent top");
                        Eq(escape, cover, "nested stacking layers overlap exactly");
                        Eq(clipper, ClipRect(view, view.At.Bs[3]), "ancestor overflow clips absolute grandchild");
                        var croppedHit = HitRect(view, "form-result.html");
                        Ok(!croppedHit.IsEmpty && croppedHit.Right <= clipper.Right, "nested clipped link hit boundary");
                        Ok(ColorDistance(CenterPixel(nestedPaint, cover), Color.FromArgb(126, 34, 206)) < 4, "parent stacking context contains high child z-index");
                        Eq(fixedParent.Left, fixedChild.Left, "fixed descendant uses parent content left");
                        Eq(fixedParent.Top, fixedChild.Top, "fixed descendant uses parent content top");
                        Ok(BlockRect(view, view.At.Bs[9]).Top > low.Bottom, "nested children do not consume root flow slots");
                        Click(view, HitRect(view, "dynamic-state.html"));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Dynamic State"; }, "higher parent context wins overlap click");
                    view.Go(nestedPositionPath);
                    Paint(view);
                    var fixedNestedBeforeScroll = BlockRect(view, view.At.Bs[8]);
                    view.Sc(220);
                    using (var nestedScrollPaint = Render(view))
                    {
                        Eq(220, view.Sy, "nested context scroll amount");
                        Eq(fixedNestedBeforeScroll, BlockRect(view, view.At.Bs[8]), "fixed descendant remains in viewport coordinates");
                        Ok(ColorDistance(CenterPixel(nestedScrollPaint, fixedNestedBeforeScroll), Color.FromArgb(29, 78, 216)) < 4, "fixed descendant remains painted after scroll");
                        Click(view, HitRect(view, "forms.html"));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Forms Test"; }, "fixed descendant link navigation after scroll");
                    view.Size = new Size(720, 700);
                    view.Go(nestedFlowPath);
                    int wideOuterWidth;
                    using (var nestedFlowPaint = Render(view))
                    {
                        Ok(view.Bc > view.At.Bs.Count, "nested flow creates child and inline paint boxes");
                        var outer = BlockRect(view, view.At.Bs[1]);
                        var outerBorder = BlockOuterRect(view, view.At.Bs[1]);
                        var first = BlockRect(view, view.At.Bs[2]);
                        var middle = BlockRect(view, view.At.Bs[3]);
                        var inner = BlockRect(view, view.At.Bs[4]);
                        var innerHeading = BlockRect(view, view.At.Bs[5]);
                        var innerText = BlockRect(view, view.At.Bs[6]);
                        var middleTail = BlockRect(view, view.At.Bs[7]);
                        var outerTail = BlockRect(view, view.At.Bs[8]);
                        var mixed = BlockRect(view, view.At.Bs[9]);
                        var mixedBorder = BlockOuterRect(view, view.At.Bs[9]);
                        var mixedChild = BlockRect(view, view.At.Bs[10]);
                        var after = BlockRect(view, view.At.Bs[11]);
                        wideOuterWidth = outer.Width;
                        Eq(360, outer.Width, "wide auto container keeps declared content width");
                        Eq(outer.Left + 4, first.Left, "outer child uses parent content and own padding");
                        Eq(outer.Left + 34, middle.Left, "middle child uses nested margin border and padding");
                        Eq(middle.Left + 19, inner.Left, "third-level child uses nested content coordinates");
                        Ok(inner.Bottom > innerText.Bottom && innerText.Top > innerHeading.Bottom, "inner auto height contains ordered children");
                        Ok(middle.Bottom > middleTail.Bottom && middleTail.Top > BlockOuterRect(view, view.At.Bs[4]).Bottom, "middle auto height resumes after inner subtree");
                        Ok(outer.Bottom > outerTail.Bottom && outerTail.Top > BlockOuterRect(view, view.At.Bs[3]).Bottom, "outer auto height resumes after middle subtree");
                        var mixedLines = TextLines(view, view.At.Bs[9]);
                        Ok(mixedLines.Length == 1 && mixedChild.Top > mixedLines[0].Bottom && mixed.Bottom > mixedChild.Bottom, "mixed direct text precedes nested child inside auto height");
                        Ok(after.Top > outerBorder.Bottom && after.Top > mixedBorder.Bottom, "root flow continues after finalized containers");
                        var nestedLink = HitRect(view, "second.html");
                        Ok(!nestedLink.IsEmpty && nestedLink.Left >= inner.Left && nestedLink.Right <= inner.Right && nestedLink.Top >= inner.Top && nestedLink.Bottom <= inner.Bottom, "nested link hit stays inside all visual parents");
                        Ok(ColorDistance(nestedFlowPaint.GetPixel(outerBorder.Left + 4, outerBorder.Top + 4), Color.FromArgb(232, 245, 239)) < 4, "auto container background paints through padding");
                        Click(view, nestedLink);
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "LeaForge Second Page"; }, "nested flow link navigation");
                    view.Size = new Size(320, 700);
                    view.Go(nestedFlowPath);
                    Paint(view);
                    var narrowOuter = BlockRect(view, view.At.Bs[1]);
                    var narrowMiddle = BlockRect(view, view.At.Bs[3]);
                    var narrowInnerBorder = BlockOuterRect(view, view.At.Bs[4]);
                    Ok(narrowOuter.Width < wideOuterWidth, "nested visual container reflows to narrow viewport");
                    Ok(narrowInnerBorder.Right <= narrowMiddle.Right, "deep nested box remains within narrowed parent");
                    Ok(BlockRect(view, view.At.Bs[11]).Top > BlockOuterRect(view, view.At.Bs[9]).Bottom, "narrow root flow follows expanded mixed container");
                    view.Size = new Size(720, 900);
                    var nestedContentLoad = view.GoA(nestedContentPath);
                    while (!nestedContentLoad.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    nestedContentLoad.GetAwaiter().GetResult();
                    using (var nestedContentPaint = Render(view))
                    {
                        var panel = BlockRect(view, view.At.Bs[1]);
                        var panelBorder = BlockOuterRect(view, view.At.Bs[1]);
                        var nameField = BlockOuterRect(view, view.At.Bs[2]);
                        var fieldClipper = BlockRect(view, view.At.Bs[3]);
                        var clippedField = BlockOuterRect(view, view.At.Bs[4]);
                        var clippedFieldClip = OwnedClip(view, view.At.Bs[4], "input");
                        var nestedImage = BlockOuterRect(view, view.At.Bs[5]);
                        var nestedTable = BlockOuterRect(view, view.At.Bs[6]);
                        var panelTail = BlockOuterRect(view, view.At.Bs[7]);
                        var afterContent = BlockOuterRect(view, view.At.Bs[8]);
                        Ok(nameField.Top >= panel.Top && fieldClipper.Top > nameField.Bottom, "nested form fields advance parent flow");
                        Ok(clippedField.Right > fieldClipper.Right && clippedFieldClip == fieldClipper, "positioned field inherits parent overflow clip");
                        Ok(nestedImage.Top > BlockOuterRect(view, view.At.Bs[3]).Bottom && nestedTable.Top > nestedImage.Bottom, "nested media and table stay in ordered flow");
                        Ok(panelTail.Top > nestedTable.Bottom && panel.Bottom > panelTail.Bottom, "nested table contributes to automatic parent height");
                        Ok(afterContent.Top > panelBorder.Bottom, "root flow follows mixed-content automatic height");
                        Ok(nestedImage.Right <= panel.Right && nestedTable.Right <= panel.Right, "nested media and table fit parent content width");
                        Ok(ColorDistance(CenterPixel(nestedContentPaint, nestedImage), Color.FromArgb(23, 107, 77)) < 4, "nested image paints from embedded source");
                    }
                    Paint(view);
                    var editorTarget = Rectangle.Intersect(BlockOuterRect(view, view.At.Bs[4]), OwnedClip(view, view.At.Bs[4], "input"));
                    Click(view, editorTarget);
                    Paint(view);
                    var nestedEditor = Editor(view);
                    var editorClip = OwnedClip(view, view.At.Bs[4], "input");
                    Ok(nestedEditor.Visible && nestedEditor.Bounds.Right <= editorClip.Right && nestedEditor.Bounds.Bottom <= editorClip.Bottom, "native field editor obeys ancestor clip");
                    nestedEditor.Text = "Edited inside clip";
                    Eq("Edited inside clip", view.At.Fs[1].V, "clipped editor updates nested field");
                    view.Size = new Size(720, 800);
                    view.Go(stackingPath);
                    using (var stackingPaint = Render(view))
                    {
                        var opacityChild = BlockOuterRect(view, view.At.Bs[2]);
                        var opacityCover = BlockOuterRect(view, view.At.Bs[3]);
                        var isolationChild = BlockOuterRect(view, view.At.Bs[5]);
                        var isolationCover = BlockOuterRect(view, view.At.Bs[6]);
                        var autoChild = BlockOuterRect(view, view.At.Bs[8]);
                        var autoCover = BlockOuterRect(view, view.At.Bs[9]);
                        var zeroChild = BlockOuterRect(view, view.At.Bs[11]);
                        var zeroCover = BlockOuterRect(view, view.At.Bs[12]);
                        Eq(opacityChild, opacityCover, "opacity context overlap geometry");
                        Eq(isolationChild, isolationCover, "isolation context overlap geometry");
                        Eq(autoChild, autoCover, "z-index auto overlap geometry");
                        Eq(zeroChild, zeroCover, "numeric zero overlap geometry");
                        Ok(ColorDistance(CenterPixel(stackingPaint, opacityCover), Color.FromArgb(126, 34, 206)) < 4, "opacity context traps high child below root z1");
                        Ok(ColorDistance(CenterPixel(stackingPaint, isolationCover), Color.FromArgb(23, 107, 77)) < 4, "isolation context traps high child below root z1");
                        Ok(ColorDistance(CenterPixel(stackingPaint, autoChild), Color.FromArgb(29, 78, 216)) < 4, "z-index auto parent does not trap high child");
                        Ok(ColorDistance(CenterPixel(stackingPaint, zeroCover), Color.FromArgb(15, 118, 110)) < 4, "numeric zero parent traps high child");
                        var autoSized = BlockRect(view, view.At.Bs[13]);
                        var normalChild = BlockOuterRect(view, view.At.Bs[14]);
                        var bottomChild = BlockOuterRect(view, view.At.Bs[15]);
                        var afterContext = BlockOuterRect(view, view.At.Bs[16]);
                        Ok(autoSized.Bottom > normalChild.Bottom, "positioned auto height includes normal child");
                        Eq(autoSized.Right - 8, bottomChild.Right, "right anchor uses finalized auto width");
                        Eq(autoSized.Bottom + 12, bottomChild.Bottom, "bottom anchor uses finalized auto height");
                        Eq(autoSized, ClipRect(view, view.At.Bs[15]), "finalized overflow clip reaches positioned child");
                        var bottomHit = HitRect(view, "short.html");
                        Ok(!bottomHit.IsEmpty && bottomHit.Bottom <= autoSized.Bottom, "negative bottom link hit clips to finalized parent");
                        Ok(afterContext.Top > BlockOuterRect(view, view.At.Bs[13]).Bottom, "root flow follows positioned auto-height context");
                        Click(view, HitRect(view, "dynamic-state.html"));
                    }
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Dynamic State"; }, "opacity context overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "second.html"));
                    WaitFor(delegate { return !view.Busy && view.T == "LeaForge Second Page"; }, "isolation context overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "form-widgets.html"));
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Form Validation"; }, "z-index auto overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "images.html"));
                    WaitFor(delegate { return !view.Busy && view.T == "Lea Image Test"; }, "numeric zero overlap click");
                    view.Go(textStylePath);
                    Paint(view);
                    Eq(8, view.At.Bs.Count, "text style render blocks");
                    using (var inheritedFont = RenderFont(view, "p", view.At.Bs[1].S)) Eq("Segoe UI", inheritedFont.FontFamily.Name, "installed inherited font");
                    using (var serifFont = RenderFont(view, "p", view.At.Bs[2].S)) Eq("Georgia", serifFont.FontFamily.Name, "installed fallback font");
                    using (var monoFont = RenderFont(view, "p", view.At.Bs[3].S)) Eq("Consolas", monoFont.FontFamily.Name, "installed generic font");
                    using (var underlineFont = RenderFont(view, "span", view.At.Bs[4].Is[0].S, false, "p")) Ok(HasFontStyle(underlineFont, FontStyle.Underline) && !HasFontStyle(underlineFont, FontStyle.Strikeout), "painted underline style");
                    using (var strikeFont = RenderFont(view, "span", view.At.Bs[4].Is[1].S, false, "p")) Ok(HasFontStyle(strikeFont, FontStyle.Strikeout) && !HasFontStyle(strikeFont, FontStyle.Underline), "painted strike style");
                    using (var bothFont = RenderFont(view, "span", view.At.Bs[4].Is[2].S, false, "p")) Ok(HasFontStyle(bothFont, FontStyle.Underline) && HasFontStyle(bothFont, FontStyle.Strikeout), "painted combined decoration");
                    using (var plainNativeFont = RenderFont(view, "u", view.At.Bs[5].Is[3].S, false, "p")) Ok(!HasFontStyle(plainNativeFont, FontStyle.Underline), "native underline cascade override");
                    using (var plainLinkFont = RenderFont(view, "a", view.At.Bs[6].Is[0].S, true, "p")) Ok(!HasFontStyle(plainLinkFont, FontStyle.Underline), "plain link removes underline");
                    using (var defaultLinkFont = RenderFont(view, "a", view.At.Bs[6].Is[1].S, true, "p")) Ok(HasFontStyle(defaultLinkFont, FontStyle.Underline), "default link keeps underline");
                    Ok(FontCacheCount(view) > 0 && FontCacheCount(view) <= 32, "bounded font family cache");
                    view.Go(recoveryPath);
                    Paint(view);
                    Eq(23, view.At.Bs.Count, "recovery render blocks");
                    Ok(view.Bc > view.At.Bs.Count, "recovered inline content paints separately");
                    var h4 = BlockRect(view, view.At.Bs[1]);
                    var h5 = BlockRect(view, view.At.Bs[19]);
                    var h6 = BlockRect(view, view.At.Bs[20]);
                    Ok(h4.Height > h5.Height && h5.Height >= h6.Height, "heading levels four through six scale down");
                    Eq(2, BlockRect(view, view.At.Bs[21]).Height, "horizontal rule height");
                    view.Size = new Size(720, 500);
                    view.Go(listPath);
                    Paint(view);
                    var listWideLines = TextLines(view, BlockById(view.At, "wrap")).Length;
                    Ok(BlockRect(view, BlockById(view.At, "minus-two")).Left > BlockRect(view, BlockById(view.At, "parent")).Left, "nested list indentation");
                    Ok(PaintedText(view, BlockById(view.At, "three")).StartsWith("3. ", StringComparison.Ordinal), "ordered marker paints on plain flow");
                    Ok(PaintedText(view, BlockById(view.At, "four")).StartsWith("4. ", StringComparison.Ordinal), "ordered marker paints on inline flow");
                    Ok(PaintedText(view, BlockById(view.At, "bullet-two")).StartsWith("- ", StringComparison.Ordinal), "unordered marker paints on inline flow");
                    Ok(PaintedText(view, BlockById(view.At, "minus-ten")).StartsWith("-10. ", StringComparison.Ordinal), "nested value marker paints");
                    Ok(LinkBoxes(view, "second.html") > 0, "list marker keeps inline link hit boxes");
                    using (var listPaint = Render(view))
                        Ok(ColorDistance(listPaint.GetPixel(2, 2), Color.FromArgb(247, 251, 255)) < 4, "list fixture paints page background");
                    view.Size = new Size(300, 500);
                    Paint(view);
                    Ok(TextLines(view, BlockById(view.At, "wrap")).Length > listWideLines, "list item reflows after resize");

                    view.Size = new Size(720, 760);
                    view.Go(disclosurePath);
                    Paint(view);
                    var viewClosedSummary = BlockById(view.At, "closed-summary");
                    var viewClosedField = FieldById(view.At, "closed-field");
                    var closedDisclosureHeight = view.Dh;
                    var closedSummaryRect = BlockRect(view, viewClosedSummary);
                    Ok(!closedSummaryRect.IsEmpty && BlockRect(view, BlockById(view.At, "closed-content")).IsEmpty, "closed disclosure layout");
                    Ok(BlockRect(view, FieldBlock(view.At, viewClosedField)).IsEmpty, "closed disclosure field stays out of layout");
                    Ok(PaintedText(view, viewClosedSummary).StartsWith("> ", StringComparison.Ordinal), "closed summary marker paints");
                    Move(view, closedSummaryRect);
                    Ok(Object.ReferenceEquals(Cursors.Hand, view.Cursor), "summary exposes pointer cursor");
                    viewClosedField.V = "Edited in view";
                    Click(view, closedSummaryRect);
                    Paint(view);
                    Ok(ById(view.At.D, "closed").Op && !BlockRect(view, BlockById(view.At, "closed-content")).IsEmpty, "pointer opens disclosure");
                    Ok(view.Dh > closedDisclosureHeight && PaintedText(view, BlockById(view.At, "closed-summary")).StartsWith("v ", StringComparison.Ordinal), "open disclosure expands and paints marker");
                    Ok(Object.ReferenceEquals(viewClosedField, FieldById(view.At, "closed-field")) && viewClosedField.V == "Edited in view", "view disclosure preserves field state");
                    var enter = Key(view, Keys.Enter);
                    Paint(view);
                    Ok(enter.Handled && enter.SuppressKeyPress && !ById(view.At.D, "closed").Op, "Enter closes focused disclosure");
                    var space = Key(view, Keys.Space);
                    Paint(view);
                    Ok(space.Handled && space.SuppressKeyPress && ById(view.At.D, "closed").Op, "Space opens focused disclosure");

                    var duplicateState = ById(view.At.D, "duplicate").Op;
                    var strayRect = BlockRect(view, BlockById(view.At, "stray-summary"));
                    Move(view, strayRect);
                    Ok(!Object.ReferenceEquals(Cursors.Hand, view.Cursor), "stray summary has no disclosure cursor");
                    Click(view, strayRect);
                    Ok(ById(view.At.D, "duplicate").Op == duplicateState, "stray summary cannot toggle a disclosure");

                    var wideSummaryLines = TextLines(view, BlockById(view.At, "wrap-summary")).Length;
                    view.Size = new Size(300, 760);
                    Paint(view);
                    Ok(TextLines(view, BlockById(view.At, "wrap-summary")).Length > wideSummaryLines, "summary reflows after resize");
                    view.Size = new Size(720, 760);
                    Paint(view);
                    var cellSummary = BlockById(view.At, "cell-summary");
                    var disclosureCellLines = TextLines(view, cellSummary);
                    Ok(disclosureCellLines.Length > 0 && BlockRect(view, BlockById(view.At, "cell-content")).IsEmpty, "table cell summary paints while content is closed");
                    Click(view, disclosureCellLines[0]);
                    Paint(view);
                    Ok(ById(view.At.D, "cell-details").Op && !BlockRect(view, BlockById(view.At, "cell-content")).IsEmpty, "table cell summary toggles visible content");

                    view.Size = new Size(620, 420);
                    view.Go(fragmentPath);
                    Paint(view);
                    var retainedFragmentPage = view.At;
                    var viewFragmentRequests = view.Dg().Rs.Count;
                    var viewFragmentField = FieldById(view.At, "fragment-field");
                    viewFragmentField.V = "Edited in fragment view";
                    Eq("encoded target", view.Ft("#encoded%20target").Av("id"), "WinForms fragment target lookup");
                    var targetHit = FragmentHitRect(view, "#target");
                    Ok(!targetHit.IsEmpty, "same-page fragment link hit target");
                    Click(view, targetHit);
                    Paint(view);
                    Ok(Object.ReferenceEquals(retainedFragmentPage, view.At) && view.U.Fragment == "#target", "fragment link retains rendered page and updates URL");
                    Ok(view.Sy > 0 && ElementVisible(view, ById(view.At.D, "target")), "fragment link scrolls target into view");
                    Eq(viewFragmentRequests, view.Dg().Rs.Count, "fragment link performs no request");
                    var targetScroll = view.Sy;
                    Eq(1, view.Fg("#missing-target"), "WinForms missing fragment history navigation");
                    Eq(targetScroll, view.Sy, "missing fragment preserves current scroll");
                    view.Bk();
                    Paint(view);
                    Ok(view.U.Fragment == "#target" && ElementVisible(view, ById(view.At.D, "target")), "fragment Back restores target placement");

                    Eq(1, view.Fg("#closed-target"), "WinForms hidden fragment navigation");
                    Paint(view);
                    Ok(ById(view.At.D, "closed-details").Op && ElementVisible(view, ById(view.At.D, "closed-target")), "fragment navigation reveals closed details target");
                    Ok(Object.ReferenceEquals(viewFragmentField, FieldById(view.At, "fragment-field")) && viewFragmentField.V == "Edited in fragment view", "WinForms fragment reveal preserves field state");
                    Eq("v", BlockById(view.At, "closed-summary").Mk, "fragment reveal updates disclosure marker");
                    Eq(1, view.Fg("#cell-target"), "WinForms table fragment navigation");
                    Paint(view);
                    Ok(ElementVisible(view, ById(view.At.D, "cell-target")), "table cell fragment target is visible");
                    Eq(1, view.Fg("#encoded%20target"), "WinForms encoded fragment navigation");
                    Paint(view);
                    Ok(ElementVisible(view, ById(view.At.D, "encoded target")), "encoded fragment target is visible");
                    Eq(1, view.Fg("#legacy-anchor"), "WinForms named anchor navigation");
                    Paint(view);
                    Ok(ElementVisible(view, ById(view.At.D, "named-block")), "named anchor fragment target is visible");
                    Eq(1, view.Fg("#"), "WinForms top fragment navigation");
                    Eq(0, view.Sy, "empty fragment scrolls to document top");

                    view.Size = new Size(720, 420);
                    Eq(1, view.Fg("#target"), "wide fragment placement");
                    Paint(view);
                    var wideTargetScroll = view.Sy;
                    view.Size = new Size(300, 420);
                    Paint(view);
                    Eq(1, view.Fg("#target"), "same fragment recalculates after resize");
                    Paint(view);
                    Ok(ElementVisible(view, ById(view.At.D, "target")) && view.Sy != wideTargetScroll, "fragment placement follows resized layout");
                    Eq(viewFragmentRequests, view.Dg().Rs.Count, "fragment resize path performs no request");

                    view.Go(Path.Combine(pages, "second.html"));
                    view.Go(new Uri(new Uri(fragmentPath).AbsoluteUri + "#target"));
                    var loadedFragmentState = FragmentState(view);
                    Paint(view);
                    Ok(view.U.Fragment == "#target" && ElementVisible(view, ById(view.At.D, "target")), "initial cross-document URL fragment before paint: " + loadedFragmentState + "; after paint: " + FragmentState(view));

                    var fragmentStabilityPath = Path.Combine(pages, "fragment-stability.html");
                    using (var resizeFragment = new V { Size = new Size(300, 140) })
                    {
                        typeof(V).GetField("picWorkerArgs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(resizeFragment, "--pipe --delay=1000");
                        resizeFragment.Go(new Uri(new Uri(fragmentStabilityPath).AbsoluteUri + "#resize-target"));
                        Paint(resizeFragment);
                        Ok(ElementVisible(resizeFragment, resizeFragment.Id("resize-target")), "initial responsive fragment placement");
                        var narrowTargetTop = BlockRect(resizeFragment, BlockById(resizeFragment.At, "resize-target")).Top;
                        Ok(resizeFragment.Fa && resizeFragment.Fj >= 2 && resizeFragment.Fy == narrowTargetTop,
                            "initial fragment diagnostics retain responsive target");
                        var narrowFragmentJumps = resizeFragment.Fj;
                        resizeFragment.Size = new Size(720, 140);
                        Paint(resizeFragment);
                        var wideTargetTop = BlockRect(resizeFragment, BlockById(resizeFragment.At, "resize-target")).Top;
                        Ok(wideTargetTop < narrowTargetTop && ElementVisible(resizeFragment, resizeFragment.Id("resize-target")) &&
                            resizeFragment.Fj > narrowFragmentJumps && resizeFragment.Fy == wideTargetTop,
                            "initial fragment follows host size settlement");
                        var settledFragmentJumps = resizeFragment.Fj;
                        resizeFragment.Sc(1);
                        Ok(!resizeFragment.Fa && resizeFragment.Fj == settledFragmentJumps && resizeFragment.Fy == wideTargetTop,
                            "manual scroll releases retained initial fragment");
                        resizeFragment.Size = new Size(640, 140);
                        Paint(resizeFragment);
                        Eq(settledFragmentJumps, resizeFragment.Fj, "released fragment stays released after resize");
                    }
                    var disposedFragment = new V { Size = new Size(300, 140) };
                    disposedFragment.Go(new Uri(new Uri(fragmentStabilityPath).AbsoluteUri + "#resize-target"));
                    Paint(disposedFragment);
                    Ok(disposedFragment.Fa, "fragment anchor before disposal");
                    disposedFragment.Dispose();
                    Ok(!disposedFragment.Fa, "disposal releases retained fragment anchor");
                    using (var imageFragment = new V { Size = new Size(500, 140) })
                    {
                        typeof(V).GetField("picWorkerArgs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(imageFragment, "--pipe --delay=300");
                        imageFragment.Go(new Uri(new Uri(fragmentStabilityPath).AbsoluteUri + "#image-target"));
                        Paint(imageFragment);
                        var fallbackTargetTop = BlockRect(imageFragment, BlockById(imageFragment.At, "image-target")).Top;
                        Ok(imageFragment.Fa && imageFragment.Fj >= 2 && imageFragment.Fy == fallbackTargetTop,
                            "initial fragment diagnostics retain image target");
                        var fallbackFragmentJumps = imageFragment.Fj;
                        WaitFor(() => imageFragment.Ic == 1, "fragment intrinsic image decode");
                        Paint(imageFragment);
                        var intrinsicTargetTop = BlockRect(imageFragment, BlockById(imageFragment.At, "image-target")).Top;
                        Ok(intrinsicTargetTop > fallbackTargetTop + 100 && ElementVisible(imageFragment, imageFragment.Id("image-target")) &&
                            imageFragment.Fj > fallbackFragmentJumps && imageFragment.Fy == intrinsicTargetTop,
                            "initial fragment follows intrinsic image settlement");
                        Eq(1, imageFragment.Fg("#missing"), "missing retained fragment navigation");
                        Ok(!imageFragment.Fa && imageFragment.Fj == 1 && imageFragment.Fy == -1,
                            "missing fragment releases retained anchor with diagnostics");
                        imageFragment.Go(Path.Combine(pages, "second.html"));
                        Ok(!imageFragment.Fa && imageFragment.Fj == 0 && imageFragment.Fy == -1,
                            "document navigation resets fragment diagnostics");
                    }

                    view.Size = new Size(620, 900);
                    view.Go(gaugePath);
                    Paint(view);
                    var viewStyledProgress = BlockById(view.At, "styled-progress");
                    var styledProgressRect = BlockRect(view, viewStyledProgress);
                    Eq(new Size(260, 18), styledProgressRect.Size, "styled progress dimensions");
                    var autoProgressRect = BlockRect(view, BlockById(view.At, "auto-progress"));
                    Ok(autoProgressRect.Width >= 80 && autoProgressRect.Height >= 14, "default progress dimensions");
                    var gaugeTable = BlockById(view.At, "gauge-table");
                    var viewCellMeter = BlockById(view.At, "cell-meter");
                    var cellMeterRect = BlockRect(view, viewCellMeter);
                    Ok(!cellMeterRect.IsEmpty && ContainingCell(view, gaugeTable, cellMeterRect).Contains(cellMeterRect), "meter lays out inside table cell");
                    using (var gaugePaint = Render(view))
                    {
                        var styledY = styledProgressRect.Top + styledProgressRect.Height / 2;
                        Ok(ColorDistance(gaugePaint.GetPixel(styledProgressRect.Left + styledProgressRect.Width / 4, styledY), Color.FromArgb(23, 107, 77)) < 4, "progress fill inherits color");
                        Ok(ColorDistance(gaugePaint.GetPixel(styledProgressRect.Left + styledProgressRect.Width * 3 / 4, styledY), Color.FromArgb(226, 232, 240)) < 4, "progress unfilled track paint");
                        var indeterminateRect = BlockRect(view, BlockById(view.At, "indeterminate-progress"));
                        Ok(ColorDistance(CenterPixel(gaugePaint, indeterminateRect), Color.FromArgb(23, 107, 77)) < 4, "indeterminate progress segment paint");
                        var bestRect = BlockRect(view, BlockById(view.At, "best-meter"));
                        var suboptimalRect = BlockRect(view, BlockById(view.At, "suboptimal-meter"));
                        var worstRect = BlockRect(view, BlockById(view.At, "worst-meter"));
                        Ok(ColorDistance(gaugePaint.GetPixel(bestRect.Left + bestRect.Width / 20, bestRect.Top + bestRect.Height / 2), Color.FromArgb(21, 128, 61)) < 4, "optimum meter paint");
                        Ok(ColorDistance(gaugePaint.GetPixel(suboptimalRect.Left + suboptimalRect.Width / 4, suboptimalRect.Top + suboptimalRect.Height / 2), Color.FromArgb(183, 121, 31)) < 4, "suboptimal meter paint");
                        Ok(ColorDistance(gaugePaint.GetPixel(worstRect.Left + worstRect.Width / 2, worstRect.Top + worstRect.Height / 2), Color.FromArgb(185, 28, 28)) < 4, "least-good meter paint");
                        Ok(ColorDistance(CenterPixel(gaugePaint, cellMeterRect), Color.FromArgb(21, 128, 61)) < 4, "table meter paint");
                    }
                    var wideAutoProgressWidth = autoProgressRect.Width;
                    view.Size = new Size(150, 900);
                    Paint(view);
                    var narrowAutoProgressRect = BlockRect(view, BlockById(view.At, "auto-progress"));
                    Ok(narrowAutoProgressRect.Width > 0 && narrowAutoProgressRect.Width < wideAutoProgressWidth, "default progress reflows in narrow view");

                    view.Size = new Size(620, 900);
                    view.Go(valuePath);
                    Paint(view);
                    var viewOutput = BlockById(view.At, "total-output");
                    var wideValueRect = BlockRect(view, viewOutput);
                    Ok(!wideValueRect.IsEmpty && viewOutput.Dv != null && viewOutput.Dv.V == "42 points", "WinForms output value layout");
                    var valueTable = BlockById(view.At, "value-table");
                    var viewCellData = BlockById(view.At, "cell-data");
                    var cellValueRect = BlockRect(view, viewCellData);
                    Ok(!cellValueRect.IsEmpty && ContainingCell(view, valueTable, cellValueRect).Contains(cellValueRect), "data value lays out inside table cell");
                    var wideValueWidth = wideValueRect.Width;
                    view.Size = new Size(150, 900);
                    Paint(view);
                    var narrowValueRect = BlockRect(view, BlockById(view.At, "total-output"));
                    Ok(narrowValueRect.Width > 0 && narrowValueRect.Width < wideValueWidth && view.At.Dvs.Count == 7, "output value reflows without rebuilding models");

                    view.Size = new Size(640, 900);
                    view.Go(timePath);
                    Paint(view);
                    var viewReleaseTime = BlockById(view.At, "release-time");
                    var wideTimeRect = BlockRect(view, viewReleaseTime);
                    Ok(!wideTimeRect.IsEmpty && viewReleaseTime.Dv != null && viewReleaseTime.Dv.K == "t", "WinForms time metadata layout");
                    var semanticTable = BlockById(view.At, "semantic-table");
                    var viewCellRemoved = BlockById(view.At, "cell-del");
                    var cellRemovedRect = BlockRect(view, viewCellRemoved);
                    Ok(!cellRemovedRect.IsEmpty && ContainingCell(view, semanticTable, cellRemovedRect).Contains(cellRemovedRect), "removed metadata lays out inside table cell");
                    var wideTimeWidth = wideTimeRect.Width;
                    view.Size = new Size(160, 900);
                    Paint(view);
                    var narrowTimeRect = BlockRect(view, BlockById(view.At, "release-time"));
                    Ok(narrowTimeRect.Width > 0 && narrowTimeRect.Width < wideTimeWidth && view.At.Dvs.Count == 10, "time metadata reflows without rebuilding models");

                    view.Size = new Size(720, 760);
                    view.Go(Path.Combine(pages, "scroll.html"));
                    Paint(view);
                    Eq(16, view.Bc, "scroll boxes");
                    view.Sc(100000);
                    Eq(Math.Max(0, view.Dh - view.Height), view.Sy, "bounded scroll");
                }

                using (var server = new CompressedServer())
                {
                    var net = new E();
                    net.Cm = 2;
                    var gzip = new Uri(server.U, "?encoding=gzip#net-target");
                    var first = net.Go(gzip);
                    Eq(200, first.St, "HTTP status");
                    Eq("text/html", first.Ct, "HTTP content type");
                    Eq("gzip", first.Ce, "GZip response encoding");
                    Eq("utf-8", first.Ch, "HTTP charset");
                    Eq("Lea gzip", first.T, "GZip decoded page");
                    Eq("#net-target", first.U.Fragment, "network response preserves requested fragment");
                    Ok(first.Sz > 0 && !first.Ca, "network response metadata");
                    Eq(1, server.Hits, "first network request");

                    var cached = net.Go(gzip);
                    Ok(cached.Ca && !Object.ReferenceEquals(first, cached), "cached payload creates a fresh page");
                    Eq(1, server.Hits, "cache avoids network");
                    Eq(1, net.Ch, "cache hit count");

                    var deflate = net.Go(new Uri(server.U, "?encoding=deflate"));
                    Eq("deflate", deflate.Ce, "Deflate response encoding");
                    Eq("Lea deflate", deflate.T, "Deflate decoded page");
                    net.Go(new Uri(server.U, "?page=third"));
                    Eq(2, net.Cc, "bounded cache count");
                    var evicted = net.Go(gzip);
                    Ok(!evicted.Ca, "least-recently-used eviction");
                    Eq(4, server.Hits, "evicted page reload");

                    var reloaded = net.Rl();
                    Ok(!reloaded.Ca, "reload bypasses cache");
                    Eq(5, server.Hits, "reload network request");
                    net.Cl();
                    Eq(0, net.Cc, "clear cache");

                    var asyncPage = Complete(net.GoA(new Uri(server.U, "?encoding=deflate&async=1")), "async deflate page");
                    Eq("deflate", asyncPage.Ce, "async response encoding");
                    Eq("Lea deflate", asyncPage.T, "async decoded page");
                    var noStoreUrl = new Uri(server.U, "?no-store=1");
                    net.Go(noStoreUrl);
                    var noStore = net.Go(noStoreUrl);
                    Ok(!noStore.Ca, "no-store response bypasses cache");
                    Eq(8, server.Hits, "no-store network requests");
                    var noCacheUrl = new Uri(server.U, "?no-cache=1");
                    net.Go(noCacheUrl);
                    var noCache = net.Go(noCacheUrl);
                    Ok(!noCache.Ca, "no-cache response bypasses reuse");
                    Eq(10, server.Hits, "no-cache network requests");

                    net.Cs = 0;
                    Eq(0, net.Cs, "default freshness setting");
                    var defaultStaleUrl = new Uri(server.U, "?default-stale=1");
                    net.Go(defaultStaleUrl);
                    var defaultStale = net.Go(defaultStaleUrl);
                    Ok(!defaultStale.Ca, "zero default freshness reloads");
                    Eq(12, server.Hits, "default-stale response requests");
                    var freshUrl = new Uri(server.U, "?max-age=60");
                    net.Go(freshUrl);
                    var fresh = net.Go(freshUrl);
                    Ok(fresh.Ca, "server max-age overrides default freshness");
                    Eq(13, server.Hits, "fresh response cache hit");
                    var expiresUrl = new Uri(server.U, "?expires=future");
                    net.Go(expiresUrl);
                    var expiresFresh = net.Go(expiresUrl);
                    Ok(expiresFresh.Ca, "Expires header controls freshness");
                    Eq(14, server.Hits, "Expires response cache hit");
                    var staleUrl = new Uri(server.U, "?max-age=0");
                    net.Go(staleUrl);
                    var stale = net.Go(staleUrl);
                    Ok(!stale.Ca, "expired response reloads");
                    Eq(16, server.Hits, "expired response requests");

                    var etagUrl = new Uri(server.U, "?etag=1");
                    var etagFirst = net.Go(etagUrl);
                    Ok(!etagFirst.Ca, "ETag first response downloads");
                    Eq("\"lea-v1\"", etagFirst.Et, "ETag response metadata");
                    Eq(17, server.Hits, "ETag first request");
                    var etagValidated = net.Go(etagUrl);
                    Ok(etagValidated.Ca && etagValidated.H == etagFirst.H, "ETag 304 reuses cached body");
                    Eq(200, etagValidated.St, "ETag keeps representation status");
                    Eq("\"lea-v1\"", server.LastIfNoneMatch, "If-None-Match request header");
                    Eq(18, server.Hits, "ETag validation request");
                    Ok(net.Go(etagUrl).Ca, "ETag validation refreshes lifetime");
                    Eq(18, server.Hits, "fresh ETag response avoids network");
                    var etagReload = net.Rl();
                    Ok(!etagReload.Ca, "reload bypasses ETag validation");
                    Eq("", server.LastIfNoneMatch, "reload omits If-None-Match");
                    Eq(19, server.Hits, "unconditional ETag reload request");

                    var modifiedUrl = new Uri(server.U, "?last-modified=1");
                    var modifiedFirst = net.Go(modifiedUrl);
                    Ok(!modifiedFirst.Ca, "Last-Modified first response downloads");
                    Ok(modifiedFirst.Lm.Length > 0, "Last-Modified response metadata");
                    Eq(20, server.Hits, "Last-Modified first request");
                    var modifiedValidated = Complete(net.GoA(modifiedUrl), "async modified validator");
                    Ok(modifiedValidated.Ca && modifiedValidated.H == modifiedFirst.H, "async Last-Modified 304 reuses cached body");
                    Ok(server.LastIfModifiedSince.Length > 0, "If-Modified-Since request header");
                    Eq(21, server.Hits, "Last-Modified validation request");
                    Ok(net.Go(modifiedUrl).Ca, "Last-Modified validation refreshes lifetime");
                    Eq(21, server.Hits, "fresh Last-Modified response avoids network");
                    Eq(7, net.Ch, "cache body reuse count includes 304 responses");

                    net.Cs = 60;
                    net.Cl();
                    var redirected = net.Go(new Uri(server.U, "?redirect=1"));
                    Eq(200, redirected.St, "redirect final status");
                    Ok(redirected.U.Query.Contains("redirected=1"), "redirect final URL");
                    Eq(23, server.Hits, "redirect request chain");

                    using (var postView = new V())
                    {
                        postView.Size = new Size(640, 520);
                        postView.CreateControl();
                        var formUrl = new Uri(server.U, "?form=1");
                        postView.Go(formUrl);
                        Paint(postView);
                        Eq("select,text,checkbox,checkbox,submit", Kinds(postView.At), "POST form fields");
                        Eq("mint", postView.At.Fs[0].V, "POST select default");
                        Eq(3, postView.At.Fs[0].Os.Count, "POST select options");
                        postView.At.Fs[0].V = "vanilla";
                        postView.At.Fs[0].Si = 0;
                        postView.At.Fs[1].V = "tiny engine";
                        var posted = postView.Sm(postView.At.Fs[4]);
                        Eq("Lea POST", posted.T, "POST result");
                        Eq("gzip", posted.Ce, "compressed POST response");
                        Eq("POST", server.LastMethod, "POST method");
                        Ok(server.LastType.StartsWith("application/x-www-form-urlencoded"), "POST content type: " + server.LastType);
                        Eq("flavor=vanilla&q=tiny+engine&news=yes&send=go", server.LastBody, "POST body");

                        postView.Go(formUrl);
                        postView.At.Fs[1].V = "async engine";
                        var postTask = postView.SmA(postView.At.Fs[4]);
                        while (!postTask.IsCompleted)
                        {
                            Application.DoEvents();
                            Thread.Sleep(2);
                        }
                        var asyncPost = postTask.GetAwaiter().GetResult();
                        Eq("Lea POST", asyncPost.T, "async POST result");
                        Ok(server.LastBody.Contains("flavor=mint") && server.LastBody.Contains("q=async+engine"), "async POST body: " + server.LastBody);

                        var upload = Path.Combine(Path.GetTempPath(), "lea-multipart-" + Guid.NewGuid().ToString("N") + ".txt");
                        File.WriteAllText(upload, "Lea multipart payload", new UTF8Encoding(false));
                        try
                        {
                            var multipartUrl = new Uri(server.U, "?multipart-form=1");
                            postView.Go(multipartUrl);
                            Paint(postView);
                            Eq("file,text,checkbox,submit", Kinds(postView.At), "multipart form fields");
                            Ok(postView.At.Fs[0].Rq && postView.At.Fs[0].En == "m", "multipart file setup");
                            Ok(postView.Sf(postView.At.Fs[0], upload), "multipart file selection");
                            postView.At.Fs[1].V = "sync upload";
                            var multipartPost = postView.Sm(postView.At.Fs[3]);
                            Eq("Lea POST", multipartPost.T, "multipart POST result");
                            Eq("POST", server.LastMethod, "multipart POST method");
                            Ok(server.LastType.StartsWith("multipart/form-data; boundary=----Lea"), "multipart content type: " + server.LastType);
                            Ok(server.LastBody.Contains("name=\"upload\"; filename=\"" + Path.GetFileName(upload) + "\"") && server.LastBody.Contains("Content-Type: text/plain") && server.LastBody.Contains("Lea multipart payload"), "multipart file part");
                            Ok(server.LastBody.Contains("name=\"note\"") && server.LastBody.Contains("sync upload") && server.LastBody.Contains("name=\"keep\"") && server.LastBody.Contains("name=\"send\""), "multipart successful controls");

                            postView.Go(multipartUrl);
                            Ok(postView.Sf(postView.At.Fs[0], upload), "async multipart file selection");
                            postView.At.Fs[1].V = "async upload";
                            var multipartTask = postView.SmA(postView.At.Fs[3]);
                            while (!multipartTask.IsCompleted)
                            {
                                Application.DoEvents();
                                Thread.Sleep(2);
                            }
                            Eq("Lea POST", multipartTask.GetAwaiter().GetResult().T, "async multipart POST result");
                            Ok(server.LastBody.Contains("async upload") && server.LastBody.Contains("Lea multipart payload"), "async multipart body");
                        }
                        finally { File.Delete(upload); }
                    }
                    Eq(29, server.Hits, "form and POST request count");

                    var unavailable = Fails(delegate { net.Go(new Uri(server.U, "?status=503")); }, "HTTP 503");
                    Eq("http", unavailable.K, "HTTP error kind");
                    Eq(503, unavailable.St, "HTTP error status");
                    Ok(unavailable.Rt, "HTTP 503 retryable");
                    var missingHttp = Fails(delegate { Complete(net.GoA(new Uri(server.U, "?status=404")), "async HTTP 404 completion"); }, "async HTTP 404");
                    Eq("http", missingHttp.K, "async HTTP error kind");
                    Eq(404, missingHttp.St, "async HTTP error status");
                    Ok(!missingHttp.Rt, "HTTP 404 not retryable");
                    var decode = Fails(delegate { net.Go(new Uri(server.U, "?bad-gzip=1")); }, "bad GZip");
                    Eq("decode", decode.K, "decode error kind");

                    var cssNet = new E();
                    var cssStart = server.Hits;
                    var linked = cssNet.Go(new Uri(server.U, "?external-page=sync"));
                    Eq("#0f6848", linked.S.Bg, "network external CSS body background");
                    Eq("#a13b18", linked.Bs[0].S.F, "network external CSS block color");
                    Eq(cssStart + 2, server.Hits, "page and stylesheet requests");
                    var linkedAgain = cssNet.Go(new Uri(server.U, "?external-page=sync"));
                    Eq("#0f6848", linkedAgain.S.Bg, "cached external CSS remains applied");
                    Eq(cssStart + 3, server.Hits, "shared cache avoids stylesheet request");
                    var asyncLinked = Complete(new E().GoA(new Uri(server.U, "?external-page=async")), "async linked stylesheet");
                    Eq("#0f6848", asyncLinked.S.Bg, "async network external CSS");
                    Eq("#a13b18", asyncLinked.Bs[0].S.F, "async linked rule");
                    var limitNet = new E();
                    limitNet.Cm = 16;
                    var limitStart = server.Hits;
                    limitNet.Go(new Uri(server.U, "?external-page=limit"));
                    Eq(limitStart + 9, server.Hits, "external stylesheet request limit");
                    limitNet.Go(new Uri(server.U, "?external-page=limit"));
                    Eq(limitStart + 10, server.Hits, "eight stylesheets reuse bounded cache");
                    var largeNet = new E();
                    var largeStart = server.Hits;
                    var largePage = largeNet.Go(new Uri(server.U, "?external-page=large"));
                    Eq("", largePage.S.Bg, "oversized stylesheet ignored");
                    Eq(0, largeNet.Cc, "oversized stylesheet removed from cache");
                    Eq(largeStart + 2, server.Hits, "oversized stylesheet first request");
                    largeNet.Go(new Uri(server.U, "?external-page=large"));
                    Eq(largeStart + 4, server.Hits, "oversized stylesheet not cached");

                    var jar = new E();
                    Eq(64, jar.Km, "bounded cookie maximum");
                    Eq(0, jar.Kc, "empty cookie jar");
                    jar.Go(new Uri(server.U, "?cookie=set-normal"));
                    Eq(1, jar.Kc, "stored normal cookie");
                    Eq("Cookie normal", jar.Go(new Uri(server.U, "?cookie=read")).T, "normal cookie round trip");
                    Ok(server.LastCookie.Contains("sid=normal"), "normal Cookie request header");
                    jar.Go(new Uri(server.U, "?max-age=60&privacy-cache=1"));
                    Ok(jar.Cc > 0, "normal cache before private mode");

                    jar.Pv = true;
                    Ok(jar.Pv && jar.Kc == 0 && jar.Cc == 0, "private mode starts isolated and clears cache");
                    Eq("Cookie empty", jar.Go(new Uri(server.U, "?cookie=read")).T, "private jar hides normal cookie");
                    jar.Go(new Uri(server.U, "?cookie=set-private"));
                    Eq("Cookie private", Complete(jar.GoA(new Uri(server.U, "?cookie=read")), "async private cookie round trip").T, "async private cookie round trip");
                    Eq(1, jar.Kc, "private cookie count");

                    jar.Pv = false;
                    Ok(!jar.Pv && jar.Kc == 1 && jar.Cc == 0, "normal jar restored after private mode");
                    Eq("Cookie normal", jar.Go(new Uri(server.U, "?cookie=read")).T, "private cookie discarded");
                    jar.Pv = true;
                    Eq(0, jar.Kc, "new private session discards prior private cookies");
                    jar.Pv = false;
                    for (var cookieIndex = 0; cookieIndex < 70; cookieIndex++) jar.Go(new Uri(server.U, "?cookie=" + cookieIndex));
                    Ok(jar.Kc > 0 && jar.Kc <= jar.Km, "cookie jar remains bounded: " + jar.Kc);
                    jar.Kcl();
                    Eq(0, jar.Kc, "clear cookie jar");
                    Eq(0, jar.Cc, "clear cookies also clears page cache");
                    Eq("Cookie empty", jar.Go(new Uri(server.U, "?cookie=read")).T, "cleared cookie omitted");

                    var networkDownload = Path.Combine(Path.GetTempPath(), "lea-network-download-" + Guid.NewGuid().ToString("N") + ".html");
                    var networkDownloadAsync = Path.Combine(Path.GetTempPath(), "lea-network-download-" + Guid.NewGuid().ToString("N") + ".html");
                    var largeDownload = Path.Combine(Path.GetTempPath(), "lea-large-download-" + Guid.NewGuid().ToString("N") + ".bin");
                    try
                    {
                        var networkSaved = net.Dl(new Uri(server.U, "?download=sync"), networkDownload);
                        Eq(200, networkSaved.St, "HTTP download status");
                        Eq("text/html", networkSaved.Ct, "HTTP download content type");
                        Ok(networkSaved.Sz > 0 && File.ReadAllText(networkDownload).Contains("Compressed Lea response"), "HTTP gzip download is streamed and decoded");
                        var networkSavedAsync = net.DlA(new Uri(server.U, "?download=async&encoding=deflate"), networkDownloadAsync).GetAwaiter().GetResult();
                        Ok(networkSavedAsync.Sz > 0 && File.ReadAllText(networkDownloadAsync).Contains("Compressed Lea response"), "async deflate download is streamed and decoded");
                        var tooLarge = Fails(delegate { net.Dl(new Uri(server.U, "?download-large=1"), largeDownload); }, "oversized HTTP download");
                        Eq("large", tooLarge.K, "oversized download error kind");
                        Ok(!File.Exists(largeDownload), "oversized download leaves no destination");
                    }
                    finally
                    {
                        File.Delete(networkDownload);
                        File.Delete(networkDownloadAsync);
                        File.Delete(largeDownload);
                    }

                    var missingFile = new Uri(Path.Combine(pages, "missing-lea-page.html"));
                    var fileError = Fails(delegate { net.Go(missingFile); }, "missing file");
                    Eq("file", fileError.K, "file error kind");
                    Ok(!fileError.Rt, "file error not retryable");
                    var urlError = Fails(delegate { new E().Go(""); }, "bad URL");
                    Eq("url", urlError.K, "URL error kind");

                    var remoteGoEngine = new E();
                    var remoteGoPage = remoteGoEngine.Go(new Uri(server.U, "?go-security=1"));
                    Eq(0, remoteGoEngine.Js(remoteGoPage, "go('" + goTarget.AbsoluteUri + "')"), "remote page cannot request a local file");
                    Eq(1, remoteGoEngine.Js(remoteGoPage, "go('/approved')"), "remote relative HTTP go accepted");
                    Eq(new Uri(server.U, "approved"), remoteGoEngine.Gt(remoteGoPage), "remote relative HTTP go request");

                    var diskRoot = Path.Combine(Path.GetTempPath(), "lea-disk-cache-" + Guid.NewGuid().ToString("N"));
                    try
                    {
                        var disk = new E();
                        Eq("", disk.Cd, "persistent cache stays opt-in");
                        Eq(32, disk.Cx, "persistent cache default entry ceiling");
                        Eq(16L * 1024 * 1024, disk.Cb, "persistent cache default byte ceiling");
                        disk.Cd = diskRoot;
                        disk.Cm = 0;
                        var diskFreshUrl = new Uri(server.U, "?max-age=60&disk=fresh");
                        var diskStart = server.Hits;
                        var diskFirst = disk.Go(diskFreshUrl);
                        Ok(!diskFirst.Ca && disk.Dc == 1 && disk.Db > 0 && disk.Cc == 0, "first persistent response stored within disk bounds");
                        Eq(diskStart + 1, server.Hits, "first persistent response uses network");

                        var restarted = new E();
                        restarted.Cm = 0;
                        restarted.Cd = diskRoot;
                        var diskHit = restarted.Go(diskFreshUrl);
                        Ok(diskHit.Ca && diskHit.H == diskFirst.H && restarted.Cc == 0 && restarted.Ch == 1, "fresh persistent response survives engine restart");
                        Eq(diskStart + 1, server.Hits, "fresh persistent response avoids network after restart");
                        var diskSnapshot = restarted.Dg(diskHit);
                        Ok(diskSnapshot.Dc == 1 && diskSnapshot.Db > 0 && diskSnapshot.Dh == 1 && Has(diskSnapshot.Rs, " disk "), "persistent hit diagnostics");

                        var diskEtagUrl = new Uri(server.U, "?etag=1&disk=validator");
                        var validatorSeed = new E();
                        validatorSeed.Cd = diskRoot;
                        var etagSeed = validatorSeed.Go(diskEtagUrl);
                        var validatorHits = server.Hits;
                        Ok(!etagSeed.Ca && etagSeed.Et == "\"lea-v1\"", "persistent validator seed");
                        var validatorRestart = new E();
                        validatorRestart.Cd = diskRoot;
                        var diskValidated = Complete(validatorRestart.GoA(diskEtagUrl), "async disk validator");
                        Ok(diskValidated.Ca && diskValidated.H == etagSeed.H, "persistent ETag 304 reuses decoded body after restart");
                        Eq(validatorHits + 1, server.Hits, "persistent validator performs one conditional request");
                        Eq("\"lea-v1\"", server.LastIfNoneMatch, "persistent If-None-Match request header");
                        var validatorSnapshot = validatorRestart.Dg(diskValidated);
                        Ok(validatorSnapshot.Dh == 1 && validatorSnapshot.Dw == 1 && Has(validatorSnapshot.Rs, " disk304 "), "persistent validator diagnostics");

                        var reloadRestart = new E();
                        reloadRestart.Cd = diskRoot;
                        reloadRestart.Go(diskFreshUrl);
                        var reloadHits = server.Hits;
                        var diskReload = reloadRestart.Rl();
                        Ok(!diskReload.Ca, "reload bypasses persistent cache");
                        Eq(reloadHits + 1, server.Hits, "persistent reload uses network");
                        Eq("", server.LastIfNoneMatch, "persistent reload remains unconditional");

                        var noStoreDiskUrl = new Uri(server.U, "?no-store=1&disk=nostore");
                        var noStoreDisk = new E();
                        noStoreDisk.Cd = diskRoot;
                        var noStoreCount = noStoreDisk.Dc;
                        var noStoreHits = server.Hits;
                        noStoreDisk.Go(noStoreDiskUrl);
                        var noStoreRestart = new E();
                        noStoreRestart.Cd = diskRoot;
                        Ok(!noStoreRestart.Go(noStoreDiskUrl).Ca, "no-store never enters persistent cache");
                        Eq(noStoreHits + 2, server.Hits, "no-store remains network-only across restart");
                        Eq(noStoreCount, noStoreRestart.Dc, "no-store leaves persistent entry count unchanged");

                        var privateDiskUrl = new Uri(server.U, "?max-age=60&disk=private");
                        var privateSeed = new E();
                        privateSeed.Cd = diskRoot;
                        privateSeed.Go(privateDiskUrl);
                        var privateDisk = new E();
                        privateDisk.Cd = diskRoot;
                        privateDisk.Pv = true;
                        var privateCount = privateDisk.Dc;
                        var privateHits = server.Hits;
                        Ok(!privateDisk.Go(privateDiskUrl).Ca, "private mode does not read persistent cache");
                        Eq(privateHits + 1, server.Hits, "private mode uses isolated network path");
                        Eq(privateCount, privateDisk.Dc, "private mode does not write persistent cache");
                        privateDisk.Pv = false;
                        var normalAfterPrivateHits = server.Hits;
                        Ok(privateDisk.Go(privateDiskUrl).Ca, "normal persistent state is reusable after private mode");
                        Eq(normalAfterPrivateHits, server.Hits, "leaving private mode restores normal persistent state");
                        var privateOnlyUrl = new Uri(server.U, "?max-age=60&disk=private-only");
                        privateDisk.Pv = true;
                        privateCount = privateDisk.Dc;
                        privateDisk.Go(privateOnlyUrl);
                        Eq(privateCount, privateDisk.Dc, "private-only response is not persisted");
                        privateDisk.Pv = false;

                        disk.Cdl();
                        Eq(0, disk.Dc, "clear persistent cache");
                        var corruptUrl = new Uri(server.U, "?max-age=60&disk=corrupt");
                        disk.Go(corruptUrl);
                        var cacheFiles = Directory.GetFiles(diskRoot, "*.lea");
                        Eq(1, cacheFiles.Length, "single corruption fixture entry");
                        File.WriteAllBytes(cacheFiles[0], Encoding.ASCII.GetBytes("partial"));
                        var corruptHits = server.Hits;
                        var recovered = new E();
                        recovered.Cd = diskRoot;
                        var recoveredPage = recovered.Go(corruptUrl);
                        Ok(!recoveredPage.Ca && recovered.Dg(recoveredPage).Dr == 1 && Has(recovered.Dg(recoveredPage).Rs, " recover "), "corrupt persistent entry falls back and reports recovery");
                        Eq(corruptHits + 1, server.Hits, "corrupt persistent entry falls back to network");
                        Eq(1, recovered.Dc, "corrupt persistent entry is replaced atomically");
                        Eq(0, Directory.GetFiles(diskRoot, "*.tmp").Length, "persistent cache leaves no partial files");

                        recovered.Cdl();
                        recovered.Cm = 0;
                        recovered.Cx = 2;
                        recovered.Cb = 16L * 1024 * 1024;
                        var lruOne = new Uri(server.U, "?max-age=60&disk=lru-one");
                        var lruTwo = new Uri(server.U, "?max-age=60&disk=lru-two");
                        var lruThree = new Uri(server.U, "?max-age=60&disk=lru-three");
                        recovered.Go(lruOne);
                        Thread.Sleep(20);
                        recovered.Go(lruTwo);
                        Thread.Sleep(20);
                        recovered.Go(lruOne);
                        Thread.Sleep(20);
                        recovered.Go(lruThree);
                        Eq(2, recovered.Dc, "persistent entry ceiling trims LRU data");
                        Ok(recovered.Db > 0 && recovered.Db <= recovered.Cb, "persistent byte accounting stays bounded");
                        var lruRestart = new E();
                        lruRestart.Cd = diskRoot;
                        lruRestart.Cm = 0;
                        lruRestart.Cx = 2;
                        var lruHits = server.Hits;
                        Ok(lruRestart.Go(lruOne).Ca, "recent persistent entry survives LRU trim");
                        Eq(lruHits, server.Hits, "recent persistent entry avoids network");
                        Ok(!lruRestart.Go(lruTwo).Ca, "old persistent entry is evicted");
                        Eq(lruHits + 1, server.Hits, "evicted persistent entry returns to network");

                        lruRestart.Cb = 1;
                        Eq(0, lruRestart.Dc, "persistent byte ceiling can disable stored payloads");
                        lruRestart.Go(new Uri(server.U, "?max-age=60&disk=byte-limit"));
                        Eq(0, lruRestart.Dc, "oversized-for-budget response is not retained");
                        lruRestart.Cx = Int32.MaxValue;
                        lruRestart.Cb = Int64.MaxValue;
                        Eq(128, lruRestart.Cx, "persistent entry ceiling clamps");
                        Eq(128L * 1024 * 1024, lruRestart.Cb, "persistent byte ceiling clamps");

                        var unrelated = Path.Combine(diskRoot, "keep.txt");
                        File.WriteAllText(unrelated, "host-owned");
                        lruRestart.Cdl();
                        Ok(File.Exists(unrelated) && lruRestart.Dc == 0 && lruRestart.Db == 0, "persistent clear preserves unrelated host files");
                        using (var diskView = new V())
                        {
                            diskView.Cd = diskRoot;
                            diskView.Cm = 3;
                            diskView.Cx = 4;
                            diskView.Cb = 4096;
                            Ok(diskView.Cd == Path.GetFullPath(diskRoot) && diskView.Cm == 3 && diskView.Cx == 4 && diskView.Cb == 4096 && diskView.Cc == 0 && diskView.Dc == 0, "WinForms persistent cache aliases");
                            diskView.Cl();
                            diskView.Cdl();
                        }
                    }
                    finally
                    {
                        if (Directory.Exists(diskRoot)) Directory.Delete(diskRoot, true);
                    }
                }

                Console.WriteLine("Lea WinForms renderer smoke passed; starting network timeout checks.");
                var timeoutEngine = new E();
                timeoutEngine.Tm = 100;
                using (var slow = new CompressedServer())
                {
                    var timeout = Fails(delegate { timeoutEngine.Go(new Uri(slow.U, "?delay=1000")); }, "synchronous request timeout");
                    Eq("timeout", timeout.K, "synchronous timeout kind");
                    Ok(timeout.Rt, "synchronous timeout retryable");
                    Eq(1, slow.Hits, "synchronous timeout request started");
                }
                using (var normal = new CompressedServer())
                {
                    timeoutEngine.Tm = 1000;
                    Eq("Lea gzip", timeoutEngine.Go(new Uri(normal.U, "?after-timeout=1")).T, "normal request after timeout");
                }

                var asyncTimeoutEngine = new E();
                asyncTimeoutEngine.Tm = 100;
                using (var slow = new CompressedServer())
                {
                    var timeoutWork = asyncTimeoutEngine.GoA(new Uri(slow.U, "?delay=1000"));
                    WaitFor(delegate { return timeoutWork.IsCompleted; }, "asynchronous timeout completion");
                    var timeout = Fails(delegate { timeoutWork.GetAwaiter().GetResult(); }, "asynchronous request timeout");
                    Eq("timeout", timeout.K, "asynchronous timeout kind");
                    Ok(timeout.Rt, "asynchronous timeout retryable");
                    Eq(1, slow.Hits, "asynchronous timeout request started");
                }

                var cancelEngine = new E();
                cancelEngine.Tm = 2000;
                using (var slow = new CompressedServer())
                {
                    var work = cancelEngine.GoA(new Uri(slow.U, "?delay=1000&cancel=1"));
                    WaitFor(delegate { return slow.Hits == 1; }, "cancel request started");
                    cancelEngine.St();
                    WaitFor(delegate { return work.IsCompleted; }, "cancel request completion");
                    var cancel = Fails(delegate { work.GetAwaiter().GetResult(); }, "host request cancellation");
                    Eq("cancel", cancel.K, "cancellation remains distinct from timeout");
                    Ok(!cancel.Rt, "cancellation not retryable");
                }

                using (var alertView = new V())
                {
                    var reentrantAlerts = 0;
                    var nestedAlert = false;
                    alertView.Al += delegate
                    {
                        reentrantAlerts++;
                        if (nestedAlert) return;
                        nestedAlert = true;
                        alertView.Js("a('Nested alert');");
                    };
                    alertView.Go(scriptPath);
                    Eq(3, reentrantAlerts, "reentrant alert delivery stays once-only");
                    Eq("Nested alert", alertView.Am, "reentrant latest alert");
                }

                if (args.Length > 1) SaveRender(scriptPath, args[1]);
                if (args.Length > 2) SaveFindRender(findPath, args[2]);
                if (args.Length > 3) SaveEventRender(eventPath, args[3]);
                if (args.Length > 4) SaveRender(tokenPath, args[4]);
                if (args.Length > 5) SaveStateRender(statePath, args[5]);
                Console.WriteLine("Lea CSS radius smoke passed: compact ellipse aliases, atomic cascade, normalized paint, overflow, fields, links, and resize.");
                Console.WriteLine("Lea logical radius smoke passed: four mapped corners, nine compact/readable aliases, bounded DOM commands, inheritance/reset, atomic rejection, and asymmetric paint.");
                Console.WriteLine("Lea CSS shadow smoke passed: compact aliases, atomic lists, bounded blur, alpha colors, outer/inset paint, fields, hits, and resize.");
                Console.WriteLine("Lea CSS gradient smoke passed: compact image/stop aliases, atomic cascade, normalized stops, linear/radial/page/field paint, hits, and resize.");
                Console.WriteLine("Lea CSS bitmap smoke passed: compact aliases, stylesheet origins, atomic cascade, bounded image loading, repeat/size/position/page/field paint, clipping, opacity, hits, and resize.");
                Console.WriteLine("Lea calculated height smoke passed: compact ht, definite viewport/parent/positioned references, auto fallback, min/max, flex/grid/table/media geometry, accessibility, reset, and resize.");
                Console.WriteLine("Lea viewport unit smoke passed: vw/vh/vmin/vmax, calc, cross-axis sizes, lazy aliases, layout families, accessibility, commands, and resize.");
                Console.WriteLine("Lea font-relative unit smoke passed: em/rem, root font state, calc, min/max, layout families, commands, font mutation, and responsive rebuild.");
                Console.WriteLine("Lea size function smoke passed: bounded min/max/clamp, lazy aliases, mixed terms, constraints, layout families, commands, and responsive rebuild.");
                Console.WriteLine("Lea logical border smoke passed: 24 compact/readable properties, per-edge cascade, atomic parsing, shared geometry, hidden widths, mixed rounded paint, DOM reset, and inheritance.");

                    Console.WriteLine("Lea smoke passed: bounded sticky positioning with compact k, shared paint/hit/editor/accessibility geometry, normal-flow retention, and ancestor clamps, bounded has relational selectors with strict lists, maximum specificity, descendant/child/adjacent/following and complex chains, queries, dynamic focus rebuilds, retained fields, WinForms geometry, and traversal/part limits, bounded nth-child of-S filters with strict compound lists, maximum specificity, forward/reverse counting, queries, dynamic form-state rebuilds, retained fields, WinForms geometry, and nesting/test limits, bounded precompiled An+B and reverse structural selectors with stylesheet/query parity, specificity, selector-list composition, type filtering, WinForms geometry, and invalid limits, bounded is, where, and list-not selector functions with maximum/zero specificity, forgiving/strict parsing, queries, dynamic state, WinForms geometry, and retained fields, bounded CSS important cascade with compiled priority, G.Ic diagnostics, stylesheet/inline tiers, custom properties, hover/media rebuilds, and retained fields, bounded inherited CSS custom properties with compiled declarations, S.Vc/Cv and G.Vc aliases, nested and cycle fallbacks, hover/media rebuilds, and retained fields, bounded responsive CSS media queries with viewport aliases, live resize, diagnostics, and retained fields, optional bounded persistent cache with restart reuse, validators, private isolation, atomic recovery, LRU entry/byte ceilings and host aliases, bounded content-box and border-box geometry, body and ID-scoped style commands, ordered row and column flex layout with around/evenly distribution, item alignment, wrapping, growth, shrink, reverse flow, and resize, semantic accessibility with WinForms MSAA and native UI Automation fragments, patterns and notifications, protected values, live states, hierarchy and rendered bounds, bounded autofocus and access keys, tabindex focus, visible rings, keyboard link and shared image-map activation, compact output/data values, bounded host aliases, safe rebuilds, inline and table ownership, resize reflow, compact progress and meter values, quality regions, inherited custom painting, bounded retained fragment history, encoded and named targets, hidden reveal, bounded details disclosures with retained state, keyboard and table interaction, bounded ordered and unordered list markers, nested list scope and resize reflow, bounded host-controlled go requests, class mutations, document base URLs and Id/Q/Qa queries, bounded HTML tokenization, raw text, RCDATA, comments, entities and malformed delimiters, bounded developer diagnostics and inspector data, safe click commands and bounded variables/text updates, streamed downloads and highlighted find-in-page, bounded script commands and alert bridging, inset stretching and translated stacking contexts, nested forms, media, tables, and clipped editors, explicit z-index and opacity/isolation contexts, two-pass positioned auto height, nested visual flow, bounded multipart file uploads, range and color widgets, temporal fields, absolute and fixed positioning, clipped viewport hits, form validation and typed inputs, responsive and embedded images, live hover and focus restyling, visibility and opacity, general sibling and negation selectors, min/max sizing, long-word breaking, compiled attribute and pseudo selectors, fonts, overflow clipping, malformed HTML recovery, semantic blocks, tables, inline flow, external CSS, private cookies, cache revalidation, typed errors, POST, compression, layout, and scrolling.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Lea smoke failed: " + error);
                return 1;
            }
        }

        private static void Paint(V view)
        {
            using (var image = Render(view)) { }
        }

        private static void BitmapBackgroundPaintSmoke(string path)
        {
            using (var syncView = new V())
            {
                syncView.Size = new Size(720, 500);
                syncView.St();
                syncView.Go(path);
                using (var firstPaint = Render(syncView)) { }
                WaitFor(() => syncView.Ic == 3, "asynchronous bitmap decode after synchronous navigation");
                using (var paint = Render(syncView))
                {
                    var repeat = BlockOuterRect(syncView, BlockById(syncView.At, "repeat"));
                    Ok(ColorDistance(CenterPixel(paint, repeat), Color.FromArgb(23, 107, 77)) < 4,
                        "synchronous bitmap navigation resumes asynchronous pictures after stop");
                }
            }
            using (var view = new V())
            {
                view.Size = new Size(720, 1400);
                var load = view.GoA(path);
                while (!load.IsCompleted)
                {
                    Application.DoEvents();
                    Thread.Sleep(2);
                }
                load.GetAwaiter().GetResult();
                Eq(3, PicCount(view), "deduplicated bitmap background cache");
                Ok(PicCached(view, view.At.S.Bi.U) &&
                    PicCached(view, BlockById(view.At, "repeat").S.Bi.U) &&
                    PicCached(view, BlockById(view.At, "external-image").S.Bi.U),
                    "deduplicated bitmap background cache keys");
                using (var paint = Render(view))
                {
                    var repeat = BlockOuterRect(view, BlockById(view.At, "repeat"));
                    Ok(ColorDistance(CenterPixel(paint, repeat), Color.FromArgb(23, 107, 77)) < 4, "repeated bitmap paint");
                    Eq(repeat, HitRect(view, "second.html"), "bitmap does not expand link hit geometry");
                    var noRepeat = BlockOuterRect(view, BlockById(view.At, "no-repeat"));
                    Ok(ColorDistance(paint.GetPixel(noRepeat.Right - 6, noRepeat.Bottom - 6), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(paint.GetPixel(noRepeat.Left + 5, noRepeat.Bottom - 6), Color.White) < 8,
                        "positioned no-repeat bitmap paint");
                    var repeatX = BlockOuterRect(view, BlockById(view.At, "repeat-x"));
                    Ok(ColorDistance(paint.GetPixel(repeatX.Right - 5, repeatX.Top + repeatX.Height / 2), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(paint.GetPixel(repeatX.Right - 5, repeatX.Bottom - 5), Color.White) < 8,
                        "horizontal repeat bitmap paint");
                    var repeatY = BlockOuterRect(view, BlockById(view.At, "repeat-y"));
                    Ok(ColorDistance(paint.GetPixel(repeatY.Left + repeatY.Width / 2, repeatY.Bottom - 5), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(paint.GetPixel(repeatY.Left + 5, repeatY.Bottom - 5), Color.White) < 8,
                        "vertical repeat bitmap paint");
                    var cover = BlockOuterRect(view, BlockById(view.At, "cover"));
                    var contain = BlockOuterRect(view, BlockById(view.At, "contain"));
                    Ok(ColorDistance(CenterPixel(paint, cover), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(CenterPixel(paint, contain), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(paint.GetPixel(contain.Left + 5, contain.Bottom - 5), Color.White) < 8,
                        "cover and contain bitmap paint");
                    var rounded = BlockOuterRect(view, BlockById(view.At, "rounded"));
                    Ok(ColorDistance(paint.GetPixel(rounded.Left, rounded.Top), Color.White) < 8 &&
                        ColorDistance(CenterPixel(paint, rounded), Color.FromArgb(23, 107, 77)) < 4,
                        "rounded bitmap clipping");
                    var field = BlockRect(view, FieldBlock(view.At, FieldById(view.At, "field")));
                    Ok(ColorDistance(paint.GetPixel(field.Right - 8, field.Top + field.Height / 2), Color.FromArgb(23, 107, 77)) < 8,
                        "field bitmap paint");
                    var alpha = BlockOuterRect(view, BlockById(view.At, "alpha"));
                    Ok(ColorDistance(CenterPixel(paint, alpha), Color.FromArgb(139, 181, 166)) < 12, "effective bitmap opacity paint");
                    var shorthand = BlockOuterRect(view, BlockById(view.At, "shorthand"));
                    Ok(ColorDistance(paint.GetPixel(shorthand.Right - 6, shorthand.Bottom - 6), Color.FromArgb(23, 107, 77)) < 4 &&
                        ColorDistance(paint.GetPixel(shorthand.Left + 5, shorthand.Bottom - 6), Color.White) < 8,
                        "bitmap shorthand paint");
                    var external = BlockOuterRect(view, BlockById(view.At, "external-image"));
                    Ok(ColorDistance(CenterPixel(paint, external), Color.White) > 30, "linked stylesheet bitmap paint");
                    Ok(ColorDistance(paint.GetPixel(paint.Width - 8, paint.Height - 8), Color.FromArgb(126, 34, 206)) < 4,
                        "page bitmap paint");
                }
                view.Size = new Size(350, 1800);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var dynamic = BlockOuterRect(view, BlockById(view.At, "dynamic"));
                    Ok(!dynamic.IsEmpty && dynamic.Bottom <= paint.Height &&
                        ColorDistance(CenterPixel(paint, dynamic), Color.FromArgb(126, 34, 206)) < 4,
                        "responsive bitmap load and paint");
                    Ok(BlockOuterRect(view, BlockById(view.At, "repeat")).Right <= view.ClientSize.Width - 1,
                        "bitmap boxes reflow without layout expansion");
                }
            }
        }

        private static void ObjectFitPaintSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 1800);
                var load = view.GoA(path);
                while (!load.IsCompleted)
                {
                    Application.DoEvents();
                    Thread.Sleep(2);
                }
                load.GetAwaiter().GetResult();
                Eq(1, PicCount(view), "deduplicated fitted image cache");
                using (var paint = Render(view))
                {
                    var fill = BlockRect(view, BlockById(view.At, "fill"));
                    Ok(ColorDistance(paint.GetPixel(fill.Left + 15, fill.Top + 15), Color.FromArgb(220, 38, 38)) < 12 &&
                        ColorDistance(paint.GetPixel(fill.Right - 15, fill.Bottom - 15), Color.FromArgb(37, 99, 235)) < 12,
                        "fill image paint");
                    var contain = BlockRect(view, BlockById(view.At, "contain"));
                    Ok(ColorDistance(paint.GetPixel(contain.Left + 10, contain.Top + 10), Color.White) < 8 &&
                        ColorDistance(paint.GetPixel(contain.Left + 20, contain.Top + contain.Height / 2), Color.FromArgb(220, 38, 38)) < 16 &&
                        ColorDistance(paint.GetPixel(contain.Right - 20, contain.Top + contain.Height / 2), Color.FromArgb(37, 99, 235)) < 16,
                        "contain letterbox paint");
                    var coverLeft = BlockRect(view, BlockById(view.At, "cover-left"));
                    var coverRight = BlockRect(view, BlockById(view.At, "cover-right"));
                    var coverLeftRed = paint.GetPixel(coverLeft.Left + 20, coverLeft.Top + 20);
                    var coverLeftBlue = paint.GetPixel(coverLeft.Right - 10, coverLeft.Top + 20);
                    var coverRightRed = paint.GetPixel(coverRight.Left + 5, coverRight.Top + 20);
                    var coverRightBlue = paint.GetPixel(coverRight.Left + 60, coverRight.Top + 20);
                    Ok(ColorDistance(coverLeftRed, Color.FromArgb(220, 38, 38)) < 16 &&
                        ColorDistance(coverLeftBlue, Color.FromArgb(37, 99, 235)) < 45 &&
                        ColorDistance(coverRightRed, Color.FromArgb(220, 38, 38)) < 16 &&
                        ColorDistance(coverRightBlue, Color.FromArgb(37, 99, 235)) < 16,
                        "positioned cover crop paint: " + coverLeftRed + "/" + coverLeftBlue + "/" + coverRightRed + "/" + coverRightBlue);
                    var none = BlockRect(view, BlockById(view.At, "none"));
                    Ok(ColorDistance(paint.GetPixel(none.Left + 5, none.Top + 5), Color.White) < 8 &&
                        ColorDistance(paint.GetPixel(none.Left + 29, none.Top + 42), Color.FromArgb(220, 38, 38)) < 16 &&
                        ColorDistance(paint.GetPixel(none.Left + 41, none.Top + 42), Color.FromArgb(37, 99, 235)) < 16,
                        "intrinsic none position paint");
                    var scale = BlockRect(view, BlockById(view.At, "scale-down"));
                    Ok(ColorDistance(paint.GetPixel(scale.Left + 1, scale.Top + 1), Color.White) < 8 &&
                        ColorDistance(paint.GetPixel(scale.Left + 2, scale.Top + 6), Color.FromArgb(220, 38, 38)) < 20 &&
                        ColorDistance(paint.GetPixel(scale.Right - 2, scale.Top + 6), Color.FromArgb(37, 99, 235)) < 20,
                        "scale-down paint");
                    var priority = BlockRect(view, BlockById(view.At, "priority"));
                    Ok(ColorDistance(paint.GetPixel(priority.Left + 10, priority.Top + 10), Color.FromArgb(220, 38, 38)) < 16 &&
                        ColorDistance(paint.GetPixel(priority.Left + 10, priority.Bottom - 10), Color.White) < 8,
                        "important top-positioned contain paint");

                    var mapped = BlockRect(view, BlockById(view.At, "mapped-fit"));
                    var leftArea = AreaHitRect(view, "fit-left");
                    var defaultArea = AreaHitRect(view, "fit-default");
                    Eq(new Rectangle(mapped.Left, mapped.Top + 20, 80, 80), leftArea, "fitted map rectangle geometry");
                    Eq(new Rectangle(mapped.Left, mapped.Top + 20, 160, 80), defaultArea, "fitted default map excludes letterbox");
                    Eq("", AreaAt(view, new Point(mapped.Left + 20, mapped.Top + 5)), "image-map letterbox is inert");
                    Eq("fit-left", AreaAt(view, new Point(mapped.Left + 20, mapped.Top + 40)), "fitted image-map point transform");
                    var fitArea = BlockById(view.At, "mapped-fit").Mp.As[0];
                    Eq(leftArea, view.Ab(view.Ax(fitArea.El)), "fitted image-map accessibility bounds");
                    Eq(2, view.Kn, "fitted image-map keyboard target count");
                    Eq(1, view.Kf(), "fitted image-map keyboard focus index");
                    Eq("fit-left", view.Ke.Av("id"), "fitted image-map keyboard element");
                    Eq(leftArea, view.Kb, "fitted image-map keyboard bounds");
                }

                view.Size = new Size(350, 1800);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var dynamic = BlockRect(view, BlockById(view.At, "dynamic"));
                    Ok(BlockById(view.At, "dynamic").S.Of == "v" &&
                        ColorDistance(paint.GetPixel(dynamic.Left + dynamic.Width / 2, dynamic.Top + dynamic.Height / 2), Color.FromArgb(37, 99, 235)) < 24,
                        "responsive object fit repaint");
                }
            }
            Console.WriteLine("Lea object-fit smoke passed: compact fit/position aliases, bounded paint, image-map geometry, accessibility, keyboard, and resize.");
        }

        private static void AspectRatioPaintSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 3600);
                var load = view.GoA(path);
                while (!load.IsCompleted)
                {
                    Application.DoEvents();
                    Thread.Sleep(2);
                }
                load.GetAwaiter().GetResult();
                using (var paint = Render(view))
                {
                    Eq(new Size(240, 135), BlockRect(view, BlockById(view.At, "wide")).Size, "width-driven aspect ratio geometry");
                    Eq(new Size(90, 120), BlockRect(view, BlockById(view.At, "tall")).Size, "height-driven aspect ratio geometry");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "content-box")).Size, "content-box ratio content geometry");
                    Eq(new Size(144, 84), BlockOuterRect(view, BlockById(view.At, "content-box")).Size, "content-box ratio outer geometry");
                    Eq(new Size(120, 60), BlockRect(view, BlockById(view.At, "border-box")).Size, "border-box ratio content geometry");
                    Eq(new Size(144, 84), BlockOuterRect(view, BlockById(view.At, "border-box")).Size, "border-box ratio outer geometry");
                    Eq(new Size(120, 50), BlockRect(view, BlockById(view.At, "both")).Size, "explicit dimensions override ratio");
                    Eq(new Size(200, 70), BlockRect(view, BlockById(view.At, "minimum")).Size, "ratio minimum-height constraint");
                    Eq(new Size(200, 80), BlockRect(view, BlockById(view.At, "maximum")).Size, "ratio maximum-height constraint");
                    Eq(new Size(140, 60), BlockRect(view, BlockById(view.At, "minimum-width")).Size, "ratio minimum-width constraint");
                    Eq(new Size(150, 100), BlockRect(view, BlockById(view.At, "maximum-width")).Size, "ratio maximum-width constraint");
                    Eq(new Size(120, 240), BlockRect(view, BlockById(view.At, "dot-ratio")).Size, "leading-decimal ratio geometry");
                    Ok(BlockRect(view, BlockById(view.At, "text-ratio")).Height > 15, "ratio text preserves content minimum");
                    var owner = BlockRect(view, BlockById(view.At, "owner"));
                    var ownerChild = BlockOuterRect(view, BlockById(view.At, "owner-child"));
                    Eq(new Size(200, 100), owner.Size, "ratio owner minimum geometry");
                    Ok(owner.Contains(ownerChild) && BlockOuterRect(view, BlockById(view.At, "after-owner")).Top > BlockOuterRect(view, BlockById(view.At, "owner")).Bottom,
                        "ratio owner contains child and advances flow");

                    var ratioImage = BlockRect(view, BlockById(view.At, "ratio-image"));
                    var autoImage = BlockRect(view, BlockById(view.At, "auto-image"));
                    Eq(new Size(100, 100), ratioImage.Size, "explicit ratio replaces raster intrinsic ratio");
                    Eq(new Size(100, 50), autoImage.Size, "auto ratio keeps raster intrinsic ratio");
                    Eq(new Size(100, 75), BlockRect(view, BlockById(view.At, "fallback-image")).Size, "missing raster ratio fallback");
                    Eq(new Size(107, 80), BlockRect(view, BlockById(view.At, "height-image")).Size, "height-driven raster ratio");
                    Eq(new Size(120, 120), BlockRect(view, BlockById(view.At, "ratio-svg")).Size, "explicit ratio replaces vector intrinsic ratio");
                    Eq(new Size(120, 40), BlockRect(view, FieldBlock(view.At, FieldById(view.At, "ratio-field"))).Size, "field aspect ratio geometry");
                    Ok(ColorDistance(paint.GetPixel(ratioImage.Left + 15, ratioImage.Top + ratioImage.Height / 2), Color.FromArgb(220, 38, 38)) < 20 &&
                        ColorDistance(paint.GetPixel(ratioImage.Right - 15, ratioImage.Top + ratioImage.Height / 2), Color.FromArgb(37, 99, 235)) < 20,
                        "ratio raster paint");

                    var flex = BlockRect(view, BlockById(view.At, "ratio-flex"));
                    var flexRatio = BlockOuterRect(view, BlockById(view.At, "flex-ratio"));
                    var flexFixed = BlockOuterRect(view, BlockById(view.At, "flex-fixed"));
                    Eq(new Size(120, 60), flexRatio.Size, "ratio flex basis geometry");
                    Ok(flexFixed.Size == new Size(80, 30) && flexFixed.Left - flexRatio.Right == 28 && flex.Height >= 68,
                        "ratio flex sibling placement: " + flex + "/" + flexRatio + "/" + flexFixed);
                    var flexColumn = BlockRect(view, BlockById(view.At, "ratio-flex-column"));
                    var flexColumnRatio = BlockOuterRect(view, BlockById(view.At, "flex-column-ratio"));
                    var flexColumnFixed = BlockOuterRect(view, BlockById(view.At, "flex-column-fixed"));
                    Eq(new Size(120, 80), flexColumnRatio.Size, "ratio column-flex basis geometry");
                    Ok(flexColumnFixed.Size == new Size(60, 40) && flexColumnFixed.Top - flexColumnRatio.Bottom == 18 && flexColumn.Height == 180,
                        "ratio column-flex sibling placement: " + flexColumn + "/" + flexColumnRatio + "/" + flexColumnFixed);
                    var grid = BlockRect(view, BlockById(view.At, "ratio-grid"));
                    var gridRatio = BlockOuterRect(view, BlockById(view.At, "grid-ratio"));
                    var gridFixed = BlockOuterRect(view, BlockById(view.At, "grid-fixed"));
                    Eq(new Size(132, 66), gridRatio.Size, "ratio grid item geometry");
                    Ok(gridFixed.Size == new Size(132, 30) && gridFixed.Left - gridRatio.Right == 28 && grid.Height >= 74 &&
                        BlockOuterRect(view, BlockById(view.At, "after-grid")).Top > BlockOuterRect(view, BlockById(view.At, "ratio-grid")).Bottom,
                        "ratio grid row and following flow: " + grid + "/" + gridRatio + "/" + gridFixed);
                    Eq(new Size(240, 120), BlockRect(view, BlockById(view.At, "ratio-table")).Size, "ratio table preferred minimum");
                    Eq(new Size(120, 80), BlockRect(view, BlockById(view.At, "atomic")).Size, "atomic decimal ratio geometry");
                    Ok(BlockRect(view, BlockById(view.At, "reset")).Height < 60, "reset ratio returns natural height");
                    Eq(new Size(180, 60), BlockRect(view, BlockById(view.At, "ratio-parent")).Size, "ratio parent geometry");
                    Eq(new Size(150, 60), BlockRect(view, BlockById(view.At, "variable")).Size, "variable ratio geometry");
                    Eq(new Size(160, 80), BlockRect(view, BlockById(view.At, "priority")).Size, "important ratio geometry");
                    Eq(new Size(200, 50), BlockRect(view, BlockById(view.At, "dynamic")).Size, "wide responsive ratio geometry");
                }

                view.Size = new Size(350, 4200);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var dynamic = BlockRect(view, BlockById(view.At, "dynamic"));
                    Eq(new Size(200, 200), dynamic.Size, "narrow responsive ratio geometry");
                    Ok(ColorDistance(CenterPixel(paint, dynamic), Color.FromArgb(223, 246, 233)) < 16, "responsive ratio repaint");
                }
            }
            Console.WriteLine("Lea aspect-ratio smoke passed: compact aliases, intrinsic fallback, shared box/flex/grid/table geometry, paint, and resize.");
        }

        private static void OutlinePaintSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 1900);
                view.Go(path);
                using (var paint = Render(view))
                {
                    var solid = BlockOuterRect(view, BlockById(view.At, "solid"));
                    var dashed = BlockOuterRect(view, BlockById(view.At, "dashed"));
                    var dotted = BlockOuterRect(view, BlockById(view.At, "dotted"));
                    var negative = BlockOuterRect(view, BlockById(view.At, "negative"));
                    var atomic = BlockOuterRect(view, BlockById(view.At, "atomic"));
                    var longhand = BlockOuterRect(view, BlockById(view.At, "longhand"));
                    Eq(new Size(160, 50), solid.Size, "outline leaves layout size unchanged");
                    Ok(ColorDistance(paint.GetPixel(solid.Left - 5, solid.Top + solid.Height / 2), Color.FromArgb(161, 59, 24)) < 16 &&
                        ColorDistance(paint.GetPixel(solid.Left - 1, solid.Top + solid.Height / 2), Color.White) < 12,
                        "positive offset solid outline and gap paint");
                    var dashedBand = new Rectangle(dashed.Left - 6, dashed.Top - 6, dashed.Width + 12, 8);
                    var dottedBand = new Rectangle(dotted.Left - 8, dotted.Top - 8, dotted.Width + 16, 10);
                    Ok(HasColor(paint, dashedBand, Color.FromArgb(23, 107, 77)), "dashed outline paint");
                    Ok(HasColor(paint, dottedBand, Color.FromArgb(37, 99, 235)), "rounded dotted outline paint");
                    Ok(ColorDistance(paint.GetPixel(negative.Left + 4, negative.Top + negative.Height / 2), Color.FromArgb(124, 58, 237)) < 20 &&
                        ColorDistance(paint.GetPixel(negative.Left - 2, negative.Top + negative.Height / 2), Color.White) < 12,
                        "negative outline offset paints inward");
                    Ok(ColorDistance(paint.GetPixel(atomic.Left - 1, atomic.Top + atomic.Height / 2), Color.FromArgb(23, 107, 77)) < 20,
                        "invalid shorthand preserves prior outline paint");
                    Ok(ColorDistance(paint.GetPixel(longhand.Left - 9, longhand.Top + longhand.Height / 2), Color.FromArgb(92, 138, 240)) < 36,
                        "alpha longhand outline paint");
                    Eq(BlockOuterRect(view, BlockById(view.At, "solid")).Size, BlockOuterRect(view, BlockById(view.At, "layout-plain")).Size,
                        "outlined and plain peers retain equal geometry");

                    var clipper = BlockRect(view, BlockById(view.At, "clipper"));
                    var clipped = BlockOuterRect(view, BlockById(view.At, "clip-child"));
                    Ok(ColorDistance(paint.GetPixel(clipped.Left + clipped.Width / 2, clipped.Top - 8), Color.FromArgb(220, 38, 38)) < 24 &&
                        ColorDistance(paint.GetPixel(clipped.Left - 8, clipped.Top + clipped.Height / 2), Color.FromArgb(191, 219, 254)) < 20 &&
                        clipper.Contains(clipped),
                        "ancestor overflow clips child outline without changing geometry");
                }

                var beforeFocus = BlockOuterRect(view, FieldBlock(view.At, FieldById(view.At, "focus-field")));
                Eq(1, view.Js("fo('focus-field')"), "focus outline command");
                Application.DoEvents();
                using (var focusPaint = Render(view))
                {
                    var focused = BlockOuterRect(view, FieldBlock(view.At, FieldById(view.At, "focus-field")));
                    Eq(beforeFocus, focused, "focus outline keeps field geometry stable");
                    Ok(FieldById(view.At, "focus-field").S.Ow == 4 && FieldById(view.At, "focus-field").S.Os == "s" &&
                        ColorDistance(focusPaint.GetPixel(focused.Left - 7, focused.Top + focused.Height / 2), Color.FromArgb(220, 38, 38)) < 24,
                        "dynamic focus outline field paint");
                }

                view.Size = new Size(350, 2300);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var responsive = BlockOuterRect(view, BlockById(view.At, "responsive"));
                    var band = responsive;
                    band.Inflate(12, 12);
                    Ok(BlockById(view.At, "responsive").S.Ow == 6 && BlockById(view.At, "responsive").S.Os == "o" &&
                        HasColor(paint, band, Color.FromArgb(37, 99, 235)),
                        "responsive dotted outline repaint");
                    Ok(BlockOuterRect(view, BlockById(view.At, "after-outline")).Top > responsive.Bottom,
                        "outline preserves following narrow flow");
                }
            }
            Console.WriteLine("Lea CSS outline smoke passed: compact aliases, atomic cascade, focus, clipping, fields, exact paint, and resize.");
        }

        private static void TextTransformSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 2300);
                view.CreateControl();
                view.Go(path);
                var retained = FieldById(view.At, "field");
                Rectangle dynamicRect;
                using (var paint = Render(view))
                {
                    var upperRect = BlockOuterRect(view, BlockById(view.At, "upper"));
                    var fieldRect = BlockOuterRect(view, FieldBlock(view.At, retained));
                    var buttonRect = BlockOuterRect(view, BlockById(view.At, "button"));
                    dynamicRect = BlockOuterRect(view, BlockById(view.At, "dynamic"));
                    Ok(!upperRect.IsEmpty && !fieldRect.IsEmpty && !buttonRect.IsEmpty && !dynamicRect.IsEmpty &&
                        ColorDistance(paint.GetPixel(upperRect.Left + 3, upperRect.Top + 3), Color.FromArgb(248, 250, 252)) < 12,
                        "text transform layout and paint");
                }

                var automation = typeof(V).GetMethod("AutomationText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
                var automationType = automation.GetType();
                var source = (string)automationType.GetField("X").GetValue(automation);
                var parts = automationType.GetField("Ps").GetValue(automation) as System.Collections.IEnumerable;
                var mappedUpper = false;
                foreach (var part in parts)
                {
                    var type = part.GetType();
                    var element = type.GetField("El").GetValue(part) as N;
                    var text = (string)type.GetField("X").GetValue(part);
                    var start = (int)type.GetField("S").GetValue(part);
                    var end = (int)type.GetField("E").GetValue(part);
                    if (element != null && element.Av("id") == "upper" && text == "UPPER MIXED TEXT" && end > start &&
                        source.Substring(start, end - start).Equals(text, StringComparison.OrdinalIgnoreCase)) mappedUpper = true;
                }
                Ok(mappedUpper, "transformed visual text maps to authored UI Automation text");

                Move(view, dynamicRect);
                Ok(BlockById(view.At, "dynamic").S.Tt == "u" && BlockById(view.At, "dynamic").X == "DYNAMIC MIXED TEXT",
                    "hover text transform repaint");

                var startEdit = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                if (startEdit == null) throw new InvalidOperationException("native editor entry point unavailable");
                startEdit.Invoke(view, new object[] { FieldById(view.At, "field") });
                Application.DoEvents();
                var editor = Editor(view);
                Ok(editor.Text == "Field MiXeD Text", "native editor exposes raw transformed field value");
                editor.Text = "Retained MiXeD Value";
                Eq("Retained MiXeD Value", retained.V, "text transform editor preserves authored casing");

                view.Size = new Size(350, 2600);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var responsive = BlockOuterRect(view, BlockById(view.At, "responsive"));
                    var after = BlockOuterRect(view, BlockById(view.At, "after-transform"));
                    Ok(BlockById(view.At, "responsive").S.Tt == "l" && BlockById(view.At, "responsive").X == "responsive mixed text" &&
                        Object.ReferenceEquals(retained, FieldById(view.At, "field")) && retained.V == "Retained MiXeD Value" &&
                        E.Tx(retained.V, retained.S) == "retained mixed value", "responsive transform and raw field retention");
                    Ok(!responsive.IsEmpty && after.Top >= responsive.Bottom &&
                        ColorDistance(paint.GetPixel(responsive.Left + 3, responsive.Top + 3), Color.FromArgb(248, 250, 252)) < 12,
                        "responsive text transform paint and following flow");
                }
            }
            Console.WriteLine("Lea text transform smoke passed: compact inherited aliases, inline overrides, controls, automation mapping, hover, raw editing, and resize.");
        }

        private static void TextSpacingSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 2600);
                view.CreateControl();
                view.Go(path);
                var normal = BlockById(view.At, "normal");
                var positive = BlockById(view.At, "positive");
                var negative = BlockById(view.At, "negative");
                var retained = FieldById(view.At, "field");
                var dynamic = BlockById(view.At, "dynamic");
                Rectangle[] normalLines;
                Rectangle[] positiveLines;
                Rectangle[] negativeLines;
                Rectangle[] dynamicBefore;
                Rectangle dynamicRect;
                using (var paint = Render(view))
                {
                    normalLines = TextLines(view, normal);
                    positiveLines = TextLines(view, positive);
                    negativeLines = TextLines(view, negative);
                    dynamicBefore = TextLines(view, dynamic);
                    dynamicRect = BlockOuterRect(view, dynamic);
                    var fieldRect = BlockOuterRect(view, FieldBlock(view.At, retained));
                    var buttonRect = BlockOuterRect(view, BlockById(view.At, "button"));
                    Ok(normalLines.Length == 1 && positiveLines.Length == 1 && negativeLines.Length == 1 &&
                        positiveLines[0].Width > normalLines[0].Width && normalLines[0].Width > negativeLines[0].Width,
                        "text spacing shared positive normal and negative metrics");
                    Ok(TextLines(view, BlockById(view.At, "inline")).Length > 0 &&
                        TextLines(view, BlockById(view.At, "preserved")).Length == 2,
                        "inline override and preserved-line spacing layout");
                    Ok(!fieldRect.IsEmpty && !buttonRect.IsEmpty &&
                        HasColor(paint, positiveLines[0], Color.FromArgb(23, 32, 51)) &&
                        HasColor(paint, fieldRect, Color.FromArgb(23, 32, 51)) &&
                        HasColor(paint, buttonRect, Color.FromArgb(23, 32, 51)),
                        "spaced block field and button glyph paint");
                    var measure = typeof(V).GetMethod("TextWidth", BindingFlags.Static | BindingFlags.NonPublic, null,
                        new Type[] { typeof(Graphics), typeof(string), typeof(Font), typeof(S) }, null);
                    using (var graphics = Graphics.FromImage(paint))
                    using (var font = RenderFont(view, "p", positive.S))
                    {
                        var flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
                        var a = TextRenderer.MeasureText(graphics, "A", font, Size.Empty, flags).Width;
                        var space = TextRenderer.MeasureText(graphics, " ", font, Size.Empty, flags).Width;
                        Eq(a, (int)measure.Invoke(null, new object[] { graphics, "A", font, positive.S }), "single glyph has no edge tracking");
                        Eq(a * 2 + 3, (int)measure.Invoke(null, new object[] { graphics, "AA", font, positive.S }), "tracking only between glyphs");
                        Eq(a * 2 + space + 8 + 6, (int)measure.Invoke(null, new object[] { graphics, "A A", font, positive.S }), "word and letter gaps combine");
                        var cluster = "e\u0301";
                        Eq(TextRenderer.MeasureText(graphics, cluster, font, Size.Empty, flags).Width,
                            (int)measure.Invoke(null, new object[] { graphics, cluster, font, positive.S }), "combining cluster remains intact");
                        Eq(8192, (int)measure.Invoke(null, new object[] { graphics, new string('A', 2000), font, positive.S }), "spaced run width ceiling");
                        var gap = typeof(V).GetMethod("TextGap", BindingFlags.Static | BindingFlags.NonPublic);
                        Eq(1, (int)gap.Invoke(null, new object[] { 10, positive.S, negative.S }), "mixed inline tracking uses rounded average");
                        var wordOnly = BlockById(view.At, "word-only").S;
                        Eq(TextRenderer.MeasureText(graphics, "office", font, Size.Empty, flags).Width,
                            (int)measure.Invoke(null, new object[] { graphics, "office", font, wordOnly }), "word-only spacing preserves shaped words");
                    }
                }

                var automation = typeof(V).GetMethod("AutomationText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
                var automationType = automation.GetType();
                var source = (string)automationType.GetField("X").GetValue(automation);
                var parts = automationType.GetField("Ps").GetValue(automation) as System.Collections.IEnumerable;
                var mappedPositive = false;
                foreach (var part in parts)
                {
                    var type = part.GetType();
                    var element = type.GetField("El").GetValue(part) as N;
                    var text = (string)type.GetField("X").GetValue(part);
                    var start = (int)type.GetField("S").GetValue(part);
                    var end = (int)type.GetField("E").GetValue(part);
                    var rect = (Rectangle)type.GetField("R").GetValue(part);
                    if (element != null && element.Av("id") == "positive" && end > start && !rect.IsEmpty &&
                        source.Substring(start, end - start) == text) mappedPositive = true;
                }
                Ok(mappedPositive, "spaced visual segments map to authored UI Automation text");

                Move(view, dynamicRect);
                Rectangle[] dynamicAfter;
                using (var hoverPaint = Render(view)) dynamicAfter = TextLines(view, BlockById(view.At, "dynamic"));
                var dynamicExpanded = dynamicAfter.Length > dynamicBefore.Length;
                if (!dynamicExpanded && dynamicAfter.Length > 0 && dynamicBefore.Length > 0)
                    dynamicExpanded = dynamicAfter[0].Width > dynamicBefore[0].Width;
                Ok(BlockById(view.At, "dynamic").S.Ls == 8 && BlockById(view.At, "dynamic").S.Wg == 14 &&
                    dynamicBefore.Length == 1 && dynamicAfter.Length > 0 && dynamicExpanded,
                    "hover spacing rebuilds measured geometry");

                var startEdit = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                if (startEdit == null) throw new InvalidOperationException("native editor entry point unavailable");
                startEdit.Invoke(view, new object[] { retained });
                Application.DoEvents();
                var editor = Editor(view);
                Ok(editor.Text == "Field raw words", "native editor exposes raw spaced field value");
                editor.Text = "Retained raw words";
                Eq("Retained raw words", retained.V, "spacing editor preserves authored value");

                view.Size = new Size(350, 2900);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var responsiveBlock = BlockById(view.At, "responsive");
                    var responsive = BlockOuterRect(view, responsiveBlock);
                    var responsiveLines = TextLines(view, responsiveBlock);
                    var after = BlockOuterRect(view, BlockById(view.At, "after-spacing"));
                    Ok(responsiveBlock.S.Ls == -1 && responsiveBlock.S.Wg == 0 &&
                        Object.ReferenceEquals(retained, FieldById(view.At, "field")) && retained.V == "Retained raw words",
                        "responsive spacing and raw field retention");
                    Ok(responsiveLines.Length > 0 && LinesFitWidth(responsiveLines, BlockRect(view, responsiveBlock)) &&
                        !responsive.IsEmpty && after.Top >= responsive.Bottom &&
                        HasColor(paint, responsive, Color.FromArgb(23, 32, 51)),
                        "responsive spacing paint and following flow");
                }
            }
            Console.WriteLine("Lea text spacing smoke passed: compact inherited aliases, signed metrics, inline controls, automation mapping, hover, raw editing, and resize.");
        }

        private static void TextOverflowSmoke(string path)
        {
            var engine = new E { Vw = 720 };
            var page = engine.Go(path);
            Eq("c", page.S.To, "text overflow defaults to clip");
            foreach (var id in new[] { "clip", "noninherited", "unset", "initial", "revert", "dynamic", "responsive" })
                Eq("c", BlockById(page, id).S.To, "text overflow clip/reset " + id);
            foreach (var id in new[] { "ellipsis", "inherited", "invalid", "variable", "important" })
                Eq("e", BlockById(page, id).S.To, "text overflow ellipsis/cascade " + id);
            Eq("e", BlockById(page, "overflow-table").Tb.Rs[0].Cs[0].S.To, "table text overflow style");
            var retained = FieldById(page, "field");
            engine.Dy(page, ById(page.D, "dynamic"), null);
            Eq("e", BlockById(page, "dynamic").S.To, "text overflow hover cascade");
            engine.Vw = 350;
            Eq("e", BlockById(page, "responsive").S.To, "text overflow media cascade");
            Ok(Object.ReferenceEquals(retained, FieldById(page, "field")), "text overflow retains form identity");

            using (var view = new V())
            {
                view.Size = new Size(720, 3200);
                view.CreateControl();
                view.Go(path);
                var raw = view.At.X;
                retained = FieldById(view.At, "field");
                Rectangle dynamicRect;
                using (var paint = Render(view))
                {
                    foreach (var id in new[] { "clip", "short", "visible", "noninherited", "unset", "initial", "revert", "dynamic", "responsive" })
                        Eq(0, OverflowParts(view, id, true).Count, "no ellipsis marker " + id);
                    foreach (var id in new[] { "ellipsis", "spaced", "negative", "inline", "inherited", "invalid", "variable", "important", "cell", "tiny", "cluster" })
                        Eq(1, OverflowParts(view, id, true).Count, "one ellipsis marker " + id);
                    Eq(2, OverflowParts(view, "pre", true).Count, "explicit lines each receive ellipsis");
                    Eq(2, OverflowParts(view, "wrapped", true).Count, "unbreakable wrapped lines receive ellipsis");
                    var clipped = OverflowParts(view, "clip", false);
                    var ellipsed = OverflowParts(view, "ellipsis", false);
                    Eq(clipped.Count, ellipsed.Count, "ellipsis preserves retained atom count");
                    for (var index = 0; index < clipped.Count; index++)
                    {
                        var type = clipped[index].GetType();
                        Eq(type.GetField("X").GetValue(clipped[index]), type.GetField("X").GetValue(ellipsed[index]), "ellipsis keeps authored atoms");
                        Eq(((Rectangle)type.GetField("R").GetValue(clipped[index])).Size,
                            ((Rectangle)type.GetField("R").GetValue(ellipsed[index])).Size, "ellipsis keeps layout sizes");
                    }
                    Eq(BlockOuterRect(view, BlockById(view.At, "clip")).Height,
                        BlockOuterRect(view, BlockById(view.At, "ellipsis")).Height, "ellipsis does not change flow height");
                    var marker = OverflowParts(view, "ellipsis", true)[0];
                    var markerRect = (Rectangle)marker.GetType().GetField("R").GetValue(marker);
                    var content = BlockRect(view, BlockById(view.At, "ellipsis"));
                    Ok(content.Contains(markerRect), "ellipsis marker fits content: " + markerRect + " in " + content);
                    marker.GetType().GetField("Px").SetValue(marker, "");
                    try
                    {
                        using (var withoutMarker = Render(view))
                        {
                            var ink = 0;
                            for (var y = markerRect.Top; y < markerRect.Bottom; y++)
                                for (var x = markerRect.Left; x < markerRect.Right; x++)
                                    if (ColorDistance(withoutMarker.GetPixel(x, y), paint.GetPixel(x, y)) > 30) ink++;
                            Ok(ink > 0, "ellipsis adds real glyph pixels on the same rendering surface");
                        }
                    }
                    finally { marker.GetType().GetField("Px").SetValue(marker, "\u2026"); }
                    var tiny = OverflowParts(view, "tiny", false)[0];
                    Eq("W", tiny.GetType().GetField("Px").GetValue(tiny), "first overflowing character is clipped rather than removed");
                    var cluster = OverflowParts(view, "cluster", false)[0];
                    var prefix = (string)cluster.GetType().GetField("Px").GetValue(cluster);
                    Ok(prefix.Length >= 2 && prefix.Length % 2 == 0 && prefix.EndsWith("e\u0301", StringComparison.Ordinal), "ellipsis preserves combining text elements");

                    var linkMarker = OverflowParts(view, "inline", true)[0];
                    var inlineParts = OverflowParts(view, "inline", false);
                    Eq(" ", (string)inlineParts[1].GetType().GetField("X").GetValue(inlineParts[1]), "space preceding inline link");
                    Ok(inlineParts[1].GetType().GetField("U").GetValue(inlineParts[1]) == null, "space preceding inline link is not clickable");
                    var linkRect = (Rectangle)linkMarker.GetType().GetField("R").GetValue(linkMarker);
                    var hitPoint = new Point(linkRect.Left + linkRect.Width / 2, linkRect.Top + linkRect.Height / 2);
                    var hits = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) as System.Collections.IEnumerable;
                    var linked = false;
                    foreach (var hit in hits)
                    {
                        var type = hit.GetType();
                        var url = type.GetField("U").GetValue(hit) as Uri;
                        if (url != null && url.Fragment == "#target" && ((Rectangle)type.GetField("R").GetValue(hit)).Contains(hitPoint)) linked = true;
                    }
                    Ok(linked, "ellipsis pointer target remains the elided link");
                    dynamicRect = BlockOuterRect(view, BlockById(view.At, "dynamic"));
                }

                var automation = typeof(V).GetMethod("AutomationText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
                var logical = (string)automation.GetType().GetField("X").GetValue(automation);
                Eq(raw, view.At.X, "ellipsis does not mutate document text");
                Ok(logical.Contains("Alpha beta gamma delta epsilon ending") && logical.IndexOf('\u2026') < 0,
                    "UI Automation keeps full logical text without synthetic markers");
                var segments = automation.GetType().GetField("Ps").GetValue(automation) as System.Collections.IEnumerable;
                var visible = false;
                foreach (var segment in segments)
                {
                    var type = segment.GetType();
                    var element = type.GetField("El").GetValue(segment) as N;
                    if (element == null || element.Av("id") != "ellipsis") continue;
                    var value = (string)type.GetField("X").GetValue(segment);
                    var start = (int)type.GetField("S").GetValue(segment);
                    var end = (int)type.GetField("E").GetValue(segment);
                    var rect = (Rectangle)type.GetField("R").GetValue(segment);
                    Ok(value.Length > 0 && value != "\u2026" && logical.Substring(start, end - start) == value,
                        "ellipsis automation segment maps to authored prefix");
                    Ok(rect.Right <= BlockRect(view, BlockById(view.At, "ellipsis")).Right, "ellipsis automation segment uses painted prefix width");
                    visible = true;
                }
                Ok(visible, "ellipsis retains visible automation text");

                Move(view, dynamicRect);
                using (var paint = Render(view)) Eq(1, OverflowParts(view, "dynamic", true).Count, "hover repaints ellipsis");
                Move(view, new Point(0, 0));
                using (var paint = Render(view)) Eq(0, OverflowParts(view, "dynamic", true).Count, "hover restores clip");
                retained.V = "Retained after overflow resize";
                view.Size = new Size(350, 3400);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    Eq(1, OverflowParts(view, "responsive", true).Count, "responsive ellipsis rebuild");
                    Ok(Object.ReferenceEquals(retained, FieldById(view.At, "field")) && retained.V == "Retained after overflow resize",
                        "ellipsis resize retains authored form state");
                    Ok(!BlockOuterRect(view, BlockById(view.At, "target")).IsEmpty, "following content remains laid out");
                }
            }
            Console.WriteLine("Lea text overflow smoke passed: compact clip/ellipsis alias, cascade, paint-only truncation, graphemes, original hits, automation, tables, hover, and resize.");
        }

        private static List<object> OverflowParts(V view, string id, bool markers)
        {
            var values = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) as System.Collections.IEnumerable;
            var result = new List<object>();
            foreach (var value in values)
            {
                var type = value.GetType();
                var block = type.GetField("B").GetValue(value) as B;
                var element = type.GetField("El").GetValue(value) as N;
                if (!(bool)type.GetField("Inline").GetValue(value) ||
                    (block == null || block.El == null || block.El.Av("id") != id) && (element == null || element.Av("id") != id)) continue;
                var marker = (string)type.GetField("X").GetValue(value) == "" && type.GetField("Px").GetValue(value) != null;
                if (marker == markers) result.Add(value);
            }
            return result;
        }

        private static void PointerInteractionSmoke(string path)
        {
            using (var view = new V())
            {
                view.Size = new Size(720, 2200);
                view.Go(path);
                Rectangle inheritedRect;
                Rectangle pointerRect;
                Rectangle textRect;
                Rectangle waitRect;
                Rectangle moveRect;
                Rectangle resizeXRect;
                Rectangle resizeYRect;
                Rectangle forbiddenRect;
                Rectangle hoverCursorRect;
                Rectangle invalidCursorRect;
                Rectangle noneChildRect;
                Rectangle autoChildRect;
                Rectangle deadLinkRect;
                Rectangle liveLinkRect;
                Rectangle deadFieldRect;
                Rectangle liveFieldRect;
                using (var paint = Render(view))
                {
                    var dead = BlockOuterRect(view, BlockById(view.At, "dead-link"));
                    var live = BlockOuterRect(view, BlockById(view.At, "live-link"));
                    inheritedRect = BlockOuterRect(view, BlockById(view.At, "inherited"));
                    pointerRect = BlockOuterRect(view, BlockById(view.At, "pointer"));
                    textRect = BlockOuterRect(view, BlockById(view.At, "text"));
                    waitRect = BlockOuterRect(view, BlockById(view.At, "wait"));
                    moveRect = BlockOuterRect(view, BlockById(view.At, "move"));
                    resizeXRect = BlockOuterRect(view, BlockById(view.At, "resize-x"));
                    resizeYRect = BlockOuterRect(view, BlockById(view.At, "resize-y"));
                    forbiddenRect = BlockOuterRect(view, BlockById(view.At, "forbidden"));
                    hoverCursorRect = BlockOuterRect(view, BlockById(view.At, "hover-cursor"));
                    invalidCursorRect = BlockOuterRect(view, BlockById(view.At, "invalid-cursor"));
                    noneChildRect = BlockOuterRect(view, BlockById(view.At, "none-child"));
                    autoChildRect = BlockOuterRect(view, BlockById(view.At, "auto-child"));
                    deadLinkRect = dead;
                    liveLinkRect = live;
                    deadFieldRect = BlockOuterRect(view, BlockById(view.At, "dead-field"));
                    liveFieldRect = BlockOuterRect(view, BlockById(view.At, "live-field"));
                    Eq(new Size(202, 46), dead.Size, "pointer suppression leaves link layout unchanged");
                    Ok(ColorDistance(CenterPixel(paint, dead), Color.FromArgb(248, 250, 252)) < 12,
                        "pointer-disabled link still paints");
                    Eq(Rectangle.Empty, HitElementRect(view, "dead-link"), "pointer-disabled link has no mouse hit");
                    Ok(!HitElementRect(view, "live-link").IsEmpty && HitRect(view, "second.html").IsEmpty && !HitRect(view, "forms.html").IsEmpty,
                        "live link hit remains while disabled link is absent");
                    Eq(Rectangle.Empty, HitElementRect(view, "dead-field"), "pointer-disabled field has no mouse hit");
                    Ok(!HitElementRect(view, "live-field").IsEmpty && live.Width == dead.Width,
                        "live field and link mouse hits remain bounded");
                }

                Move(view, inheritedRect);
                Eq(Cursors.Cross, view.Cursor, "inherited crosshair cursor");
                Move(view, pointerRect);
                Eq(Cursors.Hand, view.Cursor, "pointer cursor mapping");
                Move(view, textRect);
                Eq(Cursors.IBeam, view.Cursor, "text cursor mapping");
                Move(view, waitRect);
                Eq(Cursors.WaitCursor, view.Cursor, "wait cursor mapping");
                Move(view, moveRect);
                Eq(Cursors.SizeAll, view.Cursor, "move cursor mapping");
                Move(view, resizeXRect);
                Eq(Cursors.SizeWE, view.Cursor, "horizontal resize cursor mapping");
                Move(view, resizeYRect);
                Eq(Cursors.SizeNS, view.Cursor, "vertical resize cursor mapping");
                Move(view, forbiddenRect);
                Eq(Cursors.No, view.Cursor, "not-allowed cursor mapping");
                Move(view, hoverCursorRect);
                Eq(Cursors.Help, view.Cursor, "dynamic hover cursor applies immediately");
                Eq("h", BlockById(view.At, "hover-cursor").S.Cu, "dynamic hover cursor model");
                Move(view, invalidCursorRect);
                Eq(Cursors.Help, view.Cursor, "invalid cursor preserves prior help mapping");

                Move(view, noneChildRect);
                Eq(Cursors.Cross, view.Cursor, "pointer-disabled subtree falls through to page cursor");
                Move(view, autoChildRect);
                Eq(Cursors.Help, view.Cursor, "pointer-event child re-enable and cursor override");
                Move(view, deadFieldRect);
                Eq(Cursors.Cross, view.Cursor, "pointer-disabled field falls through");
                Move(view, liveFieldRect);
                Eq(Cursors.IBeam, view.Cursor, "live field text cursor");

                var before = view.U;
                Move(view, deadLinkRect);
                Eq(Cursors.Cross, view.Cursor, "pointer-disabled link suppresses semantic hand cursor");
                Click(view, deadLinkRect);
                Application.DoEvents();
                Eq(before, view.U, "pointer-disabled link click is inert");
                Ok(BlockById(view.At, "dead-link").S.Bg == "#f8fafc", "pointer-disabled link does not enter hover state");

                Move(view, liveLinkRect);
                Eq(Cursors.Hand, view.Cursor, "automatic live link semantic cursor");
                Ok(BlockById(view.At, "live-link").S.Bg == "#dff6e9", "live link hover remains dynamic");

                Eq(1, view.Js("fo('dead-field')"), "pointer-disabled field remains keyboard focusable");
                Application.DoEvents();
                Ok(view.Ke != null && view.Ke.Av("id") == "dead-field" && !view.Kb.IsEmpty &&
                    !view.Ab(view.Ax(FieldById(view.At, "dead-field").El)).IsEmpty,
                    "pointer suppression preserves keyboard and accessibility bounds");

                view.Size = new Size(350, 2600);
                Application.DoEvents();
                using (var paint = Render(view))
                {
                    var responsive = BlockOuterRect(view, BlockById(view.At, "responsive"));
                    var after = BlockOuterRect(view, BlockById(view.At, "after-pointer"));
                    Ok(BlockById(view.At, "responsive").S.Cu == "w" && !BlockById(view.At, "responsive").S.Pe &&
                        ColorDistance(CenterPixel(paint, responsive), Color.FromArgb(248, 250, 252)) < 12,
                        "responsive pointer state preserves paint");
                    Move(view, responsive);
                    Eq(Cursors.Cross, view.Cursor, "responsive pointer suppression falls through");
                    Ok(after.Top >= responsive.Bottom,
                        "pointer properties preserve following narrow flow");
                }
            }
            Console.WriteLine("Lea pointer interaction smoke passed: compact cursor and pointer-event aliases, inheritance, hover, hits, keyboard, accessibility, and resize.");
        }

        private static AutomationElement AutomationById(AutomationElement root, string id)
        {
            var element = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, id));
            if (element == null) throw new InvalidOperationException("Missing UI Automation element: " + id);
            return element;
        }

        private static AccessibleObject AccessibleByNameAndRole(AccessibleObject root, string name, AccessibleRole role, int depth)
        {
            if (root == null || depth > 16) return null;
            if (root.Name == name && root.Role == role) return root;
            var count = root.GetChildCount();
            for (var index = 0; index < count; index++)
            {
                var found = AccessibleByNameAndRole(root.GetChild(index), name, role, depth + 1);
                if (found != null) return found;
            }
            return null;
        }

        private static Bitmap Render(V view)
        {
            var image = new Bitmap(view.Width, view.Height);
            view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            return image;
        }

        private static void SaveRender(string page, string output)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!String.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            using (var view = new V())
            {
                view.Size = new Size(900, 600);
                view.CreateControl();
                view.Go(page);
                using (var image = Render(view)) image.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void SaveFindRender(string page, string output)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!String.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            using (var view = new V())
            {
                view.Size = new Size(700, 420);
                view.CreateControl();
                view.Go(page);
                view.Fd("cobalt");
                view.Nx();
                using (var image = Render(view)) image.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void SaveEventRender(string page, string output)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!String.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            using (var view = new V())
            {
                view.Size = new Size(720, 420);
                view.CreateControl();
                view.Go(page);
                view.Ck(ById(view.At.D, "go"));
                using (var image = Render(view)) image.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void SaveStateRender(string page, string output)
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!String.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
            using (var view = new V())
            {
                view.Size = new Size(760, 760);
                view.CreateControl();
                view.Go(page);
                view.Ck(ById(view.At.D, "apply"));
                using (var image = Render(view)) image.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void Check(V view, string root, string file, int blocks, int boxes)
        {
            view.Go(Path.Combine(pages, file));
            Paint(view);
            Eq(blocks, view.At.Bs.Count, file + " blocks");
            Eq(boxes, view.Bc, file + " boxes");
        }

        private static string Kinds(P page)
        {
            var values = new string[page.Fs.Count];
            for (var i = 0; i < values.Length; i++) values[i] = page.Fs[i].K;
            return String.Join(",", values);
        }

        private static string CellKinds(Tc cell)
        {
            var values = new string[cell.Bs.Count];
            for (var i = 0; i < values.Length; i++) values[i] = cell.Bs[i].K;
            return String.Join(",", values);
        }

        private static string BlockKinds(P page)
        {
            var values = new string[page.Bs.Count];
            for (var i = 0; i < values.Length; i++) values[i] = page.Bs[i].K;
            return String.Join(",", values);
        }

        private static List<N> Nodes(N node, string kind)
        {
            var found = new List<N>();
            AddNodes(node, kind, found);
            return found;
        }

        private static void AddNodes(N node, string kind, List<N> found)
        {
            if (node == null) return;
            if (node.K == kind) found.Add(node);
            foreach (var child in node.C) AddNodes(child, kind, found);
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

        private static int NodeDepth(N node)
        {
            if (node == null) return 0;
            var depth = 1;
            foreach (var child in node.C) depth = Math.Max(depth, 1 + NodeDepth(child));
            return depth;
        }

        private static int TreeNodes(N node)
        {
            if (node == null) return 0;
            var count = 1;
            foreach (var child in node.C) count += TreeNodes(child);
            return count;
        }

        private static string NodeText(N node)
        {
            if (node == null) return "";
            if (node.K == "#text") return node.X;
            var text = new StringBuilder();
            foreach (var child in node.C) text.Append(NodeText(child));
            return text.ToString();
        }

        private static int ClassCount(N node)
        {
            return node == null ? 0 : node.Av("class").Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static bool Has(System.Collections.Generic.IList<string> values, string text)
        {
            foreach (var value in values)
                if ((value ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static Ir Run(B block, string kind)
        {
            foreach (var run in block.Is) if (run.K == kind) return run;
            throw new InvalidOperationException("inline run unavailable: " + kind);
        }

        private static void Click(V view, Rectangle rect)
        {
            var method = typeof(V).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("mouse click method unavailable");
            method.Invoke(view, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, 0) });
        }

        private static KeyEventArgs Key(V view, Keys key)
        {
            var args = new KeyEventArgs(key);
            var method = typeof(V).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("key method unavailable");
            method.Invoke(view, new object[] { args });
            return args;
        }

        private static KeyEventArgs EditKey(V view, Keys key)
        {
            var args = new KeyEventArgs(key);
            var method = typeof(V).GetMethod("EditKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("editor key method unavailable");
            method.Invoke(view, new object[] { null, args });
            return args;
        }

        private static bool InputKey(V view, Keys key)
        {
            var method = typeof(V).GetMethod("IsInputKey", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("input-key method unavailable");
            return (bool)method.Invoke(view, new object[] { key });
        }

        private static bool CommandKey(V view, Keys key)
        {
            var method = typeof(V).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("command-key method unavailable");
            var message = Message.Create(IntPtr.Zero, 0x0104, (IntPtr)(int)(key & Keys.KeyCode), IntPtr.Zero);
            return (bool)method.Invoke(view, new object[] { message, key });
        }

        private static void Move(V view, Rectangle rect)
        {
            Move(view, new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2));
        }

        private static void Move(V view, Point point)
        {
            var method = typeof(V).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("mouse move method unavailable");
            method.Invoke(view, new object[] { new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0) });
        }

        private static Rectangle LinkRect(V view, string file)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url == null || !Path.GetFileName(url.LocalPath).Equals(file, StringComparison.OrdinalIgnoreCase)) continue;
                return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static int LinkBoxes(V view, string file, bool wordsOnly = false)
        {
            var count = 0;
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url != null && Path.GetFileName(url.LocalPath).Equals(file, StringComparison.OrdinalIgnoreCase) &&
                    (!wordsOnly || ((string)type.GetField("X").GetValue(value)).Trim().Length > 0)) count++;
            }
            return count;
        }

        private static Rectangle BlockRect(V view, B block)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || (bool)type.GetField("Inline").GetValue(value)) continue;
                return (Rectangle)type.GetField("C").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static Rectangle BlockOuterRect(V view, B block)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || (bool)type.GetField("Inline").GetValue(value)) continue;
                return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static Rectangle ElementRect(V view, N element, bool outer)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("El").GetValue(value), element) || (bool)type.GetField("Inline").GetValue(value)) continue;
                return (Rectangle)type.GetField(outer ? "R" : "C").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static B FieldBlock(P page, Fi field)
        {
            foreach (var block in page.Bs) if (Object.ReferenceEquals(block.I, field)) return block;
            throw new InvalidOperationException("field block unavailable: " + (field == null ? "" : field.N));
        }

        private static Fi FieldById(P page, string id)
        {
            foreach (var field in page.Fs)
                if (field.El != null && field.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return field;
            throw new InvalidOperationException("field unavailable: " + id);
        }

        private static B BlockById(P page, string id)
        {
            foreach (var block in page.Bs)
            {
                var found = BlockById(block, id);
                if (found != null) return found;
            }
            throw new InvalidOperationException("block unavailable: " + id);
        }

        private static bool HasBlock(P page, string id)
        {
            foreach (var block in page.Bs)
                if (BlockById(block, id) != null) return true;
            return false;
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

        private static Dv ValueById(P page, string id)
        {
            foreach (var value in page.Dvs)
                if (value.El != null && value.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return value;
            throw new InvalidOperationException("value unavailable: " + id);
        }

        private static Ir RunByValue(B block, Dv value)
        {
            if (block == null) return null;
            foreach (var run in block.Is) if (Object.ReferenceEquals(run.Dv, value)) return run;
            if (block.Tb == null) return null;
            foreach (var row in block.Tb.Rs)
                foreach (var cell in row.Cs)
                    foreach (var part in cell.Bs)
                    {
                        var found = RunByValue(part, value);
                        if (found != null) return found;
                    }
            return null;
        }

        private static TextBox Editor(V view)
        {
            var member = typeof(V).GetField("ed", BindingFlags.Instance | BindingFlags.NonPublic);
            var editor = member == null ? null : member.GetValue(view) as TextBox;
            if (editor == null) throw new InvalidOperationException("native editor unavailable");
            return editor;
        }

        private static Rectangle[] TextLines(V view, B block)
        {
            var lines = new List<Rectangle>();
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var rect = (Rectangle)type.GetField("R").GetValue(value);
                var found = -1;
                for (var i = 0; i < lines.Count; i++) if (lines[i].Top == rect.Top) { found = i; break; }
                if (found < 0) lines.Add(rect);
                else lines[found] = Rectangle.Union(lines[found], rect);
            }
            lines.Sort(delegate(Rectangle a, Rectangle b) { return a.Top.CompareTo(b.Top); });
            return lines.ToArray();
        }

        private static Rectangle ClipRect(V view, B block)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var clip = (Rectangle)type.GetField("Clip").GetValue(value);
                if (!clip.IsEmpty) return clip;
            }
            return Rectangle.Empty;
        }

        private static Rectangle OwnedClip(V view, B block, string kind)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || (string)type.GetField("K").GetValue(value) != kind) continue;
                var clip = (Rectangle)type.GetField("Clip").GetValue(value);
                if (!clip.IsEmpty) return clip;
            }
            return Rectangle.Empty;
        }

        private static Rectangle HitRect(V view, string file)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("hit values unavailable");
            foreach (var value in values)
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url != null && Path.GetFileName(url.LocalPath).Equals(file, StringComparison.OrdinalIgnoreCase))
                    return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static Rectangle HitElementRect(V view, string id)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("element hit values unavailable");
            foreach (var value in values)
            {
                var type = value.GetType();
                var element = type.GetField("El").GetValue(value) as N;
                if (element != null && element.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase))
                    return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static Rectangle AreaHitRect(V view, string id)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("area hit values unavailable");
            foreach (var value in values)
            {
                var type = value.GetType();
                var area = type.GetField("A").GetValue(value) as Ar;
                if (area != null && area.El != null && area.El.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase))
                    return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static string AreaAt(V view, Point point)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("area hit values unavailable");
            var hits = new List<object>();
            foreach (var value in values) hits.Add(value);
            for (var index = hits.Count - 1; index >= 0; index--)
            {
                var value = hits[index];
                var type = value.GetType();
                var area = type.GetField("A").GetValue(value) as Ar;
                var contains = type.GetMethod("Contains", BindingFlags.Instance | BindingFlags.Public);
                if (area != null && contains != null && (bool)contains.Invoke(value, new object[] { point })) return area.El.Av("id");
            }
            return "";
        }

        private static Rectangle FragmentHitRect(V view, string fragment)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("fragment hit values unavailable");
            foreach (var value in values)
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url != null && url.Fragment.Equals(fragment, StringComparison.OrdinalIgnoreCase))
                    return (Rectangle)type.GetField("R").GetValue(value);
            }
            return Rectangle.Empty;
        }

        private static bool ElementVisible(V view, N element)
        {
            Rectangle inline = Rectangle.Empty;
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("El").GetValue(value), element)) continue;
                var rect = (Rectangle)type.GetField("C").GetValue(value);
                if (!(bool)type.GetField("Fx").GetValue(value)) rect.Offset(0, -view.Sy);
                if (!(bool)type.GetField("Inline").GetValue(value)) return rect.IntersectsWith(view.ClientRectangle);
                if (inline.IsEmpty) inline = rect;
            }
            return !inline.IsEmpty && inline.IntersectsWith(view.ClientRectangle);
        }

        private static string PaintedText(V view, B block)
        {
            var text = new StringBuilder();
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block)) continue;
                var part = type.GetField("X").GetValue(value) as string;
                if (!String.IsNullOrEmpty(part)) text.Append(part);
            }
            return text.ToString();
        }

        private static bool Centered(Rectangle line, Rectangle box)
        {
            return Math.Abs((line.Left + line.Right) - (box.Left + box.Right)) <= 2;
        }

        private static Rectangle ContainingCell(V view, B table, Rectangle line)
        {
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), table) || (bool)type.GetField("Inline").GetValue(value)) continue;
                if ((string)type.GetField("K").GetValue(value) != "td") continue;
                var content = (Rectangle)type.GetField("C").GetValue(value);
                if (content.IntersectsWith(line)) return content;
            }
            return Rectangle.Empty;
        }

        private static bool SameInlineHeights(V view, B block)
        {
            var height = 0;
            var count = 0;
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                var current = ((Rectangle)type.GetField("R").GetValue(value)).Height;
                if (height > 0 && current != height) return false;
                height = current;
                count++;
            }
            return count >= 2;
        }

        private static bool LinesFitWidth(Rectangle[] lines, Rectangle box)
        {
            if (lines.Length == 0 || box.IsEmpty) return false;
            foreach (var line in lines) if (line.Left < box.Left || line.Right > box.Right) return false;
            return true;
        }

        private static Color BoxPixel(Bitmap image, Rectangle box)
        {
            return image.GetPixel(Math.Max(box.Left, box.Right - 12), box.Top + box.Height / 2);
        }

        private static Color CenterPixel(Bitmap image, Rectangle box)
        {
            return image.GetPixel(box.Left + box.Width / 2, box.Top + box.Height / 2);
        }

        private static int ColorDistance(Color left, Color right)
        {
            return Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);
        }

        private static bool HasColor(Bitmap image, Rectangle area, Color color)
        {
            area = Rectangle.Intersect(area, new Rectangle(Point.Empty, image.Size));
            for (var y = area.Top; y < area.Bottom; y++)
                for (var x = area.Left; x < area.Right; x++)
                    if (ColorDistance(image.GetPixel(x, y), color) < 8) return true;
            return false;
        }

        private static Font RenderFont(V view, string kind, S style, bool link = false, string baseKind = null)
        {
            var method = typeof(V).GetMethod("BoxFont", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("font renderer unavailable");
            return (Font)method.Invoke(view, new object[] { kind, style, false, link, baseKind });
        }

        private static bool HasFontStyle(Font font, FontStyle style)
        {
            return (font.Style & style) == style;
        }

        private static int FontCacheCount(V view)
        {
            var field = typeof(V).GetField("fontNames", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IDictionary;
            if (values == null) throw new InvalidOperationException("font cache unavailable");
            return values.Count;
        }

        private static int PicCount(V view)
        {
            var field = typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IDictionary;
            if (values == null) throw new InvalidOperationException("image cache unavailable");
            return values.Count;
        }

        private static bool PicCached(V view, Uri source)
        {
            var field = typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = field == null ? null : field.GetValue(view) as System.Collections.IDictionary;
            if (values == null) throw new InvalidOperationException("image cache unavailable");
            return source != null && values.Contains(source.AbsoluteUri);
        }

        private static System.Collections.IEnumerable LayoutBoxes(V view)
        {
            var field = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new InvalidOperationException("layout boxes unavailable");
            var values = field.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("layout box values unavailable");
            return values;
        }

        private static int HitCount(V view, bool fields)
        {
            var member = typeof(V).GetField("hits", BindingFlags.Instance | BindingFlags.NonPublic);
            var values = member == null ? null : member.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("hit values unavailable");
            var count = 0;
            foreach (var value in values)
            {
                var type = value.GetType();
                if (fields ? type.GetField("B").GetValue(value) != null : type.GetField("U").GetValue(value) != null) count++;
            }
            return count;
        }

        private static void CheckDisposal(string root)
        {
            var source = Path.Combine(pages, "script-bridge.html");
            using (var view = new V())
            {
                var alerts = 0;
                var done = 0;
                view.Al += delegate { alerts++; view.Dispose(); };
                view.Done += delegate { done++; };
                view.Go(source);
                Eq(1, alerts, "disposing in first alert suppresses remaining alerts");
                Eq(0, done, "disposing in alert suppresses completion");
                Ok(!view.IsHandleCreated, "disposed alert view has no native handle");
            }
            using (var view = new V())
            {
                var late = 0;
                view.Changed += delegate { view.Dispose(); };
                view.Al += delegate { late++; };
                view.Done += delegate { late++; };
                var work = view.GoA(source);
                WaitFor(() => work.IsCompleted, "dispose from async Changed completion");
                work.GetAwaiter().GetResult();
                Eq(0, late, "disposing in Changed suppresses alerts and completion");
                view.St();
                var rejected = false;
                try { view.Go(source); }
                catch (ObjectDisposedException) { rejected = true; }
                Ok(rejected, "disposed view rejects new navigation");
            }
            using (var view = new V())
            {
                var gate = new StepContext();
                var context = SynchronizationContext.Current;
                Task<P> work;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(gate);
                    work = view.GoA(source);
                }
                finally { SynchronizationContext.SetSynchronizationContext(context); }
                Ok(!work.IsCompleted && view.Busy, "local load held before disposal");
                view.Dispose();
                WaitFor(() => { gate.Step(); return work.IsCompleted; }, "disposed local load completion");
                Eq("cancel", Fails(() => work.GetAwaiter().GetResult(), "local load cancellation").K, "local cancellation before parsing");
                Ok(view.At == null, "disposed local load did not publish a page");
            }
            using (var server = new CompressedServer())
            using (var view = new V())
            {
                var events = 0;
                view.Changed += delegate { events++; };
                view.Done += delegate { events++; };
                var work = view.GoA(new Uri(server.U, "?external-page=slow"));
                WaitFor(() => server.Hits == 2, "pending stylesheet request");
                view.Dispose();
                WaitFor(() => work.IsCompleted, "disposed stylesheet load completion");
                Eq("cancel", Fails(() => work.GetAwaiter().GetResult(), "stylesheet cancellation").K, "stylesheet cancellation kind");
                Eq(0, events, "disposed stylesheet load did not publish callbacks");
            }
            using (var server = new CompressedServer())
            using (var view = new V())
            {
                var events = 0;
                view.Done += delegate { events++; };
                var work = view.GoA(new Uri(server.U, "?image-page=1"));
                WaitFor(() => server.Hits == 2, "pending image request");
                view.Dispose();
                WaitFor(() => work.IsCompleted, "disposed image request completion");
                work.GetAwaiter().GetResult();
                Eq(0, events, "disposed image load did not emit completion");
                var images = (Dictionary<string, Image>)typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                Eq(0, images.Count, "disposed image load did not repopulate released cache");
            }
            Console.WriteLine("Lea disposal smoke passed: local/CSS/image cancellation, callback disposal, and navigation rejection.");
        }

        private static void CheckInputLimits(string root)
        {
            var path = Path.Combine(Path.GetTempPath(), "lea-input-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                File.WriteAllText(path, new string(' ', 4 * 1024 * 1024 + 1), new UTF8Encoding(false));
                foreach (var asynchronous in new[] { false, true })
                {
                    var engine = new E();
                    var retained = engine.Go(Path.Combine(pages, "second.html"));
                    var error = Fails(delegate
                    {
                        if (!asynchronous) engine.Go(path);
                        else
                        {
                            var work = engine.GoA(path);
                            WaitFor(() => work.IsCompleted, "oversized local input completion");
                            work.GetAwaiter().GetResult();
                        }
                    }, "oversized local document " + asynchronous);
                    Ok(error.K == "large" && !error.Rt, "document input limit has a nonretryable typed error");
                    Ok(engine.At == retained && !engine.Busy && !engine.CanBk, "oversized input preserves the prior page and history");
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
            CheckHttpInput(root);
            CheckInputBoundaries();
            Console.WriteLine("Lea input limit smoke passed.");
        }

        private static void CheckCacheIdentity()
        {
            using (var server = new CompressedServer())
            {
                var origin = "http://127.0.0.1:" + server.U.Port;
                var memory = new E { Cm = 16 };
                var start = server.Hits;
                var pathUpper = new Uri(origin + "/CachePath?max-age=60&case-key=Path");
                var pathLower = new Uri(origin + "/cachepath?max-age=60&case-key=Path");
                var pathUpperPage = memory.Go(pathUpper);
                var pathLowerWork = memory.GoA(pathLower);
                WaitFor(() => pathLowerWork.IsCompleted, "path-case async cache request");
                Ok(!pathUpperPage.Ca && !pathLowerWork.GetAwaiter().GetResult().Ca,
                    "path-case resources stay distinct in memory cache");
                Eq(start + 2, server.Hits, "path-case resources make separate requests");

                start = server.Hits;
                var queryUpper = new Uri(origin + "/cache-query?max-age=60&case-key=Query");
                var queryLower = new Uri(origin + "/cache-query?max-age=60&case-key=query");
                Eq("Case Query", memory.Go(queryUpper).T, "upper-case query response");
                var queryPage = memory.Go(queryLower);
                Ok(!queryPage.Ca && queryPage.T == "Case query", "query-case resources stay distinct in memory cache");
                Eq(start + 2, server.Hits, "query-case resources make separate requests");

                start = server.Hits;
                var fragmentOne = new Uri(origin + "/fragment-cache?max-age=60&case-key=Fragment#one");
                var fragmentTwo = new Uri(origin + "/fragment-cache?max-age=60&case-key=Fragment#two");
                memory.Go(fragmentOne);
                var fragmentPage = memory.Go(fragmentTwo);
                Ok(!fragmentPage.Ca && fragmentPage.U.Fragment == "#two", "fragments share one exact resource representation");
                Eq(start + 1, server.Hits, "fragment-only cache reuse avoids a request");

                Eq(new Uri("HTTP://EXAMPLE.COM:80/Cache").AbsoluteUri, new Uri("http://example.com/Cache").AbsoluteUri,
                    "URI canonicalization normalizes scheme, host, and default port");

                start = server.Hits;
                var validatorUpper = new Uri(origin + "/CaseValidator?etag=1&case-key=Validator");
                var validatorLower = new Uri(origin + "/casevalidator?etag=1&case-key=Validator");
                memory.Go(validatorUpper);
                var validatorPage = memory.Go(validatorLower);
                Ok(!validatorPage.Ca && server.LastIfNoneMatch.Length == 0,
                    "case-different resource does not inherit another validator");
                Eq(start + 2, server.Hits, "case-different validator resource uses a full request");

                start = server.Hits;
                var css = new E { Cm = 16 };
                var upperSheet = css.Go(new Uri(server.U, "?external-case=Upper"));
                var lowerSheetWork = css.GoA(new Uri(server.U, "?external-case=upper"));
                WaitFor(() => lowerSheetWork.IsCompleted, "query-case async stylesheet request");
                var lowerSheet = lowerSheetWork.GetAwaiter().GetResult();
                Eq("#123456", upperSheet.S.Bg, "upper-case stylesheet resource");
                Eq("#654321", lowerSheet.S.Bg, "lower-case stylesheet resource");
                Eq(start + 4, server.Hits, "case-different linked stylesheets make separate requests");

                start = server.Hits;
                var navigation = new E();
                navigation.Go(new Uri(origin + "/CaseNavigation?no-store=1&case-key=Navigation#one"));
                var navigated = navigation.Go(new Uri(origin + "/casenavigation?no-store=1&case-key=Navigation#two"));
                Ok(navigated.U.AbsolutePath == "/casenavigation" && navigated.U.Fragment == "#two",
                    "case-different navigation retains the requested URL");
                Eq(start + 2, server.Hits, "case-different navigation is not treated as fragment-only");

                var diskRoot = Path.Combine(Path.GetTempPath(), "lea-cache-identity-" + Guid.NewGuid().ToString("N"));
                try
                {
                    start = server.Hits;
                    var diskUpper = new Uri(origin + "/DiskCache?max-age=60&case-key=Disk");
                    var diskLower = new Uri(origin + "/diskcache?max-age=60&case-key=Disk");
                    var seed = new E { Cd = diskRoot, Cm = 0 };
                    Ok(!seed.Go(diskUpper).Ca && seed.Dc == 1, "persistent case identity seed");
                    var restart = new E { Cd = diskRoot, Cm = 0 };
                    var distinct = restart.Go(diskLower);
                    Ok(!distinct.Ca && restart.Dc == 2, "persistent path-case resources use separate entries");
                    Eq(start + 2, server.Hits, "persistent case-different resource uses network");
                    var secondRestart = new E { Cd = diskRoot, Cm = 0 };
                    Ok(secondRestart.Go(diskLower).Ca && secondRestart.Dc == 2,
                        "exact persistent cache entry survives restart");
                    Eq(start + 2, server.Hits, "exact persistent restart avoids network");
                }
                finally
                {
                    if (Directory.Exists(diskRoot)) Directory.Delete(diskRoot, true);
                }
            }
            Console.WriteLine("Lea cache identity smoke passed.");
        }

        private static P InputPage(E engine, Uri url, bool asynchronous, bool post = false)
        {
            if (!asynchronous) return post ? engine.Po(url, "field=value") : engine.Go(url);
            var work = post ? engine.PoA(url, "field=value") : engine.GoA(url);
            WaitFor(() => work.IsCompleted, "bounded HTTP input completion");
            return work.GetAwaiter().GetResult();
        }

        private static byte[] InputBody(int length, bool css = false)
        {
            var prefix = css ? "body{background-color:#135724}" : "<html><head><title>Bounded Input</title></head><body><input value='Retained input'></body></html>";
            return Encoding.ASCII.GetBytes(prefix + new string(' ', Math.Max(0, length - prefix.Length)));
        }

        private static void CheckHttpInput(string root)
        {
            var seed = Path.Combine(pages, "second.html");
            foreach (var asynchronous in new[] { false, true })
            foreach (var post in new[] { false, true })
            foreach (var mode in new[] { "declared", "overflow", "gzip", "deflate" })
            using (var server = new InputServer(mode, InputBody(4 * 1024 * 1024 + 1)))
            {
                var engine = new E { Tm = 5000 };
                var retained = engine.Go(seed);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var error = Fails(() => InputPage(engine, server.U, asynchronous, post), "oversized HTTP input " + mode);
                Ok(error.K == "large" && !error.Rt && engine.At == retained && !engine.Busy && !engine.CanBk,
                    "HTTP/POST size rejection preserves history and readiness " + mode);
                if (mode == "declared" || mode == "overflow") Ok(clock.ElapsedMilliseconds < 2000, "input rejects before held response ends " + mode);
            }
            foreach (var asynchronous in new[] { false, true })
            foreach (var mode in new[] { "plain", "gzip", "deflate" })
            using (var server = new InputServer(mode, InputBody(4 * 1024 * 1024)))
            {
                var engine = new E();
                var page = InputPage(engine, server.U, asynchronous);
                Ok(page.T == "Bounded Input" && page.Sz == engine.Hm && engine.Cc == 0 && page.Fs[0].V == "Retained input",
                    "exact maximum document accepted without exceeding cache limit " + mode);
            }
            foreach (var asynchronous in new[] { false, true })
            foreach (var mode in new[] { "headers", "trickle" })
            using (var server = new InputServer(mode, InputBody(1024)))
            {
                var engine = new E { Tm = 300 };
                var retained = engine.Go(seed);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var error = Fails(() => InputPage(engine, server.U, asynchronous), "HTTP input deadline " + mode);
                Ok(error.K == "timeout" && error.Rt && clock.ElapsedMilliseconds < 1200 && !engine.Busy && engine.At == retained,
                    "one input deadline bounds headers and trickling bytes " + mode);
            }
            foreach (var asynchronous in new[] { false, true })
            foreach (var mode in new[] { "declared", "overflow", "gzip", "deflate", "trickle" })
            using (var server = new InputServer(mode, InputBody(mode == "trickle" ? 1024 : 256 * 1024 + 1, true), "text/css"))
            {
                var extra = mode == "trickle" ? "<link rel='stylesheet' href='/two'><link rel='stylesheet' href='/three'>" : "";
                server.PageMarkup = "<html><head><title>Optional CSS</title><link rel='stylesheet' href='/one'>" + extra + "</head><body><input value='Retained CSS input'></body></html>";
                var engine = new E { Tm = mode == "trickle" ? 300 : 5000 };
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var page = InputPage(engine, server.Page, asynchronous);
                Ok(page.T == "Optional CSS" && page.Fs[0].V == "Retained CSS input" && page.S.Bg != "#135724" && engine.Cc == 0 && !engine.Busy,
                    "optional oversized/stalled CSS preserves the page " + mode);
                if (mode == "trickle") Ok(server.Hits == 1 && clock.ElapsedMilliseconds < 1200, "linked sheets share the remaining input budget");
            }
            foreach (var asynchronous in new[] { false, true })
            using (var server = new InputServer("plain", InputBody(256 * 1024 + 1, true), "text/css") { Cacheable = true })
            {
                var engine = new E();
                InputPage(engine, server.U, asynchronous);
                Eq(1, engine.Cc, "large text/css document can occupy the document cache");
                server.PageMarkup = "<html><head><link rel='stylesheet' href='/input'></head><body>Cache boundary</body></html>";
                var page = InputPage(engine, server.Page, asynchronous);
                Ok(page.S.Bg != "#135724" && server.Hits == 1 && engine.Cc == 0, "cached document cannot bypass stylesheet byte limit");
            }
            using (var server = new InputServer("headers", InputBody(1024)))
            using (var good = new InputServer("plain", InputBody(1024)))
            {
                var engine = new E();
                var retained = engine.Go(seed);
                var work = engine.GoA(server.U);
                WaitFor(() => server.Hits == 1, "pending input request");
                var blocked = false;
                try { engine.Go(seed); }
                catch (InvalidOperationException) { blocked = true; }
                Ok(blocked && engine.Busy, "sync document cannot overlap async resource ownership");
                engine.St();
                WaitFor(() => work.IsCompleted, "stopped input request");
                var error = Fails(() => work.GetAwaiter().GetResult(), "stopped resource request");
                Ok(error.K == "cancel" && !error.Rt && !engine.Busy && engine.At == retained, "Stop is distinct from timeout and retains the page");
                Eq("Bounded Input", InputPage(engine, good.U, true).T, "next request succeeds after cancellation");
            }
            var path = Path.Combine(Path.GetTempPath(), "lea-css-origin-" + Guid.NewGuid().ToString("N") + ".css");
            var html = path + ".html";
            try
            {
                File.WriteAllText(path, "body{background-color:#135724}", new UTF8Encoding(false));
                var fileUrl = new Uri(path).AbsoluteUri;
                var markup = "<html><head><link rel='stylesheet' href='" + fileUrl + "'></head><body>Origin test</body></html>";
                File.WriteAllText(html, markup, new UTF8Encoding(false));
                var local = new E().Go(html);
                Eq("#135724", local.S.Bg, "local documents retain local linked CSS");
                foreach (var useBase in new[] { false, true })
                using (var server = new InputServer("plain", InputBody(1024, true), "text/css"))
                {
                    server.PageMarkup = useBase ? markup.Replace("<head>", "<head><base href='" + fileUrl + "'>").Replace("href='" + fileUrl + "'></head>", "href='" + Path.GetFileName(path) + "'></head>") : markup;
                    var page = InputPage(new E(), server.Page, true);
                    Ok(page.S.Bg != "#135724" && server.Hits == 0, "remote document/base cannot grant local CSS access");
                }
                var link = (N)Activator.CreateInstance(typeof(N), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { "link" }, null);
                link.A["href"] = "file://example.invalid/share/sheet.css";
                var method = typeof(E).GetMethod("StyleUrl", BindingFlags.Static | BindingFlags.NonPublic);
                Ok(!(bool)method.Invoke(null, new object[] { link, new Uri(html), new Uri(html), null }), "UNC stylesheet rejected before I/O");
            }
            finally { if (File.Exists(html)) File.Delete(html); if (File.Exists(path)) File.Delete(path); }
        }

        private static void CheckInputBoundaries()
        {
            var engine = new E { Tm = 100 };
            var begin = typeof(E).GetMethod("BeginInput", BindingFlags.NonPublic | BindingFlags.Instance);
            var end = typeof(E).GetMethod("EndInput", BindingFlags.NonPublic | BindingFlags.Instance);
            var classify = typeof(E).GetMethod("InputError", BindingFlags.NonPublic | BindingFlags.Instance);
            begin.Invoke(engine, null);
            try
            {
                var budget = (CancellationTokenSource)typeof(E).GetField("inputBudget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(engine);
                WaitFor(() => budget.IsCancellationRequested, "input classification deadline");
                Func<Exception, Er> kind = failure => (Er)classify.Invoke(engine, new object[] { failure, new Uri("http://example.invalid/") });
                Eq("timeout", kind(new OperationCanceledException()).K, "expired input classifies cancellation as timeout");
                Eq("decode", kind(new InvalidDataException()).K, "timeout does not hide a distinct decode error");
                Ok(kind(new InvalidOperationException()) == null, "timeout does not hide an internal exception");
                engine.St();
                Eq("cancel", kind(new InvalidDataException()).K, "Stop takes precedence over a known loading failure");
                Ok(kind(new InvalidOperationException()) == null, "Stop does not hide an internal exception");
            }
            finally { end.Invoke(engine, null); }
            var type = typeof(E).Assembly.GetType("Lea.Engine.Ri", true);
            foreach (var asynchronous in new[] { false, true })
            foreach (var length in new[] { -1L, 4097L })
            using (var stream = new CountedInput(new byte[8192]))
            {
                var method = type.GetMethod(asynchronous ? "BytesA" : "Bytes", BindingFlags.Static | BindingFlags.NonPublic);
                var error = Fails(delegate
                {
                    object result;
                    try { result = method.Invoke(null, new object[] { stream, new Uri("http://example.invalid/"), length, "", 4096, CancellationToken.None }); }
                    catch (TargetInvocationException failure) { throw failure.InnerException; }
                    if (asynchronous) ((Task<byte[]>)result).GetAwaiter().GetResult();
                }, "bounded input stream");
                Ok(error.K == "large" && stream.ReadBytes == (length < 0 ? 4097 : 0), "declared input reads zero bytes; unknown input reads only the overflow byte");
            }
            using (var view = new V())
            {
                Ok(view.Hm == 4 * 1024 * 1024 && view.Csm == 256 * 1024, "compact view input limits match engine limits");
            }
        }

        private sealed class CountedInput : MemoryStream
        {
            public int ReadBytes;
            public CountedInput(byte[] bytes) : base(bytes) { }
            public override int Read(byte[] buffer, int offset, int count)
            {
                var value = base.Read(buffer, offset, count);
                ReadBytes += value;
                return value;
            }
        }

        private sealed class InputServer : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly ManualResetEvent stop = new ManualResetEvent(false);
            private readonly Thread worker;
            private readonly byte[] body;
            private readonly string mode;
            private readonly string type;
            private Exception error;
            private int hits;
            public volatile string PageMarkup;
            public volatile bool Cacheable;
            public readonly Uri U;
            public Uri Page { get { return new Uri(U, "/page"); } }
            public int Hits { get { return Interlocked.CompareExchange(ref hits, 0, 0); } }

            public InputServer(string mode, byte[] data, string type = "text/html")
            {
                this.mode = mode;
                this.type = type;
                body = data;
                if (mode == "gzip" || mode == "deflate")
                    using (var output = new MemoryStream())
                    {
                        using (Stream zip = mode == "gzip" ? (Stream)new GZipStream(output, CompressionMode.Compress, true) : new DeflateStream(output, CompressionMode.Compress, true))
                            zip.Write(data, 0, data.Length);
                        body = output.ToArray();
                    }
                listener.Start();
                U = new Uri("http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/input");
                worker = new Thread(Serve) { IsBackground = true };
                worker.Start();
            }

            private void Serve()
            {
                try
                {
                    while (!stop.WaitOne(0))
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    try
                    {
                        stream.ReadTimeout = 2000;
                        stream.WriteTimeout = 2000;
                        var header = new StringBuilder();
                        var end = 0;
                        while (header.Length < 16384 && end != 0x0d0a0d0a)
                        {
                            var value = stream.ReadByte();
                            if (value < 0) throw new IOException("Incomplete input fixture request");
                            header.Append((char)value);
                            end = (end << 8) | value;
                        }
                        if (end != 0x0d0a0d0a) throw new InvalidDataException("Input fixture headers too large");
                        var text = header.ToString();
                        if (text.IndexOf("100-continue", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var interim = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");
                            stream.Write(interim, 0, interim.Length);
                        }
                        foreach (var line in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
                            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            {
                                int length;
                                if (!Int32.TryParse(line.Substring(15).Trim(), out length) || length < 0 || length > 10 * 1024 * 1024)
                                    throw new InvalidDataException("Input fixture request length");
                                var buffer = new byte[32768];
                                while (length > 0)
                                {
                                    var count = stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                                    if (count == 0) throw new IOException("Input fixture request ended early");
                                    length -= count;
                                }
                            }
                        if (text.StartsWith("GET /page ", StringComparison.Ordinal) && PageMarkup != null)
                        {
                            var page = Encoding.UTF8.GetBytes(PageMarkup);
                            Header(stream, page.Length, "text/html", "", false);
                            stream.Write(page, 0, page.Length);
                            continue;
                        }
                        Interlocked.Increment(ref hits);
                        if (mode == "headers") { stop.WaitOne(); continue; }
                        Header(stream, mode == "overflow" ? -1 : body.Length, type, mode == "gzip" || mode == "deflate" ? mode : "", Cacheable);
                        if (mode == "declared") { stop.WaitOne(); continue; }
                        if (mode == "trickle")
                        {
                            var at = 0;
                            while (!stop.WaitOne(80) && at < body.Length) stream.WriteByte(body[at++]);
                            continue;
                        }
                        for (var at = 0; at < body.Length; at += 32768)
                            stream.Write(body, at, Math.Min(32768, body.Length - at));
                        if (mode == "overflow") stop.WaitOne();
                    }
                    catch (IOException) { }
                }
                catch (Exception failure) { if (!stop.WaitOne(0)) error = failure; }
            }

            private static void Header(Stream stream, int length, string type, string encoding, bool cache)
            {
                var text = "HTTP/1.1 200 OK\r\nContent-Type: " + type + "; charset=utf-8\r\nCache-Control: " + (cache ? "max-age=60" : "no-store") + "\r\nConnection: close\r\n";
                if (length >= 0) text += "Content-Length: " + length + "\r\n";
                if (encoding.Length > 0) text += "Content-Encoding: " + encoding + "\r\n";
                var bytes = Encoding.ASCII.GetBytes(text + "\r\n");
                stream.Write(bytes, 0, bytes.Length);
            }

            public void Dispose()
            {
                stop.Set();
                listener.Stop();
                Ok(worker.Join(5000), "input fixture worker stopped");
                stop.Dispose();
                if (error != null) throw new InvalidOperationException("Input fixture failed", error);
            }
        }

        private static void CheckPictureLimits(string root)
        {
            var preflight = typeof(V).Assembly.GetType("Lea.Win.Pi", false);
            var inspect = preflight == null ? null : preflight.GetMethod("Rd", BindingFlags.Static | BindingFlags.Public);
            Ok(inspect != null,
                "bounded picture preflight is available before native decoding");
            var isolated = typeof(V).Assembly.GetType("Lea.Win.Di", false);
            var job = typeof(V).Assembly.GetType("Lea.Win.Jb", false);
            Ok(isolated != null && typeof(V).GetProperty("Ii") != null && typeof(V).GetProperty("Ie") != null &&
                typeof(V).GetProperty("Iw") != null && typeof(V).GetProperty("Ik") != null && job != null &&
                typeof(V).GetProperty("Ij") != null && typeof(V).GetProperty("Ih") != null && typeof(V).GetProperty("Ipm") != null &&
                typeof(V).GetProperty("Irt") != null && typeof(V).GetProperty("Ili") != null && typeof(V).GetProperty("Isp") != null &&
                typeof(V).GetProperty("Ihl") != null && typeof(V).GetProperty("Ias") != null && typeof(V).GetProperty("Icp") != null &&
                typeof(V).GetProperty("Icr") != null && typeof(V).GetProperty("Img") != null && typeof(V).GetProperty("Ipk") != null,
                "job-bounded picture decoder and compact worker aliases are available");
            using (var aliases = new V())
                Ok(aliases.Ip == 8 * 1024 * 1024 && aliases.Im == 128L * 1024 * 1024 && aliases.It == 0 &&
                    aliases.In == 0 && aliases.If == 0 && aliases.Ii && File.Exists(aliases.Ie) && aliases.Iw == 0 && aliases.Ik == 0 &&
                    !aliases.Ij && aliases.Ih == 192L * 1024 * 1024 && aliases.Ipm == 0 && !aliases.Irt && !aliases.Ili &&
                    !aliases.Isp && !aliases.Ihl && !aliases.Ias && !aliases.Icp && aliases.Icr == 2500 &&
                    aliases.Img == 0 && aliases.Ipk == 0,
                    "compact picture preflight, process-memory, decoder, and job aliases");
            var formats = new Dictionary<string, byte[]>
            {
                { "p", PictureBytes(System.Drawing.Imaging.ImageFormat.Png) },
                { "j", PictureBytes(System.Drawing.Imaging.ImageFormat.Jpeg) },
                { "g", PictureBytes(System.Drawing.Imaging.ImageFormat.Gif) },
                { "b", PictureBytes(System.Drawing.Imaging.ImageFormat.Bmp) },
                { "i", File.ReadAllBytes(Path.Combine(root, "assets", "lea.ico")) }
            };
            var formatWorkers = new HashSet<int>();
            var securityChecked = false;
            foreach (var format in formats)
            {
                var info = inspect.Invoke(null, new object[] { format.Value });
                Eq(format.Key, (string)preflight.GetField("K").GetValue(info), "picture preflight format " + format.Key);
                Ok((int)preflight.GetField("W").GetValue(info) > 0 && (int)preflight.GetField("H").GetValue(info) > 0 &&
                    (long)preflight.GetField("R").GetValue(info) <= 64L * 1024 * 1024, "bounded picture preflight dimensions " + format.Key);
                using (var server = new PictureServer("plain", format.Value))
                using (var view = new V())
                {
                    var work = view.GoA(server.U);
                    WaitFor(() => work.IsCompleted, "native format decode " + format.Key);
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 1 && view.If == 0 && view.In == 1 && view.It == view.Ib && view.Ib > 0 &&
                        view.Ii && view.Iw > 0 && view.Iw != System.Diagnostics.Process.GetCurrentProcess().Id && view.Ik == 0 &&
                        view.Ij && view.Ih == 192L * 1024 * 1024 && view.Ipm > 0 && view.Ipm <= view.Ih && view.Irt && view.Ili &&
                        view.Isp && view.Ihl && view.Ias && view.Icp && view.Icr == 2500 && (view.Img & 3) == 3 &&
                        view.Ipk > 0 && view.Ipk <= view.Ih,
                        "preflighted restricted native format accounting " + format.Key + ": Ic=" + view.Ic + "; If=" + view.If +
                        "; In=" + view.In + "; Iw=" + view.Iw + "; Ij=" + view.Ij + "; Irt=" + view.Irt + "; Ili=" + view.Ili +
                        "; Isp=" + view.Isp + "; Ihl=" + view.Ihl + "; Ias=" + view.Ias + "; Icp=" + view.Icp +
                        "; Icr=" + view.Icr + "; Img=" + view.Img + "; Ipm=" + view.Ipm + "; Ipk=" + view.Ipk);
                    formatWorkers.Add(view.Iw);
                    if (!securityChecked)
                    {
                        CheckImageWorkerSecurity(view.Iw, view.Img, view.Icr);
                        securityChecked = true;
                    }
                }
                using (var released = new V()) Eq(0L, released.It, "format disposal releases process image bytes " + format.Key);
            }
            Eq(1, formatWorkers.Count, "one job-bounded helper is reused across native codecs");
            foreach (var hostile in new[]
            {
                PicturePngDimensions(8193, 1),
                PicturePngDimensions(4096, 4096),
                new byte[32],
                PictureLongJpegHeader()
            })
            {
                Ok(PreflightRejected(inspect, hostile), "hostile image rejected by preflight");
                using (var server = new PictureServer("plain", hostile))
                using (var view = new V())
                {
                    var work = view.GoA(server.U);
                    WaitFor(() => work.IsCompleted, "hostile image integration");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 0 && view.If == 1 && view.In == 0 && view.Iw == 0 && view.Ik == 0 && !view.Ij && view.Ipm == 0 &&
                        !view.Irt && !view.Ili && !view.Isp && !view.Ihl && !view.Ias && !view.Icp &&
                        view.Img == 0 && view.Ipk == 0 && view.It == 0 && view.Le == null,
                        "hostile image never reaches native decoder");
                }
            }
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.SeaGreen)))
            using (var view = new V { Ie = Path.Combine(root, "missing-Lea.Image.exe") })
            {
                Ok(!view.Ii, "missing decoder is reported before navigation");
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "missing image worker");
                work.GetAwaiter().GetResult();
                Ok(view.Ic == 0 && view.If == 1 && view.In == 0 && view.Iw == 0 && view.Ik == 0 && !view.Ij && view.Ipm == 0 &&
                    !view.Irt && !view.Ili && !view.Isp && !view.Ihl && !view.Ias && !view.Icp &&
                    view.Img == 0 && view.Ipk == 0 && view.It == 0 && view.Le == null,
                    "missing decoder is a bounded nonfatal image failure");
            }
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.SeaGreen)))
            using (var view = new V { Ie = Assembly.GetExecutingAssembly().Location, Tm = 1000 })
            {
                typeof(V).GetField("picWorkerArgs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, "--image-fake");
                var clock = Stopwatch.StartNew();
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "unhardened image worker rejection");
                work.GetAwaiter().GetResult();
                Ok(clock.ElapsedMilliseconds < 2000 && view.Ic == 0 && view.If == 1 && view.In == 0 && view.Iw == 0 &&
                    !view.Ij && !view.Irt && !view.Ili && !view.Isp && !view.Ihl && !view.Ias && !view.Icp &&
                    view.Img == 0 && view.Ipm == 0 && view.Ipk == 0,
                    "invalid hardening handshake rejects the worker before image admission: ms=" + clock.ElapsedMilliseconds +
                    "; Ic=" + view.Ic + "; If=" + view.If + "; In=" + view.In + "; Iw=" + view.Iw + "; Ij=" + view.Ij +
                    "; Irt=" + view.Irt + "; Ili=" + view.Ili + "; Isp=" + view.Isp + "; Ihl=" + view.Ihl +
                    "; Ias=" + view.Ias + "; Icp=" + view.Icp + "; Icr=" + view.Icr +
                    "; Img=" + view.Img + "; Ipm=" + view.Ipm + "; Ipk=" + view.Ipk);
            }
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.SeaGreen)))
            using (var view = new V { Tm = 300 })
            {
                typeof(V).GetField("picWorkerArgs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, "--pipe --delay=2000");
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted && view.It == 0, "stalled native decoder termination");
                work.GetAwaiter().GetResult();
                Ok(clock.ElapsedMilliseconds < 1800 && view.Ic == 0 && view.If == 1 && view.In == 1 &&
                    view.Iw > 0 && view.Iw != System.Diagnostics.Process.GetCurrentProcess().Id && view.Ik == 1 && view.Ij &&
                    view.Irt && view.Ili && view.Isp && view.Ihl && view.Ias && view.Icp && view.Icr == 2500 &&
                    (view.Img & 3) == 3 && view.Ipm == 0 && view.Ipk == 0 &&
                    view.It == 0 && view.Le == null,
                    "decoder deadline kills the helper and releases its reservation: ms=" + clock.ElapsedMilliseconds +
                    "; Ic=" + view.Ic + "; If=" + view.If + "; In=" + view.In + "; Iw=" + view.Iw + "; Ik=" + view.Ik +
                    "; Ij=" + view.Ij + "; Irt=" + view.Irt + "; Ili=" + view.Ili + "; Isp=" + view.Isp +
                    "; Ihl=" + view.Ihl + "; Ias=" + view.Ias + "; Icp=" + view.Icp + "; Icr=" + view.Icr +
                    "; Img=" + view.Img + "; Ipm=" + view.Ipm + "; Ipk=" + view.Ipk + "; It=" + view.It);
            }
            var restartWorker = 0;
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.MediumSeaGreen)))
            using (var view = new V())
            {
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "decoder restart after termination");
                work.GetAwaiter().GetResult();
                Ok(view.Ic == 1 && view.If == 0 && view.In == 1 && view.Iw > 0 && view.Ik == 0 && view.Ij && view.Irt &&
                    view.Ili && view.Isp && view.Ihl && view.Ias && view.Icp && view.Icr == 2500 &&
                    (view.Img & 3) == 3 && view.Ipm > 0 && view.Ipm <= view.Ih &&
                    view.Ipk > 0 && view.Ipk <= view.Ih && view.It == view.Ib,
                    "decoder restarts cleanly after a killed helper");
                restartWorker = view.Iw;
            }
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.MediumSeaGreen)))
            using (var view = new V())
            {
                var oldLimit = view.Ih;
                try
                {
                    view.Ih = 1;
                    Eq(64L * 1024 * 1024, view.Ih, "image worker hard-limit minimum");
                    view.Ih = Int64.MaxValue;
                    Eq(512L * 1024 * 1024, view.Ih, "image worker hard-limit maximum");
                    view.Ih = 160L * 1024 * 1024;
                    var work = view.GoA(server.U);
                    WaitFor(() => work.IsCompleted, "decoder hard-limit restart");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 1 && view.If == 0 && view.Ij && view.Ih == 160L * 1024 * 1024 && view.Irt && view.Ili &&
                        view.Isp && view.Ihl && view.Ias && view.Icp && view.Icr == 2500 &&
                        (view.Img & 3) == 3 && view.Ipm > 0 && view.Ipm <= view.Ih &&
                        view.Ipk > 0 && view.Ipk <= view.Ih && view.Iw > 0 && view.Iw != restartWorker,
                        "changed hard limit starts a newly bounded helper");
                }
                finally { view.Ih = oldLimit; }
            }
            using (var server = new PictureServer("plain", PicturePng(3, 2, Color.MediumSeaGreen)))
            using (var view = new V())
            {
                var oldRate = view.Icr;
                try
                {
                    view.Icr = 1;
                    Eq(100, view.Icr, "image worker CPU-rate minimum");
                    view.Icr = Int32.MaxValue;
                    Eq(10000, view.Icr, "image worker CPU-rate maximum");
                    view.Icr = 2000;
                    var work = view.GoA(server.U);
                    WaitFor(() => work.IsCompleted, "decoder CPU-rate restart");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 1 && view.If == 0 && view.Ij && view.Ias && view.Icp && view.Icr == 2000 &&
                        view.Iw > 0 && view.Iw != restartWorker && view.Ipm > 0 && view.Ipk > 0,
                        "changed CPU hard cap starts a newly rate-bounded helper");
                    CheckImageWorkerSecurity(view.Iw, view.Img, view.Icr);
                }
                finally { view.Icr = oldRate; }
            }
            CheckImageJobExit(root);
            using (var gate = new ManualResetEvent(false))
            using (var server = new CompressedServer { ImageGate = gate })
            using (var view = new V { Tm = 300 })
            {
                var work = view.GoA(new Uri(server.U, "?image-page=valid"));
                try
                {
                    WaitFor(() => server.Hits == 2, "stalled image request");
                    var until = DateTime.UtcNow.AddMilliseconds(900);
                    while (!work.IsCompleted && DateTime.UtcNow < until) { Application.DoEvents(); Thread.Sleep(5); }
                    Ok(work.IsCompleted && view.Cn && !view.Busy && !gate.WaitOne(0), "picture pass respects Tm while response headers are held");
                    Ok(work.GetAwaiter().GetResult() == view.At && view.Le == null, "image timeout does not fail the document");
                }
                finally
                {
                    view.St();
                    WaitFor(() => work.IsCompleted, "stalled image cleanup");
                }
            }
            foreach (var mode in new[] { "body", "declared", "overflow", "zip-limit", "truncated", "status", "gzip", "deflate", "case" })
            using (var server = new PictureServer(mode, PicturePng(3, 2, Color.SeaGreen)))
            using (var view = new V { Size = new Size(640, 480), Tm = mode == "body" ? 300 : 5000 })
            {
                var ready = 0;
                view.Done += delegate { ready++; };
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "picture resource mode " + mode);
                work.GetAwaiter().GetResult();
                if (mode == "declared" || mode == "overflow")
                    Ok(clock.ElapsedMilliseconds < 2000, "oversized image rejects before the deadline or end of body " + mode);
                if (mode == "body") Ok(clock.ElapsedMilliseconds < 800 && server.Pictures == 1, "three pictures share one Tm budget");
                var loaded = mode == "gzip" || mode == "deflate" || mode == "case";
                Eq(loaded ? mode == "case" ? 2 : 1 : 0, view.Ic, "decoded picture count " + mode);
                Eq(loaded ? mode == "case" ? 48L : 24L : 0L, view.Ib, "decoded picture accounting " + mode);
                Eq(loaded ? 0 : mode == "body" ? 3 : 1, view.If, "failed picture accounting " + mode);
                Eq(loaded ? mode == "case" ? 2 : 1 : 0, view.In, "native decode admission " + mode);
                Ok(view.Cn && !view.Busy && view.Le == null && ready == 1 && view.At.Fs[0].V == "Retained image field",
                    "nonfatal picture completion preserves document " + mode);
                var requests = server.Pictures;
                for (var pass = 0; pass < 3; pass++) { Paint(view); Application.DoEvents(); }
                Eq(requests, server.Pictures, "failed picture does not refetch on paint " + mode);
                if (mode == "case")
                {
                    var images = (Dictionary<string, Image>)typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                    Ok(images.ContainsKey(new Uri(server.U, "/pic").AbsoluteUri) && images.ContainsKey(new Uri(server.U, "/PIC").AbsoluteUri),
                        "image URL path case is preserved");
                }
                if (mode == "truncated")
                {
                    server.Recovered = true;
                    work = view.RlA();
                    WaitFor(() => work.IsCompleted, "failed image retry on Reload");
                    work.GetAwaiter().GetResult();
                    Ok(server.Pictures == 2 && view.Ic == 1 && view.Ib == 24, "Reload retries a failed image without a new URL");
                }
            }
            using (var server = new PictureServer("budget", PicturePng(2048, 2048, Color.SeaGreen)))
            using (var view = new V { Size = new Size(640, 480) })
            {
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "bitmap cache budget");
                work.GetAwaiter().GetResult();
                Ok(view.Ic == 2 && view.Ib == 32L * 1024 * 1024 && server.Pictures == 3, "32 MiB cache retains only fitting bitmaps");
                Ok(view.If == 1 && view.In == 2 && view.It == view.Ib, "per-view preflight and process accounting");
                for (var pass = 0; pass < 3; pass++) { Paint(view); Application.DoEvents(); }
                Eq(3, server.Pictures, "full bitmap budget does not evict/refetch current images");
                view.Go(Path.Combine(pages, "second.html"));
                Ok(view.Ic == 0 && view.Ib == 0, "sync page replacement releases unreferenced bitmaps");
            }
            using (var firstServer = new PictureServer("budget", PicturePng(2048, 2048, Color.SeaGreen)))
            using (var secondServer = new PictureServer("plain", PicturePng(2048, 2048, Color.MediumSeaGreen)))
            using (var first = new V())
            using (var second = new V())
            {
                var oldLimit = first.Im;
                try
                {
                    first.Im = 64L * 1024 * 1024;
                    var work = first.GoA(firstServer.U);
                    WaitFor(() => work.IsCompleted, "shared image budget seed");
                    work.GetAwaiter().GetResult();
                    Ok(first.Ic == 2 && first.If == 1 && first.It == 32L * 1024 * 1024,
                        "shared image budget retains bounded first view");
                    work = second.GoA(secondServer.U);
                    WaitFor(() => work.IsCompleted, "shared image budget rejection");
                    work.GetAwaiter().GetResult();
                    Ok(second.Ic == 0 && second.If == 1 && second.In == 0 && second.It == first.It,
                        "shared image budget rejects before native decode");
                    first.Dispose();
                    Eq(0L, second.It, "disposing first view releases shared image bytes");
                    work = second.RlA();
                    WaitFor(() => work.IsCompleted, "shared image budget recovery");
                    work.GetAwaiter().GetResult();
                    Ok(second.Ic == 1 && second.If == 0 && second.In == 1 && second.It == second.Ib,
                        "released shared image budget is reusable");
                }
                finally { second.Im = oldLimit; }
            }
            using (var released = new V()) Eq(0L, released.It, "all shared image reservations released");
            using (var server = new PictureServer("plain", PicturePng(8193, 1, Color.SeaGreen)))
            using (var view = new V())
            {
                var work = view.GoA(server.U);
                WaitFor(() => work.IsCompleted, "bitmap dimension limit");
                work.GetAwaiter().GetResult();
                Ok(view.Ic == 0 && view.Ib == 0 && view.Le == null, "oversized dimensions do not enter retained bitmap cache");
            }
            foreach (var dispose in new[] { false, true })
            using (var server = new PictureServer("body", PicturePng(3, 2, Color.SeaGreen)))
            using (var good = new PictureServer("plain", PicturePng(3, 2, Color.SeaGreen)))
            using (var view = new V())
            {
                var ready = 0;
                view.Done += delegate { ready++; };
                var work = view.GoA(server.U);
                WaitFor(() => server.Pictures == 1, "cancellable picture body");
                if (dispose) view.Dispose(); else view.St();
                WaitFor(() => work.IsCompleted, "cancelled picture body completion");
                work.GetAwaiter().GetResult();
                Ok(view.Ic == 0 && view.Ib == 0 && ready == (dispose ? 0 : 1), "cancelled picture does not repopulate cache or publish after disposal");
                if (!dispose)
                {
                    work = view.GoA(good.U);
                    WaitFor(() => work.IsCompleted, "navigation after stopped picture");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 1 && view.Ib == 24 && view.Cn, "stopped image stream does not block successor");
                }
            }
            var copy = typeof(V).GetMethod("PictureBytes", BindingFlags.Instance | BindingFlags.NonPublic);
            using (var view = new V())
            using (var bytes = new MemoryStream(new byte[4 * 1024 * 1024]))
            {
                var work = (Task<byte[]>)copy.Invoke(view, new object[] { bytes, bytes.Length, Task.Delay(5000) });
                WaitFor(() => work.IsCompleted, "exact picture byte ceiling");
                Eq(4 * 1024 * 1024, work.GetAwaiter().GetResult().Length, "exact picture byte ceiling accepted");
            }
            using (var view = new V())
            using (var bytes = new MemoryStream(new byte[4 * 1024 * 1024 + 32768]))
            {
                var work = (Task<byte[]>)copy.Invoke(view, new object[] { bytes, -1L, Task.Delay(5000) });
                WaitFor(() => work.IsCompleted, "unknown length picture ceiling");
                var rejected = false;
                try { work.GetAwaiter().GetResult(); }
                catch (InvalidDataException) { rejected = true; }
                Ok(rejected && bytes.Position == 4 * 1024 * 1024 + 1, "unknown-length reads stop at exactly one byte over the limit");
            }
            using (var timer = new CancellationTokenSource())
            using (var view = new V())
            using (var stream = new MemoryStream(new byte[1]))
            {
                typeof(V).GetField("picBudget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, timer);
                var opening = new TaskCompletionSource<Stream>();
                var wait = typeof(V).GetMethod("PictureWait", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(Stream));
                var work = (Task<Stream>)wait.Invoke(view, new object[] { opening.Task, Task.Delay(5000, timer.Token) });
                view.St();
                WaitFor(() => work.IsCompleted, "Stop interrupts an opening resource wait");
                var stopped = false;
                try { work.GetAwaiter().GetResult(); }
                catch (TimeoutException) { stopped = true; }
                Ok(stopped && !opening.Task.IsCompleted, "Stop does not await a late platform open");
                opening.SetResult(stream);
                WaitFor(() => !stream.CanRead, "late resource is disposed");
            }
            var picturePath = Path.Combine(Path.GetTempPath(), "lea-picture-origin-" + Guid.NewGuid().ToString("N") + ".png");
            var pagePath = picturePath + ".html";
            try
            {
                File.WriteAllBytes(picturePath, PicturePng(3, 2, Color.SeaGreen));
                var address = new Uri(picturePath).AbsoluteUri;
                File.WriteAllText(pagePath, "<html><body><img src='" + address + "'></body></html>");
                using (var server = new PictureServer("plain", PicturePng(3, 2, Color.SeaGreen)) { ImageSource = address })
                using (var view = new V())
                {
                    var work = view.GoA(pagePath);
                    WaitFor(() => work.IsCompleted, "allowed local picture");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 1 && view.Ib == 24, "local documents retain local picture support");
                    work = view.GoA(server.U);
                    WaitFor(() => work.IsCompleted, "remote picture origin check");
                    work.GetAwaiter().GetResult();
                    Ok(view.Ic == 0 && view.Ib == 0 && server.Pictures == 0, "remote pages cannot reuse or read a local picture");
                }
                File.WriteAllText(pagePath, "<html><body><img src='file://example.invalid/share/sample.png'></body></html>");
                var page = new E().Go(pagePath);
                var sources = (IList<Uri>)typeof(V).GetMethod("PicSources", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { page });
                Eq(0, sources.Count, "UNC pictures are rejected without opening a network share");
            }
            finally
            {
                if (File.Exists(pagePath)) File.Delete(pagePath);
                if (File.Exists(picturePath)) File.Delete(picturePath);
            }
            Console.WriteLine("Lea picture limit smoke passed.");
        }

        private static int ImageJobHost(string root)
        {
            var page = Path.Combine(Path.GetTempPath(), "lea-image-job-host-" + Process.GetCurrentProcess().Id + ".html");
            try
            {
                var source = Convert.ToBase64String(PicturePng(3, 2, Color.MediumSeaGreen));
                File.WriteAllText(page, "<html><body><img src='data:image/png;base64," + source + "'></body></html>");
                using (var view = new V { Tm = 60000 })
                {
                    typeof(V).GetField("picWorkerArgs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, "--pipe --delay=60000");
                    var work = view.GoA(page);
                    var end = DateTime.UtcNow.AddSeconds(10);
                    while (view.Iw == 0 && !work.IsCompleted && DateTime.UtcNow < end)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    if (view.Iw == 0 || !view.Ij || !view.Irt || !view.Ili || !view.Isp || !view.Ihl ||
                        !view.Ias || !view.Icp || view.Icr != 2500 || (view.Img & 3) != 3) return 2;
                    Console.WriteLine("worker=" + view.Iw);
                    Console.Out.Flush();
                    Thread.Sleep(Timeout.Infinite);
                }
                return 3;
            }
            finally { if (File.Exists(page)) File.Delete(page); }
        }

        private static int UnhardenedImageWorker()
        {
            using (var output = Console.OpenStandardOutput())
            using (var writer = new BinaryWriter(output, Encoding.UTF8))
            {
                writer.Write(0x3241454c);
                writer.Write(0);
                writer.Flush();
                Thread.Sleep(60000);
            }
            return 0;
        }

        private static void CheckImageWorkerSecurity(int processId, int guards, int cpuRate)
        {
            using (var process = Process.GetProcessById(processId))
            {
                IntPtr token;
                Ok(OpenProcessToken(process.Handle, 8, out token), "image worker token can be inspected");
                try
                {
                    var privileges = NativeTokenInfo(token, 3);
                    try
                    {
                        var count = Marshal.ReadInt32(privileges);
                        var enabled = 0;
                        for (var index = 0; index < count; index++)
                            if ((Marshal.ReadInt32(privileges, 4 + index * 12 + 8) & 2) != 0) enabled++;
                        Ok(enabled <= 1, "restricted worker token retains at most traversal privilege");
                    }
                    finally { Marshal.FreeHGlobal(privileges); }

                    var integrity = NativeTokenInfo(token, 25);
                    try
                    {
                        var sid = Marshal.PtrToStructure<NativeSidAttribute>(integrity).Sid;
                        var count = Marshal.ReadByte(GetSidSubAuthorityCount(sid));
                        Eq(4096, Marshal.ReadInt32(GetSidSubAuthority(sid, count - 1)), "worker token is low integrity");
                    }
                    finally { Marshal.FreeHGlobal(integrity); }
                }
                finally { CloseHandle(token); }

                bool inJob;
                Ok(IsProcessInJob(process.Handle, IntPtr.Zero, out inJob) && inJob, "worker is inside a Job after suspended launch");
                Eq(3U, Mitigation(process.Handle, 1) & 3U, "forced relocation and bottom-up startup ASLR");
                Ok((guards & 3) == 3, "required worker mitigation handshake bits");
                Eq(3U, Mitigation(process.Handle, 3) & 3U, "permanent strict-handle mitigation");
                Eq(1U, Mitigation(process.Handle, 6) & 1U, "legacy extension-point mitigation");
                if ((guards & 4) != 0) Eq(7U, Mitigation(process.Handle, 10) & 7U, "image-load mitigation");
                if ((guards & 8) != 0) Eq(1U, Mitigation(process.Handle, 13) & 1U, "child-process mitigation");
            }
            var decoder = typeof(V).Assembly.GetType("Lea.Win.Di", true);
            var worker = decoder.GetField("worker", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var job = worker.GetType().GetProperty("J").GetValue(worker, null);
            var handle = (IntPtr)job.GetType().GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(job);
            var cpu = new NativeCpuLimits();
            Ok(QueryInformationJobObject(handle, 15, ref cpu, (uint)Marshal.SizeOf(typeof(NativeCpuLimits)), IntPtr.Zero),
                "worker Job CPU policy can be inspected");
            Eq(5U, cpu.ControlFlags & 5U, "worker Job CPU hard-cap flags");
            Eq((uint)cpuRate, cpu.CpuRate, "worker Job CPU hard-cap rate");
        }

        private static IntPtr NativeTokenInfo(IntPtr token, int kind)
        {
            uint size;
            GetTokenInformation(token, kind, IntPtr.Zero, 0, out size);
            Ok(size > 0, "worker token data has a bounded size");
            var data = Marshal.AllocHGlobal((int)size);
            if (!GetTokenInformation(token, kind, data, size, out size))
            {
                Marshal.FreeHGlobal(data);
                throw new InvalidOperationException("Could not read worker token data: " + Marshal.GetLastWin32Error());
            }
            return data;
        }

        private static uint Mitigation(IntPtr process, int kind)
        {
            uint policy;
            Ok(GetProcessMitigationPolicy(process, kind, out policy, new UIntPtr(4)), "worker mitigation policy " + kind);
            return policy;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSidAttribute { public IntPtr Sid; public uint Attributes; }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeCpuLimits { public uint ControlFlags; public uint CpuRate; }

        private static void CheckImageJobExit(string root)
        {
            var executable = Assembly.GetExecutingAssembly().Location;
            var start = new ProcessStartInfo(executable, "\"" + root.Replace("\"", "") + "\" --job-host")
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process host = null;
            var worker = 0;
            try
            {
                host = Process.Start(start);
                Ok(host != null, "image job child host started");
                var line = host.StandardOutput.ReadLineAsync();
                var end = DateTime.UtcNow.AddSeconds(12);
                while (!line.IsCompleted && !host.HasExited && DateTime.UtcNow < end) Thread.Sleep(5);
                Ok(line.IsCompleted && !host.HasExited, "image job child host reported its worker");
                var value = line.Result ?? "";
                Ok(value.StartsWith("worker=") && Int32.TryParse(value.Substring(7), out worker) && WorkerExists(worker),
                    "image job child worker is active");
                host.Kill();
                Ok(host.WaitForExit(5000), "image job child host terminated");
                end = DateTime.UtcNow.AddSeconds(3);
                while (WorkerExists(worker) && DateTime.UtcNow < end) Thread.Sleep(10);
                Ok(!WorkerExists(worker), "job close terminates a worker blocked inside its delay");
            }
            finally
            {
                if (host != null && !host.HasExited) try { host.Kill(); host.WaitForExit(5000); } catch { }
                if (host != null)
                {
                    var page = Path.Combine(Path.GetTempPath(), "lea-image-job-host-" + host.Id + ".html");
                    host.Dispose();
                    if (File.Exists(page)) File.Delete(page);
                }
                if (WorkerExists(worker))
                    try { using (var child = Process.GetProcessById(worker)) { child.Kill(); child.WaitForExit(5000); } }
                    catch (ArgumentException) { }
                    catch (InvalidOperationException) { }
            }
        }

        private static bool WorkerExists(int process)
        {
            if (process < 1) return false;
            try
            {
                using (var worker = Process.GetProcessById(process))
                    return !worker.HasExited && worker.ProcessName.Equals("Lea.Image", StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException) { return false; }
            catch (InvalidOperationException) { return false; }
        }

        private static byte[] PicturePng(int width, int height, Color color)
        {
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var output = new MemoryStream())
            {
                graphics.Clear(color);
                bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                return output.ToArray();
            }
        }

        private static byte[] PictureBytes(System.Drawing.Imaging.ImageFormat format)
        {
            using (var bitmap = new Bitmap(3, 2))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var output = new MemoryStream())
            {
                graphics.Clear(Color.SeaGreen);
                bitmap.Save(output, format);
                return output.ToArray();
            }
        }

        private static byte[] PicturePngDimensions(int width, int height)
        {
            var data = PicturePng(3, 2, Color.SeaGreen);
            WriteBig(data, 16, width);
            WriteBig(data, 20, height);
            return data;
        }

        private static void WriteBig(byte[] data, int at, int value)
        {
            data[at] = (byte)(value >> 24);
            data[at + 1] = (byte)(value >> 16);
            data[at + 2] = (byte)(value >> 8);
            data[at + 3] = (byte)value;
        }

        private static byte[] PictureLongJpegHeader()
        {
            var data = new byte[270000];
            data[0] = 0xff;
            data[1] = 0xd8;
            var at = 2;
            while (at + 65537 < data.Length)
            {
                data[at] = 0xff;
                data[at + 1] = 0xe1;
                data[at + 2] = 0xff;
                data[at + 3] = 0xff;
                at += 65537;
            }
            return data;
        }

        private static bool PreflightRejected(MethodInfo inspect, byte[] data)
        {
            try { inspect.Invoke(null, new object[] { data }); }
            catch (TargetInvocationException error) { return error.InnerException is InvalidDataException; }
            return false;
        }

        private sealed class PictureServer : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly ManualResetEvent stop = new ManualResetEvent(false);
            private readonly object clientGate = new object();
            private readonly Thread worker;
            private readonly string mode;
            private readonly byte[] data;
            private Exception error;
            private int pictures;
            private TcpClient activeClient;
            public volatile bool Recovered;
            public volatile string ImageSource;
            public readonly Uri U;
            public int Pictures { get { return Interlocked.CompareExchange(ref pictures, 0, 0); } }

            public PictureServer(string mode, byte[] data)
            {
                this.mode = mode;
                this.data = data;
                listener.Start();
                U = new Uri("http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/page");
                worker = new Thread(Serve) { IsBackground = true };
                worker.Start();
            }

            private void Serve()
            {
                try
                {
                    while (!stop.WaitOne(0))
                    {
                        if (!listener.Pending()) { stop.WaitOne(5); continue; }
                        using (var client = listener.AcceptTcpClient())
                        using (var stream = client.GetStream())
                        {
                            lock (clientGate) activeClient = client;
                        stream.ReadTimeout = 2000;
                        stream.WriteTimeout = 2000;
                        var head = new StringBuilder();
                        var end = 0;
                        for (var i = 0; i < 16384 && end != 0x0d0a0d0a; i++)
                        {
                            var value = stream.ReadByte();
                            if (value < 0) throw new IOException("Incomplete picture fixture request");
                            head.Append((char)value);
                            end = (end << 8) | value;
                        }
                        if (end != 0x0d0a0d0a) throw new IOException("Picture fixture request header bound");
                        if (head.ToString().StartsWith("GET /page ", StringComparison.Ordinal))
                        {
                            var extra = mode == "budget" || mode == "body" ? "<img src='/pic2'><img src='/pic3'>" : mode == "case" ? "<img src='/PIC'>" : "";
                            var html = Encoding.UTF8.GetBytes("<html><head><title>Lea Picture Limits</title></head><body><input id='field' value='Retained image field'><img src='" + (ImageSource ?? "/pic") + "' width='24' height='16'>" + extra + "</body></html>");
                            Header(stream, html.Length, "text/html", "");
                            stream.Write(html, 0, html.Length);
                            continue;
                        }
                        Interlocked.Increment(ref pictures);
                        try
                        {
                            if (mode == "declared")
                            {
                                Header(stream, 4 * 1024 * 1024 + 1, "image/png", "");
                                stop.WaitOne();
                            }
                            else if (mode == "body")
                            {
                                Header(stream, data.Length, "image/png", "");
                                stream.Write(data, 0, 1);
                                stop.WaitOne();
                            }
                            else if (mode == "overflow")
                            {
                                Header(stream, -1, "image/png", "");
                                var buffer = new byte[32768];
                                for (var i = 0; i < 129; i++) stream.Write(buffer, 0, buffer.Length);
                                stop.WaitOne();
                            }
                            else if (mode == "status")
                            {
                                Header(stream, data.Length, "image/png", "", 503);
                                stream.Write(data, 0, data.Length);
                            }
                            else if (mode == "truncated" && !Recovered)
                            {
                                Header(stream, data.Length, "image/png", "");
                                stream.Write(data, 0, 1);
                            }
                            else
                            {
                                var body = data;
                                var encoding = "";
                                if (mode == "gzip" || mode == "deflate" || mode == "zip-limit")
                                {
                                    encoding = mode == "deflate" ? "deflate" : "gzip";
                                    using (var output = new MemoryStream())
                                    {
                                        using (Stream zip = encoding == "gzip" ? (Stream)new GZipStream(output, CompressionMode.Compress, true) : new DeflateStream(output, CompressionMode.Compress, true))
                                        {
                                            if (mode == "zip-limit")
                                            {
                                                var buffer = new byte[32768];
                                                for (var i = 0; i < 129; i++) zip.Write(buffer, 0, buffer.Length);
                                            }
                                            else zip.Write(body, 0, body.Length);
                                        }
                                        body = output.ToArray();
                                    }
                                }
                                Header(stream, body.Length, "image/png", encoding);
                                stream.Write(body, 0, body.Length);
                            }
                        }
                        catch (IOException) { } // Size limits, Stop, and timeouts intentionally disconnect picture requests.
                        }
                    }
                }
                catch (Exception failure) { if (!stop.WaitOne(0)) error = failure; }
            }

            private static void Header(Stream stream, int length, string type, string encoding, int status = 200)
            {
                var header = Encoding.ASCII.GetBytes("HTTP/1.1 " + status + " Test\r\nContent-Type: " + type + "\r\n" +
                    (length < 0 ? "" : "Content-Length: " + length + "\r\n") +
                    (encoding.Length == 0 ? "" : "Content-Encoding: " + encoding + "\r\n") + "Connection: close\r\n\r\n");
                stream.Write(header, 0, header.Length);
            }

            public void Dispose()
            {
                stop.Set();
                listener.Stop();
                lock (clientGate) if (activeClient != null) activeClient.Close();
                var ended = worker.Join(4000);
                Ok(ended && error == null, "picture fixture cleanup: " + error);
                stop.Dispose();
            }
        }

        private static void CheckLoadLifecycle(string root)
        {
            var source = Path.Combine(pages, "load-lifecycle.html");
            using (var headers = new ManualResetEvent(false))
            using (var pictures = new ManualResetEvent(false))
            using (var server = new CompressedServer { RequestGate = headers, ImageGate = pictures })
            using (var view = new V())
            {
                var scheduler = new StepScheduler();
                var done = 0;
                var failures = 0;
                view.Done += delegate { done++; Ok(!view.Busy && view.Cn, "Done sees ready state"); };
                view.Fe += delegate { failures++; };
                var context = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    var start = Task.Factory.StartNew(() => view.GoA(new Uri(server.U, "?image-page=valid")), CancellationToken.None, TaskCreationOptions.None, scheduler);
                    scheduler.Step();
                    var work = start.Result;
                    Ok(!work.IsCompleted, "gated load starts pending");
                    headers.Set();
                    var sawCommitted = false;
                    var checkedOverlap = false;
                    var deadline = DateTime.UtcNow.AddSeconds(5);
                    for (var step = 0; DateTime.UtcNow < deadline && !work.IsCompleted; step++)
                    {
                        if (checkedOverlap && server.Hits >= 2) pictures.Set();
                        if (view.At != null) sawCommitted = true;
                        Ok((view.Busy && !view.Cn) || done == 1, "view stays busy until UI completion: step=" + step + "; page=" + (view.At != null) + "; done=" + done);
                        if (!checkedOverlap && view.At != null && done == 0)
                        {
                            var retained = view.At;
                            foreach (var action in new Action[] {
                                () => view.Go(source), () => view.Go(new Uri(source)), () => view.Bk(), () => view.Fw(), () => view.Rl(),
                                () => view.Fg("#target"), () => view.Po(new Uri("http://127.0.0.1:1/"), "blocked"),
                                () => view.Pb(new Uri("http://127.0.0.1:1/"), new byte[0], "text/plain") })
                                LoadBusyRejected(action);
                            foreach (var action in new Func<Task<P>>[] {
                                () => view.GoA(source), () => view.GoA(new Uri(source)), () => view.RlA(),
                                () => view.PoA(new Uri("http://127.0.0.1:1/"), "blocked"),
                                () => view.PbA(new Uri("http://127.0.0.1:1/"), new byte[0], "text/plain") })
                            {
                                var overlapping = action();
                                Ok(overlapping.IsCompleted, "overlapping load rejects promptly");
                                LoadBusyRejected(() => overlapping.GetAwaiter().GetResult());
                            }
                            var unused = Path.Combine(Path.GetTempPath(), "lea-busy-" + Guid.NewGuid().ToString("N"));
                            LoadBusyRejected(() => view.Dl(source, unused));
                            LoadBusyRejected(() => view.Dl(new Uri(source), unused));
                            LoadBusyRejected(() => view.DlA(source, unused).GetAwaiter().GetResult());
                            LoadBusyRejected(() => view.DlA(new Uri(source), unused).GetAwaiter().GetResult());
                            Ok(!File.Exists(unused), "rejected downloads do not create output");
                            var boundary = typeof(V).GetMethod("InputActivationA", BindingFlags.Instance | BindingFlags.NonPublic);
                            Func<Task<int>> activation = async () => { await view.GoA(source); return 1; };
                            Eq(1, ((Task<int>)boundary.Invoke(view, new object[] { activation })).GetAwaiter().GetResult(), "native input consumes busy rejection");
                            Ok(view.At == retained && view.Busy && view.Cursor == Cursors.WaitCursor && failures == 0 && !view.Cr,
                                "rejected overlap retains page, busy cursor, and failure state");
                            checkedOverlap = true;
                        }
                        SynchronizationContext.SetSynchronizationContext(null);
                        scheduler.Step();
                        Thread.Sleep(1);
                    }
                    Ok(work.IsCompleted && sawCommitted && checkedOverlap, "controlled load reached UI completion: state=" + work.Status + "; page=" + (view.At != null) + "; done=" + done + "; committed=" + sawCommitted + "; overlap=" + checkedOverlap);
                    work.GetAwaiter().GetResult();
                    Ok(!view.Busy && done == 1, "completed load reports ready exactly once");
                }
                finally { SynchronizationContext.SetSynchronizationContext(context); }
            }
            CheckLoadCallbacks(root);
            CheckDownloadLifecycle(source);
            CheckPictureHandoff();
            Console.WriteLine("Lea load lifecycle smoke passed.");
        }

        private static void LoadBusyRejected(Action action)
        {
            var rejected = false;
            try { action(); }
            catch (InvalidOperationException error) { rejected = error.Message.StartsWith("Lea is still loading", StringComparison.Ordinal); }
            Ok(rejected, "overlapping navigation has a predictable busy error");
        }

        private static void CheckLoadCallbacks(string root)
        {
            var source = Path.Combine(pages, "load-lifecycle.html");
            foreach (var sync in new[] { false, true })
            foreach (var fail in new[] { false, true })
            foreach (var held in new[] { false, true })
            using (var gate = new ManualResetEvent(false))
            using (var server = new CompressedServer { RequestGate = gate })
            using (var view = new V())
            {
                var scheduler = new StepScheduler();
                var context = SynchronizationContext.Current;
                Task<P> next = null;
                var ready = 0;
                var errors = 0;
                var requests = 0;
                var started = false;
                view.Gr += delegate { requests++; };
                view.Done += delegate
                {
                    ready++;
                    if (!fail && !started)
                    {
                        started = true;
                        var engine = (E)typeof(V).GetField("e", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                        engine.Js(view.At, "go('second.html')");
                        next = held ? view.GoA(server.U) : view.GoA(source);
                    }
                };
                view.Fe += delegate { errors++; Ok(!view.Busy, "failure callback follows view cleanup"); started = true; next = held ? view.GoA(server.U) : view.GoA(source); };
                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    var input = fail ? source + ".absent" : source;
                    var first = sync ? Task.Factory.StartNew(() => view.Go(input), CancellationToken.None, TaskCreationOptions.None, scheduler) :
                        Task.Factory.StartNew(() => view.GoA(input), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
                    scheduler.Step();
                    var sawHandoff = false;
                    var deadline = DateTime.UtcNow.AddSeconds(5);
                    while (DateTime.UtcNow < deadline && (!first.IsCompleted || next == null || !next.IsCompleted))
                    {
                        if (first.IsCompleted && next != null && !next.IsCompleted && view.Busy)
                        {
                            sawHandoff = true;
                            Ok(view.Cursor == Cursors.WaitCursor && view.Le == null, "old completion does not clear the new load cursor or failure state");
                            if (held) gate.Set();
                        }
                        SynchronizationContext.SetSynchronizationContext(null);
                        scheduler.Step();
                        Thread.Sleep(1);
                    }
                    // Local completion may be inline; the gated variant must expose pending ownership.
                    Ok(first.IsCompleted && next != null && next.IsCompleted && (!held || sawHandoff),
                        "callback navigation completes both owned operations: sync=" + sync + "; fail=" + fail + "; held=" + held + "; first=" + first.Status +
                        "; next=" + (next == null ? "none" : next.Status.ToString()) + "; handoff=" + sawHandoff + "; busy=" + view.Busy + "; ready=" + ready + "; errors=" + errors);
                    if (fail) Fails(() => first.GetAwaiter().GetResult(), "first callback request failure");
                    else first.GetAwaiter().GetResult();
                    next.GetAwaiter().GetResult();
                    Ok(!view.Busy && view.Le == null && ready == (fail ? 1 : 2) && errors == (fail ? 1 : 0), "callback completion counts and final ready state");
                    Eq(0, requests, "old page does not dispatch go after callback navigation");
                }
                finally { SynchronizationContext.SetSynchronizationContext(context); }
            }
        }

        private static void CheckDownloadLifecycle(string source)
        {
            foreach (var remote in new[] { false, true })
            foreach (var uri in new[] { false, true })
            using (var gate = new ManualResetEvent(false))
            using (var server = new CompressedServer { RequestGate = gate })
            using (var view = new V())
            {
                var scheduler = new StepScheduler();
                var context = SynchronizationContext.Current;
                var output = Path.Combine(Path.GetTempPath(), "lea-download-life-" + Guid.NewGuid().ToString("N"));
                Task<D> work = null;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                    var input = remote ? server.U.AbsoluteUri : source;
                    work = Task.Factory.StartNew(() => uri ? view.DlA(new Uri(input), output) : view.DlA(input, output),
                        CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
                    scheduler.Step();
                    var observed = false;
                    var deadline = DateTime.UtcNow.AddSeconds(5);
                    while (!work.IsCompleted && DateTime.UtcNow < deadline)
                    {
                        observed = true;
                        Ok(view.Busy && !view.Cn, "download retains view ownership through completion");
                        LoadBusyRejected(() => view.Go(source));
                        if (remote) gate.Set();
                        SynchronizationContext.SetSynchronizationContext(null);
                        scheduler.Step();
                        Thread.Sleep(1);
                    }
                    Ok((!remote || observed) && work.IsCompleted, "controlled download completes: " + work.Status + "; network=" + remote);
                    Ok(work.GetAwaiter().GetResult() == view.Ld && !view.Busy && view.Cn && File.Exists(output), "download result and admission restored");
                    view.Dispose();
                    Ok(!view.Cn, "disposed view rejects admission");
                }
                finally
                {
                    view.St();
                    var deadline = DateTime.UtcNow.AddSeconds(5);
                    while (work != null && !work.IsCompleted && DateTime.UtcNow < deadline)
                    {
                        SynchronizationContext.SetSynchronizationContext(null);
                        scheduler.Step();
                        Thread.Sleep(1);
                    }
                    if (work != null && work.IsFaulted) { var ignored = work.Exception; }
                    SynchronizationContext.SetSynchronizationContext(context);
                    if (File.Exists(output)) File.Delete(output);
                }
            }
        }

        private static void CheckPictureHandoff()
        {
            using (var gate = new ManualResetEvent(false))
            using (var first = new CompressedServer { ImageGate = gate })
            using (var second = new CompressedServer())
            using (var view = new V { Size = new Size(640, 480) })
            {
                view.Go(new Uri(first.U, "?image-page=1"));
                Paint(view);
                WaitFor(() => first.Hits == 2 && view.Busy, "held previous-page image");
                Ok(view.Cn, "background pictures do not prevent navigation");
                var work = view.GoA(new Uri(second.U, "?image-page=valid"));
                WaitFor(() => work.IsCompleted, "new page and image completion");
                work.GetAwaiter().GetResult();
                var images = (Dictionary<string, Image>)typeof(V).GetField("pics", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                Image loaded;
                Ok(images.TryGetValue(new Uri(second.U, "?pixel=1").AbsoluteUri, out loaded) && loaded != null,
                    "previous-page image work must not poison the new image cache");
                Ok(!gate.WaitOne(0) && !view.Busy && second.Hits == 2, "image handoff does not wait for the held old response or duplicate requests");
            }
        }

        private sealed class StepScheduler : TaskScheduler
        {
            private readonly Queue<Task> pending = new Queue<Task>();
            protected override void QueueTask(Task task) { lock (pending) pending.Enqueue(task); }
            protected override bool TryExecuteTaskInline(Task task, bool queued) { return false; }
            protected override IEnumerable<Task> GetScheduledTasks() { lock (pending) return pending.ToArray(); }
            public void Step()
            {
                Task next;
                lock (pending) { if (pending.Count == 0) return; next = pending.Dequeue(); }
                TryExecuteTask(next);
            }
        }

        private sealed class StepContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> pending = new Queue<Tuple<SendOrPostCallback, object>>();
            public override void Post(SendOrPostCallback callback, object value)
            {
                lock (pending) pending.Enqueue(Tuple.Create(callback, value));
            }
            public void Step()
            {
                Tuple<SendOrPostCallback, object> next;
                lock (pending) { if (pending.Count == 0) return; next = pending.Dequeue(); }
                var before = Current;
                try { SetSynchronizationContext(this); next.Item1(next.Item2); }
                finally { SetSynchronizationContext(before); }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool OpenClipboard(IntPtr owner);

        [System.Runtime.InteropServices.DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool CloseClipboard();

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
        private static extern IntPtr NativeEdit(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTokenInformation(IntPtr token, int kind, IntPtr data, uint length, out uint needed);

        [DllImport("advapi32.dll")]
        private static extern IntPtr GetSidSubAuthorityCount(IntPtr sid);

        [DllImport("advapi32.dll")]
        private static extern IntPtr GetSidSubAuthority(IntPtr sid, int index);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessMitigationPolicy(IntPtr process, int kind, out uint policy, UIntPtr length);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsProcessInJob(IntPtr process, IntPtr job, out bool result);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryInformationJobObject(IntPtr job, int kind, ref NativeCpuLimits limits,
            uint length, IntPtr returned);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr value);

        private static void CheckSheets(string root)
        {
            var path = Path.Combine(pages, "stylesheets.html");
            var engine = new E();
            var page = engine.Go(path);
            var html = page.H;
            var document = page.D;
            var requests = engine.Dg(page).Rs.Count;
            var field = FieldById(page, "field");
            field.V = "Retained field";
            engine.Js(page, "v('kept','yes')");
            var commands = page.Jc;
            var source = "body{--accent:#a13b18}#panel{color:var(--accent);width:180px}#inline{color:#ff0000}#important{color:#ff0000!important}@media(max-width:480px){#panel{width:130px}}";
            Eq(1, engine.Ss(page, "theme", source), "Ss accepts named CSS");
            Eq(1, page.Sn, "P.Sn sheet count");
            Eq(1, engine.Dg(page).Sn, "G.Sn sheet count");
            Eq("#a13b18", BlockById(page, "panel").S.F, "host sheet and custom property cascade");
            Eq("#123f8c", BlockById(page, "inline").S.F, "normal inline priority retained");
            Eq("#123f8c", BlockById(page, "important").S.F, "important inline priority retained");
            Eq(180, BlockById(page, "panel").S.W, "host sheet width");
            Ok(page.D == document && page.H == html && page.Jc == commands && page.Vs["kept"] == "yes", "Ss preserves DOM/source/command state");
            Ok(Object.ReferenceEquals(field, FieldById(page, "field")) && field.V == "Retained field", "Ss preserves field identity/value");
            Eq(requests, engine.Dg(page).Rs.Count, "Ss does not refetch document or stylesheet");
            Eq(1, engine.Ss(page, "theme", source), "same sheet is accepted idempotently");
            Eq(1, page.Sn, "same key does not accumulate sheets");
            engine.Vw = 320;
            Eq(130, BlockById(page, "panel").S.W, "injected media query activates");
            engine.Vw = 640;
            Eq(180, BlockById(page, "panel").S.W, "injected media query deactivates");
            engine.Ss(page, "later", "#panel{color:#075fa8}");
            engine.Ss(page, "theme", "#panel{color:#990000;width:190px}");
            Eq("#075fa8", BlockById(page, "panel").S.F, "replacement preserves sheet order");
            Eq(1, engine.Sd(page, "later"), "Sd removes one sheet");
            Eq("#990000", BlockById(page, "panel").S.F, "removal reveals earlier sheet");
            Eq(0, engine.Sd(page, "missing"), "missing sheet removal is inert");
            engine.Ss(page, "theme", ".tone{color:#ff0000}");
            Eq("#176b4d", BlockById(page, "panel").S.F, "authored specificity beats later weaker host rule");
            engine.Ss(page, "theme", "#inline{color:#880088!important}");
            Eq("#880088", BlockById(page, "inline").S.F, "host important beats normal inline");
            engine.Ss(page, "theme", "#panel:focus{color:#112233}#panel:hover{width:197px}");
            var panel = engine.Id(page, "panel");
            engine.Dy(page, panel, panel);
            Eq("#112233", BlockById(page, "panel").S.F, "dynamic focus uses injected rules");
            Eq(197, BlockById(page, "panel").S.W, "dynamic hover uses injected rules");
            engine.Dy(page, null, null);
            engine.Ss(page, "theme", "#field{display:none}");
            Ok(!page.Fs.Contains(field), "injected display none removes rendered field");
            engine.Sd(page, "theme");
            Ok(Object.ReferenceEquals(field, FieldById(page, "field")) && field.V == "Retained field", "removing hidden style restores retained field");
            engine.Je = false;
            Eq(1, engine.Ss(page, "theme", "#panel{width:177px}"), "host CSS works with page scripting disabled");
            engine.Go(Path.Combine(pages, "second.html"));
            Eq(0, engine.At.Sn, "new document does not inherit host sheets");
            engine.Bk();
            Ok(engine.At == page && page.Sn == 1 && BlockById(page, "panel").S.W == 177, "history retains named styles");
            Eq(0, engine.Ss(page, "theme", new string('x', 65537)), "oversized replacement rejected");
            Eq(177, BlockById(page, "panel").S.W, "rejected replacement is atomic");
            Eq(0, engine.Ss(page, "bad key", ""), "sheet key syntax is bounded");
            Eq(0, engine.Ss(page, new string('a', 33), ""), "sheet key length bounded");
            Eq(0, engine.Ss(page, "theme", null), "null sheet does not delete existing state");
            var foreignRejected = false;
            try { new E().Ss(page, "foreign", ""); } catch (ArgumentException) { foreignRejected = true; }
            Ok(foreignRejected, "stylesheet requires owning engine");
            engine.Sd(page, "theme");
            for (var index = 0; index < 8; index++) Eq(1, engine.Ss(page, "s" + index, ""), "sheet slot " + index);
            Eq(0, engine.Ss(page, "overflow", ""), "eight-sheet ceiling");
            for (var index = 0; index < 8; index++) engine.Sd(page, "s" + index);
            Eq(1, engine.Ss(page, "large1", new string(' ', 65536)), "first maximum source");
            Eq(1, engine.Ss(page, "large2", new string(' ', 65536)), "second maximum source");
            Eq(0, engine.Ss(page, "large3", " "), "total source ceiling");
            engine.Sd(page, "large1"); engine.Sd(page, "large2");
            var unicode = new string('\u00e9', 32768);
            Eq(1, engine.Ss(page, "unicode", unicode), "UTF-8 source byte boundary accepted");
            Eq(0, engine.Ss(page, "unicode", unicode + "\u00e9"), "UTF-8 source byte overflow rejected");
            Eq(0, engine.Ss(page, "unicode", "\ud800"), "invalid Unicode source rejected");
            Eq(1, page.Sn, "invalid replacement retains named sheet");
            engine.Ss(page, "ascii", new string(' ', 65536));
            Eq(0, engine.Ss(page, "extra", " "), "aggregate limit uses UTF-8 bytes");
            engine.Sd(page, "unicode"); engine.Sd(page, "ascii");
            var authored = engine.Dg(page).C;
            var rules = new StringBuilder();
            for (var index = authored; index < 1024; index++) rules.Append("#panel{width:178px}");
            Eq(1, engine.Ss(page, "rules", rules.ToString()), "global CSS rule boundary");
            Eq(1024, engine.Dg(page).C, "authored and host rules share one ceiling");
            Eq(0, engine.Ss(page, "extra", "#panel{width:199px}"), "no rule overflow evicts authored or earlier sheets");
            rules.Append("#panel{width:199px}");
            Eq(0, engine.Ss(page, "rules", rules.ToString()), "replacement rejects extra parsed rule");
            Eq(178, BlockById(page, "panel").S.W, "rule-bound rejection preserves style");
            engine.Sd(page, "rules");
            Eq(0, page.Sn, "all sheets removed");
            Eq(260, BlockById(page, "panel").S.W, "authored CSS restored");
            engine.Ss(page, "resource", "@import 'never.css';#panel{background-image:url('image.png')}");
            Eq(new Uri(page.Bu ?? page.U, "image.png"), BlockById(page, "panel").S.Bi.U, "host CSS uses document base URL");
            Eq(requests + 1, engine.Dg(page).Rs.Count, "host @import causes no fetch");

            using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(700, 700) })
            using (var view = new V())
            {
                form.Controls.Add(view);
                form.Show();
                view.Go(path);
                Paint(view);
                var nativeField = FieldById(view.At, "field");
                var start = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                start.Invoke(view, new object[] { nativeField });
                view.Pa("Native retained text");
                var editor = Editor(view);
                editor.Select(2, 4);
                var changed = 0;
                var edited = 0;
                view.Changed += delegate { changed++; };
                view.Ed += delegate { edited++; };
                Eq(1, view.Ss("host", "#field{width:320px;font-size:20px}#panel{background:#d7263d}@media(max-width:480px){#field{font-size:24px}}"), "V.Ss accepted");
                Eq(1, view.Sn, "V.Sn sheet count");
                Ok(editor.SelectionStart == 2 && editor.SelectionLength == 4 && view.Ce("un") && edited == 0 && changed == 1,
                    "live CSS preserves native selection/undo and emits no edit event");
                Eq(20F, editor.Font.Size, "live CSS updates native editor font");
                view.Ss("host", "#field{width:320px;font-size:20px}#panel{background:#d7263d}@media(max-width:480px){#field{font-size:24px}}");
                Eq(1, changed, "identical sheet avoids another layout notification");
                using (var rendered = Render(view))
                    Ok(HasColor(rendered, BlockRect(view, BlockById(view.At, "panel")), Color.FromArgb(215, 38, 61)), "injected CSS paints through Lea");
                form.Size = new Size(400, 700);
                Paint(view);
                Eq(24F, editor.Font.Size, "injected media CSS updates active editor font after resize");
                view.Ec("un");
                Eq("Original field", nativeField.V, "stylesheet/font changes preserve native undo");
                view.Ec("re");
                Eq("Native retained text", nativeField.V, "stylesheet/font changes preserve native redo");
                view.Ss("host", "#field{display:none}");
                Ok(!editor.Visible && !view.At.Fs.Contains(nativeField), "hidden native editor closes");
                view.Sd("host");
                Eq("Native retained text", FieldById(view.At, "field").V, "view hidden style removal retains data");
                view.Fg("#target");
                var scroll = view.Sy;
                view.Ss("paint", "#panel{background:#075fa8}");
                Eq(scroll, view.Sy, "paint-only sheet update does not jump to top");
                var done = 0;
                view.Done += delegate { done++; };
                view.Sd("paint");
                Eq(0, done, "stylesheet mutation does not report document load");
                view.Changed += delegate { view.Dispose(); };
                Eq(1, view.Ss("closing", "#panel{color:#112233}"), "stylesheet callback can dispose view");
                Ok(view.IsDisposed && view.Ss("late", "") == 0 && view.Sd("closing") == 0, "disposed stylesheet methods are inert");
            }
            Console.WriteLine("Lea stylesheet smoke passed: Ss/Sd/Sn, cascade, media, source/field/history retention, named replacement/removal, limits, native paint/font/undo, scroll, and callback disposal.");
        }

        private static void CheckPasteText(string root)
        {
            using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(720, 800) })
            using (var view = new V())
            using (var other = new TextBox { Dock = DockStyle.Bottom })
            {
                form.Controls.Add(view);
                form.Controls.Add(other);
                form.Show();
                var path = Path.Combine(pages, "editing.html");
                view.Go(path);
                Paint(view);
                var editor = Editor(view);
                var method = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                Action<Fi> start = field => method.Invoke(view, new object[] { field });
                var text = FieldById(view.At, "text");
                Eq(0, view.Pa("unfocused"), "Pa requires an active or retained editor");
                start(text);
                editor.Select(4, 5);
                other.Focus();
                Eq(1, view.Pa("engine"), "Pa reactivates a menu-retained field");
                Eq("Lea engine text", text.V, "Pa replaces only the retained selection");
                Eq(1, view.Ec("un"), "Pa native undo");
                Eq("Lea field text", text.V, "Pa undo value");
                Eq(1, view.Ec("re"), "Pa native redo");
                Eq("Lea engine text", text.V, "Pa redo value");
                Ok(view.Pa(null) == 0 && view.Pa("") == 0 && text.V == "Lea engine text", "empty supplied text is inert");
                editor.SelectAll();
                Eq(0, view.Pa("\r\nNext"), "single-line newline-only prefix preserves selection");
                Eq("Lea engine text", text.V, "empty normalized insertion preserves field");
                Eq(1, view.Pa("First\r\nSecond"), "single-line supplied text");
                Eq("First", text.V, "single-line paste stops at newline");
                editor.SelectAll();
                view.Pa("A\0B");
                Eq("A", text.V, "supplied text stops at NUL");
                editor.SelectAll();
                view.Pa(new string('x', 65536));
                Eq(32767, text.V.Length, "supplied text native field ceiling");
                view.Ec("un");
                Eq("A", text.V, "large paste preserves native undo");

                var limited = FieldById(view.At, "limited");
                start(limited);
                editor.Select(4, 0);
                view.Pa("abcdefghijk");
                Eq("1234abcd", limited.V, "Pa respects remaining maxlength capacity");
                editor.Select(editor.TextLength, 0);
                Eq(0, view.Pa("more"), "full field rejects insertion without deletion");
                Eq("1234abcd", limited.V, "full field retained");
                editor.Text = "1234567";
                editor.Select(7, 0);
                Eq(0, view.Pa("\ud83d\ude00Z"), "surrogate pair must fit completely");
                Eq("1234567", limited.V, "truncated surrogate does not replace selection");
                editor.Text = "123456";
                editor.Select(6, 0);
                view.Pa("\ud83d\ude00Z");
                Eq("123456\ud83d\ude00", limited.V, "complete surrogate fits at boundary");
                editor.Text = "12345678";
                editor.Select(2, 3);
                view.Pa("abcde");
                Eq("12abc678", limited.V, "selection replacement recovers capacity");

                var notes = FieldById(view.At, "notes");
                start(notes);
                view.Pa("One\r\nTwo");
                Eq("One\r\nTwo", notes.V, "Pa retains multiline breaks");
                notes.El.A["maxlength"] = "2";
                view.Js("ac('notes','bound-two')");
                start(notes);
                view.Pa("A\r\nB");
                Eq("A", notes.V, "maxlength does not split CRLF");
                notes.El.A["maxlength"] = "3";
                view.Js("ac('notes','bound-three')");
                start(notes);
                view.Pa("A\r\nB");
                Eq("A\r\n", notes.V, "complete CRLF fits at boundary");
                var readOnly = FieldById(view.At, "readonly");
                start(readOnly);
                Eq(0, view.Pa("blocked"), "Pa rejects readonly fields");
                Eq("Read only", readOnly.V, "readonly value retained");
                var secret = FieldById(view.At, "secret");
                start(secret);
                view.Pa("new secret");
                Eq("new secret", secret.V, "Pa permits password insertion");
                start(text);
                view.Js("ds('text')");
                Eq(0, view.Pa("blocked"), "Pa rejects disabled field");
                view.Js("en('text')");
                start(text);
                text.El.A["type"] = "checkbox";
                view.Js("ac('text','changed-kind')");
                Eq(0, view.Pa("blocked"), "Pa rejects a field changed to a non-text control");
                text.El.A["type"] = "text";
                view.Js("rc('text','changed-kind')");
                start(text);
                text.V = "external";
                Eq(0, view.Pa("blocked"), "Pa rejects stale native value");
                start(text);
                view.Js("hd('text')");
                Eq(0, view.Pa("blocked"), "Pa rejects hidden retained field");
                view.Go(path);
                Eq(0, view.Pa("blocked"), "navigation clears paste target");
                start(FieldById(view.At, "text"));
                view.Ed += delegate { view.Dispose(); };
                Eq(1, view.Pa("closing"), "paste callback can dispose view");
                Ok(view.IsDisposed && view.Pa("blocked") == 0, "disposed supplied paste is inert");
            }
            CheckPasteBeyondTabLimit();
            Console.WriteLine("Lea supplied paste smoke passed: Pa, retained selection, undo/redo, text/Unicode bounds, protected fields, late form fields, navigation, and callback disposal.");
        }

        private static void CheckPasteBeyondTabLimit()
        {
            var path = Path.Combine(Path.GetTempPath(), "lea-paste-fields-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                var html = new StringBuilder("<title>Paste beyond keyboard list</title>");
                for (var index = 0; index < 300; index++)
                    html.Append("<input id=f").Append(index).Append(index == 299 ? " style='position:fixed;top:60px;left:30px;width:300px'" : "").Append(" value=retained>");
                File.WriteAllText(path, html.ToString());
                using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(720, 800) })
                using (var view = new V())
                {
                    form.Controls.Add(view);
                    form.Show();
                    view.Go(path);
                    Paint(view);
                    Eq(256, view.Kn, "keyboard list retains its explicit bound");
                    var field = FieldById(view.At, "f299");
                    Click(view, BlockRect(view, BlockById(view.At, "f299")));
                    Ok(Editor(view).Visible, "mouse opens a field beyond the keyboard list");
                    Eq(1, view.Pa("Late field paste"), "active field beyond keyboard list accepts Pa");
                    Eq("Late field paste", field.V, "late field value");
                    Eq(1, view.Ec("un"), "late field native undo");
                    Eq("retained", field.V, "late field undo value");
                    view.Js("hd('f299')");
                    Eq(0, view.Pa("blocked"), "late hidden field remains protected");
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static void CheckClipboardContention(string root)
        {
            using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(720, 800) })
            using (var view = new V())
            using (var ready = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                form.Controls.Add(view);
                form.Show();
                view.Go(Path.Combine(pages, "editing.html"));
                Paint(view);
                var field = FieldById(view.At, "text");
                typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { field });
                var editor = Editor(view);
                var opened = false;
                var timedOut = false;
                var worker = new Thread(delegate()
                {
                    try
                    {
                        for (var attempt = 0; attempt < 100 && !opened; attempt++)
                        {
                            opened = OpenClipboard(IntPtr.Zero);
                            if (!opened) Thread.Sleep(10);
                        }
                        ready.Set();
                        if (opened) timedOut = !release.WaitOne(10000);
                    }
                    finally { if (opened) CloseClipboard(); }
                }) { IsBackground = true };
                worker.Start();
                try
                {
                    Ok(ready.WaitOne(2000) && opened, "clipboard contention fixture could not acquire lock");
                    foreach (var code in new[] { "cp", "cu", "pa" })
                    {
                        editor.Text = "Original";
                        editor.SelectAll();
                        editor.Paste("Retained text");
                        editor.SelectAll();
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var result = view.Ec(code);
                        Console.WriteLine("Locked clipboard: " + code + "; result=" + result + "; retained=" + (field.V == "Retained text") + "; ms=" + watch.ElapsedMilliseconds);
                        Eq(0, result, "unavailable clipboard command is not dispatched: " + code);
                        Ok(watch.ElapsedMilliseconds < 1000, "clipboard contention retry remains bounded: " + code);
                        Eq("Retained text", field.V, "unavailable clipboard must not change the field: " + code);
                        Ok(editor.SelectionStart == 0 && editor.SelectionLength == 13 && view.Ce("un"), "clipboard rejection retains selection and undo: " + code);
                    }
                    Eq(IntPtr.Zero, NativeEdit(editor.Handle, 0x0302, IntPtr.Zero, IntPtr.Zero), "native WM_PASTE is guarded");
                    Eq("Retained text", field.V, "native WM_PASTE retained text");
                    var key = typeof(V).GetMethod("EditKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var chord in new[] { Keys.Control | Keys.V, Keys.Shift | Keys.Insert })
                    {
                        var args = new KeyEventArgs(chord);
                        key.Invoke(view, new object[] { editor, args });
                        Ok(args.Handled && args.SuppressKeyPress && field.V == "Retained text" && view.Ce("un"), "locked clipboard keyboard paste is consumed without losing text/undo");
                    }
                    view.Ec("un");
                    Eq("Original", field.V, "blocked paste does not overwrite native undo");
                    view.Ec("re");
                    Eq("Retained text", field.V, "blocked paste does not overwrite native redo");
                    var supplied = typeof(V).GetMethod("Pa");
                    Ok(supplied != null, "supplied paste alias exists");
                    Eq(1, (int)supplied.Invoke(view, new object[] { "Direct" }), "supplied paste does not need the locked clipboard");
                }
                finally
                {
                    release.Set();
                    Ok(worker.Join(2000) && !timedOut, "clipboard contention lock did not release normally");
                }
            }
            Console.WriteLine("Lea clipboard contention smoke passed.");
        }

        private static void CheckEditCommands(string root)
        {
            var original = Clipboard.GetDataObject();
            var clipboard = new DataObject();
            if (original != null)
                foreach (var format in original.GetFormats(false)) clipboard.SetData(format, false, original.GetData(format, false));
            try
            {
                using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(720, 800) })
                using (var view = new V())
                using (var other = new TextBox { Dock = DockStyle.Bottom })
                {
                    form.Controls.Add(view);
                    form.Controls.Add(other);
                    form.Show();
                    var path = Path.Combine(pages, "editing.html");
                    view.Go(path);
                    using (var paint = Render(view)) { }
                    var start = typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic);
                    var editor = Editor(view);
                    var field = FieldById(view.At, "text");
                    Ok(!view.Ce("cp") && view.Ec("cp") == 0 && view.Ec(null) == 0 && view.Ec("copy") == 0, "empty and invalid edit commands");
                    start.Invoke(view, new object[] { field });
                    editor.Select(4, 5);
                    Eq(1, EditCommand(view, "CP"), "compact case-insensitive copy");
                    Eq("field", Clipboard.GetText(), "copy selected field text");
                    other.Focus();
                    Application.DoEvents();
                    Ok(!editor.Visible && view.Ce("cp"), "menu focus retains editor selection");
                    Eq(1, EditCommand(view, "cu"), "retained cut");
                    Eq("Lea  text", field.V, "cut updates live field");
                    Ok(view.Ce("un") && !view.Ce("re"), "native undo state");
                    view.Ec("un");
                    Eq("Lea field text", field.V, "undo restores field");
                    Ok(!view.Ce("un") && view.Ce("re"), "one-level redo state");
                    view.Ec("re");
                    Eq("Lea  text", field.V, "redo updates field");
                    var key = typeof(V).GetMethod("EditKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
                    var undoKey = new KeyEventArgs(Keys.Control | Keys.Z);
                    key.Invoke(view, new object[] { editor, undoKey });
                    Eq("Lea field text", field.V, "keyboard undo uses command state");
                    Ok(undoKey.Handled && undoKey.SuppressKeyPress, "native undo does not run twice");
                    key.Invoke(view, new object[] { editor, new KeyEventArgs(Keys.Control | Keys.Y) });
                    Eq("Lea  text", field.V, "keyboard redo uses command state");
                    view.Ec("sa");
                    SetClipboardText("Replacement");
                    CheckPaste(view, field, "Replacement", "native paste replaces selection");
                    Ok(!view.Ce("re"), "new edit clears redo");
                    editor.Select(0, 0);
                    view.Ec("de");
                    Eq("eplacement", field.V, "delete without selection removes next character");
                    Eq("Replacement", Clipboard.GetText(), "delete does not alter clipboard");
                    Eq(1, view.Ec("un"), "delete enters native undo buffer");
                    Eq("Replacement", field.V, "undo deleted character");
                    Eq(1, view.Ec("re"), "redo deleted character");
                    Eq("eplacement", field.V, "delete redo value");
                    editor.Text = "A\ud83d\ude00B";
                    editor.Select(1, 0);
                    view.Ec("de");
                    Eq("AB", field.V, "delete preserves surrogate boundaries");
                    editor.Text = "A\ud83d\ude00B";
                    editor.Select(2, 1);
                    view.Ec("de");
                    Eq("AB", field.V, "partial surrogate selection expands before delete");
                    editor.SelectAll();
                    view.Ec("de");
                    Eq("", field.V, "delete selection");

                    start.Invoke(view, new object[] { FieldById(view.At, "readonly") });
                    Ok(view.Ce("cp") && !view.Ce("cu") && !view.Ce("pa") && !view.Ce("de") && !view.Ce("un"), "read-only edit policy");
                    Eq(1, EditCommand(view, "cp"), "read-only copy command");
                    Eq("Read only", Clipboard.GetText(), "read-only copy");
                    var secret = FieldById(view.At, "secret");
                    start.Invoke(view, new object[] { secret });
                    Ok(!view.Ce("cp") && !view.Ce("cu") && view.Ec("cp") == 0 && view.Ec("cu") == 0, "password copy and cut refused");
                    Eq("Read only", Clipboard.GetText(), "password commands preserve clipboard");
                    Eq("private value", secret.V, "password cut retains value");
                    var limited = FieldById(view.At, "limited");
                    start.Invoke(view, new object[] { limited });
                    editor.Select(4, 0);
                    SetClipboardText("abcdefghijk");
                    CheckPaste(view, limited, "1234abcd", "native paste respects maxlength");
                    var notes = FieldById(view.At, "notes");
                    start.Invoke(view, new object[] { notes });
                    SetClipboardText("One\r\nTwo");
                    CheckPaste(view, notes, "One\r\nTwo", "multiline paste");
                    other.Focus();
                    notes.V = "external change";
                    Ok(!view.Ce("pa") && view.Ec("pa") == 0, "stale retained editor cannot overwrite programmatic value");
                    start.Invoke(view, new object[] { field });
                    editor.Text = "kept";
                    other.Focus();
                    view.Kf();
                    Ok(!view.Ce("pa"), "keyboard focus change clears retained editor");
                    start.Invoke(view, new object[] { field });
                    other.Focus();
                    view.Js("hd('text')");
                    Ok(!view.Ce("pa") && view.Ec("de") == 0, "hidden field cannot be edited through retained commands");
                    view.Js("sh('text')");
                    using (var paint = Render(view)) { }
                    start.Invoke(view, new object[] { field });
                    other.Focus();
                    view.Go(path);
                    Ok(!view.Ce("pa") && field.V == "kept", "navigation drops retained editor without changing old fields");
                    start.Invoke(view, new object[] { FieldById(view.At, "text") });
                    other.Focus();
                    view.Dispose();
                    Ok(!view.Ce("pa") && view.Ec("pa") == 0, "disposed edit command is inert");
                }
                CheckClipboardPasteCases(root);
            }
            finally
            {
                if (original == null) ClearClipboard(); else SetClipboardData(clipboard, true);
            }
            Console.WriteLine("Lea native editing smoke passed: aliases, menu focus, selection, clipboard, undo/redo, readonly/password, limits, stale values, and disposal.");
        }

        private static void SetClipboardText(string text, TextDataFormat format = TextDataFormat.UnicodeText)
        {
            var data = new DataObject();
            data.SetText(text, format);
            SetClipboardData(data, true);
        }

        private static void SetClipboardData(object data, bool copy)
        {
            ExternalException failure = null;
            for (var attempt = 0; attempt < 5; attempt++)
                try { Clipboard.SetDataObject(data, copy, 20, 25); return; }
                catch (ExternalException error)
                {
                    failure = error;
                    Application.DoEvents();
                    Thread.Sleep(50);
                }
            throw failure;
        }

        private static int EditCommand(V view, string code)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var result = view.Ec(code);
                if (result != 0) return result;
                Application.DoEvents();
                Thread.Sleep(50);
            }
            return 0;
        }

        private static void ClearClipboard()
        {
            SetClipboardData(new DataObject(), true);
        }

        private static void CheckClipboardPasteCases(string root)
        {
            using (var form = new Form { Opacity = 0, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), Size = new Size(720, 800) })
            using (var view = new V())
            {
                form.Controls.Add(view);
                form.Show();
                var path = Path.Combine(pages, "editing.html");
                view.Go(path);
                Paint(view);
                var field = FieldById(view.At, "text");
                typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { field });
                var editor = Editor(view);
                var key = typeof(V).GetMethod("EditKeyDown", BindingFlags.Instance | BindingFlags.NonPublic);
                var changes = 0;
                view.Ed += delegate { changes++; };
                foreach (var chord in new[] { Keys.None, Keys.Control | Keys.V, Keys.Shift | Keys.Insert })
                {
                    editor.Text = "Before";
                    editor.SelectAll();
                    SetClipboardText("Keyboard paste");
                    changes = 0;
                    if (chord == Keys.None) Eq(new IntPtr(1), NativeEdit(editor.Handle, 0x0302, IntPtr.Zero, IntPtr.Zero), "native WM_PASTE success");
                    else
                    {
                        var args = new KeyEventArgs(chord);
                        key.Invoke(view, new object[] { editor, args });
                        Ok(args.Handled && args.SuppressKeyPress, "native paste shortcut is consumed");
                    }
                    Eq("Keyboard paste", field.V, "native message/keyboard clipboard paste");
                    Eq(1, changes, "paste emits exactly one edit notification");
                    view.Ec("un");
                    Eq("Before", field.V, "clipboard paste native undo");
                    view.Ec("re");
                    Eq("Keyboard paste", field.V, "clipboard paste native redo");
                }
                editor.SelectAll();
                ClearClipboard();
                Eq(0, view.Ec("pa"), "empty clipboard is inert");
                Eq("Keyboard paste", field.V, "empty clipboard retains selected text");
                Clipboard.SetData("Lea.Paste.Test", "not a text format");
                Eq(0, view.Ec("pa"), "non-text clipboard is inert");
                Eq("Keyboard paste", field.V, "non-text clipboard retains selected text");
                SetClipboardText("ANSI paste", TextDataFormat.Text);
                CheckPaste(view, field, "ANSI paste", "Windows text format conversion");
                editor.SelectAll();
                SetClipboardText(new string('x', 65536));
                Eq(1, view.Ec("pa"), "bounded clipboard prefix paste");
                Eq(32767, field.V.Length, "clipboard text ceiling");
                view.Ec("un");
                Eq("ANSI paste", field.V, "large clipboard paste undo");
                editor.SelectAll();
                SetClipboardText("A\0B");
                CheckPaste(view, field, "A", "clipboard NUL terminator");

                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var guardedPaste = typeof(V).GetMethod("PasteEdit", flags, null, new[] { typeof(Func<string>) }, null);
                Ok(guardedPaste != null, "guarded paste reader is available");
                var reads = 0;
                Func<string> deferred = delegate { reads++; view.Go(path); return "Must not overwrite a new page"; };
                Eq(0, (int)guardedPaste.Invoke(view, new object[] { deferred }), "clipboard callback navigation cancels old paste");
                Eq(1, reads, "delayed clipboard callback exercised");
                Eq("Lea field text", FieldById(view.At, "text").V, "clipboard callback preserves new page fields");
                foreach (var change in new[] { "value", "selection", "dispose" })
                {
                    field = FieldById(view.At, "text");
                    typeof(V).GetMethod("StartEdit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { field });
                    editor.SelectAll();
                    reads = 0;
                    deferred = delegate
                    {
                        reads++;
                        if (change == "value") field.V = "Host value";
                        else if (change == "selection") editor.Select(0, 0);
                        else view.Dispose();
                        return "Stale clipboard text";
                    };
                    Eq(0, (int)guardedPaste.Invoke(view, new object[] { deferred }), "clipboard callback cancels stale paste: " + change);
                    Eq(1, reads, "clipboard callback was exercised: " + change);
                    if (change == "value") Eq("Host value", field.V, "clipboard cannot overwrite host value");
                    else if (change == "selection") Ok(editor.SelectionLength == 0 && field.V == "Host value", "clipboard cannot replace changed selection");
                    else Ok(view.IsDisposed, "clipboard callback disposal completed");
                }
            }
            Console.WriteLine("Lea clipboard paste smoke passed: WM_PASTE, shortcuts, single edit notification, undo/redo, empty/non-text, ANSI, bounded text, and deferred-owner navigation.");
        }

        private static void CheckPaste(V view, Fi field, string expected, string name)
        {
            var pasted = view.Ec("pa");
            if (pasted == 1 && field.V == expected) return;
            var editor = Editor(view);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var editing = typeof(V).GetField("edit", flags).GetValue(view) as Fi;
            var held = typeof(V).GetField("heldEdit", flags).GetValue(view) as Fi;
            var clipboard = Clipboard.GetText();
            using (var native = new TextBox { Multiline = true })
            {
                native.CreateControl();
                native.Paste();
                throw new InvalidOperationException(name + ": result=" + pasted + "; visible=" + editor.Visible + "; focused=" + editor.Focused +
                    "; editing=" + (editing == null ? "null" : editing.El.Av("id")) + "; held=" + (held == null ? "null" : held.El.Av("id")) +
                    "; field=" + field.V + "; text=" + editor.Text + "; clipboard=" + clipboard + "; native=" + native.Text);
            }
        }

        private static void CheckNavigationGuard(string root)
        {
            var forms = Path.Combine(pages, "forms.html");
            var next = Path.Combine(pages, "index.html");
            var engine = new E();
            var first = engine.Go(forms);
            using (var server = new CompressedServer())
            {
                var thread = Thread.CurrentThread.ManagedThreadId;
                var calls = 0;
                engine.Ng = url => { Eq(thread, Thread.CurrentThread.ManagedThreadId, "navigation guard calling thread"); calls++; return false; };
                Action[] actions = {
                    () => engine.Go(server.U), () => engine.Go(server.U.AbsoluteUri),
                    () => engine.GoA(server.U).GetAwaiter().GetResult(), () => engine.GoA(server.U.AbsoluteUri).GetAwaiter().GetResult(),
                    () => engine.Po(server.U, "secret=1"), () => engine.PoA(server.U, "secret=1").GetAwaiter().GetResult(),
                    () => engine.Pb(server.U, new byte[] { 1 }, "application/octet-stream"),
                    () => engine.PbA(server.U, new byte[] { 1 }, "application/octet-stream").GetAwaiter().GetResult(),
                    () => engine.Rl(), () => engine.RlA().GetAwaiter().GetResult(),
                    () => engine.Fg(new Uri(first.U.AbsoluteUri + "#blocked")), () => engine.Go("#blocked"),
                    () => engine.GoA("#blocked").GetAwaiter().GetResult(), () => engine.Go(forms)
                };
                foreach (var action in actions)
                {
                    var error = Fails(action, "host navigation rejection");
                    Eq("blocked", error.K, "navigation error kind");
                    Ok(!error.Rt && error.U != null && engine.At == first && !engine.Busy && !engine.CanBk, "blocked navigation preserves document/history/state");
                }
                Eq(actions.Length, calls, "guard runs once per blocked navigation");
                Eq(0, server.Hits, "blocked GET/POST sends no requests");
                engine.Ng = url => { throw new InvalidOperationException("guard failure"); };
                var threw = false;
                try { engine.Go(server.U); } catch (InvalidOperationException) { threw = true; }
                Ok(threw && server.Hits == 0 && engine.At == first, "guard exceptions fail closed");
                engine.Ng = null;
                var second = engine.Go(next);
                engine.Ng = url => false;
                Eq("blocked", Fails(() => engine.Bk(), "blocked Back").K, "Back guard");
                Ok(engine.At == second && engine.CanBk && !engine.CanFw, "blocked Back preserves history position");
                engine.Ng = url => true;
                engine.Bk();
                engine.Ng = url => false;
                Eq("blocked", Fails(() => engine.Fw(), "blocked Forward").K, "Forward guard");
                Ok(engine.At == first && engine.CanFw, "blocked Forward preserves history position");
                engine.Ng = url => true;
                engine.Go(server.U);
                Eq(1, server.Hits, "allowed navigation reaches network");
                engine.Ng = url => false;
                Eq("blocked", Fails(() => engine.Go(server.U), "guard before cache").K, "cached navigation guard");
                Eq(1, server.Hits, "rejected cached navigation adds no request");
            }
            var view = new V();
            view.Go(forms);
            var page = view.At;
            var changed = 0;
            view.Changed += delegate { changed++; };
            view.Ng = url => false;
            Eq("blocked", Fails(() => view.Go(next), "view guard").K, "view guard alias");
            Eq("blocked", Fails(() => view.GoA(next).GetAwaiter().GetResult(), "async view guard").K, "async view guard alias");
            Ok(view.At == page && changed == 0 && !view.Busy, "view rejection preserves presentation");
            view.Dispose();
            Ok(view.Ng == null, "view disposal releases host guard");
            using (var server = new CompressedServer())
            using (var closing = new V())
            {
                closing.Ng = url => { closing.Dispose(); return true; };
                Eq("blocked", Fails(() => closing.GoA(server.U).GetAwaiter().GetResult(), "guard callback disposal").K, "disposed guard cannot authorize navigation");
                Ok(server.Hits == 0 && closing.Ng == null && closing.IsDisposed, "guard callback disposal sends no request and releases policy");
            }
            using (var input = new V { Size = new Size(640, 480) })
            {
                input.Go(Path.Combine(pages, "focus-shortcuts.html"));
                Paint(input);
                var retained = input.At;
                var calls = 0;
                input.Ng = url => { calls++; return false; };
                var access = Key(input, Keys.Alt | Keys.Shift | Keys.G);
                var enter = Key(input, Keys.Enter);
                Ok(access.Handled && access.SuppressKeyPress && enter.Handled && enter.SuppressKeyPress &&
                    calls == 2 && input.At == retained && input.U.Fragment == "", "blocked native keyboard input is consumed without navigation");
                Eq("blocked", Fails(() => input.Ka().GetAwaiter().GetResult(), "programmatic activation rejection").K, "public activation retains typed errors");
                var boundary = typeof(V).GetMethod("InputActivationA", BindingFlags.Instance | BindingFlags.NonPublic);
                Func<Task<int>> failure = () => { throw new InvalidOperationException("host policy bug"); };
                var escaped = false;
                try { ((Task<int>)boundary.Invoke(input, new object[] { failure })).GetAwaiter().GetResult(); }
                catch (InvalidOperationException error) { escaped = error.Message == "host policy bug"; }
                Ok(escaped, "input boundary must not swallow unexpected failures");
                input.Ng = url => true;
                Key(input, Keys.Enter);
                Ok(input.U.Fragment == "#target", "allowed keyboard activation still navigates");
            }
            Console.WriteLine("Lea navigation guard smoke passed: compact Ng alias, blocked GET/POST/fragments/history/reload/cache, caller-thread policy, state retention, and disposal.");
        }

        private static string FragmentState(V view)
        {
            var target = view.Id("target");
            var rectangles = new List<string>();
            foreach (var box in LayoutBoxes(view))
                if (box.GetType().GetField("El").GetValue(box) == target)
                    rectangles.Add(box.GetType().GetField("C").GetValue(box).ToString());
            return "URL=" + view.U + "; size=" + view.Size + "; Sy=" + view.Sy + "; Fa=" + view.Fa +
                "; Fj=" + view.Fj + "; Fy=" + view.Fy + "; target=" + String.Join(",", rectangles.ToArray());
        }

        private static void CheckFragmentStartup(string root)
        {
            var path = Path.Combine(pages, "fragments.html");
            using (var view = new V { Size = new Size(300, 420) })
                for (var index = 0; index < 25; index++)
                {
                    view.Go(Path.Combine(pages, "second.html"));
                    view.Go(new Uri(new Uri(path).AbsoluteUri + "#target"));
                    Paint(view);
                    Ok(view.U.Fragment == "#target" && view.Fa && view.Fj >= 2 && view.Fy >= 0 &&
                        ElementVisible(view, view.Id("target")), "fragment startup repeat " + index + ": " + FragmentState(view));
                }
            Console.WriteLine("Lea fragment startup repetitions passed.");
        }

        private static void CheckRetainedData(string root)
        {
            var source = Path.Combine(pages, "retained-data.html");
            var engine = new E();
            Ok(engine.Ud(null), "unknown document must be retained");
            var page = engine.Go(source);
            Ok(!engine.Ud(page), "pristine empty fields and default choices do not block hibernation");
            foreach (var id in new[] { "text", "secret", "notes", "choice", "range", "color", "readonly", "disabled" })
            {
                var field = FieldById(page, id);
                field.V = " ";
                Ok(engine.Ud(page), "changed field including whitespace/read-only/disabled is retained: " + id);
                field.V = field.D;
                Ok(!engine.Ud(page), "restored field clears retained data: " + id);
            }
            foreach (var id in new[] { "check", "radio" })
            {
                var field = FieldById(page, id);
                field.C = !field.Dc;
                Ok(engine.Ud(page), "changed check/radio is retained");
                field.C = field.Dc;
            }
            var select = FieldById(page, "choice");
            select.Si++;
            Ok(engine.Ud(page), "changed select index is retained independently of value");
            select.Si = select.Di;
            var file = FieldById(page, "file");
            file.Fp.Add("private-file.txt");
            Ok(engine.Ud(page), "selected file is retained without reading its contents");
            file.Fp.Clear();
            var text = FieldById(page, "text");
            text.V = "Retain when hidden";
            engine.Js(page, "hd('text')");
            Ok(!page.Fs.Contains(text) && engine.Ud(page), "hidden retained fields cannot be lost");
            engine.Js(page, "ds('text'); sh('text')");
            Ok(engine.Ud(page), "disabling a changed field cannot make it disposable");
            text.V = text.D;
            Ok(!engine.Ud(page), "state query does not treat scripts themselves as field edits");
            page.Fs.Add(null);
            Ok(engine.Ud(page), "malformed host field list fails closed");
            page.Fs.RemoveAt(page.Fs.Count - 1);
            var count = page.Fs.Count;
            while (page.Fs.Count <= 8192) page.Fs.Add(text);
            Ok(engine.Ud(page), "retained-field scan is bounded");
            while (page.Fs.Count > count) page.Fs.RemoveAt(page.Fs.Count - 1);
            Ok(!engine.Ud(page), "bounded query does not mutate state");
            var foreign = false;
            try { new E().Ud(page); } catch (ArgumentException) { foreign = true; }
            Ok(foreign, "retained-data query requires document ownership");
            var prefilled = Path.Combine(Path.GetTempPath(), "lea-prefilled-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                File.WriteAllText(prefilled, "<input value='Initial data'>");
                var valuePage = engine.Go(prefilled);
                Ok(engine.Ud(valuePage), "editable prefilled values are conservatively retained");
                valuePage.Fs[0].V = "";
                Ok(engine.Ud(valuePage), "clearing a prefilled value is retained too");
            }
            finally { if (File.Exists(prefilled)) File.Delete(prefilled); }
            using (var view = new V { Size = new Size(640, 480) })
            {
                Ok(view.Ud, "empty view retained-data state is conservative");
                view.Go(source);
                Paint(view);
                Ok(!view.Ud, "native retained-data alias");
                view.Aa("t").GetAwaiter().GetResult();
                var editor = Editor(view);
                Ok(editor.Visible, "retained data test opened native editor");
                editor.Text = "Native edit";
                Ok(view.Ud, "active native editor is included without ending edit");
                Ok(editor.Visible && editor.Text == "Native edit", "query must not blur or mutate the editor");
                view.Dispose();
                Ok(view.Ud, "disposed view is never eligible for data discard");
            }
            Console.WriteLine("Lea retained-data smoke passed: Ud, defaults, changed/hidden/protected fields, selected files, native editing, ownership, and bounded scans.");
        }

        private static void CheckLoadFailures(string root)
        {
            var source = Path.Combine(pages, "forms.html");
            var missing = Path.Combine(Path.GetTempPath(), "lea-retry-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                using (var view = new V())
                using (var server = new CompressedServer())
                {
                    view.Go(source);
                    var retained = view.At;
                    var changes = 0;
                    var failures = 0;
                    Er notified = null;
                    view.Changed += delegate { changes++; };
                    view.Done += delegate { changes++; };
                    view.Fe += delegate { failures++; notified = view.Le; Ok(!view.Busy, "failure follows request cleanup"); };
                    var failure = Fails(() => view.Go(missing), "view missing file");
                    Ok(failure == view.Le && failure == notified && view.Cr && failures == 1, "single typed failure notification and retry state");
                    Ok(view.At == retained && changes == 0 && !view.CanBk, "failure preserves page, history, and successful events");
                    Eq(0, view.Fg(new Uri(missing + "#other")), "cross-document fragment probe");
                    Ok(view.Le == failure && view.Cr, "no-op fragment probe preserves failure");
                    view.Ng = retryUrl => false;
                    var denied = view.Ry();
                    WaitFor(() => denied.IsCompleted, "denied retry completion");
                    Eq("blocked", Fails(() => denied.GetAwaiter().GetResult(), "retry rechecks guard").K, "retry policy");
                    File.WriteAllText(missing, "<title>Retry ready</title><p>Now available</p>");
                    view.Ng = null;
                    var retry = view.Ry();
                    WaitFor(() => retry.IsCompleted, "explicit file retry");
                    retry.GetAwaiter().GetResult();
                    Ok(view.Le == null && !view.Cr && view.T == "Retry ready" && view.CanBk, "successful explicit retry clears failure and commits once");
                    var before = changes;
                    view.Ry().GetAwaiter().GetResult();
                    Eq(before, changes, "ineligible retry does not reload");
                    view.Go(source);
                    retained = view.At;
                    var url = new Uri(server.U, "?status=503");
                    Action[] posts = {
                        () => view.Po(url, "secret=one"),
                        () => view.Pb(url, Encoding.UTF8.GetBytes("secret=two"), "application/octet-stream")
                    };
                    foreach (var post in posts)
                    {
                        Fails(post, "failed POST");
                        var hits = server.Hits;
                        Ok(view.Le.St == 503 && !view.Cr && view.At == retained, "POST retains page without retry target");
                        view.Ry().GetAwaiter().GetResult();
                        Eq(hits, server.Hits, "Retry must never replay a POST");
                    }
                    foreach (var task in new Func<Task<P>>[] {
                        () => view.GoA(url), () => view.PoA(url, "secret=three"),
                        () => view.PbA(url, new byte[] { 4 }, "application/octet-stream") })
                    {
                        var count = failures;
                        var work = task();
                        WaitFor(() => work.IsCompleted, "async failure completion");
                        var error = Fails(() => work.GetAwaiter().GetResult(), "async view failure");
                        Ok(error == view.Le && error == notified && failures == count + 1, "async failure identity and count");
                        Eq(server.LastMethod == "GET", view.Cr, "only failed GET is retryable");
                        Ok(view.At == retained, "async failure preserves page");
                    }
                    view.Go(missing);
                    view.Ng = urlValue => false;
                    Fails(() => view.Bk(), "blocked retained history");
                    Ok(!view.Cr, "denied cached history does not become a GET retry");
                    view.Ng = null;
                    view.Bk();
                    Ok(view.Le == null && !view.Cr, "successful retained history clears failure");
                }
                using (var view = new V())
                {
                    Fails(() => view.Go(""), "invalid first URL");
                    Ok(view.Le.K == "url" && !view.Cr, "invalid URL cannot be retried");
                }
                using (var view = new V())
                {
                    view.Fe += delegate { view.Go(source); };
                    Fails(() => view.Go(""), "failure callback navigation");
                    Ok(view.T != "" && view.Le == null && !view.Cr, "callback navigation not overwritten by stale failure");
                }
                using (var view = new V())
                {
                    var count = 0;
                    view.Fe += delegate { count++; view.Dispose(); };
                    Fails(() => view.Go(""), "failure callback disposal");
                    Ok(count == 1 && view.IsDisposed && view.Le == null && !view.Cr, "failure callback can dispose safely");
                    Ok(typeof(V).GetField("Fe", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view) == null, "disposal releases failure subscriptions");
                }
                foreach (var dispose in new[] { false, true })
                using (var server = new CompressedServer())
                using (var view = new V())
                {
                    view.Go(source);
                    Fails(() => view.Go(missing + ".absent"), "failure before cancellation");
                    var count = 0;
                    view.Fe += delegate { count++; };
                    var work = view.GoA(new Uri(server.U, "?delay=500"));
                    WaitFor(() => server.Hits == 1, "cancellable request started");
                    Ok(view.Le == null && !view.Cr, "new request clears old failure");
                    if (dispose) view.Dispose(); else view.St();
                    WaitFor(() => work.IsCompleted, "cancelled view load completion");
                    Eq("cancel", Fails(() => work.GetAwaiter().GetResult(), "cancelled view load").K, "view cancellation kind");
                    Ok(count == 0 && view.Le == null && !view.Cr, "cancel/dispose produces no failure notification or retry");
                }
            }
            finally { if (File.Exists(missing)) File.Delete(missing); }
            Console.WriteLine("Lea load failure smoke passed: Fe/Le/Cr/Ry, retained state, typed errors, explicit retry policy, no POST replay, callback navigation/disposal, and cancellation.");
        }

        private static void WaitFor(Func<bool> ready, string name)
        {
            var end = DateTime.UtcNow.AddSeconds(3);
            while (!ready() && DateTime.UtcNow < end)
            {
                Application.DoEvents();
                Thread.Sleep(2);
            }
            Ok(ready(), name);
        }

        private static T Complete<T>(Task<T> work, string name)
        {
            WaitFor(() => work.IsCompleted, name);
            return work.GetAwaiter().GetResult();
        }

        private static void Eq<T>(T expected, T actual, string name)
        {
            if (!Object.Equals(expected, actual)) throw new InvalidOperationException(name + ": expected " + expected + ", got " + actual);
        }

        private static void Ok(bool value, string name)
        {
            if (!value) throw new InvalidOperationException(name);
        }

        private static Er Fails(Action action, string name)
        {
            try { action(); }
            catch (Er error) { return error; }
            throw new InvalidOperationException(name + ": expected Lea.Engine.Er");
        }

        private sealed class CompressedServer : IDisposable
        {
            private readonly TcpListener listener;
            private readonly Thread thread;
            private readonly object state = new object();
            private readonly ManualResetEvent stop = new ManualResetEvent(false);
            private int hits;
            private volatile bool stopping;
            private string lastMethod = "";
            private string lastBody = "";
            private string lastType = "";
            private string lastIfNoneMatch = "";
            private string lastIfModifiedSince = "";
            private string lastCookie = "";

            public Uri U { get; private set; }
            public int Hits { get { return Interlocked.CompareExchange(ref hits, 0, 0); } }
            public string LastMethod { get { lock (state) return lastMethod; } }
            public string LastBody { get { lock (state) return lastBody; } }
            public string LastType { get { lock (state) return lastType; } }
            public string LastIfNoneMatch { get { lock (state) return lastIfNoneMatch; } }
            public string LastIfModifiedSince { get { lock (state) return lastIfModifiedSince; } }
            public string LastCookie { get { lock (state) return lastCookie; } }
            public ManualResetEvent ImageGate;
            public ManualResetEvent RequestGate;

            public CompressedServer()
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                U = new Uri("http://127.0.0.1:" + port + "/page");
                thread = new Thread(Serve);
                thread.IsBackground = true;
                thread.Start();
            }

            private void Serve()
            {
                while (!stopping)
                {
                    TcpClient client;
                    try { client = listener.AcceptTcpClient(); }
                    catch (SocketException) { if (stopping) return; throw; }
                    catch (ObjectDisposedException) { return; }
                    using (client)
                    using (var stream = client.GetStream())
                    {
                        string request;
                        var contentLength = 0;
                        var contentType = "";
                        var requestBody = "";
                        var ifNoneMatch = "";
                        var ifModifiedSince = "";
                        var cookie = "";
                        using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
                        {
                            request = reader.ReadLine() ?? "";
                            string line;
                            while ((line = reader.ReadLine()) != null && line.Length > 0)
                            {
                                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) Int32.TryParse(line.Substring(15).Trim(), out contentLength);
                                else if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase)) contentType = line.Substring(13).Trim();
                                else if (line.StartsWith("If-None-Match:", StringComparison.OrdinalIgnoreCase)) ifNoneMatch = line.Substring(14).Trim();
                                else if (line.StartsWith("If-Modified-Since:", StringComparison.OrdinalIgnoreCase)) ifModifiedSince = line.Substring(18).Trim();
                                else if (line.StartsWith("Cookie:", StringComparison.OrdinalIgnoreCase)) cookie = line.Substring(7).Trim();
                            }
                            if (contentLength > 0)
                            {
                                var bodyChars = new char[contentLength];
                                var read = 0;
                                while (read < bodyChars.Length)
                                {
                                    var count = reader.Read(bodyChars, read, bodyChars.Length - read);
                                    if (count < 1) break;
                                    read += count;
                                }
                                requestBody = new String(bodyChars, 0, read);
                            }
                        }

                        Interlocked.Increment(ref hits);
                        var methodEnd = request.IndexOf(' ');
                        lock (state)
                        {
                            lastMethod = methodEnd < 0 ? request : request.Substring(0, methodEnd);
                            lastBody = requestBody;
                            lastType = contentType;
                            lastIfNoneMatch = ifNoneMatch;
                            lastIfModifiedSince = ifModifiedSince;
                            lastCookie = cookie;
                        }
                        var delay = IntValue(QueryValue(request, "delay"));
                        if (RequestGate != null && WaitHandle.WaitAny(new WaitHandle[] { stop, RequestGate }, 5000) != 1) return;
                        if (ImageGate != null && (QueryValue(request, "image") == "1" || QueryValue(request, "pixel") == "1") &&
                            WaitHandle.WaitAny(new WaitHandle[] { stop, ImageGate }, 5000) != 1) return;
                        if (delay > 0 && stop.WaitOne(Math.Min(delay, 2000))) return;
                        if (QueryValue(request, "pixel") == "1")
                        {
                            var pixel = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXYxDP9v0PAAM7Ac+IRop4AAAAAElFTkSuQmCC");
                            var header = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: image/png\r\nContent-Length: " + pixel.Length + "\r\nConnection: close\r\n\r\n");
                            stream.Write(header, 0, header.Length);
                            stream.Write(pixel, 0, pixel.Length);
                            continue;
                        }
                        if (request.IndexOf("redirect=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var redirect = Encoding.ASCII.GetBytes("HTTP/1.1 302 Found\r\nLocation: /page?encoding=gzip&redirected=1\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                            stream.Write(redirect, 0, redirect.Length);
                            continue;
                        }
                        if (request.IndexOf("status=503", StringComparison.OrdinalIgnoreCase) >= 0 || request.IndexOf("status=404", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var unavailable = request.IndexOf("status=503", StringComparison.OrdinalIgnoreCase) >= 0;
                            var status = unavailable ? "503 Service Unavailable" : "404 Not Found";
                            var failure = Encoding.ASCII.GetBytes("HTTP/1.1 " + status + "\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                            stream.Write(failure, 0, failure.Length);
                            continue;
                        }
                        if (request.IndexOf("download-large=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var large = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: 67108865\r\nConnection: close\r\n\r\n");
                            stream.Write(large, 0, large.Length);
                            continue;
                        }
                        var etag = request.IndexOf("etag=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        var lastModified = request.IndexOf("last-modified=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        if ((etag && ifNoneMatch == "\"lea-v1\"") || (lastModified && ifModifiedSince.Length > 0))
                        {
                            var validator = etag ? "ETag: \"lea-v1\"\r\n" : "Last-Modified: Wed, 12 Aug 2026 12:00:00 GMT\r\n";
                            var unchanged = Encoding.ASCII.GetBytes("HTTP/1.1 304 Not Modified\r\nCache-Control: max-age=60\r\n" + validator + "Content-Length: 0\r\nConnection: close\r\n\r\n");
                            stream.Write(unchanged, 0, unchanged.Length);
                            continue;
                        }
                        var deflate = request.IndexOf("encoding=deflate", StringComparison.OrdinalIgnoreCase) >= 0;
                        var stylesheet = request.StartsWith("GET /style", StringComparison.OrdinalIgnoreCase) || request.StartsWith("GET /large.css", StringComparison.OrdinalIgnoreCase);
                        var largeStylesheet = request.StartsWith("GET /large.css", StringComparison.OrdinalIgnoreCase);
                        string html;
                        var caseKey = QueryValue(request, "case-key");
                        if (largeStylesheet)
                            html = new String(' ', 270 * 1024) + "body{background:#ff0000}";
                        else if (stylesheet && caseKey == "Upper")
                            html = "body{background:#123456}";
                        else if (stylesheet && caseKey == "upper")
                            html = "body{background:#654321}";
                        else if (stylesheet)
                            html = "body{background:#0f6848;color:#ffffff}.remote{color:#a13b18;font-size:16px}";
                        else if (request.IndexOf("external-case=", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var variant = QueryValue(request, "external-case");
                            html = "<html><head><title>Case CSS " + variant + "</title><link rel='stylesheet' href='/StyleCase.css?case-key=" +
                                variant + "'></head><body>Case stylesheet</body></html>";
                        }
                        else if (request.IndexOf("external-page=", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var variant = QueryValue(request, "external-page");
                            var links = variant == "large" ? "<link rel='stylesheet' href='/large.css'>" : "<link rel='stylesheet' href='/style.css?v=" + variant + "'>";
                            if (variant == "slow") links = "<link rel='stylesheet' href='/style.css?delay=1000'>";
                            if (variant == "limit")
                            {
                                var many = new StringBuilder();
                                for (var styleIndex = 0; styleIndex < 10; styleIndex++) many.Append("<link rel='stylesheet' href='/style").Append(styleIndex).Append(".css'>");
                                links = many.ToString();
                            }
                            html = "<html><head><title>Lea Linked CSS</title>" + links + "</head><body><p class='remote'>Linked network CSS.</p></body></html>";
                        }
                        else if (request.IndexOf("multipart-form=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            html = "<html><head><title>Lea Multipart Form</title></head><body><form action='/page?post=1' method='post' enctype='multipart/form-data'>" +
                                "<input type='file' name='upload' accept='.txt' required><input name='note' value='lea'>" +
                                "<input type='checkbox' name='keep' value='yes' checked><button type='submit' name='send' value='upload'>Upload</button></form></body></html>";
                        }
                        else if (request.IndexOf("image-page=1", StringComparison.OrdinalIgnoreCase) >= 0)
                            html = "<title>Pending image</title><img src='/page?delay=1000&image=1'>";
                        else if (request.IndexOf("image-page=valid", StringComparison.OrdinalIgnoreCase) >= 0)
                            html = "<title>Valid image</title><img width='24' height='24' src='/page?pixel=1'>";
                        else if (request.IndexOf("form=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            html = "<html><head><title>Lea POST Form</title></head><body><form action='/page?post=1' method='post'>" +
                                "<select name='flavor'><option value='vanilla'>Vanilla</option><option value='mint' selected>Mint</option><option value='old' disabled>Old</option></select>" +
                                "<input name='q' value='lea'><input type='checkbox' name='news' value='yes' checked><input type='checkbox' name='skip' value='no'>" +
                                "<button type='submit' name='send' value='go'>Send</button></form></body></html>";
                        }
                        else if (request.StartsWith("POST ", StringComparison.OrdinalIgnoreCase))
                            html = "<html><head><title>Lea POST</title></head><body><p>POST navigation worked.</p></body></html>";
                        else if (request.IndexOf("cookie=read", StringComparison.OrdinalIgnoreCase) >= 0)
                            html = "<html><head><title>Cookie " + (CookieValue(cookie, "sid").Length == 0 ? "empty" : CookieValue(cookie, "sid")) + "</title></head><body><p>Cookie response.</p></body></html>";
                        else if (caseKey.Length > 0)
                            html = "<html><head><title>Case " + caseKey + "</title></head><body>Case identity response.</body></html>";
                        else
                        {
                            var title = deflate ? "Lea deflate" : "Lea gzip";
                            html = "<html><head><title>" + title + "</title></head><body><p>Compressed Lea response.</p></body></html>";
                        }
                        var source = Encoding.UTF8.GetBytes(html);
                        var badGzip = request.IndexOf("bad-gzip=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        var body = badGzip ? Encoding.ASCII.GetBytes("not a gzip stream") : Compress(source, deflate);
                        var noStore = request.IndexOf("no-store=1", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            request.IndexOf("external-page=", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            request.IndexOf("external-case=", StringComparison.OrdinalIgnoreCase) >= 0;
                        var cookieRoute = request.IndexOf("cookie=", StringComparison.OrdinalIgnoreCase) >= 0;
                        var noCache = request.IndexOf("no-cache=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        var maxAge = request.IndexOf("max-age=60", StringComparison.OrdinalIgnoreCase) >= 0 ? "Cache-Control: max-age=60\r\n" :
                            request.IndexOf("max-age=0", StringComparison.OrdinalIgnoreCase) >= 0 ? "Cache-Control: max-age=0\r\n" : "";
                        var expires = request.IndexOf("expires=future", StringComparison.OrdinalIgnoreCase) >= 0 ? "Expires: " + DateTime.UtcNow.AddMinutes(5).ToString("R") + "\r\n" : "";
                        var validators = etag ? "Cache-Control: max-age=0\r\nETag: \"lea-v1\"\r\n" :
                            lastModified ? "Cache-Control: max-age=0\r\nLast-Modified: Wed, 12 Aug 2026 12:00:00 GMT\r\n" : "";
                        var cookieName = QueryValue(request, "cookie");
                        var setCookie = cookieName == "set-normal" ? "Set-Cookie: sid=normal; Path=/; HttpOnly\r\n" :
                            cookieName == "set-private" ? "Set-Cookie: sid=private; Path=/; HttpOnly\r\n" :
                            IntValue(cookieName) >= 0 ? "Set-Cookie: c" + cookieName + "=v; Path=/\r\n" : "";
                        var headers = "HTTP/1.1 200 OK\r\nContent-Type: " + (stylesheet ? "text/css" : "text/html") + "; charset=utf-8\r\nContent-Encoding: " + (deflate ? "deflate" : "gzip") + "\r\n" +
                            (noStore || cookieRoute ? "Cache-Control: no-store\r\n" : stylesheet ? "Cache-Control: max-age=60\r\n" : noCache ? "Cache-Control: no-cache\r\n" : maxAge + expires + validators) + setCookie + "Content-Length: " + body.Length + "\r\nConnection: close\r\n\r\n";
                        var head = Encoding.ASCII.GetBytes(headers);
                        stream.Write(head, 0, head.Length);
                        stream.Write(body, 0, body.Length);
                    }
                }
            }

            private static byte[] Compress(byte[] source, bool deflate)
            {
                using (var output = new MemoryStream())
                {
                    using (Stream zip = deflate ? (Stream)new DeflateStream(output, CompressionMode.Compress, true) : new GZipStream(output, CompressionMode.Compress, true))
                        zip.Write(source, 0, source.Length);
                    return output.ToArray();
                }
            }

            private static string QueryValue(string request, string name)
            {
                var marker = name + "=";
                var start = request.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (start < 0) return "";
                start += marker.Length;
                var end = request.IndexOfAny(new char[] { '&', ' ' }, start);
                return request.Substring(start, (end < 0 ? request.Length : end) - start);
            }

            private static string CookieValue(string header, string name)
            {
                foreach (var item in (header ?? "").Split(';'))
                {
                    var split = item.IndexOf('=');
                    if (split > 0 && item.Substring(0, split).Trim().Equals(name, StringComparison.OrdinalIgnoreCase)) return item.Substring(split + 1).Trim();
                }
                return "";
            }

            private static int IntValue(string value)
            {
                int number;
                return Int32.TryParse(value, out number) && number >= 0 ? number : -1;
            }

            public void Dispose()
            {
                stopping = true;
                stop.Set();
                listener.Stop();
                thread.Join(2000);
                stop.Dispose();
            }
        }
    }
}
