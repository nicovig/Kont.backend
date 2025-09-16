using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kont.backend.tests.Controllers;

public class SubscriptionsControllerTests
{
    private ISubscriptionsService _service = null!;
    private ILogger<SubscriptionsController> _logger = null!;
    private SubscriptionsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _service = Substitute.For<ISubscriptionsService>();
        _logger = Substitute.For<ILogger<SubscriptionsController>>();
        _controller = new SubscriptionsController(_service, _logger);
    }

    [Test]
    public async Task GetAll_ReturnsOk()
    {
        _service.GetSubscriptionsAsync().Returns(new List<Subscription>());
        var result = await _controller.GetAll();
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NotFound()
    {
        _service.GetSubscriptionAsync(Arg.Any<Guid>()).Returns((Subscription?)null);
        var result = await _controller.GetById(Guid.NewGuid());
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_ReturnsCreated()
    {
        var sub = new Subscription { Id = Guid.NewGuid(), SubscriptionType = SubscriptionType.Klasel, PaidAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddDays(30) };
        _service.CreateSubscriptionAsync(Arg.Any<Subscription>()).Returns(sub);
        var result = await _controller.Create(sub);
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Delete_NoContent_WhenExists()
    {
        _service.DeleteSubscriptionAsync(Arg.Any<Guid>()).Returns(true);
        var result = await _controller.Delete(Guid.NewGuid());
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}


