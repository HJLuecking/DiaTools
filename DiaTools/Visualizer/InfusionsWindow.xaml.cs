                using System.Windows;
using CamAPS.DataReader;
using Visualizer.ViewModels;

namespace Visualizer;
public partial class InfusionsWindow : Window
{
    public InfusionsWindow(ReportData report)
    {
        InitializeComponent();
        DataContext = new PlotWindowViewModel(report, DiagramType.Infusions);
    }
}