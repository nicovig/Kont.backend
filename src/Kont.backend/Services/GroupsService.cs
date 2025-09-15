using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IGroupsService
{
    Task<bool> UpdatePlayerGroupAsync(Guid poolId, Guid playerId, string groupId);
    Task<IEnumerable<PlayerGroup>> GetPoolGroupsAsync(Guid poolId);
    Task<PlayerGroup> CreatePlayerGroupAsync(Guid poolId, string gameSessionId, int groupNumber);
    Task<bool> DeletePlayerGroupAsync(Guid poolId, Guid groupId);
}

public class GroupsService : IGroupsService
{
    private readonly IDatabaseContext _context;

    public GroupsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdatePlayerGroupAsync(Guid poolId, Guid playerId, string groupId)
    {
        // Verify pool exists
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return false;
        }

        // Verify player exists and is registered in the pool
        var playerRegistration = await _context.PlayerRegistration
            .Include(pr => pr.Player)
            .FirstOrDefaultAsync(pr => pr.Player.Id == playerId && pr.Pool.Id == poolId);

        if (playerRegistration == null)
        {
            return false;
        }

        // Verify new group exists
        var newGroup = await _context.PlayerGroup
            .Include(pg => pg.GameSession)
            .FirstOrDefaultAsync(pg => pg.Id == Guid.Parse(groupId));

        if (newGroup == null)
        {
            return false;
        }

        // Remove player from current groups
        var currentGroups = await _context.PlayerGroup
            .Where(pg => pg.Players.Contains(playerRegistration))
            .ToListAsync();

        foreach (var group in currentGroups)
        {
            group.Players.Remove(playerRegistration);
        }

        // Add player to new group
        newGroup.Players.Add(playerRegistration);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PlayerGroup>> GetPoolGroupsAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return new List<PlayerGroup>();
        }

        return await _context.PlayerGroup
            .Include(pg => pg.GameSession)
            .Include(pg => pg.Players)
            .ThenInclude(pr => pr.Player)
            .Where(pg => pg.GameSession.Pool.Id == poolId)
            .ToListAsync();
    }

    public async Task<PlayerGroup> CreatePlayerGroupAsync(Guid poolId, string gameSessionId, int groupNumber)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            throw new ArgumentException("Pool not found");
        }

        var gameSession = await _context.GameSession
            .Include(gs => gs.Pool)
            .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(gameSessionId) && gs.Pool.Id == poolId);

        if (gameSession == null)
        {
            throw new ArgumentException("Game session not found");
        }

        var group = new PlayerGroup
        {
            Id = Guid.NewGuid(),
            GameSession = gameSession,
            GroupNumber = groupNumber,
            CreatedAt = DateTime.UtcNow,
            Players = new List<PlayerRegistration>()
        };

        _context.PlayerGroup.Add(group);
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task<bool> DeletePlayerGroupAsync(Guid poolId, Guid groupId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return false;
        }

        var group = await _context.PlayerGroup
            .Include(pg => pg.GameSession)
            .FirstOrDefaultAsync(pg => pg.Id == groupId && pg.GameSession.Pool.Id == poolId);

        if (group == null)
        {
            return false;
        }

        _context.PlayerGroup.Remove(group);
        await _context.SaveChangesAsync();

        return true;
    }
}
