using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // Fi is Lea's compact, mutable form-field state.
    public sealed class Fi
    {
        internal Fi()
        {
            Os = new List<O>();
        }

        public string K { get; internal set; }
        public string N { get; internal set; }
        public string V { get; set; }
        public string P { get; internal set; }
        public bool C { get; set; }
        public bool Ds { get; internal set; }
        public bool Ro { get; internal set; }
        public int Mx { get; internal set; }
        public N F { get; internal set; }
        public Uri U { get; internal set; }
        public string M { get; internal set; }
        public S S { get; internal set; }
        public IList<O> Os { get; private set; }
        public int Si { get; set; }

        public string D { get; internal set; }
        public bool Dc { get; internal set; }
        public int Di { get; internal set; }
        public Lea.Engine.N El { get; internal set; }
    }
}
