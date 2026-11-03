using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class PrintWpfProbe
{
    private static int runs, missing;
    private static readonly HashSet<string> fonts = new HashSet<string>();
    private static void Inspect(Drawing drawing)
    {
        var group = drawing as DrawingGroup;
        if(group != null) foreach(Drawing child in group.Children) Inspect(child);
        var glyphs = drawing as GlyphRunDrawing;
        if(glyphs == null) return;
        runs++; fonts.Add(glyphs.GlyphRun.GlyphTypeface.FontUri.ToString());
        foreach(ushort glyph in glyphs.GlyphRun.GlyphIndices) if(glyph == 0) missing++;
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var visual = new DrawingVisual();
            using(DrawingContext drawing = visual.RenderOpen())
            {
                drawing.DrawRectangle(Brushes.White,null,new Rect(0,0,1000,260));
                var text = new FormattedText("Lea e\u0301 \uD83D\uDE00 \uD83D\uDE80\nArabic: \u0627\u0644\u0639\u0631\u0628\u064A\u0629",CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,new Typeface("Arial"),32,Brushes.Black);
                text.MaxTextWidth=940;
                drawing.DrawText(text,new Point(25,25));
            }
            Inspect(visual.Drawing);
            foreach(string font in fonts) Console.WriteLine("FONT "+font);
            Console.WriteLine("GLYPH_RUNS "+runs+" MISSING "+missing);
            if(runs < 2 || missing != 0) throw new Exception("Fallback contains missing glyph or no shaped runs");
            var bitmap = new RenderTargetBitmap(1000,260,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
            var pixels = new byte[1000*260*4];bitmap.CopyPixels(pixels,4000,0);
            int ink=0;for(int i=0;i<pixels.Length;i+=4) if(pixels[i]<200 && pixels[i+3]>0) ink++;
            if(ink<100) throw new Exception("Blank text output");
            var encoder = new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using(var stream=File.Create(Path.Combine(Path.GetDirectoryName(args[0]),"wpf-font-fallback.png"))) encoder.Save(stream);
            Console.WriteLine("PASS Lea WPF glyph diagnostic generated");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
