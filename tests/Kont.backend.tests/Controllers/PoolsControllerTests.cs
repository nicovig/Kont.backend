using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Xunit;

namespace Kont.backend.tests.Controllers;

public class PoolsControllerTests : IDisposable
{
    private readonly Mock<IDatabaseContext> _mockContext;
    private readonly Mock<ILogger<PoolsController>> _mockLogger;
    private readonly PoolsController _controller;

    public PoolsControllerTests()
    {
        _mockContext = new Mock<IDatabaseContext>();
        _mockLogger = new Mock<ILogger<PoolsController>>();
        _controller = new PoolsController(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPools_ReturnsOkResult_WithPools()
    {
        // Arrange
        var pools = new List<Pool>
        {
            new Pool { Id = Guid.NewGuid(), Name = "Test Pool 1" },
            new Pool { Id = Guid.NewGuid(), Name = "Test Pool 2" }
        };

        var mockDbSet = CreateMockDbSet(pools);
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetPools();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPools = Assert.IsAssignableFrom<IEnumerable<Pool>>(okResult.Value);
        Assert.Equal(2, returnedPools.Count());
    }

    [Fact]
    public async Task GetPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPool = Assert.IsType<Pool>(okResult.Value);
        Assert.Equal(poolId, returnedPool.Id);
    }

    [Fact]
    public async Task GetPool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreatePool_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var pool = new Pool
        {
            Name = "New Pool",
            Description = "Test Description"
        };

        var mockDbSet = new Mock<DbSet<Pool>>();
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CreatePool(pool);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedPool = Assert.IsType<Pool>(createdResult.Value);
        Assert.Equal(pool.Name, returnedPool.Name);
        Assert.NotEqual(Guid.Empty, returnedPool.Id);
        Assert.NotNull(returnedPool.QrCode);
    }

    [Fact]
    public async Task UpdatePool_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var existingPool = new Pool { Id = poolId, Name = "Original Name" };
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { existingPool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPool = Assert.IsType<Pool>(okResult.Value);
        Assert.Equal("Updated Name", returnedPool.Name);
    }

    [Fact]
    public async Task UpdatePool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeletePool_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.DeletePool(poolId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.DeletePool(poolId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetPoolStats_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool 
        { 
            Id = poolId, 
            Name = "Test Pool",
            PlayerRegistrations = new List<PlayerRegistration>
            {
                new PlayerRegistration { Id = Guid.NewGuid(), CheckedInAt = DateTime.UtcNow },
                new PlayerRegistration { Id = Guid.NewGuid(), CheckedInAt = null }
            },
            GameSessions = new List<GameSession>
            {
                new GameSession { Id = Guid.NewGuid(), Status = GameSessionStatus.Active },
                new GameSession { Id = Guid.NewGuid(), Status = GameSessionStatus.Completed }
            }
        };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetPoolStats(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<PoolStats>(okResult.Value);
        Assert.Equal(2, stats.TotalPlayers);
        Assert.Equal(1, stats.CheckedInPlayers);
        Assert.Equal(1, stats.ActiveSessions);
        Assert.Equal(1, stats.CompletedSessions);
    }

    [Fact]
    public async Task ValidateAllPlayersPresent_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool", IsAllPlayersPresent = false };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.ValidateAllPlayersPresent(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True(pool.IsAllPlayersPresent);
    }

    [Fact]
    public async Task EndPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool", Status = PoolStatus.Active };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Setup(c => c.Pool).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.EndPool(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(PoolStatus.Completed, pool.Status);
        Assert.NotNull(pool.EndedAt);
    }

    private Mock<DbSet<Pool>> CreateMockDbSet(List<Pool> pools)
    {
        var queryable = pools.AsQueryable();
        var mockDbSet = new Mock<DbSet<Pool>>();
        
        mockDbSet.As<IQueryable<Pool>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<Pool>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<Pool>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<Pool>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
