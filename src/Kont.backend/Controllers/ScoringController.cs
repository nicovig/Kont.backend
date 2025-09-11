using Kont.backend.Services;
using Kont.backend.Models.Scoring;
using Microsoft.AspNetCore.Mvc;

namespace Kont.backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoringController : ControllerBase
{
    private readonly IScoringService _scoringService;

    public ScoringController(IScoringService scoringService)
    {
        _scoringService = scoringService;
    }

    [HttpGet("pools/{poolId}/summary")]
    public async Task<IActionResult> GetPoolSummary([FromRoute] Guid poolId)
    {
        try
        {
            var summary = await _scoringService.GetPoolSummaryAsync(poolId);
            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("pools/{poolId}/rankings")]
    public async Task<IActionResult> GetPoolRankings([FromRoute] Guid poolId)
    {
        try
        {
            var rankings = await _scoringService.GetPoolRankingsAsync(poolId);
            return Ok(rankings);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("pools/{poolId}/activities/{activityId}/rankings")]
    public async Task<IActionResult> GetActivityRankings([FromRoute] Guid poolId, [FromRoute] Guid activityId)
    {
        try
        {
            var rankings = await _scoringService.GetActivityRankingsAsync(poolId, activityId);
            return Ok(rankings);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("pools/{poolId}/players/{playerId}/ranking")]
    public async Task<IActionResult> GetPlayerRanking([FromRoute] Guid poolId, [FromRoute] Guid playerId)
    {
        try
        {
            var ranking = await _scoringService.GetPlayerRankingAsync(poolId, playerId);
            return Ok(ranking);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("pools/{poolId}/players/{playerId}/global-score")]
    public async Task<IActionResult> GetPlayerGlobalScore([FromRoute] Guid poolId, [FromRoute] Guid playerId)
    {
        try
        {
            var globalScore = await _scoringService.CalculatePlayerGlobalScoreAsync(poolId, playerId);
            return Ok(new { GlobalScore = globalScore });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
