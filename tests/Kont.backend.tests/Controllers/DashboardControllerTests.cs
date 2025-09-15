using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class DashboardControllerTests
{
    private IDatabaseContext _mockContext;
    private ILogger<DashboardController> _mockLogger;
    private DashboardController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = Substitute.For<IDatabaseContext>();
        _mockLogger = Substitute.For<ILogger<DashboardController>>();
        _controller = new DashboardController(_mockContext, _mockLogger);
    }

    [Test]
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

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);
        _mockContext.Activity.Returns(mockActivityDbSet);

        // Act
        var result = await _controller.GetDashboardStats();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<DashboardStats>());
        var stats = (DashboardStats)okResult.Value;
        Assert.That(stats.TotalPools, Is.EqualTo(2));
        Assert.That(stats.ActivePools, Is.EqualTo(1));
        Assert.That(stats.TotalPlayers, Is.EqualTo(2));
        Assert.That(stats.TotalActivities, Is.EqualTo(2));
    }

    [Test]
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
        _mockContext.Pool.Returns(mockPoolDbSet);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<RecentActivity>());
        var activity = (RecentActivity)okResult.Value;
        Assert.That(activity, Is.Not.Null);
    }

    [Test]
    public async Task GetRealTimeUpdates_WithNoActivity_ReturnsDefaultActivity()
    {
        // Arrange
        var mockPoolDbSet = CreateMockDbSet(new List<Pool>());
        var mockPlayerRegistrationDbSet = CreateMockDbSet(new List<PlayerRegistration>());
        var mockGameSessionDbSet = CreateMockDbSet(new List<GameSession>());

        _mockContext.Pool.Returns(mockPoolDbSet);
        _mockContext.PlayerRegistration.Returns(mockPlayerRegistrationDbSet);
        _mockContext.GameSession.Returns(mockGameSessionDbSet);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<RecentActivity>());
        var activity = (RecentActivity)okResult.Value;
        Assert.That(activity.Type, Is.EqualTo("no_activity"));
        Assert.That(activity.Message, Is.EqualTo("Aucune activité récente"));
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
