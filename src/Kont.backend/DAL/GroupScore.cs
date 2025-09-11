using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class GroupScore
{
    public GroupScore() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public PlayerGroup Group { get; set; } = null!;

    [Required]
    public Activity Activity { get; set; } = null!;

    [Required]
    public double TotalScore { get; set; }

    [Required]
    public double AverageScore { get; set; }

    [Required]
    public int PlayerCount { get; set; }

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
