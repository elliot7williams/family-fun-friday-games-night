using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Win;

namespace Lea.Tests
{
    internal static class Smoke
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
                var forms = Path.Combine(root, "LeaForge", "test-pages", "forms.html");
                var engine = new E();
                var page = engine.Go(forms);
                Eq(200, page.St, "local status");
                Eq("text/html", page.Ct, "local content type");
                Eq("utf-8", page.Ch, "local charset");
                Ok(page.Sz > 0 && !page.Ca, "local response metadata");
                Eq(9, page.Fs.Count, "field count");
                Eq("text,password,textarea,select,checkbox,radio,radio,reset,submit", Kinds(page), "field kinds");
                Eq(3, page.Fs[3].Os.Count, "select options");
                Eq("lea", page.Fs[3].V, "selected option value");
                Eq("Lea green", page.Fs[3].Os[1].T, "selected option text");
                Ok(page.Fs[3].Os[2].Ds, "disabled option");
                Ok(page.Fs[4].C && page.Fs[5].C && !page.Fs[6].C, "choice defaults");

                using (var view = new V())
                {
                    view.Size = new Size(720, 760);
                    view.CreateControl();
                    view.Go(forms);
                    Paint(view);
                    Ok(view.Bc > page.Fs.Count, "form layout boxes");
                    Eq("Open result", view.At.Bs[view.At.Bs.Count - 1].X, "button label");

                    view.At.Fs[0].V = "changed";
                    view.At.Fs[3].V = "light";
                    view.At.Fs[3].Si = 0;
                    view.At.Fs[4].C = false;
                    view.Rs(view.At.Fs[7]);
                    Eq("Lea", view.At.Fs[0].V, "reset text");
                    Eq("lea", view.At.Fs[3].V, "reset select");
                    Eq(1, view.At.Fs[3].Si, "reset selected index");
                    Ok(view.At.Fs[4].C, "reset checkbox");

                    view.At.Fs[0].V = "small engine";
                    view.Sm(view.At.Fs[8]);
                    Eq("Lea Form Result", view.T, "GET result");
                    Ok(view.Q.Contains("q=small+engine") && view.Q.Contains("theme=lea") && view.Q.Contains("updates=yes") && view.Q.Contains("size=tiny") && view.Q.Contains("mode=lea"), "GET values: " + view.Q + " uri=" + view.U.AbsoluteUri + " local=" + view.U.LocalPath);

                    view.Go(forms);
                    view.At.Fs[0].V = "async engine";
                    var submit = view.SmA(view.At.Fs[8]);
                    while (!submit.IsCompleted)
                    {
                        Application.DoEvents();
                        Thread.Sleep(2);
                    }
                    submit.GetAwaiter().GetResult();
                    Eq("Lea Form Result", view.T, "async GET result");
                    Ok(view.Q.Contains("q=async+engine"), "async GET values");

                    Check(view, root, "css.html", 3, 4);
                    Check(view, root, "images.html", 5, 6);
                    Check(view, root, "layout.html", 3, 4);
                    var boxes = Path.Combine(root, "LeaForge", "test-pages", "boxes.html");
                    var boxPage = engine.Go(boxes);
                    Eq(4, boxPage.Bs.Count, "box-model blocks");
                    Eq(280, boxPage.Bs[1].S.W, "CSS width");
                    Eq(4, boxPage.Bs[1].S.Bt, "border shorthand width");
                    Eq("solid", boxPage.Bs[1].S.Bs, "border shorthand style");
                    Eq("#18734b", boxPage.Bs[1].S.Bc, "border shorthand color");
                    Eq(360, boxPage.Bs[2].S.W, "fixed width");
                    Eq(54, boxPage.Bs[2].S.H, "fixed height");
                    Eq(5, boxPage.Bs[3].S.Bt, "top border width");
                    Eq(2, boxPage.Bs[3].S.Br, "right border width");
                    Eq(3, boxPage.Bs[3].S.Bb, "bottom border width");
                    Eq(1, boxPage.Bs[3].S.Bl, "left border width");
                    view.Go(boxes);
                    Paint(view);
                    Eq(5, view.Bc, "box-model boxes");
                    Eq(280, view.Rc(2).Width, "solid content width");
                    Eq(308, view.Rb(2).Width, "solid border-box width");
                    Eq(54, view.Rc(3).Height, "fixed content height");
                    Eq(76, view.Rb(3).Height, "fixed border-box height");
                    Eq(320, view.Rc(4).Width, "sided content width");
                    Eq(347, view.Rb(4).Width, "sided border-box width");
                    view.Go(Path.Combine(root, "LeaForge", "test-pages", "scroll.html"));
                    Paint(view);
                    Eq(16, view.Bc, "scroll boxes");
                    view.Sc(100000);
                    Eq(Math.Max(0, view.Dh - view.Height), view.Sy, "bounded scroll");
                }

