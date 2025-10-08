using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.Services;
using Kont.backend.DAL;
// using kept minimal; fully qualify types below

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class PlayerControllerTests
{
    private IEventsService _events;
    private Kont.backend.DAL.DatabaseContext.IDatabaseContext _context;
    private PlayerController _controller;

    [SetUp]
    public void Setup()
    {
        _events = Substitute.For<IEventsService>();
        _context = Substitute.For<Kont.backend.DAL.DatabaseContext.IDatabaseContext>();
        _controller = new PlayerController(_events, _context);
    }

    [Test]
    public async Task GetById_Returns_NotFound_When_Unknown()
    {
        var id = Guid.NewGuid();
        _events.GetEventByIdAsync(id).Returns((Event?)null);
        var result = await _controller.GetById(id);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetById_Returns_Minimal_Info_When_Found()
    {
        var id = Guid.NewGuid();
        var ev = new Event { Id = id, Name = "My Event", EventLink = "abc", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" }, Status = EventStatus.Active, CreatedBy = new Administrator { Id = Guid.NewGuid() } };
        _events.GetEventByIdAsync(id).Returns(ev);
        var result = await _controller.GetById(id);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var value = ok.Value as Kont.backend.Models.Response.PlayerEventInfoResponse;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Name, Is.EqualTo("My Event"));
        Assert.That(value!.Location, Does.Contain("S"));
    }

    [Test]
    public async Task Register_Creates_PlayerRegistration_And_Returns_Created()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Pools = new List<Pool> { new Pool { Id = poolId } }, Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" } };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        var req = new Kont.backend.Models.Request.PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "1234" };

        var result = await _controller.Register(eventId, poolId, req);

        Assert.That(result, Is.InstanceOf<CreatedResult>());
        await _context.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Register_Returns_Conflict_When_Email_Exists()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Pools = new List<Pool> { new Pool { Id = poolId } } };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        _context.Player.Any(p => p.Email == "john@doe.com").Returns(true);

        var req = new Kont.backend.Models.Request.PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "12345" };
        var result = await _controller.Register(eventId, poolId, req);
        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task Register_Returns_Conflict_When_Username_Exists()
    {
        var eventId = Guid.NewGuid();
        var poolId = Guid.NewGuid();
        var ev = new Event { Id = eventId, Pools = new List<Pool> { new Pool { Id = poolId } } };
        _events.GetEventByIdAsync(eventId).Returns(ev);

        _context.Player.Any(p => p.Username == "johnd").Returns(true);

        var req = new Kont.backend.Models.Request.PlayerRegisterRequest { Firstname = "John", Lastname = "Doe", Email = "john@doe.com", Username = "johnd", Pin = "12345" };
        var result = await _controller.Register(eventId, poolId, req);
        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public void Login_Returns_BadRequest_When_Invalid_Payload()
    {
        var result = _controller.Login(new PlayerController.PlayerLoginRequest { Identifier = "", Pin = "" });
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void Login_Returns_Unauthorized_When_Not_Found()
    {
        _context.Player.FirstOrDefault(x => x.Email == "a@a.com" || x.Username == "aa").Returns((Player?)null);
        var result = _controller.Login(new PlayerController.PlayerLoginRequest { Identifier = "a@a.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public void Login_Returns_Unauthorized_When_Bad_Pin()
    {
        var p = new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Username = "u", Password = "00000" };
        _context.Player.FirstOrDefault(x => x.Email == "e@e.com" || x.Username == "u").Returns(p);
        var result = _controller.Login(new PlayerController.PlayerLoginRequest { Identifier = "e@e.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public void Login_Returns_Ok_When_Credentials_Valid()
    {
        var p = new Player { Id = Guid.NewGuid(), Firstname = "F", Lastname = "L", Email = "e@e.com", Username = "u", Password = "12345" };
        _context.Player.FirstOrDefault(x => x.Email == "e@e.com" || x.Username == "u").Returns(p);
        var result = _controller.Login(new PlayerController.PlayerLoginRequest { Identifier = "e@e.com", Pin = "12345" });
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void CheckEmail_Returns_Availability()
    {
        _context.Player.Any(p => p.Email == "e@e.com").Returns(true);
        var r1 = _controller.CheckEmail("e@e.com") as OkObjectResult;
        var r2 = _controller.CheckEmail("new@e.com") as OkObjectResult;
        Assert.That(r1, Is.Not.Null);
        Assert.That(r2, Is.Not.Null);
    }

    [Test]
    public void CheckUsername_Returns_Availability()
    {
        _context.Player.Any(p => p.Username == "u").Returns(true);
        var r1 = _controller.CheckUsername("u") as OkObjectResult;
        var r2 = _controller.CheckUsername("newu") as OkObjectResult;
        Assert.That(r1, Is.Not.Null);
        Assert.That(r2, Is.Not.Null);
    }
}


