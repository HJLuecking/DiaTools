using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CamAPS.DataReader;
using Visualizer.ViewModels;

namespace Visualizer;
public partial class MainWindow : Window
{
    private bool _initialized;
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
                GlucosePlotView.Model = new OxyPlot.PlotModel { Title = "No data" };
                InfusionsPlotView.Model = new OxyPlot.PlotModel { Title = "No data" };
                BoliPlotView.Model = new OxyPlot.PlotModel { Title = "No data" };
                return;
            }

            var text = File.ReadAllText(candidate);
            var parser = new ReportParser();
            var report = parser.Parse(text);
            _currentReport = report;

            StatusText.Text = $"ID: {report.Id}  •  Glucose points: {report.GlucoseConcentrations.Count}  •  Infusions: {report.InsulinInfusions.Count}  •  Boli: {report.InsulinBoli.Count}";

            // Build and assign all three plots
            var gvm = new GlucoseViewModel(report);
            GlucosePlotView.Model = gvm.PlotModel;

            var ivm = new InfusionsViewModel(report);
            InfusionsPlotView.Model = ivm.PlotModel;

            var bvm = new BoliViewModel(report);
            BoliPlotView.Model = bvm.PlotModel;

            // Default: show Glucose tab
            PlotTabControl.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            GlucosePlotView.Model = new OxyPlot.PlotModel { Title = "Error" };
            InfusionsPlotView.Model = new OxyPlot.PlotModel { Title = "Error" };
            BoliPlotView.Model = new OxyPlot.PlotModel { Title = "Error" };
        }
    }
}