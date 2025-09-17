using System.ComponentModel.DataAnnotations;
using Kont.backend.DAL;

namespace Kont.backend.Models.Request;

public class CreateEventRequest
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    public required string EventLink { get; set; }

    [Required]
    public required DateTime StartedAt { get; set; }

    [Required]
    public required DateTime EndedAt { get; set; }

    [Required]
    public required Guid SiteId { get; set; }

    public List<Guid> ActivityIds { get; set; } = new();

    public EventStatus Status { get; set; } = EventStatus.Pending;
}

public class UpdateEventRequest : CreateEventRequest
{
    [Required]
    public required Guid Id { get; set; }
}


