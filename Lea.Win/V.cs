using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
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
        private readonly Dictionary<string, string> fontNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> picOrder = new Queue<string>();
        private readonly TextBox ed = new TextBox();
        private const int PicLimit = 24;
        private const int FontLimit = 32;
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
        private ContextMenuStrip choices;

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
        public int Kc { get { return e.Kc; } }
        public int Km { get { return e.Km; } }
        public bool Pv { get { return e.Pv; } set { e.Pv = value; } }
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

        public P Po(Uri url, string data)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Po(url, data), true); }
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

        public async Task<P> PoA(Uri url, string data)
        {
            Cursor = Cursors.WaitCursor;
            picStop = false;
            try
            {
                var page = Show(await e.PoA(url, data), false);
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

        public void Kcl() { e.Kcl(); }

        private P Show(P page, bool done)
        {
            CloseChoices();
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

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (fontNames != null) fontNames.Clear();
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
            pe.Graphics.Clear(BackColor);
            if (At.S != null && !Hidden(At.S) && At.S.Bg.Length > 0 && At.S.Op > 0)
                using (var brush = new SolidBrush(PaintColor(pageBackground, At.S.Op))) pe.Graphics.FillRectangle(brush, ClientRectangle);
            if (layoutDirty || layoutWidth != ClientSize.Width) Lay(pe.Graphics);
            PlaceEditor();

            foreach (var box in boxes)
            {
                var rect = box.R;
                rect.Offset(0, -sy);
                var content = box.C;
                content.Offset(0, -sy);
                if (rect.Bottom < 0 || rect.Top > ClientSize.Height) continue;
                if (Hidden(box.S)) continue;
                var opacity = box.S == null ? 100 : box.S.Op;
                var clip = box.Clip;
                if (!clip.IsEmpty) clip.Offset(0, -sy);
                GraphicsState state = null;
                try
                {
                    if (!clip.IsEmpty)
                    {
                        state = pe.Graphics.Save();
                        pe.Graphics.SetClip(clip, CombineMode.Intersect);
                    }
                    if (opacity > 0 && !box.Title && (!box.Inline || box.B == null || box.B.Is.Count > 0)) PaintBox(pe.Graphics, rect, box.S, pageBackground, pageColor, opacity, box.B == null || box.B.I == null);
                    if (box.K == "img")
                    {
                        if (opacity > 0) PaintPic(pe.Graphics, box, content, pageColor, opacity);
                        continue;
                    }
                    if (box.K == "hr")
                    {
                        var color = CssColor(box.S == null ? "" : box.S.F, Color.FromArgb(148, 163, 184));
                        if (opacity > 0)
                            using (var pen = new Pen(PaintColor(color, opacity))) pe.Graphics.DrawLine(pen, content.Left, content.Top + content.Height / 2, content.Right, content.Top + content.Height / 2);
                        continue;
                    }
                    if (box.B != null && box.B.I != null)
                    {
                        if (opacity > 0) PaintField(pe.Graphics, box, content, pageColor, opacity);
                        hits.Add(new Hit(VisibleRect(rect, clip), null, box));
                        continue;
                    }

                    using (var font = BoxFont(box.K, box.S, box.Title, box.U != null, box.Inline && box.B != null ? box.B.K : null))
                    {
                        var color = box.U != null ? Color.FromArgb(0, 102, 204) : pageColor;
                        if (box.S != null) color = CssColor(box.S.F, color);
                        var flags = box.Inline ? TextFormatFlags.SingleLine | TextFormatFlags.NoPadding : TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
                        if (!clip.IsEmpty) flags |= TextFormatFlags.PreserveGraphicsClipping;
                        if (opacity > 0) PaintText(pe.Graphics, box.X, font, content, color, flags, opacity);
                    }

                    if (box.U != null)
                    {
                        var hit = VisibleRect(rect, clip);
                        if (!hit.IsEmpty) hits.Add(new Hit(hit, box.U, null));
                    }
                }
                finally { if (state != null) pe.Graphics.Restore(state); }
            }
        }

        private static Rectangle VisibleRect(Rectangle rect, Rectangle clip)
        {
            return clip.IsEmpty ? rect : Rectangle.Intersect(rect, clip);
        }

        private void Lay(Graphics graphics)
        {
            boxes.Clear();
            if (At == null) return;
            var style = At.S;
            var left = style == null ? 0 : style.Ml + style.Pl + BorderWidth(style, style.Bl);
            var right = style == null ? 0 : style.Mr + style.Pr + BorderWidth(style, style.Br);
            var x = 20 + left;
            var width = ConstrainWidth(style, Math.Max(120, ClientSize.Width - x - 20 - right));
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
                    if (block.K == "table" && block.Tb != null)
                    {
                        y = LayTable(graphics, block, x, width, y);
                        continue;
                    }
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
                    var bw = ConstrainWidth(bs, Math.Max(1, outerAvailable - bl - br - pl - pr));
                    int height;
                    List<Ix> inline = null;
                    if (block.K == "img") height = PicSize(block, bw).Height;
                    else if (block.K == "hr") height = 2;
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
                    else if (NeedsFlow(block)) inline = InlineLayout(graphics, block, bw, out height);
                    else
                    {
                        using (var font = BoxFont(block.K, bs, false)) height = Measure(graphics, block.K == "li" ? "- " + block.X : block.X, font, bw);
                    }
                    height = ConstrainHeight(bs, height);
                    var text = block.K == "li" ? "- " + block.X : block.X;
                    var link = inline == null && block.K == "a" ? block.U : null;
                    var outer = new Rectangle(bx, y, bw + pl + pr + bl + br, height + pt + pb + bt + bb);
                    var content = new Rectangle(bx + bl + pl, y + bt + pt, bw, height);
                    boxes.Add(new Bx(block.K, inline == null ? text : "", link, bs, outer, content, false, block));
                    if (inline != null) AddInlineBoxes(block, content, inline);
                    y += outer.Height + (HeadingKind(block.K) ? 12 : 8) + (bs == null ? 0 : bs.Mb);
                }
            }

            docHeight = y + 20 + (style == null ? 0 : style.Pb + BorderWidth(style, style.Bb) + style.Mb);
            if (style != null && style.H > 0) docHeight = Math.Max(docHeight, 18 + style.Mt + style.H + style.Pt + style.Pb + BorderWidth(style, style.Bt) + BorderWidth(style, style.Bb) + style.Mb);
            layoutWidth = ClientSize.Width;
            layoutDirty = false;
            sy = Math.Min(sy, MaxScroll());
        }

        private int LayTable(Graphics graphics, B block, int x, int width, int y)
        {
            var style = block.S;
            var bt = style == null ? 0 : BorderWidth(style, style.Bt);
            var br = style == null ? 0 : BorderWidth(style, style.Br);
            var bb = style == null ? 0 : BorderWidth(style, style.Bb);
            var bl = style == null ? 0 : BorderWidth(style, style.Bl);
            var pt = style == null ? 0 : style.Pt;
            var pr = style == null ? 0 : style.Pr;
            var pb = style == null ? 0 : style.Pb;
            var pl = style == null ? 0 : style.Pl;
            var ml = style == null ? 0 : style.Ml;
            var mr = style == null ? 0 : style.Mr;
            y += style == null ? 0 : style.Mt;
            var bx = x + ml;
            var available = Math.Max(40, width - ml - mr);
            var tableWidth = ConstrainWidth(style, Math.Max(1, available - bl - br - pl - pr));
            var contentX = bx + bl + pl;
            var contentY = y + bt + pt;
            var rowY = contentY;
            var first = boxes.Count;

            if (block.Tb.Cp.Length > 0)
            {
                using (var font = BoxFont("caption", style, false))
                {
                    var height = Measure(graphics, block.Tb.Cp, font, tableWidth);
                    boxes.Add(new Bx("caption", block.Tb.Cp, null, style, new Rectangle(contentX, rowY, tableWidth, height), false, block));
                    rowY += height + 8;
                }
            }

            var columns = Math.Max(1, block.Tb.Cs);
            foreach (var row in block.Tb.Rs)
            {
                var rowHeight = 1;
                var column = 0;
                foreach (var cell in row.Cs)
                {
                    var span = Math.Max(1, Math.Min(cell.Sp, columns - Math.Min(column, columns - 1)));
                    var rightColumn = Math.Min(columns, column + span);
                    var cellLeft = contentX + tableWidth * column / columns;
                    var cellRight = contentX + tableWidth * rightColumn / columns;
                    var cellWidth = Math.Max(1, cellRight - cellLeft);
                    var cellBox = CellBox(cell.S, cellWidth);
                    var height = ConstrainHeight(cell.S, CellContentHeight(graphics, cell, cellBox.Width));
                    rowHeight = Math.Max(rowHeight, height + cellBox.Height);
                    column = rightColumn;
                }

                column = 0;
                foreach (var cell in row.Cs)
                {
                    var span = Math.Max(1, Math.Min(cell.Sp, columns - Math.Min(column, columns - 1)));
                    var rightColumn = Math.Min(columns, column + span);
                    var cellLeft = contentX + tableWidth * column / columns;
                    var cellRight = contentX + tableWidth * rightColumn / columns;
                    var cellWidth = Math.Max(1, cellRight - cellLeft);
                    var inset = CellInset(cell.S);
                    var outer = new Rectangle(cellLeft, rowY, cellWidth, rowHeight);
                    var content = new Rectangle(cellLeft + inset.Left, rowY + inset.Top, Math.Max(1, cellWidth - inset.Left - inset.Right), Math.Max(1, rowHeight - inset.Top - inset.Bottom));
                    var simple = SimpleCellBlock(cell);
                    if (simple != null && !NeedsFlow(simple)) boxes.Add(new Bx(cell.H ? "th" : "td", cell.X, null, cell.S, outer, content, false, block));
                    else
                    {
                        boxes.Add(new Bx(cell.H ? "th" : "td", "", null, cell.S, outer, content, false, block));
                        if (simple != null)
                        {
                            int flowHeight;
                            AddInlineBoxes(simple, content, InlineLayout(graphics, simple, content.Width, out flowHeight));
                        }
                        else LayCellBlocks(graphics, cell, content);
                    }
                    column = rightColumn;
                }
                rowY += rowHeight;
            }

            var contentHeight = ConstrainHeight(style, rowY - contentY);
            var outerTable = new Rectangle(bx, y, tableWidth + pl + pr + bl + br, contentHeight + pt + pb + bt + bb);
            boxes.Insert(first, new Bx("table", "", null, style, outerTable, new Rectangle(contentX, contentY, tableWidth, contentHeight), false, block));
            return y + outerTable.Height + 8 + (style == null ? 0 : style.Mb);
        }

        private int CellContentHeight(Graphics graphics, Tc cell, int width)
        {
            if (SimpleCell(cell))
            {
                var part = SimpleCellBlock(cell);
                if (part != null && NeedsFlow(part))
                {
                    int flowHeight;
                    InlineLayout(graphics, part, width, out flowHeight);
                    return flowHeight;
                }
                using (var font = BoxFont(cell.H ? "th" : "td", cell.S, false)) return Measure(graphics, cell.X, font, width);
            }

            var height = 0;
            foreach (var part in cell.Bs) height += CellPartHeight(graphics, part, width);
            return Math.Max(1, height);
        }

        private void LayCellBlocks(Graphics graphics, Tc cell, Rectangle content)
        {
            var y = content.Y;
            foreach (var part in cell.Bs)
            {
                var style = part.S;
                var ml = style == null ? 0 : style.Ml;
                var mr = style == null ? 0 : style.Mr;
                var mt = style == null ? 0 : style.Mt;
                var mb = style == null ? 0 : style.Mb;
                var bt = style == null ? 0 : BorderWidth(style, style.Bt);
                var br = style == null ? 0 : BorderWidth(style, style.Br);
                var bb = style == null ? 0 : BorderWidth(style, style.Bb);
                var bl = style == null ? 0 : BorderWidth(style, style.Bl);
                var pt = style == null ? 0 : style.Pt;
                var pr = style == null ? 0 : style.Pr;
                var pb = style == null ? 0 : style.Pb;
                var pl = style == null ? 0 : style.Pl;
                var available = ConstrainWidth(style, Math.Max(1, content.Width - ml - mr - bl - br - pl - pr));
                int textHeight;
                List<Ix> flow = null;
                if (part.K != "br" && NeedsFlow(part)) flow = InlineLayout(graphics, part, available, out textHeight);
                else using (var font = BoxFont(part.K, style, false)) textHeight = part.K == "br" ? Math.Max(4, font.Height / 2) : Measure(graphics, CellText(part), font, available);
                textHeight = ConstrainHeight(style, textHeight);
                y += mt;
                var outer = new Rectangle(content.X + ml, y, available + pl + pr + bl + br, textHeight + pt + pb + bt + bb);
                var inner = new Rectangle(outer.X + bl + pl, outer.Y + bt + pt, available, textHeight);
                boxes.Add(new Bx(part.K, flow == null ? CellText(part) : "", flow == null && part.K == "a" ? part.U : null, style, outer, inner, false, part));
                if (flow != null) AddInlineBoxes(part, inner, flow);
                y += outer.Height + mb + CellGap(part.K);
            }
        }

        private int CellPartHeight(Graphics graphics, B part, int width)
        {
            var style = part.S;
            var horizontal = style == null ? 0 : style.Ml + style.Mr + style.Pl + style.Pr + BorderWidth(style, style.Bl) + BorderWidth(style, style.Br);
            var available = ConstrainWidth(style, Math.Max(1, width - horizontal));
            int height;
            if (part.K != "br" && NeedsFlow(part)) InlineLayout(graphics, part, available, out height);
            else using (var font = BoxFont(part.K, style, false)) height = part.K == "br" ? Math.Max(4, font.Height / 2) : Measure(graphics, CellText(part), font, available);
            height = ConstrainHeight(style, height);
            if (style != null) height += style.Mt + style.Mb + style.Pt + style.Pb + BorderWidth(style, style.Bt) + BorderWidth(style, style.Bb);
            return height + CellGap(part.K);
        }

        private static bool SimpleCell(Tc cell)
        {
            if (cell.Bs.Count == 0) return true;
            var part = cell.Bs.Count == 1 ? cell.Bs[0] : null;
            return part != null && part.U == null && part.K == (cell.H ? "th" : "td");
        }

        private static B SimpleCellBlock(Tc cell)
        {
            return SimpleCell(cell) && cell.Bs.Count == 1 ? cell.Bs[0] : null;
        }

        private static string CellText(B part)
        {
            return part.K == "li" ? "- " + part.X : part.X;
        }

        private static int CellGap(string kind)
        {
            return HeadingKind(kind) ? 6 : kind == "p" ? 4 : kind == "br" ? 2 : 3;
        }

        private static Size CellBox(S style, int width)
        {
            var inset = CellInset(style);
            return new Size(Math.Max(1, width - inset.Left - inset.Right), inset.Top + inset.Bottom);
        }

        private static Padding CellInset(S style)
        {
            var top = style == null ? 0 : BorderWidth(style, style.Bt) + style.Pt;
            var right = style == null ? 0 : BorderWidth(style, style.Br) + style.Pr;
            var bottom = style == null ? 0 : BorderWidth(style, style.Bb) + style.Pb;
            var left = style == null ? 0 : BorderWidth(style, style.Bl) + style.Pl;
            if (top + right + bottom + left == 0) return new Padding(6);
            return new Padding(left, top, right, bottom);
        }

        private static int Measure(Graphics graphics, string text, Font font, int width)
        {
            var size = TextRenderer.MeasureText(graphics, text ?? "", font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(font.Height, size.Height);
        }

        private static int ConstrainWidth(S style, int available)
        {
            available = Math.Max(1, available);
            var width = style != null && style.W > 0 ? Math.Min(available, style.W) : available;
            if (style != null && style.Wx > 0) width = Math.Min(width, style.Wx);
            if (style != null && style.Wn > 0) width = Math.Max(width, Math.Min(available, style.Wn));
            return Math.Max(1, width);
        }

        private static int ConstrainHeight(S style, int natural)
        {
            var height = style != null && style.H > 0 ? style.H : Math.Max(1, natural);
            if (style != null && style.Hx > 0) height = Math.Min(height, style.Hx);
            if (style != null && style.Hn > 0) height = Math.Max(height, style.Hn);
            return Math.Max(1, height);
        }

        private List<Ix> InlineLayout(Graphics graphics, B block, int width, out int height)
        {
            int baseHeight;
            using (var font = BoxFont(block.K, block.S, false)) baseHeight = CssLineHeight(block.S, font.Height);
            var lines = new List<Il>();
            var line = new Il();
            Ia pending = null;
            var endedWithBreak = false;
            var space = block.S == null ? "" : block.S.Ws;
            var wrap = space != "nw" && space != "p";

            if (block.K == "li")
            {
                AddInlineWord(graphics, lines, ref line, ref pending, InlineAtom(graphics, "li", "-", null, block.S, block.K), width, baseHeight, wrap, block.S == null ? "" : block.S.Wb);
                pending = InlineAtom(graphics, "li", " ", null, block.S, block.K);
            }

            if (block.Is.Count == 0)
            {
                AddInlineText(graphics, lines, ref line, ref pending, block.K, block.X, block.U, block.S, block.K, width, baseHeight, space, wrap, ref endedWithBreak);
            }
            else
            {
                foreach (var run in block.Is)
                {
                    if (run.K == "br")
                    {
                        FinishInlineLine(lines, ref line, baseHeight, true, true);
                        pending = null;
                        endedWithBreak = true;
                        continue;
                    }
                    AddInlineText(graphics, lines, ref line, ref pending, run.K, run.X, run.U, run.S, block.K, width, baseHeight, space, wrap, ref endedWithBreak);
                }
            }

            if (line.As.Count > 0 || lines.Count == 0 || endedWithBreak) FinishInlineLine(lines, ref line, baseHeight, true);

            var layout = new List<Ix>();
            var y = 0;
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var current = lines[lineIndex];
                var align = block.S == null ? "" : block.S.Ta;
                var justify = align == "j" && lineIndex < lines.Count - 1 && !current.Br;
                var spaces = 0;
                if (justify) foreach (var atom in current.As) if (atom.X == " ") spaces++;
                var stretch = spaces == 0 ? 0 : Math.Max(0, width - current.W);
                var each = spaces == 0 ? 0 : stretch / spaces;
                var remainder = spaces == 0 ? 0 : stretch % spaces;
                var x = align == "c" ? Math.Max(0, (width - current.W) / 2) : align == "r" ? Math.Max(0, width - current.W) : 0;
                foreach (var atom in current.As)
                {
                    var atomWidth = atom.W;
                    if (justify && atom.X == " ")
                    {
                        atomWidth += each;
                        if (remainder-- > 0) atomWidth++;
                    }
                    var rect = new Rectangle(x, y + Math.Max(0, (current.H - atom.H) / 2), atomWidth, atom.H);
                    layout.Add(new Ix(atom.K, atom.X, atom.U, atom.S, rect));
                    x += atomWidth;
                }
                y += current.H;
            }
            height = Math.Max(baseHeight, y);
            return layout;
        }

        private void AddInlineText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, int width, int baseHeight, string space, bool wrap, ref bool endedWithBreak)
        {
            value = value ?? "";
            if (space == "p" || space == "pw")
            {
                AddPreservedText(graphics, lines, ref line, ref pending, kind, value, url, style, baseKind, width, baseHeight, wrap, ref endedWithBreak);
                return;
            }

            var index = 0;
            while (index < value.Length)
            {
                var lineBreak = space == "pl" ? LineBreak(value, index) : 0;
                if (lineBreak > 0)
                {
                    FinishInlineLine(lines, ref line, baseHeight, true, true);
                    pending = null;
                    endedWithBreak = true;
                    index += lineBreak;
                    continue;
                }
                if (Char.IsWhiteSpace(value[index]))
                {
                    while (index < value.Length && Char.IsWhiteSpace(value[index]) && (space != "pl" || LineBreak(value, index) == 0)) index++;
                    pending = InlineAtom(graphics, kind, " ", null, style, baseKind);
                    continue;
                }

                var start = index;
                while (index < value.Length && !Char.IsWhiteSpace(value[index])) index++;
                var word = InlineAtom(graphics, kind, value.Substring(start, index - start), url, style, baseKind);
                AddInlineWord(graphics, lines, ref line, ref pending, word, width, baseHeight, wrap, style == null ? "" : style.Wb);
                endedWithBreak = false;
            }
        }

        private void AddPreservedText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, int width, int baseHeight, bool wrap, ref bool endedWithBreak)
        {
            var index = 0;
            while (index < value.Length)
            {
                var lineBreak = LineBreak(value, index);
                if (lineBreak > 0)
                {
                    FinishInlineLine(lines, ref line, baseHeight, true, true);
                    pending = null;
                    endedWithBreak = true;
                    index += lineBreak;
                    continue;
                }

                var spaces = Char.IsWhiteSpace(value[index]);
                var text = new StringBuilder();
                while (index < value.Length && LineBreak(value, index) == 0 && Char.IsWhiteSpace(value[index]) == spaces)
                {
                    text.Append(value[index] == '\t' ? "    " : value[index].ToString());
                    index++;
                }
                var atom = InlineAtom(graphics, kind, text.ToString(), url, style, baseKind);
                AddInlineWord(graphics, lines, ref line, ref pending, atom, width, baseHeight, wrap, style == null ? "" : style.Wb);
                endedWithBreak = false;
            }
        }

        private static int LineBreak(string value, int index)
        {
            if (index < 0 || index >= value.Length) return 0;
            if (value[index] == '\n') return 1;
            if (value[index] != '\r') return 0;
            return index + 1 < value.Length && value[index + 1] == '\n' ? 2 : 1;
        }

        private void AddInlineWord(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, Ia word, int width, int baseHeight, bool wrap, string mode)
        {
            if (!wrap)
            {
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            var breaking = String.IsNullOrEmpty(mode) ? "b" : mode;
            if (breaking == "n" || breaking == "k")
            {
                var gap = line.As.Count > 0 && pending != null ? pending.W : 0;
                if (line.As.Count > 0 && line.W + gap + word.W > width)
                {
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    pending = null;
                }
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            if (breaking != "a" && word.W <= width)
            {
                var gap = line.As.Count > 0 && pending != null ? pending.W : 0;
                if (line.As.Count > 0 && line.W + gap + word.W > width)
                {
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    pending = null;
                }
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            if (breaking == "b" && line.As.Count > 0) FinishInlineLine(lines, ref line, baseHeight, false);
            if (breaking == "a" && line.As.Count > 0 && pending != null)
            {
                if (line.W + pending.W >= width) FinishInlineLine(lines, ref line, baseHeight, false);
                else AddInlineAtom(line, pending);
            }
            pending = null;
            var piece = new StringBuilder();
            var pieceWidth = 0;
            var elements = StringInfo.GetTextElementEnumerator(word.X);
            while (elements.MoveNext())
            {
                var text = elements.GetTextElement();
                var character = InlineAtom(graphics, word.K, text, word.U, word.S, word.Z);
                if (piece.Length > 0 && line.W + pieceWidth + character.W > width)
                {
                    AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z));
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    piece.Clear();
                    pieceWidth = 0;
                }
                else if (piece.Length == 0 && line.As.Count > 0 && line.W + character.W > width)
                    FinishInlineLine(lines, ref line, baseHeight, false);
                piece.Append(text);
                pieceWidth += character.W;
            }
            if (piece.Length > 0) AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z));
        }

        private Ia InlineAtom(Graphics graphics, string kind, string text, Uri url, S style, string baseKind)
        {
            using (var font = BoxFont(kind, style, false, url != null, baseKind))
            {
                var width = TextRenderer.MeasureText(graphics, text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                return new Ia(kind, text, url, style, baseKind, Math.Max(1, width), font.Height, CssLineHeight(style, font.Height));
            }
        }

        private static void AddInlineAtom(Il line, Ia atom)
        {
            line.As.Add(atom);
            line.W += atom.W;
            line.H = Math.Max(line.H, atom.L);
        }

        private static void FinishInlineLine(List<Il> lines, ref Il line, int baseHeight, bool force, bool broken = false)
        {
            if (line.As.Count > 0 || force)
            {
                line.H = Math.Max(baseHeight, line.H);
                line.Br = broken;
                lines.Add(line);
            }
            line = new Il();
        }

        private static bool NeedsFlow(B block)
        {
            var style = block == null ? null : block.S;
            return block != null && (block.Is.Count > 0 || (style != null && (style.Ta.Length > 0 || style.Lh > 0 || style.Lm > 0 || style.Ws.Length > 0 || style.Wb.Length > 0 || style.Ov.Length > 0)));
        }

        private static int CssLineHeight(S style, int nativeHeight)
        {
            if (style == null) return nativeHeight;
            if (style.Lh > 0) return Math.Max(nativeHeight, style.Lh);
            if (style.Lm > 0) return Math.Max(nativeHeight, (int)Math.Round(nativeHeight * style.Lm / 100F));
            return nativeHeight;
        }

        private void AddInlineBoxes(B block, Rectangle content, IList<Ix> flow)
        {
            var clip = ClipsOverflow(block.S) ? content : Rectangle.Empty;
            foreach (var part in flow)
            {
                var rect = part.R;
                rect.Offset(content.X, content.Y);
                boxes.Add(new Bx(part.K, part.X, part.U, part.S, rect, false, block, true, clip));
            }
        }

        private static bool ClipsOverflow(S style)
        {
            return style != null && style.Ov.Length > 0 && style.Ov != "v";
        }

        private static int BorderWidth(S style, int width)
        {
            return style == null || style.Bs == "" || style.Bs == "none" || style.Bs == "hidden" ? 0 : width;
        }

        private Font BoxFont(string kind, S style, bool title, bool link = false, string baseKind = null)
        {
            var sizeKind = baseKind ?? kind;
            var name = title ? Font.FontFamily.Name : FontName(kind, style, sizeKind);
            var size = title ? 20F : sizeKind == "h1" ? 18F : sizeKind == "h2" ? 15F : sizeKind == "h3" ? 12F : sizeKind == "h4" ? 11F : sizeKind == "h5" ? 10F : sizeKind == "h6" ? 9F : Font.Size;
            if (!title && style != null && style.Fs > 0) size = style.Fs;
            var fontStyle = title || HeadingKind(kind) || kind == "caption" || kind == "th" || kind == "dt" || kind == "legend" || kind == "summary" || kind == "strong" || kind == "b" || (style != null && style.B) ? FontStyle.Bold : FontStyle.Regular;
            if (style != null && style.I) fontStyle |= FontStyle.Italic;
            var decoration = style == null ? "" : style.Td;
            if (decoration.IndexOf('u') >= 0 || ((kind == "a" || link) && decoration.Length == 0)) fontStyle |= FontStyle.Underline;
            if (decoration.IndexOf('s') >= 0) fontStyle |= FontStyle.Strikeout;
            return new Font(name, size, fontStyle);
        }

        private string FontName(string kind, S style, string sizeKind)
        {
            var fallback = kind == "pre" || kind == "code" || sizeKind == "pre" || sizeKind == "code" ? "Consolas" : Font.FontFamily.Name;
            if (style == null || style.Ff.Length == 0) return fallback;
            string found;
            if (fontNames.TryGetValue(style.Ff, out found)) return found;

            foreach (var value in style.Ff.Split('|'))
            {
                var name = value == "ss" || value == "ui" ? Font.FontFamily.Name : value == "s" ? "Times New Roman" : value == "m" ? "Consolas" : value == "c" ? "Segoe Script" : value == "f" ? "Impact" : value;
                try
                {
                    using (var family = new FontFamily(name)) found = family.Name;
                    if (fontNames.Count < FontLimit) fontNames[style.Ff] = found;
                    return found;
                }
                catch (ArgumentException) { }
            }
            return fallback;
        }

        private static bool HeadingKind(string kind)
        {
            return kind == "h1" || kind == "h2" || kind == "h3" || kind == "h4" || kind == "h5" || kind == "h6";
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

        private static void PaintBox(Graphics graphics, Rectangle rect, S style, Color pageBackground, Color pageColor, int opacity, bool background)
        {
            if (style == null) return;
            if (background && style.Bg.Length > 0)
                using (var brush = new SolidBrush(PaintColor(CssColor(style.Bg, pageBackground), opacity))) graphics.FillRectangle(brush, rect);

            var top = BorderWidth(style, style.Bt);
            var right = BorderWidth(style, style.Br);
            var bottom = BorderWidth(style, style.Bb);
            var left = BorderWidth(style, style.Bl);
            if (top + right + bottom + left == 0) return;
            var color = PaintColor(CssColor(style.Bc, CssColor(style.F, pageColor)), opacity);
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

        private void PaintPic(Graphics graphics, Bx box, Rectangle rect, Color color, int opacity)
        {
            Image image;
            if (box.B != null && box.B.U != null && pics.TryGetValue(box.B.U.AbsoluteUri, out image) && image != null)
            {
                var size = PicSize(box.B, rect.Width);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                PaintImage(graphics, image, new Rectangle(rect.X, rect.Y, size.Width, size.Height), opacity);
                return;
            }

            using (var brush = new SolidBrush(PaintColor(Color.FromArgb(20, color), opacity))) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(PaintColor(Color.FromArgb(80, color), opacity))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            PaintText(graphics, box.X.Length == 0 ? "Image unavailable" : box.X, Font, new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16), color, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak, opacity);
        }

        private void PaintField(Graphics graphics, Bx box, Rectangle rect, Color color, int opacity)
        {
            var field = box.B.I;
            var disabled = field.Ds;
            var background = disabled ? Color.FromArgb(238, 238, 238) : Color.White;
            if (field.S != null && field.S.Bg.Length > 0) background = CssColor(field.S.Bg, background);
            var foreground = disabled ? Color.FromArgb(130, 130, 130) : field.S == null ? color : CssColor(field.S.F, color);
            background = PaintColor(background, opacity);

            if (field.K == "checkbox" || field.K == "radio")
            {
                var mark = new Rectangle(rect.X, rect.Y + 3, 17, 17);
                using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, mark);
                using (var pen = new Pen(PaintColor(Color.FromArgb(120, 120, 120), opacity)))
                {
                    if (field.K == "radio") graphics.DrawEllipse(pen, mark);
                    else graphics.DrawRectangle(pen, mark);
                }
                if (field.C)
                {
                    using (var pen = new Pen(PaintColor(disabled ? Color.Gray : Color.FromArgb(0, 102, 204), opacity), 2F))
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
                    PaintText(graphics, box.X, font, new Rectangle(rect.X + 24, rect.Y, Math.Max(1, rect.Width - 24), rect.Height), foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding, opacity);
                return;
            }

            using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(PaintColor(box.K == "button" ? Color.FromArgb(145, 145, 145) : Color.FromArgb(165, 165, 165), opacity))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

            var text = field.K == "select" ? OptionText(field) : field.V ?? "";
            var placeholder = text.Length == 0 && field.P.Length > 0;
            if (placeholder) text = field.P;
            if (field.K == "password" && !placeholder) text = new string('*', text.Length);
            var textColor = placeholder ? Color.FromArgb(130, 130, 130) : foreground;
            var flags = box.K == "button" ? TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding : TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            var pad = box.K == "button" ? 2 : 7;
            var reserve = field.K == "select" ? 24 : 0;
            if (field.K == "select")
            {
                var middle = rect.Top + rect.Height / 2;
                var arrow = new Point[] { new Point(rect.Right - 17, middle - 2), new Point(rect.Right - 7, middle - 2), new Point(rect.Right - 12, middle + 4) };
                using (var brush = new SolidBrush(PaintColor(foreground, opacity))) graphics.FillPolygon(brush, arrow);
            }
            using (var font = BoxFont(box.K, field.S, false))
                PaintText(graphics, box.K == "button" ? box.X : text, font, new Rectangle(rect.X + pad, rect.Y + 3, Math.Max(1, rect.Width - (pad * 2) - reserve), Math.Max(1, rect.Height - 6)), textColor, flags, opacity);
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
            else if (field.K == "select") ShowChoices(field, box);
            else if (field.K == "reset") Rs(field);
            else if (field.K == "submit") await SmA(field);
            else if (field.K == "button" && Sb != null) Sb(this, EventArgs.Empty);
            Invalidate();
        }

        private static bool CanEdit(Fi field)
        {
            return field != null && field.K != "checkbox" && field.K != "radio" && field.K != "select" && field.K != "submit" && field.K != "reset" && field.K != "button" && field.K != "file";
        }

        private void ShowChoices(Fi field, Bx box)
        {
            CloseChoices();
            if (field.Os.Count == 0) return;
            var menu = new ContextMenuStrip();
            choices = menu;
            for (var i = 0; i < field.Os.Count; i++)
            {
                var index = i;
                var option = field.Os[i];
                var label = option.T.Length == 0 ? option.V.Length == 0 ? "(empty)" : option.V : option.T;
                var item = new ToolStripMenuItem(label);
                item.Enabled = !option.Ds;
                item.Checked = index == OptionIndex(field);
                item.Click += delegate { Choose(field, index); };
                menu.Items.Add(item);
            }
            menu.Closed += delegate
            {
                if (Object.ReferenceEquals(choices, menu)) choices = null;
                menu.Dispose();
            };
            var rect = box.C;
            rect.Offset(0, -sy);
            menu.Show(this, new Point(rect.Left, rect.Bottom));
        }

        private void Choose(Fi field, int index)
        {
            if (field == null || index < 0 || index >= field.Os.Count || field.Os[index].Ds) return;
            field.Si = index;
            field.V = field.Os[index].V;
            OnEdited();
            Invalidate();
        }

        private void CloseChoices()
        {
            var menu = choices;
            choices = null;
            if (menu == null) return;
            try { if (!menu.IsDisposed) menu.Close(); }
            catch (ObjectDisposedException) { }
            try { if (!menu.IsDisposed) menu.Dispose(); }
            catch (ObjectDisposedException) { }
        }

        private static int OptionIndex(Fi field)
        {
            if (field.Si >= 0 && field.Si < field.Os.Count && field.Os[field.Si].V == field.V) return field.Si;
            for (var i = 0; i < field.Os.Count; i++) if (field.Os[i].V == field.V) return i;
            return -1;
        }

        private static string OptionText(Fi field)
        {
            var index = OptionIndex(field);
            return index < 0 ? field.V ?? "" : field.Os[index].T;
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
                if (Hidden(box.S))
                {
                    ed.Visible = false;
                    return;
                }
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
                field.Si = field.Di;
            }
            OnEdited();
        }

        public P Sm(Fi button)
        {
            if (Sb != null) Sb(this, EventArgs.Empty);
            var data = FormData(button);
            if (data == null) return At;
            if (button.M == "get") return Go(FormUri(button, data));
            return button.M == "post" && CanPost(button.U) ? Po(button.U, data) : At;
        }

        public async Task<P> SmA(Fi button)
        {
            if (Sb != null) Sb(this, EventArgs.Empty);
            var data = FormData(button);
            if (data == null) return At;
            if (button.M == "get") return await GoA(FormUri(button, data));
            return button.M == "post" && CanPost(button.U) ? await PoA(button.U, data) : At;
        }

        private string FormData(Fi button)
        {
            if (At == null || button == null || button.F == null || button.U == null || (button.M != "get" && button.M != "post")) return null;

            var values = new List<string>();
            foreach (var field in At.Fs)
            {
                if (!Object.ReferenceEquals(field.F, button.F) || field.Ds || field.N.Length == 0) continue;
                if ((field.K == "checkbox" || field.K == "radio") && !field.C) continue;
                if (field.K == "select")
                {
                    var index = OptionIndex(field);
                    if (index >= 0 && field.Os[index].Ds) continue;
                }
                if (field.K == "reset" || field.K == "button" || (field.K == "submit" && field != button)) continue;
                values.Add(FormEncode(field.N) + "=" + FormEncode(field.V ?? ""));
            }
            return String.Join("&", values.ToArray());
        }

        private static Uri FormUri(Fi button, string data)
        {
            var target = new UriBuilder(button.U);
            var query = target.Query.TrimStart('?');
            if (query.Length > 0 && data.Length > 0) query += "&";
            target.Query = query + data;
            return target.Uri;
        }

        private static string FormEncode(string value)
        {
            return Uri.EscapeDataString(value ?? "").Replace("%20", "+");
        }

        private static bool CanPost(Uri url)
        {
            return url != null && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps);
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

        private static bool Hidden(S style)
        {
            return style != null && (style.Vi == "h" || style.Vi == "c");
        }

        private static Color PaintColor(Color color, int opacity)
        {
            var alpha = Math.Max(0, Math.Min(255, color.A * Math.Max(0, Math.Min(100, opacity)) / 100));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static void PaintText(Graphics graphics, string text, Font font, Rectangle rect, Color color, TextFormatFlags flags, int opacity)
        {
            if (opacity >= 100)
            {
                TextRenderer.DrawText(graphics, text, font, rect, color, flags);
                return;
            }
            if (opacity <= 0 || rect.Width < 1 || rect.Height < 1) return;
            using (var format = (StringFormat)StringFormat.GenericTypographic.Clone())
            using (var brush = new SolidBrush(PaintColor(color, opacity)))
            {
                if ((flags & TextFormatFlags.SingleLine) != 0) format.FormatFlags |= StringFormatFlags.NoWrap;
                if ((flags & TextFormatFlags.HorizontalCenter) != 0) format.Alignment = StringAlignment.Center;
                else if ((flags & TextFormatFlags.Right) != 0) format.Alignment = StringAlignment.Far;
                if ((flags & TextFormatFlags.VerticalCenter) != 0) format.LineAlignment = StringAlignment.Center;
                graphics.DrawString(text ?? "", font, brush, rect, format);
            }
        }

        private static void PaintImage(Graphics graphics, Image image, Rectangle rect, int opacity)
        {
            if (opacity >= 100) { graphics.DrawImage(image, rect); return; }
            if (opacity <= 0) return;
            using (var attributes = new ImageAttributes())
            {
                var matrix = new ColorMatrix();
                matrix.Matrix33 = opacity / 100F;
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
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
                CloseChoices();
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
            public readonly bool Inline;
            public readonly Rectangle Clip;

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle))
                : this(kind, text, url, style, rect, rect, title, block, inline, clip)
            {
            }

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, Rectangle content, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle))
            {
                K = kind;
                X = text ?? "";
                U = url;
                S = style;
                R = rect;
                C = content;
                Title = title;
                B = block;
                Inline = inline;
                Clip = clip;
            }
        }

        private sealed class Ia
        {
            public readonly string K;
            public readonly string X;
            public readonly Uri U;
            public readonly S S;
            public readonly string Z;
            public readonly int W;
            public readonly int H;
            public readonly int L;

            public Ia(string kind, string text, Uri url, S style, string baseKind, int width, int height, int lineHeight)
            {
                K = kind;
                X = text;
                U = url;
                S = style;
                Z = baseKind;
                W = width;
                H = height;
                L = lineHeight;
            }
        }

        private sealed class Il
        {
            public readonly List<Ia> As = new List<Ia>();
            public int W;
            public int H;
            public bool Br;
        }

        private sealed class Ix
        {
            public readonly string K;
            public readonly string X;
            public readonly Uri U;
            public readonly S S;
            public readonly Rectangle R;

            public Ix(string kind, string text, Uri url, S style, Rectangle rect)
            {
                K = kind;
                X = text;
                U = url;
                S = style;
                R = rect;
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
