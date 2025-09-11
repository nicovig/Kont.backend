using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class ActivitySummaryData
{
    public ActivitySummaryData() { }

    [Key]
    public Guid Id { get; set; }


    [Required]
    public int PlayersPlayed { get; set; }

    [Required]
    public double BestScore { get; set; }

    [Required]
    public double AverageScore { get; set; }

    [Required]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Activity ActivityEntity { get; set; } = null!;
    public Pool PoolEntity { get; set; } = null!;
}
