using System;
using System.Collections.Generic;
using System.Text;
using Diary.Model;

namespace Diary.Aggregator
{
    public class InsulinAggregator
    {
        public static List<AverageInsulinValuePerDayAndHour> AggregateByDateAndHour(
            IEnumerable<InsulinInfusionGlucoseEntry> entries)
        {
            // Schritt 1:
            // Insulin-Werte nach Datum und Stunde aufsummieren
            var perDayAndHour = entries
                .GroupBy(e => new
                {
                    Date = e.Time.Date,
                    Hour = e.Time.Hour
                })
                .Select(g => new
                {
                    g.Key.Hour,
                    InsulinSum = g.Sum(x => x.InsulinUnitsPerHour)
                });

            // Schritt 2:
            // Werte je Stunde aggregieren und durch Anzahl der Tage teilen
            var result = perDayAndHour
                .GroupBy(x => x.Hour)
                .Select(g => new AverageInsulinValuePerDayAndHour
                {
                    Hour = g.Key,
                    InsulinUnits = g.Sum(x => x.InsulinSum) / g.Count(),
                    Days = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();

            return result;
        }


    }

    public class AverageInsulinValuePerDayAndHour
    {
        public int Hour { get; set; }           // hour 0–23
        public double InsulinUnits { get; set; } = 0;
        public int Days { get; set; } = 0;
    }

}
