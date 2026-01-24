using Diary.Model;

namespace Diary.Aggregator;

public class InsulinByHourAggregator
{
    public static List<ValuePerHour> AggregateInsulinInfusionByHour(List<InsulinInfusionGlucoseEntry>? entries, int minValuesPerHour)
    {
        if (entries == null || entries.Count == 0) return [];
        var valuesPerHourAndDay = InsulinByHourAndDateAggregator.AggregateInsulinInfusionByHourAndDay(entries);
        var result = new List<ValuePerHour>();
        for (int hour = 0; hour < 24; hour++)
        {
            var valuePerHour = new ValuePerHour
            {
                Hour = hour
            };
            var currentHour = hour;
            var matchingEntries = valuesPerHourAndDay
                .Where(v => 
                    v.Hour == currentHour &&
                    v.CountInsulinUnitsPerHour >= minValuesPerHour)
                .ToArray();
            foreach (var entry in matchingEntries)
            {
                valuePerHour.InsulinUnitsPerHour.AddRange(entry.InsulinUnitsPerHour);
            }
            valuePerHour.DaysUsed = matchingEntries.Length;
            result.Add(valuePerHour);
        }

        if (ApplySmoothing)
        {
            for (int h = 0; h < 24; h++)
            {
                var p = h - 1;
                var c = h;
                var n = h + 1;
                if (h == 0) p = 23;
                if (h == 23) n = 0;
                result[h].SmoothedAverageInsulinPerHourPerDay=
                    (result[p].AverageInsulinPerHourPerDay +
                     result[c].AverageInsulinPerHourPerDay +
                     result[n].AverageInsulinPerHourPerDay) / 3.0;
            }
        }
        return result;
    }

    private static double Median(List<double>? sorted)
    {
        if (sorted == null || sorted.Count == 0) return 0.0;
        var n = sorted.Count;
        if (n % 2 == 1) return sorted[n / 2];
        return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
    }

    public static bool ApplySmoothing { get; set; } = true;
}

