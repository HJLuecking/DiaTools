using AwesomeAssertions;

namespace CamAPS.DataReader.Tests
{
    public class TextFileReaderTests
    {
        [Fact]
        public void GivenAFilenameItCanReadData()
        {
            var reader = new TextFileReader();
            var actual = reader.ReadLines("Data/camaps-data-20260115.txt");
            actual.Count.Should().BeGreaterThan(0);
        }
    }
}
