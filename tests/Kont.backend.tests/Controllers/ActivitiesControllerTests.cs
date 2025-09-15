using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Xunit;

namespace Kont.backend.tests.Controllers;

public class ActivitiesControllerTests : IDisposable
{
    private readonly Mock<IDatabaseContext> _mockContext;
    private readonly Mock<ILogger<ActivitiesController>> _mockLogger;
    private readonly ActivitiesController _controller;
    private readonly DbContextOptions<DatabaseContext> _options;

    public ActivitiesControllerTests()
    {
        _options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockContext = new Mock<IDatabaseContext>();
        _mockLogger = new Mock<ILogger<ActivitiesController>>();
        _controller = new ActivitiesController(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetActivities_ReturnsOkResult_WithActivities()
    {
        // Arrange
        var activities = new List<Activity>
        {
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 1" },
            new Activity { Id = Guid.NewGuid(), Name = "Test Activity 2" }
        };

        var mockDbSet = CreateMockDbSet(activities);
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetActivities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedActivities = Assert.IsAssignableFrom<IEnumerable<Activity>>(okResult.Value);
        Assert.Equal(2, returnedActivities.Count());
    }

    [Fact]
    public async Task GetActivity_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var activity = new Activity { Id = activityId, Name = "Test Activity" };

        var mockDbSet = CreateMockDbSet(new List<Activity> { activity });
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedActivity = Assert.IsType<Activity>(okResult.Value);
        Assert.Equal(activityId, returnedActivity.Id);
    }

    [Fact]
    public async Task GetActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetActivity(activityId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateActivity_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var activity = new Activity
        {
            Name = "New Activity",
            Description = "Test Description"
        };

        var mockDbSet = new Mock<DbSet<Activity>>();
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.CreateActivity(activity);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedActivity = Assert.IsType<Activity>(createdResult.Value);
        Assert.Equal(activity.Name, returnedActivity.Name);
        Assert.NotEqual(Guid.Empty, returnedActivity.Id);
    }

    [Fact]
    public async Task UpdateActivity_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var existingActivity = new Activity { Id = activityId, Name = "Original Name" };
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Activity> { existingActivity });
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedActivity = Assert.IsType<Activity>(okResult.Value);
        Assert.Equal("Updated Name", returnedActivity.Name);
    }

    [Fact]
    public async Task UpdateActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var updatedActivity = new Activity { Id = activityId, Name = "Updated Name" };

        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.UpdateActivity(activityId, updatedActivity);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteActivity_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var activity = new Activity { Id = activityId, Name = "Test Activity" };

        var mockDbSet = CreateMockDbSet(new List<Activity> { activity });
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteActivity_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Activity>());
        _mockContext.Setup(c => c.Activity).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.DeleteActivity(activityId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private Mock<DbSet<Activity>> CreateMockDbSet(List<Activity> activities)
    {
        var queryable = activities.AsQueryable();
        var mockDbSet = new Mock<DbSet<Activity>>();
        
        mockDbSet.As<IQueryable<Activity>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockDbSet.As<IQueryable<Activity>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<Activity>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<Activity>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockDbSet;
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
