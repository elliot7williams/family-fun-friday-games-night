using System.Collections.Generic;

namespace Lea.Engine
{
    // Mp is a compact named image map.
    public sealed class Mp
    {
        internal Mp()
        {
            As = new List<Ar>();
        }

        public string N { get; internal set; }
        public IList<Ar> As { get; private set; }
        public N El { get; internal set; }
    }
}
