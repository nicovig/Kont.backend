using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Administrator : User
{
    public Administrator() { }
    
    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public Guid SubscriptionId { get; set; }

    public Administrator? Manager { get; set; }

    [Required]
    public List<Site> Sites { get; set; } = new List<Site>();

    [Required]
    public Role Role { get; set; }

    [Required]
    public required bool IsActive { get; set; }

    public Subscription Subscription { get; set; }
}


