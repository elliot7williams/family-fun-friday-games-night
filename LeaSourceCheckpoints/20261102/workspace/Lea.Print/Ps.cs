using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lea.Engine;

namespace Lea.Print
{
    // Inert input for the future print renderer, not print-ready HTML or a fetch grant.
    public sealed class Ps
    {
        public const int MaxNodes = 65536;
        public const int MaxCharacters = 4 * 1024 * 1024;
        public const int MaxDepth = 256;
        public string Title { get; private set; }
        public string Source { get; private set; }
        public int Omitted { get; private set; }
        public bool Styled { get; private set; }
        public ReadOnlyCollection<Pn> Nodes { get; private set; }

        private Ps() { }

        public static Ps Cp(P page)
        { return Capture(page, null); }

        public static Ps Cf(P page)
        { return Capture(page, null, true); }

        public static Ps Cf(P page, IReadOnlyDictionary<N, S> styles)
        {
            if (styles == null) throw new ArgumentNullException("styles");
            return Capture(page, styles, true);
        }

        public static Ps Cs(P page, IReadOnlyDictionary<N, S> styles)
        {
            if (styles == null) throw new ArgumentNullException("styles");
            return Capture(page, styles);
        }
        private static Ps Capture(P page, IReadOnlyDictionary<N, S> styles, bool friendly = false)
        {
            if (page == null || page.D == null) throw new ArgumentException("Document required", "page");
            var nodes = new List<Pn>();
            var pending = new Stack<Pending>();
            var seen = new HashSet<N>();
            int scheduled = 1, characters = 0, omitted = 0;
            string title = Take(page.T, ref characters);
            string source = Take(SafeAddress(page.U), ref characters);
            Uri basis = page.Bu ?? page.U;
            pending.Push(new Pending(page.D, -1, 0, false));
            while (pending.Count != 0)
            {
                Pending item = pending.Pop();
                N node = item.Node;
                if (node == null || !seen.Add(node)) throw new InvalidOperationException("Malformed print tree");
                if (item.Depth > MaxDepth) throw new InvalidOperationException("Print depth limit");
                string kind = Take(node.K, ref characters);
                S computed = null;
                if (styles != null && (!styles.TryGetValue(node, out computed) || computed == null)) throw new InvalidOperationException("Complete print styles required");
                bool exclude = item.Exclude || Excluded(kind) || friendly && FriendlyExcluded(node, kind, ref characters) || node.A.ContainsKey("hidden") ||
                    computed != null && (computed.D == "none" || computed.Op <= 0);
                bool hidden = computed != null && (computed.Vi == "h" || computed.Vi == "c");
                bool omit = exclude || hidden && (kind == "#text" || kind == "img" || node.C.Count == 0);
                int parent = item.Parent;
                if (omit) omitted++;
                else
                {
                    var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string key in new[] { "alt", "title", "lang", "dir", "colspan", "rowspan", "width", "height" })
                    {
                        string value;
                        if (node.A.TryGetValue(key, out value)) attributes.Add(key, Take(value, ref characters));
                    }
                    string link = kind == "a" ? SafeAddress(Resolve(basis, Take(node.Av("href"), ref characters))) : "";
                    string image = "", imagePolicy = "", cacheKey = "";
                    if (kind == "img")
                    {
                        Uri uri = Resolve(basis, Take(node.Av("src"), ref characters));
                        image = SafeAddress(uri);
                        imagePolicy = ResourcePolicy(page.U, uri);
                        if (friendly && uri != null && uri.Scheme == "data") cacheKey = Take(uri.AbsoluteUri, ref characters);
                    }
                    parent = nodes.Count;
                    nodes.Add(new Pn(item.Parent, kind,
                        kind == "#text" ? Take(node.X, ref characters) : "",
                        attributes, Take(link, ref characters), Take(image, ref characters), imagePolicy,
                        computed == null ? null : Py.Cp(computed, ref characters), cacheKey));
                }
                if (node.C.Count > MaxNodes - scheduled) throw new InvalidOperationException("Print node limit");
                scheduled += node.C.Count;
                for (int i = node.C.Count - 1; i >= 0; i--)
                    pending.Push(new Pending(node.C[i], parent, item.Depth + 1, exclude));
            }
            if (styles != null && styles.Count != seen.Count) throw new InvalidOperationException("Print styles belong to a different tree");
            return new Ps { Title = title, Source = source, Omitted = omitted, Nodes = nodes.AsReadOnly(), Styled = styles != null };
        }

