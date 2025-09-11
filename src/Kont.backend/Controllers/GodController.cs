using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.User;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;
using Kont.backend.DAL;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class GodController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<GodController> _logger;

    public GodController(
        IDatabaseContext context,
        IPasswordService passwordService,
        ILogger<GodController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and get access token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Administrator</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost()]
    [ProducesResponseType(typeof(Administrator), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var administrator = await _context.Administrator.FirstOrDefaultAsync(a => a.Email == request.Email);

            if (administrator == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (administrator.Role.RoleType != RoleType.God)
            {
                return Unauthorized();
            }


            if (!_passwordService.VerifyPassword(request.Password, administrator.Password))
            {
                _logger.LogWarning("Login attempt with invalid password for user: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            _logger.LogInformation("User logged in successfully: {Username}", administrator.Email);

            return Ok(administrator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
