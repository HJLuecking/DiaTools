using Diary.Model;

namespace Diary.Aggregator
{
    public class InsulinByHourAndDateAggregator
    {

        public static List<ValuePerHourOfDate> AggregateInsulinInfusionByHourAndDay(List<InsulinInfusionGlucoseEntry>? entries)
        {
            if (entries == null || entries.Count == 0) return [];
            var result = CreateResultHourlyDateBuckets(entries);
            AddEntriesToHourlyDateBuckets(entries, result);
            return result;
        }

        private static void AddEntriesToHourlyDateBuckets(List<InsulinInfusionGlucoseEntry> entries, List<ValuePerHourOfDate> result)
        {
            foreach (var entry in entries)
            {
                var hourKey = new DateTime(
                    entry.Time.Year,
                    entry.Time.Month,
                    entry.Time.Day,
                    entry.Time.Hour,
                    0,
                    0);

                var bucket = result.First(v =>
                    v.Date == hourKey.Date &&
                    v.Hour == hourKey.Hour);

                bucket.InsulinUnitsPerHour.Add(entry.InsulinUnitsPerHour);
            }
        }

        private static List<ValuePerHourOfDate> CreateResultHourlyDateBuckets(List<InsulinInfusionGlucoseEntry> entries)
        {
            var minTime = entries.Min(e => e.Time);
            var maxTime = entries.Max(e => e.Time);
            var startDateHour = new DateTime(minTime.Year, minTime.Month, minTime.Day, minTime.Hour, 0, 0);
            var endDateHour = new DateTime(maxTime.Year, maxTime.Month, maxTime.Day, maxTime.Hour, 0, 0);

            var result = new List<ValuePerHourOfDate>();
            for (var current = startDateHour; current <= endDateHour; current = current.AddHours(1))
            {
                result.Add(new ValuePerHourOfDate
                {
                    Date = current.Date,
                    Hour = current.Hour
                });
            }
            return result;
        }
    }
}
