using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Tools;
using Kont.backend.DAL;
using Kont.backend.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[ApiController]
[Route("user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IDatabaseContext context,
        IPasswordService passwordService,
        ILogger<UserController> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    ///// <summary>
    ///// Get logged user information
    ///// </summary>
    ///// <returns>Current user information</returns>
    ///// <response code="200">User information</response>
    ///// <response code="401">Unauthorized</response>
    ///// <response code="404">User not found</response>
    //[HttpGet]
    //[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> Get()
    //{
    //    var id = User.GetIdWithLoggedUser();
    //    _logger.LogInformation("Getting user {UserId}", id);

    //    var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
    //    if (user == null)
    //    {
    //        return Problem("USER_NOT_FOUND", statusCode: StatusCodes.Status404NotFound);
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

    ///// <summary>
    ///// Update current user information
    ///// </summary>
    ///// <param name="request">User update request</param>
    ///// <returns>Updated user information</returns>
    ///// <response code="200">User updated successfully</response>
    ///// <response code="400">Invalid request data</response>
    ///// <response code="401">Unauthorized</response>
    ///// <response code="404">User not found</response>
    //[HttpPut]
    //[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
    //{
    //    var id = User.GetIdWithLoggedUser();
    //    _logger.LogInformation("Updating user {UserId}", id);

    //    var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
    //    if (user == null)
    //    {
    //        return NotFound(new { message = "User not found" });
    //    }

    //    // Check if username is being changed and if it already exists
    //    if (request.Username != user.Username && 
    //        await _context.User.AnyAsync(u => u.Username == request.Username))
    //    {
    //        return Conflict(new { message = "Username already exists" });
    //    }

    //    // Check if email is being changed and if it already exists
    //    if (request.Email != user.Email && 
    //        await _context.User.AnyAsync(u => u.Email == request.Email))
    //    {
    //        return Conflict(new { message = "Email already exists" });
    //    }

    //    // Update user properties
    //    user.Username = request.Username;
    //    user.Firstname = request.Firstname;
    //    user.Lastname = request.Lastname;
    //    user.Email = request.Email;
    //    user.CreationYear = request.CreationYear;
    //    user.IsPremium = request.IsPremium;
    //    user.Profession = request.Profession;

    //    // Update password if provided
    //    if (!string.IsNullOrEmpty(request.Password))
    //    {
    //        user.Password = _passwordService.HashPassword(request.Password);
    //    }

    //    await _context.SaveChangesAsync();

    //    _logger.LogInformation("User {UserId} updated successfully", id);

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

    ///// <summary>
    ///// Delete current user account
    ///// </summary>
    ///// <returns>No content</returns>
    ///// <response code="204">User deleted successfully</response>
    ///// <response code="401">Unauthorized</response>
    ///// <response code="404">User not found</response>
    //[HttpDelete]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> Delete()
    //{
    //    var id = User.GetIdWithLoggedUser();
    //    _logger.LogInformation("Deleting user {UserId}", id);

    //    var user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
    //    if (user == null)
    //    {
    //        return NotFound(new { message = "User not found" });
    //    }

    //    _context.User.Remove(user);
    //    await _context.SaveChangesAsync();

    //    _logger.LogInformation("User {UserId} deleted successfully", id);

    //    return NoContent();
    //}
}

