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

        private void NavigateToDashboard(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new DashboardView { DataContext = DataContext });
            UpdateActiveButton(DashboardButton);
            PageTitleText.Text = "Dashboard";
            PageSubtitleText.Text = "Real-time system monitoring and performance metrics";
        }

        private void NavigateToOptimizer(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdvancedOptimizerView { DataContext = DataContext });
            UpdateActiveButton(OptimizerButton);
            PageTitleText.Text = "Advanced Optimizer";
            PageSubtitleText.Text = "Professional system optimization tools and tweaks";
        }

        private void NavigateToAnalytics(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PerformanceAnalyticsView { DataContext = DataContext });
            UpdateActiveButton(AnalyticsButton);
            PageTitleText.Text = "Performance Analytics";
            PageSubtitleText.Text = "Historical data visualization and anomaly detection";
        }

        private void NavigateToHistory(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new OperationHistoryLogView { DataContext = DataContext });
            UpdateActiveButton(HistoryButton);
            PageTitleText.Text = "Operation History Log";
            PageSubtitleText.Text = "Timeline of all system optimizations and changes";
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SettingsView { DataContext = DataContext });
            UpdateActiveButton(SettingsButton);
            PageTitleText.Text = "Settings";
            PageSubtitleText.Text = "Configure application behavior and preferences";
        }

        private void NavigateToAbout(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AboutView { DataContext = DataContext });
            UpdateActiveButton(AboutButton);
            PageTitleText.Text = "About";
            PageSubtitleText.Text = "Application information and system details";
        }

        private void UpdateActiveButton(Button activeButton)
        {
            // Reset all buttons
            DashboardButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            DashboardButton.BorderThickness = new Thickness(0);

            OptimizerButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            OptimizerButton.BorderThickness = new Thickness(0);

            AnalyticsButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            AnalyticsButton.BorderThickness = new Thickness(0);

            HistoryButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            HistoryButton.BorderThickness = new Thickness(0);

            SettingsButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            SettingsButton.BorderThickness = new Thickness(0);

            AboutButton.Foreground = FindResource("PrimaryTextBrush") as Brush;
            AboutButton.BorderThickness = new Thickness(0);

            // Highlight active button
            activeButton.Foreground = FindResource("GoldBrightBrush") as Brush;
            activeButton.BorderThickness = new Thickness(3, 0, 0, 0);
        }
    }
}
