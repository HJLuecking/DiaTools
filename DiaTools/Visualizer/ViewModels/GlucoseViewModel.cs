using System;
using CamAPS.DataReader;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot;

namespace Visualizer.ViewModels;
public class GlucoseViewModel : PlotViewModelBase
{
    public GlucoseViewModel(ReportData report)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildGlucosePlot(report);
    }

    private PlotModel BuildGlucosePlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Glucose Points");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyPlot.OxyColors.Red,
            Title = "Glucose (mg/dl)"
        };

        foreach (var g in report.GlucoseConcentrations)
        {
            var timeOfDay = baseDate.Date + g.Time.TimeOfDay;
            var x = DateTimeAxis.ToDouble(timeOfDay);
            var y = g.MMolPerLitre * MmolToMgDl;
            if (double.IsNaN(y) || double.IsInfinity(y)) continue;
            series.Points.Add(new ScatterPoint(x, y));
        }

        plot.Series.Add(series);
        return plot;
    }
}