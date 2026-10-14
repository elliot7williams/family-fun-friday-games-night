using System.Net;

namespace Lea.Core;

/// <summary>Builds a lightweight DOM tree from HTML tokens; this is not a full WHATWG tree builder.</summary>
public static class HtmlParser
{
    private static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta",
        "param", "source", "track", "wbr"
    };

    private static readonly HashSet<string> ParagraphClosingElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "address", "article", "aside", "blockquote", "div", "dl", "fieldset", "footer", "form",
        "h1", "h2", "h3", "h4", "h5", "h6", "header", "hr", "main", "nav", "ol", "p",
        "pre", "section", "table", "ul"
    };

    public static HtmlNode Parse(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        var document = HtmlNode.CreateDocument();
        var openElements = new List<HtmlNode> { document };
        var tokenizer = new HtmlTokenizer();

        foreach (var token in tokenizer.Tokenize(html))
        {
            switch (token.Type)
            {
                case HtmlTokenType.Text:
                    AppendText(openElements[^1], WebUtility.HtmlDecode(token.Value));
                    break;

                case HtmlTokenType.Comment:
                    openElements[^1].AppendChild(HtmlNode.CreateComment(token.Value));
                    break;

                case HtmlTokenType.DocumentType:
                    document.AppendChild(HtmlNode.CreateDocumentType(token.Value));
                    break;

                case HtmlTokenType.StartTag:
                    CloseImpliedElements(openElements, token.Name);
                    var element = HtmlNode.CreateElement(token.Name, token.Attributes);
                    openElements[^1].AppendChild(element);
                    if (!token.SelfClosing && !VoidElements.Contains(token.Name))
                        openElements.Add(element);
                    break;

                case HtmlTokenType.EndTag:
                    CloseElement(openElements, token.Name);
                    break;
            }
        }

        return document;
    }

    private static void AppendText(HtmlNode parent, string text)
    {
        if (text.Length == 0)
            return;

        parent.AppendChild(HtmlNode.CreateText(text));
    }

    private static void CloseImpliedElements(List<HtmlNode> openElements, string newName)
    {
        if (ParagraphClosingElements.Contains(newName))
            CloseNearest(openElements, "p");

        switch (newName)
        {
            case "li":
                CloseNearest(openElements, "li");
                break;
            case "dt":
            case "dd":
                CloseNearest(openElements, "dt", "dd");
                break;
            case "option":
                CloseNearest(openElements, "option");
                break;
            case "optgroup":
                CloseNearest(openElements, "option");
                CloseNearest(openElements, "optgroup");
                break;
            case "tr":
                CloseNearest(openElements, "tr");
                break;
            case "td":
            case "th":
                CloseNearest(openElements, "td", "th");
                break;
            case "thead":
            case "tbody":
            case "tfoot":
                CloseNearest(openElements, "td", "th");
                CloseNearest(openElements, "tr");
                CloseNearest(openElements, "thead", "tbody", "tfoot");
                break;
        }
    }

    private static void CloseElement(List<HtmlNode> openElements, string name) => CloseNearest(openElements, name);

    private static void CloseNearest(List<HtmlNode> openElements, params string[] names)
    {
        for (var index = openElements.Count - 1; index > 0; index--)
        {
            if (Array.IndexOf(names, openElements[index].Name) >= 0)
            {
                openElements.RemoveRange(index, openElements.Count - index);
                break;
            }
        }
    }
}
