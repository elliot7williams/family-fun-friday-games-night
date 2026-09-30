using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
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
        private readonly Dictionary<B, Bx> boxOwners = new Dictionary<B, Bx>();
        private readonly object layoutGate = new object();
        private readonly Dictionary<string, Image> pics = new Dictionary<string, Image>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> fontNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<P, int> alerts = new Dictionary<P, int>();
        private readonly Queue<P> alertOrder = new Queue<P>();
        private readonly List<Bx> findMatches = new List<Bx>();
        private readonly Et ed;
        private static readonly object picMemoryGate = new object();
        private static long picMemoryTotal;
        private static long picMemoryLimit = 128L * 1024 * 1024;
        private static long picCombinedLimit = 384L * 1024 * 1024;
        private const int MaxKeyTargets = 256;
        private const int MaxKeyMapRegions = 4096;
        private const int MaxAccessTargets = 64;
        private const int PicLimit = 24;
        private const int PicBytes = 4 * 1024 * 1024;
        private const long PicMemory = 32L * 1024 * 1024;
        private const long PicMemoryMaximum = 512L * 1024 * 1024;
        private const long PicCombinedMemory = 128L * 1024 * 1024;
        private const long PicCombinedMaximum = 1024L * 1024 * 1024;
        private const int FontLimit = 32;
        private const int UploadFiles = 8;
        private const int UploadFileBytes = 4 * 1024 * 1024;
        private const int UploadBytes = 8 * 1024 * 1024;
        private const int MaxAutomationTableRows = 256;
        private const int MaxAutomationTableColumns = 256;
        private const int MaxAutomationTableCells = 512;
        private const int MaxAutomationTextChars = 32768;
        private const int MaxAutomationTextSegments = 2048;
        private const int MaxAutomationTextChildren = 64;
        private const int BalanceLineLimit = 8;
        private const int BalanceTextLimit = 2048;
        private const int MaxContainerSizes = 256;
        private const int MaxContainerPasses = 3;
        private int sy;
        private int docHeight;
        private int layoutBottom;
        private int layoutWidth = -1;
        private int layoutViewportWidth = 1;
        private int layoutViewportHeight = 1;
        private int layoutRootFontSize = 10;
        private bool layoutDirty = true;
        private bool densityExplicit;
        private bool themeExplicit;
        private bool motionExplicit;
        private bool contrastExplicit;
        private bool interactionExplicit;
        private bool sticky;
        private bool picBusy;
        private bool picStop;
        private Task picWork;
        private P picPage;
        private int picVersion;
        private long picSize;
        private int picNative;
        private int picWorker;
        private int picKilled;
        private int picJob;
        private int picRestricted;
        private int picLow;
        private int picSuspended;
        private int picHandleList;
        private int picAslr;
        private int picCpu;
        private int picMitigations;
        private int picAppContainer;
        private int picNoNetwork;
        private int picFileAcl;
        private int picAdmission;
        private long picHostMemory;
        private long picCombinedMemory;
        private long picWorkerMemory;
        private long picWorkerPeak;
        private string picWorkerPath;
        private string picWorkerArgs = "--pipe";
        private Func<long> picHostMemoryProbe = HostPrivateMemory;
        private WebRequest picRequest;
        private Stream picStream;
        private CancellationTokenSource picBudget;
        private object loadOwner;
        private bool closed;
        private Fi edit;
        private Fi heldEdit;
        private bool editRedo;
        private bool editReplay;
        private Font editFont;
        private S editStyle;
        private string editStart = "";
        private bool editLock;
        private ContextMenuStrip choices;
        private N hover;
        private N focus;
        private int keySlot = -1;
        private string findQuery = "";
        private int findIndex = -1;
        private P fragmentPage;
        private string fragmentAddress = "";
        private bool fragmentAnchor;
        private bool fragmentPending;
        private int fragmentJumps;
        private int fragmentY = -1;
        private int goRequests;
        private Ur ur;
        private Uri retryTarget;

        public event EventHandler Done;
        public event EventHandler Changed;
        public event EventHandler Ed;
        public event EventHandler Sb;
        public event EventHandler Al;
        public event EventHandler Dr;
        public event EventHandler Gr;
        public event EventHandler Fe;

        public V()
        {
            ed = new Et(this);
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
            ed.LostFocus += delegate { EndEdit(false, true); };
            ed.MouseWheel += delegate(object sender, MouseEventArgs args) { Sc(-Math.Sign(args.Delta) * 72); };
            Controls.Add(ed);
            e.Vz(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new Va(this);
        }

        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaReturnRawElementProvider", ExactSpelling = true)]
        private static extern IntPtr ReleaseProvider(IntPtr window, IntPtr wParam, IntPtr lParam, IntPtr provider);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
        private static extern IntPtr EditMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr EditTextMessage(IntPtr window, int message, IntPtr wParam, string text);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool OpenClipboard(IntPtr owner);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint action, uint parameter,
            [MarshalAs(UnmanagedType.Bool)] out bool value, uint flags);
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int GetSystemMetrics(int index);
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool CloseClipboard();
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetClipboardData(uint format);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern UIntPtr GlobalSize(IntPtr memory);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GlobalLock(IntPtr memory);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern bool GlobalUnlock(IntPtr memory);

        private sealed class Et : TextBox
        {
            private readonly V view;
            public Et(V view) { this.view = view; }
            protected override void WndProc(ref Message message)
            {
                if (message.Msg == 0x0302) // WM_PASTE also covers the native context menu.
                {
                    message.Result = new IntPtr(view.Ec("pa"));
                    return;
                }
                base.WndProc(ref message);
            }
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == 0x02E0 && !densityExplicit)
            {
                var density = (int)(message.WParam.ToInt64() & 0xffffL);
                if (density > 0) SetDensity(density, false);
            }
            if (message.Msg == 0x001A)
            {
                if (!themeExplicit) SetTheme(SystemTheme(), false);
                if (!motionExplicit) SetMotion(SystemMotion(), false);
                if (!contrastExplicit) SetContrast(SystemContrast(), false);
            }
            if (message.Msg == 0x0219 && !interactionExplicit) RefreshInteraction(false);
            if (message.Msg == 0x0002)
            {
                // Release UI Automation's native provider event-map references.
                ReleaseProvider(message.HWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            if (message.Msg == 0x003D && message.LParam.ToInt64() == AutomationInteropProvider.RootObjectId)
            {
                if (closed) { message.Result = IntPtr.Zero; return; }
                message.Result = AutomationInteropProvider.ReturnRawElementProvider(Handle, message.WParam, message.LParam, UiRoot);
                return;
            }
            base.WndProc(ref message);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!densityExplicit)
                using (var graphics = CreateGraphics()) SetDensity((int)Math.Round(graphics.DpiX), false);
            if (!themeExplicit) SetTheme(SystemTheme(), false);
            if (!motionExplicit) SetMotion(SystemMotion(), false);
            if (!contrastExplicit) SetContrast(SystemContrast(), false);
            if (!interactionExplicit) RefreshInteraction(false);
        }

        private Ur UiRoot { get { return ur ?? (ur = new Ur(this)); } }

        public P At { get { return e.At; } }
        public Uri U { get { return At == null ? null : At.U; } }
        public Uri Bu { get { return At == null ? null : At.Bu; } }
        public string T { get { return At == null ? "" : At.T; } }
        public string H { get { return At == null ? "" : At.H; } }
        public string Q { get { return At == null ? "" : At.Q; } }
        public bool CanBk { get { return e.CanBk; } }
        public bool CanFw { get { return e.CanFw; } }
        public bool Busy { get { return loadOwner != null || e.Busy || picBusy; } }
        public bool Cn { get { return !closed && loadOwner == null && !e.Busy; } }
        public long Ib { get { return picSize; } }
        public long Im
        {
            get { lock (picMemoryGate) return picMemoryLimit; }
            set { lock (picMemoryGate) picMemoryLimit = Math.Max(PicMemory, Math.Min(PicMemoryMaximum, value)); }
        }
        public long It { get { lock (picMemoryGate) return picMemoryTotal; } }
        public int Ip { get { return Pi.Pixels; } }
        public int In { get { return Interlocked.CompareExchange(ref picNative, 0, 0); } }
        public string Ie { get { return Di.P(picWorkerPath); } set { picWorkerPath = value; } }
        public bool Ii { get { return File.Exists(Ie); } }
        public int Iw { get { return Interlocked.CompareExchange(ref picWorker, 0, 0); } }
        public int Ik { get { return Interlocked.CompareExchange(ref picKilled, 0, 0); } }
        public bool Ij { get { return Interlocked.CompareExchange(ref picJob, 0, 0) != 0; } }
        public long Ih { get { return Di.L; } set { Di.L = value; } }
        public long Ipm { get { return Interlocked.Read(ref picWorkerMemory); } }
        public bool Irt { get { return Interlocked.CompareExchange(ref picRestricted, 0, 0) != 0; } }
        public bool Ili { get { return Interlocked.CompareExchange(ref picLow, 0, 0) != 0; } }
        public bool Isp { get { return Interlocked.CompareExchange(ref picSuspended, 0, 0) != 0; } }
        public bool Ihl { get { return Interlocked.CompareExchange(ref picHandleList, 0, 0) != 0; } }
        public bool Ias { get { return Interlocked.CompareExchange(ref picAslr, 0, 0) != 0; } }
        public bool Icp { get { return Interlocked.CompareExchange(ref picCpu, 0, 0) != 0; } }
        public int Icr { get { return Di.C; } set { Di.C = value; } }
        public int Img { get { return Interlocked.CompareExchange(ref picMitigations, 0, 0); } }
        public long Ipk { get { return Interlocked.Read(ref picWorkerPeak); } }
        public bool Iac { get { return Interlocked.CompareExchange(ref picAppContainer, 0, 0) != 0; } }
        public bool Ind { get { return Interlocked.CompareExchange(ref picNoNetwork, 0, 0) != 0; } }
        public bool Ial { get { return Interlocked.CompareExchange(ref picFileAcl, 0, 0) != 0; } }
        public long Icl
        {
            get { lock (picMemoryGate) return picCombinedLimit; }
            set { lock (picMemoryGate) picCombinedLimit = Math.Max(PicCombinedMemory, Math.Min(PicCombinedMaximum, value)); }
        }
        public long Ihm { get { return Interlocked.Read(ref picHostMemory); } }
        public long Icm { get { return Interlocked.Read(ref picCombinedMemory); } }
        public bool Iad { get { return Interlocked.CompareExchange(ref picAdmission, 0, 0) != 0; } }
        public int Hm { get { return e.Hm; } }
        public int Csm { get { return e.Csm; } }
        public int Ic { get { var count = 0; foreach (var item in pics.Values) if (item != null) count++; return count; } }
        public int If { get { var count = 0; foreach (var item in pics.Values) if (item == null) count++; return count; } }
        public bool Ud { get { return closed || e.Ud(At); } }
        public int Sn { get { return At == null ? 0 : At.Sn; } }
        public int Cm { get { return e.Cm; } set { e.Cm = value; } }
        public int Cc { get { return e.Cc; } }
        public int Ch { get { return e.Ch; } }
        public int Cs { get { return e.Cs; } set { e.Cs = value; } }
        public string Cd { get { return e.Cd; } set { e.Cd = value; } }
        public int Cx { get { return e.Cx; } set { e.Cx = value; } }
        public long Cb { get { return e.Cb; } set { e.Cb = value; } }
        public int Dc { get { return e.Dc; } }
        public long Db { get { return e.Db; } }
        public int Kc { get { return e.Kc; } }
        public int Km { get { return e.Km; } }
        public long Dm { get { return e.Dm; } }
        public int Tm { get { return e.Tm; } set { e.Tm = value; } }
        public bool Pv { get { return e.Pv; } set { e.Pv = value; } }
        public bool Je { get { return e.Je; } set { SetScripting(value); } }
        private Func<Uri, bool> navigationGuard;
        public Func<Uri, bool> Ng
        {
            get { return navigationGuard; }
            set
            {
                if (value != null) CheckOpen();
                navigationGuard = value;
                e.Ng = value == null ? null : (Func<Uri, bool>)AllowNavigation;
            }
        }

        private bool AllowNavigation(Uri url)
        {
            var guard = navigationGuard;
            return !closed && guard != null && guard(url) && !closed;
        }
        public string Am { get; private set; }
        public Er Le { get; private set; }
        public bool Cr { get { return !closed && !Busy && retryTarget != null; } }
        public string Fq { get { return findQuery; } }
        public int Fn { get { return findMatches.Count; } }
        public int Fx { get { return findIndex < 0 ? 0 : findIndex + 1; } }
        public Uri Du { get; private set; }
        public string Dn { get; private set; }
        public Uri Gu { get; private set; }
        public D Ld { get; private set; }
        public int Dh { get { return docHeight; } }
        public int Sy { get { return sy; } }
        public int Bc { get { lock (layoutGate) return boxes.Count; } }
        public int Vw { get { return e.Vw; } }
        public int Vh { get { return e.Vh; } }
        public int Dp { get { return e.Dp; } set { SetDensity(value, true); } }
        public char Th { get { return e.Th; } set { SetTheme(value, true); } }
        public char Rm { get { return e.Rm; } set { SetMotion(value, true); } }
        public char Pc { get { return e.Pc; } set { SetContrast(value, true); } }
        public char Pdt { get { return e.Pdt; } set { SetPreferredData(value); } }
        public char Pt { get { return e.Pt; } set { SetInteraction(value, e.Hv, e.Ap, e.Ah, true); } }
        public bool Hv { get { return e.Hv; } set { SetInteraction(e.Pt, value, e.Ap, e.Ah, true); } }
        public char Ap { get { return e.Ap; } set { SetInteraction(e.Pt, e.Hv, value, e.Ah, true); } }
        public bool Ah { get { return e.Ah; } set { SetInteraction(e.Pt, e.Hv, e.Ap, value, true); } }
        public void Mi(char pointer, bool hover, char anyPointer, bool anyHover)
        {
            SetInteraction(pointer, hover, anyPointer, anyHover, true);
        }
        public int Rf { get { return At != null && At.Rf > 0 ? At.Rf : Math.Max(1, (int)Math.Round(Font.Size)); } }
        public N Ke { get { return focus; } }
        public int Kn { get { return FocusTargets(true).Count; } }
        public int Ki { get { return FocusIndex(FocusTargets(true)) + 1; } }
        public Rectangle Kb { get { return FocusBounds(CurrentFocusTarget()); } }
        public int An { get { return AccessTargets().Count; } }
        public IList<Ax> Axs { get { return At == null ? new List<Ax>().AsReadOnly() : At.Axs; } }
        public AccessibleObject Ao { get { return AccessibilityObject ?? new Va(this); } }
        public Rectangle Rb(int index) { lock (layoutGate) return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].R; }
        public Rectangle Rc(int index) { lock (layoutGate) return index < 0 || index >= boxes.Count ? Rectangle.Empty : boxes[index].C; }
        public Rectangle Fr { get { return FindRect(); } }
        public bool Fa { get { return fragmentAnchor; } }
        public int Fj { get { return fragmentJumps; } }
        public int Fy { get { return fragmentY; } }

        // Compact commands act only on this page's current or menu-retained editor.
        public bool Ce(string code)
        {
            var field = edit ?? heldEdit;
            if (closed || code == null || code.Length != 2 || At == null || field == null ||
                !At.Fs.Contains(field) || field.Ds || !CanEdit(field) || field.V != ed.Text) return false;
            EnsureLayout();
            var visible = false;
            foreach (var box in BoxSnapshot())
                if (box.B != null && box.B.I == field && !Hidden(box.S) && !HtmlHidden(field.El) && !KeyRect(box).IsEmpty)
                { visible = true; break; }
            if (!visible) return false;
            switch (code.ToLowerInvariant())
            {
                case "sa": return ed.TextLength > 0;
                case "cp": return field.K != "password" && ed.SelectionLength > 0;
                case "cu": return !field.Ro && field.K != "password" && ed.SelectionLength > 0;
                case "pa": return !field.Ro;
                case "de": return !field.Ro && (ed.SelectionLength > 0 || ed.SelectionStart < ed.TextLength);
                case "un": return !field.Ro && ed.CanUndo && !editRedo;
                case "re": return !field.Ro && ed.CanUndo && editRedo;
                default: return false;
            }
        }

        public int Ec(string code)
        {
            if (!ReadyEdit(code)) return 0;
            switch (code.ToLowerInvariant())
            {
                case "sa": ed.SelectAll(); break;
                case "cp": return CopyEdit(false);
                case "cu": return CopyEdit(true);
                case "pa": return PasteEdit();
                case "de":
                    var text = ed.Text;
                    var start = ed.SelectionStart;
                    var end = Math.Min(text.Length, start + Math.Max(1, ed.SelectionLength));
                    if (start > 0 && start < text.Length && Char.IsLowSurrogate(text[start]) && Char.IsHighSurrogate(text[start - 1])) start--;
                    if (end > 0 && end < text.Length && Char.IsHighSurrogate(text[end - 1]) && Char.IsLowSurrogate(text[end])) end++;
                    ed.Select(start, end - start);
                    EditMessage(ed.Handle, 0x0303, IntPtr.Zero, IntPtr.Zero); // WM_CLEAR preserves native undo.
                    break;
                case "un":
                case "re":
                    editReplay = true;
                    try { ed.Undo(); editRedo = !editRedo; }
                    finally { editReplay = false; }
                    break;
            }
            return 1;
        }

        public int Pa(string text)
        {
            return String.IsNullOrEmpty(text) || !ReadyEdit("pa") ? 0 : PasteText(text);
        }

        private bool ReadyEdit(string code)
        {
            if (editLock || !Ce(code)) return false;
            var field = edit ?? heldEdit;
            edit = field;
            heldEdit = null;
            SetFocus(field.El);
            PlaceEditor();
            if (!ed.Visible) { EndEdit(false); return false; }
            ed.Focus();
            return !closed && edit == field && Ce(code);
        }

        private int CopyEdit(bool cut)
        {
            var field = edit;
            if (field == null || editLock || ed.SelectionLength < 1) return 0;
            var page = At;
            var before = ed.Text;
            var start = ed.SelectionStart;
            var selected = ed.SelectionLength;
            var text = ed.SelectedText;
            var data = new DataObject();
            data.SetText(text, TextDataFormat.UnicodeText);
            try { Clipboard.SetDataObject(data, true, 10, 20); }
            catch (ExternalException) { return 0; }
            if (closed || At != page || edit != field || ed.Text != before || ed.SelectionStart != start ||
                ed.SelectionLength != selected || !Ce(cut ? "cu" : "cp")) return 0;
            if (cut) EditMessage(ed.Handle, 0x0303, IntPtr.Zero, IntPtr.Zero);
            return 1;
        }

        private int PasteEdit()
        {
            var owner = ed.Handle;
            return PasteEdit(() => ReadPasteText(owner));
        }

        private int PasteEdit(Func<string> read)
        {
            var field = edit;
            if (field == null || editLock) return 0;
            var page = At;
            var before = ed.Text;
            var start = ed.SelectionStart;
            var selected = ed.SelectionLength;
            var text = read();
            // Clipboard owners can render data synchronously; recheck after that call.
            if (String.IsNullOrEmpty(text) || closed || At != page || edit != field || ed.Text != before ||
                ed.SelectionStart != start || ed.SelectionLength != selected || !Ce("pa")) return 0;
            return PasteText(text);
        }

        private int PasteText(string text)
        {
            var length = Math.Min(text.Length, Math.Max(0, Math.Min(32767, ed.MaxLength) - (ed.TextLength - ed.SelectionLength)));
            for (var index = 0; index < length; index++)
                if (text[index] == '\0' || (!ed.Multiline && (text[index] == '\r' || text[index] == '\n'))) { length = index; break; }
            if (length > 0 && Char.IsHighSurrogate(text[length - 1])) length--;
            if (length > 0 && length < text.Length && text[length - 1] == '\r' && text[length] == '\n') length--;
            if (length == 0) return 0;
            // EM_REPLACESEL preserves native undo without touching the control after callbacks.
            EditTextMessage(ed.Handle, 0x00C2, new IntPtr(1), length == text.Length ? text : text.Substring(0, length));
            return 1;
        }

        private static string ReadPasteText(IntPtr owner)
        {
            var opened = false;
            for (var attempt = 0; attempt < 3 && !opened; attempt++)
            {
                if (attempt > 0) System.Threading.Thread.Sleep(5);
                opened = OpenClipboard(owner);
            }
            if (!opened) return null;
            try
            {
                var memory = GetClipboardData(13); // CF_UNICODETEXT includes Windows' text-format conversion.
                if (memory == IntPtr.Zero) return null;
                var count = (int)Math.Min(32768UL, GlobalSize(memory).ToUInt64() / 2);
                if (count == 0) return null;
                var pointer = GlobalLock(memory);
                if (pointer == IntPtr.Zero) return null;
                try
                {
                    var text = Marshal.PtrToStringUni(pointer, count);
                    var end = text.IndexOf('\0');
                    return end < 0 ? text : text.Substring(0, end);
                }
                finally { GlobalUnlock(memory); }
            }
            finally { CloseClipboard(); }
        }

        public P Go(string input)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try
            {
                if (Fg(input) != 0) return At;
                return Show(LoadCore(() => e.Go(input), true), true);
            }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public P Go(Uri url)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try
            {
                if (Fg(url) != 0) return At;
                return Show(LoadCore(() => e.Go(url), true), true);
            }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public P Po(Uri url, string data)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try { return Show(LoadCore(() => e.Po(url, data), false), true); }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public P Pb(Uri url, byte[] data, string type)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try { return Show(LoadCore(() => e.Pb(url, data, type), false), true); }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public async Task<P> GoA(string input)
        {
            if (Fg(input) != 0) return At;
            return await ShowA(() => e.GoA(input), true);
        }

        public async Task<P> GoA(Uri url)
        {
            if (Fg(url) != 0) return At;
            return await ShowA(() => e.GoA(url), true);
        }

        public Task<P> PoA(Uri url, string data)
        {
            return ShowA(() => e.PoA(url, data), false);
        }

        public Task<P> PbA(Uri url, byte[] data, string type)
        {
            return ShowA(() => e.PbA(url, data, type), false);
        }

        public P Bk() { CheckOpen(); return Show(LoadCore(e.Bk, false), false); }
        public P Fw() { CheckOpen(); return Show(LoadCore(e.Fw, false), false); }
        public P Rl() { CheckOpen(); return Show(LoadCore(e.Rl, true), true); }

        public Task<P> RlA()
        {
            return ShowA(e.RlA, true);
        }

        public Task<P> Ry()
        {
            CheckOpen();
            var target = Cr ? retryTarget : null;
            return target == null ? Task.FromResult(At) : GoA(target);
        }

        private T LoadCore<T>(Func<T> load, bool retry, bool clear = true)
        {
            CheckIdle();
            if (clear) ClearFailure();
            try { return load(); }
            catch (Er error) { LoadFailed(error, retry); throw; }
        }

        private void CheckIdle()
        {
            CheckOpen();
            if (loadOwner != null || e.Busy) throw new LoadBusy();
        }

        private object StartLoad()
        {
            CheckIdle();
            var owner = new object();
            loadOwner = owner;
            try { Cursor = Cursors.WaitCursor; CheckOpen(); }
            catch { FinishLoad(owner); throw; }
            return owner;
        }

        private void FinishLoad(object owner)
        {
            if (!Object.ReferenceEquals(loadOwner, owner)) return;
            loadOwner = null;
            if (!closed) Cursor = Cursors.Default;
        }

        private void ClearFailure()
        {
            Le = null;
            retryTarget = null;
        }

        private void LoadFailed(Er error, bool retry)
        {
            if (closed) return;
            if (InvokeRequired)
            {
                try { Invoke(new Action(() => LoadFailed(error, retry))); }
                catch (InvalidOperationException) { if (!closed) throw; }
                return;
            }
            if (error.K == "cancel") { ClearFailure(); return; }
            Le = error;
            var url = error.U;
            retryTarget = retry && error.K != "url" && url != null && url.IsAbsoluteUri &&
                (url.Scheme == "http" || url.Scheme == "https" || url.Scheme == "file" || url.Scheme == "about") ? url : null;
            var handler = Fe;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private async Task<P> ShowA(Func<Task<P>> load, bool retry)
        {
            var owner = StartLoad();
            ClearFailure();
            picStop = false;
            try
            {
                P page;
                try { page = await load(); }
                catch (Er error) { FinishLoad(owner); LoadFailed(error, retry); throw; }
                if (closed) return page;
                var stoppedPictures = picStop;
                Show(page, false);
                picStop |= stoppedPictures;
                if (closed) return page;
                await PicsA(page);
                if (closed) return page;
                Dirty();
                QueueFragment(page);
                ApplyPendingFragment();
                Invalidate();
                FinishLoad(owner);
                var address = page == null ? null : page.U;
                if (Done != null) Done(this, EventArgs.Empty);
                if (!closed && loadOwner == null && At == page && (page == null || page.U == address)) GoRequest(page);
                return page;
            }
            finally { FinishLoad(owner); }
        }

        private sealed class LoadBusy : InvalidOperationException
        {
            public LoadBusy() : base("Lea is still loading or downloading. Stop and wait for completion before navigating again.") { }
        }

        public void St()
        {
            if (closed) return;
            picStop = true;
            e.St();
            CancelPics();
            Cursor = Cursors.Default;
        }

        private void CheckOpen()
        {
            if (closed) throw new ObjectDisposedException("Lea.Win.V");
        }

        public void Cl() { e.Cl(); }
        public void Cdl() { e.Cdl(); }
        public void Kcl() { e.Kcl(); }

        public int Js(string code)
        {
            if (At == null) return 0;
            return Commands(e.Js(At, code));
        }

        public bool Su(string condition) { return e.Su(condition); }
        public bool Su(string property, string value) { return e.Su(property, value); }
        public bool Mq(string query) { return e.Mq(query); }

        public int Ss(string key, string source)
        {
            if (closed || At == null) return 0;
            var page = At;
            var style = page.S;
            var sheets = page.Sn;
            var count = e.Ss(page, key, source);
            return count == 0 || style == page.S && sheets == page.Sn ? count : SheetChanged(count);
        }

        public int Sd(string key)
        {
            return closed || At == null ? 0 : SheetChanged(e.Sd(At, key));
        }

        private int SheetChanged(int count)
        {
            if (count == 0) return 0;
            Dirty();
            QueueFragment(At);
            KeepValidFocus();
            EnsureLayout();
            ApplyPendingFragment();
            PlaceEditor();
            Invalidate();
            OnChanged();
            if (!closed) NotifyAccessibility(AccessibleEvents.Reorder);
            return count;
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
            foreach (var box in BoxSnapshot())
            {
                if (box.Title || box.El == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                if (item.El != null && (OwnsElement(item.El, box.El) || item.El.K == "#text" && Object.ReferenceEquals(item.El.Up, box.El)))
                    AddAccessibleBounds(ref result, box.R, box.Clip, box.Fx);
                if (box.B == null || box.B.Mp == null || item.El == null || item.El.K != "area") continue;
                var source = MapSourceSize(box.B, box.C);
                var image = PicObjectRect(box.B, box.C);
                var visible = Rectangle.Intersect(box.C, image);
                foreach (var area in box.B.Mp.As)
                {
                    if (!Object.ReferenceEquals(area.El, item.El)) continue;
                    var rect = Rectangle.Intersect(visible, MapBounds(area, image, source));
                    AddAccessibleBounds(ref result, rect, box.Clip, box.Fx);
                }
            }
            return result;
        }

        public int Fg(string input)
        {
            var old = sy;
            if (LoadCore(() => e.Fg(input), true, false) == 0) return 0;
            Show(At, true, old);
            return 1;
        }

        public int Fg(Uri url)
        {
            var old = sy;
            if (LoadCore(() => e.Fg(url), true, false) == 0) return 0;
            Show(At, true, old);
            return 1;
        }

        private int Commands(int count)
        {
            if (count == 0) return 0;
            ClearFind(false);
            Dirty();
            QueueFragment(At);
            if (!ApplyCommandFocus(At)) KeepValidFocus();
            ApplyPendingFragment();
            OnChanged();
            if (closed) return count;
            Invalidate();
            NotifyAccessibility(AccessibleEvents.Reorder);
            Alerts(At);
            GoRequest(At);
            return count;
        }

        public D Dl(string input, string path)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try { return Ld = e.Dl(input, path); }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public D Dl(Uri url, string path)
        {
            CheckIdle();
            Cursor = Cursors.WaitCursor;
            try { return Ld = e.Dl(url, path); }
            finally { if (!closed && loadOwner == null) Cursor = Cursors.Default; }
        }

        public async Task<D> DlA(string input, string path)
        {
            var owner = StartLoad();
            try { return Ld = await e.DlA(input, path); }
            finally { FinishLoad(owner); }
        }

        public async Task<D> DlA(Uri url, string path)
        {
            var owner = StartLoad();
            try { return Ld = await e.DlA(url, path); }
            finally { FinishLoad(owner); }
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
            if (closed) return page;
            if (picPage != null && picPage != page) CancelPics();
            TrimPics(PicSources(page), preserveScroll < 0);
            ClearFailure();
            picStop = false;
            CloseChoices();
            ClearFind(false);
            hover = null;
            focus = null;
            keySlot = -1;
            EndEdit(false);
            e.Vz(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));
            e.Dy(page, null, null);
            BeginFragment(page);
            sy = 0;
            Dirty();
            var scrolled = ScrollFragment(page);
            if (scrolled && fragmentAnchor) fragmentPending = true;
            var commandFocus = ApplyCommandFocus(page);
            if (!scrolled && !commandFocus) ApplyAutofocus(page);
            if (!scrolled && preserveScroll >= 0 && page != null && page.U != null && page.U.Fragment.Length > 0)
                sy = Math.Max(0, Math.Min(MaxScroll(), preserveScroll));
            OnChanged();
            if (closed) return page;
            Invalidate();
            NotifyAccessibility(AccessibleEvents.Reorder);
            Alerts(page);
            var address = page == null ? null : page.U;
            if (!closed && done && Done != null) Done(this, EventArgs.Empty);
            if (!closed && done && loadOwner == null && At == page && (page == null || page.U == address)) GoRequest(page);
            return page;
        }

        private bool ScrollFragment(P page)
        {
            if (page == null || page.U == null || page.U.Fragment.Length == 0) return false;
            fragmentJumps = Math.Min(255, fragmentJumps + 1);
            fragmentY = -1;
            if (page.U.Fragment == "#")
            {
                fragmentY = 0;
                SetScroll(0, true);
                PlaceEditor();
                return true;
            }
            var target = e.Ft(page, page.U.Fragment);
            if (target == null) { CancelFragment(); return false; }
            if (e.Rv(page, target) > 0) Dirty();
            EnsureLayout();
            var box = FragmentBox(target);
            if (box == null) { CancelFragment(); return false; }
            fragmentY = box.C.Top;
            if (!box.Fx) SetScroll(box.C.Top - 12, true);
            PlaceEditor();
            return true;
        }

        private void BeginFragment(P page)
        {
            fragmentPage = page;
            fragmentAddress = page == null || page.U == null ? "" : page.U.Fragment;
            fragmentAnchor = fragmentAddress.Length > 0;
            fragmentPending = false;
            fragmentJumps = 0;
            fragmentY = -1;
        }

        private void QueueFragment(P page)
        {
            if (!fragmentAnchor || page == null || !Object.ReferenceEquals(page, fragmentPage) || page.U == null ||
                !String.Equals(page.U.Fragment, fragmentAddress, StringComparison.Ordinal))
            {
                CancelFragment();
                return;
            }
            fragmentPending = true;
        }

        private void ApplyPendingFragment()
        {
            if (!fragmentPending || !fragmentAnchor || !Object.ReferenceEquals(At, fragmentPage)) return;
            fragmentPending = false;
            ScrollFragment(fragmentPage);
        }

        private void CancelFragment()
        {
            fragmentAnchor = false;
            fragmentPending = false;
            fragmentPage = null;
            fragmentAddress = "";
        }

        private Bx FragmentBox(N target)
        {
            Bx exactInline = null;
            Bx descendant = null;
            foreach (var box in BoxSnapshot())
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
            if (closed || page == null) return false;
            var target = e.Gt(page);
            if (target == null) return false;
            Gu = target;
            goRequests++;
            if (Gr != null) Gr(this, EventArgs.Empty);
            return true;
        }

        private void Alerts(P page)
        {
            if (closed || page == null || page.Ms.Count == 0) return;
            int shown;
            if (!alerts.TryGetValue(page, out shown))
            {
                shown = 0;
                alerts[page] = 0;
                alertOrder.Enqueue(page);
                while (alertOrder.Count > 32) alerts.Remove(alertOrder.Dequeue());
            }
            while (!closed && alerts.TryGetValue(page, out shown) && shown < page.Ms.Count)
            {
                Am = page.Ms[shown];
                alerts[page] = shown + 1;
                if (Al != null) Al(this, EventArgs.Empty);
            }
        }

        private void EnsureLayout()
        {
            if (closed || At == null || (!layoutDirty && layoutWidth == ClientSize.Width)) return;
            if (!IsHandleCreated) CreateControl();
            using (var graphics = CreateGraphics()) Lay(graphics);
        }

        private void RebuildFind(int wanted)
        {
            foreach (var match in findMatches) { match.Fm = false; match.Fa = false; }
            findMatches.Clear();
            if (findQuery.Length > 0)
                foreach (var box in BoxSnapshot())
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
                SetScroll(match.C.Top - 24);
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

        private void SetDensity(int value, bool explicitlySet)
        {
            if (explicitlySet) densityExplicit = true;
            var previous = e.Dp;
            e.Dp = value;
            if (previous == e.Dp) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private void SetTheme(char value, bool explicitlySet)
        {
            var next = Char.ToLowerInvariant(value);
            if (next != 'l' && next != 'd') return;
            if (explicitlySet) themeExplicit = true;
            var previous = e.Th;
            e.Th = next;
            if (previous == e.Th) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private static char SystemTheme()
        {
            try
            {
                var value = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme", 1);
                return Convert.ToInt32(value, CultureInfo.InvariantCulture) == 0 ? 'd' : 'l';
            }
            catch { return 'l'; }
        }

        private void SetMotion(char value, bool explicitlySet)
        {
            var next = Char.ToLowerInvariant(value);
            if (next != 'n' && next != 'r') return;
            if (explicitlySet) motionExplicit = true;
            var previous = e.Rm;
            e.Rm = next;
            if (previous == e.Rm) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private static char SystemMotion()
        {
            try
            {
                bool enabled;
                return SystemParametersInfo(0x1042, 0, out enabled, 0) && !enabled ? 'r' : 'n';
            }
            catch { return 'n'; }
        }

        private void SetContrast(char value, bool explicitlySet)
        {
            var next = Char.ToLowerInvariant(value);
            if (next != 'n' && next != 'l' && next != 'm' && next != 'c') return;
            if (explicitlySet) contrastExplicit = true;
            var previous = e.Pc;
            e.Pc = next;
            if (previous == e.Pc) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private static char SystemContrast()
        {
            try { return SystemInformation.HighContrast ? 'c' : 'n'; }
            catch { return 'n'; }
        }

        private void SetPreferredData(char value)
        {
            var next = Char.ToLowerInvariant(value);
            if (next != 'n' && next != 'r') return;
            var previous = e.Pdt;
            e.Pdt = next;
            if (previous == e.Pdt) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private void RefreshInteraction(bool explicitlySet)
        {
            char pointer;
            bool hover;
            char anyPointer;
            bool anyHover;
            SystemInteraction(out pointer, out hover, out anyPointer, out anyHover);
            SetInteraction(pointer, hover, anyPointer, anyHover, explicitlySet);
        }

        private void SetInteraction(char pointer, bool hover, char anyPointer, bool anyHover, bool explicitlySet)
        {
            var nextPointer = Char.ToLowerInvariant(pointer);
            var nextAnyPointer = Char.ToLowerInvariant(anyPointer);
            if (nextPointer != 'n' && nextPointer != 'c' && nextPointer != 'f' ||
                nextAnyPointer != 'n' && nextAnyPointer != 'c' && nextAnyPointer != 'f' && nextAnyPointer != 'b') return;
            if (explicitlySet) interactionExplicit = true;
            var previousPointer = e.Pt;
            var previousHover = e.Hv;
            var previousAnyPointer = e.Ap;
            var previousAnyHover = e.Ah;
            e.Mi(nextPointer, hover, nextAnyPointer, anyHover);
            if (previousPointer == e.Pt && previousHover == e.Hv && previousAnyPointer == e.Ap && previousAnyHover == e.Ah) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private static void SystemInteraction(out char pointer, out bool hover, out char anyPointer, out bool anyHover)
        {
            try
            {
                var digitizer = GetSystemMetrics(94);
                var touch = GetSystemMetrics(95) > 0 || (digitizer & 0x03) != 0;
                var pen = (digitizer & 0x0c) != 0;
                var mouse = SystemInformation.MousePresent;
                var fine = mouse || pen;
                pointer = mouse ? 'f' : touch ? 'c' : pen ? 'f' : 'n';
                hover = mouse;
                anyPointer = touch && fine ? 'b' : touch ? 'c' : fine ? 'f' : 'n';
                anyHover = mouse;
            }
            catch
            {
                pointer = 'f';
                hover = true;
                anyPointer = 'f';
                anyHover = true;
            }
        }

        private void SetScripting(bool value)
        {
            var previous = e.Je;
            e.Je = value;
            if (previous == e.Je) return;
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (At != null && IsHandleCreated) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        private Bx[] BoxSnapshot()
        {
            lock (layoutGate) return boxes.ToArray();
        }

        private Bx[] PaintSnapshot(out IDictionary<B, Bx> owners)
        {
            lock (layoutGate)
            {
                owners = new Dictionary<B, Bx>(boxOwners);
                return boxes.ToArray();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            var width = Math.Max(1, ClientSize.Width);
            var height = Math.Max(1, ClientSize.Height);
            var changed = this.e.Vw != width || this.e.Vh != height;
            this.e.Vz(width, height);
            Dirty();
            QueueFragment(At);
            Invalidate();
            if (changed && At != null) NotifyAccessibility(AccessibleEvents.Reorder);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (fontNames != null) fontNames.Clear();
            editStyle = null;
            Dirty();
            QueueFragment(At);
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

            QueuePics(At);

            var pageColor = CssColor(At.S == null ? "" : At.S.F, ForeColor);
            var pageBackground = CssColor(At.S == null ? "" : At.S.Bg, BackColor);
            pe.Graphics.Clear(BackColor);
            if (At.S != null && !Hidden(At.S) && At.S.Bg.Length > 0 && At.S.Op > 0)
                using (var brush = new SolidBrush(PaintColor(pageBackground, At.S.Op))) pe.Graphics.FillRectangle(brush, ClientRectangle);
            if (At.S != null && !Hidden(At.S) && At.S.Bi != null && At.S.Op > 0)
                PaintBitmapBackground(pe.Graphics, ClientRectangle, At.S, At.S.Op);
            if (At.S != null && !Hidden(At.S) && At.S.Gi != null && At.S.Op > 0)
                PaintGradient(pe.Graphics, ClientRectangle, At.S, pageColor, At.S.Op);
            if (layoutDirty || layoutWidth != ClientSize.Width) Lay(pe.Graphics);
            ApplyPendingFragment();
            PlaceEditor();

            IDictionary<B, Bx> paintOwners;
            var paintBoxes = PaintSnapshot(out paintOwners);
            foreach (var box in paintBoxes)
            {
                var rect = box.R;
                var content = box.C;
                if (!box.Fx)
                {
                    rect.Offset(0, -sy);
                    content.Offset(0, -sy);
                }
                var outlineExtent = OutlineExtent(box.S);
                if (rect.Bottom + outlineExtent < 0 || rect.Top - outlineExtent > ClientSize.Height) continue;
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
                        ApplyRoundedOverflowClips(pe.Graphics, box, paintOwners);
                    }
                    if (opacity > 0 && !box.Title && !EllipsisMarker(box) && (!box.Inline || box.B == null || box.B.Is.Count > 0)) PaintBox(pe.Graphics, rect, box.S, pageBackground, pageColor, opacity, box.B == null || box.B.I == null);
                    if (opacity > 0 && box.Fm) PaintFind(pe.Graphics, VisibleRect(content, clip), box.Fa);
                    if (box.B != null && box.B.Vg != null)
                    {
                        PaintVector(pe.Graphics, box, content, pageColor, opacity, clip);
                        continue;
                    }
                    if (box.P != null)
                    {
                        if (opacity > 0) PaintInlinePicture(pe.Graphics, box.P, content, opacity);
                        if (box.U != null && PointerEnabled(box.S))
                        {
                            var hit = VisibleRect(rect, clip);
                            if (!hit.IsEmpty) hits.Add(new Hit(hit, box.U, null, box.El));
                        }
                        continue;
                    }
                    if (box.K == "lm")
                    {
                        if (opacity > 0 && box.Px == null) PaintListMarker(pe.Graphics, box, content, opacity);
                        continue;
                    }
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
                        if (PointerEnabled(box.S) && !fieldHit.IsEmpty) hits.Add(new Hit(fieldHit, null, box));
                        continue;
                    }

                    using (var font = BoxFont(box.K, box.S, box.Title, box.U != null, box.Inline && box.B != null ? box.B.K : null))
                    {
                        var color = box.U != null ? Color.FromArgb(0, 102, 204) : pageColor;
                        if (box.S != null) color = CssColor(box.S.F, color);
                        var flags = box.Inline ? TextFormatFlags.SingleLine | TextFormatFlags.NoPadding : TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
                        if (!clip.IsEmpty) flags |= TextFormatFlags.PreserveGraphicsClipping;
                        if (opacity > 0) PaintText(pe.Graphics, box.Px ?? box.X, font, content, color, flags, opacity, box.S, box.U != null);
                    }

                    if (EllipsisMarker(box)) continue;
                    if (box.U != null && PointerEnabled(box.S))
                    {
                        var hit = VisibleRect(rect, clip);
                        if (!hit.IsEmpty) hits.Add(new Hit(hit, box.U, null, box.El));
                    }
                    else if (PointerEnabled(box.S) && box.B != null && box.B.K == "summary" && !String.IsNullOrEmpty(box.B.Mk))
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

        private void ApplyRoundedOverflowClips(Graphics graphics, Bx box, IDictionary<B, Bx> owners)
        {
            if (box == null || box.Clip.IsEmpty) return;
            for (var block = box == null ? null : box.B; block != null; block = block.Up)
            {
                Bx owner;
                if (!owners.TryGetValue(block, out owner)) continue;
                if (Object.ReferenceEquals(owner, box)) continue;
                if (!ClipsOverflow(owner.S) || !HasRadius(owner.S)) continue;
                var content = owner.C;
                if (!owner.Fx) content.Offset(0, -sy);
                var left = Math.Max(0, owner.C.Left - owner.R.Left);
                var top = Math.Max(0, owner.C.Top - owner.R.Top);
                var right = Math.Max(0, owner.R.Right - owner.C.Right);
                var bottom = Math.Max(0, owner.R.Bottom - owner.C.Bottom);
                using (var path = RoundedPath(content, owner.S, left, top, right, bottom))
                    graphics.SetClip(path, CombineMode.Intersect);
            }
        }

        private void Lay(Graphics graphics)
        {
            lock (layoutGate) LayCore(graphics, null, 0);
        }

        private void LayCore(Graphics graphics, IDictionary<B, int> resolved, int containerPass)
        {
            boxes.Clear();
            boxOwners.Clear();
            sticky = false;
            if (At == null) return;
            layoutViewportWidth = Math.Max(1, ClientSize.Width);
            layoutViewportHeight = Math.Max(1, ClientSize.Height);
            layoutRootFontSize = Rf;
            var measured = resolved ?? new Dictionary<B, int>();
            var contexts = new Dictionary<B, Lc>();
            var owners = new HashSet<B>();
            foreach (var block in At.Bs) if (!block.Hd && block.Up != null) owners.Add(block.Up);
            var active = new List<Lc>();
            var style = At.S;
            var left = style == null ? 0 : style.Ml + style.Pl + BorderWidth(style, 3);
            var right = style == null ? 0 : style.Mr + style.Pr + BorderWidth(style, 1);
            var x = 20 + left;
            var width = ConstrainWidth(style, Math.Max(120, ClientSize.Width - x - 20 - right));
            var y = 18 + (style == null ? 0 : style.Mt + style.Pt + BorderWidth(style, 0));
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
                foreach (var block in LayoutBlocks())
                {
                    CloseContexts(block, active, ref y, measured);
                    if (block.Hd) continue;
                    var bs = block.S;
                    Lc parent = null;
                    if (block.Up != null) contexts.TryGetValue(block.Up, out parent);
                    Fp slot = null;
                    var slotChild = parent != null && parent.Fs != null && parent.Fs.TryGetValue(block, out slot);
                    var gridChild = slotChild && parent != null && parent.B != null && IsGrid(parent.B.S);
                    var lx = slotChild ? slot.X : parent == null ? x : parent.C.X;
                    var lw = slotChild ? slot.W : parent == null ? width : parent.C.Width;
                    var ly = slotChild ? slot.C ? slot.H >= 0 ? slot.Y : parent.Y : FlexTop(parent, slot) : parent == null ? y : parent.Y;
                    var parentClip = parent == null ? Rectangle.Empty : parent.Cp;
                    var fixedParent = parent != null && parent.Fx;
                    var containingContext = ContainingContext(block, contexts);
                    var containing = bs != null && bs.Pn == "f" ? ClientRectangle : containingContext == null ? rootContaining : containingContext.C;
                    var heightReference = bs != null && bs.Pn == "f" ? Math.Max(1, ClientRectangle.Height) :
                        OutOfFlow(bs) ? containingContext == null ? rootContaining.Height : containingContext.Dh ? containing.Height : 0 :
                        gridChild && slot.H >= 0 ? slot.H : parent == null ? rootContaining.Height : parent.Dh ? parent.C.Height : 0;
                    if (block.K == "table" && block.Tb != null)
                    {
                        int next;
                        var tableReference = slotChild && !gridChild ? parent.C.Width : lw;
                        var tableContext = LayTable(graphics, block, lx, lw, tableReference, heightReference, ly, containing, parentClip, fixedParent, out next);
                        tableContext.Dh = HasHeight(bs, heightReference) || StretchesHeight(bs, heightReference) ||
                            slotChild && slot.C && slot.H >= 0 && (!slot.Q || !HasHeight(bs, heightReference));
                        contexts[block] = tableContext;
                        if (parent == null) y = next;
                        else if (slotChild)
                        {
                            parent.Y = Math.Max(parent.Y, next - 8);
                            FlexHeight(parent, tableContext.R.Bottom + (bs == null ? 0 : bs.Mb));
                        }
                        else parent.Y = next;
                        continue;
                    }
                    var bt = bs == null ? 0 : BorderWidth(bs, 0);
                    var br = bs == null ? 0 : BorderWidth(bs, 1);
                    var bb = bs == null ? 0 : BorderWidth(bs, 2);
                    var bl = bs == null ? 0 : BorderWidth(bs, 3);
                    var pt = bs == null ? 0 : bs.Pt;
                    var pr = bs == null ? 0 : bs.Pr;
                    var pb = bs == null ? 0 : bs.Pb;
                    var pl = bs == null ? 0 : bs.Pl;
                    var ml = bs == null ? 0 : bs.Ml;
                    var mr = bs == null ? 0 : bs.Mr;
                    var mt = bs == null ? 0 : bs.Mt;
                    var mb = bs == null ? 0 : bs.Mb;
                    var flowY = ly + mt;
                    var outerAvailable = Math.Max(gridChild ? 1 : 40, lw - ml - mr);
                    if (StretchesWidth(bs)) outerAvailable = Math.Max(1, containing.Width - bs.Lt.Value - bs.Rt.Value - ml - mr);
                    var widthReference = slotChild && !gridChild ? parent.C.Width : lw;
                    var bw = ConstrainWidth(bs, Math.Max(1, outerAvailable - bl - br - pl - pr), widthReference);
                    if (gridChild && GridAlign(parent, bs, true) != "t" && !HasWidth(bs))
                        bw = GridFitWidth(graphics, block, bs, bw, widthReference);
                    var preferredRatio = PreferredRatio(block);
                    var explicitHeight = HasHeight(bs, heightReference);
                    if (preferredRatio && !HasWidth(bs) && explicitHeight && !StretchesWidth(bs) && (!slotChild || gridChild))
                        bw = ConstrainResolvedWidth(bs, RatioContentWidth(bs, ResolvedHeight(bs, 0, heightReference)), bw, widthReference);
                    int height;
                    List<Ix> inline = null;
                    if (block.Vg != null)
                    {
                        var size = VectorSize(block, bw);
                        if (!HasWidth(bs)) bw = Math.Min(bw, size.Width);
                        height = size.Height;
                    }
                    else if (block.K == "img")
                    {
                        var size = PicSize(block, bw);
                        if (!HasWidth(bs)) bw = Math.Min(bw, size.Width);
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
                                var buttonWidth = TextWidth(graphics, block.X, font, bs) + 28;
                                if (!HasWidth(bs)) bw = Math.Min(bw, Math.Max(88, buttonWidth));
                            }
                        }
                    }
                    else if (NeedsFlow(block)) inline = InlineLayout(graphics, block, bw, out height);
                    else
                    {
                        using (var font = BoxFont(block.K, bs, false)) height = Measure(graphics, MarkedText(block), font, bw);
                    }
                    var naturalHeight = height;
                    if (preferredRatio && !explicitHeight && !StretchesHeight(bs, heightReference))
                        height = RatioPreferredHeight(block, bw, height);
                    height = ConstrainHeight(bs, height, heightReference);
                    var resolvedHeight = 0;
                    var resolvedOwner = resolved != null && owners.Contains(block) && !explicitHeight && !StretchesHeight(bs, heightReference) && resolved.TryGetValue(block, out resolvedHeight);
                    if (resolvedOwner)
                    {
                        if (preferredRatio) resolvedHeight = Math.Max(resolvedHeight, RatioContentHeight(bs, bw));
                        height = ConstrainHeight(bs, resolvedHeight, heightReference);
                    }
                    var stretchedHeight = StretchesHeight(bs, heightReference);
                    if (stretchedHeight) height = ConstrainHeight(bs, Math.Max(1, containing.Height - bs.Tp.Value - bs.Bm.Value - mt - mb - bt - bb - pt - pb), heightReference);
                    var flexStretch = slotChild && parent.Fh && FlexAlign(parent, bs) == "t" && !explicitHeight;
                    if (flexStretch)
                        height = ConstrainHeight(bs, Math.Max(1, parent.C.Bottom - parent.Ft - mt - mb - bt - bb - pt - pb), heightReference);
                    var slotStretch = slotChild && slot.C && slot.H >= 0 && (!gridChild || GridAlign(parent, bs, false) == "t") && (!slot.Q || !explicitHeight);
                    if (slotStretch)
                        height = ConstrainFlexHeight(bs, Math.Max(1, slot.H - mt - mb - bt - bb - pt - pb), heightReference);
                    var definiteHeight = explicitHeight || stretchedHeight || flexStretch || slotStretch;
                    var text = MarkedText(block);
                    var link = inline == null && block.K == "a" ? block.U : null;
                    var outerWidth = bw + pl + pr + bl + br;
                    var outerHeight = height + pt + pb + bt + bb;
                    var alignedX = gridChild ? GridX(parent, slot, bs, outerWidth, ml, mr) : slotChild && slot.C ? FlexX(parent, bs, outerWidth, ml, mr) : lx + ml;
                    var bx = PositionX(bs, alignedX, outerWidth, containing, ml, mr);
                    var alignedY = gridChild && slot.C && slot.H >= 0 ? GridY(parent, slot, bs, outerHeight, mt, mb) : slotChild && !slot.C ? FlexY(parent, bs, outerHeight, mt, mb, flowY) : flowY;
                    var by = PositionY(bs, alignedY, outerHeight, containing, mt, mb);
                    var outer = new Rectangle(bx, by, outerWidth, outerHeight);
                    var content = new Rectangle(bx + bl + pl, by + bt + pt, bw, height);
                    var fixedSelf = bs != null && bs.Pn == "f";
                    var inheritedClip = fixedSelf ? Rectangle.Empty : parentClip;
                    var ownerBox = new Bx(block.K, inline == null ? text : "", link, bs, outer, content, false, block, false, inheritedClip);
                    boxes.Add(ownerBox);
                    boxOwners[block] = ownerBox;
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
                        Au = owners.Contains(block) && !definiteHeight,
                        Fl = !OutOfFlow(bs),
                        Fy = flowY,
                        Gp = slotChild && slot.C && slot.H < 0 ? slot.G + mb : (HeadingKind(block.K) ? 12 : 8) + mb,
                        Pc = inheritedClip,
                        Co = ClipsOverflow(bs),
                        Fh = IsFlex(bs) && !FlexColumn(bs) && bs.Wr != "w" && (definiteHeight || resolvedOwner),
                        Fm = FlexColumn(bs) && definiteHeight,
                        Gm = IsGrid(bs) && (bs.Rc > 0 || bs.Ar > 0 || bs.Rac > 0) && (definiteHeight || resolvedOwner),
                        Dh = definiteHeight,
                        Hr = heightReference
                    };
                    ChildSlots(context);
                    contexts[block] = context;
                    if (context.Au) active.Add(context);
                    if (!fixedChain) layoutBottom = Math.Max(layoutBottom, outer.Bottom + mb);
                    if (!OutOfFlow(bs))
                    {
                        var next = flowY + outer.Height + (HeadingKind(block.K) ? 12 : 8) + mb;
                        if (parent == null) y = next;
                        else if (slotChild)
                        {
                            parent.Y = Math.Max(parent.Y, outer.Bottom + mb + slot.G);
                            FlexHeight(parent, outer.Bottom + mb);
                        }
                        else parent.Y = next;
                    }
                }
                CloseContexts(null, active, ref y, measured);
            }

            if (resolved == null && measured.Count > 0)
            {
                LayCore(graphics, measured, containerPass);
                return;
            }

            docHeight = y + 20 + (style == null ? 0 : style.Pb + BorderWidth(style, 2) + style.Mb);
            docHeight = Math.Max(docHeight, layoutBottom + 20);
            if (HasHeight(style, rootContaining.Height)) docHeight = Math.Max(docHeight, 18 + style.Mt + ResolvedHeight(style, 0, rootContaining.Height) + style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2) + style.Mb);
            else if (HasRatio(style)) docHeight = Math.Max(docHeight, 18 + style.Mt + RatioContentHeight(style, width) + style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2) + style.Mb);
            StackBoxes();
            var rootHeight = HasHeight(style, rootContaining.Height) ? ResolvedHeight(style, 0, rootContaining.Height) : Math.Max(0, docHeight - anchorTop - 20);
            if (containerPass < MaxContainerPasses && e.Cz(At, ContainerSizes(width, rootHeight)) > 0)
            {
                LayCore(graphics, null, containerPass + 1);
                return;
            }
            foreach (var box in BoxSnapshot())
            {
                box.SetBase();
                if (box.S != null && box.S.Pn == "k" && (box.S.Tp.HasValue || box.S.Bm.HasValue)) sticky = true;
            }
            layoutWidth = ClientSize.Width;
            layoutDirty = false;
            sy = Math.Min(sy, MaxScroll());
            ApplySticky();
            if (findQuery.Length > 0) RebuildFind(findIndex);
        }

        private IList<Cz> ContainerSizes(int rootWidth, int rootHeight)
        {
            var result = new List<Cz>();
            var seen = new HashSet<N>();
            if (At == null) return result;
            var body = ContainerBody(At.D);
            if (body != null && At.S != null && At.S.Ct != "n" && seen.Add(body))
                result.Add(new Cz(body, Math.Max(0, Math.Min(8192, rootWidth)), Math.Max(0, Math.Min(8192, rootHeight))));
            foreach (var block in At.Bs)
            {
                Bx box;
                if (!boxOwners.TryGetValue(block, out box) || box == null || box.El == null || box.S == null || box.S.Ct == "n" || !seen.Add(box.El)) continue;
                result.Add(new Cz(box.El, Math.Max(0, Math.Min(8192, box.C.Width)), Math.Max(0, Math.Min(8192, box.C.Height))));
                if (result.Count == MaxContainerSizes) break;
            }
            return result;
        }

        private static N ContainerBody(N node)
        {
            if (node == null) return null;
            if (node.K == "body") return node;
            foreach (var child in node.C)
            {
                var body = ContainerBody(child);
                if (body != null) return body;
            }
            return null;
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

        private IList<B> LayoutBlocks()
        {
            var orderedGrid = false;
            foreach (var block in At.Bs)
                if (block.Up != null && IsGrid(block.Up.S) && (block.S != null && block.S.Or != 0 || block.Up.S.Gf != "r")) { orderedGrid = true; break; }
            if (!orderedGrid) return At.Bs;

            var result = new List<B>(At.Bs.Count);
            var known = new HashSet<B>(At.Bs);
            var children = new Dictionary<B, List<B>>();
            var roots = new List<B>();
            foreach (var block in At.Bs)
            {
                if (block.Up == null || !known.Contains(block.Up)) roots.Add(block);
                else
                {
                    List<B> owned;
                    if (!children.TryGetValue(block.Up, out owned)) children[block.Up] = owned = new List<B>();
                    owned.Add(block);
                }
            }
            foreach (var root in roots) AddLayoutBlock(root, children, result);
            return result;
        }

        private static void AddLayoutBlock(B block, IDictionary<B, List<B>> children, IList<B> result)
        {
            result.Add(block);
            List<B> owned;
            if (!children.TryGetValue(block, out owned)) return;
            if (IsGrid(block.S))
            {
                SortFlexItems(owned);
                GridVisualSort(block.S, owned);
            }
            foreach (var child in owned) AddLayoutBlock(child, children, result);
        }

        private static void GridVisualSort(S style, List<B> items)
        {
            if (style == null || style.Gf == "r" || items.Count < 2) return;
            var columns = Math.Max(style.Gc, style.Ac);
            if (columns == 0 && style.Gac == 0 && style.Gf != "c" && style.Gf != "x") return;
            columns = Math.Max(1, columns);
            var flow = new List<B>();
            foreach (var item in items) if (!item.Hd && !OutOfFlow(item.S)) flow.Add(item);
            if (flow.Count < 2) return;
            int usedColumns;
            var placements = GridPositions(flow, style, columns, out usedColumns);
            var source = new Dictionary<B, int>();
            for (var index = 0; index < flow.Count; index++) source[flow[index]] = index;
            flow.Sort(delegate(B left, B right)
            {
                var row = placements[left].R.CompareTo(placements[right].R);
                if (row != 0) return row;
                var column = placements[left].C.CompareTo(placements[right].C);
                return column != 0 ? column : source[left].CompareTo(source[right]);
            });
            var flowIndex = 0;
            for (var index = 0; index < items.Count; index++)
                if (!items[index].Hd && !OutOfFlow(items[index].S)) items[index] = flow[flowIndex++];
        }

        private static bool Inside(B block, B parent)
        {
            for (var current = block == null ? null : block.Up; current != null; current = current.Up)
                if (Object.ReferenceEquals(current, parent)) return true;
            return false;
        }

        private void ChildSlots(Lc context)
        {
            if (context == null || context.B == null) return;
            if (IsGrid(context.B.S)) { GridSlots(context); return; }
            if (!IsFlex(context.B.S)) return;
            var items = new List<B>();
            foreach (var item in At.Bs)
                if (!item.Hd && Object.ReferenceEquals(item.Up, context.B) && !OutOfFlow(item.S)) items.Add(item);
            if (items.Count == 0) return;

            var style = context.B.S;
            if (FlexColumn(style))
            {
                FlexColumnSlots(context, items);
                return;
            }
            if (style.Wr != "w") SortFlexItems(items);
            var hints = new Dictionary<B, int>();
            foreach (var item in items)
                hints[item] = FlexOuterWidth(item, context.C.Width, context.Dh ? context.C.Height : 0);

            var gap = Math.Min(style.Gp, context.C.Width);
            var lines = new List<List<B>>();
            var line = new List<B>();
            var lineWidth = 0;
            foreach (var item in items)
            {
                var addition = hints[item] + (line.Count == 0 ? 0 : gap);
                if (style.Wr == "w" && line.Count > 0 && hints[item] > 0 && lineWidth + addition > context.C.Width)
                {
                    lines.Add(line);
                    line = new List<B>();
                    lineWidth = 0;
                    addition = hints[item];
                }
                line.Add(item);
                lineWidth += addition;
            }
            if (line.Count > 0) lines.Add(line);

            context.Fs = new Dictionary<B, Fp>();
            context.Ft = context.Y;
            context.Fi = -1;
            context.Fz = 0;
            context.Fr = style.Rg;
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var visual = lines[lineIndex];
                var fixedWidth = 0;
                var growTotal = 0;
                var autos = 0;
                var autoMinimum = 0;
                var widths = new Dictionary<B, int>();
                foreach (var item in visual)
                {
                    var hint = hints[item];
                    var width = hint > 0 ? hint : FlexMinOuterWidth(item.S, context.C.Width);
                    widths[item] = width;
                    if (hint > 0) fixedWidth += width;
                    else { autos++; autoMinimum += width; }
                    var grow = item.S == null ? 0 : item.S.Fg;
                    if (grow > 0) growTotal += grow;
                }

                var lineGap = gap;
                var baseGaps = lineGap * Math.Max(0, visual.Count - 1);
                var available = Math.Max(0, context.C.Width - baseGaps);
                var baseWidth = fixedWidth + autoMinimum;
                if (baseWidth > available)
                {
                    FlexShrink(visual, hints, widths, available, false, context.C.Width);
                }
                else if (growTotal > 0)
                {
                    var free = available - baseWidth;
                    var remaining = free;
                    var remainingGrow = growTotal;
                    foreach (var item in visual)
                    {
                        var grow = item.S == null ? 0 : item.S.Fg;
                        if (grow > 0)
                        {
                            var share = remainingGrow == 0 ? 0 : remaining * grow / remainingGrow;
                            widths[item] += share;
                            remaining -= share;
                            remainingGrow -= grow;
                        }
                    }
                }
                else
                {
                    var free = available - baseWidth;
                    var autoWidth = autos == 0 ? 0 : Math.Max(1, free / autos);
                    var autoRemainder = autos == 0 ? 0 : Math.Max(0, free - autoWidth * autos);
                    foreach (var item in visual)
                    {
                        if (hints[item] == 0)
                        {
                            widths[item] += autoWidth;
                            if (autoRemainder > 0) { widths[item]++; autoRemainder--; }
                        }
                    }
                }

                var used = baseGaps;
                foreach (var item in visual) used += widths[item];
                var leftover = Math.Max(0, context.C.Width - used);
                var reverse = FlexRowReverse(style);
                var offset = FlexJustify(style.Jc, visual.Count, leftover, reverse, ref lineGap);

                if (reverse) visual.Reverse();
                var cursor = context.C.X + offset;
                foreach (var item in visual)
                {
                    context.Fs[item] = new Fp(cursor, widths[item], lineIndex);
                    cursor += widths[item] + lineGap;
                }
            }
        }

        private void GridSlots(Lc context)
        {
            var style = context.B.S;
            var columnFlow = style != null && (style.Gf == "c" || style.Gf == "x");
            if (style == null || Math.Max(style.Gc, style.Ac) == 0 && style.Gac == 0 && !columnFlow) return;
            var items = new List<B>();
            foreach (var item in At.Bs)
                if (!item.Hd && Object.ReferenceEquals(item.Up, context.B) && !OutOfFlow(item.S)) items.Add(item);
            if (items.Count == 0) return;
            SortFlexItems(items);

            var columns = Math.Max(1, Math.Max(style.Gc, style.Ac));
            int usedColumns;
            var placements = GridPositions(items, style, columns, out usedColumns);
            columns = Math.Max(columns, usedColumns);
            var gap = Math.Min(style.Gp, Math.Max(0, context.C.Width / Math.Max(1, columns - 1)));
            var widths = GridTrackSizes(style, false, columns, context.C.Width, gap);
            var used = gap * Math.Max(0, columns - 1);
            foreach (var width in widths) used += width;
            var rtl = InlineReverse(style);
            var offset = FlexJustify(style.Jc, columns, Math.Max(0, context.C.Width - used), rtl, ref gap);
            var starts = new int[columns];
            var cursor = context.C.X + offset;
            if (rtl)
            {
                var span = gap * Math.Max(0, columns - 1);
                foreach (var width in widths) span += width;
                cursor += span;
                for (var index = 0; index < columns; index++)
                {
                    cursor -= widths[index];
                    starts[index] = cursor;
                    cursor -= gap;
                }
            }
            else
            {
                for (var index = 0; index < columns; index++)
                {
                    starts[index] = cursor;
                    cursor += widths[index] + gap;
                }
            }

            context.Fs = new Dictionary<B, Fp>();
            context.Ft = context.Y;
            context.Fi = -1;
            context.Fz = 0;
            context.Fr = style.Rg;
            var rows = Math.Max(style.Rc, style.Ar);
            foreach (var placement in placements.Values) rows = Math.Max(rows, placement.R + placement.Rs);
            var rowGap = rows < 2 ? 0 : context.Gm ? Math.Min(style.Rg, Math.Max(0, context.C.Height / (rows - 1))) : style.Rg;
            context.Gh = GridTrackMinimum(style, true, rows, rowGap);
            context.Gk = rows > 0;
            for (var row = 0; row < rows; row++) if (GridTrack(style, true, row) < 0) { context.Gk = false; break; }
            var heights = context.Gm ? GridTrackSizes(style, true, rows, context.C.Height, rowGap) : null;
            var rowStarts = heights == null ? null : new int[rows];
            if (rowStarts != null)
            {
                var usedHeight = rowGap * Math.Max(0, rows - 1);
                foreach (var height in heights) usedHeight += height;
                var rowOffset = FlexJustify(style.Al, rows, Math.Max(0, context.C.Height - usedHeight), false, ref rowGap);
                var top = context.C.Y + rowOffset;
                for (var index = 0; index < rows; index++)
                {
                    rowStarts[index] = top;
                    top += heights[index] + rowGap;
                }
            }

            var naturalLine = 0;
            foreach (var item in items)
            {
                var placement = placements[item];
                var width = 0;
                for (var track = placement.C; track < placement.C + placement.Cs; track++) width += widths[track];
                width += gap * Math.Max(0, placement.Cs - 1);
                if (rowStarts != null)
                {
                    var height = 0;
                    for (var track = placement.R; track < placement.R + placement.Rs; track++) height += heights[track];
                    height += rowGap * Math.Max(0, placement.Rs - 1);
                    var column = rtl ? placement.C + placement.Cs - 1 : placement.C;
                    context.Fs[item] = new Fp(starts[column], Math.Max(1, width), placement.R, rowStarts[placement.R], Math.Max(1, height), true, 0, true);
                }
                else
                {
                    naturalLine = Math.Max(naturalLine, placement.R);
                    var column = rtl ? placement.C + placement.Cs - 1 : placement.C;
                    context.Fs[item] = new Fp(starts[column], Math.Max(1, width), style.Gf == "r" ? naturalLine : placement.R);
                }
            }
        }

        private static Dictionary<B, Gq> GridPositions(IList<B> items, S container, int columns, out int usedColumns)
        {
            var result = new Dictionary<B, Gq>();
            var occupied = new HashSet<int>();
            var columnFlow = container.Gf == "c" || container.Gf == "x";
            var dense = container.Gf == "d" || container.Gf == "x";
            var placementColumns = columnFlow ? 8 : columns;
            if (!columnFlow)
            {
                foreach (var item in items)
                {
                    int fixedColumn;
                    int columnSpan;
                    GridAxis(container, item.S, false, 8, out fixedColumn, out columnSpan);
                    placementColumns = Math.Max(placementColumns, fixedColumn >= 0 ? fixedColumn + columnSpan : columnSpan);
                }
                placementColumns = Math.Max(1, Math.Min(8, placementColumns));
            }
            var rowLimit = Math.Max(1, Math.Max(container.Rc, container.Ar));
            var queue = new List<B>(items.Count);
            foreach (var item in items) if (GridDefinite(container, item.S, placementColumns)) queue.Add(item);
            var definite = queue.Count;
            foreach (var item in items) if (!GridDefinite(container, item.S, placementColumns)) queue.Add(item);
            var cursorRow = 0;
            var cursorColumn = 0;
            usedColumns = columns;
            for (var itemIndex = 0; itemIndex < queue.Count; itemIndex++)
            {
                if (itemIndex == definite || dense && itemIndex >= definite) { cursorRow = 0; cursorColumn = 0; }
                var item = queue[itemIndex];
                var style = item.S;
                int areaRow;
                int areaColumn;
                int areaRowSpan;
                int areaColumnSpan;
                var named = GridNamedArea(container, style, placementColumns, out areaRow, out areaColumn, out areaRowSpan, out areaColumnSpan);
                int fixedColumn;
                int columnSpan;
                int fixedRow;
                int rowSpan;
                if (named)
                {
                    fixedColumn = areaColumn;
                    columnSpan = areaColumnSpan;
                    fixedRow = areaRow;
                    rowSpan = areaRowSpan;
                }
                else
                {
                    GridAxis(container, style, false, placementColumns, out fixedColumn, out columnSpan);
                    GridAxis(container, style, true, 8, out fixedRow, out rowSpan);
                }
                var row = fixedRow >= 0 ? fixedRow : cursorRow;
                var column = fixedColumn >= 0 ? fixedColumn : columnFlow ? cursorColumn : fixedRow >= 0 ? 0 : cursorColumn;
                if (!columnFlow && fixedRow < 0 && fixedColumn >= 0 && column < cursorColumn) row++;
                if (columnFlow && fixedColumn < 0 && fixedRow >= 0 && row < cursorRow) column++;

                var placed = false;
                for (var attempt = 0; attempt < 8192; attempt++)
                {
                    var withinRows = !columnFlow || fixedColumn >= 0 || row + rowSpan <= rowLimit;
                    if (column + columnSpan <= placementColumns && withinRows && GridFree(occupied, row, column, rowSpan, columnSpan)) { placed = true; break; }
                    if (columnFlow)
                    {
                        if (fixedColumn >= 0) row++;
                        else if (fixedRow >= 0) column++;
                        else
                        {
                            row++;
                            if (row + rowSpan > rowLimit) { column++; row = 0; }
                        }
                        if (column + columnSpan > placementColumns && fixedColumn < 0) break;
                    }
                    else if (fixedColumn >= 0) row++;
                    else
                    {
                        column++;
                        if (column + columnSpan > placementColumns) { row++; column = 0; }
                    }
                }
                if (!placed && columnFlow)
                {
                    column = Math.Max(0, Math.Min(placementColumns - columnSpan, fixedColumn >= 0 ? fixedColumn : placementColumns - columnSpan));
                    row = Math.Max(rowLimit, row);
                    for (var attempt = 0; attempt < 8192 && !GridFree(occupied, row, column, rowSpan, columnSpan); attempt++) row++;
                }
                else column = Math.Max(0, Math.Min(placementColumns - columnSpan, column));
                GridOccupy(occupied, row, column, rowSpan, columnSpan);
                result[item] = new Gq(row, column, rowSpan, columnSpan);
                usedColumns = Math.Max(usedColumns, column + columnSpan);

                var nextRow = columnFlow ? row + rowSpan : row;
                var nextColumn = columnFlow ? column : column + columnSpan;
                if (columnFlow && nextRow >= rowLimit) { nextRow = 0; nextColumn = column + columnSpan; }
                else if (!columnFlow && nextColumn >= placementColumns) { nextRow++; nextColumn = 0; }
                if (columnFlow ? nextColumn > cursorColumn || nextColumn == cursorColumn && nextRow > cursorRow : nextRow > cursorRow || nextRow == cursorRow && nextColumn > cursorColumn)
                {
                    cursorRow = nextRow;
                    cursorColumn = nextColumn;
                }
            }
            return result;
        }

        private static bool GridDefinite(S container, S item, int columns)
        {
            int row;
            int column;
            int rowSpan;
            int columnSpan;
            if (GridNamedArea(container, item, columns, out row, out column, out rowSpan, out columnSpan)) return true;
            GridAxis(container, item, false, columns, out column, out columnSpan);
            GridAxis(container, item, true, 8, out row, out rowSpan);
            return column >= 0 && row >= 0;
        }

        private static void GridAxis(S container, S item, bool rows, int limit, out int start, out int span)
        {
            span = Math.Max(1, Math.Min(limit, item == null ? 1 : rows ? item.Rs : item.Gs));
            start = item == null || (rows ? item.Rl : item.Gl) < 1 ? -1 : Math.Min(limit - 1, (rows ? item.Rl : item.Gl) - 1);
            if (item == null) return;
            var startName = rows ? item.Rn : item.Gn;
            var endName = rows ? item.Re : item.Ge;
            if (String.IsNullOrEmpty(startName) && String.IsNullOrEmpty(endName)) return;
            var startLine = String.IsNullOrEmpty(startName) ? 0 : GridLine(container, startName, rows, false);
            var endLine = String.IsNullOrEmpty(endName) ? 0 : GridLine(container, endName, rows, true);
            if (!String.IsNullOrEmpty(startName) && startLine == 0 || !String.IsNullOrEmpty(endName) && endLine == 0) { start = -1; return; }
            if (startLine > 0) start = startLine - 1;
            if (endLine > 0)
            {
                if (start >= 0)
                {
                    var resolvedSpan = endLine - start - 1;
                    if (resolvedSpan <= 0 && startName == endName && startLine == endLine) resolvedSpan = 1;
                    if (resolvedSpan <= 0 || resolvedSpan > limit) { start = -1; return; }
                    span = resolvedSpan;
                }
                else start = endLine - span - 1;
            }
            if (start < 0 || start + span > limit) start = -1;
        }

        private static int GridLine(S container, string name, bool rows, bool end)
        {
            var edge = (rows ? container.Rx(name + (end ? "-end" : "-start")) : container.Gx(name + (end ? "-end" : "-start")));
            return edge > 0 ? edge : rows ? container.Rx(name) : container.Gx(name);
        }

        private static bool GridNamedArea(S container, S item, int columns, out int row, out int column, out int rowSpan, out int columnSpan)
        {
            row = -1;
            column = -1;
            rowSpan = 1;
            columnSpan = 1;
            if (container == null || item == null || String.IsNullOrEmpty(item.An)) return false;
            row = container.Ag(item.An, 0) - 1;
            column = container.Ag(item.An, 1) - 1;
            rowSpan = container.Ag(item.An, 2);
            columnSpan = container.Ag(item.An, 3);
            return row >= 0 && column >= 0 && rowSpan > 0 && columnSpan > 0 && column + columnSpan <= columns;
        }

        private static bool GridFree(ISet<int> occupied, int row, int column, int rowSpan, int columnSpan)
        {
            for (var y = row; y < row + rowSpan; y++)
                for (var x = column; x < column + columnSpan; x++)
                    if (occupied.Contains(y * 8 + x)) return false;
            return true;
        }

        private static void GridOccupy(ISet<int> occupied, int row, int column, int rowSpan, int columnSpan)
        {
            for (var y = row; y < row + rowSpan; y++)
                for (var x = column; x < column + columnSpan; x++) occupied.Add(y * 8 + x);
        }

        private static int[] GridTrackSizes(S style, bool rows, int count, int available, int gap)
        {
            var result = new int[count];
            var room = Math.Max(count, available - gap * Math.Max(0, count - 1));
            var fixedTotal = 0;
            var fractionTotal = 0;
            var fractionCount = 0;
            for (var index = 0; index < count; index++)
            {
                var track = GridTrack(style, rows, index);
                if (track >= 0) fixedTotal += Math.Max(1, track);
                else { fractionTotal += GridTrackWeight(track); fractionCount++; }
            }

            if (fixedTotal + fractionCount <= room)
            {
                for (var index = 0; index < count; index++) if (GridTrack(style, rows, index) >= 0) result[index] = Math.Max(1, GridTrack(style, rows, index));
                GridFractionSizes(style, rows, result, Math.Max(0, room - fixedTotal), fractionTotal);
            }
            else
            {
                var weights = new int[count];
                var total = 0;
                for (var index = 0; index < count; index++)
                {
                    var track = GridTrack(style, rows, index);
                    weights[index] = track >= 0 ? Math.Max(1, track) : GridTrackWeight(track);
                    total += weights[index];
                }
                GridWeightedWidths(result, weights, room, total);
            }
            return result;
        }

        private static int GridTrack(S style, bool rows, int index)
        {
            var explicitCount = rows ? style.Rc : style.Gc;
            if (index < explicitCount) return rows ? style.Rv(index) : style.Gv(index);
            var automaticCount = rows ? style.Rac : style.Gac;
            if (automaticCount == 0) return S.Av;
            var automaticIndex = (index - explicitCount) % automaticCount;
            return rows ? style.Rav(automaticIndex) : style.Gav(automaticIndex);
        }

        private static int GridTrackWeight(int track)
        {
            return track == S.Av ? 1 : Math.Max(1, -track);
        }

        private static int GridTrackMinimum(S style, bool rows, int count, int gap)
        {
            var total = gap * Math.Max(0, count - 1);
            for (var index = 0; index < count; index++)
            {
                var track = GridTrack(style, rows, index);
                total += track >= 0 ? Math.Max(1, track) : 1;
            }
            return total;
        }

        private static void GridFractionSizes(S style, bool rows, int[] result, int room, int total)
        {
            if (total <= 0) return;
            var remaining = room;
            var weight = total;
            var tracks = 0;
            for (var index = 0; index < result.Length; index++) if (GridTrack(style, rows, index) < 0) tracks++;
            for (var index = 0; index < result.Length; index++)
            {
                var value = GridTrack(style, rows, index);
                if (value >= 0) continue;
                var trackWeight = GridTrackWeight(value);
                var width = tracks == 1 ? remaining : weight == 0 ? 1 : remaining * trackWeight / weight;
                width = Math.Max(1, Math.Min(Math.Max(1, remaining - tracks + 1), width));
                result[index] = width;
                remaining -= width;
                weight -= trackWeight;
                tracks--;
            }
        }

        private static void GridWeightedWidths(int[] result, int[] weights, int room, int total)
        {
            var remaining = room;
            var weight = total;
            for (var index = 0; index < result.Length; index++)
            {
                var tracks = result.Length - index;
                var width = tracks == 1 ? remaining : weight == 0 ? 1 : remaining * weights[index] / weight;
                width = Math.Max(1, Math.Min(Math.Max(1, remaining - tracks + 1), width));
                result[index] = width;
                remaining -= width;
                weight -= weights[index];
            }
        }

        private void FlexColumnSlots(Lc context, List<B> items)
        {
            context.Fs = new Dictionary<B, Fp>();
            if (!context.Fm)
            {
                for (var index = 0; index < items.Count; index++)
                    context.Fs[items[index]] = new Fp(context.C.X, context.C.Width, -1, 0, -1, true, index + 1 < items.Count ? context.B.S.Rg : 0);
                return;
            }

            SortFlexItems(items);
            var style = context.B.S;
            var hints = new Dictionary<B, int>();
            foreach (var item in items) hints[item] = FlexOuterHeight(item, context.C.Width, context.C.Height);
            var gap = Math.Min(style.Rg, context.C.Height);
            var baseGaps = gap * Math.Max(0, items.Count - 1);
            var available = Math.Max(0, context.C.Height - baseGaps);
            var heights = FlexSizes(items, hints, available, true, context.C.Height);
            var used = baseGaps;
            foreach (var item in items) used += heights[item];
            var leftover = Math.Max(0, context.C.Height - used);
            var offset = FlexJustify(style.Jc, items.Count, leftover, style.Fd == "y", ref gap);
            if (style.Fd == "y") items.Reverse();
            var cursor = context.C.Y + offset;
            foreach (var item in items)
            {
                context.Fs[item] = new Fp(context.C.X, context.C.Width, -1, cursor, heights[item], true);
                cursor += heights[item] + gap;
            }
        }

        private static void SortFlexItems(List<B> items)
        {
            var source = new Dictionary<B, int>();
            for (var index = 0; index < items.Count; index++) source[items[index]] = index;
            items.Sort(delegate(B left, B right)
            {
                var leftOrder = left.S == null ? 0 : left.S.Or;
                var rightOrder = right.S == null ? 0 : right.S.Or;
                var order = leftOrder.CompareTo(rightOrder);
                return order != 0 ? order : source[left].CompareTo(source[right]);
            });
        }

        private static int FlexJustify(string mode, int count, int leftover, bool reverse, ref int gap)
        {
            if (mode == "c") return leftover / 2;
            if (mode == "e") return reverse ? 0 : leftover;
            if (mode == "s") return reverse ? leftover : 0;
            if (mode == "b" && count > 1) gap += leftover / (count - 1);
            else if (mode == "a" && count > 0)
            {
                var space = leftover / count;
                gap += space;
                return space / 2;
            }
            else if (mode == "v" && count > 0)
            {
                var space = leftover / (count + 1);
                gap += space;
                return space;
            }
            return 0;
        }

        private Dictionary<B, int> FlexSizes(IList<B> items, IDictionary<B, int> hints, int available, bool column, int reference)
        {
            var fixedSize = 0;
            var growTotal = 0;
            var autos = 0;
            var autoMinimum = 0;
            var sizes = new Dictionary<B, int>();
            foreach (var item in items)
            {
                var hint = hints[item];
                var size = hint > 0 ? hint : FlexMinOuter(item.S, column, reference);
                sizes[item] = size;
                if (hint > 0) fixedSize += size;
                else { autos++; autoMinimum += size; }
                var grow = item.S == null ? 0 : item.S.Fg;
                if (grow > 0) growTotal += grow;
            }

            var baseSize = fixedSize + autoMinimum;
            if (baseSize > available) FlexShrink(items, hints, sizes, available, column, reference);
            else if (growTotal > 0)
            {
                var remaining = available - baseSize;
                var remainingGrow = growTotal;
                foreach (var item in items)
                {
                    var grow = item.S == null ? 0 : item.S.Fg;
                    if (grow <= 0) continue;
                    var share = remainingGrow == 0 ? 0 : remaining * grow / remainingGrow;
                    sizes[item] += share;
                    remaining -= share;
                    remainingGrow -= grow;
                }
            }
            else
            {
                var free = available - baseSize;
                var autoSize = autos == 0 ? 0 : Math.Max(1, free / autos);
                var remainder = autos == 0 ? 0 : Math.Max(0, free - autoSize * autos);
                foreach (var item in items)
                {
                    if (hints[item] != 0) continue;
                    sizes[item] += autoSize;
                    if (remainder > 0) { sizes[item]++; remainder--; }
                }
            }
            return sizes;
        }

        private int FlexOuterWidth(B block, int reference, int heightReference)
        {
            var style = block == null ? null : block.S;
            if (style == null) return 0;
            var basis = style.Fb >= 0 ? BoxContentWidth(style, style.Fb) : HasWidth(style) ? ResolvedWidth(style, 0, reference) : 0;
            if (basis == 0 && style.Fb < 0 && HasHeight(style, heightReference) && PreferredRatio(block))
            {
                basis = RatioContentWidth(style, ResolvedHeight(style, 0, heightReference));
            }
            if (basis == 0 && style.Fb < 0 && !HasMinWidth(style)) return 0;
            if (HasMaxWidth(style)) basis = Math.Min(basis, ResolvedWidth(style, 2, reference));
            if (HasMinWidth(style)) basis = Math.Max(basis, ResolvedWidth(style, 1, reference));
            return basis + FlexHorizontal(style);
        }

        private int FlexMinOuterWidth(S style, int reference)
        {
            if (style == null) return 1;
            return Math.Max(1, ResolvedWidth(style, 1, reference)) + FlexHorizontal(style);
        }

        private int FlexOuterHeight(B block, int widthReference, int heightReference)
        {
            var style = block == null ? null : block.S;
            if (style == null) return 0;
            var basis = style.Fb >= 0 ? style.Fb : HasHeight(style, heightReference) ? ResolvedHeight(style, 0, heightReference) : 0;
            var contentBasis = false;
            if (basis == 0 && style.Fb < 0 && HasWidth(style) && PreferredRatio(block))
            {
                basis = RatioContentHeight(style, ResolvedWidth(style, 0, widthReference));
                contentBasis = true;
            }
            if (basis == 0 && style.Fb < 0 && !HasMinHeight(style, heightReference)) return 0;
            if (!contentBasis && style.Fb >= 0) basis = BoxContentHeight(style, basis);
            if (HasMaxHeight(style, heightReference)) basis = Math.Min(basis, ResolvedHeight(style, 2, heightReference));
            if (HasMinHeight(style, heightReference)) basis = Math.Max(basis, ResolvedHeight(style, 1, heightReference));
            return basis + FlexVertical(style);
        }

        private int FlexMinOuter(S style, bool column, int reference)
        {
            return column ? FlexMinOuterHeight(style, reference) : FlexMinOuterWidth(style, reference);
        }

        private int FlexMinOuterHeight(S style, int reference)
        {
            if (style == null) return 1;
            return Math.Max(1, HasMinHeight(style, reference) ? ResolvedHeight(style, 1, reference) : 0) + FlexVertical(style);
        }

        private static int FlexHorizontal(S style)
        {
            if (style == null) return 0;
            return style.Ml + style.Mr + style.Pl + style.Pr + BorderWidth(style, 3) + BorderWidth(style, 1);
        }

        private static int FlexVertical(S style)
        {
            if (style == null) return 0;
            return style.Mt + style.Mb + style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2);
        }

        private void FlexShrink(IList<B> items, IDictionary<B, int> hints, IDictionary<B, int> sizes, int available, bool column, int reference)
        {
            var deficit = 0;
            foreach (var item in items) deficit += sizes[item];
            deficit = Math.Max(0, deficit - available);
            while (deficit > 0)
            {
                var active = new List<B>();
                long totalWeight = 0;
                foreach (var item in items)
                {
                    var shrink = item.S == null ? 1 : item.S.Sh;
                    if (shrink <= 0 || sizes[item] <= FlexMinOuter(item.S, column, reference)) continue;
                    active.Add(item);
                    totalWeight += (long)Math.Max(1, hints[item]) * shrink;
                }
                if (active.Count == 0 || totalWeight <= 0) break;

                var remaining = deficit;
                var remainingWeight = totalWeight;
                var reduced = 0;
                foreach (var item in active)
                {
                    if (remaining <= 0) break;
                    var weight = (long)Math.Max(1, hints[item]) * (item.S == null ? 1 : item.S.Sh);
                    var share = (int)Math.Min(remaining, ((long)remaining * weight + remainingWeight - 1) / remainingWeight);
                    var room = sizes[item] - FlexMinOuter(item.S, column, reference);
                    var amount = Math.Min(room, share);
                    sizes[item] -= amount;
                    remaining -= amount;
                    remainingWeight -= weight;
                    reduced += amount;
                }
                if (reduced == 0) break;
                deficit -= reduced;
            }
        }

        private static int FlexTop(Lc parent, Fp slot)
        {
            if (parent == null || slot == null) return parent == null ? 0 : parent.Ft;
            if (parent.Fi < 0) parent.Fi = slot.L;
            while (parent.Fi < slot.L)
            {
                parent.Ft += Math.Max(1, parent.Fz) + parent.Fr;
                parent.Fz = 0;
                parent.Fi++;
            }
            return parent.Ft;
        }

        private static void FlexHeight(Lc parent, int bottom)
        {
            if (parent == null || parent.Fi < 0) return;
            parent.Fz = Math.Max(parent.Fz, bottom - parent.Ft);
        }

        private static int FlexY(Lc parent, S style, int outerHeight, int mt, int mb, int flowY)
        {
            if (parent == null || !parent.Fh || parent.B == null || parent.B.S == null) return flowY;
            var free = Math.Max(0, parent.C.Bottom - parent.Ft - outerHeight - mt - mb);
            var align = FlexAlign(parent, style);
            if (align == "c") return parent.Ft + mt + free / 2;
            if (align == "e") return parent.Ft + mt + free;
            return parent.Ft + mt;
        }

        private static int GridX(Lc parent, Fp slot, S style, int outerWidth, int ml, int mr)
        {
            if (slot == null) return parent == null ? ml : parent.C.X + ml;
            var free = Math.Max(0, slot.W - outerWidth - ml - mr);
            var align = GridAlign(parent, style, true);
            if (align == "c") return slot.X + ml + free / 2;
            var rtl = parent != null && parent.B != null && InlineReverse(parent.B.S);
            if (align == "e") return slot.X + ml + (rtl ? 0 : free);
            if (align == "s") return slot.X + ml + (rtl ? free : 0);
            return slot.X + ml;
        }

        private static int GridY(Lc parent, Fp slot, S style, int outerHeight, int mt, int mb)
        {
            if (slot == null) return parent == null ? mt : parent.C.Y + mt;
            var free = Math.Max(0, slot.H - outerHeight - mt - mb);
            var align = GridAlign(parent, style, false);
            if (align == "c") return slot.Y + mt + free / 2;
            if (align == "e") return slot.Y + mt + free;
            return slot.Y + mt;
        }

        private static int FlexX(Lc parent, S style, int outerWidth, int ml, int mr)
        {
            if (parent == null || parent.B == null || parent.B.S == null) return parent == null ? ml : parent.C.X + ml;
            var free = Math.Max(0, parent.C.Width - outerWidth - ml - mr);
            var align = FlexAlign(parent, style);
            if (align == "c") return parent.C.X + ml + free / 2;
            var rtl = InlineReverse(parent.B.S);
            if (align == "e") return parent.C.X + ml + (rtl ? 0 : free);
            if (align == "s") return parent.C.X + ml + (rtl ? free : 0);
            return parent.C.X + ml;
        }

        private static string FlexAlign(Lc parent, S style)
        {
            return style != null && style.As.Length > 0 ? style.As : parent == null || parent.B == null || parent.B.S == null ? "t" : parent.B.S.Ai;
        }

        private static string GridAlign(Lc parent, S style, bool horizontal)
        {
            if (!horizontal) return FlexAlign(parent, style);
            return style != null && style.Js.Length > 0 ? style.Js : parent == null || parent.B == null || parent.B.S == null ? "t" : parent.B.S.Ji;
        }

        private static bool IsFlex(S style)
        {
            return style != null && (style.D == "flex" || style.D == "inline-flex");
        }

        private static bool IsGrid(S style)
        {
            return style != null && (style.D == "grid" || style.D == "inline-grid");
        }

        private static bool FlexColumn(S style)
        {
            return IsFlex(style) && (style.Fd == "c" || style.Fd == "y");
        }

        private static bool InlineReverse(S style)
        {
            return style != null && style.Dr == "r";
        }

        private static bool FlexRowReverse(S style)
        {
            return style != null && (style.Fd == "x") != InlineReverse(style);
        }

        private void FinishContext(Lc context, ref int rootY, IDictionary<B, int> measured)
        {
            var natural = Math.Max(context.C.Height, Math.Max(1, context.Y - context.C.Top));
            var needed = context.Gk ? Math.Max(context.Gh, context.C.Height) : Math.Max(context.Gh, natural);
            var height = ConstrainHeight(context.B == null ? null : context.B.S, needed, context.Hr);
            if (height != context.C.Height)
            {
                context.C = new Rectangle(context.C.X, context.C.Y, context.C.Width, height);
                context.R = new Rectangle(context.R.X, context.R.Y, context.R.Width, height + context.R.Height - context.Box.C.Height);
                context.Box.C = context.C;
                context.Box.R = context.R;
                if (context.Co) context.Cp = CombineClip(context.Pc, context.C);
            }
            if (context.B != null) measured[context.B] = height;
            Fp flex;
            if (context.Up != null && context.Up.Fs != null && context.B != null && context.Up.Fs.TryGetValue(context.B, out flex))
                FlexHeight(context.Up, context.R.Bottom + (context.B.S == null ? 0 : context.B.S.Mb));
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

        private Lc LayTable(Graphics graphics, B block, int x, int width, int widthReference, int heightReference, int y, Rectangle containing, Rectangle parentClip, bool fixedParent, out int nextY)
        {
            var style = block.S;
            var bt = style == null ? 0 : BorderWidth(style, 0);
            var br = style == null ? 0 : BorderWidth(style, 1);
            var bb = style == null ? 0 : BorderWidth(style, 2);
            var bl = style == null ? 0 : BorderWidth(style, 3);
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
            var tableWidth = ConstrainWidth(style, Math.Max(1, available - bl - br - pl - pr), widthReference);
            var preferredRatio = PreferredRatio(block);
            var explicitHeight = HasHeight(style, heightReference);
            if (preferredRatio && !HasWidth(style) && explicitHeight && !StretchesWidth(style))
                tableWidth = ConstrainResolvedWidth(style, RatioContentWidth(style, ResolvedHeight(style, 0, heightReference)), tableWidth, widthReference);
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
            if (preferredRatio && !explicitHeight && !StretchesHeight(style, heightReference)) naturalHeight = Math.Max(naturalHeight, RatioContentHeight(style, tableWidth));
            var contentHeight = ConstrainHeight(style, naturalHeight, heightReference);
            if (StretchesHeight(style, heightReference)) contentHeight = ConstrainHeight(style, Math.Max(1, containing.Height - style.Tp.Value - style.Bm.Value - mt - mb - bt - bb - pt - pb), heightReference);
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
            var ownerBox = new Bx("table", "", null, style, outerTable, contentTable, false, block, false, inheritedClip);
            boxes.Insert(first, ownerBox);
            boxOwners[block] = ownerBox;
            var fixedChain = fixedSelf || fixedParent;
            if (!fixedChain) layoutBottom = Math.Max(layoutBottom, outerTable.Bottom + mb);
            nextY = OutOfFlow(style) ? y : flowY + outerTable.Height + 8 + mb;
            return new Lc(outerTable, contentTable, contentY, childClip, fixedChain) { Hr = heightReference };
        }

        private static int PositionX(S style, int flowX, int outerWidth, Rectangle containing, int ml, int mr)
        {
            if (style == null) return flowX;
            var x = flowX;
            if (style.Pn.Length == 0 || style.Pn == "s" || style.Pn == "k") return x + style.Tx;
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
            if (style.Pn.Length == 0 || style.Pn == "s" || style.Pn == "k") return y + style.Ty;
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
            return OutOfFlow(style) && !HasWidth(style) && style.Lt.HasValue && style.Rt.HasValue;
        }

        private static bool StretchesHeight(S style, int reference)
        {
            return OutOfFlow(style) && !HasHeight(style, reference) && style.Tp.HasValue && style.Bm.HasValue;
        }

        private static bool OutOfFlow(S style)
        {
            return style != null && (style.Pn == "a" || style.Pn == "f");
        }

        private static bool Positioned(S style)
        {
            return style != null && (style.Pn == "r" || style.Pn == "a" || style.Pn == "f" || style.Pn == "k");
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
            return style != null && (style.Zc || style.Pn == "f" || style.Pn == "k" || (Positioned(style) && style.Za));
        }

        private void ApplySticky()
        {
            lock (layoutGate) ApplyStickyCore();
        }

        private void ApplyStickyCore()
        {
            if (!sticky) return;
            foreach (var box in boxes) box.Reset();

            var offsets = new Dictionary<B, int>();
            var visiting = new HashSet<B>();
            foreach (var box in BoxSnapshot())
            {
                if (box.Fx) continue;
                var offset = StickyOffset(box.B, offsets, visiting);
                if (offset != 0) box.Move(offset);
            }
            foreach (var box in boxes) box.Clip = StickyClip(box);
        }

        private int StickyOffset(B block, IDictionary<B, int> offsets, ISet<B> visiting)
        {
            if (block == null || FixedBlock(block)) return 0;
            int found;
            if (offsets.TryGetValue(block, out found)) return found;
            if (!visiting.Add(block)) return 0;

            var parentOffset = StickyOffset(block.Up, offsets, visiting);
            var offset = parentOffset;
            Bx owner;
            var style = block.S;
            if (style != null && style.Pn == "k" && boxOwners.TryGetValue(block, out owner) && (style.Tp.HasValue || style.Bm.HasValue))
            {
                var normalTop = owner.R0.Top + parentOffset;
                var desired = normalTop;
                var height = owner.R0.Height;
                if (style.Tp.HasValue && style.Bm.HasValue)
                {
                    var low = sy + style.Tp.Value;
                    var high = sy + ClientSize.Height - style.Bm.Value - height;
                    desired = low > high ? low : Math.Max(low, Math.Min(high, desired));
                }
                else if (style.Tp.HasValue) desired = Math.Max(desired, sy + style.Tp.Value);
                else desired = Math.Min(desired, sy + ClientSize.Height - style.Bm.Value - height);

                var container = new Rectangle(0, 0, Math.Max(1, ClientSize.Width), Math.Max(1, docHeight));
                Bx parent;
                if (block.Up != null && boxOwners.TryGetValue(block.Up, out parent))
                {
                    container = parent.C0;
                    container.Offset(0, StickyOffset(block.Up, offsets, visiting));
                }
                var maximum = Math.Max(container.Top, container.Bottom - height);
                desired = Math.Max(container.Top, Math.Min(maximum, desired));
                offset = parentOffset + desired - normalTop;
            }
            visiting.Remove(block);
            offsets[block] = offset;
            return offset;
        }

        private Rectangle StickyClip(Bx box)
        {
            var clip = Rectangle.Empty;
            for (var block = box == null ? null : box.B; block != null; block = block.Up)
            {
                Bx owner;
                if (boxOwners.TryGetValue(block, out owner) && !Object.ReferenceEquals(owner, box) && ClipsOverflow(owner.S))
                    clip = CombineClip(clip, owner.C);
                if (block.S != null && block.S.Pn == "f") break;
            }
            return clip;
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
                var bt = style == null ? 0 : BorderWidth(style, 0);
                var br = style == null ? 0 : BorderWidth(style, 1);
                var bb = style == null ? 0 : BorderWidth(style, 2);
                var bl = style == null ? 0 : BorderWidth(style, 3);
                var pt = style == null ? 0 : style.Pt;
                var pr = style == null ? 0 : style.Pr;
                var pb = style == null ? 0 : style.Pb;
                var pl = style == null ? 0 : style.Pl;
                var available = ConstrainWidth(style, Math.Max(1, content.Width - ml - mr - bl - br - pl - pr), content.Width);
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
                if (style != null && style.H == 0 && PreferredRatio(part)) textHeight = RatioPreferredHeight(part, available, textHeight);
                textHeight = ConstrainHeight(style, textHeight);
                y += mt;
                var outer = new Rectangle(content.X + ml, y, available + pl + pr + bl + br, textHeight + pt + pb + bt + bb);
                var inner = new Rectangle(outer.X + bl + pl, outer.Y + bt + pt, available, textHeight);
                var ownerBox = new Bx(part.K, flow == null ? CellText(part) : "", flow == null && part.K == "a" ? part.U : null, style, outer, inner, false, part);
                boxes.Add(ownerBox);
                boxOwners[part] = ownerBox;
                if (flow != null) AddInlineBoxes(part, inner, flow);
                y += outer.Height + mb + CellGap(part.K);
            }
        }

        private int CellPartHeight(Graphics graphics, B part, int width)
        {
            var style = part.S;
            var horizontal = style == null ? 0 : style.Ml + style.Mr + style.Pl + style.Pr + BorderWidth(style, 3) + BorderWidth(style, 1);
            var available = ConstrainWidth(style, Math.Max(1, width - horizontal), width);
            int height;
            if (part.Vm != null)
                using (var font = BoxFont(part.K, style, false)) height = GaugeHeight(font);
            else if (part.K != "br" && NeedsFlow(part)) InlineLayout(graphics, part, available, out height);
            else using (var font = BoxFont(part.K, style, false)) height = part.K == "br" ? Math.Max(4, font.Height / 2) : Measure(graphics, CellText(part), font, available);
            if (style != null && style.H == 0 && PreferredRatio(part)) height = RatioPreferredHeight(part, available, height);
            height = ConstrainHeight(style, height);
            if (style != null) height += style.Mt + style.Mb + style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2);
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
            if (String.IsNullOrEmpty(block.Mk)) return text;
            var marker = block.Mk;
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
            var top = style == null ? 0 : BorderWidth(style, 0) + style.Pt;
            var right = style == null ? 0 : BorderWidth(style, 1) + style.Pr;
            var bottom = style == null ? 0 : BorderWidth(style, 2) + style.Pb;
            var left = style == null ? 0 : BorderWidth(style, 3) + style.Pl;
            if (top + right + bottom + left == 0) return new Padding(6);
            return new Padding(left, top, right, bottom);
        }

        private static int Measure(Graphics graphics, string text, Font font, int width)
        {
            var size = TextRenderer.MeasureText(graphics, text ?? "", font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            return Math.Max(font.Height, size.Height);
        }

        private int GridFitWidth(Graphics graphics, B block, S style, int available, int reference)
        {
            var text = MarkedText(block);
            if (block == null || block.I != null || block.Vm != null || block.K == "img" || text.Length == 0 || NeedsFlow(block)) return available;
            using (var font = BoxFont(block.K, style, false))
            {
                var measured = TextRenderer.MeasureText(graphics, text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                var minimum = !HasMinWidth(style) ? 1 : Math.Min(available, Math.Max(1, ResolvedWidth(style, 1, reference)));
                return Math.Max(minimum, Math.Min(available, Math.Max(1, measured)));
            }
        }

        private static int GaugeWidth(S style, int available, Font font)
        {
            return HasWidth(style) ? available : Math.Min(available, Math.Max(80, font.Height * 10));
        }

        private static int GaugeHeight(Font font)
        {
            return Math.Max(14, font.Height);
        }

        private int ConstrainWidth(S style, int available)
        {
            return ConstrainWidth(style, available, available);
        }

        private int ConstrainWidth(S style, int available, int reference)
        {
            available = Math.Max(1, available);
            reference = Math.Max(1, reference);
            var width = HasWidth(style) ? Math.Min(available, Math.Max(1, ResolvedWidth(style, 0, reference))) : available;
            if (HasMaxWidth(style)) width = Math.Min(width, Math.Max(1, ResolvedWidth(style, 2, reference)));
            if (HasMinWidth(style)) width = Math.Max(width, Math.Min(available, Math.Max(1, ResolvedWidth(style, 1, reference))));
            return Math.Max(1, width);
        }

        private int ConstrainHeight(S style, int natural)
        {
            return ConstrainHeight(style, natural, 0);
        }

        private int ConstrainHeight(S style, int natural, int reference)
        {
            var height = HasHeight(style, reference) ? ResolvedHeight(style, 0, reference) : Math.Max(1, natural);
            if (HasMaxHeight(style, reference)) height = Math.Min(height, Math.Max(1, ResolvedHeight(style, 2, reference)));
            if (HasMinHeight(style, reference)) height = Math.Max(height, Math.Max(1, ResolvedHeight(style, 1, reference)));
            return Math.Max(1, height);
        }

        private int ConstrainResolvedWidth(S style, int desired, int available, int reference)
        {
            var width = Math.Min(Math.Max(1, available), Math.Max(1, desired));
            if (HasMaxWidth(style)) width = Math.Min(width, Math.Max(1, ResolvedWidth(style, 2, reference)));
            if (HasMinWidth(style)) width = Math.Max(width, Math.Min(Math.Max(1, available), Math.Max(1, ResolvedWidth(style, 1, reference))));
            return Math.Max(1, width);
        }

        private static bool HasWidth(S style)
        {
            return style != null && (style.W > 0 || style.Wp > 0 || style.Wk > 0);
        }

        private static bool HasMinWidth(S style)
        {
            return style != null && (style.Wn > 0 || style.Wnp > 0 || style.Wnk > 0);
        }

        private static bool HasMaxWidth(S style)
        {
            return style != null && (style.Wx > 0 || style.Wxp > 0 || style.Wxk > 0);
        }

        private static bool HasHeight(S style, int reference)
        {
            if (style == null) return false;
            if (style.Hk > 0) return reference > 0 || !FunctionHasPercentage(style, true, 0);
            return style.H > 0 || style.Hp > 0 && (style.Hu > 0 || reference > 0);
        }

        private static bool HasMinHeight(S style, int reference)
        {
            if (style == null) return false;
            if (style.Hnk > 0) return reference > 0 || !FunctionHasPercentage(style, true, 1);
            return style.Hn > 0 || style.Hnp > 0 && (style.Hnu > 0 || reference > 0);
        }

        private static bool HasMaxHeight(S style, int reference)
        {
            if (style == null) return false;
            if (style.Hxk > 0) return reference > 0 || !FunctionHasPercentage(style, true, 2);
            return style.Hx > 0 || style.Hxp > 0 && (style.Hxu > 0 || reference > 0);
        }

        private int ResolvedWidth(S style, int part, int reference)
        {
            return BoxContentWidth(style, ResolvedSize(style, false, part, reference));
        }

        private int ResolvedHeight(S style, int part, int reference)
        {
            return BoxContentHeight(style, ResolvedSize(style, true, part, reference));
        }

        private int ResolvedSize(S style, bool height, int part, int reference)
        {
            var kind = height ? HeightFunctionKind(style, part) : WidthFunctionKind(style, part);
            int declared;
            if (kind == 0)
            {
                var pixels = height ? part == 0 ? style.H : part == 1 ? style.Hn : style.Hx : part == 0 ? style.W : part == 1 ? style.Wn : style.Wx;
                var magnitude = height ? part == 0 ? style.Hp : part == 1 ? style.Hnp : style.Hxp : part == 0 ? style.Wp : part == 1 ? style.Wnp : style.Wxp;
                var offset = height ? part == 0 ? style.Ho : part == 1 ? style.Hno : style.Hxo : part == 0 ? style.Wo : part == 1 ? style.Wno : style.Wxo;
                var unit = height ? part == 0 ? style.Hu : part == 1 ? style.Hnu : style.Hxu : part == 0 ? style.Wu : part == 1 ? style.Wnu : style.Wxu;
                declared = magnitude > 0 ? RelativePixels(unit, magnitude, reference, style) + offset : pixels;
            }
            else
            {
                var count = height ? style.Hq(part) : style.Wq(part);
                declared = FunctionTerm(style, height, part, 0, reference);
                if (kind == 1)
                    for (var argument = 1; argument < count; argument++) declared = Math.Min(declared, FunctionTerm(style, height, part, argument, reference));
                else if (kind == 2)
                    for (var argument = 1; argument < count; argument++) declared = Math.Max(declared, FunctionTerm(style, height, part, argument, reference));
                else if (kind == 3 && count == 3)
                    declared = Math.Max(declared, Math.Min(FunctionTerm(style, height, part, 1, reference), FunctionTerm(style, height, part, 2, reference)));
            }
            return Math.Max(0, Math.Min(8192, declared));
        }

        private int FunctionTerm(S style, bool height, int part, int argument, int reference)
        {
            var pixels = height ? style.Hv(part, argument, 0) : style.Wv(part, argument, 0);
            var magnitude = height ? style.Hv(part, argument, 1) : style.Wv(part, argument, 1);
            var offset = height ? style.Hv(part, argument, 2) : style.Wv(part, argument, 2);
            var unit = height ? style.Hv(part, argument, 3) : style.Wv(part, argument, 3);
            return magnitude > 0 ? RelativePixels(unit, magnitude, reference, style) + offset : pixels;
        }

        private static bool FunctionHasPercentage(S style, bool height, int part)
        {
            var count = height ? style.Hq(part) : style.Wq(part);
            for (var argument = 0; argument < count; argument++)
            {
                var magnitude = height ? style.Hv(part, argument, 1) : style.Wv(part, argument, 1);
                var unit = height ? style.Hv(part, argument, 3) : style.Wv(part, argument, 3);
                if (magnitude > 0 && unit == 0) return true;
            }
            return false;
        }

        private static int WidthFunctionKind(S style, int part)
        {
            return part == 0 ? style.Wk : part == 1 ? style.Wnk : style.Wxk;
        }

        private static int HeightFunctionKind(S style, int part)
        {
            return part == 0 ? style.Hk : part == 1 ? style.Hnk : style.Hxk;
        }

        private int SizeReference(int unit, int reference, S style)
        {
            if (unit == 1) return layoutViewportWidth;
            if (unit == 2) return layoutViewportHeight;
            if (unit == 3) return Math.Min(layoutViewportWidth, layoutViewportHeight);
            if (unit == 4) return Math.Max(layoutViewportWidth, layoutViewportHeight);
            if (unit == 5) return style != null && style.Fs > 0 ? style.Fs : Math.Max(1, (int)Math.Round(Font.Size));
            if (unit == 6) return layoutRootFontSize;
            return Math.Max(1, reference);
        }

        private int RelativePixels(int unit, int magnitude, int reference, S style)
        {
            var scale = SizeReference(unit, reference, style);
            return (int)Math.Round(unit >= 5 ? scale * (double)magnitude : scale * magnitude / 100D);
        }

        private bool PreferredRatio(B block)
        {
            var style = block == null ? null : block.S;
            if (!HasRatio(style)) return false;
            if (!style.Ra) return true;
            Image image;
            if (block.K == "img" && block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0) return false;
            return block.Vg == null || block.Vg.W <= 0 || block.Vg.H <= 0;
        }

        private static bool HasRatio(S style)
        {
            return style != null && style.Rw > 0D && style.Rh > 0D;
        }

        private static int RatioPreferredHeight(B block, int width, int natural)
        {
            var preferred = RatioContentHeight(block.S, width);
            return block.K == "img" || block.Vg != null ? preferred : Math.Max(Math.Max(1, natural), preferred);
        }

        private static int RatioContentWidth(S style, int contentHeight)
        {
            if (!HasRatio(style)) return Math.Max(1, contentHeight);
            var horizontal = style.Bz == "b" ? style.Pl + style.Pr + BorderWidth(style, 3) + BorderWidth(style, 1) : 0;
            var vertical = style.Bz == "b" ? style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2) : 0;
            var boxHeight = Math.Max(1, contentHeight + vertical);
            var boxWidth = boxHeight * style.Rw / style.Rh;
            return Math.Max(1, Math.Min(8192, (int)Math.Round(boxWidth) - horizontal));
        }

        private static int RatioContentHeight(S style, int contentWidth)
        {
            if (!HasRatio(style)) return Math.Max(1, contentWidth);
            var horizontal = style.Bz == "b" ? style.Pl + style.Pr + BorderWidth(style, 3) + BorderWidth(style, 1) : 0;
            var vertical = style.Bz == "b" ? style.Pt + style.Pb + BorderWidth(style, 0) + BorderWidth(style, 2) : 0;
            var boxWidth = Math.Max(1, contentWidth + horizontal);
            var boxHeight = boxWidth * style.Rh / style.Rw;
            return Math.Max(1, Math.Min(8192, (int)Math.Round(boxHeight) - vertical));
        }

        private int ConstrainFlexHeight(S style, int allocated, int reference)
        {
            var height = Math.Max(1, allocated);
            if (HasMaxHeight(style, reference)) height = Math.Min(height, Math.Max(1, ResolvedHeight(style, 2, reference)));
            if (HasMinHeight(style, reference)) height = Math.Max(height, Math.Max(1, ResolvedHeight(style, 1, reference)));
            return Math.Max(1, height);
        }

        private static int BoxContentWidth(S style, int declared)
        {
            if (declared <= 0 || style == null || style.Bz != "b") return Math.Max(0, declared);
            return Math.Max(0, declared - style.Pl - style.Pr - BorderWidth(style, 3) - BorderWidth(style, 1));
        }

        private static int BoxContentHeight(S style, int declared)
        {
            if (declared <= 0 || style == null || style.Bz != "b") return Math.Max(0, declared);
            return Math.Max(0, declared - style.Pt - style.Pb - BorderWidth(style, 0) - BorderWidth(style, 2));
        }

        private List<Ix> InlineLayout(Graphics graphics, B block, int width, out int height)
        {
            int baseHeight;
            using (var font = BoxFont(block.K, block.S, false)) baseHeight = CssLineHeight(block.S, font.Height);
            var indent = CssTextIndent(block.S, width);
            var firstWidth = Math.Max(1, width - indent);
            var lines = BuildInlineLines(graphics, block, width, firstWidth, baseHeight);
            if (block.S != null && block.S.Wt == "b" && block.S.Wm != "n")
                lines = BalanceInlineLines(graphics, block, width, indent, baseHeight, lines);
            var clamped = block.S != null && block.S.Lc > 0 && lines.Count > block.S.Lc;
            if (clamped) lines.RemoveRange(block.S.Lc, lines.Count - block.S.Lc);

            var layout = new List<Ix>();
            var y = 0;
            var rtl = InlineReverse(block.S);
            var outside = OutsideListMarker(block) ? ListMarkerAtoms(graphics, block) : new List<Ia>();
            var outsideWidth = InlineAtomsWidth(outside);
            var markerGap = outside.Count == 0 ? 0 : Math.Max(2, InlineAtom(graphics, block.K, " ", null, block.Ms ?? block.S, block.K, block.El).W);
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                if (lineIndex == 0 && outside.Count > 0)
                {
                    var markerX = rtl ? width + markerGap + outsideWidth : -outsideWidth - markerGap;
                    Ia markerPrevious = null;
                    foreach (var atom in outside)
                    {
                        var gap = markerPrevious == null ? 0 : InlineGap(markerPrevious, atom);
                        if (rtl) markerX -= gap + atom.W;
                        else markerX += gap;
                        var markerRect = new Rectangle(markerX, y + Math.Max(0, (lines[lineIndex].H - atom.H) / 2), atom.W, atom.H);
                        layout.Add(new Ix(atom.K, atom.X, atom.U, atom.S, markerRect, atom.El, atom.P));
                        if (!rtl) markerX += atom.W;
                        markerPrevious = atom;
                    }
                }
                var first = layout.Count;
                var current = lines[lineIndex];
                var lastLine = lineIndex == lines.Count - 1 || current.Br;
                var lastAlignment = lastLine && block.S != null && block.S.Tl.Length > 0;
                var align = TextAlignment(block.S, block.S == null ? "" : lastAlignment ? block.S.Tl : block.S.Ta);
                var justify = align == "j" && (lastAlignment || lineIndex < lines.Count - 1 && !current.Br);
                var spaces = 0;
                if (justify) foreach (var atom in current.As) if (atom.X == " ") spaces++;
                var available = lineIndex == 0 ? firstWidth : width;
                var offset = lineIndex == 0 && !rtl ? indent : 0;
                var stretch = spaces == 0 ? 0 : Math.Max(0, available - current.W);
                var each = spaces == 0 ? 0 : stretch / spaces;
                var remainder = spaces == 0 ? 0 : stretch % spaces;
                var lineWidth = current.W + (justify ? stretch : 0);
                var rightAligned = align == "r" || rtl && align == "j";
                var lineLeft = align == "c" ? Math.Max(0, (available - lineWidth) / 2) :
                    rightAligned ? rtl ? available - lineWidth : Math.Max(0, available - lineWidth) : 0;
                lineLeft += offset;
                var x = rtl ? lineLeft + lineWidth : lineLeft;
                Ia previous = null;
                foreach (var atom in current.As)
                {
                    var gap = previous == null ? 0 : InlineGap(previous, atom);
                    var atomWidth = atom.W;
                    if (justify && atom.X == " ")
                    {
                        atomWidth += each;
                        if (remainder-- > 0) atomWidth++;
                    }
                    if (rtl) x -= gap + atomWidth;
                    else x += gap;
                    var rect = new Rectangle(x, y + Math.Max(0, (current.H - atom.H) / 2), atomWidth, atom.H);
                    layout.Add(new Ix(atom.K, atom.X, atom.U, atom.S, rect, atom.El, atom.P));
                    if (!rtl) x += atomWidth;
                    previous = atom;
                }
                if (clamped && lineIndex == lines.Count - 1) ClampInlineLine(graphics, block, layout, first, width, y, current.H);
                else EllipsizeLine(graphics, block, layout, first, width, y, current.H);
                y += current.H;
            }
            height = Math.Max(baseHeight, y);
            return layout;
        }

        private List<Il> BuildInlineLines(Graphics graphics, B block, int width, int firstWidth, int baseHeight)
        {
            var lines = new List<Il>();
            var line = new Il();
            Ia pending = null;
            var endedWithBreak = false;
            var space = block.S == null ? "" : block.S.Ws;
            var wrap = block.S == null || block.S.Wm != "n";

            if (block.K == "summary" && !String.IsNullOrEmpty(block.Mk))
            {
                var marker = block.Mk;
                AddInlineWord(graphics, lines, ref line, ref pending, InlineAtom(graphics, block.K, marker, null, block.S, block.K, block.El), width, firstWidth, baseHeight, wrap, block.S == null ? "" : block.S.Wb);
                pending = InlineAtom(graphics, block.K, " ", null, block.S, block.K, block.El);
            }
            else if (block.K == "li" && !OutsideListMarker(block))
            {
                var markers = ListMarkerAtoms(graphics, block);
                if (markers.Count > 0)
                {
                    foreach (var marker in markers)
                        AddInlineWord(graphics, lines, ref line, ref pending, marker, width, firstWidth, baseHeight, wrap, block.S == null ? "" : block.S.Wb);
                    pending = InlineAtom(graphics, block.K, " ", null, block.Ms ?? block.S, block.K, block.El);
                }
            }

            if (block.Is.Count == 0)
            {
                AddInlineText(graphics, lines, ref line, ref pending, block.K, block.X, block.U, block.S, block.K, block.El, width, firstWidth, baseHeight, space, wrap, ref endedWithBreak);
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
                    if (run.P != null)
                    {
                        AddInlineWord(graphics, lines, ref line, ref pending,
                            GeneratedImageAtom(run.K, run.P, run.U, run.S, block.K, run.El),
                            width, firstWidth, baseHeight, wrap, run.S == null ? "" : run.S.Wb);
                        endedWithBreak = false;
                        continue;
                    }
                    AddInlineText(graphics, lines, ref line, ref pending, run.K, run.X, run.U, run.S, block.K, run.El, width, firstWidth, baseHeight, space, wrap, ref endedWithBreak);
                }
            }

            if (line.As.Count > 0 || lines.Count == 0 || endedWithBreak) FinishInlineLine(lines, ref line, baseHeight, true);
            return lines;
        }

        private List<Il> BalanceInlineLines(Graphics graphics, B block, int width, int indent, int baseHeight, List<Il> normal)
        {
            if (width < 2 || normal.Count < 2 || normal.Count > BalanceLineLimit || BalanceTextLength(block) > BalanceTextLimit) return normal;
            var target = normal.Count;
            var low = Math.Max(1, indent + 1);
            var high = width;
            List<Il> balanced = normal;
            while (low < high)
            {
                var middle = low + (high - low) / 2;
                var candidate = BuildInlineLines(graphics, block, middle, Math.Max(1, middle - indent), baseHeight);
                if (candidate.Count <= target)
                {
                    high = middle;
                    if (candidate.Count == target) balanced = candidate;
                }
                else low = middle + 1;
            }
            if (high < width)
            {
                var candidate = BuildInlineLines(graphics, block, high, Math.Max(1, high - indent), baseHeight);
                if (candidate.Count == target) balanced = candidate;
            }
            var firstWidth = Math.Max(1, width - indent);
            return InlineBalanceScore(balanced, width, firstWidth) < InlineBalanceScore(normal, width, firstWidth) ? balanced : normal;
        }

        private static int BalanceTextLength(B block)
        {
            if (block == null) return 0;
            if (block.Is.Count == 0) return block.X == null ? 0 : block.X.Length;
            var length = 0;
            foreach (var run in block.Is)
            {
                length += run.X == null ? 0 : run.X.Length;
                if (length > BalanceTextLimit) break;
            }
            return length;
        }

        private static long InlineBalanceScore(IList<Il> lines, int width, int firstWidth)
        {
            long sum = 0;
            long squares = 0;
            for (var index = 0; index < lines.Count; index++)
            {
                var remaining = Math.Max(0, (index == 0 ? firstWidth : width) - lines[index].W);
                sum += remaining;
                squares += (long)remaining * remaining;
            }
            return lines.Count * squares - sum * sum;
        }

        private void EllipsizeLine(Graphics graphics, B block, List<Ix> layout, int first, int width, int top, int height)
        {
            if (block.S == null || block.S.To != "e" || !ClipsOverflow(block.S) || first == layout.Count) return;
            var rtl = InlineReverse(block.S);
            var overflow = false;
            for (var index = first; index < layout.Count; index++)
                if (rtl ? layout[index].R.Left < 0 : layout[index].R.Right > width) { overflow = true; break; }
            if (!overflow) return;

            using (var markerFont = BoxFont(block.K, block.S, false, block.U != null))
            {
                var markerWidth = TextWidth(graphics, "\u2026", markerFont, block.S);
                var edge = rtl ? width : 0;
                S lastStyle = null;
                var lastWidth = 0;
                var stopped = false;
                // Keep layout and hit rectangles intact; only replace the painted source suffix.
                for (var index = first; index < layout.Count; index++)
                {
                    var part = layout[index];
                    if (stopped) { part.Px = ""; continue; }
                    var gap = TextGap(part.R.Width, part.S, block.S);
                    var fits = rtl ? part.R.Left - gap - markerWidth >= 0 : part.R.Right + gap + markerWidth <= width;
                    if (fits)
                    {
                        edge = rtl ? part.R.Left : part.R.Right;
                        lastStyle = part.S;
                        lastWidth = part.R.Width;
                        continue;
                    }
                    using (var font = BoxFont(part.K, part.S, false, part.U != null, block.K))
                    {
                        part.Px = EllipsisPrefix(graphics, part.X, font, part.S, block.S,
                            rtl ? part.R.Right : width - part.R.Left, markerWidth, index == first);
                        if (part.Px.Length > 0)
                        {
                            lastWidth = TextWidth(graphics, part.Px, font, part.S);
                            var left = rtl ? part.R.Right - lastWidth : part.R.Left;
                            part.C = new Rectangle(left, part.R.Top, lastWidth, part.R.Height);
                            edge = rtl ? left : left + lastWidth;
                            lastStyle = part.S;
                        }
                    }
                    stopped = true;
                }
                var finalGap = lastWidth > 0 ? TextGap(lastWidth, lastStyle, block.S) : 0;
                var x = rtl ? edge - finalGap - markerWidth : edge + finalGap;
                var rect = new Rectangle(x, top + Math.Max(0, (height - markerFont.Height) / 2), markerWidth, markerFont.Height);
                layout.Add(new Ix(block.K, "", block.U, block.S, rect, block.El) { Px = "\u2026" });
            }
        }

        private void ClampInlineLine(Graphics graphics, B block, List<Ix> layout, int first, int width, int top, int height)
        {
            var rtl = InlineReverse(block.S);
            using (var markerFont = BoxFont(block.K, block.S, false, block.U != null))
            {
                var markerWidth = TextWidth(graphics, "\u2026", markerFont, block.S);
                var edge = rtl ? width : 0;
                S lastStyle = null;
                var lastWidth = 0;
                var keep = layout.Count;
                for (var index = first; index < layout.Count; index++)
                {
                    var part = layout[index];
                    var gap = TextGap(part.R.Width, part.S, block.S);
                    var fits = rtl ? part.R.Left - gap - markerWidth >= 0 : part.R.Right + gap + markerWidth <= width;
                    if (fits)
                    {
                        edge = rtl ? part.R.Left : part.R.Right;
                        lastStyle = part.S;
                        lastWidth = part.R.Width;
                        continue;
                    }

                    using (var font = BoxFont(part.K, part.S, false, part.U != null, block.K))
                    {
                        part.Px = EllipsisPrefix(graphics, part.X, font, part.S, block.S,
                            rtl ? part.R.Right : width - part.R.Left, markerWidth, false);
                        if (part.Px.Length > 0)
                        {
                            lastWidth = TextWidth(graphics, part.Px, font, part.S);
                            var left = rtl ? part.R.Right - lastWidth : part.R.Left;
                            part.R = new Rectangle(left, part.R.Top, lastWidth, part.R.Height);
                            part.C = part.R;
                            edge = rtl ? left : left + lastWidth;
                            lastStyle = part.S;
                            keep = index + 1;
                        }
                        else keep = index;
                    }
                    break;
                }
                if (keep < layout.Count) layout.RemoveRange(keep, layout.Count - keep);
                var finalGap = lastWidth > 0 ? TextGap(lastWidth, lastStyle, block.S) : 0;
                var x = rtl ? edge - finalGap - markerWidth : edge + finalGap;
                x = Math.Max(0, Math.Min(Math.Max(0, width - markerWidth), x));
                var rect = new Rectangle(x, top + Math.Max(0, (height - markerFont.Height) / 2), markerWidth, markerFont.Height);
                layout.Add(new Ix(block.K, "", block.U, block.S, rect, block.El) { Px = "\u2026" });
            }
        }

        private static string EllipsisPrefix(Graphics graphics, string text, Font font, S style, S markerStyle, int available, int markerWidth, bool first)
        {
            var ends = new List<int> { 0 };
            var elements = StringInfo.GetTextElementEnumerator(text);
            while (ends.Count <= 8192 && elements.MoveNext())
                ends.Add(elements.ElementIndex + elements.GetTextElement().Length);
            var low = 0;
            var high = ends.Count - 1;
            while (low < high)
            {
                var middle = (low + high + 1) / 2;
                var measured = TextWidth(graphics, text.Substring(0, ends[middle]), font, style);
                if (measured + TextGap(measured, style, markerStyle) + markerWidth <= available) low = middle;
                else high = middle - 1;
            }
            if (first && low == 0 && ends.Count > 1) low = 1;
            return text.Substring(0, ends[low]);
        }

        private static bool EllipsisMarker(Bx box)
        {
            return box != null && box.X.Length == 0 && box.Px != null;
        }

        private void AddInlineText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, N element, int width, int firstWidth, int baseHeight, string space, bool wrap, ref bool endedWithBreak)
        {
            value = value ?? "";
            if (space == "p" || space == "pw")
            {
                AddPreservedText(graphics, lines, ref line, ref pending, kind, value, url, style, baseKind, element, width, firstWidth, baseHeight, wrap, ref endedWithBreak);
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
                    var spaceUrl = url != null && line.As.Count > 0 &&
                        Object.ReferenceEquals(LinkElement(line.As[line.As.Count - 1].El), LinkElement(element)) ? url : null;
                    pending = InlineAtom(graphics, kind, " ", spaceUrl, style, baseKind, element);
                    continue;
                }

                var start = index;
                while (index < value.Length && !Char.IsWhiteSpace(value[index])) index++;
                var word = InlineAtom(graphics, kind, value.Substring(start, index - start), url, style, baseKind, element);
                AddInlineWord(graphics, lines, ref line, ref pending, word, width, firstWidth, baseHeight, wrap, style == null ? "" : style.Wb);
                endedWithBreak = false;
            }
        }

        private void AddPreservedText(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, string kind, string value, Uri url, S style, string baseKind, N element, int width, int firstWidth, int baseHeight, bool wrap, ref bool endedWithBreak)
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
                AddInlineWord(graphics, lines, ref line, ref pending, atom, width, firstWidth, baseHeight, wrap, style == null ? "" : style.Wb);
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

        private void AddInlineWord(Graphics graphics, List<Il> lines, ref Il line, ref Ia pending, Ia word, int width, int firstWidth, int baseHeight, bool wrap, string mode)
        {
            if (!wrap)
            {
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            if (word.P != null)
            {
                var pictureAvailable = lines.Count == 0 ? firstWidth : width;
                var gap = PendingWidth(line, pending, word);
                if (line.As.Count > 0 && line.W + gap + word.W > pictureAvailable)
                {
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    pending = null;
                }
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            var breaking = String.IsNullOrEmpty(mode) ? "b" : mode;
            var available = lines.Count == 0 ? firstWidth : width;
            if (breaking == "n" || breaking == "k")
            {
                var gap = PendingWidth(line, pending, word);
                if (line.As.Count > 0 && line.W + gap + word.W > available)
                {
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    pending = null;
                }
                if (line.As.Count > 0 && pending != null) AddInlineAtom(line, pending);
                AddInlineAtom(line, word);
                pending = null;
                return;
            }

            if (breaking != "a" && word.W <= available)
            {
                var gap = PendingWidth(line, pending, word);
                if (line.As.Count > 0 && line.W + gap + word.W > available)
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
                available = lines.Count == 0 ? firstWidth : width;
                if (line.W + InlineGap(line.As[line.As.Count - 1], pending) + pending.W >= available) FinishInlineLine(lines, ref line, baseHeight, false);
                else AddInlineAtom(line, pending);
            }
            pending = null;
            var piece = new StringBuilder();
            var pieceWidth = 0;
            var previousWidth = 0;
            var elements = StringInfo.GetTextElementEnumerator(word.X);
            while (elements.MoveNext())
            {
                var text = elements.GetTextElement();
                var character = InlineAtom(graphics, word.K, text, word.U, word.S, word.Z, word.El);
                var gap = piece.Length == 0 ? 0 : TextGap(previousWidth, word.S, word.S);
                var leading = line.As.Count == 0 ? 0 : InlineGap(line.As[line.As.Count - 1], word);
                available = lines.Count == 0 ? firstWidth : width;
                if (piece.Length > 0 && line.W + leading + pieceWidth + gap + character.W > available)
                {
                    AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z, word.El));
                    FinishInlineLine(lines, ref line, baseHeight, false);
                    piece.Clear();
                    pieceWidth = 0;
                    gap = 0;
                }
                else if (piece.Length == 0 && line.As.Count > 0 && line.W + leading + character.W > available)
                    FinishInlineLine(lines, ref line, baseHeight, false);
                piece.Append(text);
                pieceWidth += gap + character.W;
                previousWidth = character.W;
            }
            if (piece.Length > 0) AddInlineAtom(line, InlineAtom(graphics, word.K, piece.ToString(), word.U, word.S, word.Z, word.El));
        }

        private Ia InlineAtom(Graphics graphics, string kind, string text, Uri url, S style, string baseKind, N element)
        {
            using (var font = BoxFont(kind, style, false, url != null, baseKind))
            {
                var width = TextWidth(graphics, text, font, style);
                return new Ia(kind, text, url, style, baseKind, Math.Max(1, width), font.Height, CssLineHeight(style, font.Height), element);
            }
        }

        private Ia GeneratedImageAtom(string kind, Uri source, Uri link, S style, string baseKind, N element)
        {
            Image image;
            if (source != null && pics.TryGetValue(source.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
            {
                var size = ListMarkerSize(image);
                return new Ia(kind, "", link, style, baseKind, size.Width, size.Height, size.Height, element, source);
            }
            var fallback = 16;
            using (var font = BoxFont(kind, style, false, link != null, baseKind)) fallback = Math.Max(1, Math.Min(64, CssLineHeight(style, font.Height)));
            return new Ia(kind, "", link, style, baseKind, fallback, fallback, fallback, element, source);
        }

        private List<Ia> ListMarkerAtoms(Graphics graphics, B block)
        {
            var result = new List<Ia>();
            if (block == null) return result;
            var markerStyle = block.Ms ?? block.S;
            var customContent = block.Ms != null && (block.Ms.Mc != null || block.Ms.Mf != null);
            if (customContent && block.Ms.Mf != null)
            {
                foreach (var fragment in block.Ms.Mf)
                {
                    if (fragment.K == "i" && fragment.U != null)
                        result.Add(GeneratedImageAtom("lm", fragment.U, null, markerStyle, block.K, block.El));
                    else if (!String.IsNullOrEmpty(fragment.X))
                        result.Add(InlineAtom(graphics, block.K, E.Tx(fragment.X, markerStyle), null, markerStyle, block.K, block.El));
                }
                return result;
            }
            var source = block.S == null || customContent ? null : block.S.Li;
            Image image;
            if (source != null && pics.TryGetValue(source.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
            {
                var size = ListMarkerSize(image);
                result.Add(new Ia("lm", "", source, markerStyle, block.K, size.Width, size.Height, size.Height, block.El));
                return result;
            }
            if (!String.IsNullOrEmpty(block.Mk)) result.Add(InlineAtom(graphics, block.K, block.Mk, null, markerStyle, block.K, block.El));
            return result;
        }

        private static int InlineAtomsWidth(IList<Ia> atoms)
        {
            var width = 0;
            Ia previous = null;
            foreach (var atom in atoms)
            {
                if (previous != null) width += InlineGap(previous, atom);
                width += atom.W;
                previous = atom;
            }
            return width;
        }

        private static Size ListMarkerSize(Image image)
        {
            const int limit = 64;
            var width = Math.Max(1, image.Width);
            var height = Math.Max(1, image.Height);
            if (width <= limit && height <= limit) return new Size(width, height);
            var scale = Math.Min(limit / (double)width, limit / (double)height);
            return new Size(Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
        }

        private static void AddInlineAtom(Il line, Ia atom)
        {
            if (line.As.Count > 0) line.W += InlineGap(line.As[line.As.Count - 1], atom);
            line.As.Add(atom);
            line.W += atom.W;
            line.H = Math.Max(line.H, atom.L);
        }

        private static int InlineGap(Ia left, Ia right)
        {
            return TextGap(left.W, left.S, right.S);
        }

        private static int PendingWidth(Il line, Ia pending, Ia word)
        {
            if (line.As.Count == 0) return 0;
            var last = line.As[line.As.Count - 1];
            return pending == null ? InlineGap(last, word) : InlineGap(last, pending) + pending.W + InlineGap(pending, word);
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
            return block != null && (HasListMarker(block) || block.Is.Count > 0 || (style != null && (style.Dr == "r" || style.Ta.Length > 0 || style.Tl.Length > 0 || style.Ti != 0 || style.Ts != null || CustomDecoration(style) || style.Ls != 0 || style.Wg != 0 || style.Lh > 0 || style.Lm > 0 || style.Ws.Length > 0 || style.Wm.Length > 0 || style.Wt.Length > 0 || style.Wb.Length > 0 || style.Ov.Length > 0 || style.Lc > 0)));
        }

        private static bool OutsideListMarker(B block)
        {
            return HasListMarker(block) && block.S.Lp == "o";
        }

        private static bool HasListMarker(B block)
        {
            if (block == null || block.K != "li" || block.S == null || block.Ms != null && block.Ms.Mn) return false;
            if (block.Ms != null && (block.Ms.Mc != null || block.Ms.Mf != null))
                return !String.IsNullOrEmpty(block.Ms.Mc) || !String.IsNullOrEmpty(block.Ms.Ma) || block.Ms.Mf != null && block.Ms.Mf.Count > 0;
            return !String.IsNullOrEmpty(block.Mk) || block.S.Li != null;
        }

        private static string TextAlignment(S style, string align)
        {
            if (align == "s" || String.IsNullOrEmpty(align)) return style != null && style.Dr == "r" ? "r" : "l";
            if (align == "e") return style != null && style.Dr == "r" ? "l" : "r";
            return align;
        }

        private static int CssTextIndent(S style, int width)
        {
            if (style == null || style.Ti == 0 || width < 1) return 0;
            var value = style.Tip ? (long)Math.Round(width * style.Ti / 100D) : style.Ti;
            return (int)Math.Max(-width, Math.Min(width - 1L, value));
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
                var paint = part.C;
                rect.Offset(content.X, content.Y);
                paint.Offset(content.X, content.Y);
                boxes.Add(new Bx(part.K, part.X, part.U, part.S, rect, paint, false, block, true, clip, part.El, part.P) { Px = part.Px });
            }
        }

        private static bool ClipsOverflow(S style)
        {
            return style != null && style.Ov.Length > 0 && style.Ov != "v";
        }

        private static int BorderWidth(S style, int edge)
        {
            if (style == null) return 0;
            var borderStyle = style.Bsv(edge);
            return borderStyle == "" || borderStyle == "none" || borderStyle == "hidden" ? 0 :
                edge == 0 ? style.Bt : edge == 1 ? style.Br : edge == 2 ? style.Bb : style.Bl;
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
            var defaultUnderline = (kind == "a" || link) && decoration.Length == 0;
            var customDecoration = CustomDecoration(style, defaultUnderline);
            if ((decoration.IndexOf('u') >= 0 || defaultUnderline) && !customDecoration) fontStyle |= FontStyle.Underline;
            if (decoration.IndexOf('s') >= 0 && !customDecoration) fontStyle |= FontStyle.Strikeout;
            return new Font(name, size, fontStyle);
        }

        private static bool CustomDecoration(S style, bool defaultUnderline = false)
        {
            if (style == null) return false;
            var underline = style.Td.IndexOf('u') >= 0 || (defaultUnderline && style.Td.Length == 0);
            var strike = style.Td.IndexOf('s') >= 0;
            return (underline || strike) && (style.Tc.Length > 0 || style.Tds.Length > 0 || style.Tw > 0 || (underline && style.Uo != 0));
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

        private static Size VectorSize(B block, int width)
        {
            var vector = block == null ? null : block.Vg;
            if (vector == null || vector.W <= 0 || vector.H <= 0) return new Size(Math.Max(1, width), 150);
            var style = block.S;
            var explicitWidth = HasWidth(style);
            var explicitHeight = style != null && style.H > 0;
            var preferredRatio = HasRatio(style) && !style.Ra;
            var renderWidth = explicitWidth ? Math.Max(1, width) : Math.Min(Math.Max(1, width), Math.Max(1, (int)Math.Round(vector.W)));
            var renderHeight = explicitHeight ? Math.Max(1, BoxContentHeight(style, style.H)) : Math.Max(1, (int)Math.Round(renderWidth * vector.H / vector.W));
            if (!explicitWidth && explicitHeight)
                renderWidth = preferredRatio ? Math.Max(1, width) : Math.Min(Math.Max(1, width), Math.Max(1, (int)Math.Round(renderHeight * vector.W / vector.H)));
            return new Size(renderWidth, renderHeight);
        }

        private Size PicSize(B block, int width)
        {
            Image image;
            if (block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
            {
                var style = block.S;
                var explicitWidth = HasWidth(style);
                var explicitHeight = style != null && style.H > 0;
                var preferredRatio = PreferredRatio(block);
                var renderWidth = explicitWidth ? width : Math.Min(width, image.Width);
                var renderHeight = explicitHeight ? Math.Max(1, BoxContentHeight(style, style.H)) : Math.Max(1, (int)Math.Round(image.Height * (double)renderWidth / image.Width));
                if (!explicitWidth && !explicitHeight && renderHeight > 360)
                {
                    renderWidth = Math.Max(1, (int)Math.Round(renderWidth * 360D / renderHeight));
                    renderHeight = 360;
                }
                if (!explicitWidth && explicitHeight)
                    renderWidth = preferredRatio ? width : Math.Min(Math.Max(1, width), Math.Max(1, (int)Math.Round(renderHeight * (double)image.Width / image.Height)));
                return new Size(Math.Max(1, renderWidth), Math.Max(1, renderHeight));
            }
            return new Size(width, block.S != null && block.S.H > 0 ? Math.Max(1, BoxContentHeight(block.S, block.S.H)) : 72);
        }

        private void PaintBox(Graphics graphics, Rectangle rect, S style, Color pageBackground, Color pageColor, int opacity, bool background)
        {
            if (style == null) return;
            var rounded = HasRadius(style);
            var shadows = style.Sw != null && style.Sw.Count > 0;
            var smoothing = graphics.SmoothingMode;
            if (rounded || shadows) graphics.SmoothingMode = SmoothingMode.AntiAlias;
            try
            {
                if (shadows) PaintShadows(graphics, rect, style, CssColor(style.F, pageColor), opacity, false);
                if (background && style.Bg.Length > 0)
                    using (var brush = new SolidBrush(PaintColor(CssColor(style.Bg, pageBackground), opacity)))
                    {
                        if (rounded)
                            using (var path = RoundedPath(rect, style, 0, 0, 0, 0)) graphics.FillPath(brush, path);
                        else graphics.FillRectangle(brush, rect);
                    }
                if (background && style.Bi != null) PaintBitmapBackground(graphics, rect, style, opacity);
                if (background && style.Gi != null) PaintGradient(graphics, rect, style, CssColor(style.F, pageColor), opacity);
                if (background && shadows) PaintShadows(graphics, rect, style, CssColor(style.F, pageColor), opacity, true);

                var top = BorderWidth(style, 0);
                var right = BorderWidth(style, 1);
                var bottom = BorderWidth(style, 2);
                var left = BorderWidth(style, 3);
                if (top + right + bottom + left > 0)
                {
                    var currentColor = CssColor(style.F, pageColor);
                    var topStyle = style.Bsv(0);
                    var rightStyle = style.Bsv(1);
                    var bottomStyle = style.Bsv(2);
                    var leftStyle = style.Bsv(3);
                    var topColorText = style.Bcv(0);
                    var rightColorText = style.Bcv(1);
                    var bottomColorText = style.Bcv(2);
                    var leftColorText = style.Bcv(3);
                    var topColor = PaintColor(CssColor(topColorText, currentColor), opacity);
                    var rightColor = PaintColor(CssColor(rightColorText, currentColor), opacity);
                    var bottomColor = PaintColor(CssColor(bottomColorText, currentColor), opacity);
                    var leftColor = PaintColor(CssColor(leftColorText, currentColor), opacity);
                    var uniform = topStyle == rightStyle && topStyle == bottomStyle && topStyle == leftStyle &&
                        topColorText == rightColorText && topColorText == bottomColorText && topColorText == leftColorText;
                    if (rounded && uniform) PaintRoundedBorder(graphics, rect, style, top, right, bottom, left, topColor, topStyle);
                    else if (rounded)
                    {
                        PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 0, topColor, topStyle);
                        PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 1, rightColor, rightStyle);
                        PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 2, bottomColor, bottomStyle);
                        PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 3, leftColor, leftStyle);
                    }
                    else
                    {
                        PaintBorder(graphics, rect, top, 0, topColor, topStyle);
                        PaintBorder(graphics, rect, right, 1, rightColor, rightStyle);
                        PaintBorder(graphics, rect, bottom, 2, bottomColor, bottomStyle);
                        PaintBorder(graphics, rect, left, 3, leftColor, leftStyle);
                    }
                }
                PaintOutline(graphics, rect, style, CssColor(style.F, pageColor), opacity);
            }
            finally { if (rounded || shadows) graphics.SmoothingMode = smoothing; }
        }

        private static int OutlineExtent(S style)
        {
            return style == null || style.Ow < 1 || style.Os == "n" ? 0 : Math.Max(0, style.Oo) + Math.Min(32, style.Ow);
        }

        private static void PaintOutline(Graphics graphics, Rectangle rect, S style, Color currentColor, int opacity)
        {
            if (style == null || style.Ow < 1 || style.Os == "n" || rect.Width < 1 || rect.Height < 1) return;
            var width = Math.Max(1, Math.Min(32, style.Ow));
            var grow = style.Oo + (width + 1) / 2;
            var outline = rect;
            outline.Inflate(grow, grow);
            if (outline.Width < 2 || outline.Height < 2) return;
            var color = PaintColor(CssColor(style.Oc, currentColor), opacity);
            using (var pen = new Pen(color, width))
            {
                if (style.Os == "d") pen.DashStyle = DashStyle.Dash;
                else if (style.Os == "o" || style.Os == "a") pen.DashStyle = DashStyle.Dot;
                if (!HasRadius(style))
                {
                    graphics.DrawRectangle(pen, outline.X, outline.Y, Math.Max(1, outline.Width - 1), Math.Max(1, outline.Height - 1));
                    return;
                }
                var smoothing = graphics.SmoothingMode;
                try
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(outline, style, -grow, -grow, -grow, -grow)) graphics.DrawPath(pen, path);
                }
                finally { graphics.SmoothingMode = smoothing; }
            }
        }

        private static void PaintShadows(Graphics graphics, Rectangle rect, S style, Color currentColor, int opacity, bool inset)
        {
            if (style == null || style.Sw == null || style.Sw.Count == 0 || rect.Width < 1 || rect.Height < 1) return;
            using (var owner = RoundedPath(rect, style, 0, 0, 0, 0))
            {
                for (var index = style.Sw.Count - 1; index >= 0; index--)
                {
                    var shadow = style.Sw[index];
                    if (shadow == null || shadow.I != inset) continue;
                    var color = PaintColor(CssColor(shadow.C, currentColor), opacity);
                    if (color.A == 0) continue;
                    if (inset) PaintInsetShadow(graphics, rect, style, shadow, owner, color);
                    else PaintOuterShadow(graphics, rect, style, shadow, owner, color);
                }
            }
        }

        private static void PaintOuterShadow(Graphics graphics, Rectangle rect, S style, Sw shadow, GraphicsPath owner, Color color)
        {
            var steps = Math.Min(12, Math.Max(0, shadow.B));
            for (var step = steps; step > 0; step--)
            {
                var grow = shadow.R + (int)Math.Ceiling(shadow.B * step / (double)steps);
                var next = shadow.R + (int)Math.Ceiling(shadow.B * (step - 1) / (double)steps);
                using (var shape = ShadowPath(rect, style, shadow, grow, false))
                using (var inside = ShadowPath(rect, style, shadow, next, false))
                {
                    if (shape.PointCount == 0) continue;
                    using (var region = new Region(shape))
                    {
                        if (inside.PointCount > 0) region.Exclude(inside);
                        region.Exclude(owner);
                        FillShadow(graphics, region, color, steps - step + 1, steps + 1);
                    }
                }
            }
            using (var shape = ShadowPath(rect, style, shadow, shadow.R, false))
            {
                if (shape.PointCount == 0) return;
                using (var region = new Region(shape))
                {
                    region.Exclude(owner);
                    FillShadow(graphics, region, color, 1, 1);
                }
            }
        }

        private static void PaintInsetShadow(Graphics graphics, Rectangle rect, S style, Sw shadow, GraphicsPath owner, Color color)
        {
            using (var hole = ShadowPath(rect, style, shadow, shadow.R, true))
            using (var region = new Region(owner))
            {
                if (hole.PointCount > 0) region.Exclude(hole);
                FillShadow(graphics, region, color, 1, 1);
            }
            var steps = Math.Min(12, Math.Max(0, shadow.B));
            for (var step = 0; step < steps; step++)
            {
                var grow = shadow.R + (int)Math.Floor(shadow.B * step / (double)steps);
                var next = shadow.R + (int)Math.Floor(shadow.B * (step + 1) / (double)steps);
                using (var shape = ShadowPath(rect, style, shadow, grow, true))
                using (var inside = ShadowPath(rect, style, shadow, next, true))
                {
                    if (shape.PointCount == 0) continue;
                    using (var region = new Region(shape))
                    {
                        if (inside.PointCount > 0) region.Exclude(inside);
                        region.Intersect(owner);
                        FillShadow(graphics, region, color, steps - step, steps + 1);
                    }
                }
            }
        }

        private static GraphicsPath ShadowPath(Rectangle rect, S style, Sw shadow, int grow, bool inset)
        {
            var shape = rect;
            shape.Inflate(inset ? -grow : grow, inset ? -grow : grow);
            shape.Offset(shadow.X, shadow.Y);
            return shape.Width < 1 || shape.Height < 1 ? new GraphicsPath() :
                RoundedPath(shape, style, inset ? grow : -grow, inset ? grow : -grow, inset ? grow : -grow, inset ? grow : -grow);
        }

        private static void FillShadow(Graphics graphics, Region region, Color color, int alphaPart, int alphaWhole)
        {
            var alpha = Math.Max(0, Math.Min(255, (int)Math.Round(color.A * alphaPart / (double)Math.Max(1, alphaWhole))));
            if (alpha == 0) return;
            using (var brush = new SolidBrush(Color.FromArgb(alpha, color.R, color.G, color.B))) graphics.FillRegion(brush, region);
        }

        private void PaintBitmapBackground(Graphics graphics, Rectangle rect, S style, int opacity)
        {
            if (style == null || style.Bi == null || style.Bi.U == null || rect.Width < 1 || rect.Height < 1 || opacity <= 0) return;
            Image image;
            if (!pics.TryGetValue(style.Bi.U.AbsoluteUri, out image) || image == null || image.Width < 1 || image.Height < 1) return;
            var size = BackgroundImageSize(style.Bi, image, rect.Size);
            if (size.Width < 1 || size.Height < 1) return;
            var left = BackgroundImageOffset(rect.Left, rect.Width - size.Width, style.Bi.X, style.Bi.Xp);
            var top = BackgroundImageOffset(rect.Top, rect.Height - size.Height, style.Bi.Y, style.Bi.Yp);
            var state = graphics.Save();
            var interpolation = graphics.InterpolationMode;
            var offset = graphics.PixelOffsetMode;
            try
            {
                graphics.SetClip(rect, CombineMode.Intersect);
                if (HasRadius(style))
                    using (var path = RoundedPath(rect, style, 0, 0, 0, 0)) graphics.SetClip(path, CombineMode.Intersect);
                var tiny = image.Width < 4 || image.Height < 4;
                graphics.InterpolationMode = tiny ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = tiny ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                if (style.Bi.R == "n")
                {
                    PaintImage(graphics, image, new Rectangle(left, top, size.Width, size.Height), opacity);
                    return;
                }
                using (var attributes = BackgroundImageAttributes(opacity))
                using (var brush = new TextureBrush(image, new Rectangle(0, 0, image.Width, image.Height), attributes))
                using (var matrix = new Matrix(size.Width / (float)image.Width, 0F, 0F, size.Height / (float)image.Height, left, top))
                {
                    brush.Transform = matrix;
                    Rectangle fill;
                    if (style.Bi.R == "x") fill = Rectangle.Intersect(rect, new Rectangle(rect.Left, top, rect.Width, size.Height));
                    else if (style.Bi.R == "y") fill = Rectangle.Intersect(rect, new Rectangle(left, rect.Top, size.Width, rect.Height));
                    else fill = rect;
                    if (!fill.IsEmpty) graphics.FillRectangle(brush, fill);
                }
            }
            finally
            {
                graphics.InterpolationMode = interpolation;
                graphics.PixelOffsetMode = offset;
                graphics.Restore(state);
            }
        }

        private static Size BackgroundImageSize(Bi bitmap, Image image, Size box)
        {
            var sourceWidth = Math.Max(1, image.Width);
            var sourceHeight = Math.Max(1, image.Height);
            double width;
            double height;
            if (bitmap.K == "c" || bitmap.K == "n")
            {
                var horizontal = box.Width / (double)sourceWidth;
                var vertical = box.Height / (double)sourceHeight;
                var scale = bitmap.K == "c" ? Math.Max(horizontal, vertical) : Math.Min(horizontal, vertical);
                width = sourceWidth * scale;
                height = sourceHeight * scale;
            }
            else
            {
                width = bitmap.Wa ? 0D : bitmap.Wp ? box.Width * bitmap.W / 100D : bitmap.W;
                height = bitmap.Ha ? 0D : bitmap.Hp ? box.Height * bitmap.H / 100D : bitmap.H;
                if (bitmap.Wa && bitmap.Ha) { width = sourceWidth; height = sourceHeight; }
                else if (bitmap.Wa) width = height * sourceWidth / sourceHeight;
                else if (bitmap.Ha) height = width * sourceHeight / sourceWidth;
            }
            if (width <= 0D || height <= 0D || Double.IsNaN(width) || Double.IsNaN(height) || Double.IsInfinity(width) || Double.IsInfinity(height)) return Size.Empty;
            return new Size(Math.Max(1, Math.Min(8192, (int)Math.Round(width))), Math.Max(1, Math.Min(8192, (int)Math.Round(height))));
        }

        private static int BackgroundImageOffset(int start, int free, int value, bool percent)
        {
            var offset = percent ? free * (double)value / 100D : value;
            if (offset > Int32.MaxValue - (double)start) return Int32.MaxValue;
            if (offset < Int32.MinValue - (double)start) return Int32.MinValue;
            return start + (int)Math.Round(offset);
        }

        private static ImageAttributes BackgroundImageAttributes(int opacity)
        {
            var attributes = new ImageAttributes();
            attributes.SetWrapMode(WrapMode.Tile);
            if (opacity < 100)
            {
                var matrix = new ColorMatrix();
                matrix.Matrix33 = Math.Max(0, Math.Min(100, opacity)) / 100F;
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            }
            return attributes;
        }

        private static void PaintGradient(Graphics graphics, Rectangle rect, S style, Color currentColor, int opacity)
        {
            if (style == null || style.Gi == null || style.Gi.Ss == null || style.Gi.Ss.Count < 2 || rect.Width < 1 || rect.Height < 1) return;
            var blend = GradientBlend(style.Gi, currentColor, opacity);
            if (blend == null || blend.Colors.Length < 2) return;
            using (var brush = GradientBrush(style.Gi, rect, blend))
            {
                if (brush == null) return;
                if (HasRadius(style))
                    using (var path = RoundedPath(rect, style, 0, 0, 0, 0)) graphics.FillPath(brush, path);
                else graphics.FillRectangle(brush, rect);
            }
        }

        private static Brush GradientBrush(Gi gradient, Rectangle rect, ColorBlend blend)
        {
            if (gradient.K == "l")
            {
                var brush = new LinearGradientBrush(rect, blend.Colors[0], blend.Colors[blend.Colors.Length - 1], gradient.A - 90F, true);
                brush.InterpolationColors = blend;
                return brush;
            }
            var centerX = rect.Left + rect.Width * Math.Max(0, Math.Min(100, gradient.X)) / 100F;
            var centerY = rect.Top + rect.Height * Math.Max(0, Math.Min(100, gradient.Y)) / 100F;
            var dx = Math.Max(Math.Abs(centerX - rect.Left), Math.Abs(rect.Right - centerX));
            var dy = Math.Max(Math.Abs(centerY - rect.Top), Math.Abs(rect.Bottom - centerY));
            var radius = Math.Max(1F, (float)Math.Sqrt(dx * dx + dy * dy));
            using (var circle = new GraphicsPath())
            {
                circle.AddEllipse(centerX - radius, centerY - radius, radius * 2F, radius * 2F);
                var brush = new PathGradientBrush(circle);
                brush.CenterPoint = new PointF(centerX, centerY);
                brush.CenterColor = blend.Colors[0];
                brush.SurroundColors = new[] { blend.Colors[blend.Colors.Length - 1] };
                brush.InterpolationColors = ReverseGradientBlend(blend);
                return brush;
            }
        }

        private static ColorBlend ReverseGradientBlend(ColorBlend source)
        {
            var count = source.Colors.Length;
            var positions = new float[count];
            var colors = new Color[count];
            for (var index = 0; index < count; index++)
            {
                positions[index] = 1F - source.Positions[count - index - 1];
                colors[index] = source.Colors[count - index - 1];
            }
            positions[0] = 0F;
            positions[count - 1] = 1F;
            return new ColorBlend(count) { Positions = positions, Colors = colors };
        }

        private static ColorBlend GradientBlend(Gi gradient, Color currentColor, int opacity)
        {
            var positions = new List<float>();
            var colors = new List<Color>();
            foreach (var stop in gradient.Ss)
            {
                var position = Math.Max(0F, Math.Min(1F, stop.P / 100F));
                var color = PaintColor(CssColor(stop.C, currentColor), opacity);
                if (positions.Count > 0 && position <= positions[positions.Count - 1])
                {
                    var previous = positions[positions.Count - 1];
                    if (previous >= 1F) { colors[colors.Count - 1] = color; continue; }
                    position = Math.Min(1F, previous + .0001F);
                }
                positions.Add(position);
                colors.Add(color);
            }
            if (positions.Count < 2) return null;
            if (positions[0] > 0F) { positions.Insert(0, 0F); colors.Insert(0, colors[0]); }
            else positions[0] = 0F;
            if (positions[positions.Count - 1] < 1F) { positions.Add(1F); colors.Add(colors[colors.Count - 1]); }
            else positions[positions.Count - 1] = 1F;
            return new ColorBlend(positions.Count) { Positions = positions.ToArray(), Colors = colors.ToArray() };
        }

        private static bool HasRadius(S style)
        {
            return style != null && (style.R0 != 0 || style.R1 != 0 || style.R2 != 0 || style.R3 != 0 ||
                style.Q0 != 0 || style.Q1 != 0 || style.Q2 != 0 || style.Q3 != 0);
        }

        private static GraphicsPath RoundedPath(Rectangle rect, S style, int left, int top, int right, int bottom)
        {
            var x = new float[]
            {
                Math.Max(0F, Radius(style.R0, rect.Width) - left),
                Math.Max(0F, Radius(style.R1, rect.Width) - right),
                Math.Max(0F, Radius(style.R2, rect.Width) - right),
                Math.Max(0F, Radius(style.R3, rect.Width) - left)
            };
            var y = new float[]
            {
                Math.Max(0F, Radius(style.Q0, rect.Height) - top),
                Math.Max(0F, Radius(style.Q1, rect.Height) - top),
                Math.Max(0F, Radius(style.Q2, rect.Height) - bottom),
                Math.Max(0F, Radius(style.Q3, rect.Height) - bottom)
            };
            NormalizeRadii(rect.Width, rect.Height, x, y);
            var path = new GraphicsPath();
            if (rect.Width < 1 || rect.Height < 1) return path;
            path.StartFigure();
            path.AddLine(rect.Left + x[0], rect.Top, rect.Right - x[1], rect.Top);
            AddCorner(path, rect.Right - 2F * x[1], rect.Top, x[1], y[1], 270F);
            path.AddLine(rect.Right, rect.Top + y[1], rect.Right, rect.Bottom - y[2]);
            AddCorner(path, rect.Right - 2F * x[2], rect.Bottom - 2F * y[2], x[2], y[2], 0F);
            path.AddLine(rect.Right - x[2], rect.Bottom, rect.Left + x[3], rect.Bottom);
            AddCorner(path, rect.Left, rect.Bottom - 2F * y[3], x[3], y[3], 90F);
            path.AddLine(rect.Left, rect.Bottom - y[3], rect.Left, rect.Top + y[0]);
            AddCorner(path, rect.Left, rect.Top, x[0], y[0], 180F);
            path.CloseFigure();
            return path;
        }

        private static void AddCorner(GraphicsPath path, float x, float y, float radiusX, float radiusY, float start)
        {
            if (radiusX > 0F && radiusY > 0F) path.AddArc(x, y, radiusX * 2F, radiusY * 2F, start, 90F);
        }

        private static float Radius(int value, int length)
        {
            return value < 0 ? Math.Min(10000F, -value * Math.Max(0, length) / 100F) : Math.Min(2000F, value);
        }

        private static void NormalizeRadii(int width, int height, float[] x, float[] y)
        {
            var factor = 1F;
            factor = RadiusFactor(factor, width, x[0] + x[1]);
            factor = RadiusFactor(factor, width, x[3] + x[2]);
            factor = RadiusFactor(factor, height, y[0] + y[3]);
            factor = RadiusFactor(factor, height, y[1] + y[2]);
            if (factor >= 1F) return;
            for (var index = 0; index < 4; index++) { x[index] *= factor; y[index] *= factor; }
        }

        private static float RadiusFactor(float current, int length, float sum)
        {
            return sum <= 0F ? current : Math.Min(current, Math.Max(0, length) / sum);
        }

        private static void PaintRoundedBorder(Graphics graphics, Rectangle rect, S style, int top, int right, int bottom, int left, Color color, string borderStyle)
        {
            if (borderStyle == "dashed" || borderStyle == "dotted")
            {
                PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 0, color, borderStyle);
                PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 1, color, borderStyle);
                PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 2, color, borderStyle);
                PaintRoundedBorderSide(graphics, rect, style, top, right, bottom, left, 3, color, borderStyle);
                return;
            }
            using (var outer = RoundedPath(rect, style, 0, 0, 0, 0))
            using (var region = new Region(outer))
            {
                var innerRect = Rectangle.FromLTRB(rect.Left + left, rect.Top + top, rect.Right - right, rect.Bottom - bottom);
                if (innerRect.Width > 0 && innerRect.Height > 0)
                    using (var inner = RoundedPath(innerRect, style, left, top, right, bottom)) region.Exclude(inner);
                using (var brush = new SolidBrush(color)) graphics.FillRegion(brush, region);
            }
        }

        private static void PaintRoundedBorderSide(Graphics graphics, Rectangle rect, S style, int top, int right, int bottom, int left,
            int side, Color color, string borderStyle)
        {
            var width = side == 0 ? top : side == 1 ? right : side == 2 ? bottom : left;
            if (width < 1) return;
            var state = graphics.Save();
            try
            {
                using (var clip = BorderSideClip(rect, top, right, bottom, left, side)) graphics.SetClip(clip, CombineMode.Intersect);
                if (borderStyle == "dashed" || borderStyle == "dotted")
                {
                    var inset = Math.Max(0, width / 2);
                    var center = Rectangle.Inflate(rect, -inset, -inset);
                    if (center.Width < 1 || center.Height < 1) return;
                    using (var path = RoundedPath(center, style, inset, inset, inset, inset))
                    using (var pen = new Pen(color, width))
                    {
                        pen.DashStyle = borderStyle == "dashed" ? DashStyle.Dash : DashStyle.Dot;
                        graphics.DrawPath(pen, path);
                    }
                    return;
                }
                using (var outer = RoundedPath(rect, style, 0, 0, 0, 0))
                using (var region = new Region(outer))
                {
                    var innerRect = Rectangle.FromLTRB(rect.Left + left, rect.Top + top, rect.Right - right, rect.Bottom - bottom);
                    if (innerRect.Width > 0 && innerRect.Height > 0)
                        using (var inner = RoundedPath(innerRect, style, left, top, right, bottom)) region.Exclude(inner);
                    using (var brush = new SolidBrush(color)) graphics.FillRegion(brush, region);
                }
            }
            finally { graphics.Restore(state); }
        }

        private static GraphicsPath BorderSideClip(Rectangle rect, int top, int right, int bottom, int left, int side)
        {
            var innerLeft = Math.Min(rect.Right, rect.Left + Math.Max(0, left));
            var innerRight = Math.Max(rect.Left, rect.Right - Math.Max(0, right));
            var innerTop = Math.Min(rect.Bottom, rect.Top + Math.Max(0, top));
            var innerBottom = Math.Max(rect.Top, rect.Bottom - Math.Max(0, bottom));
            if (innerLeft > innerRight) innerLeft = innerRight = rect.Left + rect.Width / 2;
            if (innerTop > innerBottom) innerTop = innerBottom = rect.Top + rect.Height / 2;
            Point[] points;
            if (side == 0) points = new[] { new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top), new Point(innerRight, innerTop), new Point(innerLeft, innerTop) };
            else if (side == 1) points = new[] { new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom), new Point(innerRight, innerBottom), new Point(innerRight, innerTop) };
            else if (side == 2) points = new[] { new Point(rect.Right, rect.Bottom), new Point(rect.Left, rect.Bottom), new Point(innerLeft, innerBottom), new Point(innerRight, innerBottom) };
            else points = new[] { new Point(rect.Left, rect.Bottom), new Point(rect.Left, rect.Top), new Point(innerLeft, innerTop), new Point(innerLeft, innerBottom) };
            var path = new GraphicsPath();
            path.AddPolygon(points);
            return path;
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
                var objectRect = ObjectRect(box.S, new SizeF(image.Width, image.Height), rect);
                var tiny = image.Width < 4 || image.Height < 4;
                var state = graphics.Save();
                var interpolation = graphics.InterpolationMode;
                var offset = graphics.PixelOffsetMode;
                try
                {
                    graphics.SetClip(rect, CombineMode.Intersect);
                    graphics.InterpolationMode = tiny ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = tiny ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                    PaintImage(graphics, image, objectRect, opacity);
                }
                finally
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.PixelOffsetMode = offset;
                    graphics.Restore(state);
                }
                return;
            }

            using (var brush = new SolidBrush(PaintColor(Color.FromArgb(20, color), opacity))) graphics.FillRectangle(brush, rect);
            using (var pen = new Pen(PaintColor(Color.FromArgb(80, color), opacity))) graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            PaintText(graphics, box.X.Length == 0 ? "Image unavailable" : box.X, Font, new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16), color, TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak, opacity, box.S);
        }

        private void PaintListMarker(Graphics graphics, Bx box, Rectangle rect, int opacity)
        {
            Image image;
            if (box == null || box.U == null || !pics.TryGetValue(box.U.AbsoluteUri, out image) || image == null) return;
            var interpolation = graphics.InterpolationMode;
            var offset = graphics.PixelOffsetMode;
            try
            {
                var tiny = image.Width < 4 || image.Height < 4;
                graphics.InterpolationMode = tiny ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = tiny ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                PaintImage(graphics, image, rect, opacity);
            }
            finally
            {
                graphics.InterpolationMode = interpolation;
                graphics.PixelOffsetMode = offset;
            }
        }

        private void PaintInlinePicture(Graphics graphics, Uri source, Rectangle rect, int opacity)
        {
            Image image;
            if (source == null || !pics.TryGetValue(source.AbsoluteUri, out image) || image == null) return;
            var interpolation = graphics.InterpolationMode;
            var offset = graphics.PixelOffsetMode;
            try
            {
                var tiny = image.Width < 4 || image.Height < 4;
                graphics.InterpolationMode = tiny ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = tiny ? PixelOffsetMode.Half : PixelOffsetMode.HighQuality;
                PaintImage(graphics, image, rect, opacity);
            }
            finally
            {
                graphics.InterpolationMode = interpolation;
                graphics.PixelOffsetMode = offset;
            }
        }

        private void PaintVector(Graphics graphics, Bx box, Rectangle rect, Color color, int opacity, Rectangle clip)
        {
            var vector = box == null || box.B == null ? null : box.B.Vg;
            if (vector == null || rect.Width < 1 || rect.Height < 1 || vector.Vw <= 0 || vector.Vh <= 0) return;
            double scaleX;
            double scaleY;
            double translateX;
            double translateY;
            if (!VectorViewport(vector, rect, out scaleX, out scaleY, out translateX, out translateY)) return;
            var state = graphics.Save();
            var smoothing = graphics.SmoothingMode;
            try
            {
                graphics.SetClip(rect, CombineMode.Intersect);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var shape in vector.Ss)
                {
                    var strokeScale = VectorStrokeScale(shape, scaleX, scaleY);
                    RectangleF sourceBounds;
                    using (var path = VectorPath(shape, scaleX, scaleY, translateX, translateY, out sourceBounds))
                    using (var clipRegion = shape.Cp == null ? null : VectorClipRegion(shape, sourceBounds, scaleX, scaleY, translateX, translateY))
                    {
                        if (path == null || path.PointCount == 0) continue;
                        if (shape.Cl && (shape.Cp == null || clipRegion == null || clipRegion.IsEmpty(graphics))) continue;
                        var shapeState = graphics.Save();
                        try
                        {
                            if (clipRegion != null) graphics.SetClip(clipRegion, CombineMode.Intersect);
                            var fill = shape.K != "line" && (shape.Fd != null || !VectorNone(shape.F));
                            var stroke = shape.W > 0 && (shape.Sd != null || !VectorNone(shape.S));
                            if (opacity > 0 && fill)
                            {
                                var fillOpacity = VectorOpacity(opacity, shape.O * shape.Fo);
                                if (fillOpacity > 0)
                                    using (var brush = VectorBrush(shape, shape.Fd, shape.F, sourceBounds, scaleX, scaleY, translateX, translateY, shape.C, color, fillOpacity))
                                        if (brush != null) graphics.FillPath(brush, path);
                            }
                            if (opacity > 0 && stroke)
                            {
                                var strokeOpacity = VectorOpacity(opacity, shape.O * shape.So);
                                if (strokeOpacity > 0)
                                    using (var brush = VectorBrush(shape, shape.Sd, shape.S, sourceBounds, scaleX, scaleY, translateX, translateY, shape.C, color, strokeOpacity))
                                    {
                                        if (brush != null)
                                            using (var pen = new Pen(brush, Math.Max(.5F, Math.Min(512F, (float)(shape.W * strokeScale)))))
                                            {
                                                VectorPenStyle(pen, shape, strokeScale);
                                                graphics.DrawPath(pen, path);
                                            }
                                    }
                            }
                            if (shape.U != null && PointerEnabled(box.S))
                            {
                                var bounds = Rectangle.Ceiling(path.GetBounds());
                                var padding = VectorStrokePadding(shape, stroke, strokeScale);
                                bounds.Inflate(padding, padding);
                                if (clipRegion != null) bounds = Rectangle.Intersect(bounds, Rectangle.Ceiling(clipRegion.GetBounds(graphics)));
                                bounds = Rectangle.Intersect(bounds, rect);
                                bounds = VisibleRect(bounds, clip);
                                if (!bounds.IsEmpty) hits.Add(new Hit(bounds, shape.U, null, shape.El));
                            }
                        }
                        finally
                        {
                            graphics.Restore(shapeState);
                        }
                    }
                }
            }
            catch (ArgumentException) { }
            catch (System.Runtime.InteropServices.ExternalException) { }
            finally
            {
                graphics.SmoothingMode = smoothing;
                graphics.Restore(state);
            }
        }

        private static bool VectorViewport(Vg vector, Rectangle rect, out double scaleX, out double scaleY, out double translateX, out double translateY)
        {
            scaleX = rect.Width / vector.Vw;
            scaleY = rect.Height / vector.Vh;
            translateX = rect.Left - vector.X * scaleX;
            translateY = rect.Top - vector.Y * scaleY;
            if (vector.A != "n")
            {
                var scale = vector.M == "s" ? Math.Max(scaleX, scaleY) : Math.Min(scaleX, scaleY);
                scaleX = scale;
                scaleY = scale;
                var extraX = rect.Width - vector.Vw * scale;
                var extraY = rect.Height - vector.Vh * scale;
                var align = String.IsNullOrEmpty(vector.A) || vector.A.Length != 2 ? "mm" : vector.A;
                translateX = rect.Left - vector.X * scale + (align[0] == 'x' ? extraX : align[0] == 'm' ? extraX / 2D : 0D);
                translateY = rect.Top - vector.Y * scale + (align[1] == 'x' ? extraY : align[1] == 'm' ? extraY / 2D : 0D);
            }
            return scaleX > 0D && scaleY > 0D && !Double.IsNaN(scaleX) && !Double.IsInfinity(scaleX) &&
                !Double.IsNaN(scaleY) && !Double.IsInfinity(scaleY) && !Double.IsNaN(translateX) && !Double.IsInfinity(translateX) &&
                !Double.IsNaN(translateY) && !Double.IsInfinity(translateY);
        }

        private static double VectorStrokeScale(Vs shape, double scaleX, double scaleY)
        {
            var matrix = shape == null || shape.M == null || shape.M.Count < 6 ? null : shape.M;
            var a = matrix == null ? 1D : matrix[0];
            var b = matrix == null ? 0D : matrix[1];
            var c = matrix == null ? 0D : matrix[2];
            var d = matrix == null ? 1D : matrix[3];
            var basisX = Math.Sqrt(a * a * scaleX * scaleX + b * b * scaleY * scaleY);
            var basisY = Math.Sqrt(c * c * scaleX * scaleX + d * d * scaleY * scaleY);
            var scale = (basisX + basisY) / 2D;
            return Double.IsNaN(scale) || Double.IsInfinity(scale) ? 1D : Math.Max(.001D, Math.Min(10000D, scale));
        }

        private GraphicsPath VectorPath(Vs shape, double scaleX, double scaleY, double translateX, double translateY, out RectangleF sourceBounds)
        {
            sourceBounds = RectangleF.Empty;
            if (shape == null) return null;
            var path = new GraphicsPath(shape.Fr == "e" ? FillMode.Alternate : FillMode.Winding);
            try
            {
                var points = shape.P;
                if (shape.K == "rect" && points.Count >= 6)
                {
                    var rect = VectorRect(points[0], points[1], points[2], points[3]);
                    var rx = Math.Min(points[2] / 2D, points[4]);
                    var ry = Math.Min(points[3] / 2D, points[5]);
                    if (rx <= 0 && ry > 0) rx = ry;
                    if (ry <= 0 && rx > 0) ry = rx;
                    if (rx > 0 && ry > 0) VectorRoundRect(path, rect, (float)rx, (float)ry);
                    else path.AddRectangle(rect);
                }
                else if (shape.K == "circle" && points.Count >= 3)
                {
                    var radius = points[2];
                    path.AddEllipse(VectorRect(points[0] - radius, points[1] - radius, radius * 2D, radius * 2D));
                }
                else if (shape.K == "ellipse" && points.Count >= 4)
                    path.AddEllipse(VectorRect(points[0] - points[2], points[1] - points[3], points[2] * 2D, points[3] * 2D));
                else if (shape.K == "line" && points.Count >= 4)
                    path.AddLine(VectorPoint(points[0], points[1]), VectorPoint(points[2], points[3]));
                else if ((shape.K == "polyline" || shape.K == "polygon") && points.Count >= 4)
                {
                    var mapped = new PointF[points.Count / 2];
                    for (var index = 0; index < mapped.Length; index++) mapped[index] = VectorPoint(points[index * 2], points[index * 2 + 1]);
                    path.AddLines(mapped);
                    if (shape.K == "polygon") path.CloseFigure();
                }
                else if (shape.K == "path")
                {
                    var current = PointF.Empty;
                    var ready = false;
                    foreach (var step in shape.D)
                    {
                        if (step.K == 'M')
                        {
                            path.StartFigure();
                            current = VectorPoint(step.X, step.Y);
                            ready = true;
                        }
                        else if (step.K == 'L' && ready)
                        {
                            var next = VectorPoint(step.X, step.Y);
                            path.AddLine(current, next);
                            current = next;
                        }
                        else if (step.K == 'C' && ready)
                        {
                            var next = VectorPoint(step.X, step.Y);
                            path.AddBezier(current, VectorPoint(step.A, step.B), VectorPoint(step.C, step.D), next);
                            current = next;
                        }
                        else if (step.K == 'Q' && ready)
                        {
                            var next = VectorPoint(step.X, step.Y);
                            var control = VectorPoint(step.A, step.B);
                            var control1 = new PointF(current.X + (control.X - current.X) * 2F / 3F, current.Y + (control.Y - current.Y) * 2F / 3F);
                            var control2 = new PointF(next.X + (control.X - next.X) * 2F / 3F, next.Y + (control.Y - next.Y) * 2F / 3F);
                            path.AddBezier(current, control1, control2, next);
                            current = next;
                        }
                        else if (step.K == 'Z' && ready)
                        {
                            path.CloseFigure();
                            current = VectorPoint(step.X, step.Y);
                        }
                    }
                }
                else if (shape.K == "text" && points.Count >= 2 && shape.T.Length > 0)
                {
                    var origin = VectorPoint(points[0], points[1]);
                    var size = Math.Max(1F, Math.Min(512F, (float)shape.Z));
                    var family = Font.FontFamily;
                    var em = Math.Max(1, family.GetEmHeight(FontStyle.Regular));
                    var ascent = size * family.GetCellAscent(FontStyle.Regular) / em;
                    using (var format = (StringFormat)StringFormat.GenericTypographic.Clone())
                    {
                        format.FormatFlags |= StringFormatFlags.NoClip;
                        path.AddString(shape.T, family, (int)FontStyle.Regular, size, new PointF(origin.X, origin.Y - ascent), format);
                    }
                    if (shape.A == "m" || shape.A == "e")
                    {
                        var bounds = path.GetBounds();
                        using (var move = new Matrix())
                        {
                            move.Translate(shape.A == "m" ? -bounds.Width / 2F : -bounds.Width, 0);
                            path.Transform(move);
                        }
                    }
                }
                sourceBounds = path.GetBounds();
                using (var matrix = VectorMatrix(shape, scaleX, scaleY, translateX, translateY)) path.Transform(matrix);
                return path;
            }
            catch (ArgumentException)
            {
                path.Dispose();
                return null;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                path.Dispose();
                return null;
            }
        }

        private static void VectorRoundRect(GraphicsPath path, RectangleF rect, float rx, float ry)
        {
            rx = Math.Min(rect.Width / 2F, Math.Max(.01F, rx));
            ry = Math.Min(rect.Height / 2F, Math.Max(.01F, ry));
            path.AddArc(rect.Left, rect.Top, rx * 2F, ry * 2F, 180, 90);
            path.AddArc(rect.Right - rx * 2F, rect.Top, rx * 2F, ry * 2F, 270, 90);
            path.AddArc(rect.Right - rx * 2F, rect.Bottom - ry * 2F, rx * 2F, ry * 2F, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - ry * 2F, rx * 2F, ry * 2F, 90, 90);
            path.CloseFigure();
        }

        private Region VectorClipRegion(Vs shape, RectangleF sourceBounds, double scaleX, double scaleY, double translateX, double translateY)
        {
            if (shape == null || shape.Cp == null) return null;
            var result = new Region();
            result.MakeEmpty();
            try
            {
                foreach (var clipShape in shape.Cp.Ss)
                {
                    RectangleF unused;
                    using (var part = VectorPath(clipShape, 1D, 1D, 0D, 0D, out unused))
                    {
                        if (part == null || part.PointCount == 0) continue;
                        double[] values;
                        if (shape.Cp.U == "b")
                        {
                            if (sourceBounds.Width <= 0F || sourceBounds.Height <= 0F) continue;
                            var box = new[] { (double)sourceBounds.Width, 0D, 0D, (double)sourceBounds.Height, (double)sourceBounds.Left, (double)sourceBounds.Top };
                            values = VectorMatrixMultiply(VectorMatrixValues(shape, scaleX, scaleY, translateX, translateY), box);
                        }
                        else values = VectorMatrixValues(shape.Cm, scaleX, scaleY, translateX, translateY);
                        using (var matrix = VectorMatrix(values)) part.Transform(matrix);
                        result.Union(part);
                    }
                }
                return result;
            }
            catch (ArgumentException)
            {
                result.Dispose();
                return null;
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                result.Dispose();
                return null;
            }
        }

        private static void VectorPenStyle(Pen pen, Vs shape, double strokeScale)
        {
            if (pen == null || shape == null) return;
            var cap = shape.Lc == "r" ? LineCap.Round : shape.Lc == "s" ? LineCap.Square : LineCap.Flat;
            pen.StartCap = cap;
            pen.EndCap = cap;
            pen.DashCap = shape.Lc == "r" ? DashCap.Round : DashCap.Flat;
            pen.LineJoin = shape.Lj == "r" ? LineJoin.Round : shape.Lj == "b" ? LineJoin.Bevel : LineJoin.Miter;
            pen.MiterLimit = (float)Math.Max(1D, Math.Min(64D, shape.Ml));
            if (shape.Da == null || shape.Da.Count == 0 || pen.Width <= 0F) return;
            var unit = strokeScale / pen.Width;
            var pattern = new float[shape.Da.Count];
            for (var index = 0; index < pattern.Length; index++) pattern[index] = (float)Math.Max(.01D, Math.Min(100000D, shape.Da[index] * unit));
            pen.DashPattern = pattern;
            pen.DashOffset = (float)Math.Max(-100000D, Math.Min(100000D, shape.Do * unit));
        }

        private static int VectorStrokePadding(Vs shape, bool stroke, double strokeScale)
        {
            if (!stroke || shape == null) return 2;
            var padding = shape.W * strokeScale / 2D;
            if (shape.Lj == "m" && shape.K != "line") padding *= Math.Max(1D, Math.Min(64D, shape.Ml));
            return Math.Max(2, (int)Math.Ceiling(Math.Min(Int32.MaxValue / 4D, padding)));
        }

        private static Brush VectorBrush(Vs shape, Vd gradient, string value, RectangleF bounds, double scaleX, double scaleY, double translateX, double translateY, string current, Color fallback, int opacity)
        {
            if (gradient == null) return new SolidBrush(PaintColor(VectorColor(value, current, fallback), opacity));
            if (gradient.Ss == null || gradient.Ss.Count == 0) return null;
            if (gradient.Ss.Count == 1)
            {
                var stop = gradient.Ss[0];
                return new SolidBrush(PaintColor(VectorColor(stop.C, current, fallback), VectorOpacity(opacity, stop.A)));
            }
            if (gradient.U == "b" && (bounds.Width <= 0F || bounds.Height <= 0F)) return null;
            using (var map = VectorGradientMatrix(shape, gradient, bounds, scaleX, scaleY, translateX, translateY))
            {
                var blend = VectorGradientBlend(gradient, current, fallback, opacity);
                if (blend == null || blend.Colors.Length < 2) return null;
                if (gradient.K == "l" && gradient.P.Count >= 4)
                {
                    var points = new[] { VectorPoint(gradient.P[0], gradient.P[1]), VectorPoint(gradient.P[2], gradient.P[3]) };
                    map.TransformPoints(points);
                    if (Math.Abs(points[0].X - points[1].X) < .01F && Math.Abs(points[0].Y - points[1].Y) < .01F)
                        return new SolidBrush(blend.Colors[blend.Colors.Length - 1]);
                    var brush = new LinearGradientBrush(points[0], points[1], blend.Colors[0], blend.Colors[blend.Colors.Length - 1]);
                    brush.InterpolationColors = blend;
                    return brush;
                }
                if (gradient.K == "r" && gradient.P.Count >= 5)
                {
                    var centerX = gradient.P[0];
                    var centerY = gradient.P[1];
                    var radius = gradient.P[2];
                    var focusX = gradient.P[3];
                    var focusY = gradient.P[4];
                    var deltaX = focusX - centerX;
                    var deltaY = focusY - centerY;
                    var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    if (distance >= radius && distance > 0D)
                    {
                        var clamp = radius * .999D / distance;
                        focusX = centerX + deltaX * clamp;
                        focusY = centerY + deltaY * clamp;
                    }
                    using (var circle = new GraphicsPath())
                    {
                        circle.AddEllipse(VectorRect(centerX - radius, centerY - radius, radius * 2D, radius * 2D));
                        circle.Transform(map);
                        var focus = new[] { VectorPoint(focusX, focusY) };
                        map.TransformPoints(focus);
                        var brush = new PathGradientBrush(circle);
                        brush.CenterPoint = focus[0];
                        brush.CenterColor = blend.Colors[0];
                        brush.SurroundColors = new[] { blend.Colors[blend.Colors.Length - 1] };
                        brush.InterpolationColors = blend;
                        return brush;
                    }
                }
            }
            return null;
        }

        private static ColorBlend VectorGradientBlend(Vd gradient, string current, Color fallback, int opacity)
        {
            var positions = new List<float>();
            var colors = new List<Color>();
            foreach (var stop in gradient.Ss)
            {
                var position = (float)Math.Max(0D, Math.Min(1D, stop.O));
                var color = PaintColor(VectorColor(stop.C, current, fallback), VectorOpacity(opacity, stop.A));
                if (positions.Count > 0 && position <= positions[positions.Count - 1])
                {
                    var previous = positions[positions.Count - 1];
                    if (previous >= 1F) { colors[colors.Count - 1] = color; continue; }
                    position = Math.Min(1F, previous + .0001F);
                }
                positions.Add(position);
                colors.Add(color);
            }
            if (positions.Count == 0) return null;
            if (positions[0] > 0F)
            {
                positions.Insert(0, 0F);
                colors.Insert(0, colors[0]);
            }
            else positions[0] = 0F;
            if (positions[positions.Count - 1] < 1F)
            {
                positions.Add(1F);
                colors.Add(colors[colors.Count - 1]);
            }
            else positions[positions.Count - 1] = 1F;
            return new ColorBlend(positions.Count) { Positions = positions.ToArray(), Colors = colors.ToArray() };
        }

        private static Matrix VectorGradientMatrix(Vs shape, Vd gradient, RectangleF bounds, double scaleX, double scaleY, double translateX, double translateY)
        {
            var shapeMatrix = VectorMatrixValues(shape, scaleX, scaleY, translateX, translateY);
            var values = gradient == null || gradient.M == null || gradient.M.Count < 6 ? null : gradient.M;
            var gradientMatrix = values == null ? new[] { 1D, 0D, 0D, 1D, 0D, 0D } : new[] { values[0], values[1], values[2], values[3], values[4], values[5] };
            if (gradient != null && gradient.U == "b")
            {
                var box = new[] { (double)bounds.Width, 0D, 0D, (double)bounds.Height, (double)bounds.Left, (double)bounds.Top };
                gradientMatrix = VectorMatrixMultiply(box, gradientMatrix);
            }
            return VectorMatrix(VectorMatrixMultiply(shapeMatrix, gradientMatrix));
        }

        private static Matrix VectorMatrix(Vs shape, double scaleX, double scaleY, double translateX, double translateY)
        {
            return VectorMatrix(VectorMatrixValues(shape, scaleX, scaleY, translateX, translateY));
        }

        private static double[] VectorMatrixValues(Vs shape, double scaleX, double scaleY, double translateX, double translateY)
        {
            return VectorMatrixValues(shape == null ? null : shape.M, scaleX, scaleY, translateX, translateY);
        }

        private static double[] VectorMatrixValues(IList<double> values, double scaleX, double scaleY, double translateX, double translateY)
        {
            if (values != null && values.Count < 6) values = null;
            var a = values == null ? 1D : values[0];
            var b = values == null ? 0D : values[1];
            var c = values == null ? 0D : values[2];
            var d = values == null ? 1D : values[3];
            var e = values == null ? 0D : values[4];
            var f = values == null ? 0D : values[5];
            return new[] { a * scaleX, b * scaleY, c * scaleX, d * scaleY, translateX + e * scaleX, translateY + f * scaleY };
        }

        private static double[] VectorMatrixMultiply(double[] left, double[] right)
        {
            return new[]
            {
                left[0] * right[0] + left[2] * right[1],
                left[1] * right[0] + left[3] * right[1],
                left[0] * right[2] + left[2] * right[3],
                left[1] * right[2] + left[3] * right[3],
                left[0] * right[4] + left[2] * right[5] + left[4],
                left[1] * right[4] + left[3] * right[5] + left[5]
            };
        }

        private static Matrix VectorMatrix(double[] values)
        {
            return new Matrix(VectorFloat(values[0]), VectorFloat(values[1]), VectorFloat(values[2]), VectorFloat(values[3]), VectorFloat(values[4]), VectorFloat(values[5]));
        }

        private static PointF VectorPoint(double px, double py)
        {
            return new PointF(VectorFloat(px), VectorFloat(py));
        }

        private static RectangleF VectorRect(double px, double py, double width, double height)
        {
            var left = VectorFloat(px);
            var top = VectorFloat(py);
            var right = VectorFloat(px + width);
            var bottom = VectorFloat(py + height);
            return RectangleF.FromLTRB(Math.Min(left, right), Math.Min(top, bottom), Math.Max(left, right), Math.Max(top, bottom));
        }

        private static float VectorFloat(double value)
        {
            return (float)Math.Max(-1000000D, Math.Min(1000000D, value));
        }

        private static bool VectorNone(string value)
        {
            return String.IsNullOrWhiteSpace(value) || value.Trim().Equals("none", StringComparison.OrdinalIgnoreCase);
        }

        private static int VectorOpacity(int opacity, double scale)
        {
            if (Double.IsNaN(scale) || Double.IsInfinity(scale)) return 0;
            return Math.Max(0, Math.Min(100, (int)Math.Round(opacity * Math.Max(0D, Math.Min(1D, scale)))));
        }

        private static Color VectorColor(string value, string current, Color fallback)
        {
            var text = (value ?? "").Trim();
            if (text.Equals("currentcolor", StringComparison.OrdinalIgnoreCase)) text = current;
            var match = Regex.Match(text ?? "", "^rgba?\\(\\s*(?<r>[0-9]{1,3})\\s*,\\s*(?<g>[0-9]{1,3})\\s*,\\s*(?<b>[0-9]{1,3})(?:\\s*,\\s*(?<a>(?:0|1)(?:\\.[0-9]+)?|\\.[0-9]+))?\\s*\\)$", RegexOptions.IgnoreCase);
            if (!match.Success) return CssColor(text, fallback);
            int red;
            int green;
            int blue;
            double alpha;
            Int32.TryParse(match.Groups["r"].Value, out red);
            Int32.TryParse(match.Groups["g"].Value, out green);
            Int32.TryParse(match.Groups["b"].Value, out blue);
            if (!Double.TryParse(match.Groups["a"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out alpha)) alpha = 1D;
            return Color.FromArgb((int)Math.Round(Math.Max(0D, Math.Min(1D, alpha)) * 255D), Math.Min(255, red), Math.Min(255, green), Math.Min(255, blue));
        }

        private void AddMapHits(Bx box, Rectangle imageRect, Rectangle clip)
        {
            if (box == null || !PointerEnabled(box.S) || box.B == null || box.B.Mp == null || imageRect.Width < 1 || imageRect.Height < 1) return;
            var image = PicObjectRect(box.B, imageRect);
            var visible = VisibleRect(Rectangle.Intersect(imageRect, image), clip);
            if (visible.IsEmpty) return;
            var source = MapSourceSize(box.B, imageRect);
            for (var index = box.B.Mp.As.Count - 1; index >= 0; index--)
            {
                var area = box.B.Mp.As[index];
                var bounds = Rectangle.Intersect(visible, MapBounds(area, image, source));
                if (!bounds.IsEmpty) hits.Add(new Hit(bounds, area.U, null, area.El, area, image, source));
            }
        }

        private Rectangle PicObjectRect(B block, Rectangle rect)
        {
            Image image;
            if (block != null && block.U != null && pics.TryGetValue(block.U.AbsoluteUri, out image) && image != null && image.Width > 0 && image.Height > 0)
                return ObjectRect(block.S, new SizeF(image.Width, image.Height), rect);
            return ObjectRect(block == null ? null : block.S, block == null ? rect.Size : MapSourceSize(block, rect), rect);
        }

        private static Rectangle ObjectRect(S style, SizeF source, Rectangle box)
        {
            if (box.Width < 1 || box.Height < 1 || source.Width <= 0F || source.Height <= 0F) return Rectangle.Empty;
            var fit = style == null ? "f" : style.Of;
            double width = box.Width;
            double height = box.Height;
            if (fit != "f")
            {
                var horizontal = box.Width / (double)source.Width;
                var vertical = box.Height / (double)source.Height;
                double scale;
                if (fit == "v") scale = Math.Max(horizontal, vertical);
                else if (fit == "c") scale = Math.Min(horizontal, vertical);
                else if (fit == "s") scale = Math.Min(1D, Math.Min(horizontal, vertical));
                else scale = 1D;
                width = source.Width * scale;
                height = source.Height * scale;
            }
            if (width <= 0D || height <= 0D || Double.IsNaN(width) || Double.IsNaN(height) || Double.IsInfinity(width) || Double.IsInfinity(height)) return Rectangle.Empty;
            var objectWidth = Math.Max(1, Math.Min(8192, (int)Math.Round(width)));
            var objectHeight = Math.Max(1, Math.Min(8192, (int)Math.Round(height)));
            var left = BackgroundImageOffset(box.Left, box.Width - objectWidth, style == null ? 50 : style.Ox, style == null || style.Oxp);
            var top = BackgroundImageOffset(box.Top, box.Height - objectHeight, style == null ? 50 : style.Oy, style == null || style.Oyp);
            return new Rectangle(left, top, objectWidth, objectHeight);
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
            if (width <= 0 && block.S != null && block.S.W > 0) width = BoxContentWidth(block.S, block.S.W);
            if (height <= 0 && block.S != null && block.S.H > 0) height = BoxContentHeight(block.S, block.S.H);
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
            var borderColor = style != null && style.Bcv(0).Length > 0 ? CssColor(style.Bcv(0), Color.FromArgb(148, 163, 184)) : Color.FromArgb(148, 163, 184);
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
                    PaintText(graphics, box.X, font, new Rectangle(rect.X + 24, rect.Y, Math.Max(1, rect.Width - 24), rect.Height), foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding, opacity, field.S);
                return;
            }

            if (field.K == "range")
            {
                PaintFieldBackground(graphics, rect, field.S, background);
                PaintBitmapBackground(graphics, rect, field.S, opacity);
                PaintGradient(graphics, rect, field.S, foreground, opacity);
                PaintShadows(graphics, rect, field.S, foreground, opacity, true);
                var rangeBorder = field.Ok ? Color.FromArgb(165, 165, 165) : Color.FromArgb(185, 28, 28);
                PaintFieldOutline(graphics, rect, field.S, PaintColor(rangeBorder, opacity), field.Ok ? 1F : 2F);
                var track = new Rectangle(rect.X + 12, rect.Top + rect.Height / 2 - 2, Math.Max(1, rect.Width - 24), 4);
                var thumbX = track.Left + (int)Math.Round((track.Width - 1) * RangeRatio(field));
                using (var brush = new SolidBrush(PaintColor(Color.FromArgb(205, 210, 216), opacity))) graphics.FillRectangle(brush, track);
                using (var brush = new SolidBrush(PaintColor(disabled ? Color.Gray : Color.FromArgb(0, 102, 204), opacity))) graphics.FillRectangle(brush, new Rectangle(track.Left, track.Top, Math.Max(1, thumbX - track.Left), track.Height));
                using (var brush = new SolidBrush(PaintColor(disabled ? Color.DarkGray : Color.White, opacity))) graphics.FillEllipse(brush, thumbX - 7, track.Top - 6, 14, 14);
                using (var pen = new Pen(PaintColor(rangeBorder, opacity))) graphics.DrawEllipse(pen, thumbX - 7, track.Top - 6, 14, 14);
                return;
            }

            PaintFieldBackground(graphics, rect, field.S, background);
            PaintBitmapBackground(graphics, rect, field.S, opacity);
            PaintGradient(graphics, rect, field.S, foreground, opacity);
            PaintShadows(graphics, rect, field.S, foreground, opacity, true);
            var border = !field.Ok ? Color.FromArgb(185, 28, 28) : box.K == "button" ? Color.FromArgb(145, 145, 145) : Color.FromArgb(165, 165, 165);
            var outline = field.Ok ? rect : new Rectangle(rect.X + 1, rect.Y + 1, Math.Max(1, rect.Width - 2), Math.Max(1, rect.Height - 2));
            PaintFieldOutline(graphics, outline, field.S, PaintColor(border, opacity), field.Ok ? 1F : 2F);

            var text = field.K == "select" ? OptionText(field) : field.V ?? "";
            var placeholder = text.Length == 0 && field.P.Length > 0;
            if (placeholder) text = field.P;
            if (field.K == "password" && !placeholder) text = new string('*', text.Length);
            text = E.Tx(text, field.S);
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
                    PaintText(graphics, E.Tx("Browse", field.S), font, button, foreground, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding, opacity, field.S);
            }
            else if (field.K == "color")
            {
                var swatch = new Rectangle(rect.X + 5, rect.Y + 5, Math.Min(24, Math.Max(1, rect.Width - 10)), Math.Max(1, rect.Height - 10));
                using (var brush = new SolidBrush(PaintColor(CssColor(field.V, Color.Black), opacity))) graphics.FillRectangle(brush, swatch);
                using (var pen = new Pen(PaintColor(Color.FromArgb(120, 120, 120), opacity))) graphics.DrawRectangle(pen, swatch.X, swatch.Y, Math.Max(0, swatch.Width - 1), Math.Max(0, swatch.Height - 1));
                textLeft += 30;
            }
            using (var font = BoxFont(box.K, field.S, false))
                PaintText(graphics, box.K == "button" ? box.X : text, font, new Rectangle(textLeft, rect.Y + 3, Math.Max(1, rect.Right - textLeft - pad - reserve), Math.Max(1, rect.Height - 6)), textColor, flags, opacity, field.S);
        }

        private static void PaintFieldBackground(Graphics graphics, Rectangle rect, S style, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                if (!HasRadius(style)) { graphics.FillRectangle(brush, rect); return; }
                var smoothing = graphics.SmoothingMode;
                try
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(rect, style, 0, 0, 0, 0)) graphics.FillPath(brush, path);
                }
                finally { graphics.SmoothingMode = smoothing; }
            }
        }

        private static void PaintFieldOutline(Graphics graphics, Rectangle rect, S style, Color color, float width)
        {
            using (var pen = new Pen(color, width))
            {
                var inset = Math.Max(1, (int)Math.Ceiling(width));
                var outline = new Rectangle(rect.X, rect.Y, Math.Max(1, rect.Width - inset), Math.Max(1, rect.Height - inset));
                if (!HasRadius(style)) { graphics.DrawRectangle(pen, outline); return; }
                var smoothing = graphics.SmoothingMode;
                try
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(outline, style, 0, 0, inset, inset)) graphics.DrawPath(pen, path);
                }
                finally { graphics.SmoothingMode = smoothing; }
            }
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
            while (picWork != null && !picWork.IsCompleted)
            {
                await picWork;
                if (closed || picStop || page != At) return;
            }
            if (closed || picStop || page == null || page != At) return;
            var work = ReadPicsA(page);
            picWork = work;
            try { await work; }
            finally { if (Object.ReferenceEquals(picWork, work)) picWork = null; }
        }

        private void CancelPics()
        {
            unchecked { picVersion++; }
            if (picBudget != null) picBudget.Cancel();
            AbortPic();
        }

        private void AbortPic()
        {
            if (picRequest != null)
                try { picRequest.Abort(); }
                catch (WebException) { }
                catch (ObjectDisposedException) { }
            if (picStream != null)
                try { picStream.Dispose(); }
                catch (IOException) { }
                catch (ObjectDisposedException) { }
        }

        private async Task<T> PictureWait<T>(Task<T> work, Task deadline)
        {
            if (await Task.WhenAny(work, deadline) != work && !work.IsCompleted)
            {
                AbortPic();
                DiscardPicture(work);
                throw new TimeoutException("Lea picture pass timed out.");
            }
            return await work;
        }

        private static void DiscardPicture<T>(Task<T> work)
        {
            work.ContinueWith(completed =>
            {
                if (completed.IsFaulted)
                    foreach (var error in completed.Exception.Flatten().InnerExceptions) ClosePictureError(error);
                else if (completed.Status == TaskStatus.RanToCompletion)
                {
                    var resource = (object)completed.Result as IDisposable;
                    if (resource != null) resource.Dispose();
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private static void ClosePictureError(Exception error)
        {
            var web = error as WebException;
            if (web != null && web.Response != null) web.Response.Close();
        }

        private async Task<byte[]> ReadPicture(Uri source, Task deadline)
        {
            try
            {
                if (source.IsFile)
                {
                    var opening = Task.Factory.StartNew(() => new PictureFile(source.LocalPath), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                    using (var file = await PictureWait(opening, deadline))
                    {
                        picStream = file.Stream;
                        return await PictureBytes(file.Stream, file.Length, deadline);
                    }
                }
                if (source.Scheme != "http" && source.Scheme != "https") throw new InvalidDataException("Unsupported image address.");
                var request = (HttpWebRequest)WebRequest.Create(source);
                picRequest = request;
                request.UserAgent = "Lea/0.2";
                request.Timeout = Tm;
                request.ReadWriteTimeout = Tm;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var response = await PictureWait(request.GetResponseAsync(), deadline))
                using (var input = response.GetResponseStream())
                {
                    picStream = input;
                    return await PictureBytes(input, response.ContentLength, deadline);
                }
            }
            catch (WebException error) { ClosePictureError(error); throw; }
            finally { picStream = null; picRequest = null; }
        }

        private sealed class PictureFile : IDisposable
        {
            public readonly FileStream Stream;
            public readonly long Length;
            public PictureFile(string path)
            {
                Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true);
                try { Length = Stream.Length; }
                catch { Stream.Dispose(); throw; }
            }
            public void Dispose() { Stream.Dispose(); }
        }

        private async Task<byte[]> PictureBytes(Stream input, long length, Task deadline)
        {
            if (length > PicBytes) throw new InvalidDataException("Image response exceeds 4 MiB.");
            var buffer = new byte[32768];
            using (var output = new MemoryStream())
            {
                while (true)
                {
                    if (deadline.IsCompleted) throw new TimeoutException("Lea picture pass timed out.");
                    var count = await PictureWait(input.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, PicBytes - output.Length + 1)), deadline);
                    if (count == 0) return output.ToArray();
                    if (output.Length + count > PicBytes) throw new InvalidDataException("Image response exceeds 4 MiB.");
                    output.Write(buffer, 0, count);
                }
            }
        }

        private async Task ReadPicsA(P page)
        {
            if (closed || page == null) return;
            var version = picVersion;
            picPage = page;
            picBusy = true;
            using (var timer = new CancellationTokenSource())
            try
            {
                picBudget = timer;
                var deadline = Task.Delay(Tm, timer.Token);
                var sources = PicSources(page);
                TrimPics(sources);
                foreach (var source in sources)
                {
                    if (closed || picStop || page != At || version != picVersion) break;
                    var key = source.AbsoluteUri;
                    if (pics.ContainsKey(key)) continue;
                    if (deadline.IsCompleted) { StorePic(key, null); continue; }
                    Image image = null;
                    long reserved = 0;
                    try
                    {
                        byte[] data;
                        if (source.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase)) data = EmbeddedPic(source);
                        else data = await ReadPicture(source, deadline);
                        if (closed || picStop || page != At || version != picVersion) break;
                        if (data == null || data.Length == 0 || data.Length > PicBytes) throw new InvalidDataException("Image resource is empty or too large.");
                        var inspection = Task.Factory.StartNew(() => Pi.Rd(data), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                        var info = await PictureWait(inspection, deadline);
                        var expected = info.P * 4;
                        if (expected > PicMemory - picSize || !ReservePic(info.R))
                            throw new InvalidDataException("Decoded image exceeds the bitmap budget.");
                        reserved = info.R;
                        using (var decoder = CancellationTokenSource.CreateLinkedTokenSource(timer.Token))
                        {
                            var decoding = DecodePicture(data, info.R, decoder.Token);
                            try { image = await PictureWait(decoding, deadline); }
                            catch
                            {
                                if (!decoding.IsCompleted)
                                {
                                    decoder.Cancel();
                                    var late = reserved;
                                    reserved = 0;
                                    var release = decoding.ContinueWith(completed => ReleasePic(late), CancellationToken.None,
                                        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                                }
                                throw;
                            }
                        }
                        var size = (long)image.Width * image.Height;
                        var retained = size * 4;
                        if (image.Width > Pi.Axis || image.Height > Pi.Axis || size > Pi.Pixels || retained > PicMemory - picSize ||
                            retained > info.R || !CommitPic(ref reserved, retained))
                            throw new InvalidDataException("Decoded image exceeds its preflight or bitmap budget.");
                        if (deadline.IsCompleted) throw new TimeoutException("Lea picture pass timed out.");
                    }
                    catch
                    {
                        if (image != null) image.Dispose();
                        if (reserved > 0) ReleasePic(reserved);
                        image = null;
                        reserved = 0;
                    }
                    if (closed || picStop || page != At || version != picVersion)
                    {
                        if (image != null) image.Dispose();
                        if (reserved > 0) ReleasePic(reserved);
                        break;
                    }
                    StorePic(key, image);
                }
            }
            finally
            {
                if (Object.ReferenceEquals(picBudget, timer)) picBudget = null;
                timer.Cancel();
                picBusy = false;
                picPage = null;
            }
        }

        private Task<Image> DecodePicture(byte[] data, long reserve, CancellationToken token)
        {
            return Di.Rd(data, Ie, picWorkerArgs, token, reserve, Icl, PictureHostMemory,
                (process, bounded, restricted, low, suspended, handles, aslr, appContainer, noNetwork, fileAcl, cpu, guards) =>
                {
                    Interlocked.Increment(ref picNative);
                    Interlocked.Exchange(ref picWorker, process);
                    Interlocked.Exchange(ref picJob, bounded ? 1 : 0);
                    Interlocked.Exchange(ref picRestricted, restricted ? 1 : 0);
                    Interlocked.Exchange(ref picLow, low ? 1 : 0);
                    Interlocked.Exchange(ref picSuspended, suspended ? 1 : 0);
                    Interlocked.Exchange(ref picHandleList, handles ? 1 : 0);
                    Interlocked.Exchange(ref picAslr, aslr ? 1 : 0);
                    Interlocked.Exchange(ref picCpu, cpu);
                    Interlocked.Exchange(ref picMitigations, guards);
                    Interlocked.Exchange(ref picAppContainer, appContainer ? 1 : 0);
                    Interlocked.Exchange(ref picNoNetwork, noNetwork ? 1 : 0);
                    Interlocked.Exchange(ref picFileAcl, fileAcl ? 1 : 0);
                },
                (host, memory, peak, combined, admitted) =>
                {
                    Interlocked.Exchange(ref picHostMemory, host);
                    Interlocked.Exchange(ref picWorkerMemory, memory);
                    Interlocked.Exchange(ref picWorkerPeak, peak);
                    Interlocked.Exchange(ref picCombinedMemory, combined);
                    Interlocked.Exchange(ref picAdmission, admitted ? 1 : 0);
                },
                (host, memory, peak, combined) =>
                {
                    Interlocked.Exchange(ref picHostMemory, host);
                    Interlocked.Exchange(ref picWorkerMemory, memory);
                    Interlocked.Exchange(ref picWorkerPeak, peak);
                    Interlocked.Exchange(ref picCombinedMemory,
                        Math.Max(Interlocked.Read(ref picCombinedMemory), combined));
                },
                () => Interlocked.Increment(ref picKilled));
        }

        private long PictureHostMemory()
        {
            return picHostMemoryProbe();
        }

        private static long HostPrivateMemory()
        {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
                process.Refresh();
                return process.PrivateMemorySize64;
            }
        }

        private static bool ReservePic(long size)
        {
            if (size < 1) return false;
            lock (picMemoryGate)
            {
                if (size > picMemoryLimit - picMemoryTotal) return false;
                picMemoryTotal += size;
                return true;
            }
        }

        private static bool CommitPic(ref long reserved, long retained)
        {
            if (reserved < 1 || retained < 1) return false;
            lock (picMemoryGate)
            {
                var change = retained - reserved;
                if (change > 0 && change > picMemoryLimit - picMemoryTotal) return false;
                picMemoryTotal += change;
                reserved = retained;
                return true;
            }
        }

        private static void ReleasePic(long size)
        {
            if (size < 1) return;
            lock (picMemoryGate) picMemoryTotal = Math.Max(0, picMemoryTotal - size);
        }

        private static IList<Uri> PicSources(P page)
        {
            var result = new List<Uri>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var origin = page == null ? null : page.U;
            AddPicSource(result, seen, page == null || page.S == null || page.S.Bi == null ? null : page.S.Bi.U, origin);
            if (page != null)
            {
                foreach (var block in page.Bs)
                {
                    AddBlockPicSources(result, seen, block, origin);
                    if (result.Count == PicLimit) break;
                }
                if (result.Count < PicLimit)
                    foreach (var field in page.Fs)
                    {
                        AddPicSource(result, seen, field.S == null || field.S.Bi == null ? null : field.S.Bi.U, origin);
                        if (result.Count == PicLimit) break;
                    }
            }
            return result;
        }

        private static void AddBlockPicSources(ICollection<Uri> result, ISet<string> seen, B block, Uri origin)
        {
            if (block == null || result.Count >= PicLimit) return;
            if (block.K == "img") AddPicSource(result, seen, block.U, origin);
            if (block.K == "li")
            {
                AddPicSource(result, seen, block.S == null ? null : block.S.Li, origin);
                if (block.Ms != null && block.Ms.Mf != null)
                    foreach (var fragment in block.Ms.Mf)
                    {
                        AddPicSource(result, seen, fragment.U, origin);
                        if (result.Count == PicLimit) return;
                    }
            }
            AddPicSource(result, seen, block.S == null || block.S.Bi == null ? null : block.S.Bi.U, origin);
            foreach (var run in block.Is)
            {
                AddPicSource(result, seen, run.P, origin);
                if (result.Count == PicLimit) return;
            }
            if (block.Tb == null) return;
            foreach (var row in block.Tb.Rs)
                foreach (var cell in row.Cs)
                    foreach (var child in cell.Bs)
                    {
                        AddBlockPicSources(result, seen, child, origin);
                        if (result.Count == PicLimit) return;
                    }
        }

        private static void AddPicSource(ICollection<Uri> result, ISet<string> seen, Uri source, Uri origin)
        {
            if (source != null && source.IsFile && (source.IsUnc || origin == null || !origin.IsFile)) return;
            if (source == null || result.Count >= PicLimit || !seen.Add(source.AbsoluteUri)) return;
            result.Add(source);
        }

        private bool MissingPic(P page)
        {
            foreach (var source in PicSources(page))
                if (!pics.ContainsKey(source.AbsoluteUri)) return true;
            return false;
        }

        private void QueuePics(P page)
        {
            if (closed || page == null || loadOwner != null || picBusy || picStop || !MissingPic(page)) return;
            LoadPicsLater(page);
        }

        private async void LoadPicsLater(P page)
        {
            try
            {
                await PicsA(page);
                if (!closed && Object.ReferenceEquals(page, At))
                {
                    Dirty();
                    QueueFragment(page);
                    ApplyPendingFragment();
                    Invalidate();
                }
            }
            catch (OperationCanceledException) { }
            catch (WebException) { }
            catch (IOException) { }
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
            DropPic(key);
            pics[key] = image;
            if (image != null) picSize += (long)image.Width * image.Height * 4;
        }

        private void TrimPics(IList<Uri> sources, bool retry = false)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var source in sources) keys.Add(source.AbsoluteUri);
            // Retain only referenced resources so a full budget cannot create a repaint/refetch loop.
            foreach (var key in new List<string>(pics.Keys)) if (!keys.Contains(key) || (retry && pics[key] == null)) DropPic(key);
        }

        private void DropPic(string key)
        {
            Image image;
            if (!pics.TryGetValue(key, out image)) return;
            pics.Remove(key);
            if (image != null)
            {
                var size = (long)image.Width * image.Height * 4;
                picSize -= size;
                ReleasePic(size);
                image.Dispose();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Sc(-Math.Sign(e.Delta) * 72);
        }

        public void Sc(int delta)
        {
            var target = Math.Max(Int32.MinValue, Math.Min(Int32.MaxValue, (long)sy + delta));
            SetScroll((int)target);
            PlaceEditor();
            Invalidate();
        }

        private void SetScroll(int value, bool fragment = false)
        {
            if (!fragment) CancelFragment();
            sy = Math.Max(0, Math.Min(MaxScroll(), value));
            ApplySticky();
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
            var fallback = box == null || (!summary && !link && (box.B == null || box.B.I == null))
                ? Cursors.Default
                : box.B != null && CanEdit(box.B.I) ? Cursors.IBeam : Cursors.Hand;
            Cursor = PointerCursor(PointerStyle(box), fallback);
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
                var box = BoxAt(e.Location);
                var target = box == null ? null : FocusAt(e.Location, box.El);
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
                    var target = FocusAt(e.Location, hit.El);
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
                var key = KeyText(e.KeyData);
                if (AccessTarget(key) == null) return;
                e.SuppressKeyPress = true;
                e.Handled = true;
                await InputActivationA(() => Aa(key));
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
            e.SuppressKeyPress = true;
            e.Handled = true;
            await InputActivationA(Ka);
        }

        private async Task<int> InputActivationA(Func<Task<int>> activate)
        {
            try { return await activate(); }
            catch (Er) { return 1; }
            catch (LoadBusy) { return 1; }
        }

        // Kf moves through Lea's bounded sequential keyboard targets.
        public int Kf(bool back = false)
        {
            ForgetEdit();
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
            ForgetEdit();
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

            foreach (var box in BoxSnapshot())
            {
                if (box.Title || EllipsisMarker(box) || box.El == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
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
            foreach (var box in BoxSnapshot())
            {
                if (mapRegions == MaxKeyMapRegions) break;
                if (box.Title || box.B == null || box.B.Mp == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                var source = MapSourceSize(box.B, box.C);
                var image = PicObjectRect(box.B, box.C);
                var visible = Rectangle.Intersect(box.C, image);
                foreach (var area in box.B.Mp.As)
                {
                    if (mapRegions == MaxKeyMapRegions) break;
                    mapRegions++;
                    if (HtmlHidden(area.El)) continue;
                    var tab = e.Ti(area.El);
                    if (area.U == null && !tab.HasValue) continue;
                    if (sequential && tab.HasValue && tab.Value < 0) continue;
                    var rect = Rectangle.Intersect(visible, MapBounds(area, image, source));
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

        private Kt FocusAt(Point point, N preferred = null)
        {
            Kt found = null;
            var size = Int64.MaxValue;
            foreach (var target in FocusTargets(false))
            {
                if (preferred != null && !Object.ReferenceEquals(target.El, preferred) && !OwnsElement(target.El, preferred) && !OwnsElement(preferred, target.El)) continue;
                foreach (var region in target.Rs)
                {
                    var rect = ScreenRect(region);
                    if (!rect.Contains(point)) continue;
                    var area = (long)rect.Width * rect.Height;
                    if (area >= size) continue;
                    found = target;
                    size = area;
                }
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
                    SetScroll(region.R.Top - 24);
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
                label = E.Tx(label, field.S);
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
            SetEditFont(field);
            ed.Text = field.V;
            editLock = false;
            PlaceEditor();
            if (!ed.Visible) return;
            ed.Focus();
            ed.SelectAll();
        }

        private void SetEditFont(Fi field)
        {
            var nextFont = BoxFont(field.K, field.S, false);
            if (editFont != null && editFont.Equals(nextFont)) nextFont.Dispose();
            else
            {
                // WinForms may retain an equal Font instance in its property setter.
                var oldFont = editFont;
                ed.Font = nextFont;
                editFont = nextFont;
                if (oldFont != null) oldFont.Dispose();
            }
            editStyle = field.S;
        }

        private void ForgetEdit()
        {
            heldEdit = null;
            editRedo = false;
            ed.ClearUndo();
        }

        private void EndEdit(bool restore, bool retain = false)
        {
            if (!retain) ForgetEdit();
            if (edit == null) return;
            if (retain) heldEdit = edit;
            if (restore) edit.V = editStart;
            edit = null;
            ed.Visible = false;
            SetFocus(null);
            Invalidate();
        }

        private void EditChanged(object sender, EventArgs args)
        {
            if (editLock || edit == null) return;
            if (!editReplay) editRedo = false;
            edit.V = ed.Text;
            var restyle = ClearValidity(edit);
            OnEdited();
            NotifyAccessibility(AccessibleEvents.ValueChange);
            if (restyle) Restyle(); else Invalidate();
        }

        private async void EditKeyDown(object sender, KeyEventArgs args)
        {
            if (!args.Alt && ((args.Control && !args.Shift && args.KeyCode == Keys.V) ||
                (args.Shift && !args.Control && args.KeyCode == Keys.Insert)))
            {
                args.SuppressKeyPress = true;
                args.Handled = true;
                Ec("pa");
            }
            else if (args.Control && !args.Alt && !args.Shift && (args.KeyCode == Keys.Z || args.KeyCode == Keys.Y))
            {
                Ec(args.KeyCode == Keys.Z ? "un" : "re");
                args.SuppressKeyPress = true;
                args.Handled = true;
            }
            else if (args.Alt && args.Shift && !args.Control)
            {
                var key = KeyText(args.KeyData);
                if (AccessTarget(key) == null) return;
                args.SuppressKeyPress = true;
                args.Handled = true;
                await InputActivationA(() => Aa(key));
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
                if (!Object.ReferenceEquals(editStyle, edit.S)) SetEditFont(edit);
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
            CheckOpen();
            EndEdit(false);
            if (button == null || (!button.Nv && !Vf(button))) return At;
            if (!ValidForm(button)) return At;
            var data = FormData(button);
            var multipart = button.M == "post" && button.En == "m" && CanPost(button.U) ? MultipartData(button) : null;
            if (Sb != null) Sb(this, EventArgs.Empty);
            if (closed) return At;
            if (button.M == "get") return Go(FormUri(button, data));
            if (button.M != "post" || !CanPost(button.U)) return At;
            if (button.En != "m") return Po(button.U, data);
            return Pb(button.U, multipart.B, multipart.Ct);
        }

        public async Task<P> SmA(Fi button)
        {
            CheckOpen();
            EndEdit(false);
            if (button == null || (!button.Nv && !Vf(button))) return At;
            if (!ValidForm(button)) return At;
            var data = FormData(button);
            var multipart = button.M == "post" && button.En == "m" && CanPost(button.U) ? MultipartData(button) : null;
            if (Sb != null) Sb(this, EventArgs.Empty);
            if (closed) return At;
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
            var snapshot = BoxSnapshot();
            for (var i = snapshot.Length - 1; i >= 0; i--)
            {
                var box = snapshot[i];
                if (box.El == null || Hidden(box.S) || !PointerEnabled(box.S)) continue;
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

        private static bool PointerEnabled(S style)
        {
            return style == null || style.Pe;
        }

        private S PointerStyle(Bx box)
        {
            if (box == null) return At == null ? null : At.S;
            if (At == null) return box.S;
            var element = box.B != null && box.B.I != null ? box.B.I.El : box.B != null ? box.B.El : box.El;
            if (element == null) return box.S;
            foreach (var field in At.Fs)
                if (Object.ReferenceEquals(field.El, element)) return field.S;
            foreach (var block in At.Bs)
                if (Object.ReferenceEquals(block.El, element)) return block.S;
            return box.S;
        }

        private static Cursor PointerCursor(S style, Cursor fallback)
        {
            var cursor = style == null ? "" : style.Cu;
            if (cursor == "d") return Cursors.Default;
            if (cursor == "p") return Cursors.Hand;
            if (cursor == "t") return Cursors.IBeam;
            if (cursor == "w") return Cursors.WaitCursor;
            if (cursor == "c") return Cursors.Cross;
            if (cursor == "m") return Cursors.SizeAll;
            if (cursor == "h") return Cursors.Help;
            if (cursor == "n") return Cursors.No;
            if (cursor == "e") return Cursors.SizeWE;
            if (cursor == "s") return Cursors.SizeNS;
            return fallback;
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

        private static bool TextSpaced(S style)
        {
            return style != null && (style.Ls != 0 || style.Wg != 0);
        }

        private static int TextWidth(Graphics graphics, string text, Font font, S style)
        {
            int last;
            return TextWidth(graphics, text, font, style, out last);
        }

        private static int TextWidth(Graphics graphics, string text, Font font, S style, out int last)
        {
            text = text ?? "";
            last = 0;
            if (!TextSpaced(style))
                return TextRenderer.MeasureText(graphics, text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
            var width = 0;
            foreach (var element in SpacingUnits(text, style))
            {
                var current = SpacingUnitWidth(graphics, element, font, style);
                width += (last == 0 ? 0 : TextGap(last, style, style)) + current;
                last = current;
                if (width >= 8192) return 8192;
            }
            return Math.Max(1, width);
        }

        private static int TextGap(int leftWidth, S left, S right)
        {
            var spacing = ((left == null ? 0 : left.Ls) + (right == null ? 0 : right.Ls)) / 2D;
            return Math.Max(1 - leftWidth, (int)Math.Round(spacing, MidpointRounding.AwayFromZero));
        }

        private static IEnumerable<string> SpacingUnits(string text, S style)
        {
            if (style.Ls == 0)
            {
                var start = 0;
                for (var index = 0; index < text.Length; index++)
                {
                    if (text[index] != ' ' && text[index] != '\u00a0') continue;
                    if (index > start) yield return text.Substring(start, index - start);
                    yield return text[index].ToString();
                    start = index + 1;
                }
                if (start < text.Length) yield return text.Substring(start);
                yield break;
            }
            var elements = StringInfo.GetTextElementEnumerator(text);
            while (elements.MoveNext()) yield return elements.GetTextElement();
        }

        private static int SpacingUnitWidth(Graphics graphics, string text, Font font, S style)
        {
            var native = TextRenderer.MeasureText(graphics, text, font, Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            var spaces = 0;
            foreach (var ch in text) if (ch == ' ' || ch == '\u00a0') spaces++;
            return Math.Max(1, native + spaces * style.Wg);
        }

        private static bool TextSpaceElement(string value)
        {
            if (String.IsNullOrEmpty(value)) return false;
            foreach (var ch in value) if (!Char.IsWhiteSpace(ch)) return false;
            return true;
        }

        private static void PaintText(Graphics graphics, string text, Font font, Rectangle rect, Color color, TextFormatFlags flags, int opacity, S style = null, bool link = false)
        {
            if (opacity <= 0 || rect.Width < 1 || rect.Height < 1) return;
            PaintTextShadow(graphics, text, font, rect, color, flags, opacity, style);
            PaintTextCore(graphics, text, font, rect, color, flags, opacity, style);
            PaintTextDecoration(graphics, text, font, rect, color, flags, opacity, style, link);
        }

        private static void PaintTextShadow(Graphics graphics, string text, Font font, Rectangle rect, Color currentColor, TextFormatFlags flags, int opacity, S style)
        {
            var shadow = style == null ? null : style.Ts;
            if (shadow == null) return;
            var source = CssColor(shadow.C, currentColor);
            var shadowOpacity = source.A * Math.Max(0, Math.Min(100, opacity)) / 255;
            if (shadowOpacity <= 0) return;
            var color = Color.FromArgb(source.R, source.G, source.B);
            var origin = rect;
            origin.Offset(shadow.X, shadow.Y);
            if (shadow.B <= 0)
            {
                PaintTextCore(graphics, text, font, origin, color, flags, shadowOpacity, style);
                return;
            }
            var steps = Math.Min(4, Math.Max(1, (shadow.B + 3) / 4));
            var layerOpacity = Math.Max(1, shadowOpacity / (steps * 4));
            for (var step = steps; step > 0; step--)
            {
                var radius = Math.Max(1, (int)Math.Ceiling(shadow.B * step / (double)steps));
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        var sample = origin;
                        sample.Offset(x * radius, y * radius);
                        PaintTextCore(graphics, text, font, sample, color, flags, layerOpacity, style);
                    }
                }
            }
            PaintTextCore(graphics, text, font, origin, color, flags, Math.Max(1, shadowOpacity / (steps + 1)), style);
        }

        private static void PaintTextCore(Graphics graphics, string text, Font font, Rectangle rect, Color color, TextFormatFlags flags, int opacity, S style)
        {
            if (TextSpaced(style))
            {
                PaintSpacedText(graphics, text, font, rect, color, flags, opacity, style);
                return;
            }
            if (opacity >= 100)
            {
                TextRenderer.DrawText(graphics, text, font, rect, color, flags);
                return;
            }
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

        private static void PaintTextDecoration(Graphics graphics, string text, Font font, Rectangle rect, Color currentColor, TextFormatFlags flags, int opacity, S style, bool defaultUnderline)
        {
            if (!CustomDecoration(style, defaultUnderline) || String.IsNullOrEmpty(text)) return;
            var underline = style.Td.IndexOf('u') >= 0 || (defaultUnderline && style.Td.Length == 0);
            var strike = style.Td.IndexOf('s') >= 0;
            if (!underline && !strike) return;
            var source = CssColor(style.Tc, currentColor);
            var decorationOpacity = source.A * Math.Max(0, Math.Min(100, opacity)) / 255;
            if (decorationOpacity <= 0) return;
            var color = PaintColor(Color.FromArgb(source.R, source.G, source.B), decorationOpacity);
            var thickness = style.Tw > 0 ? style.Tw : Math.Max(1, (int)Math.Round(font.Size / 14F));
            var width = TextSpaced(style) ? TextWidth(graphics, text, font, style) :
                TextRenderer.MeasureText(graphics, text, font, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            width = Math.Max(1, Math.Min(rect.Width, width));
            var x = (flags & TextFormatFlags.HorizontalCenter) != 0 ? rect.Left + Math.Max(0, (rect.Width - width) / 2) :
                (flags & TextFormatFlags.Right) != 0 ? rect.Right - width : rect.Left;
            var top = (flags & TextFormatFlags.VerticalCenter) != 0 ? rect.Top + Math.Max(0, (rect.Height - font.Height) / 2) : rect.Top;
            var underlineOffset = UnderlineOffset(style, font);
            var state = graphics.Save();
            try
            {
                var decorationClip = rect;
                if (underlineOffset != 0) decorationClip.Inflate(0, Math.Min(72, Math.Abs(underlineOffset) + thickness * 2));
                graphics.SetClip(decorationClip, CombineMode.Intersect);
                if (underline) PaintDecorationLine(graphics, x, x + width - 1, top + font.Height - 2 + underlineOffset, color, thickness, style.Tds);
                if (strike) PaintDecorationLine(graphics, x, x + width - 1, Math.Min(rect.Bottom - 1, top + font.Height / 2), color, thickness, style.Tds);
            }
            finally { graphics.Restore(state); }
        }

        private static int UnderlineOffset(S style, Font font)
        {
            if (style == null || style.Uo == 0) return 0;
            var value = style.Uop ? (int)Math.Round((style.Fs > 0 ? style.Fs : font.Size) * style.Uo / 100D) : style.Uo;
            return Math.Max(-64, Math.Min(64, value));
        }

        private static void PaintDecorationLine(Graphics graphics, int left, int right, int y, Color color, int thickness, string type)
        {
            if (right < left) return;
            using (var pen = new Pen(color, Math.Max(1, thickness)))
            {
                if (type == "d")
                {
                    var gap = Math.Max(1, thickness);
                    graphics.DrawLine(pen, left, y - gap, right, y - gap);
                    graphics.DrawLine(pen, left, y + gap, right, y + gap);
                    return;
                }
                if (type == "o") { pen.DashStyle = DashStyle.Dot; pen.DashCap = DashCap.Round; }
                else if (type == "a") pen.DashStyle = DashStyle.Dash;
                else if (type == "w")
                {
                    pen.Width = Math.Max(1, (thickness + 1) / 2F);
                    var amplitude = Math.Max(1, thickness);
                    var step = Math.Max(2, thickness * 2);
                    var x = left;
                    var up = true;
                    while (x < right)
                    {
                        var next = Math.Min(right, x + step);
                        graphics.DrawLine(pen, x, y + (up ? -amplitude : amplitude), next, y + (up ? amplitude : -amplitude));
                        x = next;
                        up = !up;
                    }
                    return;
                }
                graphics.DrawLine(pen, left, y, right, y);
            }
        }

        private static void PaintSpacedText(Graphics graphics, string text, Font font, Rectangle rect, Color color, TextFormatFlags flags, int opacity, S style)
        {
            if (opacity <= 0 || rect.Width < 1 || rect.Height < 1) return;
            var lines = SpacedTextLines(graphics, text, font, style, rect.Width,
                (flags & TextFormatFlags.SingleLine) == 0 && (flags & TextFormatFlags.WordBreak) != 0,
                Math.Max(1, rect.Height / Math.Max(1, font.Height) + 1));
            var totalHeight = Math.Max(font.Height, lines.Count * font.Height);
            var y = (flags & TextFormatFlags.VerticalCenter) != 0 ? rect.Top + Math.Max(0, (rect.Height - totalHeight) / 2) : rect.Top;
            var state = graphics.Save();
            try
            {
                graphics.SetClip(rect, CombineMode.Intersect);
                foreach (var line in lines)
                {
                    if (y >= rect.Bottom) break;
                    var width = TextWidth(graphics, line, font, style);
                    var x = (flags & TextFormatFlags.HorizontalCenter) != 0 ? rect.Left + Math.Max(0, (rect.Width - width) / 2) :
                        (flags & TextFormatFlags.Right) != 0 ? rect.Right - width : rect.Left;
                    PaintSpacedLine(graphics, line, font, x, y, rect, color, opacity, style);
                    y += font.Height;
                }
            }
            finally { graphics.Restore(state); }
        }

        private static IList<string> SpacedTextLines(Graphics graphics, string text, Font font, S style, int width, bool wrap, int maxLines)
        {
            var result = new List<string>();
            var source = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
            if (!wrap)
            {
                result.Add(source.Replace('\n', ' '));
                return result;
            }
            foreach (var paragraph in source.Split('\n'))
            {
                var line = new StringBuilder();
                var lineWidth = 0;
                var lastWidth = 0;
                foreach (Match match in Regex.Matches(paragraph, "\\S+|\\s+"))
                {
                    var token = match.Value;
                    int tokenLast;
                    var tokenWidth = TextWidth(graphics, token, font, style, out tokenLast);
                    var gap = line.Length == 0 ? 0 : TextGap(lastWidth, style, style);
                    if (line.Length > 0 && !TextSpaceElement(token) && lineWidth + gap + tokenWidth > width)
                    {
                        result.Add(line.ToString().TrimEnd());
                        if (result.Count >= maxLines) return result;
                        line.Clear();
                        lineWidth = 0;
                        gap = 0;
                    }
                    if (tokenWidth <= width || TextSpaceElement(token))
                    {
                        if (line.Length == 0 && TextSpaceElement(token)) continue;
                        line.Append(token);
                        lineWidth += gap + tokenWidth;
                        lastWidth = tokenLast;
                        continue;
                    }
                    var elements = StringInfo.GetTextElementEnumerator(token);
                    while (elements.MoveNext())
                    {
                        var element = elements.GetTextElement();
                        var elementWidth = TextWidth(graphics, element, font, style);
                        gap = line.Length == 0 ? 0 : TextGap(lastWidth, style, style);
                        if (line.Length > 0 && lineWidth + gap + elementWidth > width)
                        {
                            result.Add(line.ToString());
                            if (result.Count >= maxLines) return result;
                            line.Clear();
                            lineWidth = 0;
                            gap = 0;
                        }
                        line.Append(element);
                        lineWidth += gap + elementWidth;
                        lastWidth = elementWidth;
                    }
                }
                result.Add(line.ToString().TrimEnd());
                if (result.Count >= maxLines) return result;
            }
            if (result.Count == 0) result.Add("");
            return result;
        }

        private static void PaintSpacedLine(Graphics graphics, string text, Font font, int x, int y, Rectangle clip, Color color, int opacity, S style)
        {
            using (var format = opacity < 100 ? (StringFormat)StringFormat.GenericTypographic.Clone() : null)
            using (var brush = opacity < 100 ? new SolidBrush(PaintColor(color, opacity)) : null)
            {
                var last = 0;
                foreach (var element in SpacingUnits(text ?? "", style))
                {
                    if (last > 0) x += TextGap(last, style, style);
                    if (x >= clip.Right) break;
                    var current = SpacingUnitWidth(graphics, element, font, style);
                    if (opacity >= 100)
                        TextRenderer.DrawText(graphics, element, font, new Rectangle(x, y, Math.Max(1, clip.Right - x), font.Height), color,
                            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping);
                    else graphics.DrawString(element, font, brush, new PointF(x, y), format);
                    x += current;
                    last = current;
                }
            }
        }

        private static void PaintImage(Graphics graphics, Image image, Rectangle rect, int opacity)
        {
            if (image == null || opacity <= 0 || rect.Width < 1 || rect.Height < 1 || image.Width < 1 || image.Height < 1) return;
            if (opacity >= 100)
            {
                graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                return;
            }
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
            var source = value.Trim();
            Color color;
            return CssMixColor(source, fallback, out color) || CssBaseColor(source, fallback, out color) ? color : fallback;
        }

        private static bool CssBaseColor(string source, Color fallback, out Color color)
        {
            color = Color.Empty;
            if (String.IsNullOrWhiteSpace(source) || source.Length > 64) return false;
            if (source.Equals("currentcolor", StringComparison.OrdinalIgnoreCase)) { color = fallback; return true; }
            if (source.Equals("transparent", StringComparison.OrdinalIgnoreCase)) { color = Color.Transparent; return true; }
            if (CssHexColor(source, out color) || CssFunctionColor(source, out color)) return true;
            try { color = ColorTranslator.FromHtml(source); return true; }
            catch { color = Color.Empty; return false; }
        }

        private static bool CssMixColor(string source, Color fallback, out Color color)
        {
            color = Color.Empty;
            if (String.IsNullOrWhiteSpace(source) || source.Length > 160 ||
                !source.StartsWith("color-mix(", StringComparison.OrdinalIgnoreCase) ||
                !source.EndsWith(")", StringComparison.Ordinal)) return false;
            List<string> groups;
            if (!CssColorParts(source.Substring(10, source.Length - 11), ',', out groups) || groups.Count != 3 ||
                !Regex.IsMatch(groups[0], "^in\\s+srgb$", RegexOptions.IgnoreCase)) return false;
            Color first;
            Color second;
            double firstWeight;
            double secondWeight;
            bool hasFirst;
            bool hasSecond;
            if (!CssMixStop(groups[1], fallback, out first, out firstWeight, out hasFirst) ||
                !CssMixStop(groups[2], fallback, out second, out secondWeight, out hasSecond)) return false;
            if (!hasFirst && !hasSecond) { firstWeight = secondWeight = .5D; }
            else
            {
                if (!hasFirst) firstWeight = 1D - secondWeight;
                if (!hasSecond) secondWeight = 1D - firstWeight;
            }
            var total = firstWeight + secondWeight;
            if (firstWeight < 0D || secondWeight < 0D || total <= 0D) return false;
            var alphaScale = Math.Min(1D, total);
            firstWeight /= total;
            secondWeight /= total;
            var firstAlpha = first.A / 255D;
            var secondAlpha = second.A / 255D;
            var mixedAlpha = firstWeight * firstAlpha + secondWeight * secondAlpha;
            var red = 0D;
            var green = 0D;
            var blue = 0D;
            if (mixedAlpha > 0D)
            {
                red = (first.R / 255D * firstAlpha * firstWeight + second.R / 255D * secondAlpha * secondWeight) / mixedAlpha;
                green = (first.G / 255D * firstAlpha * firstWeight + second.G / 255D * secondAlpha * secondWeight) / mixedAlpha;
                blue = (first.B / 255D * firstAlpha * firstWeight + second.B / 255D * secondAlpha * secondWeight) / mixedAlpha;
            }
            color = Color.FromArgb(CssByte(mixedAlpha * alphaScale), CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static bool CssMixStop(string source, Color fallback, out Color color, out double weight, out bool hasWeight)
        {
            color = Color.Empty;
            weight = 0D;
            hasWeight = false;
            List<string> parts;
            if (!CssColorParts((source ?? "").Trim(), '\0', out parts) || parts.Count < 1 || parts.Count > 2 ||
                !CssBaseColor(parts[0], fallback, out color)) return false;
            if (parts.Count == 1) return true;
            hasWeight = CssMixPercent(parts[1], out weight);
            return hasWeight;
        }

        private static bool CssMixPercent(string source, out double value)
        {
            value = 0D;
            if (String.IsNullOrWhiteSpace(source) || !source.EndsWith("%", StringComparison.Ordinal) ||
                !Double.TryParse(source.Substring(0, source.Length - 1), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out value) || Double.IsNaN(value) || Double.IsInfinity(value) ||
                value < 0D || value > 100D) return false;
            value /= 100D;
            return true;
        }

        private static bool CssColorParts(string source, char separator, out List<string> parts)
        {
            parts = new List<string>();
            var start = 0;
            var parentheses = 0;
            for (var index = 0; index <= source.Length; index++)
            {
                var end = index == source.Length;
                var value = end ? separator : source[index];
                if (!end && value == '(') { parentheses++; continue; }
                if (!end && value == ')') { if (parentheses == 0) return false; parentheses--; continue; }
                var split = end || (parentheses == 0 && (separator == '\0' ? Char.IsWhiteSpace(value) : value == separator));
                if (!split) continue;
                var part = source.Substring(start, index - start).Trim();
                if (part.Length > 0) parts.Add(part);
                else if (separator != '\0') return false;
                start = index + 1;
            }
            return parentheses == 0;
        }

        private static bool CssHexColor(string source, out Color color)
        {
            color = Color.Empty;
            if (String.IsNullOrEmpty(source) || source[0] != '#' || (source.Length != 4 && source.Length != 5 && source.Length != 7 && source.Length != 9)) return false;
            try
            {
                var shortForm = source.Length < 6;
                var red = Convert.ToInt32(source.Substring(1, shortForm ? 1 : 2), 16);
                var green = Convert.ToInt32(source.Substring(shortForm ? 2 : 3, shortForm ? 1 : 2), 16);
                var blue = Convert.ToInt32(source.Substring(shortForm ? 3 : 5, shortForm ? 1 : 2), 16);
                var alpha = source.Length == 5 ? Convert.ToInt32(source.Substring(4, 1), 16) : source.Length == 9 ? Convert.ToInt32(source.Substring(7, 2), 16) : 255;
                if (shortForm) { red *= 17; green *= 17; blue *= 17; if (source.Length == 5) alpha *= 17; }
                color = Color.FromArgb(alpha, red, green, blue);
                return true;
            }
            catch (FormatException) { return false; }
        }

        private static bool CssFunctionColor(string source, out Color color)
        {
            color = Color.Empty;
            var match = Regex.Match(source ?? "", "^rgba?\\((?<v>[^()]*)\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssRgbColor(match.Groups["v"].Value, out color);
            match = Regex.Match(source ?? "", "^hsla?\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssHslColor(match.Groups["v"].Value, out color);
            match = Regex.Match(source ?? "", "^hwb\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssHwbColor(match.Groups["v"].Value, out color);
            match = Regex.Match(source ?? "", "^lab\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssLabColor(match.Groups["v"].Value, false, out color);
            match = Regex.Match(source ?? "", "^lch\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssLabColor(match.Groups["v"].Value, true, out color);
            match = Regex.Match(source ?? "", "^color\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssPredefinedColor(match.Groups["v"].Value, out color);
            match = Regex.Match(source ?? "", "^oklab\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            if (match.Success) return CssOklabColor(match.Groups["v"].Value, false, out color);
            match = Regex.Match(source ?? "", "^oklch\\((?<v>[^()]{1,80})\\)$", RegexOptions.IgnoreCase);
            return match.Success && CssOklabColor(match.Groups["v"].Value, true, out color);
        }

        private static bool CssRgbColor(string value, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            string[] channels;
            string alpha = null;
            if (body.IndexOf(',') >= 0)
            {
                var values = body.Split(',');
                if (values.Length < 3 || values.Length > 4) return false;
                channels = new string[] { values[0].Trim(), values[1].Trim(), values[2].Trim() };
                if (values.Length == 4) alpha = values[3].Trim();
            }
            else
            {
                var slash = body.IndexOf('/');
                if (slash >= 0)
                {
                    if (body.IndexOf('/', slash + 1) >= 0) return false;
                    alpha = body.Substring(slash + 1).Trim();
                    body = body.Substring(0, slash).Trim();
                }
                channels = Regex.Split(body, "\\s+");
                if (channels.Length != 3) return false;
            }
            int red;
            int green;
            int blue;
            int opacity;
            if (!CssColorChannel(channels[0], out red) || !CssColorChannel(channels[1], out green) || !CssColorChannel(channels[2], out blue) || !CssAlpha(alpha, out opacity)) return false;
            color = Color.FromArgb(opacity, red, green, blue);
            return true;
        }

        private static bool CssHslColor(string value, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            string[] channels;
            string alpha = null;
            if (body.IndexOf(',') >= 0)
            {
                var values = body.Split(',');
                if (values.Length < 3 || values.Length > 4 || body.IndexOf('/') >= 0) return false;
                channels = new string[] { values[0].Trim(), values[1].Trim(), values[2].Trim() };
                if (values.Length == 4) alpha = values[3].Trim();
            }
            else
            {
                var slash = body.IndexOf('/');
                if (slash >= 0)
                {
                    if (body.IndexOf('/', slash + 1) >= 0) return false;
                    alpha = body.Substring(slash + 1).Trim();
                    body = body.Substring(0, slash).Trim();
                }
                channels = Regex.Split(body, "\\s+");
                if (channels.Length != 3) return false;
            }
            double hue;
            double saturation;
            double lightness;
            int opacity;
            if (!CssHue(channels[0], out hue) || !CssPercent(channels[1], out saturation) ||
                !CssPercent(channels[2], out lightness) || !CssAlpha(alpha, out opacity)) return false;
            double red;
            double green;
            double blue;
            CssHslRgb(hue, saturation, lightness, out red, out green, out blue);
            color = Color.FromArgb(opacity, CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static bool CssHwbColor(string value, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            if (body.IndexOf(',') >= 0) return false;
            string alpha = null;
            var slash = body.IndexOf('/');
            if (slash >= 0)
            {
                if (body.IndexOf('/', slash + 1) >= 0) return false;
                alpha = body.Substring(slash + 1).Trim();
                body = body.Substring(0, slash).Trim();
            }
            var channels = Regex.Split(body, "\\s+");
            double hue;
            double whiteness;
            double blackness;
            int opacity;
            if (channels.Length != 3 || !CssHue(channels[0], out hue) || !CssPercent(channels[1], out whiteness) ||
                !CssPercent(channels[2], out blackness) || !CssAlpha(alpha, out opacity)) return false;
            double red;
            double green;
            double blue;
            if (whiteness + blackness >= 1D)
            {
                red = green = blue = whiteness / (whiteness + blackness);
            }
            else
            {
                CssHslRgb(hue, 1D, .5D, out red, out green, out blue);
                var factor = 1D - whiteness - blackness;
                red = red * factor + whiteness;
                green = green * factor + whiteness;
                blue = blue * factor + whiteness;
            }
            color = Color.FromArgb(opacity, CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static bool CssOklabColor(string value, bool polar, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            if (body.IndexOf(',') >= 0) return false;
            string alpha = null;
            var slash = body.IndexOf('/');
            if (slash >= 0)
            {
                if (body.IndexOf('/', slash + 1) >= 0) return false;
                alpha = body.Substring(slash + 1).Trim();
                body = body.Substring(0, slash).Trim();
            }
            var channels = Regex.Split(body, "\\s+");
            double lightness;
            double second;
            double third;
            int opacity;
            if (channels.Length != 3 || !CssOklabComponent(channels[0], 1D, 0D, 1D, out lightness) ||
                !CssOklabComponent(channels[1], .4D, polar ? 0D : -4D, 4D, out second) ||
                (polar ? !CssHue(channels[2], out third) : !CssOklabComponent(channels[2], .4D, -4D, 4D, out third)) ||
                !CssAlpha(alpha, out opacity)) return false;
            var a = polar ? second * Math.Cos(third * Math.PI / 180D) : second;
            var b = polar ? second * Math.Sin(third * Math.PI / 180D) : third;
            double red;
            double green;
            double blue;
            CssOklabRgb(lightness, a, b, out red, out green, out blue);
            color = Color.FromArgb(opacity, CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static bool CssLabColor(string value, bool polar, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            if (body.IndexOf(',') >= 0) return false;
            string alpha = null;
            var slash = body.IndexOf('/');
            if (slash >= 0)
            {
                if (body.IndexOf('/', slash + 1) >= 0) return false;
                alpha = body.Substring(slash + 1).Trim();
                body = body.Substring(0, slash).Trim();
            }
            var channels = Regex.Split(body, "\\s+");
            double lightness;
            double second;
            double third;
            int opacity;
            if (channels.Length != 3 || !CssOklabComponent(channels[0], 100D, 0D, 100D, out lightness) ||
                !CssOklabComponent(channels[1], polar ? 150D : 125D, polar ? 0D : -1250D, polar ? 1500D : 1250D, out second) ||
                (polar ? !CssHue(channels[2], out third) : !CssOklabComponent(channels[2], 125D, -1250D, 1250D, out third)) ||
                !CssAlpha(alpha, out opacity)) return false;
            if (lightness <= 0D) { color = Color.FromArgb(opacity, 0, 0, 0); return true; }
            if (lightness >= 100D) { color = Color.FromArgb(opacity, 255, 255, 255); return true; }
            var a = polar ? second * Math.Cos(third * Math.PI / 180D) : second;
            var b = polar ? second * Math.Sin(third * Math.PI / 180D) : third;
            double okLightness;
            double okA;
            double okB;
            CssLabToOklab(lightness, a, b, out okLightness, out okA, out okB);
            double red;
            double green;
            double blue;
            CssOklabRgb(okLightness, okA, okB, out red, out green, out blue);
            color = Color.FromArgb(opacity, CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static void CssLabToOklab(double lightness, double a, double b,
            out double okLightness, out double okA, out double okB)
        {
            const double epsilon = 216D / 24389D;
            const double kappa = 24389D / 27D;
            var fy = (lightness + 16D) / 116D;
            var fx = a / 500D + fy;
            var fz = fy - b / 200D;
            var fx3 = fx * fx * fx;
            var fy3 = fy * fy * fy;
            var fz3 = fz * fz * fz;
            var x = (fx3 > epsilon ? fx3 : (116D * fx - 16D) / kappa) * (.3457D / .3585D);
            var y = lightness > kappa * epsilon ? fy3 : lightness / kappa;
            var z = (fz3 > epsilon ? fz3 : (116D * fz - 16D) / kappa) * ((1D - .3457D - .3585D) / .3585D);
            double d65X;
            double d65Y;
            double d65Z;
            CssD50ToD65(x, y, z, out d65X, out d65Y, out d65Z);
            CssXyzToOklab(d65X, d65Y, d65Z, out okLightness, out okA, out okB);
        }

        private static void CssD50ToD65(double x, double y, double z,
            out double d65X, out double d65Y, out double d65Z)
        {
            d65X = .955473421488075D * x - .02309845494876471D * y + .06325924320057072D * z;
            d65Y = -.0283697093338637D * x + 1.0099953980813041D * y + .021041441191917323D * z;
            d65Z = .012314014864481998D * x - .020507649298898964D * y + 1.330365926242124D * z;
        }

        private static bool CssPredefinedColor(string value, out Color color)
        {
            color = Color.Empty;
            var body = (value ?? "").Trim();
            if (body.IndexOf(',') >= 0) return false;
            string alpha = null;
            var slash = body.IndexOf('/');
            if (slash >= 0)
            {
                if (body.IndexOf('/', slash + 1) >= 0) return false;
                alpha = body.Substring(slash + 1).Trim();
                body = body.Substring(0, slash).Trim();
            }
            var channels = Regex.Split(body, "\\s+");
            if (channels.Length != 4) return false;
            var space = channels[0].ToLowerInvariant();
            if (space != "srgb" && space != "srgb-linear" &&
                space != "display-p3" && space != "display-p3-linear" &&
                space != "a98-rgb" && space != "prophoto-rgb" && space != "rec2020" &&
                space != "xyz" && space != "xyz-d50" && space != "xyz-d65") return false;
            double first;
            double second;
            double third;
            int opacity;
            if (!CssOklabComponent(channels[1], 1D, -10D, 10D, out first) ||
                !CssOklabComponent(channels[2], 1D, -10D, 10D, out second) ||
                !CssOklabComponent(channels[3], 1D, -10D, 10D, out third) ||
                !CssAlpha(alpha, out opacity)) return false;
            if (space == "srgb" || space == "display-p3")
            {
                first = CssSrgbLinear(first);
                second = CssSrgbLinear(second);
                third = CssSrgbLinear(third);
            }
            else if (space == "a98-rgb")
            {
                first = CssSignedPower(first, 563D / 256D);
                second = CssSignedPower(second, 563D / 256D);
                third = CssSignedPower(third, 563D / 256D);
            }
            else if (space == "prophoto-rgb")
            {
                first = CssProPhotoLinear(first);
                second = CssProPhotoLinear(second);
                third = CssProPhotoLinear(third);
            }
            else if (space == "rec2020")
            {
                first = CssSignedPower(first, 2.4D);
                second = CssSignedPower(second, 2.4D);
                third = CssSignedPower(third, 2.4D);
            }
            double x;
            double y;
            double z;
            if (space.StartsWith("display-p3", StringComparison.Ordinal))
            {
                x = 608311D / 1250200D * first + 189793D / 714400D * second + 198249D / 1000160D * third;
                y = 35783D / 156275D * first + 247089D / 357200D * second + 198249D / 2500400D * third;
                z = 32229D / 714400D * second + 5220557D / 5000800D * third;
            }
            else if (space == "srgb" || space == "srgb-linear")
            {
                x = 506752D / 1228815D * first + 87881D / 245763D * second + 12673D / 70218D * third;
                y = 87098D / 409605D * first + 175762D / 245763D * second + 12673D / 175545D * third;
                z = 7918D / 409605D * first + 87881D / 737289D * second + 1001167D / 1053270D * third;
            }
            else if (space == "a98-rgb")
            {
                x = 573536D / 994567D * first + 263643D / 1420810D * second + 187206D / 994567D * third;
                y = 591459D / 1989134D * first + 6239551D / 9945670D * second + 374412D / 4972835D * third;
                z = 53769D / 1989134D * first + 351524D / 4972835D * second + 4929758D / 4972835D * third;
            }
            else if (space == "prophoto-rgb")
            {
                x = .79776664490064230D * first + .13518129740053308D * second + .03134773412839220D * third;
                y = .28807482881940130D * first + .71183523424187300D * second + .00008993693872564D * third;
                z = .82510460251046020D * third;
            }
            else if (space == "rec2020")
            {
                x = 63426534D / 99577255D * first + 20160776D / 139408157D * second + 47086771D / 278816314D * third;
                y = 26158966D / 99577255D * first + 472592308D / 697040785D * second + 8267143D / 139408157D * third;
                z = 19567812D / 697040785D * second + 295819943D / 278816314D * third;
            }
            else
            {
                x = first;
                y = second;
                z = third;
            }
            if (space == "xyz-d50" || space == "prophoto-rgb")
            {
                double adaptedX;
                double adaptedY;
                double adaptedZ;
                CssD50ToD65(x, y, z, out adaptedX, out adaptedY, out adaptedZ);
                x = adaptedX;
                y = adaptedY;
                z = adaptedZ;
            }
            double lightness;
            double a;
            double b;
            CssXyzToOklab(x, y, z, out lightness, out a, out b);
            double red;
            double green;
            double blue;
            CssOklabRgb(lightness, a, b, out red, out green, out blue);
            color = Color.FromArgb(opacity, CssByte(red), CssByte(green), CssByte(blue));
            return true;
        }

        private static bool CssOklabComponent(string source, double percentScale, double minimum, double maximum, out double value)
        {
            value = 0D;
            if (String.IsNullOrWhiteSpace(source)) return false;
            var percent = source.EndsWith("%", StringComparison.Ordinal);
            double number;
            if (!Double.TryParse(percent ? source.Substring(0, source.Length - 1) : source, NumberStyles.Float,
                CultureInfo.InvariantCulture, out number) || Double.IsNaN(number) || Double.IsInfinity(number)) return false;
            value = percent ? number * percentScale / 100D : number;
            value = Math.Max(minimum, Math.Min(maximum, value));
            return true;
        }

        private static void CssOklabRgb(double lightness, double a, double b,
            out double red, out double green, out double blue)
        {
            if (lightness <= 0D) { red = green = blue = 0D; return; }
            if (lightness >= 1D) { red = green = blue = 1D; return; }
            CssOklabToSrgb(lightness, a, b, out red, out green, out blue);
            if (CssSrgbInGamut(red, green, blue))
            {
                red = CssUnit(red); green = CssUnit(green); blue = CssUnit(blue);
                return;
            }

            // CSS gamut mapping keeps Oklab lightness and hue while reducing chroma.
            var chroma = Math.Sqrt(a * a + b * b);
            if (chroma <= 0D) { red = CssUnit(red); green = CssUnit(green); blue = CssUnit(blue); return; }
            var cosine = a / chroma;
            var sine = b / chroma;
            double clippedRed = CssUnit(red);
            double clippedGreen = CssUnit(green);
            double clippedBlue = CssUnit(blue);
            if (CssOklabDelta(lightness, a, b, clippedRed, clippedGreen, clippedBlue) < .02D)
            {
                red = clippedRed; green = clippedGreen; blue = clippedBlue;
                return;
            }
            var minimum = 0D;
            var maximum = chroma;
            var minimumInGamut = true;
            while (maximum - minimum > .0001D)
            {
                var current = (minimum + maximum) / 2D;
                var currentA = current * cosine;
                var currentB = current * sine;
                CssOklabToSrgb(lightness, currentA, currentB, out red, out green, out blue);
                if (minimumInGamut && CssSrgbInGamut(red, green, blue))
                {
                    minimum = current;
                    continue;
                }
                clippedRed = CssUnit(red);
                clippedGreen = CssUnit(green);
                clippedBlue = CssUnit(blue);
                var difference = CssOklabDelta(lightness, currentA, currentB, clippedRed, clippedGreen, clippedBlue);
                if (difference < .02D)
                {
                    if (.02D - difference < .0001D)
                    {
                        red = clippedRed; green = clippedGreen; blue = clippedBlue;
                        return;
                    }
                    minimumInGamut = false;
                    minimum = current;
                }
                else maximum = current;
            }
            red = clippedRed; green = clippedGreen; blue = clippedBlue;
        }

        private static void CssOklabToSrgb(double lightness, double a, double b,
            out double red, out double green, out double blue)
        {
            var lRoot = lightness + .3963377773761749D * a + .2158037573099136D * b;
            var mRoot = lightness - .1055613458156586D * a - .0638541728258133D * b;
            var sRoot = lightness - .0894841775298119D * a - 1.2914855480194092D * b;
            var l = lRoot * lRoot * lRoot;
            var m = mRoot * mRoot * mRoot;
            var s = sRoot * sRoot * sRoot;
            var x = 1.2268798758459243D * l - .5578149944602171D * m + .2813910456659647D * s;
            var y = -.0405757452148008D * l + 1.112286803280317D * m - .0717110580655164D * s;
            var z = -.0763729366746601D * l - .4214933324022432D * m + 1.5869240198367816D * s;
            red = CssSrgbGamma(12831D / 3959D * x - 329D / 214D * y - 1974D / 3959D * z);
            green = CssSrgbGamma(-851781D / 878810D * x + 1648619D / 878810D * y + 36519D / 878810D * z);
            blue = CssSrgbGamma(705D / 12673D * x - 2585D / 12673D * y + 705D / 667D * z);
        }

        private static void CssSrgbToOklab(double red, double green, double blue,
            out double lightness, out double a, out double b)
        {
            red = CssSrgbLinear(red);
            green = CssSrgbLinear(green);
            blue = CssSrgbLinear(blue);
            var x = 506752D / 1228815D * red + 87881D / 245763D * green + 12673D / 70218D * blue;
            var y = 87098D / 409605D * red + 175762D / 245763D * green + 12673D / 175545D * blue;
            var z = 7918D / 409605D * red + 87881D / 737289D * green + 1001167D / 1053270D * blue;
            CssXyzToOklab(x, y, z, out lightness, out a, out b);
        }

        private static void CssXyzToOklab(double x, double y, double z,
            out double lightness, out double a, out double b)
        {
            var l = CssCubeRoot(.819022437996703D * x + .3619062600528904D * y - .1288737815209879D * z);
            var m = CssCubeRoot(.0329836539323885D * x + .9292868615863434D * y + .0361446663506424D * z);
            var s = CssCubeRoot(.0481771893596242D * x + .2642395317527308D * y + .6335478284694309D * z);
            lightness = .210454268309314D * l + .7936177747023054D * m - .0040720430116193D * s;
            a = 1.9779985324311684D * l - 2.4285922420485799D * m + .450593709617411D * s;
            b = .0259040424655478D * l + .7827717124575296D * m - .8086757549230774D * s;
        }

        private static double CssOklabDelta(double lightness, double a, double b,
            double red, double green, double blue)
        {
            double clippedLightness;
            double clippedA;
            double clippedB;
            CssSrgbToOklab(red, green, blue, out clippedLightness, out clippedA, out clippedB);
            var lightnessDifference = lightness - clippedLightness;
            var aDifference = a - clippedA;
            var bDifference = b - clippedB;
            return Math.Sqrt(lightnessDifference * lightnessDifference + aDifference * aDifference + bDifference * bDifference);
        }

        private static double CssSrgbGamma(double value)
        {
            var sign = value < 0D ? -1D : 1D;
            var absolute = Math.Abs(value);
            return absolute > .0031308D ? sign * (1.055D * Math.Pow(absolute, 1D / 2.4D) - .055D) : 12.92D * value;
        }

        private static double CssSrgbLinear(double value)
        {
            var sign = value < 0D ? -1D : 1D;
            var absolute = Math.Abs(value);
            return absolute <= .04045D ? value / 12.92D : sign * Math.Pow((absolute + .055D) / 1.055D, 2.4D);
        }

        private static double CssSignedPower(double value, double exponent)
        {
            return value < 0D ? -Math.Pow(-value, exponent) : Math.Pow(value, exponent);
        }

        private static double CssProPhotoLinear(double value)
        {
            return Math.Abs(value) <= 16D / 512D ? value / 16D : CssSignedPower(value, 1.8D);
        }

        private static double CssCubeRoot(double value)
        {
            return value < 0D ? -Math.Pow(-value, 1D / 3D) : Math.Pow(value, 1D / 3D);
        }

        private static bool CssSrgbInGamut(double red, double green, double blue)
        {
            const double tolerance = .0000001D;
            return red >= -tolerance && red <= 1D + tolerance && green >= -tolerance && green <= 1D + tolerance &&
                blue >= -tolerance && blue <= 1D + tolerance;
        }

        private static double CssUnit(double value)
        {
            return Math.Max(0D, Math.Min(1D, value));
        }

        private static void CssHslRgb(double hue, double saturation, double lightness,
            out double red, out double green, out double blue)
        {
            var chroma = (1D - Math.Abs(2D * lightness - 1D)) * saturation;
            var sector = hue / 60D;
            var middle = chroma * (1D - Math.Abs(sector % 2D - 1D));
            if (sector < 1D) { red = chroma; green = middle; blue = 0D; }
            else if (sector < 2D) { red = middle; green = chroma; blue = 0D; }
            else if (sector < 3D) { red = 0D; green = chroma; blue = middle; }
            else if (sector < 4D) { red = 0D; green = middle; blue = chroma; }
            else if (sector < 5D) { red = middle; green = 0D; blue = chroma; }
            else { red = chroma; green = 0D; blue = middle; }
            var offset = lightness - chroma / 2D;
            red += offset;
            green += offset;
            blue += offset;
        }

        private static bool CssHue(string source, out double value)
        {
            value = 0D;
            var match = Regex.Match(source ?? "", "^(?<n>[+-]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[+-]?[0-9]+)?)(?<u>deg|grad|rad|turn)?$", RegexOptions.IgnoreCase);
            double number;
            if (!match.Success || !Double.TryParse(match.Groups["n"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out number) ||
                Double.IsNaN(number) || Double.IsInfinity(number)) return false;
            var unit = match.Groups["u"].Value.ToLowerInvariant();
            if (unit == "grad") number *= .9D;
            else if (unit == "rad") number *= 180D / Math.PI;
            else if (unit == "turn") number *= 360D;
            value = number % 360D;
            if (value < 0D) value += 360D;
            return true;
        }

        private static bool CssPercent(string source, out double value)
        {
            value = 0D;
            if (String.IsNullOrWhiteSpace(source) || !source.EndsWith("%", StringComparison.Ordinal)) return false;
            double number;
            if (!Double.TryParse(source.Substring(0, source.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out number) ||
                Double.IsNaN(number) || Double.IsInfinity(number)) return false;
            value = Math.Max(0D, Math.Min(1D, number / 100D));
            return true;
        }

        private static int CssByte(double value)
        {
            return (int)Math.Round(Math.Max(0D, Math.Min(1D, value)) * 255D);
        }

        private static bool CssColorChannel(string source, out int value)
        {
            value = 0;
            if (String.IsNullOrWhiteSpace(source)) return false;
            var percent = source.EndsWith("%", StringComparison.Ordinal);
            double number;
            if (!Double.TryParse(percent ? source.Substring(0, source.Length - 1) : source, NumberStyles.Float, CultureInfo.InvariantCulture, out number) || Double.IsNaN(number) || Double.IsInfinity(number)) return false;
            value = (int)Math.Round(Math.Max(0D, Math.Min(255D, percent ? number * 2.55D : number)));
            return true;
        }

        private static bool CssAlpha(string source, out int value)
        {
            value = 255;
            if (String.IsNullOrWhiteSpace(source)) return true;
            var percent = source.EndsWith("%", StringComparison.Ordinal);
            double number;
            if (!Double.TryParse(percent ? source.Substring(0, source.Length - 1) : source, NumberStyles.Float, CultureInfo.InvariantCulture, out number) || Double.IsNaN(number) || Double.IsInfinity(number)) return false;
            value = (int)Math.Round(255D * Math.Max(0D, Math.Min(1D, percent ? number / 100D : number)));
            return true;
        }

        private void OnChanged()
        {
            if (!closed && Changed != null) Changed(this, EventArgs.Empty);
        }

        private void NotifyAccessibility(AccessibleEvents kind)
        {
            if (closed) return;
            if (IsHandleCreated) AccessibilityNotifyClients(kind, -1);
            if (!IsHandleCreated || !AutomationInteropProvider.ClientsAreListening) return;
            try
            {
                if (kind == AccessibleEvents.Reorder)
                {
                    AutomationInteropProvider.RaiseStructureChangedEvent(UiRoot, new StructureChangedEventArgs(StructureChangeType.ChildrenInvalidated, UiRoot.GetRuntimeId()));
                    AutomationInteropProvider.RaiseAutomationEvent(TextPatternIdentifiers.TextChangedEvent, UiRoot,
                        new AutomationEventArgs(TextPatternIdentifiers.TextChangedEvent));
                }
                else if (kind == AccessibleEvents.Focus)
                {
                    var item = focus == null ? null : Ax(focus);
                    var provider = item == null ? (IRawElementProviderSimple)UiRoot : new Ux(this, item);
                    AutomationInteropProvider.RaiseAutomationEvent(AutomationElementIdentifiers.AutomationFocusChangedEvent, provider,
                        new AutomationEventArgs(AutomationElementIdentifiers.AutomationFocusChangedEvent));
                }
            }
            catch (InvalidOperationException) { }
            catch (ElementNotAvailableException) { }
        }

        private void NotifyAutomation(Ax item, AutomationProperty property, object oldValue, object newValue)
        {
            if (closed) return;
            if (!IsHandleCreated || !AutomationInteropProvider.ClientsAreListening) return;
            item = CurrentAccessibility(item);
            if (item == null) return;
            try
            {
                AutomationInteropProvider.RaiseAutomationPropertyChangedEvent(new Ux(this, item),
                    new AutomationPropertyChangedEventArgs(property, oldValue, newValue));
            }
            catch (InvalidOperationException) { }
            catch (ElementNotAvailableException) { }
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

        private Ug AccessibilityGrid(Ax item)
        {
            item = CurrentAccessibility(item);
            if (At == null || item == null || item.El == null) return null;
            var tableElement = item.El;
            while (tableElement != null && tableElement.K != "table") tableElement = tableElement.Up;
            if (tableElement == null) return null;
            var tableItem = Ax(tableElement);
            if (tableItem == null || tableItem.K != "table") return null;
            foreach (var block in At.Bs)
            {
                if (block.Tb == null || !Object.ReferenceEquals(block.El, tableElement)) continue;
                var columns = Math.Min(block.Tb.Cs, MaxAutomationTableColumns);
                var rows = 0;
                var cells = 0;
                foreach (var row in block.Tb.Rs)
                {
                    if (rows == MaxAutomationTableRows || rows > 0 && cells + row.Cs.Count > MaxAutomationTableCells) break;
                    var width = 0;
                    foreach (var cell in row.Cs)
                    {
                        if (width >= columns) break;
                        width += Math.Max(1, Math.Min(cell.Sp, columns - width));
                    }
                    if (width < columns) return null;
                    rows++;
                    cells += row.Cs.Count;
                    if (cells >= MaxAutomationTableCells) break;
                }
                return rows == 0 || columns == 0 ? null : new Ug(block.Tb, tableItem, rows, columns);
            }
            return null;
        }

        private Uc AccessibilityGridCell(Ax item)
        {
            var grid = AccessibilityGrid(item);
            item = CurrentAccessibility(item);
            if (grid == null || item == null || item.El == null) return null;
            var visited = 0;
            for (var row = 0; row < grid.Rs; row++)
            {
                var column = 0;
                foreach (var cell in grid.T.Rs[row].Cs)
                {
                    if (visited++ == MaxAutomationTableCells || column >= grid.Cs) break;
                    var span = Math.Max(1, Math.Min(cell.Sp, grid.Cs - column));
                    if (Object.ReferenceEquals(cell.El, item.El)) return new Uc(grid, cell, item, row, column, span);
                    column += span;
                }
            }
            return null;
        }

        private Ax AccessibilityGridItem(Ug grid, int row, int column)
        {
            if (grid == null || row < 0 || row >= grid.Rs || column < 0 || column >= grid.Cs) return null;
            var at = 0;
            var visited = 0;
            foreach (var cell in grid.T.Rs[row].Cs)
            {
                if (visited++ == MaxAutomationTableCells || at >= grid.Cs) break;
                var span = Math.Max(1, Math.Min(cell.Sp, grid.Cs - at));
                if (column >= at && column < at + span) return cell.El == null ? null : Ax(cell.El);
                at += span;
            }
            return null;
        }

        private IRawElementProviderSimple[] AccessibilityHeaders(Ug grid, string role, Uc owner)
        {
            var result = new List<IRawElementProviderSimple>();
            if (grid == null) return result.ToArray();
            var visited = 0;
            for (var row = 0; row < grid.Rs; row++)
            {
                var column = 0;
                foreach (var cell in grid.T.Rs[row].Cs)
                {
                    if (visited++ == MaxAutomationTableCells || column >= grid.Cs) break;
                    var span = Math.Max(1, Math.Min(cell.Sp, grid.Cs - column));
                    var item = cell.El == null ? null : Ax(cell.El);
                    var rowMatch = owner == null || owner.R == row;
                    var columnMatch = owner == null || column < owner.C + owner.Sp && owner.C < column + span;
                    if (item != null && item.K == role && (role == "rowheader" ? rowMatch : columnMatch)) result.Add(new Ux(this, item));
                    column += span;
                }
                if (visited >= MaxAutomationTableCells) break;
            }
            return result.ToArray();
        }

        private Ut AutomationText()
        {
            EnsureLayout();
            var page = At;
            var text = page == null ? "" : page.X ?? "";
            if (text.Length > MaxAutomationTextChars) text = text.Substring(0, MaxAutomationTextChars);
            var order = new Dictionary<N, int>();
            var position = 0;
            if (page != null) AutomationTextOrder(page.D, order, ref position);
            var parts = new List<Us>();
            var sequence = 0;
            using (var graphics = CreateGraphics())
            {
                foreach (var box in BoxSnapshot())
                {
                    if (parts.Count == MaxAutomationTextSegments) break;
                    var value = AutomationBoxText(box);
                    if (value.Length == 0) { sequence++; continue; }
                    int nodeOrder;
                    if (box.El == null || !order.TryGetValue(box.El, out nodeOrder)) nodeOrder = Int32.MaxValue;
                    AutomationTextParts(graphics, parts, box, value, nodeOrder, ref sequence);
                }
            }
            parts.Sort(delegate(Us left, Us right)
            {
                var compared = left.O.CompareTo(right.O);
                return compared != 0 ? compared : left.Q.CompareTo(right.Q);
            });
            var mapped = new List<Us>();
            var at = 0;
            foreach (var part in parts)
            {
                if (at >= text.Length) break;
                var found = text.IndexOf(part.X, at, StringComparison.OrdinalIgnoreCase);
                if (found < 0) continue;
                part.S = found;
                part.E = Math.Min(text.Length, found + part.X.Length);
                if (part.E > part.S) mapped.Add(part);
                at = part.E;
            }
            return new Ut(page, text, mapped, AutomationTextLineBoundaries(text, mapped));
        }

        private void AutomationTextParts(Graphics graphics, IList<Us> parts, Bx box, string value, int order, ref int sequence)
        {
            var field = box.B == null ? null : box.B.I;
            if (box.Inline || field != null || box.C.Width < 2)
            {
                var rect = box.Px == null ? box.R : box.C;
                if (box.Px != null)
                    using (var font = BoxFont(box.K, box.S, false, box.U != null, box.B == null ? null : box.B.K))
                        rect.Width = Math.Min(rect.Width, TextWidth(graphics, box.Px, font, box.S));
                parts.Add(new Us(value, rect, box.Clip, box.Fx, box.El, order, sequence++, InlineReverse(box.S)));
                return;
            }

            var area = box.C.IsEmpty ? box.R : box.C;
            using (var font = BoxFont(box.K, box.S, false, box.U != null, box.B == null ? null : box.B.K))
            {
                var words = Regex.Matches(value, "\\S+");
                var line = new StringBuilder();
                var lineWidth = 0;
                var spaceWidth = TextRenderer.MeasureText(graphics, " ", font, Size.Empty,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                var y = area.Top;
                foreach (Match match in words)
                {
                    var wordWidth = TextRenderer.MeasureText(graphics, match.Value, font, Size.Empty,
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
                    if (line.Length > 0 && lineWidth + spaceWidth + wordWidth > area.Width)
                    {
                        var lineLeft = InlineReverse(box.S) ? area.Right - Math.Max(1, Math.Min(area.Width, lineWidth)) : area.Left;
                        parts.Add(new Us(line.ToString(), new Rectangle(lineLeft, y, Math.Max(1, Math.Min(area.Width, lineWidth)), font.Height),
                            box.Clip, box.Fx, box.El, order, sequence++, InlineReverse(box.S)));
                        if (parts.Count == MaxAutomationTextSegments) return;
                        y += font.Height;
                        line.Clear();
                        lineWidth = 0;
                    }
                    if (line.Length > 0) { line.Append(' '); lineWidth += spaceWidth; }
                    line.Append(match.Value);
                    lineWidth += wordWidth;
                }
                if (line.Length > 0 && parts.Count < MaxAutomationTextSegments)
                {
                    var lineLeft = InlineReverse(box.S) ? area.Right - Math.Max(1, Math.Min(area.Width, lineWidth)) : area.Left;
                    parts.Add(new Us(line.ToString(), new Rectangle(lineLeft, y, Math.Max(1, Math.Min(area.Width, lineWidth)), font.Height),
                        box.Clip, box.Fx, box.El, order, sequence++, InlineReverse(box.S)));
                }
            }
        }

        private static IList<int> AutomationTextLineBoundaries(string text, IList<Us> parts)
        {
            var result = new List<int> { 0 };
            Us previous = null;
            foreach (var part in parts)
            {
                if (previous != null && !AutomationSameTextLine(previous, part) && result[result.Count - 1] != part.S) result.Add(part.S);
                previous = part;
            }
            if (result[result.Count - 1] != text.Length) result.Add(text.Length);
            return result;
        }

        private static bool AutomationSameTextLine(Us left, Us right)
        {
            if (left.Fx != right.Fx) return false;
            var overlap = Math.Min(left.R.Bottom, right.R.Bottom) - Math.Max(left.R.Top, right.R.Top);
            return overlap > 0;
        }

        private static void AutomationTextOrder(N node, IDictionary<N, int> order, ref int position)
        {
            if (node == null || position == 8192) return;
            order[node] = position++;
            foreach (var child in node.C)
            {
                if (position == 8192) break;
                AutomationTextOrder(child, order, ref position);
            }
        }

        private static string AutomationBoxText(Bx box)
        {
            if (box == null || box.Title || box.X.Length == 0 || Hidden(box.S) || HtmlHidden(box.El) || box.S != null && box.S.Op == 0) return "";
            if (box.K == "img" || box.K == "hr" || box.K == "table" || box.B != null && box.B.Vm != null) return "";
            if (box.B != null && box.B.I != null)
            {
                var field = box.B.I;
                if (field.K != "checkbox" && field.K != "radio" && box.K != "button") return "";
            }
            var value = box.Px ?? box.X;
            if (box.B != null && box.B.Mk.Length > 0)
            {
                var prefix = box.B.Mk + " ";
                if (value.StartsWith(prefix, StringComparison.Ordinal)) value = value.Substring(prefix.Length);
            }
            return Regex.Replace(value, "\\s+", " ").Trim();
        }

        private Rectangle AutomationTextLayoutRectangle(Us part)
        {
            if (part == null) return Rectangle.Empty;
            var rect = part.R;
            if (!part.Fx) rect.Offset(0, -sy);
            return rect;
        }

        private Rectangle AutomationTextRectangle(Us part)
        {
            return part == null ? Rectangle.Empty : AutomationTextRectangle(part, part.S, part.E);
        }

        private Rectangle AutomationTextRectangle(Us part, int start, int end)
        {
            if (part == null || end <= start || part.E <= part.S) return Rectangle.Empty;
            var rect = AutomationTextLayoutRectangle(part);
            var clip = part.Cp;
            var length = part.E - part.S;
            var from = Math.Max(part.S, start) - part.S;
            var to = Math.Min(part.E, end) - part.S;
            var left = part.Rev ? rect.Right - (int)Math.Ceiling(rect.Width * to / (double)length) :
                rect.Left + (int)Math.Floor(rect.Width * from / (double)length);
            var right = part.Rev ? rect.Right - (int)Math.Floor(rect.Width * from / (double)length) :
                rect.Left + (int)Math.Ceiling(rect.Width * to / (double)length);
            rect = new Rectangle(left, rect.Top, Math.Max(1, right - left), rect.Height);
            if (!part.Fx && !clip.IsEmpty) clip.Offset(0, -sy);
            rect = Rectangle.Intersect(rect, ClientRectangle);
            if (!clip.IsEmpty) rect = Rectangle.Intersect(rect, clip);
            return rect;
        }

        private ITextRangeProvider[] AutomationVisibleTextRanges(Ut text)
        {
            return new ITextRangeProvider[0];
        }

        private ITextRangeProvider AutomationTextRangeFromPoint(Ut text, System.Windows.Point screenPoint)
        {
            var point = PointToClient(new Point((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y)));
            Us best = null;
            var distance = Int64.MaxValue;
            foreach (var part in text.Ps)
            {
                var rect = AutomationTextRectangle(part);
                if (rect.IsEmpty) continue;
                var dx = point.X < rect.Left ? rect.Left - point.X : point.X > rect.Right ? point.X - rect.Right : 0;
                var dy = point.Y < rect.Top ? rect.Top - point.Y : point.Y > rect.Bottom ? point.Y - rect.Bottom : 0;
                var score = (long)dx * dx + (long)dy * dy;
                if (score >= distance) continue;
                best = part;
                distance = score;
                if (score == 0) break;
            }
            if (best == null) return new Uq(this, text, 0, 0);
            var bounds = AutomationTextLayoutRectangle(best);
            var ratio = bounds.Width < 1 ? 0D : Math.Max(0D, Math.Min(1D, (point.X - bounds.Left) / (double)bounds.Width));
            if (best.Rev) ratio = 1D - ratio;
            var offset = best.S + (int)Math.Round((best.E - best.S) * ratio);
            offset = Math.Max(best.S, Math.Min(best.E, offset));
            if (offset == best.E && offset > best.S) offset--;
            if (offset < best.E && Char.IsWhiteSpace(text.X[offset]))
            {
                var right = offset + 1;
                while (right < best.E && Char.IsWhiteSpace(text.X[right])) right++;
                var left = offset - 1;
                while (left >= best.S && Char.IsWhiteSpace(text.X[left])) left--;
                if (right < best.E) offset = right;
                else if (left >= best.S) offset = left;
            }
            return new Uq(this, text, offset, offset);
        }

        private bool AutomationTextChildBounds(Ut text, Ax item, out int start, out int end)
        {
            start = Int32.MaxValue;
            end = -1;
            item = CurrentAccessibility(item);
            if (item == null || item.El == null) return false;
            foreach (var part in text.Ps)
            {
                if (part.El == null || !OwnsElement(item.El, part.El) && !(item.El.K == "#text" && Object.ReferenceEquals(item.El.Up, part.El))) continue;
                start = Math.Min(start, part.S);
                end = Math.Max(end, part.E);
            }
            return end >= start;
        }

        private ITextRangeProvider AutomationTextRangeFromChild(Ut text, Ax item)
        {
            int start;
            int end;
            return AutomationTextChildBounds(text, item, out start, out end) ? new Uq(this, text, start, end) : new Uq(this, text, 0, 0);
        }

        private IRawElementProviderSimple[] AutomationTextChildren(Ut text, int start, int end)
        {
            var result = new List<IRawElementProviderSimple>();
            if (At == null) return result.ToArray();
            foreach (var item in At.Axs)
            {
                if (!AutomationTextEmbedded(item.K)) continue;
                int childStart;
                int childEnd;
                if (!AutomationTextChildBounds(text, item, out childStart, out childEnd) || childEnd <= start || childStart >= end) continue;
                result.Add(new Ux(this, item));
                if (result.Count == MaxAutomationTextChildren) break;
            }
            return result.ToArray();
        }

        private static bool AutomationTextEmbedded(string role)
        {
            return role == "link" || role == "button" || role == "img" || role == "table" || role == "textbox" || role == "searchbox" ||
                   role == "checkbox" || role == "radio" || role == "combobox" || role == "listbox" || role == "slider" ||
                   role == "spinbutton" || role == "progressbar" || role == "meter";
        }

        private IRawElementProviderSimple AutomationTextEnclosing(Ut text, int start, int end)
        {
            Ax common = null;
            foreach (var part in text.Ps)
            {
                if (part.E <= start || part.S >= end || part.El == null) continue;
                var owner = AutomationTextOwner(part.El);
                common = common == null ? owner : AutomationTextCommon(common, owner);
                if (common == null || Object.ReferenceEquals(common, AccessibilityRoot())) break;
            }
            var root = AccessibilityRoot();
            return common == null || Object.ReferenceEquals(common, root) ? (IRawElementProviderSimple)UiRoot : new Ux(this, common);
        }

        private Ax AutomationTextOwner(N element)
        {
            for (var current = element; current != null; current = current.Up)
            {
                var item = Ax(current);
                if (item != null) return item;
            }
            return AccessibilityRoot();
        }

        private static Ax AutomationTextCommon(Ax left, Ax right)
        {
            if (left == null || right == null) return null;
            var parents = new HashSet<Ax>();
            for (var item = left; item != null; item = item.Up) parents.Add(item);
            for (var item = right; item != null; item = item.Up) if (parents.Contains(item)) return item;
            return null;
        }

        private double[] AutomationTextRectangles(Ut text, int start, int end)
        {
            var rectangles = new List<Rectangle>();
            if (start == end) return new double[0];
            foreach (var part in text.Ps)
            {
                if (part.E <= start || part.S >= end) continue;
                var rect = AutomationTextRectangle(part, Math.Max(start, part.S), Math.Min(end, part.E));
                if (rect.IsEmpty) continue;
                if (rectangles.Count > 0)
                {
                    var previous = rectangles[rectangles.Count - 1];
                    if (previous.Top == rect.Top && previous.Height == rect.Height && rect.Left <= previous.Right + 2)
                    {
                        rectangles[rectangles.Count - 1] = Rectangle.Union(previous, rect);
                        continue;
                    }
                }
                rectangles.Add(rect);
                if (rectangles.Count == 256) break;
            }
            var result = new double[rectangles.Count * 4];
            for (var index = 0; index < rectangles.Count; index++)
            {
                var rect = RectangleToScreen(rectangles[index]);
                result[index * 4] = rect.X;
                result[index * 4 + 1] = rect.Y;
                result[index * 4 + 2] = rect.Width;
                result[index * 4 + 3] = rect.Height;
            }
            return result;
        }

        private void ScrollAutomationText(Ut text, int start, int end, bool alignTop)
        {
            foreach (var part in text.Ps)
            {
                if (part.E <= start || part.S >= end || part.Fx) continue;
                var target = alignTop ? part.R.Top - 24 : part.R.Bottom - ClientSize.Height + 24;
                SetScroll(target);
                PlaceEditor();
                Invalidate();
                return;
            }
        }

        private async void ActivateAccessibility(Ax item)
        {
            if (!FocusAccessibility(item)) return;
            await InputActivationA(Ka);
        }

        private T OnUi<T>(Func<T> work)
        {
            if (IsDisposed) return default(T);
            return InvokeRequired ? (T)Invoke(work) : work();
        }

        private void OnUi(Action work)
        {
            if (IsDisposed) return;
            if (InvokeRequired) Invoke(work);
            else work();
        }

        private Fi AccessibilityField(Ax item)
        {
            item = CurrentAccessibility(item);
            if (At == null || item == null || item.El == null) return null;
            foreach (var field in At.Fs)
                if (Object.ReferenceEquals(field.El, item.El)) return field;
            return null;
        }

        private bool CanAccessibilityValue(Ax item)
        {
            var field = AccessibilityField(item);
            return field != null && field.K != "password" && field.K != "file" && field.K != "checkbox" &&
                field.K != "radio" && field.K != "submit" && field.K != "reset" && field.K != "button" &&
                field.K != "range" && field.K != "number";
        }

        private void SetAccessibilityValue(Ax item, string value)
        {
            var field = AccessibilityField(item);
            if (field == null || !CanAccessibilityValue(item)) throw new InvalidOperationException("The Lea field does not expose an editable value.");
            if (field.Ds || field.Ro) throw new InvalidOperationException("The Lea field is read-only.");
            value = value ?? "";
            if (value.Length > 1024) throw new ArgumentException("Lea field values are limited to 1,024 characters.", "value");
            if (field.K == "select")
            {
                var oldValue = item.V;
                for (var i = 0; i < field.Os.Count; i++)
                    if (!field.Os[i].Ds && (field.Os[i].V == value || field.Os[i].T == value))
                    {
                        Choose(field, i);
                        NotifyAccessibility(AccessibleEvents.ValueChange);
                        NotifyAutomation(item, ValuePatternIdentifiers.ValueProperty, oldValue, item.V);
                        return;
                    }
                throw new ArgumentException("The value is not an enabled option.", "value");
            }
            if (field.K == "color" && !Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$"))
                throw new ArgumentException("The color must use #RRGGBB form.", "value");
            var previous = item.V;
            field.V = value;
            if (Object.ReferenceEquals(edit, field))
            {
                editLock = true;
                ed.Text = value;
                editLock = false;
            }
            var restyle = ClearValidity(field);
            OnEdited();
            NotifyAccessibility(AccessibleEvents.ValueChange);
            NotifyAutomation(item, ValuePatternIdentifiers.ValueProperty, previous, item.V);
            if (restyle) Restyle(); else Invalidate();
        }

        private bool CanAccessibilityToggle(Ax item)
        {
            var field = AccessibilityField(item);
            return field != null && field.K == "checkbox";
        }

        private void ToggleAccessibility(Ax item)
        {
            var field = AccessibilityField(item);
            if (field == null || field.K != "checkbox") throw new InvalidOperationException("The Lea item is not a native checkbox.");
            if (field.Ds) throw new InvalidOperationException("The Lea checkbox is disabled.");
            var previous = item.Is(Lea.Engine.Ax.Mx) ? ToggleState.Indeterminate : item.Is(Lea.Engine.Ax.Ck) ? ToggleState.On : ToggleState.Off;
            field.C = !field.C;
            var restyle = ClearValidity(field);
            OnEdited();
            NotifyAccessibility(AccessibleEvents.StateChange);
            NotifyAutomation(item, TogglePatternIdentifiers.ToggleStateProperty, previous, field.C ? ToggleState.On : ToggleState.Off);
            if (restyle) Restyle(); else Invalidate();
        }

        private bool CanAccessibilitySelect(Ax item)
        {
            var field = AccessibilityField(item);
            return field != null && field.K == "radio";
        }

        private void SelectAccessibility(Ax item)
        {
            var field = AccessibilityField(item);
            if (field == null || field.K != "radio") throw new InvalidOperationException("The Lea item is not a native radio button.");
            if (field.Ds) throw new InvalidOperationException("The Lea radio button is disabled.");
            var previous = item.Is(Lea.Engine.Ax.Ck) || item.Is(Lea.Engine.Ax.Sl);
            var restyle = false;
            foreach (var candidate in At.Fs)
                if (candidate.K == "radio" && Object.ReferenceEquals(candidate.F, field.F) && candidate.N == field.N)
                {
                    candidate.C = false;
                    restyle = ClearValidity(candidate) || restyle;
                }
            field.C = true;
            OnEdited();
            NotifyAccessibility(AccessibleEvents.Selection);
            NotifyAutomation(item, SelectionItemPatternIdentifiers.IsSelectedProperty, previous, true);
            if (restyle) Restyle(); else Invalidate();
        }

        private bool CanAccessibilityExpand(Ax item)
        {
            item = CurrentAccessibility(item);
            return item != null && item.El != null && item.El.K == "summary" && (item.Is(Lea.Engine.Ax.Ex) || item.Is(Lea.Engine.Ax.Co));
        }

        private void SetAccessibilityExpanded(Ax item, bool expanded)
        {
            item = CurrentAccessibility(item);
            if (!CanAccessibilityExpand(item)) throw new InvalidOperationException("The Lea item is not a disclosure summary.");
            if (item.Is(Lea.Engine.Ax.Ex) == expanded) return;
            var previous = expanded ? ExpandCollapseState.Collapsed : ExpandCollapseState.Expanded;
            Ds(item.El);
            NotifyAutomation(item, ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, previous,
                expanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }

        private void ScrollAccessibility(Ax item)
        {
            item = CurrentAccessibility(item);
            if (item == null || item.El == null) return;
            EnsureLayout();
            foreach (var box in BoxSnapshot())
            {
                if (box.Title || box.El == null || Hidden(box.S) || HtmlHidden(box.El)) continue;
                var owns = OwnsElement(item.El, box.El) || item.El.K == "#text" && Object.ReferenceEquals(item.El.Up, box.El);
                if (!owns && item.El.K == "area" && box.B != null && box.B.Mp != null)
                    foreach (var area in box.B.Mp.As)
                        if (Object.ReferenceEquals(area.El, item.El)) { owns = true; break; }
                if (!owns) continue;
                if (!box.Fx) SetScroll(box.R.Top - 24);
                PlaceEditor();
                Invalidate();
                return;
            }
        }

        private Ra AccessibilityRange(Ax item)
        {
            item = CurrentAccessibility(item);
            if (item == null) return null;
            var field = AccessibilityField(item);
            if (field != null && (field.K == "range" || field.K == "number"))
            {
                var minimum = AccessibilityNumber(field.Lo, field.K == "range" ? 0D : -1000000000000D);
                var maximum = AccessibilityNumber(field.Hi, field.K == "range" ? 100D : 1000000000000D);
                if (maximum < minimum) maximum = minimum;
                var value = AccessibilityNumber(field.V, minimum);
                value = Math.Max(minimum, Math.Min(maximum, value));
                var step = AccessibilityNumber(field.Sp, 1D);
                if (step <= 0D) step = 1D;
                return new Ra(value, minimum, maximum, step, Math.Min(maximum - minimum, step * 10D), field.Ds || field.Ro, field);
            }
            var element = item.El;
            if (element == null || (item.K != "slider" && item.K != "progressbar" && item.K != "meter")) return null;
            var nativeProgress = element.K == "progress";
            var nativeMeter = element.K == "meter";
            var minimumValue = nativeProgress ? 0D : AccessibilityNumber(element.Av(nativeMeter ? "min" : "aria-valuemin"), 0D);
            var maximumValue = AccessibilityNumber(element.Av(nativeProgress || nativeMeter ? "max" : "aria-valuemax"), nativeProgress || nativeMeter ? 1D : 100D);
            if (maximumValue < minimumValue) maximumValue = minimumValue;
            var authored = nativeProgress || nativeMeter ? element.Av("value") : element.Av("aria-valuenow");
            var currentValue = AccessibilityNumber(authored.Length == 0 ? item.V : authored, minimumValue);
            currentValue = Math.Max(minimumValue, Math.Min(maximumValue, currentValue));
            return new Ra(currentValue, minimumValue, maximumValue, 0D, 0D, true, null);
        }

        private void SetAccessibilityRange(Ax item, double value)
        {
            var range = AccessibilityRange(item);
            if (range == null || range.F == null || range.Ro) throw new InvalidOperationException("The Lea range is read-only.");
            if (Double.IsNaN(value) || Double.IsInfinity(value) || value < range.Mn || value > range.Mx)
                throw new ArgumentOutOfRangeException("value");
            if (range.Sm > 0D)
            {
                var steps = Math.Round((value - range.Mn) / range.Sm, MidpointRounding.AwayFromZero);
                value = Math.Max(range.Mn, Math.Min(range.Mx, range.Mn + steps * range.Sm));
            }
            range.F.V = value.ToString("R", CultureInfo.InvariantCulture);
            var restyle = ClearValidity(range.F);
            OnEdited();
            NotifyAccessibility(AccessibleEvents.ValueChange);
            NotifyAutomation(item, RangeValuePatternIdentifiers.ValueProperty, range.V, value);
            if (restyle) Restyle(); else Invalidate();
        }

        private static double AccessibilityNumber(string text, double fallback)
        {
            double value;
            return Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                !Double.IsNaN(value) && !Double.IsInfinity(value) ? value : fallback;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !closed)
            {
                closed = true;
                ClearFailure();
                Fe = null;
                heldEdit = null;
                edit = null;
                e.Ng = null;
                navigationGuard = null;
                CancelFragment();
                picStop = true;
                e.St();
                CancelPics();
                CloseChoices();
                if (editFont != null) editFont.Dispose();
                foreach (var key in new List<string>(pics.Keys)) DropPic(key);
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

        private sealed class Ra
        {
            public readonly double V;
            public readonly double Mn;
            public readonly double Mx;
            public readonly double Sm;
            public readonly double Lg;
            public readonly bool Ro;
            public readonly Fi F;

            public Ra(double value, double minimum, double maximum, double small, double large, bool readOnly, Fi field)
            {
                V = value;
                Mn = minimum;
                Mx = maximum;
                Sm = small;
                Lg = large;
                Ro = readOnly;
                F = field;
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

            internal static string AccessibilityAction(Ax item)
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

            internal static AccessibleRole AccessibilityRole(string role)
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

            internal static AccessibleStates AccessibilityState(Ax item, Rectangle bounds)
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

        private sealed class Ur : MarshalByRefObject, IRawElementProviderFragmentRoot, ITextProvider
        {
            private readonly V v;

            public Ur(V owner) { v = owner; }

            public ProviderOptions ProviderOptions { get { return ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading; } }
            public IRawElementProviderSimple HostRawElementProvider
            {
                get { return v.IsHandleCreated ? AutomationInteropProvider.HostProviderFromHandle(v.Handle) : null; }
            }
            public object GetPatternProvider(int patternId)
            {
                if (patternId != TextPatternIdentifiers.Pattern.Id) return null;
                return v.OnUi<object>(delegate { return v.At == null ? null : this; });
            }
            public object GetPropertyValue(int propertyId)
            {
                return v.OnUi<object>(delegate
                {
                    if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id) return ControlType.Document.Id;
                    if (propertyId == AutomationElementIdentifiers.NameProperty.Id) return v.T.Length == 0 ? "Lea document" : v.T;
                    if (propertyId == AutomationElementIdentifiers.HelpTextProperty.Id) return "Lea web document";
                    if (propertyId == AutomationElementIdentifiers.AutomationIdProperty.Id) return "LeaDocument";
                    if (propertyId == AutomationElementIdentifiers.ClassNameProperty.Id) return "Lea.Win.V";
                    if (propertyId == AutomationElementIdentifiers.FrameworkIdProperty.Id) return "Lea";
                    if (propertyId == AutomationElementIdentifiers.IsControlElementProperty.Id || propertyId == AutomationElementIdentifiers.IsContentElementProperty.Id) return true;
                    if (propertyId == AutomationElementIdentifiers.IsEnabledProperty.Id) return v.Enabled;
                    if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id) return true;
                    if (propertyId == AutomationElementIdentifiers.HasKeyboardFocusProperty.Id) return v.Focused || v.ContainsFocus;
                    if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id) return !v.Visible;
                    if (propertyId == AutomationElementIdentifiers.NativeWindowHandleProperty.Id) return v.Handle.ToInt32();
                    if (propertyId == AutomationElementIdentifiers.IsTextPatternAvailableProperty.Id) return v.At != null;
                    return AutomationElementIdentifiers.NotSupported;
                });
            }
            public System.Windows.Rect BoundingRectangle
            {
                get
                {
                    return v.OnUi(delegate
                    {
                        var bounds = v.RectangleToScreen(v.ClientRectangle);
                        return new System.Windows.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    });
                }
            }
            public IRawElementProviderFragmentRoot FragmentRoot { get { return this; } }
            public IRawElementProviderFragment Navigate(NavigateDirection direction)
            {
                return v.OnUi<IRawElementProviderFragment>(delegate
                {
                    if (direction != NavigateDirection.FirstChild && direction != NavigateDirection.LastChild) return null;
                    var root = v.AccessibilityRoot();
                    if (root == null) return null;
                    var children = v.AccessibilityChildren(root);
                    if (children.Count == 0) return null;
                    return new Ux(v, direction == NavigateDirection.FirstChild ? children[0] : children[children.Count - 1]);
                });
            }
            public int[] GetRuntimeId() { return new[] { AutomationInteropProvider.AppendRuntimeId, 0 }; }
            public IRawElementProviderSimple[] GetEmbeddedFragmentRoots() { return null; }
            public void SetFocus() { v.OnUi(delegate { v.Focus(); }); }
            public IRawElementProviderFragment ElementProviderFromPoint(double x, double y)
            {
                return v.OnUi<IRawElementProviderFragment>(delegate
                {
                    var item = v.AccessibilityAt(new Point((int)Math.Round(x), (int)Math.Round(y)));
                    return item == null ? (IRawElementProviderFragment)this : new Ux(v, item);
                });
            }
            public IRawElementProviderFragment GetFocus()
            {
                return v.OnUi<IRawElementProviderFragment>(delegate
                {
                    var item = v.focus == null ? null : v.Ax(v.focus);
                    return item == null ? null : new Ux(v, item);
                });
            }

            ITextRangeProvider ITextProvider.DocumentRange
            {
                get { return v.OnUi<ITextRangeProvider>(delegate { var text = v.AutomationText(); return new Uq(v, text, 0, text.X.Length); }); }
            }
            SupportedTextSelection ITextProvider.SupportedTextSelection { get { return SupportedTextSelection.None; } }
            ITextRangeProvider[] ITextProvider.GetSelection() { throw new InvalidOperationException("The Lea document does not support text selection."); }
            ITextRangeProvider[] ITextProvider.GetVisibleRanges() { return v.OnUi(delegate { var text = v.AutomationText(); return v.AutomationVisibleTextRanges(text); }); }
            ITextRangeProvider ITextProvider.RangeFromChild(IRawElementProviderSimple childElement)
            {
                return v.OnUi<ITextRangeProvider>(delegate
                {
                    var child = childElement as Ux;
                    if (child == null || !Object.ReferenceEquals(child.Owner, v)) throw new ArgumentException("The child is not owned by this Lea document.", "childElement");
                    var text = v.AutomationText();
                    return v.AutomationTextRangeFromChild(text, child.Item);
                });
            }
            ITextRangeProvider ITextProvider.RangeFromPoint(System.Windows.Point screenLocation)
            {
                return v.OnUi<ITextRangeProvider>(delegate
                {
                    var text = v.AutomationText();
                    return v.AutomationTextRangeFromPoint(text, screenLocation);
                });
            }
        }

        private sealed class Ut
        {
            public readonly P P;
            public readonly string X;
            public readonly IList<Us> Ps;
            public readonly IList<int> Ls;
            public Ut(P page, string text, IList<Us> parts, IList<int> lines) { P = page; X = text ?? ""; Ps = parts; Ls = lines; }
        }

        private sealed class Us
        {
            public readonly string X;
            public readonly Rectangle R;
            public readonly Rectangle Cp;
            public readonly bool Fx;
            public readonly N El;
            public readonly int O;
            public readonly int Q;
            public readonly bool Rev;
            public int S;
            public int E;
            public Us(string text, Rectangle rect, Rectangle clip, bool fixedPosition, N element, int order, int sequence, bool reverse)
            {
                X = text;
                R = rect;
                Cp = clip;
                Fx = fixedPosition;
                El = element;
                O = order;
                Q = sequence;
                Rev = reverse;
            }
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)]
        private sealed class Uq : MarshalByRefObject, ITextRangeProvider
        {
            private readonly V v;
            private readonly Ut t;
            private int s;
            private int e;

            public Uq(V owner, Ut text, int start, int end)
            {
                v = owner;
                t = text;
                s = Math.Max(0, Math.Min(t.X.Length, start));
                e = Math.Max(s, Math.Min(t.X.Length, end));
            }

            public ITextRangeProvider Clone()
            {
                return v.OnUi<ITextRangeProvider>(delegate { Check(); return new Uq(v, t, s, e); });
            }

            public bool Compare(ITextRangeProvider range)
            {
                return v.OnUi(delegate
                {
                    Check();
                    var other = Other(range, false);
                    return other != null && s == other.s && e == other.e;
                });
            }

            public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
            {
                return v.OnUi(delegate
                {
                    Check();
                    var other = Other(targetRange, true);
                    return Point(endpoint).CompareTo(other.Point(targetEndpoint));
                });
            }

            public void ExpandToEnclosingUnit(TextUnit unit)
            {
                v.OnUi(delegate
                {
                    Check();
                    int start;
                    int end;
                    Bounds(unit, s, out start, out end);
                    s = start;
                    e = end;
                });
            }

            public ITextRangeProvider FindAttribute(int attributeId, object value, bool backward)
            {
                return v.OnUi<ITextRangeProvider>(delegate
                {
                    Check();
                    ValidateAttribute(attributeId);
                    var current = Attribute(attributeId);
                    return Object.ReferenceEquals(current, AutomationElementIdentifiers.NotSupported) || !Object.Equals(current, value)
                        ? null : new Uq(v, t, s, e);
                });
            }

            public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
            {
                return v.OnUi<ITextRangeProvider>(delegate
                {
                    Check();
                    if (text == null) throw new ArgumentNullException("text");
                    if (text.Length == 0) return null;
                    if (text.Length > 4096) throw new ArgumentException("Lea text searches are limited to 4,096 characters.", "text");
                    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    var source = t.X.Substring(s, e - s);
                    var found = backward ? source.LastIndexOf(text, comparison) : source.IndexOf(text, comparison);
                    return found < 0 ? null : new Uq(v, t, s + found, s + found + text.Length);
                });
            }

            public object GetAttributeValue(int attributeId)
            {
                return v.OnUi<object>(delegate
                {
                    Check();
                    ValidateAttribute(attributeId);
                    return Attribute(attributeId);
                });
            }

            public double[] GetBoundingRectangles()
            {
                return v.OnUi(delegate { Check(); return v.AutomationTextRectangles(t, s, e); });
            }

            public IRawElementProviderSimple GetEnclosingElement()
            {
                return v.OnUi<IRawElementProviderSimple>(delegate { Check(); return v.AutomationTextEnclosing(t, s, e); });
            }

            public string GetText(int maxLength)
            {
                return v.OnUi(delegate
                {
                    Check();
                    if (maxLength < -1) throw new ArgumentOutOfRangeException("maxLength");
                    var length = e - s;
                    if (maxLength >= 0) length = Math.Min(length, maxLength);
                    return t.X.Substring(s, length);
                });
            }

            public int Move(TextUnit unit, int count)
            {
                return v.OnUi(delegate
                {
                    Check();
                    if (count == 0) return 0;
                    int currentStart;
                    int currentEnd;
                    Bounds(unit, s, out currentStart, out currentEnd);
                    int moved;
                    var target = MovePoint(currentStart, unit, count, out moved);
                    if (moved == 0) return 0;
                    if (target == t.X.Length && count > 0) s = e = target;
                    else Bounds(unit, target, out s, out e);
                    return moved;
                });
            }

            public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
            {
                return v.OnUi(delegate
                {
                    Check();
                    int moved;
                    var point = MovePoint(Point(endpoint), unit, count, out moved);
                    SetPoint(endpoint, point);
                    return moved;
                });
            }

            public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
            {
                v.OnUi(delegate
                {
                    Check();
                    var other = Other(targetRange, true);
                    SetPoint(endpoint, other.Point(targetEndpoint));
                });
            }

            public void Select() { throw new InvalidOperationException("The Lea document does not support text selection."); }
            public void AddToSelection() { throw new InvalidOperationException("The Lea document does not support text selection."); }
            public void RemoveFromSelection() { throw new InvalidOperationException("The Lea document does not support text selection."); }
            public void ScrollIntoView(bool alignToTop) { v.OnUi(delegate { Check(); v.ScrollAutomationText(t, s, e, alignToTop); }); }
            public IRawElementProviderSimple[] GetChildren() { return v.OnUi(delegate { Check(); return v.AutomationTextChildren(t, s, e); }); }

            private void Check()
            {
                if (!Object.ReferenceEquals(v.At, t.P)) throw new ElementNotAvailableException("The Lea text range belongs to an earlier page generation.");
            }

            private Uq Other(ITextRangeProvider range, bool required)
            {
                var other = range as Uq;
                if (other != null && Object.ReferenceEquals(other.v, v) && Object.ReferenceEquals(other.t.P, t.P) && other.t.X == t.X) return other;
                if (required) throw new ArgumentException("The text range is not owned by this Lea document.", "range");
                return null;
            }

            private int Point(TextPatternRangeEndpoint endpoint) { return endpoint == TextPatternRangeEndpoint.Start ? s : e; }

            private void SetPoint(TextPatternRangeEndpoint endpoint, int value)
            {
                value = Math.Max(0, Math.Min(t.X.Length, value));
                if (endpoint == TextPatternRangeEndpoint.Start)
                {
                    s = value;
                    if (s > e) e = s;
                }
                else
                {
                    e = value;
                    if (e < s) s = e;
                }
            }

            private object Attribute(int attributeId)
            {
                if (attributeId == TextPatternIdentifiers.IsReadOnlyAttribute.Id) return true;
                if (attributeId == TextPatternIdentifiers.CultureAttribute.Id) return CultureInfo.CurrentCulture.LCID;
                return AutomationElementIdentifiers.NotSupported;
            }

            private static void ValidateAttribute(int attributeId)
            {
                if (attributeId < 40000 || attributeId > 40030) throw new ArgumentException("The text attribute is not valid.", "attributeId");
            }

            private void Bounds(TextUnit unit, int position, out int start, out int end)
            {
                unit = Effective(unit);
                position = Math.Max(0, Math.Min(t.X.Length, position));
                if (unit == TextUnit.Document)
                {
                    start = 0;
                    end = t.X.Length;
                    return;
                }
                if (position == t.X.Length)
                {
                    start = end = position;
                    return;
                }
                if (unit == TextUnit.Character)
                {
                    start = position;
                    end = Math.Min(t.X.Length, position + 1);
                    return;
                }
                if (unit == TextUnit.Line)
                {
                    start = 0;
                    end = t.X.Length;
                    for (var index = 0; index < t.Ls.Count - 1; index++)
                    {
                        if (position < t.Ls[index] || position >= t.Ls[index + 1]) continue;
                        start = t.Ls[index];
                        end = t.Ls[index + 1];
                        break;
                    }
                    return;
                }
                var kind = WordKind(t.X[position]);
                start = position;
                while (start > 0 && WordKind(t.X[start - 1]) == kind) start--;
                end = position + 1;
                while (end < t.X.Length && WordKind(t.X[end]) == kind) end++;
            }

            private int MovePoint(int point, TextUnit unit, int count, out int moved)
            {
                var boundaries = Boundaries(unit);
                point = Math.Max(0, Math.Min(t.X.Length, point));
                moved = 0;
                while (moved != count)
                {
                    var next = point;
                    if (count > 0)
                    {
                        foreach (var boundary in boundaries) if (boundary > point) { next = boundary; break; }
                        if (next == point) break;
                        moved++;
                    }
                    else
                    {
                        for (var index = boundaries.Count - 1; index >= 0; index--)
                            if (boundaries[index] < point) { next = boundaries[index]; break; }
                        if (next == point) break;
                        moved--;
                    }
                    point = next;
                }
                return point;
            }

            private List<int> Boundaries(TextUnit unit)
            {
                unit = Effective(unit);
                var result = new List<int> { 0 };
                if (unit == TextUnit.Document)
                {
                    if (t.X.Length > 0) result.Add(t.X.Length);
                    return result;
                }
                if (unit == TextUnit.Line) return new List<int>(t.Ls);
                for (var index = 1; index < t.X.Length; index++)
                {
                    if (unit == TextUnit.Character || unit == TextUnit.Word && WordKind(t.X[index - 1]) != WordKind(t.X[index])) result.Add(index);
                }
                if (result[result.Count - 1] != t.X.Length) result.Add(t.X.Length);
                return result;
            }

            private static TextUnit Effective(TextUnit unit)
            {
                if (unit < TextUnit.Character || unit > TextUnit.Document) throw new ArgumentException("The text unit is not valid.", "unit");
                if (unit == TextUnit.Format) return TextUnit.Word;
                if (unit == TextUnit.Paragraph) return TextUnit.Line;
                if (unit == TextUnit.Page) return TextUnit.Document;
                return unit;
            }

            private static int WordKind(char value)
            {
                return Char.IsWhiteSpace(value) ? 0 : Char.IsLetterOrDigit(value) || value == '_' ? 1 : 2;
            }
        }

        private sealed class Ux : IRawElementProviderFragment, IInvokeProvider, IValueProvider, IToggleProvider,
            IExpandCollapseProvider, ISelectionItemProvider, IRangeValueProvider, IScrollItemProvider,
            IGridProvider, IGridItemProvider, ITableProvider, ITableItemProvider
        {
            private readonly V v;
            private readonly Ax a;

            public Ux(V owner, Ax item)
            {
                v = owner;
                a = item;
            }

            private Ax A { get { return v.CurrentAccessibility(a) ?? a; } }
            internal V Owner { get { return v; } }
            internal Ax Item { get { return A; } }
            public ProviderOptions ProviderOptions { get { return ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading; } }
            public IRawElementProviderSimple HostRawElementProvider { get { return null; } }
            public object GetPatternProvider(int patternId)
            {
                return v.OnUi<object>(delegate
                {
                    var item = A;
                    if (patternId == InvokePatternIdentifiers.Pattern.Id) return CanInvoke(item) ? this : null;
                    if (patternId == ValuePatternIdentifiers.Pattern.Id) return v.CanAccessibilityValue(item) ? this : null;
                    if (patternId == TogglePatternIdentifiers.Pattern.Id) return v.CanAccessibilityToggle(item) ? this : null;
                    if (patternId == ExpandCollapsePatternIdentifiers.Pattern.Id) return v.CanAccessibilityExpand(item) ? this : null;
                    if (patternId == SelectionItemPatternIdentifiers.Pattern.Id) return v.CanAccessibilitySelect(item) ? this : null;
                    if (patternId == RangeValuePatternIdentifiers.Pattern.Id) return v.AccessibilityRange(item) != null ? this : null;
                    if (patternId == ScrollItemPatternIdentifiers.Pattern.Id) return item.El != null ? this : null;
                    var grid = v.AccessibilityGrid(item);
                    if (patternId == GridPatternIdentifiers.Pattern.Id || patternId == TablePatternIdentifiers.Pattern.Id)
                        return grid != null && Object.ReferenceEquals(grid.A, item) ? this : null;
                    var cell = v.AccessibilityGridCell(item);
                    if (patternId == GridItemPatternIdentifiers.Pattern.Id || patternId == TableItemPatternIdentifiers.Pattern.Id)
                        return cell == null ? null : this;
                    return null;
                });
            }
            public object GetPropertyValue(int propertyId)
            {
                return v.OnUi<object>(delegate
                {
                    var item = A;
                    if (propertyId == AutomationElementIdentifiers.ControlTypeProperty.Id) return AccessibilityControlType(item.K).Id;
                    if (propertyId == AutomationElementIdentifiers.NameProperty.Id) return item.N;
                    if (propertyId == AutomationElementIdentifiers.HelpTextProperty.Id) return item.D;
                    if (propertyId == AutomationElementIdentifiers.AutomationIdProperty.Id) return item.El == null ? "" : item.El.Av("id");
                    if (propertyId == AutomationElementIdentifiers.ClassNameProperty.Id) return item.El == null ? "Lea." + item.K : "Lea." + item.El.K;
                    if (propertyId == AutomationElementIdentifiers.FrameworkIdProperty.Id) return "Lea";
                    if (propertyId == AutomationElementIdentifiers.IsControlElementProperty.Id || propertyId == AutomationElementIdentifiers.IsContentElementProperty.Id) return true;
                    if (propertyId == AutomationElementIdentifiers.IsEnabledProperty.Id) return !item.Is(Lea.Engine.Ax.Ds);
                    if (propertyId == AutomationElementIdentifiers.IsKeyboardFocusableProperty.Id) return item.Is(Lea.Engine.Ax.Fs);
                    if (propertyId == AutomationElementIdentifiers.HasKeyboardFocusProperty.Id) return item.Is(Lea.Engine.Ax.Fc);
                    if (propertyId == AutomationElementIdentifiers.IsOffscreenProperty.Id) return v.Ab(item).IsEmpty;
                    if (propertyId == AutomationElementIdentifiers.AccessKeyProperty.Id) return item.El == null ? "" : v.Kl(item.El);
                    if (propertyId == AutomationElementIdentifiers.IsPasswordProperty.Id) return IsPassword(item);
                    if (propertyId == AutomationElementIdentifiers.IsRequiredForFormProperty.Id) return item.Is(Lea.Engine.Ax.Rq);
                    if (propertyId == AutomationElementIdentifiers.IsInvokePatternAvailableProperty.Id) return CanInvoke(item);
                    if (propertyId == AutomationElementIdentifiers.IsValuePatternAvailableProperty.Id) return v.CanAccessibilityValue(item);
                    if (propertyId == AutomationElementIdentifiers.IsTogglePatternAvailableProperty.Id) return v.CanAccessibilityToggle(item);
                    if (propertyId == AutomationElementIdentifiers.IsExpandCollapsePatternAvailableProperty.Id) return v.CanAccessibilityExpand(item);
                    if (propertyId == AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty.Id) return v.CanAccessibilitySelect(item);
                    if (propertyId == AutomationElementIdentifiers.IsRangeValuePatternAvailableProperty.Id) return v.AccessibilityRange(item) != null;
                    if (propertyId == AutomationElementIdentifiers.IsScrollItemPatternAvailableProperty.Id) return item.El != null;
                    var grid = v.AccessibilityGrid(item);
                    if (propertyId == AutomationElementIdentifiers.IsGridPatternAvailableProperty.Id || propertyId == AutomationElementIdentifiers.IsTablePatternAvailableProperty.Id)
                        return grid != null && Object.ReferenceEquals(grid.A, item);
                    var cell = v.AccessibilityGridCell(item);
                    if (propertyId == AutomationElementIdentifiers.IsGridItemPatternAvailableProperty.Id || propertyId == AutomationElementIdentifiers.IsTableItemPatternAvailableProperty.Id)
                        return cell != null;
                    return AutomationElementIdentifiers.NotSupported;
                });
            }
            public System.Windows.Rect BoundingRectangle
            {
                get
                {
                    return v.OnUi(delegate
                    {
                        var bounds = v.Ab(A);
                        if (bounds.IsEmpty) return System.Windows.Rect.Empty;
                        bounds = v.RectangleToScreen(bounds);
                        return new System.Windows.Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    });
                }
            }
            public IRawElementProviderFragmentRoot FragmentRoot { get { return v.UiRoot; } }
            public IRawElementProviderFragment Navigate(NavigateDirection direction)
            {
                return v.OnUi<IRawElementProviderFragment>(delegate
                {
                    var item = A;
                    if (direction == NavigateDirection.FirstChild || direction == NavigateDirection.LastChild)
                    {
                        var children = v.AccessibilityChildren(item);
                        if (children.Count == 0) return null;
                        return new Ux(v, direction == NavigateDirection.FirstChild ? children[0] : children[children.Count - 1]);
                    }
                    if (direction == NavigateDirection.Parent)
                    {
                        var root = v.AccessibilityRoot();
                        return item.Up == null || Object.ReferenceEquals(item.Up, root) ? (IRawElementProviderFragment)v.UiRoot : new Ux(v, item.Up);
                    }
                    if (item.Up == null) return null;
                    var siblings = v.AccessibilityChildren(item.Up);
                    var index = siblings.IndexOf(item);
                    if (direction == NavigateDirection.NextSibling) index++;
                    else if (direction == NavigateDirection.PreviousSibling) index--;
                    else return null;
                    return index < 0 || index >= siblings.Count ? null : new Ux(v, siblings[index]);
                });
            }
            public int[] GetRuntimeId()
            {
                return v.OnUi(delegate
                {
                    var index = v.At == null ? -1 : v.At.Axs.IndexOf(A);
                    return new[] { AutomationInteropProvider.AppendRuntimeId, Math.Max(1, index + 1) };
                });
            }
            public IRawElementProviderSimple[] GetEmbeddedFragmentRoots() { return null; }
            public void SetFocus() { v.OnUi(delegate { v.FocusAccessibility(A); }); }

            void IInvokeProvider.Invoke() { v.OnUi(delegate { v.ActivateAccessibility(A); }); }
            bool IValueProvider.IsReadOnly
            {
                get
                {
                    return v.OnUi(delegate
                    {
                        var field = v.AccessibilityField(A);
                        return field == null || field.Ds || field.Ro;
                    });
                }
            }
            string IValueProvider.Value { get { return v.OnUi(delegate { return A.V; }); } }
            void IValueProvider.SetValue(string value) { v.OnUi(delegate { v.SetAccessibilityValue(A, value); }); }
            ToggleState IToggleProvider.ToggleState
            {
                get
                {
                    return v.OnUi(delegate
                    {
                        var item = A;
                        return item.Is(Lea.Engine.Ax.Mx) ? ToggleState.Indeterminate : item.Is(Lea.Engine.Ax.Ck) ? ToggleState.On : ToggleState.Off;
                    });
                }
            }
            void IToggleProvider.Toggle() { v.OnUi(delegate { v.ToggleAccessibility(A); }); }
            ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
            {
                get { return v.OnUi(delegate { return A.Is(Lea.Engine.Ax.Ex) ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed; }); }
            }
            void IExpandCollapseProvider.Expand() { v.OnUi(delegate { v.SetAccessibilityExpanded(A, true); }); }
            void IExpandCollapseProvider.Collapse() { v.OnUi(delegate { v.SetAccessibilityExpanded(A, false); }); }
            bool ISelectionItemProvider.IsSelected { get { return v.OnUi(delegate { return A.Is(Lea.Engine.Ax.Ck) || A.Is(Lea.Engine.Ax.Sl); }); } }
            IRawElementProviderSimple ISelectionItemProvider.SelectionContainer { get { return null; } }
            void ISelectionItemProvider.AddToSelection() { v.OnUi(delegate { v.SelectAccessibility(A); }); }
            void ISelectionItemProvider.RemoveFromSelection() { throw new InvalidOperationException("Native radio buttons cannot be cleared without selecting a peer."); }
            void ISelectionItemProvider.Select() { v.OnUi(delegate { v.SelectAccessibility(A); }); }
            bool IRangeValueProvider.IsReadOnly { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null || range.Ro; }); } }
            double IRangeValueProvider.Value { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null ? 0D : range.V; }); } }
            double IRangeValueProvider.Minimum { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null ? 0D : range.Mn; }); } }
            double IRangeValueProvider.Maximum { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null ? 0D : range.Mx; }); } }
            double IRangeValueProvider.SmallChange { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null ? 0D : range.Sm; }); } }
            double IRangeValueProvider.LargeChange { get { return v.OnUi(delegate { var range = v.AccessibilityRange(A); return range == null ? 0D : range.Lg; }); } }
            void IRangeValueProvider.SetValue(double value) { v.OnUi(delegate { v.SetAccessibilityRange(A, value); }); }
            void IScrollItemProvider.ScrollIntoView() { v.OnUi(delegate { v.ScrollAccessibility(A); }); }
            int IGridProvider.RowCount { get { return v.OnUi(delegate { var grid = v.AccessibilityGrid(A); return grid == null ? 0 : grid.Rs; }); } }
            int IGridProvider.ColumnCount { get { return v.OnUi(delegate { var grid = v.AccessibilityGrid(A); return grid == null ? 0 : grid.Cs; }); } }
            IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
            {
                return v.OnUi<IRawElementProviderSimple>(delegate
                {
                    var grid = v.AccessibilityGrid(A);
                    if (grid == null) return null;
                    if (row < 0 || row >= grid.Rs) throw new ArgumentOutOfRangeException("row");
                    if (column < 0 || column >= grid.Cs) throw new ArgumentOutOfRangeException("column");
                    var item = v.AccessibilityGridItem(grid, row, column);
                    return item == null ? null : new Ux(v, item);
                });
            }
            RowOrColumnMajor ITableProvider.RowOrColumnMajor { get { return RowOrColumnMajor.RowMajor; } }
            IRawElementProviderSimple[] ITableProvider.GetRowHeaders() { return v.OnUi(delegate { return v.AccessibilityHeaders(v.AccessibilityGrid(A), "rowheader", null); }); }
            IRawElementProviderSimple[] ITableProvider.GetColumnHeaders() { return v.OnUi(delegate { return v.AccessibilityHeaders(v.AccessibilityGrid(A), "columnheader", null); }); }
            int IGridItemProvider.Row { get { return v.OnUi(delegate { var cell = v.AccessibilityGridCell(A); return cell == null ? 0 : cell.R; }); } }
            int IGridItemProvider.Column { get { return v.OnUi(delegate { var cell = v.AccessibilityGridCell(A); return cell == null ? 0 : cell.C; }); } }
            int IGridItemProvider.RowSpan { get { return 1; } }
            int IGridItemProvider.ColumnSpan { get { return v.OnUi(delegate { var cell = v.AccessibilityGridCell(A); return cell == null ? 1 : cell.Sp; }); } }
            IRawElementProviderSimple IGridItemProvider.ContainingGrid
            {
                get
                {
                    return v.OnUi<IRawElementProviderSimple>(delegate
                    {
                        var cell = v.AccessibilityGridCell(A);
                        return cell == null ? null : new Ux(v, cell.G.A);
                    });
                }
            }
            IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
            {
                return v.OnUi(delegate { var cell = v.AccessibilityGridCell(A); return v.AccessibilityHeaders(cell == null ? null : cell.G, "rowheader", cell); });
            }
            IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
            {
                return v.OnUi(delegate { var cell = v.AccessibilityGridCell(A); return v.AccessibilityHeaders(cell == null ? null : cell.G, "columnheader", cell); });
            }

            private static bool CanInvoke(Ax item)
            {
                if (item == null || item.El == null || item.El.K == "summary") return false;
                return (item.K == "link" || item.K == "button") && Xa.AccessibilityAction(item).Length > 0;
            }

            private static bool IsPassword(Ax item)
            {
                return item != null && item.El != null && item.El.K == "input" && item.El.Av("type").Equals("password", StringComparison.OrdinalIgnoreCase);
            }

            private static ControlType AccessibilityControlType(string role)
            {
                if (role == "document") return ControlType.Document;
                if (role == "link") return ControlType.Hyperlink;
                if (role == "button") return ControlType.Button;
                if (role == "checkbox") return ControlType.CheckBox;
                if (role == "radio") return ControlType.RadioButton;
                if (role == "textbox" || role == "searchbox") return ControlType.Edit;
                if (role == "spinbutton") return ControlType.Spinner;
                if (role == "combobox") return ControlType.ComboBox;
                if (role == "listbox" || role == "list") return ControlType.List;
                if (role == "listitem") return ControlType.ListItem;
                if (role == "slider") return ControlType.Slider;
                if (role == "progressbar" || role == "meter") return ControlType.ProgressBar;
                if (role == "img") return ControlType.Image;
                if (role == "table") return ControlType.Table;
                if (role == "row" || role == "cell") return ControlType.DataItem;
                if (role == "columnheader" || role == "rowheader") return ControlType.HeaderItem;
                if (role == "separator") return ControlType.Separator;
                if (role == "status") return ControlType.StatusBar;
                if (role == "text" || role == "paragraph" || role == "blockquote" || role == "code" || role == "caption" ||
                    role == "term" || role == "definition" || role == "heading" || role == "time" || role == "timer") return ControlType.Text;
                return ControlType.Group;
            }
        }

        private sealed class Ug
        {
            public readonly Tb T;
            public readonly Ax A;
            public readonly int Rs;
            public readonly int Cs;
            public Ug(Tb table, Ax item, int rows, int columns) { T = table; A = item; Rs = rows; Cs = columns; }
        }

        private sealed class Uc
        {
            public readonly Ug G;
            public readonly Tc T;
            public readonly Ax A;
            public readonly int R;
            public readonly int C;
            public readonly int Sp;
            public Uc(Ug grid, Tc cell, Ax item, int row, int column, int span) { G = grid; T = cell; A = item; R = row; C = column; Sp = span; }
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
            public Dictionary<B, Fp> Fs;
            public int Ft;
            public int Fi;
            public int Fz;
            public int Fr;
            public bool Fh;
            public bool Fm;
            public bool Gm;
            public bool Gk;
            public int Gh;
            public bool Dh;
            public int Hr;

            public Lc(Rectangle outer, Rectangle content, int y, Rectangle clip, bool fixedPosition)
            {
                R = outer;
                C = content;
                Y = y;
                Cp = clip;
                Fx = fixedPosition;
            }
        }

        private sealed class Fp
        {
            public readonly int X;
            public readonly int W;
            public readonly int L;
            public readonly int Y;
            public readonly int H;
            public readonly bool C;
            public readonly int G;
            public readonly bool Q;

            public Fp(int x, int width, int line) : this(x, width, line, 0, -1, false, 0, false) { }
            public Fp(int x, int width, int line, int y, int height, bool column) : this(x, width, line, y, height, column, 0, false) { }
            public Fp(int x, int width, int line, int y, int height, bool column, int gap) : this(x, width, line, y, height, column, gap, false) { }
            public Fp(int x, int width, int line, int y, int height, bool column, int gap, bool grid) { X = x; W = width; L = line; Y = y; H = height; C = column; G = gap; Q = grid; }
        }

        private sealed class Gq
        {
            public readonly int R;
            public readonly int C;
            public readonly int Rs;
            public readonly int Cs;
            public Gq(int row, int column, int rowSpan, int columnSpan) { R = row; C = column; Rs = rowSpan; Cs = columnSpan; }
        }

        private sealed class Bx
        {
            public readonly string K;
            public readonly string X;
            public string Px;
            public readonly Uri U;
            public readonly Uri P;
            public readonly S S;
            public Rectangle R;
            public Rectangle C;
            public Rectangle R0;
            public Rectangle C0;
            public Rectangle Cp0;
            public readonly bool Title;
            public readonly B B;
            public readonly bool Inline;
            public Rectangle Clip;
            public readonly N El;
            public int[] Zp;
            public bool Fx;
            public bool Fm;
            public bool Fa;

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle), N element = null, Uri picture = null)
                : this(kind, text, url, style, rect, rect, title, block, inline, clip, element, picture)
            {
            }

            public Bx(string kind, string text, Uri url, S style, Rectangle rect, Rectangle content, bool title, B block = null, bool inline = false, Rectangle clip = default(Rectangle), N element = null, Uri picture = null)
            {
                K = kind;
                X = text ?? "";
                U = url;
                P = picture;
                S = style;
                R = rect;
                C = content;
                Title = title;
                B = block;
                Inline = inline;
                Clip = clip;
                El = element ?? (block == null ? null : block.El);
            }

            public void SetBase()
            {
                R0 = R;
                C0 = C;
                Cp0 = Clip;
            }

            public void Reset()
            {
                R = R0;
                C = C0;
                Clip = Cp0;
            }

            public void Move(int y)
            {
                R.Offset(0, y);
                C.Offset(0, y);
            }
        }

        private sealed class Ia
        {
            public readonly string K;
            public readonly string X;
            public readonly Uri U;
            public readonly Uri P;
            public readonly S S;
            public readonly string Z;
            public readonly int W;
            public readonly int H;
            public readonly int L;
            public readonly N El;

            public Ia(string kind, string text, Uri url, S style, string baseKind, int width, int height, int lineHeight, N element, Uri picture = null)
            {
                K = kind;
                X = text;
                U = url;
                P = picture;
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
            public string Px;
            public readonly Uri U;
            public readonly Uri P;
            public readonly S S;
            public Rectangle R;
            public Rectangle C;
            public readonly N El;

            public Ix(string kind, string text, Uri url, S style, Rectangle rect, N element, Uri picture = null)
            {
                K = kind;
                X = text;
                U = url;
                P = picture;
                S = style;
                R = rect;
                C = rect;
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
