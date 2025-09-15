using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<RecentActivity> GetRealTimeUpdatesAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IDatabaseContext _context;

    public DashboardService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalPools = await _context.Pool.CountAsync();
        var activePools = await _context.Pool.CountAsync(p => p.Status == PoolStatus.Active);
        var totalPlayers = await _context.PlayerRegistration.CountAsync();
        var totalActivities = await _context.Activity.CountAsync();

        var recentActivity = await GetRecentActivityAsync();

        return new DashboardStats
        {
            TotalPools = totalPools,
            ActivePools = activePools,
            TotalPlayers = totalPlayers,
            TotalActivities = totalActivities,
            RecentActivity = recentActivity
        };
    }

    public async Task<RecentActivity> GetRealTimeUpdatesAsync()
    {
        var recentActivity = await GetRecentActivityAsync();
        var latestActivity = recentActivity.FirstOrDefault();

        if (latestActivity == null)
        {
            return new RecentActivity
            {
                Id = Guid.NewGuid().ToString(),
                Type = "no_activity",
                Message = "Aucune activité récente",
                Timestamp = DateTime.UtcNow
            };
        }

        return latestActivity;
    }

    private async Task<List<RecentActivity>> GetRecentActivityAsync()
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
