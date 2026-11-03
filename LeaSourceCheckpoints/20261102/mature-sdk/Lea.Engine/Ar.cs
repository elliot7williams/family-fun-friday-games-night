using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // Ar is one compact image-map area.
    public sealed class Ar
    {
        internal Ar()
        {
            Cs = new List<double>();
        }

        public string K { get; internal set; }
        public IList<double> Cs { get; private set; }
        public Uri U { get; internal set; }
        public string T { get; internal set; }
        public N El { get; internal set; }
    }
}
