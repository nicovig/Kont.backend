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
public class ActivitiesControllerTests
{
    private IActivitiesService _mockActivitiesService;
    private ILogger<ActivitiesController> _mockLogger;
    private ActivitiesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockActivitiesService = Substitute.For<IActivitiesService>();
        _mockLogger = Substitute.For<ILogger<ActivitiesController>>();
        _controller = new ActivitiesController(_mockActivitiesService, _mockLogger);
    }

    [Test]
    public async Task GetActivities_ReturnsOkResult_WithActivities()
    {
        // Arrange
        var activities = new List<Activity>
        {
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 1" },
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 2" }
        };

        _mockActivitiesService.GetActivitiesAsync().Returns(activities);

        // Act
        var result = await _controller.GetActivities();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetActivity_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var activity = new Activity { Id = activityId, Name = "Test Activity" };

        _mockActivitiesService.GetActivityByIdAsync(activityId).Returns(activity);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
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
        // Arrange
        var activityId = Guid.NewGuid();
        _mockActivitiesService.GetActivityByIdAsync(activityId).Returns((Activity?)null);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateActivity_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var activity = new Activity
        {
            Name = "New Activity",
            Description = "Test Description"
        };

        var createdActivity = new Activity
        {
            Id = Guid.NewGuid(),
            Name = "New Activity",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };

        _mockActivitiesService.CreateActivityAsync(activity).Returns(createdActivity);

        // Act
        var result = await _controller.CreateActivity(activity);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result;
        Assert.That(createdResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)createdResult.Value;
        Assert.That(returnedActivity.Name, Is.EqualTo(activity.Name));
        Assert.That(returnedActivity.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task UpdateActivity_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };
        var existingActivity = new Activity { Id = activityId, Name = "Updated Name" };

        _mockActivitiesService.UpdateActivityAsync(activityId, updatedActivity).Returns(existingActivity);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
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
        // Arrange
        var activityId = Guid.NewGuid();
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };

        _mockActivitiesService.UpdateActivityAsync(activityId, updatedActivity).Returns((Activity?)null);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteActivity_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        _mockActivitiesService.DeleteActivityAsync(activityId).Returns(true);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        _mockActivitiesService.DeleteActivityAsync(activityId).Returns(false);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

}
