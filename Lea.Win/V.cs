using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;

namespace Lea.Win
{
    // V is Lea's small, one-surface WinForms view.
    public sealed class V : Control
    {
        private readonly E e = new E();
        private readonly List<Hit> hits = new List<Hit>();
        private readonly WebClient picWeb = new WebClient();
        private readonly Dictionary<string, Image> pics = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> picOrder = new Queue<string>();
        private const int PicLimit = 24;
        private int sy;
        private bool picBusy;

        public event EventHandler Done;
        public event EventHandler Changed;

        public V()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(31, 41, 55);
            Font = new Font("Segoe UI", 10F);
            Dock = DockStyle.Fill;
            TabStop = true;
        }

        public P At { get { return e.At; } }
        public Uri U { get { return At == null ? null : At.U; } }
        public string T { get { return At == null ? "" : At.T; } }
        public string H { get { return At == null ? "" : At.H; } }
        public bool CanBk { get { return e.CanBk; } }
        public bool CanFw { get { return e.CanFw; } }
        public bool Busy { get { return e.Busy || picBusy; } }

        public P Go(string input)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var p = e.Go(input);
                sy = 0;
                OnChanged();
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                return p;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        public async Task<P> GoA(string input)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var p = await e.GoA(input);
                sy = 0;
                OnChanged();
                Invalidate();
                await PicsA(p);
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                return p;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        public P Bk()
        {
            var p = e.Bk();
            sy = 0;
            OnChanged();
            Invalidate();
            return p;
        }

        public P Fw()
        {
            var p = e.Fw();
            sy = 0;
            OnChanged();
            Invalidate();
            return p;
        }

        public P Rl()
        {
            var p = e.Rl();
            sy = 0;
            OnChanged();
            Invalidate();
            return p;
        }

        public async Task<P> RlA()
        {
            var p = await e.RlA();
            sy = 0;
            OnChanged();
            Invalidate();
            await PicsA(p);
            Invalidate();
            if (Done != null) Done(this, EventArgs.Empty);
            return p;
        }

        public void St()
        {
            e.St();
            picWeb.CancelAsync();
            Cursor = Cursors.Default;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            hits.Clear();
            if (At == null)
            {
                TextRenderer.DrawText(pe.Graphics, "Lea", Font, new Point(18, 18), ForeColor);
                return;
            }

            var pageColor = CssColor(At.S == null ? "" : At.S.F, ForeColor);
            var pageBackground = CssColor(At.S == null ? "" : At.S.Bg, BackColor);
            pe.Graphics.Clear(pageBackground);
            var pageLeft = At.S == null ? 0 : At.S.Ml + At.S.Pl;
            var pageRight = At.S == null ? 0 : At.S.Mr + At.S.Pr;
            var x = 20 + pageLeft;
            var width = Math.Max(120, ClientSize.Width - x - 20 - pageRight);
            var y = 18 - sy + (At.S == null ? 0 : At.S.Mt + At.S.Pt);
            using (var titleFont = new Font(Font.FontFamily, 20F, FontStyle.Bold))
            {
                y += Draw(pe.Graphics, At.T, titleFont, pageColor, null, x, y, width) + 14;
            }

            if (At.Bs.Count == 0)
            {
                y += Draw(pe.Graphics, At.X, Font, pageColor, null, x, y, width) + 18;
            }
            else
            {
                foreach (var block in At.Bs)
                {
                    var blockX = x + (block.S == null ? 0 : block.S.Ml + block.S.Pl);
                    var blockWidth = Math.Max(40, width - (block.S == null ? 0 : block.S.Ml + block.S.Mr + block.S.Pl + block.S.Pr));
                    y += block.S == null ? 0 : block.S.Mt + block.S.Pt;
                    if (block.K == "img")
                    {
                        y += DrawPic(pe.Graphics, block, blockX, y, blockWidth, pageColor, pageBackground) + 10 + (block.S == null ? 0 : block.S.Pb + block.S.Mb);
                        continue;
                    }

                    var fontName = block.K == "pre" || block.K == "code" ? "Consolas" : Font.FontFamily.Name;
                    var fontSize = block.K == "h1" ? 18F : block.K == "h2" ? 15F : block.K == "h3" ? 12F : Font.Size;
                    if (block.S != null && block.S.Fs > 0) fontSize = block.S.Fs;
                    var fontStyle = block.K.StartsWith("h") || (block.S != null && block.S.B) ? FontStyle.Bold : FontStyle.Regular;
                    if (block.K == "a") fontStyle |= FontStyle.Underline;
                    var blockFont = new Font(fontName, fontSize, fontStyle);
                    var blockColor = block.K == "a" ? Color.FromArgb(0, 102, 204) : pageColor;
                    if (block.S != null) blockColor = CssColor(block.S.F, blockColor);
                    Color? blockBackground = block.S != null && block.S.Bg.Length > 0 ? CssColor(block.S.Bg, pageBackground) : (Color?)null;

                    var text = block.K == "li" ? "- " + block.X : block.X;
                    var h = Draw(pe.Graphics, text, blockFont, blockColor, blockBackground, blockX, y, blockWidth);
                    if (block.U != null) hits.Add(new Hit(new Rectangle(blockX, y, blockWidth, h), block.U));
                    y += h + (block.K.StartsWith("h") ? 12 : 8) + (block.S == null ? 0 : block.S.Pb + block.S.Mb);
                    blockFont.Dispose();
                }
            }

            if (At.Ls.Count > 0 && At.Bs.Count == 0)
            {
                using (var linkFont = new Font(Font, FontStyle.Underline))
                {
                    foreach (var link in At.Ls)
                    {
                        var text = link.T.Length == 0 ? link.U.ToString() : link.T;
                        var h = Draw(pe.Graphics, text, linkFont, Color.FromArgb(0, 102, 204), null, x, y, width);
                        hits.Add(new Hit(new Rectangle(x, y, width, h), link.U));
                        y += h + 8;
                    }
                }
            }
        }

