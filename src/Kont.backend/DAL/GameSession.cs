using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class GameSession
{
    public GameSession() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public Pool Pool { get; set; } = null!;

    [Required]
    public Activity Activity { get; set; } = null!;

    [Required]
    public List<PlayerGroup> PlayerGroups { get; set; } = new List<PlayerGroup>();

    public GameSessionStatus Status { get; set; } = GameSessionStatus.Pending;

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum GameSessionStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}