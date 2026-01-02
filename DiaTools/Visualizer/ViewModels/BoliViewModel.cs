using System;
using BasalRateCalculator.Model;
using CamAPS.DataReader;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Visualizer.ViewModels;
public class BoliViewModel : PlotViewModelBase
{
    public BoliViewModel(ReportData report)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildBoliPlot(report);
    }

    private PlotModel BuildBoliPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Insulin Boli");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Diamond,
            MarkerSize = 4,
            MarkerFill = OxyPlot.OxyColors.MediumVioletRed,
            Title = "Insulin boli (U)"
        };

        foreach (var b in report.InsulinBoli)
        {
            var t = baseDate.Date + b.Time.TimeOfDay;
            series.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(t), b.Units));
        }

        plot.Series.Add(series);
        return plot;
    }
}