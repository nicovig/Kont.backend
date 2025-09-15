using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class GroupsControllerTests
{
    private IDatabaseContext _mockContext;
    private ILogger<GroupsController> _mockLogger;
    private GroupsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = Substitute.For<IDatabaseContext>();
        _mockLogger = Substitute.For<ILogger<GroupsController>>();
        _controller = new GroupsController(_mockContext, _mockLogger);
    }

    [Test]
    public async Task UpdatePlayerGroup_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var player = new Player { Id = playerId, Username = "TestPlayer" };
        var playerRegistration = new PlayerRegistration 
        { 
            Id = Guid.NewGuid(), 
            Player = player, 
            Pool = pool 
        };
        var newGroup = new PlayerGroup 
        { 
            Id = groupId, 
            GroupNumber = 1,
            GameSession = new GameSession { Pool = pool }
        };

        var request = new UpdatePlayerGroupRequest { GroupId = groupId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration> { playerRegistration });
        var mockPlayerGroupDbSet = CreateMockDbSet(new List<PlayerGroup> { newGroup });

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);
        _mockContext.PlayerGroup.Returns(mockPlayerGroupDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task UpdatePlayerGroup_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new UpdatePlayerGroupRequest { GroupId = Guid.NewGuid().ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockPoolDbSet);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
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

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerGroupDbSet = CreateMockDbSet(groups);

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerGroup.Returns(mockPlayerGroupDbSet);

        // Act
        var result = await _controller.GetPoolGroups(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.AssignableFrom<IEnumerable<PlayerGroup>>());
        var returnedGroups = (IEnumerable<PlayerGroup>)okResult.Value;
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

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockGameSessionDbSet = CreateMockDbSet(new List<GameSession> { gameSession });
        var mockPlayerGroupDbSet = Substitute.For<DbSet<PlayerGroup>>();

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.GameSession.Returns(mockGameSessionDbSet);
        _mockContext.PlayerGroup.Returns(mockPlayerGroupDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

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
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var gameSession = new GameSession { Id = Guid.NewGuid(), Pool = pool };
        var group = new PlayerGroup { Id = groupId, GameSession = gameSession };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerGroupDbSet = CreateMockDbSet(new List<PlayerGroup> { group });

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerGroup.Returns(mockPlayerGroupDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.DeletePlayerGroup(poolId, groupId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    private DbSet<T> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockDbSet = Substitute.For<DbSet<T>, IQueryable<T>>();
        
        ((IQueryable<T>)mockDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<T>)mockDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<T>)mockDbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<T>)mockDbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }
}
