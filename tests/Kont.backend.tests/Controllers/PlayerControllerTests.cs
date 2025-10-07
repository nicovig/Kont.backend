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
    private PlayerController _controller;

    [SetUp]
    public void Setup()
    {
        _events = Substitute.For<IEventsService>();
        _controller = new PlayerController(_events);
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
}


