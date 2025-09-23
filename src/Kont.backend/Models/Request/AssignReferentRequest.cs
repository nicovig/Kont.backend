using System.ComponentModel.DataAnnotations;

namespace Kont.backend.Models.Request;

public class AssignReferentRequest
{
    [Required]
    public string ReferentId { get; set; } = string.Empty;
}


