using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CamAPS.DataReader;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Legends;

namespace Visualizer;
public partial class MainWindow : Window
{
    // conversion factor mmol/L -> mg/dL
    private const double MmolToMgDl = 18.0182;

    private readonly bool _initialized;
    private ReportData? _currentReport;

    public MainWindow()
    {
        InitializeComponent();
        _initialized = true;
        LoadAndRender();
    }

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        LoadAndRender();
    }

    private void GlucoseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var model = BuildGlucosePlot(_currentReport);
        PlotView.Model = model;
    }

    private void InfusionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var model = BuildInfusionsPlot(_currentReport);
        PlotView.Model = model;
    }

    private void BoliButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var model = BuildBoliPlot(_currentReport);
        PlotView.Model = model;
    }

    private void LoadAndRender()
    {
        try
        {
            // Try to locate data file relative to app base directory (Data\...)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidate = Path.Combine(baseDir, "Data", "camaps-data-20251226.txt");

            // fallback: relative to working dir
            if (!File.Exists(candidate))
            {
                candidate = Path.Combine(Environment.CurrentDirectory, "Data", "camaps-data-20251226.txt");
            }

            if (!File.Exists(candidate))
            {
                StatusText.Text = $"Data file not found: {candidate}";
                PlotView.Model = new PlotModel { Title = "No data" };
                return;
            }

            var text = File.ReadAllText(candidate);
            var parser = new ReportParser();
            var report = parser.Parse(text);
            _currentReport = report;

            StatusText.Text = $"ID: {report.Id}  •  Glucose points: {report.GlucoseConcentrations.Count}  •  Infusions: {report.InsulinInfusions.Count}  •  Boli: {report.InsulinBoli.Count}";

            // Default view: Glucose
            var model = BuildGlucosePlot(report);
            PlotView.Model = model;
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            PlotView.Model = new PlotModel { Title = "Error" };
        }
    }

    private PlotModel BuildCommonTimeModel(string title)
    {
        var plot = new PlotModel { Title = title };

        // X-axis: time-only display (HH:mm)
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

        // Improve default margins/legend
        var l = plot.Legends.FirstOrDefault();
        if (l == null)
        {
            l = new Legend();
            l.LegendPlacement = LegendPlacement.Outside;
            l.LegendPosition = LegendPosition.TopRight;
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
            MarkerType = MarkerType.Triangle,
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