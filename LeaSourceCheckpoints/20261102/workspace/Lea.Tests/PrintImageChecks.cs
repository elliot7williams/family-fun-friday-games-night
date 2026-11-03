using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Lea.Engine;
using Lea.Print;

internal static class PrintImageChecks
{
    private static N Node(string kind)
    { return (N)Activator.CreateInstance(typeof(N), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { kind }, null); }
    internal static Ps Snapshot(int images)
    {
        var root = Node("article");
        for (int i = 0; i < images; i++) { var image = Node("img"); image.A["src"] = "https://example.test/picture.png"; image.A["alt"] = "<image & text>"; root.C.Add(image); }
        var page = (P)Activator.CreateInstance(typeof(P), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { new Uri("https://example.test/") }, null);
        typeof(P).GetProperty("D").SetValue(page, root, null);
        typeof(P).GetProperty("U").SetValue(page, new Uri("https://example.test/"), null);
        return Ps.Cf(page);
    }
    internal static int Run(string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        Action<Action, string> reject = (action, name) => { bool denied = false; try { action(); } catch (ArgumentException) { denied = true; } catch (InvalidOperationException) { denied = true; } check(denied, name); };
        var options = new Po(false, .75, 100, false, false, false, false);
        var snapshot = Snapshot(1); int imageIndex = snapshot.Nodes.Select((node, index) => new { node, index }).Single(p => p.node.Kind == "img").index;
        Pi picture;
        using (var bitmap = new Bitmap(3, 2))
        {
            bitmap.SetPixel(0, 0, Color.Red); bitmap.SetPixel(1, 0, Color.Lime); bitmap.SetPixel(2, 0, Color.Blue);
            picture = Pi.Cp(bitmap); bitmap.SetPixel(0, 0, Color.Black);
        }
        check(picture.Width == 3 && picture.Height == 2, "detached print pixel dimensions");
        var images = new Dictionary<int, Pi> { { imageIndex, picture } };
        string html = Ht.Cp(snapshot, options, null, images);
        var element = XDocument.Parse(html).Descendants("img").Single();
        check((string)element.Attribute("alt") == "<image & text>", "image alt escaping");
        check((string)element.Attribute("width") == "3" && (string)element.Attribute("height") == "2", "HTML image dimensions");
        string data = (string)element.Attribute("src");
        check(data.StartsWith("data:image/png;base64,", StringComparison.Ordinal), "embedded PNG only");
        byte[] png = Convert.FromBase64String(data.Substring("data:image/png;base64,".Length));
        check(png.Length <= Pi.MaxBytes && png.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "bounded PNG signature");
        using (var stream = new MemoryStream(png))
        using (var reopened = new Bitmap(stream))
            check(reopened.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb() && reopened.GetPixel(1, 0).ToArgb() == Color.Lime.ToArgb(), "detached source pixels retained");
        check(!html.Contains("https://example.test/picture.png"), "source image URL not copied");
        check(!XDocument.Parse(Ht.Cp(snapshot, options, null)).Descendants("img").Any(), "explicit text-only overload unchanged");
        reject(() => Pi.Cp(null), "null image");
        using (var bounded = (Stream)Activator.CreateInstance(typeof(Pi).GetNestedType("BoundedStream", BindingFlags.NonPublic), true))
        {
            bounded.SetLength(Pi.MaxBytes); bounded.Position = Pi.MaxBytes - 1; bounded.WriteByte(1);
            check(bounded.Length == Pi.MaxBytes, "encoded byte boundary accepted");
            reject(() => bounded.WriteByte(1), "encoded byte boundary rejected");
            reject(() => bounded.SetLength(Pi.MaxBytes + 1L), "encoded stream length bound");
            bounded.Position = Pi.MaxBytes;
            reject(() => bounded.Write(new byte[1], 0, 1), "encoded block write bound");
        }
        using (var wide = new Bitmap(Pi.MaxAxis + 1, 1)) reject(() => Pi.Cp(wide), "image axis bound");
        using (var large = new Bitmap(1025, 1024)) reject(() => Pi.Cp(large), "image pixel bound");
        reject(() => Ht.Cp(snapshot, options, null, new Dictionary<int, Pi>()), "missing image fails closed");
        reject(() => Ht.Cp(snapshot, options, null, new Dictionary<int, Pi> { { imageIndex, null } }), "null image value");
        reject(() => Ht.Cp(snapshot, options, null, new Dictionary<int, Pi> { { 0, picture } }), "nonimage index");
        reject(() => Ht.Cp(snapshot, options, null, new Dictionary<int, Pi> { { -1, picture } }), "negative image index");
        reject(() => Ht.Cp(snapshot, new Po(false, .75, 100, false, false, false, true), "text", images), "selected text cannot carry images");
        var excess = new Dictionary<int, Pi>(); for (int i = 0; i < 17; i++) excess.Add(i, picture);
        reject(() => Ht.Cp(snapshot, options, null, excess), "image count bound");
        using (var noise = new Bitmap(1024, 768, PixelFormat.Format32bppArgb))
        {
            var pixels = new byte[1024 * 768 * 4]; new Random(714).NextBytes(pixels);
            for (int at = 3; at < pixels.Length; at += 4) pixels[at] = 255;
            var locked = noise.LockBits(new Rectangle(0, 0, 1024, 768), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try { Marshal.Copy(pixels, 0, locked.Scan0, pixels.Length); } finally { noise.UnlockBits(locked); }
            var noisy = Pi.Cp(noise); var four = Snapshot(4); var resources = new Dictionary<int, Pi>();
            for (int i = 0; i < four.Nodes.Count; i++) if (four.Nodes[i].Kind == "img") resources.Add(i, noisy);
            reject(() => Ht.Cp(four, options, null, resources), "aggregate encoded image bound");
        }
        using (var file = new FileStream(Path.Combine(folder, "friendly-image.html"), FileMode.CreateNew))
        using (var writer = new StreamWriter(file)) writer.Write(html);
        return count;
    }
}
