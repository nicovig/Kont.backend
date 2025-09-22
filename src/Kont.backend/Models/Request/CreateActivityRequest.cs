using System.ComponentModel.DataAnnotations;
using Kont.backend.DAL;

namespace Kont.backend.Models.Request;

public class CreateActivityRequest
{
    public CreateActivityRequest() { }

    
    [StringLength(100)]
    public required string Name { get; set; } = null!;

    [StringLength(500)]
    public required string Description { get; set; } = null!;
    
    public required Site Site { get; set; } = null!;

    [Required]
    public int PlayersPerGroupLimit { get; set; } = 0;

    [MinLength(1)]
    public required List<CreateScoringMetricRequest> ScoringMetrics { get; set; } = new List<CreateScoringMetricRequest>();
}


public class UpdateActivityRequest : CreateActivityRequest
{
    public UpdateActivityRequest() { }

    public required Guid Id { get; set; }
}

public class CreateScoringMetricRequest
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Required]
    public bool HigherIsBetter { get; set; } = true;

    [Required]
    [Range(0, 1)]
    public double Coefficient { get; set; } = 1.0;
}


