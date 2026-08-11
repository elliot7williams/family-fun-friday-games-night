using System.Collections.Generic;

namespace Lea.Engine
{
    // Tb is a compact parsed table.
    public sealed class Tb
    {
        internal Tb()
        {
            Cp = "";
            Rs = new List<Tr>();
        }

        public string Cp { get; internal set; }
        public IList<Tr> Rs { get; private set; }
        public int Cs { get; internal set; }
    }
}
