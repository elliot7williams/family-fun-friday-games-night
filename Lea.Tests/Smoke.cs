using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Tests
{
    internal static class Smoke
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
                var forms = Path.Combine(root, "LeaForge", "test-pages", "forms.html");
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

                var validationPath = Path.Combine(root, "LeaForge", "test-pages", "form-validation.html");
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

                var widgetsPath = Path.Combine(root, "LeaForge", "test-pages", "form-widgets.html");
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

                var tablePage = engine.Go(Path.Combine(root, "LeaForge", "test-pages", "tables.html"));
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

                var externalPath = Path.Combine(root, "LeaForge", "test-pages", "external.html");
                var externalPage = engine.Go(externalPath);
                Eq("#edf7f2", externalPage.S.Bg, "external body background");
                Eq(3, externalPage.Bs.Count, "external CSS hidden block");
                Eq("#176b4d", externalPage.Bs[0].S.F, "external heading color");
                Eq(19, externalPage.Bs[0].S.Fs, "external heading size");
                Eq("#7e22ce", externalPage.Bs[1].S.F, "later inline stylesheet order");
                Ok(externalPage.Bs[1].S.B, "external inherited bold");
                Eq("#a13b18", externalPage.Bs[2].S.F, "external class color");
                Eq("Lea External CSS", new E().GoA(externalPath).GetAwaiter().GetResult().T, "async local external CSS");

                var basePath = Path.Combine(root, "LeaForge", "test-pages", "base-url.html");
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
                var asyncBasePage = new E().GoA(basePath).GetAwaiter().GetResult();
                Eq(expectedBase, asyncBasePage.Bu, "async document base URL");
                Eq("#edf7f2", asyncBasePage.S.Bg, "async base URL external stylesheet");

                var scriptPath = Path.Combine(root, "LeaForge", "test-pages", "script-bridge.html");
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
                Eq("Lea Script Bridge", new E().GoA(scriptPath).GetAwaiter().GetResult().T, "async page scripts");

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

                var eventPath = Path.Combine(root, "LeaForge", "test-pages", "events-vars.html");
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

                var goPath = Path.Combine(root, "LeaForge", "test-pages", "go-command.html");
                var goTargetPath = Path.Combine(root, "LeaForge", "test-pages", "go-target.html");
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

                var classPath = Path.Combine(root, "LeaForge", "test-pages", "class-commands.html");
                var classEngine = new E();
                var classPage = classEngine.Go(classPath);
                Eq(4, classPage.Jc, "page class command count");
                Eq("panel hot", ById(classPage.D, "panel").Av("class"), "add class alias");
                Eq("keep", ById(classPage.D, "remove").Av("class"), "remove class alias");
                Eq("panel hot", ById(classPage.D, "toggle").Av("class"), "toggle class alias adds");
                Eq("#b91c1c", BlockById(classPage, "panel").S.F, "added class reapplies CSS");
                var classField = FieldById(classPage, "field");
                classField.V = "Edited class value";
                Eq(3, classEngine.Js(classPage, "rc('panel','hot'); ac('field','hot'); tc('toggle','hot');"), "host class commands");
                Eq("panel", ById(classPage.D, "panel").Av("class"), "host remove class");
                Eq("field hot", ById(classPage.D, "field").Av("class"), "host add field class");
                Eq("panel", ById(classPage.D, "toggle").Av("class"), "host toggle class removes");
                Eq("#334155", BlockById(classPage, "panel").S.F, "removed class reapplies CSS");
                Eq("#166534", FieldById(classPage, "field").S.F, "field class reapplies CSS");
                Eq("Edited class value", FieldById(classPage, "field").V, "class rebuild preserves field value");
                Eq(1, classEngine.Js(classPage, "ac('field','HOT');"), "duplicate add is accepted");
                Eq(2, ClassCount(ById(classPage.D, "field")), "duplicate add stays unique");
                Eq(1, classEngine.Js(classPage, "rc('remove','missing');"), "absent remove is accepted");
                Eq(0, classEngine.Js(classPage, "ac('missing','hot'); ac('unsafe-style','hot'); ac('panel','two words'); ac('panel','" + new String('x', 65) + "');"), "invalid class commands ignored");

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
                Eq(3, clickClassEngine.Ck(clickClassPage, ById(clickClassPage.D, "flip")), "click class commands");
                Eq("panel", ById(clickClassPage.D, "panel").Av("class"), "click toggle removes class");
                Eq("field hot", ById(clickClassPage.D, "field").Av("class"), "click toggle adds class");
                Eq("Click-preserved value", FieldById(clickClassPage, "field").V, "click class rebuild preserves field value");
                Eq("Class state changed", clickClassPage.Ms[0], "click class alert");

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

                var findPath = Path.Combine(root, "LeaForge", "test-pages", "find-download.html");
                var findPage = engine.Go(findPath);
                Eq(3, engine.Fd(findPage, "cobalt"), "engine page-text find count");
                Eq(3, engine.Fd(findPage, "COBALT"), "engine find is case insensitive");
                Eq(0, engine.Fd(findPage, "missing phrase"), "engine missing find count");

                var downloadSource = Path.Combine(root, "LeaForge", "test-pages", "download-sample.txt");
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

                var inlinePath = Path.Combine(root, "LeaForge", "test-pages", "inline.html");
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

                var textPath = Path.Combine(root, "LeaForge", "test-pages", "text-layout.html");
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

                var whitePath = Path.Combine(root, "LeaForge", "test-pages", "white-space.html");
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

                var sizingPath = Path.Combine(root, "LeaForge", "test-pages", "sizing-breaks.html");
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

                var visibilityPath = Path.Combine(root, "LeaForge", "test-pages", "visibility-opacity.html");
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

                var dynamicPath = Path.Combine(root, "LeaForge", "test-pages", "dynamic-state.html");
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

                var mediaPath = Path.Combine(root, "LeaForge", "test-pages", "responsive-media.html");
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

                var positioningPath = Path.Combine(root, "LeaForge", "test-pages", "positioning.html");
                var positioningPage = engine.Go(positioningPath);
                Eq(6, positioningPage.Bs.Count, "positioning block count");
                Eq(4, positioningPage.Ls.Count, "positioning link count");
                var contextsPath = Path.Combine(root, "LeaForge", "test-pages", "position-contexts.html");
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
                var positionValuesPath = Path.Combine(root, "LeaForge", "test-pages", "position-values.html");
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
                var nestedPositionPath = Path.Combine(root, "LeaForge", "test-pages", "nested-positioning.html");
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
                var nestedFlowPath = Path.Combine(root, "LeaForge", "test-pages", "nested-flow.html");
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
                var nestedContentPath = Path.Combine(root, "LeaForge", "test-pages", "nested-content.html");
                var nestedContentPage = engine.Go(nestedContentPath);
                Eq(9, nestedContentPage.Bs.Count, "nested content block count");
                Eq(2, nestedContentPage.Fs.Count, "nested content field count");
                Eq("h1,section,input,div,input,img,table,p,p", BlockKinds(nestedContentPage), "nested content block kinds");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[2].Up, nestedContentPage.Bs[1]), "form field crosses wrapper into visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[3].Up, nestedContentPage.Bs[1]) && Object.ReferenceEquals(nestedContentPage.Bs[4].Up, nestedContentPage.Bs[3]), "clipped field keeps nearest visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[5].Up, nestedContentPage.Bs[1]), "picture image belongs to visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[6].Up, nestedContentPage.Bs[1]), "table belongs to visual parent");
                Ok(Object.ReferenceEquals(nestedContentPage.Bs[7].Up, nestedContentPage.Bs[1]) && nestedContentPage.Bs[8].Up == null, "nested content exits visual parent cleanly");
                var stackingPath = Path.Combine(root, "LeaForge", "test-pages", "stacking-contexts.html");
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

                var selectorPath = Path.Combine(root, "LeaForge", "test-pages", "selectors.html");
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

                var negationPath = Path.Combine(root, "LeaForge", "test-pages", "selector-negation.html");
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

                var selectorStatePath = Path.Combine(root, "LeaForge", "test-pages", "selector-states.html");
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

                var textStylePath = Path.Combine(root, "LeaForge", "test-pages", "text-style.html");
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

                var recoveryPath = Path.Combine(root, "LeaForge", "test-pages", "recovery.html");
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

                var listPath = Path.Combine(root, "LeaForge", "test-pages", "lists.html");
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

                var disclosurePath = Path.Combine(root, "LeaForge", "test-pages", "disclosure.html");
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

                var fragmentPath = Path.Combine(root, "LeaForge", "test-pages", "fragments.html");
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
                Eq(0, fragmentEngine.Fg(new Uri(Path.Combine(root, "LeaForge", "test-pages", "second.html"))), "cross-document URL is not a fragment navigation");

                Eq(1, fragmentEngine.Fg("#closed-target"), "closed fragment history navigation");
                var closedFragmentTarget = fragmentEngine.Ft(fragmentPage, fragmentEngine.At.U.Fragment);
                Eq(1, fragmentEngine.Rv(fragmentPage, closedFragmentTarget), "fragment reveals one closed details ancestor");
                Ok(ById(fragmentPage.D, "closed-details").Op && !BlockById(fragmentPage, "closed-target").Hd, "revealed fragment content joins layout model");
                Ok(Object.ReferenceEquals(retainedFragmentField, FieldById(fragmentPage, "fragment-field")) && retainedFragmentField.V == "Edited before fragment reveal", "fragment reveal preserves field state");
                Eq(0, fragmentEngine.Rv(fragmentPage, closedFragmentTarget), "already visible fragment needs no rebuild");
                var fragmentAsyncPage = fragmentEngine.GoA("#encoded%20target").GetAwaiter().GetResult();
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

                var gaugePath = Path.Combine(root, "LeaForge", "test-pages", "gauges.html");
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

                var valuePath = Path.Combine(root, "LeaForge", "test-pages", "values.html");
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

                var timePath = Path.Combine(root, "LeaForge", "test-pages", "time-edits.html");
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

                var tokenPath = Path.Combine(root, "LeaForge", "test-pages", "html-tokens.html");
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
                    view.Je = false;
                    Eq(0, view.Js("t('Blocked view script');"), "WinForms script setting");
                    Eq("View Script", view.T, "disabled WinForms script leaves title");
                    view.Je = true;

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
                    WaitFor(delegate { return view.T == "LeaForge Second Page"; }, "safe event link navigation");
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
                    Eq(3, view.Ck(ById(view.At.D, "flip")), "WinForms class click commands");
                    Paint(view);
                    Eq("panel", ById(view.At.D, "panel").Av("class"), "WinForms class remove");
                    Eq("field hot", ById(view.At.D, "field").Av("class"), "WinForms class add");
                    Eq("#166534", FieldById(view.At, "field").S.F, "WinForms class restyle");
                    Eq("View-preserved value", FieldById(view.At, "field").V, "WinForms class field preservation");
                    Eq(6, alertEvents, "WinForms class alert event");

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
                    Eq(6, alertEvents, "history does not replay script alerts");
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
                    var boxes = Path.Combine(root, "LeaForge", "test-pages", "boxes.html");
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
                    view.Go(boxes);
                    Paint(view);
                    Eq(5, view.Bc, "box-model boxes");
                    Eq(280, view.Rc(2).Width, "solid content width");
                    Eq(308, view.Rb(2).Width, "solid border-box width");
                    Eq(54, view.Rc(3).Height, "fixed content height");
                    Eq(76, view.Rb(3).Height, "fixed border-box height");
                    Eq(320, view.Rc(4).Width, "sided content width");
                    Eq(347, view.Rb(4).Width, "sided border-box width");
                    view.Go(Path.Combine(root, "LeaForge", "test-pages", "tables.html"));
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
                    WaitFor(delegate { return view.T == "LeaForge Second Page"; }, "table cell link navigation");
                    view.Size = new Size(300, 520);
                    view.Go(inlinePath);
                    Paint(view);
                    Ok(view.Bc > inlinePage.Bs.Count + 1, "inline runs create paint boxes");
                    var inlineBlock = BlockRect(view, view.At.Bs[1]);
                    Ok(inlineBlock.Height > 40, "inline text wraps and honors line break");
                    Ok(SameInlineHeights(view, view.At.Bs[0]), "heading inline runs retain heading size");
                    var inlineLink = LinkRect(view, "second.html");
                    Ok(!inlineLink.IsEmpty && inlineBlock.Contains(new Point(inlineLink.Left, inlineLink.Top)), "inline link stays inside paragraph");
                    Eq(3, LinkBoxes(view, "second.html"), "only inline link words are clickable");
                    Click(view, inlineLink);
                    WaitFor(delegate { return view.T == "LeaForge Second Page"; }, "inline link navigation");
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
                        Eq(4, PicCount(view), "deduplicated responsive image cache");
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
                    WaitFor(delegate { return view.T == "Lea Dynamic State"; }, "topmost overlap link navigation");
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
                    WaitFor(delegate { return view.T == "Lea Dynamic State"; }, "fixed clipped link navigation after scroll");
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
                    WaitFor(delegate { return view.T == "Lea Dynamic State"; }, "translated context overlap click");
                    view.Size = new Size(720, 500);
                    view.Go(nestedPositionPath);
                    using (var nestedPaint = Render(view))
                    {
                        Eq(42, view.Bc, "nested context layout boxes");
                        Eq(16, HitCount(view, false), "nested context link hits");
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
                    WaitFor(delegate { return view.T == "Lea Dynamic State"; }, "higher parent context wins overlap click");
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
                    WaitFor(delegate { return view.T == "Lea Forms Test"; }, "fixed descendant link navigation after scroll");
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
                    WaitFor(delegate { return view.T == "LeaForge Second Page"; }, "nested flow link navigation");
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
                    WaitFor(delegate { return view.T == "Lea Dynamic State"; }, "opacity context overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "second.html"));
                    WaitFor(delegate { return view.T == "LeaForge Second Page"; }, "isolation context overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "form-widgets.html"));
                    WaitFor(delegate { return view.T == "Lea Form Validation"; }, "z-index auto overlap click");
                    view.Go(stackingPath);
                    Paint(view);
                    Click(view, HitRect(view, "images.html"));
                    WaitFor(delegate { return view.T == "Lea Image Test"; }, "numeric zero overlap click");
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

                    view.Go(Path.Combine(root, "LeaForge", "test-pages", "second.html"));
                    view.Go(new Uri(new Uri(fragmentPath).AbsoluteUri + "#target"));
                    Paint(view);
                    Ok(view.U.Fragment == "#target" && ElementVisible(view, ById(view.At.D, "target")), "initial cross-document URL fragment scrolls after load");

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
                    view.Go(Path.Combine(root, "LeaForge", "test-pages", "scroll.html"));
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

                    var asyncPage = net.GoA(new Uri(server.U, "?encoding=deflate&async=1")).GetAwaiter().GetResult();
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
                    var modifiedValidated = net.GoA(modifiedUrl).GetAwaiter().GetResult();
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
                    var missingHttp = Fails(delegate { net.GoA(new Uri(server.U, "?status=404")).GetAwaiter().GetResult(); }, "async HTTP 404");
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
                    var asyncLinked = new E().GoA(new Uri(server.U, "?external-page=async")).GetAwaiter().GetResult();
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
                    Eq("Cookie private", jar.GoA(new Uri(server.U, "?cookie=read")).GetAwaiter().GetResult().T, "async private cookie round trip");
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

                    var missingFile = new Uri(Path.Combine(root, "LeaForge", "test-pages", "missing-lea-page.html"));
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
                }

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
                    var timeout = Fails(delegate { asyncTimeoutEngine.GoA(new Uri(slow.U, "?delay=1000")).GetAwaiter().GetResult(); }, "asynchronous request timeout");
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

                Console.WriteLine("Lea smoke passed: compact output/data values, bounded host aliases, safe rebuilds, inline and table ownership, resize reflow, compact progress and meter values, quality regions, inherited custom painting, bounded retained fragment history, encoded and named targets, hidden reveal, bounded details disclosures with retained state, keyboard and table interaction, bounded ordered and unordered list markers, nested list scope and resize reflow, bounded host-controlled go requests, class mutations, document base URLs and Id/Q/Qa queries, bounded HTML tokenization, raw text, RCDATA, comments, entities and malformed delimiters, bounded developer diagnostics and inspector data, safe click commands and bounded variables/text updates, streamed downloads and highlighted find-in-page, bounded script commands and alert bridging, inset stretching and translated stacking contexts, nested forms, media, tables, and clipped editors, explicit z-index and opacity/isolation contexts, two-pass positioned auto height, nested visual flow, bounded multipart file uploads, range and color widgets, temporal fields, absolute and fixed positioning, clipped viewport hits, form validation and typed inputs, responsive and embedded images, live hover and focus restyling, visibility and opacity, general sibling and negation selectors, min/max sizing, long-word breaking, compiled attribute and pseudo selectors, fonts, overflow clipping, malformed HTML recovery, semantic blocks, tables, inline flow, external CSS, private cookies, cache revalidation, typed errors, POST, compression, layout, and scrolling.");
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

        private static void Check(V view, string root, string file, int blocks, int boxes)
        {
            view.Go(Path.Combine(root, "LeaForge", "test-pages", file));
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

        private static int LinkBoxes(V view, string file)
        {
            var count = 0;
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                var url = type.GetField("U").GetValue(value) as Uri;
                if (url != null && Path.GetFileName(url.LocalPath).Equals(file, StringComparison.OrdinalIgnoreCase)) count++;
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
                        if (delay > 0 && stop.WaitOne(Math.Min(delay, 2000))) return;
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
                        if (largeStylesheet)
                            html = new String(' ', 270 * 1024) + "body{background:#ff0000}";
                        else if (stylesheet)
                            html = "body{background:#0f6848;color:#ffffff}.remote{color:#a13b18;font-size:16px}";
                        else if (request.IndexOf("external-page=", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var variant = QueryValue(request, "external-page");
                            var links = variant == "large" ? "<link rel='stylesheet' href='/large.css'>" : "<link rel='stylesheet' href='/style.css?v=" + variant + "'>";
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
                        else
                        {
                            var title = deflate ? "Lea deflate" : "Lea gzip";
                            html = "<html><head><title>" + title + "</title></head><body><p>Compressed Lea response.</p></body></html>";
                        }
                        var source = Encoding.UTF8.GetBytes(html);
                        var badGzip = request.IndexOf("bad-gzip=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        var body = badGzip ? Encoding.ASCII.GetBytes("not a gzip stream") : Compress(source, deflate);
                        var noStore = request.IndexOf("no-store=1", StringComparison.OrdinalIgnoreCase) >= 0 || request.IndexOf("external-page=", StringComparison.OrdinalIgnoreCase) >= 0;
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
