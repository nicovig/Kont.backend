using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class PlayerRegistration
{
    public PlayerRegistration() { }

    [Key]
    public Guid Id { get; set; }
    public Player Player { get; set; } = null!;
    public Pool Pool { get; set; } = null!;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedInAt { get; set; }

    [Required]
    public PlayerType PlayerType { get; set; } = PlayerType.Player;
}

public enum PlayerType
{
    Player,
    KeyPlayer
}
