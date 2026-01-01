using System.Windows;
using CamAPS.DataReader;
using Visualizer.ViewModels;

namespace Visualizer;
public partial class GlucoseWindow : Window
{
    public GlucoseWindow(ReportData report)
    {
        InitializeComponent();
        DataContext = new GlucoseViewModel(report);
    }
}