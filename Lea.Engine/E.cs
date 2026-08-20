using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lea.Engine
{
    // Lea's .NET Framework-compatible engine surface.
    public sealed class E
    {
        private const int MaxCachePage = 2 * 1024 * 1024;
        private const int MaxCss = 256 * 1024;
        private const int MaxStyleLinks = 8;
        private const int MaxPost = 10 * 1024 * 1024;
        private const long MaxDownload = 64L * 1024 * 1024;
        private const int MaxFind = 256;
        private const int MaxDiagnosticNodes = 512;
        private const int MaxDiagnosticRules = 256;
        private const int MaxDiagnosticRequests = 32;
        private const int MaxHtmlTokens = 32768;
        private const int MaxHtmlNodes = 8192;
        private const int MaxHtmlDepth = 128;
        private const int MaxHtmlAttributes = 128;
        private const int MaxHtmlName = 64;
        private const int MaxHtmlAttributeText = 64 * 1024;
        private const int MaxHtmlAttributeValue = 16 * 1024;
        private const int MaxBaseUrl = 4096;
        private const int MaxQueryLength = 1024;
        private const int MaxQueryGroups = 16;
        private const int MaxQueryParts = 16;
        private const int MaxQueryResults = 256;
        private readonly W web = new W();
        private readonly List<P> hs = new List<P>();
        private readonly Dictionary<string, Cr> cache = new Dictionary<string, Cr>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> lru = new List<string>();
        private readonly List<string> requests = new List<string>();
        private int at = -1;
        private bool busy;
        private int cm = 8;
        private int cs = 60;
        private int ch;
        private bool je = true;
        private Stream download;
        private bool stopped;

        public P At { get { return at >= 0 && at < hs.Count ? hs[at] : null; } }
        public bool CanBk { get { return at > 0; } }
        public bool CanFw { get { return at < hs.Count - 1; } }
        public bool Busy { get { return busy; } }
        public int Cm
        {
            get { return cm; }
            set { cm = Math.Max(0, Math.Min(16, value)); TrimCache(); }
        }
        public int Cc { get { return cache.Count; } }
        public int Ch { get { return ch; } }
        public int Kc { get { return web.Kc; } }
        public int Km { get { return W.CookieMax; } }
        public long Dm { get { return MaxDownload; } }
        public bool Je { get { return je; } set { je = value; } }
        public bool Pv
        {
            get { return web.Pv; }
            set
            {
                if (web.Pv == value) return;
                web.Pv = value;
                Cl();
            }
        }
        public int Cs
        {
            get { return cs; }
            set { cs = Math.Max(0, Math.Min(86400, value)); Cl(); }
        }

        public void Cl()
        {
            cache.Clear();
            lru.Clear();
        }

        public void Kcl()
        {
            web.Kcl();
            Cl();
        }

        // Dy reapplies compiled CSS for compact dynamic selector state.
        public P Dy(P page, N hover, N focus)
        {
            if (page == null) throw new ArgumentNullException("page");
            var rules = page.R as List<R>;
            if (rules == null) return page;
            if (!Owns(page.D, hover)) hover = null;
            if (!Owns(page.D, focus)) focus = null;
            if (Object.ReferenceEquals(page.Hv, hover) && Object.ReferenceEquals(page.Fc, focus)) return page;

            ClearState(page.D);
            for (var node = hover; node != null; node = node.Up) node.Hv = true;
            if (focus != null) focus.Fc = true;
            page.Hv = hover;
            page.Fc = focus;

            var fields = new Dictionary<N, Fi>();
            foreach (var field in page.Fs) if (field.El != null) fields[field.El] = field;
            Rebuild(page, rules, fields);
            return page;
        }

        // Js runs only Lea's bounded allow-listed page commands.
        public int Js(P page, string code)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!hs.Contains(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            if (!je) return 0;
            var rules = page.R as List<R>;
            if (rules == null) return 0;
            var count = J.Run(page, code);
            if (count == 0) return 0;
            page.Jc = (int)Math.Min(Int32.MaxValue, (long)page.Jc + count);
            var fields = new Dictionary<N, Fi>();
            foreach (var field in page.Fs) if (field.El != null) fields[field.El] = field;
            Rebuild(page, rules, fields);
            return count;
        }

        // Ck runs a page element's safe Lea click commands.
        public int Ck(P page, N element)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!hs.Contains(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            if (element == null || !Owns(page.D, element)) return 0;
            var code = element.Av("data-lea");
            if (String.IsNullOrWhiteSpace(code)) code = element.Av("onclick");
            return String.IsNullOrWhiteSpace(code) ? 0 : Js(page, code);
        }

        // Dg captures bounded page and engine diagnostics for developer tools.
        public G Dg(P page)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!hs.Contains(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            var rules = page.R as List<R>;
            var result = new G
            {
                U = page.U,
                T = page.T ?? "",
                St = page.St,
                Ct = page.Ct ?? "",
                Sz = page.Sz,
                Ca = page.Ca,
                N = page.N,
                B = page.Bs.Count,
                L = page.Ls.Count,
                F = page.Fs.Count,
                C = rules == null ? 0 : rules.Count,
                J = page.Jc,
                V = page.Vs.Count,
                M = page.Ms.Count,
                Cc = Cc,
                Ch = Ch,
                Kc = Kc
            };
            DiagnosticNodes(page.D, 0, result.Ns);
            if (rules != null)
                foreach (var rule in rules)
                {
                    if (result.Cs.Count == MaxDiagnosticRules) break;
                    result.Cs.Add(Short(rule.Q, 96) + " { " + Short(rule.V, 144) + " } [" + rule.W + "]");
                }
            lock (requests)
                foreach (var request in requests) result.Rs.Add(request);
            result.Ms.Add("commands " + page.Jc);
            foreach (var message in page.Ms) result.Ms.Add("alert " + Short(message, 180));
            var names = new List<string>(page.Vs.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names) result.Vs.Add(name + " = " + Short(page.Vs[name], 180));
            return result;
        }

        public void Dcl()
        {
            lock (requests) requests.Clear();
        }

        // Id finds the first element with a matching HTML id.
        public N Id(P page, string id)
        {
            QueryPage(page);
            var value = id ?? "";
            return value.Length == 0 || value.Length > MaxHtmlAttributeValue ? null : FindId(page.D, value);
        }

        // Q returns the first document-order match from Lea's bounded selector subset.
        public N Q(P page, string query)
        {
            QueryPage(page);
            var rules = QueryRules(query);
            return rules == null ? null : QueryOne(page.D, rules);
        }

        // Qa returns at most 256 document-order matches in a read-only list.
        public IList<N> Qa(P page, string query)
        {
            QueryPage(page);
            var found = new List<N>();
            var rules = QueryRules(query);
            if (rules != null) QueryAll(page.D, rules, found);
            return found.AsReadOnly();
        }

        private void QueryPage(P page)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!hs.Contains(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
        }

        // Fd counts bounded, case-insensitive page-text matches.
        public int Fd(P page, string query)
        {
            if (page == null) throw new ArgumentNullException("page");
            var find = (query ?? "").Trim();
            if (find.Length == 0) return 0;
            if (find.Length > 256) find = find.Substring(0, 256);
            var text = page.X ?? "";
            var count = 0;
            var at = 0;
            while (at <= text.Length - find.Length && count < MaxFind)
            {
                at = text.IndexOf(find, at, StringComparison.OrdinalIgnoreCase);
                if (at < 0) break;
                count++;
                at += find.Length;
            }
            return count;
        }

        public P Go(string input)
        {
            return Go(ToUri(input, At == null ? null : At.U));
        }

        public P Go(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = Read(url);
            Push(p);
            return p;
        }

        public P Po(string input, string data)
        {
            return Po(ToUri(input, At == null ? null : At.U), data);
        }

        public P Po(Uri url, string data)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = Post(url, Encoding.UTF8.GetBytes(data ?? ""), "application/x-www-form-urlencoded; charset=utf-8");
            Push(p);
            return p;
        }

        public P Pb(string input, byte[] data, string type)
        {
            return Pb(ToUri(input, At == null ? null : At.U), data, type);
        }

        public P Pb(Uri url, byte[] data, string type)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = Post(url, data, type);
            Push(p);
            return p;
        }

        public async Task<P> GoA(string input)
        {
            return await GoA(ToUri(input, At == null ? null : At.U));
        }

        public async Task<P> GoA(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = await ReadA(url);
            Push(p);
            return p;
        }

        public async Task<P> PoA(string input, string data)
        {
            return await PoA(ToUri(input, At == null ? null : At.U), data);
        }

        public async Task<P> PoA(Uri url, string data)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = await PostA(url, Encoding.UTF8.GetBytes(data ?? ""), "application/x-www-form-urlencoded; charset=utf-8");
            Push(p);
            return p;
        }

        public async Task<P> PbA(string input, byte[] data, string type)
        {
            return await PbA(ToUri(input, At == null ? null : At.U), data, type);
        }

        public async Task<P> PbA(Uri url, byte[] data, string type)
        {
            if (url == null) throw new ArgumentNullException("url");
            var p = await PostA(url, data, type);
            Push(p);
            return p;
        }

        public D Dl(string input, string path)
        {
            return Dl(ToUri(input, At == null ? null : At.U), path);
        }

        public D Dl(Uri url, string path)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (busy) throw new InvalidOperationException("Lea is already loading or downloading.");
            var target = DownloadPath(path);
            CheckDownloadUrl(url);
            busy = true;
            stopped = false;
            try { return Download(url, target); }
            catch (Exception error)
            {
                if (stopped) throw new Er("cancel", url, 0, false, "Lea stopped downloading" + ErrorAt(url) + ".", error);
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally
            {
                download = null;
                stopped = false;
                busy = false;
            }
        }

        public async Task<D> DlA(string input, string path)
        {
            return await DlA(ToUri(input, At == null ? null : At.U), path);
        }

        public async Task<D> DlA(Uri url, string path)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (busy) throw new InvalidOperationException("Lea is already loading or downloading.");
            var target = DownloadPath(path);
            CheckDownloadUrl(url);
            busy = true;
            stopped = false;
            try { return await DownloadA(url, target); }
            catch (Exception error)
            {
                if (stopped) throw new Er("cancel", url, 0, false, "Lea stopped downloading" + ErrorAt(url) + ".", error);
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally
            {
                download = null;
                stopped = false;
                busy = false;
            }
        }

        private void Push(P p)
        {
            if (at < hs.Count - 1) hs.RemoveRange(at + 1, hs.Count - at - 1);
            hs.Add(p);
            at = hs.Count - 1;
        }

        public P Bk()
        {
            if (CanBk) at--;
            return At;
        }

        public P Fw()
        {
            if (CanFw) at++;
            return At;
        }

        public P Rl()
        {
            if (At == null) return null;
            var p = Read(At.U, true);
            hs[at] = p;
            return p;
        }

        public async Task<P> RlA()
        {
            if (At == null) return null;
            var p = await ReadA(At.U, true);
            hs[at] = p;
            return p;
        }

        public void St()
        {
            if (!busy) return;
            stopped = true;
            web.CancelAsync();
            var active = download;
            if (active != null)
                try { active.Dispose(); }
                catch { }
        }

        private D Download(Uri url, string path)
        {
            var temp = DownloadTemp(path);
            try
            {
                long size;
                Uri finalUrl;
                string contentType;
                int status;
                using (var raw = OpenDownload(url))
                {
                    download = raw;
                    CheckDownloadLength(url, url.IsFile ? raw.Length : web.Len);
                    finalUrl = url.IsFile ? url : web.U ?? url;
                    contentType = url.IsFile ? FileType(Local(url)) : MediaType(web.Ct);
                    status = url.IsFile ? 200 : web.St == 0 ? 200 : web.St;
                    using (var input = DownloadStream(raw, url.IsFile ? "" : web.Ce))
                    using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 32768))
                        size = CopyDownload(input, output, url);
                }
                FinishDownload(temp, path);
                var result = DownloadResult(finalUrl, path, contentType, size, status);
                Trace("DL", result.U, result.St, result.Ct, result.Sz, result.U.IsFile ? "file" : "net");
                return result;
            }
            catch
            {
                DeleteTemp(temp);
                throw;
            }
            finally { download = null; }
        }

        private async Task<D> DownloadA(Uri url, string path)
        {
            var temp = DownloadTemp(path);
            try
            {
                var raw = url.IsFile
                    ? (Stream)new FileStream(Local(url), FileMode.Open, FileAccess.Read, FileShare.Read, 32768, true)
                    : await web.OpenReadTaskAsync(url);
                long size;
                Uri finalUrl;
                string contentType;
                int status;
                using (raw)
                {
                    download = raw;
                    CheckDownloadLength(url, url.IsFile ? raw.Length : web.Len);
                    finalUrl = url.IsFile ? url : web.U ?? url;
                    contentType = url.IsFile ? FileType(Local(url)) : MediaType(web.Ct);
                    status = url.IsFile ? 200 : web.St == 0 ? 200 : web.St;
                    using (var input = DownloadStream(raw, url.IsFile ? "" : web.Ce))
                    using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 32768, true))
                        size = await CopyDownloadA(input, output, url);
                }
                FinishDownload(temp, path);
                var result = DownloadResult(finalUrl, path, contentType, size, status);
                Trace("DL", result.U, result.St, result.Ct, result.Sz, result.U.IsFile ? "file" : "net");
                return result;
            }
            catch
            {
                DeleteTemp(temp);
                throw;
            }
            finally { download = null; }
        }

        private Stream OpenDownload(Uri url)
        {
            return url.IsFile
                ? (Stream)new FileStream(Local(url), FileMode.Open, FileAccess.Read, FileShare.Read)
                : web.OpenRead(url);
        }

        private static Stream DownloadStream(Stream source, string encoding)
        {
            var value = (encoding ?? "").Trim().ToLowerInvariant();
            if (value == "gzip") return new GZipStream(source, CompressionMode.Decompress);
            if (value == "deflate") return new DeflateStream(source, CompressionMode.Decompress);
            return source;
        }

        private static long CopyDownload(Stream input, Stream output, Uri url)
        {
            var buffer = new byte[32768];
            long total = 0;
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += read;
                CheckDownloadLength(url, total);
                output.Write(buffer, 0, read);
            }
            return total;
        }

        private static async Task<long> CopyDownloadA(Stream input, Stream output, Uri url)
        {
            var buffer = new byte[32768];
            long total = 0;
            int read;
            while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                total += read;
                CheckDownloadLength(url, total);
                await output.WriteAsync(buffer, 0, read);
            }
            return total;
        }

        private static void CheckDownloadLength(Uri url, long length)
        {
            if (length > MaxDownload)
                throw new Er("large", url, 0, false, "Lea downloads are limited to 64 MB" + ErrorAt(url) + ".", null);
        }

        private static void CheckDownloadUrl(Uri url)
        {
            if (!url.IsFile && url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                throw new Er("url", url, 0, false, "Lea downloads require a local, HTTP, or HTTPS address.", null);
        }

        private static string DownloadPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException("Lea requires a download destination.", "path");
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full)) throw new ArgumentException("Lea requires a file destination.", "path");
            var folder = Path.GetDirectoryName(full);
            if (String.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) throw new DirectoryNotFoundException("Lea could not find the download destination folder.");
            return full;
        }

        private static string DownloadTemp(string path)
        {
            return Path.Combine(Path.GetDirectoryName(path), ".lea-" + Guid.NewGuid().ToString("N") + ".tmp");
        }

        private static void FinishDownload(string temp, string path)
        {
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }

        private static void DeleteTemp(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }

        private static D DownloadResult(Uri url, string path, string contentType, long size, int status)
        {
            return new D
            {
                U = url,
                P = path,
                N = Path.GetFileName(path),
                Ct = contentType ?? "",
                Sz = size,
                St = status
            };
        }

        private P Read(Uri url, bool fresh = false)
        {
            try
            {
                return Parse(Fetch(url, fresh));
            }
            catch (Exception error)
            {
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
        }

        private async Task<P> ReadA(Uri url, bool fresh = false)
        {
            if (busy) throw new InvalidOperationException("Lea is already loading a page.");
            busy = true;
            try
            {
                return await ParseA(await FetchA(url, fresh));
            }
            catch (Exception error)
            {
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally
            {
                busy = false;
            }
        }

        private P Post(Uri url, byte[] data, string type)
        {
            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                throw new Er("url", url, 0, false, "Lea POST requires an HTTP or HTTPS address.", null);
            if (data != null && data.Length > MaxPost) throw new ArgumentException("Lea request body is too large.", "data");
            try
            {
                DropCache(url);
                web.Headers[HttpRequestHeader.ContentType] = BodyType(type);
                var bytes = web.UploadData(url, "POST", data ?? new byte[0]);
                var reply = ReadWeb(url, bytes);
                DropCache(reply.U);
                var load = Track("POST", new Ld(reply, false), "net");
                return Parse(load);
            }
            catch (Exception error)
            {
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally { web.Headers.Remove(HttpRequestHeader.ContentType); }
        }

        private async Task<P> PostA(Uri url, byte[] data, string type)
        {
            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
                throw new Er("url", url, 0, false, "Lea POST requires an HTTP or HTTPS address.", null);
            if (data != null && data.Length > MaxPost) throw new ArgumentException("Lea request body is too large.", "data");
            if (busy) throw new InvalidOperationException("Lea is already loading a page.");
            busy = true;
            try
            {
                DropCache(url);
                web.Headers[HttpRequestHeader.ContentType] = BodyType(type);
                var bytes = await web.UploadDataTaskAsync(url, "POST", data ?? new byte[0]);
                var reply = ReadWeb(url, bytes);
                DropCache(reply.U);
                var load = Track("POST", new Ld(reply, false), "net");
                return await ParseA(load);
            }
            catch (Exception error)
            {
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally
            {
                busy = false;
                web.Headers.Remove(HttpRequestHeader.ContentType);
            }
        }

        private static string BodyType(string type)
        {
            var value = (type ?? "").Trim();
            if (value.Length == 0) return "application/octet-stream";
            if (value.Length > 200 || value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0)
                throw new ArgumentException("Lea request content type is invalid.", "type");
            return value;
        }

        private Ld Fetch(Uri url, bool fresh = false)
        {
            Cr reply;
            if (!url.IsFile && !fresh && GetCache(url, out reply)) return Track("GET", new Ld(reply, true), "cache");
            if (url.IsFile) return Track("GET", new Ld(ReadFile(url), false), "file");

            Cr stale;
            if (fresh || !GetStale(url, out stale)) stale = null;
            web.Va(stale == null ? "" : stale.Et, stale == null ? "" : stale.Lm);
            byte[] bytes;
            try
            {
                try { bytes = web.DownloadData(url); }
                catch (WebException error)
                {
                    if (stale == null || !web.Nm(error)) throw;
                    bytes = new byte[0];
                }
                var notModified = web.St == 304 && stale != null;
                reply = notModified ? Revalidated(stale) : ReadWeb(url, bytes);
                PutCache(url, reply);
                if (notModified) ch++;
                return Track("GET", new Ld(reply, notModified), notModified ? "304" : "net");
            }
            finally { web.Va("", ""); }
        }

        private async Task<Ld> FetchA(Uri url, bool fresh = false)
        {
            Cr reply;
            if (!url.IsFile && !fresh && GetCache(url, out reply)) return Track("GET", new Ld(reply, true), "cache");
            if (url.IsFile) return Track("GET", new Ld(await Task.Factory.StartNew(delegate { return ReadFile(url); }), false), "file");

            Cr stale;
            if (fresh || !GetStale(url, out stale)) stale = null;
            web.Va(stale == null ? "" : stale.Et, stale == null ? "" : stale.Lm);
            byte[] bytes;
            try
            {
                try { bytes = await web.DownloadDataTaskAsync(url); }
                catch (WebException error)
                {
                    if (stale == null || !web.Nm(error)) throw;
                    bytes = new byte[0];
                }
                var notModified = web.St == 304 && stale != null;
                reply = notModified ? Revalidated(stale) : ReadWeb(url, bytes);
                PutCache(url, reply);
                if (notModified) ch++;
                return Track("GET", new Ld(reply, notModified), notModified ? "304" : "net");
            }
            finally { web.Va("", ""); }
        }

        private P Parse(Ld load)
        {
            var page = Page(load);
            Complete(page, Css(page.D, StyleSheets(page.D, page.Bu)));
            return page;
        }

        private async Task<P> ParseA(Ld load)
        {
            var page = Page(load);
            Complete(page, Css(page.D, await StyleSheetsA(page.D, page.Bu)));
            return page;
        }

        private static P Page(Ld load)
        {
            var reply = load.R;
            var p = new P();
            p.U = reply.U;
            p.Q = Query(reply.U);
            p.H = reply.H;
            p.St = reply.St;
            p.Ct = reply.Ct;
            p.Ce = reply.Ce;
            p.Ch = reply.Ch;
            p.Et = reply.Et;
            p.Lm = reply.Lm;
            p.Sz = reply.Sz;
            p.Ca = load.Ca;
            p.D = Tree(reply.H);
            p.Bu = DocumentBase(p.D, reply.U);
            var title = Find(p.D, "title");
            p.T = Clean(NodeText(title));
            if (p.T.Length == 0) p.T = reply.U.IsFile ? Path.GetFileName(reply.U.LocalPath) : reply.U.Host;

            return p;
        }

        private void Complete(P p, List<R> rules)
        {
            p.R = rules;
            if (je) p.Jc = J.Pg(p, p.D);
            Rebuild(p, rules, null);
        }

        private static void Rebuild(P p, List<R> rules, IDictionary<N, Fi> fields)
        {
            p.Ls.Clear();
            p.Bs.Clear();
            p.Fs.Clear();
            p.Fr = fields;
            var body = Find(p.D, "body") ?? p.D;
            p.X = Clean(NodeText(body));
            p.N = Count(p.D);
            var inherited = new S();
            var ancestors = new List<N>();
            for (var parent = body.Up; parent != null && parent.K != "#doc"; parent = parent.Up) ancestors.Add(parent);
            for (var i = ancestors.Count - 1; i >= 0; i--) inherited = Style(ancestors[i], inherited, rules);
            p.S = Style(body, inherited, rules);
            foreach (var child in body.C) Blocks(p, child, p.Bu ?? p.U, rules, p.S);
            LinkBlocks(p);
            p.Fr = null;
        }

        private static void LinkBlocks(P page)
        {
            var contexts = new Dictionary<N, B>();
            foreach (var block in page.Bs)
                if (block.El != null && BlockContext(block) && !contexts.ContainsKey(block.El)) contexts[block.El] = block;
            foreach (var block in page.Bs)
            {
                for (var node = block.El == null ? null : block.El.Up; node != null; node = node.Up)
                {
                    B parent;
                    if (!contexts.TryGetValue(node, out parent)) continue;
                    block.Up = parent;
                    break;
                }
            }
        }

        private static bool LayoutContext(S style)
        {
            return style != null && (style.Pn == "r" || style.Pn == "a" || style.Pn == "f" || style.Zc || (style.Ov.Length > 0 && style.Ov != "v"));
        }

        private static bool BlockContext(B block)
        {
            return block != null && (LayoutContext(block.S) || IsContainerBlock(block.K));
        }

        private static bool VisualContext(S style)
        {
            return LayoutContext(style) || style != null &&
                (style.Bg.Length > 0 || style.Mt != 0 || style.Mr != 0 || style.Mb != 0 || style.Ml != 0 ||
                 style.Pt != 0 || style.Pr != 0 || style.Pb != 0 || style.Pl != 0 || style.W > 0 || style.H > 0 ||
                 style.Wn > 0 || style.Wx > 0 || style.Hn > 0 || style.Hx > 0 ||
                 style.Bt > 0 || style.Br > 0 || style.Bb > 0 || style.Bl > 0);
        }

        private static bool Owns(N root, N node)
        {
            if (node == null) return true;
            for (var current = node; current != null; current = current.Up)
                if (Object.ReferenceEquals(current, root)) return true;
            return false;
        }

        private static void ClearState(N node)
        {
            if (node == null) return;
            node.Hv = false;
            node.Fc = false;
            foreach (var child in node.C) ClearState(child);
        }

        private static N Tree(string html)
        {
            var root = new N("#doc");
            var stack = new List<N>();
            stack.Add(root);
            var nodes = 1;

            foreach (var token in Tokens(html))
            {
                if (token.K == 'c')
                {
                    for (var i = stack.Count - 1; i > 0; i--)
                    {
                        if (stack[i].K != token.N) continue;
                        stack.RemoveRange(i, stack.Count - i);
                        break;
                    }
                    continue;
                }

                if (token.K == 'o')
                {
                    if (nodes == MaxHtmlNodes) break;
                    Recover(stack, token.N);
                    var node = new N(token.N);
                    Attrs(node, token.A);
                    node.Up = stack[stack.Count - 1];
                    stack[stack.Count - 1].C.Add(node);
                    nodes++;
                    if (!token.S && !Void(token.N) && stack.Count < MaxHtmlDepth) stack.Add(node);
                    continue;
                }

                if (token.X.Length == 0) continue;
                if (nodes == MaxHtmlNodes) break;
                var parent = stack[stack.Count - 1];
                parent.C.Add(new N("#text") { X = token.D ? WebUtility.HtmlDecode(token.X) : token.X, Up = parent });
                nodes++;
            }

            return root;
        }

        private static IEnumerable<Tk> Tokens(string html)
        {
            var source = html ?? "";
            var at = 0;
            var steps = 0;
            while (at < source.Length && steps++ < MaxHtmlTokens)
            {
                if (source[at] != '<')
                {
                    var next = source.IndexOf('<', at);
                    if (next < 0) next = source.Length;
                    yield return Tk.T(source.Substring(at, next - at), true);
                    at = next;
                    continue;
                }

                if (Starts(source, at, "<!--"))
                {
                    var end = source.IndexOf("-->", at + 4, StringComparison.Ordinal);
                    at = end < 0 ? source.Length : end + 3;
                    continue;
                }

                if (Starts(source, at, "<!") || Starts(source, at, "<?"))
                {
                    var end = TagEnd(source, at + 2);
                    at = end < 0 ? source.Length : end + 1;
                    continue;
                }

                var close = Starts(source, at, "</");
                var nameAt = at + (close ? 2 : 1);
                while (nameAt < source.Length && Char.IsWhiteSpace(source[nameAt])) nameAt++;
                if (nameAt >= source.Length || !NameStart(source[nameAt]))
                {
                    yield return Tk.T("<", true);
                    at++;
                    continue;
                }

                var nameEnd = nameAt + 1;
                while (nameEnd < source.Length && NameChar(source[nameEnd])) nameEnd++;
                if (nameEnd - nameAt > MaxHtmlName)
                {
                    yield return Tk.T("<", true);
                    at++;
                    continue;
                }

                var endTag = TagEnd(source, nameEnd);
                if (endTag < 0)
                {
                    yield return Tk.T(source.Substring(at), true);
                    yield break;
                }

                var name = source.Substring(nameAt, nameEnd - nameAt).ToLowerInvariant();
                if (close)
                {
                    yield return Tk.C(name);
                    at = endTag + 1;
                    continue;
                }

                var self = SelfClose(source, nameEnd, endTag);
                var attrLength = Math.Min(endTag - nameEnd, MaxHtmlAttributeText);
                var attrs = source.Substring(nameEnd, attrLength);
                yield return Tk.O(name, attrs, self);
                at = endTag + 1;

                var raw = RawKind(name);
                if (raw == 0 || self) continue;
                int rawEnd;
                var rawClose = RawClose(source, at, name, out rawEnd);
                if (rawClose < 0)
                {
                    if (at < source.Length) yield return Tk.T(source.Substring(at), raw == 2);
                    yield break;
                }
                if (rawClose > at) yield return Tk.T(source.Substring(at, rawClose - at), raw == 2);
                yield return Tk.C(name);
                at = rawEnd + 1;
            }
        }

        private static int TagEnd(string source, int start)
        {
            var quote = '\0';
            for (var i = start; i < source.Length; i++)
            {
                var value = source[i];
                if (quote != '\0')
                {
                    if (value == quote) quote = '\0';
                    continue;
                }
                if (value == '\'' || value == '"') quote = value;
                else if (value == '>') return i;
            }
            return -1;
        }

        private static int RawClose(string source, int start, string name, out int end)
        {
            var find = "</" + name;
            var at = start;
            while (at < source.Length)
            {
                var close = source.IndexOf(find, at, StringComparison.OrdinalIgnoreCase);
                if (close < 0) break;
                var after = close + find.Length;
                if (after < source.Length && !Char.IsWhiteSpace(source[after]) && source[after] != '>')
                {
                    at = after;
                    continue;
                }
                end = TagEnd(source, after);
                if (end >= 0) return close;
                break;
            }
            end = -1;
            return -1;
        }

        private static int RawKind(string name)
        {
            if (name == "title" || name == "textarea") return 2;
            return name == "script" || name == "style" || name == "xmp" || name == "iframe" ||
                   name == "noembed" || name == "noframes" ? 1 : 0;
        }

        private static bool SelfClose(string source, int start, int end)
        {
            for (var i = end - 1; i >= start; i--)
            {
                if (Char.IsWhiteSpace(source[i])) continue;
                return source[i] == '/';
            }
            return false;
        }

        private static bool Starts(string source, int at, string value)
        {
            return at >= 0 && at + value.Length <= source.Length &&
                   String.Compare(source, at, value, 0, value.Length, StringComparison.Ordinal) == 0;
        }

        private static bool NameStart(char value)
        {
            return value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z';
        }

        private static bool NameChar(char value)
        {
            return NameStart(value) || value >= '0' && value <= '9' || value == ':' || value == '-';
        }

        private static void Recover(List<N> stack, string tag)
        {
            var block = IsBlockTag(tag);
            if (block) CloseOpen(stack, "p");
            if (block) CloseOpen(stack, "h1", "h2", "h3", "h4", "h5", "h6");
            if (tag == "li") CloseScoped(stack, new[] { "li" }, new[] { "ul", "ol", "menu" });
            else if (tag == "dt" || tag == "dd") CloseScoped(stack, new[] { "dt", "dd" }, new[] { "dl" });
            else if (tag == "td" || tag == "th") CloseScoped(stack, new[] { "td", "th" }, new[] { "tr", "table" });
            else if (tag == "tr") CloseScoped(stack, new[] { "tr" }, new[] { "table" });
            else if (tag == "thead" || tag == "tbody" || tag == "tfoot") CloseScoped(stack, new[] { "thead", "tbody", "tfoot" }, new[] { "table" });
            else if (tag == "option") CloseScoped(stack, new[] { "option" }, new[] { "select", "datalist" });
            else if (tag == "optgroup")
            {
                CloseScoped(stack, new[] { "option" }, new[] { "select" });
                CloseScoped(stack, new[] { "optgroup" }, new[] { "select" });
            }
            else if (tag == "a" || tag == "button") CloseOpen(stack, tag);
        }

        private static void CloseScoped(List<N> stack, string[] tags, string[] stops)
        {
            for (var i = stack.Count - 1; i > 0; i--)
            {
                if (Has(stops, stack[i].K)) return;
                if (!Has(tags, stack[i].K)) continue;
                stack.RemoveRange(i, stack.Count - i);
                return;
            }
        }

        private static bool Has(string[] values, string value)
        {
            foreach (var item in values) if (item == value) return true;
            return false;
        }

        private static void CloseOpen(List<N> stack, params string[] tags)
        {
            for (var i = stack.Count - 1; i > 0; i--)
            {
                var close = false;
                foreach (var tag in tags) if (stack[i].K == tag) { close = true; break; }
                if (!close) continue;
                stack.RemoveRange(i, stack.Count - i);
                return;
            }
        }

        private static void Attrs(N node, string attrs)
        {
            const string pattern = "(?<n>[a-z_:][a-z0-9_:.\\-]*)(?:\\s*=\\s*(?:\"(?<d>[^\"]*)\"|'(?<s>[^']*)'|(?<b>[^\\s\"'=<>]+)))?";
            var source = attrs ?? "";
            if (source.Length > MaxHtmlAttributeText) source = source.Substring(0, MaxHtmlAttributeText);
            var count = 0;
            foreach (Match match in Regex.Matches(source, pattern, RegexOptions.IgnoreCase))
            {
                if (count++ == MaxHtmlAttributes) break;
                var name = match.Groups["n"].Value;
                if (name.Length > MaxHtmlName) continue;
                var value = match.Groups["d"].Success ? match.Groups["d"].Value : match.Groups["s"].Success ? match.Groups["s"].Value : match.Groups["b"].Value;
                if (value.Length > MaxHtmlAttributeValue) value = value.Substring(0, MaxHtmlAttributeValue);
                node.A[name] = WebUtility.HtmlDecode(value);
            }
        }

        private static bool Void(string tag)
        {
            return tag == "area" || tag == "base" || tag == "br" || tag == "col" || tag == "embed" || tag == "hr" || tag == "img" || tag == "input" || tag == "link" || tag == "meta" || tag == "param" || tag == "source" || tag == "track" || tag == "wbr";
        }

        private static bool IsBlockTag(string tag)
        {
            return IsHeading(tag) || tag == "address" || tag == "article" || tag == "aside" || tag == "blockquote" ||
                   tag == "center" || tag == "dd" || tag == "details" || tag == "div" || tag == "dl" || tag == "dt" ||
                   tag == "fieldset" || tag == "figcaption" || tag == "figure" || tag == "footer" || tag == "form" ||
                   tag == "header" || tag == "hr" || tag == "legend" || tag == "li" || tag == "main" || tag == "menu" ||
                   tag == "nav" || tag == "ol" || tag == "p" || tag == "pre" || tag == "section" || tag == "summary" ||
                   tag == "table" || tag == "ul";
        }

        private static bool IsHeading(string tag)
        {
            return tag == "h1" || tag == "h2" || tag == "h3" || tag == "h4" || tag == "h5" || tag == "h6";
        }

        private static N Find(N node, string kind)
        {
            if (node == null) return null;
            if (node.K == kind) return node;
            foreach (var child in node.C)
            {
                var found = Find(child, kind);
                if (found != null) return found;
            }
            return null;
        }

        private static Uri DocumentBase(N root, Uri responseUrl)
        {
            Uri found;
            return TryDocumentBase(root, responseUrl, out found) ? found : responseUrl;
        }

        private static bool TryDocumentBase(N node, Uri responseUrl, out Uri found)
        {
            found = null;
            if (node == null) return false;
            if (node.K == "base")
            {
                var href = node.Av("href").Trim();
                Uri candidate;
                if (href.Length > 0 && href.Length <= MaxBaseUrl && Uri.TryCreate(responseUrl, href, out candidate) &&
                    (candidate.IsFile || candidate.Scheme == Uri.UriSchemeHttp || candidate.Scheme == Uri.UriSchemeHttps))
                {
                    found = candidate;
                    return true;
                }
            }
            foreach (var child in node.C)
                if (TryDocumentBase(child, responseUrl, out found)) return true;
            return false;
        }

        private static int Count(N node)
        {
            if (node == null) return 0;
            var count = node.K.StartsWith("#") ? 0 : 1;
            foreach (var child in node.C) count += Count(child);
            return count;
        }

        private static string NodeText(N node)
        {
            if (node == null) return "";
            var text = new StringBuilder();
            AddText(node, text);
            return text.ToString();
        }

        private static void DiagnosticNodes(N node, int depth, IList<string> lines)
        {
            if (node == null || lines.Count == MaxDiagnosticNodes) return;
            var line = new StringBuilder();
            line.Append(new String(' ', Math.Min(24, depth) * 2)).Append(node.K);
            if (node.K == "#text") line.Append(" \"").Append(Short(node.X, 72)).Append('"');
            else
            {
                AddDiagnosticAttribute(line, "id", node.Av("id"));
                AddDiagnosticAttribute(line, "class", node.Av("class"));
                AddDiagnosticAttribute(line, "type", node.Av("type"));
                AddDiagnosticAttribute(line, "name", node.Av("name"));
                AddDiagnosticAttribute(line, "href", node.Av("href"));
                AddDiagnosticAttribute(line, "src", node.Av("src"));
            }
            var value = line.ToString();
            lines.Add(value.Length > 240 ? value.Substring(0, 240) : value);
            foreach (var child in node.C) DiagnosticNodes(child, depth + 1, lines);
        }

        private static void AddDiagnosticAttribute(StringBuilder line, string name, string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return;
            line.Append(' ').Append(name).Append("=\"").Append(Short(value, 80)).Append('"');
        }

        private Ld Track(string method, Ld load, string kind)
        {
            if (load != null && load.R != null) Trace(method, load.R.U, load.R.St, load.R.Ct, load.R.Sz, kind);
            return load;
        }

        private void Trace(string method, Uri url, int status, string type, long size, string kind)
        {
            var line = (method ?? "GET") + " " + status + " " + (kind ?? "net") + " " + size + "B " +
                Short(type, 48) + " " + Short(url == null ? "" : url.ToString(), 180);
            lock (requests)
            {
                requests.Add(Short(line, 320));
                if (requests.Count > MaxDiagnosticRequests) requests.RemoveAt(0);
            }
        }

        private static string Short(string value, int max)
        {
            var result = new StringBuilder();
            var space = false;
            foreach (var ch in value ?? "")
            {
                if (Char.IsWhiteSpace(ch) || Char.IsControl(ch)) { space = result.Length > 0; continue; }
                if (space && result.Length > 0) result.Append(' ');
                result.Append(ch);
                space = false;
                if (result.Length == max) break;
            }
            return result.ToString().Trim();
        }

        private static void AddText(N node, StringBuilder text)
        {
            if (node.K == "#text")
            {
                text.Append(node.X).Append(' ');
                return;
            }
            if (node.K == "script" || node.K == "style" || node.K == "noscript" || node.K == "template") return;
            foreach (var child in node.C) AddText(child, text);
        }

        private Dictionary<N, string> StyleSheets(N root, Uri baseUrl)
        {
            var sheets = new Dictionary<N, string>();
            var count = 0;
            foreach (var link in StyleLinks(root))
            {
                Uri url;
                if (!StyleUrl(link, baseUrl, out url)) continue;
                if (count++ == MaxStyleLinks) break;
                try
                {
                    var load = Fetch(url);
                    if (!UsableCss(load.R)) { DropCache(url); continue; }
                    sheets[link] = load.R.H;
                }
                catch (WebException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (NotSupportedException) { }
                catch (ArgumentException) { }
            }
            return sheets;
        }

        private async Task<Dictionary<N, string>> StyleSheetsA(N root, Uri baseUrl)
        {
            var sheets = new Dictionary<N, string>();
            var count = 0;
            foreach (var link in StyleLinks(root))
            {
                Uri url;
                if (!StyleUrl(link, baseUrl, out url)) continue;
                if (count++ == MaxStyleLinks) break;
                try
                {
                    var load = await FetchA(url);
                    if (!UsableCss(load.R)) { DropCache(url); continue; }
                    sheets[link] = load.R.H;
                }
                catch (WebException error)
                {
                    if (error.Status == WebExceptionStatus.RequestCanceled) throw;
                }
                catch (OperationCanceledException) { throw; }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (NotSupportedException) { }
                catch (ArgumentException) { }
            }
            return sheets;
        }

        private static List<N> StyleLinks(N root)
        {
            var found = new List<N>();
            var links = new List<N>();
            FindAll(root, "link", found);
            foreach (var link in found)
            {
                var rel = " " + Regex.Replace(link.Av("rel").Trim(), "\\s+", " ").ToLowerInvariant() + " ";
                var media = link.Av("media").Trim().ToLowerInvariant();
                var type = link.Av("type").Trim().ToLowerInvariant();
                if (rel.IndexOf(" stylesheet ", StringComparison.Ordinal) < 0 || rel.IndexOf(" alternate ", StringComparison.Ordinal) >= 0) continue;
                if (link.A.ContainsKey("disabled") || (media.Length > 0 && media != "all" && media != "screen") || (type.Length > 0 && type != "text/css")) continue;
                links.Add(link);
            }
            return links;
        }

        private static bool StyleUrl(N link, Uri baseUrl, out Uri url)
        {
            url = null;
            var href = link.Av("href");
            if (href.Length == 0 || !Uri.TryCreate(baseUrl, href, out url)) return false;
            return url.IsFile || url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps;
        }

        private static bool UsableCss(Cr reply)
        {
            return reply != null && reply.St >= 200 && reply.St < 300 && reply.Ct == "text/css" && reply.Sz <= MaxCss;
        }

        private static List<R> Css(N root, IDictionary<N, string> sheets)
        {
            var rules = new List<R>();
            var order = 0;
            CssRules(root, sheets, rules, ref order);
            rules.Sort(delegate(R a, R b) { var byWeight = a.W.CompareTo(b.W); return byWeight != 0 ? byWeight : a.O.CompareTo(b.O); });
            return rules;
        }

        private static void CssRules(N node, IDictionary<N, string> sheets, List<R> rules, ref int order)
        {
            string source = null;
            if (node.K == "style") source = RawText(node);
            else if (sheets != null) sheets.TryGetValue(node, out source);
            if (source != null)
            {
                foreach (Match match in Regex.Matches(source, "(?<q>[^{}]+)\\{(?<v>[^{}]*)\\}", RegexOptions.Singleline))
                {
                    foreach (var selector in CssGroups(match.Groups["q"].Value))
                    {
                        var query = selector.Trim();
                        var parts = new List<string>();
                        var joins = new List<char>();
                        if (query.Length == 0 || !CssParts(query, parts, joins)) continue;
                        var simple = new List<Se>();
                        foreach (var part in parts)
                        {
                            var value = CssSimple(part);
                            if (value == null) { simple.Clear(); break; }
                            simple.Add(value);
                        }
                        if (simple.Count == parts.Count)
                            rules.Add(new R(query, match.Groups["v"].Value, Spec(simple), order++, simple.ToArray(), joins.ToArray()));
                    }
                }
            }
            foreach (var child in node.C) CssRules(child, sheets, rules, ref order);
        }

        private static void FindAll(N node, string kind, List<N> found)
        {
            if (node.K == kind) found.Add(node);
            foreach (var child in node.C) FindAll(child, kind, found);
        }

        private static string RawText(N node)
        {
            var text = new StringBuilder();
            RawText(node, text);
            return text.ToString();
        }

        private static void RawText(N node, StringBuilder text)
        {
            if (node.K == "#text") { text.Append(node.X); return; }
            foreach (var child in node.C) RawText(child, text);
        }

        private static int Spec(List<Se> parts)
        {
            var score = 0;
            foreach (var part in parts) score += part.W;
            return score;
        }

        private static IEnumerable<string> CssGroups(string selectors)
        {
            var group = new StringBuilder();
            var brackets = 0;
            var parentheses = 0;
            var quote = '\0';
            for (var i = 0; i < (selectors ?? "").Length; i++)
            {
                var value = selectors[i];
                if (quote != '\0')
                {
                    group.Append(value);
                    if (value == quote && (i == 0 || selectors[i - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (value == '"' || value == '\'') { quote = value; group.Append(value); continue; }
                if (value == '[') brackets++;
                else if (value == ']' && brackets > 0) brackets--;
                else if (value == '(') parentheses++;
                else if (value == ')' && parentheses > 0) parentheses--;
                if (value == ',' && brackets == 0 && parentheses == 0)
                {
                    yield return group.ToString();
                    group.Clear();
                }
                else group.Append(value);
            }
            yield return group.ToString();
        }

        private static bool MatchCss(N node, R rule)
        {
            if (!MatchPart(node, rule.P[rule.P.Length - 1])) return false;

            var current = node;
            for (var i = rule.J.Length - 1; i >= 0; i--)
            {
                if (rule.J[i] == '>') current = current.Up;
                else if (rule.J[i] == '+') current = PreviousElement(current);
                else if (rule.J[i] == '~') current = PreviousMatchingElement(current, rule.P[i]);
                else
                {
                    current = current.Up;
                    while (current != null && !MatchPart(current, rule.P[i])) current = current.Up;
                    if (current == null) return false;
                    continue;
                }

                if (current == null || !MatchPart(current, rule.P[i])) return false;
            }
            return true;
        }

        private static R[] QueryRules(string query)
        {
            var source = (query ?? "").Trim();
            if (source.Length == 0 || source.Length > MaxQueryLength) return null;
            var rules = new List<R>();
            var totalParts = 0;
            foreach (var group in CssGroups(source))
            {
                var value = group.Trim();
                if (value.Length == 0 || rules.Count == MaxQueryGroups) return null;
                var parts = new List<string>();
                var joins = new List<char>();
                if (!CssParts(value, parts, joins) || totalParts + parts.Count > MaxQueryParts) return null;
                var simple = new List<Se>();
                foreach (var part in parts)
                {
                    var selector = CssSimple(part);
                    if (selector == null) return null;
                    simple.Add(selector);
                }
                totalParts += parts.Count;
                rules.Add(new R(value, "", Spec(simple), 0, simple.ToArray(), joins.ToArray()));
            }
            return rules.Count == 0 ? null : rules.ToArray();
        }

        private static N FindId(N node, string id)
        {
            if (node == null) return null;
            if (node.K.Length > 0 && node.K[0] != '#' && node.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return node;
            foreach (var child in node.C)
            {
                var found = FindId(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static N QueryOne(N node, R[] rules)
        {
            if (node == null) return null;
            if (ElementMatch(node, rules)) return node;
            foreach (var child in node.C)
            {
                var found = QueryOne(child, rules);
                if (found != null) return found;
            }
            return null;
        }

        private static void QueryAll(N node, R[] rules, List<N> found)
        {
            if (node == null || found.Count == MaxQueryResults) return;
            if (ElementMatch(node, rules)) found.Add(node);
            foreach (var child in node.C)
            {
                QueryAll(child, rules, found);
                if (found.Count == MaxQueryResults) return;
            }
        }

        private static bool ElementMatch(N node, R[] rules)
        {
            if (node.K.Length == 0 || node.K[0] == '#') return false;
            foreach (var rule in rules) if (MatchCss(node, rule)) return true;
            return false;
        }

        private static bool CssParts(string selector, List<string> parts, List<char> joins)
        {
            var source = (selector ?? "").Trim();
            var part = new StringBuilder();
            var join = ' ';
            var explicitJoin = false;
            var brackets = 0;
            var parentheses = 0;
            var quote = '\0';
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                if (quote != '\0')
                {
                    part.Append(value);
                    if (value == quote && (i == 0 || source[i - 1] != '\\')) quote = '\0';
                    continue;
                }
                if ((brackets > 0 || parentheses > 0) && (value == '\"' || value == '\'')) { quote = value; part.Append(value); continue; }
                if (value == '[') { brackets++; part.Append(value); continue; }
                if (value == ']')
                {
                    if (brackets == 0) return false;
                    brackets--;
                    part.Append(value);
                    continue;
                }
                if (brackets == 0 && value == '(') { parentheses++; part.Append(value); continue; }
                if (brackets == 0 && value == ')')
                {
                    if (parentheses == 0) return false;
                    parentheses--;
                    part.Append(value);
                    continue;
                }
                if (brackets == 0 && parentheses == 0 && (value == '>' || value == '+' || value == '~'))
                {
                    if (part.Length > 0)
                    {
                        AddCssPart(parts, joins, part, join);
                        explicitJoin = false;
                        join = ' ';
                    }
                    else if (parts.Count == 0 || explicitJoin) return false;
                    join = value;
                    explicitJoin = true;
                    continue;
                }
                if (brackets == 0 && parentheses == 0 && Char.IsWhiteSpace(value))
                {
                    if (part.Length > 0)
                    {
                        AddCssPart(parts, joins, part, join);
                        explicitJoin = false;
                        join = ' ';
                    }
                    continue;
                }
                part.Append(value);
            }
            if (quote != '\0' || brackets != 0 || parentheses != 0) return false;
            if (part.Length > 0)
            {
                AddCssPart(parts, joins, part, join);
                explicitJoin = false;
            }
            return parts.Count > 0 && !explicitJoin;
        }

        private static void AddCssPart(List<string> parts, List<char> joins, StringBuilder part, char join)
        {
            if (parts.Count > 0) joins.Add(join);
            parts.Add(part.ToString());
            part.Clear();
        }

        private static Se CssSimple(string selector)
        {
            return CssSimple(selector, true);
        }

        private static Se CssSimple(string selector, bool functions)
        {
            if (String.IsNullOrWhiteSpace(selector) || selector.Length > 512) return null;
            var tag = "";
            var index = 0;
            var weight = 0;
            var tags = Regex.Match(selector, "^(?<v>[a-z][a-z0-9-]*|\\*)", RegexOptions.IgnoreCase);
            if (tags.Success)
            {
                tag = tags.Groups["v"].Value.ToLowerInvariant();
                index = tags.Length;
                if (tag != "*") weight++;
            }

            var ids = new List<string>();
            var classes = new List<string>();
            var attrs = new List<Ca>();
            var pseudos = new List<Ps>();
            while (index < selector.Length)
            {
                var tail = selector.Substring(index);
                Match match;
                if (tail[0] == '#')
                {
                    match = Regex.Match(tail, "^#(?<v>[a-z0-9_-]+)", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;
                    ids.Add(match.Groups["v"].Value);
                    weight += 100;
                }
                else if (tail[0] == '.')
                {
                    match = Regex.Match(tail, "^\\.(?<v>[a-z0-9_-]+)", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;
                    classes.Add(match.Groups["v"].Value);
                    weight += 10;
                }
                else if (tail[0] == '[')
                {
                    match = Regex.Match(tail, "^\\[\\s*(?<n>[a-z_:][a-z0-9_:.\\-]*)\\s*(?:(?<o>[~|^$*]?=)\\s*(?<v>\"[^\"]*\"|'[^']*'|[^\\]\\s]+))?\\s*\\]", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;
                    var value = match.Groups["v"].Value;
                    if (value.Length >= 2 && ((value[0] == '\"' && value[value.Length - 1] == '\"') || (value[0] == '\'' && value[value.Length - 1] == '\''))) value = value.Substring(1, value.Length - 2);
                    attrs.Add(new Ca(match.Groups["n"].Value, match.Groups["o"].Value, WebUtility.HtmlDecode(value)));
                    weight += 10;
                }
                else if (tail[0] == ':')
                {
                    if (functions && tail.StartsWith(":not(", StringComparison.OrdinalIgnoreCase))
                    {
                        var end = CssClose(tail, 4);
                        if (end < 0) return null;
                        var excluded = CssSimple(tail.Substring(5, end - 5).Trim(), false);
                        if (excluded == null) return null;
                        pseudos.Add(new Ps("not", "", excluded));
                        weight += excluded.W;
                        index += end + 1;
                        continue;
                    }
                    match = Regex.Match(tail, "^:(?<n>nth-child|nth-of-type)\\(\\s*(?<v>odd|even|[1-9][0-9]{0,3})\\s*\\)", RegexOptions.IgnoreCase);
                    if (!match.Success)
                        match = Regex.Match(tail, "^:(?<n>first-child|last-child|only-child|first-of-type|last-of-type|only-of-type|empty|root|link|checked|disabled|enabled|required|read-only|read-write|valid|invalid|hover|focus)\\b", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;
                    pseudos.Add(new Ps(match.Groups["n"].Value.ToLowerInvariant(), match.Groups["v"].Value.ToLowerInvariant()));
                    weight += 10;
                }
                else return null;
                index += match.Length;
            }
            return new Se(tag, ids.ToArray(), classes.ToArray(), attrs.ToArray(), pseudos.ToArray(), weight);
        }

        private static int CssClose(string source, int open)
        {
            var depth = 0;
            var brackets = 0;
            var quote = '\0';
            for (var i = open; i < (source ?? "").Length; i++)
            {
                var value = source[i];
                if (quote != '\0')
                {
                    if (value == quote && (i == 0 || source[i - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (brackets > 0 && (value == '\"' || value == '\'')) { quote = value; continue; }
                if (value == '[') { brackets++; continue; }
                if (value == ']')
                {
                    if (brackets == 0) return -1;
                    brackets--;
                    continue;
                }
                if (brackets > 0) continue;
                if (value == '(') depth++;
                else if (value == ')' && --depth == 0) return i;
            }
            return -1;
        }

        private static N PreviousElement(N node)
        {
            if (node == null || node.Up == null) return null;
            N previous = null;
            foreach (var child in node.Up.C)
            {
                if (Object.ReferenceEquals(child, node)) return previous;
                if (child.K.Length > 0 && child.K[0] != '#') previous = child;
            }
            return null;
        }

        private static N PreviousMatchingElement(N node, Se selector)
        {
            if (node == null || node.Up == null) return null;
            N match = null;
            foreach (var child in node.Up.C)
            {
                if (Object.ReferenceEquals(child, node)) return match;
                if (child.K.Length > 0 && child.K[0] != '#' && MatchPart(child, selector)) match = child;
            }
            return null;
        }

        private static bool MatchPart(N node, Se selector)
        {
            if (selector.T.Length > 0 && selector.T != "*" && node.K != selector.T) return false;
            foreach (var id in selector.I) if (!node.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return false;
            if (selector.C.Length > 0)
            {
                var classes = new HashSet<string>((node.Av("class") ?? "").Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
                foreach (var value in selector.C) if (!classes.Contains(value)) return false;
            }
            foreach (var attr in selector.A) if (!MatchAttribute(node, attr)) return false;
            foreach (var pseudo in selector.Ps) if (!MatchPseudo(node, pseudo)) return false;
            return true;
        }

        private static bool MatchAttribute(N node, Ca attr)
        {
            string actual;
            if (!node.A.TryGetValue(attr.N, out actual)) return false;
            if (attr.O.Length == 0) return true;
            if (attr.O == "=") return actual.Equals(attr.V, StringComparison.OrdinalIgnoreCase);
            if (attr.O == "^=") return actual.StartsWith(attr.V, StringComparison.OrdinalIgnoreCase);
            if (attr.O == "$=") return actual.EndsWith(attr.V, StringComparison.OrdinalIgnoreCase);
            if (attr.O == "*=") return actual.IndexOf(attr.V, StringComparison.OrdinalIgnoreCase) >= 0;
            if (attr.O == "|=") return actual.Equals(attr.V, StringComparison.OrdinalIgnoreCase) || actual.StartsWith(attr.V + "-", StringComparison.OrdinalIgnoreCase);
            if (attr.O == "~=")
            {
                foreach (var value in actual.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    if (value.Equals(attr.V, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static bool MatchPseudo(N node, Ps pseudo)
        {
            if (pseudo.N == "not") return pseudo.S != null && !MatchPart(node, pseudo.S);
            if (pseudo.N == "hover") return node.Hv;
            if (pseudo.N == "focus") return node.Fc;
            if (pseudo.N == "root") return node.Up != null && node.Up.K == "#doc";
            if (pseudo.N == "empty") return node.C.Count == 0;
            if (pseudo.N == "link") return (node.K == "a" || node.K == "area") && node.A.ContainsKey("href");
            if (pseudo.N == "checked") return (node.K == "input" && node.A.ContainsKey("checked")) || (node.K == "option" && node.A.ContainsKey("selected"));
            if (pseudo.N == "disabled") return FormNode(node) && node.A.ContainsKey("disabled");
            if (pseudo.N == "enabled") return FormNode(node) && !node.A.ContainsKey("disabled");
            if (pseudo.N == "required") return RequiredNode(node) && node.A.ContainsKey("required");
            if (pseudo.N == "read-only") return EditNode(node) && node.A.ContainsKey("readonly");
            if (pseudo.N == "read-write") return EditNode(node) && !node.A.ContainsKey("readonly") && !node.A.ContainsKey("disabled");
            if (pseudo.N == "invalid") return RequiredNode(node) && node.A.ContainsKey("data-lea-invalid");
            if (pseudo.N == "valid") return RequiredNode(node) && !node.A.ContainsKey("data-lea-invalid");

            var type = pseudo.N.IndexOf("of-type", StringComparison.Ordinal) >= 0;
            int count;
            var position = ElementPosition(node, type, out count);
            if (position == 0) return false;
            if (pseudo.N == "first-child" || pseudo.N == "first-of-type") return position == 1;
            if (pseudo.N == "last-child" || pseudo.N == "last-of-type") return position == count;
            if (pseudo.N == "only-child" || pseudo.N == "only-of-type") return count == 1;
            if (pseudo.N == "nth-child" || pseudo.N == "nth-of-type")
            {
                if (pseudo.V == "odd") return position % 2 == 1;
                if (pseudo.V == "even") return position % 2 == 0;
                int expected;
                return Int32.TryParse(pseudo.V, out expected) && position == expected;
            }
            return false;
        }

        private static int ElementPosition(N node, bool type, out int count)
        {
            count = 0;
            var position = 0;
            if (node == null || node.Up == null) return 0;
            foreach (var child in node.Up.C)
            {
                if (child.K.Length == 0 || child.K[0] == '#' || (type && child.K != node.K)) continue;
                count++;
                if (Object.ReferenceEquals(child, node)) position = count;
            }
            return position;
        }

        private static bool FormNode(N node)
        {
            return node.K == "button" || node.K == "fieldset" || node.K == "input" || node.K == "option" || node.K == "select" || node.K == "textarea";
        }

        private static bool RequiredNode(N node)
        {
            return node.K == "input" || node.K == "select" || node.K == "textarea";
        }

        private static bool EditNode(N node)
        {
            return node.K == "input" || node.K == "textarea";
        }

        private static S Style(N node, S inherited, List<R> rules)
        {
            var style = inherited == null ? new S() : inherited.Inherit();
            TagStyle(style, node.K);
            foreach (var rule in rules) if (MatchCss(node, rule)) Decls(style, rule.V);
            Decls(style, node.Av("style"));
            return style;
        }

        private static void Decls(S style, string declarations)
        {
            foreach (var declaration in (declarations ?? "").Split(';'))
            {
                var split = declaration.IndexOf(':');
                if (split < 1) continue;
                var name = declaration.Substring(0, split).Trim().ToLowerInvariant();
                var value = declaration.Substring(split + 1).Trim();
                if (name == "color") style.F = value;
                else if (name == "background" || name == "background-color") style.Bg = value;
                else if (name == "font-family") FontFamily(style, value);
                else if (name == "font-weight") style.B = value.Equals("bold", StringComparison.OrdinalIgnoreCase) || value == "700" || value == "800" || value == "900";
                else if (name == "font-style") style.I = value.Equals("italic", StringComparison.OrdinalIgnoreCase) || value.Equals("oblique", StringComparison.OrdinalIgnoreCase);
                else if (name == "text-decoration" || name == "text-decoration-line") TextDecoration(style, value);
                else if (name == "text-align") TextAlign(style, value);
                else if (name == "line-height") LineHeight(style, value);
                else if (name == "white-space") WhiteSpace(style, value);
                else if (name == "word-break" || name == "overflow-wrap" || name == "word-wrap") WordBreak(style, value, name == "word-break");
                else if (name == "overflow") Overflow(style, value);
                else if (name == "visibility") Visibility(style, value);
                else if (name == "opacity") Opacity(style, value);
                else if (name == "isolation") Isolation(style, value);
                else if (name == "transform") Transform(style, value);
                else if (name == "display") style.D = value.ToLowerInvariant();
                else if (name == "position") Position(style, value);
                else if (name == "inset") Inset(style, value);
                else if (name == "top") Offset(style, value, 0);
                else if (name == "right") Offset(style, value, 1);
                else if (name == "bottom") Offset(style, value, 2);
                else if (name == "left") Offset(style, value, 3);
                else if (name == "z-index") ZIndex(style, value);
                else if (name == "margin") Box(style, value, true);
                else if (name == "padding") Box(style, value, false);
                else if (name == "margin-top") style.Mt = Px(value);
                else if (name == "margin-right") style.Mr = Px(value);
                else if (name == "margin-bottom") style.Mb = Px(value);
                else if (name == "margin-left") style.Ml = Px(value);
                else if (name == "padding-top") style.Pt = Px(value);
                else if (name == "padding-right") style.Pr = Px(value);
                else if (name == "padding-bottom") style.Pb = Px(value);
                else if (name == "padding-left") style.Pl = Px(value);
                else if (name == "width") style.W = Px(value);
                else if (name == "height") style.H = Px(value);
                else if (name == "min-width") style.Wn = Px(value);
                else if (name == "max-width") style.Wx = Px(value);
                else if (name == "min-height") style.Hn = Px(value);
                else if (name == "max-height") style.Hx = Px(value);
                else if (name == "border") Border(style, value, 0);
                else if (name == "border-top") Border(style, value, 1);
                else if (name == "border-right") Border(style, value, 2);
                else if (name == "border-bottom") Border(style, value, 3);
                else if (name == "border-left") Border(style, value, 4);
                else if (name == "border-width") BorderWidths(style, value);
                else if (name == "border-top-width") style.Bt = Px(value);
                else if (name == "border-right-width") style.Br = Px(value);
                else if (name == "border-bottom-width") style.Bb = Px(value);
                else if (name == "border-left-width") style.Bl = Px(value);
                else if (name == "border-color") style.Bc = value;
                else if (name == "border-style") style.Bs = value.ToLowerInvariant();
                else if (name == "font-size")
                {
                    var size = Regex.Match(value, "(?<n>[0-9]{1,3})(?:px)?", RegexOptions.IgnoreCase);
                    int number;
                    if (size.Success && int.TryParse(size.Groups["n"].Value, out number) && number > 0 && number < 200) style.Fs = number;
                }
            }
        }

        private static void Position(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "relative") style.Pn = "r";
            else if (mode == "absolute") style.Pn = "a";
            else if (mode == "fixed") style.Pn = "f";
            else if (mode == "static" || mode == "initial" || mode == "unset" || mode == "revert") style.Pn = "s";
        }

        private static void Offset(S style, string value, int side)
        {
            int? offset;
            if (!OffsetValue(value, out offset)) return;

            if (side == 0) style.Tp = offset;
            else if (side == 1) style.Rt = offset;
            else if (side == 2) style.Bm = offset;
            else style.Lt = offset;
        }

        private static void Inset(S style, string value)
        {
            var parts = Regex.Split((value ?? "").Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4 || parts[0].Length == 0) return;
            int? top;
            int? right;
            int? bottom;
            int? left;
            if (!OffsetValue(parts[0], out top) ||
                !OffsetValue(parts.Length > 1 ? parts[1] : parts[0], out right) ||
                !OffsetValue(parts.Length > 2 ? parts[2] : parts[0], out bottom) ||
                !OffsetValue(parts.Length > 3 ? parts[3] : parts.Length > 1 ? parts[1] : parts[0], out left)) return;
            style.Tp = top;
            style.Rt = right;
            style.Bm = bottom;
            style.Lt = left;
        }

        private static bool OffsetValue(string value, out int? offset)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            offset = null;
            if (source == "auto" || source == "initial" || source == "unset" || source == "revert") return true;
            var match = Regex.Match(source, "^(?<n>-?[0-9]{1,5})(?:px)?$");
            int number;
            if (!match.Success || !Int32.TryParse(match.Groups["n"].Value, out number)) return false;
            offset = Math.Max(-8192, Math.Min(8192, number));
            return true;
        }

        private static void ZIndex(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "auto" || source == "initial" || source == "unset" || source == "revert")
            {
                style.Zi = 0;
                style.Za = false;
                return;
            }

            var match = Regex.Match(source, "^-?[0-9]{1,5}$");
            int number;
            if (match.Success && Int32.TryParse(match.Value, out number))
            {
                style.Zi = Math.Max(-1000, Math.Min(1000, number));
                style.Za = true;
            }
        }

        private static void FontFamily(S style, string value)
        {
            var source = (value ?? "").Trim();
            var lower = source.ToLowerInvariant();
            if (lower == "inherit") return;
            if (lower == "initial" || lower == "unset" || lower == "revert") { style.Ff = ""; return; }
            if (source.Length == 0 || source.Length > 512) return;

            var names = new List<string>();
            foreach (var item in source.Split(','))
            {
                var name = Regex.Replace(item.Trim(), "\\s+", " ");
                if (name.Length >= 2 && ((name[0] == '\"' && name[name.Length - 1] == '\"') || (name[0] == '\'' && name[name.Length - 1] == '\'')))
                    name = name.Substring(1, name.Length - 2).Trim();
                lower = name.ToLowerInvariant();
                if (lower == "sans-serif") name = "ss";
                else if (lower == "serif") name = "s";
                else if (lower == "monospace") name = "m";
                else if (lower == "cursive") name = "c";
                else if (lower == "fantasy") name = "f";
                else if (lower == "system-ui" || lower == "ui-sans-serif") name = "ui";
                else if (!Regex.IsMatch(name, "^[a-z0-9 _-]{1,64}$", RegexOptions.IgnoreCase)) continue;
                names.Add(name);
                if (names.Count == 8) break;
            }
            if (names.Count > 0) style.Ff = String.Join("|", names.ToArray());
        }

        private static void TextDecoration(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "inherit") return;
            if (source == "none" || source == "initial" || source == "unset" || source == "revert") { style.Td = "n"; return; }
            var underline = Regex.IsMatch(source, "(?:^|\\s)underline(?:\\s|$)");
            var strike = Regex.IsMatch(source, "(?:^|\\s)line-through(?:\\s|$)");
            if (underline || strike) style.Td = underline ? strike ? "us" : "u" : "s";
        }

        private static void TextAlign(S style, string value)
        {
            var align = (value ?? "").Trim().ToLowerInvariant();
            if (align == "left" || align == "start") style.Ta = "l";
            else if (align == "center") style.Ta = "c";
            else if (align == "right" || align == "end") style.Ta = "r";
            else if (align == "justify") style.Ta = "j";
            else if (align == "initial") style.Ta = "";
        }

        private static void LineHeight(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "normal" || source == "initial")
            {
                style.Lh = 0;
                style.Lm = 0;
                return;
            }

            var pixels = Regex.Match(source, "^(?<n>[0-9]{1,3})px$");
            int number;
            if (pixels.Success && Int32.TryParse(pixels.Groups["n"].Value, out number) && number > 0)
            {
                style.Lh = Math.Min(400, number);
                style.Lm = 0;
                return;
            }

            var relative = Regex.Match(source, "^(?<n>[0-9]{1,3}(?:\\.[0-9]{1,2})?)(?<p>%?)$");
            double scale;
            if (!relative.Success || !Double.TryParse(relative.Groups["n"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out scale)) return;
            if (relative.Groups["p"].Value == "%") scale /= 100D;
            if (scale < 0.25D || scale > 5D) return;
            style.Lm = Math.Max(25, Math.Min(500, (int)Math.Round(scale * 100D)));
            style.Lh = 0;
        }

        private static void WhiteSpace(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "normal" || mode == "initial") style.Ws = "n";
            else if (mode == "nowrap") style.Ws = "nw";
            else if (mode == "pre") style.Ws = "p";
            else if (mode == "pre-wrap" || mode == "break-spaces") style.Ws = "pw";
            else if (mode == "pre-line") style.Ws = "pl";
        }

        private static void WordBreak(S style, string value, bool direct)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "normal" || mode == "initial") style.Wb = "n";
            else if (mode == "break-word") style.Wb = "b";
            else if (mode == "anywhere" || (direct && mode == "break-all")) style.Wb = "a";
            else if (direct && mode == "keep-all") style.Wb = "k";
        }

        private static void Overflow(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "visible" || mode == "initial") style.Ov = "v";
            else if (mode == "hidden") style.Ov = "h";
            else if (mode == "clip") style.Ov = "c";
            else if (mode == "auto") style.Ov = "a";
            else if (mode == "scroll") style.Ov = "s";
        }

        private static void Visibility(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "visible" || mode == "initial") style.Vi = "v";
            else if (mode == "hidden") style.Vi = "h";
            else if (mode == "collapse") style.Vi = "c";
        }

        private static void Opacity(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "inherit" || source == "initial" || source == "unset" || source == "revert")
            {
                style.Op = style.Ao;
                style.Zo = false;
                UpdateStackContext(style);
                return;
            }
            var match = Regex.Match(source, "^(?<n>-?(?:[0-9]{1,3}(?:\\.[0-9]{1,3})?|\\.[0-9]{1,3}))(?<p>%?)$");
            double number;
            if (!match.Success || !Double.TryParse(match.Groups["n"].Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number)) return;
            if (match.Groups["p"].Value == "%") number /= 100D;
            number = Math.Max(0D, Math.Min(1D, number));
            style.Op = Math.Max(0, Math.Min(100, (int)Math.Round(style.Ao * number)));
            style.Zo = number < 1D;
            UpdateStackContext(style);
        }

        private static void Isolation(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode != "isolate" && mode != "auto" && mode != "initial" && mode != "unset" && mode != "revert") return;
            style.Zs = mode == "isolate";
            UpdateStackContext(style);
        }

        private static void Transform(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "none" || source == "initial" || source == "unset" || source == "revert")
            {
                style.Tx = 0;
                style.Ty = 0;
                style.Zt = false;
                UpdateStackContext(style);
                return;
            }

            var match = Regex.Match(source, "^translate\\(\\s*(?<x>-?[0-9]{1,5})(?:px)?(?:\\s*,\\s*|\\s+)(?<y>-?[0-9]{1,5})(?:px)?\\s*\\)$");
            var xOnly = Regex.Match(source, "^translate(?:x)?\\(\\s*(?<x>-?[0-9]{1,5})(?:px)?\\s*\\)$");
            var yOnly = Regex.Match(source, "^translatey\\(\\s*(?<y>-?[0-9]{1,5})(?:px)?\\s*\\)$");
            var x = 0;
            var y = 0;
            if (match.Success)
            {
                if (!Int32.TryParse(match.Groups["x"].Value, out x) || !Int32.TryParse(match.Groups["y"].Value, out y)) return;
            }
            else if (xOnly.Success)
            {
                if (!Int32.TryParse(xOnly.Groups["x"].Value, out x)) return;
            }
            else if (yOnly.Success)
            {
                if (!Int32.TryParse(yOnly.Groups["y"].Value, out y)) return;
            }
            else return;
            style.Tx = Math.Max(-8192, Math.Min(8192, x));
            style.Ty = Math.Max(-8192, Math.Min(8192, y));
            style.Zt = true;
            UpdateStackContext(style);
        }

        private static void UpdateStackContext(S style)
        {
            style.Zc = style.Zo || style.Zs || style.Zt;
        }

        private static void Box(S style, string value, bool margin)
        {
            var parts = Regex.Split(value.Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4) return;
            var top = Px(parts[0]);
            var right = parts.Length > 1 ? Px(parts[1]) : top;
            var bottom = parts.Length > 2 ? Px(parts[2]) : top;
            var left = parts.Length > 3 ? Px(parts[3]) : right;
            if (parts.Length == 2) bottom = top;
            if (margin) { style.Mt = top; style.Mr = right; style.Mb = bottom; style.Ml = left; }
            else { style.Pt = top; style.Pr = right; style.Pb = bottom; style.Pl = left; }
        }

        private static int Px(string value)
        {
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,4})(?:px)?$", RegexOptions.IgnoreCase);
            int number;
            return match.Success && int.TryParse(match.Groups["n"].Value, out number) ? Math.Min(number, 2000) : 0;
        }

        private static void Border(S style, string value, int side)
        {
            var width = -1;
            var borderStyle = "";
            var color = "";
            foreach (var token in Regex.Split((value ?? "").Trim(), "\\s+"))
            {
                var lower = token.ToLowerInvariant();
                var size = BorderPx(lower);
                if (size >= 0) width = size;
                else if (lower == "none" || lower == "hidden" || lower == "solid" || lower == "dashed" || lower == "dotted" || lower == "double") borderStyle = lower;
                else if (token.Length > 0) color = token;
            }
            if (width < 0 && borderStyle.Length > 0 && borderStyle != "none" && borderStyle != "hidden") width = 3;
            if (width >= 0) SetBorder(style, side, width);
            if (borderStyle.Length > 0) style.Bs = borderStyle;
            if (color.Length > 0) style.Bc = color;
        }

        private static void BorderWidths(S style, string value)
        {
            var parts = Regex.Split((value ?? "").Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4) return;
            var top = Math.Max(0, BorderPx(parts[0]));
            var right = parts.Length > 1 ? Math.Max(0, BorderPx(parts[1])) : top;
            var bottom = parts.Length > 2 ? Math.Max(0, BorderPx(parts[2])) : top;
            var left = parts.Length > 3 ? Math.Max(0, BorderPx(parts[3])) : right;
            if (parts.Length == 2) bottom = top;
            style.Bt = top;
            style.Br = right;
            style.Bb = bottom;
            style.Bl = left;
        }

        private static int BorderPx(string value)
        {
            if (value == "thin") return 1;
            if (value == "medium") return 3;
            if (value == "thick") return 5;
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,3})(?:px)?$", RegexOptions.IgnoreCase);
            int number;
            return match.Success && int.TryParse(match.Groups["n"].Value, out number) ? Math.Min(number, 100) : -1;
        }

        private static void SetBorder(S style, int side, int width)
        {
            if (side == 0 || side == 1) style.Bt = width;
            if (side == 0 || side == 2) style.Br = width;
            if (side == 0 || side == 3) style.Bb = width;
            if (side == 0 || side == 4) style.Bl = width;
        }

        private static void Blocks(P page, N node, Uri baseUrl, List<R> rules, S inherited)
        {
            if (node == null) return;
            var kind = node.K;
            if (kind == "#text")
            {
                var loose = Clean(node.X);
                if (loose.Length > 0) page.Bs.Add(new B { K = "p", X = loose, S = inherited, El = node.Up });
                return;
            }
            if (kind == "head" || kind == "script" || kind == "style" || kind == "noscript" || kind == "template") return;
            var style = Style(node, inherited, rules);
            if (kind == "pre" && style.Ws.Length == 0) style.Ws = "p";
            if (style.D == "none") return;
            if (kind == "table")
            {
                AddTable(page, node, baseUrl, rules, style);
                return;
            }
            if (kind == "picture")
            {
                AddPicture(page, node, baseUrl, rules, style);
                return;
            }
            if (kind == "hr")
            {
                page.Bs.Add(new B { K = kind, X = "", S = style, El = node });
                return;
            }
            if (kind == "a")
            {
                Uri link;
                var href = node.Av("href");
                var text = TextValue(RawText(node), style);
                if (href.Length > 0 && Uri.TryCreate(baseUrl, href, out link))
                {
                    page.Ls.Add(new Ln { U = link, T = Clean(text) });
                    page.Bs.Add(new B { K = kind, X = text.Length == 0 ? link.ToString() : text, U = link, S = style, El = node });
                }
                return;
            }
            else if (kind == "img")
            {
                ImageDimensions(node, style);
                var image = ImageUri(node, baseUrl, style.W);
                if (image != null) page.Bs.Add(new B { K = kind, X = node.Av("alt"), U = image, S = style, El = node });
            }
            else if (kind == "input" || kind == "textarea" || kind == "button" || kind == "select")
            {
                AddField(page, node, baseUrl, style);
                return;
            }
            else if (IsTextBlock(kind) || IsContainerBlock(kind))
            {
                var text = TextValue(BlockText(node, PreservesLines(style)), style);
                var block = new B { K = kind, X = text, S = style, El = node };
                if (HasInlineStructure(node))
                {
                    var inlineStyle = style.Inherit();
                    if (IsHeading(kind)) inlineStyle.B = true;
                    foreach (var child in node.C) AddInline(page, block, child, baseUrl, rules, inlineStyle, null, kind);
                    NormalizeInlines(block);
                    block.X = InlineText(block);
                }
                if (block.X.Length > 0 || block.Is.Count > 0 || LayoutContext(style) || (IsContainerBlock(kind) && VisualContext(style))) page.Bs.Add(block);
                AddNestedBlocks(page, node, baseUrl, rules, style);
                return;
            }

            foreach (var child in node.C) Blocks(page, child, baseUrl, rules, style);
        }

        private static void AddPicture(P page, N picture, Uri baseUrl, List<R> rules, S inherited)
        {
            N image = null;
            foreach (var child in picture.C) if (child.K == "img") { image = child; break; }
            if (image == null) return;

            var style = Style(image, inherited, rules);
            if (style.D == "none") return;
            ImageDimensions(image, style);
            Uri source = null;
            foreach (var child in picture.C)
            {
                if (child.K != "source" || !ImageMedia(child.Av("media")) || !ImageType(child.Av("type"))) continue;
                source = ImageUri(child, baseUrl, style.W);
                if (source != null) break;
            }
            if (source == null) source = ImageUri(image, baseUrl, style.W);
            if (source != null) page.Bs.Add(new B { K = "img", X = image.Av("alt"), U = source, S = style, El = image });
        }

        private static void ImageDimensions(N image, S style)
        {
            if (style.W == 0) style.W = HtmlDimension(image.Av("width"));
            if (style.H == 0) style.H = HtmlDimension(image.Av("height"));
        }

        private static int HtmlDimension(string value)
        {
            var match = Regex.Match(value ?? "", "^\\s*(?<n>[1-9][0-9]{0,4})(?:px)?\\s*$", RegexOptions.IgnoreCase);
            int number;
            return match.Success && Int32.TryParse(match.Groups["n"].Value, out number) ? Math.Min(8192, number) : 0;
        }

        private static Uri ImageUri(N node, Uri baseUrl, int targetWidth)
        {
            var source = ImageCandidate(node.Av("srcset"), targetWidth);
            if (source.Length == 0) source = node.Av("src").Trim();
            Uri image;
            return source.Length > 0 && source.Length <= 12 * 1024 * 1024 && Uri.TryCreate(baseUrl, source, out image) ? image : null;
        }

        private static string ImageCandidate(string value, int targetWidth)
        {
            var candidates = ImageCandidates(value);
            Ic chosen = null;
            var widths = false;
            foreach (var candidate in candidates) if (candidate.W > 0) { widths = true; break; }
            if (widths)
            {
                var target = Math.Max(1, targetWidth);
                foreach (var candidate in candidates)
                {
                    if (candidate.W < 1) continue;
                    if (chosen == null || (chosen.W < target && candidate.W > chosen.W) || (candidate.W >= target && (chosen.W < target || candidate.W < chosen.W))) chosen = candidate;
                }
            }
            else
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.D < 1) continue;
                    if (chosen == null || (chosen.D < 100 && candidate.D > chosen.D) || (candidate.D >= 100 && (chosen.D < 100 || candidate.D < chosen.D))) chosen = candidate;
                }
            }
            return chosen == null ? "" : chosen.U;
        }

        private static List<Ic> ImageCandidates(string value)
        {
            var found = new List<Ic>();
            var source = (value ?? "").Trim();
            if (source.Length > 1024 * 1024) return found;
            var index = 0;
            while (index < source.Length && found.Count < 16)
            {
                while (index < source.Length && (source[index] == ',' || Char.IsWhiteSpace(source[index]))) index++;
                if (index >= source.Length) break;
                var start = index;
                var data = source.Substring(index).StartsWith("data:", StringComparison.OrdinalIgnoreCase);
                while (index < source.Length && !Char.IsWhiteSpace(source[index]) && (data || source[index] != ',')) index++;
                var url = source.Substring(start, index - start).Trim().Trim('"', '\'');
                while (index < source.Length && Char.IsWhiteSpace(source[index])) index++;
                start = index;
                while (index < source.Length && source[index] != ',') index++;
                var descriptor = source.Substring(start, index - start).Trim().ToLowerInvariant();
                if (index < source.Length && source[index] == ',') index++;

                int width = 0;
                var density = 0;
                if (descriptor.Length == 0) density = 100;
                else
                {
                    var widthMatch = Regex.Match(descriptor, "^(?<n>[1-9][0-9]{0,5})w$");
                    var densityMatch = Regex.Match(descriptor, "^(?<n>(?:[0-9](?:\\.[0-9]{1,2})?|\\.[0-9]{1,2}))x$");
                    if (widthMatch.Success) Int32.TryParse(widthMatch.Groups["n"].Value, out width);
                    else if (densityMatch.Success)
                    {
                        decimal parsed;
                        if (Decimal.TryParse(densityMatch.Groups["n"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed) && parsed > 0 && parsed <= 8) density = (int)Math.Round(parsed * 100);
                    }
                    else continue;
                }
                if (url.Length > 0 && (width > 0 || density > 0)) found.Add(new Ic(url, width, density));
            }
            return found;
        }

        private static bool ImageMedia(string value)
        {
            var media = (value ?? "").Trim().ToLowerInvariant();
            return media.Length == 0 || media == "all" || media == "screen";
        }

        private static bool ImageType(string value)
        {
            var type = (value ?? "").Split(';')[0].Trim().ToLowerInvariant();
            return type.Length == 0 || type == "image/png" || type == "image/jpeg" || type == "image/gif" || type == "image/bmp" || type == "image/x-icon" || type == "image/vnd.microsoft.icon";
        }

        private static bool IsTextBlock(string kind)
        {
            return IsHeading(kind) || kind == "address" || kind == "blockquote" || kind == "code" || kind == "dd" ||
                   kind == "dt" || kind == "figcaption" || kind == "label" || kind == "legend" || kind == "li" ||
                   kind == "p" || kind == "pre" || kind == "summary";
        }

        private static bool IsContainerBlock(string kind)
        {
            return kind == "article" || kind == "aside" || kind == "center" || kind == "details" || kind == "div" ||
                   kind == "figure" || kind == "footer" || kind == "header" || kind == "main" || kind == "nav" ||
                   kind == "section";
        }

        private static string BlockText(N node, bool raw)
        {
            var text = new StringBuilder();
            AddBlockText(node, text, true, raw);
            return text.ToString();
        }

        private static void AddBlockText(N node, StringBuilder text, bool root, bool raw)
        {
            if (node.K == "#text")
            {
                text.Append(node.X);
                if (!raw) text.Append(' ');
                return;
            }
            if (!root && (IsBlockTag(node.K) || node.K == "img" || node.K == "input" || node.K == "textarea" || node.K == "button" || node.K == "select")) return;
            if (node.K == "script" || node.K == "style" || node.K == "noscript" || node.K == "template") return;
            foreach (var child in node.C) AddBlockText(child, text, false, raw);
        }

        private static bool PreservesLines(S style)
        {
            return style != null && (style.Ws == "p" || style.Ws == "pw" || style.Ws == "pl");
        }

        private static string TextValue(string value, S style)
        {
            if (!PreservesLines(style)) return Clean(value);
            return (value ?? "").Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private static void AddNestedBlocks(P page, N node, Uri baseUrl, List<R> rules, S inherited)
        {
            foreach (var child in node.C)
            {
                if (child.K == "#text") continue;
                if (IsBlockTag(child.K) || child.K == "img" || child.K == "input" || child.K == "textarea" || child.K == "button" || child.K == "select")
                {
                    Blocks(page, child, baseUrl, rules, inherited);
                    continue;
                }
                var style = Style(child, inherited, rules);
                if (style.D != "none") AddNestedBlocks(page, child, baseUrl, rules, style);
            }
        }

        private static bool HasInlineStructure(N node)
        {
            foreach (var child in node.C) if (child.K != "#text") return true;
            return false;
        }

        private static void AddInline(P page, B block, N node, Uri baseUrl, List<R> rules, S inherited, Uri activeLink, string inheritedKind)
        {
            if (node.K == "#text")
            {
                if (node.X.Length > 0) block.Is.Add(new Ir { K = inheritedKind, X = node.X, U = activeLink, S = inherited, El = node.Up });
                return;
            }
            if (node.K == "script" || node.K == "style" || node.K == "noscript" || node.K == "template") return;
            if (IsBlockTag(node.K)) return;

            var style = Style(node, inherited, rules);
            if (style.D == "none") return;
            if (node.K == "br")
            {
                block.Is.Add(new Ir { K = "br", X = "", U = activeLink, S = style, El = node });
                return;
            }

            var link = activeLink;
            var kind = node.K;
            if (node.K == "a")
            {
                Uri target;
                var href = node.Av("href");
                if (href.Length > 0 && Uri.TryCreate(baseUrl, href, out target))
                {
                    link = target;
                    page.Ls.Add(new Ln { U = link, T = Clean(NodeText(node)) });
                }
            }

            var first = block.Is.Count;
            if (node.K != "img") foreach (var child in node.C) AddInline(page, block, child, baseUrl, rules, style, link, kind);
            if (node.K == "a" && link != null && block.Is.Count == first)
                block.Is.Add(new Ir { K = "a", X = link.ToString(), U = link, S = style, El = node });
        }

        private static void NormalizeInlines(B block)
        {
            var clean = new List<Ir>();
            if (PreservesLines(block.S))
            {
                foreach (var run in block.Is)
                {
                    if (run.K != "br") run.X = TextValue(WebUtility.HtmlDecode(run.X ?? ""), block.S);
                    if (run.K != "br" && run.X.Length == 0) continue;
                    var previous = clean.Count == 0 ? null : clean[clean.Count - 1];
                    if (previous != null && previous.K != "br" && previous.K == run.K && previous.U == run.U && Object.ReferenceEquals(previous.S, run.S) && Object.ReferenceEquals(previous.El, run.El)) previous.X += run.X;
                    else clean.Add(run);
                }
                block.Is.Clear();
                foreach (var run in clean) block.Is.Add(run);
                return;
            }

            var pendingSpace = false;
            var lineHasText = false;
            foreach (var run in block.Is)
            {
                if (run.K == "br")
                {
                    clean.Add(run);
                    pendingSpace = false;
                    lineHasText = false;
                    continue;
                }

                var value = WebUtility.HtmlDecode(run.X ?? "");
                var text = new StringBuilder();
                foreach (var ch in value)
                {
                    if (Char.IsWhiteSpace(ch))
                    {
                        if (lineHasText) pendingSpace = true;
                        continue;
                    }
                    if (pendingSpace)
                    {
                        text.Append(' ');
                        pendingSpace = false;
                    }
                    text.Append(ch);
                    lineHasText = true;
                }
                if (text.Length == 0) continue;
                run.X = text.ToString();
                var previous = clean.Count == 0 ? null : clean[clean.Count - 1];
                if (previous != null && previous.K != "br" && previous.K == run.K && previous.U == run.U && Object.ReferenceEquals(previous.S, run.S) && Object.ReferenceEquals(previous.El, run.El)) previous.X += run.X;
                else clean.Add(run);
            }
            block.Is.Clear();
            foreach (var run in clean) block.Is.Add(run);
        }

        private static string InlineText(B block)
        {
            var text = new StringBuilder();
            var lines = PreservesLines(block.S);
            foreach (var run in block.Is)
            {
                if (run.K == "br")
                {
                    if (lines) text.Append('\n');
                    else if (text.Length > 0 && text[text.Length - 1] != ' ') text.Append(' ');
                }
                else text.Append(run.X);
            }
            return lines ? text.ToString() : text.ToString().Trim();
        }

        private static void AddTable(P page, N node, Uri baseUrl, List<R> rules, S style)
        {
            var table = new Tb();
            foreach (var child in node.C)
            {
                if (child.K == "caption" && table.Cp.Length == 0) table.Cp = Clean(NodeText(child));
                else AddTableRows(page, table, child, baseUrl, rules, style);
            }
            if (table.Rs.Count > 0) page.Bs.Add(new B { K = "table", X = table.Cp, S = style, Tb = table, El = node });
        }

        private static void AddTableRows(P page, Tb table, N node, Uri baseUrl, List<R> rules, S inherited)
        {
            if (node.K == "table") return;
            var style = Style(node, inherited, rules);
            if (style.D == "none") return;
            if (node.K == "tr")
            {
                var row = new Tr();
                var columns = 0;
                foreach (var child in node.C)
                {
                    if (child.K != "td" && child.K != "th") continue;
                    var cellStyle = Style(child, style, rules);
                    if (cellStyle.D == "none") continue;
                    int span;
                    if (!Int32.TryParse(child.Av("colspan"), out span)) span = 1;
                    span = Math.Max(1, Math.Min(16, span));
                    var cell = new Tc { X = TextValue(RawText(child), cellStyle), S = cellStyle, H = child.K == "th", Sp = span, El = child };
                    AddCellBlocks(page, cell, child, baseUrl, rules, cellStyle, child.K);
                    if (cell.Bs.Count == 0 && cell.X.Length > 0) cell.Bs.Add(new B { K = child.K, X = cell.X, S = cellStyle, El = child });
                    row.Cs.Add(cell);
                    columns += span;
                }
                if (row.Cs.Count > 0)
                {
                    table.Rs.Add(row);
                    table.Cs = Math.Max(table.Cs, columns);
                }
                return;
            }
            if (node.K != "thead" && node.K != "tbody" && node.K != "tfoot") return;
            foreach (var child in node.C) AddTableRows(page, table, child, baseUrl, rules, style);
        }

        private static void AddCellBlocks(P page, Tc cell, N node, Uri baseUrl, List<R> rules, S inherited, string fallback)
        {
            foreach (var child in node.C)
            {
                if (child.K == "#text")
                {
                    var text = TextValue(child.X, inherited);
                    if (text.Length > 0) cell.Bs.Add(new B { K = fallback, X = text, S = inherited, El = child.Up });
                    continue;
                }

                var style = Style(child, inherited, rules);
                if (child.K == "pre" && style.Ws.Length == 0) style.Ws = "p";
                if (style.D == "none" || child.K == "script" || child.K == "style" || child.K == "noscript" || child.K == "template") continue;
                if (child.K == "br")
                {
                    cell.Bs.Add(new B { K = "br", X = "", S = style, El = child });
                    continue;
                }
                if (child.K == "a")
                {
                    Uri link;
                    var href = child.Av("href");
                    var text = TextValue(RawText(child), style);
                    if (href.Length > 0 && Uri.TryCreate(baseUrl, href, out link))
                    {
                        if (text.Length == 0) text = link.ToString();
                        page.Ls.Add(new Ln { U = link, T = Clean(text) });
                        cell.Bs.Add(new B { K = "a", X = text, U = link, S = style, El = child });
                    }
                    continue;
                }

                var nested = HasCellStructure(child);
                if (!nested)
                {
                    var text = TextValue(RawText(child), style);
                    if (text.Length > 0) cell.Bs.Add(new B { K = CellKind(child.K, fallback), X = text, S = style, El = child });
                    continue;
                }
                AddCellBlocks(page, cell, child, baseUrl, rules, style, CellKind(child.K, fallback));
            }
        }

        private static bool HasCellStructure(N node)
        {
            foreach (var child in node.C) if (child.K != "#text") return true;
            return false;
        }

        private static string CellKind(string kind, string fallback)
        {
            return IsTextBlock(kind) || kind == "strong" || kind == "b" ? kind : fallback;
        }

        private static void TagStyle(S style, string kind)
        {
            if (kind == "strong" || kind == "b") style.B = true;
            if (kind == "em" || kind == "i") style.I = true;
            if (kind == "u") AddDecoration(style, 'u');
            if (kind == "s" || kind == "strike" || kind == "del") AddDecoration(style, 's');
        }

        private static void AddDecoration(S style, char value)
        {
            if (style.Td == "n") style.Td = "";
            var underline = value == 'u' || style.Td.IndexOf('u') >= 0;
            var strike = value == 's' || style.Td.IndexOf('s') >= 0;
            style.Td = underline ? strike ? "us" : "u" : strike ? "s" : "";
        }

        private static void AddField(P page, N node, Uri baseUrl, S style)
        {
            var kind = node.K == "textarea" ? "textarea" : node.K == "select" ? "select" : node.K == "button" ? FieldKind(node, "submit") : FieldKind(node, "text");
            if (kind == "hidden") return;

            var buttonText = node.K == "button" ? Clean(NodeText(node)) : "";
            var value = node.K == "textarea" ? RawText(node).Trim() : node.Av("value");
            if (node.K == "button" && value.Length == 0) value = buttonText;
            if (kind == "file") value = "";
            if ((kind == "checkbox" || kind == "radio") && value.Length == 0) value = "on";
            if (kind == "color" && !Regex.IsMatch(value, "^#[0-9a-f]{6}$", RegexOptions.IgnoreCase)) value = "#000000";
            if (kind == "range" && value.Length == 0) value = RangeDefault(node.Av("min"), node.Av("max"));

            var form = Parent(node, "form");
            Uri action = baseUrl;
            Uri formAction;
            var actionValue = node.Av("formaction");
            if (actionValue.Length == 0 && form != null) actionValue = form.Av("action");
            if (actionValue.Length > 0 && Uri.TryCreate(baseUrl, actionValue, out formAction)) action = formAction;
            var method = node.Av("formmethod");
            if (method.Length == 0 && form != null) method = form.Av("method");
            method = method.Length == 0 ? "get" : method.Trim().ToLowerInvariant();
            int min;
            int max;
            var pattern = node.Av("pattern");
            if (pattern.Length > 512) pattern = pattern.Substring(0, 512);
            var field = new Fi
            {
                K = kind,
                N = node.Av("name"),
                V = value,
                D = value,
                P = node.Av("placeholder"),
                C = node.A.ContainsKey("checked"),
                Dc = node.A.ContainsKey("checked"),
                Ds = node.A.ContainsKey("disabled"),
                Ro = node.A.ContainsKey("readonly"),
                Rq = node.A.ContainsKey("required"),
                Mn = Int32.TryParse(node.Av("minlength"), out min) ? Math.Max(0, Math.Min(min, 32767)) : 0,
                Mx = Int32.TryParse(node.Av("maxlength"), out max) ? Math.Max(0, Math.Min(max, 32767)) : 0,
                Pa = pattern,
                Lo = node.Av("min"),
                Hi = node.Av("max"),
                Sp = node.Av("step"),
                Ac = node.Av("accept"),
                Mu = node.A.ContainsKey("multiple"),
                En = FormEncoding(node, form),
                Nv = node.A.ContainsKey("formnovalidate") || (form != null && form.A.ContainsKey("novalidate")),
                F = form,
                U = action,
                M = method,
                S = style,
                El = node
            };
            if (kind == "select") AddOptions(field, node);
            Fi current;
            if (page.Fr != null && page.Fr.TryGetValue(node, out current))
            {
                var currentValue = current.V;
                var currentChecked = current.C;
                var currentIndex = current.Si;
                var currentValid = current.Ok;
                var currentMessage = current.Vm;
                CopyField(current, field);
                current.V = currentValue;
                current.C = currentChecked;
                current.Si = currentIndex;
                current.Ok = currentValid;
                current.Vm = currentMessage;
                field = current;
            }
            page.Fs.Add(field);

            var blockKind = node.K == "button" || kind == "submit" || kind == "reset" || kind == "button" ? "button" : node.K;
            var text = node.K == "button" && buttonText.Length > 0 ? buttonText : blockKind == "button" ? value : node.Av("aria-label");
            if (blockKind == "button" && text.Length == 0) text = kind == "reset" ? "Reset" : kind == "submit" ? "Submit" : "Button";
            page.Bs.Add(new B { K = blockKind, X = text, S = style, I = field, El = node });
        }

        private static void CopyField(Fi target, Fi source)
        {
            target.K = source.K;
            target.N = source.N;
            target.P = source.P;
            target.Ds = source.Ds;
            target.Ro = source.Ro;
            target.Rq = source.Rq;
            target.Mn = source.Mn;
            target.Mx = source.Mx;
            target.Pa = source.Pa;
            target.Lo = source.Lo;
            target.Hi = source.Hi;
            target.Sp = source.Sp;
            target.Ac = source.Ac;
            target.Mu = source.Mu;
            target.En = source.En;
            target.Nv = source.Nv;
            target.Ok = source.Ok;
            target.Vm = source.Vm;
            target.F = source.F;
            target.U = source.U;
            target.M = source.M;
            target.S = source.S;
            target.D = source.D;
            target.Dc = source.Dc;
            target.Di = source.Di;
            target.El = source.El;
            target.Os.Clear();
            foreach (var option in source.Os) target.Os.Add(option);
        }

        private static void AddOptions(Fi field, N select)
        {
            var nodes = new List<N>();
            FindAll(select, "option", nodes);
            O first = null;
            O chosen = null;
            var chosenIndex = -1;
            var index = 0;
            foreach (var node in nodes)
            {
                var text = Clean(NodeText(node));
                var option = new O
                {
                    V = node.A.ContainsKey("value") ? node.Av("value") : text,
                    T = text,
                    Ds = node.A.ContainsKey("disabled") || (node.Up != null && node.Up.K == "optgroup" && node.Up.A.ContainsKey("disabled"))
                };
                field.Os.Add(option);
                if (first == null) first = option;
                if (chosen == null && node.A.ContainsKey("selected"))
                {
                    chosen = option;
                    chosenIndex = index;
                }
                index++;
            }
            if (chosen == null)
            {
                chosen = first;
                chosenIndex = first == null ? -1 : 0;
            }
            field.V = chosen == null ? "" : chosen.V;
            field.D = field.V;
            field.Si = chosenIndex;
            field.Di = chosenIndex;
        }

        private static string FieldKind(N node, string fallback)
        {
            var kind = node.Av("type").Trim().ToLowerInvariant();
            if (kind.Length == 0) return fallback;
            return Regex.IsMatch(kind, "^(?:text|password|search|email|url|tel|number|range|color|date|time|month|week|datetime-local|checkbox|radio|file|submit|reset|button|hidden)$") ? kind : fallback;
        }

        private static string FormEncoding(N node, N form)
        {
            var value = node.Av("formenctype");
            if (value.Length == 0 && form != null) value = form.Av("enctype");
            return value.Trim().Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase) ? "m" : "u";
        }

        private static string RangeDefault(string low, string high)
        {
            decimal lo;
            decimal hi;
            if (!Decimal.TryParse(low, NumberStyles.Float, CultureInfo.InvariantCulture, out lo)) lo = 0;
            if (!Decimal.TryParse(high, NumberStyles.Float, CultureInfo.InvariantCulture, out hi)) hi = 100;
            if (hi < lo) hi = lo;
            return (lo + (hi - lo) / 2M).ToString(CultureInfo.InvariantCulture);
        }

        private static N Parent(N node, string kind)
        {
            var parent = node.Up;
            while (parent != null && parent.K != kind) parent = parent.Up;
            return parent;
        }

        private static Er LoadError(Exception error, Uri url)
        {
            var typed = error as Er;
            if (typed != null) return typed;
            if (error is InvalidDataException || error is DecoderFallbackException)
                return new Er("decode", url, 0, false, "Lea could not decode the response" + ErrorAt(url) + ".", error);
            if (error is UriFormatException || error is NotSupportedException)
                return new Er("url", url, 0, false, "Lea could not use the address" + ErrorAt(url) + ".", error);
            var webError = error as WebException;
            if (webError != null)
            {
                var response = webError.Response as HttpWebResponse;
                if (response != null)
                {
                    var status = (int)response.StatusCode;
                    var retryable = status == 408 || status == 429 || status >= 500;
                    return new Er("http", response.ResponseUri ?? url, status, retryable, "Lea received HTTP " + status + ErrorAt(response.ResponseUri ?? url) + ".", error);
                }
                if (webError.Status == WebExceptionStatus.Timeout)
                    return new Er("timeout", url, 0, true, "Lea timed out while loading" + ErrorAt(url) + ".", error);
                if (webError.Status == WebExceptionStatus.RequestCanceled)
                    return new Er("cancel", url, 0, false, "Lea stopped loading" + ErrorAt(url) + ".", error);
                return new Er("net", url, 0, true, "Lea could not reach" + ErrorAt(url) + ".", error);
            }
            if (error is OperationCanceledException)
                return new Er("cancel", url, 0, false, "Lea stopped loading" + ErrorAt(url) + ".", error);
            if (error is FileNotFoundException || error is DirectoryNotFoundException || error is UnauthorizedAccessException || (error is IOException && url != null && url.IsFile))
                return new Er("file", url, 0, false, "Lea could not read the local file" + ErrorAt(url) + ".", error);
            if (error is IOException)
                return new Er("net", url, 0, true, "Lea could not read the response" + ErrorAt(url) + ".", error);
            return null;
        }

        private static string ErrorAt(Uri url)
        {
            return url == null ? "" : " " + url;
        }

        private bool GetCache(Uri url, out Cr reply)
        {
            var key = CacheKey(url);
            reply = null;
            if (cm == 0 || !cache.TryGetValue(key, out reply)) return false;
            if (reply.Fr <= DateTime.UtcNow)
            {
                reply = null;
                return false;
            }
            lru.Remove(key);
            lru.Add(key);
            ch++;
            return true;
        }

        private bool GetStale(Uri url, out Cr reply)
        {
            var key = CacheKey(url);
            reply = null;
            if (cm == 0 || !cache.TryGetValue(key, out reply) || reply.Fr > DateTime.UtcNow) return false;
            if (reply.Et.Length == 0 && reply.Lm.Length == 0)
            {
                DropCache(url);
                reply = null;
                return false;
            }
            lru.Remove(key);
            lru.Add(key);
            return true;
        }

        private void PutCache(Uri url, Cr reply)
        {
            var key = CacheKey(url);
            var reusable = reply.Fr > DateTime.UtcNow || reply.Et.Length > 0 || reply.Lm.Length > 0;
            if (cm == 0 || reply.Ns || !reusable || reply.St < 200 || reply.St >= 300 || reply.Sz > MaxCachePage)
            {
                cache.Remove(key);
                lru.Remove(key);
                return;
            }
            cache[key] = reply;
            lru.Remove(key);
            lru.Add(key);
            TrimCache();
        }

        private void TrimCache()
        {
            while (lru.Count > cm)
            {
                var key = lru[0];
                lru.RemoveAt(0);
                cache.Remove(key);
            }
        }

        private void DropCache(Uri url)
        {
            if (url == null) return;
            var key = CacheKey(url);
            cache.Remove(key);
            lru.Remove(key);
        }

        private static string CacheKey(Uri url)
        {
            if (url.Fragment.Length == 0) return url.AbsoluteUri;
            var clean = new UriBuilder(url);
            clean.Fragment = "";
            return clean.Uri.AbsoluteUri;
        }

        private static Cr ReadFile(Uri url)
        {
            var path = Local(url);
            var bytes = File.ReadAllBytes(path);
            string charset;
            var html = Decode(bytes, "", out charset);
            return new Cr(url, html, 200, FileType(path), "", charset, bytes.Length, true, DateTime.MinValue, "", "");
        }

        private Cr ReadWeb(Uri url, byte[] bytes)
        {
            var body = Expand(bytes, web.Ce);
            string charset;
            var html = Decode(body, Charset(web.Ct), out charset);
            return new Cr(web.U ?? url, html, web.St == 0 ? 200 : web.St, MediaType(web.Ct), web.Ce, charset, body.Length, web.Ns, FreshUntil(web.Cc, web.Ex), web.Et, web.Lm);
        }

        private Cr Revalidated(Cr stale)
        {
            return new Cr(web.U ?? stale.U, stale.H, stale.St, stale.Ct, stale.Ce, stale.Ch, stale.Sz, web.Ns,
                FreshUntil(web.Cc, web.Ex), web.Et.Length == 0 ? stale.Et : web.Et, web.Lm.Length == 0 ? stale.Lm : web.Lm);
        }

        private DateTime FreshUntil(string cacheControl, string expires)
        {
            var control = cacheControl ?? "";
            if (Regex.IsMatch(control, "(?:^|,)\\s*(?:no-store|no-cache)\\b", RegexOptions.IgnoreCase)) return DateTime.MinValue;
            var age = Regex.Match(control, "(?:^|,)\\s*max-age\\s*=\\s*\"?(?<n>[0-9]+)", RegexOptions.IgnoreCase);
            long seconds;
            if (age.Success && Int64.TryParse(age.Groups["n"].Value, out seconds))
                return DateTime.UtcNow.AddSeconds(Math.Min(604800L, seconds));
            DateTimeOffset expiry;
            if (DateTimeOffset.TryParse(expires ?? "", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out expiry)) return expiry.UtcDateTime;
            return DateTime.UtcNow.AddSeconds(cs);
        }

        private static byte[] Expand(byte[] bytes, string contentEncoding)
        {
            var encoding = (contentEncoding ?? "").Trim().ToLowerInvariant();
            if (encoding != "gzip" && encoding != "deflate") return bytes;
            using (var input = new MemoryStream(bytes))
            using (Stream zip = encoding == "gzip" ? (Stream)new GZipStream(input, CompressionMode.Decompress) : new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                zip.CopyTo(output);
                return output.ToArray();
            }
        }

        private static string FileType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".htm" || extension == ".html") return "text/html";
            if (extension == ".txt") return "text/plain";
            if (extension == ".css") return "text/css";
            return "application/octet-stream";
        }

        private static string MediaType(string contentType)
        {
            var split = (contentType ?? "").IndexOf(';');
            return (split < 0 ? contentType ?? "" : contentType.Substring(0, split)).Trim().ToLowerInvariant();
        }

        private static string Charset(string contentType)
        {
            var match = Regex.Match(contentType ?? "", "(?:^|;)\\s*charset\\s*=\\s*[\"']?(?<c>[^;\\s\"']+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["c"].Value.Trim() : "";
        }

        private static string Decode(byte[] bytes, string charset, out string used)
        {
            Encoding encoding = null;
            if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255) encoding = new UTF32Encoding(true, true);
            else if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0) encoding = Encoding.UTF32;
            else if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191) encoding = Encoding.UTF8;
            else if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == 255) encoding = Encoding.BigEndianUnicode;
            else if (bytes.Length >= 2 && bytes[0] == 255 && bytes[1] == 254) encoding = Encoding.Unicode;
            if (encoding == null && charset.Length > 0)
            {
                try { encoding = Encoding.GetEncoding(charset); }
                catch (ArgumentException) { }
            }
            if (encoding == null) encoding = Encoding.UTF8;
            used = encoding.WebName.ToLowerInvariant();
            using (var stream = new MemoryStream(bytes))
            using (var reader = new StreamReader(stream, encoding, true)) return reader.ReadToEnd();
        }

        private sealed class Tk
        {
            public readonly char K;
            public readonly string N;
            public readonly string A;
            public readonly string X;
            public readonly bool S;
            public readonly bool D;

            private Tk(char kind, string name, string attrs, string text, bool self, bool decode)
            {
                K = kind;
                N = name;
                A = attrs;
                X = text;
                S = self;
                D = decode;
            }

            public static Tk T(string text, bool decode) { return new Tk('t', "", "", text, false, decode); }
            public static Tk O(string name, string attrs, bool self) { return new Tk('o', name, attrs, "", self, false); }
            public static Tk C(string name) { return new Tk('c', name, "", "", false, false); }
        }

        private sealed class R
        {
            public readonly string Q;
            public readonly string V;
            public readonly int W;
            public readonly int O;
            public readonly Se[] P;
            public readonly char[] J;

            public R(string query, string values, int weight, int order, Se[] parts, char[] joins)
            {
                Q = query;
                V = values;
                W = weight;
                O = order;
                P = parts;
                J = joins;
            }
        }

        private sealed class Ic
        {
            public readonly string U;
            public readonly int W;
            public readonly int D;

            public Ic(string url, int width, int density)
            {
                U = url;
                W = width;
                D = density;
            }
        }

        private sealed class Se
        {
            public readonly string T;
            public readonly string[] I;
            public readonly string[] C;
            public readonly Ca[] A;
            public readonly Ps[] Ps;
            public readonly int W;

            public Se(string tag, string[] ids, string[] classes, Ca[] attrs, Ps[] pseudos, int weight)
            {
                T = tag;
                I = ids;
                C = classes;
                A = attrs;
                Ps = pseudos;
                W = weight;
            }
        }

        private sealed class Ca
        {
            public readonly string N;
            public readonly string O;
            public readonly string V;

            public Ca(string name, string operation, string value)
            {
                N = name;
                O = operation;
                V = value;
            }
        }

        private sealed class Ps
        {
            public readonly string N;
            public readonly string V;
            public readonly Se S;

            public Ps(string name, string value)
                : this(name, value, null)
            {
            }

            public Ps(string name, string value, Se selector)
            {
                N = name;
                V = value;
                S = selector;
            }
        }

        private sealed class Ld
        {
            public readonly Cr R;
            public readonly bool Ca;

            public Ld(Cr reply, bool cached)
            {
                R = reply;
                Ca = cached;
            }
        }

        private sealed class Cr
        {
            public readonly Uri U;
            public readonly string H;
            public readonly int St;
            public readonly string Ct;
            public readonly string Ce;
            public readonly string Ch;
            public readonly int Sz;
            public readonly bool Ns;
            public readonly DateTime Fr;
            public readonly string Et;
            public readonly string Lm;

            public Cr(Uri url, string html, int status, string contentType, string contentEncoding, string charset, int size, bool noStore, DateTime freshUntil, string etag, string lastModified)
            {
                U = url;
                H = html ?? "";
                St = status;
                Ct = contentType ?? "";
                Ce = (contentEncoding ?? "").Trim().ToLowerInvariant();
                Ch = charset ?? "";
                Sz = size;
                Ns = noStore;
                Fr = freshUntil;
                Et = etag ?? "";
                Lm = lastModified ?? "";
            }
        }

        private sealed class W : WebClient
        {
            public const int CookieMax = 64;
            private CookieContainer normal = NewJar();
            private CookieContainer privateJar = NewJar();
            private CookieContainer jar;
            private bool pv;

            public Uri U { get; private set; }
            public int St { get; private set; }
            public string Ct { get; private set; }
            public string Ce { get; private set; }
            public bool Ns { get; private set; }
            public string Cc { get; private set; }
            public string Ex { get; private set; }
            public string Et { get; private set; }
            public string Lm { get; private set; }
            public long Len { get; private set; }
            private string ve = "";
            private string vm = "";

            public W()
            {
                jar = normal;
            }

            public int Kc { get { return jar.Count; } }
            public bool Pv
            {
                get { return pv; }
                set
                {
                    if (pv == value) return;
                    pv = value;
                    if (pv)
                    {
                        privateJar = NewJar();
                        jar = privateJar;
                    }
                    else
                    {
                        privateJar = NewJar();
                        jar = normal;
                    }
                }
            }

            public void Kcl()
            {
                if (pv) privateJar = NewJar();
                else normal = NewJar();
                jar = pv ? privateJar : normal;
            }

            public void Va(string etag, string lastModified)
            {
                ve = etag ?? "";
                vm = lastModified ?? "";
            }

            public bool Nm(WebException error)
            {
                var response = error == null ? null : error.Response as HttpWebResponse;
                if (response == null || response.StatusCode != HttpStatusCode.NotModified) return false;
                Capture(response);
                return true;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                U = null;
                St = 0;
                Ct = "";
                Ce = "";
                Ns = false;
                Cc = "";
                Ex = "";
                Et = "";
                Lm = "";
                Len = -1;
                var request = base.GetWebRequest(address);
                var http = request as HttpWebRequest;
                if (http != null)
                {
                    http.CookieContainer = jar;
                    http.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    http.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    http.UserAgent = "Lea/0.3";
                    http.Timeout = 15000;
                    http.ReadWriteTimeout = 15000;
                    if (ve.Length > 0) http.Headers[HttpRequestHeader.IfNoneMatch] = ve;
                    DateTime modified;
                    if (vm.Length > 0 && DateTime.TryParse(vm, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out modified)) http.IfModifiedSince = modified.ToUniversalTime();
                }
                return request;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                return Capture(base.GetWebResponse(request));
            }

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                return Capture(base.GetWebResponse(request, result));
            }

            private WebResponse Capture(WebResponse response)
            {
                var http = response as HttpWebResponse;
                if (http == null) return response;
                U = http.ResponseUri;
                St = (int)http.StatusCode;
                Ct = http.ContentType ?? "";
                Ce = http.Headers[HttpResponseHeader.ContentEncoding] ?? "";
                Cc = http.Headers[HttpResponseHeader.CacheControl] ?? "";
                Ex = http.Headers[HttpResponseHeader.Expires] ?? "";
                Et = http.Headers[HttpResponseHeader.ETag] ?? "";
                Lm = http.Headers[HttpResponseHeader.LastModified] ?? "";
                Len = http.ContentLength;
                Ns = Cc.IndexOf("no-store", StringComparison.OrdinalIgnoreCase) >= 0;
                return response;
            }

            private static CookieContainer NewJar()
            {
                return new CookieContainer(CookieMax, 16, 4096);
            }
        }

        private static Uri ToUri(string input, Uri baseUrl)
        {
            input = (input ?? "").Trim().Trim('"');
            Uri u;
            if (Uri.TryCreate(input, UriKind.Absolute, out u)) return u;
            if (baseUrl != null && Uri.TryCreate(baseUrl, input, out u)) return u;
            if (File.Exists(input)) return new Uri(Path.GetFullPath(input));
            try { return new Uri("https://" + input); }
            catch (UriFormatException error) { throw new Er("url", null, 0, false, "Lea could not understand that address.", error); }
        }

        private static string Local(Uri url)
        {
            var path = url.LocalPath;
            var query = path.IndexOf('?');
            return query < 0 ? path : path.Substring(0, query);
        }

        private static string Query(Uri url)
        {
            var text = url.AbsoluteUri;
            var start = text.IndexOf('?');
            var mark = 1;
            if (start < 0 && url.IsFile)
            {
                start = text.IndexOf("%3F", StringComparison.OrdinalIgnoreCase);
                mark = 3;
            }
            if (start < 0) return "";
            var end = text.IndexOf('#', start);
            return end < 0 ? text.Substring(start + mark) : text.Substring(start + mark, end - start - mark);
        }

        private static string Clean(string text)
        {
            return Regex.Replace(WebUtility.HtmlDecode(text ?? ""), "\\s+", " ").Trim();
        }
    }
}
