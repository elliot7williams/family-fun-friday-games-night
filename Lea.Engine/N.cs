using System.Collections.Generic;

namespace Lea.Engine
{
    // N is a lightweight Lea document node.
    public sealed class N
    {
        internal N(string kind)
        {
            K = kind;
            X = "";
            A = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            C = new List<N>();
        }

        public string K { get; internal set; }
        public string X { get; internal set; }
        public IDictionary<string, string> A { get; private set; }
        public IList<N> C { get; private set; }
        public N Up { get; internal set; }

        public string Av(string name)
        {
            string value;
            return A.TryGetValue(name, out value) ? value : "";
        }
    }
}
