using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using Forms = System.Windows.Forms;

internal static partial class MinimalTextProviderMain
{
    private const string Payload = "Alpha bravo minimal";
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(IntPtr reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();

    private sealed class Surface : Forms.Control
    {
        internal readonly Root Provider;
        internal IntPtr NativeHandle;
        internal Surface() { Provider = new Root(this); BackColor = Color.White; }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); NativeHandle = Handle; }
        protected override void OnPaint(Forms.PaintEventArgs e) { e.Graphics.DrawString(Payload, Font, Brushes.Black, 8, 8); }
        protected override void WndProc(ref Forms.Message message)
        {
            if (message.Msg == 0x003D && message.LParam.ToInt64() == AutomationInteropProvider.RootObjectId)
            { message.Result = AutomationInteropProvider.ReturnRawElementProvider(Handle, message.WParam, message.LParam, Provider); return; }
            base.WndProc(ref message);
        }
        internal double[] BoundsOnUi()
        {
            if (InvokeRequired) return (double[])Invoke(new Func<double[]>(BoundsOnUi));
            var rectangle = RectangleToScreen(ClientRectangle);
            return new double[] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height };
        }
    }

    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    private sealed class Root : MarshalByRefObject, IRawElementProviderSimple,
#if TYPED_TEXT_ARRAYS
        TypedTextProviderContract
#else
        ITextProvider
