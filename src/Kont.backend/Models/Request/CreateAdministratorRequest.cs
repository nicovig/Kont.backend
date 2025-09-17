using System.ComponentModel.DataAnnotations;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.Models.Request;

public class CreateAdministratorRequest
{
    public CreateAdministratorRequest() { }

    public required string Firstname { get; set; } = null!;

    public required string Lastname { get; set; } = null!;

    [StringLength(100)]
    public required string Password { get; set; } = null!;

    [EmailAddress]
    public required string Email { get; set; } = null!;

    public required string PhoneNumber { get; set; } = null!;

    public required List<Site> Sites { get; set; } = new List<Site>();

    public required Role Role { get; set; }

    public required bool IsActive { get; set; }

    public required SubscriptionType SubscriptionType { get; set; }
}


