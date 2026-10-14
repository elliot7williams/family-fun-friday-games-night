namespace Lea.Core;

public static class LeaParser
{
    private static readonly HashSet<string> BlockElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "address", "article", "aside", "blockquote", "body", "dd", "div", "dl", "dt", "fieldset",
        "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "header",
        "hr", "html", "li", "main", "nav", "ol", "p", "pre", "section", "table", "tr", "ul"
    };

    private static readonly HashSet<string> HiddenElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "head", "script", "style", "noscript", "template"
    };

    public static Page Parse(Uri url, string html)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(html);

        var document = HtmlParser.Parse(html);
        var titleNode = document.Descendants().FirstOrDefault(node =>
            node.Type == HtmlNodeType.Element && node.Name.Equals("title", StringComparison.OrdinalIgnoreCase));
        var title = CleanText(titleNode?.TextContent ?? "");
        var links = GetLinks(url, document);

        return new Page
        {
            Url = url,
            Html = html,
            Document = document,
            Title = string.IsNullOrWhiteSpace(title) ? url.Host : title,
            Text = RenderText(document),
            Links = links
        };
    }

    private static List<Link> GetLinks(Uri baseUrl, HtmlNode document)
    {
        var links = new List<Link>();
        foreach (var node in document.Descendants())
        {
            if (node.Type != HtmlNodeType.Element || !node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                continue;

            var href = node.GetAttribute("href")?.Trim();
            if (string.IsNullOrWhiteSpace(href) || href.StartsWith('#'))
                continue;

            if (!Uri.TryCreate(baseUrl, href, out var target) ||
                (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps))
                continue;

            var label = CleanText(node.TextContent);
            links.Add(new Link
            {
                Text = Shorten(string.IsNullOrWhiteSpace(label) ? target.Host : label, 70),
                Url = target
            });
        }

        return links;
    }

    private static string RenderText(HtmlNode document)
    {
        var output = new System.Text.StringBuilder();
        AppendVisibleText(document, output);
        return output.ToString().Trim();
    }

    private static void AppendVisibleText(HtmlNode node, System.Text.StringBuilder output)
    {
        if (node.Type == HtmlNodeType.Text)
        {
            AppendCollapsedText(node.Value, output);
            return;
        }

        if (node.Type != HtmlNodeType.Document && node.Type != HtmlNodeType.Element)
            return;

        if (node.Type == HtmlNodeType.Element && HiddenElements.Contains(node.Name))
            return;

        if (node.Type == HtmlNodeType.Element && node.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
        {
            AppendLineBreak(output);
            return;
        }

        var isBlock = node.Type == HtmlNodeType.Element && BlockElements.Contains(node.Name);
        if (isBlock)
            AppendLineBreak(output);

        foreach (var child in node.Children)
            AppendVisibleText(child, output);

        if (isBlock)
            AppendLineBreak(output);
    }

    private static void AppendCollapsedText(string value, System.Text.StringBuilder output)
    {
        var pendingSpace = false;
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = true;
                continue;
            }

            if (pendingSpace && output.Length > 0 && output[^1] is not (' ' or '\n'))
                output.Append(' ');
            pendingSpace = false;
            output.Append(character);
        }

        if (pendingSpace && output.Length > 0 && output[^1] is not (' ' or '\n'))
            output.Append(' ');
    }

    private static void AppendLineBreak(System.Text.StringBuilder output)
    {
        while (output.Length > 0 && output[^1] == ' ')
            output.Remove(output.Length - 1, 1);

        if (output.Length > 0 && !output.ToString().EndsWith("\n\n", StringComparison.Ordinal))
        {
            if (!output.ToString().EndsWith('\n'))
                output.Append('\n');
            output.Append('\n');
        }
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        AppendCollapsedText(text, sb);
        return sb.ToString().Trim();
    }

    private static string Shorten(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }
}
