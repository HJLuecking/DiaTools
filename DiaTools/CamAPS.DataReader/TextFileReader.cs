namespace CamAPS.DataReader
{
    public class TextFileReader
    {
        public IReadOnlyList<string> ReadLines(string fileName)
        {
            return File.ReadAllLines(fileName)
                       .Where(l => !string.IsNullOrWhiteSpace(l))
                       .ToList();
        }
    }
}
