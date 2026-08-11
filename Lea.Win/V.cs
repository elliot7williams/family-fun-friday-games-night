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
        private readonly List<Bx> boxes = new List<Bx>();
        private readonly WebClient picWeb = new WebClient();
        private readonly Dictionary<string, Image> pics = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> picOrder = new Queue<string>();
        private const int PicLimit = 24;
        private int sy;
        private int docHeight;
        private int layoutWidth = -1;
        private bool layoutDirty = true;
        private bool picBusy;
        private bool picStop;

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
        public int Dh { get { return docHeight; } }
        public int Sy { get { return sy; } }
        public int Bc { get { return boxes.Count; } }

        public P Go(string input)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Go(input), true); }
            finally { Cursor = Cursors.Default; }
        }

        public async Task<P> GoA(string input)
        {
            Cursor = Cursors.WaitCursor;
            picStop = false;
            try
            {
                var page = Show(await e.GoA(input), false);
                await PicsA(page);
                Dirty();
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                return page;
            }
            finally { Cursor = Cursors.Default; }
        }

        public P Bk() { return Show(e.Bk(), false); }
        public P Fw() { return Show(e.Fw(), false); }
        public P Rl() { return Show(e.Rl(), true); }

        public async Task<P> RlA()
        {
            picStop = false;
            var page = Show(await e.RlA(), false);
            await PicsA(page);
            Dirty();
            Invalidate();
            if (Done != null) Done(this, EventArgs.Empty);
            return page;
        }

        public void St()
        {
            picStop = true;
            e.St();
            picWeb.CancelAsync();
            Cursor = Cursors.Default;
        }

        private P Show(P page, bool done)
        {
            sy = 0;
            Dirty();
            OnChanged();
            Invalidate();
            if (done && Done != null) Done(this, EventArgs.Empty);
            return page;
        }

        private void Dirty()
        {
            layoutDirty = true;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Dirty();
            Invalidate();
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
            if (layoutDirty || layoutWidth != ClientSize.Width) Lay(pe.Graphics);

            foreach (var box in boxes)
            {
                var rect = box.R;
                rect.Offset(0, -sy);
                if (rect.Bottom < 0 || rect.Top > ClientSize.Height) continue;
                if (box.K == "img")
                {
                    PaintPic(pe.Graphics, box, rect, pageColor);
                    continue;
                }

                using (var font = BoxFont(box.K, box.S, box.Title))
                {
                    var color = box.K == "a" ? Color.FromArgb(0, 102, 204) : pageColor;
                    if (box.S != null) color = CssColor(box.S.F, color);
                    if (box.S != null && box.S.Bg.Length > 0)
                    {
                        using (var brush = new SolidBrush(CssColor(box.S.Bg, pageBackground))) pe.Graphics.FillRectangle(brush, rect);
                    }
                    TextRenderer.DrawText(pe.Graphics, box.X, font, rect, color, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                }

                if (box.U != null) hits.Add(new Hit(rect, box.U));
            }
        }

        private void Lay(Graphics graphics)
        {
            boxes.Clear();
            if (At == null) return;
            var style = At.S;
            var left = style == null ? 0 : style.Ml + style.Pl;
            var right = style == null ? 0 : style.Mr + style.Pr;
            var x = 20 + left;
            var width = Math.Max(120, ClientSize.Width - x - 20 - right);
            var y = 18 + (style == null ? 0 : style.Mt + style.Pt);

            using (var titleFont = BoxFont("#title", style, true))
            {
                var height = Measure(graphics, At.T, titleFont, width);
                boxes.Add(new Bx("#title", At.T, null, style, new Rectangle(x, y, width, height), true));
                y += height + 14;
            }

            if (At.Bs.Count == 0)
            {
                using (var font = BoxFont("p", style, false))
                {
                    var height = Measure(graphics, At.X, font, width);
                    boxes.Add(new Bx("p", At.X, null, style, new Rectangle(x, y, width, height), false));
                    y += height + 18;
                }
            }
            else
            {
                foreach (var block in At.Bs)
                {
                    var bs = block.S;
                    y += bs == null ? 0 : bs.Mt + bs.Pt;
                    var bx = x + (bs == null ? 0 : bs.Ml + bs.Pl);
                    var bw = Math.Max(40, width - (bs == null ? 0 : bs.Ml + bs.Mr + bs.Pl + bs.Pr));
                    int height;
                    if (block.K == "img") height = PicSize(block, bw).Height;
                    else
                    {
                        using (var font = BoxFont(block.K, bs, false)) height = Measure(graphics, block.K == "li" ? "- " + block.X : block.X, font, bw);
                    }
                    var text = block.K == "li" ? "- " + block.X : block.X;
                    var link = block.K == "a" ? block.U : null;
                    boxes.Add(new Bx(block.K, text, link, bs, new Rectangle(bx, y, bw, height), false, block));
                    y += height + (block.K.StartsWith("h") ? 12 : 8) + (bs == null ? 0 : bs.Pb + bs.Mb);
                }
            }

            docHeight = y + 20 + (style == null ? 0 : style.Pb + style.Mb);
            layoutWidth = ClientSize.Width;
            layoutDirty = false;
            sy = Math.Min(sy, MaxScroll());
        }

        private static int Measure(Graphics graphics, string text, Font font, int width)
        {
            var size = TextRenderer.MeasureText(graphics, text ?? "", font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(font.Height, size.Height);
        }

        private Font BoxFont(string kind, S style, bool title)
        {
            var name = kind == "pre" || kind == "code" ? "Consolas" : Font.FontFamily.Name;
            var size = title ? 20F : kind == "h1" ? 18F : kind == "h2" ? 15F : kind == "h3" ? 12F : Font.Size;
            if (!title && style != null && style.Fs > 0) size = style.Fs;
            var fontStyle = title || kind.StartsWith("h") || (style != null && style.B) ? FontStyle.Bold : FontStyle.Regular;
            if (kind == "a") fontStyle |= FontStyle.Underline;
            return new Font(name, size, fontStyle);
        }

        private Size PicSize(B block, int width)
        {
            Image image;
            if (block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
            {
                var scale = Math.Min(1F, Math.Min((float)width / image.Width, 360F / image.Height));
                return new Size(Math.Max(1, (int)(image.Width * scale)), Math.Max(1, (int)(image.Height * scale)));
            }
            return new Size(width, 72);
        }

        private void PaintPic(Graphics graphics, Bx box, Rectangle rect, Color color)
        {
            Image image;
            if (box.B != null && box.B.U != null && pics.TryGetValue(box.B.U.AbsoluteUri, out image) && image != null)
            {
                var size = PicSize(box.B, rect.Width);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(rect.X, rect.Y, size.Width, size.Height));
                return;
            }

            using (var brush = new SolidBrush(Color.FromArgb(20, color))) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(Color.FromArgb(80, color))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            TextRenderer.DrawText(graphics, box.X.Length == 0 ? "Image unavailable" : box.X, Font, new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16), color, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private async Task PicsA(P page)
        {
            if (page == null) return;
            picBusy = true;
            try
            {
                foreach (var block in page.Bs)
                {
                    if (picStop) break;
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
                    catch { image = null; }
                    StorePic(key, image);
                }
            }
            finally { picBusy = false; }
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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Sc(-Math.Sign(e.Delta) * 72);
        }

        public void Sc(int delta)
        {
            sy = Math.Max(0, Math.Min(MaxScroll(), sy + delta));
            Invalidate();
        }

        private int MaxScroll()
        {
            return Math.Max(0, docHeight - ClientSize.Height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = HitAt(e.Location) == null ? Cursors.Default : Cursors.Hand;
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hit = HitAt(e.Location);
            if (hit == null) return;
            try { await GoA(hit.U.ToString()); }
            catch { Cursor = Cursors.Default; }
        }

        private Hit HitAt(Point point)
        {
            foreach (var hit in hits) if (hit.R.Contains(point)) return hit;
            return null;
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

        private sealed class Bx
        {
            public readonly string K;
            public readonly string X;
            public readonly Uri U;
            public readonly S S;
            public readonly Rectangle R;
            public readonly bool Title;
            public readonly B B;

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, bool title, B block = null)
            {
                K = kind;
                X = text ?? "";
                U = url;
                S = style;
                R = rect;
                Title = title;
                B = block;
            }
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
