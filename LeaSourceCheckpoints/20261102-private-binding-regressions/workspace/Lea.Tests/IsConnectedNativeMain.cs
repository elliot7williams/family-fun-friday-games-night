using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Script.Win;
using Lea.Win;

internal static class IsConnectedNativeMain
{
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    private static async Task Activate(V view, string id)
    {
        for (int index = 0; index <= view.Kn; index++)
        {
            if (view.Ke != null && view.Ke.Av("id") == id) { await view.Ka(); return; }
            view.Kf();
        }
        throw new Exception("Native focus target absent: " + id);
    }
    private static void Paint(V view, string path)
    {
        view.Sc(-view.Sy);
        using (var image = new Bitmap(view.Width, view.Height))
        {
            view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            int colored = 0;
            for (int y = 0; y < image.Height; y += 4) for (int x = 0; x < image.Width; x += 4)
                if (image.GetPixel(x, y).ToArgb() != Color.White.ToArgb()) colored++;
            Require(colored > 300, "Native fixture must render nonblank");
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
    private static async Task Check(Form form, V view, string fixture, string output)
    {
        var page = await view.GoA(fixture);
        Require(view.Xo != null && view.Xo.Ok && view.T == "Connectivity ready", "Inline connectivity lifecycle failed");
        var runtime = ((W)view.Xs).At;
        Require(runtime.Ac && runtime.Nd && runtime.Ne && runtime.Fd, "Native worker guards remain active");
        var field = page.Fs.Single(item => item.El.Av("id") == "field");
        await Activate(view, "field");
        var editor = view.Controls.OfType<TextBox>().Single();
        editor.Text = "Retained edit"; await view.Xt;
        Require(field.V == "Retained edit" && editor.Visible, "Native editor must retain edit");
        Paint(view, Path.Combine(output, "connected-desktop.png"));
        await Activate(view, "detach");
        Require(view.Xo.Ok && view.T == "Connectivity detached", "Native activation must update connectivity");
        var read = await view.JsA("if(connectivityChild.isConnected||connectivityGroup.isConnected||!document.isConnected)throw Error('live detached read');");
        Require(read.Ok && read.N == 0 && field.V == "Retained edit", "Detached read preserves retained field state");
        Require(!view.Controls.OfType<TextBox>().Any(item => item.Visible), "Detached field editor must close");
        Paint(view, Path.Combine(output, "detached-desktop.png"));
        form.ClientSize = new Size(420, 700);
        Paint(view, Path.Combine(output, "detached-narrow.png"));
        using (var owned = Process.GetProcessById(runtime.Pid))
        {
            var handle = owned.Handle;
            view.Dispose();
            Require(owned.WaitForExit(2000) && !runtime.Pc && runtime.Pid == 0, "Exact native worker must retire");
        }
    }
    [STAThread]
    public static int Main(string[] args)
    {
        Exception failure = null;
        using (var form = new Form { ClientSize = new Size(940, 700), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
        using (var view = new V { Dock = DockStyle.Fill })
        {
            form.Controls.Add(view); W.On(view, null, 5000);
            form.Shown += async (sender, unused) => { try { await Check(form, view, args[0], args[1]); } catch (Exception error) { failure = error; } finally { form.Close(); } };
            Application.Run(form);
        }
        if (failure != null) { Console.Error.WriteLine(failure); return 1; }
        Console.WriteLine("PASS Lea isConnected native: inline lifecycle, real activation, field retention, detached editor cleanup, desktop/narrow render, guarded worker retirement.");
        return 0;
    }
}
