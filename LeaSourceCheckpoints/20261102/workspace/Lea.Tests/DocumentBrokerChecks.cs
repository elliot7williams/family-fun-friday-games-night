using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using Lea.Win;

internal static class DocumentBrokerChecks
{
    private static void Trace(string value) { Console.WriteLine("BROKER " + value); Console.Out.Flush(); }
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(IntPtr reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    private static int Mta(Func<int> action)
    {
        int result = CoInitializeEx(IntPtr.Zero, 0);
        Marshal.ThrowExceptionForHR(result);
        Trace("explicit MTA HRESULT=" + result);
        try { return action(); }
        finally { CoUninitialize(); }
    }
    private static int Baseline(V view)
    {
        using (var form = new System.Windows.Forms.Form { Width = 360, Height = 160 })
        using (var edit = new System.Windows.Forms.TextBox { Multiline = true, ReadOnly = true, Dock = System.Windows.Forms.DockStyle.Fill, Text = "Alpha bravo baseline" })
        {
            form.Controls.Add(edit); form.Show(view.FindForm());
            IntPtr handle = edit.Handle;
            var task = Task.Run(() => Mta(() => {
                Trace("baseline Windows Edit element");
                var element = AutomationElement.FromHandle(handle);
                object pattern;
                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out pattern))
                { Trace("baseline Windows Edit TextPattern unavailable; no baseline pass claimed"); return 0; }
                Trace("baseline Windows Edit native range probe");
                int result;
                try { result = DocumentLegacyHandleChecks.Run((TextPattern)pattern); }
                catch (InvalidOperationException error)
                { Trace("baseline Windows Edit FAILED: " + error.Message + "; no baseline pass claimed"); return 0; }
                Trace("baseline Windows Edit complete");
                return result;
            }));
            var watch = Stopwatch.StartNew();
            while (!task.IsCompleted && watch.ElapsedMilliseconds < 5000)
            { System.Windows.Forms.Application.DoEvents(); Thread.Sleep(1); }
            if (!task.IsCompleted) throw new TimeoutException("Windows Edit baseline exceeded five seconds");
            return task.GetAwaiter().GetResult();
        }
    }
    internal static int Run(V view)
    {
#if !NATIVE_BROKER_ONLY
        int baselineChecks = Baseline(view);
#else
        int baselineChecks = 0;
#endif
#if NATIVE_BROKER_ONLY
        view.Sel(0, 5);
#endif
        int abiChecks = DocumentRangeAbiChecks.Run(view);
        abiChecks += DocumentRangeApartmentChecks.Run(view);
        IntPtr handle = view.Handle;
        var task = Task.Run(() => Mta(() => {
#if NATIVE_BROKER_ONLY
            return DocumentNativeClientChecks.Run(handle, true);
#else
            int checks = DocumentNativeClientChecks.Run(handle);
            Trace("element");
            var element = AutomationElement.FromHandle(handle);
            var text = (TextPattern)element.GetCurrentPattern(TextPattern.Pattern);
            checks += DocumentLegacyHandleChecks.Run(text);
            Trace("document text");
            if (!text.DocumentRange.GetText(-1).Contains("Alpha bravo")) throw new Exception("Broker document text"); checks++;
            Trace("visible array");
            var ranges = text.GetVisibleRanges();
            Trace("visible count=" + ranges.Length);
            if (ranges.Length == 0 || ranges.Length > 2048) throw new Exception("Broker visible range bounds"); checks++;
            for (int i = 0; i < ranges.Length; i++)
            {
                Trace("visible text index=" + i);
                string value = ranges[i].GetText(-1);
                if (value.Length == 0 || value.Contains("SELECTION_SECRET")) throw new Exception("Broker visible text privacy"); checks++;
                Trace("visible rectangles index=" + i);
                if (ranges[i].GetBoundingRectangles().Length == 0) throw new Exception("Broker visible geometry"); checks++;
            }
            Trace("complete"); return checks;
#endif
        }));
        var watch = Stopwatch.StartNew();
        while (!task.IsCompleted && watch.ElapsedMilliseconds < 5000) { System.Windows.Forms.Application.DoEvents(); Thread.Sleep(1); }
        if (!task.IsCompleted) throw new TimeoutException("Broker range isolation exceeded five seconds");
        return baselineChecks + abiChecks + task.GetAwaiter().GetResult();
    }
}
