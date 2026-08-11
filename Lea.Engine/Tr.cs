using System.Collections.Generic;

namespace Lea.Engine
{
    // Tr is one compact table row.
    public sealed class Tr
    {
        internal Tr()
        {
            Cs = new List<Tc>();
        }

        public IList<Tc> Cs { get; private set; }
    }
}
