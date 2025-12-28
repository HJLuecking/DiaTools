namespace CamAPS.DataReader
{
    public class Report
    {
        public string Id { get; set; } = string.Empty;
        public int LinesRead { get; set; }
        public List<MealEntry> Meals { get; set; } = [];
        public List<DateTime> PrimingEvents { get; set; } = [];
        public List<DateTime> RefillEvents { get; set; } = [];
        public List<InsulinBolusEntry> InsulinBoli { get; set; } = [];
        public List<InsulinInfusionEntry> InsulinInfusions { get; set; } = [];
        public List<GlucoseConcentrationEntry> GlucoseConcentrations { get; set; } = [];
        public List<FingerstickGlucoseConcentrationEntry> FingerstickGlucoseConcentrations { get; set; } = [];
    }
}
