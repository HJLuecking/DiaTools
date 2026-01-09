using Diary.Filter;
using Diary.Model;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Visualizer.ViewModels;
public class FilteredInsulinInfusionViewModel : PlotViewModelBase
{
    public FilteredInsulinInfusionViewModel(ReportData report)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildInfusionsPlot(report);
    }

    private PlotModel BuildInfusionsPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Filtered Insulin Infusions");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 1,
            MarkerFill = OxyPlot.OxyColors.Blue,
            Title = "Insulin per hour"
        };

        var filteredData = report.InsulinInfusionWithNearestGlucose
            .FilterByMaximumTimeDiff(30)
            .FilterByMinimumAndMaximumGlucose(80, 150)
            .FilterTimeSpanAfterBolus(report.InsulinBoli, 240);

        foreach (var i in filteredData.OrderBy(i => i.Time))
        {
            var t = baseDate.Date + i.Time.TimeOfDay;
            series.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(t), i.InsulinUnitsPerHour));
        }

        plot.Series.Add(series);
        return plot;
    }
}