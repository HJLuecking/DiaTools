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
            var bucket = new ValuePerHour
            {
                Hour = hour
            };
            var currentHour = hour;
            var matchingEntries = valuesPerHourAndDay
                .Where(v => 
                    v.Hour == currentHour &&
                    v.CountInsulinUnitsPerHour >= minValuesPerHour);
            foreach (var entry in matchingEntries)
            {
                bucket.InsulinUnitsPerHour.AddRange(entry.InsulinUnitsPerHour);
            }
            bucket.DaysUsed = matchingEntries.Count();
            result.Add(bucket);
        }

        if (ApplySmoothing)
        {
            for (int h = 0; h < 24; h++)
            {
                var p = h - 1;
                var c = h;
                var n = h + 1;
                if (h == 0) p = 24;
                if (h == 24) n = 0;
                result[h].SmoothedAverageInsulinPerHourPerDay=
                    (result[p].AverageInsulinPerHourPerDay +
                     result[c].AverageInsulinPerHourPerDay +
                     result[n].AverageInsulinPerHourPerDay) / 3.0;
            }
        }
        return result;
    }
    public static bool ApplySmoothing { get; set; } = false;
}

