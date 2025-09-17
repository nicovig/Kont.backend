using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Kont.backend.DAL;
using Microsoft.AspNetCore.Authorization;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[Authorize(Roles = $"{nameof(RoleType.Admin)},{nameof(RoleType.Manager)}")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventsService eventsService,
        IUserContextService userContextService,
        ILogger<EventsController> logger)
    {
        _eventsService = eventsService;
        _userContextService = userContextService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Event>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var list = await _eventsService.GetEventsAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await _eventsService.GetEventByIdAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found" });
        return Ok(ev);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Event), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var currentUser = _userContextService.GetCurrentUser();
        if (currentUser == null) return Unauthorized(new { message = "User not authenticated" });

        var created = await _eventsService.CreateEventAsync(request, currentUser.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch" });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _eventsService.UpdateEventAsync(id, request);
        if (updated == null) return NotFound(new { message = "Event not found" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _eventsService.DeleteEventAsync(id);
        if (!ok) return NotFound(new { message = "Event not found" });
        return NoContent();
    }
}


