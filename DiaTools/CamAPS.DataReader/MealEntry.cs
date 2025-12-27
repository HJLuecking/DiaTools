namespace CamAPS.DataReader
{
    public class MealEntry
    {
        public DateTime Time { get; set; }
        public double ChoGrams { get; set; }
    }

    public class GlucoseConcentrationEntry
    {
        public DateTime Time { get; set; }
        public double MMolPerLitre { get; set; }
    }
}
