namespace Diary.Aggregator;

public class ValuePerHour
{
    public int Hour { get; set; }           // hour 0–23
    public List<double> InsulinUnitsPerHour { get; set; } = [];
    public int CountInsulinUnitsPerHour => InsulinUnitsPerHour.Count;
    public double AverageSumInsulinPerHourPerDay => DaysUsed==0
            ?0.0
            :InsulinUnitsPerHour.Sum()/DaysUsed;
    public double AverageInsulinPerHourPerDay => DaysUsed == 0 
            ? 0.0 
            : InsulinUnitsPerHour.Count == 0 
                ? 0 
                : InsulinUnitsPerHour.Average()/DaysUsed;
    public int DaysUsed { get; set; }
    public double SmoothedAverageInsulinPerHourPerDay { get; set; }
}