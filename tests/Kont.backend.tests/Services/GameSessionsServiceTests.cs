using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

public class GameSessionsServiceTests
{
    private DatabaseContext _db = null!;
    private IGameSessionsService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _db = new DatabaseContext(options, appSettings);
        _service = new GameSessionsService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    private async Task<(Event ev, Pool pool, Activity act)> SeedEventActivityAsync(int players = 0)
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        _db.Site.Add(site);
        var ev = new Event { Id = Guid.NewGuid(), Name = "EV", EventLink = "link", Site = site, StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), Status = EventStatus.Pending, CreatedAt = DateTime.UtcNow, CreatedBy = new Administrator { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Password = "p", PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true } };
        _db.Event.Add(ev);
        var pool = new Pool { Id = Guid.NewGuid(), Name = ev.Name, QrCode = ev.EventLink, Event = ev, StartedAt = ev.StartedAt, EndedAt = ev.EndedAt, Status = PoolStatus.Pending, IsActive = true };
        _db.Pool.Add(pool);
        var act = new Activity { Id = Guid.NewGuid(), Name = "A", PlayersPerGroupLimit = 3 };
        _db.Activity.Add(act);
        for (int i = 0; i < players; i++)
        {
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com" }, Pool = pool };
            _db.PlayerRegistration.Add(pr);
            pool.PlayerRegistrations.Add(pr);
        }
        await _db.SaveChangesAsync();
        return (ev, pool, act);
    }

    [Test]
    public async Task Create_AddsGameSession()
    {
        var (ev, _, act) = await SeedEventActivityAsync();
        var created = await _service.CreateAsync(ev.Id, act.Id);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Status, Is.EqualTo(GameSessionStatus.Pending));
    }

    [Test]
    public async Task GetByEvent_ReturnsForEvent()
    {
        var (ev, _, act) = await SeedEventActivityAsync();
        await _service.CreateAsync(ev.Id, act.Id);
        var list = await _service.GetByEventAsync(ev.Id);
        Assert.That(list.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateStatus_ChangesStatus()
    {
        var (ev, _, act) = await SeedEventActivityAsync();
        var created = await _service.CreateAsync(ev.Id, act.Id);
        var updated = await _service.UpdateStatusAsync(created!.Id, GameSessionStatus.Active);
        Assert.That(updated!.Status, Is.EqualTo(GameSessionStatus.Active));
    }

    [Test]
    public async Task Delete_RemovesGameSession()
    {
        var (ev, _, act) = await SeedEventActivityAsync();
        var created = await _service.CreateAsync(ev.Id, act.Id);
        var ok = await _service.DeleteAsync(created!.Id);
        Assert.That(ok, Is.True);
        var found = await _db.GameSession.FindAsync(created.Id);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_SplitsByLimit()
    {
        var (ev, pool, act) = await SeedEventActivityAsync(players: 8);
        var gs = await _service.CreateAsync(ev.Id, act.Id);
        var groups = await _service.GenerateGroupsWithoutScoresAsync(gs!.Id);
        Assert.That(groups, Is.Not.Null);
        var list = groups!.ToList();
        Assert.That(list.Count, Is.EqualTo((int)Math.Ceiling(8.0 / act.PlayersPerGroupLimit)));
        Assert.That(list.Sum(g => g.Players.Count), Is.EqualTo(8));
    }
}


