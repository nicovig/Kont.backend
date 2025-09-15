using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
public class GroupsController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(
        IDatabaseContext context,
        ILogger<GroupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Update player group assignment
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="playerId">Player ID</param>
    /// <param name="request">Group update request</param>
    /// <returns>Success response</returns>
    /// <response code="200">Player group updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Pool, player, or group not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("pools/{poolId}/players/{playerId}/group")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePlayerGroup(Guid poolId, Guid playerId, [FromBody] UpdatePlayerGroupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify pool exists
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for group update", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            // Verify player exists and is registered in the pool
            var playerRegistration = await _context.PlayerRegistration
                .Include(pr => pr.Player)
                .FirstOrDefaultAsync(pr => pr.Player.Id == playerId && pr.Pool.Id == poolId);

            if (playerRegistration == null)
            {
                _logger.LogWarning("Player {PlayerId} not found in pool {PoolId}", playerId, poolId);
                return NotFound(new { message = "Player not found in pool" });
            }

            // Verify new group exists
            var newGroup = await _context.PlayerGroup
                .Include(pg => pg.GameSession)
                .FirstOrDefaultAsync(pg => pg.Id == Guid.Parse(request.GroupId));

            if (newGroup == null)
            {
                _logger.LogWarning("Group {GroupId} not found", request.GroupId);
                return NotFound(new { message = "Group not found" });
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

            _logger.LogInformation("Updated player {PlayerId} group to {GroupId} in pool {PoolId}", playerId, request.GroupId, poolId);
            return Ok(new { message = "Player group updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player {PlayerId} group in pool {PoolId}", playerId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get groups for a specific pool
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <returns>List of player groups</returns>
    /// <response code="200">Groups retrieved successfully</response>
    /// <response code="404">Pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("pools/{poolId}/groups")]
    [ProducesResponseType(typeof(IEnumerable<PlayerGroup>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPoolGroups(Guid poolId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for groups", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var groups = await _context.PlayerGroup
                .Include(pg => pg.GameSession)
                .Include(pg => pg.Players)
                .ThenInclude(pr => pr.Player)
                .Where(pg => pg.GameSession.Pool.Id == poolId)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} groups for pool {PoolId}", groups.Count, poolId);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving groups for pool {PoolId}", poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new player group
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="request">Group creation request</param>
    /// <returns>Created group</returns>
    /// <response code="201">Group created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Pool or game session not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("pools/{poolId}/groups")]
    [ProducesResponseType(typeof(PlayerGroup), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePlayerGroup(Guid poolId, [FromBody] CreatePlayerGroupRequest request)
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
                _logger.LogWarning("Pool with ID {PoolId} not found for group creation", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var gameSession = await _context.GameSession
                .Include(gs => gs.Pool)
                .FirstOrDefaultAsync(gs => gs.Id == Guid.Parse(request.GameSessionId) && gs.Pool.Id == poolId);

            if (gameSession == null)
            {
                _logger.LogWarning("Game session {GameSessionId} not found in pool {PoolId}", request.GameSessionId, poolId);
                return NotFound(new { message = "Game session not found" });
            }

            var group = new PlayerGroup
            {
                Id = Guid.NewGuid(),
                GameSession = gameSession,
                GroupNumber = request.GroupNumber,
                CreatedAt = DateTime.UtcNow,
                Players = new List<PlayerRegistration>()
            };

            _context.PlayerGroup.Add(group);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created group {GroupId} with number {GroupNumber} for pool {PoolId}", group.Id, request.GroupNumber, poolId);
            return CreatedAtAction(nameof(GetPoolGroups), new { poolId }, group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group for pool {PoolId}", poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a player group
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="groupId">Group ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Group deleted successfully</response>
    /// <response code="404">Pool or group not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("pools/{poolId}/groups/{groupId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePlayerGroup(Guid poolId, Guid groupId)
    {
        try
        {
            var pool = await _context.Pool.FindAsync(poolId);
            if (pool == null)
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for group deletion", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            var group = await _context.PlayerGroup
                .Include(pg => pg.GameSession)
                .FirstOrDefaultAsync(pg => pg.Id == groupId && pg.GameSession.Pool.Id == poolId);

            if (group == null)
            {
                _logger.LogWarning("Group {GroupId} not found in pool {PoolId}", groupId, poolId);
                return NotFound(new { message = "Group not found" });
            }

            _context.PlayerGroup.Remove(group);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted group {GroupId} from pool {PoolId}", groupId, poolId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId} from pool {PoolId}", groupId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

public class UpdatePlayerGroupRequest
{
    public string GroupId { get; set; } = null!;
}

public class CreatePlayerGroupRequest
{
    public string GameSessionId { get; set; } = null!;
    public int GroupNumber { get; set; }
}
