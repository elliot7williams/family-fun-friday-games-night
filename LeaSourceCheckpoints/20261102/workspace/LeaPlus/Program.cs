using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LeaPlus;

internal sealed class PlusPage
{
    public required Uri Url { get; init; }
    public required string Html { get; init; }
    public required string Title { get; set; }
    public required List<PlusNode> Nodes { get; init; }
    public required List<PlusLink> Links { get; init; }
    public Color Background { get; set; } = Color.White;
    public Color TextColor { get; set; } = Color.FromArgb(32, 32, 32);
    public int FontSize { get; set; } = 10;
}

internal sealed class PlusNode
{
    public required string Kind { get; init; }
    public required string Text { get; init; }
    public Uri? Url { get; init; }
    public string? Script { get; init; }
    public Color? Color { get; init; }
    public int? FontSize { get; init; }
}

internal sealed class PlusLink
{
    public required string Text { get; init; }
    public required Uri Url { get; init; }
}

internal sealed class PlusEngine
{
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(12) };
    private readonly List<PlusPage> history = [];
    private int cursor = -1;

    public PlusPage? Current => cursor >= 0 && cursor < history.Count ? history[cursor] : null;
    public bool CanBack => cursor > 0;
    public bool CanForward => cursor < history.Count - 1;

    public async Task<PlusPage> OpenAsync(string input)
    {
        var url = ToUri(input);
        var html = await http.GetStringAsync(url);
        var page = PlusParser.Parse(url, html);

        if (cursor < history.Count - 1)
        {
            history.RemoveRange(cursor + 1, history.Count - cursor - 1);
        }

        history.Add(page);
        cursor = history.Count - 1;
        return page;
    }

    public async Task<PlusPage?> ReloadAsync()
    {
        if (Current is null)
        {
            return null;
        }

        var page = PlusParser.Parse(Current.Url, await http.GetStringAsync(Current.Url));
        history[cursor] = page;
        return page;
    }

    public PlusPage? Back()
    {
        if (CanBack)
        {
            cursor--;
        }

        return Current;
    }

    public PlusPage? Forward()
    {
        if (CanForward)
        {
            cursor++;
        }

        return Current;
    }

    public async Task<PlusPage?> FollowAsync(Uri? url)
    {
        return url is null ? null : await OpenAsync(url.ToString());
    }

    private static Uri ToUri(string input)
    {
        return Uri.TryCreate(input, UriKind.Absolute, out var url) ? url : new Uri("https://" + input.Trim());
    }
}

internal static partial class PlusParser
{
    public static PlusPage Parse(Uri url, string html)
    {
        var page = new PlusPage
        {
            Url = url,
            Html = html,
            Title = Clean(First(html, "title")),
            Nodes = [],
            Links = []
        };

        if (string.IsNullOrWhiteSpace(page.Title))
        {
            page.Title = url.Host;
        }

        ApplyCss(page, html);
        RunTinyScript(page, html);

        var body = BodyRegex().Match(html);
        var content = body.Success ? body.Groups[1].Value : html;
        content = NoiseRegex().Replace(content, " ");

        foreach (Match match in ElementRegex().Matches(content))
        {
            var tag = match.Groups["tag"].Value.ToLowerInvariant();
            var attrs = match.Groups["attrs"].Value;
            var inner = match.Groups["inner"].Value;
            var text = Clean(TagRegex().Replace(inner, " "));
            var styleColor = ReadColor(ReadAttr(attrs, "style"));
            var styleSize = ReadFontSize(ReadAttr(attrs, "style"));

            if (tag == "a")
            {
                var href = ReadAttr(attrs, "href");
                if (Uri.TryCreate(url, href, out var linkUrl))
                {
                    var label = string.IsNullOrWhiteSpace(text) ? linkUrl.Host : text;
                    page.Links.Add(new PlusLink { Text = Shorten(label, 80), Url = linkUrl });
                    page.Nodes.Add(new PlusNode { Kind = "a", Text = Shorten(label, 160), Url = linkUrl, Color = styleColor, FontSize = styleSize });
                }
            }
            else if (tag == "img")
            {
                var src = ReadAttr(attrs, "src");
                if (Uri.TryCreate(url, src, out var imageUrl))
                {
                    page.Nodes.Add(new PlusNode { Kind = "img", Text = imageUrl.ToString(), Url = imageUrl });
                }
            }
            else if (tag == "button")
            {
                page.Nodes.Add(new PlusNode { Kind = "button", Text = Shorten(text, 120), Script = ReadAttr(attrs, "onclick"), Color = styleColor, FontSize = styleSize });
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                page.Nodes.Add(new PlusNode { Kind = tag, Text = Shorten(text, 500), Color = styleColor, FontSize = styleSize });
            }
        }

        if (page.Nodes.Count == 0)
        {
            page.Nodes.Add(new PlusNode { Kind = "p", Text = Shorten(Clean(TagRegex().Replace(content, " ")), 3000) });
        }

        return page;
    }

