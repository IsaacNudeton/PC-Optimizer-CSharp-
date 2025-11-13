using PCOptimizer.Services;

namespace PCOptimizer_API.Services
{
    /// <summary>
    /// Background service that continuously monitors system activity and collects data
    /// </summary>
    public class MonitoringBackgroundService : BackgroundService
    {
        private readonly ILogger<MonitoringBackgroundService> _logger;
        private readonly BehaviorMonitor _behaviorMonitor;
        private readonly PerformanceMonitor _performanceMonitor;
        private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(5); // Capture every 5 seconds

        public MonitoringBackgroundService(
            ILogger<MonitoringBackgroundService> logger,
            BehaviorMonitor behaviorMonitor,
            PerformanceMonitor performanceMonitor)
        {
            _logger = logger;
            _behaviorMonitor = behaviorMonitor;
            _performanceMonitor = performanceMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🎯 Monitoring Background Service started - collecting activity data every {Interval} seconds", _monitoringInterval.TotalSeconds);

            // Ensure PerformanceMonitor is in Active mode
            _performanceMonitor.CurrentMode = MonitoringMode.Active;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Capture current activity snapshot (includes processes, windows, resources)
                    var snapshot = _behaviorMonitor.CaptureSnapshot();
                    
                    _logger.LogDebug("📸 Snapshot captured - Category: {Category}, Processes: {ProcessCount}, Active: {ActiveWindow}", 
                        snapshot.Category, 
                        snapshot.RunningProcesses.Count,
                        snapshot.ActiveWindow?.WindowTitle ?? "None");

                    // Wait for next interval
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error capturing activity snapshot");
                    // Continue monitoring even if one snapshot fails
                    await Task.Delay(_monitoringInterval, stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Monitoring Background Service stopped");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping monitoring service gracefully...");
            await base.StopAsync(stoppingToken);
        }
    }
}
