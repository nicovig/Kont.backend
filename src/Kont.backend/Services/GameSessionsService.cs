using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IGameSessionsService
{
    Task<IEnumerable<GameSession>> GetByEventAsync(Guid eventId);
    Task<GameSession?> CreateAsync(Guid eventId, Guid activityId);
    Task<GameSession?> UpdateStatusAsync(Guid id, GameSessionStatus status);
    Task<GameSession?> UpdateStartTimeAsync(Guid id, DateTime startedAt);
    Task<GameSession?> UpdateEndTimeAsync(Guid id, DateTime endedAt);
    Task<bool> DeleteAsync(Guid id);
}

public class GameSessionsService : IGameSessionsService
{
    private readonly IDatabaseContext _context;

    public GameSessionsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GameSession>> GetByEventAsync(Guid eventId)
    {
        return await _context.GameSession
            .Include(gs => gs.Pool)
            .Include(gs => gs.Activity)
            .Where(gs => gs.Pool.Event.Id == eventId)
            .ToListAsync();
    }

    public async Task<GameSession?> CreateAsync(Guid eventId, Guid activityId)
    {
        var ev = await _context.Event.Include(e => e.Pools).FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return null;
        var pool = ev.Pools.FirstOrDefault();
        if (pool == null) return null;
        var activity = await _context.Activity.FirstOrDefaultAsync(a => a.Id == activityId);
        if (activity == null) return null;

        var gs = new GameSession
        {
            Id = Guid.NewGuid(),
            Pool = pool,
            Activity = activity,
            Status = GameSessionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.GameSession.Add(gs);
        await _context.SaveChangesAsync();
        return gs;
    }

    public async Task<GameSession?> UpdateStatusAsync(Guid id, GameSessionStatus status)
    {
        var gs = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gs == null) return null;
        gs.Status = status;
        await _context.SaveChangesAsync();
        return gs;
    }

    public async Task<GameSession?> UpdateStartTimeAsync(Guid id, DateTime startedAt)
    {
        var gs = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gs == null) return null;
        gs.StartedAt = startedAt;
        await _context.SaveChangesAsync();
        return gs;
    }

    public async Task<GameSession?> UpdateEndTimeAsync(Guid id, DateTime endedAt)
    {
        var gs = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gs == null) return null;
        gs.EndedAt = endedAt;
        await _context.SaveChangesAsync();
        return gs;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var gs = await _context.GameSession.FindAsync(id);
        if (gs == null) return false;
        _context.GameSession.Remove(gs);
        await _context.SaveChangesAsync();
        return true;
    }
}


