namespace BasalRateCalculator
{
    public class DoubleTimeValue(DateTime timestamp, double value)
    {
        public DateTime Timestamp { get; set; } = timestamp;
        public double Value { get; set; } = value;
    }


    public class TimeReference(DateTime start, DateTime end)
    {
        public DateTime Start { get; } = start;
        public DateTime End { get; } = end;
    }


}
