using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using PCOptimizer.Services;

namespace PCOptimizer.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public void Execute(object? parameter) => _execute();
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PerformanceMonitor _monitor;
        private readonly OptimizerService _optimizer;
        private readonly DispatcherTimer _updateTimer;

        private float _cpuUsage;
        private float _gpuUsage;
        private double _ramUsedGB;
        private double _ramTotalGB;
        private double _ramPercent;
        private string _statusMessage = "Monitoring...";
        private bool _isOptimizing;
        private bool _isWindowActive = true;

        public float CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(nameof(CpuUsage)); }
        }

        public float GpuUsage
        {
            get => _gpuUsage;
            set { _gpuUsage = value; OnPropertyChanged(nameof(GpuUsage)); }
        }

        public double RamUsedGB
        {
            get => _ramUsedGB;
            set { _ramUsedGB = value; OnPropertyChanged(nameof(RamUsedGB)); }
        }

        public double RamTotalGB
        {
            get => _ramTotalGB;
            set { _ramTotalGB = value; OnPropertyChanged(nameof(RamTotalGB)); }
        }

        public double RamPercent
        {
            get => _ramPercent;
            set { _ramPercent = value; OnPropertyChanged(nameof(RamPercent)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public bool IsOptimizing
        {
            get => _isOptimizing;
            set { _isOptimizing = value; OnPropertyChanged(nameof(IsOptimizing)); }
        }

        public bool IsWindowActive
        {
            get => _isWindowActive;
            set
            {
                if (_isWindowActive != value)
                {
                    _isWindowActive = value;
                    OnPropertyChanged(nameof(IsWindowActive));
                    UpdateMonitoringMode();
                }
            }
        }

        public ICommand OptimizeAllCommand { get; }
        public ICommand OptimizeCpuCommand { get; }
        public ICommand OptimizeRamCommand { get; }
        public ICommand OptimizeGpuCommand { get; }
        public ICommand OptimizeNetworkCommand { get; }

        // Quick Action Commands (Dashboard)
        public ICommand GamingModeCommand { get; }
        public ICommand BalancedModeCommand { get; }
        public ICommand EcoModeCommand { get; }
        public ICommand DiskCleanupCommand { get; }
        public ICommand AdvancedTweaksCommand { get; }
        public ICommand SystemBackupCommand { get; }

        public MainViewModel()
        {
            _monitor = new PerformanceMonitor();
            _optimizer = new OptimizerService();

            // Initialize commands
            OptimizeAllCommand = new RelayCommand(async () => await OptimizeAll(), () => !IsOptimizing);
            OptimizeCpuCommand = new RelayCommand(async () => await OptimizeCpu(), () => !IsOptimizing);
            OptimizeRamCommand = new RelayCommand(async () => await OptimizeRam(), () => !IsOptimizing);
            OptimizeGpuCommand = new RelayCommand(async () => await OptimizeGpu(), () => !IsOptimizing);
            OptimizeNetworkCommand = new RelayCommand(async () => await OptimizeNetwork(), () => !IsOptimizing);

            // Initialize Quick Action commands
            GamingModeCommand = new RelayCommand(async () => await ApplyGamingMode(), () => !IsOptimizing);
            BalancedModeCommand = new RelayCommand(async () => await ApplyBalancedMode(), () => !IsOptimizing);
            EcoModeCommand = new RelayCommand(async () => await ApplyEcoMode(), () => !IsOptimizing);
            DiskCleanupCommand = new RelayCommand(async () => await RunDiskCleanup(), () => !IsOptimizing);
            AdvancedTweaksCommand = new RelayCommand(() => NavigateToAdvancedOptimizer());
            SystemBackupCommand = new RelayCommand(async () => await CreateBackup(), () => !IsOptimizing);

            // Update timer - starts at 2 seconds (Active mode)
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += UpdateMetrics;
            _updateTimer.Start();

            // Initial update
            UpdateMetrics(null, null);

            // Hook into application deactivation/activation for adaptive monitoring
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Activated += (s, e) => IsWindowActive = true;
                Application.Current.MainWindow.Deactivated += (s, e) => IsWindowActive = false;
            }
        }

        private void UpdateMonitoringMode()
        {
            if (IsWindowActive)
            {
                // Window is active: Full monitoring with 2-second updates
                _monitor.CurrentMode = MonitoringMode.Active;
                _updateTimer.Interval = TimeSpan.FromSeconds(2);
                StatusMessage = "Active monitoring";
            }
            else
            {
                // Window is in background: Reduced monitoring with 10-second updates
                _monitor.CurrentMode = MonitoringMode.Background;
                _updateTimer.Interval = TimeSpan.FromSeconds(10);
                StatusMessage = "Background monitoring (reduced CPU usage)";
            }
        }

        private void UpdateMetrics(object? sender, EventArgs? e)
        {
            try
            {
                var metrics = _monitor.GetMetrics();

                CpuUsage = metrics.CpuUsage;
                GpuUsage = metrics.GpuUsage;
                RamUsedGB = metrics.RamUsedGB;
                RamTotalGB = metrics.RamTotalGB;
                RamPercent = metrics.RamPercent;

                if (!IsOptimizing)
                {
                    StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task OptimizeAll()
        {
            IsOptimizing = true;
            StatusMessage = "Applying all optimizations...";

            try
            {
                var results = await _optimizer.ApplyAllOptimizations();
                var successCount = results.Count(r => r.Success);
                StatusMessage = $"Optimizations complete: {successCount}/{results.Count} successful";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Optimization error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task OptimizeCpu()
        {
            IsOptimizing = true;
            StatusMessage = "Optimizing CPU...";

            try
            {
                var result1 = await _optimizer.SetHighPerformancePowerPlan();
                var result2 = await _optimizer.DisableCoreParking();
                StatusMessage = $"CPU: {result1.Message} | {result2.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"CPU optimization error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task OptimizeRam()
        {
            IsOptimizing = true;
            StatusMessage = "Clearing RAM...";

            try
            {
                var result = await _optimizer.ClearStandbyMemory();
                StatusMessage = $"RAM: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"RAM optimization error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task OptimizeGpu()
        {
            IsOptimizing = true;
            StatusMessage = "Optimizing GPU...";

            try
            {
                var result = await _optimizer.OptimizeGPU();
                StatusMessage = $"GPU: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"GPU optimization error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task OptimizeNetwork()
        {
            IsOptimizing = true;
            StatusMessage = "Optimizing network...";

            try
            {
                var result = await _optimizer.OptimizeNetworkAdvanced();
                StatusMessage = $"Network: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Network optimization error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        // Quick Action Methods
        private async Task ApplyGamingMode()
        {
            IsOptimizing = true;
            StatusMessage = "Activating Gaming Mode...";

            try
            {
                var result = await _optimizer.CreateGamingPowerPlan();
                StatusMessage = $"Gaming Mode: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gaming Mode error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task ApplyBalancedMode()
        {
            IsOptimizing = true;
            StatusMessage = "Activating Balanced Mode...";

            try
            {
                var result = await _optimizer.RestoreBalancedPowerPlan();
                StatusMessage = $"Balanced Mode: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Balanced Mode error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task ApplyEcoMode()
        {
            IsOptimizing = true;
            StatusMessage = "Activating Eco Mode...";

            try
            {
                var result = await _optimizer.RestoreBalancedPowerPlan();
                StatusMessage = $"Eco Mode: {result.Message} (Balanced power settings)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eco Mode error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private async Task RunDiskCleanup()
        {
            IsOptimizing = true;
            StatusMessage = "Running disk cleanup...";

            try
            {
                var result = await _optimizer.CleanTempFiles();
                StatusMessage = $"Disk Cleanup: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Disk Cleanup error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        private void NavigateToAdvancedOptimizer()
        {
            // This will be handled by MainWindow navigation
            StatusMessage = "Navigate to Advanced Optimizer...";
            // We'll need to expose an event or use a messaging system for this
            // For now, just update status
        }

        private async Task CreateBackup()
        {
            IsOptimizing = true;
            StatusMessage = "Creating system restore point...";

            try
            {
                var result = await _optimizer.CreateSystemRestorePoint("PC Optimizer - Dashboard Backup");
                StatusMessage = $"Backup: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Backup error: {ex.Message}";
            }
            finally
            {
                IsOptimizing = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
