using Microsoft.AspNetCore.Mvc;
using Kont.backend.Models.User;
using Kont.backend.Services;
using Kont.backend.DAL;
using Microsoft.AspNetCore.Authorization;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = nameof(RoleType.God))]
public class GodController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<GodController> _logger;

    public GodController(
        IAuthService authService,
        ILogger<GodController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and get access token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>JWT response with user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost()]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwtResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var jwtResponse = await _authService.LoginAsync(request.Email, request.Password, RoleType.God);
            return Ok(jwtResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
