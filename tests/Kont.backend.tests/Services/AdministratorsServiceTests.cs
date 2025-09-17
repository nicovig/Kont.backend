using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Kont.backend.tests.Services;

public class AdministratorsServiceTests
{
    private DatabaseContext _db = null!;
    private IAdministratorsService _service = null!;
    private IPasswordService _passwordService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _db = new DatabaseContext(options, appSettings);
        _passwordService = Substitute.For<IPasswordService>();
        _passwordService.HashPassword(Arg.Any<string>()).Returns("hashed_password");
        _service = new AdministratorsService(_db, _passwordService);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task CreateAdministrator_AutoCreatesSubscription_AndLinksSites()
    {
        var role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin };
        _db.Role.Add(role);
        var s1 = new Site { Id = Guid.NewGuid(), Name = "S1", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "s1@a.com" };
        var s2 = new Site { Id = Guid.NewGuid(), Name = "S2", Address = "Adr2", City = "City2", ZipCode = "75002", Country = "FR", State = "IDF", PhoneNumber = "0102030406", Email = "s2@a.com" };
        _db.Site.AddRange(s1, s2);
        await _db.SaveChangesAsync();

        var created = await _service.CreateAdministratorAsync(new CreateAdministratorRequest
        {
            Firstname = "A",
            Lastname = "B",
            Email = "a@b.com",
            Password = "p",
            PhoneNumber = "0102030405",
            Role = role,
            IsActive = true,
            Sites = new List<Site> { new Site { Id = s1.Id }, new Site { Id = s2.Id } },
            SubscriptionType = SubscriptionType.Stroll
        });

        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.Subscription, Is.Not.Null);
        Assert.That(created.Sites.Count, Is.EqualTo(2));
        Assert.That(created.Password, Is.EqualTo("hashed_password"));
        _passwordService.Received(1).HashPassword("p");
    }

    [Test]
    public async Task UpdateAdministrator_UpdatesRole_Subscription_AndSites()
    {
        var roleAdmin = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin };
        var roleManager = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Manager };
        _db.Role.AddRange(roleAdmin, roleManager);
        var s1 = new Site { Id = Guid.NewGuid(), Name = "S1", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "s1@a.com" };
        var s2 = new Site { Id = Guid.NewGuid(), Name = "S2", Address = "Adr2", City = "City2", ZipCode = "75002", Country = "FR", State = "IDF", PhoneNumber = "0102030406", Email = "s2@a.com" };
        var s3 = new Site { Id = Guid.NewGuid(), Name = "S3", Address = "Adr3", City = "City3", ZipCode = "75003", Country = "FR", State = "IDF", PhoneNumber = "0102030407", Email = "s3@a.com" };
        _db.Site.AddRange(s1, s2, s3);
        await _db.SaveChangesAsync();

        var created = await _service.CreateAdministratorAsync(new CreateAdministratorRequest
        {
            Firstname = "A",
            Lastname = "B",
            Email = "a@b.com",
            Password = "p",
            PhoneNumber = "0102030405",
            Role = roleAdmin,
            IsActive = true,
            Sites = new List<Site> { new Site { Id = s1.Id }, new Site { Id = s2.Id } },
            SubscriptionType = SubscriptionType.Stroll
        });

        var updated = await _service.UpdateAdministratorAsync(created.Id, new Administrator
        {
            Firstname = "A2",
            Lastname = "B2",
            Email = "a2@b.com",
            Password = "p2",
            PhoneNumber = "0102030406",
            Role = new Role { Id = roleManager.Id },
            IsActive = false,
            Subscription = new Subscription { SubscriptionType = SubscriptionType.Stroll, PaidAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(60) },
            Sites = new List<Site> { new Site { Id = s3.Id } }
        });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role.RoleType, Is.EqualTo(RoleType.Manager));
        Assert.That(updated.Subscription, Is.Not.Null);
        Assert.That(updated.Subscription.SubscriptionType, Is.EqualTo(SubscriptionType.Stroll));
        Assert.That(updated.Sites.Count, Is.EqualTo(1));
        Assert.That(updated.Sites[0].Id, Is.EqualTo(s3.Id));
        Assert.That(updated.Password, Is.EqualTo("hashed_password"));
        _passwordService.Received(2).HashPassword(Arg.Any<string>());
    }
}


