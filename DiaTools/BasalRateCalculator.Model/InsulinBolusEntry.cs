namespace BasalRateCalculator.Model;

public class InsulinBolusEntry
{
    public DateTime Time { get; set; }
    public double Units { get; set; }
    public int Minutes { get; set; }
}