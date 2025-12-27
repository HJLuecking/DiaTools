namespace CamAPS.DataReader;

public class InsulinBolusEntry
{
    public DateTime Time { get; set; }
    public double Units { get; set; }
    public int Minutes { get; set; }
}
public class InsulinInfusionEntry
{
    public DateTime Time { get; set; }
    public double UnitsPerHour { get; set; }
}