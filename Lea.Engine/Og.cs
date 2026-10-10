using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Lea.Engine
{
    public sealed class Og
    {
        private readonly string scheme;
        private readonly string host;
        private readonly int port;

        private Og(string scheme, string host, int port, bool local)
        {
            this.scheme = scheme;
            this.host = host;
            this.port = port;
            Lc = local;
            Tx = host == null ? "null" : scheme + "://" + host +
                (port == DefaultPort(scheme) ? "" : ":" + port.ToString(CultureInfo.InvariantCulture));
        }

        public bool Op { get { return host == null; } }
        public string Tx { get; private set; }
        // Local-script eligibility is host policy, not same-origin authority.
        public bool Lc { get; private set; }

        public static Og Of(Uri url)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (!url.IsAbsoluteUri) throw new ArgumentException("An absolute URL is required.", "url");
            var scheme = url.Scheme.ToLowerInvariant();
            var local = url.IsFile && !url.IsUnc || scheme == "about" &&
                url.AbsolutePath == "blank" && url.Host.Length == 0 && url.UserInfo.Length == 0;
            if (DefaultPort(scheme) < 0) return new Og(null, null, -1, local);
            string host;
            try { host = url.IdnHost.ToLowerInvariant(); }
            catch (UriFormatException) { return new Og(null, null, -1, false); }
            // Zone-qualified IPv6 URLs are outside Lea's supported authority syntax.
            if (host.Length == 0 || host.IndexOf('%') >= 0 || url.Port < 0)
                return new Og(null, null, -1, false);
            if (url.HostNameType == UriHostNameType.IPv6) host = "[" + host + "]";
            return new Og(scheme, host, url.Port, false);
        }

        public bool Eq(Og other)
        {
            return ReferenceEquals(this, other) || other != null && !Op && !other.Op &&
                scheme == other.scheme && host == other.host && port == other.port;
        }

        public override bool Equals(object other) { return Eq(other as Og); }
        public override int GetHashCode()
        {
            if (Op) return RuntimeHelpers.GetHashCode(this);
            unchecked { return (scheme.GetHashCode() * 397 ^ host.GetHashCode()) * 397 ^ port; }
        }
        public override string ToString() { return Tx; }

        private static int DefaultPort(string scheme)
        {
            switch (scheme)
            {
                case "ftp": return 21;
                case "http": case "ws": return 80;
                case "https": case "wss": return 443;
                default: return -1;
            }
        }
    }
}
