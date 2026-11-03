using System;

namespace Lea.Engine
{
    // Ax is one compact, host-readable accessibility record.
    public sealed class Ax
    {
        public const int Ds = 1;
        public const int Ck = 2;
        public const int Mx = 4;
        public const int Sl = 8;
        public const int Ex = 16;
        public const int Co = 32;
        public const int Rq = 64;
        public const int Ro = 128;
        public const int Iv = 256;
        public const int Fs = 512;
        public const int Fc = 1024;
        public const int Ps = 2048;
        public const int Bs = 4096;
        public const int Cr = 8192;

        private string value;
        private int state;
        internal Fi F { get; set; }

        internal Ax()
        {
            K = "";
            N = "";
            D = "";
            value = "";
        }

        public string K { get; internal set; }
        public string N { get; internal set; }
        public string D { get; internal set; }
        public string V
        {
            get
            {
                if (F == null) return value;
                if (F.K == "password" || F.K == "file" || F.K == "checkbox" || F.K == "radio") return "";
                if (F.K == "select" && F.Si >= 0 && F.Si < F.Os.Count) return Clip(F.Os[F.Si].T);
                return Clip(F.V);
            }
            internal set { this.value = Clip(value); }
        }
        public int St
        {
            get
            {
                var result = state;
                if (F != null)
                {
                    if (El == null || !El.A.ContainsKey("aria-checked"))
                    {
                        result &= ~Ck;
                        if (F.C) result |= Ck;
                    }
                    if (F.Ds) result |= Ds;
                    if (F.Rq) result |= Rq;
                    if (F.Ro) result |= Ro;
                    if (!F.Ok) result |= Iv;
                }
                if (El != null)
                {
                    if (El.Fc) result |= Fc;
                    else result &= ~Fc;
                    var details = El.K == "details" ? El : El.K == "summary" && El.Up != null && El.Up.K == "details" ? El.Up : null;
                    if (details != null)
                    {
                        result &= ~(Ex | Co);
                        result |= details.Op ? Ex : Co;
                    }
                }
                return result;
            }
            internal set { state = value; }
        }
        public int Lv { get; internal set; }
        public Uri U { get; internal set; }
        public N El { get; internal set; }
        public Ax Up { get; internal set; }

        public bool Is(int flag)
        {
            return (St & flag) != 0;
        }

        private static string Clip(string input)
        {
            var text = input ?? "";
            return text.Length <= 512 ? text : text.Substring(0, 512);
        }
    }
}
