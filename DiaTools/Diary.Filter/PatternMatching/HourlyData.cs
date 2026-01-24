namespace Diary.Aggregator.PatternMatching;

/// <summary>
/// Represents one hourly data point
/// </summary>
public class HourlyData
{
    public int Hour { get; set; }              // 0–23
    public double BasalUnits { get; set; }     // Insulin units per hour (E/h)
    public double Glucose { get; set; }        // Glucose in mg/dL
}