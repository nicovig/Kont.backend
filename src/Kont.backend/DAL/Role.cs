using System.ComponentModel.DataAnnotations;

namespace Kont.backend.DAL;

public class Role
{
    public Role() { }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public RoleType RoleType { get; set; } = RoleType.Admin;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum RoleType
{
    God,
    Admin,
    Manager
}
