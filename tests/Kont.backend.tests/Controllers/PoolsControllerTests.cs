using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class PoolsControllerTests
{
    private IDatabaseContext _mockContext;
    private ILogger<PoolsController> _mockLogger;
    private PoolsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = Substitute.For<IDatabaseContext>();
        _mockLogger = Substitute.For<ILogger<PoolsController>>();
        _controller = new PoolsController(_mockContext, _mockLogger);
    }

    [Test]
    public async Task GetPools_ReturnsOkResult_WithPools()
    {
        // Arrange
        var pools = new List<Pool>
        {
            new Pool { Id = Guid.NewGuid(), Name = "Test Pool 1" },
            new Pool { Id = Guid.NewGuid(), Name = "Test Pool 2" }
        };

        var mockDbSet = CreateMockDbSet(pools);
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.GetPools();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.AssignableFrom<IEnumerable<Pool>>());
        var returnedPools = (IEnumerable<Pool>)okResult.Value;
        Assert.That(returnedPools.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<Pool>());
        var returnedPool = (Pool)okResult.Value;
        Assert.That(returnedPool.Id, Is.EqualTo(poolId));
    }

    [Test]
    public async Task GetPool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreatePool_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var pool = new Pool
        {
            Name = "New Pool",
            Description = "Test Description"
        };

        var mockDbSet = Substitute.For<DbSet<Pool>>();
        _mockContext.Pool.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.CreatePool(pool);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result;
        Assert.That(createdResult.Value, Is.InstanceOf<Pool>());
        var returnedPool = (Pool)createdResult.Value;
        Assert.That(returnedPool.Name, Is.EqualTo(pool.Name));
        Assert.That(returnedPool.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(returnedPool.QrCode, Is.Not.Null);
    }

    [Test]
    public async Task UpdatePool_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var existingPool = new Pool { Id = poolId, Name = "Original Name" };
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { existingPool });
        _mockContext.Pool.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<Pool>());
        var returnedPool = (Pool)okResult.Value;
        Assert.That(returnedPool.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdatePool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeletePool_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Pool.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.DeletePool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeletePool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.DeletePool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
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
        _mockContext.Pool.Returns(mockDbSet);

        // Act
        var result = await _controller.GetPoolStats(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<PoolStats>());
        var stats = (PoolStats)okResult.Value;
        Assert.That(stats.TotalPlayers, Is.EqualTo(2));
        Assert.That(stats.CheckedInPlayers, Is.EqualTo(1));
        Assert.That(stats.ActiveSessions, Is.EqualTo(1));
        Assert.That(stats.CompletedSessions, Is.EqualTo(1));
    }

    [Test]
    public async Task ValidateAllPlayersPresent_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool", IsAllPlayersPresent = false };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Pool.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.ValidateAllPlayersPresent(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(pool.IsAllPlayersPresent, Is.True);
    }

    [Test]
    public async Task EndPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool", Status = PoolStatus.Active };

        var mockDbSet = CreateMockDbSet(new List<Pool> { pool });
        _mockContext.Pool.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.EndPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(pool.Status, Is.EqualTo(PoolStatus.Completed));
        Assert.That(pool.EndedAt, Is.Not.Null);
    }

    private DbSet<Pool> CreateMockDbSet(List<Pool> pools)
    {
        var queryable = pools.AsQueryable();
        var mockDbSet = Substitute.For<DbSet<Pool>, IQueryable<Pool>>();
        
        ((IQueryable<Pool>)mockDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<Pool>)mockDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<Pool>)mockDbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<Pool>)mockDbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }
}
