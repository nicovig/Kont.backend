using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[Route("admin/[controller]")]
[ApiController]
public class GroupsController : ControllerBase
{
    private readonly IGroupsService _groupsService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(
        IGroupsService groupsService,
        ILogger<GroupsController> logger)
    {
        _groupsService = groupsService;
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

            var success = await _groupsService.UpdatePlayerGroupAsync(poolId, playerId, request.GroupId);
            if (!success)
            {
                _logger.LogWarning("Failed to update player {PlayerId} group to {GroupId} in pool {PoolId}", playerId, request.GroupId, poolId);
                return NotFound(new { message = "Pool, player, or group not found" });
            }

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
            var groups = await _groupsService.GetPoolGroupsAsync(poolId);
            var groupsList = groups.ToList();

            if (!groupsList.Any())
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for groups", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            _logger.LogInformation("Retrieved {Count} groups for pool {PoolId}", groupsList.Count, poolId);
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

            var group = await _groupsService.CreatePlayerGroupAsync(poolId, request.GameSessionId, request.GroupNumber);

            _logger.LogInformation("Created group {GroupId} with number {GroupNumber} for pool {PoolId}", group.Id, request.GroupNumber, poolId);
            return CreatedAtAction(nameof(GetPoolGroups), new { poolId }, group);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to create group for pool {PoolId}: {Message}", poolId, ex.Message);
            return NotFound(new { message = ex.Message });
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
            var success = await _groupsService.DeletePlayerGroupAsync(poolId, groupId);
            if (!success)
            {
                _logger.LogWarning("Failed to delete group {GroupId} from pool {PoolId}", groupId, poolId);
                return NotFound(new { message = "Pool or group not found" });
            }

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
