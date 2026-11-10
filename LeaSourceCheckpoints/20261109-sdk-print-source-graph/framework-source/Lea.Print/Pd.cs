using System;
using System.Printing;

namespace Lea.Print
{
    public static class Pd
    {
        // Query capabilities for the selected, validated ticket before calling.
        // Dimensions already include orientation; never rotate them a second time.
        public static Pg Cp(Po options, PrintCapabilities capabilities)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            double? width = capabilities.OrientedPageMediaWidth;
            double? height = capabilities.OrientedPageMediaHeight;
            PageImageableArea area = capabilities.PageImageableArea;
            if (!width.HasValue || !height.HasValue || area == null)
                throw new InvalidOperationException("Printer did not report complete oriented page geometry");
            const double units = 100.0 / 96.0;
            return Pg.Cp(options, width.Value * units, height.Value * units,
                new Pb(area.OriginWidth * units, area.OriginHeight * units,
                    area.ExtentWidth * units, area.ExtentHeight * units));
        }
    }
}
