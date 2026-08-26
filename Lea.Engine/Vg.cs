using System.Collections.Generic;

namespace Lea.Engine
{
    // Vg is Lea's bounded, paint-ready vector graphic.
    public sealed class Vg
    {
        internal Vg()
        {
            A = "mm";
            M = "m";
            Ss = new List<Vs>();
        }

        public double W { get; internal set; }
        public double H { get; internal set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }
        public double Vw { get; internal set; }
        public double Vh { get; internal set; }
        public bool Vb { get; internal set; }
        public string A { get; internal set; }
        public string M { get; internal set; }
        public IList<Vs> Ss { get; private set; }
    }
}
