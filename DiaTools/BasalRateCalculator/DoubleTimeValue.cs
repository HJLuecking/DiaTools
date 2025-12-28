namespace BasalRateCalculator;

public class DoubleTimeValue(DateTime timestamp, double value)
{
    public DateTime Timestamp { get; set; } = timestamp;
    public double Value { get; set; } = value;
}