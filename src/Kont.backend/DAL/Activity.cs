using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Activity
{
    public Activity() { }

    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public Site Site { get; set; } = null!;

    [Required]
    public List<ScoringMetric> ScoringMetrics { get; set; } = new List<ScoringMetric>();

    [Required]
    public Administrator CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}