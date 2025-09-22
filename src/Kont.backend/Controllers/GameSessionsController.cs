using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kont.backend.DAL;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[Authorize(Roles = $"{nameof(RoleType.Admin)},{nameof(RoleType.Manager)}")]
[ApiController]
public class GameSessionsController : ControllerBase
{
    private readonly IGameSessionsService _service;

    public GameSessionsController(IGameSessionsService service)
    {
        _service = service;
    }

    [HttpGet("by-event/{eventId}")]
    [ProducesResponseType(typeof(IEnumerable<GameSession>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEvent(Guid eventId)
    {
        var list = await _service.GetByEventAsync(eventId);
        return Ok(list);
    }

    public record CreateGameSessionRequest(Guid ActivityId);

    [HttpPost("{eventId}")]
    [ProducesResponseType(typeof(GameSession), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid eventId, [FromBody] CreateGameSessionRequest req)
    {
        var created = await _service.CreateAsync(eventId, req.ActivityId);
        if (created == null) return NotFound(new { message = "Event or activity not found" });
        return CreatedAtAction(nameof(GetByEvent), new { eventId }, created);
    }

    [HttpPut("{id}/status/{status}")]
    [ProducesResponseType(typeof(GameSession), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(Guid id, GameSessionStatus status)
    {
        var updated = await _service.UpdateStatusAsync(id, status);
        if (updated == null) return NotFound(new { message = "GameSession not found" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound(new { message = "GameSession not found" });
        return NoContent();
    }

    [HttpPost("{id}/generate-groups/no-scores")]
    [ProducesResponseType(typeof(IEnumerable<PlayerGroup>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateGroupsWithoutScores(Guid id)
    {
        try
        {
            var groups = await _service.GenerateGroupsWithoutScoresAsync(id);
            if (groups == null) return NotFound(new { message = "GameSession not found" });
            return Ok(groups);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/generate-groups/with-scores")]
    [ProducesResponseType(typeof(IEnumerable<PlayerGroup>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateGroupsWithScores(Guid id)
    {
        try
        {
            var groups = await _service.GenerateGroupsWithScoresAsync(id);
            if (groups == null) return NotFound(new { message = "GameSession not found" });
            return Ok(groups);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public record UpdateStartTimeRequest(Guid? Id, DateTime StartedAt);
    [HttpPut("{id}/start-time")]
    [ProducesResponseType(typeof(GameSession), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStartTime(Guid id, [FromBody] UpdateStartTimeRequest req)
    {
        var updated = await _service.UpdateStartTimeAsync(id, req.StartedAt);
        if (updated == null) return NotFound(new { message = "GameSession not found" });
        return Ok(updated);
    }

    public record UpdateEndTimeRequest(Guid? Id, DateTime EndedAt);
    [HttpPut("{id}/end-time")]
    [ProducesResponseType(typeof(GameSession), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEndTime(Guid id, [FromBody] UpdateEndTimeRequest req)
    {
        var updated = await _service.UpdateEndTimeAsync(id, req.EndedAt);
        if (updated == null) return NotFound(new { message = "GameSession not found" });
        return Ok(updated);
    }
}


