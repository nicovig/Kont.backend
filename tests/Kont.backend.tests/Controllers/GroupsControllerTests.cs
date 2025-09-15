using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Xunit;

namespace Kont.backend.tests.Controllers;

public class GroupsControllerTests : IDisposable
{
    private readonly Mock<IDatabaseContext> _mockContext;
    private readonly Mock<ILogger<GroupsController>> _mockLogger;
    private readonly GroupsController _controller;

    public GroupsControllerTests()
    {
        _mockContext = new Mock<IDatabaseContext>();
        _mockLogger = new Mock<ILogger<GroupsController>>();
        _controller = new GroupsController(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
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

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);
        _mockContext.Setup(c => c.PlayerGroup).Returns(mockPlayerGroupDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdatePlayerGroup_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var request = new UpdatePlayerGroupRequest { GroupId = Guid.NewGuid().ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);

        // Act
        var result = await _controller.UpdatePlayerGroup(poolId, playerId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
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

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerGroup).Returns(mockPlayerGroupDbSet.Object);

        // Act
        var result = await _controller.GetPoolGroups(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGroups = Assert.IsAssignableFrom<IEnumerable<PlayerGroup>>(okResult.Value);
        Assert.Equal(2, returnedGroups.Count());
    }

    [Fact]
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
        var mockPlayerGroupDbSet = new Mock<DbSet<PlayerGroup>>();

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.GameSession).Returns(mockGameSessionDbSet.Object);
        _mockContext.Setup(c => c.PlayerGroup).Returns(mockPlayerGroupDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CreatePlayerGroup(poolId, request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedGroup = Assert.IsType<PlayerGroup>(createdResult.Value);
        Assert.Equal(1, returnedGroup.GroupNumber);
    }

    [Fact]
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

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerGroup).Returns(mockPlayerGroupDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.DeletePlayerGroup(poolId, groupId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockDbSet = new Mock<DbSet<T>>();
        
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
