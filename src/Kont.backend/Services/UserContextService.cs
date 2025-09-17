using Kont.backend.DAL;
using System.Security.Claims;

namespace Kont.backend.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(IHttpContextAccessor httpContextAccessor, ILogger<UserContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Administrator? GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("UserId");
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var firstnameLastname = principal.FindFirst(ClaimTypes.Name)?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (idClaim == null || !Guid.TryParse(idClaim.Value, out var userId))
        {
            return null;
        }

        var names = (firstnameLastname ?? " ").Split(' ', 2);
        var admin = new Administrator
        {
            Id = userId,
            Email = email ?? string.Empty,
            Firstname = names[0],
            Lastname = names.Length > 1 ? names[1] : string.Empty,
            Role = new Role { RoleType = Enum.TryParse<RoleType>(role, out var rt) ? rt : RoleType.Admin },
            IsActive = true,
            PhoneNumber = string.Empty,
            Sites = new List<Site>(),
        };

        return admin;
    }

    public bool IsUserAuthenticated()
    {
        var isAuthenticated = GetCurrentUser() != null;
        _logger.LogInformation("IsUserAuthenticated: {IsAuthenticated}", isAuthenticated);
        return isAuthenticated;
    }

    public Guid? GetCurrentUserId()
    {
        var userId = GetCurrentUser()?.Id;
        _logger.LogInformation("GetCurrentUserId: {UserId}", userId);
        return userId;
    }
}
