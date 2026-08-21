using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // Dv is a compact host-readable semantic value.
    public sealed class Dv
    {
        internal Dv()
        {
            F = new List<string>();
        }

        public string K { get; internal set; }
        public string V { get; internal set; }
        public string X { get; internal set; }
        public string N { get; internal set; }
        public string Fo { get; internal set; }
        public string Dt { get; internal set; }
        public Uri U { get; internal set; }
        public IList<string> F { get; private set; }
        public N El { get; internal set; }
    }
}
