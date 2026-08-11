using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lea.Engine
{
    // Lea's .NET Framework-compatible engine surface.
    public sealed class E
    {
        private const int MaxCachePage = 2 * 1024 * 1024;
        private readonly W web = new W();
        private readonly List<P> hs = new List<P>();
        private readonly Dictionary<string, Cr> cache = new Dictionary<string, Cr>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> lru = new List<string>();
        private int at = -1;
        private bool busy;
        private int cm = 8;
        private int ch;

        public P At { get { return at >= 0 && at < hs.Count ? hs[at] : null; } }
        public bool CanBk { get { return at > 0; } }
        public bool CanFw { get { return at < hs.Count - 1; } }
        public bool Busy { get { return busy; } }
        public int Cm
        {
            get { return cm; }
            set { cm = Math.Max(0, Math.Min(16, value)); TrimCache(); }
        }
        public int Cc { get { return cache.Count; } }
        public int Ch { get { return ch; } }

        public void Cl()
        {
            cache.Clear();
            lru.Clear();
        }

        public P Go(string input)
        {
            return Go(ToUri(input, At == null ? null : At.U));
        }

        public P Go(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = Read(url);
            Push(p);
            return p;
        }

        public P Po(string input, string data)
        {
            return Po(ToUri(input, At == null ? null : At.U), data);
        }

        public P Po(Uri url, string data)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = Post(url, data);
            Push(p);
            return p;
        }

        public async Task<P> GoA(string input)
        {
            return await GoA(ToUri(input, At == null ? null : At.U));
        }

        public async Task<P> GoA(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = await ReadA(url);
            Push(p);
            return p;
        }

        public async Task<P> PoA(string input, string data)
        {
            return await PoA(ToUri(input, At == null ? null : At.U), data);
        }

        public async Task<P> PoA(Uri url, string data)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = await PostA(url, data);
            Push(p);
            return p;
        }

        private void Push(P p)
        {
            if (at < hs.Count - 1) hs.RemoveRange(at + 1, hs.Count - at - 1);
            hs.Add(p);
            at = hs.Count - 1;
        }

        public P Bk()
        {
            if (CanBk) at--;
            return At;
        }

        public P Fw()
        {
            if (CanFw) at++;
            return At;
        }

        public P Rl()
        {
            if (At == null) return null;
            var p = Read(At.U, true);
            hs[at] = p;
            return p;
        }

        public async Task<P> RlA()
        {
            if (At == null) return null;
            var p = await ReadA(At.U, true);
            hs[at] = p;
            return p;
        }

        public void St()
        {
            if (busy) web.CancelAsync();
        }

        private P Read(Uri url, bool fresh = false)
        {
            Cr reply;
            if (!url.IsFile && !fresh && GetCache(url, out reply)) return Parse(reply, true);
            if (url.IsFile) reply = ReadFile(url);
            else
            {
                var bytes = web.DownloadData(url);
                reply = ReadWeb(url, bytes);
                PutCache(url, reply);
            }
            return Parse(reply, false);
        }

        private async Task<P> ReadA(Uri url, bool fresh = false)
        {
            Cr cached;
            if (!url.IsFile && !fresh && GetCache(url, out cached)) return Parse(cached, true);
            if (busy) throw new InvalidOperationException("Lea is already loading a page.");
            busy = true;
            try
            {
                if (url.IsFile)
                {
                    return await Task.Factory.StartNew(delegate { return Parse(ReadFile(url), false); });
                }

                var bytes = await web.DownloadDataTaskAsync(url);
                var reply = ReadWeb(url, bytes);
                PutCache(url, reply);
                return Parse(reply, false);
            }
            finally
            {
                busy = false;
            }
        }

        private P Post(Uri url, string data)
        {
            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                throw new NotSupportedException("Lea POST requires an HTTP or HTTPS address.");
            DropCache(url);
            web.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";
            try
            {
                var bytes = web.UploadData(url, "POST", Encoding.UTF8.GetBytes(data ?? ""));
                var reply = ReadWeb(url, bytes);
                DropCache(reply.U);
                return Parse(reply, false);
            }
            finally { web.Headers.Remove(HttpRequestHeader.ContentType); }
        }

        private async Task<P> PostA(Uri url, string data)
        {
            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                throw new NotSupportedException("Lea POST requires an HTTP or HTTPS address.");
            if (busy) throw new InvalidOperationException("Lea is already loading a page.");
            busy = true;
            try
            {
                DropCache(url);
                web.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";
                var bytes = await web.UploadDataTaskAsync(url, "POST", Encoding.UTF8.GetBytes(data ?? ""));
                var reply = ReadWeb(url, bytes);
                DropCache(reply.U);
                return Parse(reply, false);
            }
            finally
            {
                busy = false;
                web.Headers.Remove(HttpRequestHeader.ContentType);
            }
        }

        private static P Parse(Cr reply, bool cached)
        {
            var p = new P();
            p.U = reply.U;
            p.Q = Query(reply.U);
            p.H = reply.H;
            p.St = reply.St;
            p.Ct = reply.Ct;
            p.Ce = reply.Ce;
            p.Ch = reply.Ch;
            p.Sz = reply.Sz;
            p.Ca = cached;
            p.D = Tree(reply.H);
            var title = Find(p.D, "title");
            p.T = Clean(NodeText(title));
            if (p.T.Length == 0) p.T = reply.U.IsFile ? Path.GetFileName(reply.U.LocalPath) : reply.U.Host;

            var body = Find(p.D, "body") ?? p.D;
            p.X = Clean(NodeText(body));
            p.N = Count(p.D);
            var rules = Css(p.D);
            p.S = Style(body, new S(), rules);
            foreach (var child in body.C) Blocks(p, child, reply.U, rules, p.S);

            return p;
        }

        private static N Tree(string html)
        {
            var root = new N("#doc");
            var stack = new List<N>();
            stack.Add(root);

            foreach (Match token in Regex.Matches(html ?? "", "<!--.*?-->|<![^>]*>|</?\\s*[a-z][^>]*>|[^<]+|<", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var value = token.Value;
                if (value.StartsWith("<!--") || value.StartsWith("<!")) continue;

                if (value.StartsWith("</"))
                {
                    var close = Regex.Match(value, "^</\\s*(?<tag>[a-z][a-z0-9:-]*)", RegexOptions.IgnoreCase);
                    if (!close.Success) continue;
                    var tag = close.Groups["tag"].Value.ToLowerInvariant();
                    for (var i = stack.Count - 1; i > 0; i--)
                    {
                        if (stack[i].K != tag) continue;
                        stack.RemoveRange(i, stack.Count - i);
                        break;
                    }
                    continue;
                }

                if (value.StartsWith("<"))
                {
                    var open = Regex.Match(value, "^<\\s*(?<tag>[a-z][a-z0-9:-]*)(?<attrs>[^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (!open.Success) continue;
                    var tag = open.Groups["tag"].Value.ToLowerInvariant();
                    if (tag == "li" || tag == "p") CloseOpen(stack, tag);
                    var node = new N(tag);
                    Attrs(node, open.Groups["attrs"].Value);
                    node.Up = stack[stack.Count - 1];
                    stack[stack.Count - 1].C.Add(node);
                    if (!value.TrimEnd().EndsWith("/>") && !Void(tag)) stack.Add(node);
                    continue;
                }

                var parent = stack[stack.Count - 1];
                if (value.Length > 0) parent.C.Add(new N("#text") { X = WebUtility.HtmlDecode(value), Up = parent });
            }

            return root;
        }

        private static void CloseOpen(List<N> stack, string tag)
        {
            for (var i = stack.Count - 1; i > 0; i--)
            {
                if (stack[i].K != tag) continue;
                stack.RemoveRange(i, stack.Count - i);
                return;
            }
        }

        private static void Attrs(N node, string attrs)
        {
            const string pattern = "(?<n>[a-z_:][a-z0-9_:.\\-]*)(?:\\s*=\\s*(?:\"(?<d>[^\"]*)\"|'(?<s>[^']*)'|(?<b>[^\\s\"'=<>]+)))?";
            foreach (Match match in Regex.Matches(attrs ?? "", pattern, RegexOptions.IgnoreCase))
            {
                var value = match.Groups["d"].Success ? match.Groups["d"].Value : match.Groups["s"].Success ? match.Groups["s"].Value : match.Groups["b"].Value;
                node.A[match.Groups["n"].Value] = WebUtility.HtmlDecode(value);
            }
        }

        private static bool Void(string tag)
        {
            return tag == "area" || tag == "base" || tag == "br" || tag == "col" || tag == "embed" || tag == "hr" || tag == "img" || tag == "input" || tag == "link" || tag == "meta" || tag == "param" || tag == "source" || tag == "track" || tag == "wbr";
        }

        private static N Find(N node, string kind)
        {
            if (node == null) return null;
            if (node.K == kind) return node;
            foreach (var child in node.C)
            {
                var found = Find(child, kind);
                if (found != null) return found;
            }
            return null;
        }

        private static int Count(N node)
        {
            if (node == null) return 0;
            var count = node.K.StartsWith("#") ? 0 : 1;
            foreach (var child in node.C) count += Count(child);
            return count;
        }

        private static string NodeText(N node)
        {
            if (node == null) return "";
            var text = new StringBuilder();
            AddText(node, text);
            return text.ToString();
        }

        private static void AddText(N node, StringBuilder text)
        {
            if (node.K == "#text")
            {
                text.Append(node.X).Append(' ');
                return;
            }
            if (node.K == "script" || node.K == "style" || node.K == "noscript") return;
            foreach (var child in node.C) AddText(child, text);
        }

        private static List<R> Css(N root)
        {
            var rules = new List<R>();
            var styles = new List<N>();
            FindAll(root, "style", styles);
            var order = 0;
            foreach (var style in styles)
            {
                foreach (Match match in Regex.Matches(RawText(style), "(?<q>[^{}]+)\\{(?<v>[^{}]*)\\}", RegexOptions.Singleline))
                {
                    foreach (var selector in match.Groups["q"].Value.Split(','))
                    {
                        var query = selector.Trim();
                        if (query.Length > 0) rules.Add(new R(query, match.Groups["v"].Value, Spec(query), order++));
                    }
                }
            }
            rules.Sort(delegate(R a, R b) { var byWeight = a.W.CompareTo(b.W); return byWeight != 0 ? byWeight : a.O.CompareTo(b.O); });
            return rules;
        }

        private static void FindAll(N node, string kind, List<N> found)
        {
            if (node.K == kind) found.Add(node);
            foreach (var child in node.C) FindAll(child, kind, found);
        }

        private static string RawText(N node)
        {
            var text = new StringBuilder();
            RawText(node, text);
            return text.ToString();
        }

        private static void RawText(N node, StringBuilder text)
        {
            if (node.K == "#text") { text.Append(node.X); return; }
            foreach (var child in node.C) RawText(child, text);
        }

        private static int Spec(string selector)
        {
            var score = Regex.Matches(selector, "#[a-z0-9_-]+", RegexOptions.IgnoreCase).Count * 100;
            score += Regex.Matches(selector, "\\.[a-z0-9_-]+", RegexOptions.IgnoreCase).Count * 10;
            score += Regex.Matches(selector, "(?:^|\\s)[a-z][a-z0-9-]*", RegexOptions.IgnoreCase).Count;
            return score;
        }

        private static bool MatchCss(N node, string selector)
        {
            var parts = Regex.Split(selector.Trim(), "\\s+");
            if (parts.Length == 0 || !MatchPart(node, parts[parts.Length - 1])) return false;
            var parent = node.Up;
            for (var i = parts.Length - 2; i >= 0; i--)
            {
                while (parent != null && !MatchPart(parent, parts[i])) parent = parent.Up;
                if (parent == null) return false;
                parent = parent.Up;
            }
            return true;
        }

        private static bool MatchPart(N node, string selector)
        {
            var match = Regex.Match(selector, "^(?<tag>[a-z][a-z0-9-]*)?(?<id>#[a-z0-9_-]+)?(?<classes>(?:\\.[a-z0-9_-]+)*)$", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            var tag = match.Groups["tag"].Value;
            if (tag.Length > 0 && node.K != tag.ToLowerInvariant()) return false;
            var id = match.Groups["id"].Value.TrimStart('#');
            if (id.Length > 0 && !node.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return false;

            var classes = new HashSet<string>((node.Av("class") ?? "").Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
            foreach (Match item in Regex.Matches(match.Groups["classes"].Value, "\\.([a-z0-9_-]+)", RegexOptions.IgnoreCase))
                if (!classes.Contains(item.Groups[1].Value)) return false;
            return true;
        }

        private static S Style(N node, S inherited, List<R> rules)
        {
            var style = inherited == null ? new S() : inherited.Inherit();
            foreach (var rule in rules) if (MatchCss(node, rule.Q)) Decls(style, rule.V);
            Decls(style, node.Av("style"));
            return style;
        }

        private static void Decls(S style, string declarations)
        {
            foreach (var declaration in (declarations ?? "").Split(';'))
            {
                var split = declaration.IndexOf(':');
                if (split < 1) continue;
                var name = declaration.Substring(0, split).Trim().ToLowerInvariant();
                var value = declaration.Substring(split + 1).Trim();
                if (name == "color") style.F = value;
                else if (name == "background" || name == "background-color") style.Bg = value;
                else if (name == "font-weight") style.B = value.Equals("bold", StringComparison.OrdinalIgnoreCase) || value == "700" || value == "800" || value == "900";
                else if (name == "display") style.D = value.ToLowerInvariant();
                else if (name == "margin") Box(style, value, true);
                else if (name == "padding") Box(style, value, false);
                else if (name == "margin-top") style.Mt = Px(value);
                else if (name == "margin-right") style.Mr = Px(value);
                else if (name == "margin-bottom") style.Mb = Px(value);
                else if (name == "margin-left") style.Ml = Px(value);
                else if (name == "padding-top") style.Pt = Px(value);
                else if (name == "padding-right") style.Pr = Px(value);
                else if (name == "padding-bottom") style.Pb = Px(value);
                else if (name == "padding-left") style.Pl = Px(value);
                else if (name == "width") style.W = Px(value);
                else if (name == "height") style.H = Px(value);
                else if (name == "border") Border(style, value, 0);
                else if (name == "border-top") Border(style, value, 1);
                else if (name == "border-right") Border(style, value, 2);
                else if (name == "border-bottom") Border(style, value, 3);
                else if (name == "border-left") Border(style, value, 4);
                else if (name == "border-width") BorderWidths(style, value);
                else if (name == "border-top-width") style.Bt = Px(value);
                else if (name == "border-right-width") style.Br = Px(value);
                else if (name == "border-bottom-width") style.Bb = Px(value);
                else if (name == "border-left-width") style.Bl = Px(value);
                else if (name == "border-color") style.Bc = value;
                else if (name == "border-style") style.Bs = value.ToLowerInvariant();
                else if (name == "font-size")
                {
                    var size = Regex.Match(value, "(?<n>[0-9]{1,3})(?:px)?", RegexOptions.IgnoreCase);
                    int number;
                    if (size.Success && int.TryParse(size.Groups["n"].Value, out number) && number > 0 && number < 200) style.Fs = number;
                }
            }
        }

        private static void Box(S style, string value, bool margin)
        {
            var parts = Regex.Split(value.Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4) return;
            var top = Px(parts[0]);
            var right = parts.Length > 1 ? Px(parts[1]) : top;
            var bottom = parts.Length > 2 ? Px(parts[2]) : top;
            var left = parts.Length > 3 ? Px(parts[3]) : right;
            if (parts.Length == 2) bottom = top;
            if (margin) { style.Mt = top; style.Mr = right; style.Mb = bottom; style.Ml = left; }
            else { style.Pt = top; style.Pr = right; style.Pb = bottom; style.Pl = left; }
        }

        private static int Px(string value)
        {
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,4})(?:px)?$", RegexOptions.IgnoreCase);
            int number;
            return match.Success && int.TryParse(match.Groups["n"].Value, out number) ? Math.Min(number, 2000) : 0;
        }

        private static void Border(S style, string value, int side)
        {
            var width = -1;
            var borderStyle = "";
            var color = "";
            foreach (var token in Regex.Split((value ?? "").Trim(), "\\s+"))
            {
                var lower = token.ToLowerInvariant();
                var size = BorderPx(lower);
                if (size >= 0) width = size;
                else if (lower == "none" || lower == "hidden" || lower == "solid" || lower == "dashed" || lower == "dotted" || lower == "double") borderStyle = lower;
                else if (token.Length > 0) color = token;
            }
            if (width < 0 && borderStyle.Length > 0 && borderStyle != "none" && borderStyle != "hidden") width = 3;
            if (width >= 0) SetBorder(style, side, width);
            if (borderStyle.Length > 0) style.Bs = borderStyle;
            if (color.Length > 0) style.Bc = color;
        }

        private static void BorderWidths(S style, string value)
        {
            var parts = Regex.Split((value ?? "").Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4) return;
            var top = Math.Max(0, BorderPx(parts[0]));
            var right = parts.Length > 1 ? Math.Max(0, BorderPx(parts[1])) : top;
            var bottom = parts.Length > 2 ? Math.Max(0, BorderPx(parts[2])) : top;
            var left = parts.Length > 3 ? Math.Max(0, BorderPx(parts[3])) : right;
            if (parts.Length == 2) bottom = top;
            style.Bt = top;
            style.Br = right;
            style.Bb = bottom;
            style.Bl = left;
        }

        private static int BorderPx(string value)
        {
            if (value == "thin") return 1;
            if (value == "medium") return 3;
            if (value == "thick") return 5;
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,3})(?:px)?$", RegexOptions.IgnoreCase);
            int number;
            return match.Success && int.TryParse(match.Groups["n"].Value, out number) ? Math.Min(number, 100) : -1;
        }

        private static void SetBorder(S style, int side, int width)
        {
            if (side == 0 || side == 1) style.Bt = width;
            if (side == 0 || side == 2) style.Br = width;
            if (side == 0 || side == 3) style.Bb = width;
            if (side == 0 || side == 4) style.Bl = width;
        }

        private static void Blocks(P page, N node, Uri baseUrl, List<R> rules, S inherited)
        {
            if (node == null) return;
            var kind = node.K;
            var style = Style(node, inherited, rules);
            if (style.D == "none") return;
            if (kind == "a")
            {
                Uri link;
                var href = node.Av("href");
                var text = Clean(NodeText(node));
                if (href.Length > 0 && Uri.TryCreate(baseUrl, href, out link))
                {
                    page.Ls.Add(new Ln { U = link, T = text });
                    page.Bs.Add(new B { K = kind, X = text.Length == 0 ? link.ToString() : text, U = link, S = style });
                }
            }
            else if (kind == "img")
            {
                Uri image;
                var source = node.Av("src");
                if (source.Length > 0 && Uri.TryCreate(baseUrl, source, out image))
                    page.Bs.Add(new B { K = kind, X = node.Av("alt"), U = image, S = style });
            }
            else if (kind == "input" || kind == "textarea" || kind == "button" || kind == "select")
            {
                AddField(page, node, baseUrl, style);
            }
            else if (kind == "h1" || kind == "h2" || kind == "h3" || kind == "p" || kind == "li" || kind == "pre" || kind == "code" || kind == "label")
            {
                var text = Clean(NodeText(node));
                if (text.Length > 0) page.Bs.Add(new B { K = kind, X = text, S = style });
            }

            foreach (var child in node.C) Blocks(page, child, baseUrl, rules, style);
        }

        private static void AddField(P page, N node, Uri baseUrl, S style)
        {
            var kind = node.K == "textarea" ? "textarea" : node.K == "select" ? "select" : node.K == "button" ? FieldKind(node, "submit") : FieldKind(node, "text");
            if (kind == "hidden") return;

            var buttonText = node.K == "button" ? Clean(NodeText(node)) : "";
            var value = node.K == "textarea" ? RawText(node).Trim() : node.Av("value");
            if (node.K == "button" && value.Length == 0) value = buttonText;
            if ((kind == "checkbox" || kind == "radio") && value.Length == 0) value = "on";

            var form = Parent(node, "form");
            Uri action = baseUrl;
            Uri formAction;
            if (form != null && form.Av("action").Length > 0 && Uri.TryCreate(baseUrl, form.Av("action"), out formAction)) action = formAction;
            int max;
            var field = new Fi
            {
                K = kind,
                N = node.Av("name"),
                V = value,
                D = value,
                P = node.Av("placeholder"),
                C = node.A.ContainsKey("checked"),
                Dc = node.A.ContainsKey("checked"),
                Ds = node.A.ContainsKey("disabled"),
                Ro = node.A.ContainsKey("readonly"),
                Mx = Int32.TryParse(node.Av("maxlength"), out max) ? Math.Max(0, Math.Min(max, 32767)) : 0,
                F = form,
                U = action,
                M = form == null || form.Av("method").Length == 0 ? "get" : form.Av("method").Trim().ToLowerInvariant(),
                S = style
            };
            if (kind == "select") AddOptions(field, node);
            page.Fs.Add(field);

            var blockKind = node.K == "button" || kind == "submit" || kind == "reset" || kind == "button" ? "button" : node.K;
            var text = node.K == "button" && buttonText.Length > 0 ? buttonText : blockKind == "button" ? value : node.Av("aria-label");
            if (blockKind == "button" && text.Length == 0) text = kind == "reset" ? "Reset" : kind == "submit" ? "Submit" : "Button";
            page.Bs.Add(new B { K = blockKind, X = text, S = style, I = field });
        }

        private static void AddOptions(Fi field, N select)
        {
            var nodes = new List<N>();
            FindAll(select, "option", nodes);
            O first = null;
            O chosen = null;
            var chosenIndex = -1;
            var index = 0;
            foreach (var node in nodes)
            {
                var text = Clean(NodeText(node));
                var option = new O
                {
                    V = node.A.ContainsKey("value") ? node.Av("value") : text,
                    T = text,
                    Ds = node.A.ContainsKey("disabled") || (node.Up != null && node.Up.K == "optgroup" && node.Up.A.ContainsKey("disabled"))
                };
                field.Os.Add(option);
                if (first == null) first = option;
                if (chosen == null && node.A.ContainsKey("selected"))
                {
                    chosen = option;
                    chosenIndex = index;
                }
                index++;
            }
            if (chosen == null)
            {
                chosen = first;
                chosenIndex = first == null ? -1 : 0;
            }
            field.V = chosen == null ? "" : chosen.V;
            field.D = field.V;
            field.Si = chosenIndex;
            field.Di = chosenIndex;
        }

        private static string FieldKind(N node, string fallback)
        {
            var kind = node.Av("type").Trim().ToLowerInvariant();
            return kind.Length == 0 ? fallback : kind;
        }

        private static N Parent(N node, string kind)
        {
            var parent = node.Up;
            while (parent != null && parent.K != kind) parent = parent.Up;
            return parent;
        }

        private bool GetCache(Uri url, out Cr reply)
        {
            var key = CacheKey(url);
            reply = null;
            if (cm == 0 || !cache.TryGetValue(key, out reply)) return false;
            lru.Remove(key);
            lru.Add(key);
            ch++;
            return true;
        }

        private void PutCache(Uri url, Cr reply)
        {
            var key = CacheKey(url);
            if (cm == 0 || reply.Ns || reply.St < 200 || reply.St >= 300 || reply.Sz > MaxCachePage)
            {
                cache.Remove(key);
                lru.Remove(key);
                return;
            }
            cache[key] = reply;
            lru.Remove(key);
            lru.Add(key);
            TrimCache();
        }

        private void TrimCache()
        {
            while (lru.Count > cm)
            {
                var key = lru[0];
                lru.RemoveAt(0);
                cache.Remove(key);
            }
        }

        private void DropCache(Uri url)
        {
            if (url == null) return;
            var key = CacheKey(url);
            cache.Remove(key);
            lru.Remove(key);
        }

        private static string CacheKey(Uri url)
        {
            if (url.Fragment.Length == 0) return url.AbsoluteUri;
            var clean = new UriBuilder(url);
            clean.Fragment = "";
            return clean.Uri.AbsoluteUri;
        }

        private static Cr ReadFile(Uri url)
        {
            var path = Local(url);
            var bytes = File.ReadAllBytes(path);
            string charset;
            var html = Decode(bytes, "", out charset);
            return new Cr(url, html, 200, FileType(path), "", charset, bytes.Length, true);
        }

        private Cr ReadWeb(Uri url, byte[] bytes)
        {
            var body = Expand(bytes, web.Ce);
            string charset;
            var html = Decode(body, Charset(web.Ct), out charset);
            return new Cr(web.U ?? url, html, web.St == 0 ? 200 : web.St, MediaType(web.Ct), web.Ce, charset, body.Length, web.Ns);
        }

        private static byte[] Expand(byte[] bytes, string contentEncoding)
        {
            var encoding = (contentEncoding ?? "").Trim().ToLowerInvariant();
            if (encoding != "gzip" && encoding != "deflate") return bytes;
            using (var input = new MemoryStream(bytes))
            using (Stream zip = encoding == "gzip" ? (Stream)new GZipStream(input, CompressionMode.Decompress) : new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                zip.CopyTo(output);
                return output.ToArray();
            }
        }

        private static string FileType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".htm" || extension == ".html") return "text/html";
            if (extension == ".txt") return "text/plain";
            if (extension == ".css") return "text/css";
            return "application/octet-stream";
        }

        private static string MediaType(string contentType)
        {
            var split = (contentType ?? "").IndexOf(';');
            return (split < 0 ? contentType ?? "" : contentType.Substring(0, split)).Trim().ToLowerInvariant();
        }

        private static string Charset(string contentType)
        {
            var match = Regex.Match(contentType ?? "", "(?:^|;)\\s*charset\\s*=\\s*[\"']?(?<c>[^;\\s\"']+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["c"].Value.Trim() : "";
        }

        private static string Decode(byte[] bytes, string charset, out string used)
        {
            Encoding encoding = null;
            if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255) encoding = new UTF32Encoding(true, true);
            else if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0) encoding = Encoding.UTF32;
            else if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191) encoding = Encoding.UTF8;
            else if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == 255) encoding = Encoding.BigEndianUnicode;
            else if (bytes.Length >= 2 && bytes[0] == 255 && bytes[1] == 254) encoding = Encoding.Unicode;
            if (encoding == null && charset.Length > 0)
            {
                try { encoding = Encoding.GetEncoding(charset); }
                catch (ArgumentException) { }
            }
            if (encoding == null) encoding = Encoding.UTF8;
            used = encoding.WebName.ToLowerInvariant();
            using (var stream = new MemoryStream(bytes))
            using (var reader = new StreamReader(stream, encoding, true)) return reader.ReadToEnd();
        }

        private sealed class R
        {
            public readonly string Q;
            public readonly string V;
            public readonly int W;
            public readonly int O;

            public R(string query, string values, int weight, int order)
            {
                Q = query;
                V = values;
                W = weight;
                O = order;
            }
        }

        private sealed class Cr
        {
            public readonly Uri U;
            public readonly string H;
            public readonly int St;
            public readonly string Ct;
            public readonly string Ce;
            public readonly string Ch;
            public readonly int Sz;
            public readonly bool Ns;

            public Cr(Uri url, string html, int status, string contentType, string contentEncoding, string charset, int size, bool noStore)
            {
                U = url;
                H = html ?? "";
                St = status;
                Ct = contentType ?? "";
                Ce = (contentEncoding ?? "").Trim().ToLowerInvariant();
                Ch = charset ?? "";
                Sz = size;
                Ns = noStore;
            }
        }

        private sealed class W : WebClient
        {
            public Uri U { get; private set; }
            public int St { get; private set; }
            public string Ct { get; private set; }
            public string Ce { get; private set; }
            public bool Ns { get; private set; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                U = null;
                St = 0;
                Ct = "";
                Ce = "";
                Ns = false;
                var request = base.GetWebRequest(address);
                var http = request as HttpWebRequest;
                if (http != null)
                {
                    http.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    http.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    http.UserAgent = "Lea/0.3";
                    http.Timeout = 15000;
                    http.ReadWriteTimeout = 15000;
                }
                return request;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                return Capture(base.GetWebResponse(request));
            }

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                return Capture(base.GetWebResponse(request, result));
            }

            private WebResponse Capture(WebResponse response)
            {
                var http = response as HttpWebResponse;
                if (http == null) return response;
                U = http.ResponseUri;
                St = (int)http.StatusCode;
                Ct = http.ContentType ?? "";
                Ce = http.Headers[HttpResponseHeader.ContentEncoding] ?? "";
                var control = http.Headers[HttpResponseHeader.CacheControl] ?? "";
                Ns = control.IndexOf("no-store", StringComparison.OrdinalIgnoreCase) >= 0;
                return response;
            }
        }

        private static Uri ToUri(string input, Uri baseUrl)
        {
            input = (input ?? "").Trim().Trim('"');
            Uri u;
            if (Uri.TryCreate(input, UriKind.Absolute, out u)) return u;
            if (baseUrl != null && Uri.TryCreate(baseUrl, input, out u)) return u;
            if (File.Exists(input)) return new Uri(Path.GetFullPath(input));
            return new Uri("https://" + input);
        }

        private static string Local(Uri url)
        {
            var path = url.LocalPath;
            var query = path.IndexOf('?');
            return query < 0 ? path : path.Substring(0, query);
        }

        private static string Query(Uri url)
        {
            var text = url.AbsoluteUri;
            var start = text.IndexOf('?');
            var mark = 1;
            if (start < 0 && url.IsFile)
            {
                start = text.IndexOf("%3F", StringComparison.OrdinalIgnoreCase);
                mark = 3;
            }
            if (start < 0) return "";
            var end = text.IndexOf('#', start);
            return end < 0 ? text.Substring(start + mark) : text.Substring(start + mark, end - start - mark);
        }

        private static string Clean(string text)
        {
            return Regex.Replace(WebUtility.HtmlDecode(text ?? ""), "\\s+", " ").Trim();
        }
    }
}
