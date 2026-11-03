using System;

namespace Lea.Engine
{
    // D is the compact result of a completed Lea download.
    public sealed class D
    {
        internal D() { }

        public Uri U { get; internal set; }
        public string P { get; internal set; }
        public string N { get; internal set; }
        public string Ct { get; internal set; }
        public long Sz { get; internal set; }
        public int St { get; internal set; }
    }
}
