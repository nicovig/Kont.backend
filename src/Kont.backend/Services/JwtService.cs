using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Kont.backend.DAL;
using Kont.backend.Models.User;

namespace Kont.backend.Services;

public interface IJwtService
{
    string GenerateToken(Administrator administrator);
    JwtResponse CreateJwtResponse(Administrator administrator);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Administrator administrator)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_key_that_should_be_in_config"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, administrator.Id.ToString()),
            new Claim(ClaimTypes.Email, administrator.Email),
            new Claim(ClaimTypes.Name, $"{administrator.Firstname} {administrator.Lastname}"),
            new Claim(ClaimTypes.Role, administrator.Role.RoleType.ToString()),
            new Claim("UserId", administrator.Id.ToString()),
            new Claim("Role", administrator.Role.RoleType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtResponse CreateJwtResponse(Administrator administrator)
    {
        var token = GenerateToken(administrator);
        
        return new JwtResponse
        {
            Token = token,
            UserId = administrator.Id,
            Role = administrator.Role.RoleType.ToString(),
            Email = administrator.Email,
            Firstname = administrator.Firstname,
            Lastname = administrator.Lastname,
            SubscriptionType = administrator.Subscription.SubscriptionType,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }
}
