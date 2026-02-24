using Asp.Versioning;
using Gateway.Framework.Core.Interfaces;
using Gateway.Framework.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Host.Controllers;

/// <summary>
/// Sample gateway controller demonstrating framework features.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<GatewayController> _logger;

    /// <summary>
    /// Initializes the gateway controller.
    /// </summary>
    public GatewayController(ICacheService cacheService, IAuditLogger auditLogger, ILogger<GatewayController> logger)
    {
        _cacheService = cacheService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    /// <summary>
    /// Health ping endpoint.
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(ApiResponse.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
    }

    /// <summary>
    /// Get gateway status (requires authentication).
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetStatus()
    {
        const string cacheKey = "gateway:status";
        var cached = await _cacheService.GetAsync<object>(cacheKey);
        if (cached != null)
        {
            return Ok(ApiResponse.Ok(cached, "From cache"));
        }

        var status = new
        {
            Name = "Banking Gateway",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Timestamp = DateTime.UtcNow
        };

        await _cacheService.SetAsync(cacheKey, status, TimeSpan.FromMinutes(5));
        await _auditLogger.LogAsync("GetStatus", User.Identity?.Name ?? "anonymous");

        return Ok(ApiResponse.Ok(status));
    }
}
