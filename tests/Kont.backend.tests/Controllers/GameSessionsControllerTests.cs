using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Services;

namespace Kont.backend.tests.Controllers;

public class GameSessionsControllerTests
{
    private IGameSessionsService _service = null!;
    private ILogger<GameSessionsController> _logger = null!;
    private GameSessionsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _service = Substitute.For<IGameSessionsService>();
        _logger = Substitute.For<ILogger<GameSessionsController>>();
        _controller = new GameSessionsController(_service);
    }

    [Test]
    public async Task GetByEvent_ReturnsOk()
    {
        _service.GetByEventAsync(Arg.Any<Guid>()).Returns(new List<GameSession>());
        var result = await _controller.GetByEvent(Guid.NewGuid());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Create_ReturnsCreatedOrNotFound()
    {
        var eventId = Guid.NewGuid();
        var created = new GameSession { Id = Guid.NewGuid(), Status = GameSessionStatus.Pending, Pool = new Pool(), Activity = new Activity() };
        _service.CreateAsync(eventId, Arg.Any<Guid>()).Returns(created);
        var result = await _controller.Create(eventId, new GameSessionsController.CreateGameSessionRequest(Guid.NewGuid()));
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());

        _service.CreateAsync(eventId, Arg.Any<Guid>()).Returns((GameSession?)null);
        var result2 = await _controller.Create(eventId, new GameSessionsController.CreateGameSessionRequest(Guid.NewGuid()));
        Assert.That(result2, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateStatus_ReturnsOkOrNotFound()
    {
        _service.UpdateStatusAsync(Arg.Any<Guid>(), Arg.Any<GameSessionStatus>()).Returns(new GameSession { Id = Guid.NewGuid(), Status = GameSessionStatus.Active, Pool = new Pool(), Activity = new Activity() });
        var ok = await _controller.UpdateStatus(Guid.NewGuid(), GameSessionStatus.Active);
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _service.UpdateStatusAsync(Arg.Any<Guid>(), Arg.Any<GameSessionStatus>()).Returns((GameSession?)null);
        var nf = await _controller.UpdateStatus(Guid.NewGuid(), GameSessionStatus.Active);
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContentOrNotFound()
    {
        _service.DeleteAsync(Arg.Any<Guid>()).Returns(true);
        var nc = await _controller.Delete(Guid.NewGuid());
        Assert.That(nc, Is.InstanceOf<NoContentResult>());

        _service.DeleteAsync(Arg.Any<Guid>()).Returns(false);
        var nf = await _controller.Delete(Guid.NewGuid());
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GenerateGroupsWithoutScores_ReturnsOkOrNotFound()
    {
        _service.GenerateGroupsWithoutScoresAsync(Arg.Any<Guid>()).Returns(new List<PlayerGroup>());
        var ok = await _controller.GenerateGroupsWithoutScores(Guid.NewGuid());
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _service.GenerateGroupsWithoutScoresAsync(Arg.Any<Guid>()).Returns((IEnumerable<PlayerGroup>?)null);
        var nf = await _controller.GenerateGroupsWithoutScores(Guid.NewGuid());
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GenerateGroupsWithScores_ReturnsOkOrBadRequestOrNotFound()
    {
        _service.GenerateGroupsWithScoresAsync(Arg.Any<Guid>()).Returns(new List<PlayerGroup>());
        var ok = await _controller.GenerateGroupsWithScores(Guid.NewGuid());
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _service.GenerateGroupsWithScoresAsync(Arg.Any<Guid>()).Returns((IEnumerable<PlayerGroup>?)null);
        var nf = await _controller.GenerateGroupsWithScores(Guid.NewGuid());
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());

        _service.GenerateGroupsWithScoresAsync(Arg.Any<Guid>()).Returns<Task<IEnumerable<PlayerGroup>>?>(_ => throw new InvalidOperationException("e"));
        var br = await _controller.GenerateGroupsWithScores(Guid.NewGuid());
        Assert.That(br, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetScores_ReturnsOkOrBadRequestOrNotFound()
    {
        _service.GetSessionScoresAsync(Arg.Any<Guid>()).Returns(new List<Kont.backend.Models.Scoring.PlayerRanking>());
        var ok = await _controller.GetScores(Guid.NewGuid());
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _service.GetSessionScoresAsync(Arg.Any<Guid>()).Returns((IEnumerable<Kont.backend.Models.Scoring.PlayerRanking>?)null);
        var nf = await _controller.GetScores(Guid.NewGuid());
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());

        _service.GetSessionScoresAsync(Arg.Any<Guid>()).Returns<Task<IEnumerable<Kont.backend.Models.Scoring.PlayerRanking>>?>(_ => throw new InvalidOperationException("e"));
        var br = await _controller.GetScores(Guid.NewGuid());
        Assert.That(br, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetGroups_ReturnsOkOrNotFound()
    {
        _service.GetGroupsAsync(Arg.Any<Guid>()).Returns(new List<PlayerGroup> { new PlayerGroup { Id = Guid.NewGuid(), Players = new List<PlayerRegistration> { new PlayerRegistration { Id = Guid.NewGuid(), Player = new Player { Id = Guid.NewGuid(), Username = "u" } } } } });
        var ok = await _controller.GetGroups(Guid.NewGuid());
        Assert.That(ok, Is.InstanceOf<ObjectResult>());

        _service.GetGroupsAsync(Arg.Any<Guid>()).Returns((IEnumerable<PlayerGroup>?)null);
        var nf = await _controller.GetGroups(Guid.NewGuid());
        Assert.That(nf, Is.InstanceOf<NotFoundObjectResult>());
    }
}


