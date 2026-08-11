using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Lea.Engine
{
    // Lea's .NET Framework-compatible engine surface.
    public sealed class E
    {
        private readonly WebClient web = new WebClient();
        private readonly List<P> hs = new List<P>();
        private int at = -1;

        public P At { get { return at >= 0 && at < hs.Count ? hs[at] : null; } }
        public bool CanBk { get { return at > 0; } }
        public bool CanFw { get { return at < hs.Count - 1; } }

        public P Go(string input)
        {
            var p = Read(ToUri(input, At == null ? null : At.U));
            if (at < hs.Count - 1) hs.RemoveRange(at + 1, hs.Count - at - 1);
            hs.Add(p);
            at = hs.Count - 1;
            return p;
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

        private P Read(Uri url)
        {
            web.Headers[HttpRequestHeader.UserAgent] = "Lea/0.1";
            var html = url.IsFile ? File.ReadAllText(url.LocalPath) : web.DownloadString(url);
            return Parse(url, html);
        }

        private static P Parse(Uri url, string html)
        {
            var p = new P();
            p.U = url;
            p.H = html;
            p.T = Clean(First(html, "title"));
            if (p.T.Length == 0) p.T = url.IsFile ? Path.GetFileName(url.LocalPath) : url.Host;

            var body = Regex.Replace(html, "<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            body = Regex.Replace(body, "<[^>]+>", " ");
            p.X = Clean(body);
            p.N = Regex.Matches(html, "<(?<tag>[a-z][a-z0-9]*)\\b", RegexOptions.IgnoreCase).Count;

            foreach (Match m in Regex.Matches(html, "<(?<tag>h1|h2|h3|p|li|pre|code|button)(?:\\s+[^>]*)?>(?<body>.*?)</\\k<tag>>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var text = Clean(Regex.Replace(m.Groups["body"].Value, "<[^>]+>", " "));
                if (text.Length > 0) p.Bs.Add(new B { K = m.Groups["tag"].Value.ToLowerInvariant(), X = text });
            }

            foreach (Match m in Regex.Matches(html, "<a\\s+[^>]*href\\s*=\\s*[\"']?(?<href>[^\"'\\s>]+)[\"']?[^>]*>(?<body>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                Uri link;
                if (!Uri.TryCreate(url, WebUtility.HtmlDecode(m.Groups["href"].Value), out link)) continue;
                var text = Clean(Regex.Replace(m.Groups["body"].Value, "<[^>]+>", " "));
                p.Ls.Add(new Ln { U = link, T = text });
                p.Bs.Add(new B { K = "a", X = text, U = link });
            }

            return p;
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

        private static string First(string html, string tag)
        {
            var m = Regex.Match(html, "<" + tag + "[^>]*>(.*?)</" + tag + ">", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return m.Success ? m.Groups[1].Value : "";
        }

        private static string Clean(string text)
        {
            return Regex.Replace(WebUtility.HtmlDecode(text ?? ""), "\\s+", " ").Trim();
        }
    }
}
