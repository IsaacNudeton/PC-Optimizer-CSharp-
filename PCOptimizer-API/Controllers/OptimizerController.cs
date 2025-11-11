using Microsoft.AspNetCore.Mvc;
using PCOptimizer.Services;

namespace PCOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizerController : ControllerBase
    {
        private readonly OptimizerService _optimizer;

        public OptimizerController(OptimizerService optimizer)
        {
            _optimizer = optimizer;
        }

        [HttpPost("gpu/nvidia")]
        public async Task<ActionResult<object>> OptimizeNvidiaGPU([FromBody] GpuOptimizeRequest request)
        {
            try
            {
                var result = await _optimizer.OptimizeNvidiaGPU(
                    request.EnableLowLatency ?? true,
                    request.MaxPerformance ?? true,
                    request.DisableVSync ?? true
                );

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    category = result.Category,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("gpu/amd")]
        public async Task<ActionResult<object>> OptimizeAMDGPU([FromBody] GpuOptimizeRequest request)
        {
            try
            {
                var result = await _optimizer.OptimizeAMDGPU(
                    request.MaxPerformance ?? true,
                    request.DisableVSync ?? true
                );

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    category = result.Category,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("memory/cleanup")]
        public async Task<ActionResult<object>> CleanupMemory()
        {
            try
            {
                var result = await _optimizer.ClearStandbyMemory();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("power-plan/gaming")]
        public async Task<ActionResult<object>> CreateGamingPowerPlan()
        {
            try
            {
                var result = await _optimizer.CreateGamingPowerPlan();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("background/kill")]
        public async Task<ActionResult<object>> KillBackgroundProcesses()
        {
            try
            {
                var result = await _optimizer.KillBackgroundProcesses();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("boot/optimize")]
        public async Task<ActionResult<object>> OptimizeBootSettings()
        {
            try
            {
                var result = await _optimizer.OptimizeBootSettings();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("network/advanced")]
        public async Task<ActionResult<object>> OptimizeNetworkAdvanced()
        {
            try
            {
                var result = await _optimizer.OptimizeNetworkAdvanced();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("display/optimize")]
        public async Task<ActionResult<object>> OptimizeDisplay()
        {
            try
            {
                var result = await _optimizer.OptimizeDisplay();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("audio/optimize")]
        public async Task<ActionResult<object>> OptimizeAudioLatency()
        {
            try
            {
                var result = await _optimizer.OptimizeAudioLatency();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    changes = result.Changes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("all")]
        public async Task<ActionResult<object>> ApplyAllOptimizations()
        {
            try
            {
                var results = await _optimizer.ApplyAllOptimizations();

                return Ok(new
                {
                    success = results.All(r => r.Success),
                    totalOptimizations = results.Count,
                    optimizations = results.Select(r => new
                    {
                        success = r.Success,
                        message = r.Message,
                        category = r.Category,
                        changes = r.Changes
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class GpuOptimizeRequest
    {
        public bool? EnableLowLatency { get; set; }
        public bool? MaxPerformance { get; set; }
        public bool? DisableVSync { get; set; }
    }
}
