using Diary.Model;

namespace Diary.Aggregator
{
    public static class GlucoseFilter
    {
        /// <param name="items">Source list of <see cref="InsulinInfusionGlucoseEntry"/>.</param>
        extension(IEnumerable<InsulinInfusionGlucoseEntry>? items)
        {
            /// <summary>
            /// Filters the provided infusion-with-glucose entries keeping only those whose matched glucose value
            /// is between <paramref name="minimumGlucose"/> and <paramref name="maximumGlucose"/> (inclusive).
            /// </summary>
            /// <param name="minimumGlucose">Lower glucose bound (mg/dL).</param>
            /// <param name="maximumGlucose">Upper glucose bound (mg/dL).</param>
            /// <returns>New list containing entries with glucose in the requested range.</returns>
            /// <exception cref="ArgumentException">Thrown when MinimumGlucose &gt; MaximumGlucose.</exception>
            public List<InsulinInfusionGlucoseEntry> FilterByMinimumAndMaximumGlucose(double minimumGlucose,
                double maximumGlucose)
            {
                if (minimumGlucose > maximumGlucose)
                    throw new ArgumentException($"{nameof(minimumGlucose)} must be less than or equal to {nameof(maximumGlucose)}.");

                if (items == null) return [];

                return items
                    .Where(entry => !double.IsNaN(entry.GlucoseMgPerLitre)
                                    && entry.GlucoseMgPerLitre >= minimumGlucose
                                    && entry.GlucoseMgPerLitre <= maximumGlucose)
                    .ToList();
            }

            /// <summary>
            /// Filters the provided infusion-with-glucose entries keeping only those whose absolute time difference
            /// between infusion and matched glucose is less than or equal to <paramref name="minutes"/>.
            /// </summary>
            /// <param name="minutes">Maximum allowed absolute time difference (minutes).</param>
            /// <returns>New list containing entries with |TimeDiff| &lt;= maximumTimeDiff.</returns>
            /// <exception cref="ArgumentException">Thrown when <paramref name="minutes"/> is negative.</exception>
            public List<InsulinInfusionGlucoseEntry> FilterByMaximumTimeDiff(int minutes)
            {
                if (minutes < 0)
                    throw new ArgumentException($"{nameof(minutes)} must be non-negative.");
                var maximumTimeDiff = TimeSpan.FromMinutes(minutes);

                if (items == null) return [];

                return items
                    .Where(e => e.TimeDiff.Duration() <= maximumTimeDiff)
                    .ToList();
            }
        }

        /// <summary>
        /// Excludes infusion-with-glucose entries that fall into any bolus exclusion interval.
        /// For each bolus the exclusion interval is from bolus.Time to max(bolus.Time + Minutes, bolus.Time + ExcludeAfterBolus).
        /// </summary>
        /// <param name="glucoseEntries">Source list of <see cref="InsulinInfusionGlucoseEntry"/>.</param>
        /// <param name="insulinBoli">List of bolus events (maybe null or empty).</param>
        /// <param name="excludeMinutesAfterBolus">Exclusion timespan in minutes after bolus</param>
        /// <returns>Filtered list excluding entries within bolus exclusion intervals.</returns>
        public static List<InsulinInfusionGlucoseEntry> FilterTimeSpanAfterBolus(
            this List<InsulinInfusionGlucoseEntry>? glucoseEntries,
            List<InsulinBolusEntry>? insulinBoli,
            int excludeMinutesAfterBolus)
        {
            if (glucoseEntries == null) return [];
            if (excludeMinutesAfterBolus <= 0 || 
                insulinBoli == null || 
                insulinBoli.Count==0) return glucoseEntries.ToList();

            // Build exclusion intervals from boluses
            var excludeIntervals = insulinBoli
                .Select(entry => (
                    Start: entry.Time, 
                    End: entry.Time.AddMinutes(excludeMinutesAfterBolus)))
                .ToList();

            // Return glucoseEntries that are NOT inside any exclusion interval
            return glucoseEntries
                .Where(entry => !excludeIntervals
                    .Any(interval => 
                        interval.Start <= entry.Time && 
                        entry.Time < interval.End))
                .ToList();
        }
    }
}
