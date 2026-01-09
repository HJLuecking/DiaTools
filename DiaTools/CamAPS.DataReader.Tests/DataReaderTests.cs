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
            _lines = reader.ReadLines("Data/camaps-data-20251226.txt");
        }

        [Fact]
        public void GivenCamApsFileItCanReadData()
        {
            var reportParser = new ReportParser();
            var actual = reportParser.Parse(_lines);
            _testOutputHelper.WriteLine(actual.LinesRead.ToString());
            actual.Id.Should().Be("h-j.luecking@t-online.de");
            var dateFormat = @"dd\/MM\/yyyy HH:mm";

            // 26/09/2025 09:15
            actual.Meals.Count.Should().BeGreaterThan(0);
            actual.Meals[0].ChoGrams.Should().Be(12);
            actual.Meals[0].Time.ToString(dateFormat).Should().Be("26/09/2025 09:15"); 

            // 26/09/2025 09:14
            actual.InsulinBoli.Count.Should().BeGreaterThan(0);
            actual.InsulinBoli[0].Units.Should().Be(3);
            actual.InsulinBoli[0].Time.ToString(dateFormat).Should().Be("26/09/2025 09:14"); 

            // 26/09/2025 00:02	13.199999
            actual.InsulinInfusions.Count.Should().BeGreaterThan(0);
            actual.InsulinInfusions[0].UnitsPerHour.Should().Be(13.199999);
            actual.InsulinInfusions[0].Time.ToString(dateFormat).Should().Be("26/09/2025 00:02");

            // 26/09/2025 00:04	6.383927549999999
            actual.GlucoseConcentrations.Count.Should().BeGreaterThan(0);
            actual.GlucoseConcentrations[0].MMolPerLitre.Should().Be(6.383927549999999);
            actual.GlucoseConcentrations[0].Time.ToString(dateFormat).Should().Be("26/09/2025 00:04");

            // 26/10/2025 15:31	2.2203999999999997 c
            actual.FingerstickGlucoseConcentrations.Count.Should().Be(3);
            actual.FingerstickGlucoseConcentrations[0].MMolPerLitre.Should().Be(2.2203999999999997);
            actual.FingerstickGlucoseConcentrations[0].Time.ToString(dateFormat).Should().Be("26/10/2025 15:31");

            // 26/09/2025 09:20
            actual.SensorInsertions.Count.Should().BeGreaterThan(0);
            actual.SensorInsertions[0].ToString(dateFormat).Should().Be("26/09/2025 09:20");

            // 26/09/2025 00:07
            actual.RefillEvents.Count.Should().BeGreaterThan(0);
            actual.RefillEvents[0].ToString(dateFormat).Should().Be("26/09/2025 00:07");

            // 26/09/2025 00:07
            actual.PrimingEvents.Count.Should().Be(1);
            actual.PrimingEvents[0].ToString(dateFormat).Should().Be("26/09/2025 00:07");

            // 26/09/2025 00:07
            actual.SensorStops.Count.Should().Be(1);
            actual.SensorStops[0].ToString(dateFormat).Should().Be("26/09/2025 00:07");

            // 26/09/2025 09:05
            actual.AudioAlerts.Count.Should().BeGreaterThan(0);
            actual.AudioAlerts[0].ToString(dateFormat).Should().Be("26/09/2025 09:05");

            //14/12/2025 19:09
            actual.VibrateAlerts.Count.Should().BeGreaterThan(0);
            actual.VibrateAlerts[0].ToString(dateFormat).Should().Be("14/12/2025 19:09");

        }

    }
}
