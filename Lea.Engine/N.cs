using System;
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
            Mk = "";
            A = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            C = new List<N>();
        }

        public string K { get; internal set; }
        public string X { get; internal set; }
        public IDictionary<string, string> A { get; private set; }
        public IList<N> C { get; private set; }
        public N Up { get; internal set; }
        public bool Op { get; internal set; }
        internal bool Hv { get; set; }
        internal bool Fc { get; set; }
        internal string Mk { get; set; }
        internal List<Cn> Ct { get; set; }
        internal List<Cn> Cx { get; set; }
        internal S Gb { get; set; }
        internal S Ga { get; set; }
        internal S Gm { get; set; }
        internal Uri Bu { get; set; }
        internal int Cw { get; set; }
        internal int Ch { get; set; }
        internal bool Cv { get; set; }

        public string Av(string name)
        {
            string value;
            return A.TryGetValue(name, out value) ? value : "";
        }

        // Mt/Cl read this node's live tree, including a retained detached subtree.
        public bool Mt(string query) { return E.MatchNode(this, query, false) != null; }
        public N Cl(string query) { return E.MatchNode(this, query, true); }

        // Q/Qa search descendants, never this node; Qa is a bounded static snapshot.
        public N Q(string query) { return E.QueryNode(this, query, null); }
        public IList<N> Qa(string query)
        {
            var found = new List<N>();
            E.QueryNode(this, query, found);
            return found.AsReadOnly();
        }
    }

    internal sealed class Cn
    {
        internal readonly string N;
        internal readonly N O;
        internal int V;

        internal Cn(string name, N owner, int value)
        {
            N = name;
            O = owner;
            V = value;
        }

        internal Cn Copy()
        {
            return new Cn(N, O, V);
        }
    }
}
