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
                Eq(200, page.St, "local status");
                Eq("text/html", page.Ct, "local content type");
                Eq("utf-8", page.Ch, "local charset");
                Ok(page.Sz > 0 && !page.Ca, "local response metadata");
                Eq(9, page.Fs.Count, "field count");
                Eq("text,password,textarea,select,checkbox,radio,radio,reset,submit", Kinds(page), "field kinds");
                Eq(3, page.Fs[3].Os.Count, "select options");
                Eq("lea", page.Fs[3].V, "selected option value");
                Eq("Lea green", page.Fs[3].Os[1].T, "selected option text");
                Ok(page.Fs[3].Os[2].Ds, "disabled option");
                Ok(page.Fs[4].C && page.Fs[5].C && !page.Fs[6].C, "choice defaults");

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

                using (var view = new V())
                {
                    view.Size = new Size(720, 760);
                    view.CreateControl();
                    view.Go(forms);
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
                    var gzip = new Uri(server.U, "?encoding=gzip");
                    var first = net.Go(gzip);
                    Eq(200, first.St, "HTTP status");
                    Eq("text/html", first.Ct, "HTTP content type");
                    Eq("gzip", first.Ce, "GZip response encoding");
                    Eq("utf-8", first.Ch, "HTTP charset");
                    Eq("Lea gzip", first.T, "GZip decoded page");
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
                    }
                    Eq(26, server.Hits, "form and POST request count");

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

                    var missingFile = new Uri(Path.Combine(root, "LeaForge", "test-pages", "missing-lea-page.html"));
                    var fileError = Fails(delegate { net.Go(missingFile); }, "missing file");
                    Eq("file", fileError.K, "file error kind");
                    Ok(!fileError.Rt, "file error not retryable");
                    var urlError = Fails(delegate { new E().Go(""); }, "bad URL");
                    Eq("url", urlError.K, "URL error kind");
                }

                Console.WriteLine("Lea smoke passed: text alignment, line height, inline flow, external CSS, bounded private cookies, cache revalidation, nested table links, typed errors, selects, POST, compression, forms, CSS boxes, images, layout, and scrolling.");
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
            using (var image = new Bitmap(view.Width, view.Height))
                view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
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

        private static void Click(V view, Rectangle rect)
        {
            var method = typeof(V).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("mouse click method unavailable");
            method.Invoke(view, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, 0) });
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

        private static System.Collections.IEnumerable LayoutBoxes(V view)
        {
            var field = typeof(V).GetField("boxes", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new InvalidOperationException("layout boxes unavailable");
            var values = field.GetValue(view) as System.Collections.IEnumerable;
            if (values == null) throw new InvalidOperationException("layout box values unavailable");
            return values;
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
                listener.Stop();
                thread.Join(2000);
            }
        }
    }
}
