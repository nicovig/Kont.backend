using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Kont.backend.Models.Response;
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
    private readonly IEventInvitationService _eventInvitationService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventsService eventsService,
        IUserContextService userContextService,
        IEventInvitationService eventInvitationService,
        ILogger<EventsController> logger)
    {
        _eventsService = eventsService;
        _userContextService = userContextService;
        _eventInvitationService = eventInvitationService;
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

    [HttpPut("{id}/all-players-present")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEventAllPlayersPresent(Guid id, [FromQuery] bool isAllPlayersPresent)
    {
        var pool = await _eventsService.UpdateEventAllPlayersPresentAsync(id, isAllPlayersPresent);
        if (pool == null) return NotFound(new { message = "Pool not found" });
        return Ok(pool);
    }

    [HttpPost("{id}/send-qr-codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendQRCodeToEmailList(Guid id, [FromBody] List<string> emails)
    {
        if (emails == null || !emails.Any()) return BadRequest(new { message = "Email list is required" });
        
        var currentUser = _userContextService.GetCurrentUser();
        if (currentUser == null) return Unauthorized(new { message = "User not authenticated" });

        try
        {
            await _eventInvitationService.SendQRCodeToEmailListAsync(id, emails, currentUser.Id);
            return Ok(new { message = "QR codes sent successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending QR codes for event {EventId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}/player-registrations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerRegistrations(Guid id)
    {
        var currentUser = _userContextService.GetCurrentUser();
        if (currentUser == null) return Unauthorized(new { message = "User not authenticated" });

        try
        {
            var registrations = await _eventsService.GetPlayerRegistrationsByEventAsync(id);
            return Ok(registrations);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player registrations for event {EventId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}/players/{playerRegistrationId}/present")]
    [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlayerIsPresent(Guid id, Guid playerRegistrationId, [FromQuery] bool isPresent)
    {
        var currentUser = _userContextService.GetCurrentUser();
        if (currentUser == null) return Unauthorized(new { message = "User not authenticated" });

        var ev = await _eventsService.UpdateEventPlayerIsPresentAsync(id, playerRegistrationId, isPresent);
        if (ev == null) return NotFound(new { message = "Event or player not found" });
        return Ok(ev);
    }
}


