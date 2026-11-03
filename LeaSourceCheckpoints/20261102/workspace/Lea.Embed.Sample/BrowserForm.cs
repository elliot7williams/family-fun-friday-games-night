using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Embed.Sample
{
    public sealed class BrowserForm : Form
    {
        public V Vw { get; } = new V { Dock = DockStyle.Fill };
        private readonly ToolStripTextBox address = new ToolStripTextBox { AutoSize = false, Width = 480 };
        private readonly ToolStripButton back = new ToolStripButton("Back");
        private readonly ToolStripButton forward = new ToolStripButton("Forward");
        private readonly ToolStripButton reload = new ToolStripButton("Reload");
        private readonly ToolStripButton open = new ToolStripButton("Open");
        private readonly ToolStripStatusLabel status = new ToolStripStatusLabel { Spring = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        private bool navigating;
        private string failure;

        public BrowserForm()
        {
            Text = "Lea";
            ClientSize = new System.Drawing.Size(960, 640);
            MinimumSize = new System.Drawing.Size(640, 420);
            W.On(Vw); // Isolated, local-document scripting; original production policy.
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            toolbar.Items.AddRange(new ToolStripItem[] { back, forward, reload, address, open });
            var footer = new StatusStrip();
            footer.Items.Add(status);
            Controls.Add(Vw);
            Controls.Add(toolbar);
            Controls.Add(footer);
            back.Click += async (s, e) => await Run(() => { Vw.Bk(); return Task.CompletedTask; });
            forward.Click += async (s, e) => await Run(() => { Vw.Fw(); return Task.CompletedTask; });
            reload.Click += async (s, e) => await Run(async () => { await Vw.RlA(); });
            open.Click += async (s, e) => await Run(() => Go(address.Text));
            address.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await Run(() => Go(address.Text));
            };
            Vw.Done += (s, e) => RefreshState();
            Vw.Changed += (s, e) => RefreshState();
            toolbar.SizeChanged += (s, e) => address.Width = Math.Max(100, toolbar.ClientSize.Width - 270);
            RefreshState();
        }

        public async Task Go(string input)
        {
            await Vw.GoA(input);
            RefreshState();
        }

        private async Task Run(Func<Task> action)
        {
            if (navigating || Vw.Busy) return;
            navigating = true;
            failure = null;
            RefreshState();
            try { await action(); }
            catch (Exception error) { failure = error.Message; }
            finally { navigating = false; RefreshState(); }
        }

        private void RefreshState()
        {
            if (IsDisposed) return;
            var idle = !navigating && !Vw.Busy;
            back.Enabled = idle && Vw.CanBk;
            forward.Enabled = idle && Vw.CanFw;
            reload.Enabled = idle && Vw.U != null;
            open.Enabled = idle;
            if (!address.Focused) address.Text = Vw.U == null ? "" : Vw.U.AbsoluteUri;
            Text = Vw.T.Length == 0 ? "Lea" : Vw.T + " - Lea";
            status.Text = failure ?? (Vw.Busy ? "Loading" : Vw.Xo != null && !Vw.Xo.Ok ? Vw.Xo.S + ": " + Vw.Xo.M : "Ready");
        }
    }
}
