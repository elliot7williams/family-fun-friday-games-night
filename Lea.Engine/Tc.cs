using System.Collections.Generic;

namespace Lea.Engine
{
    // Tc is one compact table cell.
    public sealed class Tc
    {
        internal Tc()
        {
            Bs = new List<B>();
        }

        public string X { get; internal set; }
        public S S { get; internal set; }
        public bool H { get; internal set; }
        public int Sp { get; internal set; }
        public IList<B> Bs { get; private set; }
    }
}
