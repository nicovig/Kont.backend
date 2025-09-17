using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Subscription
{
    public Subscription() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Klasel;

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

    public Administrator Administrator { get; set; }
}

public enum SubscriptionType
{
    Esae,
    Deraou,
    Klasel,
    Stroll
}