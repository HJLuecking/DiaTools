using System.Globalization;

namespace CamAPS.DataReader;

public class ReportParser
{
    private const string DateFormat = "dd/MM/yyyy HH:mm";
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;


    public Report Parse(string input)
    {
        var lines = input.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();
        return Parse(lines);
    }


    public Report Parse(IReadOnlyList<string> lines)
    {
        var report = new Report();
        var dataSection = DataSection.None;
        foreach (var l in lines)
        {
            var line = l.Trim();

            // Get section information
            var lineSection = CheckSection(line);

            // ID section is a one-liner
            if (HandleIdSection(lineSection, report, line)) continue;

            // Skipp column header lines
            if (lineSection == DataSection.ColumHeader) continue;

            // Handle data sections
            if (lineSection != DataSection.None)
            {
                dataSection = lineSection;
                continue;
            }

            if (HandleMealData(dataSection, line, report)) continue;
            if (HandlePrimingEventData(dataSection, line, report)) continue;
            if (HandleRefillEventData(dataSection, line, report)) continue;
            if (HandleInsulinBolusData(dataSection, line, report)) continue;
            if (HandleInsulinInfusionData(dataSection, line, report)) continue;
        }

        return report;
    }

    private bool HandleInsulinInfusionData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.InsulinInfusion) return false;
        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 3)
        {
            report.InsulinInfusions.Add(new InsulinInfusionEntry
            {
                Time = ParseExact(parts),
                UnitsPerHour = double.Parse(parts[1])
            });
        }

        return true;
    }

    private bool HandleInsulinBolusData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.InsulinBolus) return false;
        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 4)
        {
            report.InsulinBoli.Add(new InsulinBolusEntry
            {
                Time = ParseExact(parts),
                Units = double.Parse(parts[2])
            });
        }

        return true;
    }

    private static DateTime ParseExact(string[] parts)
    {
        return DateTime.ParseExact(parts[0] + " " + parts[1], DateFormat, Culture);
    }

    private static bool HandlePrimingEventData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.PrimingEvent) return false;
        if (TryParseExact(line, out var time)) report.PrimingEvents.Add(time);
        return true;
    }


    private static bool HandleRefillEventData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.RefillEvent) return false;
        if (TryParseExact(line, out var time)) report.RefillEvents.Add(time);
        return true;
    }

    private static bool HandleMealData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.Meal) return false;

        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 3)
        {
            report.Meals.Add(new MealEntry
            {
                Time = ParseExact(parts),
                ChoGrams = double.Parse(parts[2])
            });
        }

        return true;
    }

    private static bool HandleIdSection(DataSection dataSection, Report report, string line)
    {
        if (dataSection != DataSection.Id) return false;
        report.Id = line[3..].Trim();
        return true;
    }

    private static DataSection CheckSection(string line)
    {
        if (line.StartsWith("Time") || line.StartsWith("(dd/")) return DataSection.ColumHeader;
        if (line.StartsWith("ID:")) return DataSection.Id;
        if (line.StartsWith("Meal")) return DataSection.Meal;
        if (line.StartsWith("Priming_event")) return DataSection.PrimingEvent;
        if (line.StartsWith("Refill_event")) return DataSection.RefillEvent;
        if (line.StartsWith("Insulin_bolus")) return DataSection.InsulinBolus;
        return DataSection.None;
    }


    private static bool TryParseExact(string line, out DateTime time)
    {
        return DateTime.TryParseExact(line, DateFormat, Culture, DateTimeStyles.None, out time);
    }

    private enum DataSection
    {
        None,
        Id,
        ColumHeader,
        Meal,
        PrimingEvent,
        RefillEvent,
        InsulinBolus,
        InsulinInfusion
    }
}