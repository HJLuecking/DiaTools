namespace Diary.Aggregator.PatternMatching;

/// <summary>
/// Represents one day of data (24 hours)
/// </summary>
public class DailyProfile
{
    public DateTime Date { get; set; }
    public List<HourlyData> Hours { get; set; } = [];
}