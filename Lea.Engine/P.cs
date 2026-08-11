using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    public sealed class P
    {
        internal P()
        {
            Ls = new List<Ln>();
            Bs = new List<B>();
        }

        public Uri U { get; internal set; }
        public string T { get; internal set; }
        public string H { get; internal set; }
        public string X { get; internal set; }
        public IList<Ln> Ls { get; private set; }
        public IList<B> Bs { get; private set; }
        public N D { get; internal set; }
        public int N { get; internal set; }
    }
}
