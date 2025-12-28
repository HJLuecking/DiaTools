using System;
using System.Collections.Generic;
using System.Text;

namespace BasalRateCalculator
{
    using System;

        public class DataPoint(DateTime time, double? glucoseMgDl, double basal, bool isExcluded = false)
        {
            public DateTime Time { get; set; } = time;
            public double? GlucoseMgDl { get; set; } = glucoseMgDl; // mg/dl, nullable wenn nicht interpolierbar
            public double BasalUnitsPerHour { get; set; } = basal;
            public bool IsExcluded { get; set; } = isExcluded; // true wenn ausgeschlossen (Bolus-Fenster, Glukose out-of-range, fehlend)
        }
}
