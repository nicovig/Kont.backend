using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class PlayerGlobalScore
{
    public PlayerGlobalScore() { }

    [Key]
    public Guid Id { get; set; }


    [Required]
    public double TotalScore { get; set; }

    [Required]
    public double Percentage { get; set; }

    [Required]
    public int GlobalRank { get; set; }

    [Required]
    public int ActivitiesPlayed { get; set; }

    [Required]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Player PlayerEntity { get; set; } = null!;
    public Pool PoolEntity { get; set; } = null!;
}
