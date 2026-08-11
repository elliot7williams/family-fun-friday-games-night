using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lea.Engine
{
    // Lea's .NET Framework-compatible engine surface.
    public sealed class E
    {
        private readonly WebClient web = new WebClient();
        private readonly List<P> hs = new List<P>();
        private int at = -1;
        private bool busy;

        public P At { get { return at >= 0 && at < hs.Count ? hs[at] : null; } }
        public bool CanBk { get { return at > 0; } }
        public bool CanFw { get { return at < hs.Count - 1; } }
        public bool Busy { get { return busy; } }

        public P Go(string input)
        {
            var p = Read(ToUri(input, At == null ? null : At.U));
            Push(p);
            return p;
        }

        public async Task<P> GoA(string input)
        {
            var p = await ReadA(ToUri(input, At == null ? null : At.U));
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
            var p = Read(At.U);
            hs[at] = p;
            return p;
        }

        public async Task<P> RlA()
        {
            if (At == null) return null;
            var p = await ReadA(At.U);
            hs[at] = p;
            return p;
        }

        public void St()
        {
            if (busy) web.CancelAsync();
        }

        private P Read(Uri url)
        {
            web.Headers[HttpRequestHeader.UserAgent] = "Lea/0.1";
            var html = url.IsFile ? File.ReadAllText(url.LocalPath) : web.DownloadString(url);
            return Parse(url, html);
        }

        private async Task<P> ReadA(Uri url)
        {
            if (busy) throw new InvalidOperationException("Lea is already loading a page.");
            busy = true;
            try
            {
                if (url.IsFile)
                {
                    var path = url.LocalPath;
                    return await Task.Factory.StartNew(delegate { return Parse(url, File.ReadAllText(path)); });
                }

                web.Headers[HttpRequestHeader.UserAgent] = "Lea/0.2";
                var html = await web.DownloadStringTaskAsync(url);
                return Parse(url, html);
            }
            finally
            {
                busy = false;
            }
        }

        private static P Parse(Uri url, string html)
        {
            var p = new P();
            p.U = url;
            p.H = html;
            p.D = Tree(html);
            var title = Find(p.D, "title");
            p.T = Clean(NodeText(title));
            if (p.T.Length == 0) p.T = url.IsFile ? Path.GetFileName(url.LocalPath) : url.Host;

            var body = Find(p.D, "body") ?? p.D;
            p.X = Clean(NodeText(body));
            p.N = Count(p.D);
            Blocks(p, body, url);

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
                    stack[stack.Count - 1].C.Add(node);
                    if (!value.TrimEnd().EndsWith("/>") && !Void(tag)) stack.Add(node);
                    continue;
                }

                var parent = stack[stack.Count - 1];
                if (parent.K == "script" || parent.K == "style" || parent.K == "noscript") continue;
                if (value.Length > 0) parent.C.Add(new N("#text") { X = WebUtility.HtmlDecode(value) });
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

        private static void Blocks(P page, N node, Uri baseUrl)
        {
            if (node == null) return;
            var kind = node.K;
            if (kind == "a")
            {
                Uri link;
                var href = node.Av("href");
                var text = Clean(NodeText(node));
                if (href.Length > 0 && Uri.TryCreate(baseUrl, href, out link))
                {
                    page.Ls.Add(new Ln { U = link, T = text });
                    page.Bs.Add(new B { K = kind, X = text.Length == 0 ? link.ToString() : text, U = link });
                }
            }
            else if (kind == "h1" || kind == "h2" || kind == "h3" || kind == "p" || kind == "li" || kind == "pre" || kind == "code" || kind == "button")
            {
                var text = Clean(NodeText(node));
                if (text.Length > 0) page.Bs.Add(new B { K = kind, X = text });
            }

            foreach (var child in node.C) Blocks(page, child, baseUrl);
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

        private static string Clean(string text)
        {
            return Regex.Replace(WebUtility.HtmlDecode(text ?? ""), "\\s+", " ").Trim();
        }
    }
}
