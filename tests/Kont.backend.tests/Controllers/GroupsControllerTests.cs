using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Services;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class GroupsControllerTests
{
    private IGroupsService _mockGroupsService;
    private ILogger<GroupsController> _mockLogger;
    private GroupsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockGroupsService = Substitute.For<IGroupsService>();
        _mockLogger = Substitute.For<ILogger<GroupsController>>();
        _controller = new GroupsController(_mockGroupsService, _mockLogger);
    }

    [Test]
    public async Task UpdatePlayerGroup_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var request = new UpdatePlayerGroupRequest { GroupId = groupId.ToString() };

        _mockGroupsService.UpdatePlayerGroupAsync(poolId, playerId, groupId.ToString()).Returns(true);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task UpdatePlayerGroup_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new UpdatePlayerGroupRequest { GroupId = Guid.NewGuid().ToString() };

        _mockGroupsService.UpdatePlayerGroupAsync(poolId, playerId, request.GroupId).Returns(false);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetPoolGroups_WithValidPool_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var gameSession = new GameSession { Id = Guid.NewGuid(), Pool = pool };
        var groups = new List<PlayerGroup>
        {
            new PlayerGroup { Id = Guid.NewGuid(), GroupNumber = 1, GameSession = gameSession },
            new PlayerGroup { Id = Guid.NewGuid(), GroupNumber = 2, GameSession = gameSession }
        };

        _mockGroupsService.GetPoolGroupsAsync(poolId).Returns(groups);

        // Act
        var result = await _controller.GetPoolGroups(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<IEnumerable<PlayerGroup>>());
        var returnedGroups = (IEnumerable<PlayerGroup>)objectResult.Value;
        Assert.That(returnedGroups.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task CreatePlayerGroup_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var gameSessionId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var gameSession = new GameSession { Id = gameSessionId, Pool = pool };

        var request = new CreatePlayerGroupRequest 
        { 
            GameSessionId = gameSessionId.ToString(), 
            GroupNumber = 1 
        };

        var createdGroup = new PlayerGroup
        {
            Id = Guid.NewGuid(),
            GroupNumber = 1,
            GameSession = gameSession,
            CreatedAt = DateTime.UtcNow,
            Players = new List<PlayerRegistration>()
        };

        _mockGroupsService.CreatePlayerGroupAsync(poolId, gameSessionId.ToString(), 1).Returns(createdGroup);

        // Act
        var result = await _controller.CreatePlayerGroup(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result;
        Assert.That(createdResult.Value, Is.InstanceOf<PlayerGroup>());
        var returnedGroup = (PlayerGroup)createdResult.Value;
        Assert.That(returnedGroup.GroupNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task DeletePlayerGroup_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        _mockGroupsService.DeletePlayerGroupAsync(poolId, groupId).Returns(true);

        // Act
        var result = await _controller.DeletePlayerGroup(poolId, groupId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

}
