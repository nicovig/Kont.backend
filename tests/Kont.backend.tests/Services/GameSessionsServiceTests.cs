using Kont.backend.DAL;
using Kont.backend.Services;
using Kont.backend.tests.Tools;
using NSubstitute;
using Kont.backend.Models.Scoring;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

public class GameSessionsServiceTests : DatabaseTester
{
    private IGameSessionsService _service = null!;
    private IScoringService _scoring = null!;

    [SetUp]
    public void SetupService()
    {
        _scoring = Substitute.For<IScoringService>();
        _service = new GameSessionsService(_context, _scoring);
    }

    private async Task<(Event ev, Pool pool, Activity act1, Activity act2)> SeedBasicAsync(EventStatus evStatus = EventStatus.Pending, PoolStatus poolStatus = PoolStatus.Pending)
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "Z", Country = "FR", State = "ST", PhoneNumber = "0123456789", Email = "s@s.com" };
        var admin = await _context.Administrator.FirstAsync();
        var ev = new Event { Id = Guid.NewGuid(), Name = "E", EventLink = "L", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddDays(1), Site = site, Status = evStatus, CreatedBy = admin };
        var pool = new Pool { Id = Guid.NewGuid(), Name = "P", QrCode = "Q", Event = ev, Status = poolStatus, IsActive = poolStatus == PoolStatus.Active };
        var a1 = new Activity { Id = Guid.NewGuid(), Name = "A1", Site = site, CreatedBy = admin, PlayersPerGroupLimit = 2 };
        var a2 = new Activity { Id = Guid.NewGuid(), Name = "A2", Site = site, CreatedBy = admin, PlayersPerGroupLimit = 2 };
        await _context.Site.AddAsync(site);
        await _context.Event.AddAsync(ev);
        await _context.Pool.AddAsync(pool);
        await _context.Activity.AddRangeAsync(a1, a2);
        await _context.SaveChangesAsync();
        return (ev, pool, a1, a2);
    }

    [Test]
    public async Task Create_AddsGameSession()
    {
        var (ev, _, a1, _) = await SeedBasicAsync();
        var created = await _service.CreateAsync(ev.Id, a1.Id);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Status, Is.EqualTo(GameSessionStatus.Pending));
    }

    [Test]
    public async Task GetByEvent_ReturnsForEvent()
    {
        var (ev, _, a1, _) = await SeedBasicAsync();
        await _service.CreateAsync(ev.Id, a1.Id);
        var list = await _service.GetByEventAsync(ev.Id);
        Assert.That(list.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateStatus_ChangesStatus()
    {
        var (ev, _, a1, _) = await SeedBasicAsync();
        var created = await _service.CreateAsync(ev.Id, a1.Id);
        var updated = await _service.UpdateStatusAsync(created!.Id, GameSessionStatus.Active);
        Assert.That(updated!.Status, Is.EqualTo(GameSessionStatus.Active));
    }

    [Test]
    public async Task Delete_RemovesGameSession()
    {
        var (ev, _, a1, _) = await SeedBasicAsync();
        var created = await _service.CreateAsync(ev.Id, a1.Id);
        var ok = await _service.DeleteAsync(created!.Id);
        Assert.That(ok, Is.True);
        var found = await _context.GameSession.FindAsync(created.Id);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_SplitsByLimit()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        // Seed players
        var players = new List<PlayerRegistration>();
        for (int i = 0; i < 8; i++)
        {
            var p = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com", Password = "h", Username = $"u{i}" };
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = p, Pool = pool, RegisteredAt = DateTime.UtcNow };
            await _context.Player.AddAsync(p);
            await _context.PlayerRegistration.AddAsync(pr);
            players.Add(pr);
        }
        await _context.SaveChangesAsync();
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        pool.Status = PoolStatus.Active;
        await _context.SaveChangesAsync();
        var groups = await _service.GenerateGroupsWithoutScoresAsync(gs!.Id);
        Assert.That(groups, Is.Not.Null);
        var list = groups!.ToList();
        Assert.That(list.Count, Is.EqualTo((int)Math.Ceiling(8.0 / a1.PlayersPerGroupLimit)));
        Assert.That(list.Sum(g => g.Players.Count), Is.EqualTo(8));
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_DistributesEvenly_Test1()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        a1.PlayersPerGroupLimit = 8;
        // Seed 18 players
        for (int i = 0; i < 18; i++)
        {
            var p = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com", Password = "h", Username = $"u{i}" };
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = p, Pool = pool, RegisteredAt = DateTime.UtcNow };
            await _context.Player.AddAsync(p);
            await _context.PlayerRegistration.AddAsync(pr);
        }
        await _context.SaveChangesAsync();
        pool.Status = PoolStatus.Active;
        await _context.SaveChangesAsync();
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        var groups = await _service.GenerateGroupsWithoutScoresAsync(gs!.Id);
        Assert.That(groups, Is.Not.Null);
        var list = groups!.ToList();
        Assert.That(list.Count, Is.EqualTo(3));
        CollectionAssert.AreEquivalent(new[] { 6, 6, 6 }, list.Select(g => g.Players.Count));
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_DistributesEvenly_Test2()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        a1.PlayersPerGroupLimit = 5;
        // Seed 18 players
        for (int i = 0; i < 18; i++)
        {
            var p = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com", Password = "h", Username = $"u{i}" };
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = p, Pool = pool, RegisteredAt = DateTime.UtcNow };
            await _context.Player.AddAsync(p);
            await _context.PlayerRegistration.AddAsync(pr);
        }
        await _context.SaveChangesAsync();
        pool.Status = PoolStatus.Active;
        await _context.SaveChangesAsync();
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        var groups = await _service.GenerateGroupsWithoutScoresAsync(gs!.Id);
        Assert.That(groups, Is.Not.Null);
        var list = groups!.ToList();
        Assert.That(list.Count, Is.EqualTo(4));
        CollectionAssert.AreEquivalent(new[] { 5, 5, 4, 4 }, list.Select(g => g.Players.Count));
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_ThrowsIfSessionNotPending()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        pool.Status = PoolStatus.Active;
        gs!.Status = GameSessionStatus.Active;
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GenerateGroupsWithoutScoresAsync(gs.Id));
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_ThrowsIfPoolNotActive()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        pool.Status = PoolStatus.Pending;
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GenerateGroupsWithoutScoresAsync(gs!.Id));
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_ThrowsIfOtherSessionsNotClosed()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        pool.Status = PoolStatus.Active;
        var gs1 = await _service.CreateAsync(ev.Id, a1.Id);
        var gs2 = await _service.CreateAsync(ev.Id, a1.Id);

        gs1!.Status = GameSessionStatus.Active;
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GenerateGroupsWithoutScoresAsync(gs2!.Id));
        gs1.Status = GameSessionStatus.Completed;
        await _context.SaveChangesAsync();
        var ok = await _service.GenerateGroupsWithoutScoresAsync(gs2!.Id);
        Assert.That(ok, Is.Not.Null);
    }

    [Test]
    public async Task GenerateGroupsWithScores_OrdersByPercentage()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync();
        // Seed players
        var regs = new List<PlayerRegistration>();
        for (int i = 0; i < 5; i++)
        {
            var p = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com", Password = "h", Username = $"u{i}" };
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = p, Pool = pool };
            await _context.Player.AddAsync(p);
            await _context.PlayerRegistration.AddAsync(pr);
            regs.Add(pr);
        }
        await _context.SaveChangesAsync();
        pool.Status = PoolStatus.Active;
        var gs1 = await _service.CreateAsync(ev.Id, a1.Id);
        gs1!.Status = GameSessionStatus.Completed;
        await _context.SaveChangesAsync();

        // Seed PlayerActivityScore with percentages: p0=100, p1=80, p2=60, p3=40, p4=20
        for (int i = 0; i < regs.Count; i++)
        {
            _context.PlayerActivityScore.Add(new PlayerActivityScore
            {
                PlayerEntity = regs[i].Player,
                ActivityEntity = a1,
                PoolEntity = pool,
                TotalScore = 100 - i * 10,
                Percentage = 100 - i * 20,
                Rank = i + 1
            });
        }
        await _context.SaveChangesAsync();

        var gs2 = await _service.CreateAsync(ev.Id, a1.Id);
        // Mock scoring rankings descending by our seeded order
        var rankings = regs.Select((r, idx) => new Kont.backend.Models.Scoring.PlayerRanking
        {
            PlayerId = r.Player.Id,
            PlayerName = r.Player.Firstname,
            Username = r.Player.Username,
            GlobalScore = 100 - idx * 10,
            GlobalPercentage = 100 - idx * 20,
            GlobalRank = idx + 1
        }).ToList();
        _scoring.GetActivityRankingsAsync(pool.Id, a1.Id).Returns(Task.FromResult(rankings));
        var groups = await _service.GenerateGroupsWithScoresAsync(gs2!.Id);
        Assert.That(groups, Is.Not.Null);
        var list = groups!.ToList();
        // First group should contain the highest percentage players
        var firstGroup = list[0];
        var topUsernames = firstGroup.Players.Select(p => p.Player.Username).ToList();
        CollectionAssert.Contains(topUsernames, regs[0].Player.Username);
    }

    [Test]
    public async Task UpdateStatus_Active_requires_previous_closed()
    {
        var (_, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var s1 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Pending, CreatedAt = DateTime.UtcNow.AddMinutes(1) };
        var s2 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Pending, CreatedAt = DateTime.UtcNow.AddMinutes(2) };
        _context.GameSession.AddRange(s1, s2);
        await _context.SaveChangesAsync();

        // First can start
        var upd1 = await _service.UpdateStatusAsync(s1.Id, GameSessionStatus.Active);
        Assert.That(upd1!.Status, Is.EqualTo(GameSessionStatus.Active));

        // Second cannot start while first not closed
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateStatusAsync(s2.Id, GameSessionStatus.Active));

        // Complete first, then second can start
        await _service.UpdateStatusAsync(s1.Id, GameSessionStatus.Completed);
        var upd2 = await _service.UpdateStatusAsync(s2.Id, GameSessionStatus.Active);
        Assert.That(upd2!.Status, Is.EqualTo(GameSessionStatus.Active));
    }

    [Test]
    public async Task UpdateStatus_Completed_only_from_active()
    {
        var (_, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var s1 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Pending, CreatedAt = DateTime.UtcNow };
        _context.GameSession.Add(s1);
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateStatusAsync(s1.Id, GameSessionStatus.Completed));
        await _service.UpdateStatusAsync(s1.Id, GameSessionStatus.Active);
        var done = await _service.UpdateStatusAsync(s1.Id, GameSessionStatus.Completed);
        Assert.That(done!.Status, Is.EqualTo(GameSessionStatus.Completed));
    }

    [Test]
    public async Task UpdateTimes_blocked_when_event_or_pool_closed()
    {
        var (_, pool, a1, _) = await SeedBasicAsync(EventStatus.Completed, PoolStatus.Completed);
        var s1 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Pending, CreatedAt = DateTime.UtcNow };
        _context.GameSession.Add(s1);
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateStartTimeAsync(s1.Id, DateTime.UtcNow));
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateEndTimeAsync(s1.Id, DateTime.UtcNow));
    }

    [Test]
    public async Task UpdateEndTime_cannot_be_before_start()
    {
        var (_, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var s1 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Active, CreatedAt = DateTime.UtcNow, StartedAt = DateTime.UtcNow };
        _context.GameSession.Add(s1);
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateEndTimeAsync(s1.Id, s1.StartedAt!.Value.AddMinutes(-5)));
    }

    [Test]
    public async Task GenerateGroups_blocked_if_any_session_active()
    {
        var (_, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var s1 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Active, CreatedAt = DateTime.UtcNow };
        var s2 = new GameSession { Id = Guid.NewGuid(), Pool = pool, Activity = a1, Status = GameSessionStatus.Pending, CreatedAt = DateTime.UtcNow.AddMinutes(1) };
        _context.GameSession.AddRange(s1, s2);
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GenerateGroupsWithoutScoresAsync(s2.Id));
    }

    [Test]
    public async Task GetSessionScores_returns_scores_when_completed()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        gs!.Status = GameSessionStatus.Completed;
        await _context.SaveChangesAsync();
        _scoring.GetActivityRankingsAsync(pool.Id, a1.Id).Returns(Task.FromResult(new List<Kont.backend.Models.Scoring.PlayerRanking>() as List<Kont.backend.Models.Scoring.PlayerRanking>));
        var scores = await _service.GetSessionScoresAsync(gs.Id);
        Assert.That(scores, Is.Not.Null);
    }

    [Test]
    public async Task GetSessionScores_throws_when_not_completed()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var gs = await _service.CreateAsync(ev.Id, a1.Id);
        gs!.Status = GameSessionStatus.Active;
        await _context.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetSessionScoresAsync(gs.Id));
    }

    [Test]
    public async Task GetGroups_returns_groups_with_players()
    {
        var (ev, pool, a1, _) = await SeedBasicAsync(EventStatus.Pending, PoolStatus.Active);
        var s = await _service.CreateAsync(ev.Id, a1.Id);
        var players = new List<PlayerRegistration>();
        for (int i = 0; i < 3; i++)
        {
            var p = new Player { Id = Guid.NewGuid(), Firstname = $"P{i}", Lastname = "L", Email = $"p{i}@x.com", Password = "h", Username = $"u{i}" };
            var pr = new PlayerRegistration { Id = Guid.NewGuid(), Player = p, Pool = pool, RegisteredAt = DateTime.UtcNow };
            await _context.Player.AddAsync(p);
            await _context.PlayerRegistration.AddAsync(pr);
            players.Add(pr);
        }
        await _context.SaveChangesAsync();
        var groups = await _service.GenerateGroupsWithoutScoresAsync(s!.Id);
        Assert.That(groups, Is.Not.Null);
        var fetched = await _service.GetGroupsAsync(s!.Id);
        Assert.That(fetched, Is.Not.Null);
        var list = fetched!.ToList();
        Assert.That(list.Count, Is.GreaterThan(0));
        Assert.That(list[0].Players.Count, Is.GreaterThan(0));
        Assert.That(list[0].Players[0].Player.Username, Is.Not.Null);
    }

    [Test]
    public async Task GetGroups_returns_null_when_session_not_found()
    {
        var res = await _service.GetGroupsAsync(Guid.NewGuid());
        Assert.That(res, Is.Null);
    }
}


