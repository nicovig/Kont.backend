using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Kont.backend.Models.Response;
using Kont.backend.DAL;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class EventsControllerTests
{
    private IEventsService _events;
    private IUserContextService _userCtx;
    private IEventInvitationService _eventInvitation;
    private ILogger<EventsController> _logger;
    private EventsController _controller;

    [SetUp]
    public void Setup()
    {
        _events = Substitute.For<IEventsService>();
        _userCtx = Substitute.For<IUserContextService>();
        _eventInvitation = Substitute.For<IEventInvitationService>();
        _logger = Substitute.For<ILogger<EventsController>>();
        _controller = new EventsController(_events, _userCtx, _eventInvitation, _logger);
    }

    [Test]
    public async Task GetById_NotFound()
    {
        var id = Guid.NewGuid();
        _events.GetEventByIdAsync(id).Returns((Event?)null);
        var result = await _controller.GetById(id);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Create_ReturnsUnauthorized_WhenNoUser()
    {
        _userCtx.GetCurrentUser().Returns((Administrator?)null);
        var req = new CreateEventRequest { Name = "N", EventLink = "L", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), SiteId = Guid.NewGuid() };
        var result = await _controller.Create(req);
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task UpdateEventAllPlayersPresent_ReturnsOkOrNotFound()
    {
        var eventId = Guid.NewGuid();
        _events.UpdateEventAllPlayersPresentAsync(eventId, true).Returns(new Event { Id = eventId, Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" }, Pools = new List<Pool> { new Pool { Id = Guid.NewGuid(), Name = "P", QrCode = "q", IsAllPlayersPresent = true } } });
        var ok = await _controller.UpdateEventAllPlayersPresent(eventId, true);
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _events.UpdateEventAllPlayersPresentAsync(eventId, true).Returns((Event?)null);
        var nf = await _controller.UpdateEventAllPlayersPresent(eventId, true);
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task SendQRCodeToEmailList_ReturnsOk_WhenValidRequest()
    {
        var eventId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var emails = new List<string> { "test@example.com", "test2@example.com" };

        _userCtx.GetCurrentUser().Returns(new Administrator { Id = adminId, Firstname = "Test", Lastname = "User", Email = "admin@test.com", Password = "pwd", PhoneNumber = "123", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } });

        var result = await _controller.SendQRCodeToEmailList(eventId, emails);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(200));
        await _eventInvitation.Received(1).SendQRCodeToEmailListAsync(eventId, emails, adminId);
    }

    [Test]
    public async Task SendQRCodeToEmailList_ReturnsUnauthorized_WhenNoUser()
    {
        var eventId = Guid.NewGuid();
        var emails = new List<string> { "test@example.com" };

        _userCtx.GetCurrentUser().Returns((Administrator?)null);

        var result = await _controller.SendQRCodeToEmailList(eventId, emails);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task SendQRCodeToEmailList_ReturnsBadRequest_WhenArgumentException()
    {
        var eventId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var emails = new List<string> { "test@example.com" };

        _userCtx.GetCurrentUser().Returns(new Administrator { Id = adminId, Firstname = "Test", Lastname = "User", Email = "admin@test.com", Password = "pwd", PhoneNumber = "123", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } });
        _eventInvitation.SendQRCodeToEmailListAsync(eventId, emails, adminId).Returns(Task.FromException(new ArgumentException("Event not found")));

        var result = await _controller.SendQRCodeToEmailList(eventId, emails);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SendQRCodeToEmailList_ReturnsBadRequest_WhenEmptyEmailList()
    {
        var eventId = Guid.NewGuid();
        var emails = new List<string>();

        var result = await _controller.SendQRCodeToEmailList(eventId, emails);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetPlayerRegistrations_ReturnsOk_WhenValidEvent()
    {
        var eventId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var registrations = new List<PlayerRegistrationResponse>
        {
            new() { Id = Guid.NewGuid(), PlayerFirstname = "John", PlayerLastname = "Doe", PlayerEmail = "john@test.com", PlayerUsername = "john_doe", PlayerType = PlayerType.Player, RegisteredAt = DateTime.UtcNow }
        };

        _userCtx.GetCurrentUser().Returns(new Administrator { Id = adminId, Firstname = "Test", Lastname = "User", Email = "admin@test.com", Password = "pwd", PhoneNumber = "123", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } });
        _events.GetPlayerRegistrationsByEventAsync(eventId).Returns(Task.FromResult<IEnumerable<PlayerRegistrationResponse>>(registrations));

        var result = await _controller.GetPlayerRegistrations(eventId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(200));
        await _events.Received(1).GetPlayerRegistrationsByEventAsync(eventId);
    }

    [Test]
    public async Task GetPlayerRegistrations_ReturnsUnauthorized_WhenNoUser()
    {
        var eventId = Guid.NewGuid();

        _userCtx.GetCurrentUser().Returns((Administrator?)null);

        var result = await _controller.GetPlayerRegistrations(eventId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetPlayerRegistrations_ReturnsNotFound_WhenArgumentException()
    {
        var eventId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        _userCtx.GetCurrentUser().Returns(new Administrator { Id = adminId, Firstname = "Test", Lastname = "User", Email = "admin@test.com", Password = "pwd", PhoneNumber = "123", Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Stroll } });
        _events.GetPlayerRegistrationsByEventAsync(eventId).Returns(Task.FromException<IEnumerable<PlayerRegistrationResponse>>(new ArgumentException("Event not found")));

        var result = await _controller.GetPlayerRegistrations(eventId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task UpdatePlayerIsPresent_returns_ok_or_notfound()
    {
        _userCtx.GetCurrentUser().Returns(new Administrator { Id = Guid.NewGuid(), Firstname = "A", Lastname = "B", Email = "a@a.com", Password = "p", PhoneNumber = "0", Role = new Role { RoleType = RoleType.Admin }, IsActive = true, Subscription = new Subscription { SubscriptionType = SubscriptionType.Stroll } });
        var ev = new Event { Id = Guid.NewGuid(), Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" } };
        _events.UpdateEventPlayerIsPresentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>()).Returns(ev);
        var ok = await _controller.UpdatePlayerIsPresent(Guid.NewGuid(), Guid.NewGuid(), true);
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _events.UpdateEventPlayerIsPresentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>()).Returns((Event?)null);
        var nf = await _controller.UpdatePlayerIsPresent(Guid.NewGuid(), Guid.NewGuid(), true);
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }
}


