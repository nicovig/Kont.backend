using Microsoft.AspNetCore.Mvc;
using Kont.backend.Models.User;
using Kont.backend.Services;
using Kont.backend.DAL;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
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
            var administrator = await _authService.LoginAsync(request.Email, request.Password, RoleType.God);
            return Ok(administrator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
