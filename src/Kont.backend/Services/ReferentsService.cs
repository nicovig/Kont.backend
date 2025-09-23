using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IReferentsService
{
    Task<bool> AssignReferentAsync(Guid playerRegistrationId);
    Task<bool> RemoveReferentAsync(Guid playerRegistrationId);
    Task<IEnumerable<Player>> GetPoolReferentsAsync(Guid poolId);
    Task<string> GenerateRecoveryQRAsync(Guid poolId, Guid playerId);
}

public class ReferentsService : IReferentsService
{
    private readonly IDatabaseContext _context;

    public ReferentsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> AssignReferentAsync(Guid playerRegistrationId)
    {
        var playerRegistration = await _context.PlayerRegistration
            .FirstOrDefaultAsync(pr => pr.Id == playerRegistrationId);

        if (playerRegistration == null)
        {
            return false;
        }

        playerRegistration.PlayerType = PlayerType.KeyPlayer;

        _context.PlayerRegistration.Update(playerRegistration);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveReferentAsync(Guid playerRegistrationId)
    {
        var playerRegistration = await _context.PlayerRegistration
            .FirstOrDefaultAsync(pr => pr.Id == playerRegistrationId);

        if (playerRegistration == null)
        {
            return false;
        }

        playerRegistration.PlayerType = PlayerType.Player;

        _context.PlayerRegistration.Update(playerRegistration);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Player>> GetPoolReferentsAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return new List<Player>();
        }

        return await _context.PlayerRegistration
            .Include(pr => pr.Player)
            .Where(pr => pr.Pool.Id == poolId && pr.PlayerType == PlayerType.KeyPlayer)
            .Select(pr => pr.Player)
            .ToListAsync();
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

    private string GenerateRecoveryQRCode(Guid poolId, Guid playerId)
    {
        return $"RECOVERY_{poolId:N}_{playerId:N}";
    }
}
