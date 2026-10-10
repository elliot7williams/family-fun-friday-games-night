using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;

namespace Lea.Tests
{
    internal static class Origins
    {
        internal static void Run(Func<Task<P>, P> complete)
        {
            var checks = 0;
            Action<bool, string> check = (ok, name) => {
                if (!ok) throw new InvalidOperationException("FAIL document origin: " + name);
                checks++; Console.WriteLine("PASS document origin: " + name);
            };
            Func<string, Og> of = text => Og.Of(new Uri(text));
            var origin = of("https://EXAMPLE.test:443/a?b#c");
            check(!origin.Op && !origin.Lc && origin.Tx == "https://example.test", "tuple serialization omits default port");
            foreach (var text in new[] { "https://example.test/else", "https://user:secret@example.test:443/?q#part" })
            {
                var other = of(text);
                check(origin.Eq(other) && other.Eq(origin) && origin.Equals(other) && origin.GetHashCode() == other.GetHashCode(), "path query fragment and credentials do not change authority: " + text);
            }
            foreach (var text in new[] { "http://example.test/", "https://example.test:444/", "https://child.example.test/", "https://example.test./", "wss://example.test/" })
                check(!origin.Eq(of(text)), "distinct scheme host or port: " + text);
            foreach (var text in new[] { "ftp://example.test:21/", "http://example.test:80/", "ws://example.test:80/", "wss://example.test:443/" })
            {
                var url = new Uri(text);
                check(of(text).Tx == url.Scheme + "://example.test", "scheme-specific default port: " + url.Scheme);
            }
            check(of("https://example.test:0/").Tx == "https://example.test:0", "explicit zero port stays distinct");
            check(of("https://b\u00fccher.example/").Eq(of("https://xn--bcher-kva.example/")), "IDN host is ASCII canonicalized");
            check(of("http://[2001:0DB8:0:0:0:0:0:1]/").Tx == "http://[2001:db8::1]" && of("http://[2001:db8::1]:80/").Eq(of("http://[2001:0db8::1]/")), "IPv6 canonicalization and bracket serialization");
            check(of("http://[fe80::1%251]/").Op, "zone-qualified IPv6 fails closed");
            check(of("http://127.1/").Eq(of("http://127.0.0.1:80/")), "IPv4 uses structured URI canonicalization");
            foreach (var text in new[] { "file:///C:/example.html", "file://server/share/a.html", "about:blank", "data:text/html,hello", "blob:https://example.test/id", "custom://example.test/" })
            {
                var first = of(text); var second = of(text);
                check(first.Op && second.Op && first.Tx == "null" && first.Eq(first) && !first.Eq(second) && !first.Equals(second), "opaque identity is not the serialized null string: " + text);
            }
            check(of("file:///C:/a.html").Lc && of("about:blank?empty#part").Lc && !of("file://server/share/a.html").Lc && !of("about:not-blank").Lc, "local script policy stays separate from opaque equality");
            check(!origin.Eq(null) && !origin.Equals("https://example.test"), "untyped and null values are not origins");
            var rejected = false;
            try { Og.Of(null); } catch (ArgumentNullException) { rejected = true; }
            check(rejected, "null URL rejected");
            rejected = false;
            try { Og.Of(new Uri("relative", UriKind.Relative)); } catch (ArgumentException) { rejected = true; }
            check(rejected, "relative URL rejected");
            using (var target = new Server())
            using (var entry = new Server(target.U.AbsoluteUri))
            {
                var engine = new E { Jb = false };
                var page = engine.Go(new Uri(entry.U, "/start#requested"));
                var fixedOrigin = page.Og;
                check(page.U.Port == target.U.Port && page.U.Fragment == "#requested" && fixedOrigin.Eq(Og.Of(target.U)) && !fixedOrigin.Eq(Og.Of(entry.U)), "final redirect URL supplies the origin, not the request URL");
                check(page.Bu.Host == "other.invalid" && !fixedOrigin.Eq(Og.Of(page.Bu)), "authored base changes resource resolution, not document origin");
                engine.Sa(page, engine.Id(page, "field"), "class", "edited");
                check(ReferenceEquals(page.Og, fixedOrigin), "DOM/style rebuild retains immutable origin");
                var fragment = engine.Go(new Uri(target.U, "/page#next"));
                check(ReferenceEquals(fragment, page) && ReferenceEquals(fragment.Og, fixedOrigin), "fragment navigation retains document identity");
                check(ReferenceEquals(engine.Bk().Og, fixedOrigin) && ReferenceEquals(engine.Fw().Og, fixedOrigin), "fragment history retains origin identity");
                var reload = engine.Rl();
                check(!ReferenceEquals(reload, page) && !ReferenceEquals(reload.Og, fixedOrigin) && reload.Og.Eq(fixedOrigin), "network reload constructs a new equivalent tuple origin");
                var asyncPage = complete(engine.GoA(new Uri(entry.U, "/start#async")));
                check(asyncPage.U.Port == target.U.Port && asyncPage.Og.Eq(fixedOrigin), "asynchronous redirect origin uses final URL");
            }
            var blankEngine = new E { Jb = false };
            var blank = blankEngine.Go("about:blank");
            var blankOrigin = blank.Og;
            check(ReferenceEquals(blankEngine.Go("about:blank#part").Og, blankOrigin), "standalone blank fragment retains opaque identity");
            check(!blankEngine.Rl().Og.Eq(blankOrigin), "standalone blank reload has a fresh opaque origin");
            var path = Path.Combine(Path.GetTempPath(), "Lea-origin-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                File.WriteAllText(path, "<base href='https://other.invalid/'><input id='field'>");
                var engine = new E { Jb = false };
                var first = engine.Go(path);
                check(first.Og.Op && first.Og.Lc && first.Bu.Scheme == "https", "local document remains local with a remote base");
                var reloaded = engine.Rl();
                check(!reloaded.Og.Eq(first.Og), "same file reload does not reuse opaque authority");
                engine.Go("about:blank");
                check(ReferenceEquals(engine.Bk(), reloaded) && ReferenceEquals(engine.At.Og, reloaded.Og), "history restores the same reloaded file document and opaque identity");
            }
            finally { File.Delete(path); }
            Console.WriteLine("PASS Lea document origins: " + checks + " checks.");
        }

