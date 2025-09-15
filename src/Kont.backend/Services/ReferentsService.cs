using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IReferentsService
{
    Task<bool> AssignReferentAsync(Guid poolId, string referentId);
    Task<bool> RemoveReferentAsync(Guid poolId, Guid referentId);
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

    public async Task<bool> AssignReferentAsync(Guid poolId, string referentId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return false;
        }

        var referent = await _context.Player.FindAsync(Guid.Parse(referentId));
        if (referent == null)
        {
            return false;
        }

        // Check if referent is already assigned to this pool
        var existingAssignment = await _context.PlayerRegistration
            .FirstOrDefaultAsync(pr => pr.Player.Id == referent.Id && pr.Pool.Id == poolId);

        if (existingAssignment != null)
        {
            return false; // Already assigned
        }

        // Create player registration for referent
        var referentRegistration = new PlayerRegistration
        {
            Id = Guid.NewGuid(),
            Player = referent,
            Pool = pool,
            RegisteredAt = DateTime.UtcNow,
            CheckedInAt = DateTime.UtcNow // Referents are automatically checked in
        };

        _context.PlayerRegistration.Add(referentRegistration);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveReferentAsync(Guid poolId, Guid referentId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null)
        {
            return false;
        }

        var referentRegistration = await _context.PlayerRegistration
            .Include(pr => pr.Player)
            .FirstOrDefaultAsync(pr => pr.Player.Id == referentId && pr.Pool.Id == poolId);

        if (referentRegistration == null)
        {
            return false;
        }

        _context.PlayerRegistration.Remove(referentRegistration);
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
            .Where(pr => pr.Pool.Id == poolId && pr.Player.PlayerType == PlayerType.KeyPlayer)
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
