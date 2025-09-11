using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreCalculationController : ControllerBase
{
    private readonly IScoreCalculationService _scoreCalculationService;

    public ScoreCalculationController(IScoreCalculationService scoreCalculationService)
    {
        _scoreCalculationService = scoreCalculationService;
    }

    [HttpPost("recalculate-pool/{poolId}")]
    public async Task<IActionResult> RecalculatePoolScores(Guid poolId)
    {
        try
        {
            await _scoreCalculationService.RecalculatePoolScoresAsync(poolId);
            return Ok(new { message = "Pool scores recalculated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while recalculating scores", error = ex.Message });
        }
    }

    [HttpPost("recalculate-player-activity/{poolId}/{playerId}/{activityId}")]
    public async Task<IActionResult> RecalculatePlayerActivityScore(Guid poolId, Guid playerId, Guid activityId)
    {
        try
        {
            await _scoreCalculationService.RecalculatePlayerActivityScoreAsync(poolId, playerId, activityId);
            return Ok(new { message = "Player activity score recalculated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while recalculating score", error = ex.Message });
        }
    }

    [HttpPost("recalculate-player-global/{poolId}/{playerId}")]
    public async Task<IActionResult> RecalculatePlayerGlobalScore(Guid poolId, Guid playerId)
    {
        try
        {
            await _scoreCalculationService.RecalculatePlayerGlobalScoreAsync(poolId, playerId);
            return Ok(new { message = "Player global score recalculated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while recalculating score", error = ex.Message });
        }
    }
}
