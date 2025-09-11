using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class PlayerActivityScore
{
    public PlayerActivityScore() { }

    [Key]
    public Guid Id { get; set; }


    [Required]
    public double TotalScore { get; set; }

    [Required]
    public double Percentage { get; set; }

    [Required]
    public int Rank { get; set; }

    [Required]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Player PlayerEntity { get; set; } = null!;
    public Activity ActivityEntity { get; set; } = null!;
    public Pool PoolEntity { get; set; } = null!;
}
