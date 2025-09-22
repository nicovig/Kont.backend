using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

[TestFixture]
public class ActivitiesServiceTests
{
    private IDatabaseContext _context = null!;
    private ActivitiesService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _context = new DatabaseContext(options, appSettings);
        _service = new ActivitiesService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        if (_context is IDisposable d)
        {
            d.Dispose();
        }
    }

    [Test]
    public void CreateActivityAsync_Throws_WhenCoefficientsDoNotSumToOne()
    {
        var admin = new Administrator
        {
            Id = Guid.NewGuid(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "a@b.c",
            Password = "hashedpwd",
            PhoneNumber = "0000000000",
            Role = new Role { RoleType = RoleType.Admin },
            SubscriptionId = Guid.NewGuid(),
            Sites = new List<Site>(),
            IsActive = true
        };
        _context.Administrator.Add(admin);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "X",
            Address = "1 rue",
            City = "Paris",
            ZipCode = "75000",
            Country = "FR",
            State = "IDF",
            PhoneNumber = "0102030405",
            Email = "site@example.com"
        };
        _context.Site.Add(site);

        var activity = new Activity
        {
            Name = "Karting",
            Description = "GP",
            Site = site,
            PlayersPerGroupLimit = 4,
            ScoringMetrics = new List<ScoringMetric>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 0.7 },
                new() { Name = "Clues", Unit = null, HigherIsBetter = false, Coefficient = 0.2 }
            },
            CreatedBy = admin
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await _service.CreateActivityAsync(activity));
    }

    [Test]
    public async Task CreateActivityAsync_Succeeds_WhenCoefficientsSumToOne()
    {
        var admin = new Administrator
        {
            Id = Guid.NewGuid(),
            Firstname = "Jane",
            Lastname = "Roe",
            Email = "a@b.c",
            Password = "hashedpwd",
            PhoneNumber = "0000000000",
            Role = new Role { RoleType = RoleType.Admin },
            SubscriptionId = Guid.NewGuid(),
            Sites = new List<Site>(),
            IsActive = true
        };
        _context.Administrator.Add(admin);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "X",
            Address = "1 rue",
            City = "Paris",
            ZipCode = "75000",
            Country = "FR",
            State = "IDF",
            PhoneNumber = "0102030405",
            Email = "site@example.com"
        };
        _context.Site.Add(site);

        await (_context as DatabaseContext)!.SaveChangesAsync();

        var activity = new Activity
        {
            Name = "Karting",
            Description = "GP",
            Site = site,
            PlayersPerGroupLimit = 4,
            ScoringMetrics = new List<ScoringMetric>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 0.4 },
                new() { Name = "Clues", Unit = null, HigherIsBetter = false, Coefficient = 0.6 }
            },
            CreatedBy = admin
        };

        var created = await _service.CreateActivityAsync(activity);
        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
    }
}


