using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // G is a bounded developer-diagnostics snapshot.
    public sealed class G
    {
        internal G()
        {
            Ns = new List<string>();
            Cs = new List<string>();
            Rs = new List<string>();
            Ms = new List<string>();
            Vs = new List<string>();
        }

        public Uri U { get; internal set; }
        public string T { get; internal set; }
        public int St { get; internal set; }
        public string Ct { get; internal set; }
        public int Sz { get; internal set; }
        public bool Ca { get; internal set; }
        public int N { get; internal set; }
        public int B { get; internal set; }
        public int L { get; internal set; }
        public int F { get; internal set; }
        public int C { get; internal set; }
        public int J { get; internal set; }
        public int V { get; internal set; }
        public int M { get; internal set; }
        public int Cc { get; internal set; }
        public int Ch { get; internal set; }
        public int Kc { get; internal set; }
        public IList<string> Ns { get; private set; }
        public IList<string> Cs { get; private set; }
        public IList<string> Rs { get; private set; }
        public IList<string> Ms { get; private set; }
        public IList<string> Vs { get; private set; }
    }
}
