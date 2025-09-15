using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
public class ReferentsController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly ILogger<ReferentsController> _logger;

    public ReferentsController(
        IDatabaseContext context,
        ILogger<ReferentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Assign a referent to a pool
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="request">Referent assignment request</param>
    /// <returns>Success response</returns>
    /// <response code="200">Referent assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Pool or referent not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("pools/{poolId}/referents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignReferent(Guid poolId, [FromBody] AssignReferentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for referent assignment", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var referent = await _context.Player.FindAsync(Guid.Parse(request.ReferentId));
            if (referent == null)
            {
                _logger.LogWarning("Referent {ReferentId} not found", request.ReferentId);
                return NotFound(new { message = "Referent not found" });
            }

            // Check if referent is already assigned to this pool
            var existingAssignment = await _context.PlayerRegistration
                .FirstOrDefaultAsync(pr => pr.Player.Id == referent.Id && pr.Pool.Id == poolId);

            if (existingAssignment != null)
            {
                _logger.LogWarning("Referent {ReferentId} is already assigned to pool {PoolId}", request.ReferentId, poolId);
                return BadRequest(new { message = "Referent is already assigned to this pool" });
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

            _logger.LogInformation("Assigned referent {ReferentId} to pool {PoolId}", request.ReferentId, poolId);
            return Ok(new { message = "Referent assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning referent {ReferentId} to pool {PoolId}", request.ReferentId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Remove a referent from a pool
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="referentId">Referent ID</param>
    /// <returns>Success response</returns>
    /// <response code="200">Referent removed successfully</response>
    /// <response code="404">Pool or referent not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("pools/{poolId}/referents/{referentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveReferent(Guid poolId, Guid referentId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for referent removal", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var referentRegistration = await _context.PlayerRegistration
                .Include(pr => pr.Player)
                .FirstOrDefaultAsync(pr => pr.Player.Id == referentId && pr.Pool.Id == poolId);

            if (referentRegistration == null)
            {
                _logger.LogWarning("Referent {ReferentId} not found in pool {PoolId}", referentId, poolId);
                return NotFound(new { message = "Referent not found in pool" });
            }

            _context.PlayerRegistration.Remove(referentRegistration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed referent {ReferentId} from pool {PoolId}", referentId, poolId);
            return Ok(new { message = "Referent removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing referent {ReferentId} from pool {PoolId}", referentId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get referents for a specific pool
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <returns>List of referents</returns>
    /// <response code="200">Referents retrieved successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("pools/{poolId}/referents")]
    [ProducesResponseType(typeof(IEnumerable<Player>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoolReferents(Guid poolId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for referents", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var referents = await _context.PlayerRegistration
                .Include(pr => pr.Player)
                .Where(pr => pr.Pool.Id == poolId && pr.Player.PlayerType == PlayerType.KeyPlayer)
                .Select(pr => pr.Player)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} referents for pool {PoolId}", referents.Count, poolId);
            return Ok(referents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving referents for pool {PoolId}", poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Generate recovery QR code for a player (referent action)
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="playerId">Player ID</param>
    /// <returns>Recovery QR code</returns>
    /// <response code="200">QR code generated successfully</response>
    /// <response code="404">Pool or player not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("pools/{poolId}/players/{playerId}/recovery-qr")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateRecoveryQR(Guid poolId, Guid playerId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for recovery QR", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var player = await _context.Player.FindAsync(playerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found for recovery QR", playerId);
                return NotFound(new { message = "Player not found" });
            }

            var qrCode = GenerateRecoveryQRCode(poolId, playerId);

            _logger.LogInformation("Generated recovery QR for player {PlayerId} in pool {PoolId}", playerId, poolId);
            return Ok(new { qrCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery QR for player {PlayerId} in pool {PoolId}", playerId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private string GenerateRecoveryQRCode(Guid poolId, Guid playerId)
    {
        // TODO: Implement actual recovery QR code generation
        return $"RECOVERY_{poolId:N}_{playerId:N}";
    }
}

public class AssignReferentRequest
{
    public string ReferentId { get; set; } = null!;
}
