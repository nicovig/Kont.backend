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
public class PoolsControllerTests
{
    private IPoolsService _mockPoolsService;
    private ILogger<PoolsController> _mockLogger;
    private PoolsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockPoolsService = Substitute.For<IPoolsService>();
        _mockLogger = Substitute.For<ILogger<PoolsController>>();
        _controller = new PoolsController(_mockPoolsService, _mockLogger);
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

        _mockPoolsService.GetPoolsAsync().Returns(pools);

        // Act
        var result = await _controller.GetPools();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<IEnumerable<Pool>>());
        var returnedPools = (IEnumerable<Pool>)objectResult.Value;
        Assert.That(returnedPools.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };

        _mockPoolsService.GetPoolByIdAsync(poolId).Returns(pool);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<Pool>());
        var returnedPool = (Pool)objectResult.Value;
        Assert.That(returnedPool.Id, Is.EqualTo(poolId));
    }

    [Test]
    public async Task GetPool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        _mockPoolsService.GetPoolByIdAsync(poolId).Returns((Pool?)null);

        // Act
        var result = await _controller.GetPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
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

        var createdPool = new Pool
        {
            Id = Guid.NewGuid(),
            Name = "New Pool",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            QrCode = "POOL_12345678"
        };

        _mockPoolsService.CreatePoolAsync(pool).Returns(createdPool);

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
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };
        var existingPool = new Pool { Id = poolId, Name = "Updated Name" };

        _mockPoolsService.UpdatePoolAsync(poolId, updatedPool).Returns(existingPool);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<Pool>());
        var returnedPool = (Pool)objectResult.Value;
        Assert.That(returnedPool.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdatePool_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var updatedPool = new Pool { Id = poolId, Name = "Updated Name" };

        _mockPoolsService.UpdatePoolAsync(poolId, updatedPool).Returns((Pool?)null);

        // Act
        var result = await _controller.UpdatePool(poolId, updatedPool);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeletePool_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        _mockPoolsService.DeletePoolAsync(poolId).Returns(true);

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
        _mockPoolsService.DeletePoolAsync(poolId).Returns(false);

        // Act
        var result = await _controller.DeletePool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetPoolStats_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var stats = new PoolStats
        {
            TotalPlayers = 2,
            RegisteredPlayers = 2,
            CheckedInPlayers = 1,
            ActiveSessions = 1,
            CompletedSessions = 1,
            TotalActivities = 1
        };

        _mockPoolsService.GetPoolStatsAsync(poolId).Returns(stats);

        // Act
        var result = await _controller.GetPoolStats(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<PoolStats>());
        var returnedStats = (PoolStats)objectResult.Value;
        Assert.That(returnedStats.TotalPlayers, Is.EqualTo(2));
        Assert.That(returnedStats.CheckedInPlayers, Is.EqualTo(1));
        Assert.That(returnedStats.ActiveSessions, Is.EqualTo(1));
        Assert.That(returnedStats.CompletedSessions, Is.EqualTo(1));
    }

    [Test]
    public async Task ValidateAllPlayersPresent_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        _mockPoolsService.ValidateAllPlayersPresentAsync(poolId).Returns(true);

        // Act
        var result = await _controller.ValidateAllPlayersPresent(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task EndPool_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        _mockPoolsService.EndPoolAsync(poolId).Returns(true);

        // Act
        var result = await _controller.EndPool(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
    }

}
