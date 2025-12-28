namespace BasalRateCalculator
{
    public class TimeReference(DateTime start, DateTime end)
    {
        public DateTime Start { get; } = start;
        public DateTime End { get; } = end;
    }


}
