using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // Vs is a compact SVG shape; P stores primitive coordinates and D stores path steps.
    public sealed class Vs
    {
        internal Vs()
        {
            K = "";
            T = "";
            F = "none";
            S = "none";
            C = "#000000";
            A = "s";
            P = new List<double>();
            D = new List<Vp>();
        }

        public string K { get; internal set; }
        public string T { get; internal set; }
        public string F { get; internal set; }
        public string S { get; internal set; }
        public string C { get; internal set; }
        public string A { get; internal set; }
        public double W { get; internal set; }
        public double O { get; internal set; }
        public double Fo { get; internal set; }
        public double So { get; internal set; }
        public double Z { get; internal set; }
        public Uri U { get; internal set; }
        public N El { get; internal set; }
        public IList<double> P { get; private set; }
        public IList<Vp> D { get; private set; }
    }

    // Vp is one absolute move, line, or close step in Lea's straight-path subset.
    public sealed class Vp
    {
        internal Vp(char kind, double x, double y)
        {
            K = kind;
            X = x;
            Y = y;
        }

        public char K { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
    }
}
