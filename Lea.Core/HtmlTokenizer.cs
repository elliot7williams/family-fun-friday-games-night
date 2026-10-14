namespace Lea.Core;

public enum HtmlTokenType
{
    StartTag,
    EndTag,
    Text,
    Comment,
    DocumentType
}

public sealed record HtmlAttribute(string Name, string Value);

public sealed record HtmlToken(
    HtmlTokenType Type,
    string Name,
    string Value,
    IReadOnlyList<HtmlAttribute> Attributes,
    bool SelfClosing);

/// <summary>A small, dependency-free tokenizer for ordinary HTML documents.</summary>
public sealed class HtmlTokenizer
{
    private static readonly HashSet<string> RawTextElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "script", "style", "textarea", "title", "xmp", "iframe", "noembed", "noframes"
    };

    public IEnumerable<HtmlToken> Tokenize(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var position = 0;
        string? rawTextElement = null;

        while (position < source.Length)
        {
            if (rawTextElement is not null)
            {
                var closingTag = FindRawTextClosingTag(source, position, rawTextElement);
                if (closingTag < 0)
                {
                    yield return TextToken(source[position..]);
                    yield break;
                }

                if (closingTag > position)
                    yield return TextToken(source[position..closingTag]);

                position = closingTag;
                rawTextElement = null;
            }

            if (source[position] != '<')
            {
                var nextTag = source.IndexOf('<', position);
                if (nextTag < 0)
                    nextTag = source.Length;
                yield return TextToken(source[position..nextTag]);
                position = nextTag;
                continue;
            }

            if (source.IndexOf("<!--", position, StringComparison.Ordinal) == position)
            {
                var contentStart = position + 4;
                var close = source.IndexOf("-->", contentStart, StringComparison.Ordinal);
                if (close < 0)
                {
                    yield return new HtmlToken(HtmlTokenType.Comment, "#comment", source[contentStart..], [], false);
                    yield break;
                }

                yield return new HtmlToken(HtmlTokenType.Comment, "#comment", source[contentStart..close], [], false);
                position = close + 3;
                continue;
            }

            if (StartsWithIgnoreCase(source, position, "<!doctype") &&
                (position + 9 == source.Length || char.IsWhiteSpace(source[position + 9]) || source[position + 9] == '>'))
            {
                var contentStart = position + 9;
                var close = source.IndexOf('>', contentStart);
                if (close < 0)
                {
                    yield return new HtmlToken(HtmlTokenType.DocumentType, "!doctype", source[contentStart..].Trim(), [], false);
                    yield break;
                }

                yield return new HtmlToken(HtmlTokenType.DocumentType, "!doctype", source[contentStart..close].Trim(), [], false);
                position = close + 1;
                continue;
            }

            if (source[position..].StartsWith("<!", StringComparison.Ordinal) ||
                source[position..].StartsWith("<?", StringComparison.Ordinal))
            {
                var close = source.IndexOf('>', position + 2);
                if (close < 0)
                {
                    yield return new HtmlToken(HtmlTokenType.Comment, "#comment", source[(position + 2)..], [], false);
                    yield break;
                }

                yield return new HtmlToken(HtmlTokenType.Comment, "#comment", source[(position + 2)..close], [], false);
                position = close + 1;
                continue;
            }

            if (position + 1 < source.Length && source[position + 1] == '/')
            {
                var token = ReadEndTag(source, ref position);
                if (token is not null)
                {
                    yield return token;
                }
                continue;
            }

            var startToken = ReadStartTag(source, ref position);
            if (startToken is not null)
            {
                if (RawTextElements.Contains(startToken.Name))
                {
                    rawTextElement = startToken.Name;
                }
                yield return startToken;
            }
        }
    }

    private static HtmlToken TextToken(string value) =>
        new(HtmlTokenType.Text, "#text", value, [], false);

    private static int FindRawTextClosingTag(string source, int position, string tagName)
    {
        var searchLength = tagName.Length;
        while (position < source.Length)
        {
            var lt = source.IndexOf('<', position);
            if (lt < 0) return -1;

            if (lt + 1 < source.Length && source[lt + 1] == '/')
            {
                var nameStart = lt + 2;
                if (nameStart + searchLength <= source.Length &&
                    string.Equals(source.Substring(nameStart, searchLength), tagName, StringComparison.OrdinalIgnoreCase))
                {
                    var afterName = nameStart + searchLength;
                    if (afterName == source.Length || char.IsWhiteSpace(source[afterName]) || source[afterName] == '>')
                    {
                        return lt;
                    }
                }
            }

            position = lt + 1;
        }

        return -1;
    }

    private static HtmlToken? ReadStartTag(string source, ref int position)
    {
        var start = position;
        position++; // Skip '<'

        position = SkipWhitespace(source, position);
        if (position >= source.Length)
        {
            return new HtmlToken(HtmlTokenType.Text, "#text", source[start..], [], false);
        }

        var nameEnd = position;
        while (nameEnd < source.Length && !char.IsWhiteSpace(source[nameEnd]) && source[nameEnd] != '>' && source[nameEnd] != '/')
        {
            nameEnd++;
        }

        if (nameEnd == position)
        {
            // Not a valid tag name, treat '<' as text
            position = start + 1;
            return TextToken("<");
        }

        var name = source[position..nameEnd].ToLowerInvariant();
        position = nameEnd;

        var attributes = new List<HtmlAttribute>();
        bool selfClosing = false;

        while (position < source.Length)
        {
            position = SkipWhitespace(source, position);
            if (position >= source.Length) break;

            if (source[position] == '>')
            {
                position++;
                break;
            }

            if (position + 1 < source.Length && source[position] == '/' && source[position + 1] == '>')
            {
                selfClosing = true;
                position += 2;
                break;
            }

            if (source[position] == '/')
            {
                position++;
                continue;
            }

            // Read attribute
            var attrNameStart = position;
            while (position < source.Length && !char.IsWhiteSpace(source[position]) && source[position] != '=' && source[position] != '>' && source[position] != '/')
            {
                position++;
            }

            var attrName = source[attrNameStart..position];
            if (attrName.Length == 0)
            {
                position++;
                continue;
            }

            position = SkipWhitespace(source, position);
            string attrValue = string.Empty;

            if (position < source.Length && source[position] == '=')
            {
                position++;
                position = SkipWhitespace(source, position);

                if (position < source.Length)
                {
                    var quote = source[position];
                    if (quote == '"' || quote == '\'')
                    {
                        position++;
                        var valStart = position;
                        while (position < source.Length && source[position] != quote)
                        {
                            position++;
                        }
                        attrValue = source[valStart..position];
                        if (position < source.Length) position++; // Skip closing quote
                    }
                    else
                    {
                        var valStart = position;
                        while (position < source.Length && !char.IsWhiteSpace(source[position]) && source[position] != '>')
                        {
                            position++;
                        }
                        attrValue = source[valStart..position];
                    }
                }
            }

            attributes.Add(new HtmlAttribute(attrName.ToLowerInvariant(), attrValue));
        }

        var fullValue = source[start..position];
        return new HtmlToken(HtmlTokenType.StartTag, name, fullValue, attributes, selfClosing);
    }

    private static HtmlToken? ReadEndTag(string source, ref int position)
    {
        var start = position;
        position += 2; // Skip "</"

        position = SkipWhitespace(source, position);
        if (position >= source.Length)
        {
            position = start + 1;
            return TextToken("<");
        }

        var nameEnd = position;
        while (nameEnd < source.Length && !char.IsWhiteSpace(source[nameEnd]) && source[nameEnd] != '>')
        {
            nameEnd++;
        }

        var name = source[position..nameEnd].ToLowerInvariant();
        position = nameEnd;

        position = SkipWhitespace(source, position);
        if (position < source.Length && source[position] == '>')
        {
            position++;
        }

        var fullValue = source[start..position];
        return new HtmlToken(HtmlTokenType.EndTag, name, fullValue, [], false);
    }

    private static int SkipWhitespace(string source, int position)
    {
        while (position < source.Length && char.IsWhiteSpace(source[position]))
        {
            position++;
        }
        return position;
    }

    private static bool StartsWithIgnoreCase(string source, int position, string value)
    {
        if (position + value.Length > source.Length) return false;
        return string.Equals(source.Substring(position, value.Length), value, StringComparison.OrdinalIgnoreCase);
    }
}
