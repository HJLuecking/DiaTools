using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System.ComponentModel;

namespace Visualizer.ViewModels;
public abstract class PlotViewModelBaseByHour : INotifyPropertyChanged
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

    protected PlotModel BuildCommonHourModel(string title)
    {
        var plot = new PlotModel { Title = title };

        var linearAxis = new LinearAxis
        {
            Title = "Hour",
            Position = AxisPosition.Bottom,
            Minimum = 0,
            Maximum = 24,
            MajorStep = 1,
            MinorStep = 1,
            StringFormat = "0",
            IsZoomEnabled = true,
            IsPanEnabled = true
        };

        plot.Axes.Add(linearAxis);

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