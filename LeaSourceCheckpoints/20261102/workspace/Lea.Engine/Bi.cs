using System;

namespace Lea.Engine
{
    // Bi is one compact CSS bitmap background and its bounded tile geometry.
    public sealed class Bi
    {
        internal Bi()
        {
            R = "b";
            Xp = true;
            Yp = true;
            Wa = true;
            Ha = true;
            K = "";
        }

        public Uri U { get; internal set; }
        public string R { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public bool Xp { get; internal set; }
        public bool Yp { get; internal set; }
        public int W { get; internal set; }
        public int H { get; internal set; }
        public bool Wp { get; internal set; }
        public bool Hp { get; internal set; }
        public bool Wa { get; internal set; }
        public bool Ha { get; internal set; }
        public string K { get; internal set; }

        internal Bi Copy()
        {
            return new Bi
            {
                U = U,
                R = R,
                X = X,
                Y = Y,
                Xp = Xp,
                Yp = Yp,
                W = W,
                H = H,
                Wp = Wp,
                Hp = Hp,
                Wa = Wa,
                Ha = Ha,
                K = K
            };
        }
    }
}
