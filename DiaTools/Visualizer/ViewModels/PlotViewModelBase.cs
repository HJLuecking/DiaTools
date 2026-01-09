using Diary.Model;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
    using System;
using System.ComponentModel;
using System.Linq;

namespace Visualizer.ViewModels;
public abstract class PlotViewModelBase : INotifyPropertyChanged
{
    private PlotModel? _plotModel;

    public PlotModel? PlotModel
    {
        get => _plotModel;
        protected set
        {
            if (_plotModel == value) return;
            _plotModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlotModel)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected PlotModel BuildCommonTimeModel(string title)
    {
        var plot = new PlotModel { Title = title };

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

        var yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Value"
        };
        plot.Axes.Add(yAxis);

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
}