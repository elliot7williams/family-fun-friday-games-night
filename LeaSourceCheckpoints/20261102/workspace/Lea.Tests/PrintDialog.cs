using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Print;

internal static class PrintDialogChecks
{
    private delegate bool EnumWindow(IntPtr window, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint thread, EnumWindow callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindow callback, IntPtr parameter);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr window, uint command);
    [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr window, uint flags);
    [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] private static extern int GetClassName(IntPtr window, System.Text.StringBuilder text, int length);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    private static int checks;
    private static void Check(bool condition, string name)
    { if (!condition) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static bool Denied(Action action)
    {
        try { action(); } catch (ArgumentException) { return true; } catch (InvalidOperationException) { return true; }
        return false;
    }
    private static bool Choose(Pj selection, Form owner, bool accept)
    {
        bool seen = false, posted = false;
        IntPtr ownerHandle = owner.Handle;
        using (var timer = new Timer { Interval = 100 })
        {
            timer.Tick += delegate
            {
                if (seen) return;
                EnumWindows(delegate(IntPtr window, IntPtr unused)
                {
                    var name = new System.Text.StringBuilder(64); GetClassName(window, name, name.Capacity);
                    if (window == ownerHandle || !IsWindowVisible(window) || GetAncestor(window, 3) != ownerHandle || name.ToString() != "#32770") return true;
                    seen = true;
                    // PrintDialog only returns settings; Pj contains no StartDoc/print call.
                    posted = PostMessage(window, 0x111, new IntPtr(accept ? 1 : 2), IntPtr.Zero);
                    return false;
                }, IntPtr.Zero);
            };
            timer.Start(); bool result = selection.Sh(owner); timer.Stop();
            Check(seen && posted, "dialog owned native selection action " + accept);
            return result;
        }
    }
    public static void Run()
    {
        checks = 0;
        Check(Environment.Is64BitProcess, "dialog fixture matches current64-bit AnyCPU browser");
        Check(Denied(() => new Pj(0)), "dialog zero page bound denied");
        Check(Denied(() => new Pj(Pw.MaxPages + 1)), "dialog excessive page bound denied");
        var seed = new PrinterSettings { PrinterName = "Microsoft XPS Document Writer" };
        if (!seed.IsValid) throw new InvalidOperationException("Native cancellation test requires installed Microsoft XPS Document Writer");
        using (var selection = new Pj(12, seed))
        using (var owner = new Form { Text = "Lea Printer Dialog Test", ClientSize = new Size(500, 300) })
        {
            Check(seed.MinimumPage == 0 && seed.MaximumPage == 9999, "dialog constructor does not mutate caller settings");
            Check(Denied(() => { var settings = selection.St; }), "dialog unaccepted settings denied");
            Check(Denied(() => selection.Sh(null)), "dialog null owner denied");
            Check(Denied(() => selection.Sh(owner)), "dialog uncreated owner denied");
            owner.Show(); owner.Update();
            Check(Task.Run(() => Denied(() => selection.Sh(owner))).Result, "dialog worker thread denied");
            Check(Task.Run(() => Denied(() => selection.Dispose())).Result, "dialog wrong thread disposal denied");
            bool seen = false, disabled = false, reentry = false, busyDispose = false, busySettings = false, cancelPosted = false;
            IntPtr ownerHandle = owner.Handle;
            uint thread = GetCurrentThreadId();
            bool tracedTimer = false;
            using (var timer = new Timer { Interval = 100 })
            {
                timer.Tick += delegate
                {
                    if (seen) return;
                    if (!tracedTimer)
                    {
                        tracedTimer = true; Console.WriteLine("TRACE printer modal timer dispatched on " + thread + " owner=" + ownerHandle);
                        EnumThreadWindows(thread, delegate(IntPtr window, IntPtr unused)
                        {
                            var name = new System.Text.StringBuilder(64); GetClassName(window, name, name.Capacity);
                            Console.WriteLine("TRACE thread window=" + window + " class=" + name + " visible=" + IsWindowVisible(window) + " owner=" + GetWindow(window, 4) + " rootOwner=" + GetAncestor(window, 3));
                            return true;
                        }, IntPtr.Zero);
                    }
                    EnumWindows(delegate(IntPtr window, IntPtr unused)
                    {
                        var name = new System.Text.StringBuilder(64); GetClassName(window, name, name.Capacity);
                        if (window == ownerHandle || !IsWindowVisible(window) || GetAncestor(window, 3) != ownerHandle || name.ToString() != "#32770") return true;
                        seen = true; disabled = !IsWindowEnabled(ownerHandle);
                        Console.WriteLine("TRACE printer dialog owner matched, native disabled=" + disabled);
                        reentry = Denied(() => selection.Sh(owner));
                        busyDispose = Denied(() => selection.Dispose());
                        busySettings = Denied(() => { var settings = selection.St; });
                        // Cancel only the visible native dialog rooted at this exact test owner.
                        cancelPosted = PostMessage(window, 0x111, new IntPtr(2), IntPtr.Zero);
                        Console.WriteLine("TRACE printer cancel posted=" + cancelPosted);
                        return false;
                    }, IntPtr.Zero);
                };
                timer.Start();
                bool accepted = selection.Sh(owner);
                timer.Stop();
                Check(seen && disabled, "dialog native owner and modal disable verified");
                Check(cancelPosted && !accepted, "dialog native cancel returns false without a job");
            }
            Check(reentry, "dialog reentry denied");
            Check(busyDispose, "dialog busy disposal denied");
            Check(busySettings, "dialog pending settings denied");
            Check(IsWindowEnabled(ownerHandle) && !owner.IsDisposed, "dialog cancellation restores live owner");
            Check(Denied(() => { var settings = selection.St; }), "dialog canceled settings denied");
            Check(Choose(selection, owner, true), "dialog native acceptance returns settings without dispatch");
            var chosen = selection.St;
            Check(chosen.PrinterName == "Microsoft XPS Document Writer" && chosen.PrintRange == System.Drawing.Printing.PrintRange.AllPages, "dialog accepted printer and all-page selection");
            Check(chosen.MinimumPage == 1 && chosen.MaximumPage == 12, "dialog accepted page bounds retained");
            Check(!Object.ReferenceEquals(chosen, selection.St), "dialog accepted settings defensively cloned");
            PrintTicketChecks.Run(selection);
            chosen.PrinterName = "Lea mutation sentinel";
            Check(selection.St.PrinterName == "Microsoft XPS Document Writer", "dialog caller cannot mutate accepted selection");
            Check(!Choose(selection, owner, false), "dialog cancel after acceptance returns false");
            Check(Denied(() => { var settings = selection.St; }), "dialog cancel clears previously accepted settings");
            selection.Dispose(); selection.Dispose();
            Check(Denied(() => selection.Sh(owner)), "dialog disposed selection denied");
            Check(Denied(() => { var settings = selection.St; }), "dialog disposed settings denied");
            owner.Close();
        }
        Console.WriteLine("PASS Lea native printer selection: " + checks);
    }
}
