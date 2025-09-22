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
    Task<IEnumerable<PlayerGroup>?> GenerateGroupsWithoutScoresAsync(Guid gameSessionId);
}

public class GameSessionsService : IGameSessionsService
{
    private readonly IDatabaseContext _context;

    public GameSessionsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GameSession>> GetByEventAsync(Guid eventId) => await _context.GameSession
            .Include(gs => gs.Pool)
            .Include(gs => gs.Activity)
            .Where(gs => gs.Pool.Event.Id == eventId)
            .ToListAsync();
    

    public async Task<GameSession?> CreateAsync(Guid eventId, Guid activityId)
    {
        var eventDb = await _context.Event.Include(e => e.Pools).FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventDb == null) return null;
        var pool = eventDb.Pools.FirstOrDefault();
        if (pool == null) return null;
        var activity = await _context.Activity.FirstOrDefaultAsync(a => a.Id == activityId);
        if (activity == null) return null;

        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            Pool = pool,
            Activity = activity,
            Status = GameSessionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _context.GameSession.Add(gameSession);
        await _context.SaveChangesAsync();
        return gameSession;
    }

    public async Task<GameSession?> UpdateStatusAsync(Guid id, GameSessionStatus status)
    {
        var gameSession = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gameSession == null) return null;
        gameSession.Status = status;
        await _context.SaveChangesAsync();
        return gameSession;
    }

    public async Task<GameSession?> UpdateStartTimeAsync(Guid id, DateTime startedAt)
    {
        var gameSession = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gameSession == null) return null;
        gameSession.StartedAt = startedAt;
        await _context.SaveChangesAsync();
        return gameSession;
    }

    public async Task<GameSession?> UpdateEndTimeAsync(Guid id, DateTime endedAt)
    {
        var gameSession = await _context.GameSession.Include(x => x.Pool).Include(x => x.Activity).FirstOrDefaultAsync(x => x.Id == id);
        if (gameSession == null) return null;
        gameSession.EndedAt = endedAt;
        await _context.SaveChangesAsync();
        return gameSession;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var gameSession = await _context.GameSession.FindAsync(id);
        if (gameSession == null) return false;
        _context.GameSession.Remove(gameSession);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PlayerGroup>?> GenerateGroupsWithoutScoresAsync(Guid gameSessionId)
    {
        var gameSession = await LoadGameSessionGraphAsync(gameSessionId);
        if (gameSession == null) return null;
        var limit = gameSession.Activity.PlayersPerGroupLimit;
        if (limit <= 0) return Array.Empty<PlayerGroup>();
        await ResetExistingGroupsAsync(gameSession);
        var regs = GetOrderedUniqueRegistrations(gameSession);
        var groups = BuildGroups(gameSession, regs, limit);
        _context.PlayerGroup.AddRange(groups);
        await _context.SaveChangesAsync();
        return groups;
    }




    private async Task ResetExistingGroupsAsync(GameSession gameSession)
    {
        if (!gameSession.PlayerGroups.Any()) return;
        _context.PlayerGroup.RemoveRange(gameSession.PlayerGroups);
        await _context.SaveChangesAsync();
        _context.Entry(gameSession).Collection(g => g.PlayerGroups).Load();
    }    

    private List<PlayerGroup> BuildGroups(GameSession gameSession, List<PlayerRegistration> playerRegistrations, int limit)
    {
        var groups = new List<PlayerGroup>();
        var index = 0;
        var groupNumber = 1;
        while (index < playerRegistrations.Count)
        {
            var take = Math.Min(limit, playerRegistrations.Count - index);
            var slice = playerRegistrations.GetRange(index, take);
            var playerGroup = new PlayerGroup
            {
                Id = Guid.NewGuid(),
                GameSession = gameSession,
                Players = slice,
                GroupNumber = groupNumber,
                CreatedAt = DateTime.UtcNow
            };
            groups.Add(playerGroup);
            index += take;
            groupNumber += 1;
        }
        return groups;
    }
    
    private async Task<GameSession?> LoadGameSessionGraphAsync(Guid gameSessionId) => await _context.GameSession
            .Include(x => x.Activity)
            .Include(x => x.Pool)
                .ThenInclude(p => p.PlayerRegistrations)
            .Include(x => x.PlayerGroups)
                .ThenInclude(pg => pg.Players)
            .FirstOrDefaultAsync(x => x.Id == gameSessionId);

    private List<PlayerRegistration> GetOrderedUniqueRegistrations(GameSession gameSession) => gameSession.Pool.PlayerRegistrations
            .GroupBy(r => r.Id)
            .Select(g => g.First())
            .OrderBy(r => r.RegisteredAt)
            .ThenBy(r => r.Id)
            .ToList();
}


