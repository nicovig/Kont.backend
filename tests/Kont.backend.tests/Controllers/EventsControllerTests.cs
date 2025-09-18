using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Kont.backend.DAL;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class EventsControllerTests
{
    private IEventsService _events;
    private IUserContextService _userCtx;
    private ILogger<EventsController> _logger;
    private EventsController _controller;

    [SetUp]
    public void Setup()
    {
        _events = Substitute.For<IEventsService>();
        _userCtx = Substitute.For<IUserContextService>();
        _logger = Substitute.For<ILogger<EventsController>>();
        _controller = new EventsController(_events, _userCtx, _logger);
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
}


