using AwesomeAssertions;
using CamAPS.DataReader;
using Xunit.Abstractions;

namespace BasalRateCalculator.Tests
{
    public class BasalRateCalculatorTests(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void CalculateAverageHourlyBasalRate_UsingCamApsTestFile_ProducesHourlyAverages()
        {
            // Arrange
            var reader = new TextFileReader();
            var lines = reader.ReadLines("Data/camaps-data-20251226.txt");

            var parser = new ReportParser();
            var report = parser.Parse(lines);

            // Convert GlucoseConcentrations (mmol/L) to DoubleTimeValue and Insulin boli to DoubleTimeValue
            var glucoseValues = report.GlucoseConcentrations
                .Select(g => new DoubleTimeValue(g.Time, g.MMolPerLitre))
                .ToList();

            var basalValues = report.InsulinInfusions
                .Select(b => new DoubleTimeValue(b.Time, b.UnitsPerHour))
                .ToList();

            var calculator = new BasalRateCalculator();

            // Act
            var result = calculator.CalculateAverageHourlyBasalRate(glucoseValues, basalValues);

            // Assert
            result.Keys.Count.Should().Be(24);
            result.Values.Should().NotBeNull().And.Contain(x => x > 0); // Assert that at least one-hour has average > 0

            foreach (var d in result)
            {
                testOutputHelper.WriteLine($"{d.Key} - {d.Value:F1}" );
            }
        } 
    }
}
