using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lea.Print;

internal static class PrintFrameMain
{
    private static void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);}
    private static void Reject(Action action,string name){bool denied=false;try{action();}catch(ArgumentException){denied=true;}catch(InvalidOperationException){denied=true;}Check(denied,name);}
    private static bool In(Pb b,int x,int y){return b.Height>0&&x>=b.X*.96&&x<=(b.X+b.Width)*.96&&y>=b.Y*.96&&y<=(b.Y+b.Height)*.96;}
    private static void Image(Visual visual,Pg geometry,string path,bool bands)
    {
        var bitmap=new RenderTargetBitmap(816,1056,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);
        var pixels=new byte[816*1056*4];bitmap.CopyPixels(pixels,816*4,0);int header=0,footer=0,body=0,outside=0;
        for(int y=0;y<1056;y++)for(int x=0;x<816;x++)if(pixels[(y*816+x)*4]<200){if(In(geometry.Header,x,y))header++;else if(In(geometry.Footer,x,y))footer++;else if(In(geometry.Body,x,y))body++;else outside++;}
        Check(body>100,"framed body visible");Check(outside==0,"ink inside reserved areas");
        Check(bands?header>100&&footer>100:header==0&&footer==0,"header/footer option respected");
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(path))encoder.Save(stream);
    }
    [STAThread]
    private static int Main(string[] args)
    { return Run(args); }
    internal static int Run(string[] args)
    {
        try
        {
            if(PrintWpfLayoutMain.Run(args)!=0)return 1;
            var geometry=Pg.Cp(new Po(false,.75,100,true,true,true,true),850,1100,new Pb(25,25,800,1050));
            using(var plan=Pw.Cp("Lea framed selected text.\r\nEmoji \uD83D\uDE00 and accent e\u0301.",geometry))
            {
                var frame=new Pf(plan,"Lea title\ncontinued \uD83D\uDE00","https://user:secret@example.test/article?token=secret#private");
                Check(frame.Title=="Lea title continued \uD83D\uDE00","title line normalized");
                Check(frame.Address=="https://example.test/article","address secrets removed");
                Image(frame.Dr(0),geometry,Path.Combine(Path.GetDirectoryName(args[0]),"framed-page.png"),true);
                Check(new Pf(plan,"title","javascript:alert(1)").Address=="","active source address omitted");
                Reject(()=>new Pf(plan,new string('x',4097),""),"metadata cap");
                Reject(()=>new Pf(plan,"",new string('x',4097)),"address cap");
                Reject(()=>frame.Dr(-1),"frame invalid page");
                plan.Dispose();Reject(()=>frame.Dr(0),"frame honors plan lifetime");
            }
            var plain=Pg.Cp(new Po(false,.75,100,false,false,false,true),850,1100,new Pb(25,25,800,1050));
            using(var plan=Pw.Cp("No header or footer",plain))
                Image(new Pf(plan,"hidden title","https://example.test/").Dr(0),plain,Path.Combine(Path.GetDirectoryName(args[0]),"unframed-page.png"),false);
            Console.WriteLine("PASS Lea print frame and foundation");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
