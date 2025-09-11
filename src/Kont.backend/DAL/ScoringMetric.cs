using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class ScoringMetric
{
    public ScoringMetric() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Unit { get; set; }

    [Required]
    public bool HigherIsBetter { get; set; } = true;

    [Required]
    public double Coefficient  { get; set; } = 1.0;

    [Required]
    public Activity Activity { get; set; } = null!;
}
