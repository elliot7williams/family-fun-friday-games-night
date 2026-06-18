using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LeaPlusExe
{
    internal sealed class Node
    {
        public string Kind;
        public string Text;
        public Uri Url;
        public string Script;
        public Color? Color;
        public int? FontSize;
    }

    internal sealed class Page
    {
        public Uri Url;
        public string Html;
        public string Title;
        public List<Node> Nodes = new List<Node>();
        public Color Background = Color.White;
        public Color TextColor = Color.FromArgb(32, 32, 32);
        public int FontSize = 10;
    }

    internal sealed class Engine
    {
        private readonly WebClient web = new WebClient();
        private readonly List<Page> history = new List<Page>();
        private int index = -1;

        public Page Current { get { return index >= 0 && index < history.Count ? history[index] : null; } }
        public bool CanBack { get { return index > 0; } }
        public bool CanForward { get { return index < history.Count - 1; } }

        public Page Open(string input)
        {
            var url = ToUri(input);
            var html = url.IsFile ? File.ReadAllText(url.LocalPath) : web.DownloadString(url);
            var page = Parse(url, html);
            if (index < history.Count - 1) history.RemoveRange(index + 1, history.Count - index - 1);
            history.Add(page);
            index = history.Count - 1;
            return page;
        }

        public Page Back()
        {
            if (CanBack) index--;
            return Current;
        }

        public Page Forward()
        {
            if (CanForward) index++;
            return Current;
        }

        public Page Reload()
        {
            if (Current == null) return null;
            var html = Current.Url.IsFile ? File.ReadAllText(Current.Url.LocalPath) : web.DownloadString(Current.Url);
            var page = Parse(Current.Url, html);
            history[index] = page;
            return page;
        }

        private static Uri ToUri(string input)
        {
            input = input.Trim().Trim('"');
            Uri uri;
            if (Uri.TryCreate(input, UriKind.Absolute, out uri)) return uri;
            if (File.Exists(input)) return new Uri(Path.GetFullPath(input));
            return new Uri("https://" + input);
        }

        private static Page Parse(Uri url, string html)
        {
            var page = new Page();
            page.Url = url;
            page.Html = html;
            page.Title = Clean(First(html, "title"));
            if (page.Title.Length == 0) page.Title = url.IsFile ? Path.GetFileName(url.LocalPath) : url.Host;

            ApplyCss(page, html);
            RunTitleScript(page, html);

            var bodyMatch = Regex.Match(html, "<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var body = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;
            body = Regex.Replace(body, "<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in Regex.Matches(body, "<(?<tag>h1|h2|h3|p|li|a|button|img|div|section|article)(?<attrs>[^>]*)>(?<inner>.*?)</\\k<tag>>|<(?<tag>img)(?<attrs>[^>]*)/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var tag = match.Groups["tag"].Value.ToLowerInvariant();
                var attrs = match.Groups["attrs"].Value;
                var inner = match.Groups["inner"].Value;
                var text = Clean(Regex.Replace(inner, "<[^>]+>", " "));
                var style = Attr(attrs, "style");
                var color = ReadColor(style);
                var size = ReadFontSize(style);

                if (tag == "a")
                {
                    Uri link;
                    if (Uri.TryCreate(url, Attr(attrs, "href"), out link))
                    {
                        page.Nodes.Add(new Node { Kind = "a", Text = Cut(text.Length == 0 ? link.ToString() : text, 160), Url = link, Color = color, FontSize = size });
                    }
                }
                else if (tag == "img")
                {
                    Uri image;
                    if (Uri.TryCreate(url, Attr(attrs, "src"), out image))
                    {
                        page.Nodes.Add(new Node { Kind = "img", Text = image.ToString(), Url = image });
                    }
                }
                else if (tag == "button")
                {
                    page.Nodes.Add(new Node { Kind = "button", Text = Cut(text, 120), Script = Attr(attrs, "onclick"), Color = color, FontSize = size });
                }
                else if (text.Length > 0)
                {
                    page.Nodes.Add(new Node { Kind = tag, Text = Cut(text, 600), Color = color, FontSize = size });
                }
            }

            if (page.Nodes.Count == 0)
            {
                page.Nodes.Add(new Node { Kind = "p", Text = Cut(Clean(Regex.Replace(body, "<[^>]+>", " ")), 3000) });
            }

            return page;
        }

        private static void ApplyCss(Page page, string html)
        {
            var styles = "";
            foreach (Match match in Regex.Matches(html, "<style[^>]*>(.*?)</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                styles += match.Groups[1].Value + "\n";
            }

            var body = Regex.Match(styles, "body\\s*\\{(.*?)\\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!body.Success) return;
            var rules = body.Groups[1].Value;
            page.Background = ReadColor(rules, "background", page.Background);
            page.TextColor = ReadColor(rules, "color", page.TextColor);
            var size = ReadFontSize(rules);
            if (size.HasValue) page.FontSize = size.Value;
        }

        private static void RunTitleScript(Page page, string html)
        {
            foreach (Match script in Regex.Matches(html, "<script[^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var title = Regex.Match(script.Groups[1].Value, "document\\.title\\s*=\\s*['\"](?<v>.*?)['\"]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (title.Success) page.Title = Clean(title.Groups["v"].Value);
            }
        }

        public static void RunClickScript(string script, IWin32Window owner)
        {
            if (script == null) return;
            var alert = Regex.Match(script, "alert\\s*\\(\\s*['\"](?<v>.*?)['\"]\\s*\\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (alert.Success) MessageBox.Show(owner, WebUtility.HtmlDecode(alert.Groups["v"].Value), "LeaPlus Script");
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

        private static Color? ReadColor(string style)
        {
            var match = Regex.Match(style, "(?:^|;)\\s*color\\s*:\\s*(?<v>#[0-9a-f]{6}|[a-z]+)", RegexOptions.IgnoreCase);
            return match.Success ? TryColor(match.Groups["v"].Value) : null;
        }

        private static Color ReadColor(string style, string prop, Color fallback)
        {
            var match = Regex.Match(style, prop + "(?:-color)?\\s*:\\s*(?<v>#[0-9a-f]{6}|[a-z]+)", RegexOptions.IgnoreCase);
            var color = match.Success ? TryColor(match.Groups["v"].Value) : null;
            return color.HasValue ? color.Value : fallback;
        }

        private static Color? TryColor(string value)
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

    internal sealed class Browser : Form
    {
        private readonly Engine engine = new Engine();
        private readonly TextBox address = new TextBox();
        private readonly Button back = new Button();
        private readonly Button forward = new Button();
        private readonly Button reload = new Button();
        private readonly Button go = new Button();
        private readonly FlowLayoutPanel page = new FlowLayoutPanel();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ToolStripStatusLabel label = new ToolStripStatusLabel();

        public Browser()
        {
            Text = "LeaPlus";
            MinimumSize = new Size(920, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Build();
            Buttons();
        }

        private void Build()
        {
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 5, Padding = new Padding(8) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));

            Wire(back, "<", delegate { ShowPage(engine.Back()); });
            Wire(forward, ">", delegate { ShowPage(engine.Forward()); });
            Wire(reload, "R", delegate { LoadPage(delegate { return engine.Reload(); }); });
            Wire(go, "Go", delegate { LoadPage(delegate { return engine.Open(address.Text); }); });

            address.Dock = DockStyle.Fill;
            address.Font = new Font("Segoe UI", 10);
            address.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadPage(delegate { return engine.Open(address.Text); });
                }
            };

            top.Controls.Add(back, 0, 0);
            top.Controls.Add(forward, 1, 0);
            top.Controls.Add(reload, 2, 0);
            top.Controls.Add(address, 3, 0);
            top.Controls.Add(go, 4, 0);

            page.Dock = DockStyle.Fill;
            page.AutoScroll = true;
            page.FlowDirection = FlowDirection.TopDown;
            page.WrapContents = false;
            page.Resize += delegate { ResizeChildren(); };

            status.Items.Add(label);
            label.Text = "Ready";

            Controls.Add(page);
            Controls.Add(top);
            Controls.Add(status);
        }

        private static void Wire(Button button, string text, Action action)
        {
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(0, 0, 6, 0);
            button.Click += delegate { action(); };
        }

        private void LoadPage(Func<Page> load)
        {
            try
            {
                UseWaitCursor = true;
                label.Text = "Loading...";
                ShowPage(load());
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

        private void ShowPage(Page loaded)
        {
            if (loaded == null)
            {
                Buttons();
                return;
            }

            Text = loaded.Title + " - LeaPlus";
            address.Text = loaded.Url.ToString();
            page.BackColor = loaded.Background;
            page.Controls.Clear();

            foreach (var node in loaded.Nodes)
            {
                page.Controls.Add(ControlFor(node, loaded));
            }

            label.Text = loaded.Nodes.Count + " nodes";
            Buttons();
        }

        private Control ControlFor(Node node, Page loaded)
        {
            var width = Math.Max(320, page.ClientSize.Width - 28);
            var size = node.FontSize.HasValue ? node.FontSize.Value : (node.Kind == "h1" ? 22 : node.Kind == "h2" ? 17 : loaded.FontSize);

            if (node.Kind == "button")
            {
                var button = new Button { Text = node.Text, Width = width, Height = 34, TextAlign = ContentAlignment.MiddleLeft };
                button.Click += delegate { Engine.RunClickScript(node.Script, this); };
                return button;
            }

            if (node.Kind == "a" || node.Kind == "img")
            {
                var link = new LinkLabel { Text = node.Kind == "img" ? "[image] " + node.Text : node.Text, Width = width, AutoSize = false, Height = 30, Font = new Font("Segoe UI", size) };
                link.Click += delegate { LoadPage(delegate { return engine.Open(node.Url.ToString()); }); };
                return link;
            }

            return new Label
            {
                Text = node.Text,
                Width = width,
                Height = node.Kind == "h1" ? 48 : node.Kind == "h2" ? 40 : 54,
                AutoSize = false,
                ForeColor = node.Color.HasValue ? node.Color.Value : loaded.TextColor,
                Font = new Font("Segoe UI", size, node.Kind == "h1" || node.Kind == "h2" || node.Kind == "h3" ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(6)
            };
        }

        private void ResizeChildren()
        {
            foreach (Control child in page.Controls)
            {
                child.Width = page.ClientSize.Width - 28;
            }
        }

        private void Buttons()
        {
            back.Enabled = engine.CanBack;
            forward.Enabled = engine.CanForward;
            reload.Enabled = engine.Current != null;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Browser());
        }
    }
}
