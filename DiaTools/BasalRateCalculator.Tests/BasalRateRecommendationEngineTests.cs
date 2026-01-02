using AwesomeAssertions;
using CamAPS.DataReader;
using Xunit.Abstractions;

namespace BasalRateCalculator.Tests
{
    public class BasalRateRecommendationEngineTests(ITestOutputHelper testOutputHelper)

    {
        [Fact]
        public void CalculateAverageHourlyBasalRate_UsingCamApsTestFile_ProducesHourlyAverages()
        {
            // Arrange
            var reader = new TextFileReader();
            var lines = reader.ReadLines("Data/camaps-data-20251226.txt");

            var parser = new ReportParser();
            var report = parser.Parse(lines);
            var glucoseValues = report.GlucoseConcentrations
                .Select(g => new DoubleTimeValue(g.Time, g.MgPerLitre))
                .ToList();

            var basalValues = report.InsulinInfusions
                .Select(b => new DoubleTimeValue(b.Time, b.UnitsPerHour))
                .ToList();

            var calculator = new BasalRateCalculator();
            var statistics = calculator.CollectBasalRateStatistics(glucoseValues, basalValues);
            var averagePerHour = statistics.AveragePerHour;
            var countPerHour = statistics.CountPerHour;
            var recommendationEngine = new BasalRateRecommendationEngine();

            // Act

            var recommendBasalRates = recommendationEngine.RecommendBasalRates(averagePerHour);
            // Assert
            recommendBasalRates.Keys.Count.Should().Be(24);
            recommendBasalRates.Values.Should().NotBeNull().And.Contain(x => x > 0); // Assert that at least one-hour has average > 0

            for (var i = 0; i < 24; i++)
            {
                var r = recommendBasalRates[i];
                var d = averagePerHour[i];
                var c = countPerHour[i];
                testOutputHelper.WriteLine($"{i} - {r:F1} - {d:F1} - {c}");
            }
            testOutputHelper.WriteLine("Sum: " + averagePerHour.Values.Sum(x => x).ToString("F1"));

        }
    }
}

