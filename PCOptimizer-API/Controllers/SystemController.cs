using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Management;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        [HttpGet("info")]
        public ActionResult<object> GetSystemInfo()
        {
            try
            {
                var osVersion = Environment.OSVersion.VersionString;
                var processorCount = Environment.ProcessorCount;
                var cpuModel = GetCpuModel();
                var totalRam = GetTotalRAM();
                var totalDisk = GetTotalDiskSpace();

                return Ok(new
                {
                    osName = "Windows",
                    osVersion = osVersion,
                    processorCount = processorCount,
                    cpuModel = cpuModel,
                    totalRam = totalRam,
                    totalDisk = totalDisk,
                    machineName = Environment.MachineName,
                    userName = Environment.UserName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("processes")]
        public ActionResult<object> GetProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(20)
                    .Select(p => new
                    {
                        pid = p.Id,
                        name = p.ProcessName,
                        memory = Math.Round(p.WorkingSet64 / (1024.0 * 1024.0), 2)
                    })
                    .ToList();

                return Ok(new
                {
                    count = Process.GetProcesses().Length,
                    topProcesses = processes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("disk-space")]
        public ActionResult<object> GetDiskSpace()
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                var driveInfo = drives.Select(d => new
                {
                    drive = d.Name,
                    total = Math.Round(d.TotalSize / (1024.0 * 1024.0 * 1024.0), 2),
                    free = Math.Round(d.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 2),
                    used = Math.Round((d.TotalSize - d.AvailableFreeSpace) / (1024.0 * 1024.0 * 1024.0), 2),
                    percent = Math.Round(((d.TotalSize - d.AvailableFreeSpace) / (double)d.TotalSize) * 100, 1)
                }).ToList();

                return Ok(new
                {
                    drives = driveInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("restart")]
        public ActionResult<object> RestartSystem()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 60 /c \"System restart initiated by PC Optimizer\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                return Ok(new
                {
                    success = true,
                    message = "System restart scheduled in 60 seconds"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("shutdown")]
        public ActionResult<object> ShutdownSystem()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/s /t 60 /c \"System shutdown initiated by PC Optimizer\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                return Ok(new
                {
                    success = true,
                    message = "System shutdown scheduled in 60 seconds"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetCpuModel()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private long GetTotalRAM()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (long.TryParse(obj["TotalVisibleMemorySize"]?.ToString() ?? "0", out var totalKB))
                        {
                            return totalKB * 1024; // Convert KB to bytes
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private long GetTotalDiskSpace()
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                return drives.Sum(d => d.TotalSize);
            }
            catch { }
            return 0;
        }
    }
}
