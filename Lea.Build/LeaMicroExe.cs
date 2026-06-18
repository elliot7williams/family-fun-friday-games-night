using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace LeaMicroExe
{
    internal sealed class Link
    {
        public string Text;
        public Uri Url;
    }

    internal sealed class Page
    {
        public Uri Url;
        public string Title;
        public string Text;
        public List<Link> Links;
    }

    internal static class Program
    {
        private static readonly WebClient Web = new WebClient();
        private static readonly List<Page> History = new List<Page>();
        private static int index = -1;

        [STAThread]
        private static void Main()
        {
            Console.Title = "Lea Micro";
            Console.WriteLine("Lea Micro: u site | g n | b | f | r | l | q");

            while (true)
            {
                Console.Write("lm> ");
                var line = Console.ReadLine();
                if (line == null || !Run(line))
                {
                    return;
                }
            }
        }

        private static bool Run(string line)
        {
            var parts = line.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return true;
            }

            var command = parts[0].ToLowerInvariant();
            var value = parts.Length > 1 ? parts[1] : "";

            try
            {
                if (command == "u")
                {
                    Show(Open(value));
                }
                else if (command == "g")
                {
                    var link = Current().Links[int.Parse(value) - 1];
                    Show(Open(link.Url.ToString()));
                }
                else if (command == "b")
                {
                    if (index > 0) index--;
                    Show(Current());
                }
                else if (command == "f")
                {
                    if (index < History.Count - 1) index++;
                    Show(Current());
                }
                else if (command == "r")
                {
                    var page = Current();
                    if (page != null) Show(Reload(page.Url));
                }
                else if (command == "l")
                {
                    ShowLinks(Current());
                }
                else if (command == "q")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("u/g/b/f/r/l/q");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No: " + ex.Message);
            }

            return true;
        }

        private static Page Open(string input)
        {
            var url = ToUri(input);
            var page = Parse(url, Web.DownloadString(url));

            if (index < History.Count - 1)
            {
                History.RemoveRange(index + 1, History.Count - index - 1);
            }

            History.Add(page);
            index = History.Count - 1;
            return page;
        }

        private static Page Reload(Uri url)
        {
            var page = Parse(url, Web.DownloadString(url));
            History[index] = page;
            return page;
        }

        private static Page Parse(Uri url, string html)
        {
            html = Regex.Replace(html, "<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var links = new List<Link>();
            foreach (Match match in Regex.Matches(html, "<a\\s+[^>]*href\\s*=\\s*[\"']?([^\"'\\s>]+)[\"']?[^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                Uri linkUrl;
                if (!Uri.TryCreate(url, WebUtility.HtmlDecode(match.Groups[1].Value), out linkUrl))
                {
                    continue;
                }

                var label = Clean(Regex.Replace(match.Groups[2].Value, "<[^>]+>", " "));
                links.Add(new Link { Text = label.Length == 0 ? linkUrl.Host : Cut(label, 60), Url = linkUrl });
            }

            var titleMatch = Regex.Match(html, "<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var title = Clean(titleMatch.Groups[1].Value);
            var text = Cut(Clean(Regex.Replace(html, "<[^>]+>", " ")), 2500);

            return new Page
            {
                Url = url,
                Title = title.Length == 0 ? url.Host : title,
                Text = text,
                Links = links
            };
        }

        private static void Show(Page page)
        {
            if (page == null) return;
            Console.WriteLine();
            Console.WriteLine(page.Title);
            Console.WriteLine(page.Url);
            Console.WriteLine(new string('-', 56));
            Console.WriteLine(page.Text);
            ShowLinks(page);
        }

        private static void ShowLinks(Page page)
        {
            if (page == null) return;
            for (var i = 0; i < Math.Min(15, page.Links.Count); i++)
            {
                Console.WriteLine((i + 1) + ". " + page.Links[i].Text);
            }
        }

        private static Page Current()
        {
            return index >= 0 && index < History.Count ? History[index] : null;
        }

        private static Uri ToUri(string input)
        {
            Uri uri;
            return Uri.TryCreate(input, UriKind.Absolute, out uri) ? uri : new Uri("https://" + input);
        }

        private static string Clean(string text)
        {
            return Regex.Replace(WebUtility.HtmlDecode(text), "\\s+", " ").Trim();
        }

        private static string Cut(string text, int max)
        {
            return text.Length <= max ? text : text.Substring(0, max).TrimEnd() + "...";
        }
    }
}
