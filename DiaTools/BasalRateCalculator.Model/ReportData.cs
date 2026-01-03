namespace Diary.Model;

public class ReportData
{
    public string Id { get; set; } = string.Empty;
    public int LinesRead { get; set; }

    public List<MealEntry> Meals { get; set; } = new List<MealEntry>();
    public List<DateTime> PrimingEvents { get; set; } = new List<DateTime>();
    public List<DateTime> RefillEvents { get; set; } = new List<DateTime>();
    public List<InsulinBolusEntry> InsulinBoli { get; set; } = new List<InsulinBolusEntry>();
    public List<InsulinInfusionEntry> InsulinInfusions { get; set; } = new List<InsulinInfusionEntry>();
    public List<GlucoseConcentrationEntry> GlucoseConcentrations { get; set; } = new List<GlucoseConcentrationEntry>();
    public List<FingerstickGlucoseConcentrationEntry> FingerstickGlucoseConcentrations { get; set; } = new List<FingerstickGlucoseConcentrationEntry>();
    public List<DateTime> SensorInsertions { get; set; } = new List<DateTime>();
    public List<DateTime> SensorStopps { get; set; } = new List<DateTime>();
    public List<DateTime> AudioAlerts { get; set; } = new List<DateTime>();
    public List<DateTime> VibrateAlerts { get; set; } = new List<DateTime>();

    // Combined list: each infusion matched to the nearest glucose reading
    public List<InsulinInfusionWithGlucoseEntry> InsulinInfusionsWithGlucose { get; set; } = new List<InsulinInfusionWithGlucoseEntry>();

    // Build/refresh the combined list. Uses nearest-time matching (minimum absolute time gap).
    public void BuildInsulinInfusionsWithGlucose()
    {
        InsulinInfusionsWithGlucose = new List<InsulinInfusionWithGlucoseEntry>();

        if (!InsulinInfusions.Any() || !GlucoseConcentrations.Any()) return;

        var glucoseSorted = GlucoseConcentrations.OrderBy(g => g.Time).ToList();

        foreach (var infusion in InsulinInfusions)
        {
            var nearest = glucoseSorted
                .OrderBy(g => Math.Abs((infusion.Time - g.Time).Ticks))
                .FirstOrDefault();

            if (nearest == null) continue;

            var dateDiff = infusion.Time - nearest.Time; // signed difference

            InsulinInfusionsWithGlucose.Add(new InsulinInfusionWithGlucoseEntry
            {
                Time = infusion.Time,
                DateDiff = dateDiff,
                UnitsPerHour = infusion.UnitsPerHour,
                MgPerLitre = nearest.MMolPerLitre * 18.0 // ensure mg/dL stored; adjust if your GlucoseEntry already exposes mg/dL
            });
        }
    }
}