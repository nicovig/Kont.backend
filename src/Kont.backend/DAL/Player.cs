using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.DAL;

[Index(nameof(Username), IsUnique = true)]
public class Player : User
{
    public Player() { }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;
}