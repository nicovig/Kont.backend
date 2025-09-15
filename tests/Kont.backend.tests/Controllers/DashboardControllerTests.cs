using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Xunit;

namespace Kont.backend.tests.Controllers;

public class DashboardControllerTests : IDisposable
{
    private readonly Mock<IDatabaseContext> _mockContext;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockContext = new Mock<IDatabaseContext>();
        _mockLogger = new Mock<ILogger<DashboardController>>();
        _controller = new DashboardController(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsOkResult_WithStats()
    {
        // Arrange
        var pools = new List<Pool>
        {
            new Pool { Id = Guid.NewGuid(), Status = PoolStatus.Active },
            new Pool { Id = Guid.NewGuid(), Status = PoolStatus.Pending }
        };

        var playerRegistrations = new List<PlayerRegistration>
        {
            new PlayerRegistration { Id = Guid.NewGuid() },
            new PlayerRegistration { Id = Guid.NewGuid() }
        };

        var activities = new List<Activity>
        {
            new Activity { Id = Guid.NewGuid() },
            new Activity { Id = Guid.NewGuid() }
        };

        var mockPoolDbSet = CreateMockDbSet(pools);
        var mockPlayerRegistrationDbSet = CreateMockDbSet(playerRegistrations);
        var mockActivityDbSet = CreateMockDbSet(activities);

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);
        _mockContext.Setup(c => c.Activity).Returns(mockActivityDbSet.Object);

        // Act
        var result = await _controller.GetDashboardStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<DashboardStats>(okResult.Value);
        Assert.Equal(2, stats.TotalPools);
        Assert.Equal(1, stats.ActivePools);
        Assert.Equal(2, stats.TotalPlayers);
        Assert.Equal(2, stats.TotalActivities);
    }

    [Fact]
    public async Task GetRealTimeUpdates_ReturnsOkResult_WithActivity()
    {
        // Arrange
        var pools = new List<Pool>
        {
            new Pool 
            { 
                Id = Guid.NewGuid(), 
                Name = "Test Pool",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        var mockPoolDbSet = CreateMockDbSet(pools);
        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var activity = Assert.IsType<RecentActivity>(okResult.Value);
        Assert.NotNull(activity);
    }

    [Fact]
    public async Task GetRealTimeUpdates_WithNoActivity_ReturnsDefaultActivity()
    {
        // Arrange
        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration>());
        var mockGameSessionDbSet = CreateMockDbSet(new List<GameSession>());

        _mockContext.Setup(c => c.Pool).Returns(mockPoolDbSet.Object);
        _mockContext.Setup(c => c.PlayerRegistration).Returns(mockPlayerRegistrationDbSet.Object);
        _mockContext.Setup(c => c.GameSession).Returns(mockGameSessionDbSet.Object);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var activity = Assert.IsType<RecentActivity>(okResult.Value);
        Assert.Equal("no_activity", activity.Type);
        Assert.Equal("Aucune activité récente", activity.Message);
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
