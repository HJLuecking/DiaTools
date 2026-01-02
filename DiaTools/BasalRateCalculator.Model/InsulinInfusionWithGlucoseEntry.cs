using System;

namespace BasalRateCalculator.Model;

public class InsulinInfusionWithGlucoseEntry
{
    public DateTime Time { get; set; }

    // Difference between infusion time and matched glucose time (infusion.Time - glucose.Time)
    public TimeSpan DateDiff { get; set; }

    // Value taken from the matched InsulinInfusionEntry
    public double UnitsPerHour { get; set; }

    // Value taken from the matched GlucoseConcentrationEntry (mg/dL)
    public double MgPerLitre { get; set; }
}