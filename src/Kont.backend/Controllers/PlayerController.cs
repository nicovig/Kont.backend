using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Mvc;
using Kont.backend.Models.Request;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class PlayerController : ControllerBase
{
    private readonly IEventsService _eventsService;
    private readonly IDatabaseContext _context;

    public PlayerController(IEventsService eventsService, IDatabaseContext context)
    {
        _eventsService = eventsService;
        _context = context;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] PlayerLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest(new { message = "Invalid payload" });

        var player = _context.Player.FirstOrDefault(p => p.Email == request.Identifier || p.Username == request.Identifier);
        if (player == null) return Unauthorized(new { code = "not_found", message = "Compte introuvable" });
        if (player.Password != request.Pin) return Unauthorized(new { code = "bad_pin", message = "Code PIN invalide" });

        return Ok(new { playerId = player.Id });
    }

    private async Task<(Event? ev, Pool? pool, IActionResult? error)> GetEventAndPool(Guid eventId, Guid poolId)
    {
        var ev = await _eventsService.GetEventByIdAsync(eventId);
        if (ev == null) return (null, null, NotFound(new { message = "Event not found" }));
        var pool = ev.Pools.FirstOrDefault(p => p.Id == poolId);
        if (pool == null) return (ev, null, NotFound(new { message = "Pool not found" }));
        return (ev, pool, null);
    }

    private static PlayerRegistration BuildRegistration(Player player, Pool pool)
    {
        return new PlayerRegistration
        {
            Id = Guid.NewGuid(),
            Player = player,
            Pool = pool,
            PlayerType = Kont.backend.DAL.PlayerType.Player
        };
    }

    [HttpGet("{eventId}")]
    [ProducesResponseType(typeof(Kont.backend.Models.Response.PlayerEventInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(Guid eventId)
    {
        var ev = await _eventsService.GetEventByIdAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found" });
        var location = ev.Site != null ? $"{ev.Site.Name}\n{ev.Site.Address}\n{ev.Site.City}" : string.Empty;
        return Ok(new Kont.backend.Models.Response.PlayerEventInfoResponse { Name = ev.Name, StartDate = ev.StartedAt, EndDate = ev.EndedAt, Location = location });
    }

    [HttpPost("{eventId}/{poolId}/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(Guid eventId, Guid poolId, [FromBody] PlayerRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest(new { message = "Invalid payload" });

        var (_, pool, error) = await GetEventAndPool(eventId, poolId);
        if (error != null) return error;

        // Conflicts: email or username already taken
        var emailTaken = _context.Player.Any(p => p.Email == request.Email);
        if (emailTaken) return Conflict(new { code = "email_taken", message = "Email déjà utilisé" });
        var usernameTaken = _context.Player.Any(p => p.Username == request.Username);
        if (usernameTaken) return Conflict(new { code = "username_taken", message = "Nom d'utilisateur déjà utilisé" });

        var player = new Kont.backend.DAL.Player
        {
            Id = Guid.NewGuid(),
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Email = request.Email,
            Username = request.Username,
            Password = request.Pin
        };
        _context.Entry(pool!).State = EntityState.Unchanged;
        var registration = BuildRegistration(player, pool!);
        _context.Player.Add(player);
        _context.PlayerRegistration.Add(registration);
        await _context.SaveChangesAsync();
        return Created(string.Empty, new { registration.Id });
    }

    [HttpPost("{eventId}/{poolId}/login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(Guid eventId, Guid poolId, [FromBody] PlayerLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest(new { message = "Invalid payload" });

        var (_, pool, error) = await GetEventAndPool(eventId, poolId);
        if (error != null) return error;

        var player = _context.Player.FirstOrDefault(p => p.Email == request.Identifier || p.Username == request.Identifier);
        if (player == null) return Unauthorized(new { code = "not_found", message = "Compte introuvable" });
        if (player.Password != request.Pin) return Unauthorized(new { code = "bad_pin", message = "Code PIN invalide" });

        var alreadyRegistered = _context.PlayerRegistration.Any(r => r.Player.Id == player.Id && r.Pool.Id == pool!.Id);
        if (alreadyRegistered)
        {
            return Ok(new { playerId = player.Id, registration = "exists" });
        }

        _context.Entry(pool!).State = EntityState.Unchanged;
        var registration = BuildRegistration(player, pool!);
        _context.PlayerRegistration.Add(registration);
        await _context.SaveChangesAsync();
        return Created(string.Empty, new { registration.Id });
    }

    [HttpGet("check-email")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CheckEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return Ok(new { available = false });
        var exists = _context.Player.Any(p => p.Email.ToUpper() == email.ToUpper());
        return Ok(new { available = !exists });
    }

    [HttpGet("check-username")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CheckUsername([FromQuery] string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return Ok(new { available = false });
        var exists = _context.Player.Any(p => p.Username.ToUpper() == username.ToUpper());
        return Ok(new { available = !exists });
    }
}


