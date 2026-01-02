using System;
using System.Collections.Generic;
using System.Linq;
using BasalRateCalculator.Model;

namespace CamAPS.DataReader;

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
    public List<DateTime> SensorStopps { get; set; } = [];
    public List<DateTime> AudioAlerts { get; set; } = [];
    public List<DateTime> VibrateAlerts { get; set; } = [];

    // New property: combined list matching each infusion to the nearest glucose value
    public List<InsulinInfusionWithGlucoseEntry> InsulinInfusionsWithGlucose { get; set; } = [];

    // Build/refresh the combined list. Uses nearest-time matching (minimum absolute time gap).
    public void BuildInsulinInfusionsWithGlucose()
    {
        InsulinInfusionsWithGlucose = [];

        if (!InsulinInfusions.Any() || !GlucoseConcentrations.Any()) return;

        // Pre-sort glucose entries to slightly speed up repeated nearest searches if desired
        var glucoseSorted = GlucoseConcentrations.OrderBy(g => g.Time).ToList();

        foreach (var infusion in InsulinInfusions)
        {
            // Find nearest glucose by absolute time difference
            var nearest = glucoseSorted
                .OrderBy(g => Math.Abs((infusion.Time - g.Time).Ticks))
                .FirstOrDefault();

            if (nearest == null) continue;

            var dateDiff = infusion.Time - nearest.Time; // signed difference as requested

            InsulinInfusionsWithGlucose.Add(new InsulinInfusionWithGlucoseEntry
            {
                Time = infusion.Time,
                DateDiff = dateDiff,
                UnitsPerHour = infusion.UnitsPerHour,
                InsulinInfusionEntry = nearest.MgPerLitre
            });
        }
    }
}