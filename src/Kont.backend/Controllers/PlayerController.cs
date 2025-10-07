using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using kept minimal; fully qualify types below
using Kont.backend.Models.Request;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = nameof(RoleType.Player))]
public class PlayerController : ControllerBase
{
    private readonly IEventsService _eventsService;
    private readonly IDatabaseContext _context;

    public PlayerController(IEventsService eventsService, IDatabaseContext context)
    {
        _eventsService = eventsService;
        _context = context;
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

    [HttpPost("{eventId}/{poolId}/register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(Guid eventId, Guid poolId, [FromBody] PlayerRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest(new { message = "Invalid payload" });

        var ev = await _eventsService.GetEventByIdAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });
        var pool = ev.Pools.FirstOrDefault(p => p.Id == poolId);
        if (pool == null) return NotFound(new { message = "Pool not found" });

        var player = new Kont.backend.DAL.Player
        {
            Id = Guid.NewGuid(),
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Email = request.Email,
            Username = request.Username,
            Password = request.Pin
        };
        var registration = new Kont.backend.DAL.PlayerRegistration
        {
            Id = Guid.NewGuid(),
            Player = player,
            Pool = pool,
            PlayerType = Kont.backend.DAL.PlayerType.Player
        };
        _context.Player.Add(player);
        _context.PlayerRegistration.Add(registration);
        await _context.SaveChangesAsync();
        return Created(string.Empty, new { registration.Id });
    }
}


