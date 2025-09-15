using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Scoring;
using PoolStats = Kont.backend.Controllers.PoolStats;

namespace Kont.backend.Services;

public interface IPoolsService
{
    Task<IEnumerable<Pool>> GetPoolsAsync();
    Task<Pool?> GetPoolByIdAsync(Guid id);
    Task<Pool> CreatePoolAsync(Pool pool);
    Task<Pool?> UpdatePoolAsync(Guid id, Pool pool);
    Task<bool> DeletePoolAsync(Guid id);
    Task<PoolStats?> GetPoolStatsAsync(Guid id);
    Task<bool> ValidateAllPlayersPresentAsync(Guid id);
    Task<bool> EndPoolAsync(Guid id);
    Task<IEnumerable<PlayerRegistration>> GetPoolPlayersAsync(Guid id);
    Task<string> GenerateRecoveryQRAsync(Guid poolId, Guid playerId);
}

public class PoolsService : IPoolsService
{
    private readonly IDatabaseContext _context;

    public PoolsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pool>> GetPoolsAsync()
    {
        return await _context.Pool
            .Include(p => p.Event)
            .Include(p => p.PlayerRegistrations)
            .Include(p => p.GameSessions)
            .ToListAsync();
    }

    public async Task<Pool?> GetPoolByIdAsync(Guid id)
    {
        return await _context.Pool
            .Include(p => p.Event)
            .Include(p => p.PlayerRegistrations)
            .Include(p => p.GameSessions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pool> CreatePoolAsync(Pool pool)
    {
        pool.Id = Guid.NewGuid();
        pool.CreatedAt = DateTime.UtcNow;
        pool.QrCode = GenerateQRCode(pool.Id);

        _context.Pool.Add(pool);
        await _context.SaveChangesAsync();

        return pool;
    }

    public async Task<Pool?> UpdatePoolAsync(Guid id, Pool pool)
    {
        var existingPool = await _context.Pool.FindAsync(id);
        if (existingPool == null)
        {
            return null;
        }

        existingPool.Name = pool.Name;
        existingPool.Description = pool.Description;
        existingPool.Event = pool.Event;
        existingPool.IsActive = pool.IsActive;
        existingPool.IsAllPlayersPresent = pool.IsAllPlayersPresent;

        await _context.SaveChangesAsync();

        return existingPool;
    }

    public async Task<bool> DeletePoolAsync(Guid id)
    {
        var pool = await _context.Pool.FindAsync(id);
        if (pool == null)
        {
            return false;
        }

        _context.Pool.Remove(pool);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PoolStats?> GetPoolStatsAsync(Guid id)
    {
        var pool = await _context.Pool
            .Include(p => p.PlayerRegistrations)
            .Include(p => p.GameSessions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pool == null)
        {
            return null;
        }

        return new PoolStats
        {
            TotalPlayers = pool.PlayerRegistrations.Count,
            RegisteredPlayers = pool.PlayerRegistrations.Count,
            CheckedInPlayers = pool.PlayerRegistrations.Count(p => p.CheckedInAt.HasValue),
            ActiveSessions = pool.GameSessions.Count(g => g.Status == GameSessionStatus.Active),
            CompletedSessions = pool.GameSessions.Count(g => g.Status == GameSessionStatus.Completed),
            TotalActivities = pool.GameSessions.Select(g => g.Activity).Distinct().Count()
        };
    }

    public async Task<bool> ValidateAllPlayersPresentAsync(Guid id)
    {
        var pool = await _context.Pool.FindAsync(id);
        if (pool == null)
        {
            return false;
        }

        pool.IsAllPlayersPresent = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EndPoolAsync(Guid id)
    {
        var pool = await _context.Pool.FindAsync(id);
        if (pool == null)
        {
            return false;
        }

        pool.Status = PoolStatus.Completed;
        pool.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PlayerRegistration>> GetPoolPlayersAsync(Guid id)
    {
        var pool = await _context.Pool
            .Include(p => p.PlayerRegistrations)
            .ThenInclude(pr => pr.Player)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pool == null)
        {
            return new List<PlayerRegistration>();
        }

        return pool.PlayerRegistrations;
    }

    public async Task<string> GenerateRecoveryQRAsync(Guid poolId, Guid playerId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            throw new ArgumentException("Pool not found");
        }

        var player = await _context.Player.FindAsync(playerId);
        if (player == null)
        {
            throw new ArgumentException("Player not found");
        }

        return GenerateRecoveryQRCode(poolId, playerId);
    }

    private string GenerateQRCode(Guid poolId)
    {
        return $"POOL_{poolId:N}";
    }

    private string GenerateRecoveryQRCode(Guid poolId, Guid playerId)
    {
        return $"RECOVERY_{poolId:N}_{playerId:N}";
    }
}
