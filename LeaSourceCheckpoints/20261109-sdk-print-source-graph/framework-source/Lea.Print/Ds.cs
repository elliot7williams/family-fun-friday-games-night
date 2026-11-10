using System;
using Lea.Win;

namespace Lea.Print
{
    public sealed class Ds
    {
        public Pc Capture { get; private set; }
        public string Text { get; private set; }
        private Ds() { }

        public string Ht(Po options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (!options.SelectionOnly) throw new ArgumentException("Document selection output requires SelectionOnly", "options");
            return Lea.Print.Ht.Cp(Capture.Document, options, Text);
        }

        public static Ds Cp(V view, Pg geometry)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (geometry == null) throw new ArgumentNullException("geometry");
            var page = view.At;
            string selected = view.Sds();
            if (String.IsNullOrWhiteSpace(selected)) throw new InvalidOperationException("Select document text before capturing selection output");
            var capture = Pc.Cf(view, geometry);
            if (!Object.ReferenceEquals(page, view.At) || view.Sds() != selected)
                throw new InvalidOperationException("Document selection changed during print capture");
            return new Ds { Capture = capture, Text = selected };
        }
    }
}
