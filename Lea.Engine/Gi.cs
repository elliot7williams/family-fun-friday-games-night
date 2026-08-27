using System.Collections.Generic;

namespace Lea.Engine
{
    // Gi is one compact CSS gradient image; Cs is one normalized color stop.
    public sealed class Gi
    {
        internal Gi()
        {
            K = "l";
            A = 180;
            X = 50;
            Y = 50;
            Ss = new List<Cs>();
        }

        public string K { get; internal set; }
        public int A { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public IList<Cs> Ss { get; private set; }
    }

    public sealed class Cs
    {
        internal Cs(string color, int position)
        {
            C = color;
            P = position;
        }

        public string C { get; private set; }
        public int P { get; private set; }
    }
}