        private async Task PicsA(P page)
        {
            if (page == null) return;
            picBusy = true;
            try
            {
                foreach (var block in page.Bs)
                {
                    if (block.K != "img" || block.U == null) continue;
                    var key = block.U.AbsoluteUri;
                    if (pics.ContainsKey(key)) continue;
                    Image image = null;
                    try
                    {
                        byte[] data;
                        if (block.U.IsFile)
                        {
                            var path = block.U.LocalPath;
                            data = await Task.Factory.StartNew(delegate { return File.ReadAllBytes(path); });
                        }
                        else
                        {
                            picWeb.Headers[HttpRequestHeader.UserAgent] = "Lea/0.2";
                            data = await picWeb.DownloadDataTaskAsync(block.U);
                        }
                        using (var stream = new MemoryStream(data))
                        using (var loaded = Image.FromStream(stream)) image = new Bitmap(loaded);
                    }
                    catch
                    {
                        image = null;
                    }
                    StorePic(key, image);
                }
            }
            finally
            {
                picBusy = false;
            }
        }

        private void StorePic(string key, Image image)
        {
            pics[key] = image;
            picOrder.Enqueue(key);
            while (picOrder.Count > PicLimit)
            {
                var old = picOrder.Dequeue();
                Image removed;
                if (!pics.TryGetValue(old, out removed)) continue;
                pics.Remove(old);
                if (removed != null) removed.Dispose();
            }
        }

        private int DrawPic(Graphics graphics, B block, int x, int y, int width, Color color, Color background)
        {
            Image image;
            if (block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null)
            {
                var scale = Math.Min(1F, Math.Min((float)width / image.Width, 360F / image.Height));
                var drawWidth = Math.Max(1, (int)(image.Width * scale));
                var drawHeight = Math.Max(1, (int)(image.Height * scale));
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(x, y, drawWidth, drawHeight));
                return drawHeight;
            }

            var height = 72;
            using (var brush = new SolidBrush(Color.FromArgb(20, color))) graphics.FillRectangle(brush, x, y, width, height);
            using (var pen = new Pen(Color.FromArgb(80, color))) graphics.DrawRectangle(pen, x, y, width - 1, height - 1);
            TextRenderer.DrawText(graphics, block.X.Length == 0 ? "Image unavailable" : block.X, Font, new Rectangle(x + 8, y + 8, width - 16, height - 16), color, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            return height;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            sy = Math.Max(0, sy - Math.Sign(e.Delta) * 72);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = HitAt(e.Location) == null ? Cursors.Default : Cursors.Hand;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hit = HitAt(e.Location);
            if (hit != null) Go(hit.U.ToString());
        }

        private Hit HitAt(Point point)
        {
            foreach (var hit in hits) if (hit.R.Contains(point)) return hit;
            return null;
        }

        private static int Draw(Graphics g, string text, Font font, Color color, Color? background, int x, int y, int width)
        {
            var size = TextRenderer.MeasureText(g, text ?? "", font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            var height = Math.Max(font.Height, size.Height);
            var rect = new Rectangle(x, y, width, height);
            if (background.HasValue)
            {
                using (var brush = new SolidBrush(background.Value)) g.FillRectangle(brush, rect);
            }
            TextRenderer.DrawText(g, text ?? "", font, rect, color, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return height;
        }

        private static Color CssColor(string value, Color fallback)
        {
            if (String.IsNullOrWhiteSpace(value)) return fallback;
            try { return ColorTranslator.FromHtml(value.Trim()); }
            catch { return fallback; }
        }

        private void OnChanged()
        {
            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                picWeb.Dispose();
                foreach (var image in pics.Values) if (image != null) image.Dispose();
                pics.Clear();
            }
            base.Dispose(disposing);
        }

        private sealed class Hit
        {
            public readonly Rectangle R;
            public readonly Uri U;

            public Hit(Rectangle rect, Uri url)
            {
                R = rect;
                U = url;
            }
        }
    }
}
