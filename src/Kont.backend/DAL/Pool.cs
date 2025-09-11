using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Pool
{
    public Pool() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string QrCode { get; set; } = null!;

    [Required]
    public Event Event { get; set; } = null!;

    [Required]
    public List<PlayerRegistration> PlayerRegistrations { get; set; } = new List<PlayerRegistration>();

    [Required]
    public List<GameSession> GameSessions { get; set; } = new List<GameSession>();

    public bool IsActive { get; set; } = true;

    public bool IsAllPlayersPresent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public PoolStatus Status { get; set; } = PoolStatus.Pending;
}

public enum PoolStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}

