using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class ReferentsController : ControllerBase
{
    private readonly IReferentsService _referentsService;
    private readonly ILogger<ReferentsController> _logger;

    public ReferentsController(
        IReferentsService referentsService,
        ILogger<ReferentsController> logger)
    {
        _referentsService = referentsService;
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

            var success = await _referentsService.AssignReferentAsync(poolId, request.ReferentId);
            if (!success)
            {
                _logger.LogWarning("Failed to assign referent {ReferentId} to pool {PoolId}", request.ReferentId, poolId);
                return NotFound(new { message = "Pool or referent not found, or referent already assigned" });
            }

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
            var success = await _referentsService.RemoveReferentAsync(poolId, referentId);
            if (!success)
            {
                _logger.LogWarning("Failed to remove referent {ReferentId} from pool {PoolId}", referentId, poolId);
                return NotFound(new { message = "Pool or referent not found" });
            }

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
            var referents = await _referentsService.GetPoolReferentsAsync(poolId);
            var referentsList = referents.ToList();

            if (!referentsList.Any())
            {
                _logger.LogWarning("Pool with ID {PoolId} not found for referents", poolId);
                return NotFound(new { message = "Pool not found" });
            }

            _logger.LogInformation("Retrieved {Count} referents for pool {PoolId}", referentsList.Count, poolId);
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
            var qrCode = await _referentsService.GenerateRecoveryQRAsync(poolId, playerId);

            _logger.LogInformation("Generated recovery QR for player {PlayerId} in pool {PoolId}", playerId, poolId);
            return Ok(new { qrCode });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to generate recovery QR for player {PlayerId} in pool {PoolId}: {Message}", playerId, poolId, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recovery QR for player {PlayerId} in pool {PoolId}", playerId, poolId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

}

public class AssignReferentRequest
{
    public string ReferentId { get; set; } = null!;
}
