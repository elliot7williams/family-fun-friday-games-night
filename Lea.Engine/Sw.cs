namespace Lea.Engine
{
    // Sw is one compact, bounded CSS box shadow.
    public sealed class Sw
    {
        internal Sw() { C = ""; }

        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int B { get; internal set; }
        public int R { get; internal set; }
        public string C { get; internal set; }
        public bool I { get; internal set; }
    }
}
