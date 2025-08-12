using Microsoft.AspNetCore.Mvc;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint for monitoring
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested");
            
            var healthStatus = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                uptime = Environment.TickCount64
            };

            return Ok(healthStatus);
        }

        /// <summary>
        /// Detailed health check with system information
        /// </summary>
        /// <returns>Detailed health status</returns>
        [HttpGet("detailed")]
        public IActionResult GetDetailed()
        {
            _logger.LogInformation("Detailed health check requested");
            
            var detailedHealth = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                uptime = Environment.TickCount64,
                system = new
                {
                    os = Environment.OSVersion.ToString(),
                    processorCount = Environment.ProcessorCount,
                    workingSet = Environment.WorkingSet,
                    machineName = Environment.MachineName
                },
                services = new
                {
                    userService = "operational",
                    database = "operational"
                }
            };

            return Ok(detailedHealth);
        }
    }
}
