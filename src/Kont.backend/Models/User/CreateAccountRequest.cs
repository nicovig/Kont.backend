using System.ComponentModel.DataAnnotations;

namespace Kont.backend.Models.User;

public class CreateAccountRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Firstname { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Lastname { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Range(1900, 2100)]
    public int CreationYear { get; set; }

    public bool IsPremium { get; set; } = false;

    [StringLength(100)]
    public string? Profession { get; set; }
}
