using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Request;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

public class EventsServiceTests
{
    private DatabaseContext _db = null!;
    private IEventsService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _db = new DatabaseContext(options, appSettings);
        _service = new EventsService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task CreateEvent_CreatesEventAndPool()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();

        var req = new CreateEventRequest
        {
            Name = "EV",
            EventLink = "link",
            StartedAt = DateTime.UtcNow,
            EndedAt = DateTime.UtcNow.AddHours(1),
            SiteId = site.Id,
            Status = EventStatus.Pending,
            ActivityIds = new List<Guid>()
        };

        var ev = await _service.CreateEventAsync(req, admin.Id);

        Assert.That(ev.Id, Is.Not.EqualTo(Guid.Empty));
        var pool = await _db.Pool.FirstOrDefaultAsync(p => p.Event.Id == ev.Id);
        Assert.That(pool, Is.Not.Null);
        Assert.That(pool!.Name, Is.EqualTo(ev.Name));
    }

    [Test]
    public async Task GetEvents_ReturnsList()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();
        var req = new CreateEventRequest { Name = "EV", EventLink = "link", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() };
        await _service.CreateEventAsync(req, admin.Id);

        var list = await _service.GetEventsAsync();
        Assert.That(list.Count(), Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task UpdateEvent_UpdatesAndSyncsPool()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();
        var created = await _service.CreateEventAsync(new CreateEventRequest { Name = "EV", EventLink = "link", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() }, admin.Id);

        var upd = new UpdateEventRequest
        {
            Id = created.Id,
            Name = "EV2",
            EventLink = "link2",
            StartedAt = DateTime.UtcNow.AddDays(1),
            EndedAt = DateTime.UtcNow.AddDays(1).AddHours(1),
            Status = EventStatus.Active,
            ActivityIds = new List<Guid>(),
            SiteId = site.Id
        };

        var updated = await _service.UpdateEventAsync(created.Id, upd);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("EV2"));
        var pool = await _db.Pool.FirstOrDefaultAsync(p => p.Event.Id == updated.Id);
        Assert.That(pool, Is.Not.Null);
        Assert.That(pool!.Name, Is.EqualTo("EV2"));
    }

    [Test]
    public async Task DeleteEvent_RemovesEvent()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();
        var created = await _service.CreateEventAsync(new CreateEventRequest { Name = "EV", EventLink = "link", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() }, admin.Id);

        var ok = await _service.DeleteEventAsync(created.Id);
        Assert.That(ok, Is.True);
        var ev = await _db.Event.FindAsync(created.Id);
        Assert.That(ev, Is.Null);
    }

    [Test]
    public async Task UpdateEventAllPlayersPresent_UpdatesFlag()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();

        var created = await _service.CreateEventAsync(new CreateEventRequest { Name = "EV", EventLink = "link", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() }, admin.Id);

        var updated = await _service.UpdateEventAllPlayersPresentAsync(created.Id, true);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Pools[0].IsAllPlayersPresent, Is.True);
    }

    [Test]
    public async Task CreateEvent_Succeeds_WithExistingSiteAndAdmin()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "X", Address = "1", City = "P", ZipCode = "75000", Country = "FR", State = "IDF", PhoneNumber = "01", Email = "s@e.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "J", Lastname = "D", Email = "a@b.c", Password = "x", PhoneNumber = "01", SubscriptionId = Guid.NewGuid(), Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll }, Sites = new(), Role = new Role { RoleType = RoleType.Admin }, IsActive = true };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await (_db as DatabaseContext)!.SaveChangesAsync();

        var req = new CreateEventRequest { Name = "E1", EventLink = "/l", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id };
        var ev = await _service.CreateEventAsync(req, admin.Id);
        Assert.That(ev.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(ev.Site.Id, Is.EqualTo(site.Id));
        Assert.That(ev.CreatedBy.Id, Is.EqualTo(admin.Id));
    }

    [Test]
    public async Task CreateEvent_Throws_WhenSubscriptionLimitReached_Esae()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Esae } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();

        var req = new CreateEventRequest { Name = "E1", EventLink = "link1", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() };
        await _service.CreateEventAsync(req, admin.Id);

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var req2 = new CreateEventRequest { Name = "E2", EventLink = "link2", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(2), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() };
            await _service.CreateEventAsync(req2, admin.Id);
        });
        Assert.That(ex!.Message, Does.Contain("maximum number of events"));
    }

    [Test]
    public async Task CreateEvent_Throws_WhenSubscriptionLimitReached_Deraou()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Deraou } };
        _db.Site.Add(site);
        _db.Administrator.Add(admin);
        await _db.SaveChangesAsync();

        for (int i = 0; i < 4; i++)
        {
            var req = new CreateEventRequest { Name = $"E{i}", EventLink = $"link{i}", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() };
            await _service.CreateEventAsync(req, admin.Id);
        }

        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var req5 = new CreateEventRequest { Name = "E5", EventLink = "link5", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = site.Id, Status = EventStatus.Pending, ActivityIds = new List<Guid>() };
            await _service.CreateEventAsync(req5, admin.Id);
        });
        Assert.That(ex!.Message, Does.Contain("maximum number of events"));
    }
}


