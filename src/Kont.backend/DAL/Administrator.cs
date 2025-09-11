using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Administrator : User
{
    public Administrator() { }
    
    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public required Subscription Subscription { get; set; }

    public Administrator? Manager { get; set; }

    [Required]
    public List<Site> Sites { get; set; } = new List<Site>();

    [Required]
    public required Role Role { get; set; }
}


