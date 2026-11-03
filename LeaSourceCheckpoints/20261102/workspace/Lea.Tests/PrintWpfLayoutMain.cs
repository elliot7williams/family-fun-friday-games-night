using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lea.Print;

internal static class PrintWpfLayoutMain
{
    private static int missing, glyphs;
    private static void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);}
    private static void Reject(Action action,string name)
    {
        bool denied=false;try{action();}catch(ArgumentException){denied=true;}catch(InvalidOperationException){denied=true;}
        Check(denied,name);
    }
    private static void Drawing(Drawing drawing)
    {
        var group=drawing as DrawingGroup;if(group!=null)foreach(Drawing child in group.Children)Drawing(child);
        var run=drawing as GlyphRunDrawing;
        if(run!=null)foreach(ushort glyph in run.GlyphRun.GlyphIndices){glyphs++;if(glyph==0)missing++;}
        var geometry=drawing as GeometryDrawing;
        if(geometry!=null){var brush=geometry.Brush as VisualBrush;if(brush!=null)Visual(brush.Visual);}
    }
    private static void Visual(Visual visual)
    {
        DrawingGroup drawing=VisualTreeHelper.GetDrawing(visual);if(drawing!=null)Drawing(drawing);
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(visual);i++){var child=VisualTreeHelper.GetChild(visual,i) as Visual;if(child!=null)Visual(child);}
    }
    private static void Image(Visual visual,Pg geometry,string path,int dpi)
    {
        var root=new DrawingVisual();using(var drawing=root.RenderOpen())drawing.DrawRectangle(Brushes.White,null,new Rect(0,0,816,1056));
        var group=new ContainerVisual();group.Children.Add(root);group.Children.Add(visual);
        int width=816*dpi/96,height=1056*dpi/96;
        var bitmap=new RenderTargetBitmap(width,height,dpi,dpi,PixelFormats.Pbgra32);bitmap.Render(group);
        var pixels=new byte[width*height*4];bitmap.CopyPixels(pixels,width*4,0);int ink=0,outside=0;
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)if(pixels[(y*width+x)*4]<200){ink++;if(x<geometry.Body.X*dpi/100-1||y<geometry.Body.Y*dpi/100-1||x>(geometry.Body.X+geometry.Body.Width)*dpi/100+1||y>(geometry.Body.Y+geometry.Body.Height)*dpi/100+1)outside++;}
        Check(ink>100,"nonblank WPF text "+dpi);Check(outside==0,"WPF ink body bounds "+dpi);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(path))encoder.Save(stream);
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if(PrintSnapshotChecks.Run()!=40||PrintGeometryChecks.Run()!=20||PrintCaptureProbe.Run(args)!=0)throw new Exception("Foundation failed");
            var text=new StringBuilder();for(int i=0;i<120;i++)text.Append("Paragraph "+i+": Lea e\u0301 \uD83D\uDE00 \uD83D\uDE80 Arabic \u0627\u0644\u0639\u0631\u0628\u064A\u0629\r\n");
            Pg geometry=Pg.Cp(new Po(false,.75,100,true,true,true,true),850,1100,new Pb(25,25,800,1050));
            using(Pw plan=Pw.Cp(text.ToString(),geometry))
            {
                Check(plan.PageText.Count>1,"WPF multiple pages");Check(plan.Source==text.ToString(),"original text retained");
                Check(String.Concat(plan.PageText)==plan.DocumentText,"contiguous normalized page text");
                for(int i=0;i<plan.PageText.Count;i++)Visual(plan.Dr(i));
                Check(glyphs>100&&missing==0,"no missing glyph IDs across pages");
                string folder=Path.GetDirectoryName(args[0]);
                Image(plan.Dr(0),geometry,Path.Combine(folder,"wpf-first-96.png"),96);
                Image(plan.Dr(plan.PageText.Count-1),geometry,Path.Combine(folder,"wpf-last-144.png"),144);
                Console.WriteLine("PAGES "+plan.PageText.Count+" GLYPHS "+glyphs+" MISSING "+missing);
                Reject(()=>plan.Dr(-1),"negative page denied");
                Reject(()=>plan.Dr(plan.PageText.Count),"past last page denied");
                Reject(()=>Task.Run(()=>plan.Dr(0)).GetAwaiter().GetResult(),"cross thread drawing denied");
                plan.Dispose();Reject(()=>plan.Dr(0),"disposed drawing denied");
            }
            Reject(()=>Pw.Cp(" ",geometry),"blank WPF text denied");
            Reject(()=>Pw.Cp(new string('x',Pw.MaxText+1),geometry),"WPF text cap");
            Reject(()=>Pw.Cp("x",null),"null WPF geometry denied");
            Reject(()=>Pw.Cp("x",geometry,Double.NaN),"NaN WPF font denied");
            Reject(()=>Pw.Cp("x",geometry,Double.PositiveInfinity),"infinite WPF font denied");
            Reject(()=>Pw.Cp("x",geometry,5),"small WPF font denied");
            Reject(()=>Pw.Cp("x",geometry,73),"large WPF font denied");
            Reject(()=>Task.Run(()=>Pw.Cp("x",geometry)).GetAwaiter().GetResult(),"MTA capture denied");
            var tiny=Pg.Cp(new Po(false,0,100,false,false,false,true),1,1,new Pb(0,0,1,1));
            Reject(()=>Pw.Cp("cannot fit",tiny),"tiny WPF page denied");
            var small=Pg.Cp(new Po(false,0,100,false,false,false,true),100,40,new Pb(0,0,100,40));
            Reject(()=>Pw.Cp(String.Concat(System.Linq.Enumerable.Repeat("line\r\n",600)),small),"WPF page cap enforced");
            Console.WriteLine("PASS Lea paginated WPF text and foundation");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
