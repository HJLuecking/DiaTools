using Diary.Model;

namespace BasalRateCalculator;

public class BasalOptimizer
{
    // Konfigurierbare Parameter
    public TimeSpan GridStep { get; set; } = TimeSpan.FromMinutes(5);
    public double TargetMinMgDl { get; set; } = 70.0;
    public double TargetMaxMgDl { get; set; } = 150.0;
    public TimeSpan ExcludeAfterBolus { get; set; } = TimeSpan.FromHours(4); // wie gewünscht
    public double StabilityDeltaMgPerHour { get; set; } = 15.0;
    public double MinValidFractionPerHour { get; set; } = 0.6; // z.B. 60% der 5-min Punkte
    public double MaxInterpolationGapMinutes { get; set; } = 20.0; // Lücken > 20 min nicht interpolieren
    public bool ApplySmoothing { get; set; } = true; // 3-stündiger gleitender Mittelwert

    // Rohdaten
    private readonly List<GlucoseEntry> _glucose = new List<GlucoseEntry>();
    private readonly List<InsulinInfusionEntry> _basal = new List<InsulinInfusionEntry>();
    private readonly List<InsulinBolusEntry> _boluses = new List<InsulinBolusEntry>();

    // Synchronisierte Grid-Punkte (nach BuildUnifiedGrid gefüllt)
    private List<DataPoint> _gridPoints = new List<DataPoint>();

    // Public API zum Hinzufügen von Rohdaten
    public void AddGlucoseEntries(IEnumerable<GlucoseEntry> entries) => _glucose.AddRange(entries);
    public void AddBasalEntries(IEnumerable<InsulinInfusionEntry> entries) => _basal.AddRange(entries);
    public void AddBolusEntries(IEnumerable<InsulinBolusEntry> entries) => _boluses.AddRange(entries);

    /// <summary>
    /// Baut ein gemeinsames Raster (z.B. 5-Minuten) zwischen start (inkl.) und end (exkl.).
    /// IWendet Basal als step function an und markiert Ausschlussintervalle.
    /// </summary>
    public List<DataPoint> BuildUnifiedGrid(DateTime start, DateTime end)
    {
        if (!_basal.Any())
            throw new InvalidOperationException("Basal schedule is empty. Provide InsulinInfusionEntry items.");

        // Sortiere Rohdaten
        var glucose = _glucose.OrderBy(g => g.Time).ToList();
        var basal = _basal.OrderBy(b => b.Time).ToList();
        var boluses = _boluses.OrderBy(b => b.Time).ToList();

        // Erzeuge Ausschlussintervalle: von Bolus.Time bis max(Bolus.Time + Minutes, Bolus.Time + ExcludeAfterBolus)
        var excludeIntervals = boluses
            .Select(b =>
            {
                var dur = TimeSpan.FromMinutes(Math.Max(20, 0));
                var endByDuration = b.Time + dur;
                var endByAfter = b.Time + ExcludeAfterBolus;
                var end = endByDuration > endByAfter ? endByDuration : endByAfter;
                return (Start: b.Time, End: end);
            })
            .ToList();

        bool IsInExcludeIntervals(DateTime t) => excludeIntervals.Any(iv => iv.Start <= t && t < iv.End);

        // Interpolationsvorbereitung für Glukose (in mg/dl)
        Func<DateTime, double?> interpGlucoseMgDl = (DateTime t) =>
        {
            if (!glucose.Any()) return null;
            // exakter Messzeitpunkt?
            var exact = glucose.FirstOrDefault(g => g.Time == t);
            if (exact != null) return exact.MgPerLitre;

            var right = glucose.FirstOrDefault(g => g.Time > t);
            var left = glucose.LastOrDefault(g => g.Time < t);
            if (left == null || right == null) return null;

            var gap = (right.Time - left.Time).TotalMinutes;
            if (gap > MaxInterpolationGapMinutes) return null;

            var leftMg = left.MgPerLitre;
            var rightMg = right.MgPerLitre;
            var frac = (t - left.Time).TotalSeconds / (right.Time - left.Time).TotalSeconds;
            return leftMg + frac * (rightMg - leftMg);
        };

        // Basal als step function: letzte InsulinInfusionEntry mit Start <= t, sonst erster Eintrag
        var basalAt = (DateTime t) =>
        {
            var entry = basal.LastOrDefault(b => b.Time <= t) ?? basal.First();
            return entry.UnitsPerHour;
        };

        // Erzeuge Raster
        var grid = new List<DataPoint>();
        for (var t = start; t < end; t = t + GridStep)
        {
            var basalValue = basalAt(t);
            var g = interpGlucoseMgDl(t);
            var excluded = false;

            if (IsInExcludeIntervals(t)) excluded = true;
            if (g.HasValue && (g.Value < TargetMinMgDl || g.Value > TargetMaxMgDl)) excluded = true;
            if (!g.HasValue) excluded = true; // fehlende Glukose behandeln wir als ausgeschlossen

            grid.Add(new DataPoint(t, g, basalValue, excluded));
        }

        _gridPoints = grid;
        return grid;
    }

