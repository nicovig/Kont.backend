using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class PlayerGroup
{
    public PlayerGroup() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public GameSession GameSession { get; set; } = null!;

    [Required]
    public List<PlayerRegistration> Players { get; set; } = new List<PlayerRegistration>();

    [Required]
    public int GroupNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

