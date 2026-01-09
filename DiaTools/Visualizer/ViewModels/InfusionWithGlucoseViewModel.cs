using Diary.Model;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Visualizer.ViewModels;
public class InfusionWithGlucoseViewModel : PlotViewModelBase
{
    public InfusionWithGlucoseViewModel(ReportData report)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildInfusionsPlot(report);
    }

    private PlotModel BuildInfusionsPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Insulin Infusions with Glucose");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 1,
            MarkerFill = OxyPlot.OxyColors.Blue,
            Title = "Insulin infusion (U/h)"
        };

        foreach (var i in report.InsulinInfusionWithNearestGlucose.OrderBy(i => i.Time))
        {
            var t = baseDate.Date + i.Time.TimeOfDay;
            series.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(t), i.GlucoseMgPerLitre));
        }

        plot.Series.Add(series);
        return plot;
    }
}