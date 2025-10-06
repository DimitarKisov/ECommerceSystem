using Identity.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    /// <summary>
    /// Контролер за health checks и diagnostics на Identity API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IdentityDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            IdentityDbContext dbContext,
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
                service = "Identity.API",
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

            // Проверка на Identity Tables
            try
            {
                var userCount = await _dbContext.Users.CountAsync();
                health.checks.Add(new
                {
                    name = "IdentityTables",
                    status = "healthy",
                    description = $"Identity tables accessible. User count: {userCount}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Identity tables health check failed");
                health.checks.Add(new
                {
                    name = "IdentityTables",
                    status = "unhealthy",
                    description = ex.Message
                });
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }

            return Ok(health);
        }

        /// <summary>
        /// Информация за версията на Identity API
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
                service = "Identity.API",
                version = version?.ToString() ?? "1.0.0",
                buildDate = System.IO.File.GetLastWriteTime(assembly.Location),
                dotnetVersion = Environment.Version.ToString(),
                features = new[]
                {
                    "JWT Authentication",
                    "User Registration",
                    "Password Reset",
                    "Email Confirmation",
                    "Role Management"
                }
            });
        }
    }
}