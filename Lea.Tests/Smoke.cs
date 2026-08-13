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
                    view.Go(selectorPath);
                    Paint(view);
                    Eq(12, view.At.Bs.Count, "relationship selector render blocks");
                    var firstCard = BlockRect(view, view.At.Bs[5]);
                    var secondCard = BlockRect(view, view.At.Bs[6]);
                    var thirdCard = BlockRect(view, view.At.Bs[7]);
                    Eq(firstCard.Left + 18, secondCard.Left, "adjacent selector second-card layout");
                    Eq(firstCard.Left + 18, thirdCard.Left, "adjacent selector third-card layout");
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

                Console.WriteLine("Lea smoke passed: general sibling and negation selectors, min/max sizing, long-word breaking, compiled attribute and pseudo selectors, font families, text decoration, child and adjacent CSS selectors, white-space modes, overflow clipping, malformed HTML recovery, semantic blocks, nested lists, text alignment, line height, inline flow, external CSS, bounded private cookies, cache revalidation, nested table links, typed errors, selects, POST, compression, forms, CSS boxes, images, layout, and scrolling.");
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

        private static string PaintedText(V view, B block)
        {
            var text = new StringBuilder();
            foreach (var value in LayoutBoxes(view))
            {
                var type = value.GetType();
                if (!Object.ReferenceEquals(type.GetField("B").GetValue(value), block) || !(bool)type.GetField("Inline").GetValue(value)) continue;
                text.Append((string)type.GetField("X").GetValue(value));
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
