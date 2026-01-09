using System;

namespace CamAPS.DataReader;

public class InsulinInfusionWithGlucoseEntry
{
    public DateTime Time { get; set; }

    // Difference between infusion time and matched glucose time (infusion.Time - glucose.Time)
    public TimeSpan DateDiff { get; set; }

    // Value taken from the matched GlucoseMgPerLitre
    public double UnitsPerHour { get; set; }

    // Value taken from the matched GlucoseConcentrationEntry
    public double MgPerLitre { get; set; }
}