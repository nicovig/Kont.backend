using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class ActivitiesControllerTests
{
    private IDatabaseContext _mockContext;
    private ILogger<ActivitiesController> _mockLogger;
    private ActivitiesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockContext = Substitute.For<IDatabaseContext>();
        _mockLogger = Substitute.For<ILogger<ActivitiesController>>();
        _controller = new ActivitiesController(_mockContext, _mockLogger);
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

        var mockDbSet = Substitute.For<DbSet<Activity>, IQueryable<Activity>>();
        var queryable = activities.AsQueryable();
        ((IQueryable<Activity>)mockDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<Activity>)mockDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<Activity>)mockDbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<Activity>)mockDbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        
        _mockContext.Activity.Returns(mockDbSet);

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

        var mockDbSet = CreateMockDbSet(new List<Activity> { activity });
        _mockContext.Activity.Returns(mockDbSet);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)okResult.Value;
        Assert.That(returnedActivity.Id, Is.EqualTo(activityId));
    }

    [Test]
    public async Task GetActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Activity.Returns(mockDbSet);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
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

        var mockDbSet = Substitute.For<DbSet<Activity>>();
        _mockContext.Activity.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

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
        var existingActivity = new Activity { Id = activityId, Name = "Original Name" };
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Activity> { existingActivity });
        _mockContext.Activity.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<Activity>());
        var returnedActivity = (Activity)okResult.Value;
        Assert.That(returnedActivity.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Activity.Returns(mockDbSet);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteActivity_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var activity = new Activity { Id = activityId, Name = "Test Activity" };

        var mockDbSet = CreateMockDbSet(new List<Activity> { activity });
        _mockContext.Activity.Returns(mockDbSet);
        _mockContext.SaveChangesAsync().Returns(1);

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
        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Activity.Returns(mockDbSet);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    private DbSet<Activity> CreateMockDbSet(List<Activity> activities)
    {
        var queryable = activities.AsQueryable();
        var mockDbSet = Substitute.For<DbSet<Activity>, IQueryable<Activity>>();
        
        ((IQueryable<Activity>)mockDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<Activity>)mockDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<Activity>)mockDbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<Activity>)mockDbSet).GetEnumerator().Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }
}