                using (var server = new CompressedServer())
                {
                    var net = new E();
                    net.Cm = 2;
                    var gzip = new Uri(server.U, "?encoding=gzip");
                    var first = net.Go(gzip);
                    Eq(200, first.St, "HTTP status");
                    Eq("text/html", first.Ct, "HTTP content type");
                    Eq("gzip", first.Ce, "GZip response encoding");
                    Eq("utf-8", first.Ch, "HTTP charset");
                    Eq("Lea gzip", first.T, "GZip decoded page");
                    Ok(first.Sz > 0 && !first.Ca, "network response metadata");
                    Eq(1, server.Hits, "first network request");

                    var cached = net.Go(gzip);
                    Ok(cached.Ca && !Object.ReferenceEquals(first, cached), "cached payload creates a fresh page");
                    Eq(1, server.Hits, "cache avoids network");
                    Eq(1, net.Ch, "cache hit count");

                    var deflate = net.Go(new Uri(server.U, "?encoding=deflate"));
                    Eq("deflate", deflate.Ce, "Deflate response encoding");
                    Eq("Lea deflate", deflate.T, "Deflate decoded page");
                    net.Go(new Uri(server.U, "?page=third"));
                    Eq(2, net.Cc, "bounded cache count");
                    var evicted = net.Go(gzip);
                    Ok(!evicted.Ca, "least-recently-used eviction");
                    Eq(4, server.Hits, "evicted page reload");

                    var reloaded = net.Rl();
                    Ok(!reloaded.Ca, "reload bypasses cache");
                    Eq(5, server.Hits, "reload network request");
                    net.Cl();
                    Eq(0, net.Cc, "clear cache");

                    var asyncPage = net.GoA(new Uri(server.U, "?encoding=deflate&async=1")).GetAwaiter().GetResult();
                    Eq("deflate", asyncPage.Ce, "async response encoding");
                    Eq("Lea deflate", asyncPage.T, "async decoded page");
                    var noStoreUrl = new Uri(server.U, "?no-store=1");
                    net.Go(noStoreUrl);
                    var noStore = net.Go(noStoreUrl);
                    Ok(!noStore.Ca, "no-store response bypasses cache");
                    Eq(8, server.Hits, "no-store network requests");
                    net.Cl();
                    var redirected = net.Go(new Uri(server.U, "?redirect=1"));
                    Eq(200, redirected.St, "redirect final status");
                    Ok(redirected.U.Query.Contains("redirected=1"), "redirect final URL");
                    Eq(10, server.Hits, "redirect request chain");

                    using (var postView = new V())
                    {
                        postView.Size = new Size(640, 520);
                        postView.CreateControl();
                        var formUrl = new Uri(server.U, "?form=1");
                        postView.Go(formUrl);
                        Paint(postView);
                        Eq("select,text,checkbox,checkbox,submit", Kinds(postView.At), "POST form fields");
                        Eq("mint", postView.At.Fs[0].V, "POST select default");
                        Eq(3, postView.At.Fs[0].Os.Count, "POST select options");
                        postView.At.Fs[0].V = "vanilla";
                        postView.At.Fs[0].Si = 0;
                        postView.At.Fs[1].V = "tiny engine";
                        var posted = postView.Sm(postView.At.Fs[4]);
                        Eq("Lea POST", posted.T, "POST result");
                        Eq("gzip", posted.Ce, "compressed POST response");
                        Eq("POST", server.LastMethod, "POST method");
                        Ok(server.LastType.StartsWith("application/x-www-form-urlencoded"), "POST content type: " + server.LastType);
                        Eq("flavor=vanilla&q=tiny+engine&news=yes&send=go", server.LastBody, "POST body");

                        postView.Go(formUrl);
                        postView.At.Fs[1].V = "async engine";
                        var postTask = postView.SmA(postView.At.Fs[4]);
                        while (!postTask.IsCompleted)
                        {
                            Application.DoEvents();
                            Thread.Sleep(2);
                        }
                        var asyncPost = postTask.GetAwaiter().GetResult();
                        Eq("Lea POST", asyncPost.T, "async POST result");
                        Ok(server.LastBody.Contains("flavor=mint") && server.LastBody.Contains("q=async+engine"), "async POST body: " + server.LastBody);
                    }
                    Eq(13, server.Hits, "form and POST request count");
                }

