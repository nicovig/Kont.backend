using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.DAL;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    public User() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Firstname { get; set; } = null!;

    [Required]
    public string Lastname { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}