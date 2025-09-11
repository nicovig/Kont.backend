using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Kont.backend.Models.User;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IDatabaseContext context,
        IPasswordService passwordService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    ///// <summary>
    ///// Create a new user account
    ///// </summary>
    ///// <param name="request">Account creation request</param>
    ///// <returns>Created user information</returns>
    ///// <response code="201">Account created successfully</response>
    ///// <response code="400">Invalid request data</response>
    ///// <response code="409">Username or email already exists</response>
    //[HttpPost("register")]
    //[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status409Conflict)]
    //public async Task<IActionResult> Register([FromBody] CreateAccountRequest request)
    //{
    //    try
    //    {
    //        // Check if username already exists
    //        if (await _context.Administrator.AnyAsync(u => u.Username == request.Username))
    //        {
    //            return Conflict(new { message = "Username already exists" });
    //        }

    //        // Check if email already exists
    //        if (await _context.User.AnyAsync(u => u.Email == request.Email))
    //        {
    //            return Conflict(new { message = "Email already exists" });
    //        }

    //        // Create new user
    //        var user = new Administrator
    //        {
    //            Username = request.Username,
    //            Password = _passwordService.HashPassword(request.Password),
    //            Firstname = request.Firstname,
    //            Lastname = request.Lastname,
    //            Email = request.Email,
    //            CreationYear = request.CreationYear,
    //            IsPremium = request.IsPremium,
    //            Profession = request.Profession,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        _context.User.Add(user);
    //        await _context.SaveChangesAsync();

    //        _logger.LogInformation("New user account created: {Username}", user.Username);

    //        // Return user DTO without password
    //        var userDto = new UserDto
    //        {
    //            Id = user.Id,
    //            Username = user.Username,
    //            Email = user.Email,
    //            Firstname = user.Firstname,
    //            Lastname = user.Lastname,
    //            CreationYear = user.CreationYear,
    //            IsPremium = user.IsPremium,
    //            Profession = user.Profession,
    //            CreatedAt = user.CreatedAt
    //        };

    //        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating user account for {Email}", request.Email);
    //        return StatusCode(500, new { message = "Internal server error" });
    //    }
    //}

    ///// <summary>
    ///// Authenticate user and get access token
    ///// </summary>
    ///// <param name="request">Login request</param>
    ///// <returns>Authentication response with token</returns>
    ///// <response code="200">Authentication successful</response>
    ///// <response code="400">Invalid request data</response>
    ///// <response code="401">Invalid credentials</response>
    //[HttpPost("login")]
    //[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //public async Task<IActionResult> Login([FromBody] LoginRequest request)
    //{
    //    try
    //    {
    //        // Find user by email
    //        var administrator = await _context.Administrator.FirstOrDefaultAsync(a => a.Email == request.Email);
    //        if (administrator == null)
    //        {
    //            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
    //            return Unauthorized(new { message = "Invalid credentials" });
    //        }

    //        // Verify password
    //        if (!_passwordService.VerifyPassword(request.Password, administrator.Password))
    //        {
    //            _logger.LogWarning("Login attempt with invalid password for user: {Email}", request.Email);
    //            return Unauthorized(new { message = "Invalid credentials" });
    //        }

    //        // TODO: Generate JWT token here
    //        var token = "mock-jwt-token-" + Guid.NewGuid();

    //        _logger.LogInformation("User logged in successfully: {Username}", administrator.Email);

    //        var response = new AuthResponse
    //        {
    //            Token = token,
    //            User = new UserDto
    //            {
    //                Id = administrator.Id,
    //                Email = administrator.Email,
    //                Firstname = administrator.Firstname,
    //                Lastname = administrator.Lastname,
    //                CreatedAt = administrator.CreatedAt
    //            }
    //        };

    //        return Ok(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error during login for {Email}", request.Email);
    //        return StatusCode(500, new { message = "Internal server error" });
    //    }
    //}

    ///// <summary>
    ///// Get user information by ID
    ///// </summary>
    ///// <param name="id">User ID</param>
    ///// <returns>User information</returns>
    ///// <response code="200">User found</response>
    ///// <response code="404">User not found</response>
    //[HttpGet("user/{id}")]
    //[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> GetUser(Guid id)
    //{
    //    var administrator = await _context.Administrator.FirstOrDefaultAsync(a => a.Id == id);
    //    if (administrator == null)
    //    {
    //        return NotFound(new { message = "User not found" });
    //    }

    //    var userDto = new UserDto
    //    {
    //        Id = user.Id,
    //        Username = user.Username,
    //        Email = user.Email,
    //        Firstname = user.Firstname,
    //        Lastname = user.Lastname,
    //        CreationYear = user.CreationYear,
    //        IsPremium = user.IsPremium,
    //        Profession = user.Profession,
    //        CreatedAt = user.CreatedAt
    //    };

    //    return Ok(userDto);
    //}
}