#endif
    {
        private readonly Surface owner;
        private readonly Range range;
        internal Root(Surface value) { owner = value; range = new Range(this); }
        public ProviderOptions ProviderOptions { get { return ProviderOptions.ServerSideProvider | ProviderOptions.UseComThreading; } }
        public IRawElementProviderSimple HostRawElementProvider { get { return AutomationInteropProvider.HostProviderFromHandle(owner.NativeHandle); } }
        public object GetPatternProvider(int id) { return id == TextPattern.Pattern.Id ? this : null; }
        public object GetPropertyValue(int id)
        {
            if (id == AutomationElement.ControlTypeProperty.Id) return ControlType.Document.Id;
            if (id == AutomationElement.NameProperty.Id) return "Independent fixed text provider";
            if (id == AutomationElement.IsTextPatternAvailableProperty.Id) return true;
            return null;
        }
        public ITextRangeProvider DocumentRange { get { return range; } }
        public SupportedTextSelection SupportedTextSelection { get { return SupportedTextSelection.None; } }
        public ITextRangeProvider[] GetVisibleRanges() { return new ITextRangeProvider[] { range }; }
        public ITextRangeProvider[] GetSelection() { return new ITextRangeProvider[0]; }
#if NATIVE_TEXT_ARRAYS
        int TypedTextProviderContract.GetVisibleRanges(out IntPtr array) { return NativeProviderArray.Create(GetVisibleRanges(), out array); }
        int TypedTextProviderContract.GetSelection(out IntPtr array) { return NativeProviderArray.Create(GetSelection(), out array); }
#endif
        public ITextRangeProvider RangeFromChild(IRawElementProviderSimple child) { throw new ArgumentException("No children"); }
        public ITextRangeProvider RangeFromPoint(System.Windows.Point point) { return range; }
        internal double[] Bounds() { return owner.BoundsOnUi(); }
    }

    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    private sealed class Range : MarshalByRefObject, ITextRangeProvider
    {
        private readonly Root root;
        internal Range(Root value) { root = value; }
        public ITextRangeProvider Clone() { return new Range(root); }
        public bool Compare(ITextRangeProvider range) { return range is Range; }
        public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider target, TextPatternRangeEndpoint other)
        { return (endpoint == TextPatternRangeEndpoint.End ? Payload.Length : 0) - (other == TextPatternRangeEndpoint.End ? Payload.Length : 0); }
        public void ExpandToEnclosingUnit(TextUnit unit) { }
        public ITextRangeProvider FindAttribute(int attribute, object value, bool backward) { return null; }
        public ITextRangeProvider FindText(string text, bool backward, bool ignoreCase)
        { return Payload.IndexOf(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0 ? Clone() : null; }
        public object GetAttributeValue(int attribute) { return AutomationElement.NotSupported; }
        public double[] GetBoundingRectangles() { return root.Bounds(); }
        public IRawElementProviderSimple GetEnclosingElement() { return root; }
        public string GetText(int maximum)
        { if (maximum < -1) throw new ArgumentOutOfRangeException("maximum"); return maximum < 0 ? Payload : Payload.Substring(0, Math.Min(maximum, Payload.Length)); }
        public int Move(TextUnit unit, int count) { throw new NotSupportedException(); }
        public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count) { throw new NotSupportedException(); }
        public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider target, TextPatternRangeEndpoint other) { throw new NotSupportedException(); }
        public void Select() { throw new NotSupportedException(); }
        public void AddToSelection() { throw new NotSupportedException(); }
        public void RemoveFromSelection() { throw new NotSupportedException(); }
        public void ScrollIntoView(bool top) { }
        public IRawElementProviderSimple[] GetChildren() { return new IRawElementProviderSimple[0]; }
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--server") return Server(args);
        bool control = args.Length == 4 && args[0] == "--client" && args[3] == "--control";
        bool leaProvider = args.Length == 4 && args[0] == "--client" && args[3] == "--lea";
        bool externalClient = (args.Length == 3 && args[0] == "--client") || control || leaProvider;
        bool managedOnly = args.Length == 1 && args[0] == "--managed-only";
        if (externalClient) managedOnly = true;
        using (var form = new Forms.Form { Width = 400, Height = 180, Text = "Independent UIA baseline" })
        using (var surface = new Surface { Dock = Forms.DockStyle.Fill })
        {
            IntPtr handle;
            if (externalClient)
            {
                handle = new IntPtr(Int64.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture));
                uint actual, expected = UInt32.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
                if (GetWindowThreadProcessId(handle, out actual) == 0 || actual != expected || actual == Process.GetCurrentProcess().Id)
                    throw new Exception("Provider/client process separation not verified");
                Console.WriteLine("MINIMAL_UIA CROSS_PROCESS provider=" + actual + " client=" + Process.GetCurrentProcess().Id);
                if (control) { Console.WriteLine("MINIMAL_UIA PAIR_CONTROL_PASS"); return 0; }
            }
            else { form.Controls.Add(surface); form.Show(); handle = surface.Handle; }
            var task = Task.Run(() => {
                Marshal.ThrowExceptionForHR(CoInitializeEx(IntPtr.Zero, 0));
                try
                {
                    VerifyNoLea();
                    Console.WriteLine("MINIMAL_UIA no-Lea assembly audit passed");
                    Console.WriteLine("MINIMAL_UIA client=" + typeof(TextPattern).Assembly.FullName);
                    Console.WriteLine("MINIMAL_UIA client-version=" + FileVersionInfo.GetVersionInfo(typeof(TextPattern).Assembly.Location).FileVersion);
                    Console.WriteLine("MINIMAL_UIA client-pointer-bytes=" + IntPtr.Size);
                    Console.Out.Flush();
                    int checks = 1;
#if COLD_LEGACY_CLIENT
                    Console.WriteLine("MINIMAL_UIA COLD_LEGACY no modern COM queries");
#else
                    checks += DocumentNativeClientChecks.Run(handle, leaProvider, leaProvider ? 1 : 0);
#endif
                    VerifyNoLea(); Console.WriteLine("MINIMAL_UIA no-Lea audit before managed reads passed");
                    using (var current = Process.GetCurrentProcess())
                        foreach (ProcessModule module in current.Modules)
                            if (String.Equals(module.ModuleName, "UIAutomationCore.dll", StringComparison.OrdinalIgnoreCase))
                                Console.WriteLine("MINIMAL_UIA core=" + module.FileName + "; version=" + module.FileVersionInfo.FileVersion);
                    var element = AutomationElement.FromHandle(handle);
#if WPF_BASELINE_CLIENT
                    element = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                    if (element == null) throw new Exception("WPF baseline text box missing");
                    Console.WriteLine("MINIMAL_UIA WPF descendant text box");
#endif
                    var text = (TextPattern)element.GetCurrentPattern(TextPattern.Pattern);
#if ORIGINAL_VARIANT_CLIENT
                    checks += DocumentLegacyHandleChecks.Run(text, true);
#else
                    if (!managedOnly) checks += DocumentLegacyHandleChecks.Run(text);
#endif
                    Console.WriteLine("MINIMAL_UIA managed document call"); Console.Out.Flush();
                    if (text.DocumentRange.GetText(-1) != Payload) throw new Exception("Minimal managed document"); checks++;
                    Console.WriteLine("MINIMAL_UIA managed visible array call"); Console.Out.Flush();
                    var ranges = text.GetVisibleRanges();
                    if (ranges.Length != 1) throw new Exception("Minimal visible count"); checks++;
                    UiaComponentInventory.Capture();
                    Console.WriteLine("MINIMAL_UIA managed visible text call"); Console.Out.Flush();
                    VerifyNoLea();
                    if (ranges[0].GetText(-1) != Payload) throw new Exception("Minimal managed visible text"); checks++;
                    Console.WriteLine("MINIMAL_UIA PASS " + checks); return 0;
                }
                finally { CoUninitialize(); }
            });
            var watch = Stopwatch.StartNew();
            while (!task.IsCompleted && watch.ElapsedMilliseconds < 5000) { if (!externalClient) Forms.Application.DoEvents(); Thread.Sleep(1); }
            if (!task.IsCompleted) throw new TimeoutException("Independent provider probe exceeded five seconds");
            return task.GetAwaiter().GetResult();
        }
    }

    private static void VerifyNoLea()
    {
        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            if (loaded.GetName().Name == "Lea" || loaded.GetName().Name.StartsWith("Lea.", StringComparison.Ordinal))
                throw new Exception("Independent client loaded Lea assembly");
    }
}
