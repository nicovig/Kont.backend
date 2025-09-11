using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Site
{
    public Site() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Address { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public string ZipCode { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public string Country { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public string State { get; set; } = null!;    

    [Required]
    [StringLength(10)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }

    public string? Logo { get; set; }

    public List<Administrator> Administrators { get; set; } = new List<Administrator>();

    public List<Activity> Activities { get; set; } = new List<Activity>();
}