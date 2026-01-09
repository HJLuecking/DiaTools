namespace Diary.Model;

public class InsulinInfusionWithNearestGlucoseEntry
{
    public DateTime Time { get; set; }

    // Difference between infusion time and matched glucose time (infusion.Time - glucose.Time)
    public TimeSpan TimeDiff { get; set; }

    // Value taken from the matched InsulinInfusionEntry 
    public double InsulinUnitsPerHour { get; set; }

    // Value taken from the matched GlucoseConcentrationEntry 
    public double GlucoseMgPerLitre { get; set; }
}