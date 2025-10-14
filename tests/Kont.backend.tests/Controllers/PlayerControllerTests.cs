using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.Models.Request;
using Kont.backend.Services;
using Kont.backend.DAL;
using Kont.backend.tests.Tools;
using Kont.backend.Models.Response;
// using kept minimal; fully qualify types below

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class PlayerControllerTests : DatabaseTester
{
    private IEventsService _events;
    private PlayerController _controller;

    [SetUp]
    public void Setup()
    {
        _events = Substitute.For<IEventsService>();
        _controller = new PlayerController(_events, _context);
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Unknown()
    {
        var id = Guid.NewGuid();
        _events.GetEventByIdAsync(id).Returns((Event?)null);
        var result = await _controller.GetEventById(id);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_Minimal_Info_When_Found()
    {
        var id = Guid.NewGuid();
        var ev = new Event { Id = id, Name = "My Event", EventLink = "abc", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" }, Status = EventStatus.Active, CreatedBy = new Administrator { Id = Guid.NewGuid() } };
        _events.GetEventByIdAsync(id).Returns(ev);
        var result = await _controller.GetEventById(id);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var value = ok.Value as PlayerEventInfoResponse;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Name, Is.EqualTo("My Event"));
        Assert.That(value!.Location, Does.Contain("S"));
    }

    [Test]
    public async Task Register_Creates_PlayerRegistration_And_Returns_Created()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "Adm", Lastname = "In", Email = "adm@e.com", Password = "p", IsActive = true, PhoneNumber = "0", Role = new Role { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, RoleType = RoleType.Admin }, Subscription = new Subscription { Id = Guid.NewGuid(), ExpiresAt = DateTime.UtcNow.AddDays(1), PaidAt = DateTime.UtcNow, SubscriptionType = SubscriptionType.Klasel } };
        var ev = new Event { Id = eventId, Name = "Evt", EventLink = "abc", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), Site = site, Status = EventStatus.Active, CreatedBy = admin };
        await _context.Event.AddAsync(ev);
        await _context.SaveChangesAsync();
        var pool = new Pool { Id = poolId, Name = "P1", QrCode = "QR", Event = ev };
        await _context.Pool.AddAsync(pool);
        await _context.SaveChangesAsync();
        ev.Pools = new List<Pool> { pool };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        var req = new PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "1234" };

        var result = await _controller.Register(eventId, poolId, req);

        Assert.That(result, Is.InstanceOf<CreatedResult>());
    }

    [Test]
    public async Task Register_Returns_Conflict_When_Email_Exists()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Pools = new List<Pool> { new Pool { Id = poolId } }, Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" } };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        await _context.Player.AddAsync(new Player { Id = Guid.NewGuid(), Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "any", Password = "x" });
        await _context.SaveChangesAsync();

        var req = new PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "12345" };
        var result = await _controller.Register(eventId, poolId, req);
        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task Register_Returns_Conflict_When_Username_Exists()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Pools = new List<Pool> { new Pool { Id = poolId } }, Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" } };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        await _context.Player.AddAsync(new Player { Id = Guid.NewGuid(), Firstname = "John", Lastname = "Doe", Email = "any@doe.com", Username = "johnd", Password = "x" });
        await _context.SaveChangesAsync();

        var req = new PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "12345" };
        var result = await _controller.Register(eventId, poolId, req);
        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public void Login_Returns_BadRequest_When_Invalid_Payload()
    {
        var result = _controller.Login(new PlayerLoginRequest { Identifier = "", Pin = "" });
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void Login_Returns_Unauthorized_When_Not_Found()
    {
        // no player seeded => not found
        var result = _controller.Login(new PlayerLoginRequest { Identifier = "a@a.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_Returns_Unauthorized_When_Bad_Pin()
    {
        var p = new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Username = "u", Password = "00000" };
        await _context.Player.AddAsync(p);
        await _context.SaveChangesAsync();
        var result = _controller.Login(new PlayerLoginRequest { Identifier = "e@e.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_Returns_Ok_When_Credentials_Valid()
    {
        var p = new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Username = "u", Password = "12345" };
        await _context.Player.AddAsync(p);
        await _context.SaveChangesAsync();
        var result = _controller.Login(new PlayerLoginRequest { Identifier = "e@e.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task CheckEmail_Returns_Availability()
    {
        await _context.Player.AddAsync(new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Username = "ux", Password = "p" });
        await _context.SaveChangesAsync();
        var r1 = _controller.CheckEmail("e@e.com") as OkObjectResult;
        var r2 = _controller.CheckEmail("new@e.com") as OkObjectResult;
        Assert.That(r1, Is.Not.Null);
        Assert.That(r2, Is.Not.Null);
    }

    [Test]
    public async Task CheckUsername_Returns_Availability()
    {
        await _context.Player.AddAsync(new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "ux@e.com", Username = "u", Password = "p" });
        await _context.SaveChangesAsync();
        var r1 = _controller.CheckUsername("u") as OkObjectResult;
        var r2 = _controller.CheckUsername("newu") as OkObjectResult;
        Assert.That(r1, Is.Not.Null);
        Assert.That(r2, Is.Not.Null);
    }
}


