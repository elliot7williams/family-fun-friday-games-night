using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly Dictionary<P, int> alerts = new Dictionary<P, int>();
        private readonly Queue<P> alertOrder = new Queue<P>();
        private readonly List<Bx> findMatches = new List<Bx>();
        private readonly Queue<string> picOrder = new Queue<string>();
        private readonly TextBox ed = new TextBox();
        private const int MaxKeyTargets = 256;
        private const int MaxKeyMapRegions = 4096;
        private const int MaxAccessTargets = 64;
        private const int PicLimit = 24;
        private const int PicBytes = 4 * 1024 * 1024;
        private const int FontLimit = 32;
        private const int UploadFiles = 8;
        private const int UploadFileBytes = 4 * 1024 * 1024;
        private const int UploadBytes = 8 * 1024 * 1024;
        private int sy;
        private int docHeight;
        private int layoutBottom;
        private int layoutWidth = -1;
        private bool layoutDirty = true;
        private bool picBusy;
        private bool picStop;
        private Fi edit;
        private Font editFont;
        private string editStart = "";
        private bool editLock;
        private ContextMenuStrip choices;
        private N hover;
        private N focus;
        private int keySlot = -1;
        private string findQuery = "";
        private int findIndex = -1;
        private int goRequests;

        public event EventHandler Done;
        public event EventHandler Changed;
        public event EventHandler Ed;
        public event EventHandler Sb;
        public event EventHandler Al;
        public event EventHandler Dr;
        public event EventHandler Gr;

        public V()
        {
            Am = "";
            Dn = "";
            DoubleBuffered = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(31, 41, 55);
            Font = new Font("Segoe UI", 10F);
            Dock = DockStyle.Fill;
            TabStop = true;

            ed.Visible = false;
            ed.AutoSize = false;
            ed.BorderStyle = BorderStyle.FixedSingle;
            ed.TextChanged += EditChanged;
            ed.KeyDown += EditKeyDown;
            ed.LostFocus += delegate { EndEdit(false); };
            ed.MouseWheel += delegate(object sender, MouseEventArgs args) { Sc(-Math.Sign(args.Delta) * 72); };
            Controls.Add(ed);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new Va(this);
        }

        public P At { get { return e.At; } }
        public Uri U { get { return At == null ? null : At.U; } }
        public Uri Bu { get { return At == null ? null : At.Bu; } }
        public string T { get { return At == null ? "" : At.T; } }
        public string H { get { return At == null ? "" : At.H; } }
        public string Q { get { return At == null ? "" : At.Q; } }
        public bool CanBk { get { return e.CanBk; } }
        public bool CanFw { get { return e.CanFw; } }
        public bool Busy { get { return e.Busy || picBusy; } }
        public int Kc { get { return e.Kc; } }
        public int Km { get { return e.Km; } }
        public long Dm { get { return e.Dm; } }
        public int Tm { get { return e.Tm; } set { e.Tm = value; } }
        public bool Pv { get { return e.Pv; } set { e.Pv = value; } }
        public bool Je { get { return e.Je; } set { e.Je = value; } }
        public string Am { get; private set; }
        public string Fq { get { return findQuery; } }
        public int Fn { get { return findMatches.Count; } }
        public int Fx { get { return findIndex < 0 ? 0 : findIndex + 1; } }
        public Uri Du { get; private set; }
        public string Dn { get; private set; }
        public Uri Gu { get; private set; }
        public D Ld { get; private set; }
        public int Dh { get { return docHeight; } }
        public int Sy { get { return sy; } }
        public int Bc { get { return boxes.Count; } }
        public N Ke { get { return focus; } }
        public int Kn { get { return FocusTargets(true).Count; } }
        public int Ki { get { return FocusIndex(FocusTargets(true)) + 1; } }
        public Rectangle Kb { get { return FocusBounds(CurrentFocusTarget()); } }
        public int An { get { return AccessTargets().Count; } }
        public IList<Ax> Axs { get { return At == null ? new List<Ax>().AsReadOnly() : At.Axs; } }
        public AccessibleObject Ao { get { return AccessibilityObject ?? new Va(this); } }
        public Rectangle Rb(int index) { return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].R; }
        public Rectangle Rc(int index) { return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].C; }
        public Rectangle Fr { get { return FindRect(); } }

        public P Go(string input)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (Fg(input) != 0) return At;
                return Show(e.Go(input), true);
            }
            finally { Cursor = Cursors.Default; }
        }

        public P Go(Uri url)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (Fg(url) != 0) return At;
                return Show(e.Go(url), true);
            }
            finally { Cursor = Cursors.Default; }
        }

        public P Po(Uri url, string data)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Po(url, data), true); }
            finally { Cursor = Cursors.Default; }
        }

        public P Pb(Uri url, byte[] data, string type)
        {
            Cursor = Cursors.WaitCursor;
            try { return Show(e.Pb(url, data, type), true); }
            finally { Cursor = Cursors.Default; }
        }

        public async Task<P> GoA(string input)
        {
            Cursor = Cursors.WaitCursor;
            picStop = false;
            try
            {
                if (Fg(input) != 0) return At;
                var page = Show(await e.GoA(input), false);
                await PicsA(page);
                Dirty();
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                GoRequest(page);
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
                if (Fg(url) != 0) return At;
                var page = Show(await e.GoA(url), false);
                await PicsA(page);
                Dirty();
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                GoRequest(page);
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
                GoRequest(page);
                return page;
            }
            finally { Cursor = Cursors.Default; }
        }

        public async Task<P> PbA(Uri url, byte[] data, string type)
        {
            Cursor = Cursors.WaitCursor;
            picStop = false;
            try
            {
                var page = Show(await e.PbA(url, data, type), false);
                await PicsA(page);
                Dirty();
                Invalidate();
                if (Done != null) Done(this, EventArgs.Empty);
                GoRequest(page);
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
            GoRequest(page);
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

        public int Js(string code)
        {
            if (At == null) return 0;
            return Commands(e.Js(At, code));
        }

        public int Ck(N element)
        {
            if (At == null) return 0;
            return Commands(e.Ck(At, element));
        }

        public int Ds(N element)
        {
            if (At == null) return 0;
            return Commands(e.Ds(At, element));
        }

        public int Rv(N element)
        {
            if (At == null) return 0;
            return Commands(e.Rv(At, element));
        }

        public G Dg()
        {
            return At == null ? null : e.Dg(At);
        }

        public void Dcl() { e.Dcl(); }

        public N Id(string id) { return At == null ? null : e.Id(At, id); }
        public int? Ti(N element) { return e.Ti(element); }
        public N Af() { return At == null ? null : e.Af(At); }
        public IList<N> Afs() { return At == null ? new List<N>().AsReadOnly() : e.Afs(At); }
        public string Ak(N element)
        {
            var target = AccessTarget(element);
            return target == null ? "" : target.K;
        }
        public string Kl(N element)
        {
            var key = Ak(element);
            return key.Length == 0 ? "" : "Alt+Shift+" + key;
        }
        public N Ft(string fragment) { return At == null ? null : e.Ft(At, fragment); }
        public N Q1(string query) { return At == null ? null : e.Q(At, query); }
        public IList<N> Qa(string query) { return At == null ? new List<N>().AsReadOnly() : e.Qa(At, query); }
        public Ax Ax(N element) { return At == null ? null : e.Ax(At, element); }

        // Ab returns one semantic record's visible client bounds.
        public Rectangle Ab(Ax item)
        {
            item = CurrentAccessibility(item);
            if (item == null) return Rectangle.Empty;
            if (At.Axs.Count > 0 && Object.ReferenceEquals(item, At.Axs[0])) return ClientRectangle;
            EnsureLayout();
            var result = Rectangle.Empty;
            foreach (var box in boxes)
            {
                if (box.Title || box.El == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                if (item.El != null && (OwnsElement(item.El, box.El) || item.El.K == "#text" && Object.ReferenceEquals(item.El.Up, box.El)))
                    AddAccessibleBounds(ref result, box.R, box.Clip, box.Fx);
                if (box.B == null || box.B.Mp == null || item.El == null || item.El.K != "area") continue;
                var source = MapSourceSize(box.B, box.C);
                foreach (var area in box.B.Mp.As)
                {
                    if (!Object.ReferenceEquals(area.El, item.El)) continue;
                    var rect = MapBounds(area, box.C, source);
                    AddAccessibleBounds(ref result, rect, box.Clip, box.Fx);
                }
            }
            return result;
        }

        public int Fg(string input)
        {
            var old = sy;
            if (e.Fg(input) == 0) return 0;
            Show(At, true, old);
            return 1;
        }

        public int Fg(Uri url)
        {
            var old = sy;
            if (e.Fg(url) == 0) return 0;
            Show(At, true, old);
            return 1;
        }

        private int Commands(int count)
        {
            if (count == 0) return 0;
            ClearFind(false);
            Dirty();
            if (!ApplyCommandFocus(At)) KeepValidFocus();
            OnChanged();
            Invalidate();
            NotifyAccessibility(AccessibleEvents.Reorder);
            Alerts(At);
            GoRequest(At);
            return count;
        }

        public D Dl(string input, string path)
        {
            Cursor = Cursors.WaitCursor;
            try { return Ld = e.Dl(input, path); }
            finally { Cursor = Cursors.Default; }
        }

        public D Dl(Uri url, string path)
        {
            Cursor = Cursors.WaitCursor;
            try { return Ld = e.Dl(url, path); }
            finally { Cursor = Cursors.Default; }
        }

        public async Task<D> DlA(string input, string path)
        {
            Cursor = Cursors.WaitCursor;
            try { return Ld = await e.DlA(input, path); }
            finally { Cursor = Cursors.Default; }
        }

        public async Task<D> DlA(Uri url, string path)
        {
            Cursor = Cursors.WaitCursor;
            try { return Ld = await e.DlA(url, path); }
            finally { Cursor = Cursors.Default; }
        }

        public int Fd(string query)
        {
            findQuery = (query ?? "").Trim();
            if (findQuery.Length > 256) findQuery = findQuery.Substring(0, 256);
            EnsureLayout();
            RebuildFind(0);
            ShowFind();
            return Fn;
        }

        public int Nx()
        {
            EnsureLayout();
            if (findMatches.Count == 0) return 0;
            SetFind((findIndex + 1) % findMatches.Count);
            ShowFind();
            return Fx;
        }

        public int Pr()
        {
            EnsureLayout();
            if (findMatches.Count == 0) return 0;
            SetFind((findIndex + findMatches.Count - 1) % findMatches.Count);
            ShowFind();
            return Fx;
        }

        public void Fc()
        {
            ClearFind(true);
        }

        private P Show(P page, bool done, int preserveScroll = -1)
        {
            CloseChoices();
            ClearFind(false);
            hover = null;
            focus = null;
            keySlot = -1;
            EndEdit(false);
            e.Dy(page, null, null);
            sy = 0;
            Dirty();
            var scrolled = ScrollFragment(page);
            var commandFocus = ApplyCommandFocus(page);
            if (!scrolled && !commandFocus) ApplyAutofocus(page);
            if (!scrolled && preserveScroll >= 0 && page != null && page.U != null && page.U.Fragment.Length > 0)
                sy = Math.Max(0, Math.Min(MaxScroll(), preserveScroll));
            OnChanged();
            Invalidate();
            NotifyAccessibility(AccessibleEvents.Reorder);
            Alerts(page);
            if (done && Done != null) Done(this, EventArgs.Empty);
            if (done) GoRequest(page);
            return page;
        }

        private bool ScrollFragment(P page)
        {
            if (page == null || page.U == null || page.U.Fragment.Length == 0) return false;
            if (page.U.Fragment == "#")
            {
                sy = 0;
                PlaceEditor();
                return true;
            }
            var target = e.Ft(page, page.U.Fragment);
            if (target == null) return false;
            if (e.Rv(page, target) > 0) Dirty();
            EnsureLayout();
            var box = FragmentBox(target);
            if (box == null) return false;
            if (!box.Fx) sy = Math.Max(0, Math.Min(MaxScroll(), box.C.Top - 12));
            PlaceEditor();
            return true;
        }

        private Bx FragmentBox(N target)
        {
            Bx exactInline = null;
            Bx descendant = null;
            foreach (var box in boxes)
            {
                if (box.Title || box.El == null) continue;
                if (Object.ReferenceEquals(box.El, target))
                {
                    if (!box.Inline) return box;
                    if (exactInline == null) exactInline = box;
                }
                else if (descendant == null && Inside(box.El, target)) descendant = box;
            }
            return exactInline ?? descendant;
        }

        private static bool Inside(N node, N ancestor)
        {
            for (var current = node; current != null; current = current.Up)
                if (Object.ReferenceEquals(current, ancestor)) return true;
            return false;
        }

        private bool GoRequest(P page)
        {
            if (page == null) return false;
            var target = e.Gt(page);
            if (target == null) return false;
            Gu = target;
            goRequests++;
            if (Gr != null) Gr(this, EventArgs.Empty);
            return true;
        }

        private void Alerts(P page)
        {
            if (page == null || page.Ms.Count == 0) return;
            int shown;
            if (!alerts.TryGetValue(page, out shown))
            {
                shown = 0;
                alerts[page] = 0;
                alertOrder.Enqueue(page);
                while (alertOrder.Count > 32) alerts.Remove(alertOrder.Dequeue());
            }
            while (alerts.TryGetValue(page, out shown) && shown < page.Ms.Count)
            {
                Am = page.Ms[shown];
                alerts[page] = shown + 1;
                if (Al != null) Al(this, EventArgs.Empty);
            }
        }

        private void EnsureLayout()
        {
            if (At == null || (!layoutDirty && layoutWidth == ClientSize.Width)) return;
            if (!IsHandleCreated) CreateControl();
            using (var graphics = CreateGraphics()) Lay(graphics);
        }

        private void RebuildFind(int wanted)
        {
            foreach (var match in findMatches) { match.Fm = false; match.Fa = false; }
            findMatches.Clear();
            if (findQuery.Length > 0)
                foreach (var box in boxes)
                {
                    if (box.Title || box.X.Length == 0 || Hidden(box.S)) continue;
                    if (box.X.IndexOf(findQuery, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    box.Fm = true;
                    findMatches.Add(box);
                    if (findMatches.Count == 256) break;
                }
            if (findMatches.Count == 0) { findIndex = -1; return; }
            SetFind(Math.Max(0, Math.Min(findMatches.Count - 1, wanted)));
        }

        private void SetFind(int index)
        {
            if (findIndex >= 0 && findIndex < findMatches.Count) findMatches[findIndex].Fa = false;
            findIndex = index;
            if (findIndex >= 0 && findIndex < findMatches.Count) findMatches[findIndex].Fa = true;
        }

        private void ShowFind()
        {
            if (findIndex < 0 || findIndex >= findMatches.Count) { Invalidate(); return; }
            var match = findMatches[findIndex];
            if (!match.Fx && (match.C.Top < sy + 12 || match.C.Bottom > sy + ClientSize.Height - 12))
                sy = Math.Max(0, Math.Min(MaxScroll(), match.C.Top - 24));
            PlaceEditor();
            Invalidate();
        }

        private void ClearFind(bool paint)
        {
            foreach (var match in findMatches) { match.Fm = false; match.Fa = false; }
            findMatches.Clear();
            findQuery = "";
            findIndex = -1;
            if (paint) Invalidate();
        }

        private Rectangle FindRect()
        {
            if (findIndex < 0 || findIndex >= findMatches.Count) return Rectangle.Empty;
            var box = findMatches[findIndex];
            var rect = box.C;
            var clip = box.Clip;
            if (!box.Fx)
            {
                rect.Offset(0, -sy);
                if (!clip.IsEmpty) clip.Offset(0, -sy);
            }
            return Rectangle.Intersect(VisibleRect(rect, clip), ClientRectangle);
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
                var content = box.C;
                if (!box.Fx)
                {
                    rect.Offset(0, -sy);
                    content.Offset(0, -sy);
                }
                if (rect.Bottom < 0 || rect.Top > ClientSize.Height) continue;
                if (Hidden(box.S)) continue;
                var opacity = box.S == null ? 100 : box.S.Op;
                var clip = box.Clip;
                if (box.Fx) clip = clip.IsEmpty ? ClientRectangle : Rectangle.Intersect(clip, ClientRectangle);
                else if (!clip.IsEmpty) clip.Offset(0, -sy);
                GraphicsState state = null;
                try
                {
                    if (!clip.IsEmpty)
                    {
                        state = pe.Graphics.Save();
                        pe.Graphics.SetClip(clip, CombineMode.Intersect);
                    }
                    if (opacity > 0 && !box.Title && (!box.Inline || box.B == null || box.B.Is.Count > 0)) PaintBox(pe.Graphics, rect, box.S, pageBackground, pageColor, opacity, box.B == null || box.B.I == null);
                    if (opacity > 0 && box.Fm) PaintFind(pe.Graphics, VisibleRect(content, clip), box.Fa);
                    if (box.K == "img")
                    {
                        if (opacity > 0) PaintPic(pe.Graphics, box, content, pageColor, opacity);
                        AddMapHits(box, content, clip);
                        continue;
                    }
                    if (box.K == "hr")
                    {
                        var color = CssColor(box.S == null ? "" : box.S.F, Color.FromArgb(148, 163, 184));
                        if (opacity > 0)
                            using (var pen = new Pen(PaintColor(color, opacity))) pe.Graphics.DrawLine(pen, content.Left, content.Top + content.Height / 2, content.Right, content.Top + content.Height / 2);
                        continue;
                    }
                    if (box.B != null && box.B.Vm != null)
                    {
                        if (opacity > 0) PaintGauge(pe.Graphics, box.K, box.B.Vm, box.S, content, opacity);
                        continue;
                    }
                    if (box.B != null && box.B.I != null)
                    {
                        if (opacity > 0) PaintField(pe.Graphics, box, content, pageColor, opacity);
                        var fieldHit = VisibleRect(rect, clip);
                        if (!fieldHit.IsEmpty) hits.Add(new Hit(fieldHit, null, box));
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
                        if (!hit.IsEmpty) hits.Add(new Hit(hit, box.U, null, box.El));
                    }
                    else if (box.B != null && box.B.K == "summary" && !String.IsNullOrEmpty(box.B.Mk))
                    {
                        var hit = VisibleRect(rect, clip);
                        if (!hit.IsEmpty) hits.Add(new Hit(hit, null, box, box.El));
                    }
                }
                finally { if (state != null) pe.Graphics.Restore(state); }
            }
            PaintKeyFocus(pe.Graphics);
        }

        private static Rectangle VisibleRect(Rectangle rect, Rectangle clip)
        {
            return clip.IsEmpty ? rect : Rectangle.Intersect(rect, clip);
        }

        private void Lay(Graphics graphics)
        {
            Lay(graphics, null);
        }

        private void Lay(Graphics graphics, IDictionary<B, int> resolved)
        {
            boxes.Clear();
            if (At == null) return;
            var measured = resolved ?? new Dictionary<B, int>();
            var contexts = new Dictionary<B, Lc>();
            var owners = new HashSet<B>();
            foreach (var block in At.Bs) if (!block.Hd && block.Up != null) owners.Add(block.Up);
            var active = new List<Lc>();
            var style = At.S;
            var left = style == null ? 0 : style.Ml + style.Pl + BorderWidth(style, style.Bl);
            var right = style == null ? 0 : style.Mr + style.Pr + BorderWidth(style, style.Br);
            var x = 20 + left;
            var width = ConstrainWidth(style, Math.Max(120, ClientSize.Width - x - 20 - right));
            var y = 18 + (style == null ? 0 : style.Mt + style.Pt + BorderWidth(style, style.Bt));
            var anchorTop = y;
            var rootContaining = new Rectangle(x, anchorTop, width, Math.Max(1, ClientSize.Height - anchorTop));
            layoutBottom = y;

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
                    CloseContexts(block, active, ref y, measured);
                    if (block.Hd) continue;
                    var bs = block.S;
                    Lc parent = null;
                    if (block.Up != null) contexts.TryGetValue(block.Up, out parent);
                    var lx = parent == null ? x : parent.C.X;
                    var lw = parent == null ? width : parent.C.Width;
                    var ly = parent == null ? y : parent.Y;
                    var parentClip = parent == null ? Rectangle.Empty : parent.Cp;
                    var fixedParent = parent != null && parent.Fx;
                    var containingContext = ContainingContext(block, contexts);
                    var containing = bs != null && bs.Pn == "f" ? ClientRectangle : containingContext == null ? rootContaining : containingContext.C;
                    if (block.K == "table" && block.Tb != null)
                    {
                        int next;
                        contexts[block] = LayTable(graphics, block, lx, lw, ly, containing, parentClip, fixedParent, out next);
                        if (parent == null) y = next; else parent.Y = next;
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
                    var mt = bs == null ? 0 : bs.Mt;
                    var mb = bs == null ? 0 : bs.Mb;
                    var flowY = ly + mt;
                    var outerAvailable = Math.Max(40, lw - ml - mr);
                    if (StretchesWidth(bs)) outerAvailable = Math.Max(1, containing.Width - bs.Lt.Value - bs.Rt.Value - ml - mr);
                    var bw = ConstrainWidth(bs, Math.Max(1, outerAvailable - bl - br - pl - pr));
                    int height;
                    List<Ix> inline = null;
                    if (block.K == "img")
                    {
                        var size = PicSize(block, bw);
                        if (bs == null || bs.W == 0) bw = Math.Min(bw, size.Width);
                        height = size.Height;
                    }
                    else if (block.K == "hr") height = 2;
                    else if (block.Vm != null)
                    {
                        using (var font = BoxFont(block.K, bs, false))
                        {
                            bw = GaugeWidth(bs, bw, font);
                            height = GaugeHeight(font);
                        }
                    }
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
                        using (var font = BoxFont(block.K, bs, false)) height = Measure(graphics, MarkedText(block), font, bw);
                    }
                    var naturalHeight = height;
                    height = ConstrainHeight(bs, height);
                    int resolvedHeight;
                    if (resolved != null && owners.Contains(block) && (bs == null || bs.H == 0) && !StretchesHeight(bs) && resolved.TryGetValue(block, out resolvedHeight)) height = resolvedHeight;
                    if (StretchesHeight(bs)) height = ConstrainHeight(bs, Math.Max(1, containing.Height - bs.Tp.Value - bs.Bm.Value - mt - mb - bt - bb - pt - pb));
                    var text = MarkedText(block);
                    var link = inline == null && block.K == "a" ? block.U : null;
                    var outerWidth = bw + pl + pr + bl + br;
                    var outerHeight = height + pt + pb + bt + bb;
                    var bx = PositionX(bs, lx + ml, outerWidth, containing, ml, mr);
                    var by = PositionY(bs, flowY, outerHeight, containing, mt, mb);
                    var outer = new Rectangle(bx, by, outerWidth, outerHeight);
                    var content = new Rectangle(bx + bl + pl, by + bt + pt, bw, height);
                    var fixedSelf = bs != null && bs.Pn == "f";
                    var inheritedClip = fixedSelf ? Rectangle.Empty : parentClip;
                    var ownerBox = new Bx(block.K, inline == null ? text : "", link, bs, outer, content, false, block, false, inheritedClip);
                    boxes.Add(ownerBox);
                    if (inline != null) AddInlineBoxes(block, content, inline, inheritedClip);
                    var ownClip = ClipsOverflow(bs) ? content : Rectangle.Empty;
                    var childClip = CombineClip(inheritedClip, ownClip);
                    var hasContent = block.X.Length > 0 || block.Is.Count > 0;
                    var childY = content.Y + (hasContent ? Math.Min(naturalHeight, height) + 4 : 0);
                    var fixedChain = fixedSelf || fixedParent;
                    var context = new Lc(outer, content, childY, childClip, fixedChain)
                    {
                        B = block,
                        Box = ownerBox,
                        Up = parent,
                        Au = owners.Contains(block) && (bs == null || bs.H == 0),
                        Fl = !OutOfFlow(bs),
                        Fy = flowY,
                        Gp = (HeadingKind(block.K) ? 12 : 8) + mb,
                        Pc = inheritedClip,
                        Co = ClipsOverflow(bs)
                    };
                    contexts[block] = context;
                    if (context.Au) active.Add(context);
                    if (!fixedChain) layoutBottom = Math.Max(layoutBottom, outer.Bottom + mb);
                    if (!OutOfFlow(bs))
                    {
                        var next = flowY + outer.Height + (HeadingKind(block.K) ? 12 : 8) + mb;
                        if (parent == null) y = next; else parent.Y = next;
                    }
                }
                CloseContexts(null, active, ref y, measured);
            }

            if (resolved == null && measured.Count > 0)
            {
                Lay(graphics, measured);
                return;
            }

            docHeight = y + 20 + (style == null ? 0 : style.Pb + BorderWidth(style, style.Bb) + style.Mb);
            docHeight = Math.Max(docHeight, layoutBottom + 20);
            if (style != null && style.H > 0) docHeight = Math.Max(docHeight, 18 + style.Mt + style.H + style.Pt + style.Pb + BorderWidth(style, style.Bt) + BorderWidth(style, style.Bb) + style.Mb);
            StackBoxes();
            layoutWidth = ClientSize.Width;
            layoutDirty = false;
            sy = Math.Min(sy, MaxScroll());
            if (findQuery.Length > 0) RebuildFind(findIndex);
        }

        private void CloseContexts(B next, IList<Lc> active, ref int rootY, IDictionary<B, int> measured)
        {
            while (active.Count > 0 && !Inside(next, active[active.Count - 1].B))
            {
                var index = active.Count - 1;
                var context = active[index];
                active.RemoveAt(index);
                FinishContext(context, ref rootY, measured);
            }
        }

        private static bool Inside(B block, B parent)
        {
            for (var current = block == null ? null : block.Up; current != null; current = current.Up)
                if (Object.ReferenceEquals(current, parent)) return true;
            return false;
        }

        private void FinishContext(Lc context, ref int rootY, IDictionary<B, int> measured)
        {
            var needed = Math.Max(context.C.Height, Math.Max(1, context.Y - context.C.Top));
            var height = ConstrainHeight(context.B == null ? null : context.B.S, needed);
            if (height != context.C.Height)
            {
                context.C = new Rectangle(context.C.X, context.C.Y, context.C.Width, height);
                context.R = new Rectangle(context.R.X, context.R.Y, context.R.Width, height + context.R.Height - context.Box.C.Height);
                context.Box.C = context.C;
                context.Box.R = context.R;
                if (context.Co) context.Cp = CombineClip(context.Pc, context.C);
            }
            if (context.B != null) measured[context.B] = height;
            if (!context.Fx) layoutBottom = Math.Max(layoutBottom, context.R.Bottom);
            if (!context.Fl) return;
            var next = context.Fy + context.R.Height + context.Gp;
            if (context.Up == null) rootY = Math.Max(rootY, next);
            else context.Up.Y = Math.Max(context.Up.Y, next);
        }

        private static Lc ContainingContext(B block, IDictionary<B, Lc> contexts)
        {
            for (var parent = block == null ? null : block.Up; parent != null; parent = parent.Up)
            {
                Lc context;
                if (Positioned(parent.S) && contexts.TryGetValue(parent, out context)) return context;
            }
            return null;
        }

        private Lc LayTable(Graphics graphics, B block, int x, int width, int y, Rectangle containing, Rectangle parentClip, bool fixedParent, out int nextY)
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
            var mt = style == null ? 0 : style.Mt;
            var mb = style == null ? 0 : style.Mb;
            var flowY = y + mt;
            var available = Math.Max(40, width - ml - mr);
            if (StretchesWidth(style)) available = Math.Max(1, containing.Width - style.Lt.Value - style.Rt.Value - ml - mr);
            var tableWidth = ConstrainWidth(style, Math.Max(1, available - bl - br - pl - pr));
            var captionHeight = 0;
            if (block.Tb.Cp.Length > 0)
            {
                using (var font = BoxFont("caption", style, false)) captionHeight = Measure(graphics, block.Tb.Cp, font, tableWidth);
            }

            var columns = Math.Max(1, block.Tb.Cs);
            var rowHeights = new List<int>();
            foreach (var row in block.Tb.Rs)
            {
                var rowHeight = 1;
                var column = 0;
                foreach (var cell in row.Cs)
                {
                    var span = Math.Max(1, Math.Min(cell.Sp, columns - Math.Min(column, columns - 1)));
                    var rightColumn = Math.Min(columns, column + span);
                    var cellLeft = tableWidth * column / columns;
                    var cellRight = tableWidth * rightColumn / columns;
                    var cellWidth = Math.Max(1, cellRight - cellLeft);
                    var cellBox = CellBox(cell.S, cellWidth);
                    var height = ConstrainHeight(cell.S, CellContentHeight(graphics, cell, cellBox.Width));
                    rowHeight = Math.Max(rowHeight, height + cellBox.Height);
                    column = rightColumn;
                }
                rowHeights.Add(rowHeight);
            }

            var naturalHeight = captionHeight > 0 ? captionHeight + 8 : 0;
            foreach (var rowHeight in rowHeights) naturalHeight += rowHeight;
            var contentHeight = ConstrainHeight(style, naturalHeight);
            if (StretchesHeight(style)) contentHeight = ConstrainHeight(style, Math.Max(1, containing.Height - style.Tp.Value - style.Bm.Value - mt - mb - bt - bb - pt - pb));
            var outerWidth = tableWidth + pl + pr + bl + br;
            var outerHeight = contentHeight + pt + pb + bt + bb;
            var bx = PositionX(style, x + ml, outerWidth, containing, ml, mr);
            var by = PositionY(style, flowY, outerHeight, containing, mt, mb);
            var contentX = bx + bl + pl;
            var contentY = by + bt + pt;
            var rowY = contentY;
            var first = boxes.Count;

            if (captionHeight > 0)
            {
                boxes.Add(new Bx("caption", block.Tb.Cp, null, style, new Rectangle(contentX, rowY, tableWidth, captionHeight), false, block));
                rowY += captionHeight + 8;
            }

            var rowIndex = 0;
            foreach (var row in block.Tb.Rs)
            {
                var rowHeight = rowHeights[rowIndex++];
                var column = 0;
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
                    if (simple != null && !NeedsFlow(simple)) boxes.Add(new Bx(cell.H ? "th" : "td", CellText(simple), null, cell.S, outer, content, false, block, false, Rectangle.Empty, cell.El));
                    else
                    {
                        boxes.Add(new Bx(cell.H ? "th" : "td", "", null, cell.S, outer, content, false, block, false, Rectangle.Empty, cell.El));
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

            var fixedSelf = style != null && style.Pn == "f";
            var inheritedClip = fixedSelf ? Rectangle.Empty : parentClip;
            var tableClip = ClipsOverflow(style) ? new Rectangle(contentX, contentY, tableWidth, contentHeight) : Rectangle.Empty;
            var childClip = CombineClip(inheritedClip, tableClip);
            if (!childClip.IsEmpty)
                for (var index = first; index < boxes.Count; index++) boxes[index].Clip = CombineClip(boxes[index].Clip, childClip);
            var outerTable = new Rectangle(bx, by, outerWidth, outerHeight);
            var contentTable = new Rectangle(contentX, contentY, tableWidth, contentHeight);
            boxes.Insert(first, new Bx("table", "", null, style, outerTable, contentTable, false, block, false, inheritedClip));
            var fixedChain = fixedSelf || fixedParent;
            if (!fixedChain) layoutBottom = Math.Max(layoutBottom, outerTable.Bottom + mb);
            nextY = OutOfFlow(style) ? y : flowY + outerTable.Height + 8 + mb;
            return new Lc(outerTable, contentTable, contentY, childClip, fixedChain);
        }

        private static int PositionX(S style, int flowX, int outerWidth, Rectangle containing, int ml, int mr)
        {
            if (style == null) return flowX;
            var x = flowX;
            if (style.Pn.Length == 0 || style.Pn == "s") return x + style.Tx;
            if (style.Pn == "r")
            {
                if (style.Lt.HasValue) x += style.Lt.Value;
                else if (style.Rt.HasValue) x -= style.Rt.Value;
                return x + style.Tx;
            }
            if (style.Lt.HasValue) x = containing.Left + ml + style.Lt.Value;
            else if (style.Rt.HasValue) x = containing.Right - outerWidth - mr - style.Rt.Value;
            return x + style.Tx;
        }

        private static int PositionY(S style, int flowY, int outerHeight, Rectangle containing, int mt, int mb)
        {
            if (style == null) return flowY;
            var y = flowY;
            if (style.Pn.Length == 0 || style.Pn == "s") return y + style.Ty;
            if (style.Pn == "r")
            {
                if (style.Tp.HasValue) y += style.Tp.Value;
                else if (style.Bm.HasValue) y -= style.Bm.Value;
                return y + style.Ty;
            }
            if (style.Tp.HasValue) y = containing.Top + mt + style.Tp.Value;
            else if (style.Bm.HasValue) y = containing.Bottom - outerHeight - mb - style.Bm.Value;
            return y + style.Ty;
        }

        private static bool StretchesWidth(S style)
        {
            return OutOfFlow(style) && style.W == 0 && style.Lt.HasValue && style.Rt.HasValue;
        }

        private static bool StretchesHeight(S style)
        {
            return OutOfFlow(style) && style.H == 0 && style.Tp.HasValue && style.Bm.HasValue;
        }

        private static bool OutOfFlow(S style)
        {
            return style != null && (style.Pn == "a" || style.Pn == "f");
        }

        private static bool Positioned(S style)
        {
            return style != null && (style.Pn == "r" || style.Pn == "a" || style.Pn == "f");
        }

        private static Rectangle CombineClip(Rectangle current, Rectangle parent)
        {
            if (current.IsEmpty) return parent;
            if (parent.IsEmpty) return current;
            return Rectangle.Intersect(current, parent);
        }

        private void StackBoxes()
        {
            foreach (var box in boxes)
            {
                box.Zp = StackPath(box.B);
                box.Fx = FixedBlock(box.B);
            }
            for (var index = 1; index < boxes.Count; index++)
            {
                var item = boxes[index];
                var at = index;
                while (at > 0 && CompareStack(boxes[at - 1].Zp, item.Zp) > 0)
                {
                    boxes[at] = boxes[at - 1];
                    at--;
                }
                boxes[at] = item;
            }
        }

        private static int[] StackPath(B block)
        {
            var chain = new List<B>();
            for (var current = block; current != null; current = current.Up) chain.Add(current);
            var path = new List<int>();
            for (var index = chain.Count - 1; index >= 0; index--)
            {
                var style = chain[index].S;
                if (StackingContext(style)) path.Add(style.Zi);
            }
            if (path.Count == 0) path.Add(0);
            return path.ToArray();
        }

        private static bool StackingContext(S style)
        {
            return style != null && (style.Zc || style.Pn == "f" || (Positioned(style) && style.Za));
        }

        private static bool FixedBlock(B block)
        {
            for (var current = block; current != null; current = current.Up)
                if (current.S != null && current.S.Pn == "f") return true;
            return false;
        }

        private static int CompareStack(int[] left, int[] right)
        {
            var count = Math.Min(left.Length, right.Length);
            for (var index = 0; index < count; index++)
                if (left[index] != right[index]) return left[index].CompareTo(right[index]);
            return left.Length.CompareTo(right.Length);
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
                using (var font = BoxFont(cell.H ? "th" : "td", cell.S, false)) return Measure(graphics, part == null ? cell.X : CellText(part), font, width);
            }

            var height = 0;
            foreach (var part in cell.Bs) if (!part.Hd) height += CellPartHeight(graphics, part, width);
            return Math.Max(1, height);
        }

        private void LayCellBlocks(Graphics graphics, Tc cell, Rectangle content)
        {
            var y = content.Y;
            foreach (var part in cell.Bs)
            {
                if (part.Hd) continue;
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
                if (part.Vm != null)
                {
                    using (var font = BoxFont(part.K, style, false))
                    {
                        available = GaugeWidth(style, available, font);
                        textHeight = GaugeHeight(font);
                    }
                }
                else if (part.K != "br" && NeedsFlow(part)) flow = InlineLayout(graphics, part, available, out textHeight);
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
            if (part.Vm != null)
                using (var font = BoxFont(part.K, style, false)) height = GaugeHeight(font);
            else if (part.K != "br" && NeedsFlow(part)) InlineLayout(graphics, part, available, out height);
            else using (var font = BoxFont(part.K, style, false)) height = part.K == "br" ? Math.Max(4, font.Height / 2) : Measure(graphics, CellText(part), font, available);
            height = ConstrainHeight(style, height);
            if (style != null) height += style.Mt + style.Mb + style.Pt + style.Pb + BorderWidth(style, style.Bt) + BorderWidth(style, style.Bb);
            return height + CellGap(part.K);
        }

        private static bool SimpleCell(Tc cell)
        {
            if (cell.Bs.Count == 0) return true;
            return SimpleCellBlock(cell) != null;
        }

        private static B SimpleCellBlock(Tc cell)
        {
            B found = null;
            foreach (var part in cell.Bs)
            {
                if (part.Hd) continue;
                if (found != null) return null;
                found = part;
            }
            return found != null && found.U == null && found.K == (cell.H ? "th" : "td") ? found : null;
        }

        private static string CellText(B part)
        {
            return MarkedText(part);
        }

        private static string MarkedText(B block)
        {
            if (block == null) return "";
            var text = block.X ?? "";
            if (block.K != "li" && block.K != "summary") return text;
            if (block.K == "summary" && String.IsNullOrEmpty(block.Mk)) return text;
            var marker = String.IsNullOrEmpty(block.Mk) ? "-" : block.Mk;
            return marker + " " + text;
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

        private static int GaugeWidth(S style, int available, Font font)
        {
            return style != null && style.W > 0 ? available : Math.Min(available, Math.Max(80, font.Height * 10));
        }

        private static int GaugeHeight(Font font)
        {
            return Math.Max(14, font.Height);
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

            if (block.K == "li" || (block.K == "summary" && !String.IsNullOrEmpty(block.Mk)))
            {
                var marker = String.IsNullOrEmpty(block.Mk) ? "-" : block.Mk;
                AddInlineWord(graphics, lines, ref line, ref pending, InlineAtom(graphics, block.K, marker, null, block.S, block.K, block.El), width, baseHeight, wrap, block.S == null ? "" : block.S.Wb);
                pending = InlineAtom(graphics, block.K, " ", null, block.S, block.K, block.El);
            }

            if (block.Is.Count == 0)
            {
                AddInlineText(graphics, lines, ref line, ref pending, block.K, block.X, block.U, block.S, block.K, block.El, width, baseHeight, space, wrap, ref endedWithBreak);
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
                    AddInlineText(graphics, lines, ref line, ref pending, run.K, run.X, run.U, run.S, block.K, run.El, width, baseHeight, space, wrap, ref endedWithBreak);
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
                    layout.Add(new Ix(atom.K, atom.X, atom.U, atom.S, rect, atom.El));
                    x += atomWidth;
                }
                y += current.H;
            }
            height = Math.Max(baseHeight, y);
            return layout;
        }

        private void AddInlineText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, N element, int width, int baseHeight, string space, bool wrap, ref bool endedWithBreak)
        {
            value = value ?? "";
            if (space == "p" || space == "pw")
            {
                AddPreservedText(graphics, lines, ref line, ref pending, kind, value, url, style, baseKind, element, width, baseHeight, wrap, ref endedWithBreak);
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
                    pending = InlineAtom(graphics, kind, " ", null, style, baseKind, element);
                    continue;
                }

                var start = index;
                while (index < value.Length && !Char.IsWhiteSpace(value[index])) index++;
                var word = InlineAtom(graphics, kind, value.Substring(start, index - start), url, style, baseKind, element);
                AddInlineWord(graphics, lines, ref line, ref pending, word, width, baseHeight, wrap, style == null ? "" : style.Wb);
                endedWithBreak = false;
            }
        }

        private void AddPreservedText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, N element, int width, int baseHeight, bool wrap, ref bool endedWithBreak)
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
                var atom = InlineAtom(graphics, kind, text.ToString(), url, style, baseKind, element);
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
                var character = InlineAtom(graphics, word.K, text, word.U, word.S, word.Z, word.El);
                if (piece.Length > 0 && line.W + pieceWidth + character.W > width)
                {
                    AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z, word.El));
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    piece.Clear();
                    pieceWidth = 0;
                }
                else if (piece.Length == 0 && line.As.Count > 0 && line.W + character.W > width)
                    FinishInlineLine(lines, ref line, baseHeight, false);
                piece.Append(text);
                pieceWidth += character.W;
            }
            if (piece.Length > 0) AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z, word.El));
        }

        private Ia InlineAtom(Graphics graphics, string kind, string text, Uri url, S style, string baseKind, N element)
        {
            using (var font = BoxFont(kind, style, false, url != null, baseKind))
            {
                var width = TextRenderer.MeasureText(graphics, text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                return new Ia(kind, text, url, style, baseKind, Math.Max(1, width), font.Height, CssLineHeight(style, font.Height), element);
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

        private void AddInlineBoxes(B block, Rectangle content, IList<Ix> flow, Rectangle parentClip = default(Rectangle))
        {
            var clip = ClipsOverflow(block.S) ? content : Rectangle.Empty;
            clip = CombineClip(parentClip, clip);
            foreach (var part in flow)
            {
                var rect = part.R;
                rect.Offset(content.X, content.Y);
                boxes.Add(new Bx(part.K, part.X, part.U, part.S, rect, false, block, true, clip, part.El));
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
                var style = block.S;
                var explicitWidth = style != null && style.W > 0;
                var explicitHeight = style != null && style.H > 0;
                var renderWidth = explicitWidth ? width : Math.Min(width, image.Width);
                var renderHeight = explicitHeight ? style.H : Math.Max(1, (int)Math.Round(image.Height * (double)renderWidth / image.Width));
                if (!explicitWidth && !explicitHeight && renderHeight > 360)
                {
                    renderWidth = Math.Max(1, (int)Math.Round(renderWidth * 360D / renderHeight));
                    renderHeight = 360;
                }
                return new Size(Math.Max(1, renderWidth), Math.Max(1, renderHeight));
            }
            return new Size(width, block.S != null && block.S.H > 0 ? block.S.H : 72);
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

        private static void PaintFind(Graphics graphics, Rectangle rect, bool active)
        {
            rect = Rectangle.Intersect(rect, Rectangle.Round(graphics.VisibleClipBounds));
            if (rect.Width < 1 || rect.Height < 1) return;
            using (var brush = new SolidBrush(active ? Color.FromArgb(145, 251, 146, 60) : Color.FromArgb(85, 250, 204, 21)))
                graphics.FillRectangle(brush, rect);
            if (active)
                using (var pen = new Pen(Color.FromArgb(194, 65, 12)))
                    graphics.DrawRectangle(pen, rect.X, rect.Y, Math.Max(0, rect.Width - 1), Math.Max(0, rect.Height - 1));
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
                var tiny = image.Width < 4 || image.Height < 4;
                var interpolation = graphics.InterpolationMode;
                var offset = graphics.PixelOffsetMode;
                try
                {
                    graphics.InterpolationMode = tiny ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = tiny ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                    PaintImage(graphics, image, new Rectangle(rect.X, rect.Y, Math.Min(rect.Width, size.Width), Math.Min(rect.Height, size.Height)), opacity);
                }
                finally
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.PixelOffsetMode = offset;
                }
                return;
            }

            using (var brush = new SolidBrush(PaintColor(Color.FromArgb(20, color), opacity))) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(PaintColor(Color.FromArgb(80, color), opacity))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            PaintText(graphics, box.X.Length == 0 ? "Image unavailable" : box.X, Font, new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16), color, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak, opacity);
        }

        private void AddMapHits(Bx box, Rectangle imageRect, Rectangle clip)
        {
            if (box == null || box.B == null || box.B.Mp == null || imageRect.Width < 1 || imageRect.Height < 1) return;
            var visible = VisibleRect(imageRect, clip);
            if (visible.IsEmpty) return;
            var source = MapSourceSize(box.B, imageRect);
            for (var index = box.B.Mp.As.Count - 1; index >= 0; index--)
            {
                var area = box.B.Mp.As[index];
                var bounds = Rectangle.Intersect(visible, MapBounds(area, imageRect, source));
                if (!bounds.IsEmpty) hits.Add(new Hit(bounds, area.U, null, area.El, area, imageRect, source));
            }
        }

        private SizeF MapSourceSize(B block, Rectangle imageRect)
        {
            var width = (double)block.Mw;
            var height = (double)block.Mh;
            Image image;
            if (block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
            {
                if (width > 0 && height <= 0) height = image.Height * width / image.Width;
                else if (height > 0 && width <= 0) width = image.Width * height / image.Height;
                else
                {
                    if (width <= 0) width = image.Width;
                    if (height <= 0) height = image.Height;
                }
            }
            if (width <= 0 && block.S != null && block.S.W > 0) width = block.S.W;
            if (height <= 0 && block.S != null && block.S.H > 0) height = block.S.H;
            if (width <= 0) width = imageRect.Width;
            if (height <= 0) height = imageRect.Height;
            return new SizeF((float)Math.Max(1D, width), (float)Math.Max(1D, height));
        }

        private static Rectangle MapBounds(Ar area, Rectangle imageRect, SizeF source)
        {
            if (area == null) return Rectangle.Empty;
            if (area.K == "d") return imageRect;
            if (area.K == "r" && area.Cs.Count >= 4)
                return ScaledBounds(imageRect, source, area.Cs[0], area.Cs[1], area.Cs[2], area.Cs[3]);
            if (area.K == "c" && area.Cs.Count >= 3)
                return ScaledBounds(imageRect, source, area.Cs[0] - area.Cs[2], area.Cs[1] - area.Cs[2], area.Cs[0] + area.Cs[2], area.Cs[1] + area.Cs[2]);
            if (area.K != "p" || area.Cs.Count < 6) return Rectangle.Empty;
            var left = area.Cs[0];
            var right = left;
            var top = area.Cs[1];
            var bottom = top;
            for (var index = 2; index + 1 < area.Cs.Count; index += 2)
            {
                left = Math.Min(left, area.Cs[index]);
                right = Math.Max(right, area.Cs[index]);
                top = Math.Min(top, area.Cs[index + 1]);
                bottom = Math.Max(bottom, area.Cs[index + 1]);
            }
            return ScaledBounds(imageRect, source, left, top, right, bottom);
        }

        private static Rectangle ScaledBounds(Rectangle imageRect, SizeF source, double left, double top, double right, double bottom)
        {
            var x1 = imageRect.Left + left * imageRect.Width / source.Width;
            var x2 = imageRect.Left + right * imageRect.Width / source.Width;
            var y1 = imageRect.Top + top * imageRect.Height / source.Height;
            var y2 = imageRect.Top + bottom * imageRect.Height / source.Height;
            var l = Math.Max(imageRect.Left, Math.Min(x1, x2));
            var r = Math.Min(imageRect.Right, Math.Max(x1, x2));
            var t = Math.Max(imageRect.Top, Math.Min(y1, y2));
            var b = Math.Min(imageRect.Bottom, Math.Max(y1, y2));
            if (r <= l || b <= t) return Rectangle.Empty;
            return Rectangle.FromLTRB((int)Math.Floor(l), (int)Math.Floor(t), (int)Math.Ceiling(r), (int)Math.Ceiling(b));
        }

        private static void PaintGauge(Graphics graphics, string kind, Vm model, S style, Rectangle rect, int opacity)
        {
            if (model == null || rect.Width < 1 || rect.Height < 1) return;
            var trackColor = style != null && style.Bg.Length > 0 ? CssColor(style.Bg, Color.FromArgb(226, 232, 240)) : Color.FromArgb(226, 232, 240);
            var fillColor = kind == "meter" ? model.Q == 2 ? Color.FromArgb(185, 28, 28) : model.Q == 1 ? Color.FromArgb(183, 121, 31) : Color.FromArgb(21, 128, 61) : Color.FromArgb(0, 102, 204);
            if (style != null && style.F.Length > 0) fillColor = CssColor(style.F, fillColor);
            var borderColor = style != null && style.Bc.Length > 0 ? CssColor(style.Bc, Color.FromArgb(148, 163, 184)) : Color.FromArgb(148, 163, 184);
            using (var brush = new SolidBrush(PaintColor(trackColor, opacity))) graphics.FillRectangle(brush, rect);
            var inner = new Rectangle(rect.X + 2, rect.Y + 2, Math.Max(0, rect.Width - 4), Math.Max(0, rect.Height - 4));
            if (inner.Width > 0 && inner.Height > 0)
            {
                Rectangle fill;
                if (model.In)
                {
                    var width = Math.Max(1, inner.Width / 3);
                    fill = new Rectangle(inner.X + (inner.Width - width) / 2, inner.Y, width, inner.Height);
                }
                else
                {
                    var width = (int)Math.Round(inner.Width * Math.Max(0, Math.Min(1, model.R)));
                    if (model.R > 0 && width == 0) width = 1;
                    fill = new Rectangle(inner.X, inner.Y, Math.Max(0, Math.Min(inner.Width, width)), inner.Height);
                }
                if (fill.Width > 0)
                    using (var brush = new SolidBrush(PaintColor(fillColor, opacity))) graphics.FillRectangle(brush, fill);
            }
            if (rect.Width > 1 && rect.Height > 1)
                using (var pen = new Pen(PaintColor(borderColor, opacity))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        }

        private void PaintField(Graphics graphics, Bx box, Rectangle rect, Color color, int opacity)
        {
            var field = box.B.I;
            var disabled = field.Ds;
            var background = disabled ? Color.FromArgb(238, 238, 238) : field.Ok ? Color.White : Color.FromArgb(255, 241, 242);
            if (field.S != null && field.S.Bg.Length > 0) background = CssColor(field.S.Bg, background);
            var foreground = disabled ? Color.FromArgb(130, 130, 130) : field.S == null ? color : CssColor(field.S.F, color);
            background = PaintColor(background, opacity);

            if (field.K == "checkbox" || field.K == "radio")
            {
                var mark = new Rectangle(rect.X, rect.Y + 3, 17, 17);
                using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, mark);
                using (var pen = new Pen(PaintColor(field.Ok ? Color.FromArgb(120, 120, 120) : Color.FromArgb(185, 28, 28), opacity), field.Ok ? 1F : 2F))
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

            if (field.K == "range")
            {
                using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, rect);
                var rangeBorder = field.Ok ? Color.FromArgb(165, 165, 165) : Color.FromArgb(185, 28, 28);
                using (var pen = new Pen(PaintColor(rangeBorder, opacity), field.Ok ? 1F : 2F)) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                var track = new Rectangle(rect.X + 12, rect.Top + rect.Height / 2 - 2, Math.Max(1, rect.Width - 24), 4);
                var thumbX = track.Left + (int)Math.Round((track.Width - 1) * RangeRatio(field));
                using (var brush = new SolidBrush(PaintColor(Color.FromArgb(205, 210, 216), opacity))) graphics.FillRectangle(brush, track);
                using (var brush = new SolidBrush(PaintColor(disabled ? Color.Gray : Color.FromArgb(0, 102, 204), opacity))) graphics.FillRectangle(brush, new Rectangle(track.Left, track.Top, Math.Max(1, thumbX - track.Left), track.Height));
                using (var brush = new SolidBrush(PaintColor(disabled ? Color.DarkGray : Color.White, opacity))) graphics.FillEllipse(brush, thumbX - 7, track.Top - 6, 14, 14);
                using (var pen = new Pen(PaintColor(rangeBorder, opacity))) graphics.DrawEllipse(pen, thumbX - 7, track.Top - 6, 14, 14);
                return;
            }

            using (var brush = new SolidBrush(background)) graphics.FillRectangle(brush, rect);
            var border = !field.Ok ? Color.FromArgb(185, 28, 28) : box.K == "button" ? Color.FromArgb(145, 145, 145) : Color.FromArgb(165, 165, 165);
            using (var pen = new Pen(PaintColor(border, opacity), field.Ok ? 1F : 2F)) graphics.DrawRectangle(pen, rect.X + (field.Ok ? 0 : 1), rect.Y + (field.Ok ? 0 : 1), rect.Width - (field.Ok ? 1 : 3), rect.Height - (field.Ok ? 1 : 3));

            var text = field.K == "select" ? OptionText(field) : field.V ?? "";
            var placeholder = text.Length == 0 && field.P.Length > 0;
            if (placeholder) text = field.P;
            if (field.K == "password" && !placeholder) text = new string('*', text.Length);
            var textColor = placeholder ? Color.FromArgb(130, 130, 130) : foreground;
            var flags = box.K == "button" ? TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding : TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            var pad = box.K == "button" ? 2 : 7;
            var reserve = field.K == "select" ? 24 : field.K == "file" ? 76 : 0;
            var textLeft = rect.X + pad;
            if (field.K == "select")
            {
                var middle = rect.Top + rect.Height / 2;
                var arrow = new Point[] { new Point(rect.Right - 17, middle - 2), new Point(rect.Right - 7, middle - 2), new Point(rect.Right - 12, middle + 4) };
                using (var brush = new SolidBrush(PaintColor(foreground, opacity))) graphics.FillPolygon(brush, arrow);
            }
            else if (field.K == "file")
            {
                var button = new Rectangle(Math.Max(rect.Left, rect.Right - 72), rect.Top + 3, Math.Min(68, rect.Width), Math.Max(1, rect.Height - 6));
                using (var brush = new SolidBrush(PaintColor(Color.FromArgb(240, 242, 245), opacity))) graphics.FillRectangle(brush, button);
                using (var pen = new Pen(PaintColor(Color.FromArgb(145, 145, 145), opacity))) graphics.DrawRectangle(pen, button.X, button.Y, Math.Max(0, button.Width - 1), Math.Max(0, button.Height - 1));
                using (var font = BoxFont(box.K, field.S, false))
                    PaintText(graphics, "Browse", font, button, foreground, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding, opacity);
            }
            else if (field.K == "color")
            {
                var swatch = new Rectangle(rect.X + 5, rect.Y + 5, Math.Min(24, Math.Max(1, rect.Width - 10)), Math.Max(1, rect.Height - 10));
                using (var brush = new SolidBrush(PaintColor(CssColor(field.V, Color.Black), opacity))) graphics.FillRectangle(brush, swatch);
                using (var pen = new Pen(PaintColor(Color.FromArgb(120, 120, 120), opacity))) graphics.DrawRectangle(pen, swatch.X, swatch.Y, Math.Max(0, swatch.Width - 1), Math.Max(0, swatch.Height - 1));
                textLeft += 30;
            }
            using (var font = BoxFont(box.K, field.S, false))
                PaintText(graphics, box.K == "button" ? box.X : text, font, new Rectangle(textLeft, rect.Y + 3, Math.Max(1, rect.Right - textLeft - pad - reserve), Math.Max(1, rect.Height - 6)), textColor, flags, opacity);
        }

        private static double RangeRatio(Fi field)
        {
            decimal low;
            decimal high;
            decimal value;
            if (!Decimal.TryParse(field.Lo, NumberStyles.Float, CultureInfo.InvariantCulture, out low)) low = 0;
            if (!Decimal.TryParse(field.Hi, NumberStyles.Float, CultureInfo.InvariantCulture, out high)) high = 100;
            if (!Decimal.TryParse(field.V, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) value = low;
            if (high <= low) return 0;
            value = Math.Max(low, Math.Min(high, value));
            return (double)((value - low) / (high - low));
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
                        if (block.U.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase)) data = EmbeddedPic(block.U);
                        else if (block.U.IsFile)
                        {
                            var path = block.U.LocalPath;
                            data = await Task.Factory.StartNew(delegate { return File.ReadAllBytes(path); });
                        }
                        else
                        {
                            picWeb.Headers[HttpRequestHeader.UserAgent] = "Lea/0.2";
                            data = await picWeb.DownloadDataTaskAsync(block.U);
                        }
                        if (data == null || data.Length == 0 || data.Length > PicBytes) throw new InvalidDataException("Image resource is empty or too large.");
                        using (var stream = new MemoryStream(data))
                        using (var loaded = Image.FromStream(stream)) image = new Bitmap(loaded);
                    }
                    catch { image = null; }
                    StorePic(key, image);
                }
            }
            finally { picBusy = false; }
        }

        private static byte[] EmbeddedPic(Uri url)
        {
            var source = url == null ? "" : url.OriginalString;
            var comma = source.IndexOf(',');
            if (comma < 6 || source.Length - comma - 1 > PicBytes * 4 / 3 + 4096) return null;
            var metadata = source.Substring(5, comma - 5).ToLowerInvariant();
            var type = metadata.Split(';')[0];
            if (metadata.IndexOf(";base64", StringComparison.Ordinal) < 0 ||
                (type != "image/png" && type != "image/jpeg" && type != "image/gif" && type != "image/bmp" && type != "image/x-icon" && type != "image/vnd.microsoft.icon")) return null;
            try
            {
                var data = Convert.FromBase64String(Uri.UnescapeDataString(source.Substring(comma + 1)));
                return data.Length <= PicBytes ? data : null;
            }
            catch (FormatException) { return null; }
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
            var box = BoxAt(e.Location);
            SetHover(hit != null && hit.A != null ? hit.El : box == null ? null : box.El);
            var summary = box != null && box.B != null && box.B.K == "summary" && !String.IsNullOrEmpty(box.B.Mk);
            var link = hit != null && hit.U != null || box != null && box.U != null;
            Cursor = box == null || (!summary && !link && (box.B == null || box.B.I == null))
                ? Cursors.Default
                : box.B != null && CanEdit(box.B.I) ? Cursors.IBeam : Cursors.Hand;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (edit != null && ed.Visible && ed.Bounds.Contains(PointToClient(Control.MousePosition))) SetHover(edit.El);
            else SetHover(null);
        }

        protected override async void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hit = HitAt(e.Location);
            if (hit == null)
            {
                var target = FocusAt(e.Location);
                EndEdit(false);
                if (target != null)
                {
                    Focus();
                    SetFocus(target.El, target.Sl);
                }
                else SetFocus(null);
                return;
            }
            try
            {
                if (hit.B != null && hit.B.B != null && hit.B.B.I != null) await FieldClickA(hit.B, e.Location);
                else if (hit.U != null)
                {
                    EndEdit(false);
                    SetFocus(null);
                    await ActivateLinkA(hit.U, LinkElement(hit.El));
                }
                else if (hit.B != null && hit.B.B != null && hit.B.B.K == "summary")
                {
                    EndEdit(false);
                    Focus();
                    SetFocus(hit.El);
                    Ds(hit.El);
                }
                else if (hit.A != null)
                {
                    EndEdit(false);
                    var target = FocusAt(e.Location);
                    if (target != null)
                    {
                        Focus();
                        SetFocus(target.El, target.Sl);
                    }
                    else SetFocus(null);
                }
            }
            catch { Cursor = Cursors.Default; }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            var modifiers = keyData & Keys.Modifiers;
            if ((keyData & Keys.KeyCode) == Keys.Tab && (modifiers == Keys.None || modifiers == Keys.Shift)) return true;
            if (modifiers == (Keys.Alt | Keys.Shift) && AccessTarget(KeyText(keyData)) != null) return true;
            return base.IsInputKey(keyData);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (base.ProcessCmdKey(ref msg, keyData)) return true;
            var modifiers = keyData & Keys.Modifiers;
            var key = KeyText(keyData);
            if (modifiers != (Keys.Alt | Keys.Shift) || AccessTarget(key) == null) return false;
            ActivateAccessCommand(key);
            return true;
        }

        private async void ActivateAccessCommand(string key)
        {
            try { await Aa(key); }
            catch { Cursor = Cursors.Default; }
        }

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Alt && e.Shift && !e.Control)
            {
                if (await Aa(KeyText(e.KeyData)) == 0) return;
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
            {
                if (Kf(e.Shift) == 0) return;
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Space) return;
            var target = CurrentFocusTarget();
            if (target == null) return;
            var link = target.U != null;
            var summary = DisclosureSummary(target.El) != null;
            var field = target.B != null && target.B.B != null ? target.B.B.I : null;
            if (e.KeyCode == Keys.Space && link) return;
            if (!link && !summary && field == null) return;
            if (await Ka() == 0) return;
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        // Kf moves through Lea's bounded sequential keyboard targets.
        public int Kf(bool back = false)
        {
            var targets = FocusTargets(true);
            if (targets.Count == 0)
            {
                EndEdit(false);
                SetFocus(null);
                return 0;
            }
            var index = FocusIndex(targets);
            index = index < 0 ? (back ? targets.Count - 1 : 0) : (index + (back ? -1 : 1) + targets.Count) % targets.Count;
            if (edit != null)
            {
                edit = null;
                ed.Visible = false;
            }
            var target = targets[index];
            Focus();
            SetFocus(target.El, target.Sl);
            ShowKeyTarget();
            return FocusIndex(FocusTargets(true)) + 1;
        }

        // Ka activates the focused link, area, field, or disclosure.
        public async Task<int> Ka()
        {
            var target = CurrentFocusTarget();
            if (target == null) return 0;
            if (target.U != null)
            {
                await ActivateLinkA(target.U, target.El);
                return 1;
            }
            if (DisclosureSummary(target.El) != null) return Ds(target.El);
            var field = target.B == null || target.B.B == null ? null : target.B.B.I;
            if (field == null || field.Ds) return 0;
            var rect = FocusBounds(target);
            await FieldClickA(target.B, rect.IsEmpty ? Point.Empty : new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2));
            return 1;
        }

        // Aa activates one assigned Alt+Shift page shortcut without claiming unmatched keys.
        public async Task<int> Aa(string key)
        {
            var target = AccessTarget(key);
            if (target == null) return 0;
            if (edit != null)
            {
                edit = null;
                ed.Visible = false;
            }
            Focus();
            SetFocus(target.T.El, target.T.Sl);
            ShowKeyTarget();
            await Ka();
            return 1;
        }

        private async Task ActivateLinkA(Uri url, N element)
        {
            if (url == null) return;
            element = LinkElement(element);
            var before = goRequests;
            Ck(ClickElement(element));
            if (goRequests != before) return;
            if (element != null && element.A.ContainsKey("download") && Dr != null)
            {
                Du = url;
                Dn = DownloadName(url, element.Av("download"));
                Dr(this, EventArgs.Empty);
            }
            else await GoA(url.ToString());
        }

        private bool ApplyAutofocus(P page)
        {
            if (page == null || page.U == null) return false;
            if (page.U.Fragment.Length > 0 && e.Ft(page, page.U.Fragment) != null) return false;
            var candidates = e.Afs(page);
            if (candidates.Count == 0) return false;
            var targets = FocusTargets(false);
            foreach (var candidate in candidates)
                foreach (var target in targets)
                {
                    if (!Object.ReferenceEquals(candidate, target.El)) continue;
                    SetFocus(target.El, target.Sl);
                    ShowKeyTarget();
                    if ((Focused || ContainsFocus) && target.B != null && target.B.B != null && CanEdit(target.B.B.I)) StartEdit(target.B.B.I);
                    return true;
                }
            return false;
        }

        private bool ApplyCommandFocus(P page)
        {
            if (page == null) return false;
            var candidate = e.Kp(page);
            if (candidate == null) return false;
            foreach (var target in FocusTargets(false))
            {
                if (!Object.ReferenceEquals(candidate, target.El)) continue;
                SetFocus(target.El, target.Sl);
                ShowKeyTarget();
                if ((Focused || ContainsFocus) && target.B != null && target.B.B != null && CanEdit(target.B.B.I)) StartEdit(target.B.B.I);
                return true;
            }
            return false;
        }

        private void KeepValidFocus()
        {
            if (focus == null) return;
            foreach (var target in FocusTargets(false))
                if (Object.ReferenceEquals(focus, target.El) && (keySlot < 0 || keySlot == target.Sl)) return;
            if (edit != null) EndEdit(false);
            else SetFocus(null);
        }

        private List<Kx> AccessTargets()
        {
            var result = new List<Kx>();
            var targets = FocusTargets(false);
            targets.Sort(CompareAccessTargets);
            var elements = new HashSet<N>();
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var target in targets)
            {
                if (!elements.Add(target.El)) continue;
                foreach (var key in e.Aks(target.El))
                {
                    if (!keys.Add(key)) continue;
                    result.Add(new Kx(target, key));
                    break;
                }
                if (result.Count == MaxAccessTargets) break;
            }
            return result;
        }

        private Kx AccessTarget(N element)
        {
            if (element == null) return null;
            foreach (var target in AccessTargets())
                if (Object.ReferenceEquals(target.T.El, element)) return target;
            return null;
        }

        private Kx AccessTarget(string key)
        {
            var value = NormalizeAccessKey(key);
            if (value.Length == 0) return null;
            foreach (var target in AccessTargets())
                if (target.K == value) return target;
            return null;
        }

        private static int CompareAccessTargets(Kt left, Kt right)
        {
            var value = left.Or.CompareTo(right.Or);
            return value != 0 ? value : left.Sq.CompareTo(right.Sq);
        }

        private static string NormalizeAccessKey(string key)
        {
            var value = (key ?? "").Trim();
            if (value.Length != 1) return "";
            var letter = value[0];
            if (!((letter >= 'a' && letter <= 'z') || (letter >= 'A' && letter <= 'Z') || (letter >= '0' && letter <= '9'))) return "";
            return Char.ToUpperInvariant(letter).ToString();
        }

        private static string KeyText(Keys keyData)
        {
            var value = keyData & Keys.KeyCode;
            if (value >= Keys.A && value <= Keys.Z) return ((char)('A' + (int)value - (int)Keys.A)).ToString();
            if (value >= Keys.D0 && value <= Keys.D9) return ((char)('0' + (int)value - (int)Keys.D0)).ToString();
            if (value >= Keys.NumPad0 && value <= Keys.NumPad9) return ((char)('0' + (int)value - (int)Keys.NumPad0)).ToString();
            return "";
        }

        private List<Kt> FocusTargets(bool sequential)
        {
            var targets = new List<Kt>();
            if (At == null) return targets;
            EnsureLayout();
            var order = new Dictionary<N, int>();
            var nextOrder = 0;
            NodeOrders(At.D, order, ref nextOrder);
            var grouped = new Dictionary<N, Kt>();
            var sequence = 0;

            foreach (var box in boxes)
            {
                if (box.Title || box.El == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                var element = box.El;
                var url = box.U;
                var action = (Bx)null;
                var native = false;
                if (box.B != null && box.B.I != null)
                {
                    if (box.B.I.Ds) continue;
                    element = box.B.I.El;
                    action = box;
                    native = true;
                }
                else if (box.B != null && box.B.K == "summary" && !String.IsNullOrEmpty(box.B.Mk))
                {
                    element = DisclosureSummary(box.El);
                    action = box;
                    native = element != null;
                }
                else if (url != null)
                {
                    element = LinkElement(box.El);
                    native = element != null;
                }
                if (element == null || element.K == "area") continue;
                var tab = e.Ti(element);
                if (!native && !tab.HasValue) continue;
                if (sequential && tab.HasValue && tab.Value < 0) continue;
                var rect = KeyRect(box);
                if (rect.IsEmpty) continue;
                Kt target;
                if (!grouped.TryGetValue(element, out target))
                {
                    int value;
                    target = new Kt(element, url, action, tab, order.TryGetValue(element, out value) ? value : Int32.MaxValue, sequence++);
                    grouped[element] = target;
                    targets.Add(target);
                }
                else
                {
                    if (target.U == null) target.U = url;
                    if (target.B == null) target.B = action;
                }
                target.Add(rect, box.Fx);
            }

            var mapRegions = 0;
            foreach (var box in boxes)
            {
                if (mapRegions == MaxKeyMapRegions) break;
                if (box.Title || box.B == null || box.B.Mp == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                var source = MapSourceSize(box.B, box.C);
                foreach (var area in box.B.Mp.As)
                {
                    if (mapRegions == MaxKeyMapRegions) break;
                    mapRegions++;
                    if (HtmlHidden(area.El)) continue;
                    var tab = e.Ti(area.El);
                    if (area.U == null && !tab.HasValue) continue;
                    if (sequential && tab.HasValue && tab.Value < 0) continue;
                    var rect = MapBounds(area, box.C, source);
                    if (!box.Clip.IsEmpty) rect = Rectangle.Intersect(rect, box.Clip);
                    if (rect.IsEmpty) continue;
                    int value;
                    var target = new Kt(area.El, area.U, null, tab, order.TryGetValue(area.El, out value) ? value : Int32.MaxValue, sequence++);
                    target.Add(rect, box.Fx);
                    targets.Add(target);
                }
            }

            targets.Sort(CompareTargets);
            if (targets.Count > MaxKeyTargets) targets.RemoveRange(MaxKeyTargets, targets.Count - MaxKeyTargets);
            var slots = new Dictionary<N, int>();
            foreach (var target in targets)
            {
                int slot;
                slots.TryGetValue(target.El, out slot);
                target.Sl = slot;
                slots[target.El] = slot + 1;
            }
            return targets;
        }

        private static void NodeOrders(N node, IDictionary<N, int> order, ref int next)
        {
            if (node == null) return;
            order[node] = next++;
            foreach (var child in node.C) NodeOrders(child, order, ref next);
        }

        private static int CompareTargets(Kt left, Kt right)
        {
            var leftTab = left.Ti.HasValue && left.Ti.Value > 0 ? left.Ti.Value : Int32.MaxValue;
            var rightTab = right.Ti.HasValue && right.Ti.Value > 0 ? right.Ti.Value : Int32.MaxValue;
            var value = leftTab.CompareTo(rightTab);
            if (value != 0) return value;
            value = left.Or.CompareTo(right.Or);
            return value != 0 ? value : left.Sq.CompareTo(right.Sq);
        }

        private static Rectangle KeyRect(Bx box)
        {
            return box == null ? Rectangle.Empty : VisibleRect(box.R, box.Clip);
        }

        private int FocusIndex(IList<Kt> targets)
        {
            if (focus == null) return -1;
            for (var index = 0; index < targets.Count; index++)
                if (Object.ReferenceEquals(targets[index].El, focus) && (keySlot < 0 || targets[index].Sl == keySlot)) return index;
            return -1;
        }

        private Kt FocusTarget(IList<Kt> targets)
        {
            var index = FocusIndex(targets);
            return index < 0 ? null : targets[index];
        }

        private Kt CurrentFocusTarget()
        {
            if (focus == null) return null;
            var target = FocusTarget(FocusTargets(true));
            return target ?? FocusTarget(FocusTargets(false));
        }

        private Kt FocusAt(Point point)
        {
            Kt found = null;
            var size = Int64.MaxValue;
            foreach (var target in FocusTargets(false))
                foreach (var region in target.Rs)
                {
                    var rect = ScreenRect(region);
                    if (!rect.Contains(point)) continue;
                    var area = (long)rect.Width * rect.Height;
                    if (area >= size) continue;
                    found = target;
                    size = area;
                }
            return found;
        }

        private Rectangle FocusBounds(Kt target)
        {
            var result = Rectangle.Empty;
            if (target == null) return result;
            foreach (var region in target.Rs)
            {
                var rect = Rectangle.Intersect(ScreenRect(region), ClientRectangle);
                if (rect.IsEmpty) continue;
                result = result.IsEmpty ? rect : Rectangle.Union(result, rect);
            }
            return result;
        }

        private void AddAccessibleBounds(ref Rectangle result, Rectangle rect, Rectangle clip, bool fixedPosition)
        {
            rect = VisibleRect(rect, clip);
            if (!fixedPosition) rect.Offset(0, -sy);
            rect = Rectangle.Intersect(rect, ClientRectangle);
            if (!rect.IsEmpty) result = result.IsEmpty ? rect : Rectangle.Union(result, rect);
        }

        private static bool OwnsElement(N root, N node)
        {
            for (var current = node; current != null; current = current.Up)
                if (Object.ReferenceEquals(current, root)) return true;
            return false;
        }

        private Rectangle ScreenRect(Kr region)
        {
            var rect = region.R;
            if (!region.Fx) rect.Offset(0, -sy);
            return rect;
        }

        private void ShowKeyTarget()
        {
            var target = CurrentFocusTarget();
            if (target == null) return;
            foreach (var region in target.Rs)
            {
                if (region.Fx) break;
                if (region.R.Top < sy + 12 || region.R.Bottom > sy + ClientSize.Height - 12)
                    sy = Math.Max(0, Math.Min(MaxScroll(), region.R.Top - 24));
                break;
            }
            PlaceEditor();
            Invalidate();
        }

        private void PaintKeyFocus(Graphics graphics)
        {
            var target = CurrentFocusTarget();
            if (target == null) return;
            using (var outer = new Pen(Color.White, 4F))
            using (var pen = new Pen(SystemColors.Highlight, 2F))
            {
                pen.DashStyle = DashStyle.Dot;
                foreach (var region in target.Rs)
                {
                    var rect = Rectangle.Intersect(ScreenRect(region), ClientRectangle);
                    if (rect.Width < 1 || rect.Height < 1) continue;
                    rect.Inflate(2, 2);
                    rect = Rectangle.Intersect(rect, ClientRectangle);
                    if (rect.Width < 2 || rect.Height < 2) continue;
                    graphics.DrawRectangle(outer, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }
            }
        }

        private static N DisclosureSummary(N element)
        {
            for (var current = element; current != null; current = current.Up)
            {
                if (current.K == "summary") return current;
                if (current.K == "details") return null;
            }
            return null;
        }

        private static string DownloadName(Uri url, string hint)
        {
            var value = Path.GetFileName((hint ?? "").Trim());
            if (value.Length == 0 && url != null)
            {
                try { value = Path.GetFileName(Uri.UnescapeDataString(url.AbsolutePath)); }
                catch { value = ""; }
            }
            if (value.Length == 0) value = "download.bin";
            foreach (var invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            if (value.Length > 120) value = value.Substring(0, 120);
            return value;
        }

        private static N LinkElement(N element)
        {
            for (var current = element; current != null; current = current.Up)
                if ((current.K == "a" || current.K == "area") && current.A.ContainsKey("href")) return current;
            return element;
        }

        private static N ClickElement(N element)
        {
            for (var current = element; current != null; current = current.Up)
            {
                if (current.A.ContainsKey("data-lea") || current.A.ContainsKey("onclick")) return current;
                if (current.K == "a") return current;
            }
            return element;
        }

        private async Task FieldClickA(Bx box, Point point)
        {
            var field = box.B.I;
            if (field == null || field.Ds) return;
            if (CanEdit(field))
            {
                StartEdit(field);
                return;
            }

            EndEdit(false);
            SetFocus(field.El);
            if (field.K == "reset" || field.K == "submit" || field.K == "button")
            {
                var before = goRequests;
                Ck(field.El);
                if (goRequests != before) return;
            }
            var restyle = false;
            if (field.K == "checkbox")
            {
                field.C = !field.C;
                restyle = ClearValidity(field);
                OnEdited();
            }
            else if (field.K == "radio")
            {
                foreach (var item in At.Fs)
                    if (item.K == "radio" && Object.ReferenceEquals(item.F, field.F) && item.N == field.N)
                    {
                        item.C = false;
                        restyle = ClearValidity(item) || restyle;
                    }
                field.C = true;
                OnEdited();
            }
            else if (field.K == "select") ShowChoices(field, box);
            else if (field.K == "file") ChooseFiles(field);
            else if (field.K == "range") SetRange(field, box, point);
            else if (field.K == "color") ChooseColor(field);
            else if (field.K == "reset") Rs(field);
            else if (field.K == "submit") await SmA(field);
            else if (field.K == "button" && Sb != null) Sb(this, EventArgs.Empty);
            if (restyle) Restyle(); else Invalidate();
        }

        private static bool CanEdit(Fi field)
        {
            return field != null && field.K != "checkbox" && field.K != "radio" && field.K != "select" && field.K != "submit" && field.K != "reset" && field.K != "button" && field.K != "file" && field.K != "range" && field.K != "color";
        }

        private void ChooseFiles(Fi field)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = field.Mu;
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;
                dialog.Filter = AcceptFilter(field.Ac);
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK) Sf(field, dialog.FileNames);
            }
        }

        private static string AcceptFilter(string accept)
        {
            var patterns = new List<string>();
            foreach (var raw in (accept ?? "").Split(','))
            {
                var value = raw.Trim().ToLowerInvariant();
                if (Regex.IsMatch(value, "^\\.[a-z0-9]{1,12}$")) patterns.Add("*" + value);
                else if (value == "image/*") patterns.AddRange(new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.ico" });
                else if (value == "text/*" || value == "text/plain") patterns.AddRange(new[] { "*.txt", "*.csv", "*.html", "*.css", "*.js", "*.json", "*.xml" });
                else if (value == "application/pdf") patterns.Add("*.pdf");
            }
            return patterns.Count == 0 ? "All files|*.*" : "Accepted files|" + String.Join(";", patterns.ToArray()) + "|All files|*.*";
        }

        private void SetRange(Fi field, Bx box, Point point)
        {
            var rect = box.C;
            if (!box.Fx) rect.Offset(0, -sy);
            decimal low;
            decimal high;
            decimal step;
            if (!Decimal.TryParse(field.Lo, NumberStyles.Float, CultureInfo.InvariantCulture, out low)) low = 0;
            if (!Decimal.TryParse(field.Hi, NumberStyles.Float, CultureInfo.InvariantCulture, out high)) high = 100;
            if (high < low) high = low;
            if (!Decimal.TryParse(field.Sp, NumberStyles.Float, CultureInfo.InvariantCulture, out step) || step <= 0) step = 1;
            var width = Math.Max(1, rect.Width - 24);
            var ratio = Math.Max(0D, Math.Min(1D, (point.X - rect.Left - 12D) / width));
            var raw = low + (high - low) * (decimal)ratio;
            var steps = Math.Round((raw - low) / step, 0, MidpointRounding.AwayFromZero);
            var value = Math.Max(low, Math.Min(high, low + steps * step));
            field.V = value.ToString(CultureInfo.InvariantCulture);
            var restyle = ClearValidity(field);
            OnEdited();
            if (restyle) Restyle(); else Invalidate();
        }

        private void ChooseColor(Fi field)
        {
            using (var dialog = new ColorDialog())
            {
                dialog.FullOpen = true;
                dialog.Color = CssColor(field.V, Color.Black);
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                field.V = String.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", dialog.Color.R, dialog.Color.G, dialog.Color.B);
                var restyle = ClearValidity(field);
                OnEdited();
                if (restyle) Restyle(); else Invalidate();
            }
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
            var restyle = ClearValidity(field);
            OnEdited();
            if (restyle) Restyle(); else Invalidate();
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
            SetFocus(field.El);
            editStart = field.V;
            editLock = true;
            ed.Multiline = field.K == "textarea";
            ed.AcceptsReturn = ed.Multiline;
            ed.ScrollBars = ed.Multiline ? ScrollBars.Vertical : ScrollBars.None;
            ed.ReadOnly = field.Ro;
            ed.MaxLength = field.Mx > 0 ? field.Mx : 32767;
            ed.PasswordChar = field.K == "password" ? '*' : '\0';
            ed.BackColor = field.Ok ? Color.White : Color.FromArgb(255, 241, 242);
            var oldFont = editFont;
            editFont = BoxFont(field.K, field.S, false);
            ed.Font = editFont;
            if (oldFont != null) oldFont.Dispose();
            ed.Text = field.V;
            editLock = false;
            PlaceEditor();
            if (!ed.Visible) return;
            ed.Focus();
            ed.SelectAll();
        }

        private void EndEdit(bool restore)
        {
            if (edit == null) return;
            if (restore) edit.V = editStart;
            edit = null;
            ed.Visible = false;
            SetFocus(null);
            Invalidate();
        }

        private void EditChanged(object sender, EventArgs args)
        {
            if (editLock || edit == null) return;
            edit.V = ed.Text;
            var restyle = ClearValidity(edit);
            OnEdited();
            NotifyAccessibility(AccessibleEvents.ValueChange);
            if (restyle) Restyle(); else Invalidate();
        }

        private async void EditKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Alt && args.Shift && !args.Control)
            {
                if (await Aa(KeyText(args.KeyData)) == 0) return;
                args.SuppressKeyPress = true;
                args.Handled = true;
            }
            else if (args.KeyCode == Keys.Tab && !args.Control && !args.Alt)
            {
                Kf(args.Shift);
                args.SuppressKeyPress = true;
                args.Handled = true;
            }
            else if (args.KeyCode == Keys.Escape)
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
                var clip = box.Clip;
                if (!box.Fx)
                {
                    rect.Offset(0, -sy);
                    if (!clip.IsEmpty) clip.Offset(0, -sy);
                }
                rect = Rectangle.Intersect(VisibleRect(rect, clip), ClientRectangle);
                if (rect.Width <= 2 || rect.Height <= 2)
                {
                    ed.Visible = false;
                    return;
                }
                ed.Bounds = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                ed.Visible = true;
                return;
            }
            ed.Visible = false;
        }

        public bool Sf(Fi field, params string[] paths)
        {
            if (At == null || field == null || field.K != "file" || field.Ds || !At.Fs.Contains(field)) return false;
            var selected = new List<string>();
            long total = 0;
            foreach (var value in paths ?? new string[0])
            {
                if (String.IsNullOrWhiteSpace(value)) continue;
                string path;
                try { path = Path.GetFullPath(value); }
                catch { return false; }
                if (!File.Exists(path) || selected.Count >= UploadFiles || (!field.Mu && selected.Count > 0)) return false;
                var length = new FileInfo(path).Length;
                if (length < 0 || length > UploadFileBytes || total + length > UploadBytes) return false;
                selected.Add(path);
                total += length;
            }

            field.Fp.Clear();
            foreach (var path in selected) field.Fp.Add(path);
            var names = new string[selected.Count];
            for (var i = 0; i < selected.Count; i++) names[i] = Path.GetFileName(selected[i]);
            field.V = String.Join("; ", names);
            var restyle = ClearValidity(field);
            OnEdited();
            if (restyle) Restyle(); else Invalidate();
            return true;
        }

        public void Rs(Fi button)
        {
            if (At == null || button == null || button.F == null) return;
            foreach (var field in At.Fs)
            {
                if (!Object.ReferenceEquals(field.F, button.F)) continue;
                field.Fp.Clear();
                field.V = field.D;
                field.C = field.Dc;
                field.Si = field.Di;
                SetValidity(field, "");
            }
            OnEdited();
            Restyle();
        }

        public P Sm(Fi button)
        {
            EndEdit(false);
            if (button == null || (!button.Nv && !Vf(button))) return At;
            if (!ValidForm(button)) return At;
            var data = FormData(button);
            var multipart = button.M == "post" && button.En == "m" && CanPost(button.U) ? MultipartData(button) : null;
            if (Sb != null) Sb(this, EventArgs.Empty);
            if (button.M == "get") return Go(FormUri(button, data));
            if (button.M != "post" || !CanPost(button.U)) return At;
            if (button.En != "m") return Po(button.U, data);
            return Pb(button.U, multipart.B, multipart.Ct);
        }

        public async Task<P> SmA(Fi button)
        {
            EndEdit(false);
            if (button == null || (!button.Nv && !Vf(button))) return At;
            if (!ValidForm(button)) return At;
            var data = FormData(button);
            var multipart = button.M == "post" && button.En == "m" && CanPost(button.U) ? MultipartData(button) : null;
            if (Sb != null) Sb(this, EventArgs.Empty);
            if (button.M == "get") return await GoA(FormUri(button, data));
            if (button.M != "post" || !CanPost(button.U)) return At;
            if (button.En != "m") return await PoA(button.U, data);
            return await PbA(button.U, multipart.B, multipart.Ct);
        }

        private bool ValidForm(Fi button)
        {
            return At != null && button != null && button.F != null && button.U != null && (button.M == "get" || button.M == "post");
        }

        public bool Vf(Fi button)
        {
            if (At == null || button == null || button.F == null) return false;
            Fi first = null;
            foreach (var field in At.Fs)
            {
                if (!Object.ReferenceEquals(field.F, button.F)) continue;
                var message = FieldError(field);
                SetValidity(field, message);
                if (message.Length > 0 && first == null) first = field;
            }
            if (first != null)
            {
                focus = first.El;
                keySlot = -1;
            }
            Restyle();
            return first == null;
        }

        private string FieldError(Fi field)
        {
            if (field == null || field.Ds || field.Ro || field.K == "submit" || field.K == "reset" || field.K == "button") return "";
            var value = field.V ?? "";
            if (field.Rq)
            {
                if (field.K == "file" && field.Fp.Count == 0) return "required";
                if (field.K == "checkbox" && !field.C) return "required";
                if (field.K == "radio")
                {
                    var selected = false;
                    foreach (var item in At.Fs)
                        if (item.K == "radio" && !item.Ds && Object.ReferenceEquals(item.F, field.F) && item.N == field.N && item.C) { selected = true; break; }
                    if (!selected) return "required";
                }
                else if (value.Length == 0) return "required";
            }
            if (field.K == "file") return FileError(field);
            if (value.Length == 0 || field.K == "checkbox" || field.K == "radio" || field.K == "select") return "";
            if (field.K == "email")
            {
                var addresses = field.Mu ? value.Split(',') : new[] { value };
                foreach (var addressValue in addresses)
                    if (!SafeMatch(addressValue.Trim(), "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")) return "email";
            }
            Uri address;
            if (field.K == "url" && !Uri.TryCreate(value, UriKind.Absolute, out address)) return "url";
            if (field.Mn > 0 && value.Length < field.Mn) return "short";
            if (field.Mx > 0 && value.Length > field.Mx) return "long";
            if (field.Pa.Length > 0 && !SafeMatch(value, "^(?:" + field.Pa + ")$", true)) return "pattern";
            if (field.K == "number" || field.K == "range") return NumberError(field, value);
            if (field.K == "color" && !SafeMatch(value, "^#[0-9a-fA-F]{6}$")) return "color";
            if (field.K == "date") return DateError(field, value);
            if (field.K == "time" || field.K == "month" || field.K == "week" || field.K == "datetime-local") return TemporalError(field, value);
            return "";
        }

        private static string FileError(Fi field)
        {
            if ((!field.Mu && field.Fp.Count > 1) || field.Fp.Count > UploadFiles) return "files";
            long total = 0;
            foreach (var path in field.Fp)
            {
                try
                {
                    if (!File.Exists(path)) return "file";
                    var length = new FileInfo(path).Length;
                    if (length > UploadFileBytes || total + length > UploadBytes) return "large";
                    total += length;
                }
                catch { return "file"; }
            }
            return "";
        }

        private static string NumberError(Fi field, string value)
        {
            decimal number;
            if (!Decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return "number";
            decimal bound;
            if (Decimal.TryParse(field.Lo, NumberStyles.Float, CultureInfo.InvariantCulture, out bound) && number < bound) return "min";
            if (Decimal.TryParse(field.Hi, NumberStyles.Float, CultureInfo.InvariantCulture, out bound) && number > bound) return "max";
            decimal step;
            if (field.Sp != "any" && Decimal.TryParse(field.Sp, NumberStyles.Float, CultureInfo.InvariantCulture, out step) && step > 0)
            {
                decimal basis;
                if (!Decimal.TryParse(field.Lo, NumberStyles.Float, CultureInfo.InvariantCulture, out basis)) basis = 0;
                if ((number - basis) % step != 0) return "step";
            }
            return "";
        }

        private static string DateError(Fi field, string value)
        {
            DateTime date;
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) return "date";
            DateTime bound;
            if (DateTime.TryParseExact(field.Lo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out bound) && date < bound) return "min";
            if (DateTime.TryParseExact(field.Hi, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out bound) && date > bound) return "max";
            return "";
        }

        private static string TemporalError(Fi field, string value)
        {
            var format = field.K == "time" ? new[] { "HH:mm", "HH:mm:ss", "HH:mm:ss.FFFFFFF" } :
                field.K == "month" ? new[] { "yyyy-MM" } :
                field.K == "datetime-local" ? new[] { "yyyy-MM-dd'T'HH:mm", "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF" } : null;
            if (field.K == "week")
            {
                var match = Regex.Match(value, "^(?<y>[0-9]{4})-W(?<w>[0-9]{2})$", RegexOptions.CultureInvariant);
                int week;
                if (!match.Success || !Int32.TryParse(match.Groups["w"].Value, out week) || week < 1 || week > 53) return "week";
            }
            else
            {
                DateTime parsed;
                if (!DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)) return field.K;
            }
            if (field.Lo.Length > 0 && String.CompareOrdinal(value, field.Lo) < 0) return "min";
            if (field.Hi.Length > 0 && String.CompareOrdinal(value, field.Hi) > 0) return "max";
            return "";
        }

        private static bool SafeMatch(string value, string pattern, bool invalidPatternPasses = false)
        {
            try { return Regex.IsMatch(value ?? "", pattern, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50)); }
            catch (ArgumentException) { return invalidPatternPasses; }
            catch (RegexMatchTimeoutException) { return false; }
        }

        private static void SetValidity(Fi field, string message)
        {
            if (field == null) return;
            field.Vm = message ?? "";
            field.Ok = field.Vm.Length == 0;
            if (field.El == null) return;
            if (field.Ok) field.El.A.Remove("data-lea-invalid");
            else field.El.A["data-lea-invalid"] = field.Vm;
        }

        private static bool ClearValidity(Fi field)
        {
            if (field == null || (field.Ok && (field.El == null || !field.El.A.ContainsKey("data-lea-invalid")))) return false;
            SetValidity(field, "");
            return true;
        }

        private string FormData(Fi button)
        {
            var values = new List<string>();
            foreach (var field in FormFields(button))
            {
                if (field.K == "file")
                {
                    foreach (var path in field.Fp) values.Add(FormEncode(field.N) + "=" + FormEncode(Path.GetFileName(path)));
                    continue;
                }
                values.Add(FormEncode(field.N) + "=" + FormEncode(field.V ?? ""));
            }
            return String.Join("&", values.ToArray());
        }

        private IEnumerable<Fi> FormFields(Fi button)
        {
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
                yield return field;
            }
        }

        private Mp MultipartData(Fi button)
        {
            var boundary = "----Lea" + Guid.NewGuid().ToString("N");
            using (var output = new MemoryStream())
            {
                foreach (var field in FormFields(button))
                {
                    if (field.K == "file")
                    {
                        foreach (var path in field.Fp)
                        {
                            var data = File.ReadAllBytes(path);
                            if (data.Length > UploadFileBytes) throw new InvalidDataException("Lea upload file is too large.");
                            WriteText(output, "--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + HeaderValue(field.N) + "\"; filename=\"" + HeaderValue(Path.GetFileName(path)) + "\"\r\nContent-Type: " + FileType(path) + "\r\n\r\n");
                            output.Write(data, 0, data.Length);
                            WriteText(output, "\r\n");
                        }
                    }
                    else
                    {
                        WriteText(output, "--" + boundary + "\r\nContent-Disposition: form-data; name=\"" + HeaderValue(field.N) + "\"\r\n\r\n" + (field.V ?? "") + "\r\n");
                    }
                    if (output.Length > UploadBytes + 65536) throw new InvalidDataException("Lea multipart request is too large.");
                }
                WriteText(output, "--" + boundary + "--\r\n");
                return new Mp(output.ToArray(), "multipart/form-data; boundary=" + boundary);
            }
        }

        private static void WriteText(Stream stream, string value)
        {
            var data = Encoding.UTF8.GetBytes(value ?? "");
            stream.Write(data, 0, data.Length);
        }

        private static string HeaderValue(string value)
        {
            return (value ?? "").Replace("\r", "").Replace("\n", "").Replace("\"", "%22");
        }

        private static string FileType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".txt": return "text/plain";
                case ".csv": return "text/csv";
                case ".html": case ".htm": return "text/html";
                case ".css": return "text/css";
                case ".json": return "application/json";
                case ".pdf": return "application/pdf";
                case ".png": return "image/png";
                case ".jpg": case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "application/octet-stream";
            }
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
            for (var index = hits.Count - 1; index >= 0; index--)
                if (hits[index].Contains(point)) return hits[index];
            return null;
        }

        private Bx BoxAt(Point point)
        {
            for (var i = boxes.Count - 1; i >= 0; i--)
            {
                var box = boxes[i];
                if (box.El == null || Hidden(box.S)) continue;
                var at = point;
                if (!box.Fx) at.Offset(0, sy);
                var rect = VisibleRect(box.R, box.Clip);
                if (rect.Contains(at)) return box;
            }
            return null;
        }

        private void SetHover(N node)
        {
            if (Object.ReferenceEquals(hover, node)) return;
            hover = node;
            Restyle();
        }

        private void SetFocus(N node, int slot = -1)
        {
            if (Object.ReferenceEquals(focus, node) && keySlot == slot) return;
            var restyle = !Object.ReferenceEquals(focus, node);
            focus = node;
            keySlot = node == null ? -1 : slot;
            if (restyle) Restyle(); else Invalidate();
            NotifyAccessibility(AccessibleEvents.Focus);
        }

        private void Restyle()
        {
            if (At == null) return;
            e.Dy(At, hover, focus);
            Dirty();
            Invalidate();
        }

        private static bool Hidden(S style)
        {
            return style != null && (style.Vi == "h" || style.Vi == "c");
        }

        private static bool HtmlHidden(N element)
        {
            for (var current = element; current != null; current = current.Up)
                if (current.A.ContainsKey("hidden")) return true;
            return false;
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

        private void NotifyAccessibility(AccessibleEvents kind)
        {
            if (IsHandleCreated) AccessibilityNotifyClients(kind, -1);
        }

        private Ax AccessibilityRoot()
        {
            return At == null || At.Axs.Count == 0 ? null : At.Axs[0];
        }

        private List<Ax> AccessibilityChildren(Ax parent)
        {
            var result = new List<Ax>();
            if (At == null) return result;
            parent = CurrentAccessibility(parent);
            if (parent == null) return result;
            foreach (var item in At.Axs)
                if (!Object.ReferenceEquals(item, parent) && Object.ReferenceEquals(item.Up, parent)) result.Add(item);
            return result;
        }

        private Ax AccessibilityAt(Point screenPoint)
        {
            if (At == null) return null;
            var point = PointToClient(screenPoint);
            Ax found = null;
            var size = Int64.MaxValue;
            foreach (var item in At.Axs)
            {
                if (Object.ReferenceEquals(item, AccessibilityRoot())) continue;
                var bounds = Ab(item);
                if (!bounds.Contains(point)) continue;
                var area = (long)bounds.Width * bounds.Height;
                if (area > size) continue;
                found = item;
                size = area;
            }
            return found;
        }

        private bool FocusAccessibility(Ax item)
        {
            item = CurrentAccessibility(item);
            if (item == null || !item.Is(Lea.Engine.Ax.Fs)) return false;
            foreach (var target in FocusTargets(false))
            {
                if (!Object.ReferenceEquals(target.El, item.El)) continue;
                Focus();
                SetFocus(target.El, target.Sl);
                ShowKeyTarget();
                return true;
            }
            return false;
        }

        private Ax CurrentAccessibility(Ax item)
        {
            if (At == null || item == null) return null;
            if (At.Axs.Contains(item)) return item;
            return item.El == null ? null : e.Ax(At, item.El);
        }

        private async void ActivateAccessibility(Ax item)
        {
            if (!FocusAccessibility(item)) return;
            await Ka();
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

        private sealed class Mp
        {
            public readonly byte[] B;
            public readonly string Ct;

            public Mp(byte[] body, string type)
            {
                B = body;
                Ct = type;
            }
        }

        private sealed class Va : AccessibleObject
        {
            private readonly V v;

            public Va(V owner) { v = owner; }

            public override string Name
            {
                get { return v.T.Length == 0 ? "Lea document" : v.T; }
                set { }
            }
            public override string Description { get { return "Lea web document"; } }
            public override AccessibleRole Role { get { return AccessibleRole.Document; } }
            public override AccessibleStates State
            {
                get { return AccessibleStates.Focusable | (v.Focused || v.ContainsFocus ? AccessibleStates.Focused : AccessibleStates.None); }
            }
            public override Rectangle Bounds { get { return v.RectangleToScreen(v.ClientRectangle); } }
            public override int GetChildCount()
            {
                var root = v.AccessibilityRoot();
                return root == null ? 0 : v.AccessibilityChildren(root).Count;
            }
            public override AccessibleObject GetChild(int index)
            {
                var root = v.AccessibilityRoot();
                if (root == null) return null;
                var children = v.AccessibilityChildren(root);
                return index < 0 || index >= children.Count ? null : new Xa(v, children[index]);
            }
            public override AccessibleObject GetFocused()
            {
                var item = v.focus == null ? null : v.Ax(v.focus);
                return item == null ? null : new Xa(v, item);
            }
            public override AccessibleObject HitTest(int x, int y)
            {
                var item = v.AccessibilityAt(new Point(x, y));
                return item == null ? base.HitTest(x, y) : new Xa(v, item);
            }
        }

        private sealed class Xa : AccessibleObject
        {
            private readonly V v;
            private readonly Ax a;

            public Xa(V owner, Ax item)
            {
                v = owner;
                a = item;
            }

            private Ax A { get { return v.CurrentAccessibility(a) ?? a; } }

            public override string Name
            {
                get { return A.N; }
                set { }
            }
            public override string Description { get { return A.D; } }
            public override string Value
            {
                get { return A.V; }
                set { }
            }
            public override string KeyboardShortcut { get { return A.El == null ? "" : v.Kl(A.El); } }
            public override string DefaultAction { get { return AccessibilityAction(A); } }
            public override AccessibleRole Role { get { return AccessibilityRole(A.K); } }
            public override AccessibleStates State { get { return AccessibilityState(A, v.Ab(A)); } }
            public override Rectangle Bounds
            {
                get
                {
                    var bounds = v.Ab(A);
                    return bounds.IsEmpty ? Rectangle.Empty : v.RectangleToScreen(bounds);
                }
            }
            public override AccessibleObject Parent
            {
                get
                {
                    var root = v.AccessibilityRoot();
                    var item = A;
                    return item.Up == null || Object.ReferenceEquals(item.Up, root) ? v.Ao : new Xa(v, item.Up);
                }
            }
            public override int GetChildCount() { return v.AccessibilityChildren(A).Count; }
            public override AccessibleObject GetChild(int index)
            {
                var children = v.AccessibilityChildren(A);
                return index < 0 || index >= children.Count ? null : new Xa(v, children[index]);
            }
            public override AccessibleObject GetFocused()
            {
                if (A.Is(Lea.Engine.Ax.Fc)) return this;
                var item = v.focus == null ? null : v.Ax(v.focus);
                return item != null && OwnsAx(A, item) ? new Xa(v, item) : null;
            }
            public override AccessibleObject HitTest(int x, int y)
            {
                var item = v.AccessibilityAt(new Point(x, y));
                return item != null && OwnsAx(A, item) ? new Xa(v, item) : base.HitTest(x, y);
            }
            public override void DoDefaultAction() { v.ActivateAccessibility(A); }
            public override void Select(AccessibleSelection flags)
            {
                if ((flags & (AccessibleSelection.TakeFocus | AccessibleSelection.TakeSelection)) != 0) v.FocusAccessibility(A);
            }
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (navdir == AccessibleNavigation.FirstChild) return GetChild(0);
                if (navdir == AccessibleNavigation.LastChild) return GetChild(GetChildCount() - 1);
                if (navdir == AccessibleNavigation.Up) return Parent;
                var item = A;
                if (item.Up == null) return base.Navigate(navdir);
                var siblings = v.AccessibilityChildren(item.Up);
                var index = siblings.IndexOf(item);
                if (navdir == AccessibleNavigation.Next || navdir == AccessibleNavigation.Right || navdir == AccessibleNavigation.Down) index++;
                else if (navdir == AccessibleNavigation.Previous || navdir == AccessibleNavigation.Left) index--;
                else return base.Navigate(navdir);
                return index < 0 || index >= siblings.Count ? null : new Xa(v, siblings[index]);
            }

            private static bool OwnsAx(Ax root, Ax item)
            {
                for (var current = item; current != null; current = current.Up)
                    if (Object.ReferenceEquals(current, root)) return true;
                return false;
            }

            private static string AccessibilityAction(Ax item)
            {
                if (!item.Is(Lea.Engine.Ax.Fs)) return "";
                var element = item.El;
                var command = element != null && (element.A.ContainsKey("data-lea") || element.A.ContainsKey("onclick"));
                if (item.K == "link" && (item.U != null || command)) return "Open";
                if (item.K == "button" && (command || element != null && (element.K == "button" || element.K == "input" || element.K == "summary"))) return "Press";
                if (item.K == "checkbox" && element != null && element.K == "input") return item.Is(Lea.Engine.Ax.Ck) ? "Uncheck" : "Check";
                if (item.K == "radio" && element != null && element.K == "input") return "Select";
                if ((item.K == "combobox" || item.K == "listbox") && element != null && element.K == "select") return "Open";
                return "";
            }

            private static AccessibleRole AccessibilityRole(string role)
            {
                if (role == "link") return AccessibleRole.Link;
                if (role == "button") return AccessibleRole.PushButton;
                if (role == "checkbox") return AccessibleRole.CheckButton;
                if (role == "radio") return AccessibleRole.RadioButton;
                if (role == "textbox" || role == "searchbox" || role == "spinbutton") return role == "spinbutton" ? AccessibleRole.SpinButton : AccessibleRole.Text;
                if (role == "combobox" || role == "listbox") return AccessibleRole.ComboBox;
                if (role == "slider") return AccessibleRole.Slider;
                if (role == "progressbar" || role == "meter") return AccessibleRole.ProgressBar;
                if (role == "img") return AccessibleRole.Graphic;
                if (role == "list") return AccessibleRole.List;
                if (role == "listitem") return AccessibleRole.ListItem;
                if (role == "table") return AccessibleRole.Table;
                if (role == "row") return AccessibleRole.Row;
                if (role == "cell") return AccessibleRole.Cell;
                if (role == "columnheader") return AccessibleRole.ColumnHeader;
                if (role == "rowheader") return AccessibleRole.RowHeader;
                if (role == "separator") return AccessibleRole.Separator;
                if (role == "status") return AccessibleRole.StatusBar;
                if (role == "alert") return AccessibleRole.Alert;
                if (role == "dialog") return AccessibleRole.Dialog;
                if (role == "heading") return AccessibleRole.OutlineItem;
                if (role == "text" || role == "paragraph" || role == "blockquote" || role == "code" || role == "caption" || role == "term" || role == "definition") return AccessibleRole.StaticText;
                if (role == "time" || role == "timer") return AccessibleRole.Clock;
                return AccessibleRole.Grouping;
            }

            private static AccessibleStates AccessibilityState(Ax item, Rectangle bounds)
            {
                var result = AccessibleStates.None;
                if (item.Is(Lea.Engine.Ax.Fs)) result |= AccessibleStates.Focusable;
                if (item.Is(Lea.Engine.Ax.Fc)) result |= AccessibleStates.Focused;
                if (item.Is(Lea.Engine.Ax.Ds)) result |= AccessibleStates.Unavailable;
                if (item.Is(Lea.Engine.Ax.Ck)) result |= AccessibleStates.Checked;
                if (item.Is(Lea.Engine.Ax.Mx)) result |= AccessibleStates.Mixed;
                if (item.Is(Lea.Engine.Ax.Sl)) result |= AccessibleStates.Selected;
                if (item.Is(Lea.Engine.Ax.Ex)) result |= AccessibleStates.Expanded;
                if (item.Is(Lea.Engine.Ax.Co)) result |= AccessibleStates.Collapsed;
                if (item.Is(Lea.Engine.Ax.Ro)) result |= AccessibleStates.ReadOnly;
                if (item.Is(Lea.Engine.Ax.Ps)) result |= AccessibleStates.Pressed;
                if (item.Is(Lea.Engine.Ax.Bs)) result |= AccessibleStates.Busy;
                if (item.El != null && item.El.K == "input" && item.El.Av("type").Equals("password", StringComparison.OrdinalIgnoreCase)) result |= AccessibleStates.Protected;
                if (bounds.IsEmpty) result |= AccessibleStates.Offscreen;
                return result;
            }
        }

        private sealed class Lc
        {
            public Rectangle R;
            public Rectangle C;
            public int Y;
            public Rectangle Cp;
            public readonly bool Fx;
            public B B;
            public Bx Box;
            public Lc Up;
            public bool Au;
            public bool Fl;
            public int Fy;
            public int Gp;
            public Rectangle Pc;
            public bool Co;

            public Lc(Rectangle outer, Rectangle content, int y, Rectangle clip, bool fixedPosition)
            {
                R = outer;
                C = content;
                Y = y;
                Cp = clip;
                Fx = fixedPosition;
            }
        }

        private sealed class Bx
        {
            public readonly string K;
            public readonly string X;
            public readonly Uri U;
            public readonly S S;
            public Rectangle R;
            public Rectangle C;
            public readonly bool Title;
            public readonly B B;
            public readonly bool Inline;
            public Rectangle Clip;
            public readonly N El;
            public int[] Zp;
            public bool Fx;
            public bool Fm;
            public bool Fa;

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle), N element = null)
                : this(kind, text, url, style, rect, rect, title, block, inline, clip, element)
            {
            }

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, Rectangle content, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle), N element = null)
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
                El = element ?? (block == null ? null : block.El);
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
            public readonly N El;

            public Ia(string kind, string text, Uri url, S style, string baseKind, int width, int height, int lineHeight, N element)
            {
                K = kind;
                X = text;
                U = url;
                S = style;
                Z = baseKind;
                W = width;
                H = height;
                L = lineHeight;
                El = element;
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
            public readonly N El;

            public Ix(string kind, string text, Uri url, S style, Rectangle rect, N element)
            {
                K = kind;
                X = text;
                U = url;
                S = style;
                R = rect;
                El = element;
            }
        }

        private sealed class Kx
        {
            public readonly Kt T;
            public readonly string K;

            public Kx(Kt target, string key)
            {
                T = target;
                K = key;
            }
        }

        private sealed class Kt
        {
            public readonly N El;
            public Uri U;
            public Bx B;
            public readonly int? Ti;
            public readonly int Or;
            public readonly int Sq;
            public readonly List<Kr> Rs = new List<Kr>();
            public int Sl;

            public Kt(N element, Uri url, Bx box, int? tabIndex, int order, int sequence)
            {
                El = element;
                U = url;
                B = box;
                Ti = tabIndex;
                Or = order;
                Sq = sequence;
            }

            public void Add(Rectangle rect, bool fixedPosition)
            {
                foreach (var region in Rs)
                    if (region.R == rect && region.Fx == fixedPosition) return;
                Rs.Add(new Kr(rect, fixedPosition));
            }
        }

        private sealed class Kr
        {
            public readonly Rectangle R;
            public readonly bool Fx;

            public Kr(Rectangle rect, bool fixedPosition)
            {
                R = rect;
                Fx = fixedPosition;
            }
        }

        private sealed class Hit
        {
            public readonly Rectangle R;
            public readonly Uri U;
            public readonly Bx B;
            public readonly N El;
            public readonly Ar A;
            private readonly Rectangle image;
            private readonly SizeF source;

            public Hit(Rectangle rect, Uri url, Bx box, N element = null, Ar area = null, Rectangle imageRect = default(Rectangle), SizeF sourceSize = default(SizeF))
            {
                R = rect;
                U = url;
                B = box;
                El = element ?? (box == null ? null : box.El);
                A = area;
                image = imageRect;
                source = sourceSize;
            }

            public bool Contains(Point point)
            {
                if (!R.Contains(point)) return false;
                if (A == null || A.K == "d") return true;
                if (image.Width < 1 || image.Height < 1 || source.Width <= 0 || source.Height <= 0) return false;
                var x = (point.X - image.Left) * source.Width / image.Width;
                var y = (point.Y - image.Top) * source.Height / image.Height;
                if (A.K == "r" && A.Cs.Count >= 4)
                    return x >= A.Cs[0] && x <= A.Cs[2] && y >= A.Cs[1] && y <= A.Cs[3];
                if (A.K == "c" && A.Cs.Count >= 3)
                {
                    var dx = x - A.Cs[0];
                    var dy = y - A.Cs[1];
                    return dx * dx + dy * dy <= A.Cs[2] * A.Cs[2];
                }
                if (A.K != "p" || A.Cs.Count < 6) return false;
                var inside = false;
                var last = A.Cs.Count - 2;
                for (var index = 0; index < A.Cs.Count; index += 2)
                {
                    var xi = A.Cs[index];
                    var yi = A.Cs[index + 1];
                    var xj = A.Cs[last];
                    var yj = A.Cs[last + 1];
                    if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
                    last = index;
                }
                return inside;
            }
        }
    }
}
