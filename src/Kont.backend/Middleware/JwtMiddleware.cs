using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseContext _context;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, IDatabaseContext context, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        _logger.LogInformation("Authorization header: {AuthHeader}", authHeader);
        
        var token = authHeader?.Split(" ").Last();
        _logger.LogInformation("Extracted token: {Token}", token != null ? "Present" : "Null");

        if (token != null)
        {
            await AttachUserToContextAsync(context, token);
        }

        await _next(context);
    }

    private async Task AttachUserToContextAsync(HttpContext context, string token)
    {
        try
        {
            _logger.LogInformation("Attempting to validate token");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_key_that_should_be_in_config");
            
            _logger.LogInformation("JWT Key configured: {HasKey}", !string.IsNullOrEmpty(_configuration["Jwt:Key"]));
            _logger.LogInformation("JWT Issuer: {Issuer}", _configuration["Jwt:Issuer"]);
            _logger.LogInformation("JWT Audience: {Audience}", _configuration["Jwt:Audience"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            _logger.LogInformation("Token validation successful");

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId");
            
            _logger.LogInformation("UserId claim: {UserIdClaim}", userIdClaim?.Value);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogInformation("Looking for administrator with ID: {UserId}", userId);
                
                var administrator = await _context.Administrator
                    .Include(a => a.Role)
                    .Include(a => a.Subscription)
                    .Include(a => a.Sites)
                    .FirstOrDefaultAsync(a => a.Id == userId);

                if (administrator != null)
                {
                    _logger.LogInformation("Administrator found: {Email}, attaching to context", administrator.Email);
                    context.Items["User"] = administrator;
                    context.Items["UserId"] = administrator.Id;
                    context.Items["UserRole"] = administrator.Role.RoleType.ToString();
                }
                else
                {
                    _logger.LogWarning("Administrator not found for ID: {UserId}", userId);
                }
            }
            else
            {
                _logger.LogWarning("Invalid or missing UserId claim in token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
        }
    }
}
