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
            Fr = "n";
            Lc = "b";
            Lj = "m";
            Ml = 4D;
            P = new List<double>();
            D = new List<Vp>();
            Da = new List<double>();
            Cm = new List<double> { 1D, 0D, 0D, 1D, 0D, 0D };
            M = new List<double> { 1D, 0D, 0D, 1D, 0D, 0D };
        }

        public string K { get; internal set; }
        public string T { get; internal set; }
        public string F { get; internal set; }
        public string S { get; internal set; }
        public string C { get; internal set; }
        public string A { get; internal set; }
        public string Fr { get; internal set; }
        public string Lc { get; internal set; }
        public string Lj { get; internal set; }
        public double W { get; internal set; }
        public double O { get; internal set; }
        public double Fo { get; internal set; }
        public double So { get; internal set; }
        public double Z { get; internal set; }
        public double Ml { get; internal set; }
        public double Do { get; internal set; }
        public Uri U { get; internal set; }
        public N El { get; internal set; }
        public Vd Fd { get; internal set; }
        public Vd Sd { get; internal set; }
        public Vc Cp { get; internal set; }
        public bool Cl { get; internal set; }
        public IList<double> P { get; private set; }
        public IList<Vp> D { get; private set; }
        public IList<double> Da { get; private set; }
        public IList<double> Cm { get; private set; }
        public IList<double> M { get; private set; }
    }

    // Vp stores one canonical absolute path step; curve controls use A/B and C/D.
    public sealed class Vp
    {
        internal Vp(char kind, double x, double y)
            : this(kind, x, y, 0D, 0D, 0D, 0D)
        {
        }

        internal Vp(char kind, double x, double y, double a, double b, double c, double d)
        {
            K = kind;
            X = x;
            Y = y;
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public char K { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }
        public double D { get; private set; }
    }

    // Vd is one bounded SVG linear or radial paint definition.
    public sealed class Vd
    {
        internal Vd()
        {
            I = "";
            K = "";
            U = "b";
            P = new List<double>();
            M = new List<double> { 1D, 0D, 0D, 1D, 0D, 0D };
            Ss = new List<Vt>();
        }

        public string I { get; internal set; }
        public string K { get; internal set; }
        public string U { get; internal set; }
        public IList<double> P { get; private set; }
        public IList<double> M { get; private set; }
        public IList<Vt> Ss { get; private set; }
    }

    // Vt is one normalized gradient stop.
    public sealed class Vt
    {
        internal Vt(double offset, string color, double alpha)
        {
            O = offset;
            C = color;
            A = alpha;
        }

        public double O { get; private set; }
        public string C { get; private set; }
        public double A { get; private set; }
    }

    // Vc is one bounded SVG clipping definition.
    public sealed class Vc
    {
        internal Vc()
        {
            I = "";
            U = "u";
            M = new List<double> { 1D, 0D, 0D, 1D, 0D, 0D };
            Ss = new List<Vs>();
        }

        public string I { get; internal set; }
        public string U { get; internal set; }
        public IList<double> M { get; private set; }
        public IList<Vs> Ss { get; private set; }
    }
}
