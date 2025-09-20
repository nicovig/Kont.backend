using Kont.backend.DAL;

namespace Kont.backend.Models.Response;

public class PlayerRegistrationResponse
{
    public Guid Id { get; set; }
    public string PlayerFirstname { get; set; } = null!;
    public string PlayerLastname { get; set; } = null!;
    public string PlayerEmail { get; set; } = null!;
    public string PlayerUsername { get; set; } = null!;
    public PlayerType PlayerType { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
}
