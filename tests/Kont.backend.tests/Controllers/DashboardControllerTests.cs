using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Dashboard;
using Kont.backend.Services;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class DashboardControllerTests
{
    private IDashboardService _mockDashboardService;
    private ILogger<DashboardController> _mockLogger;
    private DashboardController _controller;

    [SetUp]
    public void Setup()
    {
        _mockDashboardService = Substitute.For<IDashboardService>();
        _mockLogger = Substitute.For<ILogger<DashboardController>>();
        _controller = new DashboardController(_mockDashboardService, _mockLogger);
    }

    [Test]
    public async Task GetDashboardStats_ReturnsOkResult_WithStats()
    {
        // Arrange
        var expectedStats = new DashboardStats
        {
            TotalPools = 2,
            ActivePools = 1,
            TotalPlayers = 2,
            TotalActivities = 2,
            RecentActivity = new List<RecentActivity>()
        };

        _mockDashboardService.GetDashboardStatsAsync().Returns(expectedStats);

        // Act
        var result = await _controller.GetDashboardStats();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<DashboardStats>());
        var stats = (DashboardStats)objectResult.Value;
        Assert.That(stats.TotalPools, Is.EqualTo(2));
        Assert.That(stats.ActivePools, Is.EqualTo(1));
        Assert.That(stats.TotalPlayers, Is.EqualTo(2));
        Assert.That(stats.TotalActivities, Is.EqualTo(2));
    }

    [Test]
    public async Task GetRealTimeUpdates_ReturnsOkResult_WithActivity()
    {
        // Arrange
        var expectedActivity = new RecentActivity
        {
            Id = Guid.NewGuid().ToString(),
            Type = "pool_created",
            Message = "Nouvelle pool créée: Test Pool",
            Timestamp = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockDashboardService.GetRealTimeUpdatesAsync().Returns(expectedActivity);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<RecentActivity>());
        var activity = (RecentActivity)objectResult.Value;
        Assert.That(activity, Is.Not.Null);
    }

    [Test]
    public async Task GetRealTimeUpdates_WithNoActivity_ReturnsDefaultActivity()
    {
        // Arrange
        var expectedActivity = new RecentActivity
        {
            Id = Guid.NewGuid().ToString(),
            Type = "no_activity",
            Message = "Aucune activité récente",
            Timestamp = DateTime.UtcNow
        };

        _mockDashboardService.GetRealTimeUpdatesAsync().Returns(expectedActivity);

        // Act
        var result = await _controller.GetRealTimeUpdates();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<RecentActivity>());
        var activity = (RecentActivity)objectResult.Value;
        Assert.That(activity.Type, Is.EqualTo("no_activity"));
        Assert.That(activity.Message, Is.EqualTo("Aucune activité récente"));
    }

}
