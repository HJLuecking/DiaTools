namespace BasalRateCalculator;

public class BasalRateCalculator
{
    private const double MMolToMgDl = 18.0182;
    private const double MinGlucose = 90.0;
    private const double MaxGlucose = 150.0;

    /// <summary>
    /// Calculates the average hourly basal insulin rate based on provided glucose and insulin basal data.
    /// </summary>
    /// <param name="glucoseValuesMMol">A list of glucose measurements, where each value represents a glucose
    /// reading in mmol/L at a specific time.  Used to determine relevant time intervals for analysis.</param>
    /// <param name="insulinBasal">A list of insulin basal entries, where each value represents an insulin dose
    /// administered at a specific  time. Only basales within the time range defined by the glucose values are
    /// considered.</param>
    /// <returns>A dictionary mapping each hour of the day (0–23) to the average basal insulin rate for that hour.
    /// If no data is available for a given hour, that hour will not appear in the dictionary.</returns>
    public Dictionary<int, double> CalculateAverageHourlyBasalRate(
        List<DoubleTimeValue> glucoseValuesMMol,
        List<DoubleTimeValue> insulinBasal)
    {
        var filteredGlucose = FilterGlucoseValuesAsMgDl(glucoseValuesMMol);
        var timeReferences = ExtractTimeReferences(filteredGlucose);
        var filteredBasal = FilterBasalByGlucoseTimesInRange(insulinBasal, timeReferences);
            
        var sumPerHour = PrepareSumPerHour();
        var countPerHour = PrepareCountPerHour();
            
        CollectHourlyStatistics(filteredBasal, sumPerHour, countPerHour);
        var averagePerHour = ComputeAveragePerHour(sumPerHour, countPerHour);

        return averagePerHour;
    }

    /// <summary>
    /// Calculates the average value for each hour based on the provided sums and counts.
    /// </summary>
    /// <param name="sumPerHour">A dictionary mapping each hour to the total sum of values recorded for that hour.</param>
    /// <param name="countPerHour">A dictionary mapping each hour to the count of values recorded for that hour.
    /// Each key should correspond to a key in <paramref name="sumPerHour"/>.</param>
    /// <returns>A dictionary mapping each hour to the average value for that hour.
    /// If the count for an hour is zero, the average is 0.0.</returns>
    private static Dictionary<int, double> ComputeAveragePerHour(Dictionary<int, double> sumPerHour, Dictionary<int, int> countPerHour)
    {
        var averagePerHour = new Dictionary<int, double>();
        foreach (var hour in sumPerHour.Keys)
        {
            averagePerHour[hour] = countPerHour[hour] > 0
                ? sumPerHour[hour] / countPerHour[hour]
                : 0.0;
        }
        return averagePerHour;
    }

    private static void CollectHourlyStatistics(List<DoubleTimeValue> filteredBasal, Dictionary<int, double> sumPerHour, Dictionary<int, int> countPerHour)
    {
        foreach (var basal in filteredBasal)
        {
            var hour = basal.Timestamp.Hour;
            sumPerHour[hour] += basal.Value;
            countPerHour[hour]++;
        }
    }

    private static Dictionary<int, int> PrepareCountPerHour()
    {
        var countPerHour = Enumerable.Range(0, 24)
            .ToDictionary(h => h, h => 0);
        return countPerHour;
    }

    /// <summary>
    /// Initializes a dictionary that maps each hour of the day to a sum value of zero.
    /// </summary>
    /// <returns>A dictionary with keys representing each hour from 0 to 23 and values initialized to 0.0.</returns>
    private static Dictionary<int, double> PrepareSumPerHour()
    {
        var sumPerHour = Enumerable.Range(0, 24)
            .ToDictionary(h => h, h => 0.0);
        return sumPerHour;
    }

    private static List<DoubleTimeValue> FilterBasalByGlucoseTimesInRange(List<DoubleTimeValue> insulinBasal, List<TimeReference> timeReferences)
    {
        var filteredBasal = insulinBasal
            .Where(b => timeReferences.Any(tr =>
                b.Timestamp >= tr.Start &&
                b.Timestamp <= tr.End))
            .ToList();
        return filteredBasal;
    }

    /// <summary>
    /// Convert mmol/L glucose values to mg/dL, filter by acceptable range and sort by timestamp.
    /// </summary>
    /// <param name="glucoseValuesMMol"></param>
    /// <returns></returns>
    private static List<DoubleTimeValue> FilterGlucoseValuesAsMgDl(List<DoubleTimeValue> glucoseValuesMMol)
    {
        var filteredGlucose = glucoseValuesMMol
            .Select(g => new DoubleTimeValue(
                g.Timestamp,
                g.Value * MMolToMgDl))
            .Where(g => g.Value is >= MinGlucose and <= MaxGlucose)
            .OrderBy(g => g.Timestamp)
            .ToList();
        return filteredGlucose;
    }

    /// <summary>
    /// Identifies and extracts contiguous time intervals from a list of timestamped values where each interval
    /// spans more than one hour and consecutive timestamps are no more than x minutes apart.
    /// </summary>
    /// <remarks>Intervals are formed by grouping consecutive values where the gap between timestamps
    /// does not exceed 10 minutes. Only intervals with a total duration greater than one hour are included in
    /// the result.</remarks>
    /// <param name="values">The ordered list of timestamped values to analyze for contiguous time intervals.
    /// Must not be null.</param>
    /// <returns>A list of time references representing intervals longer than one hour, where the values are
    /// closely spaced in time. Returns an empty list if no such intervals are found.</returns>
    private List<TimeReference> ExtractTimeReferences(List<DoubleTimeValue> values)
    {
        var references = new List<TimeReference>();
        const int minutesBetween = 10;

        if (!values.Any())
            return references;

        var start = values[0].Timestamp;
        var last = start;

        for (var i = 1; i < values.Count; i++)
        {
            var current = values[i];

            if ((current.Timestamp - last).TotalMinutes > minutesBetween)
            {
                if ((last - start).TotalHours > 1)
                {
                    references.Add(new TimeReference(start, last));
                }
                start = current.Timestamp;
            }

            last = current.Timestamp;
        }

        if ((last - start).TotalHours > 1)
        {
            references.Add(new TimeReference(start, last));
        }

        return references;
    }
}