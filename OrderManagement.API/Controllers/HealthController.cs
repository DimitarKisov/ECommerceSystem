using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.API.Controllers
{
    /// <summary>
    /// Контролер за health checks и diagnostics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly OrderManagementDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            OrderManagementDbContext dbContext,
            ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Прост health check
        /// GET: api/health
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "healthy",
                service = "OrderManagement.API",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        /// <summary>
        /// Детайлен health check с проверка на зависимости
        /// GET: api/health/detailed
        /// </summary>
        [HttpGet("detailed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetDetailedHealth()
        {
            var health = new
            {
                status = "healthy",
                checks = new List<object>()
            };

            // Проверка на Database
            try
            {
                await _dbContext.Database.CanConnectAsync();
                health.checks.Add(new
                {
                    name = "Database",
                    status = "healthy",
                    description = "SQL Server connection successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                health.checks.Add(new
                {
                    name = "Database",
                    status = "unhealthy",
                    description = ex.Message
                });
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }

            // Можем да добавим още проверки (RabbitMQ, Redis и т.н.)

            return Ok(health);
        }

        /// <summary>
        /// Информация за версията на API
        /// GET: api/health/version
        /// </summary>
        [HttpGet("version")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetVersion()
        {
            var assembly = typeof(Program).Assembly;
            var version = assembly.GetName().Version;

            return Ok(new
            {
                service = "OrderManagement.API",
                version = version?.ToString() ?? "1.0.0",
                buildDate = System.IO.File.GetLastWriteTime(assembly.Location),
                dotnetVersion = Environment.Version.ToString()
            });
        }
    }
}