using Diary.Aggregator;
using Diary.Aggregator.PatternMatching;
using Diary.Model;
using OxyPlot;
using OxyPlot.Series;

namespace Visualizer.ViewModels;
public class AverageInsulinPerHourViewModel : PlotViewModelBaseByHour
{
    public AverageInsulinPerHourViewModel(ReportData report)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildInfusionsPlot(report);
    }

    private PlotModel BuildInfusionsPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonHourModel("Average insulin per hour");

        var series = new StairStepSeries()
        {
            Title = "Average insulin per hour",
            LabelFormatString = "{1:0.0}"
        };

        //PatternMatchingBasalCalculator(report, series, plot);
        AggregateByDateAndHour(report, series, plot);
        return plot;
    }

    private static void PatternMatchingBasalCalculator(ReportData report, LineSeries series, PlotModel plot)
    {
        var filteredData = report.InsulinInfusionWithNearestGlucose 
            .FilterByGlucoseAndMinutes(240, 60)
            .FilterByMaximumTimeDiff(20)
            .FilterByMinimumAndMaximumGlucose(70, 240)
            .FilterTimeSpanAfterBolus(report.InsulinBoli, 240);

        var patternMatchingBasalCalculator = new PatternMatchingBasalCalculator();
        var dailyProfiles = patternMatchingBasalCalculator.ConvertToDailyProfiles(filteredData);
        var basalProfile = patternMatchingBasalCalculator.ComputeBasalProfile(dailyProfiles);
        series.Points.Add(new DataPoint(0, 0));
        foreach (var i in basalProfile)
        {
            series.Points.Add(new DataPoint(i.Key, i.Value));
        }
        plot.Series.Add(series);
    }

    private static void AggregateByDateAndHour(ReportData report, StairStepSeries series, PlotModel plot)
    {
        var filteredData = report.InsulinInfusionWithNearestGlucose
            .FilterByGlucoseAndMinutes(200,240)
            .FilterByMaximumTimeDiff(10)
            .FilterByMinimumAndMaximumGlucose(70, 200)
            .FilterTimeSpanAfterBolus(report.InsulinBoli, 0);

        var aggregatedData = InsulinAggregator.AggregateByDateAndHour(filteredData);

        series.Points.Add(new DataPoint(0, 0));
        foreach (var i in aggregatedData)
        {
            series.Points.Add(new DataPoint(i.Hour, i.InsulinUnits));
        }

        plot.Series.Add(series);
    }
}