using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IAuthService
{
    Task<Administrator> LoginAsync(string email, string password, RoleType? requiredRole = null);
}

public class AuthService : IAuthService
{
    private readonly IDatabaseContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IDatabaseContext context, IPasswordService passwordService, ILogger<AuthService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<Administrator> LoginAsync(string email, string password, RoleType? requiredRole = null)
    {
        var administrator = await _context.Administrator
            .Include(a => a.Role)
            .Include(a => a.Subscription)
            .Include(a => a.Manager)
            .Include(a => a.Sites)
            .FirstOrDefaultAsync(a => a.Email == email);

        if (administrator == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
            throw new ArgumentException("Invalid credentials");
        }

        if (!_passwordService.VerifyPassword(password, administrator.Password))
        {
            _logger.LogWarning("Login attempt with invalid password for user: {Email}", email);
            throw new ArgumentException("Invalid credentials");
        }

        if (requiredRole.HasValue && administrator.Role.RoleType != requiredRole.Value)
        {
            _logger.LogWarning("User {Email} does not have required role {Role}", email, requiredRole.Value);
            throw new ArgumentException("Invalid credentials");
        }

        _logger.LogInformation("User logged in successfully: {Email}", email);
        return administrator;
    }
}



