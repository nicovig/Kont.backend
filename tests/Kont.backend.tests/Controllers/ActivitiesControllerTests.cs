using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Services;
using Kont.backend.Models.Request;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class ActivitiesControllerTests
{
    private IActivitiesService _mockActivitiesService;
    private IUserContextService _mockUserContextService;
    private ILogger<ActivitiesController> _mockLogger;
    private ActivitiesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockActivitiesService = Substitute.For<IActivitiesService>();
        _mockUserContextService = Substitute.For<IUserContextService>();
        _mockLogger = Substitute.For<ILogger<ActivitiesController>>();
        _controller = new ActivitiesController(_mockActivitiesService, _mockUserContextService, _mockLogger);
    }

    [Test]
    public async Task GetActivities_ReturnsOkResult_WithActivities()
    {
        var currentUser = new Administrator { Id = Guid.NewGuid(), Email = "test@example.com" };
        var activities = new List<Activity>
        {
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 1" },
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 2" }
        };

        _mockUserContextService.GetCurrentUser().Returns(currentUser);
        _mockActivitiesService.GetActivitiesAsync().Returns(activities);

        var result = await _controller.GetActivities();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetActivities_ReturnsUnauthorized_WhenUserNotAuthenticated()
    {
        _mockUserContextService.GetCurrentUser().Returns((Administrator?)null);

        var result = await _controller.GetActivities();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetActivity_WithValidId_ReturnsOkResult()
    {
        var activityId = Guid.NewGuid();
        var activity = new Activity { Id = activityId, Name = "Test Activity" };
        _mockActivitiesService.GetActivityByIdAsync(activityId).Returns(activity);

        var result = await _controller.GetActivity(activityId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)objectResult.Value;
        Assert.That(returnedActivity.Id, Is.EqualTo(activityId));
    }

    [Test]
    public async Task GetActivity_WithInvalidId_ReturnsNotFound()
    {
        var activityId = Guid.NewGuid();
        _mockActivitiesService.GetActivityByIdAsync(activityId).Returns((Activity?)null);

        var result = await _controller.GetActivity(activityId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateActivity_WithValidData_ReturnsCreatedResult()
    {
        var currentUser = new Administrator { Id = Guid.NewGuid(), Email = "test@example.com" };
        var site = new Site { Id = Guid.NewGuid(), Name = "Kart Arena" };
        var request = new CreateActivityRequest
        {
            Name = "Karting",
            Description = "Grand Prix",
            Site = site,
            ScoringMetrics = new List<CreateScoringMetricRequest>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 0.4 },
                new() { Name = "Clues", Unit = null, HigherIsBetter = false, Coefficient = 0.6 }
            }
        };

        var createdActivity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            PlayersPerGroupLimit = 4,
            Site = site,
            ScoringMetrics = new List<ScoringMetric>
            {
                new() { Id = Guid.NewGuid(), Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 1.0 },
                new() { Id = Guid.NewGuid(), Name = "Clues", Unit = null, HigherIsBetter = false, Coefficient = 0.5 }
            },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        _mockUserContextService.GetCurrentUser().Returns(currentUser);
        _mockActivitiesService.CreateActivityAsync(Arg.Any<Activity>()).Returns(createdActivity);

        var result = await _controller.CreateActivity(request);

        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result;
        Assert.That(createdResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)createdResult.Value;
        Assert.That(returnedActivity.Name, Is.EqualTo(request.Name));
        Assert.That(returnedActivity.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task CreateActivity_ReturnsUnauthorized_WhenUserNotAuthenticated()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "Kart Arena" };
        var request = new CreateActivityRequest
        {
            Name = "Karting",
            Description = "Grand Prix",
            Site = site,
            ScoringMetrics = new List<CreateScoringMetricRequest>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 1.0 }
            }
        };

        _mockUserContextService.GetCurrentUser().Returns((Administrator?)null);

        var result = await _controller.CreateActivity(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task UpdateActivity_WithValidData_ReturnsOkResult()
    {
        var activityId = Guid.NewGuid();
        var site = new Site { Id = Guid.NewGuid(), Name = "Kart Arena" };
        var updateRequest = new UpdateActivityRequest 
        { 
            Id = activityId, 
            Name = "Updated Name",
            Description = "Updated Desc",
            Site = site,
            ScoringMetrics = new List<CreateScoringMetricRequest>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 1.0 }
            }
        };
        var existingActivity = new Activity { Id = activityId, Name = "Updated Name" };

        _mockActivitiesService.UpdateActivityAsync(activityId, Arg.Any<UpdateActivityRequest>()).Returns(existingActivity);

        var result = await _controller.UpdateActivity(activityId, updateRequest);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)objectResult.Value;
        Assert.That(returnedActivity.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateActivity_WithInvalidId_ReturnsNotFound()
    {
        var activityId = Guid.NewGuid();
        var site = new Site { Id = Guid.NewGuid(), Name = "Kart Arena" };
        var updateRequest = new UpdateActivityRequest 
        { 
            Id = activityId, 
            Name = "Updated Name",
            Description = "Updated Desc",
            Site = site,
            ScoringMetrics = new List<CreateScoringMetricRequest>
            {
                new() { Name = "Time", Unit = "s", HigherIsBetter = false, Coefficient = 1.0 }
            }
        };

        _mockActivitiesService.UpdateActivityAsync(activityId, Arg.Any<UpdateActivityRequest>()).Returns((Activity?)null);

        var result = await _controller.UpdateActivity(activityId, updateRequest);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteActivity_WithValidId_ReturnsNoContent()
    {
        var activityId = Guid.NewGuid();
        _mockActivitiesService.DeleteActivityAsync(activityId).Returns(true);

        var result = await _controller.DeleteActivity(activityId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteActivity_WithInvalidId_ReturnsNotFound()
    {
        var activityId = Guid.NewGuid();
        _mockActivitiesService.DeleteActivityAsync(activityId).Returns(false);

        var result = await _controller.DeleteActivity(activityId);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }
}
