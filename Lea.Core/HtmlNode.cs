using System.Collections.ObjectModel;
using System.Net;
using System.Text;

namespace Lea.Core;

public enum HtmlNodeType
{
    Document,
    Element,
    Text,
    Comment,
    DocumentType
}

public sealed class HtmlNode
{
    private readonly List<HtmlNode> children = [];
    private readonly ReadOnlyCollection<HtmlNode> childrenView;
    private readonly Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReadOnlyDictionary<string, string> attributesView;

    internal HtmlNode(HtmlNodeType type, string name, string value = "")
    {
        Type = type;
        Name = name;
        Value = value;
        childrenView = children.AsReadOnly();
        attributesView = new ReadOnlyDictionary<string, string>(attributes);
    }

    public HtmlNodeType Type { get; }
    public string Name { get; }
    public string Value { get; }
    public HtmlNode? Parent { get; private set; }
    public IReadOnlyList<HtmlNode> Children => childrenView;
    public IReadOnlyDictionary<string, string> Attributes => attributesView;

    public string? GetAttribute(string name) => attributes.TryGetValue(name, out var value) ? value : null;

    public bool HasAttribute(string name) => attributes.ContainsKey(name);

    public string TextContent
    {
        get
        {
            var text = new StringBuilder();
            AppendTextContent(this, text);
            return text.ToString();
        }
    }

    public IEnumerable<HtmlNode> Descendants()
    {
        foreach (var child in children)
        {
            yield return child;
            foreach (var descendant in child.Descendants())
                yield return descendant;
        }
    }

    internal static HtmlNode CreateDocument() => new(HtmlNodeType.Document, "#document");

    internal static HtmlNode CreateElement(string name, IEnumerable<HtmlAttribute> sourceAttributes)
    {
        var node = new HtmlNode(HtmlNodeType.Element, name);
        foreach (var attribute in sourceAttributes)
            node.attributes.TryAdd(attribute.Name, WebUtility.HtmlDecode(attribute.Value));
        return node;
    }

    internal static HtmlNode CreateText(string value) => new(HtmlNodeType.Text, "#text", value);
    internal static HtmlNode CreateComment(string value) => new(HtmlNodeType.Comment, "#comment", value);
    internal static HtmlNode CreateDocumentType(string value) => new(HtmlNodeType.DocumentType, "!doctype", value);

    internal void AppendChild(HtmlNode child)
    {
        if (child.Parent is not null)
            throw new InvalidOperationException("An HTML node cannot be attached to more than one parent.");

        child.Parent = this;
        children.Add(child);
    }

    private static void AppendTextContent(HtmlNode node, StringBuilder target)
    {
        if (node.Type == HtmlNodeType.Text)
        {
            target.Append(node.Value);
            return;
        }

        foreach (var child in node.children)
            AppendTextContent(child, target);
    }
}
