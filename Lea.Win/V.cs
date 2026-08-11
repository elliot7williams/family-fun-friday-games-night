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
        private readonly TextBox ed = new TextBox();
        private const int PicLimit = 24;
        private int sy;
        private int docHeight;
        private int layoutWidth = -1;
        private bool layoutDirty = true;
        private bool picBusy;
        private bool picStop;
        private Fi edit;
        private Font editFont;
        private string editStart = "";
        private bool editLock;

        public event EventHandler Done;
        public event EventHandler Changed;
        public event EventHandler Ed;
        public event EventHandler Sb;

        public V()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(31, 41, 55);
            Font = new Font("Segoe UI", 10F);
            Dock = DockStyle.Fill;
            TabStop = true;

            ed.Visible = false;
            ed.BorderStyle = BorderStyle.FixedSingle;
            ed.TextChanged += EditChanged;
            ed.KeyDown += EditKeyDown;
            ed.LostFocus += delegate { EndEdit(false); };
            ed.MouseWheel += delegate(object sender, MouseEventArgs args) { Sc(-Math.Sign(args.Delta) * 72); };
            Controls.Add(ed);
        }

        public P At { get { return e.At; } }
        public Uri U { get { return At == null ? null : At.U; } }
        public string T { get { return At == null ? "" : At.T; } }
        public string H { get { return At == null ? "" : At.H; } }
        public string Q { get { return At == null ? "" : At.Q; } }
        public bool CanBk { get { return e.CanBk; } }
        public bool CanFw { get { return e.CanFw; } }
        public bool Busy { get { return e.Busy || picBusy; } }
        public int Dh { get { return docHeight; } }
        public int Sy { get { return sy; } }
        public int Bc { get { return boxes.Count; } }
        public Rectangle Rb(int index) { return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].R; }
        public Rectangle Rc(int index) { return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].C; }

        public P Go(string input)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Go(input), true); }
            finally { Cursor = Cursors.Default; }
        }

        public P Go(Uri url)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Go(url), true); }
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

        public async Task<P> GoA(Uri url)
        {
            Cursor = Cursors.WaitCursor;
            picStop = false;
            try
            {
                var page = Show(await e.GoA(url), false);
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
            EndEdit(false);
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
            PlaceEditor();

            foreach (var box in boxes)
            {
                var rect = box.R;
                rect.Offset(0, -sy);
                var content = box.C;
                content.Offset(0, -sy);
                if (rect.Bottom < 0 || rect.Top > ClientSize.Height) continue;
                if (!box.Title) PaintBox(pe.Graphics, rect, box.S, pageBackground, pageColor);
                if (box.K == "img")
                {
                    PaintPic(pe.Graphics, box, content, pageColor);
                    continue;
                }
                if (box.B != null && box.B.I != null)
                {
                    PaintField(pe.Graphics, box, content, pageColor);
                    hits.Add(new Hit(rect, null, box));
                    continue;
                }

                using (var font = BoxFont(box.K, box.S, box.Title))
                {
                    var color = box.K == "a" ? Color.FromArgb(0, 102, 204) : pageColor;
                    if (box.S != null) color = CssColor(box.S.F, color);
                    TextRenderer.DrawText(pe.Graphics, box.X, font, content, color, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                }

                if (box.U != null) hits.Add(new Hit(rect, box.U, null));
            }
        }

        private void Lay(Graphics graphics)
        {
            boxes.Clear();
            if (At == null) return;
            var style = At.S;
            var left = style == null ? 0 : style.Ml + style.Pl + BorderWidth(style, style.Bl);
            var right = style == null ? 0 : style.Mr + style.Pr + BorderWidth(style, style.Br);
            var x = 20 + left;
            var width = Math.Max(120, ClientSize.Width - x - 20 - right);
            if (style != null && style.W > 0) width = Math.Min(width, style.W);
            var y = 18 + (style == null ? 0 : style.Mt + style.Pt + BorderWidth(style, style.Bt));

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
                    var bt = bs == null ? 0 : BorderWidth(bs, bs.Bt);
                    var br = bs == null ? 0 : BorderWidth(bs, bs.Br);
                    var bb = bs == null ? 0 : BorderWidth(bs, bs.Bb);
                    var bl = bs == null ? 0 : BorderWidth(bs, bs.Bl);
                    var pt = bs == null ? 0 : bs.Pt;
                    var pr = bs == null ? 0 : bs.Pr;
                    var pb = bs == null ? 0 : bs.Pb;
                    var pl = bs == null ? 0 : bs.Pl;
                    var ml = bs == null ? 0 : bs.Ml;
                    var mr = bs == null ? 0 : bs.Mr;
                    y += bs == null ? 0 : bs.Mt;
                    var bx = x + ml;
                    var outerAvailable = Math.Max(40, width - ml - mr);
                    var bw = Math.Max(1, outerAvailable - bl - br - pl - pr);
                    if (bs != null && bs.W > 0) bw = Math.Min(bw, bs.W);
                    int height;
                    if (block.K == "img") height = PicSize(block, bw).Height;
                    else if (block.I != null)
                    {
                        using (var font = BoxFont(block.K, bs, false))
                        {
                            height = block.K == "textarea" ? Math.Max(84, font.Height * 3 + 12) : block.I.K == "checkbox" || block.I.K == "radio" ? Math.Max(24, font.Height + 4) : Math.Max(32, font.Height + 12);
                            if (block.K == "button")
                            {
                                var buttonWidth = TextRenderer.MeasureText(graphics, block.X, font, Size.Empty, TextFormatFlags.NoPadding).Width + 28;
                                if (bs == null || bs.W == 0) bw = Math.Min(bw, Math.Max(88, buttonWidth));
                            }
                        }
                    }
                    else
                    {
                        using (var font = BoxFont(block.K, bs, false)) height = Measure(graphics, block.K == "li" ? "- " + block.X : block.X, font, bw);
                    }
                    if (bs != null && bs.H > 0) height = bs.H;
                    var text = block.K == "li" ? "- " + block.X : block.X;
                    var link = block.K == "a" ? block.U : null;
                    var outer = new Rectangle(bx, y, bw + pl + pr + bl + br, height + pt + pb + bt + bb);
                    var content = new Rectangle(bx + bl + pl, y + bt + pt, bw, height);
                    boxes.Add(new Bx(block.K, text, link, bs, outer, content, false, block));
                    y += outer.Height + (block.K.StartsWith("h") ? 12 : 8) + (bs == null ? 0 : bs.Mb);
                }
            }

            docHeight = y + 20 + (style == null ? 0 : style.Pb + BorderWidth(style, style.Bb) + style.Mb);
            if (style != null && style.H > 0) docHeight = Math.Max(docHeight, 18 + style.Mt + style.H + style.Pt + style.Pb + BorderWidth(style, style.Bt) + BorderWidth(style, style.Bb) + style.Mb);
            layoutWidth = ClientSize.Width;
            layoutDirty = false;
            sy = Math.Min(sy, MaxScroll());
        }

        private static int Measure(Graphics graphics, string text, Font font, int width)
        {
            var size = TextRenderer.MeasureText(graphics, text ?? "", font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(font.Height, size.Height);
        }

        private static int BorderWidth(S style, int width)
        {
            return style == null || style.Bs == "" || style.Bs == "none" || style.Bs == "hidden" ? 0 : width;
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

        private static void PaintBox(Graphics graphics, Rectangle rect, S style, Color pageBackground, Color pageColor)
        {
            if (style == null) return;
            if (style.Bg.Length > 0)
                using (var brush = new SolidBrush(CssColor(style.Bg, pageBackground))) graphics.FillRectangle(brush, rect);

            var top = BorderWidth(style, style.Bt);
            var right = BorderWidth(style, style.Br);
            var bottom = BorderWidth(style, style.Bb);
            var left = BorderWidth(style, style.Bl);
            if (top + right + bottom + left == 0) return;
            var color = CssColor(style.Bc, CssColor(style.F, pageColor));
            PaintBorder(graphics, rect, top, 0, color, style.Bs);
            PaintBorder(graphics, rect, right, 1, color, style.Bs);
            PaintBorder(graphics, rect, bottom, 2, color, style.Bs);
            PaintBorder(graphics, rect, left, 3, color, style.Bs);
        }

        private static void PaintBorder(Graphics graphics, Rectangle rect, int width, int side, Color color, string style)
        {
            if (width < 1) return;
            if (style == "dashed" || style == "dotted")
            {
                using (var pen = new Pen(color, width))
                {
                    pen.DashStyle = style == "dashed" ? DashStyle.Dash : DashStyle.Dot;
                    if (side == 0) graphics.DrawLine(pen, rect.Left, rect.Top + width / 2, rect.Right, rect.Top + width / 2);
                    else if (side == 1) graphics.DrawLine(pen, rect.Right - width / 2 - 1, rect.Top, rect.Right - width / 2 - 1, rect.Bottom);
                    else if (side == 2) graphics.DrawLine(pen, rect.Left, rect.Bottom - width / 2 - 1, rect.Right, rect.Bottom - width / 2 - 1);
                    else graphics.DrawLine(pen, rect.Left + width / 2, rect.Top, rect.Left + width / 2, rect.Bottom);
                }
                return;
            }

            Rectangle border;
            if (side == 0) border = new Rectangle(rect.Left, rect.Top, rect.Width, width);
            else if (side == 1) border = new Rectangle(rect.Right - width, rect.Top, width, rect.Height);
            else if (side == 2) border = new Rectangle(rect.Left, rect.Bottom - width, rect.Width, width);
            else border = new Rectangle(rect.Left, rect.Top, width, rect.Height);
            using (var brush = new SolidBrush(color)) graphics.FillRectangle(brush, border);
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

        private void PaintField(Graphics graphics, Bx box, Rectangle rect, Color color)
        {
            var field = box.B.I;
            var disabled = field.Ds;
            var background = disabled ? Color.FromArgb(238, 238, 238) : Color.White;
            if (field.S != null && field.S.Bg.Length > 0) background = CssColor(field.S.Bg, background);
            var foreground = disabled ? Color.FromArgb(130, 130, 130) : field.S == null ? color : CssColor(field.S.F, color);

            if (field.K == "checkbox" || field.K == "radio")
            {
                var mark = new Rectangle(rect.X, rect.Y + 3, 17, 17);
                using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, mark);
                using (var pen = new Pen(Color.FromArgb(120, 120, 120)))
                {
                    if (field.K == "radio") graphics.DrawEllipse(pen, mark);
                    else graphics.DrawRectangle(pen, mark);
                }
                if (field.C)
                {
                    using (var pen = new Pen(disabled ? Color.Gray : Color.FromArgb(0, 102, 204), 2F))
                    {
                        if (field.K == "radio") graphics.FillEllipse(pen.Brush, new Rectangle(mark.X + 4, mark.Y + 4, 9, 9));
                        else
                        {
                            graphics.DrawLine(pen, mark.X + 3, mark.Y + 8, mark.X + 7, mark.Y + 13);
                            graphics.DrawLine(pen, mark.X + 7, mark.Y + 13, mark.X + 14, mark.Y + 4);
                        }
                    }
                }
                using (var font = BoxFont(box.K, field.S, false))
                    TextRenderer.DrawText(graphics, box.X, font, new Rectangle(rect.X + 24, rect.Y, Math.Max(1, rect.Width - 24), rect.Height), foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                return;
            }

            using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(box.K == "button" ? Color.FromArgb(145, 145, 145) : Color.FromArgb(165, 165, 165))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

            var text = field.V ?? "";
            var placeholder = text.Length == 0 && field.P.Length > 0;
            if (placeholder) text = field.P;
            if (field.K == "password" && !placeholder) text = new string('*', text.Length);
            var textColor = placeholder ? Color.FromArgb(130, 130, 130) : foreground;
            var flags = box.K == "button" ? TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding : TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            var pad = box.K == "button" ? 2 : 7;
            using (var font = BoxFont(box.K, field.S, false))
                TextRenderer.DrawText(graphics, box.K == "button" ? box.X : text, font, new Rectangle(rect.X + pad, rect.Y + 3, Math.Max(1, rect.Width - (pad * 2)), Math.Max(1, rect.Height - 6)), textColor, flags);
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
            PlaceEditor();
            Invalidate();
        }

        private int MaxScroll()
        {
            return Math.Max(0, docHeight - ClientSize.Height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var hit = HitAt(e.Location);
            Cursor = hit == null ? Cursors.Default : hit.B != null && CanEdit(hit.B.B.I) ? Cursors.IBeam : Cursors.Hand;
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hit = HitAt(e.Location);
            if (hit == null) return;
            try
            {
                if (hit.B != null) await FieldClickA(hit.B);
                else if (hit.U != null) await GoA(hit.U.ToString());
            }
            catch { Cursor = Cursors.Default; }
        }

        private async Task FieldClickA(Bx box)
        {
            var field = box.B.I;
            if (field == null || field.Ds) return;
            if (CanEdit(field))
            {
                StartEdit(field);
                return;
            }

            EndEdit(false);
            if (field.K == "checkbox")
            {
                field.C = !field.C;
                OnEdited();
            }
            else if (field.K == "radio")
            {
                foreach (var item in At.Fs)
                    if (item.K == "radio" && Object.ReferenceEquals(item.F, field.F) && item.N == field.N) item.C = false;
                field.C = true;
                OnEdited();
            }
            else if (field.K == "reset") Rs(field);
            else if (field.K == "submit") await SmA(field);
            else if (field.K == "button" && Sb != null) Sb(this, EventArgs.Empty);
            Invalidate();
        }

        private static bool CanEdit(Fi field)
        {
            return field != null && field.K != "checkbox" && field.K != "radio" && field.K != "submit" && field.K != "reset" && field.K != "button" && field.K != "file";
        }

        private void StartEdit(Fi field)
        {
            if (edit != field) EndEdit(false);
            edit = field;
            editStart = field.V;
            editLock = true;
            ed.Multiline = field.K == "textarea";
            ed.AcceptsReturn = ed.Multiline;
            ed.ScrollBars = ed.Multiline ? ScrollBars.Vertical : ScrollBars.None;
            ed.ReadOnly = field.Ro;
            ed.MaxLength = field.Mx > 0 ? field.Mx : 32767;
            ed.PasswordChar = field.K == "password" ? '*' : '\0';
            var oldFont = editFont;
            editFont = BoxFont(field.K, field.S, false);
            ed.Font = editFont;
            if (oldFont != null) oldFont.Dispose();
            ed.Text = field.V;
            editLock = false;
            PlaceEditor();
            ed.Visible = true;
            ed.Focus();
            ed.SelectAll();
        }

        private void EndEdit(bool restore)
        {
            if (edit == null) return;
            if (restore) edit.V = editStart;
            edit = null;
            ed.Visible = false;
            Invalidate();
        }

        private void EditChanged(object sender, EventArgs args)
        {
            if (editLock || edit == null) return;
            edit.V = ed.Text;
            OnEdited();
            Invalidate();
        }

        private void EditKeyDown(object sender, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Escape)
            {
                EndEdit(true);
                args.SuppressKeyPress = true;
            }
            else if (args.KeyCode == Keys.Enter && !ed.Multiline)
            {
                EndEdit(false);
                args.SuppressKeyPress = true;
            }
        }

        private void PlaceEditor()
        {
            if (edit == null) return;
            foreach (var box in boxes)
            {
                if (box.B == null || box.B.I != edit) continue;
                var rect = box.C;
                rect.Offset(0, -sy);
                if (rect.Bottom <= 0 || rect.Top >= ClientSize.Height)
                {
                    ed.Visible = false;
                    return;
                }
                ed.Bounds = new Rectangle(rect.X + 1, rect.Y + 1, Math.Max(1, rect.Width - 2), Math.Max(1, rect.Height - 2));
                ed.Visible = true;
                return;
            }
            ed.Visible = false;
        }

        public void Rs(Fi button)
        {
            if (At == null || button == null || button.F == null) return;
            foreach (var field in At.Fs)
            {
                if (!Object.ReferenceEquals(field.F, button.F)) continue;
                field.V = field.D;
                field.C = field.Dc;
            }
            OnEdited();
        }

        public P Sm(Fi button)
        {
            if (Sb != null) Sb(this, EventArgs.Empty);
            var target = FormUri(button);
            return target == null ? At : Go(target);
        }

        public async Task<P> SmA(Fi button)
        {
            if (Sb != null) Sb(this, EventArgs.Empty);
            var target = FormUri(button);
            return target == null ? At : await GoA(target);
        }

        private Uri FormUri(Fi button)
        {
            if (At == null || button == null || button.F == null || button.M != "get" || button.U == null) return null;

            var values = new List<string>();
            foreach (var field in At.Fs)
            {
                if (!Object.ReferenceEquals(field.F, button.F) || field.Ds || field.N.Length == 0) continue;
                if ((field.K == "checkbox" || field.K == "radio") && !field.C) continue;
                if (field.K == "reset" || field.K == "button" || (field.K == "submit" && field != button)) continue;
                values.Add(Uri.EscapeDataString(field.N) + "=" + Uri.EscapeDataString(field.V ?? ""));
            }

            var target = new UriBuilder(button.U);
            var query = target.Query.TrimStart('?');
            if (query.Length > 0 && values.Count > 0) query += "&";
            target.Query = query + String.Join("&", values.ToArray());
            return target.Uri;
        }

        private void OnEdited()
        {
            if (Ed != null) Ed(this, EventArgs.Empty);
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
                if (editFont != null) editFont.Dispose();
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
            public readonly Rectangle C;
            public readonly bool Title;
            public readonly B B;

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, bool title, B block = null)
                : this(kind, text, url, style, rect, rect, title, block)
            {
            }

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, Rectangle content, bool title, B block = null)
            {
                K = kind;
                X = text ?? "";
                U = url;
                S = style;
                R = rect;
                C = content;
                Title = title;
                B = block;
            }
        }

        private sealed class Hit
        {
            public readonly Rectangle R;
            public readonly Uri U;
            public readonly Bx B;

            public Hit(Rectangle rect, Uri url, Bx box)
            {
                R = rect;
                U = url;
                B = box;
            }
        }
    }
}
