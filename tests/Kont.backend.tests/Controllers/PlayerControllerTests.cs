using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.Services;
using Kont.backend.DAL;

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
    public async Task GetByLink_Returns_NotFound_When_Unknown()
    {
        _events.GetEventByLinkAsync("abc").Returns((Event?)null);
        var result = await _controller.GetByLink("abc");
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetByLink_Returns_Minimal_Info_When_Found()
    {
        var ev = new Event { Id = Guid.NewGuid(), Name = "My Event", EventLink = "abc", StartedAt = DateTime.UtcNow, EndedAt = DateTime.UtcNow.AddHours(1), Site = new Site { Id = Guid.NewGuid(), Name = "S", Address = "A", City = "C", ZipCode = "00000", Country = "FR", State = "ST", PhoneNumber = "0", Email = "s@s.com" }, Status = EventStatus.Active, CreatedBy = new Administrator { Id = Guid.NewGuid() } };
        _events.GetEventByLinkAsync("abc").Returns(ev);
        var result = await _controller.GetByLink("abc");
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        dynamic payload = ok.Value!;
        Assert.That((string)payload.name, Is.EqualTo("My Event"));
    }
}


