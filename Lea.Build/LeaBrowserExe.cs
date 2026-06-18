using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LeaBrowserExe
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
            var page = Parse(url, web.DownloadString(url));
            if (index < history.Count - 1) history.RemoveRange(index + 1, history.Count - index - 1);
            history.Add(page);
            index = history.Count - 1;
            return page;
        }

        public Page Follow(int number)
        {
            return Open(Current.Links[number - 1].Url.ToString());
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
            var page = Parse(Current.Url, web.DownloadString(Current.Url));
            history[index] = page;
            return page;
        }

        private static Uri ToUri(string input)
        {
            Uri uri;
            return Uri.TryCreate(input, UriKind.Absolute, out uri) ? uri : new Uri("https://" + input);
        }

        private static Page Parse(Uri url, string html)
        {
            html = Regex.Replace(html, "<(script|style|noscript)[^>]*>.*?</\\1>|<!--.*?-->", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var links = new List<Link>();

            foreach (Match match in Regex.Matches(html, "<a\\s+[^>]*href\\s*=\\s*[\"']?([^\"'\\s>]+)[\"']?[^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                Uri linkUrl;
                if (!Uri.TryCreate(url, WebUtility.HtmlDecode(match.Groups[1].Value), out linkUrl)) continue;
                var label = Clean(Regex.Replace(match.Groups[2].Value, "<[^>]+>", " "));
                links.Add(new Link { Text = label.Length == 0 ? linkUrl.Host : Cut(label, 70), Url = linkUrl });
            }

            var titleMatch = Regex.Match(html, "<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var title = Clean(titleMatch.Groups[1].Value);

            return new Page
            {
                Url = url,
                Title = title.Length == 0 ? url.Host : title,
                Text = Cut(Clean(Regex.Replace(html, "<[^>]+>", " ")), 4000),
                Links = links
            };
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

    internal sealed class BrowserForm : Form
    {
        private readonly Engine engine = new Engine();
        private readonly TextBox address = new TextBox();
        private readonly Button back = new Button();
        private readonly Button forward = new Button();
        private readonly Button reload = new Button();
        private readonly Button go = new Button();
        private readonly TextBox page = new TextBox();
        private readonly ListBox links = new ListBox();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ToolStripStatusLabel label = new ToolStripStatusLabel();

        public BrowserForm()
        {
            Text = "Lea Browser";
            MinimumSize = new Size(860, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Build();
            Buttons();
        }

        private void Build()
        {
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 42, ColumnCount = 5, Padding = new Padding(8, 8, 8, 4) };
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

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterDistance = 580 };
            page.Dock = DockStyle.Fill;
            page.Multiline = true;
            page.ScrollBars = ScrollBars.Vertical;
            page.ReadOnly = true;
            page.WordWrap = true;
            page.Font = new Font("Segoe UI", 10);

            links.Dock = DockStyle.Fill;
            links.Font = new Font("Segoe UI", 9);
            links.DoubleClick += delegate { Follow(); };
            links.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    Follow();
                }
            };

            split.Panel1.Controls.Add(page);
            split.Panel2.Controls.Add(links);
            status.Items.Add(label);
            label.Text = "Ready";

            Controls.Add(split);
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

        private void Follow()
        {
            if (links.SelectedIndex >= 0)
            {
                LoadPage(delegate { return engine.Follow(links.SelectedIndex + 1); });
            }
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

        private void ShowPage(Page p)
        {
            if (p == null)
            {
                Buttons();
                return;
            }

            Text = p.Title + " - Lea Browser";
            address.Text = p.Url.ToString();
            page.Text = p.Title + Environment.NewLine + p.Url + Environment.NewLine + new string('-', 72) + Environment.NewLine + p.Text;
            links.Items.Clear();
            for (var i = 0; i < Math.Min(p.Links.Count, 100); i++)
            {
                links.Items.Add((i + 1) + ". " + p.Links[i].Text);
            }

            label.Text = p.Links.Count + " links found";
            Buttons();
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
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BrowserForm());
        }
    }
}
