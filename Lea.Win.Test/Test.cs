using System;
using System.Drawing;
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
        private readonly Button go = new Button();
        private readonly Label st = new Label();

        public TestWindow()
        {
            Text = "Lea Win Test";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(980, 680);
            MinimumSize = new Size(640, 420);

            var top = new TableLayoutPanel();
            top.Dock = DockStyle.Top;
            top.Height = 38;
            top.ColumnCount = 5;
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));

            SetButton(bk, "<", delegate { v.Bk(); });
            SetButton(fw, ">", delegate { v.Fw(); });
            SetButton(rl, "R", delegate { TryGo(delegate { v.Rl(); }); });
            SetButton(go, "Go", delegate { TryGo(delegate { v.Go(u.Text); }); });
            u.Dock = DockStyle.Fill;
            u.Margin = new Padding(4, 7, 4, 4);
            u.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) { TryGo(delegate { v.Go(u.Text); }); e.SuppressKeyPress = true; }
            };

            top.Controls.Add(bk, 0, 0);
            top.Controls.Add(fw, 1, 0);
            top.Controls.Add(rl, 2, 0);
            top.Controls.Add(u, 3, 0);
            top.Controls.Add(go, 4, 0);

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
                st.Text = v.At == null ? "Lea ready" : v.At.N + " nodes, " + v.At.Ls.Count + " links";
            };

            Controls.Add(v);
            Controls.Add(st);
            Controls.Add(top);
        }

        private static void SetButton(Button button, string text, EventHandler action)
        {
            button.Text = text;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(2);
            button.Click += action;
        }

        private void TryGo(Action action)
        {
            try { action(); }
            catch (Exception ex) { st.Text = ex.Message; }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestWindow());
        }
    }
}
