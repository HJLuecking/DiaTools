    using System.Windows;
using CamAPS.DataReader;
using Visualizer.ViewModels;

namespace Visualizer;
public partial class BoliWindow : Window
{
    public BoliWindow(ReportData report)
    {
        InitializeComponent();
        DataContext = new BoliViewModel(report);
    }
}