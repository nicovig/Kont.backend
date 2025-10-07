using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using kept minimal; fully qualify types below

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = nameof(RoleType.Player))]
public class PlayerController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public PlayerController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Kont.backend.Models.Response.PlayerEventInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await _eventsService.GetEventByIdAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found" });
        var location = ev.Site != null ? $"{ev.Site.Name}\n{ev.Site.Address}\n{ev.Site.City}" : string.Empty;
        return Ok(new Kont.backend.Models.Response.PlayerEventInfoResponse { Name = ev.Name, StartDate = ev.StartedAt, EndDate = ev.EndedAt, Location = location });
    }
}


