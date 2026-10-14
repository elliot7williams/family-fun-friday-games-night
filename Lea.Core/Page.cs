namespace Lea.Core;

public sealed class Page
{
    public required Uri Url { get; init; }
    public required string Html { get; init; }
    public required string Title { get; init; }
    public required string Text { get; init; }
    public required List<Link> Links { get; init; }
    public required HtmlNode Document { get; init; }
}