    /// <summary>
    /// Berechnet optimale Basalraten pro Stunde des Tages (0..23) aus dem zuvor gebauten Grid.
    /// </summary>
    public Dictionary<int, double> ComputeOptimalBasalPerHourOfDayFromGrid()
    {
        if (_gridPoints == null || !_gridPoints.Any())
            throw new InvalidOperationException("Grid not built. Call BuildUnifiedGrid first.");

        // Gruppiere nach Stunde-Start (Datum+Stunde)
        var pointsByHourStart = _gridPoints
            .GroupBy(p => new DateTime(p.Time.Year, p.Time.Month, p.Time.Day, p.Time.Hour, 0, 0))
            .ToDictionary(g => g.Key, g => g.ToList());

        const int expectedPointsPerHour = 60 / 5; // 12

        var usableHourMeans = new Dictionary<DateTime, double>();

        foreach (var kv in pointsByHourStart)
        {
            var hourStart = kv.Key;
            var pts = kv.Value.OrderBy(p => p.Time).ToList();

            var validPts = pts.Where(p => !p.IsExcluded && p.GlucoseMgDl.HasValue).ToList();
            var validFraction = (double)validPts.Count / expectedPointsPerHour;
            if (validFraction < MinValidFractionPerHour) continue;

            // Stabilitätsprüfung: Delta zwischen erstem und letztem gültigen Messwert in der Stunde
            var first = validPts.First();
            var last = validPts.Last();
            var gStart = first.GlucoseMgDl.Value;
            var gEnd = last.GlucoseMgDl.Value;
            if (Math.Abs(gEnd - gStart) > StabilityDeltaMgPerHour) continue;

            var meanBasal = validPts.Average(p => p.BasalUnitsPerHour);
            usableHourMeans[hourStart] = meanBasal;
        }

        if (!usableHourMeans.Any()) return new Dictionary<int, double>();

        // Aggregiere nach hour-of-day und berechne Median
        var byHourOfDay = new Dictionary<int, List<double>>();
        foreach (var kv in usableHourMeans)
        {
            var hod = kv.Key.Hour;
            if (!byHourOfDay.ContainsKey(hod)) byHourOfDay[hod] = new List<double>();
            byHourOfDay[hod].Add(kv.Value);
        }

        var optimalByHour = new Dictionary<int, double>();
        foreach (var kv in byHourOfDay)
        {
            var hod = kv.Key;
            var vals = kv.Value.OrderBy(v => v).ToList();
            var median = Median(vals);
            optimalByHour[hod] = median;
        }

        // Glätten (3-stündiger gleitender Mittelwert) falls gewünscht
        if (ApplySmoothing)
        {
            var smoothed = new Dictionary<int, double>();
            for (var hod = 0; hod < 24; hod++)
            {
                var neighbors = new List<double>();
                for (var d = -1; d <= 1; d++)
                {
                    var nh = (hod + d + 24) % 24;
                    if (optimalByHour.TryGetValue(nh, out var v)) neighbors.Add(v);
                }
                if (neighbors.Count > 0) smoothed[hod] = neighbors.Average();
            }
            foreach (var kv in smoothed) optimalByHour[kv.Key] = kv.Value;
        }

        return optimalByHour;
    }

    // Hilfsfunktion: Median einer sortierten Liste
    private static double Median(List<double> sorted)
    {
        if (sorted == null || sorted.Count == 0) return 0.0;
        var n = sorted.Count;
        if (n % 2 == 1) return sorted[n / 2];
        return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
    }

    // Optional: Hilfsmethode, die alles in einem Schritt ausführt (Add + Build + Compute)
    public Dictionary<int, double> RunOptimization(DateTime start, DateTime end)
    {
        BuildUnifiedGrid(start, end);
        return ComputeOptimalBasalPerHourOfDayFromGrid();
    }
}