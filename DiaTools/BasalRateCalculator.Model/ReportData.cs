namespace Diary.Model;

public class ReportData
{
    public string Id { get; set; } = string.Empty;
    public int LinesRead { get; set; }

    public List<MealEntry> Meals { get; set; } = [];
    public List<DateTime> PrimingEvents { get; set; } = [];
    public List<DateTime> RefillEvents { get; set; } = [];
    public List<InsulinBolusEntry> InsulinBoli { get; set; } = [];
    public List<InsulinInfusionEntry> InsulinInfusions { get; set; } = [];
    public List<GlucoseConcentrationEntry> GlucoseConcentrations { get; set; } = [];
    public List<FingerstickGlucoseConcentrationEntry> FingerstickGlucoseConcentrations { get; set; } = [];
    public List<DateTime> SensorInsertions { get; set; } = [];
    public List<DateTime> SensorStops { get; set; } = [];
    public List<DateTime> AudioAlerts { get; set; } = [];
    public List<DateTime> VibrateAlerts { get; set; } = [];

    // Combined list: each infusion matched to the nearest glucose reading
    public List<InsulinInfusionWithNearestGlucoseEntry> InsulinInfusionWithNearestGlucose { get; set; } = [];

    // Build/refresh the combined list. Uses nearest-time matching (minimum absolute time gap).
    public void BuildInsulinInfusionsWithGlucose()
    {
        InsulinInfusionWithNearestGlucose = [];

        if (!InsulinInfusions.Any() || !GlucoseConcentrations.Any()) return;

        var glucoseSorted = GlucoseConcentrations.OrderBy(g => g.Time).ToList();

        foreach (var infusion in InsulinInfusions)
        {
            var nearest = glucoseSorted
                .OrderBy(g => Math.Abs((infusion.Time - g.Time).Ticks))
                .FirstOrDefault();

            if (nearest == null) continue;

            var timeDiff = infusion.Time - nearest.Time; // signed difference

            InsulinInfusionWithNearestGlucose.Add(new InsulinInfusionWithNearestGlucoseEntry
            {
                Time = infusion.Time,
                TimeDiff = timeDiff,
                InsulinUnitsPerHour = infusion.UnitsPerHour,
                GlucoseMgPerLitre = nearest.MMolPerLitre * 18.0 // ensure mg/dL stored; adjust if your GlucoseEntry already exposes mg/dL
            });
        }
    }
}