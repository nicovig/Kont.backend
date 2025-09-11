using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class PlayerScore
{
    public PlayerScore() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public Player Player { get; set; } = null!;

    [Required]
    public Activity Activity { get; set; } = null!;

    [Required]
    public ScoringMetric ScoringMetric { get; set; } = null!;

    [Required]
    public double Value { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}