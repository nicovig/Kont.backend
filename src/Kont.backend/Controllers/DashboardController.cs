using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDatabaseContext context,
        ILogger<DashboardController> logger)
    {
        _context = context;
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
            var totalPools = await _context.Pool.CountAsync();
            var activePools = await _context.Pool.CountAsync(p => p.Status == PoolStatus.Active);
            var totalPlayers = await _context.PlayerRegistration.CountAsync();
            var totalActivities = await _context.Activity.CountAsync();

            var recentActivity = await GetRecentActivity();

            var stats = new DashboardStats
            {
                TotalPools = totalPools,
                ActivePools = activePools,
                TotalPlayers = totalPlayers,
                TotalActivities = totalActivities,
                RecentActivity = recentActivity
            };

            _logger.LogInformation("Retrieved dashboard stats: {TotalPools} pools, {ActivePools} active", totalPools, activePools);
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
            var recentActivity = await GetRecentActivity();
            var latestActivity = recentActivity.FirstOrDefault();

            if (latestActivity == null)
            {
                return Ok(new RecentActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "no_activity",
                    Message = "Aucune activité récente",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(latestActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving real-time updates");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private async Task<List<RecentActivity>> GetRecentActivity()
    {
        var activities = new List<RecentActivity>();

        // Get recent pool creations
        var recentPools = await _context.Pool
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var pool in recentPools)
        {
            activities.Add(new RecentActivity
            {
                Id = pool.Id.ToString(),
                Type = "pool_created",
                Message = $"Nouvelle pool créée: {pool.Name}",
                Timestamp = pool.CreatedAt,
                PoolId = pool.Id.ToString()
            });
        }

        // Get recent player registrations
        var recentRegistrations = await _context.PlayerRegistration
            .Include(pr => pr.Player)
            .Include(pr => pr.Pool)
            .OrderByDescending(pr => pr.RegisteredAt)
            .Take(5)
            .ToListAsync();

        foreach (var registration in recentRegistrations)
        {
            activities.Add(new RecentActivity
            {
                Id = registration.Id.ToString(),
                Type = "player_registered",
                Message = $"{registration.Player.Username} s'est inscrit à {registration.Pool.Name}",
                Timestamp = registration.RegisteredAt,
                PoolId = registration.Pool.Id.ToString(),
                PlayerId = registration.Player.Id.ToString()
            });
        }

        // Get recent game session activities
        var recentSessions = await _context.GameSession
            .Include(gs => gs.Pool)
            .Include(gs => gs.Activity)
            .Where(gs => gs.Status == GameSessionStatus.Completed)
            .OrderByDescending(gs => gs.EndedAt)
            .Take(5)
            .ToListAsync();

        foreach (var session in recentSessions)
        {
            activities.Add(new RecentActivity
            {
                Id = session.Id.ToString(),
                Type = "session_completed",
                Message = $"Session {session.Activity.Name} terminée pour {session.Pool.Name}",
                Timestamp = session.EndedAt ?? session.CreatedAt,
                PoolId = session.Pool.Id.ToString()
            });
        }

        return activities
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();
    }
}

public class DashboardStats
{
    public int TotalPools { get; set; }
    public int ActivePools { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalActivities { get; set; }
    public List<RecentActivity> RecentActivity { get; set; } = new List<RecentActivity>();
}

public class RecentActivity
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? PoolId { get; set; }
    public string? PlayerId { get; set; }
}
