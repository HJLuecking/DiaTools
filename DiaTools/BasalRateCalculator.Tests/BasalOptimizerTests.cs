// Namespace der BasalOptimizer-Implementierung

using BasalRateCalculator.Model;
using CamAPS.DataReader;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BasalRateCalculator.Tests
{
    public class BasalOptimizerTests(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void ComputeOptimalBasal_NoBolus_StableGlucose_ReturnsBasalPerHour()
        {
            // Arrange
            var optimizer = new BasalOptimizer
            {
                GridStep = TimeSpan.FromMinutes(5),
                TargetMinMgDl = 90,
                TargetMaxMgDl = 130,
                ExcludeAfterBolus = TimeSpan.FromHours(3),
                MinValidFractionPerHour = 0.6,
                StabilityDeltaMgPerHour = 15,
                ApplySmoothing = true
            };


            var reader = new TextFileReader();
            var lines = reader.ReadLines("Data/camaps-data-20251226.txt");

            var parser = new ReportParser();
            var report = parser.Parse(lines);

            var start = new DateTime(2025, 9, 26, 0, 0, 0);
            var end = new DateTime(2025, 12, 25, 0, 0, 0);

            optimizer.AddBasalEntries(report.InsulinInfusions);
            optimizer.AddGlucoseEntries(report.GlucoseConcentrations);
            optimizer.AddBolusEntries(report.InsulinBoli);

            // Act
            optimizer.BuildUnifiedGrid(start, end);
            var result = optimizer.ComputeOptimalBasalPerHourOfDayFromGrid();

            // Assert
            for (var i = 0; i < 24; i++)
            {
                var d = result[i];
                testOutputHelper.WriteLine($"{i} - {d:F1}");
            }
            testOutputHelper.WriteLine("Sum: "+result.Values.Sum(x=>x).ToString("F1"));
        }
       
    }
}
