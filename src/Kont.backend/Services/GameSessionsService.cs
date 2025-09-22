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
    Task<IEnumerable<PlayerGroup>?> GenerateGroupsWithScoresAsync(Guid gameSessionId);
}

public class GameSessionsService : IGameSessionsService
{
    private readonly IDatabaseContext _context;
    private readonly IScoringService _scoringService;

    public GameSessionsService(IDatabaseContext context, IScoringService scoringService)
    {
        _context = context;
        _scoringService = scoringService;
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
        if (gameSession.Status != GameSessionStatus.Pending) throw new InvalidOperationException("GameSession must be Pending");
        if (gameSession.Pool.Status != PoolStatus.Active) throw new InvalidOperationException("Pool must be Active");
        var otherSessions = await _context.GameSession.Where(gs => gs.Pool.Id == gameSession.Pool.Id && gs.Id != gameSession.Id).ToListAsync();
        var isFirst = !otherSessions.Any();
        var othersClosed = otherSessions.All(s => s.Status == GameSessionStatus.Completed || s.Status == GameSessionStatus.Cancelled);
        if (!isFirst && !othersClosed) throw new InvalidOperationException("Other sessions must be Completed or Cancelled");
        var limit = gameSession.Activity.PlayersPerGroupLimit;
        if (limit <= 0) return Array.Empty<PlayerGroup>();
        var regs = GetOrderedUniqueRegistrations(gameSession);
        var groups = BuildGroups(gameSession, regs, limit);
        _context.PlayerGroup.AddRange(groups);
        await _context.SaveChangesAsync();
        return groups;
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
    
    public async Task<IEnumerable<PlayerGroup>?> GenerateGroupsWithScoresAsync(Guid gameSessionId)
    {
        var gameSession = await LoadGameSessionGraphAsync(gameSessionId);
        if (gameSession == null) return null;
        if (gameSession.Status != GameSessionStatus.Pending) throw new InvalidOperationException("GameSession must be Pending");
        if (gameSession.Pool.Status != PoolStatus.Active) throw new InvalidOperationException("Pool must be Active");
        var otherSessions = await _context.GameSession.Where(gs => gs.Pool.Id == gameSession.Pool.Id && gs.Id != gameSession.Id && gs.Activity.Id == gameSession.Activity.Id).ToListAsync();
        if (!otherSessions.Any()) throw new InvalidOperationException("No previous sessions to base scores on");
        var othersClosed = otherSessions.All(s => s.Status == GameSessionStatus.Completed || s.Status == GameSessionStatus.Cancelled);
        if (!othersClosed) throw new InvalidOperationException("Other sessions must be Completed or Cancelled");

        var limit = gameSession.Activity.PlayersPerGroupLimit;
        if (limit <= 0) return Array.Empty<PlayerGroup>();

        var regs = GetOrderedUniqueRegistrations(gameSession);

        var rankings = await _scoringService.GetActivityRankingsAsync(gameSession.Pool.Id, gameSession.Activity.Id);
        if (!rankings.Any()) throw new InvalidOperationException("No scores found for activity in pool");

        var playerIdToPercentage = rankings.ToDictionary(r => r.PlayerId, r => r.GlobalPercentage);

        var ordered = regs
            .OrderByDescending(r => playerIdToPercentage.GetValueOrDefault(r.Player.Id, 0))
            .ThenBy(r => r.Player.Username)
            .ToList();

        var groups = BuildGroups(gameSession, ordered, limit);
        _context.PlayerGroup.AddRange(groups);
        await _context.SaveChangesAsync();
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


