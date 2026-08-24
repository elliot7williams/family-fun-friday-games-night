using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Win.Test
{
    internal sealed class TestWindow : Form
    {
        private readonly V v = new V();
        private readonly TextBox u = new TextBox();
        private readonly Button bk = new Button();
        private readonly Button fw = new Button();
        private readonly Button rl = new Button();
        private readonly Button stp = new Button();
        private readonly Button dl = new Button();
        private readonly Button ins = new Button();
        private readonly Button go = new Button();
        private readonly Label st = new Label();
        private readonly TableLayoutPanel findBar = new TableLayoutPanel();
        private readonly TextBox findText = new TextBox();
        private readonly Label findState = new Label();
        private readonly Button findPrevious = new Button();
        private readonly Button findNext = new Button();
        private readonly Button findClose = new Button();
        private readonly ToolTip tips = new ToolTip();
        private readonly SplitContainer split = new SplitContainer();
        private readonly TabControl inspectorTabs = new TabControl();
        private readonly TextBox inspectorSummary = InspectorText();
        private readonly TextBox inspectorNodes = InspectorText();
        private readonly TextBox inspectorCss = InspectorText();
        private readonly TextBox inspectorRequests = InspectorText();
        private readonly TextBox inspectorMessages = InspectorText();
        private readonly TextBox inspectorAccess = InspectorText();

        public TestWindow(string start)
        {
            Text = "Lea Win Test";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(980, 680);
            MinimumSize = new Size(640, 420);
            KeyPreview = true;

            var top = new TableLayoutPanel();
            top.Dock = DockStyle.Top;
            top.Height = 38;
            top.ColumnCount = 8;
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));

            SetButton(bk, "<", delegate { v.Bk(); });
            SetButton(fw, ">", delegate { v.Fw(); });
            SetButton(rl, "R", async delegate { await TryGoA(delegate { return v.RlA(); }); });
            SetButton(stp, "X", delegate { v.St(); st.Text = "Stopped"; });
            SetButton(dl, "D", delegate { Download(v.U, DownloadName(v.U)); });
            SetButton(ins, "I", delegate { ToggleInspector(); });
            SetButton(go, "Go", async delegate { await TryGoA(delegate { return v.GoA(u.Text); }); });
            tips.SetToolTip(bk, "Back");
            tips.SetToolTip(fw, "Forward");
            tips.SetToolTip(rl, "Reload");
            tips.SetToolTip(stp, "Stop");
            tips.SetToolTip(dl, "Download current address");
            tips.SetToolTip(ins, "Developer diagnostics");
            u.Dock = DockStyle.Fill;
            u.Margin = new Padding(4, 7, 4, 4);
            u.KeyDown += async delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { await TryGoA(delegate { return v.GoA(u.Text); }); e.SuppressKeyPress = true; }
            };

            top.Controls.Add(bk, 0, 0);
            top.Controls.Add(fw, 1, 0);
            top.Controls.Add(rl, 2, 0);
            top.Controls.Add(stp, 3, 0);
            top.Controls.Add(dl, 4, 0);
            top.Controls.Add(u, 5, 0);
            top.Controls.Add(ins, 6, 0);
            top.Controls.Add(go, 7, 0);

            BuildInspector();

            findBar.Dock = DockStyle.Top;
            findBar.Height = 34;
            findBar.ColumnCount = 5;
            findBar.Visible = false;
            findBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            findBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            findBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            findBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            findBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            findText.Dock = DockStyle.Fill;
            findText.Margin = new Padding(4, 5, 4, 4);
            findState.Dock = DockStyle.Fill;
            findState.TextAlign = ContentAlignment.MiddleCenter;
            SetButton(findPrevious, "<", delegate { FindPrevious(); });
            SetButton(findNext, ">", delegate { FindNext(); });
            SetButton(findClose, "X", delegate { CloseFind(); });
            tips.SetToolTip(findPrevious, "Previous match");
            tips.SetToolTip(findNext, "Next match");
            tips.SetToolTip(findClose, "Close find");
            findText.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (e.Shift) FindPrevious(); else FindNext();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape) { CloseFind(); e.SuppressKeyPress = true; }
            };
            findText.TextChanged += delegate { StartFind(); };
            findBar.Controls.Add(findText, 0, 0);
            findBar.Controls.Add(findState, 1, 0);
            findBar.Controls.Add(findPrevious, 2, 0);
            findBar.Controls.Add(findNext, 3, 0);
            findBar.Controls.Add(findClose, 4, 0);

            st.Dock = DockStyle.Bottom;
            st.Height = 24;
            st.Padding = new Padding(6, 3, 0, 0);
            st.Text = "Lea ready";

            v.Changed += delegate
            {
                u.Text = v.U == null ? u.Text : v.U.ToString();
                Text = (v.T.Length == 0 ? "Lea Win Test" : v.T + " - Lea Win Test");
                bk.Enabled = v.CanBk;
                fw.Enabled = v.CanFw;
                dl.Enabled = v.U != null;
                st.Text = v.At == null ? "Lea ready" : v.At.N + " nodes, " + v.At.Ls.Count + " links, " + v.At.Fs.Count + " fields";
                if (!split.Panel2Collapsed) RefreshInspector();
            };
            v.Al += delegate { st.Text = "Alert: " + v.Am; };
            v.Dr += delegate { Download(v.Du, v.Dn); };
            v.Gr += async delegate
            {
                var target = v.Gu;
                if (target != null) await TryGoA(delegate { return v.GoA(target); });
            };

            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.F) { OpenFind(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.F12 || (e.Control && e.Shift && e.KeyCode == Keys.I)) { ToggleInspector(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape && findBar.Visible) { CloseFind(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape && !split.Panel2Collapsed) { CloseInspector(); e.SuppressKeyPress = true; }
            };

            split.Dock = DockStyle.Fill;
            split.FixedPanel = FixedPanel.Panel2;
            split.SplitterWidth = 5;
            split.Panel1.Controls.Add(v);
            split.Panel2Collapsed = true;
            Controls.Add(split);
            Controls.Add(st);
            Controls.Add(findBar);
            Controls.Add(top);
            PerformLayout();
            split.Panel2MinSize = 280;

            if (!String.IsNullOrWhiteSpace(start))
            {
                u.Text = start;
                Shown += async delegate { await TryGoA(delegate { return v.GoA(start); }); };
            }
        }

        private void BuildInspector()
        {
            var header = new TableLayoutPanel();
            header.Dock = DockStyle.Top;
            header.Height = 34;
            header.ColumnCount = 4;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            var title = new Label();
            title.Text = "Lea Inspector";
            title.Dock = DockStyle.Fill;
            title.Padding = new Padding(7, 8, 0, 0);
            var refresh = new Button();
            var clear = new Button();
            var close = new Button();
            SetButton(refresh, "R", delegate { RefreshInspector(); });
            SetButton(clear, "C", delegate { v.Dcl(); RefreshInspector(); });
            SetButton(close, "X", delegate { CloseInspector(); });
            tips.SetToolTip(refresh, "Refresh diagnostics");
            tips.SetToolTip(clear, "Clear request activity");
            tips.SetToolTip(close, "Close diagnostics");
            header.Controls.Add(title, 0, 0);
            header.Controls.Add(refresh, 1, 0);
            header.Controls.Add(clear, 2, 0);
            header.Controls.Add(close, 3, 0);

            inspectorTabs.Dock = DockStyle.Fill;
            AddInspectorTab("Summary", inspectorSummary);
            AddInspectorTab("Nodes", inspectorNodes);
            AddInspectorTab("CSS", inspectorCss);
            AddInspectorTab("Requests", inspectorRequests);
            AddInspectorTab("Messages", inspectorMessages);
            AddInspectorTab("Access", inspectorAccess);
            split.Panel2.Controls.Add(inspectorTabs);
            split.Panel2.Controls.Add(header);
        }

        private void AddInspectorTab(string name, TextBox content)
        {
            var page = new TabPage(name);
            page.Padding = new Padding(4);
            page.Controls.Add(content);
            inspectorTabs.TabPages.Add(page);
        }

        private static TextBox InspectorText()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                WordWrap = false,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Consolas", 9F)
            };
        }

        private void ToggleInspector()
        {
            if (split.Panel2Collapsed) OpenInspector(); else CloseInspector();
        }

        private void OpenInspector()
        {
            split.Panel2Collapsed = false;
            split.SplitterDistance = Math.Max(320, split.Width - 360);
            RefreshInspector();
        }

        private void CloseInspector()
        {
            split.Panel2Collapsed = true;
            v.Focus();
        }

        private void RefreshInspector()
        {
            var data = v.Dg();
            if (data == null)
            {
                inspectorSummary.Text = "No page";
                inspectorNodes.Clear();
                inspectorCss.Clear();
                inspectorRequests.Clear();
                inspectorMessages.Clear();
                inspectorAccess.Clear();
                return;
            }
            inspectorSummary.Lines = new[]
            {
                data.T,
                data.U == null ? "" : data.U.ToString(),
                "",
                "Response   " + data.St + "  " + data.Ct + "  " + data.Sz + " bytes" + (data.Ca ? "  cache" : ""),
                "Document   " + data.N + " nodes  " + data.B + " blocks  " + v.Axs.Count + " access  " + data.Vw + "px viewport",
                "Content    " + data.L + " links  " + data.F + " fields  " + data.C + " CSS rules  " + data.Vc + " custom",
                "Commands   " + data.J + " accepted  " + data.V + " variables  " + data.M + " alerts",
                "Engine     " + data.Cc + " cached  " + data.Ch + " reuses  " + data.Kc + " cookies"
            };
            inspectorNodes.Lines = Copy(data.Ns);
            inspectorCss.Lines = Copy(data.Cs);
            inspectorRequests.Lines = Copy(data.Rs);
            var messages = new System.Collections.Generic.List<string>();
            foreach (var item in data.Ms) messages.Add(item);
            if (data.Vs.Count > 0)
            {
                messages.Add("");
                messages.Add("variables");
                foreach (var item in data.Vs) messages.Add(item);
            }
            inspectorMessages.Lines = messages.ToArray();
            inspectorAccess.Lines = AccessibilityLines();
        }

        private string[] AccessibilityLines()
        {
            var lines = new System.Collections.Generic.List<string>();
            foreach (var item in v.Axs)
            {
                var depth = 0;
                for (var parent = item.Up; parent != null && depth < 12; parent = parent.Up) depth++;
                var line = new StringBuilder();
                line.Append(new string(' ', depth * 2)).Append(item.K);
                if (item.N.Length > 0) line.Append(" \"").Append(item.N).Append('"');
                if (item.V.Length > 0) line.Append(" = ").Append(item.V);
                if (item.St != 0) line.Append(" [").Append(item.St).Append(']');
                if (item.Lv > 0) line.Append(" level ").Append(item.Lv);
                lines.Add(line.ToString());
            }
            return lines.ToArray();
        }

        private static string[] Copy(System.Collections.Generic.IList<string> values)
        {
            var result = new string[values.Count];
            values.CopyTo(result, 0);
            return result;
        }

        internal bool DiagnosticsSmoke(string page, string output)
        {
            v.Go(page);
            OpenInspector();
            PerformLayout();
            var keyCount = v.Kn;
            var keyPassed = keyCount == 0 || (v.Ke != null && !v.Kb.IsEmpty) || (v.Kf() > 0 && v.Ke != null && !v.Kb.IsEmpty);
            var accessPassed = v.An == 0;
            if (!accessPassed)
                foreach (var element in v.Qa("[accesskey]"))
                {
                    if (v.Ak(element).Length == 0) continue;
                    accessPassed = v.Kl(element).StartsWith("Alt+Shift+");
                    break;
                }
            var passed = inspectorSummary.Text.Contains("Response   200") &&
                inspectorSummary.Text.Contains("px viewport") &&
                inspectorNodes.Text.Contains("body") && inspectorCss.Text.Contains("body") &&
                inspectorRequests.Text.Contains("GET 200 file") && inspectorMessages.Text.Contains("commands") &&
                inspectorAccess.Text.Contains("document") && v.Ao != null && keyPassed && accessPassed;
            if (!String.IsNullOrWhiteSpace(output))
            {
                if (inspectorTabs.TabPages.Count > 5) inspectorTabs.SelectedIndex = 5;
                var full = Path.GetFullPath(output);
                var folder = Path.GetDirectoryName(full);
                if (!String.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Location = new Point(-20000, -20000);
                Show();
                Application.DoEvents();
                using (var image = new Bitmap(ClientSize.Width, ClientSize.Height))
                {
                    DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    image.Save(full, System.Drawing.Imaging.ImageFormat.Png);
                }
                Hide();
            }
            return passed;
        }

        private static void SetButton(Button button, string text, EventHandler action)
        {
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(2);
            button.Click += action;
        }

        private async Task TryGoA(Func<Task> action)
        {
            st.Text = "Loading...";
            try { await action(); }
            catch (Exception ex) { st.Text = ex.Message; }
        }

        private void OpenFind()
        {
            findBar.Visible = true;
            findText.Focus();
            findText.SelectAll();
            StartFind();
        }

        private void CloseFind()
        {
            v.Fc();
            findBar.Visible = false;
            findState.Text = "";
            v.Focus();
        }

        private void StartFind()
        {
            if (!findBar.Visible) return;
            v.Fd(findText.Text);
            FindStatus();
        }

        private void FindNext()
        {
            if (v.Fq != findText.Text.Trim()) v.Fd(findText.Text); else v.Nx();
            FindStatus();
        }

        private void FindPrevious()
        {
            if (v.Fq != findText.Text.Trim()) v.Fd(findText.Text); else v.Pr();
            FindStatus();
        }

        private void FindStatus()
        {
            findState.Text = v.Fn == 0 ? "0 / 0" : v.Fx + " / " + v.Fn;
        }

        private async void Download(Uri url, string name)
        {
            if (url == null) return;
            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = String.IsNullOrWhiteSpace(name) ? "download.bin" : name;
                dialog.Filter = "All files|*.*";
                dialog.RestoreDirectory = true;
                dialog.Title = "Save Lea Download";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                st.Text = "Downloading to " + dialog.FileName;
                try
                {
                    var result = await v.DlA(url.ToString(), dialog.FileName);
                    st.Text = "Downloaded " + result.Sz + " bytes to " + result.P;
                }
                catch (Exception error) { st.Text = error.Message; }
            }
        }

        private static string DownloadName(Uri url)
        {
            if (url == null) return "download.bin";
            var name = Path.GetFileName(url.IsFile ? url.LocalPath : url.AbsolutePath);
            if (String.IsNullOrWhiteSpace(name)) name = "download.html";
            foreach (var invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
            return name;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 1 && args[0] == "--smoke")
            {
                try
                {
                    using (var window = new TestWindow(""))
                    {
                        window.CreateControl();
                        return window.DiagnosticsSmoke(args[1], args.Length > 2 ? args[2] : "") ? 0 : 1;
                    }
                }
                catch (Exception error)
                {
                    if (args.Length > 2) File.WriteAllText(args[2] + ".error.txt", error.ToString());
                    return 1;
                }
            }
            Application.Run(new TestWindow(args.Length > 0 ? args[0] : ""));
            return 0;
        }
    }
}
