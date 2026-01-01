using System;
using System.ComponentModel;
using System.Linq;
using CamAPS.DataReader;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace Visualizer.ViewModels;
public enum DiagramType
{
    Glucose,
    Infusions,
    Boli
}

public class PlotWindowViewModel : INotifyPropertyChanged
{
    private const double MmolToMgDl = 18.0182;
    private PlotModel? _plotModel;

    public PlotModel? PlotModel
    {
        get => _plotModel;
        private set
        {
            if (_plotModel == value) return;
            _plotModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotModel)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PlotWindowViewModel(ReportData report, DiagramType type)
    {
        if (report is null) throw new ArgumentNullException(nameof(report));
        PlotModel = BuildFor(report, type);
    }

    private PlotModel BuildFor(ReportData report, DiagramType type)
    {
        return type switch
        {
            DiagramType.Glucose => BuildGlucosePlot(report),
            DiagramType.Infusions => BuildInfusionsPlot(report),
            DiagramType.Boli => BuildBoliPlot(report),
            _ => BuildCommonTimeModel("Diagram")
        };
    }

    private PlotModel BuildCommonTimeModel(string title)
    {
        var plot = new PlotModel { Title = title };

        // X axis: time-only display (HH:mm)
        var timeAxis = new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            StringFormat = "HH:mm",
            Title = "Time",
            IntervalType = DateTimeIntervalType.Hours,
            MinorIntervalType = DateTimeIntervalType.Minutes,
            IsZoomEnabled = true,
            IsPanEnabled = true
        };
        plot.Axes.Add(timeAxis);

        // Primary Y axis placeholder
        var yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Value"
        };
        plot.Axes.Add(yAxis);

        // Legend/margins
        var l = plot.Legends.FirstOrDefault();
        if (l == null)
        {
            l = new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopRight
            };
            plot.Legends.Add(l);
        }
        else
        {
            l.LegendPlacement = LegendPlacement.Outside;
            l.LegendPosition = LegendPosition.TopRight;
        }
        plot.Padding = new OxyThickness(8);

        return plot;
    }

    private PlotModel BuildGlucosePlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Glucose Points");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColors.Red,
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

    private PlotModel BuildInfusionsPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Insulin Infusions");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Star,
            MarkerSize = 3,
            MarkerFill = OxyColors.Blue,
            Title = "Insulin infusion (U/h)"
        };

        foreach (var i in report.InsulinInfusions.OrderBy(i => i.Time))
        {
            var t = baseDate.Date + i.Time.TimeOfDay;
            series.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(t), i.UnitsPerHour));
        }

        plot.Series.Add(series);
        return plot;
    }

    private PlotModel BuildBoliPlot(ReportData report)
    {
        var baseDate = new DateTime(2000, 1, 1);
        var plot = BuildCommonTimeModel("Insulin Boli");

        var series = new ScatterSeries
        {
            MarkerType = MarkerType.Diamond,
            MarkerSize = 4,
            MarkerFill = OxyColors.MediumVioletRed,
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