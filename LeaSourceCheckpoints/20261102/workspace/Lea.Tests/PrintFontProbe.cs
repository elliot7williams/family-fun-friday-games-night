using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

internal static class PrintFontProbe
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            string folder = Path.GetDirectoryName(args[0]);
            using(var bitmap = new Bitmap(1000,400))
            {
                bitmap.SetResolution(96,96);
                using(Graphics graphics = Graphics.FromImage(bitmap))
                using(var label = new Font("Arial",12,FontStyle.Regular,GraphicsUnit.Pixel))
                {
                    graphics.Clear(Color.White);
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    int row = 0;
                    foreach(string family in new[]{"Arial","Segoe UI Emoji"})
                    using(var font = new Font(family,32,FontStyle.Regular,GraphicsUnit.Pixel))
                    {
                        Console.WriteLine("FONT requested="+family+" actual="+font.Name);
                        graphics.DrawString("GDI+ "+family,label,Brushes.Black,10,row+5);
                        using(var format = (StringFormat)StringFormat.GenericTypographic.Clone())
                            graphics.DrawString("Lea e\u0301 \uD83D\uDE00 \uD83D\uDE80",font,Brushes.Black,new RectangleF(220,row,700,90),format);
                        row+=100;
                        graphics.DrawString("GDI TextRenderer "+family,label,Brushes.Black,10,row+5);
                        TextRenderer.DrawText(graphics,"Lea e\u0301 \uD83D\uDE00 \uD83D\uDE80",font,new Rectangle(220,row,700,90),Color.Black,
                            TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix|TextFormatFlags.SingleLine);
                        row+=100;
                    }
                }
                bitmap.Save(Path.Combine(folder,"font-comparison.png"),ImageFormat.Png);
            }
            Console.WriteLine("PASS Lea print font diagnostic generated");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
