using Microsoft.AspNetCore.Mvc;
using Kont.backend.Models.Dashboard;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();

            _logger.LogInformation("Retrieved dashboard stats: {TotalPools} pools, {ActivePools} active", stats.TotalPools, stats.ActivePools);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get real-time updates
    /// </summary>
    /// <returns>Recent activity updates</returns>
    /// <response code="200">Updates retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("realtime/updates")]
    [ProducesResponseType(typeof(RecentActivity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRealTimeUpdates()
    {
        try
        {
            var activity = await _dashboardService.GetRealTimeUpdatesAsync();
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving real-time updates");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

}

