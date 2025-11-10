using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using PCOptimizer.ViewModels;
using PCOptimizer.Views;

namespace PCOptimizer
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _clockTimer;

        // Cache views for instant navigation
        private DashboardView? _dashboardView;
        private AdvancedOptimizerView? _advancedOptimizerView;
        private PerformanceAnalyticsView? _performanceAnalyticsView;
        private OperationHistoryLogView? _operationHistoryLogView;
        private SettingsView? _settingsView;
        private AboutView? _aboutView;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Initialize clock timer
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => UpdateTime();
            _clockTimer.Start();

            // Navigate to Dashboard by default
            NavigateToDashboard(null, null);
        }

        private void UpdateTime()
        {
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss - ddd, MMM dd");
        }

        private void NavigateToDashboard(object? sender, RoutedEventArgs? e)
        {
            if (_dashboardView == null)
            {
                _dashboardView = new DashboardView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_dashboardView);
            UpdateActiveButton(DashboardButton);
            PageTitleText.Text = "Dashboard";
            PageSubtitleText.Text = "Real-time system monitoring and performance metrics";
        }

        private void NavigateToOptimizer(object sender, RoutedEventArgs e)
        {
            if (_advancedOptimizerView == null)
            {
                _advancedOptimizerView = new AdvancedOptimizerView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_advancedOptimizerView);
            UpdateActiveButton(OptimizerButton);
            PageTitleText.Text = "Advanced Optimizer";
            PageSubtitleText.Text = "Professional system optimization tools and tweaks";
        }

        private void NavigateToAnalytics(object sender, RoutedEventArgs e)
        {
            if (_performanceAnalyticsView == null)
            {
                _performanceAnalyticsView = new PerformanceAnalyticsView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_performanceAnalyticsView);
            UpdateActiveButton(AnalyticsButton);
            PageTitleText.Text = "Performance Analytics";
            PageSubtitleText.Text = "Historical data visualization and anomaly detection";
        }

        private void NavigateToHistory(object sender, RoutedEventArgs e)
        {
            if (_operationHistoryLogView == null)
            {
                _operationHistoryLogView = new OperationHistoryLogView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_operationHistoryLogView);
            UpdateActiveButton(HistoryButton);
            PageTitleText.Text = "Operation History Log";
            PageSubtitleText.Text = "Timeline of all system optimizations and changes";
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            if (_settingsView == null)
            {
                _settingsView = new SettingsView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_settingsView);
            UpdateActiveButton(SettingsButton);
            PageTitleText.Text = "Settings";
            PageSubtitleText.Text = "Configure application behavior and preferences";
        }

        private void NavigateToAbout(object sender, RoutedEventArgs e)
        {
            if (_aboutView == null)
            {
                _aboutView = new AboutView { DataContext = DataContext };
            }
            ContentFrame.Navigate(_aboutView);
            UpdateActiveButton(AboutButton);
            PageTitleText.Text = "About";
            PageSubtitleText.Text = "Application information and system details";
        }

        private void UpdateActiveButton(Button activeButton)
        {
            // Reset all buttons - only update border, let DynamicResource in XAML handle colors
            foreach (var button in new[] { DashboardButton, OptimizerButton, AnalyticsButton, HistoryButton, SettingsButton, AboutButton })
            {
                button.BorderThickness = new Thickness(0);
            }

            // Highlight active button with border (color comes from style/DynamicResource)
            activeButton.BorderThickness = new Thickness(3, 0, 0, 0);
        }
    }
}