        internal sealed class Server : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly Thread thread;
            private readonly string redirect;
            private volatile bool stopped;
            private Exception error;
            private int scriptHits;
            internal readonly Uri U;
            internal volatile string Markup = "<base href='https://other.invalid/'><title>origin page</title><input id='field'>";
            internal int ScriptHits { get { return Interlocked.CompareExchange(ref scriptHits, 0, 0); } }
            internal Server(string redirect = null)
            {
                this.redirect = redirect;
                listener.Start();
                U = new Uri("http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/page");
                thread = new Thread(Serve) { IsBackground = true }; thread.Start();
            }
            private void Serve()
            {
                try
                {
                    while (!stopped)
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        stream.ReadTimeout = 2000; stream.WriteTimeout = 2000;
                        var header = new StringBuilder(); var tail = 0;
                        while (header.Length < 16384 && tail != 0x0d0a0d0a)
                        {
                            var value = stream.ReadByte(); if (value < 0) throw new IOException("Incomplete origin fixture request");
                            header.Append((char)value); tail = (tail << 8) | value;
                        }
                        if (tail != 0x0d0a0d0a) throw new IOException("Origin fixture header limit");
                        var script = header.ToString().StartsWith("GET /script.js ", StringComparison.Ordinal);
                        if (script) Interlocked.Increment(ref scriptHits);
                        var body = Encoding.UTF8.GetBytes(script ? "document.title='external executed';" : Markup);
                        var response = redirect != null ? "HTTP/1.1 302 Found\r\nLocation: " + redirect + "\r\nContent-Length: 0\r\nConnection: close\r\n\r\n" :
                            "HTTP/1.1 200 OK\r\nContent-Type: " + (script ? "text/javascript" : "text/html") + "; charset=utf-8\r\nCache-Control: no-store\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n";
                        var bytes = Encoding.ASCII.GetBytes(response); stream.Write(bytes, 0, bytes.Length);
                        if (redirect == null) stream.Write(body, 0, body.Length);
                    }
                }
                catch (Exception failure) { if (!stopped) error = failure; }
            }
            public void Dispose()
            {
                stopped = true; listener.Stop();
                if (!thread.Join(3000)) throw new IOException("Origin fixture did not retire");
                if (error != null) throw new IOException("Origin fixture failed", error);
            }
        }
    }
}
