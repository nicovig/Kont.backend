using System;

namespace Kont.backend.Models.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public int CreationYear { get; set; }
    public bool IsPremium { get; set; }
    public string? Profession { get; set; }
    public DateTime CreatedAt { get; set; }
}
