using System;
using Lea.Engine;

namespace Lea.Print
{
    public sealed class Py
    {
        public string Display { get; private set; }
        public string Visibility { get; private set; }
        public int Opacity { get; private set; }
        public int FontSize { get; private set; }
        public string FontFamily { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public string Foreground { get; private set; }
        public string Background { get; private set; }
        public string Decorations { get; private set; }
        public string Alignment { get; private set; }
        public string Direction { get; private set; }
        public string WhiteSpace { get; private set; }
        private Py() { }
        internal static Py Cp(S style, ref int characters)
        {
            if (style.Fs < 0 || style.Fs >= 200 || style.Op < 0 || style.Op > 100) throw new InvalidOperationException("Invalid print style values");
            return new Py
            {
                Display = Ps.Take(style.D, ref characters), Visibility = Ps.Take(style.Vi, ref characters), Opacity = style.Op,
                FontSize = style.Fs, FontFamily = Ps.Take(style.Ff, ref characters), Bold = style.B, Italic = style.I,
                Foreground = Ps.Take(style.F, ref characters), Background = Ps.Take(style.Bg, ref characters),
                Decorations = Ps.Take(style.Td, ref characters), Alignment = Ps.Take(style.Ta, ref characters),
                Direction = Ps.Take(style.Dr, ref characters), WhiteSpace = Ps.Take(style.Ws, ref characters)
            };
        }
    }
}
