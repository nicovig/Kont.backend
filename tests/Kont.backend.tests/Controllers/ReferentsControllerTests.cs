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
public class ReferentsControllerTests
{
    private IReferentsService _mockReferentsService;
    private ILogger<ReferentsController> _mockLogger;
    private ReferentsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockReferentsService = Substitute.For<IReferentsService>();
        _mockLogger = Substitute.For<ILogger<ReferentsController>>();
        _controller = new ReferentsController(_mockReferentsService, _mockLogger);
    }

    [Test]
    public async Task AssignReferent_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        _mockReferentsService.AssignReferentAsync(poolId, referentId.ToString()).Returns(true);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task AssignReferent_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        _mockReferentsService.AssignReferentAsync(poolId, referentId.ToString()).Returns(false);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task AssignReferent_WithInvalidReferent_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();
        var request = new AssignReferentRequest { ReferentId = referentId.ToString() };

        _mockReferentsService.AssignReferentAsync(poolId, referentId.ToString()).Returns(false);

        // Act
        var result = await _controller.AssignReferent(poolId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task RemoveReferent_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referentId = Guid.NewGuid();

        _mockReferentsService.RemoveReferentAsync(poolId, referentId).Returns(true);

        // Act
        var result = await _controller.RemoveReferent(poolId, referentId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetPoolReferents_WithValidPool_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var referent = new Player { Id = Guid.NewGuid(), Username = "TestReferent", PlayerType = PlayerType.KeyPlayer };
        var referents = new List<Player> { referent };

        _mockReferentsService.GetPoolReferentsAsync(poolId).Returns(referents);

        // Act
        var result = await _controller.GetPoolReferents(poolId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<IEnumerable<Player>>());
        var returnedReferents = (IEnumerable<Player>)objectResult.Value;
        Assert.That(returnedReferents.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GenerateRecoveryQR_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var qrCode = "RECOVERY_12345678_87654321";

        _mockReferentsService.GenerateRecoveryQRAsync(poolId, playerId).Returns(qrCode);

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task GenerateRecoveryQR_WithInvalidPool_ReturnsNotFound()
    {
        // Arrange
        var poolId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        _mockReferentsService.GenerateRecoveryQRAsync(poolId, playerId).Returns(Task.FromException<string>(new ArgumentException("Pool not found")));

        // Act
        var result = await _controller.GenerateRecoveryQR(poolId, playerId);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(404));
    }

}
