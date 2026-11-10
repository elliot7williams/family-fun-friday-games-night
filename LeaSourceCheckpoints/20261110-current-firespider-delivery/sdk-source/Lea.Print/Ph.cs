using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Lea.Print
{
    // WinForms owns the preview window; the caller still owns its print plan.
    public sealed class Ph : Form
    {
        private readonly Pv preview;
        private readonly ElementHost host;
        public Pv Vw
        {
            get
            {
                preview.Dispatcher.VerifyAccess();
                if (IsDisposed) throw new ObjectDisposedException("Ph");
                return preview;
            }
        }
        public Ph(Pp pages)
        {
            if (pages == null) throw new ArgumentNullException("pages");
            preview = new Pv(pages);
            try
            {
                Text = "Lea Print Preview";
                ClientSize = new Size(1000, 700);
                MinimumSize = new Size(360, 300);
                StartPosition = FormStartPosition.CenterParent;
                ShowInTaskbar = false;
                AutoScaleMode = AutoScaleMode.Dpi;
                host = new ElementHost { Dock = DockStyle.Fill, Child = preview, AccessibleName = "Lea print preview" };
                Controls.Add(host);
            }
            catch { Dispose(); throw; }
        }
        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            if (keyData == Keys.Escape) { Close(); return true; }
            return base.ProcessCmdKey(ref message, keyData);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed && preview != null)
            {
                preview.Dispatcher.VerifyAccess();
                if (host != null) host.Child = null;
                preview.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
