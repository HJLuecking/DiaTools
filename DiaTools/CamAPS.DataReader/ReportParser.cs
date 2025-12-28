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
            report.LinesRead++;
            var line = l.Trim();

            if (string.IsNullOrEmpty(line)) continue;

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
            if (HandleGlucoseConcentrationData(dataSection, line, report)) continue;
            if (HandleFingerstickGlucoseConcentrationData(dataSection, line, report)) continue;
            if (HandleSensorInsertedData(dataSection, line, report)) continue;
            if (HandleSensorStoppedData(dataSection, line, report)) continue;
            
        }

        return report;
    }

    private bool HandleSensorStoppedData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.SensorStopped) return false;
        if (TryParseExact(line, out var time)) report.SensorStoppedEvents.Add(time);
        return true;
    }

    private bool HandleSensorInsertedData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.SensorInserted) return false;
        if (TryParseExact(line, out var time)) report.SensorInsertedEvents.Add(time);
        return true;
    }

    private bool HandleFingerstickGlucoseConcentrationData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.FingerstickGlucoseConcentration) return false;

        // 26/10/2025 15:31	2.2203999999999997 c

        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 4)
        {
            report.FingerstickGlucoseConcentrations.Add(new FingerstickGlucoseConcentrationEntry
            {
                Time = ParseDateTime(parts),
                MMolPerLitre = ParseDouble(parts[2])
            });
        }

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
        if (line.StartsWith("Insulin_infusion")) return DataSection.InsulinInfusion;
        if (line.StartsWith("Glucose_concentration")) return DataSection.GlucoseConcentration;
        if (line.StartsWith("Fingerstick_glucose_concentration")) return DataSection.FingerstickGlucoseConcentration;
        if (line.StartsWith("Sensor_inserted")) return DataSection.SensorInserted;
        if (line.StartsWith("Sensor_stopped")) return DataSection.SensorStopped;

        


        if (line.EndsWith("*")) return DataSection.NotImplementedSection;

        return DataSection.None;
    }

    private static bool HandleIdSection(DataSection dataSection, Report report, string line)
    {
        if (dataSection != DataSection.Id) return false;
        report.Id = line[3..].Trim();
        return true;
    }

    private bool HandleGlucoseConcentrationData(DataSection dataSection, string line, Report report)
    {
        if (dataSection != DataSection.GlucoseConcentration) return false;
        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 3)
        {
            report.GlucoseConcentrations.Add(new GlucoseConcentrationEntry
            {
                Time = ParseDateTime(parts),
                MMolPerLitre = ParseDouble(parts[2])
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
                Time = ParseDateTime(parts),
                Units = ParseDouble(parts[2])
            });
        }

        return true;
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
                Time = ParseDateTime(parts),
                UnitsPerHour = ParseDouble(parts[2])
            });
        }

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
                Time = ParseDateTime(parts),
                ChoGrams = ParseDouble(parts[2])
            });
        }

        return true;
    }

    private static double ParseDouble(string part)
    {
        return double.Parse(part, Culture);
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

    private static DateTime ParseDateTime(string[] parts)
    {
        return DateTime.ParseExact(parts[0] + " " + parts[1], DateFormat, Culture);
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
        InsulinInfusion,
        NotImplementedSection,
        GlucoseConcentration,
        FingerstickGlucoseConcentration,
        SensorInserted,
        SensorStopped
    }
}

