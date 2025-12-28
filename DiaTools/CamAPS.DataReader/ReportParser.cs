using System.Globalization;
using CamAPS.DataReader.Entries;

namespace CamAPS.DataReader;

public class ReportParser
{
    private const string DateFormat = "dd/MM/yyyy HH:mm";
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public ReportData Parse(string input)
    {
        var lines = input.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();
        return Parse(lines);
    }

    public ReportData Parse(IReadOnlyList<string> lines)
    {
        var report = new ReportData();
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

            var parts = SplitLine(line);

            if (HandlePrimingEventData(dataSection, parts, report)) continue;
            if (HandleSensorInsertedData(dataSection, parts, report)) continue;
            if (HandleSensorStoppedData(dataSection, parts, report)) continue;
            if (HandleRefillEventData(dataSection, parts, report)) continue;
            if (HandleAudioAlertsData(dataSection, parts, report)) continue;
            if (HandleVibrateAlertsData(dataSection, parts, report)) continue;

            if (HandleMealData(dataSection, parts, report)) continue;
            if (HandleInsulinBolusData(dataSection, parts, report)) continue;
            if (HandleInsulinInfusionData(dataSection, parts, report)) continue;
            if (HandleGlucoseConcentrationData(dataSection, parts, report)) continue;
            if (HandleFingerstickGlucoseConcentrationData(dataSection, parts, report)) continue;
        }

        return report;
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
        if (line.StartsWith("Audio_alerts")) return DataSection.AudioAlerts;
        if (line.StartsWith("Vibrate_alerts")) return DataSection.VibrateAlerts;


        if (line.EndsWith("*")) return DataSection.NotImplementedSection;

        return DataSection.None;
    }

    private bool HandleAudioAlertsData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.AudioAlerts) return false;
        var time = ParseDateTime(parts);
        reportData.AudioAlerts.Add(time);
        return true;
    }

    private static bool HandleFingerstickGlucoseConcentrationData(DataSection dataSection, string[] parts,
        ReportData reportData)
    {
        if (dataSection != DataSection.FingerstickGlucoseConcentration) return false;
        if (parts.Length == 4)
        {
            reportData.FingerstickGlucoseConcentrations.Add(new FingerstickGlucoseConcentrationEntry
            {
                Time = ParseDateTime(parts),
                MMolPerLitre = ParseDouble(parts[2])
            });
        }

        return true;
    }

    private static bool HandleGlucoseConcentrationData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.GlucoseConcentration) return false;

        if (parts.Length == 3)
        {
            reportData.GlucoseConcentrations.Add(new GlucoseConcentrationEntry
            {
                Time = ParseDateTime(parts),
                MMolPerLitre = ParseDouble(parts[2])
            });
        }

        return true;
    }

    private static bool HandleIdSection(DataSection dataSection, ReportData reportData, string line)
    {
        if (dataSection != DataSection.Id) return false;
        reportData.Id = line[3..].Trim();
        return true;
    }

    private static bool HandleInsulinBolusData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.InsulinBolus) return false;
        if (parts.Length == 4)
        {
            reportData.InsulinBoli.Add(new InsulinBolusEntry
            {
                Time = ParseDateTime(parts),
                Units = ParseDouble(parts[2])
            });
        }

        return true;
    }

    private static bool HandleInsulinInfusionData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.InsulinInfusion) return false;
        if (parts.Length == 3)
        {
            reportData.InsulinInfusions.Add(new InsulinInfusionEntry
            {
                Time = ParseDateTime(parts),
                UnitsPerHour = ParseDouble(parts[2])
            });
        }

        return true;
    }

    private static bool HandleMealData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.Meal) return false;
        if (parts.Length == 3)
        {
            reportData.Meals.Add(new MealEntry
            {
                Time = ParseDateTime(parts),
                ChoGrams = ParseDouble(parts[2])
            });
        }

        return true;
    }


    private static bool HandlePrimingEventData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.PrimingEvent) return false;
        var time = ParseDateTime(parts);
        reportData.PrimingEvents.Add(time);
        return true;
    }


    private static bool HandleRefillEventData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.RefillEvent) return false;
        var time = ParseDateTime(parts);
        reportData.RefillEvents.Add(time);
        return true;
    }

    private static bool HandleSensorInsertedData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.SensorInserted) return false;
        var time = ParseDateTime(parts);
        reportData.SensorInsertions.Add(time);
        return true;
    }

    private static bool HandleSensorStoppedData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.SensorStopped) return false;
        var time = ParseDateTime(parts);
        reportData.SensorStopps.Add(time);
        return true;
    }

    private static bool HandleVibrateAlertsData(DataSection dataSection, string[] parts, ReportData reportData)
    {
        if (dataSection != DataSection.VibrateAlerts) return false;
        var time = ParseDateTime(parts);
        reportData.VibrateAlerts.Add(time);
        return true;
    }

    private static DateTime ParseDateTime(string[] parts)
    {
        return DateTime.ParseExact(parts[0] + " " + parts[1], DateFormat, Culture);
    }

    private static double ParseDouble(string part)
    {
        return double.Parse(part, Culture);
    }

    private static string[] SplitLine(string line)
    {
        var parts = line
            .Split('\t', ' ')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
        return parts;
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
        SensorStopped,
        AudioAlerts,
        VibrateAlerts
    }
}