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
            actual.Id.Should().NotBeNullOrWhiteSpace().And.Be("h-j.luecking@t-online.de");

            // 26/09/2025 09:15
            actual.Meals.Count.Should().BeGreaterThan(0);
            actual.Meals[0].ChoGrams.Should().Be(12);
            actual.Meals[0].Time.ToString("ddMMyy-HHmm").Should().Be("260925-0915"); 

            // 26/09/2025 09:14
            actual.InsulinBoli.Count.Should().BeGreaterThan(0);
            actual.InsulinBoli[0].Units.Should().Be(3);
            actual.InsulinBoli[0].Time.ToString("ddMMyy-HHmm").Should().Be("260925-0914"); 

            // 26/09/2025 00:02	13.199999
            actual.InsulinInfusions.Count.Should().BeGreaterThan(0);
            actual.InsulinInfusions[0].UnitsPerHour.Should().Be(13.199999);
            actual.InsulinInfusions[0].Time.ToString("ddMMyy-HHmm").Should().Be("260925-0002");

            // 26/09/2025 00:04	6.383927549999999
            actual.GlucoseConcentrations.Count.Should().BeGreaterThan(0);
            actual.GlucoseConcentrations[0].MMolPerLitre.Should().Be(6.383927549999999);
            actual.GlucoseConcentrations[0].Time.ToString("ddMMyy-HHmm").Should().Be("260925-0004");

        }

    }
}
