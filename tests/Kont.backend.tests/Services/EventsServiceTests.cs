using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Request;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

[TestFixture]
public class EventsServiceTests
{
    private IDatabaseContext _context = null!;
    private EventsService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _context = new DatabaseContext(options, appSettings);
        _service = new EventsService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        if (_context is IDisposable d) d.Dispose();
    }

    [Test]
    public async Task CreateEvent_Succeeds_WithExistingSiteAndAdmin()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "X", Address = "1", City = "P", ZipCode = "75000", Country = "FR", State = "IDF", PhoneNumber = "01", Email = "s@e.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "J", Lastname = "D", Email = "a@b.c", Password = "x", PhoneNumber = "01", SubscriptionId = Guid.NewGuid(), Sites = new(), Role = new Role { RoleType = RoleType.Admin }, IsActive = true };
        _context.Site.Add(site);
        _context.Administrator.Add(admin);
        await (_context as DatabaseContext)!.SaveChangesAsync();

        var req = new CreateEventRequest { Name = "E1", EventLink = "/l", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id };
        var ev = await _service.CreateEventAsync(req, admin.Id);
        Assert.That(ev.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(ev.Site.Id, Is.EqualTo(site.Id));
        Assert.That(ev.CreatedBy.Id, Is.EqualTo(admin.Id));
    }
}


