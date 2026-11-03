namespace Lea.Engine
{
    // Vm is one compact, paint-ready progress or meter value model.
    public sealed class Vm
    {
        internal Vm() { }

        public double V { get; internal set; }
        public double Mn { get; internal set; }
        public double Mx { get; internal set; }
        public double Lo { get; internal set; }
        public double Hi { get; internal set; }
        public double Op { get; internal set; }
        public double R { get; internal set; }
        public int Q { get; internal set; }
        public bool In { get; internal set; }
    }
}
