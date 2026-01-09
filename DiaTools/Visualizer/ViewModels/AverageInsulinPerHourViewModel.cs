using Diary.Aggregator;
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
            //MarkerType = MarkerType.Circle,
            //MarkerSize = 1,
            //MarkerFill = OxyPlot.OxyColors.Blue,
            Title = "Average insulin per hour"
        };

        var filteredData = report.InsulinInfusionWithNearestGlucose
            .FilterByMaximumTimeDiff(10)
            .FilterByMinimumAndMaximumGlucose(80, 150)
            .FilterTimeSpanAfterBolus(report.InsulinBoli, 0);

        var aggregatedData = InsulinByHourAggregator.AggregateInsulinInfusionByHour(filteredData, 3);

        foreach (var i in aggregatedData)
        {
            series.Points.Add(new DataPoint(i.Hour, i.AverageSumInsulinPerHourPerDay));
        }

        plot.Series.Add(series);
        return plot;
    }
}