using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Tools;

public static class Administrators
{
    internal static TestUser First { get; } = new() { Id = Guid.Parse("00000000-0000-0000-0024-000000000000"), Email = "first@kont.fr" };
    internal static TestUser Second { get; } = new() { Id = Guid.Parse("00000000-0000-0000-0012-000000000000"), Email = "second@kont.fr" };
}

public class TestUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
}

public class DatabaseForTests
{
    public DatabaseContext Context { get; }

    internal DatabaseForTests(DatabaseContext context) => Context = context;

    internal async Task Empty()
    {
        await Context.Administrator.AddRangeAsync(new[] { Administrators.First, Administrators.Second }
                                    .Select(e => new Administrator
                                    {
                                        Id = e.Id,
                                        Email = e.Email,
                                        IsActive = true,
                                        Role = new Role { Id = new Guid(), CreatedAt = new DateTime(), RoleType = RoleType.Admin },
                                        Subscription = new Subscription { Id = new Guid(), ExpiresAt = new DateTime().AddDays(365), PaidAt = new DateTime(), SubscriptionType = SubscriptionType.Klasel }
                                    } ));
        await Context.SaveChangesAsync();
    }

    public void Dispose() => Context.Dispose();
}