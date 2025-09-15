using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Xunit;

namespace Kont.backend.tests.Controllers;

public class ReferentsControllerTests : IDisposable
{
    private readonly Mock<IDatabaseContext> _mockContext;
    private readonly Mock<ILogger<ReferentsController>> _mockLogger;
    private readonly ReferentsController _controller;

    public ReferentsControllerTests()
    {
        _mockContext = new Mock<IDatabaseContext>();
        _mockLogger = new Mock<ILogger<ReferentsController>>();
        _controller = new ReferentsController(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
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
        var mockPlayerRegistrationDbSetForAdd = new Mock<DbSet<PlayerRegistration>>();

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.Player).Returns(mockPlayerDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task AssignReferent_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AssignReferent_WithInvalidReferent_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerDbSet = CreateMockDbSet(new List<Player>());

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.Player).Returns(mockPlayerDbSet.Object);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
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

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.RemoveReferent(poolId, referentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
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

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);

        // Act
        var result = await _controller.GetPoolReferents(poolId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReferents = Assert.IsAssignableFrom<IEnumerable<Player>>(okResult.Value);
        Assert.Single(returnedReferents);
    }

    [Fact]
    public async Task GenerateRecoveryQR_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var pool = new Pool { Id = poolId, Name = "Test Pool" };
        var player = new Player { Id = playerId, Username = "TestPlayer" };

        var mockPoolDbSet = CreateMockDbSet(new List<Pool> { pool });
        var mockPlayerDbSet = CreateMockDbSet(new List<Player> { player });

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.Player).Returns(mockPlayerDbSet.Object);

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GenerateRecoveryQR_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
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
