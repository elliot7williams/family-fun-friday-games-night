using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lea.Print;

internal static class PrintPreviewMain
{
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
    private static void Reject(Action action, string name)
    {
        bool denied = false;
        try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; }
        Check(denied, name);
    }
    private static T Find<T>(DependencyObject root, string name) where T : DependencyObject
    {
        var match = root as T;
        if (match != null && AutomationProperties.GetName(match) == name) return match;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        { var found = Find<T>(VisualTreeHelper.GetChild(root, i), name); if (found != null) return found; }
        return null;
    }
    private static void Image(Pv preview, string file)
    {
        preview.UpdateLayout();
        int width = (int)Math.Ceiling(preview.ActualWidth), height = (int)Math.Ceiling(preview.ActualHeight);
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32); bitmap.Render(preview);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var output = new FileStream(file, FileMode.CreateNew)) png.Save(output);
        Check(width > 300 && height > 300, "preview rendered window dimensions");
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if (PrintXpsMain.Run(args) != 0) return 1;
            var geometry = Pg.Cp(new Po(false, .5, 100, true, true, true, true), 850, 400, new Pb(0, 0, 850, 400));
            using (var plan = Pw.Cp(String.Join("\n", Enumerable.Range(1, 36).Select(i => "Lea preview line " + i + " emoji \uD83D\uDE00")), geometry))
            {
                var pages = new Pp(plan, "Lea print preview", "https://example.test/article");
                using (var preview = new Pv(pages))
                {
                    var window = new Window { Title = "Lea Print Preview Test", Width = 1000, Height = 720, Content = preview, WindowStartupLocation = WindowStartupLocation.CenterScreen };
                    try
                    {
                        window.Show(); window.UpdateLayout();
                        Check(preview.Count == pages.PageCount && preview.Count > 1 && preview.Page == 0 && preview.Zoom == 1, "preview initial state");
                        var previous = Find<Button>(preview, "Previous page"); var next = Find<Button>(preview, "Next page");
                        Check(previous != null && next != null && !previous.IsEnabled && next.IsEnabled, "preview first-page buttons");
                        next.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Check(preview.Page == 1 && previous.IsEnabled, "preview next command");
                        previous.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Check(preview.Page == 0, "preview previous command");
                        preview.Pg(preview.Count - 1); Check(!next.IsEnabled && previous.IsEnabled, "preview last-page buttons");
                        preview.Pg(0); preview.Zm(1.5); Check(preview.Zoom == 1.5, "preview programmatic zoom");
                        var zoom = Find<ComboBox>(preview, "Preview zoom"); zoom.SelectedItem = 75;
                        Check(preview.Zoom == .75, "preview zoom menu command");
                        Find<Button>(preview, "Fit page width").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Check(preview.Zoom >= .25 && preview.Zoom <= 3 && zoom.SelectedItem != null, "preview fit-width command and displayed zoom");
                        Image(preview, Path.Combine(Path.GetDirectoryName(args[0]), "preview-desktop.png"));
                        window.Width = 360; window.Height = 600; window.UpdateLayout(); preview.Fit(); window.UpdateLayout();
                        Check(preview.ActualWidth < 400 && next.IsVisible && zoom.IsVisible, "preview narrow window controls");
                        Image(preview, Path.Combine(Path.GetDirectoryName(args[0]), "preview-narrow.png"));
                        Reject(() => preview.Pg(-1), "preview negative page");
                        Reject(() => preview.Pg(preview.Count), "preview missing page");
                        Reject(() => preview.Zm(Double.NaN), "preview NaN zoom");
                        Reject(() => preview.Zm(Double.PositiveInfinity), "preview infinite zoom");
                        Reject(() => preview.Zm(.1), "preview minimum zoom");
                        Reject(() => preview.Zm(4), "preview maximum zoom");
                        Check(Task.Run(() => { try { preview.Zm(1); return false; } catch (InvalidOperationException) { return true; } }).Result, "preview thread affinity");
                    }
                    finally { window.Close(); }
                    preview.Dispose(); preview.Dispose();
                    Reject(() => preview.Pg(0), "preview disposed navigation");
                    Check(pages.PageCount > 0, "preview does not own plan");
                }
                using (var preview = new Pv(pages)) { plan.Dispose(); Reject(() => preview.Pg(0), "preview disposed plan"); }
            }
            Reject(() => new Pv(null), "preview null paginator");
            Console.WriteLine("PASS Lea preview and foundation"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
