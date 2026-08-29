using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lea.Engine
{
    // Lea's .NET Framework-compatible engine surface.
    public sealed class E
    {
        private const int MaxCachePage = 2 * 1024 * 1024;
        private const int MaxCss = 256 * 1024;
        private const int MaxStyleLinks = 8;
        private const int MaxCssRules = 1024;
        private const int MaxMediaDepth = 4;
        private const int MaxMediaGroups = 8;
        private const int MaxViewport = 8192;
        private const int MaxCssFunctionGroups = 8;
        private const int MaxCssFunctionTests = 16;
        private const int MaxCssRelativeParts = 4;
        private const int MaxHasVisits = 4096;
        private const int MaxNthExpression = 32;
        private const int MaxNthValue = 8192;
        private const int MaxCssDeclarations = 128;
        private const int MaxCustomProperties = 32;
        private const int MaxCustomName = 64;
        private const int MaxCustomValue = 512;
        private const int MaxVarDepth = 8;
        private const int MaxVarValue = 1024;
        private const int MaxGradientText = 1024;
        private const int MaxGradientStops = 8;
        private const int MaxBackgroundText = 4096;
        private const int MaxBackgroundLength = 2000;
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
        private const int MaxFragment = 4096;
        private const int MaxHistory = 128;
        private const int MaxGaugeText = 64;
        private const double MaxGaugeValue = 1000000000000D;
        private const int MaxImageMaps = 32;
        private const int MaxMapAreas = 64;
        private const int MaxMapCoordinates = 128;
        private const double MaxMapCoordinate = 1000000D;
        private const int MaxSvgShapes = 128;
        private const int MaxSvgPoints = 128;
        private const int MaxSvgDepth = 8;
        private const int MaxSvgText = 256;
        private const int MaxSvgGradients = 16;
        private const int MaxSvgStops = 16;
        private const int MaxSvgGradientLinks = 4;
        private const int MaxSvgGradientId = 64;
        private const int MaxSvgSources = 64;
        private const int MaxSvgClips = 16;
        private const int MaxSvgClipShapes = 32;
        private const int MaxSvgUseLinks = 4;
        private const int MaxSvgDashes = 16;
        private const int MaxSvgDashText = 256;
        private const int MaxSvgTransforms = 16;
        private const int MaxSvgTransformText = 1024;
        private const double MaxSvgCoordinate = 100000D;
        private const double MaxSvgSize = 2000D;
        private const double MaxSvgMiterLimit = 64D;
        private const double MinSvgView = .01D;
        private const double MaxSvgMatrixScale = 10000D;
        private const double MaxSvgMatrixTranslate = 1000000D;
        private const int MaxTabIndex = 32767;
        private const int MaxAutofocusCandidates = 32;
        private const int MaxAccessKeyText = 256;
        private const int MaxAccessKeyTokens = 16;
        private const int MaxAccessibilityNodes = 512;
        private const int MaxAccessibilityReferences = 16;
        private const int MaxAccessibilityText = 512;
        private const int MinTimeout = 100;
        private const int MaxTimeout = 60000;
        private const int DefaultTimeout = 15000;
        private static readonly Pd[] NoDeclarations = new Pd[0];
        private static readonly Se[] NoSelectors = new Se[0];
        private static readonly Hr[] NoRelatives = new Hr[0];
        private readonly W web = new W();
        private readonly Pc disk = new Pc();
        private readonly List<He> hs = new List<He>();
        private readonly Dictionary<string, Cr> cache = new Dictionary<string, Cr>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> lru = new List<string>();
        private readonly List<string> requests = new List<string>();
        private int at = -1;
        private bool busy;
        private int cm = 8;
        private int cs = 60;
        private int ch;
        private int vw = 1024;
        private bool je = true;
        private Stream download;
        private bool stopped;

        public P At { get { return Current(); } }
        public bool CanBk { get { return at > 0; } }
        public bool CanFw { get { return at < hs.Count - 1; } }
        public bool Busy { get { return busy; } }
        public int Vw
        {
            get { return vw; }
            set
            {
                var next = Math.Max(1, Math.Min(MaxViewport, value));
                if (vw == next) return;
                vw = next;
                ApplyViewport(At);
            }
        }
        public int Cm
        {
            get { return cm; }
            set { cm = Math.Max(0, Math.Min(16, value)); TrimCache(); }
        }
        public int Cc { get { return cache.Count; } }
        public int Ch { get { return ch; } }
        public string Cd
        {
            get { return disk.P; }
            set
            {
                var prior = disk.P;
                disk.P = value;
                if (!String.Equals(prior, disk.P, StringComparison.OrdinalIgnoreCase)) Cl();
            }
        }
        public int Cx { get { return disk.Mx; } set { disk.Mx = value; } }
        public long Cb { get { return disk.Mb; } set { disk.Mb = value; } }
        public int Dc { get { return disk.N; } }
        public long Db { get { return disk.B; } }
        public int Kc { get { return web.Kc; } }
        public int Km { get { return W.CookieMax; } }
        public long Dm { get { return MaxDownload; } }
        public int Tm
        {
            get { return web.Tm; }
            set { web.Tm = Math.Max(MinTimeout, Math.Min(MaxTimeout, value)); }
        }
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
            set
            {
                var next = Math.Max(0, Math.Min(86400, value));
                if (cs == next) return;
                cs = next;
                Cl();
                Cdl();
            }
        }

        public void Cl()
        {
            cache.Clear();
            lru.Clear();
        }

        public void Cdl()
        {
            disk.Cl();
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
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
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
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            if (element == null || !Owns(page.D, element)) return 0;
            var code = element.Av("data-lea");
            if (String.IsNullOrWhiteSpace(code)) code = element.Av("onclick");
            return String.IsNullOrWhiteSpace(code) ? 0 : Js(page, code);
        }

        // Kp takes and clears one page-command focus request.
        public N Kp(P page)
        {
            QueryPage(page);
            var target = page.Kq;
            page.Kq = null;
            return target;
        }

        // Ds toggles the bounded open state of one details disclosure.
        public int Ds(P page, N element)
        {
            QueryPage(page);
            var details = Disclosure(element);
            if (details == null) return 0;
            var rules = page.R as List<R>;
            if (rules == null) return 0;
            details.Op = !details.Op;
            if (details.Op) details.A["open"] = "";
            else details.A.Remove("open");
            var fields = new Dictionary<N, Fi>();
            foreach (var field in page.Fs) if (field.El != null) fields[field.El] = field;
            Rebuild(page, rules, fields);
            return 1;
        }

        // Rv reveals a target hidden by closed details in one retained rebuild.
        public int Rv(P page, N element)
        {
            QueryPage(page);
            if (element == null || !Owns(page.D, element)) return 0;
            var rules = page.R as List<R>;
            if (rules == null) return 0;
            var count = 0;
            for (var current = element.Up; current != null; current = current.Up)
            {
                if (current.K != "details" || current.Op) continue;
                current.Op = true;
                current.A["open"] = "";
                count++;
            }
            if (count == 0) return 0;
            var fields = new Dictionary<N, Fi>();
            foreach (var field in page.Fs) if (field.El != null) fields[field.El] = field;
            Rebuild(page, rules, fields);
            return count;
        }

        // Gt returns and clears one bounded page navigation request.
        public Uri Gt(P page)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            if (!page.Gp) return null;
            var target = page.Gu;
            page.Gu = null;
            page.Gp = false;
            return target;
        }

        // Dg captures bounded page and engine diagnostics for developer tools.
        public G Dg(P page)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
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
                Vw = page.Vw,
                Vc = page.S == null ? 0 : page.S.Vc,
                Cc = Cc,
                Ch = Ch,
                Kc = Kc,
                Dc = Dc,
                Db = Db,
                Dh = disk.H,
                Dw = disk.W,
                Dr = disk.R,
                De = disk.E
            };
            DiagnosticNodes(page.D, 0, result.Ns);
            if (rules != null)
                foreach (var rule in rules)
                {
                    foreach (var declaration in rule.Ds) if (declaration.I) result.Ic++;
                    if (result.Cs.Count < MaxDiagnosticRules)
                        result.Cs.Add((rule.Md.Length == 0 ? "" : rule.Md + " ") + Short(rule.Q, 96) + " { " + Short(rule.V, 144) + " } [" + rule.W + "]");
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

        // Axs exposes Lea's bounded semantic accessibility list.
        public IList<Ax> Axs(P page)
        {
            QueryPage(page);
            return page.Axs;
        }

        // Ax returns the first semantic record owned by one document element.
        public Ax Ax(P page, N element)
        {
            QueryPage(page);
            if (element == null || !Owns(page.D, element)) return null;
            foreach (var item in page.Axs)
                if (Object.ReferenceEquals(item.El, element)) return item;
            return null;
        }

        // Ti parses one bounded authored tabindex; null means omitted or invalid.
        public int? Ti(N element)
        {
            if (element == null || !element.A.ContainsKey("tabindex")) return null;
            var value = element.Av("tabindex").Trim();
            if (value.Length == 0 || value.Length > 16) return null;
            var digit = value[0] == '+' || value[0] == '-' ? 1 : 0;
            if (digit == value.Length) return null;
            for (var index = digit; index < value.Length; index++)
                if (value[index] < '0' || value[index] > '9') return null;
            long number;
            if (!Int64.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                return value[0] == '-' ? -MaxTabIndex : MaxTabIndex;
            return (int)Math.Max(-MaxTabIndex, Math.Min(MaxTabIndex, number));
        }

        // Aks returns bounded, unique ASCII access-key alternatives in authored order.
        public IList<string> Aks(N element)
        {
            var result = new List<string>();
            if (element == null || !element.A.ContainsKey("accesskey")) return result.AsReadOnly();
            var source = element.Av("accesskey");
            if (source.Length == 0 || source.Length > MaxAccessKeyText) return result.AsReadOnly();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var at = 0;
            while (at < source.Length && result.Count < MaxAccessKeyTokens)
            {
                while (at < source.Length && AsciiWhite(source[at])) at++;
                var start = at;
                while (at < source.Length && !AsciiWhite(source[at])) at++;
                if (at - start != 1) continue;
                var value = source[start];
                if (!((value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z') || (value >= '0' && value <= '9'))) continue;
                var key = Char.ToUpperInvariant(value).ToString();
                if (used.Add(key)) result.Add(key);
            }
            return result.AsReadOnly();
        }

        // Ak returns the first usable access-key alternative, or an empty string.
        public string Ak(N element)
        {
            var keys = Aks(element);
            return keys.Count == 0 ? "" : keys[0];
        }

        // Afs returns at most 32 authored autofocus candidates in document order.
        public IList<N> Afs(P page)
        {
            QueryPage(page);
            var result = new List<N>();
            FindAttributes(page.D, "autofocus", result, MaxAutofocusCandidates);
            return result.AsReadOnly();
        }

        public N Af(P page)
        {
            var candidates = Afs(page);
            return candidates.Count == 0 ? null : candidates[0];
        }

        // Id finds the first element with a matching HTML id.
        public N Id(P page, string id)
        {
            QueryPage(page);
            var value = id ?? "";
            return value.Length == 0 || value.Length > MaxHtmlAttributeValue ? null : FindId(page.D, value);
        }

        // Ft resolves one bounded, percent-decoded fragment target.
        public N Ft(P page, string fragment)
        {
            QueryPage(page);
            var value = fragment ?? "";
            if (value.StartsWith("#", StringComparison.Ordinal)) value = value.Substring(1);
            if (value.Length == 0 || value.Length > MaxFragment) return null;
            try { value = Uri.UnescapeDataString(value); }
            catch (UriFormatException) { return null; }
            if (value.Length == 0 || value.Length > MaxFragment) return null;
            return FindId(page.D, value) ?? FindName(page.D, value);
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
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
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

        // Fg records one same-document fragment navigation without fetching.
        public int Fg(string input)
        {
            return Fg(ToUri(input, At == null ? null : At.U));
        }

        public int Fg(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            var page = At;
            if (page == null || !SameDocument(page.U, url) || !FragmentNavigation(page.U, url)) return 0;
            if (!String.Equals(page.U.AbsoluteUri, url.AbsoluteUri, StringComparison.OrdinalIgnoreCase)) Push(page, url);
            return 1;
        }

        public P Go(string input)
        {
            var url = ToUri(input, At == null ? null : At.U);
            if (FragmentReference(input) && Fg(url) != 0) return At;
            return Go(url);
        }

        public P Go(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (!SameAddress(At == null ? null : At.U, url) && Fg(url) != 0) return At;
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
            var url = ToUri(input, At == null ? null : At.U);
            if (FragmentReference(input) && Fg(url) != 0) return At;
            return await GoA(url);
        }

        public async Task<P> GoA(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (!SameAddress(At == null ? null : At.U, url) && Fg(url) != 0) return At;
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
            Push(p, p == null ? null : p.U);
        }

        private void Push(P p, Uri url)
        {
            if (at < hs.Count - 1) hs.RemoveRange(at + 1, hs.Count - at - 1);
            hs.Add(new He(p, url));
            at = hs.Count - 1;
            if (hs.Count <= MaxHistory) return;
            hs.RemoveAt(0);
            at--;
        }

        public P Bk()
        {
            if (CanBk) at--;
            var page = At;
            ApplyViewport(page);
            return page;
        }

        public P Fw()
        {
            if (CanFw) at++;
            var page = At;
            ApplyViewport(page);
            return page;
        }

        public P Rl()
        {
            if (At == null) return null;
            var p = Read(At.U, true);
            hs[at] = new He(p, p.U);
            return p;
        }

        public async Task<P> RlA()
        {
            if (At == null) return null;
            var p = await ReadA(At.U, true);
            hs[at] = new He(p, p.U);
            return p;
        }

        private P Current()
        {
            if (at < 0 || at >= hs.Count) return null;
            var entry = hs[at];
            entry.P.U = entry.U;
            return entry.P;
        }

        private bool OwnsPage(P page)
        {
            foreach (var entry in hs) if (Object.ReferenceEquals(entry.P, page)) return true;
            return false;
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
                    : await NetA(web.OpenReadTaskAsync(url));
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

        private async Task<long> CopyDownloadA(Stream input, Stream output, Uri url)
        {
            var buffer = new byte[32768];
            long total = 0;
            int read;
            while ((read = url.IsFile
                ? await input.ReadAsync(buffer, 0, buffer.Length)
                : await NetA(input.ReadAsync(buffer, 0, buffer.Length), delegate { input.Dispose(); })) > 0)
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
                return RequestedFragment(Parse(Fetch(url, fresh)), url);
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
            stopped = false;
            try
            {
                return RequestedFragment(await ParseA(await FetchA(url, fresh)), url);
            }
            catch (Exception error)
            {
                var typed = LoadError(error, url);
                if (typed == null) throw;
                throw typed;
            }
            finally
            {
                stopped = false;
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
            stopped = false;
            try
            {
                DropCache(url);
                web.Headers[HttpRequestHeader.ContentType] = BodyType(type);
                var bytes = await NetA(web.UploadDataTaskAsync(url, "POST", data ?? new byte[0]));
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
                stopped = false;
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
            Cr stale = null;
            var diskHit = false;
            if (!url.IsFile && !fresh && GetCache(url, out reply, out stale, out diskHit)) return Track("GET", new Ld(reply, true), diskHit ? "disk" : "cache");
            if (url.IsFile) return Track("GET", new Ld(ReadFile(url), false), "file");

            if (fresh) stale = null;
            var staleDisk = stale != null && diskHit;
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
                return Track("GET", new Ld(reply, notModified), notModified ? staleDisk ? "disk304" : "304" : "net");
            }
            finally { web.Va("", ""); }
        }

        private async Task<Ld> FetchA(Uri url, bool fresh = false)
        {
            Cr reply;
            Cr stale = null;
            var diskHit = false;
            if (!url.IsFile && !fresh && GetCache(url, out reply, out stale, out diskHit)) return Track("GET", new Ld(reply, true), diskHit ? "disk" : "cache");
            if (url.IsFile) return Track("GET", new Ld(await Task.Factory.StartNew(delegate { return ReadFile(url); }), false), "file");

            if (fresh) stale = null;
            var staleDisk = stale != null && diskHit;
            web.Va(stale == null ? "" : stale.Et, stale == null ? "" : stale.Lm);
            byte[] bytes;
            try
            {
                try { bytes = await NetA(web.DownloadDataTaskAsync(url)); }
                catch (WebException error)
                {
                    if (stale == null || !web.Nm(error)) throw;
                    bytes = new byte[0];
                }
                var notModified = web.St == 304 && stale != null;
                reply = notModified ? Revalidated(stale) : ReadWeb(url, bytes);
                PutCache(url, reply);
                if (notModified) ch++;
                return Track("GET", new Ld(reply, notModified), notModified ? staleDisk ? "disk304" : "304" : "net");
            }
            finally { web.Va("", ""); }
        }

        private async Task<T> NetA<T>(Task<T> work, Action cancel = null)
        {
            using (var timer = new CancellationTokenSource())
            {
                try
                {
                    var winner = await Task.WhenAny(work, Task.Delay(Tm, timer.Token));
                    if (winner != work && !work.IsCompleted)
                    {
                        if (cancel != null) cancel();
                        else web.CancelAsync();
                        Observe(work);
                        throw new WebException("Lea network request timed out.", stopped ? WebExceptionStatus.RequestCanceled : WebExceptionStatus.Timeout);
                    }
                    return await work;
                }
                finally { timer.Cancel(); }
            }
        }

        private static void Observe(Task work)
        {
            if (work == null) return;
            work.ContinueWith(delegate(Task completed)
            {
                var ignored = completed.Exception;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private P Parse(Ld load)
        {
            var page = Page(load);
            Complete(page, Css(page.D, page.Bu, StyleSheets(page.D, page.Bu)));
            return page;
        }

        private async Task<P> ParseA(Ld load)
        {
            var page = Page(load);
            Complete(page, Css(page.D, page.Bu, await StyleSheetsA(page.D, page.Bu)));
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
            p.D.Bu = p.Bu;
            var title = Find(p.D, "title");
            p.T = Clean(NodeText(title));
            if (p.T.Length == 0) p.T = reply.U.IsFile ? Path.GetFileName(reply.U.LocalPath) : reply.U.Host;

            return p;
        }

        private static P RequestedFragment(P page, Uri requested)
        {
            if (page == null || page.U == null || requested == null || requested.Fragment.Length == 0 || page.U.Fragment.Length > 0) return page;
            page.U = new Uri(CacheKey(page.U) + requested.Fragment);
            return page;
        }

        private void Complete(P p, List<R> rules)
        {
            p.Ra = rules;
            p.Vw = vw;
            var active = MediaRules(rules, vw);
            p.R = active;
            Rebuild(p, active, null);
            if (!je) return;
            p.Jc = J.Pg(p, p.D);
            if (p.Jc > 0) Rebuild(p, active, null);
        }

        private void ApplyViewport(P page)
        {
            if (page == null) return;
            page.Vw = vw;
            var all = page.Ra as List<R>;
            if (all == null) return;
            var active = MediaRules(all, vw);
            var current = page.R as List<R>;
            if (SameRules(current, active)) return;
            var fields = new Dictionary<N, Fi>();
            foreach (var field in page.Fs) if (field.El != null) fields[field.El] = field;
            page.R = active;
            Rebuild(page, active, fields);
        }

        private static List<R> MediaRules(List<R> rules, int width)
        {
            var active = new List<R>();
            foreach (var rule in rules)
                if (rule.On(width)) active.Add(rule);
            return active;
        }

        private static bool SameRules(IList<R> left, IList<R> right)
        {
            if (left == null || right == null || left.Count != right.Count) return false;
            for (var index = 0; index < left.Count; index++)
                if (!Object.ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        private static void Rebuild(P p, List<R> rules, IDictionary<N, Fi> fields)
        {
            p.Ls.Clear();
            p.Axs.Clear();
            p.Bs.Clear();
            p.Fs.Clear();
            p.Dvs.Clear();
            p.Mps.Clear();
            if (fields != null)
                foreach (var pair in fields) p.Fr[pair.Key] = pair.Value;
            var body = Find(p.D, "body") ?? p.D;
            p.X = Clean(NodeText(body));
            p.N = Count(p.D);
            var inherited = new S();
            var ancestors = new List<N>();
            for (var parent = body.Up; parent != null && parent.K != "#doc"; parent = parent.Up) ancestors.Add(parent);
            for (var i = ancestors.Count - 1; i >= 0; i--) inherited = Style(ancestors[i], inherited, rules);
            p.S = Style(body, inherited, rules);
            MarkLists(body);
            if (p.H.IndexOf("<map", StringComparison.OrdinalIgnoreCase) >= 0) BuildMaps(p, body, p.Bu ?? p.U);
            foreach (var child in body.C) Blocks(p, child, p.Bu ?? p.U, rules, p.S);
            LinkBlocks(p);
            MarkDisclosures(p);
            BuildAccessibility(p, body, rules);
        }

        private static void BuildAccessibility(P page, N body, List<R> rules)
        {
            var data = new Axc(page);
            IndexAccessibility(page.D, data);
            foreach (var field in page.Fs)
                if (field.El != null && !data.Fs.ContainsKey(field.El)) data.Fs[field.El] = field;
            foreach (var map in page.Mps)
                foreach (var area in map.As)
                    if (area.El != null) data.As.Add(area.El);

            var root = new Ax { K = "document", N = BoundValue(page.T, MaxAccessibilityText), El = body };
            page.Axs.Add(root);
            foreach (var child in body.C) AddAccessibility(data, child, page.S, rules, root, false);
        }

        private static void IndexAccessibility(N node, Axc data)
        {
            if (node == null) return;
            var id = node.Av("id");
            if (id.Length > 0 && id.Length <= MaxHtmlAttributeValue && !data.Ids.ContainsKey(id)) data.Ids[id] = node;
            if (node.K == "label")
            {
                var target = node.Av("for");
                List<N> labels;
                if (target.Length > 0 && target.Length <= MaxHtmlAttributeValue)
                {
                    if (!data.Lbs.TryGetValue(target, out labels)) data.Lbs[target] = labels = new List<N>();
                    if (labels.Count < MaxAccessibilityReferences) labels.Add(node);
                }
            }
            foreach (var child in node.C) IndexAccessibility(child, data);
        }

        private static void AddAccessibility(Axc data, N node, S inherited, List<R> rules, Ax parent, bool suppressText)
        {
            if (node == null || data.P.Axs.Count == MaxAccessibilityNodes) return;
            if (node.K == "#text")
            {
                var text = BoundValue(Clean(node.X), MaxAccessibilityText);
                if (!suppressText && text.Length > 0) data.P.Axs.Add(new Ax { K = "text", N = text, El = node, Up = parent });
                return;
            }
            if (node.K == "head" || node.K == "script" || node.K == "style" || node.K == "noscript" || node.K == "template" || node.A.ContainsKey("hidden")) return;

            var style = Style(node, inherited, rules);
            if (style.D == "none" || DisclosureHidden(node) || AriaTrue(node, "aria-hidden")) return;
            if (node.K == "area" && !data.As.Contains(node)) return;

            if (style.Vi == "h" || style.Vi == "c")
            {
                foreach (var child in node.C)
                    if (child.K != "#text") AddAccessibility(data, child, style, rules, parent, suppressText);
                return;
            }

            var role = AccessibilityRole(node, data);
            var current = parent;
            if (role != "none" && role.Length > 0)
            {
                var name = AccessibilityName(node, role, data);
                if ((role == "form" || role == "region") && name.Length == 0) role = "";
                if (role.Length > 0)
                {
                    current = new Ax
                    {
                        K = role,
                        N = name,
                        D = AccessibilityDescription(node, name, data),
                        V = AccessibilityValue(node, data),
                        St = AccessibilityState(node, role, data),
                        Lv = HeadingLevel(node, role),
                        U = AccessibilityUrl(node, data.P.Bu ?? data.P.U),
                        El = node,
                        Up = parent,
                        F = AccessibilityField(node, data)
                    };
                    data.P.Axs.Add(current);
                }
            }

            var childParent = current;
            var childSuppress = suppressText || current != parent && AccessibilityConsumesText(role);
            foreach (var child in node.C)
            {
                if (data.P.Axs.Count == MaxAccessibilityNodes) break;
                AddAccessibility(data, child, style, rules, childParent, childSuppress);
            }
        }

        private static string AccessibilityRole(N node, Axc data)
        {
            var explicitRole = ExplicitAccessibilityRole(node.Av("role"));
            if (explicitRole.Length > 0) return explicitRole;
            var kind = node.K;
            if (kind == "a" && node.A.ContainsKey("href") || kind == "area" && node.A.ContainsKey("href")) return "link";
            if (kind == "button" || kind == "summary") return "button";
            if (kind == "input" || kind == "textarea" || kind == "select")
            {
                var field = AccessibilityField(node, data);
                if (field == null) return "";
                if (field.K == "checkbox") return "checkbox";
                if (field.K == "radio") return "radio";
                if (field.K == "range") return "slider";
                if (field.K == "number") return "spinbutton";
                if (field.K == "search") return "searchbox";
                if (field.K == "select") return field.Mu ? "listbox" : "combobox";
                if (field.K == "submit" || field.K == "reset" || field.K == "button" || field.K == "file" || field.K == "color") return "button";
                return "textbox";
            }
            if (IsHeading(kind)) return "heading";
            if (kind == "img") return node.Av("alt").Length == 0 && !node.A.ContainsKey("alt") ? "img" : node.Av("alt").Length == 0 ? "none" : "img";
            if (kind == "svg") return "img";
            if (kind == "nav") return "navigation";
            if (kind == "main") return "main";
            if (kind == "aside") return "complementary";
            if (kind == "header") return HasSectionAncestor(node) ? "group" : "banner";
            if (kind == "footer") return HasSectionAncestor(node) ? "group" : "contentinfo";
            if (kind == "form") return "form";
            if (kind == "section") return "region";
            if (kind == "article") return "article";
            if (kind == "figure") return "figure";
            if (kind == "details" || kind == "fieldset") return "group";
            if (kind == "ol" || kind == "ul" || kind == "menu") return "list";
            if (kind == "li") return "listitem";
            if (kind == "table") return "table";
            if (kind == "thead" || kind == "tbody" || kind == "tfoot") return "rowgroup";
            if (kind == "tr") return "row";
            if (kind == "th") return node.Av("scope").Equals("row", StringComparison.OrdinalIgnoreCase) ? "rowheader" : "columnheader";
            if (kind == "td") return "cell";
            if (kind == "caption") return "caption";
            if (kind == "progress") return "progressbar";
            if (kind == "meter") return "meter";
            if (kind == "output") return "status";
            if (kind == "hr") return "separator";
            if (kind == "p") return "paragraph";
            if (kind == "blockquote") return "blockquote";
            if (kind == "code" || kind == "pre") return "code";
            if (kind == "dt") return "term";
            if (kind == "dd") return "definition";
            if (kind == "figcaption" || kind == "legend") return "caption";
            if (kind == "time") return "time";
            if (kind == "ins") return "insertion";
            if (kind == "del") return "deletion";
            if (kind == "address") return "group";
            return "";
        }

        private static string ExplicitAccessibilityRole(string input)
        {
            var text = BoundValue(input, 256);
            var at = 0;
            var count = 0;
            while (at < text.Length && count++ < MaxAccessibilityReferences)
            {
                while (at < text.Length && AsciiWhite(text[at])) at++;
                var start = at;
                while (at < text.Length && !AsciiWhite(text[at])) at++;
                if (start == at) continue;
                var role = text.Substring(start, at - start).ToLowerInvariant();
                if (role == "presentation" || role == "none") return "none";
                if (Regex.IsMatch(role, "^(?:alert|article|banner|blockquote|button|caption|cell|checkbox|code|columnheader|combobox|complementary|contentinfo|definition|deletion|dialog|document|feed|figure|form|generic|group|heading|img|insertion|link|list|listbox|listitem|log|main|marquee|meter|navigation|option|paragraph|progressbar|radio|region|row|rowgroup|rowheader|search|searchbox|separator|slider|spinbutton|status|table|term|textbox|time|timer)$")) return role;
            }
            return "";
        }

        private static string AccessibilityName(N node, string role, Axc data)
        {
            var value = AccessibilityReferences(node.Av("aria-labelledby"), data);
            if (value.Length > 0) return value;
            if (node.A.ContainsKey("aria-label")) return BoundValue(Clean(node.Av("aria-label")), MaxAccessibilityText);

            var field = AccessibilityField(node, data);
            if (field != null)
            {
                value = AccessibilityLabels(node, data);
                if (value.Length > 0) return value;
                if (role == "button")
                {
                    value = node.K == "button" ? Clean(NodeText(node)) : Clean(node.Av("value"));
                    if (value.Length == 0) value = field.K == "reset" ? "Reset" : field.K == "submit" ? "Submit" : field.K == "file" ? "Choose file" : "Button";
                    return BoundValue(value, MaxAccessibilityText);
                }
                if (node.A.ContainsKey("placeholder")) return BoundValue(Clean(node.Av("placeholder")), MaxAccessibilityText);
            }
            if (role == "img" && node.A.ContainsKey("alt")) return BoundValue(Clean(node.Av("alt")), MaxAccessibilityText);
            if (role == "img" && node.K == "svg")
            {
                var title = FirstChild(node, "title");
                if (title != null) return BoundValue(Clean(NodeText(title)), MaxAccessibilityText);
            }
            if (role == "table")
            {
                var caption = FirstChild(node, "caption");
                if (caption != null) return BoundValue(Clean(NodeText(caption)), MaxAccessibilityText);
            }
            if (role == "figure")
            {
                var caption = FirstChild(node, "figcaption");
                if (caption != null) return BoundValue(Clean(NodeText(caption)), MaxAccessibilityText);
            }
            if (role == "group" && node.K == "fieldset")
            {
                var legend = FirstChild(node, "legend");
                if (legend != null) return BoundValue(Clean(NodeText(legend)), MaxAccessibilityText);
            }
            if (role == "group" && node.K == "details")
            {
                var summary = FirstChild(node, "summary");
                if (summary != null) return BoundValue(Clean(NodeText(summary)), MaxAccessibilityText);
            }
            if (AccessibilityNameFromContent(role))
            {
                value = BoundValue(Clean(NodeText(node)), MaxAccessibilityText);
                if (value.Length > 0) return value;
            }
            if (node.A.ContainsKey("title")) return BoundValue(Clean(node.Av("title")), MaxAccessibilityText);
            return "";
        }

        private static string AccessibilityDescription(N node, string name, Axc data)
        {
            var value = AccessibilityReferences(node.Av("aria-describedby"), data);
            if (value.Length > 0) return value;
            if (node.A.ContainsKey("aria-description")) return BoundValue(Clean(node.Av("aria-description")), MaxAccessibilityText);
            value = BoundValue(Clean(node.Av("title")), MaxAccessibilityText);
            return value == name ? "" : value;
        }

        private static string AccessibilityReferences(string input, Axc data)
        {
            var result = new StringBuilder();
            var text = BoundValue(input, MaxHtmlAttributeValue);
            var used = new HashSet<N>();
            var at = 0;
            var count = 0;
            while (at < text.Length && count < MaxAccessibilityReferences)
            {
                while (at < text.Length && AsciiWhite(text[at])) at++;
                var start = at;
                while (at < text.Length && !AsciiWhite(text[at])) at++;
                if (start == at) continue;
                N target;
                if (!data.Ids.TryGetValue(text.Substring(start, at - start), out target) || !used.Add(target)) continue;
                var value = Clean(NodeText(target));
                if (value.Length == 0) continue;
                if (result.Length > 0) result.Append(' ');
                result.Append(value);
                count++;
                if (result.Length >= MaxAccessibilityText) break;
            }
            return BoundValue(result.ToString(), MaxAccessibilityText);
        }

        private static string AccessibilityLabels(N node, Axc data)
        {
            var result = new StringBuilder();
            List<N> labels;
            var id = node.Av("id");
            if (id.Length > 0 && data.Lbs.TryGetValue(id, out labels))
                foreach (var label in labels) AppendAccessibilityText(result, NodeText(label));
            for (var current = node.Up; current != null; current = current.Up)
            {
                if (current.K != "label") continue;
                AppendAccessibilityText(result, NodeText(current));
                break;
            }
            return BoundValue(result.ToString(), MaxAccessibilityText);
        }

        private static void AppendAccessibilityText(StringBuilder result, string input)
        {
            var value = Clean(input);
            if (value.Length == 0 || result.Length >= MaxAccessibilityText) return;
            if (result.Length > 0) result.Append(' ');
            result.Append(value);
        }

        private static string AccessibilityValue(N node, Axc data)
        {
            var field = AccessibilityField(node, data);
            if (field != null) return "";
            var value = node.Av("aria-valuetext");
            if (value.Length == 0) value = node.Av("aria-valuenow");
            if (value.Length > 0) return BoundValue(Clean(value), MaxAccessibilityText);
            foreach (var block in data.P.Bs)
            {
                if (!Object.ReferenceEquals(block.El, node)) continue;
                if (block.Vm != null) return block.Vm.V.ToString("G", CultureInfo.InvariantCulture);
                if (block.Dv != null) return BoundValue(Clean(block.Dv.V), MaxAccessibilityText);
            }
            return "";
        }

        private static int AccessibilityState(N node, string role, Axc data)
        {
            var state = 0;
            var field = AccessibilityField(node, data);
            if (field != null && !field.Ds || field == null && AccessibilityFocusable(node)) state |= Lea.Engine.Ax.Fs;
            if (node.Fc) state |= Lea.Engine.Ax.Fc;
            if ((field == null && node.A.ContainsKey("disabled")) || AriaTrue(node, "aria-disabled")) state |= Lea.Engine.Ax.Ds;
            if ((field == null && node.A.ContainsKey("checked")) || AriaTrue(node, "aria-checked")) state |= Lea.Engine.Ax.Ck;
            if (node.Av("aria-checked").Equals("mixed", StringComparison.OrdinalIgnoreCase)) state |= Lea.Engine.Ax.Mx;
            if (node.A.ContainsKey("selected") || AriaTrue(node, "aria-selected")) state |= Lea.Engine.Ax.Sl;
            if ((field == null && node.A.ContainsKey("required")) || AriaTrue(node, "aria-required")) state |= Lea.Engine.Ax.Rq;
            if ((field == null && node.A.ContainsKey("readonly")) || AriaTrue(node, "aria-readonly")) state |= Lea.Engine.Ax.Ro;
            var invalid = node.Av("aria-invalid");
            if (invalid.Length > 0 && !invalid.Equals("false", StringComparison.OrdinalIgnoreCase)) state |= Lea.Engine.Ax.Iv;
            if (node.A.ContainsKey("aria-expanded")) state |= AriaTrue(node, "aria-expanded") ? Lea.Engine.Ax.Ex : Lea.Engine.Ax.Co;
            if (node.K == "details" || node.K == "summary") state |= (node.K == "details" ? node : node.Up).Op ? Lea.Engine.Ax.Ex : Lea.Engine.Ax.Co;
            var pressed = node.Av("aria-pressed");
            if (pressed.Equals("mixed", StringComparison.OrdinalIgnoreCase)) state |= Lea.Engine.Ax.Mx | Lea.Engine.Ax.Ps;
            else if (pressed.Equals("true", StringComparison.OrdinalIgnoreCase)) state |= Lea.Engine.Ax.Ps;
            if (AriaTrue(node, "aria-busy")) state |= Lea.Engine.Ax.Bs;
            var current = node.Av("aria-current");
            if (current.Length > 0 && !current.Equals("false", StringComparison.OrdinalIgnoreCase)) state |= Lea.Engine.Ax.Cr;
            return state;
        }

        private static bool AccessibilityFocusable(N node)
        {
            if (node == null) return false;
            if ((node.K == "a" || node.K == "area") && node.A.ContainsKey("href") || node.K == "button" || node.K == "summary") return true;
            if (!node.A.ContainsKey("tabindex")) return false;
            var value = node.Av("tabindex").Trim();
            int number;
            return value.Length > 0 && value.Length <= 16 && Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
        }

        private static Fi AccessibilityField(N node, Axc data)
        {
            Fi field;
            return node != null && data.Fs.TryGetValue(node, out field) ? field : null;
        }

        private static Uri AccessibilityUrl(N node, Uri baseUrl)
        {
            if (node == null || baseUrl == null || (node.K != "a" && node.K != "area")) return null;
            Uri result;
            return TryUri(baseUrl, node.Av("href"), out result) ? result : null;
        }

        private static bool AriaTrue(N node, string name)
        {
            return node.Av(name).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool AccessibilityConsumesText(string role)
        {
            return AccessibilityNameFromContent(role) || role == "img" || role == "textbox" || role == "searchbox" || role == "combobox" || role == "listbox";
        }

        private static bool AccessibilityNameFromContent(string role)
        {
            return role == "button" || role == "link" || role == "heading" || role == "paragraph" || role == "blockquote" || role == "code" ||
                   role == "listitem" || role == "cell" || role == "columnheader" || role == "rowheader" || role == "caption" || role == "option" ||
                   role == "status" || role == "term" || role == "definition" || role == "time" || role == "insertion" || role == "deletion";
        }

        private static int HeadingLevel(N node, string role)
        {
            int level;
            if (role != "heading") return 0;
            if (node.K.Length == 2 && node.K[0] == 'h' && node.K[1] >= '1' && node.K[1] <= '6') return node.K[1] - '0';
            return Int32.TryParse(node.Av("aria-level"), out level) ? Math.Max(1, Math.Min(9, level)) : 2;
        }

        private static N FirstChild(N node, string kind)
        {
            if (node == null) return null;
            foreach (var child in node.C) if (child.K == kind) return child;
            return null;
        }

        private static bool HasSectionAncestor(N node)
        {
            for (var current = node.Up; current != null; current = current.Up)
                if (current.K == "article" || current.K == "aside" || current.K == "main" || current.K == "nav" || current.K == "section") return true;
            return false;
        }

        private sealed class Axc
        {
            public readonly P P;
            public readonly Dictionary<string, N> Ids = new Dictionary<string, N>(StringComparer.Ordinal);
            public readonly Dictionary<string, List<N>> Lbs = new Dictionary<string, List<N>>(StringComparer.Ordinal);
            public readonly Dictionary<N, Fi> Fs = new Dictionary<N, Fi>();
            public readonly HashSet<N> As = new HashSet<N>();

            public Axc(P page) { P = page; }
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
            return style != null && (Flex(style) || Grid(style) || style.Pn == "r" || style.Pn == "a" || style.Pn == "f" || style.Zc || (style.Ov.Length > 0 && style.Ov != "v"));
        }

        private static bool Flex(S style)
        {
            return style != null && (style.D == "flex" || style.D == "inline-flex");
        }

        private static bool Grid(S style)
        {
            return style != null && (style.D == "grid" || style.D == "inline-grid");
        }

        private static bool BlockContext(B block)
        {
            return block != null && (LayoutContext(block.S) || IsContainerBlock(block.K));
        }

        private static bool VisualContext(S style)
        {
            return LayoutContext(style) || style != null &&
                (style.Bg.Length > 0 || style.Gi != null || style.Bi != null && style.Bi.U != null ||
                 style.Mt != 0 || style.Mr != 0 || style.Mb != 0 || style.Ml != 0 ||
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
                    if (node.K == "details") node.Op = node.A.ContainsKey("open");
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
                   tag == "meter" || tag == "nav" || tag == "ol" || tag == "p" || tag == "pre" || tag == "progress" || tag == "section" || tag == "summary" ||
                   tag == "svg" || tag == "table" || tag == "ul";
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

        private Dictionary<N, Sc> StyleSheets(N root, Uri baseUrl)
        {
            var sheets = new Dictionary<N, Sc>();
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
                    sheets[link] = new Sc(load.R.H, load.R.U);
                }
                catch (WebException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                catch (NotSupportedException) { }
                catch (ArgumentException) { }
            }
            return sheets;
        }

        private async Task<Dictionary<N, Sc>> StyleSheetsA(N root, Uri baseUrl)
        {
            var sheets = new Dictionary<N, Sc>();
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
                    sheets[link] = new Sc(load.R.H, load.R.U);
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
                if (link.A.ContainsKey("disabled") || CssMedia(media).Length == 0 || (type.Length > 0 && type != "text/css")) continue;
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

        private static List<R> Css(N root, Uri baseUrl, IDictionary<N, Sc> sheets)
        {
            var rules = new List<R>();
            var order = 0;
            CssRules(root, baseUrl, sheets, rules, ref order);
            rules.Sort(delegate(R a, R b) { var byWeight = a.W.CompareTo(b.W); return byWeight != 0 ? byWeight : a.O.CompareTo(b.O); });
            return rules;
        }

        private static void CssRules(N node, Uri baseUrl, IDictionary<N, Sc> sheets, List<R> rules, ref int order)
        {
            string source = null;
            var origin = baseUrl;
            if (node.K == "style") source = RawText(node);
            else if (sheets != null)
            {
                Sc sheet;
                if (sheets.TryGetValue(node, out sheet) && sheet != null) { source = sheet.X; origin = sheet.U; }
            }
            if (source != null)
            {
                var media = CssMedia(node.Av("media"));
                if (media.Length > 0) CssSource(source, origin, rules, ref order, media, 0);
            }
            foreach (var child in node.C) CssRules(child, baseUrl, sheets, rules, ref order);
        }

        private static void CssSource(string source, Uri origin, List<R> rules, ref int order, Mc[] media, int depth)
        {
            if (String.IsNullOrEmpty(source) || media == null || media.Length == 0 || depth > MaxMediaDepth) return;
            var at = 0;
            string header;
            string body;
            while (rules.Count < MaxCssRules && CssBlock(source, ref at, out header, out body))
            {
                var query = CssClean(header).Trim();
                if (query.Length == 0) continue;
                if (Regex.IsMatch(query, "^@media(?:\\s|\\()", RegexOptions.IgnoreCase))
                {
                    if (depth == MaxMediaDepth) continue;
                    var nested = CssIntersect(media, CssMedia(Regex.Replace(query, "^@media", "", RegexOptions.IgnoreCase)));
                    if (nested.Length > 0) CssSource(body, origin, rules, ref order, nested, depth + 1);
                    continue;
                }
                if (query[0] == '@') continue;
                CssRule(query, CssClean(body), origin, rules, ref order, media);
            }
        }

        private static void CssRule(string selectors, string declarations, Uri origin, List<R> rules, ref int order, Mc[] media)
        {
            if (String.IsNullOrWhiteSpace(declarations) || declarations.Length > 64 * 1024) return;
            foreach (var selector in CssGroups(selectors))
            {
                if (rules.Count == MaxCssRules) return;
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
                    rules.Add(new R(query, declarations, origin, Spec(simple), order++, simple.ToArray(), joins.ToArray(), media));
            }
        }

        private static bool CssBlock(string source, ref int at, out string header, out string body)
        {
            header = "";
            body = "";
            while (at < (source ?? "").Length)
            {
                while (at < source.Length && (Char.IsWhiteSpace(source[at]) || source[at] == ';')) at++;
                if (at >= source.Length || source[at] == '}') return false;
                var start = at;
                var quote = '\0';
                var brackets = 0;
                var parentheses = 0;
                var statement = false;
                for (var index = at; index < source.Length; index++)
                {
                    var value = source[index];
                    if (quote != '\0')
                    {
                        if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                        continue;
                    }
                    if (value == '/' && index + 1 < source.Length && source[index + 1] == '*')
                    {
                        index = CssComment(source, index);
                        continue;
                    }
                    if (value == '"' || value == '\'') { quote = value; continue; }
                    if (value == '[') brackets++;
                    else if (value == ']' && brackets > 0) brackets--;
                    else if (value == '(') parentheses++;
                    else if (value == ')' && parentheses > 0) parentheses--;
                    else if (brackets == 0 && parentheses == 0 && value == ';')
                    {
                        at = index + 1;
                        statement = true;
                        break;
                    }
                    else if (brackets == 0 && parentheses == 0 && value == '{')
                    {
                        var close = CssBrace(source, index);
                        if (close < 0) { at = source.Length; return false; }
                        header = source.Substring(start, index - start);
                        body = source.Substring(index + 1, close - index - 1);
                        at = close + 1;
                        return true;
                    }
                    else if (brackets == 0 && parentheses == 0 && value == '}')
                    {
                        at = index + 1;
                        return false;
                    }
                }
                if (!statement) { at = source.Length; return false; }
            }
            return false;
        }

        private static int CssBrace(string source, int open)
        {
            var depth = 1;
            var quote = '\0';
            for (var index = open + 1; index < (source ?? "").Length; index++)
            {
                var value = source[index];
                if (quote != '\0')
                {
                    if (value == quote && source[index - 1] != '\\') quote = '\0';
                    continue;
                }
                if (value == '/' && index + 1 < source.Length && source[index + 1] == '*')
                {
                    index = CssComment(source, index);
                    continue;
                }
                if (value == '"' || value == '\'') quote = value;
                else if (value == '{') depth++;
                else if (value == '}' && --depth == 0) return index;
            }
            return -1;
        }

        private static int CssComment(string source, int open)
        {
            var close = source.IndexOf("*/", open + 2, StringComparison.Ordinal);
            return close < 0 ? source.Length - 1 : close + 1;
        }

        private static string CssClean(string source)
        {
            var result = new StringBuilder();
            var quote = '\0';
            for (var index = 0; index < (source ?? "").Length; index++)
            {
                var value = source[index];
                if (quote != '\0')
                {
                    result.Append(value);
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (value == '/' && index + 1 < source.Length && source[index + 1] == '*')
                {
                    index = CssComment(source, index);
                    continue;
                }
                result.Append(value);
                if (value == '"' || value == '\'') quote = value;
            }
            return result.ToString();
        }

        private static Mc[] CssMedia(string value)
        {
            var source = CssClean(value).Trim().ToLowerInvariant();
            if (source.Length == 0) return new Mc[] { new Mc(0, 0) };
            var result = new List<Mc>();
            var groups = 0;
            foreach (var item in CssGroups(source))
            {
                if (++groups > MaxMediaGroups) return new Mc[0];
                var query = item.Trim();
                if (query.Length == 0 || Regex.IsMatch(query, "(?:^|\\s)not(?:\\s|$)", RegexOptions.IgnoreCase)) continue;
                var matches = Regex.Matches(query, "\\(\\s*(?<k>min-width|max-width|width)\\s*:\\s*(?<n>[0-9]{1,4})\\s*px\\s*\\)", RegexOptions.IgnoreCase);
                var minimum = 0;
                var maximum = 0;
                var valid = true;
                foreach (Match match in matches)
                {
                    int number;
                    if (!Int32.TryParse(match.Groups["n"].Value, out number) || number < 1 || number > MaxViewport) { valid = false; break; }
                    var kind = match.Groups["k"].Value.ToLowerInvariant();
                    if (kind == "min-width") minimum = Math.Max(minimum, number);
                    else if (kind == "max-width") maximum = maximum == 0 ? number : Math.Min(maximum, number);
                    else
                    {
                        minimum = Math.Max(minimum, number);
                        maximum = maximum == 0 ? number : Math.Min(maximum, number);
                    }
                }
                if (!valid || minimum > 0 && maximum > 0 && minimum > maximum) continue;
                var remainder = Regex.Replace(query, "\\(\\s*(?:min-width|max-width|width)\\s*:\\s*[0-9]{1,4}\\s*px\\s*\\)", " ", RegexOptions.IgnoreCase);
                if (remainder.IndexOf('(') >= 0 || remainder.IndexOf(')') >= 0 || remainder.IndexOf(':') >= 0) continue;
                var meaningful = matches.Count > 0;
                foreach (var token in Regex.Split(remainder.Trim(), "\\s+"))
                {
                    if (token.Length == 0 || token == "and" || token == "only") continue;
                    if (token == "screen" || token == "all") { meaningful = true; continue; }
                    valid = false;
                    break;
                }
                if (!valid || !meaningful) continue;
                var condition = new Mc(minimum, maximum);
                var duplicate = false;
                foreach (var existing in result)
                    if (existing.N == condition.N && existing.X == condition.X) { duplicate = true; break; }
                if (!duplicate) result.Add(condition);
            }
            return result.ToArray();
        }

        private static Mc[] CssIntersect(Mc[] parent, Mc[] child)
        {
            var result = new List<Mc>();
            if (parent == null || child == null) return result.ToArray();
            foreach (var left in parent)
                foreach (var right in child)
                {
                    var minimum = Math.Max(left.N, right.N);
                    var maximum = left.X == 0 ? right.X : right.X == 0 ? left.X : Math.Min(left.X, right.X);
                    if (maximum > 0 && minimum > maximum) continue;
                    var duplicate = false;
                    foreach (var existing in result)
                        if (existing.N == minimum && existing.X == maximum) { duplicate = true; break; }
                    if (!duplicate && result.Count < MaxMediaGroups) result.Add(new Mc(minimum, maximum));
                }
            return result.ToArray();
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

        private static void FindAttributes(N node, string name, List<N> found, int maximum)
        {
            if (node == null || found.Count == maximum) return;
            if (node.K.Length > 0 && node.K[0] != '#' && node.A.ContainsKey(name)) found.Add(node);
            foreach (var child in node.C)
            {
                FindAttributes(child, name, found, maximum);
                if (found.Count == maximum) return;
            }
        }

        private static bool AsciiWhite(char value)
        {
            return value == ' ' || value == '\t' || value == '\n' || value == '\f' || value == '\r';
        }

        private static N FindName(N node, string name)
        {
            if (node == null) return null;
            if (node.K == "a" && node.Av("name").Equals(name, StringComparison.OrdinalIgnoreCase)) return node;
            foreach (var child in node.C)
            {
                var found = FindName(child, name);
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
            return CssSimple(selector, true, true);
        }

        private static Se CssSimple(string selector, bool functions, bool nthFilters)
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
            var functionTests = 0;
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
                    var function = functions ? Regex.Match(tail, "^:(?<n>not|is|where|has)\\(", RegexOptions.IgnoreCase) : null;
                    if (function != null && function.Success)
                    {
                        var open = function.Length - 1;
                        var end = CssClose(tail, open);
                        if (end < 0) return null;
                        var name = function.Groups["n"].Value.ToLowerInvariant();
                        int authored;
                        if (name == "has")
                        {
                            var relatives = CssRelativeSelectors(tail.Substring(open + 1, end - open - 1), out authored);
                            if (relatives == null || functionTests + authored > MaxCssFunctionTests) return null;
                            functionTests += authored;
                            pseudos.Add(new Ps(name, relatives));
                            var maximum = 0;
                            foreach (var relative in relatives) maximum = Math.Max(maximum, relative.W);
                            weight += maximum;
                        }
                        else
                        {
                            var alternatives = CssFunctionSelectors(tail.Substring(open + 1, end - open - 1), name != "not", out authored);
                            if (alternatives == null || functionTests + authored > MaxCssFunctionTests) return null;
                            functionTests += authored;
                            pseudos.Add(new Ps(name, alternatives));
                            if (name != "where")
                            {
                                var maximum = 0;
                                foreach (var alternative in alternatives) maximum = Math.Max(maximum, alternative.W);
                                weight += maximum;
                            }
                        }
                        index += end + 1;
                        continue;
                    }
                    var nth = Regex.Match(tail, "^:(?<n>nth-last-child|nth-child|nth-last-of-type|nth-of-type)\\(", RegexOptions.IgnoreCase);
                    if (nth.Success)
                    {
                        var open = nth.Length - 1;
                        var end = CssClose(tail, open);
                        var name = nth.Groups["n"].Value.ToLowerInvariant();
                        int a;
                        int b;
                        int authored;
                        Se[] filters;
                        if (end < 0 || !CssNth(tail.Substring(open + 1, end - open - 1), nthFilters && name.IndexOf("of-type", StringComparison.Ordinal) < 0, out a, out b, out filters, out authored) || functionTests + authored > MaxCssFunctionTests) return null;
                        functionTests += authored;
                        pseudos.Add(new Ps(name, a, b, filters));
                        var maximum = 0;
                        foreach (var filter in filters) maximum = Math.Max(maximum, filter.W);
                        weight += 10 + maximum;
                        index += end + 1;
                        continue;
                    }
                    match = Regex.Match(tail, "^:(?<n>first-child|last-child|only-child|first-of-type|last-of-type|only-of-type|empty|root|link|checked|disabled|enabled|required|read-only|read-write|valid|invalid|hover|focus)\\b", RegexOptions.IgnoreCase);
                    if (!match.Success) return null;
                    pseudos.Add(new Ps(match.Groups["n"].Value.ToLowerInvariant()));
                    weight += 10;
                }
                else return null;
                index += match.Length;
            }
            return new Se(tag, ids.ToArray(), classes.ToArray(), attrs.ToArray(), pseudos.ToArray(), weight);
        }

        private static Se[] CssFunctionSelectors(string source, bool forgiving, out int authored)
        {
            var result = new List<Se>();
            authored = 0;
            foreach (var group in CssGroups(source))
            {
                authored++;
                if (authored > MaxCssFunctionGroups) return null;
                var value = group.Trim();
                var selector = value.Length == 0 ? null : CssSimple(value, false, false);
                if (selector == null)
                {
                    if (!forgiving) return null;
                    continue;
                }
                result.Add(selector);
            }
            return result.ToArray();
        }

        private static Hr[] CssRelativeSelectors(string source, out int authored)
        {
            var result = new List<Hr>();
            authored = 0;
            var alternatives = 0;
            foreach (var group in CssGroups(source))
            {
                if (++alternatives > MaxCssFunctionGroups) return null;
                var value = group.Trim();
                if (value.Length == 0) return null;
                var leading = ' ';
                if (value[0] == '>' || value[0] == '+' || value[0] == '~')
                {
                    leading = value[0];
                    value = value.Substring(1).TrimStart();
                }
                var parts = new List<string>();
                var joins = new List<char>();
                if (value.Length == 0 || !CssParts(value, parts, joins) || parts.Count > MaxCssRelativeParts || authored + parts.Count > MaxCssFunctionTests) return null;
                var simple = new List<Se>();
                foreach (var part in parts)
                {
                    var selector = CssSimple(part, false, false);
                    if (selector == null) return null;
                    simple.Add(selector);
                }
                authored += simple.Count;
                result.Add(new Hr(leading, simple.ToArray(), joins.ToArray(), Spec(simple)));
            }
            return result.Count == 0 ? null : result.ToArray();
        }

        private static bool CssNth(string source, bool filters, out int a, out int b, out Se[] selectors, out int authored)
        {
            a = 0;
            b = 0;
            selectors = NoSelectors;
            authored = 0;
            var value = (source ?? "").Trim();
            var filtered = Regex.Match(value, "^(?<f>.+?)\\s+of\\s+(?<s>.+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (filtered.Success)
            {
                if (!filters) return false;
                value = filtered.Groups["f"].Value.Trim();
                selectors = CssFunctionSelectors(filtered.Groups["s"].Value, false, out authored);
                if (selectors == null || selectors.Length == 0) return false;
            }
            if (value.Length == 0 || value.Length > MaxNthExpression) return false;
            if (value.Equals("odd", StringComparison.OrdinalIgnoreCase)) { a = 2; b = 1; return true; }
            if (value.Equals("even", StringComparison.OrdinalIgnoreCase)) { a = 2; return true; }

            var integer = Regex.Match(value, "^(?<b>[+-]?[0-9]{1,4})$");
            if (integer.Success)
            {
                if (!Int32.TryParse(integer.Groups["b"].Value, out b)) return false;
                return Math.Abs(b) <= MaxNthValue;
            }

            var formula = Regex.Match(value, "^(?<a>[+-]?(?:[0-9]{1,4})?)n(?:\\s*(?<s>[+-])\\s*(?<b>[0-9]{1,4}))?$", RegexOptions.IgnoreCase);
            if (!formula.Success) return false;
            var coefficient = formula.Groups["a"].Value;
            if (coefficient.Length == 0 || coefficient == "+") a = 1;
            else if (coefficient == "-") a = -1;
            else if (!Int32.TryParse(coefficient, out a)) return false;
            if (formula.Groups["b"].Success)
            {
                if (!Int32.TryParse(formula.Groups["b"].Value, out b)) return false;
                if (formula.Groups["s"].Value == "-") b = -b;
            }
            return Math.Abs(a) <= MaxNthValue && Math.Abs(b) <= MaxNthValue;
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
            if (pseudo.N == "not") return pseudo.Ss.Length > 0 && !MatchAny(node, pseudo.Ss);
            if (pseudo.N == "is" || pseudo.N == "where") return MatchAny(node, pseudo.Ss);
            if (pseudo.N == "has") return MatchHas(node, pseudo.Rs);
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
            var position = ElementPosition(node, type, pseudo.Ss, out count);
            if (position == 0) return false;
            if (pseudo.N == "first-child" || pseudo.N == "first-of-type") return position == 1;
            if (pseudo.N == "last-child" || pseudo.N == "last-of-type") return position == count;
            if (pseudo.N == "only-child" || pseudo.N == "only-of-type") return count == 1;
            if (pseudo.N == "nth-child" || pseudo.N == "nth-last-child" || pseudo.N == "nth-of-type" || pseudo.N == "nth-last-of-type")
            {
                if (pseudo.N.StartsWith("nth-last", StringComparison.Ordinal)) position = count - position + 1;
                return MatchNth(position, pseudo.A, pseudo.B);
            }
            return false;
        }

        private static bool MatchNth(int position, int a, int b)
        {
            var difference = position - b;
            if (a == 0) return difference == 0;
            return difference % a == 0 && difference / a >= 0;
        }

        private static bool MatchAny(N node, Se[] selectors)
        {
            foreach (var selector in selectors)
                if (MatchPart(node, selector)) return true;
            return false;
        }

        private static bool MatchHas(N anchor, Hr[] relatives)
        {
            var visits = 0;
            foreach (var relative in relatives)
            {
                if (relative.P.Length == 1)
                {
                    if (relative.L == '>')
                    {
                        foreach (var child in anchor.C)
                            if (ElementCandidate(child, anchor, relative, ref visits)) return true;
                    }
                    else if (relative.L == '+')
                    {
                        if (ElementCandidate(NextElement(anchor), anchor, relative, ref visits)) return true;
                    }
                    else if (relative.L == '~')
                    {
                        var following = false;
                        if (anchor.Up != null)
                            foreach (var child in anchor.Up.C)
                            {
                                if (Object.ReferenceEquals(child, anchor)) { following = true; continue; }
                                if (following && ElementCandidate(child, anchor, relative, ref visits)) return true;
                            }
                    }
                    else
                    {
                        foreach (var child in anchor.C)
                            if (RelativeTree(child, anchor, relative, ref visits)) return true;
                    }
                }
                else if (relative.L == '+' || relative.L == '~')
                {
                    var following = false;
                    if (anchor.Up != null)
                        foreach (var child in anchor.Up.C)
                        {
                            if (Object.ReferenceEquals(child, anchor)) { following = true; continue; }
                            if (following && RelativeTree(child, anchor, relative, ref visits)) return true;
                        }
                }
                else
                {
                    foreach (var child in anchor.C)
                        if (RelativeTree(child, anchor, relative, ref visits)) return true;
                }
                if (visits >= MaxHasVisits) return false;
            }
            return false;
        }

        private static bool RelativeTree(N node, N anchor, Hr relative, ref int visits)
        {
            if (node == null || visits >= MaxHasVisits) return false;
            if (ElementCandidate(node, anchor, relative, ref visits)) return true;
            foreach (var child in node.C)
            {
                if (RelativeTree(child, anchor, relative, ref visits)) return true;
                if (visits >= MaxHasVisits) return false;
            }
            return false;
        }

        private static bool ElementCandidate(N node, N anchor, Hr relative, ref int visits)
        {
            if (node == null || node.K.Length == 0 || node.K[0] == '#' || visits >= MaxHasVisits) return false;
            visits++;
            var current = RelativeLeft(node, relative);
            if (current == null) return false;
            if (relative.L == '>') return Object.ReferenceEquals(current.Up, anchor);
            if (relative.L == '+') return Object.ReferenceEquals(PreviousElement(current), anchor);
            if (relative.L == '~')
            {
                for (var previous = PreviousElement(current); previous != null; previous = PreviousElement(previous))
                    if (Object.ReferenceEquals(previous, anchor)) return true;
                return false;
            }
            for (var parent = current.Up; parent != null; parent = parent.Up)
                if (Object.ReferenceEquals(parent, anchor)) return true;
            return false;
        }

        private static N RelativeLeft(N node, Hr relative)
        {
            if (!MatchPart(node, relative.P[relative.P.Length - 1])) return null;
            var current = node;
            for (var index = relative.J.Length - 1; index >= 0; index--)
            {
                if (relative.J[index] == '>') current = current.Up;
                else if (relative.J[index] == '+') current = PreviousElement(current);
                else if (relative.J[index] == '~') current = PreviousMatchingElement(current, relative.P[index]);
                else
                {
                    current = current.Up;
                    while (current != null && !MatchPart(current, relative.P[index])) current = current.Up;
                    if (current == null) return null;
                    continue;
                }
                if (current == null || !MatchPart(current, relative.P[index])) return null;
            }
            return current;
        }

        private static N NextElement(N node)
        {
            if (node == null || node.Up == null) return null;
            var following = false;
            foreach (var child in node.Up.C)
            {
                if (Object.ReferenceEquals(child, node)) { following = true; continue; }
                if (following && child.K.Length > 0 && child.K[0] != '#') return child;
            }
            return null;
        }

        private static int ElementPosition(N node, bool type, Se[] filters, out int count)
        {
            count = 0;
            var position = 0;
            if (node == null || node.Up == null) return 0;
            foreach (var child in node.Up.C)
            {
                if (child.K.Length == 0 || child.K[0] == '#' || (type && child.K != node.K) || (filters.Length > 0 && !MatchAny(child, filters))) continue;
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
            var matched = new List<R>();
            foreach (var rule in rules)
                if (MatchCss(node, rule)) matched.Add(rule);
            var inline = CssDeclarations(node.Av("style"), StyleOrigin(node));
            foreach (var rule in matched) Custom(style, inherited, rule.Ds, false);
            Custom(style, inherited, inline, false);
            foreach (var rule in matched) Custom(style, inherited, rule.Ds, true);
            Custom(style, inherited, inline, true);
            TagStyle(style, node.K);
            if (node.A.ContainsKey("hidden")) style.D = "none";
            foreach (var rule in matched) Decls(style, rule.Ds, false);
            Decls(style, inline, false);
            foreach (var rule in matched) Decls(style, rule.Ds, true);
            Decls(style, inline, true);
            if (node.A.ContainsKey("data-lea-hidden")) style.D = "none";
            return style;
        }

        private static Uri StyleOrigin(N node)
        {
            var root = node;
            while (root != null && root.Up != null) root = root.Up;
            return root == null ? null : root.Bu;
        }

        private static void Custom(S style, S inherited, Pd[] declarations, bool important)
        {
            foreach (var declaration in declarations)
            {
                if (!declaration.C || declaration.I != important) continue;
                var value = declaration.V;
                if (value.Equals("initial", StringComparison.OrdinalIgnoreCase))
                {
                    if (style.Vr != null && style.Vr.ContainsKey(declaration.N)) VarRemove(style, inherited, declaration.N);
                    continue;
                }
                if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase) || value.Equals("unset", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("revert", StringComparison.OrdinalIgnoreCase) || value.Equals("revert-layer", StringComparison.OrdinalIgnoreCase))
                {
                    string parent;
                    if (inherited != null && inherited.Vr != null && inherited.Vr.TryGetValue(declaration.N, out parent))
                    {
                        Uri origin = null;
                        if (inherited.Vo != null) inherited.Vo.TryGetValue(declaration.N, out origin);
                        VarSet(style, inherited, declaration.N, parent, origin);
                    }
                    else if (style.Vr != null && style.Vr.ContainsKey(declaration.N)) VarRemove(style, inherited, declaration.N);
                    continue;
                }
                if (value.Length == 0 || value.Length > MaxCustomValue) continue;
                if (style.Vr == null || style.Vr.ContainsKey(declaration.N) || style.Vr.Count < MaxCustomProperties)
                    VarSet(style, inherited, declaration.N, value, declaration.U);
            }
        }

        private static void VarSet(S style, S inherited, string name, string value, Uri origin)
        {
            VarWrite(style, inherited)[name] = value;
            var origins = VarOriginWrite(style, inherited);
            if (origin == null) origins.Remove(name);
            else origins[name] = origin;
        }

        private static void VarRemove(S style, S inherited, string name)
        {
            VarWrite(style, inherited).Remove(name);
            VarOriginWrite(style, inherited).Remove(name);
        }

        private static Dictionary<string, string> VarWrite(S style, S inherited)
        {
            if (style.Vr == null) style.Vr = new Dictionary<string, string>(StringComparer.Ordinal);
            else if (inherited != null && Object.ReferenceEquals(style.Vr, inherited.Vr)) style.Vr = new Dictionary<string, string>(style.Vr, StringComparer.Ordinal);
            return style.Vr;
        }

        private static Dictionary<string, Uri> VarOriginWrite(S style, S inherited)
        {
            if (style.Vo == null) style.Vo = new Dictionary<string, Uri>(StringComparer.Ordinal);
            else if (inherited != null && Object.ReferenceEquals(style.Vo, inherited.Vo)) style.Vo = new Dictionary<string, Uri>(style.Vo, StringComparer.Ordinal);
            return style.Vo;
        }

        private static void Decls(S style, Pd[] declarations, bool important)
        {
            foreach (var declaration in declarations)
            {
                if (declaration.C || declaration.I != important) continue;
                var name = declaration.N;
                string value;
                Uri origin;
                if (!VarValue(style, declaration.V, declaration.U, out value, out origin)) continue;
                if (name == "color") style.F = value;
                else if (name == "background") Background(style, value, origin);
                else if (name == "background-color") style.Bg = value;
                else if (name == "background-image") BackgroundImage(style, value, origin);
                else if (name == "background-repeat") BackgroundRepeat(style, value);
                else if (name == "background-position") BackgroundPosition(style, value);
                else if (name == "background-size") BackgroundSize(style, value);
                else if (name == "font-family") FontFamily(style, value);
                else if (name == "font-weight") style.B = value.Equals("bold", StringComparison.OrdinalIgnoreCase) || value == "700" || value == "800" || value == "900";
                else if (name == "font-style") style.I = value.Equals("italic", StringComparison.OrdinalIgnoreCase) || value.Equals("oblique", StringComparison.OrdinalIgnoreCase);
                else if (name == "text-decoration" || name == "text-decoration-line") TextDecoration(style, value);
                else if (name == "text-align") TextAlign(style, value);
                else if (name == "line-height") LineHeight(style, value);
                else if (name == "white-space") WhiteSpace(style, value);
                else if (name == "word-break" || name == "overflow-wrap" || name == "word-wrap") WordBreak(style, value, name == "word-break");
                else if (name == "overflow") Overflow(style, value);
                else if (name == "object-fit") ObjectFit(style, value);
                else if (name == "object-position") ObjectPosition(style, value);
                else if (name == "visibility") Visibility(style, value);
                else if (name == "clip-path" && value.Length <= 128) style.Cp = value;
                else if (name == "opacity") Opacity(style, value);
                else if (name == "isolation") Isolation(style, value);
                else if (name == "transform") Transform(style, value);
                else if (name == "display") style.D = value.ToLowerInvariant();
                else if (name == "flex-direction") FlexDirection(style, value);
                else if (name == "flex-wrap") FlexWrap(style, value);
                else if (name == "flex-grow") FlexGrow(style, value);
                else if (name == "flex-shrink") FlexShrink(style, value);
                else if (name == "flex-basis") FlexBasis(style, value);
                else if (name == "flex") FlexValue(style, value);
                else if (name == "order") FlexOrder(style, value);
                else if (name == "justify-content") JustifyContent(style, value);
                else if (name == "align-content") AlignContent(style, value);
                else if (name == "justify-items") JustifyItems(style, value);
                else if (name == "justify-self") JustifySelf(style, value);
                else if (name == "align-items") AlignItems(style, value);
                else if (name == "align-self") AlignSelf(style, value);
                else if (name == "place-items") PlaceAlignment(style, value, false);
                else if (name == "place-self") PlaceAlignment(style, value, true);
                else if (name == "place-content") PlaceContent(style, value);
                else if (name == "gap" || name == "grid-gap") Gap(style, value);
                else if (name == "column-gap" || name == "grid-column-gap") OneGap(style, value, false);
                else if (name == "row-gap" || name == "grid-row-gap") OneGap(style, value, true);
                else if (name == "grid-template") GridShorthand(style, value, true);
                else if (name == "grid") GridShorthand(style, value, false);
                else if (name == "grid-template-columns") GridTemplate(style, value, false);
                else if (name == "grid-template-rows") GridTemplate(style, value, true);
                else if (name == "grid-template-areas") GridTemplateAreas(style, value);
                else if (name == "grid-auto-columns") GridAutoTracks(style, value, false);
                else if (name == "grid-auto-rows") GridAutoTracks(style, value, true);
                else if (name == "grid-auto-flow") GridAutoFlow(style, value);
                else if (name == "grid-column") GridPlacement(style, value, false);
                else if (name == "grid-column-start") GridPlacementStart(style, value, false);
                else if (name == "grid-column-end") GridPlacementEnd(style, value, false);
                else if (name == "grid-row") GridPlacement(style, value, true);
                else if (name == "grid-row-start") GridPlacementStart(style, value, true);
                else if (name == "grid-row-end") GridPlacementEnd(style, value, true);
                else if (name == "grid-area") GridArea(style, value);
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
                else if (name == "box-sizing") BoxSizing(style, value);
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
                else if (name == "border-radius") BorderRadius(style, value);
                else if (name == "border-top-left-radius") BorderCornerRadius(style, value, 0);
                else if (name == "border-top-right-radius") BorderCornerRadius(style, value, 1);
                else if (name == "border-bottom-right-radius") BorderCornerRadius(style, value, 2);
                else if (name == "border-bottom-left-radius") BorderCornerRadius(style, value, 3);
                else if (name == "box-shadow") BoxShadow(style, value);
                else if (name == "font-size")
                {
                    var size = Regex.Match(value, "(?<n>[0-9]{1,3})(?:px)?", RegexOptions.IgnoreCase);
                    int number;
                    if (size.Success && int.TryParse(size.Groups["n"].Value, out number) && number > 0 && number < 200) style.Fs = number;
                }
            }
        }

        private static Pd[] CssDeclarations(string declarations)
        {
            return CssDeclarations(declarations, null);
        }

        private static Pd[] CssDeclarations(string declarations, Uri origin)
        {
            var source = CssClean(declarations ?? "");
            if (source.Length == 0 || source.Length > 64 * 1024) return NoDeclarations;
            var result = new List<Pd>();
            var start = 0;
            var quote = '\0';
            var parentheses = 0;
            for (var index = 0; index <= source.Length; index++)
            {
                var end = index == source.Length;
                var value = end ? ';' : source[index];
                if (!end && quote != '\0')
                {
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (!end && (value == '"' || value == '\'')) { quote = value; continue; }
                if (!end && value == '(') { parentheses++; continue; }
                if (!end && value == ')' && parentheses > 0) { parentheses--; continue; }
                if (value != ';' || parentheses != 0) continue;
                CssDeclaration(source.Substring(start, index - start), origin, result);
                if (result.Count == MaxCssDeclarations) break;
                start = index + 1;
            }
            return result.ToArray();
        }

        private static void CssDeclaration(string source, Uri origin, IList<Pd> result)
        {
            var quote = '\0';
            var parentheses = 0;
            var split = -1;
            for (var index = 0; index < (source ?? "").Length; index++)
            {
                var value = source[index];
                if (quote != '\0')
                {
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (value == '"' || value == '\'') { quote = value; continue; }
                if (value == '(') parentheses++;
                else if (value == ')' && parentheses > 0) parentheses--;
                else if (value == ':' && parentheses == 0) { split = index; break; }
            }
            if (split < 1) return;
            var name = source.Substring(0, split).Trim();
            var custom = CustomName(name);
            if (!custom) name = name.ToLowerInvariant();
            if (name.Length == 0) return;
            string declarationValue;
            var important = CssImportant(source.Substring(split + 1), out declarationValue);
            if (declarationValue.Length == 0) return;
            result.Add(new Pd(name, declarationValue, origin, custom, important));
        }

        private static bool CssImportant(string source, out string value)
        {
            value = (source ?? "").Trim();
            var quote = '\0';
            var parentheses = 0;
            var marker = -1;
            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                if (quote != '\0')
                {
                    if (current == quote && (index == 0 || value[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (current == '"' || current == '\'') { quote = current; continue; }
                if (current == '(') { parentheses++; continue; }
                if (current == ')' && parentheses > 0) { parentheses--; continue; }
                if (current == '!' && parentheses == 0) marker = index;
            }
            if (marker < 0 || !Regex.IsMatch(value.Substring(marker), "^!\\s*important\\s*$", RegexOptions.IgnoreCase)) return false;
            value = value.Substring(0, marker).TrimEnd();
            return true;
        }

        private static bool CustomName(string name)
        {
            if (String.IsNullOrEmpty(name) || name.Length < 3 || name.Length > MaxCustomName || name[0] != '-' || name[1] != '-') return false;
            for (var index = 2; index < name.Length; index++)
            {
                var value = name[index];
                var ascii = value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z' || value >= '0' && value <= '9';
                if (!ascii && value != '-' && value != '_') return false;
            }
            return true;
        }

        private static bool VarValue(S style, string source, out string value)
        {
            value = source ?? "";
            if (value.IndexOf("var(", StringComparison.OrdinalIgnoreCase) < 0) return true;
            return ResolveVars(style, value, new HashSet<string>(StringComparer.Ordinal), 0, out value);
        }

        private static bool VarValue(S style, string source, Uri origin, out string value, out Uri valueOrigin)
        {
            valueOrigin = origin;
            if (!VarValue(style, source, out value)) return false;
            var match = Regex.Match((source ?? "").Trim(), "^var\\(\\s*(?<n>--[A-Za-z0-9_-]{1,64})\\s*\\)$", RegexOptions.IgnoreCase);
            Uri customOrigin;
            if (match.Success && style != null && style.Vo != null && style.Vo.TryGetValue(match.Groups["n"].Value, out customOrigin))
                valueOrigin = customOrigin;
            return true;
        }

        private static bool ResolveVars(S style, string source, HashSet<string> stack, int depth, out string value)
        {
            value = "";
            if (source == null || source.Length > MaxVarValue || depth > MaxVarDepth) return false;
            var result = new StringBuilder();
            var quote = '\0';
            for (var index = 0; index < source.Length; index++)
            {
                var current = source[index];
                if (quote != '\0')
                {
                    result.Append(current);
                    if (current == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (current == '"' || current == '\'') { quote = current; result.Append(current); continue; }
                if (!VarAt(source, index))
                {
                    result.Append(current);
                    if (result.Length > MaxVarValue) return false;
                    continue;
                }

                var close = CssFunctionClose(source, index + 3);
                if (close < 0) return false;
                var inside = source.Substring(index + 4, close - index - 4);
                var comma = CssVarComma(inside);
                var name = (comma < 0 ? inside : inside.Substring(0, comma)).Trim();
                if (!CustomName(name)) return false;
                var fallback = comma < 0 ? null : inside.Substring(comma + 1).Trim();
                string replacement;
                var resolved = ResolveCustom(style, name, stack, depth + 1, out replacement);
                if (!resolved && fallback != null) resolved = ResolveVars(style, fallback, stack, depth + 1, out replacement);
                if (!resolved) return false;
                result.Append(replacement);
                if (result.Length > MaxVarValue) return false;
                index = close;
            }
            value = result.ToString().Trim();
            return true;
        }

        private static bool ResolveCustom(S style, string name, HashSet<string> stack, int depth, out string value)
        {
            value = "";
            string source;
            if (style == null || style.Vr == null || depth > MaxVarDepth || stack.Contains(name) || !style.Vr.TryGetValue(name, out source)) return false;
            stack.Add(name);
            try { return ResolveVars(style, source, stack, depth, out value); }
            finally { stack.Remove(name); }
        }

        private static bool VarAt(string source, int index)
        {
            if (index < 0 || index + 4 > (source ?? "").Length || !source.Substring(index, 4).Equals("var(", StringComparison.OrdinalIgnoreCase)) return false;
            if (index == 0) return true;
            var prior = source[index - 1];
            return !Char.IsLetterOrDigit(prior) && prior != '-' && prior != '_';
        }

        private static int CssFunctionClose(string source, int open)
        {
            var depth = 1;
            var quote = '\0';
            for (var index = open + 1; index < (source ?? "").Length; index++)
            {
                var value = source[index];
                if (quote != '\0')
                {
                    if (value == quote && source[index - 1] != '\\') quote = '\0';
                    continue;
                }
                if (value == '"' || value == '\'') quote = value;
                else if (value == '(') depth++;
                else if (value == ')' && --depth == 0) return index;
            }
            return -1;
        }

        private static int CssVarComma(string source)
        {
            var depth = 0;
            var quote = '\0';
            for (var index = 0; index < (source ?? "").Length; index++)
            {
                var value = source[index];
                if (quote != '\0')
                {
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (value == '"' || value == '\'') quote = value;
                else if (value == '(') depth++;
                else if (value == ')' && depth > 0) depth--;
                else if (value == ',' && depth == 0) return index;
            }
            return -1;
        }

        private static void BoxSizing(S style, string value)
        {
            var sizing = (value ?? "").Trim().ToLowerInvariant();
            if (sizing == "border-box") style.Bz = "b";
            else if (sizing == "content-box" || sizing == "initial" || sizing == "unset" || sizing == "revert") style.Bz = "c";
        }

        private static void Position(S style, string value)
        {
            var mode = (value ?? "").Trim().ToLowerInvariant();
            if (mode == "relative") style.Pn = "r";
            else if (mode == "absolute") style.Pn = "a";
            else if (mode == "fixed") style.Pn = "f";
            else if (mode == "static" || mode == "initial" || mode == "unset" || mode == "revert") style.Pn = "s";
        }

        private static void FlexDirection(S style, string value)
        {
            var direction = (value ?? "").Trim().ToLowerInvariant();
            if (direction == "row" || direction == "initial" || direction == "normal") style.Fd = "r";
            else if (direction == "row-reverse") style.Fd = "x";
            else if (direction == "column") style.Fd = "c";
            else if (direction == "column-reverse") style.Fd = "y";
        }

        private static void FlexWrap(S style, string value)
        {
            var wrap = (value ?? "").Trim().ToLowerInvariant();
            if (wrap == "wrap") style.Wr = "w";
            else if (wrap == "nowrap" || wrap == "initial" || wrap == "normal") style.Wr = "n";
        }

        private static void FlexGrow(S style, string value)
        {
            int grow;
            if (GrowValue(value, out grow)) style.Fg = grow;
        }

        private static void FlexShrink(S style, string value)
        {
            int shrink;
            if (GrowValue(value, out shrink)) style.Sh = shrink;
        }

        private static void FlexBasis(S style, string value)
        {
            int basis;
            if (BasisValue(value, out basis)) style.Fb = basis;
        }

        private static void FlexOrder(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "initial" || source == "normal" || source == "unset" || source == "revert") { style.Or = 0; return; }
            var match = Regex.Match(source, "^(?<n>[+-]?[0-9]{1,2})$");
            int order;
            if (match.Success && Int32.TryParse(match.Groups["n"].Value, out order) && order >= -64 && order <= 64) style.Or = order;
        }

        private static void GridTemplate(S style, string value, bool rows)
        {
            var source = Regex.Replace((value ?? "").Trim(), "minmax\\(\\s*0(?:px)?\\s*,\\s*(?<n>[1-9][0-9]?)fr\\s*\\)", "${n}fr", RegexOptions.IgnoreCase);
            if (source.Equals("none", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase))
            {
                if (rows) { style.Rd = null; style.Rnd = null; }
                else { style.Gd = null; style.Gnd = null; }
                return;
            }
            if (source.Length == 0 || source.Length > 512) return;

            for (var expansion = 0; expansion < 8; expansion++)
            {
                var repeat = Regex.Match(source, "repeat\\(\\s*(?<n>[1-8])\\s*,\\s*(?<t>(?:[0-9]{1,4}px|[1-9][0-9]?fr|auto))\\s*\\)", RegexOptions.IgnoreCase);
                if (!repeat.Success) break;
                int count;
                if (!Int32.TryParse(repeat.Groups["n"].Value, out count)) return;
                var values = new string[count];
                for (var index = 0; index < count; index++) values[index] = repeat.Groups["t"].Value;
                source = source.Substring(0, repeat.Index) + String.Join(" ", values) + source.Substring(repeat.Index + repeat.Length);
                if (source.Length > 512) return;
            }
            if (source.IndexOf("repeat", StringComparison.OrdinalIgnoreCase) >= 0 || source.IndexOf('(') >= 0 || source.IndexOf(')') >= 0) return;

            var parts = Regex.Matches(source, "\\[(?<names>[^\\[\\]]*)\\]|(?<track>[^\\s\\[\\]]+)");
            if (parts.Count == 0) return;
            var tracks = new List<int>();
            Dictionary<string, int> names = null;
            var cursor = 0;
            foreach (Match part in parts)
            {
                if (!Regex.IsMatch(source.Substring(cursor, part.Index - cursor), "^\\s*$")) return;
                cursor = part.Index + part.Length;
                if (part.Groups["names"].Success)
                {
                    var group = part.Groups["names"].Value.Trim();
                    if (group.Length == 0) continue;
                    if (group.Length > 263) return;
                    var lineNames = Regex.Split(group, "\\s+");
                    if (lineNames.Length > 8) return;
                    foreach (var name in lineNames)
                    {
                        if (!GridLineName(name)) return;
                        if (names == null) names = new Dictionary<string, int>(StringComparer.Ordinal);
                        if (names.ContainsKey(name) || names.Count == 16) return;
                        names[name] = tracks.Count + 1;
                    }
                    continue;
                }
                if (tracks.Count == 8) return;
                var token = part.Groups["track"].Value;
                var pixels = Regex.Match(token, "^(?<n>[0-9]{1,4})px$", RegexOptions.IgnoreCase);
                var fraction = Regex.Match(token, "^(?<n>[1-9][0-9]?)fr$", RegexOptions.IgnoreCase);
                int number;
                if (token.Equals("auto", StringComparison.OrdinalIgnoreCase)) tracks.Add(S.Av);
                else if (pixels.Success && Int32.TryParse(pixels.Groups["n"].Value, out number) && number <= 2000) tracks.Add(number);
                else if (fraction.Success && Int32.TryParse(fraction.Groups["n"].Value, out number) && number <= 64) tracks.Add(-number);
                else return;
            }
            if (!Regex.IsMatch(source.Substring(cursor), "^\\s*$") || tracks.Count == 0) return;
            if (rows) { style.Rd = tracks.ToArray(); style.Rnd = names; }
            else { style.Gd = tracks.ToArray(); style.Gnd = names; }
        }

        private static bool GridLineName(string value)
        {
            if (!Regex.IsMatch(value ?? "", "^[A-Za-z_][A-Za-z0-9_-]{0,31}$")) return false;
            return !value.Equals("auto", StringComparison.OrdinalIgnoreCase) && !value.Equals("span", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("initial", StringComparison.OrdinalIgnoreCase) && !value.Equals("inherit", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("unset", StringComparison.OrdinalIgnoreCase) && !value.Equals("revert", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("revert-layer", StringComparison.OrdinalIgnoreCase) && !value.Equals("default", StringComparison.OrdinalIgnoreCase);
        }

        private static bool GridPlacementName(string value)
        {
            return Regex.IsMatch(value ?? "", "^[A-Za-z_][A-Za-z0-9_-]{0,37}$") &&
                !value.Equals("auto", StringComparison.OrdinalIgnoreCase) && !value.Equals("span", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("initial", StringComparison.OrdinalIgnoreCase) && !value.Equals("inherit", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("unset", StringComparison.OrdinalIgnoreCase) && !value.Equals("revert", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("revert-layer", StringComparison.OrdinalIgnoreCase) && !value.Equals("default", StringComparison.OrdinalIgnoreCase);
        }

        private static void GridAutoTracks(S style, string value, bool rows)
        {
            var source = Regex.Replace((value ?? "").Trim(), "minmax\\(\\s*0(?:px)?\\s*,\\s*(?<n>[1-9][0-9]?)fr\\s*\\)", "${n}fr", RegexOptions.IgnoreCase);
            if (source.Equals("auto", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) ||
                source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase) ||
                source.Equals("revert-layer", StringComparison.OrdinalIgnoreCase))
            {
                if (rows) style.Rad = null;
                else style.Gad = null;
                return;
            }
            if (source.Length == 0 || source.Length > 512 || source.IndexOf('(') >= 0 || source.IndexOf(')') >= 0 ||
                source.IndexOf('[') >= 0 || source.IndexOf(']') >= 0) return;
            var parts = Regex.Split(source, "\\s+");
            if (parts.Length == 0 || parts.Length > 8) return;
            var tracks = new List<int>();
            foreach (var token in parts)
            {
                if (token.Equals("auto", StringComparison.OrdinalIgnoreCase)) { tracks.Add(S.Av); continue; }
                var pixels = Regex.Match(token, "^(?<n>[0-9]{1,4})px$", RegexOptions.IgnoreCase);
                var fraction = Regex.Match(token, "^(?<n>[1-9][0-9]?)fr$", RegexOptions.IgnoreCase);
                int number;
                if (pixels.Success && Int32.TryParse(pixels.Groups["n"].Value, out number) && number <= 2000) tracks.Add(number);
                else if (fraction.Success && Int32.TryParse(fraction.Groups["n"].Value, out number) && number <= 64) tracks.Add(-number);
                else return;
            }
            if (rows) style.Rad = tracks.ToArray();
            else style.Gad = tracks.ToArray();
        }

        private static void GridAutoFlow(S style, string value)
        {
            var source = Regex.Replace((value ?? "").Trim().ToLowerInvariant(), "\\s+", " ");
            if (source == "row" || source == "initial" || source == "unset" || source == "revert") style.Gf = "r";
            else if (source == "dense" || source == "row dense" || source == "dense row") style.Gf = "d";
            else if (source == "column") style.Gf = "c";
            else if (source == "column dense" || source == "dense column") style.Gf = "x";
        }

        private static void GridShorthand(S style, string value, bool templateOnly)
        {
            var source = (value ?? "").Trim();
            if (source.Length == 0 || source.Length > 1024) return;
            var reset = source.Equals("none", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) ||
                source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase) ||
                source.Equals("revert-layer", StringComparison.OrdinalIgnoreCase);
            var parsed = new S();
            if (!reset && !GridShorthandParse(parsed, source, !templateOnly)) return;
            GridTemplateCopy(style, parsed);
            if (!templateOnly) GridAutomaticCopy(style, parsed);
        }

        private static bool GridShorthandParse(S style, string source, bool allowFlow)
        {
            var slash = GridShorthandSlash(source);
            if (slash == -2) return false;
            var left = (slash < 0 ? source : source.Substring(0, slash)).Trim();
            var right = slash < 0 ? "" : source.Substring(slash + 1).Trim();
            if (left.Length == 0 || slash >= 0 && right.Length == 0) return false;

            bool leftDense;
            var rightDense = false;
            string leftAutomatic;
            var rightAutomatic = "";
            var leftFlow = GridFlowShorthandSide(left, out leftDense, out leftAutomatic);
            var rightFlow = slash >= 0 && GridFlowShorthandSide(right, out rightDense, out rightAutomatic);
            if (leftFlow || rightFlow)
            {
                if (!allowFlow || slash < 0 || leftFlow && rightFlow) return false;
                if (leftFlow)
                {
                    if (!GridTemplateAxis(style, right, false) || !GridAutomaticAxis(style, leftAutomatic, true)) return false;
                    style.Gf = leftDense ? "d" : "r";
                }
                else
                {
                    if (!GridTemplateAxis(style, left, true) || !GridAutomaticAxis(style, rightAutomatic, false)) return false;
                    style.Gf = rightDense ? "x" : "c";
                }
                return true;
            }
            if (left.IndexOf("auto-flow", StringComparison.OrdinalIgnoreCase) >= 0 || right.IndexOf("auto-flow", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            if (left.IndexOf('"') >= 0 || left.IndexOf('\'') >= 0) return GridAreaShorthand(style, left, slash < 0 ? null : right);
            return slash >= 0 && GridTemplateAxis(style, left, true) && GridTemplateAxis(style, right, false);
        }

        private static int GridShorthandSlash(string source)
        {
            var quote = '\0';
            var parentheses = 0;
            var brackets = 0;
            var slash = -1;
            for (var index = 0; index < source.Length; index++)
            {
                var current = source[index];
                if (quote != '\0')
                {
                    if (current == quote) quote = '\0';
                    continue;
                }
                if (current == '"' || current == '\'') { quote = current; continue; }
                if (current == '(') parentheses++;
                else if (current == ')' && --parentheses < 0) return -2;
                else if (current == '[') brackets++;
                else if (current == ']' && --brackets < 0) return -2;
                else if (current == '/' && parentheses == 0 && brackets == 0)
                {
                    if (slash >= 0) return -2;
                    slash = index;
                }
            }
            return quote == '\0' && parentheses == 0 && brackets == 0 ? slash : -2;
        }

        private static bool GridFlowShorthandSide(string source, out bool dense, out string tracks)
        {
            dense = false;
            tracks = "";
            var match = Regex.Match(source ?? "", "^(?:auto-flow(?:\\s+(?<d>dense))?|(?<d>dense)\\s+auto-flow)(?:\\s+(?<t>.+))?$", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            dense = match.Groups["d"].Success;
            tracks = match.Groups["t"].Success ? match.Groups["t"].Value.Trim() : "";
            return true;
        }

        private static bool GridTemplateAxis(S style, string source, bool rows)
        {
            if (source.Equals("none", StringComparison.OrdinalIgnoreCase)) return true;
            GridTemplate(style, source, rows);
            return rows ? style.Rc > 0 : style.Gc > 0;
        }

        private static bool GridAutomaticAxis(S style, string source, bool rows)
        {
            if (String.IsNullOrEmpty(source) || source.Equals("auto", StringComparison.OrdinalIgnoreCase)) return true;
            GridAutoTracks(style, source, rows);
            return rows ? style.Rac > 0 : style.Gac > 0;
        }

        private static bool GridAreaShorthand(S style, string rows, string columns)
        {
            var matches = Regex.Matches(rows, "\"(?<d>[^\"]*)\"|'(?<s>[^']*)'");
            if (matches.Count < 1 || matches.Count > 8 || !Regex.IsMatch(rows.Substring(0, matches[0].Index), "^\\s*$")) return false;
            var areas = new List<string>();
            var tracks = new List<string>();
            for (var index = 0; index < matches.Count; index++)
            {
                var match = matches[index];
                areas.Add(match.Value);
                var end = index + 1 < matches.Count ? matches[index + 1].Index : rows.Length;
                var track = rows.Substring(match.Index + match.Length, end - match.Index - match.Length).Trim();
                tracks.Add(track.Length == 0 ? "auto" : track);
            }
            GridTemplateAreas(style, String.Join(" ", areas));
            if (style.Ar != matches.Count) return false;
            GridTemplate(style, String.Join(" ", tracks), true);
            if (style.Rc != matches.Count) return false;
            return columns == null || GridTemplateAxis(style, columns, false);
        }

        private static void GridTemplateCopy(S target, S source)
        {
            target.Gd = source.Gd;
            target.Rd = source.Rd;
            target.Gnd = source.Gnd;
            target.Rnd = source.Rnd;
            target.Ad = source.Ad;
            target.Ar = source.Ar;
            target.Ac = source.Ac;
        }

        private static void GridAutomaticCopy(S target, S source)
        {
            target.Gad = source.Gad;
            target.Rad = source.Rad;
            target.Gf = source.Gf;
        }

        private static void GridPlacement(S style, string value, bool rows)
        {
            var source = (value ?? "").Trim();
            if (source.Equals("auto", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase)) { GridPlacementSet(style, rows, 0, 1); return; }
            var spanOnly = Regex.Match(source, "^span\\s+(?<n>[1-8])$", RegexOptions.IgnoreCase);
            var placed = Regex.Match(source, "^(?<s>[1-8])(?:\\s*/\\s*span\\s+(?<n>[1-8]))?$", RegexOptions.IgnoreCase);
            var autoSpan = Regex.Match(source, "^auto\\s*/\\s*span\\s+(?<n>[1-8])$", RegexOptions.IgnoreCase);
            int number;
            if (spanOnly.Success && Int32.TryParse(spanOnly.Groups["n"].Value, out number)) GridPlacementSet(style, rows, 0, number);
            else if (autoSpan.Success && Int32.TryParse(autoSpan.Groups["n"].Value, out number)) GridPlacementSet(style, rows, 0, number);
            else if (placed.Success)
            {
                int start;
                if (!Int32.TryParse(placed.Groups["s"].Value, out start)) return;
                var span = 1;
                if (placed.Groups["n"].Success && !Int32.TryParse(placed.Groups["n"].Value, out span)) return;
                GridPlacementSet(style, rows, start, span);
            }
            else
            {
                var sides = Regex.Split(source, "\\s*/\\s*");
                if (sides.Length == 1 && GridPlacementName(sides[0])) GridPlacementNamedSet(style, rows, 0, sides[0], sides[0], 1);
                else if (sides.Length == 2)
                {
                    var startName = GridPlacementName(sides[0]) ? sides[0] : "";
                    var endName = GridPlacementName(sides[1]) ? sides[1] : "";
                    var spanMatch = Regex.Match(sides[1], "^span\\s+(?<n>[1-8])$", RegexOptions.IgnoreCase);
                    var reverseSpan = Regex.Match(sides[0], "^span\\s+(?<n>[1-8])$", RegexOptions.IgnoreCase);
                    int start;
                    if (startName.Length > 0 && endName.Length > 0) GridPlacementNamedSet(style, rows, 0, startName, endName, 1);
                    else if (startName.Length > 0 && spanMatch.Success && Int32.TryParse(spanMatch.Groups["n"].Value, out number)) GridPlacementNamedSet(style, rows, 0, startName, "", number);
                    else if ((sides[0].Equals("auto", StringComparison.OrdinalIgnoreCase) || reverseSpan.Success) && endName.Length > 0)
                    {
                        var span = reverseSpan.Success && Int32.TryParse(reverseSpan.Groups["n"].Value, out number) ? number : 1;
                        GridPlacementNamedSet(style, rows, 0, "", endName, span);
                    }
                    else if (Int32.TryParse(sides[0], out start) && start >= 1 && start <= 8 && endName.Length > 0) GridPlacementNamedSet(style, rows, start, "", endName, 1);
                    else
                    {
                        int end;
                        if (Int32.TryParse(sides[0], out start) && Int32.TryParse(sides[1], out end) && start >= 1 && start <= 8 && end > start && end <= 9) GridPlacementSet(style, rows, start, end - start);
                    }
                }
            }
        }

        private static void GridPlacementStart(S style, string value, bool rows)
        {
            var source = (value ?? "").Trim();
            if (source.Equals("auto", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase))
            {
                style.An = "";
                if (rows) style.Rn = ""; else style.Gn = "";
                if (rows) style.Rl = 0; else style.Gl = 0;
                return;
            }
            int start;
            if (Regex.IsMatch(source, "^[1-8]$") && Int32.TryParse(source, out start))
            {
                style.An = "";
                if (rows) style.Rn = ""; else style.Gn = "";
                if (rows) style.Rl = start; else style.Gl = start;
            }
            else if (GridPlacementName(source))
            {
                style.An = "";
                if (rows) { style.Rl = 0; style.Rn = source; }
                else { style.Gl = 0; style.Gn = source; }
            }
        }

        private static void GridPlacementEnd(S style, string value, bool rows)
        {
            var source = (value ?? "").Trim();
            if (source.Equals("auto", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase))
            {
                style.An = "";
                if (rows) style.Re = ""; else style.Ge = "";
                if (rows) style.Rs = 1; else style.Gs = 1;
                return;
            }
            var match = Regex.Match(source, "^span\\s+(?<n>[1-8])$", RegexOptions.IgnoreCase);
            int span;
            if (match.Success && Int32.TryParse(match.Groups["n"].Value, out span))
            {
                style.An = "";
                if (rows) style.Re = ""; else style.Ge = "";
                if (rows) style.Rs = span; else style.Gs = span;
            }
            else if (GridPlacementName(source))
            {
                style.An = "";
                if (rows) style.Re = source; else style.Ge = source;
            }
        }

        private static void GridPlacementSet(S style, bool rows, int start, int span)
        {
            style.An = "";
            if (rows) { style.Rl = start; style.Rs = span; style.Rn = ""; style.Re = ""; }
            else { style.Gl = start; style.Gs = span; style.Gn = ""; style.Ge = ""; }
        }

        private static void GridPlacementNamedSet(S style, bool rows, int start, string startName, string endName, int span)
        {
            style.An = "";
            if (rows) { style.Rl = start; style.Rs = span; style.Rn = startName; style.Re = endName; }
            else { style.Gl = start; style.Gs = span; style.Gn = startName; style.Ge = endName; }
        }

        private static void GridTemplateAreas(S style, string value)
        {
            var source = (value ?? "").Trim();
            if (source.Equals("none", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase))
            {
                style.Ad = null;
                style.Ar = 0;
                style.Ac = 0;
                return;
            }
            if (source.Length == 0 || source.Length > 512) return;

            var rows = Regex.Matches(source, "\"(?<d>[^\"]*)\"|'(?<s>[^']*)'");
            if (rows.Count < 1 || rows.Count > 8) return;
            var cells = new string[rows.Count][];
            var cursor = 0;
            var columns = 0;
            for (var row = 0; row < rows.Count; row++)
            {
                var match = rows[row];
                if (!Regex.IsMatch(source.Substring(cursor, match.Index - cursor), "^\\s*$")) return;
                var text = match.Groups["d"].Success ? match.Groups["d"].Value : match.Groups["s"].Value;
                if (text.Length == 0 || text.Length > 263 || text.IndexOf('\\') >= 0) return;
                var parts = Regex.Split(text.Trim(), "\\s+");
                if (parts.Length < 1 || parts.Length > 8 || columns != 0 && parts.Length != columns) return;
                columns = parts.Length;
                cells[row] = parts;
                cursor = match.Index + match.Length;
            }
            if (!Regex.IsMatch(source.Substring(cursor), "^\\s*$")) return;

            var areas = new Dictionary<string, int[]>(StringComparer.Ordinal);
            for (var row = 0; row < cells.Length; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var name = cells[row][column];
                    if (Regex.IsMatch(name, "^\\.+$")) continue;
                    if (!Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_-]{0,31}$")) return;
                    int[] area;
                    if (!areas.TryGetValue(name, out area))
                    {
                        if (areas.Count == 16) return;
                        areas[name] = new[] { row, column, row, column, 1 };
                    }
                    else
                    {
                        area[0] = Math.Min(area[0], row);
                        area[1] = Math.Min(area[1], column);
                        area[2] = Math.Max(area[2], row);
                        area[3] = Math.Max(area[3], column);
                        area[4]++;
                    }
                }
            }
            foreach (var area in areas.Values)
            {
                if ((area[2] - area[0] + 1) * (area[3] - area[1] + 1) != area[4]) return;
                area[2] = area[2] - area[0] + 1;
                area[3] = area[3] - area[1] + 1;
                area[0]++;
                area[1]++;
            }
            style.Ad = areas;
            style.Ar = cells.Length;
            style.Ac = columns;
        }

        private static void GridArea(S style, string value)
        {
            var source = (value ?? "").Trim();
            if (source.Equals("auto", StringComparison.OrdinalIgnoreCase) || source.Equals("initial", StringComparison.OrdinalIgnoreCase) || source.Equals("unset", StringComparison.OrdinalIgnoreCase) || source.Equals("revert", StringComparison.OrdinalIgnoreCase))
            {
                style.An = "";
                style.Gl = 0;
                style.Gs = 1;
                style.Rl = 0;
                style.Rs = 1;
                style.Gn = "";
                style.Ge = "";
                style.Rn = "";
                style.Re = "";
                return;
            }
            if (!Regex.IsMatch(source, "^[A-Za-z_][A-Za-z0-9_-]{0,31}$")) return;
            style.Gl = 0;
            style.Gs = 1;
            style.Rl = 0;
            style.Rs = 1;
            style.Gn = "";
            style.Ge = "";
            style.Rn = "";
            style.Re = "";
            style.An = source;
        }

        private static void FlexValue(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "none") { style.Fg = 0; style.Sh = 0; style.Fb = -1; return; }
            if (source == "initial") { style.Fg = 0; style.Sh = 1; style.Fb = -1; return; }
            if (source == "auto") { style.Fg = 1; style.Sh = 1; style.Fb = -1; return; }
            var parts = Regex.Split(source, "\\s+");
            int grow;
            if (parts.Length < 1 || parts.Length > 3 || !GrowValue(parts[0], out grow)) return;
            var shrink = 1;
            var basis = 0;
            if (parts.Length == 2)
            {
                if (!FlexTokenBasis(parts[1], out basis))
                {
                    if (!GrowValue(parts[1], out shrink)) return;
                    basis = 0;
                }
            }
            else if (parts.Length == 3)
            {
                if (!GrowValue(parts[1], out shrink) || !BasisValue(parts[2], out basis)) return;
            }
            style.Fg = grow;
            style.Sh = shrink;
            style.Fb = basis;
        }

        private static bool FlexTokenBasis(string value, out int basis)
        {
            basis = -1;
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source != "auto" && source != "content" && source != "0%" && !source.EndsWith("px", StringComparison.Ordinal)) return false;
            return BasisValue(source, out basis);
        }

        private static bool GrowValue(string value, out int grow)
        {
            grow = 0;
            var match = Regex.Match((value ?? "").Trim(), "^(?<n>[0-9]{1,2})$");
            return match.Success && Int32.TryParse(match.Groups["n"].Value, out grow) && grow <= 64;
        }

        private static bool BasisValue(string value, out int basis)
        {
            basis = -1;
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "auto" || source == "content") return true;
            if (source == "0%") { basis = 0; return true; }
            var match = Regex.Match(source, "^(?<n>[0-9]{1,4})(?:px)?$");
            return match.Success && Int32.TryParse(match.Groups["n"].Value, out basis);
        }

        private static void Gap(S style, string value)
        {
            var parts = Regex.Split((value ?? "").Trim().ToLowerInvariant(), "\\s+");
            int row;
            int column;
            if (parts.Length < 1 || parts.Length > 2 || !GapValue(parts[0], out row) || !GapValue(parts.Length == 2 ? parts[1] : parts[0], out column)) return;
            style.Rg = row;
            style.Gp = column;
        }

        private static void OneGap(S style, string value, bool row)
        {
            int gap;
            if (!GapValue(value, out gap)) return;
            if (row) style.Rg = gap; else style.Gp = gap;
        }

        private static bool GapValue(string value, out int gap)
        {
            gap = 0;
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "normal") return true;
            var match = Regex.Match(source, "^(?<n>[0-9]{1,3})(?:px)?$");
            return match.Success && Int32.TryParse(match.Groups["n"].Value, out gap) && gap <= 512;
        }

        private static void JustifyContent(S style, string value)
        {
            string align;
            if (ContentAlignmentValue(value, true, out align)) style.Jc = align;
        }

        private static void AlignContent(S style, string value)
        {
            string align;
            if (ContentAlignmentValue(value, false, out align)) style.Al = align;
        }

        private static void PlaceContent(S style, string value)
        {
            var source = Regex.Replace((value ?? "").Trim(), "\\s+", " ");
            if (source.Length == 0 || source.Length > 64) return;
            var parts = source.Split(' ');
            if (parts.Length < 1 || parts.Length > 2) return;
            string vertical;
            string horizontal;
            if (!ContentAlignmentValue(parts[0], false, out vertical) ||
                !ContentAlignmentValue(parts.Length == 1 ? parts[0] : parts[1], true, out horizontal)) return;
            style.Al = vertical;
            style.Jc = horizontal;
        }

        private static bool ContentAlignmentValue(string value, bool horizontal, out string align)
        {
            align = "s";
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source == "normal" || source == "stretch" || source == "initial" || source == "unset" || source == "revert" || source == "revert-layer") return true;
            if (source == "flex-start" || source == "start" || horizontal && source == "left") { align = "s"; return true; }
            if (source == "center") { align = "c"; return true; }
            if (source == "flex-end" || source == "end" || horizontal && source == "right") { align = "e"; return true; }
            if (source == "space-between") { align = "b"; return true; }
            if (source == "space-around") { align = "a"; return true; }
            if (source == "space-evenly") { align = "v"; return true; }
            return false;
        }

        private static void AlignItems(S style, string value)
        {
            string align;
            if (AlignmentValue(value, false, false, out align)) style.Ai = align;
        }

        private static void AlignSelf(S style, string value)
        {
            string align;
            if (AlignmentValue(value, true, false, out align)) style.As = align;
        }

        private static void JustifyItems(S style, string value)
        {
            string align;
            if (AlignmentValue(value, false, true, out align)) style.Ji = align;
        }

        private static void JustifySelf(S style, string value)
        {
            string align;
            if (AlignmentValue(value, true, true, out align)) style.Js = align;
        }

        private static void PlaceAlignment(S style, string value, bool self)
        {
            var source = Regex.Replace((value ?? "").Trim(), "\\s+", " ");
            if (source.Length == 0 || source.Length > 64) return;
            var parts = source.Split(' ');
            if (parts.Length < 1 || parts.Length > 2) return;
            string vertical;
            string horizontal;
            if (!AlignmentValue(parts[0], self, false, out vertical) ||
                !AlignmentValue(parts.Length == 1 ? parts[0] : parts[1], self, true, out horizontal)) return;
            if (self) { style.As = vertical; style.Js = horizontal; }
            else { style.Ai = vertical; style.Ji = horizontal; }
        }

        private static bool AlignmentValue(string value, bool self, bool horizontal, out string align)
        {
            align = "";
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (self && (source == "auto" || source == "initial" || source == "unset" || source == "revert" || source == "revert-layer")) return true;
            if (!self && (source == "initial" || source == "unset" || source == "revert" || source == "revert-layer")) { align = "t"; return true; }
            if (source == "stretch" || source == "normal") { align = "t"; return true; }
            if (source == "flex-start" || source == "start" || source == "self-start" || horizontal && source == "left") { align = "s"; return true; }
            if (source == "center") { align = "c"; return true; }
            if (source == "flex-end" || source == "end" || source == "self-end" || horizontal && source == "right") { align = "e"; return true; }
            return false;
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

        private static void BorderRadius(S style, string value)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (RadiusReset(source))
            {
                SetRadii(style, new int[4], new int[4]);
                return;
            }
            var slash = source.IndexOf('/');
            if (slash >= 0 && source.IndexOf('/', slash + 1) >= 0) return;
            int[] horizontal;
            int[] vertical;
            if (!RadiusList(slash < 0 ? source : source.Substring(0, slash), out horizontal)) return;
            if (slash < 0) vertical = (int[])horizontal.Clone();
            else if (!RadiusList(source.Substring(slash + 1), out vertical)) return;
            SetRadii(style, horizontal, vertical);
        }

        private static void BorderCornerRadius(S style, string value, int corner)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (RadiusReset(source)) { SetRadius(style, corner, 0, 0); return; }
            var parts = Regex.Split(source, "\\s+");
            int horizontal;
            int vertical;
            if (parts.Length < 1 || parts.Length > 2 || !RadiusValue(parts[0], out horizontal)) return;
            if (parts.Length == 1) vertical = horizontal;
            else if (!RadiusValue(parts[1], out vertical)) return;
            SetRadius(style, corner, horizontal, vertical);
        }

        private static bool RadiusList(string source, out int[] values)
        {
            values = null;
            var parts = Regex.Split((source ?? "").Trim(), "\\s+");
            if (parts.Length < 1 || parts.Length > 4) return false;
            var parsed = new int[parts.Length];
            for (var index = 0; index < parts.Length; index++)
                if (!RadiusValue(parts[index], out parsed[index])) return false;
            values = new int[4];
            values[0] = parsed[0];
            values[1] = parts.Length > 1 ? parsed[1] : parsed[0];
            values[2] = parts.Length > 2 ? parsed[2] : parsed[0];
            values[3] = parts.Length > 3 ? parsed[3] : values[1];
            return true;
        }

        private static bool RadiusValue(string value, out int radius)
        {
            radius = 0;
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,4})(?<u>px|%)?$", RegexOptions.IgnoreCase);
            int number;
            if (!match.Success || !Int32.TryParse(match.Groups["n"].Value, out number)) return false;
            if (match.Groups["u"].Value == "%") radius = -Math.Min(number, 1000);
            else radius = Math.Min(number, 2000);
            return true;
        }

        private static bool RadiusReset(string value)
        {
            return value == "initial" || value == "unset" || value == "revert" || value == "revert-layer";
        }

        private static void SetRadii(S style, int[] horizontal, int[] vertical)
        {
            for (var corner = 0; corner < 4; corner++) SetRadius(style, corner, horizontal[corner], vertical[corner]);
        }

        private static void SetRadius(S style, int corner, int horizontal, int vertical)
        {
            if (corner == 0) { style.R0 = horizontal; style.Q0 = vertical; }
            else if (corner == 1) { style.R1 = horizontal; style.Q1 = vertical; }
            else if (corner == 2) { style.R2 = horizontal; style.Q2 = vertical; }
            else { style.R3 = horizontal; style.Q3 = vertical; }
        }

        private static void BoxShadow(S style, string value)
        {
            var source = (value ?? "").Trim();
            var lower = source.ToLowerInvariant();
            if (lower == "none" || RadiusReset(lower)) { style.Sw.Clear(); return; }
            if (source.Length == 0 || source.Length > 512) return;
            List<string> groups;
            if (!ShadowParts(source, ',', out groups) || groups.Count < 1 || groups.Count > 4) return;
            var shadows = new List<Sw>();
            foreach (var group in groups)
            {
                Sw shadow;
                if (!ShadowValue(group, out shadow)) return;
                shadows.Add(shadow);
            }
            style.Sw.Clear();
            foreach (var shadow in shadows) style.Sw.Add(shadow);
        }

        private static void Background(S style, string value, Uri origin)
        {
            var source = (value ?? "").Trim();
            if (source.Length == 0 || source.Length > MaxBackgroundText) return;
            var next = new S();
            if (!BackgroundValue(next, source, origin)) return;
            style.Bg = next.Bg;
            style.Gi = next.Gi;
            style.Bi = next.Bi;
        }

        private static bool BackgroundValue(S style, string source, Uri origin)
        {
            var lower = source.ToLowerInvariant();
            if (lower == "none" || RadiusReset(lower)) { style.Bg = ""; style.Gi = null; style.Bi = null; return true; }
            List<string> parts;
            if (!BackgroundParts(source, out parts) || parts.Count == 0) return false;
            var bitmap = new Bi();
            Gi gradient = null;
            var position = new List<string>();
            var size = new List<string>();
            var color = "";
            var image = false;
            var geometry = false;
            var repeatSeen = false;
            var slash = false;
            foreach (var raw in parts)
            {
                var part = raw.Trim();
                var token = part.ToLowerInvariant();
                if (part == "/")
                {
                    if (slash || position.Count == 0) return false;
                    slash = true;
                    continue;
                }
                if (slash)
                {
                    if (size.Count == 2) return false;
                    size.Add(part);
                    continue;
                }
                if (GradientSource(token))
                {
                    if (image || !Gradient(part, out gradient)) return false;
                    image = true;
                    continue;
                }
                Uri url;
                if (BitmapUri(part, origin, out url))
                {
                    if (image) return false;
                    bitmap.U = url;
                    image = true;
                    continue;
                }
                if (token == "none" && !image) { image = true; continue; }
                string repeat;
                if (BackgroundRepeatValue(token, out repeat))
                {
                    if (repeatSeen) return false;
                    bitmap.R = repeat;
                    repeatSeen = true;
                    geometry = true;
                    continue;
                }
                if (BackgroundPositionToken(token))
                {
                    if (position.Count == 2) return false;
                    position.Add(part);
                    geometry = true;
                    continue;
                }
                if (color.Length == 0 && ShadowColor(part)) { color = part; continue; }
                return false;
            }
            if (slash && size.Count == 0) return false;
            if (position.Count > 0 && !BackgroundPosition(bitmap, position)) return false;
            if (size.Count > 0 && !BackgroundSize(bitmap, size)) return false;
            style.Bg = color;
            style.Gi = gradient;
            style.Bi = bitmap.U != null || geometry ? bitmap : null;
            return true;
        }

        private static bool BackgroundImage(S style, string value, Uri origin)
        {
            var source = (value ?? "").Trim();
            var lower = source.ToLowerInvariant();
            if (lower == "none" || RadiusReset(lower))
            {
                style.Gi = null;
                if (style.Bi != null) style.Bi.U = null;
                return true;
            }
            Gi gradient;
            if (source.Length <= MaxGradientText && Gradient(source, out gradient))
            {
                style.Gi = gradient;
                if (style.Bi != null) style.Bi.U = null;
                return true;
            }
            Uri url;
            if (source.Length == 0 || source.Length > MaxBackgroundText || !BitmapUri(source, origin, out url)) return false;
            var bitmap = BackgroundBitmap(style).Copy();
            bitmap.U = url;
            style.Bi = bitmap;
            style.Gi = null;
            return true;
        }

        private static void BackgroundRepeat(S style, string value)
        {
            var token = (value ?? "").Trim().ToLowerInvariant();
            string repeat;
            if (RadiusReset(token)) repeat = "b";
            else if (!BackgroundRepeatValue(token, out repeat)) return;
            var bitmap = BackgroundBitmap(style).Copy();
            bitmap.R = repeat;
            style.Bi = bitmap;
        }

        private static bool BackgroundRepeatValue(string value, out string repeat)
        {
            repeat = "";
            if (value == "repeat") repeat = "b";
            else if (value == "repeat-x") repeat = "x";
            else if (value == "repeat-y") repeat = "y";
            else if (value == "no-repeat") repeat = "n";
            else return false;
            return true;
        }

        private static void BackgroundPosition(S style, string value)
        {
            var source = (value ?? "").Trim();
            var bitmap = BackgroundBitmap(style).Copy();
            if (RadiusReset(source.ToLowerInvariant()))
            {
                bitmap.X = 0;
                bitmap.Y = 0;
                bitmap.Xp = true;
                bitmap.Yp = true;
                style.Bi = bitmap;
                return;
            }
            List<string> parts;
            if (!ShadowParts(source, '\0', out parts) || !BackgroundPosition(bitmap, parts)) return;
            style.Bi = bitmap;
        }

        private static bool BackgroundPosition(Bi bitmap, IList<string> parts)
        {
            if (parts == null || parts.Count < 1 || parts.Count > 2) return false;
            int x = 50;
            int y = 50;
            var xp = true;
            var yp = true;
            if (parts.Count == 1)
            {
                var token = parts[0].Trim().ToLowerInvariant();
                if (token == "top" || token == "bottom")
                {
                    if (!BackgroundPositionValue(token, false, out y, out yp)) return false;
                }
                else if (!BackgroundPositionValue(token, true, out x, out xp)) return false;
            }
            else if (!(BackgroundPositionValue(parts[0].Trim().ToLowerInvariant(), true, out x, out xp) &&
                       BackgroundPositionValue(parts[1].Trim().ToLowerInvariant(), false, out y, out yp)))
            {
                if (!(BackgroundPositionValue(parts[0].Trim().ToLowerInvariant(), false, out y, out yp) &&
                      BackgroundPositionValue(parts[1].Trim().ToLowerInvariant(), true, out x, out xp))) return false;
            }
            bitmap.X = x;
            bitmap.Y = y;
            bitmap.Xp = xp;
            bitmap.Yp = yp;
            return true;
        }

        private static bool BackgroundPositionValue(string value, bool horizontal, out int number, out bool percent)
        {
            number = 0;
            percent = true;
            if (value == "center") { number = 50; return true; }
            if ((horizontal && value == "left") || (!horizontal && value == "top")) return true;
            if (horizontal && value == "right" || !horizontal && value == "bottom") { number = 100; return true; }
            var match = Regex.Match(value ?? "", "^(?<n>[+-]?[0-9]{1,4})(?<u>%|px)?$", RegexOptions.IgnoreCase);
            int parsed;
            if (!match.Success || !Int32.TryParse(match.Groups["n"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)) return false;
            percent = match.Groups["u"].Value == "%";
            number = percent ? Math.Max(-1000, Math.Min(1000, parsed)) : Math.Max(-MaxBackgroundLength, Math.Min(MaxBackgroundLength, parsed));
            return true;
        }

        private static bool BackgroundPositionToken(string value)
        {
            int number;
            bool percent;
            return value == "top" || value == "bottom" || BackgroundPositionValue(value, true, out number, out percent);
        }

        private static void ObjectFit(S style, string value)
        {
            var token = (value ?? "").Trim().ToLowerInvariant();
            if (RadiusReset(token)) style.Of = "f";
            else if (token == "fill") style.Of = "f";
            else if (token == "contain") style.Of = "c";
            else if (token == "cover") style.Of = "v";
            else if (token == "none") style.Of = "n";
            else if (token == "scale-down") style.Of = "s";
        }

        private static void ObjectPosition(S style, string value)
        {
            var source = (value ?? "").Trim();
            if (RadiusReset(source.ToLowerInvariant()))
            {
                style.Ox = 50;
                style.Oy = 50;
                style.Oxp = true;
                style.Oyp = true;
                return;
            }
            List<string> parts;
            var position = new Bi();
            if (!ShadowParts(source, '\0', out parts) || !BackgroundPosition(position, parts)) return;
            style.Ox = position.X;
            style.Oy = position.Y;
            style.Oxp = position.Xp;
            style.Oyp = position.Yp;
        }

        private static void BackgroundSize(S style, string value)
        {
            var source = (value ?? "").Trim();
            var bitmap = BackgroundBitmap(style).Copy();
            if (RadiusReset(source.ToLowerInvariant()))
            {
                BackgroundSizeReset(bitmap);
                style.Bi = bitmap;
                return;
            }
            List<string> parts;
            if (!ShadowParts(source, '\0', out parts) || !BackgroundSize(bitmap, parts)) return;
            style.Bi = bitmap;
        }

        private static bool BackgroundSize(Bi bitmap, IList<string> parts)
        {
            if (parts == null || parts.Count < 1 || parts.Count > 2) return false;
            var first = parts[0].Trim().ToLowerInvariant();
            if (parts.Count == 1 && (first == "cover" || first == "contain"))
            {
                BackgroundSizeReset(bitmap);
                bitmap.K = first == "cover" ? "c" : "n";
                return true;
            }
            int width;
            int height = 0;
            bool widthPercent;
            bool heightPercent = false;
            bool widthAuto;
            bool heightAuto = true;
            if (!BackgroundSizeValue(first, out width, out widthPercent, out widthAuto)) return false;
            if (parts.Count == 2 && !BackgroundSizeValue(parts[1].Trim().ToLowerInvariant(), out height, out heightPercent, out heightAuto)) return false;
            bitmap.W = width;
            bitmap.H = height;
            bitmap.Wp = widthPercent;
            bitmap.Hp = heightPercent;
            bitmap.Wa = widthAuto;
            bitmap.Ha = heightAuto;
            bitmap.K = "";
            return true;
        }

        private static bool BackgroundSizeValue(string value, out int number, out bool percent, out bool auto)
        {
            number = 0;
            percent = false;
            auto = value == "auto";
            if (auto) return true;
            var match = Regex.Match(value ?? "", "^(?<n>[0-9]{1,4})(?<u>%|px)?$", RegexOptions.IgnoreCase);
            int parsed;
            if (!match.Success || !Int32.TryParse(match.Groups["n"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)) return false;
            percent = match.Groups["u"].Value == "%";
            number = Math.Min(percent ? 1000 : MaxBackgroundLength, parsed);
            return true;
        }

        private static void BackgroundSizeReset(Bi bitmap)
        {
            bitmap.W = 0;
            bitmap.H = 0;
            bitmap.Wp = false;
            bitmap.Hp = false;
            bitmap.Wa = true;
            bitmap.Ha = true;
            bitmap.K = "";
        }

        private static Bi BackgroundBitmap(S style)
        {
            return style.Bi ?? new Bi();
        }

        private static bool BitmapUri(string value, Uri origin, out Uri url)
        {
            url = null;
            var match = Regex.Match((value ?? "").Trim(), "^url\\((?<u>.*)\\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!match.Success) return false;
            var source = match.Groups["u"].Value.Trim();
            if (source.Length >= 2 && ((source[0] == '\'' && source[source.Length - 1] == '\'') || (source[0] == '"' && source[source.Length - 1] == '"')))
                source = source.Substring(1, source.Length - 2).Trim();
            if (source.Length == 0 || source.Length > MaxBackgroundText || source.IndexOfAny(new char[] { '\r', '\n', '\0' }) >= 0) return false;
            if (!(Uri.TryCreate(source, UriKind.Absolute, out url) || origin != null && Uri.TryCreate(origin, source, out url))) return false;
            if (!(url.IsFile || url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps || url.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase))) return false;
            if (url.IsFile && origin != null && !origin.IsFile) return false;
            return true;
        }

        private static bool BackgroundParts(string source, out List<string> parts)
        {
            parts = new List<string>();
            var start = 0;
            var quote = '\0';
            var depth = 0;
            for (var index = 0; index <= (source ?? "").Length; index++)
            {
                var end = index == source.Length;
                var value = end ? '\0' : source[index];
                if (!end && quote != '\0')
                {
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (!end && (value == '\'' || value == '"')) { quote = value; continue; }
                if (!end && value == '(') { depth++; continue; }
                if (!end && value == ')') { if (depth == 0) return false; depth--; continue; }
                if (!end && value == ',' && depth == 0) return false;
                var split = end || depth == 0 && (Char.IsWhiteSpace(value) || value == '/');
                if (!split) continue;
                var part = source.Substring(start, index - start).Trim();
                if (part.Length > 0) parts.Add(part);
                if (!end && value == '/') parts.Add("/");
                start = index + 1;
            }
            return quote == '\0' && depth == 0;
        }

        private static bool GradientSource(string value)
        {
            return value.StartsWith("linear-gradient(", StringComparison.Ordinal) || value.StartsWith("radial-gradient(", StringComparison.Ordinal);
        }

        private static bool Gradient(string source, out Gi gradient)
        {
            gradient = null;
            var match = Regex.Match(source ?? "", "^(?<k>linear|radial)-gradient\\((?<v>.*)\\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!match.Success) return false;
            List<string> groups;
            if (!ShadowParts(match.Groups["v"].Value, ',', out groups) || groups.Count < 2 || groups.Count > MaxGradientStops + 1) return false;
            var result = new Gi { K = match.Groups["k"].Value.Equals("radial", StringComparison.OrdinalIgnoreCase) ? "r" : "l" };
            var first = 0;
            if (result.K == "l")
            {
                int angle;
                if (LinearGradientHead(groups[0], out angle)) { result.A = angle; first = 1; }
            }
            else
            {
                int x;
                int y;
                if (RadialGradientHead(groups[0], out x, out y)) { result.X = x; result.Y = y; first = 1; }
            }
            if (groups.Count - first < 2 || groups.Count - first > MaxGradientStops) return false;
            var colors = new List<string>();
            var positions = new List<int>();
            for (var index = first; index < groups.Count; index++)
            {
                string color;
                int position;
                if (!GradientStop(groups[index], out color, out position)) return false;
                colors.Add(color);
                positions.Add(position);
            }
            NormalizeGradientStops(positions);
            for (var index = 0; index < colors.Count; index++) result.Ss.Add(new Cs(colors[index], positions[index]));
            gradient = result;
            return true;
        }

        private static bool LinearGradientHead(string value, out int angle)
        {
            angle = 180;
            var source = (value ?? "").Trim().ToLowerInvariant();
            var degrees = Regex.Match(source, "^(?<n>[+-]?[0-9]{1,4})(?:deg)$");
            int number;
            if (degrees.Success && Int32.TryParse(degrees.Groups["n"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                angle = ((number % 360) + 360) % 360;
                return true;
            }
            if (source == "to top") angle = 0;
            else if (source == "to top right" || source == "to right top") angle = 45;
            else if (source == "to right") angle = 90;
            else if (source == "to bottom right" || source == "to right bottom") angle = 135;
            else if (source == "to bottom") angle = 180;
            else if (source == "to bottom left" || source == "to left bottom") angle = 225;
            else if (source == "to left") angle = 270;
            else if (source == "to top left" || source == "to left top") angle = 315;
            else return false;
            return true;
        }

        private static bool RadialGradientHead(string value, out int x, out int y)
        {
            x = 50;
            y = 50;
            var source = Regex.Replace((value ?? "").Trim().ToLowerInvariant(), "\\s+", " ");
            if (!(source == "circle" || source == "ellipse" || source.StartsWith("circle at ", StringComparison.Ordinal) ||
                source.StartsWith("ellipse at ", StringComparison.Ordinal) || source.StartsWith("at ", StringComparison.Ordinal))) return false;
            if (source == "circle" || source == "ellipse") return true;
            var at = source.IndexOf(" at ", StringComparison.Ordinal);
            var position = at >= 0 ? source.Substring(at + 4) : source.Substring(3);
            var parts = position.Split(' ');
            if (parts.Length < 1 || parts.Length > 2) return false;
            if (parts.Length == 1)
            {
                if (GradientCoordinate(parts[0], true, out x)) return true;
                return GradientCoordinate(parts[0], false, out y);
            }
            if (GradientCoordinate(parts[0], true, out x) && GradientCoordinate(parts[1], false, out y)) return true;
            return GradientCoordinate(parts[0], false, out y) && GradientCoordinate(parts[1], true, out x);
        }

        private static bool GradientCoordinate(string value, bool horizontal, out int coordinate)
        {
            coordinate = 50;
            if (value == "center") return true;
            if (horizontal && value == "left" || !horizontal && value == "top") { coordinate = 0; return true; }
            if (horizontal && value == "right" || !horizontal && value == "bottom") { coordinate = 100; return true; }
            return GradientPercent(value, out coordinate);
        }

        private static bool GradientStop(string value, out string color, out int position)
        {
            color = "";
            position = -1;
            List<string> parts;
            if (!ShadowParts((value ?? "").Trim(), '\0', out parts) || parts.Count < 1 || parts.Count > 2 || !ShadowColor(parts[0])) return false;
            if (parts.Count == 2 && !GradientPercent(parts[1], out position)) return false;
            color = parts[0];
            return true;
        }

        private static bool GradientPercent(string value, out int position)
        {
            position = 0;
            var match = Regex.Match(value ?? "", "^(?<n>[+-]?[0-9]{1,4})%$");
            int number;
            if (!match.Success || !Int32.TryParse(match.Groups["n"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)) return false;
            position = Math.Max(0, Math.Min(100, number));
            return true;
        }

        private static void NormalizeGradientStops(List<int> positions)
        {
            if (positions.Count == 0) return;
            if (positions[0] < 0) positions[0] = 0;
            if (positions[positions.Count - 1] < 0) positions[positions.Count - 1] = 100;
            var last = positions[0];
            for (var index = 1; index < positions.Count; index++)
            {
                if (positions[index] >= 0) { positions[index] = Math.Max(last, positions[index]); last = positions[index]; continue; }
                var end = index + 1;
                while (end < positions.Count && positions[end] < 0) end++;
                var target = end < positions.Count ? Math.Max(last, positions[end]) : 100;
                var count = end - index + 1;
                for (var fill = index; fill < end; fill++) positions[fill] = last + (target - last) * (fill - index + 1) / count;
                index = end - 1;
            }
        }

        private static bool ShadowValue(string source, out Sw shadow)
        {
            shadow = null;
            List<string> parts;
            if (!ShadowParts(source, '\0', out parts) || parts.Count < 2 || parts.Count > 6) return false;
            var lengths = new List<int>();
            var inset = false;
            var color = "";
            foreach (var raw in parts)
            {
                var part = raw.Trim();
                if (part.Equals("inset", StringComparison.OrdinalIgnoreCase))
                {
                    if (inset) return false;
                    inset = true;
                    continue;
                }
                int length;
                if (ShadowLength(part, out length))
                {
                    if (lengths.Count == 4) return false;
                    lengths.Add(length);
                    continue;
                }
                if (color.Length > 0 || !ShadowColor(part)) return false;
                color = part;
            }
            if (lengths.Count < 2 || lengths.Count > 4 || (lengths.Count > 2 && lengths[2] < 0)) return false;
            shadow = new Sw
            {
                X = ShadowBound(lengths[0], 512),
                Y = ShadowBound(lengths[1], 512),
                B = lengths.Count > 2 ? Math.Min(lengths[2], 128) : 0,
                R = lengths.Count > 3 ? ShadowBound(lengths[3], 512) : 0,
                C = color,
                I = inset
            };
            return true;
        }

        private static bool ShadowLength(string value, out int length)
        {
            length = 0;
            var match = Regex.Match(value ?? "", "^(?<n>[+-]?[0-9]{1,4})(?:px)?$", RegexOptions.IgnoreCase);
            return match.Success && Int32.TryParse(match.Groups["n"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out length);
        }

        private static int ShadowBound(int value, int limit)
        {
            return Math.Max(-limit, Math.Min(limit, value));
        }

        private static bool ShadowColor(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length > 64) return false;
            var source = value.Trim();
            if (Regex.IsMatch(source, "^#[0-9a-f]{3}(?:[0-9a-f])?$|^#[0-9a-f]{6}(?:[0-9a-f]{2})?$", RegexOptions.IgnoreCase)) return true;
            if (Regex.IsMatch(source, "^[a-z]{1,32}$", RegexOptions.IgnoreCase)) return true;
            return ShadowFunctionColor(source);
        }

        private static bool ShadowFunctionColor(string source)
        {
            var match = Regex.Match(source ?? "", "^rgba?\\((?<v>[^()]{1,48})\\)$", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            var body = match.Groups["v"].Value.Trim();
            string[] channels;
            string alpha = null;
            if (body.IndexOf(',') >= 0)
            {
                var values = body.Split(',');
                if (values.Length < 3 || values.Length > 4) return false;
                channels = new string[] { values[0].Trim(), values[1].Trim(), values[2].Trim() };
                if (values.Length == 4) alpha = values[3].Trim();
            }
            else
            {
                var slash = body.IndexOf('/');
                if (slash >= 0)
                {
                    if (body.IndexOf('/', slash + 1) >= 0) return false;
                    alpha = body.Substring(slash + 1).Trim();
                    body = body.Substring(0, slash).Trim();
                }
                channels = Regex.Split(body, "\\s+");
                if (channels.Length != 3) return false;
            }
            return ShadowColorNumber(channels[0]) && ShadowColorNumber(channels[1]) && ShadowColorNumber(channels[2]) &&
                (alpha == null || ShadowColorNumber(alpha));
        }

        private static bool ShadowColorNumber(string source)
        {
            if (String.IsNullOrWhiteSpace(source)) return false;
            if (source.EndsWith("%", StringComparison.Ordinal)) source = source.Substring(0, source.Length - 1);
            double number;
            return Double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && !Double.IsNaN(number) && !Double.IsInfinity(number);
        }

        private static bool ShadowParts(string source, char separator, out List<string> parts)
        {
            parts = new List<string>();
            var start = 0;
            var quote = '\0';
            var parentheses = 0;
            for (var index = 0; index <= source.Length; index++)
            {
                var end = index == source.Length;
                var value = end ? separator : source[index];
                if (!end && quote != '\0')
                {
                    if (value == quote && (index == 0 || source[index - 1] != '\\')) quote = '\0';
                    continue;
                }
                if (!end && (value == '\'' || value == '"')) { quote = value; continue; }
                if (!end && value == '(') { parentheses++; continue; }
                if (!end && value == ')') { if (parentheses == 0) return false; parentheses--; continue; }
                var split = end || (parentheses == 0 && (separator == '\0' ? Char.IsWhiteSpace(value) : value == separator));
                if (!split) continue;
                var part = source.Substring(start, index - start).Trim();
                if (part.Length > 0) parts.Add(part);
                else if (separator != '\0') return false;
                start = index + 1;
            }
            return quote == '\0' && parentheses == 0;
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
            if (kind == "svg")
            {
                AddSvg(page, node, baseUrl, rules, style);
                return;
            }
            if (kind == "hr")
            {
                page.Bs.Add(new B { K = kind, X = "", S = style, El = node });
                return;
            }
            if (GaugeTag(kind))
            {
                page.Bs.Add(GaugeBlock(node, style));
                return;
            }
            if (ValueTag(kind))
            {
                AddValueBlock(page, node, baseUrl, rules, style);
                return;
            }
            if (kind == "a")
            {
                Uri link;
                var href = node.Av("href");
                var text = TextValue(RawText(node), style);
                if (href.Length > 0 && TryUri(baseUrl, href, out link))
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
                if (image != null) page.Bs.Add(ImageBlock(page, node, image, style));
            }
            else if (kind == "input" || kind == "textarea" || kind == "button" || kind == "select")
            {
                AddField(page, node, baseUrl, style);
                return;
            }
            else if (IsTextBlock(kind) || IsContainerBlock(kind) || Flex(style) || Grid(style))
            {
                var text = TextValue(Flex(style) || Grid(style) ? DirectText(node) : BlockText(node, PreservesLines(style)), style);
                var block = new B { K = kind, X = text, Mk = ListMarker(node), S = style, El = node };
                if (!Flex(style) && !Grid(style) && HasInlineStructure(node))
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
            if (source != null) page.Bs.Add(ImageBlock(page, image, source, style));
        }

        private static void AddSvg(P page, N svg, Uri baseUrl, List<R> rules, S style)
        {
            var model = SvgModel(page, svg, baseUrl, rules, style);
            if (model == null || model.Ss.Count == 0) return;
            page.Bs.Add(new B { K = "svg", X = SvgLabel(svg), S = style, Vg = model, El = svg });
        }

        private static Vg SvgModel(P page, N svg, Uri baseUrl, List<R> rules, S style)
        {
            var model = new Vg();
            var view = SvgNumbers(svg.Av("viewbox"), 4);
            if (view != null && view.Length == 4 && view[2] >= MinSvgView && view[3] >= MinSvgView)
            {
                model.X = view[0];
                model.Y = view[1];
                model.Vw = view[2];
                model.Vh = view[3];
                model.Vb = true;
                SvgAspect(svg.Av("preserveaspectratio"), model);
            }

            double width;
            double height;
            var hasWidth = SvgLength(svg.Av("width"), out width) && width > 0;
            var hasHeight = SvgLength(svg.Av("height"), out height) && height > 0;
            if (!hasWidth) width = model.Vb ? Math.Min(MaxSvgSize, model.Vw) : 300D;
            if (!hasHeight) height = model.Vb ? Math.Min(MaxSvgSize, model.Vh) : 150D;
            if (hasWidth && !hasHeight && model.Vb) height = Math.Min(MaxSvgSize, width * model.Vh / model.Vw);
            else if (hasHeight && !hasWidth && model.Vb) width = Math.Min(MaxSvgSize, height * model.Vw / model.Vh);
            model.W = Math.Max(1D, Math.Min(MaxSvgSize, width));
            model.H = Math.Max(1D, Math.Min(MaxSvgSize, height));
            if (!model.Vb)
            {
                model.Vw = model.W;
                model.Vh = model.H;
            }
            if (hasWidth && style.W == 0) style.W = Math.Max(1, (int)Math.Round(model.W));
            if (hasHeight && style.H == 0) style.H = Math.Max(1, (int)Math.Round(model.H));

            var sources = SvgSources(svg);
            SvgGradients(svg, rules, style, model);
            SvgClips(rules, style, model, sources);
            var initial = new Sv { C = style.F, Z = style.Fs > 0 ? style.Fs : 16D };
            var paint = SvgPaint(svg, initial, style, rules, true);
            foreach (var child in svg.C)
            {
                if (model.Ss.Count == MaxSvgShapes) break;
                SvgWalk(page, child, baseUrl, rules, style, paint, model, sources, new HashSet<string>(StringComparer.Ordinal), 1, 0, null, null);
            }
            return model;
        }

        private static void SvgWalk(P page, N node, Uri baseUrl, List<R> rules, S inherited, Sv parent, Vg model, Sx sources, ISet<string> usingIds, int depth, int useDepth, Uri activeLink, N linkElement)
        {
            if (node == null || node.K == "#text" || depth > MaxSvgDepth || model.Ss.Count == MaxSvgShapes) return;
            if (node.K == "script" || node.K == "style" || node.K == "defs" || node.A.ContainsKey("hidden")) return;
            var css = Style(node, inherited, rules);
            if (css.D == "none" || css.Vi == "h" || css.Vi == "c" || AriaTrue(node, "aria-hidden")) return;
            var paint = SvgPaint(node, parent, css, rules, false);
            if (node.K == "use")
            {
                SvgUse(page, node, baseUrl, rules, css, paint, model, sources, usingIds, depth, useDepth, activeLink, linkElement);
                return;
            }
            if (node.K == "symbol") return;
            var link = activeLink;
            var linkNode = linkElement;
            if (node.K == "a")
            {
                Uri target;
                if (TryUri(baseUrl, node.Av("href"), out target))
                {
                    link = target;
                    linkNode = node;
                    page.Ls.Add(new Ln { U = link, T = Clean(NodeText(node)) });
                }
            }

            var shape = SvgShape(node, paint, link, linkNode, model);
            if (shape != null) model.Ss.Add(shape);
            if (node.K != "g" && node.K != "a") return;
            foreach (var child in node.C)
            {
                if (model.Ss.Count == MaxSvgShapes) break;
                SvgWalk(page, child, baseUrl, rules, css, paint, model, sources, usingIds, depth + 1, useDepth, link, linkNode);
            }
        }

        private static void SvgUse(P page, N use, Uri baseUrl, List<R> rules, S css, Sv paint, Vg model, Sx sources, ISet<string> usingIds, int depth, int useDepth, Uri activeLink, N linkElement)
        {
            var id = SvgUseId(use);
            N target;
            if (id.Length == 0 || useDepth == MaxSvgUseLinks || usingIds.Contains(id) || !sources.Ns.TryGetValue(id, out target) || !SvgReusable(target.K)) return;
            double x;
            double y;
            if (!SvgNumber(use.Av("x"), 0D, out x) || !SvgNumber(use.Av("y"), 0D, out y)) return;
            paint = paint.Copy();
            paint.M = SvgMatrixMultiply(paint.M, new[] { 1D, 0D, 0D, 1D, x, y });
            if (!SvgMatrixSafe(paint.M)) return;
            usingIds.Add(id);
            if (target.K == "symbol")
            {
                var symbolCss = Style(target, css, rules);
                if (symbolCss.D != "none" && symbolCss.Vi != "h" && symbolCss.Vi != "c" && !AriaTrue(target, "aria-hidden"))
                {
                    var symbolPaint = SvgPaint(target, paint, symbolCss, rules, false);
                    foreach (var child in target.C)
                    {
                        if (model.Ss.Count == MaxSvgShapes) break;
                        SvgWalk(page, child, baseUrl, rules, symbolCss, symbolPaint, model, sources, usingIds, depth + 1, useDepth + 1, activeLink, linkElement);
                    }
                }
            }
            else SvgWalk(page, target, baseUrl, rules, css, paint, model, sources, usingIds, depth + 1, useDepth + 1, activeLink, linkElement);
            usingIds.Remove(id);
        }

        private static Vs SvgShape(N node, Sv paint, Uri link, N linkElement, Vg model)
        {
            var kind = node.K;
            if (kind != "rect" && kind != "circle" && kind != "ellipse" && kind != "line" && kind != "polyline" && kind != "polygon" && kind != "text" && kind != "path") return null;
            var shape = new Vs
            {
                K = kind,
                F = paint.F,
                S = paint.S,
                C = paint.C,
                Fr = paint.R,
                Lc = paint.Lc,
                Lj = paint.Lj,
                W = paint.W,
                O = paint.O,
                Fo = paint.Fo,
                So = paint.So,
                Z = paint.Z,
                Ml = paint.Ml,
                Do = paint.Do,
                A = paint.A,
                U = link,
                El = linkElement ?? node
            };
            foreach (var dash in paint.Da) shape.Da.Add(dash);
            for (var index = 0; index < 6; index++)
            {
                shape.M[index] = paint.M[index];
                shape.Cm[index] = paint.Xm[index];
            }
            shape.Cl = paint.X.Length > 0;
            shape.Cp = SvgClipReference(paint.X, model);
            shape.Fd = SvgGradientReference(paint.F, model);
            shape.Sd = SvgGradientReference(paint.S, model);
            if (SvgPaintServer(paint.F) && shape.Fd == null) shape.F = "none";
            if (SvgPaintServer(paint.S) && shape.Sd == null) shape.S = "none";
            double value;
            if (kind == "rect")
            {
                if (!SvgNumber(node.Av("x"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("y"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("width"), -1, out value) || value <= 0) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("height"), -1, out value) || value <= 0) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("rx"), 0, out value)) return null; shape.P.Add(Math.Max(0, value));
                if (!SvgNumber(node.Av("ry"), 0, out value)) return null; shape.P.Add(Math.Max(0, value));
            }
            else if (kind == "circle")
            {
                if (!SvgNumber(node.Av("cx"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("cy"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("r"), -1, out value) || value <= 0) return null; shape.P.Add(value);
            }
            else if (kind == "ellipse")
            {
                if (!SvgNumber(node.Av("cx"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("cy"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("rx"), -1, out value) || value <= 0) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("ry"), -1, out value) || value <= 0) return null; shape.P.Add(value);
            }
            else if (kind == "line")
            {
                foreach (var name in new[] { "x1", "y1", "x2", "y2" })
                {
                    if (!SvgNumber(node.Av(name), 0, out value)) return null;
                    shape.P.Add(value);
                }
            }
            else if (kind == "polyline" || kind == "polygon")
            {
                var points = SvgNumbers(node.Av("points"), MaxSvgPoints * 2);
                if (points == null || points.Length % 2 != 0 || points.Length < (kind == "polygon" ? 6 : 4)) return null;
                foreach (var point in points) shape.P.Add(point);
            }
            else if (kind == "text")
            {
                if (!SvgNumber(node.Av("x"), 0, out value)) return null; shape.P.Add(value);
                if (!SvgNumber(node.Av("y"), 0, out value)) return null; shape.P.Add(value);
                shape.T = BoundValue(Clean(NodeText(node)), MaxSvgText);
                if (shape.T.Length == 0) return null;
            }
            else if (!SvgPath(node.Av("d"), shape.D)) return null;
            return shape;
        }

        private static Sx SvgSources(N svg)
        {
            var result = new Sx();
            SvgSourceNodes(svg, 0, result);
            return result;
        }

        private static void SvgSourceNodes(N node, int depth, Sx result)
        {
            if (node == null || depth > MaxSvgDepth || result.Os.Count == MaxSvgSources) return;
            var id = node.Av("id");
            if (SvgGradientId(id) && !result.Ns.ContainsKey(id))
            {
                result.Ns[id] = node;
                result.Os.Add(node);
            }
            foreach (var child in node.C)
            {
                if (result.Os.Count == MaxSvgSources) break;
                SvgSourceNodes(child, depth + 1, result);
            }
        }

        private static string SvgUseId(N node)
        {
            var href = node.Av("href");
            if (href.Length == 0) href = node.Av("xlink:href");
            var match = Regex.Match(href ?? "", "^\\s*#(?<i>[A-Za-z_][A-Za-z0-9_.:-]{0,63})\\s*$");
            return match.Success ? match.Groups["i"].Value : "";
        }

        private static bool SvgReusable(string kind)
        {
            return kind == "rect" || kind == "circle" || kind == "ellipse" || kind == "line" || kind == "polyline" || kind == "polygon" ||
                kind == "text" || kind == "path" || kind == "g" || kind == "symbol" || kind == "use" || kind == "a";
        }

        private static void SvgClips(List<R> rules, S inherited, Vg model, Sx sources)
        {
            foreach (var node in sources.Os)
            {
                if (node.K != "clippath" || model.Cs.Count == MaxSvgClips) continue;
                var clip = SvgClip(node, rules, inherited, model, sources);
                if (clip != null && SvgClipById(model, clip.I) == null) model.Cs.Add(clip);
            }
        }

        private static Vc SvgClip(N node, List<R> rules, S inherited, Vg model, Sx sources)
        {
            var units = node.Av("clippathunits").Trim().ToLowerInvariant();
            if (units.Length > 0 && units != "userspaceonuse" && units != "objectboundingbox") return null;
            var css = Style(node, inherited, rules);
            if (css.D == "none" || css.Vi == "h" || css.Vi == "c" || AriaTrue(node, "aria-hidden")) return null;
            var initial = new Sv { C = inherited.F, Z = inherited.Fs > 0 ? inherited.Fs : 16D };
            var paint = SvgPaint(node, initial, css, rules, false);
            paint.X = "";
            paint.Xm = SvgIdentity();
            var clip = new Vc { I = node.Av("id"), U = units == "objectboundingbox" ? "b" : "u" };
            for (var index = 0; index < 6; index++) clip.M[index] = paint.M[index];
            foreach (var child in node.C)
            {
                if (clip.Ss.Count == MaxSvgClipShapes) break;
                SvgClipWalk(child, rules, css, paint, model, sources, new HashSet<string>(StringComparer.Ordinal), clip, 1, 0);
            }
            return clip.Ss.Count > 0 ? clip : null;
        }

        private static void SvgClipWalk(N node, List<R> rules, S inherited, Sv parent, Vg model, Sx sources, ISet<string> usingIds, Vc clip, int depth, int useDepth)
        {
            if (node == null || node.K == "#text" || depth > MaxSvgDepth || clip.Ss.Count == MaxSvgClipShapes) return;
            if (node.K == "script" || node.K == "style" || node.K == "defs" || node.A.ContainsKey("hidden")) return;
            var css = Style(node, inherited, rules);
            if (css.D == "none" || css.Vi == "h" || css.Vi == "c" || AriaTrue(node, "aria-hidden")) return;
            var paint = SvgPaint(node, parent, css, rules, false);
            paint.X = "";
            paint.Xm = SvgIdentity();
            if (node.K == "use")
            {
                SvgClipUse(node, rules, css, paint, model, sources, usingIds, clip, depth, useDepth);
                return;
            }
            if (node.K == "symbol") return;
            var shape = SvgShape(node, paint, null, null, model);
            if (shape != null)
            {
                shape.Fr = paint.Cr;
                shape.Cl = false;
                shape.Cp = null;
                clip.Ss.Add(shape);
            }
            if (node.K != "g" && node.K != "a") return;
            foreach (var child in node.C)
            {
                if (clip.Ss.Count == MaxSvgClipShapes) break;
                SvgClipWalk(child, rules, css, paint, model, sources, usingIds, clip, depth + 1, useDepth);
            }
        }

        private static void SvgClipUse(N use, List<R> rules, S css, Sv paint, Vg model, Sx sources, ISet<string> usingIds, Vc clip, int depth, int useDepth)
        {
            var id = SvgUseId(use);
            N target;
            if (id.Length == 0 || useDepth == MaxSvgUseLinks || usingIds.Contains(id) || !sources.Ns.TryGetValue(id, out target) || !SvgReusable(target.K)) return;
            double x;
            double y;
            if (!SvgNumber(use.Av("x"), 0D, out x) || !SvgNumber(use.Av("y"), 0D, out y)) return;
            paint = paint.Copy();
            paint.M = SvgMatrixMultiply(paint.M, new[] { 1D, 0D, 0D, 1D, x, y });
            if (!SvgMatrixSafe(paint.M)) return;
            usingIds.Add(id);
            if (target.K == "symbol")
            {
                var symbolCss = Style(target, css, rules);
                if (symbolCss.D != "none" && symbolCss.Vi != "h" && symbolCss.Vi != "c" && !AriaTrue(target, "aria-hidden"))
                {
                    var symbolPaint = SvgPaint(target, paint, symbolCss, rules, false);
                    foreach (var child in target.C)
                    {
                        if (clip.Ss.Count == MaxSvgClipShapes) break;
                        SvgClipWalk(child, rules, symbolCss, symbolPaint, model, sources, usingIds, clip, depth + 1, useDepth + 1);
                    }
                }
            }
            else SvgClipWalk(target, rules, css, paint, model, sources, usingIds, clip, depth + 1, useDepth + 1);
            usingIds.Remove(id);
        }

        private static Vc SvgClipReference(string id, Vg model)
        {
            return id.Length > 0 ? SvgClipById(model, id) : null;
        }

        private static Vc SvgClipById(Vg model, string id)
        {
            foreach (var clip in model.Cs) if (clip.I == id) return clip;
            return null;
        }

        private static void SvgGradients(N svg, List<R> rules, S inherited, Vg model)
        {
            var nodes = new List<N>();
            SvgGradientNodes(svg, false, 0, nodes);
            var sources = new Dictionary<string, N>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                var id = node.Av("id");
                if (!SvgGradientId(id) || sources.ContainsKey(id) || sources.Count == MaxSvgGradients) continue;
                sources[id] = node;
            }
            var built = new Dictionary<string, Vd>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                var id = node.Av("id");
                if (!sources.ContainsKey(id) || model.Ds.Count == MaxSvgGradients) continue;
                var gradient = SvgGradient(node, sources, built, new HashSet<string>(StringComparer.Ordinal), rules, inherited, model, 0);
                if (gradient != null && SvgGradientById(model, id) == null) model.Ds.Add(gradient);
            }
        }

        private static void SvgGradientNodes(N node, bool definitions, int depth, IList<N> result)
        {
            if (node == null || depth > MaxSvgDepth || result.Count >= MaxSvgGradients * 2) return;
            definitions = definitions || node.K == "defs";
            if (definitions && (node.K == "lineargradient" || node.K == "radialgradient")) result.Add(node);
            foreach (var child in node.C)
            {
                if (result.Count >= MaxSvgGradients * 2) break;
                SvgGradientNodes(child, definitions, depth + 1, result);
            }
        }

        private static Vd SvgGradient(N node, IDictionary<string, N> sources, IDictionary<string, Vd> built, ISet<string> resolving, List<R> rules, S inherited, Vg model, int depth)
        {
            var id = node.Av("id");
            Vd cached;
            if (built.TryGetValue(id, out cached)) return cached;
            if (depth > MaxSvgGradientLinks || resolving.Contains(id)) return null;
            resolving.Add(id);
            var kind = node.K == "lineargradient" ? "l" : node.K == "radialgradient" ? "r" : "";
            if (kind.Length == 0) { resolving.Remove(id); return null; }

            Vd gradient = null;
            var hrefId = SvgGradientHref(node);
            N hrefNode;
            if (hrefId.Length > 0 && sources.TryGetValue(hrefId, out hrefNode))
            {
                var inheritedGradient = SvgGradient(hrefNode, sources, built, resolving, rules, inherited, model, depth + 1);
                if (inheritedGradient == null) { resolving.Remove(id); return null; }
                if (inheritedGradient.K == kind) gradient = SvgGradientCopy(inheritedGradient);
            }
            var inheritedCoordinates = gradient != null;
            if (gradient == null) gradient = SvgGradientDefault(kind);
            gradient.I = id;
            gradient.K = kind;

            var inheritedUnits = gradient.U;
            var units = node.Av("gradientunits").Trim().ToLowerInvariant();
            if (units.Length > 0)
            {
                if (units == "objectboundingbox") gradient.U = "b";
                else if (units == "userspaceonuse") gradient.U = "u";
                else { resolving.Remove(id); return null; }
                if (inheritedCoordinates && inheritedUnits != gradient.U && !SvgGradientCoordinatesComplete(node, kind)) { resolving.Remove(id); return null; }
            }
            var spread = node.Av("spreadmethod").Trim().ToLowerInvariant();
            if (spread.Length > 0 && spread != "pad") { resolving.Remove(id); return null; }
            var transform = node.Av("gradienttransform");
            if (!String.IsNullOrWhiteSpace(transform))
            {
                double[] matrix;
                if (!SvgTransform(transform, out matrix)) { resolving.Remove(id); return null; }
                for (var index = 0; index < 6; index++) gradient.M[index] = matrix[index];
            }
            if (!SvgGradientCoordinates(node, gradient, model, inheritedCoordinates)) { resolving.Remove(id); return null; }

            bool authoredStops;
            List<Vt> stops;
            if (!SvgGradientStops(node, rules, inherited, out authoredStops, out stops)) { resolving.Remove(id); return null; }
            if (authoredStops)
            {
                gradient.Ss.Clear();
                foreach (var stop in stops) gradient.Ss.Add(stop);
            }
            if (gradient.Ss.Count == 0) { resolving.Remove(id); return null; }
            built[id] = gradient;
            resolving.Remove(id);
            return gradient;
        }

        private static Vd SvgGradientDefault(string kind)
        {
            var gradient = new Vd { K = kind };
            if (kind == "l")
            {
                foreach (var value in new[] { 0D, 0D, 1D, 0D }) gradient.P.Add(value);
            }
            else
            {
                foreach (var value in new[] { .5D, .5D, .5D, .5D, .5D }) gradient.P.Add(value);
            }
            return gradient;
        }

        private static Vd SvgGradientCopy(Vd source)
        {
            var copy = new Vd { I = source.I, K = source.K, U = source.U };
            foreach (var value in source.P) copy.P.Add(value);
            for (var index = 0; index < 6; index++) copy.M[index] = source.M[index];
            foreach (var stop in source.Ss) copy.Ss.Add(new Vt(stop.O, stop.C, stop.A));
            return copy;
        }

        private static bool SvgGradientCoordinates(N node, Vd gradient, Vg model, bool inherited)
        {
            if (gradient.K == "l")
            {
                return SvgGradientCoordinate(node, "x1", 'x', gradient, model, 0) &&
                    SvgGradientCoordinate(node, "y1", 'y', gradient, model, 1) &&
                    SvgGradientCoordinate(node, "x2", 'x', gradient, model, 2) &&
                    SvgGradientCoordinate(node, "y2", 'y', gradient, model, 3);
            }
            var defaultFocusX = !inherited && node.Av("fx").Length == 0 && node.Av("cx").Length > 0;
            var defaultFocusY = !inherited && node.Av("fy").Length == 0 && node.Av("cy").Length > 0;
            if (!SvgGradientCoordinate(node, "cx", 'x', gradient, model, 0) ||
                !SvgGradientCoordinate(node, "cy", 'y', gradient, model, 1) ||
                !SvgGradientCoordinate(node, "r", 'r', gradient, model, 2)) return false;
            if (defaultFocusX) gradient.P[3] = gradient.P[0];
            if (defaultFocusY) gradient.P[4] = gradient.P[1];
            return SvgGradientCoordinate(node, "fx", 'x', gradient, model, 3) &&
                SvgGradientCoordinate(node, "fy", 'y', gradient, model, 4) && gradient.P[2] > 0D;
        }

        private static bool SvgGradientCoordinatesComplete(N node, string kind)
        {
            if (kind == "l") return node.Av("x1").Length > 0 && node.Av("y1").Length > 0 && node.Av("x2").Length > 0 && node.Av("y2").Length > 0;
            return node.Av("cx").Length > 0 && node.Av("cy").Length > 0 && node.Av("r").Length > 0 && node.Av("fx").Length > 0 && node.Av("fy").Length > 0;
        }

        private static bool SvgGradientCoordinate(N node, string name, char axis, Vd gradient, Vg model, int index)
        {
            var input = node.Av(name);
            if (String.IsNullOrWhiteSpace(input)) return true;
            var match = Regex.Match(input, "^\\s*(?<n>[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?)(?<p>%?)\\s*$", RegexOptions.IgnoreCase);
            double value;
            if (!match.Success || !SvgFinite(match.Groups["n"].Value, out value)) return false;
            var percent = match.Groups["p"].Value.Length > 0;
            if (percent) value /= 100D;
            if (gradient.U == "b")
            {
                if (Math.Abs(value) > 100D) return false;
            }
            else if (percent)
            {
                if (axis == 'x') value = model.X + model.Vw * value;
                else if (axis == 'y') value = model.Y + model.Vh * value;
                else value = Math.Sqrt(model.Vw * model.Vw + model.Vh * model.Vh) / Math.Sqrt(2D) * value;
            }
            else if (Math.Abs(value) > MaxSvgCoordinate) return false;
            gradient.P[index] = value;
            return axis != 'r' || value > 0D;
        }

        private static bool SvgGradientStops(N node, List<R> rules, S inherited, out bool authored, out List<Vt> result)
        {
            authored = false;
            result = new List<Vt>();
            var count = 0;
            foreach (var child in node.C) if (child.K == "stop") count++;
            if (count == 0) return true;
            authored = true;
            if (count > MaxSvgStops) return false;
            var gradientStyle = Style(node, inherited, rules);
            var previous = 0D;
            foreach (var stop in node.C)
            {
                if (stop.K != "stop") continue;
                double offset;
                if (!SvgStopOffset(stop.Av("offset"), out offset)) return false;
                offset = Math.Max(previous, offset);
                previous = offset;
                var stopStyle = Style(stop, gradientStyle, rules);
                var color = stop.Av("stop-color").Trim();
                if (color.Length == 0) color = "#000000";
                var alpha = 1D;
                double authoredAlpha;
                if (stop.Av("stop-opacity").Length > 0 && SvgAlpha(stop.Av("stop-opacity"), out authoredAlpha)) alpha = authoredAlpha;
                foreach (var rule in rules)
                    if (MatchCss(stop, rule)) SvgStopDeclarations(rule.Ds, false, stopStyle, ref color, ref alpha);
                SvgStopDeclarations(CssDeclarations(stop.Av("style")), false, stopStyle, ref color, ref alpha);
                foreach (var rule in rules)
                    if (MatchCss(stop, rule)) SvgStopDeclarations(rule.Ds, true, stopStyle, ref color, ref alpha);
                SvgStopDeclarations(CssDeclarations(stop.Av("style")), true, stopStyle, ref color, ref alpha);
                if (color.Equals("currentcolor", StringComparison.OrdinalIgnoreCase)) color = stopStyle.F;
                if (color.Length == 0 || color.Length > 64 || color.IndexOf("url(", StringComparison.OrdinalIgnoreCase) >= 0) return false;
                result.Add(new Vt(offset, color, alpha));
            }
            return result.Count > 0;
        }

        private static void SvgStopDeclarations(Pd[] declarations, bool important, S style, ref string color, ref double alpha)
        {
            foreach (var declaration in declarations)
            {
                if (declaration.C || declaration.I != important) continue;
                string value;
                if (!VarValue(style, declaration.V, out value)) continue;
                if (declaration.N == "stop-color") color = value.Trim();
                else if (declaration.N == "stop-opacity")
                {
                    double number;
                    if (SvgAlpha(value, out number)) alpha = number;
                }
            }
        }

        private static bool SvgStopOffset(string input, out double value)
        {
            value = 0D;
            var text = (input ?? "").Trim();
            if (text.Length == 0) return true;
            var percent = text.EndsWith("%", StringComparison.Ordinal);
            if (percent) text = text.Substring(0, text.Length - 1).Trim();
            if (!SvgFinite(text, out value)) return false;
            if (percent) value /= 100D;
            value = Math.Max(0D, Math.Min(1D, value));
            return true;
        }

        private static string SvgGradientHref(N node)
        {
            var href = node.Av("href");
            if (href.Length == 0) href = node.Av("xlink:href");
            var match = Regex.Match(href ?? "", "^\\s*#(?<i>[A-Za-z_][A-Za-z0-9_.:-]{0,63})\\s*$");
            return match.Success ? match.Groups["i"].Value : "";
        }

        private static bool SvgGradientId(string value)
        {
            return !String.IsNullOrEmpty(value) && value.Length <= MaxSvgGradientId && Regex.IsMatch(value, "^[A-Za-z_][A-Za-z0-9_.:-]*$");
        }

        private static bool SvgPaintServer(string value)
        {
            return Regex.IsMatch((value ?? "").Trim(), "^url\\(\\s*['\"]?#(?<i>[A-Za-z_][A-Za-z0-9_.:-]{0,63})['\"]?\\s*\\)$", RegexOptions.IgnoreCase);
        }

        private static Vd SvgGradientReference(string value, Vg model)
        {
            var match = Regex.Match((value ?? "").Trim(), "^url\\(\\s*['\"]?#(?<i>[A-Za-z_][A-Za-z0-9_.:-]{0,63})['\"]?\\s*\\)$", RegexOptions.IgnoreCase);
            return match.Success ? SvgGradientById(model, match.Groups["i"].Value) : null;
        }

        private static Vd SvgGradientById(Vg model, string id)
        {
            foreach (var gradient in model.Ds) if (gradient.I == id) return gradient;
            return null;
        }

        private static Sv SvgPaint(N node, Sv inherited, S css, List<R> rules, bool root)
        {
            var paint = inherited.Copy();
            double[] transform = null;
            var clipChanged = false;
            SvgTransform(node.Av("transform"), out transform);
            paint.C = css != null && css.F.Length > 0 ? css.F : paint.C;
            if (css != null && css.Fs > 0) paint.Z = css.Fs;
            SvgPaintSet(paint, "fill", node.Av("fill"));
            SvgPaintSet(paint, "stroke", node.Av("stroke"));
            SvgPaintSet(paint, "stroke-width", node.Av("stroke-width"));
            SvgPaintSet(paint, "opacity", node.Av("opacity"));
            SvgPaintSet(paint, "fill-opacity", node.Av("fill-opacity"));
            SvgPaintSet(paint, "stroke-opacity", node.Av("stroke-opacity"));
            SvgPaintSet(paint, "font-size", node.Av("font-size"));
            SvgPaintSet(paint, "text-anchor", node.Av("text-anchor"));
            SvgPaintSet(paint, "fill-rule", node.Av("fill-rule"));
            SvgPaintSet(paint, "clip-rule", node.Av("clip-rule"));
            SvgPaintSet(paint, "stroke-linecap", node.Av("stroke-linecap"));
            SvgPaintSet(paint, "stroke-linejoin", node.Av("stroke-linejoin"));
            SvgPaintSet(paint, "stroke-miterlimit", node.Av("stroke-miterlimit"));
            SvgPaintSet(paint, "stroke-dasharray", node.Av("stroke-dasharray"));
            SvgPaintSet(paint, "stroke-dashoffset", node.Av("stroke-dashoffset"));
            SvgClipSet(paint, node.Av("clip-path"), ref clipChanged);
            foreach (var rule in rules)
                if (MatchCss(node, rule)) SvgPaintDeclarations(rule.Ds, false, css, paint, ref transform, ref clipChanged, root);
            var inline = CssDeclarations(node.Av("style"));
            SvgPaintDeclarations(inline, false, css, paint, ref transform, ref clipChanged, root);
            foreach (var rule in rules)
                if (MatchCss(node, rule)) SvgPaintDeclarations(rule.Ds, true, css, paint, ref transform, ref clipChanged, root);
            SvgPaintDeclarations(inline, true, css, paint, ref transform, ref clipChanged, root);
            if (transform != null) paint.M = SvgMatrixMultiply(inherited.M, transform);
            if (clipChanged) paint.Xm = (double[])paint.M.Clone();
            return paint;
        }

        private static void SvgPaintDeclarations(Pd[] declarations, bool important, S style, Sv paint, ref double[] transform, ref bool clipChanged, bool root)
        {
            foreach (var declaration in declarations)
            {
                if (declaration.C || declaration.I != important) continue;
                string value;
                if (!VarValue(style, declaration.V, out value)) continue;
                if (declaration.N == "clip-path") SvgClipSet(paint, value, ref clipChanged);
                else if (declaration.N == "transform")
                {
                    double[] parsed;
                    if (!root && SvgTransform(value, out parsed)) transform = parsed;
                }
                else if (!root || declaration.N != "opacity") SvgPaintSet(paint, declaration.N, value);
            }
        }

        private static void SvgClipSet(Sv paint, string input, ref bool changed)
        {
            var value = (input ?? "").Trim();
            if (value.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                paint.X = "";
                changed = true;
                return;
            }
            var match = Regex.Match(value, "^url\\(\\s*['\"]?#(?<i>[A-Za-z_][A-Za-z0-9_.:-]{0,63})['\"]?\\s*\\)$", RegexOptions.IgnoreCase);
            if (!match.Success) return;
            paint.X = match.Groups["i"].Value;
            changed = true;
        }

        private static void SvgPaintSet(Sv paint, string name, string input)
        {
            var value = (input ?? "").Trim();
            if (value.Length == 0) return;
            if (name == "fill" || name == "stroke")
            {
                if (value.Length > 128 || value.IndexOf("url(", StringComparison.OrdinalIgnoreCase) >= 0 && !SvgPaintServer(value)) return;
                if (name == "fill") paint.F = value;
                else paint.S = value;
                return;
            }
            double number;
            if (name == "stroke-width" && SvgLength(value, out number)) paint.W = Math.Max(0, Math.Min(256D, number));
            else if (name == "opacity" && SvgAlpha(value, out number)) paint.O *= number;
            else if (name == "fill-opacity" && SvgAlpha(value, out number)) paint.Fo = number;
            else if (name == "stroke-opacity" && SvgAlpha(value, out number)) paint.So = number;
            else if (name == "font-size" && SvgLength(value, out number) && number > 0) paint.Z = Math.Min(200D, number);
            else if (name == "fill-rule") SvgRule(value, ref paint.R);
            else if (name == "clip-rule") SvgRule(value, ref paint.Cr);
            else if (name == "stroke-linecap")
            {
                var cap = value.ToLowerInvariant();
                if (cap == "butt") paint.Lc = "b";
                else if (cap == "round") paint.Lc = "r";
                else if (cap == "square") paint.Lc = "s";
            }
            else if (name == "stroke-linejoin")
            {
                var join = value.ToLowerInvariant();
                if (join == "miter") paint.Lj = "m";
                else if (join == "round") paint.Lj = "r";
                else if (join == "bevel") paint.Lj = "b";
            }
            else if (name == "stroke-miterlimit" && SvgNumber(value, 4D, out number) && number >= 1D) paint.Ml = Math.Min(MaxSvgMiterLimit, number);
            else if (name == "stroke-dashoffset" && SvgLength(value, out number)) paint.Do = number;
            else if (name == "stroke-dasharray")
            {
                double[] dashes;
                if (SvgDashes(value, out dashes)) paint.Da = dashes;
            }
            else if (name == "text-anchor")
            {
                var anchor = value.ToLowerInvariant();
                if (anchor == "start") paint.A = "s";
                else if (anchor == "middle") paint.A = "m";
                else if (anchor == "end") paint.A = "e";
            }
        }

        private static void SvgRule(string value, ref string target)
        {
            var rule = value.ToLowerInvariant();
            if (rule == "nonzero") target = "n";
            else if (rule == "evenodd") target = "e";
        }

        private static bool SvgDashes(string input, out double[] result)
        {
            result = null;
            var source = (input ?? "").Trim();
            if (source.Equals("none", StringComparison.OrdinalIgnoreCase)) { result = new double[0]; return true; }
            if (source.Length == 0 || source.Length > MaxSvgDashText) return false;
            var matches = Regex.Matches(source, "(?<n>[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?)(?:px)?", RegexOptions.IgnoreCase);
            if (matches.Count == 0 || matches.Count > MaxSvgDashes || matches.Count % 2 == 1 && matches.Count * 2 > MaxSvgDashes) return false;
            var values = new List<double>();
            var end = 0;
            var total = 0D;
            foreach (Match match in matches)
            {
                double value;
                if (!SvgSeparators(source, end, match.Index) || !SvgFinite(match.Groups["n"].Value, out value) || value < 0D || value > MaxSvgSize) return false;
                values.Add(value);
                total += value;
                end = match.Index + match.Length;
            }
            if (!SvgSeparators(source, end, source.Length) || total <= 0D) return false;
            if (values.Count % 2 == 1) values.AddRange(values.ToArray());
            result = values.ToArray();
            return true;
        }

        private static string SvgLabel(N svg)
        {
            var label = Clean(svg.Av("aria-label"));
            if (label.Length == 0)
            {
                var title = FirstChild(svg, "title");
                if (title != null) label = Clean(NodeText(title));
            }
            return BoundValue(label, MaxSvgText);
        }

        private static bool SvgLength(string input, out double value)
        {
            value = 0;
            var match = Regex.Match(input ?? "", "^\\s*(?<n>[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?)(?:px)?\\s*$", RegexOptions.IgnoreCase);
            return match.Success && SvgFinite(match.Groups["n"].Value, out value) && value >= -MaxSvgSize && value <= MaxSvgSize;
        }

        private static bool SvgNumber(string input, double fallback, out double value)
        {
            value = fallback;
            if (String.IsNullOrWhiteSpace(input)) { value = fallback; return true; }
            var match = Regex.Match(input, "^\\s*(?<n>[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?)\\s*$", RegexOptions.IgnoreCase);
            return match.Success && SvgFinite(match.Groups["n"].Value, out value) && Math.Abs(value) <= MaxSvgCoordinate;
        }

        private static bool SvgAlpha(string input, out double value)
        {
            value = 1;
            var text = (input ?? "").Trim();
            var percent = text.EndsWith("%", StringComparison.Ordinal);
            if (percent) text = text.Substring(0, text.Length - 1).Trim();
            if (!SvgFinite(text, out value)) return false;
            if (percent) value /= 100D;
            value = Math.Max(0D, Math.Min(1D, value));
            return true;
        }

        private static double[] SvgNumbers(string input, int maximum)
        {
            var source = input ?? "";
            if (source.Length == 0 || source.Length > MaxHtmlAttributeValue) return null;
            var matches = Regex.Matches(source, "[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?", RegexOptions.IgnoreCase);
            if (matches.Count == 0 || matches.Count > maximum) return null;
            var result = new double[matches.Count];
            var end = 0;
            for (var index = 0; index < matches.Count; index++)
            {
                var match = matches[index];
                if (!SvgSeparators(source, end, match.Index) || !SvgFinite(match.Value, out result[index]) || Math.Abs(result[index]) > MaxSvgCoordinate) return null;
                end = match.Index + match.Length;
            }
            return SvgSeparators(source, end, source.Length) ? result : null;
        }

        private static void SvgAspect(string input, Vg model)
        {
            var value = Regex.Replace((input ?? "").Trim(), "\\s+", " ").ToLowerInvariant();
            if (value.Length == 0) return;
            var parts = value.Split(' ');
            if (parts.Length > 2 || parts[0] == "defer") return;
            if (parts[0] == "none")
            {
                if (parts.Length == 1) model.A = "n";
                return;
            }
            var match = Regex.Match(parts[0], "^x(?<x>min|mid|max)y(?<y>min|mid|max)$", RegexOptions.IgnoreCase);
            if (!match.Success) return;
            if (parts.Length == 2 && parts[1] != "meet" && parts[1] != "slice") return;
            model.A = SvgAspectPart(match.Groups["x"].Value) + SvgAspectPart(match.Groups["y"].Value);
            model.M = parts.Length == 2 && parts[1] == "slice" ? "s" : "m";
        }

        private static string SvgAspectPart(string value)
        {
            return value == "min" ? "n" : value == "max" ? "x" : "m";
        }

        private static bool SvgTransform(string input, out double[] matrix)
        {
            matrix = null;
            var source = (input ?? "").Trim();
            if (source.Equals("none", StringComparison.OrdinalIgnoreCase)) { matrix = SvgIdentity(); return true; }
            if (source.Length == 0 || source.Length > MaxSvgTransformText) return false;
            var matches = Regex.Matches(source, "(?<n>matrix|translate|scale|rotate|skewx|skewy)\\s*\\((?<v>[^()]*)\\)", RegexOptions.IgnoreCase);
            if (matches.Count == 0 || matches.Count > MaxSvgTransforms) return false;
            var result = SvgIdentity();
            var end = 0;
            foreach (Match match in matches)
            {
                if (!SvgSeparators(source, end, match.Index)) return false;
                var values = SvgNumbers(match.Groups["v"].Value, 6);
                if (values == null) return false;
                var name = match.Groups["n"].Value.ToLowerInvariant();
                double[] next;
                if (name == "matrix" && values.Length == 6) next = values;
                else if (name == "translate" && (values.Length == 1 || values.Length == 2)) next = new[] { 1D, 0D, 0D, 1D, values[0], values.Length == 2 ? values[1] : 0D };
                else if (name == "scale" && (values.Length == 1 || values.Length == 2)) next = new[] { values[0], 0D, 0D, values.Length == 2 ? values[1] : values[0], 0D, 0D };
                else if (name == "rotate" && (values.Length == 1 || values.Length == 3)) next = SvgRotate(values);
                else if ((name == "skewx" || name == "skewy") && values.Length == 1)
                {
                    var tangent = Math.Tan(values[0] * Math.PI / 180D);
                    if (Double.IsNaN(tangent) || Double.IsInfinity(tangent) || Math.Abs(tangent) > MaxSvgMatrixScale) return false;
                    next = name == "skewx" ? new[] { 1D, 0D, tangent, 1D, 0D, 0D } : new[] { 1D, tangent, 0D, 1D, 0D, 0D };
                }
                else return false;
                result = SvgMatrixMultiply(result, next);
                if (!SvgMatrixSafe(result)) return false;
                end = match.Index + match.Length;
            }
            if (!SvgSeparators(source, end, source.Length)) return false;
            matrix = result;
            return true;
        }

        private static double[] SvgRotate(double[] values)
        {
            var angle = values[0] % 360D * Math.PI / 180D;
            var cosine = Math.Cos(angle);
            var sine = Math.Sin(angle);
            var x = values.Length == 3 ? values[1] : 0D;
            var y = values.Length == 3 ? values[2] : 0D;
            return new[] { cosine, sine, -sine, cosine, x - cosine * x + sine * y, y - sine * x - cosine * y };
        }

        private static double[] SvgIdentity()
        {
            return new[] { 1D, 0D, 0D, 1D, 0D, 0D };
        }

        private static double[] SvgMatrixMultiply(double[] left, double[] right)
        {
            return new[]
            {
                left[0] * right[0] + left[2] * right[1],
                left[1] * right[0] + left[3] * right[1],
                left[0] * right[2] + left[2] * right[3],
                left[1] * right[2] + left[3] * right[3],
                left[0] * right[4] + left[2] * right[5] + left[4],
                left[1] * right[4] + left[3] * right[5] + left[5]
            };
        }

        private static bool SvgMatrixSafe(double[] matrix)
        {
            if (matrix == null || matrix.Length != 6) return false;
            for (var index = 0; index < matrix.Length; index++)
                if (Double.IsNaN(matrix[index]) || Double.IsInfinity(matrix[index]) || Math.Abs(matrix[index]) > (index < 4 ? MaxSvgMatrixScale : MaxSvgMatrixTranslate)) return false;
            return true;
        }

        private static bool SvgPath(string input, IList<Vp> result)
        {
            var source = input ?? "";
            if (source.Length == 0 || source.Length > 4096) return false;
            var matches = Regex.Matches(source, "[MmLlHhVvCcSsQqTtAaZz]|[-+]?(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:e[-+]?[0-9]+)?", RegexOptions.IgnoreCase);
            if (matches.Count == 0 || matches.Count > MaxSvgPoints * 8) return false;
            var tokens = new List<string>();
            var end = 0;
            foreach (Match match in matches)
            {
                if (!SvgSeparators(source, end, match.Index)) return false;
                tokens.Add(match.Value);
                end = match.Index + match.Length;
            }
            if (!SvgSeparators(source, end, source.Length)) return false;

            var at = 0;
            var command = '\0';
            var x = 0D;
            var y = 0D;
            var startX = 0D;
            var startY = 0D;
            var controlX = 0D;
            var controlY = 0D;
            var previous = '\0';
            while (at < tokens.Count)
            {
                if (tokens[at].Length == 1 && Char.IsLetter(tokens[at][0]))
                {
                    command = tokens[at++][0];
                    if (command == 'z' || command == 'Z')
                    {
                        if (result.Count == 0) return false;
                        result.Add(new Vp('Z', startX, startY));
                        x = startX;
                        y = startY;
                        previous = 'z';
                        command = '\0';
                        continue;
                    }
                }
                if (command == '\0') return false;
                var lower = Char.ToLowerInvariant(command);
                var relative = Char.IsLower(command);
                if (result.Count == 0 && lower != 'm') return false;
                double a;
                double b;
                double c;
                double d;
                double e;
                double f;
                if (lower == 'm' || lower == 'l')
                {
                    if (!SvgPathNumber(tokens, ref at, out a) || !SvgPathNumber(tokens, ref at, out b)) return false;
                    if (relative) { a += x; b += y; }
                    if (!SvgPathPoint(a, b) || result.Count == MaxSvgPoints) return false;
                    var move = lower == 'm';
                    if (move) { startX = a; startY = b; }
                    result.Add(new Vp(move ? 'M' : 'L', a, b));
                    x = a;
                    y = b;
                    previous = move ? 'm' : 'l';
                    if (move) command = relative ? 'l' : 'L';
                }
                else if (lower == 'h')
                {
                    if (!SvgPathNumber(tokens, ref at, out a)) return false;
                    x = relative ? x + a : a;
                    if (!SvgPathPoint(x, y) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('L', x, y));
                    previous = 'h';
                }
                else if (lower == 'v')
                {
                    if (!SvgPathNumber(tokens, ref at, out a)) return false;
                    y = relative ? y + a : a;
                    if (!SvgPathPoint(x, y) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('L', x, y));
                    previous = 'v';
                }
                else if (lower == 'c')
                {
                    if (!SvgPathNumber(tokens, ref at, out a) || !SvgPathNumber(tokens, ref at, out b) ||
                        !SvgPathNumber(tokens, ref at, out c) || !SvgPathNumber(tokens, ref at, out d) ||
                        !SvgPathNumber(tokens, ref at, out e) || !SvgPathNumber(tokens, ref at, out f)) return false;
                    if (relative) { a += x; b += y; c += x; d += y; e += x; f += y; }
                    if (!SvgPathPoint(a, b) || !SvgPathPoint(c, d) || !SvgPathPoint(e, f) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('C', e, f, a, b, c, d));
                    x = e;
                    y = f;
                    controlX = c;
                    controlY = d;
                    previous = 'c';
                }
                else if (lower == 's')
                {
                    if (!SvgPathNumber(tokens, ref at, out c) || !SvgPathNumber(tokens, ref at, out d) ||
                        !SvgPathNumber(tokens, ref at, out e) || !SvgPathNumber(tokens, ref at, out f)) return false;
                    a = previous == 'c' || previous == 's' ? x * 2D - controlX : x;
                    b = previous == 'c' || previous == 's' ? y * 2D - controlY : y;
                    if (relative) { c += x; d += y; e += x; f += y; }
                    if (!SvgPathPoint(a, b) || !SvgPathPoint(c, d) || !SvgPathPoint(e, f) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('C', e, f, a, b, c, d));
                    x = e;
                    y = f;
                    controlX = c;
                    controlY = d;
                    previous = 's';
                }
                else if (lower == 'q')
                {
                    if (!SvgPathNumber(tokens, ref at, out a) || !SvgPathNumber(tokens, ref at, out b) ||
                        !SvgPathNumber(tokens, ref at, out c) || !SvgPathNumber(tokens, ref at, out d)) return false;
                    if (relative) { a += x; b += y; c += x; d += y; }
                    if (!SvgPathPoint(a, b) || !SvgPathPoint(c, d) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('Q', c, d, a, b, 0D, 0D));
                    x = c;
                    y = d;
                    controlX = a;
                    controlY = b;
                    previous = 'q';
                }
                else if (lower == 't')
                {
                    if (!SvgPathNumber(tokens, ref at, out c) || !SvgPathNumber(tokens, ref at, out d)) return false;
                    a = previous == 'q' || previous == 't' ? x * 2D - controlX : x;
                    b = previous == 'q' || previous == 't' ? y * 2D - controlY : y;
                    if (relative) { c += x; d += y; }
                    if (!SvgPathPoint(a, b) || !SvgPathPoint(c, d) || result.Count == MaxSvgPoints) return false;
                    result.Add(new Vp('Q', c, d, a, b, 0D, 0D));
                    x = c;
                    y = d;
                    controlX = a;
                    controlY = b;
                    previous = 't';
                }
                else if (lower == 'a')
                {
                    bool large;
                    bool sweep;
                    if (!SvgPathNumber(tokens, ref at, out a) || !SvgPathNumber(tokens, ref at, out b) ||
                        !SvgPathNumber(tokens, ref at, out c) || !SvgPathFlag(tokens, ref at, out large) ||
                        !SvgPathFlag(tokens, ref at, out sweep) || !SvgPathNumber(tokens, ref at, out d) ||
                        !SvgPathNumber(tokens, ref at, out e)) return false;
                    if (relative) { d += x; e += y; }
                    if (!SvgArc(result, x, y, a, b, c, large, sweep, d, e)) return false;
                    x = d;
                    y = e;
                    previous = 'a';
                }
                else return false;
            }
            return at == tokens.Count && result.Count > 0;
        }

        private static bool SvgArc(IList<Vp> result, double x1, double y1, double radiusX, double radiusY, double rotation, bool large, bool sweep, double x2, double y2)
        {
            if (!SvgPathPoint(x2, y2) || !SvgPathPoint(radiusX, radiusY) || Math.Abs(rotation) > MaxSvgCoordinate) return false;
            if (Math.Abs(x1 - x2) < Double.Epsilon && Math.Abs(y1 - y2) < Double.Epsilon) return true;
            var rx = Math.Abs(radiusX);
            var ry = Math.Abs(radiusY);
            if (rx < Double.Epsilon || ry < Double.Epsilon)
            {
                if (result.Count == MaxSvgPoints) return false;
                result.Add(new Vp('L', x2, y2));
                return true;
            }

            var phi = rotation % 360D * Math.PI / 180D;
            var cosine = Math.Cos(phi);
            var sine = Math.Sin(phi);
            var halfX = (x1 - x2) / 2D;
            var halfY = (y1 - y2) / 2D;
            var x1p = cosine * halfX + sine * halfY;
            var y1p = -sine * halfX + cosine * halfY;
            var rx2 = rx * rx;
            var ry2 = ry * ry;
            var x1p2 = x1p * x1p;
            var y1p2 = y1p * y1p;
            var radii = x1p2 / rx2 + y1p2 / ry2;
            if (radii > 1D)
            {
                var scale = Math.Sqrt(radii);
                rx *= scale;
                ry *= scale;
                rx2 = rx * rx;
                ry2 = ry * ry;
            }

            var denominator = rx2 * y1p2 + ry2 * x1p2;
            if (denominator <= Double.Epsilon) return false;
            var ratio = Math.Max(0D, (rx2 * ry2 - denominator) / denominator);
            var coefficient = (large == sweep ? -1D : 1D) * Math.Sqrt(ratio);
            var cxp = coefficient * rx * y1p / ry;
            var cyp = coefficient * -ry * x1p / rx;
            var cx = cosine * cxp - sine * cyp + (x1 + x2) / 2D;
            var cy = sine * cxp + cosine * cyp + (y1 + y2) / 2D;
            var theta = Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
            var endTheta = Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx);
            var delta = endTheta - theta;
            if (!sweep && delta > 0D) delta -= Math.PI * 2D;
            else if (sweep && delta < 0D) delta += Math.PI * 2D;
            var segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(delta) / (Math.PI / 2D)));
            if (segments > 4 || result.Count + segments > MaxSvgPoints) return false;
            var step = delta / segments;
            for (var index = 0; index < segments; index++)
            {
                var from = theta + step * index;
                var to = from + step;
                var alpha = 4D / 3D * Math.Tan(step / 4D);
                var fromX = cx + rx * cosine * Math.Cos(from) - ry * sine * Math.Sin(from);
                var fromY = cy + rx * sine * Math.Cos(from) + ry * cosine * Math.Sin(from);
                var toX = cx + rx * cosine * Math.Cos(to) - ry * sine * Math.Sin(to);
                var toY = cy + rx * sine * Math.Cos(to) + ry * cosine * Math.Sin(to);
                var fromDx = -rx * cosine * Math.Sin(from) - ry * sine * Math.Cos(from);
                var fromDy = -rx * sine * Math.Sin(from) + ry * cosine * Math.Cos(from);
                var toDx = -rx * cosine * Math.Sin(to) - ry * sine * Math.Cos(to);
                var toDy = -rx * sine * Math.Sin(to) + ry * cosine * Math.Cos(to);
                var control1X = fromX + alpha * fromDx;
                var control1Y = fromY + alpha * fromDy;
                var control2X = toX - alpha * toDx;
                var control2Y = toY - alpha * toDy;
                if (index == segments - 1) { toX = x2; toY = y2; }
                if (!SvgPathPoint(control1X, control1Y) || !SvgPathPoint(control2X, control2Y) || !SvgPathPoint(toX, toY)) return false;
                result.Add(new Vp('C', toX, toY, control1X, control1Y, control2X, control2Y));
            }
            return true;
        }

        private static bool SvgPathPoint(double x, double y)
        {
            return !Double.IsNaN(x) && !Double.IsInfinity(x) && !Double.IsNaN(y) && !Double.IsInfinity(y) &&
                Math.Abs(x) <= MaxSvgCoordinate && Math.Abs(y) <= MaxSvgCoordinate;
        }

        private static bool SvgPathFlag(IList<string> tokens, ref int at, out bool value)
        {
            value = false;
            double number;
            if (!SvgPathNumber(tokens, ref at, out number) || number != 0D && number != 1D) return false;
            value = number == 1D;
            return true;
        }

        private static bool SvgPathNumber(IList<string> tokens, ref int at, out double value)
        {
            value = 0;
            if (at >= tokens.Count || tokens[at].Length == 1 && Char.IsLetter(tokens[at][0])) return false;
            return SvgFinite(tokens[at++], out value);
        }

        private static bool SvgFinite(string input, out double value)
        {
            return Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        private static bool SvgSeparators(string source, int start, int end)
        {
            for (var index = start; index < end; index++)
                if (!AsciiWhite(source[index]) && source[index] != ',') return false;
            return true;
        }

        private static B ImageBlock(P page, N image, Uri source, S style)
        {
            var map = ImageMap(page, image.Av("usemap"));
            if (map != null)
                foreach (var area in map.As)
                    if (area.U != null) page.Ls.Add(new Ln { U = area.U, T = Clean(area.T) });
            return new B
            {
                K = "img",
                X = image.Av("alt"),
                U = source,
                S = style,
                Mp = map,
                Mw = map == null ? 0 : HtmlDimension(image.Av("width")),
                Mh = map == null ? 0 : HtmlDimension(image.Av("height")),
                El = image
            };
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

        private static void BuildMaps(P page, N node, Uri baseUrl)
        {
            if (node == null || page.Mps.Count == MaxImageMaps) return;
            if (node.K == "map")
            {
                var name = node.Av("name");
                if (MapName(name) && MapByName(page, name) == null)
                {
                    var map = new Mp { N = name, El = node };
                    AddMapAreas(map, node, baseUrl);
                    page.Mps.Add(map);
                }
            }
            foreach (var child in node.C)
            {
                if (page.Mps.Count == MaxImageMaps) break;
                BuildMaps(page, child, baseUrl);
            }
        }

        private static bool MapName(string value)
        {
            if (String.IsNullOrEmpty(value) || value.Length > 128) return false;
            foreach (var c in value)
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f') return false;
            return true;
        }

        private static void AddMapAreas(Mp map, N node, Uri baseUrl)
        {
            foreach (var child in node.C)
            {
                if (map.As.Count == MaxMapAreas) return;
                if (child.K == "area") map.As.Add(MapArea(child, baseUrl));
                else AddMapAreas(map, child, baseUrl);
            }
        }

        private static Ar MapArea(N node, Uri baseUrl)
        {
            var shape = (node.Av("shape") ?? "").Trim().ToLowerInvariant();
            var kind = shape == "circle" || shape == "circ" ? "c" :
                       shape == "poly" || shape == "polygon" ? "p" :
                       shape == "default" ? "d" : "r";
            Uri link = null;
            var href = node.Av("href");
            if (node.A.ContainsKey("href") && href.Length <= 4096) TryUri(baseUrl, href, out link);
            var area = new Ar { K = kind, U = link, T = BoundValue(node.Av("alt"), 512), El = node };
            if (kind != "d")
                foreach (var value in MapCoordinates(node.Av("coords"))) area.Cs.Add(value);

            var minimum = kind == "c" ? 3 : kind == "p" ? 6 : kind == "r" ? 4 : 0;
            if (area.Cs.Count < minimum) area.Cs.Clear();
            else if (kind == "c")
            {
                while (area.Cs.Count > 3) area.Cs.RemoveAt(area.Cs.Count - 1);
                if (area.Cs[2] <= 0) area.Cs.Clear();
            }
            else if (kind == "r")
            {
                while (area.Cs.Count > 4) area.Cs.RemoveAt(area.Cs.Count - 1);
                if (area.Cs[0] > area.Cs[2]) Swap(area.Cs, 0, 2);
                if (area.Cs[1] > area.Cs[3]) Swap(area.Cs, 1, 3);
            }
            else if (kind == "p" && area.Cs.Count % 2 != 0) area.Cs.RemoveAt(area.Cs.Count - 1);
            return area;
        }

        private static IList<double> MapCoordinates(string value)
        {
            var result = new List<double>();
            value = value ?? "";
            if (value.Length > 4096) return result;
            var parts = value.Split(new char[] { ',', ' ', '\t', '\r', '\n', '\f' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (result.Count == MaxMapCoordinates) break;
                double number;
                if (!Double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out number) || Double.IsNaN(number) || Double.IsInfinity(number))
                {
                    result.Clear();
                    break;
                }
                result.Add(Math.Max(-MaxMapCoordinate, Math.Min(MaxMapCoordinate, number)));
            }
            return result;
        }

        private static void Swap(IList<double> values, int left, int right)
        {
            var value = values[left];
            values[left] = values[right];
            values[right] = value;
        }

        private static Mp ImageMap(P page, string reference)
        {
            var value = (reference ?? "").Trim();
            if (value.Length < 2 || value.Length > 4097 || value[0] != '#') return null;
            value = value.Substring(1);
            try { value = Uri.UnescapeDataString(value); }
            catch (UriFormatException) { return null; }
            return value.Length > 128 ? null : MapByName(page, value);
        }

        private static Mp MapByName(P page, string name)
        {
            foreach (var map in page.Mps)
                if (map.N.Equals(name, StringComparison.OrdinalIgnoreCase)) return map;
            return null;
        }

        private static B GaugeBlock(N node, S style)
        {
            return new B { K = node.K, X = "", S = style, Vm = Gauge(node), El = node };
        }

        private static Vm Gauge(N node)
        {
            var model = new Vm();
            double value;
            double maximum;
            if (node.K == "progress")
            {
                model.Mn = 0;
                if (!GaugeNumber(node.Av("max"), out maximum) || maximum <= 0) maximum = 1;
                model.Mx = maximum;
                model.In = !node.A.ContainsKey("value");
                if (!GaugeNumber(node.Av("value"), out value)) value = 0;
                model.V = GaugeClamp(value, model.Mn, model.Mx);
                model.R = GaugeRatio(model.V, model.Mn, model.Mx);
                return model;
            }

            if (!GaugeNumber(node.Av("min"), out value)) value = 0;
            model.Mn = value;
            if (!GaugeNumber(node.Av("max"), out maximum)) maximum = 1;
            model.Mx = Math.Max(model.Mn, maximum);
            if (!GaugeNumber(node.Av("value"), out value)) value = 0;
            model.V = GaugeClamp(value, model.Mn, model.Mx);
            if (!GaugeNumber(node.Av("low"), out value)) value = model.Mn;
            model.Lo = GaugeClamp(value, model.Mn, model.Mx);
            if (!GaugeNumber(node.Av("high"), out value)) value = model.Mx;
            model.Hi = GaugeClamp(Math.Max(model.Lo, value), model.Mn, model.Mx);
            if (!GaugeNumber(node.Av("optimum"), out value)) value = model.Mn + (model.Mx - model.Mn) / 2D;
            model.Op = GaugeClamp(value, model.Mn, model.Mx);
            model.R = GaugeRatio(model.V, model.Mn, model.Mx);
            model.Q = GaugeQuality(model);
            return model;
        }

        private static bool GaugeNumber(string input, out double value)
        {
            value = 0;
            var text = (input ?? "").Trim();
            if (text.Length == 0 || text.Length > MaxGaugeText ||
                !Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                Double.IsNaN(value) || Double.IsInfinity(value)) return false;
            value = Math.Max(-MaxGaugeValue, Math.Min(MaxGaugeValue, value));
            return true;
        }

        private static double GaugeClamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        private static double GaugeRatio(double value, double minimum, double maximum)
        {
            if (maximum <= minimum) return 0;
            return Math.Max(0, Math.Min(1, (value - minimum) / (maximum - minimum)));
        }

        private static int GaugeQuality(Vm model)
        {
            if (model.Op < model.Lo)
            {
                if (model.V < model.Lo) return 0;
                return model.V <= model.Hi ? 1 : 2;
            }
            if (model.Op > model.Hi)
            {
                if (model.V > model.Hi) return 0;
                return model.V >= model.Lo ? 1 : 2;
            }
            return model.V >= model.Lo && model.V <= model.Hi ? 0 : 1;
        }

        private static bool GaugeTag(string kind)
        {
            return kind == "progress" || kind == "meter";
        }

        private static bool ValueTag(string kind)
        {
            return kind == "output" || kind == "data" || kind == "time" || kind == "ins" || kind == "del";
        }

        private static void AddValueBlock(P page, N node, Uri baseUrl, List<R> rules, S style)
        {
            var block = ValueBlock(page, node, baseUrl, rules, style);
            if (block.X.Length > 0 || block.Is.Count > 0 || LayoutContext(style)) page.Bs.Add(block);
        }

        private static B ValueBlock(P page, N node, Uri baseUrl, List<R> rules, S style)
        {
            var text = TextValue(RawText(node), style);
            var model = ValueModel(page, node, text, baseUrl);
            var block = new B { K = node.K, X = text, S = style, Dv = model, El = node };
            if (HasInlineStructure(node))
            {
                foreach (var child in node.C) AddInline(page, block, child, baseUrl, rules, style.Inherit(), null, node.K, model);
                NormalizeInlines(block);
                block.X = InlineText(block);
            }
            return block;
        }

        private static Dv ValueModel(P page, N node, string text, Uri baseUrl)
        {
            var raw = RawText(node);
            var datetime = BoundValue(node.Av("datetime").Trim(), 128);
            Uri citation;
            if ((node.K != "ins" && node.K != "del") || !CitationUri(baseUrl, node.Av("cite"), out citation)) citation = null;
            var model = new Dv
            {
                K = ValueKind(node.K),
                X = BoundValue(text, 4096),
                V = BoundValue(node.K == "data" ? node.Av("value") : node.K == "time" && node.A.ContainsKey("datetime") ? datetime : raw, 4096),
                N = BoundValue(node.Av("name").Trim(), 256),
                Fo = BoundValue(node.Av("form").Trim(), 128),
                Dt = node.K == "time" && datetime.Length == 0 && !node.A.ContainsKey("datetime") ? BoundValue(raw, 128) : datetime,
                U = citation,
                El = node
            };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in (node.Av("for") ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
            {
                var reference = BoundValue(value, 128);
                if (reference.Length == 0 || !seen.Add(reference)) continue;
                model.F.Add(reference);
                if (model.F.Count == 32) break;
            }
            page.Dvs.Add(model);
            return model;
        }

        private static string ValueKind(string kind)
        {
            if (kind == "output") return "o";
            if (kind == "data") return "d";
            if (kind == "time") return "t";
            if (kind == "ins") return "i";
            return "r";
        }

        private static bool CitationUri(Uri baseUrl, string value, out Uri url)
        {
            url = null;
            value = (value ?? "").Trim();
            if (value.Length == 0 || value.Length > 4096 || !TryUri(baseUrl, value, out url)) return false;
            return url.IsFile || url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps;
        }

        private static string BoundValue(string value, int max)
        {
            value = value ?? "";
            return value.Length <= max ? value : value.Substring(0, max);
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

        private static void MarkDisclosures(P page)
        {
            foreach (var block in page.Bs) MarkDisclosure(block);
        }

        private static void MarkDisclosure(B block)
        {
            if (block == null) return;
            block.Hd = DisclosureHidden(block.El);
            if (block.K == "summary")
            {
                var details = Disclosure(block.El);
                if (details != null) block.Mk = details.Op ? "v" : ">";
            }
            if (block.Tb == null) return;
            foreach (var row in block.Tb.Rs)
                foreach (var cell in row.Cs)
                    foreach (var part in cell.Bs) MarkDisclosure(part);
        }

        private static bool DisclosureHidden(N node)
        {
            for (var current = node; current != null && current.Up != null; current = current.Up)
            {
                var parent = current.Up;
                if (parent.K != "details" || parent.Op) continue;
                if (!Object.ReferenceEquals(FirstSummary(parent), current)) return true;
            }
            return false;
        }

        private static N Disclosure(N element)
        {
            if (element == null) return null;
            if (element.K == "details") return element;
            for (var current = element; current != null; current = current.Up)
            {
                if (current.K == "details") return null;
                if (current.K != "summary") continue;
                var details = current.Up;
                return details != null && details.K == "details" && Object.ReferenceEquals(FirstSummary(details), current) ? details : null;
            }
            return null;
        }

        private static N FirstSummary(N details)
        {
            if (details == null) return null;
            foreach (var child in details.C) if (child.K == "summary") return child;
            return null;
        }

        private static string ListMarker(N item)
        {
            if (item == null || item.K != "li") return "";
            return String.IsNullOrEmpty(item.Mk) ? "-" : item.Mk;
        }

        private static void MarkLists(N node)
        {
            if (node == null) return;
            if (node.K == "ol" || node.K == "ul" || node.K == "menu")
            {
                var ordered = node.K == "ol";
                int number;
                if (!ordered || !ListInteger(node.Av("start"), out number)) number = 1;
                foreach (var child in node.C)
                {
                    if (child.K != "li") continue;
                    int value;
                    if (ordered && ListInteger(child.Av("value"), out value)) number = value;
                    child.Mk = ordered ? number.ToString(CultureInfo.InvariantCulture) + "." : "-";
                    if (ordered && number < 999999) number++;
                }
            }
            foreach (var child in node.C) MarkLists(child);
        }

        private static bool ListInteger(string value, out int number)
        {
            number = 0;
            var text = (value ?? "").Trim();
            return text.Length > 0 && text.Length <= 7 &&
                   Int32.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number) &&
                   number >= -999999 && number <= 999999;
        }

        private static string BlockText(N node, bool raw)
        {
            var text = new StringBuilder();
            AddBlockText(node, text, true, raw);
            return text.ToString();
        }

        private static string DirectText(N node)
        {
            var text = new StringBuilder();
            foreach (var child in node.C) if (child.K == "#text") text.Append(child.X).Append(' ');
            return Clean(text.ToString());
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
                if (Flex(inherited) || Grid(inherited) || IsBlockTag(child.K) || child.K == "img" || child.K == "input" || child.K == "textarea" || child.K == "button" || child.K == "select")
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

        private static void AddInline(P page, B block, N node, Uri baseUrl, List<R> rules, S inherited, Uri activeLink, string inheritedKind, Dv inheritedValue = null)
        {
            if (node.K == "#text")
            {
                if (node.X.Length > 0) block.Is.Add(new Ir { K = inheritedKind, X = node.X, U = activeLink, S = inherited, Dv = inheritedValue, El = node.Up });
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
            var value = inheritedValue;
            if (ValueTag(node.K)) value = ValueModel(page, node, TextValue(RawText(node), style), baseUrl);
            if (node.K == "a")
            {
                Uri target;
                var href = node.Av("href");
                if (href.Length > 0 && TryUri(baseUrl, href, out target))
                {
                    link = target;
                    page.Ls.Add(new Ln { U = link, T = Clean(NodeText(node)) });
                }
            }

            var first = block.Is.Count;
            if (node.K != "img") foreach (var child in node.C) AddInline(page, block, child, baseUrl, rules, style, link, kind, value);
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
                    if (previous != null && previous.K != "br" && previous.K == run.K && previous.U == run.U && Object.ReferenceEquals(previous.S, run.S) && Object.ReferenceEquals(previous.Dv, run.Dv) && Object.ReferenceEquals(previous.El, run.El)) previous.X += run.X;
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
                if (previous != null && previous.K != "br" && previous.K == run.K && previous.U == run.U && Object.ReferenceEquals(previous.S, run.S) && Object.ReferenceEquals(previous.Dv, run.Dv) && Object.ReferenceEquals(previous.El, run.El)) previous.X += run.X;
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
                    if (text.Length > 0) cell.Bs.Add(new B { K = fallback, X = text, Mk = ListMarker(child.Up), S = inherited, El = child.Up });
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
                    if (href.Length > 0 && TryUri(baseUrl, href, out link))
                    {
                        if (text.Length == 0) text = link.ToString();
                        page.Ls.Add(new Ln { U = link, T = Clean(text) });
                        cell.Bs.Add(new B { K = "a", X = text, U = link, S = style, El = child });
                    }
                    continue;
                }
                if (GaugeTag(child.K))
                {
                    cell.Bs.Add(GaugeBlock(child, style));
                    continue;
                }
                if (ValueTag(child.K))
                {
                    var block = ValueBlock(page, child, baseUrl, rules, style);
                    if (block.X.Length > 0 || block.Is.Count > 0 || LayoutContext(style)) cell.Bs.Add(block);
                    continue;
                }

                var nested = HasCellStructure(child);
                if (!nested)
                {
                    var text = TextValue(RawText(child), style);
                    if (text.Length > 0) cell.Bs.Add(new B { K = CellKind(child.K, fallback), X = text, Mk = ListMarker(child), S = style, El = child });
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
            if (kind == "u" || kind == "ins") AddDecoration(style, 'u');
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
            if (actionValue.Length > 0 && TryUri(baseUrl, actionValue, out formAction)) action = formAction;
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
            if (page.Fr.TryGetValue(node, out current))
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
            else page.Fr[node] = field;
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

        private bool GetCache(Uri url, out Cr reply, out Cr stale, out bool fromDisk)
        {
            var key = CacheKey(url);
            reply = null;
            stale = null;
            fromDisk = false;
            if (cm > 0 && cache.TryGetValue(key, out reply))
            {
                if (reply.Fr <= DateTime.UtcNow)
                {
                    stale = reply;
                    reply = null;
                    ValidateStale(url, ref stale);
                    return false;
                }
                lru.Remove(key);
                lru.Add(key);
                ch++;
                return true;
            }
            if (!GetDisk(url, out reply, false)) return false;
            if (reply.Fr <= DateTime.UtcNow)
            {
                stale = reply;
                reply = null;
                fromDisk = true;
                ValidateStale(url, ref stale);
                return false;
            }
            Remember(key, reply);
            fromDisk = true;
            ch++;
            return true;
        }

        private void ValidateStale(Uri url, ref Cr stale)
        {
            if (stale == null || stale.Et.Length == 0 && stale.Lm.Length == 0)
            {
                DropCache(url);
                stale = null;
            }
        }

        private void PutCache(Uri url, Cr reply)
        {
            var key = CacheKey(url);
            var reusable = reply.Fr > DateTime.UtcNow || reply.Et.Length > 0 || reply.Lm.Length > 0;
            if (reply.Ns || !reusable || reply.St < 200 || reply.St >= 300 || reply.Sz > MaxCachePage)
            {
                DropCache(url);
                return;
            }
            Remember(key, reply);
            if (DiskOn) disk.Pt(key, new Pr(reply.U, reply.H, reply.St, reply.Ct, reply.Ce, reply.Ch, reply.Sz, reply.Fr, reply.Et, reply.Lm));
        }

        private bool DiskOn { get { return !Pv && disk.On; } }

        private bool GetDisk(Uri url, out Cr reply, bool remember)
        {
            reply = null;
            if (!DiskOn) return false;
            Pr saved;
            var state = disk.Gt(CacheKey(url), out saved);
            if (state == 2) Trace("CACHE", url, 0, "", 0, "recover");
            if (state != 1 || saved == null) return false;
            reply = new Cr(saved.U, saved.H, saved.St, saved.Ct, saved.Ce, saved.Ch, saved.Sz, false, saved.Fr, saved.Et, saved.Lm);
            if (remember) Remember(CacheKey(url), reply);
            return true;
        }

        private void Remember(string key, Cr reply)
        {
            if (cm == 0)
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
            if (DiskOn) disk.Rm(key);
        }

        private static string CacheKey(Uri url)
        {
            if (url.Fragment.Length == 0) return url.AbsoluteUri;
            var clean = new UriBuilder(url);
            clean.Fragment = "";
            return clean.Uri.AbsoluteUri;
        }

        private static bool SameDocument(Uri left, Uri right)
        {
            return left != null && right != null && String.Equals(CacheKey(left), CacheKey(right), StringComparison.OrdinalIgnoreCase);
        }

        private static bool SameAddress(Uri left, Uri right)
        {
            return left != null && right != null && String.Equals(left.AbsoluteUri, right.AbsoluteUri, StringComparison.OrdinalIgnoreCase);
        }

        private static bool FragmentReference(string input)
        {
            return (input ?? "").Trim().Trim('"').StartsWith("#", StringComparison.Ordinal);
        }

        private static bool FragmentNavigation(Uri current, Uri target)
        {
            if (current == null || target == null) return false;
            return current.Fragment.Length > 0 || target.Fragment.Length > 0 || target.OriginalString.EndsWith("#", StringComparison.Ordinal);
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

        private struct Pd
        {
            public readonly string N;
            public readonly string V;
            public readonly Uri U;
            public readonly bool C;
            public readonly bool I;

            public Pd(string name, string value, Uri origin, bool custom, bool important)
            {
                N = name;
                V = value;
                U = origin;
                C = custom;
                I = important;
            }
        }

        private sealed class Sx
        {
            public readonly Dictionary<string, N> Ns = new Dictionary<string, N>(StringComparer.Ordinal);
            public readonly List<N> Os = new List<N>();
        }

        private sealed class Sv
        {
            public string F = "#000000";
            public string S = "none";
            public string C = "#000000";
            public string A = "s";
            public string R = "n";
            public string Cr = "n";
            public string Lc = "b";
            public string Lj = "m";
            public double W = 1D;
            public double O = 1D;
            public double Fo = 1D;
            public double So = 1D;
            public double Z = 16D;
            public double Ml = 4D;
            public double Do;
            public double[] Da = new double[0];
            public string X = "";
            public double[] M = SvgIdentity();
            public double[] Xm = SvgIdentity();

            public Sv Copy()
            {
                return new Sv { F = F, S = S, C = C, A = A, R = R, Cr = Cr, Lc = Lc, Lj = Lj, W = W, O = O, Fo = Fo, So = So, Z = Z, Ml = Ml, Do = Do, Da = (double[])Da.Clone(), X = X, M = (double[])M.Clone(), Xm = (double[])Xm.Clone() };
            }
        }

        private sealed class Mc
        {
            public readonly int N;
            public readonly int X;

            public Mc(int minimum, int maximum)
            {
                N = minimum;
                X = maximum;
            }

            public bool On(int width)
            {
                return width >= N && (X == 0 || width <= X);
            }

            public string D
            {
                get
                {
                    if (N == 0 && X == 0) return "";
                    if (N == X) return "@m=" + N;
                    if (N == 0) return "@m<=" + X;
                    if (X == 0) return "@m>=" + N;
                    return "@m" + N + "-" + X;
                }
            }
        }

        private sealed class Sc
        {
            public readonly string X;
            public readonly Uri U;

            public Sc(string source, Uri origin)
            {
                X = source ?? "";
                U = origin;
            }
        }

        private sealed class R
        {
            public readonly string Q;
            public readonly string V;
            public readonly int W;
            public readonly int O;
            public readonly Se[] P;
            public readonly char[] J;
            public readonly Mc[] M;
            public readonly string Md;
            public readonly Pd[] Ds;

            public R(string query, string values, int weight, int order, Se[] parts, char[] joins)
                : this(query, values, null, weight, order, parts, joins, new Mc[] { new Mc(0, 0) })
            {
            }

            public R(string query, string values, Uri origin, int weight, int order, Se[] parts, char[] joins, Mc[] media)
            {
                Q = query;
                V = values;
                W = weight;
                O = order;
                P = parts;
                J = joins;
                M = media ?? new Mc[0];
                Ds = CssDeclarations(values, origin);
                var labels = new List<string>();
                foreach (var condition in M)
                    if (condition.D.Length > 0) labels.Add(condition.D);
                Md = String.Join("|", labels.ToArray());
            }

            public bool On(int width)
            {
                foreach (var condition in M)
                    if (condition.On(width)) return true;
                return false;
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
            public readonly int A;
            public readonly int B;
            public readonly Se[] Ss;
            public readonly Hr[] Rs;

            public Ps(string name)
                : this(name, 0, 0, NoSelectors, NoRelatives)
            {
            }

            public Ps(string name, Se[] selectors)
                : this(name, 0, 0, selectors, NoRelatives)
            {
            }

            public Ps(string name, Hr[] relatives)
                : this(name, 0, 0, NoSelectors, relatives)
            {
            }

            public Ps(string name, int a, int b, Se[] selectors)
                : this(name, a, b, selectors, NoRelatives)
            {
            }

            private Ps(string name, int a, int b, Se[] selectors, Hr[] relatives)
            {
                N = name;
                A = a;
                B = b;
                Ss = selectors ?? NoSelectors;
                Rs = relatives ?? NoRelatives;
            }
        }

        private sealed class Hr
        {
            public readonly char L;
            public readonly Se[] P;
            public readonly char[] J;
            public readonly int W;

            public Hr(char leading, Se[] parts, char[] joins, int weight)
            {
                L = leading;
                P = parts;
                J = joins;
                W = weight;
            }
        }

        private sealed class He
        {
            public readonly P P;
            public readonly Uri U;

            public He(P page, Uri url)
            {
                P = page;
                U = url;
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
            private int tm = DefaultTimeout;

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
            public int Tm { get { return tm; } set { tm = value; } }
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
                    http.Timeout = tm;
                    http.ReadWriteTimeout = tm;
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
            if (baseUrl != null && input.StartsWith("#", StringComparison.Ordinal)) return new Uri(CacheKey(baseUrl) + input);
            if (Uri.TryCreate(input, UriKind.Absolute, out u)) return u;
            if (baseUrl != null && Uri.TryCreate(baseUrl, input, out u)) return u;
            if (File.Exists(input)) return new Uri(Path.GetFullPath(input));
            try { return new Uri("https://" + input); }
            catch (UriFormatException error) { throw new Er("url", null, 0, false, "Lea could not understand that address.", error); }
        }

        private static bool TryUri(Uri baseUrl, string value, out Uri url)
        {
            url = null;
            if (baseUrl == null) return false;
            try
            {
                if ((value ?? "").StartsWith("#", StringComparison.Ordinal)) url = new Uri(CacheKey(baseUrl) + value);
                else return Uri.TryCreate(baseUrl, value, out url);
                return true;
            }
            catch (UriFormatException) { return false; }
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
