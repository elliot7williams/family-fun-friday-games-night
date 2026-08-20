using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // B is a small paint-ready document block.
    public sealed class B
    {
        internal B()
        {
            Mk = "";
            Is = new List<Ir>();
        }

        public string K { get; internal set; }
        public string X { get; internal set; }
        public string Mk { get; internal set; }
        public bool Hd { get; internal set; }
        public Uri U { get; internal set; }
        public S S { get; internal set; }
        public Fi I { get; internal set; }
        public Tb Tb { get; internal set; }
        public N El { get; internal set; }
        public B Up { get; internal set; }
        public IList<Ir> Is { get; private set; }
    }
}