                Console.WriteLine("Lea smoke passed: selects, POST, network cache, compression, forms, CSS boxes, images, layout, and scrolling.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Lea smoke failed: " + error);
                return 1;
            }
        }

        private static void Paint(V view)
        {
            using (var image = new Bitmap(view.Width, view.Height))
                view.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
        }

        private static void Check(V view, string root, string file, int blocks, int boxes)
        {
            view.Go(Path.Combine(root, "LeaForge", "test-pages", file));
            Paint(view);
            Eq(blocks, view.At.Bs.Count, file + " blocks");
            Eq(boxes, view.Bc, file + " boxes");
        }

        private static string Kinds(P page)
        {
            var values = new string[page.Fs.Count];
            for (var i = 0; i < values.Length; i++) values[i] = page.Fs[i].K;
            return String.Join(",", values);
        }

        private static void Eq<T>(T expected, T actual, string name)
        {
            if (!Object.Equals(expected, actual)) throw new InvalidOperationException(name + ": expected " + expected + ", got " + actual);
        }

        private static void Ok(bool value, string name)
        {
            if (!value) throw new InvalidOperationException(name);
        }

        private sealed class CompressedServer : IDisposable
        {
            private readonly TcpListener listener;
            private readonly Thread thread;
            private readonly object state = new object();
            private int hits;
            private volatile bool stopping;
            private string lastMethod = "";
            private string lastBody = "";
            private string lastType = "";

            public Uri U { get; private set; }
            public int Hits { get { return Interlocked.CompareExchange(ref hits, 0, 0); } }
            public string LastMethod { get { lock (state) return lastMethod; } }
            public string LastBody { get { lock (state) return lastBody; } }
            public string LastType { get { lock (state) return lastType; } }

            public CompressedServer()
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                U = new Uri("http://127.0.0.1:" + port + "/page");
                thread = new Thread(Serve);
                thread.IsBackground = true;
                thread.Start();
            }

            private void Serve()
            {
                while (!stopping)
                {
                    TcpClient client;
                    try { client = listener.AcceptTcpClient(); }
                    catch (SocketException) { if (stopping) return; throw; }
                    catch (ObjectDisposedException) { return; }
                    using (client)
                    using (var stream = client.GetStream())
                    {
                        string request;
                        var contentLength = 0;
                        var contentType = "";
                        var requestBody = "";
                        using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true))
                        {
                            request = reader.ReadLine() ?? "";
                            string line;
                            while ((line = reader.ReadLine()) != null && line.Length > 0)
                            {
                                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) Int32.TryParse(line.Substring(15).Trim(), out contentLength);
                                else if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase)) contentType = line.Substring(13).Trim();
                            }
                            if (contentLength > 0)
                            {
                                var bodyChars = new char[contentLength];
                                var read = 0;
                                while (read < bodyChars.Length)
                                {
                                    var count = reader.Read(bodyChars, read, bodyChars.Length - read);
                                    if (count < 1) break;
                                    read += count;
                                }
                                requestBody = new String(bodyChars, 0, read);
                            }
                        }

                        Interlocked.Increment(ref hits);
                        var methodEnd = request.IndexOf(' ');
                        lock (state)
                        {
                            lastMethod = methodEnd < 0 ? request : request.Substring(0, methodEnd);
                            lastBody = requestBody;
                            lastType = contentType;
                        }
                        if (request.IndexOf("redirect=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var redirect = Encoding.ASCII.GetBytes("HTTP/1.1 302 Found\r\nLocation: /page?encoding=gzip&redirected=1\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                            stream.Write(redirect, 0, redirect.Length);
                            continue;
                        }
                        var deflate = request.IndexOf("encoding=deflate", StringComparison.OrdinalIgnoreCase) >= 0;
                        string html;
                        if (request.IndexOf("form=1", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            html = "<html><head><title>Lea POST Form</title></head><body><form action='/page?post=1' method='post'>" +
                                "<select name='flavor'><option value='vanilla'>Vanilla</option><option value='mint' selected>Mint</option><option value='old' disabled>Old</option></select>" +
                                "<input name='q' value='lea'><input type='checkbox' name='news' value='yes' checked><input type='checkbox' name='skip' value='no'>" +
                                "<button type='submit' name='send' value='go'>Send</button></form></body></html>";
                        }
                        else if (request.StartsWith("POST ", StringComparison.OrdinalIgnoreCase))
                            html = "<html><head><title>Lea POST</title></head><body><p>POST navigation worked.</p></body></html>";
                        else
                        {
                            var title = deflate ? "Lea deflate" : "Lea gzip";
                            html = "<html><head><title>" + title + "</title></head><body><p>Compressed Lea response.</p></body></html>";
                        }
                        var source = Encoding.UTF8.GetBytes(html);
                        var body = Compress(source, deflate);
                        var noStore = request.IndexOf("no-store=1", StringComparison.OrdinalIgnoreCase) >= 0;
                        var headers = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Encoding: " + (deflate ? "deflate" : "gzip") + "\r\n" +
                            (noStore ? "Cache-Control: no-store\r\n" : "") + "Content-Length: " + body.Length + "\r\nConnection: close\r\n\r\n";
                        var head = Encoding.ASCII.GetBytes(headers);
                        stream.Write(head, 0, head.Length);
                        stream.Write(body, 0, body.Length);
                    }
                }
            }

            private static byte[] Compress(byte[] source, bool deflate)
            {
                using (var output = new MemoryStream())
                {
                    using (Stream zip = deflate ? (Stream)new DeflateStream(output, CompressionMode.Compress, true) : new GZipStream(output, CompressionMode.Compress, true))
                        zip.Write(source, 0, source.Length);
                    return output.ToArray();
                }
            }

            public void Dispose()
            {
                stopping = true;
                listener.Stop();
                thread.Join(2000);
            }
        }
    }
}
