using System;
using System.Threading.Tasks;
using PCOptimizer.Services;

namespace BackendTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== PC OPTIMIZER BACKEND TEST ===\n");

            // Test Performance Monitor
            Console.WriteLine("Testing Performance Monitor...");
            var monitor = new PerformanceMonitor();

            for (int i = 0; i < 5; i++)
            {
                var metrics = monitor.GetMetrics();
                string cpuTemp = metrics.CpuTemp.HasValue ? $"{metrics.CpuTemp}°C" : "N/A";
                string gpuTemp = metrics.GpuTemp.HasValue ? $"{metrics.GpuTemp}°C" : "N/A";

                Console.WriteLine($"\n[{i + 1}] CPU: {metrics.CpuUsage:F1}% ({cpuTemp}) | GPU: {metrics.GpuUsage:F1}% ({gpuTemp}) | RAM: {metrics.RamUsedGB:F2}/{metrics.RamTotalGB:F2} GB ({metrics.RamPercent:F1}%)");
                await Task.Delay(2000);
            }

            monitor.Dispose();

            // Test Optimizer Service
            Console.WriteLine("\n\n=== Testing Optimizer Service ===");
            var optimizer = new OptimizerService();

            Console.WriteLine("\nClearing RAM...");
            var ramResult = await optimizer.ClearStandbyMemory();
            Console.WriteLine($"Result: {(ramResult.Success ? "SUCCESS" : "FAILED")} - {ramResult.Message}");

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
