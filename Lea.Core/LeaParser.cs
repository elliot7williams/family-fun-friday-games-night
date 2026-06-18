using System.Net;
using System.Text.RegularExpressions;

namespace Lea.Core;

public static partial class LeaParser
{
    public static Page Parse(Uri url, string html)
    {
        var cleaned = StripNoise(html);
        var links = GetLinks(url, cleaned);
        var title = ReadFirst(cleaned, "title");
        var text = RenderText(cleaned);

        return new Page
        {
            Url = url,
            Html = html,
            Title = string.IsNullOrWhiteSpace(title) ? url.Host : title,
            Text = text,
            Links = links
        };
    }

    private static string StripNoise(string html)
    {
        html = ScriptStyleCommentRegex().Replace(html, " ");
        html = BlockBreakRegex().Replace(html, "\n");
        return html;
    }

    private static List<Link> GetLinks(Uri baseUrl, string html)
    {
        var links = new List<Link>();

        foreach (Match match in AnchorRegex().Matches(html))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value).Trim();
            var label = TagsRegex().Replace(match.Groups["body"].Value, " ");
            label = CleanText(label);

            if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (!Uri.TryCreate(baseUrl, href, out var url))
            {
                continue;
            }

            links.Add(new Link
            {
                Text = string.IsNullOrWhiteSpace(label) ? url.Host : Shorten(label, 70),
                Url = url
            });
        }

        return links;
    }

    private static string ReadFirst(string html, string tag)
    {
        var pattern = $@"<{tag}[^>]*>(?<text>.*?)</{tag}>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? CleanText(match.Groups["text"].Value) : "";
    }

    private static string RenderText(string html)
    {
        html = TagsRegex().Replace(html, " ");
        var text = CleanText(html);
        return Shorten(text, 4000);
    }

    private static string CleanText(string text)
    {
        text = WebUtility.HtmlDecode(text);
        return WhitespaceRegex().Replace(text, " ").Trim();
    }

    private static string Shorten(string text, int max)
    {
        if (text.Length <= max)
        {
            return text;
        }

        return text[..max].TrimEnd() + "...";
    }

    [GeneratedRegex("<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptStyleCommentRegex();

    [GeneratedRegex("</(p|div|section|article|header|footer|h1|h2|h3|li|br)>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBreakRegex();

    [GeneratedRegex("<a\\s+[^>]*href\\s*=\\s*[\"']?(?<href>[^\"'\\s>]+)[\"']?[^>]*>(?<body>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnchorRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagsRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();
}
