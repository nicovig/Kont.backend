using Kont.backend.DAL;

namespace Kont.backend.Models.User;

public class JwtResponse
{
    public required string Token { get; set; }
    public required Guid UserId { get; set; }
    public required string Role { get; set; }
    public required string Email { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required SubscriptionType SubscriptionType { get; set; }
    public DateTime ExpiresAt { get; set; }
}
