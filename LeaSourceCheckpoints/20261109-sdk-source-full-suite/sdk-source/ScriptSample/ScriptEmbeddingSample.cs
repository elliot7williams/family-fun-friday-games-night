using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;
using Forms = System.Windows.Forms;

internal sealed class ScriptEmbeddingSample : Forms.Form
{
    private readonly V view = new V { Dock = Forms.DockStyle.Fill, Je = false };
    private readonly Forms.ToolStripTextBox address = new Forms.ToolStripTextBox { AutoSize = false, Width = 360 };
    private readonly Forms.ToolStripButton scripts = new Forms.ToolStripButton("Local Scripts") { CheckOnClick = true };
    private readonly Forms.ToolStripStatusLabel status = new Forms.ToolStripStatusLabel();
    private int checks;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    private ScriptEmbeddingSample(string input)
    {
        Text = "Lea script embedding sample"; ClientSize = new Size(800, 480);
        AutoScaleMode = Forms.AutoScaleMode.None;
        var toolbar = new Forms.ToolStrip { GripStyle = Forms.ToolStripGripStyle.Hidden, Dock = Forms.DockStyle.Top };
        var go = new Forms.ToolStripButton("Go");
        toolbar.Items.Add(address); toolbar.Items.Add(go); toolbar.Items.Add(scripts);
        var footer = new Forms.StatusStrip(); footer.Items.Add(status);
        Controls.Add(view); Controls.Add(toolbar); Controls.Add(footer);
        view.Ng = LocalFile;
        address.Text = input;
        go.Click += (sender, args) => Navigate();
        address.KeyDown += (sender, args) => { if (args.KeyCode == Forms.Keys.Enter) { args.SuppressKeyPress = true; Navigate(); } };
        scripts.CheckedChanged += (sender, args) => {
            if (scripts.Checked) { W.On(view); view.Je = true; Navigate(); }
            else { view.Je = false; W.Off(view); status.Text = "Scripts off"; }
        };
        Resize += (sender, args) => address.Width = Math.Max(80, ClientSize.Width - 220);
        FormClosing += (sender, args) => W.Off(view);
    }
    private void Navigate()
    {
        try
        {
            Uri target;
            if (!Uri.TryCreate(address.Text, UriKind.Absolute, out target)) target = new Uri(Path.GetFullPath(address.Text));
            if (!LocalFile(target)) throw new InvalidOperationException("Only local files are allowed in this sample.");
            view.Go(target); status.Text = address.Text;
        }
        catch (Exception error) { status.Text = error.Message; }
    }
    private static bool LocalFile(Uri target)
    {
        if (target == null || !target.IsAbsoluteUri || !target.IsFile || target.IsUnc) return false;
        try { return new DriveInfo(Path.GetPathRoot(target.LocalPath)).DriveType != DriveType.Network; }
        catch (ArgumentException) { return false; }
        catch (IOException) { return false; }
    }
    private void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static async Task Until(Func<bool> done)
    {
        var clock = Stopwatch.StartNew();
        while (!done() && clock.ElapsedMilliseconds < 10000) await Task.Delay(5);
        if (!done()) throw new TimeoutException("Script sample workflow deadline");
    }
    private async Task Test(string output)
    {
        var workerFiles = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker"));
        var acl = workerFiles.ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
        Check(view.Xf == null && !scripts.Checked && !view.Je, "sample scripting defaults off");
        Check(!LocalFile(new Uri("http://example.invalid/")) && !LocalFile(new Uri("https://example.invalid/")) && !LocalFile(new Uri("file://server/share/page.html")),
            "sample navigation guard rejects HTTP HTTPS and UNC before access");
        Navigate();
        await Until(() => !view.Busy && view.At != null);
        Check(view.T == "Lea script sample", "script-off navigation preserves initial title: actual=" + view.T + "; navigation=" + status.Text);
        Check(view.Xs == null, "script-off page creates no script session");
        scripts.Checked = true;
        await Until(() => !view.Busy && view.Xn == 0 && view.Xo != null);
        Check(view.Xo.Ok, "sample automatic script activation: " + view.Xo.S + "; " + view.Xo.M);
        Check(view.T == "Lea ready", "sample inline script updates title");
        Check(view.Dtx().Contains("Native JavaScript ready"), "sample script updates rendered document");
        var adapter = view.Xs as W;
        Check(adapter != null && adapter.At.Pid != 0, "sample uses isolated native worker");
        Check(adapter.At.Ac && adapter.At.Ne && adapter.At.Nd && adapter.At.Fd && adapter.At.Mg == 15, "sample native isolation flags");
        Check(adapter.At.Tm == 2000 && adapter.At.Cp == 2500 && adapter.At.Mm == 134217728, "sample original execution budgets");
        var endpoint = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(adapter.At);
        var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
        var handle = process.SafeHandle; bool retained = false; handle.DangerousAddRef(ref retained);
        try
        {
            Check(retained && WaitForSingleObject(handle.DangerousGetHandle(), 0) == 258, "sample original worker handle alive");
            var result = await view.JsA("document.getElementById('result').textContent='Consumer API ready';");
            Check(result.Ok && view.Dtx().Contains("Consumer API ready"), "sample consumer API reaches DOM");
            view.Refresh();
            using (var bitmap = new Bitmap(view.Width, view.Height))
            {
                view.DrawToBitmap(bitmap, view.ClientRectangle);
                bitmap.Save(Path.Combine(output, "script-sample.png"), ImageFormat.Png);
                int ink = 0;
                for (var y = 0; y < bitmap.Height; y++)
                    for (var x = 0; x < bitmap.Width; x++) { var pixel = bitmap.GetPixel(x, y); if (pixel.R < 80 && pixel.G < 80 && pixel.B < 80) ink++; }
                Check(ink > 100, "sample rendered output is nonblank");
            }
            using (var window = new Bitmap(Width, Height))
            { DrawToBitmap(window, new Rectangle(0, 0, Width, Height)); window.Save(Path.Combine(output, "script-sample-window.png"), ImageFormat.Png); }
            scripts.Checked = false;
            Check(!view.Je && view.Xf == null && view.Xs == null, "sample opt-out disables permission and detaches adapter");
            Check(WaitForSingleObject(handle.DangerousGetHandle(), 5000) == 0, "sample opt-out retires original worker");
            Check((await view.JsA("document.title='Denied';")).S == "off", "sample opt-out rejects supplied code");
            Check(workerFiles.All(path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == acl[path]), "sample opt-out restores exact worker ACLs");
        }
        finally { W.Off(view); if (retained) handle.DangerousRelease(); }
        Console.WriteLine("PASS LEA SDK SCRIPT SAMPLE " + checks);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        bool test = args.Length == 2 && args[0] == "--test";
        if (!test && args.Length > 1) return 2;
        string input = !test && args.Length == 1 ? args[0] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "script-embedding.html");
        Forms.Application.EnableVisualStyles();
        int exit = 0;
        using (var form = new ScriptEmbeddingSample(input))
        {
            form.Shown += async (sender, eventArgs) => {
                if (!test) { form.Navigate(); return; }
                try { await form.Test(args[1]); }
                catch (Exception error) { Console.Error.WriteLine(error); exit = 1; }
                finally { form.Close(); }
            };
            Forms.Application.Run(form);
        }
        return exit;
    }
}
