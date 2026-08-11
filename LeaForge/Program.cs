using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LeaForge
{
    internal sealed class ForgeNode
    {
        public string Kind = "p";
        public string Text = "";
        public Uri Url;
        public string Script = "";
        public Color? Color;
        public Color? Background;
        public int? FontSize;
        public bool Bold;
    }

    internal sealed class ForgePage
    {
        public Uri Url;
        public string Html = "";
        public string Title = "";
        public List<ForgeNode> Nodes = new List<ForgeNode>();
        public List<ForgeNode> Links = new List<ForgeNode>();
        public Color Background = Color.White;
        public Color TextColor = Color.FromArgb(31, 41, 55);
        public int FontSize = 10;
    }

    internal sealed class ForgeTab
    {
        public readonly List<ForgePage> History = new List<ForgePage>();
        public int Index = -1;
        public ForgePage Current { get { return Index >= 0 && Index < History.Count ? History[Index] : null; } }
        public bool CanBack { get { return Index > 0; } }
        public bool CanForward { get { return Index < History.Count - 1; } }
    }

    internal sealed class ForgeEngine
    {
        private readonly WebClient web = new WebClient();
        private readonly List<string> bookmarks = new List<string>();
        private readonly Dictionary<string, ForgePage> pageCache = new Dictionary<string, ForgePage>(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> cacheOrder = new LinkedList<string>();
        private const int CacheLimit = 12;

        public IList<string> Bookmarks { get { return bookmarks.AsReadOnly(); } }

        public ForgePage Open(ForgeTab tab, string input)
        {
            var url = ToUri(input);
            var key = CacheKey(url);
            ForgePage page;
            if (!pageCache.TryGetValue(key, out page))
            {
                page = Load(url);
                Store(key, page);
            }
            Push(tab, page);
            return page;
        }

        public ForgePage Reload(ForgeTab tab)
        {
            if (tab.Current == null) return null;
            var url = tab.Current.Url;
            var page = Load(url);
            Store(CacheKey(url), page);
            tab.History[tab.Index] = page;
            return page;
        }

        public ForgePage Follow(ForgeTab tab, Uri url)
        {
            return Open(tab, url.ToString());
        }

        public ForgePage Back(ForgeTab tab)
        {
            if (tab.CanBack) tab.Index--;
            return tab.Current;
        }

        public ForgePage Forward(ForgeTab tab)
        {
            if (tab.CanForward) tab.Index++;
            return tab.Current;
        }

        public void AddBookmark(ForgePage page)
        {
            if (page == null) return;
            var item = page.Title + " | " + page.Url;
            if (!bookmarks.Contains(item)) bookmarks.Add(item);
        }

        private ForgePage Load(Uri url)
        {
            web.Headers[HttpRequestHeader.UserAgent] = "LeaForge/0.2";
            var html = url.IsFile ? File.ReadAllText(url.LocalPath) : web.DownloadString(url);
            return ForgeParser.Parse(url, html);
        }

        private void Store(string key, ForgePage page)
        {
            pageCache[key] = page;
            cacheOrder.Remove(key);
            cacheOrder.AddLast(key);
            while (cacheOrder.Count > CacheLimit)
            {
                var oldest = cacheOrder.First.Value;
                cacheOrder.RemoveFirst();
                pageCache.Remove(oldest);
            }
        }

        private static string CacheKey(Uri url)
        {
            return url.IsFile ? Path.GetFullPath(url.LocalPath) : url.AbsoluteUri;
        }

        private static void Push(ForgeTab tab, ForgePage page)
        {
            if (tab.Index < tab.History.Count - 1)
            {
                tab.History.RemoveRange(tab.Index + 1, tab.History.Count - tab.Index - 1);
            }

            tab.History.Add(page);
            tab.Index = tab.History.Count - 1;
        }

        private static Uri ToUri(string input)
        {
            input = input.Trim().Trim('"');
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri)) return uri;
            if (File.Exists(input)) return new Uri(Path.GetFullPath(input));
            return new Uri("https://" + input);
        }
    }

    internal static class ForgeParser
    {
        public static ForgePage Parse(Uri url, string html)
        {
            var page = new ForgePage();
            page.Url = url;
            page.Html = html;
            page.Title = Clean(First(html, "title"));
            if (page.Title.Length == 0) page.Title = url.IsFile ? Path.GetFileName(url.LocalPath) : url.Host;

            ApplyCss(page, html);
            RunPageScript(page, html);

            var bodyMatch = Regex.Match(html, "<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var body = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;
            body = Regex.Replace(body, "<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in Regex.Matches(body, "<(?<tag>h1|h2|h3|p|li|a|button|img|pre|code|div|section|article)(?<attrs>[^>]*)>(?<inner>.*?)</\\k<tag>>|<(?<tag>img)(?<attrs>[^>]*)/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var tag = match.Groups["tag"].Value.ToLowerInvariant();
                var attrs = match.Groups["attrs"].Value;
                var inner = match.Groups["inner"].Value;
                var style = Attr(attrs, "style");
                var node = new ForgeNode();
                node.Kind = tag;
                node.Text = Clean(Regex.Replace(inner, "<[^>]+>", " "));
                node.Color = ReadColor(style, "color");
                node.Background = ReadColor(style, "background");
                node.FontSize = ReadFontSize(style);
                node.Bold = tag == "h1" || tag == "h2" || tag == "h3" || Regex.IsMatch(style, "font-weight\\s*:\\s*(bold|700)", RegexOptions.IgnoreCase);

                if (tag == "a")
                {
                    Uri link;
                    if (Uri.TryCreate(url, Attr(attrs, "href"), out link))
                    {
                        node.Url = link;
                        if (node.Text.Length == 0) node.Text = link.ToString();
                        node.Text = Cut(node.Text, 180);
                        page.Links.Add(node);
                        page.Nodes.Add(node);
                    }
                }
                else if (tag == "img")
                {
                    Uri image;
                    if (Uri.TryCreate(url, Attr(attrs, "src"), out image))
                    {
                        node.Url = image;
                        node.Text = Attr(attrs, "alt");
                        if (node.Text.Length == 0) node.Text = image.ToString();
                        page.Nodes.Add(node);
                    }
                }
                else if (tag == "button")
                {
                    node.Text = Cut(node.Text.Length == 0 ? "Button" : node.Text, 120);
                    node.Script = Attr(attrs, "onclick");
                    page.Nodes.Add(node);
                }
                else if (node.Text.Length > 0)
                {
                    node.Text = Cut(node.Text, tag == "pre" || tag == "code" ? 2000 : 700);
                    page.Nodes.Add(node);
                }
            }

            if (page.Nodes.Count == 0)
            {
                page.Nodes.Add(new ForgeNode { Kind = "p", Text = Cut(Clean(Regex.Replace(body, "<[^>]+>", " ")), 3500) });
            }

            return page;
        }

        public static void RunClickScript(string script, IWin32Window owner, ForgePage page)
        {
            if (script == null) return;
            var alert = Command(script, "a", "alert");
            if (alert.Success)
            {
                MessageBox.Show(owner, WebUtility.HtmlDecode(alert.Groups["v"].Value), "LeaForge Script");
            }

            var title = TitleCommand(script);
            if (title.Success && page != null)
            {
                page.Title = Clean(title.Groups["v"].Value);
            }
            ApplyShortStyle(script, page);
        }

        private static void ApplyCss(ForgePage page, string html)
        {
            var styles = "";
            foreach (Match match in Regex.Matches(html, "<style[^>]*>(.*?)</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                styles += match.Groups[1].Value + "\n";
            }

            var body = Regex.Match(styles, "body\\s*\\{(.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!body.Success) return;

            var rules = body.Groups[1].Value;
            page.Background = ReadColor(rules, "background") ?? page.Background;
            page.TextColor = ReadColor(rules, "color") ?? page.TextColor;
            page.FontSize = ReadFontSize(rules) ?? page.FontSize;
        }

        private static void RunPageScript(ForgePage page, string html)
        {
            foreach (Match script in Regex.Matches(html, "<script[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var title = TitleCommand(script.Groups[1].Value);
                if (title.Success) page.Title = Clean(title.Groups["v"].Value);
                ApplyShortStyle(script.Groups[1].Value, page);
            }
        }

        // Lea shorthand keeps common page commands to one or two letters.
        private static Match Command(string script, string shortName, string longName)
        {
            var pattern = "(?:" + shortName + "|" + longName + ")\\s*\\(\\s*['\"](?<v>.*?)['\"]\\s*\\)";
            return Regex.Match(script, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private static Match TitleCommand(string script)
        {
            return Regex.Match(script, "(?:t\\s*\\(\\s*|document\\.title\\s*=\\s*)['\"](?<v>.*?)['\"](?:\\s*\\))?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private static void ApplyShortStyle(string script, ForgePage page)
        {
            if (page == null) return;
            var bg = Command(script, "bg", "background");
            if (bg.Success) page.Background = ReadColorValue(bg.Groups["v"].Value) ?? page.Background;

            var fg = Command(script, "fg", "color");
            if (fg.Success) page.TextColor = ReadColorValue(fg.Groups["v"].Value) ?? page.TextColor;

            var size = Regex.Match(script, "(?:fs|font-size)\\s*\\(\\s*(?<v>[0-9]{1,2})\\s*\\)", RegexOptions.IgnoreCase);
            int number;
            if (size.Success && int.TryParse(size.Groups["v"].Value, out number) && number > 0 && number < 100)
                page.FontSize = number;
        }

        private static string First(string html, string tag)
        {
            var match = Regex.Match(html, "<" + tag + "[^>]*>(.*?)</" + tag + ">", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : "";
        }

        private static string Attr(string attrs, string name)
        {
            var match = Regex.Match(attrs, name + "\\s*=\\s*[\"'](?<v>.*?)[\"']", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? WebUtility.HtmlDecode(match.Groups["v"].Value) : "";
        }

        private static Color? ReadColor(string style, string prop)
        {
            var match = Regex.Match(style, prop + "(?:-color)?\\s*:\\s*(?<v>#[0-9a-f]{6}|[a-z]+)", RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            return ReadColorValue(match.Groups["v"].Value);
        }

        private static Color? ReadColorValue(string value)
        {
            try { return ColorTranslator.FromHtml(value); }
            catch { return null; }
        }

        private static int? ReadFontSize(string style)
        {
            var match = Regex.Match(style, "font-size\\s*:\\s*(?<v>\\d+)px", RegexOptions.IgnoreCase);
            return match.Success ? int.Parse(match.Groups["v"].Value) : (int?)null;
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

    internal sealed class ForgeWindow : Form
    {
        private readonly ForgeEngine engine = new ForgeEngine();
        private readonly List<ForgeTab> tabs = new List<ForgeTab>();
        private readonly TabControl tabStrip = new TabControl();
        private readonly TextBox address = new TextBox();
        private readonly FlowLayoutPanel page = new FlowLayoutPanel();
        private readonly ListBox side = new ListBox();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ToolStripStatusLabel label = new ToolStripStatusLabel();
        private readonly Button back = new Button();
        private readonly Button forward = new Button();
        private readonly Button reload = new Button();
        private readonly Button go = new Button();

        public ForgeWindow()
        {
            Text = "LeaForge";
            MinimumSize = new Size(1040, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Build();
            NewTab();
        }

        private ForgeTab ActiveTab { get { return tabs[Math.Max(0, tabStrip.SelectedIndex)]; } }

        private void Build()
        {
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 8, Padding = new Padding(8) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));

            Wire(back, "<", delegate { Render(engine.Back(ActiveTab)); });
            Wire(forward, ">", delegate { Render(engine.Forward(ActiveTab)); });
            Wire(reload, "R", delegate { LoadPage(delegate { return engine.Reload(ActiveTab); }); });
            Wire(go, "Go", delegate { LoadPage(delegate { return engine.Open(ActiveTab, address.Text); }); });

            var newTab = MakeButton("+ Tab", delegate { NewTab(); });
            var source = MakeButton("Source", delegate { ShowSource(); });
            var bookmark = MakeButton("Bookmark", delegate { engine.AddBookmark(ActiveTab.Current); RefreshSide(); });

            address.Dock = DockStyle.Fill;
            address.Font = new Font("Segoe UI", 10);
            address.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadPage(delegate { return engine.Open(ActiveTab, address.Text); });
                }
            };

            top.Controls.Add(back, 0, 0);
            top.Controls.Add(forward, 1, 0);
            top.Controls.Add(reload, 2, 0);
            top.Controls.Add(address, 3, 0);
            top.Controls.Add(go, 4, 0);
            top.Controls.Add(newTab, 5, 0);
            top.Controls.Add(source, 6, 0);
            top.Controls.Add(bookmark, 7, 0);

            tabStrip.Dock = DockStyle.Top;
            tabStrip.Height = 31;
            tabStrip.SelectedIndexChanged += delegate { Render(ActiveTab.Current); };

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterDistance = 760 };
            page.Dock = DockStyle.Fill;
            page.AutoScroll = true;
            page.FlowDirection = FlowDirection.TopDown;
            page.WrapContents = false;
            page.Resize += delegate { ResizeChildren(); };
            split.Panel1.Controls.Add(page);

            side.Dock = DockStyle.Fill;
            side.Font = new Font("Segoe UI", 9);
            side.DoubleClick += delegate { OpenSideItem(); };
            split.Panel2.Controls.Add(side);

            status.Items.Add(label);
            label.Text = "Ready";

            Controls.Add(split);
            Controls.Add(tabStrip);
            Controls.Add(top);
            Controls.Add(status);
        }

        private static Button MakeButton(string text, Action action)
        {
            var button = new Button { Text = text, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
            button.Click += delegate { action(); };
            return button;
        }

        private static void Wire(Button button, string text, Action action)
        {
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(0, 0, 6, 0);
            button.Click += delegate { action(); };
        }

        private void NewTab()
        {
            tabs.Add(new ForgeTab());
            tabStrip.TabPages.Add("New");
            tabStrip.SelectedIndex = tabStrip.TabPages.Count - 1;
            page.Controls.Clear();
            address.Text = "";
            label.Text = "New tab";
            Buttons();
        }

        private void LoadPage(Func<ForgePage> load)
        {
            try
            {
                UseWaitCursor = true;
                label.Text = "Loading...";
                Render(load());
            }
            catch (Exception ex)
            {
                label.Text = "Error: " + ex.Message;
            }
            finally
            {
                UseWaitCursor = false;
                Buttons();
            }
        }

        private void Render(ForgePage loaded)
        {
            if (loaded == null)
            {
                Buttons();
                return;
            }

            Text = loaded.Title + " - LeaForge";
            tabStrip.TabPages[tabStrip.SelectedIndex].Text = loaded.Title.Length > 18 ? loaded.Title.Substring(0, 18) : loaded.Title;
            address.Text = loaded.Url.ToString();
            page.BackColor = loaded.Background;
            page.Controls.Clear();

            foreach (var node in loaded.Nodes)
            {
                page.Controls.Add(ControlFor(node, loaded));
            }

            RefreshSide();
            label.Text = loaded.Nodes.Count + " nodes, " + loaded.Links.Count + " links";
            Buttons();
        }

        private Control ControlFor(ForgeNode node, ForgePage loaded)
        {
            var width = Math.Max(420, page.ClientSize.Width - 32);
            var size = node.FontSize.HasValue ? node.FontSize.Value : (node.Kind == "h1" ? 24 : node.Kind == "h2" ? 18 : loaded.FontSize);

            if (node.Kind == "button")
            {
                var button = new Button { Text = node.Text, Width = width, Height = 36, TextAlign = ContentAlignment.MiddleLeft };
                button.Click += delegate
                {
                    ForgeParser.RunClickScript(node.Script, this, ActiveTab.Current);
                    Render(ActiveTab.Current);
                };
                return button;
            }

            if (node.Kind == "a" || node.Kind == "img")
            {
                var link = new LinkLabel { Text = node.Kind == "img" ? "[image] " + node.Text : node.Text, Width = width, AutoSize = false, Height = 30, Font = new Font("Segoe UI", size) };
                link.Click += delegate { LoadPage(delegate { return engine.Follow(ActiveTab, node.Url); }); };
                return link;
            }

            var labelControl = new Label
            {
                Text = node.Text,
                Width = width,
                Height = node.Kind == "h1" ? 54 : node.Kind == "h2" ? 42 : node.Kind == "pre" || node.Kind == "code" ? 120 : 62,
                AutoSize = false,
                ForeColor = node.Color.HasValue ? node.Color.Value : loaded.TextColor,
                BackColor = node.Background.HasValue ? node.Background.Value : loaded.Background,
                Font = new Font(node.Kind == "pre" || node.Kind == "code" ? "Consolas" : "Segoe UI", size, node.Bold ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(8)
            };

            return labelControl;
        }

        private void RefreshSide()
        {
            side.Items.Clear();
            var current = ActiveTab.Current;
            if (current != null)
            {
                side.Items.Add("Links");
                for (var i = 0; i < current.Links.Count; i++)
                {
                    side.Items.Add((i + 1) + ". " + current.Links[i].Text);
                }
            }

            side.Items.Add("");
            side.Items.Add("Bookmarks");
            foreach (var item in engine.Bookmarks)
            {
                side.Items.Add(item);
            }
        }

        private void OpenSideItem()
        {
            if (side.SelectedItem == null) return;
            var text = side.SelectedItem.ToString();
            var current = ActiveTab.Current;
            if (current != null)
            {
                var match = Regex.Match(text, "^(\\d+)\\. ");
                int number;
                if (match.Success && int.TryParse(match.Groups[1].Value, out number) && number > 0 && number <= current.Links.Count)
                {
                    LoadPage(delegate { return engine.Follow(ActiveTab, current.Links[number - 1].Url); });
                    return;
                }
            }

            var split = text.LastIndexOf(" | ");
            if (split >= 0)
            {
                LoadPage(delegate { return engine.Open(ActiveTab, text.Substring(split + 3)); });
            }
        }

        private void ShowSource()
        {
            if (ActiveTab.Current == null) return;
            var source = new Form { Text = "Source - " + ActiveTab.Current.Title, Size = new Size(860, 620), StartPosition = FormStartPosition.CenterParent };
            var box = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, ReadOnly = true, Font = new Font("Consolas", 9), Text = ActiveTab.Current.Html };
            source.Controls.Add(box);
            source.Show(this);
        }

        private void ResizeChildren()
        {
            foreach (Control child in page.Controls)
            {
                child.Width = page.ClientSize.Width - 32;
            }
        }

        private void Buttons()
        {
            var tab = ActiveTab;
            back.Enabled = tab.CanBack;
            forward.Enabled = tab.CanForward;
            reload.Enabled = tab.Current != null;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ForgeWindow());
        }
    }
}
