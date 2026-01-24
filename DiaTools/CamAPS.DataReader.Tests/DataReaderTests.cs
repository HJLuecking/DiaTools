using AwesomeAssertions;
using Xunit.Abstractions;

namespace CamAPS.DataReader.Tests
{
    public class DataReaderTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IReadOnlyList<string> _lines;

        public DataReaderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var reader = new TextFileReader();
            _lines = reader.ReadLines("Data/camaps-data-20260115.txt");
        }

        [Fact]
        public void GivenCamApsFileItCanReadData()
        {
            var reportParser = new ReportParser();
            var actual = reportParser.Parse(_lines);
            _testOutputHelper.WriteLine(actual.LinesRead.ToString());
            actual.Id.Should().Be("h-j.luecking@t-online.de");
            actual.Meals.Count.Should().BeGreaterThan(0);
            actual.InsulinBoli.Count.Should().BeGreaterThan(0);
            actual.InsulinInfusions.Count.Should().BeGreaterThan(0);
            actual.GlucoseConcentrations.Count.Should().BeGreaterThan(0);
            //actual.SensorInsertions.Count.Should().BeGreaterThan(0);
            //actual.RefillEvents.Count.Should().BeGreaterThan(0);
            //actual.PrimingEvents.Count.Should().Be(1);
            //actual.SensorStops.Count.Should().Be(1);
            //actual.AudioAlerts.Count.Should().BeGreaterThan(0);
            //actual.VibrateAlerts.Count.Should().BeGreaterThan(0);
        }

    }
}
