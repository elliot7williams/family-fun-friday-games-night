using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    public sealed class P
    {
        internal P()
        {
            Ls = new List<Ln>();
            Axs = new List<Ax>();
            Bs = new List<B>();
            Fs = new List<Fi>();
            Dvs = new List<Dv>();
            Mps = new List<Mp>();
            Ms = new List<string>();
            Vs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Fr = new Dictionary<N, Fi>();
        }

        public Uri U { get; internal set; }
        public Uri Bu { get; internal set; }
        public string T { get; internal set; }
        public string H { get; internal set; }
        public string X { get; internal set; }
        public string Q { get; internal set; }
        public int St { get; internal set; }
        public string Ct { get; internal set; }
        public string Ce { get; internal set; }
        public string Ch { get; internal set; }
        public string Et { get; internal set; }
        public string Lm { get; internal set; }
        public int Sz { get; internal set; }
        public bool Ca { get; internal set; }
        public IList<Ln> Ls { get; private set; }
        public IList<Ax> Axs { get; private set; }
        public IList<B> Bs { get; private set; }
        public IList<Fi> Fs { get; private set; }
        public IList<Dv> Dvs { get; private set; }
        public IList<Mp> Mps { get; private set; }
        public IList<string> Ms { get; private set; }
        public IDictionary<string, string> Vs { get; private set; }
        public N D { get; internal set; }
        public S S { get; internal set; }
        public int N { get; internal set; }
        public int Jc { get; internal set; }
        public int Vw { get; internal set; }
        internal object R { get; set; }
        internal object Ra { get; set; }
        internal N Hv { get; set; }
        internal N Fc { get; set; }
        internal N Kq { get; set; }
        internal IDictionary<N, Fi> Fr { get; private set; }
        internal Uri Gu { get; set; }
        internal bool Gp { get; set; }
    }
}
