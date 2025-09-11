
using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Event
{
    public Event() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;    

    [Required]
    public string EventLink { get; set; } = null!;

    [Required]
    public DateTime? StartedAt { get; set; }

    [Required]
    public DateTime? EndedAt { get; set; }

    [Required]
    public required Site Site { get; set; }
    
    public EventStatus Status { get; set; } = EventStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Activity> Activities { get; set; } = new List<Activity>();

    public List<Pool> Pools { get; set; } = new List<Pool>();    
}


public enum EventStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}