using System.Windows.Controls;
using PCOptimizer.ViewModels;

namespace PCOptimizer.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            Loaded += DashboardView_Loaded;
        }

        private void DashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                // Subscribe to property changes to update the display
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.CpuUsage))
                    {
                        CpuValueText.Text = $"{viewModel.CpuUsage:F1}%";
                    }
                    else if (args.PropertyName == nameof(MainViewModel.GpuUsage))
                    {
                        GpuValueText.Text = $"{viewModel.GpuUsage:F1}%";
                    }
                    else if (args.PropertyName == nameof(MainViewModel.RamPercent))
                    {
                        RamValueText.Text = $"{viewModel.RamPercent:F1}%";
                    }
                    else if (args.PropertyName == nameof(MainViewModel.StatusMessage))
                    {
                        StatusText.Text = viewModel.StatusMessage;
                    }
                };
            }
        }
    }
}
