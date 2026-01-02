namespace BasalRateCalculator.Model;

public class InsulinInfusionWithGlucoseEntry
{
    public DateTime Time { get; set; }

    // Difference between infusion time and matched glucose time (infusion.Time - glucose.Time)
    public TimeSpan DateDiff { get; set; }

    public double UnitsPerHour { get; set; }

    // Value taken from the matched GlucoseConcentrationEntry (MgPerLitre chosen here)
    public double InsulinInfusionEntry { get; set; }
}