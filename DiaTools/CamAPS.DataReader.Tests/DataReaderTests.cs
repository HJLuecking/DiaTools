using AwesomeAssertions;

namespace CamAPS.DataReader.Tests
{
    public class DataReaderTests
    {
        private readonly IReadOnlyList<string> _lines;

        public DataReaderTests()
        {
            var reader = new TextFileReader();
            _lines = reader.ReadLines("Data/camaps-data-20251226.txt");
        }

        [Fact]
        public void GivenCamApsFileItCanReadData()
        {
            var reportParser = new ReportParser();
            var actual = reportParser.Parse(_lines);
            actual.Id.Should().NotBeNullOrWhiteSpace().And.Be("h-j.luecking@t-online.de");
            actual.Meals.Count.Should().BeGreaterThan(0);
        }
    }
}
