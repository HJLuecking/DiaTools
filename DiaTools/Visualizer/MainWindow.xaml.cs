using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CamAPS.DataReader;

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

    private void GlucoseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var w = new GlucoseWindow(_currentReport) { Owner = this };
        w.Show();
    }

    private void InfusionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var w = new InfusionsWindow(_currentReport) { Owner = this };
        w.Show();
    }

    private void BoliButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_initialized || _currentReport == null) return;
        var w = new BoliWindow(_currentReport) { Owner = this };
        w.Show();
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
                return;
            }

            var text = File.ReadAllText(candidate);
            var parser = new ReportParser();
            var report = parser.Parse(text);
            _currentReport = report;

            StatusText.Text = $"ID: {report.Id}  •  Glucose points: {report.GlucoseConcentrations.Count}  •  Infusions: {report.InsulinInfusions.Count}  •  Boli: {report.InsulinBoli.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }
}