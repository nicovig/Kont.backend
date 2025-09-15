using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class ReferentsControllerTests
{
    private IDatabaseContext _mockContext;
    private ILogger<ReferentsController> _mockLogger;
    private ReferentsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = Substitute.For<IDatabaseContext>();
        _mockLogger = Substitute.For<ILogger<ReferentsController>>();
        _controller = new ReferentsController(_mockContext, _mockLogger);
    }

    [Test]
    public async Task AssignReferent_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var referent = new Player { Id = referentId, Username = "TestReferent", PlayerType = PlayerType.KeyPlayer };

        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerDbSet = CreateMockDbSet(new List<Player> { referent });
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration>());
        var mockPlayerRegistrationDbSetForAdd = Substitute.For<DbSet<PlayerRegistration>>();

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.Player.Returns(mockPlayerDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task AssignReferent_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockPoolDbSet);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task AssignReferent_WithInvalidReferent_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerDbSet = CreateMockDbSet(new List<Player>());

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.Player.Returns(mockPlayerDbSet);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RemoveReferent_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var referent = new Player { Id = referentId, Username = "TestReferent" };
        var referentRegistration = new PlayerRegistration 
        { 
            Id = Guid.NewGuid(), 
            Player = referent, 
            Pool = pool 
        };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration> { referentRegistration });

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.RemoveReferent(poolId, referentId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetPoolReferents_WithValidPool_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var referent = new Player { Id = Guid.NewGuid(), Username = "TestReferent", PlayerType = PlayerType.KeyPlayer };
        var referentRegistration = new PlayerRegistration 
        { 
            Id = Guid.NewGuid(), 
            Player = referent, 
            Pool = pool 
        };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration> { referentRegistration });

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);

        // Act
        var result = await _controller.GetPoolReferents(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.AssignableFrom<IEnumerable<Player>>());
        var returnedReferents = (IEnumerable<Player>)okResult.Value;
        Assert.That(returnedReferents.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GenerateRecoveryQR_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var player = new Player { Id = playerId, Username = "TestPlayer" };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerDbSet = CreateMockDbSet(new List<Player> { player });

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.Player.Returns(mockPlayerDbSet);

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task GenerateRecoveryQR_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Pool.Returns(mockPoolDbSet);

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
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
