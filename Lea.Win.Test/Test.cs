using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private readonly Button go = new Button();
        private readonly Label st = new Label();
        private readonly TableLayoutPanel findBar = new TableLayoutPanel();
        private readonly TextBox findText = new TextBox();
        private readonly Label findState = new Label();
        private readonly Button findPrevious = new Button();
        private readonly Button findNext = new Button();
        private readonly Button findClose = new Button();
        private readonly ToolTip tips = new ToolTip();

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
            top.ColumnCount = 7;
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));

            SetButton(bk, "<", delegate { v.Bk(); });
            SetButton(fw, ">", delegate { v.Fw(); });
            SetButton(rl, "R", async delegate { await TryGoA(delegate { return v.RlA(); }); });
            SetButton(stp, "X", delegate { v.St(); st.Text = "Stopped"; });
            SetButton(dl, "D", delegate { Download(v.U, DownloadName(v.U)); });
            SetButton(go, "Go", async delegate { await TryGoA(delegate { return v.GoA(u.Text); }); });
            tips.SetToolTip(bk, "Back");
            tips.SetToolTip(fw, "Forward");
            tips.SetToolTip(rl, "Reload");
            tips.SetToolTip(stp, "Stop");
            tips.SetToolTip(dl, "Download current address");
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
            top.Controls.Add(go, 6, 0);

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
            };
            v.Al += delegate { st.Text = "Alert: " + v.Am; };
            v.Dr += delegate { Download(v.Du, v.Dn); };

            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Control && e.KeyCode == Keys.F) { OpenFind(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape && findBar.Visible) { CloseFind(); e.SuppressKeyPress = true; }
            };

            Controls.Add(v);
            Controls.Add(st);
            Controls.Add(findBar);
            Controls.Add(top);

            if (!String.IsNullOrWhiteSpace(start))
            {
                u.Text = start;
                Shown += async delegate { await TryGoA(delegate { return v.GoA(start); }); };
            }
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
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestWindow(args.Length > 0 ? args[0] : ""));
        }
    }
}