    private static void ApplyCss(PlusPage page, string html)
    {
        var css = string.Join("\n", StyleRegex().Matches(html).Select(m => m.Groups[1].Value));
        var body = BodyCssRegex().Match(css);
        if (!body.Success)
        {
            return;
        }

        var rules = body.Groups[1].Value;
        page.Background = ReadColor(rules, "background", page.Background);
        page.TextColor = ReadColor(rules, "color", page.TextColor);
        page.FontSize = ReadFontSize(rules) ?? page.FontSize;
    }

    private static void RunTinyScript(PlusPage page, string html)
    {
        var scripts = string.Join("\n", ScriptRegex().Matches(html).Select(m => m.Groups[1].Value));
        var title = Regex.Match(scripts, @"document\.title\s*=\s*['""](?<v>.*?)['""]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (title.Success)
        {
            page.Title = Clean(title.Groups["v"].Value);
        }
    }

    public static void RunClickScript(string? script, IWin32Window owner)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }

        var alert = Regex.Match(script, @"alert\s*\(\s*['""](?<v>.*?)['""]\s*\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (alert.Success)
        {
            MessageBox.Show(owner, WebUtility.HtmlDecode(alert.Groups["v"].Value), "LeaPlus Script");
        }
    }

    private static string First(string html, string tag)
    {
        var match = Regex.Match(html, $@"<{tag}[^>]*>(.*?)</{tag}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : "";
    }

    private static string ReadAttr(string attrs, string name)
    {
        var match = Regex.Match(attrs, name + @"\s*=\s*[""'](?<v>.*?)[""']", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["v"].Value) : "";
    }

    private static Color? ReadColor(string style)
    {
        var match = Regex.Match(style, @"(?:^|;)\s*color\s*:\s*(?<v>#[0-9a-f]{6}|[a-z]+)", RegexOptions.IgnoreCase);
        return match.Success ? TryColor(match.Groups["v"].Value) : null;
    }

    private static Color ReadColor(string style, string prop, Color fallback)
    {
        var match = Regex.Match(style, prop + @"(?:-color)?\s*:\s*(?<v>#[0-9a-f]{6}|[a-z]+)", RegexOptions.IgnoreCase);
        return match.Success ? TryColor(match.Groups["v"].Value) ?? fallback : fallback;
    }

    private static Color? TryColor(string value)
    {
        try
        {
            return ColorTranslator.FromHtml(value);
        }
        catch
        {
            return null;
        }
    }

    private static int? ReadFontSize(string style)
    {
        var match = Regex.Match(style, @"font-size\s*:\s*(?<v>\d+)px", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups["v"].Value) : null;
    }

    private static string Clean(string text)
    {
        return SpaceRegex().Replace(WebUtility.HtmlDecode(text), " ").Trim();
    }

    private static string Shorten(string text, int max)
    {
        return text.Length <= max ? text : text[..max].TrimEnd() + "...";
    }

    [GeneratedRegex("<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BodyRegex();

    [GeneratedRegex("<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex NoiseRegex();

    [GeneratedRegex("<(?<tag>h1|h2|h3|p|li|a|button|img|div|section|article)(?<attrs>[^>]*)>(?<inner>.*?)</\\k<tag>>|<(?<tag>img)(?<attrs>[^>]*)/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ElementRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex SpaceRegex();

    [GeneratedRegex("<style[^>]*>(.*?)</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleRegex();

    [GeneratedRegex("body\\s*\\{(.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BodyCssRegex();

    [GeneratedRegex("<script[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptRegex();
}

internal sealed class PlusBrowser : Form
{
    private readonly PlusEngine engine = new();
    private readonly TextBox address = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
    private readonly Button back = new() { Text = "<", Dock = DockStyle.Fill };
    private readonly Button forward = new() { Text = ">", Dock = DockStyle.Fill };
    private readonly Button reload = new() { Text = "R", Dock = DockStyle.Fill };
    private readonly Button go = new() { Text = "Go", Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel page = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
    private readonly StatusStrip status = new();
    private readonly ToolStripStatusLabel label = new() { Text = "Ready" };

    public PlusBrowser()
    {
        Text = "LeaPlus";
        MinimumSize = new Size(920, 620);
        StartPosition = FormStartPosition.CenterScreen;
        Build();
        UpdateButtons();
    }

    private void Build()
    {
        var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 5, Padding = new Padding(8) };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        top.Controls.Add(back, 0, 0);
        top.Controls.Add(forward, 1, 0);
        top.Controls.Add(reload, 2, 0);
        top.Controls.Add(address, 3, 0);
        top.Controls.Add(go, 4, 0);

        back.Click += (_, _) => Render(engine.Back());
        forward.Click += (_, _) => Render(engine.Forward());
        reload.Click += async (_, _) => await LoadAsync(() => engine.ReloadAsync());
        go.Click += async (_, _) => await LoadAsync(() => engine.OpenAsync(address.Text));
        address.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadAsync(() => engine.OpenAsync(address.Text));
            }
        };

        page.Resize += (_, _) =>
        {
            foreach (Control child in page.Controls)
            {
                child.Width = page.ClientSize.Width - 28;
            }
        };

        status.Items.Add(label);
        Controls.Add(page);
        Controls.Add(top);
        Controls.Add(status);
    }

    private async Task LoadAsync(Func<Task<PlusPage?>> load)
    {
        try
        {
            UseWaitCursor = true;
            label.Text = "Loading...";
            Render(await load());
        }
        catch (Exception ex)
        {
            label.Text = "Error: " + ex.Message;
        }
        finally
        {
            UseWaitCursor = false;
            UpdateButtons();
        }
    }

    private void Render(PlusPage? loaded)
    {
        if (loaded is null)
        {
            UpdateButtons();
            return;
        }

        Text = loaded.Title + " - LeaPlus";
        address.Text = loaded.Url.ToString();
        page.BackColor = loaded.Background;
        page.Controls.Clear();

        foreach (var node in loaded.Nodes)
        {
            page.Controls.Add(MakeControl(node, loaded));
        }

        label.Text = $"{loaded.Nodes.Count} nodes, {loaded.Links.Count} links";
        UpdateButtons();
    }

    private Control MakeControl(PlusNode node, PlusPage loaded)
    {
        var width = Math.Max(320, page.ClientSize.Width - 28);
        var fontSize = node.FontSize ?? (node.Kind is "h1" ? 22 : node.Kind is "h2" ? 17 : loaded.FontSize);

        if (node.Kind == "button")
        {
            var button = new Button { Text = node.Text, Width = width, Height = 34, TextAlign = ContentAlignment.MiddleLeft };
            button.Click += (_, _) => PlusParser.RunClickScript(node.Script, this);
            return button;
        }

        if (node.Kind == "img")
        {
            var link = new LinkLabel { Text = "[image] " + node.Text, Width = width, AutoSize = false, Height = 30 };
            link.Click += async (_, _) => await LoadAsync(() => engine.FollowAsync(node.Url));
            return link;
        }

        if (node.Kind == "a")
        {
            var link = new LinkLabel { Text = node.Text, Width = width, AutoSize = false, Height = 28, Font = new Font("Segoe UI", fontSize) };
            link.Click += async (_, _) => await LoadAsync(() => engine.FollowAsync(node.Url));
            return link;
        }

        var labelControl = new Label
        {
            Text = node.Text,
            Width = width,
            AutoSize = false,
            ForeColor = node.Color ?? loaded.TextColor,
            Font = new Font("Segoe UI", fontSize, node.Kind is "h1" or "h2" or "h3" ? FontStyle.Bold : FontStyle.Regular),
            Padding = new Padding(6),
            Height = node.Kind is "h1" ? 48 : node.Kind is "h2" ? 40 : 54
        };

        return labelControl;
    }

    private void UpdateButtons()
    {
        back.Enabled = engine.CanBack;
        forward.Enabled = engine.CanForward;
        reload.Enabled = engine.Current is not null;
    }
}

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PlusBrowser());
    }
}
