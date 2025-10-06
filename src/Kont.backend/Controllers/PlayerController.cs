using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[Route("public/events")]
[ApiController]
[Authorize(Roles = nameof(RoleType.Player))]
public class PlayerController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public PlayerController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpGet("{eventLink}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLink(string eventLink)
    {
        var ev = await _eventsService.GetEventByLinkAsync(eventLink);
        if (ev == null) return NotFound(new { message = "Event not found" });
        return Ok(new { name = ev.Name, date = ev.StartedAt, status = ev.Status.ToString() });
    }
}