        private static bool FriendlyExcluded(N node, string kind, ref int characters)
        {
            switch (kind)
            {
                case "nav": case "aside": case "footer": case "form": case "menu":
                case "dialog": case "video": case "audio": case "canvas": return true;
            }
            string role = Take(node.Av("role"), ref characters).Trim();
            if (String.Equals(role, "navigation", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(role, "banner", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(role, "complementary", StringComparison.OrdinalIgnoreCase)) return true;
            string classes = Take(node.Av("class"), ref characters);
            foreach (string token in classes.Split(new[] { ' ', '\t', '\r', '\n', '\f' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (token.ToLowerInvariant())
                {
                    case "ad": case "ads": case "advert": case "advertisement":
                    case "cookie": case "cookies": case "popup": case "modal":
                    case "share": case "social": return true;
                }
            }
            return false;
        }

        private static bool Excluded(string kind)
        {
            switch (kind)
            {
                case "head": case "script": case "style": case "template":
                case "iframe": case "object": case "embed": case "input":
                case "textarea": case "select": case "button": return true;
                default: return false;
            }
        }

        internal static string Take(string text, ref int count)
        {
            text = text ?? "";
            if (text.Length > MaxCharacters - count) throw new InvalidOperationException("Print character limit");
            count += text.Length;
            return text;
        }

        private static Uri Resolve(Uri basis, string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            Uri uri;
            if (Uri.TryCreate(value, UriKind.Absolute, out uri)) return uri;
            return basis != null && basis.IsAbsoluteUri && Uri.TryCreate(basis, value, out uri) ? uri : null;
        }

        private static string SafeAddress(Uri uri)
        {
            if (uri == null || !uri.IsAbsoluteUri || uri.UserInfo.Length != 0) return "";
            if (uri.Scheme != "http" && uri.Scheme != "https" && uri.Scheme != "file") return "";
            return uri.AbsoluteUri;
        }

        private static string ResourcePolicy(Uri origin, Uri resource)
        {
            if (SafeAddress(resource).Length == 0) return "denied";
            if (resource.IsFile) return "requires-file-policy";
            if (origin == null || !origin.IsAbsoluteUri || origin.UserInfo.Length != 0 ||
                (origin.Scheme != "http" && origin.Scheme != "https")) return "denied";
            return origin.Scheme == resource.Scheme &&
                String.Equals(origin.IdnHost, resource.IdnHost, StringComparison.OrdinalIgnoreCase) &&
                origin.Port == resource.Port ? "same-origin-unfetched" : "denied";
        }

        private sealed class Pending
        {
            internal readonly N Node;
            internal readonly int Parent, Depth;
            internal readonly bool Exclude;
            internal Pending(N node, int parent, int depth, bool exclude)
            { Node = node; Parent = parent; Depth = depth; Exclude = exclude; }
        }
    }

    public sealed class Pn
    {
        public int Parent { get; private set; }
        public string Kind { get; private set; }
        public string Text { get; private set; }
        public ReadOnlyDictionary<string, string> Attributes { get; private set; }
        public string Link { get; private set; }
        public string Image { get; private set; }
        public string ImagePolicy { get; private set; }
        internal string CacheKey { get; private set; }
        public Py Style { get; private set; }
        internal Pn(int parent, string kind, string text, Dictionary<string, string> attributes,
            string link, string image, string policy, Py style, string cacheKey = "")
        {
            Parent = parent; Kind = kind; Text = text;
            Attributes = new ReadOnlyDictionary<string, string>(attributes);
            Link = link; Image = image; ImagePolicy = policy;
            Style = style;
            CacheKey = cacheKey;
        }
    }
}
