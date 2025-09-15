using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Scoring;

namespace Kont.backend.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
public class PoolsController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly ILogger<PoolsController> _logger;

    public PoolsController(
        IDatabaseContext context,
        ILogger<PoolsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all pools for the current admin
    /// </summary>
    /// <returns>List of pools</returns>
    /// <response code="200">Pools retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Pool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPools()
    {
        try
        {
            var pools = await _context.Pool
                .Include(p => p.Event)
                .Include(p => p.PlayerRegistrations)
                .Include(p => p.GameSessions)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} pools", pools.Count);
            return Ok(pools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pools");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific pool by ID
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>Pool details</returns>
    /// <response code="200">Pool found</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Pool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPool(Guid id)
    {
        try
        {
            var pool = await _context.Pool
                .Include(p => p.Event)
                .Include(p => p.PlayerRegistrations)
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found", id);
                return NotFound(new { message = "Pool not found" });
            }

            return Ok(pool);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new pool
    /// </summary>
    /// <param name="pool">Pool data</param>
    /// <returns>Created pool with QR code</returns>
    /// <response code="201">Pool created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(Pool), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePool([FromBody] Pool pool)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            pool.Id = Guid.NewGuid();
            pool.CreatedAt = DateTime.UtcNow;
            pool.QrCode = GenerateQRCode(pool.Id);

            _context.Pool.Add(pool);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created pool {Id} with name {Name}", pool.Id, pool.Name);

            return CreatedAtAction(nameof(GetPool), new { id = pool.Id }, pool);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pool");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing pool
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <param name="pool">Updated pool data</param>
    /// <returns>Updated pool</returns>
    /// <response code="200">Pool updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Pool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePool(Guid id, [FromBody] Pool pool)
    {
        try
        {
            if (id != pool.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPool = await _context.Pool.FindAsync(id);
            if (existingPool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for update", id);
                return NotFound(new { message = "Pool not found" });
            }

            existingPool.Name = pool.Name;
            existingPool.Description = pool.Description;
            existingPool.Event = pool.Event;
            existingPool.IsActive = pool.IsActive;
            existingPool.IsAllPlayersPresent = pool.IsAllPlayersPresent;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated pool {Id}", id);
            return Ok(existingPool);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a pool
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Pool deleted successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePool(Guid id)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(id);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for deletion", id);
                return NotFound(new { message = "Pool not found" });
            }

            _context.Pool.Remove(pool);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted pool {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get pool statistics
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>Pool statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}/stats")]
    [ProducesResponseType(typeof(PoolStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoolStats(Guid id)
    {
        try
        {
            var pool = await _context.Pool
                .Include(p => p.PlayerRegistrations)
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for stats", id);
                return NotFound(new { message = "Pool not found" });
            }

            var stats = new PoolStats
            {
                TotalPlayers = pool.PlayerRegistrations.Count,
                RegisteredPlayers = pool.PlayerRegistrations.Count,
                CheckedInPlayers = pool.PlayerRegistrations.Count(p => p.CheckedInAt.HasValue),
                ActiveSessions = pool.GameSessions.Count(g => g.Status == GameSessionStatus.Active),
                CompletedSessions = pool.GameSessions.Count(g => g.Status == GameSessionStatus.Completed),
                TotalActivities = pool.GameSessions.Select(g => g.Activity).Distinct().Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pool stats for {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate that all players are present
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>Success response</returns>
    /// <response code="200">All players validated as present</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/validate-players")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateAllPlayersPresent(Guid id)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(id);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for player validation", id);
                return NotFound(new { message = "Pool not found" });
            }

            pool.IsAllPlayersPresent = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Validated all players present for pool {Id}", id);
            return Ok(new { message = "All players validated as present" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating players for pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// End a pool
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>Success response</returns>
    /// <response code="200">Pool ended successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/end")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EndPool(Guid id)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(id);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for ending", id);
                return NotFound(new { message = "Pool not found" });
            }

            pool.Status = PoolStatus.Completed;
            pool.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ended pool {Id}", id);
            return Ok(new { message = "Pool ended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get players in a pool
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <returns>List of player registrations</returns>
    /// <response code="200">Players retrieved successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}/players")]
    [ProducesResponseType(typeof(IEnumerable<PlayerRegistration>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoolPlayers(Guid id)
    {
        try
        {
            var pool = await _context.Pool
                .Include(p => p.PlayerRegistrations)
                .ThenInclude(pr => pr.Player)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for players", id);
                return NotFound(new { message = "Pool not found" });
            }

            return Ok(pool.PlayerRegistrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players for pool {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Generate recovery QR code for a player
    /// </summary>
    /// <param name="id">Pool ID</param>
    /// <param name="playerId">Player ID</param>
    /// <returns>Recovery QR code</returns>
    /// <response code="200">QR code generated successfully</response>
    /// <response code="404">Pool or player not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("{id}/players/{playerId}/recovery-qr")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateRecoveryQR(Guid id, Guid playerId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(id);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {Id} not found for recovery QR", id);
                return NotFound(new { message = "Pool not found" });
            }

            var player = await _context.Player.FindAsync(playerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found for recovery QR", playerId);
                return NotFound(new { message = "Player not found" });
            }

            var qrCode = GenerateRecoveryQRCode(id, playerId);

            _logger.LogInformation("Generated recovery QR for player {PlayerId} in pool {PoolId}", playerId, id);
            return Ok(new { qrCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery QR for player {PlayerId} in pool {PoolId}", playerId, id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private string GenerateQRCode(Guid poolId)
    {
        // TODO: Implement actual QR code generation
        return $"POOL_{poolId:N}";
    }

    private string GenerateRecoveryQRCode(Guid poolId, Guid playerId)
    {
        // TODO: Implement actual recovery QR code generation
        return $"RECOVERY_{poolId:N}_{playerId:N}";
    }
}

public class PoolStats
{
    public int TotalPlayers { get; set; }
    public int RegisteredPlayers { get; set; }
    public int CheckedInPlayers { get; set; }
    public int ActiveSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalActivities { get; set; }
}
